using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.XPhoto;

namespace JYPPX.OpenCvSharp.Tests.XPhoto
{
    public sealed class XPhotoTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvXPhotoConstants()
        {
            Assert.Equal(0, (int)TransformTypes.Haar);
            Assert.Equal(0, (int)Bm3dSteps.StepAll);
            Assert.Equal(1, (int)Bm3dSteps.Step1);
            Assert.Equal(2, (int)Bm3dSteps.Step2);
        }

        [Fact]
        public void XPhotoFunctionsValidateManagedArguments()
        {
            Assert.Throws<ArgumentNullException>(() => XPhotoCv2.ApplyChannelGains(null!, null!, 1.0F, 1.0F, 1.0F));
            Assert.Throws<ArgumentNullException>(() => XPhotoCv2.ApplyChannelGains(null!, 1.0F, 1.0F, 1.0F));
            Assert.Throws<ArgumentNullException>(() => XPhotoCv2.DctDenoising(null!, null!, 1.0));
            Assert.Throws<ArgumentNullException>(() => XPhotoCv2.DctDenoising(null!, 1.0));
            Assert.Throws<ArgumentNullException>(() => XPhotoCv2.Bm3dDenoising(null!, null!));
            Assert.Throws<ArgumentNullException>(() => XPhotoCv2.Bm3dDenoising(null!));
            Assert.Throws<ArgumentNullException>(() => XPhotoCv2.OilPainting(null!, null!, 3, 8));
            Assert.Throws<ArgumentNullException>(() => XPhotoCv2.OilPainting(null!, 3, 8));
        }

        [Fact]
        public void ApplyChannelGainsRejectsInvalidSourceContractBeforeNativeCall()
        {
            if (!IsXPhotoNativeObjectAvailable())
            {
                return;
            }

            using (var empty = new Mat())
            using (var gray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(12)))
            using (var color32 = new Mat(8, 8, MatType.CV_32FC3, new Scalar(12, 32, 64)))
            using (var valid = new Mat(8, 8, MatType.CV_8UC3, new Scalar(12, 32, 64)))
            using (var dst = new Mat())
            {
                ArgumentException emptyException = Assert.Throws<ArgumentException>(() =>
                    XPhotoCv2.ApplyChannelGains(empty, dst, 1.0F, 1.0F, 1.0F));
                Assert.Equal("src", emptyException.ParamName);

                ArgumentException grayException = Assert.Throws<ArgumentException>(() =>
                    XPhotoCv2.ApplyChannelGains(gray, dst, 1.0F, 1.0F, 1.0F));
                Assert.Equal("src", grayException.ParamName);

                ArgumentException color32Exception = Assert.Throws<ArgumentException>(() =>
                    XPhotoCv2.ApplyChannelGains(color32, dst, 1.0F, 1.0F, 1.0F));
                Assert.Equal("src", color32Exception.ParamName);

                using (var nonContinuous = valid.SubMat(new Rect(1, 1, 4, 4)))
                {
                    ArgumentException nonContinuousException = Assert.Throws<ArgumentException>(() =>
                        XPhotoCv2.ApplyChannelGains(nonContinuous, dst, 1.0F, 1.0F, 1.0F));
                    Assert.Equal("src", nonContinuousException.ParamName);
                }
            }
        }

        [Fact]
        public void Bm3dDenoisingRejectsInvalidTransformTypeBeforeNativeCall()
        {
            if (!IsXPhotoNativeObjectAvailable())
            {
                return;
            }

            using (var src = new Mat())
            using (var dst = new Mat())
            using (var dstStep1 = new Mat())
            using (var dstStep2 = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, dst, transformType: (TransformTypes)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, transformType: (TransformTypes)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, dstStep1, dstStep2, transformType: (TransformTypes)99));
            }
        }

        [Fact]
        public void Bm3dDenoisingRejectsInvalidStepBeforeNativeCall()
        {
            if (!IsXPhotoNativeObjectAvailable())
            {
                return;
            }

            using (var src = new Mat())
            using (var dst = new Mat())
            using (var dstStep1 = new Mat())
            using (var dstStep2 = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, dst, step: (Bm3dSteps)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, step: (Bm3dSteps)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, dstStep1, dstStep2, step: (Bm3dSteps)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, dst, step: Bm3dSteps.Step2));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, step: Bm3dSteps.Step2));
            }
        }

        [Fact]
        public void Bm3dDenoisingRejectsInvalidNormTypeBeforeNativeCall()
        {
            if (!IsXPhotoNativeObjectAvailable())
            {
                return;
            }

            using (var src = new Mat())
            using (var dst = new Mat())
            using (var dstStep1 = new Mat())
            using (var dstStep2 = new Mat())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, dst, normType: NormTypes.L2Sqr));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, normType: NormTypes.L2Sqr));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, dstStep1, dstStep2, normType: NormTypes.L2Sqr));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, dst, normType: (NormTypes)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, normType: (NormTypes)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.Bm3dDenoising(src, dstStep1, dstStep2, normType: (NormTypes)99));
            }
        }

        [Fact]
        public void Bm3dDenoisingRejectsInvalidSourceAndWindowContractBeforeNativeCall()
        {
            if (!IsXPhotoNativeObjectAvailable())
            {
                return;
            }

            using (var color = new Mat(8, 8, MatType.CV_8UC3, new Scalar(12, 32, 64)))
            using (var gray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(12)))
            using (var dst = new Mat())
            using (var dstStep1 = new Mat())
            using (var dstStep2 = new Mat())
            {
                ArgumentException sourceException = Assert.Throws<ArgumentException>(() =>
                    XPhotoCv2.Bm3dDenoising(color, dst));
                Assert.Equal("src", sourceException.ParamName);

                ArgumentOutOfRangeException searchWindowException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    XPhotoCv2.Bm3dDenoising(gray, dst, templateWindowSize: 8, searchWindowSize: 8));
                Assert.Equal("searchWindowSize", searchWindowException.ParamName);

                ArgumentOutOfRangeException returningSearchWindowException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    XPhotoCv2.Bm3dDenoising(gray, templateWindowSize: 8, searchWindowSize: 7));
                Assert.Equal("searchWindowSize", returningSearchWindowException.ParamName);

                ArgumentOutOfRangeException slidingStepException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    XPhotoCv2.Bm3dDenoising(gray, dst, templateWindowSize: 8, searchWindowSize: 16, slidingStep: 8));
                Assert.Equal("slidingStep", slidingStepException.ParamName);

                ArgumentOutOfRangeException twoOutputSlidingStepException = Assert.Throws<ArgumentOutOfRangeException>(() =>
                    XPhotoCv2.Bm3dDenoising(gray, dstStep1, dstStep2, templateWindowSize: 8, searchWindowSize: 16, slidingStep: 0));
                Assert.Equal("slidingStep", twoOutputSlidingStepException.ParamName);
            }
        }

        [Fact]
        public void DctDenoisingRejectsInvalidChannelCountBeforeNativeCall()
        {
            if (!IsXPhotoNativeObjectAvailable())
            {
                return;
            }

            using (var src = new Mat(8, 8, MatType.CV_8UC2, new Scalar(12, 32, 0)))
            using (var dst = new Mat())
            {
                Assert.Throws<ArgumentException>(() => XPhotoCv2.DctDenoising(src, dst, 1.0, 4));
                Assert.Throws<ArgumentException>(() => XPhotoCv2.DctDenoising(src, 1.0, 4));
            }
        }

        [Fact]
        public void OilPaintingRejectsInvalidManagedArgumentsBeforeNativeCall()
        {
            if (!IsXPhotoNativeObjectAvailable())
            {
                return;
            }

            using (var src = new Mat(8, 8, MatType.CV_16UC1, new Scalar(12)))
            using (var valid = new Mat(8, 8, MatType.CV_8UC1, new Scalar(12)))
            using (var dst = new Mat())
            {
                Assert.Throws<ArgumentException>(() => XPhotoCv2.OilPainting(src, dst, 3, 8));
                Assert.Throws<ArgumentException>(() => XPhotoCv2.OilPainting(src, dst, 3, 8, ColorConversionCodes.BGR2GRAY));
                Assert.Throws<ArgumentException>(() => XPhotoCv2.OilPainting(src, 3, 8));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.OilPainting(valid, dst, 0, 8));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.OilPainting(valid, dst, 3, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.OilPainting(valid, dst, 3, 128));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.OilPainting(valid, 0, 8));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.OilPainting(valid, 3, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XPhotoCv2.OilPainting(valid, 3, 128));
            }
        }

        [Fact]
        public void WhiteBalancerValidationAndDisposedStateRunWhenNativeObjectIsAvailable()
        {
            using (SimpleWB? simple = TryCreateSimpleWB())
            using (GrayworldWB? grayworld = TryCreateGrayworldWB())
            using (LearningBasedWB? learning = TryCreateLearningBasedWB())
            {
                if (simple == null && grayworld == null && learning == null)
                {
                    return;
                }

                using (var src = new Mat(4, 4, MatType.CV_8UC3, new Scalar(12, 32, 64)))
                using (var gray = new Mat(4, 4, MatType.CV_8UC1, new Scalar(32)))
                using (var dst = new Mat())
                {
                    if (simple != null)
                    {
                        using (var empty = new Mat())
                        using (var unsigned16 = new Mat(4, 4, MatType.CV_16UC1, new Scalar(12)))
                        using (var float64 = new Mat(4, 4, MatType.CV_64FC1, new Scalar(12)))
                        {
                            ArgumentException emptyVoidException = Assert.Throws<ArgumentException>(() =>
                                simple.BalanceWhite(empty, dst));
                            Assert.Equal("src", emptyVoidException.ParamName);

                            ArgumentException emptyReturningException = Assert.Throws<ArgumentException>(() =>
                                simple.BalanceWhite(empty));
                            Assert.Equal("src", emptyReturningException.ParamName);

                            ArgumentException unsigned16VoidException = Assert.Throws<ArgumentException>(() =>
                                simple.BalanceWhite(unsigned16, dst));
                            Assert.Equal("src", unsigned16VoidException.ParamName);

                            ArgumentException unsigned16ReturningException = Assert.Throws<ArgumentException>(() =>
                                simple.BalanceWhite(unsigned16));
                            Assert.Equal("src", unsigned16ReturningException.ParamName);

                            ArgumentException float64VoidException = Assert.Throws<ArgumentException>(() =>
                                simple.BalanceWhite(float64, dst));
                            Assert.Equal("src", float64VoidException.ParamName);

                            ArgumentException float64ReturningException = Assert.Throws<ArgumentException>(() =>
                                simple.BalanceWhite(float64));
                            Assert.Equal("src", float64ReturningException.ParamName);
                        }

                        Assert.Throws<ArgumentNullException>(() => simple.BalanceWhite(null!, dst));
                        Assert.Throws<ArgumentNullException>(() => simple.BalanceWhite(src, null!));
                        Assert.Throws<ArgumentNullException>(() => simple.BalanceWhite(null!));

                        simple.Dispose();
                        Assert.True(simple.IsDisposed);
                        Assert.Throws<ObjectDisposedException>(() => simple.BalanceWhite(src, dst));
                        Assert.Throws<ObjectDisposedException>(() => simple.BalanceWhite(src));
                    }

                    if (grayworld != null)
                    {
                        using (var empty = new Mat())
                        using (var color32 = new Mat(4, 4, MatType.CV_32FC3, new Scalar(12, 32, 64)))
                        using (var nonContinuous = src.SubMat(new Rect(1, 1, 2, 2)))
                        {
                            ArgumentException emptyVoidException = Assert.Throws<ArgumentException>(() =>
                                grayworld.BalanceWhite(empty, dst));
                            Assert.Equal("src", emptyVoidException.ParamName);

                            ArgumentException emptyReturningException = Assert.Throws<ArgumentException>(() =>
                                grayworld.BalanceWhite(empty));
                            Assert.Equal("src", emptyReturningException.ParamName);

                            ArgumentException color32VoidException = Assert.Throws<ArgumentException>(() =>
                                grayworld.BalanceWhite(color32, dst));
                            Assert.Equal("src", color32VoidException.ParamName);

                            ArgumentException color32ReturningException = Assert.Throws<ArgumentException>(() =>
                                grayworld.BalanceWhite(color32));
                            Assert.Equal("src", color32ReturningException.ParamName);

                            ArgumentException nonContinuousVoidException = Assert.Throws<ArgumentException>(() =>
                                grayworld.BalanceWhite(nonContinuous, dst));
                            Assert.Equal("src", nonContinuousVoidException.ParamName);

                            ArgumentException nonContinuousReturningException = Assert.Throws<ArgumentException>(() =>
                                grayworld.BalanceWhite(nonContinuous));
                            Assert.Equal("src", nonContinuousReturningException.ParamName);
                        }

                        Assert.Throws<ArgumentNullException>(() => grayworld.BalanceWhite(null!, dst));
                        Assert.Throws<ArgumentNullException>(() => grayworld.BalanceWhite(src, null!));
                        Assert.Throws<ArgumentNullException>(() => grayworld.BalanceWhite(null!));
                        Assert.Throws<ArgumentException>(() => grayworld.BalanceWhite(gray, dst));
                        Assert.Throws<ArgumentException>(() => grayworld.BalanceWhite(gray));

                        grayworld.Dispose();
                        Assert.True(grayworld.IsDisposed);
                        Assert.Throws<ObjectDisposedException>(() => grayworld.BalanceWhite(src, dst));
                        Assert.Throws<ObjectDisposedException>(() => grayworld.BalanceWhite(src));
                    }

                    if (learning != null)
                    {
                        using (var empty = new Mat())
                        using (var color32 = new Mat(4, 4, MatType.CV_32FC3, new Scalar(12, 32, 64)))
                        using (var nonContinuous = src.SubMat(new Rect(1, 1, 2, 2)))
                        {
                            ArgumentException emptyFeaturesException = Assert.Throws<ArgumentException>(() =>
                                learning.ExtractSimpleFeatures(empty, dst));
                            Assert.Equal("src", emptyFeaturesException.ParamName);

                            ArgumentException emptyBalanceException = Assert.Throws<ArgumentException>(() =>
                                learning.BalanceWhite(empty, dst));
                            Assert.Equal("src", emptyBalanceException.ParamName);

                            ArgumentException color32FeaturesException = Assert.Throws<ArgumentException>(() =>
                                learning.ExtractSimpleFeatures(color32, dst));
                            Assert.Equal("src", color32FeaturesException.ParamName);

                            ArgumentException color32BalanceException = Assert.Throws<ArgumentException>(() =>
                                learning.BalanceWhite(color32, dst));
                            Assert.Equal("src", color32BalanceException.ParamName);

                            ArgumentException nonContinuousFeaturesException = Assert.Throws<ArgumentException>(() =>
                                learning.ExtractSimpleFeatures(nonContinuous, dst));
                            Assert.Equal("src", nonContinuousFeaturesException.ParamName);

                            ArgumentException nonContinuousBalanceException = Assert.Throws<ArgumentException>(() =>
                                learning.BalanceWhite(nonContinuous, dst));
                            Assert.Equal("src", nonContinuousBalanceException.ParamName);
                        }

                        Assert.Throws<ArgumentNullException>(() => learning.ExtractSimpleFeatures(null!, dst));
                        Assert.Throws<ArgumentNullException>(() => learning.ExtractSimpleFeatures(src, null!));
                        Assert.Throws<ArgumentNullException>(() => learning.ExtractSimpleFeatures(null!));
                        Assert.Throws<ArgumentNullException>(() => learning.BalanceWhite(null!, dst));
                        Assert.Throws<ArgumentNullException>(() => learning.BalanceWhite(src, null!));
                        Assert.Throws<ArgumentNullException>(() => learning.BalanceWhite(null!));
                        Assert.Throws<ArgumentException>(() => learning.ExtractSimpleFeatures(gray, dst));
                        Assert.Throws<ArgumentException>(() => learning.ExtractSimpleFeatures(gray));
                        Assert.Throws<ArgumentException>(() => learning.BalanceWhite(gray, dst));
                        Assert.Throws<ArgumentException>(() => learning.BalanceWhite(gray));

                        learning.Dispose();
                        Assert.True(learning.IsDisposed);
                        Assert.Throws<ObjectDisposedException>(() => learning.ExtractSimpleFeatures(src, dst));
                        Assert.Throws<ObjectDisposedException>(() => learning.ExtractSimpleFeatures(src));
                        Assert.Throws<ObjectDisposedException>(() => learning.BalanceWhite(src, dst));
                        Assert.Throws<ObjectDisposedException>(() => learning.BalanceWhite(src));
                    }
                }
            }
        }

        [Fact]
        public void WhiteBalancerSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var src = new Mat(4, 4, MatType.CV_8UC3, new Scalar(12, 32, 64)))
            using (var dst = new Mat())
            using (var simple = XPhotoCv2.CreateSimpleWB())
            using (var grayworld = XPhotoCv2.CreateGrayworldWB())
            using (var learning = XPhotoCv2.CreateLearningBasedWB())
            {
                simple.InputMin = 0.0F;
                simple.InputMax = 255.0F;
                simple.OutputMin = 0.0F;
                simple.OutputMax = 255.0F;
                simple.P = 1.0F;
                simple.BalanceWhite(src, dst);

                grayworld.SaturationThreshold = 0.95F;
                grayworld.BalanceWhite(src, dst);

                learning.RangeMaxVal = 255;
                learning.HistBinNum = 64;
                learning.SaturationThreshold = 0.98F;

                Assert.False(simple.IsDisposed);
                Assert.False(grayworld.IsDisposed);
                Assert.False(learning.IsDisposed);
                Assert.Equal(1.0F, simple.P, 3);
                Assert.Equal(0.95F, grayworld.SaturationThreshold, 3);
                Assert.Equal(255, learning.RangeMaxVal);
                Assert.Equal(64, learning.HistBinNum);
            }
        }

        [Fact]
        public void WhiteBalancerPropertiesRoundTripAllSettingsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var simple = XPhotoCv2.CreateSimpleWB())
            using (var grayworld = XPhotoCv2.CreateGrayworldWB())
            using (var learning = XPhotoCv2.CreateLearningBasedWB())
            {
                simple.InputMin = 4.0F;
                simple.InputMax = 240.0F;
                simple.OutputMin = 8.0F;
                simple.OutputMax = 248.0F;
                simple.P = 1.5F;

                grayworld.SaturationThreshold = 0.82F;

                learning.RangeMaxVal = 240;
                learning.HistBinNum = 32;
                learning.SaturationThreshold = 0.87F;

                Assert.Equal(4.0F, simple.InputMin, 3);
                Assert.Equal(240.0F, simple.InputMax, 3);
                Assert.Equal(8.0F, simple.OutputMin, 3);
                Assert.Equal(248.0F, simple.OutputMax, 3);
                Assert.Equal(1.5F, simple.P, 3);

                Assert.Equal(0.82F, grayworld.SaturationThreshold, 3);

                Assert.Equal(240, learning.RangeMaxVal);
                Assert.Equal(32, learning.HistBinNum);
                Assert.Equal(0.87F, learning.SaturationThreshold, 3);
            }
        }

        [Fact]
        public void XPhotoFunctionSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var color = new Mat(8, 8, MatType.CV_8UC3, new Scalar(10, 20, 30)))
            using (var gray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(127)))
            using (var dst = new Mat())
            {
                XPhotoCv2.ApplyChannelGains(color, dst, 1.0F, 1.1F, 0.9F);
                Assert.False(dst.Empty);

                XPhotoCv2.DctDenoising(gray, dst, 1.0, 4);
                Assert.False(dst.Empty);
            }
        }

        private static SimpleWB? TryCreateSimpleWB()
        {
            try
            {
                return XPhotoCv2.CreateSimpleWB();
            }
            catch (OpenCvException ex) when (IsXPhotoModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static bool IsXPhotoNativeObjectAvailable()
        {
            using (SimpleWB? nativeBoundary = TryCreateSimpleWB())
            {
                return nativeBoundary != null;
            }
        }

        private static GrayworldWB? TryCreateGrayworldWB()
        {
            try
            {
                return XPhotoCv2.CreateGrayworldWB();
            }
            catch (OpenCvException ex) when (IsXPhotoModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static LearningBasedWB? TryCreateLearningBasedWB()
        {
            try
            {
                return XPhotoCv2.CreateLearningBasedWB();
            }
            catch (OpenCvException ex) when (IsXPhotoModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static bool IsXPhotoModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("xphoto", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("XPhoto", StringComparison.OrdinalIgnoreCase) >= 0;
        }

    }
}
