using System;
using System.IO;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgCodecs;
using JYPPX.OpenCvSharp;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgCodecs
{
    public class FileInteropTests
    {
        [Fact]
        public void ImWriteAndImReadRoundTripPngAndJpegUsingUtf8PathsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string root = Path.Combine(Path.GetTempPath(), "JYPPX.OpenCvSharp-文件-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(root);

            string pngPath = Path.Combine(root, "测试图片.png");
            string jpegPath = Path.Combine(root, "测试图片.jpg");

            byte[] bgrPixels = new byte[]
            {
                0, 0, 255,
                0, 255, 0,
                255, 0, 0,
                255, 255, 255
            };

            try
            {
                using (Mat source = new Mat(2, 2, MatType.CV_8UC3))
                {
                    source.CopyFrom(bgrPixels);

                    bool pngWritten = ImgCodecsCv2.ImWrite(pngPath, source, new int[]
                    {
                        (int)ImwriteFlags.PngCompression, 9
                    });

                    bool jpegWritten = ImgCodecsCv2.ImWrite(jpegPath, source, new int[]
                    {
                        (int)ImwriteFlags.JpegQuality, 80
                    });

                    Assert.True(pngWritten);
                    Assert.True(jpegWritten);
                    Assert.True(File.Exists(pngPath));
                    Assert.True(File.Exists(jpegPath));
                    Assert.True(new FileInfo(pngPath).Length > 0);
                    Assert.True(new FileInfo(jpegPath).Length > 0);

                    using (Mat pngRead = ImgCodecsCv2.ImRead(pngPath, ImreadModes.Color))
                    {
                        Assert.Equal(2, pngRead.Rows);
                        Assert.Equal(2, pngRead.Cols);
                        Assert.Equal(MatType.CV_8UC3, pngRead.Type);

                        byte[] decodedPixels = new byte[pngRead.ByteLength];
                        pngRead.CopyTo(decodedPixels);
                        Assert.Equal(bgrPixels, decodedPixels);
                    }

                    using (Mat jpegRead = ImgCodecsCv2.ImRead(jpegPath, ImreadModes.Color))
                    {
                        Assert.Equal(2, jpegRead.Rows);
                        Assert.Equal(2, jpegRead.Cols);
                        Assert.Equal(MatType.CV_8UC3, jpegRead.Type);
                    }
                }
            }
            finally
            {
                TryDeleteFile(pngPath);
                TryDeleteFile(jpegPath);
                TryDeleteDirectory(root);
            }
        }

        [Fact]
        public void ImWriteReturnsFalseWhenOutputDirectoryDoesNotExist()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string root = Path.Combine(Path.GetTempPath(), "JYPPX.OpenCvSharp-missing-" + Guid.NewGuid().ToString("N"));
            string path = Path.Combine(root, "image.png");

            using (Mat source = new Mat(1, 1, MatType.CV_8UC1))
            {
                source.CopyFrom(new byte[] { 255 });

                bool written = ImgCodecsCv2.ImWrite(path, source);

                Assert.False(written);
                Assert.False(File.Exists(path));
            }
        }

        [Fact]
        public void ImWriteWithoutExtensionThrowsOpenCvExceptionWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string path = Path.Combine(Path.GetTempPath(), "JYPPX.OpenCvSharp-no-extension-" + Guid.NewGuid().ToString("N"));

            using (Mat source = new Mat(1, 1, MatType.CV_8UC1))
            {
                source.CopyFrom(new byte[] { 255 });

                Assert.Throws<OpenCvException>(() => ImgCodecsCv2.ImWrite(path, source));
            }
        }

        [Fact]
        public void ImWriteRejectsOddParameterCount()
        {
            Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImWrite("image.png", null!, new int[]
            {
                (int)ImwriteFlags.PngCompression
            }));
        }

        [Fact]
        public void FilePathOperationsRejectManagedInvalidInputsBeforeNativeCall()
        {
            Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImRead(null!));
            Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImRead(" "));
            Assert.Throws<ArgumentOutOfRangeException>(() => ImgCodecsCv2.ImRead("image.png", (ImreadModes)512));
            Assert.Throws<ArgumentNullException>(() => ImgCodecsCv2.ImWrite("image.png", null!));
            Assert.Throws<ArgumentNullException>(() => ImgCodecsCv2.ImWrite("image.png", null!, Array.Empty<int>()));

            using (var image = new Mat())
            {
                Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImWrite(null!, image));
                Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImWrite(" ", image));
                Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImWrite(null!, image, Array.Empty<int>()));
                Assert.Throws<ArgumentException>(() => ImgCodecsCv2.ImWrite(" ", image, Array.Empty<int>()));
            }
        }

        private static void TryDeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private static void TryDeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, false);
            }
        }

    }
}
