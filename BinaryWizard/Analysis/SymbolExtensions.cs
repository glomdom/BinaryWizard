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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace BinaryWizard.Analysis;

public static class SymbolExtensions {
    public static string GetBinaryPrimitiveReader(this ITypeSymbol sym, Endianness endianness) {
        var typeName = sym.SpecialType switch {
            SpecialType.System_Int32 => "Int32",
            SpecialType.System_UInt32 => "UInt32",
            SpecialType.System_Int16 => "Int16",
            SpecialType.System_UInt16 => "UInt16",
            SpecialType.System_Int64 => "Int64",
            SpecialType.System_UInt64 => "UInt64",
            _ => throw new InvalidOperationException($"Unexpected type {sym.SpecialType}")
        };

        var endiannessStr = endianness == Endianness.Little ? "Little" : "Big";

        return $"BinaryPrimitives.Read{typeName}{endiannessStr}Endian";
    }

    public static bool HasBinarySerializableAttribute(this ITypeSymbol sym) => HasAttribute(sym, "BinarySerializableAttribute");
    public static bool HasAttribute(this ITypeSymbol sym, string attrName) => sym.GetAttributes().Any(a => a.AttributeClass?.Name == attrName);

    public static int GetByteSize(this ITypeSymbol primitive) {
        return primitive.SpecialType switch {
            SpecialType.System_Boolean => 1,
            SpecialType.System_Char => 1,
            SpecialType.System_SByte => 1,
            SpecialType.System_Byte => 1,
            SpecialType.System_Int16 => 2,
            SpecialType.System_UInt16 => 2,
            SpecialType.System_Int32 => 4,
            SpecialType.System_UInt32 => 4,
            SpecialType.System_Int64 => 8,
            SpecialType.System_UInt64 => 8,
            SpecialType.System_Decimal => 16,
            SpecialType.System_Double => 32,

            _ => throw new InvalidOperationException("Unexpected case encountered."),
        };
    }

    public static bool IsPrimitiveLike(this ITypeSymbol symbol) {
        return symbol.SpecialType switch {
            SpecialType.System_Boolean or
                SpecialType.System_Byte or
                SpecialType.System_SByte or
                SpecialType.System_Int16 or
                SpecialType.System_UInt16 or
                SpecialType.System_Int32 or
                SpecialType.System_UInt32 or
                SpecialType.System_Int64 or
                SpecialType.System_UInt64 or
                SpecialType.System_Single or
                SpecialType.System_Double or
                SpecialType.System_Char or
                SpecialType.System_String or
                SpecialType.System_Decimal => true,

            _ => false,
        };
    }

    public static bool IsArrayLike(this ITypeSymbol symbol, [NotNullWhen(true)] out IArrayTypeSymbol? arraySymbol) {
        if (symbol is IArrayTypeSymbol arr) {
            arraySymbol = arr;

            return true;
        }

        arraySymbol = null;

        return false;
    }
}