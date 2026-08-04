#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_create_kernel_from_functions")]
        internal static extern int FuzzyCreateKernelFromFunctions(IntPtr functionX, IntPtr functionY, IntPtr kernel, int channels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_create_kernel")]
        internal static extern int FuzzyCreateKernel(int functionType, int radius, IntPtr kernel, int channels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_inpaint")]
        internal static extern int FuzzyInpaint(IntPtr image, IntPtr mask, IntPtr output, int radius, int functionType, int algorithm);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_filter")]
        internal static extern int FuzzyFilter(IntPtr image, IntPtr kernel, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_components")]
        internal static extern int FuzzyFT02DComponents(IntPtr matrix, IntPtr kernel, IntPtr components, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_inverse_ft")]
        internal static extern int FuzzyFT02DInverseFT(IntPtr components, IntPtr kernel, IntPtr output, int width, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_process")]
        internal static extern int FuzzyFT02DProcess(IntPtr matrix, IntPtr kernel, IntPtr output, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_iteration")]
        internal static extern int FuzzyFT02DIteration(IntPtr matrix, IntPtr kernel, IntPtr output, IntPtr mask, IntPtr maskOutput, int firstStop, out int state);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_fl_process")]
        internal static extern int FuzzyFT02DFLProcess(IntPtr matrix, int radius, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_fl_process_float")]
        internal static extern int FuzzyFT02DFLProcessFloat(IntPtr matrix, int radius, IntPtr output);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_components")]
        internal static extern int FuzzyFT12DComponents(IntPtr matrix, IntPtr kernel, IntPtr components);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_polynomial")]
        internal static extern int FuzzyFT12DPolynomial(IntPtr matrix, IntPtr kernel, IntPtr c00, IntPtr c10, IntPtr c01, IntPtr components, IntPtr mask);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_vertical")]
        internal static extern int FuzzyFT12DCreatePolynomMatrixVertical(int radius, IntPtr matrix, int channels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_horizontal")]
        internal static extern int FuzzyFT12DCreatePolynomMatrixHorizontal(int radius, IntPtr matrix, int channels);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_inverse_ft")]
        internal static extern int FuzzyFT12DInverseFT(IntPtr components, IntPtr kernel, IntPtr output, int width, int height);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_process")]
        internal static extern int FuzzyFT12DProcess(IntPtr matrix, IntPtr kernel, IntPtr output, IntPtr mask);
    }
}
#endif
