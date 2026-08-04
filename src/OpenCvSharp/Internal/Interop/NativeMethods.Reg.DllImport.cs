#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_release")]
        internal static extern void RegMapRelease(IntPtr map);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_get_kind")]
        internal static extern int RegMapGetKind(IntPtr map, out int kind);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_warp")]
        internal static extern int RegMapWarp(IntPtr map, IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_inverse_warp")]
        internal static extern int RegMapInverseWarp(IntPtr map, IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_inverse_map")]
        internal static extern int RegMapInverseMap(IntPtr map, out IntPtr inverseMap);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_compose")]
        internal static extern int RegMapCompose(IntPtr map, IntPtr other);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_scale")]
        internal static extern int RegMapScale(IntPtr map, double factor);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_shift_create")]
        internal static extern int RegMapShiftCreate(double shiftX, double shiftY, out IntPtr map);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_shift_get")]
        internal static extern int RegMapShiftGet(IntPtr map, out double shiftX, out double shiftY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_affine_create")]
        internal static extern int RegMapAffineCreate(double m00, double m01, double m10, double m11, double shiftX, double shiftY, out IntPtr map);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_affine_get")]
        internal static extern int RegMapAffineGet(IntPtr map, out double m00, out double m01, out double m10, out double m11, out double shiftX, out double shiftY);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_projec_create")]
        internal static extern int RegMapProjecCreate(double m00, double m01, double m02, double m10, double m11, double m12, double m20, double m21, double m22, out IntPtr map);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_projec_get")]
        internal static extern int RegMapProjecGet(IntPtr map, out double m00, out double m01, out double m02, out double m10, out double m11, out double m12, out double m20, out double m21, out double m22);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_map_projec_normalize")]
        internal static extern int RegMapProjecNormalize(IntPtr map);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_release")]
        internal static extern void RegMapperRelease(IntPtr mapper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_grad_shift_create")]
        internal static extern int RegMapperGradShiftCreate(out IntPtr mapper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_grad_euclid_create")]
        internal static extern int RegMapperGradEuclidCreate(out IntPtr mapper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_grad_similar_create")]
        internal static extern int RegMapperGradSimilarCreate(out IntPtr mapper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_grad_affine_create")]
        internal static extern int RegMapperGradAffineCreate(out IntPtr mapper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_grad_proj_create")]
        internal static extern int RegMapperGradProjCreate(out IntPtr mapper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_pyramid_create")]
        internal static extern int RegMapperPyramidCreate(IntPtr baseMapper, out IntPtr mapper);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_calculate")]
        internal static extern int RegMapperCalculate(IntPtr mapper, IntPtr img1, IntPtr img2, IntPtr init, out IntPtr map);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_get_map")]
        internal static extern int RegMapperGetMap(IntPtr mapper, out IntPtr map);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_pyramid_get_num_levels")]
        internal static extern int RegMapperPyramidGetNumLevels(IntPtr mapper, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_pyramid_set_num_levels")]
        internal static extern int RegMapperPyramidSetNumLevels(IntPtr mapper, int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_pyramid_get_num_iterations_per_scale")]
        internal static extern int RegMapperPyramidGetNumIterationsPerScale(IntPtr mapper, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_reg_mapper_pyramid_set_num_iterations_per_scale")]
        internal static extern int RegMapperPyramidSetNumIterationsPerScale(IntPtr mapper, int value);
    }
}
#endif
