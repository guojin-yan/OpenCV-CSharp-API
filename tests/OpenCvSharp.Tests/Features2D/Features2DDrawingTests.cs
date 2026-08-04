using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using Features2DCv2 = JYPPX.OpenCvSharp.Features2D.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Features2D
{
    public class Features2DDrawingTests
    {
        [Fact]
        public void DrawFunctionsRejectNullLeadingInputsBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawKeypoints(null!, Array.Empty<KeyPoint>(), null!));
            Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(null!, Array.Empty<KeyPoint>(), null!, Array.Empty<KeyPoint>(), Array.Empty<DMatch>(), null!));
            Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatchesKnn(null!, Array.Empty<KeyPoint>(), null!, Array.Empty<KeyPoint>(), Array.Empty<DMatch[]>(), null!));

#if NETCOREAPP3_1_OR_GREATER
            Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawKeypoints(null!, ReadOnlySpan<KeyPoint>.Empty, null!));
            Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(null!, ReadOnlySpan<KeyPoint>.Empty, null!, ReadOnlySpan<KeyPoint>.Empty, ReadOnlySpan<DMatch>.Empty, null!));
#endif
        }

        [Fact]
        public void DrawKeypointsValidatesManagedArguments()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = new Mat())
            using (var outImage = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawKeypoints(null!, Array.Empty<KeyPoint>(), outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawKeypoints(image, null!, outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawKeypoints(image, Array.Empty<KeyPoint>(), null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => Features2DCv2.DrawKeypoints(image, Array.Empty<KeyPoint>(), outImage, flags: (DrawMatchesFlags)8));
            }
        }

        [Fact]
        public void DrawMatchesValidatesManagedArguments()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image1 = new Mat())
            using (var image2 = new Mat())
            using (var outImage = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(null!, Array.Empty<KeyPoint>(), image2, Array.Empty<KeyPoint>(), Array.Empty<DMatch>(), outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(image1, null!, image2, Array.Empty<KeyPoint>(), Array.Empty<DMatch>(), outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(image1, Array.Empty<KeyPoint>(), null!, Array.Empty<KeyPoint>(), Array.Empty<DMatch>(), outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(image1, Array.Empty<KeyPoint>(), image2, null!, Array.Empty<DMatch>(), outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(image1, Array.Empty<KeyPoint>(), image2, Array.Empty<KeyPoint>(), null!, outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(image1, Array.Empty<KeyPoint>(), image2, Array.Empty<KeyPoint>(), Array.Empty<DMatch>(), null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => Features2DCv2.DrawMatches(image1, Array.Empty<KeyPoint>(), image2, Array.Empty<KeyPoint>(), Array.Empty<DMatch>(), outImage, flags: (DrawMatchesFlags)8));
            }
        }

        [Fact]
        public void DrawMatchesKnnValidatesManagedArguments()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image1 = new Mat())
            using (var image2 = new Mat())
            using (var outImage = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatchesKnn(null!, Array.Empty<KeyPoint>(), image2, Array.Empty<KeyPoint>(), Array.Empty<DMatch[]>(), outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatchesKnn(image1, null!, image2, Array.Empty<KeyPoint>(), Array.Empty<DMatch[]>(), outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatchesKnn(image1, Array.Empty<KeyPoint>(), null!, Array.Empty<KeyPoint>(), Array.Empty<DMatch[]>(), outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatchesKnn(image1, Array.Empty<KeyPoint>(), image2, null!, Array.Empty<DMatch[]>(), outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatchesKnn(image1, Array.Empty<KeyPoint>(), image2, Array.Empty<KeyPoint>(), null!, outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatchesKnn(image1, Array.Empty<KeyPoint>(), image2, Array.Empty<KeyPoint>(), Array.Empty<DMatch[]>(), null!));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatchesKnn(image1, Array.Empty<KeyPoint>(), image2, Array.Empty<KeyPoint>(), new DMatch[][] { null! }, outImage));
                Assert.Throws<ArgumentOutOfRangeException>(() => Features2DCv2.DrawMatchesKnn(image1, Array.Empty<KeyPoint>(), image2, Array.Empty<KeyPoint>(), Array.Empty<DMatch[]>(), outImage, flags: (DrawMatchesFlags)8));
            }
        }

        [Fact]
        public void DrawFunctionsReportDefinedBoundaryWhenFeaturesModuleIsNotLinked()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image1 = new Mat(24, 24, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (Mat image2 = new Mat(24, 24, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (Mat outImage = new Mat())
            {
                var keypoints1 = new[] { new KeyPoint(8.0F, 8.0F, 5.0F) };
                var keypoints2 = new[] { new KeyPoint(10.0F, 10.0F, 5.0F) };
                var matches = new[] { new DMatch(0, 0, 0.0F) };
                var groupedMatches = new[] { matches };

                OpenCvException? keypointException = Record.Exception(() =>
                    Features2DCv2.DrawKeypoints(image1, keypoints1, outImage, new Scalar(0, 255, 0))) as OpenCvException;
                OpenCvException? matchesException = Record.Exception(() =>
                    Features2DCv2.DrawMatches(image1, keypoints1, image2, keypoints2, matches, outImage)) as OpenCvException;
                OpenCvException? knnException = Record.Exception(() =>
                    Features2DCv2.DrawMatchesKnn(image1, keypoints1, image2, keypoints2, groupedMatches, outImage)) as OpenCvException;

                AssertBoundaryOrSuccess(keypointException, "features2d_draw_keypoints", outImage);
                AssertBoundaryOrSuccess(matchesException, "features2d_draw_matches", outImage);
                AssertBoundaryOrSuccess(knnException, "features2d_draw_matches_knn", outImage);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        [Fact]
        public void SpanDrawOverloadsValidateManagedArguments()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image1 = new Mat())
            using (var image2 = new Mat())
            using (var outImage = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawKeypoints(null!, ReadOnlySpan<KeyPoint>.Empty, outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawKeypoints(image1, ReadOnlySpan<KeyPoint>.Empty, null!));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(null!, ReadOnlySpan<KeyPoint>.Empty, image2, ReadOnlySpan<KeyPoint>.Empty, ReadOnlySpan<DMatch>.Empty, outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(image1, ReadOnlySpan<KeyPoint>.Empty, null!, ReadOnlySpan<KeyPoint>.Empty, ReadOnlySpan<DMatch>.Empty, outImage));
                Assert.Throws<ArgumentNullException>(() => Features2DCv2.DrawMatches(image1, ReadOnlySpan<KeyPoint>.Empty, image2, ReadOnlySpan<KeyPoint>.Empty, ReadOnlySpan<DMatch>.Empty, null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => Features2DCv2.DrawKeypoints(image1, ReadOnlySpan<KeyPoint>.Empty, outImage, flags: (DrawMatchesFlags)8));
                Assert.Throws<ArgumentOutOfRangeException>(() => Features2DCv2.DrawMatches(image1, ReadOnlySpan<KeyPoint>.Empty, image2, ReadOnlySpan<KeyPoint>.Empty, ReadOnlySpan<DMatch>.Empty, outImage, flags: (DrawMatchesFlags)8));
            }
        }

        [Fact]
        public void SpanDrawOverloadsFollowSameBoundaryAsArrayOverloads()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image1 = new Mat(24, 24, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (Mat image2 = new Mat(24, 24, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (Mat outImage = new Mat())
            {
                Span<KeyPoint> keypoints1 = stackalloc KeyPoint[] { new KeyPoint(8.0F, 8.0F, 5.0F) };
                Span<KeyPoint> keypoints2 = stackalloc KeyPoint[] { new KeyPoint(10.0F, 10.0F, 5.0F) };
                Span<DMatch> matches = stackalloc DMatch[] { new DMatch(0, 0, 0.0F) };

                OpenCvException? keypointException = null;
                OpenCvException? matchesException = null;

                try
                {
                    Features2DCv2.DrawKeypoints(image1, keypoints1, outImage, new Scalar(0, 255, 0));
                }
                catch (OpenCvException ex)
                {
                    keypointException = ex;
                }

                try
                {
                    Features2DCv2.DrawMatches(image1, keypoints1, image2, keypoints2, matches, outImage);
                }
                catch (OpenCvException ex)
                {
                    matchesException = ex;
                }

                AssertBoundaryOrSuccess(keypointException, "features2d_draw_keypoints", outImage);
                AssertBoundaryOrSuccess(matchesException, "features2d_draw_matches", outImage);
            }
        }
#endif

        private static void AssertBoundaryOrSuccess(OpenCvException? exception, string apiName, Mat outImage)
        {
            if (exception != null)
            {
                Assert.Contains(apiName, exception.Message, StringComparison.OrdinalIgnoreCase);
                Assert.Contains("OpenCV", exception.Message, StringComparison.OrdinalIgnoreCase);
                return;
            }

            Assert.False(outImage.Empty);
            Assert.True(outImage.Rows > 0);
            Assert.True(outImage.Cols > 0);
        }

    }
}
