#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_xphoto_white_balancer jyppx_ocv_xphoto_white_balancer;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_simple_wb_create(
    jyppx_ocv_xphoto_white_balancer** white_balancer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_grayworld_wb_create(
    jyppx_ocv_xphoto_white_balancer** white_balancer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_learning_based_wb_create(
    const char* model_path,
    jyppx_ocv_xphoto_white_balancer** white_balancer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_xphoto_white_balancer_release_handle(
    jyppx_ocv_xphoto_white_balancer* white_balancer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_white_balancer_balance_white(
    jyppx_ocv_xphoto_white_balancer* white_balancer,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_simple_wb_get_property(
    const jyppx_ocv_xphoto_white_balancer* white_balancer,
    int property_id,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_simple_wb_set_property(
    jyppx_ocv_xphoto_white_balancer* white_balancer,
    int property_id,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_grayworld_wb_get_saturation_threshold(
    const jyppx_ocv_xphoto_white_balancer* white_balancer,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_grayworld_wb_set_saturation_threshold(
    jyppx_ocv_xphoto_white_balancer* white_balancer,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_learning_based_wb_get_int_property(
    const jyppx_ocv_xphoto_white_balancer* white_balancer,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_learning_based_wb_set_int_property(
    jyppx_ocv_xphoto_white_balancer* white_balancer,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_learning_based_wb_get_saturation_threshold(
    const jyppx_ocv_xphoto_white_balancer* white_balancer,
    float* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_learning_based_wb_set_saturation_threshold(
    jyppx_ocv_xphoto_white_balancer* white_balancer,
    float value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_learning_based_wb_extract_simple_features(
    jyppx_ocv_xphoto_white_balancer* white_balancer,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_apply_channel_gains(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float gain_b,
    float gain_g,
    float gain_r);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_dct_denoising(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double sigma,
    int psize);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_bm3d_denoising(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float h,
    int template_window_size,
    int search_window_size,
    int block_matching_step1,
    int block_matching_step2,
    int group_size,
    int sliding_step,
    float beta,
    int norm_type,
    int step,
    int transform_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_bm3d_denoising_steps(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst_step1,
    jyppx_ocv_mat* dst_step2,
    float h,
    int template_window_size,
    int search_window_size,
    int block_matching_step1,
    int block_matching_step2,
    int group_size,
    int sliding_step,
    float beta,
    int norm_type,
    int step,
    int transform_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_xphoto_oil_painting(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int size,
    int dyn_ratio,
    int code,
    int use_code);
