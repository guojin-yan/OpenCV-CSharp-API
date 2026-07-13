#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_xstereo_binary_bm jyppx_ocv_xstereo_binary_bm;
typedef struct jyppx_ocv_xstereo_binary_sgbm jyppx_ocv_xstereo_binary_sgbm;
typedef struct jyppx_ocv_xstereo_quasi_dense_stereo jyppx_ocv_xstereo_quasi_dense_stereo;

typedef struct jyppx_ocv_xstereo_match_quasi_dense
{
    int p0_x;
    int p0_y;
    int p1_x;
    int p1_y;
    float corr;
} jyppx_ocv_xstereo_match_quasi_dense;

typedef struct jyppx_ocv_xstereo_propagation_parameters
{
    int corr_win_size_x;
    int corr_win_size_y;
    int border_x;
    int border_y;
    float correlation_threshold;
    float textrure_threshold;
    int neighborhood_size;
    int disparity_gradient;
    int lk_template_size;
    int lk_pyr_lvl;
    int lk_term_param1;
    float lk_term_param2;
    float gft_quality_thres;
    int gft_min_seperation_dist;
    int gft_max_num_features;
} jyppx_ocv_xstereo_propagation_parameters;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_census_transform(
    const jyppx_ocv_mat* image,
    int kernel_size,
    jyppx_ocv_mat* dist,
    int type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_census_transform_pair(
    const jyppx_ocv_mat* image1,
    const jyppx_ocv_mat* image2,
    int kernel_size,
    jyppx_ocv_mat* dist1,
    jyppx_ocv_mat* dist2,
    int type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_modified_census_transform(
    const jyppx_ocv_mat* image,
    int kernel_size,
    jyppx_ocv_mat* dist,
    int type,
    int t,
    const jyppx_ocv_mat* integral_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_modified_census_transform_pair(
    const jyppx_ocv_mat* image1,
    const jyppx_ocv_mat* image2,
    int kernel_size,
    jyppx_ocv_mat* dist1,
    jyppx_ocv_mat* dist2,
    int type,
    int t,
    const jyppx_ocv_mat* integral_image1,
    const jyppx_ocv_mat* integral_image2);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_symetric_census_transform(
    const jyppx_ocv_mat* image,
    int kernel_size,
    jyppx_ocv_mat* dist,
    int type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_symetric_census_transform_pair(
    const jyppx_ocv_mat* image1,
    const jyppx_ocv_mat* image2,
    int kernel_size,
    jyppx_ocv_mat* dist1,
    jyppx_ocv_mat* dist2,
    int type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_star_census_transform(
    const jyppx_ocv_mat* image,
    int kernel_size,
    jyppx_ocv_mat* dist);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_star_census_transform_pair(
    const jyppx_ocv_mat* image1,
    const jyppx_ocv_mat* image2,
    int kernel_size,
    jyppx_ocv_mat* dist1,
    jyppx_ocv_mat* dist2);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_create(
    int num_disparities,
    int block_size,
    jyppx_ocv_xstereo_binary_bm** matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_xstereo_binary_bm_release(
    jyppx_ocv_xstereo_binary_bm* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_compute(
    jyppx_ocv_xstereo_binary_bm* matcher,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right,
    jyppx_ocv_mat* disparity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_create(
    int min_disparity,
    int num_disparities,
    int block_size,
    int p1,
    int p2,
    int disp12_max_diff,
    int pre_filter_cap,
    int uniqueness_ratio,
    int speckle_window_size,
    int speckle_range,
    int mode,
    jyppx_ocv_xstereo_binary_sgbm** matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_xstereo_binary_sgbm_release(
    jyppx_ocv_xstereo_binary_sgbm* matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_compute(
    jyppx_ocv_xstereo_binary_sgbm* matcher,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right,
    jyppx_ocv_mat* disparity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_min_disparity(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_min_disparity(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_num_disparities(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_num_disparities(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_block_size(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_block_size(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_speckle_window_size(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_speckle_window_size(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_speckle_range(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_speckle_range(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_disp12_max_diff(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_disp12_max_diff(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_pre_filter_type(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_pre_filter_type(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_pre_filter_size(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_pre_filter_size(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_pre_filter_cap(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_pre_filter_cap(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_texture_threshold(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_texture_threshold(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_uniqueness_ratio(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_uniqueness_ratio(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_smaller_block_size(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_smaller_block_size(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_scalle_factor(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_scalle_factor(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_spekle_removal_technique(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_spekle_removal_technique(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_use_prefilter(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_use_prefilter(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_binary_kernel_type(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_binary_kernel_type(jyppx_ocv_xstereo_binary_bm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_get_agregation_window_size(const jyppx_ocv_xstereo_binary_bm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_bm_set_agregation_window_size(jyppx_ocv_xstereo_binary_bm* matcher, int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_min_disparity(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_min_disparity(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_num_disparities(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_num_disparities(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_block_size(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_block_size(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_speckle_window_size(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_speckle_window_size(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_speckle_range(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_speckle_range(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_disp12_max_diff(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_disp12_max_diff(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_pre_filter_cap(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_pre_filter_cap(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_uniqueness_ratio(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_uniqueness_ratio(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_p1(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_p1(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_p2(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_p2(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_mode(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_mode(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_spekle_removal_technique(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_spekle_removal_technique(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_binary_kernel_type(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_binary_kernel_type(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_get_sub_pixel_interpolation_method(const jyppx_ocv_xstereo_binary_sgbm* matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_binary_sgbm_set_sub_pixel_interpolation_method(jyppx_ocv_xstereo_binary_sgbm* matcher, int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_create(
    int width,
    int height,
    const unsigned char* parameter_file_path,
    jyppx_ocv_xstereo_quasi_dense_stereo** stereo);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_xstereo_quasi_dense_release(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_load_parameters(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    const unsigned char* parameter_file_path,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_save_parameters(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    const unsigned char* parameter_file_path,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_process(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_get_sparse_matches_count(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_get_sparse_matches_fill(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    jyppx_ocv_xstereo_match_quasi_dense* matches,
    int match_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_get_dense_matches_count(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_get_dense_matches_fill(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    jyppx_ocv_xstereo_match_quasi_dense* matches,
    int match_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_get_match(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    int x,
    int y,
    float* match_x,
    float* match_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_get_disparity(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    jyppx_ocv_mat* disparity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_get_parameters(
    const jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    jyppx_ocv_xstereo_propagation_parameters* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xstereo_quasi_dense_set_parameters(
    jyppx_ocv_xstereo_quasi_dense_stereo* stereo,
    const jyppx_ocv_xstereo_propagation_parameters* parameters);
