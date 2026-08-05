using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp
{
    /// <summary>
    /// Provides managed package, native ABI, and OpenCV runtime diagnostics.
    /// </summary>
    public static class OpenCvSharpBuildInfo
    {
        private const string NuGetPackageVersionMetadataName = "NuGetPackageVersion";
        private const string NativeAbiVersionMetadataName = "NativeAbiVersion";

        static OpenCvSharpBuildInfo()
        {
            string embeddedAbiVersion = GetRequiredAssemblyMetadata(NativeAbiVersionMetadataName);
            if (!string.Equals(
                    embeddedAbiVersion,
                    NativeAbiVersion.ToString(CultureInfo.InvariantCulture),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Managed native ABI metadata is inconsistent.");
            }
        }

        /// <summary>Gets the managed package identifier.</summary>
        public const string ManagedPackageId = "JYPPX.OpenCV.CSharp.API";

        /// <summary>Gets the runtime package identifier prefix.</summary>
        public const string RuntimePackageIdPrefix = "JYPPX.OpenCV.runtime";

        /// <summary>Gets the OpenCV version targeted by this managed package.</summary>
        public const string OpenCvVersion = "5.0.0";

        /// <summary>
        /// Gets the native wrapper ABI version required by this managed package.
        /// </summary>
        public const int NativeAbiVersion = 1;

        /// <summary>
        /// Gets the exact normalized NuGet version embedded when this assembly was packed.
        /// </summary>
        public static string NuGetPackageVersion { get; } = GetRequiredAssemblyMetadata(NuGetPackageVersionMetadataName);

        /// <summary>
        /// Gets the exact normalized NuGet version. Use <see cref="NuGetPackageVersion"/> in new diagnostics.
        /// </summary>
        public static string PackageVersion
        {
            get { return NuGetPackageVersion; }
        }

        /// <summary>Gets the version-neutral native loader name.</summary>
        public const string CurrentNativeLibraryName = "JYPPX.OpenCV.Native";

        /// <summary>Gets the target framework used by the current assembly build.</summary>
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

        /// <summary>Gets managed package and target runtime information without loading native code.</summary>
        public static string GetDisplayString()
        {
            return ManagedPackageId + " " + NuGetPackageVersion +
                " for OpenCV " + OpenCvVersion +
                " (native ABI " + NativeAbiVersion.ToString(CultureInfo.InvariantCulture) +
                ", " + TargetFramework + ")";
        }

        /// <summary>Gets diagnostics for both the managed package and the loaded native runtime.</summary>
        public static string GetRuntimeDiagnosticString()
        {
            return GetDisplayString() +
                "; loaded OpenCV " + GetNativeOpenCvVersion() +
                "; loaded native ABI " + GetLoadedNativeAbiVersion().ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Gets the ABI version reported by the loaded native wrapper.</summary>
        public static int GetLoadedNativeAbiVersion()
        {
            try
            {
                return NativeMethods.GetNativeAbiVersion();
            }
            catch (EntryPointNotFoundException exception)
            {
                throw new OpenCvException(
                    "The loaded native runtime does not expose the required ABI version probe. " +
                    "Install a runtime package with the same NuGet version as " + ManagedPackageId + " " +
                    NuGetPackageVersion + ".",
                    exception);
            }
        }

        /// <summary>Gets whether the loaded native wrapper ABI exactly matches this managed package.</summary>
        public static bool IsNativeAbiCompatible()
        {
            return GetLoadedNativeAbiVersion() == NativeAbiVersion;
        }

        /// <summary>Verifies that the loaded native wrapper ABI exactly matches this managed package.</summary>
        public static void VerifyNativeAbiCompatibility()
        {
            int loadedVersion = GetLoadedNativeAbiVersion();
            if (loadedVersion != NativeAbiVersion)
            {
                throw new OpenCvException(
                    "Loaded native ABI version " + loadedVersion.ToString(CultureInfo.InvariantCulture) +
                    " does not match managed ABI version " + NativeAbiVersion.ToString(CultureInfo.InvariantCulture) +
                    " from NuGet package " + NuGetPackageVersion + ".");
            }
        }

        /// <summary>Gets the major version reported by the loaded native OpenCV runtime.</summary>
        public static int GetNativeOpenCvVersionMajor()
        {
            return NativeMethods.GetVersionMajor();
        }

        /// <summary>Gets the minor version reported by the loaded native OpenCV runtime.</summary>
        public static int GetNativeOpenCvVersionMinor()
        {
            return NativeMethods.GetVersionMinor();
        }

        /// <summary>Gets the revision reported by the loaded native OpenCV runtime.</summary>
        public static int GetNativeOpenCvVersionRevision()
        {
            return NativeMethods.GetVersionRevision();
        }

        /// <summary>Gets the version string reported by the loaded native OpenCV runtime.</summary>
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

        /// <summary>Gets whether the loaded OpenCV version exactly matches this managed package.</summary>
        public static bool IsNativeOpenCvVersionCompatible()
        {
            return string.Equals(GetNativeOpenCvVersion(), OpenCvVersion, StringComparison.Ordinal);
        }

        /// <summary>Verifies that the loaded OpenCV version exactly matches this managed package.</summary>
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

        /// <summary>Verifies both the native wrapper ABI and OpenCV runtime version.</summary>
        public static void VerifyNativeRuntimeCompatibility()
        {
            VerifyNativeAbiCompatibility();
            VerifyNativeOpenCvVersionCompatibility();
        }

        private static string GetRequiredAssemblyMetadata(string name)
        {
            object[] attributes = typeof(OpenCvSharpBuildInfo).Assembly.GetCustomAttributes(
                typeof(AssemblyMetadataAttribute),
                false);
            foreach (object attribute in attributes)
            {
                AssemblyMetadataAttribute metadata = (AssemblyMetadataAttribute)attribute;
                if (string.Equals(metadata.Key, name, StringComparison.Ordinal) &&
                    !string.IsNullOrWhiteSpace(metadata.Value))
                {
                    return metadata.Value;
                }
            }

            throw new InvalidOperationException("Required assembly metadata is missing: " + name + ".");
        }
    }
}
