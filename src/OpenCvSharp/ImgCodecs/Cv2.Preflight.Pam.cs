namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private static bool TryReadPamHeader(byte[] data, ref int offset, out int width, out int height, out PixelFacts pixels)
        {
            width = 0;
            height = 0;
            pixels = default(PixelFacts);
            // PAM is line based. Accept LF and the common CRLF compatibility form,
            // without interpreting another token on the magic-number line as a field.
            if (data.Length < 3 || data[0] != 'P' || data[1] != '7') return false;
            if (data[2] == 10) offset = 3;
            else if (data.Length >= 4 && data[2] == 13 && data[3] == 10) offset = 4;
            else return false;

            int parsedWidth = 0;
            int parsedHeight = 0;
            int depth = 0;
            int maxval = 0;
            bool sawWidth = false;
            bool sawHeight = false;
            bool sawDepth = false;
            bool sawMaxval = false;

            // Fixed state only. Comments and tuple types are scanned in the actual
            // input, never copied or concatenated. No declared geometry/depth allocates.
            while (offset < data.Length)
            {
                int end = offset;
                while (end < data.Length && data[end] != 10) ++end;
                if (end == data.Length) return false;

                int start = offset;
                offset = end + 1;
                while (start < end && IsPnmWhitespace(data[start])) ++start;
                if (start == end || data[start] == '#') continue;
                while (end > start && IsPnmWhitespace(data[end - 1])) --end;

                int tokenEnd = start;
                while (tokenEnd < end && !IsPnmWhitespace(data[tokenEnd])) ++tokenEnd;
                int valueStart = tokenEnd;
                while (valueStart < end && IsPnmWhitespace(data[valueStart])) ++valueStart;
                if (TokenEquals(data, start, tokenEnd, "ENDHDR"))
                {
                    if (valueStart != end || !sawWidth || !sawHeight || !sawDepth || !sawMaxval) return false;
                    // DEPTH is the encoded tuple size, independent of optional labels
                    // and of a particular native decoder's supported channel count.
                    width = parsedWidth;
                    height = parsedHeight;
                    pixels = new PixelFacts
                    {
                        BitDepth = maxval <= 255 ? 8 : 16,
                        BitDepthKnown = true,
                        Channels = depth,
                        ChannelsKnown = true
                    };
                    return true;
                }

                if (TokenEquals(data, start, tokenEnd, "TUPLTYPE"))
                {
                    if (valueStart == end) return false;
                    for (int index = valueStart; index < end; ++index)
                    {
                        byte value = data[index];
                        if (value > 126 || (value < 32 && !IsPnmWhitespace(value))) return false;
                    }
                    continue;
                }

                bool isWidth = TokenEquals(data, start, tokenEnd, "WIDTH");
                bool isHeight = TokenEquals(data, start, tokenEnd, "HEIGHT");
                bool isDepth = TokenEquals(data, start, tokenEnd, "DEPTH");
                bool isMaxval = TokenEquals(data, start, tokenEnd, "MAXVAL");
                if ((!isWidth && !isHeight && !isDepth && !isMaxval) ||
                    (isWidth && sawWidth) || (isHeight && sawHeight) || (isDepth && sawDepth) || (isMaxval && sawMaxval)) return false;

                int parsed;
                if (!TryReadPnmIntegerBounded(data, ref valueStart, end, out parsed) || valueStart != end ||
                    parsed <= 0 || (isMaxval && parsed > 65535)) return false;
                if (isWidth) { parsedWidth = parsed; sawWidth = true; }
                else if (isHeight) { parsedHeight = parsed; sawHeight = true; }
                else if (isDepth) { depth = parsed; sawDepth = true; }
                else { maxval = parsed; sawMaxval = true; }
            }
            return false;
        }
    }
}
