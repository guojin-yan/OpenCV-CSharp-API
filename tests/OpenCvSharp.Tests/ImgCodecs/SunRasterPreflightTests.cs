using System;
using System.Collections.Generic;
using System.IO;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class SunRasterPreflightTests
    {
        [Theory]
        [MemberData(nameof(RealSunRasterFixtures))]
        public void IdentifyReadsRealSunRasterStorageFixtures(string name, int width, int height, int depth,
            int length, string sha256, string encoded)
        {
            Assert.NotEmpty(name);
            byte[] data = Convert.FromBase64String(encoded);
            Assert.Equal(length, data.Length);
            Assert.Equal(sha256, Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(data)).ToLowerInvariant());
            AssertComplete(ImgCodecsCv2.Identify(data), width, height, depth);
            AssertComplete(ImgCodecsCv2.Identify(new ReadOnlySpan<byte>(data)), width, height, depth);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(8)]
        [InlineData(24)]
        [InlineData(32)]
        public void IdentifyCountsSunRasterRleAcrossRowsForEverySupportedDepth(int depth)
        {
            int length = RowBytes(3, depth) * 3;
            byte[] data = Raster(3, 3, depth, 2, new byte[] { 128, (byte)(length - 1), 128 });
            AssertComplete(ImgCodecsCv2.Identify(data), 3, 3, depth);
        }

        [Fact]
        public void IdentifyReadsEverySunRasterRleCountAndLiteralByte()
        {
            for (int count = 1; count <= 255; ++count)
            {
                int width = count + 1;
                byte[] payload = new byte[] { 128, (byte)count, 128 };
                if ((width & 1) != 0) payload = Join(payload, new byte[] { 0 });
                AssertComplete(ImgCodecsCv2.Identify(Raster(width, 1, 8, 2, payload)), width, 1, 8);
            }
            for (int value = 0; value <= 255; ++value)
            {
                byte[] payload = value == 128 ? new byte[] { 128, 0, 0 } : new byte[] { (byte)value, 0 };
                AssertComplete(ImgCodecsCv2.Identify(Raster(1, 1, 8, 2, payload)), 1, 1, 8);
            }
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 8)]
        [InlineData(1, 9)]
        [InlineData(1, 15)]
        [InlineData(1, 16)]
        [InlineData(1, 17)]
        [InlineData(8, 1)]
        [InlineData(8, 3)]
        [InlineData(24, 1)]
        [InlineData(24, 3)]
        [InlineData(32, 1)]
        [InlineData(32, 3)]
        public void IdentifyRequiresSunRasterSixteenBitPaddedRows(int depth, int width)
        {
            int length = RowBytes(width, depth) * 2;
            AssertComplete(ImgCodecsCv2.Identify(Raster(width, 2, depth, 2, Encode(new byte[length]))), width, 2, depth);
            foreach (int invalid in new[] { length - 1, length + 1 })
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(Raster(width, 2, depth, 2, Encode(new byte[invalid])));
                AssertHeader(result, width, 2, depth);
                AssertNoFrame(result);
            }
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(0, 8)]
        [InlineData(0, 24)]
        [InlineData(0, 32)]
        [InlineData(1, 1)]
        [InlineData(1, 8)]
        [InlineData(1, 24)]
        [InlineData(1, 32)]
        [InlineData(3, 1)]
        [InlineData(3, 8)]
        [InlineData(3, 24)]
        [InlineData(3, 32)]
        public void IdentifyPreservesSunRasterRawOldAndRgbLengthRules(int type, int depth)
        {
            byte[] data = Raster(3, 2, depth, type, new byte[RowBytes(3, depth) * 2]);
            AssertComplete(ImgCodecsCv2.Identify(data), 3, 2, depth);
            WriteBe32(data, 16, 0);
            ImageIdentifyResult zeroLength = ImgCodecsCv2.Identify(data);
            AssertHeader(zeroLength, 3, 2, depth);
            if (type == 0) AssertComplete(zeroLength, 3, 2, depth);
            else AssertNoFrame(zeroLength);
            WriteBe32(data, 16, (uint)(data.Length - 33));
            AssertNoFrame(ImgCodecsCv2.Identify(data));
        }

        [Theory]
        [InlineData("")]
        [InlineData("80")]
        [InlineData("8002")]
        [InlineData("8000")]
        [InlineData("800207")]
        [InlineData("800407")]
        [InlineData("010203")]
        [InlineData("0102030405")]
        [InlineData("80030700")]
        [InlineData("8003078000")]
        public void IdentifyRejectsIncompleteOrOverlongSunRasterEscapeStreams(string hex)
        {
            byte[] data = Raster(3, 1, 8, 2, Convert.FromHexString(hex));
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            AssertHeader(result, 3, 1, 8);
            AssertNoFrame(result);
        }

        [Theory]
        [InlineData(20, 4u)]
        [InlineData(20, 5u)]
        [InlineData(20, uint.MaxValue)]
        [InlineData(24, 3u)]
        [InlineData(24, uint.MaxValue)]
        [InlineData(28, 1u)]
        [InlineData(28, uint.MaxValue)]
        [InlineData(16, 0u)]
        [InlineData(16, 2u)]
        [InlineData(16, 4u)]
        [InlineData(16, uint.MaxValue)]
        public void IdentifyKeepsSunRasterHeaderFactsWhenPayloadDeclarationsAreUnknown(int offset, uint value)
        {
            byte[] data = Raster(3, 3, 24, 2, new byte[] { 128, 29, 7 });
            WriteBe32(data, offset, value);
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            AssertHeader(result, 3, 3, 24);
            AssertNoFrame(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(16)]
        [InlineData(int.MaxValue)]
        public void IdentifyLeavesUnsupportedSunRasterDepthUnknown(int depth)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Raster(3, 3, depth, 2, new byte[] { 128, 29, 7 }));
            Assert.True(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown);
            AssertNoFrame(result);
        }

        [Theory]
        [InlineData(4, 0u)]
        [InlineData(8, 0u)]
        [InlineData(4, 0x80000000u)]
        [InlineData(8, 0x80000000u)]
        [InlineData(4, uint.MaxValue)]
        [InlineData(8, uint.MaxValue)]
        public void IdentifyRejectsInvalidSunRasterGeometry(int offset, uint value)
        {
            byte[] data = Raster(3, 3, 8, 2, new byte[] { 128, 11, 7 });
            WriteBe32(data, offset, value);
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            Assert.Equal("sunraster", result.Format);
            Assert.False(result.IsSizeKnown);
            Assert.False(result.IsPixelFormatKnown);
            AssertNoFrame(result);
        }

        [Fact]
        public void IdentifyRequiresExactSunRasterFileBoundaryAndPreservesProvenHeaders()
        {
            foreach (object[] fixture in RealSunRasterFixtures)
            {
                int width = (int)fixture[1], height = (int)fixture[2], depth = (int)fixture[3];
                byte[] data = Convert.FromBase64String((string)fixture[6]);
                for (int length = 1; length < data.Length; ++length)
                {
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(Slice(data, length));
                    AssertNoFrame(result);
                    if (length >= 32) AssertHeader(result, width, height, depth);
                    else
                    {
                        Assert.False(result.IsSizeKnown);
                        Assert.False(result.IsPixelFormatKnown);
                    }
                }
                AssertNoFrame(ImgCodecsCv2.Identify(Join(data, new byte[] { 0 })));
                AssertNoFrame(ImgCodecsCv2.Identify(Join(data, data)));
            }
        }

        [Fact]
        public void IdentifyBoundsSunRasterMapsAndCheckedGeometryWithoutAllocating()
        {
            byte[] payload = new byte[] { 128, 29, 7 };
            foreach (int mapType in new[] { 0, 1, 2 })
            {
                byte[] data = Raster(3, 3, 24, 2, payload, mapType, new byte[] { 1, 2, 3, 4, 5, 6 });
                ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
                if (mapType == 0) AssertNoFrame(result);
                else AssertComplete(result, 3, 3, 24);
                WriteBe32(data, 28, mapType == 1 ? 5u : uint.MaxValue);
                AssertNoFrame(ImgCodecsCv2.Identify(data));
            }
            foreach (int depth in new[] { 1, 8, 24, 32 })
            {
                byte[] data = Raster(int.MaxValue, int.MaxValue, depth, 2, payload);
                long before = GC.GetAllocatedBytesForCurrentThread();
                ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                AssertHeader(result, int.MaxValue, int.MaxValue, depth);
                AssertNoFrame(result);
                Assert.InRange(allocated, 0, 32768);
            }
        }

        [Fact]
        public void IdentifyCountsSunRasterRleBeyondInt32WithoutAllocatingExpandedRaster()
        {
            const int width = 65536, height = 32769;
            const long expanded = (long)width * height;
            byte[] data = new byte[checked(32 + (int)(expanded / 256) * 3)];
            WriteBe32(data, 0, 0x59A66A95);
            WriteBe32(data, 4, width);
            WriteBe32(data, 8, height);
            WriteBe32(data, 12, 8);
            WriteBe32(data, 16, (uint)(data.Length - 32));
            WriteBe32(data, 20, 2);
            for (int offset = 32; offset < data.Length; offset += 3)
            {
                data[offset] = 128;
                data[offset + 1] = 255;
            }
            // Warm the public path before measuring allocation; input is about 24 MiB.
            ImgCodecsCv2.Identify(Raster(1, 1, 8, 2, new byte[] { 0, 0 }));
            long before = GC.GetAllocatedBytesForCurrentThread();
            ImageIdentifyResult result = ImgCodecsCv2.Identify(data);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            AssertComplete(result, width, height, 8);
            Assert.True(result.EstimatedPixelBytes > int.MaxValue);
            Assert.InRange(allocated, 0, 32768);
            ImageDecodeOptions limit = Options(data.Length, width, height, expanded, expanded, 8, 1, int.MaxValue);
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(data, limit));
            // The last run must neither underfill nor overrun the 64-bit total.
            data[data.Length - 2] = 254;
            AssertNoFrame(ImgCodecsCv2.Identify(data));
            data[data.Length - 2] = 255;
            WriteBe32(data, 8, height - 1);
            AssertNoFrame(ImgCodecsCv2.Identify(data));
        }

        [Fact]
        public void SunRasterBudgetsRejectBeforeNativeDecodeAndPreserveStreams()
        {
            foreach (int depth in new[] { 1, 8, 24, 32 })
            {
                int channels = Channels(depth);
                byte[] data = Raster(3, 3, depth, 2, Encode(new byte[RowBytes(3, depth) * 3]));
                long storage = 9L * channels; // Each encoded sample rounds up to one byte, including depth 1.
                var limits = new List<ImageDecodeOptions>
                {
                    Options(data.Length - 1, 3, 3, 9, 9, 8, channels, storage),
                    Options(data.Length, 2, 3, 9, 9, 8, channels, storage),
                    Options(data.Length, 3, 2, 9, 9, 8, channels, storage),
                    Options(data.Length, 3, 3, 8, 9, 8, channels, storage),
                    Options(data.Length, 3, 3, 9, 8, 8, channels, storage),
                    Options(data.Length, 3, 3, 9, 9, 8, channels, storage - 1)
                };
                if (depth != 1) limits.Add(Options(data.Length, 3, 3, 9, 9, 7, channels, storage));
                if (channels > 1) limits.Add(Options(data.Length, 3, 3, 9, 9, 8, channels - 1, storage));
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
                ImageDecodeOptions exact = Options(data.Length, 3, 3, 9, 9, 8, channels, storage);
                using (var stream = new MemoryStream(Join(new byte[] { 0 }, data)))
                {
                    stream.Position = 1;
                    AssertComplete(ImgCodecsCv2.Identify(stream, exact), 3, 3, depth);
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadStream(data)) AssertComplete(ImgCodecsCv2.Identify(stream, exact), 3, 3, depth);
            }
        }

        [Fact]
        public void SunRasterStrictPolicyRejectsUnknownHeaderOrMetadataBeforeNativeDecode()
        {
            byte[] valid = Raster(3, 3, 24, 2, new byte[] { 128, 29, 7 });
            byte[] unknownDepth = (byte[])valid.Clone();
            WriteBe32(unknownDepth, 12, 4);
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(unknownDepth, Options(valid.Length, 3, 3, 9, 9, 8, 3, 27)));
            ImageDecodeOptions metadata = new ImageDecodeOptions(4096, 8, 8, 64, 1, true, true,
                long.MaxValue, long.MaxValue, true, true, long.MaxValue, 8, 4, true, long.MaxValue);
            Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(valid, metadata));
            using (var stream = new MemoryStream(valid))
            {
                Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.Identify(stream, metadata));
                Assert.Equal(0, stream.Position);
            }
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusSunRasterNeverEscapesOrPublishesInconsistentStorageFacts()
        {
            foreach (object[] fixture in RealSunRasterFixtures)
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            {
                byte[] data = Convert.FromBase64String((string)fixture[6]);
                for (int offset = 0; offset < data.Length; ++offset)
                {
                    if (data[offset] == replacement) continue;
                    byte[] mutated = (byte[])data.Clone();
                    mutated[offset] = replacement;
                    ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                    if (result.IsSizeKnown)
                    {
                        Assert.InRange(result.Width, 1, int.MaxValue);
                        Assert.InRange(result.Height, 1, int.MaxValue);
                    }
                    if (result.IsPixelFormatKnown)
                    {
                        Assert.True(result.IsSizeKnown);
                        Assert.True(result.BitDepth == 1 || result.BitDepth == 8);
                        Assert.True(result.ChannelCount == 1 || result.ChannelCount == 3 || result.ChannelCount == 4);
                        if (result.BitDepth == 1) Assert.Equal(1, result.ChannelCount);
                    }
                    if (result.IsFrameCountKnown)
                    {
                        Assert.True(result.IsPixelFormatKnown);
                        Assert.Equal(1, result.FrameCount);
                        Assert.True(result.IsCumulativePixelCountKnown);
                        Assert.True(result.IsEstimatedPixelBytesKnown);
                        Assert.Equal((long)result.Width * result.Height, result.CumulativePixelCount);
                        Assert.Equal(result.CumulativePixelCount * result.ChannelCount, result.EstimatedPixelBytes);
                    }
                    else AssertNoFrame(result);
                    Assert.False(result.IsMetadataSizeKnown);
                    Assert.False(result.IsIccProfileSizeKnown);
                }
            }
        }

        // All padded storage bytes independently decoded by Netpbm's unchanged
        // loadRgb/loadByteEncoded functions; ten files also checked by Pillow.
        // A four-byte 32-bit pixel includes a padding byte, without an alpha claim.
        public static IEnumerable<object[]> RealSunRasterFixtures
        {
            get
            {
                yield return new object[] { "rle1", 9, 3, 1, 40, "315ca9e971ab059628162eb3b2a09a53fa7d90307ce6e4a78ab9e4c15004152a", "WaZqlQAAAAkAAAADAAAAAQAAAAgAAAACAAAAAAAAAACAAAARIoABgA==" };
                yield return new object[] { "rle8", 4, 3, 8, 46, "7f747562002a0aa50c41aae71cf5af26c18d57db46584cd21e665823f6699554", "WaZqlQAAAAQAAAADAAAACAAAAA4AAAACAAAAAAAAAACAAAARIoABgP+AAjMNCg==" };
                yield return new object[] { "rle24", 4, 2, 24, 60, "892776ed1e7f56010d01e67cadfcf13bea110f1b07f4ab97cdcf86baf45a7d05", "WaZqlQAAAAQAAAACAAAAGAAAABwAAAACAAAAAAAAAACAAAARIoABgP+AAjMNCoACAIAAABEigAGA/zMz" };
                yield return new object[] { "rle32", 2, 2, 32, 51, "63901072faa467ceb36378cff15d7153e6b52fff4b16071ef7cd49e6157a862b", "WaZqlQAAAAIAAAACAAAAIAAAABMAAAACAAAAAAAAAACAAAARIoABgP+AAjMNCoACAIAA" };
                yield return new object[] { "cross1", 17, 3, 1, 35, "5e4e91fbf8885719b1dc5b8106ce9a4b6c2c47285d3d8eb07de4f80a3cb98d23", "WaZqlQAAABEAAAADAAAAAQAAAAMAAAACAAAAAAAAAACACzM=" };
                yield return new object[] { "cross8", 4, 3, 8, 35, "7f777d7d0584efc89f33232bd02ecd842549fd9d700e39601edf1e2438e59a2b", "WaZqlQAAAAQAAAADAAAACAAAAAMAAAACAAAAAAAAAACACzM=" };
                yield return new object[] { "cross24", 4, 3, 24, 35, "0587d1653427eb9143ff4f288a8f937ad0fad75d541ec9a0d126cbcfc5232b0e", "WaZqlQAAAAQAAAADAAAAGAAAAAMAAAACAAAAAAAAAACAIzM=" };
                yield return new object[] { "cross32", 2, 3, 32, 35, "ba3d737fd307f763a9537fb61544abd7949b53b6f315edde8098f257607fb33e", "WaZqlQAAAAIAAAADAAAAIAAAAAMAAAACAAAAAAAAAACAFzM=" };
                yield return new object[] { "old1", 9, 3, 1, 38, "46c40df9e4b9549647e325f89e20078d3be56b0535a48a59e472379923d4bcbc", "WaZqlQAAAAkAAAADAAAAAQAAAAAAAAAAAAAAAAAAAACAABEigIA=" };
                yield return new object[] { "standard8", 3, 3, 8, 44, "3a4a736087263de7c676c5420bc016c02b0a19689e0f978a20a2ef9aee3c1e07", "WaZqlQAAAAMAAAADAAAACAAAAAwAAAABAAAAAAAAAACAABEigID/MzMzDQo=" };
                yield return new object[] { "rgb24", 3, 2, 24, 52, "ff2044cfb0ce262f276f560d823a8835fa1aa8204afbb3bd447b1cd09804f17d", "WaZqlQAAAAMAAAACAAAAGAAAABQAAAADAAAAAAAAAACAABEigID/MzMzDQoAAACAABEigA==" };
                yield return new object[] { "rgb32", 2, 2, 32, 48, "9064876d3c3d2fb5294146e2e663b7769399aece0be534ac3fe7aec652f4f8f2", "WaZqlQAAAAIAAAACAAAAIAAAABAAAAADAAAAAAAAAACAABEigID/MzMzDQoAAACA" };
                yield return new object[] { "palette8", 4, 3, 8, 814, "c0d2b4ed884a98e5d8bbb3b6aaaa8217eeef6037cbf18c0079d53adce3101262", "WaZqlQAAAAQAAAADAAAACAAAAA4AAAACAAAAAQAAAwAAAQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyAhIiMkJSYnKCkqKywtLi8wMTIzNDU2Nzg5Ojs8PT4/QEFCQ0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4eXp7fH1+f4CBgoOEhYaHiImKi4yNjo+QkZKTlJWWl5iZmpucnZ6foKGio6SlpqeoqaqrrK2ur7CxsrO0tba3uLm6u7y9vr/AwcLDxMXGx8jJysvMzc7P0NHS09TV1tfY2drb3N3e3+Dh4uPk5ebn6Onq6+zt7u/w8fLz9PX29/j5+vv8/f7/AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8gISIjJCUmJygpKissLS4vMDEyMzQ1Njc4OTo7PD0+P0BBQkNERUZHSElKS0xNTk9QUVJTVFVWV1hZWltcXV5fYGFiY2RlZmdoaWprbG1ub3BxcnN0dXZ3eHl6e3x9fn+AgYKDhIWGh4iJiouMjY6PkJGSk5SVlpeYmZqbnJ2en6ChoqOkpaanqKmqq6ytrq+wsbKztLW2t7i5uru8vb6/wMHCw8TFxsfIycrLzM3Oz9DR0tPU1dbX2Nna29zd3t/g4eLj5OXm5+jp6uvs7e7v8PHy8/T19vf4+fr7/P3+/wABAgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4fICEiIyQlJicoKSorLC0uLzAxMjM0NTY3ODk6Ozw9Pj9AQUJDREVGR0hJSktMTU5PUFFSU1RVVldYWVpbXF1eX2BhYmNkZWZnaGlqa2xtbm9wcXJzdHV2d3h5ent8fX5/gIGCg4SFhoeIiYqLjI2Oj5CRkpOUlZaXmJmam5ydnp+goaKjpKWmp6ipqqusra6vsLGys7S1tre4ubq7vL2+v8DBwsPExcbHyMnKy8zNzs/Q0dLT1NXW19jZ2tvc3d7f4OHi4+Tl5ufo6err7O3u7/Dx8vP09fb3+Pn6+/z9/v+AAAARIoABgP+AAjMNCg==" };
                yield return new object[] { "rawmap24", 3, 2, 24, 61, "0890119af8ecd93fb83db08fbf9639584abb7beec8486c0a5b6f37b91960ee77", "WaZqlQAAAAMAAAACAAAAGAAAABgAAAACAAAAAgAAAAWAAA0K/4AAABEigAGA/4ACMw0KgAIAgAAAESKAAA==" };
                yield return new object[] { "rgbmap24", 3, 2, 24, 824, "5afa71e69760032dcc2e08dc35724e5717fa1e3d70e1449d7398adc5fc7c5a0a", "WaZqlQAAAAMAAAACAAAAGAAAABgAAAACAAAAAQAAAwAAAQIDBAUGBwgJCgsMDQ4PEBESExQVFhcYGRobHB0eHyAhIiMkJSYnKCkqKywtLi8wMTIzNDU2Nzg5Ojs8PT4/QEFCQ0RFRkdISUpLTE1OT1BRUlNUVVZXWFlaW1xdXl9gYWJjZGVmZ2hpamtsbW5vcHFyc3R1dnd4eXp7fH1+f4CBgoOEhYaHiImKi4yNjo+QkZKTlJWWl5iZmpucnZ6foKGio6SlpqeoqaqrrK2ur7CxsrO0tba3uLm6u7y9vr/AwcLDxMXGx8jJysvMzc7P0NHS09TV1tfY2drb3N3e3+Dh4uPk5ebn6Onq6+zt7u/w8fLz9PX29/j5+vv8/f7/AAECAwQFBgcICQoLDA0ODxAREhMUFRYXGBkaGxwdHh8gISIjJCUmJygpKissLS4vMDEyMzQ1Njc4OTo7PD0+P0BBQkNERUZHSElKS0xNTk9QUVJTVFVWV1hZWltcXV5fYGFiY2RlZmdoaWprbG1ub3BxcnN0dXZ3eHl6e3x9fn+AgYKDhIWGh4iJiouMjY6PkJGSk5SVlpeYmZqbnJ2en6ChoqOkpaanqKmqq6ytrq+wsbKztLW2t7i5uru8vb6/wMHCw8TFxsfIycrLzM3Oz9DR0tPU1dbX2Nna29zd3t/g4eLj5OXm5+jp6uvs7e7v8PHy8/T19vf4+fr7/P3+/wABAgMEBQYHCAkKCwwNDg8QERITFBUWFxgZGhscHR4fICEiIyQlJicoKSorLC0uLzAxMjM0NTY3ODk6Ozw9Pj9AQUJDREVGR0hJSktMTU5PUFFSU1RVVldYWVpbXF1eX2BhYmNkZWZnaGlqa2xtbm9wcXJzdHV2d3h5ent8fX5/gIGCg4SFhoeIiYqLjI2Oj5CRkpOUlZaXmJmam5ydnp+goaKjpKWmp6ipqqusra6vsLGys7S1tre4ubq7vL2+v8DBwsPExcbHyMnKy8zNzs/Q0dLT1NXW19jZ2tvc3d7f4OHi4+Tl5ufo6err7O3u7/Dx8vP09fb3+Pn6+/z9/v+AAAARIoABgP+AAjMNCoACAIAAABEigAA=" };
                yield return new object[] { "maxrun8", 256, 1, 8, 35, "69905f4588ad4e1b1d2e1acb8722ce48066d256b4d1579f4b0b94b8f97c30f62", "WaZqlQAAAQAAAAABAAAACAAAAAMAAAACAAAAAAAAAACA/4A=" };
            }
        }

        private static ImageDecodeOptions Options(int length, int width, int height, long pixels,
            long cumulative, int depth, int channels, long storage)
        {
            return new ImageDecodeOptions(length, width, height, pixels, 1, true, true, long.MaxValue, long.MaxValue,
                false, false, cumulative, depth, channels, true, storage);
        }

        private static int RowBytes(int width, int depth) { return ((width * depth + 15) / 16) * 2; }
        private static int Channels(int depth) { return depth == 24 ? 3 : depth == 32 ? 4 : 1; }
        private static byte[] Raster(int width, int height, int depth, int type, byte[] payload,
            int mapType = 0, byte[]? map = null)
        {
            map = map ?? Array.Empty<byte>();
            byte[] data = new byte[32 + map.Length + payload.Length];
            WriteBe32(data, 0, 0x59A66A95);
            WriteBe32(data, 4, (uint)width);
            WriteBe32(data, 8, (uint)height);
            WriteBe32(data, 12, (uint)depth);
            WriteBe32(data, 16, (uint)payload.Length);
            WriteBe32(data, 20, (uint)type);
            WriteBe32(data, 24, (uint)mapType);
            WriteBe32(data, 28, (uint)map.Length);
            Array.Copy(map, 0, data, 32, map.Length);
            Array.Copy(payload, 0, data, 32 + map.Length, payload.Length);
            return data;
        }
        private static byte[] Encode(byte[] raw)
        {
            using (var stream = new MemoryStream())
            {
                for (int offset = 0; offset < raw.Length;)
                {
                    int count = 1;
                    while (count < 256 && offset + count < raw.Length && raw[offset + count] == raw[offset]) ++count;
                    if (count == 1 && raw[offset] == 128)
                    {
                        stream.WriteByte(128);
                        stream.WriteByte(0);
                    }
                    else if (count > 2 || raw[offset] == 128)
                    {
                        stream.WriteByte(128);
                        stream.WriteByte((byte)(count - 1));
                        stream.WriteByte(raw[offset]);
                    }
                    else for (int index = 0; index < count; ++index) stream.WriteByte(raw[offset]);
                    offset += count;
                }
                return stream.ToArray();
            }
        }
        private static void WriteBe32(byte[] data, int offset, uint value)
        {
            data[offset] = (byte)(value >> 24);
            data[offset + 1] = (byte)(value >> 16);
            data[offset + 2] = (byte)(value >> 8);
            data[offset + 3] = (byte)value;
        }
        private static byte[] Slice(byte[] data, int length)
        {
            byte[] copy = new byte[length];
            Array.Copy(data, copy, length);
            return copy;
        }
        private static byte[] Join(byte[] first, byte[] second)
        {
            byte[] data = new byte[first.Length + second.Length];
            Array.Copy(first, data, first.Length);
            Array.Copy(second, 0, data, first.Length, second.Length);
            return data;
        }
        private static void AssertHeader(ImageIdentifyResult result, int width, int height, int depth)
        {
            Assert.Equal("sunraster", result.Format);
            Assert.True(result.IsSizeKnown);
            Assert.True(result.IsPixelFormatKnown);
            Assert.Equal(width, result.Width);
            Assert.Equal(height, result.Height);
            Assert.Equal(depth == 1 ? 1 : 8, result.BitDepth);
            Assert.Equal(Channels(depth), result.ChannelCount);
            Assert.False(result.IsMetadataSizeKnown);
            Assert.False(result.IsIccProfileSizeKnown);
        }
        private static void AssertComplete(ImageIdentifyResult result, int width, int height, int depth)
        {
            AssertHeader(result, width, height, depth);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(1, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown);
            Assert.Equal((long)width * height, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown);
            Assert.Equal((long)width * height * Channels(depth), result.EstimatedPixelBytes);
        }
        private static void AssertNoFrame(ImageIdentifyResult result)
        {
            Assert.False(result.IsFrameCountKnown);
            Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
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
    }
}
