using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Tracking.Legacy
{
    /// <summary>
    /// Base wrapper for OpenCV legacy tracking trackers.
    /// OpenCV legacy tracking 跟踪器基类包装。
    /// </summary>
    public class LegacyTracker : IDisposable
    {
        private NativeTrackingLegacyTrackerHandle handle;
        private bool disposed;

        internal LegacyTracker(IntPtr nativeHandle)
        {
            handle = NativeTrackingLegacyTrackerHandle.FromNativePointer(nativeHandle);
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
        /// Initializes the OpenCV legacy tracker with an image and double-precision bounding box.
        /// 使用图像和双精度边界框初始化 OpenCV legacy 跟踪器。
        /// </summary>
        public void Init(Mat image, Rect2d boundingBox)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerInit(NativeHandle, image.NativeHandle, ToNative(boundingBox)));
        }

        /// <summary>
        /// Updates the OpenCV legacy tracker and returns the updated bounding box.
        /// 更新 OpenCV legacy 跟踪器并返回更新后的边界框。
        /// </summary>
        public bool Update(Mat image, ref Rect2d boundingBox)
        {
            ThrowIfDisposed();
            ValidateNotNull(image, nameof(image));
            NativeMethods.TrackingRect2dNative nativeBox = ToNative(boundingBox);
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyTrackerUpdate(NativeHandle, image.NativeHandle, ref nativeBox, out int result));
            boundingBox = FromNative(nativeBox);
            return result != 0;
        }

        /// <summary>
        /// Updates the OpenCV legacy tracker and returns a result object.
        /// 更新 OpenCV legacy 跟踪器并返回结果对象。
        /// </summary>
        public LegacyTrackerUpdateResult Update(Mat image, Rect2d boundingBox)
        {
            bool success = Update(image, ref boundingBox);
            return new LegacyTrackerUpdateResult(success, boundingBox);
        }

        /// <summary>
        /// Creates a modern tracker adapter that retains this legacy tracker state.
        /// 创建保留此 legacy tracker 状态的 modern tracker 适配器。
        /// </summary>
        public JYPPX.OpenCvSharp.Tracking.Tracker Upgrade()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyUpgrade(NativeHandle, out IntPtr nativeHandle));
            return new JYPPX.OpenCvSharp.Tracking.Tracker(nativeHandle);
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

        internal static NativeMethods.TrackingRect2dNative ToNative(Rect2d rect)
        {
            return new NativeMethods.TrackingRect2dNative
            {
                X = rect.X,
                Y = rect.Y,
                Width = rect.Width,
                Height = rect.Height
            };
        }

        internal static Rect2d FromNative(NativeMethods.TrackingRect2dNative rect)
        {
            return new Rect2d(rect.X, rect.Y, rect.Width, rect.Height);
        }

        internal static Rect2d[] FromNative(NativeMethods.TrackingRect2dNative[] rects, int count)
        {
            int actualCount = Math.Max(0, Math.Min(count, rects.Length));
            var result = new Rect2d[actualCount];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = FromNative(rects[i]);
            }

            return result;
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
