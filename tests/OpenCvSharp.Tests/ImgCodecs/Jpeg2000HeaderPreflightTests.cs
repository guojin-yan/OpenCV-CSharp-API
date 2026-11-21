using System;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class Jpeg2000HeaderPreflightTests
    {
        [Theory]
        [InlineData(5, 3)]
        [InlineData(4, 2)]
        public void IdentifyRequiresJp2ImageHeaderAndCodestreamGeometryToAgree(int width, int height)
        {
            AssertUnknown(ImgCodecsCv2.Identify(Jp2(Siz(), Header(width, height))));
        }

        [Fact]
        public void IdentifyRequiresJp2ImageHeaderComponentsToAgree()
        {
            AssertUnknown(ImgCodecsCv2.Identify(Jp2(Siz(), Header(channels: 4))));
        }

        [Theory]
        [InlineData(15)]
        [InlineData(135)]
        public void IdentifyRequiresJp2ConstantPrecisionAndSignToAgree(int descriptor)
        {
            AssertUnknown(ImgCodecsCv2.Identify(Jp2(Siz(), Header(descriptor: descriptor))));
        }

        [Fact]
        public void IdentifyRequiresJp2PerComponentPrecisionToAgree()
        {
            AssertUnknown(ImgCodecsCv2.Identify(Jp2(Siz(), Header(descriptor: 255,
                extra: Box("bpcc", new byte[] { 7, 15, 7 })))));
        }

        [Fact]
        public void IdentifyRejectsDuplicateJp2CodestreamBoxes()
        {
            AssertUnknown(ImgCodecsCv2.Identify(Join(Jp2(Siz()), Box("jp2c", Siz()))));
        }

        [Fact]
        public void IdentifyNeverIgnoresMalformedJp2BoxesAfterCodestream()
        {
            AssertUnknown(ImgCodecsCv2.Identify(Join(Jp2(Siz()), new byte[] { 0 })));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyRejectsJpeg2000PrecisionOutsidePartOneRange(bool container)
        {
            byte[] data = Siz(descriptors: new byte[] { 38 });
            AssertUnknown(ImgCodecsCv2.Identify(container ? Jp2(data, Header(channels: 1, descriptor: 38)) : data));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyRejectsJpeg2000TileGridsAbove65535(bool container)
        {
            byte[] data = Siz(256, 256);
            Put32(data, 24, 1); Put32(data, 28, 1);
            AssertUnknown(ImgCodecsCv2.Identify(container ? Jp2(data, Header(256, 256)) : data));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void IdentifyReadsJp2ShortExtendedAndToEofCodestreamBoxes(int lengthForm)
        {
            AssertHeader(ImgCodecsCv2.Identify(Jp2(Siz(), lengthForm: lengthForm)), 4, 3, 8, 3);
            byte[] descriptors = { 7, 139, 7 };
            ImageIdentifyResult mixed = ImgCodecsCv2.Identify(Jp2(Siz(descriptors: descriptors),
                Header(descriptor: 255, extra: Box("bpcc", descriptors, 1)), lengthForm));
            Assert.True(mixed.IsSizeKnown); Assert.Equal(3, mixed.ChannelCount);
            Assert.False(mixed.IsBitDepthKnown);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(8)]
        [InlineData(16)]
        [InlineData(31)]
        [InlineData(32)]
        [InlineData(38)]
        public void IdentifyReportsPartOneEncodedPrecisionWithoutClaimingNativeSupport(int depth)
        {
            foreach (int channels in new[] { 1, 3, 4, 16384 })
            foreach (int sign in new[] { 0, 128 })
            {
                byte[] descriptors = new byte[channels]; Array.Fill(descriptors, (byte)(sign + depth - 1));
                byte[] raw = Siz(descriptors: descriptors);
                AssertHeader(ImgCodecsCv2.Identify(raw), 4, 3, depth, channels);
                AssertHeader(ImgCodecsCv2.Identify(Jp2(raw, Header(channels: channels, descriptor: descriptors[0]))), 4, 3, depth, channels);
                AssertHeader(ImgCodecsCv2.Identify(Jp2(raw, Header(channels: channels, descriptor: 255, extra: Box("bpcc", descriptors)))), 4, 3, depth, channels);
            }
        }

        [Fact]
        public void IdentifyBoundsJpeg2000TileGridAndUnsignedReferenceCoordinates()
        {
            byte[] raw = Siz(65535, 1); Put32(raw, 24, 1);
            AssertHeader(ImgCodecsCv2.Identify(raw), 65535, 1, 8, 3);
            raw = Siz();
            Put32(raw, 8, uint.MaxValue); Put32(raw, 16, uint.MaxValue - 4); Put32(raw, 32, uint.MaxValue - 4);
            Put32(raw, 12, uint.MaxValue); Put32(raw, 20, uint.MaxValue - 3); Put32(raw, 36, uint.MaxValue - 3);
            AssertHeader(ImgCodecsCv2.Identify(Jp2(raw)), 4, 3, 8, 3);
            raw[43] = raw[44] = 255;
            AssertHeader(ImgCodecsCv2.Identify(raw), 4, 3, 8, 3);
        }

        [Theory]
        [InlineData("missing-ftyp")]
        [InlineData("missing-header")]
        [InlineData("missing-codestream")]
        [InlineData("wrong-brand")]
        [InlineData("misaligned-brands")]
        [InlineData("duplicate-ftyp")]
        [InlineData("duplicate-header")]
        [InlineData("stream-before-header")]
        [InlineData("duplicate-ihdr")]
        [InlineData("bpcc-before-ihdr")]
        [InlineData("truncated-ihdr")]
        [InlineData("missing-bpcc")]
        [InlineData("short-bpcc")]
        [InlineData("long-bpcc")]
        [InlineData("duplicate-bpcc")]
        [InlineData("child-past-parent")]
        [InlineData("nested-to-eof")]
        [InlineData("huge-extended")]
        [InlineData("short-box")]
        [InlineData("wrong-compression")]
        public void IdentifyRejectsAmbiguousOrUnboundedJp2Headers(string failure)
        {
            byte[] raw = Siz(), header = Header(), ihdr = Payload(header), data;
            byte[] bpcc = Box("bpcc", new byte[] { 7, 7, 7 });
            switch (failure)
            {
                case "missing-ftyp": data = Join(Signature(), header, Box("jp2c", raw)); break;
                case "missing-header": data = Top(Box("jp2c", raw)); break;
                case "missing-codestream": data = Top(header); break;
                case "wrong-brand": data = Join(Signature(), Box("ftyp", Join(Encoding.ASCII.GetBytes("jpx "), new byte[4])), header, Box("jp2c", raw)); break;
                case "misaligned-brands": data = Join(Signature(), Box("ftyp", new byte[9]), header, Box("jp2c", raw)); break;
                case "duplicate-ftyp": data = Top(FileType(), header, Box("jp2c", raw)); break;
                case "duplicate-header": data = Top(header, header, Box("jp2c", raw)); break;
                case "stream-before-header": data = Top(Box("jp2c", raw), header); break;
                case "duplicate-ihdr": data = Jp2(raw, Box("jp2h", Join(ihdr, ihdr))); break;
                case "bpcc-before-ihdr": data = Jp2(raw, Box("jp2h", Join(bpcc, ihdr))); break;
                case "truncated-ihdr": data = Jp2(raw, Box("jp2h", Box("ihdr", new byte[13]))); break;
                case "missing-bpcc": data = Jp2(raw, Header(descriptor: 255)); break;
                case "short-bpcc": data = Jp2(raw, Header(descriptor: 255, extra: Box("bpcc", new byte[] { 7, 7 }))); break;
                case "long-bpcc": data = Jp2(raw, Header(descriptor: 255, extra: Box("bpcc", new byte[] { 7, 7, 7, 7 }))); break;
                case "duplicate-bpcc": data = Jp2(raw, Header(descriptor: 255, extra: Join(bpcc, bpcc))); break;
                case "child-past-parent": Put32(header, 8, (uint)header.Length); data = Jp2(raw, header); break;
                case "nested-to-eof": Put32(header, 8, 0); data = Jp2(raw, header); break;
                case "huge-extended": byte[] huge = Box("free", Array.Empty<byte>(), 1); Put32(huge, 8, uint.MaxValue); data = Join(Jp2(raw), huge); break;
                case "short-box": byte[] shortBox = Box("free", Array.Empty<byte>()); Put32(shortBox, 0, 7); data = Join(Jp2(raw), shortBox); break;
                case "wrong-compression": header[27] = 6; data = Jp2(raw, header); break;
                default: throw new ArgumentException(failure);
            }
            AssertUnknown(ImgCodecsCv2.Identify(data));
        }

        [Fact]
        public void IdentifySkipsOpaqueJp2BoxesAndAcceptsCompatibleFutureBrands()
        {
            byte[] header = Box("jp2h", Join(Payload(Header()), Box("uuid", new byte[] { 0, 1, 2 }, 1)), 1);
            byte[] ftyp = Box("ftyp", Join(Encoding.ASCII.GetBytes("jpx "), new byte[4], Encoding.ASCII.GetBytes("jp2 ")), 1);
            byte[] data = Join(Signature(), ftyp, Box("free", new byte[] { 0 }), header,
                Box("jp2c", Siz(), 1), Box("uuid", Box("jp2c", Siz()), 2));
            AssertHeader(ImgCodecsCv2.Identify(data), 4, 3, 8, 3);
            AssertHeader(ImgCodecsCv2.Identify(Jp2(Siz(), Header(extra: Box("bpcc", new byte[] { 7, 7, 7 })))), 4, 3, 8, 3);
        }

        [Fact]
        public void Jpeg2000HeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams()
        {
            foreach (byte[] data in new[] { Siz(), Jp2(Siz()), RealRgbJp2, RealRgbJ2k })
            {
                foreach (var limit in new[]
                {
                    new { Options = Options(data.Length - 1), Error = "input byte limit" },
                    new { Options = Options(data.Length, width: 3), Error = "dimensions" },
                    new { Options = Options(data.Length, height: 2), Error = "dimensions" },
                    new { Options = Options(data.Length, pixels: 11), Error = "pixel count" },
                    new { Options = Options(data.Length, depth: 7), Error = "bit depth" },
                    new { Options = Options(data.Length, channels: 2), Error = "channel count" }
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
                using (var stream = new MemoryStream(data))
                {
                    AssertHeader(ImgCodecsCv2.Identify(stream, Options(data.Length)), 4, 3, 8, 3);
                    Assert.Equal(0, stream.Position);
                }
                using (var stream = new ShortReadStream(data))
                {
                    AssertHeader(ImgCodecsCv2.Identify(stream, Options(data.Length)), 4, 3, 8, 3);
                    Assert.Equal(data.Length, stream.BytesRead);
                }
            }
        }

        [Fact]
        public void Jpeg2000StrictPolicyRejectsConflictingHeadersAndMixedPrecision()
        {
            foreach (byte[] data in new[] { Jp2(Siz(), Header(5)), Jp2(Siz(), Header(descriptor: 255)), Join(Jp2(Siz()), new byte[] { 0 }) })
            {
                AssertUnknown(ImgCodecsCv2.Identify(data));
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, Options(data.Length)));
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, data)))
                {
                    stream.Position = 1;
                    Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.Identify(stream, Options(data.Length)));
                    Assert.Equal(1, stream.Position);
                }
            }
            byte[] mixed = Siz(descriptors: new byte[] { 7, 11, 7 });
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(mixed, Options(mixed.Length)));
            using (var stream = new ShortReadStream(mixed))
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(stream, Options(mixed.Length, strict: false));
                Assert.True(result.IsSizeKnown); Assert.False(result.IsBitDepthKnown); Assert.Equal(3, result.ChannelCount);
            }
        }

        [Fact]
        public void IdentifyRequiresCompleteJpeg2000HeaderAndJp2BoxRanges()
        {
            foreach (byte[] data in new[] { Siz(), Jp2(Siz()), Jp2(Siz(), lengthForm: 1) })
            for (int length = 1; length < data.Length; ++length)
            {
                byte[] prefix = new byte[length]; Array.Copy(data, prefix, length);
                ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                Assert.False(result.IsSizeKnown); Assert.False(result.IsPixelFormatKnown);
                Assert.False(result.IsFrameCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown);
            }
            // Raw SIZ and LBox=0 headers can be complete before the compressed pixels arrive.
            // Neither complete headers nor complete box ranges prove a decodable image/frame.
            AssertHeader(ImgCodecsCv2.Identify(Join(Siz(), new byte[] { 255 })), 4, 3, 8, 3);
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusJpeg2000NeverEscapesOrPublishesConflictingHeaderFacts()
        {
            foreach (byte[] data in new[] { Siz(), Jp2(Siz()), Jp2(Siz(), lengthForm: 1), Jp2(Siz(), lengthForm: 2),
                RealRgbJp2, RealRgbJ2k, RealRgbaJp2, RealGray16Jp2, RealGray16J2k })
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            for (int offset = 0; offset < data.Length; ++offset)
            {
                if (data[offset] == replacement) continue;
                byte[] mutated = (byte[])data.Clone(); mutated[offset] = replacement;
                ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                if (result.Format != "jp2" && result.Format != "j2k") continue;
                Assert.Equal(mutated.Length, result.InputBytes);
                Assert.InRange(result.BytesInspected, 0, mutated.Length);
                Assert.False(result.IsFrameCountKnown); Assert.False(result.IsCumulativePixelCountKnown);
                Assert.False(result.IsEstimatedPixelBytesKnown);
                if (result.IsSizeKnown)
                {
                    Assert.InRange(result.Width, 1, int.MaxValue); Assert.InRange(result.Height, 1, int.MaxValue);
                    Assert.True(result.IsChannelCountKnown); Assert.InRange(result.ChannelCount, 1, 16384);
                }
                else { Assert.False(result.IsPixelFormatKnown); Assert.False(result.IsChannelCountKnown); }
                if (result.IsBitDepthKnown) Assert.InRange(result.BitDepth, 1, 38);
            }
        }

        [Theory]
        [InlineData(0, "jp2", 8, 3, 292, "4a03f4bb6385ad517bf71e7bef0809a9857b801203f267c269fd61051ecbede5")]
        [InlineData(1, "j2k", 8, 3, 207, "947754ff578d154d5fa7bd3501c41fd02fc7bed119eae3cbdbb1dffd6d11db22")]
        [InlineData(2, "jp2", 8, 4, 353, "73bd78bfaeb73f497ffe51973e4084abe00c58d4675753c69a79e5913eacd4a8")]
        [InlineData(3, "jp2", 16, 1, 229, "ef8595eb756ce9409d88e8fd8957146d62b8b57288e08f143652ddafdfae566a")]
        [InlineData(4, "j2k", 16, 1, 144, "1a492735a13eb7f3ce91c20806684cdc9c51ef7e02ffe6f51d49b1d7bd8f97ab")]
        public void IdentifyReadsRealLosslessJpeg2000Fixtures(int fixture, string format, int depth, int channels, int length, string sha256)
        {
            byte[] data = new[] { RealRgbJp2, RealRgbJ2k, RealRgbaJp2, RealGray16Jp2, RealGray16J2k }[fixture];
            Assert.Equal(length, data.Length);
            Assert.Equal(sha256, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)).ToLowerInvariant());
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            Assert.Equal(format, result.Format); AssertHeader(result, 4, 3, depth, channels);
        }

        private static ImageDecodeOptions Options(int length, int width = 4, int height = 3, long pixels = 12,
            int depth = 8, int channels = 3, bool strict = true)
        {
            return new ImageDecodeOptions(length, width, height, pixels, 1, true, true,
                long.MaxValue, long.MaxValue, false, false, long.MaxValue, depth, channels, strict);
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

        // Pillow 13.0.0 / OpenJPEG 2.5.4, lossless 4x3. Every decoded sample was
        // checked against the original generated RGB/RGBA/16-bit grayscale arrays.
        private static byte[] RealRgbJp2 { get { return Convert.FromBase64String("AAAADGpQICANCocKAAAAFGZ0eXBqcDIgAAAAAGpwMiAAAAAtanAyaAAAABZpaGRyAAAAAwAAAAQAAwcHAAAAAAAPY29scgEAAAAAABAAAADXanAyY/9P/1EALwAAAAAABAAAAAMAAAAAAAAAAAAAAAQAAAADAAAAAAAAAAAAAwcBAQcBAQcBAf9SAAwAAAABAAEEBAAB/1wAB0BASEhQ/2QAJQABQ3JlYXRlZCBieSBPcGVuSlBFRyB2ZXJzaW9uIDIuNS40/5AACgAAAAAAXAAB/5PPtBQJ3l7WUMfUCA4X/23H1AgI8Md3z8AaPtA4/ACADjSWJHmnC7avCD/PwBY+0Cn5gQAMTXvq3wNXC+rPwBZ+AFH4AQAAz9Hz9QWlDF//2Q=="); } }
        private static byte[] RealRgbJ2k { get { return Convert.FromBase64String("/0//UQAvAAAAAAAEAAAAAwAAAAAAAAAAAAAABAAAAAMAAAAAAAAAAAADBwEBBwEBBwEB/1IADAAAAAEAAQQEAAH/XAAHQEBISFD/ZAAlAAFDcmVhdGVkIGJ5IE9wZW5KUEVHIHZlcnNpb24gMi41LjT/kAAKAAAAAABcAAH/k8+0FAneXtZQx9QIDhf/bcfUCAjwx3fPwBo+0Dj8AIAONJYkeacLtq8IP8/AFj7QKfmBAAxNe+rfA1cL6s/AFn4AUfgBAADP0fP1BaUMX//Z"); } }
        private static byte[] RealRgbaJp2 { get { return Convert.FromBase64String("AAAADGpQICANCocKAAAAFGZ0eXBqcDIgAAAAAGpwMiAAAABPanAyaAAAABZpaGRyAAAAAwAAAAQABAcHAAAAAAAPY29scgEAAAAAABAAAAAiY2RlZgAEAAAAAAABAAEAAAACAAIAAAADAAMAAQAAAAAA8mpwMmP/T/9RADIAAAAAAAQAAAADAAAAAAAAAAAAAAAEAAAAAwAAAAAAAAAAAAQHAQEHAQEHAQEHAQH/UgAMAAAAAQABBAQAAf9cAAdAQEhIUP9kACUAAUNyZWF0ZWQgYnkgT3BlbkpQRUcgdmVyc2lvbiAyLjUuNP+QAAoAAAAAAHQAAf+Tw+cIBsqkt8fUCAeVjUTPtBgLfWLrkn/H1AoG6Zitv8/AFj7QGPwAgAQYHZx3BAEvz8AWPtAo/ACADAbu3P0JPwk/z8AWPtAo/ACADnGOYkcFhwg/z8AWPtAo/ACABmTy0UkChwk//9k="); } }
        private static byte[] RealGray16Jp2 { get { return Convert.FromBase64String("AAAADGpQICANCocKAAAAFGZ0eXBqcDIgAAAAAGpwMiAAAAAtanAyaAAAABZpaGRyAAAAAwAAAAQAAQ8HAAAAAAAPY29scgEAAAAAABEAAACYanAyY/9P/1EAKQAAAAAABAAAAAMAAAAAAAAAAAAAAAQAAAADAAAAAAAAAAAAAQ8BAf9SAAwAAAABAAEEBAAB/1wAB0CAiIiQ/2QAJQABQ3JlYXRlZCBieSBPcGVuSlBFRyB2ZXJzaW9uIDIuNS40/5AACgAAAAAAIwAB/5PP/DAoBkx7Wm6D03mHv8D/ADAM+EP/2Q=="); } }
        private static byte[] RealGray16J2k { get { return Convert.FromBase64String("/0//UQApAAAAAAAEAAAAAwAAAAAAAAAAAAAABAAAAAMAAAAAAAAAAAABDwEB/1IADAAAAAEAAQQEAAH/XAAHQICIiJD/ZAAlAAFDcmVhdGVkIGJ5IE9wZW5KUEVHIHZlcnNpb24gMi41LjT/kAAKAAAAAAAjAAH/k8/8MCgGTHtaboPTeYe/wP8AMAz4Q//Z"); } }

        private static void AssertHeader(ImageIdentifyResult result, int width, int height, int depth, int channels)
        {
            Assert.True(result.IsSizeKnown); Assert.Equal(width, result.Width); Assert.Equal(height, result.Height);
            Assert.True(result.IsPixelFormatKnown); Assert.Equal(depth, result.BitDepth); Assert.Equal(channels, result.ChannelCount);
            Assert.False(result.IsFrameCountKnown); Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        private static void AssertUnknown(ImageIdentifyResult result)
        {
            Assert.True(result.IsFormatKnown);
            Assert.False(result.IsSizeKnown);
            Assert.False(result.IsBitDepthKnown);
            Assert.False(result.IsChannelCountKnown);
            Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        private static byte[] Siz(int width = 4, int height = 3, byte[]? descriptors = null)
        {
            descriptors = descriptors ?? new byte[] { 7, 7, 7 };
            byte[] data = new byte[42 + descriptors.Length * 3];
            data[0] = 255; data[1] = 79; data[2] = 255; data[3] = 81;
            Put16(data, 4, 38 + descriptors.Length * 3);
            Put32(data, 8, (uint)width); Put32(data, 12, (uint)height);
            Put32(data, 24, (uint)width); Put32(data, 28, (uint)height);
            Put16(data, 40, descriptors.Length);
            for (int i = 0; i < descriptors.Length; ++i)
            {
                data[42 + 3 * i] = descriptors[i];
                data[43 + 3 * i] = data[44 + 3 * i] = 1;
            }
            return data;
        }

        private static byte[] Header(int width = 4, int height = 3, int channels = 3, int descriptor = 7, byte[]? extra = null)
        {
            byte[] ihdr = new byte[14];
            Put32(ihdr, 0, (uint)height); Put32(ihdr, 4, (uint)width);
            Put16(ihdr, 8, channels); ihdr[10] = (byte)descriptor; ihdr[11] = 7;
            return Box("jp2h", Join(Box("ihdr", ihdr), extra ?? Array.Empty<byte>()));
        }

        private static byte[] Jp2(byte[] codestream, byte[]? header = null, int lengthForm = 0)
        {
            return Top(header ?? Header(), Box("jp2c", codestream, lengthForm));
        }

        private static byte[] Top(params byte[][] boxes) { return Join(Signature(), FileType(), Join(boxes)); }
        private static byte[] Signature() { return Box("jP  ", new byte[] { 13, 10, 135, 10 }); }
        private static byte[] FileType() { return Box("ftyp", Join(Encoding.ASCII.GetBytes("jp2 "), new byte[4], Encoding.ASCII.GetBytes("jp2 "))); }
        private static byte[] Payload(byte[] box) { byte[] value = new byte[box.Length - 8]; Array.Copy(box, 8, value, 0, value.Length); return value; }

        private static byte[] Box(string type, byte[] body, int lengthForm = 0)
        {
            int header = lengthForm == 1 ? 16 : 8;
            byte[] box = new byte[header + body.Length];
            Put32(box, 0, lengthForm == 1 ? 1U : lengthForm == 2 ? 0U : (uint)box.Length);
            Encoding.ASCII.GetBytes(type).CopyTo(box, 4);
            if (lengthForm == 1) Put32(box, 12, (uint)box.Length);
            body.CopyTo(box, header);
            return box;
        }

        private static byte[] Join(params byte[][] parts)
        {
            using (var stream = new MemoryStream())
            {
                foreach (byte[] part in parts) stream.Write(part, 0, part.Length);
                return stream.ToArray();
            }
        }

        private static void Put16(byte[] data, int offset, int value)
        {
            data[offset] = (byte)(value >> 8); data[offset + 1] = (byte)value;
        }

        private static void Put32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24); data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8); data[offset + 3] = (byte)value;
        }
    }
}
