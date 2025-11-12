namespace BinaryWizard.Segmenting;

public record FixedSegment : Segment {
    public int Bytes { get; set; }

    public FixedSegment(int bytes) {
        Bytes = bytes;
    }
}