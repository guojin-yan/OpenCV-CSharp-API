using System;
using System.Text;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    internal static class ObjDetectStringConvert
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

        internal static string FromUtf8Bytes(byte[] buffer, int start, int count)
        {
            if (count <= 0)
            {
                return string.Empty;
            }

            return Encoding.UTF8.GetString(buffer, start, count);
        }
    }
}
