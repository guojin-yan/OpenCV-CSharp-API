using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class FastFeatureDetectorTests
    {
        [Fact]
        public void PropertiesRoundTripWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            FastFeatureDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            {
                Assert.True(detector.DescriptorSize >= 0);
                Assert.True(detector.DescriptorType >= 0);
                Assert.True((int)detector.DefaultNorm >= 0);
                Assert.Contains("Fast", detector.DefaultName, StringComparison.OrdinalIgnoreCase);

                detector.Threshold = 16;
                detector.NonmaxSuppression = false;
                detector.Type = FastFeatureDetectorType.Type7_12;

                Assert.Equal(16, detector.Threshold);
                Assert.False(detector.NonmaxSuppression);
                Assert.Equal(FastFeatureDetectorType.Type7_12, detector.Type);
                Assert.Equal("{Threshold=16,NonmaxSuppression=False,Type=Type7_12}", detector.ToString());
            }
        }

        [Fact]
        public void DetectReturnsManagedKeypointsWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            FastFeatureDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            using (Mat image = Feature2DTestData.CreateFeatureImage())
            {
                KeyPoint[] keypoints = detector.Detect(image);
                Assert.NotNull(keypoints);
            }
        }

        [Fact]
        public void FastFeatureDetectorRejectsInvalidTypeBeforeNativeCall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FastFeatureDetector.Create(type: (FastFeatureDetectorType)99));
        }

        [Fact]
        public void FastFeatureDetectorTypeSetterRejectsInvalidValueWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            FastFeatureDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => detector.Type = (FastFeatureDetectorType)99);
            }
        }

        [Fact]
        public void DisposedFastFeatureDetectorRejectsUseWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            FastFeatureDetector? detector = TryCreateDetector();
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
                Assert.Throws<ObjectDisposedException>(() => detector.Threshold);
                Assert.Throws<ObjectDisposedException>(() => detector.Threshold = 16);
                Assert.Throws<ObjectDisposedException>(() => detector.NonmaxSuppression);
                Assert.Throws<ObjectDisposedException>(() => detector.NonmaxSuppression = false);
                Assert.Throws<ObjectDisposedException>(() => detector.Type);
                Assert.Throws<ObjectDisposedException>(() => detector.Type = FastFeatureDetectorType.Type7_12);
                Assert.Throws<ObjectDisposedException>(() => detector.Detect(image));
                Assert.Equal("{Disposed=True}", detector.ToString());
            }
        }

        private static FastFeatureDetector? TryCreateDetector()
        {
            try
            {
                return FastFeatureDetector.Create(threshold: 12, nonmaxSuppression: true, type: FastFeatureDetectorType.Type9_16);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }
    }
}
