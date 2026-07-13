using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class FlannBasedMatcherTests
    {
        [Fact]
        public void CreateExposesCollectionStateWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            FlannBasedMatcher? matcher = TryCreateMatcher();
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            {
                Assert.True(matcher.Empty);
                Assert.False(matcher.IsDisposed);
                Assert.Equal("{Empty=True,IsMaskSupported=False}", matcher.ToString());
            }
        }

        [Fact]
        public void MatchKnnAndRadiusReturnManagedMatchesWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            FlannBasedMatcher? matcher = TryCreateMatcher();
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            using (Mat query = Feature2DTestData.CreateFloatDescriptors(
                0.0F, 0.0F,
                10.0F, 10.0F))
            using (Mat train = Feature2DTestData.CreateFloatDescriptors(
                0.0F, 0.0F,
                9.0F, 9.0F,
                50.0F, 50.0F))
            {
                DMatch[] matches = matcher.Match(query, train);
                DMatch[][] knnMatches = matcher.KnnMatch(query, train, 2);
                DMatch[][] radiusMatches = matcher.RadiusMatch(query, train, 3.0F);

                Assert.Equal(2, matches.Length);
                Assert.Equal(2, knnMatches.Length);
                Assert.True(knnMatches[0].Length <= 2);
                Assert.Equal(2, radiusMatches.Length);
            }
        }

        [Fact]
        public void AddTrainAndCollectionMatchWorkWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            FlannBasedMatcher? matcher = TryCreateMatcher();
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            using (Mat query = Feature2DTestData.CreateFloatDescriptors(
                0.0F, 0.0F,
                10.0F, 10.0F))
            using (Mat train = Feature2DTestData.CreateFloatDescriptors(
                0.0F, 0.0F,
                9.0F, 9.0F))
            {
                matcher.Add(new[] { train });
                matcher.Train();
                Assert.False(matcher.Empty);

                DMatch[] matches = matcher.Match(query);
                DMatch[][] knnMatches = matcher.KnnMatch(query, 1);
                DMatch[][] radiusMatches = matcher.RadiusMatch(query, 3.0F);

                Assert.Equal(2, matches.Length);
                Assert.Equal(2, knnMatches.Length);
                Assert.Equal(2, radiusMatches.Length);

#if NETCOREAPP3_1_OR_GREATER
                matcher.Clear();
                matcher.Add(new ReadOnlySpan<Mat>(new[] { train }));
                matcher.Train();
                Assert.False(matcher.Empty);
#endif

                matcher.Clear();
                Assert.True(matcher.Empty);
            }
        }

        [Fact]
        public void DisposedMatcherRejectsUseWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            FlannBasedMatcher? matcher = TryCreateMatcher();
            if (matcher == null)
            {
                return;
            }

            matcher.Dispose();

            using (Mat descriptors = Feature2DTestData.CreateFloatDescriptors(0.0F, 0.0F))
            using (Mat mask = new Mat(1, 1, MatType.CV_8UC1, new Scalar(255)))
            {
                Assert.True(matcher.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => matcher.IsMaskSupported);
                Assert.Throws<ObjectDisposedException>(() => matcher.Empty);
                Assert.Throws<ObjectDisposedException>(() => matcher.Clone());
                Assert.Throws<ObjectDisposedException>(() => matcher.GetTrainDescriptors());
                Assert.Throws<ObjectDisposedException>(() => matcher.Add(new[] { descriptors }));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ObjectDisposedException>(() => matcher.Add(new ReadOnlySpan<Mat>(new[] { descriptors })));
#endif
                Assert.Throws<ObjectDisposedException>(() => matcher.Clear());
                Assert.Throws<ObjectDisposedException>(() => matcher.Train());
                Assert.Throws<ObjectDisposedException>(() => matcher.Match(descriptors, descriptors));
                Assert.Throws<ObjectDisposedException>(() => matcher.Match(descriptors));
                Assert.Throws<ObjectDisposedException>(() => matcher.Match(descriptors, new[] { mask }));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ObjectDisposedException>(() => matcher.Match(descriptors, new ReadOnlySpan<Mat>(new[] { mask })));
#endif
                Assert.Throws<ObjectDisposedException>(() => matcher.KnnMatch(descriptors, descriptors, 1));
                Assert.Throws<ObjectDisposedException>(() => matcher.KnnMatch(descriptors, 1));
                Assert.Throws<ObjectDisposedException>(() => matcher.KnnMatch(descriptors, 1, new[] { mask }));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ObjectDisposedException>(() => matcher.KnnMatch(descriptors, 1, new ReadOnlySpan<Mat>(new[] { mask })));
#endif
                Assert.Throws<ObjectDisposedException>(() => matcher.RadiusMatch(descriptors, descriptors, 1.0F));
                Assert.Throws<ObjectDisposedException>(() => matcher.RadiusMatch(descriptors, 1.0F));
                Assert.Throws<ObjectDisposedException>(() => matcher.RadiusMatch(descriptors, 1.0F, new[] { mask }));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ObjectDisposedException>(() => matcher.RadiusMatch(descriptors, 1.0F, new ReadOnlySpan<Mat>(new[] { mask })));
#endif
            }

            Assert.True(matcher.IsDisposed);
            Assert.Equal("{Disposed=True}", matcher.ToString());
        }

        private static FlannBasedMatcher? TryCreateMatcher()
        {
            try
            {
                return FlannBasedMatcher.Create();
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }
    }
}
