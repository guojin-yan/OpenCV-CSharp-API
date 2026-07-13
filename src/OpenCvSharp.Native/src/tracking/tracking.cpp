#include "open_cv_sharp/tracking/tracking.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "tracking_handles.h"

#include <new>
#include <string>
#include <vector>

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_tracker(const char* api_name, const jyppx_ocv_tracking_tracker* tracker)
    {
        return tracker == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "tracker")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_csrt(const char* api_name, const jyppx_ocv_tracking_tracker_csrt* tracker)
    {
        return tracker == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "tracker")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_legacy_tracker(const char* api_name, const jyppx_ocv_tracking_legacy_tracker* tracker)
    {
        return tracker == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "tracker")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_multi_tracker(const char* api_name, const jyppx_ocv_tracking_legacy_multi_tracker* tracker)
    {
        return tracker == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "multi_tracker")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int(const char* api_name, const int* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_rect(const char* api_name, const jyppx_ocv_tracking_rect* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_rect2d(const char* api_name, const jyppx_ocv_tracking_rect2d* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
    cv::Rect to_cv_rect(jyppx_ocv_tracking_rect rect)
    {
        return cv::Rect(rect.x, rect.y, rect.width, rect.height);
    }

    jyppx_ocv_tracking_rect from_cv_rect(const cv::Rect& rect)
    {
        return jyppx_ocv_tracking_rect{ rect.x, rect.y, rect.width, rect.height };
    }

    cv::Rect2d to_cv_rect2d(jyppx_ocv_tracking_rect2d rect)
    {
        return cv::Rect2d(rect.x, rect.y, rect.width, rect.height);
    }

    jyppx_ocv_tracking_rect2d from_cv_rect2d(const cv::Rect2d& rect)
    {
        return jyppx_ocv_tracking_rect2d{ rect.x, rect.y, rect.width, rect.height };
    }

    cv::tracking::TrackerKCF::Params to_kcf_params(const jyppx_ocv_tracking_kcf_params& parameters)
    {
        cv::tracking::TrackerKCF::Params result;
        result.detect_thresh = parameters.detect_thresh;
        result.sigma = parameters.sigma;
        result.lambda = parameters.lambda_value;
        result.interp_factor = parameters.interp_factor;
        result.output_sigma_factor = parameters.output_sigma_factor;
        result.pca_learning_rate = parameters.pca_learning_rate;
        result.resize = parameters.resize != 0;
        result.split_coeff = parameters.split_coeff != 0;
        result.wrap_kernel = parameters.wrap_kernel != 0;
        result.compress_feature = parameters.compress_feature != 0;
        result.max_patch_size = parameters.max_patch_size;
        result.compressed_size = parameters.compressed_size;
        result.desc_pca = parameters.desc_pca;
        result.desc_npca = parameters.desc_npca;
        return result;
    }

    jyppx_ocv_tracking_kcf_params from_kcf_params(const cv::tracking::TrackerKCF::Params& parameters)
    {
        return jyppx_ocv_tracking_kcf_params{
            parameters.detect_thresh,
            parameters.sigma,
            parameters.lambda,
            parameters.interp_factor,
            parameters.output_sigma_factor,
            parameters.pca_learning_rate,
            parameters.resize ? 1 : 0,
            parameters.split_coeff ? 1 : 0,
            parameters.wrap_kernel ? 1 : 0,
            parameters.compress_feature ? 1 : 0,
            parameters.max_patch_size,
            parameters.compressed_size,
            parameters.desc_pca,
            parameters.desc_npca
        };
    }

    cv::tracking::TrackerCSRT::Params to_csrt_params(const jyppx_ocv_tracking_csrt_params& parameters)
    {
        cv::tracking::TrackerCSRT::Params result;
        result.use_hog = parameters.use_hog != 0;
        result.use_color_names = parameters.use_color_names != 0;
        result.use_gray = parameters.use_gray != 0;
        result.use_rgb = parameters.use_rgb != 0;
        result.use_channel_weights = parameters.use_channel_weights != 0;
        result.use_segmentation = parameters.use_segmentation != 0;
        result.window_function = parameters.window_function == nullptr ? std::string() : std::string(parameters.window_function);
        result.kaiser_alpha = parameters.kaiser_alpha;
        result.cheb_attenuation = parameters.cheb_attenuation;
        result.template_size = parameters.template_size;
        result.gsl_sigma = parameters.gsl_sigma;
        result.hog_orientations = parameters.hog_orientations;
        result.hog_clip = parameters.hog_clip;
        result.padding = parameters.padding;
        result.filter_lr = parameters.filter_lr;
        result.weights_lr = parameters.weights_lr;
        result.num_hog_channels_used = parameters.num_hog_channels_used;
        result.admm_iterations = parameters.admm_iterations;
        result.histogram_bins = parameters.histogram_bins;
        result.histogram_lr = parameters.histogram_lr;
        result.background_ratio = parameters.background_ratio;
        result.number_of_scales = parameters.number_of_scales;
        result.scale_sigma_factor = parameters.scale_sigma_factor;
        result.scale_model_max_area = parameters.scale_model_max_area;
        result.scale_lr = parameters.scale_lr;
        result.scale_step = parameters.scale_step;
        result.psr_threshold = parameters.psr_threshold;
        return result;
    }

    void fill_csrt_params(const cv::tracking::TrackerCSRT::Params& parameters, jyppx_ocv_tracking_csrt_params* output)
    {
        output->use_hog = parameters.use_hog ? 1 : 0;
        output->use_color_names = parameters.use_color_names ? 1 : 0;
        output->use_gray = parameters.use_gray ? 1 : 0;
        output->use_rgb = parameters.use_rgb ? 1 : 0;
        output->use_channel_weights = parameters.use_channel_weights ? 1 : 0;
        output->use_segmentation = parameters.use_segmentation ? 1 : 0;
        output->window_function = nullptr;
        output->kaiser_alpha = parameters.kaiser_alpha;
        output->cheb_attenuation = parameters.cheb_attenuation;
        output->template_size = parameters.template_size;
        output->gsl_sigma = parameters.gsl_sigma;
        output->hog_orientations = parameters.hog_orientations;
        output->hog_clip = parameters.hog_clip;
        output->padding = parameters.padding;
        output->filter_lr = parameters.filter_lr;
        output->weights_lr = parameters.weights_lr;
        output->num_hog_channels_used = parameters.num_hog_channels_used;
        output->admm_iterations = parameters.admm_iterations;
        output->histogram_bins = parameters.histogram_bins;
        output->histogram_lr = parameters.histogram_lr;
        output->background_ratio = parameters.background_ratio;
        output->number_of_scales = parameters.number_of_scales;
        output->scale_sigma_factor = parameters.scale_sigma_factor;
        output->scale_model_max_area = parameters.scale_model_max_area;
        output->scale_lr = parameters.scale_lr;
        output->scale_step = parameters.scale_step;
        output->psr_threshold = parameters.psr_threshold;
    }

    cv::legacy::TrackerMIL::Params to_mil_params(const jyppx_ocv_tracking_mil_params& parameters)
    {
        cv::legacy::TrackerMIL::Params result;
        result.samplerInitInRadius = parameters.sampler_init_in_radius;
        result.samplerSearchWinSize = parameters.sampler_search_win_size;
        result.samplerInitMaxNegNum = parameters.sampler_init_max_neg_num;
        result.samplerTrackInRadius = parameters.sampler_track_in_radius;
        result.samplerTrackMaxPosNum = parameters.sampler_track_max_pos_num;
        result.samplerTrackMaxNegNum = parameters.sampler_track_max_neg_num;
        result.featureSetNumFeatures = parameters.feature_set_num_features;
        return result;
    }

    jyppx_ocv_tracking_mil_params from_mil_params(const cv::legacy::TrackerMIL::Params& parameters)
    {
        return jyppx_ocv_tracking_mil_params{
            parameters.samplerInitInRadius,
            parameters.samplerSearchWinSize,
            parameters.samplerInitMaxNegNum,
            parameters.samplerTrackInRadius,
            parameters.samplerTrackMaxPosNum,
            parameters.samplerTrackMaxNegNum,
            parameters.featureSetNumFeatures
        };
    }

    cv::legacy::TrackerMedianFlow::Params to_median_flow_params(const jyppx_ocv_tracking_median_flow_params& parameters)
    {
        cv::legacy::TrackerMedianFlow::Params result;
        result.pointsInGrid = parameters.points_in_grid;
        result.winSize = cv::Size(parameters.win_width, parameters.win_height);
        result.maxLevel = parameters.max_level;
        result.termCriteria = cv::TermCriteria(parameters.criteria_type, parameters.criteria_max_count, parameters.criteria_epsilon);
        result.winSizeNCC = cv::Size(parameters.win_width_ncc, parameters.win_height_ncc);
        result.maxMedianLengthOfDisplacementDifference = parameters.max_median_length_of_displacement_difference;
        return result;
    }

    jyppx_ocv_tracking_median_flow_params from_median_flow_params(const cv::legacy::TrackerMedianFlow::Params& parameters)
    {
        return jyppx_ocv_tracking_median_flow_params{
            parameters.pointsInGrid,
            parameters.winSize.width,
            parameters.winSize.height,
            parameters.maxLevel,
            parameters.termCriteria.type,
            parameters.termCriteria.maxCount,
            parameters.termCriteria.epsilon,
            parameters.winSizeNCC.width,
            parameters.winSizeNCC.height,
            parameters.maxMedianLengthOfDisplacementDifference
        };
    }

    int create_kcf_handle(const char* api_name, const cv::tracking::TrackerKCF::Params& parameters, jyppx_ocv_tracking_tracker_kcf** tracker)
    {
        if (tracker == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tracker");
        }

        *tracker = nullptr;
        jyppx_ocv_tracking_tracker_kcf* created = new (std::nothrow) jyppx_ocv_tracking_tracker_kcf();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::tracking::TrackerKCF::create(parameters);
        created->value = created->concrete;
        *tracker = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_csrt_handle(const char* api_name, const cv::tracking::TrackerCSRT::Params& parameters, jyppx_ocv_tracking_tracker_csrt** tracker)
    {
        if (tracker == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tracker");
        }

        *tracker = nullptr;
        jyppx_ocv_tracking_tracker_csrt* created = new (std::nothrow) jyppx_ocv_tracking_tracker_csrt();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::tracking::TrackerCSRT::create(parameters);
        created->value = created->concrete;
        *tracker = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    template <typename THandle, typename TNative>
    int create_legacy_handle(const char* api_name, const cv::Ptr<TNative>& native, THandle** tracker)
    {
        if (tracker == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tracker");
        }

        *tracker = nullptr;
        THandle* created = new (std::nothrow) THandle();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = native;
        created->value = native;
        *tracker = created;
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

void jyppx_ocv_tracking_tracker_release_handle(jyppx_ocv_tracking_tracker* tracker)
{
    delete tracker;
}

void jyppx_ocv_tracking_legacy_tracker_release_handle(jyppx_ocv_tracking_legacy_tracker* tracker)
{
    delete tracker;
}

void jyppx_ocv_tracking_legacy_multi_tracker_release_handle(jyppx_ocv_tracking_legacy_multi_tracker* tracker)
{
    delete tracker;
}

int jyppx_ocv_tracking_tracker_init(jyppx_ocv_tracking_tracker* tracker, const jyppx_ocv_mat* image, jyppx_ocv_tracking_rect bounding_box)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_tracker_init";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_tracker(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        tracker->value->init(opencv_csharp_native::mat_value(image), to_cv_rect(bounding_box));
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

int jyppx_ocv_tracking_tracker_update(jyppx_ocv_tracking_tracker* tracker, const jyppx_ocv_mat* image, jyppx_ocv_tracking_rect* bounding_box, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_tracker_update";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_tracker(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_rect(api_name, bounding_box, "bounding_box");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        cv::Rect rect = to_cv_rect(*bounding_box);
        bool ok = tracker->value->update(opencv_csharp_native::mat_value(image), rect);
        *bounding_box = from_cv_rect(rect);
        *result = ok ? 1 : 0;
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

int jyppx_ocv_tracking_tracker_kcf_create_default(jyppx_ocv_tracking_tracker_kcf** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_tracker_kcf_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        return create_kcf_handle(api_name, cv::tracking::TrackerKCF::Params(), tracker);
#else
        (void)tracker;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tracking_tracker_kcf_create(const jyppx_ocv_tracking_kcf_params* parameters, jyppx_ocv_tracking_tracker_kcf** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_tracker_kcf_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        return create_kcf_handle(api_name, to_kcf_params(*parameters), tracker);
#else
        (void)tracker;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tracking_tracker_kcf_get_default_params(jyppx_ocv_tracking_kcf_params* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_tracker_kcf_get_default_params";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        *parameters = from_kcf_params(cv::tracking::TrackerKCF::Params());
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

int jyppx_ocv_tracking_tracker_csrt_create_default(jyppx_ocv_tracking_tracker_csrt** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_tracker_csrt_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        return create_csrt_handle(api_name, cv::tracking::TrackerCSRT::Params(), tracker);
#else
        (void)tracker;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tracking_tracker_csrt_create(const jyppx_ocv_tracking_csrt_params* parameters, jyppx_ocv_tracking_tracker_csrt** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_tracker_csrt_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        return create_csrt_handle(api_name, to_csrt_params(*parameters), tracker);
#else
        (void)tracker;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tracking_tracker_csrt_get_default_params(jyppx_ocv_tracking_csrt_params* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_tracker_csrt_get_default_params";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        fill_csrt_params(cv::tracking::TrackerCSRT::Params(), parameters);
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

int jyppx_ocv_tracking_tracker_csrt_set_initial_mask(jyppx_ocv_tracking_tracker_csrt* tracker, const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_tracker_csrt_set_initial_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_csrt(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        tracker->concrete->setInitialMask(opencv_csharp_native::mat_value(mask));
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

int jyppx_ocv_tracking_legacy_tracker_init(jyppx_ocv_tracking_legacy_tracker* tracker, const jyppx_ocv_mat* image, jyppx_ocv_tracking_rect2d bounding_box)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_tracker_init";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_legacy_tracker(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        bool ok = tracker->value->init(opencv_csharp_native::mat_value(image), to_cv_rect2d(bounding_box));
        return ok ? OPENCV_CSHARP_STATUS_OK : opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "jyppx_ocv_tracking_legacy_tracker_init failed.");
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tracking_legacy_tracker_update(jyppx_ocv_tracking_legacy_tracker* tracker, const jyppx_ocv_mat* image, jyppx_ocv_tracking_rect2d* bounding_box, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_tracker_update";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_legacy_tracker(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_rect2d(api_name, bounding_box, "bounding_box");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        cv::Rect2d rect = to_cv_rect2d(*bounding_box);
        bool ok = tracker->value->update(opencv_csharp_native::mat_value(image), rect);
        *bounding_box = from_cv_rect2d(rect);
        *result = ok ? 1 : 0;
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

int jyppx_ocv_tracking_legacy_tracker_mosse_create(jyppx_ocv_tracking_legacy_tracker_mosse** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_tracker_mosse_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        return create_legacy_handle(api_name, cv::legacy::TrackerMOSSE::create(), tracker);
#else
        (void)tracker;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tracking_legacy_tracker_mil_create_default(jyppx_ocv_tracking_legacy_tracker_mil** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_tracker_mil_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        return create_legacy_handle(api_name, cv::legacy::TrackerMIL::create(), tracker);
#else
        (void)tracker;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tracking_legacy_tracker_mil_create(const jyppx_ocv_tracking_mil_params* parameters, jyppx_ocv_tracking_legacy_tracker_mil** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_tracker_mil_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        return create_legacy_handle(api_name, cv::legacy::TrackerMIL::create(to_mil_params(*parameters)), tracker);
#else
        (void)tracker;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tracking_legacy_tracker_mil_get_default_params(jyppx_ocv_tracking_mil_params* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_tracker_mil_get_default_params";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        *parameters = from_mil_params(cv::legacy::TrackerMIL::Params());
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

int jyppx_ocv_tracking_legacy_tracker_median_flow_create_default(jyppx_ocv_tracking_legacy_tracker_median_flow** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_tracker_median_flow_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        return create_legacy_handle(api_name, cv::legacy::TrackerMedianFlow::create(), tracker);
#else
        (void)tracker;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tracking_legacy_tracker_median_flow_create(const jyppx_ocv_tracking_median_flow_params* parameters, jyppx_ocv_tracking_legacy_tracker_median_flow** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_tracker_median_flow_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        return create_legacy_handle(api_name, cv::legacy::TrackerMedianFlow::create(to_median_flow_params(*parameters)), tracker);
#else
        (void)tracker;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_tracking_legacy_tracker_median_flow_get_default_params(jyppx_ocv_tracking_median_flow_params* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_tracker_median_flow_get_default_params";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        *parameters = from_median_flow_params(cv::legacy::TrackerMedianFlow::Params());
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

int jyppx_ocv_tracking_legacy_multi_tracker_create(jyppx_ocv_tracking_legacy_multi_tracker** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_multi_tracker_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (tracker == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tracker");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        *tracker = nullptr;
        jyppx_ocv_tracking_legacy_multi_tracker* created = new (std::nothrow) jyppx_ocv_tracking_legacy_multi_tracker();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::legacy::MultiTracker::create();
        *tracker = created;
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

int jyppx_ocv_tracking_legacy_multi_tracker_add(jyppx_ocv_tracking_legacy_multi_tracker* multi_tracker, jyppx_ocv_tracking_legacy_tracker* tracker, const jyppx_ocv_mat* image, jyppx_ocv_tracking_rect2d bounding_box, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_multi_tracker_add";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_multi_tracker(api_name, multi_tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_legacy_tracker(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        bool ok = multi_tracker->value->add(tracker->value, opencv_csharp_native::mat_value(image), to_cv_rect2d(bounding_box));
        *result = ok ? 1 : 0;
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

int jyppx_ocv_tracking_legacy_multi_tracker_update_count(jyppx_ocv_tracking_legacy_multi_tracker* multi_tracker, const jyppx_ocv_mat* image, int* result, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_multi_tracker_update_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_multi_tracker(api_name, multi_tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        std::vector<cv::Rect2d> boxes;
        bool ok = multi_tracker->value->update(opencv_csharp_native::mat_value(image), boxes);
        *result = ok ? 1 : 0;
        *count = static_cast<int>(boxes.size());
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

int jyppx_ocv_tracking_legacy_multi_tracker_update_fill(jyppx_ocv_tracking_legacy_multi_tracker* multi_tracker, const jyppx_ocv_mat* image, jyppx_ocv_tracking_rect2d* bounding_boxes, int bounding_box_capacity, int* result, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_multi_tracker_update_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_multi_tracker(api_name, multi_tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (bounding_box_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bounding_box_capacity");
        }
        if (bounding_box_capacity > 0 && bounding_boxes == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bounding_boxes");
        }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        std::vector<cv::Rect2d> boxes;
        bool ok = multi_tracker->value->update(opencv_csharp_native::mat_value(image), boxes);
        *result = ok ? 1 : 0;
        int actual_count = static_cast<int>(boxes.size());
        int copy_count = actual_count < bounding_box_capacity ? actual_count : bounding_box_capacity;
        for (int i = 0; i < copy_count; i++)
        {
            bounding_boxes[i] = from_cv_rect2d(boxes[static_cast<size_t>(i)]);
        }

        *count = actual_count;
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

int jyppx_ocv_tracking_legacy_multi_tracker_get_objects_count(const jyppx_ocv_tracking_legacy_multi_tracker* multi_tracker, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_multi_tracker_get_objects_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_multi_tracker(api_name, multi_tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        *count = static_cast<int>(multi_tracker->value->getObjects().size());
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

int jyppx_ocv_tracking_legacy_multi_tracker_get_objects_fill(const jyppx_ocv_tracking_legacy_multi_tracker* multi_tracker, jyppx_ocv_tracking_rect2d* bounding_boxes, int bounding_box_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_tracking_legacy_multi_tracker_get_objects_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_multi_tracker(api_name, multi_tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (bounding_box_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bounding_box_capacity");
        }
        if (bounding_box_capacity > 0 && bounding_boxes == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bounding_boxes");
        }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_TRACKING)
        const std::vector<cv::Rect2d>& boxes = multi_tracker->value->getObjects();
        int actual_count = static_cast<int>(boxes.size());
        int copy_count = actual_count < bounding_box_capacity ? actual_count : bounding_box_capacity;
        for (int i = 0; i < copy_count; i++)
        {
            bounding_boxes[i] = from_cv_rect2d(boxes[static_cast<size_t>(i)]);
        }

        *count = actual_count;
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


