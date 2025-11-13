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

using System.Collections.Generic;
using System.Diagnostics;
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
        
        Debug.WriteLine($"Finalized segmenting with {_segments.Count} segments");

        return _segments;
    }

    private void CommitFixed() {
        if (_currentFields.Count == 0) {
            return;
        }

        Debug.WriteLine($"Committing fixed segment with {_currentFields.Count} segments and size of {_currentFixedSize} bytes");

        _segments.Add(new FixedSegment(_currentFields.ToList(), _currentFixedSize));
        _currentFields.Clear();
        _currentFixedSize = 0;
    }
}