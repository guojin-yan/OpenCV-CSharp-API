using System;
using System.IO;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.ImgCodecs
{
    public static partial class Cv2
    {
        /// <summary>
        /// Identifies an encoded image using a managed header parser.
        /// 使用 managed 头解析器识别编码图像。
        /// </summary>
        /// <param name="buffer">The encoded image bytes. 编码图像字节。</param>
        /// <returns>Recognized format, dimensions, and proven frame facts.</returns>
        public static ImageIdentifyResult Identify(byte[] buffer)
        {
            ValidateEncodedBuffer(buffer);
            return IdentifyCore(buffer);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Identifies an encoded image span without loading the native runtime.
        /// 在不加载 native runtime 的情况下识别编码图像 Span。
        /// </summary>
        public static ImageIdentifyResult Identify(ReadOnlySpan<byte> buffer)
        {
            if (buffer.Length == 0) throw new ArgumentException("Encoded image buffer cannot be empty.", nameof(buffer));
            return IdentifyCore(buffer.ToArray());
        }
#endif

        /// <summary>
        /// Applies managed input limits before decoding an image.
        /// 在解码图像前应用 managed 输入限制。
        /// </summary>
        /// <param name="buffer">The encoded image bytes. 编码图像字节。</param>
        /// <param name="options">Managed preflight limits. Managed 预检限制。</param>
        /// <param name="flags">The image read mode. 图像读取模式。</param>
        /// <returns>The decoded image. 解码后的图像。</returns>
        public static Mat ImDecode(byte[] buffer, ImageDecodeOptions options, ImreadModes flags = ImreadModes.Color)
        {
            ValidateDecodePreflight(buffer, options);
            return ImDecode(buffer, flags);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Applies managed input limits before decoding an image span.</summary>
        public static Mat ImDecode(ReadOnlySpan<byte> buffer, ImageDecodeOptions options, ImreadModes flags = ImreadModes.Color)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            if ((long)buffer.Length > options.MaxInputBytes)
            {
                throw new InvalidDataException("Encoded image exceeds the configured input byte limit.");
            }
            ImageIdentifyResult result = Identify(buffer);
            ValidateIdentifiedInput(buffer.Length, result, options);
            return ImDecode(buffer, flags);
        }
#endif

        private static void ValidateDecodePreflight(byte[] buffer, ImageDecodeOptions options)
        {
            ValidateEncodedBuffer(buffer);
            if (options == null) throw new ArgumentNullException(nameof(options));
            if ((long)buffer.Length > options.MaxInputBytes)
            {
                throw new InvalidDataException("Encoded image exceeds the configured input byte limit.");
            }
            ValidateIdentifiedInput(buffer.Length, IdentifyCore(buffer), options);
        }

        private static void ValidateIdentifiedInput(int inputLength, ImageIdentifyResult result, ImageDecodeOptions options)
        {
            if ((long)inputLength > options.MaxInputBytes)
            {
                throw new InvalidDataException("Encoded image exceeds the configured input byte limit.");
            }

            if (options.RejectUnknownFormat && !result.IsFormatKnown)
            {
                throw new InvalidDataException("Encoded image format could not be identified safely.");
            }

            if (options.RequireKnownSize && !result.IsSizeKnown)
            {
                throw new InvalidDataException("Encoded image dimensions were not available in the header.");
            }

            if (options.RequireKnownMetadataSize && !result.IsMetadataSizeKnown)
            {
                throw new InvalidDataException("Encoded image metadata size was not available in the header.");
            }

            if (options.RequireKnownIccProfileSize && !result.IsIccProfileSizeKnown)
            {
                throw new InvalidDataException("Encoded image ICC profile size was not available in the header.");
            }

            if (options.RejectUnknownPixelFormat && !result.IsPixelFormatKnown)
            {
                throw new InvalidDataException("Encoded image depth or channel count was not available in the header.");
            }

            if (result.IsBitDepthKnown && result.BitDepth > options.MaxBitDepth)
            {
                throw new InvalidDataException("Encoded image bit depth exceeds the configured limit.");
            }

            if (result.IsChannelCountKnown && result.ChannelCount > options.MaxChannels)
            {
                throw new InvalidDataException("Encoded image channel count exceeds the configured limit.");
            }

            if (result.IsSizeKnown)
            {
                if (result.Width > options.MaxWidth || result.Height > options.MaxHeight)
                {
                    throw new InvalidDataException("Encoded image dimensions exceed the configured limit.");
                }

                long pixels = checked((long)result.Width * result.Height);
                if (pixels > options.MaxPixels)
                {
                    throw new InvalidDataException("Encoded image pixel count exceeds the configured limit.");
                }

                if (result.IsFrameCountKnown && !result.IsCumulativePixelCountKnown)
                {
                    long cumulativePixels;
                    try
                    {
                        cumulativePixels = checked(pixels * result.FrameCount);
                    }
                    catch (OverflowException)
                    {
                        throw new InvalidDataException("Encoded image cumulative pixel count exceeds the supported range.");
                    }

                    if (cumulativePixels > options.MaxCumulativePixels)
                    {
                        throw new InvalidDataException("Encoded image cumulative pixel count exceeds the configured limit.");
                    }
                }
            }

            if (result.IsCumulativePixelCountKnown && result.CumulativePixelCount > options.MaxCumulativePixels)
            {
                throw new InvalidDataException("Encoded image cumulative pixel count exceeds the configured limit.");
            }

            if (result.IsFrameCountKnown && result.FrameCount > options.MaxFrames)
            {
                throw new InvalidDataException("Encoded image frame count exceeds the configured limit.");
            }

            if (result.IsMetadataSizeKnown && result.MetadataBytes > options.MaxMetadataBytes)
            {
                throw new InvalidDataException("Encoded image metadata exceeds the configured limit.");
            }

            if (result.IsIccProfileSizeKnown && result.IccProfileBytes > options.MaxIccProfileBytes)
            {
                throw new InvalidDataException("Encoded image ICC profile exceeds the configured limit.");
            }
        }

        private static ImageIdentifyResult IdentifyCore(byte[] data)
        {
            if (StartsWith(data, new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            {
                bool hasIhdr = data.Length >= 24 &&
                    data[12] == (byte)'I' && data[13] == (byte)'H' &&
                    data[14] == (byte)'D' && data[15] == (byte)'R';
                int width = hasIhdr && ReadBe32(data, 16) <= int.MaxValue ? (int)ReadBe32(data, 16) : 0;
                int height = hasIhdr && ReadBe32(data, 20) <= int.MaxValue ? (int)ReadBe32(data, 20) : 0;
                PixelFacts pixelFacts = ReadPngPixelFacts(data, hasIhdr);
                FrameFacts frameFacts = ReadPngFrameFacts(data);
                return Result("png", width, height, width > 0 && height > 0, frameFacts.FrameCount, frameFacts.FrameCountKnown, data.Length, ReadMetadataFacts(data, "png"), pixelFacts);
            }

            if (data.Length >= 6 && data[0] == (byte)'G' && data[1] == (byte)'I' && data[2] == (byte)'F')
            {
                int width = data.Length >= 10 ? ReadLe16(data, 6) : 0;
                int height = data.Length >= 10 ? ReadLe16(data, 8) : 0;
                int frameCount;
                bool countKnown = TryCountGifFrames(data, out frameCount);
                PixelFacts pixelFacts = ReadGifPixelFacts(data);
                return Result("gif", width, height, width > 0 && height > 0, frameCount, countKnown, data.Length,
                    default(MetadataFacts), pixelFacts);
            }

            if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xD8)
            {
                int width;
                int height;
                PixelFacts pixelFacts;
                bool frameCountKnown;
                bool found = TryReadJpegSize(data, out width, out height, out pixelFacts, out frameCountKnown);
                return Result("jpeg", width, height, found, 1, frameCountKnown, data.Length, ReadMetadataFacts(data, "jpeg"), pixelFacts);
            }

            if (data.Length >= 12 && data[0] == (byte)'R' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'F' &&
                data[8] == (byte)'W' && data[9] == (byte)'E' && data[10] == (byte)'B' && data[11] == (byte)'P')
            {
                int width;
                int height;
                bool found = TryReadWebpSize(data, out width, out height);
                FrameFacts frameFacts = ReadWebpFrameFacts(data);
                PixelFacts pixelFacts = ReadWebpPixelFacts(data);
                return Result("webp", width, height, found, frameFacts.FrameCount, frameFacts.FrameCountKnown, data.Length,
                    ReadMetadataFacts(data, "webp"), pixelFacts);
            }

            if (data.Length >= 2 && data[0] == (byte)'B' && data[1] == (byte)'M')
            {
                long signedWidth = data.Length >= 22 ? ReadSignedLe32(data, 18) : 0;
                long signedHeight = data.Length >= 26 ? ReadSignedLe32(data, 22) : 0;
                long absoluteHeight = signedHeight == int.MinValue ? (long)int.MaxValue + 1 : Math.Abs(signedHeight);
                int width = signedWidth > 0 && signedWidth <= int.MaxValue ? (int)signedWidth : 0;
                int height = absoluteHeight > 0 && absoluteHeight <= int.MaxValue ? (int)absoluteHeight : 0;
                PixelFacts pixelFacts = ReadBmpPixelFacts(data);
                bool frameKnown = IsBmpHeaderComplete(data);
                return Result("bmp", width, height, width > 0 && height > 0, 1, frameKnown, data.Length,
                    default(MetadataFacts), pixelFacts);
            }

            if (data.Length >= 4 && ReadBe32(data, 0) == 0x59A66A95)
            {
                int width;
                int height;
                bool frameKnown;
                PixelFacts pixelFacts;
                bool headerKnown = TryReadSunRasterHeader(data, out width, out height, out frameKnown, out pixelFacts);
                return Result("sunraster", width, height, headerKnown, 1, frameKnown, data.Length,
                    default(MetadataFacts), pixelFacts);
            }

            if (data.Length >= 4 && ((data[0] == (byte)'I' && data[1] == (byte)'I' && data[2] == 42 && data[3] == 0) ||
                (data[0] == (byte)'M' && data[1] == (byte)'M' && data[2] == 0 && data[3] == 42)))
            {
                TiffFacts facts = ReadTiffFacts(data);
                return Result("tiff", facts.Width, facts.Height, facts.SizeKnown, facts.PageCount, facts.PageCountKnown, data.Length,
                    default(MetadataFacts), ToPixelFacts(facts), facts.CumulativePixelCount, facts.CumulativePixelCountKnown);
            }

            if (data.Length >= 4 && ((data[0] == (byte)'I' && data[1] == (byte)'I' && data[2] == 43 && data[3] == 0) ||
                (data[0] == (byte)'M' && data[1] == (byte)'M' && data[2] == 0 && data[3] == 43)))
            {
                TiffFacts facts = ReadBigTiffFacts(data);
                return Result("bigtiff", facts.Width, facts.Height, facts.SizeKnown, facts.PageCount, facts.PageCountKnown, data.Length,
                    default(MetadataFacts), ToPixelFacts(facts), facts.CumulativePixelCount, facts.CumulativePixelCountKnown);
            }

            if (data.Length >= 2 && data[0] == (byte)'P' &&
                ((data[1] >= (byte)'1' && data[1] <= (byte)'7') || data[1] == (byte)'F' || data[1] == (byte)'f'))
            {
                int width;
                int height;
                PixelFacts pixelFacts;
                bool found = TryReadPnmHeader(data, out width, out height, out pixelFacts);
                return Result("pnm", width, height, found, 1, found, data.Length, default(MetadataFacts), pixelFacts);
            }

            return Result("unknown", 0, 0, false, 0, false, data.Length);
        }

        private static ImageIdentifyResult Result(string format, int width, int height, bool sizeKnown, int frames, bool frameCountKnown, int inputLength)
        {
            return Result(format, width, height, sizeKnown, frames, frameCountKnown, inputLength, default(MetadataFacts), default(PixelFacts));
        }

        private static ImageIdentifyResult Result(string format, int width, int height, bool sizeKnown, int frames, bool frameCountKnown, int inputLength, MetadataFacts metadata)
        {
            return Result(format, width, height, sizeKnown, frames, frameCountKnown, inputLength, metadata, default(PixelFacts));
        }

        private static ImageIdentifyResult Result(string format, int width, int height, bool sizeKnown, int frames, bool frameCountKnown, int inputLength, MetadataFacts metadata, PixelFacts pixels)
        {
            long cumulativePixelCount = 0;
            bool cumulativePixelCountKnown = false;
            if (sizeKnown && frameCountKnown && width > 0 && height > 0 && frames > 0)
            {
                try
                {
                    cumulativePixelCount = checked((long)width * height * frames);
                    cumulativePixelCountKnown = true;
                }
                catch (OverflowException)
                {
                    cumulativePixelCountKnown = false;
                }
            }
            return Result(format, width, height, sizeKnown, frames, frameCountKnown, inputLength, metadata, pixels,
                cumulativePixelCount, cumulativePixelCountKnown);
        }

        private static ImageIdentifyResult Result(string format, int width, int height, bool sizeKnown, int frames, bool frameCountKnown,
            int inputLength, MetadataFacts metadata, PixelFacts pixels, long cumulativePixelCount, bool cumulativePixelCountKnown)
        {
            return new ImageIdentifyResult(format, width, height, sizeKnown, frames, frameCountKnown, inputLength, inputLength,
                metadata.MetadataBytes, metadata.MetadataSizeKnown, metadata.IccProfileBytes, metadata.IccProfileSizeKnown,
                pixels.BitDepth, pixels.BitDepthKnown, pixels.Channels, pixels.ChannelsKnown,
                cumulativePixelCount, cumulativePixelCountKnown);
        }

        private struct PixelFacts
        {
            public int BitDepth;
            public bool BitDepthKnown;
            public int Channels;
            public bool ChannelsKnown;
        }

        private static PixelFacts ReadPngPixelFacts(byte[] data, bool hasIhdr)
        {
            PixelFacts facts = new PixelFacts();
            if (!hasIhdr || data.Length < 26 || ReadBe32(data, 8) != 13) return facts;

            int bitDepth = data[24];
            int colorType = data[25];
            int channels;
            switch (colorType)
            {
                case 0: channels = 1; break;
                case 2: channels = 3; break;
                case 3: channels = 1; break;
                case 4: channels = 2; break;
                case 6: channels = 4; break;
                default: return facts;
            }

            bool validDepth = bitDepth == 1 || bitDepth == 2 || bitDepth == 4 || bitDepth == 8 || bitDepth == 16;
            if (!validDepth) return facts;
            if ((colorType == 2 || colorType == 4 || colorType == 6) && bitDepth != 8 && bitDepth != 16) return facts;
            if (colorType == 3 && bitDepth == 16) return facts;
            facts.BitDepth = bitDepth;
            facts.BitDepthKnown = true;
            facts.Channels = channels;
            facts.ChannelsKnown = true;
            return facts;
        }

        private static PixelFacts ReadGifPixelFacts(byte[] data)
        {
            PixelFacts facts = new PixelFacts();
            if (data.Length < 13 || data[0] != (byte)'G' || data[1] != (byte)'I' || data[2] != (byte)'F') return facts;

            int offset = 13;
            int globalDepth = 0;
            byte logicalScreenPacked = data[10];
            if ((logicalScreenPacked & 0x80) != 0)
            {
                globalDepth = (logicalScreenPacked & 7) + 1;
                int tableBytes = 3 * (1 << globalDepth);
                if (offset + tableBytes > data.Length) return facts;
                offset += tableBytes;
            }

            int bitDepth = 0;
            bool sawFrame = false;
            while (offset < data.Length)
            {
                byte marker = data[offset++];
                if (marker == 0x3B)
                {
                    if (!sawFrame || offset != data.Length) return new PixelFacts();
                    facts.BitDepth = bitDepth;
                    facts.BitDepthKnown = bitDepth > 0;
                    facts.Channels = 1;
                    facts.ChannelsKnown = true;
                    return facts;
                }

                if (marker == 0x2C)
                {
                    if (offset + 9 > data.Length) return new PixelFacts();
                    byte imagePacked = data[offset + 8];
                    offset += 9;
                    int frameDepth;
                    if ((imagePacked & 0x80) != 0)
                    {
                        frameDepth = (imagePacked & 7) + 1;
                        int tableBytes = 3 * (1 << frameDepth);
                        if (offset + tableBytes > data.Length) return new PixelFacts();
                        offset += tableBytes;
                    }
                    else
                    {
                        frameDepth = globalDepth;
                    }

                    if (frameDepth == 0) return new PixelFacts();
                    if (!sawFrame)
                    {
                        bitDepth = frameDepth;
                        sawFrame = true;
                    }
                    else if (bitDepth != frameDepth)
                    {
                        bitDepth = 0;
                    }

                    if (offset >= data.Length) return new PixelFacts();
                    ++offset;
                    if (!SkipGifSubBlocks(data, ref offset)) return new PixelFacts();
                    continue;
                }

                if (marker == 0x21)
                {
                    if (offset >= data.Length) return new PixelFacts();
                    byte extensionLabel = data[offset++];
                    if (extensionLabel == 0xF9)
                    {
                        if (offset + 5 >= data.Length || data[offset] != 4 || data[offset + 5] != 0) return new PixelFacts();
                        offset += 6;
                    }
                    else if (extensionLabel == 0xFF || extensionLabel == 0x01)
                    {
                        int fixedBlockLength = extensionLabel == 0xFF ? 11 : 12;
                        if (offset >= data.Length || data[offset] != fixedBlockLength || offset + 1 + fixedBlockLength > data.Length) return new PixelFacts();
                        offset += 1 + fixedBlockLength;
                        if (!SkipGifSubBlocks(data, ref offset)) return new PixelFacts();
                    }
                    else if (!SkipGifSubBlocks(data, ref offset))
                    {
                        return new PixelFacts();
                    }
                    continue;
                }

                return new PixelFacts();
            }

            return facts;
        }

        private static PixelFacts ReadWebpPixelFacts(byte[] data)
        {
            PixelFacts facts = new PixelFacts();
            if (data.Length < 12 || data[0] != (byte)'R' || data[1] != (byte)'I' || data[2] != (byte)'F' || data[3] != (byte)'F' ||
                data[8] != (byte)'W' || data[9] != (byte)'E' || data[10] != (byte)'B' || data[11] != (byte)'P') return facts;

            uint declaredLength = (uint)(data[4] | (data[5] << 8) | (data[6] << 16) | (data[7] << 24));
            if (declaredLength != data.Length - 8) return facts;

            bool sawVp8x = false;
            bool sawImage = false;
            bool alphaKnown = false;
            bool hasAlpha = false;
            int offset = 12;
            while (offset + 8 <= data.Length)
            {
                int chunkLength = ReadSignedLe32(data, offset + 4);
                if (chunkLength < 0 || offset + 8L + chunkLength > data.Length) return new PixelFacts();

                bool isVp8x = IsWebpChunkType(data, offset, 'V', 'P', '8', 'X');
                bool isVp8 = IsWebpChunkType(data, offset, 'V', 'P', '8', ' ');
                bool isVp8l = IsWebpChunkType(data, offset, 'V', 'P', '8', 'L');
                bool isAnmf = IsWebpChunkType(data, offset, 'A', 'N', 'M', 'F');
                if (isVp8x)
                {
                    if (sawVp8x || chunkLength != 10) return new PixelFacts();
                    sawVp8x = true;
                    hasAlpha = (data[offset + 8] & 0x10) != 0;
                    alphaKnown = true;
                }
                else if (isVp8)
                {
                    if (chunkLength < 6 || data[offset + 11] != 0x9D || data[offset + 12] != 0x01 || data[offset + 13] != 0x2A)
                    {
                        return new PixelFacts();
                    }
                    sawImage = true;
                }
                else if (isVp8l)
                {
                    if (chunkLength < 5 || data[offset + 8] != 0x2F) return new PixelFacts();
                    bool losslessAlpha = (data[offset + 12] & 0x10) != 0;
                    if (alphaKnown && hasAlpha != losslessAlpha && sawVp8x) return new PixelFacts();
                    hasAlpha = losslessAlpha;
                    alphaKnown = true;
                    sawImage = true;
                }
                else if (isAnmf)
                {
                    if (chunkLength < 16) return new PixelFacts();
                    sawImage = true;
                }

                offset += 8 + chunkLength + (chunkLength & 1);
            }

            if (offset != data.Length || !sawImage) return facts;
            if (!alphaKnown) hasAlpha = false;
            facts.BitDepth = 8;
            facts.BitDepthKnown = true;
            facts.Channels = hasAlpha ? 4 : 3;
            facts.ChannelsKnown = true;
            return facts;
        }

        private static PixelFacts ReadBmpPixelFacts(byte[] data)
        {
            PixelFacts facts = new PixelFacts();
            if (!IsBmpHeaderComplete(data) || data.Length < 26) return facts;

            int dibSize = ReadSignedLe32(data, 14);
            int bitCount;
            int compression = 0;
            if (dibSize == 12)
            {
                bitCount = ReadLe16(data, 24);
            }
            else
            {
                if (dibSize < 40 || data.Length < 34) return facts;
                bitCount = ReadLe16(data, 28);
                compression = ReadSignedLe32(data, 30);
            }

            if (compression != 0) return facts;
            switch (bitCount)
            {
                case 1:
                case 4:
                case 8:
                    facts.BitDepth = bitCount;
                    facts.BitDepthKnown = true;
                    facts.Channels = 1;
                    facts.ChannelsKnown = true;
                    break;
                case 24:
                case 32:
                    facts.BitDepth = 8;
                    facts.BitDepthKnown = true;
                    facts.Channels = 3;
                    facts.ChannelsKnown = true;
                    break;
            }
            return facts;
        }

        private static bool IsBmpHeaderComplete(byte[] data)
        {
            if (data.Length < 26 || data[0] != (byte)'B' || data[1] != (byte)'M') return false;
            int dibSize = ReadSignedLe32(data, 14);
            long dibEnd;
            if (dibSize == 12)
            {
                dibEnd = 26;
            }
            else
            {
                if (dibSize < 40) return false;
                dibEnd = 14L + dibSize;
                if (dibEnd > data.Length) return false;
            }

            int fileSize = ReadSignedLe32(data, 2);
            int pixelOffset = ReadSignedLe32(data, 10);
            return fileSize > pixelOffset && pixelOffset >= dibEnd && fileSize <= data.Length;
        }

        private static bool TryReadSunRasterHeader(byte[] data, out int width, out int height, out bool frameKnown, out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            frameKnown = false;
            pixelFacts = new PixelFacts();
            if (data.Length < 32 || ReadBe32(data, 0) != 0x59A66A95) return false;

            uint unsignedWidth = ReadBe32(data, 4);
            uint unsignedHeight = ReadBe32(data, 8);
            uint depth = ReadBe32(data, 12);
            uint payloadLength = ReadBe32(data, 16);
            uint type = ReadBe32(data, 20);
            uint mapType = ReadBe32(data, 24);
            uint mapLength = ReadBe32(data, 28);
            if (unsignedWidth == 0 || unsignedHeight == 0 || unsignedWidth > int.MaxValue || unsignedHeight > int.MaxValue)
            {
                return false;
            }

            width = (int)unsignedWidth;
            height = (int)unsignedHeight;
            long payloadEnd = 32L + mapLength + payloadLength;
            bool mapKnown = mapType <= 2 && (mapType != 0 || mapLength == 0) && (mapType != 1 || mapLength % 3 == 0);
            frameKnown = payloadLength > 0 && payloadEnd <= data.Length && type <= 3 && mapKnown;
            if (!frameKnown) return true;

            switch (depth)
            {
                case 1:
                case 8:
                    pixelFacts.BitDepth = (int)depth;
                    pixelFacts.BitDepthKnown = true;
                    pixelFacts.Channels = 1;
                    pixelFacts.ChannelsKnown = true;
                    break;
                case 24:
                    pixelFacts.BitDepth = 8;
                    pixelFacts.BitDepthKnown = true;
                    pixelFacts.Channels = 3;
                    pixelFacts.ChannelsKnown = true;
                    break;
                case 32:
                    pixelFacts.BitDepth = 8;
                    pixelFacts.BitDepthKnown = true;
                    pixelFacts.Channels = 4;
                    pixelFacts.ChannelsKnown = true;
                    break;
            }
            return true;
        }

        private static bool StartsWith(byte[] data, byte[] prefix)
        {
            if (data.Length < prefix.Length) return false;
            for (int index = 0; index < prefix.Length; ++index) if (data[index] != prefix[index]) return false;
            return true;
        }

        private static uint ReadBe32(byte[] data, int offset)
        {
            return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) | ((uint)data[offset + 2] << 8) | data[offset + 3];
        }

        private static int ReadLe16(byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8);
        }

        private static int ReadSignedLe32(byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
        }

        private static bool TryReadJpegSize(byte[] data, out int width, out int height, out PixelFacts pixelFacts, out bool frameCountKnown)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();
            frameCountKnown = false;
            bool sawStartOfFrame = false;
            int offset = 2;
            while (offset < data.Length)
            {
                while (offset < data.Length && data[offset] == 0xFF) ++offset;
                if (offset >= data.Length) break;
                int marker = data[offset++];
                if (marker == 0xD9)
                {
                    frameCountKnown = sawStartOfFrame && offset == data.Length;
                    break;
                }
                if (marker == 0xDA)
                {
                    if (offset + 1 >= data.Length) break;
                    int scanLength = (data[offset] << 8) | data[offset + 1];
                    if (scanLength < 2 || offset + scanLength > data.Length) break;
                    offset += scanLength;
                    frameCountKnown = sawStartOfFrame && ContainsJpegEndOfImage(data, offset);
                    break;
                }
                if (marker >= 0xD0 && marker <= 0xD7 || marker == 0x01) continue;
                if (offset + 1 >= data.Length) break;
                int length = (data[offset] << 8) | data[offset + 1];
                if (length < 2 || offset + length > data.Length) break;
                if (IsJpegStartOfFrame(marker) && length >= 7)
                {
                    int precision = data[offset + 2];
                    int channels = data[offset + 7];
                    height = (data[offset + 3] << 8) | data[offset + 4];
                    width = (data[offset + 5] << 8) | data[offset + 6];
                    if (precision > 0) pixelFacts.BitDepth = precision;
                    pixelFacts.BitDepthKnown = precision > 0;
                    if (channels > 0) pixelFacts.Channels = channels;
                    pixelFacts.ChannelsKnown = channels > 0;
                    sawStartOfFrame = width > 0 && height > 0;
                }
                offset += length;
            }
            return sawStartOfFrame;
        }

        private static bool ContainsJpegEndOfImage(byte[] data, int offset)
        {
            while (offset + 1 < data.Length)
            {
                if (data[offset++] != 0xFF) continue;
                while (offset < data.Length && data[offset] == 0xFF) ++offset;
                if (offset >= data.Length) return false;
                byte marker = data[offset++];
                if (marker == 0x00 || (marker >= 0xD0 && marker <= 0xD7)) continue;
                if (marker == 0xD9) return offset == data.Length;
            }
            return false;
        }

        private static bool IsJpegStartOfFrame(int marker)
        {
            return (marker >= 0xC0 && marker <= 0xC3) || (marker >= 0xC5 && marker <= 0xC7) ||
                (marker >= 0xC9 && marker <= 0xCB) || (marker >= 0xCD && marker <= 0xCF);
        }

        private static bool TryReadWebpSize(byte[] data, out int width, out int height)
        {
            width = 0;
            height = 0;
            int offset = 12;
            while (offset + 8 <= data.Length)
            {
                int chunkSize = ReadSignedLe32(data, offset + 4);
                if (chunkSize < 0 || offset + 8 > data.Length || offset + 8L + chunkSize > data.Length) break;
                if (data[offset] == (byte)'V' && data[offset + 1] == (byte)'P' && data[offset + 2] == (byte)'8' && data[offset + 3] == (byte)'X' && chunkSize >= 10)
                {
                    width = 1 + data[offset + 12] + (data[offset + 13] << 8) + (data[offset + 14] << 16);
                    height = 1 + data[offset + 15] + (data[offset + 16] << 8) + (data[offset + 17] << 16);
                    return width > 0 && height > 0;
                }
                if (data[offset] == (byte)'V' && data[offset + 1] == (byte)'P' && data[offset + 2] == (byte)'8' && data[offset + 3] == (byte)' ' && chunkSize >= 10 && offset + 30 <= data.Length && data[offset + 14] == 0x9D && data[offset + 15] == 0x01 && data[offset + 16] == 0x2A)
                {
                    width = ReadLe16(data, offset + 24) & 0x3FFF;
                    height = ReadLe16(data, offset + 26) & 0x3FFF;
                    return width > 0 && height > 0;
                }
                offset += 8 + chunkSize + (chunkSize & 1);
            }
            return false;
        }

        private static bool TryReadPnmHeader(byte[] data, out int width, out int height, out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();
            if (data.Length < 3 || data[0] != (byte)'P' ||
                !((data[1] >= (byte)'1' && data[1] <= (byte)'7') || data[1] == (byte)'F' || data[1] == (byte)'f') ||
                !IsPnmWhitespace(data[2])) return false;

            int kind = data[1] >= (byte)'1' && data[1] <= (byte)'7' ? data[1] - (byte)'0' : data[1] == (byte)'F' ? 8 : 9;
            int offset = 2;
            if (kind == 7) return TryReadPamHeader(data, ref offset, out width, out height, out pixelFacts);

            int maxValue;
            if (!TryReadPnmInteger(data, ref offset, out width) || !TryReadPnmInteger(data, ref offset, out height) ||
                width <= 0 || height <= 0) return false;

            if (kind == 8 || kind == 9)
            {
                if (!TryReadPfmScale(data, ref offset))
                {
                    width = 0;
                    height = 0;
                    return false;
                }
                pixelFacts.BitDepth = 32;
                pixelFacts.BitDepthKnown = true;
                pixelFacts.Channels = kind == 8 ? 3 : 1;
                pixelFacts.ChannelsKnown = true;
                return true;
            }

            if (kind == 1 || kind == 4)
            {
                if (!HasPnmTokenSeparator(data, offset)) return false;
                pixelFacts.BitDepth = 1;
                pixelFacts.BitDepthKnown = true;
                pixelFacts.Channels = 1;
                pixelFacts.ChannelsKnown = true;
                return true;
            }

            if (!TryReadPnmInteger(data, ref offset, out maxValue) || maxValue < 1 || maxValue > 65535)
            {
                width = 0;
                height = 0;
                return false;
            }
            if (!HasPnmTokenSeparator(data, offset))
            {
                width = 0;
                height = 0;
                return false;
            }

            pixelFacts.BitDepth = maxValue <= 255 ? 8 : 16;
            pixelFacts.BitDepthKnown = true;
            pixelFacts.Channels = kind == 3 || kind == 6 ? 3 : 1;
            pixelFacts.ChannelsKnown = true;
            return true;
        }

        private static bool TryReadPamHeader(byte[] data, ref int offset, out int width, out int height, out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();
            int depth = 0;
            int maxValue = 0;
            bool widthKnown = false;
            bool heightKnown = false;
            bool depthKnown = false;
            bool maxValueKnown = false;

            while (offset < data.Length)
            {
                int lineEnd = offset;
                while (lineEnd < data.Length && data[lineEnd] != 10) ++lineEnd;
                if (lineEnd >= data.Length) return false;

                int tokenStart = offset;
                while (tokenStart < lineEnd && IsPnmWhitespace(data[tokenStart])) ++tokenStart;
                if (tokenStart == lineEnd || data[tokenStart] == (byte)'#')
                {
                    offset = lineEnd + 1;
                    continue;
                }

                int tokenEnd = tokenStart;
                while (tokenEnd < lineEnd && !IsPnmWhitespace(data[tokenEnd])) ++tokenEnd;
                if (TokenEquals(data, tokenStart, tokenEnd, "ENDHDR"))
                {
                    if (tokenEnd != lineEnd && data[tokenEnd] != (byte)'#') return false;
                    if (!widthKnown || !heightKnown || !depthKnown || !maxValueKnown || width <= 0 || height <= 0 || depth <= 0 || depth > 4 || maxValue < 1 || maxValue > 65535)
                    {
                        return false;
                    }
                    pixelFacts.BitDepth = maxValue <= 255 ? 8 : 16;
                    pixelFacts.BitDepthKnown = true;
                    pixelFacts.Channels = depth;
                    pixelFacts.ChannelsKnown = true;
                    offset = lineEnd + 1;
                    return true;
                }

                bool isWidth = TokenEquals(data, tokenStart, tokenEnd, "WIDTH");
                bool isHeight = TokenEquals(data, tokenStart, tokenEnd, "HEIGHT");
                bool isDepth = TokenEquals(data, tokenStart, tokenEnd, "DEPTH");
                bool isMaxValue = TokenEquals(data, tokenStart, tokenEnd, "MAXVAL");
                if (!isWidth && !isHeight && !isDepth && !isMaxValue)
                {
                    offset = lineEnd + 1;
                    continue;
                }

                int valueOffset = tokenEnd;
                while (valueOffset < lineEnd && IsPnmWhitespace(data[valueOffset])) ++valueOffset;
                int value;
                if (!TryReadPnmIntegerBounded(data, ref valueOffset, lineEnd, out value) ||
                    (valueOffset < lineEnd && data[valueOffset] != (byte)'#')) return false;
                if (isWidth)
                {
                    width = value;
                    widthKnown = true;
                }
                else if (isHeight)
                {
                    height = value;
                    heightKnown = true;
                }
                else if (isDepth)
                {
                    depth = value;
                    depthKnown = true;
                }
                else if (isMaxValue)
                {
                    maxValue = value;
                    maxValueKnown = true;
                }
                offset = lineEnd + 1;
            }
            return false;
        }

        private static bool TryReadPfmScale(byte[] data, ref int offset)
        {
            while (offset < data.Length && (IsPnmWhitespace(data[offset]) || data[offset] == (byte)'#'))
            {
                if (data[offset] == (byte)'#')
                {
                    while (offset < data.Length && data[offset] != 10) ++offset;
                }
                else
                {
                    ++offset;
                }
            }
            int start = offset;
            while (offset < data.Length && !IsPnmWhitespace(data[offset])) ++offset;
            if (start == offset || !HasPnmTokenSeparator(data, offset)) return false;
            bool digit = false;
            bool decimalPoint = false;
            bool exponent = false;
            bool exponentDigit = false;
            for (int index = start; index < offset; ++index)
            {
                byte value = data[index];
                if (value >= (byte)'0' && value <= (byte)'9')
                {
                    digit = true;
                    if (exponent) exponentDigit = true;
                }
                else if ((value == (byte)'+' || value == (byte)'-') && (index == start || data[index - 1] == (byte)'e' || data[index - 1] == (byte)'E'))
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
            }
            if (!digit || exponent && !exponentDigit) return false;
            string token = System.Text.Encoding.ASCII.GetString(data, start, offset - start);
            double scale;
            if (!double.TryParse(token, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out scale)) return false;
            return scale != 0 && !double.IsNaN(scale) && !double.IsInfinity(scale);
        }

        private static bool TryReadPnmIntegerBounded(byte[] data, ref int offset, int end, out int value)
        {
            value = 0;
            if (offset >= end || data[offset] < (byte)'0' || data[offset] > (byte)'9') return false;
            long parsed = 0;
            while (offset < end && data[offset] >= (byte)'0' && data[offset] <= (byte)'9')
            {
                parsed = parsed * 10 + data[offset] - (byte)'0';
                if (parsed > int.MaxValue) return false;
                ++offset;
            }
            value = (int)parsed;
            return true;
        }

        private static bool TokenEquals(byte[] data, int start, int end, string token)
        {
            if (end - start != token.Length) return false;
            for (int index = 0; index < token.Length; ++index)
            {
                if (data[start + index] != (byte)token[index]) return false;
            }
            return true;
        }

        private static bool TryReadPnmInteger(byte[] data, ref int offset, out int value)
        {
            value = 0;
            while (offset < data.Length)
            {
                if (IsPnmWhitespace(data[offset]))
                {
                    ++offset;
                    continue;
                }
                if (data[offset] != (byte)'#') break;
                while (offset < data.Length && data[offset] != 10) ++offset;
            }
            if (offset >= data.Length || data[offset] < (byte)'0' || data[offset] > (byte)'9') return false;

            long parsed = 0;
            while (offset < data.Length && data[offset] >= (byte)'0' && data[offset] <= (byte)'9')
            {
                parsed = parsed * 10 + data[offset] - (byte)'0';
                if (parsed > int.MaxValue) return false;
                ++offset;
            }
            if (offset < data.Length && !IsPnmWhitespace(data[offset]) && data[offset] != (byte)'#') return false;
            value = (int)parsed;
            return true;
        }

        private static bool IsPnmWhitespace(byte value)
        {
            return value == 32 || value == 9 || value == 10 || value == 11 || value == 12 || value == 13;
        }

        private static bool HasPnmTokenSeparator(byte[] data, int offset)
        {
            return offset < data.Length && (IsPnmWhitespace(data[offset]) || data[offset] == (byte)'#');
        }

        private static bool TryCountGifFrames(byte[] data, out int frameCount)
        {
            frameCount = 0;
            if (data.Length < 13) return false;
            int offset = 13;
            if ((data[10] & 0x80) != 0)
            {
                int globalColorTableBytes = 3 * (1 << ((data[10] & 7) + 1));
                if (offset + globalColorTableBytes > data.Length) return false;
                offset += globalColorTableBytes;
            }
            while (offset < data.Length)
            {
                byte marker = data[offset++];
                if (marker == 0x3B) return frameCount > 0 && offset == data.Length;
                if (marker == 0x2C)
                {
                    if (offset + 9 > data.Length) return false;
                    byte packed = data[offset + 8];
                    offset += 9;
                    if ((packed & 0x80) != 0)
                    {
                        int tableBytes = 3 * (1 << ((packed & 7) + 1));
                        if (offset + tableBytes > data.Length) return false;
                        offset += tableBytes;
                    }
                    if (offset >= data.Length) return false;
                    ++offset;
                    if (!SkipGifSubBlocks(data, ref offset)) return false;
                    ++frameCount;
                    continue;
                }
                if (marker == 0x21)
                {
                    if (offset >= data.Length) return false;
                    byte extensionLabel = data[offset++];
                    if (extensionLabel == 0xF9)
                    {
                        if (offset + 5 >= data.Length || data[offset] != 4 || data[offset + 5] != 0) return false;
                        offset += 6;
                    }
                    else if (extensionLabel == 0xFF || extensionLabel == 0x01)
                    {
                        int fixedBlockLength = extensionLabel == 0xFF ? 11 : 12;
                        if (offset >= data.Length || data[offset] != fixedBlockLength || offset + 1 + fixedBlockLength > data.Length) return false;
                        offset += 1 + fixedBlockLength;
                        if (!SkipGifSubBlocks(data, ref offset)) return false;
                    }
                    else if (!SkipGifSubBlocks(data, ref offset)) return false;
                    continue;
                }
                return false;
            }
            return false;
        }

        private static bool SkipGifSubBlocks(byte[] data, ref int offset)
        {
            while (offset < data.Length)
            {
                int length = data[offset++];
                if (length == 0) return true;
                if (offset + length > data.Length) return false;
                offset += length;
            }
            return false;
        }
    }
}
