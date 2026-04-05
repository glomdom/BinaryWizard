/*
 * Copyright 2026 glomdom
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using BinaryWizard.Analysis;
using BinaryWizard.Diagnostics;
using BinaryWizard.Models;
using BinaryWizard.Segmenting;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace BinaryWizard;

[Generator]
public class Generator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceNames = assembly.GetManifestResourceNames();

        context.RegisterPostInitializationOutput(ctx => {
            foreach (var resourceName in resourceNames) {
                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is null) continue;

                using var reader = new StreamReader(stream);
                var source = reader.ReadToEnd();

                var hintParts = Path.GetFileNameWithoutExtension(resourceName).Split('.');
                var hintName = hintParts[hintParts.Length - 1];

                ctx.AddSource(hintName, SourceText.From(source, Encoding.UTF8));
            }
        });

        const string fullyQualifiedAttributeName = $"{Constants.Codegen.Namespace}.{Constants.Attributes.BinarySerializable}";

        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedAttributeName,
                predicate: static (s, _) => s is ClassDeclarationSyntax or StructDeclarationSyntax,
                transform: GetSerializationMeta)
            .WithTrackingName("BinarySerializableGeneration");

        context.RegisterSourceOutput(
            provider,
            GenerateCode
        );
    }

    private static ClassSerializationMeta? GetSerializationMeta(GeneratorAttributeSyntaxContext ctx, CancellationToken ct) {
        if (ctx.TargetSymbol is not INamedTypeSymbol namedSymbol) return null;

        var diagnostics = new List<Diagnostic>();
        var namespaceName = namedSymbol.ContainingNamespace.IsGlobalNamespace
            ? string.Empty
            : namedSymbol.ContainingNamespace.ToDisplayString();

        var kind = namedSymbol.TypeKind == TypeKind.Class ? "class" : "struct";

        var segmentManager = new SegmentManager();
        var fields = namedSymbol.GetMembers().OfType<IFieldSymbol>();

        foreach (var field in fields) {
            var fieldType = field.Type;

            if (fieldType.IsPrimitiveLike()) {
                string? magic = null;
                var magicAttributeData = field.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "MagicAttribute");
                if (magicAttributeData is not null && magicAttributeData.TryGetNamedArg("Magic", out var magicVal)) {
                    magic = magicVal.ToString();
                }

                var fieldDef = new FieldDef(field.Name, fieldType, magic) {
                    ByteSize = fieldType.GetByteSize(),
                };

                segmentManager.AddField(fieldDef);
                DebugUtilities.CreatedFieldDef(fieldDef);
            } else if (fieldType.IsArrayLike(out var arrSymbol)) {
                if (arrSymbol.Rank != 1) throw new NotSupportedException("Arrays which have more than 1 dimension are not supported.");

                var binaryArrayAttr = field.GetAttributes().FirstOrDefault(a => a.AttributeClass?.Name == "BinaryArrayAttribute");
                if (binaryArrayAttr is null) {
                    var diag = DiagnosticReporter.CreateArrayIsMissingAttribute(field);
                    diagnostics.Add(diag);

                    continue;
                }

                if (!binaryArrayAttr.AreAnyNamedArgsProvided("Size", "SizeMember")) {
                    var diag = DiagnosticReporter.CreateArrayIsMissingSizeArgument(field);
                    diagnostics.Add(diag);

                    continue;
                }

                if (binaryArrayAttr.AreAllNamedArgsProvided("Size", "SizeMember")) {
                    var diag = DiagnosticReporter.CreateArrayHasConflictingSizeArguments(field);
                    diagnostics.Add(diag);

                    continue;
                }

                var fieldDef = new FieldDef(field.Name, arrSymbol.ElementType) {
                    TypeModel = {
                        InnerType = arrSymbol.ElementType,
                        InnerTypeByteSize = arrSymbol.ElementType.GetByteSize(),
                    },
                };

                if (binaryArrayAttr.TryGetNamedArg("Size", out var arrSize)) {
                    var arrSizeValue = (int)arrSize.Value!;
                    fieldDef.ByteSize = arrSizeValue * arrSymbol.ElementType.GetByteSize();
                    fieldDef.TypeModel.FixedArraySize = arrSizeValue;

                    DebugUtilities.CreatedFieldDef(fieldDef);
                    segmentManager.AddField(fieldDef);
                } else if (binaryArrayAttr.TryGetNamedArg("SizeMember", out var sizeMember)) {
                    var arrSizeRef = (string)sizeMember.Value!;
                    fieldDef.ByteSize = -1;

                    DebugUtilities.CreatedFieldDef(fieldDef);
                    segmentManager.AddField(fieldDef, arrSizeRef);
                }
            } else if (fieldType.HasBinarySerializableAttribute()) {
                segmentManager.AddNestedObject(field.Name, fieldType.Name);
            } else {
                var diag = DiagnosticReporter.CreateUnmarkedSerializableForField(field);
                diagnostics.Add(diag);
            }
        }

        return new ClassSerializationMeta(
            ns: namespaceName,
            className: namedSymbol.Name,
            kind: kind,
            segments: segmentManager.Commit(),
            diagnostics: diagnostics
        );
    }

    private void GenerateCode(SourceProductionContext spc, ClassSerializationMeta? meta) {
        if (meta is null) return;

        foreach (var diag in meta.Diagnostics) {
            spc.ReportDiagnostic(diag);
        }

        if (meta.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) {
            return;
        }

        Debug.WriteLine($"Starting code generation for {meta.ClassName}");

        var bodyBuilder = new StringBuilder();
        var hasNamespace = !string.IsNullOrEmpty(meta.Namespace);
        var baseIndent = hasNamespace ? "    " : "";
        var bodyIndent = baseIndent + "        ";

        var segmentCount = 0;

        foreach (var seg in meta.Segments) {
            switch (seg) {
                case FixedSegment fixedSeg: ProcessFixedSegment(++segmentCount, fixedSeg, bodyBuilder, bodyIndent); break;
                case DynamicSegment dynSeg: ProcessDynamicSegment(dynSeg, bodyBuilder, bodyIndent); break;
                case NestedObjectSegment nestedSeg: bodyBuilder.AppendLine($"{bodyIndent}result.{nestedSeg.FieldName} = {nestedSeg.TypeName}.FromBinary(reader);"); break;
            }
        }

        var code = $$"""
                     // <autogenerated/>
                     using System;
                     using System.IO;
                     using System.Buffers.Binary;

                     {{(hasNamespace ? $"namespace {meta.Namespace}\n{{" : "")}}
                     {{baseIndent}}partial {{meta.Kind}} {{meta.ClassName}} {
                     {{baseIndent}}    public static {{meta.ClassName}} FromBinary(System.IO.BinaryReader reader) {
                     {{baseIndent}}        var result = new {{meta.ClassName}}();

                     {{bodyBuilder}}

                     {{baseIndent}}        return result;
                     {{baseIndent}}    }
                     {{baseIndent}}}
                     {{(hasNamespace ? "}" : "")}}
                     """;

        spc.AddSource($"BinarySerializable_{meta.ClassName}.g.cs", SourceText.From(code, Encoding.UTF8));
    }

    private void ProcessDynamicSegment(DynamicSegment seg, StringBuilder sb, string indent) {
        foreach (var field in seg.Fields) {
            var elementBytes = field.TypeModel.InnerType!.GetByteSize();
            var bufferName = $"__{field.Name}_buf";

            sb.AppendLine($$"""
                            {{indent}}result.{{field.Name}} = new {{field.TypeModel.Type}}[result.{{seg.LengthReferenceFieldName}}];
                            {{indent}}Span<byte> {{bufferName}} = stackalloc byte[{{elementBytes}} * result.{{seg.LengthReferenceFieldName}}];
                            {{indent}}if (reader.Read({{bufferName}}) < {{elementBytes}} * result.{{seg.LengthReferenceFieldName}}) throw new EndOfStreamException();
                            {{indent}}for (var i = 0; i < result.{{seg.LengthReferenceFieldName}}; i++) {
                            {{indent}}    result.{{field.Name}}[i] = {{field.TypeModel.Type.GetBinaryPrimitiveReader()}}({{bufferName}}.Slice(i * {{elementBytes}}, {{elementBytes}}));
                            {{indent}}}
                            """);
        }
    }

    private void ProcessFixedSegment(int segmentIndex, FixedSegment seg, StringBuilder sb, string indent) {
        var bufName = $"__buf_{segmentIndex}";
        var localOffset = 0;

        sb.AppendLine($"""
                       {indent}Span<byte> {bufName} = stackalloc byte[{seg.Bytes}];
                       {indent}if (reader.Read({bufName}) < {seg.Bytes}) throw new EndOfStreamException();
                       """);

        foreach (var field in seg.Fields) {
            if (field.TypeModel.IsFixedArray) {
                var elementBytes = field.TypeModel.InnerType!.GetByteSize();

                sb.AppendLine($$"""
                                {{indent}}result.{{field.Name}} = new {{field.TypeModel.Type}}[{{field.TypeModel.FixedArraySize!.Value}}];
                                {{indent}}for (var i = 0; i < {{field.TypeModel.FixedArraySize}}; i++) {
                                {{indent}}    result.{{field.Name}}[i] = {{field.TypeModel.Type.GetBinaryPrimitiveReader()}}({{bufName}}.Slice({{localOffset}} + ({{elementBytes}} * i), {{elementBytes}}));
                                {{indent}}}
                                """);

                localOffset += elementBytes * field.TypeModel.FixedArraySize!.Value;

                continue;
            }

            sb.AppendLine($"{indent}result.{field.Name} = {field.TypeModel.Type.GetBinaryPrimitiveReader()}({bufName}.Slice({localOffset}, {field.ByteSize}));");

            localOffset += field.ByteSize;
        }
    }
}