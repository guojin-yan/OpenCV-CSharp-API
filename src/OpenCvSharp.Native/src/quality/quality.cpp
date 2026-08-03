#include "open_cv_sharp/quality/quality.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "quality_handles.h"

#include <new>

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_quality(const char* api_name, const jyppx_ocv_quality* quality)
    {
        return quality == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "quality")
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

    int validate_string(const char* api_name, const char* value, const char* argument_name)
    {
        if (value == nullptr || value[0] == '\0')
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    void assign_bool(int* destination, bool value)
    {
        if (destination != nullptr)
        {
            *destination = value ? 1 : 0;
        }
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
    int copy_scalar(const char* api_name, const cv::Scalar& scalar, double* values, int capacity)
    {
        if (values == nullptr || capacity < 4)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "scalar_values");
        }

        values[0] = scalar[0];
        values[1] = scalar[1];
        values[2] = scalar[2];
        values[3] = scalar[3];
        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::_OutputArray optional_output_array(jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::_OutputArray() : cv::_OutputArray(opencv_csharp_native::mat_value(mat));
    }

    int create_quality_handle(const char* api_name, const cv::Ptr<cv::quality::QualityBase>& native, jyppx_ocv_quality** quality)
    {
        if (quality == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "quality");
        }

        *quality = nullptr;
        jyppx_ocv_quality* created = new (std::nothrow) jyppx_ocv_quality();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *quality = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    const cv::quality::QualityPSNR* as_psnr(const jyppx_ocv_quality* quality)
    {
        return dynamic_cast<const cv::quality::QualityPSNR*>(quality->value.get());
    }

    cv::quality::QualityPSNR* as_psnr(jyppx_ocv_quality* quality)
    {
        return dynamic_cast<cv::quality::QualityPSNR*>(quality->value.get());
    }
#endif
}

int jyppx_ocv_quality_mse_create(const jyppx_ocv_mat* reference, jyppx_ocv_quality** quality)
{
    constexpr const char* api_name = "jyppx_ocv_quality_mse_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, reference, "reference");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return create_quality_handle(api_name, cv::quality::QualityMSE::create(opencv_csharp_native::mat_value(reference)), quality);
#else
        if (quality != nullptr) { *quality = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_psnr_create(const jyppx_ocv_mat* reference, double max_pixel_value, jyppx_ocv_quality** quality)
{
    constexpr const char* api_name = "jyppx_ocv_quality_psnr_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, reference, "reference");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return create_quality_handle(api_name, cv::quality::QualityPSNR::create(opencv_csharp_native::mat_value(reference), max_pixel_value), quality);
#else
        (void)max_pixel_value;
        if (quality != nullptr) { *quality = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_ssim_create(const jyppx_ocv_mat* reference, jyppx_ocv_quality** quality)
{
    constexpr const char* api_name = "jyppx_ocv_quality_ssim_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, reference, "reference");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return create_quality_handle(api_name, cv::quality::QualitySSIM::create(opencv_csharp_native::mat_value(reference)), quality);
#else
        if (quality != nullptr) { *quality = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_gmsd_create(const jyppx_ocv_mat* reference, jyppx_ocv_quality** quality)
{
    constexpr const char* api_name = "jyppx_ocv_quality_gmsd_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, reference, "reference");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return create_quality_handle(api_name, cv::quality::QualityGMSD::create(opencv_csharp_native::mat_value(reference)), quality);
#else
        if (quality != nullptr) { *quality = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_brisque_create(const char* model_file_path, const char* range_file_path, jyppx_ocv_quality** quality)
{
    constexpr const char* api_name = "jyppx_ocv_quality_brisque_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, model_file_path, "model_file_path");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, range_file_path, "range_file_path");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return create_quality_handle(api_name, cv::quality::QualityBRISQUE::create(model_file_path, range_file_path), quality);
#else
        if (quality != nullptr) { *quality = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_quality_release_handle(jyppx_ocv_quality* quality)
{
    delete quality;
}

int jyppx_ocv_quality_compute(jyppx_ocv_quality* quality, const jyppx_ocv_mat* comparison, double* scalar_values, int scalar_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_quality_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_quality(api_name, quality);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, comparison, "comparison");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return copy_scalar(api_name, quality->value->compute(opencv_csharp_native::mat_value(comparison)), scalar_values, scalar_capacity);
#else
        (void)scalar_values;
        (void)scalar_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_get_quality_map(const jyppx_ocv_quality* quality, jyppx_ocv_mat* quality_map)
{
    constexpr const char* api_name = "jyppx_ocv_quality_get_quality_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_quality(api_name, quality);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, quality_map, "quality_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        quality->value->getQualityMap(opencv_csharp_native::mat_value(quality_map));
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

int jyppx_ocv_quality_clear(jyppx_ocv_quality* quality)
{
    constexpr const char* api_name = "jyppx_ocv_quality_clear";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_quality(api_name, quality);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        quality->value->clear();
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

int jyppx_ocv_quality_empty(const jyppx_ocv_quality* quality, int* empty)
{
    constexpr const char* api_name = "jyppx_ocv_quality_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_quality(api_name, quality);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, empty, "empty");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        assign_bool(empty, quality->value->empty());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *empty = 1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_psnr_get_max_pixel_value(const jyppx_ocv_quality* quality, double* max_pixel_value)
{
    constexpr const char* api_name = "jyppx_ocv_quality_psnr_get_max_pixel_value";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_quality(api_name, quality);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, max_pixel_value, "max_pixel_value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        const cv::quality::QualityPSNR* psnr = as_psnr(quality);
        if (psnr == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "quality");
        }

        *max_pixel_value = psnr->getMaxPixelValue();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *max_pixel_value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_psnr_set_max_pixel_value(jyppx_ocv_quality* quality, double max_pixel_value)
{
    constexpr const char* api_name = "jyppx_ocv_quality_psnr_set_max_pixel_value";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_quality(api_name, quality);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        cv::quality::QualityPSNR* psnr = as_psnr(quality);
        if (psnr == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "quality");
        }

        psnr->setMaxPixelValue(max_pixel_value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)max_pixel_value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_mse_compute_static(
    const jyppx_ocv_mat* reference,
    const jyppx_ocv_mat* comparison,
    jyppx_ocv_mat* quality_map,
    double* scalar_values,
    int scalar_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_quality_mse_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, reference, "reference");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, comparison, "comparison");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return copy_scalar(
            api_name,
            cv::quality::QualityMSE::compute(opencv_csharp_native::mat_value(reference), opencv_csharp_native::mat_value(comparison), optional_output_array(quality_map)),
            scalar_values,
            scalar_capacity);
#else
        (void)quality_map;
        (void)scalar_values;
        (void)scalar_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_psnr_compute_static(
    const jyppx_ocv_mat* reference,
    const jyppx_ocv_mat* comparison,
    jyppx_ocv_mat* quality_map,
    double max_pixel_value,
    double* scalar_values,
    int scalar_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_quality_psnr_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, reference, "reference");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, comparison, "comparison");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return copy_scalar(
            api_name,
            cv::quality::QualityPSNR::compute(opencv_csharp_native::mat_value(reference), opencv_csharp_native::mat_value(comparison), optional_output_array(quality_map), max_pixel_value),
            scalar_values,
            scalar_capacity);
#else
        (void)quality_map;
        (void)max_pixel_value;
        (void)scalar_values;
        (void)scalar_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_ssim_compute_static(
    const jyppx_ocv_mat* reference,
    const jyppx_ocv_mat* comparison,
    jyppx_ocv_mat* quality_map,
    double* scalar_values,
    int scalar_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_quality_ssim_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, reference, "reference");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, comparison, "comparison");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return copy_scalar(
            api_name,
            cv::quality::QualitySSIM::compute(opencv_csharp_native::mat_value(reference), opencv_csharp_native::mat_value(comparison), optional_output_array(quality_map)),
            scalar_values,
            scalar_capacity);
#else
        (void)quality_map;
        (void)scalar_values;
        (void)scalar_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_gmsd_compute_static(
    const jyppx_ocv_mat* reference,
    const jyppx_ocv_mat* comparison,
    jyppx_ocv_mat* quality_map,
    double* scalar_values,
    int scalar_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_quality_gmsd_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, reference, "reference");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, comparison, "comparison");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return copy_scalar(
            api_name,
            cv::quality::QualityGMSD::compute(opencv_csharp_native::mat_value(reference), opencv_csharp_native::mat_value(comparison), optional_output_array(quality_map)),
            scalar_values,
            scalar_capacity);
#else
        (void)quality_map;
        (void)scalar_values;
        (void)scalar_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_brisque_compute_static(
    const jyppx_ocv_mat* image,
    const char* model_file_path,
    const char* range_file_path,
    double* scalar_values,
    int scalar_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_quality_brisque_compute_static";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, model_file_path, "model_file_path");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, range_file_path, "range_file_path");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        return copy_scalar(
            api_name,
            cv::quality::QualityBRISQUE::compute(opencv_csharp_native::mat_value(image), model_file_path, range_file_path),
            scalar_values,
            scalar_capacity);
#else
        (void)scalar_values;
        (void)scalar_capacity;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_quality_brisque_compute_features(const jyppx_ocv_mat* image, jyppx_ocv_mat* features)
{
    constexpr const char* api_name = "jyppx_ocv_quality_brisque_compute_features";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, features, "features");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_QUALITY)
        cv::quality::QualityBRISQUE::computeFeatures(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(features));
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


