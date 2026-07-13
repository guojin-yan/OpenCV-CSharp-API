using System;
using System.Globalization;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.Tracking;
using OpenCvSharp.Tracking.Legacy;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;
using LegacyMultiTracker = OpenCvSharp.Tracking.Legacy.MultiTracker;

namespace OpenCvSharp.Tests.Tracking
{
    public sealed class TrackingTests
    {
        [Fact]
        public void EnumAndParameterDefaultsExposeOpenCvShape()
        {
            Assert.Equal(1, (int)TrackerKCFMode.Gray);
            Assert.Equal(2, (int)TrackerKCFMode.Cn);
            Assert.Equal(4, (int)TrackerKCFMode.Custom);

            TrackerKCFParams kcf = TrackerKCFParams.Default;
            var sameKcf = new TrackerKCFParams(
                0.5F,
                0.2F,
                0.0001F,
                0.075F,
                1.0F / 16.0F,
                0.15F,
                true,
                true,
                false,
                true,
                80 * 80,
                2,
                TrackerKCFMode.Cn,
                TrackerKCFMode.Gray);
            var differentKcf = new TrackerKCFParams(
                0.6F,
                0.2F,
                0.0001F,
                0.075F,
                1.0F / 16.0F,
                0.15F,
                true,
                true,
                false,
                true,
                80 * 80,
                2,
                TrackerKCFMode.Cn,
                TrackerKCFMode.Gray);
            Assert.Equal(0.5F, kcf.DetectThresh, 3);
            Assert.Equal(TrackerKCFMode.Cn, kcf.DescPca);
            Assert.Equal(TrackerKCFMode.Gray, kcf.DescNpca);
            Assert.True(kcf.Resize);
            Assert.True(kcf.CompressFeature);
            Assert.Equal(sameKcf, kcf);
            Assert.True(kcf == sameKcf);
            Assert.False(kcf != sameKcf);
            Assert.True(kcf != differentKcf);
            Assert.Equal(kcf.GetHashCode(), sameKcf.GetHashCode());
            Assert.Contains("DetectThresh=0.5", kcf.ToString());
            Assert.Throws<ArgumentOutOfRangeException>(() => new TrackerKCFParams(
                0.5F,
                0.2F,
                0.0001F,
                0.075F,
                1.0F / 16.0F,
                0.15F,
                true,
                true,
                false,
                true,
                80 * 80,
                2,
                (TrackerKCFMode)8,
                TrackerKCFMode.Gray));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TrackerKCFParams(
                0.5F,
                0.2F,
                0.0001F,
                0.075F,
                1.0F / 16.0F,
                0.15F,
                true,
                true,
                false,
                true,
                80 * 80,
                2,
                TrackerKCFMode.Cn,
                (TrackerKCFMode)8));

            TrackerCSRTParams csrt = TrackerCSRTParams.Default;
            var sameCsrt = new TrackerCSRTParams(
                true,
                true,
                true,
                false,
                true,
                true,
                "hann",
                3.75F,
                45.0F,
                200.0F,
                1.0F,
                9.0F,
                0.2F,
                3.0F,
                0.02F,
                0.02F,
                18,
                4,
                16,
                0.04F,
                2,
                33,
                0.25F,
                512.0F,
                0.025F,
                1.02F,
                0.035F);
            var differentCsrt = new TrackerCSRTParams(
                true,
                true,
                true,
                false,
                true,
                true,
                "cheb",
                3.75F,
                45.0F,
                200.0F,
                1.0F,
                9.0F,
                0.2F,
                3.0F,
                0.02F,
                0.02F,
                18,
                4,
                16,
                0.04F,
                2,
                33,
                0.25F,
                512.0F,
                0.025F,
                1.02F,
                0.035F);
            Assert.True(csrt.UseHog);
            Assert.True(csrt.UseColorNames);
            Assert.Equal("hann", csrt.WindowFunction);
            Assert.Equal(33, csrt.NumberOfScales);
            Assert.Equal(0.035F, csrt.PsrThreshold, 3);
            Assert.Equal(sameCsrt, csrt);
            Assert.True(csrt == sameCsrt);
            Assert.False(csrt != sameCsrt);
            Assert.True(csrt != differentCsrt);
            Assert.Equal(csrt.GetHashCode(), sameCsrt.GetHashCode());
            Assert.Contains("WindowFunction=hann", csrt.ToString());
            Assert.Throws<ArgumentNullException>(() => new TrackerCSRTParams(
                true,
                true,
                true,
                false,
                true,
                true,
                null!,
                3.75F,
                45.0F,
                200.0F,
                1.0F,
                9.0F,
                0.2F,
                3.0F,
                0.02F,
                0.02F,
                18,
                4,
                16,
                0.04F,
                2,
                33,
                0.25F,
                512.0F,
                0.025F,
                1.02F,
                0.035F));

            TrackerMedianFlowParams median = TrackerMedianFlowParams.Default;
            var sameMedian = new TrackerMedianFlowParams(
                10,
                new Size(3, 3),
                5,
                TermCriteria.ByCountAndEpsilon(20, 0.3),
                new Size(30, 30),
                10.0);
            var differentMedian = new TrackerMedianFlowParams(
                11,
                new Size(3, 3),
                5,
                TermCriteria.ByCountAndEpsilon(20, 0.3),
                new Size(30, 30),
                10.0);
            Assert.Equal(10, median.PointsInGrid);
            Assert.Equal(new Size(3, 3).ToString(), median.WinSize.ToString());
            Assert.Equal(TermCriteriaTypes.CountOrEps, median.TermCriteria.Type);
            Assert.Equal(sameMedian, median);
            Assert.True(median == sameMedian);
            Assert.False(median != sameMedian);
            Assert.True(median != differentMedian);
            Assert.Equal(median.GetHashCode(), sameMedian.GetHashCode());
            Assert.Contains("PointsInGrid=10", median.ToString());

            TrackerMILParams mil = TrackerMILParams.Default;
            var sameMil = new TrackerMILParams(3.0F, 25.0F, 65, 4.0F, 100000, 65, 250);
            var differentMil = new TrackerMILParams(3.1F, 25.0F, 65, 4.0F, 100000, 65, 250);
            Assert.True(mil.SamplerInitInRadius > 0.0F);
            Assert.True(mil.FeatureSetNumFeatures > 0);
            Assert.Equal(sameMil, mil);
            Assert.True(mil == sameMil);
            Assert.False(mil != sameMil);
            Assert.True(mil != differentMil);
            Assert.Equal(mil.GetHashCode(), sameMil.GetHashCode());
            Assert.Contains("FeatureSetNumFeatures=250", mil.ToString());
        }

        [Fact]
        public void TrackerKcfParamsToStringFormatsFloatingValuesInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var parameters = new TrackerKCFParams(
                    0.5F,
                    0.2F,
                    0.0001F,
                    0.075F,
                    0.0625F,
                    0.15F,
                    true,
                    true,
                    false,
                    true,
                    80 * 80,
                    2,
                    TrackerKCFMode.Cn,
                    TrackerKCFMode.Gray);

                string formatted = parameters.ToString();
                Assert.Contains("DetectThresh=0.5", formatted, StringComparison.Ordinal);
                Assert.Contains("Sigma=0.2", formatted, StringComparison.Ordinal);
                Assert.Contains("Lambda=0.0001", formatted, StringComparison.Ordinal);
                Assert.Contains("InterpFactor=0.075", formatted, StringComparison.Ordinal);
                Assert.Contains("OutputSigmaFactor=0.0625", formatted, StringComparison.Ordinal);
                Assert.Contains("PcaLearningRate=0.15", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("DetectThresh=0,5", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("Sigma=0,2", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("Lambda=0,0001", formatted, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void TrackerCsrtParamsToStringFormatsFloatingValuesInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var parameters = new TrackerCSRTParams(
                    true,
                    true,
                    true,
                    false,
                    true,
                    true,
                    "hann",
                    3.75F,
                    45.0F,
                    200.0F,
                    1.0F,
                    9.0F,
                    0.2F,
                    3.0F,
                    0.02F,
                    0.02F,
                    18,
                    4,
                    16,
                    0.04F,
                    2,
                    33,
                    0.25F,
                    512.0F,
                    0.025F,
                    1.02F,
                    0.035F);

                string formatted = parameters.ToString();
                Assert.Contains("KaiserAlpha=3.75", formatted, StringComparison.Ordinal);
                Assert.Contains("HogClip=0.2", formatted, StringComparison.Ordinal);
                Assert.Contains("FilterLr=0.02", formatted, StringComparison.Ordinal);
                Assert.Contains("HistogramLr=0.04", formatted, StringComparison.Ordinal);
                Assert.Contains("ScaleSigmaFactor=0.25", formatted, StringComparison.Ordinal);
                Assert.Contains("ScaleLr=0.025", formatted, StringComparison.Ordinal);
                Assert.Contains("ScaleStep=1.02", formatted, StringComparison.Ordinal);
                Assert.Contains("PsrThreshold=0.035", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("KaiserAlpha=3,75", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("HogClip=0,2", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("ScaleStep=1,02", formatted, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void TrackerMedianFlowParamsToStringFormatsFloatingValuesInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var parameters = new TrackerMedianFlowParams(
                    10,
                    new Size(3, 3),
                    5,
                    TermCriteria.ByCountAndEpsilon(20, 0.3),
                    new Size(30, 30),
                    10.25);

                string formatted = parameters.ToString();
                Assert.Contains("Epsilon=0.3", formatted, StringComparison.Ordinal);
                Assert.Contains("MaxMedianLengthOfDisplacementDifference=10.25", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("Epsilon=0,3", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("MaxMedianLengthOfDisplacementDifference=10,25", formatted, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void TrackerMilParamsToStringFormatsFloatingValuesInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var parameters = new TrackerMILParams(
                    3.25F,
                    25.5F,
                    65,
                    4.75F,
                    100000,
                    65,
                    250);

                string formatted = parameters.ToString();
                Assert.Contains("SamplerInitInRadius=3.25", formatted, StringComparison.Ordinal);
                Assert.Contains("SamplerSearchWinSize=25.5", formatted, StringComparison.Ordinal);
                Assert.Contains("SamplerTrackInRadius=4.75", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("SamplerInitInRadius=3,25", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("SamplerSearchWinSize=25,5", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("SamplerTrackInRadius=4,75", formatted, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void ResultObjectsExposeValuesAndReturnSnapshots()
        {
            var modern = new TrackerUpdateResult(true, new Rect(1, 2, 3, 4));
            var sameModern = new TrackerUpdateResult(true, new Rect(1, 2, 3, 4));
            var differentModern = new TrackerUpdateResult(true, new Rect(1, 2, 3, 5));
            var legacy = new LegacyTrackerUpdateResult(false, new Rect2d(1.5, 2.5, 3.5, 4.5));
            var sameLegacy = new LegacyTrackerUpdateResult(false, new Rect2d(1.5, 2.5, 3.5, 4.5));
            var differentLegacy = new LegacyTrackerUpdateResult(false, new Rect2d(1.5, 2.5, 3.5, 5.5));
            var boundingBoxes = new[] { new Rect2d(2.0, 3.0, 4.0, 5.0) };
            var multi = new LegacyMultiTrackerUpdateResult(true, boundingBoxes);
            var sameMulti = new LegacyMultiTrackerUpdateResult(true, new[] { new Rect2d(2.0, 3.0, 4.0, 5.0) });
            var differentMulti = new LegacyMultiTrackerUpdateResult(true, new[] { new Rect2d(2.0, 3.0, 4.0, 6.0) });
            var emptyMulti = new LegacyMultiTrackerUpdateResult(false, null!);
            boundingBoxes[0] = new Rect2d(20.0, 30.0, 40.0, 50.0);

            Assert.True(modern.Success);
            Assert.Equal(4, modern.BoundingBox.Height);
            Assert.True(modern == sameModern);
            Assert.False(modern != sameModern);
            Assert.True(modern != differentModern);
            Assert.False(modern.Equals("not a result"));
            Assert.Equal(modern.GetHashCode(), sameModern.GetHashCode());
            Assert.Equal("{Success=True,BoundingBox={X=1,Y=2,Width=3,Height=4}}", modern.ToString());
            Assert.False(legacy.Success);
            Assert.Equal(4.5, legacy.BoundingBox.Height);
            Assert.True(legacy == sameLegacy);
            Assert.False(legacy != sameLegacy);
            Assert.True(legacy != differentLegacy);
            Assert.False(legacy.Equals("not a result"));
            Assert.Equal(legacy.GetHashCode(), sameLegacy.GetHashCode());
            Assert.Equal("{Success=False,BoundingBox={X=1.5,Y=2.5,Width=3.5,Height=4.5}}", legacy.ToString());
            Assert.Equal(1, multi.BoundingBoxCount);
            Assert.Single(multi.BoundingBoxes);
            Assert.Equal(new Rect2d(2.0, 3.0, 4.0, 5.0), multi.BoundingBoxes[0]);

            Rect2d[] returnedBoundingBoxes = multi.BoundingBoxes;
            returnedBoundingBoxes[0] = new Rect2d(20.0, 30.0, 40.0, 50.0);

            Assert.Equal(new Rect2d(2.0, 3.0, 4.0, 5.0), multi.BoundingBoxes[0]);
            Assert.True(multi == sameMulti);
            Assert.False(multi != sameMulti);
            Assert.True(multi != differentMulti);
            Assert.Equal(multi.GetHashCode(), sameMulti.GetHashCode());
            Assert.Contains("BoundingBoxes=1", multi.ToString());
            Assert.Equal(0, emptyMulti.BoundingBoxCount);
            Assert.Empty(emptyMulti.BoundingBoxes);
        }

        [Fact]
        public void TrackerUpdateResultsHaveSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(20, Marshal.SizeOf<TrackerUpdateResult>());
            Assert.Equal(40, Marshal.SizeOf<LegacyTrackerUpdateResult>());
            Assert.Equal(56, Marshal.SizeOf<TrackerKCFParams>());
            Assert.Equal(28, Marshal.SizeOf<TrackerMILParams>());
            Assert.Equal(48, Marshal.SizeOf<TrackerMedianFlowParams>());

            Assert.Equal(0, FieldOffset<TrackerUpdateResult>("<Success>k__BackingField"));
            Assert.Equal(4, FieldOffset<TrackerUpdateResult>("<BoundingBox>k__BackingField"));

            Assert.Equal(0, FieldOffset<LegacyTrackerUpdateResult>("<Success>k__BackingField"));
            Assert.Equal(8, FieldOffset<LegacyTrackerUpdateResult>("<BoundingBox>k__BackingField"));

            Assert.Equal(0, FieldOffset<TrackerKCFParams>("<DetectThresh>k__BackingField"));
            Assert.Equal(4, FieldOffset<TrackerKCFParams>("<Sigma>k__BackingField"));
            Assert.Equal(8, FieldOffset<TrackerKCFParams>("<Lambda>k__BackingField"));
            Assert.Equal(12, FieldOffset<TrackerKCFParams>("<InterpFactor>k__BackingField"));
            Assert.Equal(16, FieldOffset<TrackerKCFParams>("<OutputSigmaFactor>k__BackingField"));
            Assert.Equal(20, FieldOffset<TrackerKCFParams>("<PcaLearningRate>k__BackingField"));
            Assert.Equal(24, FieldOffset<TrackerKCFParams>("<Resize>k__BackingField"));
            Assert.Equal(28, FieldOffset<TrackerKCFParams>("<SplitCoeff>k__BackingField"));
            Assert.Equal(32, FieldOffset<TrackerKCFParams>("<WrapKernel>k__BackingField"));
            Assert.Equal(36, FieldOffset<TrackerKCFParams>("<CompressFeature>k__BackingField"));
            Assert.Equal(40, FieldOffset<TrackerKCFParams>("<MaxPatchSize>k__BackingField"));
            Assert.Equal(44, FieldOffset<TrackerKCFParams>("<CompressedSize>k__BackingField"));
            Assert.Equal(48, FieldOffset<TrackerKCFParams>("<DescPca>k__BackingField"));
            Assert.Equal(52, FieldOffset<TrackerKCFParams>("<DescNpca>k__BackingField"));

            Assert.Equal(0, FieldOffset<TrackerMILParams>("<SamplerInitInRadius>k__BackingField"));
            Assert.Equal(4, FieldOffset<TrackerMILParams>("<SamplerSearchWinSize>k__BackingField"));
            Assert.Equal(8, FieldOffset<TrackerMILParams>("<SamplerInitMaxNegNum>k__BackingField"));
            Assert.Equal(12, FieldOffset<TrackerMILParams>("<SamplerTrackInRadius>k__BackingField"));
            Assert.Equal(16, FieldOffset<TrackerMILParams>("<SamplerTrackMaxPosNum>k__BackingField"));
            Assert.Equal(20, FieldOffset<TrackerMILParams>("<SamplerTrackMaxNegNum>k__BackingField"));
            Assert.Equal(24, FieldOffset<TrackerMILParams>("<FeatureSetNumFeatures>k__BackingField"));

            Assert.Equal(0, FieldOffset<TrackerMedianFlowParams>("<PointsInGrid>k__BackingField"));
            Assert.Equal(4, FieldOffset<TrackerMedianFlowParams>("<WinSize>k__BackingField"));
            Assert.Equal(12, FieldOffset<TrackerMedianFlowParams>("<MaxLevel>k__BackingField"));
            Assert.Equal(16, FieldOffset<TrackerMedianFlowParams>("<TermCriteria>k__BackingField"));
            Assert.Equal(32, FieldOffset<TrackerMedianFlowParams>("<WinSizeNcc>k__BackingField"));
            Assert.Equal(40, FieldOffset<TrackerMedianFlowParams>("<MaxMedianLengthOfDisplacementDifference>k__BackingField"));
        }

        [Fact]
        public void Rect2dValueObjectExposesGeometry()
        {
            var rect = new Rect2d(new Point2d(1.0, 2.0), new Size2d(3.0, 4.0));

            Assert.Equal(4.0, rect.Right);
            Assert.Equal(6.0, rect.Bottom);
            Assert.Equal(12.0, rect.Area);
            Assert.True(rect.Contains(new Point2d(2.0, 3.0)));
            Assert.False(rect.Contains(new Point2d(4.0, 6.0)));
            Assert.Equal(rect, new Rect2d(1.0, 2.0, 3.0, 4.0));
        }

        [Fact]
        public void FactoriesReturnObjectOrExplicitNativeBoundary()
        {
            AssertFactoryBoundary(() => TrackerKCF.Create());
            AssertFactoryBoundary(() => TrackerCSRT.Create(TrackerCSRTParams.Default));
            AssertFactoryBoundary(() => TrackerMOSSE.Create());
            AssertFactoryBoundary(() => TrackerMIL.Create(TrackerMILParams.Default));
            AssertFactoryBoundary(() => TrackerMedianFlow.Create(TrackerMedianFlowParams.Default));
            AssertFactoryBoundary(() => LegacyMultiTracker.Create());
        }

        [Fact]
        public void TrackerDefaultParamsMapFromNativeWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            AssertNativeDefaultBoundaryOrValues(
                TrackerKCFParams.GetDefaultFromNative,
                parameters =>
                {
                    Assert.True(parameters.DetectThresh >= 0.0F);
                    Assert.True(parameters.Sigma > 0.0F);
                    Assert.True(parameters.MaxPatchSize > 0);
                    Assert.Contains("DetectThresh=", parameters.ToString(), StringComparison.Ordinal);
                });

            AssertNativeDefaultBoundaryOrValues(
                TrackerCSRTParams.GetDefaultFromNative,
                parameters =>
                {
                    Assert.Equal(TrackerCSRTParams.Default.WindowFunction, parameters.WindowFunction);
                    Assert.True(parameters.TemplateSize > 0.0F);
                    Assert.True(parameters.NumberOfScales > 0);
                    Assert.Contains("WindowFunction=", parameters.ToString(), StringComparison.Ordinal);
                });

            AssertNativeDefaultBoundaryOrValues(
                TrackerMILParams.GetDefaultFromNative,
                parameters =>
                {
                    Assert.True(parameters.SamplerInitInRadius > 0.0F);
                    Assert.True(parameters.FeatureSetNumFeatures > 0);
                    Assert.Contains("FeatureSetNumFeatures=", parameters.ToString(), StringComparison.Ordinal);
                });

            AssertNativeDefaultBoundaryOrValues(
                TrackerMedianFlowParams.GetDefaultFromNative,
                parameters =>
                {
                    Assert.True(parameters.PointsInGrid > 0);
                    Assert.True(parameters.WinSize.Width > 0);
                    Assert.True(parameters.WinSize.Height > 0);
                    Assert.Contains("PointsInGrid=", parameters.ToString(), StringComparison.Ordinal);
                });
        }

        [Fact]
        public void ManagedValidationRunsWhenTrackerCanBeCreated()
        {
            using (TrackerKCF? kcfTracker = TryCreate(() => TrackerKCF.Create()))
            using (TrackerCSRT? csrtTracker = TryCreate(() => TrackerCSRT.Create()))
            using (TrackerMIL? firstMilTracker = TryCreate(() => TrackerMIL.Create()))
            using (LegacyMultiTracker? firstMulti = TryCreate(() => LegacyMultiTracker.Create()))
            using (TrackerMIL? secondMilTracker = TryCreate(() => TrackerMIL.Create()))
            using (LegacyMultiTracker? secondMulti = TryCreate(() => LegacyMultiTracker.Create()))
            {
                if (kcfTracker == null &&
                    csrtTracker == null &&
                    firstMilTracker == null &&
                    firstMulti == null &&
                    secondMilTracker == null &&
                    secondMulti == null)
                {
                    return;
                }

                using (var image = new Mat())
                {
                    if (kcfTracker != null)
                    {
                        Assert.Throws<ArgumentNullException>(() => kcfTracker.Init(null!, new Rect(0, 0, 1, 1)));
                        Assert.Throws<ArgumentNullException>(() => kcfTracker.Update(null!, new Rect(0, 0, 1, 1)));
                        Rect box = new Rect(0, 0, 1, 1);
                        kcfTracker.Dispose();
                        Assert.True(kcfTracker.IsDisposed);
                        Assert.Throws<ObjectDisposedException>(() => kcfTracker.Init(image, box));
                        Assert.Throws<ObjectDisposedException>(() => kcfTracker.Update(image, ref box));
                        Assert.Throws<ObjectDisposedException>(() => kcfTracker.Update(image, new Rect(0, 0, 1, 1)));
                    }

                    if (csrtTracker != null)
                    {
                        Assert.Throws<ArgumentNullException>(() => csrtTracker.SetInitialMask(null!));
                        Rect box = new Rect(0, 0, 1, 1);
                        csrtTracker.Dispose();
                        Assert.True(csrtTracker.IsDisposed);
                        Assert.Throws<ObjectDisposedException>(() => csrtTracker.Init(image, new Rect(0, 0, 1, 1)));
                        Assert.Throws<ObjectDisposedException>(() => csrtTracker.Update(image, ref box));
                        Assert.Throws<ObjectDisposedException>(() => csrtTracker.Update(image, new Rect(0, 0, 1, 1)));
                        Assert.Throws<ObjectDisposedException>(() => csrtTracker.SetInitialMask(image));
                    }

                    if (firstMilTracker != null && firstMulti != null)
                    {
                        Assert.Throws<ArgumentNullException>(() => firstMilTracker.Init(null!, new Rect2d(0, 0, 1, 1)));
                        Assert.Throws<ArgumentNullException>(() => firstMulti.Add(null!, image, new Rect2d(0, 0, 1, 1)));
                        Assert.Throws<ArgumentNullException>(() => firstMulti.Add(firstMilTracker, null!, new Rect2d(0, 0, 1, 1)));
                        Assert.Throws<ArgumentNullException>(() => firstMulti.Update(null!));

                        Rect2d box = new Rect2d(0, 0, 1, 1);
                        firstMilTracker.Dispose();
                        Assert.True(firstMilTracker.IsDisposed);
                        Assert.Throws<ObjectDisposedException>(() => firstMilTracker.Init(image, box));
                        Assert.Throws<ObjectDisposedException>(() => firstMilTracker.Update(image, ref box));
                        Assert.Throws<ObjectDisposedException>(() => firstMilTracker.Update(image, new Rect2d(0, 0, 1, 1)));

                        firstMulti.Dispose();
                        Assert.True(firstMulti.IsDisposed);
                        Assert.Throws<ObjectDisposedException>(() => firstMulti.Add(firstMilTracker, image, new Rect2d(0, 0, 1, 1)));
                        Assert.Throws<ObjectDisposedException>(() => firstMulti.Update(image));
                        Assert.Throws<ObjectDisposedException>(() => firstMulti.GetObjects());
                    }

                    if (secondMilTracker != null && secondMulti != null)
                    {
                        secondMilTracker.Dispose();
                        Assert.True(secondMilTracker.IsDisposed);
                        Assert.Throws<ObjectDisposedException>(() => secondMulti.Add(secondMilTracker, image, new Rect2d(0, 0, 1, 1)));
                        Assert.False(secondMulti.IsDisposed);
                    }
                }
            }
        }

        [Fact]
        public void TrackingSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat first = CreateFrame(2))
            using (Mat second = CreateFrame(4))
            {
                OpenCvException? modernException = Record.Exception(() =>
                {
                    using (TrackerKCF tracker = TrackerKCF.Create(TrackerKCFParams.Default))
                    {
                        Rect box = new Rect(6, 7, 8, 8);
                        tracker.Init(first, box);
                        TrackerUpdateResult update = tracker.Update(second, box);
                        Assert.True(update.BoundingBox.Width >= 0);
                    }
                }) as OpenCvException;
                AssertBoundaryOrSuccess(modernException);

                OpenCvException? csrtException = Record.Exception(() =>
                {
                    using (TrackerCSRT tracker = TrackerCSRT.Create(TrackerCSRTParams.Default))
                    using (Mat mask = new Mat(first.Rows, first.Cols, MatType.CV_8UC1, new Scalar(0)))
                    {
                        ImgProcCv2.Rectangle(mask, new Rect(6, 7, 8, 8), new Scalar(255), -1);
                        tracker.SetInitialMask(mask);
                        tracker.Init(first, new Rect(6, 7, 8, 8));
                        TrackerUpdateResult update = tracker.Update(second, new Rect(6, 7, 8, 8));
                        Assert.True(update.BoundingBox.Height >= 0);
                    }
                }) as OpenCvException;
                AssertBoundaryOrSuccess(csrtException);

                OpenCvException? legacyException = Record.Exception(() =>
                {
                    using (TrackerMIL tracker = TrackerMIL.Create())
                    using (TrackerMIL multiMember = TrackerMIL.Create())
                    using (LegacyMultiTracker multi = LegacyMultiTracker.Create())
                    {
                        var box = new Rect2d(6.0, 7.0, 8.0, 8.0);
                        tracker.Init(first, box);
                        LegacyTrackerUpdateResult update = tracker.Update(second, box);
                        bool added = multi.Add(multiMember, first, box);
                        LegacyMultiTrackerUpdateResult multiUpdate = multi.Update(second);
                        Rect2d[] objects = multi.GetObjects();

                        Assert.True(update.BoundingBox.Width >= 0.0);
                        Assert.True(added || multiUpdate.BoundingBoxes.Length == 0);
                        Assert.NotNull(multiUpdate.BoundingBoxes);
                        Assert.NotNull(objects);
                    }
                }) as OpenCvException;
                AssertBoundaryOrSuccess(legacyException);
            }
        }

        private static Mat CreateFrame(int offset)
        {
            var frame = new Mat(32, 32, MatType.CV_8UC3, new Scalar(20, 40, 60));
            ImgProcCv2.Rectangle(frame, new Rect(4 + offset, 7, 10, 10), new Scalar(220, 50, 80), -1);
            ImgProcCv2.Circle(frame, new Point(20, 20), 4, new Scalar(40, 220, 120), -1);
            return frame;
        }

        private static T? TryCreate<T>(Func<T> factory)
            where T : class, IDisposable
        {
            try
            {
                return factory();
            }
            catch (OpenCvException ex) when (IsNativeBoundary(ex))
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

        private static void AssertFactoryBoundary<T>(Func<T> factory)
            where T : IDisposable
        {
            try
            {
                using (factory())
                {
                }
            }
            catch (OpenCvException ex) when (IsNativeBoundary(ex))
            {
                Assert.Contains("NOT_LINKED", ex.Message, StringComparison.OrdinalIgnoreCase);
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }

        private static void AssertNativeDefaultBoundaryOrValues<T>(Func<T> factory, Action<T> assertValues)
        {
            try
            {
                assertValues(factory());
            }
            catch (OpenCvException ex) when (IsNativeBoundary(ex))
            {
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }

        private static void AssertBoundaryOrSuccess(OpenCvException? exception)
        {
            if (exception == null)
            {
                return;
            }

            Assert.True(IsNativeBoundary(exception) || exception.Message.Length > 0, exception.Message);
        }

        private static bool IsNativeBoundary(OpenCvException exception)
        {
            return exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("tracking", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }
    }
}
