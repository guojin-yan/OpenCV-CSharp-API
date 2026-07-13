#pragma once

#include <stddef.h>

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_encoded_buffer jyppx_ocv_encoded_buffer;

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
