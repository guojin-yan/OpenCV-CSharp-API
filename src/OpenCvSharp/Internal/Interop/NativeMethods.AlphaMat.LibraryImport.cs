#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_alphamat_info_flow")]
        internal static partial int AlphaMatInfoFlow(IntPtr image, IntPtr trimap, IntPtr result);
    }
}
#endif
