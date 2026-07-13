#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_simple_wb_create")]
        internal static extern int XPhotoSimpleWBCreate(out IntPtr whiteBalancer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_grayworld_wb_create")]
        internal static extern int XPhotoGrayworldWBCreate(out IntPtr whiteBalancer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_create")]
        internal static extern int XPhotoLearningBasedWBCreate(byte[] modelPath, out IntPtr whiteBalancer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_white_balancer_release_handle")]
        internal static extern void XPhotoWhiteBalancerReleaseHandle(IntPtr whiteBalancer);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_white_balancer_balance_white")]
        internal static extern int XPhotoWhiteBalancerBalanceWhite(IntPtr whiteBalancer, IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_simple_wb_get_property")]
        internal static extern int XPhotoSimpleWBGetProperty(IntPtr whiteBalancer, int propertyId, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_simple_wb_set_property")]
        internal static extern int XPhotoSimpleWBSetProperty(IntPtr whiteBalancer, int propertyId, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_grayworld_wb_get_saturation_threshold")]
        internal static extern int XPhotoGrayworldWBGetSaturationThreshold(IntPtr whiteBalancer, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_grayworld_wb_set_saturation_threshold")]
        internal static extern int XPhotoGrayworldWBSetSaturationThreshold(IntPtr whiteBalancer, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_get_int_property")]
        internal static extern int XPhotoLearningBasedWBGetIntProperty(IntPtr whiteBalancer, int propertyId, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_set_int_property")]
        internal static extern int XPhotoLearningBasedWBSetIntProperty(IntPtr whiteBalancer, int propertyId, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_get_saturation_threshold")]
        internal static extern int XPhotoLearningBasedWBGetSaturationThreshold(IntPtr whiteBalancer, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_set_saturation_threshold")]
        internal static extern int XPhotoLearningBasedWBSetSaturationThreshold(IntPtr whiteBalancer, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_extract_simple_features")]
        internal static extern int XPhotoLearningBasedWBExtractSimpleFeatures(IntPtr whiteBalancer, IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_apply_channel_gains")]
        internal static extern int XPhotoApplyChannelGains(IntPtr src, IntPtr dst, float gainB, float gainG, float gainR);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_dct_denoising")]
        internal static extern int XPhotoDctDenoising(IntPtr src, IntPtr dst, double sigma, int psize);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_bm3d_denoising")]
        internal static extern int XPhotoBm3dDenoising(IntPtr src, IntPtr dst, float h, int templateWindowSize, int searchWindowSize, int blockMatchingStep1, int blockMatchingStep2, int groupSize, int slidingStep, float beta, int normType, int step, int transformType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_bm3d_denoising_steps")]
        internal static extern int XPhotoBm3dDenoisingSteps(IntPtr src, IntPtr dstStep1, IntPtr dstStep2, float h, int templateWindowSize, int searchWindowSize, int blockMatchingStep1, int blockMatchingStep2, int groupSize, int slidingStep, float beta, int normType, int step, int transformType);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_oil_painting")]
        internal static extern int XPhotoOilPainting(IntPtr src, IntPtr dst, int size, int dynRatio, int code, int useCode);
    }
}
#endif
