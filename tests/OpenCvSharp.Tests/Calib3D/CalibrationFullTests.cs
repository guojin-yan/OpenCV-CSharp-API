using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    public sealed class CalibrationFullTests
    {
        [Fact]
        public void ResultObjectsExposeOwnedMatrices()
        {
            using (var cameraMatrix = new Mat())
            using (var distCoeffs = new Mat())
            using (var rvecs = new Mat())
            using (var tvecs = new Mat())
            using (var stdDeviationsIntrinsics = new Mat())
            using (var stdDeviationsExtrinsics = new Mat())
            using (var perViewErrors = new Mat())
            {
                var calibration = new CalibrationResult(1.25, cameraMatrix, distCoeffs, rvecs, tvecs);
                var extended = new CalibrationExtendedResult(calibration, stdDeviationsIntrinsics, stdDeviationsExtrinsics, perViewErrors);

                Assert.Equal(1.25, calibration.ReprojectionError);
                Assert.Same(cameraMatrix, calibration.CameraMatrix);
                Assert.Same(distCoeffs, calibration.DistCoeffs);
                Assert.Same(rvecs, calibration.Rvecs);
                Assert.Same(tvecs, calibration.Tvecs);
                Assert.Equal(1.25, extended.Calibration.ReprojectionError);
                Assert.Same(stdDeviationsIntrinsics, extended.StdDeviationsIntrinsics);
                Assert.Same(stdDeviationsExtrinsics, extended.StdDeviationsExtrinsics);
                Assert.Same(perViewErrors, extended.PerViewErrors);
                Assert.Equal("{ReprojectionError=1.25,CameraMatrix=0x0,DistCoeffs=0x0,Rvecs=0x0,Tvecs=0x0}", calibration.ToString());
                Assert.Contains("Calibration={ReprojectionError=1.25", extended.ToString(), StringComparison.Ordinal);
                Assert.Contains("StdDeviationsIntrinsics=0x0", extended.ToString(), StringComparison.Ordinal);
                Assert.Contains("PerViewErrors=0x0", extended.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void CalibrationResultFormatsInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                using (var cameraMatrix = new Mat())
                using (var distCoeffs = new Mat())
                using (var rvecs = new Mat())
                using (var tvecs = new Mat())
                using (var stdDeviationsIntrinsics = new Mat())
                using (var stdDeviationsExtrinsics = new Mat())
                using (var perViewErrors = new Mat())
                {
                    var calibration = new CalibrationResult(1.25, cameraMatrix, distCoeffs, rvecs, tvecs);
                    var extended = new CalibrationExtendedResult(calibration, stdDeviationsIntrinsics, stdDeviationsExtrinsics, perViewErrors);

                    Assert.Contains("ReprojectionError=1.25", calibration.ToString(), StringComparison.Ordinal);
                    Assert.Contains("Calibration={ReprojectionError=1.25", extended.ToString(), StringComparison.Ordinal);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void CalibrationResultConstructorsRejectNullMatrices()
        {
            using (var cameraMatrix = new Mat())
            using (var distCoeffs = new Mat())
            using (var rvecs = new Mat())
            using (var tvecs = new Mat())
            using (var stdDeviationsIntrinsics = new Mat())
            using (var stdDeviationsExtrinsics = new Mat())
            using (var perViewErrors = new Mat())
            {
                var calibration = new CalibrationResult(1.25, cameraMatrix, distCoeffs, rvecs, tvecs);

                Assert.Throws<ArgumentNullException>(() => new CalibrationResult(1.25, null!, distCoeffs, rvecs, tvecs));
                Assert.Throws<ArgumentNullException>(() => new CalibrationResult(1.25, cameraMatrix, null!, rvecs, tvecs));
                Assert.Throws<ArgumentNullException>(() => new CalibrationResult(1.25, cameraMatrix, distCoeffs, null!, tvecs));
                Assert.Throws<ArgumentNullException>(() => new CalibrationResult(1.25, cameraMatrix, distCoeffs, rvecs, null!));
                Assert.Throws<ArgumentNullException>(() => new CalibrationExtendedResult(calibration, null!, stdDeviationsExtrinsics, perViewErrors));
                Assert.Throws<ArgumentNullException>(() => new CalibrationExtendedResult(calibration, stdDeviationsIntrinsics, null!, perViewErrors));
                Assert.Throws<ArgumentNullException>(() => new CalibrationExtendedResult(calibration, stdDeviationsIntrinsics, stdDeviationsExtrinsics, null!));
            }
        }

        [Fact]
        public void CalibrationResultConstructorRejectsMismatchedVectorRows()
        {
            using (var cameraMatrix = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            using (var rvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(1, 3, MatType.CV_64FC1))
            {
                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    new CalibrationResult(1.25, cameraMatrix, distCoeffs, rvecs, tvecs));

                Assert.Equal("tvecs", exception.ParamName);
            }
        }

        [Fact]
        public void CalibrationResultConstructorRejectsNonEmptyPoseVectorsWithoutThreeColumns()
        {
            using (var cameraMatrix = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            using (var rvecs = new Mat(2, 2, MatType.CV_64FC1))
            using (var tvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var validRvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var invalidTvecs = new Mat(2, 2, MatType.CV_64FC1))
            {
                ArgumentException rvecException = Assert.Throws<ArgumentException>(() =>
                    new CalibrationResult(1.25, cameraMatrix, distCoeffs, rvecs, tvecs));
                ArgumentException tvecException = Assert.Throws<ArgumentException>(() =>
                    new CalibrationResult(1.25, cameraMatrix, distCoeffs, validRvecs, invalidTvecs));

                Assert.Equal("rvecs", rvecException.ParamName);
                Assert.Equal("tvecs", tvecException.ParamName);
            }
        }

        [Fact]
        public void CalibrationExtendedResultConstructorRejectsMismatchedPerViewErrorRows()
        {
            using (var cameraMatrix = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            using (var rvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var stdDeviationsIntrinsics = new Mat())
            using (var stdDeviationsExtrinsics = new Mat())
            using (var perViewErrors = new Mat(1, 1, MatType.CV_64FC1))
            {
                var calibration = new CalibrationResult(1.25, cameraMatrix, distCoeffs, rvecs, tvecs);

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    new CalibrationExtendedResult(calibration, stdDeviationsIntrinsics, stdDeviationsExtrinsics, perViewErrors));

                Assert.Equal("perViewErrors", exception.ParamName);
            }
        }

        [Fact]
        public void CalibrationExtendedResultConstructorRejectsNonEmptyPerViewErrorsWithoutOneColumn()
        {
            using (var cameraMatrix = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            using (var rvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var stdDeviationsIntrinsics = new Mat())
            using (var stdDeviationsExtrinsics = new Mat())
            using (var perViewErrors = new Mat(2, 2, MatType.CV_64FC1))
            {
                var calibration = new CalibrationResult(1.25, cameraMatrix, distCoeffs, rvecs, tvecs);

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    new CalibrationExtendedResult(calibration, stdDeviationsIntrinsics, stdDeviationsExtrinsics, perViewErrors));

                Assert.Equal("perViewErrors", exception.ParamName);
            }
        }

        [Fact]
        public void CalibrateCameraValidatesPointGroupShapeBeforeNativeCall()
        {
            Point3f[][] objectPoints = CreateObjectPointGroups();
            Point2f[][] imagePoints = CreateImagePointGroups(0.0F);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.CalibrateCamera(null!, imagePoints, new Size(640, 480)));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateCamera(Array.Empty<Point3f[]>(), Array.Empty<Point2f[]>(), new Size(640, 480)));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateCamera(objectPoints, new[] { imagePoints[0] }, new Size(640, 480)));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateCamera(new[] { Array.Empty<Point3f>() }, new[] { Array.Empty<Point2f>() }, new Size(640, 480)));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.CalibrateCamera(new[] { objectPoints[0] }, new[] { new[] { imagePoints[0][0] } }, new Size(640, 480)));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.CalibrateCamera(objectPoints, imagePoints, new Size(0, 480)));
        }

        [Fact]
        public void CalibrateCameraExtendedValidatesOutputMatricesBeforeNativeCall()
        {
            Point3f[][] objectPoints = CreateObjectPointGroups();
            Point2f[][] imagePoints = CreateImagePointGroups(0.0F);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.CalibrateCameraExtended(
                    objectPoints,
                    imagePoints,
                    new Size(640, 480),
                    null!,
                    new Mat(),
                    new Mat(),
                    new Mat(),
                    new Mat(),
                    new Mat(),
                    new Mat()));
        }

        [Fact]
        public void CalibrateCameraRunsOnSyntheticDataWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[][] objectPoints = CreateObjectPointGroups();
            Point2f[][] imagePoints = CreateImagePointGroups(0.0F);

            CalibrationResult result = Calib3DCv2.CalibrateCamera(objectPoints, imagePoints, new Size(640, 480));
            try
            {
                Assert.True(result.ReprojectionError >= 0.0);
                Assert.False(result.CameraMatrix.Empty);
                Assert.False(result.DistCoeffs.Empty);
                Assert.Equal(objectPoints.Length, result.Rvecs.Rows);
                Assert.Equal(3, result.Rvecs.Cols);
                Assert.Equal(objectPoints.Length, result.Tvecs.Rows);
                Assert.Equal(3, result.Tvecs.Cols);
            }
            finally
            {
                result.CameraMatrix.Dispose();
                result.DistCoeffs.Dispose();
                result.Rvecs.Dispose();
                result.Tvecs.Dispose();
            }
        }

        private static Point3f[][] CreateObjectPointGroups()
        {
            Point3f[] view = new[]
            {
                new Point3f(0.0F, 0.0F, 0.0F),
                new Point3f(1.0F, 0.0F, 0.0F),
                new Point3f(2.0F, 0.0F, 0.0F),
                new Point3f(0.0F, 1.0F, 0.0F),
                new Point3f(1.0F, 1.0F, 0.0F),
                new Point3f(2.0F, 1.0F, 0.0F)
            };
            return new[] { view, view };
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

    }
}
