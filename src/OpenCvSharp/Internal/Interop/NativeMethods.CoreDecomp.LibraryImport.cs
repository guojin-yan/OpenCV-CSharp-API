#if NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_create_empty")]
        internal static partial int CoreSvdCreateEmpty(out IntPtr svd);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_create")]
        internal static partial int CoreSvdCreate(IntPtr src, int flags, out IntPtr svd);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_release")]
        internal static partial void CoreSvdRelease(IntPtr svd);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_compute")]
        internal static partial int CoreSvdCompute(IntPtr svd, IntPtr src, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_get_w")]
        internal static partial int CoreSvdGetW(IntPtr svd, out IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_get_u")]
        internal static partial int CoreSvdGetU(IntPtr svd, out IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_get_vt")]
        internal static partial int CoreSvdGetVt(IntPtr svd, out IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_back_subst")]
        internal static partial int CoreSvdBackSubst(IntPtr svd, IntPtr rhs, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_static_compute")]
        internal static partial int CoreSvdStaticCompute(IntPtr src, IntPtr w, IntPtr u, IntPtr vt, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_static_compute_values")]
        internal static partial int CoreSvdStaticComputeValues(IntPtr src, IntPtr w, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_static_back_subst")]
        internal static partial int CoreSvdStaticBackSubst(IntPtr w, IntPtr u, IntPtr vt, IntPtr rhs, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_solve_z")]
        internal static partial int CoreSvdSolveZ(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_create_default")]
        internal static partial int CoreRngCreateDefault(out IntPtr rng);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_create")]
        internal static partial int CoreRngCreate(ulong state, out IntPtr rng);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_release")]
        internal static partial void CoreRngRelease(IntPtr rng);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_get_state")]
        internal static partial int CoreRngGetState(IntPtr rng, out ulong state);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_set_state")]
        internal static partial int CoreRngSetState(IntPtr rng, ulong state);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_next")]
        internal static partial int CoreRngNext(IntPtr rng, out uint value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_uniform_int")]
        internal static partial int CoreRngUniformInt(IntPtr rng, int a, int b, out int value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_uniform_float")]
        internal static partial int CoreRngUniformFloat(IntPtr rng, float a, float b, out float value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_uniform_double")]
        internal static partial int CoreRngUniformDouble(IntPtr rng, double a, double b, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_gaussian")]
        internal static partial int CoreRngGaussian(IntPtr rng, double sigma, out double value);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_fill")]
        internal static partial int CoreRngFill(IntPtr rng, IntPtr mat, int distType, double aV0, double aV1, double aV2, double aV3, double bV0, double bV1, double bV2, double bV3, int saturateRange);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_gemm")]
        internal static partial int CoreGemm(IntPtr src1, IntPtr src2, double alpha, IntPtr src3, double beta, IntPtr dst, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mul_transposed")]
        internal static partial int CoreMulTransposed(IntPtr src, IntPtr dst, int aTa, IntPtr delta, double scale, int dtype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_transform")]
        internal static partial int CoreTransform(IntPtr src, IntPtr dst, IntPtr m);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_perspective_transform")]
        internal static partial int CorePerspectiveTransform(IntPtr src, IntPtr dst, IntPtr m);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_magnitude")]
        internal static partial int CoreMagnitude(IntPtr x, IntPtr y, IntPtr magnitude);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_phase")]
        internal static partial int CorePhase(IntPtr x, IntPtr y, IntPtr angle, int angleInDegrees);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_cart_to_polar")]
        internal static partial int CoreCartToPolar(IntPtr x, IntPtr y, IntPtr magnitude, IntPtr angle, int angleInDegrees);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_polar_to_cart")]
        internal static partial int CorePolarToCart(IntPtr magnitude, IntPtr angle, IntPtr x, IntPtr y, int angleInDegrees);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_dft")]
        internal static partial int CoreDft(IntPtr src, IntPtr dst, int flags, int nonzeroRows);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_idft")]
        internal static partial int CoreIdft(IntPtr src, IntPtr dst, int flags, int nonzeroRows);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_dct")]
        internal static partial int CoreDct(IntPtr src, IntPtr dst, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_idct")]
        internal static partial int CoreIdct(IntPtr src, IntPtr dst, int flags);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mul_spectrums")]
        internal static partial int CoreMulSpectrums(IntPtr a, IntPtr b, IntPtr c, int flags, int conjB);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_div_spectrums")]
        internal static partial int CoreDivSpectrums(IntPtr a, IntPtr b, IntPtr c, int flags, int conjB);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_optimal_dft_size")]
        internal static partial int CoreGetOptimalDftSize(int vecSize, out int size);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_eigen")]
        internal static partial int CoreEigen(IntPtr src, IntPtr eigenvalues, IntPtr eigenvectors, out int success);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_eigen_non_symmetric")]
        internal static partial int CoreEigenNonSymmetric(IntPtr src, IntPtr eigenvalues, IntPtr eigenvectors);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_solve_cubic")]
        internal static partial int CoreSolveCubic(IntPtr coeffs, IntPtr roots, out int rootCount);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_solve_poly")]
        internal static partial int CoreSolvePoly(IntPtr coeffs, IntPtr roots, int maxIters, out double error);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_exp")]
        internal static partial int CoreExp(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_log")]
        internal static partial int CoreLog(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_sqrt")]
        internal static partial int CoreSqrt(IntPtr src, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_pow")]
        internal static partial int CorePow(IntPtr src, double power, IntPtr dst);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_calc_covar_matrix")]
        internal static partial int CoreCalcCovarMatrix(IntPtr samples, IntPtr covar, IntPtr mean, int flags, int ctype);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_pca_compute_max_components")]
        internal static partial int CorePcaComputeMaxComponents(IntPtr data, IntPtr mean, IntPtr eigenvectors, IntPtr eigenvalues, int maxComponents);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_pca_compute_retained_variance")]
        internal static partial int CorePcaComputeRetainedVariance(IntPtr data, IntPtr mean, IntPtr eigenvectors, IntPtr eigenvalues, double retainedVariance);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_pca_project")]
        internal static partial int CorePcaProject(IntPtr data, IntPtr mean, IntPtr eigenvectors, IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_pca_back_project")]
        internal static partial int CorePcaBackProject(IntPtr data, IntPtr mean, IntPtr eigenvectors, IntPtr result);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_set_rng_seed")]
        internal static partial int CoreSetRngSeed(int seed);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_randu_mat")]
        internal static partial int CoreRanduMat(IntPtr dst, IntPtr low, IntPtr high);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_randu_scalar")]
        internal static partial int CoreRanduScalar(IntPtr dst, double lowV0, double lowV1, double lowV2, double lowV3, double highV0, double highV1, double highV2, double highV3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_randn_mat")]
        internal static partial int CoreRandnMat(IntPtr dst, IntPtr mean, IntPtr stddev);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_randn_scalar")]
        internal static partial int CoreRandnScalar(IntPtr dst, double meanV0, double meanV1, double meanV2, double meanV3, double stddevV0, double stddevV1, double stddevV2, double stddevV3);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rand_shuffle")]
        internal static partial int CoreRandShuffle(IntPtr dst, double iterFactor, IntPtr rng);

        [LibraryImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_solve_lp")]
        internal static partial int CoreSolveLp(IntPtr objective, IntPtr constraints, IntPtr solution, double constraintEpsilon, out int result);
    }
}
#endif
