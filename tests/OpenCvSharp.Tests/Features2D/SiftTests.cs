using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace JYPPX.OpenCvSharp.Tests.Features2D
{
    public class SiftTests
    {
        [Fact]
        public void CreateExposesSiftSettingsAndBoundaryWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SIFT? sift = TryCreateSift();
            if (sift == null)
            {
                return;
            }

            using (sift)
            {
                Assert.True(sift.DescriptorSize > 0);
                Assert.True(sift.DescriptorType == MatType.CV_32F || sift.DescriptorType == MatType.CV_8U);
                Assert.True(sift.DefaultNorm == NormTypes.L2 || sift.DefaultNorm == NormTypes.L2Sqr);
                Assert.Contains("SIFT", sift.DefaultName, StringComparison.OrdinalIgnoreCase);

                sift.NFeatures = 256;
                sift.NOctaveLayers = 4;
                sift.ContrastThreshold = 0.03;
                sift.EdgeThreshold = 12.0;
                sift.Sigma = 1.4;

                Assert.Equal(256, sift.NFeatures);
                Assert.Equal(4, sift.NOctaveLayers);
                Assert.Equal(0.03, sift.ContrastThreshold, 6);
                Assert.Equal(12.0, sift.EdgeThreshold, 6);
                Assert.Equal(1.4, sift.Sigma, 6);
                Assert.Contains("NFeatures=256", sift.ToString(), StringComparison.Ordinal);
                Assert.Contains("ContrastThreshold=0.03", sift.ToString(), StringComparison.Ordinal);
                Assert.Contains("EdgeThreshold=12", sift.ToString(), StringComparison.Ordinal);
                Assert.Contains("Sigma=1.4", sift.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ToStringFormatsFloatingPointSettingsInvariantlyWhenNativeRuntimeIsAvailable()
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

                SIFT? sift = TryCreateSift();
                if (sift == null)
                {
                    return;
                }

                using (sift)
                {
                    sift.ContrastThreshold = 0.03;
                    sift.EdgeThreshold = 12.5;
                    sift.Sigma = 1.4;
                    string formatted = sift.ToString();

                    Assert.Contains("ContrastThreshold=0.03", formatted, StringComparison.Ordinal);
                    Assert.Contains("EdgeThreshold=12.5", formatted, StringComparison.Ordinal);
                    Assert.Contains("Sigma=1.4", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("ContrastThreshold=0,03", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("EdgeThreshold=12,5", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("Sigma=1,4", formatted, StringComparison.Ordinal);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void DetectComputeAndDetectAndComputeWorkWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SIFT? sift = TryCreateSift();
            if (sift == null)
            {
                return;
            }

            using (sift)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            {
                KeyPoint[] keypoints = sift.Detect(image);
                KeyPoint[] computed = sift.Compute(image, keypoints, descriptors);
                KeyPoint[] detected = sift.DetectAndCompute(image, null, keypoints, descriptors);
                sift.DetectAndCompute(image, null, out KeyPoint[] detectedOut, descriptors);

                Assert.NotNull(keypoints);
                Assert.NotNull(computed);
                Assert.NotNull(detected);
                Assert.NotNull(detectedOut);
                Assert.True(descriptors.Rows >= 0);

#if NETCOREAPP3_1_OR_GREATER
                KeyPoint[] spanComputed = sift.Compute(image, keypoints.AsSpan(), descriptors);
                KeyPoint[] spanDetected = sift.DetectAndCompute(image, null, keypoints.AsSpan(), descriptors);
                Assert.NotNull(spanComputed);
                Assert.NotNull(spanDetected);
#endif
            }
        }

        [Fact]
        public void DisposedSiftRejectsUseWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SIFT? sift = TryCreateSift();
            if (sift == null)
            {
                return;
            }

            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            {
                sift.Dispose();

                Assert.True(sift.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => sift.Clear());
                Assert.Throws<ObjectDisposedException>(() => sift.Empty);
                Assert.Throws<ObjectDisposedException>(() => sift.DescriptorSize);
                Assert.Throws<ObjectDisposedException>(() => sift.DescriptorType);
                Assert.Throws<ObjectDisposedException>(() => sift.DefaultNorm);
                Assert.Throws<ObjectDisposedException>(() => sift.DefaultName);
                Assert.Throws<ObjectDisposedException>(() => sift.NFeatures);
                Assert.Throws<ObjectDisposedException>(() => sift.NFeatures = 64);
                Assert.Throws<ObjectDisposedException>(() => sift.NOctaveLayers);
                Assert.Throws<ObjectDisposedException>(() => sift.NOctaveLayers = 3);
                Assert.Throws<ObjectDisposedException>(() => sift.ContrastThreshold);
                Assert.Throws<ObjectDisposedException>(() => sift.ContrastThreshold = 0.04);
                Assert.Throws<ObjectDisposedException>(() => sift.EdgeThreshold);
                Assert.Throws<ObjectDisposedException>(() => sift.EdgeThreshold = 10.0);
                Assert.Throws<ObjectDisposedException>(() => sift.Sigma);
                Assert.Throws<ObjectDisposedException>(() => sift.Sigma = 1.6);
                Assert.Throws<ObjectDisposedException>(() => sift.Detect(image));
                Assert.Throws<ObjectDisposedException>(() => sift.Compute(image, Array.Empty<KeyPoint>(), descriptors));
                Assert.Throws<ObjectDisposedException>(() => sift.DetectAndCompute(image, null, Array.Empty<KeyPoint>(), descriptors));
                Assert.Throws<ObjectDisposedException>(() => sift.DetectAndCompute(image, null, out _, descriptors));
                Assert.Equal("{Disposed=True}", sift.ToString());
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
    }
}
