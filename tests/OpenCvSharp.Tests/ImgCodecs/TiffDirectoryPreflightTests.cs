using System;
using System.IO;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class TiffDirectoryPreflightTests
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IdentifyNeverLetsDuplicateTiffDimensionsOverrideTheAdmissionBudget(bool bigTiff, bool bigEndian)
        {
            byte[] data = Tiff(bigTiff, bigEndian, new[] { Width(1000), Width(4), Height(), Bits(), Samples() });
            AssertUnknown(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IdentifyNeverLetsDuplicateTiffChannelsOverrideTheAdmissionBudget(bool bigTiff, bool bigEndian)
        {
            byte[] data = Tiff(bigTiff, bigEndian, new[] { Width(), Height(), Bits(8), Samples(4), Samples(1) });
            AssertUnknown(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IdentifyRequiresTiffPerSampleBitCountsToMatchChannels(bool bigTiff, bool bigEndian)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Tiff(bigTiff, bigEndian,
                new[] { Width(), Height(), Bits(8, 8), Samples(3) }));
            Assert.True(result.IsFrameCountKnown);
            Assert.False(result.IsBitDepthKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        [Fact]
        public void IdentifyRejectsDuplicateTiffAdmissionTagsInBothDirectoryFormats()
        {
            foreach (bool big in new[] { false, true })
            foreach (bool endian in new[] { false, true })
            foreach (Tag duplicate in new[] { Width(), Height(), Bits(), Samples() })
            {
                AssertUnknown(ImgCodecsCv2.Identify(Tiff(big, endian, new[] { Width(), Height(), Bits(), Samples(), duplicate })));
                AssertUnknown(ImgCodecsCv2.Identify(Tiff(big, endian, ValidPage(), new[] { Width(), duplicate, Height(), Bits(), Samples() })));
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IdentifyReadsTiffExplicitSampleLayoutsRegardlessOfTagOrder(bool big, bool endian)
        {
            foreach (int channels in new[] { 1, 2, 3, 4, 16, 17, 255, 65535 })
            foreach (int depth in new[] { 1, 8, 16, 32, 64 })
            {
                ulong[] values = new ulong[channels]; Array.Fill(values, (ulong)depth);
                // Scalar BitsPerSample is the compatibility convention accepted by libtiff.
                foreach (Tag bits in new[] { Bits((ulong)depth), Bits(values) })
                    AssertImage(ImgCodecsCv2.Identify(Tiff(big, endian,
                        new[] { Samples((ulong)channels), Height(), bits, Width() })), 4, 3, 1, depth, channels);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyKeepsInconsistentTiffSampleFactsUnknownWhileCountingDirectories(bool big)
        {
            foreach (Tag[] page in new[]
            {
                new[] { Width(), Height(), Bits(8, 16, 8), Samples() },
                new[] { Width(), Height(), Bits(8, 8), Samples() },
                new[] { Width(), Height(), Bits(8, 8, 8, 8), Samples() },
                new[] { Width(), Height(), new Tag(258, 3), Samples() },
                new[] { Width(), Height(), Bits(0), Samples() },
                new[] { Width(), Height(), Bits(65), Samples() },
                new[] { Width(), Height(), new Tag(258, 4, 8), Samples() },
                new[] { Width(), Height(), Bits(), new Tag(277, 4, 3) },
                new[] { Width(), Height(), Bits(), Samples(0) },
                new[] { Width(), Height(), Bits(), new Tag(277, 3, 3, 3) }
            })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(Tiff(big, false, page));
                Assert.True(result.IsSizeKnown);
                Assert.Equal(4, result.Width); Assert.Equal(3, result.Height);
                Assert.True(result.IsFrameCountKnown); Assert.Equal(1, result.FrameCount);
                Assert.True(result.IsCumulativePixelCountKnown); Assert.Equal(12, result.CumulativePixelCount);
                Assert.False(result.IsPixelFormatKnown);
                Assert.False(result.IsEstimatedPixelBytesKnown);
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IdentifyDoesNotTreatTiffIfdPointersAsDimensions(bool big, bool endian)
        {
            int pointerType = big ? 18 : 13;
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Tiff(big, endian,
                new[] { new Tag(256, pointerType, 4), Height(), Bits(), Samples() }));
            Assert.False(result.IsSizeKnown);
            Assert.True(result.IsFrameCountKnown);
            Assert.True(result.IsPixelFormatKnown);
            Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
            // A separate SubIFD pointer is bounded header data, not a page in the main chain.
            AssertImage(ImgCodecsCv2.Identify(Tiff(big, endian,
                new[] { Width(), Height(), Bits(), Samples(), new Tag(330, pointerType, 0) })), 4, 3, 1, 8, 3);
            AssertImage(ImgCodecsCv2.Identify(Tiff(big, endian,
                new[] { new Tag(256, 3, 4), new Tag(257, 3, 3), Bits(), Samples() })), 4, 3, 1, 8, 3);
            if (big)
                AssertImage(ImgCodecsCv2.Identify(Tiff(big, endian,
                    new[] { new Tag(256, 16, 4), new Tag(257, 16, 3), Bits(), Samples() })), 4, 3, 1, 8, 3);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyRetainsOnlyUniformTiffPrecisionAndSumsEveryPageSize(bool big)
        {
            byte[] data = Tiff(big, false, ValidPage(), new[] { Width(2), Height(2), Bits(), Samples() });
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            Assert.False(result.IsSizeKnown); Assert.Equal(2, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown); Assert.Equal(16, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown); Assert.Equal(48, result.EstimatedPixelBytes);
            result = ImgCodecsCv2.Identify(Tiff(big, false, ValidPage(), new[] { Width(), Height(), Bits(16, 16, 16), Samples() }));
            Assert.False(result.IsBitDepthKnown); Assert.True(result.IsChannelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
            result = ImgCodecsCv2.Identify(Tiff(big, false, ValidPage(), new[] { Width(), Height(), Bits(8), Samples(4) }));
            Assert.True(result.IsBitDepthKnown); Assert.False(result.IsChannelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IdentifyBoundsTiffValueOffsetsDirectoryCountsAndCycles(bool big, bool endian)
        {
            byte[] valid = Tiff(big, endian, ValidPage(), ValidPage());
            int header = big ? 16 : 8, countBytes = big ? 8 : 2, entryBytes = big ? 20 : 12, slot = big ? 8 : 4;
            foreach (ulong value in new[] { 0UL, 1UL, (ulong)valid.Length, 0x7FFFFFFFUL, ulong.MaxValue })
            {
                byte[] invalid = (byte[])valid.Clone(); Write(invalid, big ? 8 : 4, value, slot, endian);
                AssertUnknown(ImgCodecsCv2.Identify(invalid));
            }
            foreach (ulong value in new[] { (ulong)header, (ulong)valid.Length, ulong.MaxValue })
            {
                byte[] invalid = (byte[])valid.Clone(); Write(invalid, header + countBytes + 4 * entryBytes, value, slot, endian);
                AssertUnknown(ImgCodecsCv2.Identify(invalid));
            }
            foreach (ulong value in new[] { 0x7FFFFFFFUL, ulong.MaxValue })
            {
                byte[] invalid = (byte[])valid.Clone(); Write(invalid, header, value, countBytes, endian);
                AssertUnknown(ImgCodecsCv2.Identify(invalid));
                invalid = (byte[])valid.Clone(); Write(invalid, header + countBytes + 2 * entryBytes + 4, value, big ? 8 : 4, endian);
                AssertUnknown(ImgCodecsCv2.Identify(invalid));
            }
            Tag largeBits = Bits(8, 8, 8, 8, 8);
            byte[] external = Tiff(big, endian, new[] { Width(), Height(), largeBits, Samples(5) });
            foreach (ulong value in new[] { (ulong)external.Length - 1, ulong.MaxValue })
            {
                byte[] invalid = (byte[])external.Clone(); Write(invalid, header + countBytes + 2 * entryBytes + (big ? 12 : 8), value, slot, endian);
                AssertUnknown(ImgCodecsCv2.Identify(invalid));
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyBoundsTiffOverlappingDirectoryAndRepeatedSampleWork(bool big)
        {
            // Thousands of zero-entry directories remain bounded by the explicit page cap.
            var empty = new Tag[65537][]; for (int i = 0; i < empty.Length; ++i) empty[i] = new Tag[0];
            AssertUnknown(ImgCodecsCv2.Identify(Tiff(big, false, empty)));
            int count = 65535;
            ulong[] values = new ulong[count]; Array.Fill(values, 8UL);
            byte[] original = Tiff(big, false, new[] { Width(), Height(), Bits(values), Samples((ulong)count) },
                new[] { Width(), Height(), Bits(values), Samples((ulong)count) });
            int header = big ? 16 : 8, countBytes = big ? 8 : 2, entryBytes = big ? 20 : 12, slot = big ? 8 : 4;
            int firstSlot = header + countBytes + 2 * entryBytes + (big ? 12 : 8);
            int secondSlot = firstSlot + countBytes + 4 * entryBytes + slot;
            // Repeated references use the fixed successful-range cache.
            Array.Copy(original, firstSlot, original, secondSlot, slot);
            AssertImage(ImgCodecsCv2.Identify(original), 4, 3, 2, 8, count);
            // A physical two-IFD file with alternating shared tables cannot force one large
            // table scan per page. Clone valid IFD bytes and update only their next pointers.
            int pages = 32, ifdBytes = countBytes + 4 * entryBytes + slot;
            byte[] repeated = new byte[header + pages * ifdBytes + count * 4];
            Array.Copy(original, repeated, header);
            for (int page = 0; page < pages; ++page)
            {
                int directory = header + page * ifdBytes;
                Array.Copy(original, header, repeated, directory, ifdBytes);
                Write(repeated, directory + countBytes + 2 * entryBytes + (big ? 12 : 8),
                    (ulong)(header + pages * ifdBytes + (page % 2) * count * 2), slot, false);
                Write(repeated, directory + countBytes + 4 * entryBytes,
                    page + 1 == pages ? 0 : (ulong)(directory + ifdBytes), slot, false);
            }
            for (int i = header + pages * ifdBytes; i < repeated.Length; i += 2) Write(repeated, i, 8, 2, false);
            ImageIdentifyResult bounded = ImgCodecsCv2.Identify(repeated);
            Assert.True(bounded.IsFrameCountKnown); Assert.Equal(pages, bounded.FrameCount);
            Assert.True(bounded.IsCumulativePixelCountKnown); Assert.Equal(12 * pages, bounded.CumulativePixelCount);
            Assert.False(bounded.IsBitDepthKnown); Assert.True(bounded.IsChannelCountKnown);
            Assert.False(bounded.IsEstimatedPixelBytesKnown);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void TiffBudgetsRejectBeforeNativeDecodeAndPreserveStreams(bool big)
        {
            byte[] data = Tiff(big, false, ValidPage(), ValidPage());
            foreach (var limit in new[]
            {
                new { Options = Options(data.Length, frames: 1), Error = "frame count" },
                new { Options = Options(data.Length, cumulative: 23), Error = "cumulative pixel count" },
                new { Options = Options(data.Length, storage: 71), Error = "pixel storage" },
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
                using (var stream = new ShortReadNonSeekableStream(data))
                {
                    Assert.Contains(limit.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, limit.Options)).Message);
                    Assert.Equal(data.Length, stream.BytesRead);
                }
            }
            using (var stream = new MemoryStream(data))
            {
                AssertImage(ImgCodecsCv2.Identify(stream, Options(data.Length)), 4, 3, 2, 8, 3);
                Assert.Equal(0, stream.Position);
            }
            using (var stream = new ShortReadNonSeekableStream(data))
            {
                AssertImage(ImgCodecsCv2.Identify(stream, Options(data.Length)), 4, 3, 2, 8, 3);
                Assert.Equal(data.Length, stream.BytesRead);
            }
        }

        [Fact]
        public void TiffStrictPixelPolicyRejectsAmbiguousSamplesAndDuplicates()
        {
            foreach (bool big in new[] { false, true })
            foreach (byte[] data in new[] { Tiff(big, false, new[] { Width(), Height(), Bits(8, 8), Samples() }),
                Tiff(big, false, new[] { Width(), Height(), Bits(), Samples(), Samples(1) }) })
            {
                var strict = new ImageDecodeOptions(data.Length, int.MaxValue, int.MaxValue, long.MaxValue, int.MaxValue,
                    true, false, long.MaxValue, long.MaxValue, false, false, long.MaxValue, 64, 65535, true, long.MaxValue);
                Assert.Contains("depth or channel count", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, strict)).Message);
                Assert.Contains("depth or channel count", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(data), strict)).Message);
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, data)))
                {
                    stream.Position = 1;
                    Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, strict)); Assert.Equal(1, stream.Position);
                }
            }
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void IdentifyReadsRealTiffPagesAndExplicitPrecision(bool big, bool endian)
        {
            byte[] data = big ? endian ? RealBigTiffBig : RealBigTiffLittle : endian ? RealClassicBig : RealClassicLittle;
            AssertImage(ImgCodecsCv2.Identify(data), 4, 3, 2, endian ? 16 : 8, 3);
            Assert.Equal(big ? "bigtiff" : "tiff", ImgCodecsCv2.Identify(data).Format);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyRejectsTiffCumulativeAndEncodedStorageOverflow(bool big)
        {
            Tag[] huge = { Width(int.MaxValue), Height(int.MaxValue), Bits(64), Samples(65535) };
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Tiff(big, false, huge));
            Assert.True(result.IsSizeKnown); Assert.True(result.IsFrameCountKnown);
            Assert.True(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown);
            byte[] overflow = Tiff(big, false, huge, huge, huge);
            result = ImgCodecsCv2.Identify(overflow);
            Assert.True(result.IsSizeKnown); Assert.Equal(3, result.FrameCount);
            Assert.False(result.IsCumulativePixelCountKnown); Assert.False(result.IsEstimatedPixelBytesKnown);
            var options = new ImageDecodeOptions(overflow.Length, int.MaxValue, int.MaxValue, long.MaxValue, 3, true, true,
                long.MaxValue, long.MaxValue, false, false, long.MaxValue, 64, 65535, true, long.MaxValue);
            Assert.Contains("cumulative pixel count", Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(overflow, options)).Message);
        }

        [Fact]
        public void IdentifyRequiresCompleteTiffDirectoryAndSampleRanges()
        {
            foreach (bool big in new[] { false, true })
            foreach (bool endian in new[] { false, true })
            {
                byte[] data = Tiff(big, endian, new[] { Width(), Height(), Bits(8, 8, 8, 8, 8), Samples(5) });
                for (int length = 1; length < data.Length; ++length)
                {
                    byte[] prefix = new byte[length]; Array.Copy(data, prefix, length);
                    AssertUnknown(ImgCodecsCv2.Identify(prefix));
                }
                // TIFF has no exact EOF marker: trailing unreferenced bytes and undecoded
                // strips do not invalidate otherwise complete directory/header facts.
                AssertImage(ImgCodecsCv2.Identify(Join(data, new byte[] { 1, 2, 3 })), 4, 3, 1, 8, 5);
            }
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusTiffDirectoriesNeverEscapeOrPublishInconsistentBudgets()
        {
            foreach (byte[] data in new[] { Tiff(false, false, ValidPage(), ValidPage()), Tiff(true, true, ValidPage()),
                RealClassicLittle, RealClassicBig, RealBigTiffLittle, RealBigTiffBig })
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            for (int offset = 0; offset < data.Length; ++offset)
            {
                if (data[offset] == replacement) continue;
                byte[] mutated = (byte[])data.Clone(); mutated[offset] = replacement;
                ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                if (result.IsFrameCountKnown) Assert.InRange(result.FrameCount, 1, Math.Min(65536, mutated.Length / 6));
                if (result.IsBitDepthKnown) Assert.InRange(result.BitDepth, 1, 64);
                if (result.IsChannelCountKnown) Assert.InRange(result.ChannelCount, 1, 65535);
                if (result.IsSizeKnown)
                {
                    Assert.True(result.IsFrameCountKnown);
                    Assert.InRange(result.Width, 1, int.MaxValue); Assert.InRange(result.Height, 1, int.MaxValue);
                    if (result.IsCumulativePixelCountKnown)
                        Assert.Equal((long)result.Width * result.Height * result.FrameCount, result.CumulativePixelCount);
                }
                if (result.IsEstimatedPixelBytesKnown)
                {
                    Assert.True(result.IsCumulativePixelCountKnown); Assert.True(result.IsPixelFormatKnown);
                    Assert.Equal(checked(result.CumulativePixelCount * ((result.BitDepth + 7) / 8) * result.ChannelCount), result.EstimatedPixelBytes);
                }
            }
        }

        private static ImageDecodeOptions Options(int length, int frames = 2, long cumulative = 24, long storage = 72, int depth = 8, int channels = 3)
        {
            return new ImageDecodeOptions(length, 4, 3, 12, frames, true, true,
                long.MaxValue, long.MaxValue, false, false, cumulative, depth, channels, true, storage);
        }

        private static Tag[] ValidPage() { return new[] { Width(), Height(), Bits(), Samples() }; }

        private static void AssertImage(ImageIdentifyResult result, int width, int height, int pages, int depth, int channels)
        {
            Assert.True(result.IsSizeKnown); Assert.Equal(width, result.Width); Assert.Equal(height, result.Height);
            Assert.True(result.IsFrameCountKnown); Assert.Equal(pages, result.FrameCount);
            Assert.True(result.IsPixelFormatKnown); Assert.Equal(depth, result.BitDepth); Assert.Equal(channels, result.ChannelCount);
            Assert.True(result.IsCumulativePixelCountKnown); Assert.Equal((long)width * height * pages, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown);
            Assert.Equal((long)width * height * pages * ((depth + 7) / 8) * channels, result.EstimatedPixelBytes);
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
                int read = base.Read(buffer, offset, Math.Min(count, 2)); BytesRead += read; return read;
            }
        }

        private static void AssertUnknown(ImageIdentifyResult result)
        {
            Assert.False(result.IsSizeKnown);
            Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsPixelFormatKnown);
            Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        private sealed class Tag
        {
            public int Id;
            public int Type;
            public ulong[] Values;
            public Tag(int id, int type, params ulong[] values) { Id = id; Type = type; Values = values; }
        }

        private static Tag Width(ulong value = 4) { return new Tag(256, 4, value); }
        private static Tag Height(ulong value = 3) { return new Tag(257, 4, value); }
        private static Tag Bits(params ulong[] values) { return new Tag(258, 3, values.Length == 0 ? new ulong[] { 8, 8, 8 } : values); }
        private static Tag Samples(ulong value = 3) { return new Tag(277, 3, value); }

        // A structural IFD fixture; it proves header facts, not image strip validity.
        private static byte[] Tiff(bool bigTiff, bool bigEndian, params Tag[][] pages)
        {
            int header = bigTiff ? 16 : 8, countBytes = bigTiff ? 8 : 2;
            int entryBytes = bigTiff ? 20 : 12, slotBytes = bigTiff ? 8 : 4;
            int length = header;
            foreach (Tag[] page in pages) length += countBytes + entryBytes * page.Length + slotBytes;
            int payload = length;
            foreach (Tag[] page in pages)
            foreach (Tag tag in page)
            {
                int size = tag.Type == 3 ? 2 : tag.Type == 16 || tag.Type == 18 ? 8 : 4;
                if (size * tag.Values.Length > slotBytes) length += size * tag.Values.Length;
            }
            byte[] data = new byte[length];
            data[0] = data[1] = bigEndian ? (byte)'M' : (byte)'I';
            Write(data, 2, bigTiff ? 43UL : 42UL, 2, bigEndian);
            if (bigTiff) Write(data, 4, 8, 2, bigEndian);
            Write(data, bigTiff ? 8 : 4, (ulong)header, slotBytes, bigEndian);
            int offset = header;
            for (int index = 0; index < pages.Length; ++index)
            {
                Tag[] page = pages[index];
                Write(data, offset, (ulong)page.Length, countBytes, bigEndian);
                int entry = offset + countBytes;
                foreach (Tag tag in page)
                {
                    Write(data, entry, (ulong)tag.Id, 2, bigEndian);
                    Write(data, entry + 2, (ulong)tag.Type, 2, bigEndian);
                    Write(data, entry + 4, (ulong)tag.Values.Length, bigTiff ? 8 : 4, bigEndian);
                    int size = tag.Type == 3 ? 2 : tag.Type == 16 || tag.Type == 18 ? 8 : 4;
                    int valueOffset = entry + (bigTiff ? 12 : 8);
                    if (size * tag.Values.Length > slotBytes)
                    {
                        Write(data, valueOffset, (ulong)payload, slotBytes, bigEndian);
                        valueOffset = payload;
                        payload += size * tag.Values.Length;
                    }
                    foreach (ulong value in tag.Values)
                    {
                        Write(data, valueOffset, value, size, bigEndian); valueOffset += size;
                    }
                    entry += entryBytes;
                }
                offset = entry + slotBytes;
                Write(data, entry, index + 1 < pages.Length ? (ulong)offset : 0, slotBytes, bigEndian);
            }
            return data;
        }

        private static void Write(byte[] data, int offset, ulong value, int bytes, bool bigEndian)
        {
            for (int index = 0; index < bytes; ++index)
                data[offset + (bigEndian ? bytes - 1 - index : index)] = (byte)(value >> (index * 8));
        }
        // tifffile 2026.11.16 / NumPy: two RGB 4x3 pages, 8-bit little-endian or
        // 16-bit big-endian samples. Independently decoded and compared every sample.
        // 452 bytes, SHA256 26cbde5504865a28acaf2b07cd96c2235215b4d96b6fc7af28ed3374af2ee941.
        private static readonly byte[] RealClassicLittle = Convert.FromBase64String(
            "SUkqAAgAAAANAAABBAABAAAABAAAAAEBBAABAAAAAwAAAAIBAwADAAAAqgAAAAMBAwABAAAAAQAAAAYBAwABAAAAAgAAABEBBAABAAAAwAAAABUBAwABAAAAAwAAABYBBAABAAAAAwAAABcBBAABAAAAJAAAABoBBQABAAAAsAAAABsBBQABAAAAuAAAABwBAwABAAAAAQAAACgBAwABAAAAAQAAAOQAAAAIAAgACAABAAAAAQAAAAEAAAABAAAAAAcOFRwjKjE4P0ZNVFtiaXB3foWMk5qhqK+2vcTL0tng5+71DQAAAQQAAQAAAAQAAAABAQQAAQAAAAMAAAACAQMAAwAAAIYBAAADAQMAAQAAAAEAAAAGAQMAAQAAAAIAAAARAQQAAQAAAKABAAAVAQMAAQAAAAMAAAAWAQQAAQAAAAMAAAAXAQQAAQAAACQAAAAaAQUAAQAAAIwBAAAbAQUAAQAAAJQBAAAcAQMAAQAAAAEAAAAoAQMAAQAAAAEAAAAAAAAACAAIAAgAAQAAAAEAAAABAAAAAQAAAAAAAAA/Rk0qMTgVHCMABw6TmqF+hYxpcHdUW2Ln7vXS2eC9xMuor7Y=");

        // 520 bytes, SHA256 ba04d87afdef18cc5da684ee2b5dbcf548ebdc2dd6a6c73a34b7a72ed1f02db6.
        private static readonly byte[] RealClassicBig = Convert.FromBase64String(
            "TU0AKgAAAAgADQEAAAQAAAABAAAABAEBAAQAAAABAAAAAwECAAMAAAADAAAAqgEDAAMAAAABAAEAAAEGAAMAAAABAAIAAAERAAQAAAABAAAAwAEVAAMAAAABAAMAAAEWAAQAAAABAAAAAwEXAAQAAAABAAAASAEaAAUAAAABAAAAsAEbAAUAAAABAAAAuAEcAAMAAAABAAEAAAEoAAMAAAABAAEAAAAAAQgAEAAQABAAAAABAAAAAQAAAAEAAAABAHsA4AFFAaoCDwJ0AtkDPgOjBAgEbQTSBTcFnAYBBmYGywcwB5UH+ghfCMQJKQmOCfMKWAq9CyILhwvsDFEMtg0bDYAN5Q5KAA0BAAAEAAAAAQAAAAQBAQAEAAAAAQAAAAMBAgADAAAAAwAAAaoBAwADAAAAAQABAAABBgADAAAAAQACAAABEQAEAAAAAQAAAcABFQADAAAAAQADAAABFgAEAAAAAQAAAAMBFwAEAAAAAQAAAEgBGgAFAAAAAQAAAbABGwAFAAAAAQAAAbgBHAADAAAAAQABAAABKAADAAAAAQABAAAAAAAAABAAEAAQAAAAAQAAAAEAAAABAAAAAQQIBG0E0gLZAz4DowGqAg8CdAB7AOABRQjECSkJjgeVB/oIXwZmBssHMAU3BZwGAQ2ADeUOSgxRDLYNGwsiC4cL7AnzClgKvQ==");

        // 660 bytes, SHA256 e9dce7439d6fc4d12b73ad205ef64c2dcccf17d13dcafaea6275a9eb0b11a326.
        private static readonly byte[] RealBigTiffLittle = Convert.FromBase64String(
            "SUkrAAgAAAAQAAAAAAAAAA0AAAAAAAAAAAEEAAEAAAAAAAAABAAAAAAAAAABAQQAAQAAAAAAAAADAAAAAAAAAAIBAwADAAAAAAAAAAgACAAIAAAAAwEDAAEAAAAAAAAAAQAAAAAAAAAGAQMAAQAAAAAAAAACAAAAAAAAABEBEAABAAAAAAAAADABAAAAAAAAFQEDAAEAAAAAAAAAAwAAAAAAAAAWAQQAAQAAAAAAAAADAAAAAAAAABcBEAABAAAAAAAAACQAAAAAAAAAGgEFAAEAAAAAAAAAAQAAAAEAAAAbAQUAAQAAAAAAAAABAAAAAQAAABwBAwABAAAAAAAAAAEAAAAAAAAAKAEDAAEAAAAAAAAAAQAAAAAAAABUAQAAAAAAAAAAAAAAAAAAAAAAAAAHDhUcIyoxOD9GTVRbYmlwd36FjJOaoaivtr3Ey9LZ4Ofu9Q0AAAAAAAAAAAEEAAEAAAAAAAAABAAAAAAAAAABAQQAAQAAAAAAAAADAAAAAAAAAAIBAwADAAAAAAAAAAgACAAIAAAAAwEDAAEAAAAAAAAAAQAAAAAAAAAGAQMAAQAAAAAAAAACAAAAAAAAABEBEAABAAAAAAAAAHACAAAAAAAAFQEDAAEAAAAAAAAAAwAAAAAAAAAWAQQAAQAAAAAAAAADAAAAAAAAABcBEAABAAAAAAAAACQAAAAAAAAAGgEFAAEAAAAAAAAAAQAAAAEAAAAbAQUAAQAAAAAAAAABAAAAAQAAABwBAwABAAAAAAAAAAEAAAAAAAAAKAEDAAEAAAAAAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP0ZNKjE4FRwjAAcOk5qhfoWMaXB3VFti5+710tngvcTLqK+2");

        // 728 bytes, SHA256 009961b6c07fbe66bf58964078656ca52a56aa831bae66916cc7efbc96e44dc2.
        private static readonly byte[] RealBigTiffBig = Convert.FromBase64String(
            "TU0AKwAIAAAAAAAAAAAAEAAAAAAAAAANAQAABAAAAAAAAAABAAAABAAAAAABAQAEAAAAAAAAAAEAAAADAAAAAAECAAMAAAAAAAAAAwAQABAAEAAAAQMAAwAAAAAAAAABAAEAAAAAAAABBgADAAAAAAAAAAEAAgAAAAAAAAERABAAAAAAAAAAAQAAAAAAAAEwARUAAwAAAAAAAAABAAMAAAAAAAABFgAEAAAAAAAAAAEAAAADAAAAAAEXABAAAAAAAAAAAQAAAAAAAABIARoABQAAAAAAAAABAAAAAQAAAAEBGwAFAAAAAAAAAAEAAAABAAAAAQEcAAMAAAAAAAAAAQABAAAAAAAAASgAAwAAAAAAAAABAAEAAAAAAAAAAAAAAAABeAAAAAAAAAAAAAAAAAB7AOABRQGqAg8CdALZAz4DowQIBG0E0gU3BZwGAQZmBssHMAeVB/oIXwjECSkJjgnzClgKvQsiC4cL7AxRDLYNGw2ADeUOSgAAAAAAAAANAQAABAAAAAAAAAABAAAABAAAAAABAQAEAAAAAAAAAAEAAAADAAAAAAECAAMAAAAAAAAAAwAQABAAEAAAAQMAAwAAAAAAAAABAAEAAAAAAAABBgADAAAAAAAAAAEAAgAAAAAAAAERABAAAAAAAAAAAQAAAAAAAAKQARUAAwAAAAAAAAABAAMAAAAAAAABFgAEAAAAAAAAAAEAAAADAAAAAAEXABAAAAAAAAAAAQAAAAAAAABIARoABQAAAAAAAAABAAAAAQAAAAEBGwAFAAAAAAAAAAEAAAABAAAAAQEcAAMAAAAAAAAAAQABAAAAAAAAASgAAwAAAAAAAAABAAEAAAAAAAAAAAAAAAAAAAAAAAAECARtBNIC2QM+A6MBqgIPAnQAewDgAUUIxAkpCY4HlQf6CF8GZgbLBzAFNwWcBgENgA3lDkoMUQy2DRsLIguHC+wJ8wpYCr0=");

    }
}
