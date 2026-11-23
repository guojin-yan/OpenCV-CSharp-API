using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class PfmHeaderPreflightTests
    {
        [Theory]
        [MemberData(nameof(RealPfmFixtures))]
        public void IdentifyReadsRealPfmFloatFixtures(string name, int width, int height, int bits,
            int channels, int length, string sha256, string encoded)
        {
            Assert.NotEmpty(name);
            byte[] data = Convert.FromBase64String(encoded);
            Assert.Equal(length, data.Length);
            Assert.Equal(sha256, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)).ToLowerInvariant());
            AssertComplete(ImgCodecsCv2.Identify(data), width, height, bits, channels);
            AssertComplete(ImgCodecsCv2.Identify(new ReadOnlySpan<byte>(data)), width, height, bits, channels);
        }

        [Theory]
        [InlineData("PF", 3)]
        [InlineData("Pf", 1)]
        public void IdentifyStopsPfmHeaderCommentsAtCarriageReturn(string magic, int channels)
        {
            byte[] payload = new byte[channels * 4];
            payload[0] = 0x7f;
            string beforeDimensions = magic + "\n# dimensions\r1 1\n# scale\r";
            AssertComplete(ImgCodecsCv2.Identify(Join(Bytes(beforeDimensions + "-1\n"), payload)), 1, 1, 32, channels);

            string afterScale = magic + "\n1 1\n-1# compatible comment\r";
            AssertComplete(ImgCodecsCv2.Identify(Join(Bytes(afterScale), payload)), 1, 1, 32, channels);
        }

        [Fact]
        public void IdentifyPreservesEveryPfmFirstPayloadByteAfterEachSingleWhitespaceDelimiter()
        {
            foreach (string magic in new[] { "PF", "Pf" })
            foreach (byte delimiter in new byte[] { 9, 10, 11, 12, 13, 32 })
            for (int first = 0; first <= 255; ++first)
            {
                int channels = magic == "PF" ? 3 : 1;
                byte[] payload = new byte[channels * 4];
                payload[0] = (byte)first;
                byte[] header = Join(Bytes(magic + "\n1 1\n-1"), new[] { delimiter });
                AssertComplete(ImgCodecsCv2.Identify(Join(header, payload)), 1, 1, 32, channels);
            }
        }

        [Theory]
        [InlineData("PF", 3)]
        [InlineData("Pf", 1)]
        public void IdentifyPreservesPfmLfRasterByteAfterCrDelimiter(string magic, int channels)
        {
            byte[] payload = new byte[channels * 4];
            payload[0] = 10;
            AssertComplete(ImgCodecsCv2.Identify(Join(Bytes(magic + "\n1 1\n-1\r"), payload)), 1, 1, 32, channels);
        }

        [Theory]
        [InlineData("PF", 3)]
        [InlineData("Pf", 1)]
        public void IdentifyNeverSkipsAdditionalPfmRasterWhitespace(string magic, int channels)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Bytes(magic + "\n1 1\n-1\r\n"), new byte[channels * 4]));
            AssertHeader(result, 1, 1, 32, channels);
            AssertNoFrame(result);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("-1")]
        [InlineData("+1")]
        [InlineData(".5")]
        [InlineData("-0.5")]
        [InlineData("1.")]
        [InlineData("1.25")]
        [InlineData("001.2500")]
        [InlineData("1e0")]
        [InlineData("1E+1")]
        [InlineData("-.25e-12")]
        [InlineData("1e309")]
        [InlineData("1e-400")]
        [InlineData("1e999999999999999999999999999")]
        [InlineData("1e-999999999999999999999999999")]
        public void IdentifyAcceptsNonzeroPfmDecimalScaleWithoutInferringNativeRepresentability(string scale)
        {
            AssertComplete(ImgCodecsCv2.Identify(Join(Bytes("Pf\n1 1\n" + scale + "\n"), new byte[4])), 1, 1, 32, 1);
        }

        [Fact]
        public void IdentifyRequiresExactStandalonePfmPayloadAndPreservesProvenHeaders()
        {
            foreach (object[] fixture in RealPfmFixtures)
            {
                byte[] data = Convert.FromBase64String((string)fixture[7]);
                ImageIdentifyResult complete = ImgCodecsCv2.Identify(data);
                for (int length = 1; length < data.Length; ++length)
                {
                    byte[] prefix = new byte[length];
                    Array.Copy(data, prefix, length);
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                    AssertNoFrame(result);
                    if (result.IsSizeKnown)
                    {
                        AssertHeader(result, complete.Width, complete.Height, complete.BitDepth, complete.ChannelCount);
                        AssertNoFrame(result);
                    }
                }

                foreach (byte[] suffix in new[] { new byte[] { 0x7f }, Bytes("\n"), data })
                {
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(data, suffix));
                    AssertHeader(result, complete.Width, complete.Height, complete.BitDepth, complete.ChannelCount);
                    AssertNoFrame(result);
                }
            }
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-0")]
        [InlineData("+0.000")]
        [InlineData("0e12")]
        [InlineData("-0e-99999999999999999999999999")]
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("-Infinity")]
        [InlineData(".")]
        [InlineData("1e")]
        [InlineData("1x")]
        [InlineData("--1")]
        [InlineData("+")]
        [InlineData("e1")]
        [InlineData("1e+")]
        [InlineData("1e-")]
        [InlineData("1.2.3")]
        [InlineData("1e2e3")]
        [InlineData("1e2.3")]
        [InlineData("0x1")]
        [InlineData("1,5")]
        public void IdentifyRejectsMalformedPfmScaleBeforePublishingFacts(string scale)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Bytes("PF\n1 1\n" + scale + "\n"));
            Assert.False(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown);
            AssertNoFrame(result);
        }

        [Theory]
        [InlineData("PF", 3, 2, 1)]
        [InlineData("Pf", 2, 3, 1)]
        [InlineData("PF", 1, 1, 11)]
        [InlineData("Pf", 1, 1, 5)]
        public void IdentifyKeepsPfmHeaderFactsWhenPayloadIsIncomplete(string magic, int width, int height, int payloadBytes)
        {
            int channels = magic == "PF" ? 3 : 1;
            byte[] data = Join(Bytes(magic + "\n" + width + " " + height + "\n-1\n"), new byte[payloadBytes]);
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            AssertHeader(result, width, height, 32, channels);
            AssertNoFrame(result);
        }

        [Fact]
        public void IdentifyScansLongPfmScaleAndCommentsWithoutTokenAllocation()
        {
            string comment = new string('x', 65536);
            string zeros = new string('0', 65536);
            byte[] data = Join(Bytes("PF\n# " + comment + "\r1 1\n-" + zeros + "1e+" + zeros + "0\n"), new byte[12]);
            AssertComplete(ImgCodecsCv2.Identify(data), 1, 1, 32, 3);
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int repeat = 0; repeat < 8; ++repeat) GC.KeepAlive(ImgCodecsCv2.Identify(data));
            long allocated = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            Assert.InRange(allocated, 0, 32768);
            ImageIdentifyResult zero = ImgCodecsCv2.Identify(Bytes("Pf\n1 1\n-" + zeros + "0e999999\n"));
            Assert.False(zero.IsSizeKnown);
            AssertNoFrame(zero);
        }

        [Theory]
        [InlineData("PF", 3)]
        [InlineData("Pf", 1)]
        public void IdentifyBoundsHugePfmGeometryWithoutAllocatingSamples(string magic, int channels)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Bytes(magic + "\n2147483647 2147483647\n-1\n"));
            AssertHeader(result, int.MaxValue, int.MaxValue, 32, channels);
            AssertNoFrame(result);
        }

        [Theory]
        [InlineData("0 1")]
        [InlineData("1 -1")]
        [InlineData("2147483648 1")]
        [InlineData("1x 1")]
        [InlineData("1 1.5")]
        [InlineData("+1 1")]
        [InlineData("1\0 1")]
        public void IdentifyRejectsMalformedPfmGeometryTokens(string geometry)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Bytes("PF\n" + geometry + "\n-1\n"));
            Assert.False(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown);
            AssertNoFrame(result);
        }

        [Fact]
        public void PfmHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams()
        {
            byte[] data = Convert.FromBase64String((string)FixtureAt(0)[7]);
            ImageDecodeOptions exact = Options(data.Length, 3, 2, 6, 6, 32, 3, 72);
            var limits = new[]
            {
                Options(data.Length - 1, 3, 2, 6, 6, 32, 3, 72),
                Options(data.Length, 2, 2, 6, 6, 32, 3, 72),
                Options(data.Length, 3, 1, 6, 6, 32, 3, 72),
                Options(data.Length, 3, 2, 5, 6, 32, 3, 72),
                Options(data.Length, 3, 2, 6, 5, 32, 3, 72),
                Options(data.Length, 3, 2, 6, 6, 16, 3, 72),
                Options(data.Length, 3, 2, 6, 6, 32, 2, 72),
                Options(data.Length, 3, 2, 6, 6, 32, 3, 71)
            };
            foreach (ImageDecodeOptions options in limits)
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, options));
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(data), options));
                using (var stream = new MemoryStream(Join(new byte[] { 0 }, data)))
                {
                    stream.Position = 1;
                    Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadStream(data))
                {
                    Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(options.MaxInputBytes < data.Length ? options.MaxInputBytes + 1 : data.Length, stream.BytesRead);
                }
            }

            using (var stream = new MemoryStream(Join(new byte[] { 0 }, data)))
            {
                stream.Position = 1;
                AssertComplete(ImgCodecsCv2.Identify(stream, exact), 3, 2, 32, 3);
                Assert.Equal(1, stream.Position);
            }
            using (var stream = new ShortReadStream(data))
            {
                AssertComplete(ImgCodecsCv2.Identify(stream, exact), 3, 2, 32, 3);
                Assert.InRange(stream.BytesRead, 1, data.Length);
            }
        }

        [Fact]
        public void PfmStrictPolicyRejectsUnknownHeaderOrMetadataBeforeNativeDecode()
        {
            byte[] invalidScale = Bytes("PF\n1 1\n0\n");
            byte[] valid = Join(Bytes("Pf\n1 1\n-1\n"), new byte[4]);
            ImageDecodeOptions strict = new ImageDecodeOptions(
                4096, 4, 4, 16, 1, true, true, long.MaxValue, long.MaxValue, true, true,
                long.MaxValue, 32, 3, true, long.MaxValue);
            foreach (byte[] data in new[] { invalidScale, valid })
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, strict));
                using (var stream = new MemoryStream(data))
                {
                    Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.Identify(stream, strict));
                    Assert.Equal(0, stream.Position);
                }
            }
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusPfmNeverEscapesOrPublishesInconsistentFloatFacts()
        {
            foreach (object[] fixture in RealPfmFixtures)
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            {
                byte[] data = Convert.FromBase64String((string)fixture[7]);
                for (int offset = 0; offset < data.Length; ++offset)
                {
                    if (data[offset] == replacement) continue;
                    byte[] mutated = (byte[])data.Clone();
                    mutated[offset] = replacement;
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                    if (result.IsSizeKnown)
                    {
                        Assert.True(result.IsPixelFormatKnown);
                        Assert.InRange(result.Width, 1, int.MaxValue);
                        Assert.InRange(result.Height, 1, int.MaxValue);
                        Assert.Equal(32, result.BitDepth);
                        Assert.InRange(result.ChannelCount, 1, 3);
                    }
                    if (result.IsFrameCountKnown)
                    {
                        Assert.Equal(1, result.FrameCount);
                        Assert.True(result.IsSizeKnown);
                        Assert.True(result.IsCumulativePixelCountKnown);
                        Assert.True(result.IsEstimatedPixelBytesKnown);
                        Assert.Equal((long)result.Width * result.Height, result.CumulativePixelCount);
                        Assert.Equal(result.CumulativePixelCount * result.ChannelCount * 4L, result.EstimatedPixelBytes);
                    }
                    else
                    {
                        Assert.False(result.IsCumulativePixelCountKnown);
                        Assert.False(result.IsEstimatedPixelBytesKnown);
                    }
                    Assert.False(result.IsMetadataSizeKnown);
                    Assert.False(result.IsIccProfileSizeKnown);
                }
            }
        }

        // Format bytes independently checked by NumPy; six conventional files
        // also decoded by Netpbmfile. CR/LF and TAB raster cases use exact bytes.
        public static IEnumerable<object[]> RealPfmFixtures
        {
            get
            {
                yield return new object[] { "rgblittle", 3, 2, 32, 3, 82, "c697169b47a3434f8aa2056a9d831ea36c22acb30b919ee6d5e28b232893bf57", "UEYKMyAyCi0xCgAAkL8AAGC/AAAgvwAAwL4AAAC+AAAAPgAAwD4AACA/AABgPwAAkD8AALA/AADQPwAA8D8AAAhAAAAYQAAAKEAAADhAAABIQA==" };
                yield return new object[] { "rgbbig", 3, 2, 32, 3, 81, "b06591cf9f8339daadd999df049a58b88dc64c25af4bc405a76eead2afd061d1", "UEYKMyAyCjEKv5AAAL9gAAC/IAAAvsAAAL4AAAA+AAAAPsAAAD8gAAA/YAAAP5AAAD+wAAA/0AAAP/AAAEAIAABAGAAAQCgAAEA4AABASAAA" };
                yield return new object[] { "graylittle", 4, 3, 32, 1, 58, "d62fc65c033f0840c0a69758d7612fe88be1f815b26fb57f82b087bf05006d87", "UGYKNCAzCi0xCgAAkL8AAGC/AAAgvwAAwL4AAAC+AAAAPgAAwD4AACA/AABgPwAAkD8AALA/AADQPw==" };
                yield return new object[] { "graybig", 4, 3, 32, 1, 57, "bf34fb16f6f0b0106b81928fb274b5b4e1f85fa7d194eb6f7e2f0e97cdf18bff", "UGYKNCAzCjEKv5AAAL9gAAC/IAAAvsAAAL4AAAA+AAAAPsAAAD8gAAA/YAAAP5AAAD+wAAA/0AAA" };
                yield return new object[] { "rgbcomment", 2, 2, 32, 3, 78, "8aba1ba3622f48b5952bb984eaaf2120debbc32e1b1ed595724e26642d2b3d6e", "UEYKIyBjb21wYXRpYmxlIGNvbW1lbnQKMiAyCjIKv5AAAL9gAAC/IAAAvsAAAL4AAAA+AAAAPsAAAD8gAAA/YAAAP5AAAD+wAAA/0AAA" };
                yield return new object[] { "graycrcomment", 4, 3, 32, 1, 81, "818984055af75c90a9d8eeb51c24f21802c15f015d706223e43ff8049e466738", "UGYKIyBjb21wYXRpYmxlIGNvbW1lbnQNNCAzCi0wLjUKAACQvwAAYL8AACC/AADAvgAAAL4AAAA+AADAPgAAID8AAGA/AACQPwAAsD8AANA/" };
                yield return new object[] { "rgbcrlfpixel", 1, 1, 32, 3, 22, "cfc12d0a3f9fd3cf3a9bd0ee9c905a8eba7e1d6a6930f306072c131053409adf", "UEYKMSAxCi0xDQoAAAAAAGC/AAAgvw==" };
                yield return new object[] { "graytabpixel", 1, 1, 32, 1, 13, "f02d0b0f8348a8789f109830afee2aabb7dccd26b7bc6486a6416f47a086572d", "UGYKMSAxCjEJIwAAAA==" };
            }
        }

        private static object[] FixtureAt(int index)
        {
            int current = 0;
            foreach (object[] fixture in RealPfmFixtures)
            {
                if (current++ == index) return fixture;
            }
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        private static ImageDecodeOptions Options(int length, int width, int height, long pixels,
            long cumulative, int depth, int channels, long storage)
        {
            return new ImageDecodeOptions(length, width, height, pixels, 1, true, true, long.MaxValue, long.MaxValue, false, false,
                cumulative, depth, channels, true, storage);
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
                if (taken <= 0) return 0;
                Array.Copy(data, BytesRead, buffer, offset, taken);
                BytesRead += taken;
                return taken;
            }
            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
            public override void SetLength(long value) { throw new NotSupportedException(); }
            public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        }

        private static byte[] Bytes(string text) { return Encoding.ASCII.GetBytes(text); }
        private static byte[] Join(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length];
            Array.Copy(first, result, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length);
            return result;
        }
        private static void AssertHeader(ImageIdentifyResult result, int width, int height, int bits, int channels)
        {
            Assert.Equal("pnm", result.Format);
            Assert.True(result.IsSizeKnown);
            Assert.True(result.IsPixelFormatKnown);
            Assert.Equal(width, result.Width);
            Assert.Equal(height, result.Height);
            Assert.Equal(bits, result.BitDepth);
            Assert.Equal(channels, result.ChannelCount);
            Assert.False(result.IsMetadataSizeKnown);
            Assert.False(result.IsIccProfileSizeKnown);
        }
        private static void AssertComplete(ImageIdentifyResult result, int width, int height, int bits, int channels)
        {
            AssertHeader(result, width, height, bits, channels);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(1, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown);
            Assert.Equal((long)width * height, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown);
            Assert.Equal((long)width * height * channels * 4L, result.EstimatedPixelBytes);
        }
        private static void AssertNoFrame(ImageIdentifyResult result)
        {
            Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }
    }
}
