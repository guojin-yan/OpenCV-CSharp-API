#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_cascade_classifier jyppx_ocv_cascade_classifier;
typedef struct jyppx_ocv_hog_descriptor jyppx_ocv_hog_descriptor;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_create(
    jyppx_ocv_cascade_classifier** classifier);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_create_from_file(
    const char* filename,
    jyppx_ocv_cascade_classifier** classifier);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_cascade_classifier_release_handle(
    jyppx_ocv_cascade_classifier* classifier);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_load(
    jyppx_ocv_cascade_classifier* classifier,
    const char* filename,
    int* loaded);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_empty(
    const jyppx_ocv_cascade_classifier* classifier,
    int* empty);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_get_original_window_size(
    const jyppx_ocv_cascade_classifier* classifier,
    int* width,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_is_old_format_cascade(
    const jyppx_ocv_cascade_classifier* classifier,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_get_feature_type(
    const jyppx_ocv_cascade_classifier* classifier,
    int* feature_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_detect_multi_scale_count(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_detect_multi_scale_fill(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int* rectangles,
    int rectangle_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_detect_multi_scale2_count(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_detect_multi_scale2_fill(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int* rectangles,
    int rectangle_capacity,
    int* num_detections,
    int num_detection_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_detect_multi_scale3_count(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int output_reject_levels,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_cascade_classifier_detect_multi_scale3_fill(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int output_reject_levels,
    int* rectangles,
    int rectangle_capacity,
    int* reject_levels,
    int reject_level_capacity,
    double* level_weights,
    int level_weight_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_create(
    jyppx_ocv_hog_descriptor** descriptor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_create_with_params(
    int win_width,
    int win_height,
    int block_width,
    int block_height,
    int block_stride_width,
    int block_stride_height,
    int cell_width,
    int cell_height,
    int nbins,
    int deriv_aperture,
    double win_sigma,
    int histogram_norm_type,
    double l2_hys_threshold,
    int gamma_correction,
    int nlevels,
    int signed_gradient,
    jyppx_ocv_hog_descriptor** descriptor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_create_from_file(
    const char* filename,
    jyppx_ocv_hog_descriptor** descriptor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_hog_descriptor_release_handle(
    jyppx_ocv_hog_descriptor* descriptor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_get_default_people_detector_count(
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_get_default_people_detector_fill(
    float* values,
    int value_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_get_daimler_people_detector_count(
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_get_daimler_people_detector_fill(
    float* values,
    int value_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_set_svm_detector(
    jyppx_ocv_hog_descriptor* descriptor,
    const float* values,
    int value_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_check_detector_size(
    const jyppx_ocv_hog_descriptor* descriptor,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_get_descriptor_size(
    const jyppx_ocv_hog_descriptor* descriptor,
    size_t* descriptor_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_get_win_sigma(
    const jyppx_ocv_hog_descriptor* descriptor,
    double* win_sigma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_get_property(
    const jyppx_ocv_hog_descriptor* descriptor,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_set_property(
    jyppx_ocv_hog_descriptor* descriptor,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_detect_count(
    const jyppx_ocv_hog_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    double hit_threshold,
    int win_stride_width,
    int win_stride_height,
    int padding_width,
    int padding_height,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_detect_fill(
    const jyppx_ocv_hog_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    double hit_threshold,
    int win_stride_width,
    int win_stride_height,
    int padding_width,
    int padding_height,
    int* locations_xy,
    int location_capacity,
    double* weights,
    int weight_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_detect_multi_scale_count(
    const jyppx_ocv_hog_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    double hit_threshold,
    int win_stride_width,
    int win_stride_height,
    int padding_width,
    int padding_height,
    double scale,
    double group_threshold,
    int use_meanshift_grouping,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hog_descriptor_detect_multi_scale_fill(
    const jyppx_ocv_hog_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    double hit_threshold,
    int win_stride_width,
    int win_stride_height,
    int padding_width,
    int padding_height,
    double scale,
    double group_threshold,
    int use_meanshift_grouping,
    int* rectangles,
    int rectangle_capacity,
    double* weights,
    int weight_capacity,
    int* count);
