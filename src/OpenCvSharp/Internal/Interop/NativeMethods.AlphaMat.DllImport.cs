#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_alphamat_info_flow")]
        internal static extern int AlphaMatInfoFlow(IntPtr image, IntPtr trimap, IntPtr result);
    }
}
#endif
