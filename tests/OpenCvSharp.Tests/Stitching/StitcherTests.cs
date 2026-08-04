using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Stitching;

namespace JYPPX.OpenCvSharp.Tests.Stitching
{
    public sealed class StitcherTests
    {
        [Fact]
        public void StitcherEnumsMatchOpenCvValues()
        {
            Assert.Equal(0, (int)StitcherMode.Panorama);
            Assert.Equal(1, (int)StitcherMode.Scans);
            Assert.Equal(0, (int)StitcherStatus.OK);
            Assert.Equal(1, (int)StitcherStatus.ErrorNeedMoreImages);
            Assert.Equal(2, (int)StitcherStatus.ErrorHomographyEstimationFailed);
            Assert.Equal(3, (int)StitcherStatus.ErrorCameraParametersAdjustFailed);
            Assert.Equal(0, (int)WaveCorrectKind.Horizontal);
            Assert.Equal(1, (int)WaveCorrectKind.Vertical);
            Assert.Equal(2, (int)WaveCorrectKind.Auto);
        }

        [Fact]
        public void StitcherCreateValidatesMode()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Stitcher((StitcherMode)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => Stitcher.Create((StitcherMode)99));
        }

        [Fact]
        public void StitcherCameraParamsExposeValues()
        {
            using (var rotation = Mat.Eye(3, 3, MatType.CV_32F))
            using (var translation = new Mat(3, 1, MatType.CV_32F, new Scalar(0)))
            {
                var camera = new StitcherCameraParams(100.0, 1.2, 16.0, 17.0, rotation, translation);

                Assert.Equal(100.0, camera.Focal);
                Assert.Equal(1.2, camera.Aspect);
                Assert.Equal(16.0, camera.PrincipalPointX);
                Assert.Equal(17.0, camera.PrincipalPointY);
                Assert.Same(rotation, camera.Rotation);
                Assert.Same(translation, camera.Translation);
                Assert.Equal("{Focal=100,Aspect=1.2,PrincipalPointX=16,PrincipalPointY=17,Rotation=3x3,Translation=3x1}", camera.ToString());

                StitcherCameraParams copy = new StitcherCameraParams(camera);
                StitcherCameraParams clone = camera.Clone();
                Assert.NotSame(camera, copy);
                Assert.NotSame(camera, clone);
                Assert.NotSame(copy, clone);
                Assert.Equal(100.0, copy.Focal);
                Assert.Equal(1.2, copy.Aspect);
                Assert.Equal(16.0, copy.PrincipalPointX);
                Assert.Equal(17.0, copy.PrincipalPointY);
                Assert.Same(rotation, copy.Rotation);
                Assert.Same(translation, copy.Translation);
                Assert.Equal(100.0, clone.Focal);
                Assert.Equal(1.2, clone.Aspect);
                Assert.Equal(16.0, clone.PrincipalPointX);
                Assert.Equal(17.0, clone.PrincipalPointY);
                Assert.Same(rotation, clone.Rotation);
                Assert.Same(translation, clone.Translation);

                Assert.Throws<ArgumentNullException>(() => new StitcherCameraParams(100.0, 1.2, 16.0, 17.0, null!, translation));
                Assert.Throws<ArgumentNullException>(() => new StitcherCameraParams(100.0, 1.2, 16.0, 17.0, rotation, null!));
                Assert.Throws<ArgumentNullException>(() => new StitcherCameraParams(null!));
            }
        }

        [Fact]
        public void StitcherCameraParamsFormatsInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                using (var rotation = Mat.Eye(3, 3, MatType.CV_32F))
                using (var translation = new Mat(3, 1, MatType.CV_32F, new Scalar(0)))
                {
                    var camera = new StitcherCameraParams(100.5, 1.25, 16.75, 17.125, rotation, translation);

                    Assert.Equal(
                        "{Focal=100.5,Aspect=1.25,PrincipalPointX=16.75,PrincipalPointY=17.125,Rotation=3x3,Translation=3x1}",
                        camera.ToString());
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void StitcherManagedValidationRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var stitcher = Stitcher.Create())
            using (var pano = new Mat())
            using (var image = new Mat(8, 8, MatType.CV_8UC3, new Scalar(0)))
            using (var mask = new Mat(8, 8, MatType.CV_8UC1, new Scalar(255)))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => stitcher.WaveCorrectKind = (WaveCorrectKind)99);
                Assert.Throws<ArgumentNullException>(() => stitcher.EstimateTransform((Mat[])null!));
                Assert.Throws<ArgumentException>(() => stitcher.EstimateTransform(Array.Empty<Mat>()));
                Assert.Throws<ArgumentNullException>(() => stitcher.EstimateTransform(new Mat[] { null! }));
                Assert.Throws<ArgumentException>(() => stitcher.EstimateTransform(new[] { image }, Array.Empty<Mat>()));
                Assert.Throws<ArgumentNullException>(() => stitcher.ComposePanorama((Mat)null!));
                Assert.Throws<ArgumentNullException>(() => stitcher.ComposePanorama(new[] { image }, null!));
                Assert.Throws<ArgumentNullException>(() => stitcher.Stitch(new[] { image }, null!));
                Assert.Throws<ArgumentException>(() => stitcher.Stitch(new[] { image }, new[] { mask, mask }, pano));
            }
        }

        [Fact]
        public void StitcherDisposedStateThrowsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = new Mat(8, 8, MatType.CV_8UC3, new Scalar(0)))
            using (var pano = new Mat())
            using (var resultMask = new Mat())
            {
                var stitcher = Stitcher.Create();
                stitcher.Dispose();

                Assert.True(stitcher.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => stitcher.RegistrationResol);
                Assert.Throws<ObjectDisposedException>(() => stitcher.RegistrationResol = 0.2);
                Assert.Throws<ObjectDisposedException>(() => stitcher.SeamEstimationResol);
                Assert.Throws<ObjectDisposedException>(() => stitcher.SeamEstimationResol = 0.1);
                Assert.Throws<ObjectDisposedException>(() => stitcher.CompositingResol);
                Assert.Throws<ObjectDisposedException>(() => stitcher.CompositingResol = -1.0);
                Assert.Throws<ObjectDisposedException>(() => stitcher.PanoConfidenceThresh);
                Assert.Throws<ObjectDisposedException>(() => stitcher.PanoConfidenceThresh = 0.5);
                Assert.Throws<ObjectDisposedException>(() => stitcher.WorkScale);
                Assert.Throws<ObjectDisposedException>(() => stitcher.WaveCorrection);
                Assert.Throws<ObjectDisposedException>(() => stitcher.WaveCorrection = true);
                Assert.Throws<ObjectDisposedException>(() => stitcher.InterpolationFlags);
                Assert.Throws<ObjectDisposedException>(() => stitcher.InterpolationFlags = JYPPX.OpenCvSharp.ImgProc.InterpolationFlags.Linear);
                Assert.Throws<ObjectDisposedException>(() => stitcher.WaveCorrectKind);
                Assert.Throws<ObjectDisposedException>(() => stitcher.WaveCorrectKind = WaveCorrectKind.Auto);
                Assert.Throws<ObjectDisposedException>(() => stitcher.EstimateTransform(new[] { image }));
                Assert.Throws<ObjectDisposedException>(() => stitcher.EstimateTransform(new[] { image }, null));
                Assert.Throws<ObjectDisposedException>(() => stitcher.ComposePanorama(pano));
                Assert.Throws<ObjectDisposedException>(() => stitcher.ComposePanorama(new[] { image }, pano));
                Assert.Throws<ObjectDisposedException>(() => stitcher.Stitch(new[] { image }, pano));
                Assert.Throws<ObjectDisposedException>(() => stitcher.Stitch(new[] { image }, null, pano));
                Assert.Throws<ObjectDisposedException>(() => stitcher.GetComponent());
                Assert.Throws<ObjectDisposedException>(() => stitcher.GetCameras());
                Assert.Throws<ObjectDisposedException>(() => stitcher.GetResultMask(resultMask));
                Assert.Throws<ObjectDisposedException>(() => stitcher.GetResultMask());
            }
        }

        [Fact]
        public void StitcherSmokeReturnsStatusWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var stitcher = Stitcher.Create(StitcherMode.Panorama))
            using (var first = new Mat(24, 24, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (var second = new Mat(24, 24, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (var pano = new Mat())
            {
                stitcher.RegistrationResol = 0.2;
                stitcher.SeamEstimationResol = 0.1;
                stitcher.CompositingResol = -1.0;
                stitcher.PanoConfidenceThresh = 0.5;
                stitcher.WaveCorrection = true;
                stitcher.InterpolationFlags = JYPPX.OpenCvSharp.ImgProc.InterpolationFlags.Linear;
                stitcher.WaveCorrectKind = WaveCorrectKind.Auto;

                StitcherStatus status = stitcher.Stitch(new[] { first, second }, pano);

                Assert.True(Enum.IsDefined(typeof(StitcherStatus), status));
                Assert.Equal(0.2, stitcher.RegistrationResol, 3);
                Assert.Equal(0.1, stitcher.SeamEstimationResol, 3);
                Assert.Equal(-1.0, stitcher.CompositingResol, 3);
                Assert.Equal(0.5, stitcher.PanoConfidenceThresh, 3);
                Assert.True(stitcher.WaveCorrection);
                Assert.Equal(JYPPX.OpenCvSharp.ImgProc.InterpolationFlags.Linear, stitcher.InterpolationFlags);
                Assert.Equal(WaveCorrectKind.Auto, stitcher.WaveCorrectKind);
                Assert.NotNull(stitcher.GetComponent());
                Assert.NotNull(stitcher.GetCameras());
            }
        }

    }
}
