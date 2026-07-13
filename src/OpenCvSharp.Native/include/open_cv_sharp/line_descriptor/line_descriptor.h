#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/features2d/types.h"
#include "open_cv_sharp/status.h"

#include <stdint.h>

typedef struct jyppx_ocv_line_descriptor_binary_descriptor jyppx_ocv_line_descriptor_binary_descriptor;
typedef struct jyppx_ocv_line_descriptor_binary_descriptor_matcher jyppx_ocv_line_descriptor_binary_descriptor_matcher;

typedef struct jyppx_ocv_line_descriptor_key_line
{
    float angle;
    int32_t class_id;
    int32_t octave;
    float pt_x;
    float pt_y;
    float response;
    float size;
    float start_point_x;
    float start_point_y;
    float end_point_x;
    float end_point_y;
    float start_point_in_octave_x;
    float start_point_in_octave_y;
    float end_point_in_octave_x;
    float end_point_in_octave_y;
    float line_length;
    int32_t num_of_pixels;
} jyppx_ocv_line_descriptor_key_line;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_create(
    int num_of_octave,
    int width_of_band,
    int reduction_ratio,
    int ksize,
    jyppx_ocv_line_descriptor_binary_descriptor** descriptor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_line_descriptor_binary_descriptor_release(
    jyppx_ocv_line_descriptor_binary_descriptor* descriptor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_clear(
    jyppx_ocv_line_descriptor_binary_descriptor* descriptor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_empty(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    int* empty);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_descriptor_size(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_descriptor_type(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_default_norm(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_get_num_of_octaves(
    jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_set_num_of_octaves(
    jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_get_width_of_band(
    jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_set_width_of_band(
    jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_get_reduction_ratio(
    jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_set_reduction_ratio(
    jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_detect_count(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    int* keyline_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_detect_fill(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_line_descriptor_key_line* keylines,
    int keyline_capacity,
    int* keyline_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_compute(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_line_descriptor_key_line* keylines_in,
    int keyline_count,
    jyppx_ocv_line_descriptor_key_line* keylines_out,
    int keyline_capacity,
    int* written_keyline_count,
    jyppx_ocv_mat* descriptors,
    int return_float_descriptor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_detect_and_compute_count(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_line_descriptor_key_line* keylines_in,
    int keyline_count,
    int use_provided_keylines,
    int return_float_descriptor,
    int* output_keyline_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_detect_and_compute_fill(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_line_descriptor_key_line* keylines_in,
    int keyline_count,
    int use_provided_keylines,
    int return_float_descriptor,
    jyppx_ocv_line_descriptor_key_line* keylines_out,
    int keyline_capacity,
    int* output_keyline_count,
    jyppx_ocv_mat* descriptors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_draw_keylines(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_line_descriptor_key_line* keylines,
    int keyline_count,
    jyppx_ocv_mat* out_image,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_draw_line_matches(
    const jyppx_ocv_mat* img1,
    const jyppx_ocv_line_descriptor_key_line* keylines1,
    int keyline1_count,
    const jyppx_ocv_mat* img2,
    const jyppx_ocv_line_descriptor_key_line* keylines2,
    int keyline2_count,
    const jyppx_ocv_dmatch* matches,
    int match_count,
    jyppx_ocv_mat* out_image,
    double match_color_v0,
    double match_color_v1,
    double match_color_v2,
    double match_color_v3,
    double single_line_color_v0,
    double single_line_color_v1,
    double single_line_color_v2,
    double single_line_color_v3,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_matcher_create(
    jyppx_ocv_line_descriptor_binary_descriptor_matcher** matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_line_descriptor_binary_descriptor_matcher_release(
    jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_matcher_clear(
    jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_matcher_empty(
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    int* empty);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_count(
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_fill(
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_count(
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_fill(
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);
