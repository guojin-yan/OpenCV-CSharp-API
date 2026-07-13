#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_bgsegm_background_subtractor jyppx_ocv_bgsegm_background_subtractor;
typedef struct jyppx_ocv_bgsegm_background_subtractor_mog jyppx_ocv_bgsegm_background_subtractor_mog;
typedef struct jyppx_ocv_bgsegm_background_subtractor_gmg jyppx_ocv_bgsegm_background_subtractor_gmg;
typedef struct jyppx_ocv_bgsegm_background_subtractor_cnt jyppx_ocv_bgsegm_background_subtractor_cnt;
typedef struct jyppx_ocv_bgsegm_synthetic_sequence_generator jyppx_ocv_bgsegm_synthetic_sequence_generator;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_bgsegm_background_subtractor_release_handle(
    jyppx_ocv_bgsegm_background_subtractor* subtractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_apply(
    jyppx_ocv_bgsegm_background_subtractor* subtractor,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* fgmask,
    double learning_rate);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_apply_with_known_foreground(
    jyppx_ocv_bgsegm_background_subtractor* subtractor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* known_foreground_mask,
    jyppx_ocv_mat* fgmask,
    double learning_rate);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_get_background_image(
    const jyppx_ocv_bgsegm_background_subtractor* subtractor,
    jyppx_ocv_mat* background_image);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_mog_create(
    int history,
    int nmixtures,
    double background_ratio,
    double noise_sigma,
    jyppx_ocv_bgsegm_background_subtractor_mog** subtractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_mog_get_int(
    const jyppx_ocv_bgsegm_background_subtractor_mog* subtractor,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_mog_set_int(
    jyppx_ocv_bgsegm_background_subtractor_mog* subtractor,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_mog_get_double(
    const jyppx_ocv_bgsegm_background_subtractor_mog* subtractor,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_mog_set_double(
    jyppx_ocv_bgsegm_background_subtractor_mog* subtractor,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_gmg_create(
    int initialization_frames,
    double decision_threshold,
    jyppx_ocv_bgsegm_background_subtractor_gmg** subtractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_gmg_get_int(
    const jyppx_ocv_bgsegm_background_subtractor_gmg* subtractor,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_gmg_set_int(
    jyppx_ocv_bgsegm_background_subtractor_gmg* subtractor,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_gmg_get_double(
    const jyppx_ocv_bgsegm_background_subtractor_gmg* subtractor,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_gmg_set_double(
    jyppx_ocv_bgsegm_background_subtractor_gmg* subtractor,
    int property_id,
    double value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_cnt_create(
    int min_pixel_stability,
    int use_history,
    int max_pixel_stability,
    int is_parallel,
    jyppx_ocv_bgsegm_background_subtractor_cnt** subtractor);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_cnt_get_int(
    const jyppx_ocv_bgsegm_background_subtractor_cnt* subtractor,
    int property_id,
    int* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_background_subtractor_cnt_set_int(
    jyppx_ocv_bgsegm_background_subtractor_cnt* subtractor,
    int property_id,
    int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_synthetic_sequence_generator_create(
    const jyppx_ocv_mat* background,
    const jyppx_ocv_mat* object,
    double amplitude,
    double wavelength,
    double wavespeed,
    double objspeed,
    jyppx_ocv_bgsegm_synthetic_sequence_generator** generator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_bgsegm_synthetic_sequence_generator_release_handle(
    jyppx_ocv_bgsegm_synthetic_sequence_generator* generator);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bgsegm_synthetic_sequence_generator_get_next_frame(
    jyppx_ocv_bgsegm_synthetic_sequence_generator* generator,
    jyppx_ocv_mat* frame,
    jyppx_ocv_mat* gt_mask);
