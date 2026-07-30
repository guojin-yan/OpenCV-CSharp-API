using System;
using System.Text;

namespace OpenCvSharp.Dnn
{
    internal static class DnnStringConvert
    {
        private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);

        internal static byte[] ToNullTerminatedUtf8(string value, string parameterName)
        {
            byte[] encoded = ToUtf8Bytes(value, parameterName, true);
            var buffer = new byte[encoded.Length + 1];
            Array.Copy(encoded, buffer, encoded.Length);
            return buffer;
        }

        internal static byte[] ToUtf8Bytes(string value, string parameterName, bool allowEmpty)
        {
            if (value == null) throw new ArgumentNullException(parameterName);
            if (!allowEmpty && value.Length == 0) throw new ArgumentException("Value cannot be empty.", parameterName);
            if (value.IndexOf('\0') >= 0) throw new ArgumentException("Embedded null characters are not supported.", parameterName);
            try { return StrictUtf8.GetBytes(value); }
            catch (EncoderFallbackException exception) { throw new ArgumentException("Value is not valid UTF-16 text.", parameterName, exception); }
        }

        internal static string FromUtf8Bytes(byte[] buffer, int start, int count)
        {
            if (count <= 0) return string.Empty;
            try { return StrictUtf8.GetString(buffer, start, count); }
            catch (DecoderFallbackException exception) { throw new OpenCvException("Native DNN text is not valid UTF-8.", exception); }
        }
    }
}
