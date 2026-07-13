#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_intensity_transform_log(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_intensity_transform_gamma_correction(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float gamma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_intensity_transform_autoscaling(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_intensity_transform_contrast_stretching(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int r1,
    int s1,
    int r2,
    int s2);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_intensity_transform_bimef(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float mu,
    float a,
    float b);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_intensity_transform_bimef_with_k(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float k,
    float mu,
    float a,
    float b);
