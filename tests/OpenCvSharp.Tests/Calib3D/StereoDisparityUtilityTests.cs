using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;

namespace OpenCvSharp.Tests.Calib3D
{
    [Collection(NativeSmokeCollection.Name)]
    public sealed class StereoDisparityUtilityTests
    {
        [Fact]
        public void StereoDisparityUtilitiesValidateInputsAndOwnership()
        {
            using var image16 = new Mat(3, 3, MatType.CV_16SC1, new Scalar(16));
            using var image8 = new Mat(3, 3, MatType.CV_8UC1, new Scalar(1));
            using var output = new Mat();
            using Mat q = CreateQ();
            using var cost16 = new Mat(3, 3, MatType.CV_16SC1, new Scalar(1));
            using var empty = new Mat();
            using var wrongSpeckleType = new Mat(3, 3, MatType.CV_32FC1);
            using var wrongDisparityType = new Mat(3, 3, MatType.CV_32SC1);
            using var wrongCostType = new Mat(3, 3, MatType.CV_32FC1);
            using var mismatchedCost = new Mat(2, 3, MatType.CV_16SC1);
            using var invalidQ = new Mat(3, 4, MatType.CV_64FC1);
            using var invalidQChannels = new Mat(4, 4, MatType.CV_64FC2);
            using var invalidReprojectType = new Mat(2, 2, MatType.CV_64FC1);

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.FilterSpeckles(null!, 0.0, 1, 0.0));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FilterSpeckles(empty, 0.0, 1, 0.0));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FilterSpeckles(wrongSpeckleType, 0.0, 1, 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.FilterSpeckles(image16, double.NaN, 1, 0.0));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.FilterSpeckles(image16, 0.0, 1, double.PositiveInfinity));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.FilterSpeckles(image16, 0.0, 1, 0.0, image16));

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.ValidateDisparity(null!, cost16, 0, 16));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.ValidateDisparity(image16, null!, 0, 16));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ValidateDisparity(wrongDisparityType, cost16, 0, 16));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ValidateDisparity(image16, wrongCostType, 0, 16));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ValidateDisparity(image16, mismatchedCost, 0, 16));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.ValidateDisparity(image16, cost16, 0, 0));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ValidateDisparity(image16, image16, 0, 16));

            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.ReprojectImageTo3D(
                    disparity: null!,
                    image3D: output,
                    q: q));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.ReprojectImageTo3D(
                    disparity: image8,
                    image3D: null!,
                    q: q));
            Assert.Throws<ArgumentNullException>(() =>
                Calib3DCv2.ReprojectImageTo3D(
                    disparity: image8,
                    image3D: output,
                    q: null!));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ReprojectImageTo3D(empty, output, q));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ReprojectImageTo3D(invalidReprojectType, output, q));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ReprojectImageTo3D(image8, output, invalidQ));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ReprojectImageTo3D(image8, output, invalidQChannels));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                Calib3DCv2.ReprojectImageTo3D(image8, output, q, ddepth: MatType.CV_64F));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ReprojectImageTo3D(image8, image8, q));
            Assert.Throws<ArgumentException>(() =>
                Calib3DCv2.ReprojectImageTo3D(image8, q, q));

            using var disposedBuffer = new Mat();
            disposedBuffer.Dispose();
            Assert.Throws<ObjectDisposedException>(() =>
                Calib3DCv2.FilterSpeckles(image16, 0.0, 1, 0.0, disposedBuffer));
        }

        [Fact]
        public void FilterSpecklesRemovesSmallRegionAndReusesBufferWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var disparity = new Mat(5, 5, MatType.CV_16SC1, new Scalar(1024));
            disparity.SetValue(12, (short)4096);
            using var buffer = new Mat();

            Calib3DCv2.FilterSpeckles(
                disparity,
                newValue: 1024.0,
                maxSpeckleSize: 1,
                maxDifference: 0.0,
                buffer: buffer);

            Assert.All(disparity.ToArray<short>(), value => Assert.Equal((short)1024, value));
            Assert.False(buffer.Empty);

            using var disparity8 = new Mat(3, 3, MatType.CV_8UC1, new Scalar(10));
            disparity8.SetValue(4, (byte)99);
            Calib3DCv2.FilterSpeckles(disparity8, 10.0, 1, 0.0, buffer);
            Assert.All(disparity8.ToArray<byte>(), value => Assert.Equal((byte)10, value));
        }

        [Fact]
        public void GetValidDisparityROIUsesExactOpenCvIntegerFormulaWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Rect roi1 = new Rect(0, 0, 640, 480);
            Rect roi2 = new Rect(10, 5, 620, 470);

            Rect result = Calib3DCv2.GetValidDisparityROI(
                roi1,
                roi2,
                minDisparity: 0,
                numberOfDisparities: 64,
                blockSize: 9);
            Rect empty = Calib3DCv2.GetValidDisparityROI(
                new Rect(0, 0, 8, 8),
                new Rect(0, 0, 8, 8),
                minDisparity: 0,
                numberOfDisparities: 16,
                blockSize: 9);

            Assert.Equal(new Rect(77, 9, 549, 462), result);
            Assert.True(empty.Empty);
            Assert.Equal(new Rect(0, 0, 640, 480), roi1);
            Assert.Equal(new Rect(10, 5, 620, 470), roi2);
        }

        [Fact]
        public void ValidateDisparityRejectsInconsistentCollisionAndPreservesCostsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            short[] disparities = { 0, 0, 16, 0 };
            short[] costs16 = { 5, 1, 10, 1 };
            int[] costs32 = { 5, 1, 10, 1 };
            using Mat disparity16Cost = CreateMatrix16S(1, 4, disparities);
            using Mat disparity32Cost = CreateMatrix16S(1, 4, disparities);
            using Mat cost16 = CreateMatrix16S(1, 4, costs16);
            using Mat cost32 = CreateMatrix32S(1, 4, costs32);
            short[] cost16Snapshot = cost16.ToArray<short>();
            int[] cost32Snapshot = cost32.ToArray<int>();

            Calib3DCv2.ValidateDisparity(
                disparity16Cost,
                cost16,
                minDisparity: 0,
                numberOfDisparities: 1,
                disp12MaxDifference: 0);
            Calib3DCv2.ValidateDisparity(
                disparity32Cost,
                cost32,
                minDisparity: 0,
                numberOfDisparities: 1,
                disp12MaxDifference: 0);

            short[] expected = { 0, 0, -16, 0 };
            Assert.Equal(expected, disparity16Cost.ToArray<short>());
            Assert.Equal(expected, disparity32Cost.ToArray<short>());
            Assert.Equal(cost16Snapshot, cost16.ToArray<short>());
            Assert.Equal(cost32Snapshot, cost32.ToArray<int>());
        }

        [Fact]
        public void ReprojectImageTo3DPreservesInputsDepthsAndMissingValueBehaviorWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using Mat q = CreateQ();
            using Mat disparity = CreateMatrix32F(
                2,
                3,
                0.0F, -1.0F, -2.0F,
                1.0F, 0.0F, -1.0F);
            float[] disparitySnapshot = disparity.ToArray<float>();
            double[] qSnapshot = q.ToArray<double>();
            using var callerOwned = new Mat();

            Calib3DCv2.ReprojectImageTo3D(
                disparity,
                callerOwned,
                q,
                handleMissingValues: true);
            using Mat owned = Calib3DCv2.ReprojectImageTo3D(
                disparity,
                q,
                handleMissingValues: true);

            Assert.Equal(MatType.CV_32FC3, callerOwned.Type);
            Assert.Equal(callerOwned.ToArray<float>(), owned.ToArray<float>());
            float[] expected =
            {
                0.0F, 0.0F, 0.0F,
                0.0F, 1.0F, -1.0F,
                0.0F, 2.0F, 10000.0F,
                1.0F, 0.0F, 1.0F,
                1.0F, 1.0F, 0.0F,
                1.0F, 2.0F, -1.0F
            };
            Assert.Equal(expected, owned.ToArray<float>());

            using Mat output16 = Calib3DCv2.ReprojectImageTo3D(
                disparity,
                q,
                ddepth: MatType.CV_16S);
            using Mat output32 = Calib3DCv2.ReprojectImageTo3D(
                disparity,
                q,
                ddepth: MatType.CV_32S);
            Assert.Equal(MatType.CV_16SC3, output16.Type);
            Assert.Equal(MatType.CV_32SC3, output32.Type);

            using var disparity8 = new Mat(1, 1, MatType.CV_8UC1, new Scalar(2));
            using var disparity16 = new Mat(1, 1, MatType.CV_16SC1, new Scalar(2));
            using var disparity32 = new Mat(1, 1, MatType.CV_32SC1, new Scalar(2));
            using Mat result8 = Calib3DCv2.ReprojectImageTo3D(disparity8, q);
            using Mat result16 = Calib3DCv2.ReprojectImageTo3D(disparity16, q);
            using Mat result32 = Calib3DCv2.ReprojectImageTo3D(disparity32, q);
            Assert.Equal(MatType.CV_32FC3, result8.Type);
            Assert.Equal(MatType.CV_32FC3, result16.Type);
            Assert.Equal(MatType.CV_32FC3, result32.Type);

            Assert.Equal(disparitySnapshot, disparity.ToArray<float>());
            Assert.Equal(qSnapshot, q.ToArray<double>());
        }

        private static Mat CreateQ()
        {
            return CreateMatrix64(
                4,
                4,
                0.0, 1.0, 0.0, 0.0,
                1.0, 0.0, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                0.0, 0.0, 0.0, 1.0);
        }

        private static Mat CreateMatrix64(int rows, int cols, params double[] values)
        {
            Assert.Equal(rows * cols, values.Length);
            var result = new Mat(rows, cols, MatType.CV_64FC1);
            for (int i = 0; i < values.Length; ++i)
            {
                result.SetValue(i, values[i]);
            }
            return result;
        }

        private static Mat CreateMatrix32F(int rows, int cols, params float[] values)
        {
            Assert.Equal(rows * cols, values.Length);
            var result = new Mat(rows, cols, MatType.CV_32FC1);
            for (int i = 0; i < values.Length; ++i)
            {
                result.SetValue(i, values[i]);
            }
            return result;
        }

        private static Mat CreateMatrix16S(int rows, int cols, params short[] values)
        {
            Assert.Equal(rows * cols, values.Length);
            var result = new Mat(rows, cols, MatType.CV_16SC1);
            for (int i = 0; i < values.Length; ++i)
            {
                result.SetValue(i, values[i]);
            }
            return result;
        }

        private static Mat CreateMatrix32S(int rows, int cols, params int[] values)
        {
            Assert.Equal(rows * cols, values.Length);
            var result = new Mat(rows, cols, MatType.CV_32SC1);
            for (int i = 0; i < values.Length; ++i)
            {
                result.SetValue(i, values[i]);
            }
            return result;
        }
    }
}
