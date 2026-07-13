#include "open_cv_sharp/bgsegm/bgsegm.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "bgsegm_handles.h"

#include <new>

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_subtractor(const char* api_name, const jyppx_ocv_bgsegm_background_subtractor* subtractor)
    {
        return subtractor == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "subtractor")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mog(const char* api_name, const jyppx_ocv_bgsegm_background_subtractor_mog* subtractor)
    {
        return subtractor == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "subtractor")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_gmg(const char* api_name, const jyppx_ocv_bgsegm_background_subtractor_gmg* subtractor)
    {
        return subtractor == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "subtractor")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_cnt(const char* api_name, const jyppx_ocv_bgsegm_background_subtractor_cnt* subtractor)
    {
        return subtractor == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "subtractor")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_generator(const char* api_name, const jyppx_ocv_bgsegm_synthetic_sequence_generator* generator)
    {
        return generator == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "generator")
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
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
    int create_mog_handle(
        const char* api_name,
        int history,
        int nmixtures,
        double background_ratio,
        double noise_sigma,
        jyppx_ocv_bgsegm_background_subtractor_mog** subtractor)
    {
        if (subtractor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "subtractor");
        }

        *subtractor = nullptr;
        jyppx_ocv_bgsegm_background_subtractor_mog* created = new (std::nothrow) jyppx_ocv_bgsegm_background_subtractor_mog();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::bgsegm::createBackgroundSubtractorMOG(history, nmixtures, background_ratio, noise_sigma);
        created->value = created->concrete;
        *subtractor = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_gmg_handle(
        const char* api_name,
        int initialization_frames,
        double decision_threshold,
        jyppx_ocv_bgsegm_background_subtractor_gmg** subtractor)
    {
        if (subtractor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "subtractor");
        }

        *subtractor = nullptr;
        jyppx_ocv_bgsegm_background_subtractor_gmg* created = new (std::nothrow) jyppx_ocv_bgsegm_background_subtractor_gmg();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::bgsegm::createBackgroundSubtractorGMG(initialization_frames, decision_threshold);
        created->value = created->concrete;
        *subtractor = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_cnt_handle(
        const char* api_name,
        int min_pixel_stability,
        int use_history,
        int max_pixel_stability,
        int is_parallel,
        jyppx_ocv_bgsegm_background_subtractor_cnt** subtractor)
    {
        if (subtractor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "subtractor");
        }

        *subtractor = nullptr;
        jyppx_ocv_bgsegm_background_subtractor_cnt* created = new (std::nothrow) jyppx_ocv_bgsegm_background_subtractor_cnt();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::bgsegm::createBackgroundSubtractorCNT(
            min_pixel_stability,
            use_history != 0,
            max_pixel_stability,
            is_parallel != 0);
        created->value = created->concrete;
        *subtractor = created;
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

void jyppx_ocv_bgsegm_background_subtractor_release_handle(jyppx_ocv_bgsegm_background_subtractor* subtractor)
{
    delete subtractor;
}

int jyppx_ocv_bgsegm_background_subtractor_apply(jyppx_ocv_bgsegm_background_subtractor* subtractor, const jyppx_ocv_mat* image, jyppx_ocv_mat* fgmask, double learning_rate)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_apply";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_subtractor(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, fgmask, "fgmask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        subtractor->value->apply(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(fgmask), learning_rate);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)learning_rate;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bgsegm_background_subtractor_apply_with_known_foreground(jyppx_ocv_bgsegm_background_subtractor* subtractor, const jyppx_ocv_mat* image, const jyppx_ocv_mat* known_foreground_mask, jyppx_ocv_mat* fgmask, double learning_rate)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_apply_with_known_foreground";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_subtractor(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, known_foreground_mask, "known_foreground_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, fgmask, "fgmask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        subtractor->value->apply(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(known_foreground_mask), opencv_csharp_native::mat_value(fgmask), learning_rate);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)learning_rate;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bgsegm_background_subtractor_get_background_image(const jyppx_ocv_bgsegm_background_subtractor* subtractor, jyppx_ocv_mat* background_image)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_get_background_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_subtractor(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, background_image, "background_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        subtractor->value->getBackgroundImage(opencv_csharp_native::mat_value(background_image));
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

int jyppx_ocv_bgsegm_background_subtractor_mog_create(int history, int nmixtures, double background_ratio, double noise_sigma, jyppx_ocv_bgsegm_background_subtractor_mog** subtractor)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_mog_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        return create_mog_handle(api_name, history, nmixtures, background_ratio, noise_sigma, subtractor);
#else
        (void)history; (void)nmixtures; (void)background_ratio; (void)noise_sigma;
        if (subtractor != nullptr) { *subtractor = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bgsegm_background_subtractor_mog_get_int(const jyppx_ocv_bgsegm_background_subtractor_mog* subtractor, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_mog_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        switch (property_id)
        {
        case 0: *value = subtractor->concrete->getHistory(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = subtractor->concrete->getNMixtures(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_bgsegm_background_subtractor_mog_set_int(jyppx_ocv_bgsegm_background_subtractor_mog* subtractor, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_mog_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        switch (property_id)
        {
        case 0: subtractor->concrete->setHistory(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: subtractor->concrete->setNMixtures(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_bgsegm_background_subtractor_mog_get_double(const jyppx_ocv_bgsegm_background_subtractor_mog* subtractor, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_mog_get_double";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        switch (property_id)
        {
        case 0: *value = subtractor->concrete->getBackgroundRatio(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = subtractor->concrete->getNoiseSigma(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_bgsegm_background_subtractor_mog_set_double(jyppx_ocv_bgsegm_background_subtractor_mog* subtractor, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_mog_set_double";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        switch (property_id)
        {
        case 0: subtractor->concrete->setBackgroundRatio(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: subtractor->concrete->setNoiseSigma(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_bgsegm_background_subtractor_gmg_create(int initialization_frames, double decision_threshold, jyppx_ocv_bgsegm_background_subtractor_gmg** subtractor)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_gmg_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        return create_gmg_handle(api_name, initialization_frames, decision_threshold, subtractor);
#else
        (void)initialization_frames; (void)decision_threshold;
        if (subtractor != nullptr) { *subtractor = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bgsegm_background_subtractor_gmg_get_int(const jyppx_ocv_bgsegm_background_subtractor_gmg* subtractor, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_gmg_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_gmg(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        switch (property_id)
        {
        case 0: *value = subtractor->concrete->getMaxFeatures(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = subtractor->concrete->getNumFrames(); return OPENCV_CSHARP_STATUS_OK;
        case 2: *value = subtractor->concrete->getQuantizationLevels(); return OPENCV_CSHARP_STATUS_OK;
        case 3: *value = subtractor->concrete->getSmoothingRadius(); return OPENCV_CSHARP_STATUS_OK;
        case 4: *value = subtractor->concrete->getUpdateBackgroundModel() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_bgsegm_background_subtractor_gmg_set_int(jyppx_ocv_bgsegm_background_subtractor_gmg* subtractor, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_gmg_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_gmg(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        switch (property_id)
        {
        case 0: subtractor->concrete->setMaxFeatures(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: subtractor->concrete->setNumFrames(value); return OPENCV_CSHARP_STATUS_OK;
        case 2: subtractor->concrete->setQuantizationLevels(value); return OPENCV_CSHARP_STATUS_OK;
        case 3: subtractor->concrete->setSmoothingRadius(value); return OPENCV_CSHARP_STATUS_OK;
        case 4: subtractor->concrete->setUpdateBackgroundModel(value != 0); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_bgsegm_background_subtractor_gmg_get_double(const jyppx_ocv_bgsegm_background_subtractor_gmg* subtractor, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_gmg_get_double";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_gmg(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        switch (property_id)
        {
        case 0: *value = subtractor->concrete->getDefaultLearningRate(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = subtractor->concrete->getBackgroundPrior(); return OPENCV_CSHARP_STATUS_OK;
        case 2: *value = subtractor->concrete->getDecisionThreshold(); return OPENCV_CSHARP_STATUS_OK;
        case 3: *value = subtractor->concrete->getMinVal(); return OPENCV_CSHARP_STATUS_OK;
        case 4: *value = subtractor->concrete->getMaxVal(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_bgsegm_background_subtractor_gmg_set_double(jyppx_ocv_bgsegm_background_subtractor_gmg* subtractor, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_gmg_set_double";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_gmg(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        switch (property_id)
        {
        case 0: subtractor->concrete->setDefaultLearningRate(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: subtractor->concrete->setBackgroundPrior(value); return OPENCV_CSHARP_STATUS_OK;
        case 2: subtractor->concrete->setDecisionThreshold(value); return OPENCV_CSHARP_STATUS_OK;
        case 3: subtractor->concrete->setMinVal(value); return OPENCV_CSHARP_STATUS_OK;
        case 4: subtractor->concrete->setMaxVal(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_bgsegm_background_subtractor_cnt_create(int min_pixel_stability, int use_history, int max_pixel_stability, int is_parallel, jyppx_ocv_bgsegm_background_subtractor_cnt** subtractor)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_cnt_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        return create_cnt_handle(api_name, min_pixel_stability, use_history, max_pixel_stability, is_parallel, subtractor);
#else
        (void)min_pixel_stability; (void)use_history; (void)max_pixel_stability; (void)is_parallel;
        if (subtractor != nullptr) { *subtractor = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_bgsegm_background_subtractor_cnt_get_int(const jyppx_ocv_bgsegm_background_subtractor_cnt* subtractor, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_cnt_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_cnt(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        switch (property_id)
        {
        case 0: *value = subtractor->concrete->getMinPixelStability(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = subtractor->concrete->getMaxPixelStability(); return OPENCV_CSHARP_STATUS_OK;
        case 2: *value = subtractor->concrete->getUseHistory() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case 3: *value = subtractor->concrete->getIsParallel() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_bgsegm_background_subtractor_cnt_set_int(jyppx_ocv_bgsegm_background_subtractor_cnt* subtractor, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_background_subtractor_cnt_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_cnt(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        switch (property_id)
        {
        case 0: subtractor->concrete->setMinPixelStability(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: subtractor->concrete->setMaxPixelStability(value); return OPENCV_CSHARP_STATUS_OK;
        case 2: subtractor->concrete->setUseHistory(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case 3: subtractor->concrete->setIsParallel(value != 0); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_bgsegm_synthetic_sequence_generator_create(const jyppx_ocv_mat* background, const jyppx_ocv_mat* object, double amplitude, double wavelength, double wavespeed, double objspeed, jyppx_ocv_bgsegm_synthetic_sequence_generator** generator)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_synthetic_sequence_generator_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, background, "background");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, object, "object");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        if (generator == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "generator");
        }

        *generator = nullptr;
        jyppx_ocv_bgsegm_synthetic_sequence_generator* created = new (std::nothrow) jyppx_ocv_bgsegm_synthetic_sequence_generator();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::bgsegm::createSyntheticSequenceGenerator(
            opencv_csharp_native::mat_value(background),
            opencv_csharp_native::mat_value(object),
            amplitude,
            wavelength,
            wavespeed,
            objspeed);
        *generator = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)amplitude; (void)wavelength; (void)wavespeed; (void)objspeed;
        if (generator != nullptr) { *generator = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_bgsegm_synthetic_sequence_generator_release_handle(jyppx_ocv_bgsegm_synthetic_sequence_generator* generator)
{
    delete generator;
}

int jyppx_ocv_bgsegm_synthetic_sequence_generator_get_next_frame(jyppx_ocv_bgsegm_synthetic_sequence_generator* generator, jyppx_ocv_mat* frame, jyppx_ocv_mat* gt_mask)
{
    constexpr const char* api_name = "jyppx_ocv_bgsegm_synthetic_sequence_generator_get_next_frame";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_generator(api_name, generator);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, frame, "frame");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, gt_mask, "gt_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_BGSEGM)
        generator->value->getNextFrame(opencv_csharp_native::mat_value(frame), opencv_csharp_native::mat_value(gt_mask));
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

