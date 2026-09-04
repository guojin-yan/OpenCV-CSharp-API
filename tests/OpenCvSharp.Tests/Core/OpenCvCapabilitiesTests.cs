using System;
using JYPPX.OpenCvSharp.Dnn;
using JYPPX.OpenCvSharp.VideoIO;

namespace JYPPX.OpenCvSharp.Tests.Core
{
    public sealed class OpenCvCapabilitiesTests
    {
        [Fact]
        public void ProbeResultHasStableStateAndAvailabilitySemantics()
        {
            var unknown = new OpenCvCapabilityProbe("opencl-tapi", OpenCvCapabilityState.Unknown, "not probed");
            var verified = new OpenCvCapabilityProbe("native-runtime", OpenCvCapabilityState.Verified, string.Empty);

            Assert.False(unknown.IsAvailable);
            Assert.True(verified.IsAvailable);
            Assert.Equal("opencl-tapi:Unknown (not probed)", unknown.ToString());
            Assert.Equal("native-runtime:Verified", verified.ToString());
            Assert.Throws<ArgumentException>(() => new OpenCvCapabilityProbe("", OpenCvCapabilityState.Unknown, ""));
            Assert.Throws<ArgumentOutOfRangeException>(() => new OpenCvCapabilityProbe("invalid", (OpenCvCapabilityState)99, ""));
        }

        [Fact]
        public void SnapshotIsSafeToReadWithoutNativeSmoke()
        {
            OpenCvCapabilities capabilities = OpenCvCapabilities.GetCurrent();

            Assert.Equal(OpenCvSharpBuildInfo.OpenCvVersion, capabilities.OpenCvVersion);
            Assert.Equal(OpenCvSharpBuildInfo.NativeAbiVersion, capabilities.NativeAbiVersion);
            Assert.Equal(OpenCvSharpBuildInfo.NuGetPackageVersion, capabilities.ManagedPackageVersion);
            Assert.True(capabilities.ProcessBitness == 32 || capabilities.ProcessBitness == 64);
            Assert.NotEmpty(capabilities.OperatingSystemDescription);
            Assert.NotEmpty(capabilities.RuntimeFrameworkDescription);
            Assert.NotEmpty(capabilities.ProcessArchitecture);
            Assert.NotNull(capabilities.RuntimeIdentifier);
            Assert.NotNull(capabilities.NativeRuntime);
            Assert.NotNull(capabilities.VideoIOBackends);
            Assert.NotNull(capabilities.DnnBackends);
            Assert.NotNull(capabilities.Accelerators);
            Assert.Contains(capabilities.Accelerators, value => value.Name == "opencl-tapi" && value.State == OpenCvCapabilityState.Unknown);
            Assert.Contains(capabilities.Accelerators, value => value.Name == "cuda" && value.State == OpenCvCapabilityState.Unknown);
            Assert.DoesNotContain(capabilities.Warnings, value => value.IndexOf(Environment.CurrentDirectory, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public void SnapshotDoesNotExposeMutableCollections()
        {
            OpenCvCapabilities capabilities = OpenCvCapabilities.GetCurrent();

            Assert.IsAssignableFrom<System.Collections.Generic.IReadOnlyList<OpenCvVideoBackendCapability>>(capabilities.VideoIOBackends);
            Assert.IsAssignableFrom<System.Collections.Generic.IReadOnlyList<OpenCvDnnBackendCapability>>(capabilities.DnnBackends);
            Assert.IsAssignableFrom<System.Collections.Generic.IReadOnlyList<string>>(capabilities.Warnings);
            Assert.DoesNotContain(capabilities.Accelerators, value => value.Name == "");
        }

        [Fact]
        public void NativeSmokeSnapshotVerifiesExistingProbes()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            OpenCvCapabilities capabilities = OpenCvCapabilities.GetCurrent();

            Assert.Equal(OpenCvCapabilityState.Verified, capabilities.NativeRuntime.State);
            Assert.Equal(OpenCvSharpBuildInfo.OpenCvVersion, capabilities.NativeOpenCvVersion);
            Assert.Equal(OpenCvSharpBuildInfo.NativeAbiVersion, capabilities.LoadedNativeAbiVersion);
            Assert.NotEmpty(capabilities.CpuFeaturesLine);
            Assert.True(capabilities.LogicalCpuCount > 0);
            Assert.NotNull(capabilities.UseOptimized);
        }
    }
}
