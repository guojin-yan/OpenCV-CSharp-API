#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        internal delegate void HighGuiMouseCallback(int @event, int x, int y, int flags, IntPtr userdata);

        internal delegate void HighGuiTrackbarCallback(int pos, IntPtr userdata);

        internal delegate void HighGuiButtonCallback(int state, IntPtr userdata);

        [StructLayout(LayoutKind.Sequential)]
        internal struct HighGuiRectNative
        {
            internal int X;
            internal int Y;
            internal int Width;
            internal int Height;
        }

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_named_window")]
        internal static partial int HighGuiNamedWindow(byte[] winname, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_destroy_window")]
        internal static partial int HighGuiDestroyWindow(byte[] winname);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_destroy_all_windows")]
        internal static partial int HighGuiDestroyAllWindows();

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_current_ui_framework_length")]
        internal static partial int HighGuiCurrentUiFrameworkLength(out int byteLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_current_ui_framework_fill")]
        internal static partial int HighGuiCurrentUiFrameworkFill(byte[] buffer, int bufferCapacity, out int written);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_start_window_thread")]
        internal static partial int HighGuiStartWindowThread(out int result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_imshow")]
        internal static partial int HighGuiImShow(byte[] winname, IntPtr mat);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_wait_key")]
        internal static partial int HighGuiWaitKey(int delay, out int key);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_wait_key_ex")]
        internal static partial int HighGuiWaitKeyEx(int delay, out int key);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_poll_key")]
        internal static partial int HighGuiPollKey(out int key);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_move_window")]
        internal static partial int HighGuiMoveWindow(byte[] winname, int x, int y);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_resize_window")]
        internal static partial int HighGuiResizeWindow(byte[] winname, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_window_property")]
        internal static partial int HighGuiSetWindowProperty(byte[] winname, int propId, double propValue);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_get_window_property")]
        internal static partial int HighGuiGetWindowProperty(byte[] winname, int propId, out double propValue);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_window_title")]
        internal static partial int HighGuiSetWindowTitle(byte[] winname, byte[] title);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_get_window_image_rect")]
        internal static partial int HighGuiGetWindowImageRect(byte[] winname, out HighGuiRectNative rect);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_create_trackbar")]
        internal static partial int HighGuiCreateTrackbar(byte[] trackbarname, byte[] winname, int initialValue, int count, HighGuiTrackbarCallback? callback, IntPtr userdata, out IntPtr trackbar);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_create_trackbar_utf8")]
        internal static partial int HighGuiCreateTrackbarUtf8(byte[] trackbarName, int trackbarNameLength, byte[] windowName, int windowNameLength, int initialValue, int count, HighGuiTrackbarCallback? callback, IntPtr userdata, out IntPtr trackbar);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_trackbar_release_handle")]
        internal static partial void HighGuiTrackbarReleaseHandle(IntPtr trackbar);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_get_trackbar_pos")]
        internal static partial int HighGuiGetTrackbarPos(byte[] trackbarname, byte[] winname, out int pos);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_trackbar_pos")]
        internal static partial int HighGuiSetTrackbarPos(byte[] trackbarname, byte[] winname, int pos);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_trackbar_min")]
        internal static partial int HighGuiSetTrackbarMin(byte[] trackbarname, byte[] winname, int minval);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_trackbar_max")]
        internal static partial int HighGuiSetTrackbarMax(byte[] trackbarname, byte[] winname, int maxval);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_mouse_callback")]
        internal static partial int HighGuiSetMouseCallback(byte[] winname, HighGuiMouseCallback? callback, IntPtr userdata);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_mouse_callback_create_utf8")]
        internal static partial int HighGuiMouseCallbackCreateUtf8(byte[] windowName, int windowNameLength, HighGuiMouseCallback callback, IntPtr userdata, out IntPtr registration);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_mouse_callback_clear_utf8")]
        internal static partial int HighGuiMouseCallbackClearUtf8(byte[] windowName, int windowNameLength);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_create_button")]
        internal static partial int HighGuiCreateButton(byte[] buttonName, HighGuiButtonCallback? callback, IntPtr userdata, int type, int initialButtonState);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_button_callback_create_utf8")]
        internal static partial int HighGuiButtonCallbackCreateUtf8(byte[] buttonName, int buttonNameLength, HighGuiButtonCallback callback, IntPtr userdata, int type, int initialButtonState, out IntPtr registration);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_callback_registration_release_handle")]
        internal static partial void HighGuiCallbackRegistrationReleaseHandle(IntPtr registration);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_get_mouse_wheel_delta")]
        internal static partial int HighGuiGetMouseWheelDelta(int flags, out int delta);
    }
}
#endif
