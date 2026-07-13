#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_dnn_net jyppx_ocv_dnn_net;

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
