using System;
using System.IO;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgCodecs;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class ImgCodecsUpstreamParityTests
    {
        [Fact]
        public void MultiPageTiffRoundTripsThroughFileBufferRangeAndCollection()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            string root = Path.Combine(Path.GetTempPath(), "JYPPX.OpenCvSharp-多页-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(root, "页面.tiff");
            Directory.CreateDirectory(root);
            var pages = new Mat[]
            {
                CreateSolidImage(4, 5, 11),
                CreateSolidImage(3, 6, 77),
                CreateSolidImage(2, 7, 191)
            };

            try
            {
                Assert.True(ImgCodecsCv2.ImWriteMulti(path, pages));
                Assert.True(ImgCodecsCv2.HaveImageWriter(".tiff"));
                Assert.True(ImgCodecsCv2.HaveImageReader(path));
                Assert.Equal(3, ImgCodecsCv2.ImCount(path));

                Assert.True(ImgCodecsCv2.ImReadMulti(path, out Mat[] decoded));
                try
                {
                    Assert.Equal(3, decoded.Length);
                    Assert.Equal(4, decoded[0].Rows);
                    Assert.Equal(3, decoded[1].Rows);
                    Assert.Equal(2, decoded[2].Rows);
                }
                finally
                {
                    DisposeAll(decoded);
                }

                Assert.True(ImgCodecsCv2.ImReadMulti(path, 1, 1, out Mat[] fileRange));
                try
                {
                    Assert.Single(fileRange);
                    Assert.Equal(3, fileRange[0].Rows);
                }
                finally
                {
                    DisposeAll(fileRange);
                }

                byte[] encoded = ImgCodecsCv2.ImEncodeMulti(".tiff", pages);
                Assert.True(ImgCodecsCv2.ImDecodeMulti(encoded, 1, 3, out Mat[] memoryRange));
                try
                {
                    Assert.Equal(2, memoryRange.Length);
                    Assert.Equal(3, memoryRange[0].Rows);
                    Assert.Equal(2, memoryRange[1].Rows);
                }
                finally
                {
                    DisposeAll(memoryRange);
                }

                using (var collection = new ImageCollection(path, ImreadModes.Unchanged))
                {
                    Assert.Equal(3, collection.Count);
                    using (Mat last = collection[2]) Assert.Equal(2, last.Rows);
                    using (Mat first = collection[0]) Assert.Equal(4, first.Rows);
                    collection.ReleaseCache(0);
                    collection.Initialize(path, ImreadModes.Color);
                    Assert.Equal(3, collection.Count);
                }
            }
            finally
            {
                DisposeAll(pages);
                if (File.Exists(path)) File.Delete(path);
                if (Directory.Exists(root)) Directory.Delete(root);
            }
        }

        [Fact]
        public void JpegExifRoundTripsThroughFileAndBuffer()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            string root = Path.Combine(Path.GetTempPath(), "JYPPX.OpenCvSharp-元数据-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(root, "照片.jpeg");
            Directory.CreateDirectory(root);
            byte[] exif = CreateSampleExifData();

            using (Mat image = CreateSolidColorImage(32, 24))
            using (Mat exifMat = new Mat(1, exif.Length, MatType.CV_8UC1))
            {
                exifMat.CopyFrom(exif);
                var metadata = new[] { new ImageMetadataChunk(ImageMetadataType.Exif, exifMat) };
                try
                {
                    Assert.True(ImgCodecsCv2.ImWriteWithMetadata(path, image, metadata, new[] { (int)ImwriteFlags.JpegQuality, 95 }));
                    byte[] encoded = ImgCodecsCv2.ImEncodeWithMetadata(".jpeg", image, metadata, new[] { (int)ImwriteFlags.JpegQuality, 95 });

                    using (ImageMetadataResult fromFile = ImgCodecsCv2.ImReadWithMetadata(path))
                    using (ImageMetadataResult fromMemory = ImgCodecsCv2.ImDecodeWithMetadata(encoded))
                    {
                        Assert.Single(fromFile.Metadata);
                        Assert.Single(fromMemory.Metadata);
                        Assert.Equal(ImageMetadataType.Exif, fromFile.Metadata[0].Type);
                        Assert.Equal(exif, CopyBytes(fromFile.Metadata[0].Data));
                        Assert.Equal(exif, CopyBytes(fromMemory.Metadata[0].Data));
                        Assert.Equal(32, fromFile.Image.Rows);
                        Assert.Equal(24, fromFile.Image.Cols);
                    }
                }
                finally
                {
                    if (File.Exists(path)) File.Delete(path);
                    if (Directory.Exists(root)) Directory.Delete(root);
                }
            }
        }

        [Fact]
        public void GifAnimationRoundTripsAndOwnsClonedFrames()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            string root = Path.Combine(Path.GetTempPath(), "JYPPX.OpenCvSharp-动画-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(root, "序列.gif");
            Directory.CreateDirectory(root);
            var sourceFrames = new[]
            {
                CreateSolidColorImage(8, 9, 20),
                CreateSolidColorImage(8, 9, 100),
                CreateSolidColorImage(8, 9, 220)
            };

            try
            {
                byte[] encoded;
                using (var animation = new Animation(2, new Scalar(1, 2, 3, 4)))
                {
                    animation.SetFrames(sourceFrames, new[] { 40, 70, 100 });
                    sourceFrames[0].SetTo(new Scalar(255, 255, 255));
                    Assert.Equal(3, animation.FrameCount);
                    Assert.Equal(2, animation.LoopCount);
                    Assert.Equal(new Scalar(1, 2, 3, 4), animation.BackgroundColor);
                    using (AnimationFrame first = animation.GetFrame(0))
                    {
                        Assert.Equal(40, first.DurationMilliseconds);
                        Assert.Equal((byte)20, CopyBytes(first.Image)[0]);
                    }

                    encoded = ImgCodecsCv2.ImEncodeAnimation(".gif", animation);
                    Assert.True(ImgCodecsCv2.ImWriteAnimation(path, animation));
                }

                using (var decoded = new Animation())
                {
                    Assert.True(ImgCodecsCv2.ImDecodeAnimation(encoded, decoded));
                    Assert.Equal(3, decoded.FrameCount);
                    using (AnimationFrame second = decoded.GetFrame(1)) Assert.Equal(70, second.DurationMilliseconds);
                }

                using (var range = new Animation())
                {
                    Assert.True(ImgCodecsCv2.ImReadAnimation(path, range, 1, 1));
                    Assert.Equal(1, range.FrameCount);
                }
            }
            finally
            {
                DisposeAll(sourceFrames);
                if (File.Exists(path)) File.Delete(path);
                if (Directory.Exists(root)) Directory.Delete(root);
            }
        }

        [Fact]
        public void NewImgCodecsApisRejectInvalidManagedInputs()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ImgCodecsCv2.ImReadMulti("a.tiff", -1, 1, out _));
            Assert.Throws<ArgumentOutOfRangeException>(() => ImgCodecsCv2.ImDecodeMulti(new byte[] { 1 }, 2, 2, out _));
            Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImEncodeMulti(".tiff", Array.Empty<Mat>()));
            Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImWriteMulti("a.tiff", new Mat[] { new Mat() }, new[] { 1 }));
            Assert.Throws<ArgumentNullException>(() => ImgCodecsCv2.ImDecodeWithMetadata(null!));

            using (var animation = new Animation())
            {
                Assert.Throws<ArgumentException>(() => animation.SetFrames(Array.Empty<Mat>(), Array.Empty<int>()));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgCodecsCv2.ImDecodeAnimation(new byte[] { 1 }, animation, -1, 1));
                animation.Dispose();
                Assert.Throws<ObjectDisposedException>(() => _ = animation.FrameCount);
            }
        }

        [Fact]
        public void ImgCodecsEnumsMatchOpenCvFiveHeaderValues()
        {
            Assert.Equal(15, Enum.GetNames(typeof(ImreadModes)).Length);
            Assert.Equal(41, Enum.GetNames(typeof(ImwriteFlags)).Length);
            Assert.Equal(1, (int)ImreadModes.ColorBgr);
            Assert.Equal(50, (int)ImwriteFlags.ExrDwaCompressionLevel);
            Assert.Equal(317, (int)ImwriteFlags.TiffPredictor);
            Assert.Equal(1029, (int)ImwriteFlags.GifColorTable);
            Assert.Equal(0x221111, (int)ImwriteJpegSamplingFactor.Sampling420);
            Assert.Equal(32946, (int)ImwriteTiffCompression.Deflate);
            Assert.Equal(50002, (int)ImwriteTiffCompression.JpegXl);
            Assert.Equal(9, (int)ImwriteExrCompression.Dwab);
            Assert.Equal(8, (int)ImwriteGifCompression.ColorTableSize256);
            Assert.Equal(3, (int)ImageMetadataType.Max);
        }

        private static Mat CreateSolidImage(int rows, int cols, byte value)
        {
            var result = new Mat(rows, cols, MatType.CV_8UC1);
            result.SetTo(new Scalar(value));
            return result;
        }

        private static Mat CreateSolidColorImage(int rows, int cols, byte value = 80)
        {
            var result = new Mat(rows, cols, MatType.CV_8UC3);
            result.SetTo(new Scalar(value, value + 1, value + 2));
            return result;
        }

        private static byte[] CopyBytes(Mat value)
        {
            var result = new byte[value.ByteLength];
            value.CopyTo(result);
            return result;
        }

        private static void DisposeAll(Mat[] values)
        {
            foreach (Mat value in values) value.Dispose();
        }

        private static byte[] CreateSampleExifData()
        {
            return new byte[]
            {
                (byte)'M', (byte)'M', 0, 42, 0, 0, 0, 8, 0, 10, 1, 0, 0, 4, 0, 0, 0, 1, 0, 0, 5,
                0, 1, 1, 0, 4, 0, 0, 0, 1, 0, 0, 2, 208, 1, 2, 0, 3, 0, 0, 0, 1,
                0, 10, 0, 0, 1, 18, 0, 3, 0, 0, 0, 1, 0, 1, 0, 0, 1, 14, 0, 2, 0, 0,
                0, 34, 0, 0, 0, 176, 1, 49, 0, 2, 0, 0, 0, 7, 0, 0, 0, 210, 1, 26,
                0, 5, 0, 0, 0, 1, 0, 0, 0, 218, 1, 27, 0, 5, 0, 0, 0, 1, 0, 0, 0,
                226, 1, 40, 0, 3, 0, 0, 0, 1, 0, 2, 0, 0, 135, 105, 0, 4, 0, 0, 0,
                1, 0, 0, 0, 134, 0, 0, 0, 0, 0, 3, 144, 0, 0, 7, 0, 0, 0, 4, 48, 50,
                50, 49, 160, 2, 0, 4, 0, 0, 0, 1, 0, 0, 5, 0, 160, 3, 0, 4, 0, 0,
                0, 1, 0, 0, 2, 208, 0, 0, 0, 0, 83, 97, 109, 112, 108, 101, 32, 49, 48,
                45, 98, 105, 116, 32, 105, 109, 97, 103, 101, 32, 119, 105, 116, 104, 32,
                109, 101, 116, 97, 100, 97, 116, 97, 0, 79, 112, 101, 110, 67, 86, 0, 0,
                0, 0, 0, 72, 0, 0, 0, 1, 0, 0, 0, 72, 0, 0, 0, 1
            };
        }
    }
}
