using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private static bool TryReadPfmScale(byte[] data, ref int offset, out int payloadOffset)
        {
            payloadOffset = 0;
            SkipPnmWhitespaceAndComments(data, ref offset);
            int start = offset;
            bool digit = false;
            bool nonzero = false;
            bool decimalPoint = false;
            bool exponent = false;
            bool exponentDigit = false;
            while (offset < data.Length && !IsPnmWhitespace(data[offset]) && data[offset] != (byte)'#')
            {
                byte value = data[offset];
                if (value >= (byte)'0' && value <= (byte)'9')
                {
                    if (exponent)
                    {
                        exponentDigit = true;
                    }
                    else
                    {
                        digit = true;
                        nonzero |= value != (byte)'0';
                    }
                }
                else if ((value == (byte)'+' || value == (byte)'-') &&
                    (offset == start || data[offset - 1] == (byte)'e' || data[offset - 1] == (byte)'E'))
                {
                }
                else if (value == (byte)'.' && !decimalPoint && !exponent)
                {
                    decimalPoint = true;
                }
                else if ((value == (byte)'e' || value == (byte)'E') && !exponent && digit)
                {
                    exponent = true;
                }
                else
                {
                    return false;
                }
                ++offset;
            }
            // Only the decimal's syntax and nonzero coefficient are needed for
            // encoded header facts. Native scale conversion has a separate range.
            // Scan in fixed state, including arbitrarily long decimal exponents.
            if (!digit || !nonzero || exponent && !exponentDigit) return false;

            // The PFM description permits any one whitespace byte after scale.
            // A following LF after CR is already a float byte. As with PNM, only
            // immediately attached comments are handled before this delimiter.
            if (!TryConsumePnmBinaryDelimiter(data, offset, out payloadOffset)) return false;
            offset = payloadOffset;
            return true;
        }

        private static bool IsPfmPayloadComplete(byte[] data, int payloadOffset, int width, int height, int channels)
        {
            if (payloadOffset < 0 || payloadOffset > data.Length || width <= 0 || height <= 0 || channels <= 0) return false;
            try
            {
                long expectedBytes = checked((long)width * height * channels * 4);
                return expectedBytes <= int.MaxValue && expectedBytes == data.Length - payloadOffset;
            }
            catch (OverflowException)
            {
                return false;
            }
        }
    }
}
