using System;
using System.Globalization;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;

namespace OpenCvSharp.Tests.Features2D
{
    public class SimpleBlobDetectorTests
    {
        [Fact]
        public void ParamsUseOpenCvDefaults()
        {
            SimpleBlobDetectorParams parameters = new SimpleBlobDetectorParams();

            Assert.Equal(10.0F, parameters.ThresholdStep);
            Assert.Equal(50.0F, parameters.MinThreshold);
            Assert.Equal(220.0F, parameters.MaxThreshold);
            Assert.Equal(2, parameters.MinRepeatability);
            Assert.Equal(10.0F, parameters.MinDistBetweenBlobs);
            Assert.True(parameters.FilterByColor);
            Assert.Equal(0, parameters.BlobColor);
            Assert.True(parameters.FilterByArea);
            Assert.Equal(25.0F, parameters.MinArea);
            Assert.Equal(5000.0F, parameters.MaxArea);
            Assert.False(parameters.FilterByCircularity);
            Assert.Equal(0.8F, parameters.MinCircularity);
            Assert.Equal(float.MaxValue, parameters.MaxCircularity);
            Assert.True(parameters.FilterByInertia);
            Assert.Equal(0.1F, parameters.MinInertiaRatio);
            Assert.Equal(float.MaxValue, parameters.MaxInertiaRatio);
            Assert.True(parameters.FilterByConvexity);
            Assert.Equal(0.95F, parameters.MinConvexity);
            Assert.Equal(float.MaxValue, parameters.MaxConvexity);
            Assert.False(parameters.CollectContours);
            Assert.Contains("ThresholdStep=10", parameters.ToString(), StringComparison.Ordinal);
        }

        [Fact]
        public void ParamsCanBeClonedAndCopied()
        {
            SimpleBlobDetectorParams source = CreateStableBlobParams();
            SimpleBlobDetectorParams clone = source.Clone();
            SimpleBlobDetectorParams copy = new SimpleBlobDetectorParams(source);

            clone.MinArea = 99.0F;

            Assert.NotSame(source, clone);
            Assert.NotSame(source, copy);
            Assert.NotSame(copy, clone);
            Assert.Equal(20.0F, source.MinArea);
            Assert.Equal(99.0F, clone.MinArea);
            Assert.Equal(source.MaxArea, copy.MaxArea);
            Assert.Throws<ArgumentNullException>(() => new SimpleBlobDetectorParams(null!));
        }

        [Fact]
        public void ParamsCanBeConstructedWithExplicitValues()
        {
            SimpleBlobDetectorParams parameters = new SimpleBlobDetectorParams(
                1.5F,
                2.5F,
                3.5F,
                4,
                5.5F,
                false,
                255,
                false,
                6.5F,
                7.5F,
                true,
                0.25F,
                0.75F,
                false,
                0.35F,
                0.85F,
                false,
                0.45F,
                0.95F,
                true);

            Assert.Equal(1.5F, parameters.ThresholdStep);
            Assert.Equal(2.5F, parameters.MinThreshold);
            Assert.Equal(3.5F, parameters.MaxThreshold);
            Assert.Equal(4, parameters.MinRepeatability);
            Assert.Equal(5.5F, parameters.MinDistBetweenBlobs);
            Assert.False(parameters.FilterByColor);
            Assert.Equal(255, parameters.BlobColor);
            Assert.False(parameters.FilterByArea);
            Assert.Equal(6.5F, parameters.MinArea);
            Assert.Equal(7.5F, parameters.MaxArea);
            Assert.True(parameters.FilterByCircularity);
            Assert.Equal(0.25F, parameters.MinCircularity);
            Assert.Equal(0.75F, parameters.MaxCircularity);
            Assert.False(parameters.FilterByInertia);
            Assert.Equal(0.35F, parameters.MinInertiaRatio);
            Assert.Equal(0.85F, parameters.MaxInertiaRatio);
            Assert.False(parameters.FilterByConvexity);
            Assert.Equal(0.45F, parameters.MinConvexity);
            Assert.Equal(0.95F, parameters.MaxConvexity);
            Assert.True(parameters.CollectContours);
        }

        [Fact]
        public void ParamsToStringFormatsFloatingValuesInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                SimpleBlobDetectorParams parameters = new SimpleBlobDetectorParams
                {
                    ThresholdStep = 1.5F,
                    MinThreshold = 2.25F,
                    MaxThreshold = 3.5F,
                    MinArea = 4.75F,
                    MaxArea = 5.125F,
                    BlobColor = 255
                };

                string formatted = parameters.ToString();
                Assert.Equal("{ThresholdStep=1.5,MinThreshold=2.25,MaxThreshold=3.5,MinArea=4.75,MaxArea=5.125,BlobColor=255}", formatted);
                Assert.DoesNotContain("1,5", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("2,25", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("4,75", formatted, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void CreateReportsDefinedBoundaryWhenFeaturesModuleIsNotLinked()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            OpenCvException? exception = Record.Exception(() =>
            {
                using (SimpleBlobDetector detector = SimpleBlobDetector.Create())
                {
                    Assert.False(detector.IsDisposed);
                }
            }) as OpenCvException;

            if (exception != null)
            {
                Assert.Contains("simple_blob", exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("OpenCV", exception.Message, StringComparison.OrdinalIgnoreCase);
            }
        }

        [Fact]
        public void CreateRejectsNullParameters()
        {
            Assert.Throws<ArgumentNullException>(() => SimpleBlobDetector.Create(null!));
        }

        [Fact]
        public void ParametersRoundTripWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SimpleBlobDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            {
                Assert.True(detector.DescriptorSize >= 0);
                Assert.True(detector.DescriptorType >= 0);
                Assert.True((int)detector.DefaultNorm >= 0);
                Assert.Contains("SimpleBlob", detector.DefaultName, StringComparison.OrdinalIgnoreCase);

                SimpleBlobDetectorParams parameters = detector.Parameters;
                Assert.Equal(5.0F, parameters.ThresholdStep);
                Assert.Equal(0.0F, parameters.MinThreshold);
                Assert.Equal(255.0F, parameters.MaxThreshold);
                Assert.True(parameters.FilterByColor);
                Assert.Equal(0, parameters.BlobColor);

                parameters.MinArea = 30.0F;
                parameters.MaxArea = 2000.0F;
                parameters.MinRepeatability = 1;
                detector.Parameters = parameters;

                SimpleBlobDetectorParams roundTrip = detector.Parameters;
                Assert.Equal(30.0F, roundTrip.MinArea);
                Assert.Equal(2000.0F, roundTrip.MaxArea);
                Assert.Equal(1, roundTrip.MinRepeatability);
                Assert.Equal("{Parameters=" + roundTrip + "}", detector.ToString());
            }
        }

        [Fact]
        public void DetectReturnsStableBlobKeypointsWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SimpleBlobDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            using (Mat image = Feature2DTestData.CreateBlobImage())
            {
                KeyPoint[] keypoints = detector.Detect(image);
                Assert.True(keypoints.Length >= 1);
                Assert.All(keypoints, keypoint => Assert.True(keypoint.Size > 0.0F));
            }
        }

        [Fact]
        public void GetBlobContoursReturnsCollectedContoursWhenEnabledAndNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SimpleBlobDetectorParams parameters = CreateStableBlobParams();
            parameters.CollectContours = true;

            SimpleBlobDetector? detector;
            try
            {
                detector = SimpleBlobDetector.Create(parameters);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return;
            }

            using (detector)
            using (Mat image = Feature2DTestData.CreateBlobImage())
            {
                KeyPoint[] keypoints = detector.Detect(image);
                Point[][] contours = detector.GetBlobContours();

                Assert.NotEmpty(keypoints);
                Assert.NotEmpty(contours);
                Assert.All(contours, contour => Assert.NotEmpty(contour));
            }
        }

        [Fact]
        public void DetectWorksThroughFeature2DBaseWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SimpleBlobDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            using (Mat first = Feature2DTestData.CreateBlobImage())
            using (Mat second = Feature2DTestData.CreateBlobImage())
            {
                Feature2D feature = detector;
                KeyPoint[][] batch = feature.Detect(new[] { first, second });

                Assert.Equal(2, batch.Length);
                Assert.True(batch[0].Length >= 1);
                Assert.True(batch[1].Length >= 1);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        [Fact]
        public void DetectWorksThroughSpanBatchWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SimpleBlobDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            using (Mat first = Feature2DTestData.CreateBlobImage())
            using (Mat second = Feature2DTestData.CreateBlobImage())
            {
                Mat[] images = new[] { first, second };
                KeyPoint[][] batch = detector.Detect(images.AsSpan());
                Assert.Equal(2, batch.Length);
            }
        }
#endif

        [Fact]
        public void DetectValidatesArguments()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SimpleBlobDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using (detector)
            {
                Assert.Throws<ArgumentNullException>(() => detector.Detect((Mat)null!));
                Assert.Throws<ArgumentNullException>(() => detector.Parameters = null!);
            }
        }

        [Fact]
        public void DisposedDetectorRejectsUseWhenNativeRuntimeIsAvailable()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            SimpleBlobDetector? detector = TryCreateDetector();
            if (detector == null)
            {
                return;
            }

            using Mat image = Feature2DTestData.CreateBlobImage();
            SimpleBlobDetectorParams parameters = CreateStableBlobParams();
            detector.Dispose();

            Assert.True(detector.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => detector.Clear());
            Assert.Throws<ObjectDisposedException>(() => detector.Parameters);
            Assert.Throws<ObjectDisposedException>(() => detector.Parameters = parameters);
            Assert.Throws<ObjectDisposedException>(() => detector.Empty);
            Assert.Throws<ObjectDisposedException>(() => detector.DescriptorSize);
            Assert.Throws<ObjectDisposedException>(() => detector.DescriptorType);
            Assert.Throws<ObjectDisposedException>(() => detector.DefaultNorm);
            Assert.Throws<ObjectDisposedException>(() => detector.DefaultName);
            Assert.Throws<ObjectDisposedException>(() => detector.Detect(image));
            Assert.Throws<ObjectDisposedException>(() => detector.GetBlobContours());
            Assert.Equal("{Disposed=True}", detector.ToString());
        }

        private static SimpleBlobDetector? TryCreateDetector()
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
