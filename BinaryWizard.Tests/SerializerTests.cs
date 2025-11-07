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