#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

#include <stdint.h>

typedef struct jyppx_ocv_structured_light_pattern jyppx_ocv_structured_light_pattern;

typedef struct jyppx_ocv_structured_light_point2f
{
    float x;
    float y;
} jyppx_ocv_structured_light_point2f;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_gray_code_pattern_create(
    int width,
    int height,
    jyppx_ocv_structured_light_pattern** pattern);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_sinusoidal_pattern_create(
    int width,
    int height,
    int nbr_of_periods,
    float shift_value,
    int method_id,
    int nbr_of_pixels_between_markers,
    int horizontal,
    int set_markers,
    const jyppx_ocv_structured_light_point2f* markers,
    int marker_count,
    jyppx_ocv_structured_light_pattern** pattern);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_structured_light_pattern_release(
    jyppx_ocv_structured_light_pattern* pattern);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_pattern_generate_count(
    jyppx_ocv_structured_light_pattern* pattern,
    int* image_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_pattern_generate_fill(
    jyppx_ocv_structured_light_pattern* pattern,
    jyppx_ocv_mat** images,
    int image_capacity,
    int* image_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_gray_code_pattern_get_number_of_pattern_images(
    jyppx_ocv_structured_light_pattern* pattern,
    int* image_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_gray_code_pattern_set_white_threshold(
    jyppx_ocv_structured_light_pattern* pattern,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_gray_code_pattern_set_black_threshold(
    jyppx_ocv_structured_light_pattern* pattern,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_gray_code_pattern_get_images_for_shadow_masks(
    jyppx_ocv_structured_light_pattern* pattern,
    jyppx_ocv_mat* black_image,
    jyppx_ocv_mat* white_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_gray_code_pattern_get_proj_pixel(
    jyppx_ocv_structured_light_pattern* pattern,
    const jyppx_ocv_mat* const* pattern_images,
    int image_count,
    int x,
    int y,
    int* found,
    int* proj_x,
    int* proj_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_sinusoidal_pattern_compute_phase_map(
    jyppx_ocv_structured_light_pattern* pattern,
    const jyppx_ocv_mat* const* pattern_images,
    int image_count,
    jyppx_ocv_mat* wrapped_phase_map,
    jyppx_ocv_mat* shadow_mask,
    const jyppx_ocv_mat* fundamental);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_sinusoidal_pattern_unwrap_phase_map(
    jyppx_ocv_structured_light_pattern* pattern,
    const jyppx_ocv_mat* wrapped_phase_map,
    jyppx_ocv_mat* unwrapped_phase_map,
    int cam_width,
    int cam_height,
    const jyppx_ocv_mat* shadow_mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_structured_light_sinusoidal_pattern_compute_data_modulation_term(
    jyppx_ocv_structured_light_pattern* pattern,
    const jyppx_ocv_mat* const* pattern_images,
    int image_count,
    jyppx_ocv_mat* data_modulation_term,
    const jyppx_ocv_mat* shadow_mask);
