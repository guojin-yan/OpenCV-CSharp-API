using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class SolveP3PTests
    {
        private const double Fx = 820.0;
        private const double Fy = 790.0;
        private const double Cx = 320.0;
        private const double Cy = 240.0;
        private const double ExpectedRx = 0.18;
        private const double ExpectedRy = -0.12;
        private const double ExpectedRz = 0.09;
        private const double ExpectedTx = 0.15;
        private const double ExpectedTy = -0.08;
        private const double ExpectedTz = 4.6;

        [Fact]
        public void SolveP3PValidatesManagedInputs()
        {
            Point3f[] objectPoints = CreateObjectPoints();
            Point2f[] imagePoints = CreateImagePoints(objectPoints);

            using (Mat objectPointMat = Calib3DCv2.ToPointMat(objectPoints))
            using (Mat imagePointMat = Calib3DCv2.ToPointMat(imagePoints))
            using (Mat cameraMatrix = CreateCameraMatrix())
            using (var distCoeffs = new Mat())
            using (var rvecs = new Mat())
            using (var tvecs = new Mat())
            using (var invalidObjectLayout = new Mat(4, 1, MatType.CV_32FC2))
            using (var invalidImageLayout = new Mat(4, 1, MatType.CV_32FC3))
            using (var invalidObjectDepth = new Mat(4, 1, MatType.CV_32SC3))
            using (var invalidCameraShape = new Mat(2, 3, MatType.CV_64FC1))
            using (var invalidCameraDepth = new Mat(3, 3, MatType.CV_32SC1))
            using (var invalidDistortionShape = new Mat(2, 2, MatType.CV_64FC1))
            using (var invalidDistortionCount = new Mat(1, 6, MatType.CV_64FC1))
            using (var invalidDistortionDepth = new Mat(1, 5, MatType.CV_32SC1))
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolveP3P(
                        (Mat)null!,
                        imagePointMat,
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPointMat,
                        imagePointMat,
                        cameraMatrix,
                        distCoeffs,
                        null!,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        invalidObjectLayout,
                        imagePointMat,
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPointMat,
                        invalidImageLayout,
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        invalidObjectDepth,
                        imagePointMat,
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPointMat,
                        imagePointMat,
                        invalidCameraShape,
                        distCoeffs,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPointMat,
                        imagePointMat,
                        invalidCameraDepth,
                        distCoeffs,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPointMat,
                        imagePointMat,
                        cameraMatrix,
                        invalidDistortionShape,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPointMat,
                        imagePointMat,
                        cameraMatrix,
                        invalidDistortionCount,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPointMat,
                        imagePointMat,
                        cameraMatrix,
                        invalidDistortionDepth,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPointMat,
                        imagePointMat,
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs,
                        SolvePnPFlags.Iterative));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPointMat,
                        imagePointMat,
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        rvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPointMat,
                        imagePointMat,
                        cameraMatrix,
                        distCoeffs,
                        objectPointMat,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPoints,
                        imagePoints[..3],
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        objectPoints[..2],
                        imagePoints[..2],
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        new Point3f[5],
                        new Point2f[5],
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs));

#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.SolveP3P(
                        ReadOnlySpan<Point3f>.Empty,
                        ReadOnlySpan<Point2f>.Empty,
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs));
#endif
            }

            var disposed = Calib3DCv2.ToPointMat(objectPoints);
            disposed.Dispose();
            using (Mat imagePointMat = Calib3DCv2.ToPointMat(imagePoints))
            using (Mat cameraMatrix = CreateCameraMatrix())
            using (var distCoeffs = new Mat())
            using (var rvecs = new Mat())
            using (var tvecs = new Mat())
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    Calib3DCv2.SolveP3P(
                        disposed,
                        imagePointMat,
                        cameraMatrix,
                        distCoeffs,
                        rvecs,
                        tvecs));
            }
        }

        [Fact]
        public void SolveP3PPacksSortedP3PSolutionsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            AssertPackedSortedSolutions(SolvePnPFlags.P3P);
        }

        [Fact]
        public void SolveP3PPacksSortedAP3PSolutionsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            AssertPackedSortedSolutions(SolvePnPFlags.AP3P);
        }

        [Fact]
        public void SolveP3PAcceptsThreeAndFourPointInputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] objectPoints = CreateObjectPoints();
            Point2f[] imagePoints = CreateImagePoints(objectPoints);
            using (Mat cameraMatrix = CreateCameraMatrix())
            using (var distCoeffs = new Mat())
            using (var threeRvecs = new Mat())
            using (var threeTvecs = new Mat())
            using (var fourRvecs = new Mat())
            using (var fourTvecs = new Mat())
            {
                int threeCount = Calib3DCv2.SolveP3P(
                    objectPoints[..3],
                    imagePoints[..3],
                    cameraMatrix,
                    distCoeffs,
                    threeRvecs,
                    threeTvecs,
                    SolvePnPFlags.P3P);
                int fourCount = Calib3DCv2.SolveP3P(
                    objectPoints,
                    imagePoints,
                    cameraMatrix,
                    distCoeffs,
                    fourRvecs,
                    fourTvecs,
                    SolvePnPFlags.AP3P);

                Assert.InRange(threeCount, 1, 4);
                Assert.InRange(fourCount, 1, 4);
                AssertPoseOutput(threeCount, threeRvecs, threeTvecs);
                AssertPoseOutput(fourCount, fourRvecs, fourTvecs);
            }
        }

        [Fact]
        public void SolveP3PMatArrayAndSpanOverloadsAgreeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] objectPoints = CreateObjectPoints();
            Point2f[] imagePoints = CreateImagePoints(objectPoints);
            using (Mat objectPointMat = Calib3DCv2.ToPointMat(objectPoints))
            using (Mat imagePointMat = Calib3DCv2.ToPointMat(imagePoints))
            using (Mat cameraMatrix = CreateCameraMatrix())
            using (var distCoeffs = new Mat())
            {
                SolvePnPGenericResult matResult = Calib3DCv2.SolveP3P(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs,
                    SolvePnPFlags.AP3P);
                SolvePnPGenericResult arrayResult = Calib3DCv2.SolveP3P(
                    objectPoints,
                    imagePoints,
                    cameraMatrix,
                    distCoeffs,
                    SolvePnPFlags.AP3P);
                try
                {
                    Assert.Equal(matResult.SolutionCount, arrayResult.SolutionCount);
                    AssertMatricesNear(matResult.Rvecs, arrayResult.Rvecs, 1.0e-12);
                    AssertMatricesNear(matResult.Tvecs, arrayResult.Tvecs, 1.0e-12);
                    Assert.Null(matResult.ReprojectionError);
                    Assert.Null(arrayResult.ReprojectionError);

#if NETCOREAPP3_1_OR_GREATER
                    SolvePnPGenericResult spanResult = Calib3DCv2.SolveP3P(
                        objectPoints.AsSpan(),
                        imagePoints.AsSpan(),
                        cameraMatrix,
                        distCoeffs,
                        SolvePnPFlags.AP3P);
                    try
                    {
                        Assert.Equal(matResult.SolutionCount, spanResult.SolutionCount);
                        AssertMatricesNear(matResult.Rvecs, spanResult.Rvecs, 1.0e-12);
                        AssertMatricesNear(matResult.Tvecs, spanResult.Tvecs, 1.0e-12);
                        Assert.Null(spanResult.ReprojectionError);
                    }
                    finally
                    {
                        spanResult.Rvecs.Dispose();
                        spanResult.Tvecs.Dispose();
                    }
#endif
                }
                finally
                {
                    matResult.Rvecs.Dispose();
                    matResult.Tvecs.Dispose();
                    arrayResult.Rvecs.Dispose();
                    arrayResult.Tvecs.Dispose();
                }
            }
        }

        [Fact]
        public void SolveP3POwnedResultPreservesInputsAndRequiresOutputDisposalWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] objectPoints = CreateObjectPoints();
            Point2f[] imagePoints = CreateImagePoints(objectPoints);
            using (Mat objectPointMat = Calib3DCv2.ToPointMat(objectPoints))
            using (Mat imagePointMat = Calib3DCv2.ToPointMat(imagePoints))
            using (Mat cameraMatrix = CreateCameraMatrix())
            using (var distCoeffs = new Mat())
            {
                SolvePnPGenericResult result = Calib3DCv2.SolveP3P(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs);
                Assert.InRange(result.SolutionCount, 1, 4);
                AssertPoseOutput(result.SolutionCount, result.Rvecs, result.Tvecs);
                Assert.Null(result.ReprojectionError);

                for (int i = 0; i < objectPoints.Length; i++)
                {
                    Assert.Equal(objectPoints[i], objectPointMat.GetValue<Point3f>(i));
                    Assert.Equal(imagePoints[i], imagePointMat.GetValue<Point2f>(i));
                }
                Assert.Equal(Fx, cameraMatrix.GetValue<double>(0));
                Assert.Equal(Fy, cameraMatrix.GetValue<double>(4));
                Assert.True(distCoeffs.Empty);

                result.Rvecs.Dispose();
                result.Tvecs.Dispose();
                Assert.Throws<ObjectDisposedException>(() => result.Rvecs.GetValue<double>(0));
                Assert.Throws<ObjectDisposedException>(() => result.Tvecs.GetValue<double>(0));
            }
        }

        private static void AssertPackedSortedSolutions(SolvePnPFlags flags)
        {
            Point3f[] objectPoints = CreateObjectPoints();
            Point2f[] imagePoints = CreateImagePoints(objectPoints);
            using (Mat objectPointMat = Calib3DCv2.ToPointMat(objectPoints))
            using (Mat imagePointMat = Calib3DCv2.ToPointMat(imagePoints))
            using (Mat cameraMatrix = CreateCameraMatrix())
            using (var distCoeffs = new Mat())
            using (var rvecs = new Mat())
            using (var tvecs = new Mat())
            {
                int solutionCount = Calib3DCv2.SolveP3P(
                    objectPointMat,
                    imagePointMat,
                    cameraMatrix,
                    distCoeffs,
                    rvecs,
                    tvecs,
                    flags);

                Assert.InRange(solutionCount, 1, 4);
                AssertPoseOutput(solutionCount, rvecs, tvecs);
                double[] errors = GetReprojectionErrors(
                    objectPoints,
                    imagePoints,
                    rvecs,
                    tvecs);
                Assert.InRange(errors[0], 0.0, 1.0e-2);
                for (int i = 1; i < errors.Length; i++)
                {
                    Assert.True(
                        errors[i] + 1.0e-9 >= errors[i - 1],
                        $"Solution errors are not sorted at {i - 1} and {i}: {errors[i - 1]} > {errors[i]}.");
                }
            }
        }

        private static Point3f[] CreateObjectPoints()
        {
            return new[]
            {
                new Point3f(-0.6F, -0.5F, 0.2F),
                new Point3f(0.7F, -0.4F, 0.0F),
                new Point3f(0.5F, 0.8F, 0.3F),
                new Point3f(-0.8F, 0.6F, 0.9F)
            };
        }

        private static Point2f[] CreateImagePoints(Point3f[] objectPoints)
        {
            return Project(
                objectPoints,
                ExpectedRx,
                ExpectedRy,
                ExpectedRz,
                ExpectedTx,
                ExpectedTy,
                ExpectedTz);
        }

        private static Mat CreateCameraMatrix()
        {
            Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1);
            cameraMatrix.SetValue(0, Fx);
            cameraMatrix.SetValue(2, Cx);
            cameraMatrix.SetValue(4, Fy);
            cameraMatrix.SetValue(5, Cy);
            return cameraMatrix;
        }

        private static void AssertPoseOutput(int solutionCount, Mat rvecs, Mat tvecs)
        {
            Assert.Equal(solutionCount, rvecs.Rows);
            Assert.Equal(3, rvecs.Cols);
            Assert.Equal(MatType.CV_64FC1, rvecs.Type);
            Assert.Equal(solutionCount, tvecs.Rows);
            Assert.Equal(3, tvecs.Cols);
            Assert.Equal(MatType.CV_64FC1, tvecs.Type);
            for (int i = 0; i < solutionCount * 3; i++)
            {
                Assert.True(double.IsFinite(rvecs.GetValue<double>(i)));
                Assert.True(double.IsFinite(tvecs.GetValue<double>(i)));
            }
        }

        private static double[] GetReprojectionErrors(
            Point3f[] objectPoints,
            Point2f[] imagePoints,
            Mat rvecs,
            Mat tvecs)
        {
            var errors = new double[rvecs.Rows];
            for (int solution = 0; solution < errors.Length; solution++)
            {
                int offset = solution * 3;
                Point2f[] projected = Project(
                    objectPoints,
                    rvecs.GetValue<double>(offset),
                    rvecs.GetValue<double>(offset + 1),
                    rvecs.GetValue<double>(offset + 2),
                    tvecs.GetValue<double>(offset),
                    tvecs.GetValue<double>(offset + 1),
                    tvecs.GetValue<double>(offset + 2));
                double squaredError = 0.0;
                for (int point = 0; point < projected.Length; point++)
                {
                    double dx = projected[point].X - imagePoints[point].X;
                    double dy = projected[point].Y - imagePoints[point].Y;
                    squaredError += (dx * dx) + (dy * dy);
                }
                errors[solution] = Math.Sqrt(squaredError / projected.Length);
            }

            return errors;
        }

        private static Point2f[] Project(
            Point3f[] objectPoints,
            double rx,
            double ry,
            double rz,
            double tx,
            double ty,
            double tz)
        {
            double[,] rotation = Rodrigues(rx, ry, rz);
            var projected = new Point2f[objectPoints.Length];
            for (int i = 0; i < objectPoints.Length; i++)
            {
                Point3f point = objectPoints[i];
                double x =
                    (rotation[0, 0] * point.X) +
                    (rotation[0, 1] * point.Y) +
                    (rotation[0, 2] * point.Z) +
                    tx;
                double y =
                    (rotation[1, 0] * point.X) +
                    (rotation[1, 1] * point.Y) +
                    (rotation[1, 2] * point.Z) +
                    ty;
                double z =
                    (rotation[2, 0] * point.X) +
                    (rotation[2, 1] * point.Y) +
                    (rotation[2, 2] * point.Z) +
                    tz;
                projected[i] = new Point2f(
                    (float)((Fx * x / z) + Cx),
                    (float)((Fy * y / z) + Cy));
            }

            return projected;
        }

        private static double[,] Rodrigues(double rx, double ry, double rz)
        {
            double theta = Math.Sqrt((rx * rx) + (ry * ry) + (rz * rz));
            if (theta < 1.0e-15)
            {
                return new[,]
                {
                    { 1.0, 0.0, 0.0 },
                    { 0.0, 1.0, 0.0 },
                    { 0.0, 0.0, 1.0 }
                };
            }

            double x = rx / theta;
            double y = ry / theta;
            double z = rz / theta;
            double cosine = Math.Cos(theta);
            double sine = Math.Sin(theta);
            double oneMinusCosine = 1.0 - cosine;
            return new[,]
            {
                {
                    cosine + (x * x * oneMinusCosine),
                    (x * y * oneMinusCosine) - (z * sine),
                    (x * z * oneMinusCosine) + (y * sine)
                },
                {
                    (y * x * oneMinusCosine) + (z * sine),
                    cosine + (y * y * oneMinusCosine),
                    (y * z * oneMinusCosine) - (x * sine)
                },
                {
                    (z * x * oneMinusCosine) - (y * sine),
                    (z * y * oneMinusCosine) + (x * sine),
                    cosine + (z * z * oneMinusCosine)
                }
            };
        }

        private static void AssertMatricesNear(Mat expected, Mat actual, double tolerance)
        {
            Assert.Equal(expected.Rows, actual.Rows);
            Assert.Equal(expected.Cols, actual.Cols);
            Assert.Equal(expected.Type, actual.Type);
            for (int i = 0; i < expected.Rows * expected.Cols; i++)
            {
                Assert.InRange(
                    Math.Abs(expected.GetValue<double>(i) - actual.GetValue<double>(i)),
                    0.0,
                    tolerance);
            }
        }
    }
}
