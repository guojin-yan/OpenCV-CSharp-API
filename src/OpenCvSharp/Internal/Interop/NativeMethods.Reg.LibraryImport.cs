#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_release")]
        internal static partial void RegMapRelease(IntPtr map);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_get_kind")]
        internal static partial int RegMapGetKind(IntPtr map, out int kind);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_warp")]
        internal static partial int RegMapWarp(IntPtr map, IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_inverse_warp")]
        internal static partial int RegMapInverseWarp(IntPtr map, IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_inverse_map")]
        internal static partial int RegMapInverseMap(IntPtr map, out IntPtr inverseMap);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_compose")]
        internal static partial int RegMapCompose(IntPtr map, IntPtr other);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_scale")]
        internal static partial int RegMapScale(IntPtr map, double factor);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_shift_create")]
        internal static partial int RegMapShiftCreate(double shiftX, double shiftY, out IntPtr map);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_shift_get")]
        internal static partial int RegMapShiftGet(IntPtr map, out double shiftX, out double shiftY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_affine_create")]
        internal static partial int RegMapAffineCreate(double m00, double m01, double m10, double m11, double shiftX, double shiftY, out IntPtr map);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_affine_get")]
        internal static partial int RegMapAffineGet(IntPtr map, out double m00, out double m01, out double m10, out double m11, out double shiftX, out double shiftY);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_projec_create")]
        internal static partial int RegMapProjecCreate(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22, out IntPtr map);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_projec_get")]
        internal static partial int RegMapProjecGet(IntPtr map, out double m00, out double m01, out double m02, out double m10, out double m11, out double m12, out double m20, out double m21, out double m22);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_projec_normalize")]
        internal static partial int RegMapProjecNormalize(IntPtr map);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_release")]
        internal static partial void RegMapperRelease(IntPtr mapper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_grad_shift_create")]
        internal static partial int RegMapperGradShiftCreate(out IntPtr mapper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_grad_euclid_create")]
        internal static partial int RegMapperGradEuclidCreate(out IntPtr mapper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_grad_similar_create")]
        internal static partial int RegMapperGradSimilarCreate(out IntPtr mapper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_grad_affine_create")]
        internal static partial int RegMapperGradAffineCreate(out IntPtr mapper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_grad_proj_create")]
        internal static partial int RegMapperGradProjCreate(out IntPtr mapper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_pyramid_create")]
        internal static partial int RegMapperPyramidCreate(IntPtr baseMapper, out IntPtr mapper);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_calculate")]
        internal static partial int RegMapperCalculate(IntPtr mapper, IntPtr img1, IntPtr img2, IntPtr init, out IntPtr map);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_get_map")]
        internal static partial int RegMapperGetMap(IntPtr mapper, out IntPtr map);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_pyramid_get_num_levels")]
        internal static partial int RegMapperPyramidGetNumLevels(IntPtr mapper, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_pyramid_set_num_levels")]
        internal static partial int RegMapperPyramidSetNumLevels(IntPtr mapper, int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_pyramid_get_num_iterations_per_scale")]
        internal static partial int RegMapperPyramidGetNumIterationsPerScale(IntPtr mapper, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_pyramid_set_num_iterations_per_scale")]
        internal static partial int RegMapperPyramidSetNumIterationsPerScale(IntPtr mapper, int value);
    }
}
#endif
