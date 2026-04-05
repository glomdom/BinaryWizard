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
using BinaryWizard.Emission;
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
            CodeBuilder.Generate
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
                if (magicAttributeData is not null) {
                    if (magicAttributeData.ConstructorArguments.Length > 0) {
                        magic = magicAttributeData.ConstructorArguments[0].Value?.ToString();
                    } else if (magicAttributeData.TryGetNamedArg("Magic", out var magicVal)) {
                        magic = magicVal.Value?.ToString();
                    }
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
}