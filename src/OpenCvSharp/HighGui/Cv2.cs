using System;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.HighGui
{
    /// <summary>
    /// OpenCV HighGUI window and key helpers.
    /// OpenCV HighGUI 窗口和按键辅助函数。
    /// </summary>
    public static class Cv2
    {
        /// <summary>
        /// Managed callback invoked when a HighGUI trackbar changes.
        /// HighGUI trackbar 变化时调用的 managed 回调。
        /// </summary>
        public delegate void TrackbarCallback(int position);

        /// <summary>
        /// Managed callback invoked for HighGUI mouse events.
        /// HighGUI 鼠标事件触发时调用的 managed 回调。
        /// </summary>
        public delegate void MouseCallback(MouseEventTypes @event, int x, int y, MouseEventFlags flags);

        /// <summary>
        /// Managed callback invoked for HighGUI Qt button events.
        /// HighGUI Qt 按钮事件触发时调用的 managed 回调。
        /// </summary>
        public delegate void ButtonCallback(int state);

        private static NativeMethods.HighGuiMouseCallback? currentMouseCallback;
        private static GCHandle? currentMouseCallbackHandle;
        private static NativeMethods.HighGuiButtonCallback? currentButtonCallback;
        private static GCHandle? currentButtonCallbackHandle;

        /// <summary>
        /// Creates or reuses a named window.
        /// 创建或复用命名窗口。
        /// </summary>
        public static void NamedWindow(string winname, WindowFlags flags = WindowFlags.AutoSize)
        {
            byte[] nativeName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiNamedWindow(nativeName, (int)flags));
        }

        /// <summary>
        /// Destroys a named window.
        /// 销毁命名窗口。
        /// </summary>
        public static void DestroyWindow(string winname)
        {
            byte[] nativeName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiDestroyWindow(nativeName));
        }

        /// <summary>
        /// Destroys all HighGUI windows.
        /// 销毁所有 HighGUI 窗口。
        /// </summary>
        public static void DestroyAllWindows()
        {
            NativeException.ThrowIfError(NativeMethods.HighGuiDestroyAllWindows());
        }

        /// <summary>
        /// Displays an image in a named window.
        /// 在命名窗口中显示图像。
        /// </summary>
        public static void ImShow(string winname, Mat mat)
        {
            ValidateNotNull(mat, nameof(mat));
            byte[] nativeName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiImShow(nativeName, mat.NativeHandle));
        }

        /// <summary>
        /// Waits for a key event.
        /// 等待按键事件。
        /// </summary>
        public static int WaitKey(int delay = 0)
        {
            NativeException.ThrowIfError(NativeMethods.HighGuiWaitKey(delay, out int key));
            return key;
        }

        /// <summary>
        /// Polls for a key event without waiting.
        /// 非阻塞轮询按键事件。
        /// </summary>
        public static int PollKey()
        {
            NativeException.ThrowIfError(NativeMethods.HighGuiPollKey(out int key));
            return key;
        }

        /// <summary>
        /// Moves a named window.
        /// 移动命名窗口。
        /// </summary>
        public static void MoveWindow(string winname, int x, int y)
        {
            byte[] nativeName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiMoveWindow(nativeName, x, y));
        }

        /// <summary>
        /// Resizes a named window.
        /// 调整命名窗口大小。
        /// </summary>
        public static void ResizeWindow(string winname, int width, int height)
        {
            byte[] nativeName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiResizeWindow(nativeName, width, height));
        }

        /// <summary>
        /// Sets a property on a named window.
        /// 设置命名窗口属性。
        /// </summary>
        public static void SetWindowProperty(string winname, WindowPropertyFlags propId, double propValue)
        {
            byte[] nativeName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiSetWindowProperty(nativeName, (int)propId, propValue));
        }

        /// <summary>
        /// Gets a property from a named window.
        /// 获取命名窗口属性。
        /// </summary>
        public static double GetWindowProperty(string winname, WindowPropertyFlags propId)
        {
            byte[] nativeName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiGetWindowProperty(nativeName, (int)propId, out double value));
            return value;
        }

        /// <summary>
        /// Updates the title of a named window.
        /// 更新命名窗口标题。
        /// </summary>
        public static void SetWindowTitle(string winname, string title)
        {
            byte[] nativeName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            byte[] nativeTitle = HighGuiStringConvert.ToNullTerminatedUtf8(title, nameof(title));
            NativeException.ThrowIfError(NativeMethods.HighGuiSetWindowTitle(nativeName, nativeTitle));
        }

        /// <summary>
        /// Gets the image rendering rectangle of a named window.
        /// 获取命名窗口中的图像渲染区域。
        /// </summary>
        public static Rect GetWindowImageRect(string winname)
        {
            byte[] nativeName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiGetWindowImageRect(nativeName, out NativeMethods.HighGuiRectNative rect));
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Creates a trackbar and keeps its native callback state alive through the returned object.
        /// 创建 trackbar，并通过返回对象保持 native 回调状态存活。
        /// </summary>
        public static HighGuiTrackbar CreateTrackbar(string trackbarname, string winname, int initialValue, int count, TrackbarCallback? callback = null)
        {
            byte[] nativeTrackbarName = HighGuiStringConvert.ToNullTerminatedUtf8(trackbarname, nameof(trackbarname));
            byte[] nativeWindowName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeMethods.HighGuiTrackbarCallback? nativeCallback = callback == null
                ? null
                : (position, userdata) => callback(position);
            NativeException.ThrowIfError(NativeMethods.HighGuiCreateTrackbar(
                nativeTrackbarName,
                nativeWindowName,
                initialValue,
                count,
                nativeCallback,
                IntPtr.Zero,
                out IntPtr nativeHandle));
            return new HighGuiTrackbar(nativeHandle, nativeCallback);
        }

        /// <summary>
        /// Gets the current trackbar position.
        /// 获取当前 trackbar 位置。
        /// </summary>
        public static int GetTrackbarPos(string trackbarname, string winname)
        {
            byte[] nativeTrackbarName = HighGuiStringConvert.ToNullTerminatedUtf8(trackbarname, nameof(trackbarname));
            byte[] nativeWindowName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiGetTrackbarPos(nativeTrackbarName, nativeWindowName, out int position));
            return position;
        }

        /// <summary>
        /// Sets the current trackbar position.
        /// 设置当前 trackbar 位置。
        /// </summary>
        public static void SetTrackbarPos(string trackbarname, string winname, int pos)
        {
            byte[] nativeTrackbarName = HighGuiStringConvert.ToNullTerminatedUtf8(trackbarname, nameof(trackbarname));
            byte[] nativeWindowName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiSetTrackbarPos(nativeTrackbarName, nativeWindowName, pos));
        }

        /// <summary>
        /// Sets the minimum trackbar position.
        /// 设置 trackbar 最小位置。
        /// </summary>
        public static void SetTrackbarMin(string trackbarname, string winname, int minval)
        {
            byte[] nativeTrackbarName = HighGuiStringConvert.ToNullTerminatedUtf8(trackbarname, nameof(trackbarname));
            byte[] nativeWindowName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiSetTrackbarMin(nativeTrackbarName, nativeWindowName, minval));
        }

        /// <summary>
        /// Sets the maximum trackbar position.
        /// 设置 trackbar 最大位置。
        /// </summary>
        public static void SetTrackbarMax(string trackbarname, string winname, int maxval)
        {
            byte[] nativeTrackbarName = HighGuiStringConvert.ToNullTerminatedUtf8(trackbarname, nameof(trackbarname));
            byte[] nativeWindowName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            NativeException.ThrowIfError(NativeMethods.HighGuiSetTrackbarMax(nativeTrackbarName, nativeWindowName, maxval));
        }

        /// <summary>
        /// Registers a mouse callback for a named window.
        /// 为命名窗口注册鼠标回调。
        /// </summary>
        public static void SetMouseCallback(string winname, MouseCallback? callback)
        {
            byte[] nativeName = HighGuiStringConvert.ToNullTerminatedUtf8(winname, nameof(winname));
            ReleaseMouseCallback();
            if (callback == null)
            {
                NativeException.ThrowIfError(NativeMethods.HighGuiSetMouseCallback(nativeName, null, IntPtr.Zero));
                return;
            }

            currentMouseCallback = (eventId, x, y, flags, userdata) => callback((MouseEventTypes)eventId, x, y, (MouseEventFlags)flags);
            currentMouseCallbackHandle = GCHandle.Alloc(currentMouseCallback);
            NativeException.ThrowIfError(NativeMethods.HighGuiSetMouseCallback(nativeName, currentMouseCallback, IntPtr.Zero));
        }

        /// <summary>
        /// Creates a Qt HighGUI button.
        /// 创建 Qt HighGUI 按钮。
        /// </summary>
        public static void CreateButton(string buttonName, ButtonCallback? callback = null, QtButtonTypes type = QtButtonTypes.PushButton, bool initialButtonState = false)
        {
            byte[] nativeButtonName = HighGuiStringConvert.ToNullTerminatedUtf8(buttonName, nameof(buttonName));
            ReleaseButtonCallback();
            if (callback == null)
            {
                NativeException.ThrowIfError(NativeMethods.HighGuiCreateButton(nativeButtonName, null, IntPtr.Zero, (int)type, initialButtonState ? 1 : 0));
                return;
            }

            currentButtonCallback = (state, userdata) => callback(state);
            currentButtonCallbackHandle = GCHandle.Alloc(currentButtonCallback);
            NativeException.ThrowIfError(NativeMethods.HighGuiCreateButton(nativeButtonName, currentButtonCallback, IntPtr.Zero, (int)type, initialButtonState ? 1 : 0));
        }

        private static void ReleaseMouseCallback()
        {
            if (currentMouseCallbackHandle.HasValue)
            {
                currentMouseCallbackHandle.Value.Free();
                currentMouseCallbackHandle = null;
            }

            currentMouseCallback = null;
        }

        private static void ReleaseButtonCallback()
        {
            if (currentButtonCallbackHandle.HasValue)
            {
                currentButtonCallbackHandle.Value.Free();
                currentButtonCallbackHandle = null;
            }

            currentButtonCallback = null;
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
