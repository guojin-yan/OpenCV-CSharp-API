#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_img_hash jyppx_ocv_img_hash;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_average_create(
    jyppx_ocv_img_hash** hash);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_phash_create(
    jyppx_ocv_img_hash** hash);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_block_mean_create(
    int mode,
    jyppx_ocv_img_hash** hash);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_color_moment_create(
    jyppx_ocv_img_hash** hash);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_marr_hildreth_create(
    float alpha,
    float scale,
    jyppx_ocv_img_hash** hash);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_radial_variance_create(
    double sigma,
    int num_of_angle_line,
    jyppx_ocv_img_hash** hash);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_img_hash_release_handle(
    jyppx_ocv_img_hash* hash);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_compute(
    jyppx_ocv_img_hash* hash,
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_compare(
    const jyppx_ocv_img_hash* hash,
    const jyppx_ocv_mat* hash_one,
    const jyppx_ocv_mat* hash_two,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_block_mean_set_mode(
    jyppx_ocv_img_hash* hash,
    int mode);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_block_mean_get_mean_count(
    const jyppx_ocv_img_hash* hash,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_block_mean_get_mean_fill(
    const jyppx_ocv_img_hash* hash,
    double* values,
    int value_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_marr_hildreth_get(
    const jyppx_ocv_img_hash* hash,
    float* alpha,
    float* scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_marr_hildreth_set_kernel_param(
    jyppx_ocv_img_hash* hash,
    float alpha,
    float scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_radial_variance_get(
    const jyppx_ocv_img_hash* hash,
    double* sigma,
    int* num_of_angle_line);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_radial_variance_set_sigma(
    jyppx_ocv_img_hash* hash,
    double sigma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_radial_variance_set_num_of_angle_line(
    jyppx_ocv_img_hash* hash,
    int num_of_angle_line);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_average_compute_static(
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_phash_compute_static(
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_block_mean_compute_static(
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output,
    int mode);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_color_moment_compute_static(
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_marr_hildreth_compute_static(
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output,
    float alpha,
    float scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_img_hash_radial_variance_compute_static(
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output,
    double sigma,
    int num_of_angle_line);
