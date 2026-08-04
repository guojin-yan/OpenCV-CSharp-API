#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_simple_wb_create")]
        internal static partial int XPhotoSimpleWBCreate(out IntPtr whiteBalancer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_grayworld_wb_create")]
        internal static partial int XPhotoGrayworldWBCreate(out IntPtr whiteBalancer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_create")]
        internal static partial int XPhotoLearningBasedWBCreate(byte[] modelPath, out IntPtr whiteBalancer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_white_balancer_release_handle")]
        internal static partial void XPhotoWhiteBalancerReleaseHandle(IntPtr whiteBalancer);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_white_balancer_balance_white")]
        internal static partial int XPhotoWhiteBalancerBalanceWhite(IntPtr whiteBalancer, IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_simple_wb_get_property")]
        internal static partial int XPhotoSimpleWBGetProperty(IntPtr whiteBalancer, int propertyId, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_simple_wb_set_property")]
        internal static partial int XPhotoSimpleWBSetProperty(IntPtr whiteBalancer, int propertyId, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_grayworld_wb_get_saturation_threshold")]
        internal static partial int XPhotoGrayworldWBGetSaturationThreshold(IntPtr whiteBalancer, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_grayworld_wb_set_saturation_threshold")]
        internal static partial int XPhotoGrayworldWBSetSaturationThreshold(IntPtr whiteBalancer, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_get_int_property")]
        internal static partial int XPhotoLearningBasedWBGetIntProperty(IntPtr whiteBalancer, int propertyId, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_set_int_property")]
        internal static partial int XPhotoLearningBasedWBSetIntProperty(IntPtr whiteBalancer, int propertyId, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_get_saturation_threshold")]
        internal static partial int XPhotoLearningBasedWBGetSaturationThreshold(IntPtr whiteBalancer, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_set_saturation_threshold")]
        internal static partial int XPhotoLearningBasedWBSetSaturationThreshold(IntPtr whiteBalancer, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_learning_based_wb_extract_simple_features")]
        internal static partial int XPhotoLearningBasedWBExtractSimpleFeatures(IntPtr whiteBalancer, IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_apply_channel_gains")]
        internal static partial int XPhotoApplyChannelGains(IntPtr src, IntPtr dst, float gainB, float gainG, float gainR);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_dct_denoising")]
        internal static partial int XPhotoDctDenoising(IntPtr src, IntPtr dst, double sigma, int psize);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_bm3d_denoising")]
        internal static partial int XPhotoBm3dDenoising(IntPtr src, IntPtr dst, float h, int templateWindowSize, int searchWindowSize, int blockMatchingStep1, int blockMatchingStep2, int groupSize, int slidingStep, float beta, int normType, int step, int transformType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_bm3d_denoising_steps")]
        internal static partial int XPhotoBm3dDenoisingSteps(IntPtr src, IntPtr dstStep1, IntPtr dstStep2, float h, int templateWindowSize, int searchWindowSize, int blockMatchingStep1, int blockMatchingStep2, int groupSize, int slidingStep, float beta, int normType, int step, int transformType);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_xphoto_oil_painting")]
        internal static partial int XPhotoOilPainting(IntPtr src, IntPtr dst, int size, int dynRatio, int code, int useCode);
    }
}
#endif
