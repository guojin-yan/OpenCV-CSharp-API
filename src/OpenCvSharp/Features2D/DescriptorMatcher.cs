using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Features2D
{
    /// <summary>
    /// Provides a base type for OpenCV descriptor matchers compatible with <c>cv::DescriptorMatcher</c>.
    /// 提供与 OpenCV <c>cv::DescriptorMatcher</c> 兼容的描述子匹配器基类型。
    /// </summary>
    public abstract class DescriptorMatcher : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this matcher has been disposed.
        /// 获取此匹配器是否已经释放。
        /// </summary>
        public abstract bool IsDisposed { get; }

        /// <summary>
        /// Gets a value indicating whether descriptor masks are supported.
        /// 获取是否支持描述子掩码。
        /// </summary>
        public abstract bool IsMaskSupported { get; }

        /// <summary>
        /// Gets a value indicating whether the train descriptor collection is empty.
        /// 获取训练描述子集合是否为空。
        /// </summary>
        public abstract bool Empty { get; }

        internal abstract IntPtr NativeHandle { get; }

        /// <summary>
        /// Creates a descriptor matcher by OpenCV factory type.
        /// 按 OpenCV 工厂类型创建描述子匹配器。
        /// </summary>
        /// <param name="matcherType">The matcher factory type. 匹配器工厂类型。</param>
        /// <returns>The created matcher. 创建的匹配器。</returns>
        public static DescriptorMatcher Create(DescriptorMatcherType matcherType)
        {
            ValidateMatcherType(matcherType, nameof(matcherType));
            NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherCreateByType((int)matcherType, out IntPtr nativeHandle));
            return FromNativeHandle(nativeHandle);
        }

        /// <summary>
        /// Creates a descriptor matcher by OpenCV factory name.
        /// 按 OpenCV 工厂名称创建描述子匹配器。
        /// </summary>
        /// <param name="matcherName">The matcher factory name, such as <c>BruteForce</c> or <c>FlannBased</c>. 匹配器工厂名称，例如 <c>BruteForce</c> 或 <c>FlannBased</c>。</param>
        /// <returns>The created matcher. 创建的匹配器。</returns>
        public static unsafe DescriptorMatcher Create(string matcherName)
        {
            byte[] nameBytes = DescriptorMatcherCoreEx.ToNullTerminatedUtf8(matcherName, nameof(matcherName));
            fixed (byte* namePtr = nameBytes)
            {
                NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherCreateByName(namePtr, nameBytes.Length - 1, out IntPtr nativeHandle));
                return FromNativeHandle(nativeHandle);
            }
        }

        internal static DescriptorMatcher FromNativeHandle(IntPtr nativeHandle)
        {
            return new NativeDescriptorMatcher(nativeHandle);
        }

        private static void ValidateMatcherType(DescriptorMatcherType value, string parameterName)
        {
            if (value != DescriptorMatcherType.FlannBased
                && value != DescriptorMatcherType.BruteForce
                && value != DescriptorMatcherType.BruteForceL1
                && value != DescriptorMatcherType.BruteForceHamming
                && value != DescriptorMatcherType.BruteForceHammingLut
                && value != DescriptorMatcherType.BruteForceSL2)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Descriptor matcher type must be a defined matcher type.");
            }
        }

        /// <summary>
        /// Clones the matcher.
        /// 克隆匹配器。
        /// </summary>
        /// <param name="emptyTrainData">Whether to omit train descriptors from the clone. 是否在克隆对象中省略训练描述子。</param>
        /// <returns>The cloned matcher. 克隆后的匹配器。</returns>
        public abstract DescriptorMatcher Clone(bool emptyTrainData = false);

        /// <summary>
        /// Gets cloned train descriptor matrices.
        /// 获取训练描述子矩阵的克隆集合。
        /// </summary>
        /// <returns>The train descriptors. 训练描述子集合。</returns>
        public abstract Mat[] GetTrainDescriptors();

        /// <summary>
        /// Adds train descriptors to the matcher collection.
        /// 向匹配器训练集合添加描述子。
        /// </summary>
        /// <param name="descriptors">The descriptor matrices to add. 要添加的描述子矩阵。</param>
        public abstract void Add(Mat[] descriptors);

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Adds train descriptors from a span-backed collection.
        /// 从 Span 支持的集合向匹配器添加训练描述子。
        /// </summary>
        /// <param name="descriptors">The descriptor matrices to add. 要添加的描述子矩阵。</param>
        public abstract void Add(ReadOnlySpan<Mat> descriptors);
#endif

        /// <summary>
        /// Clears the train descriptor collection.
        /// 清除训练描述子集合。
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Trains the matcher.
        /// 训练匹配器。
        /// </summary>
        public abstract void Train();

        /// <summary>
        /// Finds the best matches between query and train descriptors.
        /// 在查询描述子和训练描述子之间查找最佳匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="trainDescriptors">The train descriptors. 训练描述子。</param>
        /// <param name="mask">The optional permissible-match mask. 可选的允许匹配掩码。</param>
        /// <returns>The best matches. 最佳匹配结果。</returns>
        public abstract DMatch[] Match(Mat queryDescriptors, Mat trainDescriptors, Mat? mask = null);

        /// <summary>
        /// Finds the best matches against the trained descriptor collection.
        /// 在已训练描述子集合中查找最佳匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <returns>The best matches. 最佳匹配结果。</returns>
        public abstract DMatch[] Match(Mat queryDescriptors);

        /// <summary>
        /// Finds the best matches against the trained descriptor collection using per-train masks.
        /// 使用每个训练描述子集合对应的掩码，在已训练描述子集合中查找最佳匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="masks">The optional masks, one per train descriptor matrix. 可选掩码，每个训练描述子矩阵一个。</param>
        /// <returns>The best matches. 最佳匹配结果。</returns>
        public abstract DMatch[] Match(Mat queryDescriptors, Mat[] masks);

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Finds the best matches against the trained descriptor collection using span-backed per-train masks.
        /// 使用 Span 支持的每个训练描述子集合掩码，在已训练描述子集合中查找最佳匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="masks">The optional masks, one per train descriptor matrix. 可选掩码，每个训练描述子矩阵一个。</param>
        /// <returns>The best matches. 最佳匹配结果。</returns>
        public abstract DMatch[] Match(Mat queryDescriptors, ReadOnlySpan<Mat> masks);
#endif

        /// <summary>
        /// Finds k nearest matches between query and train descriptors.
        /// 在查询描述子和训练描述子之间查找 k 个最近匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="trainDescriptors">The train descriptors. 训练描述子。</param>
        /// <param name="k">The maximum number of matches per descriptor. 每个描述子的最大匹配数。</param>
        /// <param name="mask">The optional permissible-match mask. 可选的允许匹配掩码。</param>
        /// <param name="compactResult">Whether to compact fully masked-out rows. 是否压缩完全被掩码排除的行。</param>
        /// <returns>The grouped matches. 分组匹配结果。</returns>
        public abstract DMatch[][] KnnMatch(Mat queryDescriptors, Mat trainDescriptors, int k, Mat? mask = null, bool compactResult = false);

        /// <summary>
        /// Finds k nearest matches against the trained descriptor collection.
        /// 在已训练描述子集合中查找 k 个最近匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="k">The maximum number of matches per descriptor. 每个描述子的最大匹配数。</param>
        /// <param name="compactResult">Whether to compact fully masked-out rows. 是否压缩完全被掩码排除的行。</param>
        /// <returns>The grouped matches. 分组匹配结果。</returns>
        public abstract DMatch[][] KnnMatch(Mat queryDescriptors, int k, bool compactResult = false);

        /// <summary>
        /// Finds k nearest matches against the trained descriptor collection using per-train masks.
        /// 使用每个训练描述子集合对应的掩码，在已训练集合中查找 k 个最近匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="k">The maximum number of matches per descriptor. 每个描述子的最大匹配数。</param>
        /// <param name="masks">The optional masks, one per train descriptor matrix. 可选掩码，每个训练描述子矩阵一个。</param>
        /// <param name="compactResult">Whether to compact fully masked-out rows. 是否压缩完全被掩码排除的行。</param>
        /// <returns>The grouped matches. 分组匹配结果。</returns>
        public abstract DMatch[][] KnnMatch(Mat queryDescriptors, int k, Mat[] masks, bool compactResult = false);

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Finds k nearest matches against the trained descriptor collection using span-backed per-train masks.
        /// 使用 Span 支持的每个训练描述子集合掩码，在已训练集合中查找 k 个最近匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="k">The maximum number of matches per descriptor. 每个描述子的最大匹配数。</param>
        /// <param name="masks">The optional masks, one per train descriptor matrix. 可选掩码，每个训练描述子矩阵一个。</param>
        /// <param name="compactResult">Whether to compact fully masked-out rows. 是否压缩完全被掩码排除的行。</param>
        /// <returns>The grouped matches. 分组匹配结果。</returns>
        public abstract DMatch[][] KnnMatch(Mat queryDescriptors, int k, ReadOnlySpan<Mat> masks, bool compactResult = false);
#endif

        /// <summary>
        /// Finds descriptor matches within a maximum distance.
        /// 查找最大距离以内的描述子匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="trainDescriptors">The train descriptors. 训练描述子。</param>
        /// <param name="maxDistance">The maximum descriptor distance. 最大描述子距离。</param>
        /// <param name="mask">The optional permissible-match mask. 可选的允许匹配掩码。</param>
        /// <param name="compactResult">Whether to compact fully masked-out rows. 是否压缩完全被掩码排除的行。</param>
        /// <returns>The grouped matches. 分组匹配结果。</returns>
        public abstract DMatch[][] RadiusMatch(Mat queryDescriptors, Mat trainDescriptors, float maxDistance, Mat? mask = null, bool compactResult = false);

        /// <summary>
        /// Finds descriptor matches in the trained collection within a maximum distance.
        /// 在已训练集合中查找最大距离以内的描述子匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="maxDistance">The maximum descriptor distance. 最大描述子距离。</param>
        /// <param name="compactResult">Whether to compact fully masked-out rows. 是否压缩完全被掩码排除的行。</param>
        /// <returns>The grouped matches. 分组匹配结果。</returns>
        public abstract DMatch[][] RadiusMatch(Mat queryDescriptors, float maxDistance, bool compactResult = false);

        /// <summary>
        /// Finds descriptor matches in the trained collection within a maximum distance using per-train masks.
        /// 使用每个训练描述子集合对应的掩码，在已训练集合中查找最大距离以内的描述子匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="maxDistance">The maximum descriptor distance. 最大描述子距离。</param>
        /// <param name="masks">The optional masks, one per train descriptor matrix. 可选掩码，每个训练描述子矩阵一个。</param>
        /// <param name="compactResult">Whether to compact fully masked-out rows. 是否压缩完全被掩码排除的行。</param>
        /// <returns>The grouped matches. 分组匹配结果。</returns>
        public abstract DMatch[][] RadiusMatch(Mat queryDescriptors, float maxDistance, Mat[] masks, bool compactResult = false);

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Finds descriptor matches in the trained collection within a maximum distance using span-backed per-train masks.
        /// 使用 Span 支持的每个训练描述子集合掩码，在已训练集合中查找最大距离以内的描述子匹配。
        /// </summary>
        /// <param name="queryDescriptors">The query descriptors. 查询描述子。</param>
        /// <param name="maxDistance">The maximum descriptor distance. 最大描述子距离。</param>
        /// <param name="masks">The optional masks, one per train descriptor matrix. 可选掩码，每个训练描述子矩阵一个。</param>
        /// <param name="compactResult">Whether to compact fully masked-out rows. 是否压缩完全被掩码排除的行。</param>
        /// <returns>The grouped matches. 分组匹配结果。</returns>
        public abstract DMatch[][] RadiusMatch(Mat queryDescriptors, float maxDistance, ReadOnlySpan<Mat> masks, bool compactResult = false);
#endif

        /// <summary>
        /// Releases resources used by this matcher.
        /// 释放此匹配器使用的资源。
        /// </summary>
        public abstract void Dispose();

        private sealed class NativeDescriptorMatcher : DescriptorMatcher
        {
            private NativeDescriptorMatcherHandle handle;
            private bool disposed;

            internal NativeDescriptorMatcher(IntPtr nativeHandle)
            {
                handle = NativeDescriptorMatcherHandle.FromNativePointer(nativeHandle);
            }

            public override bool IsDisposed
            {
                get { return disposed; }
            }

            public override bool IsMaskSupported
            {
                get
                {
                    ThrowIfDisposed();
                    NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherIsMaskSupported(NativeHandle, out int supported));
                    return supported != 0;
                }
            }

            public override bool Empty
            {
                get
                {
                    ThrowIfDisposed();
                    NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherEmpty(NativeHandle, out int empty));
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

            public override DescriptorMatcher Clone(bool emptyTrainData = false)
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherClone(NativeHandle, emptyTrainData ? 1 : 0, out IntPtr nativeClone));
                return FromNativeHandle(nativeClone);
            }

            public override Mat[] GetTrainDescriptors()
            {
                ThrowIfDisposed();
                return DescriptorMatcherCoreEx.GetTrainDescriptors(
                    NativeHandle,
                    NativeMethods.Features2DDescriptorMatcherGetTrainDescriptorsCount,
                    NativeMethods.Features2DDescriptorMatcherGetTrainDescriptorClone);
            }

            public override void Add(Mat[] descriptors)
            {
                ThrowIfDisposed();
                unsafe
                {
                    DescriptorMatcherCore.Add(NativeHandle, descriptors, NativeMethods.Features2DDescriptorMatcherAdd);
                }
            }

#if NETCOREAPP3_1_OR_GREATER
            public override void Add(ReadOnlySpan<Mat> descriptors)
            {
                ThrowIfDisposed();
                unsafe
                {
                    DescriptorMatcherCore.Add(NativeHandle, descriptors, NativeMethods.Features2DDescriptorMatcherAdd);
                }
            }
#endif

            public override void Clear()
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherClear(NativeHandle));
            }

            public override void Train()
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherTrain(NativeHandle));
            }

            public override DMatch[] Match(Mat queryDescriptors, Mat trainDescriptors, Mat? mask = null)
            {
                ThrowIfDisposed();
                DescriptorMatcherCore.ValidateMatPair(queryDescriptors, trainDescriptors);
                NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherMatchCount(
                    NativeHandle,
                    queryDescriptors.NativeHandle,
                    trainDescriptors.NativeHandle,
                    DescriptorMatcherCore.OptionalHandle(mask),
                    out int matchCount));

                unsafe
                {
                    return DescriptorMatcherCore.FillMatches(matchCount, delegate (NativeDMatch* matchesPtr, out int writtenCount)
                    {
                        return NativeMethods.Features2DDescriptorMatcherMatchFill(
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

            public override DMatch[] Match(Mat queryDescriptors)
            {
                return Match(queryDescriptors, Array.Empty<Mat>());
            }

            public override DMatch[] Match(Mat queryDescriptors, Mat[] masks)
            {
                ThrowIfDisposed();
                DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
                IntPtr[] maskHandles = DescriptorMatcherCoreEx.NormalizeMaskHandles(masks);
                unsafe
                {
                    fixed (IntPtr* masksPtr = maskHandles)
                    {
                        NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherMatchTrainCount(
                            NativeHandle,
                            queryDescriptors.NativeHandle,
                            masksPtr,
                            maskHandles.Length,
                            out int matchCount));

                        return DescriptorMatcherCore.FillMatchesWithMasks(matchCount, maskHandles, delegate (IntPtr* fillMasksPtr, NativeDMatch* matchesPtr, out int writtenCount)
                        {
                            return NativeMethods.Features2DDescriptorMatcherMatchTrainFill(
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
            public override DMatch[] Match(Mat queryDescriptors, ReadOnlySpan<Mat> masks)
            {
                ThrowIfDisposed();
                DescriptorMatcherCore.ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
                IntPtr[] maskHandles = DescriptorMatcherCoreEx.NormalizeMaskHandles(masks);
                unsafe
                {
                    fixed (IntPtr* masksPtr = maskHandles)
                    {
                        NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherMatchTrainCount(
                            NativeHandle,
                            queryDescriptors.NativeHandle,
                            masksPtr,
                            maskHandles.Length,
                            out int matchCount));

                        return DescriptorMatcherCore.FillMatchesWithMasks(matchCount, maskHandles, delegate (IntPtr* fillMasksPtr, NativeDMatch* matchesPtr, out int writtenCount)
                        {
                            return NativeMethods.Features2DDescriptorMatcherMatchTrainFill(
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

            public override DMatch[][] KnnMatch(Mat queryDescriptors, Mat trainDescriptors, int k, Mat? mask = null, bool compactResult = false)
            {
                ThrowIfDisposed();
                DescriptorMatcherCore.ValidateK(k, nameof(k));
                DescriptorMatcherCore.ValidateMatPair(queryDescriptors, trainDescriptors);
                NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherKnnMatchCount(
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
                        return NativeMethods.Features2DDescriptorMatcherKnnMatchFill(
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

            public override DMatch[][] KnnMatch(Mat queryDescriptors, int k, bool compactResult = false)
            {
                return KnnMatch(queryDescriptors, k, Array.Empty<Mat>(), compactResult);
            }

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
                        NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherKnnMatchTrainCount(
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
                            return NativeMethods.Features2DDescriptorMatcherKnnMatchTrainFill(
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
                        NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherKnnMatchTrainCount(
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
                            return NativeMethods.Features2DDescriptorMatcherKnnMatchTrainFill(
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

            public override DMatch[][] RadiusMatch(Mat queryDescriptors, Mat trainDescriptors, float maxDistance, Mat? mask = null, bool compactResult = false)
            {
                ThrowIfDisposed();
                DescriptorMatcherCore.ValidateMaxDistance(maxDistance, nameof(maxDistance));
                DescriptorMatcherCore.ValidateMatPair(queryDescriptors, trainDescriptors);
                NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherRadiusMatchCount(
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
                        return NativeMethods.Features2DDescriptorMatcherRadiusMatchFill(
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

            public override DMatch[][] RadiusMatch(Mat queryDescriptors, float maxDistance, bool compactResult = false)
            {
                return RadiusMatch(queryDescriptors, maxDistance, Array.Empty<Mat>(), compactResult);
            }

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
                        NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherRadiusMatchTrainCount(
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
                            return NativeMethods.Features2DDescriptorMatcherRadiusMatchTrainFill(
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
                        NativeException.ThrowIfError(NativeMethods.Features2DDescriptorMatcherRadiusMatchTrainCount(
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
                            return NativeMethods.Features2DDescriptorMatcherRadiusMatchTrainFill(
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

            public override void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

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

            private void ThrowIfDisposed()
            {
                if (disposed)
                {
                    throw new ObjectDisposedException(GetType().FullName);
                }
            }
        }
    }
}
