using System;
using System.Text;

namespace JYPPX.OpenCvSharp.ML
{
    internal static class MLStringConvert
    {
        internal static byte[] ToNullTerminatedUtf8(string? value, string parameterName, bool allowNull = false)
        {
            if (value == null)
            {
                if (allowNull)
                {
                    return new byte[] { 0 };
                }

                throw new ArgumentNullException(parameterName);
            }

            if (value.IndexOf('\0') >= 0)
            {
                throw new ArgumentException("String value cannot contain embedded null characters.", parameterName);
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            var buffer = new byte[byteCount + 1];
            Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            return buffer;
        }

        internal static string FromUtf8Bytes(byte[] buffer, int start, int count)
        {
            return Encoding.UTF8.GetString(buffer, start, count);
        }
    }
}
