#include "open_cv_sharp/stitching/stitching.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
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

