using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Quality;

namespace JYPPX.OpenCvSharp.Tests.Quality
{
    public sealed class QualityTests
    {
        [Fact]
        public void StaticQualityMethodsValidateManagedArguments()
        {
            Assert.Throws<ArgumentNullException>(() => QualityMSE.Compute(null!, null!));
            Assert.Throws<ArgumentNullException>(() => QualityPSNR.Compute(null!, null!));
            Assert.Throws<ArgumentNullException>(() => QualitySSIM.Compute(null!, null!));
            Assert.Throws<ArgumentNullException>(() => QualityGMSD.Compute(null!, null!));
            Assert.Throws<ArgumentNullException>(() => QualityBRISQUE.Compute(null!, "model.yml", "range.yml"));
            Assert.Throws<ArgumentNullException>(() => QualityBRISQUE.ComputeFeatures(null!, null!));
            Assert.Throws<ArgumentNullException>(() => QualityBRISQUE.ComputeFeatures(null!));

            using (var image = new Mat())
            using (var reference = new Mat(4, 4, MatType.CV_8UC1, new Scalar(10)))
            using (var twoChannel = new Mat(4, 4, MatType.CV_8UC2, new Scalar(127, 128, 0, 0)))
            using (var features = new Mat())
            {
                Assert.Throws<ArgumentException>(() => new QualityGMSD(image));
                Assert.Throws<ArgumentException>(() => QualityGMSD.Create(image));
                Assert.Throws<ArgumentException>(() => QualityGMSD.Compute(image, reference));
                Assert.Throws<ArgumentException>(() => QualityGMSD.Compute(reference, image));
                Assert.Throws<ArgumentNullException>(() => new QualityBRISQUE(null!, "range.yml"));
                Assert.Throws<ArgumentNullException>(() => new QualityBRISQUE("model.yml", null!));
                Assert.Throws<ArgumentNullException>(() => QualityBRISQUE.Create(null!, "range.yml"));
                Assert.Throws<ArgumentNullException>(() => QualityBRISQUE.Create("model.yml", null!));
                Assert.Throws<ArgumentNullException>(() => QualityBRISQUE.Compute(image, null!, "range.yml"));
                Assert.Throws<ArgumentNullException>(() => QualityBRISQUE.Compute(image, "model.yml", null!));
                Assert.Throws<ArgumentException>(() => QualityBRISQUE.Compute(image, "model.yml", "range.yml"));
                Assert.Throws<ArgumentException>(() => QualityBRISQUE.Compute(twoChannel, "model.yml", "range.yml"));
                Assert.Throws<ArgumentNullException>(() => QualityBRISQUE.ComputeFeatures(image, null!));
                Assert.Throws<ArgumentException>(() => QualityBRISQUE.ComputeFeatures(image, features));
                Assert.Throws<ArgumentException>(() => QualityBRISQUE.ComputeFeatures(image));
                Assert.Throws<ArgumentException>(() => QualityBRISQUE.ComputeFeatures(twoChannel, features));
                Assert.Throws<ArgumentException>(() => QualityBRISQUE.ComputeFeatures(twoChannel));
                Assert.Throws<ArgumentException>(() => new QualityBRISQUE("model\0.yml", "range.yml"));
                Assert.Throws<ArgumentException>(() => new QualityBRISQUE("model.yml", "range\0.yml"));
                Assert.Throws<ArgumentException>(() => QualityBRISQUE.Create("model\0.yml", "range.yml"));
                Assert.Throws<ArgumentException>(() => QualityBRISQUE.Create("model.yml", "range\0.yml"));
                Assert.Throws<ArgumentException>(() => QualityBRISQUE.Compute(image, "model\0.yml", "range.yml"));
                Assert.Throws<ArgumentException>(() => QualityBRISQUE.Compute(image, "model.yml", "range\0.yml"));
            }
        }

        [Fact]
        public void QualityPsnrDefaultMatchesOpenCvConstant()
        {
            Assert.Equal(255.0, QualityPSNR.MaxPixelValueDefault);
        }

        [Fact]
        public void FullReferenceMetricsSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var reference = new Mat(4, 4, MatType.CV_8UC1, new Scalar(10)))
            using (var comparison = new Mat(4, 4, MatType.CV_8UC1, new Scalar(12)))
            using (var qualityMap = new Mat())
            using (var empty = new Mat())
            using (var mse = QualityMSE.Create(reference))
            using (var psnr = QualityPSNR.Create(reference, 255.0))
            using (var ssim = QualitySSIM.Create(reference))
            using (var gmsd = QualityGMSD.Create(reference))
            {
                Assert.False(mse.IsDisposed);
                Assert.False(psnr.IsDisposed);
                Assert.False(ssim.IsDisposed);
                Assert.False(gmsd.IsDisposed);

                Scalar mseValue = mse.Compute(comparison);
                Scalar psnrValue = psnr.Compute(comparison);
                Scalar ssimValue = ssim.Compute(comparison);
                Scalar gmsdValue = gmsd.Compute(comparison);

                Assert.Throws<ArgumentException>(() => gmsd.Compute(empty));
                Assert.True(mseValue.V0 >= 0.0);
                Assert.True(psnrValue.V0 >= 0.0);
                Assert.True(ssimValue.V0 <= 1.0);
                Assert.True(gmsdValue.V0 >= 0.0);

                psnr.MaxPixelValue = 128.0;
                Assert.Equal(128.0, psnr.MaxPixelValue, 3);

                Assert.True(QualityMSE.Compute(reference, comparison, qualityMap).V0 >= 0.0);
                Assert.False(qualityMap.Empty);

                mse.Dispose();
                Assert.True(mse.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => mse.Empty);
                Assert.Throws<ObjectDisposedException>(() => mse.Compute(comparison));
                Assert.Throws<ObjectDisposedException>(() => mse.GetQualityMap(qualityMap));
                Assert.Throws<ObjectDisposedException>(() => mse.GetQualityMap());
                Assert.Throws<ObjectDisposedException>(() => mse.Clear());

                psnr.Dispose();
                Assert.True(psnr.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => psnr.MaxPixelValue);
                Assert.Throws<ObjectDisposedException>(() => psnr.MaxPixelValue = 255.0);
            }
        }

        [Fact]
        public void ReferenceMetricsQualityMapReturningOverloadRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var reference = new Mat(4, 4, MatType.CV_8UC1, new Scalar(10)))
            using (var comparison = new Mat(4, 4, MatType.CV_8UC1, new Scalar(12)))
            using (var mse = QualityMSE.Create(reference))
            using (var psnr = QualityPSNR.Create(reference))
            using (var ssim = QualitySSIM.Create(reference))
            using (var gmsd = QualityGMSD.Create(reference))
            {
                AssertQualityMapMatchesReference(mse, comparison, reference);
                AssertQualityMapMatchesReference(psnr, comparison, reference);
                AssertQualityMapMatchesReference(ssim, comparison, reference);
                AssertQualityMapMatchesReference(gmsd, comparison, reference);
            }
        }

        [Fact]
        public void BrisqueSmokeRunsOnlyWhenModelEnvironmentIsSet()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            string? model = TestEnvironment.GetBrisqueModelVariable();
            string? range = TestEnvironment.GetBrisqueRangeVariable();
            if (string.IsNullOrWhiteSpace(model) || string.IsNullOrWhiteSpace(range))
            {
                return;
            }

            using (var image = new Mat(32, 32, MatType.CV_8UC1, new Scalar(127)))
            using (var brisque = QualityBRISQUE.Create(model, range))
            {
                Scalar score = brisque.Compute(image);
                using (Mat features = QualityBRISQUE.ComputeFeatures(image))
                {
                    Assert.True(score.V0 >= 0.0);
                    Assert.False(features.Empty);
                }
            }
        }

        private static void AssertQualityMapMatchesReference(QualityBase quality, Mat comparison, Mat reference)
        {
            Scalar score = quality.Compute(comparison);
            using (Mat qualityMap = quality.GetQualityMap())
            {
                Assert.True(score.V0 >= 0.0);
                Assert.False(qualityMap.Empty);
                Assert.True(qualityMap.Rows > 0);
                Assert.True(qualityMap.Cols > 0);
            }
        }

    }
}
