#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_emd_l1")]
        internal static extern int ShapeEMDL1(IntPtr signature1, IntPtr signature2, out float distance);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_norm_histogram_cost_extractor_create")]
        internal static extern int ShapeNormHistogramCostExtractorCreate(int flag, int nDummies, float defaultCost, out IntPtr extractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_emd_histogram_cost_extractor_create")]
        internal static extern int ShapeEMDHistogramCostExtractorCreate(int flag, int nDummies, float defaultCost, out IntPtr extractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_chi_histogram_cost_extractor_create")]
        internal static extern int ShapeChiHistogramCostExtractorCreate(int nDummies, float defaultCost, out IntPtr extractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_emd_l1_histogram_cost_extractor_create")]
        internal static extern int ShapeEMDL1HistogramCostExtractorCreate(int nDummies, float defaultCost, out IntPtr extractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_release_handle")]
        internal static extern void ShapeHistogramCostExtractorReleaseHandle(IntPtr extractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_build_cost_matrix")]
        internal static extern int ShapeHistogramCostExtractorBuildCostMatrix(IntPtr extractor, IntPtr descriptors1, IntPtr descriptors2, IntPtr costMatrix);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_set_n_dummies")]
        internal static extern int ShapeHistogramCostExtractorSetNDummies(IntPtr extractor, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_get_n_dummies")]
        internal static extern int ShapeHistogramCostExtractorGetNDummies(IntPtr extractor, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_set_default_cost")]
        internal static extern int ShapeHistogramCostExtractorSetDefaultCost(IntPtr extractor, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_get_default_cost")]
        internal static extern int ShapeHistogramCostExtractorGetDefaultCost(IntPtr extractor, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_set_norm_flag")]
        internal static extern int ShapeHistogramCostExtractorSetNormFlag(IntPtr extractor, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_get_norm_flag")]
        internal static extern int ShapeHistogramCostExtractorGetNormFlag(IntPtr extractor, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_context_distance_extractor_create")]
        internal static extern int ShapeContextDistanceExtractorCreate(int nAngularBins, int nRadialBins, float innerRadius, float outerRadius, int iterations, out IntPtr extractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_hausdorff_distance_extractor_create")]
        internal static extern int ShapeHausdorffDistanceExtractorCreate(int distanceFlag, float rankProportion, out IntPtr extractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_distance_extractor_release_handle")]
        internal static extern void ShapeDistanceExtractorReleaseHandle(IntPtr extractor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_distance_extractor_compute_distance")]
        internal static extern int ShapeDistanceExtractorComputeDistance(IntPtr extractor, IntPtr contour1, IntPtr contour2, out float distance);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_hausdorff_distance_extractor_set_distance_flag")]
        internal static extern int ShapeHausdorffDistanceExtractorSetDistanceFlag(IntPtr extractor, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_hausdorff_distance_extractor_get_distance_flag")]
        internal static extern int ShapeHausdorffDistanceExtractorGetDistanceFlag(IntPtr extractor, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_hausdorff_distance_extractor_set_rank_proportion")]
        internal static extern int ShapeHausdorffDistanceExtractorSetRankProportion(IntPtr extractor, float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_hausdorff_distance_extractor_get_rank_proportion")]
        internal static extern int ShapeHausdorffDistanceExtractorGetRankProportion(IntPtr extractor, out float value);
    }
}
#endif
