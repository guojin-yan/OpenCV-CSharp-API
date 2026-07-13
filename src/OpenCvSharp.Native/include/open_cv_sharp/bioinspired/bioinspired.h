#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_bioinspired_retina jyppx_ocv_bioinspired_retina;
typedef struct jyppx_ocv_bioinspired_retina_fast_tone_mapping jyppx_ocv_bioinspired_retina_fast_tone_mapping;
typedef struct jyppx_ocv_bioinspired_transient_areas_segmentation_module jyppx_ocv_bioinspired_transient_areas_segmentation_module;

typedef struct jyppx_ocv_bioinspired_retina_parvo_parameters
{
    int color_mode;
    int normalise_output;
    float photoreceptors_local_adaptation_sensitivity;
    float photoreceptors_temporal_constant;
    float photoreceptors_spatial_constant;
    float horizontal_cells_gain;
    float hcells_temporal_constant;
    float hcells_spatial_constant;
    float ganglion_cells_sensitivity;
} jyppx_ocv_bioinspired_retina_parvo_parameters;

typedef struct jyppx_ocv_bioinspired_retina_magno_parameters
{
    int normalise_output;
    float parasol_cells_beta;
    float parasol_cells_tau;
    float parasol_cells_k;
    float amacrin_cells_temporal_cut_frequency;
    float v0_compression_parameter;
    float local_adapt_integration_tau;
    float local_adapt_integration_k;
} jyppx_ocv_bioinspired_retina_magno_parameters;

typedef struct jyppx_ocv_bioinspired_segmentation_parameters
{
    float threshold_on;
    float threshold_off;
    float local_energy_temporal_constant;
    float local_energy_spatial_constant;
    float neighborhood_energy_temporal_constant;
    float neighborhood_energy_spatial_constant;
    float context_energy_temporal_constant;
    float context_energy_spatial_constant;
} jyppx_ocv_bioinspired_segmentation_parameters;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_create(
    int width,
    int height,
    int color_mode,
    int color_sampling_method,
    int use_retina_log_sampling,
    float reduction_factor,
    float sampling_strength,
    jyppx_ocv_bioinspired_retina** retina);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_bioinspired_retina_release(
    jyppx_ocv_bioinspired_retina* retina);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_get_input_size(
    const jyppx_ocv_bioinspired_retina* retina,
    int* width,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_get_output_size(
    const jyppx_ocv_bioinspired_retina* retina,
    int* width,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_setup(
    jyppx_ocv_bioinspired_retina* retina,
    const unsigned char* retina_parameter_file,
    int apply_default_setup_on_failure);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_setup_parvo(
    jyppx_ocv_bioinspired_retina* retina,
    const jyppx_ocv_bioinspired_retina_parvo_parameters* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_setup_magno(
    jyppx_ocv_bioinspired_retina* retina,
    const jyppx_ocv_bioinspired_retina_magno_parameters* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_get_parameters(
    const jyppx_ocv_bioinspired_retina* retina,
    jyppx_ocv_bioinspired_retina_parvo_parameters* parvo,
    jyppx_ocv_bioinspired_retina_magno_parameters* magno);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_run(
    jyppx_ocv_bioinspired_retina* retina,
    const jyppx_ocv_mat* input);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_apply_fast_tone_mapping(
    jyppx_ocv_bioinspired_retina* retina,
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_get_parvo(
    jyppx_ocv_bioinspired_retina* retina,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_get_parvo_raw(
    jyppx_ocv_bioinspired_retina* retina,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_get_magno(
    jyppx_ocv_bioinspired_retina* retina,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_get_magno_raw(
    jyppx_ocv_bioinspired_retina* retina,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_set_color_saturation(
    jyppx_ocv_bioinspired_retina* retina,
    int saturate_colors,
    float color_saturation_value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_clear_buffers(
    jyppx_ocv_bioinspired_retina* retina);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_activate_moving_contours_processing(
    jyppx_ocv_bioinspired_retina* retina,
    int activate);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_activate_contours_processing(
    jyppx_ocv_bioinspired_retina* retina,
    int activate);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_print_setup_length(
    jyppx_ocv_bioinspired_retina* retina,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_print_setup_fill(
    jyppx_ocv_bioinspired_retina* retina,
    unsigned char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_write(
    const jyppx_ocv_bioinspired_retina* retina,
    const unsigned char* path);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_fast_tone_mapping_create(
    int width,
    int height,
    jyppx_ocv_bioinspired_retina_fast_tone_mapping** tone_mapping);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_bioinspired_retina_fast_tone_mapping_release(
    jyppx_ocv_bioinspired_retina_fast_tone_mapping* tone_mapping);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_fast_tone_mapping_setup(
    jyppx_ocv_bioinspired_retina_fast_tone_mapping* tone_mapping,
    float photoreceptors_neighborhood_radius,
    float ganglion_cells_neighborhood_radius,
    float mean_luminance_modulator_k);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_retina_fast_tone_mapping_apply(
    jyppx_ocv_bioinspired_retina_fast_tone_mapping* tone_mapping,
    const jyppx_ocv_mat* input,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_create(
    int width,
    int height,
    jyppx_ocv_bioinspired_transient_areas_segmentation_module** segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_bioinspired_transient_areas_release(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_get_size(
    const jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    int* width,
    int* height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_setup(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    const unsigned char* segmentation_parameter_file,
    int apply_default_setup_on_failure);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_setup_parameters(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    const jyppx_ocv_bioinspired_segmentation_parameters* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_get_parameters(
    const jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    jyppx_ocv_bioinspired_segmentation_parameters* parameters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_run(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    const jyppx_ocv_mat* input,
    int channel_index);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_get_segmentation_picture(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_clear_all_buffers(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_print_setup_length(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_print_setup_fill(
    jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    unsigned char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_bioinspired_transient_areas_write(
    const jyppx_ocv_bioinspired_transient_areas_segmentation_module* segmentation,
    const unsigned char* path);
