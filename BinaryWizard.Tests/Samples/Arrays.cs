namespace BinaryWizard.Tests.Samples;

[BinarySerializable]
public partial struct Arrays {
    [BinaryArray(Size = 16)] public int[] NumberArray;
    public int OtherNumbersSize;

    [BinaryArray(SizeMember = nameof(OtherNumbersSize))]
    public int[] OtherNumbers;
}