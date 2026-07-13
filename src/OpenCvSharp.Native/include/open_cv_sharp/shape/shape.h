#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_shape_histogram_cost_extractor jyppx_ocv_shape_histogram_cost_extractor;
typedef struct jyppx_ocv_shape_distance_extractor jyppx_ocv_shape_distance_extractor;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_emd_l1(
    const jyppx_ocv_mat* signature1,
    const jyppx_ocv_mat* signature2,
    float* distance);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_norm_histogram_cost_extractor_create(
    int flag,
    int n_dummies,
    float default_cost,
    jyppx_ocv_shape_histogram_cost_extractor** extractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_emd_histogram_cost_extractor_create(
    int flag,
    int n_dummies,
    float default_cost,
    jyppx_ocv_shape_histogram_cost_extractor** extractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_chi_histogram_cost_extractor_create(
    int n_dummies,
    float default_cost,
    jyppx_ocv_shape_histogram_cost_extractor** extractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_emd_l1_histogram_cost_extractor_create(
    int n_dummies,
    float default_cost,
    jyppx_ocv_shape_histogram_cost_extractor** extractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_shape_histogram_cost_extractor_release_handle(
    jyppx_ocv_shape_histogram_cost_extractor* extractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_histogram_cost_extractor_build_cost_matrix(
    jyppx_ocv_shape_histogram_cost_extractor* extractor,
    const jyppx_ocv_mat* descriptors1,
    const jyppx_ocv_mat* descriptors2,
    jyppx_ocv_mat* cost_matrix);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_histogram_cost_extractor_set_n_dummies(
    jyppx_ocv_shape_histogram_cost_extractor* extractor,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_histogram_cost_extractor_get_n_dummies(
    const jyppx_ocv_shape_histogram_cost_extractor* extractor,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_histogram_cost_extractor_set_default_cost(
    jyppx_ocv_shape_histogram_cost_extractor* extractor,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_histogram_cost_extractor_get_default_cost(
    const jyppx_ocv_shape_histogram_cost_extractor* extractor,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_histogram_cost_extractor_set_norm_flag(
    jyppx_ocv_shape_histogram_cost_extractor* extractor,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_histogram_cost_extractor_get_norm_flag(
    const jyppx_ocv_shape_histogram_cost_extractor* extractor,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_context_distance_extractor_create(
    int n_angular_bins,
    int n_radial_bins,
    float inner_radius,
    float outer_radius,
    int iterations,
    jyppx_ocv_shape_distance_extractor** extractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_hausdorff_distance_extractor_create(
    int distance_flag,
    float rank_proportion,
    jyppx_ocv_shape_distance_extractor** extractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_shape_distance_extractor_release_handle(
    jyppx_ocv_shape_distance_extractor* extractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_distance_extractor_compute_distance(
    jyppx_ocv_shape_distance_extractor* extractor,
    const jyppx_ocv_mat* contour1,
    const jyppx_ocv_mat* contour2,
    float* distance);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_hausdorff_distance_extractor_set_distance_flag(
    jyppx_ocv_shape_distance_extractor* extractor,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_hausdorff_distance_extractor_get_distance_flag(
    const jyppx_ocv_shape_distance_extractor* extractor,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_hausdorff_distance_extractor_set_rank_proportion(
    jyppx_ocv_shape_distance_extractor* extractor,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_shape_hausdorff_distance_extractor_get_rank_proportion(
    const jyppx_ocv_shape_distance_extractor* extractor,
    float* value);
