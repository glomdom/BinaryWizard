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
        messageFormat: "The type '{0}' must be annotated with a [BinarySerializable] attribute to support binary serdes",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    
    internal static readonly DiagnosticDescriptor ArrayHasNoBinaryArrayAttributeRule = new(
        id: "BW0002",
        title: "Array type does not have a [BinaryArray] attribute",
        messageFormat: "The field '{0}' must be annotated with a [BinaryArray] attribute to support binary serdes",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    
    internal static readonly DiagnosticDescriptor MarkedArraylikeHasNoSizeOrSizeProviderRule = new(
        id: "BW0003",
        title: "Array marked with [BinaryArray] is missing size provider or constant size",
        messageFormat: "The [BinaryArray] attribute on field '{0}' is missing argument defining size",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
    
    internal static readonly DiagnosticDescriptor ArrayHasConflictingSizeArguments = new(
        id: "BW0004",
        title: "Array marked with [BinaryArray] has conflicting size arguments",
        messageFormat: "The [BinaryArray] attribute on field '{0}' has arguments which conflict",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true
    );
}