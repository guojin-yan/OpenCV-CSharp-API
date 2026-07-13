using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class CameraMatrixUtilitiesTests
    {
        [Fact]
        public void CameraMatrixUtilitiesValidateInputsAndOwnership()
        {
            using Mat camera = CreateCameraMatrix64();
            using var output = new Mat();
            using var emptyCamera = new Mat();
            using var invalidShape = new Mat(2, 3, MatType.CV_64FC1);
            using var invalidChannels = new Mat(3, 3, MatType.CV_64FC2);
            using var invalidDepth = new Mat(3, 3, MatType.CV_32SC1);
            using var distCoeffs = CreateMatrix64(1, 5, 0.1, -0.05, 0.001, -0.002, 0.01);
            using var invalidDistShape = new Mat(2, 2, MatType.CV_64FC1);
            using var invalidDistCount = new Mat(1, 6, MatType.CV_64FC1);
            using var invalidDistDepth = new Mat(1, 5, MatType.CV_32SC1);
            using var invalidRectification = new Mat(2, 3, MatType.CV_64FC1);
            using var invalidProjection = new Mat(4, 4, MatType.CV_64FC1);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(null!, output));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(camera, null!));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(emptyCamera, output));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(invalidShape, output));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(invalidChannels, output));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(invalidDepth, output));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(camera, camera));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(
                    camera,
                    output,
                    new Size(0, 480),
                    centerPrincipalPoint: true));

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.GetUndistortRectangles(
                    null!,
                    distCoeffs,
                    new Size(640, 480),
                    out _,
                    out _));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.GetUndistortRectangles(
                    camera,
                    null!,
                    new Size(640, 480),
                    out _,
                    out _));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetUndistortRectangles(
                    camera,
                    invalidDistShape,
                    new Size(640, 480),
                    out _,
                    out _));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetUndistortRectangles(
                    camera,
                    invalidDistCount,
                    new Size(640, 480),
                    out _,
                    out _));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetUndistortRectangles(
                    camera,
                    invalidDistDepth,
                    new Size(640, 480),
                    out _,
                    out _));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.GetUndistortRectangles(
                    camera,
                    distCoeffs,
                    new Size(640, 0),
                    out _,
                    out _));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetUndistortRectangles(
                    camera,
                    distCoeffs,
                    new Size(640, 480),
                    out _,
                    out _,
                    r: invalidRectification));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetUndistortRectangles(
                    camera,
                    distCoeffs,
                    new Size(640, 480),
                    out _,
                    out _,
                    newCameraMatrix: invalidProjection));

            using Mat disposedCamera = CreateCameraMatrix64();
            disposedCamera.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(disposedCamera, output));

            using var disposedOutput = new Mat();
            disposedOutput.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(camera, disposedOutput));

            using Mat disposedDist = CreateMatrix64(1, 5, 0.0, 0.0, 0.0, 0.0, 0.0);
            disposedDist.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
                Calib3DCv2.GetUndistortRectangles(
                    camera,
                    disposedDist,
                    new Size(640, 480),
                    out _,
                    out _));

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.GetDefaultNewCameraMatrix(invalidShape));
        }

        [Fact]
        public void DefaultNewCameraMatrixPreservesValuesAndConvertsDepthWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            double[] expected =
            {
                610.0, 1.5, 321.25,
                0.25, 620.0, 239.75,
                0.001, 0.002, 1.0
            };
            using Mat camera64 = CreateMatrix64(3, 3, expected);
            using Mat camera32 = CreateMatrix32(
                3,
                3,
                Array.ConvertAll(expected, static value => (float)value));
            using var callerOwned = new Mat();

            Calib3DCv2.GetDefaultNewCameraMatrix(camera64, callerOwned);
            using Mat owned64 = Calib3DCv2.GetDefaultNewCameraMatrix(camera64);
            using Mat owned32 = Calib3DCv2.GetDefaultNewCameraMatrix(camera32);

            Assert.Equal(MatType.CV_64FC1, callerOwned.Type);
            Assert.Equal(MatType.CV_64FC1, owned64.Type);
            Assert.Equal(MatType.CV_64FC1, owned32.Type);
            AssertArrayNear(expected, callerOwned.ToArray<double>(), 1.0e-12);
            AssertArrayNear(expected, owned64.ToArray<double>(), 1.0e-12);
            AssertArrayNear(expected, owned32.ToArray<double>(), 1.0e-4);

            owned64.SetValue(0, -123.0);
            AssertArrayNear(expected, camera64.ToArray<double>(), 1.0e-12);
            AssertArrayNear(
                Array.ConvertAll(expected, static value => (float)value),
                camera32.ToArray<float>(),
                1.0e-6F);
        }

        [Fact]
        public void DefaultNewCameraMatrixCentersPrincipalPointAcrossOwnershipModesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat camera = CreateMatrix64(
                3,
                3,
                600.0, 2.0, 300.0,
                0.5, 610.0, 220.0,
                0.01, 0.02, 1.0);
            using var callerOwned = new Mat();
            var size = new Size(640, 480);

            Calib3DCv2.GetDefaultNewCameraMatrix(
                camera,
                callerOwned,
                size,
                centerPrincipalPoint: true);
            using Mat owned = Calib3DCv2.GetDefaultNewCameraMatrix(
                camera,
                size,
                centerPrincipalPoint: true);

            double[] expected =
            {
                600.0, 2.0, 319.5,
                0.5, 610.0, 239.5,
                0.01, 0.02, 1.0
            };
            AssertArrayNear(expected, callerOwned.ToArray<double>(), 1.0e-12);
            AssertArrayNear(expected, owned.ToArray<double>(), 1.0e-12);
            Assert.Equal(camera.ToArray<double>(), new[]
            {
                600.0, 2.0, 300.0,
                0.5, 610.0, 220.0,
                0.01, 0.02, 1.0
            });
        }

        [Fact]
        public void UndistortRectanglesDistinguishNormalizedAndPixelCoordinatesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            const int width = 640;
            const int height = 480;
            using Mat camera = CreateCameraMatrix64();
            using var zeroDistortion = new Mat();
            double[] cameraSnapshot = camera.ToArray<double>();

            Calib3DCv2.GetUndistortRectangles(
                camera,
                zeroDistortion,
                new Size(width, height),
                out Rect2d normalizedInner,
                out Rect2d normalizedOuter);
            Calib3DCv2.GetUndistortRectangles(
                camera,
                zeroDistortion,
                new Size(width, height),
                out Rect2d pixelInner,
                out Rect2d pixelOuter,
                newCameraMatrix: camera);

            Rect2d expectedNormalized = new Rect2d(
                -320.0 / 600.0,
                -240.0 / 610.0,
                (width - 1.0) / 600.0,
                (height - 1.0) / 610.0);
            Rect2d expectedPixels = new Rect2d(0.0, 0.0, width - 1.0, height - 1.0);

            AssertRectNear(expectedNormalized, normalizedInner, 1.0e-12);
            AssertRectNear(expectedNormalized, normalizedOuter, 1.0e-12);
            AssertRectNear(expectedPixels, pixelInner, 1.0e-9);
            AssertRectNear(expectedPixels, pixelOuter, 1.0e-9);
            Assert.NotEqual(normalizedOuter, pixelOuter);
            Assert.Equal(cameraSnapshot, camera.ToArray<double>());
            Assert.True(zeroDistortion.Empty);
        }

        [Fact]
        public void UndistortRectanglesSupportDistortionRectificationAndProjectionWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat camera = CreateCameraMatrix64();
            using Mat distortion = CreateMatrix64(
                1,
                5,
                0.12, -0.08, 0.0015, -0.002, 0.015);
            using Mat rectification = CreateMatrix64(
                3,
                3,
                1.0, 0.0, 0.0,
                0.0, 1.0, 0.0,
                0.0, 0.0, 1.0);
            using Mat projection = CreateMatrix64(
                3,
                4,
                590.0, 0.0, 315.0, 12.0,
                0.0, 605.0, 235.0, -8.0,
                0.0, 0.0, 1.0, 0.0);
            double[] cameraSnapshot = camera.ToArray<double>();
            double[] distortionSnapshot = distortion.ToArray<double>();
            double[] rectificationSnapshot = rectification.ToArray<double>();
            double[] projectionSnapshot = projection.ToArray<double>();

            Calib3DCv2.GetUndistortRectangles(
                camera,
                distortion,
                new Size(640, 480),
                out Rect2d inner,
                out Rect2d outer,
                rectification,
                projection);

            AssertFinite(inner);
            AssertFinite(outer);
            Assert.True(inner.Width > 0.0);
            Assert.True(inner.Height > 0.0);
            Assert.True(outer.Width > 0.0);
            Assert.True(outer.Height > 0.0);
            Assert.True(inner.Left >= outer.Left - 1.0e-9);
            Assert.True(inner.Top >= outer.Top - 1.0e-9);
            Assert.True(inner.Right <= outer.Right + 1.0e-9);
            Assert.True(inner.Bottom <= outer.Bottom + 1.0e-9);
            Assert.Equal(cameraSnapshot, camera.ToArray<double>());
            Assert.Equal(distortionSnapshot, distortion.ToArray<double>());
            Assert.Equal(rectificationSnapshot, rectification.ToArray<double>());
            Assert.Equal(projectionSnapshot, projection.ToArray<double>());
        }

        private static Mat CreateCameraMatrix64()
        {
            return CreateMatrix64(
                3,
                3,
                600.0, 0.0, 320.0,
                0.0, 610.0, 240.0,
                0.0, 0.0, 1.0);
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

        private static void AssertRectNear(Rect2d expected, Rect2d actual, double tolerance)
        {
            AssertNear(expected.X, actual.X, tolerance);
            AssertNear(expected.Y, actual.Y, tolerance);
            AssertNear(expected.Width, actual.Width, tolerance);
            AssertNear(expected.Height, actual.Height, tolerance);
        }

        private static void AssertArrayNear(double[] expected, double[] actual, double tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                AssertNear(expected[i], actual[i], tolerance);
            }
        }

        private static void AssertArrayNear(float[] expected, float[] actual, float tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.True(Math.Abs(expected[i] - actual[i]) <= tolerance);
            }
        }

        private static void AssertNear(double expected, double actual, double tolerance)
        {
            Assert.True(
                Math.Abs(expected - actual) <= tolerance,
                $"Expected {expected:R}, actual {actual:R}, tolerance {tolerance:R}.");
        }

        private static void AssertFinite(Rect2d value)
        {
            Assert.True(double.IsFinite(value.X));
            Assert.True(double.IsFinite(value.Y));
            Assert.True(double.IsFinite(value.Width));
            Assert.True(double.IsFinite(value.Height));
        }
    }
}
