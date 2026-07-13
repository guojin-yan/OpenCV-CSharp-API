using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class FisheyeCalibrationTests
    {
        [Fact]
        public void ResultObjectsValidateFisheyeShapes()
        {
            using (var cameraMatrix1 = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs1 = new Mat(4, 1, MatType.CV_64FC1))
            using (var cameraMatrix2 = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs2 = new Mat(1, 4, MatType.CV_64FC1))
            using (var r = new Mat(3, 3, MatType.CV_64FC1))
            using (var t = new Mat(3, 1, MatType.CV_64FC1))
            using (var rvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var invalidDistCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            using (var invalidRvecs = new Mat(2, 2, MatType.CV_64FC1))
            {
                var calibration = new FisheyeStereoCalibrationResult(
                    0.5,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraMatrix2,
                    distCoeffs2,
                    r,
                    t);
                var extended = new FisheyeStereoCalibrationExtendedResult(calibration, rvecs, tvecs);

                Assert.Equal(0.5, calibration.ReprojectionError);
                Assert.Equal(2, extended.ViewCount);
                Assert.Same(cameraMatrix1, calibration.CameraMatrix1);
                Assert.Same(rvecs, extended.Rvecs);
                Assert.Contains("ReprojectionError=0.5", calibration.ToString(), StringComparison.Ordinal);
                Assert.Contains("Rvecs=2x3", extended.ToString(), StringComparison.Ordinal);

                Assert.Throws<ArgumentNullException>(() =>
                    new FisheyeStereoCalibrationResult(
                        0.5,
                        null!,
                        distCoeffs1,
                        cameraMatrix2,
                        distCoeffs2,
                        r,
                        t));
                Assert.Throws<ArgumentException>(() =>
                    new FisheyeStereoCalibrationResult(
                        0.5,
                        cameraMatrix1,
                        invalidDistCoeffs,
                        cameraMatrix2,
                        distCoeffs2,
                        r,
                        t));
                Assert.Throws<ArgumentException>(() =>
                    new FisheyeStereoCalibrationExtendedResult(
                        calibration,
                        invalidRvecs,
                        tvecs));
            }
        }

        [Fact]
        public void FisheyeCalibrateValidatesPointGroupsFlagsAndCriteria()
        {
            Point3f[][] objectPoints = CreateObjectPointGroups();
            Point2f[][] imagePoints = CreateImagePointGroups(0.0F);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.FisheyeCalibrate(null!, imagePoints, CalibrationTestData.ImageSize));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FisheyeCalibrate(Array.Empty<Point3f[]>(), Array.Empty<Point2f[]>(), CalibrationTestData.ImageSize));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FisheyeCalibrate(objectPoints, new[] { imagePoints[0] }, CalibrationTestData.ImageSize));

            Point2f[][] mismatched = CopyPointGroups(imagePoints);
            mismatched[0] = CopyPrefix(mismatched[0], mismatched[0].Length - 1);
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FisheyeCalibrate(objectPoints, mismatched, CalibrationTestData.ImageSize));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.FisheyeCalibrate(objectPoints, imagePoints, new Size(0, 480)));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.FisheyeCalibrate(
                    objectPoints,
                    imagePoints,
                    CalibrationTestData.ImageSize,
                    flags: CalibrationFlags.ZeroTangentDist));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.FisheyeCalibrate(
                    objectPoints,
                    imagePoints,
                    CalibrationTestData.ImageSize,
                    criteria: new TermCriteria((TermCriteriaTypes)0, 10, 1.0e-6)));
        }

        [Fact]
        public void FisheyeStereoCalibrateValidatesPointGroupsAndInitialIntrinsics()
        {
            Point3f[][] objectPoints = CreateObjectPointGroups();
            Point2f[][] imagePoints1 = CreateImagePointGroups(0.0F);
            Point2f[][] imagePoints2 = CreateImagePointGroups(5.0F);

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FisheyeStereoCalibrate(
                    objectPoints,
                    imagePoints1,
                    new[] { imagePoints2[0] },
                    CalibrationTestData.ImageSize));

            Point2f[][] mismatched = CopyPointGroups(imagePoints2);
            mismatched[1] = CopyPrefix(mismatched[1], mismatched[1].Length - 1);
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FisheyeStereoCalibrate(
                    objectPoints,
                    imagePoints1,
                    mismatched,
                    CalibrationTestData.ImageSize));

            using (var cameraMatrix1 = new Mat())
            using (var distCoeffs1 = new Mat())
            using (var cameraMatrix2 = new Mat())
            using (var distCoeffs2 = new Mat())
            using (var r = new Mat())
            using (var t = new Mat())
            {
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.FisheyeStereoCalibrate(
                        objectPoints,
                        imagePoints1,
                        imagePoints2,
                        cameraMatrix1,
                        distCoeffs1,
                        cameraMatrix2,
                        distCoeffs2,
                        CalibrationTestData.ImageSize,
                        r,
                        t,
                        CalibrationFlags.FixIntrinsic));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FisheyeStereoCalibrate(
                        objectPoints,
                        imagePoints1,
                        imagePoints2,
                        cameraMatrix1,
                        distCoeffs1,
                        cameraMatrix2,
                        distCoeffs2,
                        CalibrationTestData.ImageSize,
                        r,
                        t,
                        CalibrationFlags.SameFocalLength));
            }
        }

        [Fact]
        public void CallerOwnedOverloadsValidateNullAndDisposedMatrices()
        {
            Point3f[][] objectPoints = CreateObjectPointGroups();
            Point2f[][] imagePoints1 = CreateImagePointGroups(0.0F);
            Point2f[][] imagePoints2 = CreateImagePointGroups(5.0F);

            using (var cameraMatrix = new Mat())
            using (var distCoeffs = new Mat())
            using (var rvecs = new Mat())
            using (var tvecs = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FisheyeCalibrate(
                        objectPoints,
                        imagePoints1,
                        CalibrationTestData.ImageSize,
                        cameraMatrix,
                        distCoeffs,
                        null!,
                        tvecs));

                cameraMatrix.Dispose();
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.FisheyeCalibrate(
                        objectPoints,
                        imagePoints1,
                        CalibrationTestData.ImageSize,
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs));
            }

            using (var cameraMatrix1 = new Mat())
            using (var distCoeffs1 = new Mat())
            using (var cameraMatrix2 = new Mat())
            using (var distCoeffs2 = new Mat())
            using (var r = new Mat())
            using (var t = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FisheyeStereoCalibrate(
                        objectPoints,
                        imagePoints1,
                        imagePoints2,
                        cameraMatrix1,
                        distCoeffs1,
                        cameraMatrix2,
                        distCoeffs2,
                        CalibrationTestData.ImageSize,
                        null!,
                        t,
                        CalibrationFlags.None));

                r.Dispose();
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.FisheyeStereoCalibrate(
                        objectPoints,
                        imagePoints1,
                        imagePoints2,
                        cameraMatrix1,
                        distCoeffs1,
                        cameraMatrix2,
                        distCoeffs2,
                        CalibrationTestData.ImageSize,
                        r,
                        t,
                        CalibrationFlags.None));
            }
        }

        [Fact]
        public void OwnedOverloadsRejectFlagsThatRequireInitialIntrinsics()
        {
            Point3f[][] objectPoints = CreateObjectPointGroups();
            Point2f[][] imagePoints1 = CreateImagePointGroups(0.0F);
            Point2f[][] imagePoints2 = CreateImagePointGroups(5.0F);

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FisheyeCalibrate(
                    objectPoints,
                    imagePoints1,
                    CalibrationTestData.ImageSize,
                    CalibrationFlags.UseIntrinsicGuess));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FisheyeStereoCalibrate(
                    objectPoints,
                    imagePoints1,
                    imagePoints2,
                    CalibrationTestData.ImageSize,
                    CalibrationFlags.FixIntrinsic));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FisheyeStereoCalibrateExtended(
                    objectPoints,
                    imagePoints1,
                    imagePoints2,
                    CalibrationTestData.ImageSize,
                    CalibrationFlags.UseIntrinsicGuess));
        }

        [Fact]
        public void FisheyeCalibrateRunsOnSyntheticDataWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticFisheyeCalibrationData(
                out Point3f[][] objectPoints,
                out Point2f[][] imagePoints,
                out Mat expectedCameraMatrix,
                out Mat expectedDistCoeffs);
            expectedCameraMatrix.Dispose();
            expectedDistCoeffs.Dispose();

            CalibrationResult result = Calib3DCv2.FisheyeCalibrate(
                objectPoints,
                imagePoints,
                CalibrationTestData.ImageSize,
                CalibrationFlags.RecomputeExtrinsic |
                CalibrationFlags.CheckCond |
                CalibrationFlags.FixSkew);
            try
            {
                Assert.True(double.IsFinite(result.ReprojectionError));
                Assert.InRange(result.ReprojectionError, 0.0, 0.1);
                Assert.Equal(3, result.CameraMatrix.Rows);
                Assert.Equal(3, result.CameraMatrix.Cols);
                Assert.Equal(4, result.DistCoeffs.Rows * result.DistCoeffs.Cols);
                Assert.Equal(objectPoints.Length, result.Rvecs.Rows);
                Assert.Equal(3, result.Rvecs.Cols);
                Assert.Equal(objectPoints.Length, result.Tvecs.Rows);
                Assert.Equal(3, result.Tvecs.Cols);
            }
            finally
            {
                Dispose(result);
            }
        }

        [Fact]
        public void FisheyeIntrinsicGuessPreservesFixedCameraValuesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticFisheyeCalibrationData(
                out Point3f[][] objectPoints,
                out Point2f[][] imagePoints,
                out Mat cameraMatrix,
                out Mat distCoeffs);
            using (cameraMatrix)
            using (distCoeffs)
            using (var expectedCameraMatrix = cameraMatrix.Clone())
            using (var rvecs = new Mat())
            using (var tvecs = new Mat())
            {
                double error = Calib3DCv2.FisheyeCalibrate(
                    objectPoints,
                    imagePoints,
                    CalibrationTestData.ImageSize,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    CalibrationFlags.UseIntrinsicGuess |
                    CalibrationFlags.FixFocalLength |
                    CalibrationFlags.FixPrincipalPoint |
                    CalibrationFlags.FixSkew |
                    CalibrationFlags.RecomputeExtrinsic);

                Assert.True(double.IsFinite(error));
                AssertMatNear(expectedCameraMatrix, cameraMatrix, 1.0e-10);
                Assert.Equal(4, distCoeffs.Rows * distCoeffs.Cols);
                for (int i = 0; i < 4; ++i)
                {
                    Assert.True(double.IsFinite(distCoeffs.GetValue<double>(i)));
                }
                Assert.Equal(objectPoints.Length, rvecs.Rows);
                Assert.Equal(objectPoints.Length, tvecs.Rows);
            }
        }

        [Fact]
        public void FixedIntrinsicStereoCompactAndExtendedAgreeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticFisheyeStereoCalibrationData(
                out Point3f[][] objectPoints,
                out Point2f[][] imagePoints1,
                out Point2f[][] imagePoints2,
                out Mat expectedCameraMatrix1,
                out Mat expectedDistCoeffs1,
                out Mat expectedCameraMatrix2,
                out Mat expectedDistCoeffs2);
            using (expectedCameraMatrix1)
            using (expectedDistCoeffs1)
            using (expectedCameraMatrix2)
            using (expectedDistCoeffs2)
            using (var cameraMatrix1 = expectedCameraMatrix1.Clone())
            using (var distCoeffs1 = expectedDistCoeffs1.Clone())
            using (var cameraMatrix2 = expectedCameraMatrix2.Clone())
            using (var distCoeffs2 = expectedDistCoeffs2.Clone())
            using (var r = new Mat())
            using (var t = new Mat())
            using (var extendedCameraMatrix1 = expectedCameraMatrix1.Clone())
            using (var extendedDistCoeffs1 = expectedDistCoeffs1.Clone())
            using (var extendedCameraMatrix2 = expectedCameraMatrix2.Clone())
            using (var extendedDistCoeffs2 = expectedDistCoeffs2.Clone())
            using (var extendedR = new Mat())
            using (var extendedT = new Mat())
            using (var rvecs = new Mat())
            using (var tvecs = new Mat())
            {
                CalibrationFlags flags =
                    CalibrationFlags.FixIntrinsic |
                    CalibrationFlags.RecomputeExtrinsic |
                    CalibrationFlags.CheckCond |
                    CalibrationFlags.FixSkew;
                double compactError = Calib3DCv2.FisheyeStereoCalibrate(
                    objectPoints,
                    imagePoints1,
                    imagePoints2,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraMatrix2,
                    distCoeffs2,
                    CalibrationTestData.ImageSize,
                    r,
                    t,
                    flags);
                double extendedError = Calib3DCv2.FisheyeStereoCalibrateExtended(
                    objectPoints,
                    imagePoints1,
                    imagePoints2,
                    extendedCameraMatrix1,
                    extendedDistCoeffs1,
                    extendedCameraMatrix2,
                    extendedDistCoeffs2,
                    CalibrationTestData.ImageSize,
                    extendedR,
                    extendedT,
                    rvecs,
                    tvecs,
                    flags);

                Assert.True(double.IsFinite(compactError));
                Assert.InRange(compactError, 0.0, 0.1);
                Assert.InRange(Math.Abs(compactError - extendedError), 0.0, 1.0e-10);
                AssertMatNear(expectedCameraMatrix1, cameraMatrix1, 1.0e-10);
                AssertMatNear(expectedDistCoeffs1, distCoeffs1, 1.0e-10);
                AssertMatNear(expectedCameraMatrix2, cameraMatrix2, 1.0e-10);
                AssertMatNear(expectedDistCoeffs2, distCoeffs2, 1.0e-10);
                AssertMatNear(r, extendedR, 1.0e-9);
                AssertMatNear(t, extendedT, 1.0e-9);
                Assert.Equal(3, r.Rows);
                Assert.Equal(3, r.Cols);
                Assert.Equal(3, t.Rows);
                Assert.Equal(1, t.Cols);
                Assert.Equal(objectPoints.Length, rvecs.Rows);
                Assert.Equal(3, rvecs.Cols);
                Assert.Equal(objectPoints.Length, tvecs.Rows);
                Assert.Equal(3, tvecs.Cols);
            }
        }

        [Fact]
        public void JointFisheyeStereoCalibrationReturnsOwnedOutputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CalibrationTestData.CreateSyntheticFisheyeStereoCalibrationData(
                out Point3f[][] objectPoints,
                out Point2f[][] imagePoints1,
                out Point2f[][] imagePoints2,
                out Mat cameraMatrix1,
                out Mat distCoeffs1,
                out Mat cameraMatrix2,
                out Mat distCoeffs2);
            cameraMatrix1.Dispose();
            distCoeffs1.Dispose();
            cameraMatrix2.Dispose();
            distCoeffs2.Dispose();

            FisheyeStereoCalibrationExtendedResult result =
                Calib3DCv2.FisheyeStereoCalibrateExtended(
                    objectPoints,
                    imagePoints1,
                    imagePoints2,
                    CalibrationTestData.ImageSize,
                    CalibrationFlags.RecomputeExtrinsic |
                    CalibrationFlags.CheckCond |
                    CalibrationFlags.FixSkew);
            try
            {
                Assert.True(double.IsFinite(result.Calibration.ReprojectionError));
                Assert.InRange(result.Calibration.ReprojectionError, 0.0, 0.2);
                Assert.Equal(3, result.Calibration.CameraMatrix1.Rows);
                Assert.Equal(4, result.Calibration.DistCoeffs1.Rows * result.Calibration.DistCoeffs1.Cols);
                Assert.Equal(3, result.Calibration.CameraMatrix2.Rows);
                Assert.Equal(4, result.Calibration.DistCoeffs2.Rows * result.Calibration.DistCoeffs2.Cols);
                Assert.Equal(3, result.Calibration.R.Rows);
                Assert.Equal(3, result.Calibration.R.Cols);
                Assert.Equal(3, result.Calibration.T.Rows);
                Assert.Equal(1, result.Calibration.T.Cols);
                Assert.Equal(objectPoints.Length, result.ViewCount);
                Assert.Equal(3, result.Rvecs.Cols);
                Assert.Equal(3, result.Tvecs.Cols);
            }
            finally
            {
                Dispose(result);
            }
        }

        private static Point3f[][] CreateObjectPointGroups()
        {
            Point3f[] view =
            {
                new Point3f(0.0F, 0.0F, 0.0F),
                new Point3f(1.0F, 0.0F, 0.0F),
                new Point3f(2.0F, 0.0F, 0.0F),
                new Point3f(0.0F, 1.0F, 0.0F),
                new Point3f(1.0F, 1.0F, 0.0F),
                new Point3f(2.0F, 1.0F, 0.0F)
            };
            return new[] { view, (Point3f[])view.Clone() };
        }

        private static Point2f[][] CreateImagePointGroups(float offset)
        {
            return new[]
            {
                new[]
                {
                    new Point2f(100.0F + offset, 120.0F),
                    new Point2f(180.0F + offset, 120.0F),
                    new Point2f(260.0F + offset, 120.0F),
                    new Point2f(100.0F + offset, 200.0F),
                    new Point2f(180.0F + offset, 200.0F),
                    new Point2f(260.0F + offset, 200.0F)
                },
                new[]
                {
                    new Point2f(102.0F + offset, 118.0F),
                    new Point2f(181.0F + offset, 119.0F),
                    new Point2f(258.0F + offset, 121.0F),
                    new Point2f(103.0F + offset, 198.0F),
                    new Point2f(181.0F + offset, 199.0F),
                    new Point2f(259.0F + offset, 201.0F)
                }
            };
        }

        private static Point2f[][] CopyPointGroups(Point2f[][] source)
        {
            var result = new Point2f[source.Length][];
            for (int i = 0; i < source.Length; ++i)
            {
                result[i] = (Point2f[])source[i].Clone();
            }
            return result;
        }

        private static Point2f[] CopyPrefix(Point2f[] source, int length)
        {
            var result = new Point2f[length];
            Array.Copy(source, result, length);
            return result;
        }

        private static void AssertMatNear(Mat expected, Mat actual, double tolerance)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Cols, actual.Cols);
            int count = expected.Rows * expected.Cols;
            for (int i = 0; i < count; ++i)
            {
                Assert.InRange(
                    Math.Abs(expected.GetValue<double>(i) - actual.GetValue<double>(i)),
                    0.0,
                    tolerance);
            }
        }

        private static void Dispose(CalibrationResult result)
        {
            result.CameraMatrix.Dispose();
            result.DistCoeffs.Dispose();
            result.Rvecs.Dispose();
            result.Tvecs.Dispose();
        }

        private static void Dispose(FisheyeStereoCalibrationExtendedResult result)
        {
            result.Calibration.CameraMatrix1.Dispose();
            result.Calibration.DistCoeffs1.Dispose();
            result.Calibration.CameraMatrix2.Dispose();
            result.Calibration.DistCoeffs2.Dispose();
            result.Calibration.R.Dispose();
            result.Calibration.T.Dispose();
            result.Rvecs.Dispose();
            result.Tvecs.Dispose();
        }
    }
}
