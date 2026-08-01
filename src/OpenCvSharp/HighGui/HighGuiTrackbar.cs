using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.HighGui
{
    /// <summary>
    /// Represents a HighGUI trackbar registration and keeps callback state alive.
    /// 表示一个 HighGUI trackbar 注册，并保持回调状态存活。
    /// </summary>
    public sealed class HighGuiTrackbar : IDisposable
    {
        private NativeHighGuiTrackbarHandle handle;
        private NativeMethods.HighGuiTrackbarCallback? nativeCallback;
        private bool disposed;

        internal HighGuiTrackbar(IntPtr nativeHandle, NativeMethods.HighGuiTrackbarCallback? callback)
        {
            handle = NativeHighGuiTrackbarHandle.FromNativePointer(nativeHandle);
            nativeCallback = callback;
        }

        /// <summary>
        /// Gets whether this trackbar registration has been disposed.
        /// 获取 trackbar 注册是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Releases native and callback resources.
        /// 释放 native 与回调资源。
        /// </summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                nativeCallback = null;
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }
    }
}
