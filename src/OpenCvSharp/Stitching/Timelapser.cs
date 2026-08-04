using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Built-in timelapse destination strategy.</summary>
    public enum TimelapserType
    {
        /// <summary>Uses the union of all image placements.</summary>
        AsIs = 0,
        /// <summary>Uses the common intersection of all image placements.</summary>
        Crop = 1
    }

    /// <summary>Owns an OpenCV timelapser and its initialized destination state.</summary>
    public class Timelapser : IDisposable
    {
        private NativeTimelapserHandle handle;
        private bool disposed;
        private bool initialized;

        internal Timelapser(IntPtr nativeHandle)
        {
            handle = NativeTimelapserHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this timelapser has been disposed.</summary>
        public bool IsDisposed => disposed;

        internal IntPtr NativeHandle
        {
            get { ThrowIfDisposed(); return handle.DangerousGetHandle(); }
        }

        /// <summary>Creates an as-is or common-intersection timelapser.</summary>
        public static Timelapser CreateDefault(TimelapserType type)
        {
            if (type < TimelapserType.AsIs || type > TimelapserType.Crop) throw new ArgumentOutOfRangeException(nameof(type));
            NativeException.ThrowIfError(NativeMethods.StitchingTimelapserCreateDefault((int)type, out IntPtr nativeHandle));
            return type == TimelapserType.Crop ? new TimelapserCrop(nativeHandle) : new Timelapser(nativeHandle);
        }

        /// <summary>Initializes destination storage from equal-length placements.</summary>
        public void Initialize(Point[] corners, Size[] sizes)
        {
            ThrowIfDisposed();
            StitchingDetailMarshal.GetPlacements(corners, sizes, out int[] x, out int[] y, out int[] widths, out int[] heights);
            initialized = false;
            NativeException.ThrowIfError(NativeMethods.StitchingTimelapserInitialize(
                NativeHandle, x, y, x.Length, widths, heights, widths.Length));
            initialized = true;
        }

        /// <summary>Processes one exact CV_16SC3 image at its panorama top-left position.</summary>
        public void Process(Mat image, Mat? mask, Point topLeft)
        {
            ThrowIfDisposed();
            if (!initialized) throw new InvalidOperationException("Initialize must succeed before Process.");
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (image.Empty || image.Dims != 2 || image.Type != MatType.CV_16SC3)
                throw new ArgumentException("The image must be a non-empty two-dimensional CV_16SC3 matrix.", nameof(image));
            if (mask != null && mask.Empty) throw new ArgumentException("A supplied mask must not be empty.", nameof(mask));
            NativeException.ThrowIfError(NativeMethods.StitchingTimelapserProcess(
                NativeHandle, image.NativeHandle, mask?.NativeHandle ?? IntPtr.Zero, topLeft.X, topLeft.Y));
            GC.KeepAlive(image); GC.KeepAlive(mask);
        }

        /// <summary>Returns an independently owned CPU copy of the destination.</summary>
        public Mat GetDestination()
        {
            ThrowIfDisposed();
            if (!initialized) throw new InvalidOperationException("Initialize must succeed before GetDestination.");
            var result = new Mat();
            try
            {
                NativeException.ThrowIfError(NativeMethods.StitchingTimelapserGetDst(NativeHandle, result.NativeHandle));
                return result;
            }
            catch
            {
                result.Dispose(); throw;
            }
        }

        /// <summary>Releases the owned native timelapser.</summary>
        public void Dispose()
        {
            if (disposed) return;
            handle.Dispose(); disposed = true; GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }

    /// <summary>Common-intersection timelapser returned by <see cref="Timelapser.CreateDefault"/>.</summary>
    public sealed class TimelapserCrop : Timelapser
    {
        internal TimelapserCrop(IntPtr nativeHandle) : base(nativeHandle) { }
    }
}
