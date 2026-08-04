using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace JYPPX.OpenCvSharp.Tests.Features2D
{
    public class Feature2DDefaultNameTests
    {
        [Fact]
        public void DefaultNameIsExposedForImplementedFeatureDetectorsWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            using (ORB? orb = TryCreateOrb())
            using (SIFT? sift = TryCreateSift())
            using (FastFeatureDetector? fast = TryCreateFast())
            using (GFTTDetector? gftt = TryCreateGftt())
            using (MSER? mser = TryCreateMser())
            using (SimpleBlobDetector? simpleBlob = TryCreateSimpleBlob())
            using (BRISK? brisk = TryCreateBrisk())
            using (KAZE? kaze = TryCreateKaze())
            using (AKAZE? akaze = TryCreateAkaze())
            {
                AssertDefaultName(orb, "ORB");
                AssertDefaultName(sift, "SIFT");
                AssertDefaultName(fast, "Fast");
                AssertDefaultName(gftt, "GFTT");
                AssertDefaultName(mser, "MSER");
                AssertDefaultName(simpleBlob, "SimpleBlob");
                AssertDefaultName(brisk, "BRISK");
                AssertDefaultName(kaze, "KAZE");
                AssertDefaultName(akaze, "AKAZE");
            }
        }

        [Fact]
        public void DefaultNameWorksThroughFeature2DBaseWhenNativeRuntimeIsAvailable()
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
                Feature2D feature = orb;
                string defaultName = feature.DefaultName;

                Assert.False(string.IsNullOrWhiteSpace(defaultName));
                Assert.Contains("ORB", defaultName, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void DefaultNameRejectsDisposedFeatureWhenNativeRuntimeIsAvailable()
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

            orb.Dispose();

            Assert.Throws<ObjectDisposedException>(() => orb.DefaultName);
        }

        private static void AssertDefaultName(Feature2D? feature, string expectedFragment)
        {
            if (feature == null)
            {
                return;
            }

            string defaultName = feature.DefaultName;
            Assert.False(string.IsNullOrWhiteSpace(defaultName));
            Assert.Contains(expectedFragment, defaultName, StringComparison.OrdinalIgnoreCase);
            Assert.Contains(expectedFragment, feature.ToString() + defaultName, StringComparison.OrdinalIgnoreCase);
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
                return SimpleBlobDetector.Create(CreateStableBlobParams());
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

        private static SimpleBlobDetectorParams CreateStableBlobParams()
        {
            return new SimpleBlobDetectorParams
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
            };
        }
    }
}
