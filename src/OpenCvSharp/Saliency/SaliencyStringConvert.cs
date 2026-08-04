using System;
using System.Text;

namespace JYPPX.OpenCvSharp.Saliency
{
    internal static class SaliencyStringConvert
    {
        internal static byte[] ToNullTerminatedUtf8(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (value.IndexOf('\0') >= 0)
            {
                throw new ArgumentException("Embedded null characters are not allowed.", parameterName);
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            var buffer = new byte[byteCount + 1];
            Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            return buffer;
        }
    }
}
