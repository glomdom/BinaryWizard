using System.Collections.Generic;
using System.Linq;
using BinaryWizard.Model;

namespace BinaryWizard.Segmenting;

sealed internal class SegmentManager {
    private readonly List<Segment> _segments = [];
    private readonly List<FieldDef> _currentFields = [];
    private int _currentFixedSize;

    public void AddField(FieldDef field) {
        _currentFields.Add(field);
        _currentFixedSize += field.ByteSize;
    }

    public IReadOnlyList<Segment> Commit() {
        CommitFixed();

        return _segments;
    }

    private void CommitFixed() {
        if (_currentFields.Count == 0) {
            return;
        }

        _segments.Add(new FixedSegment(_currentFields.ToList(), _currentFixedSize));
        _currentFields.Clear();
        _currentFixedSize = 0;
    }
}