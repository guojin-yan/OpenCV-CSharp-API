using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class StereoRectifyTests
    {
        [Fact]
        public void StereoRectifyValidatesInputsBeforeNativeCall()
        {
            using Mat camera1 = CreateCameraMatrix();
            using Mat camera2 = CreateCameraMatrix();
            using Mat distCoeffs1 = CreateZeroDistCoeffs();
            using Mat distCoeffs2 = CreateZeroDistCoeffs();
            using Mat r = Mat.Eye(3, 3, MatType.CV_64FC1);
            using Mat t = CreateTranslation();
            using var r1 = new Mat();
            using var r2 = new Mat();
            using var p1 = new Mat();
            using var p2 = new Mat();
            using var q = new Mat();
            using var invalidCamera = new Mat(2, 3, MatType.CV_64FC1);
            using var invalidDistCoeffs = new Mat(1, 6, MatType.CV_64FC1);
            using var invalidRotation = new Mat(2, 3, MatType.CV_64FC1);
            using var invalidTranslation = new Mat(2, 1, MatType.CV_64FC1);

            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoRectify(
                    invalidCamera,
                    distCoeffs1,
                    camera2,
                    distCoeffs2,
                    new Size(640, 480),
                    r,
                    t,
                    r1,
                    r2,
                    p1,
                    p2,
                    q,
                    StereoRectifyFlags.ZeroDisparity,
                    -1.0,
                    new Size(),
                    out _,
                    out _));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoRectify(
                    camera1,
                    invalidDistCoeffs,
                    camera2,
                    distCoeffs2,
                    new Size(640, 480),
                    r,
                    t,
                    r1,
                    r2,
                    p1,
                    p2,
                    q,
                    StereoRectifyFlags.ZeroDisparity,
                    -1.0,
                    new Size(),
                    out _,
                    out _));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.StereoRectify(
                    camera1,
                    distCoeffs1,
                    camera2,
                    distCoeffs2,
                    new Size(0, 480),
                    r,
                    t,
                    r1,
                    r2,
                    p1,
                    p2,
                    q,
                    StereoRectifyFlags.ZeroDisparity,
                    -1.0,
                    new Size(),
                    out _,
                    out _));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoRectify(
                    camera1,
                    distCoeffs1,
                    camera2,
                    distCoeffs2,
                    new Size(640, 480),
                    invalidRotation,
                    t,
                    r1,
                    r2,
                    p1,
                    p2,
                    q,
                    StereoRectifyFlags.ZeroDisparity,
                    -1.0,
                    new Size(),
                    out _,
                    out _));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoRectify(
                    camera1,
                    distCoeffs1,
                    camera2,
                    distCoeffs2,
                    new Size(640, 480),
                    r,
                    invalidTranslation,
                    r1,
                    r2,
                    p1,
                    p2,
                    q,
                    StereoRectifyFlags.ZeroDisparity,
                    -1.0,
                    new Size(),
                    out _,
                    out _));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.StereoRectify(
                    camera1,
                    distCoeffs1,
                    camera2,
                    distCoeffs2,
                    new Size(640, 480),
                    r,
                    t,
                    r1,
                    r2,
                    p1,
                    p2,
                    q,
                    StereoRectifyFlags.ZeroDisparity,
                    -1.0,
                    new Size(-1, 480),
                    out _,
                    out _));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.StereoRectify(
                    camera1,
                    distCoeffs1,
                    camera2,
                    distCoeffs2,
                    new Size(640, 480),
                    r,
                    t,
                    r1,
                    r1,
                    p1,
                    p2,
                    q,
                    StereoRectifyFlags.ZeroDisparity,
                    -1.0,
                    new Size(),
                    out _,
                    out _));

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.StereoRectify(
                    null!,
                    distCoeffs1,
                    camera2,
                    distCoeffs2,
                    new Size(640, 480),
                    r,
                    t));
            Assert.Throws<ArgumentNullException>(() =>
                new StereoRectifyResult(null!, r2, p1, p2, q, default, default));
            Assert.Throws<ArgumentNullException>(() =>
                new StereoRectifyResult(r1, null!, p1, p2, q, default, default));
            Assert.Throws<ArgumentNullException>(() =>
                new StereoRectifyResult(r1, r2, null!, p2, q, default, default));
            Assert.Throws<ArgumentNullException>(() =>
                new StereoRectifyResult(r1, r2, p1, null!, q, default, default));
            Assert.Throws<ArgumentNullException>(() =>
                new StereoRectifyResult(r1, r2, p1, p2, null!, default, default));
        }

        [Fact]
        public void StereoRectifyOwnedAndCallerOwnedOutputsAgreeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Size imageSize = new Size(640, 480);
            using Mat camera1 = CreateCameraMatrix();
            using Mat camera2 = CreateCameraMatrix();
            using Mat distCoeffs1 = CreateZeroDistCoeffs();
            using Mat distCoeffs2 = CreateZeroDistCoeffs();
            using Mat r = Mat.Eye(3, 3, MatType.CV_64FC1);
            using Mat t = CreateTranslation();
            using var r1 = new Mat();
            using var r2 = new Mat();
            using var p1 = new Mat();
            using var p2 = new Mat();
            using var q = new Mat();

            Calib3DCv2.StereoRectify(
                camera1,
                distCoeffs1,
                camera2,
                distCoeffs2,
                imageSize,
                r,
                t,
                r1,
                r2,
                p1,
                p2,
                q,
                StereoRectifyFlags.ZeroDisparity,
                -1.0,
                new Size(),
                out Rect roi1,
                out Rect roi2);

            StereoRectifyResult owned = Calib3DCv2.StereoRectify(
                camera1,
                distCoeffs1,
                camera2,
                distCoeffs2,
                imageSize,
                r,
                t);

            using (owned.R1)
            using (owned.R2)
            using (owned.P1)
            using (owned.P2)
            using (owned.Q)
            {
                AssertMatrixShape(r1, 3, 3);
                AssertMatrixShape(r2, 3, 3);
                AssertMatrixShape(p1, 3, 4);
                AssertMatrixShape(p2, 3, 4);
                AssertMatrixShape(q, 4, 4);
                AssertMatrixShape(owned.R1, 3, 3);
                AssertMatrixShape(owned.R2, 3, 3);
                AssertMatrixShape(owned.P1, 3, 4);
                AssertMatrixShape(owned.P2, 3, 4);
                AssertMatrixShape(owned.Q, 4, 4);
                Assert.Equal(roi1, owned.ValidPixROI1);
                Assert.Equal(roi2, owned.ValidPixROI2);
                AssertArrayNear(r1.ToArray<double>(), owned.R1.ToArray<double>(), 1.0e-9);
                AssertArrayNear(r2.ToArray<double>(), owned.R2.ToArray<double>(), 1.0e-9);
                AssertArrayNear(p1.ToArray<double>(), owned.P1.ToArray<double>(), 1.0e-9);
                AssertArrayNear(p2.ToArray<double>(), owned.P2.ToArray<double>(), 1.0e-9);
                AssertArrayNear(q.ToArray<double>(), owned.Q.ToArray<double>(), 1.0e-9);
                Assert.Contains("R1=3x3", owned.ToString(), StringComparison.Ordinal);
                Assert.Contains("P2=3x4", owned.ToString(), StringComparison.Ordinal);
                Assert.Contains("Q=4x4", owned.ToString(), StringComparison.Ordinal);
            }
        }

        private static Mat CreateCameraMatrix()
        {
            var result = new Mat(3, 3, MatType.CV_64FC1);
            result.CopyFrom(new[]
            {
                500.0, 0.0, 320.0,
                0.0, 500.0, 240.0,
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

        private static Mat CreateTranslation()
        {
            var result = new Mat(3, 1, MatType.CV_64FC1);
            result.CopyFrom(new[] { -0.1, 0.0, 0.0 });
            return result;
        }

        private static void AssertMatrixShape(Mat value, int rows, int cols)
        {
            Assert.Equal(rows, value.Rows);
            Assert.Equal(cols, value.Cols);
            Assert.Equal(MatType.CV_64FC1, value.Type);
        }

        private static void AssertArrayNear(double[] expected, double[] actual, double tolerance)
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
