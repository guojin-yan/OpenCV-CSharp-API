using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Base class for owned OpenCV stitching exposure compensators.</summary>
    public abstract class ExposureCompensator : IDisposable
    {
        private NativeExposureCompensatorHandle handle;
        private bool disposed;

        internal ExposureCompensator(IntPtr nativeHandle)
        {
            handle = NativeExposureCompensatorHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this compensator has been disposed.</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets whether a subsequent feed updates the current gains.</summary>
        public bool UpdateGain
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StitchingExposureGetUpdateGain(NativeHandle, out int value));
                return value != 0;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StitchingExposureSetUpdateGain(NativeHandle, value ? 1 : 0));
            }
        }

        /// <summary>Creates one of OpenCV's built-in compensators.</summary>
        public static ExposureCompensator CreateDefault(ExposureCompensatorType type)
        {
            ValidateType(type);
            NativeException.ThrowIfError(NativeMethods.StitchingExposureCreateDefault((int)type, out IntPtr nativeHandle));
            switch (type)
            {
                case ExposureCompensatorType.None: return new NoExposureCompensator(nativeHandle);
                case ExposureCompensatorType.Gain: return new GainCompensator(nativeHandle);
                case ExposureCompensatorType.GainBlocks: return new BlocksGainCompensator(nativeHandle);
                case ExposureCompensatorType.Channels: return new ChannelsCompensator(nativeHandle);
                case ExposureCompensatorType.ChannelsBlocks: return new BlocksChannelsCompensator(nativeHandle);
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        /// <summary>Estimates gains from equally sized corner, image, and mask collections.</summary>
        public void Feed(Point[] corners, Mat[] images, Mat[] masks)
        {
            ThrowIfDisposed();
            if (corners == null) throw new ArgumentNullException(nameof(corners));
            if (images == null) throw new ArgumentNullException(nameof(images));
            if (masks == null) throw new ArgumentNullException(nameof(masks));
            if (images.Length == 0) throw new ArgumentException("At least one image is required.", nameof(images));
            if (corners.Length != images.Length) throw new ArgumentException("Corner and image counts must match.", nameof(corners));
            if (masks.Length != images.Length) throw new ArgumentException("Mask and image counts must match.", nameof(masks));

            var cornerX = new int[corners.Length];
            var cornerY = new int[corners.Length];
            var imageHandles = new IntPtr[images.Length];
            var maskHandles = new IntPtr[masks.Length];
            for (int i = 0; i < images.Length; ++i)
            {
                Mat image = images[i] ?? throw new ArgumentNullException(nameof(images), "The image collection contains null.");
                Mat mask = masks[i] ?? throw new ArgumentNullException(nameof(masks), "The mask collection contains null.");
                ValidateImageAndMask(image, mask, nameof(images), nameof(masks));
                cornerX[i] = corners[i].X;
                cornerY[i] = corners[i].Y;
                imageHandles[i] = image.NativeHandle;
                maskHandles[i] = mask.NativeHandle;
            }

            NativeException.ThrowIfError(NativeMethods.StitchingExposureFeed(
                NativeHandle,
                cornerX,
                cornerY,
                corners.Length,
                imageHandles,
                imageHandles.Length,
                maskHandles,
                maskHandles.Length));
        }

        /// <summary>Applies the previously estimated gain to an image in place.</summary>
        public void Apply(int index, Point corner, Mat image, Mat mask)
        {
            ThrowIfDisposed();
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (mask == null) throw new ArgumentNullException(nameof(mask));
            ValidateImageAndMask(image, mask, nameof(image), nameof(mask));
            NativeException.ThrowIfError(NativeMethods.StitchingExposureApply(
                NativeHandle,
                index,
                corner.X,
                corner.Y,
                image.NativeHandle,
                mask.NativeHandle));
        }

        /// <summary>Returns independent matrices containing the current gains.</summary>
        public Mat[] GetMatGains()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitchingExposureGetMatGainsCount(NativeHandle, out int count));
            if (count == 0) return Array.Empty<Mat>();
            if (count < 0) throw new OpenCvException("Native exposure gain count is invalid.");

            var nativeHandles = new IntPtr[count];
            int status = NativeMethods.StitchingExposureGetMatGainsFill(NativeHandle, nativeHandles, nativeHandles.Length, out int returnedCount);
            if (status != 0 || returnedCount != count)
            {
                ReleaseNativeMats(nativeHandles);
                NativeException.ThrowIfError(status);
                throw new OpenCvException("Native exposure gain count changed while it was being copied.");
            }

            var result = new Mat[count];
            int created = 0;
            try
            {
                for (; created < result.Length; ++created)
                {
                    result[created] = new Mat(nativeHandles[created]);
                    nativeHandles[created] = IntPtr.Zero;
                }
                return result;
            }
            catch
            {
                for (int i = 0; i < created; ++i) result[i]?.Dispose();
                ReleaseNativeMats(nativeHandles);
                throw;
            }
        }

        /// <summary>Replaces the current gains, borrowing each matrix only for this call.</summary>
        public void SetMatGains(Mat[] gains)
        {
            ThrowIfDisposed();
            if (gains == null) throw new ArgumentNullException(nameof(gains));
            var handles = new IntPtr[gains.Length];
            for (int i = 0; i < gains.Length; ++i)
            {
                Mat gain = gains[i] ?? throw new ArgumentNullException(nameof(gains), "The gain collection contains null.");
                handles[i] = gain.NativeHandle;
            }
            NativeException.ThrowIfError(NativeMethods.StitchingExposureSetMatGains(NativeHandle, handles, handles.Length));
        }

        /// <summary>Releases the native compensator.</summary>
        public void Dispose()
        {
            if (disposed) return;
            handle.Dispose();
            disposed = true;
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Reads the feed count supported by a derived compensator.</summary>
        internal int GetNumberOfFeeds()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitchingExposureGetNumberOfFeeds(NativeHandle, out int value));
            return value;
        }

        /// <summary>Updates the feed count supported by a derived compensator.</summary>
        internal void SetNumberOfFeeds(int value)
        {
            ThrowIfDisposed();
            if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            NativeException.ThrowIfError(NativeMethods.StitchingExposureSetNumberOfFeeds(NativeHandle, value));
        }

        /// <summary>Reads the similarity threshold supported by a derived compensator.</summary>
        internal double GetSimilarityThreshold()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitchingExposureGetSimilarityThreshold(NativeHandle, out double value));
            return value;
        }

        /// <summary>Updates the similarity threshold supported by a derived compensator.</summary>
        internal void SetSimilarityThreshold(double value)
        {
            ThrowIfDisposed();
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentOutOfRangeException(nameof(value));
            NativeException.ThrowIfError(NativeMethods.StitchingExposureSetSimilarityThreshold(NativeHandle, value));
        }

        /// <summary>Reads the block size supported by a block compensator.</summary>
        internal Size GetBlockSize()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitchingExposureGetBlockSize(NativeHandle, out int width, out int height));
            return new Size(width, height);
        }

        /// <summary>Updates the block size supported by a block compensator.</summary>
        internal void SetBlockSize(Size value)
        {
            ThrowIfDisposed();
            if (value.Width <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Block width must be positive.");
            if (value.Height <= 0) throw new ArgumentOutOfRangeException(nameof(value), "Block height must be positive.");
            NativeException.ThrowIfError(NativeMethods.StitchingExposureSetBlockSize(NativeHandle, value.Width, value.Height));
        }

        /// <summary>Reads the filtering iteration count supported by a block compensator.</summary>
        internal int GetFilteringIterations()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitchingExposureGetFilteringIterations(NativeHandle, out int value));
            return value;
        }

        /// <summary>Updates the filtering iteration count supported by a block compensator.</summary>
        internal void SetFilteringIterations(int value)
        {
            ThrowIfDisposed();
            if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            NativeException.ThrowIfError(NativeMethods.StitchingExposureSetFilteringIterations(NativeHandle, value));
        }

        /// <summary>Throws when the native compensator has been released.</summary>
        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        internal static void ValidateNumberOfFeeds(int numberOfFeeds)
        {
            if (numberOfFeeds <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfFeeds));
        }

        internal static void ValidateBlockArguments(int blockWidth, int blockHeight, int numberOfFeeds)
        {
            if (blockWidth <= 0) throw new ArgumentOutOfRangeException(nameof(blockWidth));
            if (blockHeight <= 0) throw new ArgumentOutOfRangeException(nameof(blockHeight));
            ValidateNumberOfFeeds(numberOfFeeds);
        }

        private static void ValidateType(ExposureCompensatorType type)
        {
            if (type < ExposureCompensatorType.None || type > ExposureCompensatorType.ChannelsBlocks)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private static void ValidateImageAndMask(Mat image, Mat mask, string imageName, string maskName)
        {
            if (image.Empty) throw new ArgumentException("The image must not be empty.", imageName);
            if (mask.Empty) throw new ArgumentException("The mask must not be empty.", maskName);
            if (image.Rows != mask.Rows || image.Cols != mask.Cols)
            {
                throw new ArgumentException("The mask size must match the image size.", maskName);
            }
            if (mask.Type != MatType.CV_8UC1)
            {
                throw new ArgumentException("The mask must have type CV_8UC1.", maskName);
            }
        }

        private static void ReleaseNativeMats(IntPtr[] handles)
        {
            for (int i = 0; i < handles.Length; ++i)
            {
                if (handles[i] != IntPtr.Zero) NativeMethods.MatRelease(handles[i]);
            }
        }
    }
}
