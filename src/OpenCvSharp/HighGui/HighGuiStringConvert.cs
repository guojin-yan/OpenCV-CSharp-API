using System;
using System.Text;

namespace OpenCvSharp.HighGui
{
    internal static class HighGuiStringConvert
    {
        private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

        internal static byte[] ToUtf8(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            if (value.IndexOf('\0') >= 0)
            {
                throw new ArgumentException("Embedded null characters are not supported.", parameterName);
            }
            try
            {
                return StrictUtf8.GetBytes(value);
            }
            catch (EncoderFallbackException exception)
            {
                throw new ArgumentException("Value is not valid UTF-16 text.", parameterName, exception);
            }
        }

        internal static byte[] ToNullTerminatedUtf8(string value, string parameterName)
        {
            byte[] bytes = ToUtf8(value, parameterName);
            var buffer = new byte[bytes.Length + 1];
            Buffer.BlockCopy(bytes, 0, buffer, 0, bytes.Length);
            return buffer;
        }

        internal static string FromUtf8(byte[] value, int length)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (length < 0 || length > value.Length) throw new ArgumentOutOfRangeException(nameof(length));
            try
            {
                return StrictUtf8.GetString(value, 0, length);
            }
            catch (DecoderFallbackException exception)
            {
                throw new OpenCvException("Native HighGUI returned invalid UTF-8 text.", exception);
            }
        }
    }
}
