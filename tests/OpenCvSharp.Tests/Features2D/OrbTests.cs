using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Features2D
{
    public class OrbTests
    {
        [Fact]
        public void CreateReportsDefinedBoundaryWhenFeaturesModuleIsNotLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            OpenCvException? exception = Record.Exception(() =>
            {
                using (ORB orb = ORB.Create())
                {
                    Assert.False(orb.IsDisposed);
                }
            }) as OpenCvException;

            if (exception != null)
            {
                Assert.Contains("features2d_orb_create", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("OpenCV", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void OrbRejectsInvalidScoreTypeBeforeNativeCall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ORB.Create(scoreType: (OrbScoreType)99));
        }

        [Fact]
        public void OrbPropertiesRoundTripWhenFeaturesModuleIsLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
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
                Assert.NotNull(orb.Empty.ToString());
                Assert.True(orb.DescriptorSize > 0);
                Assert.True(orb.DescriptorType >= 0);
                Assert.True(orb.DefaultNorm == NormTypes.Hamming || orb.DefaultNorm == NormTypes.Hamming2);
                Assert.Contains("ORB", orb.DefaultName, StringComparison.OrdinalIgnoreCase);

                orb.MaxFeatures = 128;
                orb.ScaleFactor = 1.1;
                orb.NLevels = 4;
                orb.EdgeThreshold = 15;
                orb.FirstLevel = 0;
                orb.WtaK = 2;
                orb.ScoreType = OrbScoreType.FastScore;
                orb.PatchSize = 21;
                orb.FastThreshold = 12;

                Assert.Equal(128, orb.MaxFeatures);
                Assert.Equal(1.1, orb.ScaleFactor, 6);
                Assert.Equal(4, orb.NLevels);
                Assert.Equal(15, orb.EdgeThreshold);
                Assert.Equal(0, orb.FirstLevel);
                Assert.Equal(2, orb.WtaK);
                Assert.Equal(OrbScoreType.FastScore, orb.ScoreType);
                Assert.Equal(21, orb.PatchSize);
                Assert.Equal(12, orb.FastThreshold);
                Assert.Contains("MaxFeatures=128", orb.ToString(), StringComparison.Ordinal);
                Assert.Contains("ScaleFactor=1.1", orb.ToString(), StringComparison.Ordinal);
            }
        }

        [Fact]
        public void OrbScoreTypeSetterRejectsInvalidValueWhenFeaturesModuleIsLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
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
                Assert.Throws<ArgumentOutOfRangeException>(() => orb.ScoreType = (OrbScoreType)99);
            }
        }

        [Fact]
        public void ToStringFormatsScaleFactorInvariantlyWhenFeaturesModuleIsLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                ORB? orb = TryCreateOrb();
                if (orb == null)
                {
                    return;
                }

                using (orb)
                {
                    orb.ScaleFactor = 1.1;
                    string formatted = orb.ToString();

                    Assert.Contains("ScaleFactor=1.1", formatted, StringComparison.Ordinal);
                    Assert.DoesNotContain("ScaleFactor=1,1", formatted, StringComparison.Ordinal);
                }
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void DetectComputeAndDetectAndComputeReturnManagedKeypointsWhenFeaturesModuleIsLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            ORB? orb = TryCreateOrb();
            if (orb == null)
            {
                return;
            }

            using (orb)
            using (Mat image = CreateFeatureImage())
            using (Mat descriptorsFromCompute = new Mat())
            using (Mat descriptorsFromDetectAndCompute = new Mat())
            {
                KeyPoint[] keypoints = orb.Detect(image);
                Assert.NotNull(keypoints);

                KeyPoint[] computedKeypoints = orb.Compute(image, keypoints, descriptorsFromCompute);
                Assert.NotNull(computedKeypoints);
                Assert.True(descriptorsFromCompute.Rows >= 0);

                KeyPoint[] mutableKeypoints = keypoints;
                orb.Compute(image, ref mutableKeypoints, descriptorsFromCompute);
                Assert.NotNull(mutableKeypoints);

                orb.DetectAndCompute(image, null, out KeyPoint[] detectedAndComputed, descriptorsFromDetectAndCompute);
                Assert.NotNull(detectedAndComputed);
                Assert.True(descriptorsFromDetectAndCompute.Rows >= 0);

                KeyPoint[] provided = detectedAndComputed;
                KeyPoint[] providedResult = orb.DetectAndCompute(image, null, provided, descriptorsFromDetectAndCompute);
                Assert.NotNull(providedResult);
                orb.DetectAndComputeInPlace(image, null, ref provided, descriptorsFromDetectAndCompute, useProvidedKeypoints: true);
                Assert.NotNull(provided);

#if NETCOREAPP3_1_OR_GREATER
                KeyPoint[] spanComputed = orb.Compute(image, keypoints.AsSpan(), descriptorsFromCompute);
                KeyPoint[] spanDetectedAndComputed = orb.DetectAndCompute(image, null, keypoints.AsSpan(), descriptorsFromDetectAndCompute);
                Assert.NotNull(spanComputed);
                Assert.NotNull(spanDetectedAndComputed);
#endif
            }
        }

        [Fact]
        public void DisposedOrbRejectsUseWhenFeaturesModuleIsLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            ORB? orb = TryCreateOrb();
            if (orb == null)
            {
                return;
            }

            using Mat image = CreateFeatureImage();
            using Mat descriptors = new Mat();
            orb.Dispose();

            Assert.True(orb.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => orb.Clear());
            Assert.Throws<ObjectDisposedException>(() => orb.Empty);
            Assert.Throws<ObjectDisposedException>(() => orb.DescriptorSize);
            Assert.Throws<ObjectDisposedException>(() => orb.DescriptorType);
            Assert.Throws<ObjectDisposedException>(() => orb.DefaultNorm);
            Assert.Throws<ObjectDisposedException>(() => orb.DefaultName);
            Assert.Throws<ObjectDisposedException>(() => orb.MaxFeatures);
            Assert.Throws<ObjectDisposedException>(() => orb.MaxFeatures = 64);
            Assert.Throws<ObjectDisposedException>(() => orb.ScaleFactor);
            Assert.Throws<ObjectDisposedException>(() => orb.ScaleFactor = 1.1);
            Assert.Throws<ObjectDisposedException>(() => orb.NLevels);
            Assert.Throws<ObjectDisposedException>(() => orb.NLevels = 4);
            Assert.Throws<ObjectDisposedException>(() => orb.EdgeThreshold);
            Assert.Throws<ObjectDisposedException>(() => orb.EdgeThreshold = 15);
            Assert.Throws<ObjectDisposedException>(() => orb.FirstLevel);
            Assert.Throws<ObjectDisposedException>(() => orb.FirstLevel = 0);
            Assert.Throws<ObjectDisposedException>(() => orb.WtaK);
            Assert.Throws<ObjectDisposedException>(() => orb.WtaK = 2);
            Assert.Throws<ObjectDisposedException>(() => orb.ScoreType);
            Assert.Throws<ObjectDisposedException>(() => orb.ScoreType = OrbScoreType.FastScore);
            Assert.Throws<ObjectDisposedException>(() => orb.PatchSize);
            Assert.Throws<ObjectDisposedException>(() => orb.PatchSize = 21);
            Assert.Throws<ObjectDisposedException>(() => orb.FastThreshold);
            Assert.Throws<ObjectDisposedException>(() => orb.FastThreshold = 12);
            Assert.Throws<ObjectDisposedException>(() => orb.Detect(image));
            Assert.Throws<ObjectDisposedException>(() => orb.Compute(image, Array.Empty<KeyPoint>(), descriptors));
            Assert.Throws<ObjectDisposedException>(() => orb.DetectAndCompute(image, null, Array.Empty<KeyPoint>(), descriptors));
            Assert.Throws<ObjectDisposedException>(() => orb.DetectAndCompute(image, null, out _, descriptors));
            Assert.Equal("{Disposed=True}", orb.ToString());
        }

        private static ORB? TryCreateOrb()
        {
            try
            {
                return ORB.Create(maxFeatures: 128, fastThreshold: 8);
            }
            catch (OpenCvException ex) when (IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static Mat CreateFeatureImage()
        {
            return Feature2DTestData.CreateFeatureImage();
        }

        private static bool IsFeaturesModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("features2d", StringComparison.OrdinalIgnoreCase) >= 0
                && exception.Message.IndexOf("OpenCV", StringComparison.OrdinalIgnoreCase) >= 0;
        }

    }
}
