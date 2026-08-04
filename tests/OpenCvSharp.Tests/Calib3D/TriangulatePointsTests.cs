using System;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class TriangulatePointsTests
    {
        [Fact]
        public void TriangulatePointsValidatesInputsBeforeNativeCall()
        {
            using Mat projection1 = CreateProjectionMatrix(0.0F);
            using Mat projection2 = CreateProjectionMatrix(1.0F);
            using Mat points1 = CreatePointCoordinateMatrix(
                new Point2f(0.25F, 0.125F),
                new Point2f(0.40F, -0.20F));
            using Mat points2 = CreatePointCoordinateMatrix(
                new Point2f(0.00F, 0.125F),
                new Point2f(0.20F, -0.20F));
            using var output = new Mat();
            using var invalidProjectionShape = new Mat(3, 3, MatType.CV_32FC1);
            using var invalidProjectionChannels = new Mat(3, 4, MatType.CV_32FC2);
            using var invalidProjectionDepth = new Mat(3, 4, MatType.CV_32SC1);
            using var invalidPointShape = new Mat(3, 2, MatType.CV_32FC1);
            using var invalidPointDepth = new Mat(2, 2, MatType.CV_32SC1);
            using Mat mismatchedPoints = CreatePointCoordinateMatrix(new Point2f(0.00F, 0.125F));

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.TriangulatePoints(
                    invalidProjectionShape,
                    projection2,
                    points1,
                    points2,
                    output));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.TriangulatePoints(
                    invalidProjectionChannels,
                    projection2,
                    points1,
                    points2,
                    output));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.TriangulatePoints(
                    invalidProjectionDepth,
                    projection2,
                    points1,
                    points2,
                    output));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.TriangulatePoints(
                    projection1,
                    projection2,
                    invalidPointShape,
                    points2,
                    output));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.TriangulatePoints(
                    projection1,
                    projection2,
                    invalidPointDepth,
                    points2,
                    output));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.TriangulatePoints(
                    projection1,
                    projection2,
                    points1,
                    mismatchedPoints,
                    output));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.TriangulatePoints(
                    projection1,
                    projection2,
                    points1,
                    points2,
                    points1));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.TriangulatePoints(
                    null!,
                    projection2,
                    points1,
                    points2));
        }

        [Fact]
        public void TriangulatePointsOwnedAndCallerOwnedOutputsAgreeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] world =
            {
                new Point3f(1.0F, 0.5F, 4.0F),
                new Point3f(2.0F, -1.0F, 5.0F),
                new Point3f(-0.5F, 0.75F, 3.0F)
            };
            using Mat projection1 = CreateProjectionMatrix(0.0F);
            using Mat projection2 = CreateProjectionMatrix(1.0F);
            using Mat points1 = ProjectPoints(world, cameraOffsetX: 0.0F);
            using Mat points2 = ProjectPoints(world, cameraOffsetX: 1.0F);
            using var callerOwned = new Mat();

            Calib3DCv2.TriangulatePoints(
                projection1,
                projection2,
                points1,
                points2,
                callerOwned);
            using Mat owned = Calib3DCv2.TriangulatePoints(
                projection1,
                projection2,
                points1,
                points2);

            AssertMatrixShape(callerOwned, 4, world.Length, MatType.CV_32FC1);
            AssertMatrixShape(owned, 4, world.Length, MatType.CV_32FC1);
            AssertArrayNear(callerOwned.ToArray<float>(), owned.ToArray<float>(), 1.0e-4F);
            AssertWorldPointsNear(world, owned, 1.0e-3F);
        }

        private static Mat CreateProjectionMatrix(float cameraOffsetX)
        {
            var result = new Mat(3, 4, MatType.CV_32FC1);
            result.CopyFrom(new[]
            {
                1.0F, 0.0F, 0.0F, -cameraOffsetX,
                0.0F, 1.0F, 0.0F, 0.0F,
                0.0F, 0.0F, 1.0F, 0.0F
            });
            return result;
        }

        private static Mat ProjectPoints(Point3f[] world, float cameraOffsetX)
        {
            var points = new Point2f[world.Length];
            for (int i = 0; i < world.Length; ++i)
            {
                points[i] = new Point2f(
                    (world[i].X - cameraOffsetX) / world[i].Z,
                    world[i].Y / world[i].Z);
            }

            return CreatePointCoordinateMatrix(points);
        }

        private static Mat CreatePointCoordinateMatrix(params Point2f[] points)
        {
            var result = new Mat(2, points.Length, MatType.CV_32FC1);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i].X);
                result.SetValue(points.Length + i, points[i].Y);
            }

            return result;
        }

        private static void AssertWorldPointsNear(Point3f[] expected, Mat homogeneousPoints, float tolerance)
        {
            for (int i = 0; i < expected.Length; ++i)
            {
                float x = homogeneousPoints.GetValue<float>(i);
                float y = homogeneousPoints.GetValue<float>(homogeneousPoints.Cols + i);
                float z = homogeneousPoints.GetValue<float>((2 * homogeneousPoints.Cols) + i);
                float w = homogeneousPoints.GetValue<float>((3 * homogeneousPoints.Cols) + i);
                AssertNear(expected[i].X, x / w, tolerance);
                AssertNear(expected[i].Y, y / w, tolerance);
                AssertNear(expected[i].Z, z / w, tolerance);
            }
        }

        private static void AssertMatrixShape(Mat value, int rows, int cols, int type)
        {
            Assert.Equal(rows, value.Rows);
            Assert.Equal(cols, value.Cols);
            Assert.Equal(type, value.Type);
        }

        private static void AssertArrayNear(float[] expected, float[] actual, float tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                AssertNear(expected[i], actual[i], tolerance);
            }
        }

        private static void AssertNear(float expected, float actual, float tolerance)
        {
            Assert.InRange(Math.Abs(expected - actual), 0.0F, tolerance);
        }
    }
}
