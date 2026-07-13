#include "open_cv_sharp/saliency/saliency.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "saliency_handles.h"

#include <new>
#include <string>

namespace
{
    int validate_saliency(const char* api_name, const jyppx_ocv_saliency_saliency* saliency)
    {
        return saliency == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "saliency")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_static_saliency(const char* api_name, const jyppx_ocv_saliency_static* saliency)
    {
        return saliency == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "saliency")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_spectral_residual(const char* api_name, const jyppx_ocv_saliency_spectral_residual* saliency)
    {
        return saliency == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "saliency")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_motion_bin_wang(const char* api_name, const jyppx_ocv_saliency_motion_bin_wang* saliency)
    {
        return saliency == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "saliency")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_objectness_bing(const char* api_name, const jyppx_ocv_saliency_objectness_bing* saliency)
    {
        return saliency == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "saliency")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
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

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
    std::string safe_string(const char* value)
    {
        return value == nullptr ? std::string() : std::string(value);
    }

    template <typename THandle, typename TConcrete>
    int create_static_handle(const char* api_name, const cv::Ptr<TConcrete>& native, THandle** saliency)
    {
        if (saliency == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "saliency");
        }

        *saliency = nullptr;
        THandle* created = new (std::nothrow) THandle();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = native;
        created->static_value = native;
        created->value = native;
        *saliency = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_objectness_bing_handle(const char* api_name, jyppx_ocv_saliency_objectness_bing** saliency)
    {
        if (saliency == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "saliency");
        }

        *saliency = nullptr;
        jyppx_ocv_saliency_objectness_bing* created = new (std::nothrow) jyppx_ocv_saliency_objectness_bing();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::saliency::ObjectnessBING::create();
        created->value = created->concrete;
        *saliency = created;
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

void jyppx_ocv_saliency_saliency_release_handle(jyppx_ocv_saliency_saliency* saliency)
{
    delete saliency;
}

int jyppx_ocv_saliency_compute_saliency(jyppx_ocv_saliency_saliency* saliency, const jyppx_ocv_mat* image, jyppx_ocv_mat* saliency_map, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_compute_saliency";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_saliency(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, saliency_map, "saliency_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *result = saliency->value->computeSaliency(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(saliency_map)) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_static_compute_binary_map(jyppx_ocv_saliency_static* saliency, const jyppx_ocv_mat* saliency_map, jyppx_ocv_mat* binary_map, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_static_compute_binary_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_static_saliency(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, saliency_map, "saliency_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, binary_map, "binary_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *result = saliency->static_value->computeBinaryMap(opencv_csharp_native::mat_value(saliency_map), opencv_csharp_native::mat_value(binary_map)) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_spectral_residual_create(jyppx_ocv_saliency_spectral_residual** saliency)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_spectral_residual_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        return create_static_handle(api_name, cv::saliency::StaticSaliencySpectralResidual::create(), saliency);
#else
        (void)saliency;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_spectral_residual_get_image_width(const jyppx_ocv_saliency_spectral_residual* saliency, int* width)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_spectral_residual_get_image_width";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_spectral_residual(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, width, "width");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *width = saliency->concrete->getImageWidth();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *width = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_spectral_residual_set_image_width(jyppx_ocv_saliency_spectral_residual* saliency, int width)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_spectral_residual_set_image_width";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_spectral_residual(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->concrete->setImageWidth(width);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_spectral_residual_get_image_height(const jyppx_ocv_saliency_spectral_residual* saliency, int* height)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_spectral_residual_get_image_height";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_spectral_residual(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, height, "height");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *height = saliency->concrete->getImageHeight();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_spectral_residual_set_image_height(jyppx_ocv_saliency_spectral_residual* saliency, int height)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_spectral_residual_set_image_height";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_spectral_residual(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->concrete->setImageHeight(height);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_fine_grained_create(jyppx_ocv_saliency_fine_grained** saliency)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_fine_grained_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        return create_static_handle(api_name, cv::saliency::StaticSaliencyFineGrained::create(), saliency);
#else
        (void)saliency;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_motion_bin_wang_create(jyppx_ocv_saliency_motion_bin_wang** saliency)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_motion_bin_wang_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (saliency == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "saliency");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *saliency = nullptr;
        jyppx_ocv_saliency_motion_bin_wang* created = new (std::nothrow) jyppx_ocv_saliency_motion_bin_wang();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::saliency::MotionSaliencyBinWangApr2014::create();
        created->value = created->concrete;
        *saliency = created;
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

int jyppx_ocv_saliency_motion_bin_wang_set_image_size(jyppx_ocv_saliency_motion_bin_wang* saliency, int width, int height)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_motion_bin_wang_set_image_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_motion_bin_wang(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->concrete->setImagesize(width, height);
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

int jyppx_ocv_saliency_motion_bin_wang_init(jyppx_ocv_saliency_motion_bin_wang* saliency, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_motion_bin_wang_init";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_motion_bin_wang(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *result = saliency->concrete->init() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_motion_bin_wang_get_image_width(const jyppx_ocv_saliency_motion_bin_wang* saliency, int* width)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_motion_bin_wang_get_image_width";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_motion_bin_wang(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, width, "width");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *width = saliency->concrete->getImageWidth();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *width = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_motion_bin_wang_set_image_width(jyppx_ocv_saliency_motion_bin_wang* saliency, int width)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_motion_bin_wang_set_image_width";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_motion_bin_wang(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->concrete->setImageWidth(width);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_motion_bin_wang_get_image_height(const jyppx_ocv_saliency_motion_bin_wang* saliency, int* height)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_motion_bin_wang_get_image_height";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_motion_bin_wang(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, height, "height");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *height = saliency->concrete->getImageHeight();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_motion_bin_wang_set_image_height(jyppx_ocv_saliency_motion_bin_wang* saliency, int height)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_motion_bin_wang_set_image_height";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_motion_bin_wang(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->concrete->setImageHeight(height);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_create(jyppx_ocv_saliency_objectness_bing** saliency)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        return create_objectness_bing_handle(api_name, saliency);
#else
        if (saliency != nullptr) { *saliency = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_set_training_path(jyppx_ocv_saliency_objectness_bing* saliency, const char* training_path)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_set_training_path";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (training_path == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "training_path");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->concrete->setTrainingPath(safe_string(training_path));
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

int jyppx_ocv_saliency_objectness_bing_set_bb_res_dir(jyppx_ocv_saliency_objectness_bing* saliency, const char* results_dir)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_set_bb_res_dir";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (results_dir == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "results_dir");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->concrete->setBBResDir(safe_string(results_dir));
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

int jyppx_ocv_saliency_objectness_bing_get_base(const jyppx_ocv_saliency_objectness_bing* saliency, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_get_base";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *value = saliency->concrete->getBase();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_set_base(jyppx_ocv_saliency_objectness_bing* saliency, double value)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_set_base";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->concrete->setBase(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_get_nss(const jyppx_ocv_saliency_objectness_bing* saliency, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_get_nss";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *value = saliency->concrete->getNSS();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_set_nss(jyppx_ocv_saliency_objectness_bing* saliency, int value)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_set_nss";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->concrete->setNSS(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_get_w(const jyppx_ocv_saliency_objectness_bing* saliency, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_get_w";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *value = saliency->concrete->getW();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_set_w(jyppx_ocv_saliency_objectness_bing* saliency, int value)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_set_w";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->concrete->setW(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_compute(jyppx_ocv_saliency_objectness_bing* saliency, const jyppx_ocv_mat* image, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        saliency->last_boxes.clear();
        saliency->last_values.clear();
        *result = saliency->concrete->computeSaliency(opencv_csharp_native::mat_value(image), saliency->last_boxes) ? 1 : 0;
        saliency->last_values = saliency->concrete->getobjectnessValues();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_get_boxes_count(const jyppx_ocv_saliency_objectness_bing* saliency, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_get_boxes_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *count = static_cast<int>(saliency->last_boxes.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_get_boxes_fill(const jyppx_ocv_saliency_objectness_bing* saliency, int* boxes, int box_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_get_boxes_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (box_capacity < 0 || (box_capacity > 0 && boxes == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "boxes");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        int actual = static_cast<int>(saliency->last_boxes.size());
        int copy = actual < box_capacity ? actual : box_capacity;
        for (int i = 0; i < copy; ++i)
        {
            const cv::Vec4i& box = saliency->last_boxes[static_cast<size_t>(i)];
            const int offset = i * 4;
            boxes[offset] = box[0];
            boxes[offset + 1] = box[1];
            boxes[offset + 2] = box[2];
            boxes[offset + 3] = box[3];
        }

        *count = actual;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_get_objectness_values_count(const jyppx_ocv_saliency_objectness_bing* saliency, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_get_objectness_values_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        *count = static_cast<int>(saliency->last_values.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_saliency_objectness_bing_get_objectness_values_fill(const jyppx_ocv_saliency_objectness_bing* saliency, float* values, int value_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_saliency_objectness_bing_get_objectness_values_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_objectness_bing(api_name, saliency);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (value_capacity < 0 || (value_capacity > 0 && values == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "values");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SALIENCY)
        int actual = static_cast<int>(saliency->last_values.size());
        int copy = actual < value_capacity ? actual : value_capacity;
        for (int i = 0; i < copy; ++i)
        {
            values[i] = saliency->last_values[static_cast<size_t>(i)];
        }

        *count = actual;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

