#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_create_kernel_from_functions(
    const jyppx_ocv_mat* function_x,
    const jyppx_ocv_mat* function_y,
    jyppx_ocv_mat* kernel,
    int channels);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_create_kernel(
    int function_type,
    int radius,
    jyppx_ocv_mat* kernel,
    int channels);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_inpaint(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_mat* output,
    int radius,
    int function_type,
    int algorithm);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_filter(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft02d_components(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* components,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft02d_inverse_ft(
    const jyppx_ocv_mat* components,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output,
    int width,
    int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft02d_process(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft02d_iteration(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_mat* mask_output,
    int first_stop,
    int* state);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft02d_fl_process(
    const jyppx_ocv_mat* matrix,
    int radius,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft02d_fl_process_float(
    const jyppx_ocv_mat* matrix,
    int radius,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft12d_components(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* components);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft12d_polynomial(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* c00,
    jyppx_ocv_mat* c10,
    jyppx_ocv_mat* c01,
    jyppx_ocv_mat* components,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_vertical(
    int radius,
    jyppx_ocv_mat* matrix,
    int channels);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_horizontal(
    int radius,
    jyppx_ocv_mat* matrix,
    int channels);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft12d_inverse_ft(
    const jyppx_ocv_mat* components,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output,
    int width,
    int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_fuzzy_ft12d_process(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output,
    const jyppx_ocv_mat* mask);
