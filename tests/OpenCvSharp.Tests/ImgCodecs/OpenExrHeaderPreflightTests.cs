using System;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class OpenExrHeaderPreflightTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void IdentifyRejectsDuplicateOpenExrChannelNames(int pixelType)
        {
            AssertUnknown(ImgCodecsCv2.Identify(Exr(Window(), Channels(C("R", pixelType), C("R", pixelType)))));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyNeverIgnoresWrongTypedOpenExrChannels(bool wrongFirst)
        {
            byte[] wrong = Attribute("channels", "string", new byte[] { 42 });
            byte[] correct = Channels(C("R"), C("G"), C("B"));
            AssertUnknown(ImgCodecsCv2.Identify(Exr(Window(), wrongFirst ? wrong : correct, wrongFirst ? correct : wrong)));
        }

        [Theory]
        [InlineData(0x100)]
        [InlineData(0x2000)]
        [InlineData(int.MinValue)]
        public void IdentifyRejectsUnsupportedOpenExrVersionFlags(int flags)
        {
            byte[] data = Exr(); Put32(data, 4, 2 | flags);
            AssertUnknown(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void IdentifyRejectsOpenExrSamplingThatConflictsWithDataWindow(int failure)
        {
            byte[] window = failure == 0 ? Window(1, 0, 4, 3) : failure == 1 ? Window(0, -1, 3, 2) :
                failure == 2 ? Window(0, 0, 2, 3) : Window(0, 0, 3, 2);
            AssertUnknown(ImgCodecsCv2.Identify(Exr(window, Channels(C("R", 1, 2, 2)))));
        }

        [Theory]
        [InlineData(0, 32)]
        [InlineData(1, 16)]
        [InlineData(2, 32)]
        public void IdentifyReadsUniqueOpenExrChannelsInAnyOrder(int type, int depth)
        {
            foreach (int version in new[] { 1, 2 })
            foreach (int flags in new[] { 0, 0x200, 0x400, 0x800, 0x1000 })
            {
                byte[] data = Exr(Channels(C("R", type), C("B", type), C("G", type)),
                    Attribute("custom", "opaque", new byte[] { 1, 2, 3 }), Window());
                Put32(data, 4, version | flags);
                AssertHeader(ImgCodecsCv2.Identify(data), 4, 3, depth, 3);
            }
        }

        [Fact]
        public void IdentifyPreservesOpenExrMixedStorageAndDistinctByteNames()
        {
            AssertHeader(ImgCodecsCv2.Identify(Exr(Window(), Channels(C("A", 0), C("a", 2)))), 4, 3, 32, 2);
            AssertHeader(ImgCodecsCv2.Identify(Exr(Window(), Channels(C("R", 1), C("G", 2)))), 4, 3, 0, 2);
            Channel first = C("placeholder"), second = C("placeholder");
            first.Name = new byte[] { 255 }; second.Name = new byte[] { 254 };
            AssertHeader(ImgCodecsCv2.Identify(Exr(Window(), Channels(first, second))), 4, 3, 16, 2);
            AssertUnknown(ImgCodecsCv2.Identify(Exr(Window(), Channels(first, second, first))));
        }

        [Theory]
        [InlineData("attribute")]
        [InlineData("type")]
        [InlineData("channel")]
        public void IdentifyBoundsOpenExrNameFieldsByActualTerminatedBytes(string field)
        {
            foreach (int length in new[] { 1, 31, 32, 255, 256 })
            {
                string name = new string('x', length);
                byte[] data = field == "attribute" ? Exr(Window(), Channels(C("R")), Attribute(name, "opaque", new byte[1])) :
                    field == "type" ? Exr(Window(), Channels(C("R")), Attribute("custom", name, new byte[1])) :
                    Exr(Window(), Channels(C(name)));
                if (length <= 255) AssertHeader(ImgCodecsCv2.Identify(data), 4, 3, 16, 1);
                else AssertUnknown(ImgCodecsCv2.Identify(data));
            }
        }

        [Theory]
        [InlineData("missing-window")]
        [InlineData("wrong-window-type")]
        [InlineData("duplicate-window")]
        [InlineData("wrong-channels-type")]
        [InlineData("empty-channels")]
        [InlineData("short-channel-record")]
        [InlineData("zero-x-sampling")]
        [InlineData("negative-y-sampling")]
        [InlineData("invalid-pixel-type")]
        [InlineData("negative-attribute-size")]
        [InlineData("out-of-range-attribute")]
        [InlineData("unterminated-name")]
        [InlineData("unterminated-type")]
        [InlineData("channel-list-suffix")]
        [InlineData("duplicate-channels")]
        [InlineData("duplicate-part-type")]
        public void IdentifyRejectsAmbiguousOrUnboundedOpenExrHeaders(string failure)
        {
            byte[] channelList = Channels(C("R")), data;
            switch (failure)
            {
                case "missing-window": data = Exr(channelList); break;
                case "wrong-window-type": data = Exr(Attribute("dataWindow", "string", new byte[16]), channelList); break;
                case "duplicate-window": data = Exr(Window(), channelList, Window()); break;
                case "wrong-channels-type": data = Exr(Window(), Attribute("channels", "opaque", new byte[] { 0 })); break;
                case "empty-channels": data = Exr(Window(), Channels()); break;
                case "short-channel-record": data = Exr(Window(), Attribute("channels", "chlist", new byte[] { (byte)'R', 0, 1, 0 })); break;
                case "zero-x-sampling": data = Exr(Window(), Channels(C("R", x: 0))); break;
                case "negative-y-sampling": data = Exr(Window(), Channels(C("R", y: -1))); break;
                case "invalid-pixel-type": data = Exr(Window(), Channels(C("R", type: 3))); break;
                case "negative-attribute-size": Put32(channelList, 16, -1); data = Exr(Window(), channelList); break;
                case "out-of-range-attribute": Put32(channelList, 16, int.MaxValue); data = Exr(Window(), channelList); break;
                case "unterminated-name": data = Exr(Window(), channelList); Array.Resize(ref data, data.Length - 1); data = Join(data, Encoding.ASCII.GetBytes("name")); break;
                case "unterminated-type": data = Exr(Window(), channelList); Array.Resize(ref data, data.Length - 1); data = Join(data, Encoding.ASCII.GetBytes("name\0type")); break;
                case "channel-list-suffix": Put32(channelList, 16, channelList.Length - 20 + 1); data = Exr(Window(), Join(channelList, new byte[] { 42 })); break;
                case "duplicate-channels": data = Exr(Window(), channelList, channelList); break;
                case "duplicate-part-type": data = Exr(Window(), channelList, Attribute("type", "string", Encoding.ASCII.GetBytes("scanlineimage")), Attribute("type", "string", Encoding.ASCII.GetBytes("scanlineimage"))); break;
                default: throw new ArgumentException(failure);
            }
            AssertUnknown(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData("tiledimage")]
        [InlineData("deepscanline")]
        [InlineData("deeptile")]
        public void IdentifyChecksSamplingOfTheExplicitFirstOpenExrPart(string kind)
        {
            foreach (int flags in new[] { 0, 0x1800 })
            {
                byte[] type = Attribute("type", "string", Encoding.ASCII.GetBytes(kind));
                byte[] data = Exr(type, Channels(C("R", 1, 2, 2)), Window(0, 0, 3, 3)); Put32(data, 4, flags | 2);
                AssertUnknown(ImgCodecsCv2.Identify(data));
                data = Exr(type, Channels(C("R")), Window()); Put32(data, 4, flags | 2);
                AssertHeader(ImgCodecsCv2.Identify(data), 4, 3, 16, 1);
            }
        }

        [Fact]
        public void IdentifyDoesNotApplyLaterOpenExrDeepPartsToTheFirstScanlinePart()
        {
            byte[] data = Exr(Window(0, 0, 3, 3), Channels(C("R", 1, 2, 2)),
                Attribute("type", "string", Encoding.ASCII.GetBytes("scanlineimage")));
            Put32(data, 4, 0x1802);
            AssertHeader(ImgCodecsCv2.Identify(data), 4, 4, 16, 1);
            foreach (int flags in new[] { 0x200, 0x800 })
            {
                data = Exr(Window(0, 0, 3, 3), Channels(C("R", 1, 2, 2))); Put32(data, 4, flags | 2);
                AssertUnknown(ImgCodecsCv2.Identify(data));
            }
        }

        [Fact]
        public void IdentifyBoundsOpenExrGeometryAndExplicitChannelCount()
        {
            AssertHeader(ImgCodecsCv2.Identify(Exr(Window(int.MinValue, int.MinValue, -2, -2), Channels(C("R")))), int.MaxValue, int.MaxValue, 16, 1);
            AssertUnknown(ImgCodecsCv2.Identify(Exr(Window(int.MinValue, 0, -1, 2), Channels(C("R")))));
            AssertUnknown(ImgCodecsCv2.Identify(Exr(Window(5, 0, 4, 2), Channels(C("R")))));
            Channel[] channels = new Channel[65535];
            for (int index = 0; index < channels.Length; ++index) channels[index] = C("channel" + index);
            AssertHeader(ImgCodecsCv2.Identify(Exr(Window(), Channels(channels))), 4, 3, 16, 65535);
            Array.Resize(ref channels, 65536); channels[65535] = C("last");
            AssertUnknown(ImgCodecsCv2.Identify(Exr(Window(), Channels(channels))));
        }

        [Fact]
        public void OpenExrHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams()
        {
            foreach (byte[] data in new[] { Exr(), RealHalfRgb, RealTiled, RealMultipart })
            {
                foreach (var limit in new[]
                {
                    new { Options = Options(data.Length - 1), Error = "input byte limit" },
                    new { Options = Options(data.Length, width: 3), Error = "dimensions" },
                    new { Options = Options(data.Length, height: 2), Error = "dimensions" },
                    new { Options = Options(data.Length, pixels: 11), Error = "pixel count" },
                    new { Options = Options(data.Length, depth: 15), Error = "bit depth" },
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
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, data)))
                {
                    stream.Position = 1;
                    AssertHeader(ImgCodecsCv2.Identify(stream, Options(data.Length)), 4, 3, 16, 3);
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadStream(data))
                {
                    AssertHeader(ImgCodecsCv2.Identify(stream, Options(data.Length)), 4, 3, 16, 3);
                    Assert.Equal(data.Length, stream.BytesRead);
                }
            }
        }

        [Fact]
        public void OpenExrStrictPolicyRejectsConflictingOrUnknownPixelFacts()
        {
            byte[] missing = Exr(Window());
            ImageIdentifyResult geometry = ImgCodecsCv2.Identify(missing);
            Assert.True(geometry.IsSizeKnown); Assert.Equal(4, geometry.Width); Assert.Equal(3, geometry.Height);
            Assert.False(geometry.IsBitDepthKnown); Assert.False(geometry.IsChannelCountKnown);
            foreach (byte[] data in new[] { missing, RealMixed, Exr(Window(), Channels(C("R"), C("R"))),
                Exr(Window(), Channels(C("R", x: 2, y: 2))) })
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, Options(data.Length, depth: 32)));
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, data)))
                {
                    stream.Position = 1;
                    Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.Identify(stream, Options(data.Length, depth: 32)));
                    Assert.Equal(1, stream.Position);
                }
            }
            using (var stream = new ShortReadStream(RealMixed))
            {
                AssertHeader(ImgCodecsCv2.Identify(stream, Options(RealMixed.Length, strict: false)), 4, 3, 0, 2);
            }
        }

        [Fact]
        public void IdentifyRequiresCompleteFirstOpenExrHeaderBeforePublishingFacts()
        {
            byte[][] fixtures = RealFixtures;
            int[] headerEnds = RealHeaderEnds;
            for (int fixture = -1; fixture < fixtures.Length; ++fixture)
            {
                byte[] data = fixture < 0 ? Exr() : fixtures[fixture];
                int end = fixture < 0 ? data.Length : headerEnds[fixture];
                for (int length = 1; length < end; ++length)
                {
                    byte[] prefix = new byte[length]; Array.Copy(data, prefix, length);
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                    Assert.False(result.IsSizeKnown); Assert.False(result.IsPixelFormatKnown);
                    Assert.False(result.IsFrameCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown);
                }
                // The first header can prove its own geometry/storage without any later
                // part header, offset table or compressed chunk. It cannot prove a frame.
                byte[] complete = new byte[end]; Array.Copy(data, complete, end);
                Assert.True(ImgCodecsCv2.Identify(complete).IsSizeKnown);
                Assert.False(ImgCodecsCv2.Identify(complete).IsFrameCountKnown);
            }
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusOpenExrNeverEscapesOrPublishesConflictingHeaderFacts()
        {
            foreach (byte[] data in RealFixtures)
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            for (int offset = 0; offset < data.Length; ++offset)
            {
                if (data[offset] == replacement) continue;
                byte[] mutated = (byte[])data.Clone(); mutated[offset] = replacement;
                ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                if (result.Format != "exr") continue;
                Assert.Equal(mutated.Length, result.InputBytes);
                Assert.InRange(result.BytesInspected, 0, mutated.Length);
                Assert.False(result.IsFrameCountKnown); Assert.False(result.IsCumulativePixelCountKnown);
                Assert.False(result.IsEstimatedPixelBytesKnown);
                if (result.IsSizeKnown)
                {
                    Assert.InRange(result.Width, 1, int.MaxValue); Assert.InRange(result.Height, 1, int.MaxValue);
                }
                else { Assert.False(result.IsPixelFormatKnown); Assert.False(result.IsChannelCountKnown); }
                if (result.IsChannelCountKnown) Assert.InRange(result.ChannelCount, 1, 65535);
                if (result.IsBitDepthKnown) Assert.Contains(result.BitDepth, new[] { 16, 32 });
            }
        }

        [Theory]
        [InlineData(0, 16, 3, 413, "418eae47d738e9c910291eab803563ba67b0be6199d2a7e1b239cab5e9e809a5")]
        [InlineData(1, 32, 4, 600, "0728e5b9df3c7146a07003286c5105203feb69745ead7dced51258274f957dfd")]
        [InlineData(2, 32, 1, 402, "15826245ca4d6ba19e8752b8402651dbaa5d7a45803a551c0540d0bff9fa18d8")]
        [InlineData(3, 0, 2, 444, "ebfb7c560134d5f20d5f1d278a5a44a11163359ee904c909920424d9cc5be84b")]
        [InlineData(4, 16, 3, 450, "4d88047bb4a66a83e38a48f5030ddbf4a861e36bbb670a7329b60639eb97eaf7")]
        [InlineData(5, 16, 3, 901, "83f0e8dbbd74d2bd68c38f865ce33507579d631d869a77f44ba7228fcb14af80")]
        [InlineData(6, 32, 1, 600, "e3a723d2822287e3bab41c136199c0acf33a338482ce782b34612594ae9691d6")]
        public void IdentifyReadsRealOpenExrFirstHeaderFixtures(int fixture, int depth, int channels, int length, string sha256)
        {
            byte[] data = RealFixtures[fixture];
            Assert.Equal(length, data.Length);
            Assert.Equal(sha256, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)).ToLowerInvariant());
            AssertHeader(ImgCodecsCv2.Identify(data), 4, 3, depth, channels);
        }

        private static ImageDecodeOptions Options(int length, int width = 4, int height = 3, long pixels = 12,
            int depth = 16, int channels = 3, bool strict = true)
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

        private static void AssertHeader(ImageIdentifyResult result, int width, int height, int depth, int channels)
        {
            Assert.Equal("exr", result.Format); Assert.True(result.IsSizeKnown);
            Assert.Equal(width, result.Width); Assert.Equal(height, result.Height);
            Assert.True(result.IsChannelCountKnown); Assert.Equal(channels, result.ChannelCount);
            Assert.Equal(depth != 0, result.IsBitDepthKnown); Assert.Equal(depth, result.BitDepth);
            Assert.False(result.IsFrameCountKnown); Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        private static void AssertUnknown(ImageIdentifyResult result)
        {
            Assert.Equal("exr", result.Format);
            Assert.False(result.IsSizeKnown); Assert.False(result.IsBitDepthKnown);
            Assert.False(result.IsChannelCountKnown); Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        private struct Channel
        {
            public byte[] Name;
            public int Type;
            public int X;
            public int Y;
        }

        private static Channel C(string name, int type = 1, int x = 1, int y = 1)
        {
            return new Channel { Name = Encoding.UTF8.GetBytes(name), Type = type, X = x, Y = y };
        }

        private static byte[] Exr(params byte[][] attributes)
        {
            if (attributes.Length == 0) attributes = new[] { Window(), Channels(C("R"), C("G"), C("B")) };
            return Join(new byte[] { 0x76, 0x2f, 0x31, 1, 2, 0, 0, 0 }, Join(attributes), new byte[] { 0 });
        }

        private static byte[] Window(int minX = 0, int minY = 0, int maxX = 3, int maxY = 2)
        {
            byte[] value = new byte[16];
            Put32(value, 0, minX); Put32(value, 4, minY); Put32(value, 8, maxX); Put32(value, 12, maxY);
            return Attribute("dataWindow", "box2i", value);
        }

        private static byte[] Channels(params Channel[] channels)
        {
            using (var stream = new MemoryStream())
            {
                foreach (Channel channel in channels)
                {
                    stream.Write(channel.Name, 0, channel.Name.Length); stream.WriteByte(0);
                    byte[] record = new byte[16]; Put32(record, 0, channel.Type);
                    Put32(record, 8, channel.X); Put32(record, 12, channel.Y);
                    stream.Write(record, 0, record.Length);
                }
                stream.WriteByte(0);
                return Attribute("channels", "chlist", stream.ToArray());
            }
        }

        private static byte[] Attribute(string name, string type, byte[] value)
        {
            byte[] size = new byte[4]; Put32(size, 0, value.Length);
            return Join(Encoding.UTF8.GetBytes(name + "\0" + type + "\0"), size, value);
        }

        private static byte[] Join(params byte[][] parts)
        {
            using (var stream = new MemoryStream())
            {
                foreach (byte[] part in parts) stream.Write(part, 0, part.Length);
                return stream.ToArray();
            }
        }

        // OpenEXR 3.5.4 / NumPy 2.3.5: every part/channel sample was independently
        // read back and compared with the original arrays, including deep samples.
        // These fixtures require no Python package or native decoder in the tests.
        private static byte[] RealHalfRgb { get { return Convert.FromBase64String("di8xAQIAAABjaGFubmVscwBjaGxpc3QANwAAAEIAAQAAAAAAAAABAAAAAQAAAEcAAQAAAAAAAAABAAAAAQAAAFIAAQAAAAAAAAABAAAAAQAAAABjb21wcmVzc2lvbgBjb21wcmVzc2lvbgABAAAAA2RhdGFXaW5kb3cAYm94MmkAEAAAAAAAAAAAAAAAAwAAAAIAAABkaXNwbGF5V2luZG93AGJveDJpABAAAAAAAAAAAAAAAAMAAAACAAAAbGluZU9yZGVyAGxpbmVPcmRlcgABAAAAAHBpeGVsQXNwZWN0UmF0aW8AZmxvYXQABAAAAAAAgD9zY3JlZW5XaW5kb3dDZW50ZXIAdjJmAAgAAAAAAAAAAAAAAHNjcmVlbldpbmRvd1dpZHRoAGZsb2F0AAQAAAAAAIA/dHlwZQBzdHJpbmcADQAAAHNjYW5saW5laW1hZ2UAXgEAAAAAAAAAAAAANwAAAHheY1iwYIHCASBoaGhgALIXQNkNMHEGBoYAILcGiE32tDT1AelaIC5vbGzsgrKrGhobAF6pJMo="); } }
        private static byte[] RealFloatRgba { get { return Convert.FromBase64String("di8xAQIAAABjaGFubmVscwBjaGxpc3QASQAAAEEAAgAAAAAAAAABAAAAAQAAAEIAAgAAAAAAAAABAAAAAQAAAEcAAgAAAAAAAAABAAAAAQAAAFIAAgAAAAAAAAABAAAAAQAAAABjb21wcmVzc2lvbgBjb21wcmVzc2lvbgABAAAAAGRhdGFXaW5kb3cAYm94MmkAEAAAAPz////+/////////wAAAABkaXNwbGF5V2luZG93AGJveDJpABAAAAD8/////v////////8AAAAAbGluZU9yZGVyAGxpbmVPcmRlcgABAAAAAHBpeGVsQXNwZWN0UmF0aW8AZmxvYXQABAAAAAAAgD9zY3JlZW5XaW5kb3dDZW50ZXIAdjJmAAgAAAAAAAAAAAAAAHNjcmVlbldpbmRvd1dpZHRoAGZsb2F0AAQAAAAAAIA/dHlwZQBzdHJpbmcADQAAAHNjYW5saW5laW1hZ2UAgAEAAAAAAADIAQAAAAAAABACAAAAAAAA/v///0AAAAAAAMBCAADCQgAAxEIAAMZCAACAQgAAgkIAAIRCAACGQgAAAEIAAARCAAAIQgAADEIAAAAAAACAPwAAAEAAAEBA/////0AAAAAAAMhCAADKQgAAzEIAAM5CAACIQgAAikIAAIxCAACOQgAAEEIAABRCAAAYQgAAHEIAAIBAAACgQAAAwEAAAOBAAAAAAEAAAAAAANBCAADSQgAA1EIAANZCAACQQgAAkkIAAJRCAACWQgAAIEIAACRCAAAoQgAALEIAAABBAAAQQQAAIEEAADBB"); } }
        private static byte[] RealUintZ { get { return Convert.FromBase64String("di8xAQIAAABjaGFubmVscwBjaGxpc3QAEwAAAFoAAAAAAAAAAAABAAAAAQAAAABjb21wcmVzc2lvbgBjb21wcmVzc2lvbgABAAAAAmRhdGFXaW5kb3cAYm94MmkAEAAAAAAAAAAAAAAAAwAAAAIAAABkaXNwbGF5V2luZG93AGJveDJpABAAAAAAAAAAAAAAAAMAAAACAAAAbGluZU9yZGVyAGxpbmVPcmRlcgABAAAAAHBpeGVsQXNwZWN0UmF0aW8AZmxvYXQABAAAAAAAgD9zY3JlZW5XaW5kb3dDZW50ZXIAdjJmAAgAAAAAAAAAAAAAAHNjcmVlbldpbmRvd1dpZHRoAGZsb2F0AAQAAAAAAIA/dHlwZQBzdHJpbmcADQAAAHNjYW5saW5laW1hZ2UASgEAAAAAAABiAQAAAAAAAHoBAAAAAAAAAAAAABAAAAAAAAAAh9YSAA6tJQCVgzgAAQAAABAAAAAcWksAozBeACoHcQCx3YMAAgAAABAAAAA4tJYAv4qpAEZhvADNN88A"); } }
        private static byte[] RealMixed { get { return Convert.FromBase64String("di8xAQIAAABjaGFubmVscwBjaGxpc3QAJQAAAEcAAgAAAAAAAAABAAAAAQAAAFIAAQAAAAAAAAABAAAAAQAAAABjb21wcmVzc2lvbgBjb21wcmVzc2lvbgABAAAAAGRhdGFXaW5kb3cAYm94MmkAEAAAAAAAAAAAAAAAAwAAAAIAAABkaXNwbGF5V2luZG93AGJveDJpABAAAAAAAAAAAAAAAAMAAAACAAAAbGluZU9yZGVyAGxpbmVPcmRlcgABAAAAAHBpeGVsQXNwZWN0UmF0aW8AZmxvYXQABAAAAAAAgD9zY3JlZW5XaW5kb3dDZW50ZXIAdjJmAAgAAAAAAAAAAAAAAHNjcmVlbldpbmRvd1dpZHRoAGZsb2F0AAQAAAAAAIA/dHlwZQBzdHJpbmcADQAAAHNjYW5saW5laW1hZ2UAXAEAAAAAAAB8AQAAAAAAAJwBAAAAAAAAAAAAABgAAAAAAABCAAAEQgAACEIAAAxCAAAAPABAAEIBAAAAGAAAAAAAEEIAABRCAAAYQgAAHEIARABFAEYARwIAAAAYAAAAAAAgQgAAJEIAAChCAAAsQgBIgEgASYBJ"); } }
        private static byte[] RealTiled { get { return Convert.FromBase64String("di8xAQICAABjaGFubmVscwBjaGxpc3QANwAAAEIAAQAAAAAAAAABAAAAAQAAAEcAAQAAAAAAAAABAAAAAQAAAFIAAQAAAAAAAAABAAAAAQAAAABjb21wcmVzc2lvbgBjb21wcmVzc2lvbgABAAAAA2RhdGFXaW5kb3cAYm94MmkAEAAAAAAAAAAAAAAAAwAAAAIAAABkaXNwbGF5V2luZG93AGJveDJpABAAAAAAAAAAAAAAAAMAAAACAAAAbGluZU9yZGVyAGxpbmVPcmRlcgABAAAAAHBpeGVsQXNwZWN0UmF0aW8AZmxvYXQABAAAAAAAgD9zY3JlZW5XaW5kb3dDZW50ZXIAdjJmAAgAAAAAAAAAAAAAAHNjcmVlbldpbmRvd1dpZHRoAGZsb2F0AAQAAAAAAIA/dGlsZXMAdGlsZWRlc2MACQAAACAAAAAgAAAAAHR5cGUAc3RyaW5nAAoAAAB0aWxlZGltYWdlAHcBAAAAAAAAAAAAAAAAAAAAAAAAAAAAADcAAAB4XmNYsGCBwgEgaGhoYACyF0DZDTBxBgaGACC3BohN9rQ09QHpWiAub2xs7IKyqxoaGwBeqSTK"); } }
        private static byte[] RealMultipart { get { return Convert.FromBase64String("di8xAQIQAABjaGFubmVscwBjaGxpc3QANwAAAEIAAQAAAAAAAAABAAAAAQAAAEcAAQAAAAAAAAABAAAAAQAAAFIAAQAAAAAAAAABAAAAAQAAAABjaHVua0NvdW50AGludAAEAAAAAQAAAGNvbXByZXNzaW9uAGNvbXByZXNzaW9uAAEAAAADZGF0YVdpbmRvdwBib3gyaQAQAAAAAAAAAAAAAAADAAAAAgAAAGRpc3BsYXlXaW5kb3cAYm94MmkAEAAAAAAAAAAAAAAABQAAAAQAAABsaW5lT3JkZXIAbGluZU9yZGVyAAEAAAAAbmFtZQBzdHJpbmcABQAAAGZpcnN0cGl4ZWxBc3BlY3RSYXRpbwBmbG9hdAAEAAAAAACAP3NjcmVlbldpbmRvd0NlbnRlcgB2MmYACAAAAAAAAAAAAAAAc2NyZWVuV2luZG93V2lkdGgAZmxvYXQABAAAAAAAgD90eXBlAHN0cmluZwANAAAAc2NhbmxpbmVpbWFnZQBjaGFubmVscwBjaGxpc3QAEwAAAFkAAgAAAAAAAAABAAAAAQAAAABjaHVua0NvdW50AGludAAEAAAAAQAAAGNvbXByZXNzaW9uAGNvbXByZXNzaW9uAAEAAAADZGF0YVdpbmRvdwBib3gyaQAQAAAAAAAAAAAAAAAFAAAABAAAAGRpc3BsYXlXaW5kb3cAYm94MmkAEAAAAAAAAAAAAAAABQAAAAQAAABsaW5lT3JkZXIAbGluZU9yZGVyAAEAAAAAbmFtZQBzdHJpbmcABgAAAHNlY29uZHBpeGVsQXNwZWN0UmF0aW8AZmxvYXQABAAAAAAAgD9zY3JlZW5XaW5kb3dDZW50ZXIAdjJmAAgAAAAAAAAAAAAAAHNjcmVlbldpbmRvd1dpZHRoAGZsb2F0AAQAAAAAAIA/dHlwZQBzdHJpbmcADQAAAHNjYW5saW5laW1hZ2UAAOoCAAAAAAAALQMAAAAAAAAAAAAAAAAAADcAAAB4XmNYsGCBwgEgaGhoYACyF0DZDTBxBgaGACC3BohN9rQ09QHpWiAub2xs7IKyqxoaGwBeqSTKAQAAAAAAAABMAAAAeF5jWBGxJmRDwBafHR57XA44HLE5YXHG5ILBFZ0bGndUHig8kXkh8Ubkg8AXnh8cf1gYGJj+sfxh+8Xxg+sbzxe+T4fsyIYAtDs6ww=="); } }
        private static byte[] RealDeep { get { return Convert.FromBase64String("di8xAQIIAABjaGFubmVscwBjaGxpc3QAEwAAAFoAAgAAAAAAAAABAAAAAQAAAABjaHVua0NvdW50AGludAAEAAAAAwAAAGNvbXByZXNzaW9uAGNvbXByZXNzaW9uAAEAAAACZGF0YVdpbmRvdwBib3gyaQAQAAAAAAAAAAAAAAADAAAAAgAAAGRpc3BsYXlXaW5kb3cAYm94MmkAEAAAAAAAAAAAAAAAAwAAAAIAAABsaW5lT3JkZXIAbGluZU9yZGVyAAEAAAAAcGl4ZWxBc3BlY3RSYXRpbwBmbG9hdAAEAAAAAACAP3NjcmVlbldpbmRvd0NlbnRlcgB2MmYACAAAAAAAAAAAAAAAc2NyZWVuV2luZG93V2lkdGgAZmxvYXQABAAAAAAAgD90eXBlAHN0cmluZwAMAAAAZGVlcHNjYW5saW5ldmVyc2lvbgBpbnQABAAAAAEAAAAAdAEAAAAAAADAAQAAAAAAAAwCAAAAAAAAAAAAABAAAAAAAAAAIAAAAAAAAAAgAAAAAAAAAAIAAAAEAAAABgAAAAgAAAAAAAAAAAAAPwAAgD8AAMA/AAAAQAAAIEAAAEBAAABgQAEAAAAQAAAAAAAAACAAAAAAAAAAIAAAAAAAAAACAAAABAAAAAYAAAAIAAAAAACAPwAAwD8AAABAAAAgQAAAQEAAAGBAAACAQAAAkEACAAAAEAAAAAAAAAAgAAAAAAAAACAAAAAAAAAAAgAAAAQAAAAGAAAACAAAAAAAAEAAACBAAABAQAAAYEAAAIBAAACQQAAAoEAAALBA"); } }
        private static byte[][] RealFixtures { get { return new[] { RealHalfRgb, RealFloatRgba, RealUintZ, RealMixed, RealTiled, RealMultipart, RealDeep }; } }
        private static int[] RealHeaderEnds { get { return new[] { 342, 360, 306, 324, 367, 386, 348 }; } }

        private static void Put32(byte[] data, int offset, int value)
        {
            data[offset] = (byte)value; data[offset + 1] = (byte)(value >> 8);
            data[offset + 2] = (byte)(value >> 16); data[offset + 3] = (byte)(value >> 24);
        }
    }
}
