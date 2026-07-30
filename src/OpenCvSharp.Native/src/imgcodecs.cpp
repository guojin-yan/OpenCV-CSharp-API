#include "open_cv_sharp/imgcodecs.h"

#include "core/mat_handle.h"
#include "error_state.h"

#include <atomic>
#include <chrono>
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

struct jyppx_ocv_imgcodecs_mat_vector
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::vector<cv::Mat> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_imgcodecs_metadata_result
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Mat image;
    std::vector<int> types;
    std::vector<cv::Mat> metadata;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_imgcodecs_animation
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Animation value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_imgcodecs_image_collection
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::ImageCollection value;
    std::filesystem::path temporary_path;

    ~jyppx_ocv_imgcodecs_image_collection()
    {
        value = cv::ImageCollection();
        if (!temporary_path.empty())
        {
            std::error_code ignored;
            std::filesystem::remove(temporary_path, ignored);
        }
    }
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
    bool contains_non_ascii(const char* value)
    {
        for (const unsigned char* current = reinterpret_cast<const unsigned char*>(value); *current != 0; ++current)
        {
            if (*current >= 0x80)
            {
                return true;
            }
        }
        return false;
    }

    int initialize_image_collection(
        const char* api_name,
        jyppx_ocv_imgcodecs_image_collection& collection,
        const char* filename,
        int flags)
    {
        collection.value = cv::ImageCollection();
        if (!collection.temporary_path.empty())
        {
            std::error_code ignored;
            std::filesystem::remove(collection.temporary_path, ignored);
            collection.temporary_path.clear();
        }

        if (!contains_non_ascii(filename))
        {
            collection.value.init(filename, flags);
            return OPENCV_CSHARP_STATUS_OK;
        }

        std::vector<unsigned char> bytes;
        int status = read_file_bytes(api_name, filename, bytes);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        static std::atomic<unsigned long long> sequence{0};
        const auto timestamp = std::chrono::steady_clock::now().time_since_epoch().count();
        const std::string extension = extension_from_path_bytes(filename);
        collection.temporary_path = std::filesystem::temp_directory_path() /
            ("jyppx-opencv-image-collection-" + std::to_string(timestamp) + "-" +
             std::to_string(sequence.fetch_add(1)) + extension);

        int written = 0;
        status = write_file_bytes(collection.temporary_path.string().c_str(), bytes, &written);
        if (status != OPENCV_CSHARP_STATUS_OK || written == 0)
        {
            collection.temporary_path.clear();
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, std::string(api_name) + " failed: temporary collection file could not be written.");
        }

        collection.value.init(collection.temporary_path.string(), flags);
        return OPENCV_CSHARP_STATUS_OK;
    }

    std::vector<int> params_to_vector(const int* params, size_t params_length)
    {
        std::vector<int> encode_params;
        if (params_length != 0)
        {
            encode_params.assign(params, params + params_length);
        }

        return encode_params;
    }

    cv::Mat encoded_mat(const unsigned char* buffer, size_t buffer_length)
    {
        return cv::Mat(1, static_cast<int>(buffer_length), CV_8UC1, const_cast<unsigned char*>(buffer));
    }

    jyppx_ocv_mat* clone_mat(const cv::Mat& value)
    {
        auto result = std::make_unique<jyppx_ocv_mat>();
        result->value = value.clone();
        return result.release();
    }

    std::vector<cv::Mat> mats_from_handles(const jyppx_ocv_mat* const* handles, size_t count)
    {
        std::vector<cv::Mat> result;
        result.reserve(count);
        for (size_t index = 0; index < count; ++index)
        {
            result.push_back(opencv_csharp_native::mat_value(handles[index]));
        }
        return result;
    }

    std::unique_ptr<jyppx_ocv_encoded_buffer> make_encoded_buffer(std::vector<unsigned char>&& value)
    {
        auto result = std::make_unique<jyppx_ocv_encoded_buffer>();
        result->value = std::move(value);
        return result;
    }
#endif

    int validate_buffer(const char* api_name, const unsigned char* buffer, size_t buffer_length)
    {
        if (buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }
        if (buffer_length == 0 || buffer_length > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer_length");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat_handles(
        const char* api_name,
        const jyppx_ocv_mat* const* images,
        size_t image_count,
        const char* parameter_name)
    {
        if (images == nullptr || image_count == 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }
        for (size_t index = 0; index < image_count; ++index)
        {
            if (images[index] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
            }
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

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

int jyppx_ocv_imgcodecs_imread_into(const char* filename, int flags, jyppx_ocv_mat* image)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imread_into";

    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }
        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<unsigned char> encoded;
        int status = read_file_bytes(api_name, filename, encoded);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::Mat decoded = cv::imdecode(encoded_mat(encoded.data(), encoded.size()), flags, &image->value);
        if (decoded.empty())
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imdecode returned an empty image.");
        }
        image->value = decoded;
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

int jyppx_ocv_imgcodecs_imread_multi(
    const char* filename,
    int flags,
    int has_range,
    int start,
    int count,
    jyppx_ocv_imgcodecs_mat_vector** out_images,
    int* out_success)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imread_multi";

    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }
        if (out_images == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_images");
        }
        if (out_success == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_success");
        }
        if (has_range != 0 && (start < 0 || count <= 0 || start > std::numeric_limits<int>::max() - count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "range");
        }
        *out_images = nullptr;
        *out_success = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<unsigned char> encoded;
        int status = read_file_bytes(api_name, filename, encoded);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        auto images = std::make_unique<jyppx_ocv_imgcodecs_mat_vector>();
        const cv::Range range = has_range != 0 ? cv::Range(start, start + count) : cv::Range::all();
        *out_success = cv::imdecodemulti(encoded_mat(encoded.data(), encoded.size()), flags, images->value, range) ? 1 : 0;
        *out_images = images.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        (void)start;
        (void)count;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_imdecode_multi(
    const unsigned char* buffer,
    size_t buffer_length,
    int flags,
    int has_range,
    int start,
    int end,
    jyppx_ocv_imgcodecs_mat_vector** out_images,
    int* out_success)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imdecode_multi";

    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_buffer(api_name, buffer, buffer_length);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        if (out_images == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_images");
        }
        if (out_success == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_success");
        }
        if (has_range != 0 && (start < 0 || end <= start))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "range");
        }
        *out_images = nullptr;
        *out_success = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto images = std::make_unique<jyppx_ocv_imgcodecs_mat_vector>();
        const cv::Range range = has_range != 0 ? cv::Range(start, end) : cv::Range::all();
        *out_success = cv::imdecodemulti(encoded_mat(buffer, buffer_length), flags, images->value, range) ? 1 : 0;
        *out_images = images.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        (void)start;
        (void)end;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_imwrite_multi(
    const char* filename,
    const jyppx_ocv_mat* const* images,
    size_t image_count,
    const int* params,
    size_t params_length,
    int* out_written)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imwrite_multi";

    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }
        if (out_written == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_written");
        }
        *out_written = 0;
        int status = validate_mat_handles(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        status = validate_params(api_name, params, params_length);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string extension = extension_from_path_bytes(filename);
        if (extension.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }
        std::vector<unsigned char> encoded;
        if (!cv::imencodemulti(extension, mats_from_handles(images, image_count), encoded, params_to_vector(params, params_length)))
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imencodemulti returned false.");
        }
        return write_file_bytes(filename, encoded, out_written);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_imencode_multi(
    const char* ext,
    const jyppx_ocv_mat* const* images,
    size_t image_count,
    const int* params,
    size_t params_length,
    jyppx_ocv_encoded_buffer** out_buffer)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imencode_multi";

    try
    {
        opencv_csharp_native::clear_last_error();
        if (ext == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "ext");
        }
        if (out_buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_buffer");
        }
        *out_buffer = nullptr;
        int status = validate_mat_handles(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        status = validate_params(api_name, params, params_length);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<unsigned char> encoded;
        if (!cv::imencodemulti(ext, mats_from_handles(images, image_count), encoded, params_to_vector(params, params_length)))
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imencodemulti returned false.");
        }
        *out_buffer = make_encoded_buffer(std::move(encoded)).release();
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

int jyppx_ocv_imgcodecs_imcount(const char* filename, int flags, size_t* out_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imcount";

    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }
        if (out_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_count");
        }
        *out_count = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<unsigned char> encoded;
        int status = read_file_bytes(api_name, filename, encoded);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        std::vector<cv::Mat> images;
        if (cv::imdecodemulti(encoded_mat(encoded.data(), encoded.size()), flags, images, cv::Range::all()))
        {
            *out_count = images.size();
        }
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

int jyppx_ocv_imgcodecs_have_image_reader(const char* filename, int* out_available)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_have_image_reader";

    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }
        if (out_available == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_available");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<unsigned char> encoded;
        int status = read_file_bytes(api_name, filename, encoded);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            opencv_csharp_native::clear_last_error();
            *out_available = 0;
            return OPENCV_CSHARP_STATUS_OK;
        }
        *out_available = cv::imdecode(encoded_mat(encoded.data(), encoded.size()), cv::IMREAD_UNCHANGED).empty() ? 0 : 1;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_available = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_have_image_writer(const char* filename_or_extension, int* out_available)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_have_image_writer";

    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename_or_extension == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename_or_extension");
        }
        if (out_available == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_available");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_available = cv::haveImageWriter(filename_or_extension) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_available = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_mat_vector_count(const jyppx_ocv_imgcodecs_mat_vector* images, size_t* out_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_mat_vector_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (images == nullptr || out_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, images == nullptr ? "images" : "out_count");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_count = images->value.size();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_mat_vector_clone_at(
    const jyppx_ocv_imgcodecs_mat_vector* images,
    size_t index,
    jyppx_ocv_mat** out_image)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_mat_vector_clone_at";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (images == nullptr || out_image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, images == nullptr ? "images" : "out_image");
        }
        *out_image = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (index >= images->value.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }
        *out_image = clone_mat(images->value[index]);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)index;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_imgcodecs_mat_vector_release(jyppx_ocv_imgcodecs_mat_vector* images)
{
    delete images;
}

int jyppx_ocv_imgcodecs_imread_with_metadata(
    const char* filename,
    int flags,
    jyppx_ocv_imgcodecs_metadata_result** out_result)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imread_with_metadata";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr || out_result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, filename == nullptr ? "filename" : "out_result");
        }
        *out_result = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<unsigned char> encoded;
        int status = read_file_bytes(api_name, filename, encoded);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        auto result = std::make_unique<jyppx_ocv_imgcodecs_metadata_result>();
        result->image = cv::imdecodeWithMetadata(encoded_mat(encoded.data(), encoded.size()), result->types, result->metadata, flags);
        if (result->image.empty())
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imdecodeWithMetadata returned an empty image.");
        }
        if (result->types.size() != result->metadata.size())
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "OpenCV returned mismatched metadata arrays.");
        }
        *out_result = result.release();
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

int jyppx_ocv_imgcodecs_imdecode_with_metadata(
    const unsigned char* buffer,
    size_t buffer_length,
    int flags,
    jyppx_ocv_imgcodecs_metadata_result** out_result)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imdecode_with_metadata";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_buffer(api_name, buffer, buffer_length);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        if (out_result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_result");
        }
        *out_result = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto result = std::make_unique<jyppx_ocv_imgcodecs_metadata_result>();
        result->image = cv::imdecodeWithMetadata(encoded_mat(buffer, buffer_length), result->types, result->metadata, flags);
        if (result->image.empty())
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imdecodeWithMetadata returned an empty image.");
        }
        if (result->types.size() != result->metadata.size())
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "OpenCV returned mismatched metadata arrays.");
        }
        *out_result = result.release();
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

int jyppx_ocv_imgcodecs_imwrite_with_metadata(
    const char* filename,
    const jyppx_ocv_mat* image,
    const int* metadata_types,
    const jyppx_ocv_mat* const* metadata,
    size_t metadata_count,
    const int* params,
    size_t params_length,
    int* out_written)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imwrite_with_metadata";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr || image == nullptr || out_written == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, filename == nullptr ? "filename" : (image == nullptr ? "image" : "out_written"));
        }
        *out_written = 0;
        if ((metadata_types == nullptr && metadata_count != 0) || (metadata == nullptr && metadata_count != 0))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, metadata_types == nullptr ? "metadata_types" : "metadata");
        }
        int status = metadata_count == 0 ? OPENCV_CSHARP_STATUS_OK : validate_mat_handles(api_name, metadata, metadata_count, "metadata");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        status = validate_params(api_name, params, params_length);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string extension = extension_from_path_bytes(filename);
        if (extension.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }
        std::vector<int> types;
        if (metadata_count != 0)
        {
            types.assign(metadata_types, metadata_types + metadata_count);
        }
        std::vector<cv::Mat> chunks = metadata_count == 0 ? std::vector<cv::Mat>() : mats_from_handles(metadata, metadata_count);
        std::vector<unsigned char> encoded;
        if (!cv::imencodeWithMetadata(extension, opencv_csharp_native::mat_value(image), types, chunks, encoded, params_to_vector(params, params_length)))
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imencodeWithMetadata returned false.");
        }
        return write_file_bytes(filename, encoded, out_written);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_imencode_with_metadata(
    const char* ext,
    const jyppx_ocv_mat* image,
    const int* metadata_types,
    const jyppx_ocv_mat* const* metadata,
    size_t metadata_count,
    const int* params,
    size_t params_length,
    jyppx_ocv_encoded_buffer** out_buffer)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imencode_with_metadata";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (ext == nullptr || image == nullptr || out_buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, ext == nullptr ? "ext" : (image == nullptr ? "image" : "out_buffer"));
        }
        *out_buffer = nullptr;
        if ((metadata_types == nullptr && metadata_count != 0) || (metadata == nullptr && metadata_count != 0))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, metadata_types == nullptr ? "metadata_types" : "metadata");
        }
        int status = metadata_count == 0 ? OPENCV_CSHARP_STATUS_OK : validate_mat_handles(api_name, metadata, metadata_count, "metadata");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        status = validate_params(api_name, params, params_length);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<int> types;
        if (metadata_count != 0)
        {
            types.assign(metadata_types, metadata_types + metadata_count);
        }
        std::vector<cv::Mat> chunks = metadata_count == 0 ? std::vector<cv::Mat>() : mats_from_handles(metadata, metadata_count);
        std::vector<unsigned char> encoded;
        if (!cv::imencodeWithMetadata(ext, opencv_csharp_native::mat_value(image), types, chunks, encoded, params_to_vector(params, params_length)))
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imencodeWithMetadata returned false.");
        }
        *out_buffer = make_encoded_buffer(std::move(encoded)).release();
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

int jyppx_ocv_imgcodecs_metadata_result_image_clone(
    const jyppx_ocv_imgcodecs_metadata_result* result,
    jyppx_ocv_mat** out_image)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_metadata_result_image_clone";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (result == nullptr || out_image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, result == nullptr ? "result" : "out_image");
        }
        *out_image = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_image = clone_mat(result->image);
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

int jyppx_ocv_imgcodecs_metadata_result_count(
    const jyppx_ocv_imgcodecs_metadata_result* result,
    size_t* out_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_metadata_result_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (result == nullptr || out_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, result == nullptr ? "result" : "out_count");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_count = result->metadata.size();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_metadata_result_clone_at(
    const jyppx_ocv_imgcodecs_metadata_result* result,
    size_t index,
    int* out_type,
    jyppx_ocv_mat** out_metadata)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_metadata_result_clone_at";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (result == nullptr || out_type == nullptr || out_metadata == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, result == nullptr ? "result" : (out_type == nullptr ? "out_type" : "out_metadata"));
        }
        *out_metadata = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (index >= result->metadata.size() || index >= result->types.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }
        *out_type = result->types[index];
        *out_metadata = clone_mat(result->metadata[index]);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)index;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_imgcodecs_metadata_result_release(jyppx_ocv_imgcodecs_metadata_result* result)
{
    delete result;
}

int jyppx_ocv_imgcodecs_animation_create(
    int loop_count,
    double bg0,
    double bg1,
    double bg2,
    double bg3,
    jyppx_ocv_imgcodecs_animation** out_animation)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_animation_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_animation == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_animation");
        }
        *out_animation = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto animation = std::make_unique<jyppx_ocv_imgcodecs_animation>();
        animation->value = cv::Animation(loop_count, cv::Scalar(bg0, bg1, bg2, bg3));
        *out_animation = animation.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)loop_count;
        (void)bg0;
        (void)bg1;
        (void)bg2;
        (void)bg3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_imgcodecs_animation_release(jyppx_ocv_imgcodecs_animation* animation)
{
    delete animation;
}

int jyppx_ocv_imgcodecs_animation_get_loop_count(
    const jyppx_ocv_imgcodecs_animation* animation,
    int* out_loop_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_animation_get_loop_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (animation == nullptr || out_loop_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, animation == nullptr ? "animation" : "out_loop_count");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_loop_count = animation->value.loop_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_loop_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_animation_set_loop_count(jyppx_ocv_imgcodecs_animation* animation, int loop_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_animation_set_loop_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (animation == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "animation");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        animation->value.loop_count = loop_count < 0 || loop_count > 0xffff ? 0 : loop_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)loop_count;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_animation_get_background_color(
    const jyppx_ocv_imgcodecs_animation* animation,
    double* out_bg0,
    double* out_bg1,
    double* out_bg2,
    double* out_bg3)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_animation_get_background_color";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (animation == nullptr || out_bg0 == nullptr || out_bg1 == nullptr || out_bg2 == nullptr || out_bg3 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_bg0 = animation->value.bgcolor[0];
        *out_bg1 = animation->value.bgcolor[1];
        *out_bg2 = animation->value.bgcolor[2];
        *out_bg3 = animation->value.bgcolor[3];
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_bg0 = *out_bg1 = *out_bg2 = *out_bg3 = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_animation_set_background_color(
    jyppx_ocv_imgcodecs_animation* animation,
    double bg0,
    double bg1,
    double bg2,
    double bg3)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_animation_set_background_color";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (animation == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "animation");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        animation->value.bgcolor = cv::Scalar(bg0, bg1, bg2, bg3);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)bg0;
        (void)bg1;
        (void)bg2;
        (void)bg3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_animation_set_frames(
    jyppx_ocv_imgcodecs_animation* animation,
    const jyppx_ocv_mat* const* frames,
    const int* durations,
    size_t frame_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_animation_set_frames";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (animation == nullptr || durations == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, animation == nullptr ? "animation" : "durations");
        }
        int status = validate_mat_handles(api_name, frames, frame_count, "frames");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> owned_frames;
        owned_frames.reserve(frame_count);
        for (size_t index = 0; index < frame_count; ++index)
        {
            owned_frames.push_back(opencv_csharp_native::mat_value(frames[index]).clone());
        }
        animation->value.frames = std::move(owned_frames);
        animation->value.durations.assign(durations, durations + frame_count);
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

int jyppx_ocv_imgcodecs_animation_frame_count(
    const jyppx_ocv_imgcodecs_animation* animation,
    size_t* out_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_animation_frame_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (animation == nullptr || out_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, animation == nullptr ? "animation" : "out_count");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (animation->value.frames.size() != animation->value.durations.size())
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "Animation frame and duration counts differ.");
        }
        *out_count = animation->value.frames.size();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_animation_frame_clone_at(
    const jyppx_ocv_imgcodecs_animation* animation,
    size_t index,
    jyppx_ocv_mat** out_frame,
    int* out_duration)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_animation_frame_clone_at";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (animation == nullptr || out_frame == nullptr || out_duration == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output");
        }
        *out_frame = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (index >= animation->value.frames.size() || index >= animation->value.durations.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }
        *out_frame = clone_mat(animation->value.frames[index]);
        *out_duration = animation->value.durations[index];
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)index;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_animation_still_image_clone(
    const jyppx_ocv_imgcodecs_animation* animation,
    jyppx_ocv_mat** out_image)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_animation_still_image_clone";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (animation == nullptr || out_image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, animation == nullptr ? "animation" : "out_image");
        }
        *out_image = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_image = clone_mat(animation->value.still_image);
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

int jyppx_ocv_imgcodecs_animation_set_still_image(
    jyppx_ocv_imgcodecs_animation* animation,
    const jyppx_ocv_mat* image)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_animation_set_still_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (animation == nullptr || image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, animation == nullptr ? "animation" : "image");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        animation->value.still_image = opencv_csharp_native::mat_value(image).clone();
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

int jyppx_ocv_imgcodecs_imread_animation(
    const char* filename,
    int start,
    int count,
    jyppx_ocv_imgcodecs_animation* animation,
    int* out_success)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imread_animation";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr || animation == nullptr || out_success == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "argument");
        }
        if (start < 0 || count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "range");
        }
        *out_success = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<unsigned char> encoded;
        int status = read_file_bytes(api_name, filename, encoded);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        *out_success = cv::imdecodeanimation(encoded_mat(encoded.data(), encoded.size()), animation->value, start, count) ? 1 : 0;
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

int jyppx_ocv_imgcodecs_imdecode_animation(
    const unsigned char* buffer,
    size_t buffer_length,
    int start,
    int count,
    jyppx_ocv_imgcodecs_animation* animation,
    int* out_success)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imdecode_animation";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_buffer(api_name, buffer, buffer_length);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        if (animation == nullptr || out_success == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, animation == nullptr ? "animation" : "out_success");
        }
        if (start < 0 || count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "range");
        }
        *out_success = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_success = cv::imdecodeanimation(encoded_mat(buffer, buffer_length), animation->value, start, count) ? 1 : 0;
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

int jyppx_ocv_imgcodecs_imwrite_animation(
    const char* filename,
    const jyppx_ocv_imgcodecs_animation* animation,
    const int* params,
    size_t params_length,
    int* out_written)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imwrite_animation";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr || animation == nullptr || out_written == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "argument");
        }
        *out_written = 0;
        int status = validate_params(api_name, params, params_length);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string extension = extension_from_path_bytes(filename);
        if (extension.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }
        std::vector<unsigned char> encoded;
        if (!cv::imencodeanimation(extension, animation->value, encoded, params_to_vector(params, params_length)))
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imencodeanimation returned false.");
        }
        return write_file_bytes(filename, encoded, out_written);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgcodecs_imencode_animation(
    const char* ext,
    const jyppx_ocv_imgcodecs_animation* animation,
    const int* params,
    size_t params_length,
    jyppx_ocv_encoded_buffer** out_buffer)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_imencode_animation";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (ext == nullptr || animation == nullptr || out_buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "argument");
        }
        *out_buffer = nullptr;
        int status = validate_params(api_name, params, params_length);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<unsigned char> encoded;
        if (!cv::imencodeanimation(ext, animation->value, encoded, params_to_vector(params, params_length)))
        {
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "cv::imencodeanimation returned false.");
        }
        *out_buffer = make_encoded_buffer(std::move(encoded)).release();
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

int jyppx_ocv_imgcodecs_image_collection_create(jyppx_ocv_imgcodecs_image_collection** out_collection)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_image_collection_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_collection == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_collection");
        }
        *out_collection = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_collection = new jyppx_ocv_imgcodecs_image_collection();
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

int jyppx_ocv_imgcodecs_image_collection_create_file(
    const char* filename,
    int flags,
    jyppx_ocv_imgcodecs_image_collection** out_collection)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_image_collection_create_file";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr || out_collection == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, filename == nullptr ? "filename" : "out_collection");
        }
        *out_collection = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto collection = std::make_unique<jyppx_ocv_imgcodecs_image_collection>();
        int status = initialize_image_collection(api_name, *collection, filename, flags);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        *out_collection = collection.release();
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

void jyppx_ocv_imgcodecs_image_collection_release(jyppx_ocv_imgcodecs_image_collection* collection)
{
    delete collection;
}

int jyppx_ocv_imgcodecs_image_collection_init(
    jyppx_ocv_imgcodecs_image_collection* collection,
    const char* filename,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_image_collection_init";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (collection == nullptr || filename == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, collection == nullptr ? "collection" : "filename");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return initialize_image_collection(api_name, *collection, filename, flags);
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

int jyppx_ocv_imgcodecs_image_collection_size(
    const jyppx_ocv_imgcodecs_image_collection* collection,
    size_t* out_size)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_image_collection_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (collection == nullptr || out_size == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, collection == nullptr ? "collection" : "out_size");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_size = collection->value.size();
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

int jyppx_ocv_imgcodecs_image_collection_clone_at(
    jyppx_ocv_imgcodecs_image_collection* collection,
    int index,
    jyppx_ocv_mat** out_image)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_image_collection_clone_at";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (collection == nullptr || out_image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, collection == nullptr ? "collection" : "out_image");
        }
        *out_image = nullptr;
        if (index < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (static_cast<size_t>(index) >= collection->value.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }
        *out_image = clone_mat(collection->value.at(index));
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

int jyppx_ocv_imgcodecs_image_collection_release_cache(
    jyppx_ocv_imgcodecs_image_collection* collection,
    int index)
{
    constexpr const char* api_name = "jyppx_ocv_imgcodecs_image_collection_release_cache";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (collection == nullptr || index < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, collection == nullptr ? "collection" : "index");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (static_cast<size_t>(index) >= collection->value.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }
        collection->value.releaseCache(index);
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
