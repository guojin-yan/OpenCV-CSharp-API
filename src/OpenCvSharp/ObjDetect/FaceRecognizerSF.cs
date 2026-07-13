using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// DNN-based face recognizer compatible with OpenCV <c>cv::FaceRecognizerSF</c>.
    /// 与 OpenCV <c>cv::FaceRecognizerSF</c> 兼容的 DNN 人脸识别器。
    /// </summary>
    public sealed class FaceRecognizerSF : IDisposable
    {
        private NativeFaceRecognizerSFHandle handle;
        private bool disposed;

        private FaceRecognizerSF(IntPtr nativeHandle)
        {
            handle = NativeFaceRecognizerSFHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether this recognizer has been disposed.
        /// 获取此识别器是否已经释放。
        /// </summary>
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

        /// <summary>
        /// Creates a face recognizer from model and config paths.
        /// 根据模型和配置路径创建人脸识别器。
        /// </summary>
        public static FaceRecognizerSF Create(
            string model,
            string config,
            DnnBackend backendId = DnnBackend.Default,
            DnnTarget targetId = DnnTarget.Cpu)
        {
            byte[] nativeModel = ObjDetectStringConvert.ToNullTerminatedUtf8(model, nameof(model));
            byte[] nativeConfig = ObjDetectStringConvert.ToNullTerminatedUtf8(config ?? string.Empty, nameof(config));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerSFCreate(nativeModel, nativeConfig, (int)backendId, (int)targetId, out IntPtr nativeHandle));
            return new FaceRecognizerSF(nativeHandle);
        }

        /// <summary>
        /// Creates a face recognizer from model and config buffers.
        /// 根据模型和配置缓冲区创建人脸识别器。
        /// </summary>
        public static unsafe FaceRecognizerSF Create(
            string framework,
            byte[] modelBuffer,
            byte[]? configBuffer,
            DnnBackend backendId = DnnBackend.Default,
            DnnTarget targetId = DnnTarget.Cpu)
        {
            ValidateNotNull(modelBuffer, nameof(modelBuffer));
            byte[] nativeFramework = ObjDetectStringConvert.ToNullTerminatedUtf8(framework, nameof(framework));
            ValidateNotEmpty(modelBuffer, nameof(modelBuffer));
            byte[] emptyConfig = Array.Empty<byte>();
            byte[] config = configBuffer ?? emptyConfig;
            fixed (byte* modelPtr = modelBuffer)
            fixed (byte* configPtr = config)
            {
                NativeException.ThrowIfError(NativeMethods.FaceRecognizerSFCreateFromBuffer(
                    nativeFramework,
                    modelPtr,
                    modelBuffer.Length,
                    configPtr,
                    config.Length,
                    (int)backendId,
                    (int)targetId,
                    out IntPtr nativeHandle));
                return new FaceRecognizerSF(nativeHandle);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Creates a face recognizer from span-backed model and config buffers.
        /// 根据 Span 支持的模型和配置缓冲区创建人脸识别器。
        /// </summary>
        public static unsafe FaceRecognizerSF Create(
            string framework,
            ReadOnlySpan<byte> modelBuffer,
            ReadOnlySpan<byte> configBuffer,
            DnnBackend backendId = DnnBackend.Default,
            DnnTarget targetId = DnnTarget.Cpu)
        {
            byte[] nativeFramework = ObjDetectStringConvert.ToNullTerminatedUtf8(framework, nameof(framework));
            if (modelBuffer.Length == 0)
            {
                throw new ArgumentException("Model buffer cannot be empty.", nameof(modelBuffer));
            }

            fixed (byte* modelPtr = modelBuffer)
            fixed (byte* configPtr = configBuffer)
            {
                NativeException.ThrowIfError(NativeMethods.FaceRecognizerSFCreateFromBuffer(
                    nativeFramework,
                    modelPtr,
                    modelBuffer.Length,
                    configPtr,
                    configBuffer.Length,
                    (int)backendId,
                    (int)targetId,
                    out IntPtr nativeHandle));
                return new FaceRecognizerSF(nativeHandle);
            }
        }
#endif

        /// <summary>
        /// Aligns and crops a detected face.
        /// 对检测到的人脸进行对齐和裁剪。
        /// </summary>
        public void AlignCrop(Mat sourceImage, Mat faceBox, Mat alignedImage)
        {
            ThrowIfDisposed();
            ValidateNotNull(sourceImage, nameof(sourceImage));
            ValidateNotNull(faceBox, nameof(faceBox));
            ValidateNotNull(alignedImage, nameof(alignedImage));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerSFAlignCrop(NativeHandle, sourceImage.NativeHandle, faceBox.NativeHandle, alignedImage.NativeHandle));
        }

        /// <summary>
        /// Aligns, crops, and returns a face image.
        /// 对人脸进行对齐、裁剪并返回结果图像。
        /// </summary>
        public Mat AlignCrop(Mat sourceImage, Mat faceBox)
        {
            var alignedImage = new Mat();
            try
            {
                AlignCrop(sourceImage, faceBox, alignedImage);
                return alignedImage;
            }
            catch
            {
                alignedImage.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Extracts a face feature from an aligned image.
        /// 从已对齐图像提取人脸特征。
        /// </summary>
        public void Feature(Mat alignedImage, Mat faceFeature)
        {
            ThrowIfDisposed();
            ValidateNotNull(alignedImage, nameof(alignedImage));
            ValidateNotNull(faceFeature, nameof(faceFeature));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerSFFeature(NativeHandle, alignedImage.NativeHandle, faceFeature.NativeHandle));
        }

        /// <summary>
        /// Extracts and returns a face feature from an aligned image.
        /// 从已对齐图像提取并返回人脸特征。
        /// </summary>
        public Mat Feature(Mat alignedImage)
        {
            var faceFeature = new Mat();
            try
            {
                Feature(alignedImage, faceFeature);
                return faceFeature;
            }
            catch
            {
                faceFeature.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Calculates the distance between two face features.
        /// 计算两个人脸特征之间的距离。
        /// </summary>
        public double Match(Mat faceFeature1, Mat faceFeature2, FaceRecognizerSFDistanceType distanceType = FaceRecognizerSFDistanceType.Cosine)
        {
            ThrowIfDisposed();
            ValidateNotNull(faceFeature1, nameof(faceFeature1));
            ValidateNotNull(faceFeature2, nameof(faceFeature2));
            NativeException.ThrowIfError(NativeMethods.FaceRecognizerSFMatch(NativeHandle, faceFeature1.NativeHandle, faceFeature2.NativeHandle, (int)distanceType, out double result));
            return result;
        }

        /// <summary>
        /// Releases the native recognizer.
        /// 释放 native 识别器。
        /// </summary>
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

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private static void ValidateNotEmpty(byte[] value, string parameterName)
        {
            if (value.Length == 0)
            {
                throw new ArgumentException("Model buffer cannot be empty.", parameterName);
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
