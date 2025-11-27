namespace BinaryWizard.Tests.Samples;

[BinarySerializable]
public partial struct Arrays {
    [BinaryArray(Size = 16)]
    public int[] NumberArray;
}