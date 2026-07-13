using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ObjDetect
{
    /// <summary>
    /// QR code detector compatible with OpenCV <c>cv::QRCodeDetector</c>.
    /// 与 OpenCV <c>cv::QRCodeDetector</c> 兼容的二维码检测器。
    /// </summary>
    public sealed class QRCodeDetector : IDisposable
    {
        private NativeQRCodeDetectorHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes a QR code detector.
        /// 初始化二维码检测器。
        /// </summary>
        public QRCodeDetector()
        {
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorCreate(out IntPtr nativeHandle));
            handle = NativeQRCodeDetectorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether this detector has been disposed.
        /// 获取此检测器是否已经释放。
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
        /// Creates a QR code detector.
        /// 创建二维码检测器。
        /// </summary>
        /// <returns>The created detector. 创建的检测器。</returns>
        public static QRCodeDetector Create()
        {
            return new QRCodeDetector();
        }

        /// <summary>
        /// Sets horizontal scan epsilon used for QR stop-marker detection.
        /// 设置二维码停止标记检测水平扫描使用的 epsilon。
        /// </summary>
        /// <param name="epsX">The horizontal epsilon. 水平 epsilon。</param>
        /// <returns>This detector. 当前检测器。</returns>
        public QRCodeDetector SetEpsX(double epsX)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorSetEpsX(NativeHandle, epsX));
            return this;
        }

        /// <summary>
        /// Sets vertical scan epsilon used for QR stop-marker detection.
        /// 设置二维码停止标记检测垂直扫描使用的 epsilon。
        /// </summary>
        /// <param name="epsY">The vertical epsilon. 垂直 epsilon。</param>
        /// <returns>This detector. 当前检测器。</returns>
        public QRCodeDetector SetEpsY(double epsY)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorSetEpsY(NativeHandle, epsY));
            return this;
        }

        /// <summary>
        /// Enables or disables alignment marker usage for corner refinement.
        /// 启用或禁用用于角点细化的 alignment marker。
        /// </summary>
        /// <param name="useAlignmentMarkers">Whether to use alignment markers. 是否使用 alignment marker。</param>
        /// <returns>This detector. 当前检测器。</returns>
        public QRCodeDetector SetUseAlignmentMarkers(bool useAlignmentMarkers)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorSetUseAlignmentMarkers(NativeHandle, useAlignmentMarkers ? 1 : 0));
            return this;
        }

        /// <summary>
        /// Detects a QR code and writes the quadrangle points into a matrix.
        /// 检测二维码，并将四边形顶点写入矩阵。
        /// </summary>
        /// <param name="image">The source image. 源图像。</param>
        /// <param name="points">The output points matrix. 输出顶点矩阵。</param>
        /// <returns><c>true</c> when a QR code was detected. 检测到二维码时返回 <c>true</c>。</returns>
        public bool Detect(Mat image, Mat points)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorDetect(NativeHandle, image.NativeHandle, points.NativeHandle, out int detected));
            return detected != 0;
        }

        /// <summary>
        /// Detects a QR code and returns the quadrangle points matrix.
        /// 检测二维码并返回四边形顶点矩阵。
        /// </summary>
        /// <returns>The detected points matrix. 检测到的顶点矩阵。</returns>
        public Mat Detect(Mat image, out bool detected)
        {
            var points = new Mat();
            try
            {
                detected = Detect(image, points);
                return points;
            }
            catch
            {
                points.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Decodes a QR code from already detected quadrangle points.
        /// 根据已检测到的四边形顶点解码二维码。
        /// </summary>
        public string Decode(Mat image, Mat points, Mat? straightQRCode = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            unsafe
            {
                return GetString(
                    NativeMethods.QRCodeDetectorDecodeLength,
                    NativeMethods.QRCodeDetectorDecodeFill,
                    image.NativeHandle,
                    points.NativeHandle,
                    OptionalHandle(straightQRCode));
            }
        }

        /// <summary>
        /// Detects and decodes a QR code.
        /// 检测并解码二维码。
        /// </summary>
        public string DetectAndDecode(Mat image, Mat? points = null, Mat? straightQRCode = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            unsafe
            {
                return GetString(
                    NativeMethods.QRCodeDetectorDetectAndDecodeLength,
                    NativeMethods.QRCodeDetectorDetectAndDecodeFill,
                    image.NativeHandle,
                    OptionalHandle(points),
                    OptionalHandle(straightQRCode));
            }
        }

        /// <summary>
        /// Decodes a QR code on a curved surface from already detected points.
        /// 根据已检测到的顶点解码曲面上的二维码。
        /// </summary>
        public string DecodeCurved(Mat image, Mat points, Mat? straightQRCode = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            unsafe
            {
                return GetString(
                    NativeMethods.QRCodeDetectorDecodeCurvedLength,
                    NativeMethods.QRCodeDetectorDecodeCurvedFill,
                    image.NativeHandle,
                    points.NativeHandle,
                    OptionalHandle(straightQRCode));
            }
        }

        /// <summary>
        /// Detects and decodes a QR code on a curved surface.
        /// 检测并解码曲面上的二维码。
        /// </summary>
        public string DetectAndDecodeCurved(Mat image, Mat? points = null, Mat? straightQRCode = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            unsafe
            {
                return GetString(
                    NativeMethods.QRCodeDetectorDetectAndDecodeCurvedLength,
                    NativeMethods.QRCodeDetectorDetectAndDecodeCurvedFill,
                    image.NativeHandle,
                    OptionalHandle(points),
                    OptionalHandle(straightQRCode));
            }
        }

        /// <summary>
        /// Detects multiple QR codes and writes their quadrangle points into a matrix.
        /// 检测多个二维码，并将四边形顶点写入矩阵。
        /// </summary>
        public bool DetectMulti(Mat image, Mat points)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorDetectMulti(NativeHandle, image.NativeHandle, points.NativeHandle, out int detected));
            return detected != 0;
        }

        /// <summary>
        /// Detects multiple QR codes and returns the quadrangle points matrix.
        /// 检测多个二维码并返回四边形顶点矩阵。
        /// </summary>
        public Mat DetectMulti(Mat image, out bool detected)
        {
            var points = new Mat();
            try
            {
                detected = DetectMulti(image, points);
                return points;
            }
            catch
            {
                points.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Decodes multiple QR codes from already detected points.
        /// 根据已检测到的顶点解码多个二维码。
        /// </summary>
        public unsafe QRCodeMultiDecodeResult DecodeMulti(Mat image, Mat points)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            string[] decodedInfo = GetStringArray(
                NativeMethods.QRCodeDetectorDecodeMultiCount,
                NativeMethods.QRCodeDetectorDecodeMultiFill,
                image.NativeHandle,
                points.NativeHandle,
                out bool decoded);
            return new QRCodeMultiDecodeResult(decoded, decodedInfo, points);
        }

        /// <summary>
        /// Detects and decodes multiple QR codes.
        /// 检测并解码多个二维码。
        /// </summary>
        public unsafe QRCodeMultiDecodeResult DetectAndDecodeMulti(Mat image, Mat? points = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            Mat? ownedPoints = points;
            if (ownedPoints == null)
            {
                ownedPoints = new Mat();
            }

            try
            {
                string[] decodedInfo = GetStringArray(
                    NativeMethods.QRCodeDetectorDetectAndDecodeMultiCount,
                    NativeMethods.QRCodeDetectorDetectAndDecodeMultiFill,
                    image.NativeHandle,
                    ownedPoints.NativeHandle,
                    out bool decoded);
                return new QRCodeMultiDecodeResult(decoded, decodedInfo, ownedPoints);
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

        /// <summary>
        /// Gets the ECI encoding reported by the latest decode call.
        /// 获取最近一次解码调用报告的 ECI 编码。
        /// </summary>
        /// <param name="codeIndex">The decoded QR code index. 解码二维码索引。</param>
        /// <returns>The ECI encoding. ECI 编码。</returns>
        public QRCodeEncoderECIEncodings GetEncoding(int codeIndex = 0)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorGetEncoding(NativeHandle, codeIndex, out int encoding));
            return (QRCodeEncoderECIEncodings)encoding;
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

        private delegate int StringLengthGetter(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, out int length);

        private unsafe delegate int StringFillMethod(IntPtr detector, IntPtr image, IntPtr points, IntPtr straightQRCode, byte* buffer, int bufferCapacity, out int written);

        private delegate int StringArrayCountGetter(IntPtr detector, IntPtr image, IntPtr points, out int decoded, out int stringCount, out int byteCount);

        private unsafe delegate int StringArrayFillMethod(IntPtr detector, IntPtr image, IntPtr points, int* offsets, int offsetCapacity, byte* buffer, int bufferCapacity, out int decoded, out int stringCount, out int byteCount);

        private unsafe string GetString(StringLengthGetter getLength, StringFillMethod fill, IntPtr image, IntPtr points, IntPtr straightQRCode)
        {
            NativeException.ThrowIfError(getLength(NativeHandle, image, points, straightQRCode, out int length));
            if (length <= 0)
            {
                return string.Empty;
            }

            var buffer = new byte[length];
            fixed (byte* bufferPtr = buffer)
            {
                NativeException.ThrowIfError(fill(NativeHandle, image, points, straightQRCode, bufferPtr, buffer.Length, out int written));
                return ObjDetectStringConvert.FromUtf8Bytes(buffer, 0, Math.Min(written, buffer.Length));
            }
        }

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
                NativeException.ThrowIfError(fill(
                    NativeHandle,
                    image,
                    points,
                    offsetsPtr,
                    offsets.Length,
                    bufferPtr,
                    buffer.Length,
                    out decodedValue,
                    out int writtenStringCount,
                    out int writtenByteCount));
                decoded = decodedValue != 0;
                return DecodeStringArray(offsets, Math.Min(writtenStringCount, stringCount), buffer, Math.Min(writtenByteCount, buffer.Length));
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
                if (start < 0 || end < start || end > byteCount)
                {
                    result[i] = string.Empty;
                }
                else
                {
                    result[i] = ObjDetectStringConvert.FromUtf8Bytes(buffer, start, end - start);
                }
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

        private static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
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
    }
}
