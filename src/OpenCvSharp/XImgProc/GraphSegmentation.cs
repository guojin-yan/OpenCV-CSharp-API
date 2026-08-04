using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.XImgProc
{
    /// <summary>
    /// Graph-based image segmentation wrapper.
    /// 基于图的图像分割包装。
    /// </summary>
    public sealed class GraphSegmentation : IDisposable
    {
        private NativeXImgProcGraphSegmentationHandle handle;
        private bool disposed;

        private GraphSegmentation(IntPtr nativeHandle)
        {
            handle = NativeXImgProcGraphSegmentationHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this segmenter has been disposed. 获取分割器是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets the smoothing sigma. 获取或设置平滑 sigma。</summary>
        public double Sigma
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcGraphSegmentationGetSigma(NativeHandle, out double value)); return value; }
            set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcGraphSegmentationSetSigma(NativeHandle, value)); }
        }

        /// <summary>Gets or sets the segmentation scale parameter. 获取或设置分割尺度参数。</summary>
        public float K
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcGraphSegmentationGetK(NativeHandle, out float value)); return value; }
            set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcGraphSegmentationSetK(NativeHandle, value)); }
        }

        /// <summary>Gets or sets the minimum segment size. 获取或设置最小分割区域大小。</summary>
        public int MinSize
        {
            get { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcGraphSegmentationGetMinSize(NativeHandle, out int value)); return value; }
            set { ThrowIfDisposed(); NativeException.ThrowIfError(NativeMethods.XImgProcGraphSegmentationSetMinSize(NativeHandle, value)); }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a graph segmentation object. 创建基于图的分割对象。</summary>
        public static GraphSegmentation Create(double sigma = 0.5, float k = 300.0F, int minSize = 100)
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcGraphSegmentationCreate(sigma, k, minSize, out IntPtr nativeHandle));
            return new GraphSegmentation(nativeHandle);
        }

        /// <summary>Segments <paramref name="src"/> and writes labels into <paramref name="dst"/>. 分割 <paramref name="src"/> 并将标签写入 <paramref name="dst"/>。</summary>
        public void ProcessImage(Mat src, Mat dst)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(src, nameof(src));
            XImgProcCv2.ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.XImgProcGraphSegmentationProcessImage(NativeHandle, src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Segments an image and returns labels as a new matrix. 分割图像并以新矩阵返回标签。</summary>
        public Mat ProcessImage(Mat src)
        {
            var dst = new Mat();
            try
            {
                ProcessImage(src, dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
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
    }
}
