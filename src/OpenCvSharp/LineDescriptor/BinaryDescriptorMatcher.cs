using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.LineDescriptor
{
    /// <summary>
    /// Matches line_descriptor binary descriptor matrices.
    /// 匹配 line_descriptor 二进制描述子矩阵。
    /// </summary>
    public sealed class BinaryDescriptorMatcher : IDisposable
    {
        private NativeLineDescriptorBinaryDescriptorMatcherHandle handle;
        private bool disposed;

        private BinaryDescriptorMatcher(IntPtr nativeHandle)
        {
            handle = NativeLineDescriptorBinaryDescriptorMatcherHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this matcher has been disposed. 获取匹配器是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets whether the native matcher is empty. 获取 native 匹配器是否为空。</summary>
        public bool Empty
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorMatcherEmpty(NativeHandle, out int empty));
                return empty != 0;
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

        /// <summary>
        /// Creates a binary descriptor matcher.
        /// 创建二进制描述子匹配器。
        /// </summary>
        public static BinaryDescriptorMatcher Create()
        {
            NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorMatcherCreate(out IntPtr nativeHandle));
            return new BinaryDescriptorMatcher(nativeHandle);
        }

        /// <summary>Clears native matcher state. 清除 native 匹配器状态。</summary>
        public void Clear()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorMatcherClear(NativeHandle));
        }

        /// <summary>
        /// Finds the best matches between query and train descriptor matrices.
        /// 在查询和训练描述子矩阵之间查找最佳匹配。
        /// </summary>
        public DMatch[] Match(Mat queryDescriptors, Mat trainDescriptors, Mat? mask = null)
        {
            ThrowIfDisposed();
            ValidateMatPair(queryDescriptors, trainDescriptors);
            NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorMatcherMatchCount(
                NativeHandle,
                queryDescriptors.NativeHandle,
                trainDescriptors.NativeHandle,
                OptionalHandle(mask),
                out int matchCount));

            unsafe
            {
                return DescriptorMatcherCore.FillMatches(matchCount, delegate (NativeDMatch* matchesPtr, out int writtenCount)
                {
                    return NativeMethods.LineDescriptorBinaryDescriptorMatcherMatchFill(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        trainDescriptors.NativeHandle,
                        OptionalHandle(mask),
                        matchesPtr,
                        matchCount,
                        out writtenCount);
                });
            }
        }

        /// <summary>
        /// Finds k nearest matches between query and train descriptor matrices.
        /// 在查询和训练描述子矩阵之间查找 k 个最近匹配。
        /// </summary>
        public DMatch[][] KnnMatch(Mat queryDescriptors, Mat trainDescriptors, int k, Mat? mask = null, bool compactResult = false)
        {
            ThrowIfDisposed();
            ValidateMatPair(queryDescriptors, trainDescriptors);
            if (k <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(k));
            }

            NativeException.ThrowIfError(NativeMethods.LineDescriptorBinaryDescriptorMatcherKnnMatchCount(
                NativeHandle,
                queryDescriptors.NativeHandle,
                trainDescriptors.NativeHandle,
                k,
                OptionalHandle(mask),
                compactResult ? 1 : 0,
                out int groupCount,
                out int totalMatchCount));

            unsafe
            {
                return DescriptorMatcherCore.FillGroupedMatches(groupCount, totalMatchCount, delegate (int* offsetsPtr, NativeDMatch* matchesPtr, out int writtenGroupCount, out int writtenMatchCount)
                {
                    return NativeMethods.LineDescriptorBinaryDescriptorMatcherKnnMatchFill(
                        NativeHandle,
                        queryDescriptors.NativeHandle,
                        trainDescriptors.NativeHandle,
                        k,
                        OptionalHandle(mask),
                        compactResult ? 1 : 0,
                        offsetsPtr,
                        groupCount + 1,
                        matchesPtr,
                        Math.Max(totalMatchCount, 1),
                        out writtenGroupCount,
                        out writtenMatchCount);
                });
            }
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed ? "{Disposed=True}" : "{Empty=" + Empty + "}";
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

        private static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private static void ValidateMatPair(Mat queryDescriptors, Mat trainDescriptors)
        {
            ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            ValidateNotNull(trainDescriptors, nameof(trainDescriptors));
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
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
