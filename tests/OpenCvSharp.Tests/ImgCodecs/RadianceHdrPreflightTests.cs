using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class RadianceHdrPreflightTests
    {
        [Theory]
        [MemberData(nameof(RealRadianceHdrFixtures))]
        public void IdentifyReadsRealRadianceHdrScanlineFixtures(string name, int width, int height,
            int length, string sha256, string encoded)
        {
            Assert.NotEmpty(name);
            byte[] data = Convert.FromBase64String(encoded);
            Assert.Equal(length, data.Length);
            Assert.Equal(sha256, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)).ToLowerInvariant());
            AssertComplete(ImgCodecsCv2.Identify(data), width, height);
            AssertComplete(ImgCodecsCv2.Identify(new ReadOnlySpan<byte>(data)), width, height);
        }

        [Theory]
        [InlineData("-Y 2 +X 3", 3, 2)]
        [InlineData("-Y 2 -X 3", 3, 2)]
        [InlineData("+Y 2 +X 3", 3, 2)]
        [InlineData("+Y 2 -X 3", 3, 2)]
        [InlineData("+X 3 +Y 2", 3, 2)]
        [InlineData("+X 3 -Y 2", 3, 2)]
        [InlineData("-X 3 +Y 2", 3, 2)]
        [InlineData("-X 3 -Y 2", 3, 2)]
        public void IdentifyReadsEveryRadianceHdrOrientationWithoutApplyingColorTransforms(string resolution, int width, int height)
        {
            AssertComplete(ImgCodecsCv2.Identify(Join(Header(resolution), Flat(width * height))), width, height);
        }

        [Theory]
        [InlineData("32-bit_rle_rgbe")]
        [InlineData("32-bit_rle_xyze")]
        public void IdentifyReadsRadianceHdrSharedExponentStorageWithoutInferringNativeSupport(string format)
        {
            AssertComplete(ImgCodecsCv2.Identify(Join(Header("-Y 2 +X 3", "FORMAT=" + format + "\n"), Flat(6))), 3, 2);
        }

        [Theory]
        [InlineData("FORMAT= 32-bit_rle_rgbe\n")]
        [InlineData("FORMAT=\t32-bit_rle_rgbe\t \n")]
        [InlineData("FORMAT=32-bit_rle_rgbe   \n")]
        [InlineData("FORMAT=\v32-bit_rle_xyze\f\n")]
        public void IdentifyAcceptsRadianceHdrFormatValueWhitespace(string formatLine)
        {
            AssertComplete(ImgCodecsCv2.Identify(Join(Header("-Y 2 +X 3", formatLine), Flat(6))), 3, 2);
        }

        [Theory]
        [InlineData("FORMAT=32-bit_rle_rgbe\nFORMAT=32-bit_rle_rgbe\n")]
        [InlineData("FORMAT=32-bit_rle_xyze\nFORMAT=32-bit_rle_xyze\n")]
        [InlineData("FORMAT=32-bit_rle_rgbe\nFORMAT=32-bit_rle_xyze\n")]
        [InlineData("FORMAT=32-bit_rle_xyze\nFORMAT=32-bit_rle_rgbe\n")]
        [InlineData("FORMAT=unknown\nFORMAT=32-bit_rle_rgbe\n")]
        [InlineData("FORMAT=32-bit_rle_rgbe\nFORMAT=unknown\n")]
        [InlineData("FORMAT=\nFORMAT=32-bit_rle_rgbe\n")]
        [InlineData("FORMAT=32-bit_rle_rgbe\nFORMAT=\n")]
        public void IdentifyRejectsDuplicateRadianceHdrFormatsEvenWhenValuesMatch(string formatLines)
        {
            AssertUnknownHeader(ImgCodecsCv2.Identify(Join(Header("-Y 2 +X 3", formatLines), Flat(6))));
        }

        [Theory]
        [InlineData("")]
        [InlineData("FORMAT=\n")]
        [InlineData("FORMAT=unknown\n")]
        [InlineData("FORMAT=32-bit_rle_rgbe junk\n")]
        [InlineData("FORMAT=32-bit_rle_rgbe\0\n")]
        [InlineData("FORMAT=32-bit_rle_rgbe#comment\n")]
        [InlineData("# FORMAT=32-bit_rle_rgbe\n")]
        [InlineData(" format=32-bit_rle_rgbe\n")]
        public void IdentifyRequiresOneSupportedRadianceHdrFormatDeclaration(string formatLines)
        {
            AssertUnknownHeader(ImgCodecsCv2.Identify(Join(Header("-Y 2 +X 3", formatLines), Flat(6))));
        }

        [Theory]
        [InlineData("-Y 0 +X 3")]
        [InlineData("-Y 2 +X 0")]
        [InlineData("-Y -2 +X 3")]
        [InlineData("-Y 2147483648 +X 3")]
        [InlineData("+X 3 -Y 2147483648")]
        [InlineData("-Y 2 +Y 3")]
        [InlineData("+X 3 -X 2")]
        [InlineData("Y 2 +X 3")]
        [InlineData("-Y 2 X 3")]
        [InlineData("-y 2 +X 3")]
        [InlineData("-Y 2 +x 3")]
        [InlineData("-Y 2+X 3")]
        [InlineData("-Y 2 +X3")]
        [InlineData("-Y 2 +X 3 junk")]
        [InlineData("-Y 2.0 +X 3")]
        [InlineData("-Y 2 +X 3\0")]
        public void IdentifyRejectsMalformedRadianceHdrResolutionAsOneHeader(string resolution)
        {
            AssertUnknownHeader(ImgCodecsCv2.Identify(Join(Header(resolution), Flat(6))));
        }

        [Theory]
        [InlineData(" -Y 2 +X 3 ")]
        [InlineData("\t-Y\t2\t+X\t3\t")]
        [InlineData("-Y +2 +X +3")]
        [InlineData("+X   3   -Y   2")]
        public void IdentifyAcceptsWhitespaceAndPositiveCountsInRadianceHdrResolution(string resolution)
        {
            AssertComplete(ImgCodecsCv2.Identify(Join(Header(resolution), Flat(6))), 3, 2);
        }

        [Fact]
        public void IdentifyScansLongRadianceHdrLinesWithoutTokenOrScanlineAllocation()
        {
            byte[] data = Join(Bytes("#?RADIANCE\n# " + new string('x', 131072) + "\nFORMAT=32-bit_rle_rgbe\n\n-Y 2 +X 3\n"), Flat(6));
            AssertComplete(ImgCodecsCv2.Identify(data), 3, 2);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int repeat = 0; repeat < 8; ++repeat) GC.KeepAlive(ImgCodecsCv2.Identify(data));
            Assert.InRange(GC.GetAllocatedBytesForCurrentThread() - before, 0, 32768);
        }

        [Theory]
        [InlineData("\n")]
        [InlineData("\r\n")]
        public void IdentifyPreservesRadianceHdrPayloadAfterTerminatedResolutionLine(string newline)
        {
            byte[] header = Bytes("#?RGBE" + newline + "FORMAT=32-bit_rle_rgbe" + newline + newline + "-Y 1 +X 1" + newline);
            byte[] payload = new byte[] { 10, 13, 35, 128 };
            AssertComplete(ImgCodecsCv2.Identify(Join(header, payload)), 1, 1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(257)]
        [InlineData(65537)]
        [InlineData(2147483647)]
        public void IdentifyCountsOldRadianceHdrRunsWithoutExpandingPixels(int width)
        {
            AssertComplete(ImgCodecsCv2.Identify(Join(Header("-Y 1 +X " + width), OldRow(width))), width, 1);
        }

        [Fact]
        public void IdentifyNeverMistakesOldRadianceHdrRunBytesForLiteralPixels()
        {
            // Eight encoded bytes expand to three pixels, not the declared two.
            byte[] payload = Join(Pixel(), new byte[] { 1, 1, 1, 2 });
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Header("-Y 1 +X 2"), payload));
            AssertHeader(result, 2, 1);
            AssertNoFrame(result);
        }

        [Fact]
        public void IdentifyResetsOldRadianceHdrRunShiftAfterEachLiteral()
        {
            byte[] payload = Join(Pixel(), new byte[] { 1, 1, 1, 0, 1, 1, 1, 1 });
            payload = Join(Join(payload, Pixel()), new byte[] { 1, 1, 1, 2 });
            AssertComplete(ImgCodecsCv2.Identify(Join(Header("-Y 1 +X 260"), payload)), 260, 1);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void IdentifyNeverCarriesOldRadianceHdrRunsAcrossScanlinesOrIntegerShifts(int malformed)
        {
            string resolution = "-Y 1 +X 3";
            byte[] payload;
            switch (malformed)
            {
                case 0: payload = new byte[] { 1, 1, 1, 3 }; break;
                case 1: payload = Join(Pixel(), new byte[] { 1, 1, 1, 3 }); break;
                case 2: payload = Join(Pixel(), new byte[] { 1, 1, 1 }); break;
                case 3: payload = Join(Pixel(), new byte[] { 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1 }); break;
                case 4: payload = Join(Pixel(), new byte[] { 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 1, 1, 1 }); break;
                case 5: resolution = "-Y 2 +X 2"; payload = Join(Pixel(), new byte[] { 1, 1, 1, 3 }); break;
                default: resolution = "-Y 2 +X 2"; payload = Join(OldRow(2), new byte[] { 1, 1, 1, 2 }); break;
            }
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Header(resolution), payload));
            Assert.True(result.IsSizeKnown);
            AssertNoFrame(result);
        }

        [Fact]
        public void IdentifyBoundsLongZeroCarryChainsInOldRadianceHdrRuns()
        {
            using (var stream = new MemoryStream())
            {
                Write(stream, Header("-Y 1 +X 2"));
                Write(stream, Pixel());
                for (int index = 0; index < 16384; ++index) Write(stream, new byte[] { 1, 1, 1, 0 });
                Write(stream, Pixel());
                AssertComplete(ImgCodecsCv2.Identify(stream.ToArray()), 2, 1);
                Write(stream, new byte[] { 1, 1, 1, 1 });
                AssertNoFrame(ImgCodecsCv2.Identify(stream.ToArray()));
            }
        }

        [Fact]
        public void IdentifyReadsEveryModernRadianceHdrPacketLengthWithoutExpandingPixels()
        {
            for (int count = 1; count <= 128; ++count)
            {
                AssertComplete(ImgCodecsCv2.Identify(Join(Header("-Y 1 +X " + count), NewRow(count, true))), count, 1);
                if (count < 128)
                    AssertComplete(ImgCodecsCv2.Identify(Join(Header("-Y 1 +X " + count), NewRow(count, false))), count, 1);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        public void IdentifyRejectsMalformedModernRadianceHdrPacketsButKeepsHeader(int malformed)
        {
            byte[] payload = Join(NewRow(8, false), NewRow(8, false));
            switch (malformed)
            {
                case 0: payload[4] = 0; break;
                case 1: payload[4] = 137; break;
                case 2: payload[4] = 9; break;
                case 3: payload[3] = 7; break;
                case 4: Array.Resize(ref payload, payload.Length - 1); break;
                case 5: payload = Join(payload, new byte[] { 0 }); break;
                default: payload[12 + 3] = 9; break;
            }
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Header("-Y 2 +X 8"), payload));
            AssertHeader(result, 8, 2);
            AssertNoFrame(result);
        }

        [Fact]
        public void IdentifyUsesSecondRadianceHdrAxisAsRleScanlineLength()
        {
            byte[] payload = Join(Join(NewRow(8, false), NewRow(8, true)), NewRow(8, false));
            AssertComplete(ImgCodecsCv2.Identify(Join(Header("+X 3 -Y 8"), payload)), 3, 8);
            ImageIdentifyResult wrongLength = ImgCodecsCv2.Identify(Join(Header("+X 3 -Y 8"), Join(Join(NewRow(3, false), NewRow(3, false)), NewRow(3, false))));
            AssertHeader(wrongLength, 3, 8);
            AssertNoFrame(wrongLength);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IdentifyChecksEachRadianceHdrScanlineEncodingIndependently(bool firstNew, bool secondNew)
        {
            byte[] first = firstNew ? NewRow(8, false) : OldRow(8);
            byte[] second = secondNew ? NewRow(8, true) : Flat(8);
            AssertComplete(ImgCodecsCv2.Identify(Join(Header("-Y 2 +X 8"), Join(first, second))), 8, 2);
        }

        [Fact]
        public void IdentifyRequiresExactRadianceHdrScanlinesAndPreservesProvenHeaders()
        {
            foreach (object[] fixture in RealRadianceHdrFixtures)
            {
                byte[] data = Convert.FromBase64String((string)fixture[5]);
                ImageIdentifyResult complete = ImgCodecsCv2.Identify(data);
                for (int length = 1; length < data.Length; ++length)
                {
                    byte[] prefix = new byte[length];
                    Array.Copy(data, prefix, length);
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                    AssertNoFrame(result);
                    if (result.IsSizeKnown) AssertHeader(result, complete.Width, complete.Height);
                }
                foreach (byte[] suffix in new[] { new byte[] { 0 }, Bytes("\n"), data })
                {
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(data, suffix));
                    AssertHeader(result, complete.Width, complete.Height);
                    AssertNoFrame(result);
                }
            }
        }

        [Fact]
        public void IdentifyKeepsHugeRadianceHdrGeometryWithoutAllocatingScanlines()
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Header("+X 2147483647 -Y 2147483647"));
            AssertHeader(result, int.MaxValue, int.MaxValue);
            AssertNoFrame(result);
        }

        [Fact]
        public void RadianceHdrBudgetsRejectBeforeNativeDecodeAndPreserveStreams()
        {
            byte[] data = Join(Header("-Y 2 +X 8"), Join(NewRow(8, false), NewRow(8, false)));
            ImageDecodeOptions exact = Options(data.Length, 8, 2, 16, 16, 8, 4, 64);
            var limits = new[]
            {
                Options(data.Length - 1, 8, 2, 16, 16, 8, 4, 64),
                Options(data.Length, 7, 2, 16, 16, 8, 4, 64),
                Options(data.Length, 8, 1, 16, 16, 8, 4, 64),
                Options(data.Length, 8, 2, 15, 16, 8, 4, 64),
                Options(data.Length, 8, 2, 16, 15, 8, 4, 64),
                Options(data.Length, 8, 2, 16, 16, 7, 4, 64),
                Options(data.Length, 8, 2, 16, 16, 8, 3, 64),
                Options(data.Length, 8, 2, 16, 16, 8, 4, 63)
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
                AssertComplete(ImgCodecsCv2.Identify(stream, exact), 8, 2);
                Assert.Equal(1, stream.Position);
            }
            using (var stream = new ShortReadStream(data)) AssertComplete(ImgCodecsCv2.Identify(stream, exact), 8, 2);
        }

        [Fact]
        public void RadianceHdrStrictPolicyRejectsUnknownHeaderOrMetadataBeforeNativeDecode()
        {
            byte[] invalid = Join(Header("-Y 1 +X 1", "FORMAT=unknown\n"), Pixel());
            byte[] valid = Join(Header("-Y 1 +X 1"), Pixel());
            ImageDecodeOptions strict = new ImageDecodeOptions(4096, 8, 8, 64, 1, true, true,
                long.MaxValue, long.MaxValue, true, true, long.MaxValue, 8, 4, true, long.MaxValue);
            foreach (byte[] data in new[] { invalid, valid })
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
        public void IdentifyDeterministicMutationCorpusRadianceHdrNeverEscapesOrPublishesInconsistentScanlineFacts()
        {
            foreach (object[] fixture in RealRadianceHdrFixtures)
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            {
                byte[] data = Convert.FromBase64String((string)fixture[5]);
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
                        Assert.Equal(8, result.BitDepth);
                        Assert.Equal(4, result.ChannelCount);
                    }
                    if (result.IsFrameCountKnown)
                    {
                        Assert.Equal(1, result.FrameCount);
                        Assert.True(result.IsCumulativePixelCountKnown);
                        Assert.True(result.IsEstimatedPixelBytesKnown);
                        Assert.Equal((long)result.Width * result.Height, result.CumulativePixelCount);
                        Assert.Equal(result.CumulativePixelCount * 4L, result.EstimatedPixelBytes);
                    }
                    else AssertNoFrame(result);
                    Assert.False(result.IsMetadataSizeKnown);
                    Assert.False(result.IsIccProfileSizeKnown);
                }
            }
        }

        // All stored COLR samples independently decoded by the official Radiance
        // color.c functions; 11 conventional files also checked by imagecodecs.
        public static IEnumerable<object[]> RealRadianceHdrFixtures
        {
            get
            {
                yield return new object[] { "orient0", 3, 2, 69, "20f1cfafec438d3e9a5ecbbbbaa7e20cab1e23dff2c7dcb75de266a095f98ee3", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgotWSAyICtYIDMKgEAggYBAIIGAQCCBgUEhgoFBIYKBQSGC" };
                yield return new object[] { "orient1", 3, 2, 69, "1128002c08be352470fbcbc65eaae9acce0afd0a18e0aaf0e2e2ea70c81cc37b", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgotWSAyIC1YIDMKgEAggYBAIIGAQCCBgUEhgoFBIYKBQSGC" };
                yield return new object[] { "orient2", 3, 2, 69, "5b02310030ac2a5a4aa44d5460301fef856d1b06ca0db60ae59e35ddeef97f8e", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgorWSAyICtYIDMKgEAggYBAIIGAQCCBgUEhgoFBIYKBQSGC" };
                yield return new object[] { "orient3", 3, 2, 69, "be18a795d4f80d7d29ed1fdf86cef46b71a9536ada87d0b6aeb119d0eed547b5", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgorWSAyIC1YIDMKgEAggYBAIIGAQCCBgUEhgoFBIYKBQSGC" };
                yield return new object[] { "orient4", 3, 2, 69, "56b822f4e1209c0eb674f742b3e35b512ea029357675f924c18ec9b0ecfc4b48", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgorWCAzICtZIDIKgEAggYBAIIGBQSGCgUEhgoJCIoOCQiKD" };
                yield return new object[] { "orient5", 3, 2, 69, "d156ce16890d542cda044ec70572c12b085cbf76bcfcfa56be4fcc42dac44637", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgorWCAzIC1ZIDIKgEAggYBAIIGBQSGCgUEhgoJCIoOCQiKD" };
                yield return new object[] { "orient6", 3, 2, 69, "1291cdbbfcc752d183ed54b61722b0edd6fd9b3267b9f0ff8c93a410b2eacae7", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgotWCAzICtZIDIKgEAggYBAIIGBQSGCgUEhgoJCIoOCQiKD" };
                yield return new object[] { "orient7", 3, 2, 69, "d290763a196f9bf66dbb8f56f3223fe94cc7d84af35391e00eea5b5316bebd69", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgotWCAzIC1ZIDIKgEAggYBAIIGBQSGCgUEhgoJCIoOCQiKD" };
                yield return new object[] { "rgbnew", 12, 3, 82, "2ee035659ca7e5ae31669cef34ee5b01663f549aad24469c618c44b0e4ec4b23", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgotWSAzICtYIDEyCgICAAyMgIxAjCCMgQICAAyMgYxBjCGMggICAAyMgoxCjCKMgw==" };
                yield return new object[] { "rgbold", 8, 2, 61, "6067af967c42fe36f3892c9c153894fefa93db369ba2fa12beda0f8df5223fb1", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgotWSAyICtYIDgKgEAggQEBAQeBQSGCAQEBBw==" };
                yield return new object[] { "oldcarry", 257, 1, 59, "a3ed49ce3e80df6c5836ac853b11ae0ab4c57a86192fded0bd78b5f68dea8994", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgotWSAxICtYIDI1NwqAQCCBAQEBAAEBAQE=" };
                yield return new object[] { "mixed", 12, 3, 118, "675f55771f4bdb260a5b4304444b802f2e218ead58e3c42cc5e5e7cec0090748", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgotWSAzICtYIDEyCgICAAyMgIxAjCCMgYFBIYKBQSGCgUEhgoFBIYKBQSGCgUEhgoFBIYKBQSGCgUEhgoFBIYKBQSGCgUEhggICAAyMgoxCjCKMgw==" };
                yield return new object[] { "xyznew", 12, 2, 70, "652fb4403a12d755f7774a521fa3178294dda058648f3e5b33513e34d80880a4", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV94eXplCgotWSAyICtYIDEyCgICAAyMgIxAjCCMgQICAAyMgYxBjCGMgg==" };
                yield return new object[] { "crlf", 3, 2, 73, "563801b6c2b5e6d797d1bf0641c163a977f80f29b8876644455ba8309804ab5e", "Iz9SQURJQU5DRQ0KRk9STUFUPTMyLWJpdF9ybGVfcmdiZQ0KDQotWSAyICtYIDMNCoBAIIGAQCCBgEAggYFBIYKBQSGCgUEhgg==" };
                yield return new object[] { "longcomment", 3, 2, 328, "9da89195945fd2f311493a8696e0dc3e9b2d7e6395c11abddc77665e8a6734c1", "Iz9SQURJQU5DRQojIHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHh4eHgKRk9STUFUPTMyLWJpdF9ybGVfcmdiZQoKLVkgMiArWCAzCoBAIIGAQCCBgEAggYFBIYKBQSGCgUEhgg==" };
                yield return new object[] { "literal128", 128, 1, 567, "ee01f57773af27e0bf90d84705754defb5eba5050a2af618c141c3baedc387d6", "Iz9SQURJQU5DRQpGT1JNQVQ9MzItYml0X3JsZV9yZ2JlCgotWSAxICtYIDEyOAoCAgCAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAgCAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAggIGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGBgYGB" };
            }
        }

        private static ImageDecodeOptions Options(int length, int width, int height, long pixels,
            long cumulative, int depth, int channels, long storage)
        {
            return new ImageDecodeOptions(length, width, height, pixels, 1, true, true, long.MaxValue, long.MaxValue,
                false, false, cumulative, depth, channels, true, storage);
        }

        private static byte[] Header(string resolution, string formatLines = "FORMAT=32-bit_rle_rgbe\n")
        {
            return Bytes("#?RADIANCE\n" + formatLines + "\n" + resolution + "\n");
        }
        private static byte[] Pixel() { return new byte[] { 128, 64, 32, 129 }; }
        private static byte[] Flat(int pixels)
        {
            byte[] data = new byte[checked(pixels * 4)];
            for (int index = 0; index < pixels; ++index) Array.Copy(Pixel(), 0, data, index * 4, 4);
            return data;
        }
        private static byte[] OldRow(int width)
        {
            using (var stream = new MemoryStream())
            {
                Write(stream, Pixel());
                int repeat = width - 1;
                while (repeat != 0)
                {
                    Write(stream, new byte[] { 1, 1, 1, (byte)(repeat & 255) });
                    repeat >>= 8;
                }
                return stream.ToArray();
            }
        }
        private static byte[] NewRow(int width, bool literal)
        {
            using (var stream = new MemoryStream())
            {
                Write(stream, new byte[] { 2, 2, (byte)(width >> 8), (byte)width });
                for (int channel = 0; channel < 4; ++channel)
                {
                    int remaining = width;
                    while (remaining > 0)
                    {
                        int count = Math.Min(remaining, literal ? 128 : 127);
                        stream.WriteByte((byte)(literal ? count : count + 128));
                        for (int index = 0; index < (literal ? count : 1); ++index) stream.WriteByte(Pixel()[channel]);
                        remaining -= count;
                    }
                }
                return stream.ToArray();
            }
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
        private static void Write(Stream stream, byte[] data) { stream.Write(data, 0, data.Length); }
        private static byte[] Join(byte[] first, byte[] second)
        {
            byte[] data = new byte[first.Length + second.Length];
            Array.Copy(first, data, first.Length);
            Array.Copy(second, 0, data, first.Length, second.Length);
            return data;
        }
        private static void AssertHeader(ImageIdentifyResult result, int width, int height)
        {
            Assert.Equal("hdr", result.Format);
            Assert.True(result.IsSizeKnown);
            Assert.True(result.IsPixelFormatKnown);
            Assert.Equal(width, result.Width);
            Assert.Equal(height, result.Height);
            Assert.Equal(8, result.BitDepth);
            Assert.Equal(4, result.ChannelCount);
            Assert.False(result.IsMetadataSizeKnown);
            Assert.False(result.IsIccProfileSizeKnown);
        }
        private static void AssertComplete(ImageIdentifyResult result, int width, int height)
        {
            AssertHeader(result, width, height);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(1, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown);
            Assert.Equal((long)width * height, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown);
            Assert.Equal((long)width * height * 4, result.EstimatedPixelBytes);
        }
        private static void AssertUnknownHeader(ImageIdentifyResult result)
        {
            Assert.False(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown);
            AssertNoFrame(result);
        }
        private static void AssertNoFrame(ImageIdentifyResult result)
        {
            Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }
    }
}
