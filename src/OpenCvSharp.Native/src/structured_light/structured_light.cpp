#include "open_cv_sharp/structured_light/structured_light.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "structured_light_handles.h"

#include <new>
#include <vector>

namespace
{
    constexpr int STRUCTURED_LIGHT_KIND_GRAY_CODE = 1;
    constexpr int STRUCTURED_LIGHT_KIND_SINUSOIDAL = 2;

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_pattern(const char* api_name, const jyppx_ocv_structured_light_pattern* pattern)
    {
        return pattern == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "pattern")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int(const char* api_name, const int* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat_array(const char* api_name, const jyppx_ocv_mat* const* images, int image_count)
    {
        if (image_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_count");
        }

        if (image_count > 0 && images == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "images");
        }

        for (int i = 0; i < image_count; ++i)
        {
            if (images[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "images");
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_marker_array(const char* api_name, const jyppx_ocv_structured_light_point2f* markers, int marker_count)
    {
        if (marker_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "marker_count");
        }

        return marker_count > 0 && markers == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "markers")
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
    cv::structured_light::GrayCodePattern* as_gray_code(jyppx_ocv_structured_light_pattern* pattern)
    {
        return pattern->kind == STRUCTURED_LIGHT_KIND_GRAY_CODE
            ? dynamic_cast<cv::structured_light::GrayCodePattern*>(pattern->value.get())
            : nullptr;
    }

    cv::structured_light::SinusoidalPattern* as_sinusoidal(jyppx_ocv_structured_light_pattern* pattern)
    {
        return pattern->kind == STRUCTURED_LIGHT_KIND_SINUSOIDAL
            ? dynamic_cast<cv::structured_light::SinusoidalPattern*>(pattern->value.get())
            : nullptr;
    }

    cv::InputArray optional_input_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    std::vector<cv::Mat> to_mat_vector(const jyppx_ocv_mat* const* images, int image_count)
    {
        std::vector<cv::Mat> result;
        result.reserve(static_cast<size_t>(image_count));
        for (int i = 0; i < image_count; ++i)
        {
            result.push_back(opencv_csharp_native::mat_value(images[i]));
        }

        return result;
    }

    std::vector<cv::Point2f> to_marker_vector(const jyppx_ocv_structured_light_point2f* markers, int marker_count)
    {
        std::vector<cv::Point2f> result;
        result.reserve(static_cast<size_t>(marker_count));
        for (int i = 0; i < marker_count; ++i)
        {
            result.push_back(cv::Point2f(markers[i].x, markers[i].y));
        }

        return result;
    }

    int create_mat_handle(const char* api_name, const cv::Mat& source, jyppx_ocv_mat** out_mat)
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

        created->value = source;
        *out_mat = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int generate_core(
        const char* api_name,
        jyppx_ocv_structured_light_pattern* pattern,
        std::vector<cv::Mat>& images)
    {
        int status = validate_pattern(api_name, pattern);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        bool generated = pattern->value->generate(images);
        return generated
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, std::string(api_name) + ": OpenCV failed to generate structured light pattern images.");
    }
#endif
}

int jyppx_ocv_structured_light_gray_code_pattern_create(
    int width,
    int height,
    jyppx_ocv_structured_light_pattern** pattern)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_gray_code_pattern_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (pattern == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern");
        }

        *pattern = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        cv::structured_light::GrayCodePattern::Params parameters;
        parameters.width = width;
        parameters.height = height;

        jyppx_ocv_structured_light_pattern* created = new (std::nothrow) jyppx_ocv_structured_light_pattern();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::structured_light::GrayCodePattern::create(parameters);
        created->kind = STRUCTURED_LIGHT_KIND_GRAY_CODE;
        *pattern = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width; (void)height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_structured_light_sinusoidal_pattern_create(
    int width,
    int height,
    int nbr_of_periods,
    float shift_value,
    int method_id,
    int nbr_of_pixels_between_markers,
    int horizontal,
    int set_markers,
    const jyppx_ocv_structured_light_point2f* markers,
    int marker_count,
    jyppx_ocv_structured_light_pattern** pattern)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_sinusoidal_pattern_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (pattern == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern");
        }

        int status = validate_marker_array(api_name, markers, marker_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        *pattern = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        cv::Ptr<cv::structured_light::SinusoidalPattern::Params> parameters = cv::makePtr<cv::structured_light::SinusoidalPattern::Params>();
        parameters->width = width;
        parameters->height = height;
        parameters->nbrOfPeriods = nbr_of_periods;
        parameters->shiftValue = shift_value;
        parameters->methodId = method_id;
        parameters->nbrOfPixelsBetweenMarkers = nbr_of_pixels_between_markers;
        parameters->horizontal = horizontal != 0;
        parameters->setMarkers = set_markers != 0;
        parameters->markersLocation = to_marker_vector(markers, marker_count);

        jyppx_ocv_structured_light_pattern* created = new (std::nothrow) jyppx_ocv_structured_light_pattern();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::structured_light::SinusoidalPattern::create(parameters);
        created->kind = STRUCTURED_LIGHT_KIND_SINUSOIDAL;
        *pattern = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width; (void)height; (void)nbr_of_periods; (void)shift_value; (void)method_id;
        (void)nbr_of_pixels_between_markers; (void)horizontal; (void)set_markers;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_structured_light_pattern_release(jyppx_ocv_structured_light_pattern* pattern)
{
    delete pattern;
}

int jyppx_ocv_structured_light_pattern_generate_count(jyppx_ocv_structured_light_pattern* pattern, int* image_count)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_pattern_generate_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, image_count, "image_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        std::vector<cv::Mat> images;
        status = generate_core(api_name, pattern, images);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *image_count = static_cast<int>(images.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern;
        *image_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_structured_light_pattern_generate_fill(
    jyppx_ocv_structured_light_pattern* pattern,
    jyppx_ocv_mat** images,
    int image_capacity,
    int* image_count)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_pattern_generate_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, image_count, "image_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_capacity < 0 || (image_capacity > 0 && images == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "images");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        std::vector<cv::Mat> native_images;
        status = generate_core(api_name, pattern, native_images);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *image_count = static_cast<int>(native_images.size());
        if (image_capacity < *image_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_capacity");
        }

        int written = 0;
        for (int i = 0; i < *image_count; ++i)
        {
            status = create_mat_handle(api_name, native_images[static_cast<size_t>(i)], &images[i]);
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                for (int j = 0; j < written; ++j)
                {
                    delete images[j];
                    images[j] = nullptr;
                }

                *image_count = 0;
                return status;
            }

            ++written;
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern; (void)images;
        *image_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_structured_light_gray_code_pattern_get_number_of_pattern_images(
    jyppx_ocv_structured_light_pattern* pattern,
    int* image_count)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_gray_code_pattern_get_number_of_pattern_images";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, image_count, "image_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        status = validate_pattern(api_name, pattern);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::structured_light::GrayCodePattern* typed = as_gray_code(pattern);
        if (typed == nullptr)
        {
            *image_count = 0;
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern");
        }

        *image_count = static_cast<int>(typed->getNumberOfPatternImages());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern;
        *image_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_structured_light_gray_code_pattern_set_white_threshold(
    jyppx_ocv_structured_light_pattern* pattern,
    int value)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_gray_code_pattern_set_white_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        int status = validate_pattern(api_name, pattern);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::structured_light::GrayCodePattern* typed = as_gray_code(pattern);
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern");
        }

        typed->setWhiteThreshold(static_cast<size_t>(value));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_structured_light_gray_code_pattern_set_black_threshold(
    jyppx_ocv_structured_light_pattern* pattern,
    int value)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_gray_code_pattern_set_black_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        int status = validate_pattern(api_name, pattern);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::structured_light::GrayCodePattern* typed = as_gray_code(pattern);
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern");
        }

        typed->setBlackThreshold(static_cast<size_t>(value));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_structured_light_gray_code_pattern_get_images_for_shadow_masks(
    jyppx_ocv_structured_light_pattern* pattern,
    jyppx_ocv_mat* black_image,
    jyppx_ocv_mat* white_image)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_gray_code_pattern_get_images_for_shadow_masks";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, black_image, "black_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, white_image, "white_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        status = validate_pattern(api_name, pattern);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::structured_light::GrayCodePattern* typed = as_gray_code(pattern);
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern");
        }

        typed->getImagesForShadowMasks(
            opencv_csharp_native::mat_value(black_image),
            opencv_csharp_native::mat_value(white_image));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_structured_light_gray_code_pattern_get_proj_pixel(
    jyppx_ocv_structured_light_pattern* pattern,
    const jyppx_ocv_mat* const* pattern_images,
    int image_count,
    int x,
    int y,
    int* found,
    int* proj_x,
    int* proj_y)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_gray_code_pattern_get_proj_pixel";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, found, "found");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, proj_x, "proj_x");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, proj_y, "proj_y");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, pattern_images, image_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        status = validate_pattern(api_name, pattern);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::structured_light::GrayCodePattern* typed = as_gray_code(pattern);
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern");
        }

        std::vector<cv::Mat> images = to_mat_vector(pattern_images, image_count);
        cv::Point point;
        bool ok = typed->getProjPixel(images, x, y, point);
        *found = ok ? 1 : 0;
        *proj_x = point.x;
        *proj_y = point.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern; (void)x; (void)y;
        *found = 0;
        *proj_x = 0;
        *proj_y = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_structured_light_sinusoidal_pattern_compute_phase_map(
    jyppx_ocv_structured_light_pattern* pattern,
    const jyppx_ocv_mat* const* pattern_images,
    int image_count,
    jyppx_ocv_mat* wrapped_phase_map,
    jyppx_ocv_mat* shadow_mask,
    const jyppx_ocv_mat* fundamental)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_sinusoidal_pattern_compute_phase_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat_array(api_name, pattern_images, image_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, wrapped_phase_map, "wrapped_phase_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, shadow_mask, "shadow_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        status = validate_pattern(api_name, pattern);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::structured_light::SinusoidalPattern* typed = as_sinusoidal(pattern);
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern");
        }

        std::vector<cv::Mat> images = to_mat_vector(pattern_images, image_count);
        typed->computePhaseMap(
            images,
            opencv_csharp_native::mat_value(wrapped_phase_map),
            opencv_csharp_native::mat_value(shadow_mask),
            optional_input_array(fundamental));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern; (void)fundamental;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_structured_light_sinusoidal_pattern_unwrap_phase_map(
    jyppx_ocv_structured_light_pattern* pattern,
    const jyppx_ocv_mat* wrapped_phase_map,
    jyppx_ocv_mat* unwrapped_phase_map,
    int cam_width,
    int cam_height,
    const jyppx_ocv_mat* shadow_mask)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_sinusoidal_pattern_unwrap_phase_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, wrapped_phase_map, "wrapped_phase_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, unwrapped_phase_map, "unwrapped_phase_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        status = validate_pattern(api_name, pattern);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::structured_light::SinusoidalPattern* typed = as_sinusoidal(pattern);
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern");
        }

        typed->unwrapPhaseMap(
            opencv_csharp_native::mat_value(wrapped_phase_map),
            opencv_csharp_native::mat_value(unwrapped_phase_map),
            cv::Size(cam_width, cam_height),
            optional_input_array(shadow_mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern; (void)cam_width; (void)cam_height; (void)shadow_mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_structured_light_sinusoidal_pattern_compute_data_modulation_term(
    jyppx_ocv_structured_light_pattern* pattern,
    const jyppx_ocv_mat* const* pattern_images,
    int image_count,
    jyppx_ocv_mat* data_modulation_term,
    const jyppx_ocv_mat* shadow_mask)
{
    constexpr const char* api_name = "jyppx_ocv_structured_light_sinusoidal_pattern_compute_data_modulation_term";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat_array(api_name, pattern_images, image_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, data_modulation_term, "data_modulation_term");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, shadow_mask, "shadow_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT)
        status = validate_pattern(api_name, pattern);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::structured_light::SinusoidalPattern* typed = as_sinusoidal(pattern);
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern");
        }

        std::vector<cv::Mat> images = to_mat_vector(pattern_images, image_count);
        typed->computeDataModulationTerm(
            images,
            opencv_csharp_native::mat_value(data_modulation_term),
            opencv_csharp_native::mat_value(shadow_mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}


