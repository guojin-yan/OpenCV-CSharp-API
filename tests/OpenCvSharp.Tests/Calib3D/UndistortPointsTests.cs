using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class UndistortPointsTests
    {
        [Fact]
        public void UndistortPointsValidatesInputsBeforeNativeCall()
        {
            using Mat points = Calib3DCv2.ToPointMat(new[]
            {
                new Point2f(320.0F, 240.0F),
                new Point2f(940.0F, 855.0F)
            });
            using var output = new Mat();
            using Mat camera = CreateCameraMatrix();
            using Mat distortion = CreateDistortion();
            using var emptyPoints = new Mat();
            using var invalidPointShape = new Mat(3, 3, MatType.CV_32FC1);
            using var invalidPointChannels = new Mat(1, 2, MatType.CV_32FC3);
            using var invalidPointDepth = new Mat(1, 2, MatType.CV_32SC2);
            using var invalidCamera = new Mat(2, 3, MatType.CV_64FC1);
            using var invalidDistortion = new Mat(1, 6, MatType.CV_64FC1);
            using var invalidRectification = new Mat(2, 2, MatType.CV_64FC1);
            using var invalidProjection = new Mat(2, 3, MatType.CV_64FC1);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.UndistortPoints(null!, output, camera, distortion));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.UndistortPoints(points, null!, camera, distortion));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.UndistortPoints(points, output, null!, distortion));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.UndistortPoints(
                    src: points,
                    dst: output,
                    cameraMatrix: camera,
                    distCoeffs: null!));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortPoints(emptyPoints, output, camera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortPoints(invalidPointShape, output, camera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortPoints(invalidPointChannels, output, camera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortPoints(invalidPointDepth, output, camera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortPoints(points, output, invalidCamera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortPoints(points, output, camera, invalidDistortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortPoints(points, output, camera, distortion, invalidRectification));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortPoints(points, output, camera, distortion, p: invalidProjection));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortPoints(points, points, camera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortPoints(points, camera, camera, distortion));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.UndistortPoints(
                    points,
                    output,
                    camera,
                    distortion,
                    criteria: new TermCriteria((TermCriteriaTypes)0, 5, 0.01)));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.UndistortPoints(
                    points,
                    output,
                    camera,
                    distortion,
                    criteria: TermCriteria.ByCount(0)));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.UndistortPoints(null!, camera, distortion));
        }

        [Fact]
        public void OwnedAndCallerOwnedOutputsAgreeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f[] input =
            {
                new Point2f(320.0F, 240.0F),
                new Point2f(940.0F, 855.0F),
                new Point2f(10.0F, -67.5F)
            };
            Point2f[] expectedNormalized =
            {
                new Point2f(0.0F, 0.0F),
                new Point2f(1.0F, 1.0F),
                new Point2f(-0.5F, -0.5F)
            };
            using Mat points = Calib3DCv2.ToPointMat(input);
            using Mat camera = CreateCameraMatrix();
            using var zeroDistortion = new Mat();
            using var callerOwned = new Mat();

            Calib3DCv2.UndistortPoints(points, callerOwned, camera, zeroDistortion);
            using Mat owned = Calib3DCv2.UndistortPoints(points, camera, zeroDistortion);
            using var pixelProjected = new Mat();

            Calib3DCv2.UndistortPoints(
                points,
                pixelProjected,
                camera,
                zeroDistortion,
                p: camera);

            Assert.Equal(MatType.CV_32FC2, callerOwned.Type);
            Assert.Equal(MatType.CV_32FC2, owned.Type);
            Assert.Equal(MatType.CV_32FC2, pixelProjected.Type);
            AssertPointArraysNear(callerOwned.ToArray<Point2f>(), owned.ToArray<Point2f>(), 1.0e-6F);
            AssertPointArraysNear(expectedNormalized, owned.ToArray<Point2f>(), 1.0e-6F);
            AssertPointArraysNear(input, pixelProjected.ToArray<Point2f>(), 1.0e-4F);
            AssertPointArraysNear(input, points.ToArray<Point2f>(), 0.0F);
            Assert.True(zeroDistortion.Empty);
        }

        [Fact]
        public void ScalarPointMatricesAndDoublePrecisionAreSupportedWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat camera = CreateCameraMatrix();
            using var zeroDistortion = new Mat();
            using Mat rowScalarPoints = CreateMatrix32(
                3,
                2,
                320.0F,
                240.0F,
                940.0F,
                855.0F,
                10.0F,
                -67.5F);
            using Mat columnScalarPoints = CreateMatrix32(
                2,
                3,
                320.0F,
                940.0F,
                10.0F,
                240.0F,
                855.0F,
                -67.5F);
            using Mat doublePoints = CreatePoint2dMat(
                new Point2d(320.0, 240.0),
                new Point2d(940.0, 855.0));

            using Mat rowResult = Calib3DCv2.UndistortPoints(rowScalarPoints, camera, zeroDistortion);
            using Mat columnResult = Calib3DCv2.UndistortPoints(columnScalarPoints, camera, zeroDistortion);
            using Mat doubleResult = Calib3DCv2.UndistortPoints(doublePoints, camera, zeroDistortion);

            Assert.Equal(MatType.CV_32FC2, rowResult.Type);
            Assert.Equal(MatType.CV_32FC2, columnResult.Type);
            Assert.Equal(MatType.CV_64FC2, doubleResult.Type);
            AssertPointArraysNear(rowResult.ToArray<Point2f>(), columnResult.ToArray<Point2f>(), 1.0e-6F);
            AssertPointArraysNear(
                new[]
                {
                    new Point2f(0.0F, 0.0F),
                    new Point2f(1.0F, 1.0F),
                    new Point2f(-0.5F, -0.5F)
                },
                rowResult.ToArray<Point2f>(),
                1.0e-6F);
            AssertPointArraysNear(
                new[]
                {
                    new Point2d(0.0, 0.0),
                    new Point2d(1.0, 1.0)
                },
                doubleResult.ToArray<Point2d>(),
                1.0e-12);
        }

        private static Mat CreateCameraMatrix()
        {
            return CreateMatrix64(
                3,
                3,
                620.0, 0.0, 320.0,
                0.0, 615.0, 240.0,
                0.0, 0.0, 1.0);
        }

        private static Mat CreateDistortion()
        {
            return CreateMatrix64(1, 5, 0.14, -0.09, 0.0012, -0.0018, 0.018);
        }

        private static Mat CreateMatrix64(int rows, int cols, params double[] values)
        {
            Assert.Equal(rows * cols, values.Length);
            var result = new Mat(rows, cols, MatType.CV_64FC1);
            for (int i = 0; i < values.Length; ++i)
            {
                result.SetValue(i, values[i]);
            }

            return result;
        }

        private static Mat CreateMatrix32(int rows, int cols, params float[] values)
        {
            Assert.Equal(rows * cols, values.Length);
            var result = new Mat(rows, cols, MatType.CV_32FC1);
            for (int i = 0; i < values.Length; ++i)
            {
                result.SetValue(i, values[i]);
            }

            return result;
        }

        private static Mat CreatePoint2dMat(params Point2d[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_64FC2);
            for (int i = 0; i < points.Length; ++i)
            {
                result.SetValue(i, points[i]);
            }

            return result;
        }

        private static void AssertPointArraysNear(
            Point2f[] expected,
            Point2f[] actual,
            float tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.True(Math.Abs(expected[i].X - actual[i].X) <= tolerance);
                Assert.True(Math.Abs(expected[i].Y - actual[i].Y) <= tolerance);
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
                Assert.True(Math.Abs(expected[i].X - actual[i].X) <= tolerance);
                Assert.True(Math.Abs(expected[i].Y - actual[i].Y) <= tolerance);
            }
        }
    }
}
