using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Owns one pairwise stitching match result, including copied matches, inliers, and homography.</summary>
    public sealed unsafe class MatchesInfo : IDisposable
    {
        private NativeMatchesInfoHandle handle;
        private bool disposed;

        /// <summary>Creates an empty match result.</summary>
        public MatchesInfo()
        {
            NativeException.ThrowIfError(NativeMethods.StitchingMatchesInfoCreate(out IntPtr nativeHandle));
            handle = NativeMatchesInfoHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this result has been disposed.</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets the source image index, or -1 when no pair was matched.</summary>
        public int SourceImageIndex
        {
            get { return GetMetadata().SourceImageIndex; }
        }

        /// <summary>Gets the destination image index, or -1 when no pair was matched.</summary>
        public int DestinationImageIndex
        {
            get { return GetMetadata().DestinationImageIndex; }
        }

        /// <summary>Gets the number of geometrically consistent matches.</summary>
        public int NumberOfInliers
        {
            get { return GetMetadata().NumberOfInliers; }
        }

        /// <summary>Gets OpenCV's panorama match confidence.</summary>
        public double Confidence
        {
            get { return GetMetadata().Confidence; }
        }

        /// <summary>Gets a copy of all descriptor matches.</summary>
        public DMatch[] Matches
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StitchingMatchesInfoGetMatchesCount(NativeHandle, out int count));
                if (count <= 0) return Array.Empty<DMatch>();
                var native = new NativeDMatch[count];
                fixed (NativeDMatch* pointer = native)
                {
                    NativeException.ThrowIfError(NativeMethods.StitchingMatchesInfoGetMatchesFill(
                        NativeHandle, pointer, native.Length, out int written));
                    return DMatchMarshaller.FromNative(native, CheckedWrittenCount(written, native.Length, "matches"));
                }
            }
        }

        /// <summary>Gets a copy of the geometrically consistent match mask.</summary>
        public byte[] Inliers
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StitchingMatchesInfoGetInliersCount(NativeHandle, out int count));
                if (count <= 0) return Array.Empty<byte>();
                var result = new byte[count];
                fixed (byte* pointer = result)
                {
                    NativeException.ThrowIfError(NativeMethods.StitchingMatchesInfoGetInliersFill(
                        NativeHandle, pointer, result.Length, out int written));
                    int length = CheckedWrittenCount(written, result.Length, "inliers");
                    if (length == result.Length) return result;
                    var trimmed = new byte[length];
                    Array.Copy(result, trimmed, length);
                    return trimmed;
                }
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Copies the homography or affine transform into a new independently owned Mat.</summary>
        public Mat GetHomography()
        {
            var result = new Mat();
            try
            {
                CopyHomographyTo(result);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>Copies the homography or affine transform into caller-owned Mat storage.</summary>
        public void CopyHomographyTo(Mat homography)
        {
            ThrowIfDisposed();
            if (homography == null) throw new ArgumentNullException(nameof(homography));
            NativeException.ThrowIfError(NativeMethods.StitchingMatchesInfoCopyHomography(NativeHandle, homography.NativeHandle));
            GC.KeepAlive(homography);
        }

        /// <summary>Releases the owned native match result.</summary>
        public void Dispose()
        {
            if (disposed) return;
            handle.Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }

        private Metadata GetMetadata()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.StitchingMatchesInfoGetMetadata(
                NativeHandle,
                out int sourceImageIndex,
                out int destinationImageIndex,
                out int numberOfInliers,
                out double confidence));
            return new Metadata(sourceImageIndex, destinationImageIndex, numberOfInliers, confidence);
        }

        private static int CheckedWrittenCount(int written, int capacity, string collectionName)
        {
            if (written < 0 || written > capacity)
            {
                throw new OpenCvException("Native " + collectionName + " count exceeded the allocated capacity.");
            }
            return written;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        private readonly struct Metadata
        {
            internal Metadata(int sourceImageIndex, int destinationImageIndex, int numberOfInliers, double confidence)
            {
                SourceImageIndex = sourceImageIndex;
                DestinationImageIndex = destinationImageIndex;
                NumberOfInliers = numberOfInliers;
                Confidence = confidence;
            }

            internal int SourceImageIndex { get; }
            internal int DestinationImageIndex { get; }
            internal int NumberOfInliers { get; }
            internal double Confidence { get; }
        }
    }
}
