using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// DNN-based face detector compatible with OpenCV <c>cv::FaceDetectorYN</c>.
    /// 与 OpenCV <c>cv::FaceDetectorYN</c> 兼容的 DNN 人脸检测器。
    /// </summary>
    public sealed class FaceDetectorYN : IDisposable
    {
        private NativeFaceDetectorYNHandle handle;
        private bool disposed;

        private FaceDetectorYN(IntPtr nativeHandle)
        {
            handle = NativeFaceDetectorYNHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether this detector has been disposed.
        /// 获取此检测器是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the network input size.
        /// 获取或设置网络输入尺寸。
        /// </summary>
        public Size InputSize
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceDetectorYNGetInputSize(NativeHandle, out int width, out int height));
                return new Size(width, height);
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceDetectorYNSetInputSize(NativeHandle, value.Width, value.Height));
            }
        }

        /// <summary>
        /// Gets or sets the score threshold.
        /// 获取或设置分数阈值。
        /// </summary>
        public float ScoreThreshold
        {
            get { return GetFloat(NativeMethods.FaceDetectorYNGetScoreThreshold); }
            set { SetFloat(NativeMethods.FaceDetectorYNSetScoreThreshold, value); }
        }

        /// <summary>
        /// Gets or sets the non-maximum suppression threshold.
        /// 获取或设置非极大值抑制阈值。
        /// </summary>
        public float NMSThreshold
        {
            get { return GetFloat(NativeMethods.FaceDetectorYNGetNMSThreshold); }
            set { SetFloat(NativeMethods.FaceDetectorYNSetNMSThreshold, value); }
        }

        /// <summary>
        /// Gets or sets the number of bounding boxes preserved before NMS.
        /// 获取或设置 NMS 前保留的候选框数量。
        /// </summary>
        public int TopK
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceDetectorYNGetTopK(NativeHandle, out int value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.FaceDetectorYNSetTopK(NativeHandle, value));
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

        /// <summary>
        /// Creates a face detector from model and config paths.
        /// 根据模型和配置路径创建人脸检测器。
        /// </summary>
        public static FaceDetectorYN Create(
            string model,
            string config,
            Size inputSize,
            float scoreThreshold = 0.9F,
            float nmsThreshold = 0.3F,
            int topK = 5000,
            DnnBackend backendId = DnnBackend.Default,
            DnnTarget targetId = DnnTarget.Cpu)
        {
            byte[] nativeModel = ObjDetectStringConvert.ToNullTerminatedUtf8(model, nameof(model));
            byte[] nativeConfig = ObjDetectStringConvert.ToNullTerminatedUtf8(config ?? string.Empty, nameof(config));
            NativeException.ThrowIfError(NativeMethods.FaceDetectorYNCreate(
                nativeModel,
                nativeConfig,
                inputSize.Width,
                inputSize.Height,
                scoreThreshold,
                nmsThreshold,
                topK,
                (int)backendId,
                (int)targetId,
                out IntPtr nativeHandle));
            return new FaceDetectorYN(nativeHandle);
        }

        /// <summary>
        /// Creates a face detector from model and config buffers.
        /// 根据模型和配置缓冲区创建人脸检测器。
        /// </summary>
        public static unsafe FaceDetectorYN Create(
            string framework,
            byte[] modelBuffer,
            byte[]? configBuffer,
            Size inputSize,
            float scoreThreshold = 0.9F,
            float nmsThreshold = 0.3F,
            int topK = 5000,
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
                NativeException.ThrowIfError(NativeMethods.FaceDetectorYNCreateFromBuffer(
                    nativeFramework,
                    modelPtr,
                    modelBuffer.Length,
                    configPtr,
                    config.Length,
                    inputSize.Width,
                    inputSize.Height,
                    scoreThreshold,
                    nmsThreshold,
                    topK,
                    (int)backendId,
                    (int)targetId,
                    out IntPtr nativeHandle));
                return new FaceDetectorYN(nativeHandle);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Creates a face detector from span-backed model and config buffers.
        /// 根据 Span 支持的模型和配置缓冲区创建人脸检测器。
        /// </summary>
        public static unsafe FaceDetectorYN Create(
            string framework,
            ReadOnlySpan<byte> modelBuffer,
            ReadOnlySpan<byte> configBuffer,
            Size inputSize,
            float scoreThreshold = 0.9F,
            float nmsThreshold = 0.3F,
            int topK = 5000,
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
                NativeException.ThrowIfError(NativeMethods.FaceDetectorYNCreateFromBuffer(
                    nativeFramework,
                    modelPtr,
                    modelBuffer.Length,
                    configPtr,
                    configBuffer.Length,
                    inputSize.Width,
                    inputSize.Height,
                    scoreThreshold,
                    nmsThreshold,
                    topK,
                    (int)backendId,
                    (int)targetId,
                    out IntPtr nativeHandle));
                return new FaceDetectorYN(nativeHandle);
            }
        }
#endif

        /// <summary>
        /// Detects faces and writes the OpenCV result matrix.
        /// 检测人脸并写入 OpenCV 结果矩阵。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="faces">The output matrix with shape <c>N x 15</c>. 形状为 <c>N x 15</c> 的输出矩阵。</param>
        /// <returns>The OpenCV result code. OpenCV 返回码。</returns>
        public int Detect(Mat image, Mat faces)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(faces, nameof(faces));
            NativeException.ThrowIfError(NativeMethods.FaceDetectorYNDetect(NativeHandle, image.NativeHandle, faces.NativeHandle, out int result));
            return result;
        }

        /// <summary>
        /// Detects faces and returns the OpenCV result matrix.
        /// 检测人脸并返回 OpenCV 结果矩阵。
        /// </summary>
        public Mat Detect(Mat image, out int result)
        {
            var faces = new Mat();
            try
            {
                result = Detect(image, faces);
                return faces;
            }
            catch
            {
                faces.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Converts a <c>N x 15</c> face result matrix into managed face detection objects.
        /// 将 <c>N x 15</c> 人脸结果矩阵转换为 managed 人脸检测对象。
        /// </summary>
        public static FaceDetection[] ToFaceDetections(Mat faces)
        {
            ValidateNotNull(faces, nameof(faces));
            if (faces.Rows <= 0 || faces.Cols < 15)
            {
                return Array.Empty<FaceDetection>();
            }

            float[] values = ToFloatArray(faces);
            var result = new FaceDetection[faces.Rows];
            for (int i = 0; i < result.Length; i++)
            {
                int offset = i * faces.Cols;
                result[i] = new FaceDetection(
                    new Rect((int)values[offset], (int)values[offset + 1], (int)values[offset + 2], (int)values[offset + 3]),
                    new Point2f(values[offset + 4], values[offset + 5]),
                    new Point2f(values[offset + 6], values[offset + 7]),
                    new Point2f(values[offset + 8], values[offset + 9]),
                    new Point2f(values[offset + 10], values[offset + 11]),
                    new Point2f(values[offset + 12], values[offset + 13]),
                    values[offset + 14]);
            }

            return result;
        }

        /// <summary>
        /// Releases the native detector.
        /// 释放 native 检测器。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private delegate int FloatGetter(IntPtr handle, out float value);

        private delegate int FloatSetter(IntPtr handle, float value);

        private static float[] ToFloatArray(Mat mat)
        {
#if NETCOREAPP3_1_OR_GREATER
            return mat.ToArray<float>();
#else
            byte[] bytes = mat.ToBytes();
            var values = new float[bytes.Length / sizeof(float)];
            Buffer.BlockCopy(bytes, 0, values, 0, bytes.Length);
            return values;
#endif
        }

        private float GetFloat(FloatGetter getter)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(getter(NativeHandle, out float value));
            return value;
        }

        private void SetFloat(FloatSetter setter, float value)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(setter(NativeHandle, value));
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
