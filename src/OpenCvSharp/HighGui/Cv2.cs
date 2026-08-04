using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.HighGui
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

        private static readonly object CallbackSync = new object();
        private static readonly Dictionary<string, CallbackRegistration> MouseRegistrations =
            new Dictionary<string, CallbackRegistration>(StringComparer.Ordinal);
        private static readonly List<CallbackRegistration> ButtonRegistrations = new List<CallbackRegistration>();
        private static readonly Queue<ExceptionDispatchInfo> PendingCallbackExceptions = new Queue<ExceptionDispatchInfo>();

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
            ReleaseMouseRegistration(winname);
        }

        /// <summary>
        /// Destroys all HighGUI windows.
        /// 销毁所有 HighGUI 窗口。
        /// </summary>
        public static void DestroyAllWindows()
        {
            NativeException.ThrowIfError(NativeMethods.HighGuiDestroyAllWindows());
            ReleaseAllCallbackRegistrations();
        }

        /// <summary>
        /// Gets the active HighGUI backend name, or an empty string when no UI backend is available.
        /// 获取当前 HighGUI backend 名称；没有可用 UI backend 时返回空字符串。
        /// </summary>
        public static string GetCurrentUIFramework()
        {
            NativeException.ThrowIfError(NativeMethods.HighGuiCurrentUiFrameworkLength(out int length));
            if (length < 0) throw new OpenCvException("Native HighGUI returned a negative backend-name length.");
            var buffer = new byte[length];
            NativeException.ThrowIfError(NativeMethods.HighGuiCurrentUiFrameworkFill(buffer, buffer.Length, out int written));
            if (written < 0 || written > buffer.Length)
                throw new OpenCvException("Native HighGUI returned an invalid backend-name length.");
            return HighGuiStringConvert.FromUtf8(buffer, written);
        }

        /// <summary>
        /// Starts the backend-specific HighGUI window thread when the active backend supports it.
        /// 在当前 backend 支持时启动其 HighGUI window thread。
        /// </summary>
        /// <remarks>Most backends return zero and continue to require <see cref="WaitKey(int)"/> or <see cref="PollKey"/> for event processing.</remarks>
        public static int StartWindowThread()
        {
            NativeException.ThrowIfError(NativeMethods.HighGuiStartWindowThread(out int result));
            return result;
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
        /// Waits for a key event and returns the backend-specific full key code.
        /// 等待按键事件并返回 backend-specific 完整键码。
        /// </summary>
        public static int WaitKeyEx(int delay = 0)
        {
            NativeException.ThrowIfError(NativeMethods.HighGuiWaitKeyEx(delay, out int key));
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
        /// Resizes a named window to the specified image-area size.
        /// 按指定图像区域尺寸调整命名窗口大小。
        /// </summary>
        public static void ResizeWindow(string winname, Size size)
        {
            ResizeWindow(winname, size.Width, size.Height);
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
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (initialValue < 0 || initialValue > count) throw new ArgumentOutOfRangeException(nameof(initialValue));
            byte[] nativeTrackbarName = HighGuiStringConvert.ToUtf8(trackbarname, nameof(trackbarname));
            byte[] nativeWindowName = HighGuiStringConvert.ToUtf8(winname, nameof(winname));
            NativeMethods.HighGuiTrackbarCallback? nativeCallback = callback == null
                ? null
                : (position, userdata) => InvokeCallbackSafely(() => callback(position));
            NativeException.ThrowIfError(NativeMethods.HighGuiCreateTrackbarUtf8(
                nativeTrackbarName,
                nativeTrackbarName.Length,
                nativeWindowName,
                nativeWindowName.Length,
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
            byte[] nativeName = HighGuiStringConvert.ToUtf8(winname, nameof(winname));
            if (callback == null)
            {
                NativeException.ThrowIfError(NativeMethods.HighGuiMouseCallbackClearUtf8(nativeName, nativeName.Length));
                ReleaseMouseRegistration(winname);
                return;
            }

            NativeMethods.HighGuiMouseCallback nativeCallback = (eventId, x, y, flags, userdata) =>
                InvokeCallbackSafely(() => callback((MouseEventTypes)eventId, x, y, (MouseEventFlags)flags));
            NativeException.ThrowIfError(NativeMethods.HighGuiMouseCallbackCreateUtf8(
                nativeName,
                nativeName.Length,
                nativeCallback,
                IntPtr.Zero,
                out IntPtr nativeHandle));
            CallbackRegistration registration;
            try
            {
                registration = new CallbackRegistration(nativeHandle, nativeCallback);
            }
            catch
            {
                NativeMethods.HighGuiCallbackRegistrationReleaseHandle(nativeHandle);
                throw;
            }

            CallbackRegistration? previous = null;
            lock (CallbackSync)
            {
                MouseRegistrations.TryGetValue(winname, out previous);
                MouseRegistrations[winname] = registration;
            }
            if (previous != null) previous.Dispose();
        }

        /// <summary>
        /// Creates a Qt HighGUI button.
        /// 创建 Qt HighGUI 按钮。
        /// </summary>
        public static void CreateButton(string buttonName, ButtonCallback? callback = null, QtButtonTypes type = QtButtonTypes.PushButton, bool initialButtonState = false)
        {
            byte[] nativeButtonName = HighGuiStringConvert.ToUtf8(buttonName, nameof(buttonName));
            if (callback == null)
            {
                byte[] terminatedName = HighGuiStringConvert.ToNullTerminatedUtf8(buttonName, nameof(buttonName));
                NativeException.ThrowIfError(NativeMethods.HighGuiCreateButton(terminatedName, null, IntPtr.Zero, (int)type, initialButtonState ? 1 : 0));
                return;
            }

            NativeMethods.HighGuiButtonCallback nativeCallback = (state, userdata) =>
                InvokeCallbackSafely(() => callback(state));
            NativeException.ThrowIfError(NativeMethods.HighGuiButtonCallbackCreateUtf8(
                nativeButtonName,
                nativeButtonName.Length,
                nativeCallback,
                IntPtr.Zero,
                (int)type,
                initialButtonState ? 1 : 0,
                out IntPtr nativeHandle));
            CallbackRegistration registration;
            try
            {
                registration = new CallbackRegistration(nativeHandle, nativeCallback);
            }
            catch
            {
                NativeMethods.HighGuiCallbackRegistrationReleaseHandle(nativeHandle);
                throw;
            }
            lock (CallbackSync)
            {
                ButtonRegistrations.Add(registration);
            }
        }

        /// <summary>
        /// Extracts the signed mouse-wheel delta from a HighGUI mouse callback flags value.
        /// 从 HighGUI mouse callback flags 中提取有符号滚轮增量。
        /// </summary>
        public static int GetMouseWheelDelta(MouseEventFlags flags)
        {
            NativeException.ThrowIfError(NativeMethods.HighGuiGetMouseWheelDelta((int)flags, out int delta));
            return delta;
        }

        /// <summary>
        /// Rethrows the oldest managed exception captured from a HighGUI callback, if one is pending.
        /// 若 HighGUI callback 捕获了 managed 异常，则重新抛出最早的一项。
        /// </summary>
        public static void ThrowPendingCallbackException()
        {
            ExceptionDispatchInfo? pending = null;
            lock (CallbackSync)
            {
                if (PendingCallbackExceptions.Count > 0)
                    pending = PendingCallbackExceptions.Dequeue();
            }
            if (pending != null) pending.Throw();
        }

        internal static void CaptureCallbackException(Exception exception)
        {
            if (exception == null) return;
            lock (CallbackSync)
            {
                PendingCallbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(exception));
            }
        }

        private static void InvokeCallbackSafely(Action callback)
        {
            try { callback(); }
            catch (Exception exception) { CaptureCallbackException(exception); }
        }

        private static void ReleaseMouseRegistration(string winname)
        {
            CallbackRegistration? registration = null;
            lock (CallbackSync)
            {
                if (MouseRegistrations.TryGetValue(winname, out registration))
                    MouseRegistrations.Remove(winname);
            }
            if (registration != null) registration.Dispose();
        }

        private static void ReleaseAllCallbackRegistrations()
        {
            var registrations = new List<CallbackRegistration>();
            lock (CallbackSync)
            {
                registrations.AddRange(MouseRegistrations.Values);
                registrations.AddRange(ButtonRegistrations);
                MouseRegistrations.Clear();
                ButtonRegistrations.Clear();
            }
            foreach (CallbackRegistration registration in registrations) registration.Dispose();
        }

        private sealed class CallbackRegistration : IDisposable
        {
            private readonly NativeHighGuiCallbackRegistrationHandle handle;
            private Delegate? callback;

            internal CallbackRegistration(IntPtr nativeHandle, Delegate callback)
            {
                handle = NativeHighGuiCallbackRegistrationHandle.FromNativePointer(nativeHandle);
                this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
            }

            public void Dispose()
            {
                handle.Dispose();
                callback = null;
            }
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
