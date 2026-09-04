using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using JYPPX.OpenCvSharp.VideoIO;
using CoreCv2 = JYPPX.OpenCvSharp.Core.Cv2;

namespace JYPPX.OpenCvSharp
{
    /// <summary>
    /// Describes how strongly a runtime capability has been established.
    /// 描述运行时能力被确认的强度。
    /// </summary>
    public enum OpenCvCapabilityState
    {
        /// <summary>No probe result is available. 没有可用的探针结果。</summary>
        Unknown = 0,

        /// <summary>The capability is declared by metadata or an enum. 能力由元数据或枚举声明。</summary>
        Declared = 1,

        /// <summary>The capability initialized or queried successfully. 能力已成功初始化或查询。</summary>
        Available = 2,

        /// <summary>A side-effect-free runtime probe verified the capability. 无副作用运行时探针已验证能力。</summary>
        Verified = 3,

        /// <summary>The probe ran and reported that the capability is unavailable. 探针已执行并报告能力不可用。</summary>
        Unavailable = 4
    }

    /// <summary>
    /// A structured capability result with a stable diagnostic reason.
    /// 带有稳定诊断原因的结构化能力结果。
    /// </summary>
    public sealed class OpenCvCapabilityProbe
    {
        /// <summary>Initializes a capability result.</summary>
        public OpenCvCapabilityProbe(string name, OpenCvCapabilityState state, string reason)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Capability name cannot be empty.", nameof(name));
            }
            if (state < OpenCvCapabilityState.Unknown || state > OpenCvCapabilityState.Unavailable)
            {
                throw new ArgumentOutOfRangeException(nameof(state));
            }

            Name = name;
            State = state;
            Reason = reason ?? string.Empty;
        }

        /// <summary>Gets the stable capability name.</summary>
        public string Name { get; }

        /// <summary>Gets the probe state.</summary>
        public OpenCvCapabilityState State { get; }

        /// <summary>Gets a non-sensitive diagnostic reason.</summary>
        public string Reason { get; }

        /// <summary>Gets whether the capability was established as available.</summary>
        public bool IsAvailable
        {
            get { return State == OpenCvCapabilityState.Available || State == OpenCvCapabilityState.Verified; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return Name + ":" + State + (string.IsNullOrEmpty(Reason) ? string.Empty : " (" + Reason + ")");
        }
    }

    /// <summary>
    /// Describes one VideoIO backend without opening a camera or stream.
    /// 描述一个 VideoIO 后端，不打开摄像头或流。
    /// </summary>
    public sealed class OpenCvVideoBackendCapability
    {
        internal OpenCvVideoBackendCapability(VideoCaptureAPIs api, string name, OpenCvCapabilityState state, bool isBuiltIn, string reason)
        {
            Api = api;
            Name = name ?? string.Empty;
            State = state;
            IsBuiltIn = isBuiltIn;
            Reason = reason ?? string.Empty;
        }

        /// <summary>Gets the OpenCV backend identifier.</summary>
        public VideoCaptureAPIs Api { get; }

        /// <summary>Gets the backend name.</summary>
        public string Name { get; }

        /// <summary>Gets the backend probe state.</summary>
        public OpenCvCapabilityState State { get; }

        /// <summary>Gets whether OpenCV reports the backend as built in.</summary>
        public bool IsBuiltIn { get; }

        /// <summary>Gets a non-sensitive diagnostic reason.</summary>
        public string Reason { get; }

        /// <summary>Gets whether the backend was reported as available.</summary>
        public bool IsAvailable
        {
            get { return State == OpenCvCapabilityState.Available || State == OpenCvCapabilityState.Verified; }
        }
    }

    /// <summary>
    /// Describes DNN targets returned for one backend.
    /// 描述一个 DNN 后端返回的目标设备。
    /// </summary>
    public sealed class OpenCvDnnBackendCapability
    {
        internal OpenCvDnnBackendCapability(DnnBackend backend, OpenCvCapabilityState state, DnnTarget[] targets, string reason)
        {
            Backend = backend;
            State = state;
            Targets = new ReadOnlyCollection<DnnTarget>(targets ?? Array.Empty<DnnTarget>());
            Reason = reason ?? string.Empty;
        }

        /// <summary>Gets the DNN backend.</summary>
        public DnnBackend Backend { get; }

        /// <summary>Gets the probe state.</summary>
        public OpenCvCapabilityState State { get; }

        /// <summary>Gets the targets returned by OpenCV.</summary>
        public IReadOnlyList<DnnTarget> Targets { get; }

        /// <summary>Gets a non-sensitive diagnostic reason.</summary>
        public string Reason { get; }

        /// <summary>Gets whether the backend returned a usable target list.</summary>
        public bool IsAvailable
        {
            get { return State == OpenCvCapabilityState.Available || State == OpenCvCapabilityState.Verified; }
        }
    }

    /// <summary>
    /// A side-effect-free snapshot assembled from the existing managed runtime probes.
    /// 由现有 managed 运行时探针组合出的无副作用快照。
    /// </summary>
    public sealed class OpenCvCapabilities
    {
        private static readonly DnnBackend[] KnownDnnBackends =
        {
            DnnBackend.Default,
            DnnBackend.InferenceEngine,
            DnnBackend.OpenCV,
            DnnBackend.VkCom,
            DnnBackend.Cuda,
            DnnBackend.WebNN,
            DnnBackend.TimVx,
            DnnBackend.Cann
        };

        private OpenCvCapabilities(
            OpenCvCapabilityProbe nativeRuntime,
            string nativeOpenCvVersion,
            int? loadedNativeAbiVersion,
            string cpuFeaturesLine,
            int? logicalCpuCount,
            bool? useOptimized,
            string operatingSystemDescription,
            string runtimeFrameworkDescription,
            string processArchitecture,
            string runtimeIdentifier,
            IReadOnlyList<OpenCvVideoBackendCapability> videoIoBackends,
            IReadOnlyList<OpenCvDnnBackendCapability> dnnBackends,
            IReadOnlyList<OpenCvCapabilityProbe> accelerators,
            IReadOnlyList<string> warnings)
        {
            NativeRuntime = nativeRuntime;
            ManagedPackageVersion = OpenCvSharpBuildInfo.NuGetPackageVersion;
            OpenCvVersion = OpenCvSharpBuildInfo.OpenCvVersion;
            NativeAbiVersion = OpenCvSharpBuildInfo.NativeAbiVersion;
            TargetFramework = OpenCvSharpBuildInfo.TargetFramework;
            ProcessBitness = IntPtr.Size * 8;
            OperatingSystem = Environment.OSVersion.Platform.ToString();
            OperatingSystemDescription = operatingSystemDescription ?? string.Empty;
            RuntimeFrameworkDescription = runtimeFrameworkDescription ?? string.Empty;
            ProcessArchitecture = processArchitecture ?? string.Empty;
            RuntimeIdentifier = runtimeIdentifier ?? string.Empty;
            NativeOpenCvVersion = nativeOpenCvVersion ?? string.Empty;
            LoadedNativeAbiVersion = loadedNativeAbiVersion;
            CpuFeaturesLine = cpuFeaturesLine ?? string.Empty;
            LogicalCpuCount = logicalCpuCount;
            UseOptimized = useOptimized;
            VideoIOBackends = videoIoBackends ?? new ReadOnlyCollection<OpenCvVideoBackendCapability>(Array.Empty<OpenCvVideoBackendCapability>());
            DnnBackends = dnnBackends ?? new ReadOnlyCollection<OpenCvDnnBackendCapability>(Array.Empty<OpenCvDnnBackendCapability>());
            Accelerators = accelerators ?? new ReadOnlyCollection<OpenCvCapabilityProbe>(Array.Empty<OpenCvCapabilityProbe>());
            Warnings = warnings ?? new ReadOnlyCollection<string>(Array.Empty<string>());
        }

        /// <summary>Gets a fresh snapshot of the current runtime.</summary>
        /// <remarks>This method does not open devices, load models, or execute image algorithms.</remarks>
        public static OpenCvCapabilities GetCurrent()
        {
            var warnings = new List<string>();
            int? loadedAbi = null;
            string nativeVersion = string.Empty;
            OpenCvCapabilityState nativeState = OpenCvCapabilityState.Unknown;

            try
            {
                loadedAbi = OpenCvSharpBuildInfo.GetLoadedNativeAbiVersion();
                nativeState = loadedAbi.Value == OpenCvSharpBuildInfo.NativeAbiVersion
                    ? OpenCvCapabilityState.Available
                    : OpenCvCapabilityState.Unavailable;
                if (nativeState == OpenCvCapabilityState.Unavailable)
                {
                    warnings.Add("Loaded native ABI does not match the managed package.");
                }
            }
            catch (Exception exception) when (IsRuntimeProbeFailure(exception))
            {
                warnings.Add("Native ABI probe unavailable: " + GetExceptionReason(exception));
            }

            try
            {
                nativeVersion = OpenCvSharpBuildInfo.GetNativeOpenCvVersion();
                if (string.Equals(nativeVersion, OpenCvSharpBuildInfo.OpenCvVersion, StringComparison.Ordinal))
                {
                    nativeState = nativeState == OpenCvCapabilityState.Available
                        ? OpenCvCapabilityState.Verified
                        : nativeState;
                }
                else
                {
                    nativeState = OpenCvCapabilityState.Unavailable;
                    warnings.Add("Loaded OpenCV version does not match the managed target.");
                }
            }
            catch (Exception exception) when (IsRuntimeProbeFailure(exception))
            {
                warnings.Add("Native OpenCV version probe unavailable: " + GetExceptionReason(exception));
            }

            var nativeRuntime = new OpenCvCapabilityProbe(
                "native-runtime",
                nativeState,
                nativeState == OpenCvCapabilityState.Verified
                    ? "ABI and OpenCV version match."
                    : "ABI/version verification did not complete.");

            string cpuFeaturesLine = string.Empty;
            int? logicalCpuCount = null;
            bool? useOptimized = null;
            try
            {
                cpuFeaturesLine = CoreCv2.GetCpuFeaturesLine();
            }
            catch (Exception exception) when (IsRuntimeProbeFailure(exception))
            {
                warnings.Add("CPU feature probe unavailable: " + GetExceptionReason(exception));
            }

            try
            {
                logicalCpuCount = CoreCv2.GetNumberOfCpus();
            }
            catch (Exception exception) when (IsRuntimeProbeFailure(exception))
            {
                warnings.Add("CPU count probe unavailable: " + GetExceptionReason(exception));
            }

            try
            {
                useOptimized = CoreCv2.UseOptimized();
            }
            catch (Exception exception) when (IsRuntimeProbeFailure(exception))
            {
                warnings.Add("Optimization probe unavailable: " + GetExceptionReason(exception));
            }

            IReadOnlyList<OpenCvVideoBackendCapability> videoBackends = ProbeVideoIo(warnings);
            IReadOnlyList<OpenCvDnnBackendCapability> dnnBackends = ProbeDnn(warnings);
            var accelerators = new ReadOnlyCollection<OpenCvCapabilityProbe>(new[]
            {
                new OpenCvCapabilityProbe("opencl-tapi", OpenCvCapabilityState.Unknown, "No public UMat/OpenCL execution probe is currently exposed."),
                new OpenCvCapabilityProbe("cuda", OpenCvCapabilityState.Unknown, "No public CUDA execution probe is currently exposed.")
            });

            return new OpenCvCapabilities(
                nativeRuntime,
                nativeVersion,
                loadedAbi,
                cpuFeaturesLine,
                logicalCpuCount,
                useOptimized,
                GetOperatingSystemDescription(),
                GetRuntimeFrameworkDescription(),
                GetProcessArchitecture(),
                GetRuntimeIdentifier(),
                videoBackends,
                dnnBackends,
                accelerators,
                new ReadOnlyCollection<string>(warnings.ToArray()));
        }

        /// <summary>Gets the managed package version.</summary>
        public string ManagedPackageVersion { get; }

        /// <summary>Gets the OpenCV version targeted by the managed package.</summary>
        public string OpenCvVersion { get; }

        /// <summary>Gets the managed native ABI version.</summary>
        public int NativeAbiVersion { get; }

        /// <summary>Gets the current managed target framework.</summary>
        public string TargetFramework { get; }

        /// <summary>Gets the process bitness.</summary>
        public int ProcessBitness { get; }

        /// <summary>Gets the coarse platform identifier without exposing a machine path.</summary>
        public string OperatingSystem { get; }

        /// <summary>Gets the runtime-provided operating system description.</summary>
        public string OperatingSystemDescription { get; }

        /// <summary>Gets the runtime framework description.</summary>
        public string RuntimeFrameworkDescription { get; }

        /// <summary>Gets the process architecture reported by the runtime.</summary>
        public string ProcessArchitecture { get; }

        /// <summary>Gets the runtime identifier when the target framework provides one.</summary>
        public string RuntimeIdentifier { get; }

        /// <summary>Gets the native runtime verification result.</summary>
        public OpenCvCapabilityProbe NativeRuntime { get; }

        /// <summary>Gets the loaded native OpenCV version, or an empty string when unavailable.</summary>
        public string NativeOpenCvVersion { get; }

        /// <summary>Gets the loaded native ABI version, or null when unavailable.</summary>
        public int? LoadedNativeAbiVersion { get; }

        /// <summary>Gets the CPU dispatch line reported by OpenCV.</summary>
        public string CpuFeaturesLine { get; }

        /// <summary>Gets the logical CPU count reported by OpenCV, or null when unavailable.</summary>
        public int? LogicalCpuCount { get; }

        /// <summary>Gets whether OpenCV optimized dispatch is enabled, or null when unavailable.</summary>
        public bool? UseOptimized { get; }

        /// <summary>Gets the VideoIO backends reported by OpenCV.</summary>
        public IReadOnlyList<OpenCvVideoBackendCapability> VideoIOBackends { get; }

        /// <summary>Gets the DNN backend target probes.</summary>
        public IReadOnlyList<OpenCvDnnBackendCapability> DnnBackends { get; }

        /// <summary>Gets explicitly separated accelerator probes.</summary>
        public IReadOnlyList<OpenCvCapabilityProbe> Accelerators { get; }

        /// <summary>Gets non-sensitive warnings collected while building the snapshot.</summary>
        public IReadOnlyList<string> Warnings { get; }

        private static IReadOnlyList<OpenCvVideoBackendCapability> ProbeVideoIo(List<string> warnings)
        {
            try
            {
                VideoCaptureAPIs[] backends = VideoIORegistry.GetBackends();
                var result = new List<OpenCvVideoBackendCapability>(backends.Length);
                for (int i = 0; i < backends.Length; i++)
                {
                    VideoCaptureAPIs api = backends[i];
                    string name = string.Empty;
                    bool isBuiltIn = false;
                    try
                    {
                        name = VideoIORegistry.GetBackendName(api);
                        isBuiltIn = VideoIORegistry.IsBackendBuiltIn(api);
                        result.Add(new OpenCvVideoBackendCapability(api, name, OpenCvCapabilityState.Verified, isBuiltIn, "Backend registry probe succeeded."));
                    }
                    catch (Exception exception) when (IsRuntimeProbeFailure(exception))
                    {
                        string reason = GetExceptionReason(exception);
                        warnings.Add("VideoIO backend probe failed for " + api + ": " + reason);
                        result.Add(new OpenCvVideoBackendCapability(api, name, OpenCvCapabilityState.Unavailable, isBuiltIn, reason));
                    }
                }

                return new ReadOnlyCollection<OpenCvVideoBackendCapability>(result.ToArray());
            }
            catch (Exception exception) when (IsRuntimeProbeFailure(exception))
            {
                warnings.Add("VideoIO registry probe unavailable: " + GetExceptionReason(exception));
                return new ReadOnlyCollection<OpenCvVideoBackendCapability>(Array.Empty<OpenCvVideoBackendCapability>());
            }
        }

        private static IReadOnlyList<OpenCvDnnBackendCapability> ProbeDnn(List<string> warnings)
        {
            var result = new List<OpenCvDnnBackendCapability>(KnownDnnBackends.Length);
            for (int i = 0; i < KnownDnnBackends.Length; i++)
            {
                DnnBackend backend = KnownDnnBackends[i];
                try
                {
                    DnnTarget[] targets = Dnn.Cv2.GetAvailableTargets(backend);
                    OpenCvCapabilityState state = targets.Length == 0
                        ? OpenCvCapabilityState.Unavailable
                        : OpenCvCapabilityState.Verified;
                    result.Add(new OpenCvDnnBackendCapability(backend, state, targets, targets.Length == 0 ? "No usable target was reported." : "Target probe succeeded."));
                }
                catch (Exception exception) when (IsRuntimeProbeFailure(exception))
                {
                    string reason = GetExceptionReason(exception);
                    warnings.Add("DNN backend probe failed for " + backend + ": " + reason);
                    result.Add(new OpenCvDnnBackendCapability(backend, OpenCvCapabilityState.Unavailable, Array.Empty<DnnTarget>(), reason));
                }
            }

            return new ReadOnlyCollection<OpenCvDnnBackendCapability>(result.ToArray());
        }

        private static bool IsRuntimeProbeFailure(Exception exception)
        {
            return exception is DllNotFoundException ||
                exception is EntryPointNotFoundException ||
                exception is BadImageFormatException ||
                exception is OpenCvException ||
                exception is InvalidOperationException;
        }

        private static string GetExceptionReason(Exception exception)
        {
            if (exception is DllNotFoundException)
            {
                return "Native library was not found.";
            }
            if (exception is EntryPointNotFoundException)
            {
                return "Required native entry point was not found.";
            }
            if (exception is BadImageFormatException)
            {
                return "Native library architecture or format is incompatible.";
            }
            if (exception is OpenCvException)
            {
                return "OpenCV runtime probe failed.";
            }

            return "Runtime metadata or probe state is invalid.";
        }

        private static string GetOperatingSystemDescription()
        {
#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            return RuntimeInformation.OSDescription;
#else
            return Environment.OSVersion.VersionString;
#endif
        }

        private static string GetRuntimeFrameworkDescription()
        {
#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            return RuntimeInformation.FrameworkDescription;
#else
            return typeof(object).Assembly.ImageRuntimeVersion;
#endif
        }

        private static string GetProcessArchitecture()
        {
#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            return RuntimeInformation.ProcessArchitecture.ToString();
#else
            return IntPtr.Size == 8 ? "X64" : "X86";
#endif
        }

        private static string GetRuntimeIdentifier()
        {
#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            return RuntimeInformation.RuntimeIdentifier;
#else
            return string.Empty;
#endif
        }
    }
}
