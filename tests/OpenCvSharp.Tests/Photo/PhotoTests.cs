using System;
using OpenCvSharp.Core;
using OpenCvSharp.Photo;

namespace OpenCvSharp.Tests.Photo
{
    public sealed class PhotoTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvPhotoConstants()
        {
            Assert.Equal(0, (int)InpaintMethod.Ns);
            Assert.Equal(1, (int)InpaintMethod.Telea);
        }

        [Fact]
        public void PhotoFunctionsValidateFirstManagedArgumentBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.Inpaint(null!, null!, null!, 3.0, InpaintMethod.Telea));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.Inpaint(null!, null!, 3.0, InpaintMethod.Telea));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.FastNlMeansDenoising(null!, null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.FastNlMeansDenoising(null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.FastNlMeansDenoisingColored(null!, null!));
            Assert.Throws<ArgumentNullException>(() => PhotoCv2.FastNlMeansDenoisingColored(null!));
        }

        [Fact]
        public void InpaintRejectsInvalidMethodBeforeNativeCall()
        {
            using (var src = new Mat())
            using (var mask = new Mat())
            using (var dst = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => PhotoCv2.Inpaint(src, mask, dst, 3.0, (InpaintMethod)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => PhotoCv2.Inpaint(src, mask, 3.0, (InpaintMethod)99));
            }
        }

        [Fact]
        public void InpaintRejectsInvalidImageAndMaskContractBeforeNativeCall()
        {
            using (var src = new Mat(4, 4, MatType.CV_8UC1))
            using (var unsupportedSource = new Mat(4, 4, MatType.CV_8UC2))
            using (var colorMask = new Mat(4, 4, MatType.CV_8UC3))
            using (var smallMask = new Mat(3, 4, MatType.CV_8UC1))
            using (var mask = new Mat(4, 4, MatType.CV_8UC1))
            using (var dst = new Mat())
            {
                ArgumentException sourceException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.Inpaint(unsupportedSource, mask, dst, 3.0, InpaintMethod.Telea));
                ArgumentException maskTypeException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.Inpaint(src, colorMask, dst, 3.0, InpaintMethod.Telea));
                ArgumentException maskSizeException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.Inpaint(src, smallMask, dst, 3.0, InpaintMethod.Telea));
                ArgumentException returnedSourceException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.Inpaint(unsupportedSource, mask, 3.0, InpaintMethod.Telea));
                ArgumentException returnedMaskException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.Inpaint(src, colorMask, 3.0, InpaintMethod.Telea));

                Assert.Equal("src", sourceException.ParamName);
                Assert.Equal("inpaintMask", maskTypeException.ParamName);
                Assert.Equal("inpaintMask", maskSizeException.ParamName);
                Assert.Equal("src", returnedSourceException.ParamName);
                Assert.Equal("inpaintMask", returnedMaskException.ParamName);
            }
        }

        [Fact]
        public void PhotoFunctionsValidateArrayArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat())
            using (Mat dst = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.Inpaint(src, null!, dst, 3.0, InpaintMethod.Telea));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.Inpaint(src, src, null!, 3.0, InpaintMethod.Telea));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.Inpaint(src, null!, 3.0, InpaintMethod.Telea));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.FastNlMeansDenoising(src, null!));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.FastNlMeansDenoising(src, dst, (float[])null!));
                Assert.Throws<ArgumentException>(() => PhotoCv2.FastNlMeansDenoising(src, dst, Array.Empty<float>()));
                Assert.Throws<ArgumentException>(() => PhotoCv2.FastNlMeansDenoising(src, dst, ReadOnlySpan<float>.Empty));
                Assert.Throws<ArgumentNullException>(() => PhotoCv2.FastNlMeansDenoisingColored(src, null!));
            }
        }

        [Fact]
        public void FastNlMeansDenoisingRejectsInvalidSourceAndNormContractBeforeNativeCall()
        {
            using (var empty = new Mat())
            using (var gray8 = new Mat(4, 4, MatType.CV_8UC1))
            using (var color8 = new Mat(4, 4, MatType.CV_8UC3))
            using (var gray16 = new Mat(4, 4, MatType.CV_16UC1))
            using (var gray32 = new Mat(4, 4, MatType.CV_32FC1))
            using (var dst = new Mat())
            {
                ArgumentException emptyException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoising(empty, dst));
                ArgumentException hLengthException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoising(color8, dst, new[] { 1.0F, 2.0F }));
                ArgumentException spanHLengthException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoising(color8, dst, new ReadOnlySpan<float>(new[] { 1.0F, 2.0F })));
                ArgumentException l2DepthException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoising(gray16, dst));
                ArgumentException l1DepthException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoising(gray32, dst, new[] { 1.0F }, normType: NormTypes.L1));
                ArgumentOutOfRangeException normException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    PhotoCv2.FastNlMeansDenoising(gray8, dst, new[] { 1.0F }, normType: NormTypes.Inf));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoising(gray16));

                Assert.Equal("src", emptyException.ParamName);
                Assert.Equal("h", hLengthException.ParamName);
                Assert.Equal("h", spanHLengthException.ParamName);
                Assert.Equal("src", l2DepthException.ParamName);
                Assert.Equal("src", l1DepthException.ParamName);
                Assert.Equal("normType", normException.ParamName);
                Assert.Equal("src", returningException.ParamName);
            }
        }

        [Fact]
        public void FastNlMeansDenoisingColoredRejectsInvalidSourceTypeBeforeNativeCall()
        {
            using (var gray8 = new Mat(4, 4, MatType.CV_8UC1))
            using (var color16 = new Mat(4, 4, MatType.CV_16UC3))
            using (var dst = new Mat())
            {
                ArgumentException grayException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingColored(gray8, dst));
                ArgumentException depthException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingColored(color16, dst));
                ArgumentException returningException = Assert.Throws<ArgumentException>(() =>
                    PhotoCv2.FastNlMeansDenoisingColored(gray8));

                Assert.Equal("src", grayException.ParamName);
                Assert.Equal("src", depthException.ParamName);
                Assert.Equal("src", returningException.ParamName);
            }
        }

        [Fact]
        public void TonemapObjectsCanBeCreatedWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Tonemap tonemap = PhotoCv2.CreateTonemap(1.2F))
            using (TonemapDrago drago = PhotoCv2.CreateTonemapDrago(1.0F, 0.8F, 0.7F))
            using (TonemapReinhard reinhard = PhotoCv2.CreateTonemapReinhard(1.0F, 0.1F, 0.9F, 0.2F))
            using (TonemapMantiuk mantiuk = PhotoCv2.CreateTonemapMantiuk(1.0F, 0.6F, 0.8F))
            {
                Assert.False(tonemap.IsDisposed);
                Assert.False(drago.IsDisposed);
                Assert.False(reinhard.IsDisposed);
                Assert.False(mantiuk.IsDisposed);

                tonemap.Gamma = 1.1F;
                drago.Saturation = 0.9F;
                drago.Bias = 0.8F;
                reinhard.Intensity = 0.2F;
                reinhard.LightAdaptation = 0.7F;
                reinhard.ColorAdaptation = 0.3F;
                mantiuk.Scale = 0.5F;
                mantiuk.Saturation = 0.7F;
            }
        }

        [Fact]
        public void TonemapPropertiesRoundTripAllSettingsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Tonemap tonemap = PhotoCv2.CreateTonemap())
            using (TonemapDrago drago = PhotoCv2.CreateTonemapDrago())
            using (TonemapReinhard reinhard = PhotoCv2.CreateTonemapReinhard())
            using (TonemapMantiuk mantiuk = PhotoCv2.CreateTonemapMantiuk())
            {
                tonemap.Gamma = 1.35F;

                drago.Gamma = 1.25F;
                drago.Saturation = 0.85F;
                drago.Bias = 0.75F;

                reinhard.Gamma = 1.15F;
                reinhard.Intensity = 0.2F;
                reinhard.LightAdaptation = 0.7F;
                reinhard.ColorAdaptation = 0.3F;

                mantiuk.Gamma = 1.05F;
                mantiuk.Scale = 0.55F;
                mantiuk.Saturation = 0.65F;

                Assert.Equal(1.35F, tonemap.Gamma, 3);

                Assert.Equal(1.25F, drago.Gamma, 3);
                Assert.Equal(0.85F, drago.Saturation, 3);
                Assert.Equal(0.75F, drago.Bias, 3);

                Assert.Equal(1.15F, reinhard.Gamma, 3);
                Assert.Equal(0.2F, reinhard.Intensity, 3);
                Assert.Equal(0.7F, reinhard.LightAdaptation, 3);
                Assert.Equal(0.3F, reinhard.ColorAdaptation, 3);

                Assert.Equal(1.05F, mantiuk.Gamma, 3);
                Assert.Equal(0.55F, mantiuk.Scale, 3);
                Assert.Equal(0.65F, mantiuk.Saturation, 3);
            }
        }

        [Fact]
        public void TonemapValidatesManagedArgumentsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Tonemap tonemap = PhotoCv2.CreateTonemap())
            using (Mat src = new Mat(2, 2, MatType.CV_32FC3, new Scalar(0.25, 0.5, 0.75)))
            using (Mat dst = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => tonemap.Process(null!, dst));
                Assert.Throws<ArgumentNullException>(() => tonemap.Process(src, null!));
                Assert.Throws<ArgumentNullException>(() => tonemap.Process(null!));
            }
        }

        [Fact]
        public void TonemapThrowsAfterDisposeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(2, 2, MatType.CV_32FC3, new Scalar(0.25, 0.5, 0.75)))
            using (Mat dst = new Mat())
            {
                Tonemap tonemap = PhotoCv2.CreateTonemap();
                tonemap.Dispose();

                Assert.True(tonemap.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => { float gamma = tonemap.Gamma; });
                Assert.Throws<ObjectDisposedException>(() => tonemap.Gamma = 1.1F);
                Assert.Throws<ObjectDisposedException>(() => tonemap.Process(src, dst));
                Assert.Throws<ObjectDisposedException>(() => tonemap.Process(src));
            }
        }

        [Fact]
        public void TonemapDerivedPropertiesThrowAfterDisposeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            TonemapDrago drago = PhotoCv2.CreateTonemapDrago();
            TonemapReinhard reinhard = PhotoCv2.CreateTonemapReinhard();
            TonemapMantiuk mantiuk = PhotoCv2.CreateTonemapMantiuk();

            drago.Dispose();
            reinhard.Dispose();
            mantiuk.Dispose();

            Assert.True(drago.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => { float saturation = drago.Saturation; });
            Assert.Throws<ObjectDisposedException>(() => drago.Saturation = 0.9F);
            Assert.Throws<ObjectDisposedException>(() => { float bias = drago.Bias; });
            Assert.Throws<ObjectDisposedException>(() => drago.Bias = 0.8F);

            Assert.True(reinhard.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => { float intensity = reinhard.Intensity; });
            Assert.Throws<ObjectDisposedException>(() => reinhard.Intensity = 0.2F);
            Assert.Throws<ObjectDisposedException>(() => { float lightAdaptation = reinhard.LightAdaptation; });
            Assert.Throws<ObjectDisposedException>(() => reinhard.LightAdaptation = 0.7F);
            Assert.Throws<ObjectDisposedException>(() => { float colorAdaptation = reinhard.ColorAdaptation; });
            Assert.Throws<ObjectDisposedException>(() => reinhard.ColorAdaptation = 0.3F);

            Assert.True(mantiuk.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => { float scale = mantiuk.Scale; });
            Assert.Throws<ObjectDisposedException>(() => mantiuk.Scale = 0.5F);
            Assert.Throws<ObjectDisposedException>(() => { float saturation = mantiuk.Saturation; });
            Assert.Throws<ObjectDisposedException>(() => mantiuk.Saturation = 0.7F);
        }

        [Fact]
        public void TonemapReturningOverloadRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Tonemap tonemap = PhotoCv2.CreateTonemap())
            using (Mat src = new Mat(2, 2, MatType.CV_32FC3, new Scalar(0.25, 0.5, 0.75)))
            using (Mat mapped = tonemap.Process(src))
            {
                Assert.False(mapped.Empty);
                Assert.Equal(src.Rows, mapped.Rows);
                Assert.Equal(src.Cols, mapped.Cols);
                Assert.Equal(MatType.CV_32FC3, mapped.Type);
            }
        }

        [Fact]
        public void InpaintReturningOverloadRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat src = new Mat(8, 8, MatType.CV_8UC1, new Scalar(32)))
            using (Mat mask = new Mat(8, 8, MatType.CV_8UC1, new Scalar(0)))
            using (Mat repaired = PhotoCv2.Inpaint(src, mask, 3.0, InpaintMethod.Telea))
            {
                Assert.Equal(src.Rows, repaired.Rows);
                Assert.Equal(src.Cols, repaired.Cols);
                Assert.Equal(src.Type, repaired.Type);
            }
        }

        [Fact]
        public void SingleFrameDenoiseReturningOverloadsRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat gray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(32)))
            using (Mat color = new Mat(8, 8, MatType.CV_8UC3, new Scalar(10, 20, 30)))
            using (Mat denoised = PhotoCv2.FastNlMeansDenoising(gray))
            using (Mat coloredDenoised = PhotoCv2.FastNlMeansDenoisingColored(color))
            {
                Assert.Equal(gray.Rows, denoised.Rows);
                Assert.Equal(gray.Cols, denoised.Cols);
                Assert.Equal(gray.Type, denoised.Type);

                Assert.Equal(color.Rows, coloredDenoised.Rows);
                Assert.Equal(color.Cols, coloredDenoised.Cols);
                Assert.Equal(color.Type, coloredDenoised.Type);
            }
        }

    }
}
