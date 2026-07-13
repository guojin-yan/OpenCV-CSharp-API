#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_hfs_segment jyppx_ocv_hfs_segment;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hfs_segment_create(
    int height,
    int width,
    float seg_egb_threshold_i,
    int min_region_size_i,
    float seg_egb_threshold_ii,
    int min_region_size_ii,
    float spatial_weight,
    int slic_spixel_size,
    int num_slic_iter,
    jyppx_ocv_hfs_segment** segment);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_hfs_segment_release(
    jyppx_ocv_hfs_segment* segment);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hfs_segment_get_float_property(
    const jyppx_ocv_hfs_segment* segment,
    int property_id,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hfs_segment_set_float_property(
    jyppx_ocv_hfs_segment* segment,
    int property_id,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hfs_segment_get_int_property(
    const jyppx_ocv_hfs_segment* segment,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hfs_segment_set_int_property(
    jyppx_ocv_hfs_segment* segment,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hfs_segment_perform_segment_cpu(
    jyppx_ocv_hfs_segment* segment,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int draw);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_hfs_segment_perform_segment_gpu(
    jyppx_ocv_hfs_segment* segment,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int draw);
