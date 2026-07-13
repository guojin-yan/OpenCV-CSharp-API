#include "open_cv_sharp/videoio/videoio.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "videoio_handles.h"

#include <cstring>
#include <limits>
#include <new>
#include <vector>

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/videoio/registry.hpp>
#endif

namespace
{
    int validate_capture(const char* api_name, const jyppx_ocv_video_capture* capture)
    {
        return capture == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "capture")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_writer(const char* api_name, const jyppx_ocv_video_writer* writer)
    {
        return writer == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "writer")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_filename(const char* api_name, const char* filename)
    {
        if (filename == nullptr || filename[0] == '\0')
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_bool_output(const char* api_name, int* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    void assign_bool(int* destination, bool value)
    {
        if (destination != nullptr)
        {
            *destination = value ? 1 : 0;
        }
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    int copy_string_to_output(const char* api_name, const cv::String& source, char* buffer, int buffer_capacity, int* written)
    {
        if (written == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "written");
        }

        if (source.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "source");
        }

        *written = static_cast<int>(source.size());
        if (buffer == nullptr || buffer_capacity < *written)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        if (*written > 0)
        {
            std::memcpy(buffer, source.c_str(), static_cast<size_t>(*written));
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    bool capture_retrieve(jyppx_ocv_video_capture* capture, jyppx_ocv_mat* image, int flag)
    {
        return capture->value.retrieve(opencv_csharp_native::mat_value(image), flag);
    }

    bool capture_read(jyppx_ocv_video_capture* capture, jyppx_ocv_mat* image)
    {
        return capture->value.read(opencv_csharp_native::mat_value(image));
    }

    bool writer_write(jyppx_ocv_video_writer* writer, const jyppx_ocv_mat* image)
    {
        return writer->value.write(opencv_csharp_native::mat_value(image));
    }
#endif
}

int jyppx_ocv_video_capture_create(jyppx_ocv_video_capture** capture)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (capture == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "capture");
        }

        *capture = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_video_capture* created = new (std::nothrow) jyppx_ocv_video_capture();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *capture = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_video_capture_release_handle(jyppx_ocv_video_capture* capture)
{
    delete capture;
}

int jyppx_ocv_video_capture_open_file(jyppx_ocv_video_capture* capture, const char* filename, int api_preference, int* opened)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_open_file";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_filename(api_name, filename);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, opened, "opened");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(opened, capture->value.open(filename, api_preference));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filename;
        (void)api_preference;
        assign_bool(opened, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_capture_open_index(jyppx_ocv_video_capture* capture, int index, int api_preference, int* opened)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_open_index";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, opened, "opened");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(opened, capture->value.open(index, api_preference));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)index;
        (void)api_preference;
        assign_bool(opened, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_capture_is_opened(const jyppx_ocv_video_capture* capture, int* opened)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_is_opened";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, opened, "opened");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(opened, capture->value.isOpened());
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(opened, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_capture_release(jyppx_ocv_video_capture* capture)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_release";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        capture->value.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_capture_grab(jyppx_ocv_video_capture* capture, int* grabbed)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_grab";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, grabbed, "grabbed");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(grabbed, capture->value.grab());
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(grabbed, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_capture_retrieve(jyppx_ocv_video_capture* capture, jyppx_ocv_mat* image, int flag, int* retrieved)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_retrieve";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, retrieved, "retrieved");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(retrieved, capture_retrieve(capture, image, flag));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flag;
        assign_bool(retrieved, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_capture_read(jyppx_ocv_video_capture* capture, jyppx_ocv_mat* image, int* read)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_read";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, read, "read");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(read, capture_read(capture, image));
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(read, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_capture_get(const jyppx_ocv_video_capture* capture, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_get";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *value = capture->value.get(property_id);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)property_id;
        *value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_capture_set(jyppx_ocv_video_capture* capture, int property_id, double value, int* success)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_set";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, success, "success");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(success, capture->value.set(property_id, value));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)property_id;
        (void)value;
        assign_bool(success, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_capture_backend_name_length(const jyppx_ocv_video_capture* capture, int* length)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_backend_name_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (length == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "length");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::String name = capture->value.getBackendName();
        *length = static_cast<int>(name.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *length = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_capture_backend_name_fill(const jyppx_ocv_video_capture* capture, char* buffer, int buffer_capacity, int* written)
{
    constexpr const char* api_name = "jyppx_ocv_video_capture_backend_name_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_capture(api_name, capture);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::String name = capture->value.getBackendName();
        return copy_string_to_output(api_name, name, buffer, buffer_capacity, written);
#else
        (void)buffer;
        (void)buffer_capacity;
        if (written != nullptr) { *written = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_writer_create(jyppx_ocv_video_writer** writer)
{
    constexpr const char* api_name = "jyppx_ocv_video_writer_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (writer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "writer");
        }

        *writer = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_video_writer* created = new (std::nothrow) jyppx_ocv_video_writer();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *writer = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_video_writer_release_handle(jyppx_ocv_video_writer* writer)
{
    delete writer;
}

int jyppx_ocv_video_writer_open(
    jyppx_ocv_video_writer* writer,
    const char* filename,
    int api_preference,
    int fourcc,
    double fps,
    int frame_width,
    int frame_height,
    int is_color,
    int* opened)
{
    constexpr const char* api_name = "jyppx_ocv_video_writer_open";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_writer(api_name, writer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_filename(api_name, filename);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, opened, "opened");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (frame_width <= 0 || frame_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "frame_size");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Size frame_size(frame_width, frame_height);
        bool result = api_preference == 0
            ? writer->value.open(filename, fourcc, fps, frame_size, is_color != 0)
            : writer->value.open(filename, api_preference, fourcc, fps, frame_size, is_color != 0);
        assign_bool(opened, result);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filename;
        (void)api_preference;
        (void)fourcc;
        (void)fps;
        (void)is_color;
        assign_bool(opened, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_writer_is_opened(const jyppx_ocv_video_writer* writer, int* opened)
{
    constexpr const char* api_name = "jyppx_ocv_video_writer_is_opened";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_writer(api_name, writer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, opened, "opened");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(opened, writer->value.isOpened());
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(opened, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_writer_release(jyppx_ocv_video_writer* writer)
{
    constexpr const char* api_name = "jyppx_ocv_video_writer_release";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_writer(api_name, writer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        writer->value.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_writer_write(jyppx_ocv_video_writer* writer, const jyppx_ocv_mat* image, int* written)
{
    constexpr const char* api_name = "jyppx_ocv_video_writer_write";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_writer(api_name, writer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, written, "written");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(written, writer_write(writer, image));
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(written, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_writer_get(const jyppx_ocv_video_writer* writer, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_video_writer_get";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_writer(api_name, writer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *value = writer->value.get(property_id);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)property_id;
        *value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_writer_set(jyppx_ocv_video_writer* writer, int property_id, double value, int* success)
{
    constexpr const char* api_name = "jyppx_ocv_video_writer_set";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_writer(api_name, writer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_bool_output(api_name, success, "success");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(success, writer->value.set(property_id, value));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)property_id;
        (void)value;
        assign_bool(success, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_writer_backend_name_length(const jyppx_ocv_video_writer* writer, int* length)
{
    constexpr const char* api_name = "jyppx_ocv_video_writer_backend_name_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_writer(api_name, writer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (length == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "length");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::String name = writer->value.getBackendName();
        *length = static_cast<int>(name.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *length = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_writer_backend_name_fill(const jyppx_ocv_video_writer* writer, char* buffer, int buffer_capacity, int* written)
{
    constexpr const char* api_name = "jyppx_ocv_video_writer_backend_name_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_writer(api_name, writer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::String name = writer->value.getBackendName();
        return copy_string_to_output(api_name, name, buffer, buffer_capacity, written);
#else
        (void)buffer;
        (void)buffer_capacity;
        if (written != nullptr) { *written = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_writer_fourcc(int c1, int c2, int c3, int c4, int* fourcc)
{
    constexpr const char* api_name = "jyppx_ocv_video_writer_fourcc";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (fourcc == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "fourcc");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *fourcc = cv::VideoWriter::fourcc(
            static_cast<char>(c1),
            static_cast<char>(c2),
            static_cast<char>(c3),
            static_cast<char>(c4));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *fourcc =
            (c1 & 255) |
            ((c2 & 255) << 8) |
            ((c3 & 255) << 16) |
            ((c4 & 255) << 24);
        return OPENCV_CSHARP_STATUS_OK;
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_videoio_registry_get_backends_count(int* count)
{
    constexpr const char* api_name = "jyppx_ocv_videoio_registry_get_backends_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::VideoCaptureAPIs> backends = cv::videoio_registry::getBackends();
        if (backends.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "backends");
        }

        *count = static_cast<int>(backends.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_videoio_registry_get_backends_fill(int* backends, int backend_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_videoio_registry_get_backends_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::VideoCaptureAPIs> values = cv::videoio_registry::getBackends();
        if (values.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "backends");
        }

        *count = static_cast<int>(values.size());
        if (backends == nullptr || backend_capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "backends");
        }

        for (int i = 0; i < *count; ++i)
        {
            backends[i] = static_cast<int>(values[static_cast<size_t>(i)]);
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)backends;
        (void)backend_capacity;
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_videoio_registry_get_backend_name_length(int api, int* length)
{
    constexpr const char* api_name = "jyppx_ocv_videoio_registry_get_backend_name_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (length == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "length");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::String name = cv::videoio_registry::getBackendName(static_cast<cv::VideoCaptureAPIs>(api));
        if (name.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "name");
        }

        *length = static_cast<int>(name.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *length = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_videoio_registry_get_backend_name_fill(int api, char* buffer, int buffer_capacity, int* written)
{
    constexpr const char* api_name = "jyppx_ocv_videoio_registry_get_backend_name_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::String name = cv::videoio_registry::getBackendName(static_cast<cv::VideoCaptureAPIs>(api));
        return copy_string_to_output(api_name, name, buffer, buffer_capacity, written);
#else
        (void)api;
        (void)buffer;
        (void)buffer_capacity;
        if (written != nullptr) { *written = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_videoio_registry_has_backend(int api, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_videoio_registry_has_backend";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "result");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(result, cv::videoio_registry::hasBackend(static_cast<cv::VideoCaptureAPIs>(api)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(result, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_videoio_registry_is_backend_built_in(int api, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_videoio_registry_is_backend_built_in";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "result");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(result, cv::videoio_registry::isBackendBuiltIn(static_cast<cv::VideoCaptureAPIs>(api)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(result, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

