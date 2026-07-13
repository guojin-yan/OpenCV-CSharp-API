#include "open_cv_sharp/features2d/features2d.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "feature_handles.h"

#include <cstring>
#include <new>
#include <string>
#include <vector>

#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
#include <opencv2/features.hpp>
#endif

namespace
{
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* parameter_name)
    {
        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_mat(const char* api_name, jyppx_ocv_mat* mat, const char* parameter_name)
    {
        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_orb(const char* api_name, const jyppx_ocv_features2d_orb* orb)
    {
        if (orb == nullptr || orb->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "orb");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_bf_matcher(const char* api_name, const jyppx_ocv_features2d_bf_matcher* matcher)
    {
        if (matcher == nullptr || matcher->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_descriptor_matcher(const char* api_name, const jyppx_ocv_features2d_descriptor_matcher* matcher)
    {
        if (matcher == nullptr || matcher->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
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

    int validate_keypoint_input(const char* api_name, const jyppx_ocv_key_point* keypoints, int count, const char* parameter_name)
    {
        int status = validate_non_negative_count(api_name, count, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (count > 0 && keypoints == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_dmatch_input(const char* api_name, const jyppx_ocv_dmatch* matches, int count, const char* parameter_name)
    {
        int status = validate_non_negative_count(api_name, count, parameter_name);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (count > 0 && matches == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::InputArray optional_input_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::InputArray optional_masks_input_array(const std::vector<cv::Mat>& masks)
    {
        return masks.empty() ? cv::noArray() : cv::InputArray(masks);
    }

    cv::Scalar scalar_from_values(double v0, double v1, double v2, double v3)
    {
        return cv::Scalar(v0, v1, v2, v3);
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

    std::vector<cv::Mat> to_mat_vector(
        const char* api_name,
        const jyppx_ocv_mat* const* mats,
        int mat_count,
        const char* parameter_name,
        int& status)
    {
        status = validate_non_negative_count(api_name, mat_count, parameter_name);
        std::vector<cv::Mat> result;
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return result;
        }

        if (mat_count > 0 && mats == nullptr)
        {
            status = opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
            return result;
        }

        result.reserve(static_cast<size_t>(mat_count));
        for (int i = 0; i < mat_count; ++i)
        {
            status = validate_mat(api_name, mats[i], parameter_name);
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                return result;
            }

            result.push_back(opencv_csharp_native::mat_value(mats[i]));
        }

        status = OPENCV_CSHARP_STATUS_OK;
        return result;
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

    std::vector<std::vector<cv::DMatch>> to_cv_dmatch_groups(const int* offsets, int offset_count, const jyppx_ocv_dmatch* matches, int match_count)
    {
        std::vector<std::vector<cv::DMatch>> result;
        int group_count = offset_count - 1;
        result.reserve(static_cast<size_t>(group_count));
        for (int i = 0; i < group_count; ++i)
        {
            int start = offsets[i];
            int end = offsets[i + 1];
            std::vector<cv::DMatch> group;
            group.reserve(static_cast<size_t>(end - start));
            for (int j = start; j < end && j < match_count; ++j)
            {
                group.push_back(to_cv_dmatch(matches[j]));
            }

            result.push_back(group);
        }

        return result;
    }

    cv::DrawMatchesFlags draw_flags_from_int(int flags)
    {
        return static_cast<cv::DrawMatchesFlags>(flags);
    }
#endif
}

#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)

int jyppx_ocv_features2d_orb_create(
    int max_features,
    float scale_factor,
    int nlevels,
    int edge_threshold,
    int first_level,
    int wta_k,
    int score_type,
    int patch_size,
    int fast_threshold,
    jyppx_ocv_features2d_orb** orb)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (orb == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "orb");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_orb{
            cv::ORB::create(max_features, scale_factor, nlevels, edge_threshold, first_level, wta_k, static_cast<cv::ORB::ScoreType>(score_type), patch_size, fast_threshold)
        };
        if (result == nullptr)
        {
            *orb = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *orb = result;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        if (orb != nullptr)
        {
            *orb = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_features2d_orb_release(jyppx_ocv_features2d_orb* orb)
{
    try
    {
        delete orb;
    }
    catch (...)
    {
    }
}

#define OCV_CSHARP_ORB_GET_INT(function_name, getter_name) \
int function_name(const jyppx_ocv_features2d_orb* orb, int* value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        if (value == nullptr) \
        { \
            return opencv_csharp_native::set_invalid_argument(api_name, "value"); \
        } \
        int status = validate_orb(api_name, orb); \
        if (status != OPENCV_CSHARP_STATUS_OK) \
        { \
            return status; \
        } \
        *value = orb->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}

#define OCV_CSHARP_ORB_SET_INT(function_name, setter_name) \
int function_name(jyppx_ocv_features2d_orb* orb, int value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_orb(api_name, orb); \
        if (status != OPENCV_CSHARP_STATUS_OK) \
        { \
            return status; \
        } \
        orb->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}

OCV_CSHARP_ORB_GET_INT(jyppx_ocv_features2d_orb_get_max_features, getMaxFeatures)
OCV_CSHARP_ORB_SET_INT(jyppx_ocv_features2d_orb_set_max_features, setMaxFeatures)
OCV_CSHARP_ORB_GET_INT(jyppx_ocv_features2d_orb_get_nlevels, getNLevels)
OCV_CSHARP_ORB_SET_INT(jyppx_ocv_features2d_orb_set_nlevels, setNLevels)
OCV_CSHARP_ORB_GET_INT(jyppx_ocv_features2d_orb_get_edge_threshold, getEdgeThreshold)
OCV_CSHARP_ORB_SET_INT(jyppx_ocv_features2d_orb_set_edge_threshold, setEdgeThreshold)
OCV_CSHARP_ORB_GET_INT(jyppx_ocv_features2d_orb_get_first_level, getFirstLevel)
OCV_CSHARP_ORB_SET_INT(jyppx_ocv_features2d_orb_set_first_level, setFirstLevel)
OCV_CSHARP_ORB_GET_INT(jyppx_ocv_features2d_orb_get_wta_k, getWTA_K)
OCV_CSHARP_ORB_SET_INT(jyppx_ocv_features2d_orb_set_wta_k, setWTA_K)
OCV_CSHARP_ORB_GET_INT(jyppx_ocv_features2d_orb_get_score_type, getScoreType)
OCV_CSHARP_ORB_GET_INT(jyppx_ocv_features2d_orb_get_patch_size, getPatchSize)
OCV_CSHARP_ORB_SET_INT(jyppx_ocv_features2d_orb_set_patch_size, setPatchSize)
OCV_CSHARP_ORB_GET_INT(jyppx_ocv_features2d_orb_get_fast_threshold, getFastThreshold)
OCV_CSHARP_ORB_SET_INT(jyppx_ocv_features2d_orb_set_fast_threshold, setFastThreshold)

#undef OCV_CSHARP_ORB_GET_INT
#undef OCV_CSHARP_ORB_SET_INT

int jyppx_ocv_features2d_orb_set_score_type(jyppx_ocv_features2d_orb* orb, int value)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_set_score_type";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        orb->value->setScoreType(static_cast<cv::ORB::ScoreType>(value));
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_get_scale_factor(const jyppx_ocv_features2d_orb* orb, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_get_scale_factor";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = orb->value->getScaleFactor();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_set_scale_factor(jyppx_ocv_features2d_orb* orb, double value)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_set_scale_factor";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        orb->value->setScaleFactor(value);
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_clear(jyppx_ocv_features2d_orb* orb)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_clear";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        orb->value->clear();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_empty(const jyppx_ocv_features2d_orb* orb, int* empty)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (empty == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "empty");
        }

        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *empty = orb->value->empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_descriptor_size(const jyppx_ocv_features2d_orb* orb, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_descriptor_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = orb->value->descriptorSize();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_descriptor_type(const jyppx_ocv_features2d_orb* orb, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_descriptor_type";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = orb->value->descriptorType();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_default_norm(const jyppx_ocv_features2d_orb* orb, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_default_norm";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = orb->value->defaultNorm();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_default_name_length(const jyppx_ocv_features2d_orb* orb, int* length)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_default_name_length";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (length == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "length");
        }

        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::String name = orb->value->getDefaultName();
        *length = static_cast<int>(name.size());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_default_name_fill(const jyppx_ocv_features2d_orb* orb, char* buffer, int buffer_capacity, int* written)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_default_name_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (written == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "written");
        }

        int status = validate_non_negative_count(api_name, buffer_capacity, "buffer_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::String name = orb->value->getDefaultName();
        *written = static_cast<int>(name.size());
        if (name.empty())
        {
            return OPENCV_CSHARP_STATUS_OK;
        }

        if (buffer == nullptr || buffer_capacity < *written)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        std::memcpy(buffer, name.c_str(), static_cast<size_t>(*written));
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_detect_count(
    const jyppx_ocv_features2d_orb* orb,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    int* keypoint_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_detect_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (keypoint_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "keypoint_count");
        }

        int status = validate_orb(api_name, orb);
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
        const_cast<jyppx_ocv_features2d_orb*>(orb)->value->detect(opencv_csharp_native::mat_value(image), keypoints, optional_input_array(mask));
        *keypoint_count = static_cast<int>(keypoints.size());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_detect_fill(
    const jyppx_ocv_features2d_orb* orb,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_key_point* keypoints,
    int keypoint_capacity,
    int* keypoint_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_detect_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_non_negative_count(api_name, keypoint_capacity, "keypoint_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::KeyPoint> detected;
        const_cast<jyppx_ocv_features2d_orb*>(orb)->value->detect(opencv_csharp_native::mat_value(image), detected, optional_input_array(mask));
        return copy_keypoints_to_output(api_name, detected, keypoints, keypoint_capacity, keypoint_count);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_compute(
    const jyppx_ocv_features2d_orb* orb,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_key_point* keypoints_in,
    int keypoint_count,
    jyppx_ocv_key_point* keypoints_out,
    int keypoint_capacity,
    int* written_keypoint_count,
    jyppx_ocv_mat* descriptors)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_keypoint_input(api_name, keypoints_in, keypoint_count, "keypoints_in");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_non_negative_count(api_name, keypoint_capacity, "keypoint_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, descriptors, "descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::KeyPoint> keypoints = to_cv_keypoints(keypoints_in, keypoint_count);
        const_cast<jyppx_ocv_features2d_orb*>(orb)->value->compute(opencv_csharp_native::mat_value(image), keypoints, opencv_csharp_native::mat_value(descriptors));
        return copy_keypoints_to_output(api_name, keypoints, keypoints_out, keypoint_capacity, written_keypoint_count);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_detect_and_compute_count(
    const jyppx_ocv_features2d_orb* orb,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_key_point* keypoints_in,
    int keypoint_count,
    int use_provided_keypoints,
    int* output_keypoint_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_detect_and_compute_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (output_keypoint_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output_keypoint_count");
        }

        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (use_provided_keypoints != 0)
        {
            status = validate_keypoint_input(api_name, keypoints_in, keypoint_count, "keypoints_in");
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                return status;
            }

            *output_keypoint_count = keypoint_count;
            return OPENCV_CSHARP_STATUS_OK;
        }

        std::vector<cv::KeyPoint> keypoints;
        const_cast<jyppx_ocv_features2d_orb*>(orb)->value->detect(opencv_csharp_native::mat_value(image), keypoints, optional_input_array(mask));
        *output_keypoint_count = static_cast<int>(keypoints.size());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_orb_detect_and_compute_fill(
    const jyppx_ocv_features2d_orb* orb,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_key_point* keypoints_in,
    int keypoint_count,
    int use_provided_keypoints,
    jyppx_ocv_key_point* keypoints_out,
    int keypoint_capacity,
    int* output_keypoint_count,
    jyppx_ocv_mat* descriptors)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_orb_detect_and_compute_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_orb(api_name, orb);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, descriptors, "descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_non_negative_count(api_name, keypoint_capacity, "keypoint_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::KeyPoint> keypoints;
        if (use_provided_keypoints != 0)
        {
            status = validate_keypoint_input(api_name, keypoints_in, keypoint_count, "keypoints_in");
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                return status;
            }

            keypoints = to_cv_keypoints(keypoints_in, keypoint_count);
        }

        const_cast<jyppx_ocv_features2d_orb*>(orb)->value->detectAndCompute(
            opencv_csharp_native::mat_value(image),
            optional_input_array(mask),
            keypoints,
            opencv_csharp_native::mat_value(descriptors),
            use_provided_keypoints != 0);
        return copy_keypoints_to_output(api_name, keypoints, keypoints_out, keypoint_capacity, output_keypoint_count);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_create(
    int norm_type,
    int cross_check,
    jyppx_ocv_features2d_bf_matcher** matcher)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }

        auto result = new (std::nothrow) jyppx_ocv_features2d_bf_matcher{
            cv::BFMatcher::create(norm_type, cross_check != 0),
            norm_type,
            cross_check != 0
        };
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

void jyppx_ocv_features2d_bf_matcher_release(jyppx_ocv_features2d_bf_matcher* matcher)
{
    try
    {
        delete matcher;
    }
    catch (...)
    {
    }
}

int jyppx_ocv_features2d_bf_matcher_get_norm_type(const jyppx_ocv_features2d_bf_matcher* matcher, int* norm_type)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_get_norm_type";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (norm_type == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "norm_type");
        }

        int status = validate_bf_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *norm_type = matcher->norm_type;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_get_cross_check(const jyppx_ocv_features2d_bf_matcher* matcher, int* cross_check)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_get_cross_check";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (cross_check == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "cross_check");
        }

        int status = validate_bf_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *cross_check = matcher->cross_check ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_is_mask_supported(const jyppx_ocv_features2d_bf_matcher* matcher, int* supported)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_is_mask_supported";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (supported == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "supported");
        }

        int status = validate_bf_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *supported = matcher->value->isMaskSupported() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_empty(const jyppx_ocv_features2d_bf_matcher* matcher, int* empty)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (empty == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "empty");
        }

        int status = validate_bf_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *empty = matcher->value->empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_clear(jyppx_ocv_features2d_bf_matcher* matcher)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_clear";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_bf_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        matcher->value->clear();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_train(jyppx_ocv_features2d_bf_matcher* matcher)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_train";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_bf_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        matcher->value->train();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_add(
    jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* const* descriptors,
    int descriptor_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_add";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_bf_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_non_negative_count(api_name, descriptor_count, "descriptor_count");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (descriptor_count > 0 && descriptors == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "descriptors");
        }

        std::vector<cv::Mat> mats;
        mats.reserve(static_cast<size_t>(descriptor_count));
        for (int i = 0; i < descriptor_count; ++i)
        {
            status = validate_mat(api_name, descriptors[i], "descriptors");
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                return status;
            }

            mats.push_back(opencv_csharp_native::mat_value(descriptors[i]));
        }

        matcher->value->add(mats);
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_create_by_type(
    int matcher_type,
    jyppx_ocv_features2d_descriptor_matcher** matcher)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_create_by_type";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }

        *matcher = nullptr;
        cv::Ptr<cv::DescriptorMatcher> native = cv::DescriptorMatcher::create(static_cast<cv::DescriptorMatcher::MatcherType>(matcher_type));
        return wrap_descriptor_matcher(api_name, native, matcher);
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

int jyppx_ocv_features2d_descriptor_matcher_create_by_name(
    const char* matcher_name,
    int matcher_name_length,
    jyppx_ocv_features2d_descriptor_matcher** matcher)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_create_by_name";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }

        *matcher = nullptr;
        int status = validate_non_negative_count(api_name, matcher_name_length, "matcher_name_length");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (matcher_name_length > 0 && matcher_name == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher_name");
        }

        std::string name(matcher_name == nullptr ? "" : matcher_name, static_cast<size_t>(matcher_name_length));
        cv::Ptr<cv::DescriptorMatcher> native = cv::DescriptorMatcher::create(name);
        return wrap_descriptor_matcher(api_name, native, matcher);
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

void jyppx_ocv_features2d_descriptor_matcher_release(jyppx_ocv_features2d_descriptor_matcher* matcher)
{
    try
    {
        delete matcher;
    }
    catch (...)
    {
    }
}

int jyppx_ocv_features2d_descriptor_matcher_clone(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    int empty_train_data,
    jyppx_ocv_features2d_descriptor_matcher** clone)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_clone";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (clone == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "clone");
        }

        *clone = nullptr;
        int status = validate_descriptor_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return wrap_descriptor_matcher(api_name, matcher->value->clone(empty_train_data != 0), clone);
    }
    catch (...)
    {
        if (clone != nullptr)
        {
            *clone = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_is_mask_supported(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    int* supported)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_is_mask_supported";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (supported == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "supported");
        }

        int status = validate_descriptor_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *supported = matcher->value->isMaskSupported() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_empty(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    int* empty)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (empty == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "empty");
        }

        int status = validate_descriptor_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *empty = matcher->value->empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_clear(jyppx_ocv_features2d_descriptor_matcher* matcher)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_clear";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_descriptor_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        matcher->value->clear();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_train(jyppx_ocv_features2d_descriptor_matcher* matcher)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_train";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_descriptor_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        matcher->value->train();
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_add(
    jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* const* descriptors,
    int descriptor_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_add";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_descriptor_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::Mat> mats = to_mat_vector(api_name, descriptors, descriptor_count, "descriptors", status);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        matcher->value->add(mats);
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_get_train_descriptors_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    int* descriptor_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_get_train_descriptors_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (descriptor_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "descriptor_count");
        }

        int status = validate_descriptor_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *descriptor_count = static_cast<int>(matcher->value->getTrainDescriptors().size());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_get_train_descriptor_clone(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    int index,
    jyppx_ocv_mat** descriptor)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_get_train_descriptor_clone";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (descriptor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "descriptor");
        }

        *descriptor = nullptr;
        int status = validate_descriptor_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        const std::vector<cv::Mat>& descriptors = matcher->value->getTrainDescriptors();
        if (index < 0 || index >= static_cast<int>(descriptors.size()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }

        return clone_mat_to_handle(api_name, descriptors[static_cast<size_t>(index)], descriptor);
    }
    catch (...)
    {
        if (descriptor != nullptr)
        {
            *descriptor = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int descriptor_match_core(
    const char* api_name,
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    std::vector<cv::DMatch>& matches)
{
    int status = validate_descriptor_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, train_descriptors, "train_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    const_cast<jyppx_ocv_features2d_descriptor_matcher*>(matcher)->value->match(
        opencv_csharp_native::mat_value(query_descriptors),
        opencv_csharp_native::mat_value(train_descriptors),
        matches,
        optional_input_array(mask));
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_descriptor_matcher_match_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_match_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<cv::DMatch> matches;
        int status = descriptor_match_core(api_name, matcher, query_descriptors, train_descriptors, mask, matches);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (match_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "match_count");
        }

        *match_count = static_cast<int>(matches.size());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_match_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_match_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_non_negative_count(api_name, match_capacity, "match_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::DMatch> native_matches;
        status = descriptor_match_core(api_name, matcher, query_descriptors, train_descriptors, mask, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK
            ? copy_matches_to_output(api_name, native_matches, matches, match_capacity, match_count)
            : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int descriptor_match_train_core(
    const char* api_name,
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    std::vector<cv::DMatch>& matches)
{
    int status = validate_descriptor_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    std::vector<cv::Mat> native_masks = to_mat_vector(api_name, masks, mask_count, "masks", status);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    const_cast<jyppx_ocv_features2d_descriptor_matcher*>(matcher)->value->match(
        opencv_csharp_native::mat_value(query_descriptors),
        matches,
        optional_masks_input_array(native_masks));
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_descriptor_matcher_match_train_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_match_train_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<cv::DMatch> matches;
        int status = descriptor_match_train_core(api_name, matcher, query_descriptors, masks, mask_count, matches);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (match_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "match_count");
        }

        *match_count = static_cast<int>(matches.size());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_match_train_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_match_train_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_non_negative_count(api_name, match_capacity, "match_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::DMatch> native_matches;
        status = descriptor_match_train_core(api_name, matcher, query_descriptors, masks, mask_count, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK
            ? copy_matches_to_output(api_name, native_matches, matches, match_capacity, match_count)
            : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int descriptor_knn_match_core(
    const char* api_name,
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    std::vector<std::vector<cv::DMatch>>& matches)
{
    int status = validate_descriptor_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, train_descriptors, "train_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    if (k <= 0)
    {
        return opencv_csharp_native::set_invalid_argument(api_name, "k");
    }

    const_cast<jyppx_ocv_features2d_descriptor_matcher*>(matcher)->value->knnMatch(
        opencv_csharp_native::mat_value(query_descriptors),
        opencv_csharp_native::mat_value(train_descriptors),
        matches,
        k,
        optional_input_array(mask),
        compact_result != 0);
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_descriptor_matcher_knn_match_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_knn_match_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> matches;
        int status = descriptor_knn_match_core(api_name, matcher, query_descriptors, train_descriptors, k, mask, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_knn_match_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
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
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_knn_match_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> native_matches;
        int status = descriptor_knn_match_core(api_name, matcher, query_descriptors, train_descriptors, k, mask, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int descriptor_knn_train_core(
    const char* api_name,
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    std::vector<std::vector<cv::DMatch>>& matches)
{
    int status = validate_descriptor_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    if (k <= 0)
    {
        return opencv_csharp_native::set_invalid_argument(api_name, "k");
    }

    std::vector<cv::Mat> native_masks = to_mat_vector(api_name, masks, mask_count, "masks", status);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    const_cast<jyppx_ocv_features2d_descriptor_matcher*>(matcher)->value->knnMatch(
        opencv_csharp_native::mat_value(query_descriptors),
        matches,
        k,
        optional_masks_input_array(native_masks),
        compact_result != 0);
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_descriptor_matcher_knn_match_train_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_knn_match_train_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> matches;
        int status = descriptor_knn_train_core(api_name, matcher, query_descriptors, k, masks, mask_count, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_knn_match_train_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_knn_match_train_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> native_matches;
        int status = descriptor_knn_train_core(api_name, matcher, query_descriptors, k, masks, mask_count, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int descriptor_radius_match_core(
    const char* api_name,
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    float max_distance,
    const jyppx_ocv_mat* mask,
    int compact_result,
    std::vector<std::vector<cv::DMatch>>& matches)
{
    int status = validate_descriptor_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, train_descriptors, "train_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    const_cast<jyppx_ocv_features2d_descriptor_matcher*>(matcher)->value->radiusMatch(
        opencv_csharp_native::mat_value(query_descriptors),
        opencv_csharp_native::mat_value(train_descriptors),
        matches,
        max_distance,
        optional_input_array(mask),
        compact_result != 0);
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_descriptor_matcher_radius_match_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    float max_distance,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_radius_match_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> matches;
        int status = descriptor_radius_match_core(api_name, matcher, query_descriptors, train_descriptors, max_distance, mask, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_radius_match_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    float max_distance,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_radius_match_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> native_matches;
        int status = descriptor_radius_match_core(api_name, matcher, query_descriptors, train_descriptors, max_distance, mask, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int descriptor_radius_train_core(
    const char* api_name,
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    std::vector<std::vector<cv::DMatch>>& matches)
{
    int status = validate_descriptor_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    std::vector<cv::Mat> native_masks = to_mat_vector(api_name, masks, mask_count, "masks", status);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    const_cast<jyppx_ocv_features2d_descriptor_matcher*>(matcher)->value->radiusMatch(
        opencv_csharp_native::mat_value(query_descriptors),
        matches,
        max_distance,
        optional_masks_input_array(native_masks),
        compact_result != 0);
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_descriptor_matcher_radius_match_train_count(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_radius_match_train_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> matches;
        int status = descriptor_radius_train_core(api_name, matcher, query_descriptors, max_distance, masks, mask_count, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_descriptor_matcher_radius_match_train_fill(
    const jyppx_ocv_features2d_descriptor_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_descriptor_matcher_radius_match_train_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> native_matches;
        int status = descriptor_radius_train_core(api_name, matcher, query_descriptors, max_distance, masks, mask_count, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_clone(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    int empty_train_data,
    jyppx_ocv_features2d_descriptor_matcher** clone)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_clone";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (clone == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "clone");
        }

        *clone = nullptr;
        int status = validate_bf_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return wrap_descriptor_matcher(api_name, matcher->value->clone(empty_train_data != 0), clone);
    }
    catch (...)
    {
        if (clone != nullptr)
        {
            *clone = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_get_train_descriptors_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    int* descriptor_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_get_train_descriptors_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (descriptor_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "descriptor_count");
        }

        int status = validate_bf_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *descriptor_count = static_cast<int>(matcher->value->getTrainDescriptors().size());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_get_train_descriptor_clone(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    int index,
    jyppx_ocv_mat** descriptor)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_get_train_descriptor_clone";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (descriptor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "descriptor");
        }

        *descriptor = nullptr;
        int status = validate_bf_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        const std::vector<cv::Mat>& descriptors = matcher->value->getTrainDescriptors();
        if (index < 0 || index >= static_cast<int>(descriptors.size()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index");
        }

        return clone_mat_to_handle(api_name, descriptors[static_cast<size_t>(index)], descriptor);
    }
    catch (...)
    {
        if (descriptor != nullptr)
        {
            *descriptor = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int bf_match_core(
    const char* api_name,
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    std::vector<cv::DMatch>& matches)
{
    int status = validate_bf_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, train_descriptors, "train_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    const_cast<jyppx_ocv_features2d_bf_matcher*>(matcher)->value->match(
        opencv_csharp_native::mat_value(query_descriptors),
        opencv_csharp_native::mat_value(train_descriptors),
        matches,
        optional_input_array(mask));
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_bf_matcher_match_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_match_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<cv::DMatch> matches;
        int status = bf_match_core(api_name, matcher, query_descriptors, train_descriptors, mask, matches);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (match_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "match_count");
        }

        *match_count = static_cast<int>(matches.size());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_match_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_match_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_non_negative_count(api_name, match_capacity, "match_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::DMatch> native_matches;
        status = bf_match_core(api_name, matcher, query_descriptors, train_descriptors, mask, native_matches);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return copy_matches_to_output(api_name, native_matches, matches, match_capacity, match_count);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int bf_match_train_core(
    const char* api_name,
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    std::vector<cv::DMatch>& matches)
{
    int status = validate_bf_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    std::vector<cv::Mat> native_masks = to_mat_vector(api_name, masks, mask_count, "masks", status);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    const_cast<jyppx_ocv_features2d_bf_matcher*>(matcher)->value->match(
        opencv_csharp_native::mat_value(query_descriptors),
        matches,
        optional_masks_input_array(native_masks));
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_bf_matcher_match_train_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_match_train_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<cv::DMatch> matches;
        int status = bf_match_train_core(api_name, matcher, query_descriptors, nullptr, 0, matches);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (match_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "match_count");
        }

        *match_count = static_cast<int>(matches.size());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_match_train_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_match_train_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_non_negative_count(api_name, match_capacity, "match_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::DMatch> native_matches;
        status = bf_match_train_core(api_name, matcher, query_descriptors, nullptr, 0, native_matches);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return copy_matches_to_output(api_name, native_matches, matches, match_capacity, match_count);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int bf_knn_match_core(
    const char* api_name,
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    std::vector<std::vector<cv::DMatch>>& matches)
{
    int status = validate_bf_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, train_descriptors, "train_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    if (k <= 0)
    {
        return opencv_csharp_native::set_invalid_argument(api_name, "k");
    }

    const_cast<jyppx_ocv_features2d_bf_matcher*>(matcher)->value->knnMatch(
        opencv_csharp_native::mat_value(query_descriptors),
        opencv_csharp_native::mat_value(train_descriptors),
        matches,
        k,
        optional_input_array(mask),
        compact_result != 0);
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_bf_matcher_knn_match_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    int k,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_knn_match_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> matches;
        int status = bf_knn_match_core(api_name, matcher, query_descriptors, train_descriptors, k, mask, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_knn_match_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
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
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_knn_match_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> native_matches;
        int status = bf_knn_match_core(api_name, matcher, query_descriptors, train_descriptors, k, mask, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_match_train_with_masks_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_match_train_with_masks_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<cv::DMatch> matches;
        int status = bf_match_train_core(api_name, matcher, query_descriptors, masks, mask_count, matches);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (match_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "match_count");
        }

        *match_count = static_cast<int>(matches.size());
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_match_train_with_masks_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_match_train_with_masks_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_non_negative_count(api_name, match_capacity, "match_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        std::vector<cv::DMatch> native_matches;
        status = bf_match_train_core(api_name, matcher, query_descriptors, masks, mask_count, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK
            ? copy_matches_to_output(api_name, native_matches, matches, match_capacity, match_count)
            : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int bf_knn_train_core(
    const char* api_name,
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    std::vector<std::vector<cv::DMatch>>& matches)
{
    int status = validate_bf_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    if (k <= 0)
    {
        return opencv_csharp_native::set_invalid_argument(api_name, "k");
    }

    std::vector<cv::Mat> native_masks = to_mat_vector(api_name, masks, mask_count, "masks", status);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    const_cast<jyppx_ocv_features2d_bf_matcher*>(matcher)->value->knnMatch(
        opencv_csharp_native::mat_value(query_descriptors),
        matches,
        k,
        optional_masks_input_array(native_masks),
        compact_result != 0);
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_bf_matcher_knn_match_train_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_knn_match_train_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> matches;
        int status = bf_knn_train_core(api_name, matcher, query_descriptors, k, nullptr, 0, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_knn_match_train_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_knn_match_train_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> native_matches;
        int status = bf_knn_train_core(api_name, matcher, query_descriptors, k, nullptr, 0, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int bf_radius_match_core(
    const char* api_name,
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    float max_distance,
    const jyppx_ocv_mat* mask,
    int compact_result,
    std::vector<std::vector<cv::DMatch>>& matches)
{
    int status = validate_bf_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, train_descriptors, "train_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    const_cast<jyppx_ocv_features2d_bf_matcher*>(matcher)->value->radiusMatch(
        opencv_csharp_native::mat_value(query_descriptors),
        opencv_csharp_native::mat_value(train_descriptors),
        matches,
        max_distance,
        optional_input_array(mask),
        compact_result != 0);
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_bf_matcher_radius_match_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    float max_distance,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_radius_match_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> matches;
        int status = bf_radius_match_core(api_name, matcher, query_descriptors, train_descriptors, max_distance, mask, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_radius_match_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    const jyppx_ocv_mat* train_descriptors,
    float max_distance,
    const jyppx_ocv_mat* mask,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_radius_match_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> native_matches;
        int status = bf_radius_match_core(api_name, matcher, query_descriptors, train_descriptors, max_distance, mask, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> matches;
        int status = bf_knn_train_core(api_name, matcher, query_descriptors, k, masks, mask_count, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    int k,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> native_matches;
        int status = bf_knn_train_core(api_name, matcher, query_descriptors, k, masks, mask_count, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

static int bf_radius_train_core(
    const char* api_name,
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    std::vector<std::vector<cv::DMatch>>& matches)
{
    int status = validate_bf_matcher(api_name, matcher);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    status = validate_mat(api_name, query_descriptors, "query_descriptors");
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    std::vector<cv::Mat> native_masks = to_mat_vector(api_name, masks, mask_count, "masks", status);
    if (status != OPENCV_CSHARP_STATUS_OK)
    {
        return status;
    }

    const_cast<jyppx_ocv_features2d_bf_matcher*>(matcher)->value->radiusMatch(
        opencv_csharp_native::mat_value(query_descriptors),
        matches,
        max_distance,
        optional_masks_input_array(native_masks),
        compact_result != 0);
    return OPENCV_CSHARP_STATUS_OK;
}

int jyppx_ocv_features2d_bf_matcher_radius_match_train_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_radius_match_train_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> matches;
        int status = bf_radius_train_core(api_name, matcher, query_descriptors, max_distance, nullptr, 0, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_radius_match_train_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_radius_match_train_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> native_matches;
        int status = bf_radius_train_core(api_name, matcher, query_descriptors, max_distance, nullptr, 0, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_count(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> matches;
        int status = bf_radius_train_core(api_name, matcher, query_descriptors, max_distance, masks, mask_count, compact_result, matches);
        return status == OPENCV_CSHARP_STATUS_OK ? summarize_grouped_matches(api_name, matches, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_fill(
    const jyppx_ocv_features2d_bf_matcher* matcher,
    const jyppx_ocv_mat* query_descriptors,
    float max_distance,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int compact_result,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* group_count,
    int* total_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::vector<std::vector<cv::DMatch>> native_matches;
        int status = bf_radius_train_core(api_name, matcher, query_descriptors, max_distance, masks, mask_count, compact_result, native_matches);
        return status == OPENCV_CSHARP_STATUS_OK ? copy_grouped_matches_to_output(api_name, native_matches, offsets, offset_capacity, matches, match_capacity, group_count, total_match_count) : status;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_draw_keypoints(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_key_point* keypoints,
    int keypoint_count,
    jyppx_ocv_mat* out_image,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_draw_keypoints";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_keypoint_input(api_name, keypoints, keypoint_count, "keypoints");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, out_image, "out_image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::drawKeypoints(
            opencv_csharp_native::mat_value(image),
            to_cv_keypoints(keypoints, keypoint_count),
            opencv_csharp_native::mat_value(out_image),
            scalar_from_values(color_v0, color_v1, color_v2, color_v3),
            draw_flags_from_int(flags));
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_draw_matches(
    const jyppx_ocv_mat* img1,
    const jyppx_ocv_key_point* keypoints1,
    int keypoint1_count,
    const jyppx_ocv_mat* img2,
    const jyppx_ocv_key_point* keypoints2,
    int keypoint2_count,
    const jyppx_ocv_dmatch* matches,
    int match_count,
    jyppx_ocv_mat* out_image,
    double match_color_v0,
    double match_color_v1,
    double match_color_v2,
    double match_color_v3,
    double single_point_color_v0,
    double single_point_color_v1,
    double single_point_color_v2,
    double single_point_color_v3,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_draw_matches";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, img1, "img1");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, img2, "img2");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_keypoint_input(api_name, keypoints1, keypoint1_count, "keypoints1");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_keypoint_input(api_name, keypoints2, keypoint2_count, "keypoints2");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_dmatch_input(api_name, matches, match_count, "matches");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, out_image, "out_image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::drawMatches(
            opencv_csharp_native::mat_value(img1),
            to_cv_keypoints(keypoints1, keypoint1_count),
            opencv_csharp_native::mat_value(img2),
            to_cv_keypoints(keypoints2, keypoint2_count),
            to_cv_dmatches(matches, match_count),
            opencv_csharp_native::mat_value(out_image),
            scalar_from_values(match_color_v0, match_color_v1, match_color_v2, match_color_v3),
            scalar_from_values(single_point_color_v0, single_point_color_v1, single_point_color_v2, single_point_color_v3),
            std::vector<char>(),
            draw_flags_from_int(flags));
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_features2d_draw_matches_knn(
    const jyppx_ocv_mat* img1,
    const jyppx_ocv_key_point* keypoints1,
    int keypoint1_count,
    const jyppx_ocv_mat* img2,
    const jyppx_ocv_key_point* keypoints2,
    int keypoint2_count,
    const int* offsets,
    int offset_count,
    const jyppx_ocv_dmatch* matches,
    int match_count,
    jyppx_ocv_mat* out_image,
    double match_color_v0,
    double match_color_v1,
    double match_color_v2,
    double match_color_v3,
    double single_point_color_v0,
    double single_point_color_v1,
    double single_point_color_v2,
    double single_point_color_v3,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_features2d_draw_matches_knn";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, img1, "img1");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, img2, "img2");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_keypoint_input(api_name, keypoints1, keypoint1_count, "keypoints1");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_keypoint_input(api_name, keypoints2, keypoint2_count, "keypoints2");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_dmatch_input(api_name, matches, match_count, "matches");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (offsets == nullptr || offset_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "offsets");
        }

        status = validate_output_mat(api_name, out_image, "out_image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::drawMatches(
            opencv_csharp_native::mat_value(img1),
            to_cv_keypoints(keypoints1, keypoint1_count),
            opencv_csharp_native::mat_value(img2),
            to_cv_keypoints(keypoints2, keypoint2_count),
            to_cv_dmatch_groups(offsets, offset_count, matches, match_count),
            opencv_csharp_native::mat_value(out_image),
            scalar_from_values(match_color_v0, match_color_v1, match_color_v2, match_color_v3),
            scalar_from_values(single_point_color_v0, single_point_color_v1, single_point_color_v2, single_point_color_v3),
            std::vector<std::vector<char>>(),
            draw_flags_from_int(flags));
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

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

int jyppx_ocv_features2d_orb_create(int, float, int, int, int, int, int, int, int, jyppx_ocv_features2d_orb** orb)
{
    if (orb != nullptr)
    {
        *orb = nullptr;
    }

    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_create");
}

void jyppx_ocv_features2d_orb_release(jyppx_ocv_features2d_orb* orb)
{
    delete orb;
}

int jyppx_ocv_features2d_orb_clear(jyppx_ocv_features2d_orb*) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_clear"); }
int jyppx_ocv_features2d_orb_empty(const jyppx_ocv_features2d_orb*, int* empty) { set_zero(empty); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_empty"); }
int jyppx_ocv_features2d_orb_get_max_features(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_get_max_features"); }
int jyppx_ocv_features2d_orb_set_max_features(jyppx_ocv_features2d_orb*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_set_max_features"); }
int jyppx_ocv_features2d_orb_get_scale_factor(const jyppx_ocv_features2d_orb*, double* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_get_scale_factor"); }
int jyppx_ocv_features2d_orb_set_scale_factor(jyppx_ocv_features2d_orb*, double) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_set_scale_factor"); }
int jyppx_ocv_features2d_orb_get_nlevels(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_get_nlevels"); }
int jyppx_ocv_features2d_orb_set_nlevels(jyppx_ocv_features2d_orb*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_set_nlevels"); }
int jyppx_ocv_features2d_orb_get_edge_threshold(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_get_edge_threshold"); }
int jyppx_ocv_features2d_orb_set_edge_threshold(jyppx_ocv_features2d_orb*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_set_edge_threshold"); }
int jyppx_ocv_features2d_orb_get_first_level(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_get_first_level"); }
int jyppx_ocv_features2d_orb_set_first_level(jyppx_ocv_features2d_orb*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_set_first_level"); }
int jyppx_ocv_features2d_orb_get_wta_k(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_get_wta_k"); }
int jyppx_ocv_features2d_orb_set_wta_k(jyppx_ocv_features2d_orb*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_set_wta_k"); }
int jyppx_ocv_features2d_orb_get_score_type(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_get_score_type"); }
int jyppx_ocv_features2d_orb_set_score_type(jyppx_ocv_features2d_orb*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_set_score_type"); }
int jyppx_ocv_features2d_orb_get_patch_size(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_get_patch_size"); }
int jyppx_ocv_features2d_orb_set_patch_size(jyppx_ocv_features2d_orb*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_set_patch_size"); }
int jyppx_ocv_features2d_orb_get_fast_threshold(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_get_fast_threshold"); }
int jyppx_ocv_features2d_orb_set_fast_threshold(jyppx_ocv_features2d_orb*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_set_fast_threshold"); }
int jyppx_ocv_features2d_orb_descriptor_size(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_descriptor_size"); }
int jyppx_ocv_features2d_orb_descriptor_type(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_descriptor_type"); }
int jyppx_ocv_features2d_orb_default_norm(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_default_norm"); }
int jyppx_ocv_features2d_orb_default_name_length(const jyppx_ocv_features2d_orb*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_default_name_length"); }
int jyppx_ocv_features2d_orb_default_name_fill(const jyppx_ocv_features2d_orb*, char*, int, int* written) { set_zero(written); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_default_name_fill"); }
int jyppx_ocv_features2d_orb_detect_count(const jyppx_ocv_features2d_orb*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_detect_count"); }
int jyppx_ocv_features2d_orb_detect_fill(const jyppx_ocv_features2d_orb*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, jyppx_ocv_key_point*, int, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_detect_fill"); }
int jyppx_ocv_features2d_orb_compute(const jyppx_ocv_features2d_orb*, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, jyppx_ocv_key_point*, int, int* count, jyppx_ocv_mat*) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_compute"); }
int jyppx_ocv_features2d_orb_detect_and_compute_count(const jyppx_ocv_features2d_orb*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, int, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_detect_and_compute_count"); }
int jyppx_ocv_features2d_orb_detect_and_compute_fill(const jyppx_ocv_features2d_orb*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, int, jyppx_ocv_key_point*, int, int* count, jyppx_ocv_mat*) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_orb_detect_and_compute_fill"); }

int jyppx_ocv_features2d_bf_matcher_create(int, int, jyppx_ocv_features2d_bf_matcher** matcher)
{
    if (matcher != nullptr)
    {
        *matcher = nullptr;
    }

    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_create");
}

void jyppx_ocv_features2d_bf_matcher_release(jyppx_ocv_features2d_bf_matcher* matcher)
{
    delete matcher;
}

int jyppx_ocv_features2d_bf_matcher_get_norm_type(const jyppx_ocv_features2d_bf_matcher*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_get_norm_type"); }
int jyppx_ocv_features2d_bf_matcher_get_cross_check(const jyppx_ocv_features2d_bf_matcher*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_get_cross_check"); }
int jyppx_ocv_features2d_bf_matcher_is_mask_supported(const jyppx_ocv_features2d_bf_matcher*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_is_mask_supported"); }
int jyppx_ocv_features2d_bf_matcher_empty(const jyppx_ocv_features2d_bf_matcher*, int* value) { set_zero(value); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_empty"); }
int jyppx_ocv_features2d_bf_matcher_clear(jyppx_ocv_features2d_bf_matcher*) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_clear"); }
int jyppx_ocv_features2d_bf_matcher_train(jyppx_ocv_features2d_bf_matcher*) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_train"); }
int jyppx_ocv_features2d_bf_matcher_add(jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat* const*, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_add"); }

int jyppx_ocv_features2d_descriptor_matcher_create_by_type(
    int,
    jyppx_ocv_features2d_descriptor_matcher** matcher)
{
    if (matcher != nullptr)
    {
        *matcher = nullptr;
    }

    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_create_by_type");
}

int jyppx_ocv_features2d_descriptor_matcher_create_by_name(
    const char*,
    int,
    jyppx_ocv_features2d_descriptor_matcher** matcher)
{
    if (matcher != nullptr)
    {
        *matcher = nullptr;
    }

    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_create_by_name");
}

void jyppx_ocv_features2d_descriptor_matcher_release(
    jyppx_ocv_features2d_descriptor_matcher* matcher)
{
    delete matcher;
}

int jyppx_ocv_features2d_descriptor_matcher_clone(
    const jyppx_ocv_features2d_descriptor_matcher*,
    int,
    jyppx_ocv_features2d_descriptor_matcher** clone)
{
    if (clone != nullptr)
    {
        *clone = nullptr;
    }

    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_clone");
}

int jyppx_ocv_features2d_descriptor_matcher_is_mask_supported(
    const jyppx_ocv_features2d_descriptor_matcher*,
    int* supported)
{
    set_zero(supported);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_is_mask_supported");
}

int jyppx_ocv_features2d_descriptor_matcher_empty(
    const jyppx_ocv_features2d_descriptor_matcher*,
    int* empty)
{
    set_zero(empty);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_empty");
}

int jyppx_ocv_features2d_descriptor_matcher_clear(
    jyppx_ocv_features2d_descriptor_matcher*)
{
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_clear");
}

int jyppx_ocv_features2d_descriptor_matcher_train(
    jyppx_ocv_features2d_descriptor_matcher*)
{
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_train");
}

int jyppx_ocv_features2d_descriptor_matcher_add(
    jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat* const*,
    int)
{
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_add");
}

int jyppx_ocv_features2d_descriptor_matcher_get_train_descriptors_count(
    const jyppx_ocv_features2d_descriptor_matcher*,
    int* descriptor_count)
{
    set_zero(descriptor_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_get_train_descriptors_count");
}

int jyppx_ocv_features2d_descriptor_matcher_get_train_descriptor_clone(
    const jyppx_ocv_features2d_descriptor_matcher*,
    int,
    jyppx_ocv_mat** descriptor)
{
    if (descriptor != nullptr)
    {
        *descriptor = nullptr;
    }

    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_get_train_descriptor_clone");
}

int jyppx_ocv_features2d_descriptor_matcher_match_count(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    const jyppx_ocv_mat*,
    const jyppx_ocv_mat*,
    int* match_count)
{
    set_zero(match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_match_count");
}

int jyppx_ocv_features2d_descriptor_matcher_match_fill(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    const jyppx_ocv_mat*,
    const jyppx_ocv_mat*,
    jyppx_ocv_dmatch*,
    int,
    int* match_count)
{
    set_zero(match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_match_fill");
}

int jyppx_ocv_features2d_descriptor_matcher_match_train_count(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    const jyppx_ocv_mat* const*,
    int,
    int* match_count)
{
    set_zero(match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_match_train_count");
}

int jyppx_ocv_features2d_descriptor_matcher_match_train_fill(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    const jyppx_ocv_mat* const*,
    int,
    jyppx_ocv_dmatch*,
    int,
    int* match_count)
{
    set_zero(match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_match_train_fill");
}

int jyppx_ocv_features2d_descriptor_matcher_knn_match_count(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    const jyppx_ocv_mat*,
    int,
    const jyppx_ocv_mat*,
    int,
    int* group_count,
    int* total_match_count)
{
    set_zero(group_count);
    set_zero(total_match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_knn_match_count");
}

int jyppx_ocv_features2d_descriptor_matcher_knn_match_fill(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    const jyppx_ocv_mat*,
    int,
    const jyppx_ocv_mat*,
    int,
    int*,
    int,
    jyppx_ocv_dmatch*,
    int,
    int* group_count,
    int* total_match_count)
{
    set_zero(group_count);
    set_zero(total_match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_knn_match_fill");
}

int jyppx_ocv_features2d_descriptor_matcher_knn_match_train_count(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    int,
    const jyppx_ocv_mat* const*,
    int,
    int,
    int* group_count,
    int* total_match_count)
{
    set_zero(group_count);
    set_zero(total_match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_knn_match_train_count");
}

int jyppx_ocv_features2d_descriptor_matcher_knn_match_train_fill(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    int,
    const jyppx_ocv_mat* const*,
    int,
    int,
    int*,
    int,
    jyppx_ocv_dmatch*,
    int,
    int* group_count,
    int* total_match_count)
{
    set_zero(group_count);
    set_zero(total_match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_knn_match_train_fill");
}

int jyppx_ocv_features2d_descriptor_matcher_radius_match_count(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    const jyppx_ocv_mat*,
    float,
    const jyppx_ocv_mat*,
    int,
    int* group_count,
    int* total_match_count)
{
    set_zero(group_count);
    set_zero(total_match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_radius_match_count");
}

int jyppx_ocv_features2d_descriptor_matcher_radius_match_fill(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    const jyppx_ocv_mat*,
    float,
    const jyppx_ocv_mat*,
    int,
    int*,
    int,
    jyppx_ocv_dmatch*,
    int,
    int* group_count,
    int* total_match_count)
{
    set_zero(group_count);
    set_zero(total_match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_radius_match_fill");
}

int jyppx_ocv_features2d_descriptor_matcher_radius_match_train_count(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    float,
    const jyppx_ocv_mat* const*,
    int,
    int,
    int* group_count,
    int* total_match_count)
{
    set_zero(group_count);
    set_zero(total_match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_radius_match_train_count");
}

int jyppx_ocv_features2d_descriptor_matcher_radius_match_train_fill(
    const jyppx_ocv_features2d_descriptor_matcher*,
    const jyppx_ocv_mat*,
    float,
    const jyppx_ocv_mat* const*,
    int,
    int,
    int*,
    int,
    jyppx_ocv_dmatch*,
    int,
    int* group_count,
    int* total_match_count)
{
    set_zero(group_count);
    set_zero(total_match_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_descriptor_matcher_radius_match_train_fill");
}

int jyppx_ocv_features2d_bf_matcher_clone(
    const jyppx_ocv_features2d_bf_matcher*,
    int,
    jyppx_ocv_features2d_descriptor_matcher** clone)
{
    if (clone != nullptr)
    {
        *clone = nullptr;
    }

    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_clone");
}

int jyppx_ocv_features2d_bf_matcher_get_train_descriptors_count(
    const jyppx_ocv_features2d_bf_matcher*,
    int* descriptor_count)
{
    set_zero(descriptor_count);
    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_get_train_descriptors_count");
}

int jyppx_ocv_features2d_bf_matcher_get_train_descriptor_clone(
    const jyppx_ocv_features2d_bf_matcher*,
    int,
    jyppx_ocv_mat** descriptor)
{
    if (descriptor != nullptr)
    {
        *descriptor = nullptr;
    }

    OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_get_train_descriptor_clone");
}

int jyppx_ocv_features2d_bf_matcher_match_count(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_match_count"); }
int jyppx_ocv_features2d_bf_matcher_match_fill(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, jyppx_ocv_dmatch*, int, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_match_fill"); }
int jyppx_ocv_features2d_bf_matcher_match_train_count(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_match_train_count"); }
int jyppx_ocv_features2d_bf_matcher_match_train_fill(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, jyppx_ocv_dmatch*, int, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_match_train_fill"); }
int jyppx_ocv_features2d_bf_matcher_match_train_with_masks_count(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat* const*, int, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_match_train_with_masks_count"); }
int jyppx_ocv_features2d_bf_matcher_match_train_with_masks_fill(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat* const*, int, jyppx_ocv_dmatch*, int, int* count) { set_zero(count); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_match_train_with_masks_fill"); }
int jyppx_ocv_features2d_bf_matcher_knn_match_count(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, int, const jyppx_ocv_mat*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_knn_match_count"); }
int jyppx_ocv_features2d_bf_matcher_knn_match_fill(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, int, const jyppx_ocv_mat*, int, int*, int, jyppx_ocv_dmatch*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_knn_match_fill"); }
int jyppx_ocv_features2d_bf_matcher_knn_match_train_count(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, int, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_knn_match_train_count"); }
int jyppx_ocv_features2d_bf_matcher_knn_match_train_fill(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, int, int, int*, int, jyppx_ocv_dmatch*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_knn_match_train_fill"); }
int jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_count(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, int, const jyppx_ocv_mat* const*, int, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_count"); }
int jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_fill(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, int, const jyppx_ocv_mat* const*, int, int, int*, int, jyppx_ocv_dmatch*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_knn_match_train_with_masks_fill"); }
int jyppx_ocv_features2d_bf_matcher_radius_match_count(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, float, const jyppx_ocv_mat*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_radius_match_count"); }
int jyppx_ocv_features2d_bf_matcher_radius_match_fill(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, const jyppx_ocv_mat*, float, const jyppx_ocv_mat*, int, int*, int, jyppx_ocv_dmatch*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_radius_match_fill"); }
int jyppx_ocv_features2d_bf_matcher_radius_match_train_count(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, float, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_radius_match_train_count"); }
int jyppx_ocv_features2d_bf_matcher_radius_match_train_fill(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, float, int, int*, int, jyppx_ocv_dmatch*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_radius_match_train_fill"); }
int jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_count(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, float, const jyppx_ocv_mat* const*, int, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_count"); }
int jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_fill(const jyppx_ocv_features2d_bf_matcher*, const jyppx_ocv_mat*, float, const jyppx_ocv_mat* const*, int, int, int*, int, jyppx_ocv_dmatch*, int, int* groups, int* total) { set_zero(groups); set_zero(total); OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_bf_matcher_radius_match_train_with_masks_fill"); }

int jyppx_ocv_features2d_draw_keypoints(const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, jyppx_ocv_mat*, double, double, double, double, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_draw_keypoints"); }
int jyppx_ocv_features2d_draw_matches(const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, const jyppx_ocv_dmatch*, int, jyppx_ocv_mat*, double, double, double, double, double, double, double, double, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_draw_matches"); }
int jyppx_ocv_features2d_draw_matches_knn(const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, const jyppx_ocv_mat*, const jyppx_ocv_key_point*, int, const int*, int, const jyppx_ocv_dmatch*, int, jyppx_ocv_mat*, double, double, double, double, double, double, double, double, int) { OCV_CSHARP_STUB_BODY("jyppx_ocv_features2d_draw_matches_knn"); }

#undef OCV_CSHARP_STUB_BODY

#endif

