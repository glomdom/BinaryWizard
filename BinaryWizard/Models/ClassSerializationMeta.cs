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

using System.Collections.Generic;
using BinaryWizard.Segmenting;
using Microsoft.CodeAnalysis;

namespace BinaryWizard.Models;

// TODO: Maybe make a factory for this, ctor might grow more.
internal sealed record ClassSerializationMeta {
    internal string Namespace { get; }
    internal string ClassName { get; }
    internal string Kind { get; }
    internal Endianness Endianness { get; }
    internal IReadOnlyList<Segment> Segments { get; }
    internal IReadOnlyList<Diagnostic> Diagnostics { get; }

    internal ClassSerializationMeta(string ns, string className, string kind, IReadOnlyList<Segment> segments, IReadOnlyList<Diagnostic> diagnostics, Endianness endianness) {
        Namespace = ns;
        ClassName = className;
        Kind = kind;
        Segments = segments;
        Diagnostics = diagnostics;
        Endianness = endianness;
    }
}