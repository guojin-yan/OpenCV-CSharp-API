using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    public sealed class CameraRegistrationTests
    {
        [Fact]
        public void CameraModelValuesAndResultObjectsExposeExpectedShapes()
        {
            Assert.Equal(0, (int)CameraModel.Pinhole);
            Assert.Equal(1, (int)CameraModel.Fisheye);

            using (var r = new Mat(3, 3, MatType.CV_64FC1))
            using (var t = new Mat(3, 1, MatType.CV_64FC1))
            using (var e = new Mat(3, 3, MatType.CV_64FC1))
            using (var f = new Mat(3, 3, MatType.CV_64FC1))
            using (var perViewErrors = new Mat(4, 2, MatType.CV_64FC1))
            using (var rvecs = new Mat(4, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(4, 3, MatType.CV_64FC1))
            {
                var registration = new CameraRegistrationResult(0.25, r, t, e, f, perViewErrors);
                var extended = new CameraRegistrationExtendedResult(registration, rvecs, tvecs);

                Assert.Equal(0.25, registration.ReprojectionError);
                Assert.Same(r, registration.R);
                Assert.Same(t, registration.T);
                Assert.Same(e, registration.E);
                Assert.Same(f, registration.F);
                Assert.Same(perViewErrors, registration.PerViewErrors);
                Assert.Equal(4, registration.ViewCount);
                Assert.Same(rvecs, extended.Rvecs);
                Assert.Same(tvecs, extended.Tvecs);
                Assert.Equal(4, extended.ViewCount);
                Assert.Contains("PerViewErrors=4x2", registration.ToString(), StringComparison.Ordinal);
                Assert.Contains("Rvecs=4x3", extended.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ResultConstructorsRejectNullAndInvalidOutputShapes()
        {
            using (var r = new Mat(3, 3, MatType.CV_64FC1))
            using (var t = new Mat(3, 1, MatType.CV_64FC1))
            using (var e = new Mat(3, 3, MatType.CV_64FC1))
            using (var f = new Mat(3, 3, MatType.CV_64FC1))
            using (var perViewErrors = new Mat(4, 2, MatType.CV_64FC1))
            using (var invalidR = new Mat(2, 3, MatType.CV_64FC1))
            using (var invalidErrors = new Mat(4, 1, MatType.CV_64FC1))
            using (var rvecs = new Mat(4, 3, MatType.CV_64FC1))
            using (var tvecs = new Mat(4, 3, MatType.CV_64FC1))
            using (var invalidTvecs = new Mat(3, 3, MatType.CV_64FC1))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    new CameraRegistrationResult(0.25, null!, t, e, f, perViewErrors));

                ArgumentException rotationException = Assert.Throws<ArgumentException>(() =>
                    new CameraRegistrationResult(0.25, invalidR, t, e, f, perViewErrors));
                Assert.Equal("r", rotationException.ParamName);

                ArgumentException errorException = Assert.Throws<ArgumentException>(() =>
                    new CameraRegistrationResult(0.25, r, t, e, f, invalidErrors));
                Assert.Equal("perViewErrors", errorException.ParamName);

                var registration = new CameraRegistrationResult(0.25, r, t, e, f, perViewErrors);
                Assert.Throws<ArgumentNullException>(() =>
                    new CameraRegistrationExtendedResult(registration, null!, tvecs));

                ArgumentException poseException = Assert.Throws<ArgumentException>(() =>
                    new CameraRegistrationExtendedResult(registration, rvecs, invalidTvecs));
                Assert.Equal("tvecs", poseException.ParamName);
            }
        }

        [Fact]
        public void RegisterCamerasValidatesPointGroupsBeforeNativeCall()
        {
            using (var fixture = new RegistrationFixture())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    fixture.Register(null!, fixture.ObjectPoints2, fixture.ImagePoints1, fixture.ImagePoints2));
                Assert.Throws<ArgumentException>(() =>
                    fixture.Register(
                        Array.Empty<Point3f[]>(),
                        Array.Empty<Point3f[]>(),
                        Array.Empty<Point2f[]>(),
                        Array.Empty<Point2f[]>()));

                Point3f[][] fewerFrames = CopyOuter(fixture.ObjectPoints2, fixture.ObjectPoints2.Length - 1);
                Point2f[][] fewerImageFrames = CopyOuter(fixture.ImagePoints2, fixture.ImagePoints2.Length - 1);
                Assert.Throws<ArgumentException>(() =>
                    fixture.Register(fixture.ObjectPoints1, fewerFrames, fixture.ImagePoints1, fewerImageFrames));

                Point2f[][] mismatchedImagePoints1 = CopyOuter(fixture.ImagePoints1, fixture.ImagePoints1.Length);
                mismatchedImagePoints1[0] = CopyPrefix(fixture.ImagePoints1[0], fixture.ImagePoints1[0].Length - 1);
                Assert.Throws<ArgumentException>(() =>
                    fixture.Register(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        mismatchedImagePoints1,
                        fixture.ImagePoints2));
            }
        }

        [Fact]
        public void RegisterCamerasAllowsDifferentPointCountsBetweenCameras()
        {
            using (var fixture = new RegistrationFixture())
            {
                Point3f[][] shorterObjectPoints2 = new Point3f[fixture.ObjectPoints2.Length][];
                Point2f[][] shorterImagePoints2 = new Point2f[fixture.ImagePoints2.Length][];
                for (int view = 0; view < shorterObjectPoints2.Length; ++view)
                {
                    int pointCount = fixture.ObjectPoints2[view].Length - 6;
                    shorterObjectPoints2[view] = CopyPrefix(fixture.ObjectPoints2[view], pointCount);
                    shorterImagePoints2[view] = CopyPrefix(fixture.ImagePoints2[view], pointCount);
                }

                Exception? exception = Record.Exception(() =>
                    fixture.Register(
                        fixture.ObjectPoints1,
                        shorterObjectPoints2,
                        fixture.ImagePoints1,
                        shorterImagePoints2));

                Assert.False(exception is ArgumentException);
                if (TestEnvironment.IsNativeSmokeEnabled())
                {
                    Assert.Null(exception);
                }
            }
        }

        [Fact]
        public void RegisterCamerasValidatesMatricesModelsAndCriteriaBeforeNativeCall()
        {
            using (var fixture = new RegistrationFixture())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RegisterCameras(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        null!,
                        fixture.DistCoeffs1,
                        CameraModel.Pinhole,
                        fixture.CameraMatrix2,
                        fixture.DistCoeffs2,
                        CameraModel.Pinhole,
                        fixture.R,
                        fixture.T,
                        fixture.E,
                        fixture.F,
                        fixture.PerViewErrors));

                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RegisterCamerasExtended(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        fixture.CameraMatrix1,
                        fixture.DistCoeffs1,
                        CameraModel.Pinhole,
                        fixture.CameraMatrix2,
                        fixture.DistCoeffs2,
                        CameraModel.Pinhole,
                        fixture.R,
                        fixture.T,
                        fixture.E,
                        fixture.F,
                        null!,
                        fixture.Tvecs,
                        fixture.PerViewErrors));

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    fixture.Register(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        (CameraModel)2,
                        CameraModel.Pinhole));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    fixture.Register(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        CameraModel.Pinhole,
                        CameraModel.Pinhole,
                        new TermCriteria((TermCriteriaTypes)0, 10, 1e-6)));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    fixture.Register(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        CameraModel.Pinhole,
                        CameraModel.Pinhole,
                        new TermCriteria(TermCriteriaTypes.Count, 0, 0.0)));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    fixture.Register(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        CameraModel.Pinhole,
                        CameraModel.Pinhole,
                        new TermCriteria(TermCriteriaTypes.Eps, 0, double.NaN)));

                var disposed = new Mat();
                disposed.Dispose();
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.RegisterCameras(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        disposed,
                        fixture.DistCoeffs1,
                        CameraModel.Pinhole,
                        fixture.CameraMatrix2,
                        fixture.DistCoeffs2,
                        CameraModel.Pinhole,
                        fixture.R,
                        fixture.T,
                        fixture.E,
                        fixture.F,
                        fixture.PerViewErrors));

                var disposedDistCoeffs = new Mat();
                disposedDistCoeffs.Dispose();
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.RegisterCameras(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        fixture.CameraMatrix1,
                        disposedDistCoeffs,
                        CameraModel.Pinhole,
                        fixture.CameraMatrix2,
                        fixture.DistCoeffs2,
                        CameraModel.Pinhole,
                        fixture.R,
                        fixture.T,
                        fixture.E,
                        fixture.F,
                        fixture.PerViewErrors));

                var disposedOutput = new Mat();
                disposedOutput.Dispose();
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.RegisterCamerasExtended(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        fixture.CameraMatrix1,
                        fixture.DistCoeffs1,
                        CameraModel.Pinhole,
                        fixture.CameraMatrix2,
                        fixture.DistCoeffs2,
                        CameraModel.Pinhole,
                        fixture.R,
                        fixture.T,
                        fixture.E,
                        fixture.F,
                        disposedOutput,
                        fixture.Tvecs,
                        fixture.PerViewErrors));
            }
        }

        [Fact]
        public void OwnedOverloadsRejectExtrinsicGuess()
        {
            using (var fixture = new RegistrationFixture())
            {
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.RegisterCameras(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        fixture.CameraMatrix1,
                        fixture.DistCoeffs1,
                        CameraModel.Pinhole,
                        fixture.CameraMatrix2,
                        fixture.DistCoeffs2,
                        CameraModel.Pinhole,
                        CalibrationFlags.UseExtrinsicGuess));

                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.RegisterCamerasExtended(
                        fixture.ObjectPoints1,
                        fixture.ObjectPoints2,
                        fixture.ImagePoints1,
                        fixture.ImagePoints2,
                        fixture.CameraMatrix1,
                        fixture.DistCoeffs1,
                        CameraModel.Pinhole,
                        fixture.CameraMatrix2,
                        fixture.DistCoeffs2,
                        CameraModel.Pinhole,
                        CalibrationFlags.UseExtrinsicGuess));
            }
        }

        [Fact]
        public void RegisterCamerasRunsOnSyntheticDataWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var fixture = new RegistrationFixture())
            {
                CameraRegistrationResult result = Calib3DCv2.RegisterCameras(
                    fixture.ObjectPoints1,
                    fixture.ObjectPoints2,
                    fixture.ImagePoints1,
                    fixture.ImagePoints2,
                    fixture.CameraMatrix1,
                    fixture.DistCoeffs1,
                    CameraModel.Pinhole,
                    fixture.CameraMatrix2,
                    fixture.DistCoeffs2,
                    CameraModel.Pinhole);

                try
                {
                    Assert.True(double.IsFinite(result.ReprojectionError));
                    Assert.InRange(result.ReprojectionError, 0.0, 0.1);
                    AssertShape(result.R, 3, 3);
                    AssertShape(result.T, 3, 1);
                    AssertShape(result.E, 3, 3);
                    AssertShape(result.F, 3, 3);
                    AssertShape(result.PerViewErrors, fixture.ObjectPoints1.Length, 2);
                    AssertAllFinite(result.R);
                    AssertAllFinite(result.T);
                    AssertAllFinite(result.E);
                    AssertAllFinite(result.F);
                    AssertAllFinite(result.PerViewErrors);
                }
                finally
                {
                    Dispose(result);
                }
            }
        }

        [Fact]
        public void RegisterCamerasExtendedRunsOnSyntheticDataWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var fixture = new RegistrationFixture())
            {
                CameraRegistrationExtendedResult result = Calib3DCv2.RegisterCamerasExtended(
                    fixture.ObjectPoints1,
                    fixture.ObjectPoints2,
                    fixture.ImagePoints1,
                    fixture.ImagePoints2,
                    fixture.CameraMatrix1,
                    fixture.DistCoeffs1,
                    CameraModel.Pinhole,
                    fixture.CameraMatrix2,
                    fixture.DistCoeffs2,
                    CameraModel.Pinhole);

                try
                {
                    Assert.True(double.IsFinite(result.Registration.ReprojectionError));
                    Assert.InRange(result.Registration.ReprojectionError, 0.0, 0.1);
                    AssertShape(result.Registration.PerViewErrors, fixture.ObjectPoints1.Length, 2);
                    AssertShape(result.Rvecs, fixture.ObjectPoints1.Length, 3);
                    AssertShape(result.Tvecs, fixture.ObjectPoints1.Length, 3);
                    AssertAllFinite(result.Rvecs);
                    AssertAllFinite(result.Tvecs);
                    AssertAllFinite(result.Registration.PerViewErrors);
                }
                finally
                {
                    Dispose(result);
                }
            }
        }

        private static T[][] CopyOuter<T>(T[][] source, int count)
        {
            var result = new T[count][];
            Array.Copy(source, result, count);
            return result;
        }

        private static T[] CopyPrefix<T>(T[] source, int count)
        {
            var result = new T[count];
            Array.Copy(source, result, count);
            return result;
        }

        private static void AssertShape(Mat value, int rows, int cols)
        {
            Assert.Equal(rows, value.Rows);
            Assert.Equal(cols, value.Cols);
            Assert.Equal(1, value.Channels);
        }

        private static void AssertAllFinite(Mat value)
        {
            double[] values = value.ToArray<double>();
            Assert.NotEmpty(values);
            foreach (double item in values)
            {
                Assert.True(double.IsFinite(item));
            }
        }

        private static void Dispose(CameraRegistrationResult result)
        {
            result.R.Dispose();
            result.T.Dispose();
            result.E.Dispose();
            result.F.Dispose();
            result.PerViewErrors.Dispose();
        }

        private static void Dispose(CameraRegistrationExtendedResult result)
        {
            Dispose(result.Registration);
            result.Rvecs.Dispose();
            result.Tvecs.Dispose();
        }

        private sealed class RegistrationFixture : IDisposable
        {
            internal RegistrationFixture()
            {
                CalibrationTestData.CreateSyntheticCameraRegistrationData(
                    out Point3f[][] objectPoints1,
                    out Point3f[][] objectPoints2,
                    out Point2f[][] imagePoints1,
                    out Point2f[][] imagePoints2,
                    out Mat cameraMatrix1,
                    out Mat distCoeffs1,
                    out Mat cameraMatrix2,
                    out Mat distCoeffs2);

                ObjectPoints1 = objectPoints1;
                ObjectPoints2 = objectPoints2;
                ImagePoints1 = imagePoints1;
                ImagePoints2 = imagePoints2;
                CameraMatrix1 = cameraMatrix1;
                DistCoeffs1 = distCoeffs1;
                CameraMatrix2 = cameraMatrix2;
                DistCoeffs2 = distCoeffs2;
                R = new Mat();
                T = new Mat();
                E = new Mat();
                F = new Mat();
                Rvecs = new Mat();
                Tvecs = new Mat();
                PerViewErrors = new Mat();
            }

            internal Point3f[][] ObjectPoints1 { get; }

            internal Point3f[][] ObjectPoints2 { get; }

            internal Point2f[][] ImagePoints1 { get; }

            internal Point2f[][] ImagePoints2 { get; }

            internal Mat CameraMatrix1 { get; }

            internal Mat DistCoeffs1 { get; }

            internal Mat CameraMatrix2 { get; }

            internal Mat DistCoeffs2 { get; }

            internal Mat R { get; }

            internal Mat T { get; }

            internal Mat E { get; }

            internal Mat F { get; }

            internal Mat Rvecs { get; }

            internal Mat Tvecs { get; }

            internal Mat PerViewErrors { get; }

            internal double Register(
                Point3f[][] objectPoints1,
                Point3f[][] objectPoints2,
                Point2f[][] imagePoints1,
                Point2f[][] imagePoints2,
                CameraModel cameraModel1 = CameraModel.Pinhole,
                CameraModel cameraModel2 = CameraModel.Pinhole,
                TermCriteria? criteria = null)
            {
                return Calib3DCv2.RegisterCameras(
                    objectPoints1,
                    objectPoints2,
                    imagePoints1,
                    imagePoints2,
                    CameraMatrix1,
                    DistCoeffs1,
                    cameraModel1,
                    CameraMatrix2,
                    DistCoeffs2,
                    cameraModel2,
                    R,
                    T,
                    E,
                    F,
                    PerViewErrors,
                    CalibrationFlags.None,
                    criteria);
            }

            public void Dispose()
            {
                CameraMatrix1.Dispose();
                DistCoeffs1.Dispose();
                CameraMatrix2.Dispose();
                DistCoeffs2.Dispose();
                R.Dispose();
                T.Dispose();
                E.Dispose();
                F.Dispose();
                Rvecs.Dispose();
                Tvecs.Dispose();
                PerViewErrors.Dispose();
            }
        }
    }
}
