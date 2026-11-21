using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class PnmHeaderPreflightTests
    {
        [Theory]
        [InlineData("0101")]
        [InlineData("0000")]
        [InlineData("1111")]
        [InlineData("01 01")]
        [InlineData("0\t1\v0\f1")]
        [InlineData("0# first\r1# second\n0# third\r\n1")]
        public void IdentifyReadsPlainPbmPixelsAsIndividualBits(string raster)
        {
            AssertComplete(ImgCodecsCv2.Identify(Bytes("P1\n2 2\n" + raster + "\n")), 2, 2, 1, 1);
        }

        [Theory]
        [InlineData("00")]
        [InlineData("01")]
        [InlineData("00001")]
        [InlineData("00000000000000000000000000000001")]
        public void IdentifyNeverCollapsesMultiplePlainPbmBitsIntoOneSample(string raster)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Bytes("P1\n1 1\n" + raster + "\n"));
            AssertHeader(result, 1, 1, 1, 1); AssertNoFrame(result);
        }

        [Theory]
        [InlineData("P1")]
        [InlineData("P2")]
        [InlineData("P3")]
        [InlineData("P4")]
        [InlineData("P5")]
        [InlineData("P6")]
        public void IdentifyStopsPnmHeaderCommentsAtCarriageReturn(string magic)
        {
            string text = magic + "\n# dimensions\r2 # height\r2\n";
            if (magic != "P1" && magic != "P4") text += "# maxval\r255\n";
            int kind = magic[1] - '0';
            byte[] raster = kind == 1 ? Bytes("0101\n") : kind == 2 ? Bytes("0 1 2 3\n") :
                kind == 3 ? Bytes("0 1 2 3 4 5 6 7 8 9 10 11\n") : new byte[kind == 4 ? 2 : kind == 5 ? 4 : 12];
            AssertComplete(ImgCodecsCv2.Identify(Join(Bytes(text), raster)), 2, 2, kind == 1 || kind == 4 ? 1 : 8, kind == 3 || kind == 6 ? 3 : 1);
        }

        [Theory]
        [InlineData("P4")]
        [InlineData("P5")]
        [InlineData("P6")]
        public void IdentifyPreservesLineFeedRasterByteAfterRawPnmCarriageReturnDelimiter(string magic)
        {
            byte[] raster = new byte[magic == "P6" ? 3 : 1]; raster[0] = 10;
            string header = magic + "\n1 1" + (magic == "P4" ? "\r" : "\n255\r");
            AssertComplete(ImgCodecsCv2.Identify(Join(Bytes(header), raster)), 1, 1, magic == "P4" ? 1 : 8, magic == "P6" ? 3 : 1);
        }

        [Theory]
        [InlineData("P4")]
        [InlineData("P5")]
        [InlineData("P6")]
        public void IdentifyNeverSkipsExtraRawPnmLineFeedAsHeaderWhitespace(string magic)
        {
            byte[] raster = new byte[magic == "P6" ? 3 : 1];
            string header = magic + "\n1 1" + (magic == "P4" ? "\r\n" : "\n255\r\n");
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Bytes(header), raster));
            AssertHeader(result, 1, 1, magic == "P4" ? 1 : 8, magic == "P6" ? 3 : 1); AssertNoFrame(result);
        }

        [Theory]
        [InlineData("P4")]
        [InlineData("P5")]
        [InlineData("P6")]
        public void IdentifyTreatsRawPnmCommentCarriageReturnAsItsOnlyRasterDelimiter(string magic)
        {
            string header = magic + "\n1 1" + (magic == "P4" ? "" : "\n255") + "# final comment\r";
            byte[] raster = new byte[magic == "P6" ? 3 : 1]; raster[0] = 10;
            AssertComplete(ImgCodecsCv2.Identify(Join(Bytes(header), raster)), 1, 1, magic == "P4" ? 1 : 8, magic == "P6" ? 3 : 1);
        }

        [Theory]
        [InlineData("P1", "0")]
        [InlineData("P2", "255")]
        [InlineData("P3", "0 1 2")]
        public void IdentifyDoesNotHidePlainPnmTrailingSamplesInsideCarriageReturnComments(string magic, string raster)
        {
            string header = magic + "\n1 1\n" + (magic == "P1" ? "" : "255\n");
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Bytes(header + raster + " # comment\r1\n"));
            AssertHeader(result, 1, 1, magic == "P1" ? 1 : 8, magic == "P3" ? 3 : 1); AssertNoFrame(result);
        }

        [Theory]
        [InlineData("P1\n0 1\n0\n")]
        [InlineData("P2\n1 -1\n255\n0\n")]
        [InlineData("P2\n1 1\n0\n0\n")]
        [InlineData("P3\n1 1\n65536\n0 0 0\n")]
        [InlineData("P4\n2147483648 1\n")]
        [InlineData("P4\n1 1")]
        [InlineData("P5\n1x 1\n255\n")]
        [InlineData("P5\n1 1\n255")]
        [InlineData("P6\n1 1\n-1\n")]
        [InlineData("P6\n1\0 1\n255\n")]
        [InlineData("P3\n1 1x\n255\n0 0 0\n")]
        [InlineData("P2\n1 1\n2.5\n0\n")]
        public void IdentifyRejectsMalformedPnmAdmissionTokens(string header)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Bytes(header));
            Assert.False(result.IsSizeKnown); Assert.False(result.IsPixelFormatKnown); AssertNoFrame(result);
        }

        [Theory]
        [InlineData("P1", "2")]
        [InlineData("P2", "-1")]
        [InlineData("P2", "256")]
        [InlineData("P2", "0x1")]
        [InlineData("P3", "0 1 256")]
        [InlineData("P3", "0 1")]
        public void IdentifyKeepsPnmHeaderFactsWhenAsciiSamplesAreMalformed(string magic, string raster)
        {
            string header = magic + "\n1 1\n" + (magic == "P1" ? "" : "255\n");
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Bytes(header + raster + "\n"));
            AssertHeader(result, 1, 1, magic == "P1" ? 1 : 8, magic == "P3" ? 3 : 1); AssertNoFrame(result);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(15)]
        [InlineData(16)]
        [InlineData(17)]
        public void IdentifyUsesPackedPbmRowsAndIgnoresTheirPaddingBitValues(int width)
        {
            int rowBytes = (width + 7) / 8;
            byte[] raster = new byte[rowBytes * 3]; Array.Fill(raster, (byte)255);
            byte[] header = Bytes("P4\n" + width + " 3\n");
            AssertComplete(ImgCodecsCv2.Identify(Join(header, raster)), width, 3, 1, 1);
            AssertNoFrame(ImgCodecsCv2.Identify(Join(header, new byte[raster.Length - 1])));
            AssertNoFrame(ImgCodecsCv2.Identify(Join(header, new byte[raster.Length + 1])));
        }

        [Theory]
        [InlineData(1, 8)]
        [InlineData(255, 8)]
        [InlineData(256, 16)]
        [InlineData(65535, 16)]
        public void IdentifyDerivesPnmStorageWidthFromMaxvalAcrossPlainAndRawVariants(int maxval, int bits)
        {
            foreach (string magic in new[] { "P2", "P3", "P5", "P6" })
            {
                int channels = magic == "P3" || magic == "P6" ? 3 : 1;
                byte[] raster = magic == "P2" ? Bytes("0\n") : magic == "P3" ? Bytes("0 0 0\n") : new byte[channels * (bits / 8)];
                AssertComplete(ImgCodecsCv2.Identify(Join(Bytes(magic + "\n1 1\n" + maxval + "\n"), raster)), 1, 1, bits, channels);
            }
        }

        [Theory]
        [InlineData("P1")]
        [InlineData("P2")]
        [InlineData("P3")]
        [InlineData("P4")]
        [InlineData("P5")]
        [InlineData("P6")]
        public void IdentifyBoundsPnmDeclaredGeometryWithoutAllocatingSamples(string magic)
        {
            byte[] data = Bytes(magic + "\n2147483647 2147483647\n" + (magic == "P1" || magic == "P4" ? "" : "65535\n"));
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            AssertHeader(result, int.MaxValue, int.MaxValue, magic == "P1" || magic == "P4" ? 1 : 16, magic == "P3" || magic == "P6" ? 3 : 1);
            AssertNoFrame(result);
        }

        [Fact]
        public void IdentifyPreservesEveryRawPnmFirstByteAfterEachSingleWhitespaceDelimiter()
        {
            foreach (string magic in new[] { "P4", "P5", "P6" })
            foreach (byte delimiter in new byte[] { 9, 10, 11, 12, 13, 32 })
            foreach (byte first in new byte[] { 0, 9, 10, 13, 32, 35, 255 })
            {
                byte[] raster = new byte[magic == "P6" ? 3 : 1]; raster[0] = first;
                byte[] header = Join(Bytes(magic + "\n1 1" + (magic == "P4" ? "" : "\n255")), new[] { delimiter });
                AssertComplete(ImgCodecsCv2.Identify(Join(header, raster)), 1, 1, magic == "P4" ? 1 : 8, magic == "P6" ? 3 : 1);
            }
        }

        [Fact]
        public void IdentifyScansLongPnmCommentsAndIntegersWithFixedState()
        {
            string zeros = new string('0', 65536);
            string comment = "# " + new string('x', 65536) + "\r";
            AssertComplete(ImgCodecsCv2.Identify(Bytes("P2\n" + comment + zeros + "1 1\n255\n0\n")), 1, 1, 8, 1);
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Bytes("P1\n1 1\n" + zeros + "1\n"));
            AssertHeader(result, 1, 1, 1, 1); AssertNoFrame(result);
            result = ImgCodecsCv2.Identify(Bytes("P5\n" + new string('9', 65536) + " 1\n255\n"));
            Assert.False(result.IsSizeKnown); AssertNoFrame(result);
        }

        [Fact]
        public void PnmHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams()
        {
            byte[] data = Join(Bytes("P6\n# geometry\r4 3\n65535\r"), new byte[72]);
            ImageDecodeOptions exact = Options(data.Length);
            var limits = new[]
            {
                Options(data.Length - 1), Options(data.Length, width: 3), Options(data.Length, height: 2),
                Options(data.Length, pixels: 11), Options(data.Length, cumulative: 11), Options(data.Length, depth: 8),
                Options(data.Length, channels: 2), Options(data.Length, storage: 71)
            };
            foreach (ImageDecodeOptions options in limits)
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, options));
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(data), options));
            }
            foreach (ImageDecodeOptions options in AllOptions(exact, limits))
            {
                using (var stream = new MemoryStream(Join(new byte[] { 0 }, data)))
                {
                    stream.Position = 1;
                    if (options == exact) AssertComplete(ImgCodecsCv2.Identify(stream, options), 4, 3, 16, 3);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadStream(data))
                {
                    if (options == exact) AssertComplete(ImgCodecsCv2.Identify(stream, options), 4, 3, 16, 3);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.InRange(stream.BytesRead, 1, data.Length);
                    Assert.Equal(options.MaxInputBytes < data.Length ? options.MaxInputBytes + 1 : data.Length, stream.BytesRead);
                }
            }
        }

        [Fact]
        public void PnmStrictPolicyRejectsUnknownHeaderOrMetadataBeforeNativeDecode()
        {
            byte[] malformed = Bytes("P6\n1 1\n65536\n\0\0\0");
            byte[] valid = Join(Bytes("P5\n1 1\n255\n"), new byte[] { 0 });
            foreach (byte[] data in new[] { malformed, valid })
            {
                ImageDecodeOptions options = data == malformed ? Options(data.Length) :
                    new ImageDecodeOptions(data.Length, 4, 3, 12, 1, true, true, long.MaxValue, long.MaxValue, true, true);
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, options));
                using (var stream = new MemoryStream(data))
                {
                    Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.Identify(stream, options));
                    Assert.Equal(0, stream.Position);
                }
            }
        }

        [Theory]
        [MemberData(nameof(RealPnmFixtures))]
        public void IdentifyReadsRealPnmPlainAndRawFixtures(string name, int width, int height, int bits,
            int channels, int length, string sha256, string encoded)
        {
            Assert.NotEmpty(name);
            byte[] data = Convert.FromBase64String(encoded);
            Assert.Equal(length, data.Length);
            Assert.Equal(sha256, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)).ToLowerInvariant());
            AssertComplete(ImgCodecsCv2.Identify(data), width, height, bits, channels);
            AssertComplete(ImgCodecsCv2.Identify(new ReadOnlySpan<byte>(data)), width, height, bits, channels);
        }

        [Fact]
        public void IdentifyRequiresExactStandalonePnmRasterAndPreservesProvenHeaders()
        {
            foreach (byte[] data in Fixtures())
            {
                ImageIdentifyResult complete = ImgCodecsCv2.Identify(data);
                for (int length = 1; length < data.Length; ++length)
                {
                    byte[] prefix = new byte[length]; Array.Copy(data, prefix, length);
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                    // A plain raster can finish before optional trailing whitespace.
                    if (data[1] >= '4') AssertNoFrame(result);
                    if (result.IsFrameCountKnown)
                        AssertComplete(result, complete.Width, complete.Height, complete.BitDepth, complete.ChannelCount);
                }
                foreach (byte[] suffix in new[] { new byte[] { 0 }, Bytes(" 1\n"), data })
                {
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(data, suffix));
                    AssertHeader(result, complete.Width, complete.Height, complete.BitDepth, complete.ChannelCount); AssertNoFrame(result);
                }
            }
            // Format-valid trailing plain text and concatenated raw images are
            // outside this conservative standalone-image completion contract.
            AssertNoFrame(ImgCodecsCv2.Identify(Bytes("P1\n1 1\n0\njunk permitted by plain PBM\n")));
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusPnmNeverEscapesOrPublishesInconsistentRasterFacts()
        {
            foreach (byte[] data in Fixtures())
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            for (int offset = 0; offset < data.Length; ++offset)
            {
                if (data[offset] == replacement) continue;
                byte[] mutated = (byte[])data.Clone(); mutated[offset] = replacement;
                ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                if (result.IsSizeKnown)
                {
                    Assert.True(result.IsPixelFormatKnown); Assert.InRange(result.Width, 1, int.MaxValue);
                    Assert.InRange(result.Height, 1, int.MaxValue); Assert.InRange(result.ChannelCount, 1, 3);
                    Assert.True(result.BitDepth == 1 || result.BitDepth == 8 || result.BitDepth == 16);
                }
                if (result.IsFrameCountKnown)
                {
                    Assert.Equal(1, result.FrameCount); Assert.True(result.IsSizeKnown); Assert.True(result.IsCumulativePixelCountKnown);
                    Assert.True(result.IsEstimatedPixelBytesKnown);
                    Assert.Equal((long)result.Width * result.Height, result.CumulativePixelCount);
                    Assert.Equal(result.CumulativePixelCount * result.ChannelCount * ((result.BitDepth + 7) / 8), result.EstimatedPixelBytes);
                }
                else { Assert.False(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown); }
                Assert.False(result.IsMetadataSizeKnown); Assert.False(result.IsIccProfileSizeKnown);
            }
        }

        // Eleven files were encoded with Netpbmfile 2026.10.15; five explicit
        // format fixtures add compact bits, CR comments and LF-valued pixels.
        // Netpbmfile and NumPy 2.3.5 independently verified every stored sample.
        public static IEnumerable<object[]> RealPnmFixtures
        {
            get
            {
                yield return new object[] { "p1spaced", 4, 3, 1, 1, 54, "e2df54509c4fef28dc687eb9b7bd731f234f5fbc87402396d472213dd9e67115", "UDEKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo0IDMKMQowCjEKMAowCjEKMAoxCjEKMAoxCjAK" };
                yield return new object[] { "p2gray15", 4, 3, 8, 1, 62, "e508beaca52e2ff9c068120a172d5155c3fa0aef838cafd415cca758d9381d4f", "UDIKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo0IDMgMTUKMTEKMTAKOQo4CjAKMTUKMTQKMTMKNQo0CjMKMgo=" };
                yield return new object[] { "p2gray16", 4, 3, 16, 1, 92, "f8f7c10731f21997ec7541a49523b22dc9fa94520046936af2ab1d3622c76e90", "UDIKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo0IDMgNjU1MzUKMTEKNDQyCjg3MwoxMzA0CjEwMDgKMTQzOQoxODcwCjIzMDEKMjAwNQoyNDM2CjI4NjcKMzI5OAo=" };
                yield return new object[] { "p3rgb8", 4, 3, 8, 3, 165, "5138fa37ea8f8c6bde7fc0129dd55712bccf4089374b1f2470b8292bb73e30c5", "UDMKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo0IDMgMjU1CjExIDIwNCAxNDEKMTg2IDEyMyA2MAoxMDUgNDIgMjM1CjI0IDIxNyAxNTQKMjQwIDE3NyAxMTQKMTU5IDk2IDMzCjc4IDE1IDIwOAoyNTMgMTkwIDEyNwoyMTMgMTUwIDg3CjEzMiA2OSA2CjUxIDI0NCAxODEKMjI2IDE2MyAxMDAK" };
                yield return new object[] { "p3rgb16", 4, 3, 16, 3, 178, "dc28f7921f4706b5016a065985f1175e35de5db14c1322636b8f1b30ca16ab2d", "UDMKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo0IDMgMTAyMwoxMSAyMDQgMzk3CjQ0MiA2MzUgODI4Cjg3MyA0MiAyMzUKMjgwIDQ3MyA2NjYKMTAwOCAxNzcgMzcwCjQxNSA2MDggODAxCjg0NiAxNSAyMDgKMjUzIDQ0NiA2MzkKOTgxIDE1MCAzNDMKMzg4IDU4MSA3NzQKODE5IDEwMTIgMTgxCjIyNiA0MTkgNjEyCg==" };
                yield return new object[] { "p4bits4", 4, 3, 1, 1, 33, "5dda20d533928cbeb48b9ea71fca1e007c73997889e17f5c999a4a13e3b5e322", "UDQKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo0IDMKoFCg" };
                yield return new object[] { "p4bits9", 9, 3, 1, 1, 36, "1b47c349cb46ae42d0b0dbde9c6ed37753357e89330b7b6b50c73571950ed63c", "UDQKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo5IDMKqoBVAKqA" };
                yield return new object[] { "p5gray8", 4, 3, 8, 1, 46, "8c8311d3da8272ea3154f93166c407f39ea1eff996212685ac8f153369562015", "UDUKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo0IDMgMjU1Cgu6aRjwn0791YQz4g==" };
                yield return new object[] { "p5gray16", 4, 3, 16, 1, 60, "9808bad1c59f79415a6d9e0b9a80323a17279b78d400b187805cb842f453faec", "UDUKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo0IDMgNjU1MzUKAAsBugNpBRgD8AWfB04I/QfVCYQLMwzi" };
                yield return new object[] { "p6rgb8", 4, 3, 8, 3, 70, "1d6627d2b6cd8b7bf58d38ba042043ef46ed336bf00dd02c61c9f1ad85aed86a", "UDYKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo0IDMgMjU1CgvMjbp7PGkq6xjZmvCxcp9gIU4P0P2+f9WWV4RFBjP0teKjZA==" };
                yield return new object[] { "p6rgb16", 4, 3, 16, 3, 108, "e92fd79ed236be6fc6dfbcc56001dd3a4839adf76f91aae635137376062039aa", "UDYKIyBQTk0gcHJlZmxpZ2h0IHNhbXBsZQo0IDMgNjU1MzUKAAsAzAGNAboCewM8A2kEKgTrBRgF2QaaA/AEsQVyBZ8GYAchB04IDwjQCP0Jvgp/B9UIlglXCYQKRQsGCzML9Ay1DOINow5k" };
                yield return new object[] { "p1compact", 4, 3, 1, 1, 20, "e2e697da17615df747a935f67d4506007f623b13fb9a2f75b2108108fa84483d", "UDEKNCAzCjAxMDExMDEwMDExMAo=" };
                yield return new object[] { "p1commentscr", 4, 3, 1, 1, 43, "ed35e5315be5626534fbdd6a79a23c4d731099ea90ce636a8fce63ad4d524166", "UDEKIyBnZW9tZXRyeQ00IDMKMDEjIHJvdw0wMTEwIyByb3cKMTAwMTEwCg==" };
                yield return new object[] { "p4crlfpixel", 4, 3, 1, 1, 10, "0bd2b80f649cb52153b8dd3e6cb677ebe8bfd2a983a544cc0f4f972c30c4f2e1", "UDQKNCAzDQqP7w==" };
                yield return new object[] { "p5crlfpixel", 4, 3, 8, 1, 23, "c59c4bfcb207ef3a94b4a4b384b6c687353cabd184eea6cbb62419fe4488f1eb", "UDUKNCAzCjI1NQ0KPGuayfgnVoW04xI=" };
                yield return new object[] { "p6crlfpixel", 4, 3, 8, 3, 47, "a23181757e6da7b50c20f6b05f08330296d60ac81ea30af602beb3de9a151767", "UDYKNCAzCjI1NQ0KPGuayfgnVoW04xJBcJ/O/SxbirnoF0Z1pNMCMWCPvu0cS3o=" };
            }
        }

        private static IEnumerable<byte[]> Fixtures()
        {
            foreach (object[] fixture in RealPnmFixtures) yield return Convert.FromBase64String((string)fixture[7]);
        }
        private static ImageDecodeOptions Options(int length, int width = 4, int height = 3, long pixels = 12,
            long cumulative = 12, int depth = 16, int channels = 3, long storage = 72)
        {
            return new ImageDecodeOptions(length, width, height, pixels, 1, true, true, long.MaxValue, long.MaxValue, false, false,
                cumulative, depth, channels, true, storage);
        }
        private static IEnumerable<ImageDecodeOptions> AllOptions(ImageDecodeOptions exact, ImageDecodeOptions[] limits)
        {
            yield return exact; foreach (ImageDecodeOptions options in limits) yield return options;
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

        private static byte[] Bytes(string text) { return Encoding.ASCII.GetBytes(text); }
        private static byte[] Join(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length]; Array.Copy(first, result, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length); return result;
        }
        private static void AssertHeader(ImageIdentifyResult result, int width, int height, int bits, int channels)
        {
            Assert.Equal("pnm", result.Format); Assert.True(result.IsSizeKnown); Assert.True(result.IsPixelFormatKnown);
            Assert.Equal(width, result.Width); Assert.Equal(height, result.Height); Assert.Equal(bits, result.BitDepth);
            Assert.Equal(channels, result.ChannelCount); Assert.False(result.IsMetadataSizeKnown); Assert.False(result.IsIccProfileSizeKnown);
        }
        private static void AssertComplete(ImageIdentifyResult result, int width, int height, int bits, int channels)
        {
            AssertHeader(result, width, height, bits, channels); Assert.True(result.IsFrameCountKnown); Assert.Equal(1, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown); Assert.Equal((long)width * height, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown);
            Assert.Equal((long)width * height * channels * ((bits + 7) / 8), result.EstimatedPixelBytes);
        }
        private static void AssertNoFrame(ImageIdentifyResult result)
        {
            Assert.False(result.IsFrameCountKnown); Assert.False(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown);
        }
    }
}
