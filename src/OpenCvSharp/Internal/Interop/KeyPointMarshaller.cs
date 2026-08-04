using System;
using JYPPX.OpenCvSharp.Features2D;

#if NETCOREAPP3_1_OR_GREATER
using System.Buffers;
#endif

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static class KeyPointMarshaller
    {
#if NETCOREAPP3_1_OR_GREATER
        internal const int StackallocThreshold = 64;
#endif

        internal static NativeKeyPoint[] ToNative(KeyPoint[] keypoints)
        {
            if (keypoints == null)
            {
                throw new ArgumentNullException(nameof(keypoints));
            }

            var result = new NativeKeyPoint[keypoints.Length];
            for (int i = 0; i < keypoints.Length; i++)
            {
                result[i] = keypoints[i].ToNative();
            }

            return result;
        }

        internal static KeyPoint[] FromNative(NativeKeyPoint[] keypoints, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<KeyPoint>();
            }

            var result = new KeyPoint[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = KeyPoint.FromNative(keypoints[i]);
            }

            return result;
        }

#if NETCOREAPP3_1_OR_GREATER
        internal static void CopyToNative(ReadOnlySpan<KeyPoint> source, Span<NativeKeyPoint> destination)
        {
            for (int i = 0; i < source.Length; i++)
            {
                destination[i] = source[i].ToNative();
            }
        }

        internal static KeyPoint[] FromNative(ReadOnlySpan<NativeKeyPoint> keypoints, int count)
        {
            if (count <= 0)
            {
                return Array.Empty<KeyPoint>();
            }

            var result = new KeyPoint[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = KeyPoint.FromNative(keypoints[i]);
            }

            return result;
        }

        internal static NativeKeyPoint[]? RentOrStack(ReadOnlySpan<KeyPoint> source, Span<NativeKeyPoint> destination)
        {
            CopyToNative(source, destination);
            return null;
        }
#endif
    }
}
