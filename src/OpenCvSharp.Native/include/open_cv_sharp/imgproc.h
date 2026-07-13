#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_clahe jyppx_ocv_clahe;
typedef struct jyppx_ocv_line_segment_detector jyppx_ocv_line_segment_detector;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_cvt_color(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int code,
    int dst_cn);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_resize(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    double fx,
    double fy,
    int interpolation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_threshold(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double thresh,
    double maxval,
    int type,
    double* out_threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_adaptive_threshold(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double max_value,
    int adaptive_method,
    int threshold_type,
    int block_size,
    double c);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_integral(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* sum,
    int sdepth);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_integral2(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* sum,
    jyppx_ocv_mat* sqsum,
    int sdepth,
    int sqdepth);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_integral3(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* sum,
    jyppx_ocv_mat* sqsum,
    jyppx_ocv_mat* tilted,
    int sdepth,
    int sqdepth);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_distance_transform(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int distance_type,
    int mask_size,
    int dst_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_distance_transform_with_labels(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    jyppx_ocv_mat* labels,
    int distance_type,
    int mask_size,
    int label_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_flood_fill(
    jyppx_ocv_mat* image,
    int seed_x,
    int seed_y,
    double new_value_v0,
    double new_value_v1,
    double new_value_v2,
    double new_value_v3,
    int* rect_x,
    int* rect_y,
    int* rect_width,
    int* rect_height,
    double lo_diff_v0,
    double lo_diff_v1,
    double lo_diff_v2,
    double lo_diff_v3,
    double up_diff_v0,
    double up_diff_v1,
    double up_diff_v2,
    double up_diff_v3,
    int flags,
    int* filled_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_flood_fill_mask(
    jyppx_ocv_mat* image,
    jyppx_ocv_mat* mask,
    int seed_x,
    int seed_y,
    double new_value_v0,
    double new_value_v1,
    double new_value_v2,
    double new_value_v3,
    int* rect_x,
    int* rect_y,
    int* rect_width,
    int* rect_height,
    double lo_diff_v0,
    double lo_diff_v1,
    double lo_diff_v2,
    double lo_diff_v3,
    double up_diff_v0,
    double up_diff_v1,
    double up_diff_v2,
    double up_diff_v3,
    int flags,
    int* filled_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_connected_components(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* labels,
    int connectivity,
    int ltype,
    int* label_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_connected_components_with_algorithm(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* labels,
    int connectivity,
    int ltype,
    int ccltype,
    int* label_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_connected_components_with_stats(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* labels,
    jyppx_ocv_mat* stats,
    jyppx_ocv_mat* centroids,
    int connectivity,
    int ltype,
    int* label_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_connected_components_with_stats_with_algorithm(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* labels,
    jyppx_ocv_mat* stats,
    jyppx_ocv_mat* centroids,
    int connectivity,
    int ltype,
    int ccltype,
    int* label_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_equalize_hist(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_clahe_create(
    double clip_limit,
    int tiles_grid_width,
    int tiles_grid_height,
    jyppx_ocv_clahe** clahe);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_imgproc_clahe_release(
    jyppx_ocv_clahe* clahe);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_clahe_apply(
    jyppx_ocv_clahe* clahe,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_clahe_get_clip_limit(
    const jyppx_ocv_clahe* clahe,
    double* clip_limit);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_clahe_set_clip_limit(
    jyppx_ocv_clahe* clahe,
    double clip_limit);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_clahe_get_tiles_grid_size(
    const jyppx_ocv_clahe* clahe,
    int* width,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_clahe_set_tiles_grid_size(
    jyppx_ocv_clahe* clahe,
    int width,
    int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_clahe_get_bit_shift(
    const jyppx_ocv_clahe* clahe,
    int* bit_shift);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_clahe_set_bit_shift(
    jyppx_ocv_clahe* clahe,
    int bit_shift);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_clahe_collect_garbage(
    jyppx_ocv_clahe* clahe);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_corner_harris(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int block_size,
    int ksize,
    double k,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_corner_min_eigen_val(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int block_size,
    int ksize,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_corner_eigen_vals_and_vecs(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int block_size,
    int ksize,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_pre_corner_detect(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ksize,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_corner_sub_pix(
    const jyppx_ocv_mat* image,
    float* corners_xy,
    int corner_count,
    int win_width,
    int win_height,
    int zero_zone_width,
    int zero_zone_height,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_good_features_to_track_count(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    int max_corners,
    double quality_level,
    double min_distance,
    int block_size,
    int gradient_size,
    int use_harris_detector,
    double k,
    int* corner_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_good_features_to_track_fill(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    int max_corners,
    double quality_level,
    double min_distance,
    int block_size,
    int gradient_size,
    int use_harris_detector,
    double k,
    float* corners_xy,
    int corner_capacity,
    int* corner_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_hough_lines_count(
    const jyppx_ocv_mat* image,
    double rho,
    double theta,
    int threshold,
    double srn,
    double stn,
    double min_theta,
    double max_theta,
    int use_edgeval,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_hough_lines_fill(
    const jyppx_ocv_mat* image,
    double rho,
    double theta,
    int threshold,
    double srn,
    double stn,
    double min_theta,
    double max_theta,
    int use_edgeval,
    float* lines,
    int line_capacity,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_hough_lines_p_count(
    const jyppx_ocv_mat* image,
    double rho,
    double theta,
    int threshold,
    double min_line_length,
    double max_line_gap,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_hough_lines_p_fill(
    const jyppx_ocv_mat* image,
    double rho,
    double theta,
    int threshold,
    double min_line_length,
    double max_line_gap,
    int* lines,
    int line_capacity,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_hough_lines_point_set_count(
    const int* points_xy,
    int point_count,
    int lines_max,
    int threshold,
    double min_rho,
    double max_rho,
    double rho_step,
    double min_theta,
    double max_theta,
    double theta_step,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_hough_lines_point_set_fill(
    const int* points_xy,
    int point_count,
    int lines_max,
    int threshold,
    double min_rho,
    double max_rho,
    double rho_step,
    double min_theta,
    double max_theta,
    double theta_step,
    double* lines,
    int line_capacity,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_hough_circles_count(
    const jyppx_ocv_mat* image,
    int method,
    double dp,
    double min_dist,
    double param1,
    double param2,
    int min_radius,
    int max_radius,
    int* circle_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_hough_circles_fill(
    const jyppx_ocv_mat* image,
    int method,
    double dp,
    double min_dist,
    double param1,
    double param2,
    int min_radius,
    int max_radius,
    float* circles,
    int circle_capacity,
    int* circle_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_calc_hist_uniform(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const int* channels,
    int channel_count,
    jyppx_ocv_mat* hist,
    const int* hist_size,
    int hist_dims,
    const float* ranges,
    int range_count,
    int accumulate);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_calc_back_project_uniform(
    const jyppx_ocv_mat* image,
    const int* channels,
    int channel_count,
    const jyppx_ocv_mat* hist,
    jyppx_ocv_mat* back_project,
    const float* ranges,
    int range_count,
    double scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_compare_hist(
    const jyppx_ocv_mat* h1,
    const jyppx_ocv_mat* h2,
    int method,
    double* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_line_segment_detector_create(
    int refine,
    double scale,
    double sigma_scale,
    double quant,
    double ang_th,
    double log_eps,
    double density_th,
    int n_bins,
    jyppx_ocv_line_segment_detector** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_imgproc_line_segment_detector_release(
    jyppx_ocv_line_segment_detector* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_line_segment_detector_detect(
    jyppx_ocv_line_segment_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* lines,
    jyppx_ocv_mat* width,
    jyppx_ocv_mat* prec,
    jyppx_ocv_mat* nfa);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_line_segment_detector_detect_count(
    jyppx_ocv_line_segment_detector* detector,
    const jyppx_ocv_mat* image,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_line_segment_detector_detect_fill(
    jyppx_ocv_line_segment_detector* detector,
    const jyppx_ocv_mat* image,
    float* lines,
    int line_capacity,
    int* line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_line_segment_detector_draw_segments(
    jyppx_ocv_line_segment_detector* detector,
    jyppx_ocv_mat* image,
    const jyppx_ocv_mat* lines);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_line_segment_detector_draw_segments_array(
    jyppx_ocv_line_segment_detector* detector,
    jyppx_ocv_mat* image,
    const float* lines,
    int line_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_line_segment_detector_compare_segments(
    jyppx_ocv_line_segment_detector* detector,
    int width,
    int height,
    const jyppx_ocv_mat* lines1,
    const jyppx_ocv_mat* lines2,
    jyppx_ocv_mat* image,
    int* mismatch_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_line_segment_detector_compare_segments_array(
    jyppx_ocv_line_segment_detector* detector,
    int width,
    int height,
    const float* lines1,
    int line1_count,
    const float* lines2,
    int line2_count,
    jyppx_ocv_mat* image,
    int* mismatch_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_gaussian_blur(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    double sigma_x,
    double sigma_y,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_blur(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    int anchor_x,
    int anchor_y,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_box_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    int width,
    int height,
    int anchor_x,
    int anchor_y,
    int normalize,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_sqr_box_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    int width,
    int height,
    int anchor_x,
    int anchor_y,
    int normalize,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_median_blur(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ksize);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_bilateral_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int d,
    double sigma_color,
    double sigma_space,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_filter2d(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    const jyppx_ocv_mat* kernel,
    int anchor_x,
    int anchor_y,
    double delta,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_sep_filter2d(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    const jyppx_ocv_mat* kernel_x,
    const jyppx_ocv_mat* kernel_y,
    int anchor_x,
    int anchor_y,
    double delta,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_sobel(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    int dx,
    int dy,
    int ksize,
    double scale,
    double delta,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_scharr(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    int dx,
    int dy,
    double scale,
    double delta,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_laplacian(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    int ksize,
    double scale,
    double delta,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_canny(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* edges,
    double threshold1,
    double threshold2,
    int aperture_size,
    int l2_gradient);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_canny_derivatives(
    const jyppx_ocv_mat* dx,
    const jyppx_ocv_mat* dy,
    jyppx_ocv_mat* edges,
    double threshold1,
    double threshold2,
    int l2_gradient);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_get_gaussian_kernel(
    int ksize,
    double sigma,
    int ktype,
    jyppx_ocv_mat** out_kernel);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_get_deriv_kernels(
    jyppx_ocv_mat* kx,
    jyppx_ocv_mat* ky,
    int dx,
    int dy,
    int ksize,
    int normalize,
    int ktype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_get_gabor_kernel(
    int width,
    int height,
    double sigma,
    double theta,
    double lambd,
    double gamma,
    double psi,
    int ktype,
    jyppx_ocv_mat** out_kernel);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_pyr_down(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_pyr_up(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    int border_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_warp_affine(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* transform,
    int width,
    int height,
    int flags,
    int border_mode,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_warp_perspective(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* transform,
    int width,
    int height,
    int flags,
    int border_mode,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_get_rotation_matrix2d(
    float center_x,
    float center_y,
    double angle,
    double scale,
    jyppx_ocv_mat** out_transform);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_get_affine_transform(
    const float* src_xy,
    const float* dst_xy,
    jyppx_ocv_mat** out_transform);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_get_perspective_transform(
    const float* src_xy,
    const float* dst_xy,
    int solve_method,
    jyppx_ocv_mat** out_transform);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_invert_affine_transform(
    const jyppx_ocv_mat* transform,
    jyppx_ocv_mat* inverse_transform);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_remap(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* map1,
    const jyppx_ocv_mat* map2,
    int interpolation,
    int border_mode,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_convert_maps(
    const jyppx_ocv_mat* map1,
    const jyppx_ocv_mat* map2,
    jyppx_ocv_mat* dstmap1,
    jyppx_ocv_mat* dstmap2,
    int dstmap1type,
    int nninterpolation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_get_structuring_element(
    int shape,
    int width,
    int height,
    int anchor_x,
    int anchor_y,
    jyppx_ocv_mat** out_element);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_erode(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* kernel,
    int anchor_x,
    int anchor_y,
    int iterations,
    int border_type,
    int has_border_value,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_dilate(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* kernel,
    int anchor_x,
    int anchor_y,
    int iterations,
    int border_type,
    int has_border_value,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_morphology_ex(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int op,
    const jyppx_ocv_mat* kernel,
    int anchor_x,
    int anchor_y,
    int iterations,
    int border_type,
    int has_border_value,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_line(
    jyppx_ocv_mat* img,
    int x1,
    int y1,
    int x2,
    int y2,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_arrowed_line(
    jyppx_ocv_mat* img,
    int x1,
    int y1,
    int x2,
    int y2,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift,
    double tip_length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_clip_line_rect(
    int rect_x,
    int rect_y,
    int rect_width,
    int rect_height,
    int* pt1_x,
    int* pt1_y,
    int* pt2_x,
    int* pt2_y,
    int* intersects);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_polylines(
    jyppx_ocv_mat* img,
    const int* points_xy,
    int point_count,
    int is_closed,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_fill_poly(
    jyppx_ocv_mat* img,
    const int* points_xy,
    int point_count,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int line_type,
    int shift,
    int offset_x,
    int offset_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_ellipse2_poly_count(
    int center_x,
    int center_y,
    int axes_width,
    int axes_height,
    int angle,
    int arc_start,
    int arc_end,
    int delta,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_ellipse2_poly_fill(
    int center_x,
    int center_y,
    int axes_width,
    int axes_height,
    int angle,
    int arc_start,
    int arc_end,
    int delta,
    int* points_xy,
    int point_capacity,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_contour_area(
    const int* points_xy,
    int point_count,
    int oriented,
    double* area);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_find_contours_count(
    const jyppx_ocv_mat* image,
    int mode,
    int method,
    int offset_x,
    int offset_y,
    int* contour_count,
    int* total_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_find_contours_fill(
    const jyppx_ocv_mat* image,
    int mode,
    int method,
    int offset_x,
    int offset_y,
    int* contours_xy,
    int point_capacity,
    int* contour_lengths,
    int contour_capacity,
    int* hierarchy,
    int hierarchy_capacity,
    int* contour_count,
    int* total_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_draw_contours(
    jyppx_ocv_mat* image,
    const int* contours_xy,
    const int* contour_lengths,
    int contour_count,
    int contour_index,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    const int* hierarchy,
    int has_hierarchy,
    int max_level,
    int offset_x,
    int offset_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_moments_points(
    const int* points_xy,
    int point_count,
    int binary_image,
    double* values,
    int value_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_moments_mat(
    const jyppx_ocv_mat* array,
    int binary_image,
    double* values,
    int value_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_hu_moments(
    const double* moments_values,
    int value_count,
    double* hu_values,
    int hu_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_arc_length(
    const int* points_xy,
    int point_count,
    int closed,
    double* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_approx_poly_dp_count(
    const int* curve_xy,
    int point_count,
    double epsilon,
    int closed,
    int* approx_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_approx_poly_dp_fill(
    const int* curve_xy,
    int point_count,
    double epsilon,
    int closed,
    int* approx_points_xy,
    int approx_point_capacity,
    int* approx_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_approx_poly_n_count(
    const int* curve_xy,
    int point_count,
    int nsides,
    float epsilon_percentage,
    int ensure_convex,
    int* approx_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_approx_poly_n_fill(
    const int* curve_xy,
    int point_count,
    int nsides,
    float epsilon_percentage,
    int ensure_convex,
    float* approx_points_xy,
    int approx_point_capacity,
    int* approx_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_bounding_rect(
    const int* points_xy,
    int point_count,
    int* x,
    int* y,
    int* width,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_is_contour_convex(
    const int* points_xy,
    int point_count,
    int* is_convex);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_convex_hull_count(
    const int* points_xy,
    int point_count,
    int clockwise,
    int* hull_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_convex_hull_fill(
    const int* points_xy,
    int point_count,
    int clockwise,
    int* hull_points_xy,
    int hull_point_capacity,
    int* hull_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_convex_hull_indices_count(
    const int* points_xy,
    int point_count,
    int clockwise,
    int* hull_index_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_convex_hull_indices_fill(
    const int* points_xy,
    int point_count,
    int clockwise,
    int* hull_indices,
    int hull_index_capacity,
    int* hull_index_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_convexity_defects_count(
    const int* contour_xy,
    int contour_point_count,
    const int* hull_indices,
    int hull_index_count,
    int* defect_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_convexity_defects_fill(
    const int* contour_xy,
    int contour_point_count,
    const int* hull_indices,
    int hull_index_count,
    int* defects,
    int defect_capacity,
    int* defect_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_min_enclosing_circle(
    const int* points_xy,
    int point_count,
    float* center_x,
    float* center_y,
    float* radius);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_point_polygon_test(
    const int* contour_xy,
    int point_count,
    float point_x,
    float point_y,
    int measure_dist,
    double* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_match_shapes(
    const int* contour1_xy,
    int contour1_point_count,
    const int* contour2_xy,
    int contour2_point_count,
    int method,
    double parameter,
    double* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_min_area_rect(
    const int* points_xy,
    int point_count,
    float* center_x,
    float* center_y,
    float* width,
    float* height,
    float* angle);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_box_points(
    float center_x,
    float center_y,
    float width,
    float height,
    float angle,
    float* points_xy,
    int point_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_fit_ellipse(
    const int* points_xy,
    int point_count,
    float* center_x,
    float* center_y,
    float* width,
    float* height,
    float* angle);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_fit_ellipse_ams(
    const int* points_xy,
    int point_count,
    float* center_x,
    float* center_y,
    float* width,
    float* height,
    float* angle);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_fit_ellipse_direct(
    const int* points_xy,
    int point_count,
    float* center_x,
    float* center_y,
    float* width,
    float* height,
    float* angle);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_rotated_rectangle_intersection_count(
    float rect1_center_x,
    float rect1_center_y,
    float rect1_width,
    float rect1_height,
    float rect1_angle,
    float rect2_center_x,
    float rect2_center_y,
    float rect2_width,
    float rect2_height,
    float rect2_angle,
    int* intersection_type,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_rotated_rectangle_intersection_fill(
    float rect1_center_x,
    float rect1_center_y,
    float rect1_width,
    float rect1_height,
    float rect1_angle,
    float rect2_center_x,
    float rect2_center_y,
    float rect2_width,
    float rect2_height,
    float rect2_angle,
    float* points_xy,
    int point_capacity,
    int* intersection_type,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_get_closest_ellipse_points(
    float center_x,
    float center_y,
    float width,
    float height,
    float angle,
    const int* points_xy,
    int point_count,
    float* closest_points_xy,
    int closest_point_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_min_enclosing_triangle(
    const int* points_xy,
    int point_count,
    float* triangle_points_xy,
    int triangle_point_capacity,
    double* area);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_min_enclosing_convex_polygon(
    const int* points_xy,
    int point_count,
    int k,
    float* polygon_points_xy,
    int polygon_point_capacity,
    int* polygon_point_count,
    double* area);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_intersect_convex_convex_count(
    const int* polygon1_xy,
    int polygon1_point_count,
    const int* polygon2_xy,
    int polygon2_point_count,
    int handle_nested,
    float* area,
    int* intersecting_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_intersect_convex_convex_fill(
    const int* polygon1_xy,
    int polygon1_point_count,
    const int* polygon2_xy,
    int polygon2_point_count,
    int handle_nested,
    float* intersecting_points_xy,
    int intersecting_point_capacity,
    float* area,
    int* intersecting_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_fit_line_2d(
    const int* points_xy,
    int point_count,
    int dist_type,
    double param,
    double reps,
    double aeps,
    float* vx,
    float* vy,
    float* x0,
    float* y0);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_rectangle(
    jyppx_ocv_mat* img,
    int x1,
    int y1,
    int x2,
    int y2,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_rectangle_by_rect(
    jyppx_ocv_mat* img,
    int x,
    int y,
    int width,
    int height,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_circle(
    jyppx_ocv_mat* img,
    int center_x,
    int center_y,
    int radius,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_ellipse(
    jyppx_ocv_mat* img,
    int center_x,
    int center_y,
    int axes_width,
    int axes_height,
    double angle,
    double start_angle,
    double end_angle,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_put_text(
    jyppx_ocv_mat* img,
    const char* text,
    int org_x,
    int org_y,
    int font_face,
    double font_scale,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int bottom_left_origin);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_imgproc_get_text_size(
    const char* text,
    int font_face,
    double font_scale,
    int thickness,
    int* width,
    int* height,
    int* base_line);
