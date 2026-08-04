#if !NET7_0_OR_GREATER
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

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_named_window")]
        internal static extern int HighGuiNamedWindow(byte[] winname, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_destroy_window")]
        internal static extern int HighGuiDestroyWindow(byte[] winname);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_destroy_all_windows")]
        internal static extern int HighGuiDestroyAllWindows();

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_current_ui_framework_length")]
        internal static extern int HighGuiCurrentUiFrameworkLength(out int byteLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_current_ui_framework_fill")]
        internal static extern int HighGuiCurrentUiFrameworkFill(byte[] buffer, int bufferCapacity, out int written);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_start_window_thread")]
        internal static extern int HighGuiStartWindowThread(out int result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_imshow")]
        internal static extern int HighGuiImShow(byte[] winname, IntPtr mat);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_wait_key")]
        internal static extern int HighGuiWaitKey(int delay, out int key);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_wait_key_ex")]
        internal static extern int HighGuiWaitKeyEx(int delay, out int key);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_poll_key")]
        internal static extern int HighGuiPollKey(out int key);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_move_window")]
        internal static extern int HighGuiMoveWindow(byte[] winname, int x, int y);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_resize_window")]
        internal static extern int HighGuiResizeWindow(byte[] winname, int width, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_window_property")]
        internal static extern int HighGuiSetWindowProperty(byte[] winname, int propId, double propValue);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_get_window_property")]
        internal static extern int HighGuiGetWindowProperty(byte[] winname, int propId, out double propValue);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_window_title")]
        internal static extern int HighGuiSetWindowTitle(byte[] winname, byte[] title);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_get_window_image_rect")]
        internal static extern int HighGuiGetWindowImageRect(byte[] winname, out HighGuiRectNative rect);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_create_trackbar")]
        internal static extern int HighGuiCreateTrackbar(byte[] trackbarname, byte[] winname, int initialValue, int count, HighGuiTrackbarCallback? callback, IntPtr userdata, out IntPtr trackbar);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_create_trackbar_utf8")]
        internal static extern int HighGuiCreateTrackbarUtf8(byte[] trackbarName, int trackbarNameLength, byte[] windowName, int windowNameLength, int initialValue, int count, HighGuiTrackbarCallback? callback, IntPtr userdata, out IntPtr trackbar);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_trackbar_release_handle")]
        internal static extern void HighGuiTrackbarReleaseHandle(IntPtr trackbar);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_get_trackbar_pos")]
        internal static extern int HighGuiGetTrackbarPos(byte[] trackbarname, byte[] winname, out int pos);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_trackbar_pos")]
        internal static extern int HighGuiSetTrackbarPos(byte[] trackbarname, byte[] winname, int pos);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_trackbar_min")]
        internal static extern int HighGuiSetTrackbarMin(byte[] trackbarname, byte[] winname, int minval);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_trackbar_max")]
        internal static extern int HighGuiSetTrackbarMax(byte[] trackbarname, byte[] winname, int maxval);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_set_mouse_callback")]
        internal static extern int HighGuiSetMouseCallback(byte[] winname, HighGuiMouseCallback? callback, IntPtr userdata);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_mouse_callback_create_utf8")]
        internal static extern int HighGuiMouseCallbackCreateUtf8(byte[] windowName, int windowNameLength, HighGuiMouseCallback callback, IntPtr userdata, out IntPtr registration);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_mouse_callback_clear_utf8")]
        internal static extern int HighGuiMouseCallbackClearUtf8(byte[] windowName, int windowNameLength);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_create_button")]
        internal static extern int HighGuiCreateButton(byte[] buttonName, HighGuiButtonCallback? callback, IntPtr userdata, int type, int initialButtonState);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_button_callback_create_utf8")]
        internal static extern int HighGuiButtonCallbackCreateUtf8(byte[] buttonName, int buttonNameLength, HighGuiButtonCallback callback, IntPtr userdata, int type, int initialButtonState, out IntPtr registration);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_callback_registration_release_handle")]
        internal static extern void HighGuiCallbackRegistrationReleaseHandle(IntPtr registration);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_highgui_get_mouse_wheel_delta")]
        internal static extern int HighGuiGetMouseWheelDelta(int flags, out int delta);
    }
}
#endif
