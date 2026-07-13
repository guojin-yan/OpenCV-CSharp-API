using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Shape
{
    /// <summary>
    /// Base class for histogram cost extractors with a norm flag.
    /// 带范数标志的直方图代价提取器基类。
    /// </summary>
    public abstract class NormHistogramCostExtractorBase : HistogramCostExtractor
    {
        internal NormHistogramCostExtractorBase(NativeShapeHistogramCostExtractorHandle handle)
            : base(handle)
        {
        }

        /// <summary>Gets or sets the norm used by the extractor. 获取或设置提取器使用的范数。</summary>
        public NormTypes NormFlag
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ShapeHistogramCostExtractorGetNormFlag(NativeHandle, out int value));
                return (NormTypes)value;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.ShapeHistogramCostExtractorSetNormFlag(NativeHandle, (int)value));
            }
        }
    }
}
