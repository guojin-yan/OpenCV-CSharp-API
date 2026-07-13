using System;
using OpenCvSharp.Core;
using OpenCvSharp.Shape;

namespace OpenCvSharp.Tests.Shape
{
    public sealed class ShapeTests
    {
        [Fact]
        public void StaticFunctionsValidateManagedArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ShapeCv2.CreateHausdorffDistanceExtractor(rankProportion: 0.0F));
            Assert.Throws<ArgumentOutOfRangeException>(() => HausdorffDistanceExtractor.Create(rankProportion: float.NaN));
            Assert.Throws<ArgumentOutOfRangeException>(() => ShapeCv2.CreateShapeContextDistanceExtractor(angularBins: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ShapeCv2.CreateShapeContextDistanceExtractor(radialBins: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ShapeCv2.CreateShapeContextDistanceExtractor(innerRadius: 0.0F));
            Assert.Throws<ArgumentOutOfRangeException>(() => ShapeCv2.CreateShapeContextDistanceExtractor(outerRadius: 0.0F));
            Assert.Throws<ArgumentOutOfRangeException>(() => ShapeCv2.CreateShapeContextDistanceExtractor(iterations: 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => ShapeContextDistanceExtractor.Create(innerRadius: float.NaN));

            using (HausdorffDistanceExtractor? nativeBoundary = TryCreateHausdorff())
            {
                if (nativeBoundary == null)
                {
                    return;
                }
            }

            using (var empty = new Mat())
            using (Mat signature = CreateSignature(0.2F, 0.3F, 0.5F))
            using (var rowMismatch = new Mat(2, 1, MatType.CV_32FC1, new Scalar(0.5)))
            using (var columnMismatch = new Mat(3, 2, MatType.CV_32FC1, new Scalar(0.5)))
            {
                Assert.Throws<ArgumentNullException>(() => ShapeCv2.EMDL1(null!, signature));
                Assert.Throws<ArgumentNullException>(() => ShapeCv2.EMDL1(signature, null!));
                Assert.Throws<ArgumentException>(() => ShapeCv2.EMDL1(empty, signature));
                Assert.Throws<ArgumentException>(() => ShapeCv2.EMDL1(signature, empty));
                Assert.Throws<ArgumentException>(() => ShapeCv2.EMDL1(signature, rowMismatch));
                Assert.Throws<ArgumentException>(() => ShapeCv2.EMDL1(signature, columnMismatch));
            }
        }

        [Fact]
        public void EMDL1SmokeOrBoundaryRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat first = CreateSignature(0.2F, 0.3F, 0.5F))
            using (Mat second = CreateSignature(0.1F, 0.4F, 0.5F))
            {
                try
                {
                    float distance = ShapeCv2.EMDL1(first, second);
                    Assert.True(distance >= 0.0F);
                }
                catch (OpenCvException ex) when (IsShapeModuleMissing(ex))
                {
                    AssertBoundary(ex);
                }
            }
        }

        [Fact]
        public void HistogramCostExtractorSmokeOrBoundaryRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (NormHistogramCostExtractor norm = ShapeCv2.CreateNormHistogramCostExtractor(NormTypes.L2, 2, 0.25F))
                using (EMDHistogramCostExtractor emd = ShapeCv2.CreateEMDHistogramCostExtractor(NormTypes.L2, 2, 0.25F))
                using (ChiHistogramCostExtractor chi = ShapeCv2.CreateChiHistogramCostExtractor(2, 0.25F))
                using (EMDL1HistogramCostExtractor emdL1 = ShapeCv2.CreateEMDL1HistogramCostExtractor(2, 0.25F))
                using (Mat descriptors1 = CreateDescriptors())
                using (Mat descriptors2 = CreateDescriptors())
                using (Mat cost = norm.BuildCostMatrix(descriptors1, descriptors2))
                {
                    norm.NDummies = 3;
                    norm.DefaultCost = 0.35F;
                    norm.NormFlag = NormTypes.L1;
                    emd.NDummies = 4;
                    emd.DefaultCost = 0.45F;
                    emd.NormFlag = NormTypes.L1;
                    chi.NDummies = 5;
                    chi.DefaultCost = 0.55F;
                    emdL1.NDummies = 6;
                    emdL1.DefaultCost = 0.65F;

                    Assert.False(cost.Empty);
                    Assert.Equal(NormTypes.L1, norm.NormFlag);
                    Assert.Equal(3, norm.NDummies);
                    Assert.Equal(0.35F, norm.DefaultCost, 3);
                    Assert.Equal(NormTypes.L1, emd.NormFlag);
                    Assert.Equal(4, emd.NDummies);
                    Assert.Equal(0.45F, emd.DefaultCost, 3);
                    Assert.Equal(5, chi.NDummies);
                    Assert.Equal(0.55F, chi.DefaultCost, 3);
                    Assert.Equal(6, emdL1.NDummies);
                    Assert.Equal(0.65F, emdL1.DefaultCost, 3);

                    norm.Dispose();
                    Assert.True(norm.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => norm.NDummies);
                    Assert.Throws<ObjectDisposedException>(() => norm.BuildCostMatrix(descriptors1, descriptors2, descriptors1));
                    Assert.Throws<ObjectDisposedException>(() => norm.BuildCostMatrix(descriptors1, descriptors2));
                }
            }
            catch (OpenCvException ex) when (IsShapeModuleMissing(ex))
            {
                AssertBoundary(ex);
            }
        }

        [Fact]
        public void HistogramCostExtractorArgumentValidationRunsWhenNativeObjectIsAvailable()
        {
            using (NormHistogramCostExtractor? extractor = TryCreate(() => ShapeCv2.CreateNormHistogramCostExtractor()))
            {
                if (extractor == null)
                {
                    return;
                }

                using (Mat descriptors = CreateDescriptors())
                using (var cost = new Mat())
                {
                    Assert.Throws<ArgumentNullException>(() => extractor.BuildCostMatrix(null!, descriptors, cost));
                    Assert.Throws<ArgumentNullException>(() => extractor.BuildCostMatrix(descriptors, null!, cost));
                    Assert.Throws<ArgumentNullException>(() => extractor.BuildCostMatrix(descriptors, descriptors, null!));
                    Assert.Throws<ArgumentNullException>(() => extractor.BuildCostMatrix(null!, descriptors));
                    Assert.Throws<ArgumentNullException>(() => extractor.BuildCostMatrix(descriptors, null!));
                }
            }
        }

        [Fact]
        public void HistogramCostExtractorDisposedStateThrowsWhenNativeObjectIsAvailable()
        {
            NormHistogramCostExtractor? extractor = TryCreate(() => ShapeCv2.CreateNormHistogramCostExtractor());
            if (extractor == null)
            {
                return;
            }

            extractor.Dispose();
            Assert.True(extractor.IsDisposed);
            using (Mat descriptors = CreateDescriptors())
            using (var cost = new Mat())
            {
                Assert.Throws<ObjectDisposedException>(() => extractor.NDummies);
                Assert.Throws<ObjectDisposedException>(() => extractor.NDummies = 1);
                Assert.Throws<ObjectDisposedException>(() => extractor.DefaultCost);
                Assert.Throws<ObjectDisposedException>(() => extractor.DefaultCost = 0.1F);
                Assert.Throws<ObjectDisposedException>(() => extractor.NormFlag);
                Assert.Throws<ObjectDisposedException>(() => extractor.NormFlag = NormTypes.L1);
                Assert.Throws<ObjectDisposedException>(() => extractor.BuildCostMatrix(descriptors, descriptors, cost));
                Assert.Throws<ObjectDisposedException>(() => extractor.BuildCostMatrix(descriptors, descriptors));
            }
        }

        [Fact]
        public void ShapeContextDistanceExtractorDisposedStateThrowsWhenNativeObjectIsAvailable()
        {
            ShapeContextDistanceExtractor? extractor = TryCreate(() => ShapeCv2.CreateShapeContextDistanceExtractor());
            if (extractor == null)
            {
                return;
            }

            extractor.Dispose();
            Assert.True(extractor.IsDisposed);
            using (Mat contour = CreateContour(0.0F))
            {
                Assert.Throws<ObjectDisposedException>(() => extractor.ComputeDistance(contour, contour));
            }
        }

        [Fact]
        public void DistanceExtractorArgumentValidationRunsWhenNativeObjectIsAvailable()
        {
            using (HausdorffDistanceExtractor? extractor = TryCreateHausdorff())
            {
                if (extractor == null)
                {
                    return;
                }

                using (Mat contour = CreateContour(0.0F))
                using (var singleChannel = new Mat(4, 1, MatType.CV_32FC1, new Scalar(1.0)))
                using (var noColumns = new Mat(0, 0, MatType.CV_32FC2))
                {
                    Assert.Throws<ArgumentNullException>(() => extractor.ComputeDistance(null!, contour));
                    Assert.Throws<ArgumentNullException>(() => extractor.ComputeDistance(contour, null!));
                    Assert.Throws<ArgumentException>(() => extractor.ComputeDistance(singleChannel, contour));
                    Assert.Throws<ArgumentException>(() => extractor.ComputeDistance(contour, singleChannel));
                    Assert.Throws<ArgumentException>(() => extractor.ComputeDistance(noColumns, contour));
                    Assert.Throws<ArgumentException>(() => extractor.ComputeDistance(contour, noColumns));
                    Assert.Throws<ArgumentOutOfRangeException>(() => extractor.RankProportion = 0.0F);
                    Assert.Throws<ArgumentOutOfRangeException>(() => extractor.RankProportion = 1.1F);
                    Assert.Throws<ArgumentOutOfRangeException>(() => extractor.RankProportion = float.NaN);
                }
            }
        }

        [Fact]
        public void DistanceExtractorDisposedStateThrowsWhenNativeObjectIsAvailable()
        {
            HausdorffDistanceExtractor? extractor = TryCreateHausdorff();
            if (extractor == null)
            {
                return;
            }

            extractor.Dispose();
            Assert.True(extractor.IsDisposed);
            using (Mat contour = CreateContour(0.0F))
            {
                Assert.Throws<ObjectDisposedException>(() => extractor.ComputeDistance(contour, contour));
                Assert.Throws<ObjectDisposedException>(() => extractor.DistanceFlag);
                Assert.Throws<ObjectDisposedException>(() => extractor.DistanceFlag = NormTypes.L1);
                Assert.Throws<ObjectDisposedException>(() => extractor.RankProportion);
                Assert.Throws<ObjectDisposedException>(() => extractor.RankProportion = 0.5F);
            }
        }

        [Fact]
        public void DistanceExtractorSmokeOrBoundaryRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (HausdorffDistanceExtractor hausdorff = ShapeCv2.CreateHausdorffDistanceExtractor(NormTypes.L2, 0.6F))
                using (ShapeContextDistanceExtractor shapeContext = ShapeCv2.CreateShapeContextDistanceExtractor())
                using (Mat contour1 = CreateContour(0.0F))
                using (Mat contour2 = CreateContour(0.2F))
                {
                    hausdorff.DistanceFlag = NormTypes.L1;
                    hausdorff.RankProportion = 0.7F;

                    float hausdorffDistance = hausdorff.ComputeDistance(contour1, contour2);
                    float contextDistance = shapeContext.ComputeDistance(contour1, contour2);

                    Assert.True(hausdorffDistance >= 0.0F);
                    Assert.True(contextDistance >= 0.0F);
                    Assert.Equal(NormTypes.L1, hausdorff.DistanceFlag);
                    Assert.Equal(0.7F, hausdorff.RankProportion, 3);
                }
            }
            catch (OpenCvException ex) when (IsShapeModuleMissing(ex))
            {
                AssertBoundary(ex);
            }
        }

        private static Mat CreateSignature(params float[] values)
        {
            var mat = new Mat(values.Length, 1, MatType.CV_32FC1);
            mat.CopyFrom(values);
            return mat;
        }

        private static Mat CreateDescriptors()
        {
            var mat = new Mat(3, 2, MatType.CV_32FC1);
            mat.CopyFrom(new float[]
            {
                0.0F, 1.0F,
                1.0F, 0.0F,
                0.5F, 0.5F
            });
            return mat;
        }

        private static Mat CreateContour(float offset)
        {
            var mat = new Mat(4, 1, MatType.CV_32FC2);
            mat.CopyFrom(new float[]
            {
                0.0F + offset, 0.0F,
                1.0F + offset, 0.0F,
                1.0F + offset, 1.0F,
                0.0F + offset, 1.0F
            });
            return mat;
        }

        private static HausdorffDistanceExtractor? TryCreateHausdorff()
        {
            return TryCreate(() => ShapeCv2.CreateHausdorffDistanceExtractor());
        }

        private static T? TryCreate<T>(Func<T> factory)
            where T : class, IDisposable
        {
            try
            {
                return factory();
            }
            catch (OpenCvException ex) when (IsShapeModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static bool IsShapeModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("shape", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static void AssertBoundary(OpenCvException exception)
        {
            Assert.True(IsShapeModuleMissing(exception), exception.Message);
        }

    }
}
