using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Owns an OpenCV stitching blender and its prepare/feed/blend state.</summary>
    public class Blender : IDisposable
    {
        private readonly BlenderType blenderType;
        private NativeBlenderHandle handle;
        private bool disposed;
        private bool prepared;
        private Rect preparedRoi;

        internal Blender(IntPtr nativeHandle, BlenderType blenderType)
        {
            handle = NativeBlenderHandle.FromNativePointer(nativeHandle);
            this.blenderType = blenderType;
        }

        /// <summary>Gets whether this blender has been disposed.</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Creates one of OpenCV's built-in blender strategies.</summary>
        public static Blender CreateDefault(BlenderType type, bool tryGpu = false)
        {
            ValidateType(type);
            NativeException.ThrowIfError(NativeMethods.StitchingBlenderCreateDefault((int)type, tryGpu ? 1 : 0, out IntPtr nativeHandle));
            switch (type)
            {
                case BlenderType.None: return new Blender(nativeHandle, type);
                case BlenderType.Feather: return new FeatherBlender(nativeHandle);
                case BlenderType.MultiBand: return new MultiBandBlender(nativeHandle);
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        /// <summary>Prepares a destination ROI derived from equal-length corner and size collections.</summary>
        public void Prepare(Point[] corners, Size[] sizes)
        {
            ThrowIfDisposed();
            if (corners == null) throw new ArgumentNullException(nameof(corners));
            if (sizes == null) throw new ArgumentNullException(nameof(sizes));
            if (corners.Length == 0) throw new ArgumentException("At least one image placement is required.", nameof(corners));
            if (corners.Length != sizes.Length) throw new ArgumentException("Corner and size counts must match.", nameof(sizes));

            var cornerX = new int[corners.Length];
            var cornerY = new int[corners.Length];
            var widths = new int[sizes.Length];
            var heights = new int[sizes.Length];
            long left = int.MaxValue;
            long top = int.MaxValue;
            long right = int.MinValue;
            long bottom = int.MinValue;
            for (int i = 0; i < corners.Length; ++i)
            {
                Size size = sizes[i];
                if (size.Width <= 0 || size.Height <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sizes), "Every width and height must be positive.");
                }

                long itemRight = (long)corners[i].X + size.Width;
                long itemBottom = (long)corners[i].Y + size.Height;
                if (itemRight > int.MaxValue || itemRight < int.MinValue || itemBottom > int.MaxValue || itemBottom < int.MinValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(sizes), "An image placement exceeds the Int32 coordinate range.");
                }

                cornerX[i] = corners[i].X;
                cornerY[i] = corners[i].Y;
                widths[i] = size.Width;
                heights[i] = size.Height;
                left = Math.Min(left, corners[i].X);
                top = Math.Min(top, corners[i].Y);
                right = Math.Max(right, itemRight);
                bottom = Math.Max(bottom, itemBottom);
            }

            if (right - left > int.MaxValue || bottom - top > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(sizes), "The combined destination ROI exceeds the Int32 size range.");
            }

            prepared = false;
            NativeException.ThrowIfError(NativeMethods.StitchingBlenderPrepare(
                NativeHandle, cornerX, cornerY, widths, heights, corners.Length));
            preparedRoi = new Rect((int)left, (int)top, (int)(right - left), (int)(bottom - top));
            prepared = true;
        }

        /// <summary>Prepares explicit destination storage for subsequent feeds.</summary>
        public void Prepare(Rect destinationRoi)
        {
            ThrowIfDisposed();
            ValidateRoi(destinationRoi, nameof(destinationRoi));
            prepared = false;
            NativeException.ThrowIfError(NativeMethods.StitchingBlenderPrepareRoi(
                NativeHandle,
                destinationRoi.X,
                destinationRoi.Y,
                destinationRoi.Width,
                destinationRoi.Height));
            preparedRoi = destinationRoi;
            prepared = true;
        }

        /// <summary>Feeds one caller-owned image and mask into the prepared destination.</summary>
        public void Feed(Mat image, Mat mask, Point topLeft)
        {
            ThrowIfDisposed();
            if (!prepared) throw new InvalidOperationException("Prepare must succeed before Feed.");
            ValidateFeedImage(image, nameof(image));
            ValidateMask(mask, nameof(mask));
            if (image.Rows != mask.Rows || image.Cols != mask.Cols)
            {
                throw new ArgumentException("The mask size must match the image size.", nameof(mask));
            }

            long right = (long)topLeft.X + image.Cols;
            long bottom = (long)topLeft.Y + image.Rows;
            long preparedRight = (long)preparedRoi.X + preparedRoi.Width;
            long preparedBottom = (long)preparedRoi.Y + preparedRoi.Height;
            if (topLeft.X < preparedRoi.X || topLeft.Y < preparedRoi.Y || right > preparedRight || bottom > preparedBottom)
            {
                throw new ArgumentOutOfRangeException(nameof(topLeft), "The image placement must fit completely inside the prepared ROI.");
            }

            NativeException.ThrowIfError(NativeMethods.StitchingBlenderFeed(
                NativeHandle, image.NativeHandle, mask.NativeHandle, topLeft.X, topLeft.Y));
            GC.KeepAlive(image);
            GC.KeepAlive(mask);
        }

        /// <summary>
        /// Completes blending into caller-owned CV_16SC3 image and CV_8UC1 mask objects.
        /// Prepare must be called again before another feed or blend cycle.
        /// </summary>
        public void Blend(Mat destination, Mat destinationMask)
        {
            ThrowIfDisposed();
            if (!prepared) throw new InvalidOperationException("Prepare must succeed before Blend.");
            ValidateOutput(destination, nameof(destination));
            ValidateOutput(destinationMask, nameof(destinationMask));
            if (ReferenceEquals(destination, destinationMask) || destination.NativeHandle == destinationMask.NativeHandle)
            {
                throw new ArgumentException("Destination image and mask must use distinct Mat handles.", nameof(destinationMask));
            }

            prepared = false;
            NativeException.ThrowIfError(NativeMethods.StitchingBlenderBlend(
                NativeHandle, destination.NativeHandle, destinationMask.NativeHandle));
            GC.KeepAlive(destination);
            GC.KeepAlive(destinationMask);
        }

        /// <summary>Normalizes a CV_16SC3 source in place using a same-sized CV_32FC1 or CV_16SC1 weight map.</summary>
        public static void NormalizeUsingWeightMap(Mat weight, Mat source)
        {
            ValidateInput(weight, nameof(weight));
            ValidateInput(source, nameof(source));
            if (source.Type != MatType.CV_16SC3)
            {
                throw new ArgumentException("The source must have type CV_16SC3.", nameof(source));
            }
            if (weight.Type != MatType.CV_32FC1 && weight.Type != MatType.CV_16SC1)
            {
                throw new ArgumentException("The weight map must have type CV_32FC1 or CV_16SC1.", nameof(weight));
            }
            if (weight.Rows != source.Rows || weight.Cols != source.Cols)
            {
                throw new ArgumentException("The weight map size must match the source size.", nameof(weight));
            }

            NativeException.ThrowIfError(NativeMethods.StitchingNormalizeUsingWeightMap(weight.NativeHandle, source.NativeHandle));
            GC.KeepAlive(weight);
            GC.KeepAlive(source);
        }

        /// <summary>Creates a caller-owned CV_32FC1 feather weight map from a CV_8UC1 mask.</summary>
        public static void CreateWeightMap(Mat mask, float sharpness, Mat weight)
        {
            ValidateMask(mask, nameof(mask));
            ValidateSharpness(sharpness);
            ValidateOutput(weight, nameof(weight));
            if (ReferenceEquals(mask, weight) || mask.NativeHandle == weight.NativeHandle)
            {
                throw new ArgumentException("Mask and weight output must use distinct Mat handles.", nameof(weight));
            }

            NativeException.ThrowIfError(NativeMethods.StitchingCreateWeightMap(mask.NativeHandle, sharpness, weight.NativeHandle));
            GC.KeepAlive(mask);
            GC.KeepAlive(weight);
        }

        /// <summary>Creates an independently owned CPU Laplacian pyramid.</summary>
        public static Mat[] CreateLaplacePyramid(Mat image, int numberOfLevels)
        {
            return CreateLaplacePyramidCore(image, numberOfLevels, false);
        }

        /// <summary>
        /// Creates an independently owned CUDA Laplacian pyramid. A non-CUDA OpenCV build reports
        /// the upstream StsNotImplemented error through <see cref="NativeException"/>.
        /// </summary>
        public static Mat[] CreateLaplacePyramidGpu(Mat image, int numberOfLevels)
        {
            return CreateLaplacePyramidCore(image, numberOfLevels, true);
        }

        /// <summary>Restores the first image of a CPU Laplacian pyramid in place.</summary>
        public static void RestoreImageFromLaplacePyramid(Mat[] pyramid)
        {
            RestoreImageFromLaplacePyramidCore(pyramid, false);
        }

        /// <summary>
        /// Restores the first image using CUDA. A non-CUDA OpenCV build reports the upstream
        /// StsNotImplemented error through <see cref="NativeException"/>.
        /// </summary>
        public static void RestoreImageFromLaplacePyramidGpu(Mat[] pyramid)
        {
            RestoreImageFromLaplacePyramidCore(pyramid, true);
        }

        /// <summary>Releases the owned native blender.</summary>
        public void Dispose()
        {
            if (disposed) return;
            handle.Dispose();
            disposed = true;
            prepared = false;
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        internal static void ValidateSharpness(float sharpness)
        {
            if (float.IsNaN(sharpness) || float.IsInfinity(sharpness))
            {
                throw new ArgumentOutOfRangeException(nameof(sharpness), "Sharpness must be finite.");
            }
        }

        internal static void ValidateNumberOfBands(int numberOfBands)
        {
            if (numberOfBands < 0 || numberOfBands > 30)
            {
                throw new ArgumentOutOfRangeException(nameof(numberOfBands), "Band count must be between 0 and 30.");
            }
        }

        internal static void ValidateWeightType(int weightType)
        {
            if (weightType != MatType.CV_32FC1 && weightType != MatType.CV_16SC1)
            {
                throw new ArgumentOutOfRangeException(nameof(weightType), "Weight type must be CV_32FC1 or CV_16SC1.");
            }
        }

        internal static void ValidateMask(Mat mask, string parameterName)
        {
            ValidateInput(mask, parameterName);
            if (mask.Type != MatType.CV_8UC1)
            {
                throw new ArgumentException("The mask must have type CV_8UC1.", parameterName);
            }
        }

        internal static Mat[] CreateEmptyMats(int count)
        {
            var result = new Mat[count];
            int created = 0;
            try
            {
                for (; created < count; ++created) result[created] = new Mat();
                return result;
            }
            catch
            {
                for (int i = 0; i < created; ++i) result[i]?.Dispose();
                throw;
            }
        }

        internal static IntPtr[] GetHandles(Mat[] mats)
        {
            var handles = new IntPtr[mats.Length];
            for (int i = 0; i < mats.Length; ++i) handles[i] = mats[i].NativeHandle;
            return handles;
        }

        internal static void DisposeMats(Mat[] mats)
        {
            for (int i = 0; i < mats.Length; ++i) mats[i]?.Dispose();
        }

        private void ValidateFeedImage(Mat image, string parameterName)
        {
            ValidateInput(image, parameterName);
            bool valid = blenderType == BlenderType.MultiBand
                ? image.Type == MatType.CV_8UC3 || image.Type == MatType.CV_16SC3
                : image.Type == MatType.CV_16SC3;
            if (!valid)
            {
                string required = blenderType == BlenderType.MultiBand ? "CV_8UC3 or CV_16SC3" : "CV_16SC3";
                throw new ArgumentException("The image must have type " + required + ".", parameterName);
            }
        }

        private static Mat[] CreateLaplacePyramidCore(Mat image, int numberOfLevels, bool useGpu)
        {
            ValidateInput(image, nameof(image));
            ValidateNumberOfBands(numberOfLevels);
            Mat[] result = CreateEmptyMats(numberOfLevels + 1);
            try
            {
                IntPtr[] handles = GetHandles(result);
                int status = useGpu
                    ? NativeMethods.StitchingCreateLaplacePyramidGpu(image.NativeHandle, numberOfLevels, handles, handles.Length)
                    : NativeMethods.StitchingCreateLaplacePyramid(image.NativeHandle, numberOfLevels, handles, handles.Length);
                NativeException.ThrowIfError(status);
                GC.KeepAlive(image);
                GC.KeepAlive(result);
                return result;
            }
            catch
            {
                DisposeMats(result);
                throw;
            }
        }

        private static void RestoreImageFromLaplacePyramidCore(Mat[] pyramid, bool useGpu)
        {
            if (pyramid == null) throw new ArgumentNullException(nameof(pyramid));
            ValidatePyramid(pyramid);
            IntPtr[] handles = GetHandles(pyramid);
            int status = useGpu
                ? NativeMethods.StitchingRestoreImageFromLaplacePyramidGpu(handles, handles.Length)
                : NativeMethods.StitchingRestoreImageFromLaplacePyramid(handles, handles.Length);
            NativeException.ThrowIfError(status);
            GC.KeepAlive(pyramid);
        }

        private static void ValidatePyramid(Mat[] pyramid)
        {
            if (pyramid.Length == 0) return;
            Mat first = pyramid[0] ?? throw new ArgumentNullException(nameof(pyramid), "The pyramid contains null.");
            ValidateInput(first, nameof(pyramid));
            for (int i = 1; i < pyramid.Length; ++i)
            {
                Mat previous = pyramid[i - 1];
                Mat current = pyramid[i] ?? throw new ArgumentNullException(nameof(pyramid), "The pyramid contains null.");
                ValidateInput(current, nameof(pyramid));
                if (current.Type != first.Type || current.Cols != (previous.Cols + 1) / 2 || current.Rows != (previous.Rows + 1) / 2)
                {
                    throw new ArgumentException("Pyramid levels must retain type and use ceil(previous size / 2).", nameof(pyramid));
                }
                for (int j = 0; j < i; ++j)
                {
                    if (ReferenceEquals(current, pyramid[j]) || current.NativeHandle == pyramid[j].NativeHandle)
                    {
                        throw new ArgumentException("Pyramid levels must use distinct Mat handles.", nameof(pyramid));
                    }
                }
            }
        }

        private static void ValidateType(BlenderType type)
        {
            if (type < BlenderType.None || type > BlenderType.MultiBand)
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        private static void ValidateRoi(Rect roi, string parameterName)
        {
            if (roi.Width <= 0 || roi.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "ROI width and height must be positive.");
            }
            long right = (long)roi.X + roi.Width;
            long bottom = (long)roi.Y + roi.Height;
            if (right > int.MaxValue || right < int.MinValue || bottom > int.MaxValue || bottom < int.MinValue)
            {
                throw new ArgumentOutOfRangeException(parameterName, "ROI coordinates exceed the Int32 range.");
            }
        }

        private static void ValidateInput(Mat input, string parameterName)
        {
            if (input == null) throw new ArgumentNullException(parameterName);
            if (input.Empty) throw new ArgumentException("The matrix must not be empty.", parameterName);
        }

        private static void ValidateOutput(Mat output, string parameterName)
        {
            if (output == null) throw new ArgumentNullException(parameterName);
            _ = output.NativeHandle;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
