#include "open_cv_sharp/features2d/features2d.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "feature_handles.h"

#include <algorithm>
#include <atomic>
#include <chrono>
#include <cstdint>
#include <cstring>
#include <filesystem>
#include <new>
#include <string>
#include <vector>

#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
#include <opencv2/features.hpp>
#endif

#if defined(OPENCV_CSHARP_HAS_OPENCV_XFEATURES2D)
#include <opencv2/xfeatures2d.hpp>
#endif

namespace
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    bool valid_utf8(const unsigned char* data, int length)
    {
        int index = 0;
        while (index < length)
        {
            const unsigned char first = data[index++];
            if (first <= 0x7f)
            {
                continue;
            }

            int continuation_count;
            std::uint32_t code_point;
            std::uint32_t minimum;
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

            if (continuation_count > length - index)
            {
                return false;
            }
            for (int i = 0; i < continuation_count; ++i)
            {
                const unsigned char next = data[index++];
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

    int read_path(
        const char* api_name,
        const unsigned char* data,
        int length,
        std::string& value)
    {
        if (data == nullptr || length <= 0 ||
            std::find(data, data + length, static_cast<unsigned char>(0)) != data + length ||
            !valid_utf8(data, length))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename_utf8");
        }

        value.assign(reinterpret_cast<const char*>(data), static_cast<size_t>(length));
        return OPENCV_CSHARP_STATUS_OK;
    }

    std::filesystem::path path_from_utf8(const std::string& value)
    {
        return std::filesystem::u8path(value);
    }

#if defined(_WIN32)
    std::filesystem::path make_ann_temporary_path()
    {
        static std::atomic<unsigned long long> sequence{0};
        const auto timestamp = std::chrono::steady_clock::now().time_since_epoch().count();
        return std::filesystem::temp_directory_path() /
            ("jyppx-opencv-ann-" + std::to_string(timestamp) + "-" +
             std::to_string(sequence.fetch_add(1)) + ".ann");
    }

    int copy_ann_file(
        const char* api_name,
        const std::filesystem::path& source,
        const std::filesystem::path& destination)
    {
        std::error_code error;
        std::filesystem::copy_file(
            source,
            destination,
            std::filesystem::copy_options::overwrite_existing,
            error);
        if (error)
        {
            return opencv_csharp_native::set_last_error(
                OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION,
                std::string(api_name) + " failed: " + error.message());
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    void remove_ann_temporary_path(std::filesystem::path& path)
    {
        if (!path.empty())
        {
            std::error_code ignored;
            std::filesystem::remove(path, ignored);
            path.clear();
        }
    }
#endif

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* parameter_name)
    {
        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_ann_index(
        const char* api_name,
        const jyppx_ocv_features2d_ann_index* index)
    {
        if (index == nullptr || index->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int ann_feature_type(const jyppx_ocv_features2d_ann_index* index)
    {
        return index->distance == cv::ANNIndex::DIST_HAMMING ? CV_8UC1 : CV_32FC1;
    }

    int validate_ann_features(
        const char* api_name,
        const jyppx_ocv_features2d_ann_index* index,
        const jyppx_ocv_mat* features,
        const char* parameter_name,
        bool require_continuous)
    {
        int status = validate_mat(api_name, features, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        const cv::Mat& value = opencv_csharp_native::mat_value(features);
        if (value.empty() || value.dims != 2 || value.rows <= 0 ||
            value.cols != index->dimension || value.type() != ann_feature_type(index) ||
            (require_continuous && !value.isContinuous()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_non_negative_count(const char* api_name, int count, const char* parameter_name)
    {
        if (count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::_InputArray optional_input_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::_InputArray() : cv::_InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::KeyPoint to_cv_keypoint(const jyppx_ocv_key_point& keypoint)
    {
        return cv::KeyPoint(
            keypoint.x,
            keypoint.y,
            keypoint.size,
            keypoint.angle,
            keypoint.response,
            keypoint.octave,
            keypoint.class_id);
    }

    jyppx_ocv_key_point from_cv_keypoint(const cv::KeyPoint& keypoint)
    {
        return jyppx_ocv_key_point{
            keypoint.pt.x,
            keypoint.pt.y,
            keypoint.size,
            keypoint.angle,
            keypoint.response,
            keypoint.octave,
            keypoint.class_id
        };
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

    jyppx_ocv_point from_cv_point(const cv::Point& point)
    {
        return jyppx_ocv_point{ point.x, point.y };
    }

    jyppx_ocv_rect from_cv_rect(const cv::Rect& rect)
    {
        return jyppx_ocv_rect{ rect.x, rect.y, rect.width, rect.height };
    }

    std::vector<cv::KeyPoint> to_cv_keypoints(const jyppx_ocv_key_point* keypoints, int count)
    {
        std::vector<cv::KeyPoint> result;
        result.reserve(static_cast<size_t>(count));
        for (int i = 0; i < count; ++i)
        {
            result.push_back(to_cv_keypoint(keypoints[i]));
        }

        return result;
    }

    int copy_keypoints_to_output(
        const char* api_name,
        const std::vector<cv::KeyPoint>& source,
        jyppx_ocv_key_point* destination,
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
            return opencv_csharp_native::set_invalid_argument(api_name, "keypoints");
        }

        for (int i = 0; i < *count; ++i)
        {
            destination[i] = from_cv_keypoint(source[static_cast<size_t>(i)]);
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

    int summarize_regions(
        const char* api_name,
        const std::vector<std::vector<cv::Point>>& regions,
        int* region_count,
        int* total_point_count)
    {
        if (region_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "region_count");
        }

        if (total_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "total_point_count");
        }

        int total = 0;
        for (size_t i = 0; i < regions.size(); ++i)
        {
            total += static_cast<int>(regions[i].size());
        }

        *region_count = static_cast<int>(regions.size());
        *total_point_count = total;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_regions_to_output(
        const char* api_name,
        const std::vector<std::vector<cv::Point>>& regions,
        const std::vector<cv::Rect>& bboxes,
        int* offsets,
        int offset_capacity,
        jyppx_ocv_point* points,
        int point_capacity,
        jyppx_ocv_rect* destination_bboxes,
        int bbox_capacity,
        int* region_count,
        int* total_point_count)
    {
        int status = summarize_regions(api_name, regions, region_count, total_point_count);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (offsets == nullptr || offset_capacity < *region_count + 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "offsets");
        }

        if (*total_point_count > 0 && (points == nullptr || point_capacity < *total_point_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points");
        }

        if (*region_count > 0 && (destination_bboxes == nullptr || bbox_capacity < *region_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bboxes");
        }

        int offset = 0;
        offsets[0] = 0;
        for (int i = 0; i < *region_count; ++i)
        {
            const std::vector<cv::Point>& region = regions[static_cast<size_t>(i)];
            for (size_t j = 0; j < region.size(); ++j)
            {
                points[offset++] = from_cv_point(region[j]);
            }

            destination_bboxes[i] = from_cv_rect(bboxes[static_cast<size_t>(i)]);
            offsets[i + 1] = offset;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_contours_to_output(
        const char* api_name,
        const std::vector<std::vector<cv::Point>>& contours,
        int* offsets,
        int offset_capacity,
        jyppx_ocv_point* points,
        int point_capacity,
        int* contour_count,
        int* total_point_count)
    {
        int status = summarize_regions(api_name, contours, contour_count, total_point_count);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (offsets == nullptr || offset_capacity < *contour_count + 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "offsets");
        }

        if (*total_point_count > 0 && (points == nullptr || point_capacity < *total_point_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points");
        }

        int offset = 0;
        offsets[0] = 0;
        for (int i = 0; i < *contour_count; ++i)
        {
            const std::vector<cv::Point>& contour = contours[static_cast<size_t>(i)];
            for (size_t j = 0; j < contour.size(); ++j)
            {
                points[offset++] = from_cv_point(contour[j]);
            }

            offsets[i + 1] = offset;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    template<typename TObject>
    int validate_feature(const char* api_name, const TObject* object, const char* parameter_name)
    {
        if (object == nullptr || object->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    template<typename TObject>
    int feature_clear(const char* api_name, TObject* object, const char* parameter_name)
    {
        int status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        object->value->clear();
        return OPENCV_CSHARP_STATUS_OK;
    }

    template<typename TObject>
    int feature_empty(const char* api_name, const TObject* object, const char* parameter_name, int* empty)
    {
        if (empty == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "empty");
        }

        int status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *empty = object->value->empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    }

    template<typename TObject>
    int feature_descriptor_size(const char* api_name, const TObject* object, const char* parameter_name, int* value)
    {
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

        int status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = object->value->descriptorSize();
        return OPENCV_CSHARP_STATUS_OK;
    }

    template<typename TObject>
    int feature_descriptor_type(const char* api_name, const TObject* object, const char* parameter_name, int* value)
    {
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

        int status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = object->value->descriptorType();
        return OPENCV_CSHARP_STATUS_OK;
    }

    template<typename TObject>
    int feature_default_norm(const char* api_name, const TObject* object, const char* parameter_name, int* value)
    {
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

        int status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = object->value->defaultNorm();
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_string_to_output(const char* api_name, const cv::String& source, char* buffer, int buffer_capacity, int* written)
    {
        if (written == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "written");
        }

        *written = static_cast<int>(source.size());
        if (source.empty())
        {
            return OPENCV_CSHARP_STATUS_OK;
        }

        if (buffer == nullptr || buffer_capacity < *written)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        std::memcpy(buffer, source.c_str(), static_cast<size_t>(*written));
        return OPENCV_CSHARP_STATUS_OK;
    }

    template<typename TObject>
    int feature_default_name_length(const char* api_name, const TObject* object, const char* parameter_name, int* length)
    {
        if (length == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "length");
        }

        int status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::String name = object->value->getDefaultName();
        *length = static_cast<int>(name.size());
        return OPENCV_CSHARP_STATUS_OK;
    }

    template<typename TObject>
    int feature_default_name_fill(const char* api_name, const TObject* object, const char* parameter_name, char* buffer, int buffer_capacity, int* written)
    {
        int status = validate_non_negative_count(api_name, buffer_capacity, "buffer_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::String name = object->value->getDefaultName();
        return copy_string_to_output(api_name, name, buffer, buffer_capacity, written);
    }

    template<typename TObject>
    int feature_detect_count(const char* api_name, const TObject* object, const char* parameter_name, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count)
    {
        if (keypoint_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "keypoint_count");
        }

        int status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::KeyPoint> keypoints;
        const_cast<TObject*>(object)->value->detect(opencv_csharp_native::mat_value(image), keypoints, optional_input_array(mask));
        *keypoint_count = static_cast<int>(keypoints.size());
        return OPENCV_CSHARP_STATUS_OK;
    }

    template<typename TObject>
    int feature_detect_fill(const char* api_name, const TObject* object, const char* parameter_name, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count)
    {
        int status = validate_non_negative_count(api_name, keypoint_capacity, "keypoint_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::KeyPoint> detected;
        const_cast<TObject*>(object)->value->detect(opencv_csharp_native::mat_value(image), detected, optional_input_array(mask));
        return copy_keypoints_to_output(api_name, detected, keypoints, keypoint_capacity, keypoint_count);
    }

    template<typename TObject>
    int feature_compute(const char* api_name, const TObject* object, const char* parameter_name, const jyppx_ocv_mat* image, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* written_keypoint_count, jyppx_ocv_mat* descriptors)
    {
        int status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, descriptors, "descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_non_negative_count(api_name, keypoint_count, "keypoint_count");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (keypoint_count > 0 && keypoints_in == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "keypoints_in");
        }

        std::vector<cv::KeyPoint> keypoints = to_cv_keypoints(keypoints_in, keypoint_count);
        const_cast<TObject*>(object)->value->compute(opencv_csharp_native::mat_value(image), keypoints, opencv_csharp_native::mat_value(descriptors));
        return copy_keypoints_to_output(api_name, keypoints, keypoints_out, keypoint_capacity, written_keypoint_count);
    }

    template<typename TObject>
    int feature_detect_and_compute_count(const char* api_name, const TObject* object, const char* parameter_name, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, int* output_keypoint_count)
    {
        if (output_keypoint_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output_keypoint_count");
        }

        int status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_non_negative_count(api_name, keypoint_count, "keypoint_count");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (keypoint_count > 0 && keypoints_in == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "keypoints_in");
        }

        std::vector<cv::KeyPoint> keypoints;
        if (use_provided_keypoints != 0)
        {
            keypoints = to_cv_keypoints(keypoints_in, keypoint_count);
        }
        else
        {
            const_cast<TObject*>(object)->value->detect(opencv_csharp_native::mat_value(image), keypoints, optional_input_array(mask));
        }

        *output_keypoint_count = static_cast<int>(keypoints.size());
        return OPENCV_CSHARP_STATUS_OK;
    }

    template<typename TObject>
    int feature_detect_and_compute_fill(const char* api_name, const TObject* object, const char* parameter_name, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* output_keypoint_count, jyppx_ocv_mat* descriptors)
    {
        int status = validate_feature(api_name, object, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, descriptors, "descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (keypoint_count > 0 && keypoints_in == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "keypoints_in");
        }

        std::vector<cv::KeyPoint> keypoints;
        if (use_provided_keypoints != 0)
        {
            keypoints = to_cv_keypoints(keypoints_in, keypoint_count);
        }

        const_cast<TObject*>(object)->value->detectAndCompute(
            opencv_csharp_native::mat_value(image),
            optional_input_array(mask),
            keypoints,
            opencv_csharp_native::mat_value(descriptors),
            use_provided_keypoints != 0);
        return copy_keypoints_to_output(api_name, keypoints, keypoints_out, keypoint_capacity, output_keypoint_count);
    }

    std::vector<cv::Mat> to_mat_vector(const char* api_name, const jyppx_ocv_mat* const* descriptors, int descriptor_count, int& status)
    {
        status = validate_non_negative_count(api_name, descriptor_count, "descriptor_count");
        std::vector<cv::Mat> mats;
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return mats;
        }

        if (descriptor_count > 0 && descriptors == nullptr)
        {
            status = opencv_csharp_native::set_invalid_argument(api_name, "descriptors");
            return mats;
        }

        mats.reserve(static_cast<size_t>(descriptor_count));
        for (int i = 0; i < descriptor_count; ++i)
        {
            status = validate_mat(api_name, descriptors[i], "descriptors");
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                return mats;
            }

            mats.push_back(opencv_csharp_native::mat_value(descriptors[i]));
        }

        status = OPENCV_CSHARP_STATUS_OK;
        return mats;
    }

    int validate_flann(const char* api_name, const jyppx_ocv_features2d_flann_matcher* matcher)
    {
        if (matcher == nullptr || matcher->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int wrap_descriptor_matcher(
        const char* api_name,
        const cv::Ptr<cv::DescriptorMatcher>& value,
        jyppx_ocv_features2d_descriptor_matcher** matcher)
    {
        if (matcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_descriptor_matcher{ value };
        if (result == nullptr)
        {
            *matcher = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *matcher = result;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int clone_mat_to_handle(const char* api_name, const cv::Mat& mat, jyppx_ocv_mat** descriptor)
    {
        if (descriptor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "descriptor");
        }

        auto result = new (std::nothrow) jyppx_ocv_mat{ mat.clone() };
        if (result == nullptr)
        {
            *descriptor = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *descriptor = result;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_simple_blob_params(const char* api_name, const jyppx_ocv_simple_blob_params* parameters)
    {
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }

        if (parameters->size < static_cast<int>(sizeof(jyppx_ocv_simple_blob_params)))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters.size");
        }

        if (parameters->min_repeatability < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters.min_repeatability");
        }

        if (parameters->blob_color < 0 || parameters->blob_color > 255)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters.blob_color");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::SimpleBlobDetector::Params to_cv_simple_blob_params(const jyppx_ocv_simple_blob_params& source)
    {
        cv::SimpleBlobDetector::Params result;
        result.thresholdStep = source.threshold_step;
        result.minThreshold = source.min_threshold;
        result.maxThreshold = source.max_threshold;
        result.minRepeatability = static_cast<size_t>(source.min_repeatability);
        result.minDistBetweenBlobs = source.min_dist_between_blobs;
        result.filterByColor = source.filter_by_color != 0;
        result.blobColor = static_cast<uchar>(source.blob_color);
        result.filterByArea = source.filter_by_area != 0;
        result.minArea = source.min_area;
        result.maxArea = source.max_area;
        result.filterByCircularity = source.filter_by_circularity != 0;
        result.minCircularity = source.min_circularity;
        result.maxCircularity = source.max_circularity;
        result.filterByInertia = source.filter_by_inertia != 0;
        result.minInertiaRatio = source.min_inertia_ratio;
        result.maxInertiaRatio = source.max_inertia_ratio;
        result.filterByConvexity = source.filter_by_convexity != 0;
        result.minConvexity = source.min_convexity;
        result.maxConvexity = source.max_convexity;
        result.collectContours = source.collect_contours != 0;
        return result;
    }

    jyppx_ocv_simple_blob_params from_cv_simple_blob_params(const cv::SimpleBlobDetector::Params& source)
    {
        return jyppx_ocv_simple_blob_params{
            static_cast<int32_t>(sizeof(jyppx_ocv_simple_blob_params)),
            source.thresholdStep,
            source.minThreshold,
            source.maxThreshold,
            static_cast<int32_t>(source.minRepeatability),
            source.minDistBetweenBlobs,
            source.filterByColor ? 1 : 0,
            static_cast<int32_t>(source.blobColor),
            source.filterByArea ? 1 : 0,
            source.minArea,
            source.maxArea,
            source.filterByCircularity ? 1 : 0,
            source.minCircularity,
            source.maxCircularity,
            source.filterByInertia ? 1 : 0,
            source.minInertiaRatio,
            source.maxInertiaRatio,
            source.filterByConvexity ? 1 : 0,
            source.minConvexity,
            source.maxConvexity,
            source.collectContours ? 1 : 0
        };
    }

    template<typename TBackend>
    int create_affine_from_backend(
        const char* api_name,
        const TBackend* backend,
        const char* backend_parameter_name,
        int max_tilt,
        int min_tilt,
        float tilt_step,
        float rotate_step_base,
        jyppx_ocv_features2d_affine** affine)
    {
        if (affine == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "affine");
        }

        int status = validate_feature(api_name, backend, backend_parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            *affine = nullptr;
            return status;
        }

        cv::Ptr<cv::Feature2D> native_backend = backend->value;
        auto result = new (std::nothrow) jyppx_ocv_features2d_affine{
            cv::AffineFeature::create(native_backend, max_tilt, min_tilt, tilt_step, rotate_step_base)
        };
        if (result == nullptr)
        {
            *affine = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *affine = result;
        return OPENCV_CSHARP_STATUS_OK;
    }

    std::vector<float> to_float_vector(const char* api_name, const float* values, int count, const char* parameter_name, int& status)
    {
        status = validate_non_negative_count(api_name, count, parameter_name);
        std::vector<float> result;
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return result;
        }

        if (count > 0 && values == nullptr)
        {
            status = opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
            return result;
        }

        result.reserve(static_cast<size_t>(count));
        for (int i = 0; i < count; ++i)
        {
            result.push_back(values[i]);
        }

        status = OPENCV_CSHARP_STATUS_OK;
        return result;
    }

    std::vector<int> to_int_vector(const char* api_name, const int* values, int count, const char* parameter_name, int& status)
    {
        status = validate_non_negative_count(api_name, count, parameter_name);
        std::vector<int> result;
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return result;
        }

        if (count > 0 && values == nullptr)
        {
            status = opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
            return result;
        }

        result.reserve(static_cast<size_t>(count));
        for (int i = 0; i < count; ++i)
        {
            result.push_back(values[i]);
        }

        status = OPENCV_CSHARP_STATUS_OK;
        return result;
    }

    int xfeatures_not_linked(const char* api_name)
    {
        return opencv_csharp_native::set_not_linked(api_name);
    }

    void set_zero(int* value)
    {
        if (value != nullptr)
        {
            *value = 0;
        }
    }

    void set_zero(double* value)
    {
        if (value != nullptr)
        {
            *value = 0.0;
        }
    }

    void set_zero(float* value)
    {
        if (value != nullptr)
        {
            *value = 0.0F;
        }
    }

    int copy_floats_to_output(const char* api_name, const std::vector<float>& source, float* destination, int capacity, int* count, const char* parameter_name)
    {
        if (count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        *count = static_cast<int>(source.size());
        if (source.empty())
        {
            return OPENCV_CSHARP_STATUS_OK;
        }

        if (destination == nullptr || capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        for (int i = 0; i < *count; ++i)
        {
            destination[i] = source[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

#define OCV_CSHARP_TRY_BEGIN(api_name_literal) constexpr const char* api_name = api_name_literal; try { opencv_csharp_native::clear_last_error();
#define OCV_CSHARP_CATCH return OPENCV_CSHARP_STATUS_OK; } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
#endif
}

#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)

#define OCV_CSHARP_FEATURE_META(prefix, handle_type, parameter_name) \
int prefix##_clear(handle_type* handle) { OCV_CSHARP_TRY_BEGIN(#prefix "_clear") return feature_clear(api_name, handle, parameter_name); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } } \
int prefix##_empty(const handle_type* handle, int* empty) { OCV_CSHARP_TRY_BEGIN(#prefix "_empty") return feature_empty(api_name, handle, parameter_name, empty); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } } \
int prefix##_descriptor_size(const handle_type* handle, int* value) { OCV_CSHARP_TRY_BEGIN(#prefix "_descriptor_size") return feature_descriptor_size(api_name, handle, parameter_name, value); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } } \
int prefix##_descriptor_type(const handle_type* handle, int* value) { OCV_CSHARP_TRY_BEGIN(#prefix "_descriptor_type") return feature_descriptor_type(api_name, handle, parameter_name, value); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } } \
int prefix##_default_norm(const handle_type* handle, int* value) { OCV_CSHARP_TRY_BEGIN(#prefix "_default_norm") return feature_default_norm(api_name, handle, parameter_name, value); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } } \
int prefix##_default_name_length(const handle_type* handle, int* length) { OCV_CSHARP_TRY_BEGIN(#prefix "_default_name_length") return feature_default_name_length(api_name, handle, parameter_name, length); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } } \
int prefix##_default_name_fill(const handle_type* handle, char* buffer, int buffer_capacity, int* written) { OCV_CSHARP_TRY_BEGIN(#prefix "_default_name_fill") return feature_default_name_fill(api_name, handle, parameter_name, buffer, buffer_capacity, written); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } } \
int prefix##_detect_count(const handle_type* handle, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, int* keypoint_count) { OCV_CSHARP_TRY_BEGIN(#prefix "_detect_count") return feature_detect_count(api_name, handle, parameter_name, image, mask, keypoint_count); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } } \
int prefix##_detect_fill(const handle_type* handle, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, jyppx_ocv_key_point* keypoints, int keypoint_capacity, int* keypoint_count) { OCV_CSHARP_TRY_BEGIN(#prefix "_detect_fill") return feature_detect_fill(api_name, handle, parameter_name, image, mask, keypoints, keypoint_capacity, keypoint_count); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } }

#define OCV_CSHARP_FEATURE_DESCRIPTORS(prefix, handle_type, parameter_name) \
int prefix##_compute(const handle_type* handle, const jyppx_ocv_mat* image, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* written_keypoint_count, jyppx_ocv_mat* descriptors) { OCV_CSHARP_TRY_BEGIN(#prefix "_compute") return feature_compute(api_name, handle, parameter_name, image, keypoints_in, keypoint_count, keypoints_out, keypoint_capacity, written_keypoint_count, descriptors); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } } \
int prefix##_detect_and_compute_count(const handle_type* handle, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, int* output_keypoint_count) { OCV_CSHARP_TRY_BEGIN(#prefix "_detect_and_compute_count") return feature_detect_and_compute_count(api_name, handle, parameter_name, image, mask, keypoints_in, keypoint_count, use_provided_keypoints, output_keypoint_count); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } } \
int prefix##_detect_and_compute_fill(const handle_type* handle, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask, const jyppx_ocv_key_point* keypoints_in, int keypoint_count, int use_provided_keypoints, jyppx_ocv_key_point* keypoints_out, int keypoint_capacity, int* output_keypoint_count, jyppx_ocv_mat* descriptors) { OCV_CSHARP_TRY_BEGIN(#prefix "_detect_and_compute_fill") return feature_detect_and_compute_fill(api_name, handle, parameter_name, image, mask, keypoints_in, keypoint_count, use_provided_keypoints, keypoints_out, keypoint_capacity, output_keypoint_count, descriptors); } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } }

int jyppx_ocv_features2d_sift_create(
    int nfeatures,
    int n_octave_layers,
    double contrast_threshold,
    double edge_threshold,
    double sigma,
    int descriptor_type,
    int enable_precise_upscale,
    jyppx_ocv_features2d_sift** sift)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_sift_create")
        if (sift == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "sift");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_sift{
            cv::SIFT::create(nfeatures, n_octave_layers, contrast_threshold, edge_threshold, sigma, descriptor_type, enable_precise_upscale != 0),
            descriptor_type,
            enable_precise_upscale != 0
        };
        if (result == nullptr)
        {
            *sift = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *sift = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (sift != nullptr)
        {
            *sift = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_sift_release(jyppx_ocv_features2d_sift* sift)
{
    delete sift;
}

OCV_CSHARP_FEATURE_META(jyppx_ocv_features2d_sift, jyppx_ocv_features2d_sift, "sift")
OCV_CSHARP_FEATURE_DESCRIPTORS(jyppx_ocv_features2d_sift, jyppx_ocv_features2d_sift, "sift")

#define OCV_CSHARP_SIFT_GET_INT(function_name, getter_name) \
int function_name(const jyppx_ocv_features2d_sift* sift, int* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, sift, "sift"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = sift->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_SIFT_SET_INT(function_name, setter_name) \
int function_name(jyppx_ocv_features2d_sift* sift, int value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        int status = validate_feature(api_name, sift, "sift"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        sift->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_SIFT_GET_DOUBLE(function_name, getter_name) \
int function_name(const jyppx_ocv_features2d_sift* sift, double* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, sift, "sift"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = sift->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_SIFT_SET_DOUBLE(function_name, setter_name) \
int function_name(jyppx_ocv_features2d_sift* sift, double value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        int status = validate_feature(api_name, sift, "sift"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        sift->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

OCV_CSHARP_SIFT_GET_INT(jyppx_ocv_features2d_sift_get_nfeatures, getNFeatures)
OCV_CSHARP_SIFT_SET_INT(jyppx_ocv_features2d_sift_set_nfeatures, setNFeatures)
OCV_CSHARP_SIFT_GET_INT(jyppx_ocv_features2d_sift_get_n_octave_layers, getNOctaveLayers)
OCV_CSHARP_SIFT_SET_INT(jyppx_ocv_features2d_sift_set_n_octave_layers, setNOctaveLayers)
OCV_CSHARP_SIFT_GET_DOUBLE(jyppx_ocv_features2d_sift_get_contrast_threshold, getContrastThreshold)
OCV_CSHARP_SIFT_SET_DOUBLE(jyppx_ocv_features2d_sift_set_contrast_threshold, setContrastThreshold)
OCV_CSHARP_SIFT_GET_DOUBLE(jyppx_ocv_features2d_sift_get_edge_threshold, getEdgeThreshold)
OCV_CSHARP_SIFT_SET_DOUBLE(jyppx_ocv_features2d_sift_set_edge_threshold, setEdgeThreshold)
OCV_CSHARP_SIFT_GET_DOUBLE(jyppx_ocv_features2d_sift_get_sigma, getSigma)
OCV_CSHARP_SIFT_SET_DOUBLE(jyppx_ocv_features2d_sift_set_sigma, setSigma)

int jyppx_ocv_features2d_fast_create(int threshold, int nonmax_suppression, int type, jyppx_ocv_features2d_fast** fast)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_fast_create")
        if (fast == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "fast");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_fast{
            cv::FastFeatureDetector::create(threshold, nonmax_suppression != 0, static_cast<cv::FastFeatureDetector::DetectorType>(type))
        };
        if (result == nullptr)
        {
            *fast = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *fast = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (fast != nullptr)
        {
            *fast = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_fast_release(jyppx_ocv_features2d_fast* fast) { delete fast; }

OCV_CSHARP_FEATURE_META(jyppx_ocv_features2d_fast, jyppx_ocv_features2d_fast, "fast")

#define OCV_CSHARP_FAST_GET_INT(function_name, getter_name) \
int function_name(const jyppx_ocv_features2d_fast* fast, int* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, fast, "fast"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = fast->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

OCV_CSHARP_FAST_GET_INT(jyppx_ocv_features2d_fast_get_threshold, getThreshold)
OCV_CSHARP_FAST_GET_INT(jyppx_ocv_features2d_fast_get_nonmax_suppression, getNonmaxSuppression)
OCV_CSHARP_FAST_GET_INT(jyppx_ocv_features2d_fast_get_type, getType)

int jyppx_ocv_features2d_fast_set_threshold(jyppx_ocv_features2d_fast* fast, int value)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_fast_set_threshold")
        int status = validate_feature(api_name, fast, "fast");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        fast->value->setThreshold(value);
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_fast_set_nonmax_suppression(jyppx_ocv_features2d_fast* fast, int value)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_fast_set_nonmax_suppression")
        int status = validate_feature(api_name, fast, "fast");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        fast->value->setNonmaxSuppression(value != 0);
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_fast_set_type(jyppx_ocv_features2d_fast* fast, int value)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_fast_set_type")
        int status = validate_feature(api_name, fast, "fast");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        fast->value->setType(static_cast<cv::FastFeatureDetector::DetectorType>(value));
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_gftt_create(int max_corners, double quality_level, double min_distance, int block_size, int gradient_size, int use_harris_detector, double k, jyppx_ocv_features2d_gftt** gftt)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_gftt_create")
        if (gftt == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "gftt");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_gftt{
            cv::GFTTDetector::create(max_corners, quality_level, min_distance, block_size, gradient_size, use_harris_detector != 0, k)
        };
        if (result == nullptr)
        {
            *gftt = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *gftt = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (gftt != nullptr)
        {
            *gftt = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_gftt_release(jyppx_ocv_features2d_gftt* gftt) { delete gftt; }

OCV_CSHARP_FEATURE_META(jyppx_ocv_features2d_gftt, jyppx_ocv_features2d_gftt, "gftt")

#define OCV_CSHARP_GFTT_GET_INT(function_name, getter_name) \
int function_name(const jyppx_ocv_features2d_gftt* gftt, int* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, gftt, "gftt"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = gftt->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_GFTT_SET_INT(function_name, setter_name) \
int function_name(jyppx_ocv_features2d_gftt* gftt, int value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        int status = validate_feature(api_name, gftt, "gftt"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        gftt->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_GFTT_GET_DOUBLE(function_name, getter_name) \
int function_name(const jyppx_ocv_features2d_gftt* gftt, double* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, gftt, "gftt"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = gftt->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_GFTT_SET_DOUBLE(function_name, setter_name) \
int function_name(jyppx_ocv_features2d_gftt* gftt, double value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        int status = validate_feature(api_name, gftt, "gftt"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        gftt->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

OCV_CSHARP_GFTT_GET_INT(jyppx_ocv_features2d_gftt_get_max_features, getMaxFeatures)
OCV_CSHARP_GFTT_SET_INT(jyppx_ocv_features2d_gftt_set_max_features, setMaxFeatures)
OCV_CSHARP_GFTT_GET_DOUBLE(jyppx_ocv_features2d_gftt_get_quality_level, getQualityLevel)
OCV_CSHARP_GFTT_SET_DOUBLE(jyppx_ocv_features2d_gftt_set_quality_level, setQualityLevel)
OCV_CSHARP_GFTT_GET_DOUBLE(jyppx_ocv_features2d_gftt_get_min_distance, getMinDistance)
OCV_CSHARP_GFTT_SET_DOUBLE(jyppx_ocv_features2d_gftt_set_min_distance, setMinDistance)
OCV_CSHARP_GFTT_GET_INT(jyppx_ocv_features2d_gftt_get_block_size, getBlockSize)
OCV_CSHARP_GFTT_SET_INT(jyppx_ocv_features2d_gftt_set_block_size, setBlockSize)
OCV_CSHARP_GFTT_GET_INT(jyppx_ocv_features2d_gftt_get_gradient_size, getGradientSize)
OCV_CSHARP_GFTT_SET_INT(jyppx_ocv_features2d_gftt_set_gradient_size, setGradientSize)
OCV_CSHARP_GFTT_GET_INT(jyppx_ocv_features2d_gftt_get_harris_detector, getHarrisDetector)

int jyppx_ocv_features2d_gftt_set_harris_detector(jyppx_ocv_features2d_gftt* gftt, int value)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_gftt_set_harris_detector")
        int status = validate_feature(api_name, gftt, "gftt");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        gftt->value->setHarrisDetector(value != 0);
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

OCV_CSHARP_GFTT_GET_DOUBLE(jyppx_ocv_features2d_gftt_get_k, getK)
OCV_CSHARP_GFTT_SET_DOUBLE(jyppx_ocv_features2d_gftt_set_k, setK)

int jyppx_ocv_features2d_mser_create(
    int delta,
    int min_area,
    int max_area,
    double max_variation,
    double min_diversity,
    int max_evolution,
    double area_threshold,
    double min_margin,
    int edge_blur_size,
    jyppx_ocv_features2d_mser** mser)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_mser_create")
        if (mser == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mser");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_mser{
            cv::MSER::create(delta, min_area, max_area, max_variation, min_diversity, max_evolution, area_threshold, min_margin, edge_blur_size)
        };
        if (result == nullptr)
        {
            *mser = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *mser = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (mser != nullptr)
        {
            *mser = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_mser_release(jyppx_ocv_features2d_mser* mser) { delete mser; }

OCV_CSHARP_FEATURE_META(jyppx_ocv_features2d_mser, jyppx_ocv_features2d_mser, "mser")

#define OCV_CSHARP_MSER_GET_INT(function_name, getter_name) \
int function_name(const jyppx_ocv_features2d_mser* mser, int* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, mser, "mser"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = mser->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_MSER_SET_INT(function_name, setter_name) \
int function_name(jyppx_ocv_features2d_mser* mser, int value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        int status = validate_feature(api_name, mser, "mser"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        mser->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_MSER_GET_DOUBLE(function_name, getter_name) \
int function_name(const jyppx_ocv_features2d_mser* mser, double* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, mser, "mser"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = mser->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_MSER_SET_DOUBLE(function_name, setter_name) \
int function_name(jyppx_ocv_features2d_mser* mser, double value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        int status = validate_feature(api_name, mser, "mser"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        mser->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

OCV_CSHARP_MSER_GET_INT(jyppx_ocv_features2d_mser_get_delta, getDelta)
OCV_CSHARP_MSER_SET_INT(jyppx_ocv_features2d_mser_set_delta, setDelta)
OCV_CSHARP_MSER_GET_INT(jyppx_ocv_features2d_mser_get_min_area, getMinArea)
OCV_CSHARP_MSER_SET_INT(jyppx_ocv_features2d_mser_set_min_area, setMinArea)
OCV_CSHARP_MSER_GET_INT(jyppx_ocv_features2d_mser_get_max_area, getMaxArea)
OCV_CSHARP_MSER_SET_INT(jyppx_ocv_features2d_mser_set_max_area, setMaxArea)
OCV_CSHARP_MSER_GET_DOUBLE(jyppx_ocv_features2d_mser_get_max_variation, getMaxVariation)
OCV_CSHARP_MSER_SET_DOUBLE(jyppx_ocv_features2d_mser_set_max_variation, setMaxVariation)
OCV_CSHARP_MSER_GET_DOUBLE(jyppx_ocv_features2d_mser_get_min_diversity, getMinDiversity)
OCV_CSHARP_MSER_SET_DOUBLE(jyppx_ocv_features2d_mser_set_min_diversity, setMinDiversity)
OCV_CSHARP_MSER_GET_INT(jyppx_ocv_features2d_mser_get_max_evolution, getMaxEvolution)
OCV_CSHARP_MSER_SET_INT(jyppx_ocv_features2d_mser_set_max_evolution, setMaxEvolution)
OCV_CSHARP_MSER_GET_DOUBLE(jyppx_ocv_features2d_mser_get_area_threshold, getAreaThreshold)
OCV_CSHARP_MSER_SET_DOUBLE(jyppx_ocv_features2d_mser_set_area_threshold, setAreaThreshold)
OCV_CSHARP_MSER_GET_DOUBLE(jyppx_ocv_features2d_mser_get_min_margin, getMinMargin)
OCV_CSHARP_MSER_SET_DOUBLE(jyppx_ocv_features2d_mser_set_min_margin, setMinMargin)
OCV_CSHARP_MSER_GET_INT(jyppx_ocv_features2d_mser_get_edge_blur_size, getEdgeBlurSize)
OCV_CSHARP_MSER_SET_INT(jyppx_ocv_features2d_mser_set_edge_blur_size, setEdgeBlurSize)

int jyppx_ocv_features2d_mser_get_pass2_only(const jyppx_ocv_features2d_mser* mser, int* value)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_mser_get_pass2_only")
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); }
        int status = validate_feature(api_name, mser, "mser");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = mser->value->getPass2Only() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_mser_set_pass2_only(jyppx_ocv_features2d_mser* mser, int value)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_mser_set_pass2_only")
        int status = validate_feature(api_name, mser, "mser");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        mser->value->setPass2Only(value != 0);
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_mser_detect_regions_count(
    const jyppx_ocv_features2d_mser* mser,
    const jyppx_ocv_mat* image,
    int* region_count,
    int* total_point_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_mser_detect_regions_count")
        int status = validate_feature(api_name, mser, "mser");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        std::vector<std::vector<cv::Point>> regions;
        std::vector<cv::Rect> bboxes;
        const_cast<jyppx_ocv_features2d_mser*>(mser)->value->detectRegions(opencv_csharp_native::mat_value(image), regions, bboxes);
        return summarize_regions(api_name, regions, region_count, total_point_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_mser_detect_regions_fill(
    const jyppx_ocv_features2d_mser* mser,
    const jyppx_ocv_mat* image,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_point* points,
    int point_capacity,
    jyppx_ocv_rect* bboxes,
    int bbox_capacity,
    int* region_count,
    int* total_point_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_mser_detect_regions_fill")
        int status = validate_non_negative_count(api_name, offset_capacity, "offset_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_non_negative_count(api_name, point_capacity, "point_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_non_negative_count(api_name, bbox_capacity, "bbox_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_feature(api_name, mser, "mser");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        std::vector<std::vector<cv::Point>> regions;
        std::vector<cv::Rect> native_bboxes;
        const_cast<jyppx_ocv_features2d_mser*>(mser)->value->detectRegions(opencv_csharp_native::mat_value(image), regions, native_bboxes);
        return copy_regions_to_output(api_name, regions, native_bboxes, offsets, offset_capacity, points, point_capacity, bboxes, bbox_capacity, region_count, total_point_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_simple_blob_create_default(jyppx_ocv_features2d_simple_blob** detector)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_simple_blob_create_default")
        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_simple_blob{ cv::SimpleBlobDetector::create() };
        if (result == nullptr)
        {
            *detector = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *detector = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (detector != nullptr)
        {
            *detector = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_simple_blob_create(const jyppx_ocv_simple_blob_params* parameters, jyppx_ocv_features2d_simple_blob** detector)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_simple_blob_create")
        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        int status = validate_simple_blob_params(api_name, parameters);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            *detector = nullptr;
            return status;
        }

        cv::SimpleBlobDetector::Params native_params = to_cv_simple_blob_params(*parameters);
        auto result = new (std::nothrow) jyppx_ocv_features2d_simple_blob{ cv::SimpleBlobDetector::create(native_params) };
        if (result == nullptr)
        {
            *detector = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *detector = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (detector != nullptr)
        {
            *detector = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_simple_blob_release(jyppx_ocv_features2d_simple_blob* detector) { delete detector; }

OCV_CSHARP_FEATURE_META(jyppx_ocv_features2d_simple_blob, jyppx_ocv_features2d_simple_blob, "detector")

int jyppx_ocv_features2d_simple_blob_get_params(const jyppx_ocv_features2d_simple_blob* detector, jyppx_ocv_simple_blob_params* parameters)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_simple_blob_get_params")
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }

        int status = validate_feature(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *parameters = from_cv_simple_blob_params(detector->value->getParams());
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_simple_blob_set_params(jyppx_ocv_features2d_simple_blob* detector, const jyppx_ocv_simple_blob_params* parameters)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_simple_blob_set_params")
        int status = validate_feature(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_simple_blob_params(api_name, parameters);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        detector->value->setParams(to_cv_simple_blob_params(*parameters));
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_simple_blob_get_blob_contours_count(
    const jyppx_ocv_features2d_simple_blob* detector,
    int* contour_count,
    int* total_point_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_simple_blob_get_blob_contours_count")
        int status = validate_feature(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return summarize_regions(api_name, detector->value->getBlobContours(), contour_count, total_point_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_simple_blob_get_blob_contours_fill(
    const jyppx_ocv_features2d_simple_blob* detector,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_point* points,
    int point_capacity,
    int* contour_count,
    int* total_point_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_simple_blob_get_blob_contours_fill")
        int status = validate_non_negative_count(api_name, offset_capacity, "offset_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_non_negative_count(api_name, point_capacity, "point_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_feature(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return copy_contours_to_output(api_name, detector->value->getBlobContours(), offsets, offset_capacity, points, point_capacity, contour_count, total_point_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV_XFEATURES2D)

int jyppx_ocv_features2d_brisk_create(int threshold, int octaves, float pattern_scale, jyppx_ocv_features2d_brisk** brisk)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_brisk_create")
        if (brisk == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "brisk");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_brisk{ cv::xfeatures2d::BRISK::create(threshold, octaves, pattern_scale) };
        if (result == nullptr)
        {
            *brisk = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *brisk = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (brisk != nullptr)
        {
            *brisk = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_brisk_create_pattern(
    const float* radius_list,
    int radius_count,
    const int* number_list,
    int number_count,
    float d_max,
    float d_min,
    const int* index_change,
    int index_change_count,
    jyppx_ocv_features2d_brisk** brisk)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_brisk_create_pattern")
        if (brisk == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "brisk");
        }

        if (radius_count != number_count)
        {
            *brisk = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "number_list");
        }

        int status = OPENCV_CSHARP_STATUS_OK;
        std::vector<float> radius = to_float_vector(api_name, radius_list, radius_count, "radius_list", status);
        if (status != OPENCV_CSHARP_STATUS_OK) { *brisk = nullptr; return status; }
        std::vector<int> number = to_int_vector(api_name, number_list, number_count, "number_list", status);
        if (status != OPENCV_CSHARP_STATUS_OK) { *brisk = nullptr; return status; }
        std::vector<int> index = to_int_vector(api_name, index_change, index_change_count, "index_change", status);
        if (status != OPENCV_CSHARP_STATUS_OK) { *brisk = nullptr; return status; }

        auto result = new (std::nothrow) jyppx_ocv_features2d_brisk{ cv::xfeatures2d::BRISK::create(radius, number, d_max, d_min, index) };
        if (result == nullptr)
        {
            *brisk = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *brisk = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (brisk != nullptr)
        {
            *brisk = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_brisk_create_pattern_with_threshold(
    int threshold,
    int octaves,
    const float* radius_list,
    int radius_count,
    const int* number_list,
    int number_count,
    float d_max,
    float d_min,
    const int* index_change,
    int index_change_count,
    jyppx_ocv_features2d_brisk** brisk)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_brisk_create_pattern_with_threshold")
        if (brisk == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "brisk");
        }

        if (radius_count != number_count)
        {
            *brisk = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "number_list");
        }

        int status = OPENCV_CSHARP_STATUS_OK;
        std::vector<float> radius = to_float_vector(api_name, radius_list, radius_count, "radius_list", status);
        if (status != OPENCV_CSHARP_STATUS_OK) { *brisk = nullptr; return status; }
        std::vector<int> number = to_int_vector(api_name, number_list, number_count, "number_list", status);
        if (status != OPENCV_CSHARP_STATUS_OK) { *brisk = nullptr; return status; }
        std::vector<int> index = to_int_vector(api_name, index_change, index_change_count, "index_change", status);
        if (status != OPENCV_CSHARP_STATUS_OK) { *brisk = nullptr; return status; }

        auto result = new (std::nothrow) jyppx_ocv_features2d_brisk{ cv::xfeatures2d::BRISK::create(threshold, octaves, radius, number, d_max, d_min, index) };
        if (result == nullptr)
        {
            *brisk = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *brisk = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (brisk != nullptr)
        {
            *brisk = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_brisk_release(jyppx_ocv_features2d_brisk* brisk) { delete brisk; }

OCV_CSHARP_FEATURE_META(jyppx_ocv_features2d_brisk, jyppx_ocv_features2d_brisk, "brisk")
OCV_CSHARP_FEATURE_DESCRIPTORS(jyppx_ocv_features2d_brisk, jyppx_ocv_features2d_brisk, "brisk")

#define OCV_CSHARP_XFEATURE_GET_INT(function_name, handle_type, parameter_name, getter_name) \
int function_name(const handle_type* handle, int* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, handle, parameter_name); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = handle->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_XFEATURE_GET_BOOL(function_name, handle_type, parameter_name, getter_name) \
int function_name(const handle_type* handle, int* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, handle, parameter_name); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = handle->value->getter_name() ? 1 : 0; \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_XFEATURE_SET_INT(function_name, handle_type, parameter_name, setter_name) \
int function_name(handle_type* handle, int value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        int status = validate_feature(api_name, handle, parameter_name); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        handle->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_XFEATURE_SET_BOOL(function_name, handle_type, parameter_name, setter_name) \
int function_name(handle_type* handle, int value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        int status = validate_feature(api_name, handle, parameter_name); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        handle->value->setter_name(value != 0); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_XFEATURE_GET_DOUBLE(function_name, handle_type, parameter_name, getter_name) \
int function_name(const handle_type* handle, double* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, handle, parameter_name); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = handle->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_XFEATURE_SET_DOUBLE(function_name, handle_type, parameter_name, setter_name) \
int function_name(handle_type* handle, double value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        int status = validate_feature(api_name, handle, parameter_name); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        handle->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_XFEATURE_GET_FLOAT(function_name, handle_type, parameter_name, getter_name) \
int function_name(const handle_type* handle, float* value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
        int status = validate_feature(api_name, handle, parameter_name); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = handle->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

#define OCV_CSHARP_XFEATURE_SET_FLOAT(function_name, handle_type, parameter_name, setter_name) \
int function_name(handle_type* handle, float value) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        int status = validate_feature(api_name, handle, parameter_name); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        handle->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_brisk_get_threshold, jyppx_ocv_features2d_brisk, "brisk", getThreshold)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_brisk_set_threshold, jyppx_ocv_features2d_brisk, "brisk", setThreshold)
OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_brisk_get_octaves, jyppx_ocv_features2d_brisk, "brisk", getOctaves)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_brisk_set_octaves, jyppx_ocv_features2d_brisk, "brisk", setOctaves)
OCV_CSHARP_XFEATURE_GET_FLOAT(jyppx_ocv_features2d_brisk_get_pattern_scale, jyppx_ocv_features2d_brisk, "brisk", getPatternScale)
OCV_CSHARP_XFEATURE_SET_FLOAT(jyppx_ocv_features2d_brisk_set_pattern_scale, jyppx_ocv_features2d_brisk, "brisk", setPatternScale)

int jyppx_ocv_features2d_kaze_create(int extended, int upright, float threshold, int n_octaves, int n_octave_layers, int diffusivity, jyppx_ocv_features2d_kaze** kaze)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_kaze_create")
        if (kaze == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "kaze");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_kaze{ cv::xfeatures2d::KAZE::create(extended != 0, upright != 0, threshold, n_octaves, n_octave_layers, diffusivity) };
        if (result == nullptr)
        {
            *kaze = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *kaze = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (kaze != nullptr)
        {
            *kaze = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_kaze_release(jyppx_ocv_features2d_kaze* kaze) { delete kaze; }

OCV_CSHARP_FEATURE_META(jyppx_ocv_features2d_kaze, jyppx_ocv_features2d_kaze, "kaze")
OCV_CSHARP_FEATURE_DESCRIPTORS(jyppx_ocv_features2d_kaze, jyppx_ocv_features2d_kaze, "kaze")

OCV_CSHARP_XFEATURE_GET_BOOL(jyppx_ocv_features2d_kaze_get_extended, jyppx_ocv_features2d_kaze, "kaze", getExtended)
OCV_CSHARP_XFEATURE_SET_BOOL(jyppx_ocv_features2d_kaze_set_extended, jyppx_ocv_features2d_kaze, "kaze", setExtended)
OCV_CSHARP_XFEATURE_GET_BOOL(jyppx_ocv_features2d_kaze_get_upright, jyppx_ocv_features2d_kaze, "kaze", getUpright)
OCV_CSHARP_XFEATURE_SET_BOOL(jyppx_ocv_features2d_kaze_set_upright, jyppx_ocv_features2d_kaze, "kaze", setUpright)
OCV_CSHARP_XFEATURE_GET_DOUBLE(jyppx_ocv_features2d_kaze_get_threshold, jyppx_ocv_features2d_kaze, "kaze", getThreshold)
OCV_CSHARP_XFEATURE_SET_DOUBLE(jyppx_ocv_features2d_kaze_set_threshold, jyppx_ocv_features2d_kaze, "kaze", setThreshold)
OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_kaze_get_n_octaves, jyppx_ocv_features2d_kaze, "kaze", getNOctaves)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_kaze_set_n_octaves, jyppx_ocv_features2d_kaze, "kaze", setNOctaves)
OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_kaze_get_n_octave_layers, jyppx_ocv_features2d_kaze, "kaze", getNOctaveLayers)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_kaze_set_n_octave_layers, jyppx_ocv_features2d_kaze, "kaze", setNOctaveLayers)
OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_kaze_get_diffusivity, jyppx_ocv_features2d_kaze, "kaze", getDiffusivity)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_kaze_set_diffusivity, jyppx_ocv_features2d_kaze, "kaze", setDiffusivity)

int jyppx_ocv_features2d_akaze_create(int descriptor_type, int descriptor_size, int descriptor_channels, float threshold, int n_octaves, int n_octave_layers, int diffusivity, int max_points, jyppx_ocv_features2d_akaze** akaze)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_akaze_create")
        if (akaze == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "akaze");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_akaze{ cv::xfeatures2d::AKAZE::create(descriptor_type, descriptor_size, descriptor_channels, threshold, n_octaves, n_octave_layers, diffusivity, max_points) };
        if (result == nullptr)
        {
            *akaze = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *akaze = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (akaze != nullptr)
        {
            *akaze = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_akaze_release(jyppx_ocv_features2d_akaze* akaze) { delete akaze; }

OCV_CSHARP_FEATURE_META(jyppx_ocv_features2d_akaze, jyppx_ocv_features2d_akaze, "akaze")
OCV_CSHARP_FEATURE_DESCRIPTORS(jyppx_ocv_features2d_akaze, jyppx_ocv_features2d_akaze, "akaze")

OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_akaze_get_descriptor_type, jyppx_ocv_features2d_akaze, "akaze", getDescriptorType)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_akaze_set_descriptor_type, jyppx_ocv_features2d_akaze, "akaze", setDescriptorType)
OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_akaze_get_descriptor_size, jyppx_ocv_features2d_akaze, "akaze", getDescriptorSize)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_akaze_set_descriptor_size, jyppx_ocv_features2d_akaze, "akaze", setDescriptorSize)
OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_akaze_get_descriptor_channels, jyppx_ocv_features2d_akaze, "akaze", getDescriptorChannels)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_akaze_set_descriptor_channels, jyppx_ocv_features2d_akaze, "akaze", setDescriptorChannels)
OCV_CSHARP_XFEATURE_GET_DOUBLE(jyppx_ocv_features2d_akaze_get_threshold, jyppx_ocv_features2d_akaze, "akaze", getThreshold)
OCV_CSHARP_XFEATURE_SET_DOUBLE(jyppx_ocv_features2d_akaze_set_threshold, jyppx_ocv_features2d_akaze, "akaze", setThreshold)
OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_akaze_get_n_octaves, jyppx_ocv_features2d_akaze, "akaze", getNOctaves)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_akaze_set_n_octaves, jyppx_ocv_features2d_akaze, "akaze", setNOctaves)
OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_akaze_get_n_octave_layers, jyppx_ocv_features2d_akaze, "akaze", getNOctaveLayers)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_akaze_set_n_octave_layers, jyppx_ocv_features2d_akaze, "akaze", setNOctaveLayers)
OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_akaze_get_diffusivity, jyppx_ocv_features2d_akaze, "akaze", getDiffusivity)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_akaze_set_diffusivity, jyppx_ocv_features2d_akaze, "akaze", setDiffusivity)
OCV_CSHARP_XFEATURE_GET_INT(jyppx_ocv_features2d_akaze_get_max_points, jyppx_ocv_features2d_akaze, "akaze", getMaxPoints)
OCV_CSHARP_XFEATURE_SET_INT(jyppx_ocv_features2d_akaze_set_max_points, jyppx_ocv_features2d_akaze, "akaze", setMaxPoints)

#undef OCV_CSHARP_XFEATURE_SET_FLOAT
#undef OCV_CSHARP_XFEATURE_GET_FLOAT
#undef OCV_CSHARP_XFEATURE_SET_DOUBLE
#undef OCV_CSHARP_XFEATURE_GET_DOUBLE
#undef OCV_CSHARP_XFEATURE_SET_BOOL
#undef OCV_CSHARP_XFEATURE_SET_INT
#undef OCV_CSHARP_XFEATURE_GET_BOOL
#undef OCV_CSHARP_XFEATURE_GET_INT

#else

int jyppx_ocv_features2d_brisk_create(int, int, float, jyppx_ocv_features2d_brisk** brisk) { if (brisk != nullptr) { *brisk = nullptr; } return xfeatures_not_linked("jyppx_ocv_features2d_brisk_create"); }
int jyppx_ocv_features2d_brisk_create_pattern(const float*, int, const int*, int, float, float, const int*, int, jyppx_ocv_features2d_brisk** brisk) { if (brisk != nullptr) { *brisk = nullptr; } return xfeatures_not_linked("jyppx_ocv_features2d_brisk_create_pattern"); }
int jyppx_ocv_features2d_brisk_create_pattern_with_threshold(int, int, const float*, int, const int*, int, float, float, const int*, int, jyppx_ocv_features2d_brisk** brisk) { if (brisk != nullptr) { *brisk = nullptr; } return xfeatures_not_linked("jyppx_ocv_features2d_brisk_create_pattern_with_threshold"); }
void jyppx_ocv_features2d_brisk_release(jyppx_ocv_features2d_brisk* brisk) { delete brisk; }
int jyppx_ocv_features2d_kaze_create(int, int, float, int, int, int, jyppx_ocv_features2d_kaze** kaze) { if (kaze != nullptr) { *kaze = nullptr; } return xfeatures_not_linked("jyppx_ocv_features2d_kaze_create"); }
void jyppx_ocv_features2d_kaze_release(jyppx_ocv_features2d_kaze* kaze) { delete kaze; }
int jyppx_ocv_features2d_akaze_create(int, int, int, float, int, int, int, int, jyppx_ocv_features2d_akaze** akaze) { if (akaze != nullptr) { *akaze = nullptr; } return xfeatures_not_linked("jyppx_ocv_features2d_akaze_create"); }
void jyppx_ocv_features2d_akaze_release(jyppx_ocv_features2d_akaze* akaze) { delete akaze; }

#define OCV_CSHARP_XFEATURE_STUB_INT_OUT(function_name, handle_type) int function_name(const handle_type*, int* value) { set_zero(value); return xfeatures_not_linked(#function_name); }
#define OCV_CSHARP_XFEATURE_STUB_DOUBLE_OUT(function_name, handle_type) int function_name(const handle_type*, double* value) { set_zero(value); return xfeatures_not_linked(#function_name); }
#define OCV_CSHARP_XFEATURE_STUB_FLOAT_OUT(function_name, handle_type) int function_name(const handle_type*, float* value) { set_zero(value); return xfeatures_not_linked(#function_name); }
#define OCV_CSHARP_XFEATURE_STUB_SET_INT(function_name, handle_type) int function_name(handle_type*, int) { return xfeatures_not_linked(#function_name); }
#define OCV_CSHARP_XFEATURE_STUB_SET_DOUBLE(function_name, handle_type) int function_name(handle_type*, double) { return xfeatures_not_linked(#function_name); }
#define OCV_CSHARP_XFEATURE_STUB_SET_FLOAT(function_name, handle_type) int function_name(handle_type*, float) { return xfeatures_not_linked(#function_name); }
#define OCV_CSHARP_XFEATURE_STUB_FEATURE(prefix, handle_type) \
int prefix##_clear(handle_type*) { return xfeatures_not_linked(#prefix "_clear"); } \
OCV_CSHARP_XFEATURE_STUB_INT_OUT(prefix##_empty, handle_type) \
OCV_CSHARP_XFEATURE_STUB_INT_OUT(prefix##_descriptor_size, handle_type) \
OCV_CSHARP_XFEATURE_STUB_INT_OUT(prefix##_descriptor_type, handle_type) \
OCV_CSHARP_XFEATURE_STUB_INT_OUT(prefix##_default_norm, handle_type) \
OCV_CSHARP_XFEATURE_STUB_INT_OUT(prefix##_default_name_length, handle_type) \
int prefix##_default_name_fill(const handle_type*, char*, int, int* written) { set_zero(written); return xfeatures_not_linked(#prefix "_default_name_fill"); } \
int prefix##_detect_count(const handle_type*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, int* count) { set_zero(count); return xfeatures_not_linked(#prefix "_detect_count"); } \
int prefix##_detect_fill(const handle_type*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, jyppx_ocv_key_point*, int, int* count) { set_zero(count); return xfeatures_not_linked(#prefix "_detect_fill"); }
#define OCV_CSHARP_XFEATURE_STUB_DESCRIPTORS(prefix, handle_type) \
int prefix##_compute(const handle_type*, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, jyppx_ocv_key_point*, int, int* count, jyppx_ocv_mat*) { set_zero(count); return xfeatures_not_linked(#prefix "_compute"); } \
int prefix##_detect_and_compute_count(const handle_type*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, int, int* count) { set_zero(count); return xfeatures_not_linked(#prefix "_detect_and_compute_count"); } \
int prefix##_detect_and_compute_fill(const handle_type*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, int, jyppx_ocv_key_point*, int, int* count, jyppx_ocv_mat*) { set_zero(count); return xfeatures_not_linked(#prefix "_detect_and_compute_fill"); }

OCV_CSHARP_XFEATURE_STUB_FEATURE(jyppx_ocv_features2d_brisk, jyppx_ocv_features2d_brisk)
OCV_CSHARP_XFEATURE_STUB_DESCRIPTORS(jyppx_ocv_features2d_brisk, jyppx_ocv_features2d_brisk)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_brisk_get_threshold, jyppx_ocv_features2d_brisk)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_brisk_set_threshold, jyppx_ocv_features2d_brisk)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_brisk_get_octaves, jyppx_ocv_features2d_brisk)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_brisk_set_octaves, jyppx_ocv_features2d_brisk)
OCV_CSHARP_XFEATURE_STUB_FLOAT_OUT(jyppx_ocv_features2d_brisk_get_pattern_scale, jyppx_ocv_features2d_brisk)
OCV_CSHARP_XFEATURE_STUB_SET_FLOAT(jyppx_ocv_features2d_brisk_set_pattern_scale, jyppx_ocv_features2d_brisk)

OCV_CSHARP_XFEATURE_STUB_FEATURE(jyppx_ocv_features2d_kaze, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_DESCRIPTORS(jyppx_ocv_features2d_kaze, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_kaze_get_extended, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_kaze_set_extended, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_kaze_get_upright, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_kaze_set_upright, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_DOUBLE_OUT(jyppx_ocv_features2d_kaze_get_threshold, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_SET_DOUBLE(jyppx_ocv_features2d_kaze_set_threshold, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_kaze_get_n_octaves, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_kaze_set_n_octaves, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_kaze_get_n_octave_layers, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_kaze_set_n_octave_layers, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_kaze_get_diffusivity, jyppx_ocv_features2d_kaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_kaze_set_diffusivity, jyppx_ocv_features2d_kaze)

OCV_CSHARP_XFEATURE_STUB_FEATURE(jyppx_ocv_features2d_akaze, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_DESCRIPTORS(jyppx_ocv_features2d_akaze, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_descriptor_type, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_descriptor_type, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_descriptor_size, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_descriptor_size, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_descriptor_channels, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_descriptor_channels, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_DOUBLE_OUT(jyppx_ocv_features2d_akaze_get_threshold, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_SET_DOUBLE(jyppx_ocv_features2d_akaze_set_threshold, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_n_octaves, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_n_octaves, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_n_octave_layers, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_n_octave_layers, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_diffusivity, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_diffusivity, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_max_points, jyppx_ocv_features2d_akaze)
OCV_CSHARP_XFEATURE_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_max_points, jyppx_ocv_features2d_akaze)

#undef OCV_CSHARP_XFEATURE_STUB_DESCRIPTORS
#undef OCV_CSHARP_XFEATURE_STUB_FEATURE
#undef OCV_CSHARP_XFEATURE_STUB_SET_FLOAT
#undef OCV_CSHARP_XFEATURE_STUB_SET_DOUBLE
#undef OCV_CSHARP_XFEATURE_STUB_SET_INT
#undef OCV_CSHARP_XFEATURE_STUB_FLOAT_OUT
#undef OCV_CSHARP_XFEATURE_STUB_DOUBLE_OUT
#undef OCV_CSHARP_XFEATURE_STUB_INT_OUT

#endif

int jyppx_ocv_features2d_flann_matcher_create(jyppx_ocv_features2d_flann_matcher** matcher)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_create")
        if (matcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_flann_matcher{ cv::FlannBasedMatcher::create() };
        if (result == nullptr)
        {
            *matcher = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *matcher = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (matcher != nullptr)
        {
            *matcher = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_flann_matcher_release(jyppx_ocv_features2d_flann_matcher* matcher) { delete matcher; }

int jyppx_ocv_features2d_flann_matcher_clone(const jyppx_ocv_features2d_flann_matcher* matcher, int empty_train_data, jyppx_ocv_features2d_descriptor_matcher** clone)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_clone")
        if (clone == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "clone");
        }

        *clone = nullptr;
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return wrap_descriptor_matcher(api_name, matcher->value->clone(empty_train_data != 0), clone);
    } catch (...)
    {
        if (clone != nullptr)
        {
            *clone = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_flann_matcher_is_mask_supported(const jyppx_ocv_features2d_flann_matcher* matcher, int* supported)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_is_mask_supported")
        if (supported == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "supported"); }
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *supported = matcher->value->isMaskSupported() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_empty(const jyppx_ocv_features2d_flann_matcher* matcher, int* empty)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_empty")
        if (empty == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "empty"); }
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *empty = matcher->value->empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_clear(jyppx_ocv_features2d_flann_matcher* matcher)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_clear")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        matcher->value->clear();
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_train(jyppx_ocv_features2d_flann_matcher* matcher)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_train")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        matcher->value->train();
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_add(jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* const* descriptors, int descriptor_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_add")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::Mat> mats = to_mat_vector(api_name, descriptors, descriptor_count, status);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        matcher->value->add(mats);
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_get_train_descriptors_count(const jyppx_ocv_features2d_flann_matcher* matcher, int* descriptor_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_get_train_descriptors_count")
        if (descriptor_count == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "descriptor_count"); }
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *descriptor_count = static_cast<int>(matcher->value->getTrainDescriptors().size());
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_get_train_descriptor_clone(const jyppx_ocv_features2d_flann_matcher* matcher, int index, jyppx_ocv_mat** descriptor)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_get_train_descriptor_clone")
        if (descriptor == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "descriptor"); }
        *descriptor = nullptr;
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        const std::vector<cv::Mat>& descriptors = matcher->value->getTrainDescriptors();
        if (index < 0 || index >= static_cast<int>(descriptors.size()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }

        return clone_mat_to_handle(api_name, descriptors[static_cast<size_t>(index)], descriptor);
    } catch (...)
    {
        if (descriptor != nullptr)
        {
            *descriptor = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

#define OCV_CSHARP_FLANN_MATCH_CORE(method_suffix, method_call) \
int jyppx_ocv_features2d_flann_matcher_##method_suffix##_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, int* match_count) \
{ \
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_" #method_suffix "_count") \
        if (match_count == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "match_count"); } \
        int status = validate_flann(api_name, matcher); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        status = validate_mat(api_name, query_descriptors, "query_descriptors"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        status = validate_mat(api_name, train_descriptors, "train_descriptors"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        std::vector<cv::DMatch> native_matches; \
        matcher->value->method_call(opencv_csharp_native::mat_value(query_descriptors), opencv_csharp_native::mat_value(train_descriptors), native_matches); \
        *match_count = static_cast<int>(native_matches.size()); \
        return OPENCV_CSHARP_STATUS_OK; \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
} \
int jyppx_ocv_features2d_flann_matcher_##method_suffix##_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, jyppx_ocv_dmatch* matches, int match_capacity, int* match_count) \
{ \
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_" #method_suffix "_fill") \
        int status = validate_flann(api_name, matcher); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        status = validate_mat(api_name, query_descriptors, "query_descriptors"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        status = validate_mat(api_name, train_descriptors, "train_descriptors"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        std::vector<cv::DMatch> native_matches; \
        matcher->value->method_call(opencv_csharp_native::mat_value(query_descriptors), opencv_csharp_native::mat_value(train_descriptors), native_matches); \
        return copy_matches_to_output(api_name, native_matches, matches, match_capacity, match_count); \
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); } \
}

OCV_CSHARP_FLANN_MATCH_CORE(match, match)

int jyppx_ocv_features2d_flann_matcher_match_train_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, int* match_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_match_train_count")
        if (match_count == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "match_count"); }
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query_descriptors, "query_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::DMatch> native_matches;
        const_cast<jyppx_ocv_features2d_flann_matcher*>(matcher)->value->match(opencv_csharp_native::mat_value(query_descriptors), native_matches);
        *match_count = static_cast<int>(native_matches.size());
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_match_train_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, jyppx_ocv_dmatch* matches, int match_capacity, int* match_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_match_train_fill")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query_descriptors, "query_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::DMatch> native_matches;
        const_cast<jyppx_ocv_features2d_flann_matcher*>(matcher)->value->match(opencv_csharp_native::mat_value(query_descriptors), native_matches);
        return copy_matches_to_output(api_name, native_matches, matches, match_capacity, match_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_knn_match_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, int k, int compact_result, int* group_count, int* total_match_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_knn_match_count")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query_descriptors, "query_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, train_descriptors, "train_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<std::vector<cv::DMatch>> native_matches;
        matcher->value->knnMatch(opencv_csharp_native::mat_value(query_descriptors), opencv_csharp_native::mat_value(train_descriptors), native_matches, k, cv::noArray(), compact_result != 0);
        return summarize_grouped_matches(api_name, native_matches, group_count, total_match_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_knn_match_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, int k, int compact_result, int* offsets, int offset_capacity, jyppx_ocv_dmatch* matches, int match_capacity, int* group_count, int* total_match_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_knn_match_fill")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query_descriptors, "query_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, train_descriptors, "train_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<std::vector<cv::DMatch>> native_matches;
        matcher->value->knnMatch(opencv_csharp_native::mat_value(query_descriptors), opencv_csharp_native::mat_value(train_descriptors), native_matches, k, cv::noArray(), compact_result != 0);
        return copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_knn_match_train_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, int k, int compact_result, int* group_count, int* total_match_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_knn_match_train_count")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query_descriptors, "query_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<std::vector<cv::DMatch>> native_matches;
        const_cast<jyppx_ocv_features2d_flann_matcher*>(matcher)->value->knnMatch(opencv_csharp_native::mat_value(query_descriptors), native_matches, k, cv::noArray(), compact_result != 0);
        return summarize_grouped_matches(api_name, native_matches, group_count, total_match_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_knn_match_train_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, int k, int compact_result, int* offsets, int offset_capacity, jyppx_ocv_dmatch* matches, int match_capacity, int* group_count, int* total_match_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_knn_match_train_fill")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query_descriptors, "query_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<std::vector<cv::DMatch>> native_matches;
        const_cast<jyppx_ocv_features2d_flann_matcher*>(matcher)->value->knnMatch(opencv_csharp_native::mat_value(query_descriptors), native_matches, k, cv::noArray(), compact_result != 0);
        return copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_radius_match_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, float max_distance, int compact_result, int* group_count, int* total_match_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_radius_match_count")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query_descriptors, "query_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, train_descriptors, "train_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<std::vector<cv::DMatch>> native_matches;
        matcher->value->radiusMatch(opencv_csharp_native::mat_value(query_descriptors), opencv_csharp_native::mat_value(train_descriptors), native_matches, max_distance, cv::noArray(), compact_result != 0);
        return summarize_grouped_matches(api_name, native_matches, group_count, total_match_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_radius_match_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, const jyppx_ocv_mat* train_descriptors, float max_distance, int compact_result, int* offsets, int offset_capacity, jyppx_ocv_dmatch* matches, int match_capacity, int* group_count, int* total_match_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_radius_match_fill")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query_descriptors, "query_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, train_descriptors, "train_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<std::vector<cv::DMatch>> native_matches;
        matcher->value->radiusMatch(opencv_csharp_native::mat_value(query_descriptors), opencv_csharp_native::mat_value(train_descriptors), native_matches, max_distance, cv::noArray(), compact_result != 0);
        return copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_radius_match_train_count(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, float max_distance, int compact_result, int* group_count, int* total_match_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_radius_match_train_count")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query_descriptors, "query_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<std::vector<cv::DMatch>> native_matches;
        const_cast<jyppx_ocv_features2d_flann_matcher*>(matcher)->value->radiusMatch(opencv_csharp_native::mat_value(query_descriptors), native_matches, max_distance, cv::noArray(), compact_result != 0);
        return summarize_grouped_matches(api_name, native_matches, group_count, total_match_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_flann_matcher_radius_match_train_fill(const jyppx_ocv_features2d_flann_matcher* matcher, const jyppx_ocv_mat* query_descriptors, float max_distance, int compact_result, int* offsets, int offset_capacity, jyppx_ocv_dmatch* matches, int match_capacity, int* group_count, int* total_match_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_flann_matcher_radius_match_train_fill")
        int status = validate_flann(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query_descriptors, "query_descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<std::vector<cv::DMatch>> native_matches;
        const_cast<jyppx_ocv_features2d_flann_matcher*>(matcher)->value->radiusMatch(opencv_csharp_native::mat_value(query_descriptors), native_matches, max_distance, cv::noArray(), compact_result != 0);
        return copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count);
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

#define OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND(function_name, backend_type, parameter_name) \
int function_name(const backend_type* backend, int max_tilt, int min_tilt, float tilt_step, float rotate_step_base, jyppx_ocv_features2d_affine** affine) \
{ \
    OCV_CSHARP_TRY_BEGIN(#function_name) \
        return create_affine_from_backend(api_name, backend, parameter_name, max_tilt, min_tilt, tilt_step, rotate_step_base, affine); \
    } catch (...) { if (affine != nullptr) { *affine = nullptr; } return opencv_csharp_native::translate_current_exception(api_name); } \
}

OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND(jyppx_ocv_features2d_affine_create_from_orb, jyppx_ocv_features2d_orb, "backend")
OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND(jyppx_ocv_features2d_affine_create_from_sift, jyppx_ocv_features2d_sift, "backend")
OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND(jyppx_ocv_features2d_affine_create_from_fast, jyppx_ocv_features2d_fast, "backend")
OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND(jyppx_ocv_features2d_affine_create_from_gftt, jyppx_ocv_features2d_gftt, "backend")
OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND(jyppx_ocv_features2d_affine_create_from_mser, jyppx_ocv_features2d_mser, "backend")
OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND(jyppx_ocv_features2d_affine_create_from_simple_blob, jyppx_ocv_features2d_simple_blob, "backend")

#if defined(OPENCV_CSHARP_HAS_OPENCV_XFEATURES2D)
OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND(jyppx_ocv_features2d_affine_create_from_brisk, jyppx_ocv_features2d_brisk, "backend")
OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND(jyppx_ocv_features2d_affine_create_from_kaze, jyppx_ocv_features2d_kaze, "backend")
OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND(jyppx_ocv_features2d_affine_create_from_akaze, jyppx_ocv_features2d_akaze, "backend")
#else
int jyppx_ocv_features2d_affine_create_from_brisk(const jyppx_ocv_features2d_brisk*, int, int, float, float, jyppx_ocv_features2d_affine** affine)
{
    if (affine != nullptr)
    {
        *affine = nullptr;
    }

    return xfeatures_not_linked("jyppx_ocv_features2d_affine_create_from_brisk");
}

int jyppx_ocv_features2d_affine_create_from_kaze(const jyppx_ocv_features2d_kaze*, int, int, float, float, jyppx_ocv_features2d_affine** affine)
{
    if (affine != nullptr)
    {
        *affine = nullptr;
    }

    return xfeatures_not_linked("jyppx_ocv_features2d_affine_create_from_kaze");
}

int jyppx_ocv_features2d_affine_create_from_akaze(const jyppx_ocv_features2d_akaze*, int, int, float, float, jyppx_ocv_features2d_affine** affine)
{
    if (affine != nullptr)
    {
        *affine = nullptr;
    }

    return xfeatures_not_linked("jyppx_ocv_features2d_affine_create_from_akaze");
}
#endif

void jyppx_ocv_features2d_affine_release(jyppx_ocv_features2d_affine* affine)
{
    delete affine;
}

OCV_CSHARP_FEATURE_META(jyppx_ocv_features2d_affine, jyppx_ocv_features2d_affine, "affine")

int jyppx_ocv_features2d_affine_set_view_params(jyppx_ocv_features2d_affine* affine, const float* tilts, int tilt_count, const float* rolls, int roll_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_affine_set_view_params")
        int status = validate_feature(api_name, affine, "affine");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (tilt_count != roll_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "roll_count");
        }

        std::vector<float> native_tilts = to_float_vector(api_name, tilts, tilt_count, "tilts", status);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<float> native_rolls = to_float_vector(api_name, rolls, roll_count, "rolls", status);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        affine->value->setViewParams(native_tilts, native_rolls);
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_affine_get_view_params_count(const jyppx_ocv_features2d_affine* affine, int* tilt_count, int* roll_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_affine_get_view_params_count")
        if (tilt_count == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "tilt_count"); }
        if (roll_count == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "roll_count"); }
        int status = validate_feature(api_name, affine, "affine");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<float> tilts;
        std::vector<float> rolls;
        affine->value->getViewParams(tilts, rolls);
        *tilt_count = static_cast<int>(tilts.size());
        *roll_count = static_cast<int>(rolls.size());
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_affine_get_view_params_fill(
    const jyppx_ocv_features2d_affine* affine,
    float* tilts,
    int tilt_capacity,
    float* rolls,
    int roll_capacity,
    int* tilt_count,
    int* roll_count)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_affine_get_view_params_fill")
        int status = validate_non_negative_count(api_name, tilt_capacity, "tilt_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_non_negative_count(api_name, roll_capacity, "roll_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_feature(api_name, affine, "affine");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<float> native_tilts;
        std::vector<float> native_rolls;
        affine->value->getViewParams(native_tilts, native_rolls);
        status = copy_floats_to_output(api_name, native_tilts, tilts, tilt_capacity, tilt_count, "tilts");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_floats_to_output(api_name, native_rolls, rolls, roll_capacity, roll_count, "rolls");
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_ann_index_create(
    int dimension,
    int distance,
    jyppx_ocv_features2d_ann_index** index)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_ann_index_create")
        if (index == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }
        *index = nullptr;
        if (dimension <= 0 || distance < static_cast<int>(cv::ANNIndex::DIST_EUCLIDEAN) ||
            distance > static_cast<int>(cv::ANNIndex::DIST_DOTPRODUCT))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, dimension <= 0 ? "dimension" : "distance");
        }

        const auto native_distance = static_cast<cv::ANNIndex::Distance>(distance);
        auto result = new (std::nothrow) jyppx_ocv_features2d_ann_index{
            cv::ANNIndex::create(dimension, native_distance),
            dimension,
            native_distance,
            {},
            {}
        };
        if (result == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        if (result->value.empty())
        {
            delete result;
            return opencv_csharp_native::set_invalid_argument(api_name, "distance");
        }

        *index = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (index != nullptr)
        {
            *index = nullptr;
        }
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_ann_index_release(jyppx_ocv_features2d_ann_index* index)
{
    delete index;
}

int jyppx_ocv_features2d_ann_index_add_items(
    jyppx_ocv_features2d_ann_index* index,
    const jyppx_ocv_mat* features)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_ann_index_add_items")
        int status = validate_ann_index(api_name, index);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_ann_features(api_name, index, features, "features", false);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        index->value->addItems(opencv_csharp_native::mat_value(features));
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_ann_index_build(
    jyppx_ocv_features2d_ann_index* index,
    int trees)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_ann_index_build")
        int status = validate_ann_index(api_name, index);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (trees == 0 || trees < -1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "trees");
        }
        index->value->build(trees);
#if defined(_WIN32)
        if (!index->on_disk_target_path.empty())
        {
            status = copy_ann_file(api_name, index->temporary_path, index->on_disk_target_path);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
#endif
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_ann_index_knn_search(
    const jyppx_ocv_features2d_ann_index* index,
    const jyppx_ocv_mat* query,
    jyppx_ocv_mat* indices,
    jyppx_ocv_mat* distances,
    int knn,
    int search_k)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_ann_index_knn_search")
        int status = validate_ann_index(api_name, index);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_ann_features(api_name, index, query, "query", true);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (indices == nullptr || distances == nullptr || query == indices || query == distances || indices == distances)
        {
            return opencv_csharp_native::set_invalid_argument(
                api_name,
                indices == nullptr ? "indices" : distances == nullptr ? "distances" : "output_alias");
        }
        if (knn <= 0 || knn > index->value->getItemNumber())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "knn");
        }
        if (search_k == 0 || search_k < -1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "search_k");
        }

        index->value->knnSearch(
            opencv_csharp_native::mat_value(query),
            opencv_csharp_native::mat_value(indices),
            opencv_csharp_native::mat_value(distances),
            knn,
            search_k);
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_ann_index_save(
    jyppx_ocv_features2d_ann_index* index,
    const unsigned char* filename_utf8,
    int filename_byte_length,
    int prefault)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_ann_index_save")
        int status = validate_ann_index(api_name, index);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (prefault != 0 && prefault != 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "prefault");
        }
        std::string filename;
        status = read_path(api_name, filename_utf8, filename_byte_length, filename);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(_WIN32)
        const std::filesystem::path temporary_path = make_ann_temporary_path();
        const std::string temporary_name = temporary_path.string();
        index->value->save(temporary_name, prefault != 0);
        remove_ann_temporary_path(index->temporary_path);
        index->temporary_path = temporary_path;
        index->on_disk_target_path.clear();
        return copy_ann_file(api_name, temporary_path, path_from_utf8(filename));
#else
        index->value->save(filename, prefault != 0);
        return OPENCV_CSHARP_STATUS_OK;
#endif
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_ann_index_load(
    jyppx_ocv_features2d_ann_index* index,
    const unsigned char* filename_utf8,
    int filename_byte_length,
    int prefault)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_ann_index_load")
        int status = validate_ann_index(api_name, index);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (prefault != 0 && prefault != 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "prefault");
        }
        std::string filename;
        status = read_path(api_name, filename_utf8, filename_byte_length, filename);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(_WIN32)
        const std::filesystem::path temporary_path = make_ann_temporary_path();
        status = copy_ann_file(api_name, path_from_utf8(filename), temporary_path);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        try
        {
            index->value->load(temporary_path.string(), prefault != 0);
        }
        catch (...)
        {
            std::error_code ignored;
            std::filesystem::remove(temporary_path, ignored);
            throw;
        }
        remove_ann_temporary_path(index->temporary_path);
        index->temporary_path = temporary_path;
        index->on_disk_target_path.clear();
#else
        index->value->load(filename, prefault != 0);
#endif
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_ann_index_get_tree_number(
    const jyppx_ocv_features2d_ann_index* index,
    int* tree_number)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_ann_index_get_tree_number")
        if (tree_number == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tree_number");
        }
        int status = validate_ann_index(api_name, index);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *tree_number = index->value->getTreeNumber();
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_ann_index_get_item_number(
    const jyppx_ocv_features2d_ann_index* index,
    int* item_number)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_ann_index_get_item_number")
        if (item_number == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "item_number");
        }
        int status = validate_ann_index(api_name, index);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *item_number = index->value->getItemNumber();
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_ann_index_set_on_disk_build(
    jyppx_ocv_features2d_ann_index* index,
    const unsigned char* filename_utf8,
    int filename_byte_length,
    int* enabled)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_ann_index_set_on_disk_build")
        if (enabled == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "enabled");
        }
        *enabled = 0;
        int status = validate_ann_index(api_name, index);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::string filename;
        status = read_path(api_name, filename_utf8, filename_byte_length, filename);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(_WIN32)
        const std::filesystem::path temporary_path = make_ann_temporary_path();
        *enabled = index->value->setOnDiskBuild(temporary_path.string()) ? 1 : 0;
        if (*enabled != 0)
        {
            remove_ann_temporary_path(index->temporary_path);
            index->temporary_path = temporary_path;
            index->on_disk_target_path = path_from_utf8(filename);
        }
        else
        {
            std::error_code ignored;
            std::filesystem::remove(temporary_path, ignored);
        }
#else
        *enabled = index->value->setOnDiskBuild(filename) ? 1 : 0;
#endif
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_features2d_ann_index_set_seed(
    jyppx_ocv_features2d_ann_index* index,
    int seed)
{
    OCV_CSHARP_TRY_BEGIN("jyppx_ocv_features2d_ann_index_set_seed")
        int status = validate_ann_index(api_name, index);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        index->value->setSeed(seed);
        return OPENCV_CSHARP_STATUS_OK;
    } catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

#undef OCV_CSHARP_AFFINE_CREATE_FROM_BACKEND
#undef OCV_CSHARP_FLANN_MATCH_CORE
#undef OCV_CSHARP_GFTT_SET_DOUBLE
#undef OCV_CSHARP_GFTT_GET_DOUBLE
#undef OCV_CSHARP_GFTT_SET_INT
#undef OCV_CSHARP_GFTT_GET_INT
#undef OCV_CSHARP_FAST_GET_INT
#undef OCV_CSHARP_SIFT_SET_DOUBLE
#undef OCV_CSHARP_SIFT_GET_DOUBLE
#undef OCV_CSHARP_SIFT_SET_INT
#undef OCV_CSHARP_SIFT_GET_INT
#undef OCV_CSHARP_FEATURE_DESCRIPTORS
#undef OCV_CSHARP_FEATURE_META
#undef OCV_CSHARP_CATCH
#undef OCV_CSHARP_TRY_BEGIN

#else

namespace
{
    int not_linked(const char* api_name)
    {
        return opencv_csharp_native::set_not_linked(api_name);
    }

    void set_zero(int* value)
    {
        if (value != nullptr)
        {
            *value = 0;
        }
    }

    void set_zero(double* value)
    {
        if (value != nullptr)
        {
            *value = 0.0;
        }
    }
}

#define OCV_CSHARP_STUB_BODY(api_name) return not_linked(api_name)
#define OCV_CSHARP_STUB_CREATE(handle_type, handle_name, function_name) int function_name(handle_type** handle_name) { if (handle_name != nullptr) { *handle_name = nullptr; } OCV_CSHARP_STUB_BODY(#function_name); }

int jyppx_ocv_features2d_sift_create(int, int, double, double, double, int, int, jyppx_ocv_features2d_sift** sift) { if (sift != nullptr) { *sift = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_sift_create"); }
void jyppx_ocv_features2d_sift_release(jyppx_ocv_features2d_sift* sift) { delete sift; }
int jyppx_ocv_features2d_fast_create(int, int, int, jyppx_ocv_features2d_fast** fast) { if (fast != nullptr) { *fast = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_fast_create"); }
void jyppx_ocv_features2d_fast_release(jyppx_ocv_features2d_fast* fast) { delete fast; }
int jyppx_ocv_features2d_gftt_create(int, double, double, int, int, int, double, jyppx_ocv_features2d_gftt** gftt) { if (gftt != nullptr) { *gftt = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_gftt_create"); }
void jyppx_ocv_features2d_gftt_release(jyppx_ocv_features2d_gftt* gftt) { delete gftt; }
int jyppx_ocv_features2d_mser_create(int, int, int, double, double, int, double, double, int, jyppx_ocv_features2d_mser** mser) { if (mser != nullptr) { *mser = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_mser_create"); }
void jyppx_ocv_features2d_mser_release(jyppx_ocv_features2d_mser* mser) { delete mser; }
int jyppx_ocv_features2d_simple_blob_create_default(jyppx_ocv_features2d_simple_blob** detector) { if (detector != nullptr) { *detector = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_simple_blob_create_default"); }
int jyppx_ocv_features2d_simple_blob_create(const jyppx_ocv_simple_blob_params*, jyppx_ocv_features2d_simple_blob** detector) { if (detector != nullptr) { *detector = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_simple_blob_create"); }
void jyppx_ocv_features2d_simple_blob_release(jyppx_ocv_features2d_simple_blob* detector) { delete detector; }
int jyppx_ocv_features2d_brisk_create(int, int, float, jyppx_ocv_features2d_brisk** brisk) { if (brisk != nullptr) { *brisk = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_brisk_create"); }
int jyppx_ocv_features2d_brisk_create_pattern(const float*, int, const int*, int, float, float, const int*, int, jyppx_ocv_features2d_brisk** brisk) { if (brisk != nullptr) { *brisk = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_brisk_create_pattern"); }
int jyppx_ocv_features2d_brisk_create_pattern_with_threshold(int, int, const float*, int, const int*, int, float, float, const int*, int, jyppx_ocv_features2d_brisk** brisk) { if (brisk != nullptr) { *brisk = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_brisk_create_pattern_with_threshold"); }
void jyppx_ocv_features2d_brisk_release(jyppx_ocv_features2d_brisk* brisk) { delete brisk; }
int jyppx_ocv_features2d_kaze_create(int, int, float, int, int, int, jyppx_ocv_features2d_kaze** kaze) { if (kaze != nullptr) { *kaze = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_kaze_create"); }
void jyppx_ocv_features2d_kaze_release(jyppx_ocv_features2d_kaze* kaze) { delete kaze; }
int jyppx_ocv_features2d_akaze_create(int, int, int, float, int, int, int, int, jyppx_ocv_features2d_akaze** akaze) { if (akaze != nullptr) { *akaze = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_akaze_create"); }
void jyppx_ocv_features2d_akaze_release(jyppx_ocv_features2d_akaze* akaze) { delete akaze; }
OCV_CSHARP_STUB_CREATE(jyppx_ocv_features2d_flann_matcher, matcher, jyppx_ocv_features2d_flann_matcher_create)
void jyppx_ocv_features2d_flann_matcher_release(jyppx_ocv_features2d_flann_matcher* matcher) { delete matcher; }
int jyppx_ocv_features2d_flann_matcher_clone(const jyppx_ocv_features2d_flann_matcher*, int, jyppx_ocv_features2d_descriptor_matcher** clone) { if (clone != nullptr) { *clone = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_clone"); }

#define OCV_CSHARP_STUB_INT_OUT(function_name, handle_type) int function_name(const handle_type*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY(#function_name); }
#define OCV_CSHARP_STUB_DOUBLE_OUT(function_name, handle_type) int function_name(const handle_type*, double* value) { set_zero(value); OCV_CSHARP_STUB_BODY(#function_name); }
#define OCV_CSHARP_STUB_SET_INT(function_name, handle_type) int function_name(handle_type*, int) { OCV_CSHARP_STUB_BODY(#function_name); }
#define OCV_CSHARP_STUB_SET_DOUBLE(function_name, handle_type) int function_name(handle_type*, double) { OCV_CSHARP_STUB_BODY(#function_name); }

#define OCV_CSHARP_STUB_FEATURE(prefix, handle_type) \
int prefix##_clear(handle_type*) { OCV_CSHARP_STUB_BODY(#prefix "_clear"); } \
OCV_CSHARP_STUB_INT_OUT(prefix##_empty, handle_type) \
OCV_CSHARP_STUB_INT_OUT(prefix##_descriptor_size, handle_type) \
OCV_CSHARP_STUB_INT_OUT(prefix##_descriptor_type, handle_type) \
OCV_CSHARP_STUB_INT_OUT(prefix##_default_norm, handle_type) \
OCV_CSHARP_STUB_INT_OUT(prefix##_default_name_length, handle_type) \
int prefix##_default_name_fill(const handle_type*, char*, int, int* written) { set_zero(written); OCV_CSHARP_STUB_BODY(#prefix "_default_name_fill"); } \
int prefix##_detect_count(const handle_type*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY(#prefix "_detect_count"); } \
int prefix##_detect_fill(const handle_type*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, jyppx_ocv_key_point*, int, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY(#prefix "_detect_fill"); }

#define OCV_CSHARP_STUB_DESCRIPTORS(prefix, handle_type) \
int prefix##_compute(const handle_type*, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, jyppx_ocv_key_point*, int, int* count, jyppx_ocv_mat*) { set_zero(count); OCV_CSHARP_STUB_BODY(#prefix "_compute"); } \
int prefix##_detect_and_compute_count(const handle_type*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, int, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY(#prefix "_detect_and_compute_count"); } \
int prefix##_detect_and_compute_fill(const handle_type*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, int, jyppx_ocv_key_point*, int, int* count, jyppx_ocv_mat*) { set_zero(count); OCV_CSHARP_STUB_BODY(#prefix "_detect_and_compute_fill"); }

OCV_CSHARP_STUB_FEATURE(jyppx_ocv_features2d_sift, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_DESCRIPTORS(jyppx_ocv_features2d_sift, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_sift_get_nfeatures, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_sift_set_nfeatures, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_sift_get_n_octave_layers, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_sift_set_n_octave_layers, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_sift_get_contrast_threshold, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_sift_set_contrast_threshold, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_sift_get_edge_threshold, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_sift_set_edge_threshold, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_sift_get_sigma, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_sift_set_sigma, jyppx_ocv_features2d_sift)

OCV_CSHARP_STUB_FEATURE(jyppx_ocv_features2d_fast, jyppx_ocv_features2d_fast)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_fast_get_threshold, jyppx_ocv_features2d_fast)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_fast_set_threshold, jyppx_ocv_features2d_fast)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_fast_get_nonmax_suppression, jyppx_ocv_features2d_fast)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_fast_set_nonmax_suppression, jyppx_ocv_features2d_fast)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_fast_get_type, jyppx_ocv_features2d_fast)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_fast_set_type, jyppx_ocv_features2d_fast)

OCV_CSHARP_STUB_FEATURE(jyppx_ocv_features2d_gftt, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_gftt_get_max_features, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_gftt_set_max_features, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_gftt_get_quality_level, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_gftt_set_quality_level, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_gftt_get_min_distance, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_gftt_set_min_distance, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_gftt_get_block_size, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_gftt_set_block_size, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_gftt_get_gradient_size, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_gftt_set_gradient_size, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_gftt_get_harris_detector, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_gftt_set_harris_detector, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_gftt_get_k, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_gftt_set_k, jyppx_ocv_features2d_gftt)

OCV_CSHARP_STUB_FEATURE(jyppx_ocv_features2d_mser, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_mser_get_delta, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_mser_set_delta, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_mser_get_min_area, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_mser_set_min_area, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_mser_get_max_area, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_mser_set_max_area, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_mser_get_max_variation, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_mser_set_max_variation, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_mser_get_min_diversity, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_mser_set_min_diversity, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_mser_get_max_evolution, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_mser_set_max_evolution, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_mser_get_area_threshold, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_mser_set_area_threshold, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_mser_get_min_margin, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_mser_set_min_margin, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_mser_get_edge_blur_size, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_mser_set_edge_blur_size, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_mser_get_pass2_only, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_mser_set_pass2_only, jyppx_ocv_features2d_mser)
int jyppx_ocv_features2d_mser_detect_regions_count(const jyppx_ocv_features2d_mser*, const jyppx_ocv_mat*, int* regions, int* total) { set_zero(regions); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_mser_detect_regions_count"); }
int jyppx_ocv_features2d_mser_detect_regions_fill(const jyppx_ocv_features2d_mser*, const jyppx_ocv_mat*, int*, int, jyppx_ocv_point*, int, jyppx_ocv_rect*, int, int* regions, int* total) { set_zero(regions); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_mser_detect_regions_fill"); }
OCV_CSHARP_STUB_FEATURE(jyppx_ocv_features2d_simple_blob, jyppx_ocv_features2d_simple_blob)
int jyppx_ocv_features2d_simple_blob_get_params(const jyppx_ocv_features2d_simple_blob*, jyppx_ocv_simple_blob_params*) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_simple_blob_get_params"); }
int jyppx_ocv_features2d_simple_blob_set_params(jyppx_ocv_features2d_simple_blob*, const jyppx_ocv_simple_blob_params*) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_simple_blob_set_params"); }
int jyppx_ocv_features2d_simple_blob_get_blob_contours_count(const jyppx_ocv_features2d_simple_blob*, int* contours, int* total) { set_zero(contours); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_simple_blob_get_blob_contours_count"); }
int jyppx_ocv_features2d_simple_blob_get_blob_contours_fill(const jyppx_ocv_features2d_simple_blob*, int*, int, jyppx_ocv_point*, int, int* contours, int* total) { set_zero(contours); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_simple_blob_get_blob_contours_fill"); }

OCV_CSHARP_STUB_FEATURE(jyppx_ocv_features2d_brisk, jyppx_ocv_features2d_brisk)
OCV_CSHARP_STUB_DESCRIPTORS(jyppx_ocv_features2d_brisk, jyppx_ocv_features2d_brisk)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_brisk_get_threshold, jyppx_ocv_features2d_brisk)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_brisk_set_threshold, jyppx_ocv_features2d_brisk)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_brisk_get_octaves, jyppx_ocv_features2d_brisk)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_brisk_set_octaves, jyppx_ocv_features2d_brisk)
int jyppx_ocv_features2d_brisk_get_pattern_scale(const jyppx_ocv_features2d_brisk*, float* value) { if (value != nullptr) { *value = 0.0F; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_brisk_get_pattern_scale"); }
int jyppx_ocv_features2d_brisk_set_pattern_scale(jyppx_ocv_features2d_brisk*, float) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_brisk_set_pattern_scale"); }

OCV_CSHARP_STUB_FEATURE(jyppx_ocv_features2d_kaze, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_DESCRIPTORS(jyppx_ocv_features2d_kaze, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_kaze_get_extended, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_kaze_set_extended, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_kaze_get_upright, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_kaze_set_upright, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_kaze_get_threshold, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_kaze_set_threshold, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_kaze_get_n_octaves, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_kaze_set_n_octaves, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_kaze_get_n_octave_layers, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_kaze_set_n_octave_layers, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_kaze_get_diffusivity, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_kaze_set_diffusivity, jyppx_ocv_features2d_kaze)

OCV_CSHARP_STUB_FEATURE(jyppx_ocv_features2d_akaze, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_DESCRIPTORS(jyppx_ocv_features2d_akaze, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_descriptor_type, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_descriptor_type, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_descriptor_size, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_descriptor_size, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_descriptor_channels, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_descriptor_channels, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_DOUBLE_OUT(jyppx_ocv_features2d_akaze_get_threshold, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_SET_DOUBLE(jyppx_ocv_features2d_akaze_set_threshold, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_n_octaves, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_n_octaves, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_n_octave_layers, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_n_octave_layers, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_diffusivity, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_diffusivity, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_akaze_get_max_points, jyppx_ocv_features2d_akaze)
OCV_CSHARP_STUB_SET_INT(jyppx_ocv_features2d_akaze_set_max_points, jyppx_ocv_features2d_akaze)

#define OCV_CSHARP_STUB_AFFINE_CREATE(function_name, backend_type) \
int function_name(const backend_type*, int, int, float, float, jyppx_ocv_features2d_affine** affine) { if (affine != nullptr) { *affine = nullptr; } OCV_CSHARP_STUB_BODY(#function_name); }

OCV_CSHARP_STUB_AFFINE_CREATE(jyppx_ocv_features2d_affine_create_from_orb, jyppx_ocv_features2d_orb)
OCV_CSHARP_STUB_AFFINE_CREATE(jyppx_ocv_features2d_affine_create_from_sift, jyppx_ocv_features2d_sift)
OCV_CSHARP_STUB_AFFINE_CREATE(jyppx_ocv_features2d_affine_create_from_fast, jyppx_ocv_features2d_fast)
OCV_CSHARP_STUB_AFFINE_CREATE(jyppx_ocv_features2d_affine_create_from_gftt, jyppx_ocv_features2d_gftt)
OCV_CSHARP_STUB_AFFINE_CREATE(jyppx_ocv_features2d_affine_create_from_mser, jyppx_ocv_features2d_mser)
OCV_CSHARP_STUB_AFFINE_CREATE(jyppx_ocv_features2d_affine_create_from_simple_blob, jyppx_ocv_features2d_simple_blob)
OCV_CSHARP_STUB_AFFINE_CREATE(jyppx_ocv_features2d_affine_create_from_brisk, jyppx_ocv_features2d_brisk)
OCV_CSHARP_STUB_AFFINE_CREATE(jyppx_ocv_features2d_affine_create_from_kaze, jyppx_ocv_features2d_kaze)
OCV_CSHARP_STUB_AFFINE_CREATE(jyppx_ocv_features2d_affine_create_from_akaze, jyppx_ocv_features2d_akaze)
void jyppx_ocv_features2d_affine_release(jyppx_ocv_features2d_affine* affine) { delete affine; }
OCV_CSHARP_STUB_FEATURE(jyppx_ocv_features2d_affine, jyppx_ocv_features2d_affine)
int jyppx_ocv_features2d_affine_set_view_params(jyppx_ocv_features2d_affine*, const float*, int, const float*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_affine_set_view_params"); }
int jyppx_ocv_features2d_affine_get_view_params_count(const jyppx_ocv_features2d_affine*, int* tilts, int* rolls) { set_zero(tilts); set_zero(rolls); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_affine_get_view_params_count"); }
int jyppx_ocv_features2d_affine_get_view_params_fill(const jyppx_ocv_features2d_affine*, float*, int, float*, int, int* tilts, int* rolls) { set_zero(tilts); set_zero(rolls); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_affine_get_view_params_fill"); }

int jyppx_ocv_features2d_ann_index_create(int, int, jyppx_ocv_features2d_ann_index** index) { if (index != nullptr) { *index = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_ann_index_create"); }
void jyppx_ocv_features2d_ann_index_release(jyppx_ocv_features2d_ann_index* index) { delete index; }
int jyppx_ocv_features2d_ann_index_add_items(jyppx_ocv_features2d_ann_index*, const jyppx_ocv_mat*) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_ann_index_add_items"); }
int jyppx_ocv_features2d_ann_index_build(jyppx_ocv_features2d_ann_index*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_ann_index_build"); }
int jyppx_ocv_features2d_ann_index_knn_search(const jyppx_ocv_features2d_ann_index*, const jyppx_ocv_mat*, jyppx_ocv_mat*, jyppx_ocv_mat*, int, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_ann_index_knn_search"); }
int jyppx_ocv_features2d_ann_index_save(jyppx_ocv_features2d_ann_index*, const unsigned char*, int, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_ann_index_save"); }
int jyppx_ocv_features2d_ann_index_load(jyppx_ocv_features2d_ann_index*, const unsigned char*, int, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_ann_index_load"); }
int jyppx_ocv_features2d_ann_index_get_tree_number(const jyppx_ocv_features2d_ann_index*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_ann_index_get_tree_number"); }
int jyppx_ocv_features2d_ann_index_get_item_number(const jyppx_ocv_features2d_ann_index*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_ann_index_get_item_number"); }
int jyppx_ocv_features2d_ann_index_set_on_disk_build(jyppx_ocv_features2d_ann_index*, const unsigned char*, int, int* enabled) { set_zero(enabled); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_ann_index_set_on_disk_build"); }
int jyppx_ocv_features2d_ann_index_set_seed(jyppx_ocv_features2d_ann_index*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_ann_index_set_seed"); }

OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_flann_matcher_is_mask_supported, jyppx_ocv_features2d_flann_matcher)
OCV_CSHARP_STUB_INT_OUT(jyppx_ocv_features2d_flann_matcher_empty, jyppx_ocv_features2d_flann_matcher)
int jyppx_ocv_features2d_flann_matcher_clear(jyppx_ocv_features2d_flann_matcher*) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_clear"); }
int jyppx_ocv_features2d_flann_matcher_train(jyppx_ocv_features2d_flann_matcher*) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_train"); }
int jyppx_ocv_features2d_flann_matcher_add(jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat* const*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_add"); }
int jyppx_ocv_features2d_flann_matcher_get_train_descriptors_count(const jyppx_ocv_features2d_flann_matcher*, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_get_train_descriptors_count"); }
int jyppx_ocv_features2d_flann_matcher_get_train_descriptor_clone(const jyppx_ocv_features2d_flann_matcher*, int, jyppx_ocv_mat** descriptor) { if (descriptor != nullptr) { *descriptor = nullptr; } OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_get_train_descriptor_clone"); }
#define OCV_CSHARP_STUB_FLANN_MATCH(prefix) \
int prefix##_count(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY(#prefix "_count"); } \
int prefix##_fill(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, jyppx_ocv_dmatch*, int, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY(#prefix "_fill"); }
OCV_CSHARP_STUB_FLANN_MATCH(jyppx_ocv_features2d_flann_matcher_match)
int jyppx_ocv_features2d_flann_matcher_match_train_count(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_match_train_count"); }
int jyppx_ocv_features2d_flann_matcher_match_train_fill(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, jyppx_ocv_dmatch*, int, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_match_train_fill"); }
int jyppx_ocv_features2d_flann_matcher_knn_match_count(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, int, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_knn_match_count"); }
int jyppx_ocv_features2d_flann_matcher_knn_match_fill(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, int, int, int*, int, jyppx_ocv_dmatch*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_knn_match_fill"); }
int jyppx_ocv_features2d_flann_matcher_knn_match_train_count(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, int, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_knn_match_train_count"); }
int jyppx_ocv_features2d_flann_matcher_knn_match_train_fill(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, int, int, int*, int, jyppx_ocv_dmatch*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_knn_match_train_fill"); }
int jyppx_ocv_features2d_flann_matcher_radius_match_count(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, float, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_radius_match_count"); }
int jyppx_ocv_features2d_flann_matcher_radius_match_fill(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, float, int, int*, int, jyppx_ocv_dmatch*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_radius_match_fill"); }
int jyppx_ocv_features2d_flann_matcher_radius_match_train_count(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, float, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_radius_match_train_count"); }
int jyppx_ocv_features2d_flann_matcher_radius_match_train_fill(const jyppx_ocv_features2d_flann_matcher*, const jyppx_ocv_mat*, float, int, int*, int, jyppx_ocv_dmatch*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_flann_matcher_radius_match_train_fill"); }

#undef OCV_CSHARP_STUB_FLANN_MATCH
#undef OCV_CSHARP_STUB_AFFINE_CREATE
#undef OCV_CSHARP_STUB_FEATURE
#undef OCV_CSHARP_STUB_DESCRIPTORS
#undef OCV_CSHARP_STUB_INT_OUT
#undef OCV_CSHARP_STUB_DOUBLE_OUT
#undef OCV_CSHARP_STUB_SET_INT
#undef OCV_CSHARP_STUB_SET_DOUBLE
#undef OCV_CSHARP_STUB_CREATE
#undef OCV_CSHARP_STUB_BODY

#endif

