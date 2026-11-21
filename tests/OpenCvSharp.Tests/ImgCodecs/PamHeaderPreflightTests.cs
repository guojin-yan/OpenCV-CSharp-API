using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class PamHeaderPreflightTests
    {
        [Theory]
        [InlineData("WIDTH", 4)]
        [InlineData("WIDTH", 1)]
        [InlineData("WIDTH", 2000000000)]
        [InlineData("HEIGHT", 3)]
        [InlineData("HEIGHT", 1)]
        [InlineData("HEIGHT", 2000000000)]
        [InlineData("DEPTH", 3)]
        [InlineData("DEPTH", 1)]
        [InlineData("DEPTH", 2000000000)]
        [InlineData("MAXVAL", 255)]
        [InlineData("MAXVAL", 1)]
        [InlineData("MAXVAL", 65535)]
        public void IdentifyRejectsDuplicatePamAdmissionFieldsEvenWhenValuesMatch(string field, int value)
        {
            string text = Header().Replace("ENDHDR\n", field + " " + value + "\nENDHDR\n");
            AssertUnknown(ImgCodecsCv2.Identify(Bytes(text, new byte[36])));
        }

        [Theory]
        [InlineData("WIDTH", "4", "1")]
        [InlineData("HEIGHT", "3", "1")]
        [InlineData("DEPTH", "3", "1")]
        [InlineData("MAXVAL", "65535", "1")]
        public void IdentifyNeverLetsLaterPamFieldsLowerAnAdmissionBudget(string field, string larger, string smaller)
        {
            int original = field == "WIDTH" ? 4 : field == "HEIGHT" ? 3 : field == "DEPTH" ? 3 : 255;
            string text = Header().Replace(field + " " + original, field + " " + larger + "\n" + field + " " + smaller);
            AssertUnknown(ImgCodecsCv2.Identify(Bytes(text, new byte[36])));
        }

        [Theory]
        [InlineData("WIDTH 4 ")]
        [InlineData("HEIGHT 3\t")]
        [InlineData("DEPTH 3\v\f")]
        [InlineData("MAXVAL 255 \t")]
        [InlineData("ENDHDR \t")]
        public void IdentifyAcceptsPamHeaderLineTrailingWhitespace(string replacement)
        {
            string key = replacement.Substring(0, replacement.IndexOfAny(new[] { ' ', '\t', '\v', '\f' }));
            string original = key == "ENDHDR" ? key : key + " " + (key == "WIDTH" ? 4 : key == "HEIGHT" ? 3 : key == "DEPTH" ? 3 : 255);
            AssertComplete(ImgCodecsCv2.Identify(Bytes(Header().Replace(original + "\n", replacement + "\n"), new byte[36])), 4, 3, 8, 3);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(17)]
        [InlineData(256)]
        [InlineData(65536)]
        public void IdentifyReadsPamArbitraryPositiveDepthWithoutGuessingNativeSupport(int depth)
        {
            AssertComplete(ImgCodecsCv2.Identify(Pam(1, 1, depth)), 1, 1, 8, depth);
        }

        [Theory]
        [InlineData("P7 \n")]
        [InlineData("P7\t\n")]
        [InlineData("P7 WIDTH 4\n")]
        [InlineData("P7\v")]
        public void IdentifyRequiresPamMagicOnItsOwnLine(string magic)
        {
            AssertUnknown(ImgCodecsCv2.Identify(Bytes(magic + Header().Substring(3), new byte[36])));
        }

        [Theory]
        [InlineData("UNKNOWN 1\n")]
        [InlineData("WIDT 4\n")]
        [InlineData("width 4\n")]
        [InlineData("WIDTH12345 4\n")]
        [InlineData("TUPLTYPE\n")]
        [InlineData("TUPLTYPE \t\n")]
        [InlineData("TUPLTYPE RGB\0ALPHA\n")]
        public void IdentifyRejectsUnknownPamKeywordsAndEmptyTupleType(string line)
        {
            AssertUnknown(ImgCodecsCv2.Identify(Bytes(Header().Replace("ENDHDR\n", line + "ENDHDR\n"), new byte[36])));
        }

        [Theory]
        [InlineData("WIDTH", "0")]
        [InlineData("HEIGHT", "-1")]
        [InlineData("DEPTH", "0")]
        [InlineData("MAXVAL", "0")]
        [InlineData("MAXVAL", "65536")]
        [InlineData("WIDTH", "2147483648")]
        [InlineData("HEIGHT", "1x")]
        [InlineData("DEPTH", "3 4")]
        [InlineData("MAXVAL", "255 # inline comment")]
        public void IdentifyRejectsMalformedPamScalarTokens(string key, string value)
        {
            int original = key == "WIDTH" ? 4 : key == "HEIGHT" ? 3 : key == "DEPTH" ? 3 : 255;
            AssertUnknown(ImgCodecsCv2.Identify(Bytes(Header().Replace(key + " " + original, key + " " + value), new byte[36])));
        }

        [Theory]
        [InlineData("WIDTH 4\n")]
        [InlineData("HEIGHT 3\n")]
        [InlineData("DEPTH 3\n")]
        [InlineData("MAXVAL 255\n")]
        public void IdentifyRequiresEveryPamAdmissionFieldBeforeEndhdr(string missing)
        {
            AssertUnknown(ImgCodecsCv2.Identify(Bytes(Header().Replace(missing, ""), new byte[36])));
        }

        [Fact]
        public void IdentifyAcceptsUnorderedPamFieldsCommentsBlankLinesAndRepeatedCustomTupleTypes()
        {
            string text = "P7\n\t# opaque comment \0\u007f\n\n TUPLTYPE  SPECTRAL \t\nMAXVAL\t1023 \n\tDEPTH 5\n" +
                "TUPLTYPE WAVELENGTHS\nHEIGHT 3\n\fWIDTH 4\v\n\tENDHDR \f\n";
            AssertComplete(ImgCodecsCv2.Identify(Bytes(text, new byte[120])), 4, 3, 16, 5);
            // TUPLTYPE labels sample meanings; DEPTH alone defines encoded storage.
            AssertComplete(ImgCodecsCv2.Identify(Pam(4, 3, 5, tuple: "RGB")), 4, 3, 8, 5);
        }

        [Fact]
        public void IdentifyAcceptsPamCrLfWithoutConsumingTheFirstBinarySample()
        {
            foreach (byte first in new byte[] { 0, 9, 10, 13, 32, 35, 255 })
            {
                byte[] payload = new byte[36]; payload[0] = first;
                byte[] data = Bytes(Header().Replace("\n", "\r\n"), payload);
                AssertComplete(ImgCodecsCv2.Identify(data), 4, 3, 8, 3);
            }
        }

        [Theory]
        [InlineData("ENDHDR value\n")]
        [InlineData("ENDHDR # comment\n")]
        [InlineData("ENDHDR\0\n")]
        [InlineData("ENDHDR")]
        public void IdentifyRequiresAnExactTerminatedPamEndhdrLine(string end)
        {
            AssertUnknown(ImgCodecsCv2.Identify(Bytes(Header().Replace("ENDHDR\n", end), new byte[36])));
        }

        [Fact]
        public void IdentifyNeverTreatsPamCommentOrTupleTypeTextAsAHeaderTerminator()
        {
            string text = Header().Replace("ENDHDR\n", "# ENDHDR\nTUPLTYPE ENDHDR\n");
            AssertUnknown(ImgCodecsCv2.Identify(Bytes(text, new byte[36])));
            AssertComplete(ImgCodecsCv2.Identify(Bytes(text + "ENDHDR\n", new byte[36])), 4, 3, 8, 3);
        }

        [Theory]
        [InlineData(1, 8)]
        [InlineData(255, 8)]
        [InlineData(256, 16)]
        [InlineData(65535, 16)]
        public void IdentifyUsesPamMaxvalStorageWidthWithoutGuessingTupleSemantics(int maxval, int bits)
        {
            AssertComplete(ImgCodecsCv2.Identify(Pam(4, 3, 2, maxval)), 4, 3, bits, 2);
        }

        [Fact]
        public void IdentifyBoundsPamGeometryAndStorageOverflowWithoutAllocatingDeclaredDepth()
        {
            byte[] maximum = Bytes(Header(int.MaxValue, int.MaxValue, int.MaxValue, 65535));
            ImageIdentifyResult result = ImgCodecsCv2.Identify(maximum);
            AssertHeader(result, int.MaxValue, int.MaxValue, 16, int.MaxValue); AssertNoFrame(result);
            byte[] deep = Bytes(Header(1, 1, int.MaxValue));
            AssertHeader(ImgCodecsCv2.Identify(deep), 1, 1, 8, int.MaxValue);
            AssertUnknown(ImgCodecsCv2.Identify(Bytes(Header().Replace("DEPTH 3", "DEPTH " + new string('9', 10000)))));
        }

        [Fact]
        public void IdentifyReadsLongPamCommentsAndTupleTypeWithoutHeaderSizedAllocations()
        {
            string comment = "# " + new string('x', 65536) + "\n";
            string tuple = "TUPLTYPE " + new string('A', 65536) + "\n";
            byte[] data = Bytes(Header().Replace("ENDHDR\n", comment + tuple + "ENDHDR\n"), new byte[36]);
            AssertComplete(ImgCodecsCv2.Identify(data), 4, 3, 8, 3);
        }

        [Fact]
        public void PamHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams()
        {
            byte[] data = Pam(4, 3, 5, 65535, tuple: "SPECTRAL");
            ImageDecodeOptions exact = Options(data.Length);
            var limits = new[]
            {
                Options(data.Length, width: 3), Options(data.Length, height: 2), Options(data.Length, pixels: 11),
                Options(data.Length, cumulative: 11), Options(data.Length, depth: 8), Options(data.Length, channels: 4),
                Options(data.Length, storage: 119)
            };
            foreach (ImageDecodeOptions options in limits)
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, options));
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(data), options));
            }
            foreach (ImageDecodeOptions options in AllOptions(exact, limits))
            {
                using (var stream = new MemoryStream(Bytes("!", data)))
                {
                    stream.Position = 1;
                    if (options == exact) AssertComplete(ImgCodecsCv2.Identify(stream, options), 4, 3, 16, 5);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadStream(data))
                {
                    if (options == exact) AssertComplete(ImgCodecsCv2.Identify(stream, options), 4, 3, 16, 5);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(data.Length, stream.BytesRead);
                }
            }
        }

        [Fact]
        public void PamStrictPolicyRejectsAmbiguousHeadersBeforeNativeDecode()
        {
            byte[] data = Bytes(Header().Replace("ENDHDR\n", "WIDTH 1\nENDHDR\n"), new byte[36]);
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, Options(data.Length)));
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(data), Options(data.Length)));
            using (var stream = new MemoryStream(data))
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.Identify(stream, Options(data.Length)));
                Assert.Equal(0, stream.Position);
            }
            using (var stream = new ShortReadStream(data))
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.Identify(stream, Options(data.Length)));
                Assert.Equal(data.Length, stream.BytesRead);
            }
        }

        [Theory]
        [MemberData(nameof(RealPamFixtures))]
        public void IdentifyReadsRealPamTupleAndArbitraryDepthFixtures(string name, int bits, int channels,
            int length, string sha256, string encoded)
        {
            Assert.NotEmpty(name);
            byte[] data = Convert.FromBase64String(encoded);
            Assert.Equal(length, data.Length);
            Assert.Equal(sha256, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)).ToLowerInvariant());
            AssertComplete(ImgCodecsCv2.Identify(data), 4, 3, bits, channels);
            AssertComplete(ImgCodecsCv2.Identify(new ReadOnlySpan<byte>(data)), 4, 3, bits, channels);
        }

        [Fact]
        public void IdentifyRequiresExactPamPayloadButPreservesTheCompleteHeader()
        {
            foreach (byte[] data in Fixtures())
            {
                int payload = FindPayload(data);
                ImageIdentifyResult complete = ImgCodecsCv2.Identify(data);
                Assert.True(complete.IsFrameCountKnown);
                for (int length = 1; length < data.Length; ++length)
                {
                    byte[] prefix = new byte[length]; Array.Copy(data, prefix, length);
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                    AssertNoFrame(result);
                    Assert.Equal(length >= payload, result.IsSizeKnown);
                    Assert.Equal(length >= payload, result.IsPixelFormatKnown);
                }
                foreach (byte[] suffix in new[] { new byte[] { 0 }, Encoding.ASCII.GetBytes("\nENDHDR\n"), data })
                {
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(data, suffix));
                    AssertHeader(result, complete.Width, complete.Height, complete.BitDepth, complete.ChannelCount);
                    AssertNoFrame(result);
                }
            }
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusPamNeverEscapesOrPublishesAmbiguousHeaderFacts()
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
                    Assert.InRange(result.Height, 1, int.MaxValue); Assert.InRange(result.ChannelCount, 1, int.MaxValue);
                    Assert.True(result.BitDepth == 8 || result.BitDepth == 16);
                }
                if (result.IsFrameCountKnown)
                {
                    Assert.Equal("pnm", result.Format); Assert.Equal(1, result.FrameCount); Assert.True(result.IsSizeKnown);
                    Assert.True(result.IsCumulativePixelCountKnown); Assert.True(result.IsEstimatedPixelBytesKnown);
                    Assert.Equal((long)result.Width * result.Height, result.CumulativePixelCount);
                    Assert.Equal(result.CumulativePixelCount * result.ChannelCount * (result.BitDepth / 8), result.EstimatedPixelBytes);
                    Assert.InRange(result.EstimatedPixelBytes, 1, mutated.Length);
                }
                else { Assert.False(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown); }
                Assert.False(result.IsMetadataSizeKnown); Assert.False(result.IsIccProfileSizeKnown);
            }
        }

        private static IEnumerable<byte[]> Fixtures()
        {
            yield return Pam();
            yield return Pam(4, 3, 5, 1023, "CUSTOM", "WAVELENGTHS");
            foreach (object[] fixture in RealPamFixtures) yield return Convert.FromBase64String((string)fixture[5]);
        }

        // Netpbmfile 2026.10.15 encoded the ten visual fixtures. Three byte-authored
        // custom fixtures cover 5/17 channels and repeated TUPLTYPE. Netpbmfile and
        // NumPy independently rechecked every sample, including big-endian 16-bit.
        public static IEnumerable<object[]> RealPamFixtures
        {
            get
            {
                yield return new object[] { "bw1", 8, 1, 102, "64cdb5216d3c09506542985805537c534c6bff41eb6f1bf9da9fbad412ab5c43", "UDcKIyBQQU0gcHJlZmxpZ2h0IHNhbXBsZQpIRUlHSFQgMwpXSURUSCA0CkRFUFRIIDEKTUFYVkFMIDEKVFVQTFRZUEUgQkxBQ0tBTkRXSElURQpFTkRIRFIKAQABAAABAAEBAAEA" };
                yield return new object[] { "bwa1", 8, 2, 120, "3ec4a38db70e7ba6315d633f613786d4aba9ea171df0e61dcbb09befc5f16754", "UDcKIyBQQU0gcHJlZmxpZ2h0IHNhbXBsZQpIRUlHSFQgMwpXSURUSCA0CkRFUFRIIDIKTUFYVkFMIDEKVFVQTFRZUEUgQkxBQ0tBTkRXSElURV9BTFBIQQpFTkRIRFIKAQAAAQEAAAEAAQEAAAEBAAEAAAEBAAAB" };
                yield return new object[] { "gray8", 8, 1, 100, "42e689c3bea08946d225d805a7686c0734cf07765346e4119e91798f9a70d52c", "UDcKIyBQQU0gcHJlZmxpZ2h0IHNhbXBsZQpIRUlHSFQgMwpXSURUSCA0CkRFUFRIIDEKTUFYVkFMIDI1NQpUVVBMVFlQRSBHUkFZU0NBTEUKRU5ESERSCgu6aRjwn0791YQz4g==" };
                yield return new object[] { "gray16", 16, 1, 114, "742f07501019d8445b95908192f98596dfb88aa4df8e05ff37c56066175fc16d", "UDcKIyBQQU0gcHJlZmxpZ2h0IHNhbXBsZQpIRUlHSFQgMwpXSURUSCA0CkRFUFRIIDEKTUFYVkFMIDY1NTM1ClRVUExUWVBFIEdSQVlTQ0FMRQpFTkRIRFIKAAsBugNpBRgD8AWfB04I/QfVCYQLMwzi" };
                yield return new object[] { "grayalpha8", 8, 2, 118, "b6bd14a46db58506b0ddca762817b44bb3579a9ae9942309a9b9345697cb176c", "UDcKIyBQQU0gcHJlZmxpZ2h0IHNhbXBsZQpIRUlHSFQgMwpXSURUSCA0CkRFUFRIIDIKTUFYVkFMIDI1NQpUVVBMVFlQRSBHUkFZU0NBTEVfQUxQSEEKRU5ESERSCgvMuntpKhjZ8LGfYE4P/b7VloRFM/Tiow==" };
                yield return new object[] { "grayalpha16", 16, 2, 143, "88901effa6ab01d63b14780dab2f824a793dbbaa5bd2b99e5681ebc0f6868842", "UDcKIyBQQU0gcHJlZmxpZ2h0IHNhbXBsZQpIRUlHSFQgMwpXSURUSCA0CkRFUFRIIDIKTUFYVkFMIDEwMjMKVFVQTFRZUEUgR1JBWVNDQUxFX0FMUEhBCkVOREhEUgoACwDMAboCewNpACoBGAHZA/AAsQGfAmADTgAPAP0BvgPVAJYBhAJFAzMD9ADiAaM=" };
                yield return new object[] { "rgb8", 8, 3, 118, "144ff72df7fb75a1145180856651be3a540dae712edfb40751266b8c7925768b", "UDcKIyBQQU0gcHJlZmxpZ2h0IHNhbXBsZQpIRUlHSFQgMwpXSURUSCA0CkRFUFRIIDMKTUFYVkFMIDI1NQpUVVBMVFlQRSBSR0IKRU5ESERSCgvMjbp7PGkq6xjZmvCxcp9gIU4P0P2+f9WWV4RFBjP0teKjZA==" };
                yield return new object[] { "rgb16", 16, 3, 156, "13244403ee951ab25709ef501fc43aa0b7bdd303d249840a5656d1e4818c3029", "UDcKIyBQQU0gcHJlZmxpZ2h0IHNhbXBsZQpIRUlHSFQgMwpXSURUSCA0CkRFUFRIIDMKTUFYVkFMIDY1NTM1ClRVUExUWVBFIFJHQgpFTkRIRFIKAAsAzAGNAboCewM8A2kEKgTrBRgF2QaaA/AEsQVyBZ8GYAchB04IDwjQCP0Jvgp/B9UIlglXCYQKRQsGCzML9Ay1DOINow5k" };
                yield return new object[] { "rgba8", 8, 4, 136, "25e5276917ce915b00218f3d9be0ec705ade71f178037abb02aaf3a9058bbb18", "UDcKIyBQQU0gcHJlZmxpZ2h0IHNhbXBsZQpIRUlHSFQgMwpXSURUSCA0CkRFUFRIIDQKTUFYVkFMIDI1NQpUVVBMVFlQRSBSR0JfQUxQSEEKRU5ESERSCgvMjU66ezz9aSrrrBjZmlvwsXIzn2Ah4k4P0JH9vn9A1ZZXGIRFBscz9LV24qNkJQ==" };
                yield return new object[] { "rgba16", 16, 4, 186, "2ee11b247233c81c2b940d8559b9316ad9a856ca9096272d36a2f08c41482958", "UDcKIyBQQU0gcHJlZmxpZ2h0IHNhbXBsZQpIRUlHSFQgMwpXSURUSCA0CkRFUFRIIDQKTUFYVkFMIDY1NTM1ClRVUExUWVBFIFJHQl9BTFBIQQpFTkRIRFIKAAsAzAGNAk4BugJ7AzwD/QNpBCoE6wWsBRgF2QaaB1sD8ASxBXIGMwWfBmAHIQfiB04IDwjQCZEI/Qm+Cn8LQAfVCJYJVwoYCYQKRQsGC8cLMwv0DLUNdgziDaMOZA8l" };
                yield return new object[] { "custom5", 8, 5, 140, "f818705ad91e1694bc90e907ab709e136d07ff9c413c9fb3c3582bb25ec9897f", "UDcKIyBieXRlLWF1dGhvcmVkClRVUExUWVBFIFNQRUNUUkFMCk1BWFZBTCAyNTUKREVQVEggNQpIRUlHSFQgMwpXSURUSCA0CkVOREhEUgoLzI1OD7p7PP2+aSrrrG0Y2ZpbHPCxcjP0n2Ah4qNOD9CRUv2+f0AB1ZZXGNmERQbHiDP0tXY34qNkJeY=" };
                yield return new object[] { "custom17", 16, 17, 494, "87fb84017ba5d8ffc7ecc9fe165797ad3694f251c07cca8ea03a196ac9bf4d04", "UDcKIyBieXRlLWF1dGhvcmVkClRVUExUWVBFIFdBVkVMRU5HVEhTCk1BWFZBTCA2NTUzNQpERVBUSCAxNwpIRUlHSFQgMwpXSURUSCA0CkVOREhEUgoACwDMAY0CTgMPA9AEkQVSBhMG1AeVCFYJFwnYCpkLWgwbAboCewM8A/0EvgV/BkAHAQfCCIMJRAoFCsYLhwxIDQkNygNpBCoE6wWsBm0HLgfvCLAJcQoyCvMLtAx1DTYN9w64D3kFGAXZBpoHWwgcCN0JngpfCyAL4QyiDWMOJA7lD6YQZxEoA/AEsQVyBjMG9Ae1CHYJNwn4CrkLegw7DPwNvQ5+Dz8QAAWfBmAHIQfiCKMJZAolCuYLpwxoDSkN6g6rD2wQLRDuEa8HTggPCNAJkQpSCxML1AyVDVYOFw7YD5kQWhEbEdwSnRNeCP0Jvgp/C0AMAQzCDYMORA8FD8YQhxFIEgkSyhOLFEwVDQfVCJYJVwoYCtkLmgxbDRwN3Q6eD18QIBDhEaISYxMkE+UJhApFCwYLxwyIDUkOCg7LD4wQTREOEc8SkBNRFBIU0xWUCzML9Ay1DXYONw74D7kQehE7EfwSvRN+FD8VABXBFoIXQwziDaMOZA8lD+YQpxFoEikS6hOrFGwVLRXuFq8XcBgxGPI=" };
                yield return new object[] { "custom5multiline", 16, 5, 220, "789c774eb14ff2f90234f85467e8abbf3a936cde643380e119084428b0a9cfd6", "UDcKIyBieXRlLWF1dGhvcmVkClRVUExUWVBFIENVU1RPTQpUVVBMVFlQRSBXQVZFTEVOR1RIUwpNQVhWQUwgMTAyMwpERVBUSCA1CkhFSUdIVCAzCldJRFRIIDQKRU5ESERSCgALAMwBjQJOAw8BugJ7AzwD/QC+A2kAKgDrAawCbQEYAdkCmgNbABwD8ACxAXICMwL0AZ8CYAMhA+IAowNOAA8A0AGRAlIA/QG+An8DQAABA9UAlgFXAhgC2QGEAkUDBgPHAIgDMwP0ALUBdgI3AOIBowJkAyUD5g==" };
            }
        }

        private static int FindPayload(byte[] data)
        {
            byte[] marker = Encoding.ASCII.GetBytes("ENDHDR\n");
            for (int i = 0; i <= data.Length - marker.Length; ++i)
            {
                int j = 0; while (j < marker.Length && data[i + j] == marker[j]) ++j;
                if (j == marker.Length) return i + j;
            }
            throw new InvalidDataException("Fixture must have a complete ENDHDR line.");
        }

        private static void AssertUnknown(ImageIdentifyResult result)
        {
            Assert.Equal("pnm", result.Format); Assert.False(result.IsSizeKnown); Assert.False(result.IsPixelFormatKnown);
            AssertNoFrame(result);
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
            Assert.True(result.IsEstimatedPixelBytesKnown); Assert.Equal((long)width * height * channels * (bits / 8), result.EstimatedPixelBytes);
        }

        private static void AssertNoFrame(ImageIdentifyResult result)
        {
            Assert.False(result.IsFrameCountKnown); Assert.False(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        private static ImageDecodeOptions Options(int length, int width = 4, int height = 3, long pixels = 12,
            long cumulative = 12, int depth = 16, int channels = 5, long storage = 120)
        {
            return new ImageDecodeOptions(length, width, height, pixels, 1, true, true, long.MaxValue, long.MaxValue, false, false,
                cumulative, depth, channels, true, storage);
        }

        private static IEnumerable<ImageDecodeOptions> AllOptions(ImageDecodeOptions exact, ImageDecodeOptions[] limits)
        {
            yield return exact; foreach (ImageDecodeOptions options in limits) yield return options;
        }

        private static string Header(int width = 4, int height = 3, int depth = 3, int maxval = 255)
        {
            return "P7\nWIDTH " + width + "\nHEIGHT " + height + "\nDEPTH " + depth + "\nMAXVAL " + maxval + "\nENDHDR\n";
        }

        private static byte[] Pam(int width = 4, int height = 3, int depth = 3, int maxval = 255, string? tuple = null, string? secondTuple = null)
        {
            string text = Header(width, height, depth, maxval);
            if (tuple != null) text = text.Replace("ENDHDR\n", "TUPLTYPE " + tuple + "\nENDHDR\n");
            if (secondTuple != null) text = text.Replace("ENDHDR\n", "TUPLTYPE " + secondTuple + "\nENDHDR\n");
            return Bytes(text, new byte[width * height * depth * (maxval <= 255 ? 1 : 2)]);
        }

        private static byte[] Bytes(string text, byte[]? payload = null) { return Join(Encoding.ASCII.GetBytes(text), payload ?? new byte[0]); }
        private static byte[] Join(byte[] first, byte[] second)
        {
            byte[] result = new byte[first.Length + second.Length]; Array.Copy(first, result, first.Length);
            Array.Copy(second, 0, result, first.Length, second.Length); return result;
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
    }
}
