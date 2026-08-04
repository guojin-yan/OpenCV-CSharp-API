using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// QR code detector based on OpenCV's ArUco marker detection code.
    /// 基于 OpenCV ArUco 标记检测代码的二维码检测器。
    /// </summary>
    public sealed unsafe class QRCodeDetectorAruco : IDisposable
    {
        private NativeQRCodeDetectorArucoHandle handle;
        private bool disposed;

        /// <summary>Initializes a detector with OpenCV defaults. 使用 OpenCV 默认参数初始化检测器。</summary>
        public QRCodeDetectorAruco()
        {
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorArucoCreate(out IntPtr nativeHandle));
            handle = NativeQRCodeDetectorArucoHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Initializes a detector with explicit parameters. 使用显式参数初始化检测器。</summary>
        public QRCodeDetectorAruco(QRCodeDetectorArucoParams parameters)
        {
            NativeMethods.QRCodeDetectorArucoParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorArucoCreateWithParams(ref native, out IntPtr nativeHandle));
            handle = NativeQRCodeDetectorArucoHandle.FromNativePointer(nativeHandle);
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

        /// <summary>Creates an ArUco QR detector. 创建 ArUco 二维码检测器。</summary>
        public static QRCodeDetectorAruco Create()
        {
            return new QRCodeDetectorAruco();
        }

        /// <summary>Creates an ArUco QR detector with parameters. 使用参数创建 ArUco 二维码检测器。</summary>
        public static QRCodeDetectorAruco Create(QRCodeDetectorArucoParams parameters)
        {
            return new QRCodeDetectorAruco(parameters);
        }

        /// <summary>Gets detector parameters. 获取检测器参数。</summary>
        public QRCodeDetectorArucoParams GetDetectorParameters()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorArucoGetDetectorParameters(NativeHandle, out NativeMethods.QRCodeDetectorArucoParamsNative native));
            return QRCodeDetectorArucoParams.FromNative(native);
        }

        /// <summary>Sets detector parameters. 设置检测器参数。</summary>
        public QRCodeDetectorAruco SetDetectorParameters(QRCodeDetectorArucoParams parameters)
        {
            ThrowIfDisposed();
            NativeMethods.QRCodeDetectorArucoParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorArucoSetDetectorParameters(NativeHandle, ref native));
            return this;
        }

        /// <summary>Gets the ArUco marker detector parameters used by the QR detector.</summary>
        public ArucoDetectorParameters GetArucoParameters()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorArucoGetArucoParameters(NativeHandle, out NativeMethods.ArucoDetectorParamsNative parameters));
            return ArucoDetectorParameters.FromNative(parameters);
        }

        /// <summary>Sets the ArUco marker detector parameters used by the QR detector.</summary>
        public QRCodeDetectorAruco SetArucoParameters(ArucoDetectorParameters parameters)
        {
            ThrowIfDisposed();
            if (parameters == null) throw new ArgumentNullException(nameof(parameters));
            NativeMethods.ArucoDetectorParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorArucoSetArucoParameters(NativeHandle, ref native));
            return this;
        }

        /// <summary>Detects a QR code and writes quadrangle points into a matrix. 检测二维码并将四边形顶点写入矩阵。</summary>
        public bool Detect(Mat image, Mat points)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorArucoDetect(NativeHandle, image.NativeHandle, points.NativeHandle, out int detected));
            return detected != 0;
        }

        /// <summary>Decodes a QR code from detected points. 根据检测到的顶点解码二维码。</summary>
        public string Decode(Mat image, Mat points, Mat? straightQRCode = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            return GetString(NativeMethods.QRCodeDetectorArucoDecodeLength, NativeMethods.QRCodeDetectorArucoDecodeFill, image.NativeHandle, points.NativeHandle, OptionalHandle(straightQRCode));
        }

        /// <summary>Detects and decodes a QR code. 检测并解码二维码。</summary>
        public string DetectAndDecode(Mat image, Mat? points = null, Mat? straightQRCode = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            return GetString(NativeMethods.QRCodeDetectorArucoDetectAndDecodeLength, NativeMethods.QRCodeDetectorArucoDetectAndDecodeFill, image.NativeHandle, OptionalHandle(points), OptionalHandle(straightQRCode));
        }

        /// <summary>Detects multiple QR codes. 检测多个二维码。</summary>
        public bool DetectMulti(Mat image, Mat points)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            NativeException.ThrowIfError(NativeMethods.QRCodeDetectorArucoDetectMulti(NativeHandle, image.NativeHandle, points.NativeHandle, out int detected));
            return detected != 0;
        }

        /// <summary>Decodes multiple QR codes from detected points. 根据检测到的顶点解码多个二维码。</summary>
        public QRCodeMultiDecodeResult DecodeMulti(Mat image, Mat points)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(points, nameof(points));
            string[] decodedInfo = GetStringArray(NativeMethods.QRCodeDetectorArucoDecodeMultiCount, NativeMethods.QRCodeDetectorArucoDecodeMultiFill, image.NativeHandle, points.NativeHandle, out bool decoded);
            return new QRCodeMultiDecodeResult(decoded, decodedInfo, points);
        }

        /// <summary>Detects and decodes multiple QR codes. 检测并解码多个二维码。</summary>
        public QRCodeMultiDecodeResult DetectAndDecodeMulti(Mat image, Mat? points = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            Mat? ownedPoints = points ?? new Mat();
            try
            {
                string[] decodedInfo = GetStringArray(NativeMethods.QRCodeDetectorArucoDetectAndDecodeMultiCount, NativeMethods.QRCodeDetectorArucoDetectAndDecodeMultiFill, image.NativeHandle, ownedPoints.NativeHandle, out bool decoded);
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

        /// <summary>Releases the native detector. 释放 native 检测器。</summary>
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
                NativeException.ThrowIfError(fill(NativeHandle, image, points, offsetsPtr, offsets.Length, bufferPtr, buffer.Length, out decodedValue, out int writtenStringCount, out int writtenByteCount));
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
