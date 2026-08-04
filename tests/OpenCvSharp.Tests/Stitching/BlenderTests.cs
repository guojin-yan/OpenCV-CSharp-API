using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Stitching;

namespace JYPPX.OpenCvSharp.Tests.Stitching
{
    public sealed class BlenderTests
    {
        [Fact]
        public void BlenderTypeAndConstructorValidationFailClosed()
        {
            Assert.Equal(0, (int)BlenderType.None);
            Assert.Equal(1, (int)BlenderType.Feather);
            Assert.Equal(2, (int)BlenderType.MultiBand);
            Assert.Throws<ArgumentOutOfRangeException>(() => Blender.CreateDefault((BlenderType)3));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FeatherBlender(float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FeatherBlender(float.PositiveInfinity));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MultiBandBlender(numberOfBands: -1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MultiBandBlender(numberOfBands: 31));
            Assert.Throws<ArgumentOutOfRangeException>(() => new MultiBandBlender(weightType: MatType.CV_8UC1));
        }

        [Fact]
        public void FactoriesAndPropertiesMatchOpenCvWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (Blender none = Blender.CreateDefault(BlenderType.None))
            using (Blender featherDefault = Blender.CreateDefault(BlenderType.Feather))
            using (Blender multiDefault = Blender.CreateDefault(BlenderType.MultiBand, tryGpu: true))
            using (var feather = new FeatherBlender(-0.25f))
            using (var multi = new MultiBandBlender(tryGpu: true, numberOfBands: 3, weightType: MatType.CV_16SC1))
            {
                Assert.IsType<Blender>(none);
                Assert.IsType<FeatherBlender>(featherDefault);
                Assert.IsType<MultiBandBlender>(multiDefault);
                Assert.Equal(-0.25f, feather.Sharpness);
                feather.Sharpness = 0.125f;
                Assert.Equal(0.125f, feather.Sharpness);
                Assert.Equal(3, multi.NumberOfBands);
                multi.NumberOfBands = 1;
                Assert.Equal(1, multi.NumberOfBands);
                Assert.Throws<ArgumentOutOfRangeException>(() => feather.Sharpness = float.NegativeInfinity);
                Assert.Throws<ArgumentOutOfRangeException>(() => multi.NumberOfBands = 31);
            }
        }

        [Fact]
        public void BaseBlenderSupportsBothPrepareFormsAndOverwritesMaskedPixelsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var first = new Mat(4, 4, MatType.CV_16SC3, new Scalar(10, 20, 30)))
            using (var second = new Mat(4, 4, MatType.CV_16SC3, new Scalar(40, 50, 60)))
            using (var mask = new Mat(4, 4, MatType.CV_8UC1, new Scalar(255)))
            using (var destination = new Mat())
            using (var destinationMask = new Mat())
            using (var blender = Blender.CreateDefault(BlenderType.None))
            {
                blender.Prepare(
                    new[] { new Point(-1, 2), new Point(3, 2) },
                    new[] { new Size(4, 4), new Size(4, 4) });
                blender.Feed(first, mask, new Point(-1, 2));
                blender.Feed(second, mask, new Point(3, 2));
                blender.Blend(destination, destinationMask);

                Assert.Equal(new Size(8, 4), new Size(destination.Cols, destination.Rows));
                Assert.Equal(MatType.CV_16SC3, destination.Type);
                Assert.Equal(MatType.CV_8UC1, destinationMask.Type);
                Assert.Equal(255.0, Cv2.Mean(destinationMask).V0, 12);
                Assert.Equal(25.0, Cv2.Mean(destination).V0, 12);

                Assert.Throws<InvalidOperationException>(() => blender.Feed(first, mask, new Point(-1, 2)));
                blender.Prepare(new Rect(0, 0, 4, 4));
                blender.Blend(destination, destinationMask);
                Assert.Equal(0.0, Cv2.Mean(destinationMask).V0, 12);
                blender.Prepare(new Rect(0, 0, 4, 4));
                blender.Feed(first, mask, new Point(0, 0));
                blender.Blend(destination, destinationMask);
                Assert.Equal(10.0, Cv2.Mean(destination).V0, 12);
            }
        }

        [Fact]
        public void FeatherWeightMapsAndBlendHaveIndependentOwnershipWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var first = new Mat(4, 4, MatType.CV_16SC3, new Scalar(20, 20, 20)))
            using (var second = new Mat(4, 4, MatType.CV_16SC3, new Scalar(60, 60, 60)))
            using (var firstMask = new Mat(4, 4, MatType.CV_8UC1, new Scalar(255)))
            using (var secondMask = new Mat(4, 4, MatType.CV_8UC1, new Scalar(255)))
            using (var destination = new Mat())
            using (var destinationMask = new Mat())
            {
                var blender = new FeatherBlender(1.0f);
                Mat[] weights = blender.CreateWeightMaps(
                    new[] { firstMask, secondMask },
                    new[] { new Point(0, 0), new Point(2, 0) },
                    out Rect roi);
                try
                {
                    Assert.Equal(new Rect(0, 0, 6, 4), roi);
                    Assert.Equal(2, weights.Length);
                    Assert.All(weights, weight => Assert.Equal(MatType.CV_32FC1, weight.Type));
                    Assert.All(weights, weight => Assert.Equal(new Size(4, 4), new Size(weight.Cols, weight.Rows)));
                    Assert.All(weights, weight => Assert.InRange(Cv2.Mean(weight).V0, 0.0, 1.0));

                    blender.Prepare(roi);
                    blender.Feed(first, firstMask, new Point(0, 0));
                    blender.Feed(second, secondMask, new Point(2, 0));
                    blender.Blend(destination, destinationMask);
                    Assert.Equal(new Size(6, 4), new Size(destination.Cols, destination.Rows));
                    Assert.Equal(MatType.CV_16SC3, destination.Type);
                    Assert.InRange(Cv2.Mean(destination).V0, 20.0, 60.0);

                    blender.Dispose();
                    Assert.All(weights, weight => Assert.False(weight.Empty));
                }
                finally
                {
                    DisposeMats(weights);
                    blender.Dispose();
                }
            }
        }

        [Theory]
        [InlineData(MatType.CV_32FC1)]
        [InlineData(MatType.CV_16SC1)]
        public void MultiBandTryGpuFallsBackToCpuForBothWeightTypesWhenNativeSmokeIsEnabled(int weightType)
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var parent = new Mat(10, 10, MatType.CV_8UC3, new Scalar(32, 48, 64)))
            using (var image = parent.SubMat(new Rect(1, 1, 8, 8)))
            using (var maskParent = new Mat(10, 10, MatType.CV_8UC1, new Scalar(255)))
            using (var mask = maskParent.SubMat(new Rect(1, 1, 8, 8)))
            using (var destination = new Mat())
            using (var destinationMask = new Mat())
            using (var blender = new MultiBandBlender(tryGpu: true, numberOfBands: 2, weightType: weightType))
            {
                Assert.False(image.IsContinuous);
                Assert.False(mask.IsContinuous);
                blender.Prepare(new Rect(0, 0, 8, 8));
                blender.Feed(image, mask, new Point(0, 0));
                blender.Blend(destination, destinationMask);
                Assert.Equal(new Size(8, 8), new Size(destination.Cols, destination.Rows));
                Assert.Equal(MatType.CV_16SC3, destination.Type);
                Assert.Equal(MatType.CV_8UC1, destinationMask.Type);
                Assert.InRange(Cv2.Mean(destination).V0, 30.0, 34.0);
                Assert.Equal(255.0, Cv2.Mean(destinationMask).V0, 12);
            }
        }

        [Fact]
        public void WeightHelpersMutateOnlyCallerOwnedOutputsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var mask = new Mat(5, 6, MatType.CV_8UC1, new Scalar(255)))
            using (var weight = new Mat())
            using (var source = new Mat(5, 6, MatType.CV_16SC3, new Scalar(100, 200, 300)))
            {
                Blender.CreateWeightMap(mask, 1.0f, weight);
                Assert.Equal(MatType.CV_32FC1, weight.Type);
                Assert.Equal(new Size(6, 5), new Size(weight.Cols, weight.Rows));
                Assert.Equal(1.0, Cv2.Mean(weight).V0, 6);

                Blender.NormalizeUsingWeightMap(weight, source);
                Scalar mean = Cv2.Mean(source);
                Assert.InRange(mean.V0, 98.0, 100.0);
                Assert.InRange(mean.V1, 198.0, 200.0);
                Assert.InRange(mean.V2, 298.0, 300.0);
                Assert.Equal(255.0, Cv2.Mean(mask).V0, 12);
            }
        }

        [Fact]
        public void CpuLaplacePyramidRoundTripsRoiInputAndOwnsEveryLevelWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var parent = new Mat(9, 9, MatType.CV_8UC3, new Scalar(40, 50, 60)))
            using (var image = parent.SubMat(new Rect(1, 1, 7, 7)))
            {
                Mat[] pyramid = Blender.CreateLaplacePyramid(image, 2);
                try
                {
                    Assert.Equal(3, pyramid.Length);
                    Assert.Equal(new Size(7, 7), new Size(pyramid[0].Cols, pyramid[0].Rows));
                    Assert.Equal(new Size(4, 4), new Size(pyramid[1].Cols, pyramid[1].Rows));
                    Assert.Equal(new Size(2, 2), new Size(pyramid[2].Cols, pyramid[2].Rows));
                    Assert.All(pyramid, level => Assert.Equal(MatType.CV_16SC3, level.Type));

                    Blender.RestoreImageFromLaplacePyramid(pyramid);
                    Scalar mean = Cv2.Mean(pyramid[0]);
                    Assert.InRange(mean.V0, 39.0, 41.0);
                    Assert.InRange(mean.V1, 49.0, 51.0);
                    Assert.InRange(mean.V2, 59.0, 61.0);
                }
                finally
                {
                    DisposeMats(pyramid);
                }
            }
        }

        [Fact]
        public void GpuNamedPyramidHelpersPreserveUpstreamUnavailableErrorWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var image = new Mat(8, 8, MatType.CV_8UC3, new Scalar(10, 20, 30)))
            {
                OpenCvException createError = Assert.Throws<OpenCvException>(() => Blender.CreateLaplacePyramidGpu(image, 1));
                Assert.Contains("CUDA optimization is unavailable", createError.Message, StringComparison.Ordinal);

                Mat[] pyramid = Blender.CreateLaplacePyramid(image, 1);
                try
                {
                    OpenCvException restoreError = Assert.Throws<OpenCvException>(() => Blender.RestoreImageFromLaplacePyramidGpu(pyramid));
                    Assert.Contains("CUDA optimization is unavailable", restoreError.Message, StringComparison.Ordinal);
                }
                finally
                {
                    DisposeMats(pyramid);
                }
            }
        }

        [Fact]
        public void StateShapeTypeAndDisposalValidationFailClosedWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            using (var image16 = new Mat(4, 4, MatType.CV_16SC3, new Scalar(1, 2, 3)))
            using (var image8 = new Mat(4, 4, MatType.CV_8UC3, new Scalar(1, 2, 3)))
            using (var mask = new Mat(4, 4, MatType.CV_8UC1, new Scalar(255)))
            using (var wrongMask = new Mat(3, 4, MatType.CV_8UC1, new Scalar(255)))
            using (var output = new Mat())
            {
                var blender = Blender.CreateDefault(BlenderType.None);
                Assert.Throws<InvalidOperationException>(() => blender.Feed(image16, mask, new Point()));
                Assert.Throws<InvalidOperationException>(() => blender.Blend(output, new Mat()));
                Assert.Throws<ArgumentException>(() => blender.Prepare(Array.Empty<Point>(), Array.Empty<Size>()));
                Assert.Throws<ArgumentException>(() => blender.Prepare(new[] { new Point() }, Array.Empty<Size>()));
                Assert.Throws<ArgumentOutOfRangeException>(() => blender.Prepare(new Rect(0, 0, 0, 4)));

                blender.Prepare(new Rect(0, 0, 4, 4));
                Assert.Throws<ArgumentException>(() => blender.Feed(image8, mask, new Point()));
                Assert.Throws<ArgumentException>(() => blender.Feed(image16, wrongMask, new Point()));
                Assert.Throws<ArgumentOutOfRangeException>(() => blender.Feed(image16, mask, new Point(1, 0)));
                Assert.Throws<ArgumentException>(() => blender.Blend(output, output));

                blender.Dispose();
                blender.Dispose();
                Assert.True(blender.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => blender.Prepare(new Rect(0, 0, 4, 4)));
                Assert.Throws<ObjectDisposedException>(() => blender.Feed(image16, mask, new Point()));
            }
        }

        private static void DisposeMats(Mat[] mats)
        {
            foreach (Mat mat in mats) mat.Dispose();
        }
    }
}
