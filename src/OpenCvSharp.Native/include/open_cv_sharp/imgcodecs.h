#pragma once

#include <stddef.h>

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_encoded_buffer jyppx_ocv_encoded_buffer;
typedef struct jyppx_ocv_imgcodecs_mat_vector jyppx_ocv_imgcodecs_mat_vector;
typedef struct jyppx_ocv_imgcodecs_metadata_result jyppx_ocv_imgcodecs_metadata_result;
typedef struct jyppx_ocv_imgcodecs_animation jyppx_ocv_imgcodecs_animation;
typedef struct jyppx_ocv_imgcodecs_image_collection jyppx_ocv_imgcodecs_image_collection;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imencode(
    const char* ext,
    const jyppx_ocv_mat* image,
    jyppx_ocv_encoded_buffer** out_buffer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imencode_with_params(
    const char* ext,
    const jyppx_ocv_mat* image,
    const int* params,
    size_t params_length,
    jyppx_ocv_encoded_buffer** out_buffer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imdecode(
    const unsigned char* buffer,
    size_t buffer_length,
    int flags,
    jyppx_ocv_mat** out_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imread(
    const char* filename,
    int flags,
    jyppx_ocv_mat** out_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imread_into(
    const char* filename,
    int flags,
    jyppx_ocv_mat* image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imread_multi(
    const char* filename,
    int flags,
    int has_range,
    int start,
    int count,
    jyppx_ocv_imgcodecs_mat_vector** out_images,
    int* out_success);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imdecode_multi(
    const unsigned char* buffer,
    size_t buffer_length,
    int flags,
    int has_range,
    int start,
    int end,
    jyppx_ocv_imgcodecs_mat_vector** out_images,
    int* out_success);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imwrite_multi(
    const char* filename,
    const jyppx_ocv_mat* const* images,
    size_t image_count,
    const int* params,
    size_t params_length,
    int* out_written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imencode_multi(
    const char* ext,
    const jyppx_ocv_mat* const* images,
    size_t image_count,
    const int* params,
    size_t params_length,
    jyppx_ocv_encoded_buffer** out_buffer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imcount(
    const char* filename,
    int flags,
    size_t* out_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_have_image_reader(
    const char* filename,
    int* out_available);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_have_image_writer(
    const char* filename_or_extension,
    int* out_available);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_mat_vector_count(
    const jyppx_ocv_imgcodecs_mat_vector* images,
    size_t* out_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_mat_vector_clone_at(
    const jyppx_ocv_imgcodecs_mat_vector* images,
    size_t index,
    jyppx_ocv_mat** out_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_imgcodecs_mat_vector_release(
    jyppx_ocv_imgcodecs_mat_vector* images);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imread_with_metadata(
    const char* filename,
    int flags,
    jyppx_ocv_imgcodecs_metadata_result** out_result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imdecode_with_metadata(
    const unsigned char* buffer,
    size_t buffer_length,
    int flags,
    jyppx_ocv_imgcodecs_metadata_result** out_result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imwrite_with_metadata(
    const char* filename,
    const jyppx_ocv_mat* image,
    const int* metadata_types,
    const jyppx_ocv_mat* const* metadata,
    size_t metadata_count,
    const int* params,
    size_t params_length,
    int* out_written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imencode_with_metadata(
    const char* ext,
    const jyppx_ocv_mat* image,
    const int* metadata_types,
    const jyppx_ocv_mat* const* metadata,
    size_t metadata_count,
    const int* params,
    size_t params_length,
    jyppx_ocv_encoded_buffer** out_buffer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_metadata_result_image_clone(
    const jyppx_ocv_imgcodecs_metadata_result* result,
    jyppx_ocv_mat** out_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_metadata_result_count(
    const jyppx_ocv_imgcodecs_metadata_result* result,
    size_t* out_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_metadata_result_clone_at(
    const jyppx_ocv_imgcodecs_metadata_result* result,
    size_t index,
    int* out_type,
    jyppx_ocv_mat** out_metadata);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_imgcodecs_metadata_result_release(
    jyppx_ocv_imgcodecs_metadata_result* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_animation_create(
    int loop_count,
    double bg0,
    double bg1,
    double bg2,
    double bg3,
    jyppx_ocv_imgcodecs_animation** out_animation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_imgcodecs_animation_release(
    jyppx_ocv_imgcodecs_animation* animation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_animation_get_loop_count(
    const jyppx_ocv_imgcodecs_animation* animation,
    int* out_loop_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_animation_set_loop_count(
    jyppx_ocv_imgcodecs_animation* animation,
    int loop_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_animation_get_background_color(
    const jyppx_ocv_imgcodecs_animation* animation,
    double* out_bg0,
    double* out_bg1,
    double* out_bg2,
    double* out_bg3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_animation_set_background_color(
    jyppx_ocv_imgcodecs_animation* animation,
    double bg0,
    double bg1,
    double bg2,
    double bg3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_animation_set_frames(
    jyppx_ocv_imgcodecs_animation* animation,
    const jyppx_ocv_mat* const* frames,
    const int* durations,
    size_t frame_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_animation_frame_count(
    const jyppx_ocv_imgcodecs_animation* animation,
    size_t* out_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_animation_frame_clone_at(
    const jyppx_ocv_imgcodecs_animation* animation,
    size_t index,
    jyppx_ocv_mat** out_frame,
    int* out_duration);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_animation_still_image_clone(
    const jyppx_ocv_imgcodecs_animation* animation,
    jyppx_ocv_mat** out_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_animation_set_still_image(
    jyppx_ocv_imgcodecs_animation* animation,
    const jyppx_ocv_mat* image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imread_animation(
    const char* filename,
    int start,
    int count,
    jyppx_ocv_imgcodecs_animation* animation,
    int* out_success);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imdecode_animation(
    const unsigned char* buffer,
    size_t buffer_length,
    int start,
    int count,
    jyppx_ocv_imgcodecs_animation* animation,
    int* out_success);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imwrite_animation(
    const char* filename,
    const jyppx_ocv_imgcodecs_animation* animation,
    const int* params,
    size_t params_length,
    int* out_written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imencode_animation(
    const char* ext,
    const jyppx_ocv_imgcodecs_animation* animation,
    const int* params,
    size_t params_length,
    jyppx_ocv_encoded_buffer** out_buffer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_image_collection_create(
    jyppx_ocv_imgcodecs_image_collection** out_collection);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_image_collection_create_file(
    const char* filename,
    int flags,
    jyppx_ocv_imgcodecs_image_collection** out_collection);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_imgcodecs_image_collection_release(
    jyppx_ocv_imgcodecs_image_collection* collection);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_image_collection_init(
    jyppx_ocv_imgcodecs_image_collection* collection,
    const char* filename,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_image_collection_size(
    const jyppx_ocv_imgcodecs_image_collection* collection,
    size_t* out_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_image_collection_clone_at(
    jyppx_ocv_imgcodecs_image_collection* collection,
    int index,
    jyppx_ocv_mat** out_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_image_collection_release_cache(
    jyppx_ocv_imgcodecs_image_collection* collection,
    int index);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imwrite(
    const char* filename,
    const jyppx_ocv_mat* image,
    int* out_written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgcodecs_imwrite_with_params(
    const char* filename,
    const jyppx_ocv_mat* image,
    const int* params,
    size_t params_length,
    int* out_written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_encoded_buffer_size(
    const jyppx_ocv_encoded_buffer* buffer,
    size_t* out_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_encoded_buffer_data(
    const jyppx_ocv_encoded_buffer* buffer,
    const unsigned char** out_data);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_encoded_buffer_release(
    jyppx_ocv_encoded_buffer* buffer);
