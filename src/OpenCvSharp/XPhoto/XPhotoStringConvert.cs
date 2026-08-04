using System;
using System.Text;

namespace JYPPX.OpenCvSharp.XPhoto
{
    internal static class XPhotoStringConvert
    {
        internal static byte[] ToNullTerminatedUtf8(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            var buffer = new byte[byteCount + 1];
            Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            return buffer;
        }

        internal static byte[] ToOptionalNullTerminatedUtf8(string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return new byte[] { 0 };
            }

            return ToNullTerminatedUtf8(value!, nameof(value));
        }
    }
}
