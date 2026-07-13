using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Fourier-descriptor contour fitting wrapper.
    /// Fourier descriptor 轮廓拟合包装。
    /// </summary>
    public sealed class ContourFitting : IDisposable
    {
        private NativeXImgProcContourFittingHandle handle;
        private bool disposed;

        private ContourFitting(IntPtr nativeHandle)
        {
            handle = NativeXImgProcContourFittingHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this fitter has been disposed. 获取拟合器是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets the contour resampling size. 获取或设置轮廓重采样大小。</summary>
        public int CtrSize
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcContourFittingGetCtrSize(NativeHandle, out int value)); return value; }
            set { ThrowIfDisposed(); ValidatePositiveSize(value, nameof(value)); NativeException.ThrowIfError(NativeMethods.XImgProcContourFittingSetCtrSize(NativeHandle, value)); }
        }

        /// <summary>Gets or sets the number of Fourier descriptors used for matching. 获取或设置用于匹配的 Fourier descriptor 数量。</summary>
        public int FDSize
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcContourFittingGetFDSize(NativeHandle, out int value)); return value; }
            set { ThrowIfDisposed(); ValidatePositiveSize(value, nameof(value)); NativeException.ThrowIfError(NativeMethods.XImgProcContourFittingSetFDSize(NativeHandle, value)); }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a contour fitting object. 创建轮廓拟合对象。</summary>
        public static ContourFitting Create(int ctr = 1024, int fd = 16)
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcContourFittingCreate(ctr, fd, out IntPtr nativeHandle));
            return new ContourFitting(nativeHandle);
        }

        /// <summary>Estimates the transform between two contours or descriptor matrices. 估计两个轮廓或 descriptor 矩阵之间的变换。</summary>
        public double EstimateTransformation(Mat src, Mat dst, Mat alphaPhiST, bool fdContour = false)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(src, nameof(src));
            XImgProcCv2.ValidateNotNull(dst, nameof(dst));
            XImgProcCv2.ValidateNotNull(alphaPhiST, nameof(alphaPhiST));
            ValidateDescriptorSizeForContourSize(CtrSize, FDSize);
            NativeException.ThrowIfError(NativeMethods.XImgProcContourFittingEstimateTransformation(NativeHandle, src.NativeHandle, dst.NativeHandle, alphaPhiST.NativeHandle, out double distance, fdContour ? 1 : 0));
            return distance;
        }

        /// <summary>Estimates the transform and returns the transform matrix. 估计变换并返回变换矩阵。</summary>
        public Mat EstimateTransformation(Mat src, Mat dst, out double distance, bool fdContour = false)
        {
            var transform = new Mat();
            try
            {
                distance = EstimateTransformation(src, dst, transform, fdContour);
                return transform;
            }
            catch
            {
                transform.Dispose();
                throw;
            }
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && handle != null)
                {
                    handle.Dispose();
                }

                disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private static void ValidatePositiveSize(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Contour fitting size must be greater than zero.");
            }
        }

        private static void ValidateDescriptorSizeForContourSize(int ctrSize, int fdSize)
        {
            if (fdSize > ctrSize / 2 - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(FDSize), "FDSize must be less than or equal to CtrSize / 2 - 1.");
            }
        }
    }
}
