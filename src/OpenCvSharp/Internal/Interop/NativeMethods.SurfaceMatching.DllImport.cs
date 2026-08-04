#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_icp_create")]
        internal static extern int SurfaceMatchingIcpCreate(int iterations, float tolerance, float rejectionScale, int numLevels, int sampleType, int numMaxCorr, out IntPtr icp);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_icp_release")]
        internal static extern void SurfaceMatchingIcpRelease(IntPtr icp);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_icp_register_model_to_scene")]
        internal static extern int SurfaceMatchingIcpRegisterModelToScene(IntPtr icp, IntPtr srcPc, IntPtr dstPc, out int resultCode, out double residual, double[] pose16, int pose16Capacity);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_create")]
        internal static extern int SurfaceMatchingPpf3DDetectorCreate(double relativeSamplingStep, double relativeDistanceStep, double numAngles, out IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_release")]
        internal static extern void SurfaceMatchingPpf3DDetectorRelease(IntPtr detector);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_set_search_params")]
        internal static extern int SurfaceMatchingPpf3DDetectorSetSearchParams(IntPtr detector, double positionThreshold, double rotationThreshold, int useWeightedClustering);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_train_model")]
        internal static extern int SurfaceMatchingPpf3DDetectorTrainModel(IntPtr detector, IntPtr model);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_match_count")]
        internal static extern int SurfaceMatchingPpf3DDetectorMatchCount(IntPtr detector, IntPtr scene, double relativeSceneSampleStep, double relativeSceneDistance, out int resultCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_match_fill")]
        internal static extern int SurfaceMatchingPpf3DDetectorMatchFill(IntPtr detector, IntPtr scene, double relativeSceneSampleStep, double relativeSceneDistance, NativeSurfaceMatchingPose3DResult[] results, int resultCapacity, out int resultCount);
    }
}
#endif
