using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    public sealed class StereoCalibrationTests
    {
        [Fact]
        public void StereoResultObjectsExposeMatrices()
        {
            using (var cameraMatrix1 = new Mat())
            using (var distCoeffs1 = new Mat())
            using (var cameraMatrix2 = new Mat())
            using (var distCoeffs2 = new Mat())
            using (var r = new Mat())
            using (var t = new Mat())
            using (var e = new Mat())
            using (var f = new Mat())
            using (var rvecs = new Mat())
            using (var tvecs = new Mat())
            using (var perViewErrors = new Mat())
            {
                var calibration = new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f);
                var extended = new StereoCalibrationExtendedResult(calibration, rvecs, tvecs, perViewErrors);

                Assert.Equal(2.5, calibration.ReprojectionError);
                Assert.Same(cameraMatrix1, calibration.CameraMatrix1);
                Assert.Same(distCoeffs1, calibration.DistCoeffs1);
                Assert.Same(cameraMatrix2, calibration.CameraMatrix2);
                Assert.Same(distCoeffs2, calibration.DistCoeffs2);
                Assert.Same(r, calibration.R);
                Assert.Same(t, calibration.T);
                Assert.Same(e, calibration.E);
                Assert.Same(f, calibration.F);
                Assert.Same(rvecs, extended.Rvecs);
                Assert.Same(tvecs, extended.Tvecs);
                Assert.Same(perViewErrors, extended.PerViewErrors);
                Assert.Contains("ReprojectionError=2.5", calibration.ToString(), StringComparison.Ordinal);
                Assert.Contains("CameraMatrix1=0x0", calibration.ToString(), StringComparison.Ordinal);
                Assert.Contains("DistCoeffs2=0x0", calibration.ToString(), StringComparison.Ordinal);
                Assert.Contains("Calibration={ReprojectionError=2.5", extended.ToString(), StringComparison.Ordinal);
                Assert.Contains("Rvecs=0x0", extended.ToString(), StringComparison.Ordinal);
                Assert.Contains("PerViewErrors=0x0", extended.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void StereoCalibrationResultFormatsInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                using (var cameraMatrix1 = new Mat())
                using (var distCoeffs1 = new Mat())
                using (var cameraMatrix2 = new Mat())
                using (var distCoeffs2 = new Mat())
                using (var r = new Mat())
                using (var t = new Mat())
                using (var e = new Mat())
                using (var f = new Mat())
                using (var rvecs = new Mat())
                using (var tvecs = new Mat())
                using (var perViewErrors = new Mat())
                {
                    var calibration = new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f);
                    var extended = new StereoCalibrationExtendedResult(calibration, rvecs, tvecs, perViewErrors);

                    Assert.Contains("ReprojectionError=2.5", calibration.ToString(), StringComparison.Ordinal);
                    Assert.Contains("Calibration={ReprojectionError=2.5", extended.ToString(), StringComparison.Ordinal);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void StereoCalibrationResultConstructorsRejectNullMatrices()
        {
            using (var cameraMatrix1 = new Mat())
            using (var distCoeffs1 = new Mat())
            using (var cameraMatrix2 = new Mat())
            using (var distCoeffs2 = new Mat())
            using (var r = new Mat())
            using (var t = new Mat())
            using (var e = new Mat())
            using (var f = new Mat())
            using (var rvecs = new Mat())
            using (var tvecs = new Mat())
            using (var perViewErrors = new Mat())
            {
                var calibration = new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f);

                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationResult(2.5, null!, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationResult(2.5, cameraMatrix1, null!, cameraMatrix2, distCoeffs2, r, t, e, f));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, null!, distCoeffs2, r, t, e, f));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, null!, r, t, e, f));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, null!, t, e, f));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, null!, e, f));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, null!, f));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, null!));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationExtendedResult(calibration, null!, tvecs, perViewErrors));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationExtendedResult(calibration, rvecs, null!, perViewErrors));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationExtendedResult(calibration, rvecs, tvecs, null!));
            }
        }

        [Fact]
        public void StereoCalibrationExtendedResultConstructorRejectsMismatchedVectorRows()
        {
            using (var cameraMatrix1 = new Mat())
            using (var distCoeffs1 = new Mat())
            using (var cameraMatrix2 = new Mat())
            using (var distCoeffs2 = new Mat())
            using (var r = new Mat())
            using (var t = new Mat())
            using (var e = new Mat())
            using (var f = new Mat())
            using (var rvecs = new Mat(3, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var perViewErrors = new Mat())
            {
                var calibration = new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f);

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    new StereoCalibrationExtendedResult(calibration, rvecs, tvecs, perViewErrors));

                Assert.Equal("tvecs", exception.ParamName);
            }
        }

        [Fact]
        public void StereoCalibrationExtendedResultConstructorRejectsNonEmptyPoseVectorsWithoutThreeColumns()
        {
            using (var cameraMatrix1 = new Mat())
            using (var distCoeffs1 = new Mat())
            using (var cameraMatrix2 = new Mat())
            using (var distCoeffs2 = new Mat())
            using (var r = new Mat())
            using (var t = new Mat())
            using (var e = new Mat())
            using (var f = new Mat())
            using (var rvecs = new Mat(3, 2, MatType.CV_64FC1))
            using (var tvecs = new Mat(3, 3, MatType.CV_64FC1))
            using (var validRvecs = new Mat(3, 3, MatType.CV_64FC1))
            using (var invalidTvecs = new Mat(3, 2, MatType.CV_64FC1))
            using (var perViewErrors = new Mat(3, 2, MatType.CV_64FC1))
            {
                var calibration = new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f);

                ArgumentException rvecException = Assert.Throws<ArgumentException>(() =>
                    new StereoCalibrationExtendedResult(calibration, rvecs, tvecs, perViewErrors));
                ArgumentException tvecException = Assert.Throws<ArgumentException>(() =>
                    new StereoCalibrationExtendedResult(calibration, validRvecs, invalidTvecs, perViewErrors));

                Assert.Equal("rvecs", rvecException.ParamName);
                Assert.Equal("tvecs", tvecException.ParamName);
            }
        }

        [Fact]
        public void StereoCalibrationExtendedResultConstructorRejectsMismatchedPerViewErrorRows()
        {
            using (var cameraMatrix1 = new Mat())
            using (var distCoeffs1 = new Mat())
            using (var cameraMatrix2 = new Mat())
            using (var distCoeffs2 = new Mat())
            using (var r = new Mat())
            using (var t = new Mat())
            using (var e = new Mat())
            using (var f = new Mat())
            using (var rvecs = new Mat(3, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(3, 3, MatType.CV_64FC1))
            using (var perViewErrors = new Mat(2, 2, MatType.CV_64FC1))
            {
                var calibration = new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f);

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    new StereoCalibrationExtendedResult(calibration, rvecs, tvecs, perViewErrors));

                Assert.Equal("perViewErrors", exception.ParamName);
            }
        }

        [Fact]
        public void StereoCalibrationExtendedResultConstructorRejectsNonEmptyPerViewErrorsWithoutTwoColumns()
        {
            using (var cameraMatrix1 = new Mat())
            using (var distCoeffs1 = new Mat())
            using (var cameraMatrix2 = new Mat())
            using (var distCoeffs2 = new Mat())
            using (var r = new Mat())
            using (var t = new Mat())
            using (var e = new Mat())
            using (var f = new Mat())
            using (var rvecs = new Mat(3, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(3, 3, MatType.CV_64FC1))
            using (var perViewErrors = new Mat(3, 1, MatType.CV_64FC1))
            {
                var calibration = new StereoCalibrationResult(2.5, cameraMatrix1, distCoeffs1, cameraMatrix2, distCoeffs2, r, t, e, f);

                ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                    new StereoCalibrationExtendedResult(calibration, rvecs, tvecs, perViewErrors));

                Assert.Equal("perViewErrors", exception.ParamName);
            }
        }

        [Fact]
        public void StereoCalibrateValidatesPointGroupShapeBeforeNativeCall()
        {
            Point3f[][] objectPoints = CreateObjectPointGroups();
            Point2f[][] imagePoints1 = CreateImagePointGroups(0.0F);
            Point2f[][] imagePoints2 = CreateImagePointGroups(4.0F);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.StereoCalibrate(null!, imagePoints1, imagePoints2, new Size(640, 480)));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoCalibrate(objectPoints, imagePoints1, new[] { imagePoints2[0] }, new Size(640, 480)));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoCalibrate(objectPoints, imagePoints1, new[] { new[] { imagePoints2[0][0] }, imagePoints2[1] }, new Size(640, 480)));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.StereoCalibrate(objectPoints, imagePoints1, imagePoints2, new Size(640, 0)));
        }

        [Fact]
        public void Rectify3CollinearValidatesPointGroupsBeforeNativeCall()
        {
            Point2f[][] imagePoints1 = CreateImagePointGroups(0.0F);
            Point2f[][] imagePoints3 = CreateImagePointGroups(6.0F);

            using (Mat cameraMatrix1 = new Mat())
            using (Mat distCoeffs1 = new Mat())
            using (Mat cameraMatrix2 = new Mat())
            using (Mat distCoeffs2 = new Mat())
            using (Mat cameraMatrix3 = new Mat())
            using (Mat distCoeffs3 = new Mat())
            using (Mat r12 = new Mat())
            using (Mat t12 = new Mat())
            using (Mat r13 = new Mat())
            using (Mat t13 = new Mat())
            using (Mat r1 = new Mat())
            using (Mat r2 = new Mat())
            using (Mat r3 = new Mat())
            using (Mat p1 = new Mat())
            using (Mat p2 = new Mat())
            using (Mat p3 = new Mat())
            using (Mat q = new Mat())
            {
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.Rectify3Collinear(
                        cameraMatrix1,
                        distCoeffs1,
                        cameraMatrix2,
                        distCoeffs2,
                        cameraMatrix3,
                        distCoeffs3,
                        imagePoints1,
                        new[] { imagePoints3[0] },
                        new Size(640, 480),
                        r12,
                        t12,
                        r13,
                        t13,
                        r1,
                        r2,
                        r3,
                        p1,
                        p2,
                        p3,
                        q,
                        -1.0,
                        new Size()));
            }
        }

        [Fact]
        public void Rectify3CollinearResultStoresValues()
        {
            var result = new Rectify3CollinearResult(0.75F, new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8));

            Assert.Equal(0.75F, result.Scale);
            Assert.Equal(new Rect(1, 2, 3, 4), result.ValidPixROI1);
            Assert.Equal(new Rect(5, 6, 7, 8), result.ValidPixROI2);
            Assert.Equal(new Rectify3CollinearResult(0.75F, new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8)), result);
            Assert.True(result == new Rectify3CollinearResult(0.75F, new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8)));
            Assert.True(result != new Rectify3CollinearResult(1.0F, new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8)));
            Assert.Equal(new Rectify3CollinearResult(0.75F, new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8)).GetHashCode(), result.GetHashCode());
            Assert.Equal("{Scale=0.75,ValidPixROI1={X=1,Y=2,Width=3,Height=4},ValidPixROI2={X=5,Y=6,Width=7,Height=8}}", result.ToString());
        }

        [Fact]
        public void Rectify3CollinearResultFormatsInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var result = new Rectify3CollinearResult(0.75F, new Rect(1, 2, 3, 4), new Rect(5, 6, 7, 8));

                Assert.Equal("{Scale=0.75,ValidPixROI1={X=1,Y=2,Width=3,Height=4},ValidPixROI2={X=5,Y=6,Width=7,Height=8}}", result.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void Rectify3CollinearResultHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(36, Marshal.SizeOf<Rectify3CollinearResult>());
            Assert.Equal(0, FieldOffset<Rectify3CollinearResult>("<Scale>k__BackingField"));
            Assert.Equal(4, FieldOffset<Rectify3CollinearResult>("<ValidPixROI1>k__BackingField"));
            Assert.Equal(20, FieldOffset<Rectify3CollinearResult>("<ValidPixROI2>k__BackingField"));
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

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }
    }
}
