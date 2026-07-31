#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_stitcher jyppx_ocv_stitcher;
typedef struct jyppx_ocv_stitching_exposure_compensator jyppx_ocv_stitching_exposure_compensator;
typedef struct jyppx_ocv_stitching_py_rotation_warper jyppx_ocv_stitching_py_rotation_warper;

typedef struct jyppx_ocv_stitching_point2f
{
    float x;
    float y;
} jyppx_ocv_stitching_point2f;

typedef struct jyppx_ocv_stitching_point
{
    int x;
    int y;
} jyppx_ocv_stitching_point;

typedef struct jyppx_ocv_stitching_rect
{
    int x;
    int y;
    int width;
    int height;
} jyppx_ocv_stitching_rect;

typedef struct jyppx_ocv_stitching_camera_params
{
    double focal;
    double aspect;
    double ppx;
    double ppy;
    jyppx_ocv_mat* r;
    jyppx_ocv_mat* t;
} jyppx_ocv_stitching_camera_params;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_create(
    int mode,
    jyppx_ocv_stitcher** stitcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stitcher_release_handle(
    jyppx_ocv_stitcher* stitcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_double_property(
    const jyppx_ocv_stitcher* stitcher,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_set_double_property(
    jyppx_ocv_stitcher* stitcher,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_int_property(
    const jyppx_ocv_stitcher* stitcher,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_set_int_property(
    jyppx_ocv_stitcher* stitcher,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_estimate_transform(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int* status_code);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_compose_panorama(
    jyppx_ocv_stitcher* stitcher,
    jyppx_ocv_mat* pano,
    int* status_code);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_compose_panorama_images(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    jyppx_ocv_mat* pano,
    int* status_code);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_stitch(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    jyppx_ocv_mat* pano,
    int* status_code);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_component_count(
    const jyppx_ocv_stitcher* stitcher,
    int* component_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_component_fill(
    const jyppx_ocv_stitcher* stitcher,
    int* components,
    int component_capacity,
    int* component_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_cameras_count(
    const jyppx_ocv_stitcher* stitcher,
    int* camera_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_cameras_fill(
    const jyppx_ocv_stitcher* stitcher,
    jyppx_ocv_stitching_camera_params* cameras,
    int camera_capacity,
    int* camera_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitcher_get_result_mask(
    const jyppx_ocv_stitcher* stitcher,
    jyppx_ocv_mat* result_mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_create_default(
    int type,
    jyppx_ocv_stitching_exposure_compensator** compensator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_create_no(
    jyppx_ocv_stitching_exposure_compensator** compensator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_create_gain(
    int number_of_feeds,
    jyppx_ocv_stitching_exposure_compensator** compensator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_create_channels(
    int number_of_feeds,
    jyppx_ocv_stitching_exposure_compensator** compensator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_create_blocks_gain(
    int block_width,
    int block_height,
    int number_of_feeds,
    jyppx_ocv_stitching_exposure_compensator** compensator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_create_blocks_channels(
    int block_width,
    int block_height,
    int number_of_feeds,
    jyppx_ocv_stitching_exposure_compensator** compensator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stitching_exposure_release_handle(
    jyppx_ocv_stitching_exposure_compensator* compensator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_feed(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    const int* corner_x,
    const int* corner_y,
    int corner_count,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_apply(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    int index,
    int corner_x,
    int corner_y,
    jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_get_mat_gains_count(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    int* gain_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_get_mat_gains_fill(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    jyppx_ocv_mat** gains,
    int gain_capacity,
    int* gain_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_set_mat_gains(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    const jyppx_ocv_mat* const* gains,
    int gain_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_get_update_gain(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    int* update_gain);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_set_update_gain(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    int update_gain);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_get_number_of_feeds(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    int* number_of_feeds);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_set_number_of_feeds(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    int number_of_feeds);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_get_similarity_threshold(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    double* similarity_threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_set_similarity_threshold(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    double similarity_threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_get_block_size(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    int* block_width,
    int* block_height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_set_block_size(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    int block_width,
    int block_height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_get_filtering_iterations(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    int* filtering_iterations);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_exposure_set_filtering_iterations(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    int filtering_iterations);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_py_rotation_warper_create_default(
    jyppx_ocv_stitching_py_rotation_warper** warper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_py_rotation_warper_create(
    const unsigned char* type_utf8,
    int type_byte_count,
    float scale,
    jyppx_ocv_stitching_py_rotation_warper** warper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stitching_py_rotation_warper_release_handle(
    jyppx_ocv_stitching_py_rotation_warper* warper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_py_rotation_warper_warp_point(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    float point_x,
    float point_y,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    jyppx_ocv_stitching_point2f* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_py_rotation_warper_warp_point_backward(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    float point_x,
    float point_y,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    jyppx_ocv_stitching_point2f* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_py_rotation_warper_build_maps(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    int source_width,
    int source_height,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    jyppx_ocv_mat* x_map,
    jyppx_ocv_mat* y_map,
    jyppx_ocv_stitching_rect* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_py_rotation_warper_warp(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    int interpolation_mode,
    int border_mode,
    jyppx_ocv_mat* destination,
    jyppx_ocv_stitching_point* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_py_rotation_warper_warp_backward(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    int interpolation_mode,
    int border_mode,
    int destination_width,
    int destination_height,
    jyppx_ocv_mat* destination);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_py_rotation_warper_warp_roi(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    int source_width,
    int source_height,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    jyppx_ocv_stitching_rect* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_py_rotation_warper_get_scale(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    float* scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_py_rotation_warper_set_scale(
    jyppx_ocv_stitching_py_rotation_warper* warper,
    float scale);
