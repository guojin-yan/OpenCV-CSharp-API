#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

#include <stddef.h>
#include <stdint.h>

typedef struct jyppx_ocv_core_file_storage jyppx_ocv_core_file_storage;
typedef struct jyppx_ocv_core_file_node jyppx_ocv_core_file_node;
typedef struct jyppx_ocv_core_utf8_result jyppx_ocv_core_utf8_result;
typedef struct jyppx_ocv_core_string_list jyppx_ocv_core_string_list;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_create(
    jyppx_ocv_core_file_storage** out_storage);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_core_file_storage_release_handle(
    jyppx_ocv_core_file_storage* storage);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_open(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* source_utf8,
    int source_byte_length,
    int flags,
    const unsigned char* encoding_utf8,
    int encoding_byte_length,
    int* out_opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_is_opened(
    const jyppx_ocv_core_file_storage* storage,
    int* out_opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_release(
    jyppx_ocv_core_file_storage* storage);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_release_and_get_string(
    jyppx_ocv_core_file_storage* storage,
    jyppx_ocv_core_utf8_result** out_result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_get_first_top_level_node(
    const jyppx_ocv_core_file_storage* storage,
    jyppx_ocv_core_file_node** out_node);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_root(
    const jyppx_ocv_core_file_storage* storage,
    int stream_index,
    jyppx_ocv_core_file_node** out_node);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_get_node(
    const jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    jyppx_ocv_core_file_node** out_node);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_write_int(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_write_bool(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_write_int64(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    int64_t value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_write_double(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_write_string(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    const unsigned char* value_utf8,
    int value_byte_length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_write_mat(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    const jyppx_ocv_mat* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_write_string_vector(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    const unsigned char* values_utf8,
    int values_byte_length,
    const int* value_offsets,
    const int* value_lengths,
    int value_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_write_comment(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* comment_utf8,
    int comment_byte_length,
    int append);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_start_write_struct(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    int flags,
    const unsigned char* type_name_utf8,
    int type_name_byte_length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_end_write_struct(
    jyppx_ocv_core_file_storage* storage);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_storage_get_format(
    const jyppx_ocv_core_file_storage* storage,
    int* out_format);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_create(
    jyppx_ocv_core_file_node** out_node);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_core_file_node_release(
    jyppx_ocv_core_file_node* node);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_get_node(
    const jyppx_ocv_core_file_node* node,
    const unsigned char* name_utf8,
    int name_byte_length,
    jyppx_ocv_core_file_node** out_node);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_at(
    const jyppx_ocv_core_file_node* node,
    int index,
    jyppx_ocv_core_file_node** out_node);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_keys(
    const jyppx_ocv_core_file_node* node,
    jyppx_ocv_core_string_list** out_keys);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_type(
    const jyppx_ocv_core_file_node* node,
    int* out_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_empty(
    const jyppx_ocv_core_file_node* node,
    int* out_empty);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_name(
    const jyppx_ocv_core_file_node* node,
    jyppx_ocv_core_utf8_result** out_result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_size(
    const jyppx_ocv_core_file_node* node,
    size_t* out_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_raw_size(
    const jyppx_ocv_core_file_node* node,
    size_t* out_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_real(
    const jyppx_ocv_core_file_node* node,
    double* out_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_string(
    const jyppx_ocv_core_file_node* node,
    jyppx_ocv_core_utf8_result** out_result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_file_node_mat(
    const jyppx_ocv_core_file_node* node,
    jyppx_ocv_mat* out_mat);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_string_list_count(
    const jyppx_ocv_core_string_list* values,
    size_t* out_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_string_list_get(
    const jyppx_ocv_core_string_list* values,
    size_t index,
    jyppx_ocv_core_utf8_result** out_result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_core_string_list_release(
    jyppx_ocv_core_string_list* values);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_utf8_result_size(
    const jyppx_ocv_core_utf8_result* result,
    size_t* out_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_core_utf8_result_data(
    const jyppx_ocv_core_utf8_result* result,
    const unsigned char** out_data);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_core_utf8_result_release(
    jyppx_ocv_core_utf8_result* result);
