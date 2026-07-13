#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_icp_create")]
        internal static partial int SurfaceMatchingIcpCreate(int iterations, float tolerance, float rejectionScale, int numLevels, int sampleType, int numMaxCorr, out IntPtr icp);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_icp_release")]
        internal static partial void SurfaceMatchingIcpRelease(IntPtr icp);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_icp_register_model_to_scene")]
        internal static partial int SurfaceMatchingIcpRegisterModelToScene(IntPtr icp, IntPtr srcPc, IntPtr dstPc, out int resultCode, out double residual, double[] pose16, int pose16Capacity);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_create")]
        internal static partial int SurfaceMatchingPpf3DDetectorCreate(double relativeSamplingStep, double relativeDistanceStep, double numAngles, out IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_release")]
        internal static partial void SurfaceMatchingPpf3DDetectorRelease(IntPtr detector);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_set_search_params")]
        internal static partial int SurfaceMatchingPpf3DDetectorSetSearchParams(IntPtr detector, double positionThreshold, double rotationThreshold, int useWeightedClustering);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_train_model")]
        internal static partial int SurfaceMatchingPpf3DDetectorTrainModel(IntPtr detector, IntPtr model);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_match_count")]
        internal static partial int SurfaceMatchingPpf3DDetectorMatchCount(IntPtr detector, IntPtr scene, double relativeSceneSampleStep, double relativeSceneDistance, out int resultCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_surface_matching_ppf_3d_detector_match_fill")]
        internal static partial int SurfaceMatchingPpf3DDetectorMatchFill(IntPtr detector, IntPtr scene, double relativeSceneSampleStep, double relativeSceneDistance, NativeSurfaceMatchingPose3DResult[] results, int resultCapacity, out int resultCount);
    }
}
#endif
