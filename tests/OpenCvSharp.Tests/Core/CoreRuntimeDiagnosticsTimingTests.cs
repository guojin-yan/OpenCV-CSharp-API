using System;
using System.Globalization;
using OpenCvSharp.Core;

namespace OpenCvSharp.Tests.Core
{
    [Collection(NativeSmokeCollection.Name)]
    public class CoreRuntimeDiagnosticsTimingTests
    {
        [Fact]
        public void VersionAndBuildDiagnosticsAgreeWithExistingBuildInfo()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            int major = OpenCvSharpBuildInfo.GetNativeOpenCvVersionMajor();
            int minor = OpenCvSharpBuildInfo.GetNativeOpenCvVersionMinor();
            int revision = OpenCvSharpBuildInfo.GetNativeOpenCvVersionRevision();
            string numericVersion = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", major, minor, revision);

            Assert.StartsWith(numericVersion, OpenCvSharpBuildInfo.GetNativeOpenCvVersion(), StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(Cv2.GetBuildInformation()));
            Assert.NotNull(Cv2.GetCpuFeaturesLine());
        }

        [Fact]
        public void ClockHardwareAndAlgorithmDiagnosticsHaveStableMinimumContracts()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            long firstTick = Cv2.GetTickCount();
            long secondTick = Cv2.GetTickCount();
            Assert.True(Cv2.GetTickFrequency() > 0.0);
            Assert.True(secondTick >= firstTick);
            _ = Cv2.GetCpuTickCount();
            Assert.True(Cv2.GetNumberOfCpus() > 0);
            Assert.InRange((int)Cv2.GetDefaultAlgorithmHint(), (int)AlgorithmHint.Default, (int)AlgorithmHint.Approximate);
            Assert.NotNull(Cv2.GetHardwareFeatureName(CpuFeatures.None));
            _ = Cv2.CheckHardwareSupport(CpuFeatures.None);
        }

        [Fact]
        public void ThreadConfigurationAcceptsOpenCvResetAndRestoresGlobalState()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            int originalThreadCount = Cv2.GetNumThreads();
            try
            {
                Cv2.SetNumThreads(1);
                Assert.Equal(1, Cv2.GetNumThreads());

                // OpenCV defines any negative count as a reset-to-default request.
                Cv2.SetNumThreads(-1);
                Assert.True(Cv2.GetNumThreads() >= 0);
            }
            finally
            {
                Cv2.SetNumThreads(originalThreadCount);
            }
        }

        [Fact]
        public void OptimizationStateChangesAndRestoresInFinally()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            bool originalUseOptimized = Cv2.UseOptimized();
            try
            {
                Cv2.SetUseOptimized(!originalUseOptimized);
                Assert.Equal(!originalUseOptimized, Cv2.UseOptimized());
            }
            finally
            {
                Cv2.SetUseOptimized(originalUseOptimized);
            }
        }

        [Fact]
        public void TickMeterMaintainsTotalsLastValuesAndResetSemantics()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using var meter = new TickMeter();
            Assert.Equal(0L, meter.Counter);
            Assert.Equal(0L, meter.TimeTicks);

            meter.Start();
            _ = Cv2.GetTickCount();
            meter.Stop();
            Assert.Equal(1L, meter.Counter);
            Assert.True(meter.TimeTicks >= meter.LastTimeTicks);
            Assert.True(meter.TimeMicroseconds >= meter.LastTimeMicroseconds);
            Assert.True(meter.TimeMilliseconds >= meter.LastTimeMilliseconds);
            Assert.True(meter.TimeSeconds >= meter.LastTimeSeconds);
            Assert.Equal(meter.TimeMicroseconds / 1000.0, meter.TimeMilliseconds, 6);
            Assert.Equal(meter.TimeMilliseconds / 1000.0, meter.TimeSeconds, 9);
            Assert.Equal(meter.TimeSeconds, meter.AverageTimeSeconds, 9);
            Assert.Equal(meter.TimeMilliseconds, meter.AverageTimeMilliseconds, 6);
            Assert.False(double.IsNaN(meter.FramesPerSecond));
            Assert.False(double.IsInfinity(meter.FramesPerSecond));

            meter.Start();
            _ = Cv2.GetTickCount();
            meter.Stop();
            Assert.Equal(2L, meter.Counter);
            Assert.Equal(meter.TimeSeconds / meter.Counter, meter.AverageTimeSeconds, 9);

            meter.Reset();
            Assert.Equal(0L, meter.Counter);
            Assert.Equal(0L, meter.TimeTicks);
            Assert.Equal(0.0, meter.TimeMicroseconds);
            Assert.Equal(0.0, meter.TimeMilliseconds);
            Assert.Equal(0.0, meter.TimeSeconds);
        }

        [Fact]
        public void TickMeterDisposalAndInvalidFeatureValuesFailBeforeNativeUse()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var meter = new TickMeter();
            meter.Dispose();
            meter.Dispose();
            Assert.True(meter.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => meter.Start());
            Assert.Throws<ObjectDisposedException>(() => _ = meter.Counter);
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.CheckHardwareSupport((CpuFeatures)(-1)));
            Assert.Throws<ArgumentOutOfRangeException>(() => Cv2.GetHardwareFeatureName((CpuFeatures)513));
        }
    }
}
