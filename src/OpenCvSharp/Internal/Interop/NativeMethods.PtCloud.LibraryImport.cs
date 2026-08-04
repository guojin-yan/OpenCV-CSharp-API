#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_register_depth")]
        internal static partial int PtCloudRegisterDepth(IntPtr unregisteredCameraMatrix, IntPtr registeredCameraMatrix, IntPtr registeredDistCoeffs, IntPtr rt, IntPtr unregisteredDepth, int outputWidth, int outputHeight, IntPtr registeredDepth, int depthDilation);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_depth_to_3d")]
        internal static partial int PtCloudDepthTo3d(IntPtr depth, IntPtr cameraMatrix, IntPtr points3d, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_depth_to_3d_sparse")]
        internal static partial int PtCloudDepthTo3dSparse(IntPtr depth, IntPtr cameraMatrix, IntPtr points, IntPtr points3d);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_rescale_depth")]
        internal static partial int PtCloudRescaleDepth(IntPtr src, int type, IntPtr dst, double depthFactor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_warp_frame")]
        internal static partial int PtCloudWarpFrame(IntPtr depth, IntPtr image, IntPtr mask, IntPtr rt, IntPtr cameraMatrix, IntPtr warpedDepth, IntPtr warpedImage, IntPtr warpedMask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_find_planes")]
        internal static partial int PtCloudFindPlanes(IntPtr points3d, IntPtr normals, IntPtr mask, IntPtr planeCoefficients, int blockSize, int minSize, double threshold, double sensorErrorA, double sensorErrorB, double sensorErrorC, int method);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_create")]
        internal static partial int RgbdNormalsCreate(int rows, int cols, int depth, IntPtr cameraMatrix, int windowSize, float diffThreshold, int method, out IntPtr normals);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_release_handle")]
        internal static partial void RgbdNormalsReleaseHandle(IntPtr normals);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_apply")]
        internal static partial int RgbdNormalsApply(IntPtr normals, IntPtr points, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_cache")]
        internal static partial int RgbdNormalsCache(IntPtr normals);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_get_int_property")]
        internal static partial int RgbdNormalsGetIntProperty(IntPtr normals, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_set_int_property")]
        internal static partial int RgbdNormalsSetIntProperty(IntPtr normals, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_get_k")]
        internal static partial int RgbdNormalsGetK(IntPtr normals, IntPtr cameraMatrix);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_set_k")]
        internal static partial int RgbdNormalsSetK(IntPtr normals, IntPtr cameraMatrix);
    }
}
#endif
