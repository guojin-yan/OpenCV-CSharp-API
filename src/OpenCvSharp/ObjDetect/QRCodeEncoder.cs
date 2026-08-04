using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// QR code encoder compatible with OpenCV <c>cv::QRCodeEncoder</c>.
    /// 与 OpenCV <c>cv::QRCodeEncoder</c> 兼容的二维码编码器。
    /// </summary>
    public sealed class QRCodeEncoder : IDisposable
    {
        private NativeQRCodeEncoderHandle handle;
        private bool disposed;

        /// <summary>Initializes a QR code encoder with default parameters. 使用默认参数初始化二维码编码器。</summary>
        public QRCodeEncoder()
            : this(QRCodeEncoderParams.Default)
        {
        }

        /// <summary>Initializes a QR code encoder with parameters. 使用参数初始化二维码编码器。</summary>
        public QRCodeEncoder(QRCodeEncoderParams parameters)
        {
            NativeMethods.QRCodeEncoderParamsNative native = parameters.ToNative();
            NativeException.ThrowIfError(NativeMethods.QRCodeEncoderCreate(ref native, out IntPtr nativeHandle));
            handle = NativeQRCodeEncoderHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this encoder has been disposed. 获取编码器是否已经释放。</summary>
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

        /// <summary>Creates a QR code encoder. 创建二维码编码器。</summary>
        public static QRCodeEncoder Create()
        {
            return new QRCodeEncoder();
        }

        /// <summary>Creates a QR code encoder with parameters. 使用参数创建二维码编码器。</summary>
        public static QRCodeEncoder Create(QRCodeEncoderParams parameters)
        {
            return new QRCodeEncoder(parameters);
        }

        /// <summary>Encodes text into a QR code matrix. 将文本编码为二维码矩阵。</summary>
        public Mat Encode(string encodedInfo)
        {
            var qrcode = new Mat();
            try
            {
                Encode(encodedInfo, qrcode);
                return qrcode;
            }
            catch
            {
                qrcode.Dispose();
                throw;
            }
        }

        /// <summary>Encodes text into an existing QR code matrix. 将文本编码到已有二维码矩阵。</summary>
        public void Encode(string encodedInfo, Mat qrcode)
        {
            ThrowIfDisposed();
            byte[] encoded = ObjDetectStringConvert.ToNullTerminatedUtf8(encodedInfo, nameof(encodedInfo));
            if (qrcode == null)
            {
                throw new ArgumentNullException(nameof(qrcode));
            }

            NativeException.ThrowIfError(NativeMethods.QRCodeEncoderEncode(NativeHandle, encoded, qrcode.NativeHandle));
        }

        /// <summary>Encodes text using structured append mode. 使用结构化追加模式编码文本。</summary>
        public Mat[] EncodeStructuredAppend(string encodedInfo)
        {
            ThrowIfDisposed();
            byte[] encoded = ObjDetectStringConvert.ToNullTerminatedUtf8(encodedInfo, nameof(encodedInfo));
            NativeException.ThrowIfError(NativeMethods.QRCodeEncoderEncodeStructuredAppendCount(NativeHandle, encoded, out int count));
            if (count <= 0)
            {
                return Array.Empty<Mat>();
            }

            var handles = new IntPtr[count];
            NativeException.ThrowIfError(NativeMethods.QRCodeEncoderEncodeStructuredAppendFill(NativeHandle, encoded, handles, handles.Length, out int written));
            int resultCount = Math.Max(0, Math.Min(written, handles.Length));
            var result = new Mat[resultCount];
            try
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = new Mat(handles[i]);
                    handles[i] = IntPtr.Zero;
                }

                return result;
            }
            catch
            {
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i] != null)
                    {
                        result[i].Dispose();
                    }
                }

                throw;
            }
        }

        /// <summary>Releases the native encoder. 释放 native 编码器。</summary>
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
