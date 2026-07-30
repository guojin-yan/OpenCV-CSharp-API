#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/core/persistence.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_tonemap jyppx_ocv_tonemap;
typedef struct jyppx_ocv_align_mtb jyppx_ocv_align_mtb;
typedef struct jyppx_ocv_calibrate_crf jyppx_ocv_calibrate_crf;
typedef struct jyppx_ocv_merge_exposures jyppx_ocv_merge_exposures;
typedef struct jyppx_ocv_color_correction_model jyppx_ocv_color_correction_model;
typedef struct jyppx_ocv_intelligent_scissors_mb jyppx_ocv_intelligent_scissors_mb;

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

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_denoise_tvl1(
    const jyppx_ocv_mat* const* observations,
    int observation_count,
    jyppx_ocv_mat* result,
    double lambda,
    int niters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_correct_chromatic_aberration(
    const jyppx_ocv_mat* input_image,
    const jyppx_ocv_mat* coefficients,
    jyppx_ocv_mat* output_image,
    int calibration_width,
    int calibration_height,
    int calibration_degree,
    int bayer_pattern);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_load_chromatic_aberration_params(
    const jyppx_ocv_core_file_node* node,
    jyppx_ocv_mat* coefficients,
    int* calibration_width,
    int* calibration_height,
    int* degree);

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

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_create(
    int max_bits,
    int exclude_range,
    int cut,
    jyppx_ocv_align_mtb** aligner);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_align_mtb_release_handle(
    jyppx_ocv_align_mtb* aligner);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_process(
    jyppx_ocv_align_mtb* aligner,
    const jyppx_ocv_mat* const* src_images,
    jyppx_ocv_mat* const* dst_images,
    int image_count,
    const jyppx_ocv_mat* times,
    const jyppx_ocv_mat* response,
    int use_extra_inputs);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_calculate_shift(
    jyppx_ocv_align_mtb* aligner,
    const jyppx_ocv_mat* img0,
    const jyppx_ocv_mat* img1,
    int* shift_x,
    int* shift_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_shift_mat(
    jyppx_ocv_align_mtb* aligner,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int shift_x,
    int shift_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_compute_bitmaps(
    jyppx_ocv_align_mtb* aligner,
    const jyppx_ocv_mat* img,
    jyppx_ocv_mat* threshold_bitmap,
    jyppx_ocv_mat* exclude_bitmap);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_get_max_bits(
    const jyppx_ocv_align_mtb* aligner,
    int* max_bits);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_set_max_bits(
    jyppx_ocv_align_mtb* aligner,
    int max_bits);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_get_exclude_range(
    const jyppx_ocv_align_mtb* aligner,
    int* exclude_range);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_set_exclude_range(
    jyppx_ocv_align_mtb* aligner,
    int exclude_range);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_get_cut(
    const jyppx_ocv_align_mtb* aligner,
    int* cut);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_align_mtb_set_cut(
    jyppx_ocv_align_mtb* aligner,
    int cut);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_debevec_create(
    int samples,
    float lambda,
    int random,
    jyppx_ocv_calibrate_crf** calibrator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_robertson_create(
    int max_iter,
    float threshold,
    jyppx_ocv_calibrate_crf** calibrator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_calibrate_crf_release_handle(
    jyppx_ocv_calibrate_crf* calibrator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_crf_process(
    jyppx_ocv_calibrate_crf* calibrator,
    const jyppx_ocv_mat* const* src_images,
    int image_count,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* times);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_debevec_get_lambda(
    const jyppx_ocv_calibrate_crf* calibrator,
    float* lambda);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_debevec_set_lambda(
    jyppx_ocv_calibrate_crf* calibrator,
    float lambda);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_debevec_get_samples(
    const jyppx_ocv_calibrate_crf* calibrator,
    int* samples);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_debevec_set_samples(
    jyppx_ocv_calibrate_crf* calibrator,
    int samples);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_debevec_get_random(
    const jyppx_ocv_calibrate_crf* calibrator,
    int* random);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_debevec_set_random(
    jyppx_ocv_calibrate_crf* calibrator,
    int random);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_robertson_get_max_iter(
    const jyppx_ocv_calibrate_crf* calibrator,
    int* max_iter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_robertson_set_max_iter(
    jyppx_ocv_calibrate_crf* calibrator,
    int max_iter);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_robertson_get_threshold(
    const jyppx_ocv_calibrate_crf* calibrator,
    float* threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_robertson_set_threshold(
    jyppx_ocv_calibrate_crf* calibrator,
    float threshold);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calibrate_robertson_get_radiance(
    const jyppx_ocv_calibrate_crf* calibrator,
    jyppx_ocv_mat* radiance);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_merge_debevec_create(
    jyppx_ocv_merge_exposures** merger);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_merge_mertens_create(
    float contrast_weight,
    float saturation_weight,
    float exposure_weight,
    jyppx_ocv_merge_exposures** merger);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_merge_robertson_create(
    jyppx_ocv_merge_exposures** merger);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_merge_exposures_release_handle(
    jyppx_ocv_merge_exposures* merger);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_merge_exposures_process(
    jyppx_ocv_merge_exposures* merger,
    const jyppx_ocv_mat* const* src_images,
    int image_count,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* times,
    const jyppx_ocv_mat* response,
    int input_mode);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_merge_mertens_get_contrast_weight(
    const jyppx_ocv_merge_exposures* merger,
    float* contrast_weight);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_merge_mertens_set_contrast_weight(
    jyppx_ocv_merge_exposures* merger,
    float contrast_weight);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_merge_mertens_get_saturation_weight(
    const jyppx_ocv_merge_exposures* merger,
    float* saturation_weight);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_merge_mertens_set_saturation_weight(
    jyppx_ocv_merge_exposures* merger,
    float saturation_weight);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_merge_mertens_get_exposure_weight(
    const jyppx_ocv_merge_exposures* merger,
    float* exposure_weight);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_merge_mertens_set_exposure_weight(
    jyppx_ocv_merge_exposures* merger,
    float exposure_weight);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_gamma_correction(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double gamma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_create(
    jyppx_ocv_color_correction_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_create_color_checker(
    const jyppx_ocv_mat* src,
    int color_checker,
    jyppx_ocv_color_correction_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_create_reference_colors(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* colors,
    int reference_color_space,
    jyppx_ocv_color_correction_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_create_reference_colors_masked(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* colors,
    int reference_color_space,
    const jyppx_ocv_mat* colored_patches_mask,
    jyppx_ocv_color_correction_model** model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_photo_ccm_release_handle(
    jyppx_ocv_color_correction_model* model);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_color_space(
    jyppx_ocv_color_correction_model* model,
    int color_space);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_ccm_type(
    jyppx_ocv_color_correction_model* model,
    int ccm_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_distance(
    jyppx_ocv_color_correction_model* model,
    int distance);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_linearization(
    jyppx_ocv_color_correction_model* model,
    int linearization);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_linearization_gamma(
    jyppx_ocv_color_correction_model* model,
    double gamma);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_linearization_degree(
    jyppx_ocv_color_correction_model* model,
    int degree);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_saturated_threshold(
    jyppx_ocv_color_correction_model* model,
    double lower,
    double upper);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_weights_list(
    jyppx_ocv_color_correction_model* model,
    const jyppx_ocv_mat* weights_list);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_weight_coeff(
    jyppx_ocv_color_correction_model* model,
    double weight_coeff);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_initial_method(
    jyppx_ocv_color_correction_model* model,
    int initial_method);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_max_count(
    jyppx_ocv_color_correction_model* model,
    int max_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_epsilon(
    jyppx_ocv_color_correction_model* model,
    double epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_set_rgb(
    jyppx_ocv_color_correction_model* model,
    int rgb);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_compute(
    jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* color_correction_matrix);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_get_color_correction_matrix(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* color_correction_matrix);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_get_loss(
    const jyppx_ocv_color_correction_model* model,
    double* loss);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_get_src_linear_rgb(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* src_linear_rgb);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_get_ref_linear_rgb(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* ref_linear_rgb);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_get_mask(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_get_weights(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* weights);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_correct_image(
    const jyppx_ocv_color_correction_model* model,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int is_linear);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_write(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_core_file_storage* storage);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_ccm_read(
    jyppx_ocv_color_correction_model* model,
    const jyppx_ocv_core_file_node* node);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_intelligent_scissors_create(
    jyppx_ocv_intelligent_scissors_mb** scissors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_photo_intelligent_scissors_release_handle(
    jyppx_ocv_intelligent_scissors_mb* scissors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_intelligent_scissors_set_weights(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    float weight_non_edge,
    float weight_gradient_direction,
    float weight_gradient_magnitude);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_intelligent_scissors_set_gradient_magnitude_max_limit(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    float gradient_magnitude_threshold_max);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_intelligent_scissors_set_edge_feature_zero_crossing_parameters(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    float gradient_magnitude_min_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_intelligent_scissors_set_edge_feature_canny_parameters(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    double threshold1,
    double threshold2,
    int aperture_size,
    int l2_gradient);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_intelligent_scissors_apply_image(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    const jyppx_ocv_mat* image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_intelligent_scissors_apply_image_features(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    const jyppx_ocv_mat* non_edge,
    const jyppx_ocv_mat* gradient_direction,
    const jyppx_ocv_mat* gradient_magnitude,
    const jyppx_ocv_mat* image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_intelligent_scissors_build_map(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    int source_x,
    int source_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_photo_intelligent_scissors_get_contour(
    const jyppx_ocv_intelligent_scissors_mb* scissors,
    int target_x,
    int target_y,
    jyppx_ocv_mat* contour,
    int backward);
