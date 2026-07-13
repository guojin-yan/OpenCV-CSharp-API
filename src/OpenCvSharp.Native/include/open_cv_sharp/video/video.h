#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_kalman_filter jyppx_ocv_kalman_filter;
typedef struct jyppx_ocv_background_subtractor jyppx_ocv_background_subtractor;
typedef struct jyppx_ocv_background_subtractor_mog2 jyppx_ocv_background_subtractor_mog2;
typedef struct jyppx_ocv_background_subtractor_knn jyppx_ocv_background_subtractor_knn;

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
