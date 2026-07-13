using System;
using OpenCvSharp.Core;
using OpenCvSharp.Video;

namespace OpenCvSharp.Tests.Video
{
    public sealed class BackgroundSubtractorTests
    {
        [Fact]
        public void BackgroundSubtractorManagedValidationThrows()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var subtractor = BackgroundSubtractorMOG2.Create())
            using (var image = new Mat(4, 4, MatType.CV_8UC1, new Scalar(0)))
            using (var mask = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => subtractor.Apply(null!, mask));
                Assert.Throws<ArgumentNullException>(() => subtractor.Apply(image, null!));
                Assert.Throws<ArgumentNullException>(() => subtractor.Apply(image, null!, mask));
                Assert.Throws<ArgumentNullException>(() => subtractor.Apply(null!));
                Assert.Throws<ArgumentNullException>(() => subtractor.ApplyWithKnownForeground(null!, mask));
                Assert.Throws<ArgumentNullException>(() => subtractor.ApplyWithKnownForeground(image, null!));
                Assert.Throws<ArgumentNullException>(() => subtractor.GetBackgroundImage(null!));
            }
        }

        [Fact]
        public void BackgroundSubtractorDisposedStateThrowsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = new Mat(4, 4, MatType.CV_8UC1, new Scalar(0)))
            using (var mask = new Mat())
            {
                var subtractor = BackgroundSubtractorKNN.Create();
                subtractor.Dispose();

                Assert.True(subtractor.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => subtractor.Apply(image, mask));
                Assert.Throws<ObjectDisposedException>(() => subtractor.Apply(image));
                Assert.Throws<ObjectDisposedException>(() => subtractor.ApplyWithKnownForeground(image, mask));
                Assert.Throws<ObjectDisposedException>(() => subtractor.GetBackgroundImage());
            }
        }

        [Fact]
        public void BackgroundSubtractorMOG2PropertiesRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var subtractor = BackgroundSubtractorMOG2.Create(history: 7, varThreshold: 12.0, detectShadows: false))
            {
                subtractor.History = 9;
                subtractor.NMixtures = 3;
                subtractor.DetectShadows = true;
                subtractor.ShadowValue = 120;
                subtractor.BackgroundRatio = 0.6;
                subtractor.VarThreshold = 14.0;
                subtractor.VarThresholdGen = 11.0;
                subtractor.VarInit = 16.0;
                subtractor.VarMin = 4.0;
                subtractor.VarMax = 70.0;
                subtractor.ComplexityReductionThreshold = 0.04;
                subtractor.ShadowThreshold = 0.45;

                Assert.Equal(9, subtractor.History);
                Assert.Equal(3, subtractor.NMixtures);
                Assert.True(subtractor.DetectShadows);
                Assert.Equal(120, subtractor.ShadowValue);
                Assert.Equal(0.6, subtractor.BackgroundRatio, 3);
                Assert.Equal(14.0, subtractor.VarThreshold, 3);
                Assert.Equal(11.0, subtractor.VarThresholdGen, 3);
                Assert.Equal(16.0, subtractor.VarInit, 3);
                Assert.Equal(4.0, subtractor.VarMin, 3);
                Assert.Equal(70.0, subtractor.VarMax, 3);
                Assert.Equal(0.04, subtractor.ComplexityReductionThreshold, 3);
                Assert.Equal(0.45, subtractor.ShadowThreshold, 3);
            }
        }

        [Fact]
        public void BackgroundSubtractorKNNPropertiesRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var subtractor = BackgroundSubtractorKNN.Create(history: 8, dist2Threshold: 300.0, detectShadows: false))
            {
                subtractor.History = 10;
                subtractor.NSamples = 12;
                subtractor.DetectShadows = true;
                subtractor.ShadowValue = 90;
                subtractor.KnnSamples = 4;
                subtractor.Dist2Threshold = 250.0;
                subtractor.ShadowThreshold = 0.35;

                Assert.Equal(10, subtractor.History);
                Assert.Equal(12, subtractor.NSamples);
                Assert.True(subtractor.DetectShadows);
                Assert.Equal(90, subtractor.ShadowValue);
                Assert.Equal(4, subtractor.KnnSamples);
                Assert.Equal(250.0, subtractor.Dist2Threshold, 3);
                Assert.Equal(0.35, subtractor.ShadowThreshold, 3);
            }
        }

        [Fact]
        public void BackgroundSubtractorApplyRunsOnTinyFramesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var mog2 = BackgroundSubtractorMOG2.Create())
            using (var knn = BackgroundSubtractorKNN.Create())
            using (var first = new Mat(8, 8, MatType.CV_8UC1, new Scalar(0)))
            using (var second = new Mat(8, 8, MatType.CV_8UC1, new Scalar(255)))
            using (var knownForeground = new Mat(8, 8, MatType.CV_8UC1, new Scalar(0)))
            using (var mog2Mask = mog2.Apply(first))
            using (var knnMask = knn.ApplyWithKnownForeground(second, knownForeground))
            using (var background = new Mat())
            {
                mog2.GetBackgroundImage(background);

                Assert.Equal(8, mog2Mask.Rows);
                Assert.Equal(8, mog2Mask.Cols);
                Assert.Equal(8, knnMask.Rows);
                Assert.Equal(8, knnMask.Cols);
            }
        }

    }
}
