#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_tracking_tracker jyppx_ocv_tracking_tracker;
typedef struct jyppx_ocv_tracking_tracker_kcf jyppx_ocv_tracking_tracker_kcf;
typedef struct jyppx_ocv_tracking_tracker_csrt jyppx_ocv_tracking_tracker_csrt;
typedef struct jyppx_ocv_tracking_legacy_tracker jyppx_ocv_tracking_legacy_tracker;
typedef struct jyppx_ocv_tracking_legacy_tracker_mosse jyppx_ocv_tracking_legacy_tracker_mosse;
typedef struct jyppx_ocv_tracking_legacy_tracker_mil jyppx_ocv_tracking_legacy_tracker_mil;
typedef struct jyppx_ocv_tracking_legacy_tracker_median_flow jyppx_ocv_tracking_legacy_tracker_median_flow;
typedef struct jyppx_ocv_tracking_legacy_tracker_boosting jyppx_ocv_tracking_legacy_tracker_boosting;
typedef struct jyppx_ocv_tracking_legacy_tracker_tld jyppx_ocv_tracking_legacy_tracker_tld;
typedef struct jyppx_ocv_tracking_legacy_tracker_kcf jyppx_ocv_tracking_legacy_tracker_kcf;
typedef struct jyppx_ocv_tracking_legacy_tracker_csrt jyppx_ocv_tracking_legacy_tracker_csrt;
typedef struct jyppx_ocv_tracking_legacy_multi_tracker jyppx_ocv_tracking_legacy_multi_tracker;

typedef struct jyppx_ocv_tracking_rect
{
    int x;
    int y;
    int width;
    int height;
} jyppx_ocv_tracking_rect;

typedef struct jyppx_ocv_tracking_rect2d
{
    double x;
    double y;
    double width;
    double height;
} jyppx_ocv_tracking_rect2d;

typedef struct jyppx_ocv_tracking_kcf_params
{
    float detect_thresh;
    float sigma;
    float lambda_value;
    float interp_factor;
    float output_sigma_factor;
    float pca_learning_rate;
    int resize;
    int split_coeff;
    int wrap_kernel;
    int compress_feature;
    int max_patch_size;
    int compressed_size;
    int desc_pca;
    int desc_npca;
} jyppx_ocv_tracking_kcf_params;

typedef struct jyppx_ocv_tracking_csrt_params
{
    int use_hog;
    int use_color_names;
    int use_gray;
    int use_rgb;
    int use_channel_weights;
    int use_segmentation;
    const char* window_function;
    float kaiser_alpha;
    float cheb_attenuation;
    float template_size;
    float gsl_sigma;
    float hog_orientations;
    float hog_clip;
    float padding;
    float filter_lr;
    float weights_lr;
    int num_hog_channels_used;
    int admm_iterations;
    int histogram_bins;
    float histogram_lr;
    int background_ratio;
    int number_of_scales;
    float scale_sigma_factor;
    float scale_model_max_area;
    float scale_lr;
    float scale_step;
    float psr_threshold;
} jyppx_ocv_tracking_csrt_params;

typedef struct jyppx_ocv_tracking_mil_params
{
    float sampler_init_in_radius;
    float sampler_search_win_size;
    int sampler_init_max_neg_num;
    float sampler_track_in_radius;
    int sampler_track_max_pos_num;
    int sampler_track_max_neg_num;
    int feature_set_num_features;
} jyppx_ocv_tracking_mil_params;

typedef struct jyppx_ocv_tracking_median_flow_params
{
    int points_in_grid;
    int win_width;
    int win_height;
    int max_level;
    int criteria_type;
    int criteria_max_count;
    double criteria_epsilon;
    int win_width_ncc;
    int win_height_ncc;
    double max_median_length_of_displacement_difference;
} jyppx_ocv_tracking_median_flow_params;

typedef struct jyppx_ocv_tracking_boosting_params
{
    int num_classifiers;
    float sampler_overlap;
    float sampler_search_factor;
    int iteration_init;
    int feature_set_num_features;
} jyppx_ocv_tracking_boosting_params;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_tracking_tracker_release_handle(
    jyppx_ocv_tracking_tracker* tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_tracking_legacy_tracker_release_handle(
    jyppx_ocv_tracking_legacy_tracker* tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_tracking_legacy_multi_tracker_release_handle(
    jyppx_ocv_tracking_legacy_multi_tracker* tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_tracker_init(
    jyppx_ocv_tracking_tracker* tracker,
    const jyppx_ocv_mat* image,
    jyppx_ocv_tracking_rect bounding_box);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_tracker_update(
    jyppx_ocv_tracking_tracker* tracker,
    const jyppx_ocv_mat* image,
    jyppx_ocv_tracking_rect* bounding_box,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_tracker_kcf_create_default(
    jyppx_ocv_tracking_tracker_kcf** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_tracker_kcf_create(
    const jyppx_ocv_tracking_kcf_params* parameters,
    jyppx_ocv_tracking_tracker_kcf** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_tracker_kcf_get_default_params(
    jyppx_ocv_tracking_kcf_params* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_tracker_csrt_create_default(
    jyppx_ocv_tracking_tracker_csrt** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_tracker_csrt_create(
    const jyppx_ocv_tracking_csrt_params* parameters,
    jyppx_ocv_tracking_tracker_csrt** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_tracker_csrt_get_default_params(
    jyppx_ocv_tracking_csrt_params* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_tracker_csrt_set_initial_mask(
    jyppx_ocv_tracking_tracker_csrt* tracker,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_init(
    jyppx_ocv_tracking_legacy_tracker* tracker,
    const jyppx_ocv_mat* image,
    jyppx_ocv_tracking_rect2d bounding_box);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_update(
    jyppx_ocv_tracking_legacy_tracker* tracker,
    const jyppx_ocv_mat* image,
    jyppx_ocv_tracking_rect2d* bounding_box,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_mosse_create(
    jyppx_ocv_tracking_legacy_tracker_mosse** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_mil_create_default(
    jyppx_ocv_tracking_legacy_tracker_mil** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_mil_create(
    const jyppx_ocv_tracking_mil_params* parameters,
    jyppx_ocv_tracking_legacy_tracker_mil** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_mil_get_default_params(
    jyppx_ocv_tracking_mil_params* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_median_flow_create_default(
    jyppx_ocv_tracking_legacy_tracker_median_flow** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_median_flow_create(
    const jyppx_ocv_tracking_median_flow_params* parameters,
    jyppx_ocv_tracking_legacy_tracker_median_flow** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_median_flow_get_default_params(
    jyppx_ocv_tracking_median_flow_params* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_boosting_create_default(
    jyppx_ocv_tracking_legacy_tracker_boosting** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_boosting_create(
    const jyppx_ocv_tracking_boosting_params* parameters,
    jyppx_ocv_tracking_legacy_tracker_boosting** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_boosting_get_default_params(
    jyppx_ocv_tracking_boosting_params* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_tld_create(
    jyppx_ocv_tracking_legacy_tracker_tld** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_kcf_create_default(
    jyppx_ocv_tracking_legacy_tracker_kcf** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_kcf_create(
    const jyppx_ocv_tracking_kcf_params* parameters,
    jyppx_ocv_tracking_legacy_tracker_kcf** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_csrt_create_default(
    jyppx_ocv_tracking_legacy_tracker_csrt** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_csrt_create(
    const jyppx_ocv_tracking_csrt_params* parameters,
    jyppx_ocv_tracking_legacy_tracker_csrt** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_tracker_csrt_set_initial_mask(
    jyppx_ocv_tracking_legacy_tracker_csrt* tracker,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_upgrade(
    const jyppx_ocv_tracking_legacy_tracker* legacy_tracker,
    jyppx_ocv_tracking_tracker** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_multi_tracker_create(
    jyppx_ocv_tracking_legacy_multi_tracker** tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_multi_tracker_add(
    jyppx_ocv_tracking_legacy_multi_tracker* multi_tracker,
    jyppx_ocv_tracking_legacy_tracker* tracker,
    const jyppx_ocv_mat* image,
    jyppx_ocv_tracking_rect2d bounding_box,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_multi_tracker_update_count(
    jyppx_ocv_tracking_legacy_multi_tracker* multi_tracker,
    const jyppx_ocv_mat* image,
    int* result,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_multi_tracker_update_fill(
    jyppx_ocv_tracking_legacy_multi_tracker* multi_tracker,
    const jyppx_ocv_mat* image,
    jyppx_ocv_tracking_rect2d* bounding_boxes,
    int bounding_box_capacity,
    int* result,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_multi_tracker_get_objects_count(
    const jyppx_ocv_tracking_legacy_multi_tracker* multi_tracker,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tracking_legacy_multi_tracker_get_objects_fill(
    const jyppx_ocv_tracking_legacy_multi_tracker* multi_tracker,
    jyppx_ocv_tracking_rect2d* bounding_boxes,
    int bounding_box_capacity,
    int* count);
