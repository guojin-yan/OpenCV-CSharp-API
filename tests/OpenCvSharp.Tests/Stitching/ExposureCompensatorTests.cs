using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Stitching;

namespace JYPPX.OpenCvSharp.Tests.Stitching
{
    public sealed class ExposureCompensatorTests
    {
        [Fact]
        public void ExposureCompensatorTypeMatchesOpenCvValues()
        {
            Assert.Equal(0, (int)ExposureCompensatorType.None);
            Assert.Equal(1, (int)ExposureCompensatorType.Gain);
            Assert.Equal(2, (int)ExposureCompensatorType.GainBlocks);
            Assert.Equal(3, (int)ExposureCompensatorType.Channels);
            Assert.Equal(4, (int)ExposureCompensatorType.ChannelsBlocks);
            Assert.Throws<ArgumentOutOfRangeException>(() => ExposureCompensator.CreateDefault((ExposureCompensatorType)5));
        }

        [Fact]
        public void ConstructorsValidateArgumentsBeforeNativeExecution()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new GainCompensator(0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChannelsCompensator(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BlocksGainCompensator(0, 32, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BlocksGainCompensator(32, 0, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BlocksChannelsCompensator(32, 32, 0));
        }

        [Fact]
        public void FactoryAndPropertiesRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (ExposureCompensator none = ExposureCompensator.CreateDefault(ExposureCompensatorType.None))
            using (ExposureCompensator gainDefault = ExposureCompensator.CreateDefault(ExposureCompensatorType.Gain))
            using (ExposureCompensator gainBlocks = ExposureCompensator.CreateDefault(ExposureCompensatorType.GainBlocks))
            using (ExposureCompensator channelsDefault = ExposureCompensator.CreateDefault(ExposureCompensatorType.Channels))
            using (ExposureCompensator channelsBlocks = ExposureCompensator.CreateDefault(ExposureCompensatorType.ChannelsBlocks))
            using (var gain = new GainCompensator(2))
            using (var channels = new ChannelsCompensator(3))
            using (var blocks = new BlocksGainCompensator(16, 24, 2))
            using (var blockChannels = new BlocksChannelsCompensator())
            {
                Assert.IsType<NoExposureCompensator>(none);
                Assert.IsType<GainCompensator>(gainDefault);
                Assert.IsType<BlocksGainCompensator>(gainBlocks);
                Assert.IsType<ChannelsCompensator>(channelsDefault);
                Assert.IsType<BlocksChannelsCompensator>(channelsBlocks);

                gain.UpdateGain = false;
                gain.NumberOfFeeds = 4;
                gain.SimilarityThreshold = 0.75;
                Assert.False(gain.UpdateGain);
                Assert.Equal(4, gain.NumberOfFeeds);
                Assert.Equal(0.75, gain.SimilarityThreshold, 12);

                channels.NumberOfFeeds = 5;
                channels.SimilarityThreshold = 0.5;
                Assert.Equal(5, channels.NumberOfFeeds);
                Assert.Equal(0.5, channels.SimilarityThreshold, 12);

                blocks.BlockSize = new Size(20, 28);
                blocks.NumberOfFeeds = 3;
                blocks.SimilarityThreshold = 0.25;
                blocks.FilteringIterations = 4;
                Assert.Equal(new Size(20, 28), blocks.BlockSize);
                Assert.Equal(3, blocks.NumberOfFeeds);
                Assert.Equal(0.25, blocks.SimilarityThreshold, 12);
                Assert.Equal(4, blocks.FilteringIterations);
                Assert.Equal(new Size(32, 32), blockChannels.BlockSize);

                Assert.Throws<ArgumentOutOfRangeException>(() => gain.NumberOfFeeds = 0);
                Assert.Throws<ArgumentOutOfRangeException>(() => gain.SimilarityThreshold = double.NaN);
                Assert.Throws<ArgumentOutOfRangeException>(() => blocks.BlockSize = new Size(0, 1));
                Assert.Throws<ArgumentOutOfRangeException>(() => blocks.FilteringIterations = -1);
            }
        }

        [Fact]
        public void FeedApplyAndGainOwnershipAreDeterministicWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var first = new Mat(24, 24, MatType.CV_8UC3, new Scalar(40, 40, 40)))
            using (var second = new Mat(24, 24, MatType.CV_8UC3, new Scalar(80, 80, 80)))
            using (var firstMask = new Mat(24, 24, MatType.CV_8UC1, new Scalar(255)))
            using (var secondMask = new Mat(24, 24, MatType.CV_8UC1, new Scalar(255)))
            using (var compensator = new GainCompensator())
            {
                var corners = new[] { new Point(0, 0), new Point(0, 0) };
                compensator.Feed(corners, new[] { first, second }, new[] { firstMask, secondMask });
                Mat[] gains = compensator.GetMatGains();
                try
                {
                    Assert.Equal(2, gains.Length);
                    Assert.All(gains, gain => Assert.Equal(MatType.CV_64FC1, gain.Type));
                    compensator.Apply(0, corners[0], first, firstMask);
                    compensator.Apply(1, corners[1], second, secondMask);
                    double firstMean = Cv2.Mean(first).V0;
                    double secondMean = Cv2.Mean(second).V0;
                    Assert.False(double.IsNaN(firstMean) || double.IsInfinity(firstMean));
                    Assert.False(double.IsNaN(secondMean) || double.IsInfinity(secondMean));
                    Assert.InRange(Math.Abs(firstMean - secondMean), 0.0, 39.0);

                    compensator.SetMatGains(gains);
                    compensator.Dispose();
                    Assert.All(gains, gain => Assert.False(gain.IsDisposed));
                    Assert.All(gains, gain => Assert.False(gain.Empty));
                }
                finally
                {
                    foreach (Mat gain in gains) gain.Dispose();
                }
            }
        }

        [Fact]
        public void ValidationAndDisposalAreFailClosedWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var image = new Mat(8, 8, MatType.CV_8UC3, new Scalar(10, 10, 10)))
            using (var mask = new Mat(8, 8, MatType.CV_8UC1, new Scalar(255)))
            using (var wrongMask = new Mat(7, 8, MatType.CV_8UC1, new Scalar(255)))
            using (var wrongType = new Mat(8, 8, MatType.CV_8UC3, new Scalar(255, 255, 255)))
            {
                var compensator = new NoExposureCompensator();
                Assert.Throws<ArgumentNullException>(() => compensator.Feed(null!, new[] { image }, new[] { mask }));
                Assert.Throws<ArgumentException>(() => compensator.Feed(Array.Empty<Point>(), Array.Empty<Mat>(), Array.Empty<Mat>()));
                Assert.Throws<ArgumentException>(() => compensator.Feed(new[] { new Point() }, new[] { image }, Array.Empty<Mat>()));
                Assert.Throws<ArgumentException>(() => compensator.Feed(new[] { new Point() }, new[] { image }, new[] { wrongMask }));
                Assert.Throws<ArgumentException>(() => compensator.Apply(0, new Point(), image, wrongType));
                Assert.Throws<ArgumentOutOfRangeException>(() => compensator.Apply(-1, new Point(), image, mask));
                Assert.Throws<ArgumentNullException>(() => compensator.SetMatGains(null!));

                compensator.Dispose();
                compensator.Dispose();
                Assert.True(compensator.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => compensator.UpdateGain);
                Assert.Throws<ObjectDisposedException>(() => compensator.Feed(new[] { new Point() }, new[] { image }, new[] { mask }));
                Assert.Throws<ObjectDisposedException>(() => compensator.Apply(0, new Point(), image, mask));
                Assert.Throws<ObjectDisposedException>(() => compensator.GetMatGains());
            }
        }
    }
}
