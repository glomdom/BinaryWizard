using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryWizard.Diagnostics;

public static class DiagnosticReporter {
    public static void ReportArrayHasConflictingSizeArguments(this SourceProductionContext spc, IFieldSymbol field) {
        var location = GetVariableDeclaratorNameLocation(field);

        spc.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.ArrayHasConflictingSizeArguments,
            location,
            field.Name
        ));
    }

    public static void ReportArrayIsMissingSizeArgument(this SourceProductionContext spc, IFieldSymbol field) {
        var location = GetVariableDeclaratorNameLocation(field);

        spc.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.MarkedArraylikeHasNoSizeOrSizeProviderRule,
            location,
            field.Name
        ));
    }

    public static void ReportArrayIsMissingAttribute(this SourceProductionContext spc, IFieldSymbol field) {
        var location = GetVariableDeclaratorNameLocation(field);

        spc.ReportDiagnostic(Diagnostic.Create(
            Diagnostics.ArrayHasNoBinaryArrayAttributeRule,
            location,
            field.Name
        ));
    }

    public static void ReportUnmarkedSerializableForField(this SourceProductionContext spc, IFieldSymbol field) {
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