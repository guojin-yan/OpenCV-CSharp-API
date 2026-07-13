#include "open_cv_sharp/intensity_transform/intensity_transform.h"

#include "../core/mat_handle.h"
#include "../error_state.h"

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_src_dst(const char* api_name, const jyppx_ocv_mat* src, const jyppx_ocv_mat* dst)
    {
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return validate_mat(api_name, dst, "dst");
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_INTENSITY_TRANSFORM)
#include <opencv2/intensity_transform.hpp>
#endif

int jyppx_ocv_intensity_transform_log(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_intensity_transform_log";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_src_dst(api_name, src, dst);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_INTENSITY_TRANSFORM)
        cv::intensity_transform::logTransform(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_intensity_transform_gamma_correction(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, float gamma)
{
    constexpr const char* api_name = "jyppx_ocv_intensity_transform_gamma_correction";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_src_dst(api_name, src, dst);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_INTENSITY_TRANSFORM)
        cv::intensity_transform::gammaCorrection(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), gamma);
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

int jyppx_ocv_intensity_transform_autoscaling(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_intensity_transform_autoscaling";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_src_dst(api_name, src, dst);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_INTENSITY_TRANSFORM)
        cv::intensity_transform::autoscaling(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_intensity_transform_contrast_stretching(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int r1,
    int s1,
    int r2,
    int s2)
{
    constexpr const char* api_name = "jyppx_ocv_intensity_transform_contrast_stretching";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_src_dst(api_name, src, dst);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_INTENSITY_TRANSFORM)
        cv::intensity_transform::contrastStretching(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), r1, s1, r2, s2);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)r1; (void)s1; (void)r2; (void)s2;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_intensity_transform_bimef(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float mu,
    float a,
    float b)
{
    constexpr const char* api_name = "jyppx_ocv_intensity_transform_bimef";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_src_dst(api_name, src, dst);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_INTENSITY_TRANSFORM)
        cv::intensity_transform::BIMEF(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), mu, a, b);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mu; (void)a; (void)b;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_intensity_transform_bimef_with_k(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float k,
    float mu,
    float a,
    float b)
{
    constexpr const char* api_name = "jyppx_ocv_intensity_transform_bimef_with_k";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_src_dst(api_name, src, dst);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_INTENSITY_TRANSFORM)
        cv::intensity_transform::BIMEF(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), k, mu, a, b);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)k; (void)mu; (void)a; (void)b;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}
