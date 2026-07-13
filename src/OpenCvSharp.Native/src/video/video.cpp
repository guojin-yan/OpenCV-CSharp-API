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

