#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_kalman_filter jyppx_ocv_kalman_filter;
typedef struct jyppx_ocv_background_subtractor jyppx_ocv_background_subtractor;
typedef struct jyppx_ocv_background_subtractor_mog2 jyppx_ocv_background_subtractor_mog2;
typedef struct jyppx_ocv_background_subtractor_knn jyppx_ocv_background_subtractor_knn;
typedef struct jyppx_ocv_dense_optical_flow jyppx_ocv_dense_optical_flow;
typedef struct jyppx_ocv_sparse_optical_flow jyppx_ocv_sparse_optical_flow;
typedef struct jyppx_ocv_video_tracker jyppx_ocv_video_tracker;

typedef struct jyppx_ocv_video_point2f
{
    float x;
    float y;
} jyppx_ocv_video_point2f;

typedef struct jyppx_ocv_video_rect
{
    int x;
    int y;
    int width;
    int height;
} jyppx_ocv_video_rect;

typedef struct jyppx_ocv_video_rotated_rect
{
    float center_x;
    float center_y;
    float width;
    float height;
    float angle;
} jyppx_ocv_video_rotated_rect;

typedef struct jyppx_ocv_video_tracker_mil_params
{
    float sampler_init_in_radius;
    int sampler_init_max_neg_num;
    float sampler_search_win_size;
    float sampler_track_in_radius;
    int sampler_track_max_pos_num;
    int sampler_track_max_neg_num;
    int feature_set_num_features;
} jyppx_ocv_video_tracker_mil_params;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_calc_optical_flow_pyr_lk(
    const jyppx_ocv_mat* prev_img,
    const jyppx_ocv_mat* next_img,
    const jyppx_ocv_video_point2f* prev_points,
    int point_count,
    const jyppx_ocv_video_point2f* initial_next_points,
    int use_initial_flow,
    jyppx_ocv_video_point2f* next_points,
    unsigned char* status,
    float* err,
    int win_width,
    int win_height,
    int max_level,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int flags,
    double min_eig_threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_calc_optical_flow_farneback(
    const jyppx_ocv_mat* prev,
    const jyppx_ocv_mat* next,
    jyppx_ocv_mat* flow,
    double pyr_scale,
    int levels,
    int winsize,
    int iterations,
    int poly_n,
    double poly_sigma,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_read_optical_flow(
    const char* path,
    jyppx_ocv_mat** flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_write_optical_flow(
    const char* path,
    const jyppx_ocv_mat* flow,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_build_optical_flow_pyramid_count(
    const jyppx_ocv_mat* image,
    int win_width,
    int win_height,
    int max_level,
    int with_derivatives,
    int pyr_border,
    int deriv_border,
    int try_reuse_input_image,
    int* level_count,
    int* mat_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_build_optical_flow_pyramid_fill(
    const jyppx_ocv_mat* image,
    int win_width,
    int win_height,
    int max_level,
    int with_derivatives,
    int pyr_border,
    int deriv_border,
    int try_reuse_input_image,
    jyppx_ocv_mat** pyramid,
    int pyramid_capacity,
    int* level_count,
    int* mat_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_mean_shift(
    const jyppx_ocv_mat* prob_image,
    jyppx_ocv_video_rect* window,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int* iterations);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_cam_shift(
    const jyppx_ocv_mat* prob_image,
    jyppx_ocv_video_rect* window,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    jyppx_ocv_video_rotated_rect* box);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_kalman_filter_create(
    int dynam_params,
    int measure_params,
    int control_params,
    int type,
    jyppx_ocv_kalman_filter** filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_kalman_filter_release_handle(
    jyppx_ocv_kalman_filter* filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_kalman_filter_init(
    jyppx_ocv_kalman_filter* filter,
    int dynam_params,
    int measure_params,
    int control_params,
    int type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_kalman_filter_predict(
    jyppx_ocv_kalman_filter* filter,
    const jyppx_ocv_mat* control,
    jyppx_ocv_mat* prediction);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_kalman_filter_correct(
    jyppx_ocv_kalman_filter* filter,
    const jyppx_ocv_mat* measurement,
    jyppx_ocv_mat* corrected);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_kalman_filter_get_matrix(
    const jyppx_ocv_kalman_filter* filter,
    int matrix_id,
    jyppx_ocv_mat* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_kalman_filter_set_matrix(
    jyppx_ocv_kalman_filter* filter,
    int matrix_id,
    const jyppx_ocv_mat* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_background_subtractor_release_handle(
    jyppx_ocv_background_subtractor* subtractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_apply(
    jyppx_ocv_background_subtractor* subtractor,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* fgmask,
    double learning_rate);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_apply_with_known_foreground(
    jyppx_ocv_background_subtractor* subtractor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* known_foreground_mask,
    jyppx_ocv_mat* fgmask,
    double learning_rate);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_get_background_image(
    const jyppx_ocv_background_subtractor* subtractor,
    jyppx_ocv_mat* background_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_create(
    int history,
    double var_threshold,
    int detect_shadows,
    jyppx_ocv_background_subtractor_mog2** subtractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_get_history(
    const jyppx_ocv_background_subtractor_mog2* subtractor,
    int* history);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_set_history(
    jyppx_ocv_background_subtractor_mog2* subtractor,
    int history);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_get_n_mixtures(
    const jyppx_ocv_background_subtractor_mog2* subtractor,
    int* n_mixtures);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_set_n_mixtures(
    jyppx_ocv_background_subtractor_mog2* subtractor,
    int n_mixtures);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_get_detect_shadows(
    const jyppx_ocv_background_subtractor_mog2* subtractor,
    int* detect_shadows);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_set_detect_shadows(
    jyppx_ocv_background_subtractor_mog2* subtractor,
    int detect_shadows);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_get_int_property(
    const jyppx_ocv_background_subtractor_mog2* subtractor,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_set_int_property(
    jyppx_ocv_background_subtractor_mog2* subtractor,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_get_double_property(
    const jyppx_ocv_background_subtractor_mog2* subtractor,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_mog2_set_double_property(
    jyppx_ocv_background_subtractor_mog2* subtractor,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_create(
    int history,
    double dist2_threshold,
    int detect_shadows,
    jyppx_ocv_background_subtractor_knn** subtractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_get_history(
    const jyppx_ocv_background_subtractor_knn* subtractor,
    int* history);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_set_history(
    jyppx_ocv_background_subtractor_knn* subtractor,
    int history);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_get_n_samples(
    const jyppx_ocv_background_subtractor_knn* subtractor,
    int* n_samples);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_set_n_samples(
    jyppx_ocv_background_subtractor_knn* subtractor,
    int n_samples);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_get_detect_shadows(
    const jyppx_ocv_background_subtractor_knn* subtractor,
    int* detect_shadows);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_set_detect_shadows(
    jyppx_ocv_background_subtractor_knn* subtractor,
    int detect_shadows);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_get_int_property(
    const jyppx_ocv_background_subtractor_knn* subtractor,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_set_int_property(
    jyppx_ocv_background_subtractor_knn* subtractor,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_get_double_property(
    const jyppx_ocv_background_subtractor_knn* subtractor,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_background_subtractor_knn_set_double_property(
    jyppx_ocv_background_subtractor_knn* subtractor,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_dense_optical_flow_release_handle(
    jyppx_ocv_dense_optical_flow* optical_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dense_optical_flow_calc(
    jyppx_ocv_dense_optical_flow* optical_flow,
    const jyppx_ocv_mat* first,
    const jyppx_ocv_mat* second,
    jyppx_ocv_mat* flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dense_optical_flow_collect_garbage(
    jyppx_ocv_dense_optical_flow* optical_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_sparse_optical_flow_release_handle(
    jyppx_ocv_sparse_optical_flow* optical_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_sparse_optical_flow_calc(
    jyppx_ocv_sparse_optical_flow* optical_flow,
    const jyppx_ocv_mat* previous_image,
    const jyppx_ocv_mat* next_image,
    const jyppx_ocv_video_point2f* previous_points,
    int point_count,
    jyppx_ocv_video_point2f* next_points,
    unsigned char* status,
    float* error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_farneback_optical_flow_create(
    int num_levels,
    double pyramid_scale,
    int fast_pyramids,
    int window_size,
    int num_iterations,
    int polynomial_neighborhood,
    double polynomial_sigma,
    int flags,
    jyppx_ocv_dense_optical_flow** optical_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_farneback_optical_flow_get_int_property(
    const jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_farneback_optical_flow_set_int_property(
    jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_farneback_optical_flow_get_double_property(
    const jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_farneback_optical_flow_set_double_property(
    jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_farneback_optical_flow_get_bool_property(
    const jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_farneback_optical_flow_set_bool_property(
    jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_variational_refinement_create(
    jyppx_ocv_dense_optical_flow** optical_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_variational_refinement_calc_uv(
    jyppx_ocv_dense_optical_flow* optical_flow,
    const jyppx_ocv_mat* first,
    const jyppx_ocv_mat* second,
    jyppx_ocv_mat* flow_u,
    jyppx_ocv_mat* flow_v);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_variational_refinement_get_int_property(
    const jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_variational_refinement_set_int_property(
    jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_variational_refinement_get_float_property(
    const jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_variational_refinement_set_float_property(
    jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dis_optical_flow_create(
    int preset,
    jyppx_ocv_dense_optical_flow** optical_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dis_optical_flow_get_int_property(
    const jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dis_optical_flow_set_int_property(
    jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dis_optical_flow_get_float_property(
    const jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dis_optical_flow_set_float_property(
    jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dis_optical_flow_get_bool_property(
    const jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dis_optical_flow_set_bool_property(
    jyppx_ocv_dense_optical_flow* optical_flow,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_sparse_pyr_lk_optical_flow_create(
    int window_width,
    int window_height,
    int max_level,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int flags,
    double min_eigenvalue_threshold,
    jyppx_ocv_sparse_optical_flow** optical_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property(
    const jyppx_ocv_sparse_optical_flow* optical_flow,
    int* width,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_sparse_pyr_lk_optical_flow_set_size_property(
    jyppx_ocv_sparse_optical_flow* optical_flow,
    int width,
    int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_sparse_pyr_lk_optical_flow_get_int_property(
    const jyppx_ocv_sparse_optical_flow* optical_flow,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_sparse_pyr_lk_optical_flow_set_int_property(
    jyppx_ocv_sparse_optical_flow* optical_flow,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria(
    const jyppx_ocv_sparse_optical_flow* optical_flow,
    int* type,
    int* max_count,
    double* epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_sparse_pyr_lk_optical_flow_set_term_criteria(
    jyppx_ocv_sparse_optical_flow* optical_flow,
    int type,
    int max_count,
    double epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_sparse_pyr_lk_optical_flow_get_min_eig_threshold(
    const jyppx_ocv_sparse_optical_flow* optical_flow,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_sparse_pyr_lk_optical_flow_set_min_eig_threshold(
    jyppx_ocv_sparse_optical_flow* optical_flow,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_compute_ecc(
    const jyppx_ocv_mat* template_image,
    const jyppx_ocv_mat* input_image,
    const jyppx_ocv_mat* input_mask,
    double* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_find_transform_ecc(
    const jyppx_ocv_mat* template_image,
    const jyppx_ocv_mat* input_image,
    jyppx_ocv_mat* warp_matrix,
    int motion_type,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    const jyppx_ocv_mat* input_mask,
    int gaussian_filter_size,
    double* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_find_transform_ecc_with_mask(
    const jyppx_ocv_mat* template_image,
    const jyppx_ocv_mat* input_image,
    const jyppx_ocv_mat* template_mask,
    const jyppx_ocv_mat* input_mask,
    jyppx_ocv_mat* warp_matrix,
    int motion_type,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int gaussian_filter_size,
    double* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_ecc_parameters_get_default(
    int* motion_type,
    int* criteria_type,
    int* criteria_max_count,
    double* criteria_epsilon,
    int* gaussian_filter_size,
    int* level_count,
    int* interpolation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_find_transform_ecc_multi_scale(
    const jyppx_ocv_mat* reference_image,
    const jyppx_ocv_mat* sample_image,
    jyppx_ocv_mat* warp_matrix,
    int motion_type,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    const int* iterations_per_level,
    int iteration_count,
    int gaussian_filter_size,
    int level_count,
    int interpolation,
    const jyppx_ocv_mat* reference_mask,
    const jyppx_ocv_mat* sample_mask,
    double* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_video_tracker_release_handle(
    jyppx_ocv_video_tracker* tracker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_tracker_init(
    jyppx_ocv_video_tracker* tracker,
    const jyppx_ocv_mat* image,
    jyppx_ocv_video_rect bounding_box);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_tracker_update(
    jyppx_ocv_video_tracker* tracker,
    const jyppx_ocv_mat* image,
    jyppx_ocv_video_rect* bounding_box,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_tracker_get_tracking_score(
    const jyppx_ocv_video_tracker* tracker,
    float* score);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_tracker_mil_get_default_params(
    jyppx_ocv_video_tracker_mil_params* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_tracker_mil_create(
    const jyppx_ocv_video_tracker_mil_params* parameters,
    jyppx_ocv_video_tracker** tracker);
