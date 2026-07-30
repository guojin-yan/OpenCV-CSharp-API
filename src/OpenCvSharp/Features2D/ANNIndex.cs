using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Features2D
{
    /// <summary>
    /// Owns an OpenCV approximate-nearest-neighbor index backed by <c>cv::ANNIndex</c>.
    /// </summary>
    public sealed class ANNIndex : IDisposable
    {
        private readonly NativeAnnIndexHandle handle;
        private readonly int dimension;
        private readonly ANNIndexDistance distance;

        private ANNIndex(IntPtr nativeHandle, int dimension, ANNIndexDistance distance)
        {
            handle = NativeAnnIndexHandle.FromNativePointer(nativeHandle);
            this.dimension = dimension;
            this.distance = distance;
        }

        /// <summary>Gets whether this wrapper has released its native index.</summary>
        public bool IsDisposed
        {
            get { return handle.IsClosed; }
        }

        /// <summary>Gets the number of trees in the built or loaded index.</summary>
        public int TreeNumber
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.Features2DAnnIndexGetTreeNumber(NativeHandle, out int value));
                return value;
            }
        }

        /// <summary>Gets the number of feature vectors stored in the index.</summary>
        public int ItemNumber
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.Features2DAnnIndexGetItemNumber(NativeHandle, out int value));
                return value;
            }
        }

        /// <summary>Creates an approximate-nearest-neighbor index.</summary>
        /// <param name="dimension">The number of values in each feature row.</param>
        /// <param name="distance">The distance metric and corresponding matrix type.</param>
        public static ANNIndex Create(int dimension, ANNIndexDistance distance = ANNIndexDistance.Euclidean)
        {
            if (dimension <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(dimension));
            }
            ValidateDistance(distance, nameof(distance));

            NativeException.ThrowIfError(NativeMethods.Features2DAnnIndexCreate(dimension, (int)distance, out IntPtr nativeHandle));
            return new ANNIndex(nativeHandle, dimension, distance);
        }

        /// <summary>Adds feature rows to the index.</summary>
        /// <remarks>
        /// Hamming indices accept <c>CV_8UC1</c>; all other metrics accept <c>CV_32FC1</c>.
        /// A non-contiguous row view is accepted because OpenCV reads each row independently.
        /// </remarks>
        public void AddItems(Mat features)
        {
            ValidateFeatures(features, nameof(features), false);
            NativeException.ThrowIfError(NativeMethods.Features2DAnnIndexAddItems(NativeHandle, features.NativeHandle));
        }

        /// <summary>Builds the index after feature rows have been added.</summary>
        /// <param name="trees">A positive tree count, or <c>-1</c> for OpenCV's automatic choice.</param>
        public void Build(int trees = -1)
        {
            if (trees == 0 || trees < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(trees));
            }
            NativeException.ThrowIfError(NativeMethods.Features2DAnnIndexBuild(NativeHandle, trees));
        }

        /// <summary>Finds the nearest indexed feature rows for each query row.</summary>
        /// <param name="query">A continuous query matrix with the configured feature dimension and type.</param>
        /// <param name="indices">Caller-owned output populated as <c>query.Rows x knn</c> <c>CV_32SC1</c>.</param>
        /// <param name="distances">Caller-owned output populated as <c>query.Rows x knn</c> with the feature matrix type.</param>
        /// <param name="knn">The positive number of neighbors, not exceeding <see cref="ItemNumber"/>.</param>
        /// <param name="searchK">A positive search limit, or <c>-1</c> for OpenCV's default.</param>
        public void KnnSearch(Mat query, Mat indices, Mat distances, int knn, int searchK = -1)
        {
            ValidateFeatures(query, nameof(query), true);
            if (indices == null) throw new ArgumentNullException(nameof(indices));
            if (distances == null) throw new ArgumentNullException(nameof(distances));
            if (ReferenceEquals(query, indices) || ReferenceEquals(query, distances) || ReferenceEquals(indices, distances))
            {
                throw new ArgumentException("Query, indices, and distances must be distinct matrix objects.");
            }
            if (knn <= 0 || knn > ItemNumber)
            {
                throw new ArgumentOutOfRangeException(nameof(knn));
            }
            if (searchK == 0 || searchK < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(searchK));
            }

            NativeException.ThrowIfError(NativeMethods.Features2DAnnIndexKnnSearch(
                NativeHandle,
                query.NativeHandle,
                indices.NativeHandle,
                distances.NativeHandle,
                knn,
                searchK));
        }

        /// <summary>Saves the built index to a UTF-8 path.</summary>
        public void Save(string filename, bool prefault = false)
        {
            byte[] filenameUtf8 = CorePersistenceMarshal.Encode(filename, nameof(filename), false);
            NativeException.ThrowIfError(NativeMethods.Features2DAnnIndexSave(
                NativeHandle,
                filenameUtf8,
                filenameUtf8.Length,
                prefault ? 1 : 0));
        }

        /// <summary>Loads an index from a UTF-8 path into this dimension and metric configuration.</summary>
        public void Load(string filename, bool prefault = false)
        {
            byte[] filenameUtf8 = CorePersistenceMarshal.Encode(filename, nameof(filename), false);
            NativeException.ThrowIfError(NativeMethods.Features2DAnnIndexLoad(
                NativeHandle,
                filenameUtf8,
                filenameUtf8.Length,
                prefault ? 1 : 0));
        }

        /// <summary>Requests that the subsequent index build write directly to a UTF-8 path.</summary>
        /// <returns><see langword="true"/> when OpenCV enabled on-disk building.</returns>
        public bool SetOnDiskBuild(string filename)
        {
            byte[] filenameUtf8 = CorePersistenceMarshal.Encode(filename, nameof(filename), false);
            NativeException.ThrowIfError(NativeMethods.Features2DAnnIndexSetOnDiskBuild(
                NativeHandle,
                filenameUtf8,
                filenameUtf8.Length,
                out int enabled));
            return enabled != 0;
        }

        /// <summary>Sets the index builder's 32-bit seed before adding items.</summary>
        public void SetSeed(int seed)
        {
            NativeException.ThrowIfError(NativeMethods.Features2DAnnIndexSetSeed(NativeHandle, seed));
        }

        /// <summary>Releases the native index. Repeated disposal is safe.</summary>
        public void Dispose()
        {
            handle.Dispose();
        }

        private IntPtr NativeHandle
        {
            get
            {
                if (handle.IsClosed || handle.IsInvalid)
                {
                    throw new ObjectDisposedException(nameof(ANNIndex));
                }
                return handle.DangerousGetHandle();
            }
        }

        private void ValidateFeatures(Mat features, string parameterName, bool requireContinuous)
        {
            if (features == null) throw new ArgumentNullException(parameterName);
            if (features.Empty || features.Dims != 2 || features.Rows <= 0 || features.Cols != dimension)
            {
                throw new ArgumentException("Feature matrices must be non-empty, two-dimensional, and match the configured feature dimension.", parameterName);
            }
            int expectedType = distance == ANNIndexDistance.Hamming ? MatType.CV_8UC1 : MatType.CV_32FC1;
            if (features.Type != expectedType)
            {
                throw new ArgumentException(
                    distance == ANNIndexDistance.Hamming
                        ? "Hamming feature matrices must be CV_8UC1."
                        : "Non-Hamming feature matrices must be CV_32FC1.",
                    parameterName);
            }
            if (requireContinuous && !features.IsContinuous)
            {
                throw new ArgumentException("Query matrices must be continuous.", parameterName);
            }
        }

        private static void ValidateDistance(ANNIndexDistance distance, string parameterName)
        {
            int value = (int)distance;
            if (value < (int)ANNIndexDistance.Euclidean || value > (int)ANNIndexDistance.DotProduct)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }
}
