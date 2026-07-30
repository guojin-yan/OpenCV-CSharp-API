using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Edge-aware sparse match interpolator.
    /// 边缘感知稀疏匹配插值器。
    /// </summary>
    public sealed class EdgeAwareInterpolator : SparseMatchInterpolator
    {
        private NativeXImgProcEdgeAwareInterpolatorHandle handle;

        private EdgeAwareInterpolator(IntPtr nativeHandle)
        {
            handle = NativeXImgProcEdgeAwareInterpolatorHandle.FromNativePointer(nativeHandle);
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
        public int K
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorGetK(NativeHandle, out int value)); return value; }
            set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorSetK(NativeHandle, value)); }
        }

        /// <summary>Gets or sets local affine weighting sigma. 获取或设置局部仿射权重 sigma。</summary>
        public float Sigma
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorGetSigma(NativeHandle, out float value)); return value; }
            set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorSetSigma(NativeHandle, value)); }
        }

        /// <summary>Gets or sets edge-aware lambda. 获取或设置边缘感知 lambda。</summary>
        public float Lambda
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorGetLambda(NativeHandle, out float value)); return value; }
            set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorSetLambda(NativeHandle, value)); }
        }

        /// <summary>Gets or sets whether FGS post-processing is used. 获取或设置是否使用 FGS 后处理。</summary>
        public bool UsePostProcessing
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorGetUsePostProcessing(NativeHandle, out int value)); return value != 0; }
            set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorSetUsePostProcessing(NativeHandle, value ? 1 : 0)); }
        }

        /// <summary>Gets or sets FGS lambda. 获取或设置 FGS lambda。</summary>
        public float FGSLambda
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorGetFgsLambda(NativeHandle, out float value)); return value; }
            set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorSetFgsLambda(NativeHandle, value)); }
        }

        /// <summary>Gets or sets FGS sigma. 获取或设置 FGS sigma。</summary>
        public float FGSSigma
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorGetFgsSigma(NativeHandle, out float value)); return value; }
            set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorSetFgsSigma(NativeHandle, value)); }
        }

        /// <summary>Creates an edge-aware interpolator. 创建边缘感知插值器。</summary>
        public static EdgeAwareInterpolator Create()
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorCreate(out IntPtr nativeHandle));
            return new EdgeAwareInterpolator(nativeHandle);
        }

        /// <inheritdoc/>
        public override void Interpolate(Mat fromImage, Mat fromPoints, Mat toImage, Mat toPoints, Mat denseFlow)
        {
            ThrowIfDisposed();
            ValidateInterpolateArgs(fromImage, fromPoints, toImage, toPoints, denseFlow);
            ValidateEdgeAwareMatchCount(fromPoints);
            NativeException.ThrowIfError(NativeMethods.XImgProcSparseMatchInterpolatorInterpolate(NativeHandle, fromImage.NativeHandle, fromPoints.NativeHandle, toImage.NativeHandle, toPoints.NativeHandle, denseFlow.NativeHandle));
        }

        /// <summary>Sets an explicit cost map. 设置显式 cost map。</summary>
        public void SetCostMap(Mat costMap)
        {
            ThrowIfDisposed();
            ValidateCostMap(costMap);
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgeAwareInterpolatorSetCostMap(NativeHandle, costMap.NativeHandle));
        }

        internal static void ValidateInterpolateArgs(Mat fromImage, Mat fromPoints, Mat toImage, Mat toPoints, Mat denseFlow)
        {
            XImgProcCv2.ValidateNotNull(fromImage, nameof(fromImage));
            XImgProcCv2.ValidateNotNull(fromPoints, nameof(fromPoints));
            XImgProcCv2.ValidateNotNull(toImage, nameof(toImage));
            XImgProcCv2.ValidateNotNull(toPoints, nameof(toPoints));
            XImgProcCv2.ValidateNotNull(denseFlow, nameof(denseFlow));

            ValidateSparseMatchImage(fromImage, nameof(fromImage));
            ValidateSparseMatchPoints(fromPoints, nameof(fromPoints));
            ValidateSparseMatchPoints(toPoints, nameof(toPoints));

            if (fromPoints.Rows != toPoints.Rows || fromPoints.Cols != toPoints.Cols)
            {
                throw new ArgumentException("Sparse match point matrices must have the same size.", nameof(toPoints));
            }
        }

        internal static void ValidateSparseMatchImage(Mat image, string parameterName)
        {
            if (image.Empty)
            {
                throw new ArgumentException("Sparse match source image must not be empty.", parameterName);
            }

            if (MatType.Depth(image.Type) != MatType.CV_8U)
            {
                throw new ArgumentException("Sparse match source image depth must be CV_8U.", parameterName);
            }

            int channels = MatType.Channels(image.Type);
            if (channels != 1 && channels != 3)
            {
                throw new ArgumentException("Sparse match source image must have 1 or 3 channels.", parameterName);
            }
        }

        private static void ValidateSparseMatchPoints(Mat points, string parameterName)
        {
            if (points.Empty)
            {
                throw new ArgumentException("Sparse match point matrix must not be empty.", parameterName);
            }

            if (MatType.Depth(points.Type) != MatType.CV_32F)
            {
                throw new ArgumentException("Sparse match point matrix depth must be CV_32F.", parameterName);
            }

            int channels = MatType.Channels(points.Type);
            if ((channels == 2 && points.Cols == 1) ||
                (channels == 1 && points.Cols == 2))
            {
                return;
            }

            throw new ArgumentException("Sparse match point matrix must describe 2D points.", parameterName);
        }

        internal static void ValidateCostMap(Mat costMap)
        {
            XImgProcCv2.ValidateNotNull(costMap, nameof(costMap));

            if (!costMap.Empty && costMap.Type != MatType.CV_32FC1)
            {
                throw new ArgumentException("Sparse match cost map must be empty or CV_32FC1.", nameof(costMap));
            }
        }

        private static void ValidateEdgeAwareMatchCount(Mat fromPoints)
        {
            if (fromPoints.Rows >= short.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(fromPoints), fromPoints.Rows, "Edge-aware sparse match interpolator requires fewer than 32767 matches.");
            }
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
