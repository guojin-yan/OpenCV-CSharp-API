using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Video
{
    /// <summary>Base class for owned trackers from the main OpenCV Video module.</summary>
    public abstract class Tracker : IDisposable
    {
        private readonly NativeVideoTrackerHandle handle;
        private bool disposed;
        private bool initialized;

        /// <summary>Initializes an owned main Video tracker wrapper.</summary>
        protected Tracker(IntPtr nativeHandle)
        {
            handle = NativeVideoTrackerHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether native state has been released.</summary>
        public bool IsDisposed => disposed;

        /// <summary>Gets whether initialization completed successfully.</summary>
        public bool IsInitialized => initialized;

        /// <summary>Gets the tracker score reported by the upstream base implementation.</summary>
        public float TrackingScore
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.VideoTrackerGetTrackingScore(NativeHandle, out float score));
                return score;
            }
        }

        private IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Initializes or reinitializes the tracker with a known target rectangle.</summary>
        public void Init(Mat image, Rect boundingBox)
        {
            ThrowIfDisposed();
            ValidateFrame(image, nameof(image));
            ValidateBoundingBox(image, boundingBox, nameof(boundingBox));
            initialized = false;
            NativeException.ThrowIfError(NativeMethods.VideoTrackerInit(NativeHandle, image.NativeHandle, ToNative(boundingBox)));
            initialized = true;
        }

        /// <summary>Updates the tracker; a false result leaves the caller rectangle unchanged.</summary>
        public bool Update(Mat image, ref Rect boundingBox)
        {
            ThrowIfDisposed();
            if (!initialized)
            {
                throw new InvalidOperationException("The tracker must be initialized before Update.");
            }
            ValidateFrame(image, nameof(image));
            NativeMethods.VideoRectNative native = ToNative(boundingBox);
            NativeException.ThrowIfError(NativeMethods.VideoTrackerUpdate(NativeHandle, image.NativeHandle, ref native, out int result));
            if (result != 0)
            {
                boundingBox = FromNative(native);
                return true;
            }
            return false;
        }

        /// <summary>Releases the owned tracker. Repeated disposal is harmless.</summary>
        public void Dispose()
        {
            if (disposed)
            {
                return;
            }
            handle.Dispose();
            initialized = false;
            disposed = true;
            GC.SuppressFinalize(this);
        }

        private static void ValidateFrame(Mat image, string parameterName)
        {
            if (image == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            if (image.Empty || image.Dims != 2)
            {
                throw new ArgumentException("Tracker frames must be non-empty two-dimensional Mats.", parameterName);
            }
        }

        private static void ValidateBoundingBox(Mat image, Rect value, string parameterName)
        {
            long right = (long)value.X + value.Width;
            long bottom = (long)value.Y + value.Height;
            if (value.X < 0 || value.Y < 0 || value.Width <= 0 || value.Height <= 0 ||
                right > image.Cols || bottom > image.Rows)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "The target rectangle must be positive and fully contained in the initial frame.");
            }
        }

        private static NativeMethods.VideoRectNative ToNative(Rect value)
        {
            return new NativeMethods.VideoRectNative
            {
                X = value.X,
                Y = value.Y,
                Width = value.Width,
                Height = value.Height
            };
        }

        private static Rect FromNative(NativeMethods.VideoRectNative value)
        {
            return new Rect(value.X, value.Y, value.Width, value.Height);
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
