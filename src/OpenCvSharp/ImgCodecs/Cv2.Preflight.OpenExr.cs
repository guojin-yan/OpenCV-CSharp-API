using System;
using System.Collections.Generic;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct OpenExrWindow
        {
            public int MinX;
            public int MinY;
            public int Width;
            public int Height;
        }

        private static bool TryReadOpenExrHeader(byte[] data, out int width, out int height, out PixelFacts pixelFacts)
        {
            width = height = 0;
            pixelFacts = default(PixelFacts);
            if (data.Length < 8) return false;
            int version = ReadSignedLe32(data, 4);
            int number = version & 255;
            if ((number != 1 && number != 2) || (version & ~0x1EFF) != 0) return false;

            // NON_IMAGE is file-wide in multipart files: it must not impose a later deep
            // part's sampling rules on an ordinary first part. A first-part type can refine it.
            bool unitSampling = (version & 0x1000) == 0 && (version & 0xA00) != 0;
            bool sawWindow = false, sawChannels = false, sawPartType = false;
            OpenExrWindow window = default(OpenExrWindow);
            int channelsStart = 0, channelsEnd = 0;
            int offset = 8;
            while (offset < data.Length)
            {
                if (data[offset] == 0)
                {
                    if (!sawWindow) return false;
                    PixelFacts candidate = default(PixelFacts);
                    if (sawChannels && !TryReadOpenExrChannels(data, channelsStart, channelsEnd, window, unitSampling, out candidate))
                        return false;
                    width = window.Width; height = window.Height; pixelFacts = candidate;
                    return true;
                }
                int nameStart, nameEnd, typeStart, typeEnd;
                if (!TryReadOpenExrCString(data, ref offset, data.Length, out nameStart, out nameEnd) || nameStart == nameEnd ||
                    !TryReadOpenExrCString(data, ref offset, data.Length, out typeStart, out typeEnd) || typeStart == typeEnd ||
                    data.Length - offset < 4) return false;
                uint size = (uint)ReadSignedLe32(data, offset);
                offset += 4;
                if (size > data.Length - offset) return false;
                int end = offset + (int)size;

                if (TokenEquals(data, nameStart, nameEnd, "dataWindow"))
                {
                    if (sawWindow || !TokenEquals(data, typeStart, typeEnd, "box2i") || size != 16) return false;
                    int minX = ReadSignedLe32(data, offset), minY = ReadSignedLe32(data, offset + 4);
                    long candidateWidth = (long)ReadSignedLe32(data, offset + 8) - minX + 1;
                    long candidateHeight = (long)ReadSignedLe32(data, offset + 12) - minY + 1;
                    if (candidateWidth <= 0 || candidateHeight <= 0 || candidateWidth > int.MaxValue || candidateHeight > int.MaxValue)
                        return false;
                    window.MinX = minX; window.MinY = minY;
                    window.Width = (int)candidateWidth; window.Height = (int)candidateHeight;
                    sawWindow = true;
                }
                else if (TokenEquals(data, nameStart, nameEnd, "channels"))
                {
                    // Check the name before the type: a wrong-typed duplicate cannot shadow
                    // an otherwise readable chlist. Defer the scan until dataWindow is known.
                    if (sawChannels || !TokenEquals(data, typeStart, typeEnd, "chlist")) return false;
                    sawChannels = true; channelsStart = offset; channelsEnd = end;
                }
                else if (TokenEquals(data, nameStart, nameEnd, "type"))
                {
                    if (sawPartType || !TokenEquals(data, typeStart, typeEnd, "string")) return false;
                    sawPartType = true;
                    unitSampling |= TokenEquals(data, offset, end, "tiledimage") ||
                        TokenEquals(data, offset, end, "deepscanline") || TokenEquals(data, offset, end, "deeptile");
                }
                offset = end;
            }
            return false;
        }

        private static bool TryReadOpenExrCString(byte[] data, ref int offset, int limit, out int start, out int end)
        {
            start = end = offset;
            while (end < limit && data[end] != 0)
            {
                if (end - start == 255) return false;
                ++end;
            }
            if (end == limit) return false;
            offset = end + 1;
            return true;
        }

        private static bool TryReadOpenExrChannels(byte[] data, int offset, int end, OpenExrWindow window,
            bool unitSampling, out PixelFacts facts)
        {
            facts = default(PixelFacts);
            var names = new HashSet<string>(StringComparer.Ordinal);
            int count = 0, depth = 0;
            bool uniform = true;
            while (offset < end)
            {
                int nameStart, nameEnd;
                if (!TryReadOpenExrCString(data, ref offset, end, out nameStart, out nameEnd)) return false;
                if (nameStart == nameEnd)
                {
                    if (offset != end || count == 0) return false;
                    facts.Channels = count; facts.ChannelsKnown = true;
                    facts.BitDepth = uniform ? depth : 0; facts.BitDepthKnown = uniform;
                    return true;
                }
                if (end - offset < 16 || count == 65535) return false;
                // Native names are byte strings, not necessarily valid UTF-8. Preserve every
                // byte in the key; decoding with replacement could merge distinct names.
                char[] key = new char[nameEnd - nameStart];
                for (int index = 0; index < key.Length; ++index) key[index] = (char)data[nameStart + index];
                if (!names.Add(new string(key))) return false;
                int type = ReadSignedLe32(data, offset);
                int x = ReadSignedLe32(data, offset + 8), y = ReadSignedLe32(data, offset + 12);
                if (type < 0 || type > 2 || x <= 0 || y <= 0 ||
                    window.MinX % x != 0 || window.MinY % y != 0 || window.Width % x != 0 || window.Height % y != 0 ||
                    (unitSampling && (x != 1 || y != 1))) return false;
                int currentDepth = type == 1 ? 16 : 32; // HALF, UINT or FLOAT encoded storage
                if (count == 0) depth = currentDepth;
                else uniform &= currentDepth == depth;
                ++count;
                // pLinear and the three reserved bytes do not change encoded depth/count.
                offset += 16;
            }
            return false;
        }
    }
}
