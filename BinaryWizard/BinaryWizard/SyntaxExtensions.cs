using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BinaryWizard;

public static class SyntaxExtensions {
    public static MethodDeclarationSyntax AddParameters(this MethodDeclarationSyntax method, params (string name, string type)[] parameters) {
        var list = SyntaxFactory.SeparatedList(parameters.Select(p =>
            SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.name))
                .WithType(SyntaxFactory.ParseTypeName(p.type))
        ));

        return method.WithParameterList(SyntaxFactory.ParameterList(list));
    }
}