using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static class NativeException
    {
        internal static void ThrowIfError(int status)
        {
            if (status == NativeStatus.Ok)
            {
                return;
            }

            string message = GetLastErrorMessage();
            if (string.IsNullOrEmpty(message))
            {
                message = "Native OpenCV call failed with status " + status + ".";
            }

            if (status == NativeStatus.NotLinked &&
                message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) < 0)
            {
                message = "NOT_LINKED: " + message;
            }

            throw new OpenCvException(message);
        }

        internal static string GetLastErrorMessage()
        {
            IntPtr pointer = NativeMethods.GetLastErrorPointer();
            if (pointer == IntPtr.Zero)
            {
                return string.Empty;
            }

#if NETCOREAPP3_1_OR_GREATER
            return Marshal.PtrToStringUTF8(pointer) ?? string.Empty;
#else
            return Marshal.PtrToStringAnsi(pointer) ?? string.Empty;
#endif
        }
    }
}
