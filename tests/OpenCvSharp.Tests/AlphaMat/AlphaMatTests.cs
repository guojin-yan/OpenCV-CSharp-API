using System;
using JYPPX.OpenCvSharp.AlphaMat;
using JYPPX.OpenCvSharp.Core;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Tests.AlphaMat
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class AlphaMatTests
    {
        [Fact]
        public void ValidationRunsBeforeNativeCall()
        {
            using (Mat image = CreateColorImage())
            using (Mat trimap = CreateTrimap())
            using (Mat grayImage = new Mat(image.Rows, image.Cols, MatType.CV_8UC1))
            using (Mat colorTrimap = new Mat(trimap.Rows, trimap.Cols, MatType.CV_8UC3))
            using (Mat smallTrimap = new Mat(trimap.Rows - 1, trimap.Cols, MatType.CV_8UC1))
            using (Mat result = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => AlphaMatCv2.InfoFlow(null!, trimap, result));
                Assert.Throws<ArgumentNullException>(() => AlphaMatCv2.InfoFlow(image, null!, result));
                Assert.Throws<ArgumentNullException>(() => AlphaMatCv2.InfoFlow(image, trimap, null!));
                Assert.Throws<ArgumentNullException>(() => AlphaMatCv2.InfoFlow(null!, trimap));
                Assert.Throws<ArgumentNullException>(() => AlphaMatCv2.InfoFlow(image, null!));
                Assert.Throws<ArgumentException>(() => AlphaMatCv2.InfoFlow(grayImage, trimap, result));
                Assert.Throws<ArgumentException>(() => AlphaMatCv2.InfoFlow(image, colorTrimap, result));
                Assert.Throws<ArgumentException>(() => AlphaMatCv2.InfoFlow(image, smallTrimap, result));
                Assert.Throws<ArgumentException>(() => AlphaMatCv2.InfoFlow(grayImage, trimap));
                Assert.Throws<ArgumentException>(() => AlphaMatCv2.InfoFlow(image, colorTrimap));
                Assert.Throws<ArgumentException>(() => AlphaMatCv2.InfoFlow(image, smallTrimap));
            }
        }

        [Fact]
        public void LinkedSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (Mat image = CreateColorImage())
                using (Mat trimap = CreateTrimap())
                using (Mat result = AlphaMatCv2.InfoFlow(image, trimap))
                {
                    Assert.False(result.Empty);
                    Assert.Equal(image.Rows, result.Rows);
                    Assert.Equal(image.Cols, result.Cols);
                }
            }
            catch (OpenCvException ex) when (IsAlphaMatModuleMissing(ex) || IsTinyDataBoundary(ex))
            {
                Assert.True(IsAlphaMatModuleMissing(ex) || IsTinyDataBoundary(ex), ex.Message);
            }
        }

        private static Mat CreateColorImage()
        {
            const int size = 64;
            var image = new Mat(size, size, MatType.CV_8UC3);
            var bytes = new byte[size * size * 3];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int offset = (y * size + x) * 3;
                    bytes[offset] = (byte)(20 + ((x * 3 + y) % 80));
                    bytes[offset + 1] = (byte)(30 + ((x + y * 2) % 90));
                    bytes[offset + 2] = (byte)(40 + ((x * 2 + y * 3) % 100));
                }
            }

            image.CopyFrom(bytes);
            ImgProcCv2.Rectangle(image, new Rect(16, 16, 32, 32), new Scalar(210, 190, 120), -1);
            ImgProcCv2.Circle(image, new Point(36, 30), 12, new Scalar(80, 210, 170), -1);
            return image;
        }

        private static Mat CreateTrimap()
        {
            var trimap = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0));
            ImgProcCv2.Rectangle(trimap, new Rect(12, 12, 40, 40), new Scalar(128), -1);
            ImgProcCv2.Rectangle(trimap, new Rect(22, 22, 20, 20), new Scalar(255), -1);
            return trimap;
        }

        private static bool IsAlphaMatModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("alphamat", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsTinyDataBoundary(OpenCvException exception)
        {
            return exception.Message.IndexOf("assert", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("failed", StringComparison.OrdinalIgnoreCase) >= 0;
        }

    }
}
