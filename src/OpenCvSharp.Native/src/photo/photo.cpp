#include "open_cv_sharp/photo/photo.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "photo_handles.h"

#include <new>
#include <vector>

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_tonemap(const char* api_name, const jyppx_ocv_tonemap* tonemap)
    {
        return tonemap == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "tonemap")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
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

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::vector<float> to_float_vector(const float* values, int value_count)
    {
        return value_count <= 0 ? std::vector<float>() : std::vector<float>(values, values + value_count);
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

    template <typename TTonemap>
    TTonemap* as_tonemap(jyppx_ocv_tonemap* tonemap)
    {
        return dynamic_cast<TTonemap*>(tonemap->value.get());
    }

    template <typename TTonemap>
    const TTonemap* as_tonemap(const jyppx_ocv_tonemap* tonemap)
    {
        return dynamic_cast<const TTonemap*>(tonemap->value.get());
    }

    template <typename TTonemap>
    int require_tonemap_type(const char* api_name, const jyppx_ocv_tonemap* tonemap, const TTonemap** typed)
    {
        *typed = as_tonemap<TTonemap>(tonemap);
        if (*typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tonemap");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    template <typename TTonemap>
    int require_tonemap_type(const char* api_name, jyppx_ocv_tonemap* tonemap, TTonemap** typed)
    {
        *typed = as_tonemap<TTonemap>(tonemap);
        if (*typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tonemap");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_tonemap_handle(const char* api_name, const cv::Ptr<cv::Tonemap>& native, jyppx_ocv_tonemap** tonemap)
    {
        if (tonemap == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tonemap");
        }

        *tonemap = nullptr;
        jyppx_ocv_tonemap* created = new (std::nothrow) jyppx_ocv_tonemap();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *tonemap = created;
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_photo_decolor(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* grayscale,
    jyppx_ocv_mat* color_boost)
{
    constexpr const char* api_name = "jyppx_ocv_photo_decolor";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, grayscale, "grayscale");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, color_boost, "color_boost");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::decolor(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(grayscale), opencv_csharp_native::mat_value(color_boost));
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

int jyppx_ocv_photo_inpaint(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* inpaint_mask,
    jyppx_ocv_mat* dst,
    double inpaint_radius,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_photo_inpaint";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, inpaint_mask, "inpaint_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::inpaint(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(inpaint_mask), opencv_csharp_native::mat_value(dst), inpaint_radius, flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)inpaint_radius;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_fast_nl_means_denoising(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float h,
    int template_window_size,
    int search_window_size)
{
    constexpr const char* api_name = "jyppx_ocv_photo_fast_nl_means_denoising";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::fastNlMeansDenoising(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), h, template_window_size, search_window_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)h;
        (void)template_window_size;
        (void)search_window_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_fast_nl_means_denoising_with_h_array(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const float* h,
    int h_count,
    int template_window_size,
    int search_window_size,
    int norm_type)
{
    constexpr const char* api_name = "jyppx_ocv_photo_fast_nl_means_denoising_with_h_array";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_float_array(api_name, h, h_count, "h");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::fastNlMeansDenoising(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), to_float_vector(h, h_count), template_window_size, search_window_size, norm_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)template_window_size;
        (void)search_window_size;
        (void)norm_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_fast_nl_means_denoising_colored(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float h,
    float h_color,
    int template_window_size,
    int search_window_size)
{
    constexpr const char* api_name = "jyppx_ocv_photo_fast_nl_means_denoising_colored";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::fastNlMeansDenoisingColored(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), h, h_color, template_window_size, search_window_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)h;
        (void)h_color;
        (void)template_window_size;
        (void)search_window_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_fast_nl_means_denoising_multi(
    const jyppx_ocv_mat* const* src_images,
    int image_count,
    jyppx_ocv_mat* dst,
    int img_to_denoise_index,
    int temporal_window_size,
    float h,
    int template_window_size,
    int search_window_size)
{
    constexpr const char* api_name = "jyppx_ocv_photo_fast_nl_means_denoising_multi";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat_array(api_name, src_images, image_count, "src_images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_images = to_mat_vector(src_images, image_count);
        cv::fastNlMeansDenoisingMulti(native_images, opencv_csharp_native::mat_value(dst), img_to_denoise_index, temporal_window_size, h, template_window_size, search_window_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)img_to_denoise_index;
        (void)temporal_window_size;
        (void)h;
        (void)template_window_size;
        (void)search_window_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_fast_nl_means_denoising_multi_with_h_array(
    const jyppx_ocv_mat* const* src_images,
    int image_count,
    jyppx_ocv_mat* dst,
    int img_to_denoise_index,
    int temporal_window_size,
    const float* h,
    int h_count,
    int template_window_size,
    int search_window_size,
    int norm_type)
{
    constexpr const char* api_name = "jyppx_ocv_photo_fast_nl_means_denoising_multi_with_h_array";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat_array(api_name, src_images, image_count, "src_images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_float_array(api_name, h, h_count, "h");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_images = to_mat_vector(src_images, image_count);
        cv::fastNlMeansDenoisingMulti(native_images, opencv_csharp_native::mat_value(dst), img_to_denoise_index, temporal_window_size, to_float_vector(h, h_count), template_window_size, search_window_size, norm_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)img_to_denoise_index;
        (void)temporal_window_size;
        (void)template_window_size;
        (void)search_window_size;
        (void)norm_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_fast_nl_means_denoising_colored_multi(
    const jyppx_ocv_mat* const* src_images,
    int image_count,
    jyppx_ocv_mat* dst,
    int img_to_denoise_index,
    int temporal_window_size,
    float h,
    float h_color,
    int template_window_size,
    int search_window_size)
{
    constexpr const char* api_name = "jyppx_ocv_photo_fast_nl_means_denoising_colored_multi";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat_array(api_name, src_images, image_count, "src_images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_images = to_mat_vector(src_images, image_count);
        cv::fastNlMeansDenoisingColoredMulti(native_images, opencv_csharp_native::mat_value(dst), img_to_denoise_index, temporal_window_size, h, h_color, template_window_size, search_window_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)img_to_denoise_index;
        (void)temporal_window_size;
        (void)h;
        (void)h_color;
        (void)template_window_size;
        (void)search_window_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_seamless_clone(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask,
    int point_x,
    int point_y,
    jyppx_ocv_mat* blend,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_photo_seamless_clone";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, blend, "blend");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::seamlessClone(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), opencv_csharp_native::mat_value(mask), cv::Point(point_x, point_y), opencv_csharp_native::mat_value(blend), flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)point_x;
        (void)point_y;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_color_change(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_mat* dst,
    float red_mul,
    float green_mul,
    float blue_mul)
{
    constexpr const char* api_name = "jyppx_ocv_photo_color_change";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::colorChange(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(mask), opencv_csharp_native::mat_value(dst), red_mul, green_mul, blue_mul);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)red_mul;
        (void)green_mul;
        (void)blue_mul;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_illumination_change(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_mat* dst,
    float alpha,
    float beta)
{
    constexpr const char* api_name = "jyppx_ocv_photo_illumination_change";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::illuminationChange(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(mask), opencv_csharp_native::mat_value(dst), alpha, beta);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)alpha;
        (void)beta;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_texture_flattening(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_mat* dst,
    float low_threshold,
    float high_threshold,
    int kernel_size)
{
    constexpr const char* api_name = "jyppx_ocv_photo_texture_flattening";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::textureFlattening(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(mask), opencv_csharp_native::mat_value(dst), low_threshold, high_threshold, kernel_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)low_threshold;
        (void)high_threshold;
        (void)kernel_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_edge_preserving_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int flags,
    float sigma_s,
    float sigma_r)
{
    constexpr const char* api_name = "jyppx_ocv_photo_edge_preserving_filter";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::edgePreservingFilter(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), flags, sigma_s, sigma_r);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        (void)sigma_s;
        (void)sigma_r;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_detail_enhance(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float sigma_s,
    float sigma_r)
{
    constexpr const char* api_name = "jyppx_ocv_photo_detail_enhance";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::detailEnhance(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), sigma_s, sigma_r);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma_s;
        (void)sigma_r;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_pencil_sketch(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst1,
    jyppx_ocv_mat* dst2,
    float sigma_s,
    float sigma_r,
    float shade_factor)
{
    constexpr const char* api_name = "jyppx_ocv_photo_pencil_sketch";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst1, "dst1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst2, "dst2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::pencilSketch(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst1), opencv_csharp_native::mat_value(dst2), sigma_s, sigma_r, shade_factor);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma_s;
        (void)sigma_r;
        (void)shade_factor;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_photo_stylization(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float sigma_s,
    float sigma_r)
{
    constexpr const char* api_name = "jyppx_ocv_photo_stylization";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::stylization(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), sigma_s, sigma_r);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma_s;
        (void)sigma_r;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_create(float gamma, jyppx_ocv_tonemap** tonemap)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_create";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_tonemap_handle(api_name, cv::createTonemap(gamma), tonemap);
#else
        (void)gamma;
        if (tonemap != nullptr) { *tonemap = nullptr; }
        return tonemap == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "tonemap")
            : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_drago_create(float gamma, float saturation, float bias, jyppx_ocv_tonemap** tonemap)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_drago_create";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_tonemap_handle(api_name, cv::createTonemapDrago(gamma, saturation, bias), tonemap);
#else
        (void)gamma;
        (void)saturation;
        (void)bias;
        if (tonemap != nullptr) { *tonemap = nullptr; }
        return tonemap == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "tonemap")
            : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_reinhard_create(float gamma, float intensity, float light_adapt, float color_adapt, jyppx_ocv_tonemap** tonemap)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_reinhard_create";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_tonemap_handle(api_name, cv::createTonemapReinhard(gamma, intensity, light_adapt, color_adapt), tonemap);
#else
        (void)gamma;
        (void)intensity;
        (void)light_adapt;
        (void)color_adapt;
        if (tonemap != nullptr) { *tonemap = nullptr; }
        return tonemap == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "tonemap")
            : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_mantiuk_create(float gamma, float scale, float saturation, jyppx_ocv_tonemap** tonemap)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_mantiuk_create";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_tonemap_handle(api_name, cv::createTonemapMantiuk(gamma, scale, saturation), tonemap);
#else
        (void)gamma;
        (void)scale;
        (void)saturation;
        if (tonemap != nullptr) { *tonemap = nullptr; }
        return tonemap == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "tonemap")
            : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_tonemap_release_handle(jyppx_ocv_tonemap* tonemap)
{
    delete tonemap;
}

int jyppx_ocv_tonemap_process(jyppx_ocv_tonemap* tonemap, const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_process";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        tonemap->value->process(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_tonemap_get_gamma(const jyppx_ocv_tonemap* tonemap, float* gamma)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_get_gamma";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, gamma, "gamma");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *gamma = tonemap->value->getGamma();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *gamma = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_set_gamma(jyppx_ocv_tonemap* tonemap, float gamma)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_set_gamma";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        tonemap->value->setGamma(gamma);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)gamma;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_drago_get_saturation(const jyppx_ocv_tonemap* tonemap, float* saturation)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_drago_get_saturation";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, saturation, "saturation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::TonemapDrago* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *saturation = typed->getSaturation();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *saturation = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_drago_set_saturation(jyppx_ocv_tonemap* tonemap, float saturation)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_drago_set_saturation";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::TonemapDrago* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        typed->setSaturation(saturation);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)saturation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_drago_get_bias(const jyppx_ocv_tonemap* tonemap, float* bias)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_drago_get_bias";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, bias, "bias");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::TonemapDrago* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *bias = typed->getBias();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *bias = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_drago_set_bias(jyppx_ocv_tonemap* tonemap, float bias)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_drago_set_bias";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::TonemapDrago* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        typed->setBias(bias);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)bias;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_reinhard_get_intensity(const jyppx_ocv_tonemap* tonemap, float* intensity)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_reinhard_get_intensity";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, intensity, "intensity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::TonemapReinhard* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *intensity = typed->getIntensity();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *intensity = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_reinhard_set_intensity(jyppx_ocv_tonemap* tonemap, float intensity)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_reinhard_set_intensity";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::TonemapReinhard* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        typed->setIntensity(intensity);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)intensity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_reinhard_get_light_adaptation(const jyppx_ocv_tonemap* tonemap, float* light_adapt)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_reinhard_get_light_adaptation";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, light_adapt, "light_adapt");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::TonemapReinhard* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *light_adapt = typed->getLightAdaptation();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *light_adapt = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_reinhard_set_light_adaptation(jyppx_ocv_tonemap* tonemap, float light_adapt)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_reinhard_set_light_adaptation";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::TonemapReinhard* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        typed->setLightAdaptation(light_adapt);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)light_adapt;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_reinhard_get_color_adaptation(const jyppx_ocv_tonemap* tonemap, float* color_adapt)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_reinhard_get_color_adaptation";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, color_adapt, "color_adapt");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::TonemapReinhard* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *color_adapt = typed->getColorAdaptation();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *color_adapt = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_reinhard_set_color_adaptation(jyppx_ocv_tonemap* tonemap, float color_adapt)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_reinhard_set_color_adaptation";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::TonemapReinhard* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        typed->setColorAdaptation(color_adapt);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color_adapt;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_mantiuk_get_scale(const jyppx_ocv_tonemap* tonemap, float* scale)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_mantiuk_get_scale";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, scale, "scale");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::TonemapMantiuk* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *scale = typed->getScale();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *scale = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_mantiuk_set_scale(jyppx_ocv_tonemap* tonemap, float scale)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_mantiuk_set_scale";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::TonemapMantiuk* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        typed->setScale(scale);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)scale;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_mantiuk_get_saturation(const jyppx_ocv_tonemap* tonemap, float* saturation)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_mantiuk_get_saturation";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, saturation, "saturation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::TonemapMantiuk* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *saturation = typed->getSaturation();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *saturation = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tonemap_mantiuk_set_saturation(jyppx_ocv_tonemap* tonemap, float saturation)
{
    constexpr const char* api_name = "jyppx_ocv_tonemap_mantiuk_set_saturation";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_tonemap(api_name, tonemap);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::TonemapMantiuk* typed = nullptr;
        status = require_tonemap_type(api_name, tonemap, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        typed->setSaturation(saturation);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)saturation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

