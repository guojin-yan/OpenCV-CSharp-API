#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_qrcode_detector jyppx_ocv_qrcode_detector;
typedef struct jyppx_ocv_barcode_detector jyppx_ocv_barcode_detector;
typedef struct jyppx_ocv_qrcode_detector_aruco jyppx_ocv_qrcode_detector_aruco;
typedef struct jyppx_ocv_qrcode_encoder jyppx_ocv_qrcode_encoder;
typedef struct jyppx_ocv_face_detector_yn jyppx_ocv_face_detector_yn;
typedef struct jyppx_ocv_face_recognizer_sf jyppx_ocv_face_recognizer_sf;
typedef struct jyppx_ocv_aruco_detector_params jyppx_ocv_aruco_detector_params;

typedef struct jyppx_ocv_qrcode_detector_aruco_params
{
    float min_module_size_in_pyramid;
    float max_rotation;
    float max_module_size_mismatch;
    float max_timing_pattern_mismatch;
    float max_penalties;
    float max_colors_mismatch;
    float scale_timing_pattern_score;
} jyppx_ocv_qrcode_detector_aruco_params;

typedef struct jyppx_ocv_qrcode_encoder_params
{
    int version;
    int correction_level;
    int mode;
    int structure_number;
} jyppx_ocv_qrcode_encoder_params;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_create(
    jyppx_ocv_qrcode_detector** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_qrcode_detector_release_handle(
    jyppx_ocv_qrcode_detector* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_set_eps_x(
    jyppx_ocv_qrcode_detector* detector,
    double eps_x);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_set_eps_y(
    jyppx_ocv_qrcode_detector* detector,
    double eps_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_set_use_alignment_markers(
    jyppx_ocv_qrcode_detector* detector,
    int use_alignment_markers);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_detect(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* detected);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_decode_length(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_decode_fill(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_detect_and_decode_length(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_detect_and_decode_fill(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_decode_curved_length(
    jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_decode_curved_fill(
    jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_detect_and_decode_curved_length(
    jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_detect_and_decode_curved_fill(
    jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_detect_multi(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* detected);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_decode_multi_count(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_decode_multi_fill(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_detect_and_decode_multi_count(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_detect_and_decode_multi_fill(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_get_encoding(
    const jyppx_ocv_qrcode_detector* detector,
    int code_index,
    int* encoding);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_create(
    jyppx_ocv_barcode_detector** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_create_with_super_resolution(
    const char* super_resolution_model_path,
    jyppx_ocv_barcode_detector** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_barcode_detector_release_handle(
    jyppx_ocv_barcode_detector* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_detect(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* detected);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_decode_count(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_decode_fill(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_decode_with_type_count(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* decoded,
    int* info_count,
    int* info_byte_count,
    int* type_count,
    int* type_byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_decode_with_type_fill(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* info_offsets,
    int info_offset_capacity,
    char* info_buffer,
    int info_buffer_capacity,
    int* type_offsets,
    int type_offset_capacity,
    char* type_buffer,
    int type_buffer_capacity,
    int* decoded,
    int* info_count,
    int* info_byte_count,
    int* type_count,
    int* type_byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_detect_and_decode_count(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_detect_and_decode_fill(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_detect_and_decode_with_type_count(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* decoded,
    int* info_count,
    int* info_byte_count,
    int* type_count,
    int* type_byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_detect_and_decode_with_type_fill(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* info_offsets,
    int info_offset_capacity,
    char* info_buffer,
    int info_buffer_capacity,
    int* type_offsets,
    int type_offset_capacity,
    char* type_buffer,
    int type_buffer_capacity,
    int* decoded,
    int* info_count,
    int* info_byte_count,
    int* type_count,
    int* type_byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_get_downsampling_threshold(
    const jyppx_ocv_barcode_detector* detector,
    double* threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_set_downsampling_threshold(
    jyppx_ocv_barcode_detector* detector,
    double threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_get_gradient_threshold(
    const jyppx_ocv_barcode_detector* detector,
    double* threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_set_gradient_threshold(
    jyppx_ocv_barcode_detector* detector,
    double threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_get_detector_scales_count(
    const jyppx_ocv_barcode_detector* detector,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_get_detector_scales_fill(
    const jyppx_ocv_barcode_detector* detector,
    float* scales,
    int scale_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_barcode_detector_set_detector_scales(
    jyppx_ocv_barcode_detector* detector,
    const float* scales,
    int scale_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_default_params(
    jyppx_ocv_qrcode_detector_aruco_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_create(
    jyppx_ocv_qrcode_detector_aruco** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_create_with_params(
    const jyppx_ocv_qrcode_detector_aruco_params* params,
    jyppx_ocv_qrcode_detector_aruco** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_qrcode_detector_aruco_release_handle(
    jyppx_ocv_qrcode_detector_aruco* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_get_detector_parameters(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    jyppx_ocv_qrcode_detector_aruco_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_set_detector_parameters(
    jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_qrcode_detector_aruco_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_get_aruco_parameters(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    jyppx_ocv_aruco_detector_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_set_aruco_parameters(
    jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_aruco_detector_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_detect(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* detected);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_decode_length(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_decode_fill(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_detect_and_decode_length(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_detect_and_decode_fill(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_detect_multi(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* detected);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_decode_multi_count(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_decode_multi_fill(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_detect_and_decode_multi_count(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_detector_aruco_detect_and_decode_multi_fill(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_encoder_default_params(
    jyppx_ocv_qrcode_encoder_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_encoder_create(
    const jyppx_ocv_qrcode_encoder_params* params,
    jyppx_ocv_qrcode_encoder** encoder);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_qrcode_encoder_release_handle(
    jyppx_ocv_qrcode_encoder* encoder);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_encoder_encode(
    const jyppx_ocv_qrcode_encoder* encoder,
    const char* encoded_info,
    jyppx_ocv_mat* qrcode);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_encoder_encode_structured_append_count(
    const jyppx_ocv_qrcode_encoder* encoder,
    const char* encoded_info,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_qrcode_encoder_encode_structured_append_fill(
    const jyppx_ocv_qrcode_encoder* encoder,
    const char* encoded_info,
    jyppx_ocv_mat** qrcodes,
    int qrcode_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_create(
    const char* model,
    const char* config,
    int input_width,
    int input_height,
    float score_threshold,
    float nms_threshold,
    int top_k,
    int backend_id,
    int target_id,
    jyppx_ocv_face_detector_yn** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_create_from_buffer(
    const char* framework,
    const unsigned char* model_buffer,
    int model_buffer_length,
    const unsigned char* config_buffer,
    int config_buffer_length,
    int input_width,
    int input_height,
    float score_threshold,
    float nms_threshold,
    int top_k,
    int backend_id,
    int target_id,
    jyppx_ocv_face_detector_yn** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_face_detector_yn_release_handle(
    jyppx_ocv_face_detector_yn* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_set_input_size(
    jyppx_ocv_face_detector_yn* detector,
    int width,
    int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_get_input_size(
    jyppx_ocv_face_detector_yn* detector,
    int* width,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_set_score_threshold(
    jyppx_ocv_face_detector_yn* detector,
    float score_threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_get_score_threshold(
    jyppx_ocv_face_detector_yn* detector,
    float* score_threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_set_nms_threshold(
    jyppx_ocv_face_detector_yn* detector,
    float nms_threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_get_nms_threshold(
    jyppx_ocv_face_detector_yn* detector,
    float* nms_threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_set_top_k(
    jyppx_ocv_face_detector_yn* detector,
    int top_k);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_get_top_k(
    jyppx_ocv_face_detector_yn* detector,
    int* top_k);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_detector_yn_detect(
    jyppx_ocv_face_detector_yn* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* faces,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_sf_create(
    const char* model,
    const char* config,
    int backend_id,
    int target_id,
    jyppx_ocv_face_recognizer_sf** recognizer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_sf_create_from_buffer(
    const char* framework,
    const unsigned char* model_buffer,
    int model_buffer_length,
    const unsigned char* config_buffer,
    int config_buffer_length,
    int backend_id,
    int target_id,
    jyppx_ocv_face_recognizer_sf** recognizer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_face_recognizer_sf_release_handle(
    jyppx_ocv_face_recognizer_sf* recognizer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_sf_align_crop(
    const jyppx_ocv_face_recognizer_sf* recognizer,
    const jyppx_ocv_mat* source_image,
    const jyppx_ocv_mat* face_box,
    jyppx_ocv_mat* aligned_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_sf_feature(
    jyppx_ocv_face_recognizer_sf* recognizer,
    const jyppx_ocv_mat* aligned_image,
    jyppx_ocv_mat* face_feature);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_face_recognizer_sf_match(
    const jyppx_ocv_face_recognizer_sf* recognizer,
    const jyppx_ocv_mat* face_feature1,
    const jyppx_ocv_mat* face_feature2,
    int distance_type,
    double* result);
