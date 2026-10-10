using System;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private static bool IsAvifSignature(byte[] data)
        {
            if (data.Length < 16 || !IsAvifBoxType(data, 4, 'f', 't', 'y', 'p')) return false;
            if (IsAvifBrand(data, 8)) return true;

            uint declaredSize = ReadBe32(data, 0);
            int end = declaredSize >= 16 && declaredSize <= data.Length ? (int)declaredSize : data.Length;
            for (int offset = 16; offset + 4 <= end; offset += 4)
            {
                if (IsAvifBrand(data, offset)) return true;
            }
            return false;
        }

        private static bool TryReadAvifHeader(byte[] data, out int width, out int height, out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();
            if (data.Length < 16) return false;

            int offset = 0;
            bool sawFileType = false;
            bool sawMeta = false;
            bool dimensionsSeen = false;
            bool dimensionsConflict = false;
            int dimensionWidth = 0;
            int dimensionHeight = 0;
            bool pixelProfileSeen = false;
            bool pixelProfileConflict = false;
            int pixelBitDepth = 0;
            int pixelChannels = 0;

            while (offset < data.Length)
            {
                int next;
                int payloadOffset;
                int payloadLength;
                if (!TryReadAvifBox(data, offset, data.Length, out next, out payloadOffset, out payloadLength)) return false;

                if (!sawFileType)
                {
                    if (!IsAvifBoxType(data, offset + 4, 'f', 't', 'y', 'p') ||
                        !TryReadAvifFileTypePayload(data, payloadOffset, payloadLength)) return false;
                    sawFileType = true;
                }
                else if (IsAvifBoxType(data, offset + 4, 'f', 't', 'y', 'p'))
                {
                    return false;
                }
                else if (IsAvifBoxType(data, offset + 4, 'm', 'e', 't', 'a'))
                {
                    if (sawMeta || !TryReadAvifMeta(data, payloadOffset, payloadLength,
                        ref dimensionsSeen, ref dimensionsConflict, ref dimensionWidth, ref dimensionHeight,
                        ref pixelProfileSeen, ref pixelProfileConflict, ref pixelBitDepth, ref pixelChannels))
                    {
                        return false;
                    }
                    sawMeta = true;
                }

                offset = next;
            }

            if (!sawFileType || !sawMeta || !dimensionsSeen || dimensionsConflict) return false;
            width = dimensionWidth;
            height = dimensionHeight;
            if (pixelProfileSeen && !pixelProfileConflict)
            {
                pixelFacts.BitDepth = pixelBitDepth;
                pixelFacts.BitDepthKnown = true;
                pixelFacts.Channels = pixelChannels;
                pixelFacts.ChannelsKnown = true;
            }
            return true;
        }

        private static bool TryReadAvifFileTypePayload(byte[] data, int payloadOffset, int payloadLength)
        {
            if (payloadLength < 8) return false;
            if (IsAvifBrand(data, payloadOffset)) return true;
            for (int offset = payloadOffset + 8; offset + 4 <= payloadOffset + payloadLength; offset += 4)
            {
                if (IsAvifBrand(data, offset)) return true;
            }
            return false;
        }

        private static bool TryReadAvifMeta(byte[] data, int payloadOffset, int payloadLength,
            ref bool dimensionsSeen, ref bool dimensionsConflict, ref int dimensionWidth, ref int dimensionHeight,
            ref bool pixelProfileSeen, ref bool pixelProfileConflict, ref int pixelBitDepth, ref int pixelChannels)
        {
            if (payloadLength < 4) return false;
            int offset = payloadOffset + 4;
            int end = payloadOffset + payloadLength;
            while (offset < end)
            {
                int next;
                int childPayloadOffset;
                int childPayloadLength;
                if (!TryReadAvifBox(data, offset, end, out next, out childPayloadOffset, out childPayloadLength)) return false;
                if (IsAvifBoxType(data, offset + 4, 'i', 'p', 'r', 'p') &&
                    !TryReadAvifIprp(data, childPayloadOffset, childPayloadLength,
                        ref dimensionsSeen, ref dimensionsConflict, ref dimensionWidth, ref dimensionHeight,
                        ref pixelProfileSeen, ref pixelProfileConflict, ref pixelBitDepth, ref pixelChannels))
                {
                    return false;
                }
                offset = next;
            }
            return offset == end;
        }

        private static bool TryReadAvifIprp(byte[] data, int payloadOffset, int payloadLength,
            ref bool dimensionsSeen, ref bool dimensionsConflict, ref int dimensionWidth, ref int dimensionHeight,
            ref bool pixelProfileSeen, ref bool pixelProfileConflict, ref int pixelBitDepth, ref int pixelChannels)
        {
            int offset = payloadOffset;
            int end = payloadOffset + payloadLength;
            while (offset < end)
            {
                int next;
                int childPayloadOffset;
                int childPayloadLength;
                if (!TryReadAvifBox(data, offset, end, out next, out childPayloadOffset, out childPayloadLength)) return false;
                if (IsAvifBoxType(data, offset + 4, 'i', 'p', 'c', 'o') &&
                    !TryReadAvifIpco(data, childPayloadOffset, childPayloadLength,
                        ref dimensionsSeen, ref dimensionsConflict, ref dimensionWidth, ref dimensionHeight,
                        ref pixelProfileSeen, ref pixelProfileConflict, ref pixelBitDepth, ref pixelChannels))
                {
                    return false;
                }
                offset = next;
            }
            return offset == end;
        }

        private static bool TryReadAvifIpco(byte[] data, int payloadOffset, int payloadLength,
            ref bool dimensionsSeen, ref bool dimensionsConflict, ref int dimensionWidth, ref int dimensionHeight,
            ref bool pixelProfileSeen, ref bool pixelProfileConflict, ref int pixelBitDepth, ref int pixelChannels)
        {
            int offset = payloadOffset;
            int end = payloadOffset + payloadLength;
            while (offset < end)
            {
                int next;
                int propertyPayloadOffset;
                int propertyPayloadLength;
                if (!TryReadAvifBox(data, offset, end, out next, out propertyPayloadOffset, out propertyPayloadLength)) return false;

                if (IsAvifBoxType(data, offset + 4, 'i', 's', 'p', 'e'))
                {
                    if (propertyPayloadLength != 12) return false;
                    uint unsignedWidth = ReadBe32(data, propertyPayloadOffset + 4);
                    uint unsignedHeight = ReadBe32(data, propertyPayloadOffset + 8);
                    if (unsignedWidth == 0 || unsignedHeight == 0 || unsignedWidth > int.MaxValue || unsignedHeight > int.MaxValue) return false;
                    int currentWidth = (int)unsignedWidth;
                    int currentHeight = (int)unsignedHeight;
                    if (!dimensionsSeen)
                    {
                        dimensionWidth = currentWidth;
                        dimensionHeight = currentHeight;
                        dimensionsSeen = true;
                    }
                    else if (dimensionWidth != currentWidth || dimensionHeight != currentHeight)
                    {
                        dimensionsConflict = true;
                    }
                }
                else if (IsAvifBoxType(data, offset + 4, 'p', 'i', 'x', 'i'))
                {
                    if (propertyPayloadLength < 5) return false;
                    int channels = data[propertyPayloadOffset + 4];
                    if (channels <= 0 || propertyPayloadLength != 5 + channels) return false;
                    int bitDepth = data[propertyPayloadOffset + 5];
                    if (bitDepth != 8 && bitDepth != 10 && bitDepth != 12) return false;
                    for (int channel = 1; channel < channels; ++channel)
                    {
                        if (data[propertyPayloadOffset + 5 + channel] != bitDepth) return false;
                    }
                    if (channels != 1 && channels != 3 && channels != 4) return false;
                    if (!pixelProfileSeen)
                    {
                        pixelBitDepth = bitDepth;
                        pixelChannels = channels;
                        pixelProfileSeen = true;
                    }
                    else if (pixelBitDepth != bitDepth || pixelChannels != channels)
                    {
                        pixelProfileConflict = true;
                    }
                }

                offset = next;
            }
            return offset == end;
        }

        private static bool TryReadAvifBox(byte[] data, int offset, int end, out int next, out int payloadOffset, out int payloadLength)
        {
            next = offset;
            payloadOffset = 0;
            payloadLength = 0;
            if (offset < 0 || offset > end || end - offset < 8) return false;

            uint size = ReadBe32(data, offset);
            int headerLength = 8;
            long boxLength;
            if (size == 1)
            {
                if (end - offset < 16) return false;
                ulong extendedSize = ((ulong)ReadBe32(data, offset + 8) << 32) | ReadBe32(data, offset + 12);
                boxLength = (long)extendedSize;
                headerLength = 16;
            }
            else if (size == 0)
            {
                boxLength = end - offset;
            }
            else
            {
                boxLength = size;
            }

            if (boxLength < headerLength || boxLength > end - offset) return false;
            long nextOffset = offset + boxLength;
            if (nextOffset > int.MaxValue) return false;
            next = (int)nextOffset;
            payloadOffset = offset + headerLength;
            payloadLength = (int)boxLength - headerLength;
            return true;
        }

        private static bool IsAvifBrand(byte[] data, int offset)
        {
            return offset >= 0 && offset + 4 <= data.Length &&
                data[offset] == (byte)'a' && data[offset + 1] == (byte)'v' &&
                data[offset + 2] == (byte)'i' && (data[offset + 3] == (byte)'f' || data[offset + 3] == (byte)'s');
        }

        private static bool IsAvifBoxType(byte[] data, int offset, char first, char second, char third, char fourth)
        {
            return offset >= 0 && offset + 4 <= data.Length && data[offset] == (byte)first && data[offset + 1] == (byte)second &&
                data[offset + 2] == (byte)third && data[offset + 3] == (byte)fourth;
        }
    }
}
