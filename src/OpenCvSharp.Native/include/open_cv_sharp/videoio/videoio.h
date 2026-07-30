#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_video_capture jyppx_ocv_video_capture;
typedef struct jyppx_ocv_video_writer jyppx_ocv_video_writer;
typedef struct jyppx_ocv_video_stream_reader jyppx_ocv_video_stream_reader;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_create(
    jyppx_ocv_video_capture** capture);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_video_capture_release_handle(
    jyppx_ocv_video_capture* capture);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_open_file(
    jyppx_ocv_video_capture* capture,
    const char* filename,
    int api_preference,
    int* opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_open_index(
    jyppx_ocv_video_capture* capture,
    int index,
    int api_preference,
    int* opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_open_file_params(
    jyppx_ocv_video_capture* capture,
    const char* filename,
    int api_preference,
    const int* parameters,
    int parameter_count,
    int* opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_open_index_params(
    jyppx_ocv_video_capture* capture,
    int index,
    int api_preference,
    const int* parameters,
    int parameter_count,
    int* opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_open_stream(
    jyppx_ocv_video_capture* capture,
    jyppx_ocv_video_stream_reader* reader,
    int api_preference,
    const int* parameters,
    int parameter_count,
    int* opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_is_opened(
    const jyppx_ocv_video_capture* capture,
    int* opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_release(
    jyppx_ocv_video_capture* capture);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_grab(
    jyppx_ocv_video_capture* capture,
    int* grabbed);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_retrieve(
    jyppx_ocv_video_capture* capture,
    jyppx_ocv_mat* image,
    int flag,
    int* retrieved);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_read(
    jyppx_ocv_video_capture* capture,
    jyppx_ocv_mat* image,
    int* read);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_get(
    const jyppx_ocv_video_capture* capture,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_set(
    jyppx_ocv_video_capture* capture,
    int property_id,
    double value,
    int* success);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_set_exception_mode(
    jyppx_ocv_video_capture* capture,
    int enabled);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_get_exception_mode(
    const jyppx_ocv_video_capture* capture,
    int* enabled);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_wait_any(
    jyppx_ocv_video_capture* const* captures,
    int capture_count,
    int* ready_indices,
    int ready_capacity,
    long long timeout_ns,
    int* ready_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_backend_name_length(
    const jyppx_ocv_video_capture* capture,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_capture_backend_name_fill(
    const jyppx_ocv_video_capture* capture,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_create(
    jyppx_ocv_video_writer** writer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_video_writer_release_handle(
    jyppx_ocv_video_writer* writer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_open(
    jyppx_ocv_video_writer* writer,
    const char* filename,
    int api_preference,
    int fourcc,
    double fps,
    int frame_width,
    int frame_height,
    int is_color,
    int* opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_open_params(
    jyppx_ocv_video_writer* writer,
    const char* filename,
    int fourcc,
    double fps,
    int frame_width,
    int frame_height,
    const int* parameters,
    int parameter_count,
    int* opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_open_api_params(
    jyppx_ocv_video_writer* writer,
    const char* filename,
    int api_preference,
    int fourcc,
    double fps,
    int frame_width,
    int frame_height,
    const int* parameters,
    int parameter_count,
    int* opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_is_opened(
    const jyppx_ocv_video_writer* writer,
    int* opened);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_release(
    jyppx_ocv_video_writer* writer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_write(
    jyppx_ocv_video_writer* writer,
    const jyppx_ocv_mat* image,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_get(
    const jyppx_ocv_video_writer* writer,
    int property_id,
    double* value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_set(
    jyppx_ocv_video_writer* writer,
    int property_id,
    double value,
    int* success);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_backend_name_length(
    const jyppx_ocv_video_writer* writer,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_backend_name_fill(
    const jyppx_ocv_video_writer* writer,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_writer_fourcc(
    int c1,
    int c2,
    int c3,
    int c4,
    int* fourcc);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_backends_count(
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_backends_fill(
    int* backends,
    int backend_capacity,
    int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_camera_backends_count(int* count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_camera_backends_fill(int* backends, int backend_capacity, int* count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_stream_backends_count(int* count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_stream_backends_fill(int* backends, int backend_capacity, int* count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_stream_buffered_backends_count(int* count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_stream_buffered_backends_fill(int* backends, int backend_capacity, int* count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_writer_backends_count(int* count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_writer_backends_fill(int* backends, int backend_capacity, int* count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_backend_name_length(
    int api,
    int* length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_backend_name_fill(
    int api,
    char* buffer,
    int buffer_capacity,
    int* written);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_has_backend(
    int api,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_is_backend_built_in(
    int api,
    int* result);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_camera_plugin_version_length(int api, int* version_abi, int* version_api, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_camera_plugin_version_fill(int api, int* version_abi, int* version_api, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_stream_plugin_version_length(int api, int* version_abi, int* version_api, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_stream_plugin_version_fill(int api, int* version_abi, int* version_api, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_stream_buffered_plugin_version_length(int api, int* version_abi, int* version_api, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_stream_buffered_plugin_version_fill(int api, int* version_abi, int* version_api, char* buffer, int buffer_capacity, int* written);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_writer_plugin_version_length(int api, int* version_abi, int* version_api, int* length);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_videoio_registry_get_writer_plugin_version_fill(int api, int* version_abi, int* version_api, char* buffer, int buffer_capacity, int* written);

typedef long long (*jyppx_ocv_video_stream_reader_read_callback)(void* context, char* buffer, long long size);
typedef long long (*jyppx_ocv_video_stream_reader_seek_callback)(void* context, long long offset, int origin);
typedef void (*jyppx_ocv_video_stream_reader_release_callback)(void* context);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_stream_reader_create(
    void* context,
    jyppx_ocv_video_stream_reader_read_callback read_callback,
    jyppx_ocv_video_stream_reader_seek_callback seek_callback,
    jyppx_ocv_video_stream_reader_release_callback release_callback,
    jyppx_ocv_video_stream_reader** reader);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_stream_reader_read(
    jyppx_ocv_video_stream_reader* reader,
    char* buffer,
    long long size,
    long long* bytes_read);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_video_stream_reader_seek(
    jyppx_ocv_video_stream_reader* reader,
    long long offset,
    int origin,
    long long* position);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_video_stream_reader_release_handle(jyppx_ocv_video_stream_reader* reader);
