using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.XImgProc;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace OpenCvSharp.Tests.XImgProc
{
    public sealed class XImgProcTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvXImgProcConstants()
        {
            Assert.Equal(0, (int)ThinningTypes.ZhangSuen);
            Assert.Equal(1, (int)ThinningTypes.GuoHall);
            Assert.Equal(0, (int)LocalBinarizationMethods.NiBlack);
            Assert.Equal(1, (int)LocalBinarizationMethods.Sauvola);
            Assert.Equal(2, (int)LocalBinarizationMethods.Wolf);
            Assert.Equal(3, (int)LocalBinarizationMethods.Nick);
            Assert.Equal(1, (int)WeightedMedianFilterWeightType.Exp);
            Assert.Equal(32, (int)WeightedMedianFilterWeightType.Off);
            Assert.Equal(100, (int)SLICType.SLIC);
            Assert.Equal(101, (int)SLICType.SLICO);
            Assert.Equal(102, (int)SLICType.MSLIC);
            Assert.Equal(0, (int)DomainTransformFilterMode.NormalizedConvolution);
            Assert.Equal(1, (int)DomainTransformFilterMode.InterpolatedConvolution);
            Assert.Equal(2, (int)DomainTransformFilterMode.RecursiveFiltering);
            Assert.Equal(6, (int)AngleRangeOption.Aro315To135);
            Assert.Equal(2, (int)HoughOp.Add);
            Assert.Equal(1, (int)HoughDeskewOption.Deskew);
            Assert.Equal(1, (int)RulesOption.IgnoreBorders);
            Assert.Equal(0, (int)EdgeDrawingGradientOperator.Prewitt);
            Assert.Equal(1, (int)EdgeDrawingGradientOperator.Sobel);
            Assert.Equal(2, (int)EdgeDrawingGradientOperator.Scharr);
            Assert.Equal(3, (int)EdgeDrawingGradientOperator.Lsd);
        }

        [Fact]
        public void StaticFunctionsValidateManagedArguments()
        {
            using (var mat = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.NiBlackThreshold(null!, mat, 255.0, ThresholdTypes.Binary, 3, -0.2));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.NiBlackThreshold(mat, null!, 255.0, ThresholdTypes.Binary, 3, -0.2));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.NiBlackThreshold(mat, mat, 255.0, ThresholdTypes.Binary, 3, -0.2, (LocalBinarizationMethods)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.NiBlackThreshold(mat, 255.0, ThresholdTypes.Binary, 3, -0.2, (LocalBinarizationMethods)99));
                using (var niBlackEmpty = new Mat())
                using (var niBlackGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var niBlackColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var niBlackFloat = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.5)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.NiBlackThreshold(niBlackEmpty, mat, 255.0, ThresholdTypes.Binary, 3, -0.2));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.NiBlackThreshold(niBlackEmpty, 255.0, ThresholdTypes.Binary, 3, -0.2));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.NiBlackThreshold(niBlackColor, mat, 255.0, ThresholdTypes.Binary, 3, -0.2));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.NiBlackThreshold(niBlackColor, 255.0, ThresholdTypes.Binary, 3, -0.2));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.NiBlackThreshold(niBlackFloat, mat, 255.0, ThresholdTypes.Binary, 3, -0.2));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.NiBlackThreshold(niBlackFloat, 255.0, ThresholdTypes.Binary, 3, -0.2));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.NiBlackThreshold(niBlackGray, mat, 255.0, ThresholdTypes.Binary, 2, -0.2));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.NiBlackThreshold(niBlackGray, 255.0, ThresholdTypes.Binary, 1, -0.2));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.NiBlackThreshold(niBlackGray, mat, 255.0, ThresholdTypes.Mask, 3, -0.2));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.NiBlackThreshold(niBlackGray, 255.0, ThresholdTypes.Mask, 3, -0.2));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.NiBlackThreshold(niBlackGray, mat, 255.0, ThresholdTypes.Binary, 3, -0.2, LocalBinarizationMethods.Sauvola, 0.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.NiBlackThreshold(niBlackGray, 255.0, ThresholdTypes.Binary, 3, -0.2, LocalBinarizationMethods.Sauvola, 0.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.NiBlackThreshold(niBlackGray, niBlackGray, 255.0, ThresholdTypes.Binary, 3, -0.2));
                }
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.Thinning(null!, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.Thinning(mat, null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.Thinning(mat, mat, (ThinningTypes)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.Thinning(mat, (ThinningTypes)99));
                using (var thinningColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var thinningFloat = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.5)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.Thinning(mat, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.Thinning(mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.Thinning(thinningColor, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.Thinning(thinningColor));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.Thinning(thinningFloat, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.Thinning(thinningFloat));
                }
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.AnisotropicDiffusion(null!, mat, 0.1F, 10.0F, 1));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.AnisotropicDiffusion(mat, null!, 0.1F, 10.0F, 1));
                using (var anisotropicColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var anisotropicGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.AnisotropicDiffusion(anisotropicGray, mat, 0.1F, 10.0F, 1));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.AnisotropicDiffusion(anisotropicGray, 0.1F, 10.0F, 1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AnisotropicDiffusion(anisotropicColor, mat, 0.0F, 10.0F, 1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AnisotropicDiffusion(anisotropicColor, 0.0F, 10.0F, 1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AnisotropicDiffusion(anisotropicColor, mat, 0.1F, 0.0F, 1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AnisotropicDiffusion(anisotropicColor, 0.1F, 0.0F, 1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AnisotropicDiffusion(anisotropicColor, mat, 0.1F, 10.0F, -1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AnisotropicDiffusion(anisotropicColor, 0.1F, 10.0F, -1));
                }

                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.JointBilateralFilter(null!, mat, mat, 3, 10.0, 3.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.JointBilateralFilter(mat, null!, mat, 3, 10.0, 3.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.JointBilateralFilter(mat, mat, null!, 3, 10.0, 3.0));
                using (var jointBilateralEmpty = new Mat())
                using (var jointBilateralGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var jointBilateralColor = new Mat(8, 8, MatType.CV_32FC3, new Scalar(0.25, 0.5, 0.75)))
                using (var jointBilateralColorSource = new Mat(8, 8, MatType.CV_32FC3, new Scalar(0.75, 0.5, 0.25)))
                using (var jointBilateral16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var jointBilateralFourChannel = new Mat(8, 8, MatType.CV_8UC4, new Scalar(1, 2, 3, 4)))
                using (var jointBilateralSizeMismatch = new Mat(4, 4, MatType.CV_8UC1, new Scalar(48)))
                using (var jointBilateralDepthMismatch = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.5)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.JointBilateralFilter(jointBilateralGray, jointBilateralEmpty, mat, 3, 10.0, 3.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.JointBilateralFilter(jointBilateralGray, jointBilateralEmpty, 3, 10.0, 3.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.JointBilateralFilter(jointBilateralGray, jointBilateral16U, mat, 3, 10.0, 3.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.JointBilateralFilter(jointBilateralGray, jointBilateral16U, 3, 10.0, 3.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.JointBilateralFilter(jointBilateralFourChannel, jointBilateralGray, mat, 3, 10.0, 3.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.JointBilateralFilter(jointBilateralFourChannel, jointBilateralGray, 3, 10.0, 3.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.JointBilateralFilter(jointBilateralSizeMismatch, jointBilateralGray, mat, 3, 10.0, 3.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.JointBilateralFilter(jointBilateralSizeMismatch, jointBilateralGray, 3, 10.0, 3.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.JointBilateralFilter(jointBilateralDepthMismatch, jointBilateralGray, mat, 3, 10.0, 3.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.JointBilateralFilter(jointBilateralDepthMismatch, jointBilateralGray, 3, 10.0, 3.0));
                    if (!IsXImgProcModuleLinked())
                    {
                        return;
                    }

                    using (Mat jointBilateralFallback = XImgProcCv2.JointBilateralFilter(jointBilateralEmpty, jointBilateralGray, 3, 10.0, 3.0))
                    using (Mat jointBilateralSelf = XImgProcCv2.JointBilateralFilter(jointBilateralGray, jointBilateralGray, 3, 10.0, 3.0))
                    using (Mat jointBilateralFiltered = XImgProcCv2.JointBilateralFilter(jointBilateralColor, jointBilateralColorSource, -1, 0.0, -1.0))
                    {
                        Assert.Equal(jointBilateralGray.Rows, jointBilateralFallback.Rows);
                        Assert.Equal(jointBilateralGray.Cols, jointBilateralFallback.Cols);
                        Assert.Equal(jointBilateralGray.Rows, jointBilateralSelf.Rows);
                        Assert.Equal(jointBilateralGray.Cols, jointBilateralSelf.Cols);
                        Assert.Equal(jointBilateralColorSource.Rows, jointBilateralFiltered.Rows);
                        Assert.Equal(jointBilateralColorSource.Cols, jointBilateralFiltered.Cols);
                    }
                }

                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GuidedFilter(null!, mat, mat, 2, 1.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GuidedFilter(mat, null!, mat, 2, 1.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GuidedFilter(mat, mat, null!, 2, 1.0));
                using (var guidedEmpty = new Mat())
                using (var guidedGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var guidedColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var guidedGuide16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var guidedGuide32F = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.25)))
                using (var guidedGuide64F = new Mat(8, 8, MatType.CV_64FC1, new Scalar(0.25)))
                using (var guidedGuideFourChannel = new Mat(8, 8, MatType.CV_8UC4, new Scalar(1, 2, 3, 4)))
                using (var guidedSrc32F = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.25)))
                using (var guidedSrc16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var guidedSizeMismatch = new Mat(4, 4, MatType.CV_8UC1, new Scalar(48)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GuidedFilter(guidedEmpty, guidedGray, mat, 2, 1.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GuidedFilter(guidedGuide64F, guidedGray, mat, 2, 1.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GuidedFilter(guidedGuideFourChannel, guidedGray, mat, 2, 1.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.GuidedFilter(guidedGray, guidedGray, mat, -1, 1.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.GuidedFilter(guidedGray, guidedGray, mat, 2, -1.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.GuidedFilter(guidedGray, guidedGray, mat, 2, 1.0, scale: 1.5));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GuidedFilter(guidedGray, guidedEmpty, mat, 2, 1.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GuidedFilter(guidedGray, guidedSrc16U, mat, 2, 1.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GuidedFilter(guidedGray, guidedSizeMismatch, mat, 2, 1.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CreateGuidedFilter(guidedEmpty, 2, 1.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CreateGuidedFilter(guidedGuide64F, 2, 1.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CreateGuidedFilter(guidedGuideFourChannel, 2, 1.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.CreateGuidedFilter(guidedGray, -1, 1.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.CreateGuidedFilter(guidedGray, 2, -1.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.CreateGuidedFilter(guidedGray, 2, 1.0, scale: 1.5));
                    using (GuidedFilter guidedReusable = XImgProcCv2.CreateGuidedFilter(guidedGray, 2, 1.0))
                    using (Mat guidedGrayFiltered = XImgProcCv2.GuidedFilter(guidedGray, guidedGray, 2, 1.0))
                    using (Mat guidedColorFiltered = XImgProcCv2.GuidedFilter(guidedColor, guidedSrc32F, 2, 1.0))
                    using (Mat guided16UFiltered = XImgProcCv2.GuidedFilter(guidedGuide16U, guidedGray, 2, 1.0))
                    using (Mat guided32FGuideFiltered = XImgProcCv2.GuidedFilter(guidedGuide32F, guidedSrc32F, 2, 1.0))
                    using (Mat guidedReusableFiltered = guidedReusable.Filter(guidedGray))
                    {
                        Assert.Throws<ArgumentException>(() => guidedReusable.Filter(guidedSizeMismatch));
                        Assert.Equal(guidedGray.Rows, guidedGrayFiltered.Rows);
                        Assert.Equal(guidedSrc32F.Cols, guidedColorFiltered.Cols);
                        Assert.Equal(guidedGuide16U.Rows, guided16UFiltered.Rows);
                        Assert.Equal(guidedGuide32F.Cols, guided32FGuideFiltered.Cols);
                        Assert.Equal(guidedGray.Rows, guidedReusableFiltered.Rows);
                    }
                }
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.RollingGuidanceFilter(null!, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.RollingGuidanceFilter(mat, null!));
                using (var rollingEmpty = new Mat())
                using (var rollingGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var rollingColor = new Mat(8, 8, MatType.CV_32FC3, new Scalar(0.25, 0.5, 0.75)))
                using (var rolling16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var rollingFourChannel = new Mat(8, 8, MatType.CV_8UC4, new Scalar(1, 2, 3, 4)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.RollingGuidanceFilter(rollingEmpty, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.RollingGuidanceFilter(rollingEmpty));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.RollingGuidanceFilter(rolling16U, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.RollingGuidanceFilter(rolling16U));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.RollingGuidanceFilter(rollingFourChannel, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.RollingGuidanceFilter(rollingFourChannel));
                    using (Mat rollingFilteredGray = XImgProcCv2.RollingGuidanceFilter(rollingGray, sigmaColor: 0.0, sigmaSpace: -1.0))
                    using (Mat rollingFilteredColor = XImgProcCv2.RollingGuidanceFilter(rollingColor, sigmaColor: 0.0, sigmaSpace: -1.0))
                    {
                        Assert.Equal(rollingGray.Rows, rollingFilteredGray.Rows);
                        Assert.Equal(rollingGray.Cols, rollingFilteredGray.Cols);
                        Assert.Equal(rollingColor.Rows, rollingFilteredColor.Rows);
                        Assert.Equal(rollingColor.Cols, rollingFilteredColor.Cols);
                    }
                }

                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.WeightedMedianFilter(null!, mat, mat, 1));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.WeightedMedianFilter(mat, null!, mat, 1));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.WeightedMedianFilter(mat, mat, null!, 1));
                using (var weightedEmpty = new Mat())
                using (var weightedGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var weightedFloat = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.25)))
                using (var weighted16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var weightedJointGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(48)))
                using (var weightedJointColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var weightedJointFloat = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.5)))
                using (var weightedJointFourChannel = new Mat(8, 8, MatType.CV_8UC4, new Scalar(1, 2, 3, 4)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weightedEmpty, mat, 1));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weightedEmpty, 1));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weighted16U, mat, 1));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weighted16U, 1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weightedGray, mat, 0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weightedGray, 0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weightedGray, mat, 1, 0.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weightedGray, 1, 0.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weightedGray, mat, 1, weightType: (WeightedMedianFilterWeightType)99));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weightedGray, 1, weightType: (WeightedMedianFilterWeightType)99));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weightedGray, mat, 1, weightType: WeightedMedianFilterWeightType.Exp | WeightedMedianFilterWeightType.Off));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointGray, weightedGray, 1, weightType: WeightedMedianFilterWeightType.Exp | WeightedMedianFilterWeightType.Off));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointFloat, weightedGray, mat, 1));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointFloat, weightedGray, 1));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointFourChannel, weightedGray, mat, 1));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.WeightedMedianFilter(weightedJointFourChannel, weightedGray, 1));
                    using (Mat weightedFallback = XImgProcCv2.WeightedMedianFilter(weightedEmpty, weightedGray, 1))
                    using (Mat weightedFiltered = XImgProcCv2.WeightedMedianFilter(weightedJointColor, weightedFloat, 1))
                    {
                        Assert.Equal(weightedGray.Rows, weightedFallback.Rows);
                        Assert.Equal(weightedGray.Cols, weightedFallback.Cols);
                        Assert.Equal(weightedFloat.Rows, weightedFiltered.Rows);
                        Assert.Equal(weightedFloat.Cols, weightedFiltered.Cols);
                    }
                }

                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.DtFilter(null!, mat, mat, 4.0, 4.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.DtFilter(mat, null!, mat, 4.0, 4.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.DtFilter(mat, mat, null!, 4.0, 4.0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.DtFilter(mat, mat, mat, 4.0, 4.0, (DomainTransformFilterMode)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.DtFilter(mat, mat, 4.0, 4.0, (DomainTransformFilterMode)99));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.AmFilter(null!, mat, mat, 4.0, 0.2));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.AmFilter(mat, null!, mat, 4.0, 0.2));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.AmFilter(mat, mat, null!, 4.0, 0.2));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AmFilter(mat, mat, mat, 0.5, 0.2));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AmFilter(mat, mat, 0.5, 0.2));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AmFilter(mat, mat, mat, 4.0, 0.0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AmFilter(mat, mat, 4.0, 0.0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AmFilter(mat, mat, mat, 4.0, 1.5));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.AmFilter(mat, mat, 4.0, 1.5));
                using (var amEmpty = new Mat())
                using (var amGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var amJoint64F = new Mat(8, 8, MatType.CV_64FC1, new Scalar(0.25)))
                using (var amSizeMismatch = new Mat(4, 4, MatType.CV_8UC1, new Scalar(48)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.AmFilter(amEmpty, amGray, mat, 4.0, 0.5));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.AmFilter(amEmpty, amGray, 4.0, 0.5));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.AmFilter(amGray, amEmpty, mat, 4.0, 0.5));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.AmFilter(amGray, amEmpty, 4.0, 0.5));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.AmFilter(amSizeMismatch, amGray, mat, 4.0, 0.5));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.AmFilter(amSizeMismatch, amGray, 4.0, 0.5));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.AmFilter(amJoint64F, amGray, mat, 4.0, 0.5));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.AmFilter(amJoint64F, amGray, 4.0, 0.5));
                }
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.BilateralTextureFilter(null!, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.BilateralTextureFilter(mat, null!));
                using (var bilateralEmpty = new Mat())
                using (var bilateral8U = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var bilateral32F = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.5)))
                using (var bilateral16S = new Mat(8, 8, MatType.CV_16SC1, new Scalar(1)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.BilateralTextureFilter(bilateralEmpty, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.BilateralTextureFilter(bilateralEmpty));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.BilateralTextureFilter(bilateral16S, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.BilateralTextureFilter(bilateral16S));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.BilateralTextureFilter(bilateral8U, mat, 0, 1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.BilateralTextureFilter(bilateral8U, 0, 1));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.BilateralTextureFilter(bilateral32F, mat, 3, 0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.BilateralTextureFilter(bilateral32F, 3, 0));
                }

                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.EdgePreservingFilter(null!, mat, 3, 10.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.EdgePreservingFilter(mat, null!, 3, 10.0));
                using (var edgePreservingColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var edgePreservingGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.EdgePreservingFilter(edgePreservingGray, mat, 3, 10.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.EdgePreservingFilter(edgePreservingGray, 3, 10.0));
                    using (Mat edgePreserved = XImgProcCv2.EdgePreservingFilter(edgePreservingColor, 2, -1.0))
                    {
                        Assert.Equal(edgePreservingColor.Rows, edgePreserved.Rows);
                        Assert.Equal(edgePreservingColor.Cols, edgePreserved.Cols);
                    }
                }

                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FastGlobalSmootherFilter(null!, mat, mat, 8.0, 12.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FastGlobalSmootherFilter(mat, null!, mat, 8.0, 12.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FastGlobalSmootherFilter(mat, mat, null!, 8.0, 12.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.CreateFastGlobalSmootherFilter(null!, 8.0, 12.0));
                using (var fgsEmpty = new Mat())
                using (var fgsGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var fgsColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var fgsGuide16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var fgsGuideFourChannel = new Mat(8, 8, MatType.CV_8UC4, new Scalar(1, 2, 3, 4)))
                using (var fgsSrc16S = new Mat(8, 8, MatType.CV_16SC1, new Scalar(1)))
                using (var fgsSrc32F = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.25)))
                using (var fgsSrc64F = new Mat(8, 8, MatType.CV_64FC1, new Scalar(0.25)))
                using (var fgsSrcFiveChannel = new Mat(8, 8, MatType.MakeType(MatType.CV_8U, 5)))
                using (var fgsSizeMismatch = new Mat(4, 4, MatType.CV_8UC1, new Scalar(48)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastGlobalSmootherFilter(fgsEmpty, fgsGray, mat, 8.0, 12.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastGlobalSmootherFilter(fgsGuide16U, fgsGray, mat, 8.0, 12.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastGlobalSmootherFilter(fgsGuideFourChannel, fgsGray, mat, 8.0, 12.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FastGlobalSmootherFilter(fgsGray, fgsGray, mat, -1.0, 12.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FastGlobalSmootherFilter(fgsGray, fgsGray, mat, 8.0, -1.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FastGlobalSmootherFilter(fgsGray, fgsGray, mat, 8.0, 12.0, numIter: 0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastGlobalSmootherFilter(fgsGray, fgsEmpty, mat, 8.0, 12.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastGlobalSmootherFilter(fgsGray, fgsSrc64F, mat, 8.0, 12.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastGlobalSmootherFilter(fgsGray, fgsSrcFiveChannel, mat, 8.0, 12.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastGlobalSmootherFilter(fgsGray, fgsSizeMismatch, mat, 8.0, 12.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CreateFastGlobalSmootherFilter(fgsEmpty, 8.0, 12.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CreateFastGlobalSmootherFilter(fgsGuide16U, 8.0, 12.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CreateFastGlobalSmootherFilter(fgsGuideFourChannel, 8.0, 12.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.CreateFastGlobalSmootherFilter(fgsGray, -1.0, 12.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.CreateFastGlobalSmootherFilter(fgsGray, 8.0, -1.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.CreateFastGlobalSmootherFilter(fgsGray, 8.0, 12.0, numIter: 0));
                    using (FastGlobalSmootherFilter fgsReusable = XImgProcCv2.CreateFastGlobalSmootherFilter(fgsGray, 8.0, 12.0))
                    using (Mat fgsGrayFiltered = XImgProcCv2.FastGlobalSmootherFilter(fgsGray, fgsGray, 8.0, 12.0))
                    using (Mat fgsColorFiltered = XImgProcCv2.FastGlobalSmootherFilter(fgsColor, fgsSrc16S, 8.0, 12.0))
                    using (Mat fgsFloatFiltered = XImgProcCv2.FastGlobalSmootherFilter(fgsGray, fgsSrc32F, 8.0, 12.0))
                    using (Mat fgsReusableFiltered = fgsReusable.Filter(fgsGray))
                    {
                        Assert.Throws<ArgumentException>(() => fgsReusable.Filter(fgsSizeMismatch));
                        Assert.Equal(fgsGray.Rows, fgsGrayFiltered.Rows);
                        Assert.Equal(fgsSrc16S.Cols, fgsColorFiltered.Cols);
                        Assert.Equal(fgsSrc32F.Rows, fgsFloatFiltered.Rows);
                        Assert.Equal(fgsGray.Cols, fgsReusableFiltered.Cols);
                    }
                }
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.L0Smooth(null!, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.L0Smooth(mat, null!));
                using (var l0Empty = new Mat())
                using (var l0U8 = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var l0U16 = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var l0F32 = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.25)))
                using (var l0F64 = new Mat(8, 8, MatType.CV_64FC1, new Scalar(0.25)))
                using (var l0S16 = new Mat(8, 8, MatType.CV_16SC1, new Scalar(1)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.L0Smooth(l0Empty, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.L0Smooth(l0Empty));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.L0Smooth(l0S16, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.L0Smooth(l0S16));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.L0Smooth(l0U8, mat, 0.0, 2.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.L0Smooth(l0U8, 0.0, 2.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.L0Smooth(l0U8, mat, 0.02, 1.0));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.L0Smooth(l0U8, 0.02, 1.0));
                    using (Mat l0U8Smoothed = XImgProcCv2.L0Smooth(l0U8))
                    using (Mat l0U16Smoothed = XImgProcCv2.L0Smooth(l0U16))
                    using (Mat l0F32Smoothed = XImgProcCv2.L0Smooth(l0F32))
                    using (Mat l0F64Smoothed = XImgProcCv2.L0Smooth(l0F64))
                    {
                        Assert.Equal(l0U8.Rows, l0U8Smoothed.Rows);
                        Assert.Equal(l0U8.Cols, l0U8Smoothed.Cols);
                        Assert.Equal(l0U16.Rows, l0U16Smoothed.Rows);
                        Assert.Equal(l0U16.Cols, l0U16Smoothed.Cols);
                        Assert.Equal(l0F32.Rows, l0F32Smoothed.Rows);
                        Assert.Equal(l0F32.Cols, l0F32Smoothed.Cols);
                        Assert.Equal(l0F64.Rows, l0F64Smoothed.Rows);
                        Assert.Equal(l0F64.Cols, l0F64Smoothed.Cols);
                    }
                }

                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FastHoughTransform(null!, mat, MatType.CV_32S));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FastHoughTransform(mat, null!, MatType.CV_32S));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FastHoughTransform(mat, mat, MatType.CV_32S, (AngleRangeOption)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FastHoughTransform(mat, mat, MatType.CV_32S, op: (HoughOp)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FastHoughTransform(mat, mat, MatType.CV_32S, makeSkew: (HoughDeskewOption)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FastHoughTransform(mat, MatType.CV_32S, (AngleRangeOption)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FastHoughTransform(mat, MatType.CV_32S, op: (HoughOp)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FastHoughTransform(mat, MatType.CV_32S, makeSkew: (HoughDeskewOption)99));
                Assert.Throws<ArgumentException>(() => XImgProcCv2.FastHoughTransform(mat, mat, MatType.CV_32S));
                Assert.Throws<ArgumentException>(() => XImgProcCv2.FastHoughTransform(mat, MatType.CV_32S));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.HoughPointToLine(0, 0, null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.HoughPointToLine(0, 0, mat, (AngleRangeOption)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.HoughPointToLine(0, 0, mat, makeSkew: (HoughDeskewOption)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.HoughPointToLine(0, 0, mat, rules: (RulesOption)99));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.PeiLinNormalization(null!, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.PeiLinNormalization(mat, null!));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.CreateGuidedFilter(null!, 2, 1.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.CreateFastGlobalSmootherFilter(null!, 8.0, 12.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.CreateSuperpixelSLIC(null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.CreateSuperpixelSLIC(mat, (SLICType)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => SuperpixelSLIC.Create(mat, (SLICType)99));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.CreateSuperpixelLSC(null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.CreateRidgeDetectionFilter(ksize: 2));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.CreateRidgeDetectionFilter(ddepth: MatType.CV_8UC1));
                Assert.Throws<ArgumentOutOfRangeException>(() => RidgeDetectionFilter.Create(ksize: 9));
                Assert.Throws<ArgumentOutOfRangeException>(() => RidgeDetectionFilter.Create(ddepth: MatType.CV_32FC2));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GetDisparityVis(null!, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GetDisparityVis(mat, null!));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.ComputeMSE(null!, mat, new Rect()));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.ComputeMSE(mat, null!, new Rect()));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.ComputeBadPixelPercent(null!, mat, new Rect()));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.ComputeBadPixelPercent(mat, null!, new Rect()));
                using (var disparityEmpty = new Mat())
                using (var disparity16S = new Mat(8, 8, MatType.CV_16SC1, new Scalar(16)))
                using (var disparity32F = new Mat(8, 8, MatType.CV_32FC1, new Scalar(1.0)))
                using (var disparity8U = new Mat(8, 8, MatType.CV_8UC1, new Scalar(1)))
                using (var disparityColor = new Mat(8, 8, MatType.CV_16SC3, new Scalar(16, 16, 16)))
                using (var disparitySizeMismatch = new Mat(4, 4, MatType.CV_16SC1, new Scalar(16)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GetDisparityVis(disparityEmpty, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GetDisparityVis(disparityEmpty));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GetDisparityVis(disparity8U, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GetDisparityVis(disparity8U));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GetDisparityVis(disparityColor, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GetDisparityVis(disparityColor));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeMSE(disparityEmpty, disparity16S, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeMSE(disparity16S, disparityEmpty, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeMSE(disparity8U, disparity16S, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeMSE(disparity16S, disparity8U, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeMSE(disparityColor, disparity16S, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeMSE(disparity16S, disparityColor, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeMSE(disparity16S, disparitySizeMismatch, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeBadPixelPercent(disparityEmpty, disparity16S, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeBadPixelPercent(disparity16S, disparityEmpty, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeBadPixelPercent(disparity8U, disparity16S, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeBadPixelPercent(disparity16S, disparity8U, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeBadPixelPercent(disparityColor, disparity16S, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeBadPixelPercent(disparity16S, disparityColor, new Rect(0, 0, 8, 8)));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.ComputeBadPixelPercent(disparity16S, disparitySizeMismatch, new Rect(0, 0, 8, 8)));
                    using (Mat vis16S = XImgProcCv2.GetDisparityVis(disparity16S))
                    using (Mat vis32F = XImgProcCv2.GetDisparityVis(disparity32F))
                    {
                        Assert.Equal(disparity16S.Rows, vis16S.Rows);
                        Assert.Equal(disparity32F.Cols, vis32F.Cols);
                        Assert.Equal(0.0, XImgProcCv2.ComputeMSE(disparity16S, disparity16S, new Rect(0, 0, 8, 8)));
                        Assert.Equal(0.0, XImgProcCv2.ComputeMSE(disparity32F, disparity32F, new Rect(0, 0, 8, 8)));
                        Assert.Equal(0.0, XImgProcCv2.ComputeBadPixelPercent(disparity16S, disparity16S, new Rect(0, 0, 8, 8)));
                        Assert.Equal(0.0, XImgProcCv2.ComputeBadPixelPercent(disparity32F, disparity32F, new Rect(0, 0, 8, 8)));
                    }
                }
                using (var wlsLeftEmpty = new Mat())
                using (var wlsLeft = new Mat(8, 8, MatType.CV_16SC1, new Scalar(0)))
                using (var wlsLeftColor = new Mat(8, 8, MatType.CV_16SC3, new Scalar(0, 0, 0)))
                using (var wlsLeftView = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var wlsLeftViewColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var wlsLeftView16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var wlsLeftViewFourChannel = new Mat(8, 8, MatType.CV_8UC4, new Scalar(1, 2, 3, 4)))
                using (var wlsRightEmpty = new Mat())
                using (var wlsRight = new Mat(8, 8, MatType.CV_16SC1, new Scalar(0)))
                using (var wlsRightColor = new Mat(8, 8, MatType.CV_16SC3, new Scalar(0, 0, 0)))
                using (var wlsRightSizeMismatch = new Mat(4, 4, MatType.CV_16SC1, new Scalar(0)))
                using (var wlsFiltered = new Mat())
                using (var wlsNoConfidence = DisparityWLSFilter.CreateGeneric(useConfidence: false))
                using (var wlsConfidence = DisparityWLSFilter.CreateGeneric(useConfidence: true))
                {
                    Assert.Throws<ArgumentException>(() => wlsNoConfidence.Filter(wlsLeftEmpty, wlsLeftView, filteredDisparityMap: wlsFiltered));
                    Assert.Throws<ArgumentException>(() => wlsNoConfidence.Filter(wlsLeftColor, wlsLeftView, filteredDisparityMap: wlsFiltered));
                    Assert.Throws<ArgumentException>(() => wlsNoConfidence.Filter(wlsLeft, wlsLeftEmpty, filteredDisparityMap: wlsFiltered));
                    Assert.Throws<ArgumentException>(() => wlsNoConfidence.Filter(wlsLeft, wlsLeftView16U, filteredDisparityMap: wlsFiltered));
                    Assert.Throws<ArgumentException>(() => wlsNoConfidence.Filter(wlsLeft, wlsLeftViewFourChannel, filteredDisparityMap: wlsFiltered));
                    Assert.Throws<ArgumentException>(() => wlsConfidence.Filter(wlsLeft, wlsLeftView, filteredDisparityMap: wlsFiltered, disparityMapRight: wlsRightEmpty));
                    Assert.Throws<ArgumentException>(() => wlsConfidence.Filter(wlsLeft, wlsLeftView, filteredDisparityMap: wlsFiltered, disparityMapRight: wlsRightColor));
                    Assert.Throws<ArgumentException>(() => wlsConfidence.Filter(wlsLeft, wlsLeftView, filteredDisparityMap: wlsFiltered, disparityMapRight: wlsRightSizeMismatch));
                    using (Mat filteredGray = wlsNoConfidence.Filter(wlsLeft, wlsLeftView))
                    using (Mat filteredColor = wlsNoConfidence.Filter(wlsLeft, wlsLeftViewColor))
                    using (Mat filteredConfidence = wlsConfidence.Filter(wlsLeft, wlsLeftView, disparityMapRight: wlsRight))
                    {
                        Assert.Equal(wlsLeftView.Rows, filteredGray.Rows);
                        Assert.Equal(wlsLeftViewColor.Cols, filteredColor.Cols);
                        Assert.Equal(wlsLeftView.Rows, filteredConfidence.Rows);
                    }
                }
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FastBilateralSolverFilter(null!, mat, mat, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FastBilateralSolverFilter(mat, null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FastBilateralSolverFilter(mat, mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FastBilateralSolverFilter(mat, mat, mat, null!));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.CreateFastBilateralSolverFilter(null!, 4.0, 4.0, 4.0));
                using (var sparseEmpty = new Mat())
                using (var sparseGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var sparseColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var sparse16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var sparseFourChannel = new Mat(8, 8, MatType.CV_8UC4, new Scalar(1, 2, 3, 4)))
                using (var sparsePoints = Calib3DCv2.ToPointMat(CreateFromPoints()))
                using (var sparseToPoints = Calib3DCv2.ToPointMat(CreateToPoints()))
                using (var sparseShortPoints = new Mat(1, 1, MatType.CV_32FC2, new Scalar(0)))
                using (var sparseBadDepthPoints = new Mat(4, 1, MatType.CV_64FC2, new Scalar(0)))
                using (var sparseBadShapePoints = new Mat(4, 3, MatType.CV_32FC1, new Scalar(0)))
                using (var sparseTooManyPoints = new Mat(short.MaxValue, 1, MatType.CV_32FC2, new Scalar(0)))
                using (var sparseCostMap8U = new Mat(8, 8, MatType.CV_8UC1, new Scalar(1)))
                using (var sparseCostMap32FColor = new Mat(8, 8, MatType.CV_32FC3, new Scalar(1.0, 1.0, 1.0)))
                using (var sparseFlow = new Mat())
                using (var edgeAware = EdgeAwareInterpolator.Create())
                using (var ric = RICInterpolator.Create())
                {
                    Assert.Throws<ArgumentException>(() => edgeAware.Interpolate(sparseEmpty, sparsePoints, sparseColor, sparseToPoints, sparseFlow));
                    Assert.Throws<ArgumentException>(() => edgeAware.Interpolate(sparse16U, sparsePoints, sparseColor, sparseToPoints, sparseFlow));
                    Assert.Throws<ArgumentException>(() => edgeAware.Interpolate(sparseFourChannel, sparsePoints, sparseColor, sparseToPoints, sparseFlow));
                    Assert.Throws<ArgumentException>(() => edgeAware.Interpolate(sparseGray, sparseEmpty, sparseColor, sparseToPoints, sparseFlow));
                    Assert.Throws<ArgumentException>(() => edgeAware.Interpolate(sparseGray, sparseBadDepthPoints, sparseColor, sparseToPoints, sparseFlow));
                    Assert.Throws<ArgumentException>(() => edgeAware.Interpolate(sparseGray, sparseBadShapePoints, sparseColor, sparseToPoints, sparseFlow));
                    Assert.Throws<ArgumentException>(() => edgeAware.Interpolate(sparseGray, sparsePoints, sparseColor, sparseShortPoints, sparseFlow));
                    Assert.Throws<ArgumentOutOfRangeException>(() => edgeAware.Interpolate(sparseGray, sparseTooManyPoints, sparseColor, sparseTooManyPoints, sparseFlow));
                    Assert.Throws<ArgumentException>(() => edgeAware.SetCostMap(sparseCostMap8U));
                    Assert.Throws<ArgumentException>(() => edgeAware.SetCostMap(sparseCostMap32FColor));

                    ric.UseVariationalRefinement = false;
                    Assert.Throws<ArgumentException>(() => ric.Interpolate(sparseEmpty, sparsePoints, sparseColor, sparseToPoints, sparseFlow));
                    Assert.Throws<ArgumentException>(() => ric.Interpolate(sparseGray, sparseBadDepthPoints, sparseColor, sparseToPoints, sparseFlow));
                    Assert.Throws<ArgumentException>(() => ric.SetCostMap(sparseCostMap8U));
                    Assert.Throws<ArgumentException>(() => ric.SetCostMap(sparseCostMap32FColor));
                    ric.UseVariationalRefinement = true;
                    Assert.Throws<ArgumentException>(() => ric.Interpolate(sparseGray, sparsePoints, sparseEmpty, sparseToPoints, sparseFlow));
                    Assert.Throws<ArgumentException>(() => ric.Interpolate(sparseGray, sparsePoints, sparse16U, sparseToPoints, sparseFlow));
                }
                using (var fbsEmpty = new Mat())
                using (var fbsGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var fbsColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var fbsGuide16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var fbsGuideFourChannel = new Mat(8, 8, MatType.CV_8UC4, new Scalar(1, 2, 3, 4)))
                using (var fbsSource16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var fbsSource16S = new Mat(8, 8, MatType.CV_16SC1, new Scalar(1)))
                using (var fbsSource32F = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.25)))
                using (var fbsSource64F = new Mat(8, 8, MatType.CV_64FC1, new Scalar(0.25)))
                using (var fbsSourceFiveChannel = new Mat(8, 8, MatType.MakeType(MatType.CV_8U, 5)))
                using (var fbsConfidence32F = new Mat(8, 8, MatType.CV_32FC1, new Scalar(1.0)))
                using (var fbsConfidence8U = new Mat(8, 8, MatType.CV_8UC1, new Scalar(255)))
                using (var fbsConfidence16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var fbsConfidenceColor = new Mat(8, 8, MatType.CV_32FC3, new Scalar(1.0, 1.0, 1.0)))
                using (var fbsSizeMismatch = new Mat(4, 4, MatType.CV_8UC1, new Scalar(48)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsEmpty, fbsSource32F, fbsConfidence32F, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsGuide16U, fbsSource32F, fbsConfidence32F, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsGuideFourChannel, fbsSource32F, fbsConfidence32F, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsGray, fbsEmpty, fbsConfidence32F, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsGray, fbsSource64F, fbsConfidence32F, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsGray, fbsSourceFiveChannel, fbsConfidence32F, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsGray, fbsSizeMismatch, fbsConfidence32F, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsGray, fbsSource32F, fbsEmpty, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsGray, fbsSource32F, fbsConfidence16U, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsGray, fbsSource32F, fbsConfidenceColor, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.FastBilateralSolverFilter(fbsGray, fbsSource32F, fbsSizeMismatch, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CreateFastBilateralSolverFilter(fbsEmpty, 4.0, 4.0, 4.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CreateFastBilateralSolverFilter(fbsGuide16U, 4.0, 4.0, 4.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CreateFastBilateralSolverFilter(fbsGuideFourChannel, 4.0, 4.0, 4.0));
                }
                using (var ridgeEmpty = new Mat())
                using (var ridgeGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var ridgeColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var ridgeFourChannel = new Mat(8, 8, MatType.CV_8UC4, new Scalar(1, 2, 3, 4)))
                using (var ridgeDefault = RidgeDetectionFilter.Create())
                using (var ridge64 = RidgeDetectionFilter.Create(ddepth: MatType.CV_64FC1, ksize: 5))
                using (var ridgeDst = new Mat())
                {
                    Assert.Throws<ArgumentException>(() => ridgeDefault.GetRidgeFilteredImage(ridgeEmpty, ridgeDst));
                    Assert.Throws<ArgumentException>(() => ridgeDefault.GetRidgeFilteredImage(ridgeFourChannel, ridgeDst));
                    using (Mat ridgeGrayResult = ridgeDefault.GetRidgeFilteredImage(ridgeGray))
                    using (Mat ridgeColorResult = ridge64.GetRidgeFilteredImage(ridgeColor))
                    {
                        Assert.Equal(ridgeGray.Rows, ridgeGrayResult.Rows);
                        Assert.Equal(ridgeColor.Cols, ridgeColorResult.Cols);
                    }
                }
                using (var edgeDrawingEmpty = new Mat())
                using (var edgeDrawingGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var edgeDrawingColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(24, 48, 72)))
                using (var edgeDrawingAlpha = new Mat(8, 8, MatType.CV_8UC4, new Scalar(24, 48, 72, 255)))
                using (var edgeDrawing16U = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var edgeDrawingTwoChannel = new Mat(8, 8, MatType.CV_8UC2, new Scalar(1)))
                using (var edgeDrawing = EdgeDrawing.Create())
                {
                    Assert.Throws<ArgumentException>(() => edgeDrawing.DetectEdges(edgeDrawingEmpty));
                    Assert.Throws<ArgumentException>(() => edgeDrawing.DetectEdges(edgeDrawing16U));
                    Assert.Throws<ArgumentException>(() => edgeDrawing.DetectEdges(edgeDrawingTwoChannel));
                    edgeDrawing.DetectEdges(edgeDrawingGray);
                    edgeDrawing.DetectEdges(edgeDrawingColor);
                    edgeDrawing.DetectEdges(edgeDrawingAlpha);
                    using (Mat edgeImage = edgeDrawing.GetEdgeImage())
                    {
                        Assert.Equal(edgeDrawingAlpha.Rows, edgeImage.Rows);
                        Assert.Equal(edgeDrawingAlpha.Cols, edgeImage.Cols);
                    }
                }
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GradientDericheX(null!, mat, 0.5, 0.001));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GradientDericheX(mat, null!, 0.5, 0.001));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GradientDericheY(null!, mat, 0.5, 0.001));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GradientDericheY(mat, null!, 0.5, 0.001));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GradientPaillouX(null!, mat, 1.0, 1.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GradientPaillouX(mat, null!, 1.0, 1.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GradientPaillouY(null!, mat, 1.0, 1.0));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.GradientPaillouY(mat, null!, 1.0, 1.0));
                using (var gradientEmpty = new Mat())
                using (var gradientU8 = new Mat(8, 8, MatType.CV_8UC1, new Scalar(96)))
                using (var gradientS8 = new Mat(8, 8, MatType.CV_8SC1, new Scalar(1)))
                using (var gradientU16 = new Mat(8, 8, MatType.CV_16UC1, new Scalar(1)))
                using (var gradientS16 = new Mat(8, 8, MatType.CV_16SC1, new Scalar(1)))
                using (var gradientF32 = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.25)))
                using (var gradientF64 = new Mat(8, 8, MatType.CV_64FC1, new Scalar(0.25)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GradientDericheX(gradientEmpty, mat, 0.5, 0.001));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GradientDericheX(gradientEmpty, 0.5, 0.001));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GradientDericheY(gradientF64, mat, 0.5, 0.001));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GradientDericheY(gradientF64, 0.5, 0.001));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GradientPaillouX(gradientEmpty, mat, 1.0, 1.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GradientPaillouX(gradientEmpty, 1.0, 1.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GradientPaillouY(gradientF64, mat, 1.0, 1.0));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.GradientPaillouY(gradientF64, 1.0, 1.0));
                    using (Mat dericheU8 = XImgProcCv2.GradientDericheX(gradientU8, 0.5, 0.001))
                    using (Mat dericheS8 = XImgProcCv2.GradientDericheY(gradientS8, 0.5, 0.001))
                    using (Mat paillouU16 = XImgProcCv2.GradientPaillouX(gradientU16, 1.0, 1.0))
                    using (Mat paillouS16 = XImgProcCv2.GradientPaillouY(gradientS16, 1.0, 1.0))
                    using (Mat paillouF32 = XImgProcCv2.GradientPaillouX(gradientF32, 1.0, 1.0))
                    {
                        Assert.Equal(gradientU8.Rows, dericheU8.Rows);
                        Assert.Equal(gradientS8.Cols, dericheS8.Cols);
                        Assert.Equal(gradientU16.Rows, paillouU16.Rows);
                        Assert.Equal(gradientS16.Cols, paillouS16.Cols);
                        Assert.Equal(gradientF32.Rows, paillouF32.Rows);
                    }
                }
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FourierDescriptor(null!, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.FourierDescriptor(mat, null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FourierDescriptor(mat, mat, nbElt: 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FourierDescriptor(mat, mat, nbElt: -2));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FourierDescriptor(mat, mat, nbFD: 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FourierDescriptor(mat, mat, nbFD: -2));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FourierDescriptor(mat, mat, nbElt: 8, nbFD: 5));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FourierDescriptor(mat, nbElt: 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FourierDescriptor(mat, nbFD: 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.FourierDescriptor(mat, nbElt: 8, nbFD: 5));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.TransformFD(null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.TransformFD(mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.TransformFD(mat, mat, null!));
                using (var transformEmpty = new Mat())
                using (var transformWrongCols = new Mat(1, 4, MatType.CV_64FC1))
                using (var transformWrongRows = new Mat(2, 5, MatType.CV_64FC1))
                using (var transformWrongDepth = new Mat(1, 5, MatType.CV_32FC1))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.TransformFD(mat, transformEmpty, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.TransformFD(mat, transformWrongCols, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.TransformFD(mat, transformWrongRows, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.TransformFD(mat, transformWrongDepth, mat));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.TransformFD(mat, transformEmpty));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.TransformFD(mat, transformWrongDepth));
                }
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.ContourSampling(null!, mat, 8));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.ContourSampling(mat, null!, 8));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.ContourSampling(mat, mat, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.ContourSampling(mat, mat, -1));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.ContourSampling(mat, 0));
                Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcCv2.ContourSampling(mat, -1));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.CovarianceEstimation(null!, mat, 2, 2));
                Assert.Throws<ArgumentNullException>(() => XImgProcCv2.CovarianceEstimation(mat, null!, 2, 2));
                using (var covarianceColor = new Mat(4, 4, MatType.CV_32FC3, new Scalar(1, 2, 3)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CovarianceEstimation(covarianceColor, mat, 2, 2));
                    Assert.Throws<ArgumentException>(() => XImgProcCv2.CovarianceEstimation(covarianceColor, 2, 2));
                }
                Assert.Throws<ArgumentNullException>(() => XImgProcRlCv2.Threshold(null!, mat, 1.0, ThresholdTypes.Binary));
                using (var rleThresholdEmpty = new Mat())
                using (var rleThresholdColor = new Mat(8, 8, MatType.CV_8UC3, new Scalar(1, 2, 3)))
                using (var rleThresholdGray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(1)))
                {
                    Assert.Throws<ArgumentException>(() => XImgProcRlCv2.Threshold(rleThresholdEmpty, mat, 1.0, ThresholdTypes.Binary));
                    Assert.Throws<ArgumentException>(() => XImgProcRlCv2.Threshold(rleThresholdEmpty, 1.0, ThresholdTypes.Binary));
                    Assert.Throws<ArgumentException>(() => XImgProcRlCv2.Threshold(rleThresholdColor, mat, 1.0, ThresholdTypes.Binary));
                    Assert.Throws<ArgumentException>(() => XImgProcRlCv2.Threshold(rleThresholdColor, 1.0, ThresholdTypes.Binary));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcRlCv2.Threshold(rleThresholdGray, mat, 1.0, ThresholdTypes.Trunc));
                    Assert.Throws<ArgumentOutOfRangeException>(() => XImgProcRlCv2.Threshold(rleThresholdGray, 1.0, ThresholdTypes.Trunc));
                }

                Assert.Throws<ArgumentNullException>(() => XImgProcRlCv2.Dilate(null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcRlCv2.Erode(null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => XImgProcRlCv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3), null!));
                Assert.Throws<ArgumentNullException>(() => XImgProcRlCv2.Paint(null!, mat, new Scalar(1)));
                Assert.Throws<ArgumentNullException>(() => XImgProcRlCv2.IsRLMorphologyPossible(null!));
                Assert.Throws<ArgumentNullException>(() => XImgProcRlCv2.CreateRLEImage(new[] { new Point3i(0, 1, 0) }, new Size(2, 2), null!));
                Assert.Throws<ArgumentNullException>(() => XImgProcRlCv2.MorphologyEx(null!, mat, MorphTypes.Open, mat));
                Assert.Throws<ArgumentException>(() => XImgProcRlCv2.CreateRLEImage(Array.Empty<Point3i>(), new Size(2, 2), mat));
            }
        }

        [Fact]
        public void ValueObjectsExposeExpectedShape()
        {
            var edgeBox = new EdgeBox(new Rect(1, 2, 3, 4), 0.5F);
            var ellipse = new EdgeDrawingEllipse(1.0, 2.0, 3.0, 4.0, 5.0, 6.0);
            var parameters = new EdgeDrawingParams(
                pfMode: true,
                edgeDetectionOperator: EdgeDrawingGradientOperator.Sobel,
                gradientThresholdValue: 12,
                anchorThresholdValue: 2,
                scanInterval: 1,
                minPathLength: 5,
                sigma: 1.2F,
                sumFlag: true,
                nfaValidation: true,
                minLineLength: 8,
                maxDistanceBetweenTwoLines: 4.0,
                lineFitErrorThreshold: 1.0,
                maxErrorThreshold: 2.0);

            Assert.Equal(3, edgeBox.Rectangle.Width);
            Assert.Equal(0.5F, edgeBox.Score);
            Assert.Equal(new EdgeBox(new Rect(1, 2, 3, 4), 0.5F), edgeBox);
            Assert.True(edgeBox == new EdgeBox(new Rect(1, 2, 3, 4), 0.5F));
            Assert.True(edgeBox != new EdgeBox(new Rect(1, 2, 3, 4), 0.25F));
            Assert.False(edgeBox.Equals("not an edge box"));
            Assert.Equal(new EdgeBox(new Rect(1, 2, 3, 4), 0.5F).GetHashCode(), edgeBox.GetHashCode());
            Assert.Equal("{Rectangle={X=1,Y=2,Width=3,Height=4},Score=0.5}", edgeBox.ToString());
            Assert.Equal(1.0F, ellipse.Center.X);
            Assert.Equal(6.0, ellipse.Score);
            Assert.Equal(new EdgeDrawingEllipse(1.0, 2.0, 3.0, 4.0, 5.0, 6.0), ellipse);
            Assert.True(ellipse == new EdgeDrawingEllipse(1.0, 2.0, 3.0, 4.0, 5.0, 6.0));
            Assert.True(ellipse != new EdgeDrawingEllipse(1.0, 2.0, 3.0, 4.0, 5.0, 7.0));
            Assert.False(ellipse.Equals("not an edge drawing ellipse"));
            Assert.Equal(new EdgeDrawingEllipse(1.0, 2.0, 3.0, 4.0, 5.0, 6.0).GetHashCode(), ellipse.GetHashCode());
            Assert.Equal("{Center={X=1,Y=2},AxisA=3,AxisB=4,Angle=5,Score=6}", ellipse.ToString());
            Assert.True(ellipse.IsCircle);
            Assert.False(ellipse.IsEllipse);
            Assert.Equal(3.0, ellipse.Radius);
            Assert.Equal(4.0, ellipse.EllipseAxisA);
            Assert.Equal(5.0, ellipse.EllipseAxisB);
            Assert.Equal(6.0, ellipse.EllipseAngle);
            Assert.True(parameters.PFMode);
            Assert.Equal(EdgeDrawingGradientOperator.Sobel, parameters.EdgeDetectionOperator);
            Assert.Equal(new EdgeDrawingParams(true, EdgeDrawingGradientOperator.Sobel, 12, 2, 1, 5, 1.2F, true, true, 8, 4.0, 1.0, 2.0), parameters);
            Assert.True(parameters == new EdgeDrawingParams(true, EdgeDrawingGradientOperator.Sobel, 12, 2, 1, 5, 1.2F, true, true, 8, 4.0, 1.0, 2.0));
            Assert.True(parameters != new EdgeDrawingParams(false, EdgeDrawingGradientOperator.Sobel, 12, 2, 1, 5, 1.2F, true, true, 8, 4.0, 1.0, 2.0));
            Assert.False(parameters.Equals("not edge drawing params"));
            Assert.Equal(new EdgeDrawingParams(true, EdgeDrawingGradientOperator.Sobel, 12, 2, 1, 5, 1.2F, true, true, 8, 4.0, 1.0, 2.0).GetHashCode(), parameters.GetHashCode());
            Assert.Equal("EdgeDrawingParams(PFMode=True, EdgeDetectionOperator=Sobel, GradientThresholdValue=12, AnchorThresholdValue=2, ScanInterval=1, MinPathLength=5, Sigma=1.2, SumFlag=True, NFAValidation=True, MinLineLength=8, MaxDistanceBetweenTwoLines=4, LineFitErrorThreshold=1, MaxErrorThreshold=2)", parameters.ToString());
            Assert.Equal(new Point3i(1, 2, 3), new Point3i(1, 2, 3));
            Assert.Equal("{X=1,Y=2,Z=3}", new Point3i(1, 2, 3).ToString());
        }

        [Fact]
        public void EdgeDrawingEllipseExposesOpenCvVec6dSemantics()
        {
            var ellipseRow = new EdgeDrawingEllipse(10.0, 20.0, 0.0, 30.0, 40.0, 50.0);
            Assert.False(ellipseRow.IsCircle);
            Assert.True(ellipseRow.IsEllipse);
            Assert.Equal(0.0, ellipseRow.Radius);
            Assert.Equal(30.0, ellipseRow.EllipseAxisA);
            Assert.Equal(40.0, ellipseRow.EllipseAxisB);
            Assert.Equal(50.0, ellipseRow.EllipseAngle);

            var circleRow = new EdgeDrawingEllipse(10.0, 20.0, 15.0, 0.0, 0.0, 0.0);
            Assert.True(circleRow.IsCircle);
            Assert.False(circleRow.IsEllipse);
            Assert.Equal(15.0, circleRow.Radius);
            Assert.Equal(0.0, circleRow.EllipseAxisA);
            Assert.Equal(0.0, circleRow.EllipseAxisB);
            Assert.Equal(0.0, circleRow.EllipseAngle);
        }

        [Fact]
        public void EdgeDrawingValueObjectsFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                Assert.Equal(
                    "{Rectangle={X=1,Y=2,Width=3,Height=4},Score=0.5}",
                    new EdgeBox(new Rect(1, 2, 3, 4), 0.5F).ToString());
                Assert.Equal(
                    "{Center={X=1.5,Y=2.25},AxisA=3.5,AxisB=4.25,Angle=5.5,Score=6.25}",
                    new EdgeDrawingEllipse(1.5, 2.25, 3.5, 4.25, 5.5, 6.25).ToString());
                Assert.Equal(
                    "EdgeDrawingParams(PFMode=True, EdgeDetectionOperator=Sobel, GradientThresholdValue=12, AnchorThresholdValue=2, ScanInterval=1, MinPathLength=5, Sigma=1.2, SumFlag=True, NFAValidation=True, MinLineLength=8, MaxDistanceBetweenTwoLines=4.5, LineFitErrorThreshold=1.25, MaxErrorThreshold=2.5)",
                    new EdgeDrawingParams(true, EdgeDrawingGradientOperator.Sobel, 12, 2, 1, 5, 1.2F, true, true, 8, 4.5, 1.25, 2.5).ToString());
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void EdgeBoxHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(20, Marshal.SizeOf<EdgeBox>());

            Assert.Equal(0, FieldOffset<EdgeBox>("<Rectangle>k__BackingField"));
            Assert.Equal(16, FieldOffset<EdgeBox>("<Score>k__BackingField"));
        }

        [Fact]
        public void EdgeDrawingEllipseHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(40, Marshal.SizeOf<EdgeDrawingEllipse>());

            Assert.Equal(0, FieldOffset<EdgeDrawingEllipse>("<Center>k__BackingField"));
            Assert.Equal(8, FieldOffset<EdgeDrawingEllipse>("<AxisA>k__BackingField"));
            Assert.Equal(16, FieldOffset<EdgeDrawingEllipse>("<AxisB>k__BackingField"));
            Assert.Equal(24, FieldOffset<EdgeDrawingEllipse>("<Angle>k__BackingField"));
            Assert.Equal(32, FieldOffset<EdgeDrawingEllipse>("<Score>k__BackingField"));
        }

        [Fact]
        public void EdgeDrawingParamsHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(64, Marshal.SizeOf<EdgeDrawingParams>());

            Assert.Equal(0, FieldOffset<EdgeDrawingParams>("<PFMode>k__BackingField"));
            Assert.Equal(4, FieldOffset<EdgeDrawingParams>("<EdgeDetectionOperator>k__BackingField"));
            Assert.Equal(8, FieldOffset<EdgeDrawingParams>("<GradientThresholdValue>k__BackingField"));
            Assert.Equal(12, FieldOffset<EdgeDrawingParams>("<AnchorThresholdValue>k__BackingField"));
            Assert.Equal(16, FieldOffset<EdgeDrawingParams>("<ScanInterval>k__BackingField"));
            Assert.Equal(20, FieldOffset<EdgeDrawingParams>("<MinPathLength>k__BackingField"));
            Assert.Equal(24, FieldOffset<EdgeDrawingParams>("<Sigma>k__BackingField"));
            Assert.Equal(28, FieldOffset<EdgeDrawingParams>("<SumFlag>k__BackingField"));
            Assert.Equal(32, FieldOffset<EdgeDrawingParams>("<NFAValidation>k__BackingField"));
            Assert.Equal(36, FieldOffset<EdgeDrawingParams>("<MinLineLength>k__BackingField"));
            Assert.Equal(40, FieldOffset<EdgeDrawingParams>("<MaxDistanceBetweenTwoLines>k__BackingField"));
            Assert.Equal(48, FieldOffset<EdgeDrawingParams>("<LineFitErrorThreshold>k__BackingField"));
            Assert.Equal(56, FieldOffset<EdgeDrawingParams>("<MaxErrorThreshold>k__BackingField"));
        }

        [Fact]
        public void StaticFunctionSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var gray = CreateGrayImage())
            using (var color = CreateColorImage())
            using (var binary = new Mat())
            using (var skeleton = new Mat())
            using (var joint = new Mat())
            using (var guided = new Mat())
            using (var rolling = new Mat())
            using (var weighted = new Mat())
            using (var dt = new Mat())
            using (var am = new Mat())
            using (var texture = new Mat())
            using (var edgePreserved = new Mat())
            using (var fgs = new Mat())
            using (var l0 = new Mat())
            using (var diffusion = new Mat())
            using (var peiLin = new Mat())
            {
                XImgProcCv2.NiBlackThreshold(gray, binary, 255.0, ThresholdTypes.Binary, 3, -0.2);
                XImgProcCv2.Thinning(binary, skeleton);
                XImgProcCv2.JointBilateralFilter(gray, gray, joint, 3, 12.0, 3.0);
                XImgProcCv2.GuidedFilter(gray, gray, guided, 2, 1.0);
                XImgProcCv2.RollingGuidanceFilter(color, rolling, d: 3, sigmaColor: 12.0, sigmaSpace: 3.0, numOfIter: 1);
                XImgProcCv2.WeightedMedianFilter(gray, gray, weighted, 1, 12.0, WeightedMedianFilterWeightType.Off);
                XImgProcCv2.DtFilter(color, color, dt, 4.0, 8.0);
                XImgProcCv2.AmFilter(color, color, am, 4.0, 0.2);
                XImgProcCv2.BilateralTextureFilter(color, texture, 3, 1);
                XImgProcCv2.EdgePreservingFilter(color, edgePreserved, 3, 20.0);
                XImgProcCv2.FastGlobalSmootherFilter(gray, gray, fgs, 8.0, 12.0);
                XImgProcCv2.L0Smooth(color, l0);
                XImgProcCv2.AnisotropicDiffusion(color, diffusion, 0.1F, 10.0F, 1);
                XImgProcCv2.PeiLinNormalization(gray, peiLin);

                Assert.Equal(gray.Rows, binary.Rows);
                Assert.Equal(gray.Cols, skeleton.Cols);
                Assert.Equal(gray.Rows, guided.Rows);
                Assert.Equal(color.Rows, rolling.Rows);
                Assert.Equal(gray.Rows, weighted.Rows);
                Assert.Equal(color.Rows, dt.Rows);
                Assert.Equal(color.Rows, am.Rows);
                Assert.Equal(color.Rows, texture.Rows);
                Assert.Equal(color.Rows, edgePreserved.Rows);
                Assert.Equal(gray.Rows, fgs.Rows);
                Assert.Equal(color.Rows, l0.Rows);
                Assert.Equal(color.Rows, diffusion.Rows);
                Assert.Equal(2, peiLin.Rows);
                Assert.Equal(3, peiLin.Cols);
            }
        }

        [Fact]
        public void FilterObjectsAndSuperpixelsSmokeRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var gray = CreateGrayImage())
            using (var color = CreateColorImage())
            using (var guidedDst = new Mat())
            using (var fgsDst = new Mat())
            using (var slicLabels = new Mat())
            using (var slicMask = new Mat())
            using (var seedsLabels = new Mat())
            using (var seedsMask = new Mat())
            using (var lscLabels = new Mat())
            using (var lscMask = new Mat())
            using (var guided = GuidedFilter.Create(gray, 2, 1.0))
            using (var smoother = FastGlobalSmootherFilter.Create(gray, 8.0, 12.0))
            using (var slic = SuperpixelSLIC.Create(color, SLICType.SLICO, 8, 10.0F))
            using (var seeds = SuperpixelSEEDS.Create(color.Cols, color.Rows, color.Channels, 4, 2))
            using (var lsc = SuperpixelLSC.Create(color, 8, 0.075F))
            {
                guided.Filter(gray, guidedDst);
                smoother.Filter(gray, fgsDst);

                slic.Iterate(1);
                slic.EnforceLabelConnectivity(10);
                slic.GetLabels(slicLabels);
                slic.GetLabelContourMask(slicMask);

                seeds.Iterate(color, 1);
                seeds.GetLabels(seedsLabels);
                seeds.GetLabelContourMask(seedsMask);

                lsc.Iterate(1);
                lsc.EnforceLabelConnectivity(10);
                lsc.GetLabels(lscLabels);
                lsc.GetLabelContourMask(lscMask);

                Assert.False(guided.IsDisposed);
                Assert.False(smoother.IsDisposed);
                Assert.Equal(gray.Rows, guidedDst.Rows);
                Assert.Equal(gray.Rows, fgsDst.Rows);
                Assert.Equal(color.Rows, slicLabels.Rows);
                Assert.Equal(color.Cols, slicMask.Cols);
                Assert.Equal(color.Rows, seedsLabels.Rows);
                Assert.Equal(color.Cols, seedsMask.Cols);
                Assert.Equal(color.Rows, lscLabels.Rows);
                Assert.Equal(color.Cols, lscMask.Cols);
                Assert.True(slic.NumberOfSuperpixels >= 0);
                Assert.True(seeds.NumberOfSuperpixels >= 0);
                Assert.True(lsc.NumberOfSuperpixels >= 0);
            }
        }

        [Fact]
        public void FastLineDetectorSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0)))
            using (var drawing = new Mat(32, 32, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (var linesMat = new Mat())
            using (var detector = FastLineDetector.Create(lengthThreshold: 6, cannyApertureSize: 3))
            {
                ImgProcCv2.Line(image, new Point(3, 4), new Point(28, 24), new Scalar(255), 1);

                detector.Detect(image, linesMat);
                LineSegment[] lines = detector.Detect(image);
                detector.DrawSegments(drawing, linesMat);
                if (lines.Length > 0)
                {
                    detector.DrawSegments(drawing, lines);
                }

                Assert.False(detector.IsDisposed);
                Assert.True(linesMat.Rows >= 0);
                Assert.NotNull(lines);
                Assert.Equal(image.Rows, drawing.Rows);
            }
        }

        [Fact]
        public void DisparityAndFastBilateralSolverSmokeRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var gray = CreateGrayImage())
            using (var disparity = CreateDisparityMap())
            using (var disparityRight = CreateDisparityMap())
            using (var filtered = new Mat())
            using (var vis = new Mat())
            using (var wls = DisparityWLSFilter.CreateGeneric(useConfidence: false))
            {
                wls.Lambda = 8000.0;
                wls.SigmaColor = 1.5;
                wls.LrcThreshold = 24;
                wls.DepthDiscontinuityRadius = 3;
                wls.Filter(disparity, gray, filtered, disparityRight, new Rect(0, 0, gray.Cols, gray.Rows), gray);
                using (var confidenceMap = new Mat())
                {
                    wls.GetConfidenceMap(confidenceMap);
                }
                XImgProcCv2.GetDisparityVis(filtered, vis, 1.0);
                double mse = XImgProcCv2.ComputeMSE(disparity, filtered, new Rect(0, 0, gray.Cols, gray.Rows));
                double bad = XImgProcCv2.ComputeBadPixelPercent(disparity, filtered, new Rect(0, 0, gray.Cols, gray.Rows));

                Assert.False(wls.IsDisposed);
                Assert.Equal(gray.Rows, filtered.Rows);
                Assert.Equal(gray.Rows, vis.Rows);
                Assert.True(mse >= 0.0);
                Assert.True(bad >= 0.0);

                using (var src32 = gray.ConvertTo(MatType.CV_32FC1, 1.0 / 255.0))
                using (var confidence = new Mat(gray.Rows, gray.Cols, MatType.CV_32FC1, new Scalar(1.0)))
                using (var confidence8U = new Mat(gray.Rows, gray.Cols, MatType.CV_8UC1, new Scalar(255)))
                using (var src16U = new Mat(gray.Rows, gray.Cols, MatType.CV_16UC1, new Scalar(128)))
                using (var src16S = new Mat(gray.Rows, gray.Cols, MatType.CV_16SC1, new Scalar(128)))
                using (var sizeMismatch = new Mat(4, 4, MatType.CV_8UC1, new Scalar(48)))
                using (var fbsDst = new Mat())
                {
                    try
                    {
                        using (var fbs = FastBilateralSolverFilter.Create(gray, 4.0, 4.0, 4.0, numIter: 5))
                        {
                            fbs.Filter(src32, confidence, fbsDst);
                            Assert.Throws<ArgumentException>(() => fbs.Filter(sizeMismatch, confidence));
                            Assert.Throws<ArgumentException>(() => fbs.Filter(src32, sizeMismatch));
                        }

                        using (Mat oneShot = XImgProcCv2.FastBilateralSolverFilter(gray, src32, confidence, 4.0, 4.0, 4.0, numIter: 5))
                        using (Mat oneShot16U = XImgProcCv2.FastBilateralSolverFilter(gray, src16U, confidence, 4.0, 4.0, 4.0, numIter: 5))
                        using (Mat oneShot16S = XImgProcCv2.FastBilateralSolverFilter(gray, src16S, confidence8U, 4.0, 4.0, 4.0, numIter: 5))
                        {
                            Assert.Equal(gray.Rows, oneShot.Rows);
                            Assert.Equal(gray.Rows, oneShot16U.Rows);
                            Assert.Equal(gray.Cols, oneShot16S.Cols);
                        }

                        Assert.Equal(gray.Rows, fbsDst.Rows);
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("needs to be compiled with EIGEN", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Assert.Contains("EIGEN", ex.Message, StringComparison.OrdinalIgnoreCase);
                    }
                }
            }
        }

        [Fact]
        public void SparseInterpolatorsSmokeRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var from = CreateColorImage())
            using (var to = CreateColorImage())
            using (var fromPoints = Calib3DCv2.ToPointMat(CreateFromPoints()))
            using (var toPoints = Calib3DCv2.ToPointMat(CreateToPoints()))
            using (var edgeAwareFlow = new Mat())
            using (var ricFlow = new Mat())
            using (var edgeAware = EdgeAwareInterpolator.Create())
            using (var ric = RICInterpolator.Create())
            {
                edgeAware.K = 4;
                edgeAware.Sigma = 0.05F;
                edgeAware.Lambda = 10.0F;
                edgeAware.UsePostProcessing = false;
                edgeAware.Interpolate(from, fromPoints, to, toPoints, edgeAwareFlow);

                ric.K = 4;
                ric.SuperpixelSize = 8;
                ric.SuperpixelNNCount = 8;
                Assert.Throws<ArgumentOutOfRangeException>(() => ric.SuperpixelMode = (SLICType)99);
                ric.SuperpixelMode = SLICType.SLIC;
                ric.UseGlobalSmootherFilter = false;
                ric.UseVariationalRefinement = false;
                ric.Interpolate(from, fromPoints, to, toPoints, ricFlow);

                Assert.Equal(from.Rows, edgeAwareFlow.Rows);
                Assert.Equal(from.Cols, edgeAwareFlow.Cols);
                Assert.Equal(from.Rows, ricFlow.Rows);
                Assert.False(edgeAware.IsDisposed);
                Assert.False(ric.IsDisposed);
            }
        }

        [Fact]
        public void EdgeDrawingAndEdgeBoxesSmokeRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = new Mat(48, 48, MatType.CV_8UC1, new Scalar(0)))
            using (var edgeMap = new Mat(48, 48, MatType.CV_32FC1, new Scalar(0.1)))
            using (var orientationMap = new Mat(48, 48, MatType.CV_32FC1, new Scalar(0.0)))
            using (var edgeImage = new Mat())
            using (var gradientImage = new Mat())
            using (var linesMat = new Mat())
            using (var ellipsesMat = new Mat())
            using (var drawing = EdgeDrawing.Create())
            using (var boxes = EdgeBoxes.Create(maxBoxes: 5, minScore: 0.0F, minBoxArea: 4.0F))
            {
                boxes.Alpha = 0.70F;
                Assert.Equal(0.70F, boxes.Alpha, 4);
                boxes.Beta = 0.80F;
                Assert.Equal(0.80F, boxes.Beta, 4);
                boxes.Eta = 0.95F;
                Assert.Equal(0.95F, boxes.Eta, 4);
                boxes.MinScore = 0.02F;
                Assert.Equal(0.02F, boxes.MinScore, 4);
                boxes.MaxBoxes = 4;
                Assert.Equal(4, boxes.MaxBoxes);
                boxes.EdgeMinMag = 0.20F;
                Assert.Equal(0.20F, boxes.EdgeMinMag, 4);
                boxes.EdgeMergeThr = 0.40F;
                Assert.Equal(0.40F, boxes.EdgeMergeThr, 4);
                boxes.ClusterMinMag = 0.60F;
                Assert.Equal(0.60F, boxes.ClusterMinMag, 4);
                boxes.MaxAspectRatio = 2.50F;
                Assert.Equal(2.50F, boxes.MaxAspectRatio, 4);
                boxes.MinBoxArea = 8.0F;
                Assert.Equal(8.0F, boxes.MinBoxArea, 4);
                boxes.Gamma = 1.75F;
                Assert.Equal(1.75F, boxes.Gamma, 4);
                boxes.Kappa = 1.25F;
                Assert.Equal(1.25F, boxes.Kappa, 4);

                ImgProcCv2.Line(image, new Point(4, 4), new Point(42, 34), new Scalar(255), 1);
                ImgProcCv2.Circle(image, new Point(24, 24), 8, new Scalar(255), 1);

                EdgeDrawingParams parameters = drawing.Params;
                parameters.EdgeDetectionOperator = EdgeDrawingGradientOperator.Sobel;
                parameters.GradientThresholdValue = 10;
                parameters.AnchorThresholdValue = 2;
                parameters.MinLineLength = 4;
                parameters.MinPathLength = 4;
                drawing.Params = parameters;
                EdgeDrawingParams roundTripParameters = drawing.Params;
                Assert.Equal(parameters.EdgeDetectionOperator, roundTripParameters.EdgeDetectionOperator);
                Assert.Equal(parameters.GradientThresholdValue, roundTripParameters.GradientThresholdValue);
                Assert.Equal(parameters.AnchorThresholdValue, roundTripParameters.AnchorThresholdValue);
                Assert.Equal(parameters.MinLineLength, roundTripParameters.MinLineLength);
                Assert.Equal(parameters.MinPathLength, roundTripParameters.MinPathLength);
                drawing.DetectEdges(image);
                drawing.GetEdgeImage(edgeImage);
                drawing.GetGradientImage(gradientImage);
                Point[][] segments = drawing.GetSegments();
                drawing.DetectLines(linesMat);
                LineSegment[] lines = drawing.DetectLines();
                int[] indices = drawing.GetSegmentIndicesOfLines();
                drawing.DetectEllipses(ellipsesMat);
                EdgeDrawingEllipse[] ellipses = drawing.DetectEllipses();
                EdgeBox[] proposals = boxes.GetBoundingBoxes(edgeMap, orientationMap);

                Assert.Equal(image.Rows, edgeImage.Rows);
                Assert.Equal(image.Rows, gradientImage.Rows);
                Assert.NotNull(segments);
                Assert.NotNull(lines);
                Assert.NotNull(indices);
                Assert.NotNull(ellipses);
                Assert.NotNull(proposals);
                Assert.True(boxes.MaxBoxes <= 4);
            }
        }

        [Fact]
        public void EdgeBoxesRejectsNonFloatMaps()
        {
            if (!IsXImgProcModuleLinked())
            {
                return;
            }

            using (var edgeMap = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.1)))
            using (var orientationMap = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.0)))
            using (var badEdgeMap = new Mat(edgeMap.Rows, edgeMap.Cols, MatType.CV_8UC1, new Scalar(1)))
            using (var badOrientationMap = new Mat(orientationMap.Rows, orientationMap.Cols, MatType.CV_16UC1, new Scalar(0)))
            using (var boxes = EdgeBoxes.Create())
            {
                Assert.Throws<ArgumentException>(() => boxes.GetBoundingBoxes(badEdgeMap, orientationMap));
                Assert.Throws<ArgumentException>(() => boxes.GetBoundingBoxes(edgeMap, badOrientationMap));
            }
        }

        [Fact]
        public void ScanSegmentRejectsInvalidIterateImages()
        {
            if (!IsXImgProcModuleLinked())
            {
                return;
            }

            using (var color = CreateColorImage())
            using (var empty = new Mat())
            using (var sizeMismatch = new Mat(color.Rows / 2, color.Cols / 2, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (var depthMismatch = new Mat(color.Rows, color.Cols, MatType.CV_32FC3, new Scalar(0.1, 0.2, 0.3)))
            using (var channelMismatch = new Mat(color.Rows, color.Cols, MatType.CV_8UC1, new Scalar(0)))
            using (var scan = ScanSegment.Create(color.Cols, color.Rows, 4, slices: 1, mergeSmall: true))
            {
                Assert.Throws<ArgumentException>(() => scan.Iterate(empty));
                Assert.Throws<ArgumentException>(() => scan.Iterate(sizeMismatch));
                Assert.Throws<ArgumentException>(() => scan.Iterate(depthMismatch));
                Assert.Throws<ArgumentException>(() => scan.Iterate(channelMismatch));
            }
        }

        [Fact]
        public void SuperpixelSEEDSRejectsInvalidIterateImages()
        {
            if (!IsXImgProcModuleLinked())
            {
                return;
            }

            using (var color = CreateColorImage())
            using (var empty = new Mat())
            using (var sizeMismatch = new Mat(color.Rows / 2, color.Cols / 2, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (var depthMismatch = new Mat(color.Rows, color.Cols, MatType.CV_64FC3, new Scalar(0.1, 0.2, 0.3)))
            using (var channelMismatch = new Mat(color.Rows, color.Cols, MatType.CV_8UC1, new Scalar(0)))
            using (var valid16U = new Mat(color.Rows, color.Cols, MatType.CV_16UC3, new Scalar(10, 20, 30)))
            using (var valid32F = new Mat(color.Rows, color.Cols, MatType.CV_32FC3, new Scalar(0.1, 0.2, 0.3)))
            using (var seeds = SuperpixelSEEDS.Create(color.Cols, color.Rows, color.Channels, 4, 2))
            {
                Assert.Throws<ArgumentException>(() => seeds.Iterate(empty));
                Assert.Throws<ArgumentException>(() => seeds.Iterate(sizeMismatch));
                Assert.Throws<ArgumentException>(() => seeds.Iterate(depthMismatch));
                Assert.Throws<ArgumentException>(() => seeds.Iterate(channelMismatch));

                seeds.Iterate(color, 1);
                seeds.Iterate(valid16U, 1);
                seeds.Iterate(valid32F, 1);
            }
        }

        [Fact]
        public void SuperpixelSLICRejectsInvalidInputs()
        {
            if (!IsXImgProcModuleLinked())
            {
                return;
            }

            using (var color = CreateColorImage())
            using (var empty = new Mat())
            using (var slic = SuperpixelSLIC.Create(color, SLICType.SLICO, 8, 10.0F))
            {
                Assert.Throws<ArgumentException>(() => SuperpixelSLIC.Create(empty, SLICType.SLICO, 8, 10.0F));
                Assert.Throws<ArgumentOutOfRangeException>(() => slic.EnforceLabelConnectivity(-1));
                Assert.Throws<ArgumentOutOfRangeException>(() => slic.EnforceLabelConnectivity(101));

                slic.EnforceLabelConnectivity(0);
                slic.EnforceLabelConnectivity(100);
            }
        }

        [Fact]
        public void SuperpixelLSCRejectsEmptyCreateImage()
        {
            using (var empty = new Mat())
            {
                Assert.Throws<ArgumentException>(() => SuperpixelLSC.Create(empty, 8, 0.075F));
            }
        }

        [Fact]
        public void FastLineDetectorRejectsInvalidCreateParameters()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FastLineDetector.Create(lengthThreshold: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => FastLineDetector.Create(distanceThreshold: 0.0F));
            Assert.Throws<ArgumentOutOfRangeException>(() => FastLineDetector.Create(cannyTh1: 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => FastLineDetector.Create(cannyTh2: 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => FastLineDetector.Create(cannyApertureSize: -1));
        }

        [Fact]
        public void FastLineDetectorRejectsInvalidDrawSegmentImages()
        {
            if (!IsXImgProcModuleLinked())
            {
                return;
            }

            using (var detector = FastLineDetector.Create(lengthThreshold: 6, cannyApertureSize: 3))
            using (var empty = new Mat())
            using (var invalidChannels = new Mat(8, 8, MatType.CV_8UC2, new Scalar(0)))
            using (var lines = new Mat(1, 1, MatType.CV_32FC4, new Scalar(0, 0, 1, 1)))
            {
                var managedLines = new[] { new LineSegment(0, 0, 1, 1) };

                Assert.Throws<ArgumentException>(() => detector.DrawSegments(empty, lines));
                Assert.Throws<ArgumentException>(() => detector.DrawSegments(empty, managedLines));
                Assert.Throws<ArgumentException>(() => detector.DrawSegments(invalidChannels, lines));
                Assert.Throws<ArgumentException>(() => detector.DrawSegments(invalidChannels, managedLines));
            }
        }

        [Fact]
        public void FastLineDetectorRejectsInvalidDrawSegmentLines()
        {
            if (!IsXImgProcModuleLinked())
            {
                return;
            }

            using (var detector = FastLineDetector.Create(lengthThreshold: 6, cannyApertureSize: 3))
            using (var image = new Mat(8, 8, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (var emptyLines = new Mat())
            using (var validLines = new Mat(1, 1, MatType.CV_32FC4, new Scalar(0, 0, 7, 7)))
            using (var validColumnLines = new Mat(1, 4, MatType.CV_32FC1, new Scalar(0)))
            using (var invalidDepth = new Mat(1, 1, MatType.CV_32SC4, new Scalar(0, 0, 7, 7)))
            using (var invalidShape = new Mat(2, 2, MatType.CV_32FC1, new Scalar(0)))
            {
                detector.DrawSegments(image, emptyLines);
                detector.DrawSegments(image, validLines);
                detector.DrawSegments(image, validColumnLines);

                Assert.Throws<ArgumentException>(() => detector.DrawSegments(image, invalidDepth));
                Assert.Throws<ArgumentException>(() => detector.DrawSegments(image, invalidShape));
            }
        }

        [Fact]
        public void RemainingXImgProcUtilitiesSmokeRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var gray = CreateGrayImage())
            using (var color = CreateColorImage())
            using (var dericheX = new Mat())
            using (var dericheY = new Mat())
            using (var paillouX = new Mat())
            using (var paillouY = new Mat())
            using (var ridge = RidgeDetectionFilter.Create())
            using (var ridgeDst = new Mat())
            using (var graph = GraphSegmentation.Create(sigma: 0.5, k: 50.0F, minSize: 2))
            using (var graphLabels = new Mat())
            using (var scan = ScanSegment.Create(color.Cols, color.Rows, 8, slices: 1, mergeSmall: true))
            using (var scanLabels = new Mat())
            using (var scanMask = new Mat())
            {
                XImgProcCv2.GradientDericheX(color, dericheX, 0.5, 0.0005);
                XImgProcCv2.GradientDericheY(color, dericheY, 0.5, 0.0005);
                XImgProcCv2.GradientPaillouX(color, paillouX, 1.0, 1.0);
                XImgProcCv2.GradientPaillouY(color, paillouY, 1.0, 1.0);
                ridge.GetRidgeFilteredImage(gray, ridgeDst);

                using (var contour = CreateContourPointMat())
                using (var sampled = XImgProcCv2.ContourSampling(contour, 8))
                using (var descriptor = XImgProcCv2.FourierDescriptor(contour, nbElt: 8, nbFD: 4))
                using (var fitting = ContourFitting.Create(8, 3))
                using (var transform = fitting.EstimateTransformation(sampled, sampled, out double distance))
                using (var transformed = XImgProcCv2.TransformFD(sampled, transform, fdContour: false))
                using (var complex = CreateComplexImage())
                using (var covariance = XImgProcCv2.CovarianceEstimation(complex, 2, 2))
                {
                    Assert.Equal(color.Rows, dericheX.Rows);
                    Assert.Equal(color.Rows, dericheY.Rows);
                    Assert.Equal(color.Rows, paillouX.Rows);
                    Assert.Equal(color.Rows, paillouY.Rows);
                    Assert.Equal(gray.Rows, ridgeDst.Rows);
                    Assert.True(sampled.Rows > 0 || sampled.Cols > 0);
                    Assert.True(descriptor.Rows > 0 || descriptor.Cols > 0);
                    Assert.True(distance >= 0.0);
                    Assert.True(transform.Rows > 0 || transform.Cols > 0);
                    Assert.True(transformed.Rows > 0 || transformed.Cols > 0);
                    Assert.Equal(4, covariance.Rows);
                    Assert.Equal(4, covariance.Cols);
                }

                using (var fittingValidation = ContourFitting.Create(8, 3))
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => fittingValidation.CtrSize = 0);
                    Assert.Throws<ArgumentOutOfRangeException>(() => fittingValidation.CtrSize = -1);
                    Assert.Throws<ArgumentOutOfRangeException>(() => fittingValidation.FDSize = 0);
                    Assert.Throws<ArgumentOutOfRangeException>(() => fittingValidation.FDSize = -1);
                    fittingValidation.CtrSize = 8;
                    fittingValidation.FDSize = 3;
                    Assert.Equal(8, fittingValidation.CtrSize);
                    Assert.Equal(3, fittingValidation.FDSize);
                }

                using (var invalidContour = CreateContourPointMat())
                using (var invalidSampled = XImgProcCv2.ContourSampling(invalidContour, 8))
                using (var invalidDescriptorCount = ContourFitting.Create(8, 4))
                using (var transformOut = new Mat())
                {
                    Assert.Throws<ArgumentOutOfRangeException>(() => invalidDescriptorCount.EstimateTransformation(invalidSampled, invalidSampled, transformOut));
                    Assert.Throws<ArgumentOutOfRangeException>(() => invalidDescriptorCount.EstimateTransformation(invalidSampled, invalidSampled, out _));
                }

                using (var rl = XImgProcRlCv2.Threshold(gray, 100.0, ThresholdTypes.Binary))
                using (var kernel = XImgProcRlCv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
                using (var dilated = XImgProcRlCv2.Dilate(rl, kernel))
                using (var eroded = XImgProcRlCv2.Erode(dilated, kernel))
                using (var opened = XImgProcRlCv2.MorphologyEx(rl, MorphTypes.Open, kernel))
                using (var painted = new Mat(gray.Rows, gray.Cols, MatType.CV_8UC1, new Scalar(0)))
                using (var fromRuns = XImgProcRlCv2.CreateRLEImage(new[] { new Point3i(1, 4, 1), new Point3i(1, 4, 2) }, gray.Size))
                {
                    XImgProcRlCv2.Paint(painted, opened, new Scalar(255));
                    Assert.True(XImgProcRlCv2.IsRLMorphologyPossible(kernel));
                    Assert.True(rl.Rows > 0);
                    Assert.True(eroded.Rows > 0);
                    Assert.True(fromRuns.Rows > 0);
                    Assert.Equal(gray.Rows, painted.Rows);
                }

                scan.Iterate(color);
                scan.GetLabels(scanLabels);
                scan.GetLabelContourMask(scanMask);
                graph.ProcessImage(color, graphLabels);

                Assert.False(ridge.IsDisposed);
                Assert.False(scan.IsDisposed);
                Assert.False(graph.IsDisposed);
                Assert.True(scan.NumberOfSuperpixels >= 0);
                Assert.Equal(color.Rows, scanLabels.Rows);
                Assert.Equal(color.Rows, scanMask.Rows);
                Assert.Equal(color.Rows, graphLabels.Rows);

                using (var segmentation = SelectiveSearchSegmentation.Create())
                using (var strategy = SelectiveSearchSegmentationStrategy.CreateColor())
                using (var multiple = SelectiveSearchSegmentationStrategy.CreateMultiple())
                {
                    segmentation.SetBaseImage(color);
                    segmentation.SwitchToSingleStrategy(k: 20, sigma: 0.8F);
                    multiple.AddStrategy(strategy, 1.0F);
                    multiple.ClearStrategies();
                    Rect[] proposals = segmentation.Process();
                    Assert.NotNull(proposals);
                    Assert.False(segmentation.IsDisposed);
                }
            }
        }

        [Fact]
        public void DisposedObjectsThrowWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var gray = CreateGrayImage())
            using (var color = CreateColorImage())
            using (var dst = new Mat())
            {
                GuidedFilter guided = GuidedFilter.Create(gray, 2, 1.0);
                guided.Dispose();
                Assert.True(guided.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => guided.Filter(gray, dst));

                FastGlobalSmootherFilter smoother = FastGlobalSmootherFilter.Create(gray, 8.0, 12.0);
                smoother.Dispose();
                Assert.True(smoother.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => smoother.Filter(gray, dst));

                DisparityWLSFilter wls = DisparityWLSFilter.CreateGeneric(false);
                wls.Dispose();
                Assert.True(wls.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => wls.Filter(dst, gray, filteredDisparityMap: dst));

                RidgeDetectionFilter ridge = RidgeDetectionFilter.Create();
                ridge.Dispose();
                Assert.True(ridge.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => ridge.GetRidgeFilteredImage(gray, dst));

                GraphSegmentation graph = GraphSegmentation.Create();
                graph.Dispose();
                Assert.True(graph.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => graph.ProcessImage(gray, dst));

                EdgeDrawing edgeDrawing = EdgeDrawing.Create();
                edgeDrawing.Dispose();
                Assert.True(edgeDrawing.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.Params);
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.Params = new EdgeDrawingParams());
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.DetectEdges(gray));
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.GetEdgeImage(dst));
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.GetEdgeImage());
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.GetGradientImage(dst));
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.GetGradientImage());
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.GetSegments());
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.DetectLines(dst));
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.DetectLines());
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.GetSegmentIndicesOfLines());
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.DetectEllipses(dst));
                Assert.Throws<ObjectDisposedException>(() => edgeDrawing.DetectEllipses());

                EdgeBoxes edgeBoxes = EdgeBoxes.Create();
                edgeBoxes.Dispose();
                Assert.True(edgeBoxes.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.Alpha);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.Alpha = 0.65F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.Beta);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.Beta = 0.75F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.Eta);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.Eta = 1.0F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.MinScore);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.MinScore = 0.01F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.MaxBoxes);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.MaxBoxes = 100);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.EdgeMinMag);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.EdgeMinMag = 0.1F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.EdgeMergeThr);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.EdgeMergeThr = 0.5F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.ClusterMinMag);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.ClusterMinMag = 0.5F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.MaxAspectRatio);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.MaxAspectRatio = 3.0F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.MinBoxArea);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.MinBoxArea = 1000.0F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.Gamma);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.Gamma = 2.0F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.Kappa);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.Kappa = 1.5F);
                Assert.Throws<ObjectDisposedException>(() => edgeBoxes.GetBoundingBoxes(gray, gray));

                ContourFitting contourFitting = ContourFitting.Create();
                contourFitting.Dispose();
                Assert.True(contourFitting.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => contourFitting.CtrSize);
                Assert.Throws<ObjectDisposedException>(() => contourFitting.CtrSize = 128);
                Assert.Throws<ObjectDisposedException>(() => contourFitting.FDSize);
                Assert.Throws<ObjectDisposedException>(() => contourFitting.FDSize = 8);
                Assert.Throws<ObjectDisposedException>(() => contourFitting.EstimateTransformation(dst, dst, dst));
                Assert.Throws<ObjectDisposedException>(() => contourFitting.EstimateTransformation(dst, dst, out _));

                ScanSegment scanSegment = ScanSegment.Create(color.Cols, color.Rows, 4);
                scanSegment.Dispose();
                Assert.True(scanSegment.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => scanSegment.NumberOfSuperpixels);
                Assert.Throws<ObjectDisposedException>(() => scanSegment.Iterate(color));
                Assert.Throws<ObjectDisposedException>(() => scanSegment.GetLabels(dst));
                Assert.Throws<ObjectDisposedException>(() => scanSegment.GetLabels());
                Assert.Throws<ObjectDisposedException>(() => scanSegment.GetLabelContourMask(dst));
                Assert.Throws<ObjectDisposedException>(() => scanSegment.GetLabelContourMask());

                SelectiveSearchSegmentation segmentation = SelectiveSearchSegmentation.Create();
                segmentation.Dispose();
                Assert.True(segmentation.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => segmentation.SetBaseImage(color));
                Assert.Throws<ObjectDisposedException>(() => segmentation.SwitchToSingleStrategy());
                Assert.Throws<ObjectDisposedException>(() => segmentation.SwitchToSelectiveSearchFast());
                Assert.Throws<ObjectDisposedException>(() => segmentation.SwitchToSelectiveSearchQuality());
                Assert.Throws<ObjectDisposedException>(() => segmentation.AddImage(color));
                Assert.Throws<ObjectDisposedException>(() => segmentation.ClearImages());
                Assert.Throws<ObjectDisposedException>(() => segmentation.AddGraphSegmentation(GraphSegmentation.Create()));
                Assert.Throws<ObjectDisposedException>(() => segmentation.ClearGraphSegmentations());
                Assert.Throws<ObjectDisposedException>(() => segmentation.AddStrategy(SelectiveSearchSegmentationStrategy.CreateColor()));
                Assert.Throws<ObjectDisposedException>(() => segmentation.ClearStrategies());
                Assert.Throws<ObjectDisposedException>(() => segmentation.Process());

                SelectiveSearchSegmentationStrategy strategy = SelectiveSearchSegmentationStrategy.CreateColor();
                strategy.Dispose();
                Assert.True(strategy.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => strategy.SetImage(color, dst, dst));
                Assert.Throws<ObjectDisposedException>(() => strategy.Get(0, 1));
                Assert.Throws<ObjectDisposedException>(() => strategy.Merge(0, 1));

                FastLineDetector detector = FastLineDetector.Create(lengthThreshold: 6, cannyApertureSize: 3);
                using (var emptyImage = new Mat())
                using (var colorInput = new Mat(gray.Rows, gray.Cols, MatType.CV_8UC3, new Scalar(0, 0, 0)))
                {
                    Assert.Throws<ArgumentException>(() => detector.Detect(emptyImage, dst));
                    Assert.Throws<ArgumentException>(() => detector.Detect(emptyImage));
                    Assert.Throws<ArgumentException>(() => detector.Detect(colorInput, dst));
                    Assert.Throws<ArgumentException>(() => detector.Detect(colorInput));
                }

                detector.Dispose();
                Assert.True(detector.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(gray, dst));
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(gray));
                Assert.Throws<ObjectDisposedException>(() => detector.DrawSegments(color, dst));
                Assert.Throws<ObjectDisposedException>(() => detector.DrawSegments(color, Array.Empty<LineSegment>()));

                EdgeAwareInterpolator edgeAware = EdgeAwareInterpolator.Create();
                edgeAware.Dispose();
                Assert.True(edgeAware.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.K);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.K = 4);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.Sigma);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.Sigma = 0.05F);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.Lambda);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.Lambda = 10.0F);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.UsePostProcessing);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.UsePostProcessing = false);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.FGSLambda);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.FGSLambda = 500.0F);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.FGSSigma);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.FGSSigma = 1.5F);
                Assert.Throws<ObjectDisposedException>(() => edgeAware.Interpolate(color, dst, color, dst, dst));
                Assert.Throws<ObjectDisposedException>(() => edgeAware.SetCostMap(dst));

                RICInterpolator ric = RICInterpolator.Create();
                ric.Dispose();
                Assert.True(ric.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => ric.K);
                Assert.Throws<ObjectDisposedException>(() => ric.K = 4);
                Assert.Throws<ObjectDisposedException>(() => ric.SuperpixelSize);
                Assert.Throws<ObjectDisposedException>(() => ric.SuperpixelSize = 8);
                Assert.Throws<ObjectDisposedException>(() => ric.SuperpixelNNCount);
                Assert.Throws<ObjectDisposedException>(() => ric.SuperpixelNNCount = 8);
                Assert.Throws<ObjectDisposedException>(() => ric.SuperpixelRuler);
                Assert.Throws<ObjectDisposedException>(() => ric.SuperpixelRuler = 10.0F);
                Assert.Throws<ObjectDisposedException>(() => ric.SuperpixelMode);
                Assert.Throws<ObjectDisposedException>(() => ric.SuperpixelMode = SLICType.SLIC);
                Assert.Throws<ObjectDisposedException>(() => ric.Alpha);
                Assert.Throws<ObjectDisposedException>(() => ric.Alpha = 0.7F);
                Assert.Throws<ObjectDisposedException>(() => ric.ModelIter);
                Assert.Throws<ObjectDisposedException>(() => ric.ModelIter = 4);
                Assert.Throws<ObjectDisposedException>(() => ric.RefineModels);
                Assert.Throws<ObjectDisposedException>(() => ric.RefineModels = true);
                Assert.Throws<ObjectDisposedException>(() => ric.MaxFlow);
                Assert.Throws<ObjectDisposedException>(() => ric.MaxFlow = 250.0F);
                Assert.Throws<ObjectDisposedException>(() => ric.UseVariationalRefinement);
                Assert.Throws<ObjectDisposedException>(() => ric.UseVariationalRefinement = false);
                Assert.Throws<ObjectDisposedException>(() => ric.UseGlobalSmootherFilter);
                Assert.Throws<ObjectDisposedException>(() => ric.UseGlobalSmootherFilter = false);
                Assert.Throws<ObjectDisposedException>(() => ric.FGSLambda);
                Assert.Throws<ObjectDisposedException>(() => ric.FGSLambda = 500.0F);
                Assert.Throws<ObjectDisposedException>(() => ric.FGSSigma);
                Assert.Throws<ObjectDisposedException>(() => ric.FGSSigma = 1.5F);
                Assert.Throws<ObjectDisposedException>(() => ric.Interpolate(color, dst, color, dst, dst));
                Assert.Throws<ObjectDisposedException>(() => ric.SetCostMap(dst));

                SuperpixelSLIC slic = SuperpixelSLIC.Create(color, SLICType.SLICO, 8, 10.0F);
                slic.Dispose();
                Assert.True(slic.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => slic.NumberOfSuperpixels);
                Assert.Throws<ObjectDisposedException>(() => slic.Iterate(1));
                Assert.Throws<ObjectDisposedException>(() => slic.EnforceLabelConnectivity(10));
                Assert.Throws<ObjectDisposedException>(() => slic.GetLabels(dst));
                Assert.Throws<ObjectDisposedException>(() => slic.GetLabels());
                Assert.Throws<ObjectDisposedException>(() => slic.GetLabelContourMask(dst));
                Assert.Throws<ObjectDisposedException>(() => slic.GetLabelContourMask());

                SuperpixelSEEDS seeds = SuperpixelSEEDS.Create(color.Cols, color.Rows, color.Channels, 4, 2);
                seeds.Dispose();
                Assert.True(seeds.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => seeds.NumberOfSuperpixels);
                Assert.Throws<ObjectDisposedException>(() => seeds.Iterate(color, 1));
                Assert.Throws<ObjectDisposedException>(() => seeds.GetLabels(dst));
                Assert.Throws<ObjectDisposedException>(() => seeds.GetLabels());
                Assert.Throws<ObjectDisposedException>(() => seeds.GetLabelContourMask(dst));
                Assert.Throws<ObjectDisposedException>(() => seeds.GetLabelContourMask());

                SuperpixelLSC lsc = SuperpixelLSC.Create(color, 8, 0.075F);
                lsc.Dispose();
                Assert.True(lsc.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => lsc.NumberOfSuperpixels);
                Assert.Throws<ObjectDisposedException>(() => lsc.Iterate(1));
                Assert.Throws<ObjectDisposedException>(() => lsc.EnforceLabelConnectivity(10));
                Assert.Throws<ObjectDisposedException>(() => lsc.GetLabels(dst));
                Assert.Throws<ObjectDisposedException>(() => lsc.GetLabels());
                Assert.Throws<ObjectDisposedException>(() => lsc.GetLabelContourMask(dst));
                Assert.Throws<ObjectDisposedException>(() => lsc.GetLabelContourMask());
            }
        }

        private static Mat CreateGrayImage()
        {
            var image = new Mat(16, 16, MatType.CV_8UC1, new Scalar(96));
            ImgProcCv2.Rectangle(image, new Rect(4, 4, 8, 8), new Scalar(180), -1);
            return image;
        }

        private static Mat CreateColorImage()
        {
            var image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(24, 48, 72));
            ImgProcCv2.Rectangle(image, new Rect(4, 4, 10, 10), new Scalar(220, 40, 30), -1);
            ImgProcCv2.Rectangle(image, new Rect(18, 4, 10, 10), new Scalar(30, 200, 80), -1);
            ImgProcCv2.Circle(image, new Point(16, 23), 5, new Scalar(40, 80, 220), -1);
            return image;
        }

        private static Mat CreateDisparityMap()
        {
            var disparity = new Mat(16, 16, MatType.CV_16SC1, new Scalar(0));
            short[] values = new short[16 * 16];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    values[(y * 16) + x] = (short)((x + 1) * 16);
                }
            }

            disparity.CopyFrom(values);
            return disparity;
        }

        private static Mat CreateContourPointMat()
        {
            return Calib3DCv2.ToPointMat(new[]
            {
                new Point2f(0.0F, 0.0F),
                new Point2f(16.0F, 0.0F),
                new Point2f(16.0F, 16.0F),
                new Point2f(0.0F, 16.0F)
            });
        }

        private static Mat CreateComplexImage()
        {
            var complex = new Mat(4, 4, MatType.CV_32FC2, new Scalar(0));
            float[] values = new float[4 * 4 * 2];
            for (int i = 0; i < values.Length; i += 2)
            {
                values[i] = (i / 2) + 1.0F;
                values[i + 1] = 0.5F;
            }

            complex.CopyFrom(values);
            return complex;
        }

        private static Point2f[] CreateFromPoints()
        {
            return new[]
            {
                new Point2f(2.0F, 2.0F),
                new Point2f(13.0F, 2.0F),
                new Point2f(2.0F, 13.0F),
                new Point2f(13.0F, 13.0F)
            };
        }

        private static Point2f[] CreateToPoints()
        {
            return new[]
            {
                new Point2f(3.0F, 2.0F),
                new Point2f(14.0F, 2.0F),
                new Point2f(3.0F, 13.0F),
                new Point2f(14.0F, 13.0F)
            };
        }

        private static bool IsXImgProcModuleLinked()
        {
            try
            {
                using (FastLineDetector.Create(lengthThreshold: 6, cannyApertureSize: 3))
                {
                    return true;
                }
            }
            catch (OpenCvException ex) when (
                ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Assert.Contains("NOT_LINKED", ex.Message, StringComparison.OrdinalIgnoreCase);
                return false;
            }
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }

    }
}
