using System;
using System.Globalization;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class KazeTests
    {
        [Fact]
        public void CreateReportsDefinedBoundaryWhenXFeatures2DModuleIsNotLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            OpenCvException? exception = Record.Exception(() =>
            {
                using (KAZE kaze = KAZE.Create())
                {
                    Assert.False(kaze.IsDisposed);
                }
            }) as OpenCvException;

            if (exception != null)
            {
                Assert.Contains("features2d_kaze_create", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("OpenCV", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void DiffusivityEnumValuesMatchOpenCvConstants()
        {
            Assert.Equal(0, (int)KazeDiffusivityType.DiffPmG1);
            Assert.Equal(1, (int)KazeDiffusivityType.DiffPmG2);
            Assert.Equal(2, (int)KazeDiffusivityType.DiffWeickert);
            Assert.Equal(3, (int)KazeDiffusivityType.DiffCharbonnier);
        }

        [Fact]
        public void CreateRejectsInvalidDiffusivityBeforeNativeCall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => KAZE.Create(diffusivity: (KazeDiffusivityType)99));
        }

        [Fact]
        public void ToStringFormatsFloatingValuesInvariantlyWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            KAZE? kaze = TryCreateKaze();
            if (kaze == null)
            {
                return;
            }

            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                using (kaze)
                {
                    kaze.Threshold = 0.002;

                    Assert.Contains("Threshold=0.002", kaze.ToString(), StringComparison.Ordinal);
                    Assert.DoesNotContain("Threshold=0,002", kaze.ToString(), StringComparison.Ordinal);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void PropertiesRoundTripWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            KAZE? kaze = TryCreateKaze();
            if (kaze == null)
            {
                return;
            }

            using (kaze)
            {
                Assert.NotNull(kaze.Empty.ToString());
                Assert.True(kaze.DescriptorSize >= 0);
                Assert.True(kaze.DescriptorType >= 0);
                Assert.True((int)kaze.DefaultNorm >= 0);
                Assert.Contains("KAZE", kaze.DefaultName, StringComparison.OrdinalIgnoreCase);

                kaze.Extended = true;
                kaze.Upright = true;
                kaze.Threshold = 0.002;
                kaze.NOctaves = 3;
                kaze.NOctaveLayers = 3;
                kaze.Diffusivity = KazeDiffusivityType.DiffCharbonnier;

                Assert.True(kaze.Extended);
                Assert.True(kaze.Upright);
                Assert.Equal(0.002, kaze.Threshold, 6);
                Assert.Equal(3, kaze.NOctaves);
                Assert.Equal(3, kaze.NOctaveLayers);
                Assert.Equal(KazeDiffusivityType.DiffCharbonnier, kaze.Diffusivity);
                Assert.Contains("Threshold=0.002", kaze.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void DiffusivitySetterRejectsInvalidValueWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            KAZE? kaze = TryCreateKaze();
            if (kaze == null)
            {
                return;
            }

            using (kaze)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => kaze.Diffusivity = (KazeDiffusivityType)99);
            }
        }

        [Fact]
        public void DetectComputeAndDetectAndComputeWorkWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            KAZE? kaze = TryCreateKaze();
            if (kaze == null)
            {
                return;
            }

            using (kaze)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            {
                KeyPoint[] keypoints = kaze.Detect(image);
                KeyPoint[] computed = kaze.Compute(image, keypoints, descriptors);
                KeyPoint[] detected = kaze.DetectAndCompute(image, null, keypoints, descriptors);
                kaze.DetectAndCompute(image, null, out KeyPoint[] detectedOut, descriptors);

                Assert.NotNull(keypoints);
                Assert.NotNull(computed);
                Assert.NotNull(detected);
                Assert.NotNull(detectedOut);
                Assert.True(descriptors.Rows >= 0);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        [Fact]
        public void SpanOverloadsWorkWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            KAZE? kaze = TryCreateKaze();
            if (kaze == null)
            {
                return;
            }

            using (kaze)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            {
                KeyPoint[] keypoints = kaze.Detect(image);
                KeyPoint[] spanComputed = kaze.Compute(image, keypoints.AsSpan(), descriptors);
                KeyPoint[] spanDetected = kaze.DetectAndCompute(image, null, keypoints.AsSpan(), descriptors);

                Assert.NotNull(spanComputed);
                Assert.NotNull(spanDetected);
            }
        }
#endif

        [Fact]
        public void ValidatesArgumentsWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            KAZE? kaze = TryCreateKaze();
            if (kaze == null)
            {
                return;
            }

            using (kaze)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => kaze.Detect(null!));
                Assert.Throws<ArgumentNullException>(() => kaze.Compute(null!, Array.Empty<KeyPoint>(), descriptors));
                Assert.Throws<ArgumentNullException>(() => kaze.Compute(image, (KeyPoint[])null!, descriptors));
                Assert.Throws<ArgumentNullException>(() => kaze.Compute(image, Array.Empty<KeyPoint>(), null!));
                Assert.Throws<ArgumentNullException>(() => kaze.DetectAndCompute(image, null, (KeyPoint[])null!, descriptors));
                Assert.Throws<ArgumentNullException>(() => kaze.DetectAndCompute(image, null, Array.Empty<KeyPoint>(), null!));
            }
        }

        [Fact]
        public void DisposedKazeRejectsUseWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            KAZE? kaze = TryCreateKaze();
            if (kaze == null)
            {
                return;
            }

            using Mat image = Feature2DTestData.CreateFeatureImage();
            using Mat descriptors = new Mat();
            kaze.Dispose();

            Assert.True(kaze.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => kaze.Clear());
            Assert.Throws<ObjectDisposedException>(() => kaze.Empty);
            Assert.Throws<ObjectDisposedException>(() => kaze.DescriptorSize);
            Assert.Throws<ObjectDisposedException>(() => kaze.DescriptorType);
            Assert.Throws<ObjectDisposedException>(() => kaze.DefaultNorm);
            Assert.Throws<ObjectDisposedException>(() => kaze.DefaultName);
            Assert.Throws<ObjectDisposedException>(() => kaze.Extended);
            Assert.Throws<ObjectDisposedException>(() => kaze.Extended = true);
            Assert.Throws<ObjectDisposedException>(() => kaze.Upright);
            Assert.Throws<ObjectDisposedException>(() => kaze.Upright = true);
            Assert.Throws<ObjectDisposedException>(() => kaze.Threshold);
            Assert.Throws<ObjectDisposedException>(() => kaze.Threshold = 0.002);
            Assert.Throws<ObjectDisposedException>(() => kaze.NOctaves);
            Assert.Throws<ObjectDisposedException>(() => kaze.NOctaves = 3);
            Assert.Throws<ObjectDisposedException>(() => kaze.NOctaveLayers);
            Assert.Throws<ObjectDisposedException>(() => kaze.NOctaveLayers = 3);
            Assert.Throws<ObjectDisposedException>(() => kaze.Diffusivity);
            Assert.Throws<ObjectDisposedException>(() => kaze.Diffusivity = KazeDiffusivityType.DiffCharbonnier);
            Assert.Throws<ObjectDisposedException>(() => kaze.Detect(image));
            Assert.Throws<ObjectDisposedException>(() => kaze.Compute(image, Array.Empty<KeyPoint>(), descriptors));
            Assert.Throws<ObjectDisposedException>(() => kaze.DetectAndCompute(image, null, Array.Empty<KeyPoint>(), descriptors));
            Assert.Throws<ObjectDisposedException>(() => kaze.DetectAndCompute(image, null, out _, descriptors));
            Assert.Equal("{Disposed=True}", kaze.ToString());
        }

        private static KAZE? TryCreateKaze()
        {
            try
            {
                return KAZE.Create(threshold: 0.001F, nOctaves: 3, nOctaveLayers: 3);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }
    }
}
