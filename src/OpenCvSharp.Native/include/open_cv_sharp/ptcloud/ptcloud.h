#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_rgbd_normals jyppx_ocv_rgbd_normals;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ptcloud_register_depth(
    const jyppx_ocv_mat* unregistered_camera_matrix,
    const jyppx_ocv_mat* registered_camera_matrix,
    const jyppx_ocv_mat* registered_dist_coeffs,
    const jyppx_ocv_mat* rt,
    const jyppx_ocv_mat* unregistered_depth,
    int output_width,
    int output_height,
    jyppx_ocv_mat* registered_depth,
    int depth_dilation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ptcloud_depth_to_3d(
    const jyppx_ocv_mat* depth,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* points3d,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ptcloud_depth_to_3d_sparse(
    const jyppx_ocv_mat* depth,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* points3d);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ptcloud_rescale_depth(
    const jyppx_ocv_mat* src,
    int type,
    jyppx_ocv_mat* dst,
    double depth_factor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ptcloud_warp_frame(
    const jyppx_ocv_mat* depth,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_mat* rt,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* warped_depth,
    jyppx_ocv_mat* warped_image,
    jyppx_ocv_mat* warped_mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ptcloud_find_planes(
    const jyppx_ocv_mat* points3d,
    const jyppx_ocv_mat* normals,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat* plane_coefficients,
    int block_size,
    int min_size,
    double threshold,
    double sensor_error_a,
    double sensor_error_b,
    double sensor_error_c,
    int method);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rgbd_normals_create(
    int rows,
    int cols,
    int depth,
    const jyppx_ocv_mat* camera_matrix,
    int window_size,
    float diff_threshold,
    int method,
    jyppx_ocv_rgbd_normals** normals);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_rgbd_normals_release_handle(
    jyppx_ocv_rgbd_normals* normals);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rgbd_normals_apply(
    const jyppx_ocv_rgbd_normals* normals,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rgbd_normals_cache(
    const jyppx_ocv_rgbd_normals* normals);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rgbd_normals_get_int_property(
    const jyppx_ocv_rgbd_normals* normals,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rgbd_normals_set_int_property(
    jyppx_ocv_rgbd_normals* normals,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rgbd_normals_get_k(
    const jyppx_ocv_rgbd_normals* normals,
    jyppx_ocv_mat* camera_matrix);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_rgbd_normals_set_k(
    jyppx_ocv_rgbd_normals* normals,
    const jyppx_ocv_mat* camera_matrix);
