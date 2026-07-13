using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Features2D
{
    internal static class DescriptorMatcherCoreEx
    {
        internal delegate int GetTrainDescriptorCount(IntPtr matcher, out int descriptorCount);

        internal delegate int GetTrainDescriptorClone(IntPtr matcher, int index, out IntPtr descriptor);

        internal static Mat[] GetTrainDescriptors(IntPtr matcher, GetTrainDescriptorCount getCount, GetTrainDescriptorClone getClone)
        {
            NativeException.ThrowIfError(getCount(matcher, out int descriptorCount));
            if (descriptorCount <= 0)
            {
                return Array.Empty<Mat>();
            }

            var descriptors = new Mat[descriptorCount];
            for (int i = 0; i < descriptorCount; i++)
            {
                NativeException.ThrowIfError(getClone(matcher, i, out IntPtr nativeDescriptor));
                descriptors[i] = new Mat(nativeDescriptor);
            }

            return descriptors;
        }

        internal static IntPtr[] NormalizeMaskHandles(Mat[]? masks)
        {
            if (masks == null)
            {
                throw new ArgumentNullException(nameof(masks));
            }

            IntPtr[] handles = new IntPtr[masks.Length];
            for (int i = 0; i < masks.Length; i++)
            {
                DescriptorMatcherCore.ValidateNotNull(masks[i], nameof(masks));
                handles[i] = masks[i].NativeHandle;
            }

            return handles;
        }

#if NETCOREAPP3_1_OR_GREATER
        internal static IntPtr[] NormalizeMaskHandles(ReadOnlySpan<Mat> masks)
        {
            if (masks.IsEmpty)
            {
                return Array.Empty<IntPtr>();
            }

            var handles = new IntPtr[masks.Length];
            for (int i = 0; i < masks.Length; i++)
            {
                DescriptorMatcherCore.ValidateNotNull(masks[i], nameof(masks));
                handles[i] = masks[i].NativeHandle;
            }

            return handles;
        }
#endif

        internal static byte[] ToNullTerminatedUtf8(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            int byteCount = System.Text.Encoding.UTF8.GetByteCount(value);
            var buffer = new byte[byteCount + 1];
            System.Text.Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            buffer[byteCount] = 0;
            return buffer;
        }
    }
}
