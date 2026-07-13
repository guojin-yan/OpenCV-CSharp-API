using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class BFMatcherTests
    {
        [Fact]
        public void CreateRejectsUnsupportedNormTypeBeforeNativeCall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BFMatcher.Create(NormTypes.Inf));
            Assert.Throws<ArgumentOutOfRangeException>(() => BFMatcher.Create(NormTypes.MinMax));
            Assert.Throws<ArgumentOutOfRangeException>(() => BFMatcher.Create(NormTypes.Relative | NormTypes.L2));
            Assert.Throws<ArgumentOutOfRangeException>(() => BFMatcher.Create((NormTypes)99));
        }

        [Fact]
        public void CreateReportsDefinedBoundaryWhenFeaturesModuleIsNotLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            OpenCvException? exception = Record.Exception(() =>
            {
                using (BFMatcher matcher = BFMatcher.Create())
                {
                    Assert.False(matcher.IsDisposed);
                }
            }) as OpenCvException;

            if (exception != null)
            {
                Assert.Contains("features2d_bf_matcher_create", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("OpenCV", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void PropertiesExposeConstructorSettingsWhenFeaturesModuleIsLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            BFMatcher? matcher = TryCreateMatcher(NormTypes.L2, crossCheck: true);
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            {
                Assert.Equal(NormTypes.L2, matcher.NormType);
                Assert.True(matcher.CrossCheck);
                Assert.True(matcher.IsMaskSupported);
                Assert.True(matcher.Empty);
                Assert.Equal("{NormType=L2,CrossCheck=True}", matcher.ToString());
            }
        }

        [Fact]
        public void MatchKnnAndRadiusReturnManagedMatchesWhenFeaturesModuleIsLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            BFMatcher? matcher = TryCreateMatcher(NormTypes.L2, crossCheck: false);
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            using (Mat query = CreateFloatDescriptors(new float[]
            {
                0.0F, 0.0F,
                10.0F, 10.0F
            }))
            using (Mat train = CreateFloatDescriptors(new float[]
            {
                0.0F, 0.0F,
                9.0F, 9.0F,
                50.0F, 50.0F
            }))
            {
                DMatch[] matches = matcher.Match(query, train);
                DMatch[][] knnMatches = matcher.KnnMatch(query, train, 2);
                DMatch[][] radiusMatches = matcher.RadiusMatch(query, train, 3.0F);

                Assert.Equal(2, matches.Length);
                Assert.Equal(0, matches[0].QueryIdx);
                Assert.Equal(0, matches[0].TrainIdx);
                Assert.Equal(2, knnMatches.Length);
                Assert.True(knnMatches[0].Length <= 2);
                Assert.Equal(2, radiusMatches.Length);
                Assert.True(radiusMatches[0].Length >= 1);
            }
        }

        [Fact]
        public void AddTrainAndCollectionMatchWorkWhenFeaturesModuleIsLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            BFMatcher? matcher = TryCreateMatcher(NormTypes.L2, crossCheck: false);
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            using (Mat query = CreateFloatDescriptors(new float[]
            {
                0.0F, 0.0F,
                10.0F, 10.0F
            }))
            using (Mat train = CreateFloatDescriptors(new float[]
            {
                0.0F, 0.0F,
                9.0F, 9.0F
            }))
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

                matcher.Clear();
                Assert.True(matcher.Empty);
            }
        }

        [Fact]
        public void ManagedValidationRejectsInvalidDescriptorInputs()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            BFMatcher? matcher = TryCreateMatcher(NormTypes.L2, crossCheck: false);
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            {
                Assert.Throws<ArgumentNullException>(() => matcher.Add(null!));
            }
        }

        [Fact]
        public void DisposedMatcherRejectsUseWhenFeaturesModuleIsLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            BFMatcher? matcher = TryCreateMatcher(NormTypes.L2, crossCheck: false);
            if (matcher == null)
            {
                return;
            }

            matcher.Dispose();

            using (Mat descriptors = CreateFloatDescriptors(new float[]
            {
                0.0F, 0.0F
            }))
            using (Mat mask = new Mat(1, 1, MatType.CV_8UC1, new Scalar(255)))
            {
                Assert.True(matcher.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => matcher.NormType);
                Assert.Throws<ObjectDisposedException>(() => matcher.CrossCheck);
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

        private static BFMatcher? TryCreateMatcher(NormTypes normType, bool crossCheck)
        {
            try
            {
                return BFMatcher.Create(normType, crossCheck);
            }
            catch (OpenCvException ex) when (IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static Mat CreateFloatDescriptors(float[] values)
        {
            Mat descriptors = new Mat(values.Length / 2, 2, MatType.CV_32FC1);
            descriptors.CopyFrom(values);
            return descriptors;
        }

        private static bool IsFeaturesModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("features2d", StringComparison.OrdinalIgnoreCase) >= 0
                && exception.Message.IndexOf("OpenCV", StringComparison.OrdinalIgnoreCase) >= 0;
        }

    }
}
