using System;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using Xunit;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class JpegMarkerPreflightTests
    {
        [Fact]
        public void IdentifyDoesNotReadPastTruncatedJpegComponentTable()
        {
            byte[] jpeg = { 0xFF, 0xD8, 0xFF, 0xC0, 0, 7, 8, 0, 3, 0, 2 };
            ImageIdentifyResult result = ImgCodecsCv2.Identify(jpeg);
            Assert.Equal("jpeg", result.Format);
            Assert.False(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown);
            Assert.False(result.IsFrameCountKnown);
            AssertUnknownMetadata(result);
        }

        [Fact]
        public void IdentifyDoesNotTreatJpegMetadataPayloadAsEndOfImage()
        {
            byte[] jpeg = Join(Soi, Sof(), Sos(), new byte[] { 0x12 }, Segment(0xE1, Eoi));
            ImageIdentifyResult result = ImgCodecsCv2.Identify(jpeg);
            Assert.True(result.IsSizeKnown);
            Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsCumulativePixelCountKnown);
            AssertUnknownMetadata(result);
        }

        [Fact]
        public void IdentifyAcceptsOutOfOrderJpegIccFragments()
        {
            byte[] jpeg = Join(Soi, Icc(2, 2, 0x22), Icc(1, 2, 0x11), Eoi);
            ImageIdentifyResult result = ImgCodecsCv2.Identify(jpeg);
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(30, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(2, result.IccProfileBytes);
        }

        [Fact]
        public void IdentifyRequiresExactJpegEndForMetadataFacts()
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Soi, Sof(), Icc(1, 1, 1), Eoi, new byte[] { 0 }));
            Assert.True(result.IsSizeKnown);
            Assert.False(result.IsFrameCountKnown);
            AssertUnknownMetadata(result);
        }

        [Fact]
        public void IdentifyWalksJpegScansAndSkipsEverySegmentPayload()
        {
            byte[] jpeg = Join(Soi, new byte[] { 0xFF, 0x01, 0xFF }, Sof(0xC2),
                Segment(0xE0, 1, 2), Segment(0xDB, Eoi), Sos(),
                new byte[] { 7, 0xFF, 0, 0xD9, 0xFF, 0xD0, 9, 0xFF, 1, 11 },
                Segment(0xE1, 0xFF, 0xD9, 0xFF, 0), Segment(0xFE, 3, 4), Icc(2, 2, 0x22),
                Sos(), new byte[] { 0xFF, 0xD7, 8 }, Icc(1, 2, 0x11), new byte[] { 0xFF }, Eoi);
            ImageIdentifyResult result = ImgCodecsCv2.Identify(jpeg);
            AssertHeader(result, 8, 1);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(1, result.FrameCount);
            Assert.Equal(6, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown);
            Assert.Equal(6, result.EstimatedPixelBytes);
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(38, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(2, result.IccProfileBytes);
        }

        [Theory]
        [InlineData(0xC0, 8)]
        [InlineData(0xC1, 12)]
        [InlineData(0xC2, 8)]
        [InlineData(0xC2, 12)]
        [InlineData(0xC3, 2)]
        [InlineData(0xC3, 16)]
        [InlineData(0xC9, 12)]
        [InlineData(0xCA, 12)]
        [InlineData(0xCB, 16)]
        public void IdentifyReadsCompleteNonHierarchicalJpegComponentTables(byte marker, byte precision)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Soi, Sof(marker, precision, 3), Eoi));
            AssertHeader(result, precision, 3);
            Assert.True(result.IsFrameCountKnown);
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(0, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(0, result.IccProfileBytes);
        }

        [Fact]
        public void IdentifyResolvesJpegDnlHeightOnlyInsideEntropyData()
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(
                Soi, Sof(height: 0), Sos(), new byte[] { 0x12, 0x34 }, Dnl(3), new byte[] { 0x56 }, Eoi));

            AssertHeader(result, 8, 1);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(1, result.FrameCount);
            Assert.Equal(6, result.CumulativePixelCount);
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(0, result.MetadataBytes);
        }

        [Fact]
        public void IdentifyRejectsJpegDnlOutsideEntropyOrWithAmbiguousHeight()
        {
            ImageIdentifyResult beforeScan = ImgCodecsCv2.Identify(Join(Soi, Sof(height: 0), Dnl(3), Eoi));
            Assert.False(beforeScan.IsSizeKnown);
            Assert.False(beforeScan.IsPixelFormatKnown);
            Assert.False(beforeScan.IsFrameCountKnown);

            foreach (byte[] invalid in new[]
            {
                Dnl(0), Segment(0xDC, 0, 3, 0), Segment(0xDC, 0, 0, 0, 3),
                Join(Dnl(3), Dnl(3))
            })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(
                    Join(Soi, Sof(height: 0), Sos(), new byte[] { 0x12 }, invalid, Eoi));
                Assert.False(result.IsFrameCountKnown);
                AssertUnknownMetadata(result);
            }

            ImageIdentifyResult knownHeight = ImgCodecsCv2.Identify(
                Join(Soi, Sof(), Sos(), new byte[] { 0x12 }, Dnl(3), Eoi));
            Assert.True(knownHeight.IsSizeKnown);
            Assert.False(knownHeight.IsFrameCountKnown);
            AssertUnknownMetadata(knownHeight);
        }

        [Fact]
        public void IdentifyKeepsInvalidJpegFrameHeadersUnknown()
        {
            byte[] frame = { 8, 0, 3, 0, 2, 1, 1, 0x11, 0 };
            for (int length = 0; length < frame.Length; ++length)
            {
                byte[] payload = new byte[length];
                Array.Copy(frame, payload, length);
                AssertUnknownHeader(ImgCodecsCv2.Identify(Join(Soi, Segment(0xC0, payload), Eoi)));
            }
            foreach (int field in new[] { 0, 2, 4, 5, 7, 8 })
            {
                byte[] payload = (byte[])frame.Clone();
                payload[field] = field == 8 ? (byte)4 : (byte)0;
                AssertUnknownHeader(ImgCodecsCv2.Identify(Join(Soi, Segment(0xC0, payload), Eoi)));
            }
            byte[] duplicateComponents = { 8, 0, 3, 0, 2, 2, 1, 0x11, 0, 1, 0x11, 0 };
            foreach (byte[] sof in new[]
            {
                Segment(0xC0, Join(frame, new byte[] { 0 })), Segment(0xC0, duplicateComponents),
                Sof(0xC0, 12), Sof(0xC1, 16), Sof(0xC2, 0), Sof(0xC3, 1), Sof(0xCB, 17)
            }) AssertUnknownHeader(ImgCodecsCv2.Identify(Join(Soi, sof, Eoi)));
        }

        [Fact]
        public void IdentifyDoesNotPublishOneSizeForMultipleOrHierarchicalJpegFrames()
        {
            foreach (byte[] frames in new[]
            {
                Join(Sof(), Sof()), Join(Sof(), Sof(0xC2)), Join(Sof(), Segment(0xDE)),
                Join(Sof(), Segment(0xDF)), Sof(0xC5), Sof(0xC6), Sof(0xC7), Sof(0xCD), Sof(0xCE), Sof(0xCF)
            }) AssertUnknownHeader(ImgCodecsCv2.Identify(Join(Soi, frames, Eoi)));
        }

        [Fact]
        public void IdentifyRequiresJpegMarkerIntroducersAndBoundedSegmentLengths()
        {
            foreach (byte[] broken in new[]
            {
                new byte[] { 0xE1, 0, 2 }, new byte[] { 0xFF, 0 }, new byte[] { 0xFF, 0xD0 }, Soi,
                new byte[] { 0xFF, 0xE1 }, new byte[] { 0xFF, 0xE1, 0 },
                new byte[] { 0xFF, 0xE1, 0, 0 }, new byte[] { 0xFF, 0xE1, 0, 1 },
                new byte[] { 0xFF, 0xE1, 0xFF, 0xFF, 0xFF, 0xD9 }, new byte[] { 0xFF, 0x02, 0, 2 },
                Segment(0xDC, 0, 3)
            })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Soi, Sof(), broken, Eoi));
                AssertHeader(result, 8, 1);
                Assert.False(result.IsFrameCountKnown);
                AssertUnknownMetadata(result);
            }
            AssertUnknownHeader(ImgCodecsCv2.Identify(Join(Soi, new byte[] { 0xC0 }, Sof(), Eoi)));
        }

        [Fact]
        public void IdentifyRequiresCompleteJpegScanComponentTables()
        {
            foreach (byte[] scan in new[]
            {
                Segment(0xDA), Segment(0xDA, 1, 1, 0, 0), Segment(0xDA, 0, 0, 63, 0),
                Segment(0xDA, 2, 1, 0, 0, 63, 0), Segment(0xDA, 1, 2, 0, 0, 63, 0),
                Segment(0xDA, 2, 1, 0, 1, 0, 0, 63, 0), Segment(0xDA, 1, 1, 0x40, 0, 63, 0),
                Segment(0xDA, 1, 1, 4, 0, 63, 0), Segment(0xDA, 5, 1, 0, 2, 0, 3, 0, 4, 0, 5, 0, 0, 63, 0)
            })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Soi, Sof(), scan, Eoi));
                AssertHeader(result, 8, 1);
                Assert.False(result.IsFrameCountKnown);
                AssertUnknownMetadata(result);
            }
            AssertUnknownHeader(ImgCodecsCv2.Identify(Join(Soi, Sos(), Eoi)));
            ImageIdentifyResult truncated = ImgCodecsCv2.Identify(Join(Soi, Sof(), Sos(), new byte[] { 0xFF }));
            AssertHeader(truncated, 8, 1);
            Assert.False(truncated.IsFrameCountKnown);
            AssertUnknownMetadata(truncated);
        }

        [Theory]
        [InlineData(0xC9, 0, 63)]
        [InlineData(0xCA, 0, 0)]
        [InlineData(0xCB, 1, 0)]
        public void IdentifyAcceptsJpegArithmeticConditioningTableSelectors(byte marker, byte spectralStart, byte spectralEnd)
        {
            byte[] scan = Segment(0xDA, 1, 1, 0xFF, spectralStart, spectralEnd, 0);
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Soi, Sof(marker), scan, new byte[] { 42 }, Eoi));
            AssertHeader(result, 8, 1);
            Assert.True(result.IsFrameCountKnown);
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(0, result.MetadataBytes);
        }

        [Fact]
        public void IdentifyTracksAll255JpegIccFragmentsInAnyPhysicalOrder()
        {
            using (var fragments = new MemoryStream())
            {
                for (int sequence = 255; sequence >= 1; --sequence)
                {
                    byte[] fragment = Icc((byte)sequence, 255, (byte)sequence);
                    fragments.Write(fragment, 0, fragment.Length);
                }
                ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Soi, Sof(), fragments.ToArray(), Eoi));
                Assert.True(result.IsFrameCountKnown);
                Assert.True(result.IsMetadataSizeKnown);
                Assert.Equal(255 * 15, result.MetadataBytes);
                Assert.True(result.IsIccProfileSizeKnown);
                Assert.Equal(255, result.IccProfileBytes);
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(65519)]
        public void IdentifyCountsJpegIccBytesUpToTheMaximumSegmentLength(int payloadLength)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Soi, Icc(1, 1, new byte[payloadLength]), Eoi));
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(14L + payloadLength, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(payloadLength, result.IccProfileBytes);
        }

        [Fact]
        public void IdentifyKeepsInvalidJpegIccSetsUnknownWithoutLosingTheFrameBoundary()
        {
            byte[] signature = Encoding.ASCII.GetBytes("ICC_PROFILE\0");
            foreach (byte[] fragments in new[]
            {
                Join(Icc(1, 1, 1), Icc(1, 1, 2)), Icc(1, 2, 1), Icc(2, 2, 1),
                Join(Icc(2, 2, 1), Icc(1, 3, 2)), Icc(0, 1, 1), Icc(1, 0, 1), Icc(3, 2, 1),
                Segment(0xE2, signature), Segment(0xE2, Join(signature, new byte[] { 1 }))
            })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Soi, Sof(), fragments, Eoi));
                AssertHeader(result, 8, 1);
                Assert.True(result.IsFrameCountKnown);
                AssertUnknownMetadata(result);
            }
        }

        [Fact]
        public void IdentifyMatchesTheExactJpegIccSignatureAndCountsEmptyFragments()
        {
            ImageIdentifyResult empty = ImgCodecsCv2.Identify(Join(Soi, Icc(1, 1), Eoi));
            Assert.True(empty.IsMetadataSizeKnown);
            Assert.Equal(14, empty.MetadataBytes);
            Assert.True(empty.IsIccProfileSizeKnown);
            Assert.Equal(0, empty.IccProfileBytes);
            Assert.False(empty.IsFrameCountKnown);
            byte[] signature = Encoding.ASCII.GetBytes("ICC_PROFILE\0");
            for (int index = 0; index < signature.Length; ++index)
            {
                byte[] name = (byte[])signature.Clone();
                name[index] ^= 1;
                ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(Soi,
                    Segment(0xE2, Join(name, new byte[] { 1, 1, 42 })), Eoi));
                Assert.True(result.IsMetadataSizeKnown);
                Assert.Equal(15, result.MetadataBytes);
                Assert.True(result.IsIccProfileSizeKnown);
                Assert.Equal(0, result.IccProfileBytes);
            }
        }

        [Fact]
        public void JpegMetadataBudgetsRejectBeforeNativeDecodeAndAcceptExactStoredLengths()
        {
            byte[] jpeg = Join(Soi, Sof(channels: 3), Segment(0xE1, 1, 2, 3), Icc(2, 2, 2), Icc(1, 2, 1), Eoi);
            ImageDecodeOptions exact = Options(jpeg.Length, 33, 2);
            ImageDecodeOptions metadataLimit = Options(jpeg.Length, 32, 2);
            ImageDecodeOptions iccLimit = Options(jpeg.Length, 33, 1);
            Assert.Contains("metadata exceeds", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(jpeg, metadataLimit)).Message);
            Assert.Contains("ICC profile exceeds", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(jpeg, iccLimit)).Message);
            Assert.Contains("metadata exceeds", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(jpeg), metadataLimit)).Message);
            Assert.Contains("ICC profile exceeds", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(jpeg), iccLimit)).Message);
            foreach (ImageDecodeOptions options in new[] { exact, metadataLimit, iccLimit, Options(jpeg.Length, 33, 2, 17) })
            {
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, jpeg)))
                {
                    stream.Position = 1;
                    if (options == exact)
                    {
                        ImageIdentifyResult result = ImgCodecsCv2.Identify(stream, options);
                        Assert.Equal(33, result.MetadataBytes);
                        Assert.Equal(2, result.IccProfileBytes);
                        Assert.Equal(18, result.EstimatedPixelBytes);
                    }
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadNonSeekableStream(jpeg))
                {
                    if (options == exact) ImgCodecsCv2.Identify(stream, options);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(jpeg.Length, stream.BytesRead);
                }
            }
        }

        [Fact]
        public void JpegStrictMetadataPolicyRejectsUnknownLengthsAndRestoresSeekPosition()
        {
            byte[] jpeg = Join(Soi, Sof(), Icc(1, 2, 1), Eoi);
            AssertUnknownMetadata(ImgCodecsCv2.Identify(jpeg));
            Assert.Contains("metadata size", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(jpeg, Options(jpeg.Length, 15, 1))).Message);
            var strictIcc = new ImageDecodeOptions(jpeg.Length, 2, 3, 6, 1, true, true, 15, 1, false, true);
            Assert.Contains("ICC profile size", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(jpeg), strictIcc)).Message);
            using (var stream = new MemoryStream(Join(new byte[] { 42 }, jpeg)))
            {
                stream.Position = 1;
                AssertUnknownMetadata(ImgCodecsCv2.Identify(stream));
                Assert.Equal(1, stream.Position);
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, strictIcc));
                Assert.Equal(1, stream.Position);
            }
        }

        [Fact]
        public void IdentifyKeepsJpegCompletionUnknownForEveryStrictPrefixAndTrailingInput()
        {
            foreach (byte[] jpeg in new[] { Join(Soi, Sof(), Sos(), new byte[] { 42 }, Icc(1, 1, 1), Eoi), RealBaseline, RealProgressive })
            {
                for (int length = 1; length < jpeg.Length; ++length)
                {
                    byte[] prefix = new byte[length];
                    Array.Copy(jpeg, prefix, length);
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                    Assert.False(result.IsFrameCountKnown);
                    AssertUnknownMetadata(result);
                }
                foreach (byte[] trailing in new[] { new byte[] { 0 }, Eoi, jpeg })
                {
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(Join(jpeg, trailing));
                    AssertHeader(result, 8, jpeg == RealBaseline || jpeg == RealProgressive ? 3 : 1);
                    Assert.False(result.IsFrameCountKnown);
                    AssertUnknownMetadata(result);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyReadsRealBaselineAndProgressiveJpegWithReorderedIcc(bool progressive)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(progressive ? RealProgressive : RealBaseline);
            AssertHeader(result, 8, 3);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(1, result.FrameCount);
            Assert.Equal(6, result.CumulativePixelCount);
            Assert.Equal(18, result.EstimatedPixelBytes);
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(progressive ? 14 : 630, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(progressive ? 0 : 588, result.IccProfileBytes);
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusJpegMarkersNeverEscapeOrPublishUnboundedFacts()
        {
            foreach (byte[] jpeg in new[] { Join(Soi, Sof(), Sos(), new byte[] { 42 }, Icc(2, 2, 2), Icc(1, 2, 1), Eoi), RealBaseline, RealProgressive })
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            for (int offset = 0; offset < jpeg.Length; ++offset)
            {
                if (jpeg[offset] == replacement) continue;
                byte[] mutated = (byte[])jpeg.Clone();
                mutated[offset] = replacement;
                ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                if (result.Format != "jpeg") continue;
                Assert.Equal(result.IsSizeKnown, result.IsPixelFormatKnown);
                if (result.IsSizeKnown)
                {
                    Assert.InRange(result.Width, 1, 65535);
                    Assert.InRange(result.Height, 1, 65535);
                    Assert.InRange(result.BitDepth, 2, 16);
                    Assert.InRange(result.ChannelCount, 1, 255);
                }
                Assert.Equal(result.IsMetadataSizeKnown, result.IsIccProfileSizeKnown);
                if (result.IsMetadataSizeKnown)
                {
                    Assert.Equal(0xFF, mutated[mutated.Length - 2]);
                    Assert.Equal(0xD9, mutated[mutated.Length - 1]);
                    Assert.InRange(result.MetadataBytes, 0, mutated.LongLength);
                    Assert.InRange(result.IccProfileBytes, 0, result.MetadataBytes);
                }
                else AssertUnknownMetadata(result);
                if (result.IsFrameCountKnown)
                {
                    Assert.True(result.IsSizeKnown);
                    Assert.Equal(1, result.FrameCount);
                    Assert.Equal(0xFF, mutated[mutated.Length - 2]);
                    Assert.Equal(0xD9, mutated[mutated.Length - 1]);
                    Assert.True(result.IsCumulativePixelCountKnown);
                    Assert.Equal((long)result.Width * result.Height, result.CumulativePixelCount);
                    Assert.True(result.IsEstimatedPixelBytesKnown);
                    Assert.Equal(result.CumulativePixelCount * ((result.BitDepth + 7) / 8) * result.ChannelCount, result.EstimatedPixelBytes);
                }
            }
        }

        private static readonly byte[] Soi = { 0xFF, 0xD8 };
        private static readonly byte[] Eoi = { 0xFF, 0xD9 };

        private static ImageDecodeOptions Options(int inputLength, long metadata, long icc, long estimatedBytes = 18)
        {
            return new ImageDecodeOptions(inputLength, 2, 3, 6, 1, true, true, metadata, icc, true, true, 6, 8, 3, true, estimatedBytes);
        }

        private static void AssertHeader(ImageIdentifyResult result, int precision, int channels)
        {
            Assert.Equal("jpeg", result.Format);
            Assert.True(result.IsSizeKnown);
            Assert.Equal(2, result.Width);
            Assert.Equal(3, result.Height);
            Assert.True(result.IsPixelFormatKnown);
            Assert.Equal(precision, result.BitDepth);
            Assert.Equal(channels, result.ChannelCount);
        }

        private static void AssertUnknownHeader(ImageIdentifyResult result)
        {
            Assert.Equal("jpeg", result.Format);
            Assert.False(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown);
            Assert.False(result.IsFrameCountKnown);
            AssertUnknownMetadata(result);
        }

        private static void AssertUnknownMetadata(ImageIdentifyResult result)
        {
            Assert.False(result.IsMetadataSizeKnown);
            Assert.False(result.IsIccProfileSizeKnown);
            Assert.Equal(0, result.MetadataBytes);
            Assert.Equal(0, result.IccProfileBytes);
        }

        private static byte[] Sof(byte marker = 0xC0, byte precision = 8, int channels = 1, int height = 3)
        {
            byte[] payload = new byte[6 + 3 * channels];
            payload[0] = precision;
            payload[1] = (byte)(height >> 8);
            payload[2] = (byte)height;
            payload[4] = 2;
            payload[5] = (byte)channels;
            for (int index = 0; index < channels; ++index)
            {
                payload[6 + 3 * index] = (byte)(index + 1);
                payload[7 + 3 * index] = 0x11;
            }
            return Segment(marker, payload);
        }

        private static byte[] Dnl(int height)
        {
            return Segment(0xDC, (byte)(height >> 8), (byte)height);
        }

        private static byte[] Sos()
        {
            return Segment(0xDA, new byte[] { 1, 1, 0, 0, 63, 0 });
        }

        private static byte[] Icc(byte sequence, byte count, params byte[] payload)
        {
            return Segment(0xE2, Join(Encoding.ASCII.GetBytes("ICC_PROFILE\0"), new byte[] { sequence, count }, payload));
        }

        private static byte[] Segment(byte marker, params byte[] payload)
        {
            int length = payload.Length + 2;
            return Join(new byte[] { 0xFF, marker, (byte)(length >> 8), (byte)length }, payload);
        }

        private static byte[] Join(params byte[][] parts)
        {
            using (var stream = new MemoryStream())
            {
                foreach (byte[] part in parts) stream.Write(part, 0, part.Length);
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

        // Pillow 13.0.0, RGB 2x3 (24,80,160), quality=95; both fixtures independently decoded.
        // Baseline: 588-byte sRGB ICC split into two reversed APP2 fragments and independently reassembled.
        // SHA256 9c1bf8abb2f7ca071a16fcd79c341bd274b974ff77b2e2e967b007ecb699ce60 (1258 bytes).
        private static readonly byte[] RealBaseline = Convert.FromBase64String(
            "/9j/4AAQSkZJRgABAQAAAQABAAD/4gE2SUNDX1BST0ZJTEUAAgIAUgBHAEIAIABiAHUAaQBsAHQALQBpAG4AAG1sdWMAAAAAAAAAAQAAAAxlblVTAAAAMAAAABwATgBvACAAYwBvAHAAeQByAGkAZwBoAHQALAAgAHUAcwBlACAAZgByAGUAZQBsAHlYWVogAAAAAAAA9tYAAQAAAADTLXNmMzIAAAAAAAEMQgAABd7///MlAAAHkwAA/ZD///uh///9ogAAA9wAAMBuWFlaIAAAAAAAAG+gAAA49QAAA5BYWVogAAAAAAAAJJ8AAA+EAAC2w1hZWiAAAAAAAABilwAAt4cAABjZcGFyYQAAAAAAAwAAAAJmZgAA8qcAAA1ZAAAT0AAACltjaHJtAAAAAAADAAAAAKPXAABUewAATM0AAJmaAAAmZgAAD1z/4gE2SUNDX1BST0ZJTEUAAQIAAAJMbGNtcwRAAABtbnRyUkdCIFhZWiAH6gALABQADQA3ACBhY3NwTVNGVAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA9tYAAQAAAADTLWxjbXMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAtkZXNjAAABCAAAADZjcHJ0AAABQAAAAEx3dHB0AAABjAAAABRjaGFkAAABoAAAACxyWFlaAAABzAAAABRiWFlaAAAB4AAAABRnWFlaAAAB9AAAABRyVFJDAAACCAAAACBnVFJDAAACCAAAACBiVFJDAAACCAAAACBjaHJtAAACKAAAACRtbHVjAAAAAAAAAAEAAAAMZW5VUwAAABoAAAAcAHP/2wBDAAIBAQEBAQIBAQECAgICAgQDAgICAgUEBAMEBgUGBgYFBgYGBwkIBgcJBwYGCAsICQoKCgoKBggLDAsKDAkKCgr/2wBDAQICAgICAgUDAwUKBwYHCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgr/wAARCAADAAIDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKU1RVVldYWVpjZGVmZ2hpanN0dXZ3eHl6goOEhYaHiImKkpOUlZaXmJmaoqOkpaanqKmqsrO0tba3uLm6wsPExcbHyMnK0tPU1dbX2Nna4uPk5ebn6Onq8vP09fb3+Pn6/9oADAMBAAIRAxEAPwD4fooor+xD+cz/2Q==");

        // Progressive SHA256 cd175642b4d8b339e72aee548bc6837fd8eeee20480f9fc32f55dc9690c8abc0 (521 bytes).
        private static readonly byte[] RealProgressive = Convert.FromBase64String(
            "/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAIBAQEBAQIBAQECAgICAgQDAgICAgUEBAMEBgUGBgYFBgYGBwkIBgcJBwYGCAsICQoKCgoKBggLDAsKDAkKCgr/2wBDAQICAgICAgUDAwUKBwYHCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgr/wgARCAADAAIDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAf/xAAUAQEAAAAAAAAAAAAAAAAAAAAH/9oADAMBAAIQAxAAAAGHhiOf/8QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAgBAQABBQJ//8QAFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAgBAwEBPwF//8QAFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAgBAgEBPwF//8QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAgBAQAGPwJ//8QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAgBAQABPyF//9oADAMBAAIAAwAAABAD/8QAFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAgBAwEBPxB//8QAFBEBAAAAAAAAAAAAAAAAAAAAAP/aAAgBAgEBPxB//8QAFBABAAAAAAAAAAAAAAAAAAAAAP/aAAgBAQABPxB//9k=");
    }
}
