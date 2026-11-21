using System;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class WebpContainerPreflightTests
    {
        [Fact]
        public void IdentifyReadsTheActualSimpleLossyWebpDimensionOffsets()
        {
            AssertImage(ImgCodecsCv2.Identify(RealLossy), 4, 3, 1, 3);
        }

        [Fact]
        public void IdentifyReadsSimpleLosslessWebpDimensions()
        {
            AssertImage(ImgCodecsCv2.Identify(RealLossless), 4, 3, 1, 3);
        }

        [Fact]
        public void IdentifyNeverCountsHeaderOnlyWebpAnimationFrames()
        {
            AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8x(), Anim(), Chunk("ANMF", FrameHeader()))));
        }

        [Fact]
        public void IdentifyNeverCountsWebpFramesOutsideTheCanvas()
        {
            AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8x(), Anim(), Frame(2, 2, 3, 1))));
        }

        [Fact]
        public void IdentifyRequiresWebpFrameAndBitstreamDimensionsToAgree()
        {
            AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8x(), Anim(),
                Chunk("ANMF", Join(FrameHeader(), Vp8l(1, 1))))));
        }

        [Fact]
        public void IdentifyNeverMixesStaticAndAnimatedWebpImageData()
        {
            AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8x(), Vp8l(6, 4), Anim(), Frame())));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IdentifyAcceptsStaticWebpHeadersAndLossyAlpha(bool lossless, bool alpha)
        {
            byte[] image = lossless ? Vp8l(alpha: alpha) : Vp8();
            byte[] alph = alpha && !lossless ? Chunk("ALPH", new byte[25]) : new byte[0];
            AssertImage(ImgCodecsCv2.Identify(Riff(Vp8x(flags: alpha ? (byte)0x10 : (byte)0), alph, image)), 6, 4, 1, alpha ? 4 : 3);
            if (lossless || !alpha) AssertImage(ImgCodecsCv2.Identify(Riff(image)), 6, 4, 1, alpha ? 4 : 3);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(255)]
        public void IdentifyAcceptsWebpBlendDisposalAndIgnoresReservedFrameBits(byte flags)
        {
            AssertImage(ImgCodecsCv2.Identify(Riff(Vp8x(), Anim(), Frame(flags: flags), Frame(2, 2, 2, 1, flags))), 6, 4, 2, 3);
        }

        [Fact]
        public void IdentifyIgnoresWebpReservedVp8xBitsAndFutureHeaderFields()
        {
            byte[] header = Chunk("VP8X", Join(new byte[] { 0xC1, 255, 255, 255 }, U24(5), U24(3), new byte[] { 1, 2, 3 }));
            AssertImage(ImgCodecsCv2.Identify(Riff(header, Vp8l())), 6, 4, 1, 3);
            // Reserved ALPH bits are ignored; all defined filters and preprocessing values work.
            foreach (byte flags in new byte[] { 0, 4, 8, 12, 16, 20, 24, 28, 192, 220 })
            {
                byte[] raw = new byte[25]; raw[0] = flags;
                AssertImage(ImgCodecsCv2.Identify(Riff(Vp8x(flags: 0x10), Chunk("ALPH", raw), Vp8())), 6, 4, 1, 4);
            }
            AssertImage(ImgCodecsCv2.Identify(Riff(Vp8x(flags: 0x10), Chunk("ALPH", 1, 0), Vp8())), 6, 4, 1, 4);
            AssertImage(ImgCodecsCv2.Identify(Riff(Vp8x(flags: 0x10), Vp8l())), 6, 4, 1, 4);
        }

        [Fact]
        public void IdentifyAcceptsWebpUnknownChunksAndOutOfOrderMetadata()
        {
            byte[] frame = Chunk("ANMF", Join(FrameHeader(), Vp8l(), Chunk("????", 1, 2, 3)));
            byte[] webp = Riff(Vp8x(flags: 0x2E), Chunk("EXIF", 1, 2), Chunk("ICCP", 3, 4, 5),
                Chunk("????", 7), Anim(), frame, Chunk("XMP ", 6), frame, Chunk("????"));
            ImageIdentifyResult result = ImgCodecsCv2.Identify(webp);
            AssertImage(result, 6, 4, 2, 3);
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(6, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(3, result.IccProfileBytes);
            AssertImage(ImgCodecsCv2.Identify(Riff(Vp8l(), Chunk("????", 1))), 6, 4, 1, 3);
        }

        [Theory]
        [InlineData(2, 2, 3, 0)]
        [InlineData(2, 2, 0, 2)]
        [InlineData(7, 1, 0, 0)]
        [InlineData(1, 5, 0, 0)]
        [InlineData(1, 1, 16777215, 0)]
        [InlineData(1, 1, 0, 16777215)]
        [InlineData(16777216, 16777216, 16777215, 16777215)]
        public void IdentifyRejectsWebpFrameRectanglesUsingDoubledOffsets(int width, int height, int x, int y)
        {
            AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8x(), Anim(), Frame(width, height, x, y))));
        }

        [Fact]
        public void IdentifyBoundsWebpCanvasAreaAndUsesLongCumulativeArithmetic()
        {
            byte[] maximum = Riff(Vp8x(65535, 65537), Anim(), Frame(1, 1), Frame(1, 1));
            AssertImage(ImgCodecsCv2.Identify(maximum), 65535, 65537, 2, 3);
            var exact = new ImageDecodeOptions(maximum.Length, 65535, 65537, uint.MaxValue, 2, true, true,
                1, 1, true, true, 2L * uint.MaxValue, 8, 3, true, 6L * uint.MaxValue);
            using (var stream = new MemoryStream(maximum))
                AssertImage(ImgCodecsCv2.Identify(stream, exact), 65535, 65537, 2, 3);
            foreach (byte[] invalid in new[] { Riff(Vp8x(65536, 65536), Anim(), Frame(1, 1)),
                Riff(Vp8x(16777216, 16777216), Anim(), Frame(1, 1)) })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(invalid);
                AssertUnknownContainer(result);
                Assert.False(result.IsSizeKnown);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyReadsWebpBitstreamDimensionExtremes(bool lossless)
        {
            int maximum = lossless ? 16384 : 16383;
            AssertImage(ImgCodecsCv2.Identify(Riff(lossless ? Vp8l(maximum, maximum) : Vp8(maximum, maximum))), maximum, maximum, 1, 3);
            if (!lossless)
            {
                byte[] scaled = Vp8(1, 1);
                scaled[15] = 0xC0; scaled[17] = 0xC0;
                AssertImage(ImgCodecsCv2.Identify(Riff(scaled)), 1, 1, 1, 3);
            }
        }

        [Fact]
        public void IdentifyRequiresWebpImageDataAfterTheBitstreamHeader()
        {
            foreach (byte[] image in new[] {
                Chunk("VP8 ", 0x30, 0, 0, 0x9D, 1, 0x2A, 6, 0, 4, 0),
                Chunk("VP8L", 0x2F, 5, 192, 0, 0) })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(Riff(image));
                AssertUnknownContainer(result);
                Assert.True(result.IsSizeKnown);
                Assert.Equal(6, result.Width);
                Assert.Equal(4, result.Height);
                AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8x(), Anim(), Chunk("ANMF", Join(FrameHeader(), image)))));
            }
            byte[] invalidPartition = Vp8(); invalidPartition[8] = 0x50;
            AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(invalidPartition)));
        }

        [Fact]
        public void IdentifyRejectsMalformedWebpChunkOrderingAndNestedImages()
        {
            var fixtures = new[]
            {
                new { Name = "vp8x-not-first", Bytes = Riff(Chunk("????"), Vp8x(), Anim(), Frame()) },
                new { Name = "duplicate-vp8x", Bytes = Riff(Vp8x(), Vp8x(), Anim(), Frame()) },
                new { Name = "short-vp8x", Bytes = Riff(Chunk("VP8X", new byte[9]), Vp8l()) },
                new { Name = "missing-vp8x", Bytes = Riff(Anim(), Frame()) },
                new { Name = "animation-flag-clear", Bytes = Riff(Vp8x(flags: 0), Anim(), Frame()) },
                new { Name = "duplicate-anim", Bytes = Riff(Vp8x(), Anim(), Anim(), Frame()) },
                new { Name = "short-anim", Bytes = Riff(Vp8x(), Chunk("ANIM", new byte[5]), Frame()) },
                new { Name = "long-anim", Bytes = Riff(Vp8x(), Chunk("ANIM", new byte[7]), Frame()) },
                new { Name = "frame-before-anim", Bytes = Riff(Vp8x(), Frame(), Anim()) },
                new { Name = "no-frame", Bytes = Riff(Vp8x(), Anim()) },
                new { Name = "short-anmf", Bytes = Riff(Vp8x(), Anim(), Chunk("ANMF", new byte[15])) },
                new { Name = "late-static-image", Bytes = Riff(Vp8x(), Anim(), Frame(), Vp8l()) },
                new { Name = "alpha-outside-frame", Bytes = Riff(Vp8x(flags: 0x12), Anim(), Chunk("ALPH", 1, 0), Frame()) },
                new { Name = "duplicate-static-image", Bytes = Riff(Vp8l(), Vp8l()) },
                new { Name = "mixed-static-bitstreams", Bytes = Riff(Vp8x(flags: 0), Vp8(), Vp8l()) },
                new { Name = "static-canvas-mismatch", Bytes = Riff(Vp8x(flags: 0), Vp8l(1, 1)) },
                new { Name = "undeclared-alpha", Bytes = Riff(Vp8x(flags: 0), Vp8l(alpha: true)) },
                new { Name = "undeclared-frame-alpha", Bytes = Riff(Vp8x(), Anim(), Chunk("ANMF", Join(FrameHeader(), Vp8l(alpha: true)))) },
                new { Name = "alpha-without-vp8x", Bytes = Riff(Chunk("ALPH", 1, 0), Vp8()) },
                new { Name = "alpha-without-flag", Bytes = Riff(Vp8x(flags: 0), Chunk("ALPH", 1, 0), Vp8()) },
                new { Name = "alpha-without-image", Bytes = Riff(Vp8x(flags: 0x10), Chunk("ALPH", 1, 0)) },
                new { Name = "duplicate-alpha", Bytes = Riff(Vp8x(flags: 0x10), Chunk("ALPH", 1, 0), Chunk("ALPH", 1, 0), Vp8()) },
                new { Name = "alpha-after-image", Bytes = Riff(Vp8x(flags: 0x10), Vp8(), Chunk("ALPH", 1, 0)) },
                new { Name = "alpha-plus-lossless", Bytes = Riff(Vp8x(flags: 0x10), Chunk("ALPH", 1, 0), Vp8l(alpha: true)) },
                new { Name = "icc-after-image", Bytes = Riff(Vp8x(flags: 0x20), Vp8l(), Chunk("ICCP", 1)) },
                new { Name = "icc-after-anim", Bytes = Riff(Vp8x(flags: 0x22), Anim(), Chunk("ICCP", 1), Frame()) },
                new { Name = "duplicate-icc", Bytes = Riff(Vp8x(flags: 0x20), Chunk("ICCP", 1), Chunk("ICCP", 2), Vp8l()) },
                new { Name = "undeclared-icc", Bytes = Riff(Vp8x(flags: 0), Chunk("ICCP", 1), Vp8l()) },
                new { Name = "nested-two-images", Bytes = BadFrame(Vp8l(), Vp8l()) },
                new { Name = "nested-mixed-images", Bytes = BadFrame(Vp8(), Vp8l()) },
                new { Name = "nested-alpha-only", Bytes = BadFrame(Chunk("ALPH", 1, 0)) },
                new { Name = "nested-duplicate-alpha", Bytes = BadFrame(Chunk("ALPH", 1, 0), Chunk("ALPH", 1, 0), Vp8()) },
                new { Name = "nested-alpha-after-image", Bytes = BadFrame(Vp8(), Chunk("ALPH", 1, 0)) },
                new { Name = "nested-alpha-plus-lossless", Bytes = BadFrame(Chunk("ALPH", 1, 0), Vp8l()) },
                new { Name = "nested-unknown-before-image", Bytes = BadFrame(Chunk("????"), Vp8l()) },
                new { Name = "nested-anim", Bytes = BadFrame(Vp8l(), Anim()) },
                new { Name = "nested-anmf", Bytes = BadFrame(Vp8l(), Frame()) },
                new { Name = "nested-vp8x", Bytes = BadFrame(Vp8l(), Vp8x()) },
                new { Name = "nested-icc", Bytes = BadFrame(Vp8l(), Chunk("ICCP", 1)) },
                new { Name = "nested-exif", Bytes = BadFrame(Vp8l(), Chunk("EXIF", 1)) },
                new { Name = "nested-xmp", Bytes = BadFrame(Vp8l(), Chunk("XMP ", 1)) },
                new { Name = "nested-trailing-byte", Bytes = BadFrame(Vp8l(), new byte[] { 0 }) }
            };
            foreach (var fixture in fixtures)
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(fixture.Bytes);
                Assert.False(result.IsFrameCountKnown, fixture.Name);
                AssertUnknownContainer(result);
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
        public void IdentifyRejectsUnsupportedOrTruncatedWebpAlpha(int kind)
        {
            byte[][] invalid = { new byte[0], new byte[1], new byte[24], new byte[26], new byte[] { 2, 0 }, new byte[] { 3, 0 }, new byte[] { 0x21, 0 } };
            AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8x(flags: 0x10), Chunk("ALPH", invalid[kind]), Vp8())));
            AssertUnknownContainer(ImgCodecsCv2.Identify(BadFrame(Chunk("ALPH", invalid[kind]), Vp8())));
        }

        [Fact]
        public void IdentifyRejectsUnsupportedWebpImageHeaderFields()
        {
            foreach (int field in new[] { 8, 11, 12, 13, 14, 16 })
            {
                byte[] invalid = Vp8(); invalid[field] = 0;
                AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(invalid)));
            }
            foreach (byte tag in new byte[] { 0x31, 0x38, 0x3E })
            {
                byte[] invalid = Vp8(); invalid[8] = tag;
                AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(invalid)));
            }
            foreach (byte version in new byte[] { 0x20, 0x40, 0xE0 })
            {
                byte[] invalid = Vp8l(); invalid[12] = version;
                ImageIdentifyResult result = ImgCodecsCv2.Identify(Riff(invalid));
                AssertUnknownContainer(result);
                Assert.False(result.IsSizeKnown);
            }
        }

        [Fact]
        public void IdentifyRequiresWebpPaddingAndNestedLengthsToStayInsideTheirParent()
        {
            byte[] odd = Chunk("????", 1);
            byte[] nonzeroPadding = (byte[])odd.Clone(); nonzeroPadding[9] = 1;
            AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8l(), nonzeroPadding)));
            AssertUnknownContainer(ImgCodecsCv2.Identify(BadFrame(Vp8l(), nonzeroPadding)));
            AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8l(), Join(Encoding.ASCII.GetBytes("????"), U32(1), new byte[] { 1 }))));
            foreach (uint length in new[] { 0U, 1U, 7U, 0x7FFFFFFFU, uint.MaxValue })
            {
                byte[] invalid = Frame(); Array.Copy(U32(length), 0, invalid, 28, 4);
                AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8x(), Anim(), invalid)));
                byte[] outer = (byte[])invalid.Clone(); Array.Copy(U32(length), 0, outer, 4, 4);
                AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8x(), Anim(), outer)));
            }
            // A nested image must not borrow bytes from the next top-level frame.
            byte[] crossing = Frame(); Array.Copy(U32(7), 0, crossing, 28, 4);
            AssertUnknownContainer(ImgCodecsCv2.Identify(Riff(Vp8x(), Anim(), crossing, Frame())));
        }

        [Fact]
        public void IdentifyKeepsWebpHeaderAndStoredMetadataIndependentOfFrameCompleteness()
        {
            byte[] invalid = Riff(Vp8x(flags: 0x22), Chunk("ICCP", 1, 2, 3), Chunk("EXIF", 4, 5), Anim(), Chunk("ANMF", FrameHeader()));
            ImageIdentifyResult result = ImgCodecsCv2.Identify(invalid);
            AssertUnknownContainer(result);
            Assert.True(result.IsSizeKnown);
            Assert.Equal(6, result.Width);
            Assert.Equal(4, result.Height);
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(5, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(3, result.IccProfileBytes);
            Assert.Contains("depth or channel count", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(invalid,
                Options(invalid.Length, 6, 4, 2, 3))).Message);
        }

        [Fact]
        public void WebpCanvasBudgetsRejectBeforeNativeDecodeAndPreserveStreams()
        {
            foreach (byte[] webp in new[] { Animation(), RealLossy, RealLossless, RealLosslessAlpha,
                RealLossyAlpha, RealAnimationLossless, RealAnimationLossy })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(webp);
                ImageDecodeOptions exact = Options(webp.Length, result.Width, result.Height, result.FrameCount, result.ChannelCount);
                using (var stream = new MemoryStream(webp))
                    AssertImage(ImgCodecsCv2.Identify(stream, exact), result.Width, result.Height, result.FrameCount, result.ChannelCount);
                var limits = new[]
                {
                    new { Options = Options(webp.Length, result.Width, result.Height, result.FrameCount, result.ChannelCount,
                        result.CumulativePixelCount - 1), Error = "cumulative pixel count" },
                    new { Options = Options(webp.Length, result.Width, result.Height, result.FrameCount, result.ChannelCount,
                        storage: result.EstimatedPixelBytes - 1), Error = "pixel storage" },
                    new { Options = Options(webp.Length, result.Width, result.Height, result.FrameCount, result.ChannelCount - 1), Error = "channel count" }
                };
                foreach (var limit in limits)
                {
                    Assert.Contains(limit.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(webp, limit.Options)).Message);
                    Assert.Contains(limit.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(webp), limit.Options)).Message);
                    using (var stream = new MemoryStream(Join(new byte[] { 42 }, webp)))
                    {
                        stream.Position = 1;
                        Assert.Contains(limit.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, limit.Options)).Message);
                        Assert.Equal(1, stream.Position);
                    }
                    using (var stream = new ShortReadNonSeekableStream(webp))
                    {
                        Assert.Contains(limit.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, limit.Options)).Message);
                        Assert.Equal(webp.Length, stream.BytesRead);
                    }
                }
                if (result.FrameCount > 1)
                    Assert.Contains("frame count", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(webp,
                        Options(webp.Length, result.Width, result.Height, result.FrameCount - 1, result.ChannelCount,
                            result.CumulativePixelCount, result.EstimatedPixelBytes))).Message);
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, webp)))
                {
                    stream.Position = 1;
                    AssertImage(ImgCodecsCv2.Identify(stream, exact), result.Width, result.Height, result.FrameCount, result.ChannelCount);
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadNonSeekableStream(webp))
                {
                    AssertImage(ImgCodecsCv2.Identify(stream, exact), result.Width, result.Height, result.FrameCount, result.ChannelCount);
                    Assert.Equal(webp.Length, stream.BytesRead);
                }
            }
        }

        [Fact]
        public void WebpSimpleDimensionLimitsRejectBeforeNativeDecode()
        {
            foreach (byte[] webp in new[] { RealLossy, RealLossless, RealLosslessAlpha, RealLossyAlpha })
            {
                var options = Options(webp.Length, 3, 3, 1, 4);
                Assert.Contains("dimensions", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(webp, options)).Message);
                Assert.Contains("dimensions", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(webp), options)).Message);
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, webp)))
                {
                    stream.Position = 1;
                    Assert.Contains("dimensions", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options)).Message);
                    Assert.Equal(1, stream.Position);
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyReadsRealWebpAlphaAndCroppedAnimationFrames(bool lossless)
        {
            AssertImage(ImgCodecsCv2.Identify(lossless ? RealLosslessAlpha : RealLossyAlpha), 4, 3, 1, 4);
            AssertImage(ImgCodecsCv2.Identify(lossless ? RealAnimationLossless : RealAnimationLossy), 6, 4, 3, 4);
        }

        [Fact]
        public void IdentifyKeepsWebpCompletionUnknownForPrefixesAndTrailingContainers()
        {
            foreach (byte[] webp in Fixtures())
            {
                for (int length = 1; length < webp.Length; ++length)
                {
                    byte[] prefix = new byte[length]; Array.Copy(webp, prefix, length);
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                    Assert.False(result.IsFrameCountKnown);
                    Assert.False(result.IsEstimatedPixelBytesKnown);
                }
                foreach (byte[] extra in new[] { new byte[] { 0 }, webp, Chunk("????") })
                    AssertUnknownContainer(ImgCodecsCv2.Identify(Join(webp, extra)));
                foreach (uint length in new[] { 0U, 1U, 0x7FFFFFFFU, uint.MaxValue })
                {
                    byte[] invalid = (byte[])webp.Clone(); Array.Copy(U32(length), 0, invalid, 4, 4);
                    AssertUnknownContainer(ImgCodecsCv2.Identify(invalid));
                }
            }
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusWebpContainersNeverEscapeOrPublishUnboundedCanvasFacts()
        {
            foreach (byte[] webp in Fixtures())
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            for (int offset = 0; offset < webp.Length; ++offset)
            {
                if (webp[offset] == replacement) continue;
                byte[] mutated = (byte[])webp.Clone(); mutated[offset] = replacement;
                ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                if (!result.IsFrameCountKnown)
                {
                    Assert.False(result.IsPixelFormatKnown);
                    Assert.False(result.IsCumulativePixelCountKnown);
                    Assert.False(result.IsEstimatedPixelBytesKnown);
                    continue;
                }
                Assert.InRange(result.FrameCount, 1, mutated.Length / 14);
                Assert.InRange((long)result.Width * result.Height, 1, uint.MaxValue);
                AssertImage(result, result.Width, result.Height, result.FrameCount, result.ChannelCount);
                Assert.InRange(result.ChannelCount, 3, 4);
                Assert.Equal(mutated.Length - 8, BitConverter.ToInt32(mutated, 4));
            }
        }

        private static byte[][] Fixtures()
        {
            return new[] { Animation(), RealLossy, RealLossless, RealLosslessAlpha, RealLossyAlpha, RealAnimationLossless, RealAnimationLossy };
        }

        private static byte[] BadFrame(params byte[][] chunks)
        {
            return Riff(Vp8x(flags: 0x12), Anim(), Chunk("ANMF", Join(FrameHeader(), Join(chunks))));
        }

        private static byte[] Animation() { return Riff(Vp8x(), Anim(), Frame(), Frame(2, 2, 2, 1)); }

        private static ImageDecodeOptions Options(int length, int width, int height, int frames, int channels,
            long cumulative = -1, long storage = -1)
        {
            return new ImageDecodeOptions(length, width, height, (long)width * height, frames, true, true,
                1024, 1024, true, true, cumulative < 0 ? (long)width * height * frames : cumulative,
                8, channels, true, storage < 0 ? (long)width * height * frames * channels : storage);
        }

        private static void AssertImage(ImageIdentifyResult result, int width, int height, int frames, int channels)
        {
            Assert.Equal("webp", result.Format);
            Assert.True(result.IsSizeKnown);
            Assert.Equal(width, result.Width);
            Assert.Equal(height, result.Height);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(frames, result.FrameCount);
            Assert.True(result.IsPixelFormatKnown);
            Assert.Equal(8, result.BitDepth);
            Assert.Equal(channels, result.ChannelCount);
            Assert.True(result.IsCumulativePixelCountKnown);
            Assert.Equal((long)width * height * frames, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown);
            Assert.Equal((long)width * height * frames * channels, result.EstimatedPixelBytes);
        }

        private static void AssertUnknownContainer(ImageIdentifyResult result)
        {
            Assert.Equal("webp", result.Format);
            Assert.False(result.IsFrameCountKnown);
            Assert.Equal(0, result.FrameCount);
            Assert.False(result.IsPixelFormatKnown);
            Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        private static byte[] Riff(params byte[][] chunks)
        {
            byte[] payload = Join(chunks);
            return Join(Encoding.ASCII.GetBytes("RIFF"), U32((uint)payload.Length + 4), Encoding.ASCII.GetBytes("WEBP"), payload);
        }

        private static byte[] Vp8x(int width = 6, int height = 4, byte flags = 2)
        {
            return Chunk("VP8X", Join(new byte[] { flags, 0, 0, 0 }, U24(width - 1), U24(height - 1)));
        }

        private static byte[] Anim() { return Chunk("ANIM", new byte[6]); }

        // Synthetic fixtures contain bounded bitstream headers and one data byte, not decoded pixels.
        private static byte[] Vp8(int width = 6, int height = 4)
        {
            return Chunk("VP8 ", 0x30, 0, 0, 0x9D, 1, 0x2A, (byte)width, (byte)(width >> 8), (byte)height, (byte)(height >> 8), 0);
        }

        private static byte[] Vp8l(int width = 6, int height = 4, bool alpha = false)
        {
            uint bits = (uint)(width - 1) | ((uint)(height - 1) << 14) | (alpha ? 1U << 28 : 0);
            return Chunk("VP8L", Join(new byte[] { 0x2F }, U32(bits), new byte[1]));
        }

        private static byte[] FrameHeader(int width = 6, int height = 4, int x = 0, int y = 0, byte flags = 0)
        {
            return Join(U24(x), U24(y), U24(width - 1), U24(height - 1), U24(0), new byte[] { flags });
        }

        private static byte[] Frame(int width = 6, int height = 4, int x = 0, int y = 0, byte flags = 0)
        {
            return Chunk("ANMF", Join(FrameHeader(width, height, x, y, flags), Vp8l(width, height)));
        }

        private static byte[] U24(int value) { return new byte[] { (byte)value, (byte)(value >> 8), (byte)(value >> 16) }; }
        private static byte[] U32(uint value) { return new byte[] { (byte)value, (byte)(value >> 8), (byte)(value >> 16), (byte)(value >> 24) }; }

        private static byte[] Chunk(string type, params byte[] payload)
        {
            return Join(Encoding.ASCII.GetBytes(type), U32((uint)payload.Length), payload, new byte[payload.Length & 1]);
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

        // Pillow 13.0.0. All six files were independently decoded, including every animation
        // frame. Lossless RGBA bytes equal the originals. No Pillow dependency in these tests.
        // 100 bytes, SHA256 1da489ad7c3e0560b10b1b0cb5e29143ec0f77b26584470d77729a5d36aca10f.
        private static readonly byte[] RealLossy = Convert.FromBase64String(
            "UklGRlwAAABXRUJQVlA4IFAAAADwAQCdASoEAAMAAMASJZACdEyAAdWhF0AA/vqjxf/fywdT3sb0Kym35RObmvwwgdR/e/9+COWz+//yu/4Jp/6h5UWv9Df/zL8sv3/+TeUwAA==");

        // 78 bytes, SHA256 b8ea9dbe70e2a6997f1c01516bcfa26b530b3190a175ddd6032392347f1fba29.
        private static readonly byte[] RealLossless = Convert.FromBase64String(
            "UklGRkYAAABXRUJQVlA4TDkAAAAvA4AAAF8gFkzx7sAiyLY5xCHqTzMcsWAisTt/uCDmP3oB4KFp2gMAQBBAohBLDrOsNMukEf0PbHYA");

        // RGBA 4x3, 88 bytes, SHA256 5fea21462c68ead05a6723feb059b8ada5b8a2e5798888464508c059e050170d.
        private static readonly byte[] RealLosslessAlpha = Convert.FromBase64String(
            "UklGRlAAAABXRUJQVlA4TEQAAAAvA4AAEF8gFkzx7sAiyLY5xCHqTzMcsWAisTt/uCAIsm1UZnaU56a7SfQAADzV12qfYBCCIIBEIZYcZllplkkj+h/Y7A==");

        // RGBA 4x3, 140 bytes, SHA256 90cef5e5d249d3e1bcbac24cf63b6628e0550648045662ce0c4c8d41bb917463.
        private static readonly byte[] RealLossyAlpha = Convert.FromBase64String(
            "UklGRoQAAABXRUJQVlA4WAoAAAAQAAAAAwAAAgAAQUxQSA0AAAAAIEBggKDA4P//gGBAAFZQOCBQAAAA8AEAnQEqBAADAADAEiWYAnRMgAHVoRdAAP76o8X/38sHU97G9CspuAptMSP/cJk1R/ef95Lwnfv/8rv+DcufSOCK3/Bv/776DH3/+TfQ4AA=");

        // Three RGBA 6x4 frames, 194 bytes, SHA256 e73fb05f7dcffac3a68d1b981841b2768d25ceb1f24b67e6bd27ddaefbcac7c7.
        private static readonly byte[] RealAnimationLossless = Convert.FromBase64String(
            "UklGRroAAABXRUJQVlA4WAoAAAASAAAABQAAAwAAQU5JTQYAAAAAAAAAAABBTk1GKgAAAAAAAAAAAAUAAAMAAB4AAAJWUDhMEQAAAC8FwAAAB1CZUlSr/4GI6H8AAEFOTUYqAAAAAgAAAQAAAAAAAAAAKAAAAlZQOEwRAAAALwAAABAHUI9yl6yAgYjofwAAQU5NRioAAAABAAABAAAAAAAAAAAyAAAAVlA4TBEAAAAvAAAAAAdQ7qLUqP+BiOh/AAA=");

        // Three RGBA 6x4 frames, 304 bytes, SHA256 53c0b9824a58968a2fce5380aa6adea892674e0d758adcd6822cc6000a4943b3.
        private static readonly byte[] RealAnimationLossy = Convert.FromBase64String(
            "UklGRigBAABXRUJQVlA4WAoAAAASAAAABQAAAwAAQU5JTQYAAAAAAAAAAABBTk1GSgAAAAAAAAAAAAUAAAMAAB4AAAJWUDggMgAAALABAJ0BKgYABAAAwBIlkAADXxQXCUAA/v9jgsf//OJf/QL/wF34///KV/7Gv8FeVAAAQU5NRlIAAAACAAABAAAAAAAAAAAoAAACQUxQSAIAAAAAgFZQOCAwAAAAsAEAnQEqAQABAADAEiWgAANfFBaXAAD++EcqPPG6FX///eej//eej//eej/vLAAAQU5NRlAAAAABAAABAAAAAAAAAAAyAAAAVlA4IDgAAACUAQCdASoBAAEAAAASJaAAAzo9G0AA/t5B5//+8+f/9z5//3Pn/b///+0ff/+0ff/+0ff+1cgAAA==");
    }
}
