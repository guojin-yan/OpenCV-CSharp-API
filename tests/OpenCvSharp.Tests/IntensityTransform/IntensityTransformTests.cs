using System;
using OpenCvSharp.Core;
using OpenCvSharp.IntensityTransform;

namespace OpenCvSharp.Tests.IntensityTransform
{
    public sealed class IntensityTransformTests
    {
        [Fact]
        public void ManagedValidationRejectsNullAndOutOfRangeArguments()
        {
            using (Mat src = new Mat(4, 4, MatType.CV_8UC1, new Scalar(32)))
            using (Mat bgr = new Mat(4, 4, MatType.CV_8UC3, new Scalar(32, 64, 128)))
            using (Mat dst = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.LogTransform(null!, dst));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.LogTransform(src, null!));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.LogTransform(null!));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.GammaCorrection(null!, dst, 1.0F));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.GammaCorrection(src, null!, 1.0F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.GammaCorrection(src, dst, 0.0F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.GammaCorrection(src, dst, float.NaN));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.GammaCorrection(null!, 1.0F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.GammaCorrection(src, 0.0F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.GammaCorrection(src, float.NaN));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.Autoscaling(null!, dst));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.Autoscaling(src, null!));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.Autoscaling(null!));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.ContrastStretching(null!, dst, 0, 0, 128, 255));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.ContrastStretching(src, null!, 0, 0, 128, 255));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.ContrastStretching(src, dst, -1, 0, 128, 255));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.ContrastStretching(src, dst, 0, 256, 128, 255));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.ContrastStretching(src, dst, 0, 0, 256, 255));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.ContrastStretching(src, dst, 0, 0, 128, 256));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.ContrastStretching(src, dst, 128, 0, 128, 255));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.ContrastStretching(null!, 0, 0, 128, 255));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.ContrastStretching(src, -1, 0, 128, 255));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.ContrastStretching(src, 0, 256, 128, 255));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.ContrastStretching(src, 0, 0, 256, 255));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.ContrastStretching(src, 0, 0, 128, 256));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.ContrastStretching(src, 128, 0, 128, 255));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.Bimef(null!, dst));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.Bimef(src, null!));
                Assert.Throws<ArgumentException>(() => IntensityTransformCv2.Bimef(src, dst));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, dst, 0.0F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, dst, float.NaN));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, dst, 0.5F, float.NaN, 1.1258F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, dst, 0.5F, -0.3293F, float.NegativeInfinity));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.Bimef(null!, dst, 1.0F, 0.5F, -0.3293F, 1.1258F));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.Bimef(src, null!, 1.0F, 0.5F, -0.3293F, 1.1258F));
                Assert.Throws<ArgumentException>(() => IntensityTransformCv2.Bimef(src, dst, 1.0F, 0.5F, -0.3293F, 1.1258F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, dst, 0.0F, 0.5F, -0.3293F, 1.1258F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, dst, 1.0F, float.PositiveInfinity, -0.3293F, 1.1258F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, dst, 1.0F, 0.5F, float.NaN, 1.1258F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, dst, 1.0F, 0.5F, -0.3293F, float.NegativeInfinity));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.Bimef(null!));
                Assert.Throws<ArgumentException>(() => IntensityTransformCv2.Bimef(src));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, 0.0F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, float.NaN));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, 0.5F, float.NaN, 1.1258F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, 0.5F, -0.3293F, float.NegativeInfinity));
                Assert.Throws<ArgumentNullException>(() => IntensityTransformCv2.Bimef(null!, 1.0F, 0.5F, -0.3293F, 1.1258F));
                Assert.Throws<ArgumentException>(() => IntensityTransformCv2.Bimef(src, 1.0F, 0.5F, -0.3293F, 1.1258F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, 0.0F, 0.5F, -0.3293F, 1.1258F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, 1.0F, float.PositiveInfinity, -0.3293F, 1.1258F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, 1.0F, 0.5F, float.NaN, 1.1258F));
                Assert.Throws<ArgumentOutOfRangeException>(() => IntensityTransformCv2.Bimef(bgr, 1.0F, 0.5F, -0.3293F, float.NegativeInfinity));
            }
        }

        [Fact]
        public void FunctionSmokeOrBoundaryRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (Mat gray = CreateGrayImage())
                using (Mat bgr = CreateBgrImage())
                using (Mat log = IntensityTransformCv2.LogTransform(gray))
                using (Mat gamma = IntensityTransformCv2.GammaCorrection(gray, 1.2F))
                using (Mat autoscaled = IntensityTransformCv2.Autoscaling(gray))
                using (Mat stretched = IntensityTransformCv2.ContrastStretching(gray, 16, 0, 192, 255))
                {
                    AssertOutputShape(log, gray);
                    AssertOutputShape(gamma, gray);
                    AssertOutputShape(autoscaled, gray);
                    AssertOutputShape(stretched, gray);
                    using (Mat? bimef = TryBimef(bgr))
                    using (Mat? bimefWithK = TryBimefWithK(bgr))
                    {
                        if (bimef != null)
                        {
                            AssertOutputShape(bimef, bgr);
                        }

                        if (bimefWithK != null)
                        {
                            AssertOutputShape(bimefWithK, bgr);
                        }
                    }
                }
            }
            catch (OpenCvException ex) when (IsIntensityTransformModuleMissing(ex))
            {
                Assert.True(IsIntensityTransformModuleMissing(ex), ex.Message);
            }
        }

        private static Mat CreateGrayImage()
        {
            var mat = new Mat(8, 8, MatType.CV_8UC1);
            var values = new byte[64];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (byte)(16 + i * 3);
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static Mat CreateBgrImage()
        {
            var mat = new Mat(8, 8, MatType.CV_8UC3);
            var values = new byte[8 * 8 * 3];
            for (int i = 0; i < values.Length; i += 3)
            {
                int pixel = i / 3;
                values[i] = (byte)(16 + pixel % 31);
                values[i + 1] = (byte)(32 + pixel % 47);
                values[i + 2] = (byte)(64 + pixel % 63);
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static Mat? TryBimef(Mat source)
        {
            try
            {
                return IntensityTransformCv2.Bimef(source);
            }
            catch (OpenCvException ex) when (IsBimefRequiresEigen(ex))
            {
                return null;
            }
        }

        private static Mat? TryBimefWithK(Mat source)
        {
            try
            {
                return IntensityTransformCv2.Bimef(source, 0.8F, 0.5F, -0.3293F, 1.1258F);
            }
            catch (OpenCvException ex) when (IsBimefRequiresEigen(ex))
            {
                return null;
            }
        }

        private static void AssertOutputShape(Mat output, Mat source)
        {
            Assert.False(output.Empty);
            Assert.Equal(source.Rows, output.Rows);
            Assert.Equal(source.Cols, output.Cols);
        }

        private static bool IsIntensityTransformModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool IsBimefRequiresEigen(OpenCvException exception)
        {
            return exception.Message.IndexOf("BIMEF", StringComparison.OrdinalIgnoreCase) >= 0 &&
                exception.Message.IndexOf("Eigen", StringComparison.OrdinalIgnoreCase) >= 0;
        }

    }
}
