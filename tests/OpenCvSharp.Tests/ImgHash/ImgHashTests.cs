using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgHash;
using JYPPX.OpenCvSharp.ImgProc;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Tests.ImgHash
{
    public sealed class ImgHashTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvImgHashConstants()
        {
            Assert.Equal(0, (int)BlockMeanHashMode.Mode0);
            Assert.Equal(1, (int)BlockMeanHashMode.Mode1);
        }

        [Fact]
        public void StaticFunctionsValidateManagedArguments()
        {
            using (var mat = new Mat())
            using (var unsupportedInput = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.25)))
            {
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.AverageHash(null!, mat));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.AverageHash(null!));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.AverageHash(mat, null!));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.AverageHash(mat, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.AverageHash(mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.AverageHash(unsupportedInput, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.AverageHash(unsupportedInput));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.PHash(null!, mat));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.PHash(null!));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.PHash(mat, null!));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.PHash(mat, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.PHash(mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.PHash(unsupportedInput, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.PHash(unsupportedInput));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.BlockMeanHash(null!, mat));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.BlockMeanHash(null!));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.BlockMeanHash(mat, null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgHashCv2.BlockMeanHash(mat, mat, (BlockMeanHashMode)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => ImgHashCv2.BlockMeanHash(mat, (BlockMeanHashMode)99));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.BlockMeanHash(mat, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.BlockMeanHash(mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.BlockMeanHash(unsupportedInput, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.BlockMeanHash(unsupportedInput));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.ColorMomentHash(null!, mat));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.ColorMomentHash(null!));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.ColorMomentHash(mat, null!));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.ColorMomentHash(mat, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.ColorMomentHash(mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.ColorMomentHash(unsupportedInput, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.ColorMomentHash(unsupportedInput));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.MarrHildrethHash(null!, mat));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.MarrHildrethHash(null!));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.MarrHildrethHash(mat, null!));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.MarrHildrethHash(mat, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.MarrHildrethHash(mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.MarrHildrethHash(unsupportedInput, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.MarrHildrethHash(unsupportedInput));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.RadialVarianceHash(null!, mat));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.RadialVarianceHash(null!));
                Assert.Throws<ArgumentNullException>(() => ImgHashCv2.RadialVarianceHash(mat, null!));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.RadialVarianceHash(mat, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.RadialVarianceHash(mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.RadialVarianceHash(unsupportedInput, mat));
                Assert.Throws<ArgumentException>(() => ImgHashCv2.RadialVarianceHash(unsupportedInput));
            }
        }

        [Fact]
        public void BlockMeanHashRejectsInvalidModeBeforeNativeCall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BlockMeanHash.Create((BlockMeanHashMode)99));
        }

        [Fact]
        public void ObjectHashRejectsInvalidInputBeforeNativeCall()
        {
            AverageHash average;
            try
            {
                average = AverageHash.Create();
            }
            catch (OpenCvException ex) when (
                ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Assert.Contains("NOT_LINKED", ex.Message, StringComparison.OrdinalIgnoreCase);
                return;
            }

            using (var empty = new Mat())
            using (var unsupportedInput = new Mat(8, 8, MatType.CV_32FC1, new Scalar(0.25)))
            using (var output = new Mat())
            using (average)
            {
                Assert.Throws<ArgumentException>(() => average.Compute(empty, output));
                Assert.Throws<ArgumentException>(() => average.Compute(empty));
                Assert.Throws<ArgumentException>(() => average.Compute(unsupportedInput, output));
                Assert.Throws<ArgumentException>(() => average.Compute(unsupportedInput));
            }
        }

        [Fact]
        public void ObjectDisposedStateThrowsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = CreateImage())
            using (var output = new Mat())
            {
                AverageHash average = AverageHash.Create();
                average.Dispose();

                Assert.True(average.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => average.Compute(image, output));
                Assert.Throws<ObjectDisposedException>(() => average.Compute(image));
                Assert.Throws<ObjectDisposedException>(() => average.Compare(output, output));

                BlockMeanHash block = BlockMeanHash.Create();
                block.Dispose();

                Assert.True(block.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => block.SetMode(BlockMeanHashMode.Mode1));
                Assert.Throws<ObjectDisposedException>(() => block.GetMean());

                MarrHildrethHash marr = MarrHildrethHash.Create();
                marr.Dispose();

                Assert.True(marr.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => marr.GetKernelParam(out _, out _));
                Assert.Throws<ObjectDisposedException>(() => marr.Alpha);
                Assert.Throws<ObjectDisposedException>(() => marr.Scale);
                Assert.Throws<ObjectDisposedException>(() => marr.SetKernelParam(2.0F, 1.0F));

                RadialVarianceHash radial = RadialVarianceHash.Create();
                radial.Dispose();

                Assert.True(radial.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => radial.GetParameters(out _, out _));
                Assert.Throws<ObjectDisposedException>(() => radial.Sigma);
                Assert.Throws<ObjectDisposedException>(() => radial.Sigma = 1.0);
                Assert.Throws<ObjectDisposedException>(() => radial.NumOfAngleLine);
                Assert.Throws<ObjectDisposedException>(() => radial.NumOfAngleLine = 180);
            }
        }

        [Fact]
        public void ObjectHashSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = CreateImage())
            using (var average = AverageHash.Create())
            using (var phash = PHash.Create())
            using (var block = BlockMeanHash.Create(BlockMeanHashMode.Mode0))
            using (var color = ColorMomentHash.Create())
            using (var marr = MarrHildrethHash.Create(2.0F, 1.0F))
            using (var radial = RadialVarianceHash.Create(1.0, 180))
            using (Mat averageHash = average.Compute(image))
            using (Mat phashHash = phash.Compute(image))
            using (Mat blockHash = block.Compute(image))
            using (Mat colorHash = color.Compute(image))
            using (Mat marrHash = marr.Compute(image))
            using (Mat radialHash = radial.Compute(image))
            {
                block.SetMode(BlockMeanHashMode.Mode0);
                marr.SetKernelParam(2.0F, 1.0F);
                radial.Sigma = 1.0;
                radial.NumOfAngleLine = 180;

                Assert.False(averageHash.Empty);
                Assert.False(phashHash.Empty);
                Assert.False(blockHash.Empty);
                Assert.False(colorHash.Empty);
                Assert.False(marrHash.Empty);
                Assert.False(radialHash.Empty);
                Assert.Equal(MatType.CV_64F, MatType.Depth(colorHash.Type));
                Assert.Equal(0.0, block.Compare(blockHash, blockHash), 6);
                Assert.Equal(2.0F, marr.Alpha, 3);
                Assert.Equal(1.0F, marr.Scale, 3);
                Assert.Equal(1.0, radial.Sigma, 3);
                Assert.Equal(180, radial.NumOfAngleLine);
                Assert.NotNull(block.GetMean());
            }
        }

        [Fact]
        public void RadialVarianceHashParametersRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var radial = RadialVarianceHash.Create(2.5, 90))
            {
                radial.GetParameters(out double sigma, out int numOfAngleLine);

                Assert.Equal(2.5, sigma, 6);
                Assert.Equal(90, numOfAngleLine);
                Assert.Equal(2.5, radial.Sigma, 6);
                Assert.Equal(90, radial.NumOfAngleLine);

                radial.Sigma = 3.25;
                radial.NumOfAngleLine = 120;
                radial.GetParameters(out sigma, out numOfAngleLine);

                Assert.Equal(3.25, sigma, 6);
                Assert.Equal(120, numOfAngleLine);
                Assert.Equal(3.25, radial.Sigma, 6);
                Assert.Equal(120, radial.NumOfAngleLine);
            }
        }

        [Fact]
        public void RadialVarianceHashRejectsInvalidSetterParametersBeforeNativeCall()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var radial = RadialVarianceHash.Create())
            {
                Assert.Throws<ArgumentOutOfRangeException>(() => radial.Sigma = 0.999);
                Assert.Throws<ArgumentOutOfRangeException>(() => radial.NumOfAngleLine = 0);
            }
        }

        [Fact]
        public void MarrHildrethHashKernelParametersRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var marr = MarrHildrethHash.Create(3.0F, 1.5F))
            {
                marr.GetKernelParam(out float alpha, out float scale);

                Assert.Equal(3.0F, alpha, 6);
                Assert.Equal(1.5F, scale, 6);
                Assert.Equal(3.0F, marr.Alpha, 6);
                Assert.Equal(1.5F, marr.Scale, 6);

                marr.SetKernelParam(4.0F, 2.0F);
                marr.GetKernelParam(out alpha, out scale);

                Assert.Equal(4.0F, alpha, 6);
                Assert.Equal(2.0F, scale, 6);
                Assert.Equal(4.0F, marr.Alpha, 6);
                Assert.Equal(2.0F, marr.Scale, 6);
            }
        }

        [Fact]
        public void StaticHashSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var image = CreateImage())
            using (Mat average = ImgHashCv2.AverageHash(image))
            using (Mat phash = ImgHashCv2.PHash(image))
            using (Mat block = ImgHashCv2.BlockMeanHash(image, BlockMeanHashMode.Mode0))
            using (Mat color = ImgHashCv2.ColorMomentHash(image))
            using (Mat marr = ImgHashCv2.MarrHildrethHash(image))
            using (Mat radial = ImgHashCv2.RadialVarianceHash(image))
            {
                Assert.False(average.Empty);
                Assert.False(phash.Empty);
                Assert.False(block.Empty);
                Assert.False(color.Empty);
                Assert.False(marr.Empty);
                Assert.False(radial.Empty);
                Assert.Equal(MatType.CV_64F, MatType.Depth(color.Type));
            }
        }

        private static Mat CreateImage()
        {
            var image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(16, 32, 48));
            ImgProcCv2.Rectangle(image, new Rect(4, 4, 10, 10), new Scalar(220, 40, 30), -1);
            ImgProcCv2.Circle(image, new Point(23, 22), 6, new Scalar(30, 200, 120), -1);
            return image;
        }

    }
}
