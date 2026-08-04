using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace JYPPX.OpenCvSharp.Tests.Features2D
{
    public class GfttDetectorTests
    {
        [Fact]
        public void ToStringFormatsFloatingValuesInvariantlyWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            GFTTDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                using (detector)
                {
                    detector.QualityLevel = 0.02;
                    detector.MinDistance = 3.5;

                    string formatted = detector.ToString();
                    Assert.Contains("QualityLevel=0.02", formatted, StringComparison.Ordinal);
                    Assert.Contains("MinDistance=3.5", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("QualityLevel=0,02", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("MinDistance=3,5", formatted, StringComparison.Ordinal);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void PropertiesRoundTripWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            GFTTDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            {
                Assert.True(detector.DescriptorSize >= 0);
                Assert.True(detector.DescriptorType >= 0);
                Assert.True((int)detector.DefaultNorm >= 0);
                Assert.Contains("GFTT", detector.DefaultName, StringComparison.OrdinalIgnoreCase);

                detector.MaxFeatures = 24;
                detector.QualityLevel = 0.02;
                detector.MinDistance = 3.0;
                detector.BlockSize = 5;
                detector.GradientSize = 3;
                detector.HarrisDetector = true;
                detector.K = 0.05;

                Assert.Equal(24, detector.MaxFeatures);
                Assert.Equal(0.02, detector.QualityLevel, 6);
                Assert.Equal(3.0, detector.MinDistance, 6);
                Assert.Equal(5, detector.BlockSize);
                Assert.Equal(3, detector.GradientSize);
                Assert.True(detector.HarrisDetector);
                Assert.Equal(0.05, detector.K, 6);
                Assert.Contains("MaxFeatures=24", detector.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void DetectReturnsManagedKeypointsWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            GFTTDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            {
                KeyPoint[] keypoints = detector.Detect(image);
                Assert.NotNull(keypoints);
                Assert.True(keypoints.Length <= detector.MaxFeatures);
            }
        }

        [Fact]
        public void DisposedGfttDetectorRejectsUseWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            GFTTDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (Mat image = Feature2DTestData.CreateFeatureImage())
            {
                detector.Dispose();

                Assert.True(detector.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => detector.Clear());
                Assert.Throws<ObjectDisposedException>(() => detector.Empty);
                Assert.Throws<ObjectDisposedException>(() => detector.DescriptorSize);
                Assert.Throws<ObjectDisposedException>(() => detector.DescriptorType);
                Assert.Throws<ObjectDisposedException>(() => detector.DefaultNorm);
                Assert.Throws<ObjectDisposedException>(() => detector.DefaultName);
                Assert.Throws<ObjectDisposedException>(() => detector.MaxFeatures);
                Assert.Throws<ObjectDisposedException>(() => detector.MaxFeatures = 24);
                Assert.Throws<ObjectDisposedException>(() => detector.QualityLevel);
                Assert.Throws<ObjectDisposedException>(() => detector.QualityLevel = 0.02);
                Assert.Throws<ObjectDisposedException>(() => detector.MinDistance);
                Assert.Throws<ObjectDisposedException>(() => detector.MinDistance = 3.0);
                Assert.Throws<ObjectDisposedException>(() => detector.BlockSize);
                Assert.Throws<ObjectDisposedException>(() => detector.BlockSize = 5);
                Assert.Throws<ObjectDisposedException>(() => detector.GradientSize);
                Assert.Throws<ObjectDisposedException>(() => detector.GradientSize = 3);
                Assert.Throws<ObjectDisposedException>(() => detector.HarrisDetector);
                Assert.Throws<ObjectDisposedException>(() => detector.HarrisDetector = true);
                Assert.Throws<ObjectDisposedException>(() => detector.K);
                Assert.Throws<ObjectDisposedException>(() => detector.K = 0.05);
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(image));
                Assert.Equal("{Disposed=True}", detector.ToString());
            }
        }

        private static GFTTDetector? TryCreateDetector()
        {
            try
            {
                return GFTTDetector.Create(maxCorners: 32, qualityLevel: 0.01, minDistance: 2.0, blockSize: 3, gradientSize: 3);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }
    }
}
