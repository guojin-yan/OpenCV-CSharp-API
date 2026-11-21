using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class PngHeaderPreflightTests
    {
        [Theory]
        [InlineData(24)]
        [InlineData(26)]
        [InlineData(29)]
        [InlineData(32)]
        public void IdentifyRejectsIncompletePngIhdrBeforePublishingHeaderFacts(int length)
        {
            byte[] data = Png(); Array.Resize(ref data, length);
            AssertUnknownHeader(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData(11, 0)]
        [InlineData(11, 12)]
        [InlineData(11, 14)]
        [InlineData(16, 128)]
        [InlineData(19, 0)]
        [InlineData(20, 128)]
        [InlineData(23, 0)]
        [InlineData(24, 3)]
        [InlineData(25, 1)]
        [InlineData(26, 1)]
        [InlineData(27, 1)]
        [InlineData(28, 2)]
        public void IdentifyRejectsUnsupportedPngIhdrFieldsAsOneHeader(int offset, int value)
        {
            byte[] data = Png(); data[offset] = (byte)value;
            AssertUnknownHeader(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 32)]
        [InlineData(2, 1)]
        [InlineData(2, 4)]
        [InlineData(3, 16)]
        [InlineData(4, 2)]
        [InlineData(6, 4)]
        [InlineData(5, 8)]
        public void IdentifyRejectsNonstandardPngColorDepthPairs(int color, int depth)
        {
            AssertUnknownHeader(ImgCodecsCv2.Identify(Png(depth, color)));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(8)]
        public void IdentifyRejectsIndexedPngCompletionWithoutRequiredPalette(int depth)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Png(depth, 3));
            AssertHeader(result, depth, 1); AssertNoCompletion(result);
        }

        [Theory]
        [InlineData("empty")]
        [InlineData("partial-entry")]
        [InlineData("over-256")]
        [InlineData("over-depth")]
        [InlineData("duplicate")]
        [InlineData("after-idat")]
        [InlineData("gray")]
        [InlineData("gray-alpha")]
        public void IdentifyRejectsConflictingPngPaletteDeclarations(string kind)
        {
            byte[] palette = Chunk("PLTE", new byte[6]);
            byte[] data;
            switch (kind)
            {
                case "empty": data = Png(1, 3, Chunk("PLTE"), Idat()); break;
                case "partial-entry": data = Png(1, 3, Chunk("PLTE", new byte[4]), Idat()); break;
                case "over-256": data = Png(8, 3, Chunk("PLTE", new byte[771]), Idat()); break;
                case "over-depth": data = Png(1, 3, Chunk("PLTE", new byte[9]), Idat()); break;
                case "duplicate": data = Png(1, 3, palette, palette, Idat()); break;
                case "after-idat": data = Png(1, 3, Idat(), palette); break;
                case "gray": data = Png(8, 0, palette, Idat()); break;
                default: data = Png(8, 4, palette, Idat()); break;
            }
            AssertNoCompletion(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData("unknown-critical")]
        [InlineData("reserved-bit")]
        [InlineData("non-letter")]
        [InlineData("duplicate-ihdr")]
        [InlineData("missing-idat")]
        [InlineData("empty-idat")]
        [InlineData("split-idat")]
        [InlineData("long-iend")]
        [InlineData("trailing-byte")]
        public void IdentifyRejectsInvalidPngCoreChunkGrammar(string kind)
        {
            byte[] data;
            switch (kind)
            {
                case "unknown-critical": data = Png(8, 2, Chunk("ABCD"), Idat()); break;
                case "reserved-bit": data = Png(8, 2, Chunk("abcd"), Idat()); break;
                case "non-letter": data = Png(8, 2, Chunk("a1CD"), Idat()); break;
                case "duplicate-ihdr": data = Png(8, 2, Header(), Idat()); break;
                case "missing-idat": data = Join(Signature, Header(), Chunk("IEND")); break;
                case "empty-idat": data = Png(8, 2, Chunk("IDAT")); break;
                case "split-idat": data = Png(8, 2, Idat(), Chunk("tEXt", 65, 0, 66), Idat()); break;
                case "long-iend": data = Join(Signature, Header(), Idat(), Chunk("IEND", 1)); break;
                default: data = Join(Png(), new byte[] { 0 }); break;
            }
            AssertNoCompletion(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData("indexed-without-plte")]
        [InlineData("indexed-too-long")]
        [InlineData("indexed-empty")]
        [InlineData("gray-short")]
        [InlineData("rgb-short")]
        [InlineData("gray-alpha")]
        [InlineData("rgba")]
        [InlineData("duplicate")]
        [InlineData("after-idat")]
        [InlineData("before-optional-plte")]
        public void IdentifyRejectsConflictingPngTransparencyEnvelopes(string kind)
        {
            byte[] palette = Chunk("PLTE", new byte[6]);
            byte[] data;
            switch (kind)
            {
                case "indexed-without-plte": data = Png(1, 3, Chunk("tRNS", 0), Idat()); break;
                case "indexed-too-long": data = Png(1, 3, palette, Chunk("tRNS", 0, 1, 2), Idat()); break;
                case "indexed-empty": data = Png(1, 3, palette, Chunk("tRNS"), Idat()); break;
                case "gray-short": data = Png(8, 0, Chunk("tRNS", 0), Idat()); break;
                case "rgb-short": data = Png(8, 2, Chunk("tRNS", new byte[5]), Idat()); break;
                case "gray-alpha": data = Png(8, 4, Chunk("tRNS", 0, 1), Idat()); break;
                case "rgba": data = Png(8, 6, Chunk("tRNS", new byte[6]), Idat()); break;
                case "duplicate": data = Png(8, 0, Chunk("tRNS", 0, 1), Chunk("tRNS", 0, 1), Idat()); break;
                case "after-idat": data = Png(8, 0, Idat(), Chunk("tRNS", 0, 1)); break;
                default: data = Png(8, 2, Chunk("tRNS", new byte[6]), palette, Idat()); break;
            }
            AssertNoCompletion(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData("empty-name")]
        [InlineData("long-name")]
        [InlineData("leading-space")]
        [InlineData("trailing-space")]
        [InlineData("repeated-space")]
        [InlineData("control")]
        [InlineData("nbsp")]
        [InlineData("no-terminator")]
        [InlineData("no-method")]
        [InlineData("unsupported-method")]
        [InlineData("no-data")]
        [InlineData("duplicate")]
        [InlineData("after-plte")]
        [InlineData("after-idat")]
        public void IdentifyRejectsInvalidPngIccStorageEnvelopes(string kind)
        {
            byte[] icc = Icc(); byte[] data;
            switch (kind)
            {
                case "empty-name": icc = Chunk("iCCP", 0, 0, 1); break;
                case "long-name": icc = Icc(new string('a', 80)); break;
                case "leading-space": icc = Icc(" a"); break;
                case "trailing-space": icc = Icc("a "); break;
                case "repeated-space": icc = Icc("a  b"); break;
                case "control": icc = Chunk("iCCP", 31, 0, 0, 1); break;
                case "nbsp": icc = Chunk("iCCP", 160, 0, 0, 1); break;
                case "no-terminator": icc = Chunk("iCCP", 65); break;
                case "no-method": icc = Chunk("iCCP", 65, 0); break;
                case "unsupported-method": icc = Chunk("iCCP", 65, 0, 1, 1); break;
                case "no-data": icc = Chunk("iCCP", 65, 0, 0); break;
            }
            if (kind == "duplicate") data = Png(8, 2, icc, icc, Idat());
            else if (kind == "after-plte") data = Png(8, 2, Chunk("PLTE", new byte[3]), icc, Idat());
            else if (kind == "after-idat") data = Png(8, 2, Idat(), icc);
            else data = Png(8, 2, icc, Idat());
            AssertHeader(ImgCodecsCv2.Identify(data), 8, 3);
            AssertNoCompletion(ImgCodecsCv2.Identify(data));
        }

        [Fact]
        public void IdentifyRequiresInitialPngIhdrBeforeMetadataCompletion()
        {
            AssertUnknownHeader(ImgCodecsCv2.Identify(Join(Signature, Icc(), Header(), Idat(), Chunk("IEND"))));
        }

        [Theory]
        [InlineData(0, 1, 1)]
        [InlineData(0, 2, 1)]
        [InlineData(0, 4, 1)]
        [InlineData(0, 8, 1)]
        [InlineData(0, 16, 1)]
        [InlineData(2, 8, 3)]
        [InlineData(2, 16, 3)]
        [InlineData(3, 1, 1)]
        [InlineData(3, 2, 1)]
        [InlineData(3, 4, 1)]
        [InlineData(3, 8, 1)]
        [InlineData(4, 8, 2)]
        [InlineData(4, 16, 2)]
        [InlineData(6, 8, 4)]
        [InlineData(6, 16, 4)]
        public void IdentifyAcceptsAllStandardPngColorDepthPairs(int color, int depth, int channels)
        {
            foreach (byte interlace in new byte[] { 0, 1 })
            {
                byte[] header = Header(depth, color); header[20] = interlace;
                byte[] data = Join(Signature, header, color == 3 ? Chunk("PLTE", new byte[3 * (1 << depth)]) : new byte[0], Idat(), Chunk("IEND"));
                AssertComplete(ImgCodecsCv2.Identify(data), depth, channels);
            }
        }

        [Fact]
        public void IdentifyKeepsEncodedPngChannelsWhenPaletteOrTransparencyExpandsAtDecode()
        {
            AssertComplete(ImgCodecsCv2.Identify(Png(1, 3, Chunk("PLTE", new byte[6]), Chunk("tRNS", 0), Idat())), 1, 1);
            // PNG 3 requires decoders to mask unused upper bits in tRNS samples.
            AssertComplete(ImgCodecsCv2.Identify(Png(1, 0, Chunk("tRNS", 255, 255), Idat())), 1, 1);
            AssertComplete(ImgCodecsCv2.Identify(Png(8, 2, Chunk("PLTE", new byte[768]), Chunk("tRNS", 255, 1, 255, 2, 255, 3), Idat())), 8, 3);
        }

        [Fact]
        public void IdentifyCountsPngStoredMetadataWithoutInflatingProfilesOrText()
        {
            byte[] data = Png(8, 2, Icc(), Chunk("sRGB", 0), Chunk("eXIf", 1, 2, 3), Chunk("tEXt", 65, 0, 66),
                Idat(), Chunk("zTXt", 65, 0, 0, 1), Chunk("iTXt", 65, 0, 0, 0, 0, 0, 66), Chunk("vpAg", 1, 2));
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            AssertComplete(result, 8, 3);
            Assert.Equal(22, result.MetadataBytes); Assert.Equal(2, result.IccProfileBytes);
            // PNG 3 allows sRGB together with iCCP (SHOULD NOT is not SHALL NOT).
            AssertComplete(ImgCodecsCv2.Identify(Png(8, 2, Icc(new string('a', 79)), Idat())), 8, 3);
            AssertComplete(ImgCodecsCv2.Identify(Png(8, 2, Chunk("iCCP", 161, 255, 0, 0, 1), Idat())), 8, 3);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyRejectsDuplicateOrMisplacedPngExif(bool duplicate)
        {
            byte[] exif = Chunk("eXIf", 1, 2, 3);
            AssertNoCompletion(ImgCodecsCv2.Identify(duplicate ? Png(8, 2, exif, exif, Idat()) : Png(8, 2, Idat(), exif)));
        }

        [Fact]
        public void PngHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams()
        {
            byte[] data = Png(16, 6, Icc(), Idat());
            ImageDecodeOptions exact = Options(data.Length);
            var limits = new[]
            {
                Options(data.Length, width: 3), Options(data.Length, height: 2), Options(data.Length, pixels: 11),
                Options(data.Length, cumulative: 11), Options(data.Length, depth: 8), Options(data.Length, channels: 3),
                Options(data.Length, storage: 95), Options(data.Length, metadata: 4), Options(data.Length, icc: 1)
            };
            foreach (ImageDecodeOptions options in limits)
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, options));
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(data), options));
            }
            foreach (ImageDecodeOptions options in JoinOptions(exact, limits))
            {
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, data)))
                {
                    stream.Position = 1;
                    if (options == exact) AssertComplete(ImgCodecsCv2.Identify(stream, options), 16, 4);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadStream(data))
                {
                    if (options == exact) AssertComplete(ImgCodecsCv2.Identify(stream, options), 16, 4);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(data.Length, stream.BytesRead);
                }
            }
        }

        [Fact]
        public void PngStrictPolicyRejectsUnknownHeaderOrMetadataFacts()
        {
            byte[] invalid = Png(); invalid[26] = 1;
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(invalid, Options(invalid.Length)));
            byte[] paletteMissing = Png(1, 3);
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(paletteMissing, Options(paletteMissing.Length)));
            using (var stream = new MemoryStream(paletteMissing))
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.Identify(stream, Options(paletteMissing.Length)));
                Assert.Equal(0, stream.Position);
            }
        }

        [Theory]
        [MemberData(nameof(RealPngFixtures))]
        public void IdentifyReadsRealPngColorDepthPaletteAndAdam7Fixtures(string name, int depth, int channels,
            int length, string sha256, long metadata, long icc, string encoded)
        {
            Assert.NotEmpty(name);
            byte[] data = Convert.FromBase64String(encoded);
            Assert.Equal(length, data.Length);
            Assert.Equal(sha256, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)).ToLowerInvariant());
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            AssertComplete(result, depth, channels);
            Assert.Equal(metadata, result.MetadataBytes); Assert.Equal(icc, result.IccProfileBytes);
            AssertComplete(ImgCodecsCv2.Identify(new ReadOnlySpan<byte>(data)), depth, channels);
        }

        [Fact]
        public void IdentifyKeepsPngCompletionUnknownForEveryStrictPrefixAndHugeChunkLength()
        {
            foreach (byte[] data in Fixtures())
            {
                for (int length = 1; length < data.Length; ++length)
                {
                    byte[] prefix = new byte[length]; Array.Copy(data, prefix, length);
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                    AssertNoCompletion(result);
                    if (length < 33) Assert.False(result.IsSizeKnown);
                }
                foreach (uint declared in new[] { 0x7FFFFFFFU, 0x80000000U, uint.MaxValue })
                {
                    byte[] invalid = (byte[])data.Clone(); Array.Copy(U32(declared), 0, invalid, 33, 4);
                    AssertNoCompletion(ImgCodecsCv2.Identify(invalid));
                }
            }
            byte[] headerOnly = Join(Signature, Header());
            AssertHeader(ImgCodecsCv2.Identify(headerOnly), 8, 3);
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusPngNeverEscapesOrPublishesInconsistentHeaderFacts()
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
                    Assert.InRange(result.Height, 1, int.MaxValue); Assert.InRange(result.BitDepth, 1, 16);
                    Assert.InRange(result.ChannelCount, 1, 4);
                }
                if (result.IsFrameCountKnown)
                {
                    Assert.True(result.IsSizeKnown); Assert.Equal(1, result.FrameCount);
                    Assert.Equal((long)result.Width * result.Height, result.CumulativePixelCount);
                    long expected = result.CumulativePixelCount * ((result.BitDepth + 7) / 8) * result.ChannelCount;
                    Assert.Equal(expected, result.EstimatedPixelBytes);
                }
                else { Assert.False(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown); }
                if (result.IsMetadataSizeKnown)
                {
                    Assert.True(result.IsIccProfileSizeKnown); Assert.True(result.IsFrameCountKnown);
                    Assert.InRange(result.MetadataBytes, 0, mutated.Length);
                    Assert.InRange(result.IccProfileBytes, 0, result.MetadataBytes);
                }
            }
        }

        private static IEnumerable<byte[]> Fixtures()
        {
            yield return Png(8, 2, Icc(), Chunk("tEXt", 65, 0, 66), Idat());
            yield return Png(2, 3, Chunk("PLTE", new byte[12]), Chunk("tRNS", 0, 255), Idat());
            foreach (object[] fixture in RealPngFixtures) yield return Convert.FromBase64String((string)fixture[7]);
        }

        // Byte-authored PNGs cover all fifteen standard color/depth pairs, including
        // three Adam7 images. PyPNG independently checked every original stored sample
        // (including full 16-bit channels). Pillow independently decoded all images
        // and palette/tRNS expansion; its multi-channel 16-bit output is reduced to 8-bit.
        // The RGB8 file contains a real 588-byte sRGB ICC plus three text chunk types.
        public static IEnumerable<object[]> RealPngFixtures
        {
            get
            {
                yield return new object[] { "c0d1", 1, 1, 98, "4b581702297cda8f01a06b5c1ca993e90e777269b8ba11a19d72d229798eb4a9", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADAQAAAAHriKP9AAAAAElEQVQ1rwYeAAAACElEQVR4nGNoYGhgOK6qNDAAAAAJSURBVMAAAgEAD5wCEbHYYyUAAAAASUVORK5CYII=" };
                yield return new object[] { "c0d2", 2, 1, 95, "101bf252537f558d2e5dca0a972f0041d199bc37be8fa03eb8efcd2b971cbc79", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADAgAAAADbL+m7AAAAAElEQVQ1rwYeAAAAB0lEQVR4nGM4xrCR4ZepGwAAAAdJREFUIQcABmMB5Owf5DEAAAAASUVORK5CYII=" };
                yield return new object[] { "c0d4", 4, 1, 112, "7345da82ec613b8624b248e72cd0b80760320876a8adc6acf2df6c5284b45056", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADBAAAAABUbxwbAAAAAnRSTlMAA++anIIAAAAASURBVDWvBh4AAAAISURBVHicY7C4xJDN/cbS9AAAAAlJREFUyjDPAgALJgJRuxKbIgAAAABJRU5ErkJggg==" };
                yield return new object[] { "c0d8", 8, 1, 104, "99bbd02d698b69a7fead8b601e710f589657828cf84fca6dcdeaa0ede1ec3b0b", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADCAAAAACRn/EaAAAAAElEQVQ1rwYeAAAAC0lEQVR4nGNg1vAtYhCzTnEPMY4AAAAMSURBVGhl0PQrngEAFisDoyRFe0sAAAAASUVORK5CYII=" };
                yield return new object[] { "c0d16", 16, 1, 116, "8af20fd7281495093b23ead026a7f8a58bb47adb05319405fd88a3b4c5624b0d", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADEAAAAADBDy1ZAAAAAElEQVQ1rwYeAAAAEUlEQVR4nGNgu6Yt659SvJpBstx2X+mgbRYAAAASSURBVBJrmw+DjkRAfMmyGW8BcY0JTBoLVCMAAAAASUVORK5CYII=" };
                yield return new object[] { "c2d8", 8, 3, 627, "7188b5788189a9445f2c8135ba0bfcdee947a5afec184f9512e033190e4c1acd", 448, 361, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADCAIAAAA7ljmRAAABb2lDQ1BzUkdCAAB4nHWRPUvDUBSGH1vFYpUOKog4dKjioCAK4igV7KIObQWrLk2atEJTQ9IixVVwcRAcRBe/Bv+BroKrgiAogoizo1+LSDy3FSrS3nBzHt5738PJG/DN5HXLbZ4Eq1B04rFoeCG1GG59IUg3LYwRSOuuPZuYTtJwfd7RpOrtsOrV+F7dFcwYrg5NAeFx3XaKwjINM2tFW/GWcJeeS2eED4WHHBlQ+ErpWpWfFWer/K7YScanwKd6hrN/WPvDes6xhAeFI1a+pP/Oo76k3SjMJ6T2yu7DJU6MKGE0SqyQp8iw1IJkVt83UvHNsSoeXd42ZRxxZMmJd0jUknQ1pJqiG/LkKavc/+fpmmOj1e7tUWh58ry3fmjdge9tz/s68rzvY/A/wkWh5l+VnCY+RN+uaZEDCG3A2WVN03bhfBN6Huy0k65Iftk+04TXU+hIQecNtC1Vs/o95+Qekuvyi65hbx8G5H5o+QcmV2gbCxEUBwAAABN0RVh0VGl0bGUAY29sb3Igc2FtcGxlc44yfT8AAAAfelRYdE5vdGUAAHicS8ovzUtJTVEoLskvAlIlqRUlAErQB3jZ4ucUAAAAH2lUWHREZXNjcmlwdGlvbgAAAGVuAABVVEYtOCBzYW1wbGVzWXcm5QAAAABJREFUNa8GHgAAABlJREFUeJwBJwDY/wADTJUocbpNlt9yuwQAFl+oOz0mvS0AAAAZSURBVITNYKnyhc4XAClyu06X4HO8BZjhKj1mESuzgykGAAAAAElFTkSuQmCC" };
                yield return new object[] { "c2d16", 16, 3, 183, "9deae497c542ebf37bc4b77e9796cf9667bba4ff8e52a0b5580a728864af63d9", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADEAIAAABrBuXSAAAABnRSTlMG1k5plfw6unG8AAAAAElEQVQ1rwYeAAAAKklEQVR4nGNgu+aXOfWPtmzRhl3O/inTvt/rKl69247pIoNkeSLXirm2+1oDzzzEtDYsAAAAKklEQVRJYl0546N2m8/Z+6JFDDoSIBUB8dM/3W8tWbbHkvnMjLcPGjSEASkoI51+Em7WAAAAAElFTkSuQmCC" };
                yield return new object[] { "c3d1", 1, 1, 113, "d59c04d0b7ee5f135d9e6e43bd537ae71db4be5aa5dea23663527e44c300b6fb", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADAQMAAACOOjyFAAAABlBMVEULFy9AfHavR9LFAAAAAElEQVQ1rwYeAAAAB0lEQVR4nGNYwBDAbFf/IQAAAAdJREFUsAAABLYBkbmBE+IAAAAASUVORK5CYII=" };
                yield return new object[] { "c3d2", 2, 1, 133, "336f886a84fcd6ac617d6253d4194965649b384c0adfa82b9cdd66676052a162", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADAgMAAADJmkZVAAAADFBMVEULFy9AfHZ14b2qRgQD12gyAAAAAnRSTlMAf7YpoZUAAAAASURBVDWvBh4AAAAHSURBVHicYzjGsJHhl6kbAAAAB0lEQVQhBwAGYwHk7B/kMQAAAABJRU5ErkJggg==" };
                yield return new object[] { "c3d4", 4, 1, 126, "f795d1a4ee0fbce090f462b01875ea6629d65ea16bc8faa3eb0a6ee2ae87c3a5", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADBAMAAAEx3YNjAAAADFBMVEULFy9AfHZ14b2qRgQD12gyAAAAAElEQVQ1rwYeAAAACklEQVR4nGMwYBBgEGZgLvjI7AAAAAtJREFUYlBgUGYEAARYAJpaIExdAAAAAElFTkSuQmCC" };
                yield return new object[] { "c3d8", 8, 1, 139, "4cffddca60593db9b68eac8d7327aebf4db4151442e11617d54f66cc3fae2b0b", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADCAMAAACDKl70AAAADFBMVEULFy9AfHZ14b2qRgQD12gyAAAAAnRSTlMAf7YpoZUAAAAASURBVDWvBh4AAAAKSURBVHicY2BmYGRiYAIPV64SAAAACklEQVSSQJqZAQAAmwATQbnaHQAAAABJRU5ErkJggg==" };
                yield return new object[] { "c4d8", 8, 2, 117, "9a40c14d9a2d966ef2e80913ef4e988e0dd71b983fee73575da068fe901f4206", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADCAQAAAAe/WZNAAAAAElEQVQ1rwYeAAAAEklEQVR4nGNg9tEo9J1WtJtBLN66JWEtMi+GAAAAEklEQVRl6zkGzSK/6cV7ZjwEAHtRCrHZjR+TAAAAAElFTkSuQmCC" };
                yield return new object[] { "c4d16", 16, 2, 143, "00d7516abf4b312abb5ea05baf6f15cf0c17657b8991cfbfc9bfa5ed6d846761", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADEAQAAABObboOAAAAAElEQVQ1rwYeAAAAH0lEQVR4nAEzAMz/AAbWTmkrHXKwT2SW93Oruz4AGXdhCj2++ThxvQAAAB9JREFUhVFiBamYhkzN3wAsGHOrUF+X8nSmvDmY7eCAFnUW1fR30BsAAAAASUVORK5CYII=" };
                yield return new object[] { "c6d8", 8, 4, 159, "a6cc7e5bec1406eb7f456f679f91035e7a98f700692b712639a12b29f3520098", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADCAYAAAC09K7GAAAABlBMVEUAAAD///+l2Z/dAAAAAElEQVQ1rwYeAAAAHklEQVR4nGNg9pl6T6NwF7PvtPsaRbtZfBnE4ld8tG45K5ayJuguAAAAHklEQVSw8pN16znxBAZNoLDf9AeaxXtY/WY81CoGABA9FRGljyBAAAAAAElFTkSuQmCC" };
                yield return new object[] { "c6d16", 16, 4, 191, "6fa97399d4770ac5f1672111f89d78422e60f6fac1e5d081eddf2a0bde8aa999", 0, 0, "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADEAYAAAGTY0ITAAAAAElEQVQ1rwYeAAAAN0lEQVR4nGNgu+aXOfXP3X4G/5Rp3+91qcky6EgUr95tx3SxZNkeS+Yz3vEM2rJFG3Y5M16DCHulMATE9LTVoQAAADdJREFUT/90v1VdYsbbBw0awvnLGCTLE7lWzP1gYLuvNfDME5HyJNaVMz5qW+xr8zl7X7QolhUA3q4spXp0CVoAAAAASUVORK5CYII=" };
            }
        }

        private static void AssertUnknownHeader(ImageIdentifyResult result)
        {
            Assert.Equal("png", result.Format); Assert.False(result.IsSizeKnown); Assert.False(result.IsPixelFormatKnown);
            AssertNoCompletion(result);
        }

        private static void AssertHeader(ImageIdentifyResult result, int depth, int channels)
        {
            Assert.Equal("png", result.Format); Assert.True(result.IsSizeKnown);
            Assert.Equal(4, result.Width); Assert.Equal(3, result.Height);
            Assert.True(result.IsPixelFormatKnown); Assert.Equal(depth, result.BitDepth); Assert.Equal(channels, result.ChannelCount);
        }

        private static void AssertComplete(ImageIdentifyResult result, int depth, int channels)
        {
            AssertHeader(result, depth, channels); Assert.True(result.IsFrameCountKnown); Assert.Equal(1, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown); Assert.Equal(12, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown); Assert.Equal(12 * ((depth + 7) / 8) * channels, result.EstimatedPixelBytes);
            Assert.True(result.IsMetadataSizeKnown); Assert.True(result.IsIccProfileSizeKnown);
        }

        private static void AssertNoCompletion(ImageIdentifyResult result)
        {
            Assert.False(result.IsFrameCountKnown); Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown); Assert.False(result.IsMetadataSizeKnown); Assert.False(result.IsIccProfileSizeKnown);
        }

        private static ImageDecodeOptions Options(int length, int width = 4, int height = 3, long pixels = 12,
            long cumulative = 12, int depth = 16, int channels = 4, long storage = 96, long metadata = 5, long icc = 2)
        {
            return new ImageDecodeOptions(length, width, height, pixels, 1, true, true, metadata, icc, true, true,
                cumulative, depth, channels, true, storage);
        }

        private static IEnumerable<ImageDecodeOptions> JoinOptions(ImageDecodeOptions exact, ImageDecodeOptions[] limits)
        {
            yield return exact; foreach (ImageDecodeOptions limit in limits) yield return limit;
        }

        private static byte[] Png(int depth = 8, int color = 2, params byte[][] chunks)
        {
            return Join(Signature, Header(depth, color), chunks.Length == 0 ? Idat() : Join(chunks), Chunk("IEND"));
        }

        private static byte[] Header(int depth = 8, int color = 2)
        {
            return Chunk("IHDR", Join(U32(4), U32(3), new byte[] { (byte)depth, (byte)color, 0, 0, 0 }));
        }

        // Synthetic IDAT/iCCP data deliberately exercises stored ranges without inflation or CRC validation.
        private static byte[] Idat() { return Chunk("IDAT", 1); }
        private static byte[] Icc(string name = "A") { return Chunk("iCCP", Join(Encoding.ASCII.GetBytes(name), new byte[] { 0, 0, 1, 2 })); }
        private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
        private static byte[] Chunk(string type, params byte[] payload)
        {
            return Join(U32((uint)payload.Length), Encoding.ASCII.GetBytes(type), payload, new byte[4]);
        }

        private static byte[] U32(uint value)
        {
            return new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
        }

        private static byte[] Join(params byte[][] pieces)
        {
            int length = 0; foreach (byte[] piece in pieces) length += piece.Length;
            byte[] result = new byte[length]; int offset = 0;
            foreach (byte[] piece in pieces) { Array.Copy(piece, 0, result, offset, piece.Length); offset += piece.Length; }
            return result;
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
