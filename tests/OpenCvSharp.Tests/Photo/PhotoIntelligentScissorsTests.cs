using System;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Photo;

namespace JYPPX.OpenCvSharp.Tests.Photo
{
    public sealed class PhotoIntelligentScissorsTests
    {
        [Fact]
        public void DefaultImageProducesIndependentForwardAndBackwardContours()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using var image = CreateRectangleImage(32, 32, MatType.CV_8UC1, 1);
            using var scissors = new IntelligentScissorsMB();
            Assert.False(scissors.IsDisposed);
            Assert.Same(scissors, scissors.ApplyImage(image));
            scissors.BuildMap(new Point(4, 4));

            using var callerOwned = new Mat();
            scissors.GetContour(new Point(27, 4), callerOwned);
            using Mat forward = scissors.GetContour(new Point(27, 4));
            using Mat backward = scissors.GetContour(new Point(27, 4), backward: true);

            AssertContour(forward, new Point(4, 4), new Point(27, 4));
            AssertContour(backward, new Point(27, 4), new Point(4, 4));
            Assert.Equal(forward.ToArray<int>(), callerOwned.ToArray<int>());
            callerOwned.SetTo(new Scalar(0));
            Assert.Equal(4, forward.ToArray<int>()[0]);
        }

        [Fact]
        public void CannySupportsBgraRoiAndConfigurationInvalidatesFeatures()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using var source = CreateRectangleImage(32, 40, MatType.CV_8UC4, 4);
            using var roi = source.SubMat(new Rect(4, 4, 32, 24));
            using var scissors = new IntelligentScissorsMB();
            Assert.Same(scissors, scissors.SetEdgeFeatureCannyParameters(20.0, 60.0, 3, l2Gradient: true));
            Assert.Same(scissors, scissors.SetGradientMagnitudeMaxLimit(16.0F));
            scissors.ApplyImage(roi).BuildMap(new Point(2, 2));
            using (Mat contour = scissors.GetContour(new Point(29, 2)))
            {
                AssertContour(contour, new Point(2, 2), new Point(29, 2));
            }

            scissors.SetEdgeFeatureZeroCrossingParameters(2.0F);
            Assert.Throws<InvalidOperationException>(() => scissors.BuildMap(new Point(2, 2)));
            Assert.Throws<InvalidOperationException>(() => scissors.GetContour(new Point(29, 2)));
            scissors.ApplyImage(roi).BuildMap(new Point(2, 2));
            using Mat rebuilt = scissors.GetContour(new Point(29, 2));
            Assert.True(rebuilt.Rows > 1);
        }

        [Fact]
        public void CustomFeaturesRetainStorageAndAllowZeroWeightOmissions()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            var nonEdge = new Mat(12, 12, MatType.CV_8UC1, new Scalar(1));
            var gradientDirection = new Mat(12, 12, MatType.CV_32FC2, new Scalar(0, 0, 0, 0));
            var gradientMagnitude = new Mat(12, 12, MatType.CV_32FC1, new Scalar(0));
            using var scissors = new IntelligentScissorsMB();
            scissors.SetWeights(0.9F, 0.0F, 0.1F)
                .ApplyImageFeatures(nonEdge, gradientDirection, gradientMagnitude);
            nonEdge.Dispose();
            gradientDirection.Dispose();
            gradientMagnitude.Dispose();

            scissors.BuildMap(new Point(1, 1));
            using Mat retained = scissors.GetContour(new Point(10, 1));
            AssertContour(retained, new Point(1, 1), new Point(10, 1));

            using var nonEdgeOnly = new Mat(8, 8, MatType.CV_8UC1, new Scalar(255));
            using var noDirectionNeeded = new IntelligentScissorsMB();
            noDirectionNeeded.SetWeights(1.0F, 0.0F, 0.0F)
                .ApplyImageFeatures(nonEdgeOnly, null, null)
                .BuildMap(new Point(0, 0));
            using Mat hintedValuesAreNotWrapperRestricted = noDirectionNeeded.GetContour(new Point(7, 0));
            AssertContour(hintedValuesAreNotWrapperRestricted, new Point(0, 0), new Point(7, 0));
        }

        [Fact]
        public void OptionalImageDerivesMissingFeaturesAndAcceptsNonContiguousInputs()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using var imageParent = CreateRectangleImage(18, 20, MatType.CV_8UC3, 3);
            using var imageRoi = imageParent.SubMat(new Rect(2, 2, 16, 14));
            using var nonEdgeParent = new Mat(18, 20, MatType.CV_8UC1, new Scalar(1));
            using var nonEdgeRoi = nonEdgeParent.SubMat(new Rect(2, 2, 16, 14));
            using var scissors = new IntelligentScissorsMB();
            scissors.ApplyImageFeatures(nonEdgeRoi, null, null, imageRoi)
                .BuildMap(new Point(1, 1));
            using Mat contour = scissors.GetContour(new Point(14, 1));
            AssertContour(contour, new Point(1, 1), new Point(14, 1));

            using var allEmptyImage = new Mat();
            using var imageOnly = new IntelligentScissorsMB();
            Assert.Throws<ArgumentException>(() => imageOnly.ApplyImageFeatures(null, null, null, allEmptyImage));
            imageOnly.ApplyImageFeatures(null, null, null, imageRoi).BuildMap(new Point(1, 1));
            using Mat imageDerived = imageOnly.GetContour(new Point(14, 1));
            Assert.True(imageDerived.Rows > 1);
        }

        [Fact]
        public void InvalidArgumentsStateAndDisposalFailDeterministically()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using var scissors = new IntelligentScissorsMB();
            using var image = CreateRectangleImage(10, 10, MatType.CV_8UC1, 1);
            using var wrongImage = new Mat(10, 10, MatType.CV_32FC1);
            using var wrongFeature = new Mat(10, 10, MatType.CV_8UC1);
            using var sizeMismatch = new Mat(9, 10, MatType.CV_32FC1);
            using var output = new Mat();

            Assert.Throws<InvalidOperationException>(() => scissors.BuildMap(new Point(0, 0)));
            Assert.Throws<InvalidOperationException>(() => scissors.GetContour(new Point(0, 0), output));
            Assert.Throws<ArgumentNullException>(() => scissors.ApplyImage(null!));
            Assert.Throws<ArgumentException>(() => scissors.ApplyImage(wrongImage));
            Assert.Throws<ArgumentOutOfRangeException>(() => scissors.SetWeights(-1.0F, 1.0F, 1.0F));
            Assert.Throws<ArgumentOutOfRangeException>(() => scissors.SetWeights(0.0F, 0.0F, 0.0F));
            Assert.Throws<ArgumentOutOfRangeException>(() => scissors.SetWeights(float.NaN, 1.0F, 1.0F));
            Assert.Throws<ArgumentOutOfRangeException>(() => scissors.SetGradientMagnitudeMaxLimit(float.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => scissors.SetEdgeFeatureZeroCrossingParameters(-1.0F));
            Assert.Throws<ArgumentOutOfRangeException>(() => scissors.SetEdgeFeatureCannyParameters(-1.0, 2.0));
            Assert.Throws<ArgumentOutOfRangeException>(() => scissors.SetEdgeFeatureCannyParameters(1.0, 2.0, apertureSize: 4));
            Assert.Throws<ArgumentException>(() => scissors.ApplyImageFeatures(null, null, null));
            Assert.Throws<ArgumentException>(() => scissors.ApplyImageFeatures(wrongFeature, wrongFeature, null, image));
            Assert.Throws<ArgumentException>(() => scissors.ApplyImageFeatures(wrongFeature, null, sizeMismatch, image));

            scissors.ApplyImage(image);
            Assert.Throws<ArgumentOutOfRangeException>(() => scissors.BuildMap(new Point(-1, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => scissors.BuildMap(new Point(10, 0)));
            scissors.BuildMap(new Point(0, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => scissors.GetContour(new Point(0, -1), output));
            Assert.Throws<ArgumentNullException>(() => scissors.GetContour(new Point(9, 0), null!));

            var disposed = new IntelligentScissorsMB();
            disposed.Dispose();
            disposed.Dispose();
            Assert.True(disposed.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => disposed.ApplyImage(image));
        }

        private static Mat CreateRectangleImage(int rows, int cols, int type, int channels)
        {
            var result = new Mat(rows, cols, type);
            var values = new byte[rows * cols * channels];
            int left = 2;
            int right = cols - 3;
            int top = 2;
            int bottom = rows - 3;
            for (int x = left; x <= right; x++)
            {
                SetPixel(values, cols, channels, x, top, 255);
                SetPixel(values, cols, channels, x, bottom, 255);
            }
            for (int y = top; y <= bottom; y++)
            {
                SetPixel(values, cols, channels, left, y, 255);
                SetPixel(values, cols, channels, right, y, 255);
            }
            result.CopyFrom(values);
            return result;
        }

        private static void SetPixel(byte[] values, int cols, int channels, int x, int y, byte value)
        {
            int offset = (y * cols + x) * channels;
            for (int channel = 0; channel < channels; channel++) values[offset + channel] = value;
        }

        private static void AssertContour(Mat contour, Point first, Point last)
        {
            Assert.Equal(MatType.CV_32SC2, contour.Type);
            Assert.Equal(1, contour.Cols);
            Assert.True(contour.Rows >= 1);
            int[] values = contour.ToArray<int>();
            Assert.Equal(first.X, values[0]);
            Assert.Equal(first.Y, values[1]);
            Assert.Equal(last.X, values[values.Length - 2]);
            Assert.Equal(last.Y, values[values.Length - 1]);
        }
    }
}
