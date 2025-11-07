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

using System.Text;
using BinaryWizard.Tests.Samples;

namespace BinaryWizard.Tests;

public class SerializerTests {
    [Fact]
    public void Vector3_CorrectlySerialized() {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true)) {
            writer.Write(1); // X
            writer.Write(2); // Y
            writer.Write(3); // Z
        }

        stream.Position = 0;

        using var reader = new BinaryReader(stream);

        var actual = Vector3.FromBinary(reader);
        var expected = new Vector3 { X = 1, Y = 2, Z = 3 };

        Assert.Equal(expected, actual);
    }
}