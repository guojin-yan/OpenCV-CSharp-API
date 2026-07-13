#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_tonemap jyppx_ocv_tonemap;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_decolor(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* grayscale,
    jyppx_ocv_mat* color_boost);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_inpaint(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* inpaint_mask,
    jyppx_ocv_mat* dst,
    double inpaint_radius,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_fast_nl_means_denoising(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float h,
    int template_window_size,
    int search_window_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_fast_nl_means_denoising_with_h_array(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const float* h,
    int h_count,
    int template_window_size,
    int search_window_size,
    int norm_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_fast_nl_means_denoising_colored(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float h,
    float h_color,
    int template_window_size,
    int search_window_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_fast_nl_means_denoising_multi(
    const jyppx_ocv_mat* const* src_images,
    int image_count,
    jyppx_ocv_mat* dst,
    int img_to_denoise_index,
    int temporal_window_size,
    float h,
    int template_window_size,
    int search_window_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_fast_nl_means_denoising_multi_with_h_array(
    const jyppx_ocv_mat* const* src_images,
    int image_count,
    jyppx_ocv_mat* dst,
    int img_to_denoise_index,
    int temporal_window_size,
    const float* h,
    int h_count,
    int template_window_size,
    int search_window_size,
    int norm_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_fast_nl_means_denoising_colored_multi(
    const jyppx_ocv_mat* const* src_images,
    int image_count,
    jyppx_ocv_mat* dst,
    int img_to_denoise_index,
    int temporal_window_size,
    float h,
    float h_color,
    int template_window_size,
    int search_window_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_seamless_clone(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask,
    int point_x,
    int point_y,
    jyppx_ocv_mat* blend,
    int flags);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_color_change(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_mat* dst,
    float red_mul,
    float green_mul,
    float blue_mul);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_illumination_change(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_mat* dst,
    float alpha,
    float beta);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_texture_flattening(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_mat* dst,
    float low_threshold,
    float high_threshold,
    int kernel_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_edge_preserving_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int flags,
    float sigma_s,
    float sigma_r);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_detail_enhance(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float sigma_s,
    float sigma_r);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_pencil_sketch(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst1,
    jyppx_ocv_mat* dst2,
    float sigma_s,
    float sigma_r,
    float shade_factor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_stylization(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float sigma_s,
    float sigma_r);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_create(
    float gamma,
    jyppx_ocv_tonemap** tonemap);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_drago_create(
    float gamma,
    float saturation,
    float bias,
    jyppx_ocv_tonemap** tonemap);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_reinhard_create(
    float gamma,
    float intensity,
    float light_adapt,
    float color_adapt,
    jyppx_ocv_tonemap** tonemap);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_mantiuk_create(
    float gamma,
    float scale,
    float saturation,
    jyppx_ocv_tonemap** tonemap);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_tonemap_release_handle(
    jyppx_ocv_tonemap* tonemap);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_process(
    jyppx_ocv_tonemap* tonemap,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_get_gamma(
    const jyppx_ocv_tonemap* tonemap,
    float* gamma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_set_gamma(
    jyppx_ocv_tonemap* tonemap,
    float gamma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_drago_get_saturation(
    const jyppx_ocv_tonemap* tonemap,
    float* saturation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_drago_set_saturation(
    jyppx_ocv_tonemap* tonemap,
    float saturation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_drago_get_bias(
    const jyppx_ocv_tonemap* tonemap,
    float* bias);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_drago_set_bias(
    jyppx_ocv_tonemap* tonemap,
    float bias);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_reinhard_get_intensity(
    const jyppx_ocv_tonemap* tonemap,
    float* intensity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_reinhard_set_intensity(
    jyppx_ocv_tonemap* tonemap,
    float intensity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_reinhard_get_light_adaptation(
    const jyppx_ocv_tonemap* tonemap,
    float* light_adapt);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_reinhard_set_light_adaptation(
    jyppx_ocv_tonemap* tonemap,
    float light_adapt);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_reinhard_get_color_adaptation(
    const jyppx_ocv_tonemap* tonemap,
    float* color_adapt);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_reinhard_set_color_adaptation(
    jyppx_ocv_tonemap* tonemap,
    float color_adapt);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_mantiuk_get_scale(
    const jyppx_ocv_tonemap* tonemap,
    float* scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_mantiuk_set_scale(
    jyppx_ocv_tonemap* tonemap,
    float scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_mantiuk_get_saturation(
    const jyppx_ocv_tonemap* tonemap,
    float* saturation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_tonemap_mantiuk_set_saturation(
    jyppx_ocv_tonemap* tonemap,
    float saturation);
