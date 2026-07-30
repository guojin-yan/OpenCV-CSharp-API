#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/core/persistence.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

#include <stdint.h>

typedef struct jyppx_ocv_dnn_net jyppx_ocv_dnn_net;
typedef struct jyppx_ocv_dnn_layer jyppx_ocv_dnn_layer;
typedef struct jyppx_ocv_dnn_mat_groups jyppx_ocv_dnn_mat_groups;

typedef struct jyppx_ocv_dnn_rect
{
    int x;
    int y;
    int width;
    int height;
} jyppx_ocv_dnn_rect;

typedef struct jyppx_ocv_dnn_image2blob_params
{
    double scale_v0;
    double scale_v1;
    double scale_v2;
    double scale_v3;
    int size_width;
    int size_height;
    double mean_v0;
    double mean_v1;
    double mean_v2;
    double mean_v3;
    int swap_rb;
    int ddepth;
    int data_layout;
    int padding_mode;
    double border_v0;
    double border_v1;
    double border_v2;
    double border_v3;
} jyppx_ocv_dnn_image2blob_params;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_create_empty(jyppx_ocv_dnn_net** net);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net(
    const char* model,
    const char* config,
    const char* framework,
    int engine,
    jyppx_ocv_dnn_net** net);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net_from_buffer(
    const char* framework,
    const unsigned char* model_buffer,
    int model_buffer_size,
    const unsigned char* config_buffer,
    int config_buffer_size,
    int engine,
    jyppx_ocv_dnn_net** net);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net_from_onnx(
    const char* model,
    int engine,
    jyppx_ocv_dnn_net** net);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net_from_tensorflow(
    const char* model,
    const char* config,
    int engine,
    jyppx_ocv_dnn_net** net);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net_from_tflite(
    const char* model,
    int engine,
    jyppx_ocv_dnn_net** net);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net_from_model_optimizer(
    const char* xml,
    const char* bin,
    jyppx_ocv_dnn_net** net);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_dnn_net_release_handle(jyppx_ocv_dnn_net* net);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_empty(const jyppx_ocv_dnn_net* net, int* empty);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_set_preferable_backend(jyppx_ocv_dnn_net* net, int backend_id);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_set_preferable_target(jyppx_ocv_dnn_net* net, int target_id);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_set_input(
    jyppx_ocv_dnn_net* net,
    const jyppx_ocv_mat* blob,
    const char* name,
    double scale_factor,
    double mean_v0,
    double mean_v1,
    double mean_v2,
    double mean_v3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_forward(
    jyppx_ocv_dnn_net* net,
    const char* output_name,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_forward_many(
    jyppx_ocv_dnn_net* net,
    const char* names_buffer,
    const int* name_offsets,
    int name_count,
    jyppx_ocv_mat** outputs,
    int output_capacity,
    int* output_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layer_id(
    const jyppx_ocv_dnn_net* net,
    const char* layer_name,
    int* layer_id);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_unconnected_out_layers_count(
    const jyppx_ocv_dnn_net* net,
    int* layer_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_unconnected_out_layers_fill(
    const jyppx_ocv_dnn_net* net,
    int* layers,
    int layer_capacity,
    int* layer_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_set_inputs_names(
    jyppx_ocv_dnn_net* net,
    const char* names_buffer,
    const int* name_offsets,
    int name_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_set_input_shape(
    jyppx_ocv_dnn_net* net,
    const char* input_name,
    const int* shape,
    int shape_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_flops(
    const jyppx_ocv_dnn_net* net,
    const int* input_shape,
    int input_shape_count,
    int input_type,
    long long* flops);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layer_flops(
    const jyppx_ocv_dnn_net* net,
    int layer_id,
    const int* input_shape,
    int input_shape_count,
    int input_type,
    long long* flops);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_perf_profile_count(
    jyppx_ocv_dnn_net* net,
    int* timing_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_perf_profile_fill(
    jyppx_ocv_dnn_net* net,
    double* timings,
    int timing_capacity,
    int* timing_count,
    long long* tick_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layer_names_count(
    const jyppx_ocv_dnn_net* net,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layer_names_fill(
    const jyppx_ocv_dnn_net* net,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_unconnected_out_layers_names_count(
    const jyppx_ocv_dnn_net* net,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_unconnected_out_layers_names_fill(
    const jyppx_ocv_dnn_net* net,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layer_types_count(
    const jyppx_ocv_dnn_net* net,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layer_types_fill(
    const jyppx_ocv_dnn_net* net,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* string_count,
    int* byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layers_count_by_type(
    const jyppx_ocv_dnn_net* net,
    const char* layer_type,
    int* layer_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_blob_from_image(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* blob,
    double scale_factor,
    int size_width,
    int size_height,
    double mean_v0,
    double mean_v1,
    double mean_v2,
    double mean_v3,
    int swap_rb,
    int crop,
    int ddepth);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_blob_from_images(
    const jyppx_ocv_mat* const* images,
    int image_count,
    jyppx_ocv_mat* blob,
    double scale_factor,
    int size_width,
    int size_height,
    double mean_v0,
    double mean_v1,
    double mean_v2,
    double mean_v3,
    int swap_rb,
    int crop,
    int ddepth);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_images_from_blob_count(
    const jyppx_ocv_mat* blob,
    int* image_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_images_from_blob_fill(
    const jyppx_ocv_mat* blob,
    jyppx_ocv_mat** images,
    int image_capacity,
    int* image_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_get_available_targets_count(
    int backend_id,
    int* target_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_get_available_targets_fill(
    int backend_id,
    int* targets,
    int target_capacity,
    int* target_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net_from_tensorflow_ex(
    const char* model,
    const char* config,
    int engine,
    const char* extra_outputs_buffer,
    const int* extra_output_offsets,
    int extra_output_count,
    jyppx_ocv_dnn_net** net);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net_from_tensorflow_buffer(
    const unsigned char* model_buffer,
    int model_buffer_size,
    const unsigned char* config_buffer,
    int config_buffer_size,
    int engine,
    const char* extra_outputs_buffer,
    const int* extra_output_offsets,
    int extra_output_count,
    jyppx_ocv_dnn_net** net);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net_from_tflite_buffer(
    const unsigned char* model_buffer,
    int model_buffer_size,
    int engine,
    jyppx_ocv_dnn_net** net);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net_from_model_optimizer_buffer(
    const unsigned char* model_config_buffer,
    int model_config_buffer_size,
    const unsigned char* weights_buffer,
    int weights_buffer_size,
    jyppx_ocv_dnn_net** net);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_net_from_onnx_buffer(
    const unsigned char* model_buffer,
    int model_buffer_size,
    int engine,
    jyppx_ocv_dnn_net** net);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_read_tensor_from_onnx(
    const char* path,
    jyppx_ocv_mat* output);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_dump(
    jyppx_ocv_dnn_net* net,
    jyppx_ocv_core_utf8_result** result);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_dump_to_file(
    jyppx_ocv_dnn_net* net,
    const char* path);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_dump_to_pbtxt(
    jyppx_ocv_dnn_net* net,
    const char* path);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_connect(
    jyppx_ocv_dnn_net* net,
    const char* output_pin,
    const char* input_pin);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_register_output(
    jyppx_ocv_dnn_net* net,
    const char* output_name,
    int layer_id,
    int output_port,
    int* registered_layer_id);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layer_by_id(
    const jyppx_ocv_dnn_net* net,
    int layer_id,
    jyppx_ocv_dnn_layer** layer);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layer_by_name(
    const jyppx_ocv_dnn_net* net,
    const char* layer_name,
    jyppx_ocv_dnn_layer** layer);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_dnn_layer_release_handle(
    jyppx_ocv_dnn_layer* layer);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_layer_output_name_to_index(
    jyppx_ocv_dnn_layer* layer,
    const char* output_name,
    int* output_index);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_forward_and_retrieve(
    jyppx_ocv_dnn_net* net,
    const char* names_buffer,
    const int* name_offsets,
    int name_count,
    jyppx_ocv_dnn_mat_groups** result);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_mat_groups_get_counts(
    const jyppx_ocv_dnn_mat_groups* result,
    int* group_count,
    int* mat_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_mat_groups_get_group_offsets(
    const jyppx_ocv_dnn_mat_groups* result,
    int* offsets,
    int offset_capacity,
    int* group_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_mat_groups_take_mats(
    const jyppx_ocv_dnn_mat_groups* result,
    jyppx_ocv_mat** mats,
    int mat_capacity,
    int* mat_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_dnn_mat_groups_release_handle(
    jyppx_ocv_dnn_mat_groups* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_finalize(jyppx_ocv_dnn_net* net);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_set_tracing_mode(jyppx_ocv_dnn_net* net, int mode);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_tracing_mode(const jyppx_ocv_dnn_net* net, int* mode);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_set_profiling_mode(jyppx_ocv_dnn_net* net, int mode);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_profiling_mode(const jyppx_ocv_dnn_net* net, int* mode);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_model_format(const jyppx_ocv_dnn_net* net, int* format);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_set_param_by_id(
    jyppx_ocv_dnn_net* net,
    int layer_id,
    int parameter_index,
    const jyppx_ocv_mat* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_set_param_by_name(
    jyppx_ocv_dnn_net* net,
    const char* layer_name,
    int parameter_index,
    const jyppx_ocv_mat* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_param_by_id(
    const jyppx_ocv_dnn_net* net,
    int layer_id,
    int parameter_index,
    jyppx_ocv_mat* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_param_by_name(
    const jyppx_ocv_dnn_net* net,
    const char* layer_name,
    int parameter_index,
    jyppx_ocv_mat* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layer_shapes_count(
    const jyppx_ocv_dnn_net* net,
    const int* input_shape_offsets,
    int input_shape_count,
    const int* input_shape_values,
    int input_value_count,
    const int* input_types,
    int input_type_count,
    int layer_id,
    int* input_layer_shape_count,
    int* input_layer_value_count,
    int* output_layer_shape_count,
    int* output_layer_value_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_layer_shapes_fill(
    const jyppx_ocv_dnn_net* net,
    const int* input_shape_offsets,
    int input_shape_count,
    const int* input_shape_values,
    int input_value_count,
    const int* input_types,
    int input_type_count,
    int layer_id,
    int* input_layer_offsets,
    int input_layer_offset_capacity,
    int* input_layer_values,
    int input_layer_value_capacity,
    int* output_layer_offsets,
    int output_layer_offset_capacity,
    int* output_layer_values,
    int output_layer_value_capacity,
    int* input_layer_shape_count,
    int* input_layer_value_count,
    int* output_layer_shape_count,
    int* output_layer_value_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_flops_many(
    const jyppx_ocv_dnn_net* net,
    const int* input_shape_offsets,
    int input_shape_count,
    const int* input_shape_values,
    int input_value_count,
    const int* input_types,
    int input_type_count,
    int64_t* flops);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_memory_consumption(
    const jyppx_ocv_dnn_net* net,
    const int* input_shape_offsets,
    int input_shape_count,
    const int* input_shape_values,
    int input_value_count,
    const int* input_types,
    int input_type_count,
    uint64_t* weights_bytes,
    uint64_t* blob_bytes);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_enable_fusion(jyppx_ocv_dnn_net* net, int enabled);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_enable_winograd(jyppx_ocv_dnn_net* net, int enabled);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_enable_kv_cache(jyppx_ocv_dnn_net* net);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_disable_kv_cache(jyppx_ocv_dnn_net* net);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_reset_kv_cache(jyppx_ocv_dnn_net* net);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_detailed_perf_profile_count(
    const jyppx_ocv_dnn_net* net,
    int* row_count,
    int* name_byte_count,
    int* time_byte_count,
    int* invocation_byte_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_net_get_detailed_perf_profile_fill(
    const jyppx_ocv_dnn_net* net,
    int* name_offsets,
    int name_offset_capacity,
    char* names,
    int name_capacity,
    int* time_offsets,
    int time_offset_capacity,
    char* times,
    int time_capacity,
    int* invocation_offsets,
    int invocation_offset_capacity,
    char* invocations,
    int invocation_capacity,
    int* row_count,
    int* name_byte_count,
    int* time_byte_count,
    int* invocation_byte_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_blob_from_image_with_params(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* blob,
    const jyppx_ocv_dnn_image2blob_params* parameters);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_blob_from_images_with_params(
    const jyppx_ocv_mat* const* images,
    int image_count,
    jyppx_ocv_mat* blob,
    const jyppx_ocv_dnn_image2blob_params* parameters);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_blob_rect_to_image_rect(
    const jyppx_ocv_dnn_image2blob_params* parameters,
    const jyppx_ocv_dnn_rect* blob_rect,
    int image_width,
    int image_height,
    jyppx_ocv_dnn_rect* image_rect);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_dnn_blob_rects_to_image_rects(
    const jyppx_ocv_dnn_image2blob_params* parameters,
    const jyppx_ocv_dnn_rect* blob_rects,
    int blob_rect_count,
    int image_width,
    int image_height,
    jyppx_ocv_dnn_rect* image_rects,
    int image_rect_capacity,
    int* image_rect_count);
