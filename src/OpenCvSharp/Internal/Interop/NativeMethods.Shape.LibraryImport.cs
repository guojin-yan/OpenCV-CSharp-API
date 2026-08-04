#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_emd_l1")]
        internal static partial int ShapeEMDL1(IntPtr signature1, IntPtr signature2, out float distance);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_norm_histogram_cost_extractor_create")]
        internal static partial int ShapeNormHistogramCostExtractorCreate(int flag, int nDummies, float defaultCost, out IntPtr extractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_emd_histogram_cost_extractor_create")]
        internal static partial int ShapeEMDHistogramCostExtractorCreate(int flag, int nDummies, float defaultCost, out IntPtr extractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_chi_histogram_cost_extractor_create")]
        internal static partial int ShapeChiHistogramCostExtractorCreate(int nDummies, float defaultCost, out IntPtr extractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_emd_l1_histogram_cost_extractor_create")]
        internal static partial int ShapeEMDL1HistogramCostExtractorCreate(int nDummies, float defaultCost, out IntPtr extractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_release_handle")]
        internal static partial void ShapeHistogramCostExtractorReleaseHandle(IntPtr extractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_build_cost_matrix")]
        internal static partial int ShapeHistogramCostExtractorBuildCostMatrix(IntPtr extractor, IntPtr descriptors1, IntPtr descriptors2, IntPtr costMatrix);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_set_n_dummies")]
        internal static partial int ShapeHistogramCostExtractorSetNDummies(IntPtr extractor, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_get_n_dummies")]
        internal static partial int ShapeHistogramCostExtractorGetNDummies(IntPtr extractor, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_set_default_cost")]
        internal static partial int ShapeHistogramCostExtractorSetDefaultCost(IntPtr extractor, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_get_default_cost")]
        internal static partial int ShapeHistogramCostExtractorGetDefaultCost(IntPtr extractor, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_set_norm_flag")]
        internal static partial int ShapeHistogramCostExtractorSetNormFlag(IntPtr extractor, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_histogram_cost_extractor_get_norm_flag")]
        internal static partial int ShapeHistogramCostExtractorGetNormFlag(IntPtr extractor, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_context_distance_extractor_create")]
        internal static partial int ShapeContextDistanceExtractorCreate(int nAngularBins, int nRadialBins, float innerRadius, float outerRadius, int iterations, out IntPtr extractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_hausdorff_distance_extractor_create")]
        internal static partial int ShapeHausdorffDistanceExtractorCreate(int distanceFlag, float rankProportion, out IntPtr extractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_distance_extractor_release_handle")]
        internal static partial void ShapeDistanceExtractorReleaseHandle(IntPtr extractor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_distance_extractor_compute_distance")]
        internal static partial int ShapeDistanceExtractorComputeDistance(IntPtr extractor, IntPtr contour1, IntPtr contour2, out float distance);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_hausdorff_distance_extractor_set_distance_flag")]
        internal static partial int ShapeHausdorffDistanceExtractorSetDistanceFlag(IntPtr extractor, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_hausdorff_distance_extractor_get_distance_flag")]
        internal static partial int ShapeHausdorffDistanceExtractorGetDistanceFlag(IntPtr extractor, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_hausdorff_distance_extractor_set_rank_proportion")]
        internal static partial int ShapeHausdorffDistanceExtractorSetRankProportion(IntPtr extractor, float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_shape_hausdorff_distance_extractor_get_rank_proportion")]
        internal static partial int ShapeHausdorffDistanceExtractorGetRankProportion(IntPtr extractor, out float value);
    }
}
#endif
