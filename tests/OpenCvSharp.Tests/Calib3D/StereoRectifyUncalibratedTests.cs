using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class StereoRectifyUncalibratedTests
    {
        [Fact]
        public void StereoRectifyUncalibratedValidatesInputsBeforeNativeCall()
        {
            using Mat points1 = CreatePointMat(CreatePoints1());
            using Mat points2 = CreatePointMat(CreatePoints2());
            using Mat fundamental = CreateFundamentalMatrix();
            using var h1 = new Mat();
            using var h2 = new Mat();
            using var invalidPoints = new Mat(2, 1, MatType.CV_32FC1);
            using Mat mismatchedPoints = CreatePointMat(new[] { new Point2f(1.0F, 1.0F) });
            using var invalidFundamental = new Mat(2, 3, MatType.CV_64FC1);

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoRectifyUncalibrated(
                    invalidPoints,
                    points2,
                    fundamental,
                    new Size(640, 480),
                    h1,
                    h2));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoRectifyUncalibrated(
                    points1,
                    mismatchedPoints,
                    fundamental,
                    new Size(640, 480),
                    h1,
                    h2));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoRectifyUncalibrated(
                    points1,
                    points2,
                    invalidFundamental,
                    new Size(640, 480),
                    h1,
                    h2));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.StereoRectifyUncalibrated(
                    points1,
                    points2,
                    fundamental,
                    new Size(0, 480),
                    h1,
                    h2));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoRectifyUncalibrated(
                    points1,
                    points2,
                    fundamental,
                    new Size(640, 480),
                    h1,
                    h1));

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.StereoRectifyUncalibrated(
                    null!,
                    points2,
                    fundamental,
                    new Size(640, 480)));
            Assert.Throws<ArgumentNullException>(() =>
                new StereoRectifyUncalibratedResult(true, null!, h2));
            Assert.Throws<ArgumentNullException>(() =>
                new StereoRectifyUncalibratedResult(true, h1, null!));
        }

        [Fact]
        public void StereoRectifyUncalibratedOwnedAndCallerOwnedOutputsAgreeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat points1 = CreatePointMat(CreatePoints1());
            using Mat points2 = CreatePointMat(CreatePoints2());
            using Mat fundamental = CreateFundamentalMatrix();
            using var h1 = new Mat();
            using var h2 = new Mat();

            bool callerSuccess = Calib3DCv2.StereoRectifyUncalibrated(
                points1,
                points2,
                fundamental,
                new Size(640, 480),
                h1,
                h2,
                threshold: 5.0);

            StereoRectifyUncalibratedResult owned = Calib3DCv2.StereoRectifyUncalibrated(
                points1,
                points2,
                fundamental,
                new Size(640, 480),
                threshold: 5.0);

            using (owned.H1)
            using (owned.H2)
            {
                Assert.True(callerSuccess);
                Assert.True(owned.Success);
                AssertMatrixShape(h1, 3, 3);
                AssertMatrixShape(h2, 3, 3);
                AssertMatrixShape(owned.H1, 3, 3);
                AssertMatrixShape(owned.H2, 3, 3);
                AssertArrayNear(h1.ToArray<double>(), owned.H1.ToArray<double>(), 1.0e-9);
                AssertArrayNear(h2.ToArray<double>(), owned.H2.ToArray<double>(), 1.0e-9);
                Assert.Contains("Success=True", owned.ToString(), StringComparison.Ordinal);
                Assert.Contains("H1=3x3", owned.ToString(), StringComparison.Ordinal);
                Assert.Contains("H2=3x3", owned.ToString(), StringComparison.Ordinal);
            }
        }

        private static Point2f[] CreatePoints1()
        {
            return new[]
            {
                new Point2f(80.0F, 100.0F),
                new Point2f(140.0F, 120.0F),
                new Point2f(220.0F, 160.0F),
                new Point2f(300.0F, 200.0F),
                new Point2f(360.0F, 240.0F),
                new Point2f(420.0F, 300.0F),
                new Point2f(500.0F, 340.0F),
                new Point2f(560.0F, 380.0F)
            };
        }

        private static Point2f[] CreatePoints2()
        {
            Point2f[] points1 = CreatePoints1();
            var result = new Point2f[points1.Length];
            for (int i = 0; i < points1.Length; i++)
            {
                result[i] = new Point2f(points1[i].X + 24.0F, points1[i].Y);
            }

            return result;
        }

        private static Mat CreatePointMat(Point2f[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_32FC2);
            for (int i = 0; i < points.Length; i++)
            {
                result.SetValue(i, points[i]);
            }

            return result;
        }

        private static Mat CreateFundamentalMatrix()
        {
            var result = new Mat(3, 3, MatType.CV_64FC1);
            result.CopyFrom(new[]
            {
                0.0, 0.0, 0.0,
                0.0, 0.0, -1.0,
                0.0, 1.0, 0.0
            });
            return result;
        }

        private static void AssertMatrixShape(Mat value, int rows, int cols)
        {
            Assert.Equal(rows, value.Rows);
            Assert.Equal(cols, value.Cols);
            Assert.Equal(MatType.CV_64FC1, value.Type);
        }

        private static void AssertArrayNear(double[] expected, double[] actual, double tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.True(
                    Math.Abs(expected[i] - actual[i]) <= tolerance,
                    $"Expected {expected[i]:R}, actual {actual[i]:R}, tolerance {tolerance:R} at index {i}.");
            }
        }
    }
}
