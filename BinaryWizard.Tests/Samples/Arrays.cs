namespace BinaryWizard.Tests.Samples;

[BinarySerializable]
public partial class Arrays {
    [BinaryArray(Size = 16)]
    public int[] NumberArray;
}