using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// SEEDS superpixel segmentation wrapper.
    /// SEEDS 超像素分割包装。
    /// </summary>
    public sealed class SuperpixelSEEDS : IDisposable
    {
        private NativeXImgProcSuperpixelSEEDSHandle handle;
        private readonly int imageWidth;
        private readonly int imageHeight;
        private readonly int imageChannels;
        private bool disposed;

        private SuperpixelSEEDS(IntPtr nativeHandle, int imageWidth, int imageHeight, int imageChannels)
        {
            handle = NativeXImgProcSuperpixelSEEDSHandle.FromNativePointer(nativeHandle);
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.imageChannels = imageChannels;
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
                NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSEEDSGetNumber(NativeHandle, out int value));
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

        /// <summary>Creates a SEEDS superpixel segmenter. 创建 SEEDS 超像素分割器。</summary>
        public static SuperpixelSEEDS Create(int imageWidth, int imageHeight, int imageChannels, int numSuperpixels, int numLevels, int prior = 2, int histogramBins = 5, bool doubleStep = false)
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSEEDSCreate(imageWidth, imageHeight, imageChannels, numSuperpixels, numLevels, prior, histogramBins, doubleStep ? 1 : 0, out IntPtr nativeHandle));
            return new SuperpixelSEEDS(nativeHandle, imageWidth, imageHeight, imageChannels);
        }

        /// <summary>Runs segmentation iterations for the supplied image. 对给定图像执行分割迭代。</summary>
        public void Iterate(Mat image, int numIterations = 4)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            ValidateIterateImage(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSEEDSIterate(NativeHandle, image.NativeHandle, numIterations));
        }

        /// <summary>Writes labels into <paramref name="labels"/>. 将标签写入 <paramref name="labels"/>。</summary>
        public void GetLabels(Mat labels)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(labels, nameof(labels));
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSEEDSGetLabels(NativeHandle, labels.NativeHandle));
        }

        /// <summary>Gets labels as a new matrix. 以新矩阵返回标签。</summary>
        public Mat GetLabels()
        {
            return CreateOutput(GetLabels);
        }

        /// <summary>Writes the contour mask into <paramref name="image"/>. 将轮廓掩码写入 <paramref name="image"/>。</summary>
        public void GetLabelContourMask(Mat image, bool thickLine = false)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.XImgProcSuperpixelSEEDSGetLabelContourMask(NativeHandle, image.NativeHandle, thickLine ? 1 : 0));
        }

        /// <summary>Gets the contour mask as a new matrix. 以新矩阵返回轮廓掩码。</summary>
        public Mat GetLabelContourMask(bool thickLine = false)
        {
            return CreateOutput(delegate (Mat image) { GetLabelContourMask(image, thickLine); });
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

        private void ValidateIterateImage(Mat image, string parameterName)
        {
            if (image.Empty)
            {
                throw new ArgumentException("Source image must not be empty.", parameterName);
            }

            if (image.Cols != imageWidth || image.Rows != imageHeight)
            {
                throw new ArgumentException("Source image size must match the dimensions used to create the SuperpixelSEEDS instance.", parameterName);
            }

            int depth = MatType.Depth(image.Type);
            if (depth != MatType.CV_8U && depth != MatType.CV_16U && depth != MatType.CV_32F)
            {
                throw new ArgumentException("Source image depth must be CV_8U, CV_16U, or CV_32F.", parameterName);
            }

            if (image.Channels != imageChannels)
            {
                throw new ArgumentException("Source image channel count must match the channel count used to create the SuperpixelSEEDS instance.", parameterName);
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
