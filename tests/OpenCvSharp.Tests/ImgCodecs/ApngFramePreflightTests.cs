using System;
using System.IO;
using System.Text;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class ApngFramePreflightTests
    {
        [Fact]
        public void IdentifyDoesNotCountApngFramesOutsideTheCanvas()
        {
            byte[] png = Png(Actl(2), Control(0), Idat(1), Control(1, width: 5), Fdat(2, 1));
            AssertUnknownFrames(ImgCodecsCv2.Identify(png));
        }

        [Fact]
        public void IdentifyRequiresApngDefaultImageDataBeforeFrameData()
        {
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Actl(1), Control(0), Fdat(1, 1))));
        }

        [Fact]
        public void IdentifyNeverMixesApngIdatAndFdatInOneFrame()
        {
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Actl(1), Control(0), Idat(1), Fdat(1, 1))));
        }

        [Fact]
        public void IdentifyRequiresDefaultApngFrameToMatchTheWholeCanvas()
        {
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Actl(1), Control(0, width: 2), Idat(1))));
        }

        [Fact]
        public void IdentifyRequiresConsecutiveApngDefaultImageChunks()
        {
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Actl(1), Control(0), Idat(1), Chunk("tEXt", 65, 0, 66), Idat(2))));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyCountsApngAnimationFramesWithEitherDefaultImageRole(bool separateDefault)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Animation(separateDefault));
            AssertFrames(result, 2);
            // Every displayed frame uses the full canvas, including cropped fdAT rectangles.
            Assert.Equal(24, result.CumulativePixelCount);
            Assert.Equal(96, result.EstimatedPixelBytes);
        }

        [Fact]
        public void IdentifyAcceptsStaticAndSingleFramePngBoundaries()
        {
            AssertFrames(ImgCodecsCv2.Identify(Png(Idat(), Idat(1), Idat())), 1);
            AssertFrames(ImgCodecsCv2.Identify(Png(Actl(1), Control(0), Idat(1))), 1);
            AssertFrames(ImgCodecsCv2.Identify(Png(Actl(1), Idat(1), Control(0, 1, 1, 3, 2), Fdat(1, 2))), 1);
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Idat())));
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Actl(1), Control(0), Idat())));
        }

        [Theory]
        [InlineData(0U, 1U, 0U, 0U)]
        [InlineData(1U, 0U, 0U, 0U)]
        [InlineData(5U, 1U, 0U, 0U)]
        [InlineData(1U, 4U, 0U, 0U)]
        [InlineData(2U, 1U, 3U, 0U)]
        [InlineData(1U, 2U, 0U, 2U)]
        [InlineData(uint.MaxValue, 1U, 1U, 0U)]
        [InlineData(1U, uint.MaxValue, 0U, 1U)]
        [InlineData(1U, 1U, uint.MaxValue, 0U)]
        [InlineData(1U, 1U, 0U, uint.MaxValue)]
        public void IdentifyRejectsApngFrameRectanglesWithoutUnsignedWraparound(uint width, uint height, uint x, uint y)
        {
            byte[] png = Png(Actl(2), Control(0), Idat(1), Control(1, width, height, x, y), Fdat(2, 1));
            AssertUnknownFrames(ImgCodecsCv2.Identify(png));
        }

        [Theory]
        [InlineData(1U, 3U, 0U, 0U)]
        [InlineData(4U, 1U, 0U, 0U)]
        [InlineData(1U, 1U, 1U, 0U)]
        [InlineData(1U, 1U, 0U, 1U)]
        public void IdentifyRejectsCroppedDefaultApngFrameEvenWhenItFitsTheCanvas(uint width, uint height, uint x, uint y)
        {
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Actl(1), Control(0, width, height, x, y), Idat(1))));
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 1)]
        [InlineData(2, 0)]
        [InlineData(2, 1)]
        public void IdentifyAcceptsAllDefinedApngDisposalAndBlendOperations(byte dispose, byte blend)
        {
            // PREVIOUS (2) on the first frame is defined to behave as BACKGROUND, so it is accepted.
            AssertFrames(ImgCodecsCv2.Identify(Png(Actl(1), Control(0, dispose: dispose, blend: blend), Idat(1))), 1);
            AssertFrames(ImgCodecsCv2.Identify(Png(Actl(1), Idat(1),
                Control(0, 1, 1, 3, 2, dispose, blend), Fdat(1, 1))), 1);
        }

        [Theory]
        [InlineData(3, 0)]
        [InlineData(255, 0)]
        [InlineData(0, 2)]
        [InlineData(0, 255)]
        public void IdentifyRejectsUndefinedApngDisposalAndBlendOperations(byte dispose, byte blend)
        {
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Actl(1), Control(0, dispose: dispose, blend: blend), Idat(1))));
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Actl(1), Idat(1),
                Control(0, 1, 1, 3, 2, dispose, blend), Fdat(1, 1))));
        }

        [Fact]
        public void IdentifyAcceptsZeroLengthApngDataFragmentsWithoutTreatingThemAsCompleteFrames()
        {
            byte[] png = Png(Actl(2), Control(0), Idat(), Idat(1), Idat(),
                Control(1, 1, 1, 3, 2), Fdat(2), Chunk("tEXt", 65, 0, 66), Fdat(3, 2), Fdat(4));
            AssertFrames(ImgCodecsCv2.Identify(png), 2);
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Actl(1), Idat(1), Control(0), Fdat(1))));
        }

        [Fact]
        public void IdentifyAcceptsApngZeroDelayDenominatorAndTheMaximumLoopCount()
        {
            byte[] control = Join(U32(0), U32(4), U32(3), U32(0), U32(0), new byte[] { 0, 0, 0, 0, 2, 1 });
            AssertFrames(ImgCodecsCv2.Identify(Png(Chunk("acTL", Join(U32(1), U32(int.MaxValue))),
                Chunk("fcTL", control), Idat(1))), 1);
        }

        [Fact]
        public void IdentifyRejectsMalformedApngControlSequencesAndDataTransitions()
        {
            var fixtures = new[]
            {
                new { Name = "zero-frame-count", Bytes = Png(Actl(0), Control(0), Idat(1)) },
                new { Name = "oversized-frame-count", Bytes = Png(Actl(uint.MaxValue), Control(0), Idat(1)) },
                new { Name = "incomplete-frame-count", Bytes = Png(Actl(int.MaxValue), Control(0), Idat(1)) },
                new { Name = "invalid-loop-count", Bytes = Png(Chunk("acTL", Join(U32(1), U32(uint.MaxValue))), Control(0), Idat(1)) },
                new { Name = "actl-short", Bytes = Png(Chunk("acTL", new byte[7]), Control(0), Idat(1)) },
                new { Name = "actl-long", Bytes = Png(Chunk("acTL", new byte[9]), Control(0), Idat(1)) },
                new { Name = "duplicate-actl", Bytes = Png(Actl(1), Actl(1), Control(0), Idat(1)) },
                new { Name = "actl-after-empty-idat", Bytes = Png(Idat(), Actl(1), Control(0), Idat(1)) },
                new { Name = "actl-after-idat", Bytes = Png(Idat(1), Actl(1), Control(0), Fdat(1, 1)) },
                new { Name = "missing-actl", Bytes = Png(Control(0), Idat(1)) },
                new { Name = "missing-fctl", Bytes = Png(Actl(1), Idat(1), Fdat(0, 1)) },
                new { Name = "fctl-short", Bytes = Png(Actl(1), Chunk("fcTL", new byte[25]), Idat(1)) },
                new { Name = "fctl-long", Bytes = Png(Actl(1), Chunk("fcTL", new byte[27]), Idat(1)) },
                new { Name = "first-sequence-gap", Bytes = Png(Actl(1), Control(1), Idat(1)) },
                new { Name = "first-sequence-large", Bytes = Png(Actl(1), Control(uint.MaxValue), Idat(1)) },
                new { Name = "frame-sequence-gap", Bytes = Png(Actl(2), Control(0), Idat(1), Control(2), Fdat(3, 1)) },
                new { Name = "frame-sequence-duplicate", Bytes = Png(Actl(2), Control(0), Idat(1), Control(0), Fdat(1, 1)) },
                new { Name = "data-sequence-gap", Bytes = Png(Actl(1), Idat(1), Control(0), Fdat(2, 1)) },
                new { Name = "data-sequence-duplicate", Bytes = Png(Actl(1), Idat(1), Control(0), Fdat(1, 1), Fdat(1, 1)) },
                new { Name = "missing-animated-data", Bytes = Png(Actl(1), Idat(1), Control(0)) },
                new { Name = "missing-first-frame-data", Bytes = Png(Actl(2), Control(0), Control(1), Idat(1)) },
                new { Name = "missing-last-frame-data", Bytes = Png(Actl(2), Control(0), Idat(1), Control(1)) },
                new { Name = "missing-default-image", Bytes = Png(Actl(1), Control(0), Fdat(1, 1)) },
                new { Name = "empty-default-image", Bytes = Png(Actl(1), Idat(), Control(0), Fdat(1, 1)) },
                new { Name = "excess-frame-count", Bytes = Png(Actl(1), Control(0), Idat(1), Control(1), Fdat(2, 1)) },
                new { Name = "short-frame-count", Bytes = Png(Actl(2), Control(0), Idat(1)) },
                new { Name = "idat-after-fdat", Bytes = Png(Actl(1), Idat(1), Control(0), Fdat(1, 1), Idat(1)) },
                new { Name = "idat-in-second-frame", Bytes = Png(Actl(2), Control(0), Idat(1), Control(1), Idat(1)) },
                new { Name = "fdat-in-default-frame", Bytes = Png(Actl(1), Control(0), Idat(1), Fdat(1, 1)) },
                new { Name = "empty-idat-after-ancillary", Bytes = Png(Actl(1), Control(0), Idat(1), Chunk("tEXt", 65, 0, 66), Idat()) },
                new { Name = "short-fdat", Bytes = Png(Actl(1), Idat(1), Control(0), Chunk("fdAT", 0, 0, 0)) },
                new { Name = "empty-frame-data", Bytes = Png(Actl(1), Idat(1), Control(0), Fdat(1)) }
            };
            foreach (var fixture in fixtures)
            {
                ImageIdentifyResult result = ImgCodecsCv2.Identify(fixture.Bytes);
                Assert.False(result.IsFrameCountKnown, fixture.Name);
                AssertUnknownFrames(result);
            }
        }

        [Fact]
        public void IdentifyRequiresACompleteSupportedPngCanvasBeforeCountingFrames()
        {
            byte[] valid = Animation(false);
            foreach (int field in new[] { 16, 20, 24, 25, 26, 27, 28 })
            {
                byte[] invalid = (byte[])valid.Clone();
                invalid[field] = field < 24 ? (byte)128 : (byte)255;
                AssertNoCompletion(ImgCodecsCv2.Identify(invalid));
            }
            foreach (int field in new[] { 19, 23 })
            {
                byte[] invalid = (byte[])valid.Clone();
                invalid[field] = 0;
                AssertNoCompletion(ImgCodecsCv2.Identify(invalid));
            }
            AssertUnknownFrames(ImgCodecsCv2.Identify(Png(Chunk("IHDR", new byte[13]), Idat(1))));
            AssertNoCompletion(ImgCodecsCv2.Identify(Join(Signature, Chunk("acTL", new byte[8]), Chunk("IEND"))));
        }

        [Fact]
        public void IdentifyKeepsApngMetadataAndHeaderFactsIndependentOfFrameCompleteness()
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(Png(Actl(1), Control(0, width: 2), Idat(1), Chunk("tEXt", 65, 0, 66)));
            AssertUnknownFrames(result);
            Assert.True(result.IsMetadataSizeKnown);
            Assert.Equal(3, result.MetadataBytes);
            Assert.True(result.IsIccProfileSizeKnown);
            Assert.Equal(0, result.IccProfileBytes);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ApngFrameBudgetsRejectBeforeNativeDecodeAndPreserveStreams(bool separateDefault)
        {
            byte[] png = Animation(separateDefault);
            ImageDecodeOptions exact = Options(png.Length);
            ImageDecodeOptions frames = Options(png.Length, frames: 1);
            ImageDecodeOptions pixels = Options(png.Length, cumulative: 23);
            ImageDecodeOptions storage = Options(png.Length, storage: 95);
            foreach (var pair in new[]
            {
                new { Options = frames, Error = "frame count" },
                new { Options = pixels, Error = "cumulative pixel count" },
                new { Options = storage, Error = "pixel storage" }
            })
            {
                Assert.Contains(pair.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(png, pair.Options)).Message);
                Assert.Contains(pair.Error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(png), pair.Options)).Message);
            }
            foreach (ImageDecodeOptions options in new[] { exact, frames, pixels, storage })
            {
                using (var stream = new MemoryStream(Join(new byte[] { 42 }, png)))
                {
                    stream.Position = 1;
                    if (options == exact) AssertFrames(ImgCodecsCv2.Identify(stream, options), 2);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(1, stream.Position);
                }
                using (var stream = new ShortReadNonSeekableStream(png))
                {
                    if (options == exact) AssertFrames(ImgCodecsCv2.Identify(stream, options), 2);
                    else Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options));
                    Assert.Equal(png.Length, stream.BytesRead);
                }
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void ApngHugeCanvasBudgetsRejectCumulativeOrStorageOverflowBeforeNativeDecode(int frames)
        {
            byte[] png = Join(Signature,
                Chunk("IHDR", Join(U32(int.MaxValue), U32(int.MaxValue), new byte[] { 16, 6, 0, 0, 0 })),
                Actl((uint)frames), Control(0, int.MaxValue, int.MaxValue), Idat(1),
                frames == 1 ? new byte[0] : Join(Control(1, int.MaxValue, int.MaxValue), Fdat(2, 1),
                    Control(3, int.MaxValue, int.MaxValue), Fdat(4, 1)), Chunk("IEND"));
            ImageIdentifyResult result = ImgCodecsCv2.Identify(png);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(frames, result.FrameCount);
            Assert.Equal(frames == 1, result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
            var options = new ImageDecodeOptions(png.Length, int.MaxValue, int.MaxValue, long.MaxValue,
                frames, true, true, long.MaxValue, long.MaxValue, false, false,
                long.MaxValue, 16, 4, true, long.MaxValue);
            string error = frames == 1 ? "pixel storage" : "cumulative pixel count";
            Assert.Contains(error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(png, options)).Message);
            Assert.Contains(error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(new ReadOnlySpan<byte>(png), options)).Message);
            using (var stream = new MemoryStream(png))
            {
                Assert.Contains(error, Assert.Throws<InvalidDataException>(() => ImgCodecsCv2.ImDecode(stream, options)).Message);
                Assert.Equal(0, stream.Position);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void IdentifyReadsRealApngFramesWithCroppedRectanglesAndEitherDefaultImageRole(bool separateDefault)
        {
            ImageIdentifyResult result = ImgCodecsCv2.Identify(separateDefault ? RealSeparateDefault : RealIncludedDefault);
            AssertFrames(result, separateDefault ? 2 : 3);
            Assert.Equal(separateDefault ? 24 : 36, result.CumulativePixelCount);
            Assert.Equal(separateDefault ? 96 : 144, result.EstimatedPixelBytes);
        }

        [Fact]
        public void IdentifyKeepsApngCompletionUnknownForEveryPrefixAndTrailingContainer()
        {
            foreach (byte[] png in new[] { Animation(false), Animation(true), RealIncludedDefault, RealSeparateDefault })
            {
                for (int length = 1; length < png.Length; ++length)
                {
                    byte[] prefix = new byte[length];
                    Array.Copy(png, prefix, length);
                    AssertNoCompletion(ImgCodecsCv2.Identify(prefix));
                }
                foreach (byte[] trailing in new[] { new byte[] { 0 }, Chunk("IEND"), png })
                    AssertUnknownFrames(ImgCodecsCv2.Identify(Join(png, trailing)));
                foreach (int length in new[] { 0, 1, 0x7FFFFFFF, -1 })
                {
                    byte[] invalid = (byte[])png.Clone();
                    Array.Copy(U32(unchecked((uint)length)), 0, invalid, 8, 4);
                    AssertNoCompletion(ImgCodecsCv2.Identify(invalid));
                }
            }
        }

        [Fact]
        public void IdentifyDeterministicMutationCorpusApngFramesNeverEscapeOrPublishInvalidCanvasBudgets()
        {
            foreach (byte[] png in new[] { Animation(false), Animation(true), RealIncludedDefault, RealSeparateDefault })
            foreach (byte replacement in new byte[] { 0, 1, 127, 128, 255 })
            for (int offset = 0; offset < png.Length; ++offset)
            {
                if (png[offset] == replacement) continue;
                byte[] mutated = (byte[])png.Clone();
                mutated[offset] = replacement;
                ImageIdentifyResult result = ImgCodecsCv2.Identify(mutated);
                if (!result.IsFrameCountKnown)
                {
                    Assert.False(result.IsCumulativePixelCountKnown);
                    Assert.False(result.IsEstimatedPixelBytesKnown);
                    continue;
                }
                Assert.Equal("png", result.Format);
                Assert.True(result.IsSizeKnown);
                Assert.True(result.IsPixelFormatKnown);
                Assert.InRange(result.FrameCount, 1, mutated.Length / 12);
                Assert.Equal((long)result.Width * result.Height * result.FrameCount, result.CumulativePixelCount);
                Assert.True(result.IsEstimatedPixelBytesKnown);
                Assert.Equal(result.CumulativePixelCount * ((result.BitDepth + 7) / 8) * result.ChannelCount, result.EstimatedPixelBytes);
                Assert.Equal((byte)'I', mutated[mutated.Length - 8]);
                Assert.Equal((byte)'E', mutated[mutated.Length - 7]);
                Assert.Equal((byte)'N', mutated[mutated.Length - 6]);
                Assert.Equal((byte)'D', mutated[mutated.Length - 5]);
            }
        }

        private static ImageDecodeOptions Options(int length, int frames = 2, long cumulative = 24, long storage = 96)
        {
            return new ImageDecodeOptions(length, 4, 3, 12, frames, true, true, 1024, 1024, false, false,
                cumulative, 8, 4, true, storage);
        }

        private static byte[] Animation(bool separateDefault)
        {
            return separateDefault
                ? Png(Actl(2), Idat(1), Control(0, 1, 1, 3, 2), Fdat(1, 2), Control(2, 2, 2, 2, 1), Fdat(3, 3))
                : Png(Actl(2), Control(0), Idat(1), Control(1, 1, 1, 3, 2), Fdat(2, 2));
        }

        private static void AssertFrames(ImageIdentifyResult result, int count)
        {
            AssertHeader(result);
            Assert.True(result.IsFrameCountKnown);
            Assert.Equal(count, result.FrameCount);
            Assert.True(result.IsCumulativePixelCountKnown);
            Assert.Equal(12L * count, result.CumulativePixelCount);
            Assert.True(result.IsEstimatedPixelBytesKnown);
            Assert.Equal(48L * count, result.EstimatedPixelBytes);
        }

        private static void AssertUnknownFrames(ImageIdentifyResult result)
        {
            AssertHeader(result);
            AssertNoCompletion(result);
        }

        private static void AssertHeader(ImageIdentifyResult result)
        {
            Assert.Equal("png", result.Format);
            Assert.True(result.IsSizeKnown);
            Assert.Equal(4, result.Width);
            Assert.Equal(3, result.Height);
            Assert.True(result.IsPixelFormatKnown);
            Assert.Equal(8, result.BitDepth);
            Assert.Equal(4, result.ChannelCount);
        }

        private static void AssertNoCompletion(ImageIdentifyResult result)
        {
            Assert.False(result.IsFrameCountKnown);
            Assert.Equal(0, result.FrameCount);
            Assert.False(result.IsCumulativePixelCountKnown);
            Assert.False(result.IsEstimatedPixelBytesKnown);
        }

        private static byte[] Png(params byte[][] chunks)
        {
            return Join(Signature,
                Chunk("IHDR", Join(U32(4), U32(3), new byte[] { 8, 6, 0, 0, 0 })), Join(chunks), Chunk("IEND"));
        }

        private static byte[] Actl(uint frames)
        {
            return Chunk("acTL", Join(U32(frames), U32(0)));
        }

        private static byte[] Control(uint sequence, uint width = 4, uint height = 3, uint x = 0, uint y = 0, byte dispose = 0, byte blend = 0)
        {
            return Chunk("fcTL", Join(U32(sequence), U32(width), U32(height), U32(x), U32(y), new byte[] { 0, 1, 0, 10, dispose, blend }));
        }

        private static byte[] Idat(params byte[] payload) { return Chunk("IDAT", payload); }
        private static byte[] Fdat(uint sequence, params byte[] payload) { return Chunk("fdAT", Join(U32(sequence), payload)); }
        private static byte[] U32(uint value) { return new byte[] { (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value }; }

        private static byte[] Chunk(string type, params byte[] payload)
        {
            byte[] body = Join(Encoding.ASCII.GetBytes(type), payload);
            uint crc = uint.MaxValue;
            foreach (byte value in body)
            {
                crc ^= value;
                for (int bit = 0; bit < 8; ++bit) crc = (crc >> 1) ^ ((crc & 1) == 0 ? 0 : 0xEDB88320U);
            }
            return Join(U32((uint)payload.Length), body, U32(~crc));
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

        private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

        // Pillow 13.0.0, three RGBA 4x3 images with one changed pixel in each later image.
        // Independently decoded every image and compared complete RGBA bytes with the originals.
        // Included default: 283 bytes, SHA256 95a0fdb2b1cb371833339df1c17374b2f989ed5409f108eadfec121624f484f1.
        private static readonly byte[] RealIncludedDefault = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADCAYAAAC09K7GAAAACGFjVEwAAAADAAAAAM7tusAAAAAaZmNUTAAAAAAAAAAEAAAAAwAAAAAAAAAAAAEAGQAA0hMR0gAAABVJREFUeNpjlAhY8J8BCTAxoAEMAQBhnQINxdKYUgAAABpmY1RMAAAAAQAAAAEAAAABAAAAAwAAAAIAAQAUAADZySNWAAAAEWZkQVQAAAACeNpjOCEX9R8ABTICQMZ8SS4AAAAaZmNUTAAAAAMAAAACAAAAAgAAAAIAAAABAAMAMgAAMBKwAAAAAB5mZEFUAAAABHjaY9A44fZfImDBf0aJgAX/GRgYGABCyAZFmWzT0wAAAABJRU5ErkJggg==");

        // Separate default: 258 bytes, SHA256 d26b80538e92af873c8c7938bb0831ec6b0d27887357865437897e747475c273.
        // acTL counts two animated frames; the IDAT still image is not part of that count.
        private static readonly byte[] RealSeparateDefault = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAQAAAADCAYAAAC09K7GAAAACGFjVEwAAAACAAAAAPONk3AAAAAVSURBVHjaY5QIWPCfAQkwMaABDAEAYZ0CDcXSmFIAAAAaZmNUTAAAAAAAAAAEAAAAAwAAAAAAAAAAAAEAFAAA2suCgQAAAB5mZEFUAAAAAXjaY5QIWPCfAQkwMaABFIEN53YxAABoOwRFz6mYZQAAABpmY1RMAAAAAgAAAAIAAAACAAAAAgAAAAEAAwAyAACrYVrUAAAAHmZkQVQAAAADeNpj0Djh9l8iYMF/RomABf8ZGBgYAELIBkVLuZVVAAAAAElFTkSuQmCC");
    }
}
