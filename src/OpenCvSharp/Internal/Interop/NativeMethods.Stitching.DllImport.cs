#if !NET7_0_OR_GREATER
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

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_create")]
        internal static extern int StitcherCreate(int mode, out IntPtr stitcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_release_handle")]
        internal static extern void StitcherReleaseHandle(IntPtr stitcher);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_double_property")]
        internal static extern int StitcherGetDoubleProperty(IntPtr stitcher, int propertyId, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_set_double_property")]
        internal static extern int StitcherSetDoubleProperty(IntPtr stitcher, int propertyId, double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_int_property")]
        internal static extern int StitcherGetIntProperty(IntPtr stitcher, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_set_int_property")]
        internal static extern int StitcherSetIntProperty(IntPtr stitcher, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_estimate_transform")]
        internal static extern int StitcherEstimateTransform(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount, out int statusCode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_compose_panorama")]
        internal static extern int StitcherComposePanorama(IntPtr stitcher, IntPtr pano, out int statusCode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_compose_panorama_images")]
        internal static extern int StitcherComposePanoramaImages(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr pano, out int statusCode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_stitch")]
        internal static extern int StitcherStitch(IntPtr stitcher, IntPtr[] images, int imageCount, IntPtr[] masks, int maskCount, IntPtr pano, out int statusCode);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_component_count")]
        internal static extern int StitcherGetComponentCount(IntPtr stitcher, out int componentCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_component_fill")]
        internal static extern int StitcherGetComponentFill(IntPtr stitcher, int[] components, int componentCapacity, out int componentCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_cameras_count")]
        internal static extern int StitcherGetCamerasCount(IntPtr stitcher, out int cameraCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_cameras_fill")]
        internal static extern int StitcherGetCamerasFill(IntPtr stitcher, StitchingCameraParamsNative[] cameras, int cameraCapacity, out int cameraCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_stitcher_get_result_mask")]
        internal static extern int StitcherGetResultMask(IntPtr stitcher, IntPtr resultMask);
    }
}
#endif
