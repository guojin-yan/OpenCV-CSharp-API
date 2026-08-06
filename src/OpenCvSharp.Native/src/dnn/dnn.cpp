#include "open_cv_sharp/dnn/dnn.h"

#include "../core/mat_handle.h"
#include "../core/utf8_result_handle.h"
#include "../error_state.h"
#include "dnn_handles.h"

#include <algorithm>
#include <cmath>
#include <cstdint>
#include <cstring>
#include <limits>
#include <new>
#include <string>
#include <vector>

#if defined(_WIN32)
#define NOMINMAX
#include <Windows.h>
#endif

namespace
{
    int validate_net(const char* api_name, const jyppx_ocv_dnn_net* net)
    {
        return net == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "net")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_layer(const char* api_name, const jyppx_ocv_dnn_layer* layer)
    {
        return layer == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "layer")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat_groups(const char* api_name, const jyppx_ocv_dnn_mat_groups* result)
    {
        return result == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "result")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int(const char* api_name, const int* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int64(const char* api_name, const long long* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_double(const char* api_name, const double* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_uint64(const char* api_name, const uint64_t* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_engine(const char* api_name, int engine)
    {
        return engine < 1 || engine > 4
            ? opencv_csharp_native::set_invalid_argument(api_name, "engine")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_backend(const char* api_name, int backend)
    {
        return backend != 0 && (backend < 2 || backend > 8)
            ? opencv_csharp_native::set_invalid_argument(api_name, "backend_id")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_target(const char* api_name, int target)
    {
        return target < 0 || target > 10
            ? opencv_csharp_native::set_invalid_argument(api_name, "target_id")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mode(const char* api_name, int mode, const char* argument_name)
    {
        return mode < 0 || mode > 2
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_buffer(const char* api_name, const unsigned char* buffer, int buffer_size, const char* argument_name)
    {
        if (buffer_size < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (buffer_size > 0 && buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat_array(const char* api_name, const jyppx_ocv_mat* const* values, int value_count, const char* argument_name)
    {
        if (value_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (value_count > 0 && values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < value_count; ++i)
        {
            if (values[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_int_array(const char* api_name, const int* values, int value_count, const char* argument_name)
    {
        if (value_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (value_count > 0 && values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    template <typename TBox>
    int validate_nms_arguments(
        const char* api_name,
        const TBox* boxes,
        int box_count,
        const float* scores,
        int score_count,
        float score_threshold,
        float nms_threshold,
        float eta,
        int top_k,
        int* indices,
        int index_capacity,
        int* index_count)
    {
        if (box_count < 0 || score_count != box_count || index_capacity < box_count ||
            (box_count > 0 && (boxes == nullptr || scores == nullptr || indices == nullptr)) ||
            index_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "boxes_scores_or_indices");
        }
        if (!std::isfinite(score_threshold) || !std::isfinite(nms_threshold) || !std::isfinite(eta) ||
            nms_threshold < 0.0F || eta <= 0.0F || top_k < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "thresholds_eta_or_top_k");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat_shape_array(const char* api_name, const int* values, int value_count, const char* argument_name)
    {
        const int status = validate_int_array(api_name, values, value_count, argument_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (value_count > cv::MatShape::MAX_DIMS)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }
#endif

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_string_array_buffer(
        const char* api_name,
        const char* names_buffer,
        const int* name_offsets,
        int name_count)
    {
        if (name_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "name_count");
        }

        if (name_count > 0 && (names_buffer == nullptr || name_offsets == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "names");
        }

        for (int i = 0; i < name_count; ++i)
        {
            if (name_offsets[i] < 0 || name_offsets[i + 1] < name_offsets[i])
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "name_offsets");
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_packed_shapes(
        const char* api_name,
        const int* offsets,
        int shape_count,
        const int* values,
        int value_count,
        const int* types,
        int type_count)
    {
        if (shape_count <= 0 || value_count < 0 || type_count != shape_count ||
            offsets == nullptr || types == nullptr || (value_count > 0 && values == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "input_shapes");
        }
        if (offsets[0] != 0 || offsets[shape_count] != value_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "input_shape_offsets");
        }
        for (int i = 0; i < shape_count; ++i)
        {
            if (offsets[i] < 0 || offsets[i + 1] < offsets[i] || offsets[i + 1] - offsets[i] > 10)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "input_shape_offsets");
            }
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_image_parameters(const char* api_name, const jyppx_ocv_dnn_image2blob_params* parameters)
    {
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
        const bool empty_size = parameters->size_width == 0 && parameters->size_height == 0;
        const bool positive_size = parameters->size_width > 0 && parameters->size_height > 0;
        if ((!empty_size && !positive_size) ||
            (parameters->ddepth != 0 && parameters->ddepth != 5) ||
            (parameters->data_layout != 2 && parameters->data_layout != 4) ||
            parameters->padding_mode < 0 || parameters->padding_mode > 2)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::string safe_string(const char* value)
    {
        return value == nullptr ? std::string() : std::string(value);
    }

    int path_from_utf8(const char* api_name, const char* value, const char* argument_name, std::string& result)
    {
        result = safe_string(value);
#if defined(_WIN32)
        if (!result.empty() && GetACP() != CP_UTF8)
        {
            if (result.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
            const int byte_count = static_cast<int>(result.size());
            const int wide_count = MultiByteToWideChar(CP_UTF8, MB_ERR_INVALID_CHARS, result.data(), byte_count, nullptr, 0);
            if (wide_count <= 0)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
            std::wstring wide(static_cast<size_t>(wide_count), L'\0');
            if (MultiByteToWideChar(CP_UTF8, MB_ERR_INVALID_CHARS, result.data(), byte_count, wide.data(), wide_count) != wide_count)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
            BOOL used_default = FALSE;
            const UINT code_page = GetACP();
            const int encoded_count = WideCharToMultiByte(code_page, WC_NO_BEST_FIT_CHARS, wide.data(), wide_count, nullptr, 0, nullptr, &used_default);
            if (encoded_count <= 0 || used_default)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
            std::string encoded(static_cast<size_t>(encoded_count), '\0');
            used_default = FALSE;
            if (WideCharToMultiByte(code_page, WC_NO_BEST_FIT_CHARS, wide.data(), wide_count, encoded.data(), encoded_count, nullptr, &used_default) != encoded_count || used_default)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
            result = std::move(encoded);
        }
#else
        (void)api_name;
        (void)argument_name;
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }

    std::vector<unsigned char> to_byte_vector(const unsigned char* buffer, int buffer_size)
    {
        return buffer_size <= 0
            ? std::vector<unsigned char>()
            : std::vector<unsigned char>(buffer, buffer + buffer_size);
    }

    std::vector<cv::Mat> to_mat_vector(const jyppx_ocv_mat* const* values, int value_count)
    {
        std::vector<cv::Mat> result;
        result.reserve(static_cast<size_t>(value_count));
        for (int i = 0; i < value_count; ++i)
        {
            result.push_back(opencv_csharp_native::mat_value(values[i]));
        }

        return result;
    }

    std::vector<cv::String> to_string_vector(const char* names_buffer, const int* name_offsets, int name_count)
    {
        std::vector<cv::String> result;
        result.reserve(static_cast<size_t>(name_count));
        for (int i = 0; i < name_count; ++i)
        {
            const int start = name_offsets[i];
            const int end = name_offsets[i + 1];
            result.emplace_back(names_buffer + start, static_cast<size_t>(end - start));
        }

        return result;
    }

    cv::MatShape to_mat_shape(const int* shape, int shape_count)
    {
        cv::MatShape result;
        result.reserve(static_cast<size_t>(shape_count));
        for (int i = 0; i < shape_count; ++i)
        {
            result.push_back(shape[i]);
        }

        return result;
    }

    std::vector<cv::MatShape> to_mat_shapes(
        const int* offsets,
        int shape_count,
        const int* values)
    {
        std::vector<cv::MatShape> result;
        result.reserve(static_cast<size_t>(shape_count));
        for (int i = 0; i < shape_count; ++i)
        {
            const int dimension_count = offsets[i + 1] - offsets[i];
            result.push_back(dimension_count == 0
                ? cv::MatShape()
                : to_mat_shape(values + offsets[i], dimension_count));
        }
        return result;
    }

    std::vector<int> to_int_vector(const int* values, int count)
    {
        return count == 0 ? std::vector<int>() : std::vector<int>(values, values + count);
    }

    cv::dnn::Image2BlobParams to_image_parameters(const jyppx_ocv_dnn_image2blob_params& value)
    {
        return cv::dnn::Image2BlobParams(
            cv::Scalar(value.scale_v0, value.scale_v1, value.scale_v2, value.scale_v3),
            cv::Size(value.size_width, value.size_height),
            cv::Scalar(value.mean_v0, value.mean_v1, value.mean_v2, value.mean_v3),
            value.swap_rb != 0,
            value.ddepth,
            static_cast<cv::DataLayout>(value.data_layout),
            static_cast<cv::dnn::ImagePaddingMode>(value.padding_mode),
            cv::Scalar(value.border_v0, value.border_v1, value.border_v2, value.border_v3));
    }

    cv::Rect to_rect(const jyppx_ocv_dnn_rect& value)
    {
        return cv::Rect(value.x, value.y, value.width, value.height);
    }

    jyppx_ocv_dnn_rect from_rect(const cv::Rect& value)
    {
        return { value.x, value.y, value.width, value.height };
    }

    cv::Rect2d to_rect2d(const jyppx_ocv_dnn_rect2d& value)
    {
        return cv::Rect2d(value.x, value.y, value.width, value.height);
    }

    cv::RotatedRect to_rotated_rect(const jyppx_ocv_dnn_rotated_rect& value)
    {
        return cv::RotatedRect(
            cv::Point2f(value.center_x, value.center_y),
            cv::Size2f(value.width, value.height),
            value.angle);
    }

    template <typename TNativeBox, typename TOpenCvBox, typename TConverter>
    int nms_boxes(
        const char* api_name,
        const TNativeBox* boxes,
        int box_count,
        const float* scores,
        int score_count,
        float score_threshold,
        float nms_threshold,
        float eta,
        int top_k,
        int* indices,
        int index_capacity,
        int* index_count,
        TConverter converter)
    {
        const int status = validate_nms_arguments(
            api_name, boxes, box_count, scores, score_count, score_threshold, nms_threshold,
            eta, top_k, indices, index_capacity, index_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<TOpenCvBox> native_boxes;
        native_boxes.reserve(static_cast<size_t>(box_count));
        for (int index = 0; index < box_count; ++index) native_boxes.push_back(converter(boxes[index]));
        const std::vector<float> native_scores = score_count == 0
            ? std::vector<float>()
            : std::vector<float>(scores, scores + score_count);
        std::vector<int> selected;
        cv::dnn::NMSBoxes(native_boxes, native_scores, score_threshold, nms_threshold, selected, eta, top_k);
        if (selected.size() > static_cast<size_t>(index_capacity))
            return opencv_csharp_native::set_invalid_argument(api_name, "index_capacity");
        *index_count = static_cast<int>(selected.size());
        std::copy(selected.begin(), selected.end(), indices);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)converter;
        *index_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }

    template <typename TNativeBox, typename TOpenCvBox, typename TConverter>
    int nms_boxes_batched(
        const char* api_name,
        const TNativeBox* boxes,
        int box_count,
        const float* scores,
        int score_count,
        const int* class_ids,
        int class_id_count,
        float score_threshold,
        float nms_threshold,
        float eta,
        int top_k,
        int* indices,
        int index_capacity,
        int* index_count,
        TConverter converter)
    {
        int status = validate_nms_arguments(
            api_name, boxes, box_count, scores, score_count, score_threshold, nms_threshold,
            eta, top_k, indices, index_capacity, index_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_int_array(api_name, class_ids, class_id_count, "class_ids");
        if (status != OPENCV_CSHARP_STATUS_OK || class_id_count != box_count)
            return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_invalid_argument(api_name, "class_id_count");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<TOpenCvBox> native_boxes;
        native_boxes.reserve(static_cast<size_t>(box_count));
        for (int index = 0; index < box_count; ++index) native_boxes.push_back(converter(boxes[index]));
        const std::vector<float> native_scores = score_count == 0
            ? std::vector<float>()
            : std::vector<float>(scores, scores + score_count);
        const std::vector<int> native_class_ids = class_id_count == 0
            ? std::vector<int>()
            : std::vector<int>(class_ids, class_ids + class_id_count);
        std::vector<int> selected;
        cv::dnn::NMSBoxesBatched(native_boxes, native_scores, native_class_ids, score_threshold, nms_threshold, selected, eta, top_k);
        if (selected.size() > static_cast<size_t>(index_capacity))
            return opencv_csharp_native::set_invalid_argument(api_name, "index_capacity");
        *index_count = static_cast<int>(selected.size());
        std::copy(selected.begin(), selected.end(), indices);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)converter;
        *index_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }

    int create_net_handle(const char* api_name, const cv::dnn::Net& native, jyppx_ocv_dnn_net** net)
    {
        if (net == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "net");
        }

        *net = nullptr;
        jyppx_ocv_dnn_net* created = new (std::nothrow) jyppx_ocv_dnn_net();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *net = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_layer_handle(const char* api_name, const cv::Ptr<cv::dnn::Layer>& native, jyppx_ocv_dnn_layer** layer)
    {
        if (layer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "layer");
        }
        *layer = nullptr;
        if (native.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "layer_id");
        }
        jyppx_ocv_dnn_layer* created = new (std::nothrow) jyppx_ocv_dnn_layer();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        created->value = native;
        *layer = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_mat_groups_handle(
        const char* api_name,
        std::vector<std::vector<cv::Mat>>&& values,
        jyppx_ocv_dnn_mat_groups** result)
    {
        if (result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "result");
        }
        *result = nullptr;
        jyppx_ocv_dnn_mat_groups* created = new (std::nothrow) jyppx_ocv_dnn_mat_groups();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        created->values = std::move(values);
        *result = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_mat_handle(const char* api_name, const cv::Mat& value, jyppx_ocv_mat** out_mat)
    {
        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "images");
        }

        *out_mat = nullptr;
        jyppx_ocv_mat* created = new (std::nothrow) jyppx_ocv_mat();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = value;
        *out_mat = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int set_string_counts(const char* api_name, const std::vector<cv::String>& values, int* string_count, int* byte_count)
    {
        if (string_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "string_count");
        }

        if (byte_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "byte_count");
        }

        if (values.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "string_count");
        }
        *string_count = static_cast<int>(values.size());
        size_t total = 0;
        for (const cv::String& value : values)
        {
            if (value.size() > static_cast<size_t>(std::numeric_limits<int>::max()) - total)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "byte_count");
            }
            total += value.size();
        }

        *byte_count = static_cast<int>(total);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int set_shape_counts(
        const char* api_name,
        const std::vector<cv::MatShape>& shapes,
        int* shape_count,
        int* value_count)
    {
        if (shape_count == nullptr || value_count == nullptr || shapes.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "shape_counts");
        }
        size_t values = 0;
        for (const cv::MatShape& shape : shapes)
        {
            if (shape.size() > static_cast<size_t>(std::numeric_limits<int>::max()) - values)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "shape_values");
            }
            values += shape.size();
        }
        *shape_count = static_cast<int>(shapes.size());
        *value_count = static_cast<int>(values);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_shapes(
        const char* api_name,
        const std::vector<cv::MatShape>& shapes,
        int* offsets,
        int offset_capacity,
        int* values,
        int value_capacity,
        int* shape_count,
        int* value_count)
    {
        int status = set_shape_counts(api_name, shapes, shape_count, value_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (offset_capacity < *shape_count + 1 || offsets == nullptr || value_capacity < *value_count || (*value_count > 0 && values == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "shape_capacity");
        }
        int cursor = 0;
        offsets[0] = 0;
        for (int i = 0; i < *shape_count; ++i)
        {
            const cv::MatShape& shape = shapes[static_cast<size_t>(i)];
            std::copy(shape.begin(), shape.end(), values + cursor);
            cursor += static_cast<int>(shape.size());
            offsets[i + 1] = cursor;
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_strings(
        const char* api_name,
        const std::vector<cv::String>& values,
        int* offsets,
        int offset_capacity,
        char* buffer,
        int buffer_capacity,
        int* string_count,
        int* byte_count)
    {
        int status = set_string_counts(api_name, values, string_count, byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (offset_capacity < (*string_count + 1) || (offset_capacity > 0 && offsets == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "offsets");
        }

        if (buffer_capacity < *byte_count || (buffer_capacity > 0 && buffer == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        int cursor = 0;
        offsets[0] = 0;
        for (int i = 0; i < *string_count; ++i)
        {
            const cv::String& value = values[static_cast<size_t>(i)];
            if (!value.empty())
            {
                std::memcpy(buffer + cursor, value.data(), value.size());
                cursor += static_cast<int>(value.size());
            }

            offsets[i + 1] = cursor;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_dnn_net_create_empty(jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_create_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (net == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "net");
        }

        *net = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_net_handle(api_name, cv::dnn::Net(), net);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_read_net(const char* model, const char* config, const char* framework, int engine, jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (model == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }
        int status = validate_engine(api_name, engine);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string model_path;
        std::string config_path;
        status = path_from_utf8(api_name, model, "model", model_path);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = path_from_utf8(api_name, config, "config", config_path);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::dnn::Net native = cv::dnn::readNet(model_path, config_path, safe_string(framework), engine);
        return create_net_handle(api_name, native, net);
#else
        (void)config; (void)framework; (void)engine; (void)net;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_read_net_from_buffer(
    const char* framework,
    const unsigned char* model_buffer,
    int model_buffer_size,
    const unsigned char* config_buffer,
    int config_buffer_size,
    int engine,
    jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net_from_buffer";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (framework == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "framework");
        }

        int status = validate_buffer(api_name, model_buffer, model_buffer_size, "model_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_buffer(api_name, config_buffer, config_buffer_size, "config_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_engine(api_name, engine);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::Net native = cv::dnn::readNet(
            safe_string(framework),
            to_byte_vector(model_buffer, model_buffer_size),
            to_byte_vector(config_buffer, config_buffer_size),
            engine);
        return create_net_handle(api_name, native, net);
#else
        (void)engine; (void)net;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_read_net_from_onnx(const char* model, int engine, jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net_from_onnx";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (model == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }
        int status = validate_engine(api_name, engine);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string model_path;
        status = path_from_utf8(api_name, model, "model", model_path);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::dnn::Net native = cv::dnn::readNetFromONNX(model_path, engine);
        return create_net_handle(api_name, native, net);
#else
        (void)engine; (void)net;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_read_net_from_tensorflow(const char* model, const char* config, int engine, jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net_from_tensorflow";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (model == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }
        int status = validate_engine(api_name, engine);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string model_path;
        std::string config_path;
        status = path_from_utf8(api_name, model, "model", model_path);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = path_from_utf8(api_name, config, "config", config_path);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::dnn::Net native = cv::dnn::readNetFromTensorflow(model_path, config_path, engine);
        return create_net_handle(api_name, native, net);
#else
        (void)config; (void)engine; (void)net;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_read_net_from_tflite(const char* model, int engine, jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net_from_tflite";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (model == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }
        int status = validate_engine(api_name, engine);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string model_path;
        status = path_from_utf8(api_name, model, "model", model_path);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::dnn::Net native = cv::dnn::readNetFromTFLite(model_path, engine);
        return create_net_handle(api_name, native, net);
#else
        (void)engine; (void)net;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_read_net_from_model_optimizer(const char* xml, const char* bin, jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net_from_model_optimizer";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (xml == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "xml");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string xml_path;
        std::string bin_path;
        int status = path_from_utf8(api_name, xml, "xml", xml_path);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = path_from_utf8(api_name, bin, "bin", bin_path);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::dnn::Net native = cv::dnn::readNetFromModelOptimizer(xml_path, bin_path);
        return create_net_handle(api_name, native, net);
#else
        (void)bin; (void)net;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_dnn_net_release_handle(jyppx_ocv_dnn_net* net)
{
    delete net;
}

int jyppx_ocv_dnn_net_empty(const jyppx_ocv_dnn_net* net, int* empty)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, empty, "empty");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *empty = net->value.empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *empty = 1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_set_preferable_backend(jyppx_ocv_dnn_net* net, int backend_id)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_set_preferable_backend";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_backend(api_name, backend_id);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.setPreferableBackend(backend_id);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)backend_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_set_preferable_target(jyppx_ocv_dnn_net* net, int target_id)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_set_preferable_target";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_target(api_name, target_id);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.setPreferableTarget(target_id);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)target_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_set_input(
    jyppx_ocv_dnn_net* net,
    const jyppx_ocv_mat* blob,
    const char* name,
    double scale_factor,
    double mean_v0,
    double mean_v1,
    double mean_v2,
    double mean_v3)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_set_input";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, blob, "blob");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.setInput(
            opencv_csharp_native::mat_value(blob),
            safe_string(name),
            scale_factor,
            cv::Scalar(mean_v0, mean_v1, mean_v2, mean_v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)name; (void)scale_factor; (void)mean_v0; (void)mean_v1; (void)mean_v2; (void)mean_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_forward(jyppx_ocv_dnn_net* net, const char* output_name, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_forward";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat result = net->value.forward(safe_string(output_name));
        result.copyTo(opencv_csharp_native::mat_value(output));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)output_name;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_forward_many(
    jyppx_ocv_dnn_net* net,
    const char* names_buffer,
    const int* name_offsets,
    int name_count,
    jyppx_ocv_mat** outputs,
    int output_capacity,
    int* output_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_forward_many";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string_array_buffer(api_name, names_buffer, name_offsets, name_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, output_count, "output_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (output_capacity < 0 || (output_capacity > 0 && outputs == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "outputs");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_outputs;
        net->value.forward(native_outputs, to_string_vector(names_buffer, name_offsets, name_count));
        if (native_outputs.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output_count");
        }
        *output_count = static_cast<int>(native_outputs.size());
        if (output_capacity < *output_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output_capacity");
        }

        std::fill(outputs, outputs + *output_count, nullptr);
        for (int i = 0; i < *output_count; ++i)
        {
            status = create_mat_handle(api_name, native_outputs[static_cast<size_t>(i)], &outputs[i]);
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                for (int j = 0; j < i; ++j) delete outputs[j];
                std::fill(outputs, outputs + *output_count, nullptr);
                return status;
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        *output_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_layer_id(const jyppx_ocv_dnn_net* net, const char* layer_name, int* layer_id)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layer_id";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (layer_name == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "layer_name");
        }
        status = validate_output_int(api_name, layer_id, "layer_id");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *layer_id = net->value.getLayerId(safe_string(layer_name));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *layer_id = -1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_unconnected_out_layers_count(const jyppx_ocv_dnn_net* net, int* layer_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_unconnected_out_layers_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, layer_count, "layer_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *layer_count = static_cast<int>(net->value.getUnconnectedOutLayers().size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *layer_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_unconnected_out_layers_fill(
    const jyppx_ocv_dnn_net* net,
    int* layers,
    int layer_capacity,
    int* layer_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_unconnected_out_layers_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, layer_count, "layer_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (layer_capacity < 0 || (layer_capacity > 0 && layers == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "layers");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<int> native_layers = net->value.getUnconnectedOutLayers();
        *layer_count = static_cast<int>(native_layers.size());
        if (layer_capacity < *layer_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "layer_capacity");
        }

        for (int i = 0; i < *layer_count; ++i)
        {
            layers[i] = native_layers[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        *layer_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_set_inputs_names(
    jyppx_ocv_dnn_net* net,
    const char* names_buffer,
    const int* name_offsets,
    int name_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_set_inputs_names";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string_array_buffer(api_name, names_buffer, name_offsets, name_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.setInputsNames(to_string_vector(names_buffer, name_offsets, name_count));
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

int jyppx_ocv_dnn_net_set_input_shape(
    jyppx_ocv_dnn_net* net,
    const char* input_name,
    const int* shape,
    int shape_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_set_input_shape";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (input_name == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "input_name");
        }
        status = validate_mat_shape_array(api_name, shape, shape_count, "shape");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.setInputShape(safe_string(input_name), to_mat_shape(shape, shape_count));
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

int jyppx_ocv_dnn_net_get_flops(
    const jyppx_ocv_dnn_net* net,
    const int* input_shape,
    int input_shape_count,
    int input_type,
    long long* flops)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_flops";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_shape_array(api_name, input_shape, input_shape_count, "input_shape");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int64(api_name, flops, "flops");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *flops = static_cast<long long>(net->value.getFLOPS(to_mat_shape(input_shape, input_shape_count), input_type));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)input_type;
        *flops = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_layer_flops(
    const jyppx_ocv_dnn_net* net,
    int layer_id,
    const int* input_shape,
    int input_shape_count,
    int input_type,
    long long* flops)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layer_flops";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_shape_array(api_name, input_shape, input_shape_count, "input_shape");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int64(api_name, flops, "flops");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *flops = static_cast<long long>(net->value.getFLOPS(layer_id, to_mat_shape(input_shape, input_shape_count), input_type));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)layer_id; (void)input_type;
        *flops = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_perf_profile_count(jyppx_ocv_dnn_net* net, int* timing_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_perf_profile_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, timing_count, "timing_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<double> timings;
        net->value.getPerfProfile(timings);
        *timing_count = static_cast<int>(timings.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *timing_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_perf_profile_fill(
    jyppx_ocv_dnn_net* net,
    double* timings,
    int timing_capacity,
    int* timing_count,
    long long* tick_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_perf_profile_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, timing_count, "timing_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int64(api_name, tick_count, "tick_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (timing_capacity < 0 || (timing_capacity > 0 && timings == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "timings");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<double> native_timings;
        *tick_count = static_cast<long long>(net->value.getPerfProfile(native_timings));
        *timing_count = static_cast<int>(native_timings.size());
        if (timing_capacity < *timing_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "timing_capacity");
        }

        for (int i = 0; i < *timing_count; ++i)
        {
            timings[i] = native_timings[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        *timing_count = 0;
        *tick_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_layer_names_count(const jyppx_ocv_dnn_net* net, int* string_count, int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layer_names_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_string_counts(api_name, net->value.getLayerNames(), string_count, byte_count);
#else
        if (string_count != nullptr) { *string_count = 0; }
        if (byte_count != nullptr) { *byte_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_layer_names_fill(
    const jyppx_ocv_dnn_net* net,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layer_names_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return copy_strings(api_name, net->value.getLayerNames(), offsets, offset_capacity, buffer, buffer_capacity, string_count, byte_count);
#else
        (void)offsets; (void)offset_capacity; (void)buffer; (void)buffer_capacity;
        if (string_count != nullptr) { *string_count = 0; }
        if (byte_count != nullptr) { *byte_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_unconnected_out_layers_names_count(const jyppx_ocv_dnn_net* net, int* string_count, int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_unconnected_out_layers_names_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_string_counts(api_name, net->value.getUnconnectedOutLayersNames(), string_count, byte_count);
#else
        if (string_count != nullptr) { *string_count = 0; }
        if (byte_count != nullptr) { *byte_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_unconnected_out_layers_names_fill(
    const jyppx_ocv_dnn_net* net,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_unconnected_out_layers_names_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return copy_strings(api_name, net->value.getUnconnectedOutLayersNames(), offsets, offset_capacity, buffer, buffer_capacity, string_count, byte_count);
#else
        (void)offsets; (void)offset_capacity; (void)buffer; (void)buffer_capacity;
        if (string_count != nullptr) { *string_count = 0; }
        if (byte_count != nullptr) { *byte_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_layer_types_count(const jyppx_ocv_dnn_net* net, int* string_count, int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layer_types_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::String> types;
        net->value.getLayerTypes(types);
        return set_string_counts(api_name, types, string_count, byte_count);
#else
        if (string_count != nullptr) { *string_count = 0; }
        if (byte_count != nullptr) { *byte_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_layer_types_fill(
    const jyppx_ocv_dnn_net* net,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layer_types_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::String> types;
        net->value.getLayerTypes(types);
        return copy_strings(api_name, types, offsets, offset_capacity, buffer, buffer_capacity, string_count, byte_count);
#else
        (void)offsets; (void)offset_capacity; (void)buffer; (void)buffer_capacity;
        if (string_count != nullptr) { *string_count = 0; }
        if (byte_count != nullptr) { *byte_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_net_get_layers_count_by_type(const jyppx_ocv_dnn_net* net, const char* layer_type, int* layer_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layers_count_by_type";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (layer_type == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "layer_type");
        }
        status = validate_output_int(api_name, layer_count, "layer_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *layer_count = net->value.getLayersCount(safe_string(layer_type));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *layer_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_blob_from_image(
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
    int ddepth)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_blob_from_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, blob, "blob");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::blobFromImage(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(blob),
            scale_factor,
            cv::Size(size_width, size_height),
            cv::Scalar(mean_v0, mean_v1, mean_v2, mean_v3),
            swap_rb != 0,
            crop != 0,
            ddepth);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)scale_factor; (void)size_width; (void)size_height; (void)mean_v0; (void)mean_v1; (void)mean_v2; (void)mean_v3; (void)swap_rb; (void)crop; (void)ddepth;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_blob_from_images(
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
    int ddepth)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_blob_from_images";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, blob, "blob");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::blobFromImages(
            to_mat_vector(images, image_count),
            opencv_csharp_native::mat_value(blob),
            scale_factor,
            cv::Size(size_width, size_height),
            cv::Scalar(mean_v0, mean_v1, mean_v2, mean_v3),
            swap_rb != 0,
            crop != 0,
            ddepth);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)scale_factor; (void)size_width; (void)size_height; (void)mean_v0; (void)mean_v1; (void)mean_v2; (void)mean_v3; (void)swap_rb; (void)crop; (void)ddepth;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_images_from_blob_count(const jyppx_ocv_mat* blob, int* image_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_images_from_blob_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, blob, "blob");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, image_count, "image_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> images;
        cv::dnn::imagesFromBlob(opencv_csharp_native::mat_value(blob), images);
        *image_count = static_cast<int>(images.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *image_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_images_from_blob_fill(const jyppx_ocv_mat* blob, jyppx_ocv_mat** images, int image_capacity, int* image_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_images_from_blob_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, blob, "blob");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, image_count, "image_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_capacity < 0 || (image_capacity > 0 && images == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "images");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_images;
        cv::dnn::imagesFromBlob(opencv_csharp_native::mat_value(blob), native_images);
        if (native_images.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_count");
        }
        *image_count = static_cast<int>(native_images.size());
        if (image_capacity < *image_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_capacity");
        }

        std::fill(images, images + *image_count, nullptr);
        for (int i = 0; i < *image_count; ++i)
        {
            status = create_mat_handle(api_name, native_images[static_cast<size_t>(i)], &images[i]);
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                for (int j = 0; j < i; ++j) delete images[j];
                std::fill(images, images + *image_count, nullptr);
                return status;
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        *image_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_dnn_get_available_targets_count(int backend_id, int* target_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_get_available_targets_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_backend(api_name, backend_id);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_int(api_name, target_count, "target_count");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::dnn::Target> values = cv::dnn::getAvailableTargets(static_cast<cv::dnn::Backend>(backend_id));
        if (values.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
            return opencv_csharp_native::set_invalid_argument(api_name, "target_count");
        *target_count = static_cast<int>(values.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *target_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_get_available_targets_fill(int backend_id, int* targets, int target_capacity, int* target_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_get_available_targets_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_backend(api_name, backend_id);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_int(api_name, target_count, "target_count");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (target_capacity < 0 || (target_capacity > 0 && targets == nullptr))
            return opencv_csharp_native::set_invalid_argument(api_name, "targets");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::dnn::Target> values = cv::dnn::getAvailableTargets(static_cast<cv::dnn::Backend>(backend_id));
        if (values.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
            return opencv_csharp_native::set_invalid_argument(api_name, "target_count");
        *target_count = static_cast<int>(values.size());
        if (target_capacity < *target_count)
            return opencv_csharp_native::set_invalid_argument(api_name, "target_capacity");
        for (int i = 0; i < *target_count; ++i) targets[i] = static_cast<int>(values[static_cast<size_t>(i)]);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *target_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_read_net_from_tensorflow_ex(
    const char* model,
    const char* config,
    int engine,
    const char* extra_outputs_buffer,
    const int* extra_output_offsets,
    int extra_output_count,
    jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net_from_tensorflow_ex";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (model == nullptr || net == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, model == nullptr ? "model" : "net");
        *net = nullptr;
        int status = validate_engine(api_name, engine);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_string_array_buffer(api_name, extra_outputs_buffer, extra_output_offsets, extra_output_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string model_path;
        std::string config_path;
        status = path_from_utf8(api_name, model, "model", model_path);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = path_from_utf8(api_name, config, "config", config_path);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::dnn::Net value = cv::dnn::readNetFromTensorflow(
            model_path,
            config_path,
            engine,
            to_string_vector(extra_outputs_buffer, extra_output_offsets, extra_output_count));
        return create_net_handle(api_name, value, net);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_read_net_from_tensorflow_buffer(
    const unsigned char* model_buffer,
    int model_buffer_size,
    const unsigned char* config_buffer,
    int config_buffer_size,
    int engine,
    const char* extra_outputs_buffer,
    const int* extra_output_offsets,
    int extra_output_count,
    jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net_from_tensorflow_buffer";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (net == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "net");
        *net = nullptr;
        int status = validate_buffer(api_name, model_buffer, model_buffer_size, "model_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK || model_buffer_size == 0)
            return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_invalid_argument(api_name, "model_buffer");
        status = validate_buffer(api_name, config_buffer, config_buffer_size, "config_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_engine(api_name, engine);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_string_array_buffer(api_name, extra_outputs_buffer, extra_output_offsets, extra_output_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::Net value = cv::dnn::readNetFromTensorflow(
            to_byte_vector(model_buffer, model_buffer_size),
            to_byte_vector(config_buffer, config_buffer_size),
            engine,
            to_string_vector(extra_outputs_buffer, extra_output_offsets, extra_output_count));
        return create_net_handle(api_name, value, net);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_read_net_from_tflite_buffer(const unsigned char* model_buffer, int model_buffer_size, int engine, jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net_from_tflite_buffer";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (net == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "net");
        *net = nullptr;
        int status = validate_buffer(api_name, model_buffer, model_buffer_size, "model_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK || model_buffer_size == 0)
            return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_invalid_argument(api_name, "model_buffer");
        status = validate_engine(api_name, engine);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_net_handle(api_name, cv::dnn::readNetFromTFLite(to_byte_vector(model_buffer, model_buffer_size), engine), net);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_read_net_from_model_optimizer_buffer(
    const unsigned char* model_config_buffer,
    int model_config_buffer_size,
    const unsigned char* weights_buffer,
    int weights_buffer_size,
    jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net_from_model_optimizer_buffer";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (net == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "net");
        *net = nullptr;
        int status = validate_buffer(api_name, model_config_buffer, model_config_buffer_size, "model_config_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK || model_config_buffer_size == 0)
            return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_invalid_argument(api_name, "model_config_buffer");
        status = validate_buffer(api_name, weights_buffer, weights_buffer_size, "weights_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK || weights_buffer_size == 0)
            return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_invalid_argument(api_name, "weights_buffer");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_net_handle(api_name, cv::dnn::readNetFromModelOptimizer(
            to_byte_vector(model_config_buffer, model_config_buffer_size),
            to_byte_vector(weights_buffer, weights_buffer_size)), net);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_read_net_from_onnx_buffer(const unsigned char* model_buffer, int model_buffer_size, int engine, jyppx_ocv_dnn_net** net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_net_from_onnx_buffer";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (net == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "net");
        *net = nullptr;
        int status = validate_buffer(api_name, model_buffer, model_buffer_size, "model_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK || model_buffer_size == 0)
            return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_invalid_argument(api_name, "model_buffer");
        status = validate_engine(api_name, engine);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_net_handle(
            api_name,
            cv::dnn::readNetFromONNX(
                reinterpret_cast<const char*>(model_buffer),
                static_cast<size_t>(model_buffer_size),
                engine),
            net);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_read_tensor_from_onnx(const char* path, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_read_tensor_from_onnx";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (path == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "path");
        int status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string native_path;
        status = path_from_utf8(api_name, path, "path", native_path);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        opencv_csharp_native::mat_value(output) = cv::dnn::readTensorFromONNX(native_path);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_dump(jyppx_ocv_dnn_net* net, jyppx_ocv_core_utf8_result** result)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_dump";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return opencv_csharp_native::make_core_utf8_result(api_name, net->value.dump(), result);
#else
        (void)result;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_dump_to_file(jyppx_ocv_dnn_net* net, const char* path)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_dump_to_file";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (path == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "path");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string native_path;
        status = path_from_utf8(api_name, path, "path", native_path);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        net->value.dumpToFile(native_path);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_dump_to_pbtxt(jyppx_ocv_dnn_net* net, const char* path)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_dump_to_pbtxt";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (path == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "path");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string native_path;
        status = path_from_utf8(api_name, path, "path", native_path);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        net->value.dumpToPbtxt(native_path);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_connect(jyppx_ocv_dnn_net* net, const char* output_pin, const char* input_pin)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_connect";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (output_pin == nullptr || input_pin == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, output_pin == nullptr ? "output_pin" : "input_pin");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.connect(safe_string(output_pin), safe_string(input_pin));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_register_output(jyppx_ocv_dnn_net* net, const char* output_name, int layer_id, int output_port, int* registered_layer_id)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_register_output";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (output_name == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "output_name");
        status = validate_output_int(api_name, registered_layer_id, "registered_layer_id");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *registered_layer_id = net->value.registerOutput(safe_string(output_name), layer_id, output_port);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *registered_layer_id = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_layer_by_id(const jyppx_ocv_dnn_net* net, int layer_id, jyppx_ocv_dnn_layer** layer)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layer_by_id";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_layer_handle(api_name, net->value.getLayer(layer_id), layer);
#else
        (void)layer_id; (void)layer;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_layer_by_name(const jyppx_ocv_dnn_net* net, const char* layer_name, jyppx_ocv_dnn_layer** layer)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layer_by_name";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (layer_name == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "layer_name");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_layer_handle(api_name, net->value.getLayer(safe_string(layer_name)), layer);
#else
        (void)layer;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

void jyppx_ocv_dnn_layer_release_handle(jyppx_ocv_dnn_layer* layer)
{
    delete layer;
}

int jyppx_ocv_dnn_layer_output_name_to_index(jyppx_ocv_dnn_layer* layer, const char* output_name, int* output_index)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_layer_output_name_to_index";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_layer(api_name, layer);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (output_name == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "output_name");
        status = validate_output_int(api_name, output_index, "output_index");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *output_index = layer->value->outputNameToIndex(safe_string(output_name));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *output_index = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_forward_and_retrieve(
    jyppx_ocv_dnn_net* net,
    const char* names_buffer,
    const int* name_offsets,
    int name_count,
    jyppx_ocv_dnn_mat_groups** result)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_forward_and_retrieve";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_string_array_buffer(api_name, names_buffer, name_offsets, name_count);
        if (status != OPENCV_CSHARP_STATUS_OK || name_count == 0)
            return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_invalid_argument(api_name, "name_count");
        if (result == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "result");
        *result = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Mat>> values;
        net->value.forward(values, to_string_vector(names_buffer, name_offsets, name_count));
        return create_mat_groups_handle(api_name, std::move(values), result);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_mat_groups_get_counts(const jyppx_ocv_dnn_mat_groups* result, int* group_count, int* mat_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_mat_groups_get_counts";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat_groups(api_name, result);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_int(api_name, group_count, "group_count");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_int(api_name, mat_count, "mat_count");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (result->values.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
            return opencv_csharp_native::set_invalid_argument(api_name, "group_count");
        size_t total = 0;
        for (const auto& group : result->values)
        {
            if (group.size() > static_cast<size_t>(std::numeric_limits<int>::max()) - total)
                return opencv_csharp_native::set_invalid_argument(api_name, "mat_count");
            total += group.size();
        }
        *group_count = static_cast<int>(result->values.size());
        *mat_count = static_cast<int>(total);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *group_count = 0; *mat_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_mat_groups_get_group_offsets(const jyppx_ocv_dnn_mat_groups* result, int* offsets, int offset_capacity, int* group_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_mat_groups_get_group_offsets";
    try
    {
        opencv_csharp_native::clear_last_error();
        int mat_count = 0;
        int status = jyppx_ocv_dnn_mat_groups_get_counts(result, group_count, &mat_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (offset_capacity < *group_count + 1 || offsets == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "offsets");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int cursor = 0;
        offsets[0] = 0;
        for (int i = 0; i < *group_count; ++i)
        {
            cursor += static_cast<int>(result->values[static_cast<size_t>(i)].size());
            offsets[i + 1] = cursor;
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_mat_groups_take_mats(const jyppx_ocv_dnn_mat_groups* result, jyppx_ocv_mat** mats, int mat_capacity, int* mat_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_mat_groups_take_mats";
    try
    {
        opencv_csharp_native::clear_last_error();
        int group_count = 0;
        int status = jyppx_ocv_dnn_mat_groups_get_counts(result, &group_count, mat_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (mat_capacity < *mat_count || (*mat_count > 0 && mats == nullptr))
            return opencv_csharp_native::set_invalid_argument(api_name, "mats");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (*mat_count > 0) std::fill(mats, mats + *mat_count, nullptr);
        int cursor = 0;
        for (const auto& group : result->values)
        {
            for (const cv::Mat& value : group)
            {
                status = create_mat_handle(api_name, value, &mats[cursor]);
                if (status != OPENCV_CSHARP_STATUS_OK)
                {
                    for (int i = 0; i < cursor; ++i) delete mats[i];
                    if (*mat_count > 0) std::fill(mats, mats + *mat_count, nullptr);
                    return status;
                }
                ++cursor;
            }
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

void jyppx_ocv_dnn_mat_groups_release_handle(jyppx_ocv_dnn_mat_groups* result)
{
    delete result;
}

int jyppx_ocv_dnn_net_finalize(jyppx_ocv_dnn_net* net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_finalize";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.finalizeNet();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_set_tracing_mode(jyppx_ocv_dnn_net* net, int mode)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_set_tracing_mode";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mode(api_name, mode, "mode");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.setTracingMode(static_cast<cv::dnn::TracingMode>(mode));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_tracing_mode(const jyppx_ocv_dnn_net* net, int* mode)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_tracing_mode";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_int(api_name, mode, "mode");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *mode = static_cast<int>(net->value.getTracingMode());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *mode = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_set_profiling_mode(jyppx_ocv_dnn_net* net, int mode)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_set_profiling_mode";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mode(api_name, mode, "mode");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.setProfilingMode(static_cast<cv::dnn::ProfilingMode>(mode));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_profiling_mode(const jyppx_ocv_dnn_net* net, int* mode)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_profiling_mode";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_int(api_name, mode, "mode");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *mode = static_cast<int>(net->value.getProfilingMode());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *mode = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_model_format(const jyppx_ocv_dnn_net* net, int* format)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_model_format";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_int(api_name, format, "format");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *format = static_cast<int>(net->value.getModelFormat());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *format = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_set_param_by_id(jyppx_ocv_dnn_net* net, int layer_id, int parameter_index, const jyppx_ocv_mat* value)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_set_param_by_id";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (parameter_index < 0) return opencv_csharp_native::set_invalid_argument(api_name, "parameter_index");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.setParam(layer_id, parameter_index, opencv_csharp_native::mat_value(value));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_set_param_by_name(jyppx_ocv_dnn_net* net, const char* layer_name, int parameter_index, const jyppx_ocv_mat* value)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_set_param_by_name";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (layer_name == nullptr || parameter_index < 0)
            return opencv_csharp_native::set_invalid_argument(api_name, layer_name == nullptr ? "layer_name" : "parameter_index");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.setParam(safe_string(layer_name), parameter_index, opencv_csharp_native::mat_value(value));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_param_by_id(const jyppx_ocv_dnn_net* net, int layer_id, int parameter_index, jyppx_ocv_mat* value)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_param_by_id";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (parameter_index < 0) return opencv_csharp_native::set_invalid_argument(api_name, "parameter_index");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        opencv_csharp_native::mat_value(value) = net->value.getParam(layer_id, parameter_index);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_param_by_name(const jyppx_ocv_dnn_net* net, const char* layer_name, int parameter_index, jyppx_ocv_mat* value)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_param_by_name";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (layer_name == nullptr || parameter_index < 0)
            return opencv_csharp_native::set_invalid_argument(api_name, layer_name == nullptr ? "layer_name" : "parameter_index");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        opencv_csharp_native::mat_value(value) = net->value.getParam(safe_string(layer_name), parameter_index);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_enable_fusion(jyppx_ocv_dnn_net* net, int enabled)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_enable_fusion";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.enableFusion(enabled != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)enabled;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_enable_winograd(jyppx_ocv_dnn_net* net, int enabled)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_enable_winograd";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        net->value.enableWinograd(enabled != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)enabled;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_enable_kv_cache(jyppx_ocv_dnn_net* net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_enable_kv_cache";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (net->value.getMainGraph().empty())
            return opencv_csharp_native::set_invalid_argument(api_name, "kv_cache_requires_new_engine");
        net->value.enableKVCache(); return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_disable_kv_cache(jyppx_ocv_dnn_net* net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_disable_kv_cache";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (net->value.getMainGraph().empty())
            return opencv_csharp_native::set_invalid_argument(api_name, "kv_cache_requires_new_engine");
        net->value.disableKVCache(); return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_reset_kv_cache(jyppx_ocv_dnn_net* net)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_reset_kv_cache";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (net->value.getMainGraph().empty())
            return opencv_csharp_native::set_invalid_argument(api_name, "kv_cache_requires_new_engine");
        net->value.resetKVCache(); return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_layer_shapes_count(
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
    int* output_layer_value_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layer_shapes_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_packed_shapes(api_name, input_shape_offsets, input_shape_count, input_shape_values, input_value_count, input_types, input_type_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::MatShape> input_shapes = to_mat_shapes(input_shape_offsets, input_shape_count, input_shape_values);
        std::vector<cv::MatShape> in_shapes;
        std::vector<cv::MatShape> out_shapes;
        net->value.getLayerShapes(input_shapes, to_int_vector(input_types, input_type_count), layer_id, in_shapes, out_shapes);
        status = set_shape_counts(api_name, in_shapes, input_layer_shape_count, input_layer_value_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return set_shape_counts(api_name, out_shapes, output_layer_shape_count, output_layer_value_count);
#else
        if (input_layer_shape_count != nullptr) *input_layer_shape_count = 0;
        if (input_layer_value_count != nullptr) *input_layer_value_count = 0;
        if (output_layer_shape_count != nullptr) *output_layer_shape_count = 0;
        if (output_layer_value_count != nullptr) *output_layer_value_count = 0;
        (void)layer_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_layer_shapes_fill(
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
    int* output_layer_value_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_layer_shapes_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_packed_shapes(api_name, input_shape_offsets, input_shape_count, input_shape_values, input_value_count, input_types, input_type_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::MatShape> input_shapes = to_mat_shapes(input_shape_offsets, input_shape_count, input_shape_values);
        std::vector<cv::MatShape> in_shapes;
        std::vector<cv::MatShape> out_shapes;
        net->value.getLayerShapes(input_shapes, to_int_vector(input_types, input_type_count), layer_id, in_shapes, out_shapes);
        status = set_shape_counts(api_name, in_shapes, input_layer_shape_count, input_layer_value_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = set_shape_counts(api_name, out_shapes, output_layer_shape_count, output_layer_value_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (input_layer_offset_capacity < *input_layer_shape_count + 1 || input_layer_offsets == nullptr ||
            input_layer_value_capacity < *input_layer_value_count || (*input_layer_value_count > 0 && input_layer_values == nullptr) ||
            output_layer_offset_capacity < *output_layer_shape_count + 1 || output_layer_offsets == nullptr ||
            output_layer_value_capacity < *output_layer_value_count || (*output_layer_value_count > 0 && output_layer_values == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output_capacity");
        }
        status = copy_shapes(api_name, in_shapes, input_layer_offsets, input_layer_offset_capacity, input_layer_values, input_layer_value_capacity, input_layer_shape_count, input_layer_value_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return copy_shapes(api_name, out_shapes, output_layer_offsets, output_layer_offset_capacity, output_layer_values, output_layer_value_capacity, output_layer_shape_count, output_layer_value_count);
#else
        if (input_layer_shape_count != nullptr) *input_layer_shape_count = 0;
        if (input_layer_value_count != nullptr) *input_layer_value_count = 0;
        if (output_layer_shape_count != nullptr) *output_layer_shape_count = 0;
        if (output_layer_value_count != nullptr) *output_layer_value_count = 0;
        (void)layer_id; (void)input_layer_offsets; (void)input_layer_offset_capacity; (void)input_layer_values; (void)input_layer_value_capacity;
        (void)output_layer_offsets; (void)output_layer_offset_capacity; (void)output_layer_values; (void)output_layer_value_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_flops_many(
    const jyppx_ocv_dnn_net* net,
    const int* input_shape_offsets,
    int input_shape_count,
    const int* input_shape_values,
    int input_value_count,
    const int* input_types,
    int input_type_count,
    int64_t* flops)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_flops_many";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_packed_shapes(api_name, input_shape_offsets, input_shape_count, input_shape_values, input_value_count, input_types, input_type_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (flops == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "flops");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *flops = static_cast<int64_t>(net->value.getFLOPS(
            to_mat_shapes(input_shape_offsets, input_shape_count, input_shape_values),
            to_int_vector(input_types, input_type_count)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *flops = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_memory_consumption(
    const jyppx_ocv_dnn_net* net,
    const int* input_shape_offsets,
    int input_shape_count,
    const int* input_shape_values,
    int input_value_count,
    const int* input_types,
    int input_type_count,
    uint64_t* weights_bytes,
    uint64_t* blob_bytes)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_memory_consumption";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_packed_shapes(api_name, input_shape_offsets, input_shape_count, input_shape_values, input_value_count, input_types, input_type_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_uint64(api_name, weights_bytes, "weights_bytes");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_uint64(api_name, blob_bytes, "blob_bytes");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        size_t weights = 0;
        size_t blobs = 0;
        net->value.getMemoryConsumption(
            to_mat_shapes(input_shape_offsets, input_shape_count, input_shape_values),
            to_int_vector(input_types, input_type_count),
            weights,
            blobs);
        *weights_bytes = static_cast<uint64_t>(weights);
        *blob_bytes = static_cast<uint64_t>(blobs);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *weights_bytes = 0; *blob_bytes = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_detailed_perf_profile_count(
    const jyppx_ocv_dnn_net* net,
    int* row_count,
    int* name_byte_count,
    int* time_byte_count,
    int* invocation_byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_detailed_perf_profile_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> names;
        std::vector<std::string> times;
        std::vector<std::string> invocations;
        net->value.getPerfProfile(names, times, invocations);
        if (names.size() != times.size() || names.size() != invocations.size())
            return opencv_csharp_native::set_invalid_argument(api_name, "profile_rows");
        int name_count = 0;
        int time_count = 0;
        int invocation_count = 0;
        status = set_string_counts(api_name, names, &name_count, name_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = set_string_counts(api_name, times, &time_count, time_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = set_string_counts(api_name, invocations, &invocation_count, invocation_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (row_count == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "row_count");
        *row_count = name_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        if (row_count != nullptr) *row_count = 0;
        if (name_byte_count != nullptr) *name_byte_count = 0;
        if (time_byte_count != nullptr) *time_byte_count = 0;
        if (invocation_byte_count != nullptr) *invocation_byte_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_net_get_detailed_perf_profile_fill(
    const jyppx_ocv_dnn_net* net,
    int* name_offsets,
    int name_offset_capacity,
    char* names_buffer,
    int name_capacity,
    int* time_offsets,
    int time_offset_capacity,
    char* times_buffer,
    int time_capacity,
    int* invocation_offsets,
    int invocation_offset_capacity,
    char* invocation_buffer,
    int invocation_capacity,
    int* row_count,
    int* name_byte_count,
    int* time_byte_count,
    int* invocation_byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_net_get_detailed_perf_profile_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_net(api_name, net);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> names;
        std::vector<std::string> times;
        std::vector<std::string> invocations;
        net->value.getPerfProfile(names, times, invocations);
        if (names.size() != times.size() || names.size() != invocations.size())
            return opencv_csharp_native::set_invalid_argument(api_name, "profile_rows");
        int name_count = 0;
        int time_count = 0;
        int invocation_count = 0;
        status = set_string_counts(api_name, names, &name_count, name_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = set_string_counts(api_name, times, &time_count, time_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = set_string_counts(api_name, invocations, &invocation_count, invocation_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (row_count == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "row_count");
        *row_count = name_count;
        if (name_offset_capacity < name_count + 1 || name_offsets == nullptr || name_capacity < *name_byte_count || (*name_byte_count > 0 && names_buffer == nullptr) ||
            time_offset_capacity < time_count + 1 || time_offsets == nullptr || time_capacity < *time_byte_count || (*time_byte_count > 0 && times_buffer == nullptr) ||
            invocation_offset_capacity < invocation_count + 1 || invocation_offsets == nullptr || invocation_capacity < *invocation_byte_count || (*invocation_byte_count > 0 && invocation_buffer == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "profile_capacity");
        }
        status = copy_strings(api_name, names, name_offsets, name_offset_capacity, names_buffer, name_capacity, &name_count, name_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = copy_strings(api_name, times, time_offsets, time_offset_capacity, times_buffer, time_capacity, &time_count, time_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return copy_strings(api_name, invocations, invocation_offsets, invocation_offset_capacity, invocation_buffer, invocation_capacity, &invocation_count, invocation_byte_count);
#else
        if (row_count != nullptr) *row_count = 0;
        if (name_byte_count != nullptr) *name_byte_count = 0;
        if (time_byte_count != nullptr) *time_byte_count = 0;
        if (invocation_byte_count != nullptr) *invocation_byte_count = 0;
        (void)name_offsets; (void)name_offset_capacity; (void)names_buffer; (void)name_capacity;
        (void)time_offsets; (void)time_offset_capacity; (void)times_buffer; (void)time_capacity;
        (void)invocation_offsets; (void)invocation_offset_capacity; (void)invocation_buffer; (void)invocation_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_blob_from_image_with_params(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* blob,
    const jyppx_ocv_dnn_image2blob_params* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_blob_from_image_with_params";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, blob, "blob");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_image_parameters(api_name, parameters);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::blobFromImageWithParams(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(blob),
            to_image_parameters(*parameters));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_blob_from_images_with_params(
    const jyppx_ocv_mat* const* images,
    int image_count,
    jyppx_ocv_mat* blob,
    const jyppx_ocv_dnn_image2blob_params* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_blob_from_images_with_params";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK || image_count == 0)
            return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_invalid_argument(api_name, "images");
        status = validate_mat(api_name, blob, "blob");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_image_parameters(api_name, parameters);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::blobFromImagesWithParams(
            to_mat_vector(images, image_count),
            opencv_csharp_native::mat_value(blob),
            to_image_parameters(*parameters));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_blob_rect_to_image_rect(
    const jyppx_ocv_dnn_image2blob_params* parameters,
    const jyppx_ocv_dnn_rect* blob_rect,
    int image_width,
    int image_height,
    jyppx_ocv_dnn_rect* image_rect)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_blob_rect_to_image_rect";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_image_parameters(api_name, parameters);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (blob_rect == nullptr || image_rect == nullptr || image_width <= 0 || image_height <= 0)
        {
            const char* argument = blob_rect == nullptr ? "blob_rect" : image_rect == nullptr ? "image_rect" : "image_size";
            return opencv_csharp_native::set_invalid_argument(api_name, argument);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::Image2BlobParams value = to_image_parameters(*parameters);
        *image_rect = from_rect(value.blobRectToImageRect(to_rect(*blob_rect), cv::Size(image_width, image_height)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_blob_rects_to_image_rects(
    const jyppx_ocv_dnn_image2blob_params* parameters,
    const jyppx_ocv_dnn_rect* blob_rects,
    int blob_rect_count,
    int image_width,
    int image_height,
    jyppx_ocv_dnn_rect* image_rects,
    int image_rect_capacity,
    int* image_rect_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_blob_rects_to_image_rects";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_image_parameters(api_name, parameters);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_int(api_name, image_rect_count, "image_rect_count");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (blob_rect_count < 0 || (blob_rect_count > 0 && blob_rects == nullptr) || image_width <= 0 || image_height <= 0 ||
            image_rect_capacity < blob_rect_count || (blob_rect_count > 0 && image_rects == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rectangles_or_capacity");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Rect> native_blob_rects;
        native_blob_rects.reserve(static_cast<size_t>(blob_rect_count));
        for (int i = 0; i < blob_rect_count; ++i) native_blob_rects.push_back(to_rect(blob_rects[i]));
        std::vector<cv::Rect> native_image_rects;
        cv::dnn::Image2BlobParams value = to_image_parameters(*parameters);
        value.blobRectsToImageRects(native_blob_rects, native_image_rects, cv::Size(image_width, image_height));
        if (native_image_rects.size() != static_cast<size_t>(blob_rect_count))
            return opencv_csharp_native::set_invalid_argument(api_name, "image_rect_count");
        *image_rect_count = blob_rect_count;
        for (int i = 0; i < blob_rect_count; ++i) image_rects[i] = from_rect(native_image_rects[static_cast<size_t>(i)]);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *image_rect_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_nms_boxes_rect(
    const jyppx_ocv_dnn_rect* boxes, int box_count,
    const float* scores, int score_count,
    float score_threshold, float nms_threshold, float eta, int top_k,
    int* indices, int index_capacity, int* index_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_nms_boxes_rect";
    try
    {
        opencv_csharp_native::clear_last_error();
        return nms_boxes<jyppx_ocv_dnn_rect, cv::Rect>(
            api_name, boxes, box_count, scores, score_count, score_threshold, nms_threshold,
            eta, top_k, indices, index_capacity, index_count, to_rect);
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_nms_boxes_rect2d(
    const jyppx_ocv_dnn_rect2d* boxes, int box_count,
    const float* scores, int score_count,
    float score_threshold, float nms_threshold, float eta, int top_k,
    int* indices, int index_capacity, int* index_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_nms_boxes_rect2d";
    try
    {
        opencv_csharp_native::clear_last_error();
        return nms_boxes<jyppx_ocv_dnn_rect2d, cv::Rect2d>(
            api_name, boxes, box_count, scores, score_count, score_threshold, nms_threshold,
            eta, top_k, indices, index_capacity, index_count, to_rect2d);
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_nms_boxes_rotated_rect(
    const jyppx_ocv_dnn_rotated_rect* boxes, int box_count,
    const float* scores, int score_count,
    float score_threshold, float nms_threshold, float eta, int top_k,
    int* indices, int index_capacity, int* index_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_nms_boxes_rotated_rect";
    try
    {
        opencv_csharp_native::clear_last_error();
        return nms_boxes<jyppx_ocv_dnn_rotated_rect, cv::RotatedRect>(
            api_name, boxes, box_count, scores, score_count, score_threshold, nms_threshold,
            eta, top_k, indices, index_capacity, index_count, to_rotated_rect);
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_nms_boxes_batched_rect(
    const jyppx_ocv_dnn_rect* boxes, int box_count,
    const float* scores, int score_count,
    const int* class_ids, int class_id_count,
    float score_threshold, float nms_threshold, float eta, int top_k,
    int* indices, int index_capacity, int* index_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_nms_boxes_batched_rect";
    try
    {
        opencv_csharp_native::clear_last_error();
        return nms_boxes_batched<jyppx_ocv_dnn_rect, cv::Rect>(
            api_name, boxes, box_count, scores, score_count, class_ids, class_id_count,
            score_threshold, nms_threshold, eta, top_k, indices, index_capacity, index_count, to_rect);
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_nms_boxes_batched_rect2d(
    const jyppx_ocv_dnn_rect2d* boxes, int box_count,
    const float* scores, int score_count,
    const int* class_ids, int class_id_count,
    float score_threshold, float nms_threshold, float eta, int top_k,
    int* indices, int index_capacity, int* index_count)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_nms_boxes_batched_rect2d";
    try
    {
        opencv_csharp_native::clear_last_error();
        return nms_boxes_batched<jyppx_ocv_dnn_rect2d, cv::Rect2d>(
            api_name, boxes, box_count, scores, score_count, class_ids, class_id_count,
            score_threshold, nms_threshold, eta, top_k, indices, index_capacity, index_count, to_rect2d);
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_dnn_soft_nms_boxes_rect(
    const jyppx_ocv_dnn_rect* boxes, int box_count,
    const float* scores, int score_count,
    float score_threshold, float nms_threshold,
    float* updated_scores, int updated_score_capacity, int* updated_score_count,
    int* indices, int index_capacity, int* index_count,
    int top_k, float sigma, int method)
{
    constexpr const char* api_name = "jyppx_ocv_dnn_soft_nms_boxes_rect";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_nms_arguments(
            api_name, boxes, box_count, scores, score_count, score_threshold, nms_threshold,
            1.0F, top_k, indices, index_capacity, index_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (updated_score_count == nullptr || updated_score_capacity < box_count ||
            (box_count > 0 && updated_scores == nullptr) || !std::isfinite(sigma) || sigma < 0.0F ||
            (method != 1 && method != 2))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "updated_scores_sigma_or_method");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Rect> native_boxes;
        native_boxes.reserve(static_cast<size_t>(box_count));
        for (int index = 0; index < box_count; ++index) native_boxes.push_back(to_rect(boxes[index]));
        const std::vector<float> native_scores = score_count == 0
            ? std::vector<float>()
            : std::vector<float>(scores, scores + score_count);
        std::vector<float> adjusted_scores;
        std::vector<int> selected;
        cv::dnn::softNMSBoxes(
            native_boxes, native_scores, adjusted_scores, score_threshold, nms_threshold, selected,
            static_cast<size_t>(top_k), sigma, static_cast<cv::dnn::SoftNMSMethod>(method));
        if (selected.size() > static_cast<size_t>(index_capacity) ||
            adjusted_scores.size() > static_cast<size_t>(updated_score_capacity))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output_capacity");
        }
        *index_count = static_cast<int>(selected.size());
        *updated_score_count = static_cast<int>(adjusted_scores.size());
        std::copy(selected.begin(), selected.end(), indices);
        std::copy(adjusted_scores.begin(), adjusted_scores.end(), updated_scores);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *index_count = 0;
        *updated_score_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

