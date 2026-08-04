using System;
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class TwoCameraEssentialPoseTests
    {
        [Fact]
        public void TwoCameraEssentialPoseManagedValidationRunsBeforeNativeCall()
        {
            using (Mat points = CreatePointMat(CreateObjectPoints2D()))
            using (Mat camera1 = CreateCameraMatrix(500.0, 510.0, 320.0, 240.0))
            using (Mat dist1 = CreateZeroDistCoeffs())
            using (Mat camera2 = CreateCameraMatrix(520.0, 530.0, 300.0, 250.0))
            using (Mat dist2 = CreateZeroDistCoeffs())
            using (var essential = new Mat())
            using (var r = new Mat())
            using (var t = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FindEssentialMat(null!, points, camera1, dist1, camera2, dist2));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FindEssentialMat(points, null!, camera1, dist1, camera2, dist2));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FindEssentialMat(points, points, null!, dist1, camera2, dist2));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FindEssentialMat(points, points, camera1, null!, camera2, dist2));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FindEssentialMat(points, points, camera1, dist1, null!, dist2));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.FindEssentialMat(points, points, camera1, dist1, camera2, null!));

                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RecoverPose(null!, points, camera1, dist1, camera2, dist2, essential, r, t));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RecoverPose(points, null!, camera1, dist1, camera2, dist2, essential, r, t));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RecoverPose(points, points, null!, dist1, camera2, dist2, essential, r, t));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RecoverPose(points, points, camera1, null!, camera2, dist2, essential, r, t));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RecoverPose(points, points, camera1, dist1, null!, dist2, essential, r, t));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RecoverPose(points, points, camera1, dist1, camera2, null!, essential, r, t));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RecoverPose(points, points, camera1, dist1, camera2, dist2, null!, r, t));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RecoverPose(points, points, camera1, dist1, camera2, dist2, essential, null!, t));
                Assert.Throws<ArgumentNullException>(() =>
                    Calib3DCv2.RecoverPose(points, points, camera1, dist1, camera2, dist2, essential, r, null!));
            }
        }

        [Fact]
        public void TwoCameraEssentialPoseRecoversFiniteOutputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Point3f[] objectPoints = CreateObjectPoints3D();

            using (Mat camera1 = CreateCameraMatrix(500.0, 510.0, 320.0, 240.0))
            using (Mat dist1 = CreateZeroDistCoeffs())
            using (Mat camera2 = CreateCameraMatrix(520.0, 530.0, 300.0, 250.0))
            using (Mat dist2 = CreateZeroDistCoeffs())
            using (Mat rvec1 = CreateVector(0.0, 0.0, 0.0))
            using (Mat tvec1 = CreateVector(0.0, 0.0, 0.0))
            using (Mat rvec2 = CreateVector(0.02, -0.03, 0.01))
            using (Mat tvec2 = CreateVector(0.30, 0.02, 0.05))
            using (Mat points1 = Calib3DCv2.ProjectPoints(objectPoints, rvec1, tvec1, camera1, dist1))
            using (Mat points2 = Calib3DCv2.ProjectPoints(objectPoints, rvec2, tvec2, camera2, dist2))
            using (var mask = new Mat())
            using (Mat estimatedEssential = Calib3DCv2.FindEssentialMat(
                points1,
                points2,
                camera1,
                dist1,
                camera2,
                dist2,
                RobustEstimationAlgorithms.RANSAC,
                0.999,
                1.0,
                mask))
            using (var recoveredEssential = new Mat())
            using (var r = new Mat())
            using (var t = new Mat())
            {
                AssertMatrixShape(estimatedEssential, 3, 3);
                AssertFinite(estimatedEssential);
                Assert.Equal(objectPoints.Length, mask.Rows * mask.Cols);
                Assert.Equal(1, mask.Channels);

                RecoverPoseResult result = Calib3DCv2.RecoverPose(
                    points1,
                    points2,
                    camera1,
                    dist1,
                    camera2,
                    dist2,
                    recoveredEssential,
                    r,
                    t,
                    RobustEstimationAlgorithms.RANSAC,
                    0.999,
                    1.0,
                    mask);

                Assert.True(result.HasInliers);
                Assert.InRange(result.InlierCount, 1, objectPoints.Length);
                AssertMatrixShape(recoveredEssential, 3, 3);
                AssertMatrixShape(r, 3, 3);
                AssertMatrixShape(t, 3, 1);
                AssertFinite(recoveredEssential);
                AssertFinite(r);
                AssertFinite(t);
                Assert.Equal(objectPoints.Length, mask.Rows * mask.Cols);
            }
        }

        private static Point2f[] CreateObjectPoints2D()
        {
            return new[]
            {
                new Point2f(100.0F, 120.0F),
                new Point2f(130.0F, 160.0F),
                new Point2f(170.0F, 110.0F),
                new Point2f(210.0F, 150.0F),
                new Point2f(250.0F, 180.0F),
                new Point2f(280.0F, 140.0F),
                new Point2f(320.0F, 170.0F),
                new Point2f(360.0F, 130.0F)
            };
        }

        private static Point3f[] CreateObjectPoints3D()
        {
            Point3f[] result = new Point3f[24];
            int index = 0;
            for (int y = 0; y < 4; ++y)
            {
                for (int x = 0; x < 6; ++x)
                {
                    float z = 4.0F + (0.08F * x) + (0.05F * y);
                    result[index++] = new Point3f(
                        -0.8F + (0.32F * x),
                        -0.5F + (0.28F * y),
                        z);
                }
            }
            return result;
        }

        private static Mat CreatePointMat(Point2f[] points)
        {
            var result = new Mat(points.Length, 1, MatType.CV_32FC2);
            try
            {
                for (int i = 0; i < points.Length; ++i)
                {
                    result.SetValue(i, points[i]);
                }
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private static Mat CreateCameraMatrix(double fx, double fy, double cx, double cy)
        {
            var result = new Mat(3, 3, MatType.CV_64FC1);
            try
            {
                result.CopyFrom(new[]
                {
                    fx, 0.0, cx,
                    0.0, fy, cy,
                    0.0, 0.0, 1.0
                });
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private static Mat CreateZeroDistCoeffs()
        {
            var result = new Mat(1, 5, MatType.CV_64FC1);
            result.SetTo(new Scalar(0.0));
            return result;
        }

        private static Mat CreateVector(double x, double y, double z)
        {
            var result = new Mat(3, 1, MatType.CV_64FC1);
            try
            {
                result.CopyFrom(new[] { x, y, z });
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private static void AssertMatrixShape(Mat value, int rows, int cols)
        {
            Assert.Equal(rows, value.Rows);
            Assert.Equal(cols, value.Cols);
            Assert.Equal(1, value.Channels);
        }

        private static void AssertFinite(Mat value)
        {
            foreach (double item in value.ToArray<double>())
            {
                Assert.True(double.IsFinite(item));
            }
        }
    }
}
