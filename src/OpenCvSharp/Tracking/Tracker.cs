using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Tracking
{
    /// <summary>
    /// Base wrapper for modern OpenCV tracking trackers.
    /// 现代 OpenCV tracking 跟踪器基类包装。
    /// </summary>
    public class Tracker : IDisposable
    {
        private NativeTrackingTrackerHandle handle;
        private bool disposed;

        internal Tracker(IntPtr nativeHandle)
        {
            handle = NativeTrackingTrackerHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>
        /// Initializes the tracker with an image and bounding box.
        /// 使用图像和边界框初始化跟踪器。
        /// </summary>
        public void Init(Mat image, Rect boundingBox)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.TrackingTrackerInit(NativeHandle, image.NativeHandle, ToNative(boundingBox)));
        }

        /// <summary>
        /// Updates the tracker and returns the new bounding box.
        /// 更新跟踪器并返回新的边界框。
        /// </summary>
        public bool Update(Mat image, ref Rect boundingBox)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeMethods.TrackingRectNative nativeBox = ToNative(boundingBox);
            NativeException.ThrowIfError(NativeMethods.TrackingTrackerUpdate(NativeHandle, image.NativeHandle, ref nativeBox, out int result));
            boundingBox = FromNative(nativeBox);
            return result != 0;
        }

        /// <summary>
        /// Updates the tracker and returns a result object.
        /// 更新跟踪器并返回结果对象。
        /// </summary>
        public TrackerUpdateResult Update(Mat image, Rect boundingBox)
        {
            bool success = Update(image, ref boundingBox);
            return new TrackerUpdateResult(success, boundingBox);
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        internal static NativeMethods.TrackingRectNative ToNative(Rect rect)
        {
            return new NativeMethods.TrackingRectNative
            {
                X = rect.X,
                Y = rect.Y,
                Width = rect.Width,
                Height = rect.Height
            };
        }

        internal static Rect FromNative(NativeMethods.TrackingRectNative rect)
        {
            return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        /// <summary>Throws when disposed. 已释放时抛出异常。</summary>
        protected void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
