using System;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace OpenCvSharp.Tests.ImgProc
{
    public class ImgProcUpstreamParityTests
    {
        [Fact]
        public void NewImgProcFamiliesValidateManagedArguments()
        {
            using (Mat mat = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => ImgProcCv2.CvtColorTwoPlane(null!, mat, mat, ColorConversionCodes.YUV2BGR_NV12));
                Assert.Throws<ArgumentNullException>(() => ImgProcCv2.Demosaicing(null!, mat, ColorConversionCodes.BayerBG2BGR));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.ApplyColorMap(mat, mat, (ColormapTypes)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.StackBlur(mat, mat, new Size(2, 3)));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.SpatialGradient(mat, mat, mat, 5));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.DrawMarker(mat, new Point(), new Scalar(), (MarkerTypes)99));
                Assert.Throws<ArgumentException>(() => ImgProcCv2.FillConvexPoly(mat, new[] { new Point(0, 0), new Point(1, 1) }, new Scalar()));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgProcCv2.GetFontScaleFromHeight(HersheyFonts.HersheySimplex, 0));
            }
        }

        [Fact]
        public void ColorFilterThresholdAndDrawingFamiliesExecuteWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat gray = new Mat(16, 16, MatType.CV_8UC1))
            using (Mat second = new Mat(16, 16, MatType.CV_8UC1))
            using (Mat weights1 = new Mat(16, 16, MatType.CV_32FC1))
            using (Mat weights2 = new Mat(16, 16, MatType.CV_32FC1))
            using (Mat mask = new Mat(16, 16, MatType.CV_8UC1))
            using (Mat colored = new Mat())
            using (Mat blended = new Mat())
            using (Mat blurred = new Mat())
            using (Mat dx = new Mat())
            using (Mat dy = new Mat())
            using (Mat thresholded = new Mat(16, 16, MatType.CV_8UC1))
            using (Mat userColor = new Mat(256, 1, MatType.CV_8UC3))
            using (Mat userColored = new Mat())
            {
                gray.SetTo(new Scalar(32));
                second.SetTo(new Scalar(192));
                weights1.SetTo(new Scalar(0.25));
                weights2.SetTo(new Scalar(0.75));
                mask.SetTo(new Scalar(255));
                thresholded.SetTo(new Scalar(7));
                userColor.SetTo(new Scalar(16, 64, 192));

                ImgProcCv2.ApplyColorMap(gray, colored, ColormapTypes.Turbo);
                ImgProcCv2.ApplyColorMap(gray, userColored, userColor);
                ImgProcCv2.BlendLinear(gray, second, weights1, weights2, blended);
                ImgProcCv2.StackBlur(gray, blurred, new Size(3, 3));
                ImgProcCv2.SpatialGradient(gray, dx, dy);
                double threshold = ImgProcCv2.ThresholdWithMask(gray, thresholded, mask, 64, 255, ThresholdTypes.Binary);
                ImgProcCv2.DrawMarker(colored, new Point(8, 8), new Scalar(0, 255, 0), MarkerTypes.Star, 7);
                Point[] triangle = { new Point(2, 12), new Point(8, 2), new Point(14, 12) };
                ImgProcCv2.FillConvexPoly(colored, triangle, new Scalar(255, 0, 0));
#if NETCOREAPP3_1_OR_GREATER
                ImgProcCv2.FillConvexPoly(colored, triangle.AsSpan(), new Scalar(0, 0, 255));
#endif
                double fontScale = ImgProcCv2.GetFontScaleFromHeight(HersheyFonts.HersheySimplex, 18);

                Assert.Equal(MatType.CV_8UC3, colored.Type);
                Assert.Equal(MatType.CV_8UC3, userColored.Type);
                Assert.Equal(MatType.CV_8UC1, blended.Type);
                Assert.Equal(MatType.CV_16SC1, dx.Type);
                Assert.Equal(MatType.CV_16SC1, dy.Type);
                Assert.Equal(64, threshold, 6);
                Assert.True(fontScale > 0);
            }
        }

        [Fact]
        public void TwoPlaneAndDemosaicingConversionsExecuteWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat y = new Mat(8, 8, MatType.CV_8UC1))
            using (Mat uv = new Mat(4, 4, MatType.CV_8UC2))
            using (Mat bayer = new Mat(8, 8, MatType.CV_8UC1))
            using (Mat color = ImgProcCv2.CvtColorTwoPlane(y, uv, ColorConversionCodes.YUV2BGR_NV12))
            using (Mat demosaiced = ImgProcCv2.Demosaicing(bayer, ColorConversionCodes.BayerBG2BGR))
            {
                y.SetTo(new Scalar(128));
                uv.SetTo(new Scalar(128, 128, 0, 0));
                bayer.SetTo(new Scalar(96));

                ImgProcCv2.CvtColorTwoPlane(y, uv, color, ColorConversionCodes.YUV2BGR_NV12);
                ImgProcCv2.Demosaicing(bayer, demosaiced, ColorConversionCodes.BayerBG2BGR);

                Assert.Equal(MatType.CV_8UC3, color.Type);
                Assert.Equal(MatType.CV_8UC3, demosaiced.Type);
                Assert.Equal(8, color.Rows);
                Assert.Equal(8, demosaiced.Cols);
            }
        }

        [Fact]
        public void GeneralizedHoughFamiliesExposePropertiesDetectionAndDisposalWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat template = new Mat(12, 12, MatType.CV_8UC1))
            using (Mat image = new Mat(32, 32, MatType.CV_8UC1))
            using (Mat positions = new Mat())
            using (Mat templateEdges = new Mat())
            using (Mat templateDx = new Mat())
            using (Mat templateDy = new Mat())
            using (Mat imageEdges = new Mat())
            using (Mat imageDx = new Mat())
            using (Mat imageDy = new Mat())
            using (GeneralizedHoughBallard ballard = ImgProcCv2.CreateGeneralizedHoughBallard())
            using (GeneralizedHoughGuil guil = ImgProcCv2.CreateGeneralizedHoughGuil())
            {
                template.SetTo(new Scalar(0));
                image.SetTo(new Scalar(0));
                ImgProcCv2.Rectangle(template, new Rect(2, 2, 8, 8), new Scalar(255), 1);
                ImgProcCv2.Rectangle(image, new Rect(10, 10, 8, 8), new Scalar(255), 1);

                ballard.CannyLowThreshold = 25;
                ballard.CannyHighThreshold = 75;
                ballard.MinDistance = 1;
                ballard.Dp = 1;
                ballard.MaxBufferSize = 100;
                ballard.Levels = 90;
                ballard.VotesThreshold = 1;
                ballard.SetTemplate(template);
                ballard.Detect(image, positions);
                using (Mat returned = ballard.Detect(image))
                {
                    Assert.NotNull(returned);
                }

                ImgProcCv2.Canny(template, templateEdges, 25, 75);
                ImgProcCv2.Sobel(template, templateDx, MatType.CV_32F, 1, 0);
                ImgProcCv2.Sobel(template, templateDy, MatType.CV_32F, 0, 1);
                ImgProcCv2.Canny(image, imageEdges, 25, 75);
                ImgProcCv2.Sobel(image, imageDx, MatType.CV_32F, 1, 0);
                ImgProcCv2.Sobel(image, imageDy, MatType.CV_32F, 0, 1);
                ballard.SetTemplate(templateEdges, templateDx, templateDy);
                ballard.Detect(imageEdges, imageDx, imageDy, positions);
                using (Mat edgeReturned = ballard.DetectEdges(imageEdges, imageDx, imageDy))
                {
                    Assert.NotNull(edgeReturned);
                }

                guil.Xi = 80;
                guil.Levels = 180;
                guil.AngleEpsilon = 1;
                guil.MinAngle = 0;
                guil.MaxAngle = 360;
                guil.AngleStep = 1;
                guil.AngleThreshold = 1;
                guil.MinScale = 0.5;
                guil.MaxScale = 2;
                guil.ScaleStep = 0.05;
                guil.ScaleThreshold = 1;
                guil.PositionThreshold = 1;

                Assert.Equal(25, ballard.CannyLowThreshold);
                Assert.Equal(90, ballard.Levels);
                Assert.Equal(80, guil.Xi, 6);
                Assert.Equal(2, guil.MaxScale, 6);
                Assert.False(ballard.IsDisposed);
                Assert.False(guil.IsDisposed);
            }
        }

        [Fact]
        public void GeneralizedHoughValidatesArgumentsAndThrowsAfterDisposeWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat mat = new Mat())
            {
                GeneralizedHoughBallard detector = ImgProcCv2.CreateGeneralizedHoughBallard();
                Assert.Throws<ArgumentNullException>(() => detector.SetTemplate(null!));
                Assert.Throws<ArgumentNullException>(() => detector.Detect(null!, mat));
                Assert.Throws<ArgumentOutOfRangeException>(() => detector.Dp = 0);
                Assert.Throws<ArgumentOutOfRangeException>(() => detector.Levels = 0);
                detector.Dispose();

                Assert.True(detector.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => detector.CannyLowThreshold);
                Assert.Throws<ObjectDisposedException>(() => detector.SetTemplate(mat));
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(mat, mat));
            }
        }
    }
}
