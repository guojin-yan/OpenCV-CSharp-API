#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_rapid_tracker jyppx_ocv_rapid_tracker;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_draw_correspondencies(
    jyppx_ocv_mat* bundle,
    const jyppx_ocv_mat* cols,
    const jyppx_ocv_mat* colors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_draw_search_lines(
    jyppx_ocv_mat* img,
    const jyppx_ocv_mat* locations,
    double color0,
    double color1,
    double color2,
    double color3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_draw_wireframe(
    jyppx_ocv_mat* img,
    const jyppx_ocv_mat* pts2d,
    const jyppx_ocv_mat* tris,
    double color0,
    double color1,
    double color2,
    double color3,
    int line_type,
    int cull_backface);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_extract_control_points(
    int num,
    int len,
    const jyppx_ocv_mat* pts3d,
    const jyppx_ocv_mat* rvec,
    const jyppx_ocv_mat* tvec,
    const jyppx_ocv_mat* camera_matrix,
    int image_width,
    int image_height,
    const jyppx_ocv_mat* tris,
    jyppx_ocv_mat* ctl2d,
    jyppx_ocv_mat* ctl3d);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_extract_line_bundle(
    int len,
    const jyppx_ocv_mat* ctl2d,
    const jyppx_ocv_mat* img,
    jyppx_ocv_mat* bundle,
    jyppx_ocv_mat* src_locations);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_find_correspondencies(
    const jyppx_ocv_mat* bundle,
    jyppx_ocv_mat* cols,
    jyppx_ocv_mat* response);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_convert_correspondencies(
    const jyppx_ocv_mat* cols,
    const jyppx_ocv_mat* src_locations,
    jyppx_ocv_mat* pts2d,
    jyppx_ocv_mat* pts3d,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_run(
    const jyppx_ocv_mat* img,
    int num,
    int len,
    const jyppx_ocv_mat* pts3d,
    const jyppx_ocv_mat* tris,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int compute_rmsd,
    float* ratio,
    double* rmsd);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_tracker_create(
    const jyppx_ocv_mat* pts3d,
    const jyppx_ocv_mat* tris,
    jyppx_ocv_rapid_tracker** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_ols_tracker_create(
    const jyppx_ocv_mat* pts3d,
    const jyppx_ocv_mat* tris,
    int hist_bins,
    int sobel_thresh,
    jyppx_ocv_rapid_tracker** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_rapid_tracker_release(
    jyppx_ocv_rapid_tracker* tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_tracker_compute(
    jyppx_ocv_rapid_tracker* tracker,
    const jyppx_ocv_mat* img,
    int num,
    int len,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    float* ratio);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rapid_tracker_clear_state(
    jyppx_ocv_rapid_tracker* tracker);
