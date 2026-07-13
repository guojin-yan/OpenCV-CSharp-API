#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static unsafe partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_create")]
        internal static extern int LineDescriptorBinaryDescriptorCreate(int numOfOctave, int widthOfBand, int reductionRatio, int ksize, out IntPtr descriptor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_release")]
        internal static extern void LineDescriptorBinaryDescriptorRelease(IntPtr descriptor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_clear")]
        internal static extern int LineDescriptorBinaryDescriptorClear(IntPtr descriptor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_empty")]
        internal static extern int LineDescriptorBinaryDescriptorEmpty(IntPtr descriptor, out int empty);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_descriptor_size")]
        internal static extern int LineDescriptorBinaryDescriptorDescriptorSize(IntPtr descriptor, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_descriptor_type")]
        internal static extern int LineDescriptorBinaryDescriptorDescriptorType(IntPtr descriptor, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_default_norm")]
        internal static extern int LineDescriptorBinaryDescriptorDefaultNorm(IntPtr descriptor, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_get_num_of_octaves")]
        internal static extern int LineDescriptorBinaryDescriptorGetNumOfOctaves(IntPtr descriptor, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_set_num_of_octaves")]
        internal static extern int LineDescriptorBinaryDescriptorSetNumOfOctaves(IntPtr descriptor, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_get_width_of_band")]
        internal static extern int LineDescriptorBinaryDescriptorGetWidthOfBand(IntPtr descriptor, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_set_width_of_band")]
        internal static extern int LineDescriptorBinaryDescriptorSetWidthOfBand(IntPtr descriptor, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_get_reduction_ratio")]
        internal static extern int LineDescriptorBinaryDescriptorGetReductionRatio(IntPtr descriptor, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_set_reduction_ratio")]
        internal static extern int LineDescriptorBinaryDescriptorSetReductionRatio(IntPtr descriptor, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_detect_count")]
        internal static extern int LineDescriptorBinaryDescriptorDetectCount(IntPtr descriptor, IntPtr image, IntPtr mask, out int keylineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_detect_fill")]
        internal static extern int LineDescriptorBinaryDescriptorDetectFill(IntPtr descriptor, IntPtr image, IntPtr mask, NativeLineDescriptorKeyLine* keylines, int keylineCapacity, out int keylineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_compute")]
        internal static extern int LineDescriptorBinaryDescriptorCompute(IntPtr descriptor, IntPtr image, NativeLineDescriptorKeyLine* keylinesIn, int keylineCount, NativeLineDescriptorKeyLine* keylinesOut, int keylineCapacity, out int writtenKeylineCount, IntPtr descriptors, int returnFloatDescriptor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_detect_and_compute_count")]
        internal static extern int LineDescriptorBinaryDescriptorDetectAndComputeCount(IntPtr descriptor, IntPtr image, IntPtr mask, NativeLineDescriptorKeyLine* keylinesIn, int keylineCount, int useProvidedKeylines, int returnFloatDescriptor, out int outputKeylineCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_detect_and_compute_fill")]
        internal static extern int LineDescriptorBinaryDescriptorDetectAndComputeFill(IntPtr descriptor, IntPtr image, IntPtr mask, NativeLineDescriptorKeyLine* keylinesIn, int keylineCount, int useProvidedKeylines, int returnFloatDescriptor, NativeLineDescriptorKeyLine* keylinesOut, int keylineCapacity, out int outputKeylineCount, IntPtr descriptors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_draw_keylines")]
        internal static extern int LineDescriptorDrawKeylines(IntPtr image, NativeLineDescriptorKeyLine* keylines, int keylineCount, IntPtr outImage, double colorV0, double colorV1, double colorV2, double colorV3, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_draw_line_matches")]
        internal static extern int LineDescriptorDrawLineMatches(IntPtr img1, NativeLineDescriptorKeyLine* keylines1, int keyline1Count, IntPtr img2, NativeLineDescriptorKeyLine* keylines2, int keyline2Count, NativeDMatch* matches, int matchCount, IntPtr outImage, double matchColorV0, double matchColorV1, double matchColorV2, double matchColorV3, double singleLineColorV0, double singleLineColorV1, double singleLineColorV2, double singleLineColorV3, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_create")]
        internal static extern int LineDescriptorBinaryDescriptorMatcherCreate(out IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_release")]
        internal static extern void LineDescriptorBinaryDescriptorMatcherRelease(IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_clear")]
        internal static extern int LineDescriptorBinaryDescriptorMatcherClear(IntPtr matcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_empty")]
        internal static extern int LineDescriptorBinaryDescriptorMatcherEmpty(IntPtr matcher, out int empty);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_count")]
        internal static extern int LineDescriptorBinaryDescriptorMatcherMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, IntPtr mask, out int matchCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_fill")]
        internal static extern int LineDescriptorBinaryDescriptorMatcherMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, IntPtr mask, NativeDMatch* matches, int matchCapacity, out int matchCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_count")]
        internal static extern int LineDescriptorBinaryDescriptorMatcherKnnMatchCount(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, int k, IntPtr mask, int compactResult, out int groupCount, out int totalMatchCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_fill")]
        internal static extern int LineDescriptorBinaryDescriptorMatcherKnnMatchFill(IntPtr matcher, IntPtr queryDescriptors, IntPtr trainDescriptors, int k, IntPtr mask, int compactResult, int* offsets, int offsetCapacity, NativeDMatch* matches, int matchCapacity, out int groupCount, out int totalMatchCount);
    }
}
#endif
