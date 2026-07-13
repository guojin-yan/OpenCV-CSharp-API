using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides FLANN-based descriptor matching compatible with <c>cv::FlannBasedMatcher</c>.
    /// 提供与 OpenCV <c>cv::FlannBasedMatcher</c> 兼容的 FLANN 描述子匹配能力。
    /// </summary>
    public sealed class FlannBasedMatcher : DescriptorMatcher
    {
        private NativeFlannBasedMatcherHandle handle;
        private bool disposed;

        private FlannBasedMatcher(IntPtr nativeHandle)
        {
            handle = NativeFlannBasedMatcherHandle.FromNativePointer(nativeHandle);
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
        /// Gets a value indicating whether descriptor masks are supported.
        /// 获取是否支持描述子掩码。
        /// </summary>
        public override bool IsMaskSupported
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherIsMaskSupported(NativeHandle, out int supported));
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
                NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherEmpty(NativeHandle, out int empty));
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
        /// Creates a FLANN-based matcher.
        /// 创建 FLANN 匹配器。
        /// </summary>
        public static FlannBasedMatcher Create()
        {
            NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherCreate(out IntPtr nativeHandle));
            return new FlannBasedMatcher(nativeHandle);
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
            NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherClone(NativeHandle, emptyTrainData ? 1 : 0, out IntPtr nativeClone));
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
                NativeMethods.Features2DFlannMatcherGetTrainDescriptorsCount,
                NativeMethods.Features2DFlannMatcherGetTrainDescriptorClone);
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
                DescriptorMatcherCore.Add(NativeHandle, descriptors, NativeMethods.Features2DFlannMatcherAdd);
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Adds train descriptors from a span-backed collection.
        /// 从 Span 支持的集合向匹配器添加训练描述子。
        /// </summary>
        public override void Add(ReadOnlySpan<Mat> descriptors)
        {
            ThrowIfDisposed();
            unsafe
            {
                DescriptorMatcherCore.Add(NativeHandle, descriptors, NativeMethods.Features2DFlannMatcherAdd);
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
            NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherClear(NativeHandle));
        }

        /// <summary>
        /// Trains the matcher.
        /// 训练匹配器。
        /// </summary>
        public override void Train()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherTrain(NativeHandle));
        }

        /// <summary>
        /// Finds the best matches between query and train descriptors.
        /// 在查询描述子和训练描述子之间查找最佳匹配。
        /// </summary>
        public override DMatch[] Match(Mat queryDescriptors, Mat trainDescriptors, Mat? mask = null)
        {
            ThrowIfDisposed();
            ThrowIfMaskProvided(mask);
            DescriptorMatcherCore.ValidateMatPair(queryDescriptors, trainDescriptors);
            NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherMatchCount(
                NativeHandle,
                queryDescriptors.NativeHandle,
                trainDescriptors.NativeHandle,
                out int matchCount));

            unsafe
            {
                return DescriptorMatcherCore.FillMatches(matchCount, delegate (NativeDMatch* matchesPtr, out int writtenCount)
                {
                    return NativeMethods.Features2DFlannMatcherMatchFill(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        trainDescriptors.NativeHandle,
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
            NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherMatchTrainCount(NativeHandle, queryDescriptors.NativeHandle, out int matchCount));
            unsafe
            {
                return DescriptorMatcherCore.FillMatches(matchCount, delegate (NativeDMatch* matchesPtr, out int writtenCount)
                {
                    return NativeMethods.Features2DFlannMatcherMatchTrainFill(NativeHandle, queryDescriptors.NativeHandle, matchesPtr, matchCount, out writtenCount);
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
            ThrowIfMasksProvided(masks);
            return Match(queryDescriptors);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <inheritdoc/>
        public override DMatch[] Match(Mat queryDescriptors, ReadOnlySpan<Mat> masks)
        {
            ThrowIfDisposed();
            ThrowIfMasksProvided(masks);
            return Match(queryDescriptors);
        }
#endif

        /// <summary>
        /// Finds k nearest matches between query and train descriptors.
        /// 在查询描述子和训练描述子之间查找 k 个最近匹配。
        /// </summary>
        public override DMatch[][] KnnMatch(Mat queryDescriptors, Mat trainDescriptors, int k, Mat? mask = null, bool compactResult = false)
        {
            ThrowIfDisposed();
            ThrowIfMaskProvided(mask);
            DescriptorMatcherCore.ValidateK(k, nameof(k));
            DescriptorMatcherCore.ValidateMatPair(queryDescriptors, trainDescriptors);
            NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherKnnMatchCount(
                NativeHandle,
                queryDescriptors.NativeHandle,
                trainDescriptors.NativeHandle,
                k,
                compactResult ? 1 : 0,
                out int groupCount,
                out int totalMatchCount));

            unsafe
            {
                return DescriptorMatcherCore.FillGroupedMatches(groupCount, totalMatchCount, delegate (int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                {
                    return NativeMethods.Features2DFlannMatcherKnnMatchFill(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        trainDescriptors.NativeHandle,
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
        /// Finds k nearest matches against the trained descriptor collection.
        /// 在已训练描述子集合中查找 k 个最近匹配。
        /// </summary>
        public override DMatch[][] KnnMatch(Mat queryDescriptors, int k, bool compactResult = false)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateK(k, nameof(k));
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherKnnMatchTrainCount(
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
                    return NativeMethods.Features2DFlannMatcherKnnMatchTrainFill(
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
            ThrowIfMasksProvided(masks);
            DescriptorMatcherCore.ValidateK(k, nameof(k));
            return KnnMatch(queryDescriptors, k, compactResult);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <inheritdoc/>
        public override DMatch[][] KnnMatch(Mat queryDescriptors, int k, ReadOnlySpan<Mat> masks, bool compactResult = false)
        {
            ThrowIfDisposed();
            ThrowIfMasksProvided(masks);
            DescriptorMatcherCore.ValidateK(k, nameof(k));
            return KnnMatch(queryDescriptors, k, compactResult);
        }
#endif

        /// <summary>
        /// Finds descriptor matches within a maximum distance.
        /// 查找最大距离以内的描述子匹配。
        /// </summary>
        public override DMatch[][] RadiusMatch(Mat queryDescriptors, Mat trainDescriptors, float maxDistance, Mat? mask = null, bool compactResult = false)
        {
            ThrowIfDisposed();
            ThrowIfMaskProvided(mask);
            DescriptorMatcherCore.ValidateMaxDistance(maxDistance, nameof(maxDistance));
            DescriptorMatcherCore.ValidateMatPair(queryDescriptors, trainDescriptors);
            NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherRadiusMatchCount(
                NativeHandle,
                queryDescriptors.NativeHandle,
                trainDescriptors.NativeHandle,
                maxDistance,
                compactResult ? 1 : 0,
                out int groupCount,
                out int totalMatchCount));

            unsafe
            {
                return DescriptorMatcherCore.FillGroupedMatches(groupCount, totalMatchCount, delegate (int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                {
                    return NativeMethods.Features2DFlannMatcherRadiusMatchFill(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        trainDescriptors.NativeHandle,
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
        /// Finds descriptor matches in the trained collection within a maximum distance.
        /// 在已训练集合中查找最大距离以内的描述子匹配。
        /// </summary>
        public override DMatch[][] RadiusMatch(Mat queryDescriptors, float maxDistance, bool compactResult = false)
        {
            ThrowIfDisposed();
            DescriptorMatcherCore.ValidateMaxDistance(maxDistance, nameof(maxDistance));
            DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            NativeException.ThrowIfError(NativeMethods.Features2DFlannMatcherRadiusMatchTrainCount(
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
                    return NativeMethods.Features2DFlannMatcherRadiusMatchTrainFill(
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
            ThrowIfMasksProvided(masks);
            DescriptorMatcherCore.ValidateMaxDistance(maxDistance, nameof(maxDistance));
            return RadiusMatch(queryDescriptors, maxDistance, compactResult);
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <inheritdoc/>
        public override DMatch[][] RadiusMatch(Mat queryDescriptors, float maxDistance, ReadOnlySpan<Mat> masks, bool compactResult = false)
        {
            ThrowIfDisposed();
            ThrowIfMasksProvided(masks);
            DescriptorMatcherCore.ValidateMaxDistance(maxDistance, nameof(maxDistance));
            return RadiusMatch(queryDescriptors, maxDistance, compactResult);
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
            return disposed ? "{Disposed=True}" : "{Empty=" + Empty + ",IsMaskSupported=" + IsMaskSupported + "}";
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

        private static void ThrowIfMaskProvided(Mat? mask)
        {
            if (mask != null)
            {
                throw new NotSupportedException("FlannBasedMatcher does not support descriptor masks.");
            }
        }

        private static void ThrowIfMasksProvided(Mat[] masks)
        {
            IntPtr[] maskHandles = DescriptorMatcherCoreEx.NormalizeMaskHandles(masks);
            if (maskHandles.Length > 0)
            {
                throw new NotSupportedException("FlannBasedMatcher does not support descriptor masks.");
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private static void ThrowIfMasksProvided(ReadOnlySpan<Mat> masks)
        {
            IntPtr[] maskHandles = DescriptorMatcherCoreEx.NormalizeMaskHandles(masks);
            if (maskHandles.Length > 0)
            {
                throw new NotSupportedException("FlannBasedMatcher does not support descriptor masks.");
            }
        }
#endif

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
