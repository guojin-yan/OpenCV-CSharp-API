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

            if (result.IsCumulativePixelCountKnown && result.IsPixelFormatKnown)
            {
                if (!result.IsEstimatedPixelBytesKnown || result.EstimatedPixelBytes > options.MaxEstimatedPixelBytes)
                {
                    throw new InvalidDataException("Estimated encoded pixel storage exceeds the configured limit or supported range.");
                }
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
                PngFacts facts = ReadPngFacts(data);
                return Result("png", facts.Width, facts.Height, facts.SizeKnown, facts.Frames.FrameCount,
                    facts.Frames.FrameCountKnown, data.Length, facts.Metadata, facts.Pixels);
            }

            if (data.Length >= 6 && data[0] == (byte)'G' && data[1] == (byte)'I' && data[2] == (byte)'F')
            {
                int width = data.Length >= 10 ? ReadLe16(data, 6) : 0;
                int height = data.Length >= 10 ? ReadLe16(data, 8) : 0;
                int frameCount;
                MetadataFacts metadataFacts;
                bool countKnown = TryReadGifFrameAndMetadataFacts(data, out frameCount, out metadataFacts);
                PixelFacts pixelFacts = ReadGifPixelFacts(data);
                return Result("gif", width, height, width > 0 && height > 0, frameCount, countKnown, data.Length,
                    metadataFacts, pixelFacts);
            }

            if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xD8)
            {
                int width;
                int height;
                PixelFacts pixelFacts;
                bool frameCountKnown;
                MetadataFacts metadataFacts;
                bool found = TryReadJpegFacts(data, out width, out height, out pixelFacts, out frameCountKnown, out metadataFacts);
                return Result("jpeg", width, height, found, 1, frameCountKnown, data.Length, metadataFacts, pixelFacts);
            }

            if (data.Length >= 12 && data[0] == (byte)'R' && data[1] == (byte)'I' && data[2] == (byte)'F' && data[3] == (byte)'F' &&
                data[8] == (byte)'W' && data[9] == (byte)'E' && data[10] == (byte)'B' && data[11] == (byte)'P')
            {
                int width;
                int height;
                bool found = TryReadWebpSize(data, out width, out height);
                WebpContainerFacts facts = ReadWebpContainerFacts(data);
                return Result("webp", width, height, found, facts.Frames.FrameCount, facts.Frames.FrameCountKnown, data.Length,
                    ReadMetadataFacts(data, "webp"), facts.Pixels);
            }

            if (IsAvifSignature(data))
            {
                int width;
                int height;
                PixelFacts pixelFacts;
                bool headerKnown = TryReadAvifHeader(data, out width, out height, out pixelFacts);
                return Result("avif", width, height, headerKnown, 0, false, data.Length,
                    default(MetadataFacts), pixelFacts);
            }

            if (data.Length >= 2 && data[0] == (byte)'B' && data[1] == (byte)'M')
            {
                BmpFacts facts = ReadBmpFacts(data);
                return Result("bmp", facts.Width, facts.Height, facts.SizeKnown, 1, facts.FrameKnown, data.Length,
                    default(MetadataFacts), facts.Pixels);
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

            if (StartsWith(data, new byte[] { (byte)'#', (byte)'?', (byte)'R', (byte)'G', (byte)'B', (byte)'E' }) ||
                StartsWith(data, new byte[] { (byte)'#', (byte)'?', (byte)'R', (byte)'A', (byte)'D', (byte)'I', (byte)'A', (byte)'N', (byte)'C', (byte)'E' }))
            {
                int width;
                int height;
                int pixelDataOffset;
                PixelFacts pixelFacts;
                bool headerKnown = TryReadRadianceHdrHeader(data, out width, out height, out pixelDataOffset, out pixelFacts);
                bool frameKnown = headerKnown && IsRadianceHdrPayloadComplete(data, pixelDataOffset, width, height);
                return Result("hdr", width, height, headerKnown, 1, frameKnown, data.Length,
                    default(MetadataFacts), pixelFacts);
            }

            if (data.Length >= 4 && data[0] == 0x76 && data[1] == 0x2F && data[2] == 0x31 && data[3] == 0x01)
            {
                int width;
                int height;
                PixelFacts pixelFacts;
                bool headerKnown = TryReadOpenExrHeader(data, out width, out height, out pixelFacts);
                return Result("exr", width, height, headerKnown, 1, false, data.Length,
                    default(MetadataFacts), pixelFacts);
            }

            if (IsJpeg2000Codestream(data) || IsJp2Container(data))
            {
                int width;
                int height;
                PixelFacts pixelFacts;
                bool isJp2 = IsJp2Container(data);
                bool headerKnown = TryReadJpeg2000Facts(data, isJp2, out width, out height, out pixelFacts);
                return Result(isJp2 ? "jp2" : "j2k", width, height, headerKnown, 1, false, data.Length,
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
                bool frameKnown;
                bool found = TryReadPnmHeader(data, out width, out height, out pixelFacts, out frameKnown);
                return Result("pnm", width, height, found, 1, found && frameKnown, data.Length, default(MetadataFacts), pixelFacts);
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
            EstimatedPixelBytesFacts estimatedPixelBytes = GetEstimatedPixelBytes(cumulativePixelCount, cumulativePixelCountKnown, pixels);
            return new ImageIdentifyResult(format, width, height, sizeKnown, frames, frameCountKnown, inputLength, inputLength,
                metadata.MetadataBytes, metadata.MetadataSizeKnown, metadata.IccProfileBytes, metadata.IccProfileSizeKnown,
                pixels.BitDepth, pixels.BitDepthKnown, pixels.Channels, pixels.ChannelsKnown,
                cumulativePixelCount, cumulativePixelCountKnown,
                estimatedPixelBytes.Value, estimatedPixelBytes.Known);
        }

        private static EstimatedPixelBytesFacts GetEstimatedPixelBytes(long cumulativePixelCount, bool cumulativePixelCountKnown, PixelFacts pixels)
        {
            if (!cumulativePixelCountKnown || !pixels.BitDepthKnown || !pixels.ChannelsKnown)
            {
                return new EstimatedPixelBytesFacts();
            }

            try
            {
                long bytesPerSample = (pixels.BitDepth + 7L) / 8L;
                long estimated = checked(checked(cumulativePixelCount * bytesPerSample) * pixels.Channels);
                return new EstimatedPixelBytesFacts { Value = estimated, Known = true };
            }
            catch (OverflowException)
            {
                return new EstimatedPixelBytesFacts();
            }
        }

        private struct EstimatedPixelBytesFacts
        {
            public long Value;
            public bool Known;
        }

        private struct PixelFacts
        {
            public int BitDepth;
            public bool BitDepthKnown;
            public int Channels;
            public bool ChannelsKnown;
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
            bool mapKnown = mapType <= 2 && (mapType != 0 || mapLength == 0) && (mapType != 1 || mapLength % 3 == 0);

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

            if (!mapKnown || type > 3) return true;
            try
            {
                long payloadOffset = checked(32L + mapLength);
                if (payloadOffset > data.Length) return true;

                long expectedRawPayload = 0;
                if (type == 0 || type == 1 || type == 3)
                {
                    if (depth != 1 && depth != 8 && depth != 24 && depth != 32) return true;
                    long rowBits = checked((long)width * depth);
                    long rowBytes = checked(((rowBits + 15) / 16) * 2);
                    expectedRawPayload = checked(rowBytes * height);
                }

                long declaredPayload = payloadLength;
                if (type == 0 && declaredPayload == 0) declaredPayload = expectedRawPayload;
                if (declaredPayload <= 0) return true;
                long payloadEnd = checked(payloadOffset + declaredPayload);
                if (payloadEnd != data.Length) return true;
                if (type == 2)
                {
                    if (depth != 8) return true;
                    long rowBits = checked((long)width * depth);
                    long rowBytes = checked(((rowBits + 15) / 16) * 2);
                    long expectedDecodedBytes = checked(rowBytes * height);
                    frameKnown = expectedDecodedBytes <= int.MaxValue &&
                        IsSunRasterRlePayloadComplete(data, (int)payloadOffset, data.Length, expectedDecodedBytes);
                }
                else
                {
                    frameKnown = declaredPayload == expectedRawPayload;
                }
            }
            catch (OverflowException)
            {
                frameKnown = false;
            }
            return true;
        }

        private static bool IsSunRasterRlePayloadComplete(byte[] data, int offset, int end, long expectedDecodedBytes)
        {
            if (offset < 0 || offset > end || end > data.Length || expectedDecodedBytes <= 0) return false;
            long decodedBytes = 0;
            while (decodedBytes < expectedDecodedBytes)
            {
                if (offset >= end) return false;
                byte value = data[offset++];
                if (value != 0x80)
                {
                    ++decodedBytes;
                    continue;
                }

                if (offset >= end) return false;
                int runLength = data[offset++];
                if (runLength == 0)
                {
                    ++decodedBytes;
                    continue;
                }
                if (offset >= end) return false;
                ++offset;
                decodedBytes = checked(decodedBytes + runLength + 1L);
                if (decodedBytes > expectedDecodedBytes) return false;
            }
            return decodedBytes == expectedDecodedBytes && offset == end;
        }

        private static bool TryReadRadianceHdrHeader(byte[] data, out int width, out int height, out int pixelDataOffset, out PixelFacts pixelFacts)
        {
            width = 0;
            height = 0;
            pixelDataOffset = 0;
            pixelFacts = new PixelFacts();

            int offset = 0;
            int lineStart;
            int lineEnd;
            if (!TryReadRadianceHdrLine(data, ref offset, out lineStart, out lineEnd)) return false;

            bool hasFormat = false;
            bool foundHeaderEnd = false;
            while (TryReadRadianceHdrLine(data, ref offset, out lineStart, out lineEnd))
            {
                if (lineStart == lineEnd)
                {
                    foundHeaderEnd = true;
                    break;
                }
                if (TokenEquals(data, lineStart, lineEnd, "FORMAT=32-bit_rle_rgbe")) hasFormat = true;
            }
            if (!hasFormat || !foundHeaderEnd ||
                !TryReadRadianceHdrLine(data, ref offset, out lineStart, out lineEnd) ||
                !TryReadRadianceResolution(data, lineStart, lineEnd, out width, out height))
            {
                width = 0;
                height = 0;
                return false;
            }

            pixelDataOffset = offset;
            pixelFacts.BitDepth = 8;
            pixelFacts.BitDepthKnown = true;
            pixelFacts.Channels = 4;
            pixelFacts.ChannelsKnown = true;
            return true;
        }

        private static bool TryReadRadianceHdrLine(byte[] data, ref int offset, out int lineStart, out int lineEnd)
        {
            lineStart = offset;
            lineEnd = offset;
            while (lineEnd < data.Length && data[lineEnd] != 10) ++lineEnd;
            if (lineEnd >= data.Length || lineEnd - lineStart + 1 > 127) return false;
            offset = lineEnd + 1;
            return true;
        }

        private static bool TryReadRadianceResolution(byte[] data, int start, int end, out int width, out int height)
        {
            width = 0;
            height = 0;
            int offset = start;
            if (offset + 2 > end || data[offset] != (byte)'-' || data[offset + 1] != (byte)'Y') return false;
            offset += 2;
            if (!SkipRadianceWhitespace(data, ref offset, end) ||
                !TryReadPositiveAsciiInteger(data, ref offset, end, out height) ||
                !SkipRadianceWhitespace(data, ref offset, end) || offset + 2 > end ||
                data[offset] != (byte)'+' || data[offset + 1] != (byte)'X')
            {
                width = 0;
                height = 0;
                return false;
            }
            offset += 2;
            if (!SkipRadianceWhitespace(data, ref offset, end) ||
                !TryReadPositiveAsciiInteger(data, ref offset, end, out width))
            {
                width = 0;
                height = 0;
                return false;
            }
            while (offset < end && IsRadianceWhitespace(data[offset])) ++offset;
            return offset == end;
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

        private static bool IsRadianceHdrPayloadComplete(byte[] data, int offset, int width, int height)
        {
            if (offset < 0 || offset > data.Length || width <= 0 || height <= 0) return false;

            if (width < 8 || width > 0x7FFF)
            {
                return HasExactRadianceFlatPayload(data, offset, width, height);
            }

            int remainingScanlines = height;
            while (remainingScanlines > 0)
            {
                if (offset + 4 > data.Length) return false;
                bool isRleScanline = data[offset] == 2 && data[offset + 1] == 2 && (data[offset + 2] & 0x80) == 0;
                if (!isRleScanline)
                {
                    return HasExactRadianceFlatPayload(data, offset, width, remainingScanlines);
                }
                if ((data[offset + 2] << 8 | data[offset + 3]) != width) return false;
                offset += 4;

                for (int channel = 0; channel < 4; ++channel)
                {
                    int decoded = 0;
                    while (decoded < width)
                    {
                        if (offset >= data.Length) return false;
                        int count = data[offset++];
                        if (count > 128)
                        {
                            count -= 128;
                            if (count > width - decoded || offset >= data.Length) return false;
                            ++offset;
                        }
                        else
                        {
                            if (count == 0 || count > width - decoded || offset + count > data.Length) return false;
                            offset += count;
                        }
                        decoded += count;
                    }
                }
                --remainingScanlines;
            }
            return offset == data.Length;
        }

        private static bool HasExactRadianceFlatPayload(byte[] data, int offset, int width, int scanlines)
        {
            long expectedBytes;
            try
            {
                expectedBytes = checked((long)width * scanlines * 4);
            }
            catch (OverflowException)
            {
                return false;
            }
            return (long)data.Length - offset == expectedBytes;
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

        private static uint ReadUnsignedLe32(byte[] data, int offset)
        {
            return (uint)(data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24));
        }

        private static int ReadSignedLe32(byte[] data, int offset)
        {
            return data[offset] | (data[offset + 1] << 8) | (data[offset + 2] << 16) | (data[offset + 3] << 24);
        }

        private static bool TryReadPnmHeader(byte[] data, out int width, out int height, out PixelFacts pixelFacts, out bool frameKnown)
        {
            width = 0;
            height = 0;
            pixelFacts = new PixelFacts();
            frameKnown = false;
            if (data.Length < 3 || data[0] != (byte)'P' ||
                !((data[1] >= (byte)'1' && data[1] <= (byte)'7') || data[1] == (byte)'F' || data[1] == (byte)'f') ||
                !IsPnmWhitespace(data[2])) return false;

            int kind = data[1] >= (byte)'1' && data[1] <= (byte)'7' ? data[1] - (byte)'0' : data[1] == (byte)'F' ? 8 : 9;
            int offset = 2;
            if (kind == 7)
            {
                bool found = TryReadPamHeader(data, ref offset, out width, out height, out pixelFacts);
                frameKnown = found && IsPamPayloadComplete(data, offset, width, height, pixelFacts.Channels, pixelFacts.BitDepth);
                return found;
            }

            int maxValue;
            if (!TryReadPnmInteger(data, ref offset, out width) || !TryReadPnmInteger(data, ref offset, out height) ||
                width <= 0 || height <= 0) return false;

            if (kind == 8 || kind == 9)
            {
                int payloadOffset;
                if (!TryReadPfmScale(data, ref offset, out payloadOffset))
                {
                    width = 0;
                    height = 0;
                    return false;
                }
                pixelFacts.BitDepth = 32;
                pixelFacts.BitDepthKnown = true;
                pixelFacts.Channels = kind == 8 ? 3 : 1;
                pixelFacts.ChannelsKnown = true;
                frameKnown = IsPfmPayloadComplete(data, payloadOffset, width, height, pixelFacts.Channels);
                return true;
            }

            if (kind == 1)
            {
                if (!HasPnmTokenSeparator(data, offset)) return false;
                pixelFacts.BitDepth = 1;
                pixelFacts.BitDepthKnown = true;
                pixelFacts.Channels = 1;
                pixelFacts.ChannelsKnown = true;
                long asciiSampleCount;
                try
                {
                    asciiSampleCount = checked((long)width * height);
                }
                catch (OverflowException)
                {
                    asciiSampleCount = long.MaxValue;
                }
                frameKnown = TryReadPlainPbmPayload(data, offset, asciiSampleCount);
                return true;
            }

            if (kind == 4)
            {
                if (!HasPnmTokenSeparator(data, offset)) return false;
                pixelFacts.BitDepth = 1;
                pixelFacts.BitDepthKnown = true;
                pixelFacts.Channels = 1;
                pixelFacts.ChannelsKnown = true;
                frameKnown = IsPnmBinaryPayloadComplete(data, offset, width, height, 1, 1, true);
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
            int channels = pixelFacts.Channels;
            long sampleCount;
            try
            {
                sampleCount = checked((long)width * height * channels);
            }
            catch (OverflowException)
            {
                sampleCount = long.MaxValue;
            }
            if (kind == 2 || kind == 3)
            {
                frameKnown = TryReadPnmAsciiPayload(data, offset, sampleCount, maxValue);
            }
            else
            {
                frameKnown = IsPnmBinaryPayloadComplete(data, offset, width, height, channels, maxValue <= 255 ? 1 : 2, false);
            }
            return true;
        }

        private static bool IsPamPayloadComplete(byte[] data, int payloadOffset, int width, int height, int channels, int bitDepth)
        {
            if (payloadOffset < 0 || payloadOffset > data.Length || width <= 0 || height <= 0 || channels <= 0 || bitDepth <= 0) return false;
            int bytesPerSample = bitDepth <= 8 ? 1 : bitDepth == 16 ? 2 : 0;
            if (bytesPerSample == 0) return false;
            try
            {
                long expectedBytes = checked((long)width * height * channels * bytesPerSample);
                return expectedBytes <= int.MaxValue && expectedBytes == data.Length - payloadOffset;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        private static bool TryReadPnmAsciiPayload(byte[] data, int offset, long sampleCount, int maxValue)
        {
            if (offset < 0 || offset > data.Length || sampleCount <= 0 || maxValue <= 0) return false;
            long samplesRead = 0;
            while (samplesRead < sampleCount)
            {
                int sample;
                if (!TryReadPnmInteger(data, ref offset, out sample) || sample < 0 || sample > maxValue) return false;
                ++samplesRead;
            }
            return HasOnlyPnmWhitespaceAndComments(data, offset);
        }

        private static bool TryReadPlainPbmPayload(byte[] data, int offset, long sampleCount)
        {
            // P1 pixels are individual ASCII bits, with optional whitespace;
            // unlike P2/P3 samples, adjacent digits do not form an integer.
            for (long sample = 0; sample < sampleCount; ++sample)
            {
                SkipPnmWhitespaceAndComments(data, ref offset);
                if (offset >= data.Length || (data[offset] != '0' && data[offset] != '1')) return false;
                ++offset;
            }
            return HasOnlyPnmWhitespaceAndComments(data, offset);
        }

        private static bool IsPnmBinaryPayloadComplete(byte[] data, int headerOffset, int width, int height, int channels, int bytesPerSample, bool packedBits)
        {
            int payloadOffset;
            if (!TryConsumePnmBinaryDelimiter(data, headerOffset, out payloadOffset) || width <= 0 || height <= 0 || channels <= 0 || bytesPerSample <= 0) return false;
            try
            {
                long expectedBytes = packedBits
                    ? checked(((long)width + 7) / 8 * height)
                    : checked((long)width * height * channels * bytesPerSample);
                return expectedBytes <= int.MaxValue && expectedBytes == data.Length - payloadOffset;
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        private static bool TryConsumePnmBinaryDelimiter(byte[] data, int offset, out int payloadOffset)
        {
            payloadOffset = 0;
            if (offset < 0 || offset >= data.Length || !HasPnmTokenSeparator(data, offset)) return false;
            int delimiter = offset;
            if (data[delimiter] == (byte)'#')
            {
                while (delimiter < data.Length && data[delimiter] != 10 && data[delimiter] != 13) ++delimiter;
            }
            if (delimiter >= data.Length || !IsPnmWhitespace(data[delimiter])) return false;
            // Raw P4/P5/P6 have exactly one whitespace byte before the raster.
            // After a CR delimiter, LF is already a binary sample, not CRLF.
            payloadOffset = delimiter + 1;
            return true;
        }

        private static bool HasOnlyPnmWhitespaceAndComments(byte[] data, int offset)
        {
            SkipPnmWhitespaceAndComments(data, ref offset);
            return offset == data.Length;
        }

        private static void SkipPnmWhitespaceAndComments(byte[] data, ref int offset)
        {
            while (offset < data.Length)
            {
                if (IsPnmWhitespace(data[offset]))
                {
                    ++offset;
                    continue;
                }
                if (data[offset] != (byte)'#') return;
                // Netpbm comments end before either CR or LF. The terminator
                // remains whitespace, or the sole raw-raster delimiter above.
                while (offset < data.Length && data[offset] != 10 && data[offset] != 13) ++offset;
            }
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
            SkipPnmWhitespaceAndComments(data, ref offset);
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

        private static bool TryReadGifFrameAndMetadataFacts(byte[] data, out int frameCount, out MetadataFacts metadataFacts)
        {
            frameCount = 0;
            metadataFacts = default(MetadataFacts);
            if (data.Length < 13 || data[3] != (byte)'8' ||
                (data[4] != (byte)'7' && data[4] != (byte)'9') || data[5] != (byte)'a') return false;
            MetadataFacts facts = new MetadataFacts();
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
                if (marker == 0x3B)
                {
                    if (frameCount == 0 || offset != data.Length) return false;
                    facts.MetadataSizeKnown = true;
                    facts.IccProfileSizeKnown = true;
                    metadataFacts = facts;
                    return true;
                }
                if (marker == 0x2C)
                {
                    if (data.Length - offset < 9) return false;
                    byte packed = data[offset + 8];
                    offset += 9;
                    if ((packed & 0x80) != 0)
                    {
                        int tableBytes = 3 * (1 << ((packed & 7) + 1));
                        if (tableBytes > data.Length - offset) return false;
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
                        if (data.Length - offset < 6 || data[offset] != 4 || data[offset + 5] != 0) return false;
                        facts.MetadataBytes += 4;
                        offset += 6;
                    }
                    else if (extensionLabel == 0xFF || extensionLabel == 0x01)
                    {
                        int fixedBlockLength = extensionLabel == 0xFF ? 11 : 12;
                        if (data.Length - offset < 1 + fixedBlockLength || data[offset] != fixedBlockLength) return false;
                        bool isIccProfile = extensionLabel == 0xFF && IsGifIccProfileExtension(data, offset + 1);
                        offset += 1 + fixedBlockLength;
                        int payloadBytes;
                        if (!SkipGifSubBlocks(data, ref offset, out payloadBytes)) return false;
                        facts.MetadataBytes += fixedBlockLength + (long)payloadBytes;
                        if (isIccProfile) facts.IccProfileBytes += payloadBytes;
                    }
                    else
                    {
                        int payloadBytes;
                        if (!SkipGifSubBlocks(data, ref offset, out payloadBytes)) return false;
                        facts.MetadataBytes += payloadBytes;
                    }
                    continue;
                }
                return false;
            }
            return false;
        }

        private static bool SkipGifSubBlocks(byte[] data, ref int offset)
        {
            int payloadBytes;
            return SkipGifSubBlocks(data, ref offset, out payloadBytes);
        }

        private static bool SkipGifSubBlocks(byte[] data, ref int offset, out int payloadBytes)
        {
            payloadBytes = 0;
            while (offset < data.Length)
            {
                int length = data[offset++];
                if (length == 0) return true;
                if (length > data.Length - offset) return false;
                payloadBytes += length;
                offset += length;
            }
            return false;
        }
    }
}
