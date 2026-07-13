#include "open_cv_sharp/imgcodecs.h"

#include "core/mat_handle.h"
#include "error_state.h"

#include <filesystem>
#include <fstream>
#include <limits>
#include <memory>
#include <string>
#include <vector>

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/imgcodecs.hpp>
#endif

struct jyppx_ocv_encoded_buffer
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::vector<unsigned char> value;
#else
    int placeholder;
#endif
};

namespace
{
    std::filesystem::path path_from_utf8(const char* filename)
    {
        return std::filesystem::u8path(filename);
    }

    std::string extension_from_path_bytes(const char* filename)
    {
        std::string path(filename == nullptr ? "" : filename);
        std::string::size_type separator = path.find_last_of("/\\");
        std::string::size_type dot = path.find_last_of('.');

        if (dot == std::string::npos)
        {
            return std::string();
        }

        if (separator != std::string::npos && dot < separator)
        {
            return std::string();
        }

        return path.substr(dot);
    }

    int read_file_bytes(const char* api_name, const char* filename, std::vector<unsigned char>& bytes)
    {
        std::ifstream stream(path_from_utf8(filename), std::ios::binary);
        if (!stream)
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, std::string(api_name) + " failed: file could not be opened for reading.");
        }

        bytes.assign(
            std::istreambuf_iterator<char>(stream),
            std::istreambuf_iterator<char>());

        if (bytes.empty())
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, std::string(api_name) + " failed: file is empty.");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int write_file_bytes(const char* filename, const std::vector<unsigned char>& bytes, int* out_written)
    {
        std::ofstream stream(path_from_utf8(filename), std::ios::binary | std::ios::trunc);
        if (!stream)
        {
            *out_written = 0;
            return OPENCV_CSHARP_STATUS_OK;
        }

        stream.write(reinterpret_cast<const char*>(bytes.data()), static_cast<std::streamsize>(bytes.size()));
        stream.flush();
        *out_written = stream.good() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::vector<int> params_to_vector(const int* params, size_t params_length)
    {
        std::vector<int> encode_params;
        if (params_length != 0)
        {
            encode_params.assign(params, params + params_length);
        }

        return encode_params;
    }
#endif

    int validate_params(const char* api_name, const int* params, size_t params_length)
    {
        if (params == nullptr && params_length != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

        if ((params_length % 2) != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params_length");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int imencode_core(
        const char* api_name,
        const char* ext,
        const jyppx_ocv_mat* image,
        const int* params,
        size_t params_length,
        jyppx_ocv_encoded_buffer** out_buffer)
    {
        if (ext == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "ext");
        }

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (out_buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_buffer");
        }

        *out_buffer = nullptr;

        int params_status = validate_params(api_name, params, params_length);
        if (params_status != OPENCV_CSHARP_STATUS_OK)
        {
            return params_status;
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<int> encode_params = params_to_vector(params, params_length);
        auto encoded_buffer = std::make_unique<jyppx_ocv_encoded_buffer>();
        if (!cv::imencode(ext, opencv_csharp_native::mat_value(image), encoded_buffer->value, encode_params))
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imencode returned false.");
        }

        *out_buffer = encoded_buffer.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }

    int imwrite_core(
        const char* api_name,
        const char* filename,
        const jyppx_ocv_mat* image,
        const int* params,
        size_t params_length,
        int* out_written)
    {
        if (filename == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (out_written == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_written");
        }

        *out_written = 0;

        int params_status = validate_params(api_name, params, params_length);
        if (params_status != OPENCV_CSHARP_STATUS_OK)
        {
            return params_status;
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string extension = extension_from_path_bytes(filename);
        if (extension.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }

        std::vector<int> write_params = params_to_vector(params, params_length);
        std::vector<unsigned char> encoded;
        if (!cv::imencode(extension, opencv_csharp_native::mat_value(image), encoded, write_params))
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, std::string(api_name) + " failed: cv::imencode returned false.");
        }

        return write_file_bytes(filename, encoded, out_written);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
}

int jyppx_ocv_imgcodecs_imencode(
    const char* ext,
    const jyppx_ocv_mat* image,
    jyppx_ocv_encoded_buffer** out_buffer)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imencode";

    try
    {
        opencv_csharp_native::clear_last_error();
        return imencode_core(api_name, ext, image, nullptr, 0, out_buffer);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_imencode_with_params(
    const char* ext,
    const jyppx_ocv_mat* image,
    const int* params,
    size_t params_length,
    jyppx_ocv_encoded_buffer** out_buffer)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imencode_with_params";

    try
    {
        opencv_csharp_native::clear_last_error();
        return imencode_core(api_name, ext, image, params, params_length, out_buffer);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_imdecode(
    const unsigned char* buffer,
    size_t buffer_length,
    int flags,
    jyppx_ocv_mat** out_image)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imdecode";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        if (buffer_length == 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer_length");
        }

        if (out_image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_image");
        }

        *out_image = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (buffer_length > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer_length");
        }

        cv::Mat encoded(1, static_cast<int>(buffer_length), CV_8UC1, const_cast<unsigned char*>(buffer));
        cv::Mat decoded = cv::imdecode(encoded, flags);
        if (decoded.empty())
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imdecode returned an empty image.");
        }

        auto image = new jyppx_ocv_mat();
        image->value = decoded;
        *out_image = image;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_imread(const char* filename, int flags, jyppx_ocv_mat** out_image)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imread";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (filename == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }

        if (out_image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_image");
        }

        *out_image = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<unsigned char> encoded;
        int read_status = read_file_bytes(api_name, filename, encoded);
        if (read_status != OPENCV_CSHARP_STATUS_OK)
        {
            return read_status;
        }

        if (encoded.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }

        cv::Mat encoded_view(1, static_cast<int>(encoded.size()), CV_8UC1, encoded.data());
        cv::Mat decoded = cv::imdecode(encoded_view, flags);
        if (decoded.empty())
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imdecode returned an empty image.");
        }

        auto image = new jyppx_ocv_mat();
        image->value = decoded;
        *out_image = image;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_imwrite(
    const char* filename,
    const jyppx_ocv_mat* image,
    int* out_written)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imwrite";

    try
    {
        opencv_csharp_native::clear_last_error();
        return imwrite_core(api_name, filename, image, nullptr, 0, out_written);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_imwrite_with_params(
    const char* filename,
    const jyppx_ocv_mat* image,
    const int* params,
    size_t params_length,
    int* out_written)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imwrite_with_params";

    try
    {
        opencv_csharp_native::clear_last_error();
        return imwrite_core(api_name, filename, image, params, params_length, out_written);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_encoded_buffer_size(const jyppx_ocv_encoded_buffer* buffer, size_t* out_size)
{
    constexpr const char* api_name = "jyppx_ocv_encoded_buffer_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        if (out_size == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_size");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_size = buffer->value.size();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_size = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_encoded_buffer_data(
    const jyppx_ocv_encoded_buffer* buffer,
    const unsigned char** out_data)
{
    constexpr const char* api_name = "jyppx_ocv_encoded_buffer_data";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        if (out_data == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_data");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_data = buffer->value.empty() ? nullptr : buffer->value.data();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_data = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_encoded_buffer_release(jyppx_ocv_encoded_buffer* buffer)
{
    delete buffer;
}
