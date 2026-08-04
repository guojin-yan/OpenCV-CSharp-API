using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Stitching;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Stitching
{
    public sealed class PyRotationWarperTests
    {
        private static readonly string[] SupportedNames =
        {
            "plane", "affine", "cylindrical", "spherical", "fisheye", "stereographic",
            "compressedPlaneA2B1", "compressedPlaneA1.5B1",
            "compressedPlanePortraitA2B1", "compressedPlanePortraitA1.5B1",
            "paniniA2B1", "paniniA1.5B1", "paniniPortraitA2B1", "paniniPortraitA1.5B1",
            "mercator", "transverseMercator"
        };

        [Fact]
        public void ConstructorsValidateTextAndScaleBeforeNativeExecution()
        {
            Assert.Throws<ArgumentNullException>(() => new PyRotationWarper(null!, 1.0f));
            Assert.Throws<ArgumentException>(() => new PyRotationWarper(string.Empty, 1.0f));
            Assert.Throws<ArgumentException>(() => new PyRotationWarper("pla\0ne", 1.0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PyRotationWarper("plane", 0.0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PyRotationWarper("plane", -1.0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PyRotationWarper("plane", float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new PyRotationWarper("plane", float.PositiveInfinity));
        }

        [Fact]
        public void ExactUpstreamProjectorNamesConstructWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            foreach (string name in SupportedNames)
            {
                using (var warper = new PyRotationWarper(name, 1.0f))
                {
                    Assert.Equal(1.0f, warper.Scale);
                }
            }
            Assert.Throws<OpenCvException>(() => new PyRotationWarper("Plane", 1.0f));
            Assert.Throws<OpenCvException>(() => new PyRotationWarper("unknown", 1.0f));
        }

        [Fact]
        public void DefaultStateAndScaleMatchOpenCvFiveWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var camera = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var rotation = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var warper = new PyRotationWarper())
            {
                Assert.Equal(1.0f, warper.Scale);
                warper.Scale = 2.0f;
                Assert.Equal(1.0f, warper.Scale);
                Assert.Throws<InvalidOperationException>(() => warper.WarpPoint(new Point2f(1, 2), camera, rotation));
                Assert.Throws<ArgumentOutOfRangeException>(() => warper.Scale = 0.0f);
            }
        }

        [Fact]
        public void PointProjectionRoundTripsForContinuousAndRoiMatricesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var camera = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var rotationParent = Mat.Zeros(5, 5, MatType.CV_32FC1))
            using (var rotation = rotationParent.SubMat(new Rect(1, 1, 3, 3)))
            using (var identity = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var warper = new PyRotationWarper("plane", 1.0f))
            {
                identity.CopyTo(rotation);
                Assert.False(rotation.IsContinuous);
                var source = new Point2f(2.25f, 3.5f);
                Point2f projected = warper.WarpPoint(source, camera, rotation);
                Point2f restored = warper.WarpPointBackward(projected, camera, rotation);
                Assert.InRange(Math.Abs(projected.X - source.X), 0.0f, 0.00001f);
                Assert.InRange(Math.Abs(projected.Y - source.Y), 0.0f, 0.00001f);
                Assert.InRange(Math.Abs(restored.X - source.X), 0.0f, 0.00001f);
                Assert.InRange(Math.Abs(restored.Y - source.Y), 0.0f, 0.00001f);
            }
        }

        [Fact]
        public void MapsAndRoiHaveExactShapeTypeAndCallerOwnershipWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var camera = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var rotation = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var xMap = new Mat())
            using (var yMap = new Mat())
            {
                var warper = new PyRotationWarper("plane", 1.0f);
                Rect mapRoi = warper.BuildMaps(new Size(5, 4), camera, rotation, xMap, yMap);
                Rect warpRoi = warper.WarpRoi(new Size(5, 4), camera, rotation);
                Assert.Equal(MatType.CV_32FC1, xMap.Type);
                Assert.Equal(MatType.CV_32FC1, yMap.Type);
                Assert.Equal(new Size(5, 4), new Size(xMap.Cols, xMap.Rows));
                Assert.Equal(xMap.Rows, yMap.Rows);
                Assert.Equal(xMap.Cols, yMap.Cols);
                Assert.Equal(new Rect(0, 0, 4, 3), mapRoi);
                Assert.Equal(new Rect(0, 0, 5, 4), warpRoi);
                warper.Dispose();
                Assert.False(xMap.Empty);
                Assert.False(yMap.Empty);
            }
        }

        [Fact]
        public void ForwardAndBackwardWarpSupportSourceRoiWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var camera = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var rotation = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var parent = new Mat(6, 7, MatType.CV_8UC1, new Scalar(37)))
            using (var source = parent.SubMat(new Rect(1, 1, 5, 4)))
            using (var projected = new Mat())
            using (var restored = new Mat())
            using (var warper = new PyRotationWarper("plane", 1.0f))
            {
                Assert.False(source.IsContinuous);
                Point topLeft = warper.Warp(source, camera, rotation, InterpolationFlags.Nearest, BorderTypes.Replicate, projected);
                Assert.Equal(new Point(0, 0), topLeft);
                Assert.Equal(new Size(5, 4), new Size(projected.Cols, projected.Rows));
                Assert.Equal(MatType.CV_8UC1, projected.Type);
                Assert.Equal(37.0, CoreCv2.Mean(projected).V0, 12);

                warper.WarpBackward(projected, camera, rotation, InterpolationFlags.Nearest, BorderTypes.Replicate, new Size(5, 4), restored);
                Assert.Equal(new Size(5, 4), new Size(restored.Cols, restored.Rows));
                Assert.Equal(MatType.CV_8UC1, restored.Type);
                Assert.Equal(37.0, CoreCv2.Mean(restored).V0, 12);
            }
        }

        [Fact]
        public void ValidationAndDisposalFailClosedWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var camera = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var rotation = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var wrongDepth = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (var wrongShape = Mat.Eye(2, 3, MatType.CV_32FC1))
            using (var source = new Mat(4, 5, MatType.CV_8UC1, new Scalar(1)))
            using (var output = new Mat())
            {
                var warper = new PyRotationWarper("plane", 1.0f);
                Assert.Throws<ArgumentException>(() => warper.WarpPoint(new Point2f(), wrongDepth, rotation));
                Assert.Throws<ArgumentException>(() => warper.WarpPoint(new Point2f(), camera, wrongShape));
                Assert.Throws<ArgumentOutOfRangeException>(() => warper.WarpRoi(new Size(0, 4), camera, rotation));
                Assert.Throws<ArgumentException>(() => warper.BuildMaps(new Size(5, 4), camera, rotation, output, output));
                Assert.Throws<ArgumentException>(() => warper.Warp(source, camera, rotation, InterpolationFlags.Nearest, BorderTypes.Replicate, source));

                warper.Dispose();
                warper.Dispose();
                Assert.True(warper.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => _ = warper.Scale);
                Assert.Throws<ObjectDisposedException>(() => warper.WarpPoint(new Point2f(), camera, rotation));
            }
        }
    }
}
