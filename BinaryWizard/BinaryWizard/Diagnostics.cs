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

using Microsoft.CodeAnalysis;

namespace BinaryWizard;

internal static class Diagnostics {
    internal static readonly DiagnosticDescriptor MissingBinarySerializableAttributeRule = new(
        id: "BW0001",
        title: "Type must be annotated with [BinarySerializable]",
        messageFormat: "The type '{0}' must be annotated with the [BinarySerializable] attribute to support binary parsing",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    
    internal static readonly DiagnosticDescriptor ArraylikeHasNoConstCapacityRule = new(
        id: "BW0002",
        title: "Array-like type has no const capacity defined",
        messageFormat: "The type '{0}' must be annotated with either the [BinaryArray] attribute or have a known size at compile time to support binary parsing",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}