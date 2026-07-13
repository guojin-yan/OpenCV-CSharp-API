using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class AffineFeatureTests
    {
        [Fact]
        public void CreateFromOrbExposesFeature2DMetadataAndDetectsWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            ORB? orb = TryCreateOrb();
            if (orb == null)
            {
                return;
            }

            using (orb)
            using (AffineFeature? affine = TryCreateAffine(orb))
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            {
                if (affine == null)
                {
                    return;
                }

                Assert.Same(orb, affine.Backend);
                Assert.False(affine.IsDisposed);
                Assert.True(affine.DescriptorSize >= 0);
                Assert.True(affine.DescriptorType >= 0);
                Assert.Equal(orb.DefaultNorm, affine.DefaultNorm);
                Assert.False(string.IsNullOrWhiteSpace(affine.DefaultName));
                Assert.Contains("Affine", affine.DefaultName, StringComparison.OrdinalIgnoreCase);

                affine.SetViewParams(new[] { 1.0F }, new[] { 0.0F });
                KeyPoint[] keypoints = affine.Detect(image);

                Assert.NotNull(keypoints);
                Assert.Equal("{DefaultName=" + affine.DefaultName + ",ViewCount=1,Backend=ORB}", affine.ToString());
            }
        }

        [Fact]
        public void CreateSupportsTypedBackendsWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            using (SIFT? sift = TryCreateSift())
            using (FastFeatureDetector? fast = TryCreateFast())
            using (GFTTDetector? gftt = TryCreateGftt())
            using (MSER? mser = TryCreateMser())
            using (SimpleBlobDetector? simpleBlob = TryCreateSimpleBlob())
            using (BRISK? brisk = TryCreateBrisk())
            using (KAZE? kaze = TryCreateKaze())
            using (AKAZE? akaze = TryCreateAkaze())
            {
                AssertCanCreateAffine(sift, "SIFT");
                AssertCanCreateAffine(fast, "Fast");
                AssertCanCreateAffine(gftt, "GFTT");
                AssertCanCreateAffine(mser, "MSER");
                AssertCanCreateAffine(simpleBlob, "SimpleBlob");
                AssertCanCreateAffine(brisk, "BRISK");
                AssertCanCreateAffine(kaze, "KAZE");
                AssertCanCreateAffine(akaze, "AKAZE");
            }
        }

        [Fact]
        public void ViewParametersRoundTripWithArraysWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            ORB? orb = TryCreateOrb();
            if (orb == null)
            {
                return;
            }

            using (orb)
            using (AffineFeature? affine = TryCreateAffine(orb))
            {
                if (affine == null)
                {
                    return;
                }

                var tilts = new[] { 1.0F, 1.41421356F, 2.0F };
                var rolls = new[] { 0.0F, 45.0F, 90.0F };

                affine.SetViewParams(tilts, rolls);
                affine.GetViewParams(out float[] returnedTilts, out float[] returnedRolls);

                Assert.Equal(3, affine.ViewCount);
                AssertFloatArraysEqual(tilts, returnedTilts);
                AssertFloatArraysEqual(rolls, returnedRolls);

                affine.SetViewParams(Array.Empty<float>(), Array.Empty<float>());
                affine.GetViewParams(out returnedTilts, out returnedRolls);

                Assert.Equal(0, affine.ViewCount);
                Assert.Empty(returnedTilts);
                Assert.Empty(returnedRolls);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        [Fact]
        public void ViewParametersRoundTripWithSpansWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            ORB? orb = TryCreateOrb();
            if (orb == null)
            {
                return;
            }

            using (orb)
            using (AffineFeature? affine = TryCreateAffine(orb))
            {
                if (affine == null)
                {
                    return;
                }

                ReadOnlySpan<float> tilts = stackalloc float[] { 1.0F, 2.0F };
                ReadOnlySpan<float> rolls = stackalloc float[] { 0.0F, 60.0F };
                Span<float> returnedTilts = stackalloc float[2];
                Span<float> returnedRolls = stackalloc float[2];

                affine.SetViewParams(tilts, rolls);
                int written = affine.GetViewParams(returnedTilts, returnedRolls);

                Assert.Equal(2, written);
                Assert.Equal(1.0F, returnedTilts[0], 5);
                Assert.Equal(2.0F, returnedTilts[1], 5);
                Assert.Equal(0.0F, returnedRolls[0], 5);
                Assert.Equal(60.0F, returnedRolls[1], 5);
                Assert.Throws<ArgumentException>(() => GetViewParamsWithSmallTiltSpan(affine));
                Assert.Throws<ArgumentException>(() => SetMismatchedSpanViewParams(affine));
            }
        }
#endif

        [Fact]
        public void CreateValidatesUnsupportedAndNullBackends()
        {
            Assert.Throws<ArgumentNullException>(() => AffineFeature.Create((Feature2D)null!));
            Assert.Throws<ArgumentNullException>(() => AffineFeature.Create((ORB)null!));
            Assert.Throws<NotSupportedException>(() => AffineFeature.Create(new TestFeature2D()));
        }

        [Fact]
        public void AffineFeatureValidatesArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            ORB? orb = TryCreateOrb();
            if (orb == null)
            {
                return;
            }

            using (orb)
            using (AffineFeature? affine = TryCreateAffine(orb))
            {
                if (affine == null)
                {
                    return;
                }

                Assert.Throws<ArgumentNullException>(() => affine.SetViewParams(null!, Array.Empty<float>()));
                Assert.Throws<ArgumentNullException>(() => affine.SetViewParams(Array.Empty<float>(), null!));
                Assert.Throws<ArgumentException>(() => affine.SetViewParams(new[] { 1.0F }, Array.Empty<float>()));
                Assert.Throws<ArgumentNullException>(() => affine.Detect(null!));
            }

            orb = TryCreateOrb();
            if (orb == null)
            {
                return;
            }

            orb.Dispose();
            Assert.Throws<ObjectDisposedException>(() => AffineFeature.Create(orb));
        }

        [Fact]
        public void DisposedAffineFeatureRejectsUseWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            ORB? orb = TryCreateOrb();
            if (orb == null)
            {
                return;
            }

            using (orb)
            {
                AffineFeature? affine = TryCreateAffine(orb);
                if (affine == null)
                {
                    return;
                }

                affine.Dispose();

                using (Mat image = Feature2DTestData.CreateFeatureImage())
                {
                    Assert.Throws<ObjectDisposedException>(() => affine.Empty);
                    Assert.Throws<ObjectDisposedException>(() => affine.DescriptorSize);
                    Assert.Throws<ObjectDisposedException>(() => affine.DescriptorType);
                    Assert.Throws<ObjectDisposedException>(() => affine.DefaultNorm);
                    Assert.Throws<ObjectDisposedException>(() => affine.ViewCount);
                    Assert.Throws<ObjectDisposedException>(() => affine.Detect(image));
                }

                Assert.True(affine.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => affine.Backend);
                Assert.Throws<ObjectDisposedException>(() => affine.DefaultName);
                Assert.Throws<ObjectDisposedException>(() => affine.Clear());
                Assert.Throws<ObjectDisposedException>(() => affine.SetViewParams(new[] { 1.0F }, new[] { 0.0F }));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ObjectDisposedException>(() => affine.SetViewParams(ReadOnlySpan<float>.Empty, ReadOnlySpan<float>.Empty));
#endif
                Assert.Throws<ObjectDisposedException>(() => affine.GetViewParams(out _, out _));
#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ObjectDisposedException>(() => GetViewParamsWithSingleElementSpans(affine));
#endif
                Assert.Equal("{Disposed=True}", affine.ToString());
            }
        }

        private static void AssertCanCreateAffine(Feature2D? backend, string expectedBackendName)
        {
            if (backend == null)
            {
                return;
            }

            using (AffineFeature? affine = TryCreateAffine(backend))
            {
                if (affine == null)
                {
                    return;
                }

                Assert.Same(backend, affine.Backend);
                Assert.False(string.IsNullOrWhiteSpace(affine.DefaultName));
                Assert.Contains("Affine", affine.DefaultName, StringComparison.OrdinalIgnoreCase);
                Assert.Contains(expectedBackendName, affine.Backend.DefaultName, StringComparison.OrdinalIgnoreCase);
            }
        }

        private static AffineFeature? TryCreateAffine(Feature2D backend)
        {
            try
            {
                return AffineFeature.Create(backend, maxTilt: 1, minTilt: 0, tiltStep: 1.41421356F, rotateStepBase: 72.0F);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static ORB? TryCreateOrb()
        {
            try
            {
                return ORB.Create(maxFeatures: 64, fastThreshold: 8);
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
                return SIFT.Create(nFeatures: 64);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static FastFeatureDetector? TryCreateFast()
        {
            try
            {
                return FastFeatureDetector.Create(threshold: 12);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static GFTTDetector? TryCreateGftt()
        {
            try
            {
                return GFTTDetector.Create(maxCorners: 16, qualityLevel: 0.01, minDistance: 2.0);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static MSER? TryCreateMser()
        {
            try
            {
                return MSER.Create(delta: 5, minArea: 20, maxArea: 22000);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static SimpleBlobDetector? TryCreateSimpleBlob()
        {
            try
            {
                return SimpleBlobDetector.Create(new SimpleBlobDetectorParams
                {
                    ThresholdStep = 5.0F,
                    MinThreshold = 0.0F,
                    MaxThreshold = 255.0F,
                    MinRepeatability = 1,
                    MinDistBetweenBlobs = 5.0F,
                    FilterByColor = true,
                    BlobColor = 0,
                    FilterByArea = true,
                    MinArea = 20.0F,
                    MaxArea = 2500.0F,
                    FilterByCircularity = false,
                    FilterByInertia = false,
                    FilterByConvexity = false
                });
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static BRISK? TryCreateBrisk()
        {
            try
            {
                return BRISK.Create(threshold: 24, octaves: 2);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static KAZE? TryCreateKaze()
        {
            try
            {
                return KAZE.Create(nOctaves: 3, nOctaveLayers: 3);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static AKAZE? TryCreateAkaze()
        {
            try
            {
                return AKAZE.Create(nOctaves: 3, nOctaveLayers: 3, maxPoints: 128);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static void AssertFloatArraysEqual(float[] expected, float[] actual)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.Equal(expected[i], actual[i], 5);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void GetViewParamsWithSmallTiltSpan(AffineFeature affine)
        {
            Span<float> tilts = stackalloc float[1];
            Span<float> rolls = stackalloc float[2];
            affine.GetViewParams(tilts, rolls);
        }

        private static void SetMismatchedSpanViewParams(AffineFeature affine)
        {
            ReadOnlySpan<float> tilts = stackalloc float[] { 1.0F };
            ReadOnlySpan<float> rolls = stackalloc float[] { 0.0F, 1.0F };
            affine.SetViewParams(tilts, rolls);
        }

        private static void GetViewParamsWithSingleElementSpans(AffineFeature affine)
        {
            Span<float> tilts = stackalloc float[1];
            Span<float> rolls = stackalloc float[1];
            affine.GetViewParams(tilts, rolls);
        }
#endif

        private sealed class TestFeature2D : Feature2D
        {
            public override bool IsDisposed
            {
                get { return false; }
            }

            public override bool Empty
            {
                get { return false; }
            }

            public override int DescriptorSize
            {
                get { return 0; }
            }

            public override int DescriptorType
            {
                get { return 0; }
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
            }

            public override KeyPoint[] Detect(Mat image, Mat? mask = null)
            {
                if (image == null)
                {
                    throw new ArgumentNullException(nameof(image));
                }

                return Array.Empty<KeyPoint>();
            }

            public override void Dispose()
            {
            }
        }
    }
}
