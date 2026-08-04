using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Video;
using VideoCv2 = JYPPX.OpenCvSharp.Video.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Video
{
    public sealed class VideoMotionTests
    {
        [Fact]
        public void VideoResultObjectsExposeValues()
        {
            var meanShift = new MeanShiftResult(3, new Rect(1, 2, 3, 4));
            var camShift = new CamShiftResult(new Rect(5, 6, 7, 8), new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 4.5F), 15.0F));
            var pyramid = new OpticalFlowPyramidResult(2, Array.Empty<Mat>());

            Assert.Equal(3, meanShift.Iterations);
            Assert.Equal(0, new MeanShiftResult(0, new Rect(1, 2, 3, 4)).Iterations);
            Assert.Throws<ArgumentOutOfRangeException>(() => new MeanShiftResult(-1, new Rect(1, 2, 3, 4)));
            Assert.Equal(4, meanShift.Window.Height);
            Assert.Equal(7, camShift.Window.Width);
            Assert.Equal(15.0F, camShift.Box.Angle);
            Assert.Equal(2, pyramid.LevelCount);
            Assert.Equal(0, pyramid.PyramidCount);
            Assert.Empty(pyramid.Pyramid);
            Assert.Equal("OpticalFlowPyramidResult(LevelCount=2, Pyramid=0)", pyramid.ToString());
        }

        [Fact]
        public void OpticalFlowPyramidResultNormalizesNullPyramidAndReportsCount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new OpticalFlowPyramidResult(-1, Array.Empty<Mat>()));
            Assert.Throws<ArgumentNullException>(() => new OpticalFlowPyramidResult(0, new Mat[] { null! }));

            var nullPyramid = new OpticalFlowPyramidResult(0, null!);
            Assert.Equal(0, nullPyramid.LevelCount);
            Assert.Equal(0, nullPyramid.PyramidCount);
            Assert.Empty(nullPyramid.Pyramid);
            Assert.Equal("OpticalFlowPyramidResult(LevelCount=0, Pyramid=0)", nullPyramid.ToString());

            Mat[] mats =
            {
                new Mat(),
                new Mat()
            };

            var pyramid = new OpticalFlowPyramidResult(1, mats);
            Mat originalFirst = mats[0];
            mats[0] = new Mat();
            Assert.Equal(1, pyramid.LevelCount);
            Assert.Equal(2, pyramid.PyramidCount);
            Assert.NotSame(mats, pyramid.Pyramid);
            Assert.Same(originalFirst, pyramid.Pyramid[0]);
            Mat[] returnedPyramid = pyramid.Pyramid;
            returnedPyramid[0] = new Mat();
            Assert.NotSame(returnedPyramid, pyramid.Pyramid);
            Assert.Same(originalFirst, pyramid.Pyramid[0]);
            Assert.Equal("OpticalFlowPyramidResult(LevelCount=1, Pyramid=2)", pyramid.ToString());
        }

        [Fact]
        public void ShiftTrackingResultsHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(20, Marshal.SizeOf<MeanShiftResult>());
            Assert.Equal(36, Marshal.SizeOf<CamShiftResult>());

            MeanShiftResult[] meanShiftResults =
            {
                new MeanShiftResult(1, new Rect(2, 3, 4, 5)),
                new MeanShiftResult(6, new Rect(7, 8, 9, 10))
            };
            CamShiftResult[] camShiftResults =
            {
                new CamShiftResult(
                    new Rect(1, 2, 3, 4),
                    new RotatedRect(new Point2f(5.5F, 6.5F), new Size2f(7.5F, 8.5F), 9.5F)),
                new CamShiftResult(
                    new Rect(10, 11, 12, 13),
                    new RotatedRect(new Point2f(14.5F, 15.5F), new Size2f(16.5F, 17.5F), 18.5F))
            };

            ReadOnlySpan<int> meanShiftFields = MemoryMarshal.Cast<MeanShiftResult, int>(meanShiftResults.AsSpan());
            ReadOnlySpan<byte> camShiftBytes = MemoryMarshal.AsBytes(camShiftResults.AsSpan());

            Assert.Equal(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, meanShiftFields.ToArray());
            Assert.Equal(BitConverter.GetBytes(1), camShiftBytes.Slice(0, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(2), camShiftBytes.Slice(4, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(3), camShiftBytes.Slice(8, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(4), camShiftBytes.Slice(12, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(5.5F), camShiftBytes.Slice(16, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(6.5F), camShiftBytes.Slice(20, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(7.5F), camShiftBytes.Slice(24, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(8.5F), camShiftBytes.Slice(28, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(9.5F), camShiftBytes.Slice(32, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(10), camShiftBytes.Slice(36, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(11), camShiftBytes.Slice(40, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(12), camShiftBytes.Slice(44, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(13), camShiftBytes.Slice(48, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(14.5F), camShiftBytes.Slice(52, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(15.5F), camShiftBytes.Slice(56, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(16.5F), camShiftBytes.Slice(60, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(17.5F), camShiftBytes.Slice(64, 4).ToArray());
            Assert.Equal(BitConverter.GetBytes(18.5F), camShiftBytes.Slice(68, 4).ToArray());
        }

        [Fact]
        public void ShiftTrackingResultsReportValueEquality()
        {
            var meanShift = new MeanShiftResult(3, new Rect(1, 2, 3, 4));
            var sameMeanShift = new MeanShiftResult(3, new Rect(1, 2, 3, 4));
            var camShift = new CamShiftResult(
                new Rect(5, 6, 7, 8),
                new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 4.5F), 15.0F));
            var sameCamShift = new CamShiftResult(
                new Rect(5, 6, 7, 8),
                new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 4.5F), 15.0F));

            Assert.Equal(sameMeanShift, meanShift);
            Assert.True(meanShift == sameMeanShift);
            Assert.False(meanShift.Equals("not a mean-shift result"));
            Assert.Equal(sameMeanShift.GetHashCode(), meanShift.GetHashCode());
            Assert.NotEqual(new MeanShiftResult(9, new Rect(1, 2, 3, 4)), meanShift);
            Assert.True(meanShift != new MeanShiftResult(3, new Rect(9, 2, 3, 4)));
            Assert.Equal("{Iterations=3,Window={X=1,Y=2,Width=3,Height=4}}", meanShift.ToString());

            Assert.Equal(sameCamShift, camShift);
            Assert.True(camShift == sameCamShift);
            Assert.False(camShift.Equals("not a cam-shift result"));
            Assert.Equal(sameCamShift.GetHashCode(), camShift.GetHashCode());
            Assert.NotEqual(
                new CamShiftResult(new Rect(9, 6, 7, 8), new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 4.5F), 15.0F)),
                camShift);
            Assert.True(
                camShift != new CamShiftResult(new Rect(5, 6, 7, 8), new RotatedRect(new Point2f(9.5F, 2.5F), new Size2f(3.5F, 4.5F), 15.0F)));
            Assert.Equal(
                "{Window={X=5,Y=6,Width=7,Height=8},Box={Center={X=1.5,Y=2.5},Size={Width=3.5,Height=4.5},Angle=15}}",
                camShift.ToString());
        }

        [Fact]
        public void ShiftTrackingResultsFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var camShift = new CamShiftResult(
                    new Rect(5, 6, 7, 8),
                    new RotatedRect(new Point2f(1.5F, 2.5F), new Size2f(3.5F, 4.5F), 15.25F));

                Assert.Equal("{Iterations=3,Window={X=1,Y=2,Width=3,Height=4}}", new MeanShiftResult(3, new Rect(1, 2, 3, 4)).ToString());
                Assert.Equal(
                    "{Window={X=5,Y=6,Width=7,Height=8},Box={Center={X=1.5,Y=2.5},Size={Width=3.5,Height=4.5},Angle=15.25}}",
                    camShift.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void VideoEnumsMatchOpenCvValues()
        {
            Assert.Equal(4, (int)OpticalFlowFlags.UseInitialFlow);
            Assert.Equal(8, (int)OpticalFlowFlags.LkGetMinEigenvals);
            Assert.Equal(256, (int)OpticalFlowFlags.FarnebackGaussian);
        }

        [Fact]
        public void OpticalFlowValidatesManagedArguments()
        {
            using (var image = new Mat())
            using (var flow = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    VideoCv2.CalcOpticalFlowPyrLK(null!, image, new[] { new Point2f(1.0F, 1.0F) }, out _, out _));
                Assert.Throws<ArgumentNullException>(() =>
                    VideoCv2.CalcOpticalFlowPyrLK(image, null!, new[] { new Point2f(1.0F, 1.0F) }, out _, out _));
                Assert.Throws<ArgumentNullException>(() =>
                    VideoCv2.CalcOpticalFlowPyrLK(image, image, null!, out _, out _));
                Assert.Throws<ArgumentException>(() =>
                    VideoCv2.CalcOpticalFlowPyrLK(image, image, new[] { new Point2f(1.0F, 1.0F) }, Array.Empty<Point2f>(), out _, out _));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    VideoCv2.CalcOpticalFlowPyrLK(image, image, new[] { new Point2f(1.0F, 1.0F) }, out _, out _, flags: (OpticalFlowFlags)16));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    VideoCv2.CalcOpticalFlowPyrLK(image, image, new ReadOnlySpan<Point2f>(new[] { new Point2f(1.0F, 1.0F) }), out _, out _, flags: (OpticalFlowFlags)16));
#endif
                Assert.Throws<ArgumentNullException>(() =>
                    VideoCv2.CalcOpticalFlowFarneback(null!, image, flow, 0.5, 1, 3, 1, 5, 1.1));
                Assert.Throws<ArgumentNullException>(() =>
                    VideoCv2.CalcOpticalFlowFarneback(image, null!, flow, 0.5, 1, 3, 1, 5, 1.1));
                Assert.Throws<ArgumentNullException>(() =>
                    VideoCv2.CalcOpticalFlowFarneback(image, image, null!, 0.5, 1, 3, 1, 5, 1.1));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    VideoCv2.CalcOpticalFlowFarneback(image, image, flow, 0.5, 1, 3, 1, 5, 1.1, (OpticalFlowFlags)16));
                Assert.Throws<ArgumentNullException>(() =>
                    VideoCv2.BuildOpticalFlowPyramid(null!, new Size(3, 3), 1));
                Assert.Throws<ArgumentNullException>(() =>
                    VideoCv2.MeanShift(null!, new Rect(0, 0, 1, 1)));
                Assert.Throws<ArgumentNullException>(() =>
                    VideoCv2.CamShift(null!, new Rect(0, 0, 1, 1)));
            }
        }

        [Fact]
        public void OpticalFlowRunsOnTinyFramesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var prev = new Mat(8, 8, MatType.CV_8UC1, new Scalar(0)))
            using (var next = new Mat(8, 8, MatType.CV_8UC1, new Scalar(0)))
            using (var flow = new Mat())
            {
                VideoCv2.CalcOpticalFlowFarneback(prev, next, flow, 0.5, 1, 3, 1, 5, 1.1);

                Assert.Equal(8, flow.Rows);
                Assert.Equal(8, flow.Cols);
            }
        }

        [Fact]
        public void ShiftTrackersRunOnTinyProbabilityImageWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var probImage = CreateProbabilityImage())
            {
                var initialWindow = new Rect(4, 4, 8, 8);

                MeanShiftResult meanShift = VideoCv2.MeanShift(probImage, initialWindow);
                CamShiftResult camShift = VideoCv2.CamShift(probImage, initialWindow);

                Assert.True(meanShift.Iterations >= 0);
                Assert.True(meanShift.Window.Width > 0);
                Assert.True(meanShift.Window.Height > 0);
                Assert.True(meanShift.Window.X >= 0);
                Assert.True(meanShift.Window.Y >= 0);
                Assert.True(meanShift.Window.X + meanShift.Window.Width <= probImage.Cols);
                Assert.True(meanShift.Window.Y + meanShift.Window.Height <= probImage.Rows);

                Assert.True(camShift.Window.Width > 0);
                Assert.True(camShift.Window.Height > 0);
                Assert.True(camShift.Window.X >= 0);
                Assert.True(camShift.Window.Y >= 0);
                Assert.True(camShift.Window.X + camShift.Window.Width <= probImage.Cols);
                Assert.True(camShift.Window.Y + camShift.Window.Height <= probImage.Rows);
                Assert.True(camShift.Box.Size.Width >= 0.0F);
                Assert.True(camShift.Box.Size.Height >= 0.0F);
            }
        }

        private static Mat CreateProbabilityImage()
        {
            var image = new Mat(16, 16, MatType.CV_8UC1);
            var values = new byte[16 * 16];
            for (int y = 5; y < 11; y++)
            {
                for (int x = 5; x < 11; x++)
                {
                    values[(y * 16) + x] = 255;
                }
            }

            image.CopyFrom(values);
            return image;
        }

    }
}
