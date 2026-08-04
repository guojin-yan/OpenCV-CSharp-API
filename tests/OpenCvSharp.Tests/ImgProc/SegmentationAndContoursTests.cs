using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Geometry;
using JYPPX.OpenCvSharp.ImgProc;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgProc
{
    public class SegmentationAndContoursTests
    {
        [Fact]
        public void FloodFillFlagsValidateManagedArguments()
        {
            Assert.Equal(4, (int)FloodFillFlags.Connectivity4);
            Assert.Equal(8, (int)FloodFillFlags.Connectivity8);
            Assert.Equal(1 << 16, (int)FloodFillFlags.FixedRange);
            Assert.Equal(1 << 17, (int)FloodFillFlags.MaskOnly);

            using (Mat image = new Mat())
            using (Mat mask = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    Rect rect;
                    ImgProcCv2.FloodFill(image, new Point(0, 0), new Scalar(1), out rect, flags: (FloodFillFlags)5);
                });

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    Rect rect;
                    ImgProcCv2.FloodFill(image, mask, new Point(0, 0), new Scalar(1), out rect, flags: (FloodFillFlags)(1 << 18));
                });
            }
        }

        [Fact]
        public void DistanceTransformEnumsValidateManagedArguments()
        {
            Assert.Equal(0, (int)DistanceTransformMasks.Precise);
            Assert.Equal(3, (int)DistanceTransformMasks.Mask3);
            Assert.Equal(5, (int)DistanceTransformMasks.Mask5);
            Assert.Equal(0, (int)DistanceTransformLabelTypes.CComp);
            Assert.Equal(1, (int)DistanceTransformLabelTypes.Pixel);

            using (Mat src = new Mat())
            using (Mat dst = new Mat())
            using (Mat labels = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    ImgProcCv2.DistanceTransform(src, dst, DistanceTypes.L2, (DistanceTransformMasks)4));

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    ImgProcCv2.DistanceTransform(src, dst, labels, DistanceTypes.L2, (DistanceTransformMasks)4));

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    ImgProcCv2.DistanceTransform(src, dst, labels, DistanceTypes.L2, DistanceTransformMasks.Mask3, (DistanceTransformLabelTypes)99));
            }
        }

        [Fact]
        public void ConnectedComponentsAlgorithmValidatesManagedArgument()
        {
            Assert.Equal(-1, (int)ConnectedComponentsAlgorithmsTypes.Default);
            Assert.Equal(0, (int)ConnectedComponentsAlgorithmsTypes.Wu);
            Assert.Equal(1, (int)ConnectedComponentsAlgorithmsTypes.Grana);
            Assert.Equal(2, (int)ConnectedComponentsAlgorithmsTypes.Bolelli);
            Assert.Equal(3, (int)ConnectedComponentsAlgorithmsTypes.Sauf);
            Assert.Equal(4, (int)ConnectedComponentsAlgorithmsTypes.Bbdt);
            Assert.Equal(5, (int)ConnectedComponentsAlgorithmsTypes.Spaghetti);

            using (Mat image = new Mat())
            using (Mat labels = new Mat())
            using (Mat stats = new Mat())
            using (Mat centroids = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    ImgProcCv2.ConnectedComponentsWithAlgorithm(image, labels, 8, MatType.CV_32S, (ConnectedComponentsAlgorithmsTypes)99));

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    ImgProcCv2.ConnectedComponentsWithStatsWithAlgorithm(image, labels, stats, centroids, 8, MatType.CV_32S, (ConnectedComponentsAlgorithmsTypes)99));
            }
        }

        [Fact]
        public void AdaptiveThresholdAndEqualizeHistProduceExpectedBinaryAndHistogramOutputsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(3, 3, MatType.CV_8UC1))
            using (Mat adaptive = new Mat())
            using (Mat equalized = new Mat())
            {
                src.CopyFrom(new byte[]
                {
                    10, 20, 30,
                    40, 50, 60,
                    70, 80, 90
                });

                ImgProcCv2.AdaptiveThreshold(src, adaptive, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 3, 2);
                ImgProcCv2.EqualizeHist(src, equalized);

                Assert.Equal(3, adaptive.Rows);
                Assert.Equal(3, adaptive.Cols);
                Assert.Equal(MatType.CV_8UC1, adaptive.Type);
                Assert.Equal(MatType.CV_8UC1, equalized.Type);
                Assert.Equal(3, equalized.Rows);
                Assert.Equal(3, equalized.Cols);
            }
        }

        [Fact]
        public void IntegralAndDistanceTransformProduceExpectedShapesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(3, 3, MatType.CV_8UC1))
            using (Mat sum = new Mat())
            using (Mat sqsum = new Mat())
            using (Mat tilted = new Mat())
            using (Mat distance = new Mat())
            using (Mat labels = new Mat())
            {
                src.CopyFrom(new byte[]
                {
                    0, 0, 0,
                    0, 255, 0,
                    0, 0, 0
                });

                ImgProcCv2.Integral(src, sum);
                ImgProcCv2.Integral2(src, sum, sqsum);
                ImgProcCv2.Integral3(src, sum, sqsum, tilted);
                ImgProcCv2.DistanceTransform(src, distance, DistanceTypes.L2, DistanceTransformMasks.Mask3);
                ImgProcCv2.DistanceTransform(src, distance, labels, DistanceTypes.L2, DistanceTransformMasks.Mask3);

                Assert.True(sum.Rows >= 4);
                Assert.True(sum.Cols >= 4);
                Assert.True(sqsum.Rows >= 4);
                Assert.True(tilted.Rows >= 4);
                Assert.Equal(3, distance.Rows);
                Assert.Equal(3, distance.Cols);
            }
        }

        [Fact]
        public void FloodFillConnectedComponentsAndMomentsWorkOnSimpleShapesWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat flood = new Mat(5, 5, MatType.CV_8UC1))
            using (Mat floodMask = new Mat(7, 7, MatType.CV_8UC1))
            using (Mat labels = new Mat())
            using (Mat stats = new Mat())
            using (Mat centroids = new Mat())
            using (Mat contourImage = new Mat(5, 5, MatType.CV_8UC1))
            using (Mat corners = new Mat())
            {
                flood.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0,
                    0, 10, 10, 0, 0,
                    0, 10, 10, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0
                });
                floodMask.SetTo(new Scalar(0));

                int filled = ImgProcCv2.FloodFill(flood, new Point(1, 1), new Scalar(255), out Rect rect);
                int filledWithMask = ImgProcCv2.FloodFill(flood, floodMask, new Point(1, 1), new Scalar(128), out Rect maskRect, flags: FloodFillFlags.Connectivity4);

                contourImage.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0,
                    0, 255, 255, 0, 0,
                    0, 255, 255, 0, 0,
                    0, 0, 0, 255, 0,
                    0, 0, 0, 0, 0
                });

                int componentCount = ImgProcCv2.ConnectedComponents(contourImage, labels);
                int componentCountWithStats = ImgProcCv2.ConnectedComponentsWithStats(contourImage, labels, stats, centroids);
                Moments moments = ImgProcCv2.Moments(contourImage);
                double[] hu = ImgProcCv2.HuMoments(moments);
                ImgProcCv2.CornerHarris(contourImage, corners, 3, 3, 0.04);
                ImgProcCv2.CornerMinEigenVal(contourImage, corners, 3);
                ImgProcCv2.CornerEigenValsAndVecs(contourImage, corners, 3, 3);
                ImgProcCv2.PreCornerDetect(contourImage, corners, 3);

                Assert.True(filled > 0);
                Assert.True(filledWithMask > 0);
                Assert.True(rect.Width > 0);
                Assert.True(maskRect.Width > 0);
                Assert.True(componentCount >= 2);
                Assert.Equal(componentCount, componentCountWithStats);
                Assert.Equal(24, moments.ToArray().Length);
                Assert.Equal(7, hu.Length);
                Assert.Equal(5, corners.Rows);
                Assert.Equal(5, corners.Cols);
            }
        }

        [Fact]
        public void FindContoursAndDrawContoursRoundTripWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(6, 6, MatType.CV_8UC1))
            using (Mat drawing = new Mat(6, 6, MatType.CV_8UC1))
            {
                src.CopyFrom(new byte[]
                {
                    0, 0, 0, 0, 0, 0,
                    0, 255, 255, 0, 0, 0,
                    0, 255, 255, 0, 0, 0,
                    0, 0, 0, 0, 255, 0,
                    0, 0, 0, 0, 255, 0,
                    0, 0, 0, 0, 0, 0
                });

                ImgProcCv2.FindContours(src, out Point[][] contours, out Vec4i[] hierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

                Assert.NotEmpty(contours);
                Assert.NotEmpty(hierarchy);
                Assert.All(contours, contour => Assert.NotEmpty(contour));

                ImgProcCv2.DrawContours(drawing, contours, -1, new Scalar(255));
                Assert.Contains(drawing.ToBytes(), value => value != 0);
            }
        }

    }
}
