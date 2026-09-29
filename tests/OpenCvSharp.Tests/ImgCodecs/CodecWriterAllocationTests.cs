#if NETCOREAPP3_1_OR_GREATER
using System;
using System.Buffers;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public sealed class CodecWriterAllocationTests
    {
        [Fact]
        public void ReusedCallerOwnedWriterMaintainsPayloadAcrossNativeEncodes()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat source = new Mat(64, 64, MatType.CV_8UC3))
            {
                byte[] pixels = new byte[source.ByteLength];
                for (int index = 0; index < pixels.Length; index++)
                {
                    pixels[index] = (byte)((index * 37 + 19) % 256);
                }

                source.CopyFrom(pixels);
                byte[] expected = ImgCodecsCv2.ImEncode(".png", source);
                var writer = new FixedBufferWriter(Math.Max(4096, expected.Length));

                const int iterations = 64;
                for (int iteration = 0; iteration < iterations; iteration++)
                {
                    writer.Reset();
                    ImgCodecsCv2.ImEncodeTo(".png", source, writer);
                    Assert.Equal(expected.Length, writer.WrittenCount);
                    Assert.True(writer.WrittenSpan.SequenceEqual(expected));
                }

                Assert.Equal(iterations, writer.GetSpanCalls);
                Assert.Equal(iterations, writer.AdvanceCalls);
                Assert.Equal(expected.Length, writer.LastAdvanceCount);
            }
        }

        [Fact]
        public void AdvanceFaultCanBeRepeatedWithoutPoisoningTheNextEncode()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat source = new Mat(8, 8, MatType.CV_8UC1))
            {
                source.CopyFrom(new byte[ source.ByteLength ]);
                byte[] expected = ImgCodecsCv2.ImEncode(".png", source);
                var writer = new FixedBufferWriter(Math.Max(4096, expected.Length)) { ThrowOnAdvance = true };

                const int failedIterations = 16;
                for (int iteration = 0; iteration < failedIterations; iteration++)
                {
                    writer.Reset();
                    Assert.Throws<InvalidOperationException>(() => ImgCodecsCv2.ImEncodeTo(".png", source, writer));
                }

                writer.ThrowOnAdvance = false;
                writer.Reset();
                ImgCodecsCv2.ImEncodeTo(".png", source, writer);

                Assert.Equal(expected.Length, writer.WrittenCount);
                Assert.True(writer.WrittenSpan.SequenceEqual(expected));
                Assert.Equal(failedIterations + 1, writer.GetSpanCalls);
                Assert.Equal(failedIterations + 1, writer.AdvanceCalls);
            }
        }

        private sealed class FixedBufferWriter : IBufferWriter<byte>
        {
            private readonly byte[] buffer;

            public FixedBufferWriter(int capacity)
            {
                buffer = new byte[capacity];
            }

            public bool ThrowOnAdvance { get; set; }

            public int WrittenCount { get; private set; }

            public int GetSpanCalls { get; private set; }

            public int AdvanceCalls { get; private set; }

            public int LastAdvanceCount { get; private set; }

            public ReadOnlySpan<byte> WrittenSpan => buffer.AsSpan(0, WrittenCount);

            public void Reset()
            {
                WrittenCount = 0;
            }

            public void Advance(int count)
            {
                AdvanceCalls++;
                LastAdvanceCount = count;
                if (ThrowOnAdvance)
                {
                    throw new InvalidOperationException("advance failure");
                }
                if (count < 0 || count > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }

                WrittenCount = count;
            }

            public Memory<byte> GetMemory(int sizeHint = 0)
            {
                ValidateSizeHint(sizeHint);
                GetSpanCalls++;
                return buffer;
            }

            public Span<byte> GetSpan(int sizeHint = 0)
            {
                ValidateSizeHint(sizeHint);
                GetSpanCalls++;
                return buffer.AsSpan();
            }

            private void ValidateSizeHint(int sizeHint)
            {
                if (sizeHint < 0 || sizeHint > buffer.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(sizeHint));
                }
            }
        }
    }
}
#endif
