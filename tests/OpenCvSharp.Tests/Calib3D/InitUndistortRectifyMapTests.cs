using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class InitUndistortRectifyMapTests
    {
        [Fact]
        public void InitUndistortRectifyMapValidatesInputsBeforeNativeCall()
        {
            using Mat camera = CreateCameraMatrix();
            using Mat distCoeffs = CreateZeroDistCoeffs();
            using Mat r = Mat.Eye(3, 3, MatType.CV_64FC1);
            using Mat newCamera = CreateCameraMatrix();
            using var map1 = new Mat();
            using var map2 = new Mat();
            using var invalidCamera = new Mat(2, 3, MatType.CV_64FC1);
            using var invalidDistCoeffs = new Mat(1, 6, MatType.CV_64FC1);
            using var invalidRectification = new Mat(2, 3, MatType.CV_64FC1);
            using var invalidProjection = new Mat(4, 4, MatType.CV_64FC1);

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.InitUndistortRectifyMap(
                    invalidCamera,
                    distCoeffs,
                    r,
                    newCamera,
                    new Size(4, 3),
                    MatType.CV_32FC1,
                    map1,
                    map2));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.InitUndistortRectifyMap(
                    camera,
                    invalidDistCoeffs,
                    r,
                    newCamera,
                    new Size(4, 3),
                    MatType.CV_32FC1,
                    map1,
                    map2));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.InitUndistortRectifyMap(
                    camera,
                    distCoeffs,
                    invalidRectification,
                    newCamera,
                    new Size(4, 3),
                    MatType.CV_32FC1,
                    map1,
                    map2));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.InitUndistortRectifyMap(
                    camera,
                    distCoeffs,
                    r,
                    invalidProjection,
                    new Size(4, 3),
                    MatType.CV_32FC1,
                    map1,
                    map2));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.InitUndistortRectifyMap(
                    camera,
                    distCoeffs,
                    r,
                    newCamera,
                    new Size(0, 3),
                    MatType.CV_32FC1,
                    map1,
                    map2));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.InitUndistortRectifyMap(
                    camera,
                    distCoeffs,
                    r,
                    newCamera,
                    new Size(4, 3),
                    MatType.CV_8UC1,
                    map1,
                    map2));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.InitUndistortRectifyMap(
                    camera,
                    distCoeffs,
                    r,
                    newCamera,
                    new Size(4, 3),
                    MatType.CV_32FC1,
                    map1,
                    map1));

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.InitUndistortRectifyMap(
                    null!,
                    distCoeffs,
                    r,
                    newCamera,
                    new Size(4, 3),
                    MatType.CV_32FC1));
            Assert.Throws<ArgumentNullException>(() =>
                new UndistortRectifyMapResult(null!, map2));
            Assert.Throws<ArgumentNullException>(() =>
                new UndistortRectifyMapResult(map1, null!));
        }

        [Fact]
        public void InitUndistortRectifyMapOwnedAndCallerOwnedMapsAgreeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Size size = new Size(4, 3);
            using Mat camera = CreateCameraMatrix();
            using Mat distCoeffs = CreateZeroDistCoeffs();
            using Mat r = Mat.Eye(3, 3, MatType.CV_64FC1);
            using Mat newCamera = CreateCameraMatrix();
            using var map1 = new Mat();
            using var map2 = new Mat();

            Calib3DCv2.InitUndistortRectifyMap(
                camera,
                distCoeffs,
                r,
                newCamera,
                size,
                MatType.CV_32FC1,
                map1,
                map2);

            UndistortRectifyMapResult owned = Calib3DCv2.InitUndistortRectifyMap(
                camera,
                distCoeffs,
                r,
                newCamera,
                size,
                MatType.CV_32FC1);

            using (owned.Map1)
            using (owned.Map2)
            {
                AssertMapShape(map1, size, MatType.CV_32FC1);
                AssertMapShape(map2, size, MatType.CV_32FC1);
                AssertMapShape(owned.Map1, size, MatType.CV_32FC1);
                AssertMapShape(owned.Map2, size, MatType.CV_32FC1);
                Assert.Equal(map1.ToArray<float>(), owned.Map1.ToArray<float>());
                Assert.Equal(map2.ToArray<float>(), owned.Map2.ToArray<float>());
                Assert.Equal(size.Height, owned.Rows);
                Assert.Equal(size.Width, owned.Cols);
                Assert.Equal("{Map1=3x4,Map2=3x4}", owned.ToString());

                float[] expectedX =
                {
                    0.0F, 1.0F, 2.0F, 3.0F,
                    0.0F, 1.0F, 2.0F, 3.0F,
                    0.0F, 1.0F, 2.0F, 3.0F
                };
                float[] expectedY =
                {
                    0.0F, 0.0F, 0.0F, 0.0F,
                    1.0F, 1.0F, 1.0F, 1.0F,
                    2.0F, 2.0F, 2.0F, 2.0F
                };
                AssertArrayNear(expectedX, map1.ToArray<float>(), 1.0e-5F);
                AssertArrayNear(expectedY, map2.ToArray<float>(), 1.0e-5F);
            }
        }

        private static Mat CreateCameraMatrix()
        {
            var result = new Mat(3, 3, MatType.CV_64FC1);
            result.CopyFrom(new[]
            {
                500.0, 0.0, 1.5,
                0.0, 500.0, 1.0,
                0.0, 0.0, 1.0
            });
            return result;
        }

        private static Mat CreateZeroDistCoeffs()
        {
            var result = new Mat(1, 5, MatType.CV_64FC1);
            result.SetTo(new Scalar(0.0));
            return result;
        }

        private static void AssertMapShape(Mat value, Size size, int type)
        {
            Assert.Equal(size.Height, value.Rows);
            Assert.Equal(size.Width, value.Cols);
            Assert.Equal(type, value.Type);
        }

        private static void AssertArrayNear(float[] expected, float[] actual, float tolerance)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; ++i)
            {
                Assert.True(
                    Math.Abs(expected[i] - actual[i]) <= tolerance,
                    $"Expected {expected[i]:R}, actual {actual[i]:R}, tolerance {tolerance:R} at index {i}.");
            }
        }
    }
}
