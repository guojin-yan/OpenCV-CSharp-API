namespace OpenCvSharp
{
    /// <summary>
    /// Provides build and package information for OpenCV CSharp API.
    /// 提供 OpenCV CSharp API 的构建信息和包信息。
    /// </summary>
    public static class OpenCvSharpBuildInfo
    {
        /// <summary>
        /// Gets the managed package identifier.
        /// 获取 managed 主包标识。
        /// </summary>
        public const string ManagedPackageId = "JYPPX.OpenCV.CSharp.API";

        /// <summary>
        /// Gets the runtime package identifier prefix; the RID is appended by runtime packages.
        /// 获取 runtime 包标识前缀；runtime 包会追加 RID。
        /// </summary>
        public const string RuntimePackageIdPrefix = "JYPPX.OpenCV.runtime";

        /// <summary>
        /// Gets the OpenCV version targeted by this package.
        /// 获取当前包适配的 OpenCV 版本。
        /// </summary>
        public const string OpenCvVersion = "5.0.0";

        /// <summary>
        /// Gets the managed package version metadata for the selected OpenCV runtime identity.
        /// The version carries the OpenCV runtime identity and package revision while package identifiers remain version-neutral.
        /// 获取当前 OpenCV runtime 身份的 managed 包版本元数据。
        /// 版本承载 OpenCV runtime 身份和包修订号，包标识保持版本中性。
        /// </summary>
        public const string PackageVersion = "5.0.0.0";

        /// <summary>
        /// Gets the version-neutral primary native library name used by the current managed package.
        /// 获取当前 managed 包使用的版本中立主 native 库名称。
        /// </summary>
        public const string CurrentNativeLibraryName = "JYPPX.OpenCV.Native";

        /// <summary>
        /// Gets the compatibility native library copy name retained for earlier fixed-major consumers.
        /// 获取为早期固定大版本消费者保留的 compatibility native 库副本名称。
        /// </summary>
        public const string LegacyNativeLibraryName = "OpenCv5Sharp.Native";

        /// <summary>
        /// Gets the existing-caller build-info property value.
        /// New code should prefer <see cref="CurrentNativeLibraryName"/>.
        /// 获取既有调用方 build-info 属性值。
        /// 新增代码应优先使用 <see cref="CurrentNativeLibraryName"/>。
        /// </summary>
        public const string NativeLibraryName = LegacyNativeLibraryName;

        /// <summary>
        /// Gets the target framework used by the current assembly build.
        /// 获取当前程序集构建所使用的目标框架。
        /// </summary>
        public static string TargetFramework
        {
            get
            {
#if NET10_0
                return "net10.0";
#elif NET9_0
                return "net9.0";
#elif NET8_0
                return "net8.0";
#elif NET7_0
                return "net7.0";
#elif NET6_0
                return "net6.0";
#elif NET5_0
                return "net5.0";
#elif NETCOREAPP3_1
                return "netcoreapp3.1";
#elif NET481
                return "net481";
#elif NET48
                return "net48";
#elif NET472
                return "net472";
#elif NET471
                return "net471";
#elif NET47
                return "net47";
#elif NET462
                return "net462";
#elif NET461
                return "net461";
#elif NET46
                return "net46";
#else
                return "unknown";
#endif
            }
        }

        /// <summary>
        /// Gets a display string containing package and OpenCV version information.
        /// 获取包含包版本和 OpenCV 版本的显示字符串。
        /// </summary>
        /// <returns>A human-readable version string. 人类可读的版本字符串。</returns>
        public static string GetDisplayString()
        {
            return "OpenCV CSharp API " + PackageVersion + " for OpenCV " + OpenCvVersion + " (" + TargetFramework + ")";
        }
    }

    /// <summary>
    /// Preserves the OpenCv5Sharp build-info facade only for existing callers.
    /// 仅为既有 OpenCv5Sharp 构建信息调用方保留 build-info facade。
    /// </summary>
    public static class OpenCv5SharpBuildInfo // Compatibility facade for existing callers.
    {
        /// <summary>
        /// Gets the managed package identifier.
        /// 获取 managed 主包标识。
        /// </summary>
        public const string ManagedPackageId = OpenCvSharpBuildInfo.ManagedPackageId;

        /// <summary>
        /// Gets the runtime package identifier prefix; the RID is appended by runtime packages.
        /// 获取 runtime 包标识前缀；runtime 包会追加 RID。
        /// </summary>
        public const string RuntimePackageIdPrefix = OpenCvSharpBuildInfo.RuntimePackageIdPrefix;

        /// <summary>
        /// Gets the OpenCV version targeted by this package.
        /// 获取当前包适配的 OpenCV 版本。
        /// </summary>
        public const string OpenCvVersion = OpenCvSharpBuildInfo.OpenCvVersion;

        /// <summary>
        /// Gets package version metadata through the existing-caller facade.
        /// The forwarded value preserves the OpenCV runtime identity and package revision without changing version-neutral package identifiers.
        /// 通过既有调用方 facade 获取当前 build-info 包版本元数据。
        /// 转发值保留 OpenCV runtime 身份和包修订号，不改变版本中性的包标识。
        /// </summary>
        public const string PackageVersion = OpenCvSharpBuildInfo.PackageVersion;

        /// <summary>
        /// Gets the existing-caller facade view of the current version-neutral native library name.
        /// The value is forwarded from the current build-info surface; legacy loader compatibility remains exposed through <see cref="LegacyNativeLibraryName"/> and <see cref="NativeLibraryName"/>.
        /// 获取当前版本中立 native 库名称的既有调用方 facade 视图。
        /// 该值转发自当前 build-info 接口面；legacy loader 兼容性仍通过 <see cref="LegacyNativeLibraryName"/> 和 <see cref="NativeLibraryName"/> 暴露。
        /// </summary>
        public const string CurrentNativeLibraryName = OpenCvSharpBuildInfo.CurrentNativeLibraryName;

        /// <summary>
        /// Gets the compatibility native library copy name retained for earlier fixed-major consumers.
        /// 获取为早期固定大版本消费者保留的 compatibility native 库副本名称。
        /// </summary>
        public const string LegacyNativeLibraryName = OpenCvSharpBuildInfo.LegacyNativeLibraryName;

        /// <summary>
        /// Gets the existing-caller build-info property value.
        /// New code should prefer <see cref="CurrentNativeLibraryName"/>.
        /// 获取既有调用方 build-info 属性值。
        /// 新增代码应优先使用 <see cref="CurrentNativeLibraryName"/>。
        /// </summary>
        public const string NativeLibraryName = OpenCvSharpBuildInfo.NativeLibraryName;

        /// <summary>
        /// Gets the target framework used by the current assembly build.
        /// 获取当前程序集构建所使用的目标框架。
        /// </summary>
        public static string TargetFramework
        {
            get { return OpenCvSharpBuildInfo.TargetFramework; }
        }

        /// <summary>
        /// Gets a display string containing package and OpenCV version information.
        /// 获取包含包版本和 OpenCV 版本的显示字符串。
        /// </summary>
        /// <returns>A human-readable version string. 人类可读的版本字符串。</returns>
        public static string GetDisplayString()
        {
            return OpenCvSharpBuildInfo.GetDisplayString();
        }
    }
}
