/*
 * Copyright 2025 glomdom
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