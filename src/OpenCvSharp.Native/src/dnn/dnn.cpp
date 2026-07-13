#include "open_cv_sharp/dnn/dnn.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "dnn_handles.h"

#include <cstring>
#include <new>
#include <string>
#include <vector>

namespace
{
    int validate_net(const char* api_name, const jyppx_ocv_dnn_net* net)
    {
        return net == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "net")
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

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::string safe_string(const char* value)
    {
        return value == nullptr ? std::string() : std::string(value);
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

    int create_mat_handle(const char* api_name, const cv::Mat& value, jyppx_ocv_mat** out_mat)
    {
        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "images");
        }

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

        *string_count = static_cast<int>(values.size());
        int total = 0;
        for (const cv::String& value : values)
        {
            total += static_cast<int>(value.size());
        }

        *byte_count = total;
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
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::Net native = cv::dnn::readNet(safe_string(model), safe_string(config), safe_string(framework), engine);
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
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::Net native = cv::dnn::readNetFromONNX(safe_string(model), engine);
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
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::Net native = cv::dnn::readNetFromTensorflow(safe_string(model), safe_string(config), engine);
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
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::dnn::Net native = cv::dnn::readNetFromTFLite(safe_string(model), engine);
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
        cv::dnn::Net native = cv::dnn::readNetFromModelOptimizer(safe_string(xml), safe_string(bin));
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
        *output_count = static_cast<int>(native_outputs.size());
        if (output_capacity < *output_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output_capacity");
        }

        for (int i = 0; i < *output_count; ++i)
        {
            status = create_mat_handle(api_name, native_outputs[static_cast<size_t>(i)], &outputs[i]);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
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
        *image_count = static_cast<int>(native_images.size());
        if (image_capacity < *image_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_capacity");
        }

        for (int i = 0; i < *image_count; ++i)
        {
            status = create_mat_handle(api_name, native_images[static_cast<size_t>(i)], &images[i]);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
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

