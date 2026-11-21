namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        private struct PngFacts
        {
            public int Width;
            public int Height;
            public bool SizeKnown;
            public PixelFacts Pixels;
            public FrameFacts Frames;
            public MetadataFacts Metadata;
        }

        private static PngFacts ReadPngHeader(byte[] data)
        {
            // Include the complete IHDR payload and its CRC field before trusting any
            // header facts. CRC values and compressed content are not checked here.
            if (data.Length < 33 || ReadBe32(data, 8) != 13 ||
                !IsPngChunkType(data, 12, 'I', 'H', 'D', 'R')) return default(PngFacts);
            uint width = ReadBe32(data, 16);
            uint height = ReadBe32(data, 20);
            if (width == 0 || width > int.MaxValue || height == 0 || height > int.MaxValue ||
                data[26] != 0 || data[27] != 0 || data[28] > 1) return default(PngFacts);

            int depth = data[24];
            int color = data[25];
            int channels;
            switch (color)
            {
                case 0: channels = 1; break;
                case 2: channels = 3; break;
                case 3: channels = 1; break;
                case 4: channels = 2; break;
                case 6: channels = 4; break;
                default: return default(PngFacts);
            }
            if (depth != 1 && depth != 2 && depth != 4 && depth != 8 && depth != 16) return default(PngFacts);
            if ((color == 2 || color == 4 || color == 6) && depth != 8 && depth != 16) return default(PngFacts);
            if (color == 3 && depth == 16) return default(PngFacts);

            return new PngFacts
            {
                Width = (int)width,
                Height = (int)height,
                SizeKnown = true,
                Pixels = new PixelFacts { BitDepth = depth, BitDepthKnown = true, Channels = channels, ChannelsKnown = true }
            };
        }

        private static PngFacts ReadPngFacts(byte[] data)
        {
            PngFacts facts = ReadPngHeader(data);
            if (!facts.SizeKnown) return facts;
            int color = data[25];
            int paletteEntries = 0;
            bool sawPalette = false;
            bool sawTransparency = false;
            bool sawIcc = false;
            bool sawExif = false;
            bool sawIdat = false;
            bool idatEnded = false;
            bool hasImageData = false;
            MetadataFacts metadata = default(MetadataFacts);
            int offset = 33;

            // Fixed state; jump over payloads using subtraction-checked input ranges.
            // Only the ICC name (at most 80 bytes including its terminator) is scanned.
            // No declared chunk, profile, image, or frame length allocates storage.
            while (data.Length - offset >= 12)
            {
                uint length = ReadBe32(data, offset);
                if (length > (uint)(data.Length - offset - 12) || !IsPngChunkName(data, offset + 4)) return facts;
                int payload = offset + 8;
                int count = (int)length;
                bool isPalette = IsPngChunkType(data, offset + 4, 'P', 'L', 'T', 'E');
                bool isIdat = IsPngChunkType(data, offset + 4, 'I', 'D', 'A', 'T');
                bool isIend = IsPngChunkType(data, offset + 4, 'I', 'E', 'N', 'D');
                // IHDR is unique and first; all other unknown critical chunks fail closed.
                if ((data[offset + 4] & 32) == 0 && !isPalette && !isIdat && !isIend) return facts;
                if (sawIdat && !isIdat) idatEnded = true;

                if (isPalette)
                {
                    if (sawPalette || sawIdat || sawTransparency || color == 0 || color == 4 ||
                        count == 0 || count > 768 || count % 3 != 0) return facts;
                    paletteEntries = count / 3;
                    if (color == 3 && paletteEntries > (1 << facts.Pixels.BitDepth)) return facts;
                    sawPalette = true;
                }
                else if (IsPngChunkType(data, offset + 4, 't', 'R', 'N', 'S'))
                {
                    if (sawTransparency || sawIdat) return facts;
                    if (color == 3)
                    {
                        if (!sawPalette || count == 0 || count > paletteEntries) return facts;
                    }
                    else if (color == 0 ? count != 2 : color == 2 ? count != 6 : true) return facts;
                    // Unused sample high bits are masked by PNG 3 decoders; encoded
                    // channels remain the IHDR channels even with palette/tRNS expansion.
                    sawTransparency = true;
                }
                else if (IsPngChunkType(data, offset + 4, 'i', 'C', 'C', 'P'))
                {
                    if (sawIcc || sawPalette || sawIdat) return facts;
                    int nameLength = ReadPngProfileNameLength(data, payload, count);
                    if (nameLength < 1 || count <= nameLength + 2 || data[payload + nameLength + 1] != 0) return facts;
                    metadata.IccProfileBytes = count - nameLength - 2;
                    sawIcc = true;
                }
                else if (IsPngChunkType(data, offset + 4, 'e', 'X', 'I', 'f'))
                {
                    if (sawExif || sawIdat) return facts;
                    sawExif = true;
                }
                else if (isIdat)
                {
                    if (idatEnded || (color == 3 && !sawPalette)) return facts;
                    sawIdat = true;
                    if (count > 0) hasImageData = true;
                }

                if (IsPngMetadataChunk(data, offset + 4)) metadata.MetadataBytes += count;
                offset += 12 + count;
                if (isIend)
                {
                    if (count != 0 || offset != data.Length || !hasImageData) return facts;
                    metadata.MetadataSizeKnown = true;
                    metadata.IccProfileSizeKnown = true;
                    facts.Metadata = metadata;
                    // Animation transitions affect frame completeness independently of
                    // stored metadata. Reuse the established APNG state machine.
                    facts.Frames = ReadPngFrameFacts(data);
                    return facts;
                }
            }
            return facts;
        }

        private static bool IsPngChunkName(byte[] data, int offset)
        {
            for (int index = 0; index < 4; ++index)
            {
                byte value = data[offset + index];
                if (!((value >= 'A' && value <= 'Z') || (value >= 'a' && value <= 'z'))) return false;
            }
            return (data[offset + 2] & 32) == 0;
        }

        private static int ReadPngProfileNameLength(byte[] data, int offset, int length)
        {
            bool previousSpace = false;
            for (int index = 0; index < length && index < 80; ++index)
            {
                byte value = data[offset + index];
                if (value == 0) return index > 0 && !previousSpace ? index : -1;
                if (value < 32 || (value > 126 && value < 161)) return -1;
                bool space = value == 32;
                if (space && (index == 0 || previousSpace)) return -1;
                previousSpace = space;
            }
            return -1;
        }
    }
}
