#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_phase_unwrapping jyppx_ocv_phase_unwrapping;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_phase_unwrapping_histogram_create(
    int width,
    int height,
    float hist_thresh,
    int nbr_of_small_bins,
    int nbr_of_large_bins,
    jyppx_ocv_phase_unwrapping** phase_unwrapping);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_phase_unwrapping_release(
    jyppx_ocv_phase_unwrapping* phase_unwrapping);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_phase_unwrapping_unwrap_phase_map(
    jyppx_ocv_phase_unwrapping* phase_unwrapping,
    const jyppx_ocv_mat* wrapped_phase_map,
    jyppx_ocv_mat* unwrapped_phase_map,
    const jyppx_ocv_mat* shadow_mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_phase_unwrapping_histogram_get_inverse_reliability_map(
    jyppx_ocv_phase_unwrapping* phase_unwrapping,
    jyppx_ocv_mat* reliability_map);
