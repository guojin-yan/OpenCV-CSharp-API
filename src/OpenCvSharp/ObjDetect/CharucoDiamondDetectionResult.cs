using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>Represents detected ChArUco diamonds and their marker observations.</summary>
    public sealed class CharucoDiamondDetectionResult
    {
        private readonly Point2f[][] diamondCorners;
        private readonly Vec4i[] diamondIds;
        private readonly Point2f[][] markerCorners;
        private readonly int[] markerIds;

        /// <summary>Initializes a diamond detection result.</summary>
        public CharucoDiamondDetectionResult(Point2f[][] diamondCorners, Vec4i[] diamondIds, Point2f[][] markerCorners, int[] markerIds)
        {
            this.diamondCorners = Clone(diamondCorners);
            this.diamondIds = Clone(diamondIds);
            this.markerCorners = Clone(markerCorners);
            this.markerIds = Clone(markerIds);
            if (this.diamondCorners.Length != this.diamondIds.Length) throw new ArgumentException("Diamond corner and id counts must match.", nameof(diamondIds));
            if (this.markerCorners.Length != this.markerIds.Length) throw new ArgumentException("Marker corner and id counts must match.", nameof(markerIds));
        }

        /// <summary>Gets independent diamond corner groups.</summary>
        public Point2f[][] DiamondCorners => Clone(diamondCorners);

        /// <summary>Gets independent four-marker diamond identifiers.</summary>
        public Vec4i[] DiamondIds => Clone(diamondIds);

        /// <summary>Gets independent detected or supplied marker corner groups.</summary>
        public Point2f[][] MarkerCorners => Clone(markerCorners);

        /// <summary>Gets independent detected or supplied marker identifiers.</summary>
        public int[] MarkerIds => Clone(markerIds);

        /// <summary>Gets the diamond count.</summary>
        public int Count => diamondIds.Length;

        /// <inheritdoc />
        public override string ToString() => $"{nameof(CharucoDiamondDetectionResult)}({nameof(Count)}={Count}, Markers={markerIds.Length})";

        private static Point2f[][] Clone(Point2f[][] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var result = new Point2f[values.Length][];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null) throw new ArgumentNullException(nameof(values));
                result[i] = (Point2f[])values[i].Clone();
            }
            return result;
        }

        private static Vec4i[] Clone(Vec4i[] values) => values == null ? throw new ArgumentNullException(nameof(values)) : (Vec4i[])values.Clone();
        private static int[] Clone(int[] values) => values == null ? throw new ArgumentNullException(nameof(values)) : (int[])values.Clone();
    }
}
