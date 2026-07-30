#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/features2d/types.h"
#include "open_cv_sharp/status.h"

#include <stddef.h>

typedef struct jyppx_ocv_features2d_orb jyppx_ocv_features2d_orb;
typedef struct jyppx_ocv_features2d_descriptor_matcher jyppx_ocv_features2d_descriptor_matcher;
typedef struct jyppx_ocv_features2d_bf_matcher jyppx_ocv_features2d_bf_matcher;
typedef struct jyppx_ocv_features2d_sift jyppx_ocv_features2d_sift;
typedef struct jyppx_ocv_features2d_fast jyppx_ocv_features2d_fast;
typedef struct jyppx_ocv_features2d_gftt jyppx_ocv_features2d_gftt;
typedef struct jyppx_ocv_features2d_flann_matcher jyppx_ocv_features2d_flann_matcher;
typedef struct jyppx_ocv_features2d_mser jyppx_ocv_features2d_mser;
typedef struct jyppx_ocv_features2d_simple_blob jyppx_ocv_features2d_simple_blob;
typedef struct jyppx_ocv_features2d_brisk jyppx_ocv_features2d_brisk;
typedef struct jyppx_ocv_features2d_kaze jyppx_ocv_features2d_kaze;
typedef struct jyppx_ocv_features2d_akaze jyppx_ocv_features2d_akaze;
typedef struct jyppx_ocv_features2d_affine jyppx_ocv_features2d_affine;
typedef struct jyppx_ocv_features2d_ann_index jyppx_ocv_features2d_ann_index;
typedef struct jyppx_ocv_features2d_bow_kmeans_trainer jyppx_ocv_features2d_bow_kmeans_trainer;
typedef struct jyppx_ocv_features2d_bow_img_descriptor_extractor jyppx_ocv_features2d_bow_img_descriptor_extractor;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_create(
    int max_features,
    float scale_factor,
    int nlevels,
    int edge_threshold,
    int first_level,
    int wta_k,
    int score_type,
    int patch_size,
    int fast_threshold,
    jyppx_ocv_features2d_orb** orb);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_orb_release(
    jyppx_ocv_features2d_orb* orb);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_clear(
    jyppx_ocv_features2d_orb* orb);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_empty(
    const jyppx_ocv_features2d_orb* orb,
    int* empty);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_get_max_features(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_set_max_features(
    jyppx_ocv_features2d_orb* orb,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_get_scale_factor(
    const jyppx_ocv_features2d_orb* orb,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_set_scale_factor(
    jyppx_ocv_features2d_orb* orb,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_get_nlevels(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_set_nlevels(
    jyppx_ocv_features2d_orb* orb,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_get_edge_threshold(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_set_edge_threshold(
    jyppx_ocv_features2d_orb* orb,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_get_first_level(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_set_first_level(
    jyppx_ocv_features2d_orb* orb,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_get_wta_k(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_set_wta_k(
    jyppx_ocv_features2d_orb* orb,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_get_score_type(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_set_score_type(
    jyppx_ocv_features2d_orb* orb,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_get_patch_size(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_set_patch_size(
    jyppx_ocv_features2d_orb* orb,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_get_fast_threshold(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_set_fast_threshold(
    jyppx_ocv_features2d_orb* orb,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_descriptor_size(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_descriptor_type(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_default_norm(
    const jyppx_ocv_features2d_orb* orb,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_default_name_length(
    const jyppx_ocv_features2d_orb* orb,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_default_name_fill(
    const jyppx_ocv_features2d_orb* orb,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_detect_count(
    const jyppx_ocv_features2d_orb* orb,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    int* keypoint_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_detect_fill(
    const jyppx_ocv_features2d_orb* orb,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_key_point* keypoints,
    int keypoint_capacity,
    int* keypoint_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_compute(
    const jyppx_ocv_features2d_orb* orb,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_key_point* keypoints_in,
    int keypoint_count,
    jyppx_ocv_key_point* keypoints_out,
    int keypoint_capacity,
    int* written_keypoint_count,
    jyppx_ocv_mat* descriptors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_detect_and_compute_count(
    const jyppx_ocv_features2d_orb* orb,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_key_point* keypoints_in,
    int keypoint_count,
    int use_provided_keypoints,
    int* output_keypoint_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_orb_detect_and_compute_fill(
    const jyppx_ocv_features2d_orb* orb,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_key_point* keypoints_in,
    int keypoint_count,
    int use_provided_keypoints,
    jyppx_ocv_key_point* keypoints_out,
    int keypoint_capacity,
    int* output_keypoint_count,
    jyppx_ocv_mat* descriptors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_create(
    int norm_type,
    int cross_check,
    jyppx_ocv_features2d_bf_matcher** matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_bf_matcher_release(
    jyppx_ocv_features2d_bf_matcher* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_get_norm_type(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    int* norm_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_get_cross_check(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    int* cross_check);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_is_mask_supported(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    int* supported);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_empty(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    int* empty);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_clear(
    jyppx_ocv_features2d_bf_matcher* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_train(
    jyppx_ocv_features2d_bf_matcher* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_add(
    jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* const* descriptors,
    int descriptor_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_create_by_type(
    int matcher_type,
    jyppx_ocv_features2d_descriptor_matcher** matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_create_by_name(
    const char* matcher_name,
    int matcher_name_length,
    jyppx_ocv_features2d_descriptor_matcher** matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_descriptor_matcher_release(
    jyppx_ocv_features2d_descriptor_matcher* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_clone(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    int empty_train_data,
    jyppx_ocv_features2d_descriptor_matcher** clone);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_is_mask_supported(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    int* supported);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_empty(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    int* empty);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_clear(
    jyppx_ocv_features2d_descriptor_matcher* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_train(
    jyppx_ocv_features2d_descriptor_matcher* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_add(
    jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* const* descriptors,
    int descriptor_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_get_train_descriptors_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    int* descriptor_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_get_train_descriptor_clone(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    int index,
    jyppx_ocv_mat** descriptor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_match_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_match_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_match_train_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_match_train_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_knn_match_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_knn_match_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_knn_match_train_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_knn_match_train_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_radius_match_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    float max_distance,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_radius_match_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    float max_distance,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_radius_match_train_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_descriptor_matcher_radius_match_train_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_clone(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    int empty_train_data,
    jyppx_ocv_features2d_descriptor_matcher** clone);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_get_train_descriptors_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    int* descriptor_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_get_train_descriptor_clone(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    int index,
    jyppx_ocv_mat** descriptor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_match_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_match_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_match_train_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_match_train_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_match_train_with_masks_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_match_train_with_masks_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_knn_match_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_knn_match_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_knn_match_train_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_knn_match_train_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_radius_match_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    float max_distance,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_radius_match_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    float max_distance,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_radius_match_train_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_radius_match_train_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_draw_keypoints(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_key_point* keypoints,
    int keypoint_count,
    jyppx_ocv_mat* out_image,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_draw_matches(
    const jyppx_ocv_mat* img1,
    const jyppx_ocv_key_point* keypoints1,
    int keypoint1_count,
    const jyppx_ocv_mat* img2,
    const jyppx_ocv_key_point* keypoints2,
    int keypoint2_count,
    const jyppx_ocv_dmatch* matches,
    int match_count,
    jyppx_ocv_mat* out_image,
    double match_color_v0,
    double match_color_v1,
    double match_color_v2,
    double match_color_v3,
    double single_point_color_v0,
    double single_point_color_v1,
    double single_point_color_v2,
    double single_point_color_v3,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_draw_matches_knn(
    const jyppx_ocv_mat* img1,
    const jyppx_ocv_key_point* keypoints1,
    int keypoint1_count,
    const jyppx_ocv_mat* img2,
    const jyppx_ocv_key_point* keypoints2,
    int keypoint2_count,
    const int* offsets,
    int offset_count,
    const jyppx_ocv_dmatch* matches,
    int match_count,
    jyppx_ocv_mat* out_image,
    double match_color_v0,
    double match_color_v1,
    double match_color_v2,
    double match_color_v3,
    double single_point_color_v0,
    double single_point_color_v1,
    double single_point_color_v2,
    double single_point_color_v3,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_create(
    int nfeatures,
    int n_octave_layers,
    double contrast_threshold,
    double edge_threshold,
    double sigma,
    int descriptor_type,
    int enable_precise_upscale,
    jyppx_ocv_features2d_sift** sift);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_sift_release(
    jyppx_ocv_features2d_sift* sift);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_clear(jyppx_ocv_features2d_sift* sift);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_empty(const jyppx_ocv_features2d_sift* sift, int* empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_descriptor_size(const jyppx_ocv_features2d_sift* sift, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_descriptor_type(const jyppx_ocv_features2d_sift* sift, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_default_norm(const jyppx_ocv_features2d_sift* sift, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_default_name_length(const jyppx_ocv_features2d_sift* sift, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_default_name_fill(const jyppx_ocv_features2d_sift* sift, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_get_nfeatures(const jyppx_ocv_features2d_sift* sift, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_set_nfeatures(jyppx_ocv_features2d_sift* sift, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_get_n_octave_layers(const jyppx_ocv_features2d_sift* sift, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_set_n_octave_layers(jyppx_ocv_features2d_sift* sift, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_get_contrast_threshold(const jyppx_ocv_features2d_sift* sift, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_set_contrast_threshold(jyppx_ocv_features2d_sift* sift, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_get_edge_threshold(const jyppx_ocv_features2d_sift* sift, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_set_edge_threshold(jyppx_ocv_features2d_sift* sift, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_get_sigma(const jyppx_ocv_features2d_sift* sift, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_set_sigma(jyppx_ocv_features2d_sift* sift, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_detect_count(const jyppx_ocv_features2d_sift* sift, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_detect_fill(const jyppx_ocv_features2d_sift* sift, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_compute(const jyppx_ocv_features2d_sift* sift, const jyppx_ocv_mat* image, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* written_keypoint_count, jyppx_ocv_mat* descriptors);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_detect_and_compute_count(const jyppx_ocv_features2d_sift* sift, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, int* output_keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_sift_detect_and_compute_fill(const jyppx_ocv_features2d_sift* sift, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* output_keypoint_count, jyppx_ocv_mat* descriptors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_create(int threshold, int nonmax_suppression, int type, jyppx_ocv_features2d_fast** fast);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_fast_release(jyppx_ocv_features2d_fast* fast);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_clear(jyppx_ocv_features2d_fast* fast);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_empty(const jyppx_ocv_features2d_fast* fast, int* empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_descriptor_size(const jyppx_ocv_features2d_fast* fast, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_descriptor_type(const jyppx_ocv_features2d_fast* fast, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_default_norm(const jyppx_ocv_features2d_fast* fast, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_default_name_length(const jyppx_ocv_features2d_fast* fast, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_default_name_fill(const jyppx_ocv_features2d_fast* fast, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_get_threshold(const jyppx_ocv_features2d_fast* fast, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_set_threshold(jyppx_ocv_features2d_fast* fast, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_get_nonmax_suppression(const jyppx_ocv_features2d_fast* fast, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_set_nonmax_suppression(jyppx_ocv_features2d_fast* fast, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_get_type(const jyppx_ocv_features2d_fast* fast, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_set_type(jyppx_ocv_features2d_fast* fast, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_detect_count(const jyppx_ocv_features2d_fast* fast, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_fast_detect_fill(const jyppx_ocv_features2d_fast* fast, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_create(int max_corners, double quality_level, double min_distance, int block_size, int gradient_size, int use_harris_detector, double k, jyppx_ocv_features2d_gftt** gftt);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_gftt_release(jyppx_ocv_features2d_gftt* gftt);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_clear(jyppx_ocv_features2d_gftt* gftt);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_empty(const jyppx_ocv_features2d_gftt* gftt, int* empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_descriptor_size(const jyppx_ocv_features2d_gftt* gftt, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_descriptor_type(const jyppx_ocv_features2d_gftt* gftt, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_default_norm(const jyppx_ocv_features2d_gftt* gftt, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_default_name_length(const jyppx_ocv_features2d_gftt* gftt, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_default_name_fill(const jyppx_ocv_features2d_gftt* gftt, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_get_max_features(const jyppx_ocv_features2d_gftt* gftt, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_set_max_features(jyppx_ocv_features2d_gftt* gftt, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_get_quality_level(const jyppx_ocv_features2d_gftt* gftt, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_set_quality_level(jyppx_ocv_features2d_gftt* gftt, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_get_min_distance(const jyppx_ocv_features2d_gftt* gftt, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_set_min_distance(jyppx_ocv_features2d_gftt* gftt, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_get_block_size(const jyppx_ocv_features2d_gftt* gftt, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_set_block_size(jyppx_ocv_features2d_gftt* gftt, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_get_gradient_size(const jyppx_ocv_features2d_gftt* gftt, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_set_gradient_size(jyppx_ocv_features2d_gftt* gftt, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_get_harris_detector(const jyppx_ocv_features2d_gftt* gftt, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_set_harris_detector(jyppx_ocv_features2d_gftt* gftt, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_get_k(const jyppx_ocv_features2d_gftt* gftt, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_set_k(jyppx_ocv_features2d_gftt* gftt, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_detect_count(const jyppx_ocv_features2d_gftt* gftt, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_gftt_detect_fill(const jyppx_ocv_features2d_gftt* gftt, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_create(jyppx_ocv_features2d_flann_matcher** matcher);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_flann_matcher_release(jyppx_ocv_features2d_flann_matcher* matcher);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_clone(const jyppx_ocv_features2d_flann_matcher* matcher, int empty_train_data, jyppx_ocv_features2d_descriptor_matcher** clone);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_is_mask_supported(const jyppx_ocv_features2d_flann_matcher* matcher, int* supported);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_empty(const jyppx_ocv_features2d_flann_matcher* matcher, int* empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_clear(jyppx_ocv_features2d_flann_matcher* matcher);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_train(jyppx_ocv_features2d_flann_matcher* matcher);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_add(jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* const* descriptors, int descriptor_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_get_train_descriptors_count(const jyppx_ocv_features2d_flann_matcher* matcher, int* descriptor_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_get_train_descriptor_clone(const jyppx_ocv_features2d_flann_matcher* matcher, int index, jyppx_ocv_mat** descriptor);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_match_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, int* match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_match_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, jyppx_ocv_dmatch* matches, int match_capacity, int* match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_match_train_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, int* match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_match_train_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, jyppx_ocv_dmatch* matches, int match_capacity, int* match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_knn_match_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, int k, int compact_result, int* group_count, int* total_match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_knn_match_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, int k, int compact_result, int* offsets, int offset_capacity, jyppx_ocv_dmatch* matches, int match_capacity, int* group_count, int* total_match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_knn_match_train_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, int k, int compact_result, int* group_count, int* total_match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_knn_match_train_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, int k, int compact_result, int* offsets, int offset_capacity, jyppx_ocv_dmatch* matches, int match_capacity, int* group_count, int* total_match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_radius_match_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, float max_distance, int compact_result, int* group_count, int* total_match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_radius_match_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, float max_distance, int compact_result, int* offsets, int offset_capacity, jyppx_ocv_dmatch* matches, int match_capacity, int* group_count, int* total_match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_radius_match_train_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, float max_distance, int compact_result, int* group_count, int* total_match_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_flann_matcher_radius_match_train_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, float max_distance, int compact_result, int* offsets, int offset_capacity, jyppx_ocv_dmatch* matches, int match_capacity, int* group_count, int* total_match_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_create(
    int delta,
    int min_area,
    int max_area,
    double max_variation,
    double min_diversity,
    int max_evolution,
    double area_threshold,
    double min_margin,
    int edge_blur_size,
    jyppx_ocv_features2d_mser** mser);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_mser_release(
    jyppx_ocv_features2d_mser* mser);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_clear(jyppx_ocv_features2d_mser* mser);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_empty(const jyppx_ocv_features2d_mser* mser, int* empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_descriptor_size(const jyppx_ocv_features2d_mser* mser, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_descriptor_type(const jyppx_ocv_features2d_mser* mser, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_default_norm(const jyppx_ocv_features2d_mser* mser, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_default_name_length(const jyppx_ocv_features2d_mser* mser, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_default_name_fill(const jyppx_ocv_features2d_mser* mser, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_detect_count(const jyppx_ocv_features2d_mser* mser, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_detect_fill(const jyppx_ocv_features2d_mser* mser, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_get_delta(const jyppx_ocv_features2d_mser* mser, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_set_delta(jyppx_ocv_features2d_mser* mser, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_get_min_area(const jyppx_ocv_features2d_mser* mser, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_set_min_area(jyppx_ocv_features2d_mser* mser, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_get_max_area(const jyppx_ocv_features2d_mser* mser, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_set_max_area(jyppx_ocv_features2d_mser* mser, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_get_max_variation(const jyppx_ocv_features2d_mser* mser, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_set_max_variation(jyppx_ocv_features2d_mser* mser, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_get_min_diversity(const jyppx_ocv_features2d_mser* mser, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_set_min_diversity(jyppx_ocv_features2d_mser* mser, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_get_max_evolution(const jyppx_ocv_features2d_mser* mser, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_set_max_evolution(jyppx_ocv_features2d_mser* mser, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_get_area_threshold(const jyppx_ocv_features2d_mser* mser, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_set_area_threshold(jyppx_ocv_features2d_mser* mser, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_get_min_margin(const jyppx_ocv_features2d_mser* mser, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_set_min_margin(jyppx_ocv_features2d_mser* mser, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_get_edge_blur_size(const jyppx_ocv_features2d_mser* mser, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_set_edge_blur_size(jyppx_ocv_features2d_mser* mser, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_get_pass2_only(const jyppx_ocv_features2d_mser* mser, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_set_pass2_only(jyppx_ocv_features2d_mser* mser, int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_detect_regions_count(
    const jyppx_ocv_features2d_mser* mser,
    const jyppx_ocv_mat* image,
    int* region_count,
    int* total_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_mser_detect_regions_fill(
    const jyppx_ocv_features2d_mser* mser,
    const jyppx_ocv_mat* image,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_point* points,
    int point_capacity,
    jyppx_ocv_rect* bboxes,
    int bbox_capacity,
    int* region_count,
    int* total_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_create_default(
    jyppx_ocv_features2d_simple_blob** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_create(
    const jyppx_ocv_simple_blob_params* parameters,
    jyppx_ocv_features2d_simple_blob** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_simple_blob_release(
    jyppx_ocv_features2d_simple_blob* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_clear(jyppx_ocv_features2d_simple_blob* detector);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_empty(const jyppx_ocv_features2d_simple_blob* detector, int* empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_descriptor_size(const jyppx_ocv_features2d_simple_blob* detector, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_descriptor_type(const jyppx_ocv_features2d_simple_blob* detector, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_default_norm(const jyppx_ocv_features2d_simple_blob* detector, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_default_name_length(const jyppx_ocv_features2d_simple_blob* detector, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_default_name_fill(const jyppx_ocv_features2d_simple_blob* detector, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_detect_count(const jyppx_ocv_features2d_simple_blob* detector, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_detect_fill(const jyppx_ocv_features2d_simple_blob* detector, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_get_params(const jyppx_ocv_features2d_simple_blob* detector, jyppx_ocv_simple_blob_params* parameters);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_set_params(jyppx_ocv_features2d_simple_blob* detector, const jyppx_ocv_simple_blob_params* parameters);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_get_blob_contours_count(const jyppx_ocv_features2d_simple_blob* detector, int* contour_count, int* total_point_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_simple_blob_get_blob_contours_fill(const jyppx_ocv_features2d_simple_blob* detector, int* offsets, int offset_capacity, jyppx_ocv_point* points, int point_capacity, int* contour_count, int* total_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_create(
    int threshold,
    int octaves,
    float pattern_scale,
    jyppx_ocv_features2d_brisk** brisk);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_create_pattern(
    const float* radius_list,
    int radius_count,
    const int* number_list,
    int number_count,
    float d_max,
    float d_min,
    const int* index_change,
    int index_change_count,
    jyppx_ocv_features2d_brisk** brisk);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_create_pattern_with_threshold(
    int threshold,
    int octaves,
    const float* radius_list,
    int radius_count,
    const int* number_list,
    int number_count,
    float d_max,
    float d_min,
    const int* index_change,
    int index_change_count,
    jyppx_ocv_features2d_brisk** brisk);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_brisk_release(jyppx_ocv_features2d_brisk* brisk);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_clear(jyppx_ocv_features2d_brisk* brisk);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_empty(const jyppx_ocv_features2d_brisk* brisk, int* empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_descriptor_size(const jyppx_ocv_features2d_brisk* brisk, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_descriptor_type(const jyppx_ocv_features2d_brisk* brisk, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_default_norm(const jyppx_ocv_features2d_brisk* brisk, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_default_name_length(const jyppx_ocv_features2d_brisk* brisk, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_default_name_fill(const jyppx_ocv_features2d_brisk* brisk, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_detect_count(const jyppx_ocv_features2d_brisk* brisk, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_detect_fill(const jyppx_ocv_features2d_brisk* brisk, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_compute(const jyppx_ocv_features2d_brisk* brisk, const jyppx_ocv_mat* image, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* written_keypoint_count, jyppx_ocv_mat* descriptors);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_detect_and_compute_count(const jyppx_ocv_features2d_brisk* brisk, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, int* output_keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_detect_and_compute_fill(const jyppx_ocv_features2d_brisk* brisk, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* output_keypoint_count, jyppx_ocv_mat* descriptors);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_get_threshold(const jyppx_ocv_features2d_brisk* brisk, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_set_threshold(jyppx_ocv_features2d_brisk* brisk, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_get_octaves(const jyppx_ocv_features2d_brisk* brisk, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_set_octaves(jyppx_ocv_features2d_brisk* brisk, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_get_pattern_scale(const jyppx_ocv_features2d_brisk* brisk, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_brisk_set_pattern_scale(jyppx_ocv_features2d_brisk* brisk, float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_create(
    int extended,
    int upright,
    float threshold,
    int n_octaves,
    int n_octave_layers,
    int diffusivity,
    jyppx_ocv_features2d_kaze** kaze);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_kaze_release(jyppx_ocv_features2d_kaze* kaze);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_clear(jyppx_ocv_features2d_kaze* kaze);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_empty(const jyppx_ocv_features2d_kaze* kaze, int* empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_descriptor_size(const jyppx_ocv_features2d_kaze* kaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_descriptor_type(const jyppx_ocv_features2d_kaze* kaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_default_norm(const jyppx_ocv_features2d_kaze* kaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_default_name_length(const jyppx_ocv_features2d_kaze* kaze, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_default_name_fill(const jyppx_ocv_features2d_kaze* kaze, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_detect_count(const jyppx_ocv_features2d_kaze* kaze, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_detect_fill(const jyppx_ocv_features2d_kaze* kaze, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_compute(const jyppx_ocv_features2d_kaze* kaze, const jyppx_ocv_mat* image, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* written_keypoint_count, jyppx_ocv_mat* descriptors);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_detect_and_compute_count(const jyppx_ocv_features2d_kaze* kaze, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, int* output_keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_detect_and_compute_fill(const jyppx_ocv_features2d_kaze* kaze, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* output_keypoint_count, jyppx_ocv_mat* descriptors);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_get_extended(const jyppx_ocv_features2d_kaze* kaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_set_extended(jyppx_ocv_features2d_kaze* kaze, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_get_upright(const jyppx_ocv_features2d_kaze* kaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_set_upright(jyppx_ocv_features2d_kaze* kaze, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_get_threshold(const jyppx_ocv_features2d_kaze* kaze, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_set_threshold(jyppx_ocv_features2d_kaze* kaze, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_get_n_octaves(const jyppx_ocv_features2d_kaze* kaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_set_n_octaves(jyppx_ocv_features2d_kaze* kaze, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_get_n_octave_layers(const jyppx_ocv_features2d_kaze* kaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_set_n_octave_layers(jyppx_ocv_features2d_kaze* kaze, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_get_diffusivity(const jyppx_ocv_features2d_kaze* kaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_kaze_set_diffusivity(jyppx_ocv_features2d_kaze* kaze, int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_create(
    int descriptor_type,
    int descriptor_size,
    int descriptor_channels,
    float threshold,
    int n_octaves,
    int n_octave_layers,
    int diffusivity,
    int max_points,
    jyppx_ocv_features2d_akaze** akaze);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_akaze_release(jyppx_ocv_features2d_akaze* akaze);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_clear(jyppx_ocv_features2d_akaze* akaze);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_empty(const jyppx_ocv_features2d_akaze* akaze, int* empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_descriptor_size(const jyppx_ocv_features2d_akaze* akaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_descriptor_type(const jyppx_ocv_features2d_akaze* akaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_default_norm(const jyppx_ocv_features2d_akaze* akaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_default_name_length(const jyppx_ocv_features2d_akaze* akaze, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_default_name_fill(const jyppx_ocv_features2d_akaze* akaze, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_detect_count(const jyppx_ocv_features2d_akaze* akaze, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_detect_fill(const jyppx_ocv_features2d_akaze* akaze, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_compute(const jyppx_ocv_features2d_akaze* akaze, const jyppx_ocv_mat* image, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* written_keypoint_count, jyppx_ocv_mat* descriptors);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_detect_and_compute_count(const jyppx_ocv_features2d_akaze* akaze, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, int* output_keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_detect_and_compute_fill(const jyppx_ocv_features2d_akaze* akaze, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* output_keypoint_count, jyppx_ocv_mat* descriptors);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_get_descriptor_type(const jyppx_ocv_features2d_akaze* akaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_set_descriptor_type(jyppx_ocv_features2d_akaze* akaze, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_get_descriptor_size(const jyppx_ocv_features2d_akaze* akaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_set_descriptor_size(jyppx_ocv_features2d_akaze* akaze, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_get_descriptor_channels(const jyppx_ocv_features2d_akaze* akaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_set_descriptor_channels(jyppx_ocv_features2d_akaze* akaze, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_get_threshold(const jyppx_ocv_features2d_akaze* akaze, double* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_set_threshold(jyppx_ocv_features2d_akaze* akaze, double value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_get_n_octaves(const jyppx_ocv_features2d_akaze* akaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_set_n_octaves(jyppx_ocv_features2d_akaze* akaze, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_get_n_octave_layers(const jyppx_ocv_features2d_akaze* akaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_set_n_octave_layers(jyppx_ocv_features2d_akaze* akaze, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_get_diffusivity(const jyppx_ocv_features2d_akaze* akaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_set_diffusivity(jyppx_ocv_features2d_akaze* akaze, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_get_max_points(const jyppx_ocv_features2d_akaze* akaze, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_akaze_set_max_points(jyppx_ocv_features2d_akaze* akaze, int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_create_from_orb(
    const jyppx_ocv_features2d_orb* backend,
    int max_tilt,
    int min_tilt,
    float tilt_step,
    float rotate_step_base,
    jyppx_ocv_features2d_affine** affine);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_create_from_sift(
    const jyppx_ocv_features2d_sift* backend,
    int max_tilt,
    int min_tilt,
    float tilt_step,
    float rotate_step_base,
    jyppx_ocv_features2d_affine** affine);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_create_from_fast(
    const jyppx_ocv_features2d_fast* backend,
    int max_tilt,
    int min_tilt,
    float tilt_step,
    float rotate_step_base,
    jyppx_ocv_features2d_affine** affine);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_create_from_gftt(
    const jyppx_ocv_features2d_gftt* backend,
    int max_tilt,
    int min_tilt,
    float tilt_step,
    float rotate_step_base,
    jyppx_ocv_features2d_affine** affine);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_create_from_mser(
    const jyppx_ocv_features2d_mser* backend,
    int max_tilt,
    int min_tilt,
    float tilt_step,
    float rotate_step_base,
    jyppx_ocv_features2d_affine** affine);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_create_from_simple_blob(
    const jyppx_ocv_features2d_simple_blob* backend,
    int max_tilt,
    int min_tilt,
    float tilt_step,
    float rotate_step_base,
    jyppx_ocv_features2d_affine** affine);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_create_from_brisk(
    const jyppx_ocv_features2d_brisk* backend,
    int max_tilt,
    int min_tilt,
    float tilt_step,
    float rotate_step_base,
    jyppx_ocv_features2d_affine** affine);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_create_from_kaze(
    const jyppx_ocv_features2d_kaze* backend,
    int max_tilt,
    int min_tilt,
    float tilt_step,
    float rotate_step_base,
    jyppx_ocv_features2d_affine** affine);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_create_from_akaze(
    const jyppx_ocv_features2d_akaze* backend,
    int max_tilt,
    int min_tilt,
    float tilt_step,
    float rotate_step_base,
    jyppx_ocv_features2d_affine** affine);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_affine_release(jyppx_ocv_features2d_affine* affine);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_clear(jyppx_ocv_features2d_affine* affine);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_empty(const jyppx_ocv_features2d_affine* affine, int* empty);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_descriptor_size(const jyppx_ocv_features2d_affine* affine, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_descriptor_type(const jyppx_ocv_features2d_affine* affine, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_default_norm(const jyppx_ocv_features2d_affine* affine, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_default_name_length(const jyppx_ocv_features2d_affine* affine, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_default_name_fill(const jyppx_ocv_features2d_affine* affine, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_detect_count(const jyppx_ocv_features2d_affine* affine, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_detect_fill(const jyppx_ocv_features2d_affine* affine, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_set_view_params(jyppx_ocv_features2d_affine* affine, const float* tilts, int tilt_count, const float* rolls, int roll_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_get_view_params_count(const jyppx_ocv_features2d_affine* affine, int* tilt_count, int* roll_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_affine_get_view_params_fill(const jyppx_ocv_features2d_affine* affine, float* tilts, int tilt_capacity, float* rolls, int roll_capacity, int* tilt_count, int* roll_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_ann_index_create(
    int dimension,
    int distance,
    jyppx_ocv_features2d_ann_index** index);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_features2d_ann_index_release(
    jyppx_ocv_features2d_ann_index* index);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_ann_index_add_items(
    jyppx_ocv_features2d_ann_index* index,
    const jyppx_ocv_mat* features);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_ann_index_build(
    jyppx_ocv_features2d_ann_index* index,
    int trees);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_ann_index_knn_search(
    const jyppx_ocv_features2d_ann_index* index,
    const jyppx_ocv_mat* query,
    jyppx_ocv_mat* indices,
    jyppx_ocv_mat* distances,
    int knn,
    int search_k);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_ann_index_save(
    jyppx_ocv_features2d_ann_index* index,
    const unsigned char* filename_utf8,
    int filename_byte_length,
    int prefault);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_ann_index_load(
    jyppx_ocv_features2d_ann_index* index,
    const unsigned char* filename_utf8,
    int filename_byte_length,
    int prefault);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_ann_index_get_tree_number(
    const jyppx_ocv_features2d_ann_index* index,
    int* tree_number);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_ann_index_get_item_number(
    const jyppx_ocv_features2d_ann_index* index,
    int* item_number);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_ann_index_set_on_disk_build(
    jyppx_ocv_features2d_ann_index* index,
    const unsigned char* filename_utf8,
    int filename_byte_length,
    int* enabled);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_features2d_ann_index_set_seed(
    jyppx_ocv_features2d_ann_index* index,
    int seed);
