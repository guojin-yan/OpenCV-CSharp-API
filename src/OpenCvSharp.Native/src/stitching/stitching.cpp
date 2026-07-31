#include "open_cv_sharp/stitching/stitching.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "stitching_handles.h"

#include <cmath>
#include <cstring>
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

