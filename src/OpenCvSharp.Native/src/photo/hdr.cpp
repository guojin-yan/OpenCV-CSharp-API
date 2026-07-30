#include "open_cv_sharp/photo/photo.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "photo_handles.h"

#include <new>
#include <stdexcept>
#include <vector>

#if !defined(OPENCV_CSHARP_HAS_OPENCV)
namespace cv
{
    class AlignMTB;
    class CalibrateDebevec;
    class CalibrateRobertson;
    class MergeMertens;
}
#endif

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

    template <typename T>
    int validate_pointer(const char* api_name, const T* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_input_mats(
        const char* api_name,
        const jyppx_ocv_mat* const* values,
        int value_count,
        const char* argument_name)
    {
        if (value_count <= 0 || values == nullptr)
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

    int validate_output_mats(
        const char* api_name,
        jyppx_ocv_mat* const* values,
        int value_count,
        const char* argument_name)
    {
        if (value_count <= 0 || values == nullptr)
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

    template <typename THandle, typename TValue>
    int create_handle(const char* api_name, const cv::Ptr<TValue>& value, THandle** output)
    {
        int status = validate_pointer(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *output = nullptr;

        THandle* created = new (std::nothrow) THandle();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = value;
        *output = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    template <typename TDerived, typename THandle>
    TDerived* as(THandle* handle)
    {
        return dynamic_cast<TDerived*>(handle->value.get());
    }

    template <typename TDerived, typename THandle>
    const TDerived* as(const THandle* handle)
    {
        return dynamic_cast<const TDerived*>(handle->value.get());
    }

    template <typename TDerived, typename THandle>
    int require_type(const char* api_name, THandle* handle, TDerived** typed)
    {
        *typed = as<TDerived>(handle);
        return *typed == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "handle")
            : OPENCV_CSHARP_STATUS_OK;
    }

    template <typename TDerived, typename THandle>
    int require_type(const char* api_name, const THandle* handle, const TDerived** typed)
    {
        *typed = as<TDerived>(handle);
        return *typed == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "handle")
            : OPENCV_CSHARP_STATUS_OK;
    }

    void assign_output_vector(
        const std::vector<cv::Mat>& values,
        jyppx_ocv_mat* const* output,
        int expected_count)
    {
        if (values.size() != static_cast<size_t>(expected_count))
        {
            throw std::runtime_error("OpenCV returned an unexpected aligned-image count.");
        }

        for (int i = 0; i < expected_count; ++i)
        {
            opencv_csharp_native::mat_value(output[i]) = values[static_cast<size_t>(i)];
        }
    }
#endif

    template <typename THandle, typename TValue, typename TNative, typename TGetter>
    int get_typed_value(
        const char* api_name,
        const THandle* handle,
        TValue* output,
        TGetter getter)
    {
        return guarded(api_name, [&]() {
            int status = validate_pointer(api_name, handle, "handle");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            status = validate_pointer(api_name, output, "output");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
            const TNative* typed = nullptr;
            status = require_type<TNative>(api_name, handle, &typed);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            *output = getter(*typed);
            return OPENCV_CSHARP_STATUS_OK;
#else
            (void)getter;
            *output = TValue();
            return opencv_csharp_native::set_not_linked(api_name);
#endif
        });
    }

    template <typename THandle, typename TValue, typename TNative, typename TSetter>
    int set_typed_value(
        const char* api_name,
        THandle* handle,
        TValue value,
        TSetter setter)
    {
        return guarded(api_name, [&]() {
            int status = validate_pointer(api_name, handle, "handle");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
            TNative* typed = nullptr;
            status = require_type<TNative>(api_name, handle, &typed);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            setter(*typed, value);
            return OPENCV_CSHARP_STATUS_OK;
#else
            (void)value;
            (void)setter;
            return opencv_csharp_native::set_not_linked(api_name);
#endif
        });
    }
}

int jyppx_ocv_align_mtb_create(
    int max_bits,
    int exclude_range,
    int cut,
    jyppx_ocv_align_mtb** aligner)
{
    constexpr const char* api_name = "jyppx_ocv_align_mtb_create";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_handle(api_name, cv::createAlignMTB(max_bits, exclude_range, cut != 0), aligner);
#else
        (void)max_bits;
        (void)exclude_range;
        (void)cut;
        if (aligner != nullptr) { *aligner = nullptr; }
        return aligner == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "aligner")
            : opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

void jyppx_ocv_align_mtb_release_handle(jyppx_ocv_align_mtb* aligner)
{
    delete aligner;
}

int jyppx_ocv_align_mtb_process(
    jyppx_ocv_align_mtb* aligner,
    const jyppx_ocv_mat* const* src_images,
    jyppx_ocv_mat* const* dst_images,
    int image_count,
    const jyppx_ocv_mat* times,
    const jyppx_ocv_mat* response,
    int use_extra_inputs)
{
    constexpr const char* api_name = "jyppx_ocv_align_mtb_process";
    return guarded(api_name, [&]() {
        int status = validate_pointer(api_name, aligner, "aligner");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mats(api_name, src_images, image_count, "src_images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mats(api_name, dst_images, image_count, "dst_images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (use_extra_inputs != 0)
        {
            status = validate_pointer(api_name, times, "times");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            status = validate_pointer(api_name, response, "response");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_src = to_mat_vector(src_images, image_count);
        std::vector<cv::Mat> native_dst;
        if (use_extra_inputs != 0)
        {
            aligner->value->process(
                native_src,
                native_dst,
                opencv_csharp_native::mat_value(times),
                opencv_csharp_native::mat_value(response));
        }
        else
        {
            aligner->value->process(native_src, native_dst);
        }
        assign_output_vector(native_dst, dst_images, image_count);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_align_mtb_calculate_shift(
    jyppx_ocv_align_mtb* aligner,
    const jyppx_ocv_mat* img0,
    const jyppx_ocv_mat* img1,
    int* shift_x,
    int* shift_y)
{
    constexpr const char* api_name = "jyppx_ocv_align_mtb_calculate_shift";
    return guarded(api_name, [&]() {
        int status = validate_pointer(api_name, aligner, "aligner");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, img0, "img0");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, img1, "img1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, shift_x, "shift_x");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, shift_y, "shift_y");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Point shift = aligner->value->calculateShift(
            opencv_csharp_native::mat_value(img0),
            opencv_csharp_native::mat_value(img1));
        *shift_x = shift.x;
        *shift_y = shift.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *shift_x = 0;
        *shift_y = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_align_mtb_shift_mat(
    jyppx_ocv_align_mtb* aligner,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int shift_x,
    int shift_y)
{
    constexpr const char* api_name = "jyppx_ocv_align_mtb_shift_mat";
    return guarded(api_name, [&]() {
        int status = validate_pointer(api_name, aligner, "aligner");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        aligner->value->shiftMat(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            cv::Point(shift_x, shift_y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)shift_x;
        (void)shift_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_align_mtb_compute_bitmaps(
    jyppx_ocv_align_mtb* aligner,
    const jyppx_ocv_mat* img,
    jyppx_ocv_mat* threshold_bitmap,
    jyppx_ocv_mat* exclude_bitmap)
{
    constexpr const char* api_name = "jyppx_ocv_align_mtb_compute_bitmaps";
    return guarded(api_name, [&]() {
        int status = validate_pointer(api_name, aligner, "aligner");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, img, "img");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, threshold_bitmap, "threshold_bitmap");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, exclude_bitmap, "exclude_bitmap");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        aligner->value->computeBitmaps(
            opencv_csharp_native::mat_value(img),
            opencv_csharp_native::mat_value(threshold_bitmap),
            opencv_csharp_native::mat_value(exclude_bitmap));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_align_mtb_get_max_bits(const jyppx_ocv_align_mtb* aligner, int* max_bits)
{
    return get_typed_value<jyppx_ocv_align_mtb, int, cv::AlignMTB>(
        "jyppx_ocv_align_mtb_get_max_bits", aligner, max_bits,
        [](const auto& value) { return value.getMaxBits(); });
}

int jyppx_ocv_align_mtb_set_max_bits(jyppx_ocv_align_mtb* aligner, int max_bits)
{
    return set_typed_value<jyppx_ocv_align_mtb, int, cv::AlignMTB>(
        "jyppx_ocv_align_mtb_set_max_bits", aligner, max_bits,
        [](auto& target, int value) { target.setMaxBits(value); });
}

int jyppx_ocv_align_mtb_get_exclude_range(const jyppx_ocv_align_mtb* aligner, int* exclude_range)
{
    return get_typed_value<jyppx_ocv_align_mtb, int, cv::AlignMTB>(
        "jyppx_ocv_align_mtb_get_exclude_range", aligner, exclude_range,
        [](const auto& value) { return value.getExcludeRange(); });
}

int jyppx_ocv_align_mtb_set_exclude_range(jyppx_ocv_align_mtb* aligner, int exclude_range)
{
    return set_typed_value<jyppx_ocv_align_mtb, int, cv::AlignMTB>(
        "jyppx_ocv_align_mtb_set_exclude_range", aligner, exclude_range,
        [](auto& target, int value) { target.setExcludeRange(value); });
}

int jyppx_ocv_align_mtb_get_cut(const jyppx_ocv_align_mtb* aligner, int* cut)
{
    return get_typed_value<jyppx_ocv_align_mtb, int, cv::AlignMTB>(
        "jyppx_ocv_align_mtb_get_cut", aligner, cut,
        [](const auto& value) { return value.getCut() ? 1 : 0; });
}

int jyppx_ocv_align_mtb_set_cut(jyppx_ocv_align_mtb* aligner, int cut)
{
    return set_typed_value<jyppx_ocv_align_mtb, int, cv::AlignMTB>(
        "jyppx_ocv_align_mtb_set_cut", aligner, cut,
        [](auto& target, int value) { target.setCut(value != 0); });
}

int jyppx_ocv_calibrate_debevec_create(
    int samples,
    float lambda,
    int random,
    jyppx_ocv_calibrate_crf** calibrator)
{
    constexpr const char* api_name = "jyppx_ocv_calibrate_debevec_create";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_handle(api_name, cv::createCalibrateDebevec(samples, lambda, random != 0), calibrator);
#else
        (void)samples;
        (void)lambda;
        (void)random;
        if (calibrator != nullptr) { *calibrator = nullptr; }
        return calibrator == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "calibrator")
            : opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_calibrate_robertson_create(
    int max_iter,
    float threshold,
    jyppx_ocv_calibrate_crf** calibrator)
{
    constexpr const char* api_name = "jyppx_ocv_calibrate_robertson_create";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_handle(api_name, cv::createCalibrateRobertson(max_iter, threshold), calibrator);
#else
        (void)max_iter;
        (void)threshold;
        if (calibrator != nullptr) { *calibrator = nullptr; }
        return calibrator == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "calibrator")
            : opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

void jyppx_ocv_calibrate_crf_release_handle(jyppx_ocv_calibrate_crf* calibrator)
{
    delete calibrator;
}

int jyppx_ocv_calibrate_crf_process(
    jyppx_ocv_calibrate_crf* calibrator,
    const jyppx_ocv_mat* const* src_images,
    int image_count,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* times)
{
    constexpr const char* api_name = "jyppx_ocv_calibrate_crf_process";
    return guarded(api_name, [&]() {
        int status = validate_pointer(api_name, calibrator, "calibrator");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mats(api_name, src_images, image_count, "src_images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, times, "times");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        calibrator->value->process(
            to_mat_vector(src_images, image_count),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(times));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_calibrate_debevec_get_lambda(const jyppx_ocv_calibrate_crf* calibrator, float* lambda)
{
    return get_typed_value<jyppx_ocv_calibrate_crf, float, cv::CalibrateDebevec>(
        "jyppx_ocv_calibrate_debevec_get_lambda", calibrator, lambda,
        [](const auto& value) { return value.getLambda(); });
}

int jyppx_ocv_calibrate_debevec_set_lambda(jyppx_ocv_calibrate_crf* calibrator, float lambda)
{
    return set_typed_value<jyppx_ocv_calibrate_crf, float, cv::CalibrateDebevec>(
        "jyppx_ocv_calibrate_debevec_set_lambda", calibrator, lambda,
        [](auto& target, float value) { target.setLambda(value); });
}

int jyppx_ocv_calibrate_debevec_get_samples(const jyppx_ocv_calibrate_crf* calibrator, int* samples)
{
    return get_typed_value<jyppx_ocv_calibrate_crf, int, cv::CalibrateDebevec>(
        "jyppx_ocv_calibrate_debevec_get_samples", calibrator, samples,
        [](const auto& value) { return value.getSamples(); });
}

int jyppx_ocv_calibrate_debevec_set_samples(jyppx_ocv_calibrate_crf* calibrator, int samples)
{
    return set_typed_value<jyppx_ocv_calibrate_crf, int, cv::CalibrateDebevec>(
        "jyppx_ocv_calibrate_debevec_set_samples", calibrator, samples,
        [](auto& target, int value) { target.setSamples(value); });
}

int jyppx_ocv_calibrate_debevec_get_random(const jyppx_ocv_calibrate_crf* calibrator, int* random)
{
    return get_typed_value<jyppx_ocv_calibrate_crf, int, cv::CalibrateDebevec>(
        "jyppx_ocv_calibrate_debevec_get_random", calibrator, random,
        [](const auto& value) { return value.getRandom() ? 1 : 0; });
}

int jyppx_ocv_calibrate_debevec_set_random(jyppx_ocv_calibrate_crf* calibrator, int random)
{
    return set_typed_value<jyppx_ocv_calibrate_crf, int, cv::CalibrateDebevec>(
        "jyppx_ocv_calibrate_debevec_set_random", calibrator, random,
        [](auto& target, int value) { target.setRandom(value != 0); });
}

int jyppx_ocv_calibrate_robertson_get_max_iter(const jyppx_ocv_calibrate_crf* calibrator, int* max_iter)
{
    return get_typed_value<jyppx_ocv_calibrate_crf, int, cv::CalibrateRobertson>(
        "jyppx_ocv_calibrate_robertson_get_max_iter", calibrator, max_iter,
        [](const auto& value) { return value.getMaxIter(); });
}

int jyppx_ocv_calibrate_robertson_set_max_iter(jyppx_ocv_calibrate_crf* calibrator, int max_iter)
{
    return set_typed_value<jyppx_ocv_calibrate_crf, int, cv::CalibrateRobertson>(
        "jyppx_ocv_calibrate_robertson_set_max_iter", calibrator, max_iter,
        [](auto& target, int value) { target.setMaxIter(value); });
}

int jyppx_ocv_calibrate_robertson_get_threshold(const jyppx_ocv_calibrate_crf* calibrator, float* threshold)
{
    return get_typed_value<jyppx_ocv_calibrate_crf, float, cv::CalibrateRobertson>(
        "jyppx_ocv_calibrate_robertson_get_threshold", calibrator, threshold,
        [](const auto& value) { return value.getThreshold(); });
}

int jyppx_ocv_calibrate_robertson_set_threshold(jyppx_ocv_calibrate_crf* calibrator, float threshold)
{
    return set_typed_value<jyppx_ocv_calibrate_crf, float, cv::CalibrateRobertson>(
        "jyppx_ocv_calibrate_robertson_set_threshold", calibrator, threshold,
        [](auto& target, float value) { target.setThreshold(value); });
}

int jyppx_ocv_calibrate_robertson_get_radiance(
    const jyppx_ocv_calibrate_crf* calibrator,
    jyppx_ocv_mat* radiance)
{
    constexpr const char* api_name = "jyppx_ocv_calibrate_robertson_get_radiance";
    return guarded(api_name, [&]() {
        int status = validate_pointer(api_name, calibrator, "calibrator");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, radiance, "radiance");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::CalibrateRobertson* typed = nullptr;
        status = require_type<cv::CalibrateRobertson>(api_name, calibrator, &typed);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        typed->getRadiance().copyTo(opencv_csharp_native::mat_value(radiance));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_merge_debevec_create(jyppx_ocv_merge_exposures** merger)
{
    constexpr const char* api_name = "jyppx_ocv_merge_debevec_create";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_handle(api_name, cv::createMergeDebevec(), merger);
#else
        if (merger != nullptr) { *merger = nullptr; }
        return merger == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "merger")
            : opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_merge_mertens_create(
    float contrast_weight,
    float saturation_weight,
    float exposure_weight,
    jyppx_ocv_merge_exposures** merger)
{
    constexpr const char* api_name = "jyppx_ocv_merge_mertens_create";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_handle(
            api_name,
            cv::createMergeMertens(contrast_weight, saturation_weight, exposure_weight),
            merger);
#else
        (void)contrast_weight;
        (void)saturation_weight;
        (void)exposure_weight;
        if (merger != nullptr) { *merger = nullptr; }
        return merger == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "merger")
            : opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_merge_robertson_create(jyppx_ocv_merge_exposures** merger)
{
    constexpr const char* api_name = "jyppx_ocv_merge_robertson_create";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_handle(api_name, cv::createMergeRobertson(), merger);
#else
        if (merger != nullptr) { *merger = nullptr; }
        return merger == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "merger")
            : opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

void jyppx_ocv_merge_exposures_release_handle(jyppx_ocv_merge_exposures* merger)
{
    delete merger;
}

int jyppx_ocv_merge_exposures_process(
    jyppx_ocv_merge_exposures* merger,
    const jyppx_ocv_mat* const* src_images,
    int image_count,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* times,
    const jyppx_ocv_mat* response,
    int input_mode)
{
    constexpr const char* api_name = "jyppx_ocv_merge_exposures_process";
    return guarded(api_name, [&]() {
        int status = validate_pointer(api_name, merger, "merger");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mats(api_name, src_images, image_count, "src_images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_pointer(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (input_mode < 0 || input_mode > 2)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "input_mode");
        }
        if (input_mode >= 1)
        {
            status = validate_pointer(api_name, times, "times");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
        if (input_mode == 2)
        {
            status = validate_pointer(api_name, response, "response");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_src = to_mat_vector(src_images, image_count);
        cv::Mat& native_dst = opencv_csharp_native::mat_value(dst);
        if (input_mode == 2)
        {
            merger->value->process(
                native_src,
                native_dst,
                opencv_csharp_native::mat_value(times),
                opencv_csharp_native::mat_value(response));
        }
        else if (input_mode == 1)
        {
            if (cv::MergeDebevec* debevec = as<cv::MergeDebevec>(merger))
            {
                debevec->process(native_src, native_dst, opencv_csharp_native::mat_value(times));
            }
            else if (cv::MergeRobertson* robertson = as<cv::MergeRobertson>(merger))
            {
                robertson->process(native_src, native_dst, opencv_csharp_native::mat_value(times));
            }
            else
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "input_mode");
            }
        }
        else
        {
            cv::MergeMertens* mertens = as<cv::MergeMertens>(merger);
            if (mertens == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "input_mode");
            }
            mertens->process(native_src, native_dst);
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_merge_mertens_get_contrast_weight(
    const jyppx_ocv_merge_exposures* merger,
    float* contrast_weight)
{
    return get_typed_value<jyppx_ocv_merge_exposures, float, cv::MergeMertens>(
        "jyppx_ocv_merge_mertens_get_contrast_weight", merger, contrast_weight,
        [](const auto& value) { return value.getContrastWeight(); });
}

int jyppx_ocv_merge_mertens_set_contrast_weight(
    jyppx_ocv_merge_exposures* merger,
    float contrast_weight)
{
    return set_typed_value<jyppx_ocv_merge_exposures, float, cv::MergeMertens>(
        "jyppx_ocv_merge_mertens_set_contrast_weight", merger, contrast_weight,
        [](auto& target, float value) { target.setContrastWeight(value); });
}

int jyppx_ocv_merge_mertens_get_saturation_weight(
    const jyppx_ocv_merge_exposures* merger,
    float* saturation_weight)
{
    return get_typed_value<jyppx_ocv_merge_exposures, float, cv::MergeMertens>(
        "jyppx_ocv_merge_mertens_get_saturation_weight", merger, saturation_weight,
        [](const auto& value) { return value.getSaturationWeight(); });
}

int jyppx_ocv_merge_mertens_set_saturation_weight(
    jyppx_ocv_merge_exposures* merger,
    float saturation_weight)
{
    return set_typed_value<jyppx_ocv_merge_exposures, float, cv::MergeMertens>(
        "jyppx_ocv_merge_mertens_set_saturation_weight", merger, saturation_weight,
        [](auto& target, float value) { target.setSaturationWeight(value); });
}

int jyppx_ocv_merge_mertens_get_exposure_weight(
    const jyppx_ocv_merge_exposures* merger,
    float* exposure_weight)
{
    return get_typed_value<jyppx_ocv_merge_exposures, float, cv::MergeMertens>(
        "jyppx_ocv_merge_mertens_get_exposure_weight", merger, exposure_weight,
        [](const auto& value) { return value.getExposureWeight(); });
}

int jyppx_ocv_merge_mertens_set_exposure_weight(
    jyppx_ocv_merge_exposures* merger,
    float exposure_weight)
{
    return set_typed_value<jyppx_ocv_merge_exposures, float, cv::MergeMertens>(
        "jyppx_ocv_merge_mertens_set_exposure_weight", merger, exposure_weight,
        [](auto& target, float value) { target.setExposureWeight(value); });
}
