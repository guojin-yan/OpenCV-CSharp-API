using System;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Ridge detection filter wrapper.
    /// RidgeDetectionFilter 脊线检测滤波器包装。
    /// </summary>
    public sealed class RidgeDetectionFilter : IDisposable
    {
        private NativeXImgProcRidgeDetectionFilterHandle handle;
        private bool disposed;

        private RidgeDetectionFilter(IntPtr nativeHandle)
        {
            handle = NativeXImgProcRidgeDetectionFilterHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this filter has been disposed. 获取滤波器是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a ridge detection filter. 创建脊线检测滤波器。</summary>
        public static RidgeDetectionFilter Create(int ddepth = MatType.CV_32FC1, int dx = 1, int dy = 1, int ksize = 3, int outDtype = MatType.CV_8UC1, double scale = 1.0, double delta = 0.0, BorderTypes borderType = BorderTypes.Default)
        {
            ValidateCreateArguments(ddepth, ksize);
            NativeException.ThrowIfError(NativeMethods.XImgProcRidgeDetectionFilterCreate(ddepth, dx, dy, ksize, outDtype, scale, delta, (int)borderType, out IntPtr nativeHandle));
            return new RidgeDetectionFilter(nativeHandle);
        }

        /// <summary>Writes the ridge-filtered image into <paramref name="dst"/>. 将脊线滤波结果写入 <paramref name="dst"/>。</summary>
        public void GetRidgeFilteredImage(Mat src, Mat dst)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(src, nameof(src));
            XImgProcCv2.ValidateNotNull(dst, nameof(dst));
            ValidateSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcRidgeDetectionFilterGetImage(NativeHandle, src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Gets the ridge-filtered image as a new matrix. 以新矩阵返回脊线滤波结果。</summary>
        public Mat GetRidgeFilteredImage(Mat src)
        {
            return CreateOutput(delegate (Mat dst) { GetRidgeFilteredImage(src, dst); });
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static Mat CreateOutput(Action<Mat> action)
        {
            var dst = new Mat();
            try
            {
                action(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }

        private static void ValidateCreateArguments(int ddepth, int ksize)
        {
            if (ksize != 1 && ksize != 3 && ksize != 5 && ksize != 7)
            {
                throw new ArgumentOutOfRangeException(nameof(ksize), "Ridge detection kernel size must be 1, 3, 5, or 7.");
            }

            if (ddepth != MatType.CV_32FC1 && ddepth != MatType.CV_64FC1)
            {
                throw new ArgumentOutOfRangeException(nameof(ddepth), "Ridge detection ddepth must be CV_32FC1 or CV_64FC1.");
            }
        }

        private static void ValidateSource(Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Ridge detection source image must not be empty.", nameof(src));
            }

            int channels = MatType.Channels(src.Type);
            if (channels != 1 && channels != 3)
            {
                throw new ArgumentException("Ridge detection source image must have 1 or 3 channels.", nameof(src));
            }
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
    }
}
