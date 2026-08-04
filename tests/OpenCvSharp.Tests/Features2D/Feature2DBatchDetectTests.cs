using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace JYPPX.OpenCvSharp.Tests.Features2D
{
    public class Feature2DBatchDetectTests
    {
        [Fact]
        public void BatchDetectWorksForOrB()
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
                Mat[] images = Feature2DTestData.CreateBatchFeatureImages();
                try
                {
                    KeyPoint[][] keypoints = orb.Detect(images);
                    Assert.Equal(images.Length, keypoints.Length);
                    Assert.True(keypoints[0].Length >= 0);
                }
                finally
                {
                    DisposeAll(images);
                }
            }
        }

        [Fact]
        public void BatchDetectWorksForSiftFastAndGftt()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            using (SIFT? sift = TryCreateSift())
            using (FastFeatureDetector? fast = TryCreateFast())
            using (GFTTDetector? gftt = TryCreateGftt())
            {
                Mat[] images = Feature2DTestData.CreateBatchFeatureImages();
                try
                {
                    if (sift != null)
                    {
                        KeyPoint[][] siftKeypoints = sift.Detect(images);
                        Assert.Equal(images.Length, siftKeypoints.Length);
                    }

                    if (fast != null)
                    {
                        KeyPoint[][] fastKeypoints = fast.Detect(images);
                        Assert.Equal(images.Length, fastKeypoints.Length);
                    }

                    if (gftt != null)
                    {
                        KeyPoint[][] gfttKeypoints = gftt.Detect(images);
                        Assert.Equal(images.Length, gfttKeypoints.Length);
                    }
                }
                finally
                {
                    DisposeAll(images);
                }
            }
        }

        [Fact]
        public void BatchDetectWorksForSimpleBlobDetector()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SimpleBlobDetector? detector = TryCreateSimpleBlobDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            {
                Mat[] images = new[]
                {
                    Feature2DTestData.CreateBlobImage(),
                    Feature2DTestData.CreateBlobImage()
                };

                try
                {
                    KeyPoint[][] keypoints = detector.Detect(images);
                    Assert.Equal(images.Length, keypoints.Length);
                    Assert.True(keypoints[0].Length >= 1);
                    Assert.True(keypoints[1].Length >= 1);
                }
                finally
                {
                    DisposeAll(images);
                }
            }
        }

        [Fact]
        public void BatchDetectWorksForXFeatures2DDetectorsWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            using (BRISK? brisk = TryCreateBrisk())
            using (KAZE? kaze = TryCreateKaze())
            using (AKAZE? akaze = TryCreateAkaze())
            {
                Mat[] images = Feature2DTestData.CreateBatchFeatureImages();
                try
                {
                    AssertBatchDetect(brisk, images);
                    AssertBatchDetect(kaze, images);
                    AssertBatchDetect(akaze, images);
                }
                finally
                {
                    DisposeAll(images);
                }
            }
        }

        [Fact]
        public void BatchDetectValidatesArguments()
        {
            using (TestFeature2D detector = new TestFeature2D())
            {
                Assert.Throws<ArgumentNullException>(() => detector.Detect((Mat[])null!));
                Assert.Throws<ArgumentException>(() => detector.Detect(Array.Empty<Mat>()));
                Assert.Throws<ArgumentNullException>(() => detector.Detect(new Mat[] { null! }));

                using (Mat image = new Mat(16, 16, MatType.CV_8UC1))
                using (Mat mask1 = new Mat(16, 16, MatType.CV_8UC1))
                using (Mat mask2 = new Mat(16, 16, MatType.CV_8UC1))
                {
                    Assert.Throws<ArgumentException>(() => detector.Detect(new[] { image }, new[] { mask1, mask2 }));
                }

#if NETCOREAPP3_1_OR_GREATER
                Assert.Throws<ArgumentException>(() => detector.Detect(ReadOnlySpan<Mat>.Empty));
#endif
            }
        }

        [Fact]
        public void BatchDetectRejectsDisposedDetectors()
        {
            TestFeature2D detector = new TestFeature2D();
            detector.Dispose();

            using (Mat image = Feature2DTestData.CreateFeatureImage())
            {
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(new[] { image }));
            }
        }

        [Fact]
        public void MserDetectRegionsWorksWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            MSER? mser = TryCreateMser();
            if (mser == null)
            {
                return;
            }

            using (mser)
            using (Mat image = Feature2DTestData.CreateMserImage())
            {
                bool isEmpty = mser.Empty;
                Assert.True(isEmpty || !isEmpty);
                Assert.True(mser.DescriptorSize >= 0);
                Assert.True(mser.DescriptorType >= 0);

                mser.Delta = 6;
                mser.MinArea = 40;
                mser.MaxArea = 20000;
                mser.MaxVariation = 0.3;
                mser.MinDiversity = 0.25;
                mser.MaxEvolution = 180;
                mser.AreaThreshold = 1.02;
                mser.MinMargin = 0.004;
                mser.EdgeBlurSize = 3;
                mser.Pass2Only = true;

                Assert.Equal(6, mser.Delta);
                Assert.Equal(40, mser.MinArea);
                Assert.Equal(20000, mser.MaxArea);
                Assert.Equal(0.3, mser.MaxVariation, 6);
                Assert.Equal(0.25, mser.MinDiversity, 6);
                Assert.Equal(180, mser.MaxEvolution);
                Assert.Equal(1.02, mser.AreaThreshold, 6);
                Assert.Equal(0.004, mser.MinMargin, 6);
                Assert.Equal(3, mser.EdgeBlurSize);
                Assert.True(mser.Pass2Only);

                KeyPoint[] keypoints = mser.Detect(image);
                MserRegion[] regions = mser.DetectRegions(image);

                Assert.True(keypoints.Length >= 0);
                Assert.NotNull(regions);
                for (int i = 0; i < regions.Length; i++)
                {
                    Assert.True(regions[i].PointCount >= 0);
                    Assert.True(regions[i].BoundingBox.Width >= 0);
                    Assert.True(regions[i].BoundingBox.Height >= 0);
                }

                Assert.Contains("Delta=6", mser.ToString(), StringComparison.Ordinal);
                Assert.Contains("MaxVariation=0.3", mser.ToString(), StringComparison.Ordinal);
                Assert.Contains("MinDiversity=0.25", mser.ToString(), StringComparison.Ordinal);
                Assert.Contains("AreaThreshold=1.02", mser.ToString(), StringComparison.Ordinal);
                Assert.Contains("MinMargin=0.004", mser.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void MserToStringFormatsFloatingPointSettingsInvariantlyWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                MSER? mser = TryCreateMser();
                if (mser == null)
                {
                    return;
                }

                using (mser)
                {
                    mser.MaxVariation = 0.3;
                    mser.MinDiversity = 0.25;
                    mser.AreaThreshold = 1.02;
                    mser.MinMargin = 0.004;
                    string formatted = mser.ToString();

                    Assert.Contains("MaxVariation=0.3", formatted, StringComparison.Ordinal);
                    Assert.Contains("MinDiversity=0.25", formatted, StringComparison.Ordinal);
                    Assert.Contains("AreaThreshold=1.02", formatted, StringComparison.Ordinal);
                    Assert.Contains("MinMargin=0.004", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("MaxVariation=0,3", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("MinDiversity=0,25", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("AreaThreshold=1,02", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("MinMargin=0,004", formatted, StringComparison.Ordinal);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        private static ORB? TryCreateOrb()
        {
            try
            {
                return ORB.Create(maxFeatures: 128, fastThreshold: 8);
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
                return FastFeatureDetector.Create();
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
                return GFTTDetector.Create();
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
                return MSER.Create();
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static SimpleBlobDetector? TryCreateSimpleBlobDetector()
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

        private static void AssertBatchDetect(Feature2D? detector, Mat[] images)
        {
            if (detector == null)
            {
                return;
            }

            KeyPoint[][] keypoints = detector.Detect(images);
            Assert.Equal(images.Length, keypoints.Length);
            Assert.True(keypoints[0].Length >= 0);
            Assert.True(keypoints[1].Length >= 0);
        }

        private static void DisposeAll(Mat[] images)
        {
            for (int i = 0; i < images.Length; i++)
            {
                images[i].Dispose();
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
                ThrowIfDisposed();
            }

            public override KeyPoint[] Detect(Mat image, Mat? mask = null)
            {
                ThrowIfDisposed();
                if (image == null)
                {
                    throw new ArgumentNullException(nameof(image));
                }

                return new[] { new KeyPoint(1.0F, 1.0F, 1.0F) };
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
