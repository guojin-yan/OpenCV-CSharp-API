using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// ScanSegment superpixel segmenter wrapper.
    /// ScanSegment 超像素分割器包装。
    /// </summary>
    public sealed class ScanSegment : IDisposable
    {
        private NativeXImgProcScanSegmentHandle handle;
        private readonly int imageWidth;
        private readonly int imageHeight;
        private bool disposed;

        private ScanSegment(IntPtr nativeHandle, int imageWidth, int imageHeight)
        {
            handle = NativeXImgProcScanSegmentHandle.FromNativePointer(nativeHandle);
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
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
                NativeException.ThrowIfError(NativeMethods.XImgProcScanSegmentGetNumber(NativeHandle, out int value));
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

        /// <summary>Creates a ScanSegment object. 创建 ScanSegment 对象。</summary>
        public static ScanSegment Create(int imageWidth, int imageHeight, int numSuperpixels, int slices = 8, bool mergeSmall = true)
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcScanSegmentCreate(imageWidth, imageHeight, numSuperpixels, slices, mergeSmall ? 1 : 0, out IntPtr nativeHandle));
            return new ScanSegment(nativeHandle, imageWidth, imageHeight);
        }

        /// <summary>Runs the segmentation on an image in Lab color space. 对 Lab 色彩空间图像执行分割。</summary>
        public void Iterate(Mat image)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(image, nameof(image));
            ValidateIterateImage(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.XImgProcScanSegmentIterate(NativeHandle, image.NativeHandle));
        }

        /// <summary>Writes labels into <paramref name="labels"/>. 将标签写入 <paramref name="labels"/>。</summary>
        public void GetLabels(Mat labels)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(labels, nameof(labels));
            NativeException.ThrowIfError(NativeMethods.XImgProcScanSegmentGetLabels(NativeHandle, labels.NativeHandle));
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
            NativeException.ThrowIfError(NativeMethods.XImgProcScanSegmentGetLabelContourMask(NativeHandle, image.NativeHandle, thickLine ? 1 : 0));
        }

        /// <summary>Gets the contour mask as a new matrix. 以新矩阵返回轮廓掩码。</summary>
        public Mat GetLabelContourMask(bool thickLine = true)
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
                throw new ArgumentException("Source image size must match the dimensions used to create the ScanSegment instance.", parameterName);
            }

            if (MatType.Depth(image.Type) != MatType.CV_8U)
            {
                throw new ArgumentException("Source image depth must be CV_8U.", parameterName);
            }

            if (image.Channels != 3)
            {
                throw new ArgumentException("Source image must have 3 channels.", parameterName);
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
