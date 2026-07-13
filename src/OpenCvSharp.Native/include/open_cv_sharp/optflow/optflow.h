#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_optflow_dense jyppx_ocv_optflow_dense;
typedef struct jyppx_ocv_optflow_sparse jyppx_ocv_optflow_sparse;
typedef struct jyppx_ocv_optflow_dual_tvl1 jyppx_ocv_optflow_dual_tvl1;
typedef struct jyppx_ocv_optflow_rlof_parameter jyppx_ocv_optflow_rlof_parameter;
typedef struct jyppx_ocv_optflow_dense_rlof jyppx_ocv_optflow_dense_rlof;
typedef struct jyppx_ocv_optflow_sparse_rlof jyppx_ocv_optflow_sparse_rlof;

typedef struct jyppx_ocv_optflow_rect
{
    int x;
    int y;
    int width;
    int height;
} jyppx_ocv_optflow_rect;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_optflow_dense_release_handle(
    jyppx_ocv_optflow_dense* flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_optflow_sparse_release_handle(
    jyppx_ocv_optflow_sparse* flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_optflow_rlof_parameter_release_handle(
    jyppx_ocv_optflow_rlof_parameter* parameter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_calc(
    jyppx_ocv_optflow_dense* flow,
    const jyppx_ocv_mat* i0,
    const jyppx_ocv_mat* i1,
    jyppx_ocv_mat* output_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_collect_garbage(
    jyppx_ocv_optflow_dense* flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_sparse_calc(
    jyppx_ocv_optflow_sparse* flow,
    const jyppx_ocv_mat* prev_img,
    const jyppx_ocv_mat* next_img,
    const jyppx_ocv_mat* prev_pts,
    jyppx_ocv_mat* next_pts,
    jyppx_ocv_mat* status,
    jyppx_ocv_mat* err);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dual_tvl1_create(
    double tau,
    double lambda_value,
    double theta,
    int nscales,
    int warps,
    double epsilon,
    int inner_iterations,
    int outer_iterations,
    double scale_step,
    double gamma,
    int median_filtering,
    int use_initial_flow,
    jyppx_ocv_optflow_dual_tvl1** flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dual_tvl1_get_int(
    const jyppx_ocv_optflow_dual_tvl1* flow,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dual_tvl1_set_int(
    jyppx_ocv_optflow_dual_tvl1* flow,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dual_tvl1_get_double(
    const jyppx_ocv_optflow_dual_tvl1* flow,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dual_tvl1_set_double(
    jyppx_ocv_optflow_dual_tvl1* flow,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_rlof_parameter_create(
    jyppx_ocv_optflow_rlof_parameter** parameter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_rlof_parameter_get_int(
    const jyppx_ocv_optflow_rlof_parameter* parameter,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_rlof_parameter_set_int(
    jyppx_ocv_optflow_rlof_parameter* parameter,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_rlof_parameter_get_float(
    const jyppx_ocv_optflow_rlof_parameter* parameter,
    int property_id,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_rlof_parameter_set_float(
    jyppx_ocv_optflow_rlof_parameter* parameter,
    int property_id,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_rlof_parameter_set_use_m_estimator(
    jyppx_ocv_optflow_rlof_parameter* parameter,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_rlof_create(
    const jyppx_ocv_optflow_rlof_parameter* parameter,
    float forward_backward_threshold,
    int grid_width,
    int grid_height,
    int interpolation_type,
    int epic_k,
    float epic_sigma,
    float epic_lambda,
    int ric_sp_size,
    int ric_slic_type,
    int use_post_proc,
    float fgs_lambda,
    float fgs_sigma,
    int use_variational_refinement,
    jyppx_ocv_optflow_dense_rlof** flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_rlof_get_parameter(
    const jyppx_ocv_optflow_dense_rlof* flow,
    jyppx_ocv_optflow_rlof_parameter** parameter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_rlof_set_parameter(
    jyppx_ocv_optflow_dense_rlof* flow,
    const jyppx_ocv_optflow_rlof_parameter* parameter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_rlof_get_int(
    const jyppx_ocv_optflow_dense_rlof* flow,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_rlof_set_int(
    jyppx_ocv_optflow_dense_rlof* flow,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_rlof_get_float(
    const jyppx_ocv_optflow_dense_rlof* flow,
    int property_id,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_rlof_set_float(
    jyppx_ocv_optflow_dense_rlof* flow,
    int property_id,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_rlof_get_grid_step(
    const jyppx_ocv_optflow_dense_rlof* flow,
    int* width,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_dense_rlof_set_grid_step(
    jyppx_ocv_optflow_dense_rlof* flow,
    int width,
    int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_sparse_rlof_create(
    const jyppx_ocv_optflow_rlof_parameter* parameter,
    float forward_backward_threshold,
    jyppx_ocv_optflow_sparse_rlof** flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_sparse_rlof_get_parameter(
    const jyppx_ocv_optflow_sparse_rlof* flow,
    jyppx_ocv_optflow_rlof_parameter** parameter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_sparse_rlof_set_parameter(
    jyppx_ocv_optflow_sparse_rlof* flow,
    const jyppx_ocv_optflow_rlof_parameter* parameter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_sparse_rlof_get_forward_backward(
    const jyppx_ocv_optflow_sparse_rlof* flow,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_sparse_rlof_set_forward_backward(
    jyppx_ocv_optflow_sparse_rlof* flow,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_create_deep_flow(
    jyppx_ocv_optflow_dense** flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_create_simple_flow(
    jyppx_ocv_optflow_dense** flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_create_farneback(
    jyppx_ocv_optflow_dense** flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_create_sparse_to_dense(
    jyppx_ocv_optflow_dense** flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_calc_optical_flow_sf_simple(
    const jyppx_ocv_mat* from,
    const jyppx_ocv_mat* to,
    jyppx_ocv_mat* flow,
    int layers,
    int averaging_block_size,
    int max_flow);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_calc_optical_flow_sf(
    const jyppx_ocv_mat* from,
    const jyppx_ocv_mat* to,
    jyppx_ocv_mat* flow,
    int layers,
    int averaging_block_size,
    int max_flow,
    double sigma_dist,
    double sigma_color,
    int postprocess_window,
    double sigma_dist_fix,
    double sigma_color_fix,
    double occ_thr,
    int upscale_averaging_radius,
    double upscale_sigma_dist,
    double upscale_sigma_color,
    double speed_up_thr);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_calc_optical_flow_sparse_to_dense(
    const jyppx_ocv_mat* from,
    const jyppx_ocv_mat* to,
    jyppx_ocv_mat* flow,
    int grid_step,
    int k,
    float sigma,
    int use_post_proc,
    float fgs_lambda,
    float fgs_sigma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_calc_optical_flow_dense_rlof(
    const jyppx_ocv_mat* i0,
    const jyppx_ocv_mat* i1,
    jyppx_ocv_mat* flow,
    const jyppx_ocv_optflow_rlof_parameter* parameter,
    float forward_backward_threshold,
    int grid_width,
    int grid_height,
    int interpolation_type,
    int epic_k,
    float epic_sigma,
    float epic_lambda,
    int ric_sp_size,
    int ric_slic_type,
    int use_post_proc,
    float fgs_lambda,
    float fgs_sigma,
    int use_variational_refinement);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_optflow_calc_optical_flow_sparse_rlof(
    const jyppx_ocv_mat* prev_img,
    const jyppx_ocv_mat* next_img,
    const jyppx_ocv_mat* prev_pts,
    jyppx_ocv_mat* next_pts,
    jyppx_ocv_mat* status,
    jyppx_ocv_mat* err,
    const jyppx_ocv_optflow_rlof_parameter* parameter,
    float forward_backward_threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_motempl_update_motion_history(
    const jyppx_ocv_mat* silhouette,
    jyppx_ocv_mat* mhi,
    double timestamp,
    double duration);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_motempl_calc_motion_gradient(
    const jyppx_ocv_mat* mhi,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat* orientation,
    double delta1,
    double delta2,
    int aperture_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_motempl_calc_global_orientation(
    const jyppx_ocv_mat* orientation,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_mat* mhi,
    double timestamp,
    double duration,
    double* angle);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_motempl_segment_motion_count(
    const jyppx_ocv_mat* mhi,
    jyppx_ocv_mat* segmask,
    double timestamp,
    double seg_thresh,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_motempl_segment_motion_fill(
    const jyppx_ocv_mat* mhi,
    jyppx_ocv_mat* segmask,
    double timestamp,
    double seg_thresh,
    jyppx_ocv_optflow_rect* rects,
    int rect_capacity,
    int* count);
