using System;
using System.Globalization;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class BriskTests
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
                using (BRISK brisk = BRISK.Create())
                {
                    Assert.False(brisk.IsDisposed);
                }
            }) as OpenCvException;

            if (exception != null)
            {
                Assert.Contains("features2d_brisk_create", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("OpenCV", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void CreatePatternValidatesNullArraysBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => BRISK.Create((float[])null!, new[] { 8 }));
            Assert.Throws<ArgumentNullException>(() => BRISK.Create(new[] { 2.0F }, (int[])null!));
            Assert.Throws<ArgumentNullException>(() => BRISK.Create(20, 2, (float[])null!, new[] { 8 }));
            Assert.Throws<ArgumentNullException>(() => BRISK.Create(20, 2, new[] { 2.0F }, (int[])null!));
        }

        [Fact]
        public void PropertiesRoundTripWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            BRISK? brisk = TryCreateBrisk();
            if (brisk == null)
            {
                return;
            }

            using (brisk)
            {
                Assert.NotNull(brisk.Empty.ToString());
                Assert.True(brisk.DescriptorSize >= 0);
                Assert.True(brisk.DescriptorType >= 0);
                Assert.True((int)brisk.DefaultNorm >= 0);
                Assert.Contains("BRISK", brisk.DefaultName, StringComparison.OrdinalIgnoreCase);

                brisk.Threshold = 18;
                brisk.Octaves = 2;
                brisk.PatternScale = 1.25F;

                Assert.Equal(18, brisk.Threshold);
                Assert.Equal(2, brisk.Octaves);
                Assert.Equal(1.25F, brisk.PatternScale, 5);
                Assert.Contains("Threshold=18", brisk.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void ToStringFormatsFloatingValuesInvariantlyWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            BRISK? brisk = TryCreateBrisk();
            if (brisk == null)
            {
                return;
            }

            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                using (brisk)
                {
                    brisk.PatternScale = 1.25F;

                    string text = brisk.ToString();

                    Assert.Contains("PatternScale=1.25", text, StringComparison.Ordinal);
                    Assert.DoesNotContain("PatternScale=1,25", text, StringComparison.Ordinal);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void CustomPatternCreateWorksWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            BRISK? brisk = TryCreateCustomPatternBrisk();
            if (brisk == null)
            {
                return;
            }

            using (brisk)
            {
                Assert.False(brisk.IsDisposed);
                Assert.True(brisk.DescriptorSize >= 0);
                Assert.Contains("BRISK", brisk.DefaultName, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void DetectComputeAndDetectAndComputeWorkWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            BRISK? brisk = TryCreateBrisk();
            if (brisk == null)
            {
                return;
            }

            using (brisk)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptorsFromCompute = new Mat())
            using (Mat descriptorsFromDetectAndCompute = new Mat())
            {
                KeyPoint[] keypoints = brisk.Detect(image);
                Assert.NotNull(keypoints);

                KeyPoint[] computed = brisk.Compute(image, keypoints, descriptorsFromCompute);
                Assert.NotNull(computed);
                Assert.True(descriptorsFromCompute.Rows >= 0);

                KeyPoint[] mutable = keypoints;
                brisk.Compute(image, ref mutable, descriptorsFromCompute);
                Assert.NotNull(mutable);

                brisk.DetectAndCompute(image, null, out KeyPoint[] detectedOut, descriptorsFromDetectAndCompute);
                Assert.NotNull(detectedOut);
                Assert.True(descriptorsFromDetectAndCompute.Rows >= 0);

                KeyPoint[] provided = detectedOut;
                KeyPoint[] detected = brisk.DetectAndCompute(image, null, provided, descriptorsFromDetectAndCompute);
                Assert.NotNull(detected);
                brisk.DetectAndComputeInPlace(image, null, ref provided, descriptorsFromDetectAndCompute, useProvidedKeypoints: true);
                Assert.NotNull(provided);
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

            BRISK? brisk = TryCreateBrisk();
            if (brisk == null)
            {
                return;
            }

            using (brisk)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            {
                KeyPoint[] keypoints = brisk.Detect(image);
                KeyPoint[] spanComputed = brisk.Compute(image, keypoints.AsSpan(), descriptors);
                KeyPoint[] spanDetected = brisk.DetectAndCompute(image, null, keypoints.AsSpan(), descriptors);

                Assert.NotNull(spanComputed);
                Assert.NotNull(spanDetected);

                ReadOnlySpan<float> radii = stackalloc float[] { 2.0F, 4.0F, 8.0F };
                ReadOnlySpan<int> counts = stackalloc int[] { 8, 12, 16 };
                using (BRISK custom = BRISK.Create(20, 2, radii, counts))
                {
                    Assert.False(custom.IsDisposed);
                }
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

            BRISK? brisk = TryCreateBrisk();
            if (brisk == null)
            {
                return;
            }

            using (brisk)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => brisk.Detect(null!));
                Assert.Throws<ArgumentNullException>(() => brisk.Compute(null!, Array.Empty<KeyPoint>(), descriptors));
                Assert.Throws<ArgumentNullException>(() => brisk.Compute(image, (KeyPoint[])null!, descriptors));
                Assert.Throws<ArgumentNullException>(() => brisk.Compute(image, Array.Empty<KeyPoint>(), null!));
                Assert.Throws<ArgumentNullException>(() => brisk.DetectAndCompute(image, null, (KeyPoint[])null!, descriptors));
                Assert.Throws<ArgumentNullException>(() => brisk.DetectAndCompute(image, null, Array.Empty<KeyPoint>(), null!));
            }
        }

        [Fact]
        public void DisposedBriskRejectsUseWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            BRISK? brisk = TryCreateBrisk();
            if (brisk == null)
            {
                return;
            }

            using Mat image = Feature2DTestData.CreateFeatureImage();
            using Mat descriptors = new Mat();
            brisk.Dispose();

            Assert.True(brisk.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => brisk.Clear());
            Assert.Throws<ObjectDisposedException>(() => brisk.Empty);
            Assert.Throws<ObjectDisposedException>(() => brisk.DescriptorSize);
            Assert.Throws<ObjectDisposedException>(() => brisk.DescriptorType);
            Assert.Throws<ObjectDisposedException>(() => brisk.DefaultNorm);
            Assert.Throws<ObjectDisposedException>(() => brisk.DefaultName);
            Assert.Throws<ObjectDisposedException>(() => brisk.Threshold);
            Assert.Throws<ObjectDisposedException>(() => brisk.Threshold = 18);
            Assert.Throws<ObjectDisposedException>(() => brisk.Octaves);
            Assert.Throws<ObjectDisposedException>(() => brisk.Octaves = 2);
            Assert.Throws<ObjectDisposedException>(() => brisk.PatternScale);
            Assert.Throws<ObjectDisposedException>(() => brisk.PatternScale = 1.25F);
            Assert.Throws<ObjectDisposedException>(() => brisk.Detect(image));
            Assert.Throws<ObjectDisposedException>(() => brisk.Compute(image, Array.Empty<KeyPoint>(), descriptors));
            Assert.Throws<ObjectDisposedException>(() => brisk.DetectAndCompute(image, null, Array.Empty<KeyPoint>(), descriptors));
            Assert.Throws<ObjectDisposedException>(() => brisk.DetectAndCompute(image, null, out _, descriptors));
            Assert.Equal("{Disposed=True}", brisk.ToString());
        }

        private static BRISK? TryCreateBrisk()
        {
            try
            {
                return BRISK.Create(threshold: 24, octaves: 2, patternScale: 1.0F);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static BRISK? TryCreateCustomPatternBrisk()
        {
            try
            {
                return BRISK.Create(
                    threshold: 20,
                    octaves: 2,
                    radiusList: new[] { 2.0F, 4.0F, 8.0F },
                    numberList: new[] { 8, 12, 16 });
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }
    }
}
