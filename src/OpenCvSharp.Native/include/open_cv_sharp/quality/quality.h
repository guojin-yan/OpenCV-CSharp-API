#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_quality jyppx_ocv_quality;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_mse_create(
    const jyppx_ocv_mat* reference,
    jyppx_ocv_quality** quality);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_psnr_create(
    const jyppx_ocv_mat* reference,
    double max_pixel_value,
    jyppx_ocv_quality** quality);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_ssim_create(
    const jyppx_ocv_mat* reference,
    jyppx_ocv_quality** quality);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_gmsd_create(
    const jyppx_ocv_mat* reference,
    jyppx_ocv_quality** quality);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_brisque_create(
    const char* model_file_path,
    const char* range_file_path,
    jyppx_ocv_quality** quality);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_quality_release_handle(
    jyppx_ocv_quality* quality);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_compute(
    jyppx_ocv_quality* quality,
    const jyppx_ocv_mat* comparison,
    double* scalar_values,
    int scalar_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_get_quality_map(
    const jyppx_ocv_quality* quality,
    jyppx_ocv_mat* quality_map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_clear(
    jyppx_ocv_quality* quality);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_empty(
    const jyppx_ocv_quality* quality,
    int* empty);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_psnr_get_max_pixel_value(
    const jyppx_ocv_quality* quality,
    double* max_pixel_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_psnr_set_max_pixel_value(
    jyppx_ocv_quality* quality,
    double max_pixel_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_mse_compute_static(
    const jyppx_ocv_mat* reference,
    const jyppx_ocv_mat* comparison,
    jyppx_ocv_mat* quality_map,
    double* scalar_values,
    int scalar_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_psnr_compute_static(
    const jyppx_ocv_mat* reference,
    const jyppx_ocv_mat* comparison,
    jyppx_ocv_mat* quality_map,
    double max_pixel_value,
    double* scalar_values,
    int scalar_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_ssim_compute_static(
    const jyppx_ocv_mat* reference,
    const jyppx_ocv_mat* comparison,
    jyppx_ocv_mat* quality_map,
    double* scalar_values,
    int scalar_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_gmsd_compute_static(
    const jyppx_ocv_mat* reference,
    const jyppx_ocv_mat* comparison,
    jyppx_ocv_mat* quality_map,
    double* scalar_values,
    int scalar_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_brisque_compute_static(
    const jyppx_ocv_mat* image,
    const char* model_file_path,
    const char* range_file_path,
    double* scalar_values,
    int scalar_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_quality_brisque_compute_features(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* features);
