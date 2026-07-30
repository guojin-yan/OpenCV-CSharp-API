using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Robust Interpolation of Correspondences sparse match interpolator.
    /// RIC 稀疏匹配插值器。
    /// </summary>
    public sealed class RICInterpolator : SparseMatchInterpolator
    {
        private NativeXImgProcRICInterpolatorHandle handle;

        private RICInterpolator(IntPtr nativeHandle)
        {
            handle = NativeXImgProcRICInterpolatorHandle.FromNativePointer(nativeHandle);
        }

        internal override IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Gets or sets nearest-neighbor match count. 获取或设置近邻匹配数量。</summary>
        public int K { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetK(NativeHandle, out int v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetK(NativeHandle, value)); } }

        /// <summary>Gets or sets superpixel size. 获取或设置超像素尺寸。</summary>
        public int SuperpixelSize { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetSuperpixelSize(NativeHandle, out int v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetSuperpixelSize(NativeHandle, value)); } }

        /// <summary>Gets or sets superpixel nearest-neighbor count. 获取或设置每个超像素近邻数量。</summary>
        public int SuperpixelNNCount { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetSuperpixelNNCount(NativeHandle, out int v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetSuperpixelNNCount(NativeHandle, value)); } }

        /// <summary>Gets or sets superpixel ruler. 获取或设置超像素 ruler。</summary>
        public float SuperpixelRuler { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetSuperpixelRuler(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetSuperpixelRuler(NativeHandle, value)); } }

        /// <summary>Gets or sets the SLIC mode used for superpixels. 获取或设置超像素 SLIC 模式。</summary>
        public SLICType SuperpixelMode
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetSuperpixelMode(NativeHandle, out int v)); return (SLICType)v; }
            set
            {
                ThrowIfDisposed();
                XImgProcCv2.ValidateSLICType(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetSuperpixelMode(NativeHandle, (int)value));
            }
        }

        /// <summary>Gets or sets alpha. 获取或设置 alpha。</summary>
        public float Alpha { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetAlpha(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetAlpha(NativeHandle, value)); } }

        /// <summary>Gets or sets model iteration count. 获取或设置模型迭代次数。</summary>
        public int ModelIter { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetModelIter(NativeHandle, out int v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetModelIter(NativeHandle, value)); } }

        /// <summary>Gets or sets whether piecewise models are refined. 获取或设置是否细化分片模型。</summary>
        public bool RefineModels { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetRefineModels(NativeHandle, out int v)); return v != 0; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetRefineModels(NativeHandle, value ? 1 : 0)); } }

        /// <summary>Gets or sets maximum flow threshold. 获取或设置最大 flow 阈值。</summary>
        public float MaxFlow { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetMaxFlow(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetMaxFlow(NativeHandle, value)); } }

        /// <summary>Gets or sets whether variational refinement is used. 获取或设置是否使用 variational refinement。</summary>
        public bool UseVariationalRefinement { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetUseVariationalRefinement(NativeHandle, out int v)); return v != 0; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetUseVariationalRefinement(NativeHandle, value ? 1 : 0)); } }

        /// <summary>Gets or sets whether fast global smoother is used. 获取或设置是否使用 fast global smoother。</summary>
        public bool UseGlobalSmootherFilter { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetUseGlobalSmootherFilter(NativeHandle, out int v)); return v != 0; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetUseGlobalSmootherFilter(NativeHandle, value ? 1 : 0)); } }

        /// <summary>Gets or sets FGS lambda. 获取或设置 FGS lambda。</summary>
        public float FGSLambda { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetFgsLambda(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetFgsLambda(NativeHandle, value)); } }

        /// <summary>Gets or sets FGS sigma. 获取或设置 FGS sigma。</summary>
        public float FGSSigma { get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorGetFgsSigma(NativeHandle, out float v)); return v; } set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetFgsSigma(NativeHandle, value)); } }

        /// <summary>Creates a RIC interpolator. 创建 RIC 插值器。</summary>
        public static RICInterpolator Create()
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorCreate(out IntPtr nativeHandle));
            return new RICInterpolator(nativeHandle);
        }

        /// <inheritdoc/>
        public override void Interpolate(Mat fromImage, Mat fromPoints, Mat toImage, Mat toPoints, Mat denseFlow)
        {
            ThrowIfDisposed();
            EdgeAwareInterpolator.ValidateInterpolateArgs(fromImage, fromPoints, toImage, toPoints, denseFlow);

            if (UseVariationalRefinement)
            {
                EdgeAwareInterpolator.ValidateSparseMatchImage(toImage, nameof(toImage));
            }

            NativeException.ThrowIfError(NativeMethods.XImgProcSparseMatchInterpolatorInterpolate(NativeHandle, fromImage.NativeHandle, fromPoints.NativeHandle, toImage.NativeHandle, toPoints.NativeHandle, denseFlow.NativeHandle));
        }

        /// <summary>Sets an explicit cost map. 设置显式 cost map。</summary>
        public void SetCostMap(Mat costMap)
        {
            ThrowIfDisposed();
            EdgeAwareInterpolator.ValidateCostMap(costMap);
            NativeException.ThrowIfError(NativeMethods.XImgProcRICInterpolatorSetCostMap(NativeHandle, costMap.NativeHandle));
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing && handle != null)
            {
                handle.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
