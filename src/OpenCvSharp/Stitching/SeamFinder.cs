using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Built-in seam-finding strategy.</summary>
    public enum SeamFinderType
    {
        /// <summary>Leaves input masks unchanged.</summary>
        None = 0,
        /// <summary>Uses pairwise Voronoi seams.</summary>
        Voronoi = 1,
        /// <summary>Uses dynamic-programming seams.</summary>
        DynamicProgramming = 2
    }

    /// <summary>Dynamic-programming seam cost.</summary>
    public enum DpSeamCost
    {
        /// <summary>Uses color differences.</summary>
        Color = 0,
        /// <summary>Uses color and gradient differences.</summary>
        ColorGradient = 1
    }

    /// <summary>Graph-cut seam cost.</summary>
    public enum GraphCutSeamCost
    {
        /// <summary>Uses color differences.</summary>
        Color = 0,
        /// <summary>Uses color and gradient differences.</summary>
        ColorGradient = 1
    }

    /// <summary>Owns a seam-finding strategy and updates caller-owned masks transactionally.</summary>
    public abstract class SeamFinder : IDisposable
    {
        private NativeSeamFinderHandle handle;
        private bool disposed;

        internal SeamFinder(IntPtr nativeHandle)
        {
            handle = NativeSeamFinderHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this seam finder has been disposed.</summary>
        public bool IsDisposed => disposed;

        internal IntPtr NativeHandle
        {
            get { ThrowIfDisposed(); return handle.DangerousGetHandle(); }
        }

        /// <summary>Creates a no-op, Voronoi, or dynamic-programming seam finder.</summary>
        public static SeamFinder CreateDefault(SeamFinderType type)
        {
            if (type < SeamFinderType.None || type > SeamFinderType.DynamicProgramming)
                throw new ArgumentOutOfRangeException(nameof(type));
            NativeException.ThrowIfError(NativeMethods.StitchingSeamFinderCreateDefault((int)type, out IntPtr nativeHandle));
            switch (type)
            {
                case SeamFinderType.None: return new NoSeamFinder(nativeHandle);
                case SeamFinderType.Voronoi: return new VoronoiSeamFinder(nativeHandle);
                case SeamFinderType.DynamicProgramming: return new DpSeamFinder(nativeHandle);
                default: throw new ArgumentOutOfRangeException(nameof(type));
            }
        }

        /// <summary>
        /// Finds seams and commits all mask changes only after the native operation succeeds.
        /// Images are borrowed; masks remain caller-owned exact CV_8UC1 matrices.
        /// </summary>
        public void Find(Mat[] images, Point[] corners, Mat[] masks)
        {
            ThrowIfDisposed();
            if (images == null) throw new ArgumentNullException(nameof(images));
            if (corners == null) throw new ArgumentNullException(nameof(corners));
            if (masks == null) throw new ArgumentNullException(nameof(masks));
            if (images.Length == 0) throw new ArgumentException("At least one image is required.", nameof(images));
            if (corners.Length != images.Length) throw new ArgumentException("Corner and image counts must match.", nameof(corners));
            if (masks.Length != images.Length) throw new ArgumentException("Mask and image counts must match.", nameof(masks));

            var imageHandles = new IntPtr[images.Length];
            var maskHandles = new IntPtr[masks.Length];
            var cornerX = new int[corners.Length];
            var cornerY = new int[corners.Length];
            for (int i = 0; i < images.Length; ++i)
            {
                Mat image = images[i] ?? throw new ArgumentNullException(nameof(images), "The image collection contains null.");
                Mat mask = masks[i] ?? throw new ArgumentNullException(nameof(masks), "The mask collection contains null.");
                if (image.Empty) throw new ArgumentException("Images must not be empty.", nameof(images));
                if (mask.Empty || mask.Dims != 2 || mask.Rows != image.Rows || mask.Cols != image.Cols || mask.Type != MatType.CV_8UC1)
                    throw new ArgumentException("Each mask must be an exact same-sized CV_8UC1 matrix.", nameof(masks));
                for (int j = 0; j < i; ++j)
                    if (ReferenceEquals(mask, masks[j])) throw new ArgumentException("Mask objects must be distinct.", nameof(masks));
                imageHandles[i] = image.NativeHandle; maskHandles[i] = mask.NativeHandle;
                cornerX[i] = corners[i].X; cornerY[i] = corners[i].Y;
            }
            NativeException.ThrowIfError(NativeMethods.StitchingSeamFinderFind(
                NativeHandle, imageHandles, imageHandles.Length, cornerX, cornerY, corners.Length, maskHandles, maskHandles.Length));
            GC.KeepAlive(images); GC.KeepAlive(masks);
        }

        /// <summary>Releases the owned native strategy.</summary>
        public void Dispose()
        {
            if (disposed) return;
            handle.Dispose(); disposed = true; GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        internal static byte[] EncodeDpCost(DpSeamCost cost)
        {
            if (cost < DpSeamCost.Color || cost > DpSeamCost.ColorGradient) throw new ArgumentOutOfRangeException(nameof(cost));
            return CorePersistenceMarshal.Encode(cost == DpSeamCost.Color ? "COLOR" : "COLOR_GRAD", nameof(cost), false);
        }

        internal static byte[] EncodeGraphCutCost(GraphCutSeamCost cost)
        {
            if (cost < GraphCutSeamCost.Color || cost > GraphCutSeamCost.ColorGradient) throw new ArgumentOutOfRangeException(nameof(cost));
            return CorePersistenceMarshal.Encode(cost == GraphCutSeamCost.Color ? "COST_COLOR" : "COST_COLOR_GRAD", nameof(cost), false);
        }
    }

    /// <summary>No-op seam finder returned by <see cref="SeamFinder.CreateDefault"/>.</summary>
    public sealed class NoSeamFinder : SeamFinder
    {
        internal NoSeamFinder(IntPtr nativeHandle) : base(nativeHandle) { }
    }

    /// <summary>Base class for pairwise seam finders.</summary>
    public abstract class PairwiseSeamFinder : SeamFinder
    {
        internal PairwiseSeamFinder(IntPtr nativeHandle) : base(nativeHandle) { }
    }

    /// <summary>Voronoi pairwise seam finder returned by <see cref="SeamFinder.CreateDefault"/>.</summary>
    public sealed class VoronoiSeamFinder : PairwiseSeamFinder
    {
        internal VoronoiSeamFinder(IntPtr nativeHandle) : base(nativeHandle) { }
    }

    /// <summary>Dynamic-programming seam finder.</summary>
    public sealed class DpSeamFinder : SeamFinder
    {
        /// <summary>Creates a dynamic-programming seam finder.</summary>
        public DpSeamFinder(DpSeamCost cost = DpSeamCost.Color) : base(CreateNative(cost)) { }
        internal DpSeamFinder(IntPtr nativeHandle) : base(nativeHandle) { }

        /// <summary>Changes the cost used by subsequent calls.</summary>
        public void SetCostFunction(DpSeamCost cost)
        {
            byte[] value = EncodeDpCost(cost);
            NativeException.ThrowIfError(NativeMethods.StitchingSeamFinderSetDpCost(NativeHandle, value, value.Length));
        }

        private static IntPtr CreateNative(DpSeamCost cost)
        {
            byte[] value = EncodeDpCost(cost);
            NativeException.ThrowIfError(NativeMethods.StitchingSeamFinderCreateDp(value, value.Length, out IntPtr handle));
            return handle;
        }
    }

    /// <summary>Minimum graph-cut seam finder.</summary>
    public sealed class GraphCutSeamFinder : SeamFinder
    {
        /// <summary>Creates a graph-cut seam finder with finite terminal and bad-region costs.</summary>
        public GraphCutSeamFinder(
            GraphCutSeamCost cost = GraphCutSeamCost.ColorGradient,
            float terminalCost = 10000F,
            float badRegionPenalty = 1000F)
            : base(CreateNative(cost, terminalCost, badRegionPenalty)) { }

        private static IntPtr CreateNative(GraphCutSeamCost cost, float terminalCost, float badRegionPenalty)
        {
            if (float.IsNaN(terminalCost) || float.IsInfinity(terminalCost)) throw new ArgumentOutOfRangeException(nameof(terminalCost));
            if (float.IsNaN(badRegionPenalty) || float.IsInfinity(badRegionPenalty)) throw new ArgumentOutOfRangeException(nameof(badRegionPenalty));
            byte[] value = EncodeGraphCutCost(cost);
            NativeException.ThrowIfError(NativeMethods.StitchingSeamFinderCreateGraphCut(
                value, value.Length, terminalCost, badRegionPenalty, out IntPtr handle));
            return handle;
        }
    }
}
