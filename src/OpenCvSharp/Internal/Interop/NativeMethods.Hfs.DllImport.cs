#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_create")]
        internal static extern int HfsSegmentCreate(int height, int width, float segEgbThresholdI, int minRegionSizeI, float segEgbThresholdII, int minRegionSizeII, float spatialWeight, int slicSpixelSize, int numSlicIter, out IntPtr segment);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_release")]
        internal static extern void HfsSegmentRelease(IntPtr segment);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_get_float_property")]
        internal static extern int HfsSegmentGetFloatProperty(IntPtr segment, int propertyId, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_set_float_property")]
        internal static extern int HfsSegmentSetFloatProperty(IntPtr segment, int propertyId, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_get_int_property")]
        internal static extern int HfsSegmentGetIntProperty(IntPtr segment, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_set_int_property")]
        internal static extern int HfsSegmentSetIntProperty(IntPtr segment, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_perform_segment_cpu")]
        internal static extern int HfsSegmentPerformSegmentCpu(IntPtr segment, IntPtr src, IntPtr dst, int draw);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_hfs_segment_perform_segment_gpu")]
        internal static extern int HfsSegmentPerformSegmentGpu(IntPtr segment, IntPtr src, IntPtr dst, int draw);
    }
}
#endif
