using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class ProjectPointsTests
    {
        [Fact]
        public void ProjectPointsValidatesInputsAndOutputAliasesBeforeNativeCall()
        {
            Mat[] inputs = CreateDoubleInputs();
            using var imagePoints = new Mat();
            using var jacobian = new Mat();
            try
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints((Mat)null!, inputs[1], inputs[2], inputs[3], inputs[4]));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints(inputs[0], null!, inputs[2], inputs[3], inputs[4]));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints(inputs[0], inputs[1], null!, inputs[3], inputs[4]));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints(inputs[0], inputs[1], inputs[2], null!, inputs[4]));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints(inputs[0], inputs[1], inputs[2], inputs[3], null!));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.ProjectPoints(inputs[0], inputs[1], inputs[2], inputs[3], inputs[4], null!));

                using (var invalidObjectShape = new Mat(2, 2, MatType.CV_64FC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallCallerOwned(invalidObjectShape, inputs[1], inputs[2], inputs[3], inputs[4], imagePoints));
                }
                using (var invalidObjectDepth = new Mat(1, 3, MatType.CV_32SC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallCallerOwned(invalidObjectDepth, inputs[1], inputs[2], inputs[3], inputs[4], imagePoints));
                }
                using (var invalidRotation = new Mat(2, 1, MatType.CV_64FC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallCallerOwned(inputs[0], invalidRotation, inputs[2], inputs[3], inputs[4], imagePoints));
                }
                using (var invalidTranslation = new Mat(3, 3, MatType.CV_64FC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallCallerOwned(inputs[0], inputs[1], invalidTranslation, inputs[3], inputs[4], imagePoints));
                }
                using (var invalidCamera = new Mat(2, 3, MatType.CV_64FC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallCallerOwned(inputs[0], inputs[1], inputs[2], invalidCamera, inputs[4], imagePoints));
                }
                using (var invalidDistortion = new Mat(1, 6, MatType.CV_64FC1))
                {
                    Assert.Throws<ArgumentException>(() =>
                        CallCallerOwned(inputs[0], inputs[1], inputs[2], inputs[3], invalidDistortion, imagePoints));
                }

                Assert.Throws<ArgumentOutOfRangeException>(() =>
                    Calib3DCv2.ProjectPoints(
                        inputs[0],
                        inputs[1],
                        inputs[2],
                        inputs[3],
                        inputs[4],
                        imagePoints,
                        aspectRatio: double.NaN));
                Assert.Throws<ArgumentException>(() =>
                    CallCallerOwned(inputs[0], inputs[1], inputs[2], inputs[3], inputs[4], inputs[0]));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.ProjectPoints(inputs[0], inputs[1], inputs[2], inputs[3], inputs[4], imagePoints, inputs[3]));
                Assert.Throws<ArgumentException>(() =>
                    Calib3DCv2.ProjectPoints(inputs[0], inputs[1], inputs[2], inputs[3], inputs[4], imagePoints, imagePoints));
            }
            finally
            {
                DisposeAll(inputs);
            }

            Mat[] disposedInputs = CreateDoubleInputs();
            using var disposedOutput = new Mat();
            disposedInputs[1].Dispose();
            try
            {
                Assert.Throws<ObjectDisposedException>(() =>
                    CallCallerOwned(
                        disposedInputs[0],
                        disposedInputs[1],
                        disposedInputs[2],
                        disposedInputs[3],
                        disposedInputs[4],
                        disposedOutput));
            }
            finally
            {
                DisposeAll(disposedInputs);
            }
        }

        [Fact]
        public void OwnedAndCallerOwnedProjectPointsAgreeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Mat[] inputs = CreateDoubleInputs();
            double[] objectSnapshot = inputs[0].ToArray<double>();
            using var callerOwned = new Mat();
            using var jacobian = new Mat();
            try
            {
                Calib3DCv2.ProjectPoints(
                    inputs[0],
                    inputs[1],
                    inputs[2],
                    inputs[3],
                    inputs[4],
                    callerOwned,
                    jacobian);
                using Mat owned = Calib3DCv2.ProjectPoints(inputs[0], inputs[1], inputs[2], inputs[3], inputs[4]);

                AssertImagePoints(callerOwned, 3, MatType.CV_64FC2);
                AssertImagePoints(owned, 3, MatType.CV_64FC2);
                Assert.Equal(6, jacobian.Rows);
                Assert.Equal(15, jacobian.Cols);
                Assert.Equal(MatType.CV_64FC1, jacobian.Type);
                AssertPointArraysNear(callerOwned.ToArray<Point2d>(), owned.ToArray<Point2d>(), 1.0e-12);
                AssertPointArraysNear(
                    new[]
                    {
                        new Point2d(1.0, 2.0),
                        new Point2d(1.0, 2.0),
                        new Point2d(-0.5, 0.25)
                    },
                    owned.ToArray<Point2d>(),
                    1.0e-12);
                AssertMatricesNear(objectSnapshot, inputs[0].ToArray<double>(), 0.0);
                Assert.True(inputs[4].Empty);
            }
            finally
            {
                DisposeAll(inputs);
            }
        }

        [Fact]
        public void ScalarObjectPointLayoutsKeepFloatProjectionShapeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat rowObjectPoints = CreateMat(
                2,
                3,
                MatType.CV_32FC1,
                1.0F, 2.0F, 1.0F,
                2.0F, 4.0F, 2.0F);
            using Mat columnObjectPoints = CreateMat(
                3,
                2,
                MatType.CV_32FC1,
                1.0F, 2.0F,
                2.0F, 4.0F,
                1.0F, 2.0F);
            using Mat rvec = CreateMat(1, 3, MatType.CV_32FC1, 0.0F, 0.0F, 0.0F);
            using Mat tvec = CreateMat(3, 1, MatType.CV_32FC1, 0.0F, 0.0F, 0.0F);
            using Mat camera = CreateMat(
                3,
                3,
                MatType.CV_32FC1,
                1.0F, 0.0F, 0.0F,
                0.0F, 1.0F, 0.0F,
                0.0F, 0.0F, 1.0F);
            using var zeroDistortion = new Mat();

            using Mat rowResult = Calib3DCv2.ProjectPoints(rowObjectPoints, rvec, tvec, camera, zeroDistortion);
            using Mat columnResult = Calib3DCv2.ProjectPoints(columnObjectPoints, rvec, tvec, camera, zeroDistortion);

            AssertImagePoints(rowResult, 2, MatType.CV_32FC2);
            AssertImagePoints(columnResult, 2, MatType.CV_32FC2);
            AssertPointArraysNear(rowResult.ToArray<Point2f>(), columnResult.ToArray<Point2f>(), 1.0e-6F);
            AssertPointArraysNear(
                new[]
                {
                    new Point2f(1.0F, 2.0F),
                    new Point2f(1.0F, 2.0F)
                },
                rowResult.ToArray<Point2f>(),
                1.0e-6F);
        }

        private static Mat[] CreateDoubleInputs()
        {
            return new[]
            {
                CreateMat(
                    3,
                    3,
                    MatType.CV_64FC1,
                    1.0, 2.0, 1.0,
                    2.0, 4.0, 2.0,
                    -1.0, 0.5, 2.0),
                CreateMat(3, 1, MatType.CV_64FC1, 0.0, 0.0, 0.0),
                CreateMat(3, 1, MatType.CV_64FC1, 0.0, 0.0, 0.0),
                CreateMat(
                    3,
                    3,
                    MatType.CV_64FC1,
                    1.0, 0.0, 0.0,
                    0.0, 1.0, 0.0,
                    0.0, 0.0, 1.0),
                new Mat()
            };
        }

        private static Mat CreateMat(
            int rows,
            int columns,
            int type,
            params double[] values)
        {
            Assert.Equal(rows * columns, values.Length);
            var result = new Mat(rows, columns, type);
            result.CopyFrom<double>(values);
            return result;
        }

        private static Mat CreateMat(
            int rows,
            int columns,
            int type,
            params float[] values)
        {
            Assert.Equal(rows * columns, values.Length);
            var result = new Mat(rows, columns, type);
            result.CopyFrom<float>(values);
            return result;
        }

        private static void CallCallerOwned(
            Mat objectPoints,
            Mat rvec,
            Mat tvec,
            Mat cameraMatrix,
            Mat distCoeffs,
            Mat imagePoints)
        {
            Calib3DCv2.ProjectPoints(
                objectPoints,
                rvec,
                tvec,
                cameraMatrix,
                distCoeffs,
                imagePoints);
        }

        private static void AssertImagePoints(Mat matrix, int pointCount, int type)
        {
            Assert.Equal(pointCount, matrix.Rows);
            Assert.Equal(1, matrix.Cols);
            Assert.Equal(type, matrix.Type);
        }

        private static void AssertPointArraysNear(
            Point2f[] expected,
            Point2f[] actual,
            float tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.InRange(Math.Abs(expected[i].X - actual[i].X), 0.0F, tolerance);
                Assert.InRange(Math.Abs(expected[i].Y - actual[i].Y), 0.0F, tolerance);
            }
        }

        private static void AssertPointArraysNear(
            Point2d[] expected,
            Point2d[] actual,
            double tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.InRange(Math.Abs(expected[i].X - actual[i].X), 0.0, tolerance);
                Assert.InRange(Math.Abs(expected[i].Y - actual[i].Y), 0.0, tolerance);
            }
        }

        private static void AssertMatricesNear(
            double[] expected,
            double[] actual,
            double tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.InRange(Math.Abs(expected[i] - actual[i]), 0.0, tolerance);
            }
        }

        private static void DisposeAll(Mat[] matrices)
        {
            foreach (Mat matrix in matrices)
            {
                matrix.Dispose();
            }
        }
    }
}
