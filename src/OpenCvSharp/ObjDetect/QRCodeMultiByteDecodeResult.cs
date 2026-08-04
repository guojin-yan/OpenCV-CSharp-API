using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>Represents a multi-code QR decode result whose payloads retain their original bytes.</summary>
    public sealed class QRCodeMultiByteDecodeResult
    {
        private readonly byte[][] decodedInfo;

        /// <summary>Initializes a binary QR multi-code result.</summary>
        public QRCodeMultiByteDecodeResult(bool success, byte[][] decodedInfo, Mat? points)
        {
            Success = success;
            this.decodedInfo = Clone(decodedInfo);
            Points = points;
        }

        /// <summary>Gets whether OpenCV reported success.</summary>
        public bool Success { get; }

        /// <summary>Gets independent copies of the decoded byte payloads.</summary>
        public byte[][] DecodedInfo => Clone(decodedInfo);

        /// <summary>Gets the number of decoded payloads.</summary>
        public int DecodedInfoCount => decodedInfo.Length;

        /// <summary>Gets detected quadrangle points, or null when not requested.</summary>
        public Mat? Points { get; }

        /// <summary>Gets whether detected quadrangle points are available.</summary>
        public bool HasPoints => Points != null;

        /// <inheritdoc />
        public override string ToString() => $"{nameof(QRCodeMultiByteDecodeResult)}({nameof(Success)}={Success}, {nameof(DecodedInfo)}={DecodedInfoCount}, {nameof(HasPoints)}={HasPoints})";

        private static byte[][] Clone(byte[][] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            var result = new byte[values.Length][];
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null) throw new ArgumentNullException(nameof(values), "Decoded payloads cannot contain null elements.");
                result[i] = (byte[])values[i].Clone();
            }
            return result;
        }
    }
}
