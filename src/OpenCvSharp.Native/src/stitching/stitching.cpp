#include "open_cv_sharp/stitching/stitching.h"

#include "../core/mat_handle.h"
#include "../core/utf8_result_handle.h"
#include "../error_state.h"
#include "../features2d/feature_handles.h"
#include "stitching_handles.h"

#include <algorithm>
#include <cmath>
#include <cstdint>
#include <cstring>
#include <limits>
#include <memory>
#include <new>
#include <string>
#include <vector>

namespace
{
    constexpr int DOUBLE_PROPERTY_REGISTRATION_RESOL = 0;
    constexpr int DOUBLE_PROPERTY_SEAM_ESTIMATION_RESOL = 1;
    constexpr int DOUBLE_PROPERTY_COMPOSITING_RESOL = 2;
    constexpr int DOUBLE_PROPERTY_PANO_CONFIDENCE_THRESH = 3;
    constexpr int DOUBLE_PROPERTY_WORK_SCALE = 4;

    constexpr int INT_PROPERTY_WAVE_CORRECTION = 0;
    constexpr int INT_PROPERTY_INTERPOLATION_FLAGS = 1;
    constexpr int INT_PROPERTY_WAVE_CORRECT_KIND = 2;

    int validate_stitcher(const char* api_name, const jyppx_ocv_stitcher* stitcher)
    {
        return stitcher == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "stitcher")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int(const char* api_name, const int* value, const char* argument_name)
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

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat_array(const char* api_name, const jyppx_ocv_mat* const* mats, int mat_count, const char* argument_name)
    {
        if (mat_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (mat_count > 0 && mats == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < mat_count; ++i)
        {
            if (mats[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_exposure_compensator(
        const char* api_name,
        const jyppx_ocv_stitching_exposure_compensator* compensator)
    {
        return compensator == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "compensator")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_py_rotation_warper(
        const char* api_name,
        const jyppx_ocv_stitching_py_rotation_warper* warper,
        bool require_configured)
    {
        if (warper == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "warper");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (!warper->value)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "warper");
        }
        if (require_configured && !warper->configured)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "warper_state");
        }
#else
        (void)require_configured;
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_blender(const char* api_name, const jyppx_ocv_stitching_blender* blender)
    {
        if (blender == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "blender");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (blender->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "blender");
        }
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_bool_int(const char* api_name, int value, const char* argument_name)
    {
        return value == 0 || value == 1
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_invalid_argument(api_name, argument_name);
    }

    int validate_rect_values(const char* api_name, int x, int y, int width, int height, const char* argument_name)
    {
        if (width <= 0 || height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        const std::int64_t right = static_cast<std::int64_t>(x) + width;
        const std::int64_t bottom = static_cast<std::int64_t>(y) + height;
        if (right > std::numeric_limits<int>::max() || right < std::numeric_limits<int>::min() ||
            bottom > std::numeric_limits<int>::max() || bottom < std::numeric_limits<int>::min())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mutable_mat_array(const char* api_name, jyppx_ocv_mat* const* mats, int mat_count, const char* argument_name)
    {
        return validate_mat_array(
            api_name,
            const_cast<const jyppx_ocv_mat* const*>(mats),
            mat_count,
            argument_name);
    }

    int validate_optional_masks(
        const char* api_name,
        const jyppx_ocv_mat* const* masks,
        int mask_count,
        int image_count)
    {
        if (mask_count == 0)
        {
            return OPENCV_CSHARP_STATUS_OK;
        }

        if (mask_count != image_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "masks");
        }

        return validate_mat_array(api_name, masks, mask_count, "masks");
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
    std::vector<cv::Mat> to_mat_vector(const jyppx_ocv_mat* const* mats, int mat_count)
    {
        std::vector<cv::Mat> result;
        result.reserve(static_cast<size_t>(mat_count));
        for (int i = 0; i < mat_count; ++i)
        {
            result.push_back(opencv_csharp_native::mat_value(mats[i]));
        }

        return result;
    }

    int create_mat_handle(const char* api_name, const cv::Mat& value, jyppx_ocv_mat** mat)
    {
        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        *mat = nullptr;
        jyppx_ocv_mat* created = new (std::nothrow) jyppx_ocv_mat();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = value;
        *mat = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_camera_params(
        const char* api_name,
        const cv::detail::CameraParams& source,
        jyppx_ocv_stitching_camera_params* destination)
    {
        destination->focal = source.focal;
        destination->aspect = source.aspect;
        destination->ppx = source.ppx;
        destination->ppy = source.ppy;
        destination->r = nullptr;
        destination->t = nullptr;

        int status = create_mat_handle(api_name, source.R, &destination->r);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = create_mat_handle(api_name, source.t, &destination->t);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            delete destination->r;
            destination->r = nullptr;
            return status;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_exposure_handle(
        const char* api_name,
        cv::Ptr<cv::detail::ExposureCompensator> value,
        jyppx_ocv_stitching_exposure_compensator** compensator)
    {
        if (compensator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "compensator");
        }

        *compensator = nullptr;
        if (value.empty())
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        auto* created = new (std::nothrow) jyppx_ocv_stitching_exposure_compensator();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = std::move(value);
        *compensator = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    std::vector<cv::UMat> to_umat_vector(const jyppx_ocv_mat* const* mats, int mat_count)
    {
        std::vector<cv::UMat> result;
        result.reserve(static_cast<size_t>(mat_count));
        for (int i = 0; i < mat_count; ++i)
        {
            result.push_back(opencv_csharp_native::mat_value(mats[i]).getUMat(cv::ACCESS_READ));
        }

        return result;
    }

    std::vector<cv::UMat> to_writable_umat_vector(jyppx_ocv_mat* const* mats, int mat_count)
    {
        std::vector<cv::UMat> result;
        result.reserve(static_cast<size_t>(mat_count));
        for (int i = 0; i < mat_count; ++i)
        {
            result.push_back(opencv_csharp_native::mat_value(mats[i]).getUMat(cv::ACCESS_RW));
        }

        return result;
    }

    cv::detail::GainCompensator* as_gain(cv::detail::ExposureCompensator* value)
    {
        return dynamic_cast<cv::detail::GainCompensator*>(value);
    }

    cv::detail::ChannelsCompensator* as_channels(cv::detail::ExposureCompensator* value)
    {
        return dynamic_cast<cv::detail::ChannelsCompensator*>(value);
    }

    cv::detail::BlocksCompensator* as_blocks(cv::detail::ExposureCompensator* value)
    {
        return dynamic_cast<cv::detail::BlocksCompensator*>(value);
    }

    int create_blender_handle(
        const char* api_name,
        cv::Ptr<cv::detail::Blender> value,
        int kind,
        jyppx_ocv_stitching_blender** blender)
    {
        if (blender == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "blender");
        }

        *blender = nullptr;
        if (value.empty())
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        auto* created = new (std::nothrow) jyppx_ocv_stitching_blender();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = std::move(value);
        created->kind = kind;
        *blender = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::detail::FeatherBlender* as_feather(cv::detail::Blender* value)
    {
        return dynamic_cast<cv::detail::FeatherBlender*>(value);
    }

    cv::detail::MultiBandBlender* as_multi_band(cv::detail::Blender* value)
    {
        return dynamic_cast<cv::detail::MultiBandBlender*>(value);
    }

    int validate_pyramid(const char* api_name, jyppx_ocv_mat* const* pyramid, int pyramid_count)
    {
        int status = validate_mutable_mat_array(api_name, pyramid, pyramid_count, "pyramid");
        if (status != OPENCV_CSHARP_STATUS_OK || pyramid_count == 0)
        {
            return status;
        }

        const cv::Mat& first = opencv_csharp_native::mat_value(pyramid[0]);
        if (first.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pyramid");
        }

        for (int i = 1; i < pyramid_count; ++i)
        {
            const cv::Mat& previous = opencv_csharp_native::mat_value(pyramid[i - 1]);
            const cv::Mat& current = opencv_csharp_native::mat_value(pyramid[i]);
            if (current.empty() || current.type() != first.type() ||
                current.cols != (previous.cols + 1) / 2 || current.rows != (previous.rows + 1) / 2)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "pyramid");
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    void copy_umat_vector_to_mats(const std::vector<cv::UMat>& source, jyppx_ocv_mat* const* destination)
    {
        for (size_t i = 0; i < source.size(); ++i)
        {
            source[i].copyTo(opencv_csharp_native::mat_value(destination[i]));
        }
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
            keypoint.class_id};
    }

    jyppx_ocv_dmatch from_cv_dmatch(const cv::DMatch& match)
    {
        return jyppx_ocv_dmatch{match.queryIdx, match.trainIdx, match.imgIdx, match.distance};
    }

    int validate_image_features(
        const char* api_name,
        const jyppx_ocv_stitching_image_features* features,
        const char* argument_name)
    {
        return features == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_matches_info(
        const char* api_name,
        const jyppx_ocv_stitching_matches_info* matches_info,
        const char* argument_name)
    {
        return matches_info == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_features_matcher(
        const char* api_name,
        const jyppx_ocv_stitching_features_matcher* matcher)
    {
        if (matcher == nullptr || matcher->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_keypoints(
        const char* api_name,
        const jyppx_ocv_key_point* keypoints,
        int keypoint_count)
    {
        if (keypoint_count < 0 || (keypoint_count > 0 && keypoints == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "keypoints");
        }
        for (int i = 0; i < keypoint_count; ++i)
        {
            if (!std::isfinite(keypoints[i].x) || !std::isfinite(keypoints[i].y) ||
                !std::isfinite(keypoints[i].size) || !std::isfinite(keypoints[i].angle) ||
                !std::isfinite(keypoints[i].response) || keypoints[i].size < 0.0f)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "keypoints");
            }
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int get_feature_finder(
        const char* api_name,
        int finder_kind,
        const void* finder_handle,
        cv::Ptr<cv::Feature2D>& finder)
    {
        if (finder_handle == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "finder_handle");
        }

        switch (finder_kind)
        {
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES2D)
        case 0:
            finder = static_cast<const jyppx_ocv_features2d_orb*>(finder_handle)->value;
            break;
        case 1:
            finder = static_cast<const jyppx_ocv_features2d_sift*>(finder_handle)->value;
            break;
        case 2:
            finder = static_cast<const jyppx_ocv_features2d_fast*>(finder_handle)->value;
            break;
        case 3:
            finder = static_cast<const jyppx_ocv_features2d_gftt*>(finder_handle)->value;
            break;
        case 4:
            finder = static_cast<const jyppx_ocv_features2d_mser*>(finder_handle)->value;
            break;
        case 5:
            finder = static_cast<const jyppx_ocv_features2d_simple_blob*>(finder_handle)->value;
            break;
        case 9:
            finder = static_cast<const jyppx_ocv_features2d_affine*>(finder_handle)->value;
            break;
#endif
#if defined(OPENCV_CSHARP_HAS_OPENCV_XFEATURES2D)
        case 6:
            finder = static_cast<const jyppx_ocv_features2d_brisk*>(finder_handle)->value;
            break;
        case 7:
            finder = static_cast<const jyppx_ocv_features2d_kaze*>(finder_handle)->value;
            break;
        case 8:
            finder = static_cast<const jyppx_ocv_features2d_akaze*>(finder_handle)->value;
            break;
#endif
        default:
            return opencv_csharp_native::set_invalid_argument(api_name, "finder_kind");
        }

        if (finder.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "finder_handle");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_feature_descriptors(
        const char* api_name,
        const cv::detail::ImageFeatures& features,
        const char* argument_name)
    {
        if (features.descriptors.empty())
        {
            return OPENCV_CSHARP_STATUS_OK;
        }
        if (features.descriptors.dims != 2 || features.descriptors.channels() != 1 ||
            (features.descriptors.depth() != CV_8U && features.descriptors.depth() != CV_32F) ||
            features.descriptors.rows != static_cast<int>(features.keypoints.size()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_feature_pair(
        const char* api_name,
        const cv::detail::ImageFeatures& first,
        const cv::detail::ImageFeatures& second)
    {
        int status = validate_feature_descriptors(api_name, first, "first");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_feature_descriptors(api_name, second, "second");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!first.descriptors.empty() && !second.descriptors.empty() &&
            first.descriptors.type() != second.descriptors.type())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "descriptors");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_features_matcher_handle(
        const char* api_name,
        cv::Ptr<cv::detail::FeaturesMatcher> value,
        jyppx_ocv_stitching_features_matcher** matcher)
    {
        if (value.empty())
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        auto* created = new (std::nothrow) jyppx_ocv_stitching_features_matcher();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        created->value = std::move(value);
        *matcher = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_matcher_options(
        const char* api_name,
        int try_gpu,
        float match_confidence,
        int number_of_matches_threshold1,
        int number_of_matches_threshold2,
        double matches_confidence_threshold)
    {
        int status = validate_bool_int(api_name, try_gpu, "try_gpu");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!std::isfinite(match_confidence) || match_confidence < 0.0f ||
            number_of_matches_threshold1 < 0 || number_of_matches_threshold2 < 0 ||
            !std::isfinite(matches_confidence_threshold))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher_options");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_estimator(
        const char* api_name,
        const jyppx_ocv_stitching_estimator* estimator)
    {
        if (estimator == nullptr || estimator->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "estimator");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_bundle_adjuster(
        const char* api_name,
        const jyppx_ocv_stitching_estimator* estimator)
    {
        const int status = validate_estimator(api_name, estimator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (estimator->bundle_adjuster.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bundle_adjuster");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_estimator_handle(
        const char* api_name,
        cv::Ptr<cv::detail::Estimator> value,
        cv::Ptr<cv::detail::BundleAdjusterBase> bundle_adjuster,
        bool requires_initial_cameras,
        jyppx_ocv_stitching_estimator** estimator)
    {
        if (value.empty())
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        auto* created = new (std::nothrow) jyppx_ocv_stitching_estimator();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        created->value = std::move(value);
        created->bundle_adjuster = std::move(bundle_adjuster);
        created->requires_initial_cameras = requires_initial_cameras;
        *estimator = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_feature_match_collections(
        const char* api_name,
        const jyppx_ocv_stitching_image_features* const* features,
        int feature_count,
        const jyppx_ocv_stitching_matches_info* const* pairwise_matches,
        int pairwise_match_count)
    {
        if (feature_count <= 0 || features == nullptr || pairwise_matches == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "collections");
        }
        const std::int64_t required64 = static_cast<std::int64_t>(feature_count) * feature_count;
        if (required64 > std::numeric_limits<int>::max() ||
            pairwise_match_count != static_cast<int>(required64))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pairwise_matches");
        }
        for (int i = 0; i < feature_count; ++i)
        {
            if (features[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "features");
            }
            const int status = validate_feature_descriptors(api_name, features[i]->value, "features");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
        for (int i = 0; i < pairwise_match_count; ++i)
        {
            if (pairwise_matches[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "pairwise_matches");
            }
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int camera_from_abi(
        const char* api_name,
        const jyppx_ocv_stitching_camera_params& source,
        cv::detail::CameraParams& destination)
    {
        if (!std::isfinite(source.focal) || !std::isfinite(source.aspect) ||
            !std::isfinite(source.ppx) || !std::isfinite(source.ppy) ||
            source.r == nullptr || source.t == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "initial_cameras");
        }
        const cv::Mat& rotation = opencv_csharp_native::mat_value(source.r);
        const cv::Mat& translation = opencv_csharp_native::mat_value(source.t);
        if (rotation.dims != 2 || rotation.rows != 3 || rotation.cols != 3 ||
            rotation.channels() != 1 || (rotation.depth() != CV_32F && rotation.depth() != CV_64F) ||
            translation.dims != 2 || translation.rows != 3 || translation.cols != 1 ||
            translation.channels() != 1 || (translation.depth() != CV_32F && translation.depth() != CV_64F))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "initial_cameras");
        }
        destination.focal = source.focal;
        destination.aspect = source.aspect;
        destination.ppx = source.ppx;
        destination.ppy = source.ppy;
        rotation.copyTo(destination.R);
        translation.copyTo(destination.t);
        return OPENCV_CSHARP_STATUS_OK;
    }

    void release_camera_params(jyppx_ocv_stitching_camera_params& value)
    {
        delete value.r;
        delete value.t;
        value.r = nullptr;
        value.t = nullptr;
    }

    void release_camera_params(std::vector<jyppx_ocv_stitching_camera_params>& values)
    {
        for (auto& value : values) { release_camera_params(value); }
    }

    int validate_output_handle_collections(
        const char* api_name,
        jyppx_ocv_stitching_image_features* const* features,
        int feature_capacity,
        jyppx_ocv_stitching_matches_info* const* matches,
        int match_capacity)
    {
        if (feature_capacity <= 0 || features == nullptr || match_capacity <= 0 || matches == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output_collections");
        }
        for (int i = 0; i < feature_capacity; ++i)
        {
            if (features[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "component_features");
            }
            for (int j = 0; j < i; ++j)
            {
                if (features[i] == features[j])
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "component_features");
                }
            }
        }
        for (int i = 0; i < match_capacity; ++i)
        {
            if (matches[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "component_matches");
            }
            for (int j = 0; j < i; ++j)
            {
                if (matches[i] == matches[j])
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "component_matches");
                }
            }
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_packed_paths(
        const char* api_name,
        const unsigned char* buffer,
        int byte_count,
        const int* offsets,
        int path_count,
        int offset_count)
    {
        if (path_count <= 0 || path_count == std::numeric_limits<int>::max() || byte_count < 0 ||
            offset_count != path_count + 1 || offsets == nullptr ||
            (byte_count > 0 && buffer == nullptr) || offsets[0] != 0 || offsets[path_count] != byte_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "paths");
        }
        for (int i = 0; i < path_count; ++i)
        {
            if (offsets[i] < 0 || offsets[i] > offsets[i + 1] || offsets[i + 1] > byte_count)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "paths");
            }
            for (int j = offsets[i]; j < offsets[i + 1]; ++j)
            {
                if (buffer[j] == 0)
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "paths");
                }
            }
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_term_criteria_values(
        const char* api_name,
        int type,
        int max_count,
        double epsilon)
    {
        constexpr int allowed = cv::TermCriteria::COUNT | cv::TermCriteria::EPS;
        if (type <= 0 || (type & ~allowed) != 0 || max_count < 0 || !std::isfinite(epsilon) || epsilon < 0.0 ||
            ((type & cv::TermCriteria::COUNT) != 0 && max_count <= 0) ||
            ((type & cv::TermCriteria::EPS) != 0 && epsilon <= 0.0))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "term_criteria");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_stitching_py_rotation_warper_create_default(jyppx_ocv_stitching_py_rotation_warper** warper)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_py_rotation_warper_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (warper == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "warper");
        }
        *warper = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::unique_ptr<jyppx_ocv_stitching_py_rotation_warper> created(
            new (std::nothrow) jyppx_ocv_stitching_py_rotation_warper());
        if (!created)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        created->value = std::make_unique<cv::PyRotationWarper>();
        created->configured = false;
        *warper = created.release();
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

int jyppx_ocv_stitching_py_rotation_warper_create(
    const unsigned char* type_utf8,
    int type_byte_count,
    float scale,
    jyppx_ocv_stitching_py_rotation_warper** warper)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_py_rotation_warper_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (warper == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "warper");
        }
        *warper = nullptr;
        if (type_byte_count <= 0 || type_utf8 == nullptr ||
            std::memchr(type_utf8, 0, static_cast<size_t>(type_byte_count)) != nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "type_utf8");
        }
        if (!std::isfinite(scale) || scale <= 0.0f)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "scale");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::string type(reinterpret_cast<const char*>(type_utf8), static_cast<size_t>(type_byte_count));
        std::unique_ptr<jyppx_ocv_stitching_py_rotation_warper> created(
            new (std::nothrow) jyppx_ocv_stitching_py_rotation_warper());
        if (!created)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        created->value = std::make_unique<cv::PyRotationWarper>(cv::String(type), scale);
        created->configured = true;
        *warper = created.release();
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

void jyppx_ocv_stitching_py_rotation_warper_release_handle(jyppx_ocv_stitching_py_rotation_warper* warper)
{
    delete warper;
}

int jyppx_ocv_stitching_py_rotation_warper_warp_point(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    float point_x,
    float point_y,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    jyppx_ocv_stitching_point2f* result)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_py_rotation_warper_warp_point";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_py_rotation_warper(api_name, warper, true);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rotation_matrix, "rotation_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (result == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "result"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        cv::Point2f value = warper->value->warpPoint(
            cv::Point2f(point_x, point_y),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(rotation_matrix));
        result->x = value.x;
        result->y = value.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        result->x = 0.0f; result->y = 0.0f;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_py_rotation_warper_warp_point_backward(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    float point_x,
    float point_y,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    jyppx_ocv_stitching_point2f* result)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_py_rotation_warper_warp_point_backward";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_py_rotation_warper(api_name, warper, true);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rotation_matrix, "rotation_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (result == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "result"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        cv::Point2f value = warper->value->warpPointBackward(
            cv::Point2f(point_x, point_y),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(rotation_matrix));
        result->x = value.x;
        result->y = value.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        result->x = 0.0f; result->y = 0.0f;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_py_rotation_warper_build_maps(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    int source_width,
    int source_height,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    jyppx_ocv_mat* x_map,
    jyppx_ocv_mat* y_map,
    jyppx_ocv_stitching_rect* result)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_py_rotation_warper_build_maps";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_py_rotation_warper(api_name, warper, true);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (source_width <= 0 || source_height <= 0) { return opencv_csharp_native::set_invalid_argument(api_name, "source_size"); }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rotation_matrix, "rotation_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, x_map, "x_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, y_map, "y_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (x_map == y_map) { return opencv_csharp_native::set_invalid_argument(api_name, "y_map"); }
        if (result == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "result"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        cv::Rect value = warper->value->buildMaps(
            cv::Size(source_width, source_height),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(rotation_matrix),
            opencv_csharp_native::mat_value(x_map),
            opencv_csharp_native::mat_value(y_map));
        result->x = value.x; result->y = value.y; result->width = value.width; result->height = value.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        result->x = 0; result->y = 0; result->width = 0; result->height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_py_rotation_warper_warp(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    int interpolation_mode,
    int border_mode,
    jyppx_ocv_mat* destination,
    jyppx_ocv_stitching_point* result)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_py_rotation_warper_warp";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_py_rotation_warper(api_name, warper, true);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, source, "source"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rotation_matrix, "rotation_matrix"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, destination, "destination"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (source == destination) { return opencv_csharp_native::set_invalid_argument(api_name, "destination"); }
        if (result == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "result"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        cv::Point value = warper->value->warp(
            opencv_csharp_native::mat_value(source),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(rotation_matrix),
            interpolation_mode,
            border_mode,
            opencv_csharp_native::mat_value(destination));
        result->x = value.x; result->y = value.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        result->x = 0; result->y = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_py_rotation_warper_warp_backward(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    int interpolation_mode,
    int border_mode,
    int destination_width,
    int destination_height,
    jyppx_ocv_mat* destination)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_py_rotation_warper_warp_backward";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_py_rotation_warper(api_name, warper, true);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (destination_width <= 0 || destination_height <= 0) { return opencv_csharp_native::set_invalid_argument(api_name, "destination_size"); }
        status = validate_mat(api_name, source, "source"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rotation_matrix, "rotation_matrix"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, destination, "destination"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (source == destination) { return opencv_csharp_native::set_invalid_argument(api_name, "destination"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        warper->value->warpBackward(
            opencv_csharp_native::mat_value(source),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(rotation_matrix),
            interpolation_mode,
            border_mode,
            cv::Size(destination_width, destination_height),
            opencv_csharp_native::mat_value(destination));
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

int jyppx_ocv_stitching_py_rotation_warper_warp_roi(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    int source_width,
    int source_height,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* rotation_matrix,
    jyppx_ocv_stitching_rect* result)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_py_rotation_warper_warp_roi";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_py_rotation_warper(api_name, warper, true);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (source_width <= 0 || source_height <= 0) { return opencv_csharp_native::set_invalid_argument(api_name, "source_size"); }
        status = validate_mat(api_name, camera_matrix, "camera_matrix"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rotation_matrix, "rotation_matrix"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (result == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "result"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        cv::Rect value = warper->value->warpRoi(
            cv::Size(source_width, source_height),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(rotation_matrix));
        result->x = value.x; result->y = value.y; result->width = value.width; result->height = value.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        result->x = 0; result->y = 0; result->width = 0; result->height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_py_rotation_warper_get_scale(
    const jyppx_ocv_stitching_py_rotation_warper* warper,
    float* scale)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_py_rotation_warper_get_scale";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_py_rotation_warper(api_name, warper, false);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (scale == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "scale"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *scale = warper->value->getScale();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *scale = 0.0f;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_py_rotation_warper_set_scale(
    jyppx_ocv_stitching_py_rotation_warper* warper,
    float scale)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_py_rotation_warper_set_scale";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_py_rotation_warper(api_name, warper, false);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!std::isfinite(scale) || scale <= 0.0f) { return opencv_csharp_native::set_invalid_argument(api_name, "scale"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        warper->value->setScale(scale);
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

int jyppx_ocv_stitcher_create(int mode, jyppx_ocv_stitcher** stitcher)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (stitcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "stitcher");
        }

        *stitcher = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        jyppx_ocv_stitcher* created = new (std::nothrow) jyppx_ocv_stitcher();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::Stitcher::create(static_cast<cv::Stitcher::Mode>(mode));
        *stitcher = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mode;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_stitcher_release_handle(jyppx_ocv_stitcher* stitcher)
{
    delete stitcher;
}

int jyppx_ocv_stitcher_get_double_property(const jyppx_ocv_stitcher* stitcher, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_double_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        switch (property_id)
        {
        case DOUBLE_PROPERTY_REGISTRATION_RESOL: *value = stitcher->value->registrationResol(); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_SEAM_ESTIMATION_RESOL: *value = stitcher->value->seamEstimationResol(); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_COMPOSITING_RESOL: *value = stitcher->value->compositingResol(); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_PANO_CONFIDENCE_THRESH: *value = stitcher->value->panoConfidenceThresh(); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_WORK_SCALE: *value = stitcher->value->workScale(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
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

int jyppx_ocv_stitcher_set_double_property(jyppx_ocv_stitcher* stitcher, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_set_double_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        switch (property_id)
        {
        case DOUBLE_PROPERTY_REGISTRATION_RESOL: stitcher->value->setRegistrationResol(value); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_SEAM_ESTIMATION_RESOL: stitcher->value->setSeamEstimationResol(value); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_COMPOSITING_RESOL: stitcher->value->setCompositingResol(value); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_PANO_CONFIDENCE_THRESH: stitcher->value->setPanoConfidenceThresh(value); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_WORK_SCALE: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_int_property(const jyppx_ocv_stitcher* stitcher, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        switch (property_id)
        {
        case INT_PROPERTY_WAVE_CORRECTION: *value = stitcher->value->waveCorrection() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case INT_PROPERTY_INTERPOLATION_FLAGS: *value = static_cast<int>(stitcher->value->interpolationFlags()); return OPENCV_CSHARP_STATUS_OK;
        case INT_PROPERTY_WAVE_CORRECT_KIND: *value = static_cast<int>(stitcher->value->waveCorrectKind()); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_set_int_property(jyppx_ocv_stitcher* stitcher, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_set_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        switch (property_id)
        {
        case INT_PROPERTY_WAVE_CORRECTION: stitcher->value->setWaveCorrection(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case INT_PROPERTY_INTERPOLATION_FLAGS: stitcher->value->setInterpolationFlags(static_cast<cv::InterpolationFlags>(value)); return OPENCV_CSHARP_STATUS_OK;
        case INT_PROPERTY_WAVE_CORRECT_KIND: stitcher->value->setWaveCorrectKind(static_cast<cv::detail::WaveCorrectKind>(value)); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_estimate_transform(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int* status_code)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_estimate_transform";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_optional_masks(api_name, masks, mask_count, image_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, status_code, "status_code");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Mat> native_images = to_mat_vector(images, image_count);
        if (mask_count == 0)
        {
            *status_code = static_cast<int>(stitcher->value->estimateTransform(native_images));
        }
        else
        {
            std::vector<cv::Mat> native_masks = to_mat_vector(masks, mask_count);
            *status_code = static_cast<int>(stitcher->value->estimateTransform(native_images, native_masks));
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)images; (void)image_count; (void)masks; (void)mask_count;
        *status_code = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_compose_panorama(jyppx_ocv_stitcher* stitcher, jyppx_ocv_mat* pano, int* status_code)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_compose_panorama";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, pano, "pano");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, status_code, "status_code");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *status_code = static_cast<int>(stitcher->value->composePanorama(opencv_csharp_native::mat_value(pano)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *status_code = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_compose_panorama_images(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    jyppx_ocv_mat* pano,
    int* status_code)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_compose_panorama_images";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, pano, "pano");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, status_code, "status_code");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Mat> native_images = to_mat_vector(images, image_count);
        *status_code = static_cast<int>(stitcher->value->composePanorama(native_images, opencv_csharp_native::mat_value(pano)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)images; (void)image_count;
        *status_code = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_stitch(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    jyppx_ocv_mat* pano,
    int* status_code)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_stitch";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_optional_masks(api_name, masks, mask_count, image_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, pano, "pano");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, status_code, "status_code");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Mat> native_images = to_mat_vector(images, image_count);
        if (mask_count == 0)
        {
            *status_code = static_cast<int>(stitcher->value->stitch(native_images, opencv_csharp_native::mat_value(pano)));
        }
        else
        {
            std::vector<cv::Mat> native_masks = to_mat_vector(masks, mask_count);
            *status_code = static_cast<int>(stitcher->value->stitch(native_images, native_masks, opencv_csharp_native::mat_value(pano)));
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)images; (void)image_count; (void)masks; (void)mask_count;
        *status_code = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_component_count(const jyppx_ocv_stitcher* stitcher, int* component_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_component_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, component_count, "component_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *component_count = static_cast<int>(stitcher->value->component().size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *component_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_component_fill(const jyppx_ocv_stitcher* stitcher, int* components, int component_capacity, int* component_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_component_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (component_capacity < 0 || (component_capacity > 0 && components == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "components");
        }
        status = validate_output_int(api_name, component_count, "component_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<int> values = stitcher->value->component();
        *component_count = static_cast<int>(values.size());
        const int writable = component_capacity < *component_count ? component_capacity : *component_count;
        for (int i = 0; i < writable; ++i)
        {
            components[i] = values[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        *component_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_cameras_count(const jyppx_ocv_stitcher* stitcher, int* camera_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_cameras_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, camera_count, "camera_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *camera_count = static_cast<int>(stitcher->value->cameras().size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *camera_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_cameras_fill(
    const jyppx_ocv_stitcher* stitcher,
    jyppx_ocv_stitching_camera_params* cameras,
    int camera_capacity,
    int* camera_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_cameras_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (camera_capacity < 0 || (camera_capacity > 0 && cameras == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "cameras");
        }
        status = validate_output_int(api_name, camera_count, "camera_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::detail::CameraParams> values = stitcher->value->cameras();
        *camera_count = static_cast<int>(values.size());
        const int writable = camera_capacity < *camera_count ? camera_capacity : *camera_count;
        for (int i = 0; i < writable; ++i)
        {
            status = copy_camera_params(api_name, values[static_cast<size_t>(i)], &cameras[i]);
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                for (int cleanup = 0; cleanup < i; ++cleanup)
                {
                    delete cameras[cleanup].r;
                    delete cameras[cleanup].t;
                    cameras[cleanup].r = nullptr;
                    cameras[cleanup].t = nullptr;
                }

                return status;
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        *camera_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_result_mask(const jyppx_ocv_stitcher* stitcher, jyppx_ocv_mat* result_mask)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_result_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, result_mask, "result_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        stitcher->value->resultMask().copyTo(opencv_csharp_native::mat_value(result_mask));
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

int jyppx_ocv_stitching_exposure_create_default(
    int type,
    jyppx_ocv_stitching_exposure_compensator** compensator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (compensator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "compensator");
        }
        *compensator = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (type < cv::detail::ExposureCompensator::NO || type > cv::detail::ExposureCompensator::CHANNELS_BLOCKS)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "type");
        }
        return create_exposure_handle(api_name, cv::detail::ExposureCompensator::createDefault(type), compensator);
#else
        (void)type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_create_no(jyppx_ocv_stitching_exposure_compensator** compensator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_create_no";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (compensator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "compensator");
        }
        *compensator = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        return create_exposure_handle(api_name, cv::makePtr<cv::detail::NoExposureCompensator>(), compensator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_create_gain(
    int number_of_feeds,
    jyppx_ocv_stitching_exposure_compensator** compensator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_create_gain";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (compensator == nullptr || number_of_feeds <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, compensator == nullptr ? "compensator" : "number_of_feeds");
        }
        *compensator = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        return create_exposure_handle(api_name, cv::makePtr<cv::detail::GainCompensator>(number_of_feeds), compensator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_create_channels(
    int number_of_feeds,
    jyppx_ocv_stitching_exposure_compensator** compensator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_create_channels";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (compensator == nullptr || number_of_feeds <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, compensator == nullptr ? "compensator" : "number_of_feeds");
        }
        *compensator = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        return create_exposure_handle(api_name, cv::makePtr<cv::detail::ChannelsCompensator>(number_of_feeds), compensator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_create_blocks_gain(
    int block_width,
    int block_height,
    int number_of_feeds,
    jyppx_ocv_stitching_exposure_compensator** compensator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_create_blocks_gain";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (compensator == nullptr || block_width <= 0 || block_height <= 0 || number_of_feeds <= 0)
        {
            const char* argument = compensator == nullptr ? "compensator" : block_width <= 0 ? "block_width" : block_height <= 0 ? "block_height" : "number_of_feeds";
            return opencv_csharp_native::set_invalid_argument(api_name, argument);
        }
        *compensator = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        return create_exposure_handle(api_name, cv::makePtr<cv::detail::BlocksGainCompensator>(block_width, block_height, number_of_feeds), compensator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_create_blocks_channels(
    int block_width,
    int block_height,
    int number_of_feeds,
    jyppx_ocv_stitching_exposure_compensator** compensator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_create_blocks_channels";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (compensator == nullptr || block_width <= 0 || block_height <= 0 || number_of_feeds <= 0)
        {
            const char* argument = compensator == nullptr ? "compensator" : block_width <= 0 ? "block_width" : block_height <= 0 ? "block_height" : "number_of_feeds";
            return opencv_csharp_native::set_invalid_argument(api_name, argument);
        }
        *compensator = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        return create_exposure_handle(api_name, cv::makePtr<cv::detail::BlocksChannelsCompensator>(block_width, block_height, number_of_feeds), compensator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_stitching_exposure_release_handle(jyppx_ocv_stitching_exposure_compensator* compensator)
{
    delete compensator;
}

int jyppx_ocv_stitching_exposure_feed(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    const int* corner_x,
    const int* corner_y,
    int corner_count,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_feed";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (corner_count < 0 || (corner_count > 0 && (corner_x == nullptr || corner_y == nullptr)))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "corners");
        }
        status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, masks, mask_count, "masks");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (corner_count != image_count || mask_count != image_count || image_count == 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "collection_count");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Point> native_corners;
        native_corners.reserve(static_cast<size_t>(corner_count));
        for (int i = 0; i < corner_count; ++i)
        {
            native_corners.emplace_back(corner_x[i], corner_y[i]);
        }
        compensator->value->feed(native_corners, to_umat_vector(images, image_count), to_umat_vector(masks, mask_count));
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

int jyppx_ocv_stitching_exposure_apply(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    int index,
    int corner_x,
    int corner_y,
    jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_apply";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (index < 0) { return opencv_csharp_native::set_invalid_argument(api_name, "index"); }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        compensator->value->apply(index, cv::Point(corner_x, corner_y), opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(mask));
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

int jyppx_ocv_stitching_exposure_get_mat_gains_count(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    int* gain_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_get_mat_gains_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, gain_count, "gain_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Mat> gains;
        compensator->value->getMatGains(gains);
        *gain_count = static_cast<int>(gains.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *gain_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_get_mat_gains_fill(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    jyppx_ocv_mat** gains,
    int gain_capacity,
    int* gain_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_get_mat_gains_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (gain_capacity < 0 || (gain_capacity > 0 && gains == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "gains");
        }
        status = validate_output_int(api_name, gain_count, "gain_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        for (int i = 0; i < gain_capacity; ++i) { gains[i] = nullptr; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Mat> native_gains;
        compensator->value->getMatGains(native_gains);
        *gain_count = static_cast<int>(native_gains.size());
        const int writable = gain_capacity < *gain_count ? gain_capacity : *gain_count;
        int created_count = 0;
        try
        {
            for (; created_count < writable; ++created_count)
            {
                status = create_mat_handle(
                    api_name,
                    native_gains[static_cast<size_t>(created_count)].clone(),
                    &gains[created_count]);
                if (status != OPENCV_CSHARP_STATUS_OK)
                {
                    for (int cleanup = 0; cleanup < created_count; ++cleanup)
                    {
                        delete gains[cleanup];
                        gains[cleanup] = nullptr;
                    }
                    return status;
                }
            }
        }
        catch (...)
        {
            for (int cleanup = 0; cleanup < created_count; ++cleanup)
            {
                delete gains[cleanup];
                gains[cleanup] = nullptr;
            }
            throw;
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        *gain_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_set_mat_gains(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    const jyppx_ocv_mat* const* gains,
    int gain_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_set_mat_gains";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, gains, gain_count, "gains");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Mat> native_gains = to_mat_vector(gains, gain_count);
        compensator->value->setMatGains(native_gains);
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

int jyppx_ocv_stitching_exposure_get_update_gain(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    int* update_gain)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_get_update_gain";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, update_gain, "update_gain");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *update_gain = compensator->value->getUpdateGain() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *update_gain = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_set_update_gain(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    int update_gain)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_set_update_gain";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (update_gain != 0 && update_gain != 1) { return opencv_csharp_native::set_invalid_argument(api_name, "update_gain"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        compensator->value->setUpdateGain(update_gain != 0);
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

int jyppx_ocv_stitching_exposure_get_number_of_feeds(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    int* number_of_feeds)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_get_number_of_feeds";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, number_of_feeds, "number_of_feeds");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (auto* value = as_gain(compensator->value.get())) { *number_of_feeds = value->getNrFeeds(); }
        else if (auto* value = as_channels(compensator->value.get())) { *number_of_feeds = value->getNrFeeds(); }
        else if (auto* value = as_blocks(compensator->value.get())) { *number_of_feeds = value->getNrFeeds(); }
        else { return opencv_csharp_native::set_invalid_argument(api_name, "compensator_type"); }
        return OPENCV_CSHARP_STATUS_OK;
#else
        *number_of_feeds = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_set_number_of_feeds(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    int number_of_feeds)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_set_number_of_feeds";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (number_of_feeds <= 0) { return opencv_csharp_native::set_invalid_argument(api_name, "number_of_feeds"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (auto* value = as_gain(compensator->value.get())) { value->setNrFeeds(number_of_feeds); }
        else if (auto* value = as_channels(compensator->value.get())) { value->setNrFeeds(number_of_feeds); }
        else if (auto* value = as_blocks(compensator->value.get())) { value->setNrFeeds(number_of_feeds); }
        else { return opencv_csharp_native::set_invalid_argument(api_name, "compensator_type"); }
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

int jyppx_ocv_stitching_exposure_get_similarity_threshold(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    double* similarity_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_get_similarity_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, similarity_threshold, "similarity_threshold");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (auto* value = as_gain(compensator->value.get())) { *similarity_threshold = value->getSimilarityThreshold(); }
        else if (auto* value = as_channels(compensator->value.get())) { *similarity_threshold = value->getSimilarityThreshold(); }
        else if (auto* value = as_blocks(compensator->value.get())) { *similarity_threshold = value->getSimilarityThreshold(); }
        else { return opencv_csharp_native::set_invalid_argument(api_name, "compensator_type"); }
        return OPENCV_CSHARP_STATUS_OK;
#else
        *similarity_threshold = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_set_similarity_threshold(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    double similarity_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_set_similarity_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!std::isfinite(similarity_threshold)) { return opencv_csharp_native::set_invalid_argument(api_name, "similarity_threshold"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (auto* value = as_gain(compensator->value.get())) { value->setSimilarityThreshold(similarity_threshold); }
        else if (auto* value = as_channels(compensator->value.get())) { value->setSimilarityThreshold(similarity_threshold); }
        else if (auto* value = as_blocks(compensator->value.get())) { value->setSimilarityThreshold(similarity_threshold); }
        else { return opencv_csharp_native::set_invalid_argument(api_name, "compensator_type"); }
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

int jyppx_ocv_stitching_exposure_get_block_size(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    int* block_width,
    int* block_height)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_get_block_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, block_width, "block_width");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, block_height, "block_height");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        auto* value = as_blocks(compensator->value.get());
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "compensator_type"); }
        const cv::Size size = value->getBlockSize();
        *block_width = size.width;
        *block_height = size.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *block_width = 0;
        *block_height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_set_block_size(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    int block_width,
    int block_height)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_set_block_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (block_width <= 0 || block_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, block_width <= 0 ? "block_width" : "block_height");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        auto* value = as_blocks(compensator->value.get());
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "compensator_type"); }
        value->setBlockSize(block_width, block_height);
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

int jyppx_ocv_stitching_exposure_get_filtering_iterations(
    const jyppx_ocv_stitching_exposure_compensator* compensator,
    int* filtering_iterations)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_get_filtering_iterations";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, filtering_iterations, "filtering_iterations");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        auto* value = as_blocks(compensator->value.get());
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "compensator_type"); }
        *filtering_iterations = value->getNrGainsFilteringIterations();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *filtering_iterations = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_exposure_set_filtering_iterations(
    jyppx_ocv_stitching_exposure_compensator* compensator,
    int filtering_iterations)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_exposure_set_filtering_iterations";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_exposure_compensator(api_name, compensator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (filtering_iterations < 0) { return opencv_csharp_native::set_invalid_argument(api_name, "filtering_iterations"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        auto* value = as_blocks(compensator->value.get());
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "compensator_type"); }
        value->setNrGainsFilteringIterations(filtering_iterations);
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

int jyppx_ocv_stitching_blender_create_default(
    int type,
    int try_gpu,
    jyppx_ocv_stitching_blender** blender)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (blender == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "blender"); }
        *blender = nullptr;
        if (type < 0 || type > 2) { return opencv_csharp_native::set_invalid_argument(api_name, "type"); }
        int status = validate_bool_int(api_name, try_gpu, "try_gpu");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        return create_blender_handle(
            api_name,
            cv::detail::Blender::createDefault(type, try_gpu != 0),
            type,
            blender);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_blender_create_feather(
    float sharpness,
    jyppx_ocv_stitching_blender** blender)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_create_feather";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (blender == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "blender"); }
        *blender = nullptr;
        if (!std::isfinite(sharpness)) { return opencv_csharp_native::set_invalid_argument(api_name, "sharpness"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        return create_blender_handle(
            api_name,
            cv::makePtr<cv::detail::FeatherBlender>(sharpness),
            1,
            blender);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_blender_create_multi_band(
    int try_gpu,
    int number_of_bands,
    int weight_type,
    jyppx_ocv_stitching_blender** blender)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_create_multi_band";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (blender == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "blender"); }
        *blender = nullptr;
        int status = validate_bool_int(api_name, try_gpu, "try_gpu");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (number_of_bands < 0 || number_of_bands > 30)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "number_of_bands");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (weight_type != CV_32FC1 && weight_type != CV_16SC1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "weight_type");
        }
        return create_blender_handle(
            api_name,
            cv::makePtr<cv::detail::MultiBandBlender>(try_gpu, number_of_bands, weight_type),
            2,
            blender);
#else
        (void)weight_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_stitching_blender_release_handle(jyppx_ocv_stitching_blender* blender)
{
    delete blender;
}

int jyppx_ocv_stitching_blender_prepare(
    jyppx_ocv_stitching_blender* blender,
    const int* corner_x,
    const int* corner_y,
    const int* widths,
    const int* heights,
    int item_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_prepare";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_blender(api_name, blender);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (item_count <= 0 || corner_x == nullptr || corner_y == nullptr || widths == nullptr || heights == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "items");
        }

        std::int64_t left = std::numeric_limits<int>::max();
        std::int64_t top = std::numeric_limits<int>::max();
        std::int64_t right = std::numeric_limits<int>::min();
        std::int64_t bottom = std::numeric_limits<int>::min();
        for (int i = 0; i < item_count; ++i)
        {
            status = validate_rect_values(api_name, corner_x[i], corner_y[i], widths[i], heights[i], "sizes");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            left = std::min(left, static_cast<std::int64_t>(corner_x[i]));
            top = std::min(top, static_cast<std::int64_t>(corner_y[i]));
            right = std::max(right, static_cast<std::int64_t>(corner_x[i]) + widths[i]);
            bottom = std::max(bottom, static_cast<std::int64_t>(corner_y[i]) + heights[i]);
        }

        if (right - left > std::numeric_limits<int>::max() || bottom - top > std::numeric_limits<int>::max())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "items");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Point> corners;
        std::vector<cv::Size> sizes;
        corners.reserve(static_cast<size_t>(item_count));
        sizes.reserve(static_cast<size_t>(item_count));
        for (int i = 0; i < item_count; ++i)
        {
            corners.emplace_back(corner_x[i], corner_y[i]);
            sizes.emplace_back(widths[i], heights[i]);
        }

        blender->prepared = false;
        blender->value->prepare(corners, sizes);
        blender->prepared_roi = cv::Rect(
            static_cast<int>(left),
            static_cast<int>(top),
            static_cast<int>(right - left),
            static_cast<int>(bottom - top));
        blender->prepared = true;
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

int jyppx_ocv_stitching_blender_prepare_roi(
    jyppx_ocv_stitching_blender* blender,
    int x,
    int y,
    int width,
    int height)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_prepare_roi";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_blender(api_name, blender);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_rect_values(api_name, x, y, width, height, "destination_roi");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        blender->prepared = false;
        blender->value->prepare(cv::Rect(x, y, width, height));
        blender->prepared_roi = cv::Rect(x, y, width, height);
        blender->prepared = true;
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

int jyppx_ocv_stitching_blender_feed(
    jyppx_ocv_stitching_blender* blender,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    int top_left_x,
    int top_left_y)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_feed";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_blender(api_name, blender);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (!blender->prepared) { return opencv_csharp_native::set_invalid_argument(api_name, "blender_state"); }
        const cv::Mat& native_image = opencv_csharp_native::mat_value(image);
        const cv::Mat& native_mask = opencv_csharp_native::mat_value(mask);
        const bool valid_image_type = blender->kind == 2
            ? native_image.type() == CV_8UC3 || native_image.type() == CV_16SC3
            : native_image.type() == CV_16SC3;
        if (native_image.empty() || !valid_image_type)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }
        if (native_mask.empty() || native_mask.type() != CV_8UC1 || native_mask.size() != native_image.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mask");
        }

        const std::int64_t right = static_cast<std::int64_t>(top_left_x) + native_image.cols;
        const std::int64_t bottom = static_cast<std::int64_t>(top_left_y) + native_image.rows;
        const std::int64_t prepared_right = static_cast<std::int64_t>(blender->prepared_roi.x) + blender->prepared_roi.width;
        const std::int64_t prepared_bottom = static_cast<std::int64_t>(blender->prepared_roi.y) + blender->prepared_roi.height;
        if (top_left_x < blender->prepared_roi.x || top_left_y < blender->prepared_roi.y ||
            right > prepared_right || bottom > prepared_bottom)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "top_left");
        }

        blender->value->feed(native_image, native_mask, cv::Point(top_left_x, top_left_y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)top_left_x; (void)top_left_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_blender_blend(
    jyppx_ocv_stitching_blender* blender,
    jyppx_ocv_mat* destination,
    jyppx_ocv_mat* destination_mask)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_blend";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_blender(api_name, blender);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, destination, "destination");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, destination_mask, "destination_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (destination == destination_mask)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "destination_mask");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (!blender->prepared) { return opencv_csharp_native::set_invalid_argument(api_name, "blender_state"); }
        blender->prepared = false;
        blender->value->blend(
            opencv_csharp_native::mat_value(destination),
            opencv_csharp_native::mat_value(destination_mask));
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

int jyppx_ocv_stitching_blender_get_sharpness(
    const jyppx_ocv_stitching_blender* blender,
    float* sharpness)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_get_sharpness";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_blender(api_name, blender);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, sharpness, "sharpness");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        auto* value = as_feather(blender->value.get());
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "blender_type"); }
        *sharpness = value->sharpness();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *sharpness = 0.0f;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_blender_set_sharpness(
    jyppx_ocv_stitching_blender* blender,
    float sharpness)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_set_sharpness";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_blender(api_name, blender);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!std::isfinite(sharpness)) { return opencv_csharp_native::set_invalid_argument(api_name, "sharpness"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        auto* value = as_feather(blender->value.get());
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "blender_type"); }
        value->setSharpness(sharpness);
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

int jyppx_ocv_stitching_blender_get_number_of_bands(
    const jyppx_ocv_stitching_blender* blender,
    int* number_of_bands)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_get_number_of_bands";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_blender(api_name, blender);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, number_of_bands, "number_of_bands");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        auto* value = as_multi_band(blender->value.get());
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "blender_type"); }
        *number_of_bands = value->numBands();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *number_of_bands = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_blender_set_number_of_bands(
    jyppx_ocv_stitching_blender* blender,
    int number_of_bands)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_set_number_of_bands";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_blender(api_name, blender);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (number_of_bands < 0 || number_of_bands > 30)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "number_of_bands");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        auto* value = as_multi_band(blender->value.get());
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "blender_type"); }
        value->setNumBands(number_of_bands);
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

int jyppx_ocv_stitching_blender_create_weight_maps(
    jyppx_ocv_stitching_blender* blender,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    const int* corner_x,
    const int* corner_y,
    int corner_count,
    jyppx_ocv_mat* const* weight_maps,
    int weight_map_count,
    jyppx_ocv_stitching_rect* result)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_blender_create_weight_maps";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_blender(api_name, blender);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, masks, mask_count, "masks");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mutable_mat_array(api_name, weight_maps, weight_map_count, "weight_maps");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (mask_count <= 0 || corner_count != mask_count || weight_map_count != mask_count ||
            corner_x == nullptr || corner_y == nullptr || result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "collections");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        auto* value = as_feather(blender->value.get());
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "blender_type"); }

        std::vector<cv::Point> corners;
        corners.reserve(static_cast<size_t>(corner_count));
        for (int i = 0; i < mask_count; ++i)
        {
            const cv::Mat& mask = opencv_csharp_native::mat_value(masks[i]);
            if (mask.empty() || mask.type() != CV_8UC1)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "masks");
            }
            status = validate_rect_values(api_name, corner_x[i], corner_y[i], mask.cols, mask.rows, "corners");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            corners.emplace_back(corner_x[i], corner_y[i]);
        }

        std::vector<cv::UMat> native_weight_maps;
        const cv::Rect roi = value->createWeightMaps(to_umat_vector(masks, mask_count), corners, native_weight_maps);
        if (native_weight_maps.size() != static_cast<size_t>(weight_map_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "weight_maps");
        }
        copy_umat_vector_to_mats(native_weight_maps, weight_maps);
        result->x = roi.x;
        result->y = roi.y;
        result->width = roi.width;
        result->height = roi.height;
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

int jyppx_ocv_stitching_normalize_using_weight_map(
    const jyppx_ocv_mat* weight,
    jyppx_ocv_mat* source)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_normalize_using_weight_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, weight, "weight");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, source, "source");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        const cv::Mat& native_weight = opencv_csharp_native::mat_value(weight);
        cv::Mat& native_source = opencv_csharp_native::mat_value(source);
        if (native_source.empty() || native_source.type() != CV_16SC3)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "source");
        }
        if (native_weight.empty() ||
            (native_weight.type() != CV_32FC1 && native_weight.type() != CV_16SC1) ||
            native_weight.size() != native_source.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "weight");
        }
        cv::detail::normalizeUsingWeightMap(native_weight, native_source);
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

int jyppx_ocv_stitching_create_weight_map(
    const jyppx_ocv_mat* mask,
    float sharpness,
    jyppx_ocv_mat* weight)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_create_weight_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, weight, "weight");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!std::isfinite(sharpness)) { return opencv_csharp_native::set_invalid_argument(api_name, "sharpness"); }
        if (mask == weight) { return opencv_csharp_native::set_invalid_argument(api_name, "weight"); }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        const cv::Mat& native_mask = opencv_csharp_native::mat_value(mask);
        if (native_mask.empty() || native_mask.type() != CV_8UC1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mask");
        }
        cv::detail::createWeightMap(native_mask, sharpness, opencv_csharp_native::mat_value(weight));
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

int jyppx_ocv_stitching_create_laplace_pyramid(
    const jyppx_ocv_mat* image,
    int number_of_levels,
    jyppx_ocv_mat* const* pyramid,
    int pyramid_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_create_laplace_pyramid";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mutable_mat_array(api_name, pyramid, pyramid_count, "pyramid");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (number_of_levels < 0 || number_of_levels > 30 || pyramid_count != number_of_levels + 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "number_of_levels");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        const cv::Mat& native_image = opencv_csharp_native::mat_value(image);
        if (native_image.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "image"); }
        std::vector<cv::UMat> native_pyramid;
        cv::detail::createLaplacePyr(native_image, number_of_levels, native_pyramid);
        copy_umat_vector_to_mats(native_pyramid, pyramid);
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

int jyppx_ocv_stitching_create_laplace_pyramid_gpu(
    const jyppx_ocv_mat* image,
    int number_of_levels,
    jyppx_ocv_mat* const* pyramid,
    int pyramid_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_create_laplace_pyramid_gpu";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mutable_mat_array(api_name, pyramid, pyramid_count, "pyramid");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (number_of_levels < 0 || number_of_levels > 30 || pyramid_count != number_of_levels + 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "number_of_levels");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        const cv::Mat& native_image = opencv_csharp_native::mat_value(image);
        if (native_image.empty()) { return opencv_csharp_native::set_invalid_argument(api_name, "image"); }
        std::vector<cv::UMat> native_pyramid;
        cv::detail::createLaplacePyrGpu(native_image, number_of_levels, native_pyramid);
        copy_umat_vector_to_mats(native_pyramid, pyramid);
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

int jyppx_ocv_stitching_restore_image_from_laplace_pyramid(
    jyppx_ocv_mat* const* pyramid,
    int pyramid_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_restore_image_from_laplace_pyramid";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mutable_mat_array(api_name, pyramid, pyramid_count, "pyramid");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        status = validate_pyramid(api_name, pyramid, pyramid_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::UMat> native_pyramid = to_writable_umat_vector(pyramid, pyramid_count);
        cv::detail::restoreImageFromLaplacePyr(native_pyramid);
        copy_umat_vector_to_mats(native_pyramid, pyramid);
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

int jyppx_ocv_stitching_restore_image_from_laplace_pyramid_gpu(
    jyppx_ocv_mat* const* pyramid,
    int pyramid_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_restore_image_from_laplace_pyramid_gpu";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mutable_mat_array(api_name, pyramid, pyramid_count, "pyramid");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        status = validate_pyramid(api_name, pyramid, pyramid_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::UMat> native_pyramid = to_writable_umat_vector(pyramid, pyramid_count);
        cv::detail::restoreImageFromLaplacePyrGpu(native_pyramid);
        copy_umat_vector_to_mats(native_pyramid, pyramid);
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

int jyppx_ocv_stitching_image_features_create(
    int image_index,
    int image_width,
    int image_height,
    const jyppx_ocv_key_point* keypoints,
    int keypoint_count,
    const jyppx_ocv_mat* descriptors,
    jyppx_ocv_stitching_image_features** features)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_image_features_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (features == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "features");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *features = nullptr;
        int status = validate_keypoints(api_name, keypoints, keypoint_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, descriptors, "descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_width < 0 || image_height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_size");
        }

        std::unique_ptr<jyppx_ocv_stitching_image_features> created(
            new (std::nothrow) jyppx_ocv_stitching_image_features());
        if (!created)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        created->value.img_idx = image_index;
        created->value.img_size = cv::Size(image_width, image_height);
        created->value.keypoints.reserve(static_cast<size_t>(keypoint_count));
        for (int i = 0; i < keypoint_count; ++i)
        {
            created->value.keypoints.push_back(to_cv_keypoint(keypoints[i]));
        }
        opencv_csharp_native::mat_value(descriptors).copyTo(created->value.descriptors);
        status = validate_feature_descriptors(api_name, created->value, "descriptors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *features = created.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image_index;
        (void)image_width;
        (void)image_height;
        (void)keypoints;
        (void)keypoint_count;
        (void)descriptors;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_stitching_image_features_release_handle(jyppx_ocv_stitching_image_features* features)
{
    delete features;
}

int jyppx_ocv_stitching_image_features_get_image_index(
    const jyppx_ocv_stitching_image_features* features,
    int* image_index)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_image_features_get_image_index";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (features == nullptr || image_index == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *image_index = features->value.img_idx;
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

int jyppx_ocv_stitching_image_features_set_image_index(
    jyppx_ocv_stitching_image_features* features,
    int image_index)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_image_features_set_image_index";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (features == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "features");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        features->value.img_idx = image_index;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image_index;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_image_features_get_image_size(
    const jyppx_ocv_stitching_image_features* features,
    int* image_width,
    int* image_height)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_image_features_get_image_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (features == nullptr || image_width == nullptr || image_height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *image_width = features->value.img_size.width;
        *image_height = features->value.img_size.height;
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

int jyppx_ocv_stitching_image_features_set_image_size(
    jyppx_ocv_stitching_image_features* features,
    int image_width,
    int image_height)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_image_features_set_image_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (features == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "features");
        }
        if (image_width < 0 || image_height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_size");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        features->value.img_size = cv::Size(image_width, image_height);
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

int jyppx_ocv_stitching_image_features_get_keypoints_count(
    const jyppx_ocv_stitching_image_features* features,
    int* keypoint_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_image_features_get_keypoints_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (features == nullptr || keypoint_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (features->value.keypoints.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "keypoints");
        }
        *keypoint_count = static_cast<int>(features->value.keypoints.size());
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

int jyppx_ocv_stitching_image_features_get_keypoints_fill(
    const jyppx_ocv_stitching_image_features* features,
    jyppx_ocv_key_point* keypoints,
    int keypoint_capacity,
    int* keypoint_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_image_features_get_keypoints_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (features == nullptr || keypoint_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        const size_t required = features->value.keypoints.size();
        if (required > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "keypoints");
        }
        *keypoint_count = static_cast<int>(required);
        if (keypoint_capacity < 0 || static_cast<size_t>(keypoint_capacity) < required ||
            (required > 0 && keypoints == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "keypoints");
        }
        for (size_t i = 0; i < required; ++i)
        {
            keypoints[i] = from_cv_keypoint(features->value.keypoints[i]);
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)keypoints;
        (void)keypoint_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_image_features_copy_descriptors(
    const jyppx_ocv_stitching_image_features* features,
    jyppx_ocv_mat* descriptors)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_image_features_copy_descriptors";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (features == nullptr || descriptors == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        features->value.descriptors.copyTo(opencv_csharp_native::mat_value(descriptors));
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

int jyppx_ocv_stitching_compute_image_features(
    int finder_kind,
    const void* finder_handle,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_stitching_image_features* features)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_compute_image_features";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_image_features(api_name, features, "features");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (opencv_csharp_native::mat_value(image).empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }
        if (mask != nullptr)
        {
            const cv::Mat& native_mask = opencv_csharp_native::mat_value(mask);
            if (!native_mask.empty() &&
                (native_mask.type() != CV_8UC1 || native_mask.size() != opencv_csharp_native::mat_value(image).size()))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "mask");
            }
        }
        cv::Ptr<cv::Feature2D> finder;
        status = get_feature_finder(api_name, finder_kind, finder_handle, finder);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        features->value = cv::detail::ImageFeatures();
        cv::detail::computeImageFeatures(
            finder,
            opencv_csharp_native::mat_value(image),
            features->value,
            mask == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mask)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)finder_kind;
        (void)finder_handle;
        (void)image;
        (void)mask;
        (void)features;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_compute_image_features_batch(
    int finder_kind,
    const void* finder_handle,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    jyppx_ocv_stitching_image_features* const* features,
    int feature_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_compute_image_features_batch";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_optional_masks(api_name, masks, mask_count, image_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_count <= 0 || feature_count != image_count || features == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "collections");
        }
        for (int i = 0; i < image_count; ++i)
        {
            if (features[i] == nullptr || opencv_csharp_native::mat_value(images[i]).empty())
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "collections");
            }
            for (int j = 0; j < i; ++j)
            {
                if (features[i] == features[j])
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "features");
                }
            }
            if (mask_count > 0)
            {
                const cv::Mat& native_mask = opencv_csharp_native::mat_value(masks[i]);
                if (!native_mask.empty() &&
                    (native_mask.type() != CV_8UC1 || native_mask.size() != opencv_csharp_native::mat_value(images[i]).size()))
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "masks");
                }
            }
        }
        cv::Ptr<cv::Feature2D> finder;
        status = get_feature_finder(api_name, finder_kind, finder_handle, finder);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        for (int i = 0; i < feature_count; ++i)
        {
            features[i]->value = cv::detail::ImageFeatures();
        }
        std::vector<cv::detail::ImageFeatures> native_features;
        const std::vector<cv::Mat> native_images = to_mat_vector(images, image_count);
        if (mask_count == 0)
        {
            cv::detail::computeImageFeatures(finder, native_images, native_features);
        }
        else
        {
            cv::detail::computeImageFeatures(finder, native_images, native_features, to_mat_vector(masks, mask_count));
        }
        if (native_features.size() != static_cast<size_t>(feature_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "features");
        }
        for (int i = 0; i < feature_count; ++i)
        {
            features[i]->value = native_features[static_cast<size_t>(i)];
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)finder_kind;
        (void)finder_handle;
        (void)images;
        (void)image_count;
        (void)masks;
        (void)mask_count;
        (void)features;
        (void)feature_count;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_matches_info_create(jyppx_ocv_stitching_matches_info** matches_info)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_matches_info_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matches_info == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matches_info");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *matches_info = nullptr;
        auto* created = new (std::nothrow) jyppx_ocv_stitching_matches_info();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        *matches_info = created;
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

void jyppx_ocv_stitching_matches_info_release_handle(jyppx_ocv_stitching_matches_info* matches_info)
{
    delete matches_info;
}

int jyppx_ocv_stitching_matches_info_get_metadata(
    const jyppx_ocv_stitching_matches_info* matches_info,
    int* source_image_index,
    int* destination_image_index,
    int* number_of_inliers,
    double* confidence)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_matches_info_get_metadata";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matches_info == nullptr || source_image_index == nullptr || destination_image_index == nullptr ||
            number_of_inliers == nullptr || confidence == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *source_image_index = matches_info->value.src_img_idx;
        *destination_image_index = matches_info->value.dst_img_idx;
        *number_of_inliers = matches_info->value.num_inliers;
        *confidence = matches_info->value.confidence;
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

int jyppx_ocv_stitching_matches_info_copy_homography(
    const jyppx_ocv_stitching_matches_info* matches_info,
    jyppx_ocv_mat* homography)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_matches_info_copy_homography";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matches_info == nullptr || homography == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        matches_info->value.H.copyTo(opencv_csharp_native::mat_value(homography));
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

int jyppx_ocv_stitching_matches_info_get_matches_count(
    const jyppx_ocv_stitching_matches_info* matches_info,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_matches_info_get_matches_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matches_info == nullptr || match_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (matches_info->value.matches.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matches");
        }
        *match_count = static_cast<int>(matches_info->value.matches.size());
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

int jyppx_ocv_stitching_matches_info_get_matches_fill(
    const jyppx_ocv_stitching_matches_info* matches_info,
    jyppx_ocv_dmatch* matches,
    int match_capacity,
    int* match_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_matches_info_get_matches_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matches_info == nullptr || match_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        const size_t required = matches_info->value.matches.size();
        if (required > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matches");
        }
        *match_count = static_cast<int>(required);
        if (match_capacity < 0 || static_cast<size_t>(match_capacity) < required ||
            (required > 0 && matches == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matches");
        }
        for (size_t i = 0; i < required; ++i)
        {
            matches[i] = from_cv_dmatch(matches_info->value.matches[i]);
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)matches;
        (void)match_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_matches_info_get_inliers_count(
    const jyppx_ocv_stitching_matches_info* matches_info,
    int* inlier_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_matches_info_get_inliers_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matches_info == nullptr || inlier_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (matches_info->value.inliers_mask.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "inliers");
        }
        *inlier_count = static_cast<int>(matches_info->value.inliers_mask.size());
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

int jyppx_ocv_stitching_matches_info_get_inliers_fill(
    const jyppx_ocv_stitching_matches_info* matches_info,
    unsigned char* inliers,
    int inlier_capacity,
    int* inlier_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_matches_info_get_inliers_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matches_info == nullptr || inlier_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        const size_t required = matches_info->value.inliers_mask.size();
        if (required > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "inliers");
        }
        *inlier_count = static_cast<int>(required);
        if (inlier_capacity < 0 || static_cast<size_t>(inlier_capacity) < required ||
            (required > 0 && inliers == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "inliers");
        }
        if (required > 0)
        {
            std::memcpy(inliers, matches_info->value.inliers_mask.data(), required);
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)inliers;
        (void)inlier_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_features_matcher_create_best_of_two_nearest(
    int try_gpu,
    float match_confidence,
    int number_of_matches_threshold1,
    int number_of_matches_threshold2,
    double matches_confidence_threshold,
    jyppx_ocv_stitching_features_matcher** matcher)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_features_matcher_create_best_of_two_nearest";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *matcher = nullptr;
        int status = validate_matcher_options(
            api_name, try_gpu, match_confidence, number_of_matches_threshold1,
            number_of_matches_threshold2, matches_confidence_threshold);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_features_matcher_handle(
            api_name,
            cv::makePtr<cv::detail::BestOf2NearestMatcher>(
                try_gpu != 0, match_confidence, number_of_matches_threshold1,
                number_of_matches_threshold2, matches_confidence_threshold),
            matcher);
#else
        (void)try_gpu;
        (void)match_confidence;
        (void)number_of_matches_threshold1;
        (void)number_of_matches_threshold2;
        (void)matches_confidence_threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_features_matcher_factory_best_of_two_nearest(
    int try_gpu,
    float match_confidence,
    int number_of_matches_threshold1,
    int number_of_matches_threshold2,
    double matches_confidence_threshold,
    jyppx_ocv_stitching_features_matcher** matcher)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_features_matcher_factory_best_of_two_nearest";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *matcher = nullptr;
        int status = validate_matcher_options(
            api_name, try_gpu, match_confidence, number_of_matches_threshold1,
            number_of_matches_threshold2, matches_confidence_threshold);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_features_matcher_handle(
            api_name,
            cv::detail::BestOf2NearestMatcher::create(
                try_gpu != 0, match_confidence, number_of_matches_threshold1,
                number_of_matches_threshold2, matches_confidence_threshold),
            matcher);
#else
        (void)try_gpu;
        (void)match_confidence;
        (void)number_of_matches_threshold1;
        (void)number_of_matches_threshold2;
        (void)matches_confidence_threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_features_matcher_create_range(
    int range_width,
    int try_gpu,
    float match_confidence,
    int number_of_matches_threshold1,
    int number_of_matches_threshold2,
    jyppx_ocv_stitching_features_matcher** matcher)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_features_matcher_create_range";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *matcher = nullptr;
        int status = validate_matcher_options(
            api_name, try_gpu, match_confidence, number_of_matches_threshold1,
            number_of_matches_threshold2, 3.0);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (range_width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "range_width");
        }
        return create_features_matcher_handle(
            api_name,
            cv::makePtr<cv::detail::BestOf2NearestRangeMatcher>(
                range_width, try_gpu != 0, match_confidence,
                number_of_matches_threshold1, number_of_matches_threshold2),
            matcher);
#else
        (void)range_width;
        (void)try_gpu;
        (void)match_confidence;
        (void)number_of_matches_threshold1;
        (void)number_of_matches_threshold2;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_features_matcher_create_affine(
    int full_affine,
    int try_gpu,
    float match_confidence,
    int number_of_matches_threshold1,
    jyppx_ocv_stitching_features_matcher** matcher)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_features_matcher_create_affine";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "matcher");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *matcher = nullptr;
        int status = validate_bool_int(api_name, full_affine, "full_affine");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_matcher_options(
            api_name, try_gpu, match_confidence, number_of_matches_threshold1,
            number_of_matches_threshold1, 3.0);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_features_matcher_handle(
            api_name,
            cv::makePtr<cv::detail::AffineBestOf2NearestMatcher>(
                full_affine != 0, try_gpu != 0, match_confidence, number_of_matches_threshold1),
            matcher);
#else
        (void)full_affine;
        (void)try_gpu;
        (void)match_confidence;
        (void)number_of_matches_threshold1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_stitching_features_matcher_release_handle(jyppx_ocv_stitching_features_matcher* matcher)
{
    delete matcher;
}

int jyppx_ocv_stitching_features_matcher_match_pair(
    jyppx_ocv_stitching_features_matcher* matcher,
    const jyppx_ocv_stitching_image_features* first,
    const jyppx_ocv_stitching_image_features* second,
    jyppx_ocv_stitching_matches_info* matches_info)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_features_matcher_match_pair";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_features_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_image_features(api_name, first, "first");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_image_features(api_name, second, "second");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_matches_info(api_name, matches_info, "matches_info");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        matches_info->value = cv::detail::MatchesInfo();
        status = validate_feature_pair(api_name, first->value, second->value);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        cv::detail::MatchesInfo result;
        (*matcher->value)(first->value, second->value, result);
        matches_info->value = result;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)matcher;
        (void)first;
        (void)second;
        (void)matches_info;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_features_matcher_match_batch(
    jyppx_ocv_stitching_features_matcher* matcher,
    const jyppx_ocv_stitching_image_features* const* features,
    int feature_count,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_stitching_matches_info* const* pairwise_matches,
    int pairwise_match_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_features_matcher_match_batch";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_features_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (feature_count <= 0 || features == nullptr || pairwise_matches == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "collections");
        }
        const std::int64_t required64 = static_cast<std::int64_t>(feature_count) * feature_count;
        if (required64 > std::numeric_limits<int>::max() || pairwise_match_count != static_cast<int>(required64))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pairwise_matches");
        }
        for (int i = 0; i < feature_count; ++i)
        {
            if (features[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "features");
            }
            status = validate_feature_descriptors(api_name, features[i]->value, "features");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
        for (int i = 0; i < pairwise_match_count; ++i)
        {
            if (pairwise_matches[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "pairwise_matches");
            }
            for (int j = 0; j < i; ++j)
            {
                if (pairwise_matches[i] == pairwise_matches[j])
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "pairwise_matches");
                }
            }
        }

        cv::UMat native_mask;
        if (mask != nullptr)
        {
            const cv::Mat& native_mask_mat = opencv_csharp_native::mat_value(mask);
            if (native_mask_mat.type() != CV_8UC1 || native_mask_mat.rows != feature_count ||
                native_mask_mat.cols != feature_count)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "mask");
            }
            native_mask = native_mask_mat.getUMat(cv::ACCESS_READ);
        }

        for (int i = 0; i < pairwise_match_count; ++i)
        {
            pairwise_matches[i]->value = cv::detail::MatchesInfo();
        }
        std::vector<cv::detail::ImageFeatures> native_features;
        native_features.reserve(static_cast<size_t>(feature_count));
        for (int i = 0; i < feature_count; ++i)
        {
            native_features.push_back(features[i]->value);
        }
        std::vector<cv::detail::MatchesInfo> native_matches;
        (*matcher->value)(native_features, native_matches, native_mask);
        if (native_matches.size() != static_cast<size_t>(pairwise_match_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pairwise_matches");
        }
        for (int i = 0; i < pairwise_match_count; ++i)
        {
            pairwise_matches[i]->value = native_matches[static_cast<size_t>(i)];
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)matcher;
        (void)features;
        (void)feature_count;
        (void)mask;
        (void)pairwise_matches;
        (void)pairwise_match_count;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_features_matcher_is_thread_safe(
    const jyppx_ocv_stitching_features_matcher* matcher,
    int* is_thread_safe)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_features_matcher_is_thread_safe";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (matcher == nullptr || is_thread_safe == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_features_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *is_thread_safe = matcher->value->isThreadSafe() ? 1 : 0;
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

int jyppx_ocv_stitching_features_matcher_collect_garbage(
    jyppx_ocv_stitching_features_matcher* matcher)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_features_matcher_collect_garbage";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_features_matcher(api_name, matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        matcher->value->collectGarbage();
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

int jyppx_ocv_stitching_camera_params_get_k(
    double focal,
    double aspect,
    double ppx,
    double ppy,
    jyppx_ocv_mat* camera_matrix)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_camera_params_get_k";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (!std::isfinite(focal) || !std::isfinite(aspect) || !std::isfinite(ppx) ||
            !std::isfinite(ppy) || camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
        cv::detail::CameraParams camera;
        camera.focal = focal;
        camera.aspect = aspect;
        camera.ppx = ppx;
        camera.ppy = ppy;
        camera.K().copyTo(opencv_csharp_native::mat_value(camera_matrix));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)focal;
        (void)aspect;
        (void)ppx;
        (void)ppy;
        (void)camera_matrix;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_focals_from_homography(
    const jyppx_ocv_mat* homography,
    double* focal_x,
    double* focal_y,
    int* focal_x_estimated,
    int* focal_y_estimated)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_focals_from_homography";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (homography == nullptr || focal_x == nullptr || focal_y == nullptr ||
            focal_x_estimated == nullptr || focal_y_estimated == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
        const cv::Mat& value = opencv_csharp_native::mat_value(homography);
        if (value.dims != 2 || value.rows != 3 || value.cols != 3 || value.type() != CV_64FC1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "homography");
        }
        double native_focal_x = 0.0;
        double native_focal_y = 0.0;
        bool native_focal_x_estimated = false;
        bool native_focal_y_estimated = false;
        cv::detail::focalsFromHomography(
            value,
            native_focal_x,
            native_focal_y,
            native_focal_x_estimated,
            native_focal_y_estimated);
        *focal_x = native_focal_x;
        *focal_y = native_focal_y;
        *focal_x_estimated = native_focal_x_estimated ? 1 : 0;
        *focal_y_estimated = native_focal_y_estimated ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)homography;
        (void)focal_x;
        (void)focal_y;
        (void)focal_x_estimated;
        (void)focal_y_estimated;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_calibrate_rotating_camera(
    const jyppx_ocv_mat* const* homographies,
    int homography_count,
    jyppx_ocv_mat* camera_matrix,
    int* calibrated)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_calibrate_rotating_camera";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (homography_count <= 0 || camera_matrix == nullptr || calibrated == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
        int status = validate_mat_array(api_name, homographies, homography_count, "homographies");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::Mat> values;
        values.reserve(static_cast<size_t>(homography_count));
        for (int i = 0; i < homography_count; ++i)
        {
            const cv::Mat& value = opencv_csharp_native::mat_value(homographies[i]);
            if (value.dims != 2 || value.rows != 3 || value.cols != 3 || value.type() != CV_64FC1)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "homographies");
            }
            values.push_back(value);
        }
        cv::Mat result;
        const bool success = cv::detail::calibrateRotatingCamera(values, result);
        if (success)
        {
            result.copyTo(opencv_csharp_native::mat_value(camera_matrix));
        }
        *calibrated = success ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)homographies;
        (void)homography_count;
        (void)camera_matrix;
        (void)calibrated;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_estimator_create_homography(
    int focal_lengths_estimated,
    jyppx_ocv_stitching_estimator** estimator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_estimator_create_homography";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (estimator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "estimator");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_bool_int(api_name, focal_lengths_estimated, "focal_lengths_estimated");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *estimator = nullptr;
        return create_estimator_handle(
            api_name,
            cv::makePtr<cv::detail::HomographyBasedEstimator>(focal_lengths_estimated != 0),
            cv::Ptr<cv::detail::BundleAdjusterBase>(),
            focal_lengths_estimated != 0,
            estimator);
#else
        (void)focal_lengths_estimated;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_estimator_create_affine(jyppx_ocv_stitching_estimator** estimator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_estimator_create_affine";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (estimator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "estimator");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *estimator = nullptr;
        return create_estimator_handle(
            api_name,
            cv::makePtr<cv::detail::AffineBasedEstimator>(),
            cv::Ptr<cv::detail::BundleAdjusterBase>(),
            false,
            estimator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_estimator_create_no_bundle_adjuster(jyppx_ocv_stitching_estimator** estimator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_estimator_create_no_bundle_adjuster";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (estimator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "estimator");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *estimator = nullptr;
        cv::Ptr<cv::detail::NoBundleAdjuster> value = cv::makePtr<cv::detail::NoBundleAdjuster>();
        return create_estimator_handle(api_name, value, value, true, estimator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_estimator_create_bundle_adjuster_reproj(jyppx_ocv_stitching_estimator** estimator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_reproj";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (estimator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "estimator");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *estimator = nullptr;
        cv::Ptr<cv::detail::BundleAdjusterReproj> value = cv::makePtr<cv::detail::BundleAdjusterReproj>();
        return create_estimator_handle(api_name, value, value, true, estimator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_estimator_create_bundle_adjuster_ray(jyppx_ocv_stitching_estimator** estimator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_ray";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (estimator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "estimator");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *estimator = nullptr;
        cv::Ptr<cv::detail::BundleAdjusterRay> value = cv::makePtr<cv::detail::BundleAdjusterRay>();
        return create_estimator_handle(api_name, value, value, true, estimator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_estimator_create_bundle_adjuster_affine(jyppx_ocv_stitching_estimator** estimator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_affine";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (estimator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "estimator");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *estimator = nullptr;
        cv::Ptr<cv::detail::BundleAdjusterAffine> value = cv::makePtr<cv::detail::BundleAdjusterAffine>();
        return create_estimator_handle(api_name, value, value, true, estimator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_estimator_create_bundle_adjuster_affine_partial(jyppx_ocv_stitching_estimator** estimator)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_estimator_create_bundle_adjuster_affine_partial";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (estimator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "estimator");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *estimator = nullptr;
        cv::Ptr<cv::detail::BundleAdjusterAffinePartial> value = cv::makePtr<cv::detail::BundleAdjusterAffinePartial>();
        return create_estimator_handle(api_name, value, value, true, estimator);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_stitching_estimator_release_handle(jyppx_ocv_stitching_estimator* estimator)
{
    delete estimator;
}

int jyppx_ocv_stitching_estimator_apply(
    jyppx_ocv_stitching_estimator* estimator,
    const jyppx_ocv_stitching_image_features* const* features,
    int feature_count,
    const jyppx_ocv_stitching_matches_info* const* pairwise_matches,
    int pairwise_match_count,
    const jyppx_ocv_stitching_camera_params* initial_cameras,
    int initial_camera_count,
    jyppx_ocv_stitching_camera_params* cameras,
    int camera_capacity,
    int* succeeded)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_estimator_apply";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_estimator(api_name, estimator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_feature_match_collections(
            api_name, features, feature_count, pairwise_matches, pairwise_match_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (cameras == nullptr || camera_capacity != feature_count || succeeded == nullptr ||
            (initial_camera_count != 0 && initial_camera_count != feature_count) ||
            (initial_camera_count > 0 && initial_cameras == nullptr) ||
            (estimator->requires_initial_cameras && initial_camera_count != feature_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "cameras");
        }
        for (int i = 0; i < camera_capacity; ++i)
        {
            if (cameras[i].r != nullptr || cameras[i].t != nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "cameras");
            }
        }

        std::vector<cv::detail::ImageFeatures> native_features;
        native_features.reserve(static_cast<size_t>(feature_count));
        for (int i = 0; i < feature_count; ++i) { native_features.push_back(features[i]->value); }
        std::vector<cv::detail::MatchesInfo> native_matches;
        native_matches.reserve(static_cast<size_t>(pairwise_match_count));
        for (int i = 0; i < pairwise_match_count; ++i) { native_matches.push_back(pairwise_matches[i]->value); }
        std::vector<cv::detail::CameraParams> native_cameras;
        native_cameras.reserve(static_cast<size_t>(feature_count));
        for (int i = 0; i < initial_camera_count; ++i)
        {
            cv::detail::CameraParams camera;
            status = camera_from_abi(api_name, initial_cameras[i], camera);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            native_cameras.push_back(camera);
        }

        const bool success = (*estimator->value)(native_features, native_matches, native_cameras);
        if (native_cameras.size() != static_cast<size_t>(feature_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_results");
        }
        std::vector<jyppx_ocv_stitching_camera_params> native_results(
            static_cast<size_t>(feature_count), jyppx_ocv_stitching_camera_params{});
        try
        {
            for (int i = 0; i < feature_count; ++i)
            {
                status = copy_camera_params(api_name, native_cameras[static_cast<size_t>(i)], &native_results[static_cast<size_t>(i)]);
                if (status != OPENCV_CSHARP_STATUS_OK)
                {
                    release_camera_params(native_results);
                    return status;
                }
            }
        }
        catch (...)
        {
            release_camera_params(native_results);
            throw;
        }
        for (int i = 0; i < feature_count; ++i)
        {
            cameras[i] = native_results[static_cast<size_t>(i)];
            native_results[static_cast<size_t>(i)].r = nullptr;
            native_results[static_cast<size_t>(i)].t = nullptr;
        }
        *succeeded = success ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)estimator;
        (void)features;
        (void)feature_count;
        (void)pairwise_matches;
        (void)pairwise_match_count;
        (void)initial_cameras;
        (void)initial_camera_count;
        (void)cameras;
        (void)camera_capacity;
        (void)succeeded;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_bundle_adjuster_copy_refinement_mask(
    const jyppx_ocv_stitching_estimator* estimator,
    jyppx_ocv_mat* refinement_mask)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_bundle_adjuster_copy_refinement_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_bundle_adjuster(api_name, estimator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (refinement_mask == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "refinement_mask");
        }
        estimator->bundle_adjuster->refinementMask().copyTo(opencv_csharp_native::mat_value(refinement_mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)estimator;
        (void)refinement_mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_bundle_adjuster_set_refinement_mask(
    jyppx_ocv_stitching_estimator* estimator,
    const jyppx_ocv_mat* refinement_mask)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_bundle_adjuster_set_refinement_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_bundle_adjuster(api_name, estimator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (refinement_mask == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "refinement_mask");
        }
        const cv::Mat& value = opencv_csharp_native::mat_value(refinement_mask);
        if (value.dims != 2 || value.rows != 3 || value.cols != 3 || value.type() != CV_8UC1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "refinement_mask");
        }
        estimator->bundle_adjuster->setRefinementMask(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)estimator;
        (void)refinement_mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_bundle_adjuster_get_confidence_threshold(
    const jyppx_ocv_stitching_estimator* estimator,
    double* confidence_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_bundle_adjuster_get_confidence_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_bundle_adjuster(api_name, estimator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (confidence_threshold == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "confidence_threshold");
        }
        *confidence_threshold = estimator->bundle_adjuster->confThresh();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)estimator;
        (void)confidence_threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_bundle_adjuster_set_confidence_threshold(
    jyppx_ocv_stitching_estimator* estimator,
    double confidence_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_bundle_adjuster_set_confidence_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_bundle_adjuster(api_name, estimator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!std::isfinite(confidence_threshold))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "confidence_threshold");
        }
        estimator->bundle_adjuster->setConfThresh(confidence_threshold);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)estimator;
        (void)confidence_threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_bundle_adjuster_get_term_criteria(
    const jyppx_ocv_stitching_estimator* estimator,
    int* criteria_type,
    int* max_count,
    double* epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_bundle_adjuster_get_term_criteria";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_bundle_adjuster(api_name, estimator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (criteria_type == nullptr || max_count == nullptr || epsilon == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "term_criteria");
        }
        const cv::TermCriteria value = estimator->bundle_adjuster->termCriteria();
        *criteria_type = value.type;
        *max_count = value.maxCount;
        *epsilon = value.epsilon;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)estimator;
        (void)criteria_type;
        (void)max_count;
        (void)epsilon;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_bundle_adjuster_set_term_criteria(
    jyppx_ocv_stitching_estimator* estimator,
    int criteria_type,
    int max_count,
    double epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_bundle_adjuster_set_term_criteria";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_bundle_adjuster(api_name, estimator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_term_criteria_values(api_name, criteria_type, max_count, epsilon);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        estimator->bundle_adjuster->setTermCriteria(cv::TermCriteria(criteria_type, max_count, epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)estimator;
        (void)criteria_type;
        (void)max_count;
        (void)epsilon;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_wave_correct(
    jyppx_ocv_mat* const* rotation_matrices,
    int rotation_matrix_count,
    int correction_kind)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_wave_correct";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_mutable_mat_array(
            api_name, rotation_matrices, rotation_matrix_count, "rotation_matrices");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (correction_kind < cv::detail::WAVE_CORRECT_HORIZ ||
            correction_kind > cv::detail::WAVE_CORRECT_AUTO)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "correction_kind");
        }
        std::vector<cv::Mat> values;
        values.reserve(static_cast<size_t>(rotation_matrix_count));
        for (int i = 0; i < rotation_matrix_count; ++i)
        {
            const cv::Mat& value = opencv_csharp_native::mat_value(rotation_matrices[i]);
            if (value.dims != 2 || value.rows != 3 || value.cols != 3 || value.type() != CV_32FC1)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "rotation_matrices");
            }
            for (int j = 0; j < i; ++j)
            {
                if (rotation_matrices[i] == rotation_matrices[j])
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "rotation_matrices");
                }
            }
            values.push_back(value.clone());
        }
        cv::detail::waveCorrect(values, static_cast<cv::detail::WaveCorrectKind>(correction_kind));
        for (int i = 0; i < rotation_matrix_count; ++i)
        {
            opencv_csharp_native::mat_value(rotation_matrices[i]) = values[static_cast<size_t>(i)];
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rotation_matrices;
        (void)rotation_matrix_count;
        (void)correction_kind;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_matches_graph_as_string(
    const unsigned char* path_buffer,
    int path_byte_count,
    const int* path_offsets,
    int path_count,
    int path_offset_count,
    const jyppx_ocv_stitching_matches_info* const* pairwise_matches,
    int pairwise_match_count,
    float confidence_threshold,
    jyppx_ocv_core_utf8_result** result)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_matches_graph_as_string";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "result");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_packed_paths(
            api_name, path_buffer, path_byte_count, path_offsets, path_count, path_offset_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        const std::int64_t required64 = static_cast<std::int64_t>(path_count) * path_count;
        if (required64 > std::numeric_limits<int>::max() ||
            pairwise_match_count != static_cast<int>(required64) || pairwise_matches == nullptr ||
            !std::isfinite(confidence_threshold))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        }
        std::vector<cv::String> paths;
        paths.reserve(static_cast<size_t>(path_count));
        for (int i = 0; i < path_count; ++i)
        {
            const int start = path_offsets[i];
            const int length = path_offsets[i + 1] - start;
            const char* value = length == 0
                ? ""
                : reinterpret_cast<const char*>(path_buffer + start);
            paths.emplace_back(value, static_cast<size_t>(length));
        }
        std::vector<cv::detail::MatchesInfo> matches;
        matches.reserve(static_cast<size_t>(pairwise_match_count));
        for (int i = 0; i < pairwise_match_count; ++i)
        {
            if (pairwise_matches[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "pairwise_matches");
            }
            matches.push_back(pairwise_matches[i]->value);
        }
        const cv::String value = cv::detail::matchesGraphAsString(paths, matches, confidence_threshold);
        *result = nullptr;
        return opencv_csharp_native::make_core_utf8_result(api_name, value, result);
#else
        (void)path_buffer;
        (void)path_byte_count;
        (void)path_offsets;
        (void)path_count;
        (void)path_offset_count;
        (void)pairwise_matches;
        (void)pairwise_match_count;
        (void)confidence_threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_leave_biggest_component(
    const jyppx_ocv_stitching_image_features* const* features,
    int feature_count,
    const jyppx_ocv_stitching_matches_info* const* pairwise_matches,
    int pairwise_match_count,
    float confidence_threshold,
    jyppx_ocv_stitching_image_features* const* component_features,
    int component_feature_capacity,
    jyppx_ocv_stitching_matches_info* const* component_matches,
    int component_match_capacity,
    int* original_indices,
    int original_index_capacity,
    int* component_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_leave_biggest_component";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        int status = validate_feature_match_collections(
            api_name, features, feature_count, pairwise_matches, pairwise_match_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!std::isfinite(confidence_threshold) || component_feature_capacity != feature_count ||
            component_match_capacity != pairwise_match_count || original_index_capacity < feature_count ||
            original_indices == nullptr || component_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "outputs");
        }
        status = validate_output_handle_collections(
            api_name, component_features, component_feature_capacity,
            component_matches, component_match_capacity);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        for (int i = 0; i < component_feature_capacity; ++i)
        {
            for (int j = 0; j < feature_count; ++j)
            {
                if (component_features[i] == features[j])
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "component_features");
                }
            }
        }
        for (int i = 0; i < component_match_capacity; ++i)
        {
            for (int j = 0; j < pairwise_match_count; ++j)
            {
                if (component_matches[i] == pairwise_matches[j])
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "component_matches");
                }
            }
        }

        std::vector<cv::detail::ImageFeatures> native_features;
        native_features.reserve(static_cast<size_t>(feature_count));
        for (int i = 0; i < feature_count; ++i) { native_features.push_back(features[i]->value); }
        std::vector<cv::detail::MatchesInfo> native_matches;
        native_matches.reserve(static_cast<size_t>(pairwise_match_count));
        for (int i = 0; i < pairwise_match_count; ++i) { native_matches.push_back(pairwise_matches[i]->value); }
        const std::vector<int> indices = cv::detail::leaveBiggestComponent(
            native_features, native_matches, confidence_threshold);
        if (indices.empty() || indices.size() > static_cast<size_t>(feature_count) ||
            native_features.size() != indices.size() ||
            native_matches.size() != indices.size() * indices.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "component_results");
        }
        const int selected = static_cast<int>(indices.size());
        for (int i = 0; i < selected; ++i)
        {
            component_features[i]->value = native_features[static_cast<size_t>(i)];
            original_indices[i] = indices[static_cast<size_t>(i)];
        }
        const int selected_match_count = selected * selected;
        for (int i = 0; i < selected_match_count; ++i)
        {
            component_matches[i]->value = native_matches[static_cast<size_t>(i)];
        }
        *component_count = selected;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)features;
        (void)feature_count;
        (void)pairwise_matches;
        (void)pairwise_match_count;
        (void)confidence_threshold;
        (void)component_features;
        (void)component_feature_capacity;
        (void)component_matches;
        (void)component_match_capacity;
        (void)original_indices;
        (void)original_index_capacity;
        (void)component_count;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitching_seam_finder_create_default(
    int type, jyppx_ocv_stitching_seam_finder** seam_finder)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_seam_finder_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (seam_finder == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "seam_finder");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *seam_finder = nullptr;
        std::unique_ptr<jyppx_ocv_stitching_seam_finder> created(new (std::nothrow) jyppx_ocv_stitching_seam_finder());
        if (!created) return opencv_csharp_native::set_out_of_memory(api_name);
        created->value = cv::detail::SeamFinder::createDefault(type);
        *seam_finder = created.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)type; return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_seam_finder_create_dp(
    const unsigned char* cost_utf8, int cost_byte_count,
    jyppx_ocv_stitching_seam_finder** seam_finder)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_seam_finder_create_dp";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (seam_finder == nullptr || cost_utf8 == nullptr || cost_byte_count <= 0 ||
            std::memchr(cost_utf8, 0, static_cast<size_t>(cost_byte_count)) != nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "cost");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *seam_finder = nullptr;
        std::unique_ptr<jyppx_ocv_stitching_seam_finder> created(new (std::nothrow) jyppx_ocv_stitching_seam_finder());
        if (!created) return opencv_csharp_native::set_out_of_memory(api_name);
        created->value = cv::makePtr<cv::detail::DpSeamFinder>(cv::String(reinterpret_cast<const char*>(cost_utf8), static_cast<size_t>(cost_byte_count)));
        *seam_finder = created.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_seam_finder_create_graph_cut(
    const unsigned char* cost_utf8, int cost_byte_count, float terminal_cost,
    float bad_region_penalty, jyppx_ocv_stitching_seam_finder** seam_finder)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_seam_finder_create_graph_cut";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (seam_finder == nullptr || cost_utf8 == nullptr || cost_byte_count <= 0 ||
            std::memchr(cost_utf8, 0, static_cast<size_t>(cost_byte_count)) != nullptr ||
            !std::isfinite(terminal_cost) || !std::isfinite(bad_region_penalty))
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *seam_finder = nullptr;
        std::unique_ptr<jyppx_ocv_stitching_seam_finder> created(new (std::nothrow) jyppx_ocv_stitching_seam_finder());
        if (!created) return opencv_csharp_native::set_out_of_memory(api_name);
        created->value = cv::makePtr<cv::detail::GraphCutSeamFinder>(
            cv::String(reinterpret_cast<const char*>(cost_utf8), static_cast<size_t>(cost_byte_count)),
            terminal_cost, bad_region_penalty);
        *seam_finder = created.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

void jyppx_ocv_stitching_seam_finder_release_handle(jyppx_ocv_stitching_seam_finder* seam_finder)
{
    delete seam_finder;
}

int jyppx_ocv_stitching_seam_finder_set_dp_cost(
    jyppx_ocv_stitching_seam_finder* seam_finder, const unsigned char* cost_utf8, int cost_byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_seam_finder_set_dp_cost";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (seam_finder == nullptr || cost_utf8 == nullptr || cost_byte_count <= 0 ||
            std::memchr(cost_utf8, 0, static_cast<size_t>(cost_byte_count)) != nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "cost");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        auto* dp = dynamic_cast<cv::detail::DpSeamFinder*>(seam_finder->value.get());
        if (dp == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "seam_finder");
        dp->setCostFunction(cv::String(reinterpret_cast<const char*>(cost_utf8), static_cast<size_t>(cost_byte_count)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_seam_finder_find(
    jyppx_ocv_stitching_seam_finder* seam_finder, const jyppx_ocv_mat* const* images, int image_count,
    const int* corner_x, const int* corner_y, int corner_count, jyppx_ocv_mat* const* masks, int mask_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_seam_finder_find";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (seam_finder == nullptr || image_count <= 0 || corner_count != image_count || mask_count != image_count ||
            images == nullptr || masks == nullptr || corner_x == nullptr || corner_y == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "collections");
        for (int i = 0; i < image_count; ++i)
            if (images[i] == nullptr || masks[i] == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "collections");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::UMat> src;
        std::vector<cv::Mat> mask_copies;
        std::vector<cv::UMat> mask_umat;
        std::vector<cv::Point> corners;
        src.reserve(static_cast<size_t>(image_count)); mask_copies.resize(static_cast<size_t>(image_count));
        mask_umat.reserve(static_cast<size_t>(image_count)); corners.reserve(static_cast<size_t>(image_count));
        for (int i = 0; i < image_count; ++i)
        {
            const cv::Mat& image = opencv_csharp_native::mat_value(images[i]);
            const cv::Mat& mask = opencv_csharp_native::mat_value(masks[i]);
            if (image.empty() || mask.empty() || mask.type() != CV_8UC1 || mask.size() != image.size())
                return opencv_csharp_native::set_invalid_argument(api_name, "masks");
            src.push_back(image.getUMat(cv::ACCESS_READ));
            mask.copyTo(mask_copies[static_cast<size_t>(i)]);
            mask_umat.push_back(mask_copies[static_cast<size_t>(i)].getUMat(cv::ACCESS_RW));
            corners.emplace_back(corner_x[i], corner_y[i]);
        }
        seam_finder->value->find(src, corners, mask_umat);
        for (int i = 0; i < image_count; ++i)
            mask_umat[static_cast<size_t>(i)].copyTo(opencv_csharp_native::mat_value(masks[i]));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_timelapser_create_default(int type, jyppx_ocv_stitching_timelapser** timelapser)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_timelapser_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (timelapser == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "timelapser");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *timelapser = nullptr;
        std::unique_ptr<jyppx_ocv_stitching_timelapser> created(new (std::nothrow) jyppx_ocv_stitching_timelapser());
        if (!created) return opencv_csharp_native::set_out_of_memory(api_name);
        created->value = cv::detail::Timelapser::createDefault(type);
        *timelapser = created.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)type; return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

void jyppx_ocv_stitching_timelapser_release_handle(jyppx_ocv_stitching_timelapser* timelapser)
{
    delete timelapser;
}

int jyppx_ocv_stitching_timelapser_initialize(
    jyppx_ocv_stitching_timelapser* timelapser, const int* corner_x, const int* corner_y, int corner_count,
    const int* widths, const int* heights, int size_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_timelapser_initialize";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (timelapser == nullptr || corner_count <= 0 || size_count != corner_count || corner_x == nullptr || corner_y == nullptr || widths == nullptr || heights == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "collections");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Point> corners; std::vector<cv::Size> sizes;
        corners.reserve(static_cast<size_t>(corner_count)); sizes.reserve(static_cast<size_t>(corner_count));
        for (int i = 0; i < corner_count; ++i)
        {
            if (widths[i] <= 0 || heights[i] <= 0) return opencv_csharp_native::set_invalid_argument(api_name, "sizes");
            corners.emplace_back(corner_x[i], corner_y[i]); sizes.emplace_back(widths[i], heights[i]);
        }
        timelapser->value->initialize(corners, sizes);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_timelapser_process(
    jyppx_ocv_stitching_timelapser* timelapser, const jyppx_ocv_mat* image, const jyppx_ocv_mat* mask,
    int top_left_x, int top_left_y)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_timelapser_process";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (timelapser == nullptr || image == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        const cv::Mat& value = opencv_csharp_native::mat_value(image);
        if (value.empty() || value.type() != CV_16SC3) return opencv_csharp_native::set_invalid_argument(api_name, "image");
        if (mask != nullptr && opencv_csharp_native::mat_value(mask).empty()) return opencv_csharp_native::set_invalid_argument(api_name, "mask");
        timelapser->value->process(value, mask == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mask)), cv::Point(top_left_x, top_left_y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask; (void)top_left_x; (void)top_left_y; return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_timelapser_get_dst(const jyppx_ocv_stitching_timelapser* timelapser, jyppx_ocv_mat* destination)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_timelapser_get_dst";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (timelapser == nullptr || destination == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        timelapser->value->getDst().copyTo(opencv_csharp_native::mat_value(destination));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_overlap_roi(
    int first_x, int first_y, int first_width, int first_height,
    int second_x, int second_y, int second_width, int second_height,
    jyppx_ocv_stitching_rect* roi, int* overlaps)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_overlap_roi";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (roi == nullptr || overlaps == nullptr || first_width <= 0 || first_height <= 0 || second_width <= 0 || second_height <= 0)
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
        const std::int64_t first_right = static_cast<std::int64_t>(first_x) + first_width;
        const std::int64_t first_bottom = static_cast<std::int64_t>(first_y) + first_height;
        const std::int64_t second_right = static_cast<std::int64_t>(second_x) + second_width;
        const std::int64_t second_bottom = static_cast<std::int64_t>(second_y) + second_height;
        if (first_right > std::numeric_limits<int>::max() || first_bottom > std::numeric_limits<int>::max() ||
            second_right > std::numeric_limits<int>::max() || second_bottom > std::numeric_limits<int>::max())
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        cv::Rect value;
        const bool result = cv::detail::overlapRoi(cv::Point(first_x, first_y), cv::Point(second_x, second_y),
            cv::Size(first_width, first_height), cv::Size(second_width, second_height), value);
        roi->x = value.x; roi->y = value.y; roi->width = value.width; roi->height = value.height; *overlaps = result ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)first_x; (void)first_y; (void)second_x; (void)second_y; return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_result_roi_sizes(
    const int* corner_x, const int* corner_y, int corner_count, const int* widths, const int* heights, int size_count,
    jyppx_ocv_stitching_rect* roi)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_result_roi_sizes";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (roi == nullptr || corner_count <= 0 || size_count != corner_count || corner_x == nullptr || corner_y == nullptr || widths == nullptr || heights == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "collections");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Point> corners; std::vector<cv::Size> sizes;
        corners.reserve(static_cast<size_t>(corner_count)); sizes.reserve(static_cast<size_t>(corner_count));
        for (int i = 0; i < corner_count; ++i)
        {
            if (widths[i] <= 0 || heights[i] <= 0) return opencv_csharp_native::set_invalid_argument(api_name, "sizes");
            corners.emplace_back(corner_x[i], corner_y[i]); sizes.emplace_back(widths[i], heights[i]);
        }
        const cv::Rect value = cv::detail::resultRoi(corners, sizes);
        roi->x = value.x; roi->y = value.y; roi->width = value.width; roi->height = value.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_result_roi_images(
    const int* corner_x, const int* corner_y, int corner_count, const jyppx_ocv_mat* const* images, int image_count,
    jyppx_ocv_stitching_rect* roi)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_result_roi_images";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (roi == nullptr || corner_count <= 0 || image_count != corner_count || corner_x == nullptr || corner_y == nullptr || images == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "collections");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Point> corners; std::vector<cv::UMat> values;
        corners.reserve(static_cast<size_t>(corner_count)); values.reserve(static_cast<size_t>(corner_count));
        for (int i = 0; i < corner_count; ++i)
        {
            if (images[i] == nullptr || opencv_csharp_native::mat_value(images[i]).empty()) return opencv_csharp_native::set_invalid_argument(api_name, "images");
            corners.emplace_back(corner_x[i], corner_y[i]); values.push_back(opencv_csharp_native::mat_value(images[i]).getUMat(cv::ACCESS_READ));
        }
        const cv::Rect value = cv::detail::resultRoi(corners, values);
        roi->x = value.x; roi->y = value.y; roi->width = value.width; roi->height = value.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_result_roi_intersection(
    const int* corner_x, const int* corner_y, int corner_count, const int* widths, const int* heights, int size_count,
    jyppx_ocv_stitching_rect* roi)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_result_roi_intersection";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (roi == nullptr || corner_count <= 0 || size_count != corner_count || corner_x == nullptr || corner_y == nullptr || widths == nullptr || heights == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "collections");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Point> corners; std::vector<cv::Size> sizes;
        corners.reserve(static_cast<size_t>(corner_count)); sizes.reserve(static_cast<size_t>(corner_count));
        for (int i = 0; i < corner_count; ++i)
        {
            if (widths[i] <= 0 || heights[i] <= 0) return opencv_csharp_native::set_invalid_argument(api_name, "sizes");
            corners.emplace_back(corner_x[i], corner_y[i]); sizes.emplace_back(widths[i], heights[i]);
        }
        const cv::Rect value = cv::detail::resultRoiIntersection(corners, sizes);
        roi->x = value.x; roi->y = value.y; roi->width = value.width; roi->height = value.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_result_tl(const int* corner_x, const int* corner_y, int corner_count, jyppx_ocv_stitching_point* point)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_result_tl";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (point == nullptr || corner_count <= 0 || corner_x == nullptr || corner_y == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "collections");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Point> corners; corners.reserve(static_cast<size_t>(corner_count));
        for (int i = 0; i < corner_count; ++i) corners.emplace_back(corner_x[i], corner_y[i]);
        const cv::Point value = cv::detail::resultTl(corners); point->x = value.x; point->y = value.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_select_random_subset(int count, int size, int* subset, int subset_capacity, int* subset_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_select_random_subset";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (count < 0 || size <= 0 || count > size || subset_count == nullptr || subset_capacity < count || (count > 0 && subset == nullptr))
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<int> values; cv::detail::selectRandomSubset(count, size, values);
        if (values.size() != static_cast<size_t>(count))
        {
            // OpenCV 5.0.0's signed modulo can overshoot; preserve its intended exact-count contract.
            values.clear();
            int remaining = count;
            for (int i = 0; i < size && remaining > 0; ++i)
            {
                if (cv::theRNG().uniform(0, size - i) < remaining)
                {
                    values.push_back(i);
                    --remaining;
                }
            }
        }
        if (values.size() != static_cast<size_t>(count)) return opencv_csharp_native::set_invalid_argument(api_name, "subset");
        for (int i = 0; i < count; ++i) subset[i] = values[static_cast<size_t>(i)];
        *subset_count = count; return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_log_level(int* level)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_log_level";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (level == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "level");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *level = cv::detail::stitchingLogLevel(); return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_spherical_projector_create(
    float scale, const jyppx_ocv_mat* camera_matrix, const jyppx_ocv_mat* rotation_matrix,
    const jyppx_ocv_mat* translation, jyppx_ocv_stitching_spherical_projector** projector)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_spherical_projector_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (projector == nullptr || camera_matrix == nullptr || rotation_matrix == nullptr || !std::isfinite(scale) || scale <= 0.0f)
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *projector = nullptr;
        const cv::Mat& K = opencv_csharp_native::mat_value(camera_matrix); const cv::Mat& R = opencv_csharp_native::mat_value(rotation_matrix);
        if (K.dims != 2 || K.rows != 3 || K.cols != 3 || K.type() != CV_32FC1 || R.dims != 2 || R.rows != 3 || R.cols != 3 || R.type() != CV_32FC1)
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_parameters");
        cv::Mat T = cv::Mat::zeros(3, 1, CV_32FC1);
        if (translation != nullptr)
        {
            const cv::Mat& source = opencv_csharp_native::mat_value(translation);
            if (source.type() != CV_32FC1 || !((source.rows == 3 && source.cols == 1) || (source.rows == 1 && source.cols == 3)))
                return opencv_csharp_native::set_invalid_argument(api_name, "translation");
            source.copyTo(T);
            if (source.rows == 1) T = T.reshape(0, 3);
        }
        std::unique_ptr<jyppx_ocv_stitching_spherical_projector> created(new (std::nothrow) jyppx_ocv_stitching_spherical_projector());
        if (!created) return opencv_csharp_native::set_out_of_memory(api_name);
        created->value.scale = scale; created->value.setCameraParams(K, R, T); created->configured = true;
        *projector = created.release(); return OPENCV_CSHARP_STATUS_OK;
#else
        (void)translation; return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

void jyppx_ocv_stitching_spherical_projector_release_handle(jyppx_ocv_stitching_spherical_projector* projector)
{
    delete projector;
}

int jyppx_ocv_stitching_spherical_projector_map_forward(
    const jyppx_ocv_stitching_spherical_projector* projector, float x, float y, float* u, float* v)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_spherical_projector_map_forward";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (projector == nullptr || u == nullptr || v == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (!projector->configured) return opencv_csharp_native::set_invalid_argument(api_name, "projector");
        projector->value.mapForward(x, y, *u, *v); return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_stitching_spherical_projector_map_backward(
    const jyppx_ocv_stitching_spherical_projector* projector, float u, float v, float* x, float* y)
{
    constexpr const char* api_name = "jyppx_ocv_stitching_spherical_projector_map_backward";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (projector == nullptr || x == nullptr || y == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "arguments");
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        if (!projector->configured) return opencv_csharp_native::set_invalid_argument(api_name, "projector");
        projector->value.mapBackward(u, v, *x, *y); return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

