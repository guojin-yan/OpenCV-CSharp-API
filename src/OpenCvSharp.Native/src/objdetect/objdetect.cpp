#include "open_cv_sharp/objdetect/objdetect.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "objdetect_handles.h"

#include <cstring>
#include <limits>
#include <new>
#include <string>
#include <vector>

namespace
{
    int validate_qrcode_detector(const char* api_name, const jyppx_ocv_qrcode_detector* detector)
    {
        return detector == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "detector")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_barcode_detector(const char* api_name, const jyppx_ocv_barcode_detector* detector)
    {
        return detector == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "detector")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_qrcode_detector_aruco(const char* api_name, const jyppx_ocv_qrcode_detector_aruco* detector)
    {
        return detector == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "detector")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_qrcode_encoder(const char* api_name, const jyppx_ocv_qrcode_encoder* encoder)
    {
        return encoder == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "encoder")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_face_detector(const char* api_name, const jyppx_ocv_face_detector_yn* detector)
    {
        return detector == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "detector")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_face_recognizer(const char* api_name, const jyppx_ocv_face_recognizer_sf* recognizer)
    {
        return recognizer == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "recognizer")
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

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
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

    int validate_required_string(const char* api_name, const char* value, const char* argument_name)
    {
        if (value == nullptr || value[0] == '\0')
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_float_array(const char* api_name, const float* values, int value_count, const char* argument_name)
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

    int validate_positive_size(const char* api_name, int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "input_size");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_buffer(const char* api_name, const unsigned char* buffer, int buffer_length, const char* argument_name)
    {
        if (buffer_length < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (buffer_length > 0 && buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    void assign_bool(int* destination, bool value)
    {
        if (destination != nullptr)
        {
            *destination = value ? 1 : 0;
        }
    }

    void set_default_qrcode_detector_aruco_params(jyppx_ocv_qrcode_detector_aruco_params* params)
    {
        if (params == nullptr)
        {
            return;
        }

        params->min_module_size_in_pyramid = 4.0F;
        params->max_rotation = 0.261799395F;
        params->max_module_size_mismatch = 1.75F;
        params->max_timing_pattern_mismatch = 2.0F;
        params->max_penalties = 0.4F;
        params->max_colors_mismatch = 0.2F;
        params->scale_timing_pattern_score = 0.9F;
    }

    void set_default_qrcode_encoder_params(jyppx_ocv_qrcode_encoder_params* params)
    {
        if (params == nullptr)
        {
            return;
        }

        params->version = 0;
        params->correction_level = 0;
        params->mode = -1;
        params->structure_number = 1;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::QRCodeDetectorAruco::Params to_aruco_params(const jyppx_ocv_qrcode_detector_aruco_params& source)
    {
        cv::QRCodeDetectorAruco::Params params;
        params.minModuleSizeInPyramid = source.min_module_size_in_pyramid;
        params.maxRotation = source.max_rotation;
        params.maxModuleSizeMismatch = source.max_module_size_mismatch;
        params.maxTimingPatternMismatch = source.max_timing_pattern_mismatch;
        params.maxPenalties = source.max_penalties;
        params.maxColorsMismatch = source.max_colors_mismatch;
        params.scaleTimingPatternScore = source.scale_timing_pattern_score;
        return params;
    }

    jyppx_ocv_qrcode_detector_aruco_params from_aruco_params(const cv::QRCodeDetectorAruco::Params& source)
    {
        jyppx_ocv_qrcode_detector_aruco_params params{};
        params.min_module_size_in_pyramid = source.minModuleSizeInPyramid;
        params.max_rotation = source.maxRotation;
        params.max_module_size_mismatch = source.maxModuleSizeMismatch;
        params.max_timing_pattern_mismatch = source.maxTimingPatternMismatch;
        params.max_penalties = source.maxPenalties;
        params.max_colors_mismatch = source.maxColorsMismatch;
        params.scale_timing_pattern_score = source.scaleTimingPatternScore;
        return params;
    }

    cv::QRCodeEncoder::Params to_encoder_params(const jyppx_ocv_qrcode_encoder_params& source)
    {
        cv::QRCodeEncoder::Params params;
        params.version = source.version;
        params.correction_level = static_cast<cv::QRCodeEncoder::CorrectionLevel>(source.correction_level);
        params.mode = static_cast<cv::QRCodeEncoder::EncodeMode>(source.mode);
        params.structure_number = source.structure_number;
        return params;
    }

    int copy_string_to_output(const char* api_name, const std::string& source, char* buffer, int buffer_capacity, int* written)
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
            std::memcpy(buffer, source.data(), static_cast<size_t>(*written));
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int set_string_length(const char* api_name, const std::string& source, int* length)
    {
        if (length == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "length");
        }

        if (source.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "source");
        }

        *length = static_cast<int>(source.size());
        return OPENCV_CSHARP_STATUS_OK;
    }

    int set_multi_string_counts(
        const char* api_name,
        bool decoded_value,
        const std::vector<std::string>& decoded_info,
        int* decoded,
        int* string_count,
        int* byte_count)
    {
        if (decoded == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "decoded");
        }

        if (string_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "string_count");
        }

        if (byte_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "byte_count");
        }

        if (decoded_info.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "decoded_info");
        }

        size_t total_bytes = 0;
        for (const std::string& value : decoded_info)
        {
            total_bytes += value.size();
            if (total_bytes > static_cast<size_t>(std::numeric_limits<int>::max()))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "decoded_info");
            }
        }

        assign_bool(decoded, decoded_value);
        *string_count = static_cast<int>(decoded_info.size());
        *byte_count = static_cast<int>(total_bytes);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_multi_strings_to_output(
        const char* api_name,
        bool decoded_value,
        const std::vector<std::string>& decoded_info,
        int* offsets,
        int offset_capacity,
        char* buffer,
        int buffer_capacity,
        int* decoded,
        int* string_count,
        int* byte_count)
    {
        int status = set_multi_string_counts(api_name, decoded_value, decoded_info, decoded, string_count, byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (offsets == nullptr || offset_capacity < (*string_count + 1))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "offsets");
        }

        if (buffer == nullptr || buffer_capacity < *byte_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        int offset = 0;
        offsets[0] = 0;
        for (int i = 0; i < *string_count; ++i)
        {
            const std::string& value = decoded_info[static_cast<size_t>(i)];
            if (!value.empty())
            {
                std::memcpy(buffer + offset, value.data(), value.size());
                offset += static_cast<int>(value.size());
            }

            offsets[i + 1] = offset;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int set_string_pair_counts(
        const char* api_name,
        bool decoded_value,
        const std::vector<std::string>& decoded_info,
        const std::vector<std::string>& decoded_type,
        int* decoded,
        int* info_count,
        int* info_byte_count,
        int* type_count,
        int* type_byte_count)
    {
        int status = set_multi_string_counts(api_name, decoded_value, decoded_info, decoded, info_count, info_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        int ignored_decoded = 0;
        return set_multi_string_counts(api_name, decoded_value, decoded_type, &ignored_decoded, type_count, type_byte_count);
    }

    int copy_string_pair_to_output(
        const char* api_name,
        bool decoded_value,
        const std::vector<std::string>& decoded_info,
        const std::vector<std::string>& decoded_type,
        int* info_offsets,
        int info_offset_capacity,
        char* info_buffer,
        int info_buffer_capacity,
        int* type_offsets,
        int type_offset_capacity,
        char* type_buffer,
        int type_buffer_capacity,
        int* decoded,
        int* info_count,
        int* info_byte_count,
        int* type_count,
        int* type_byte_count)
    {
        int status = set_string_pair_counts(
            api_name,
            decoded_value,
            decoded_info,
            decoded_type,
            decoded,
            info_count,
            info_byte_count,
            type_count,
            type_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        int ignored_decoded = 0;
        status = copy_multi_strings_to_output(
            api_name,
            decoded_value,
            decoded_info,
            info_offsets,
            info_offset_capacity,
            info_buffer,
            info_buffer_capacity,
            &ignored_decoded,
            info_count,
            info_byte_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        return copy_multi_strings_to_output(
            api_name,
            decoded_value,
            decoded_type,
            type_offsets,
            type_offset_capacity,
            type_buffer,
            type_buffer_capacity,
            &ignored_decoded,
            type_count,
            type_byte_count);
    }

    std::vector<unsigned char> to_vector(const unsigned char* buffer, int buffer_length)
    {
        return buffer_length <= 0
            ? std::vector<unsigned char>()
            : std::vector<unsigned char>(buffer, buffer + buffer_length);
    }

    std::string qrcode_decode(
        const jyppx_ocv_qrcode_detector* detector,
        const jyppx_ocv_mat* image,
        const jyppx_ocv_mat* points,
        jyppx_ocv_mat* straight_qrcode)
    {
        return straight_qrcode == nullptr
            ? detector->value.decode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points))
            : detector->value.decode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), opencv_csharp_native::mat_value(straight_qrcode));
    }

    std::string qrcode_detect_and_decode(
        const jyppx_ocv_qrcode_detector* detector,
        const jyppx_ocv_mat* image,
        jyppx_ocv_mat* points,
        jyppx_ocv_mat* straight_qrcode)
    {
        if (points == nullptr && straight_qrcode == nullptr)
        {
            return detector->value.detectAndDecode(opencv_csharp_native::mat_value(image));
        }

        if (straight_qrcode == nullptr)
        {
            return detector->value.detectAndDecode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points));
        }

        return detector->value.detectAndDecode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), opencv_csharp_native::mat_value(straight_qrcode));
    }

    std::string qrcode_decode_curved(
        jyppx_ocv_qrcode_detector* detector,
        const jyppx_ocv_mat* image,
        const jyppx_ocv_mat* points,
        jyppx_ocv_mat* straight_qrcode)
    {
        return straight_qrcode == nullptr
            ? detector->value.decodeCurved(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points))
            : detector->value.decodeCurved(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), opencv_csharp_native::mat_value(straight_qrcode));
    }

    std::string qrcode_detect_and_decode_curved(
        jyppx_ocv_qrcode_detector* detector,
        const jyppx_ocv_mat* image,
        jyppx_ocv_mat* points,
        jyppx_ocv_mat* straight_qrcode)
    {
        if (points == nullptr && straight_qrcode == nullptr)
        {
            return detector->value.detectAndDecodeCurved(opencv_csharp_native::mat_value(image));
        }

        if (straight_qrcode == nullptr)
        {
            return detector->value.detectAndDecodeCurved(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points));
        }

        return detector->value.detectAndDecodeCurved(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), opencv_csharp_native::mat_value(straight_qrcode));
    }

    std::vector<float> to_float_vector(const float* values, int value_count)
    {
        return value_count <= 0
            ? std::vector<float>()
            : std::vector<float>(values, values + value_count);
    }

    std::vector<std::string> to_single_decoded_string(const std::string& decoded)
    {
        return decoded.empty()
            ? std::vector<std::string>()
            : std::vector<std::string>{ decoded };
    }

    std::string qrcode_aruco_decode(
        const jyppx_ocv_qrcode_detector_aruco* detector,
        const jyppx_ocv_mat* image,
        const jyppx_ocv_mat* points,
        jyppx_ocv_mat* straight_qrcode)
    {
        return straight_qrcode == nullptr
            ? detector->value.decode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points))
            : detector->value.decode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), opencv_csharp_native::mat_value(straight_qrcode));
    }

    std::string qrcode_aruco_detect_and_decode(
        const jyppx_ocv_qrcode_detector_aruco* detector,
        const jyppx_ocv_mat* image,
        jyppx_ocv_mat* points,
        jyppx_ocv_mat* straight_qrcode)
    {
        if (points == nullptr && straight_qrcode == nullptr)
        {
            return detector->value.detectAndDecode(opencv_csharp_native::mat_value(image));
        }

        if (straight_qrcode == nullptr)
        {
            return detector->value.detectAndDecode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points));
        }

        return detector->value.detectAndDecode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), opencv_csharp_native::mat_value(straight_qrcode));
    }
#endif
}

int jyppx_ocv_qrcode_detector_create(jyppx_ocv_qrcode_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_qrcode_detector* created = new (std::nothrow) jyppx_ocv_qrcode_detector();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *detector = created;
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

void jyppx_ocv_qrcode_detector_release_handle(jyppx_ocv_qrcode_detector* detector)
{
    delete detector;
}

int jyppx_ocv_qrcode_detector_set_eps_x(jyppx_ocv_qrcode_detector* detector, double eps_x)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_set_eps_x";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setEpsX(eps_x);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)eps_x;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_set_eps_y(jyppx_ocv_qrcode_detector* detector, double eps_y)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_set_eps_y";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setEpsY(eps_y);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)eps_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_set_use_alignment_markers(jyppx_ocv_qrcode_detector* detector, int use_alignment_markers)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_set_use_alignment_markers";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setUseAlignmentMarkers(use_alignment_markers != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)use_alignment_markers;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_detect(const jyppx_ocv_qrcode_detector* detector, const jyppx_ocv_mat* image, jyppx_ocv_mat* points, int* detected)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_detect";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, detected, "detected");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(detected, detector->value.detect(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(detected, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_decode_length(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_decode_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_string_length(api_name, qrcode_decode(detector, image, points, straight_qrcode), length);
#else
        (void)straight_qrcode;
        if (length != nullptr) { *length = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_decode_fill(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_decode_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return copy_string_to_output(api_name, qrcode_decode(detector, image, points, straight_qrcode), buffer, buffer_capacity, written);
#else
        (void)straight_qrcode;
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

int jyppx_ocv_qrcode_detector_detect_and_decode_length(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_detect_and_decode_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_string_length(api_name, qrcode_detect_and_decode(detector, image, points, straight_qrcode), length);
#else
        (void)points;
        (void)straight_qrcode;
        if (length != nullptr) { *length = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_detect_and_decode_fill(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_detect_and_decode_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return copy_string_to_output(api_name, qrcode_detect_and_decode(detector, image, points, straight_qrcode), buffer, buffer_capacity, written);
#else
        (void)points;
        (void)straight_qrcode;
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

int jyppx_ocv_qrcode_detector_decode_curved_length(
    jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_decode_curved_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_string_length(api_name, qrcode_decode_curved(detector, image, points, straight_qrcode), length);
#else
        (void)straight_qrcode;
        if (length != nullptr) { *length = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_decode_curved_fill(
    jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_decode_curved_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return copy_string_to_output(api_name, qrcode_decode_curved(detector, image, points, straight_qrcode), buffer, buffer_capacity, written);
#else
        (void)straight_qrcode;
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

int jyppx_ocv_qrcode_detector_detect_and_decode_curved_length(
    jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_detect_and_decode_curved_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_string_length(api_name, qrcode_detect_and_decode_curved(detector, image, points, straight_qrcode), length);
#else
        (void)points;
        (void)straight_qrcode;
        if (length != nullptr) { *length = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_detect_and_decode_curved_fill(
    jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_detect_and_decode_curved_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return copy_string_to_output(api_name, qrcode_detect_and_decode_curved(detector, image, points, straight_qrcode), buffer, buffer_capacity, written);
#else
        (void)points;
        (void)straight_qrcode;
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

int jyppx_ocv_qrcode_detector_detect_multi(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* detected)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_detect_multi";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, detected, "detected");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(detected, detector->value.detectMulti(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(detected, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_decode_multi_count(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_decode_multi_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        bool result = detector->value.decodeMulti(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), decoded_info);
        return set_multi_string_counts(api_name, result, decoded_info, decoded, string_count, byte_count);
#else
        assign_bool(decoded, false);
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

int jyppx_ocv_qrcode_detector_decode_multi_fill(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_decode_multi_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        bool result = detector->value.decodeMulti(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), decoded_info);
        return copy_multi_strings_to_output(api_name, result, decoded_info, offsets, offset_capacity, buffer, buffer_capacity, decoded, string_count, byte_count);
#else
        (void)offsets;
        (void)offset_capacity;
        (void)buffer;
        (void)buffer_capacity;
        assign_bool(decoded, false);
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

int jyppx_ocv_qrcode_detector_detect_and_decode_multi_count(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_detect_and_decode_multi_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        bool result = points == nullptr
            ? detector->value.detectAndDecodeMulti(opencv_csharp_native::mat_value(image), decoded_info)
            : detector->value.detectAndDecodeMulti(opencv_csharp_native::mat_value(image), decoded_info, opencv_csharp_native::mat_value(points));
        return set_multi_string_counts(api_name, result, decoded_info, decoded, string_count, byte_count);
#else
        (void)points;
        assign_bool(decoded, false);
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

int jyppx_ocv_qrcode_detector_detect_and_decode_multi_fill(
    const jyppx_ocv_qrcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_detect_and_decode_multi_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        bool result = points == nullptr
            ? detector->value.detectAndDecodeMulti(opencv_csharp_native::mat_value(image), decoded_info)
            : detector->value.detectAndDecodeMulti(opencv_csharp_native::mat_value(image), decoded_info, opencv_csharp_native::mat_value(points));
        return copy_multi_strings_to_output(api_name, result, decoded_info, offsets, offset_capacity, buffer, buffer_capacity, decoded, string_count, byte_count);
#else
        (void)points;
        (void)offsets;
        (void)offset_capacity;
        (void)buffer;
        (void)buffer_capacity;
        assign_bool(decoded, false);
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

int jyppx_ocv_qrcode_detector_get_encoding(const jyppx_ocv_qrcode_detector* detector, int code_index, int* encoding)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_get_encoding";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, encoding, "encoding");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *encoding = static_cast<int>(const_cast<jyppx_ocv_qrcode_detector*>(detector)->value.getEncoding(code_index));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)code_index;
        *encoding = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_create(jyppx_ocv_barcode_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_barcode_detector* created = new (std::nothrow) jyppx_ocv_barcode_detector();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *detector = created;
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

int jyppx_ocv_barcode_detector_create_with_super_resolution(const char* super_resolution_model_path, jyppx_ocv_barcode_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_create_with_super_resolution";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_required_string(api_name, super_resolution_model_path, "super_resolution_model_path");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_barcode_detector* created = new (std::nothrow) jyppx_ocv_barcode_detector{ cv::barcode::BarcodeDetector(super_resolution_model_path) };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *detector = created;
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

void jyppx_ocv_barcode_detector_release_handle(jyppx_ocv_barcode_detector* detector)
{
    delete detector;
}

int jyppx_ocv_barcode_detector_detect(const jyppx_ocv_barcode_detector* detector, const jyppx_ocv_mat* image, jyppx_ocv_mat* points, int* detected)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_detect";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, detected, "detected");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(detected, detector->value.detect(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(detected, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_decode_count(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_decode_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string decoded_text = detector->value.decode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points));
        std::vector<std::string> decoded_info = to_single_decoded_string(decoded_text);
        return set_multi_string_counts(api_name, !decoded_text.empty(), decoded_info, decoded, string_count, byte_count);
#else
        assign_bool(decoded, false);
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

int jyppx_ocv_barcode_detector_decode_fill(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_decode_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string decoded_text = detector->value.decode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points));
        std::vector<std::string> decoded_info = to_single_decoded_string(decoded_text);
        return copy_multi_strings_to_output(api_name, !decoded_text.empty(), decoded_info, offsets, offset_capacity, buffer, buffer_capacity, decoded, string_count, byte_count);
#else
        (void)offsets;
        (void)offset_capacity;
        (void)buffer;
        (void)buffer_capacity;
        assign_bool(decoded, false);
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

int jyppx_ocv_barcode_detector_decode_with_type_count(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* decoded,
    int* info_count,
    int* info_byte_count,
    int* type_count,
    int* type_byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_decode_with_type_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        std::vector<std::string> decoded_type;
        bool result = detector->value.decodeWithType(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), decoded_info, decoded_type);
        return set_string_pair_counts(api_name, result, decoded_info, decoded_type, decoded, info_count, info_byte_count, type_count, type_byte_count);
#else
        assign_bool(decoded, false);
        if (info_count != nullptr) { *info_count = 0; }
        if (info_byte_count != nullptr) { *info_byte_count = 0; }
        if (type_count != nullptr) { *type_count = 0; }
        if (type_byte_count != nullptr) { *type_byte_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_decode_with_type_fill(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* info_offsets,
    int info_offset_capacity,
    char* info_buffer,
    int info_buffer_capacity,
    int* type_offsets,
    int type_offset_capacity,
    char* type_buffer,
    int type_buffer_capacity,
    int* decoded,
    int* info_count,
    int* info_byte_count,
    int* type_count,
    int* type_byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_decode_with_type_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        std::vector<std::string> decoded_type;
        bool result = detector->value.decodeWithType(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), decoded_info, decoded_type);
        return copy_string_pair_to_output(
            api_name,
            result,
            decoded_info,
            decoded_type,
            info_offsets,
            info_offset_capacity,
            info_buffer,
            info_buffer_capacity,
            type_offsets,
            type_offset_capacity,
            type_buffer,
            type_buffer_capacity,
            decoded,
            info_count,
            info_byte_count,
            type_count,
            type_byte_count);
#else
        (void)info_offsets;
        (void)info_offset_capacity;
        (void)info_buffer;
        (void)info_buffer_capacity;
        (void)type_offsets;
        (void)type_offset_capacity;
        (void)type_buffer;
        (void)type_buffer_capacity;
        assign_bool(decoded, false);
        if (info_count != nullptr) { *info_count = 0; }
        if (info_byte_count != nullptr) { *info_byte_count = 0; }
        if (type_count != nullptr) { *type_count = 0; }
        if (type_byte_count != nullptr) { *type_byte_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_detect_and_decode_count(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_detect_and_decode_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string decoded_text = points == nullptr
            ? detector->value.detectAndDecode(opencv_csharp_native::mat_value(image))
            : detector->value.detectAndDecode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points));
        std::vector<std::string> decoded_info = to_single_decoded_string(decoded_text);
        return set_multi_string_counts(api_name, !decoded_text.empty(), decoded_info, decoded, string_count, byte_count);
#else
        (void)points;
        assign_bool(decoded, false);
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

int jyppx_ocv_barcode_detector_detect_and_decode_fill(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_detect_and_decode_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::string decoded_text = points == nullptr
            ? detector->value.detectAndDecode(opencv_csharp_native::mat_value(image))
            : detector->value.detectAndDecode(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points));
        std::vector<std::string> decoded_info = to_single_decoded_string(decoded_text);
        return copy_multi_strings_to_output(api_name, !decoded_text.empty(), decoded_info, offsets, offset_capacity, buffer, buffer_capacity, decoded, string_count, byte_count);
#else
        (void)points;
        (void)offsets;
        (void)offset_capacity;
        (void)buffer;
        (void)buffer_capacity;
        assign_bool(decoded, false);
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

int jyppx_ocv_barcode_detector_detect_and_decode_with_type_count(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* decoded,
    int* info_count,
    int* info_byte_count,
    int* type_count,
    int* type_byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_detect_and_decode_with_type_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        std::vector<std::string> decoded_type;
        bool result = points == nullptr
            ? detector->value.detectAndDecodeWithType(opencv_csharp_native::mat_value(image), decoded_info, decoded_type)
            : detector->value.detectAndDecodeWithType(opencv_csharp_native::mat_value(image), decoded_info, decoded_type, opencv_csharp_native::mat_value(points));
        return set_string_pair_counts(api_name, result, decoded_info, decoded_type, decoded, info_count, info_byte_count, type_count, type_byte_count);
#else
        (void)points;
        assign_bool(decoded, false);
        if (info_count != nullptr) { *info_count = 0; }
        if (info_byte_count != nullptr) { *info_byte_count = 0; }
        if (type_count != nullptr) { *type_count = 0; }
        if (type_byte_count != nullptr) { *type_byte_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_detect_and_decode_with_type_fill(
    const jyppx_ocv_barcode_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* info_offsets,
    int info_offset_capacity,
    char* info_buffer,
    int info_buffer_capacity,
    int* type_offsets,
    int type_offset_capacity,
    char* type_buffer,
    int type_buffer_capacity,
    int* decoded,
    int* info_count,
    int* info_byte_count,
    int* type_count,
    int* type_byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_detect_and_decode_with_type_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        std::vector<std::string> decoded_type;
        bool result = points == nullptr
            ? detector->value.detectAndDecodeWithType(opencv_csharp_native::mat_value(image), decoded_info, decoded_type)
            : detector->value.detectAndDecodeWithType(opencv_csharp_native::mat_value(image), decoded_info, decoded_type, opencv_csharp_native::mat_value(points));
        return copy_string_pair_to_output(
            api_name,
            result,
            decoded_info,
            decoded_type,
            info_offsets,
            info_offset_capacity,
            info_buffer,
            info_buffer_capacity,
            type_offsets,
            type_offset_capacity,
            type_buffer,
            type_buffer_capacity,
            decoded,
            info_count,
            info_byte_count,
            type_count,
            type_byte_count);
#else
        (void)points;
        (void)info_offsets;
        (void)info_offset_capacity;
        (void)info_buffer;
        (void)info_buffer_capacity;
        (void)type_offsets;
        (void)type_offset_capacity;
        (void)type_buffer;
        (void)type_buffer_capacity;
        assign_bool(decoded, false);
        if (info_count != nullptr) { *info_count = 0; }
        if (info_byte_count != nullptr) { *info_byte_count = 0; }
        if (type_count != nullptr) { *type_count = 0; }
        if (type_byte_count != nullptr) { *type_byte_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_get_downsampling_threshold(const jyppx_ocv_barcode_detector* detector, double* threshold)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_get_downsampling_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, threshold, "threshold");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *threshold = detector->value.getDownsamplingThreshold();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *threshold = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_set_downsampling_threshold(jyppx_ocv_barcode_detector* detector, double threshold)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_set_downsampling_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setDownsamplingThreshold(threshold);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_get_gradient_threshold(const jyppx_ocv_barcode_detector* detector, double* threshold)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_get_gradient_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, threshold, "threshold");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *threshold = detector->value.getGradientThreshold();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *threshold = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_set_gradient_threshold(jyppx_ocv_barcode_detector* detector, double threshold)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_set_gradient_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setGradientThreshold(threshold);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_get_detector_scales_count(const jyppx_ocv_barcode_detector* detector, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_get_detector_scales_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<float> scales;
        detector->value.getDetectorScales(scales);
        if (scales.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "scales");
        }

        *count = static_cast<int>(scales.size());
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

int jyppx_ocv_barcode_detector_get_detector_scales_fill(const jyppx_ocv_barcode_detector* detector, float* scales, int scale_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_get_detector_scales_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<float> values;
        detector->value.getDetectorScales(values);
        if (values.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "scales");
        }

        *count = static_cast<int>(values.size());
        if (scales == nullptr || scale_capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "scales");
        }

        for (int i = 0; i < *count; ++i)
        {
            scales[i] = values[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)scales;
        (void)scale_capacity;
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_barcode_detector_set_detector_scales(jyppx_ocv_barcode_detector* detector, const float* scales, int scale_count)
{
    constexpr const char* api_name = "jyppx_ocv_barcode_detector_set_detector_scales";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_barcode_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_float_array(api_name, scales, scale_count, "scales");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setDetectorScales(to_float_vector(scales, scale_count));
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

int jyppx_ocv_qrcode_detector_aruco_default_params(jyppx_ocv_qrcode_detector_aruco_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_default_params";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *params = from_aruco_params(cv::QRCodeDetectorAruco::Params());
#else
        set_default_qrcode_detector_aruco_params(params);
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_aruco_create(jyppx_ocv_qrcode_detector_aruco** detector)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_qrcode_detector_aruco* created = new (std::nothrow) jyppx_ocv_qrcode_detector_aruco();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *detector = created;
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

int jyppx_ocv_qrcode_detector_aruco_create_with_params(
    const jyppx_ocv_qrcode_detector_aruco_params* params,
    jyppx_ocv_qrcode_detector_aruco** detector)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_create_with_params";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_qrcode_detector_aruco* created = new (std::nothrow) jyppx_ocv_qrcode_detector_aruco{ cv::QRCodeDetectorAruco(to_aruco_params(*params)) };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *detector = created;
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

void jyppx_ocv_qrcode_detector_aruco_release_handle(jyppx_ocv_qrcode_detector_aruco* detector)
{
    delete detector;
}

int jyppx_ocv_qrcode_detector_aruco_get_detector_parameters(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    jyppx_ocv_qrcode_detector_aruco_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_get_detector_parameters";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *params = from_aruco_params(detector->value.getDetectorParameters());
        return OPENCV_CSHARP_STATUS_OK;
#else
        set_default_qrcode_detector_aruco_params(params);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_aruco_set_detector_parameters(
    jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_qrcode_detector_aruco_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_set_detector_parameters";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setDetectorParameters(to_aruco_params(*params));
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

int jyppx_ocv_qrcode_detector_aruco_detect(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* detected)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_detect";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, detected, "detected");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(detected, detector->value.detect(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(detected, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_aruco_decode_length(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_decode_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_string_length(api_name, qrcode_aruco_decode(detector, image, points, straight_qrcode), length);
#else
        (void)straight_qrcode;
        if (length != nullptr) { *length = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_aruco_decode_fill(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_decode_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return copy_string_to_output(api_name, qrcode_aruco_decode(detector, image, points, straight_qrcode), buffer, buffer_capacity, written);
#else
        (void)straight_qrcode;
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

int jyppx_ocv_qrcode_detector_aruco_detect_and_decode_length(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    int* length)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_string_length(api_name, qrcode_aruco_detect_and_decode(detector, image, points, straight_qrcode), length);
#else
        (void)points;
        (void)straight_qrcode;
        if (length != nullptr) { *length = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_aruco_detect_and_decode_fill(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    jyppx_ocv_mat* straight_qrcode,
    char* buffer,
    int buffer_capacity,
    int* written)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return copy_string_to_output(api_name, qrcode_aruco_detect_and_decode(detector, image, points, straight_qrcode), buffer, buffer_capacity, written);
#else
        (void)points;
        (void)straight_qrcode;
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

int jyppx_ocv_qrcode_detector_aruco_detect_multi(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* detected)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_detect_multi";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, detected, "detected");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(detected, detector->value.detectMulti(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(detected, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_detector_aruco_decode_multi_count(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_decode_multi_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        bool result = detector->value.decodeMulti(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), decoded_info);
        return set_multi_string_counts(api_name, result, decoded_info, decoded, string_count, byte_count);
#else
        assign_bool(decoded, false);
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

int jyppx_ocv_qrcode_detector_aruco_decode_multi_fill(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_decode_multi_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        bool result = detector->value.decodeMulti(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(points), decoded_info);
        return copy_multi_strings_to_output(api_name, result, decoded_info, offsets, offset_capacity, buffer, buffer_capacity, decoded, string_count, byte_count);
#else
        (void)offsets;
        (void)offset_capacity;
        (void)buffer;
        (void)buffer_capacity;
        assign_bool(decoded, false);
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

int jyppx_ocv_qrcode_detector_aruco_detect_and_decode_multi_count(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_multi_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        bool result = detector->value.detectAndDecodeMulti(opencv_csharp_native::mat_value(image), decoded_info, opencv_csharp_native::mat_value(points));
        return set_multi_string_counts(api_name, result, decoded_info, decoded, string_count, byte_count);
#else
        assign_bool(decoded, false);
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

int jyppx_ocv_qrcode_detector_aruco_detect_and_decode_multi_fill(
    const jyppx_ocv_qrcode_detector_aruco* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* points,
    int* offsets,
    int offset_capacity,
    char* buffer,
    int buffer_capacity,
    int* decoded,
    int* string_count,
    int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_detector_aruco_detect_and_decode_multi_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_detector_aruco(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::string> decoded_info;
        bool result = detector->value.detectAndDecodeMulti(opencv_csharp_native::mat_value(image), decoded_info, opencv_csharp_native::mat_value(points));
        return copy_multi_strings_to_output(api_name, result, decoded_info, offsets, offset_capacity, buffer, buffer_capacity, decoded, string_count, byte_count);
#else
        (void)offsets;
        (void)offset_capacity;
        (void)buffer;
        (void)buffer_capacity;
        assign_bool(decoded, false);
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

int jyppx_ocv_qrcode_encoder_default_params(jyppx_ocv_qrcode_encoder_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_encoder_default_params";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::QRCodeEncoder::Params native_params;
        params->version = native_params.version;
        params->correction_level = static_cast<int>(native_params.correction_level);
        params->mode = static_cast<int>(native_params.mode);
        params->structure_number = native_params.structure_number;
#else
        set_default_qrcode_encoder_params(params);
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_qrcode_encoder_create(const jyppx_ocv_qrcode_encoder_params* params, jyppx_ocv_qrcode_encoder** encoder)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_encoder_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

        if (encoder == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "encoder");
        }

        *encoder = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_qrcode_encoder* created = new (std::nothrow) jyppx_ocv_qrcode_encoder();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::QRCodeEncoder::create(to_encoder_params(*params));
        *encoder = created;
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

void jyppx_ocv_qrcode_encoder_release_handle(jyppx_ocv_qrcode_encoder* encoder)
{
    delete encoder;
}

int jyppx_ocv_qrcode_encoder_encode(const jyppx_ocv_qrcode_encoder* encoder, const char* encoded_info, jyppx_ocv_mat* qrcode)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_encoder_encode";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_encoder(api_name, encoder);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_required_string(api_name, encoded_info, "encoded_info");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, qrcode, "qrcode");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        encoder->value->encode(encoded_info, opencv_csharp_native::mat_value(qrcode));
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

int jyppx_ocv_qrcode_encoder_encode_structured_append_count(const jyppx_ocv_qrcode_encoder* encoder, const char* encoded_info, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_encoder_encode_structured_append_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_encoder(api_name, encoder);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_required_string(api_name, encoded_info, "encoded_info");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> qrcodes;
        encoder->value->encodeStructuredAppend(encoded_info, qrcodes);
        if (qrcodes.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "qrcodes");
        }

        *count = static_cast<int>(qrcodes.size());
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

int jyppx_ocv_qrcode_encoder_encode_structured_append_fill(
    const jyppx_ocv_qrcode_encoder* encoder,
    const char* encoded_info,
    jyppx_ocv_mat** qrcodes,
    int qrcode_capacity,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_qrcode_encoder_encode_structured_append_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_qrcode_encoder(api_name, encoder);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_required_string(api_name, encoded_info, "encoded_info");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_qrcodes;
        encoder->value->encodeStructuredAppend(encoded_info, native_qrcodes);
        if (native_qrcodes.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "qrcodes");
        }

        *count = static_cast<int>(native_qrcodes.size());
        if (qrcodes == nullptr || qrcode_capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "qrcodes");
        }

        for (int i = 0; i < *count; ++i)
        {
            qrcodes[i] = nullptr;
        }

        for (int i = 0; i < *count; ++i)
        {
            qrcodes[i] = new (std::nothrow) jyppx_ocv_mat{ native_qrcodes[static_cast<size_t>(i)] };
            if (qrcodes[i] == nullptr)
            {
                for (int j = 0; j < i; ++j)
                {
                    delete qrcodes[j];
                    qrcodes[j] = nullptr;
                }

                return opencv_csharp_native::set_out_of_memory(api_name);
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)qrcodes;
        (void)qrcode_capacity;
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_detector_yn_create(
    const char* model,
    const char* config,
    int input_width,
    int input_height,
    float score_threshold,
    float nms_threshold,
    int top_k,
    int backend_id,
    int target_id,
    jyppx_ocv_face_detector_yn** detector)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_required_string(api_name, model, "model");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_positive_size(api_name, input_width, input_height);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_face_detector_yn* created = new (std::nothrow) jyppx_ocv_face_detector_yn();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        const char* config_value = config == nullptr ? "" : config;
        created->value = cv::FaceDetectorYN::create(model, config_value, cv::Size(input_width, input_height), score_threshold, nms_threshold, top_k, backend_id, target_id);
        *detector = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)config;
        (void)score_threshold;
        (void)nms_threshold;
        (void)top_k;
        (void)backend_id;
        (void)target_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_detector_yn_create_from_buffer(
    const char* framework,
    const unsigned char* model_buffer,
    int model_buffer_length,
    const unsigned char* config_buffer,
    int config_buffer_length,
    int input_width,
    int input_height,
    float score_threshold,
    float nms_threshold,
    int top_k,
    int backend_id,
    int target_id,
    jyppx_ocv_face_detector_yn** detector)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_create_from_buffer";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_required_string(api_name, framework, "framework");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_buffer(api_name, model_buffer, model_buffer_length, "model_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_buffer(api_name, config_buffer, config_buffer_length, "config_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_positive_size(api_name, input_width, input_height);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_face_detector_yn* created = new (std::nothrow) jyppx_ocv_face_detector_yn();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::FaceDetectorYN::create(
            framework,
            to_vector(model_buffer, model_buffer_length),
            to_vector(config_buffer, config_buffer_length),
            cv::Size(input_width, input_height),
            score_threshold,
            nms_threshold,
            top_k,
            backend_id,
            target_id);
        *detector = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)score_threshold;
        (void)nms_threshold;
        (void)top_k;
        (void)backend_id;
        (void)target_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_face_detector_yn_release_handle(jyppx_ocv_face_detector_yn* detector)
{
    delete detector;
}

int jyppx_ocv_face_detector_yn_set_input_size(jyppx_ocv_face_detector_yn* detector, int width, int height)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_set_input_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_positive_size(api_name, width, height);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value->setInputSize(cv::Size(width, height));
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

int jyppx_ocv_face_detector_yn_get_input_size(jyppx_ocv_face_detector_yn* detector, int* width, int* height)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_get_input_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, width, "width");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, height, "height");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Size input_size = detector->value->getInputSize();
        *width = input_size.width;
        *height = input_size.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *width = 0;
        *height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_detector_yn_set_score_threshold(jyppx_ocv_face_detector_yn* detector, float score_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_set_score_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value->setScoreThreshold(score_threshold);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)score_threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_detector_yn_get_score_threshold(jyppx_ocv_face_detector_yn* detector, float* score_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_get_score_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, score_threshold, "score_threshold");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *score_threshold = detector->value->getScoreThreshold();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *score_threshold = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_detector_yn_set_nms_threshold(jyppx_ocv_face_detector_yn* detector, float nms_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_set_nms_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value->setNMSThreshold(nms_threshold);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)nms_threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_detector_yn_get_nms_threshold(jyppx_ocv_face_detector_yn* detector, float* nms_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_get_nms_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, nms_threshold, "nms_threshold");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *nms_threshold = detector->value->getNMSThreshold();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *nms_threshold = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_detector_yn_set_top_k(jyppx_ocv_face_detector_yn* detector, int top_k)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_set_top_k";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value->setTopK(top_k);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)top_k;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_detector_yn_get_top_k(jyppx_ocv_face_detector_yn* detector, int* top_k)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_get_top_k";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, top_k, "top_k");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *top_k = detector->value->getTopK();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *top_k = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_detector_yn_detect(jyppx_ocv_face_detector_yn* detector, const jyppx_ocv_mat* image, jyppx_ocv_mat* faces, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_face_detector_yn_detect";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, faces, "faces");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *result = detector->value->detect(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(faces));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_sf_create(const char* model, const char* config, int backend_id, int target_id, jyppx_ocv_face_recognizer_sf** recognizer)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_sf_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_required_string(api_name, model, "model");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (recognizer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "recognizer");
        }

        *recognizer = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_face_recognizer_sf* created = new (std::nothrow) jyppx_ocv_face_recognizer_sf();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        const char* config_value = config == nullptr ? "" : config;
        created->value = cv::FaceRecognizerSF::create(model, config_value, backend_id, target_id);
        *recognizer = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)config;
        (void)backend_id;
        (void)target_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_sf_create_from_buffer(
    const char* framework,
    const unsigned char* model_buffer,
    int model_buffer_length,
    const unsigned char* config_buffer,
    int config_buffer_length,
    int backend_id,
    int target_id,
    jyppx_ocv_face_recognizer_sf** recognizer)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_sf_create_from_buffer";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_required_string(api_name, framework, "framework");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_buffer(api_name, model_buffer, model_buffer_length, "model_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_buffer(api_name, config_buffer, config_buffer_length, "config_buffer");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (recognizer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "recognizer");
        }

        *recognizer = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_face_recognizer_sf* created = new (std::nothrow) jyppx_ocv_face_recognizer_sf();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::FaceRecognizerSF::create(
            framework,
            to_vector(model_buffer, model_buffer_length),
            to_vector(config_buffer, config_buffer_length),
            backend_id,
            target_id);
        *recognizer = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)backend_id;
        (void)target_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_face_recognizer_sf_release_handle(jyppx_ocv_face_recognizer_sf* recognizer)
{
    delete recognizer;
}

int jyppx_ocv_face_recognizer_sf_align_crop(
    const jyppx_ocv_face_recognizer_sf* recognizer,
    const jyppx_ocv_mat* source_image,
    const jyppx_ocv_mat* face_box,
    jyppx_ocv_mat* aligned_image)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_sf_align_crop";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, source_image, "source_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, face_box, "face_box");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, aligned_image, "aligned_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        recognizer->value->alignCrop(opencv_csharp_native::mat_value(source_image), opencv_csharp_native::mat_value(face_box), opencv_csharp_native::mat_value(aligned_image));
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

int jyppx_ocv_face_recognizer_sf_feature(
    jyppx_ocv_face_recognizer_sf* recognizer,
    const jyppx_ocv_mat* aligned_image,
    jyppx_ocv_mat* face_feature)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_sf_feature";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, aligned_image, "aligned_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, face_feature, "face_feature");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        recognizer->value->feature(opencv_csharp_native::mat_value(aligned_image), opencv_csharp_native::mat_value(face_feature));
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

int jyppx_ocv_face_recognizer_sf_match(
    const jyppx_ocv_face_recognizer_sf* recognizer,
    const jyppx_ocv_mat* face_feature1,
    const jyppx_ocv_mat* face_feature2,
    int distance_type,
    double* result)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_sf_match";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_face_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, face_feature1, "face_feature1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, face_feature2, "face_feature2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *result = recognizer->value->match(opencv_csharp_native::mat_value(face_feature1), opencv_csharp_native::mat_value(face_feature2), distance_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)distance_type;
        *result = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

