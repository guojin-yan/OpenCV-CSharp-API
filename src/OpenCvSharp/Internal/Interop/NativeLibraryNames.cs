namespace OpenCvSharp.Internal.Interop
{
    internal static class NativeLibraryNames
    {
        // CurrentNativeLibrary is the version-neutral primary loader used by current interop declarations.
        internal const string CurrentNativeLibrary = "JYPPX.OpenCV.Native";

        // LegacyNativeLibrary is exposed only as the compatibility loader copy for earlier fixed-major consumers.
        internal const string LegacyNativeLibrary = "OpenCv5Sharp.Native";

    }
}
