#pragma once

#include <stddef.h>

#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_mat jyppx_ocv_mat;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_create_empty(jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_create(int rows, int cols, int type, jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_create_with_scalar(int rows, int cols, int type, double v0, double v1, double v2, double v3, jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_create_in_place(jyppx_ocv_mat* mat, int rows, int cols, int type);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_zeros(int rows, int cols, int type, jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_ones(int rows, int cols, int type, jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_eye(int rows, int cols, int type, jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_mat_release(jyppx_ocv_mat* mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_clone(const jyppx_ocv_mat* mat, jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_copy_to(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_convert_to(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int rtype, double alpha, double beta);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_set_to(jyppx_ocv_mat* mat, double v0, double v1, double v2, double v3);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_submat(const jyppx_ocv_mat* mat, int x, int y, int width, int height, jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_row_range(const jyppx_ocv_mat* mat, int start_row, int end_row, jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_col_range(const jyppx_ocv_mat* mat, int start_col, int end_col, jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_reshape(const jyppx_ocv_mat* mat, int channels, int rows, jyppx_ocv_mat** out_mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_empty(const jyppx_ocv_mat* mat, int* out_empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_dims(const jyppx_ocv_mat* mat, int* out_dims);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_rows(const jyppx_ocv_mat* mat, int* out_rows);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_cols(const jyppx_ocv_mat* mat, int* out_cols);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_channels(const jyppx_ocv_mat* mat, int* out_channels);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_depth(const jyppx_ocv_mat* mat, int* out_depth);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_type(const jyppx_ocv_mat* mat, int* out_type);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_total(const jyppx_ocv_mat* mat, size_t* out_total);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_elem_size(const jyppx_ocv_mat* mat, size_t* out_elem_size);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_elem_size1(const jyppx_ocv_mat* mat, size_t* out_elem_size1);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_step(const jyppx_ocv_mat* mat, size_t* out_step);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_step1(const jyppx_ocv_mat* mat, size_t* out_step1);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_data(const jyppx_ocv_mat* mat, unsigned char** out_data);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_is_continuous(const jyppx_ocv_mat* mat, int* out_is_continuous);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mat_is_submatrix(const jyppx_ocv_mat* mat, int* out_is_submatrix);
