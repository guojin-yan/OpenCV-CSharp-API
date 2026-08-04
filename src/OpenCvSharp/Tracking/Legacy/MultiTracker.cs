using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Tracking.Legacy
{
    /// <summary>
    /// OpenCV legacy multi-object tracker wrapper.
    /// OpenCV legacy 多目标跟踪器包装。
    /// </summary>
    public sealed class MultiTracker : IDisposable
    {
        private NativeTrackingLegacyMultiTrackerHandle handle;
        private bool disposed;

        private MultiTracker(IntPtr nativeHandle)
        {
            handle = NativeTrackingLegacyMultiTrackerHandle.FromNativePointer(nativeHandle);
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

        /// <summary>Creates an OpenCV legacy MultiTracker. 创建 OpenCV legacy MultiTracker。</summary>
        public static MultiTracker Create()
        {
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyMultiTrackerCreate(out IntPtr nativeHandle));
            return new MultiTracker(nativeHandle);
        }

        /// <summary>
        /// Adds a tracker and its initial object box.
        /// 添加跟踪器和初始目标框。
        /// </summary>
        public bool Add(LegacyTracker tracker, Mat image, Rect2d boundingBox)
        {
            ThrowIfDisposed();
            LegacyTracker.ValidateNotNull(tracker, nameof(tracker));
            LegacyTracker.ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyMultiTrackerAdd(
                NativeHandle,
                tracker.NativeHandle,
                image.NativeHandle,
                LegacyTracker.ToNative(boundingBox),
                out int result));
            return result != 0;
        }

        /// <summary>
        /// Updates all trackers and returns their boxes.
        /// 更新所有跟踪器并返回边界框。
        /// </summary>
        public LegacyMultiTrackerUpdateResult Update(Mat image)
        {
            ThrowIfDisposed();
            LegacyTracker.ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyMultiTrackerUpdateCount(NativeHandle, image.NativeHandle, out int result, out int count));
            if (count <= 0)
            {
                return new LegacyMultiTrackerUpdateResult(result != 0, Array.Empty<Rect2d>());
            }

            var boxes = new NativeMethods.TrackingRect2dNative[count];
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyMultiTrackerUpdateFill(NativeHandle, image.NativeHandle, boxes, boxes.Length, out result, out int written));
            return new LegacyMultiTrackerUpdateResult(result != 0, LegacyTracker.FromNative(boxes, written));
        }

        /// <summary>
        /// Gets current object boxes without updating.
        /// 获取当前目标框，不执行更新。
        /// </summary>
        public Rect2d[] GetObjects()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyMultiTrackerGetObjectsCount(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<Rect2d>();
            }

            var boxes = new NativeMethods.TrackingRect2dNative[count];
            NativeException.ThrowIfError(NativeMethods.TrackingLegacyMultiTrackerGetObjectsFill(NativeHandle, boxes, boxes.Length, out int written));
            return LegacyTracker.FromNative(boxes, written);
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

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
