#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/features2d/types.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_stitcher jyppx_ocv_stitcher;
typedef struct jyppx_ocv_stitching_exposure_compensator jyppx_ocv_stitching_exposure_compensator;
typedef struct jyppx_ocv_stitching_py_rotation_warper jyppx_ocv_stitching_py_rotation_warper;
typedef struct jyppx_ocv_stitching_blender jyppx_ocv_stitching_blender;
typedef struct jyppx_ocv_stitching_image_features jyppx_ocv_stitching_image_features;
typedef struct jyppx_ocv_stitching_matches_info jyppx_ocv_stitching_matches_info;
typedef struct jyppx_ocv_stitching_features_matcher jyppx_ocv_stitching_features_matcher;

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

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_create_default(
    int type,
    int try_gpu,
    jyppx_ocv_stitching_blender** blender);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_create_feather(
    float sharpness,
    jyppx_ocv_stitching_blender** blender);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_create_multi_band(
    int try_gpu,
    int number_of_bands,
    int weight_type,
    jyppx_ocv_stitching_blender** blender);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stitching_blender_release_handle(
    jyppx_ocv_stitching_blender* blender);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_prepare(
    jyppx_ocv_stitching_blender* blender,
    const int* corner_x,
    const int* corner_y,
    const int* widths,
    const int* heights,
    int item_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_prepare_roi(
    jyppx_ocv_stitching_blender* blender,
    int x,
    int y,
    int width,
    int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_feed(
    jyppx_ocv_stitching_blender* blender,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    int top_left_x,
    int top_left_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_blend(
    jyppx_ocv_stitching_blender* blender,
    jyppx_ocv_mat* destination,
    jyppx_ocv_mat* destination_mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_get_sharpness(
    const jyppx_ocv_stitching_blender* blender,
    float* sharpness);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_set_sharpness(
    jyppx_ocv_stitching_blender* blender,
    float sharpness);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_get_number_of_bands(
    const jyppx_ocv_stitching_blender* blender,
    int* number_of_bands);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_set_number_of_bands(
    jyppx_ocv_stitching_blender* blender,
    int number_of_bands);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_blender_create_weight_maps(
    jyppx_ocv_stitching_blender* blender,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    const int* corner_x,
    const int* corner_y,
    int corner_count,
    jyppx_ocv_mat* const* weight_maps,
    int weight_map_count,
    jyppx_ocv_stitching_rect* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_normalize_using_weight_map(
    const jyppx_ocv_mat* weight,
    jyppx_ocv_mat* source);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_create_weight_map(
    const jyppx_ocv_mat* mask,
    float sharpness,
    jyppx_ocv_mat* weight);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_create_laplace_pyramid(
    const jyppx_ocv_mat* image,
    int number_of_levels,
    jyppx_ocv_mat* const* pyramid,
    int pyramid_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_create_laplace_pyramid_gpu(
    const jyppx_ocv_mat* image,
    int number_of_levels,
    jyppx_ocv_mat* const* pyramid,
    int pyramid_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_restore_image_from_laplace_pyramid(
    jyppx_ocv_mat* const* pyramid,
    int pyramid_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_restore_image_from_laplace_pyramid_gpu(
    jyppx_ocv_mat* const* pyramid,
    int pyramid_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_image_features_create(
    int image_index,
    int image_width,
    int image_height,
    const jyppx_ocv_key_point* keypoints,
    int keypoint_count,
    const jyppx_ocv_mat* descriptors,
    jyppx_ocv_stitching_image_features** features);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stitching_image_features_release_handle(
    jyppx_ocv_stitching_image_features* features);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_image_features_get_image_index(
    const jyppx_ocv_stitching_image_features* features,
    int* image_index);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_image_features_set_image_index(
    jyppx_ocv_stitching_image_features* features,
    int image_index);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_image_features_get_image_size(
    const jyppx_ocv_stitching_image_features* features,
    int* image_width,
    int* image_height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_image_features_set_image_size(
    jyppx_ocv_stitching_image_features* features,
    int image_width,
    int image_height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_image_features_get_keypoints_count(
    const jyppx_ocv_stitching_image_features* features,
    int* keypoint_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_image_features_get_keypoints_fill(
    const jyppx_ocv_stitching_image_features* features,
    jyppx_ocv_key_point* keypoints,
    int keypoint_capacity,
    int* keypoint_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_image_features_copy_descriptors(
    const jyppx_ocv_stitching_image_features* features,
    jyppx_ocv_mat* descriptors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_compute_image_features(
    int finder_kind,
    const void* finder_handle,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_stitching_image_features* features);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_compute_image_features_batch(
    int finder_kind,
    const void* finder_handle,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    jyppx_ocv_stitching_image_features* const* features,
    int feature_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_matches_info_create(
    jyppx_ocv_stitching_matches_info** matches_info);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stitching_matches_info_release_handle(
    jyppx_ocv_stitching_matches_info* matches_info);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_matches_info_get_metadata(
    const jyppx_ocv_stitching_matches_info* matches_info,
    int* source_image_index,
    int* destination_image_index,
    int* number_of_inliers,
    double* confidence);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_matches_info_copy_homography(
    const jyppx_ocv_stitching_matches_info* matches_info,
    jyppx_ocv_mat* homography);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_matches_info_get_matches_count(
    const jyppx_ocv_stitching_matches_info* matches_info,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_matches_info_get_matches_fill(
    const jyppx_ocv_stitching_matches_info* matches_info,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_matches_info_get_inliers_count(
    const jyppx_ocv_stitching_matches_info* matches_info,
    int* inlier_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_matches_info_get_inliers_fill(
    const jyppx_ocv_stitching_matches_info* matches_info,
    unsigned char* inliers,
    int inlier_capacity,
    int* inlier_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_features_matcher_create_best_of_two_nearest(
    int try_gpu,
    float match_confidence,
    int number_of_matches_threshold1,
    int number_of_matches_threshold2,
    double matches_confidence_threshold,
    jyppx_ocv_stitching_features_matcher** matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_features_matcher_factory_best_of_two_nearest(
    int try_gpu,
    float match_confidence,
    int number_of_matches_threshold1,
    int number_of_matches_threshold2,
    double matches_confidence_threshold,
    jyppx_ocv_stitching_features_matcher** matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_features_matcher_create_range(
    int range_width,
    int try_gpu,
    float match_confidence,
    int number_of_matches_threshold1,
    int number_of_matches_threshold2,
    jyppx_ocv_stitching_features_matcher** matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_features_matcher_create_affine(
    int full_affine,
    int try_gpu,
    float match_confidence,
    int number_of_matches_threshold1,
    jyppx_ocv_stitching_features_matcher** matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stitching_features_matcher_release_handle(
    jyppx_ocv_stitching_features_matcher* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_features_matcher_match_pair(
    jyppx_ocv_stitching_features_matcher* matcher,
    const jyppx_ocv_stitching_image_features* first,
    const jyppx_ocv_stitching_image_features* second,
    jyppx_ocv_stitching_matches_info* matches_info);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_features_matcher_match_batch(
    jyppx_ocv_stitching_features_matcher* matcher,
    const jyppx_ocv_stitching_image_features* const* features,
    int feature_count,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_stitching_matches_info* const* pairwise_matches,
    int pairwise_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_features_matcher_is_thread_safe(
    const jyppx_ocv_stitching_features_matcher* matcher,
    int* is_thread_safe);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stitching_features_matcher_collect_garbage(
    jyppx_ocv_stitching_features_matcher* matcher);
