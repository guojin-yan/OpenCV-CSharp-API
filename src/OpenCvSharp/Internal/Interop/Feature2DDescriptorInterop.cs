using System;
using JYPPX.OpenCvSharp.Features2D;

#if NETCOREAPP3_1_OR_GREATER
using System.Buffers;
#endif

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe class Feature2DDescriptorInterop
    {
#if NETCOREAPP3_1_OR_GREATER
        private const int StackallocKeyPointThreshold = 64;
#endif

        internal static KeyPoint[] Compute(
            IntPtr nativeHandle,
            IntPtr imageHandle,
            KeyPoint[] keypoints,
            IntPtr descriptorsHandle,
            ComputeDelegate compute)
        {
            if (keypoints == null)
            {
                throw new ArgumentNullException(nameof(keypoints));
            }

            return ComputeCore(nativeHandle, imageHandle, keypoints, descriptorsHandle, compute);
        }

#if NETCOREAPP3_1_OR_GREATER
        internal static KeyPoint[] Compute(
            IntPtr nativeHandle,
            IntPtr imageHandle,
            ReadOnlySpan<KeyPoint> keypoints,
            IntPtr descriptorsHandle,
            ComputeDelegate compute)
        {
            return ComputeCore(nativeHandle, imageHandle, keypoints, descriptorsHandle, compute);
        }
#endif

        internal static KeyPoint[] DetectAndCompute(
            IntPtr nativeHandle,
            IntPtr imageHandle,
            IntPtr maskHandle,
            KeyPoint[] keypoints,
            IntPtr descriptorsHandle,
            bool useProvidedKeypoints,
            DetectAndComputeCountDelegate count,
            DetectAndComputeFillDelegate fill)
        {
            if (keypoints == null)
            {
                throw new ArgumentNullException(nameof(keypoints));
            }

            return DetectAndComputeCore(nativeHandle, imageHandle, maskHandle, keypoints, descriptorsHandle, useProvidedKeypoints, count, fill);
        }

#if NETCOREAPP3_1_OR_GREATER
        internal static KeyPoint[] DetectAndCompute(
            IntPtr nativeHandle,
            IntPtr imageHandle,
            IntPtr maskHandle,
            ReadOnlySpan<KeyPoint> keypoints,
            IntPtr descriptorsHandle,
            bool useProvidedKeypoints,
            DetectAndComputeCountDelegate count,
            DetectAndComputeFillDelegate fill)
        {
            return DetectAndComputeCore(nativeHandle, imageHandle, maskHandle, keypoints, descriptorsHandle, useProvidedKeypoints, count, fill);
        }
#endif

        internal delegate int ComputeDelegate(IntPtr nativeHandle, IntPtr imageHandle, NativeKeyPoint* keypointsIn, int keypointCount, NativeKeyPoint* keypointsOut, int keypointCapacity, out int writtenKeypointCount, IntPtr descriptorsHandle);

        internal delegate int DetectCountDelegate(IntPtr nativeHandle, IntPtr imageHandle, IntPtr maskHandle, out int keypointCount);

        internal delegate int DetectFillDelegate(IntPtr nativeHandle, IntPtr imageHandle, IntPtr maskHandle, NativeKeyPoint* keypoints, int keypointCapacity, out int keypointCount);

        internal delegate int DetectAndComputeCountDelegate(IntPtr nativeHandle, IntPtr imageHandle, IntPtr maskHandle, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, out int outputKeypointCount);

        internal delegate int DetectAndComputeFillDelegate(IntPtr nativeHandle, IntPtr imageHandle, IntPtr maskHandle, NativeKeyPoint* keypointsIn, int keypointCount, int useProvidedKeypoints, NativeKeyPoint* keypointsOut, int keypointCapacity, out int outputKeypointCount, IntPtr descriptorsHandle);

        internal static KeyPoint[] Detect(
            IntPtr nativeHandle,
            IntPtr imageHandle,
            IntPtr maskHandle,
            DetectCountDelegate count,
            DetectFillDelegate fill)
        {
            NativeException.ThrowIfError(count(nativeHandle, imageHandle, maskHandle, out int keypointCount));
            if (keypointCount <= 0)
            {
                return Array.Empty<KeyPoint>();
            }

            var native = new NativeKeyPoint[keypointCount];
            unsafe
            {
                fixed (NativeKeyPoint* nativePtr = native)
                {
                    NativeException.ThrowIfError(fill(nativeHandle, imageHandle, maskHandle, nativePtr, native.Length, out int writtenCount));
                    return KeyPointMarshaller.FromNative(native, writtenCount);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private unsafe static KeyPoint[] DetectAndComputeCore(
            IntPtr nativeHandle,
            IntPtr imageHandle,
            IntPtr maskHandle,
            ReadOnlySpan<KeyPoint> keypoints,
            IntPtr descriptorsHandle,
            bool useProvidedKeypoints,
            DetectAndComputeCountDelegate count,
            DetectAndComputeFillDelegate fill)
        {
            NativeKeyPoint[]? rentedInput = null;
            Span<NativeKeyPoint> nativeInput = keypoints.Length <= StackallocKeyPointThreshold
                ? stackalloc NativeKeyPoint[keypoints.Length]
                : (rentedInput = ArrayPool<NativeKeyPoint>.Shared.Rent(keypoints.Length)).AsSpan(0, keypoints.Length);

            try
            {
                KeyPointMarshaller.CopyToNative(keypoints, nativeInput);
                fixed (NativeKeyPoint* inputPtr = nativeInput)
                {
                    NativeException.ThrowIfError(count(nativeHandle, imageHandle, maskHandle, inputPtr, keypoints.Length, useProvidedKeypoints ? 1 : 0, out int outputCount));

                    var nativeOutput = new NativeKeyPoint[Math.Max(outputCount, 1)];
                    fixed (NativeKeyPoint* outputPtr = nativeOutput)
                    {
                        NativeException.ThrowIfError(fill(nativeHandle, imageHandle, maskHandle, inputPtr, keypoints.Length, useProvidedKeypoints ? 1 : 0, outputPtr, nativeOutput.Length, out int writtenCount, descriptorsHandle));
                        return KeyPointMarshaller.FromNative(nativeOutput, writtenCount);
                    }
                }
            }
            finally
            {
                if (rentedInput != null)
                {
                    ArrayPool<NativeKeyPoint>.Shared.Return(rentedInput);
                }
            }
        }
#endif

        private unsafe static KeyPoint[] ComputeCore(
            IntPtr nativeHandle,
            IntPtr imageHandle,
            KeyPoint[] keypoints,
            IntPtr descriptorsHandle,
            ComputeDelegate compute)
        {
            NativeKeyPoint[] nativeInput = KeyPointMarshaller.ToNative(keypoints);
            var nativeOutput = new NativeKeyPoint[Math.Max(nativeInput.Length, 1)];
            fixed (NativeKeyPoint* inputPtr = nativeInput)
            fixed (NativeKeyPoint* outputPtr = nativeOutput)
            {
                NativeException.ThrowIfError(compute(nativeHandle, imageHandle, inputPtr, nativeInput.Length, outputPtr, nativeOutput.Length, out int writtenCount, descriptorsHandle));
                return KeyPointMarshaller.FromNative(nativeOutput, writtenCount);
            }
        }

        private unsafe static KeyPoint[] DetectAndComputeCore(
            IntPtr nativeHandle,
            IntPtr imageHandle,
            IntPtr maskHandle,
            KeyPoint[] keypoints,
            IntPtr descriptorsHandle,
            bool useProvidedKeypoints,
            DetectAndComputeCountDelegate count,
            DetectAndComputeFillDelegate fill)
        {
            NativeKeyPoint[] nativeInput = KeyPointMarshaller.ToNative(keypoints);
            fixed (NativeKeyPoint* inputPtr = nativeInput)
            {
                NativeException.ThrowIfError(count(nativeHandle, imageHandle, maskHandle, inputPtr, nativeInput.Length, useProvidedKeypoints ? 1 : 0, out int outputCount));

                var nativeOutput = new NativeKeyPoint[Math.Max(outputCount, 1)];
                fixed (NativeKeyPoint* outputPtr = nativeOutput)
                {
                    NativeException.ThrowIfError(fill(nativeHandle, imageHandle, maskHandle, inputPtr, nativeInput.Length, useProvidedKeypoints ? 1 : 0, outputPtr, nativeOutput.Length, out int writtenCount, descriptorsHandle));
                    return KeyPointMarshaller.FromNative(nativeOutput, writtenCount);
                }
            }
        }

#if NETCOREAPP3_1_OR_GREATER
        private unsafe static KeyPoint[] ComputeCore(
            IntPtr nativeHandle,
            IntPtr imageHandle,
            ReadOnlySpan<KeyPoint> keypoints,
            IntPtr descriptorsHandle,
            ComputeDelegate compute)
        {
            NativeKeyPoint[]? rentedInput = null;
            NativeKeyPoint[]? rentedOutput = null;
            Span<NativeKeyPoint> nativeInput = keypoints.Length <= StackallocKeyPointThreshold
                ? stackalloc NativeKeyPoint[keypoints.Length]
                : (rentedInput = ArrayPool<NativeKeyPoint>.Shared.Rent(keypoints.Length)).AsSpan(0, keypoints.Length);
            Span<NativeKeyPoint> nativeOutput = keypoints.Length <= StackallocKeyPointThreshold
                ? stackalloc NativeKeyPoint[Math.Max(keypoints.Length, 1)]
                : (rentedOutput = ArrayPool<NativeKeyPoint>.Shared.Rent(Math.Max(keypoints.Length, 1))).AsSpan(0, Math.Max(keypoints.Length, 1));

            try
            {
                KeyPointMarshaller.CopyToNative(keypoints, nativeInput);
                fixed (NativeKeyPoint* inputPtr = nativeInput)
                fixed (NativeKeyPoint* outputPtr = nativeOutput)
                {
                    NativeException.ThrowIfError(compute(nativeHandle, imageHandle, inputPtr, keypoints.Length, outputPtr, nativeOutput.Length, out int writtenCount, descriptorsHandle));
                    return KeyPointMarshaller.FromNative(nativeOutput, writtenCount);
                }
            }
            finally
            {
                if (rentedInput != null)
                {
                    ArrayPool<NativeKeyPoint>.Shared.Return(rentedInput);
                }

                if (rentedOutput != null)
                {
                    ArrayPool<NativeKeyPoint>.Shared.Return(rentedOutput);
                }
            }
        }
#endif
    }
}
