using System;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>Pairs a marker detection result with the source dictionary index for each marker.</summary>
    public sealed class ArucoMultiDictionaryDetectionResult
    {
        private readonly int[] dictionaryIndices;

        /// <summary>Initializes a multi-dictionary detection result.</summary>
        public ArucoMultiDictionaryDetectionResult(ArucoDetectionResult detection, int[] dictionaryIndices)
        {
            Detection = detection ?? throw new ArgumentNullException(nameof(detection));
            if (dictionaryIndices == null) throw new ArgumentNullException(nameof(dictionaryIndices));
            if (dictionaryIndices.Length != detection.Count) throw new ArgumentException("Dictionary index count must match detection count.", nameof(dictionaryIndices));
            this.dictionaryIndices = (int[])dictionaryIndices.Clone();
        }

        /// <summary>Gets the marker corners, ids, confidence values, and rejected candidates.</summary>
        public ArucoDetectionResult Detection { get; }

        /// <summary>Gets a copy of source dictionary indices.</summary>
        public int[] DictionaryIndices => (int[])dictionaryIndices.Clone();

        /// <summary>Gets the marker count.</summary>
        public int Count => dictionaryIndices.Length;

        /// <inheritdoc />
        public override string ToString() => $"{nameof(ArucoMultiDictionaryDetectionResult)}({nameof(Count)}={Count})";
    }
}
