using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    public sealed class CalibrationROTests
    {
        [Fact]
        public void ResultObjectsAcceptEmptyRefinedOutputsForStandardCalibration()
        {
            using (var cameraMatrix = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            using (var rvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var newObjectPoints = new Mat())
            using (var stdDeviationsIntrinsics = new Mat())
            using (var stdDeviationsExtrinsics = new Mat())
            using (var stdDeviationsObjectPoints = new Mat())
            using (var perViewErrors = new Mat(2, 1, MatType.CV_64FC1))
            {
                var calibration = new CalibrationResult(0.25, cameraMatrix, distCoeffs, rvecs, tvecs);
                var ro = new CalibrationROResult(calibration, newObjectPoints);
                var extended = new CalibrationROExtendedResult(
                    ro,
                    stdDeviationsIntrinsics,
                    stdDeviationsExtrinsics,
                    stdDeviationsObjectPoints,
                    perViewErrors);

                Assert.Equal(2, ro.ViewCount);
                Assert.Equal(0, ro.ObjectPointCount);
                Assert.Equal(2, extended.ViewCount);
                Assert.Same(newObjectPoints, ro.NewObjectPoints);
                Assert.Same(stdDeviationsObjectPoints, extended.StdDeviationsObjectPoints);
            }
        }

        [Fact]
        public void ResultObjectsRejectInvalidRefinedOutputShapes()
        {
            using (var cameraMatrix = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            using (var rvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var invalidNewObjectPoints = new Mat(4, 2, MatType.CV_64FC1))
            using (var validNewObjectPoints = new Mat(4, 3, MatType.CV_64FC1))
            using (var stdDeviationsIntrinsics = new Mat())
            using (var stdDeviationsExtrinsics = new Mat())
            using (var invalidStdDeviationsObjectPoints = new Mat(11, 1, MatType.CV_64FC1))
            using (var perViewErrors = new Mat(2, 1, MatType.CV_64FC1))
            {
                var calibration = new CalibrationResult(0.25, cameraMatrix, distCoeffs, rvecs, tvecs);

                ArgumentException newPointsException = Assert.Throws<ArgumentException>(() =>
                    new CalibrationROResult(calibration, invalidNewObjectPoints));
                Assert.Equal("newObjectPoints", newPointsException.ParamName);

                var ro = new CalibrationROResult(calibration, validNewObjectPoints);
                ArgumentException standardDeviationException = Assert.Throws<ArgumentException>(() =>
                    new CalibrationROExtendedResult(
                        ro,
                        stdDeviationsIntrinsics,
                        stdDeviationsExtrinsics,
                        invalidStdDeviationsObjectPoints,
                        perViewErrors));
                Assert.Equal("stdDeviationsObjectPoints", standardDeviationException.ParamName);
            }
        }

        [Fact]
        public void CalibrateCameraROValidatesGroupedInputsBeforeNativeCall()
        {
            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.CalibrateCameraRO(null!, imagePoints, new Size(640, 480), 6));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateCameraRO(Array.Empty<Point3f[]>(), Array.Empty<Point2f[]>(), new Size(640, 480), 6));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateCameraRO(objectPoints, new[] { imagePoints[0] }, new Size(640, 480), 6));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateCameraRO(
                    new[] { objectPoints[0] },
                    new[] { new[] { imagePoints[0][0] } },
                    new Size(640, 480),
                    6));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.CalibrateCameraRO(objectPoints, imagePoints, new Size(0, 480), 6));
        }

        [Fact]
        public void CalibrateCameraROExtendedValidatesOutputMatricesBeforeNativeCall()
        {
            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.CalibrateCameraROExtended(
                    objectPoints,
                    imagePoints,
                    new Size(640, 480),
                    6,
                    null!,
                    new Mat(),
                    new Mat(),
                    new Mat(),
                    new Mat(),
                    new Mat(),
                    new Mat(),
                    new Mat(),
                    new Mat()));
        }

        [Fact]
        public void CalibrateCameraROAcceptsStandardCalibrationFallbackWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);
            CalibrationROResult result = Calib3DCv2.CalibrateCameraRO(
                objectPoints,
                imagePoints,
                new Size(640, 480),
                -1);

            try
            {
                Assert.True(double.IsFinite(result.Calibration.ReprojectionError));
                Assert.Equal(objectPoints.Length, result.ViewCount);
                Assert.True(result.NewObjectPoints.Empty);
                Assert.Equal(0, result.ObjectPointCount);
            }
            finally
            {
                Dispose(result);
            }
        }

        [Fact]
        public void CalibrateCameraRORunsOnSyntheticDataWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);
            CalibrationROResult result = Calib3DCv2.CalibrateCameraRO(
                objectPoints,
                imagePoints,
                new Size(640, 480),
                6);

            try
            {
                Assert.True(double.IsFinite(result.Calibration.ReprojectionError));
                Assert.InRange(result.Calibration.ReprojectionError, 0.0, 0.1);
                Assert.Equal(objectPoints.Length, result.ViewCount);
                Assert.Equal(objectPoints[0].Length, result.ObjectPointCount);
                Assert.Equal(objectPoints[0].Length, result.NewObjectPoints.Rows);
                Assert.Equal(3, result.NewObjectPoints.Cols);
                Assert.Equal(1, result.NewObjectPoints.Channels);
                Assert.Equal(objectPoints.Length, result.Calibration.Rvecs.Rows);
                Assert.Equal(objectPoints.Length, result.Calibration.Tvecs.Rows);
            }
            finally
            {
                Dispose(result);
            }
        }

        [Fact]
        public void CalibrateCameraROExtendedRunsOnSyntheticDataWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticCalibrationData(out Point3f[][] objectPoints, out Point2f[][] imagePoints);
            CalibrationROExtendedResult result = Calib3DCv2.CalibrateCameraROExtended(
                objectPoints,
                imagePoints,
                new Size(640, 480),
                6);

            try
            {
                Assert.True(double.IsFinite(result.Calibration.Calibration.ReprojectionError));
                Assert.InRange(result.Calibration.Calibration.ReprojectionError, 0.0, 0.1);
                Assert.Equal(objectPoints.Length, result.ViewCount);
                Assert.Equal(objectPoints[0].Length, result.Calibration.ObjectPointCount);
                Assert.False(result.StdDeviationsIntrinsics.Empty);
                Assert.False(result.StdDeviationsExtrinsics.Empty);
                Assert.Equal(objectPoints[0].Length * 3, result.StdDeviationsObjectPoints.Rows);
                Assert.Equal(1, result.StdDeviationsObjectPoints.Cols);
                Assert.Equal(objectPoints.Length, result.PerViewErrors.Rows);
                Assert.Equal(1, result.PerViewErrors.Cols);
                AssertAllFinite(result.StdDeviationsIntrinsics.ToArray<double>());
                AssertAllFinite(result.StdDeviationsExtrinsics.ToArray<double>());
                AssertAllFinite(result.StdDeviationsObjectPoints.ToArray<double>());
                AssertAllFinite(result.PerViewErrors.ToArray<double>());
            }
            finally
            {
                Dispose(result);
            }
        }

        private static void AssertAllFinite(double[] values)
        {
            Assert.NotEmpty(values);
            foreach (double value in values)
            {
                Assert.True(double.IsFinite(value));
            }
        }

        private static void Dispose(CalibrationROResult result)
        {
            result.Calibration.CameraMatrix.Dispose();
            result.Calibration.DistCoeffs.Dispose();
            result.Calibration.Rvecs.Dispose();
            result.Calibration.Tvecs.Dispose();
            result.NewObjectPoints.Dispose();
        }

        private static void Dispose(CalibrationROExtendedResult result)
        {
            Dispose(result.Calibration);
            result.StdDeviationsIntrinsics.Dispose();
            result.StdDeviationsExtrinsics.Dispose();
            result.StdDeviationsObjectPoints.Dispose();
            result.PerViewErrors.Dispose();
        }
    }
}
