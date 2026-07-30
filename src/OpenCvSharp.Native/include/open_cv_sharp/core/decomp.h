#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

#include <stdint.h>

typedef struct jyppx_ocv_svd jyppx_ocv_svd;
typedef struct jyppx_ocv_rng jyppx_ocv_rng;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_create_empty(
    jyppx_ocv_svd** svd);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_create(
    const jyppx_ocv_mat* src,
    int flags,
    jyppx_ocv_svd** svd);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_core_svd_release(
    jyppx_ocv_svd* svd);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_compute(
    jyppx_ocv_svd* svd,
    const jyppx_ocv_mat* src,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_get_w(
    const jyppx_ocv_svd* svd,
    jyppx_ocv_mat** dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_get_u(
    const jyppx_ocv_svd* svd,
    jyppx_ocv_mat** dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_get_vt(
    const jyppx_ocv_svd* svd,
    jyppx_ocv_mat** dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_back_subst(
    const jyppx_ocv_svd* svd,
    const jyppx_ocv_mat* rhs,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_static_compute(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* w,
    jyppx_ocv_mat* u,
    jyppx_ocv_mat* vt,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_static_compute_values(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* w,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_static_back_subst(
    const jyppx_ocv_mat* w,
    const jyppx_ocv_mat* u,
    const jyppx_ocv_mat* vt,
    const jyppx_ocv_mat* rhs,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_svd_solve_z(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rng_create_default(
    jyppx_ocv_rng** rng);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rng_create(
    uint64_t state,
    jyppx_ocv_rng** rng);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_core_rng_release(
    jyppx_ocv_rng* rng);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rng_get_state(
    const jyppx_ocv_rng* rng,
    uint64_t* state);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rng_set_state(
    jyppx_ocv_rng* rng,
    uint64_t state);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rng_next(
    jyppx_ocv_rng* rng,
    uint32_t* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rng_uniform_int(
    jyppx_ocv_rng* rng,
    int a,
    int b,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rng_uniform_float(
    jyppx_ocv_rng* rng,
    float a,
    float b,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rng_uniform_double(
    jyppx_ocv_rng* rng,
    double a,
    double b,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rng_gaussian(
    jyppx_ocv_rng* rng,
    double sigma,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rng_fill(
    jyppx_ocv_rng* rng,
    jyppx_ocv_mat* mat,
    int dist_type,
    double a_v0,
    double a_v1,
    double a_v2,
    double a_v3,
    double b_v0,
    double b_v1,
    double b_v2,
    double b_v3,
    int saturate_range);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_gemm(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    double alpha,
    const jyppx_ocv_mat* src3,
    double beta,
    jyppx_ocv_mat* dst,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_mul_transposed(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int a_ta,
    const jyppx_ocv_mat* delta,
    double scale,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_transform(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* m);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_perspective_transform(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* m);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_magnitude(
    const jyppx_ocv_mat* x,
    const jyppx_ocv_mat* y,
    jyppx_ocv_mat* magnitude);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_phase(
    const jyppx_ocv_mat* x,
    const jyppx_ocv_mat* y,
    jyppx_ocv_mat* angle,
    int angle_in_degrees);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_cart_to_polar(
    const jyppx_ocv_mat* x,
    const jyppx_ocv_mat* y,
    jyppx_ocv_mat* magnitude,
    jyppx_ocv_mat* angle,
    int angle_in_degrees);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_polar_to_cart(
    const jyppx_ocv_mat* magnitude,
    const jyppx_ocv_mat* angle,
    jyppx_ocv_mat* x,
    jyppx_ocv_mat* y,
    int angle_in_degrees);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_dft(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int flags,
    int nonzero_rows);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_idft(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int flags,
    int nonzero_rows);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_dct(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_idct(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_mul_spectrums(
    const jyppx_ocv_mat* a,
    const jyppx_ocv_mat* b,
    jyppx_ocv_mat* c,
    int flags,
    int conj_b);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_div_spectrums(
    const jyppx_ocv_mat* a,
    const jyppx_ocv_mat* b,
    jyppx_ocv_mat* c,
    int flags,
    int conj_b);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_get_optimal_dft_size(
    int vec_size,
    int* out_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_eigen(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* eigenvalues,
    jyppx_ocv_mat* eigenvectors,
    int* out_success);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_eigen_non_symmetric(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* eigenvalues,
    jyppx_ocv_mat* eigenvectors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_solve_cubic(
    const jyppx_ocv_mat* coeffs,
    jyppx_ocv_mat* roots,
    int* out_root_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_solve_poly(
    const jyppx_ocv_mat* coeffs,
    jyppx_ocv_mat* roots,
    int max_iters,
    double* out_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_exp(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_log(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_sqrt(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_pow(
    const jyppx_ocv_mat* src,
    double power,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_calc_covar_matrix(
    const jyppx_ocv_mat* samples,
    jyppx_ocv_mat* covar,
    jyppx_ocv_mat* mean,
    int flags,
    int ctype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_pca_compute_max_components(
    const jyppx_ocv_mat* data,
    jyppx_ocv_mat* mean,
    jyppx_ocv_mat* eigenvectors,
    jyppx_ocv_mat* eigenvalues,
    int max_components);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_pca_compute_retained_variance(
    const jyppx_ocv_mat* data,
    jyppx_ocv_mat* mean,
    jyppx_ocv_mat* eigenvectors,
    jyppx_ocv_mat* eigenvalues,
    double retained_variance);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_pca_project(
    const jyppx_ocv_mat* data,
    const jyppx_ocv_mat* mean,
    const jyppx_ocv_mat* eigenvectors,
    jyppx_ocv_mat* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_pca_back_project(
    const jyppx_ocv_mat* data,
    const jyppx_ocv_mat* mean,
    const jyppx_ocv_mat* eigenvectors,
    jyppx_ocv_mat* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_set_rng_seed(int seed);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_randu_mat(
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* low,
    const jyppx_ocv_mat* high);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_randu_scalar(
    jyppx_ocv_mat* dst,
    double low_v0,
    double low_v1,
    double low_v2,
    double low_v3,
    double high_v0,
    double high_v1,
    double high_v2,
    double high_v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_randn_mat(
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mean,
    const jyppx_ocv_mat* stddev);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_randn_scalar(
    jyppx_ocv_mat* dst,
    double mean_v0,
    double mean_v1,
    double mean_v2,
    double mean_v3,
    double stddev_v0,
    double stddev_v1,
    double stddev_v2,
    double stddev_v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rand_shuffle(
    jyppx_ocv_mat* dst,
    double iter_factor,
    jyppx_ocv_rng* rng);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_solve_lp(
    const jyppx_ocv_mat* objective,
    const jyppx_ocv_mat* constraints,
    jyppx_ocv_mat* solution,
    double constraint_epsilon,
    int* out_result);
