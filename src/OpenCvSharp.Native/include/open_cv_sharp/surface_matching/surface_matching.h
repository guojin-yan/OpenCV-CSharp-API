#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

#include <stdint.h>

typedef struct jyppx_ocv_surface_matching_icp jyppx_ocv_surface_matching_icp;
typedef struct jyppx_ocv_surface_matching_ppf_3d_detector jyppx_ocv_surface_matching_ppf_3d_detector;

typedef struct jyppx_ocv_surface_matching_pose_3d_result
{
    double alpha;
    double residual;
    uint64_t model_index;
    uint64_t num_votes;
    double angle;
    double t0;
    double t1;
    double t2;
    double q0;
    double q1;
    double q2;
    double q3;
    double pose00;
    double pose01;
    double pose02;
    double pose03;
    double pose10;
    double pose11;
    double pose12;
    double pose13;
    double pose20;
    double pose21;
    double pose22;
    double pose23;
    double pose30;
    double pose31;
    double pose32;
    double pose33;
} jyppx_ocv_surface_matching_pose_3d_result;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_surface_matching_icp_create(
    int iterations,
    float tolerance,
    float rejection_scale,
    int num_levels,
    int sample_type,
    int num_max_corr,
    jyppx_ocv_surface_matching_icp** icp);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_surface_matching_icp_release(
    jyppx_ocv_surface_matching_icp* icp);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_surface_matching_icp_register_model_to_scene(
    jyppx_ocv_surface_matching_icp* icp,
    const jyppx_ocv_mat* src_pc,
    const jyppx_ocv_mat* dst_pc,
    int* result_code,
    double* residual,
    double* pose16,
    int pose16_capacity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_surface_matching_ppf_3d_detector_create(
    double relative_sampling_step,
    double relative_distance_step,
    double num_angles,
    jyppx_ocv_surface_matching_ppf_3d_detector** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_surface_matching_ppf_3d_detector_release(
    jyppx_ocv_surface_matching_ppf_3d_detector* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_surface_matching_ppf_3d_detector_set_search_params(
    jyppx_ocv_surface_matching_ppf_3d_detector* detector,
    double position_threshold,
    double rotation_threshold,
    int use_weighted_clustering);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_surface_matching_ppf_3d_detector_train_model(
    jyppx_ocv_surface_matching_ppf_3d_detector* detector,
    const jyppx_ocv_mat* model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_surface_matching_ppf_3d_detector_match_count(
    jyppx_ocv_surface_matching_ppf_3d_detector* detector,
    const jyppx_ocv_mat* scene,
    double relative_scene_sample_step,
    double relative_scene_distance,
    int* result_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_surface_matching_ppf_3d_detector_match_fill(
    jyppx_ocv_surface_matching_ppf_3d_detector* detector,
    const jyppx_ocv_mat* scene,
    double relative_scene_sample_step,
    double relative_scene_distance,
    jyppx_ocv_surface_matching_pose_3d_result* results,
    int result_capacity,
    int* result_count);
