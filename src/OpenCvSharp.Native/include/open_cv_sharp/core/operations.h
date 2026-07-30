#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_add(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_add_scalar(
    const jyppx_ocv_mat* src,
    double v0,
    double v1,
    double v2,
    double v3,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_subtract(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_subtract_scalar(
    const jyppx_ocv_mat* src,
    double v0,
    double v1,
    double v2,
    double v3,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_multiply(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst,
    double scale,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_divide(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst,
    double scale,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_scale_add(
    const jyppx_ocv_mat* src1,
    double alpha,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_add_weighted(
    const jyppx_ocv_mat* src1,
    double alpha,
    const jyppx_ocv_mat* src2,
    double beta,
    double gamma,
    jyppx_ocv_mat* dst,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_absdiff(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_absdiff_scalar(
    const jyppx_ocv_mat* src,
    double v0,
    double v1,
    double v2,
    double v3,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_bitwise_and(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_bitwise_or(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_bitwise_xor(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_bitwise_not(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_compare(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst,
    int cmpop);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_min(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_max(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_in_range(
    const jyppx_ocv_mat* src,
    double lower_v0,
    double lower_v1,
    double lower_v2,
    double lower_v3,
    double upper_v0,
    double upper_v1,
    double upper_v2,
    double upper_v3,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_patch_nans(
    jyppx_ocv_mat* src,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_count_non_zero(
    const jyppx_ocv_mat* src,
    int* out_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_mean(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* mask,
    double* out_values,
    int out_values_length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_mean_std_dev(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* mask,
    double* out_mean,
    int out_mean_length,
    double* out_stddev,
    int out_stddev_length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_min_max_loc(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* mask,
    double* out_min_val,
    double* out_max_val,
    int* out_min_x,
    int* out_min_y,
    int* out_max_x,
    int* out_max_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_norm(
    const jyppx_ocv_mat* src1,
    int norm_type,
    const jyppx_ocv_mat* mask,
    double* out_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_norm_diff(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    int norm_type,
    const jyppx_ocv_mat* mask,
    double* out_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_normalize(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double alpha,
    double beta,
    int norm_type,
    int dtype,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_reduce(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int dim,
    int rtype,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_sum(
    const jyppx_ocv_mat* src,
    double* out_values,
    int out_values_length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_trace(
    const jyppx_ocv_mat* src,
    double* out_values,
    int out_values_length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_determinant(
    const jyppx_ocv_mat* src,
    double* out_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_invert(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int flags,
    double* out_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_solve(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst,
    int flags,
    int* out_success);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_mahalanobis(
    const jyppx_ocv_mat* v1,
    const jyppx_ocv_mat* v2,
    const jyppx_ocv_mat* icovar,
    double* out_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_split_count(
    const jyppx_ocv_mat* src,
    int* out_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_split_fill(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat** dst,
    int dst_capacity,
    int* out_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_merge(
    const jyppx_ocv_mat* const* src,
    int src_count,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_hconcat(
    const jyppx_ocv_mat* const* src,
    int src_count,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_vconcat(
    const jyppx_ocv_mat* const* src,
    int src_count,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_extract_channel(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int coi);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_insert_channel(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int coi);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_mix_channels(
    const jyppx_ocv_mat* const* src,
    int src_count,
    jyppx_ocv_mat** dst,
    int dst_count,
    const int* from_to,
    int pair_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_repeat(
    const jyppx_ocv_mat* src,
    int ny,
    int nx,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_flip(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int flip_code);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_rotate(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int rotate_code);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_transpose(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_lut(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* lut,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_convert_scale_abs(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double alpha,
    double beta);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_complete_symm(
    jyppx_ocv_mat* mat,
    int lower_to_upper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_set_identity(
    jyppx_ocv_mat* mat,
    double v0,
    double v1,
    double v2,
    double v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_kmeans(
    const jyppx_ocv_mat* data,
    int k,
    jyppx_ocv_mat* best_labels,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int attempts,
    int flags,
    jyppx_ocv_mat* centers,
    double* compactness);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_border_interpolate(
    int p,
    int len,
    int border_type,
    int* out_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_copy_make_border(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int top,
    int bottom,
    int left,
    int right,
    int border_type,
    double v0,
    double v1,
    double v2,
    double v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_has_non_zero(
    const jyppx_ocv_mat* src,
    int* out_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_find_non_zero(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_psnr(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    double max_value,
    double* out_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_reduce_arg_min(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int axis,
    int last_index);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_reduce_arg_max(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int axis,
    int last_index);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_flip_nd(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int axis);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_broadcast(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* shape,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_copy_to_mask(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_check_range(
    const jyppx_ocv_mat* src,
    double min_value,
    double max_value,
    int* out_valid,
    int* out_x,
    int* out_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_finite_mask(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_transpose_nd(
    const jyppx_ocv_mat* src,
    const int* order,
    int order_count,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_sort(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_sort_idx(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_cube_root(
    float value,
    float* out_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_fast_atan2(
    float y,
    float x,
    float* out_degrees);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_batch_distance(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* distances,
    int dtype,
    jyppx_ocv_mat* indices,
    int norm_type,
    int k,
    const jyppx_ocv_mat* mask,
    int update,
    int crosscheck);
