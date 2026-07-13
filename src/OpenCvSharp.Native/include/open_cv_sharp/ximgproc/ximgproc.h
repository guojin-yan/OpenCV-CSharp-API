#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_stereo_bm jyppx_ocv_stereo_bm;
typedef struct jyppx_ocv_stereo_sgbm jyppx_ocv_stereo_sgbm;
typedef struct jyppx_ocv_stereo_matcher jyppx_ocv_stereo_matcher;
typedef struct jyppx_ocv_ximgproc_guided_filter jyppx_ocv_ximgproc_guided_filter;
typedef struct jyppx_ocv_ximgproc_fast_global_smoother_filter jyppx_ocv_ximgproc_fast_global_smoother_filter;
typedef struct jyppx_ocv_ximgproc_superpixel_slic jyppx_ocv_ximgproc_superpixel_slic;
typedef struct jyppx_ocv_ximgproc_superpixel_seeds jyppx_ocv_ximgproc_superpixel_seeds;
typedef struct jyppx_ocv_ximgproc_superpixel_lsc jyppx_ocv_ximgproc_superpixel_lsc;
typedef struct jyppx_ocv_ximgproc_fast_line_detector jyppx_ocv_ximgproc_fast_line_detector;
typedef struct jyppx_ocv_ximgproc_disparity_filter jyppx_ocv_ximgproc_disparity_filter;
typedef struct jyppx_ocv_ximgproc_disparity_wls_filter jyppx_ocv_ximgproc_disparity_wls_filter;
typedef struct jyppx_ocv_ximgproc_fast_bilateral_solver_filter jyppx_ocv_ximgproc_fast_bilateral_solver_filter;
typedef struct jyppx_ocv_ximgproc_sparse_match_interpolator jyppx_ocv_ximgproc_sparse_match_interpolator;
typedef struct jyppx_ocv_ximgproc_edge_aware_interpolator jyppx_ocv_ximgproc_edge_aware_interpolator;
typedef struct jyppx_ocv_ximgproc_ric_interpolator jyppx_ocv_ximgproc_ric_interpolator;
typedef struct jyppx_ocv_ximgproc_edge_drawing jyppx_ocv_ximgproc_edge_drawing;
typedef struct jyppx_ocv_ximgproc_edge_boxes jyppx_ocv_ximgproc_edge_boxes;
typedef struct jyppx_ocv_ximgproc_ridge_detection_filter jyppx_ocv_ximgproc_ridge_detection_filter;
typedef struct jyppx_ocv_ximgproc_contour_fitting jyppx_ocv_ximgproc_contour_fitting;
typedef struct jyppx_ocv_ximgproc_scan_segment jyppx_ocv_ximgproc_scan_segment;
typedef struct jyppx_ocv_ximgproc_graph_segmentation jyppx_ocv_ximgproc_graph_segmentation;
typedef struct jyppx_ocv_ximgproc_selective_search_strategy jyppx_ocv_ximgproc_selective_search_strategy;
typedef struct jyppx_ocv_ximgproc_selective_search_segmentation jyppx_ocv_ximgproc_selective_search_segmentation;

typedef struct jyppx_ocv_ximgproc_rect
{
    int x;
    int y;
    int width;
    int height;
} jyppx_ocv_ximgproc_rect;

typedef struct jyppx_ocv_ximgproc_point
{
    int x;
    int y;
} jyppx_ocv_ximgproc_point;

typedef struct jyppx_ocv_ximgproc_point3i
{
    int x;
    int y;
    int z;
} jyppx_ocv_ximgproc_point3i;

typedef struct jyppx_ocv_ximgproc_edge_box
{
    int x;
    int y;
    int width;
    int height;
    float score;
} jyppx_ocv_ximgproc_edge_box;

typedef struct jyppx_ocv_ximgproc_edge_drawing_params
{
    int pf_mode;
    int edge_detection_operator;
    int gradient_threshold_value;
    int anchor_threshold_value;
    int scan_interval;
    int min_path_length;
    float sigma;
    int sum_flag;
    int nfa_validation;
    int min_line_length;
    double max_distance_between_two_lines;
    double line_fit_error_threshold;
    double max_error_threshold;
} jyppx_ocv_ximgproc_edge_drawing_params;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ni_black_threshold(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double max_value,
    int type,
    int block_size,
    double k,
    int binarization_method,
    double r);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_thinning(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int thinning_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_anisotropic_diffusion(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float alpha,
    float k,
    int niters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_joint_bilateral_filter(
    const jyppx_ocv_mat* joint,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int d,
    double sigma_color,
    double sigma_space,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_guided_filter_run(
    const jyppx_ocv_mat* guide,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int radius,
    double eps,
    int ddepth,
    double scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_rolling_guidance_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int d,
    double sigma_color,
    double sigma_space,
    int num_of_iter,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_weighted_median_filter(
    const jyppx_ocv_mat* joint,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int r,
    double sigma,
    int weight_type,
    const jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_dt_filter(
    const jyppx_ocv_mat* guide,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double sigma_spatial,
    double sigma_color,
    int mode,
    int num_iters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_am_filter(
    const jyppx_ocv_mat* joint,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double sigma_s,
    double sigma_r,
    int adjust_outliers);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_bilateral_texture_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int fr,
    int num_iter,
    double sigma_alpha,
    double sigma_avg);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_preserving_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int d,
    double threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_global_smoother_filter_run(
    const jyppx_ocv_mat* guide,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double lambda,
    double sigma_color,
    double lambda_attenuation,
    int num_iter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_l0_smooth(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double lambda,
    double kappa);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_hough_transform(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int dst_mat_depth,
    int angle_range,
    int op,
    int make_skew);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_hough_point_to_line(
    int hough_x,
    int hough_y,
    const jyppx_ocv_mat* src_img_info,
    int angle_range,
    int make_skew,
    int rules,
    int* x1,
    int* y1,
    int* x2,
    int* y2);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_pei_lin_normalization(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_guided_filter_create(
    const jyppx_ocv_mat* guide,
    int radius,
    double eps,
    double scale,
    jyppx_ocv_ximgproc_guided_filter** filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_guided_filter_release_handle(
    jyppx_ocv_ximgproc_guided_filter* filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_guided_filter_filter(
    jyppx_ocv_ximgproc_guided_filter* filter,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_global_smoother_filter_create(
    const jyppx_ocv_mat* guide,
    double lambda,
    double sigma_color,
    double lambda_attenuation,
    int num_iter,
    jyppx_ocv_ximgproc_fast_global_smoother_filter** filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_fast_global_smoother_filter_release_handle(
    jyppx_ocv_ximgproc_fast_global_smoother_filter* filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_global_smoother_filter_filter(
    jyppx_ocv_ximgproc_fast_global_smoother_filter* filter,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_slic_create(
    const jyppx_ocv_mat* image,
    int algorithm,
    int region_size,
    float ruler,
    jyppx_ocv_ximgproc_superpixel_slic** superpixel);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_superpixel_slic_release_handle(
    jyppx_ocv_ximgproc_superpixel_slic* superpixel);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_slic_get_number(
    const jyppx_ocv_ximgproc_superpixel_slic* superpixel,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_slic_iterate(
    jyppx_ocv_ximgproc_superpixel_slic* superpixel,
    int num_iterations);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_slic_get_labels(
    const jyppx_ocv_ximgproc_superpixel_slic* superpixel,
    jyppx_ocv_mat* labels);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_slic_get_label_contour_mask(
    const jyppx_ocv_ximgproc_superpixel_slic* superpixel,
    jyppx_ocv_mat* image,
    int thick_line);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_slic_enforce_label_connectivity(
    jyppx_ocv_ximgproc_superpixel_slic* superpixel,
    int min_element_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_seeds_create(
    int image_width,
    int image_height,
    int image_channels,
    int num_superpixels,
    int num_levels,
    int prior,
    int histogram_bins,
    int double_step,
    jyppx_ocv_ximgproc_superpixel_seeds** superpixel);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_superpixel_seeds_release_handle(
    jyppx_ocv_ximgproc_superpixel_seeds* superpixel);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_seeds_get_number(
    jyppx_ocv_ximgproc_superpixel_seeds* superpixel,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_seeds_iterate(
    jyppx_ocv_ximgproc_superpixel_seeds* superpixel,
    const jyppx_ocv_mat* image,
    int num_iterations);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_seeds_get_labels(
    jyppx_ocv_ximgproc_superpixel_seeds* superpixel,
    jyppx_ocv_mat* labels);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_seeds_get_label_contour_mask(
    jyppx_ocv_ximgproc_superpixel_seeds* superpixel,
    jyppx_ocv_mat* image,
    int thick_line);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_lsc_create(
    const jyppx_ocv_mat* image,
    int region_size,
    float ratio,
    jyppx_ocv_ximgproc_superpixel_lsc** superpixel);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_superpixel_lsc_release_handle(
    jyppx_ocv_ximgproc_superpixel_lsc* superpixel);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_lsc_get_number(
    const jyppx_ocv_ximgproc_superpixel_lsc* superpixel,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_lsc_iterate(
    jyppx_ocv_ximgproc_superpixel_lsc* superpixel,
    int num_iterations);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_lsc_get_labels(
    const jyppx_ocv_ximgproc_superpixel_lsc* superpixel,
    jyppx_ocv_mat* labels);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_lsc_get_label_contour_mask(
    const jyppx_ocv_ximgproc_superpixel_lsc* superpixel,
    jyppx_ocv_mat* image,
    int thick_line);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_superpixel_lsc_enforce_label_connectivity(
    jyppx_ocv_ximgproc_superpixel_lsc* superpixel,
    int min_element_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_line_detector_create(
    int length_threshold,
    float distance_threshold,
    double canny_th1,
    double canny_th2,
    int canny_aperture_size,
    int do_merge,
    jyppx_ocv_ximgproc_fast_line_detector** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_fast_line_detector_release_handle(
    jyppx_ocv_ximgproc_fast_line_detector* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_line_detector_detect(
    jyppx_ocv_ximgproc_fast_line_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* lines);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_line_detector_detect_count(
    jyppx_ocv_ximgproc_fast_line_detector* detector,
    const jyppx_ocv_mat* image,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_line_detector_detect_fill(
    jyppx_ocv_ximgproc_fast_line_detector* detector,
    const jyppx_ocv_mat* image,
    float* lines,
    int line_capacity,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_line_detector_draw_segments(
    jyppx_ocv_ximgproc_fast_line_detector* detector,
    jyppx_ocv_mat* image,
    const jyppx_ocv_mat* lines,
    int draw_arrow,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int line_thickness);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_line_detector_draw_segments_array(
    jyppx_ocv_ximgproc_fast_line_detector* detector,
    jyppx_ocv_mat* image,
    const float* lines,
    int line_count,
    int draw_arrow,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int line_thickness);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_create_generic(
    int use_confidence,
    jyppx_ocv_ximgproc_disparity_wls_filter** filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_bm(
    const jyppx_ocv_stereo_bm* matcher_left,
    jyppx_ocv_ximgproc_disparity_wls_filter** filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_sgbm(
    const jyppx_ocv_stereo_sgbm* matcher_left,
    jyppx_ocv_ximgproc_disparity_wls_filter** filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_matcher(
    const jyppx_ocv_stereo_matcher* matcher_left,
    jyppx_ocv_ximgproc_disparity_wls_filter** filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_create_right_matcher_from_stereo_bm(
    const jyppx_ocv_stereo_bm* matcher_left,
    jyppx_ocv_stereo_matcher** matcher_right);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_create_right_matcher_from_stereo_sgbm(
    const jyppx_ocv_stereo_sgbm* matcher_left,
    jyppx_ocv_stereo_matcher** matcher_right);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_create_right_matcher_from_stereo_matcher(
    const jyppx_ocv_stereo_matcher* matcher_left,
    jyppx_ocv_stereo_matcher** matcher_right);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_disparity_wls_filter_release_handle(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_filter_filter(
    jyppx_ocv_ximgproc_disparity_filter* filter,
    const jyppx_ocv_mat* disparity_map_left,
    const jyppx_ocv_mat* left_view,
    jyppx_ocv_mat* filtered_disparity_map,
    const jyppx_ocv_mat* disparity_map_right,
    const jyppx_ocv_ximgproc_rect* roi,
    const jyppx_ocv_mat* right_view);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_filter(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    const jyppx_ocv_mat* disparity_map_left,
    const jyppx_ocv_mat* left_view,
    jyppx_ocv_mat* filtered_disparity_map,
    const jyppx_ocv_mat* disparity_map_right,
    const jyppx_ocv_ximgproc_rect* roi,
    const jyppx_ocv_mat* right_view);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_get_lambda(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_set_lambda(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_get_sigma_color(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_set_sigma_color(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_get_lrc_thresh(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_set_lrc_thresh(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_get_depth_discontinuity_radius(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_set_depth_discontinuity_radius(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_get_confidence_map(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    jyppx_ocv_mat* confidence_map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_disparity_wls_filter_get_roi(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    jyppx_ocv_ximgproc_rect* roi);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_get_disparity_vis(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_compute_mse(
    const jyppx_ocv_mat* gt,
    const jyppx_ocv_mat* src,
    const jyppx_ocv_ximgproc_rect* roi,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_compute_bad_pixel_percent(
    const jyppx_ocv_mat* gt,
    const jyppx_ocv_mat* src,
    const jyppx_ocv_ximgproc_rect* roi,
    int thresh,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_bilateral_solver_filter_create(
    const jyppx_ocv_mat* guide,
    double sigma_spatial,
    double sigma_luma,
    double sigma_chroma,
    double lambda,
    int num_iter,
    double max_tol,
    jyppx_ocv_ximgproc_fast_bilateral_solver_filter** filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_fast_bilateral_solver_filter_release_handle(
    jyppx_ocv_ximgproc_fast_bilateral_solver_filter* filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_bilateral_solver_filter_filter(
    jyppx_ocv_ximgproc_fast_bilateral_solver_filter* filter,
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* confidence,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fast_bilateral_solver_filter_run(
    const jyppx_ocv_mat* guide,
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* confidence,
    jyppx_ocv_mat* dst,
    double sigma_spatial,
    double sigma_luma,
    double sigma_chroma,
    double lambda,
    int num_iter,
    double max_tol);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_create(
    jyppx_ocv_ximgproc_edge_aware_interpolator** interpolator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_edge_aware_interpolator_release_handle(
    jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_create(
    jyppx_ocv_ximgproc_ric_interpolator** interpolator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_ric_interpolator_release_handle(
    jyppx_ocv_ximgproc_ric_interpolator* interpolator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_sparse_match_interpolator_interpolate(
    jyppx_ocv_ximgproc_sparse_match_interpolator* interpolator,
    const jyppx_ocv_mat* from_image,
    const jyppx_ocv_mat* from_points,
    const jyppx_ocv_mat* to_image,
    const jyppx_ocv_mat* to_points,
    jyppx_ocv_mat* dense_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_interpolate(
    jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator,
    const jyppx_ocv_mat* from_image,
    const jyppx_ocv_mat* from_points,
    const jyppx_ocv_mat* to_image,
    const jyppx_ocv_mat* to_points,
    jyppx_ocv_mat* dense_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_interpolate(
    jyppx_ocv_ximgproc_ric_interpolator* interpolator,
    const jyppx_ocv_mat* from_image,
    const jyppx_ocv_mat* from_points,
    const jyppx_ocv_mat* to_image,
    const jyppx_ocv_mat* to_points,
    jyppx_ocv_mat* dense_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_set_cost_map(
    jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator,
    const jyppx_ocv_mat* cost_map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_cost_map(
    jyppx_ocv_ximgproc_ric_interpolator* interpolator,
    const jyppx_ocv_mat* cost_map);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_get_k(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_set_k(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_get_sigma(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_set_sigma(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_get_lambda(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_set_lambda(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_get_use_post_processing(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_set_use_post_processing(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_get_fgs_lambda(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_set_fgs_lambda(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_get_fgs_sigma(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_aware_interpolator_set_fgs_sigma(jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator, float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_k(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_k(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_size(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_size(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_nn_count(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_nn_count(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_ruler(jyppx_ocv_ximgproc_ric_interpolator* interpolator, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_ruler(jyppx_ocv_ximgproc_ric_interpolator* interpolator, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_mode(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_mode(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_alpha(jyppx_ocv_ximgproc_ric_interpolator* interpolator, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_alpha(jyppx_ocv_ximgproc_ric_interpolator* interpolator, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_model_iter(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_model_iter(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_refine_models(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_refine_models(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_max_flow(jyppx_ocv_ximgproc_ric_interpolator* interpolator, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_max_flow(jyppx_ocv_ximgproc_ric_interpolator* interpolator, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_use_variational_refinement(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_use_variational_refinement(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_use_global_smoother_filter(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_use_global_smoother_filter(jyppx_ocv_ximgproc_ric_interpolator* interpolator, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_fgs_lambda(jyppx_ocv_ximgproc_ric_interpolator* interpolator, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_fgs_lambda(jyppx_ocv_ximgproc_ric_interpolator* interpolator, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_get_fgs_sigma(jyppx_ocv_ximgproc_ric_interpolator* interpolator, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ric_interpolator_set_fgs_sigma(jyppx_ocv_ximgproc_ric_interpolator* interpolator, float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_create(
    jyppx_ocv_ximgproc_edge_drawing** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_edge_drawing_release_handle(
    jyppx_ocv_ximgproc_edge_drawing* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_get_params(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    jyppx_ocv_ximgproc_edge_drawing_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_set_params(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    const jyppx_ocv_ximgproc_edge_drawing_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_detect_edges(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    const jyppx_ocv_mat* src);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_get_edge_image(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_get_gradient_image(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_get_segments_count(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* group_count,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_get_segments_fill(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_ximgproc_point* points,
    int point_capacity,
    int* group_count,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_detect_lines(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    jyppx_ocv_mat* lines);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_detect_lines_count(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_detect_lines_fill(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    float* lines,
    int line_capacity,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_count(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* index_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_fill(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* indices,
    int index_capacity,
    int* index_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_detect_ellipses(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    jyppx_ocv_mat* ellipses);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_detect_ellipses_count(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* ellipse_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_drawing_detect_ellipses_fill(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    double* ellipses,
    int ellipse_capacity,
    int* ellipse_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_create(
    float alpha,
    float beta,
    float eta,
    float min_score,
    int max_boxes,
    float edge_min_mag,
    float edge_merge_thr,
    float cluster_min_mag,
    float max_aspect_ratio,
    float min_box_area,
    float gamma,
    float kappa,
    jyppx_ocv_ximgproc_edge_boxes** edge_boxes);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_edge_boxes_release_handle(
    jyppx_ocv_ximgproc_edge_boxes* edge_boxes);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_count(
    jyppx_ocv_ximgproc_edge_boxes* edge_boxes,
    const jyppx_ocv_mat* edge_map,
    const jyppx_ocv_mat* orientation_map,
    int* box_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_fill(
    jyppx_ocv_ximgproc_edge_boxes* edge_boxes,
    const jyppx_ocv_mat* edge_map,
    const jyppx_ocv_mat* orientation_map,
    jyppx_ocv_ximgproc_edge_box* boxes,
    int box_capacity,
    int* box_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_alpha(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_alpha(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_beta(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_beta(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_eta(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_eta(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_min_score(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_min_score(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_max_boxes(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_max_boxes(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_edge_min_mag(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_edge_min_mag(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_edge_merge_thr(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_edge_merge_thr(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_cluster_min_mag(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_cluster_min_mag(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_max_aspect_ratio(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_max_aspect_ratio(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_min_box_area(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_min_box_area(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_gamma(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_gamma(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_get_kappa(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_edge_boxes_set_kappa(jyppx_ocv_ximgproc_edge_boxes* edge_boxes, float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_gradient_deriche_x(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double alpha,
    double omega);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_gradient_deriche_y(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double alpha,
    double omega);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_gradient_paillou_x(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double alpha,
    double omega);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_gradient_paillou_y(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double alpha,
    double omega);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ridge_detection_filter_create(
    int ddepth,
    int dx,
    int dy,
    int ksize,
    int out_dtype,
    double scale,
    double delta,
    int border_type,
    jyppx_ocv_ximgproc_ridge_detection_filter** filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_ridge_detection_filter_release_handle(
    jyppx_ocv_ximgproc_ridge_detection_filter* filter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_ridge_detection_filter_get_image(
    jyppx_ocv_ximgproc_ridge_detection_filter* filter,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_fourier_descriptor(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int nb_elt,
    int nb_fd);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_transform_fd(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* transform,
    jyppx_ocv_mat* dst,
    int fd_contour);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_contour_sampling(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int nb_elt);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_contour_fitting_create(
    int ctr,
    int fd,
    jyppx_ocv_ximgproc_contour_fitting** fitting);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_contour_fitting_release_handle(
    jyppx_ocv_ximgproc_contour_fitting* fitting);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_contour_fitting_estimate_transformation(
    jyppx_ocv_ximgproc_contour_fitting* fitting,
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* dst,
    jyppx_ocv_mat* alpha_phi_st,
    double* distance,
    int fd_contour);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_contour_fitting_get_ctr_size(
    jyppx_ocv_ximgproc_contour_fitting* fitting,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_contour_fitting_set_ctr_size(
    jyppx_ocv_ximgproc_contour_fitting* fitting,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_contour_fitting_get_fd_size(
    jyppx_ocv_ximgproc_contour_fitting* fitting,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_contour_fitting_set_fd_size(
    jyppx_ocv_ximgproc_contour_fitting* fitting,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_rl_threshold(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* rl_dst,
    double thresh,
    int type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_rl_dilate(
    const jyppx_ocv_mat* rl_src,
    jyppx_ocv_mat* rl_dst,
    const jyppx_ocv_mat* rl_kernel,
    int anchor_x,
    int anchor_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_rl_erode(
    const jyppx_ocv_mat* rl_src,
    jyppx_ocv_mat* rl_dst,
    const jyppx_ocv_mat* rl_kernel,
    int boundary_on,
    int anchor_x,
    int anchor_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_rl_get_structuring_element(
    int shape,
    int width,
    int height,
    jyppx_ocv_mat* rl_kernel);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_rl_paint(
    jyppx_ocv_mat* image,
    const jyppx_ocv_mat* rl_src,
    double value_v0,
    double value_v1,
    double value_v2,
    double value_v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_rl_is_morphology_possible(
    const jyppx_ocv_mat* rl_structuring_element,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_rl_create_rle_image(
    const jyppx_ocv_ximgproc_point3i* runs,
    int run_count,
    int width,
    int height,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_rl_morphology_ex(
    const jyppx_ocv_mat* rl_src,
    jyppx_ocv_mat* rl_dst,
    int op,
    const jyppx_ocv_mat* rl_kernel,
    int boundary_on_for_erosion,
    int anchor_x,
    int anchor_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_scan_segment_create(
    int image_width,
    int image_height,
    int num_superpixels,
    int slices,
    int merge_small,
    jyppx_ocv_ximgproc_scan_segment** segment);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_scan_segment_release_handle(
    jyppx_ocv_ximgproc_scan_segment* segment);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_scan_segment_get_number(
    jyppx_ocv_ximgproc_scan_segment* segment,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_scan_segment_iterate(
    jyppx_ocv_ximgproc_scan_segment* segment,
    const jyppx_ocv_mat* image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_scan_segment_get_labels(
    jyppx_ocv_ximgproc_scan_segment* segment,
    jyppx_ocv_mat* labels);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_scan_segment_get_label_contour_mask(
    jyppx_ocv_ximgproc_scan_segment* segment,
    jyppx_ocv_mat* image,
    int thick_line);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_graph_segmentation_create(
    double sigma,
    float k,
    int min_size,
    jyppx_ocv_ximgproc_graph_segmentation** segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_graph_segmentation_release_handle(
    jyppx_ocv_ximgproc_graph_segmentation* segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_graph_segmentation_process_image(
    jyppx_ocv_ximgproc_graph_segmentation* segmentation,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_graph_segmentation_get_sigma(
    jyppx_ocv_ximgproc_graph_segmentation* segmentation,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_graph_segmentation_set_sigma(
    jyppx_ocv_ximgproc_graph_segmentation* segmentation,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_graph_segmentation_get_k(
    jyppx_ocv_ximgproc_graph_segmentation* segmentation,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_graph_segmentation_set_k(
    jyppx_ocv_ximgproc_graph_segmentation* segmentation,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_graph_segmentation_get_min_size(
    jyppx_ocv_ximgproc_graph_segmentation* segmentation,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_graph_segmentation_set_min_size(
    jyppx_ocv_ximgproc_graph_segmentation* segmentation,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_strategy_create_color(
    jyppx_ocv_ximgproc_selective_search_strategy** strategy);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_strategy_create_size(
    jyppx_ocv_ximgproc_selective_search_strategy** strategy);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_strategy_create_texture(
    jyppx_ocv_ximgproc_selective_search_strategy** strategy);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_strategy_create_fill(
    jyppx_ocv_ximgproc_selective_search_strategy** strategy);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_strategy_create_multiple(
    jyppx_ocv_ximgproc_selective_search_strategy** strategy);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_selective_search_strategy_release_handle(
    jyppx_ocv_ximgproc_selective_search_strategy* strategy);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_strategy_set_image(
    jyppx_ocv_ximgproc_selective_search_strategy* strategy,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* regions,
    const jyppx_ocv_mat* sizes,
    int image_id);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_strategy_get(
    jyppx_ocv_ximgproc_selective_search_strategy* strategy,
    int r1,
    int r2,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_strategy_merge(
    jyppx_ocv_ximgproc_selective_search_strategy* strategy,
    int r1,
    int r2);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_strategy_multiple_add(
    jyppx_ocv_ximgproc_selective_search_strategy* multiple,
    jyppx_ocv_ximgproc_selective_search_strategy* strategy,
    float weight);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_strategy_multiple_clear(
    jyppx_ocv_ximgproc_selective_search_strategy* multiple);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_create(
    jyppx_ocv_ximgproc_selective_search_segmentation** segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_ximgproc_selective_search_segmentation_release_handle(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_set_base_image(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    const jyppx_ocv_mat* image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_single_strategy(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    int k,
    float sigma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_fast(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    int base_k,
    int inc_k,
    float sigma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_quality(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    int base_k,
    int inc_k,
    float sigma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_add_image(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    const jyppx_ocv_mat* image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_clear_images(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_add_graph_segmentation(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    jyppx_ocv_ximgproc_graph_segmentation* graph_segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_clear_graph_segmentations(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_add_strategy(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    jyppx_ocv_ximgproc_selective_search_strategy* strategy);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_clear_strategies(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_process_count(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    int* rect_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_selective_search_segmentation_process_fill(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    jyppx_ocv_ximgproc_rect* rects,
    int rect_capacity,
    int* rect_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_ximgproc_covariance_estimation(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int window_rows,
    int window_cols);
