namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private static bool TryReadJpegFacts(byte[] data, out int width, out int height, out PixelFacts pixelFacts,
            out bool frameCountKnown, out MetadataFacts metadataFacts)
        {
            width = 0;
            height = 0;
            pixelFacts = default(PixelFacts);
            frameCountKnown = false;
            metadataFacts = default(MetadataFacts);
            MetadataFacts stored = new MetadataFacts();
            bool sawStartOfFrame = false;
            bool arithmeticCoding = false;
            bool inEntropyData = false;
            bool iccValid = true;
            int iccSegmentCount = 0;
            int iccSegmentsSeen = 0;
            bool[]? iccSegments = null;
            bool[]? components = null;
            int offset = 2;

            // Every cursor advances within the input. Segment payloads, including APP/COM after a scan,
            // are skipped by their validated lengths; an embedded FF D9 is never an EOI marker.
            while (offset < data.Length)
            {
                if (inEntropyData)
                {
                    while (offset < data.Length && data[offset] != 0xFF) ++offset;
                    if (offset == data.Length) break;
                }
                else if (data[offset] != 0xFF) break;

                while (offset < data.Length && data[offset] == 0xFF) ++offset;
                if (offset == data.Length) break;
                int marker = data[offset++];
                if (inEntropyData && (marker == 0 || (marker >= 0xD0 && marker <= 0xD7) || marker == 0x01)) continue;
                inEntropyData = false;

                if (marker == 0xD9)
                {
                    if (offset != data.Length) break;
                    frameCountKnown = sawStartOfFrame;
                    if (iccValid && iccSegmentsSeen == iccSegmentCount)
                    {
                        stored.MetadataSizeKnown = true;
                        stored.IccProfileSizeKnown = true;
                        metadataFacts = stored;
                    }
                    break;
                }
                if (marker == 0x01) continue;
                if (marker < 0xC0 || marker == 0xD8 || (marker >= 0xD0 && marker <= 0xD7)) break;

                // A second SOF or a hierarchical/differential frame cannot describe one proven image.
                if (marker == 0xDE || marker == 0xDF ||
                    (IsJpegStartOfFrame(marker) && (sawStartOfFrame || !IsJpegNonHierarchicalFrame(marker))))
                {
                    width = height = 0;
                    pixelFacts = default(PixelFacts);
                    return false;
                }
                if (data.Length - offset < 2) break;
                int length = (data[offset] << 8) | data[offset + 1];
                if (length < 2 || length > data.Length - offset) break;
                int payloadStart = offset + 2;
                int payloadLength = length - 2;

                if (IsJpegStartOfFrame(marker))
                {
                    if (length < 8) break;
                    int precision = data[offset + 2];
                    int channels = data[offset + 7];
                    int frameHeight = (data[offset + 3] << 8) | data[offset + 4];
                    int frameWidth = (data[offset + 5] << 8) | data[offset + 6];
                    if (channels == 0 || length != 8 + 3 * channels || frameWidth == 0 || frameHeight == 0 ||
                        !IsJpegPrecisionSupported(marker, precision)) break;
                    components = new bool[256];
                    bool validComponents = true;
                    for (int index = 0; index < channels; ++index)
                    {
                        int componentOffset = offset + 8 + 3 * index;
                        int component = data[componentOffset];
                        int sampling = data[componentOffset + 1];
                        if (components[component] || (sampling >> 4) < 1 || (sampling >> 4) > 4 ||
                            (sampling & 15) < 1 || (sampling & 15) > 4 || data[componentOffset + 2] > 3)
                        {
                            validComponents = false;
                            break;
                        }
                        components[component] = true;
                    }
                    if (!validComponents) break;
                    width = frameWidth;
                    height = frameHeight;
                    pixelFacts.BitDepth = precision;
                    pixelFacts.BitDepthKnown = true;
                    pixelFacts.Channels = channels;
                    pixelFacts.ChannelsKnown = true;
                    arithmeticCoding = marker >= 0xC9;
                    sawStartOfFrame = true;
                }
                else if (marker == 0xDA)
                {
                    if (!sawStartOfFrame || components == null || length < 6) break;
                    int scanComponents = data[payloadStart];
                    if (scanComponents == 0 || scanComponents > 4 || length != 6 + 2 * scanComponents) break;
                    bool validScan = true;
                    for (int index = 0; index < scanComponents; ++index)
                    {
                        int component = data[payloadStart + 1 + 2 * index];
                        int tables = data[payloadStart + 2 + 2 * index];
                        if (!components[component] || (!arithmeticCoding && ((tables >> 4) > 3 || (tables & 15) > 3))) validScan = false;
                        for (int previous = 0; previous < index; ++previous)
                            if (data[payloadStart + 1 + 2 * previous] == component) validScan = false;
                    }
                    if (!validScan) break;
                    inEntropyData = true;
                }
                else if (marker == 0xDC)
                {
                    // Height supplied by DNL, including resuming entropy after DNL, is not implemented.
                    break;
                }
                else if ((marker >= 0xE0 && marker <= 0xEF) || marker == 0xFE)
                {
                    // Disjoint payloads sum to at most data.Length; no expanded profile is allocated.
                    stored.MetadataBytes += payloadLength;
                    if (marker == 0xE2 && IsJpegIccProfileSegment(data, payloadStart, payloadLength))
                    {
                        if (payloadLength < 14) iccValid = false;
                        else
                        {
                            int sequence = data[payloadStart + 12];
                            int count = data[payloadStart + 13];
                            if (count == 0 || sequence == 0 || sequence > count ||
                                (iccSegmentCount != 0 && count != iccSegmentCount)) iccValid = false;
                            else
                            {
                                if (iccSegments == null) iccSegments = new bool[256];
                                if (iccSegments[sequence]) iccValid = false;
                                else
                                {
                                    iccSegments[sequence] = true;
                                    iccSegmentCount = count;
                                    ++iccSegmentsSeen;
                                    stored.IccProfileBytes += payloadLength - 14;
                                }
                            }
                        }
                    }
                }
                offset += length;
            }
            return sawStartOfFrame;
        }

        private static bool IsJpegStartOfFrame(int marker)
        {
            return (marker >= 0xC0 && marker <= 0xC3) || (marker >= 0xC5 && marker <= 0xC7) ||
                (marker >= 0xC9 && marker <= 0xCB) || (marker >= 0xCD && marker <= 0xCF);
        }

        private static bool IsJpegNonHierarchicalFrame(int marker)
        {
            return (marker >= 0xC0 && marker <= 0xC3) || (marker >= 0xC9 && marker <= 0xCB);
        }

        private static bool IsJpegPrecisionSupported(int marker, int precision)
        {
            if (marker == 0xC0) return precision == 8;
            if (marker == 0xC3 || marker == 0xCB) return precision >= 2 && precision <= 16;
            return precision == 8 || precision == 12;
        }

        private static bool IsJpegIccProfileSegment(byte[] data, int offset, int length)
        {
            return length >= 12 && data[offset] == (byte)'I' && data[offset + 1] == (byte)'C' &&
                data[offset + 2] == (byte)'C' && data[offset + 3] == (byte)'_' &&
                data[offset + 4] == (byte)'P' && data[offset + 5] == (byte)'R' &&
                data[offset + 6] == (byte)'O' && data[offset + 7] == (byte)'F' &&
                data[offset + 8] == (byte)'I' && data[offset + 9] == (byte)'L' &&
                data[offset + 10] == (byte)'E' && data[offset + 11] == 0;
        }
    }
}
