#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_register_depth")]
        internal static extern int PtCloudRegisterDepth(IntPtr unregisteredCameraMatrix, IntPtr registeredCameraMatrix, IntPtr registeredDistCoeffs, IntPtr rt, IntPtr unregisteredDepth, int outputWidth, int outputHeight, IntPtr registeredDepth, int depthDilation);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_depth_to_3d")]
        internal static extern int PtCloudDepthTo3d(IntPtr depth, IntPtr cameraMatrix, IntPtr points3d, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_depth_to_3d_sparse")]
        internal static extern int PtCloudDepthTo3dSparse(IntPtr depth, IntPtr cameraMatrix, IntPtr points, IntPtr points3d);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_rescale_depth")]
        internal static extern int PtCloudRescaleDepth(IntPtr src, int type, IntPtr dst, double depthFactor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_warp_frame")]
        internal static extern int PtCloudWarpFrame(IntPtr depth, IntPtr image, IntPtr mask, IntPtr rt, IntPtr cameraMatrix, IntPtr warpedDepth, IntPtr warpedImage, IntPtr warpedMask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_ptcloud_find_planes")]
        internal static extern int PtCloudFindPlanes(IntPtr points3d, IntPtr normals, IntPtr mask, IntPtr planeCoefficients, int blockSize, int minSize, double threshold, double sensorErrorA, double sensorErrorB, double sensorErrorC, int method);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_create")]
        internal static extern int RgbdNormalsCreate(int rows, int cols, int depth, IntPtr cameraMatrix, int windowSize, float diffThreshold, int method, out IntPtr normals);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_release_handle")]
        internal static extern void RgbdNormalsReleaseHandle(IntPtr normals);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_apply")]
        internal static extern int RgbdNormalsApply(IntPtr normals, IntPtr points, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_cache")]
        internal static extern int RgbdNormalsCache(IntPtr normals);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_get_int_property")]
        internal static extern int RgbdNormalsGetIntProperty(IntPtr normals, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_set_int_property")]
        internal static extern int RgbdNormalsSetIntProperty(IntPtr normals, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_get_k")]
        internal static extern int RgbdNormalsGetK(IntPtr normals, IntPtr cameraMatrix);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_rgbd_normals_set_k")]
        internal static extern int RgbdNormalsSetK(IntPtr normals, IntPtr cameraMatrix);
    }
}
#endif
