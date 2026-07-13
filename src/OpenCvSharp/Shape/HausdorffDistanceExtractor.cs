using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Shape
{
    /// <summary>
    /// Hausdorff distance extractor for contour shapes.
    /// 面向轮廓形状的 Hausdorff 距离提取器。
    /// </summary>
    public sealed class HausdorffDistanceExtractor : ShapeDistanceExtractor
    {
        private HausdorffDistanceExtractor(NativeShapeDistanceExtractorHandle handle)
            : base(handle)
        {
        }

        /// <summary>Gets or sets the norm used by the Hausdorff distance. 获取或设置 Hausdorff 距离使用的范数。</summary>
        public NormTypes DistanceFlag
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.ShapeHausdorffDistanceExtractorGetDistanceFlag(NativeHandle, out int value));
                return (NormTypes)value;
            }
            set
            {
                NativeException.ThrowIfError(NativeMethods.ShapeHausdorffDistanceExtractorSetDistanceFlag(NativeHandle, (int)value));
            }
        }

        /// <summary>Gets or sets the partial Hausdorff rank proportion. 获取或设置 partial Hausdorff 的 rank proportion。</summary>
        public float RankProportion
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.ShapeHausdorffDistanceExtractorGetRankProportion(NativeHandle, out float value));
                return value;
            }
            set
            {
                ValidateRankProportion(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.ShapeHausdorffDistanceExtractorSetRankProportion(NativeHandle, value));
            }
        }

        /// <summary>
        /// Creates a Hausdorff distance extractor.
        /// 创建 Hausdorff 形状距离提取器。
        /// </summary>
        public static HausdorffDistanceExtractor Create(NormTypes distanceFlag = NormTypes.L2, float rankProportion = 0.6F)
        {
            ValidateRankProportion(rankProportion, nameof(rankProportion));
            NativeException.ThrowIfError(NativeMethods.ShapeHausdorffDistanceExtractorCreate((int)distanceFlag, rankProportion, out IntPtr nativeHandle));
            return new HausdorffDistanceExtractor(NativeShapeDistanceExtractorHandle.FromNativePointer(nativeHandle));
        }

        private static void ValidateRankProportion(float value, string parameterName)
        {
            if (float.IsNaN(value) || value <= 0.0F || value > 1.0F)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Rank proportion must be greater than 0.0 and less than or equal to 1.0.");
            }
        }
    }
}
