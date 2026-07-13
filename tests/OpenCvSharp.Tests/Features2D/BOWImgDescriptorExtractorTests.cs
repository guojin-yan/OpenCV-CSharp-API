using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class BOWImgDescriptorExtractorTests
    {
        [Fact]
        public void ConstructorAndVocabularyValidationRejectInvalidArguments()
        {
            using (var feature = new TestFeature2D())
            {
                Assert.Throws<ArgumentNullException>(() => new BOWImgDescriptorExtractor(feature, null!));
            }

            Assert.Throws<ArgumentNullException>(() => new BOWImgDescriptorExtractor((DescriptorMatcher)null!));
            Assert.Throws<ArgumentNullException>(() => new BOWImgDescriptorExtractor(null!, null!));
        }

        [Fact]
        public void PrecomputedDescriptorHistogramIsNormalizedWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            BFMatcher? matcher = TryCreateBFMatcher();
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            using (Mat vocabulary = Feature2DTestData.CreateFloatDescriptors(
                0.0F, 0.0F,
                10.0F, 10.0F))
            using (Mat query = Feature2DTestData.CreateFloatDescriptors(
                0.1F, 0.1F,
                9.9F, 10.1F,
                0.2F, 0.0F,
                10.2F, 9.8F))
            using (Mat histogram = new Mat())
            using (var extractor = new BOWImgDescriptorExtractor(matcher))
            {
                extractor.SetVocabulary(vocabulary);
                extractor.Compute(query, histogram, out int[][] clusters);

                Assert.Equal(2, extractor.DescriptorSize);
                Assert.Equal(MatType.CV_32FC1, extractor.DescriptorType);
                Assert.Equal(1, histogram.Rows);
                Assert.Equal(2, histogram.Cols);
                Assert.Equal(new float[] { 0.5F, 0.5F }, histogram.ToArray<float>());
                Assert.Equal(2, clusters.Length);
                Assert.Equal(new[] { 0, 2 }, clusters[0]);
                Assert.Equal(new[] { 1, 3 }, clusters[1]);

                using (Mat clone = extractor.GetVocabulary())
                {
                    Assert.Equal(vocabulary.ToArray<float>(), clone.ToArray<float>());
                    clone.SetTo(new Scalar(99.0));
                }

                using (Mat afterCloneMutation = extractor.GetVocabulary())
                {
                    Assert.Equal(vocabulary.ToArray<float>(), afterCloneMutation.ToArray<float>());
                }

                extractor.Clear();

                Assert.Equal(0, extractor.DescriptorSize);
                using (Mat clearedVocabulary = extractor.GetVocabulary())
                {
                    Assert.True(clearedVocabulary.Empty);
                }

                Assert.Throws<InvalidOperationException>(() => extractor.Compute(query, histogram));
            }
        }

        [Fact]
        public void ImageDescriptorComputeUsesSiftExtractorWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SIFT? sift = TryCreateSift();
            BFMatcher? matcher = TryCreateBFMatcher();
            if (sift == null || matcher == null)
            {
                matcher?.Dispose();
                sift?.Dispose();
                return;
            }

            using (sift)
            using (matcher)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            using (Mat bowDescriptor = new Mat())
            using (var extractor = new BOWImgDescriptorExtractor(sift, matcher))
            {
                KeyPoint[] keypoints = sift.DetectAndCompute(image, null, Array.Empty<KeyPoint>(), descriptors, useProvidedKeypoints: false);
                if (descriptors.Empty || descriptors.Rows < 2)
                {
                    return;
                }

                using (Mat vocabulary = descriptors.RowRange(0, Math.Min(2, descriptors.Rows)).Clone())
                using (Mat rawDescriptors = new Mat())
                {
                    extractor.SetVocabulary(vocabulary);
                    KeyPoint[] returned = extractor.Compute(image, keypoints, bowDescriptor, rawDescriptors);

                    Assert.NotNull(returned);
                    Assert.Equal(1, bowDescriptor.Rows);
                    Assert.Equal(vocabulary.Rows, bowDescriptor.Cols);
                    Assert.Equal(MatType.CV_32FC1, bowDescriptor.Type);
                    Assert.False(rawDescriptors.Empty);

#if NETCOREAPP3_1_OR_GREATER
                    using (Mat spanDescriptor = new Mat())
                    {
                        KeyPoint[] fromSpan = extractor.Compute(image, keypoints.AsSpan(), spanDescriptor);
                        Assert.NotNull(fromSpan);
                        Assert.Equal(vocabulary.Rows, spanDescriptor.Cols);
                    }
#endif
                }
            }
        }

        [Fact]
        public void EmptyKeypointsProduceEmptyDescriptorWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SIFT? sift = TryCreateSift();
            BFMatcher? matcher = TryCreateBFMatcher();
            if (sift == null || matcher == null)
            {
                matcher?.Dispose();
                sift?.Dispose();
                return;
            }

            using (sift)
            using (matcher)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat bowDescriptor = new Mat())
            using (var extractor = new BOWImgDescriptorExtractor(sift, matcher))
            {
                KeyPoint[] returned = extractor.Compute(image, Array.Empty<KeyPoint>(), bowDescriptor);

                Assert.Empty(returned);
                Assert.True(bowDescriptor.Empty);
            }
        }

        [Fact]
        public void CustomFeature2DDescriptorExtractorThrowsNotSupportedWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            BFMatcher? matcher = TryCreateBFMatcher();
            if (matcher == null)
            {
                return;
            }

            using (var feature = new TestFeature2D())
            using (matcher)
            using (var image = new Mat(1, 1, MatType.CV_8UC1))
            using (var vocabulary = Feature2DTestData.CreateFloatDescriptors(0.0F, 0.0F))
            using (var bowDescriptor = new Mat())
            using (var extractor = new BOWImgDescriptorExtractor(feature, matcher))
            {
                extractor.SetVocabulary(vocabulary);

                Assert.Throws<NotSupportedException>(() =>
                    extractor.Compute(image, new[] { new KeyPoint(0.0F, 0.0F, 1.0F) }, bowDescriptor));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<NotSupportedException>(() =>
                    extractor.Compute(image, new[] { new KeyPoint(0.0F, 0.0F, 1.0F) }.AsSpan(), bowDescriptor));
#endif
            }
        }

        [Fact]
        public void ManagedValidationRejectsInvalidStateWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            BFMatcher? matcher = TryCreateBFMatcher();
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            using (Mat query = Feature2DTestData.CreateFloatDescriptors(0.0F, 0.0F))
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat output = new Mat())
            using (Mat rawDescriptors = new Mat())
            using (Mat vocabulary = Feature2DTestData.CreateFloatDescriptors(0.0F, 0.0F))
            using (var extractor = new BOWImgDescriptorExtractor(matcher))
            {
                Assert.Throws<ArgumentNullException>(() => extractor.SetVocabulary(null!));
                Assert.Throws<InvalidOperationException>(() => extractor.Compute(query, output));

                extractor.Dispose();
                Assert.True(extractor.IsDisposed);
                Assert.Equal("{Disposed=True}", extractor.ToString());
                Assert.Throws<ObjectDisposedException>(() => extractor.DescriptorExtractor);
                Assert.Throws<ObjectDisposedException>(() => extractor.DescriptorMatcher);
                Assert.Throws<ObjectDisposedException>(() => extractor.DescriptorSize);
                Assert.Throws<ObjectDisposedException>(() => extractor.DescriptorType);
                Assert.Throws<ObjectDisposedException>(() => extractor.Vocabulary);
                Assert.Throws<ObjectDisposedException>(() => extractor.SetVocabulary(vocabulary));
                Assert.Throws<ObjectDisposedException>(() => extractor.GetVocabulary());
                Assert.Throws<ObjectDisposedException>(() => extractor.Clear());
                Assert.Throws<ObjectDisposedException>(() => extractor.Compute(image, Array.Empty<KeyPoint>(), output));
                Assert.Throws<ObjectDisposedException>(() => extractor.Compute(image, Array.Empty<KeyPoint>(), output, out int[][] _));

                KeyPoint[] keypoints = Array.Empty<KeyPoint>();
                Assert.Throws<ObjectDisposedException>(() => extractor.Compute(image, ref keypoints, output));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ObjectDisposedException>(() => extractor.Compute(image, ReadOnlySpan<KeyPoint>.Empty, output));
                Assert.Throws<ObjectDisposedException>(() => extractor.Compute(image, ReadOnlySpan<KeyPoint>.Empty, output, out int[][] _));
#endif
                Assert.Throws<ObjectDisposedException>(() => extractor.Compute(query, output));
                Assert.Throws<ObjectDisposedException>(() => extractor.Compute(query, output, normalize: false));
                Assert.Throws<ObjectDisposedException>(() => extractor.Compute(query, output, out int[][] _));
                Assert.Throws<ObjectDisposedException>(() => extractor.Compute(query, output, out int[][] _, normalize: false));
            }
        }

        private static BFMatcher? TryCreateBFMatcher()
        {
            try
            {
                return BFMatcher.Create(NormTypes.L2, crossCheck: false);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static SIFT? TryCreateSift()
        {
            try
            {
                return SIFT.Create(nFeatures: 128, descriptorType: MatType.CV_32F, enablePreciseUpscale: true);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private sealed class TestFeature2D : Feature2D
        {
            private bool disposed;

            public override bool IsDisposed
            {
                get { return disposed; }
            }

            public override bool Empty
            {
                get { return false; }
            }

            public override int DescriptorSize
            {
                get { return 2; }
            }

            public override int DescriptorType
            {
                get { return MatType.CV_32FC1; }
            }

            public override NormTypes DefaultNorm
            {
                get { return NormTypes.L2; }
            }

            public override string DefaultName
            {
                get { return "Feature2D.Test"; }
            }

            public override void Clear()
            {
                ThrowIfDisposed();
            }

            public override KeyPoint[] Detect(Mat image, Mat? mask = null)
            {
                ThrowIfDisposed();
                if (image == null)
                {
                    throw new ArgumentNullException(nameof(image));
                }

                return new[] { new KeyPoint(0.0F, 0.0F, 1.0F) };
            }

            public override void Dispose()
            {
                disposed = true;
            }

            private void ThrowIfDisposed()
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(nameof(TestFeature2D));
                }
            }
        }

    }
}
