using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp
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

        /// <summary>
        /// Gets the major version reported by the loaded native OpenCV runtime.
        /// 获取已加载 native OpenCV runtime 报告的主版本号。
        /// </summary>
        /// <returns>The native OpenCV major version. native OpenCV 主版本号。</returns>
        public static int GetNativeOpenCvVersionMajor()
        {
            return NativeMethods.GetVersionMajor();
        }

        /// <summary>
        /// Gets the minor version reported by the loaded native OpenCV runtime.
        /// 获取已加载 native OpenCV runtime 报告的次版本号。
        /// </summary>
        /// <returns>The native OpenCV minor version. native OpenCV 次版本号。</returns>
        public static int GetNativeOpenCvVersionMinor()
        {
            return NativeMethods.GetVersionMinor();
        }

        /// <summary>
        /// Gets the revision reported by the loaded native OpenCV runtime.
        /// 获取已加载 native OpenCV runtime 报告的修订版本号。
        /// </summary>
        /// <returns>The native OpenCV revision. native OpenCV 修订版本号。</returns>
        public static int GetNativeOpenCvVersionRevision()
        {
            return NativeMethods.GetVersionRevision();
        }

        /// <summary>
        /// Gets the version reported by the loaded native OpenCV runtime.
        /// 获取已加载 native OpenCV runtime 报告的版本。
        /// </summary>
        /// <returns>The native OpenCV version string. native OpenCV 版本字符串。</returns>
        /// <exception cref="OpenCvException">Thrown when native numeric and string version probes disagree. 当 native 数字版本与字符串版本探针不一致时抛出。</exception>
        public static string GetNativeOpenCvVersion()
        {
            int major = GetNativeOpenCvVersionMajor();
            int minor = GetNativeOpenCvVersionMinor();
            int revision = GetNativeOpenCvVersionRevision();
            string numericVersion = string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", major, minor, revision);
            IntPtr versionPointer = NativeMethods.GetVersionStringPointer();
            string? versionString = versionPointer == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(versionPointer);
            if (versionString == null || versionString.Length == 0 ||
                !versionString.StartsWith(numericVersion, StringComparison.Ordinal))
            {
                throw new OpenCvException("Native OpenCV version probes are inconsistent.");
            }

            return versionString;
        }

        /// <summary>
        /// Gets whether the loaded native OpenCV runtime exactly matches the managed package target version.
        /// 获取已加载 native OpenCV runtime 是否与 managed 包目标版本完全匹配。
        /// </summary>
        public static bool IsNativeOpenCvVersionCompatible()
        {
            return string.Equals(GetNativeOpenCvVersion(), OpenCvVersion, StringComparison.Ordinal);
        }

        /// <summary>
        /// Verifies that the loaded native OpenCV runtime exactly matches the managed package target version.
        /// 验证已加载 native OpenCV runtime 与 managed 包目标版本完全匹配。
        /// </summary>
        /// <exception cref="OpenCvException">Thrown when the loaded native runtime version does not match <see cref="OpenCvVersion"/>. 当已加载 native runtime 版本与 <see cref="OpenCvVersion"/> 不一致时抛出。</exception>
        public static void VerifyNativeOpenCvVersionCompatibility()
        {
            string nativeVersion = GetNativeOpenCvVersion();
            if (!string.Equals(nativeVersion, OpenCvVersion, StringComparison.Ordinal))
            {
                throw new OpenCvException(
                    "Native OpenCV runtime version " + nativeVersion +
                    " does not match managed target version " + OpenCvVersion + ".");
            }
        }
    }
}
