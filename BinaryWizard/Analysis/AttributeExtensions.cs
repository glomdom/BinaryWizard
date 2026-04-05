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

using System.Linq;
using Microsoft.CodeAnalysis;

namespace BinaryWizard.Analysis;

public static class AttributeExtensions {
    public static bool AreAllNamedArgsProvided(this AttributeData attr, params string[] names) {
        return attr.NamedArguments.Length != 0 && names.Select(name => attr.NamedArguments.Any(pair => pair.Key == name)).All(found => found);
    }

    public static bool AreAnyNamedArgsProvided(this AttributeData attr, params string[] names) {
        return attr.NamedArguments.Length != 0 && names.Any(name => attr.NamedArguments.Any(pair => pair.Key == name));
    }

    public static bool TryGetNamedArg(
        this AttributeData attr,
        string name,
        out TypedConstant typedConstant
    ) {
        foreach (var pair in attr.NamedArguments.Where(pair => pair.Key == name)) {
            typedConstant = pair.Value;

            return true;
        }

        typedConstant = default;

        return false;
    }
}