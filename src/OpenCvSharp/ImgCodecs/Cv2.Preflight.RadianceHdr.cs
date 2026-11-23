using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct RadianceHdrFacts
        {
            public int Width;
            public int Height;
            public bool HeaderKnown;
            public bool FrameKnown;
            public PixelFacts Pixels;
        }

        private static RadianceHdrFacts ReadRadianceHdrFacts(byte[] data)
        {
            RadianceHdrFacts facts = new RadianceHdrFacts();
            int offset = 0;
            int lineStart;
            int lineEnd;
            if (!TryReadRadianceHdrLine(data, ref offset, out lineStart, out lineEnd)) return facts;

            bool hasFormat = false;
            bool headerEnd = false;
            while (TryReadRadianceHdrLine(data, ref offset, out lineStart, out lineEnd))
            {
                if (lineStart == lineEnd)
                {
                    headerEnd = true;
                    break;
                }
                if (lineEnd - lineStart < 7 || !TokenEquals(data, lineStart, lineStart + 7, "FORMAT=")) continue;
                // The picture format permits at most one FORMAT assignment.
                // Unknown header variables do not supply admission/color facts.
                if (hasFormat) return facts;
                int valueStart = lineStart + 7;
                while (valueStart < lineEnd && IsRadianceWhitespace(data[valueStart])) ++valueStart;
                int valueEnd = lineEnd;
                while (valueEnd > valueStart && IsRadianceWhitespace(data[valueEnd - 1])) --valueEnd;
                if (!TokenEquals(data, valueStart, valueEnd, "32-bit_rle_rgbe") &&
                    !TokenEquals(data, valueStart, valueEnd, "32-bit_rle_xyze")) return facts;
                hasFormat = true;
            }

            int width;
            int height;
            int scanlineLength;
            int scanlines;
            if (!hasFormat || !headerEnd ||
                !TryReadRadianceHdrLine(data, ref offset, out lineStart, out lineEnd) ||
                !TryReadRadianceResolution(data, lineStart, lineEnd, out width, out height, out scanlineLength, out scanlines))
                return facts;

            facts.Width = width;
            facts.Height = height;
            facts.HeaderKnown = true;
            facts.Pixels.BitDepth = 8;
            facts.Pixels.BitDepthKnown = true;
            facts.Pixels.Channels = 4;
            facts.Pixels.ChannelsKnown = true;
            facts.FrameKnown = IsRadianceHdrPayloadComplete(data, offset, scanlineLength, scanlines);
            return facts;
        }

        private static bool TryReadRadianceHdrLine(byte[] data, ref int offset, out int start, out int end)
        {
            start = offset;
            end = offset;
            while (end < data.Length && data[end] != 10) ++end;
            if (end >= data.Length) return false;
            offset = end + 1;
            // CRLF header lines are accepted without consuming any raster bytes.
            if (end > start && data[end - 1] == 13) --end;
            return true;
        }

        private static bool TryReadRadianceResolution(byte[] data, int start, int end, out int width, out int height,
            out int scanlineLength, out int scanlines)
        {
            width = 0;
            height = 0;
            scanlineLength = 0;
            scanlines = 0;
            int offset = start;
            while (offset < end && IsRadianceWhitespace(data[offset])) ++offset;
            byte firstAxis;
            byte secondAxis;
            int firstCount;
            int secondCount;
            if (!TryReadRadianceAxis(data, ref offset, end, out firstAxis, out firstCount) ||
                !SkipRadianceWhitespace(data, ref offset, end) ||
                !TryReadRadianceAxis(data, ref offset, end, out secondAxis, out secondCount) || firstAxis == secondAxis)
                return false;
            while (offset < end && IsRadianceWhitespace(data[offset])) ++offset;
            if (offset != end) return false;
            width = firstAxis == (byte)'X' ? firstCount : secondCount;
            height = firstAxis == (byte)'Y' ? firstCount : secondCount;
            // The first axis counts records; the second counts pixels per record.
            // Axis signs affect orientation, not encoded geometry or storage.
            scanlines = firstCount;
            scanlineLength = secondCount;
            return true;
        }

        private static bool TryReadRadianceAxis(byte[] data, ref int offset, int end, out byte axis, out int count)
        {
            axis = 0;
            count = 0;
            if (end - offset < 2 || (data[offset] != (byte)'+' && data[offset] != (byte)'-') ||
                (data[offset + 1] != (byte)'X' && data[offset + 1] != (byte)'Y')) return false;
            axis = data[offset + 1];
            offset += 2;
            return SkipRadianceWhitespace(data, ref offset, end) && TryReadPositiveAsciiInteger(data, ref offset, end, out count);
        }

        private static bool SkipRadianceWhitespace(byte[] data, ref int offset, int end)
        {
            int start = offset;
            while (offset < end && IsRadianceWhitespace(data[offset])) ++offset;
            return offset > start;
        }

        private static bool IsRadianceWhitespace(byte value)
        {
            return value == 9 || value == 11 || value == 12 || value == 13 || value == 32;
        }

        private static bool TryReadPositiveAsciiInteger(byte[] data, ref int offset, int end, out int value)
        {
            value = 0;
            if (offset < end && data[offset] == (byte)'+') ++offset;
            if (offset >= end || data[offset] < (byte)'0' || data[offset] > (byte)'9') return false;
            long parsed = 0;
            while (offset < end && data[offset] >= (byte)'0' && data[offset] <= (byte)'9')
            {
                parsed = parsed * 10 + data[offset] - (byte)'0';
                if (parsed > int.MaxValue) return false;
                ++offset;
            }
            if (parsed == 0) return false;
            value = (int)parsed;
            return true;
        }

        private static bool IsRadianceHdrPayloadComplete(byte[] data, int offset, int scanlineLength, int scanlines)
        {
            if (offset < 0 || offset > data.Length || scanlineLength <= 0 || scanlines <= 0) return false;
            for (int scanline = 0; scanline < scanlines; ++scanline)
            {
                if (data.Length - offset < 4) return false;
                if (data[offset] == 2 && data[offset + 1] == 2 && (data[offset + 2] & 0x80) == 0)
                {
                    if ((data[offset + 2] << 8 | data[offset + 3]) != scanlineLength) return false;
                    offset += 4;
                    if (!TryConsumeRadianceHdrComponentRuns(data, ref offset, scanlineLength)) return false;
                }
                else if (!TryConsumeRadianceHdrOldScanline(data, ref offset, scanlineLength)) return false;
            }
            return offset == data.Length;
        }

        private static bool TryConsumeRadianceHdrComponentRuns(byte[] data, ref int offset, int length)
        {
            for (int component = 0; component < 4; ++component)
            {
                int decoded = 0;
                while (decoded < length)
                {
                    if (offset >= data.Length) return false;
                    int code = data[offset++];
                    int count = code > 128 ? code - 128 : code;
                    if (count == 0 || count > length - decoded) return false;
                    int encodedBytes = code > 128 ? 1 : count;
                    if (encodedBytes > data.Length - offset) return false;
                    offset += encodedBytes;
                    decoded += count;
                }
            }
            return true;
        }

        private static bool TryConsumeRadianceHdrOldScanline(byte[] data, ref int offset, int length)
        {
            int decoded = 0;
            int shift = 0;
            while (decoded < length)
            {
                if (data.Length - offset < 4) return false;
                bool repeat = data[offset] == 1 && data[offset + 1] == 1 && data[offset + 2] == 1;
                int value = data[offset + 3];
                offset += 4;
                if (repeat)
                {
                    if (decoded == 0 || (shift >= 32 && value != 0)) return false;
                    long count = (long)value << shift;
                    if (count > length - decoded) return false;
                    decoded += (int)count;
                    // Saturate zero high-order bytes; never allow C# shift wrap.
                    shift = Math.Min(32, shift + 8);
                }
                else
                {
                    ++decoded;
                    shift = 0;
                }
            }
            return true;
        }
    }
}
