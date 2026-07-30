#include "open_cv_sharp/photo/photo.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "photo_handles.h"

#include <cfloat>
#include <climits>
#include <cmath>
#include <memory>

namespace
{
    template <typename TAction>
    int guarded(const char* api_name, TAction action) noexcept
    {
        try
        {
            opencv_csharp_native::clear_last_error();
            return action();
        }
        catch (...)
        {
            return opencv_csharp_native::translate_current_exception(api_name);
        }
    }

    int validate_scissors(
        const char* api_name,
        const jyppx_ocv_intelligent_scissors_mb* scissors)
    {
        if (scissors == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "scissors");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!scissors->value)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "scissors");
        }
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }

    bool finite_non_negative(double value)
    {
        return std::isfinite(value) && value >= 0.0;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    int validate_image(
        const char* api_name,
        const jyppx_ocv_mat* image,
        const char* argument_name,
        bool optional,
        cv::Size* size)
    {
        if (image == nullptr)
        {
            return optional
                ? OPENCV_CSHARP_STATUS_OK
                : opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        const cv::Mat& value = opencv_csharp_native::mat_value(image);
        if (value.empty())
        {
            return optional
                ? OPENCV_CSHARP_STATUS_OK
                : opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }
        if (value.dims != 2 ||
            (value.type() != CV_8UC1 && value.type() != CV_8UC3 && value.type() != CV_8UC4))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }
        if (size != nullptr)
        {
            *size = value.size();
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_feature(
        const char* api_name,
        const jyppx_ocv_mat* feature,
        const char* argument_name,
        int expected_type,
        cv::Size& size)
    {
        if (feature == nullptr)
        {
            return OPENCV_CSHARP_STATUS_OK;
        }
        const cv::Mat& value = opencv_csharp_native::mat_value(feature);
        if (value.empty())
        {
            return OPENCV_CSHARP_STATUS_OK;
        }
        if (value.dims != 2 || value.type() != expected_type)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }
        if (!size.empty() && size != value.size())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }
        size = value.size();
        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::InputArray input_or_no_array(const jyppx_ocv_mat* value)
    {
        return value == nullptr
            ? cv::noArray()
            : cv::InputArray(opencv_csharp_native::mat_value(value));
    }

    int validate_point(
        const char* api_name,
        const jyppx_ocv_intelligent_scissors_mb* scissors,
        int x,
        int y,
        const char* argument_name)
    {
        return x < 0 || y < 0 || x >= scissors->width || y >= scissors->height
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    void invalidate_features(jyppx_ocv_intelligent_scissors_mb* scissors)
    {
        scissors->width = 0;
        scissors->height = 0;
        scissors->features_applied = false;
        scissors->map_built = false;
    }
#endif
}

int jyppx_ocv_photo_intelligent_scissors_create(
    jyppx_ocv_intelligent_scissors_mb** scissors)
{
    constexpr const char* api_name = "jyppx_ocv_photo_intelligent_scissors_create";
    return guarded(api_name, [&]() {
        if (scissors == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "scissors");
        }
        *scissors = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto result = std::make_unique<jyppx_ocv_intelligent_scissors_mb>();
        result->value = std::make_unique<cv::segmentation::IntelligentScissorsMB>();
        *scissors = result.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

void jyppx_ocv_photo_intelligent_scissors_release_handle(
    jyppx_ocv_intelligent_scissors_mb* scissors)
{
    delete scissors;
}

int jyppx_ocv_photo_intelligent_scissors_set_weights(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    float weight_non_edge,
    float weight_gradient_direction,
    float weight_gradient_magnitude)
{
    constexpr const char* api_name = "jyppx_ocv_photo_intelligent_scissors_set_weights";
    return guarded(api_name, [&]() {
        int status = validate_scissors(api_name, scissors);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        const double sum = static_cast<double>(weight_non_edge) +
            static_cast<double>(weight_gradient_direction) +
            static_cast<double>(weight_gradient_magnitude);
        if (!finite_non_negative(weight_non_edge) ||
            !finite_non_negative(weight_gradient_direction) ||
            !finite_non_negative(weight_gradient_magnitude) || sum <= FLT_EPSILON)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "weights");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        scissors->value->setWeights(
            weight_non_edge,
            weight_gradient_direction,
            weight_gradient_magnitude);
        invalidate_features(scissors);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_intelligent_scissors_set_gradient_magnitude_max_limit(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    float gradient_magnitude_threshold_max)
{
    constexpr const char* api_name = "jyppx_ocv_photo_intelligent_scissors_set_gradient_magnitude_max_limit";
    return guarded(api_name, [&]() {
        int status = validate_scissors(api_name, scissors);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!finite_non_negative(gradient_magnitude_threshold_max))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "gradient_magnitude_threshold_max");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        scissors->value->setGradientMagnitudeMaxLimit(gradient_magnitude_threshold_max);
        invalidate_features(scissors);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_intelligent_scissors_set_edge_feature_zero_crossing_parameters(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    float gradient_magnitude_min_value)
{
    constexpr const char* api_name = "jyppx_ocv_photo_intelligent_scissors_set_edge_feature_zero_crossing_parameters";
    return guarded(api_name, [&]() {
        int status = validate_scissors(api_name, scissors);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!finite_non_negative(gradient_magnitude_min_value))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "gradient_magnitude_min_value");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        scissors->value->setEdgeFeatureZeroCrossingParameters(gradient_magnitude_min_value);
        invalidate_features(scissors);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_intelligent_scissors_set_edge_feature_canny_parameters(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    double threshold1,
    double threshold2,
    int aperture_size,
    int l2_gradient)
{
    constexpr const char* api_name = "jyppx_ocv_photo_intelligent_scissors_set_edge_feature_canny_parameters";
    return guarded(api_name, [&]() {
        int status = validate_scissors(api_name, scissors);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!finite_non_negative(threshold1) || !finite_non_negative(threshold2) ||
            (aperture_size != -1 && aperture_size != 3 && aperture_size != 5 && aperture_size != 7))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "canny_parameters");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        scissors->value->setEdgeFeatureCannyParameters(
            threshold1,
            threshold2,
            aperture_size,
            l2_gradient != 0);
        invalidate_features(scissors);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_intelligent_scissors_apply_image(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    const jyppx_ocv_mat* image)
{
    constexpr const char* api_name = "jyppx_ocv_photo_intelligent_scissors_apply_image";
    return guarded(api_name, [&]() {
        int status = validate_scissors(api_name, scissors);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Size size;
        status = validate_image(api_name, image, "image", false, &size);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        scissors->value->applyImage(opencv_csharp_native::mat_value(image));
        scissors->width = size.width;
        scissors->height = size.height;
        scissors->features_applied = true;
        scissors->map_built = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_intelligent_scissors_apply_image_features(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    const jyppx_ocv_mat* non_edge,
    const jyppx_ocv_mat* gradient_direction,
    const jyppx_ocv_mat* gradient_magnitude,
    const jyppx_ocv_mat* image)
{
    constexpr const char* api_name = "jyppx_ocv_photo_intelligent_scissors_apply_image_features";
    return guarded(api_name, [&]() {
        int status = validate_scissors(api_name, scissors);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Size size;
        status = validate_feature(api_name, non_edge, "non_edge", CV_8UC1, size);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_feature(api_name, gradient_direction, "gradient_direction", CV_32FC2, size);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_feature(api_name, gradient_magnitude, "gradient_magnitude", CV_32FC1, size);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::Size image_size;
        status = validate_image(api_name, image, "image", true, &image_size);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!image_size.empty())
        {
            if (!size.empty() && size != image_size)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "image");
            }
            size = image_size;
        }
        if (size.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "features");
        }

        scissors->value->applyImageFeatures(
            input_or_no_array(non_edge),
            input_or_no_array(gradient_direction),
            input_or_no_array(gradient_magnitude),
            input_or_no_array(image));
        scissors->width = size.width;
        scissors->height = size.height;
        scissors->features_applied = true;
        scissors->map_built = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)non_edge;
        (void)gradient_direction;
        (void)gradient_magnitude;
        (void)image;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_intelligent_scissors_build_map(
    jyppx_ocv_intelligent_scissors_mb* scissors,
    int source_x,
    int source_y)
{
    constexpr const char* api_name = "jyppx_ocv_photo_intelligent_scissors_build_map";
    return guarded(api_name, [&]() {
        int status = validate_scissors(api_name, scissors);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!scissors->features_applied)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model_state");
        }
        status = validate_point(api_name, scissors, source_x, source_y, "source_point");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        scissors->value->buildMap(cv::Point(source_x, source_y));
        scissors->map_built = true;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)source_x;
        (void)source_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_intelligent_scissors_get_contour(
    const jyppx_ocv_intelligent_scissors_mb* scissors,
    int target_x,
    int target_y,
    jyppx_ocv_mat* contour,
    int backward)
{
    constexpr const char* api_name = "jyppx_ocv_photo_intelligent_scissors_get_contour";
    return guarded(api_name, [&]() {
        int status = validate_scissors(api_name, scissors);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (contour == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!scissors->map_built)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model_state");
        }
        status = validate_point(api_name, scissors, target_x, target_y, "target_point");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::Mat& output = opencv_csharp_native::mat_value(contour);
        scissors->value->getContour(
            cv::Point(target_x, target_y),
            output,
            backward != 0);
        const size_t point_count = output.total();
        if (point_count > static_cast<size_t>(INT_MAX))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_size");
        }
        if (!output.empty() && (output.rows != static_cast<int>(point_count) || output.cols != 1))
        {
            output = output.reshape(2, static_cast<int>(point_count));
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)target_x;
        (void)target_y;
        (void)backward;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}
