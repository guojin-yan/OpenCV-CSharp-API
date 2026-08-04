using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class UndistortImagePointsTests
    {
        [Fact]
        public void UndistortImagePointsValidateInputsAndCriteria()
        {
            using Mat points = Calib3DCv2.ToPointMat(new[]
            {
                new Point2f(120.0F, 90.0F),
                new Point2f(320.0F, 240.0F)
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

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.UndistortImagePoints(
                    src: null!,
                    dst: output,
                    cameraMatrix: camera,
                    distCoeffs: distortion));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.UndistortImagePoints(
                    src: points,
                    dst: null!,
                    cameraMatrix: camera,
                    distCoeffs: distortion));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.UndistortImagePoints(
                    src: points,
                    dst: output,
                    cameraMatrix: null!,
                    distCoeffs: distortion));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.UndistortImagePoints(
                    src: points,
                    dst: output,
                    cameraMatrix: camera,
                    distCoeffs: null!));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortImagePoints(emptyPoints, output, camera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortImagePoints(invalidPointShape, output, camera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortImagePoints(invalidPointChannels, output, camera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortImagePoints(invalidPointDepth, output, camera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortImagePoints(points, output, invalidCamera, distortion));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortImagePoints(points, output, camera, invalidDistortion));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.UndistortImagePoints(
                    points,
                    output,
                    camera,
                    distortion,
                    new TermCriteria((TermCriteriaTypes)0, 5, 0.01)));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.UndistortImagePoints(
                    points,
                    output,
                    camera,
                    distortion,
                    TermCriteria.ByCount(0)));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortImagePoints(Array.Empty<Point2f>(), camera, distortion));

#if NETCOREAPP3_1_OR_GREATER
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.UndistortImagePoints(
                    ReadOnlySpan<Point2f>.Empty,
                    camera,
                    distortion));
#endif

            using Mat disposedPoints = Calib3DCv2.ToPointMat(new[] { new Point2f(1.0F, 2.0F) });
            disposedPoints.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
                Calib3DCv2.UndistortImagePoints(disposedPoints, output, camera, distortion));

            using var disposedOutput = new Mat();
            disposedOutput.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
                Calib3DCv2.UndistortImagePoints(points, disposedOutput, camera, distortion));
        }

        [Fact]
        public void CallerOwnedAndOwnedFloatOutputsAgreeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f[] input =
            {
                new Point2f(96.0F, 72.0F),
                new Point2f(320.0F, 240.0F),
                new Point2f(548.0F, 391.0F)
            };
            using Mat points = Calib3DCv2.ToPointMat(input);
            using Mat camera = CreateCameraMatrix();
            using Mat distortion = CreateDistortion();
            using var callerOwned = new Mat();
            Point2f[] snapshot = points.ToArray<Point2f>();

            Calib3DCv2.UndistortImagePoints(points, callerOwned, camera, distortion);
            using Mat owned = Calib3DCv2.UndistortImagePoints(points, camera, distortion);

            Assert.Equal(MatType.CV_32FC2, callerOwned.Type);
            Assert.Equal(MatType.CV_32FC2, owned.Type);
            AssertPointArraysNear(
                callerOwned.ToArray<Point2f>(),
                owned.ToArray<Point2f>(),
                1.0e-6F);
            AssertPointArraysNear(snapshot, points.ToArray<Point2f>(), 0.0F);
        }

        [Fact]
        public void DoublePrecisionZeroDistortionPreservesPixelCoordinatesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2d[] input =
            {
                new Point2d(40.25, 51.75),
                new Point2d(319.5, 239.5),
                new Point2d(601.125, 421.875)
            };
            using Mat points = CreatePoint2dMat(input);
            using Mat camera = CreateCameraMatrix();
            using var zeroDistortion = new Mat();
            using Mat result = Calib3DCv2.UndistortImagePoints(
                points,
                camera,
                zeroDistortion);

            Assert.Equal(MatType.CV_64FC2, result.Type);
            AssertPointArraysNear(input, result.ToArray<Point2d>(), 1.0e-12);
            AssertPointArraysNear(input, points.ToArray<Point2d>(), 0.0);
            Assert.True(zeroDistortion.Empty);
        }

        [Fact]
        public void UndistortImagePointsMatchesProjectedUndistortPointsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat points = Calib3DCv2.ToPointMat(new[]
            {
                new Point2f(71.0F, 53.0F),
                new Point2f(211.0F, 183.0F),
                new Point2f(507.0F, 364.0F),
                new Point2f(612.0F, 455.0F)
            });
            using Mat camera = CreateCameraMatrix();
            using Mat distortion = CreateDistortion();
            TermCriteria criteria = TermCriteria.ByCountAndEpsilon(50, 1.0e-12);
            using Mat actual = Calib3DCv2.UndistortImagePoints(
                points,
                camera,
                distortion,
                criteria);
            using var expected = new Mat();

            Calib3DCv2.UndistortPoints(
                points,
                expected,
                camera,
                distortion,
                p: camera,
                criteria: criteria);

            AssertPointArraysNear(
                expected.ToArray<Point2f>(),
                actual.ToArray<Point2f>(),
                1.0e-6F);
        }

        [Fact]
        public void ArrayAndSpanOverloadsAgreeAndPreserveInputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point2f[] input =
            {
                new Point2f(84.0F, 61.0F),
                new Point2f(298.0F, 219.0F),
                new Point2f(531.0F, 407.0F)
            };
            Point2f[] snapshot = (Point2f[])input.Clone();
            using Mat camera = CreateCameraMatrix();
            using Mat distortion = CreateDistortion();
            using Mat arrayResult = Calib3DCv2.UndistortImagePoints(
                input,
                camera,
                distortion);

#if NETCOREAPP3_1_OR_GREATER
            using Mat spanResult = Calib3DCv2.UndistortImagePoints(
                input.AsSpan(),
                camera,
                distortion);
            AssertPointArraysNear(
                arrayResult.ToArray<Point2f>(),
                spanResult.ToArray<Point2f>(),
                1.0e-6F);
#endif

            Assert.Equal(snapshot, input);
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
