using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

#if NETCOREAPP3_1_OR_GREATER
using System.Buffers;
#endif

namespace JYPPX.OpenCvSharp.Features2D
{
    internal static unsafe class DescriptorMatcherCore
    {
#if NETCOREAPP3_1_OR_GREATER
        private const int StackallocHandleThreshold = 32;
#endif

        internal delegate int AddNative(IntPtr matcher, IntPtr* descriptors, int descriptorCount);

        internal delegate int MatchFill(NativeDMatch* matches, out int matchCount);

        internal delegate int GroupedMatchFill(int* offsets, NativeDMatch* matches, out int groupCount, out int totalMatchCount);

        internal delegate int MaskedMatchFill(IntPtr* masks, NativeDMatch* matches, out int matchCount);

        internal delegate int MaskedGroupedMatchFill(IntPtr* masks, int* offsets, NativeDMatch* matches, out int groupCount, out int totalMatchCount);

        internal static void Add(IntPtr matcher, Mat[] descriptors, AddNative add)
        {
            ValidateNonNullArray(descriptors, nameof(descriptors));

            IntPtr[] handles = new IntPtr[descriptors.Length];
            for (int i = 0; i < descriptors.Length; i++)
            {
                handles[i] = descriptors[i].NativeHandle;
            }

            fixed (IntPtr* handlesPtr = handles)
            {
                NativeException.ThrowIfError(add(matcher, handlesPtr, handles.Length));
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        internal static void Add(IntPtr matcher, ReadOnlySpan<Mat> descriptors, AddNative add)
        {
            if (descriptors.IsEmpty)
            {
                NativeException.ThrowIfError(add(matcher, null, 0));
                return;
            }

            IntPtr[]? rentedHandles = null;
            Span<IntPtr> handles = descriptors.Length <= StackallocHandleThreshold
                ? stackalloc IntPtr[descriptors.Length]
                : (rentedHandles = ArrayPool<IntPtr>.Shared.Rent(descriptors.Length)).AsSpan(0, descriptors.Length);

            try
            {
                for (int i = 0; i < descriptors.Length; i++)
                {
                    ValidateNotNull(descriptors[i], nameof(descriptors));
                    handles[i] = descriptors[i].NativeHandle;
                }

                fixed (IntPtr* handlesPtr = handles)
                {
                    NativeException.ThrowIfError(add(matcher, handlesPtr, handles.Length));
                }
            }
            finally
            {
                if (rentedHandles != null)
                {
                    ArrayPool<IntPtr>.Shared.Return(rentedHandles);
                }
            }
        }
#endif

        internal static DMatch[] FillMatches(int matchCount, MatchFill fill)
        {
            if (matchCount <= 0)
            {
                return Array.Empty<DMatch>();
            }

            var nativeMatches = new NativeDMatch[matchCount];
            fixed (NativeDMatch* matchesPtr = nativeMatches)
            {
                NativeException.ThrowIfError(fill(matchesPtr, out int writtenCount));
                return DMatchMarshaller.FromNative(nativeMatches, writtenCount);
            }
        }

        internal static DMatch[] FillMatchesWithMasks(int matchCount, IntPtr[] maskHandles, MaskedMatchFill fill)
        {
            if (matchCount <= 0)
            {
                return Array.Empty<DMatch>();
            }

            var nativeMatches = new NativeDMatch[matchCount];
            fixed (IntPtr* masksPtr = maskHandles)
            fixed (NativeDMatch* matchesPtr = nativeMatches)
            {
                NativeException.ThrowIfError(fill(masksPtr, matchesPtr, out int writtenCount));
                return DMatchMarshaller.FromNative(nativeMatches, writtenCount);
            }
        }

        internal static DMatch[][] FillGroupedMatches(int groupCount, int totalMatchCount, GroupedMatchFill fill)
        {
            if (groupCount <= 0)
            {
                return Array.Empty<DMatch[]>();
            }

            var offsets = new int[groupCount + 1];
            var nativeMatches = new NativeDMatch[Math.Max(totalMatchCount, 1)];
            fixed (int* offsetsPtr = offsets)
            fixed (NativeDMatch* matchesPtr = nativeMatches)
            {
                NativeException.ThrowIfError(fill(offsetsPtr, matchesPtr, out int writtenGroupCount, out int writtenMatchCount));
                return DMatchMarshaller.FromGroupedNative(offsets, writtenGroupCount, nativeMatches, writtenMatchCount);
            }
        }

        internal static DMatch[][] FillGroupedMatchesWithMasks(int groupCount, int totalMatchCount, IntPtr[] maskHandles, MaskedGroupedMatchFill fill)
        {
            if (groupCount <= 0)
            {
                return Array.Empty<DMatch[]>();
            }

            var offsets = new int[groupCount + 1];
            var nativeMatches = new NativeDMatch[Math.Max(totalMatchCount, 1)];
            fixed (IntPtr* masksPtr = maskHandles)
            fixed (int* offsetsPtr = offsets)
            fixed (NativeDMatch* matchesPtr = nativeMatches)
            {
                NativeException.ThrowIfError(fill(masksPtr, offsetsPtr, matchesPtr, out int writtenGroupCount, out int writtenMatchCount));
                return DMatchMarshaller.FromGroupedNative(offsets, writtenGroupCount, nativeMatches, writtenMatchCount);
            }
        }

        internal static IntPtr OptionalHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        internal static void ValidateMatPair(Mat queryDescriptors, Mat trainDescriptors)
        {
            ValidateNotNull(queryDescriptors, nameof(queryDescriptors));
            ValidateNotNull(trainDescriptors, nameof(trainDescriptors));
        }

        internal static void ValidateK(int k, string parameterName)
        {
            if (k <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "KNN match count must be positive.");
            }
        }

        internal static void ValidateMaxDistance(float maxDistance, string parameterName)
        {
            if (maxDistance < 0.0F || float.IsNaN(maxDistance) || float.IsInfinity(maxDistance))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Maximum descriptor distance must be finite and non-negative.");
            }
        }

        internal static void ValidateNonNullArray<T>(T[] values, string parameterName)
            where T : class
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] == null)
                {
                    throw new ArgumentNullException(parameterName);
                }
            }
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}
