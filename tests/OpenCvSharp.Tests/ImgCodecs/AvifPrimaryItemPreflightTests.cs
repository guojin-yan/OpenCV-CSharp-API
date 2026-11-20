using System;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class AvifPrimaryItemPreflightTests
    {
        // Generated from a solid RGB (240,120,60) 2x3 image with Pillow 13.0.0, AVIF quality=95.
        // SHA256: 0632e166f06b0358e15b2cd202881d379e0ee761d01263686604e6b2585856ad.
        private const string EncodedAvif =
            "AAAAIGZ0eXBhdmlmAAAAAGF2aWZtaWYxbWlhZk1BMUIAAADrbWV0YQAAAAAAAAAhaGRscgAAAAAAAAAAcGljdAAAAAAAAAAAAAAAAAAAAAAOcGl0bQAAAAAAAQAAAB5pbG9jAAAAAEQAAAEAAQAAAAEAAAETAAAAKAAAAChpaW5mAAAAAAABAAAAGmluZmUCAAAAAAEAAGF2MDFDb2xvcgAAAABqaXBycAAAAEtpcGNvAAAAFGlzcGUAAAAAAAAAAgAAAAMAAAAQcGl4aQAAAAADCAgIAAAADGF2MUOBAAwAAAAAE2NvbHJuY2x4AAEADQAGgAAAABdpcG1hAAAAAAAAAAEAAQQBAoMEAAAAMG1kYXQSAAoIGABzRAQ0GhAyGhEBqkAAeQAA/1rLPao6IALeZ+x1Grp87yiA";

        [Fact]
        public void IdentifyReadsAnActualEncodedAvifWithoutNativeRuntime()
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Convert.FromBase64String(EncodedAvif));
            AssertPrimaryFacts(result, 2, 3, 8, 3);
            Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsMetadataSizeKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        [Fact]
        public void IdentifyDoesNotUseUnassociatedAvifProperties()
        {
            byte[] avif = Container(Pitm(1), Box("iprp", Box("ipco", Ispe(640, 480), Pixi(10, 3))));

            ImageIdentifyResult result = ImgCodecsCv2.Identify(avif);

            Assert.Equal("avif", result.Format);
            Assert.False(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown);
        }

        [Fact]
        public void IdentifyUsesOnlyPropertiesOfThePrimaryAvifItem()
        {
            byte[] avif = Container(
                Box("iprp", Box("ipco", Ispe(32, 24), Pixi(8, 1), Ispe(640, 480), Pixi(10, 3)),
                    Ipma(0, false, Entry(1, 0, false, 0x83, 4), Entry(2, 0, false, 0x81, 2))),
                Pitm(1));

            ImageIdentifyResult result = ImgCodecsCv2.Identify(avif);

            Assert.True(result.IsSizeKnown);
            Assert.Equal(640, result.Width);
            Assert.Equal(480, result.Height);
            Assert.True(result.IsPixelFormatKnown);
            Assert.Equal(10, result.BitDepth);
            Assert.Equal(3, result.ChannelCount);
            Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsCumulativePixelCountKnown);
        }

        [Theory]
        [InlineData(0, 0, false, 1u)]
        [InlineData(0, 1, true, 65535u)]
        [InlineData(1, 0, true, 42u)]
        [InlineData(1, 1, false, 70000u)]
        [InlineData(1, 1, true, uint.MaxValue)]
        public void IdentifyReadsAvifItemIdAndPropertyIndexWidths(byte pitmVersion, byte ipmaVersion, bool wideIndices, uint itemId)
        {
            int essential = wideIndices ? 0x8000 : 0x80;
            byte[] avif = Container(Pitm(itemId, pitmVersion), Box("iprp", Box("ipco", Ispe(640, 480), Pixi(12, 4)),
                Ipma(ipmaVersion, wideIndices, Entry(itemId, ipmaVersion, wideIndices, 0, essential | 1, 2))));
            AssertPrimaryFacts(ImgCodecsCv2.Identify(avif), 640, 480, 12, 4);
        }

        [Fact]
        public void IdentifyResolvesWideAvifIndicesAndAllFourAssociationTables()
        {
            byte[][] properties = new byte[130][];
            for (int index = 0; index < properties.Length; ++index) properties[index] = Box("free");
            properties[127] = Ispe(640, 480);
            properties[128] = Pixi(10, 3);
            byte[] avif = Container(Pitm(8), Box("iprp", Box("ipco", properties),
                Ipma(0, false, Entry(2, 0, false), Entry(6, 0, false)),
                Ipma(0, true, Entry(1, 0, true), Entry(5, 0, true)),
                Ipma(1, false, Entry(3, 1, false), Entry(7, 1, false)),
                Ipma(1, true, Entry(4, 1, true), Entry(8, 1, true, 0x8080, 129))));
            AssertPrimaryFacts(ImgCodecsCv2.Identify(avif), 640, 480, 10, 3);

            byte[][] maximumProperties = new byte[255][];
            int[] maximumAssociations = new int[255];
            for (int index = 0; index < 255; ++index)
            {
                maximumProperties[index] = Box("free");
                maximumAssociations[index] = index + 1;
            }
            maximumProperties[254] = Ispe(2, 3);
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Container(Pitm(1), Box("iprp", Box("ipco", maximumProperties),
                Ipma(0, true, Entry(1, 0, true, maximumAssociations)))));
            Assert.True(result.IsSizeKnown);
            Assert.Equal(2, result.Width);
            Assert.Equal(3, result.Height);
        }

        [Fact]
        public void IdentifyNeverBorrowsAvifPrecisionFromAnotherItemOrUnassociatedProperty()
        {
            byte[] avif = Container(Pitm(1), Box("iprp", Box("ipco", Ispe(640, 480), Pixi(8, 3), Box("ispe", new byte[] { 1 })),
                Ipma(0, false, Entry(1, 0, false, 1), Entry(2, 0, false, 2))));
            ImageIdentifyResult result = ImgCodecsCv2.Identify(avif);
            Assert.True(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown);
            Assert.False(result.IsBitDepthKnown);
            Assert.False(result.IsChannelCountKnown);

            byte[][] unsupportedProfiles =
            {
                Pixi(16, 3), Pixi(8, 2), FullBox("pixi", 0, 0, new byte[] { 3, 8, 10, 12 })
            };
            foreach (byte[] profile in unsupportedProfiles)
            {
                result = ImgCodecsCv2.Identify(Associated(Ispe(640, 480), profile));
                Assert.True(result.IsSizeKnown);
                Assert.False(result.IsPixelFormatKnown);
            }
        }

        [Fact]
        public void IdentifyRejectsAmbiguousOrMalformedAvifPrimaryAssociations()
        {
            byte[] ipco = Box("ipco", Ispe(640, 480), Pixi(10, 3));
            byte[] validTable = Ipma(0, false, Entry(1, 0, false, 1, 2));
            byte[] iprp = Box("iprp", ipco, validTable);
            var malformed = new[]
            {
                new { Name = "missing-pitm", Bytes = Container(iprp) },
                new { Name = "missing-iprp", Bytes = Container(Pitm(1)) },
                new { Name = "missing-ipma", Bytes = Container(Pitm(1), Box("iprp", ipco)) },
                new { Name = "missing-ipco", Bytes = Container(Pitm(1), Box("iprp", validTable)) },
                new { Name = "misplaced-ipco", Bytes = Container(Pitm(1), Box("iprp", validTable, ipco)) },
                new { Name = "duplicate-pitm", Bytes = Container(Pitm(1), Pitm(1), iprp) },
                new { Name = "duplicate-iprp", Bytes = Container(Pitm(1), iprp, iprp) },
                new { Name = "duplicate-ipco", Bytes = Container(Pitm(1), Box("iprp", ipco, ipco, validTable)) },
                new { Name = "zero-primary-id", Bytes = Container(Pitm(0), iprp) },
                new { Name = "unsupported-pitm-version", Bytes = Container(Pitm(1, 2), iprp) },
                new { Name = "pitm-flags", Bytes = Container(FullBox("pitm", 0, 1, U16(1)), iprp) },
                new { Name = "pitm-trailing", Bytes = Container(FullBox("pitm", 0, 0, U16(1), new byte[1]), iprp) },
                new { Name = "missing-primary-entry", Bytes = Container(Pitm(3), iprp) },
                new { Name = "unsupported-ipma-version", Bytes = Container(Pitm(1), Box("iprp", ipco, Ipma(2, false, Entry(1, 2, false, 1, 2)))) },
                new { Name = "ipma-flags", Bytes = Container(Pitm(1), Box("iprp", ipco, FullBox("ipma", 0, 2, U32(1), Entry(1, 0, false, 1, 2)))) },
                new { Name = "ipma-high-flags", Bytes = Container(Pitm(1), Box("iprp", ipco, FullBox("ipma", 0, 0x100, U32(1), Entry(1, 0, false, 1, 2)))) },
                new { Name = "huge-entry-count", Bytes = Container(Pitm(1), Box("iprp", ipco, FullBox("ipma", 0, 0, U32(uint.MaxValue)))) },
                new { Name = "ipma-trailing", Bytes = Container(Pitm(1), Box("iprp", ipco, FullBox("ipma", 0, 0, U32(1), Entry(1, 0, false, 1, 2), new byte[1]))) },
                new { Name = "empty-ipma-trailing", Bytes = Container(Pitm(1), Box("iprp", ipco, FullBox("ipma", 0, 0, U32(0), new byte[1]))) },
                new { Name = "truncated-association", Bytes = Container(Pitm(1), Box("iprp", ipco, FullBox("ipma", 0, 0, U32(1), U16(1), new byte[] { 255, 1, 2 }))) },
                new { Name = "truncated-wide-association", Bytes = Container(Pitm(1), Box("iprp", ipco, FullBox("ipma", 0, 1, U32(1), U16(1), new byte[] { 2, 0, 1, 0 }))) },
                new { Name = "zero-item-id", Bytes = Container(Pitm(1), Box("iprp", ipco, Ipma(0, false, Entry(0, 0, false, 1, 2)))) },
                new { Name = "duplicate-item-id", Bytes = Container(Pitm(1), Box("iprp", ipco, Ipma(0, false, Entry(1, 0, false, 1), Entry(1, 0, false, 2)))) },
                new { Name = "descending-item-id", Bytes = Container(Pitm(1), Box("iprp", ipco, Ipma(0, false, Entry(2, 0, false), Entry(1, 0, false, 1, 2)))) },
                new { Name = "cross-table-duplicate", Bytes = Container(Pitm(1), Box("iprp", ipco, validTable, Ipma(1, false, Entry(1, 1, false, 1, 2)))) },
                new { Name = "duplicate-version-flags", Bytes = Container(Pitm(1), Box("iprp", ipco, validTable, Ipma(0, false))) },
                new { Name = "essential-zero-index", Bytes = Container(Pitm(1), Box("iprp", ipco, Ipma(0, false, Entry(1, 0, false, 1, 2, 0x80)))) },
                new { Name = "essential-wide-zero", Bytes = Container(Pitm(1), Box("iprp", ipco, Ipma(0, true, Entry(1, 0, true, 1, 2, 0x8000)))) },
                new { Name = "out-of-range-index", Bytes = Container(Pitm(1), Box("iprp", ipco, Ipma(0, false, Entry(1, 0, false, 3)))) },
                new { Name = "out-of-range-other-item", Bytes = Container(Pitm(1), Box("iprp", ipco, Ipma(0, false, Entry(1, 0, false, 1, 2), Entry(2, 0, false, 127)))) },
                new { Name = "duplicate-primary-index", Bytes = Container(Pitm(1), Box("iprp", ipco, Ipma(0, false, Entry(1, 0, false, 1, 2, 1)))) },
                new { Name = "duplicate-ispe", Bytes = Associated(Ispe(640, 480), Ispe(640, 480)) },
                new { Name = "conflicting-ispe", Bytes = Associated(Ispe(640, 480), Ispe(32, 24)) },
                new { Name = "duplicate-pixi", Bytes = Associated(Ispe(640, 480), Pixi(10, 3), Pixi(10, 3)) },
                new { Name = "unsupported-ispe-version", Bytes = Associated(FullBox("ispe", 1, 0, U32(640), U32(480))) },
                new { Name = "unsupported-pixi-flags", Bytes = Associated(Ispe(640, 480), FullBox("pixi", 0, 1, new byte[] { 3, 8, 8, 8 })) },
                new { Name = "zero-pixi-channels", Bytes = Associated(Ispe(640, 480), FullBox("pixi", 0, 0, new byte[] { 0 })) },
                new { Name = "truncated-pixi-channels", Bytes = Associated(Ispe(640, 480), FullBox("pixi", 0, 0, new byte[] { 3, 8, 8 })) },
                new { Name = "zero-width", Bytes = Associated(Ispe(0, 480)) },
                new { Name = "width-overflow", Bytes = Associated(Ispe(uint.MaxValue, 480)) }
            };
            foreach (var fixture in malformed)
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(fixture.Bytes);
                Assert.Equal("avif", result.Format);
                Assert.False(result.IsSizeKnown, fixture.Name);
                Assert.False(result.IsPixelFormatKnown, fixture.Name);
            }
        }

        [Fact]
        public void IdentifyValidatesAvifFullBoxesAndExtendedContainerLengths()
        {
            byte[] valid = Associated(Ispe(2, 3), Pixi(8, 3));
            byte[] meta = new byte[valid.Length - 20 - 12];
            Array.Copy(valid, 20, meta, 0, meta.Length);
            byte[] extendedFileType = ExtendedBox("ftyp", Encoding.ASCII.GetBytes("mif1"), U32(0), Encoding.ASCII.GetBytes("avif"));
            AssertPrimaryFacts(ImgCodecsCv2.Identify(Join(extendedFileType, meta)), 2, 3, 8, 3);
            byte[] extendedMeta = ExtendedBox("meta", new byte[4], Pitm(1),
                ExtendedBox("iprp", ExtendedBox("ipco", Ispe(2, 3), Pixi(8, 3)), Ipma(0, false, Entry(1, 0, false, 1, 2))));
            AssertPrimaryFacts(ImgCodecsCv2.Identify(Join(Box("ftyp", Encoding.ASCII.GetBytes("avif"), U32(0)), extendedMeta)), 2, 3, 8, 3);
            foreach (int byteIndex in new[] { 8, 9, 10, 11 })
            {
                byte[] invalidMeta = (byte[])meta.Clone();
                invalidMeta[byteIndex] = 1;
                Assert.False(ImgCodecsCv2.Identify(Join(extendedFileType, invalidMeta)).IsSizeKnown);
            }
            byte[] hugeBox = Join(U32(1), Encoding.ASCII.GetBytes("free"), U32(uint.MaxValue), U32(uint.MaxValue));
            ImageIdentifyResult malformed = ImgCodecsCv2.Identify(Join(valid, hugeBox));
            Assert.False(malformed.IsSizeKnown);
            Assert.False(malformed.IsPixelFormatKnown);
            byte[] misleadingBrand = Join(Box("ftyp", Encoding.ASCII.GetBytes("mif1"), U32(0)), Encoding.ASCII.GetBytes("avif"), meta);
            Assert.Equal("unknown", ImgCodecsCv2.Identify(misleadingBrand).Format);
        }

        [Fact]
        public void AvifPrimaryItemLimitsRejectBeforeNativeDecodeAndPreserveStreamPosition()
        {
            byte[] avif = Container(Pitm(1), Box("iprp", Box("ipco", Ispe(32, 24), Pixi(8, 1), Ispe(640, 480), Pixi(12, 4)),
                Ipma(0, false, Entry(1, 0, false, 3, 4), Entry(2, 0, false, 1, 2))));
            var sizeLimit = new ImageDecodeOptions(avif.Length, 100, 1000, 1000000, 1, true, true);
            var depthLimit = new ImageDecodeOptions(avif.Length, 1000, 1000, 1000000, 1, true, true,
                long.MaxValue, long.MaxValue, false, false, long.MaxValue, 8, 4, true);
            var channelLimit = new ImageDecodeOptions(avif.Length, 1000, 1000, 1000000, 1, true, true,
                long.MaxValue, long.MaxValue, false, false, long.MaxValue, 12, 3, true);
            foreach (ImageDecodeOptions options in new[] { sizeLimit, depthLimit, channelLimit })
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(avif, options));
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(avif), options));
                using (var stream = new MemoryStream())
                {
                    stream.WriteByte(0x7F);
                    stream.Write(avif, 0, avif.Length);
                    stream.Position = 1;
                    Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(1, stream.Position);
                }
            }
            byte[] unassociated = Container(Pitm(1), Box("iprp", Box("ipco", Ispe(2, 3))));
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(unassociated,
                new ImageDecodeOptions(unassociated.Length, 1000, 1000, 1000000, 1, true, true)));
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusAvifPrimaryItemKeepsEveryTraversalBounded()
        {
            byte[] avif = Associated(Ispe(640, 480), Pixi(10, 3));
            for (int length = 1; length < avif.Length; ++length)
            {
                byte[] prefix = new byte[length];
                Array.Copy(avif, prefix, length);
                ImageIdentifyResult result = ImgCodecsCv2.Identify(prefix);
                // A boundary immediately after meta still proves header facts; payload completeness stays unknown.
                Assert.Equal(length == avif.Length - 12, result.IsSizeKnown);
                Assert.Equal(length == avif.Length - 12, result.IsPixelFormatKnown);
                Assert.False(result.IsFrameCountKnown);
                Assert.False(result.IsCumulativePixelCountKnown);
            }
            VerifyMutations(avif);
            VerifyMutations(Convert.FromBase64String(EncodedAvif));
        }

        private static void VerifyMutations(byte[] avif)
        {
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            {
                for (int offset = 0; offset < avif.Length; ++offset)
                {
                    if (avif[offset] == replacement) continue;
                    byte[] mutated = (byte[])avif.Clone();
                    mutated[offset] = replacement;
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                    Assert.False(result.IsFrameCountKnown);
                    Assert.False(result.IsCumulativePixelCountKnown);
                    if (result.IsSizeKnown)
                    {
                        Assert.InRange(result.Width, 1, int.MaxValue);
                        Assert.InRange(result.Height, 1, int.MaxValue);
                    }
                    if (result.IsPixelFormatKnown)
                    {
                        Assert.True(result.IsSizeKnown);
                        Assert.Contains(result.BitDepth, new[] { 8, 10, 12 });
                        Assert.Contains(result.ChannelCount, new[] { 1, 3, 4 });
                    }
                }
            }
        }

        private static void AssertPrimaryFacts(ImageIdentifyResult result, int width, int height, int depth, int channels)
        {
            Assert.Equal("avif", result.Format);
            Assert.True(result.IsSizeKnown);
            Assert.Equal(width, result.Width);
            Assert.Equal(height, result.Height);
            Assert.True(result.IsPixelFormatKnown);
            Assert.Equal(depth, result.BitDepth);
            Assert.Equal(channels, result.ChannelCount);
        }

        private static byte[] Associated(params byte[][] properties)
        {
            int[] indices = new int[properties.Length];
            for (int index = 0; index < indices.Length; ++index) indices[index] = index + 1;
            return Container(Pitm(1), Box("iprp", Box("ipco", properties), Ipma(0, false, Entry(1, 0, false, indices))));
        }

        private static byte[] ExtendedBox(string type, params byte[][] payload)
        {
            byte[] contents = Join(payload);
            return Join(U32(1), Encoding.ASCII.GetBytes(type), U32(0), U32((uint)(16 + contents.Length)), contents);
        }

        private static byte[] Container(params byte[][] metaChildren)
        {
            return Join(Box("ftyp", Encoding.ASCII.GetBytes("avif"), U32(0), Encoding.ASCII.GetBytes("mif1")),
                FullBox("meta", 0, 0, metaChildren), Box("mdat", new byte[] { 1, 2, 3, 4 }));
        }

        private static byte[] Pitm(uint itemId, byte version = 0)
        {
            return FullBox("pitm", version, 0, version == 0 ? U16((int)itemId) : U32(itemId));
        }

        private static byte[] Ispe(uint width, uint height)
        {
            return FullBox("ispe", 0, 0, U32(width), U32(height));
        }

        private static byte[] Pixi(byte depth, byte channels)
        {
            byte[] payload = new byte[1 + channels];
            payload[0] = channels;
            for (int index = 1; index < payload.Length; ++index) payload[index] = depth;
            return FullBox("pixi", 0, 0, payload);
        }

        private static byte[] Ipma(byte version, bool wideIndices, params byte[][] entries)
        {
            return FullBox("ipma", version, wideIndices ? 1u : 0u, U32((uint)entries.Length), Join(entries));
        }

        private static byte[] Entry(uint itemId, byte version, bool wideIndices, params int[] indices)
        {
            using (var stream = new MemoryStream())
            {
                byte[] id = version == 0 ? U16((int)itemId) : U32(itemId);
                stream.Write(id, 0, id.Length);
                stream.WriteByte((byte)indices.Length);
                foreach (int index in indices)
                {
                    if (wideIndices) stream.WriteByte((byte)(index >> 8));
                    stream.WriteByte((byte)index);
                }
                return stream.ToArray();
            }
        }

        private static byte[] FullBox(string type, byte version, uint flags, params byte[][] payload)
        {
            return Box(type, new byte[] { version, (byte)(flags >> 16), (byte)(flags >> 8), (byte)flags }, Join(payload));
        }

        private static byte[] Box(string type, params byte[][] payload)
        {
            byte[] contents = Join(payload);
            return Join(U32((uint)(8 + contents.Length)), Encoding.ASCII.GetBytes(type), contents);
        }

        private static byte[] Join(params byte[][] parts)
        {
            using (var stream = new MemoryStream())
            {
                foreach (byte[] part in parts) stream.Write(part, 0, part.Length);
                return stream.ToArray();
            }
        }

        private static byte[] U16(int value)
        {
            return new byte[] { (byte)(value >> 8), (byte)value };
        }

        private static byte[] U32(uint value)
        {
            return new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value };
        }
    }
}
