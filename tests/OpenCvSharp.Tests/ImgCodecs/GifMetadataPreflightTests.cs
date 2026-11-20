using System;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class GifMetadataPreflightTests
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IdentifyReportsZeroGifMetadataWithGlobalOrLocalPalette(bool gif87, bool localPalette)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(CreateGif(new byte[0], 1, false, gif87, localPalette));

            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(1, result.FrameCount);
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(0, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(0, result.IccProfileBytes);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyCountsAllGifExtensionPayloadsAndRepeatedIccProfiles(bool afterFrames)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(CreateGif(CreateMixedExtensions(), 2, afterFrames));

            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(63, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(5, result.IccProfileBytes);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(2, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown);
            Assert.Equal(2, result.CumulativePixelCount);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(254)]
        [InlineData(255)]
        [InlineData(256)]
        [InlineData(511)]
        [InlineData(1024)]
        public void IdentifyCountsGifIccStoredBytesAcrossSubBlockBoundaries(int payloadLength)
        {
            byte[] payload = new byte[payloadLength];
            for (int index = 0; index < payload.Length; ++index) payload[index] = (byte)index;
            ImageIdentifyResult result = ImgCodecsCv2.Identify(CreateGif(CreateApplication("ICCRGBG1012", payload)));

            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(11L + payloadLength, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(payloadLength, result.IccProfileBytes);
        }

        [Fact]
        public void IdentifyMatchesTheEntireGifIccIdentifierAndAuthenticationCode()
        {
            foreach (string identifier in new[] { "ICCRGBG101X", "ICCRGBG2012", "XCCRGBG1012" })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(CreateGif(CreateApplication(identifier, new byte[] { 1, 2, 3 })));
                Assert.True(result.IsMetadataSizeKnown);
                Assert.Equal(14, result.MetadataBytes);
                Assert.True(result.IsIccProfileSizeKnown);
                Assert.Equal(0, result.IccProfileBytes);
            }
        }

        [Fact]
        public void IdentifyKeepsGifMetadataUnknownForEveryStrictPrefixAndTrailingInput()
        {
            byte[] gif = CreateGif(CreateMixedExtensions(), 2, true);
            for (int length = 1; length < gif.Length; ++length)
            {
                byte[] prefix = new byte[length];
                Array.Copy(gif, prefix, length);
                AssertUnknownMetadata(ImgCodecsCv2.Identify(prefix));
            }

            byte[] trailing = new byte[gif.Length + 1];
            Array.Copy(gif, trailing, gif.Length);
            AssertUnknownMetadata(ImgCodecsCv2.Identify(trailing));
            byte[] concatenated = new byte[2 * gif.Length];
            Array.Copy(gif, concatenated, gif.Length);
            Array.Copy(gif, 0, concatenated, gif.Length, gif.Length);
            AssertUnknownMetadata(ImgCodecsCv2.Identify(concatenated));
            gif[4] = (byte)'0';
            AssertUnknownMetadata(ImgCodecsCv2.Identify(gif));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(12)]
        [InlineData(255)]
        public void IdentifyKeepsGifMetadataUnknownForInvalidApplicationHeaderLength(byte length)
        {
            byte[] extension = CreateApplication("ICCRGBG1012", new byte[] { 1, 2, 3 });
            extension[2] = length;
            AssertUnknownMetadata(ImgCodecsCv2.Identify(CreateGif(extension)));
        }

        [Fact]
        public void IdentifyDiscardsPartialGifMetadataAfterMalformedExtensionsOrMissingImages()
        {
            byte[] icc = CreateApplication("ICCRGBG1012", new byte[] { 1, 2, 3 });
            byte[][] brokenExtensions =
            {
                new byte[] { 0x21, 0xF9, 3, 0, 0, 0, 0, 0 },
                new byte[] { 0x21, 0xF9, 4, 0, 0, 0, 0, 1 },
                new byte[] { 0x21, 0x01, 11, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
                new byte[] { 0x21, 0xFE, 255, 1, 2, 3, 0 },
                new byte[] { 0x21, 0xCE, 255, 0 },
                new byte[] { 0x21 }
            };
            foreach (byte[] broken in brokenExtensions)
            {
                byte[] extensions = new byte[icc.Length + broken.Length];
                Array.Copy(icc, extensions, icc.Length);
                Array.Copy(broken, 0, extensions, icc.Length, broken.Length);
                AssertUnknownMetadata(ImgCodecsCv2.Identify(CreateGif(extensions, 1, true)));
            }
            AssertUnknownMetadata(ImgCodecsCv2.Identify(CreateGif(icc, 0)));
        }

        [Fact]
        public void GifMetadataBudgetsRejectBeforeNativeDecodeAndAcceptExactStoredLengths()
        {
            byte[] gif = CreateGif(CreateMixedExtensions(), 2);
            ImageDecodeOptions exact = Options(gif.Length, 63, 5);
            using (var stream = new MemoryStream(gif))
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(stream, exact);
                Assert.Equal(63, result.MetadataBytes);
                Assert.Equal(5, result.IccProfileBytes);
                Assert.Equal(0, stream.Position);
            }

            ImageDecodeOptions metadataLimit = Options(gif.Length, 62, 5);
            ImageDecodeOptions iccLimit = Options(gif.Length, 63, 4);
            Assert.Contains("metadata exceeds", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(gif, metadataLimit)).Message);
            Assert.Contains("ICC profile exceeds", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(gif, iccLimit)).Message);
            Assert.Contains("metadata exceeds", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(gif), metadataLimit)).Message);
            Assert.Contains("ICC profile exceeds", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(gif), iccLimit)).Message);

            foreach (ImageDecodeOptions options in new[] { exact, metadataLimit, iccLimit })
            {
                using (var stream = new MemoryStream())
                {
                    stream.WriteByte(0x7F);
                    stream.Write(gif, 0, gif.Length);
                    stream.Position = 1;
                    if (options == exact) ImgCodecsCv2.Identify(stream, options);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadNonSeekableStream(gif))
                {
                    if (options == exact) ImgCodecsCv2.Identify(stream, options);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(gif.Length, stream.BytesRead);
                }
            }
        }

        [Fact]
        public void GifStrictMetadataPolicyRejectsUnknownLengthsWhilePermissiveIdentifyPreservesThem()
        {
            byte[] gif = CreateGif(CreateMixedExtensions());
            Array.Resize(ref gif, gif.Length - 1);
            AssertUnknownMetadata(ImgCodecsCv2.Identify(gif));
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(gif, Options(gif.Length, 63, 5)));
            ImageDecodeOptions strictIccOnly = new ImageDecodeOptions(gif.Length, 2, 2, 4, 2, true, true, 63, 5, false, true);
            Assert.Contains("ICC profile size", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(gif, strictIccOnly)).Message);
            using (var stream = new MemoryStream(gif))
            {
                AssertUnknownMetadata(ImgCodecsCv2.Identify(stream));
                Assert.Equal(0, stream.Position);
            }
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusGifMetadataNeverEscapesParserOrPublishesUnboundedSizes()
        {
            byte[] gif = CreateGif(CreateMixedExtensions(), 2, true);
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            {
                for (int offset = 0; offset < gif.Length; ++offset)
                {
                    if (gif[offset] == replacement) continue;
                    byte[] mutated = (byte[])gif.Clone();
                    mutated[offset] = replacement;
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                    if (!result.IsMetadataSizeKnown)
                    {
                        AssertUnknownMetadata(result);
                        continue;
                    }
                    Assert.True(result.IsFrameCountKnown);
                    Assert.True(result.IsIccProfileSizeKnown);
                    Assert.InRange(result.MetadataBytes, 0L, mutated.LongLength);
                    Assert.InRange(result.IccProfileBytes, 0L, result.MetadataBytes);
                }
            }
        }

        private static ImageDecodeOptions Options(int inputLength, long metadataBytes, long iccBytes)
        {
            return new ImageDecodeOptions(inputLength, 2, 2, 4, 2, true, true, metadataBytes, iccBytes, true, true);
        }

        private static void AssertUnknownMetadata(ImageIdentifyResult result)
        {
            Assert.False(result.IsMetadataSizeKnown);
            Assert.False(result.IsIccProfileSizeKnown);
            Assert.Equal(0, result.MetadataBytes);
            Assert.Equal(0, result.IccProfileBytes);
        }

        private static byte[] CreateMixedExtensions()
        {
            using (var extensions = new MemoryStream())
            {
                byte[] commentAndControl = { 0x21, 0xFE, 2, (byte)'a', (byte)'b', 1, (byte)'c', 0, 0x21, 0xF9, 4, 0, 0, 0, 0, 0 };
                byte[] plainTextAndUnknown = { 0x21, 0x01, 12, 0, 0, 0, 0, 1, 0, 1, 0, 1, 1, 1, 0, 2, (byte)'h', (byte)'i', 0, 0x21, 0xCE, 1, 0x7E, 0 };
                foreach (byte[] extension in new[]
                {
                    commentAndControl, plainTextAndUnknown,
                    CreateApplication("NETSCAPE2.0", new byte[] { 1, 0, 0 }),
                    CreateApplication("ICCRGBG1012", new byte[] { 1, 2, 3 }, 2),
                    CreateApplication("ICCRGBG1012", new byte[] { 4, 5 }, 1)
                }) extensions.Write(extension, 0, extension.Length);
                return extensions.ToArray();
            }
        }

        private static byte[] CreateApplication(string identifier, byte[] payload, int blockLength = 255)
        {
            using (var stream = new MemoryStream())
            {
                stream.WriteByte(0x21); stream.WriteByte(0xFF); stream.WriteByte(11);
                byte[] name = Encoding.ASCII.GetBytes(identifier);
                Assert.Equal(11, name.Length);
                stream.Write(name, 0, name.Length);
                for (int offset = 0; offset < payload.Length; offset += blockLength)
                {
                    int count = Math.Min(blockLength, payload.Length - offset);
                    stream.WriteByte((byte)count);
                    stream.Write(payload, offset, count);
                }
                stream.WriteByte(0);
                return stream.ToArray();
            }
        }

        private static byte[] CreateGif(byte[] extensions, int frameCount = 1, bool afterFrames = false, bool gif87 = false, bool localPalette = false)
        {
            using (var stream = new MemoryStream())
            {
                byte[] header = { (byte)'G', (byte)'I', (byte)'F', (byte)'8', (byte)(gif87 ? '7' : '9'), (byte)'a', 1, 0, 1, 0, (byte)(localPalette ? 0 : 0x80), 0, 0 };
                byte[] palette = { 0, 0, 0, 255, 255, 255 };
                byte[] image = { 0x2C, 0, 0, 0, 0, 1, 0, 1, 0, (byte)(localPalette ? 0x80 : 0) };
                byte[] imageData = { 2, 2, 0x44, 0x01, 0 };
                stream.Write(header, 0, header.Length);
                if (!localPalette) stream.Write(palette, 0, palette.Length);
                if (!afterFrames) stream.Write(extensions, 0, extensions.Length);
                for (int frame = 0; frame < frameCount; ++frame)
                {
                    stream.Write(image, 0, image.Length);
                    if (localPalette) stream.Write(palette, 0, palette.Length);
                    stream.Write(imageData, 0, imageData.Length);
                }
                if (afterFrames) stream.Write(extensions, 0, extensions.Length);
                stream.WriteByte(0x3B);
                return stream.ToArray();
            }
        }

        private sealed class ShortReadNonSeekableStream : MemoryStream
        {
            public ShortReadNonSeekableStream(byte[] data) : base(data, false) { }
            public int BytesRead { get; private set; }
            public override bool CanSeek { get { return false; } }
            public override int Read(byte[] buffer, int offset, int count)
            {
                int read = base.Read(buffer, offset, Math.Min(count, 2));
                BytesRead += read;
                return read;
            }
        }
    }
}
