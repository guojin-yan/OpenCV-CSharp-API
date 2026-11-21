using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct WebpContainerFacts
        {
            public FrameFacts Frames;
            public PixelFacts Pixels;
        }

        private static WebpContainerFacts ReadWebpContainerFacts(byte[] data)
        {
            if (data.Length < 20 || (uint)ReadSignedLe32(data, 4) != data.Length - 8)
                return default(WebpContainerFacts);

            int payload, length, next;
            if (!TryReadWebpChunk(data, 12, data.Length, out payload, out length, out next))
                return default(WebpContainerFacts);

            bool extended = IsWebpChunkType(data, 12, 'V', 'P', '8', 'X');
            int canvasWidth = 0, canvasHeight = 0;
            byte flags = 0;
            if (extended)
            {
                if (!TryReadWebpCanvas(data, payload, length, out canvasWidth, out canvasHeight))
                    return default(WebpContainerFacts);
                flags = data[payload];
            }

            bool animated = (flags & 2) != 0;
            bool declaredAlpha = (flags & 0x10) != 0;
            bool sawAnim = false, sawImage = false, sawIcc = false, sawAlpha = false;
            bool imageAlpha = false;
            int alphaPayload = 0, alphaLength = 0;
            int frameCount = 0;
            int offset = extended ? next : 12;

            // Fixed state and one level of ANMF subchunks only. Declared sizes/counts never
            // allocate storage; each iteration consumes bytes inside its actual parent boundary.
            while (offset < data.Length)
            {
                if (!TryReadWebpChunk(data, offset, data.Length, out payload, out length, out next))
                    return default(WebpContainerFacts);
                if (IsWebpChunkType(data, offset, 'V', 'P', '8', 'X')) return default(WebpContainerFacts);
                if (IsWebpChunkType(data, offset, 'A', 'N', 'I', 'M'))
                {
                    if (!animated || sawAnim || frameCount != 0 || length != 6) return default(WebpContainerFacts);
                    sawAnim = true;
                }
                else if (IsWebpChunkType(data, offset, 'A', 'N', 'M', 'F'))
                {
                    if (!animated || !sawAnim || length < 16) return default(WebpContainerFacts);
                    int x = 2 * ReadWebpLe24(data, payload);
                    int y = 2 * ReadWebpLe24(data, payload + 3);
                    int width = 1 + ReadWebpLe24(data, payload + 6);
                    int height = 1 + ReadWebpLe24(data, payload + 9);
                    if ((long)x + width > canvasWidth || (long)y + height > canvasHeight)
                        return default(WebpContainerFacts);
                    bool frameAlpha;
                    if (!TryReadWebpFrameImage(data, payload + 16, payload + length, width, height, out frameAlpha) ||
                        (frameAlpha && !declaredAlpha)) return default(WebpContainerFacts);
                    // Frame flags' reserved six bits MUST be ignored by readers.
                    ++frameCount;
                }
                else if (IsWebpChunkType(data, offset, 'A', 'L', 'P', 'H'))
                {
                    if (!extended || animated || sawImage || sawAlpha || !declaredAlpha) return default(WebpContainerFacts);
                    sawAlpha = true;
                    alphaPayload = payload;
                    alphaLength = length;
                }
                else if (IsWebpChunkType(data, offset, 'V', 'P', '8', ' ') ||
                         IsWebpChunkType(data, offset, 'V', 'P', '8', 'L'))
                {
                    int width, height;
                    bool alpha;
                    if (animated || sawImage || !TryReadWebpImage(data, offset, payload, length, out width, out height, out alpha) ||
                        (sawAlpha && (IsWebpChunkType(data, offset, 'V', 'P', '8', 'L') ||
                            !IsWebpAlphaPayloadSupported(data, alphaPayload, alphaLength, width, height))))
                        return default(WebpContainerFacts);
                    if (extended && (width != canvasWidth || height != canvasHeight)) return default(WebpContainerFacts);
                    if (!extended) { canvasWidth = width; canvasHeight = height; }
                    imageAlpha = alpha || sawAlpha;
                    if (extended && imageAlpha && !declaredAlpha) return default(WebpContainerFacts);
                    sawImage = true;
                }
                else if (IsWebpChunkType(data, offset, 'I', 'C', 'C', 'P'))
                {
                    if (!extended || (flags & 0x20) == 0 || sawIcc || sawAnim || sawImage || sawAlpha)
                        return default(WebpContainerFacts);
                    sawIcc = true;
                }
                else if (!extended && (!sawImage || IsKnownWebpChunk(data, offset))) return default(WebpContainerFacts);
                // Metadata and unknown chunks may appear out of order. Their stored payload
                // lengths are inspected independently; this walk does not validate ICC/EXIF/XMP.
                offset = next;
            }

            if (animated ? !sawAnim || frameCount == 0 : !sawImage) return default(WebpContainerFacts);
            return new WebpContainerFacts
            {
                Frames = new FrameFacts { FrameCount = animated ? frameCount : 1, FrameCountKnown = true },
                // An extended alpha flag budgets four channels even when individual frames
                // are opaque. VP8L's alpha bit is an encoded header hint, not decoded pixels.
                Pixels = new PixelFacts { BitDepth = 8, BitDepthKnown = true,
                    Channels = declaredAlpha || imageAlpha ? 4 : 3, ChannelsKnown = true }
            };
        }

        private static bool TryReadWebpFrameImage(byte[] data, int offset, int end, int width, int height, out bool alpha)
        {
            alpha = false;
            bool sawImage = false, sawAlpha = false;
            int alphaPayload = 0, alphaLength = 0;
            while (offset < end)
            {
                int payload, length, next;
                if (!TryReadWebpChunk(data, offset, end, out payload, out length, out next)) return false;
                if (IsWebpChunkType(data, offset, 'A', 'L', 'P', 'H'))
                {
                    if (sawAlpha || sawImage) return false;
                    sawAlpha = true;
                    alphaPayload = payload;
                    alphaLength = length;
                }
                else if (IsWebpChunkType(data, offset, 'V', 'P', '8', ' ') ||
                         IsWebpChunkType(data, offset, 'V', 'P', '8', 'L'))
                {
                    int imageWidth, imageHeight;
                    bool imageAlpha;
                    if (sawImage || !TryReadWebpImage(data, offset, payload, length, out imageWidth, out imageHeight, out imageAlpha) ||
                        imageWidth != width || imageHeight != height ||
                        (sawAlpha && (IsWebpChunkType(data, offset, 'V', 'P', '8', 'L') ||
                            !IsWebpAlphaPayloadSupported(data, alphaPayload, alphaLength, width, height)))) return false;
                    alpha = imageAlpha || sawAlpha;
                    sawImage = true;
                }
                else if (!sawImage || IsKnownWebpChunk(data, offset)) return false;
                offset = next;
            }
            return sawImage;
        }

        private static bool IsWebpAlphaPayloadSupported(byte[] data, int payload, int length, int width, int height)
        {
            if (length < 2) return false;
            int compression = data[payload] & 3;
            int preprocessing = (data[payload] >> 4) & 3;
            // Reserved bits are ignored; undefined compression/preprocessing stays unknown.
            if (compression > 1 || preprocessing > 1) return false;
            return compression == 1 || (long)length - 1 == (long)width * height;
        }

        private static bool TryReadWebpImage(byte[] data, int chunk, int payload, int length,
            out int width, out int height, out bool alpha)
        {
            if (!TryReadWebpImageHeader(data, chunk, payload, length, out width, out height, out alpha)) return false;
            if (IsWebpChunkType(data, chunk, 'V', 'P', '8', 'L')) return length > 5;
            int partitionLength = ReadWebpLe24(data, payload) >> 5;
            return partitionLength > 0 && partitionLength <= length - 10;
        }

        private static bool TryReadWebpImageHeader(byte[] data, int chunk, int payload, int length,
            out int width, out int height, out bool alpha)
        {
            width = 0; height = 0; alpha = false;
            if (IsWebpChunkType(data, chunk, 'V', 'P', '8', ' ') && length >= 10)
            {
                int tag = ReadWebpLe24(data, payload);
                if ((tag & 1) != 0 || ((tag >> 1) & 7) > 3 || (tag & 0x10) == 0 ||
                    data[payload + 3] != 0x9D || data[payload + 4] != 1 || data[payload + 5] != 0x2A) return false;
                width = ReadLe16(data, payload + 6) & 0x3FFF;
                height = ReadLe16(data, payload + 8) & 0x3FFF;
                return width > 0 && height > 0;
            }
            if (IsWebpChunkType(data, chunk, 'V', 'P', '8', 'L') && length >= 5 &&
                data[payload] == 0x2F && (data[payload + 4] >> 5) == 0)
            {
                uint bits = (uint)ReadSignedLe32(data, payload + 1);
                width = (int)(bits & 0x3FFF) + 1;
                height = (int)((bits >> 14) & 0x3FFF) + 1;
                alpha = (bits & (1U << 28)) != 0;
                return true;
            }
            return false;
        }

        private static bool TryReadWebpSize(byte[] data, out int width, out int height)
        {
            width = 0; height = 0;
            int payload, length, next;
            if (!TryReadWebpChunk(data, 12, data.Length, out payload, out length, out next)) return false;
            if (IsWebpChunkType(data, 12, 'V', 'P', '8', 'X'))
                return TryReadWebpCanvas(data, payload, length, out width, out height);
            bool alpha;
            return TryReadWebpImageHeader(data, 12, payload, length, out width, out height, out alpha);
        }

        private static bool TryReadWebpCanvas(byte[] data, int payload, int length, out int width, out int height)
        {
            width = 0; height = 0;
            if (length < 10) return false;
            int w = 1 + ReadWebpLe24(data, payload + 4);
            int h = 1 + ReadWebpLe24(data, payload + 7);
            if ((ulong)w * (ulong)h > uint.MaxValue) return false;
            // Reserved bits/bytes and future VP8X trailing fields MUST be ignored by readers.
            width = w; height = h;
            return true;
        }

        private static bool TryReadWebpChunk(byte[] data, int offset, int end, out int payload, out int length, out int next)
        {
            payload = 0; length = 0; next = 0;
            if (end - offset < 8) return false;
            uint declared = (uint)ReadSignedLe32(data, offset + 4);
            if (declared > (uint)(end - offset - 8)) return false;
            payload = offset + 8;
            length = (int)declared;
            int payloadEnd = payload + length;
            if ((length & 1) != 0)
            {
                if (payloadEnd == end || data[payloadEnd] != 0) return false;
                ++payloadEnd;
            }
            next = payloadEnd;
            return true;
        }

        private static bool IsKnownWebpChunk(byte[] data, int offset)
        {
            return IsWebpChunkType(data, offset, 'V', 'P', '8', 'X') || IsWebpChunkType(data, offset, 'V', 'P', '8', ' ') ||
                IsWebpChunkType(data, offset, 'V', 'P', '8', 'L') || IsWebpChunkType(data, offset, 'A', 'L', 'P', 'H') ||
                IsWebpChunkType(data, offset, 'A', 'N', 'I', 'M') || IsWebpChunkType(data, offset, 'A', 'N', 'M', 'F') ||
                IsWebpChunkType(data, offset, 'I', 'C', 'C', 'P') || IsWebpChunkType(data, offset, 'E', 'X', 'I', 'F') ||
                IsWebpChunkType(data, offset, 'X', 'M', 'P', ' ');
        }

        private static int ReadWebpLe24(byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16);
        }

        private static bool IsWebpChunkType(byte[] data, int offset, char first, char second, char third, char fourth)
        {
            return data[offset] == (byte)first && data[offset + 1] == (byte)second &&
                data[offset + 2] == (byte)third && data[offset + 3] == (byte)fourth;
        }
    }
}
