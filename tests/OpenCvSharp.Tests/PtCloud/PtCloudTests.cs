using System;
using OpenCvSharp.Core;
using OpenCvSharp.PtCloud;

namespace OpenCvSharp.Tests.PtCloud
{
    public sealed class PtCloudTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvPtCloudConstants()
        {
            Assert.Equal(0, (int)RgbdNormalsMethod.Fals);
            Assert.Equal(1, (int)RgbdNormalsMethod.Linemod);
            Assert.Equal(2, (int)RgbdNormalsMethod.Sri);
            Assert.Equal(3, (int)RgbdNormalsMethod.CrossProduct);
            Assert.Equal(0, (int)RgbdPlaneMethod.Default);
        }

        [Fact]
        public void PtCloudFunctionsValidateManagedArguments()
        {
            using (var mat = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.RegisterDepth(null!, mat, mat, mat, mat, new Size(1, 1), mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.RegisterDepth(mat, null!, mat, mat, mat, new Size(1, 1), mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.RegisterDepth(mat, mat, null!, mat, mat, new Size(1, 1), mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.RegisterDepth(mat, mat, mat, null!, mat, new Size(1, 1), mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.RegisterDepth(mat, mat, mat, mat, null!, new Size(1, 1), mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.RegisterDepth(mat, mat, mat, mat, mat, new Size(1, 1), null!));

                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.RescaleDepth(null!, MatType.CV_32F, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.RescaleDepth(mat, MatType.CV_32F, null!));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.RescaleDepth(null!));

                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3d(null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3d(mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3d(mat, mat, null!, null));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3d(null!, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3d(mat, null!));

                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3dSparse(null!, mat, mat, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3dSparse(mat, null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3dSparse(mat, mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3dSparse(mat, mat, mat, null!));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3dSparse(null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3dSparse(mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.DepthTo3dSparse(mat, mat, null!));

                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.WarpFrame(null!, mat, mat, mat, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.WarpFrame(mat, mat, mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.WarpFrame(mat, mat, mat, mat, null!));

                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.FindPlanes(null!, mat, mat, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.FindPlanes(mat, mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => PtCloudCv2.FindPlanes(mat, mat, mat, null!));
            }
        }

        [Fact]
        public void RgbdNormalsValidateManagedArgumentsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var cameraMatrix = Mat.Eye(3, 3, MatType.CV_32F))
            using (var normals = RgbdNormals.Create(2, 2, MatType.CV_32F, cameraMatrix, 3, 50.0F, RgbdNormalsMethod.Fals))
            using (var mat = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => normals.Apply(null!, mat));
                Assert.Throws<ArgumentNullException>(() => normals.Apply(mat, null!));
                Assert.Throws<ArgumentNullException>(() => normals.Apply(null!));
                Assert.Throws<ArgumentNullException>(() => normals.GetK(null!));
                Assert.Throws<ArgumentNullException>(() => normals.SetK(null!));
            }
        }

        [Fact]
        public void RgbdNormalsSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var cameraMatrix = Mat.Eye(3, 3, MatType.CV_32F))
            using (var normals = RgbdNormals.Create(2, 2, MatType.CV_32F, cameraMatrix, 3, 50.0F, RgbdNormalsMethod.Fals))
            using (var points = new Mat(2, 2, MatType.CV_32FC4, new Scalar(0)))
            using (var dst = new Mat())
            {
                Assert.False(normals.IsDisposed);
                Assert.Equal(2, normals.Rows);
                Assert.Equal(2, normals.Cols);
                Assert.Equal(3, normals.WindowSize);
                Assert.Equal(MatType.CV_32F, normals.Depth);
                Assert.Equal(RgbdNormalsMethod.Fals, normals.Method);

                normals.Rows = 2;
                normals.Cols = 2;
                normals.WindowSize = 5;
                normals.SetK(cameraMatrix);
                normals.Cache();
                normals.Apply(points, dst);
                using (Mat returnedNormals = normals.Apply(points))
                using (Mat returnedCameraMatrix = normals.GetK())
                {
                    Assert.False(returnedNormals.Empty);
                    Assert.False(returnedCameraMatrix.Empty);
                }

                Assert.Equal(2, normals.Rows);
                Assert.Equal(2, normals.Cols);
                Assert.Equal(5, normals.WindowSize);
                Assert.Equal(RgbdNormalsMethod.Fals, normals.Method);

                normals.Dispose();
                Assert.True(normals.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => normals.Rows);
                Assert.Throws<ObjectDisposedException>(() => normals.Rows = 2);
                Assert.Throws<ObjectDisposedException>(() => normals.Cols);
                Assert.Throws<ObjectDisposedException>(() => normals.Cols = 2);
                Assert.Throws<ObjectDisposedException>(() => normals.WindowSize);
                Assert.Throws<ObjectDisposedException>(() => normals.WindowSize = 3);
                Assert.Throws<ObjectDisposedException>(() => normals.Depth);
                Assert.Throws<ObjectDisposedException>(() => normals.Method);
                Assert.Throws<ObjectDisposedException>(() => normals.Apply(points, dst));
                Assert.Throws<ObjectDisposedException>(() => normals.Apply(points));
                Assert.Throws<ObjectDisposedException>(() => normals.Cache());
                Assert.Throws<ObjectDisposedException>(() => normals.GetK(dst));
                Assert.Throws<ObjectDisposedException>(() => normals.GetK());
                Assert.Throws<ObjectDisposedException>(() => normals.SetK(cameraMatrix));
            }
        }

        [Fact]
        public void PtCloudFunctionSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var depth = new Mat(2, 2, MatType.CV_16UC1, new Scalar(1000)))
            using (var cameraMatrix = Mat.Eye(3, 3, MatType.CV_32F))
            using (var rescaled = new Mat())
            using (var points3d = new Mat())
            {
                PtCloudCv2.RescaleDepth(depth, MatType.CV_32F, rescaled);
                PtCloudCv2.DepthTo3d(rescaled, cameraMatrix, points3d);
                using (Mat returnedRescaled = PtCloudCv2.RescaleDepth(depth))
                using (Mat returnedPoints3d = PtCloudCv2.DepthTo3d(rescaled, cameraMatrix))
                {
                    Assert.False(returnedRescaled.Empty);
                    Assert.False(returnedPoints3d.Empty);
                }

                Assert.False(rescaled.Empty);
                Assert.NotNull(points3d);
            }
        }

    }
}
