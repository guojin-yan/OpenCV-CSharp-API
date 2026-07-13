using System;
using OpenCvSharp.Core;
using OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = OpenCvSharp.ImgCodecs.Cv2;

namespace OpenCvSharp.Tests.ImgCodecs
{
    public class Cv2InteropTests
    {
        [Fact]
        public void ImEncodeAndImDecodeRoundTripPngWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            byte[] bgrPixels = new byte[]
            {
                0, 0, 255,
                0, 255, 0,
                255, 0, 0,
                255, 255, 255
            };

            using (Mat source = new Mat(2, 2, MatType.CV_8UC3))
            {
                source.CopyFrom(bgrPixels);

                byte[] encoded = ImgCodecsCv2.ImEncode(".png", source);

                Assert.NotEmpty(encoded);
                Assert.Equal(0x89, encoded[0]);
                Assert.Equal((byte)'P', encoded[1]);
                Assert.Equal((byte)'N', encoded[2]);
                Assert.Equal((byte)'G', encoded[3]);

                using (Mat decoded = ImgCodecsCv2.ImDecode(encoded, ImreadModes.Color))
                {
                    Assert.False(decoded.Empty);
                    Assert.Equal(2, decoded.Rows);
                    Assert.Equal(2, decoded.Cols);
                    Assert.Equal(MatType.CV_8UC3, decoded.Type);

                    byte[] decodedPixels = new byte[decoded.ByteLength];
                    decoded.CopyTo(decodedPixels);

                    Assert.Equal(bgrPixels, decodedPixels);
                }

#if NETCOREAPP3_1_OR_GREATER
                using (Mat decodedFromSpan = ImgCodecsCv2.ImDecode(encoded.AsSpan(), ImreadModes.Color))
                {
                    Assert.Equal(2, decodedFromSpan.Rows);
                    Assert.Equal(2, decodedFromSpan.Cols);
                    Assert.Equal(MatType.CV_8UC3, decodedFromSpan.Type);
                }
#endif
            }
        }

        [Fact]
        public void ImEncodeAcceptsPngAndJpegParametersWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat source = new Mat(8, 8, MatType.CV_8UC3))
            {
                byte[] pixels = new byte[source.ByteLength];
                for (int index = 0; index < pixels.Length; index += 3)
                {
                    pixels[index] = (byte)(index % 251);
                    pixels[index + 1] = (byte)((index / 3) % 239);
                    pixels[index + 2] = (byte)(255 - (index % 251));
                }

                source.CopyFrom(pixels);

                byte[] png = ImgCodecsCv2.ImEncode(".png", source, new int[]
                {
                    (int)ImwriteFlags.PngCompression, 9,
                    (int)ImwriteFlags.PngStrategy, (int)ImwritePngStrategy.Rle,
                    (int)ImwriteFlags.PngFilter, (int)ImwritePngFilterFlags.FastFilters
                });

                Assert.NotEmpty(png);
                Assert.Equal(0x89, png[0]);
                Assert.Equal((byte)'P', png[1]);
                Assert.Equal((byte)'N', png[2]);
                Assert.Equal((byte)'G', png[3]);

                byte[] jpeg = ImgCodecsCv2.ImEncode(".jpg", source, new int[]
                {
                    (int)ImwriteFlags.JpegQuality, 80,
                    (int)ImwriteFlags.JpegProgressive, 1,
                    (int)ImwriteFlags.JpegOptimize, 1
                });

                Assert.NotEmpty(jpeg);
                Assert.Equal(0xFF, jpeg[0]);
                Assert.Equal(0xD8, jpeg[1]);

                using (Mat decodedJpeg = ImgCodecsCv2.ImDecode(jpeg, ImreadModes.Color))
                {
                    Assert.Equal(8, decodedJpeg.Rows);
                    Assert.Equal(8, decodedJpeg.Cols);
                    Assert.Equal(MatType.CV_8UC3, decodedJpeg.Type);
                }

                byte[] webp = ImgCodecsCv2.ImEncode(".webp", source, new int[]
                {
                    (int)ImwriteFlags.WebPQuality, 80,
                    (int)ImwriteFlags.WebPLosslessMode, (int)ImwriteWebPLosslessMode.On
                });

                Assert.NotEmpty(webp);
                Assert.Equal(0x52, webp[0]);
                Assert.Equal((byte)'I', webp[1]);
                Assert.Equal((byte)'F', webp[2]);
                Assert.Equal((byte)'F', webp[3]);
            }
        }

        [Fact]
        public void ImEncodeRejectsOddParameterCount()
        {
            Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImEncode(".png", null!, new int[]
            {
                (int)ImwriteFlags.PngCompression
            }));
        }

        [Fact]
        public void ImEncodeRejectsManagedInvalidInputsBeforeNativeCall()
        {
            Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImEncode(null!, null!));
            Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImEncode(" ", null!));
            Assert.Throws<ArgumentNullException>(() => ImgCodecsCv2.ImEncode(".png", null!));
            Assert.Throws<ArgumentNullException>(() => ImgCodecsCv2.ImEncode(".png", null!, Array.Empty<int>()));
        }

        [Fact]
        public void ImDecodeRejectsManagedInvalidInputsBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => ImgCodecsCv2.ImDecode((byte[])null!));
            Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImDecode(Array.Empty<byte>()));
            Assert.Throws<ArgumentOutOfRangeException>(() => ImgCodecsCv2.ImDecode(new byte[] { 0xFF }, (ImreadModes)512));

#if NETCOREAPP3_1_OR_GREATER
            Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImDecode(ReadOnlySpan<byte>.Empty));
            Assert.Throws<ArgumentOutOfRangeException>(() => ImgCodecsCv2.ImDecode(new byte[] { 0xFF }.AsSpan(), (ImreadModes)512));
#endif
        }

    }
}
