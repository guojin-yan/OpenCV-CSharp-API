#include "open_cv_sharp/phase_unwrapping/phase_unwrapping.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "phase_unwrapping_handles.h"

#include <new>

namespace
{
    constexpr int PHASE_UNWRAPPING_KIND_HISTOGRAM = 1;

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_phase_unwrapping(const char* api_name, const jyppx_ocv_phase_unwrapping* phase_unwrapping)
    {
        return phase_unwrapping == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "phase_unwrapping")
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PHASE_UNWRAPPING)
    cv::InputArray optional_input_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::phase_unwrapping::HistogramPhaseUnwrapping* as_histogram(jyppx_ocv_phase_unwrapping* phase_unwrapping)
    {
        return phase_unwrapping->kind == PHASE_UNWRAPPING_KIND_HISTOGRAM
            ? dynamic_cast<cv::phase_unwrapping::HistogramPhaseUnwrapping*>(phase_unwrapping->value.get())
            : nullptr;
    }
#endif
}

int jyppx_ocv_phase_unwrapping_histogram_create(
    int width,
    int height,
    float hist_thresh,
    int nbr_of_small_bins,
    int nbr_of_large_bins,
    jyppx_ocv_phase_unwrapping** phase_unwrapping)
{
    constexpr const char* api_name = "jyppx_ocv_phase_unwrapping_histogram_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (phase_unwrapping == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "phase_unwrapping");
        }

        *phase_unwrapping = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PHASE_UNWRAPPING)
        cv::phase_unwrapping::HistogramPhaseUnwrapping::Params parameters;
        parameters.width = width;
        parameters.height = height;
        parameters.histThresh = hist_thresh;
        parameters.nbrOfSmallBins = nbr_of_small_bins;
        parameters.nbrOfLargeBins = nbr_of_large_bins;

        jyppx_ocv_phase_unwrapping* created = new (std::nothrow) jyppx_ocv_phase_unwrapping();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::phase_unwrapping::HistogramPhaseUnwrapping::create(parameters);
        created->kind = PHASE_UNWRAPPING_KIND_HISTOGRAM;
        *phase_unwrapping = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width; (void)height; (void)hist_thresh; (void)nbr_of_small_bins; (void)nbr_of_large_bins;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_phase_unwrapping_release(jyppx_ocv_phase_unwrapping* phase_unwrapping)
{
    delete phase_unwrapping;
}

int jyppx_ocv_phase_unwrapping_unwrap_phase_map(
    jyppx_ocv_phase_unwrapping* phase_unwrapping,
    const jyppx_ocv_mat* wrapped_phase_map,
    jyppx_ocv_mat* unwrapped_phase_map,
    const jyppx_ocv_mat* shadow_mask)
{
    constexpr const char* api_name = "jyppx_ocv_phase_unwrapping_unwrap_phase_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_phase_unwrapping(api_name, phase_unwrapping);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, wrapped_phase_map, "wrapped_phase_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, unwrapped_phase_map, "unwrapped_phase_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PHASE_UNWRAPPING)
        phase_unwrapping->value->unwrapPhaseMap(
            opencv_csharp_native::mat_value(wrapped_phase_map),
            opencv_csharp_native::mat_value(unwrapped_phase_map),
            optional_input_array(shadow_mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)shadow_mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_phase_unwrapping_histogram_get_inverse_reliability_map(
    jyppx_ocv_phase_unwrapping* phase_unwrapping,
    jyppx_ocv_mat* reliability_map)
{
    constexpr const char* api_name = "jyppx_ocv_phase_unwrapping_histogram_get_inverse_reliability_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_phase_unwrapping(api_name, phase_unwrapping);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, reliability_map, "reliability_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PHASE_UNWRAPPING)
        cv::phase_unwrapping::HistogramPhaseUnwrapping* typed = as_histogram(phase_unwrapping);
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "phase_unwrapping");
        }

        typed->getInverseReliabilityMap(opencv_csharp_native::mat_value(reliability_map));
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
