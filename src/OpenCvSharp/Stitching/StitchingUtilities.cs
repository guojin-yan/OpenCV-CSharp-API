using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Stitching
{
    /// <summary>Placement, subset, and diagnostic helpers used by detailed stitching workflows.</summary>
    public static class StitchingUtilities
    {
        /// <summary>Computes the positive-area intersection of two placed rectangles.</summary>
        public static bool TryOverlapRoi(Rect first, Rect second, out Rect roi)
        {
            ValidateRect(first, nameof(first)); ValidateRect(second, nameof(second));
            NativeException.ThrowIfError(NativeMethods.StitchingOverlapRoi(
                first.X, first.Y, first.Width, first.Height,
                second.X, second.Y, second.Width, second.Height,
                out NativeMethods.StitchingRectNative value, out int overlaps));
            roi = ToRect(value); return overlaps != 0;
        }

        /// <summary>Returns the union ROI of equal-length corner and size collections.</summary>
        public static Rect ResultRoi(Point[] corners, Size[] sizes)
        {
            StitchingDetailMarshal.GetPlacements(corners, sizes, out int[] x, out int[] y, out int[] widths, out int[] heights);
            NativeException.ThrowIfError(NativeMethods.StitchingResultRoiSizes(
                x, y, x.Length, widths, heights, widths.Length, out NativeMethods.StitchingRectNative value));
            return ToRect(value);
        }

        /// <summary>Returns the union ROI of equal-length corner and borrowed image collections.</summary>
        public static Rect ResultRoi(Point[] corners, Mat[] images)
        {
            StitchingDetailMarshal.GetCorners(corners, out int[] x, out int[] y);
            if (images == null) throw new ArgumentNullException(nameof(images));
            if (images.Length != corners.Length) throw new ArgumentException("Corner and image counts must match.", nameof(images));
            var handles = new IntPtr[images.Length];
            for (int i = 0; i < images.Length; ++i)
            {
                Mat image = images[i] ?? throw new ArgumentNullException(nameof(images), "The image collection contains null.");
                if (image.Empty) throw new ArgumentException("Images must not be empty.", nameof(images));
                handles[i] = image.NativeHandle;
            }
            NativeException.ThrowIfError(NativeMethods.StitchingResultRoiImages(
                x, y, x.Length, handles, handles.Length, out NativeMethods.StitchingRectNative value));
            GC.KeepAlive(images); return ToRect(value);
        }

        /// <summary>Returns the common-intersection ROI of equal-length corner and size collections.</summary>
        public static Rect ResultRoiIntersection(Point[] corners, Size[] sizes)
        {
            StitchingDetailMarshal.GetPlacements(corners, sizes, out int[] x, out int[] y, out int[] widths, out int[] heights);
            NativeException.ThrowIfError(NativeMethods.StitchingResultRoiIntersection(
                x, y, x.Length, widths, heights, widths.Length, out NativeMethods.StitchingRectNative value));
            return ToRect(value);
        }

        /// <summary>Returns the component-wise minimum top-left point.</summary>
        public static Point ResultTopLeft(Point[] corners)
        {
            StitchingDetailMarshal.GetCorners(corners, out int[] x, out int[] y);
            NativeException.ThrowIfError(NativeMethods.StitchingResultTl(
                x, y, x.Length, out NativeMethods.StitchingPointNative value));
            return new Point(value.X, value.Y);
        }

        /// <summary>Selects exactly <paramref name="count"/> distinct indices in ascending order.</summary>
        public static int[] SelectRandomSubset(int count, int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            if (count < 0 || count > size) throw new ArgumentOutOfRangeException(nameof(count));
            var result = new int[count];
            NativeException.ThrowIfError(NativeMethods.StitchingSelectRandomSubset(
                count, size, result, result.Length, out int written));
            if (written != result.Length) throw new OpenCvException("Native random-subset count changed unexpectedly.");
            return result;
        }

        /// <summary>Gets the current upstream Stitching diagnostic log level without mutating global state.</summary>
        public static int LogLevel
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.StitchingLogLevel(out int level));
                return level;
            }
        }

        private static Rect ToRect(NativeMethods.StitchingRectNative value)
        {
            return new Rect(value.X, value.Y, value.Width, value.Height);
        }

        private static void ValidateRect(Rect value, string parameterName)
        {
            if (value.Width <= 0 || value.Height <= 0) throw new ArgumentOutOfRangeException(parameterName);
            long right = (long)value.X + value.Width; long bottom = (long)value.Y + value.Height;
            if (right > int.MaxValue || right < int.MinValue || bottom > int.MaxValue || bottom < int.MinValue)
                throw new ArgumentOutOfRangeException(parameterName, "Rectangle coordinates exceed the Int32 range.");
        }
    }

    internal static class StitchingDetailMarshal
    {
        internal static void GetCorners(Point[] corners, out int[] x, out int[] y)
        {
            if (corners == null) throw new ArgumentNullException(nameof(corners));
            if (corners.Length == 0) throw new ArgumentException("At least one corner is required.", nameof(corners));
            x = new int[corners.Length]; y = new int[corners.Length];
            for (int i = 0; i < corners.Length; ++i) { x[i] = corners[i].X; y[i] = corners[i].Y; }
        }

        internal static void GetPlacements(
            Point[] corners, Size[] sizes, out int[] x, out int[] y, out int[] widths, out int[] heights)
        {
            GetCorners(corners, out x, out y);
            if (sizes == null) throw new ArgumentNullException(nameof(sizes));
            if (sizes.Length != corners.Length) throw new ArgumentException("Corner and size counts must match.", nameof(sizes));
            widths = new int[sizes.Length]; heights = new int[sizes.Length];
            for (int i = 0; i < sizes.Length; ++i)
            {
                if (sizes[i].Width <= 0 || sizes[i].Height <= 0)
                    throw new ArgumentOutOfRangeException(nameof(sizes), "Every width and height must be positive.");
                long right = (long)corners[i].X + sizes[i].Width; long bottom = (long)corners[i].Y + sizes[i].Height;
                if (right > int.MaxValue || right < int.MinValue || bottom > int.MaxValue || bottom < int.MinValue)
                    throw new ArgumentOutOfRangeException(nameof(sizes), "An image placement exceeds the Int32 range.");
                widths[i] = sizes[i].Width; heights[i] = sizes[i].Height;
            }
        }
    }
}
