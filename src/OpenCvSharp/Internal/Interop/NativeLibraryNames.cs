namespace OpenCvSharp.Internal.Interop
{
    internal static class NativeLibraryNames
    {
        // CurrentNativeLibrary is the version-neutral primary loader used by current interop declarations.
        internal const string CurrentNativeLibrary = "JYPPX.OpenCV.Native";

        // LegacyNativeLibrary is the compatibility loader name for assemblies compiled against previous package versions.
        internal const string LegacyNativeLibrary = "OpenCvSharp.Native";

    }
}
