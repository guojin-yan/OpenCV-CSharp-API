using System;
using System.Text;

namespace OpenCvSharp.BioInspired
{
    internal static class BioInspiredStringConvert
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
            return value == null ? new byte[] { 0 } : ToNullTerminatedUtf8(value, nameof(value));
        }
    }
}
