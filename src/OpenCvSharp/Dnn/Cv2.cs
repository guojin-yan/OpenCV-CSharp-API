using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Dnn
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
            ValidateBlobDepth(ddepth, nameof(ddepth));
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
            ValidateBlobDepth(ddepth, nameof(ddepth));
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
            if (count < 0) throw new OpenCvException("Native DNN image count is negative.");
            if (count == 0)
            {
                return Array.Empty<Mat>();
            }

            var handles = new IntPtr[count];
            NativeException.ThrowIfError(NativeMethods.DnnImagesFromBlobFill(blob.NativeHandle, handles, handles.Length, out int written));
            if (written != count)
            {
                ReleaseMatHandles(handles);
                throw new OpenCvException("Native DNN image count changed during retrieval.");
            }

            var result = new Mat[count];
            int created = 0;
            try
            {
                for (; created < result.Length; created++)
                {
                    if (handles[created] == IntPtr.Zero) throw new OpenCvException("Native DNN image handle is null.");
                    result[created] = new Mat(handles[created]);
                    handles[created] = IntPtr.Zero;
                }
                return result;
            }
            catch
            {
                for (int i = 0; i < created; i++) result[i]?.Dispose();
                throw;
            }
            finally
            {
                ReleaseMatHandles(handles);
            }
        }

        /// <summary>Gets runtime targets available for the specified backend.</summary>
        public static DnnTarget[] GetAvailableTargets(DnnBackend backend)
        {
            ValidateBackend(backend, nameof(backend));
            NativeException.ThrowIfError(NativeMethods.DnnGetAvailableTargetsCount((int)backend, out int count));
            if (count < 0) throw new OpenCvException("Native DNN target count is negative.");
            if (count == 0) return Array.Empty<DnnTarget>();
            var values = new int[count];
            NativeException.ThrowIfError(NativeMethods.DnnGetAvailableTargetsFill((int)backend, values, values.Length, out int written));
            if (written != count) throw new OpenCvException("Native DNN target count changed during retrieval.");
            var result = new DnnTarget[written];
            for (int i = 0; i < result.Length; i++)
            {
                if (values[i] < (int)DnnTarget.Cpu || values[i] > (int)DnnTarget.CpuFp16)
                    throw new OpenCvException("Native DNN target value is invalid.");
                result[i] = (DnnTarget)values[i];
            }
            return result;
        }

        /// <summary>Reads a tensor serialized in an ONNX TensorProto file into an existing Mat.</summary>
        /// <param name="path">UTF-8 TensorProto path. Embedded null characters are rejected.</param>
        /// <param name="output">Caller-owned matrix whose header is replaced with the decoded tensor.</param>
        public static void ReadTensorFromOnnx(string path, Mat output)
        {
            ValidateNotNull(output, nameof(output));
            byte[] nativePath = DnnStringConvert.ToNullTerminatedUtf8(path, nameof(path));
            NativeException.ThrowIfError(NativeMethods.DnnReadTensorFromOnnx(nativePath, output.NativeHandle));
        }

        /// <summary>Reads and returns a tensor serialized in an ONNX TensorProto file.</summary>
        /// <param name="path">UTF-8 TensorProto path. Embedded null characters are rejected.</param>
        /// <returns>An independently disposable tensor matrix.</returns>
        public static Mat ReadTensorFromOnnx(string path)
        {
            var output = new Mat();
            try
            {
                ReadTensorFromOnnx(path, output);
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }

        /// <summary>Creates a blob from one image using structured preprocessing parameters.</summary>
        /// <param name="image">Caller-owned source image; ROI and non-contiguous matrices are accepted by OpenCV.</param>
        /// <param name="blob">Caller-owned output matrix whose header is replaced with the result.</param>
        /// <param name="parameters">Shape, depth, layout, scale, mean, channel-swap, and padding settings.</param>
        public static void BlobFromImage(Mat image, Mat blob, Image2BlobParams parameters)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(blob, nameof(blob));
            ValidateNotNull(parameters, nameof(parameters));
            NativeDnnImage2BlobParams native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.DnnBlobFromImageWithParams(image.NativeHandle, blob.NativeHandle, in native));
        }

        /// <summary>Creates and returns a blob from one image using structured preprocessing parameters.</summary>
        /// <param name="image">Caller-owned source image.</param>
        /// <param name="parameters">Shape, depth, layout, scale, mean, channel-swap, and padding settings.</param>
        /// <returns>An independently disposable NCHW or NHWC blob matrix.</returns>
        public static Mat BlobFromImage(Mat image, Image2BlobParams parameters)
        {
            var blob = new Mat();
            try
            {
                BlobFromImage(image, blob, parameters);
                return blob;
            }
            catch
            {
                blob.Dispose();
                throw;
            }
        }

        /// <summary>Creates a blob from images using structured preprocessing parameters.</summary>
        /// <param name="images">Non-empty caller-owned source images in batch order.</param>
        /// <param name="blob">Caller-owned output matrix whose header is replaced with the result.</param>
        /// <param name="parameters">Shape, depth, layout, scale, mean, channel-swap, and padding settings.</param>
        public static void BlobFromImages(Mat[] images, Mat blob, Image2BlobParams parameters)
        {
            ValidateNotNull(images, nameof(images));
            ValidateNotNull(blob, nameof(blob));
            ValidateNotNull(parameters, nameof(parameters));
            IntPtr[] handles = ToHandleArray(images, nameof(images));
            NativeDnnImage2BlobParams native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.DnnBlobFromImagesWithParams(handles, handles.Length, blob.NativeHandle, in native));
        }

        /// <summary>Creates and returns a blob from images using structured preprocessing parameters.</summary>
        /// <param name="images">Non-empty caller-owned source images in batch order.</param>
        /// <param name="parameters">Shape, depth, layout, scale, mean, channel-swap, and padding settings.</param>
        /// <returns>An independently disposable NCHW or NHWC batch blob.</returns>
        public static Mat BlobFromImages(Mat[] images, Image2BlobParams parameters)
        {
            var blob = new Mat();
            try
            {
                BlobFromImages(images, blob, parameters);
                return blob;
            }
            catch
            {
                blob.Dispose();
                throw;
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Creates a blob from span-backed images using structured preprocessing parameters.</summary>
        /// <param name="images">Non-empty caller-owned source images in batch order.</param>
        /// <param name="blob">Caller-owned output matrix whose header is replaced with the result.</param>
        /// <param name="parameters">Shape, depth, layout, scale, mean, channel-swap, and padding settings.</param>
        public static void BlobFromImages(ReadOnlySpan<Mat> images, Mat blob, Image2BlobParams parameters)
        {
            BlobFromImages(images.ToArray(), blob, parameters);
        }
#endif

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

        private static void ValidateBlobDepth(int value, string parameterName)
        {
            if (value != MatType.CV_32F && value != MatType.CV_8U) throw new ArgumentOutOfRangeException(parameterName);
        }

        private static void ValidateBackend(DnnBackend value, string parameterName)
        {
            if (value != DnnBackend.Default && value != DnnBackend.InferenceEngine && value != DnnBackend.OpenCV &&
                value != DnnBackend.VkCom && value != DnnBackend.Cuda && value != DnnBackend.WebNN &&
                value != DnnBackend.TimVx && value != DnnBackend.Cann)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }

        private static void ReleaseMatHandles(IntPtr[] handles)
        {
            for (int i = 0; i < handles.Length; i++)
            {
                if (handles[i] == IntPtr.Zero) continue;
                NativeMethods.MatRelease(handles[i]);
                handles[i] = IntPtr.Zero;
            }
        }
    }
}
