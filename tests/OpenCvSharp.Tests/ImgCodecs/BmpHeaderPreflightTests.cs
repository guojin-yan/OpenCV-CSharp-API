using System;
using System.IO;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class BmpHeaderPreflightTests
    {
        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(8)]
        [InlineData(24)]
        [InlineData(32)]
        public void IdentifyReadsBmpCoreDimensionsFromUnsigned16BitFields(int bits)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Bmp(12, bits));
            Assert.True(result.IsSizeKnown); Assert.Equal(4, result.Width); Assert.Equal(3, result.Height);
            Assert.True(result.IsFrameCountKnown); Assert.Equal(12, result.CumulativePixelCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(8)]
        public void IdentifyNeverPublishesIndexedBmpFactsWithoutCompletePalette(int bits)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Bmp(40, bits, includePalette: false));
            Assert.True(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown); Assert.False(result.IsFrameCountKnown);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(-1)]
        public void IdentifyRejectsConflictingBmpImageByteLengths(int declared)
        {
            byte[] data = Bmp(); Put32(data, 34, declared);
            AssertGeometryOnly(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData(13)]
        [InlineData(41)]
        public void IdentifyRejectsUnrecognizedBmpDibLayouts(int size)
        {
            byte[] data = Bmp(); Put32(data, 14, size);
            AssertUnknown(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        public void IdentifyRequiresOneBmpColorPlane(int planes)
        {
            byte[] data = Bmp(); Put16(data, 26, planes);
            AssertUnknown(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        public void IdentifyDoesNotClaimFrameForInvalidBmpBitCounts(int bits)
        {
            AssertUnknown(ImgCodecsCv2.Identify(Bmp(bits: bits)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        public void IdentifyNeverLetsBmpV5ProfilesOverlapHeaders(int relativeOffset)
        {
            byte[] data = Bmp(124); Put32(data, 70, 0x4D424544);
            Put32(data, 126, relativeOffset); Put32(data, 130, 1);
            AssertGeometryOnly(ImgCodecsCv2.Identify(data));
        }

        [Fact]
        public void IdentifyDoesNotInterpretUnsignedOs2BmpHeightAsTopDown()
        {
            AssertUnknown(ImgCodecsCv2.Identify(Bmp(64, height: -3)));
        }

        [Theory]
        [InlineData(12)]
        [InlineData(40)]
        [InlineData(52)]
        [InlineData(56)]
        [InlineData(64)]
        [InlineData(108)]
        [InlineData(124)]
        public void IdentifyReadsEstablishedBmpDibLayoutsAndIndexedStorage(int dib)
        {
            foreach (int bits in new[] { 1, 4, 8, 24, 32 })
            foreach (int width in new[] { 1, 4, 7 })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(Bmp(dib, bits, width));
                AssertComplete(result, width, 3, bits <= 8 ? bits : 8, bits <= 8 ? 1 : 3);
            }
        }

        [Theory]
        [InlineData(40)]
        [InlineData(52)]
        [InlineData(56)]
        [InlineData(108)]
        [InlineData(124)]
        public void IdentifyReadsTopDownWindowsBmpWithoutChangingPixelBudget(int dib)
        {
            AssertComplete(ImgCodecsCv2.Identify(Bmp(dib, width: 5, height: -3)), 5, 3, 8, 3);
        }

        [Theory]
        [InlineData("offset-in-header")]
        [InlineData("offset-in-palette")]
        [InlineData("offset-at-eof")]
        [InlineData("offset-past-eof")]
        [InlineData("oversized-palette")]
        [InlineData("huge-truecolor-palette")]
        [InlineData("missing-truecolor-palette")]
        [InlineData("wrong-file-size")]
        [InlineData("reserved-one")]
        [InlineData("reserved-two")]
        [InlineData("compressed")]
        [InlineData("top-down-compressed")]
        [InlineData("short-pixels")]
        [InlineData("trailing-pixels")]
        [InlineData("v5-reserved")]
        [InlineData("profile-before-pixels")]
        [InlineData("profile-in-pixels")]
        [InlineData("profile-past-eof")]
        [InlineData("profile-zero-size")]
        [InlineData("profile-huge-size")]
        [InlineData("profile-file-relative")]
        [InlineData("profile-trailing-byte")]
        public void IdentifyRequiresBoundedBmpPalettesAndPixelRanges(string failure)
        {
            byte[] data = Bmp();
            switch (failure)
            {
                case "offset-in-header": Put32(data, 10, 53); break;
                case "offset-in-palette": data = Bmp(bits: 8); Put32(data, 10, 54 + 1023); break;
                case "offset-at-eof": Put32(data, 10, data.Length); break;
                case "offset-past-eof": Put32(data, 10, -1); break;
                case "oversized-palette": data = Bmp(bits: 4); Put32(data, 46, 17); break;
                case "huge-truecolor-palette": Put32(data, 46, -1); break;
                case "missing-truecolor-palette": data = Bmp(colors: 1, includePalette: false); break;
                case "wrong-file-size": Put32(data, 2, data.Length - 1); break;
                case "reserved-one": Put16(data, 6, 1); break;
                case "reserved-two": Put16(data, 8, 1); break;
                case "compressed": Put32(data, 30, 3); break;
                case "top-down-compressed": Put32(data, 22, -3); Put32(data, 30, 1); break;
                case "short-pixels": Array.Resize(ref data, data.Length - 1); Put32(data, 2, data.Length); break;
                case "trailing-pixels": Array.Resize(ref data, data.Length + 1); Put32(data, 2, data.Length); break;
                case "v5-reserved": data = Bmp(124); Put32(data, 134, 1); break;
                default:
                    data = ProfileBmp();
                    switch (failure)
                    {
                        case "profile-before-pixels": Put32(data, 126, 124); Put32(data, 10, 142); break;
                        case "profile-in-pixels": Put32(data, 126, 128); break;
                        case "profile-past-eof": Put32(data, 126, data.Length); break;
                        case "profile-zero-size": Put32(data, 130, 0); break;
                        case "profile-huge-size": Put32(data, 130, -1); break;
                        case "profile-file-relative": Put32(data, 126, data.Length - 4); break;
                        case "profile-trailing-byte": Array.Resize(ref data, data.Length + 1); Put32(data, 2, data.Length); break;
                        default: throw new ArgumentException(failure);
                    }
                    break;
            }
            AssertGeometryOnly(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData("zero-width")]
        [InlineData("negative-width")]
        [InlineData("zero-height")]
        [InlineData("min-height")]
        [InlineData("core-zero-width")]
        [InlineData("truncated-dib")]
        [InlineData("huge-dib")]
        public void IdentifyDoesNotBorrowBmpGeometryFromIncompleteOrInvalidDib(string failure)
        {
            byte[] data = Bmp();
            switch (failure)
            {
                case "zero-width": Put32(data, 18, 0); break;
                case "negative-width": Put32(data, 18, -4); break;
                case "zero-height": Put32(data, 22, 0); break;
                case "min-height": Put32(data, 22, int.MinValue); break;
                case "core-zero-width": data = Bmp(12); Put16(data, 18, 0); break;
                case "truncated-dib": Array.Resize(ref data, 53); break;
                case "huge-dib": Put32(data, 14, int.MaxValue); break;
                default: throw new ArgumentException(failure);
            }
            AssertUnknown(ImgCodecsCv2.Identify(data));
        }

        [Fact]
        public void IdentifyAcceptsBmpOptionalPalettesAndUninterpretedGapBytes()
        {
            AssertComplete(ImgCodecsCv2.Identify(Bmp(bits: 8, colors: 2, gap: 7)), 4, 3, 8, 1);
            AssertComplete(ImgCodecsCv2.Identify(Bmp(colors: 2, gap: 3)), 4, 3, 8, 3);
            byte[] exact = Bmp(); Put32(exact, 34, exact.Length - 54);
            AssertComplete(ImgCodecsCv2.Identify(exact), 4, 3, 8, 3);
            byte[] data = Bmp(bits: 8); data[54 + 3] = 255;
            AssertComplete(ImgCodecsCv2.Identify(data), 4, 3, 8, 1);
            data = Bmp(124); Put32(data, 70, 0x73524742); Put32(data, 126, -1); Put32(data, 130, -1);
            AssertComplete(ImgCodecsCv2.Identify(data), 4, 3, 8, 3);
            // BI_RGB ignores bitfield masks and inactive V5 profile pointer/size fields.
            Put32(data, 54, -1); Put32(data, 58, -1); Put32(data, 62, -1);
            AssertComplete(ImgCodecsCv2.Identify(data), 4, 3, 8, 3);
        }

        [Fact]
        public void IdentifyBoundsBmpV5EmbeddedAndLinkedProfileBytesWithoutOpeningThem()
        {
            foreach (int kind in new[] { 0x4D424544, 0x4C494E4B })
            foreach (int gap in new[] { 0, 7 })
            {
                byte[] data = ProfileBmp(gap); Put32(data, 70, kind);
                AssertComplete(ImgCodecsCv2.Identify(data), 4, 3, 8, 3);
                Assert.False(ImgCodecsCv2.Identify(data).IsIccProfileSizeKnown);
                Assert.False(ImgCodecsCv2.Identify(data).IsMetadataSizeKnown);
            }
        }

        [Fact]
        public void IdentifyPreservesBmpGeometryWhileBoundingUncompressedStorageArithmetic()
        {
            AssertComplete(ImgCodecsCv2.Identify(Bmp(12, width: 65535, height: 1)), 65535, 1, 8, 3);
            AssertComplete(ImgCodecsCv2.Identify(Bmp(12, width: 1, height: 65535)), 1, 65535, 8, 3);
            byte[] data = Bmp(bits: 32); Put32(data, 18, int.MaxValue); Put32(data, 22, int.MaxValue);
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            Assert.True(result.IsSizeKnown); Assert.Equal(int.MaxValue, result.Width); Assert.Equal(int.MaxValue, result.Height);
            Assert.False(result.IsFrameCountKnown); Assert.False(result.IsPixelFormatKnown);
            // 16-bit RGB uses packed 5/5/5 storage, which the uniform-depth public result
            // does not model. Its exact uncompressed frame range is still independent.
            result = ImgCodecsCv2.Identify(Bmp(bits: 16));
            Assert.True(result.IsSizeKnown); Assert.True(result.IsFrameCountKnown); Assert.False(result.IsPixelFormatKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        [Fact]
        public void BmpHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams()
        {
            foreach (byte[] data in new[] { Bmp(), Bmp(12), ProfileBmp(), RealInfoRgb24, RealV5Rgb24Icc })
            {
                foreach (var limit in new[]
                {
                    new { Options = Options(data.Length - 1), Error = "input byte limit" },
                    new { Options = Options(data.Length, width: 3), Error = "dimensions" },
                    new { Options = Options(data.Length, height: 2), Error = "dimensions" },
                    new { Options = Options(data.Length, pixels: 11), Error = "pixel count" },
                    new { Options = Options(data.Length, depth: 7), Error = "bit depth" },
                    new { Options = Options(data.Length, channels: 2), Error = "channel count" },
                    new { Options = Options(data.Length, cumulative: 11), Error = "cumulative pixel count" },
                    new { Options = Options(data.Length, storage: 35), Error = "pixel storage" }
                })
                {
                    Assert.Contains(limit.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, limit.Options)).Message);
                    Assert.Contains(limit.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(data), limit.Options)).Message);
                    using (var stream = new MemoryStream(Join(new byte[] { 42 }, data)))
                    {
                        stream.Position = 1;
                        Assert.Contains(limit.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, limit.Options)).Message);
                        Assert.Equal(1, stream.Position);
                    }
                    using (var stream = new ShortReadStream(data))
                    {
                        Assert.Contains(limit.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, limit.Options)).Message);
                        Assert.Equal(data.Length, stream.BytesRead);
                    }
                }
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, data)))
                {
                    stream.Position = 1;
                    AssertComplete(ImgCodecsCv2.Identify(stream, Options(data.Length)), 4, 3, 8, 3);
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadStream(data))
                {
                    AssertComplete(ImgCodecsCv2.Identify(stream, Options(data.Length)), 4, 3, 8, 3);
                    Assert.Equal(data.Length, stream.BytesRead);
                }
            }
        }

        [Fact]
        public void BmpStrictPolicyRejectsUnknownLayoutsAndConflictingStorageFacts()
        {
            byte[] conflict = Bmp(); Put32(conflict, 34, 1);
            byte[] unknown = Bmp(); Put32(unknown, 14, 41);
            foreach (byte[] data in new[] { conflict, unknown, Bmp(bits: 8, includePalette: false), RealV5Bgra32Bitfields })
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, Options(data.Length)));
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, data)))
                {
                    stream.Position = 1;
                    Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.Identify(stream, Options(data.Length)));
                    Assert.Equal(1, stream.Position);
                }
            }
            using (var stream = new ShortReadStream(RealV5Bgra32Bitfields))
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(stream, Options(RealV5Bgra32Bitfields.Length, strict: false));
                AssertGeometryOnly(result);
            }
        }

        [Fact]
        public void IdentifyRequiresCompleteBmpDibAndExactPayloadBeforePublishingFrameFacts()
        {
            foreach (byte[] data in RealFixtures)
            for (int length = 1; length < data.Length; ++length)
            {
                byte[] prefix = new byte[length]; Array.Copy(data, prefix, length);
                ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                Assert.False(result.IsFrameCountKnown); Assert.False(result.IsPixelFormatKnown);
                Assert.False(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown);
                int headerEnd = 14 + BitConverter.ToInt32(data, 14);
                Assert.Equal(length >= headerEnd, result.IsSizeKnown);
                if (result.IsSizeKnown) { Assert.Equal(4, result.Width); Assert.Equal(3, result.Height); }
            }
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusBmpNeverEscapesOrPublishesInconsistentStorageFacts()
        {
            foreach (byte[] data in RealFixtures)
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            for (int offset = 0; offset < data.Length; ++offset)
            {
                if (data[offset] == replacement) continue;
                byte[] mutated = (byte[])data.Clone(); mutated[offset] = replacement;
                ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                if (result.Format != "bmp") continue;
                Assert.Equal(mutated.Length, result.InputBytes); Assert.InRange(result.BytesInspected, 0, mutated.Length);
                if (result.IsSizeKnown) { Assert.InRange(result.Width, 1, int.MaxValue); Assert.InRange(result.Height, 1, int.MaxValue); }
                else { Assert.False(result.IsFrameCountKnown); Assert.False(result.IsPixelFormatKnown); }
                if (result.IsFrameCountKnown)
                {
                    Assert.Equal(1, result.FrameCount); Assert.True(result.IsCumulativePixelCountKnown);
                    Assert.Equal((long)result.Width * result.Height, result.CumulativePixelCount);
                }
                else { Assert.False(result.IsPixelFormatKnown); Assert.False(result.IsEstimatedPixelBytesKnown); }
                if (result.IsPixelFormatKnown)
                {
                    Assert.Contains(result.BitDepth, new[] { 1, 4, 8 }); Assert.Contains(result.ChannelCount, new[] { 1, 3 });
                    Assert.True(result.IsEstimatedPixelBytesKnown); Assert.InRange(result.EstimatedPixelBytes, 1, (long)mutated.Length * 8);
                }
            }
        }

        private static void AssertComplete(ImageIdentifyResult result, int width, int height, int depth, int channels)
        {
            Assert.Equal("bmp", result.Format); Assert.True(result.IsSizeKnown);
            Assert.Equal(width, result.Width); Assert.Equal(height, result.Height);
            Assert.True(result.IsPixelFormatKnown); Assert.Equal(depth, result.BitDepth); Assert.Equal(channels, result.ChannelCount);
            Assert.True(result.IsFrameCountKnown); Assert.Equal(1, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown); Assert.Equal((long)width * height, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown);
            // The shared admission estimate rounds each stored sample to a byte; it
            // does not claim packed row bytes or expanded palette/native allocation.
            Assert.Equal((long)width * height * ((depth + 7) / 8) * channels, result.EstimatedPixelBytes);
            Assert.False(result.IsMetadataSizeKnown); Assert.False(result.IsIccProfileSizeKnown);
        }

        private static ImageDecodeOptions Options(int length, int width = 4, int height = 3, long pixels = 12,
            int depth = 8, int channels = 3, long cumulative = 12, long storage = 36, bool strict = true)
        {
            return new ImageDecodeOptions(length, width, height, pixels, 1, true, true,
                long.MaxValue, long.MaxValue, false, false, cumulative, depth, channels, strict, storage);
        }

        private sealed class ShortReadStream : Stream
        {
            private readonly byte[] data;
            public int BytesRead { get; private set; }
            public ShortReadStream(byte[] data) { this.data = data; }
            public override bool CanRead { get { return true; } }
            public override bool CanSeek { get { return false; } }
            public override bool CanWrite { get { return false; } }
            public override long Length { get { throw new NotSupportedException(); } }
            public override long Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
            public override int Read(byte[] buffer, int offset, int count)
            {
                int taken = Math.Min(Math.Min(count, 3), data.Length - BytesRead);
                Array.Copy(data, BytesRead, buffer, offset, taken); BytesRead += taken; return taken;
            }
            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
            public override void SetLength(long value) { throw new NotSupportedException(); }
            public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        }

        private static void AssertUnknown(ImageIdentifyResult result)
        {
            Assert.Equal("bmp", result.Format); Assert.False(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown); Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        private static void AssertGeometryOnly(ImageIdentifyResult result)
        {
            Assert.Equal("bmp", result.Format); Assert.True(result.IsSizeKnown);
            Assert.Equal(4, result.Width); Assert.Equal(3, result.Height);
            Assert.False(result.IsPixelFormatKnown); Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        private static byte[] Bmp(int dib = 40, int bits = 24, int width = 4, int height = 3,
            bool includePalette = true, int colors = 0, int gap = 0)
        {
            int entries = bits <= 8 && colors == 0 ? 1 << bits : colors;
            int offset = 14 + dib + (includePalette ? entries * (dib == 12 ? 3 : 4) : 0) + gap;
            int stride = ((width * bits + 31) / 32) * 4;
            byte[] data = new byte[offset + stride * Math.Abs(height)];
            data[0] = (byte)'B'; data[1] = (byte)'M';
            Put32(data, 2, data.Length); Put32(data, 10, offset); Put32(data, 14, dib);
            if (dib == 12)
            {
                Put16(data, 18, width); Put16(data, 20, height); Put16(data, 22, 1); Put16(data, 24, bits);
            }
            else
            {
                Put32(data, 18, width); Put32(data, 22, height); Put16(data, 26, 1); Put16(data, 28, bits);
                Put32(data, 46, colors);
            }
            return data;
        }

        private static byte[] ProfileBmp(int gap = 0)
        {
            byte[] data = Bmp(124); int start = data.Length + gap;
            Array.Resize(ref data, start + 4); Put32(data, 2, data.Length);
            Put32(data, 70, 0x4D424544); Put32(data, 126, start - 14); Put32(data, 130, 4);
            // Range-only evidence: contents need not be a valid ICC profile or path.
            data[start] = (byte)'n'; data[start + 1] = (byte)'o'; data[start + 2] = (byte)'!';
            return data;
        }

        private static byte[] Join(byte[] prefix, byte[] data)
        {
            byte[] result = new byte[prefix.Length + data.Length];
            Array.Copy(prefix, result, prefix.Length); Array.Copy(data, 0, result, prefix.Length, data.Length);
            return result;
        }

        [Theory]
        [InlineData(0,8,3,true,90,"e746c3d7f3f533349aa60d394004382aba405a3ae6535252531f944adc52815e")]
        [InlineData(1,8,1,true,1090,"3f9c1a057e5c4bb859cb1a824b65897a5ff22ae7ea4f82012f80286f589f9a3e")]
        [InlineData(2,1,1,true,74,"c4d6bb00104fc5b87fc614bc865ca9a0689995b625352ab457a96a0e711400de")]
        [InlineData(3,8,3,true,62,"7323fdd9012a8580c0ff5bd80109b7b065c23d1cde1d5cd5da61dce22d3d9727")]
        [InlineData(4,4,1,true,86,"357bbc21521ced0035b276027b1bd4988a5dbaec5b9f3e824e86e57013aa848f")]
        [InlineData(5,8,3,true,102,"eed5242fda2a1da0bd91b094e1608f400bb1ad4be878295ec0d9f62293452572")]
        [InlineData(6,8,3,true,158,"7cab4187750553193cf2f31df5d5df53260878c9401bf59e5a9bc8cb545f85e1")]
        [InlineData(7,8,3,true,762,"5c16c3707e126763da536a9ce2c9fd3e87b77cf2bedb925c39b15addcb46695a")]
        [InlineData(8,0,0,false,186,"dcf0821a7c8a5a62006bf77271dd2113979c5717e30e60d1acd498b1ea2d5d62")]
        [InlineData(9,8,3,true,114,"5e277d32afc7418675af615bc7e4dedc24bb50072065f82ecfd739f7f9af5817")]
        public void IdentifyReadsRealBmpDibAndPaletteFixtures(int fixture, int depth, int channels, bool complete, int length, string sha256)
        {
            byte[] data = RealFixtures[fixture];
            Assert.Equal(length, data.Length);
            Assert.Equal(sha256, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)).ToLowerInvariant());
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            if (complete) AssertComplete(result, 4, 3, depth, channels);
            else AssertGeometryOnly(result);
        }

        // Pillow 13.0.0 encoded INFO fixtures; byte-authored CORE/OS2/V4/V5
        // variants were all independently decoded with Pillow. Every RGB/RGBA
        // sample matches its original array, including indexed color expansion.
        // Embedded bytes and decoded sample hashes were rechecked after insertion.
        private static byte[] RealInfoRgb24 { get { return Convert.FromBase64String("Qk1aAAAAAAAAADYAAAAoAAAABAAAAAMAAAABABgAAAAAACQAAADEDgAAxA4AAAAAAAAAAAAAvayb8N/OIxIBVkU08eDPJBMCV0Y1inloJRQDWEc2i3ppvq2c"); } }
        private static byte[] RealInfoIndex8 { get { return Convert.FromBase64String("Qk1CBAAAAAAAADYEAAAoAAAABAAAAAMAAAABAAgAAAAAAAwAAADEDgAAxA4AAAABAAAAAQAAAAAAADUlEQBqSiIAn28zANSURAAJuVUAPt5mAHMDdwCoKIgA3U2ZABJyqgBHl7sAfLzMALHh3QDmBu4AGyv/AFBQEACFdSEAupoyAO+/QwAk5FQAWQllAI4udgDDU4cA+HiYAC2dqQBiwroAl+fLAMwM3AABMe0ANlb+AGt7DwCgoCAA1cUxAArqQgA/D1MAdDRkAKlZdQDefoYAE6OXAEjIqAB97bkAshLKAOc32wAcXOwAUYH9AIamDgC7yx8A8PAwACUVQQBaOlIAj19jAMSEdAD5qYUALs6WAGPzpwCYGLgAzT3JAAJi2gA3h+sAbKz8AKHRDQDW9h4ACxsvAEBAQAB1ZVEAqopiAN+vcwAU1IQASfmVAH4epgCzQ7cA6GjIAB2N2QBSsuoAh9f7ALz8DADxIR0AJkYuAFtrPwCQkFAAxbVhAPracgAv/4MAZCSUAJlJpQDObrYAA5PHADi42ABt3ekAogL6ANcnCwAMTBwAQXEtAHaWPgCru08A4OBgABUFcQBKKoIAf0+TALR0pADpmbUAHr7GAFPj1wCICOgAvS35APJSCgAndxsAXJwsAJHBPQDG5k4A+wtfADAwcABlVYEAmnqSAM+fowAExLQAOenFAG4O1gCjM+cA2Fj4AA19CQBCohoAd8crAKzsPADhEU0AFjZeAEtbbwCAgIAAtaWRAOrKogAf77MAVBTEAIk51QC+XuYA84P3ACioCABdzRkAkvIqAMcXOwD8PEwAMWFdAGaGbgCbq38A0NCQAAX1oQA6GrIAbz/DAKRk1ADZieUADq72AEPTBwB4+BgArR0pAOJCOgAXZ0sATIxcAIGxbQC21n4A6/uPACAgoABVRbEAimrCAL+P0wD0tOQAKdn1AF7+BgCTIxcAyEgoAP1tOQAykkoAZ7dbAJzcbADRAX0ABiaOADtLnwBwcLAApZXBANq60gAP3+MARAT0AHkpBQCuThYA43MnABiYOABNvUkAguJaALcHawDsLHwAIVGNAFZ2ngCLm68AwMDAAPXl0QAqCuIAXy/zAJRUBADJeRUA/p4mADPDNwBo6EgAnQ1ZANIyagAHV3sAPHyMAHGhnQCmxq4A2+u/ABAQ0ABFNeEAelryAK9/AwDkpBQAGcklAE7uNgCDE0cAuDhYAO1daQAignoAV6eLAIzMnADB8a0A9ha+ACs7zwBgYOAAlYXxAMqqAgD/zxMANPQkAGkZNQCePkYA02NXAAiIaAA9rXkActKKAKf3mwDcHKwAEUG9AEZmzgB7i98AsLDwAOXVAQAa+hIATx8jAIRENAC5aUUA7o5WACOzZwBY2HgAjf2JAMIimgD3R6sALGy8AGGRzQCWtt4Ay9vvAAQFBgcCAwQFAAECAw=="); } }
        private static byte[] RealInfoMono1 { get { return Convert.FromBase64String("Qk1KAAAAAAAAAD4AAAAoAAAABAAAAAMAAAABAAEAAAAAAAwAAADEDgAAxA4AAAIAAAACAAAAAAAAAP///wBQAAAAoAAAAFAAAAA="); } }
        private static byte[] RealCoreRgb24 { get { return Convert.FromBase64String("Qk0+AAAAAAAAABoAAAAMAAAABAADAAEAGAC9rJvw384jEgFWRTTx4M8kEwJXRjWKeWglFANYRzaLemm+rZw="); } }
        private static byte[] RealCoreIndex4 { get { return Convert.FromBase64String("Qk1WAAAAAAAAAEoAAAAMAAAABAADAAEABAAAAAA1JRFqSiKfbzPUlEQJuVU+3mZzA3eoKIjdTZkScqpHl7t8vMyx4d3mBu4bK/9FZwAAI0UAAAEjAAA="); } }
        private static byte[] RealTopDownBgrx32 { get { return Convert.FromBase64String("Qk1mAAAAAAAAADYAAAAoAAAABAAAAP3///8BACAAAAAAAAAAAAATCwAAEwsAAAAAAAAAAAAAJRQDTVhHNk2LemlNvq2cTfHgz00kEwJNV0Y1TYp5aE29rJtN8N/OTSMSAU1WRTRN"); } }
        private static byte[] RealV4Rgb24 { get { return Convert.FromBase64String("Qk2eAAAAAAAAAHoAAABsAAAABAAAAAMAAAABABgAAAAAAAAAAAATCwAAEwsAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAC9rJvw384jEgFWRTTx4M8kEwJXRjWKeWglFANYRzaLemm+rZw="); } }
        private static byte[] RealV5Rgb24Icc { get { return Convert.FromBase64String("Qk36AgAAAAAAAIoAAAB8AAAABAAAAAMAAAABABgAAAAAAAAAAAATCwAAEwsAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAERFQk0AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAoAAAAEwCAAAAAAAAvayb8N/OIxIBVkU08eDPJBMCV0Y1inloJRQDWEc2i3ppvq2cAAACTGxjbXMEQAAAbW50clJHQiBYWVogB+oACwAVAAUACgApYWNzcE1TRlQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAPbWAAEAAAAA0y1sY21zAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAALZGVzYwAAAQgAAAA2Y3BydAAAAUAAAABMd3RwdAAAAYwAAAAUY2hhZAAAAaAAAAAsclhZWgAAAcwAAAAUYlhZWgAAAeAAAAAUZ1hZWgAAAfQAAAAUclRSQwAAAggAAAAgZ1RSQwAAAggAAAAgYlRSQwAAAggAAAAgY2hybQAAAigAAAAkbWx1YwAAAAAAAAABAAAADGVuVVMAAAAaAAAAHABzAFIARwBCACAAYgB1AGkAbAB0AC0AaQBuAABtbHVjAAAAAAAAAAEAAAAMZW5VUwAAADAAAAAcAE4AbwAgAGMAbwBwAHkAcgBpAGcAaAB0ACwAIAB1AHMAZQAgAGYAcgBlAGUAbAB5WFlaIAAAAAAAAPbWAAEAAAAA0y1zZjMyAAAAAAABDEIAAAXe///zJQAAB5MAAP2Q///7of///aIAAAPcAADAblhZWiAAAAAAAABvoAAAOPUAAAOQWFlaIAAAAAAAACSfAAAPhAAAtsNYWVogAAAAAAAAYpcAALeHAAAY2XBhcmEAAAAAAAMAAAACZmYAAPKnAAANWQAAE9AAAApbY2hybQAAAAAAAwAAAACj1wAAVHsAAEzNAACZmgAAJmYAAA9c"); } }
        private static byte[] RealV5Bgra32Bitfields { get { return Convert.FromBase64String("Qk26AAAAAAAAAIoAAAB8AAAABAAAAAMAAAABACAAAwAAAAAAAAATCwAAEwsAAAAAAAAAAAAAAAD/AAD/AAD/AAAAAAAA/0JHUnMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAvaybo/DfzrYjEgHJVkU03PHgz1ckEwJqV0Y1fYp5aJAlFAMLWEc2Hot6aTG+rZxE"); } }
        private static byte[] RealOs2Rgb24 { get { return Convert.FromBase64String("Qk1yAAAAAAAAAE4AAABAAAAABAAAAAMAAAABABgAAAAAAAAAAAATCwAAEwsAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAvayb8N/OIxIBVkU08eDPJBMCV0Y1inloJRQDWEc2i3ppvq2c"); } }
        private static byte[][] RealFixtures { get { return new[] { RealInfoRgb24, RealInfoIndex8, RealInfoMono1, RealCoreRgb24, RealCoreIndex4, RealTopDownBgrx32, RealV4Rgb24, RealV5Rgb24Icc, RealV5Bgra32Bitfields, RealOs2Rgb24 }; } }

        private static void Put16(byte[] data, int offset, int value)
        {
            data[offset] = (byte)value; data[offset + 1] = (byte)(value >> 8);
        }

        private static void Put32(byte[] data, int offset, int value)
        {
            Put16(data, offset, value); Put16(data, offset + 2, value >> 16);
        }
    }
}
