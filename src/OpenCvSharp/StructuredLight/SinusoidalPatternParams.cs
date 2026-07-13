using System;
using System.Globalization;
using OpenCvSharp.Core;

namespace OpenCvSharp.StructuredLight
{
    /// <summary>
    /// Parameters for a sinusoidal structured-light pattern.
    /// 正弦结构光图案参数。
    /// </summary>
    public sealed class SinusoidalPatternParams
    {
        private Point2f[] markersLocation;

        /// <summary>
        /// Initializes default sinusoidal pattern parameters.
        /// 初始化默认正弦图案参数。
        /// </summary>
        public SinusoidalPatternParams()
        {
            Width = 800;
            Height = 600;
            NbrOfPeriods = 20;
            ShiftValue = (float)(2.0 * Math.PI / 3.0);
            Method = SinusoidalPatternMethod.Ftp;
            NbrOfPixelsBetweenMarkers = 70;
            Horizontal = false;
            SetMarkers = false;
            markersLocation = Array.Empty<Point2f>();
        }

        /// <summary>
        /// Initializes parameters by copying another instance.
        /// 通过复制另一个实例初始化参数。
        /// </summary>
        /// <param name="other">The parameters to copy. 要复制的参数。</param>
        public SinusoidalPatternParams(SinusoidalPatternParams other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Width = other.Width;
            Height = other.Height;
            NbrOfPeriods = other.NbrOfPeriods;
            ShiftValue = other.ShiftValue;
            Method = other.Method;
            NbrOfPixelsBetweenMarkers = other.NbrOfPixelsBetweenMarkers;
            Horizontal = other.Horizontal;
            SetMarkers = other.SetMarkers;
            markersLocation = Clone(other.markersLocation);
        }

        /// <summary>Gets the projector width. 获取投影仪宽度。</summary>
        public int Width { get; set; }

        /// <summary>Gets the projector height. 获取投影仪高度。</summary>
        public int Height { get; set; }

        /// <summary>Gets the number of periods along the pattern direction. 获取图案方向上的周期数量。</summary>
        public int NbrOfPeriods { get; set; }

        /// <summary>Gets the phase shift between consecutive patterns. 获取相邻图案之间的相位偏移。</summary>
        public float ShiftValue { get; set; }

        /// <summary>Gets the sinusoidal pattern method. 获取正弦图案方法。</summary>
        public SinusoidalPatternMethod Method { get; set; }

        /// <summary>Gets the number of pixels between marker points. 获取相邻 marker 之间的像素数。</summary>
        public int NbrOfPixelsBetweenMarkers { get; set; }

        /// <summary>Gets whether the pattern is horizontal. 获取图案是否为水平方向。</summary>
        public bool Horizontal { get; set; }

        /// <summary>Gets whether marker locations should be used. 获取是否使用 marker 位置。</summary>
        public bool SetMarkers { get; set; }

        /// <summary>Gets marker locations used by marker-enabled patterns. 获取启用 marker 时使用的位置。</summary>
        public Point2f[] MarkersLocation
        {
            get { return Clone(markersLocation); }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                markersLocation = Clone(value);
            }
        }

        /// <summary>Creates an OpenCV default parameter set. 创建 OpenCV 默认参数。</summary>
        public static SinusoidalPatternParams Default()
        {
            return new SinusoidalPatternParams();
        }

        /// <summary>
        /// Creates a deep copy of this parameter set.
        /// 创建此参数集的深拷贝。
        /// </summary>
        public SinusoidalPatternParams Clone()
        {
            return new SinusoidalPatternParams(this);
        }

        /// <summary>Validates parameter ranges. 验证参数范围。</summary>
        public void Validate()
        {
            if (Width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Width), "Width must be positive.");
            }

            if (Height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(Height), "Height must be positive.");
            }

            if (NbrOfPeriods <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(NbrOfPeriods), "Period count must be positive.");
            }

            if (ShiftValue <= 0.0F || float.IsNaN(ShiftValue) || float.IsInfinity(ShiftValue))
            {
                throw new ArgumentOutOfRangeException(nameof(ShiftValue), "Shift value must be a finite positive value.");
            }

            if (!Enum.IsDefined(typeof(SinusoidalPatternMethod), Method))
            {
                throw new ArgumentOutOfRangeException(nameof(Method), "Unknown sinusoidal pattern method.");
            }

            if (NbrOfPixelsBetweenMarkers < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(NbrOfPixelsBetweenMarkers), "Marker spacing must be non-negative.");
            }

            if (markersLocation == null)
            {
                throw new ArgumentNullException(nameof(MarkersLocation));
            }
        }

        internal Point2f[] GetMarkersLocationSnapshot()
        {
            return Clone(markersLocation);
        }

        private static Point2f[] Clone(Point2f[] values)
        {
            if (values.Length == 0)
            {
                return Array.Empty<Point2f>();
            }

            var result = new Point2f[values.Length];
            Array.Copy(values, result, values.Length);
            return result;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Width=" + Width
                + ",Height=" + Height
                + ",NbrOfPeriods=" + NbrOfPeriods
                + ",ShiftValue=" + ShiftValue.ToString(CultureInfo.InvariantCulture)
                + ",Method=" + Method
                + ",NbrOfPixelsBetweenMarkers=" + NbrOfPixelsBetweenMarkers
                + ",Horizontal=" + Horizontal
                + ",SetMarkers=" + SetMarkers
                + ",MarkersLocation=" + (markersLocation == null ? "<null>" : markersLocation.Length.ToString(CultureInfo.InvariantCulture))
                + "}";
        }
    }
}
