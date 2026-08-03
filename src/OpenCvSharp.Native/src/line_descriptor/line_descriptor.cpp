#include "open_cv_sharp/line_descriptor/line_descriptor.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "line_descriptor_handles.h"

#include <new>
#include <vector>

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
#include <opencv2/line_descriptor.hpp>
#endif

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* parameter_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, parameter_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_mat(const char* api_name, jyppx_ocv_mat* mat, const char* parameter_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, parameter_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int(const char* api_name, const int* value, const char* parameter_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, parameter_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_non_negative_count(const char* api_name, int count, const char* parameter_name)
    {
        return count < 0
            ? opencv_csharp_native::set_invalid_argument(api_name, parameter_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_keyline_input(const char* api_name, const jyppx_ocv_line_descriptor_key_line* keylines, int count, const char* parameter_name)
    {
        int status = validate_non_negative_count(api_name, count, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return count > 0 && keylines == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, parameter_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_match_input(const char* api_name, const jyppx_ocv_dmatch* matches, int count)
    {
        int status = validate_non_negative_count(api_name, count, "match_count");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return count > 0 && matches == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "matches")
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
    int validate_descriptor(const char* api_name, const jyppx_ocv_line_descriptor_binary_descriptor* descriptor)
    {
        return descriptor == nullptr || descriptor->value.empty()
            ? opencv_csharp_native::set_invalid_argument(api_name, "descriptor")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_matcher(const char* api_name, const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher)
    {
        return matcher == nullptr || matcher->value.empty()
            ? opencv_csharp_native::set_invalid_argument(api_name, "matcher")
            : OPENCV_CSHARP_STATUS_OK;
    }

    cv::_InputArray optional_input_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::_InputArray() : cv::_InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::Mat optional_mat(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::Mat() : opencv_csharp_native::mat_value(mat);
    }

    cv::Scalar scalar_from_values(double v0, double v1, double v2, double v3)
    {
        return cv::Scalar(v0, v1, v2, v3);
    }

    cv::line_descriptor::KeyLine to_cv_keyline(const jyppx_ocv_line_descriptor_key_line& keyline)
    {
        cv::line_descriptor::KeyLine result;
        result.angle = keyline.angle;
        result.class_id = keyline.class_id;
        result.octave = keyline.octave;
        result.pt = cv::Point2f(keyline.pt_x, keyline.pt_y);
        result.response = keyline.response;
        result.size = keyline.size;
        result.startPointX = keyline.start_point_x;
        result.startPointY = keyline.start_point_y;
        result.endPointX = keyline.end_point_x;
        result.endPointY = keyline.end_point_y;
        result.sPointInOctaveX = keyline.start_point_in_octave_x;
        result.sPointInOctaveY = keyline.start_point_in_octave_y;
        result.ePointInOctaveX = keyline.end_point_in_octave_x;
        result.ePointInOctaveY = keyline.end_point_in_octave_y;
        result.lineLength = keyline.line_length;
        result.numOfPixels = keyline.num_of_pixels;
        return result;
    }

    jyppx_ocv_line_descriptor_key_line from_cv_keyline(const cv::line_descriptor::KeyLine& keyline)
    {
        return jyppx_ocv_line_descriptor_key_line{
            keyline.angle,
            keyline.class_id,
            keyline.octave,
            keyline.pt.x,
            keyline.pt.y,
            keyline.response,
            keyline.size,
            keyline.startPointX,
            keyline.startPointY,
            keyline.endPointX,
            keyline.endPointY,
            keyline.sPointInOctaveX,
            keyline.sPointInOctaveY,
            keyline.ePointInOctaveX,
            keyline.ePointInOctaveY,
            keyline.lineLength,
            keyline.numOfPixels
        };
    }

    cv::DMatch to_cv_dmatch(const jyppx_ocv_dmatch& match)
    {
        return cv::DMatch(match.query_idx, match.train_idx, match.img_idx, match.distance);
    }

    jyppx_ocv_dmatch from_cv_dmatch(const cv::DMatch& match)
    {
        return jyppx_ocv_dmatch{
            match.queryIdx,
            match.trainIdx,
            match.imgIdx,
            match.distance
        };
    }

    std::vector<cv::line_descriptor::KeyLine> to_cv_keylines(const jyppx_ocv_line_descriptor_key_line* keylines, int count)
    {
        std::vector<cv::line_descriptor::KeyLine> result;
        result.reserve(static_cast<size_t>(count));
        for (int i = 0; i < count; ++i)
        {
            result.push_back(to_cv_keyline(keylines[i]));
        }

        return result;
    }

    std::vector<cv::DMatch> to_cv_dmatches(const jyppx_ocv_dmatch* matches, int count)
    {
        std::vector<cv::DMatch> result;
        result.reserve(static_cast<size_t>(count));
        for (int i = 0; i < count; ++i)
        {
            result.push_back(to_cv_dmatch(matches[i]));
        }

        return result;
    }

    int copy_keylines_to_output(
        const char* api_name,
        const std::vector<cv::line_descriptor::KeyLine>& source,
        jyppx_ocv_line_descriptor_key_line* destination,
        int capacity,
        int* count)
    {
        if (count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "count");
        }

        *count = static_cast<int>(source.size());
        if (source.empty())
        {
            return OPENCV_CSHARP_STATUS_OK;
        }

        if (destination == nullptr || capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "keylines");
        }

        for (int i = 0; i < *count; ++i)
        {
            destination[i] = from_cv_keyline(source[static_cast<size_t>(i)]);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_matches_to_output(
        const char* api_name,
        const std::vector<cv::DMatch>& source,
        jyppx_ocv_dmatch* destination,
        int capacity,
        int* count)
    {
        if (count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "count");
        }

        *count = static_cast<int>(source.size());
        if (source.empty())
        {
            return OPENCV_CSHARP_STATUS_OK;
        }

        if (destination == nullptr || capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matches");
        }

        for (int i = 0; i < *count; ++i)
        {
            destination[i] = from_cv_dmatch(source[static_cast<size_t>(i)]);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int summarize_grouped_matches(
        const char* api_name,
        const std::vector<std::vector<cv::DMatch>>& groups,
        int* group_count,
        int* total_match_count)
    {
        if (group_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "group_count");
        }

        if (total_match_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "total_match_count");
        }

        int total = 0;
        for (size_t i = 0; i < groups.size(); ++i)
        {
            total += static_cast<int>(groups[i].size());
        }

        *group_count = static_cast<int>(groups.size());
        *total_match_count = total;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_grouped_matches_to_output(
        const char* api_name,
        const std::vector<std::vector<cv::DMatch>>& groups,
        int* offsets,
        int offset_capacity,
        jyppx_ocv_dmatch* matches,
        int match_capacity,
        int* group_count,
        int* total_match_count)
    {
        int status = summarize_grouped_matches(api_name, groups, group_count, total_match_count);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (offsets == nullptr || offset_capacity < *group_count + 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "offsets");
        }

        if (*total_match_count > 0 && (matches == nullptr || match_capacity < *total_match_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matches");
        }

        int offset = 0;
        offsets[0] = 0;
        for (int i = 0; i < *group_count; ++i)
        {
            const std::vector<cv::DMatch>& group = groups[static_cast<size_t>(i)];
            for (size_t j = 0; j < group.size(); ++j)
            {
                matches[offset++] = from_cv_dmatch(group[j]);
            }

            offsets[i + 1] = offset;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_line_descriptor_binary_descriptor_create(
    int num_of_octave,
    int width_of_band,
    int reduction_ratio,
    int ksize,
    jyppx_ocv_line_descriptor_binary_descriptor** descriptor)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (descriptor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "descriptor");
        }

        *descriptor = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        cv::line_descriptor::BinaryDescriptor::Params parameters;
        parameters.numOfOctave_ = num_of_octave;
        parameters.widthOfBand_ = width_of_band;
        parameters.reductionRatio = reduction_ratio;
        parameters.ksize_ = ksize;

        jyppx_ocv_line_descriptor_binary_descriptor* created = new (std::nothrow) jyppx_ocv_line_descriptor_binary_descriptor();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::line_descriptor::BinaryDescriptor::createBinaryDescriptor(parameters);
        *descriptor = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)num_of_octave; (void)width_of_band; (void)reduction_ratio; (void)ksize;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_line_descriptor_binary_descriptor_release(jyppx_ocv_line_descriptor_binary_descriptor* descriptor)
{
    delete descriptor;
}

int jyppx_ocv_line_descriptor_binary_descriptor_clear(jyppx_ocv_line_descriptor_binary_descriptor* descriptor)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_clear";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        int status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        descriptor->value->clear();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_empty(const jyppx_ocv_line_descriptor_binary_descriptor* descriptor, int* empty)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, empty, "empty");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *empty = descriptor->value->empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor;
        *empty = 1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_descriptor_size(const jyppx_ocv_line_descriptor_binary_descriptor* descriptor, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_descriptor_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = descriptor->value->descriptorSize();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_descriptor_type(const jyppx_ocv_line_descriptor_binary_descriptor* descriptor, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_descriptor_type";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = descriptor->value->descriptorType();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_default_norm(const jyppx_ocv_line_descriptor_binary_descriptor* descriptor, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_default_norm";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = descriptor->value->defaultNorm();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_get_num_of_octaves(jyppx_ocv_line_descriptor_binary_descriptor* descriptor, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_get_num_of_octaves";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = descriptor->value->getNumOfOctaves();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_set_num_of_octaves(jyppx_ocv_line_descriptor_binary_descriptor* descriptor, int value)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_set_num_of_octaves";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        int status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        descriptor->value->setNumOfOctaves(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_get_width_of_band(jyppx_ocv_line_descriptor_binary_descriptor* descriptor, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_get_width_of_band";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = descriptor->value->getWidthOfBand();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_set_width_of_band(jyppx_ocv_line_descriptor_binary_descriptor* descriptor, int value)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_set_width_of_band";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        int status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        descriptor->value->setWidthOfBand(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_get_reduction_ratio(jyppx_ocv_line_descriptor_binary_descriptor* descriptor, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_get_reduction_ratio";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = descriptor->value->getReductionRatio();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_set_reduction_ratio(jyppx_ocv_line_descriptor_binary_descriptor* descriptor, int value)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_set_reduction_ratio";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        int status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        descriptor->value->setReductionRatio(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_detect_count(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    int* keyline_count)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_detect_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, keyline_count, "keyline_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::line_descriptor::KeyLine> keylines;
        descriptor->value->detect(opencv_csharp_native::mat_value(image), keylines, optional_mat(mask));
        *keyline_count = static_cast<int>(keylines.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)descriptor; (void)mask;
        *keyline_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_detect_fill(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_line_descriptor_key_line* keylines,
    int keyline_capacity,
    int* keyline_count)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_detect_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_non_negative_count(api_name, keyline_capacity, "keyline_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::line_descriptor::KeyLine> native_keylines;
        descriptor->value->detect(opencv_csharp_native::mat_value(image), native_keylines, optional_mat(mask));
        return copy_keylines_to_output(api_name, native_keylines, keylines, keyline_capacity, keyline_count);
#else
        (void)descriptor; (void)mask; (void)keylines;
        if (keyline_count != nullptr) { *keyline_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_compute(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_line_descriptor_key_line* keylines_in,
    int keyline_count,
    jyppx_ocv_line_descriptor_key_line* keylines_out,
    int keyline_capacity,
    int* written_keyline_count,
    jyppx_ocv_mat* descriptors,
    int return_float_descriptor)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_keyline_input(api_name, keylines_in, keyline_count, "keylines");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_non_negative_count(api_name, keyline_capacity, "keyline_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, descriptors, "descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_descriptor(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::line_descriptor::KeyLine> native_keylines = to_cv_keylines(keylines_in, keyline_count);
        descriptor->value->compute(
            opencv_csharp_native::mat_value(image),
            native_keylines,
            opencv_csharp_native::mat_value(descriptors),
            return_float_descriptor != 0);
        return copy_keylines_to_output(api_name, native_keylines, keylines_out, keyline_capacity, written_keyline_count);
#else
        (void)descriptor; (void)return_float_descriptor; (void)keylines_out;
        if (written_keyline_count != nullptr) { *written_keyline_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int detect_and_compute_core(
    const char* api_name,
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_line_descriptor_key_line* keylines_in,
    int keyline_count,
    int use_provided_keylines,
    int return_float_descriptor,
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
    std::vector<cv::line_descriptor::KeyLine>& native_keylines,
    cv::Mat& native_descriptors
#else
    int& output_count
#endif
    )
{
    int status = validate_mat(api_name, image, "image");
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    status = validate_keyline_input(api_name, keylines_in, keyline_count, "keylines");
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
    status = validate_descriptor(api_name, descriptor);
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    native_keylines = use_provided_keylines != 0
        ? to_cv_keylines(keylines_in, keyline_count)
        : std::vector<cv::line_descriptor::KeyLine>();
    descriptor->value->operator()(
        opencv_csharp_native::mat_value(image),
        optional_input_array(mask),
        native_keylines,
        native_descriptors,
        use_provided_keylines != 0,
        return_float_descriptor != 0);
    return OPENCV_CSHARP_STATUS_OK;
#else
    (void)descriptor; (void)mask; (void)use_provided_keylines; (void)return_float_descriptor;
    output_count = 0;
    return opencv_csharp_native::set_not_linked(api_name);
#endif
}

int jyppx_ocv_line_descriptor_binary_descriptor_detect_and_compute_count(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_line_descriptor_key_line* keylines_in,
    int keyline_count,
    int use_provided_keylines,
    int return_float_descriptor,
    int* output_keyline_count)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_detect_and_compute_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, output_keyline_count, "output_keyline_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        std::vector<cv::line_descriptor::KeyLine> native_keylines;
        cv::Mat native_descriptors;
        status = detect_and_compute_core(api_name, descriptor, image, mask, keylines_in, keyline_count, use_provided_keylines, return_float_descriptor, native_keylines, native_descriptors);
        if (status == OPENCV_CSHARP_STATUS_OK)
        {
            *output_keyline_count = static_cast<int>(native_keylines.size());
        }

        return status;
#else
        int count = 0;
        status = detect_and_compute_core(api_name, descriptor, image, mask, keylines_in, keyline_count, use_provided_keylines, return_float_descriptor, count);
        *output_keyline_count = count;
        return status;
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_detect_and_compute_fill(
    const jyppx_ocv_line_descriptor_binary_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_line_descriptor_key_line* keylines_in,
    int keyline_count,
    int use_provided_keylines,
    int return_float_descriptor,
    jyppx_ocv_line_descriptor_key_line* keylines_out,
    int keyline_capacity,
    int* output_keyline_count,
    jyppx_ocv_mat* descriptors)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_detect_and_compute_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_non_negative_count(api_name, keyline_capacity, "keyline_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, descriptors, "descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        std::vector<cv::line_descriptor::KeyLine> native_keylines;
        cv::Mat native_descriptors;
        status = detect_and_compute_core(api_name, descriptor, image, mask, keylines_in, keyline_count, use_provided_keylines, return_float_descriptor, native_keylines, native_descriptors);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        opencv_csharp_native::mat_value(descriptors) = native_descriptors;
        return copy_keylines_to_output(api_name, native_keylines, keylines_out, keyline_capacity, output_keyline_count);
#else
        int count = 0;
        (void)keylines_out;
        status = detect_and_compute_core(api_name, descriptor, image, mask, keylines_in, keyline_count, use_provided_keylines, return_float_descriptor, count);
        if (output_keyline_count != nullptr) { *output_keyline_count = count; }
        return status;
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_draw_keylines(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_line_descriptor_key_line* keylines,
    int keyline_count,
    jyppx_ocv_mat* out_image,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_draw_keylines";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_keyline_input(api_name, keylines, keyline_count, "keylines");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, out_image, "out_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        std::vector<cv::line_descriptor::KeyLine> native_keylines = to_cv_keylines(keylines, keyline_count);
        cv::line_descriptor::drawKeylines(
            opencv_csharp_native::mat_value(image),
            native_keylines,
            opencv_csharp_native::mat_value(out_image),
            scalar_from_values(color_v0, color_v1, color_v2, color_v3),
            flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color_v0; (void)color_v1; (void)color_v2; (void)color_v3; (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_draw_line_matches(
    const jyppx_ocv_mat* img1,
    const jyppx_ocv_line_descriptor_key_line* keylines1,
    int keyline1_count,
    const jyppx_ocv_mat* img2,
    const jyppx_ocv_line_descriptor_key_line* keylines2,
    int keyline2_count,
    const jyppx_ocv_dmatch* matches,
    int match_count,
    jyppx_ocv_mat* out_image,
    double match_color_v0,
    double match_color_v1,
    double match_color_v2,
    double match_color_v3,
    double single_line_color_v0,
    double single_line_color_v1,
    double single_line_color_v2,
    double single_line_color_v3,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_draw_line_matches";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, img1, "img1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_keyline_input(api_name, keylines1, keyline1_count, "keylines1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, img2, "img2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_keyline_input(api_name, keylines2, keyline2_count, "keylines2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_match_input(api_name, matches, match_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, out_image, "out_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        std::vector<cv::line_descriptor::KeyLine> native_keylines1 = to_cv_keylines(keylines1, keyline1_count);
        std::vector<cv::line_descriptor::KeyLine> native_keylines2 = to_cv_keylines(keylines2, keyline2_count);
        std::vector<cv::DMatch> native_matches = to_cv_dmatches(matches, match_count);
        std::vector<char> match_mask(native_matches.size(), 1);
        cv::line_descriptor::drawLineMatches(
            opencv_csharp_native::mat_value(img1),
            native_keylines1,
            opencv_csharp_native::mat_value(img2),
            native_keylines2,
            native_matches,
            opencv_csharp_native::mat_value(out_image),
            scalar_from_values(match_color_v0, match_color_v1, match_color_v2, match_color_v3),
            scalar_from_values(single_line_color_v0, single_line_color_v1, single_line_color_v2, single_line_color_v3),
            match_mask,
            flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)match_color_v0; (void)match_color_v1; (void)match_color_v2; (void)match_color_v3;
        (void)single_line_color_v0; (void)single_line_color_v1; (void)single_line_color_v2; (void)single_line_color_v3; (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_matcher_create(
    jyppx_ocv_line_descriptor_binary_descriptor_matcher** matcher)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }

        *matcher = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        jyppx_ocv_line_descriptor_binary_descriptor_matcher* created = new (std::nothrow) jyppx_ocv_line_descriptor_binary_descriptor_matcher();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::line_descriptor::BinaryDescriptorMatcher::createBinaryDescriptorMatcher();
        *matcher = created;
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

void jyppx_ocv_line_descriptor_binary_descriptor_matcher_release(
    jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher)
{
    delete matcher;
}

int jyppx_ocv_line_descriptor_binary_descriptor_matcher_clear(jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_clear";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        int status = validate_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        matcher->value->clear();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)matcher;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_matcher_empty(const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher, int* empty)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, empty, "empty");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        status = validate_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *empty = matcher->value->empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)matcher;
        *empty = 1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
static int matcher_match_core(
    const char* api_name,
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    std::vector<cv::DMatch>& matches)
{
    int status = validate_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    status = validate_mat(api_name, train_descriptors, "train_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    matcher->value->match(
        opencv_csharp_native::mat_value(query_descriptors),
        opencv_csharp_native::mat_value(train_descriptors),
        matches,
        optional_mat(mask));
    return OPENCV_CSHARP_STATUS_OK;
}
#endif

int jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_count(
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, match_count, "match_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        std::vector<cv::DMatch> matches;
        status = matcher_match_core(api_name, matcher, query_descriptors, train_descriptors, mask, matches);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *match_count = static_cast<int>(matches.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)matcher; (void)query_descriptors; (void)train_descriptors; (void)mask;
        *match_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_fill(
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_non_negative_count(api_name, match_capacity, "match_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        std::vector<cv::DMatch> native_matches;
        status = matcher_match_core(api_name, matcher, query_descriptors, train_descriptors, mask, native_matches);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_matches_to_output(api_name, native_matches, matches, match_capacity, match_count);
#else
        (void)matcher; (void)query_descriptors; (void)train_descriptors; (void)mask; (void)matches;
        if (match_count != nullptr) { *match_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
static int matcher_knn_core(
    const char* api_name,
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    std::vector<std::vector<cv::DMatch>>& matches)
{
    int status = validate_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    status = validate_mat(api_name, train_descriptors, "train_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    matcher->value->knnMatch(
        opencv_csharp_native::mat_value(query_descriptors),
        opencv_csharp_native::mat_value(train_descriptors),
        matches,
        k,
        optional_mat(mask),
        compact_result != 0);
    return OPENCV_CSHARP_STATUS_OK;
}
#endif

int jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_count(
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_count";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        std::vector<std::vector<cv::DMatch>> matches;
        int status = matcher_knn_core(api_name, matcher, query_descriptors, train_descriptors, k, mask, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
#else
        (void)matcher; (void)query_descriptors; (void)train_descriptors; (void)k; (void)mask; (void)compact_result;
        if (group_count != nullptr) { *group_count = 0; }
        if (total_match_count != nullptr) { *total_match_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_fill(
    const jyppx_ocv_line_descriptor_binary_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_non_negative_count(api_name, offset_capacity, "offset_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_non_negative_count(api_name, match_capacity, "match_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR)
        std::vector<std::vector<cv::DMatch>> native_matches;
        status = matcher_knn_core(api_name, matcher, query_descriptors, train_descriptors, k, mask, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
#else
        (void)matcher; (void)query_descriptors; (void)train_descriptors; (void)k; (void)mask; (void)compact_result; (void)offsets; (void)matches;
        if (group_count != nullptr) { *group_count = 0; }
        if (total_match_count != nullptr) { *total_match_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

