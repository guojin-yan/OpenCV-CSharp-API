#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_create_kernel_from_functions")]
        internal static partial int FuzzyCreateKernelFromFunctions(IntPtr functionX, IntPtr functionY, IntPtr kernel, int channels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_create_kernel")]
        internal static partial int FuzzyCreateKernel(int functionType, int radius, IntPtr kernel, int channels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_inpaint")]
        internal static partial int FuzzyInpaint(IntPtr image, IntPtr mask, IntPtr output, int radius, int functionType, int algorithm);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_filter")]
        internal static partial int FuzzyFilter(IntPtr image, IntPtr kernel, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_components")]
        internal static partial int FuzzyFT02DComponents(IntPtr matrix, IntPtr kernel, IntPtr components, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_inverse_ft")]
        internal static partial int FuzzyFT02DInverseFT(IntPtr components, IntPtr kernel, IntPtr output, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_process")]
        internal static partial int FuzzyFT02DProcess(IntPtr matrix, IntPtr kernel, IntPtr output, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_iteration")]
        internal static partial int FuzzyFT02DIteration(IntPtr matrix, IntPtr kernel, IntPtr output, IntPtr mask, IntPtr maskOutput, int firstStop, out int state);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_fl_process")]
        internal static partial int FuzzyFT02DFLProcess(IntPtr matrix, int radius, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft02d_fl_process_float")]
        internal static partial int FuzzyFT02DFLProcessFloat(IntPtr matrix, int radius, IntPtr output);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_components")]
        internal static partial int FuzzyFT12DComponents(IntPtr matrix, IntPtr kernel, IntPtr components);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_polynomial")]
        internal static partial int FuzzyFT12DPolynomial(IntPtr matrix, IntPtr kernel, IntPtr c00, IntPtr c10, IntPtr c01, IntPtr components, IntPtr mask);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_vertical")]
        internal static partial int FuzzyFT12DCreatePolynomMatrixVertical(int radius, IntPtr matrix, int channels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_horizontal")]
        internal static partial int FuzzyFT12DCreatePolynomMatrixHorizontal(int radius, IntPtr matrix, int channels);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_inverse_ft")]
        internal static partial int FuzzyFT12DInverseFT(IntPtr components, IntPtr kernel, IntPtr output, int width, int height);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_fuzzy_ft12d_process")]
        internal static partial int FuzzyFT12DProcess(IntPtr matrix, IntPtr kernel, IntPtr output, IntPtr mask);
    }
}
#endif
