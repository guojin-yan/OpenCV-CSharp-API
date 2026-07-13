#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_saliency_saliency jyppx_ocv_saliency_saliency;
typedef struct jyppx_ocv_saliency_static jyppx_ocv_saliency_static;
typedef struct jyppx_ocv_saliency_spectral_residual jyppx_ocv_saliency_spectral_residual;
typedef struct jyppx_ocv_saliency_fine_grained jyppx_ocv_saliency_fine_grained;
typedef struct jyppx_ocv_saliency_motion_bin_wang jyppx_ocv_saliency_motion_bin_wang;
typedef struct jyppx_ocv_saliency_objectness_bing jyppx_ocv_saliency_objectness_bing;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_saliency_saliency_release_handle(
    jyppx_ocv_saliency_saliency* saliency);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_compute_saliency(
    jyppx_ocv_saliency_saliency* saliency,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* saliency_map,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_static_compute_binary_map(
    jyppx_ocv_saliency_static* saliency,
    const jyppx_ocv_mat* saliency_map,
    jyppx_ocv_mat* binary_map,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_spectral_residual_create(
    jyppx_ocv_saliency_spectral_residual** saliency);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_spectral_residual_get_image_width(
    const jyppx_ocv_saliency_spectral_residual* saliency,
    int* width);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_spectral_residual_set_image_width(
    jyppx_ocv_saliency_spectral_residual* saliency,
    int width);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_spectral_residual_get_image_height(
    const jyppx_ocv_saliency_spectral_residual* saliency,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_spectral_residual_set_image_height(
    jyppx_ocv_saliency_spectral_residual* saliency,
    int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_fine_grained_create(
    jyppx_ocv_saliency_fine_grained** saliency);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_motion_bin_wang_create(
    jyppx_ocv_saliency_motion_bin_wang** saliency);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_motion_bin_wang_set_image_size(
    jyppx_ocv_saliency_motion_bin_wang* saliency,
    int width,
    int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_motion_bin_wang_init(
    jyppx_ocv_saliency_motion_bin_wang* saliency,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_motion_bin_wang_get_image_width(
    const jyppx_ocv_saliency_motion_bin_wang* saliency,
    int* width);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_motion_bin_wang_set_image_width(
    jyppx_ocv_saliency_motion_bin_wang* saliency,
    int width);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_motion_bin_wang_get_image_height(
    const jyppx_ocv_saliency_motion_bin_wang* saliency,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_motion_bin_wang_set_image_height(
    jyppx_ocv_saliency_motion_bin_wang* saliency,
    int height);

/* ObjectnessBING second batch: output boxes and objectness scores stay cached
   behind the opaque handle and are copied through count/fill helpers. */
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_create(
    jyppx_ocv_saliency_objectness_bing** saliency);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_set_training_path(
    jyppx_ocv_saliency_objectness_bing* saliency,
    const char* training_path);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_set_bb_res_dir(
    jyppx_ocv_saliency_objectness_bing* saliency,
    const char* results_dir);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_get_base(
    const jyppx_ocv_saliency_objectness_bing* saliency,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_set_base(
    jyppx_ocv_saliency_objectness_bing* saliency,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_get_nss(
    const jyppx_ocv_saliency_objectness_bing* saliency,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_set_nss(
    jyppx_ocv_saliency_objectness_bing* saliency,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_get_w(
    const jyppx_ocv_saliency_objectness_bing* saliency,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_set_w(
    jyppx_ocv_saliency_objectness_bing* saliency,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_compute(
    jyppx_ocv_saliency_objectness_bing* saliency,
    const jyppx_ocv_mat* image,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_get_boxes_count(
    const jyppx_ocv_saliency_objectness_bing* saliency,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_get_boxes_fill(
    const jyppx_ocv_saliency_objectness_bing* saliency,
    int* boxes,
    int box_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_get_objectness_values_count(
    const jyppx_ocv_saliency_objectness_bing* saliency,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_saliency_objectness_bing_get_objectness_values_fill(
    const jyppx_ocv_saliency_objectness_bing* saliency,
    float* values,
    int value_capacity,
    int* count);
