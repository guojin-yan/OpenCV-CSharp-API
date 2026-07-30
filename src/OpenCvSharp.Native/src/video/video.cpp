#include "open_cv_sharp/video/video.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "video_handles.h"

#include <new>
#include <stdexcept>
#include <vector>

namespace
{
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

    int validate_output_rect(const char* api_name, const jyppx_ocv_video_rect* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_rotated_rect(const char* api_name, const jyppx_ocv_video_rotated_rect* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_filter(const char* api_name, const jyppx_ocv_kalman_filter* filter)
    {
        return filter == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "filter")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_background_subtractor(const char* api_name, const jyppx_ocv_background_subtractor* subtractor)
    {
        return subtractor == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "subtractor")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mog2(const char* api_name, const jyppx_ocv_background_subtractor_mog2* subtractor)
    {
        return subtractor == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "subtractor")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_knn(const char* api_name, const jyppx_ocv_background_subtractor_knn* subtractor)
    {
        return subtractor == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "subtractor")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_double(const char* api_name, const double* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_video_tracker(const char* api_name, const jyppx_ocv_video_tracker* tracker)
    {
        return tracker == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "tracker")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_iteration_schedule(
        const char* api_name,
        const int* iterations_per_level,
        int iteration_count)
    {
        if (iteration_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "iteration_count");
        }
        if (iteration_count > 0 && iterations_per_level == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "iterations_per_level");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_points(
        const char* api_name,
        const jyppx_ocv_video_point2f* prev_points,
        int point_count,
        const jyppx_ocv_video_point2f* initial_next_points,
        int use_initial_flow,
        const jyppx_ocv_video_point2f* next_points,
        const unsigned char* status,
        const float* err)
    {
        if (point_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (point_count > 0 && prev_points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "prev_points");
        }

        if (use_initial_flow != 0 && point_count > 0 && initial_next_points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "initial_next_points");
        }

        if (point_count > 0 && (next_points == nullptr || status == nullptr || err == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output arrays");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Point2f to_cv_point(jyppx_ocv_video_point2f point)
    {
        return cv::Point2f(point.x, point.y);
    }

    jyppx_ocv_video_point2f from_cv_point(cv::Point2f point)
    {
        return jyppx_ocv_video_point2f{ point.x, point.y };
    }

    cv::Rect to_cv_rect(jyppx_ocv_video_rect rect)
    {
        return cv::Rect(rect.x, rect.y, rect.width, rect.height);
    }

    jyppx_ocv_video_rect from_cv_rect(cv::Rect rect)
    {
        return jyppx_ocv_video_rect{ rect.x, rect.y, rect.width, rect.height };
    }

    jyppx_ocv_video_rotated_rect from_cv_rotated_rect(cv::RotatedRect rect)
    {
        return jyppx_ocv_video_rotated_rect{
            rect.center.x,
            rect.center.y,
            rect.size.width,
            rect.size.height,
            rect.angle
        };
    }

    cv::TermCriteria to_criteria(int type, int max_count, double epsilon)
    {
        return cv::TermCriteria(type, max_count, epsilon);
    }

    cv::TrackerMIL::Params to_tracker_mil_params(const jyppx_ocv_video_tracker_mil_params& parameters)
    {
        cv::TrackerMIL::Params result;
        result.samplerInitInRadius = parameters.sampler_init_in_radius;
        result.samplerInitMaxNegNum = parameters.sampler_init_max_neg_num;
        result.samplerSearchWinSize = parameters.sampler_search_win_size;
        result.samplerTrackInRadius = parameters.sampler_track_in_radius;
        result.samplerTrackMaxPosNum = parameters.sampler_track_max_pos_num;
        result.samplerTrackMaxNegNum = parameters.sampler_track_max_neg_num;
        result.featureSetNumFeatures = parameters.feature_set_num_features;
        return result;
    }

    jyppx_ocv_video_tracker_mil_params from_tracker_mil_params(const cv::TrackerMIL::Params& parameters)
    {
        return jyppx_ocv_video_tracker_mil_params{
            parameters.samplerInitInRadius,
            parameters.samplerInitMaxNegNum,
            parameters.samplerSearchWinSize,
            parameters.samplerTrackInRadius,
            parameters.samplerTrackMaxPosNum,
            parameters.samplerTrackMaxNegNum,
            parameters.featureSetNumFeatures
        };
    }

    cv::Mat& select_kalman_matrix(jyppx_ocv_kalman_filter* filter, int matrix_id)
    {
        switch (matrix_id)
        {
        case 0: return filter->value.statePre;
        case 1: return filter->value.statePost;
        case 2: return filter->value.transitionMatrix;
        case 3: return filter->value.controlMatrix;
        case 4: return filter->value.measurementMatrix;
        case 5: return filter->value.processNoiseCov;
        case 6: return filter->value.measurementNoiseCov;
        case 7: return filter->value.errorCovPre;
        case 8: return filter->value.gain;
        case 9: return filter->value.errorCovPost;
        default: throw std::out_of_range("matrix_id");
        }
    }

    const cv::Mat& select_kalman_matrix(const jyppx_ocv_kalman_filter* filter, int matrix_id)
    {
        return select_kalman_matrix(const_cast<jyppx_ocv_kalman_filter*>(filter), matrix_id);
    }

    int create_mat_handle(const char* api_name, const cv::Mat& value, jyppx_ocv_mat** out_mat)
    {
        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pyramid");
        }

        jyppx_ocv_mat* created = new (std::nothrow) jyppx_ocv_mat();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = value;
        *out_mat = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    const cv::Ptr<cv::BackgroundSubtractor>& to_background_subtractor(const jyppx_ocv_background_subtractor* subtractor)
    {
        return subtractor->value;
    }

    cv::Ptr<cv::BackgroundSubtractor>& to_background_subtractor(jyppx_ocv_background_subtractor* subtractor)
    {
        return subtractor->value;
    }

    int create_mog2_handle(
        const char* api_name,
        int history,
        double var_threshold,
        int detect_shadows,
        jyppx_ocv_background_subtractor_mog2** subtractor)
    {
        if (subtractor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "subtractor");
        }

        *subtractor = nullptr;
        jyppx_ocv_background_subtractor_mog2* created = new (std::nothrow) jyppx_ocv_background_subtractor_mog2();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::createBackgroundSubtractorMOG2(history, var_threshold, detect_shadows != 0);
        created->jyppx_ocv_background_subtractor::value = created->value;
        *subtractor = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_knn_handle(
        const char* api_name,
        int history,
        double dist2_threshold,
        int detect_shadows,
        jyppx_ocv_background_subtractor_knn** subtractor)
    {
        if (subtractor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "subtractor");
        }

        *subtractor = nullptr;
        jyppx_ocv_background_subtractor_knn* created = new (std::nothrow) jyppx_ocv_background_subtractor_knn();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::createBackgroundSubtractorKNN(history, dist2_threshold, detect_shadows != 0);
        created->jyppx_ocv_background_subtractor::value = created->value;
        *subtractor = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int get_mog2_double_property(const char* api_name, const cv::Ptr<cv::BackgroundSubtractorMOG2>& subtractor, int property_id, double* value)
    {
        switch (property_id)
        {
        case 0: *value = subtractor->getBackgroundRatio(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = subtractor->getVarThreshold(); return OPENCV_CSHARP_STATUS_OK;
        case 2: *value = subtractor->getVarThresholdGen(); return OPENCV_CSHARP_STATUS_OK;
        case 3: *value = subtractor->getVarInit(); return OPENCV_CSHARP_STATUS_OK;
        case 4: *value = subtractor->getVarMin(); return OPENCV_CSHARP_STATUS_OK;
        case 5: *value = subtractor->getVarMax(); return OPENCV_CSHARP_STATUS_OK;
        case 6: *value = subtractor->getComplexityReductionThreshold(); return OPENCV_CSHARP_STATUS_OK;
        case 7: *value = subtractor->getShadowThreshold(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
    }

    int set_mog2_double_property(const char* api_name, const cv::Ptr<cv::BackgroundSubtractorMOG2>& subtractor, int property_id, double value)
    {
        switch (property_id)
        {
        case 0: subtractor->setBackgroundRatio(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: subtractor->setVarThreshold(value); return OPENCV_CSHARP_STATUS_OK;
        case 2: subtractor->setVarThresholdGen(value); return OPENCV_CSHARP_STATUS_OK;
        case 3: subtractor->setVarInit(value); return OPENCV_CSHARP_STATUS_OK;
        case 4: subtractor->setVarMin(value); return OPENCV_CSHARP_STATUS_OK;
        case 5: subtractor->setVarMax(value); return OPENCV_CSHARP_STATUS_OK;
        case 6: subtractor->setComplexityReductionThreshold(value); return OPENCV_CSHARP_STATUS_OK;
        case 7: subtractor->setShadowThreshold(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
    }

    int get_knn_double_property(const char* api_name, const cv::Ptr<cv::BackgroundSubtractorKNN>& subtractor, int property_id, double* value)
    {
        switch (property_id)
        {
        case 0: *value = subtractor->getDist2Threshold(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = subtractor->getShadowThreshold(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
    }

    int set_knn_double_property(const char* api_name, const cv::Ptr<cv::BackgroundSubtractorKNN>& subtractor, int property_id, double value)
    {
        switch (property_id)
        {
        case 0: subtractor->setDist2Threshold(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: subtractor->setShadowThreshold(value); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
    }
#endif
}

int jyppx_ocv_video_calc_optical_flow_pyr_lk(
    const jyppx_ocv_mat* prev_img,
    const jyppx_ocv_mat* next_img,
    const jyppx_ocv_video_point2f* prev_points,
    int point_count,
    const jyppx_ocv_video_point2f* initial_next_points,
    int use_initial_flow,
    jyppx_ocv_video_point2f* next_points,
    unsigned char* status,
    float* err,
    int win_width,
    int win_height,
    int max_level,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int flags,
    double min_eig_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_video_calc_optical_flow_pyr_lk";
    try
    {
        opencv_csharp_native::clear_last_error();
        int native_status = validate_mat(api_name, prev_img, "prev_img");
        if (native_status != OPENCV_CSHARP_STATUS_OK) { return native_status; }
        native_status = validate_mat(api_name, next_img, "next_img");
        if (native_status != OPENCV_CSHARP_STATUS_OK) { return native_status; }
        native_status = validate_points(api_name, prev_points, point_count, initial_next_points, use_initial_flow, next_points, status, err);
        if (native_status != OPENCV_CSHARP_STATUS_OK) { return native_status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Point2f> prev;
        std::vector<cv::Point2f> next;
        prev.reserve(static_cast<size_t>(point_count));
        next.reserve(static_cast<size_t>(point_count));
        for (int i = 0; i < point_count; ++i)
        {
            prev.push_back(to_cv_point(prev_points[i]));
            next.push_back(use_initial_flow != 0 ? to_cv_point(initial_next_points[i]) : cv::Point2f());
        }

        std::vector<unsigned char> native_statuses;
        std::vector<float> native_errors;
        cv::calcOpticalFlowPyrLK(
            opencv_csharp_native::mat_value(prev_img),
            opencv_csharp_native::mat_value(next_img),
            prev,
            next,
            native_statuses,
            native_errors,
            cv::Size(win_width, win_height),
            max_level,
            to_criteria(criteria_type, criteria_max_count, criteria_epsilon),
            flags,
            min_eig_threshold);

        for (int i = 0; i < point_count; ++i)
        {
            next_points[i] = from_cv_point(next[static_cast<size_t>(i)]);
            status[i] = native_statuses[static_cast<size_t>(i)];
            err[i] = native_errors.empty() ? 0.0F : native_errors[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)win_width; (void)win_height; (void)max_level; (void)criteria_type; (void)criteria_max_count;
        (void)criteria_epsilon; (void)flags; (void)min_eig_threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_calc_optical_flow_farneback(
    const jyppx_ocv_mat* prev,
    const jyppx_ocv_mat* next,
    jyppx_ocv_mat* flow,
    double pyr_scale,
    int levels,
    int winsize,
    int iterations,
    int poly_n,
    double poly_sigma,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_video_calc_optical_flow_farneback";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, prev, "prev");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, next, "next");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, flow, "flow");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::calcOpticalFlowFarneback(
            opencv_csharp_native::mat_value(prev),
            opencv_csharp_native::mat_value(next),
            opencv_csharp_native::mat_value(flow),
            pyr_scale,
            levels,
            winsize,
            iterations,
            poly_n,
            poly_sigma,
            flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pyr_scale; (void)levels; (void)winsize; (void)iterations; (void)poly_n; (void)poly_sigma; (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_read_optical_flow(const char* path, jyppx_ocv_mat** flow)
{
    constexpr const char* api_name = "jyppx_ocv_video_read_optical_flow";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (path == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "path");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat loaded = cv::readOpticalFlow(path);
        return create_mat_handle(api_name, loaded, flow);
#else
        (void)flow;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_write_optical_flow(const char* path, const jyppx_ocv_mat* flow, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_video_write_optical_flow";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (path == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "path");
        }

        int status = validate_mat(api_name, flow, "flow");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *result = cv::writeOpticalFlow(path, opencv_csharp_native::mat_value(flow)) ? 1 : 0;
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

int jyppx_ocv_video_build_optical_flow_pyramid_count(
    const jyppx_ocv_mat* image,
    int win_width,
    int win_height,
    int max_level,
    int with_derivatives,
    int pyr_border,
    int deriv_border,
    int try_reuse_input_image,
    int* level_count,
    int* mat_count)
{
    constexpr const char* api_name = "jyppx_ocv_video_build_optical_flow_pyramid_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, level_count, "level_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, mat_count, "mat_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> pyramid;
        *level_count = cv::buildOpticalFlowPyramid(
            opencv_csharp_native::mat_value(image),
            pyramid,
            cv::Size(win_width, win_height),
            max_level,
            with_derivatives != 0,
            pyr_border,
            deriv_border,
            try_reuse_input_image != 0);
        *mat_count = static_cast<int>(pyramid.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)win_width; (void)win_height; (void)max_level; (void)with_derivatives; (void)pyr_border; (void)deriv_border; (void)try_reuse_input_image;
        *level_count = 0;
        *mat_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_build_optical_flow_pyramid_fill(
    const jyppx_ocv_mat* image,
    int win_width,
    int win_height,
    int max_level,
    int with_derivatives,
    int pyr_border,
    int deriv_border,
    int try_reuse_input_image,
    jyppx_ocv_mat** pyramid,
    int pyramid_capacity,
    int* level_count,
    int* mat_count)
{
    constexpr const char* api_name = "jyppx_ocv_video_build_optical_flow_pyramid_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, level_count, "level_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, mat_count, "mat_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (pyramid_capacity < 0 || (pyramid_capacity > 0 && pyramid == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pyramid");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_pyramid;
        *level_count = cv::buildOpticalFlowPyramid(
            opencv_csharp_native::mat_value(image),
            native_pyramid,
            cv::Size(win_width, win_height),
            max_level,
            with_derivatives != 0,
            pyr_border,
            deriv_border,
            try_reuse_input_image != 0);
        *mat_count = static_cast<int>(native_pyramid.size());
        if (pyramid_capacity < *mat_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pyramid_capacity");
        }

        for (int i = 0; i < *mat_count; ++i)
        {
            status = create_mat_handle(api_name, native_pyramid[static_cast<size_t>(i)], &pyramid[i]);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)win_width; (void)win_height; (void)max_level; (void)with_derivatives; (void)pyr_border; (void)deriv_border; (void)try_reuse_input_image;
        *level_count = 0;
        *mat_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_mean_shift(
    const jyppx_ocv_mat* prob_image,
    jyppx_ocv_video_rect* window,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int* iterations)
{
    constexpr const char* api_name = "jyppx_ocv_video_mean_shift";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, prob_image, "prob_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_rect(api_name, window, "window");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, iterations, "iterations");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Rect native_window = to_cv_rect(*window);
        *iterations = cv::meanShift(
            opencv_csharp_native::mat_value(prob_image),
            native_window,
            to_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        *window = from_cv_rect(native_window);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)criteria_type; (void)criteria_max_count; (void)criteria_epsilon;
        *iterations = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_cam_shift(
    const jyppx_ocv_mat* prob_image,
    jyppx_ocv_video_rect* window,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    jyppx_ocv_video_rotated_rect* box)
{
    constexpr const char* api_name = "jyppx_ocv_video_cam_shift";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, prob_image, "prob_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_rect(api_name, window, "window");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_rotated_rect(api_name, box, "box");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Rect native_window = to_cv_rect(*window);
        cv::RotatedRect native_box = cv::CamShift(
            opencv_csharp_native::mat_value(prob_image),
            native_window,
            to_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        *window = from_cv_rect(native_window);
        *box = from_cv_rotated_rect(native_box);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)criteria_type; (void)criteria_max_count; (void)criteria_epsilon;
        *box = jyppx_ocv_video_rotated_rect{ 0, 0, 0, 0, 0 };
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_kalman_filter_create(int dynam_params, int measure_params, int control_params, int type, jyppx_ocv_kalman_filter** filter)
{
    constexpr const char* api_name = "jyppx_ocv_kalman_filter_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (filter == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filter");
        }

        *filter = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_kalman_filter* created = new (std::nothrow) jyppx_ocv_kalman_filter();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::KalmanFilter(dynam_params, measure_params, control_params, type);
        *filter = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dynam_params; (void)measure_params; (void)control_params; (void)type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_kalman_filter_release_handle(jyppx_ocv_kalman_filter* filter)
{
    delete filter;
}

int jyppx_ocv_kalman_filter_init(jyppx_ocv_kalman_filter* filter, int dynam_params, int measure_params, int control_params, int type)
{
    constexpr const char* api_name = "jyppx_ocv_kalman_filter_init";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_filter(api_name, filter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        filter->value.init(dynam_params, measure_params, control_params, type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dynam_params; (void)measure_params; (void)control_params; (void)type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_kalman_filter_predict(jyppx_ocv_kalman_filter* filter, const jyppx_ocv_mat* control, jyppx_ocv_mat* prediction)
{
    constexpr const char* api_name = "jyppx_ocv_kalman_filter_predict";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_filter(api_name, filter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, prediction, "prediction");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Mat& native_prediction = control == nullptr
            ? filter->value.predict()
            : filter->value.predict(opencv_csharp_native::mat_value(control));
        native_prediction.copyTo(opencv_csharp_native::mat_value(prediction));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)control;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_kalman_filter_correct(jyppx_ocv_kalman_filter* filter, const jyppx_ocv_mat* measurement, jyppx_ocv_mat* corrected)
{
    constexpr const char* api_name = "jyppx_ocv_kalman_filter_correct";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_filter(api_name, filter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, measurement, "measurement");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, corrected, "corrected");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Mat& native_corrected = filter->value.correct(opencv_csharp_native::mat_value(measurement));
        native_corrected.copyTo(opencv_csharp_native::mat_value(corrected));
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

int jyppx_ocv_kalman_filter_get_matrix(const jyppx_ocv_kalman_filter* filter, int matrix_id, jyppx_ocv_mat* value)
{
    constexpr const char* api_name = "jyppx_ocv_kalman_filter_get_matrix";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_filter(api_name, filter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        select_kalman_matrix(filter, matrix_id).copyTo(opencv_csharp_native::mat_value(value));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)matrix_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_kalman_filter_set_matrix(jyppx_ocv_kalman_filter* filter, int matrix_id, const jyppx_ocv_mat* value)
{
    constexpr const char* api_name = "jyppx_ocv_kalman_filter_set_matrix";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_filter(api_name, filter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        opencv_csharp_native::mat_value(value).copyTo(select_kalman_matrix(filter, matrix_id));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)matrix_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_background_subtractor_release_handle(jyppx_ocv_background_subtractor* subtractor)
{
    delete subtractor;
}

int jyppx_ocv_background_subtractor_apply(
    jyppx_ocv_background_subtractor* subtractor,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* fgmask,
    double learning_rate)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_apply";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_background_subtractor(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, fgmask, "fgmask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        to_background_subtractor(subtractor)->apply(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(fgmask), learning_rate);
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

int jyppx_ocv_background_subtractor_apply_with_known_foreground(
    jyppx_ocv_background_subtractor* subtractor,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* known_foreground_mask,
    jyppx_ocv_mat* fgmask,
    double learning_rate)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_apply_with_known_foreground";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_background_subtractor(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, known_foreground_mask, "known_foreground_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, fgmask, "fgmask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        to_background_subtractor(subtractor)->apply(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(known_foreground_mask),
            opencv_csharp_native::mat_value(fgmask),
            learning_rate);
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

int jyppx_ocv_background_subtractor_get_background_image(
    const jyppx_ocv_background_subtractor* subtractor,
    jyppx_ocv_mat* background_image)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_get_background_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_background_subtractor(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, background_image, "background_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        to_background_subtractor(subtractor)->getBackgroundImage(opencv_csharp_native::mat_value(background_image));
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

int jyppx_ocv_background_subtractor_mog2_create(
    int history,
    double var_threshold,
    int detect_shadows,
    jyppx_ocv_background_subtractor_mog2** subtractor)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_mog2_handle(api_name, history, var_threshold, detect_shadows, subtractor);
#else
        (void)history; (void)var_threshold; (void)detect_shadows; (void)subtractor;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_mog2_get_history(const jyppx_ocv_background_subtractor_mog2* subtractor, int* history)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_get_history";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog2(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, history, "history");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *history = subtractor->value->getHistory();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *history = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_mog2_set_history(jyppx_ocv_background_subtractor_mog2* subtractor, int history)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_set_history";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog2(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        subtractor->value->setHistory(history);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)history;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_mog2_get_n_mixtures(const jyppx_ocv_background_subtractor_mog2* subtractor, int* n_mixtures)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_get_n_mixtures";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog2(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, n_mixtures, "n_mixtures");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *n_mixtures = subtractor->value->getNMixtures();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *n_mixtures = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_mog2_set_n_mixtures(jyppx_ocv_background_subtractor_mog2* subtractor, int n_mixtures)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_set_n_mixtures";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog2(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        subtractor->value->setNMixtures(n_mixtures);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)n_mixtures;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_mog2_get_detect_shadows(const jyppx_ocv_background_subtractor_mog2* subtractor, int* detect_shadows)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_get_detect_shadows";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog2(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, detect_shadows, "detect_shadows");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *detect_shadows = subtractor->value->getDetectShadows() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *detect_shadows = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_mog2_set_detect_shadows(jyppx_ocv_background_subtractor_mog2* subtractor, int detect_shadows)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_set_detect_shadows";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog2(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        subtractor->value->setDetectShadows(detect_shadows != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detect_shadows;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_mog2_get_int_property(const jyppx_ocv_background_subtractor_mog2* subtractor, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_get_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog2(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (property_id == 0)
        {
            *value = subtractor->value->getShadowValue();
            return OPENCV_CSHARP_STATUS_OK;
        }
        return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
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

int jyppx_ocv_background_subtractor_mog2_set_int_property(jyppx_ocv_background_subtractor_mog2* subtractor, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_set_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog2(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (property_id == 0)
        {
            subtractor->value->setShadowValue(value);
            return OPENCV_CSHARP_STATUS_OK;
        }
        return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
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

int jyppx_ocv_background_subtractor_mog2_get_double_property(const jyppx_ocv_background_subtractor_mog2* subtractor, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_get_double_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog2(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return get_mog2_double_property(api_name, subtractor->value, property_id, value);
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

int jyppx_ocv_background_subtractor_mog2_set_double_property(jyppx_ocv_background_subtractor_mog2* subtractor, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_mog2_set_double_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mog2(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_mog2_double_property(api_name, subtractor->value, property_id, value);
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

int jyppx_ocv_background_subtractor_knn_create(
    int history,
    double dist2_threshold,
    int detect_shadows,
    jyppx_ocv_background_subtractor_knn** subtractor)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_knn_handle(api_name, history, dist2_threshold, detect_shadows, subtractor);
#else
        (void)history; (void)dist2_threshold; (void)detect_shadows; (void)subtractor;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_knn_get_history(const jyppx_ocv_background_subtractor_knn* subtractor, int* history)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_get_history";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_knn(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, history, "history");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *history = subtractor->value->getHistory();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *history = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_knn_set_history(jyppx_ocv_background_subtractor_knn* subtractor, int history)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_set_history";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_knn(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        subtractor->value->setHistory(history);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)history;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_knn_get_n_samples(const jyppx_ocv_background_subtractor_knn* subtractor, int* n_samples)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_get_n_samples";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_knn(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, n_samples, "n_samples");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *n_samples = subtractor->value->getNSamples();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *n_samples = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_knn_set_n_samples(jyppx_ocv_background_subtractor_knn* subtractor, int n_samples)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_set_n_samples";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_knn(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        subtractor->value->setNSamples(n_samples);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)n_samples;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_knn_get_detect_shadows(const jyppx_ocv_background_subtractor_knn* subtractor, int* detect_shadows)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_get_detect_shadows";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_knn(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, detect_shadows, "detect_shadows");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *detect_shadows = subtractor->value->getDetectShadows() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *detect_shadows = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_knn_set_detect_shadows(jyppx_ocv_background_subtractor_knn* subtractor, int detect_shadows)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_set_detect_shadows";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_knn(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        subtractor->value->setDetectShadows(detect_shadows != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detect_shadows;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_background_subtractor_knn_get_int_property(const jyppx_ocv_background_subtractor_knn* subtractor, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_get_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_knn(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        switch (property_id)
        {
        case 0: *value = subtractor->value->getShadowValue(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = subtractor->value->getkNNSamples(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_background_subtractor_knn_set_int_property(jyppx_ocv_background_subtractor_knn* subtractor, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_set_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_knn(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        switch (property_id)
        {
        case 0: subtractor->value->setShadowValue(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: subtractor->value->setkNNSamples(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_background_subtractor_knn_get_double_property(const jyppx_ocv_background_subtractor_knn* subtractor, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_get_double_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_knn(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return get_knn_double_property(api_name, subtractor->value, property_id, value);
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

int jyppx_ocv_background_subtractor_knn_set_double_property(jyppx_ocv_background_subtractor_knn* subtractor, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_background_subtractor_knn_set_double_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_knn(api_name, subtractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_knn_double_property(api_name, subtractor->value, property_id, value);
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

namespace
{
    template <typename TAction>
    int video_object_guarded(const char* api_name, TAction&& action)
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

    int validate_dense_optical_flow(const char* api_name, const jyppx_ocv_dense_optical_flow* optical_flow)
    {
        return optical_flow == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "optical_flow")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_sparse_optical_flow(const char* api_name, const jyppx_ocv_sparse_optical_flow* optical_flow)
    {
        return optical_flow == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "optical_flow")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_float(const char* api_name, const float* value)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "value")
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    template <typename TAlgorithm>
    cv::Ptr<TAlgorithm> require_dense_algorithm(
        const char* api_name,
        const jyppx_ocv_dense_optical_flow* optical_flow)
    {
        cv::Ptr<TAlgorithm> algorithm = optical_flow->value.dynamicCast<TAlgorithm>();
        if (algorithm.empty())
        {
            throw std::invalid_argument(std::string(api_name) + ": optical_flow has the wrong concrete type");
        }
        return algorithm;
    }

    cv::Ptr<cv::SparsePyrLKOpticalFlow> require_sparse_pyr_lk(
        const char* api_name,
        const jyppx_ocv_sparse_optical_flow* optical_flow)
    {
        cv::Ptr<cv::SparsePyrLKOpticalFlow> algorithm = optical_flow->value.dynamicCast<cv::SparsePyrLKOpticalFlow>();
        if (algorithm.empty())
        {
            throw std::invalid_argument(std::string(api_name) + ": optical_flow has the wrong concrete type");
        }
        return algorithm;
    }

    template <typename TAlgorithm>
    int create_dense_optical_flow(
        const char* api_name,
        cv::Ptr<TAlgorithm> algorithm,
        jyppx_ocv_dense_optical_flow** optical_flow)
    {
        if (optical_flow == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "optical_flow");
        }
        *optical_flow = nullptr;
        jyppx_ocv_dense_optical_flow* created = new (std::nothrow) jyppx_ocv_dense_optical_flow();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        created->value = algorithm;
        *optical_flow = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_sparse_optical_flow(
        const char* api_name,
        cv::Ptr<cv::SparsePyrLKOpticalFlow> algorithm,
        jyppx_ocv_sparse_optical_flow** optical_flow)
    {
        if (optical_flow == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "optical_flow");
        }
        *optical_flow = nullptr;
        jyppx_ocv_sparse_optical_flow* created = new (std::nothrow) jyppx_ocv_sparse_optical_flow();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        created->value = algorithm;
        *optical_flow = created;
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

void jyppx_ocv_dense_optical_flow_release_handle(jyppx_ocv_dense_optical_flow* optical_flow)
{
    delete optical_flow;
}

int jyppx_ocv_dense_optical_flow_calc(
    jyppx_ocv_dense_optical_flow* optical_flow,
    const jyppx_ocv_mat* first,
    const jyppx_ocv_mat* second,
    jyppx_ocv_mat* flow)
{
    constexpr const char* api_name = "jyppx_ocv_dense_optical_flow_calc";
    return video_object_guarded(api_name, [&]()
    {
        int status = validate_dense_optical_flow(api_name, optical_flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, first, "first");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, second, "second");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, flow, "flow");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        optical_flow->value->calc(
            opencv_csharp_native::mat_value(first),
            opencv_csharp_native::mat_value(second),
            opencv_csharp_native::mat_value(flow));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_dense_optical_flow_collect_garbage(jyppx_ocv_dense_optical_flow* optical_flow)
{
    constexpr const char* api_name = "jyppx_ocv_dense_optical_flow_collect_garbage";
    return video_object_guarded(api_name, [&]()
    {
        int status = validate_dense_optical_flow(api_name, optical_flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        optical_flow->value->collectGarbage();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

void jyppx_ocv_sparse_optical_flow_release_handle(jyppx_ocv_sparse_optical_flow* optical_flow)
{
    delete optical_flow;
}

int jyppx_ocv_sparse_optical_flow_calc(
    jyppx_ocv_sparse_optical_flow* optical_flow,
    const jyppx_ocv_mat* previous_image,
    const jyppx_ocv_mat* next_image,
    const jyppx_ocv_video_point2f* previous_points,
    int point_count,
    jyppx_ocv_video_point2f* next_points,
    unsigned char* status_values,
    float* error)
{
    constexpr const char* api_name = "jyppx_ocv_sparse_optical_flow_calc";
    return video_object_guarded(api_name, [&]()
    {
        int status = validate_sparse_optical_flow(api_name, optical_flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, previous_image, "previous_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, next_image, "next_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_points(api_name, previous_points, point_count, next_points, 1, next_points, status_values, error);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Point2f> previous;
        std::vector<cv::Point2f> next;
        previous.reserve(static_cast<size_t>(point_count));
        next.reserve(static_cast<size_t>(point_count));
        for (int i = 0; i < point_count; ++i)
        {
            previous.push_back(to_cv_point(previous_points[i]));
            next.push_back(to_cv_point(next_points[i]));
        }
        std::vector<unsigned char> native_status;
        std::vector<float> native_error;
        optical_flow->value->calc(
            opencv_csharp_native::mat_value(previous_image),
            opencv_csharp_native::mat_value(next_image),
            previous,
            next,
            native_status,
            native_error);
        if (next.size() != static_cast<size_t>(point_count) || native_status.size() != static_cast<size_t>(point_count))
        {
            throw std::runtime_error("Sparse optical flow returned an unexpected point or status count.");
        }
        for (int i = 0; i < point_count; ++i)
        {
            next_points[i] = from_cv_point(next[static_cast<size_t>(i)]);
            status_values[i] = native_status[static_cast<size_t>(i)];
            error[i] = native_error.empty() ? 0.0F : native_error[static_cast<size_t>(i)];
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_farneback_optical_flow_create(
    int num_levels,
    double pyramid_scale,
    int fast_pyramids,
    int window_size,
    int num_iterations,
    int polynomial_neighborhood,
    double polynomial_sigma,
    int flags,
    jyppx_ocv_dense_optical_flow** optical_flow)
{
    constexpr const char* api_name = "jyppx_ocv_farneback_optical_flow_create";
    return video_object_guarded(api_name, [&]()
    {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_dense_optical_flow(
            api_name,
            cv::FarnebackOpticalFlow::create(num_levels, pyramid_scale, fast_pyramids != 0, window_size, num_iterations, polynomial_neighborhood, polynomial_sigma, flags),
            optical_flow);
#else
        (void)num_levels; (void)pyramid_scale; (void)fast_pyramids; (void)window_size; (void)num_iterations;
        (void)polynomial_neighborhood; (void)polynomial_sigma; (void)flags;
        if (optical_flow == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "optical_flow"); }
        *optical_flow = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_variational_refinement_create(jyppx_ocv_dense_optical_flow** optical_flow)
{
    constexpr const char* api_name = "jyppx_ocv_variational_refinement_create";
    return video_object_guarded(api_name, [&]()
    {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_dense_optical_flow(api_name, cv::VariationalRefinement::create(), optical_flow);
#else
        if (optical_flow == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "optical_flow"); }
        *optical_flow = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_variational_refinement_calc_uv(
    jyppx_ocv_dense_optical_flow* optical_flow,
    const jyppx_ocv_mat* first,
    const jyppx_ocv_mat* second,
    jyppx_ocv_mat* flow_u,
    jyppx_ocv_mat* flow_v)
{
    constexpr const char* api_name = "jyppx_ocv_variational_refinement_calc_uv";
    return video_object_guarded(api_name, [&]()
    {
        int status = validate_dense_optical_flow(api_name, optical_flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, first, "first"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, second, "second"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, flow_u, "flow_u"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, flow_v, "flow_v"); if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        require_dense_algorithm<cv::VariationalRefinement>(api_name, optical_flow)->calcUV(
            opencv_csharp_native::mat_value(first), opencv_csharp_native::mat_value(second),
            opencv_csharp_native::mat_value(flow_u), opencv_csharp_native::mat_value(flow_v));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_dis_optical_flow_create(int preset, jyppx_ocv_dense_optical_flow** optical_flow)
{
    constexpr const char* api_name = "jyppx_ocv_dis_optical_flow_create";
    return video_object_guarded(api_name, [&]()
    {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_dense_optical_flow(api_name, cv::DISOpticalFlow::create(preset), optical_flow);
#else
        (void)preset;
        if (optical_flow == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "optical_flow"); }
        *optical_flow = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_create(
    int window_width,
    int window_height,
    int max_level,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int flags,
    double min_eigenvalue_threshold,
    jyppx_ocv_sparse_optical_flow** optical_flow)
{
    constexpr const char* api_name = "jyppx_ocv_sparse_pyr_lk_optical_flow_create";
    return video_object_guarded(api_name, [&]()
    {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_sparse_optical_flow(
            api_name,
            cv::SparsePyrLKOpticalFlow::create(
                cv::Size(window_width, window_height), max_level,
                cv::TermCriteria(criteria_type, criteria_max_count, criteria_epsilon), flags, min_eigenvalue_threshold),
            optical_flow);
#else
        (void)window_width; (void)window_height; (void)max_level; (void)criteria_type; (void)criteria_max_count;
        (void)criteria_epsilon; (void)flags; (void)min_eigenvalue_threshold;
        if (optical_flow == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "optical_flow"); }
        *optical_flow = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

#if defined(OPENCV_CSHARP_HAS_OPENCV)
namespace
{
    template <typename TAlgorithm, typename TValue, typename TGetter>
    int get_dense_property(
        const char* api_name,
        const jyppx_ocv_dense_optical_flow* optical_flow,
        int property_id,
        TValue* value,
        TGetter&& getter)
    {
        return video_object_guarded(api_name, [&]()
        {
            int status = validate_dense_optical_flow(api_name, optical_flow);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); }
            cv::Ptr<TAlgorithm> algorithm = require_dense_algorithm<TAlgorithm>(api_name, optical_flow);
            *value = getter(*algorithm, property_id);
            return OPENCV_CSHARP_STATUS_OK;
        });
    }

    template <typename TAlgorithm, typename TValue, typename TSetter>
    int set_dense_property(
        const char* api_name,
        jyppx_ocv_dense_optical_flow* optical_flow,
        int property_id,
        TValue value,
        TSetter&& setter)
    {
        return video_object_guarded(api_name, [&]()
        {
            int status = validate_dense_optical_flow(api_name, optical_flow);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            cv::Ptr<TAlgorithm> algorithm = require_dense_algorithm<TAlgorithm>(api_name, optical_flow);
            setter(*algorithm, property_id, value);
            return OPENCV_CSHARP_STATUS_OK;
        });
    }

    template <typename TValue, typename TGetter>
    int get_sparse_property(
        const char* api_name,
        const jyppx_ocv_sparse_optical_flow* optical_flow,
        int property_id,
        TValue* value,
        TGetter&& getter)
    {
        return video_object_guarded(api_name, [&]()
        {
            int status = validate_sparse_optical_flow(api_name, optical_flow);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); }
            cv::Ptr<cv::SparsePyrLKOpticalFlow> algorithm = require_sparse_pyr_lk(api_name, optical_flow);
            *value = getter(*algorithm, property_id);
            return OPENCV_CSHARP_STATUS_OK;
        });
    }

    template <typename TValue, typename TSetter>
    int set_sparse_property(
        const char* api_name,
        jyppx_ocv_sparse_optical_flow* optical_flow,
        int property_id,
        TValue value,
        TSetter&& setter)
    {
        return video_object_guarded(api_name, [&]()
        {
            int status = validate_sparse_optical_flow(api_name, optical_flow);
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            cv::Ptr<cv::SparsePyrLKOpticalFlow> algorithm = require_sparse_pyr_lk(api_name, optical_flow);
            setter(*algorithm, property_id, value);
            return OPENCV_CSHARP_STATUS_OK;
        });
    }

    int invalid_property_id(const char* api_name)
    {
        throw std::invalid_argument(std::string(api_name) + ": property_id");
    }
}

int jyppx_ocv_farneback_optical_flow_get_int_property(const jyppx_ocv_dense_optical_flow* optical_flow, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_farneback_optical_flow_get_int_property";
    return get_dense_property<cv::FarnebackOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::FarnebackOpticalFlow& value_ref, int id)
    {
        switch (id) { case 0: return value_ref.getNumLevels(); case 1: return value_ref.getWinSize(); case 2: return value_ref.getNumIters(); case 3: return value_ref.getPolyN(); case 4: return value_ref.getFlags(); default: return invalid_property_id(api_name); }
    });
}

int jyppx_ocv_farneback_optical_flow_set_int_property(jyppx_ocv_dense_optical_flow* optical_flow, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_farneback_optical_flow_set_int_property";
    return set_dense_property<cv::FarnebackOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::FarnebackOpticalFlow& value_ref, int id, int input)
    {
        switch (id) { case 0: value_ref.setNumLevels(input); return; case 1: value_ref.setWinSize(input); return; case 2: value_ref.setNumIters(input); return; case 3: value_ref.setPolyN(input); return; case 4: value_ref.setFlags(input); return; default: invalid_property_id(api_name); }
    });
}

int jyppx_ocv_farneback_optical_flow_get_double_property(const jyppx_ocv_dense_optical_flow* optical_flow, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_farneback_optical_flow_get_double_property";
    return get_dense_property<cv::FarnebackOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::FarnebackOpticalFlow& value_ref, int id)
    {
        switch (id) { case 0: return value_ref.getPyrScale(); case 1: return value_ref.getPolySigma(); default: invalid_property_id(api_name); return 0.0; }
    });
}

int jyppx_ocv_farneback_optical_flow_set_double_property(jyppx_ocv_dense_optical_flow* optical_flow, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_farneback_optical_flow_set_double_property";
    return set_dense_property<cv::FarnebackOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::FarnebackOpticalFlow& value_ref, int id, double input)
    {
        switch (id) { case 0: value_ref.setPyrScale(input); return; case 1: value_ref.setPolySigma(input); return; default: invalid_property_id(api_name); }
    });
}

int jyppx_ocv_farneback_optical_flow_get_bool_property(const jyppx_ocv_dense_optical_flow* optical_flow, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_farneback_optical_flow_get_bool_property";
    return get_dense_property<cv::FarnebackOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::FarnebackOpticalFlow& value_ref, int id)
    {
        if (id != 0) { return invalid_property_id(api_name); }
        return value_ref.getFastPyramids() ? 1 : 0;
    });
}

int jyppx_ocv_farneback_optical_flow_set_bool_property(jyppx_ocv_dense_optical_flow* optical_flow, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_farneback_optical_flow_set_bool_property";
    return set_dense_property<cv::FarnebackOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::FarnebackOpticalFlow& value_ref, int id, int input)
    {
        if (id != 0) { invalid_property_id(api_name); }
        value_ref.setFastPyramids(input != 0);
    });
}

int jyppx_ocv_variational_refinement_get_int_property(const jyppx_ocv_dense_optical_flow* optical_flow, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_variational_refinement_get_int_property";
    return get_dense_property<cv::VariationalRefinement>(api_name, optical_flow, property_id, value, [api_name](cv::VariationalRefinement& value_ref, int id)
    {
        switch (id) { case 0: return value_ref.getFixedPointIterations(); case 1: return value_ref.getSorIterations(); default: return invalid_property_id(api_name); }
    });
}

int jyppx_ocv_variational_refinement_set_int_property(jyppx_ocv_dense_optical_flow* optical_flow, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_variational_refinement_set_int_property";
    return set_dense_property<cv::VariationalRefinement>(api_name, optical_flow, property_id, value, [api_name](cv::VariationalRefinement& value_ref, int id, int input)
    {
        switch (id) { case 0: value_ref.setFixedPointIterations(input); return; case 1: value_ref.setSorIterations(input); return; default: invalid_property_id(api_name); }
    });
}

int jyppx_ocv_variational_refinement_get_float_property(const jyppx_ocv_dense_optical_flow* optical_flow, int property_id, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_variational_refinement_get_float_property";
    return get_dense_property<cv::VariationalRefinement>(api_name, optical_flow, property_id, value, [api_name](cv::VariationalRefinement& value_ref, int id)
    {
        switch (id) { case 0: return value_ref.getOmega(); case 1: return value_ref.getAlpha(); case 2: return value_ref.getDelta(); case 3: return value_ref.getGamma(); case 4: return value_ref.getEpsilon(); default: invalid_property_id(api_name); return 0.0F; }
    });
}

int jyppx_ocv_variational_refinement_set_float_property(jyppx_ocv_dense_optical_flow* optical_flow, int property_id, float value)
{
    constexpr const char* api_name = "jyppx_ocv_variational_refinement_set_float_property";
    return set_dense_property<cv::VariationalRefinement>(api_name, optical_flow, property_id, value, [api_name](cv::VariationalRefinement& value_ref, int id, float input)
    {
        switch (id) { case 0: value_ref.setOmega(input); return; case 1: value_ref.setAlpha(input); return; case 2: value_ref.setDelta(input); return; case 3: value_ref.setGamma(input); return; case 4: value_ref.setEpsilon(input); return; default: invalid_property_id(api_name); }
    });
}

int jyppx_ocv_dis_optical_flow_get_int_property(const jyppx_ocv_dense_optical_flow* optical_flow, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_dis_optical_flow_get_int_property";
    return get_dense_property<cv::DISOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::DISOpticalFlow& value_ref, int id)
    {
        switch (id) { case 0: return value_ref.getFinestScale(); case 1: return value_ref.getCoarsestScale(); case 2: return value_ref.getPatchSize(); case 3: return value_ref.getPatchStride(); case 4: return value_ref.getGradientDescentIterations(); case 5: return value_ref.getVariationalRefinementIterations(); default: return invalid_property_id(api_name); }
    });
}

int jyppx_ocv_dis_optical_flow_set_int_property(jyppx_ocv_dense_optical_flow* optical_flow, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_dis_optical_flow_set_int_property";
    return set_dense_property<cv::DISOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::DISOpticalFlow& value_ref, int id, int input)
    {
        switch (id) { case 0: value_ref.setFinestScale(input); return; case 1: value_ref.setCoarsestScale(input); return; case 2: value_ref.setPatchSize(input); return; case 3: value_ref.setPatchStride(input); return; case 4: value_ref.setGradientDescentIterations(input); return; case 5: value_ref.setVariationalRefinementIterations(input); return; default: invalid_property_id(api_name); }
    });
}

int jyppx_ocv_dis_optical_flow_get_float_property(const jyppx_ocv_dense_optical_flow* optical_flow, int property_id, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_dis_optical_flow_get_float_property";
    return get_dense_property<cv::DISOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::DISOpticalFlow& value_ref, int id)
    {
        switch (id) { case 0: return value_ref.getVariationalRefinementAlpha(); case 1: return value_ref.getVariationalRefinementDelta(); case 2: return value_ref.getVariationalRefinementGamma(); case 3: return value_ref.getVariationalRefinementEpsilon(); default: invalid_property_id(api_name); return 0.0F; }
    });
}

int jyppx_ocv_dis_optical_flow_set_float_property(jyppx_ocv_dense_optical_flow* optical_flow, int property_id, float value)
{
    constexpr const char* api_name = "jyppx_ocv_dis_optical_flow_set_float_property";
    return set_dense_property<cv::DISOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::DISOpticalFlow& value_ref, int id, float input)
    {
        switch (id) { case 0: value_ref.setVariationalRefinementAlpha(input); return; case 1: value_ref.setVariationalRefinementDelta(input); return; case 2: value_ref.setVariationalRefinementGamma(input); return; case 3: value_ref.setVariationalRefinementEpsilon(input); return; default: invalid_property_id(api_name); }
    });
}

int jyppx_ocv_dis_optical_flow_get_bool_property(const jyppx_ocv_dense_optical_flow* optical_flow, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_dis_optical_flow_get_bool_property";
    return get_dense_property<cv::DISOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::DISOpticalFlow& value_ref, int id)
    {
        switch (id) { case 0: return value_ref.getUseMeanNormalization() ? 1 : 0; case 1: return value_ref.getUseSpatialPropagation() ? 1 : 0; default: return invalid_property_id(api_name); }
    });
}

int jyppx_ocv_dis_optical_flow_set_bool_property(jyppx_ocv_dense_optical_flow* optical_flow, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_dis_optical_flow_set_bool_property";
    return set_dense_property<cv::DISOpticalFlow>(api_name, optical_flow, property_id, value, [api_name](cv::DISOpticalFlow& value_ref, int id, int input)
    {
        switch (id) { case 0: value_ref.setUseMeanNormalization(input != 0); return; case 1: value_ref.setUseSpatialPropagation(input != 0); return; default: invalid_property_id(api_name); }
    });
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property(const jyppx_ocv_sparse_optical_flow* optical_flow, int* width, int* height)
{
    constexpr const char* api_name = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property";
    return video_object_guarded(api_name, [&]()
    {
        int status = validate_sparse_optical_flow(api_name, optical_flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (width == nullptr || height == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "width/height"); }
        cv::Size value = require_sparse_pyr_lk(api_name, optical_flow)->getWinSize();
        *width = value.width; *height = value.height;
        return OPENCV_CSHARP_STATUS_OK;
    });
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_set_size_property(jyppx_ocv_sparse_optical_flow* optical_flow, int width, int height)
{
    constexpr const char* api_name = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_size_property";
    return video_object_guarded(api_name, [&]()
    {
        int status = validate_sparse_optical_flow(api_name, optical_flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        require_sparse_pyr_lk(api_name, optical_flow)->setWinSize(cv::Size(width, height));
        return OPENCV_CSHARP_STATUS_OK;
    });
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_get_int_property(const jyppx_ocv_sparse_optical_flow* optical_flow, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_int_property";
    return get_sparse_property(api_name, optical_flow, property_id, value, [api_name](cv::SparsePyrLKOpticalFlow& value_ref, int id)
    {
        switch (id) { case 0: return value_ref.getMaxLevel(); case 1: return value_ref.getFlags(); default: return invalid_property_id(api_name); }
    });
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_set_int_property(jyppx_ocv_sparse_optical_flow* optical_flow, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_int_property";
    return set_sparse_property(api_name, optical_flow, property_id, value, [api_name](cv::SparsePyrLKOpticalFlow& value_ref, int id, int input)
    {
        switch (id) { case 0: value_ref.setMaxLevel(input); return; case 1: value_ref.setFlags(input); return; default: invalid_property_id(api_name); }
    });
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria(const jyppx_ocv_sparse_optical_flow* optical_flow, int* type, int* max_count, double* epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria";
    return video_object_guarded(api_name, [&]()
    {
        int status = validate_sparse_optical_flow(api_name, optical_flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (type == nullptr || max_count == nullptr || epsilon == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "criteria outputs"); }
        cv::TermCriteria value = require_sparse_pyr_lk(api_name, optical_flow)->getTermCriteria();
        *type = value.type; *max_count = value.maxCount; *epsilon = value.epsilon;
        return OPENCV_CSHARP_STATUS_OK;
    });
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_set_term_criteria(jyppx_ocv_sparse_optical_flow* optical_flow, int type, int max_count, double epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_term_criteria";
    return video_object_guarded(api_name, [&]()
    {
        int status = validate_sparse_optical_flow(api_name, optical_flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::TermCriteria criteria(type, max_count, epsilon);
        require_sparse_pyr_lk(api_name, optical_flow)->setTermCriteria(criteria);
        return OPENCV_CSHARP_STATUS_OK;
    });
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_get_min_eig_threshold(const jyppx_ocv_sparse_optical_flow* optical_flow, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_sparse_pyr_lk_optical_flow_get_min_eig_threshold";
    return get_sparse_property(api_name, optical_flow, 0, value, [](cv::SparsePyrLKOpticalFlow& value_ref, int) { return value_ref.getMinEigThreshold(); });
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_set_min_eig_threshold(jyppx_ocv_sparse_optical_flow* optical_flow, double value)
{
    constexpr const char* api_name = "jyppx_ocv_sparse_pyr_lk_optical_flow_set_min_eig_threshold";
    return set_sparse_property(api_name, optical_flow, 0, value, [](cv::SparsePyrLKOpticalFlow& value_ref, int, double input) { value_ref.setMinEigThreshold(input); });
}

#else
namespace
{
    template <typename TValue>
    int get_dense_not_linked(const char* api_name, const jyppx_ocv_dense_optical_flow* optical_flow, TValue* value)
    {
        int status = validate_dense_optical_flow(api_name, optical_flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); }
        *value = TValue{};
        return opencv_csharp_native::set_not_linked(api_name);
    }

    template <typename TValue>
    int set_dense_not_linked(const char* api_name, jyppx_ocv_dense_optical_flow* optical_flow, TValue)
    {
        int status = validate_dense_optical_flow(api_name, optical_flow);
        return status == OPENCV_CSHARP_STATUS_OK ? opencv_csharp_native::set_not_linked(api_name) : status;
    }

    template <typename TValue>
    int get_sparse_not_linked(const char* api_name, const jyppx_ocv_sparse_optical_flow* optical_flow, TValue* value)
    {
        int status = validate_sparse_optical_flow(api_name, optical_flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); }
        *value = TValue{};
        return opencv_csharp_native::set_not_linked(api_name);
    }

    template <typename TValue>
    int set_sparse_not_linked(const char* api_name, jyppx_ocv_sparse_optical_flow* optical_flow, TValue)
    {
        int status = validate_sparse_optical_flow(api_name, optical_flow);
        return status == OPENCV_CSHARP_STATUS_OK ? opencv_csharp_native::set_not_linked(api_name) : status;
    }
}

#define VIDEO_DENSE_GET_STUB(name, type) int name(const jyppx_ocv_dense_optical_flow* optical_flow, int property_id, type* value) { (void)property_id; return get_dense_not_linked(#name, optical_flow, value); }
#define VIDEO_DENSE_SET_STUB(name, type) int name(jyppx_ocv_dense_optical_flow* optical_flow, int property_id, type value) { (void)property_id; return set_dense_not_linked(#name, optical_flow, value); }
#define VIDEO_SPARSE_GET_STUB(name, type) int name(const jyppx_ocv_sparse_optical_flow* optical_flow, int property_id, type* value) { (void)property_id; return get_sparse_not_linked(#name, optical_flow, value); }
#define VIDEO_SPARSE_SET_STUB(name, type) int name(jyppx_ocv_sparse_optical_flow* optical_flow, int property_id, type value) { (void)property_id; return set_sparse_not_linked(#name, optical_flow, value); }

VIDEO_DENSE_GET_STUB(jyppx_ocv_farneback_optical_flow_get_int_property, int)
VIDEO_DENSE_SET_STUB(jyppx_ocv_farneback_optical_flow_set_int_property, int)
VIDEO_DENSE_GET_STUB(jyppx_ocv_farneback_optical_flow_get_double_property, double)
VIDEO_DENSE_SET_STUB(jyppx_ocv_farneback_optical_flow_set_double_property, double)
VIDEO_DENSE_GET_STUB(jyppx_ocv_farneback_optical_flow_get_bool_property, int)
VIDEO_DENSE_SET_STUB(jyppx_ocv_farneback_optical_flow_set_bool_property, int)
VIDEO_DENSE_GET_STUB(jyppx_ocv_variational_refinement_get_int_property, int)
VIDEO_DENSE_SET_STUB(jyppx_ocv_variational_refinement_set_int_property, int)
VIDEO_DENSE_GET_STUB(jyppx_ocv_variational_refinement_get_float_property, float)
VIDEO_DENSE_SET_STUB(jyppx_ocv_variational_refinement_set_float_property, float)
VIDEO_DENSE_GET_STUB(jyppx_ocv_dis_optical_flow_get_int_property, int)
VIDEO_DENSE_SET_STUB(jyppx_ocv_dis_optical_flow_set_int_property, int)
VIDEO_DENSE_GET_STUB(jyppx_ocv_dis_optical_flow_get_float_property, float)
VIDEO_DENSE_SET_STUB(jyppx_ocv_dis_optical_flow_set_float_property, float)
VIDEO_DENSE_GET_STUB(jyppx_ocv_dis_optical_flow_get_bool_property, int)
VIDEO_DENSE_SET_STUB(jyppx_ocv_dis_optical_flow_set_bool_property, int)
VIDEO_SPARSE_GET_STUB(jyppx_ocv_sparse_pyr_lk_optical_flow_get_int_property, int)
VIDEO_SPARSE_SET_STUB(jyppx_ocv_sparse_pyr_lk_optical_flow_set_int_property, int)

int jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property(const jyppx_ocv_sparse_optical_flow* optical_flow, int* width, int* height)
{
    int status = validate_sparse_optical_flow("jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property", optical_flow);
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    if (width == nullptr || height == nullptr) { return opencv_csharp_native::set_invalid_argument("jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property", "width/height"); }
    *width = 0; *height = 0;
    return opencv_csharp_native::set_not_linked("jyppx_ocv_sparse_pyr_lk_optical_flow_get_size_property");
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_set_size_property(jyppx_ocv_sparse_optical_flow* optical_flow, int width, int height)
{
    (void)width; (void)height;
    return set_sparse_not_linked("jyppx_ocv_sparse_pyr_lk_optical_flow_set_size_property", optical_flow, 0);
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria(const jyppx_ocv_sparse_optical_flow* optical_flow, int* type, int* max_count, double* epsilon)
{
    int status = validate_sparse_optical_flow("jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria", optical_flow);
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    if (type == nullptr || max_count == nullptr || epsilon == nullptr) { return opencv_csharp_native::set_invalid_argument("jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria", "criteria outputs"); }
    *type = 0; *max_count = 0; *epsilon = 0.0;
    return opencv_csharp_native::set_not_linked("jyppx_ocv_sparse_pyr_lk_optical_flow_get_term_criteria");
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_set_term_criteria(jyppx_ocv_sparse_optical_flow* optical_flow, int type, int max_count, double epsilon)
{
    (void)type; (void)max_count; (void)epsilon;
    return set_sparse_not_linked("jyppx_ocv_sparse_pyr_lk_optical_flow_set_term_criteria", optical_flow, 0);
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_get_min_eig_threshold(const jyppx_ocv_sparse_optical_flow* optical_flow, double* value)
{
    return get_sparse_not_linked("jyppx_ocv_sparse_pyr_lk_optical_flow_get_min_eig_threshold", optical_flow, value);
}

int jyppx_ocv_sparse_pyr_lk_optical_flow_set_min_eig_threshold(jyppx_ocv_sparse_optical_flow* optical_flow, double value)
{
    return set_sparse_not_linked("jyppx_ocv_sparse_pyr_lk_optical_flow_set_min_eig_threshold", optical_flow, value);
}

#undef VIDEO_DENSE_GET_STUB
#undef VIDEO_DENSE_SET_STUB
#undef VIDEO_SPARSE_GET_STUB
#undef VIDEO_SPARSE_SET_STUB
#endif

int jyppx_ocv_video_compute_ecc(
    const jyppx_ocv_mat* template_image,
    const jyppx_ocv_mat* input_image,
    const jyppx_ocv_mat* input_mask,
    double* result)
{
    constexpr const char* api_name = "jyppx_ocv_video_compute_ecc";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, template_image, "template_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, input_image, "input_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat mask = input_mask == nullptr
            ? cv::Mat()
            : opencv_csharp_native::mat_value(input_mask);
        *result = cv::computeECC(
            opencv_csharp_native::mat_value(template_image),
            opencv_csharp_native::mat_value(input_image),
            mask);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)input_mask;
        *result = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_find_transform_ecc(
    const jyppx_ocv_mat* template_image,
    const jyppx_ocv_mat* input_image,
    jyppx_ocv_mat* warp_matrix,
    int motion_type,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    const jyppx_ocv_mat* input_mask,
    int gaussian_filter_size,
    double* result)
{
    constexpr const char* api_name = "jyppx_ocv_video_find_transform_ecc";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, template_image, "template_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, input_image, "input_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, warp_matrix, "warp_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat mask = input_mask == nullptr
            ? cv::Mat()
            : opencv_csharp_native::mat_value(input_mask);
        *result = cv::findTransformECC(
            opencv_csharp_native::mat_value(template_image),
            opencv_csharp_native::mat_value(input_image),
            opencv_csharp_native::mat_value(warp_matrix),
            motion_type,
            to_criteria(criteria_type, criteria_max_count, criteria_epsilon),
            mask,
            gaussian_filter_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)motion_type; (void)criteria_type; (void)criteria_max_count; (void)criteria_epsilon;
        (void)input_mask; (void)gaussian_filter_size;
        *result = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_find_transform_ecc_with_mask(
    const jyppx_ocv_mat* template_image,
    const jyppx_ocv_mat* input_image,
    const jyppx_ocv_mat* template_mask,
    const jyppx_ocv_mat* input_mask,
    jyppx_ocv_mat* warp_matrix,
    int motion_type,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int gaussian_filter_size,
    double* result)
{
    constexpr const char* api_name = "jyppx_ocv_video_find_transform_ecc_with_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, template_image, "template_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, input_image, "input_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, template_mask, "template_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, input_mask, "input_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, warp_matrix, "warp_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *result = cv::findTransformECCWithMask(
            opencv_csharp_native::mat_value(template_image),
            opencv_csharp_native::mat_value(input_image),
            opencv_csharp_native::mat_value(template_mask),
            opencv_csharp_native::mat_value(input_mask),
            opencv_csharp_native::mat_value(warp_matrix),
            motion_type,
            to_criteria(criteria_type, criteria_max_count, criteria_epsilon),
            gaussian_filter_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)motion_type; (void)criteria_type; (void)criteria_max_count; (void)criteria_epsilon;
        (void)gaussian_filter_size;
        *result = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_ecc_parameters_get_default(
    int* motion_type,
    int* criteria_type,
    int* criteria_max_count,
    double* criteria_epsilon,
    int* gaussian_filter_size,
    int* level_count,
    int* interpolation)
{
    constexpr const char* api_name = "jyppx_ocv_video_ecc_parameters_get_default";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (motion_type == nullptr || criteria_type == nullptr || criteria_max_count == nullptr ||
            criteria_epsilon == nullptr || gaussian_filter_size == nullptr || level_count == nullptr ||
            interpolation == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameter outputs");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::ECCParameters parameters;
        *motion_type = parameters.motionType;
        *criteria_type = parameters.criteria.type;
        *criteria_max_count = parameters.criteria.maxCount;
        *criteria_epsilon = parameters.criteria.epsilon;
        *gaussian_filter_size = parameters.gaussFiltSize;
        *level_count = parameters.nlevels;
        *interpolation = parameters.interpolation;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *motion_type = 0; *criteria_type = 0; *criteria_max_count = 0; *criteria_epsilon = 0.0;
        *gaussian_filter_size = 0; *level_count = 0; *interpolation = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_find_transform_ecc_multi_scale(
    const jyppx_ocv_mat* reference_image,
    const jyppx_ocv_mat* sample_image,
    jyppx_ocv_mat* warp_matrix,
    int motion_type,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    const int* iterations_per_level,
    int iteration_count,
    int gaussian_filter_size,
    int level_count,
    int interpolation,
    const jyppx_ocv_mat* reference_mask,
    const jyppx_ocv_mat* sample_mask,
    double* result)
{
    constexpr const char* api_name = "jyppx_ocv_video_find_transform_ecc_multi_scale";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, reference_image, "reference_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, sample_image, "sample_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, warp_matrix, "warp_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_iteration_schedule(api_name, iterations_per_level, iteration_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::ECCParameters parameters;
        parameters.motionType = motion_type;
        parameters.criteria = to_criteria(criteria_type, criteria_max_count, criteria_epsilon);
        if (iteration_count > 0)
        {
            parameters.itersPerLevel.assign(iterations_per_level, iterations_per_level + iteration_count);
        }
        parameters.gaussFiltSize = gaussian_filter_size;
        parameters.nlevels = level_count;
        parameters.interpolation = interpolation;
        cv::Mat reference_mask_input = reference_mask == nullptr
            ? cv::Mat()
            : opencv_csharp_native::mat_value(reference_mask);
        cv::Mat sample_mask_input = sample_mask == nullptr
            ? cv::Mat()
            : opencv_csharp_native::mat_value(sample_mask);
        *result = cv::findTransformECCMultiScale(
            opencv_csharp_native::mat_value(reference_image),
            opencv_csharp_native::mat_value(sample_image),
            opencv_csharp_native::mat_value(warp_matrix),
            parameters,
            reference_mask_input,
            sample_mask_input);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)motion_type; (void)criteria_type; (void)criteria_max_count; (void)criteria_epsilon;
        (void)iterations_per_level; (void)iteration_count; (void)gaussian_filter_size; (void)level_count;
        (void)interpolation; (void)reference_mask; (void)sample_mask;
        *result = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_video_tracker_release_handle(jyppx_ocv_video_tracker* tracker)
{
    delete tracker;
}

int jyppx_ocv_video_tracker_init(
    jyppx_ocv_video_tracker* tracker,
    const jyppx_ocv_mat* image,
    jyppx_ocv_video_rect bounding_box)
{
    constexpr const char* api_name = "jyppx_ocv_video_tracker_init";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_video_tracker(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        tracker->initialized = false;
        tracker->value->init(opencv_csharp_native::mat_value(image), to_cv_rect(bounding_box));
        tracker->initialized = true;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)bounding_box;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_tracker_update(
    jyppx_ocv_video_tracker* tracker,
    const jyppx_ocv_mat* image,
    jyppx_ocv_video_rect* bounding_box,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_video_tracker_update";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_video_tracker(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_rect(api_name, bounding_box, "bounding_box");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!tracker->initialized)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tracker is not initialized");
        }
        cv::Rect updated = to_cv_rect(*bounding_box);
        bool found = tracker->value->update(opencv_csharp_native::mat_value(image), updated);
        if (found)
        {
            *bounding_box = from_cv_rect(updated);
        }
        *result = found ? 1 : 0;
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

int jyppx_ocv_video_tracker_get_tracking_score(
    const jyppx_ocv_video_tracker* tracker,
    float* score)
{
    constexpr const char* api_name = "jyppx_ocv_video_tracker_get_tracking_score";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_video_tracker(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, score, "score");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *score = tracker->value->getTrackingScore();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *score = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_tracker_mil_get_default_params(
    jyppx_ocv_video_tracker_mil_params* parameters)
{
    constexpr const char* api_name = "jyppx_ocv_video_tracker_mil_get_default_params";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *parameters = from_tracker_mil_params(cv::TrackerMIL::Params());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *parameters = jyppx_ocv_video_tracker_mil_params{};
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_video_tracker_mil_create(
    const jyppx_ocv_video_tracker_mil_params* parameters,
    jyppx_ocv_video_tracker** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_video_tracker_mil_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (parameters == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameters");
        }
        if (tracker == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tracker");
        }
        *tracker = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_video_tracker* created = new (std::nothrow) jyppx_ocv_video_tracker();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        try
        {
            created->value = cv::TrackerMIL::create(to_tracker_mil_params(*parameters));
            if (created->value.empty())
            {
                delete created;
                return opencv_csharp_native::set_invalid_argument(api_name, "OpenCV returned an empty TrackerMIL");
            }
            *tracker = created;
            return OPENCV_CSHARP_STATUS_OK;
        }
        catch (...)
        {
            delete created;
            throw;
        }
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

