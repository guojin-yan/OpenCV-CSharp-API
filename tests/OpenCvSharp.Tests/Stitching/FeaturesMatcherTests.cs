using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;
using OpenCvSharp.Stitching;

namespace OpenCvSharp.Tests.Stitching
{
    public sealed class FeaturesMatcherTests
    {
        [Fact]
        public void ConstructorValidationFailsClosedBeforeNativeCalls()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BestOf2NearestMatcher(matchConfidence: float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => BestOf2NearestMatcher.Create(numberOfMatchesThreshold1: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BestOf2NearestRangeMatcher(rangeWidth: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BestOf2NearestRangeMatcher(numberOfMatchesThreshold2: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AffineBestOf2NearestMatcher(matchConfidence: float.PositiveInfinity));
        }

        [Fact]
        public void ImageFeaturesOwnsCopiedValuesWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            KeyPoint[] keypoints = CreateKeypoints(0.0F, 0.0F);
            using (Mat descriptors = CreateDescriptors())
            {
                var features = new ImageFeatures(7, new Size(100, 80), keypoints, descriptors);
                descriptors.SetTo(new Scalar(99));
                keypoints[0] = new KeyPoint(90, 90, 1);

                using (Mat copied = features.GetDescriptors())
                {
                    Assert.Equal(7, features.ImageIndex);
                    Assert.Equal(new Size(100, 80), features.ImageSize);
                    Assert.Equal(10.0F, features.Keypoints[0].X);
                    Assert.Equal(0.0, copied.GetValue<float>(0), 6);

                    features.ImageIndex = 9;
                    features.ImageSize = new Size(120, 90);
                    Assert.Equal(9, features.ImageIndex);
                    Assert.Equal(new Size(120, 90), features.ImageSize);
                }

                features.Dispose();
                features.Dispose();
                Assert.True(features.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => features.GetDescriptors());
            }
        }

        [Fact]
        public void OrbComputesSingleBatchAndNonContinuousRoiWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (ORB orb = ORB.Create(maxFeatures: 100))
            using (Mat parent = OpenCvSharp.Tests.Features2D.Feature2DTestData.CreateFeatureImage())
            using (Mat padded = new Mat(100, 100, MatType.CV_8UC1, new Scalar(0)))
            using (Mat roi = padded.SubMat(new Rect(2, 2, 96, 96)))
            using (Mat mask = new Mat(96, 96, MatType.CV_8UC1, new Scalar(255)))
            {
                parent.CopyTo(roi);
                Assert.False(roi.IsContinuous);

                using (ImageFeatures single = ImageFeatures.Compute(orb, roi, mask))
                using (Mat descriptors = single.GetDescriptors())
                {
                    Assert.Equal(-1, single.ImageIndex);
                    Assert.Equal(new Size(96, 96), single.ImageSize);
                    Assert.NotEmpty(single.Keypoints);
                    Assert.Equal(single.Keypoints.Length, descriptors.Rows);
                    Assert.Equal(MatType.CV_8U, descriptors.Depth);
                }

                ImageFeatures[] batch = ImageFeatures.Compute(orb, new[] { parent, roi }, new[] { mask, mask });
                try
                {
                    Assert.Equal(2, batch.Length);
                    Assert.Equal(0, batch[0].ImageIndex);
                    Assert.Equal(1, batch[1].ImageIndex);
                    Assert.All(batch, item => Assert.Equal(new Size(96, 96), item.ImageSize));
                    Assert.All(batch, item => Assert.NotEmpty(item.Keypoints));
                }
                finally
                {
                    DisposeFeatures(batch);
                }
            }
        }

        [Fact]
        public void PairMatchingReturnsCopiedMatchesInliersAndHomographyWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (ImageFeatures first = CreateFeatures(10, 0.0F, 0.0F))
            using (ImageFeatures second = CreateFeatures(20, 5.0F, 3.0F))
            using (var matcher = new BestOf2NearestMatcher(matchConfidence: 0.8F))
            using (MatchesInfo result = matcher.Match(first, second))
            using (Mat homography = result.GetHomography())
            {
                Assert.True(matcher.IsThreadSafe);
                Assert.Equal(-1, result.SourceImageIndex);
                Assert.Equal(-1, result.DestinationImageIndex);
                Assert.Equal(8, result.Matches.Length);
                Assert.Equal(8, result.Inliers.Length);
                Assert.Equal(8, result.NumberOfInliers);
                Assert.Equal(new Size(3, 3), new Size(homography.Cols, homography.Rows));
                Assert.InRange(homography.GetValue<double>(2), 4.9, 5.1);
                Assert.InRange(homography.GetValue<double>(5), 2.9, 3.1);
                matcher.CollectGarbage();
            }
        }

        [Fact]
        public void BatchMaskAndRangeMatcherProduceExactRowMajorResultsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            ImageFeatures[] features =
            {
                CreateFeatures(0, 0.0F, 0.0F),
                CreateFeatures(1, 3.0F, 2.0F),
                CreateFeatures(2, 6.0F, 4.0F)
            };
            try
            {
                using (var range = new BestOf2NearestRangeMatcher(rangeWidth: 1, matchConfidence: 0.8F))
                {
                    MatchesInfo[] results = range.Match(features);
                    try
                    {
                        Assert.Equal(9, results.Length);
                        Assert.Equal(0, results[1].SourceImageIndex);
                        Assert.Equal(1, results[1].DestinationImageIndex);
                        Assert.Equal(1, results[3].SourceImageIndex);
                        Assert.Equal(0, results[3].DestinationImageIndex);
                        Assert.NotEmpty(results[1].Matches);
                        Assert.NotEmpty(results[5].Matches);
                        Assert.Empty(results[2].Matches);
                        Assert.Equal(-1, results[2].SourceImageIndex);
                    }
                    finally
                    {
                        DisposeMatches(results);
                    }
                }

                var maskValues = new byte[9];
                maskValues[2] = 255;
                using (var mask = new Mat(3, 3, MatType.CV_8UC1))
                using (var matcher = BestOf2NearestMatcher.Create(matchConfidence: 0.8F))
                {
                    mask.CopyFrom(maskValues);
                    MatchesInfo[] results = matcher.Match(features, mask);
                    try
                    {
                        Assert.Empty(results[1].Matches);
                        Assert.NotEmpty(results[2].Matches);
                        Assert.Equal(0, results[2].SourceImageIndex);
                        Assert.Equal(2, results[2].DestinationImageIndex);
                        Assert.NotEmpty(results[6].Matches);
                    }
                    finally
                    {
                        DisposeMatches(results);
                    }
                }
            }
            finally
            {
                DisposeFeatures(features);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AffineMatcherReturnsThreeByThreeTransformWhenNativeSmokeIsEnabled(bool fullAffine)
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (ImageFeatures first = CreateFeatures(0, 0.0F, 0.0F))
            using (ImageFeatures second = CreateFeatures(1, 4.0F, -2.0F))
            using (var matcher = new AffineBestOf2NearestMatcher(fullAffine, matchConfidence: 0.8F))
            using (MatchesInfo result = matcher.Match(first, second))
            using (Mat transform = result.GetHomography())
            {
                Assert.Equal(8, result.Matches.Length);
                Assert.Equal(8, result.Inliers.Length);
                Assert.Equal(new Size(3, 3), new Size(transform.Cols, transform.Rows));
            }
        }

        [Fact]
        public void ShapeTypeCollectionAndDisposalValidationFailClosedWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Mat wrongDescriptors = new Mat(8, 2, MatType.CV_16SC1, new Scalar(1)))
            using (Mat wrongRows = new Mat(7, 2, MatType.CV_32FC1, new Scalar(1)))
            using (ImageFeatures first = CreateFeatures(0, 0.0F, 0.0F))
            using (var matcher = new BestOf2NearestMatcher())
            using (var wrongMask = new Mat(2, 3, MatType.CV_8UC1, new Scalar(255)))
            {
                Assert.Throws<ArgumentException>(() => new ImageFeatures(0, new Size(10, 10), CreateKeypoints(0, 0), wrongDescriptors));
                Assert.Throws<ArgumentException>(() => new ImageFeatures(0, new Size(10, 10), CreateKeypoints(0, 0), wrongRows));
                Assert.Throws<ArgumentOutOfRangeException>(() => first.ImageSize = new Size(-1, 2));
                Assert.Throws<ArgumentNullException>(() => matcher.Match(null!, first));
                Assert.Throws<ArgumentException>(() => matcher.Match(Array.Empty<ImageFeatures>()));
                Assert.Throws<ArgumentException>(() => matcher.Match(new[] { first, first }, wrongMask));

                matcher.Dispose();
                matcher.Dispose();
                Assert.True(matcher.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => matcher.IsThreadSafe);
                Assert.Throws<ObjectDisposedException>(() => matcher.Match(first, first));
            }
        }

        private static ImageFeatures CreateFeatures(int imageIndex, float offsetX, float offsetY)
        {
            using (Mat descriptors = CreateDescriptors())
            {
                return new ImageFeatures(imageIndex, new Size(100, 80), CreateKeypoints(offsetX, offsetY), descriptors);
            }
        }

        private static KeyPoint[] CreateKeypoints(float offsetX, float offsetY)
        {
            return new[]
            {
                new KeyPoint(10 + offsetX, 10 + offsetY, 1),
                new KeyPoint(30 + offsetX, 10 + offsetY, 1),
                new KeyPoint(50 + offsetX, 10 + offsetY, 1),
                new KeyPoint(10 + offsetX, 30 + offsetY, 1),
                new KeyPoint(30 + offsetX, 30 + offsetY, 1),
                new KeyPoint(50 + offsetX, 30 + offsetY, 1),
                new KeyPoint(20 + offsetX, 50 + offsetY, 1),
                new KeyPoint(40 + offsetX, 50 + offsetY, 1)
            };
        }

        private static Mat CreateDescriptors()
        {
            var values = new float[8 * 4];
            for (int i = 0; i < 8; ++i)
            {
                values[i * 4] = i;
                values[i * 4 + 1] = i * i;
                values[i * 4 + 2] = (i % 3) * 10;
                values[i * 4 + 3] = i * 0.5F;
            }
            var descriptors = new Mat(8, 4, MatType.CV_32FC1);
            descriptors.CopyFrom(values);
            return descriptors;
        }

        private static void DisposeFeatures(ImageFeatures[] features)
        {
            foreach (ImageFeatures feature in features) feature.Dispose();
        }

        private static void DisposeMatches(MatchesInfo[] matches)
        {
            foreach (MatchesInfo match in matches) match.Dispose();
        }
    }
}
