using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    public sealed class Calib3DTests
    {
        [Fact]
        public void Point3fStoresCoordinates()
        {
            var point = new Point3f(1.5F, 2.5F, 3.5F);

            Assert.Equal(1.5F, point.X);
            Assert.Equal(2.5F, point.Y);
            Assert.Equal(3.5F, point.Z);
            Assert.Equal(new Point3f(1.5F, 2.5F, 3.5F), point);
            Assert.NotEqual(new Point3f(1.5F, 2.5F, 4.0F), point);
        }

        [Fact]
        public void EnumValuesMatchOpenCvConstants()
        {
            Assert.Equal(8, (int)RobustEstimationAlgorithms.RANSAC);
            Assert.Equal(16, (int)RobustEstimationAlgorithms.RHO);
            Assert.Equal(38, (int)RobustEstimationAlgorithms.USACMagsac);
            Assert.Equal(1, (int)FundamentalMatMethods.FM7Point);
            Assert.Equal(2, (int)FundamentalMatMethods.FM8Point);
            Assert.Equal(6, (int)SolvePnPFlags.SQPNP);
            Assert.Equal(0x00400, (int)StereoRectifyFlags.ZeroDisparity);
            Assert.Equal(ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage, ChessboardFlags.Default);
            Assert.Equal(256, (int)ChessboardFlags.Plain);
            Assert.Equal(4, (int)CirclesGridFlags.Clustering);
            Assert.Equal(0x04000, (int)CalibrationFlags.RationalModel);
            Assert.Equal(1 << 26, (int)CalibrationFlags.StereoRegistration);
            Assert.Equal(0, (int)StereoBMPreFilterType.NormalizedResponse);
            Assert.Equal(1, (int)StereoBMPreFilterType.XSobel);
            Assert.Equal(4, StereoBM.DispShift);
            Assert.Equal(16, StereoBM.DispScale);
        }

        [Fact]
        public void ToPointMatCreatesTwoChannelMatrix()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f[] points = new[]
            {
                new Point2f(1.0F, 2.0F),
                new Point2f(3.0F, 4.0F)
            };

            using (Mat mat = Calib3DCv2.ToPointMat(points))
            {
                Assert.Equal(2, mat.Rows);
                Assert.Equal(1, mat.Cols);
                Assert.Equal(2, mat.Channels);
                Assert.Equal(MatType.CV_32FC2, mat.Type);
                Assert.Equal(new[] { 1.0F, 2.0F, 3.0F, 4.0F }, mat.ToArray<float>());
            }
        }

        [Fact]
        public void ToPointMatCreatesThreeChannelMatrix()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] points = new[]
            {
                new Point3f(1.0F, 2.0F, 3.0F),
                new Point3f(4.0F, 5.0F, 6.0F)
            };

            using (Mat mat = Calib3DCv2.ToPointMat(points))
            {
                Assert.Equal(2, mat.Rows);
                Assert.Equal(1, mat.Cols);
                Assert.Equal(3, mat.Channels);
                Assert.Equal(MatType.CV_32FC3, mat.Type);
                Assert.Equal(new[] { 1.0F, 2.0F, 3.0F, 4.0F, 5.0F, 6.0F }, mat.ToArray<float>());
            }
        }

        [Fact]
        public void RodriguesProducesIdentityMatrix()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat rvec = new Mat(3, 1, MatType.CV_64FC1))
            using (Mat rotation = new Mat())
            {
                rvec.CopyFrom<double>(new double[] { 0.0, 0.0, 0.0 });

                Calib3DCv2.Rodrigues(rvec, rotation);

                Assert.Equal(3, rotation.Rows);
                Assert.Equal(3, rotation.Cols);
                Assert.Equal(new[]
                {
                    1.0, 0.0, 0.0,
                    0.0, 1.0, 0.0,
                    0.0, 0.0, 1.0
                }, rotation.ToArray<double>());
            }
        }

        [Fact]
        public void ProjectPointsProjectsIdentityCamera()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] objectPoints = new[]
            {
                new Point3f(1.0F, 2.0F, 1.0F),
                new Point3f(2.0F, 4.0F, 2.0F)
            };

            using (Mat rvec = new Mat(3, 1, MatType.CV_64FC1))
            using (Mat tvec = new Mat(3, 1, MatType.CV_64FC1))
            using (Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (Mat distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            {
                rvec.SetTo(new Scalar(0.0));
                tvec.SetTo(new Scalar(0.0));
                distCoeffs.SetTo(new Scalar(0.0));

                using (Mat imagePoints = Calib3DCv2.ProjectPoints(objectPoints, rvec, tvec, cameraMatrix, distCoeffs))
                {
                    Assert.Equal(2, imagePoints.Rows);
                    Assert.Equal(1, imagePoints.Cols);
                    Assert.Equal(2, imagePoints.Channels);

                    float[] values = imagePoints.ToArray<float>();
                    Assert.Equal(1.0F, values[0], 5);
                    Assert.Equal(2.0F, values[1], 5);
                    Assert.Equal(1.0F, values[2], 5);
                    Assert.Equal(2.0F, values[3], 5);
                }
            }
        }

        [Fact]
        public void SolvePnPAcceptsPointArrays()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] objectPoints = new[]
            {
                new Point3f(-1.0F, -1.0F, 0.0F),
                new Point3f(1.0F, -1.0F, 0.0F),
                new Point3f(1.0F, 1.0F, 0.0F),
                new Point3f(-1.0F, 1.0F, 0.0F)
            };
            Point2f[] imagePoints = new[]
            {
                new Point2f(100.0F, 100.0F),
                new Point2f(200.0F, 100.0F),
                new Point2f(200.0F, 200.0F),
                new Point2f(100.0F, 200.0F)
            };

            using (Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (Mat distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            using (Mat rvec = new Mat())
            using (Mat tvec = new Mat())
            {
                cameraMatrix.SetValue(0, 100.0);
                cameraMatrix.SetValue(2, 150.0);
                cameraMatrix.SetValue(4, 100.0);
                cameraMatrix.SetValue(5, 150.0);
                distCoeffs.SetTo(new Scalar(0.0));

                bool solved = Calib3DCv2.SolvePnP(
                    objectPoints,
                    imagePoints,
                    cameraMatrix,
                    distCoeffs,
                    rvec,
                    tvec,
                    flags: SolvePnPFlags.IPPE);

                Assert.True(solved);
                Assert.False(rvec.Empty);
                Assert.False(tvec.Empty);
            }
        }

        [Fact]
        public void SolvePnPGenericReturnsPackedPoseMatrices()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] objectPoints = new[]
            {
                new Point3f(-1.0F, -1.0F, 0.0F),
                new Point3f(1.0F, -1.0F, 0.0F),
                new Point3f(1.0F, 1.0F, 0.0F),
                new Point3f(-1.0F, 1.0F, 0.0F)
            };
            Point2f[] imagePoints = new[]
            {
                new Point2f(100.0F, 100.0F),
                new Point2f(200.0F, 100.0F),
                new Point2f(200.0F, 200.0F),
                new Point2f(100.0F, 200.0F)
            };

            using (Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (Mat distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            {
                cameraMatrix.SetValue(0, 100.0);
                cameraMatrix.SetValue(2, 150.0);
                cameraMatrix.SetValue(4, 100.0);
                cameraMatrix.SetValue(5, 150.0);
                distCoeffs.SetTo(new Scalar(0.0));

                SolvePnPGenericResult result = Calib3DCv2.SolvePnPGeneric(
                    objectPoints,
                    imagePoints,
                    cameraMatrix,
                    distCoeffs,
                    flags: SolvePnPFlags.IPPE,
                    returnReprojectionError: true);
                try
                {
                    Assert.True(result.SolutionCount >= 1);
                    Assert.Equal(result.SolutionCount, result.Rvecs.Rows);
                    Assert.Equal(3, result.Rvecs.Cols);
                    Assert.Equal(result.SolutionCount, result.Tvecs.Rows);
                    Assert.Equal(3, result.Tvecs.Cols);
                    Assert.NotNull(result.ReprojectionError);
                    Assert.Contains("ReprojectionError=", result.ToString(), StringComparison.Ordinal);
                }
                finally
                {
                    result.Rvecs.Dispose();
                    result.Tvecs.Dispose();
                    result.ReprojectionError?.Dispose();
                }
            }
        }

        [Fact]
        public void PnPManagedValidationRunsBeforeNativeCall()
        {
            Point3f[] objectPoints = new[]
            {
                new Point3f(-1.0F, -1.0F, 0.0F),
                new Point3f(1.0F, -1.0F, 0.0F),
                new Point3f(1.0F, 1.0F, 0.0F),
                new Point3f(-1.0F, 1.0F, 0.0F)
            };
            Point2f[] imagePoints = new[]
            {
                new Point2f(100.0F, 100.0F),
                new Point2f(200.0F, 100.0F),
                new Point2f(200.0F, 200.0F),
                new Point2f(100.0F, 200.0F)
            };

            using (Mat objectPointMat = Calib3DCv2.ToPointMat(objectPoints))
            using (Mat imagePointMat = Calib3DCv2.ToPointMat(imagePoints))
            using (Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (Mat distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            using (Mat rvec = new Mat())
            using (Mat tvec = new Mat())
            using (Mat inliers = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints((Mat)null!, rvec, tvec, cameraMatrix, distCoeffs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints(objectPointMat, null!, tvec, cameraMatrix, distCoeffs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints(objectPointMat, rvec, null!, cameraMatrix, distCoeffs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints(objectPointMat, rvec, tvec, null!, distCoeffs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints(objectPointMat, rvec, tvec, cameraMatrix, null!));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints((Mat)null!, rvec, tvec, cameraMatrix, distCoeffs, new Mat()));

                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnP(null!, imagePointMat, cameraMatrix, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnP(objectPointMat, null!, cameraMatrix, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnP(objectPointMat, imagePointMat, null!, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnP(objectPointMat, imagePointMat, cameraMatrix, null!, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnP(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, null!, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnP(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, rvec, null!));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnP(null!, imagePoints, cameraMatrix, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnP(objectPoints, null!, cameraMatrix, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolvePnP(Array.Empty<Point3f>(), imagePoints, cameraMatrix, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolvePnP(objectPoints, Array.Empty<Point2f>(), cameraMatrix, distCoeffs, rvec, tvec));

                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRansac(null!, imagePointMat, cameraMatrix, distCoeffs, rvec, tvec, inliers: inliers));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRansac(objectPointMat, null!, cameraMatrix, distCoeffs, rvec, tvec, inliers: inliers));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRansac(objectPointMat, imagePointMat, null!, distCoeffs, rvec, tvec, inliers: inliers));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRansac(objectPointMat, imagePointMat, cameraMatrix, null!, rvec, tvec, inliers: inliers));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRansac(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, null!, tvec, inliers: inliers));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRansac(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, rvec, null!, inliers: inliers));

                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPGeneric(null!, imagePointMat, cameraMatrix, distCoeffs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPGeneric(objectPointMat, null!, cameraMatrix, distCoeffs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPGeneric(objectPointMat, imagePointMat, null!, distCoeffs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPGeneric(objectPointMat, imagePointMat, cameraMatrix, null!));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPGeneric(null!, imagePoints, cameraMatrix, distCoeffs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPGeneric(objectPoints, null!, cameraMatrix, distCoeffs));

                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineLM(null!, imagePointMat, cameraMatrix, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineLM(objectPointMat, null!, cameraMatrix, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineLM(objectPointMat, imagePointMat, null!, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineLM(objectPointMat, imagePointMat, cameraMatrix, null!, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineLM(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, null!, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineLM(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, rvec, null!));

                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineVVS(null!, imagePointMat, cameraMatrix, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineVVS(objectPointMat, null!, cameraMatrix, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineVVS(objectPointMat, imagePointMat, null!, distCoeffs, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineVVS(objectPointMat, imagePointMat, cameraMatrix, null!, rvec, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineVVS(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, null!, tvec));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolvePnPRefineVVS(objectPointMat, imagePointMat, cameraMatrix, distCoeffs, rvec, null!));
            }
        }

        [Fact]
        public void CalibrationUtilityResultTypesExposeValues()
        {
            var calibration = new CalibrationMatrixValuesResult(60.0, 45.0, 35.0, new Point2d(12.0, 10.0), 1.2);
            var sameCalibration = new CalibrationMatrixValuesResult(60.0, 45.0, 35.0, new Point2d(12.0, 10.0), 1.2);
            var differentCalibration = new CalibrationMatrixValuesResult(61.0, 45.0, 35.0, new Point2d(12.0, 10.0), 1.2);
            var recover = new RecoverPoseResult(7);
            var sameRecover = new RecoverPoseResult(7);
            var differentRecover = new RecoverPoseResult(8);
            var recoverWithoutInliers = new RecoverPoseResult(0);
            var rq = new RQDecomp3x3Result(1.0, 2.0, 3.0);
            var sameRq = new RQDecomp3x3Result(1.0, 2.0, 3.0);
            var differentRq = new RQDecomp3x3Result(1.0, 2.0, 4.0);

            Assert.Equal(60.0, calibration.FovX);
            Assert.Equal(45.0, calibration.FovY);
            Assert.Equal(35.0, calibration.FocalLength);
            Assert.Equal(new Point2d(12.0, 10.0), calibration.PrincipalPoint);
            Assert.Equal(1.2, calibration.AspectRatio);
            Assert.True(calibration == sameCalibration);
            Assert.False(calibration != sameCalibration);
            Assert.True(calibration != differentCalibration);
            Assert.False(calibration.Equals("not a result"));
            Assert.Equal(calibration.GetHashCode(), sameCalibration.GetHashCode());
            Assert.True(recover == sameRecover);
            Assert.False(recover != sameRecover);
            Assert.True(recover != differentRecover);
            Assert.False(recover.Equals("not a result"));
            Assert.Equal(recover.GetHashCode(), sameRecover.GetHashCode());
            Assert.True(recover.HasInliers);
            Assert.False(recoverWithoutInliers.HasInliers);
            Assert.Equal(0, recoverWithoutInliers.InlierCount);
            Assert.Throws<ArgumentOutOfRangeException>(() => new RecoverPoseResult(-1));
            Assert.True(rq == sameRq);
            Assert.False(rq != sameRq);
            Assert.True(rq != differentRq);
            Assert.False(rq.Equals("not a result"));
            Assert.Equal(rq.GetHashCode(), sameRq.GetHashCode());
            Assert.Contains("FovX=60", calibration.ToString());
            Assert.Contains("InlierCount=7", recover.ToString());
            Assert.Contains("X=1", rq.ToString());

            using (var cameraMatrix = new Mat())
            using (var rvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var emptyRvecs = new Mat())
            using (var emptyTvecs = new Mat())
            using (var mismatchedRvecs = new Mat(1, 3, MatType.CV_64FC1))
            using (var mismatchedTvecs = new Mat(1, 3, MatType.CV_64FC1))
            using (var invalidColumnRvecs = new Mat(2, 2, MatType.CV_64FC1))
            using (var invalidColumnTvecs = new Mat(2, 2, MatType.CV_64FC1))
            using (var reprojectionError = new Mat())
            {
                var optimal = new OptimalNewCameraMatrixResult(cameraMatrix, new Rect(1, 2, 3, 4));
                var solveWithError = new SolvePnPGenericResult(2, rvecs, tvecs, reprojectionError);
                var solveWithoutError = new SolvePnPGenericResult(0, emptyRvecs, emptyTvecs, null);

                Assert.Same(cameraMatrix, optimal.CameraMatrix);
                Assert.Equal(0, optimal.CameraMatrixRows);
                Assert.Equal(0, optimal.CameraMatrixCols);
                Assert.Equal(new Rect(1, 2, 3, 4), optimal.ValidPixROI);
                Assert.Equal("{CameraMatrix=0x0,ValidPixROI={X=1,Y=2,Width=3,Height=4}}", optimal.ToString());
                Assert.Throws<ArgumentNullException>(() => new OptimalNewCameraMatrixResult(null!, new Rect(1, 2, 3, 4)));

                Assert.Equal(2, solveWithError.SolutionCount);
                Assert.Same(rvecs, solveWithError.Rvecs);
                Assert.Same(tvecs, solveWithError.Tvecs);
                Assert.Same(reprojectionError, solveWithError.ReprojectionError);
                Assert.True(solveWithError.HasReprojectionError);
                Assert.Contains("ReprojectionError=0x0", solveWithError.ToString(), StringComparison.Ordinal);
                Assert.Throws<ArgumentNullException>(() => new SolvePnPGenericResult(2, null!, tvecs, reprojectionError));
                Assert.Throws<ArgumentNullException>(() => new SolvePnPGenericResult(2, rvecs, null!, reprojectionError));
                Assert.Throws<ArgumentOutOfRangeException>(() => new SolvePnPGenericResult(-1, rvecs, tvecs, reprojectionError));
                Assert.Equal("rvecs", Assert.Throws<ArgumentException>(() => new SolvePnPGenericResult(2, mismatchedRvecs, tvecs, reprojectionError)).ParamName);
                Assert.Equal("tvecs", Assert.Throws<ArgumentException>(() => new SolvePnPGenericResult(2, rvecs, mismatchedTvecs, reprojectionError)).ParamName);
                Assert.Equal("rvecs", Assert.Throws<ArgumentException>(() => new SolvePnPGenericResult(2, invalidColumnRvecs, tvecs, reprojectionError)).ParamName);
                Assert.Equal("tvecs", Assert.Throws<ArgumentException>(() => new SolvePnPGenericResult(2, rvecs, invalidColumnTvecs, reprojectionError)).ParamName);

                Assert.Equal(0, solveWithoutError.SolutionCount);
                Assert.Null(solveWithoutError.ReprojectionError);
                Assert.False(solveWithoutError.HasReprojectionError);
                Assert.Contains("ReprojectionError=<null>", solveWithoutError.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void CalibrationAggregateResultsExposeViewCounts()
        {
            using (var cameraMatrix = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
            using (var rvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(2, 3, MatType.CV_64FC1))
            using (var stdDeviationsIntrinsics = new Mat(1, 18, MatType.CV_64FC1))
            using (var stdDeviationsExtrinsics = new Mat(2, 6, MatType.CV_64FC1))
            using (var perViewErrors = new Mat(2, 1, MatType.CV_64FC1))
            {
                var calibration = new CalibrationResult(0.5, cameraMatrix, distCoeffs, rvecs, tvecs);
                var extended = new CalibrationExtendedResult(calibration, stdDeviationsIntrinsics, stdDeviationsExtrinsics, perViewErrors);

                Assert.Equal(2, calibration.ViewCount);
                Assert.Equal(2, extended.ViewCount);
                Assert.Same(cameraMatrix, calibration.CameraMatrix);
                Assert.Same(perViewErrors, extended.PerViewErrors);
                Assert.Throws<ArgumentNullException>(() => new CalibrationResult(0.5, null!, distCoeffs, rvecs, tvecs));
                Assert.Throws<ArgumentNullException>(() => new CalibrationExtendedResult(calibration, null!, stdDeviationsExtrinsics, perViewErrors));
            }

            using (var cameraMatrix1 = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs1 = new Mat(1, 5, MatType.CV_64FC1))
            using (var cameraMatrix2 = new Mat(3, 3, MatType.CV_64FC1))
            using (var distCoeffs2 = new Mat(1, 5, MatType.CV_64FC1))
            using (var rotation = new Mat(3, 3, MatType.CV_64FC1))
            using (var translation = new Mat(3, 1, MatType.CV_64FC1))
            using (var essential = new Mat(3, 3, MatType.CV_64FC1))
            using (var fundamental = new Mat(3, 3, MatType.CV_64FC1))
            using (var rvecs = new Mat(3, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(3, 3, MatType.CV_64FC1))
            using (var perViewErrors = new Mat(3, 2, MatType.CV_64FC1))
            {
                var stereo = new StereoCalibrationResult(
                    0.75,
                    cameraMatrix1,
                    distCoeffs1,
                    cameraMatrix2,
                    distCoeffs2,
                    rotation,
                    translation,
                    essential,
                    fundamental);
                var extended = new StereoCalibrationExtendedResult(stereo, rvecs, tvecs, perViewErrors);

                Assert.Equal(3, extended.ViewCount);
                Assert.Same(rvecs, extended.Rvecs);
                Assert.Same(perViewErrors, extended.PerViewErrors);
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationResult(0.75, null!, distCoeffs1, cameraMatrix2, distCoeffs2, rotation, translation, essential, fundamental));
                Assert.Throws<ArgumentNullException>(() => new StereoCalibrationExtendedResult(stereo, null!, tvecs, perViewErrors));
            }
        }

        [Fact]
        public void CalibrationUtilityResultTypesFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var calibration = new CalibrationMatrixValuesResult(60.5, 45.25, 35.125, new Point2d(12.5, 10.25), 1.2);
                var rq = new RQDecomp3x3Result(1.5, -2.25, 3.75);

                Assert.Equal("{FovX=60.5,FovY=45.25,FocalLength=35.125,PrincipalPoint={X=12.5,Y=10.25},AspectRatio=1.2}", calibration.ToString());
                Assert.Equal("{X=1.5,Y=-2.25,Z=3.75}", rq.ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void CalibrationUtilityResultTypesHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(4, Marshal.SizeOf<RecoverPoseResult>());
            Assert.Equal(24, Marshal.SizeOf<RQDecomp3x3Result>());
            Assert.Equal(48, Marshal.SizeOf<CalibrationMatrixValuesResult>());

            Assert.Equal(0, FieldOffset<RecoverPoseResult>("<InlierCount>k__BackingField"));

            Assert.Equal(0, FieldOffset<RQDecomp3x3Result>("<X>k__BackingField"));
            Assert.Equal(8, FieldOffset<RQDecomp3x3Result>("<Y>k__BackingField"));
            Assert.Equal(16, FieldOffset<RQDecomp3x3Result>("<Z>k__BackingField"));

            Assert.Equal(0, FieldOffset<CalibrationMatrixValuesResult>("<FovX>k__BackingField"));
            Assert.Equal(8, FieldOffset<CalibrationMatrixValuesResult>("<FovY>k__BackingField"));
            Assert.Equal(16, FieldOffset<CalibrationMatrixValuesResult>("<FocalLength>k__BackingField"));
            Assert.Equal(24, FieldOffset<CalibrationMatrixValuesResult>("<PrincipalPoint>k__BackingField"));
            Assert.Equal(40, FieldOffset<CalibrationMatrixValuesResult>("<AspectRatio>k__BackingField"));
        }

        [Fact]
        public void CalibrationPatternManagedValidationRunsBeforeNativeCall()
        {
            using (var image = new Mat())
            using (var corners = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FindChessboardCorners(null!, new Size(2, 2), corners));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FindChessboardCorners(image, new Size(2, 2), null!));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FindChessboardCorners(image, new Size(0, 2), corners));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.CheckChessboard(null!, new Size(2, 2)));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.CheckChessboard(image, new Size(0, 2)));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FindCirclesGrid(null!, new Size(2, 2), corners));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FindCirclesGrid(image, new Size(2, 2), null!));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.FindCirclesGrid(image, new Size(2, 0), corners));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.DrawChessboardCorners(null!, new Size(2, 2), corners, patternWasFound: false));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.DrawChessboardCorners(image, new Size(2, 2), null!, patternWasFound: false));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.DrawChessboardCorners(image, new Size(0, 2), corners, patternWasFound: false));
            }
        }

        [Fact]
        public void EpipolarAndRectificationManagedValidationRunsBeforeNativeCall()
        {
            using (var mat = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ComputeCorrespondEpilines(null!, 1, mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ComputeCorrespondEpilines(mat, 1, null!, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ComputeCorrespondEpilines(mat, 1, mat, null!));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.TriangulatePoints(null!, mat, mat, mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.TriangulatePoints(mat, null!, mat, mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.TriangulatePoints(mat, mat, null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.TriangulatePoints(mat, mat, mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.TriangulatePoints(mat, mat, mat, mat, null!));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.UndistortPoints(null!, mat, mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.UndistortPoints(mat, null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.UndistortPoints(mat, mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.UndistortPoints(mat, mat, mat, null!));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.InitUndistortRectifyMap(null!, mat, mat, mat, new Size(2, 2), MatType.CV_32FC1, mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.InitUndistortRectifyMap(mat, null!, mat, mat, new Size(2, 2), MatType.CV_32FC1, mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.InitUndistortRectifyMap(mat, mat, null!, mat, new Size(2, 2), MatType.CV_32FC1, mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.InitUndistortRectifyMap(mat, mat, mat, null!, new Size(2, 2), MatType.CV_32FC1, mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.InitUndistortRectifyMap(mat, mat, mat, mat, new Size(2, 2), MatType.CV_32FC1, null!, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.InitUndistortRectifyMap(mat, mat, mat, mat, new Size(2, 2), MatType.CV_32FC1, mat, null!));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.StereoRectifyUncalibrated(null!, mat, mat, new Size(2, 2), mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.StereoRectifyUncalibrated(mat, null!, mat, new Size(2, 2), mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.StereoRectifyUncalibrated(mat, mat, null!, new Size(2, 2), mat, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.StereoRectifyUncalibrated(mat, mat, mat, new Size(2, 2), null!, mat));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.StereoRectifyUncalibrated(mat, mat, mat, new Size(2, 2), mat, null!));
            }
        }

        [Fact]
        public void StereoBMManagedValidationRunsWhenNativeObjectIsAvailable()
        {
            using (StereoBM? stereo = TryCreateStereoBM())
            {
                if (stereo == null)
                {
                    return;
                }

                using (var left = new Mat())
                using (var right = new Mat())
                using (var disparity = new Mat())
                {
                    Assert.Throws<ArgumentNullException>(() => stereo.Compute(null!, right, disparity));
                    Assert.Throws<ArgumentNullException>(() => stereo.Compute(left, null!, disparity));
                    Assert.Throws<ArgumentNullException>(() => stereo.Compute(left, right, null!));
                    Assert.Throws<ArgumentNullException>(() => stereo.Compute(null!, right));
                    Assert.Throws<ArgumentNullException>(() => stereo.Compute(left, null!));

                    stereo.Dispose();
                    Assert.True(stereo.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => stereo.Compute(left, right, disparity));
                    Assert.Throws<ObjectDisposedException>(() => stereo.Compute(left, right));
                    Assert.Throws<ObjectDisposedException>(() => stereo.MinDisparity);
                    Assert.Throws<ObjectDisposedException>(() => stereo.MinDisparity = 0);
                    Assert.Throws<ObjectDisposedException>(() => stereo.NumDisparities);
                    Assert.Throws<ObjectDisposedException>(() => stereo.NumDisparities = 16);
                    Assert.Throws<ObjectDisposedException>(() => stereo.BlockSize);
                    Assert.Throws<ObjectDisposedException>(() => stereo.BlockSize = 9);
                    Assert.Throws<ObjectDisposedException>(() => stereo.PreFilterType);
                    Assert.Throws<ObjectDisposedException>(() => stereo.PreFilterType = StereoBMPreFilterType.XSobel);
                    Assert.Throws<ObjectDisposedException>(() => stereo.ROI1);
                    Assert.Throws<ObjectDisposedException>(() => stereo.ROI1 = new Rect(0, 0, 1, 1));
                    Assert.Throws<ObjectDisposedException>(() => stereo.ROI2);
                    Assert.Throws<ObjectDisposedException>(() => stereo.ROI2 = new Rect(0, 0, 1, 1));
                }
            }
        }

        [Fact]
        public void StereoBMComputesDisparityForSyntheticImages()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat left = new Mat(64, 64, MatType.CV_8UC1))
            using (Mat right = new Mat(64, 64, MatType.CV_8UC1))
            using (Mat disparity = new Mat())
            using (StereoBM stereo = StereoBM.Create(16, 9))
            {
                left.SetTo(new Scalar(0));
                right.SetTo(new Scalar(0));
                JYPPX.OpenCvSharp.ImgProc.Cv2.Rectangle(left, new Rect(20, 20, 20, 20), new Scalar(255), -1);
                JYPPX.OpenCvSharp.ImgProc.Cv2.Rectangle(right, new Rect(16, 20, 20, 20), new Scalar(255), -1);

                stereo.MinDisparity = 0;
                stereo.NumDisparities = 16;
                stereo.BlockSize = 9;
                stereo.PreFilterType = StereoBMPreFilterType.XSobel;
                stereo.ROI1 = new Rect(0, 0, 64, 64);
                stereo.ROI2 = new Rect(0, 0, 64, 64);
                stereo.Compute(left, right, disparity);

                Assert.Equal(64, disparity.Rows);
                Assert.Equal(64, disparity.Cols);
                Assert.Equal(MatType.CV_16SC1, disparity.Type);
                Assert.Equal(StereoBMPreFilterType.XSobel, stereo.PreFilterType);
                Assert.Equal(new Rect(0, 0, 64, 64).ToString(), stereo.ROI1.ToString());
            }
        }

        private static StereoBM? TryCreateStereoBM()
        {
            try
            {
                return StereoBM.Create(16, 9);
            }
            catch (OpenCvException ex) when (IsStereoBmBoundary(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static bool IsStereoBmBoundary(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("StereoBM", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("calib3d", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        [Fact]
        public void FindHomographyReturnsThreeByThreeMatrix()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = Calib3DCv2.ToPointMat(new[]
            {
                new Point2f(0.0F, 0.0F),
                new Point2f(1.0F, 0.0F),
                new Point2f(1.0F, 1.0F),
                new Point2f(0.0F, 1.0F)
            }))
            using (Mat dst = Calib3DCv2.ToPointMat(new[]
            {
                new Point2f(10.0F, 20.0F),
                new Point2f(11.0F, 20.0F),
                new Point2f(11.0F, 21.0F),
                new Point2f(10.0F, 21.0F)
            }))
            using (Mat homography = Calib3DCv2.FindHomography(src, dst))
            {
                Assert.Equal(3, homography.Rows);
                Assert.Equal(3, homography.Cols);
            }
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }
    }
}
