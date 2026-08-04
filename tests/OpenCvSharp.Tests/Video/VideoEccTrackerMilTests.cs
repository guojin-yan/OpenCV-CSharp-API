using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Video;
using Xunit;
using VideoCv2 = JYPPX.OpenCvSharp.Video.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Video
{
    public sealed class VideoEccTrackerMilTests
    {
        [Fact]
        public void EccDefaultsMatchLinkedOpenCv()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            var local = new ECCParameters();
            ECCParameters native = ECCParameters.GetDefaultFromNative();
            Assert.Equal(MotionType.Affine, local.MotionType);
            Assert.Equal(local.MotionType, native.MotionType);
            Assert.Equal(local.Criteria, native.Criteria);
            Assert.Empty(native.IterationsPerLevel);
            Assert.Equal(5, native.GaussianFilterSize);
            Assert.Equal(4, native.LevelCount);
            Assert.Equal(InterpolationFlags.Linear, native.Interpolation);
        }

        [Fact]
        public void ComputeAndSingleScaleEccSupportOwnedAndCallerWarp()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat image = CreateEccImage())
            using (Mat callerWarp = new Mat())
            {
                double correlation = VideoCv2.ComputeECC(image, image);
                Assert.Equal(1.0, correlation, 5);

                double score = VideoCv2.FindTransformECC(
                    image,
                    image,
                    callerWarp,
                    MotionType.Translation,
                    TermCriteria.ByCountAndEpsilon(20, 1e-6));
                Assert.InRange(score, 0.999, 1.001);
                Assert.Equal(MatType.CV_32FC1, callerWarp.Type);
                Assert.Equal(2, callerWarp.Rows);
                Assert.Equal(3, callerWarp.Cols);

                using ECCRegistrationResult owned = VideoCv2.FindTransformECC(
                    image,
                    image,
                    MotionType.Homography,
                    TermCriteria.ByCountAndEpsilon(20, 1e-6));
                Assert.InRange(owned.Score, 0.999, 1.001);
                Assert.Equal(MatType.CV_32FC1, owned.WarpMatrix.Type);
                Assert.Equal(3, owned.WarpMatrix.Rows);
                Assert.Equal(3, owned.WarpMatrix.Cols);
            }
        }

        [Fact]
        public void DualMaskAndMultiscaleEccUseExactMaskAndScheduleContracts()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat image = CreateEccImage())
            using (Mat mask = new Mat(64, 64, MatType.CV_8UC1, new Scalar(255)))
            using (Mat dualWarp = new Mat())
            {
                double dualScore = VideoCv2.FindTransformECCWithMask(
                    image,
                    image,
                    mask,
                    mask,
                    dualWarp,
                    MotionType.Translation,
                    TermCriteria.ByCountAndEpsilon(20, 1e-6));
                Assert.InRange(dualScore, 0.999, 1.001);

                var parameters = new ECCParameters(
                    MotionType.Translation,
                    TermCriteria.ByCountAndEpsilon(20, 1e-6),
                    new[] { 4, 4, 4, 4 },
                    gaussianFilterSize: 5,
                    levelCount: 4,
                    interpolation: InterpolationFlags.Linear);
                using ECCRegistrationResult multiscale = VideoCv2.FindTransformECCMultiScale(
                    image,
                    image,
                    parameters,
                    mask,
                    mask);
                Assert.InRange(multiscale.Score, 0.999, 1.001);
                Assert.Equal(MatType.CV_64FC1, multiscale.WarpMatrix.Type);
                Assert.Equal(2, multiscale.WarpMatrix.Rows);
                Assert.Equal(3, multiscale.WarpMatrix.Cols);
            }
        }

        [Fact]
        public void EccValidationRejectsUnsafeShapesAndParameters()
        {
            using (Mat image = CreateEccImage())
            using (Mat wrongMask = new Mat(8, 8, MatType.CV_8UC1))
            using (Mat wrongWarp = new Mat(2, 3, MatType.CV_64FC1))
            using (Mat warp = new Mat())
            {
                Assert.Throws<ArgumentException>(() => VideoCv2.ComputeECC(image, wrongMask));
                Assert.Throws<ArgumentException>(() => VideoCv2.ComputeECC(image, image, wrongMask));
                Assert.Throws<ArgumentException>(() => VideoCv2.FindTransformECC(image, image, wrongWarp));
                Assert.Throws<ArgumentOutOfRangeException>(() => VideoCv2.FindTransformECC(image, image, warp, gaussianFilterSize: 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => new ECCParameters(levelCount: 0));
                Assert.Throws<ArgumentException>(() => new ECCParameters(iterationsPerLevel: new[] { 1, 2 }, levelCount: 4));
                Assert.Throws<ArgumentOutOfRangeException>(() => new ECCParameters(interpolation: InterpolationFlags.Cubic));
            }
        }

        [Fact]
        public void TrackerMilDefaultsMatchLinkedOpenCv()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            TrackerMILParams local = TrackerMILParams.Default;
            TrackerMILParams native = TrackerMILParams.GetDefaultFromNative();
            Assert.Equal(local, native);
            Assert.Equal(3.0F, native.SamplerInitInRadius);
            Assert.Equal(65, native.SamplerInitMaxNegNum);
            Assert.Equal(25.0F, native.SamplerSearchWinSize);
            Assert.Equal(250, native.FeatureSetNumFeatures);
        }

        [Fact]
        public void TrackerMilLifecycleUpdateAndDisposalAreDeterministic()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat first = CreateTrackingFrame(0))
            using (Mat second = CreateTrackingFrame(2))
            {
                var parameters = new TrackerMILParams(3.0F, 20, 25.0F, 4.0F, 100, 20, 250);
                var tracker = TrackerMIL.Create(parameters);
                var box = new Rect(20, 22, 20, 20);
                Assert.False(tracker.IsInitialized);
                Assert.Equal(-1.0F, tracker.TrackingScore);
                Assert.Throws<InvalidOperationException>(() => tracker.Update(second, ref box));

                tracker.Init(first, box);
                Assert.True(tracker.IsInitialized);
                bool found = tracker.Update(second, ref box);
                if (found)
                {
                    Assert.True(box.Width > 0);
                    Assert.True(box.Height > 0);
                    Assert.InRange(box.X, 0, second.Cols - 1);
                    Assert.InRange(box.Y, 0, second.Rows - 1);
                }
                Assert.Equal(-1.0F, tracker.TrackingScore);

                tracker.Dispose();
                tracker.Dispose();
                Assert.True(tracker.IsDisposed);
                Assert.False(tracker.IsInitialized);
                Assert.Throws<ObjectDisposedException>(() => _ = tracker.TrackingScore);
                Assert.Throws<ObjectDisposedException>(() => tracker.Init(first, new Rect(20, 22, 20, 20)));
            }
        }

        [Fact]
        public void TrackerMilRejectsInvalidParametersRectanglesAndDefaultStruct()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new TrackerMILParams(3.0F, 0, 25.0F, 4.0F, 100, 65, 250));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TrackerMILParams(float.NaN, 65, 25.0F, 4.0F, 100, 65, 250));
            Assert.Throws<ArgumentOutOfRangeException>(() => TrackerMIL.Create(default));

            if (!TestEnvironment.IsNativeSmokeEnabled()) return;
            using (Mat image = CreateTrackingFrame(0))
            using (TrackerMIL tracker = TrackerMIL.Create(new TrackerMILParams(3.0F, 20, 15.0F, 4.0F, 100, 20, 20)))
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => tracker.Init(image, new Rect(-1, 0, 20, 20)));
                Assert.Throws<ArgumentOutOfRangeException>(() => tracker.Init(image, new Rect(70, 70, 20, 20)));
                Assert.Throws<ArgumentOutOfRangeException>(() => tracker.Init(image, new Rect(10, 10, 0, 20)));
            }
        }

        private static Mat CreateEccImage()
        {
            var image = new Mat(64, 64, MatType.CV_8UC1);
            var data = new byte[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    int value = (x * 7 + y * 11 + ((x / 5 + y / 7) & 1) * 80) & 255;
                    data[(y * 64) + x] = (byte)value;
                }
            }
            image.CopyFrom(data);
            return image;
        }

        private static Mat CreateTrackingFrame(int offset)
        {
            var frame = new Mat(80, 80, MatType.CV_8UC1);
            var data = new byte[80 * 80];
            for (int y = 0; y < 80; y++)
            {
                for (int x = 0; x < 80; x++)
                {
                    data[(y * 80) + x] = (byte)((x * 3 + y * 5) & 31);
                }
            }
            for (int y = 22; y < 42; y++)
            {
                for (int x = 20 + offset; x < 40 + offset; x++)
                {
                    data[(y * 80) + x] = (byte)(((x + y) & 1) == 0 ? 240 : 96);
                }
            }
            frame.CopyFrom(data);
            return frame;
        }
    }
}
