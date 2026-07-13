#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_create")]
        internal static partial int HfsSegmentCreate(int height, int width, float segEgbThresholdI, int minRegionSizeI, float segEgbThresholdII, int minRegionSizeII, float spatialWeight, int slicSpixelSize, int numSlicIter, out IntPtr segment);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_release")]
        internal static partial void HfsSegmentRelease(IntPtr segment);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_get_float_property")]
        internal static partial int HfsSegmentGetFloatProperty(IntPtr segment, int propertyId, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_set_float_property")]
        internal static partial int HfsSegmentSetFloatProperty(IntPtr segment, int propertyId, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_get_int_property")]
        internal static partial int HfsSegmentGetIntProperty(IntPtr segment, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_set_int_property")]
        internal static partial int HfsSegmentSetIntProperty(IntPtr segment, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_perform_segment_cpu")]
        internal static partial int HfsSegmentPerformSegmentCpu(IntPtr segment, IntPtr src, IntPtr dst, int draw);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_perform_segment_gpu")]
        internal static partial int HfsSegmentPerformSegmentGpu(IntPtr segment, IntPtr src, IntPtr dst, int draw);
    }
}
#endif
