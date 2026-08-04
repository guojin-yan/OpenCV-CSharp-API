#if !NET7_0_OR_GREATER
using System;
using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_create_empty")]
        internal static extern int CoreSvdCreateEmpty(out IntPtr svd);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_create")]
        internal static extern int CoreSvdCreate(IntPtr src, int flags, out IntPtr svd);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_release")]
        internal static extern void CoreSvdRelease(IntPtr svd);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_compute")]
        internal static extern int CoreSvdCompute(IntPtr svd, IntPtr src, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_get_w")]
        internal static extern int CoreSvdGetW(IntPtr svd, out IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_get_u")]
        internal static extern int CoreSvdGetU(IntPtr svd, out IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_get_vt")]
        internal static extern int CoreSvdGetVt(IntPtr svd, out IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_back_subst")]
        internal static extern int CoreSvdBackSubst(IntPtr svd, IntPtr rhs, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_static_compute")]
        internal static extern int CoreSvdStaticCompute(IntPtr src, IntPtr w, IntPtr u, IntPtr vt, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_static_compute_values")]
        internal static extern int CoreSvdStaticComputeValues(IntPtr src, IntPtr w, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_static_back_subst")]
        internal static extern int CoreSvdStaticBackSubst(IntPtr w, IntPtr u, IntPtr vt, IntPtr rhs, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_svd_solve_z")]
        internal static extern int CoreSvdSolveZ(IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_create_default")]
        internal static extern int CoreRngCreateDefault(out IntPtr rng);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_create")]
        internal static extern int CoreRngCreate(ulong state, out IntPtr rng);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_release")]
        internal static extern void CoreRngRelease(IntPtr rng);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_get_state")]
        internal static extern int CoreRngGetState(IntPtr rng, out ulong state);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_set_state")]
        internal static extern int CoreRngSetState(IntPtr rng, ulong state);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_next")]
        internal static extern int CoreRngNext(IntPtr rng, out uint value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_uniform_int")]
        internal static extern int CoreRngUniformInt(IntPtr rng, int a, int b, out int value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_uniform_float")]
        internal static extern int CoreRngUniformFloat(IntPtr rng, float a, float b, out float value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_uniform_double")]
        internal static extern int CoreRngUniformDouble(IntPtr rng, double a, double b, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_gaussian")]
        internal static extern int CoreRngGaussian(IntPtr rng, double sigma, out double value);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rng_fill")]
        internal static extern int CoreRngFill(IntPtr rng, IntPtr mat, int distType, double aV0, double aV1, double aV2, double aV3, double bV0, double bV1, double bV2, double bV3, int saturateRange);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_gemm")]
        internal static extern int CoreGemm(IntPtr src1, IntPtr src2, double alpha, IntPtr src3, double beta, IntPtr dst, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mul_transposed")]
        internal static extern int CoreMulTransposed(IntPtr src, IntPtr dst, int aTa, IntPtr delta, double scale, int dtype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_transform")]
        internal static extern int CoreTransform(IntPtr src, IntPtr dst, IntPtr m);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_perspective_transform")]
        internal static extern int CorePerspectiveTransform(IntPtr src, IntPtr dst, IntPtr m);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_magnitude")]
        internal static extern int CoreMagnitude(IntPtr x, IntPtr y, IntPtr magnitude);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_phase")]
        internal static extern int CorePhase(IntPtr x, IntPtr y, IntPtr angle, int angleInDegrees);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_cart_to_polar")]
        internal static extern int CoreCartToPolar(IntPtr x, IntPtr y, IntPtr magnitude, IntPtr angle, int angleInDegrees);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_polar_to_cart")]
        internal static extern int CorePolarToCart(IntPtr magnitude, IntPtr angle, IntPtr x, IntPtr y, int angleInDegrees);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_dft")]
        internal static extern int CoreDft(IntPtr src, IntPtr dst, int flags, int nonzeroRows);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_idft")]
        internal static extern int CoreIdft(IntPtr src, IntPtr dst, int flags, int nonzeroRows);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_dct")]
        internal static extern int CoreDct(IntPtr src, IntPtr dst, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_idct")]
        internal static extern int CoreIdct(IntPtr src, IntPtr dst, int flags);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_mul_spectrums")]
        internal static extern int CoreMulSpectrums(IntPtr a, IntPtr b, IntPtr c, int flags, int conjB);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_div_spectrums")]
        internal static extern int CoreDivSpectrums(IntPtr a, IntPtr b, IntPtr c, int flags, int conjB);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_get_optimal_dft_size")]
        internal static extern int CoreGetOptimalDftSize(int vecSize, out int size);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_eigen")]
        internal static extern int CoreEigen(IntPtr src, IntPtr eigenvalues, IntPtr eigenvectors, out int success);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_eigen_non_symmetric")]
        internal static extern int CoreEigenNonSymmetric(IntPtr src, IntPtr eigenvalues, IntPtr eigenvectors);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_solve_cubic")]
        internal static extern int CoreSolveCubic(IntPtr coeffs, IntPtr roots, out int rootCount);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_solve_poly")]
        internal static extern int CoreSolvePoly(IntPtr coeffs, IntPtr roots, int maxIters, out double error);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_exp")]
        internal static extern int CoreExp(IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_log")]
        internal static extern int CoreLog(IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_sqrt")]
        internal static extern int CoreSqrt(IntPtr src, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_pow")]
        internal static extern int CorePow(IntPtr src, double power, IntPtr dst);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_calc_covar_matrix")]
        internal static extern int CoreCalcCovarMatrix(IntPtr samples, IntPtr covar, IntPtr mean, int flags, int ctype);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_pca_compute_max_components")]
        internal static extern int CorePcaComputeMaxComponents(IntPtr data, IntPtr mean, IntPtr eigenvectors, IntPtr eigenvalues, int maxComponents);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_pca_compute_retained_variance")]
        internal static extern int CorePcaComputeRetainedVariance(IntPtr data, IntPtr mean, IntPtr eigenvectors, IntPtr eigenvalues, double retainedVariance);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_pca_project")]
        internal static extern int CorePcaProject(IntPtr data, IntPtr mean, IntPtr eigenvectors, IntPtr result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_pca_back_project")]
        internal static extern int CorePcaBackProject(IntPtr data, IntPtr mean, IntPtr eigenvectors, IntPtr result);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_set_rng_seed")]
        internal static extern int CoreSetRngSeed(int seed);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_randu_mat")]
        internal static extern int CoreRanduMat(IntPtr dst, IntPtr low, IntPtr high);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_randu_scalar")]
        internal static extern int CoreRanduScalar(IntPtr dst, double lowV0, double lowV1, double lowV2, double lowV3, double highV0, double highV1, double highV2, double highV3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_randn_mat")]
        internal static extern int CoreRandnMat(IntPtr dst, IntPtr mean, IntPtr stddev);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_randn_scalar")]
        internal static extern int CoreRandnScalar(IntPtr dst, double meanV0, double meanV1, double meanV2, double meanV3, double stddevV0, double stddevV1, double stddevV2, double stddevV3);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_rand_shuffle")]
        internal static extern int CoreRandShuffle(IntPtr dst, double iterFactor, IntPtr rng);

        [DllImport(NativeLibraryNames.CurrentNativeLibrary, EntryPoint = "jyppx_ocv_core_solve_lp")]
        internal static extern int CoreSolveLp(IntPtr objective, IntPtr constraints, IntPtr solution, double constraintEpsilon, out int result);
    }
}
#endif
