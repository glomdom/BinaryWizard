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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryWizard.Diagnostics;

internal static class DiagnosticReporter {
    internal static Diagnostic CreateArrayHasConflictingSizeArguments(IFieldSymbol field) {
        var location = GetVariableDeclaratorNameLocation(field);

        return Diagnostic.Create(
            Diagnostics.ArrayHasConflictingSizeArguments,
            location,
            field.Name
        );
    }

    internal static Diagnostic CreateArrayIsMissingSizeArgument(IFieldSymbol field) {
        var location = GetVariableDeclaratorNameLocation(field);

        return Diagnostic.Create(
            Diagnostics.MarkedArraylikeHasNoSizeOrSizeProviderRule,
            location,
            field.Name
        );
    }

    internal static Diagnostic CreateArrayIsMissingAttribute(IFieldSymbol field) {
        var location = GetVariableDeclaratorNameLocation(field);

        return Diagnostic.Create(
            Diagnostics.ArrayHasNoBinaryArrayAttributeRule,
            location,
            field.Name
        );
    }

    internal static Diagnostic CreateUnmarkedSerializableForField(IFieldSymbol field) {
        var location = GetVariableDeclaratorLocation(field);

        return Diagnostic.Create(
            Diagnostics.MissingBinarySerializableAttributeRule,
            location,
            field.Type.Name
        );
    }


    internal static void ReportArrayHasConflictingSizeArguments(this SourceProductionContext spc, IFieldSymbol field) {
        var location = GetVariableDeclaratorNameLocation(field);

        spc.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.ArrayHasConflictingSizeArguments,
            location,
            field.Name
        ));
    }

    internal static void ReportArrayIsMissingSizeArgument(this SourceProductionContext spc, IFieldSymbol field) {
        var location = GetVariableDeclaratorNameLocation(field);

        spc.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.MarkedArraylikeHasNoSizeOrSizeProviderRule,
            location,
            field.Name
        ));
    }

    internal static void ReportArrayIsMissingAttribute(this SourceProductionContext spc, IFieldSymbol field) {
        var location = GetVariableDeclaratorNameLocation(field);

        spc.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.ArrayHasNoBinaryArrayAttributeRule,
            location,
            field.Name
        ));
    }

    internal static void ReportUnmarkedSerializableForField(this SourceProductionContext spc, IFieldSymbol field) {
        var location = GetVariableDeclaratorLocation(field);

        spc.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.MissingBinarySerializableAttributeRule,
            location,
            field.Type.Name
        ));
    }

    private static Location GetVariableDeclaratorLocation(IFieldSymbol field) {
        var fieldSyntaxRef = field.DeclaringSyntaxReferences.FirstOrDefault();
        var fieldSyntax = fieldSyntaxRef?.GetSyntax() as VariableDeclaratorSyntax;
        if (fieldSyntax?.Parent is not VariableDeclarationSyntax varDecl) throw new Exception("Field parent is not a variable declaration.");

        return varDecl.Type.GetLocation();
    }

    private static Location GetVariableDeclaratorNameLocation(IFieldSymbol field) {
        var fieldSyntaxRef = field.DeclaringSyntaxReferences.FirstOrDefault();
        if (fieldSyntaxRef is null) throw new Exception("No syntax reference found for field.");

        return fieldSyntaxRef.GetSyntax() is not VariableDeclaratorSyntax fieldSyntax
            ? throw new Exception("Field syntax is not a VariableDeclaratorSyntax.")
            : fieldSyntax.Identifier.GetLocation();
    }
}