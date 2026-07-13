#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct StitchingCameraParamsNative
        {
            public double Focal;
            public double Aspect;
            public double Ppx;
            public double Ppy;
            public IntPtr R;
            public IntPtr T;
        }

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_create")]
        internal static partial int StitcherCreate(int mode, out IntPtr stitcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_release_handle")]
        internal static partial void StitcherReleaseHandle(IntPtr stitcher);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_double_property")]
        internal static partial int StitcherGetDoubleProperty(IntPtr stitcher, int propertyId, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_set_double_property")]
        internal static partial int StitcherSetDoubleProperty(IntPtr stitcher, int propertyId, double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_int_property")]
        internal static partial int StitcherGetIntProperty(IntPtr stitcher, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_set_int_property")]
        internal static partial int StitcherSetIntProperty(IntPtr stitcher, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_estimate_transform")]
        internal static partial int StitcherEstimateTransform(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount, out int statusCode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_compose_panorama")]
        internal static partial int StitcherComposePanorama(IntPtr stitcher, IntPtr pano, out int statusCode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_compose_panorama_images")]
        internal static partial int StitcherComposePanoramaImages(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr pano, out int statusCode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_stitch")]
        internal static partial int StitcherStitch(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount, IntPtr pano, out int statusCode);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_component_count")]
        internal static partial int StitcherGetComponentCount(IntPtr stitcher, out int componentCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_component_fill")]
        internal static partial int StitcherGetComponentFill(IntPtr stitcher, int[] components, int componentCapacity, out int componentCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_cameras_count")]
        internal static partial int StitcherGetCamerasCount(IntPtr stitcher, out int cameraCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_cameras_fill")]
        internal static partial int StitcherGetCamerasFill(IntPtr stitcher, StitchingCameraParamsNative[] cameras, int cameraCapacity, out int cameraCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_result_mask")]
        internal static partial int StitcherGetResultMask(IntPtr stitcher, IntPtr resultMask);
    }
}
#endif
