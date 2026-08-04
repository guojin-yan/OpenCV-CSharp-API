using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Barcode detector compatible with OpenCV <c>cv::barcode::BarcodeDetector</c>.
    /// 与 OpenCV <c>cv::barcode::BarcodeDetector</c> 兼容的条形码检测器。
    /// </summary>
    public sealed unsafe class BarcodeDetector : IDisposable
    {
        private NativeBarcodeDetectorHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes a barcode detector without a super-resolution model.
        /// 初始化不带超分辨率模型的条形码检测器。
        /// </summary>
        public BarcodeDetector()
        {
            NativeException.ThrowIfError(NativeMethods.BarcodeDetectorCreate(out IntPtr nativeHandle));
            handle = NativeBarcodeDetectorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Initializes a barcode detector with a single-file ONNX super-resolution model.
        /// 使用单文件 ONNX 超分辨率模型初始化条形码检测器。
        /// </summary>
        public BarcodeDetector(string superResolutionModelPath)
        {
            byte[] path = ObjDetectStringConvert.ToNullTerminatedUtf8(superResolutionModelPath, nameof(superResolutionModelPath));
            NativeException.ThrowIfError(NativeMethods.BarcodeDetectorCreateWithSuperResolution(path, out IntPtr nativeHandle));
            handle = NativeBarcodeDetectorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this detector has been disposed. 获取检测器是否已经释放。</summary>
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

        /// <summary>Creates a barcode detector. 创建条形码检测器。</summary>
        public static BarcodeDetector Create()
        {
            return new BarcodeDetector();
        }

        /// <summary>Creates a barcode detector with a super-resolution model. 创建带超分辨率模型的条形码检测器。</summary>
        public static BarcodeDetector Create(string superResolutionModelPath)
        {
            return new BarcodeDetector(superResolutionModelPath);
        }

        /// <summary>Gets or sets the downsampling threshold. 获取或设置下采样阈值。</summary>
        public double DownsamplingThreshold
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BarcodeDetectorGetDownsamplingThreshold(NativeHandle, out double threshold));
                return threshold;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BarcodeDetectorSetDownsamplingThreshold(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the gradient threshold. 获取或设置梯度阈值。</summary>
        public double GradientThreshold
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BarcodeDetectorGetGradientThreshold(NativeHandle, out double threshold));
                return threshold;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.BarcodeDetectorSetGradientThreshold(NativeHandle, value));
            }
        }

        /// <summary>Detects barcode quadrangle points. 检测条形码四边形顶点。</summary>
        public bool Detect(Mat image, Mat points)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            NativeException.ThrowIfError(NativeMethods.BarcodeDetectorDetect(NativeHandle, image.NativeHandle, points.NativeHandle, out int detected));
            return detected != 0;
        }

        /// <summary>Decodes barcodes from already detected points. 根据已检测顶点解码条形码。</summary>
        public BarcodeDecodeResult Decode(Mat image, Mat points)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            string[] info = GetStringArray(NativeMethods.BarcodeDetectorDecodeCount, NativeMethods.BarcodeDetectorDecodeFill, image.NativeHandle, points.NativeHandle, out bool decoded);
            return new BarcodeDecodeResult(decoded, info, Array.Empty<string>(), points);
        }

        /// <summary>Decodes barcodes and returns barcode type names. 解码条形码并返回条形码类型名称。</summary>
        public BarcodeDecodeResult DecodeWithType(Mat image, Mat points)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            StringPair pair = GetStringPair(NativeMethods.BarcodeDetectorDecodeWithTypeCount, NativeMethods.BarcodeDetectorDecodeWithTypeFill, image.NativeHandle, points.NativeHandle, out bool decoded);
            return new BarcodeDecodeResult(decoded, pair.Info, pair.Types, points);
        }

        /// <summary>Detects and decodes barcodes. 检测并解码条形码。</summary>
        public BarcodeDecodeResult DetectAndDecode(Mat image, Mat? points = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            Mat? ownedPoints = points ?? new Mat();
            try
            {
                string[] info = GetStringArray(NativeMethods.BarcodeDetectorDetectAndDecodeCount, NativeMethods.BarcodeDetectorDetectAndDecodeFill, image.NativeHandle, ownedPoints.NativeHandle, out bool decoded);
                return new BarcodeDecodeResult(decoded, info, Array.Empty<string>(), ownedPoints);
            }
            catch
            {
                if (points == null)
                {
                    ownedPoints.Dispose();
                }

                throw;
            }
        }

        /// <summary>Detects and decodes barcodes with barcode type names. 检测并解码条形码，同时返回类型名称。</summary>
        public BarcodeDecodeResult DetectAndDecodeWithType(Mat image, Mat? points = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            Mat? ownedPoints = points ?? new Mat();
            try
            {
                StringPair pair = GetStringPair(NativeMethods.BarcodeDetectorDetectAndDecodeWithTypeCount, NativeMethods.BarcodeDetectorDetectAndDecodeWithTypeFill, image.NativeHandle, ownedPoints.NativeHandle, out bool decoded);
                return new BarcodeDecodeResult(decoded, pair.Info, pair.Types, ownedPoints);
            }
            catch
            {
                if (points == null)
                {
                    ownedPoints.Dispose();
                }

                throw;
            }
        }

        /// <summary>Gets detector box-filter scales. 获取检测器 box filter 尺度。</summary>
        public float[] GetDetectorScales()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BarcodeDetectorGetDetectorScalesCount(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<float>();
            }

            var scales = new float[count];
            NativeException.ThrowIfError(NativeMethods.BarcodeDetectorGetDetectorScalesFill(NativeHandle, scales, scales.Length, out int written));
            if (written != scales.Length)
            {
                Array.Resize(ref scales, Math.Max(0, Math.Min(written, scales.Length)));
            }

            return scales;
        }

        /// <summary>Sets detector box-filter scales. 设置检测器 box filter 尺度。</summary>
        public BarcodeDetector SetDetectorScales(float[] scales)
        {
            if (scales == null)
            {
                throw new ArgumentNullException(nameof(scales));
            }

            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.BarcodeDetectorSetDetectorScales(NativeHandle, scales, scales.Length));
            return this;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Sets detector box-filter scales from a span. 从 Span 设置检测器 box filter 尺度。</summary>
        public unsafe BarcodeDetector SetDetectorScales(ReadOnlySpan<float> scales)
        {
            ThrowIfDisposed();
            fixed (float* scalesPtr = scales)
            {
                NativeException.ThrowIfError(NativeMethods.BarcodeDetectorSetDetectorScales(NativeHandle, scalesPtr, scales.Length));
            }

            return this;
        }
#endif

        /// <summary>Releases the native detector. 释放 native 检测器。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private delegate int StringArrayCountGetter(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        private unsafe delegate int StringArrayFillMethod(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        private delegate int StringPairCountGetter(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount);

        private unsafe delegate int StringPairFillMethod(IntPtr detector, IntPtr image, IntPtr points, int* infoOffsets, int infoOffsetCapacity, byte* infoBuffer, int infoBufferCapacity, int* typeOffsets, int typeOffsetCapacity, byte* typeBuffer, int typeBufferCapacity, out int decoded, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount);

        private unsafe string[] GetStringArray(StringArrayCountGetter count, StringArrayFillMethod fill, IntPtr image, IntPtr points, out bool decoded)
        {
            NativeException.ThrowIfError(count(NativeHandle, image, points, out int decodedValue, out int stringCount, out int byteCount));
            decoded = decodedValue != 0;
            if (stringCount <= 0)
            {
                return Array.Empty<string>();
            }

            var offsets = new int[stringCount + 1];
            var buffer = new byte[Math.Max(byteCount, 1)];
            fixed (int* offsetsPtr = offsets)
            fixed (byte* bufferPtr = buffer)
            {
                NativeException.ThrowIfError(fill(NativeHandle, image, points, offsetsPtr, offsets.Length, bufferPtr, buffer.Length, out decodedValue, out int writtenStringCount, out int writtenByteCount));
                decoded = decodedValue != 0;
                return DecodeStringArray(offsets, Math.Min(writtenStringCount, stringCount), buffer, Math.Min(writtenByteCount, buffer.Length));
            }
        }

        private unsafe StringPair GetStringPair(StringPairCountGetter count, StringPairFillMethod fill, IntPtr image, IntPtr points, out bool decoded)
        {
            NativeException.ThrowIfError(count(NativeHandle, image, points, out int decodedValue, out int infoCount, out int infoByteCount, out int typeCount, out int typeByteCount));
            decoded = decodedValue != 0;
            var infoOffsets = new int[Math.Max(infoCount, 0) + 1];
            var typeOffsets = new int[Math.Max(typeCount, 0) + 1];
            var infoBuffer = new byte[Math.Max(infoByteCount, 1)];
            var typeBuffer = new byte[Math.Max(typeByteCount, 1)];
            fixed (int* infoOffsetsPtr = infoOffsets)
            fixed (int* typeOffsetsPtr = typeOffsets)
            fixed (byte* infoBufferPtr = infoBuffer)
            fixed (byte* typeBufferPtr = typeBuffer)
            {
                NativeException.ThrowIfError(fill(NativeHandle, image, points, infoOffsetsPtr, infoOffsets.Length, infoBufferPtr, infoBuffer.Length, typeOffsetsPtr, typeOffsets.Length, typeBufferPtr, typeBuffer.Length, out decodedValue, out int writtenInfoCount, out int writtenInfoBytes, out int writtenTypeCount, out int writtenTypeBytes));
                decoded = decodedValue != 0;
                return new StringPair(
                    DecodeStringArray(infoOffsets, Math.Min(writtenInfoCount, Math.Max(infoCount, 0)), infoBuffer, Math.Min(writtenInfoBytes, infoBuffer.Length)),
                    DecodeStringArray(typeOffsets, Math.Min(writtenTypeCount, Math.Max(typeCount, 0)), typeBuffer, Math.Min(writtenTypeBytes, typeBuffer.Length)));
            }
        }

        private static string[] DecodeStringArray(int[] offsets, int count, byte[] buffer, int byteCount)
        {
            if (count <= 0)
            {
                return Array.Empty<string>();
            }

            var result = new string[count];
            for (int i = 0; i < count; i++)
            {
                int start = offsets[i];
                int end = offsets[i + 1];
                result[i] = start < 0 || end < start || end > byteCount
                    ? string.Empty
                    : ObjDetectStringConvert.FromUtf8Bytes(buffer, start, end - start);
            }

            return result;
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        private readonly struct StringPair
        {
            internal StringPair(string[] info, string[] types)
            {
                Info = info;
                Types = types;
            }

            internal string[] Info { get; }
            internal string[] Types { get; }
        }
    }
}
