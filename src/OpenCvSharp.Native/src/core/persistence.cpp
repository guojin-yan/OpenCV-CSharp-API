#include "open_cv_sharp/core/persistence.h"

#include "mat_handle.h"
#include "persistence_handle_access.h"
#include "utf8_result_handle.h"
#include "../error_state.h"

#include <algorithm>
#include <cstdint>
#include <limits>
#include <memory>
#include <new>
#include <string>
#include <utility>
#include <vector>

#if defined(_WIN32)
#include <windows.h>
#endif

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/core/persistence.hpp>
#endif

struct jyppx_ocv_core_utf8_result
{
    std::string value;
};

struct jyppx_ocv_core_string_list
{
    std::vector<std::string> values;
};

#if defined(OPENCV_CSHARP_HAS_OPENCV)
namespace
{
    struct file_storage_state
    {
        cv::FileStorage value;
        std::uint64_t generation = 0;
        std::vector<std::pair<std::string, bool>> pending_comments;
        bool has_written_value = false;
    };
}
#endif

struct jyppx_ocv_core_file_storage
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::shared_ptr<file_storage_state> state;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_core_file_node
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::shared_ptr<file_storage_state> state;
    std::uint64_t generation = 0;
    cv::FileNode value;
#else
    int placeholder;
#endif
};

namespace
{
    bool valid_utf8(const unsigned char* data, int length)
    {
        int i = 0;
        while (i < length)
        {
            const unsigned char first = data[i++];
            if (first <= 0x7f)
            {
                continue;
            }

            int continuation_count = 0;
            std::uint32_t code_point = 0;
            std::uint32_t minimum = 0;
            if ((first & 0xe0) == 0xc0)
            {
                continuation_count = 1;
                code_point = first & 0x1f;
                minimum = 0x80;
            }
            else if ((first & 0xf0) == 0xe0)
            {
                continuation_count = 2;
                code_point = first & 0x0f;
                minimum = 0x800;
            }
            else if ((first & 0xf8) == 0xf0)
            {
                continuation_count = 3;
                code_point = first & 0x07;
                minimum = 0x10000;
            }
            else
            {
                return false;
            }

            if (continuation_count > length - i)
            {
                return false;
            }

            for (int j = 0; j < continuation_count; ++j)
            {
                const unsigned char next = data[i++];
                if ((next & 0xc0) != 0x80)
                {
                    return false;
                }
                code_point = (code_point << 6) | (next & 0x3f);
            }

            if (code_point < minimum || code_point > 0x10ffff ||
                (code_point >= 0xd800 && code_point <= 0xdfff))
            {
                return false;
            }
        }

        return true;
    }

    int read_utf8(
        const char* api_name,
        const unsigned char* data,
        int length,
        const char* parameter_name,
        bool allow_empty,
        std::string& value)
    {
        if (length < 0 || (length > 0 && data == nullptr) || (!allow_empty && length == 0))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }
        if (length > 0 && (std::find(data, data + length, static_cast<unsigned char>(0)) != data + length || !valid_utf8(data, length)))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        if (length == 0)
            value.clear();
        else
            value.assign(reinterpret_cast<const char*>(data), static_cast<size_t>(length));
        return OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    int validate_storage(const char* api_name, const jyppx_ocv_core_file_storage* storage)
    {
        if (storage == nullptr || !storage->state)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "storage");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_node(const char* api_name, const jyppx_ocv_core_file_node* node)
    {
        if (node == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "node");
        }
        if (node->state && node->generation != node->state->generation)
        {
            return opencv_csharp_native::set_last_error(
                OPENCV_CSHARP_STATUS_INVALID_ARGUMENT,
                std::string(api_name) + " failed: node was invalidated by FileStorage.Open or FileStorage.Release.");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int make_node(
        const char* api_name,
        const std::shared_ptr<file_storage_state>& state,
        const cv::FileNode& value,
        jyppx_ocv_core_file_node** out_node)
    {
        if (out_node == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_node");
        }
        *out_node = nullptr;
        auto result = std::make_unique<jyppx_ocv_core_file_node>();
        result->state = state;
        result->generation = state ? state->generation : 0;
        result->value = value;
        *out_node = result.release();
        return OPENCV_CSHARP_STATUS_OK;
    }

    int read_name(
        const char* api_name,
        const unsigned char* data,
        int length,
        std::string& value,
        bool allow_empty = true)
    {
        return read_utf8(api_name, data, length, "name", allow_empty, value);
    }

    void advance_generation(const std::shared_ptr<file_storage_state>& state)
    {
        ++state->generation;
        state->pending_comments.clear();
        state->has_written_value = false;
    }

    void mark_value_written(const std::shared_ptr<file_storage_state>& state)
    {
        if (state->has_written_value)
        {
            return;
        }

        state->has_written_value = true;
        for (const auto& pending : state->pending_comments)
        {
            // A standalone comment after the YAML 1.2 stream marker is emitted by
            // OpenCV but cannot be read back by its parser. Attach deferred leading
            // comments to the first value so the stream remains self-readable.
            state->value.writeComment(pending.first, true);
        }
        state->pending_comments.clear();
    }

    int prepare_file_storage_source(const char* api_name, int flags, std::string& source)
    {
#if defined(_WIN32)
        if ((flags & cv::FileStorage::MEMORY) == 0 && !source.empty() && GetACP() != CP_UTF8)
        {
            const int wide_length = MultiByteToWideChar(
                CP_UTF8,
                MB_ERR_INVALID_CHARS,
                source.data(),
                static_cast<int>(source.size()),
                nullptr,
                0);
            if (wide_length <= 0)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "source");
            }

            std::wstring wide(static_cast<size_t>(wide_length), L'\0');
            if (MultiByteToWideChar(
                    CP_UTF8,
                    MB_ERR_INVALID_CHARS,
                    source.data(),
                    static_cast<int>(source.size()),
                    wide.data(),
                    wide_length) != wide_length)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "source");
            }

            BOOL used_default = FALSE;
            const UINT code_page = GetACP();
            const int encoded_length = WideCharToMultiByte(
                code_page,
                WC_NO_BEST_FIT_CHARS,
                wide.data(),
                wide_length,
                nullptr,
                0,
                nullptr,
                &used_default);
            if (encoded_length <= 0 || used_default)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "source_path_encoding");
            }

            std::string encoded(static_cast<size_t>(encoded_length), '\0');
            used_default = FALSE;
            if (WideCharToMultiByte(
                    code_page,
                    WC_NO_BEST_FIT_CHARS,
                    wide.data(),
                    wide_length,
                    encoded.data(),
                    encoded_length,
                    nullptr,
                    &used_default) != encoded_length || used_default)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "source_path_encoding");
            }
            source = std::move(encoded);
        }
#else
        (void)api_name;
        (void)flags;
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }

    bool valid_storage_flags(int flags)
    {
        constexpr int operation_mask = cv::FileStorage::WRITE | cv::FileStorage::APPEND;
        constexpr int known_mask = operation_mask | cv::FileStorage::MEMORY | cv::FileStorage::FORMAT_MASK | cv::FileStorage::BASE64;
        const int operation = flags & operation_mask;
        const int format = flags & cv::FileStorage::FORMAT_MASK;
        return (flags & ~known_mask) == 0 && operation != operation_mask &&
            (format == cv::FileStorage::FORMAT_AUTO || format == cv::FileStorage::FORMAT_XML ||
             format == cv::FileStorage::FORMAT_YAML || format == cv::FileStorage::FORMAT_JSON ||
             format == cv::FileStorage::FORMAT_YAML_1_0);
    }
#endif
}

int opencv_csharp_native::make_core_utf8_result(
    const char* api_name,
    const std::string& value,
    jyppx_ocv_core_utf8_result** out_result)
{
    if (out_result == nullptr)
    {
        return set_invalid_argument(api_name, "out_result");
    }
    *out_result = nullptr;
    auto result = std::make_unique<jyppx_ocv_core_utf8_result>();
    result->value = value;
    *out_result = result.release();
    return OPENCV_CSHARP_STATUS_OK;
}

#if defined(OPENCV_CSHARP_HAS_OPENCV)
int opencv_csharp_native::access_core_file_storage(
    const char* api_name,
    jyppx_ocv_core_file_storage* storage,
    cv::FileStorage** out_value)
{
    if (out_value == nullptr)
    {
        return set_invalid_argument(api_name, "out_value");
    }
    *out_value = nullptr;
    const int status = validate_storage(api_name, storage);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }
    if (!storage->state->value.isOpened())
    {
        return set_invalid_argument(api_name, "storage");
    }
    *out_value = &storage->state->value;
    return OPENCV_CSHARP_STATUS_OK;
}

int opencv_csharp_native::access_core_file_node(
    const char* api_name,
    const jyppx_ocv_core_file_node* node,
    const cv::FileNode** out_value)
{
    if (out_value == nullptr)
    {
        return set_invalid_argument(api_name, "out_value");
    }
    *out_value = nullptr;
    const int status = validate_node(api_name, node);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }
    *out_value = &node->value;
    return OPENCV_CSHARP_STATUS_OK;
}
#endif

int jyppx_ocv_core_file_storage_create(jyppx_ocv_core_file_storage** out_storage)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_storage == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_storage");
        }
        *out_storage = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto storage = std::make_unique<jyppx_ocv_core_file_storage>();
        storage->state = std::make_shared<file_storage_state>();
        *out_storage = storage.release();
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

void jyppx_ocv_core_file_storage_release_handle(jyppx_ocv_core_file_storage* storage)
{
    delete storage;
}

int jyppx_ocv_core_file_storage_open(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* source_utf8,
    int source_byte_length,
    int flags,
    const unsigned char* encoding_utf8,
    int encoding_byte_length,
    int* out_opened)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_open";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_opened == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_opened");
        }
        *out_opened = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!valid_storage_flags(flags)) return opencv_csharp_native::set_invalid_argument(api_name, "flags");
        std::string source;
        std::string encoding;
        status = read_utf8(api_name, source_utf8, source_byte_length, "source", true, source);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = read_utf8(api_name, encoding_utf8, encoding_byte_length, "encoding", true, encoding);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = prepare_file_storage_source(api_name, flags, source);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        advance_generation(storage->state);
        *out_opened = storage->state->value.open(source, flags, encoding) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)storage; (void)source_utf8; (void)source_byte_length; (void)flags; (void)encoding_utf8; (void)encoding_byte_length;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_is_opened(const jyppx_ocv_core_file_storage* storage, int* out_opened)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_is_opened";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_opened == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_opened");
        }
        *out_opened = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        *out_opened = storage->state->value.isOpened() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)storage;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_release(jyppx_ocv_core_file_storage* storage)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_release";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        storage->state->value.release();
        advance_generation(storage->state);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)storage;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_release_and_get_string(
    jyppx_ocv_core_file_storage* storage,
    jyppx_ocv_core_utf8_result** out_result)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_release_and_get_string";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_result");
        }
        *out_result = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        const std::string value = storage->state->value.releaseAndGetString();
        advance_generation(storage->state);
        return opencv_csharp_native::make_core_utf8_result(api_name, value, out_result);
#else
        (void)storage;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_get_first_top_level_node(
    const jyppx_ocv_core_file_storage* storage,
    jyppx_ocv_core_file_node** out_node)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_get_first_top_level_node";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return make_node(api_name, storage->state, storage->state->value.getFirstTopLevelNode(), out_node);
#else
        (void)storage; (void)out_node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_root(
    const jyppx_ocv_core_file_storage* storage,
    int stream_index,
    jyppx_ocv_core_file_node** out_node)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_root";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (stream_index < 0) return opencv_csharp_native::set_invalid_argument(api_name, "stream_index");
        return make_node(api_name, storage->state, storage->state->value.root(stream_index), out_node);
#else
        (void)storage; (void)stream_index; (void)out_node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_get_node(
    const jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    jyppx_ocv_core_file_node** out_node)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_get_node";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        std::string name;
        status = read_name(api_name, name_utf8, name_byte_length, name, false);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return make_node(api_name, storage->state, storage->state->value[name], out_node);
#else
        (void)storage; (void)name_utf8; (void)name_byte_length; (void)out_node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV)
namespace
{
    template <typename T>
    int write_scalar_value(
        const char* api_name,
        jyppx_ocv_core_file_storage* storage,
        const unsigned char* name_utf8,
        int name_byte_length,
        const T& value)
    {
        int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        std::string name;
        status = read_name(api_name, name_utf8, name_byte_length, name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        storage->state->value.write(name, value);
        mark_value_written(storage->state);
        return OPENCV_CSHARP_STATUS_OK;
    }
}
#endif

#define JYPPX_OCV_FILE_STORAGE_WRITE_SCALAR(function_name, native_type, expression) \
int function_name(jyppx_ocv_core_file_storage* storage, const unsigned char* name_utf8, int name_byte_length, native_type value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        (void)value; \
        (void)storage; (void)name_utf8; (void)name_byte_length; \
        /* The cast expression preserves the exact OpenCV overload selected by the ABI. */ \
        /* NOLINTNEXTLINE(bugprone-macro-parentheses) */ \
        /* clang-format off */ \
        /* The preprocessor branch must stay inside the exported function. */ \
        /* clang-format on */ \
        \
        return expression; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#define JYPPX_OCV_WRITE_BODY(cast_value) write_scalar_value(api_name, storage, name_utf8, name_byte_length, cast_value)
#else
#define JYPPX_OCV_WRITE_BODY(cast_value) opencv_csharp_native::set_not_linked(api_name)
#endif

JYPPX_OCV_FILE_STORAGE_WRITE_SCALAR(jyppx_ocv_core_file_storage_write_int, int, JYPPX_OCV_WRITE_BODY(value))
JYPPX_OCV_FILE_STORAGE_WRITE_SCALAR(jyppx_ocv_core_file_storage_write_bool, int, JYPPX_OCV_WRITE_BODY(value != 0))
JYPPX_OCV_FILE_STORAGE_WRITE_SCALAR(jyppx_ocv_core_file_storage_write_int64, int64_t, JYPPX_OCV_WRITE_BODY(static_cast<std::int64_t>(value)))
JYPPX_OCV_FILE_STORAGE_WRITE_SCALAR(jyppx_ocv_core_file_storage_write_double, double, JYPPX_OCV_WRITE_BODY(value))

#undef JYPPX_OCV_WRITE_BODY
#undef JYPPX_OCV_FILE_STORAGE_WRITE_SCALAR

int jyppx_ocv_core_file_storage_write_string(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    const unsigned char* value_utf8,
    int value_byte_length)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_write_string";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        std::string name;
        std::string value;
        status = read_name(api_name, name_utf8, name_byte_length, name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = read_utf8(api_name, value_utf8, value_byte_length, "value", true, value);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        storage->state->value.write(name, value);
        mark_value_written(storage->state);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)storage; (void)name_utf8; (void)name_byte_length; (void)value_utf8; (void)value_byte_length;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_write_mat(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    const jyppx_ocv_mat* value)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_write_mat";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (value == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "value");
        std::string name;
        status = read_name(api_name, name_utf8, name_byte_length, name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        storage->state->value.write(name, opencv_csharp_native::mat_value(value));
        mark_value_written(storage->state);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)storage; (void)name_utf8; (void)name_byte_length; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_write_string_vector(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    const unsigned char* values_utf8,
    int values_byte_length,
    const int* value_offsets,
    const int* value_lengths,
    int value_count)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_write_string_vector";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (values_byte_length < 0 || value_count < 0 ||
            (values_byte_length > 0 && values_utf8 == nullptr) ||
            (value_count > 0 && (value_offsets == nullptr || value_lengths == nullptr)))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "values");
        }
        std::string name;
        status = read_name(api_name, name_utf8, name_byte_length, name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;

        std::vector<cv::String> values;
        values.reserve(static_cast<size_t>(value_count));
        for (int i = 0; i < value_count; ++i)
        {
            const int offset = value_offsets[i];
            const int length = value_lengths[i];
            if (offset < 0 || length < 0 || offset > values_byte_length || length > values_byte_length - offset)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "values");
            }
            std::string value;
            const unsigned char* item = length == 0 ? nullptr : values_utf8 + offset;
            status = read_utf8(api_name, item, length, "values", true, value);
            if (status != OPENCV_CSHARP_STATUS_OK) return status;
            values.push_back(value);
        }
        storage->state->value.write(name, values);
        mark_value_written(storage->state);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)storage; (void)name_utf8; (void)name_byte_length; (void)values_utf8; (void)values_byte_length; (void)value_offsets; (void)value_lengths; (void)value_count;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_write_comment(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* comment_utf8,
    int comment_byte_length,
    int append)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_write_comment";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        std::string comment;
        status = read_utf8(api_name, comment_utf8, comment_byte_length, "comment", true, comment);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (storage->state->has_written_value)
        {
            storage->state->value.writeComment(comment, append != 0);
        }
        else
        {
            storage->state->pending_comments.emplace_back(std::move(comment), append != 0);
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)storage; (void)comment_utf8; (void)comment_byte_length; (void)append;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_start_write_struct(
    jyppx_ocv_core_file_storage* storage,
    const unsigned char* name_utf8,
    int name_byte_length,
    int flags,
    const unsigned char* type_name_utf8,
    int type_name_byte_length)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_start_write_struct";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        const int type = flags & cv::FileNode::TYPE_MASK;
        if ((flags & ~(cv::FileNode::TYPE_MASK | cv::FileNode::FLOW)) != 0 ||
            (type != cv::FileNode::SEQ && type != cv::FileNode::MAP))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "flags");
        }
        std::string name;
        std::string type_name;
        status = read_name(api_name, name_utf8, name_byte_length, name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = read_utf8(api_name, type_name_utf8, type_name_byte_length, "type_name", true, type_name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        storage->state->value.startWriteStruct(name, flags, type_name);
        mark_value_written(storage->state);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)storage; (void)name_utf8; (void)name_byte_length; (void)flags; (void)type_name_utf8; (void)type_name_byte_length;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_end_write_struct(jyppx_ocv_core_file_storage* storage)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_end_write_struct";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        storage->state->value.endWriteStruct();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)storage;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_storage_get_format(const jyppx_ocv_core_file_storage* storage, int* out_format)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_storage_get_format";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_format == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "out_format");
        *out_format = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_storage(api_name, storage);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        *out_format = storage->state->value.getFormat();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)storage;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_create(jyppx_ocv_core_file_node** out_node)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_node == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "out_node");
        *out_node = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return make_node(api_name, nullptr, cv::FileNode(), out_node);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_core_file_node_release(jyppx_ocv_core_file_node* node)
{
    delete node;
}

int jyppx_ocv_core_file_node_get_node(
    const jyppx_ocv_core_file_node* node,
    const unsigned char* name_utf8,
    int name_byte_length,
    jyppx_ocv_core_file_node** out_node)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_get_node";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!node->value.isMap()) return opencv_csharp_native::set_invalid_argument(api_name, "node");
        std::string name;
        status = read_name(api_name, name_utf8, name_byte_length, name, false);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return make_node(api_name, node->state, node->value[name], out_node);
#else
        (void)node; (void)name_utf8; (void)name_byte_length; (void)out_node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_at(
    const jyppx_ocv_core_file_node* node,
    int index,
    jyppx_ocv_core_file_node** out_node)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_at";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!node->value.isSeq() || index < 0 || static_cast<size_t>(index) >= node->value.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }
        return make_node(api_name, node->state, node->value[index], out_node);
#else
        (void)node; (void)index; (void)out_node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_keys(
    const jyppx_ocv_core_file_node* node,
    jyppx_ocv_core_string_list** out_keys)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_keys";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_keys == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "out_keys");
        *out_keys = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!node->value.isMap()) return opencv_csharp_native::set_invalid_argument(api_name, "node");
        auto result = std::make_unique<jyppx_ocv_core_string_list>();
        const std::vector<cv::String> keys = node->value.keys();
        result->values.assign(keys.begin(), keys.end());
        *out_keys = result.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_type(const jyppx_ocv_core_file_node* node, int* out_type)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_type";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_type == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "out_type");
        *out_type = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        *out_type = node->value.type();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_empty(const jyppx_ocv_core_file_node* node, int* out_empty)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_empty == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "out_empty");
        *out_empty = 1;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        *out_empty = node->value.empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_name(
    const jyppx_ocv_core_file_node* node,
    jyppx_ocv_core_utf8_result** out_result)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_name";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return opencv_csharp_native::make_core_utf8_result(api_name, node->value.name(), out_result);
#else
        (void)node; (void)out_result;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_size(const jyppx_ocv_core_file_node* node, size_t* out_size)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_size == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "out_size");
        *out_size = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        *out_size = node->value.size();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_raw_size(const jyppx_ocv_core_file_node* node, size_t* out_size)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_raw_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_size == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "out_size");
        *out_size = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        *out_size = node->value.rawSize();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_real(const jyppx_ocv_core_file_node* node, double* out_value)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_real";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_value == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "out_value");
        *out_value = 0.0;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        *out_value = node->value.real();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_string(
    const jyppx_ocv_core_file_node* node,
    jyppx_ocv_core_utf8_result** out_result)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_string";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return opencv_csharp_native::make_core_utf8_result(api_name, node->value.string(), out_result);
#else
        (void)node; (void)out_result;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_file_node_mat(const jyppx_ocv_core_file_node* node, jyppx_ocv_mat* out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_core_file_node_mat";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int status = validate_node(api_name, node);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (out_mat == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        opencv_csharp_native::mat_value(out_mat) = node->value.mat();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)node; (void)out_mat;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_string_list_count(const jyppx_ocv_core_string_list* values, size_t* out_count)
{
    constexpr const char* api_name = "jyppx_ocv_core_string_list_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (values == nullptr || out_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, values == nullptr ? "values" : "out_count");
        }
        *out_count = values->values.size();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_string_list_get(
    const jyppx_ocv_core_string_list* values,
    size_t index,
    jyppx_ocv_core_utf8_result** out_result)
{
    constexpr const char* api_name = "jyppx_ocv_core_string_list_get";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (values == nullptr || index >= values->values.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, values == nullptr ? "values" : "index");
        }
        return opencv_csharp_native::make_core_utf8_result(api_name, values->values[index], out_result);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_core_string_list_release(jyppx_ocv_core_string_list* values)
{
    delete values;
}

int jyppx_ocv_core_utf8_result_size(const jyppx_ocv_core_utf8_result* result, size_t* out_size)
{
    constexpr const char* api_name = "jyppx_ocv_core_utf8_result_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (result == nullptr || out_size == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, result == nullptr ? "result" : "out_size");
        }
        *out_size = result->value.size();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_utf8_result_data(const jyppx_ocv_core_utf8_result* result, const unsigned char** out_data)
{
    constexpr const char* api_name = "jyppx_ocv_core_utf8_result_data";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (result == nullptr || out_data == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, result == nullptr ? "result" : "out_data");
        }
        *out_data = reinterpret_cast<const unsigned char*>(result->value.data());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_core_utf8_result_release(jyppx_ocv_core_utf8_result* result)
{
    delete result;
}
