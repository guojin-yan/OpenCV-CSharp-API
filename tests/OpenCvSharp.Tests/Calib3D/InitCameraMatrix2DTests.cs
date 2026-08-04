using System;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    public sealed class InitCameraMatrix2DTests
    {
        [Fact]
        public void ValidatesGroupedInputsBeforeNativeCall()
        {
            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.InitCameraMatrix2D(null!, imagePoints, CalibrationTestData.ImageSize));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.InitCameraMatrix2D(Array.Empty<Point3f[]>(), Array.Empty<Point2f[]>(), CalibrationTestData.ImageSize));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.InitCameraMatrix2D(objectPoints, new[] { imagePoints[0] }, CalibrationTestData.ImageSize));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.InitCameraMatrix2D(objectPoints, imagePoints, new Size(0, 480)));
        }

        [Fact]
        public void ValidatesOutputPlanarityAndFiniteAspectRatioBeforeNativeCall()
        {
            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);
            Point3f[][] nonPlanar = CloneObjectPoints(objectPoints);
            nonPlanar[0][0] = new Point3f(nonPlanar[0][0].X, nonPlanar[0][0].Y, 0.01F);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.InitCameraMatrix2D(objectPoints, imagePoints, CalibrationTestData.ImageSize, null!));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.InitCameraMatrix2D(nonPlanar, imagePoints, CalibrationTestData.ImageSize));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.InitCameraMatrix2D(objectPoints, imagePoints, CalibrationTestData.ImageSize, double.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.InitCameraMatrix2D(objectPoints, imagePoints, CalibrationTestData.ImageSize, double.PositiveInfinity));
        }

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        public void AcceptsNonPositiveAspectRatiosWhenNativeSmokeIsEnabled(double aspectRatio)
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);
            using (Mat cameraMatrix = Calib3DCv2.InitCameraMatrix2D(
                objectPoints,
                imagePoints,
                CalibrationTestData.ImageSize,
                aspectRatio))
            {
                AssertCameraMatrix(cameraMatrix);
            }
        }

        [Fact]
        public void PositiveAspectRatioConstrainsFocalLengthsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            const double aspectRatio = 1.25;
            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);
            using (Mat cameraMatrix = Calib3DCv2.InitCameraMatrix2D(
                objectPoints,
                imagePoints,
                CalibrationTestData.ImageSize,
                aspectRatio))
            {
                AssertCameraMatrix(cameraMatrix);
                double[] values = cameraMatrix.ToArray<double>();
                Assert.InRange(values[0] / values[4], aspectRatio - 1.0e-9, aspectRatio + 1.0e-9);
            }
        }

        [Fact]
        public void OwnedOverloadReturnsCenteredFiniteCameraMatrixWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);
            using (Mat cameraMatrix = Calib3DCv2.InitCameraMatrix2D(
                objectPoints,
                imagePoints,
                CalibrationTestData.ImageSize,
                0.0))
            {
                AssertCameraMatrix(cameraMatrix);
                double[] values = cameraMatrix.ToArray<double>();
                Assert.InRange(values[2], 319.499999, 319.500001);
                Assert.InRange(values[5], 239.499999, 239.500001);
                Assert.Equal(1.0, values[8]);
            }
        }

        private static Point3f[][] CloneObjectPoints(Point3f[][] source)
        {
            var clone = new Point3f[source.Length][];
            for (int i = 0; i < source.Length; ++i)
            {
                clone[i] = (Point3f[])source[i].Clone();
            }

            return clone;
        }

        private static void AssertCameraMatrix(Mat cameraMatrix)
        {
            Assert.Equal(3, cameraMatrix.Rows);
            Assert.Equal(3, cameraMatrix.Cols);
            Assert.Equal(1, cameraMatrix.Channels);
            double[] values = cameraMatrix.ToArray<double>();
            Assert.Equal(9, values.Length);
            foreach (double value in values)
            {
                Assert.True(double.IsFinite(value));
            }

            Assert.True(values[0] > 0.0);
            Assert.True(values[4] > 0.0);
        }
    }
}
