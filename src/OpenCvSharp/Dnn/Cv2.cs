using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Dnn
{
    /// <summary>
    /// OpenCV DNN helper functions.
    /// OpenCV DNN 辅助函数。
    /// </summary>
    public static class Cv2
    {
        /// <summary>
        /// Creates a blob from one image.
        /// 从单张图像创建 blob。
        /// </summary>
        public static void BlobFromImage(Mat image, Mat blob, double scaleFactor = 1.0, Size? size = null, Scalar? mean = null, bool swapRB = false, bool crop = false, int ddepth = MatType.CV_32F)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(blob, nameof(blob));
            Size actualSize = size ?? new Size(0, 0);
            Scalar actualMean = mean ?? new Scalar(0.0);
            NativeException.ThrowIfError(NativeMethods.DnnBlobFromImage(image.NativeHandle, blob.NativeHandle, scaleFactor, actualSize.Width, actualSize.Height, actualMean.V0, actualMean.V1, actualMean.V2, actualMean.V3, swapRB ? 1 : 0, crop ? 1 : 0, ddepth));
        }

        /// <summary>
        /// Creates and returns a blob from one image.
        /// 从单张图像创建并返回 blob。
        /// </summary>
        public static Mat BlobFromImage(Mat image, double scaleFactor = 1.0, Size? size = null, Scalar? mean = null, bool swapRB = false, bool crop = false, int ddepth = MatType.CV_32F)
        {
            var blob = new Mat();
            try
            {
                BlobFromImage(image, blob, scaleFactor, size, mean, swapRB, crop, ddepth);
                return blob;
            }
            catch
            {
                blob.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Creates a blob from multiple images.
        /// 从多张图像创建 blob。
        /// </summary>
        public static void BlobFromImages(Mat[] images, Mat blob, double scaleFactor = 1.0, Size? size = null, Scalar? mean = null, bool swapRB = false, bool crop = false, int ddepth = MatType.CV_32F)
        {
            ValidateNotNull(images, nameof(images));
            BlobFromImagesCore(images, blob, scaleFactor, size, mean, swapRB, crop, ddepth);
        }

        /// <summary>
        /// Creates and returns a blob from multiple images.
        /// 从多张图像创建并返回 blob。
        /// </summary>
        public static Mat BlobFromImages(Mat[] images, double scaleFactor = 1.0, Size? size = null, Scalar? mean = null, bool swapRB = false, bool crop = false, int ddepth = MatType.CV_32F)
        {
            var blob = new Mat();
            try
            {
                BlobFromImages(images, blob, scaleFactor, size, mean, swapRB, crop, ddepth);
                return blob;
            }
            catch
            {
                blob.Dispose();
                throw;
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Creates a blob from span-backed image handles.
        /// 从 Span 支持的图像句柄创建 blob。
        /// </summary>
        public static void BlobFromImages(ReadOnlySpan<Mat> images, Mat blob, double scaleFactor = 1.0, Size? size = null, Scalar? mean = null, bool swapRB = false, bool crop = false, int ddepth = MatType.CV_32F)
        {
            BlobFromImagesCore(images.ToArray(), blob, scaleFactor, size, mean, swapRB, crop, ddepth);
        }
#endif

        private static void BlobFromImagesCore(Mat[] images, Mat blob, double scaleFactor, Size? size, Scalar? mean, bool swapRB, bool crop, int ddepth)
        {
            ValidateNotNull(blob, nameof(blob));
            IntPtr[] handles = ToHandleArray(images, nameof(images));
            Size actualSize = size ?? new Size(0, 0);
            Scalar actualMean = mean ?? new Scalar(0.0);
            NativeException.ThrowIfError(NativeMethods.DnnBlobFromImages(handles, handles.Length, blob.NativeHandle, scaleFactor, actualSize.Width, actualSize.Height, actualMean.V0, actualMean.V1, actualMean.V2, actualMean.V3, swapRB ? 1 : 0, crop ? 1 : 0, ddepth));
        }

        /// <summary>
        /// Splits images out of a blob.
        /// 从 blob 中拆出图像。
        /// </summary>
        public static Mat[] ImagesFromBlob(Mat blob)
        {
            ValidateNotNull(blob, nameof(blob));
            NativeException.ThrowIfError(NativeMethods.DnnImagesFromBlobCount(blob.NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<Mat>();
            }

            var handles = new IntPtr[count];
            NativeException.ThrowIfError(NativeMethods.DnnImagesFromBlobFill(blob.NativeHandle, handles, handles.Length, out int written));
            int resultCount = Math.Max(0, Math.Min(written, handles.Length));
            var result = new Mat[resultCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = new Mat(handles[i]);
            }

            return result;
        }

        private static IntPtr[] ToHandleArray(Mat[] mats, string parameterName)
        {
            if (mats.Length == 0)
            {
                throw new ArgumentException("At least one Mat is required.", parameterName);
            }

            var handles = new IntPtr[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }

                handles[i] = mats[i].NativeHandle;
            }

            return handles;
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
