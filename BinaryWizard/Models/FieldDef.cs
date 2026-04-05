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
using Microsoft.CodeAnalysis;

namespace BinaryWizard.Models;

public sealed record FieldDef {
    public string Name { get; set; }
    public int ByteSize { get; set; }
    public TypeModel TypeModel { get; set; }

    public bool HasMagic => _magic is not null;
    public string Magic => HasMagic ? _magic! : throw new InvalidOperationException("Attempted to get magic when HasMagic is false.");

    public bool IsDynamic => ByteSize == -1;

    private readonly string? _magic;

    public FieldDef(string name, ITypeSymbol type, string? magic = null) {
        Name = name;
        TypeModel = new TypeModel(type);

        _magic = magic;
    }
}