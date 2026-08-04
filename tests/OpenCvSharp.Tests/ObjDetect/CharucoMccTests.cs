using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ObjDetect;

namespace JYPPX.OpenCvSharp.Tests.ObjDetect
{
    public sealed class CharucoMccTests
    {
        [Fact]
        public void ColorChartEnumValuesMatchOpenCvMccConstants()
        {
            Assert.Equal(0, (int)ColorChart.Mcc24);
            Assert.Equal(1, (int)ColorChart.Sg140);
            Assert.Equal(2, (int)ColorChart.Vinyl18);
        }

        [Fact]
        public void RefineParametersStoreValues()
        {
            var parameters = new ArucoRefineParameters(10.0F, 3.0F, true);

            Assert.Equal(10.0F, parameters.MinRepDistance, 5);
            Assert.Equal(3.0F, parameters.ErrorCorrectionRate, 5);
            Assert.True(parameters.CheckAllOrders);
        }

        [Fact]
        public void MccParametersCanBeConstructedAndClonedWithoutNativeDefaults()
        {
            var parameters = new DetectorParametersMCC(
                adaptiveThreshWinSizeMin: 3,
                adaptiveThreshWinSizeMax: 23,
                adaptiveThreshWinSizeStep: 4,
                adaptiveThreshConstant: 7.5,
                minContoursAreaRate: 0.01,
                minContoursArea: 12.5,
                confidenceThreshold: 0.8,
                minContourSolidity: 0.9,
                findCandidatesApproxPolyDPEpsMultiplier: 0.05,
                borderWidth: 2,
                b0Factor: 1.25F,
                maxError: 0.4F,
                minContourPointsAllowed: 5,
                minContourLengthAllowed: 6,
                minInterContourDistance: 7,
                minInterCheckerDistance: 8,
                minImageSize: 9,
                minGroupSize: 10);

            DetectorParametersMCC copy = new DetectorParametersMCC(parameters);
            DetectorParametersMCC clone = parameters.Clone();
            Assert.NotSame(parameters, copy);
            Assert.NotSame(parameters, clone);
            Assert.NotSame(copy, clone);
            parameters.MinGroupSize = 99;
            clone.BorderWidth = 42;

            Assert.Equal(3, copy.AdaptiveThreshWinSizeMin);
            Assert.Equal(23, copy.AdaptiveThreshWinSizeMax);
            Assert.Equal(4, copy.AdaptiveThreshWinSizeStep);
            Assert.Equal(7.5, copy.AdaptiveThreshConstant, 5);
            Assert.Equal(0.01, copy.MinContoursAreaRate, 5);
            Assert.Equal(12.5, copy.MinContoursArea, 5);
            Assert.Equal(0.8, copy.ConfidenceThreshold, 5);
            Assert.Equal(0.9, copy.MinContourSolidity, 5);
            Assert.Equal(0.05, copy.FindCandidatesApproxPolyDPEpsMultiplier, 5);
            Assert.Equal(2, copy.BorderWidth);
            Assert.Equal(1.25F, copy.B0Factor, 5);
            Assert.Equal(0.4F, copy.MaxError, 5);
            Assert.Equal(5, copy.MinContourPointsAllowed);
            Assert.Equal(6, copy.MinContourLengthAllowed);
            Assert.Equal(7, copy.MinInterContourDistance);
            Assert.Equal(8, copy.MinInterCheckerDistance);
            Assert.Equal(9, copy.MinImageSize);
            Assert.Equal(10, copy.MinGroupSize);
            Assert.Equal(10, clone.MinGroupSize);
            Assert.Equal(2, copy.BorderWidth);
            Assert.Equal(
                "DetectorParametersMCC(AdaptiveThreshWinSizeMin=3, AdaptiveThreshWinSizeMax=23, AdaptiveThreshWinSizeStep=4, AdaptiveThreshConstant=7.5, MinContoursAreaRate=0.01, MinContoursArea=12.5, ConfidenceThreshold=0.8, MinContourSolidity=0.9, FindCandidatesApproxPolyDPEpsMultiplier=0.05, BorderWidth=2, B0Factor=1.25, MaxError=0.4, MinContourPointsAllowed=5, MinContourLengthAllowed=6, MinInterContourDistance=7, MinInterCheckerDistance=8, MinImageSize=9, MinGroupSize=10)",
                copy.ToString());
            Assert.Throws<ArgumentNullException>(() => new DetectorParametersMCC(null!));
        }

        [Fact]
        public void MccParametersFormatInvariantly()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
                CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

                var parameters = new DetectorParametersMCC(
                    adaptiveThreshWinSizeMin: 3,
                    adaptiveThreshWinSizeMax: 23,
                    adaptiveThreshWinSizeStep: 4,
                    adaptiveThreshConstant: 7.5,
                    minContoursAreaRate: 0.01,
                    minContoursArea: 12.5,
                    confidenceThreshold: 0.8,
                    minContourSolidity: 0.9,
                    findCandidatesApproxPolyDPEpsMultiplier: 0.05,
                    borderWidth: 2,
                    b0Factor: 1.25F,
                    maxError: 0.4F,
                    minContourPointsAllowed: 5,
                    minContourLengthAllowed: 6,
                    minInterContourDistance: 7,
                    minInterCheckerDistance: 8,
                    minImageSize: 9,
                    minGroupSize: 10);

                string formatted = parameters.ToString();
                Assert.Contains("AdaptiveThreshConstant=7.5", formatted, StringComparison.Ordinal);
                Assert.Contains("MinContoursAreaRate=0.01", formatted, StringComparison.Ordinal);
                Assert.Contains("B0Factor=1.25", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("7,5", formatted, StringComparison.Ordinal);
                Assert.DoesNotContain("0,01", formatted, StringComparison.Ordinal);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUiCulture;
            }
        }

        [Fact]
        public void MccDefaultParametersAreAvailableBehindNativeSmokeGuard()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var parameters = new DetectorParametersMCC();

            Assert.True(parameters.AdaptiveThreshWinSizeMin >= 0);
            Assert.True(parameters.AdaptiveThreshWinSizeMax >= parameters.AdaptiveThreshWinSizeMin);
            Assert.True(parameters.MinGroupSize >= 0);
        }

        [Fact]
        public void MccCheckerDetectorDetectionParametersRoundTripWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var parameters = new DetectorParametersMCC(
                adaptiveThreshWinSizeMin: 5,
                adaptiveThreshWinSizeMax: 31,
                adaptiveThreshWinSizeStep: 6,
                adaptiveThreshConstant: 9.25,
                minContoursAreaRate: 0.015,
                minContoursArea: 18.5,
                confidenceThreshold: 0.75,
                minContourSolidity: 0.85,
                findCandidatesApproxPolyDPEpsMultiplier: 0.07,
                borderWidth: 3,
                b0Factor: 1.5F,
                maxError: 0.35F,
                minContourPointsAllowed: 7,
                minContourLengthAllowed: 8,
                minInterContourDistance: 9,
                minInterCheckerDistance: 10,
                minImageSize: 11,
                minGroupSize: 12);

            using (var detector = new CCheckerDetector())
            {
                Assert.Same(detector, detector.SetDetectionParams(parameters));

                DetectorParametersMCC roundTrip = detector.GetDetectionParams();

                AssertDetectorParametersEqual(parameters, roundTrip);
            }
        }

        [Fact]
        public void MccCheckerDetectorValidatesManagedArgumentsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var detector = new CCheckerDetector())
            using (var checker = new CChecker())
            using (var image = new Mat())
            using (var charts = new Mat())
            {
                Assert.False(detector.IsDisposed);
                Assert.False(checker.IsDisposed);
                checker.Target = ColorChart.Mcc24;
                Assert.Equal(ColorChart.Mcc24, checker.Target);

                Assert.Throws<ArgumentNullException>(() => detector.Process(null!));
                Assert.Throws<ArgumentNullException>(() => detector.Process(image, (Rect[])null!));
                Assert.Throws<ArgumentNullException>(() => detector.Draw(null!, image));
                Assert.Throws<ArgumentNullException>(() => detector.Draw(new[] { checker }, null!));
                Assert.Throws<ArgumentNullException>(() => detector.SetDetectionParams(null!));
                Assert.Throws<ArgumentNullException>(() => checker.SetBox((Point2f[])null!));
                Assert.Throws<ArgumentNullException>(() => checker.SetChartsRGB(null!));
                Assert.Throws<ArgumentNullException>(() => checker.SetChartsYCbCr(null!));

                checker.SetBox(new[]
                {
                    new Point2f(0.0F, 0.0F),
                    new Point2f(1.0F, 0.0F),
                    new Point2f(1.0F, 1.0F),
                    new Point2f(0.0F, 1.0F)
                });
                checker.SetChartsRGB(charts);
                checker.SetChartsYCbCr(charts);
                DetectorParametersMCC parameters = detector.GetDetectionParams();
                Assert.Same(detector, detector.SetDetectionParams(new DetectorParametersMCC()));
                detector.Draw(Array.Empty<CChecker>(), image);

                detector.Dispose();

                Assert.True(detector.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => detector.ColorChartType);
                Assert.Throws<ObjectDisposedException>(() => detector.ColorChartType = ColorChart.Sg140);
                Assert.Throws<ObjectDisposedException>(() => detector.Process(image));
                Assert.Throws<ObjectDisposedException>(() => detector.Process(image, Array.Empty<Rect>()));
                Assert.Throws<ObjectDisposedException>(() => detector.GetBestColorChecker());
                Assert.Throws<ObjectDisposedException>(() => detector.GetListColorChecker());
                Assert.Throws<ObjectDisposedException>(() => detector.Draw(Array.Empty<CChecker>(), image));
                Assert.Throws<ObjectDisposedException>(() => detector.GetRefColors());
                Assert.Throws<ObjectDisposedException>(() => detector.GetDetectionParams());
                Assert.Throws<ObjectDisposedException>(() => detector.SetDetectionParams(parameters));

                checker.Dispose();

                Assert.True(checker.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => checker.Target);
                Assert.Throws<ObjectDisposedException>(() => checker.Target = ColorChart.Sg140);
                Assert.Throws<ObjectDisposedException>(() => checker.Cost);
                Assert.Throws<ObjectDisposedException>(() => checker.Cost = 1.0F);
                Assert.Throws<ObjectDisposedException>(() => checker.Center);
                Assert.Throws<ObjectDisposedException>(() => checker.Center = new Point2f(1.0F, 2.0F));
                Assert.Throws<ObjectDisposedException>(() => checker.GetBox());
                Assert.Throws<ObjectDisposedException>(() => checker.SetBox(Array.Empty<Point2f>()));
                Assert.Throws<ObjectDisposedException>(() => checker.GetColorCharts());
                Assert.Throws<ObjectDisposedException>(() => checker.GetChartsRGB());
                Assert.Throws<ObjectDisposedException>(() => checker.SetChartsRGB(charts));
                Assert.Throws<ObjectDisposedException>(() => checker.GetChartsYCbCr());
                Assert.Throws<ObjectDisposedException>(() => checker.SetChartsYCbCr(charts));
            }
        }

        private static void AssertDetectorParametersEqual(DetectorParametersMCC expected, DetectorParametersMCC actual)
        {
            Assert.Equal(expected.AdaptiveThreshWinSizeMin, actual.AdaptiveThreshWinSizeMin);
            Assert.Equal(expected.AdaptiveThreshWinSizeMax, actual.AdaptiveThreshWinSizeMax);
            Assert.Equal(expected.AdaptiveThreshWinSizeStep, actual.AdaptiveThreshWinSizeStep);
            Assert.Equal(expected.AdaptiveThreshConstant, actual.AdaptiveThreshConstant, 5);
            Assert.Equal(expected.MinContoursAreaRate, actual.MinContoursAreaRate, 5);
            Assert.Equal(expected.MinContoursArea, actual.MinContoursArea, 5);
            Assert.Equal(expected.ConfidenceThreshold, actual.ConfidenceThreshold, 5);
            Assert.Equal(expected.MinContourSolidity, actual.MinContourSolidity, 5);
            Assert.Equal(expected.FindCandidatesApproxPolyDPEpsMultiplier, actual.FindCandidatesApproxPolyDPEpsMultiplier, 5);
            Assert.Equal(expected.BorderWidth, actual.BorderWidth);
            Assert.Equal(expected.B0Factor, actual.B0Factor, 5);
            Assert.Equal(expected.MaxError, actual.MaxError, 5);
            Assert.Equal(expected.MinContourPointsAllowed, actual.MinContourPointsAllowed);
            Assert.Equal(expected.MinContourLengthAllowed, actual.MinContourLengthAllowed);
            Assert.Equal(expected.MinInterContourDistance, actual.MinInterContourDistance);
            Assert.Equal(expected.MinInterCheckerDistance, actual.MinInterCheckerDistance);
            Assert.Equal(expected.MinImageSize, actual.MinImageSize);
            Assert.Equal(expected.MinGroupSize, actual.MinGroupSize);
        }

    }
}
