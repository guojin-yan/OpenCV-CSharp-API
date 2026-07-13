using System;
using System.Globalization;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class AkazeTests
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
                using (AKAZE akaze = AKAZE.Create())
                {
                    Assert.False(akaze.IsDisposed);
                }
            }) as OpenCvException;

            if (exception != null)
            {
                Assert.Contains("features2d_akaze_create", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("OpenCV", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void DescriptorEnumValuesMatchOpenCvConstants()
        {
            Assert.Equal(2, (int)AkazeDescriptorType.DescriptorKazeUpright);
            Assert.Equal(3, (int)AkazeDescriptorType.DescriptorKaze);
            Assert.Equal(4, (int)AkazeDescriptorType.DescriptorMldbUpright);
            Assert.Equal(5, (int)AkazeDescriptorType.DescriptorMldb);
        }

        [Fact]
        public void CreateRejectsInvalidEnumValuesBeforeNativeCall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => AKAZE.Create(descriptorType: (AkazeDescriptorType)99));
            Assert.Throws<ArgumentOutOfRangeException>(() => AKAZE.Create(diffusivity: (KazeDiffusivityType)99));
        }

        [Fact]
        public void PropertiesRoundTripWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            AKAZE? akaze = TryCreateAkaze();
            if (akaze == null)
            {
                return;
            }

            using (akaze)
            {
                Assert.NotNull(akaze.Empty.ToString());
                Assert.True(akaze.DescriptorSize >= 0);
                Assert.True(akaze.DescriptorType >= 0);
                Assert.True((int)akaze.DefaultNorm >= 0);
                Assert.Contains("AKAZE", akaze.DefaultName, StringComparison.OrdinalIgnoreCase);

                akaze.AkazeDescriptorType = AkazeDescriptorType.DescriptorMldbUpright;
                akaze.AkazeDescriptorSize = 64;
                akaze.DescriptorChannels = 2;
                akaze.Threshold = 0.002;
                akaze.NOctaves = 3;
                akaze.NOctaveLayers = 3;
                akaze.Diffusivity = KazeDiffusivityType.DiffWeickert;
                akaze.MaxPoints = 128;

                Assert.Equal(AkazeDescriptorType.DescriptorMldbUpright, akaze.AkazeDescriptorType);
                Assert.Equal(64, akaze.AkazeDescriptorSize);
                Assert.Equal(2, akaze.DescriptorChannels);
                Assert.Equal(0.002, akaze.Threshold, 6);
                Assert.Equal(3, akaze.NOctaves);
                Assert.Equal(3, akaze.NOctaveLayers);
                Assert.Equal(KazeDiffusivityType.DiffWeickert, akaze.Diffusivity);
                Assert.Equal(128, akaze.MaxPoints);
                Assert.Contains("Threshold=0.002", akaze.ToString(), StringComparison.Ordinal);
                Assert.Contains("MaxPoints=128", akaze.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void EnumSettersRejectInvalidValuesWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            AKAZE? akaze = TryCreateAkaze();
            if (akaze == null)
            {
                return;
            }

            using (akaze)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => akaze.AkazeDescriptorType = (AkazeDescriptorType)99);
                Assert.Throws<ArgumentOutOfRangeException>(() => akaze.Diffusivity = (KazeDiffusivityType)99);
            }
        }

        [Fact]
        public void ToStringFormatsThresholdInvariantlyWhenXFeatures2DModuleIsLinked()
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

                AKAZE? akaze = TryCreateAkaze();
                if (akaze == null)
                {
                    return;
                }

                using (akaze)
                {
                    akaze.Threshold = 0.002;
                    string formatted = akaze.ToString();

                    Assert.Contains("Threshold=0.002", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("Threshold=0,002", formatted, StringComparison.Ordinal);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void DetectComputeAndDetectAndComputeWorkWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            AKAZE? akaze = TryCreateAkaze();
            if (akaze == null)
            {
                return;
            }

            using (akaze)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            {
                KeyPoint[] keypoints = akaze.Detect(image);
                KeyPoint[] computed = akaze.Compute(image, keypoints, descriptors);
                KeyPoint[] detected = akaze.DetectAndCompute(image, null, keypoints, descriptors);
                akaze.DetectAndCompute(image, null, out KeyPoint[] detectedOut, descriptors);

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

            AKAZE? akaze = TryCreateAkaze();
            if (akaze == null)
            {
                return;
            }

            using (akaze)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            {
                KeyPoint[] keypoints = akaze.Detect(image);
                KeyPoint[] spanComputed = akaze.Compute(image, keypoints.AsSpan(), descriptors);
                KeyPoint[] spanDetected = akaze.DetectAndCompute(image, null, keypoints.AsSpan(), descriptors);

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

            AKAZE? akaze = TryCreateAkaze();
            if (akaze == null)
            {
                return;
            }

            using (akaze)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            using (Mat descriptors = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => akaze.Detect(null!));
                Assert.Throws<ArgumentNullException>(() => akaze.Compute(null!, Array.Empty<KeyPoint>(), descriptors));
                Assert.Throws<ArgumentNullException>(() => akaze.Compute(image, (KeyPoint[])null!, descriptors));
                Assert.Throws<ArgumentNullException>(() => akaze.Compute(image, Array.Empty<KeyPoint>(), null!));
                Assert.Throws<ArgumentNullException>(() => akaze.DetectAndCompute(image, null, (KeyPoint[])null!, descriptors));
                Assert.Throws<ArgumentNullException>(() => akaze.DetectAndCompute(image, null, Array.Empty<KeyPoint>(), null!));
            }
        }

        [Fact]
        public void DisposedAkazeRejectsUseWhenXFeatures2DModuleIsLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            AKAZE? akaze = TryCreateAkaze();
            if (akaze == null)
            {
                return;
            }

            using Mat image = Feature2DTestData.CreateFeatureImage();
            using Mat descriptors = new Mat();
            akaze.Dispose();

            Assert.True(akaze.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => akaze.Clear());
            Assert.Throws<ObjectDisposedException>(() => akaze.Empty);
            Assert.Throws<ObjectDisposedException>(() => akaze.DescriptorSize);
            Assert.Throws<ObjectDisposedException>(() => akaze.DescriptorType);
            Assert.Throws<ObjectDisposedException>(() => akaze.DefaultNorm);
            Assert.Throws<ObjectDisposedException>(() => akaze.DefaultName);
            Assert.Throws<ObjectDisposedException>(() => akaze.AkazeDescriptorType);
            Assert.Throws<ObjectDisposedException>(() => akaze.AkazeDescriptorType = AkazeDescriptorType.DescriptorMldbUpright);
            Assert.Throws<ObjectDisposedException>(() => akaze.AkazeDescriptorSize);
            Assert.Throws<ObjectDisposedException>(() => akaze.AkazeDescriptorSize = 64);
            Assert.Throws<ObjectDisposedException>(() => akaze.DescriptorChannels);
            Assert.Throws<ObjectDisposedException>(() => akaze.DescriptorChannels = 2);
            Assert.Throws<ObjectDisposedException>(() => akaze.Threshold);
            Assert.Throws<ObjectDisposedException>(() => akaze.Threshold = 0.002);
            Assert.Throws<ObjectDisposedException>(() => akaze.NOctaves);
            Assert.Throws<ObjectDisposedException>(() => akaze.NOctaves = 3);
            Assert.Throws<ObjectDisposedException>(() => akaze.NOctaveLayers);
            Assert.Throws<ObjectDisposedException>(() => akaze.NOctaveLayers = 3);
            Assert.Throws<ObjectDisposedException>(() => akaze.Diffusivity);
            Assert.Throws<ObjectDisposedException>(() => akaze.Diffusivity = KazeDiffusivityType.DiffWeickert);
            Assert.Throws<ObjectDisposedException>(() => akaze.MaxPoints);
            Assert.Throws<ObjectDisposedException>(() => akaze.MaxPoints = 128);
            Assert.Throws<ObjectDisposedException>(() => akaze.Detect(image));
            Assert.Throws<ObjectDisposedException>(() => akaze.Compute(image, Array.Empty<KeyPoint>(), descriptors));
            Assert.Throws<ObjectDisposedException>(() => akaze.DetectAndCompute(image, null, Array.Empty<KeyPoint>(), descriptors));
            Assert.Throws<ObjectDisposedException>(() => akaze.DetectAndCompute(image, null, out _, descriptors));
            Assert.Equal("{Disposed=True}", akaze.ToString());
        }

        private static AKAZE? TryCreateAkaze()
        {
            try
            {
                return AKAZE.Create(
                    descriptorType: AkazeDescriptorType.DescriptorMldb,
                    descriptorSize: 0,
                    descriptorChannels: 3,
                    threshold: 0.001F,
                    nOctaves: 3,
                    nOctaveLayers: 3,
                    maxPoints: 256);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }
    }
}
