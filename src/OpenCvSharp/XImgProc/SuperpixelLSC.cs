using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// LSC superpixel segmentation wrapper.
    /// LSC 超像素分割包装。
    /// </summary>
    public sealed class SuperpixelLSC : IDisposable
    {
        private NativeXImgProcSuperpixelLSCHandle handle;
        private bool disposed;

        private SuperpixelLSC(IntPtr nativeHandle)
        {
            handle = NativeXImgProcSuperpixelLSCHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this segmenter has been disposed. 获取分割器是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets the current number of superpixels. 获取当前超像素数量。</summary>
        public int NumberOfSuperpixels
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelLSCGetNumber(NativeHandle, out int value));
                return value;
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates an LSC superpixel segmenter. 创建 LSC 超像素分割器。</summary>
        public static SuperpixelLSC Create(Mat image, int regionSize = 10, float ratio = 0.075F)
        {
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            if (image.Empty)
            {
                throw new ArgumentException("Image must not be empty.", nameof(image));
            }

            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelLSCCreate(image.NativeHandle, regionSize, ratio, out IntPtr nativeHandle));
            return new SuperpixelLSC(nativeHandle);
        }

        /// <summary>Runs segmentation iterations. 执行分割迭代。</summary>
        public void Iterate(int numIterations = 10)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelLSCIterate(NativeHandle, numIterations));
        }

        /// <summary>Writes labels into <paramref name="labels"/>. 将标签写入 <paramref name="labels"/>。</summary>
        public void GetLabels(Mat labels)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(labels, nameof(labels));
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelLSCGetLabels(NativeHandle, labels.NativeHandle));
        }

        /// <summary>Gets labels as a new matrix. 以新矩阵返回标签。</summary>
        public Mat GetLabels()
        {
            return CreateOutput(GetLabels);
        }

        /// <summary>Writes the contour mask into <paramref name="image"/>. 将轮廓掩码写入 <paramref name="image"/>。</summary>
        public void GetLabelContourMask(Mat image, bool thickLine = true)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelLSCGetLabelContourMask(NativeHandle, image.NativeHandle, thickLine ? 1 : 0));
        }

        /// <summary>Gets the contour mask as a new matrix. 以新矩阵返回轮廓掩码。</summary>
        public Mat GetLabelContourMask(bool thickLine = true)
        {
            return CreateOutput(delegate (Mat image) { GetLabelContourMask(image, thickLine); });
        }

        /// <summary>Enforces superpixel label connectivity. 强制超像素标签连通性。</summary>
        public void EnforceLabelConnectivity(int minElementSize = 25)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelLSCEnforceLabelConnectivity(NativeHandle, minElementSize));
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static Mat CreateOutput(Action<Mat> action)
        {
            var mat = new Mat();
            try
            {
                action(mat);
                return mat;
            }
            catch
            {
                mat.Dispose();
                throw;
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
