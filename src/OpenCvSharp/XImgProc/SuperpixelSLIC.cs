using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// SLIC superpixel segmentation wrapper.
    /// SLIC 超像素分割包装。
    /// </summary>
    public sealed class SuperpixelSLIC : IDisposable
    {
        private NativeXImgProcSuperpixelSLICHandle handle;
        private bool disposed;

        private SuperpixelSLIC(IntPtr nativeHandle)
        {
            handle = NativeXImgProcSuperpixelSLICHandle.FromNativePointer(nativeHandle);
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
                NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSLICGetNumber(NativeHandle, out int value));
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

        /// <summary>Creates a SLIC superpixel segmenter. 创建 SLIC 超像素分割器。</summary>
        public static SuperpixelSLIC Create(Mat image, SLICType algorithm = SLICType.SLICO, int regionSize = 10, float ruler = 10.0F)
        {
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            XImgProcCv2.ValidateSLICType(algorithm, nameof(algorithm));
            ValidateCreateImage(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSLICCreate(image.NativeHandle, (int)algorithm, regionSize, ruler, out IntPtr nativeHandle));
            return new SuperpixelSLIC(nativeHandle);
        }

        /// <summary>Runs segmentation iterations. 执行分割迭代。</summary>
        public void Iterate(int numIterations = 10)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSLICIterate(NativeHandle, numIterations));
        }

        /// <summary>Writes labels into <paramref name="labels"/>. 将标签写入 <paramref name="labels"/>。</summary>
        public void GetLabels(Mat labels)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(labels, nameof(labels));
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSLICGetLabels(NativeHandle, labels.NativeHandle));
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
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSLICGetLabelContourMask(NativeHandle, image.NativeHandle, thickLine ? 1 : 0));
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
            ValidateMinElementSize(minElementSize, nameof(minElementSize));
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSLICEnforceLabelConnectivity(NativeHandle, minElementSize));
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

        private static void ValidateCreateImage(Mat image, string parameterName)
        {
            if (image.Empty)
            {
                throw new ArgumentException("Source image must not be empty.", parameterName);
            }
        }

        private static void ValidateMinElementSize(int value, string parameterName)
        {
            if (value < 0 || value > 100)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Minimum element size must be between 0 and 100.");
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
