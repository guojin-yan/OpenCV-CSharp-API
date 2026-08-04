#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_create")]
        internal static partial int LineDescriptorBinaryDescriptorCreate(int numOfOctave, int widthOfBand, int reductionRatio, int ksize, out IntPtr descriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_release")]
        internal static partial void LineDescriptorBinaryDescriptorRelease(IntPtr descriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_clear")]
        internal static partial int LineDescriptorBinaryDescriptorClear(IntPtr descriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_empty")]
        internal static partial int LineDescriptorBinaryDescriptorEmpty(IntPtr descriptor, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_descriptor_size")]
        internal static partial int LineDescriptorBinaryDescriptorDescriptorSize(IntPtr descriptor, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_descriptor_type")]
        internal static partial int LineDescriptorBinaryDescriptorDescriptorType(IntPtr descriptor, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_default_norm")]
        internal static partial int LineDescriptorBinaryDescriptorDefaultNorm(IntPtr descriptor, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_get_num_of_octaves")]
        internal static partial int LineDescriptorBinaryDescriptorGetNumOfOctaves(IntPtr descriptor, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_set_num_of_octaves")]
        internal static partial int LineDescriptorBinaryDescriptorSetNumOfOctaves(IntPtr descriptor, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_get_width_of_band")]
        internal static partial int LineDescriptorBinaryDescriptorGetWidthOfBand(IntPtr descriptor, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_set_width_of_band")]
        internal static partial int LineDescriptorBinaryDescriptorSetWidthOfBand(IntPtr descriptor, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_get_reduction_ratio")]
        internal static partial int LineDescriptorBinaryDescriptorGetReductionRatio(IntPtr descriptor, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_set_reduction_ratio")]
        internal static partial int LineDescriptorBinaryDescriptorSetReductionRatio(IntPtr descriptor, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_detect_count")]
        internal static partial int LineDescriptorBinaryDescriptorDetectCount(IntPtr descriptor, IntPtr image, IntPtr mask, out int keylineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_detect_fill")]
        internal static partial int LineDescriptorBinaryDescriptorDetectFill(IntPtr descriptor, IntPtr image, IntPtr mask, NativeLineDescriptorKeyLine* keylines, int keylineCapacity, out int keylineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_compute")]
        internal static partial int LineDescriptorBinaryDescriptorCompute(IntPtr descriptor, IntPtr image, NativeLineDescriptorKeyLine* keylinesIn, int keylineCount, NativeLineDescriptorKeyLine* keylinesOut, int keylineCapacity, out int writtenKeylineCount, IntPtr descriptors, int returnFloatDescriptor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_detect_and_compute_count")]
        internal static partial int LineDescriptorBinaryDescriptorDetectAndComputeCount(IntPtr descriptor, IntPtr image, IntPtr mask, NativeLineDescriptorKeyLine* keylinesIn, int keylineCount, int useProvidedKeylines, int returnFloatDescriptor, out int outputKeylineCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_detect_and_compute_fill")]
        internal static partial int LineDescriptorBinaryDescriptorDetectAndComputeFill(IntPtr descriptor, IntPtr image, IntPtr mask, NativeLineDescriptorKeyLine* keylinesIn, int keylineCount, int useProvidedKeylines, int returnFloatDescriptor, NativeLineDescriptorKeyLine* keylinesOut, int keylineCapacity, out int outputKeylineCount, IntPtr descriptors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_draw_keylines")]
        internal static partial int LineDescriptorDrawKeylines(IntPtr image, NativeLineDescriptorKeyLine* keylines, int keylineCount, IntPtr outImage, double colorV0, double colorV1, double colorV2, double colorV3, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_draw_line_matches")]
        internal static partial int LineDescriptorDrawLineMatches(IntPtr img1, NativeLineDescriptorKeyLine* keylines1, int keyline1Count, IntPtr img2, NativeLineDescriptorKeyLine* keylines2, int keyline2Count, NativeDMatch* matches, int matchCount, IntPtr outImage, double matchColorV0, double matchColorV1, double matchColorV2, double matchColorV3, double singleLineColorV0, double singleLineColorV1, double singleLineColorV2, double singleLineColorV3, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_create")]
        internal static partial int LineDescriptorBinaryDescriptorMatcherCreate(out IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_release")]
        internal static partial void LineDescriptorBinaryDescriptorMatcherRelease(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_clear")]
        internal static partial int LineDescriptorBinaryDescriptorMatcherClear(IntPtr matcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_empty")]
        internal static partial int LineDescriptorBinaryDescriptorMatcherEmpty(IntPtr matcher, out int empty);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_count")]
        internal static partial int LineDescriptorBinaryDescriptorMatcherMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, IntPtr mask, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_fill")]
        internal static partial int LineDescriptorBinaryDescriptorMatcherMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, IntPtr mask, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_count")]
        internal static partial int LineDescriptorBinaryDescriptorMatcherKnnMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, int k, IntPtr mask, int compactResult, out int groupCount, out int totalMatchCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_fill")]
        internal static partial int LineDescriptorBinaryDescriptorMatcherKnnMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, int k, IntPtr mask, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);
    }
}
#endif
