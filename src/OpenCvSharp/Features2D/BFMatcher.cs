using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides brute-force descriptor matching compatible with <c>cv::BFMatcher</c>.
    /// 提供与 OpenCV <c>cv::BFMatcher</c> 兼容的暴力描述子匹配能力。
    /// </summary>
    public sealed class BFMatcher : DescriptorMatcher
    {
        private NativeBFMatcherHandle handle;
        private bool disposed;

        private BFMatcher(IntPtr nativeHandle)
        {
            handle = NativeBFMatcherHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// 获取此对象是否已经释放。
        /// </summary>
        public override bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets the norm type used for descriptor distance.
        /// 获取用于描述子距离计算的范数类型。
        /// </summary>
        public NormTypes NormType
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherGetNormType(NativeHandle, out int normType));
                return (NormTypes)normType;
            }
        }

        /// <summary>
        /// Gets a value indicating whether cross-check mode is enabled.
        /// 获取是否启用 cross-check 模式。
        /// </summary>
        public bool CrossCheck
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherGetCrossCheck(NativeHandle, out int crossCheck));
                return crossCheck != 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether descriptor masks are supported.
        /// 获取是否支持描述子掩码。
        /// </summary>
        public override bool IsMaskSupported
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherIsMaskSupported(NativeHandle, out int supported));
                return supported != 0;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the train descriptor collection is empty.
        /// 获取训练描述子集合是否为空。
        /// </summary>
        public override bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherEmpty(NativeHandle, out int empty));
                return empty != 0;
            }
        }

        internal override IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>
        /// Creates a brute-force matcher.
        /// 创建暴力匹配器。
        /// </summary>
        public static BFMatcher Create(NormTypes normType = NormTypes.L2, bool crossCheck = false)
        {
            ValidateNormType(normType, nameof(normType));
            NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherCreate((int)normType, crossCheck ? 1 : 0, out IntPtr nativeHandle));
            return new BFMatcher(nativeHandle);
        }

        /// <summary>
        /// Clones the matcher.
        /// 克隆匹配器。
        /// </summary>
        /// <param name="emptyTrainData">Whether to omit train descriptors from the clone. 是否在克隆对象中省略训练描述子。</param>
        /// <returns>The cloned matcher. 克隆后的匹配器。</returns>
        public override DescriptorMatcher Clone(bool emptyTrainData = false)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherClone(NativeHandle, emptyTrainData ? 1 : 0, out IntPtr nativeClone));
            return DescriptorMatcher.FromNativeHandle(nativeClone);
        }

        /// <summary>
        /// Gets cloned train descriptor matrices.
        /// 获取训练描述子矩阵的克隆集合。
        /// </summary>
        /// <returns>The train descriptors. 训练描述子集合。</returns>
        public override Mat[] GetTrainDescriptors()
        {
            ThrowIfDisposed();
            return DescriptorMatcherCoreEx.GetTrainDescriptors(
                NativeHandle,
                NativeMethods.Features2DBFMatcherGetTrainDescriptorsCount,
                NativeMethods.Features2DBFMatcherGetTrainDescriptorClone);
        }

        /// <summary>
        /// Adds train descriptors to the matcher collection.
        /// 向匹配器训练集合添加描述子。
        /// </summary>
        public override void Add(Mat[] descriptors)
        {
            ThrowIfDisposed();
            unsafe
            {
                DescriptorMatcherCore.Add(NativeHandle, descriptors, NativeMethods.Features2DBFMatcherAdd);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <inheritdoc/>
        public override void Add(ReadOnlySpan<Mat> descriptors)
        {
            ThrowIfDisposed();
            unsafe
            {
                DescriptorMatcherCore.Add(NativeHandle, descriptors, NativeMethods.Features2DBFMatcherAdd);
            }
        }
#endif

        /// <summary>
        /// Clears the train descriptor collection.
        /// 清除训练描述子集合。
        /// </summary>
        public override void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherClear(NativeHandle));
        }

        /// <summary>
        /// Trains the matcher.
        /// 训练匹配器。
        /// </summary>
        public override void Train()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherTrain(NativeHandle));
        }

        /// <summary>
        /// Finds the best matches between query and train descriptors.
        /// 在查询描述子和训练描述子之间查找最佳匹配。
        /// </summary>
        public override DMatch[] Match(Mat queryDescriptors, Mat trainDescriptors, Mat? mask = null)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateMatPair(queryDescriptors, trainDescriptors);
            NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherMatchCount(
                NativeHandle,
                queryDescriptors.NativeHandle,
                trainDescriptors.NativeHandle,
                DescriptorMatcherCore.OptionalHandle(mask),
                out int matchCount));

            unsafe
            {
                return DescriptorMatcherCore.FillMatches(matchCount, delegate (NativeDMatch* matchesPtr, out int writtenCount)
                {
                    return NativeMethods.Features2DBFMatcherMatchFill(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        trainDescriptors.NativeHandle,
                        DescriptorMatcherCore.OptionalHandle(mask),
                        matchesPtr,
                        matchCount,
                        out writtenCount);
                });
            }
        }

        /// <summary>
        /// Finds the best matches against the trained descriptor collection.
        /// 在已训练描述子集合中查找最佳匹配。
        /// </summary>
        public override DMatch[] Match(Mat queryDescriptors)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherMatchTrainCount(NativeHandle, queryDescriptors.NativeHandle, out int matchCount));
            unsafe
            {
                return DescriptorMatcherCore.FillMatches(matchCount, delegate (NativeDMatch* matchesPtr, out int writtenCount)
                {
                    return NativeMethods.Features2DBFMatcherMatchTrainFill(NativeHandle, queryDescriptors.NativeHandle, matchesPtr, matchCount, out writtenCount);
                });
            }
        }

        /// <summary>
        /// Finds the best matches against the trained descriptor collection using per-train masks.
        /// 使用每个训练描述子集合对应的掩码，在已训练描述子集合中查找最佳匹配。
        /// </summary>
        public override DMatch[] Match(Mat queryDescriptors, Mat[] masks)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            IntPtr[] maskHandles = DescriptorMatcherCoreEx.NormalizeMaskHandles(masks);
            unsafe
            {
                fixed (IntPtr* masksPtr = maskHandles)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherMatchTrainWithMasksCount(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        masksPtr,
                        maskHandles.Length,
                        out int matchCount));

                    return DescriptorMatcherCore.FillMatchesWithMasks(matchCount, maskHandles, delegate (IntPtr* fillMasksPtr, NativeDMatch* matchesPtr, out int writtenCount)
                    {
                        return NativeMethods.Features2DBFMatcherMatchTrainWithMasksFill(
                            NativeHandle,
                            queryDescriptors.NativeHandle,
                            fillMasksPtr,
                            maskHandles.Length,
                            matchesPtr,
                            matchCount,
                            out writtenCount);
                    });
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <inheritdoc/>
        public override DMatch[] Match(Mat queryDescriptors, ReadOnlySpan<Mat> masks)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            IntPtr[] maskHandles = DescriptorMatcherCoreEx.NormalizeMaskHandles(masks);
            unsafe
            {
                fixed (IntPtr* masksPtr = maskHandles)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherMatchTrainWithMasksCount(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        masksPtr,
                        maskHandles.Length,
                        out int matchCount));

                    return DescriptorMatcherCore.FillMatchesWithMasks(matchCount, maskHandles, delegate (IntPtr* fillMasksPtr, NativeDMatch* matchesPtr, out int writtenCount)
                    {
                        return NativeMethods.Features2DBFMatcherMatchTrainWithMasksFill(
                            NativeHandle,
                            queryDescriptors.NativeHandle,
                            fillMasksPtr,
                            maskHandles.Length,
                            matchesPtr,
                            matchCount,
                            out writtenCount);
                    });
                }
            }
        }
#endif

        /// <summary>
        /// Finds k nearest matches between query and train descriptors.
        /// 在查询描述子和训练描述子之间查找 k 个最近匹配。
        /// </summary>
        public override DMatch[][] KnnMatch(Mat queryDescriptors, Mat trainDescriptors, int k, Mat? mask = null, bool compactResult = false)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateK(k, nameof(k));
            DescriptorMatcherCore.ValidateMatPair(queryDescriptors, trainDescriptors);
            NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherKnnMatchCount(
                NativeHandle,
                queryDescriptors.NativeHandle,
                trainDescriptors.NativeHandle,
                k,
                DescriptorMatcherCore.OptionalHandle(mask),
                compactResult ? 1 : 0,
                out int groupCount,
                out int totalMatchCount));

            unsafe
            {
                return DescriptorMatcherCore.FillGroupedMatches(groupCount, totalMatchCount, delegate (int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                {
                    return NativeMethods.Features2DBFMatcherKnnMatchFill(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        trainDescriptors.NativeHandle,
                        k,
                        DescriptorMatcherCore.OptionalHandle(mask),
                        compactResult ? 1 : 0,
                        offsetsPtr,
                        groupCount + 1,
                        matchesPtr,
                        totalMatchCount,
                        out writtenGroupCount,
                        out writtenMatchCount);
                });
            }
        }

        /// <summary>
        /// Finds k nearest matches against the trained descriptor collection.
        /// 在已训练描述子集合中查找 k 个最近匹配。
        /// </summary>
        public override DMatch[][] KnnMatch(Mat queryDescriptors, int k, bool compactResult = false)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateK(k, nameof(k));
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherKnnMatchTrainCount(
                NativeHandle,
                queryDescriptors.NativeHandle,
                k,
                compactResult ? 1 : 0,
                out int groupCount,
                out int totalMatchCount));

            unsafe
            {
                return DescriptorMatcherCore.FillGroupedMatches(groupCount, totalMatchCount, delegate (int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                {
                    return NativeMethods.Features2DBFMatcherKnnMatchTrainFill(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        k,
                        compactResult ? 1 : 0,
                        offsetsPtr,
                        groupCount + 1,
                        matchesPtr,
                        totalMatchCount,
                        out writtenGroupCount,
                        out writtenMatchCount);
                });
            }
        }

        /// <summary>
        /// Finds k nearest matches against the trained descriptor collection using per-train masks.
        /// 使用每个训练描述子集合对应的掩码，在已训练集合中查找 k 个最近匹配。
        /// </summary>
        public override DMatch[][] KnnMatch(Mat queryDescriptors, int k, Mat[] masks, bool compactResult = false)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateK(k, nameof(k));
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            IntPtr[] maskHandles = DescriptorMatcherCoreEx.NormalizeMaskHandles(masks);
            unsafe
            {
                fixed (IntPtr* masksPtr = maskHandles)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherKnnMatchTrainWithMasksCount(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        k,
                        masksPtr,
                        maskHandles.Length,
                        compactResult ? 1 : 0,
                        out int groupCount,
                        out int totalMatchCount));

                    return DescriptorMatcherCore.FillGroupedMatchesWithMasks(groupCount, totalMatchCount, maskHandles, delegate (IntPtr* fillMasksPtr, int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                    {
                        return NativeMethods.Features2DBFMatcherKnnMatchTrainWithMasksFill(
                            NativeHandle,
                            queryDescriptors.NativeHandle,
                            k,
                            fillMasksPtr,
                            maskHandles.Length,
                            compactResult ? 1 : 0,
                            offsetsPtr,
                            groupCount + 1,
                            matchesPtr,
                            totalMatchCount,
                            out writtenGroupCount,
                            out writtenMatchCount);
                    });
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <inheritdoc/>
        public override DMatch[][] KnnMatch(Mat queryDescriptors, int k, ReadOnlySpan<Mat> masks, bool compactResult = false)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateK(k, nameof(k));
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            IntPtr[] maskHandles = DescriptorMatcherCoreEx.NormalizeMaskHandles(masks);
            unsafe
            {
                fixed (IntPtr* masksPtr = maskHandles)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherKnnMatchTrainWithMasksCount(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        k,
                        masksPtr,
                        maskHandles.Length,
                        compactResult ? 1 : 0,
                        out int groupCount,
                        out int totalMatchCount));

                    return DescriptorMatcherCore.FillGroupedMatchesWithMasks(groupCount, totalMatchCount, maskHandles, delegate (IntPtr* fillMasksPtr, int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                    {
                        return NativeMethods.Features2DBFMatcherKnnMatchTrainWithMasksFill(
                            NativeHandle,
                            queryDescriptors.NativeHandle,
                            k,
                            fillMasksPtr,
                            maskHandles.Length,
                            compactResult ? 1 : 0,
                            offsetsPtr,
                            groupCount + 1,
                            matchesPtr,
                            totalMatchCount,
                            out writtenGroupCount,
                            out writtenMatchCount);
                    });
                }
            }
        }
#endif

        /// <summary>
        /// Finds descriptor matches within a maximum distance.
        /// 查找最大距离以内的描述子匹配。
        /// </summary>
        public override DMatch[][] RadiusMatch(Mat queryDescriptors, Mat trainDescriptors, float maxDistance, Mat? mask = null, bool compactResult = false)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateMaxDistance(maxDistance, nameof(maxDistance));
            DescriptorMatcherCore.ValidateMatPair(queryDescriptors, trainDescriptors);
            NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherRadiusMatchCount(
                NativeHandle,
                queryDescriptors.NativeHandle,
                trainDescriptors.NativeHandle,
                maxDistance,
                DescriptorMatcherCore.OptionalHandle(mask),
                compactResult ? 1 : 0,
                out int groupCount,
                out int totalMatchCount));

            unsafe
            {
                return DescriptorMatcherCore.FillGroupedMatches(groupCount, totalMatchCount, delegate (int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                {
                    return NativeMethods.Features2DBFMatcherRadiusMatchFill(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        trainDescriptors.NativeHandle,
                        maxDistance,
                        DescriptorMatcherCore.OptionalHandle(mask),
                        compactResult ? 1 : 0,
                        offsetsPtr,
                        groupCount + 1,
                        matchesPtr,
                        totalMatchCount,
                        out writtenGroupCount,
                        out writtenMatchCount);
                });
            }
        }

        /// <summary>
        /// Finds descriptor matches in the trained collection within a maximum distance.
        /// 在已训练集合中查找最大距离以内的描述子匹配。
        /// </summary>
        public override DMatch[][] RadiusMatch(Mat queryDescriptors, float maxDistance, bool compactResult = false)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateMaxDistance(maxDistance, nameof(maxDistance));
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherRadiusMatchTrainCount(
                NativeHandle,
                queryDescriptors.NativeHandle,
                maxDistance,
                compactResult ? 1 : 0,
                out int groupCount,
                out int totalMatchCount));

            unsafe
            {
                return DescriptorMatcherCore.FillGroupedMatches(groupCount, totalMatchCount, delegate (int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                {
                    return NativeMethods.Features2DBFMatcherRadiusMatchTrainFill(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        maxDistance,
                        compactResult ? 1 : 0,
                        offsetsPtr,
                        groupCount + 1,
                        matchesPtr,
                        totalMatchCount,
                        out writtenGroupCount,
                        out writtenMatchCount);
                });
            }
        }

        /// <summary>
        /// Finds descriptor matches in the trained collection within a maximum distance using per-train masks.
        /// 使用每个训练描述子集合对应的掩码，在已训练集合中查找最大距离以内的描述子匹配。
        /// </summary>
        public override DMatch[][] RadiusMatch(Mat queryDescriptors, float maxDistance, Mat[] masks, bool compactResult = false)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateMaxDistance(maxDistance, nameof(maxDistance));
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            IntPtr[] maskHandles = DescriptorMatcherCoreEx.NormalizeMaskHandles(masks);
            unsafe
            {
                fixed (IntPtr* masksPtr = maskHandles)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherRadiusMatchTrainWithMasksCount(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        maxDistance,
                        masksPtr,
                        maskHandles.Length,
                        compactResult ? 1 : 0,
                        out int groupCount,
                        out int totalMatchCount));

                    return DescriptorMatcherCore.FillGroupedMatchesWithMasks(groupCount, totalMatchCount, maskHandles, delegate (IntPtr* fillMasksPtr, int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                    {
                        return NativeMethods.Features2DBFMatcherRadiusMatchTrainWithMasksFill(
                            NativeHandle,
                            queryDescriptors.NativeHandle,
                            maxDistance,
                            fillMasksPtr,
                            maskHandles.Length,
                            compactResult ? 1 : 0,
                            offsetsPtr,
                            groupCount + 1,
                            matchesPtr,
                            totalMatchCount,
                            out writtenGroupCount,
                            out writtenMatchCount);
                    });
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <inheritdoc/>
        public override DMatch[][] RadiusMatch(Mat queryDescriptors, float maxDistance, ReadOnlySpan<Mat> masks, bool compactResult = false)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateMaxDistance(maxDistance, nameof(maxDistance));
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            IntPtr[] maskHandles = DescriptorMatcherCoreEx.NormalizeMaskHandles(masks);
            unsafe
            {
                fixed (IntPtr* masksPtr = maskHandles)
                {
                    NativeException.ThrowIfError(NativeMethods.Features2DBFMatcherRadiusMatchTrainWithMasksCount(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        maxDistance,
                        masksPtr,
                        maskHandles.Length,
                        compactResult ? 1 : 0,
                        out int groupCount,
                        out int totalMatchCount));

                    return DescriptorMatcherCore.FillGroupedMatchesWithMasks(groupCount, totalMatchCount, maskHandles, delegate (IntPtr* fillMasksPtr, int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                    {
                        return NativeMethods.Features2DBFMatcherRadiusMatchTrainWithMasksFill(
                            NativeHandle,
                            queryDescriptors.NativeHandle,
                            maxDistance,
                            fillMasksPtr,
                            maskHandles.Length,
                            compactResult ? 1 : 0,
                            offsetsPtr,
                            groupCount + 1,
                            matchesPtr,
                            totalMatchCount,
                            out writtenGroupCount,
                            out writtenMatchCount);
                    });
                }
            }
        }
#endif

        /// <summary>
        /// Releases the native matcher.
        /// 释放 native 匹配器。
        /// </summary>
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed ? "{Disposed=True}" : "{NormType=" + NormType + ",CrossCheck=" + CrossCheck + "}";
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && handle != null)
                {
                    handle.Dispose();
                }

                disposed = true;
            }
        }

        private static void ValidateNormType(NormTypes value, string parameterName)
        {
            if (value != NormTypes.L1
                && value != NormTypes.L2
                && value != NormTypes.L2Sqr
                && value != NormTypes.Hamming
                && value != NormTypes.Hamming2)
            {
                throw new ArgumentOutOfRangeException(parameterName, "BFMatcher norm type must be L1, L2, L2Sqr, Hamming, or Hamming2.");
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
