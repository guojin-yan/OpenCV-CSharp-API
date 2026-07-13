using System;
using System.Globalization;
using OpenCvSharp.BgSegm;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace OpenCvSharp.Tests.BgSegm
{
    public sealed class BgSegmTests
    {
        [Fact]
        public void FactoriesValidateManagedArguments()
        {
            using (var mat = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => SyntheticSequenceGenerator.Create(null!, mat));
                Assert.Throws<ArgumentNullException>(() => SyntheticSequenceGenerator.Create(mat, null!));
            }

            using (var background = new Mat(8, 8, MatType.CV_8UC3, new Scalar(16, 24, 32)))
            using (var obj = new Mat(4, 4, MatType.CV_8UC3, new Scalar(220, 40, 30)))
            using (var empty = new Mat())
            using (var twoChannelBackground = new Mat(8, 8, MatType.CV_8UC2))
            using (var twoChannelObject = new Mat(4, 4, MatType.CV_8UC2))
            using (var sameWidthObject = new Mat(4, 8, MatType.CV_8UC3))
            using (var sameHeightObject = new Mat(8, 4, MatType.CV_8UC3))
            {
                Assert.Throws<ArgumentException>(() => SyntheticSequenceGenerator.Create(empty, obj));
                Assert.Throws<ArgumentException>(() => SyntheticSequenceGenerator.Create(background, empty));
                Assert.Throws<ArgumentException>(() => SyntheticSequenceGenerator.Create(twoChannelBackground, obj));
                Assert.Throws<ArgumentException>(() => SyntheticSequenceGenerator.Create(background, twoChannelObject));
                Assert.Throws<ArgumentException>(() => SyntheticSequenceGenerator.Create(background, sameWidthObject));
                Assert.Throws<ArgumentException>(() => SyntheticSequenceGenerator.Create(background, sameHeightObject));
            }

            Assert.Throws<ArgumentOutOfRangeException>(() => BackgroundSubtractorGMG.Create(initializationFrames: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => BackgroundSubtractorCNT.Create(minPixelStability: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => BackgroundSubtractorCNT.Create(minPixelStability: 4, maxPixelStability: 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => BackgroundSubtractorCNT.Create(minPixelStability: 5, maxPixelStability: 4));
        }

        [Fact]
        public void ObjectPropertySmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var mog = BackgroundSubtractorMOG.Create(history: 20, nmixtures: 3, backgroundRatio: 0.6, noiseSigma: 0.0))
            using (var gmg = BackgroundSubtractorGMG.Create(initializationFrames: 4, decisionThreshold: 0.7))
            using (var cnt = BackgroundSubtractorCNT.Create(minPixelStability: 2, useHistory: true, maxPixelStability: 8, isParallel: false))
            {
                mog.History = 12;
                mog.NMixtures = 4;
                mog.BackgroundRatio = 0.5;
                mog.NoiseSigma = 0.2;

                gmg.MaxFeatures = 32;
                gmg.NumFrames = 3;
                gmg.DecisionThreshold = 0.65;
                gmg.UpdateBackgroundModel = true;

                cnt.MinPixelStability = 3;
                cnt.MaxPixelStability = 10;
                cnt.UseHistory = false;
                cnt.IsParallel = false;

                Assert.Equal(12, mog.History);
                Assert.Equal(4, mog.NMixtures);
                Assert.Equal(0.5, mog.BackgroundRatio, 3);
                Assert.Equal(3, gmg.NumFrames);
                Assert.Equal(0.65, gmg.DecisionThreshold, 3);
                Assert.Equal(3, cnt.MinPixelStability);
                Assert.False(cnt.UseHistory);
                Assert.Contains("BackgroundRatio=0.5", mog.ToString(), StringComparison.Ordinal);
                Assert.Contains("NoiseSigma=0.2", mog.ToString(), StringComparison.Ordinal);
                Assert.Contains("DecisionThreshold=0.65", gmg.ToString(), StringComparison.Ordinal);
                Assert.Contains("MinPixelStability=3", cnt.ToString(), StringComparison.Ordinal);
                Assert.Contains("MaxPixelStability=10", cnt.ToString(), StringComparison.Ordinal);
                Assert.Contains("UseHistory=False", cnt.ToString(), StringComparison.Ordinal);
                Assert.Contains("IsParallel=False", cnt.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void BackgroundSubtractorPropertiesRoundTripAllSettingsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var mog = BackgroundSubtractorMOG.Create(history: 20, nmixtures: 3, backgroundRatio: 0.6, noiseSigma: 0.0))
            using (var gmg = BackgroundSubtractorGMG.Create(initializationFrames: 4, decisionThreshold: 0.7))
            using (var cnt = BackgroundSubtractorCNT.Create(minPixelStability: 2, useHistory: true, maxPixelStability: 8, isParallel: false))
            {
                mog.History = 13;
                mog.NMixtures = 4;
                mog.BackgroundRatio = 0.55;
                mog.NoiseSigma = 0.25;

                gmg.MaxFeatures = 48;
                gmg.DefaultLearningRate = 0.12;
                gmg.NumFrames = 5;
                gmg.QuantizationLevels = 16;
                gmg.BackgroundPrior = 0.35;
                gmg.SmoothingRadius = 3;
                gmg.DecisionThreshold = 0.62;
                gmg.UpdateBackgroundModel = false;
                gmg.MinVal = 7.0;
                gmg.MaxVal = 211.0;

                cnt.MinPixelStability = 4;
                cnt.MaxPixelStability = 11;
                cnt.UseHistory = false;
                cnt.IsParallel = true;

                Assert.Equal(13, mog.History);
                Assert.Equal(4, mog.NMixtures);
                Assert.Equal(0.55, mog.BackgroundRatio, 3);
                Assert.Equal(0.25, mog.NoiseSigma, 3);

                Assert.Equal(48, gmg.MaxFeatures);
                Assert.Equal(0.12, gmg.DefaultLearningRate, 3);
                Assert.Equal(5, gmg.NumFrames);
                Assert.Equal(16, gmg.QuantizationLevels);
                Assert.Equal(0.35, gmg.BackgroundPrior, 3);
                Assert.Equal(3, gmg.SmoothingRadius);
                Assert.Equal(0.62, gmg.DecisionThreshold, 3);
                Assert.False(gmg.UpdateBackgroundModel);
                Assert.Equal(7.0, gmg.MinVal, 3);
                Assert.Equal(211.0, gmg.MaxVal, 3);

                Assert.Equal(4, cnt.MinPixelStability);
                Assert.Equal(11, cnt.MaxPixelStability);
                Assert.False(cnt.UseHistory);
                Assert.True(cnt.IsParallel);
            }
        }

        [Fact]
        public void BackgroundSubtractorGmgValidatesInitializationSettingsBeforeApply()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var gmg = BackgroundSubtractorGMG.Create(initializationFrames: 4, decisionThreshold: 0.7))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => gmg.MaxFeatures = 0);
                Assert.Throws<ArgumentOutOfRangeException>(() => gmg.DefaultLearningRate = -0.1);
                Assert.Throws<ArgumentOutOfRangeException>(() => gmg.DefaultLearningRate = 1.1);
                Assert.Throws<ArgumentOutOfRangeException>(() => gmg.DefaultLearningRate = double.NaN);
                Assert.Throws<ArgumentOutOfRangeException>(() => gmg.NumFrames = 0);
                Assert.Throws<ArgumentOutOfRangeException>(() => gmg.QuantizationLevels = 0);
                Assert.Throws<ArgumentOutOfRangeException>(() => gmg.QuantizationLevels = 256);
                Assert.Throws<ArgumentOutOfRangeException>(() => gmg.BackgroundPrior = -0.1);
                Assert.Throws<ArgumentOutOfRangeException>(() => gmg.BackgroundPrior = 1.1);
                Assert.Throws<ArgumentOutOfRangeException>(() => gmg.BackgroundPrior = double.NaN);
            }

            using (var cnt = BackgroundSubtractorCNT.Create(minPixelStability: 2, maxPixelStability: 6))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => cnt.MinPixelStability = 0);
                Assert.Throws<ArgumentOutOfRangeException>(() => cnt.MinPixelStability = 6);
                Assert.Throws<ArgumentOutOfRangeException>(() => cnt.MaxPixelStability = 2);
            }
        }

        [Fact]
        public void BackgroundSubtractorToStringFormatsFloatingPointSettingsInvariantlyWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                using (var mog = BackgroundSubtractorMOG.Create(history: 20, nmixtures: 3, backgroundRatio: 0.6, noiseSigma: 0.2))
                using (var gmg = BackgroundSubtractorGMG.Create(initializationFrames: 4, decisionThreshold: 0.65))
                {
                    string mogFormatted = mog.ToString();
                    string gmgFormatted = gmg.ToString();

                    Assert.Contains("BackgroundRatio=0.6", mogFormatted, StringComparison.Ordinal);
                    Assert.Contains("NoiseSigma=0.2", mogFormatted, StringComparison.Ordinal);
                    Assert.Contains("DecisionThreshold=0.65", gmgFormatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("BackgroundRatio=0,6", mogFormatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("NoiseSigma=0,2", mogFormatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("DecisionThreshold=0,65", gmgFormatted, StringComparison.Ordinal);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void BackgroundSubtractorManagedValidationThrowsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = CreateFrame(0))
            using (var knownForeground = new Mat(image.Rows, image.Cols, MatType.CV_8UC1, new Scalar(0)))
            using (var mask = new Mat())
            using (var subtractor = BackgroundSubtractorCNT.Create(minPixelStability: 2, maxPixelStability: 6))
            {
                Assert.Throws<ArgumentNullException>(() => subtractor.Apply(null!, mask));
                Assert.Throws<ArgumentNullException>(() => subtractor.Apply(image, null!));
                Assert.Throws<ArgumentNullException>(() => subtractor.Apply(null!, knownForeground, mask));
                Assert.Throws<ArgumentNullException>(() => subtractor.Apply(image, null!, mask));
                Assert.Throws<ArgumentNullException>(() => subtractor.Apply(image, knownForeground, null!));
                Assert.Throws<ArgumentNullException>(() => subtractor.ApplyWithKnownForeground(null!, knownForeground));
                Assert.Throws<ArgumentNullException>(() => subtractor.ApplyWithKnownForeground(image, null!));
                Assert.Throws<ArgumentNullException>(() => subtractor.GetBackgroundImage(null!));
            }
        }

        [Fact]
        public void BackgroundSubtractorApplyValidatesImageFormatBeforeNativeCallWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = CreateFrame(0))
            using (var floatImage = new Mat(image.Rows, image.Cols, MatType.CV_32FC1, new Scalar(0)))
            using (var fourChannelImage = new Mat(image.Rows, image.Cols, MatType.CV_8UC4, new Scalar(0)))
            using (var knownForeground = new Mat(image.Rows, image.Cols, MatType.CV_8UC1, new Scalar(0)))
            using (var mask = new Mat())
            using (var mog = BackgroundSubtractorMOG.Create(history: 10, nmixtures: 3))
            using (var cnt = BackgroundSubtractorCNT.Create(minPixelStability: 2, maxPixelStability: 6))
            {
                Assert.Throws<ArgumentException>(() => mog.Apply(floatImage, mask));
                Assert.Throws<ArgumentException>(() => mog.Apply(fourChannelImage, mask));
                Assert.Throws<ArgumentException>(() => mog.Apply(floatImage));
                Assert.Throws<ArgumentException>(() => mog.Apply(floatImage, knownForeground, mask));
                Assert.Throws<ArgumentException>(() => mog.ApplyWithKnownForeground(floatImage, knownForeground));

                Assert.Throws<ArgumentException>(() => cnt.Apply(floatImage, mask));
                Assert.Throws<ArgumentException>(() => cnt.Apply(floatImage));
                Assert.Throws<ArgumentException>(() => cnt.Apply(floatImage, knownForeground, mask));
                Assert.Throws<ArgumentException>(() => cnt.ApplyWithKnownForeground(floatImage, knownForeground));
            }
        }

        [Fact]
        public void BackgroundSubtractorDisposedStateThrowsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = CreateFrame(0))
            using (var knownForeground = new Mat(image.Rows, image.Cols, MatType.CV_8UC1, new Scalar(0)))
            using (var mask = new Mat())
            {
                var subtractor = BackgroundSubtractorCNT.Create(minPixelStability: 2, maxPixelStability: 6);
                subtractor.Dispose();

                Assert.True(subtractor.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => subtractor.Apply(image, mask));
                Assert.Throws<ObjectDisposedException>(() => subtractor.Apply(image));
                Assert.Throws<ObjectDisposedException>(() => subtractor.Apply(image, knownForeground, mask));
                Assert.Throws<ObjectDisposedException>(() => subtractor.ApplyWithKnownForeground(image, knownForeground));
                Assert.Throws<ObjectDisposedException>(() => subtractor.GetBackgroundImage());
                Assert.Equal("{Disposed=True}", subtractor.ToString());
            }
        }

        [Fact]
        public void ApplySmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var first = CreateFrame(2))
            using (var second = CreateFrame(6))
            using (var mask = new Mat())
            using (var background = new Mat())
            using (var knownForeground = new Mat(first.Rows, first.Cols, MatType.CV_8UC1, new Scalar(0)))
            using (var mog = BackgroundSubtractorMOG.Create(history: 10, nmixtures: 3))
            using (var cnt = BackgroundSubtractorCNT.Create(minPixelStability: 2, maxPixelStability: 6))
            {
                mog.Apply(first, mask, 1.0);
                mog.Apply(second, mask, 0.5);
                cnt.Apply(first, mask, 1.0);
                cnt.Apply(second, mask, 0.5);
                using (var knownForegroundMask = cnt.ApplyWithKnownForeground(second, knownForeground, 0.5))
                {
                    Assert.False(knownForegroundMask.Empty);
                    Assert.Equal(first.Rows, knownForegroundMask.Rows);
                    Assert.Equal(first.Cols, knownForegroundMask.Cols);
                }

                cnt.GetBackgroundImage(background);

                Assert.False(mask.Empty);
                Assert.False(background.Empty);
            }
        }

        [Fact]
        public void SyntheticSequenceGeneratorSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var background = new Mat(32, 32, MatType.CV_8UC3, new Scalar(16, 24, 32)))
            using (var obj = new Mat(8, 8, MatType.CV_8UC3, new Scalar(220, 40, 30)))
            using (var frame = new Mat())
            using (var gtMask = new Mat())
            using (var generator = SyntheticSequenceGenerator.Create(background, obj))
            {
                generator.GetNextFrame(frame, gtMask);

                Assert.False(frame.Empty);
                Assert.False(gtMask.Empty);
                Assert.Equal(background.Rows, frame.Rows);
                Assert.Equal(background.Cols, frame.Cols);
            }
        }

        [Fact]
        public void SyntheticSequenceGeneratorValidatesManagedArgumentsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var background = new Mat(32, 32, MatType.CV_8UC3, new Scalar(16, 24, 32)))
            using (var obj = new Mat(8, 8, MatType.CV_8UC3, new Scalar(220, 40, 30)))
            using (var frame = new Mat())
            using (var gtMask = new Mat())
            using (var generator = SyntheticSequenceGenerator.Create(background, obj))
            {
                Assert.Throws<ArgumentNullException>(() => generator.GetNextFrame(null!, gtMask));
                Assert.Throws<ArgumentNullException>(() => generator.GetNextFrame(frame, null!));
            }
        }

        [Fact]
        public void SyntheticSequenceGeneratorThrowsAfterDisposeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var background = new Mat(32, 32, MatType.CV_8UC3, new Scalar(16, 24, 32)))
            using (var obj = new Mat(8, 8, MatType.CV_8UC3, new Scalar(220, 40, 30)))
            using (var frame = new Mat())
            using (var gtMask = new Mat())
            {
                var generator = SyntheticSequenceGenerator.Create(background, obj);
                generator.Dispose();

                Assert.True(generator.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => generator.GetNextFrame(frame, gtMask));
            }
        }

        private static Mat CreateFrame(int offset)
        {
            var frame = new Mat(24, 24, MatType.CV_8UC3, new Scalar(20, 40, 60));
            ImgProcCv2.Rectangle(frame, new Rect(4 + offset, 5, 8, 9), new Scalar(200, 30, 80), -1);
            return frame;
        }

    }
}
