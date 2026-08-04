using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.XImgProc
{
    /// <summary>
    /// Combined Selective Search strategy wrapper.
    /// Selective Search 组合策略包装。
    /// </summary>
    public sealed class SelectiveSearchSegmentationStrategyMultiple : SelectiveSearchSegmentationStrategy
    {
        internal SelectiveSearchSegmentationStrategyMultiple(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Adds a sub-strategy with a weight. 按权重添加子策略。</summary>
        public void AddStrategy(SelectiveSearchSegmentationStrategy strategy, float weight)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(strategy, nameof(strategy));
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchStrategyMultipleAdd(NativeHandle, strategy.NativeHandle, weight));
        }

        /// <summary>Clears all sub-strategies. 清空所有子策略。</summary>
        public void ClearStrategies()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSelectiveSearchStrategyMultipleClear(NativeHandle));
        }
    }
}
