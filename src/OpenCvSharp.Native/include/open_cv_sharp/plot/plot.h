#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_plot_2d jyppx_ocv_plot_2d;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_create(
    const jyppx_ocv_mat* data,
    jyppx_ocv_plot_2d** plot);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_create_xy(
    const jyppx_ocv_mat* data_x,
    const jyppx_ocv_mat* data_y,
    jyppx_ocv_plot_2d** plot);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_plot_2d_release_handle(
    jyppx_ocv_plot_2d* plot);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_min_x(
    jyppx_ocv_plot_2d* plot,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_min_y(
    jyppx_ocv_plot_2d* plot,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_max_x(
    jyppx_ocv_plot_2d* plot,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_max_y(
    jyppx_ocv_plot_2d* plot,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_plot_line_width(
    jyppx_ocv_plot_2d* plot,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_need_plot_line(
    jyppx_ocv_plot_2d* plot,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_plot_line_color(
    jyppx_ocv_plot_2d* plot,
    double v0,
    double v1,
    double v2,
    double v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_plot_background_color(
    jyppx_ocv_plot_2d* plot,
    double v0,
    double v1,
    double v2,
    double v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_plot_axis_color(
    jyppx_ocv_plot_2d* plot,
    double v0,
    double v1,
    double v2,
    double v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_plot_grid_color(
    jyppx_ocv_plot_2d* plot,
    double v0,
    double v1,
    double v2,
    double v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_plot_text_color(
    jyppx_ocv_plot_2d* plot,
    double v0,
    double v1,
    double v2,
    double v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_plot_size(
    jyppx_ocv_plot_2d* plot,
    int width,
    int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_show_grid(
    jyppx_ocv_plot_2d* plot,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_show_text(
    jyppx_ocv_plot_2d* plot,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_grid_lines_number(
    jyppx_ocv_plot_2d* plot,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_invert_orientation(
    jyppx_ocv_plot_2d* plot,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_set_point_idx_to_print(
    jyppx_ocv_plot_2d* plot,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_plot_2d_render(
    jyppx_ocv_plot_2d* plot,
    jyppx_ocv_mat* result);
