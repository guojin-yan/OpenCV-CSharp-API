#include "open_cv_sharp/ximgproc/ximgproc.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "../stereo/stereo_handles.h"
#include "ximgproc_handles.h"

#include <cstring>
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

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_count_fill_args(
        const char* api_name,
        const void* buffer,
        int capacity,
        const char* buffer_name,
        const char* capacity_name)
    {
        if (capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, capacity_name);
        }

        if (buffer == nullptr && capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, buffer_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
    template <typename THandle, typename TNative>
    int create_handle(
        const char* api_name,
        const cv::Ptr<TNative>& native,
        THandle** handle,
        const char* argument_name)
    {
        if (handle == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        *handle = nullptr;
        THandle* created = new (std::nothrow) THandle();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *handle = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    template <typename THandle>
    int validate_handle(const char* api_name, const THandle* handle, const char* argument_name)
    {
        if (handle == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (handle->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::Mat get_vec4f_mat_from_flat(const float* values, int value_count)
    {
        cv::Mat mat(value_count, 1, CV_32FC4);
        if (value_count > 0)
        {
            std::memcpy(mat.ptr<float>(), values, static_cast<size_t>(value_count) * 4U * sizeof(float));
        }

        return mat;
    }

    cv::Rect to_rect(const jyppx_ocv_ximgproc_rect* rect)
    {
        return rect == nullptr
            ? cv::Rect()
            : cv::Rect(rect->x, rect->y, rect->width, rect->height);
    }

    void from_rect(const cv::Rect& source, jyppx_ocv_ximgproc_rect* destination)
    {
        destination->x = source.x;
        destination->y = source.y;
        destination->width = source.width;
        destination->height = source.height;
    }

    cv::ximgproc::EdgeDrawing::Params to_edge_drawing_params(const jyppx_ocv_ximgproc_edge_drawing_params& source)
    {
        cv::ximgproc::EdgeDrawing::Params result;
        result.PFmode = source.pf_mode != 0;
        result.EdgeDetectionOperator = source.edge_detection_operator;
        result.GradientThresholdValue = source.gradient_threshold_value;
        result.AnchorThresholdValue = source.anchor_threshold_value;
        result.ScanInterval = source.scan_interval;
        result.MinPathLength = source.min_path_length;
        result.Sigma = source.sigma;
        result.SumFlag = source.sum_flag != 0;
        result.NFAValidation = source.nfa_validation != 0;
        result.MinLineLength = source.min_line_length;
        result.MaxDistanceBetweenTwoLines = source.max_distance_between_two_lines;
        result.LineFitErrorThreshold = source.line_fit_error_threshold;
        result.MaxErrorThreshold = source.max_error_threshold;
        return result;
    }

    void from_edge_drawing_params(
        const cv::ximgproc::EdgeDrawing::Params& source,
        jyppx_ocv_ximgproc_edge_drawing_params* destination)
    {
        destination->pf_mode = source.PFmode ? 1 : 0;
        destination->edge_detection_operator = source.EdgeDetectionOperator;
        destination->gradient_threshold_value = source.GradientThresholdValue;
        destination->anchor_threshold_value = source.AnchorThresholdValue;
        destination->scan_interval = source.ScanInterval;
        destination->min_path_length = source.MinPathLength;
        destination->sigma = source.Sigma;
        destination->sum_flag = source.SumFlag ? 1 : 0;
        destination->nfa_validation = source.NFAValidation ? 1 : 0;
        destination->min_line_length = source.MinLineLength;
        destination->max_distance_between_two_lines = source.MaxDistanceBetweenTwoLines;
        destination->line_fit_error_threshold = source.LineFitErrorThreshold;
        destination->max_error_threshold = source.MaxErrorThreshold;
    }

    int disparity_filter_core(
        const char* api_name,
        cv::ximgproc::DisparityFilter* filter,
        const jyppx_ocv_mat* disparity_map_left,
        const jyppx_ocv_mat* left_view,
        jyppx_ocv_mat* filtered_disparity_map,
        const jyppx_ocv_mat* disparity_map_right,
        const jyppx_ocv_ximgproc_rect* roi,
        const jyppx_ocv_mat* right_view)
    {
        int status = validate_mat(api_name, disparity_map_left, "disparity_map_left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, left_view, "left_view");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, filtered_disparity_map, "filtered_disparity_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        const cv::Mat empty;
        const cv::Mat& right_disparity =
            disparity_map_right == nullptr ? empty : opencv_csharp_native::mat_value(disparity_map_right);
        const cv::Mat& right =
            right_view == nullptr ? empty : opencv_csharp_native::mat_value(right_view);

        filter->filter(
            opencv_csharp_native::mat_value(disparity_map_left),
            opencv_csharp_native::mat_value(left_view),
            opencv_csharp_native::mat_value(filtered_disparity_map),
            right_disparity,
            to_rect(roi),
            right);
        return OPENCV_CSHARP_STATUS_OK;
    }

    template <typename THandle>
    int sparse_match_interpolate_core(
        const char* api_name,
        THandle* interpolator,
        const jyppx_ocv_mat* from_image,
        const jyppx_ocv_mat* from_points,
        const jyppx_ocv_mat* to_image,
        const jyppx_ocv_mat* to_points,
        jyppx_ocv_mat* dense_flow)
    {
        int status = validate_handle(api_name, interpolator, "interpolator");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, from_image, "from_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, from_points, "from_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, to_image, "to_image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, to_points, "to_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dense_flow, "dense_flow");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        interpolator->value->interpolate(
            opencv_csharp_native::mat_value(from_image),
            opencv_csharp_native::mat_value(from_points),
            opencv_csharp_native::mat_value(to_image),
            opencv_csharp_native::mat_value(to_points),
            opencv_csharp_native::mat_value(dense_flow));
        return OPENCV_CSHARP_STATUS_OK;
    }

    int edge_boxes_core(
        const char* api_name,
        jyppx_ocv_ximgproc_edge_boxes* edge_boxes,
        const jyppx_ocv_mat* edge_map,
        const jyppx_ocv_mat* orientation_map,
        std::vector<cv::Rect>& boxes,
        cv::Mat& scores)
    {
        int status = validate_handle(api_name, edge_boxes, "edge_boxes");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, edge_map, "edge_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, orientation_map, "orientation_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        edge_boxes->value->getBoundingBoxes(
            opencv_csharp_native::mat_value(edge_map),
            opencv_csharp_native::mat_value(orientation_map),
            boxes,
            scores);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int fast_line_detect_core(
        const char* api_name,
        jyppx_ocv_ximgproc_fast_line_detector* detector,
        const jyppx_ocv_mat* image,
        cv::Mat& lines)
    {
        int status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        detector->value->detect(opencv_csharp_native::mat_value(image), lines);
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_ximgproc_ni_black_threshold(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double max_value,
    int type,
    int block_size,
    double k,
    int binarization_method,
    double r)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_ni_black_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::niBlackThreshold(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            max_value,
            type,
            block_size,
            k,
            binarization_method,
            r);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)max_value; (void)type; (void)block_size; (void)k; (void)binarization_method; (void)r;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_thinning(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int thinning_type)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_thinning";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::thinning(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), thinning_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)thinning_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_anisotropic_diffusion(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    float alpha,
    float k,
    int niters)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_anisotropic_diffusion";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::anisotropicDiffusion(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), alpha, k, niters);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)alpha; (void)k; (void)niters;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_joint_bilateral_filter(
    const jyppx_ocv_mat* joint,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int d,
    double sigma_color,
    double sigma_space,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_joint_bilateral_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, joint, "joint");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::jointBilateralFilter(
            opencv_csharp_native::mat_value(joint),
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            d,
            sigma_color,
            sigma_space,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)d; (void)sigma_color; (void)sigma_space; (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_guided_filter_run(
    const jyppx_ocv_mat* guide,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int radius,
    double eps,
    int ddepth,
    double scale)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_guided_filter_run";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, guide, "guide");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::guidedFilter(
            opencv_csharp_native::mat_value(guide),
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            radius,
            eps,
            ddepth,
            scale);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)radius; (void)eps; (void)ddepth; (void)scale;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_rolling_guidance_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int d,
    double sigma_color,
    double sigma_space,
    int num_of_iter,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_rolling_guidance_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::rollingGuidanceFilter(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            d,
            sigma_color,
            sigma_space,
            num_of_iter,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)d; (void)sigma_color; (void)sigma_space; (void)num_of_iter; (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_weighted_median_filter(
    const jyppx_ocv_mat* joint,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int r,
    double sigma,
    int weight_type,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_weighted_median_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, joint, "joint");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        if (mask == nullptr)
        {
            cv::ximgproc::weightedMedianFilter(
                opencv_csharp_native::mat_value(joint),
                opencv_csharp_native::mat_value(src),
                opencv_csharp_native::mat_value(dst),
                r,
                sigma,
                weight_type,
                cv::noArray());
        }
        else
        {
            cv::ximgproc::weightedMedianFilter(
                opencv_csharp_native::mat_value(joint),
                opencv_csharp_native::mat_value(src),
                opencv_csharp_native::mat_value(dst),
                r,
                sigma,
                weight_type,
                opencv_csharp_native::mat_value(mask));
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)r; (void)sigma; (void)weight_type; (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_dt_filter(
    const jyppx_ocv_mat* guide,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double sigma_spatial,
    double sigma_color,
    int mode,
    int num_iters)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_dt_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, guide, "guide");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::dtFilter(
            opencv_csharp_native::mat_value(guide),
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            sigma_spatial,
            sigma_color,
            mode,
            num_iters);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma_spatial; (void)sigma_color; (void)mode; (void)num_iters;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_am_filter(
    const jyppx_ocv_mat* joint,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double sigma_s,
    double sigma_r,
    int adjust_outliers)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_am_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, joint, "joint");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::amFilter(
            opencv_csharp_native::mat_value(joint),
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            sigma_s,
            sigma_r,
            adjust_outliers != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma_s; (void)sigma_r; (void)adjust_outliers;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_bilateral_texture_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int fr,
    int num_iter,
    double sigma_alpha,
    double sigma_avg)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_bilateral_texture_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::bilateralTextureFilter(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), fr, num_iter, sigma_alpha, sigma_avg);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)fr; (void)num_iter; (void)sigma_alpha; (void)sigma_avg;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_preserving_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int d,
    double threshold)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_preserving_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::edgePreservingFilter(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), d, threshold);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)d; (void)threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fast_global_smoother_filter_run(
    const jyppx_ocv_mat* guide,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double lambda,
    double sigma_color,
    double lambda_attenuation,
    int num_iter)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_global_smoother_filter_run";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, guide, "guide");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::fastGlobalSmootherFilter(
            opencv_csharp_native::mat_value(guide),
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            lambda,
            sigma_color,
            lambda_attenuation,
            num_iter);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)lambda; (void)sigma_color; (void)lambda_attenuation; (void)num_iter;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_l0_smooth(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, double lambda, double kappa)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_l0_smooth";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::l0Smooth(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), lambda, kappa);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)lambda; (void)kappa;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fast_hough_transform(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int dst_mat_depth,
    int angle_range,
    int op,
    int make_skew)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_hough_transform";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::FastHoughTransform(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            dst_mat_depth,
            angle_range,
            op,
            make_skew);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dst_mat_depth; (void)angle_range; (void)op; (void)make_skew;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_hough_point_to_line(
    int hough_x,
    int hough_y,
    const jyppx_ocv_mat* src_img_info,
    int angle_range,
    int make_skew,
    int rules,
    int* x1,
    int* y1,
    int* x2,
    int* y2)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_hough_point_to_line";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src_img_info, "src_img_info");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, x1, "x1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, y1, "y1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, x2, "x2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, y2, "y2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        const cv::Vec4i line = cv::ximgproc::HoughPoint2Line(
            cv::Point(hough_x, hough_y),
            opencv_csharp_native::mat_value(src_img_info),
            angle_range,
            make_skew,
            rules);
        *x1 = line[0];
        *y1 = line[1];
        *x2 = line[2];
        *y2 = line[3];
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)hough_x; (void)hough_y; (void)angle_range; (void)make_skew; (void)rules;
        *x1 = 0; *y1 = 0; *x2 = 0; *y2 = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_pei_lin_normalization(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_pei_lin_normalization";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::PeiLinNormalization(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_ximgproc_guided_filter_create(
    const jyppx_ocv_mat* guide,
    int radius,
    double eps,
    double scale,
    jyppx_ocv_ximgproc_guided_filter** filter)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_guided_filter_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, guide, "guide");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::createGuidedFilter(opencv_csharp_native::mat_value(guide), radius, eps, scale),
            filter,
            "filter");
#else
        (void)radius; (void)eps; (void)scale;
        if (filter != nullptr) { *filter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_guided_filter_release_handle(jyppx_ocv_ximgproc_guided_filter* filter)
{
    delete filter;
}

int jyppx_ocv_ximgproc_guided_filter_filter(
    jyppx_ocv_ximgproc_guided_filter* filter,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_guided_filter_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        filter->value->filter(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), ddepth);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter; (void)ddepth;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fast_global_smoother_filter_create(
    const jyppx_ocv_mat* guide,
    double lambda,
    double sigma_color,
    double lambda_attenuation,
    int num_iter,
    jyppx_ocv_ximgproc_fast_global_smoother_filter** filter)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_global_smoother_filter_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, guide, "guide");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::createFastGlobalSmootherFilter(opencv_csharp_native::mat_value(guide), lambda, sigma_color, lambda_attenuation, num_iter),
            filter,
            "filter");
#else
        (void)lambda; (void)sigma_color; (void)lambda_attenuation; (void)num_iter;
        if (filter != nullptr) { *filter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_fast_global_smoother_filter_release_handle(jyppx_ocv_ximgproc_fast_global_smoother_filter* filter)
{
    delete filter;
}

int jyppx_ocv_ximgproc_fast_global_smoother_filter_filter(
    jyppx_ocv_ximgproc_fast_global_smoother_filter* filter,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_global_smoother_filter_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        filter->value->filter(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_slic_create(
    const jyppx_ocv_mat* image,
    int algorithm,
    int region_size,
    float ruler,
    jyppx_ocv_ximgproc_superpixel_slic** superpixel)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_slic_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::createSuperpixelSLIC(opencv_csharp_native::mat_value(image), algorithm, region_size, ruler),
            superpixel,
            "superpixel");
#else
        (void)algorithm; (void)region_size; (void)ruler;
        if (superpixel != nullptr) { *superpixel = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_superpixel_slic_release_handle(jyppx_ocv_ximgproc_superpixel_slic* superpixel)
{
    delete superpixel;
}

int jyppx_ocv_ximgproc_superpixel_slic_get_number(const jyppx_ocv_ximgproc_superpixel_slic* superpixel, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_slic_get_number";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = superpixel->value->getNumberOfSuperpixels();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_slic_iterate(jyppx_ocv_ximgproc_superpixel_slic* superpixel, int num_iterations)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_slic_iterate";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->iterate(num_iterations);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel; (void)num_iterations;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_slic_get_labels(const jyppx_ocv_ximgproc_superpixel_slic* superpixel, jyppx_ocv_mat* labels)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_slic_get_labels";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, labels, "labels");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->getLabels(opencv_csharp_native::mat_value(labels));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_slic_get_label_contour_mask(
    const jyppx_ocv_ximgproc_superpixel_slic* superpixel,
    jyppx_ocv_mat* image,
    int thick_line)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_slic_get_label_contour_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->getLabelContourMask(opencv_csharp_native::mat_value(image), thick_line != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel; (void)thick_line;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_slic_enforce_label_connectivity(jyppx_ocv_ximgproc_superpixel_slic* superpixel, int min_element_size)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_slic_enforce_label_connectivity";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->enforceLabelConnectivity(min_element_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel; (void)min_element_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_seeds_create(
    int image_width,
    int image_height,
    int image_channels,
    int num_superpixels,
    int num_levels,
    int prior,
    int histogram_bins,
    int double_step,
    jyppx_ocv_ximgproc_superpixel_seeds** superpixel)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_seeds_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::createSuperpixelSEEDS(image_width, image_height, image_channels, num_superpixels, num_levels, prior, histogram_bins, double_step != 0),
            superpixel,
            "superpixel");
#else
        (void)image_width; (void)image_height; (void)image_channels; (void)num_superpixels; (void)num_levels; (void)prior; (void)histogram_bins; (void)double_step;
        if (superpixel != nullptr) { *superpixel = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_superpixel_seeds_release_handle(jyppx_ocv_ximgproc_superpixel_seeds* superpixel)
{
    delete superpixel;
}

int jyppx_ocv_ximgproc_superpixel_seeds_get_number(jyppx_ocv_ximgproc_superpixel_seeds* superpixel, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_seeds_get_number";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = superpixel->value->getNumberOfSuperpixels();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_seeds_iterate(
    jyppx_ocv_ximgproc_superpixel_seeds* superpixel,
    const jyppx_ocv_mat* image,
    int num_iterations)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_seeds_iterate";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->iterate(opencv_csharp_native::mat_value(image), num_iterations);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel; (void)num_iterations;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_seeds_get_labels(jyppx_ocv_ximgproc_superpixel_seeds* superpixel, jyppx_ocv_mat* labels)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_seeds_get_labels";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, labels, "labels");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->getLabels(opencv_csharp_native::mat_value(labels));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_seeds_get_label_contour_mask(
    jyppx_ocv_ximgproc_superpixel_seeds* superpixel,
    jyppx_ocv_mat* image,
    int thick_line)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_seeds_get_label_contour_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->getLabelContourMask(opencv_csharp_native::mat_value(image), thick_line != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel; (void)thick_line;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_lsc_create(
    const jyppx_ocv_mat* image,
    int region_size,
    float ratio,
    jyppx_ocv_ximgproc_superpixel_lsc** superpixel)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_lsc_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::createSuperpixelLSC(opencv_csharp_native::mat_value(image), region_size, ratio),
            superpixel,
            "superpixel");
#else
        (void)region_size; (void)ratio;
        if (superpixel != nullptr) { *superpixel = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_superpixel_lsc_release_handle(jyppx_ocv_ximgproc_superpixel_lsc* superpixel)
{
    delete superpixel;
}

int jyppx_ocv_ximgproc_superpixel_lsc_get_number(const jyppx_ocv_ximgproc_superpixel_lsc* superpixel, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_lsc_get_number";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = superpixel->value->getNumberOfSuperpixels();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_lsc_iterate(jyppx_ocv_ximgproc_superpixel_lsc* superpixel, int num_iterations)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_lsc_iterate";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->iterate(num_iterations);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel; (void)num_iterations;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_lsc_get_labels(const jyppx_ocv_ximgproc_superpixel_lsc* superpixel, jyppx_ocv_mat* labels)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_lsc_get_labels";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, labels, "labels");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->getLabels(opencv_csharp_native::mat_value(labels));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_lsc_get_label_contour_mask(
    const jyppx_ocv_ximgproc_superpixel_lsc* superpixel,
    jyppx_ocv_mat* image,
    int thick_line)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_lsc_get_label_contour_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->getLabelContourMask(opencv_csharp_native::mat_value(image), thick_line != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel; (void)thick_line;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_superpixel_lsc_enforce_label_connectivity(jyppx_ocv_ximgproc_superpixel_lsc* superpixel, int min_element_size)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_superpixel_lsc_enforce_label_connectivity";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, superpixel, "superpixel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        superpixel->value->enforceLabelConnectivity(min_element_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)superpixel; (void)min_element_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fast_line_detector_create(
    int length_threshold,
    float distance_threshold,
    double canny_th1,
    double canny_th2,
    int canny_aperture_size,
    int do_merge,
    jyppx_ocv_ximgproc_fast_line_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_line_detector_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::createFastLineDetector(length_threshold, distance_threshold, canny_th1, canny_th2, canny_aperture_size, do_merge != 0),
            detector,
            "detector");
#else
        (void)length_threshold; (void)distance_threshold; (void)canny_th1; (void)canny_th2; (void)canny_aperture_size; (void)do_merge;
        if (detector != nullptr) { *detector = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_fast_line_detector_release_handle(jyppx_ocv_ximgproc_fast_line_detector* detector)
{
    delete detector;
}

int jyppx_ocv_ximgproc_fast_line_detector_detect(
    jyppx_ocv_ximgproc_fast_line_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* lines)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_line_detector_detect";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, lines, "lines");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->detect(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(lines));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fast_line_detector_detect_count(
    jyppx_ocv_ximgproc_fast_line_detector* detector,
    const jyppx_ocv_mat* image,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_line_detector_detect_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, line_count, "line_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::Mat output;
        status = fast_line_detect_core(api_name, detector, image, output);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *line_count = static_cast<int>(output.total());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector; (void)image;
        *line_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fast_line_detector_detect_fill(
    jyppx_ocv_ximgproc_fast_line_detector* detector,
    const jyppx_ocv_mat* image,
    float* lines,
    int line_capacity,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_line_detector_detect_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, line_count, "line_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (lines == nullptr && line_capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::Mat output;
        status = fast_line_detect_core(api_name, detector, image, output);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        const int actual_count = static_cast<int>(output.total());
        if (line_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const cv::Vec4f value = output.at<cv::Vec4f>(i);
            const int offset = i * 4;
            lines[offset] = value[0];
            lines[offset + 1] = value[1];
            lines[offset + 2] = value[2];
            lines[offset + 3] = value[3];
        }

        *line_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector; (void)image; (void)lines; (void)line_capacity;
        *line_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fast_line_detector_draw_segments(
    jyppx_ocv_ximgproc_fast_line_detector* detector,
    jyppx_ocv_mat* image,
    const jyppx_ocv_mat* lines,
    int draw_arrow,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int line_thickness)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_line_detector_draw_segments";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, lines, "lines");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->drawSegments(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(lines),
            draw_arrow != 0,
            cv::Scalar(color_v0, color_v1, color_v2, color_v3),
            line_thickness);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector; (void)draw_arrow; (void)color_v0; (void)color_v1; (void)color_v2; (void)color_v3; (void)line_thickness;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fast_line_detector_draw_segments_array(
    jyppx_ocv_ximgproc_fast_line_detector* detector,
    jyppx_ocv_mat* image,
    const float* lines,
    int line_count,
    int draw_arrow,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int line_thickness)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_line_detector_draw_segments_array";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (lines == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines");
        }
        if (line_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Mat lines_mat = get_vec4f_mat_from_flat(lines, line_count);
        detector->value->drawSegments(
            opencv_csharp_native::mat_value(image),
            lines_mat,
            draw_arrow != 0,
            cv::Scalar(color_v0, color_v1, color_v2, color_v3),
            line_thickness);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector; (void)draw_arrow; (void)color_v0; (void)color_v1; (void)color_v2; (void)color_v3; (void)line_thickness;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_create_generic(
    int use_confidence,
    jyppx_ocv_ximgproc_disparity_wls_filter** filter)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_create_generic";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::createDisparityWLSFilterGeneric(use_confidence != 0),
            filter,
            "filter");
#else
        (void)use_confidence;
        if (filter != nullptr) { *filter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_bm(
    const jyppx_ocv_stereo_bm* matcher_left,
    jyppx_ocv_ximgproc_disparity_wls_filter** filter)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_bm";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, matcher_left, "matcher_left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_handle(
            api_name,
            cv::ximgproc::createDisparityWLSFilter(matcher_left->value),
            filter,
            "filter");
#else
        (void)matcher_left;
        if (filter != nullptr) { *filter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_sgbm(
    const jyppx_ocv_stereo_sgbm* matcher_left,
    jyppx_ocv_ximgproc_disparity_wls_filter** filter)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_sgbm";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, matcher_left, "matcher_left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_handle(
            api_name,
            cv::ximgproc::createDisparityWLSFilter(matcher_left->value),
            filter,
            "filter");
#else
        (void)matcher_left;
        if (filter != nullptr) { *filter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_matcher(
    const jyppx_ocv_stereo_matcher* matcher_left,
    jyppx_ocv_ximgproc_disparity_wls_filter** filter)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_create_from_stereo_matcher";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, matcher_left, "matcher_left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_handle(
            api_name,
            cv::ximgproc::createDisparityWLSFilter(matcher_left->value),
            filter,
            "filter");
#else
        (void)matcher_left;
        if (filter != nullptr) { *filter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_create_right_matcher_from_stereo_bm(
    const jyppx_ocv_stereo_bm* matcher_left,
    jyppx_ocv_stereo_matcher** matcher_right)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_create_right_matcher_from_stereo_bm";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, matcher_left, "matcher_left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_handle(
            api_name,
            cv::ximgproc::createRightMatcher(matcher_left->value),
            matcher_right,
            "matcher_right");
#else
        (void)matcher_left;
        if (matcher_right != nullptr) { *matcher_right = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_create_right_matcher_from_stereo_sgbm(
    const jyppx_ocv_stereo_sgbm* matcher_left,
    jyppx_ocv_stereo_matcher** matcher_right)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_create_right_matcher_from_stereo_sgbm";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, matcher_left, "matcher_left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_handle(
            api_name,
            cv::ximgproc::createRightMatcher(matcher_left->value),
            matcher_right,
            "matcher_right");
#else
        (void)matcher_left;
        if (matcher_right != nullptr) { *matcher_right = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_create_right_matcher_from_stereo_matcher(
    const jyppx_ocv_stereo_matcher* matcher_left,
    jyppx_ocv_stereo_matcher** matcher_right)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_create_right_matcher_from_stereo_matcher";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, matcher_left, "matcher_left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_handle(
            api_name,
            cv::ximgproc::createRightMatcher(matcher_left->value),
            matcher_right,
            "matcher_right");
#else
        (void)matcher_left;
        if (matcher_right != nullptr) { *matcher_right = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_disparity_wls_filter_release_handle(jyppx_ocv_ximgproc_disparity_wls_filter* filter)
{
    delete filter;
}

int jyppx_ocv_ximgproc_disparity_filter_filter(
    jyppx_ocv_ximgproc_disparity_filter* filter,
    const jyppx_ocv_mat* disparity_map_left,
    const jyppx_ocv_mat* left_view,
    jyppx_ocv_mat* filtered_disparity_map,
    const jyppx_ocv_mat* disparity_map_right,
    const jyppx_ocv_ximgproc_rect* roi,
    const jyppx_ocv_mat* right_view)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_filter_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return disparity_filter_core(
            api_name,
            filter->value.get(),
            disparity_map_left,
            left_view,
            filtered_disparity_map,
            disparity_map_right,
            roi,
            right_view);
#else
        (void)filter; (void)disparity_map_left; (void)left_view; (void)filtered_disparity_map; (void)disparity_map_right; (void)roi; (void)right_view;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_filter(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    const jyppx_ocv_mat* disparity_map_left,
    const jyppx_ocv_mat* left_view,
    jyppx_ocv_mat* filtered_disparity_map,
    const jyppx_ocv_mat* disparity_map_right,
    const jyppx_ocv_ximgproc_rect* roi,
    const jyppx_ocv_mat* right_view)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return disparity_filter_core(
            api_name,
            filter->value.get(),
            disparity_map_left,
            left_view,
            filtered_disparity_map,
            disparity_map_right,
            roi,
            right_view);
#else
        (void)filter; (void)disparity_map_left; (void)left_view; (void)filtered_disparity_map; (void)disparity_map_right; (void)roi; (void)right_view;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_get_lambda(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    double* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_get_lambda";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = 0.0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = filter->value->getLambda();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_set_lambda(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    double value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_set_lambda";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        filter->value->setLambda(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_get_sigma_color(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    double* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_get_sigma_color";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = 0.0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = filter->value->getSigmaColor();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_set_sigma_color(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    double value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_set_sigma_color";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        filter->value->setSigmaColor(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_get_lrc_thresh(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_get_lrc_thresh";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = filter->value->getLRCthresh();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_set_lrc_thresh(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    int value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_set_lrc_thresh";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        filter->value->setLRCthresh(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_get_depth_discontinuity_radius(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_get_depth_discontinuity_radius";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = filter->value->getDepthDiscontinuityRadius();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_set_depth_discontinuity_radius(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    int value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_set_depth_discontinuity_radius";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        filter->value->setDepthDiscontinuityRadius(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_get_confidence_map(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    jyppx_ocv_mat* confidence_map)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_get_confidence_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, confidence_map, "confidence_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        opencv_csharp_native::mat_value(confidence_map) = filter->value->getConfidenceMap();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_disparity_wls_filter_get_roi(
    jyppx_ocv_ximgproc_disparity_wls_filter* filter,
    jyppx_ocv_ximgproc_rect* roi)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_disparity_wls_filter_get_roi";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (roi == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "roi");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        from_rect(filter->value->getROI(), roi);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter;
        roi->x = 0; roi->y = 0; roi->width = 0; roi->height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_get_disparity_vis(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double scale)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_get_disparity_vis";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::getDisparityVis(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), scale);
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

int jyppx_ocv_ximgproc_compute_mse(
    const jyppx_ocv_mat* gt,
    const jyppx_ocv_mat* src,
    const jyppx_ocv_ximgproc_rect* roi,
    double* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_compute_mse";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, gt, "gt");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = 0.0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        *value = cv::ximgproc::computeMSE(opencv_csharp_native::mat_value(gt), opencv_csharp_native::mat_value(src), to_rect(roi));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)roi;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_compute_bad_pixel_percent(
    const jyppx_ocv_mat* gt,
    const jyppx_ocv_mat* src,
    const jyppx_ocv_ximgproc_rect* roi,
    int thresh,
    double* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_compute_bad_pixel_percent";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, gt, "gt");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = 0.0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        *value = cv::ximgproc::computeBadPixelPercent(opencv_csharp_native::mat_value(gt), opencv_csharp_native::mat_value(src), to_rect(roi), thresh);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)roi; (void)thresh;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fast_bilateral_solver_filter_create(
    const jyppx_ocv_mat* guide,
    double sigma_spatial,
    double sigma_luma,
    double sigma_chroma,
    double lambda,
    int num_iter,
    double max_tol,
    jyppx_ocv_ximgproc_fast_bilateral_solver_filter** filter)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, guide, "guide");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::createFastBilateralSolverFilter(
                opencv_csharp_native::mat_value(guide),
                sigma_spatial,
                sigma_luma,
                sigma_chroma,
                lambda,
                num_iter,
                max_tol),
            filter,
            "filter");
#else
        (void)sigma_spatial; (void)sigma_luma; (void)sigma_chroma; (void)lambda; (void)num_iter; (void)max_tol;
        if (filter != nullptr) { *filter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_fast_bilateral_solver_filter_release_handle(
    jyppx_ocv_ximgproc_fast_bilateral_solver_filter* filter)
{
    delete filter;
}

int jyppx_ocv_ximgproc_fast_bilateral_solver_filter_filter(
    jyppx_ocv_ximgproc_fast_bilateral_solver_filter* filter,
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* confidence,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, confidence, "confidence");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        filter->value->filter(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(confidence),
            opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fast_bilateral_solver_filter_run(
    const jyppx_ocv_mat* guide,
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* confidence,
    jyppx_ocv_mat* dst,
    double sigma_spatial,
    double sigma_luma,
    double sigma_chroma,
    double lambda,
    int num_iter,
    double max_tol)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fast_bilateral_solver_filter_run";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, guide, "guide");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, confidence, "confidence");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::fastBilateralSolverFilter(
            opencv_csharp_native::mat_value(guide),
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(confidence),
            opencv_csharp_native::mat_value(dst),
            sigma_spatial,
            sigma_luma,
            sigma_chroma,
            lambda,
            num_iter,
            max_tol);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma_spatial; (void)sigma_luma; (void)sigma_chroma; (void)lambda; (void)num_iter; (void)max_tol;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_aware_interpolator_create(
    jyppx_ocv_ximgproc_edge_aware_interpolator** interpolator)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_aware_interpolator_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::createEdgeAwareInterpolator(), interpolator, "interpolator");
#else
        if (interpolator != nullptr) { *interpolator = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_edge_aware_interpolator_release_handle(
    jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator)
{
    delete interpolator;
}

int jyppx_ocv_ximgproc_ric_interpolator_create(
    jyppx_ocv_ximgproc_ric_interpolator** interpolator)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_ric_interpolator_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::createRICInterpolator(), interpolator, "interpolator");
#else
        if (interpolator != nullptr) { *interpolator = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_ric_interpolator_release_handle(
    jyppx_ocv_ximgproc_ric_interpolator* interpolator)
{
    delete interpolator;
}

int jyppx_ocv_ximgproc_sparse_match_interpolator_interpolate(
    jyppx_ocv_ximgproc_sparse_match_interpolator* interpolator,
    const jyppx_ocv_mat* from_image,
    const jyppx_ocv_mat* from_points,
    const jyppx_ocv_mat* to_image,
    const jyppx_ocv_mat* to_points,
    jyppx_ocv_mat* dense_flow)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_sparse_match_interpolator_interpolate";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return sparse_match_interpolate_core(api_name, interpolator, from_image, from_points, to_image, to_points, dense_flow);
#else
        (void)interpolator; (void)from_image; (void)from_points; (void)to_image; (void)to_points; (void)dense_flow;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_aware_interpolator_interpolate(
    jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator,
    const jyppx_ocv_mat* from_image,
    const jyppx_ocv_mat* from_points,
    const jyppx_ocv_mat* to_image,
    const jyppx_ocv_mat* to_points,
    jyppx_ocv_mat* dense_flow)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_aware_interpolator_interpolate";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return sparse_match_interpolate_core(api_name, interpolator, from_image, from_points, to_image, to_points, dense_flow);
#else
        (void)interpolator; (void)from_image; (void)from_points; (void)to_image; (void)to_points; (void)dense_flow;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_ric_interpolator_interpolate(
    jyppx_ocv_ximgproc_ric_interpolator* interpolator,
    const jyppx_ocv_mat* from_image,
    const jyppx_ocv_mat* from_points,
    const jyppx_ocv_mat* to_image,
    const jyppx_ocv_mat* to_points,
    jyppx_ocv_mat* dense_flow)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_ric_interpolator_interpolate";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return sparse_match_interpolate_core(api_name, interpolator, from_image, from_points, to_image, to_points, dense_flow);
#else
        (void)interpolator; (void)from_image; (void)from_points; (void)to_image; (void)to_points; (void)dense_flow;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_aware_interpolator_set_cost_map(
    jyppx_ocv_ximgproc_edge_aware_interpolator* interpolator,
    const jyppx_ocv_mat* cost_map)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_aware_interpolator_set_cost_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, cost_map, "cost_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, interpolator, "interpolator");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        interpolator->value->setCostMap(opencv_csharp_native::mat_value(cost_map));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)interpolator;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_ric_interpolator_set_cost_map(
    jyppx_ocv_ximgproc_ric_interpolator* interpolator,
    const jyppx_ocv_mat* cost_map)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_ric_interpolator_set_cost_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, cost_map, "cost_map");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, interpolator, "interpolator");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        interpolator->value->setCostMap(opencv_csharp_native::mat_value(cost_map));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)interpolator;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
#define OPENCV_CSHARP_XIMGPROC_GET_INT(function_name, handle_type, getter_name) \
int function_name(handle_type* handle, int* value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_output_int(api_name, value, "value"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = 0; \
        status = validate_handle(api_name, handle, "handle"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = handle->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}

#define OPENCV_CSHARP_XIMGPROC_SET_INT(function_name, handle_type, setter_name) \
int function_name(handle_type* handle, int value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_handle(api_name, handle, "handle"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        handle->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}

#define OPENCV_CSHARP_XIMGPROC_GET_FLOAT(function_name, handle_type, getter_name) \
int function_name(handle_type* handle, float* value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_output_float(api_name, value, "value"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = 0.0F; \
        status = validate_handle(api_name, handle, "handle"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = handle->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}

#define OPENCV_CSHARP_XIMGPROC_SET_FLOAT(function_name, handle_type, setter_name) \
int function_name(handle_type* handle, float value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_handle(api_name, handle, "handle"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        handle->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}

#define OPENCV_CSHARP_XIMGPROC_GET_BOOL(function_name, handle_type, getter_name) \
int function_name(handle_type* handle, int* value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_output_int(api_name, value, "value"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = 0; \
        status = validate_handle(api_name, handle, "handle"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = handle->value->getter_name() ? 1 : 0; \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}

#define OPENCV_CSHARP_XIMGPROC_SET_BOOL(function_name, handle_type, setter_name) \
int function_name(handle_type* handle, int value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_handle(api_name, handle, "handle"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        handle->value->setter_name(value != 0); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}
#else
#define OPENCV_CSHARP_XIMGPROC_GET_INT(function_name, handle_type, getter_name) \
int function_name(handle_type* handle, int* value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    int status = validate_output_int(api_name, value, "value"); \
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
    (void)handle; *value = 0; \
    return opencv_csharp_native::set_not_linked(api_name); \
}

#define OPENCV_CSHARP_XIMGPROC_SET_INT(function_name, handle_type, setter_name) \
int function_name(handle_type* handle, int value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    (void)handle; (void)value; \
    return opencv_csharp_native::set_not_linked(api_name); \
}

#define OPENCV_CSHARP_XIMGPROC_GET_FLOAT(function_name, handle_type, getter_name) \
int function_name(handle_type* handle, float* value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    int status = validate_output_float(api_name, value, "value"); \
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
    (void)handle; *value = 0.0F; \
    return opencv_csharp_native::set_not_linked(api_name); \
}

#define OPENCV_CSHARP_XIMGPROC_SET_FLOAT(function_name, handle_type, setter_name) \
int function_name(handle_type* handle, float value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    (void)handle; (void)value; \
    return opencv_csharp_native::set_not_linked(api_name); \
}

#define OPENCV_CSHARP_XIMGPROC_GET_BOOL(function_name, handle_type, getter_name) \
int function_name(handle_type* handle, int* value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    int status = validate_output_int(api_name, value, "value"); \
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
    (void)handle; *value = 0; \
    return opencv_csharp_native::set_not_linked(api_name); \
}

#define OPENCV_CSHARP_XIMGPROC_SET_BOOL(function_name, handle_type, setter_name) \
int function_name(handle_type* handle, int value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    (void)handle; (void)value; \
    return opencv_csharp_native::set_not_linked(api_name); \
}
#endif

OPENCV_CSHARP_XIMGPROC_GET_INT(jyppx_ocv_ximgproc_edge_aware_interpolator_get_k, jyppx_ocv_ximgproc_edge_aware_interpolator, getK)
OPENCV_CSHARP_XIMGPROC_SET_INT(jyppx_ocv_ximgproc_edge_aware_interpolator_set_k, jyppx_ocv_ximgproc_edge_aware_interpolator, setK)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_aware_interpolator_get_sigma, jyppx_ocv_ximgproc_edge_aware_interpolator, getSigma)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_aware_interpolator_set_sigma, jyppx_ocv_ximgproc_edge_aware_interpolator, setSigma)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_aware_interpolator_get_lambda, jyppx_ocv_ximgproc_edge_aware_interpolator, getLambda)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_aware_interpolator_set_lambda, jyppx_ocv_ximgproc_edge_aware_interpolator, setLambda)
OPENCV_CSHARP_XIMGPROC_GET_BOOL(jyppx_ocv_ximgproc_edge_aware_interpolator_get_use_post_processing, jyppx_ocv_ximgproc_edge_aware_interpolator, getUsePostProcessing)
OPENCV_CSHARP_XIMGPROC_SET_BOOL(jyppx_ocv_ximgproc_edge_aware_interpolator_set_use_post_processing, jyppx_ocv_ximgproc_edge_aware_interpolator, setUsePostProcessing)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_aware_interpolator_get_fgs_lambda, jyppx_ocv_ximgproc_edge_aware_interpolator, getFGSLambda)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_aware_interpolator_set_fgs_lambda, jyppx_ocv_ximgproc_edge_aware_interpolator, setFGSLambda)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_aware_interpolator_get_fgs_sigma, jyppx_ocv_ximgproc_edge_aware_interpolator, getFGSSigma)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_aware_interpolator_set_fgs_sigma, jyppx_ocv_ximgproc_edge_aware_interpolator, setFGSSigma)

OPENCV_CSHARP_XIMGPROC_GET_INT(jyppx_ocv_ximgproc_ric_interpolator_get_k, jyppx_ocv_ximgproc_ric_interpolator, getK)
OPENCV_CSHARP_XIMGPROC_SET_INT(jyppx_ocv_ximgproc_ric_interpolator_set_k, jyppx_ocv_ximgproc_ric_interpolator, setK)
OPENCV_CSHARP_XIMGPROC_GET_INT(jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_size, jyppx_ocv_ximgproc_ric_interpolator, getSuperpixelSize)
OPENCV_CSHARP_XIMGPROC_SET_INT(jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_size, jyppx_ocv_ximgproc_ric_interpolator, setSuperpixelSize)
OPENCV_CSHARP_XIMGPROC_GET_INT(jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_nn_count, jyppx_ocv_ximgproc_ric_interpolator, getSuperpixelNNCnt)
OPENCV_CSHARP_XIMGPROC_SET_INT(jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_nn_count, jyppx_ocv_ximgproc_ric_interpolator, setSuperpixelNNCnt)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_ruler, jyppx_ocv_ximgproc_ric_interpolator, getSuperpixelRuler)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_ruler, jyppx_ocv_ximgproc_ric_interpolator, setSuperpixelRuler)
OPENCV_CSHARP_XIMGPROC_GET_INT(jyppx_ocv_ximgproc_ric_interpolator_get_superpixel_mode, jyppx_ocv_ximgproc_ric_interpolator, getSuperpixelMode)
OPENCV_CSHARP_XIMGPROC_SET_INT(jyppx_ocv_ximgproc_ric_interpolator_set_superpixel_mode, jyppx_ocv_ximgproc_ric_interpolator, setSuperpixelMode)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_ric_interpolator_get_alpha, jyppx_ocv_ximgproc_ric_interpolator, getAlpha)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_ric_interpolator_set_alpha, jyppx_ocv_ximgproc_ric_interpolator, setAlpha)
OPENCV_CSHARP_XIMGPROC_GET_INT(jyppx_ocv_ximgproc_ric_interpolator_get_model_iter, jyppx_ocv_ximgproc_ric_interpolator, getModelIter)
OPENCV_CSHARP_XIMGPROC_SET_INT(jyppx_ocv_ximgproc_ric_interpolator_set_model_iter, jyppx_ocv_ximgproc_ric_interpolator, setModelIter)
OPENCV_CSHARP_XIMGPROC_GET_BOOL(jyppx_ocv_ximgproc_ric_interpolator_get_refine_models, jyppx_ocv_ximgproc_ric_interpolator, getRefineModels)
OPENCV_CSHARP_XIMGPROC_SET_BOOL(jyppx_ocv_ximgproc_ric_interpolator_set_refine_models, jyppx_ocv_ximgproc_ric_interpolator, setRefineModels)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_ric_interpolator_get_max_flow, jyppx_ocv_ximgproc_ric_interpolator, getMaxFlow)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_ric_interpolator_set_max_flow, jyppx_ocv_ximgproc_ric_interpolator, setMaxFlow)
OPENCV_CSHARP_XIMGPROC_GET_BOOL(jyppx_ocv_ximgproc_ric_interpolator_get_use_variational_refinement, jyppx_ocv_ximgproc_ric_interpolator, getUseVariationalRefinement)
OPENCV_CSHARP_XIMGPROC_SET_BOOL(jyppx_ocv_ximgproc_ric_interpolator_set_use_variational_refinement, jyppx_ocv_ximgproc_ric_interpolator, setUseVariationalRefinement)
OPENCV_CSHARP_XIMGPROC_GET_BOOL(jyppx_ocv_ximgproc_ric_interpolator_get_use_global_smoother_filter, jyppx_ocv_ximgproc_ric_interpolator, getUseGlobalSmootherFilter)
OPENCV_CSHARP_XIMGPROC_SET_BOOL(jyppx_ocv_ximgproc_ric_interpolator_set_use_global_smoother_filter, jyppx_ocv_ximgproc_ric_interpolator, setUseGlobalSmootherFilter)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_ric_interpolator_get_fgs_lambda, jyppx_ocv_ximgproc_ric_interpolator, getFGSLambda)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_ric_interpolator_set_fgs_lambda, jyppx_ocv_ximgproc_ric_interpolator, setFGSLambda)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_ric_interpolator_get_fgs_sigma, jyppx_ocv_ximgproc_ric_interpolator, getFGSSigma)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_ric_interpolator_set_fgs_sigma, jyppx_ocv_ximgproc_ric_interpolator, setFGSSigma)

int jyppx_ocv_ximgproc_edge_drawing_create(
    jyppx_ocv_ximgproc_edge_drawing** detector)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::createEdgeDrawing(), detector, "detector");
#else
        if (detector != nullptr) { *detector = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_edge_drawing_release_handle(jyppx_ocv_ximgproc_edge_drawing* detector)
{
    delete detector;
}

int jyppx_ocv_ximgproc_edge_drawing_get_params(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    jyppx_ocv_ximgproc_edge_drawing_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_get_params";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        from_edge_drawing_params(detector->value->params, params);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        std::memset(params, 0, sizeof(*params));
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_set_params(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    const jyppx_ocv_ximgproc_edge_drawing_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_set_params";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->setParams(to_edge_drawing_params(*params));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_detect_edges(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    const jyppx_ocv_mat* src)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_detect_edges";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->detectEdges(opencv_csharp_native::mat_value(src));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_get_edge_image(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_get_edge_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->getEdgeImage(opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_get_gradient_image(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_get_gradient_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->getGradientImage(opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_get_segments_count(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* group_count,
    int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_get_segments_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, group_count, "group_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, point_count, "point_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *group_count = 0;
        *point_count = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<std::vector<cv::Point> > segments = detector->value->getSegments();
        *group_count = static_cast<int>(segments.size());
        int total = 0;
        for (const auto& segment : segments)
        {
            total += static_cast<int>(segment.size());
        }

        *point_count = total;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_get_segments_fill(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_ximgproc_point* points,
    int point_capacity,
    int* group_count,
    int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_get_segments_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, group_count, "group_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, point_count, "point_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_count_fill_args(api_name, offsets, offset_capacity, "offsets", "offset_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_count_fill_args(api_name, points, point_capacity, "points", "point_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *group_count = 0;
        *point_count = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<std::vector<cv::Point> > segments = detector->value->getSegments();
        const int actual_groups = static_cast<int>(segments.size());
        int actual_points = 0;
        for (const auto& segment : segments)
        {
            actual_points += static_cast<int>(segment.size());
        }

        if (offset_capacity < actual_groups + 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "offset_capacity");
        }
        if (point_capacity < actual_points)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_capacity");
        }

        int point_offset = 0;
        offsets[0] = 0;
        for (int i = 0; i < actual_groups; ++i)
        {
            for (const cv::Point& point : segments[static_cast<size_t>(i)])
            {
                points[point_offset].x = point.x;
                points[point_offset].y = point.y;
                ++point_offset;
            }

            offsets[i + 1] = point_offset;
        }

        *group_count = actual_groups;
        *point_count = actual_points;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector; (void)offsets; (void)points;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_detect_lines(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    jyppx_ocv_mat* lines)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_detect_lines";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, lines, "lines");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->detectLines(opencv_csharp_native::mat_value(lines));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_detect_lines_count(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_detect_lines_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, line_count, "line_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *line_count = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Mat lines;
        detector->value->detectLines(lines);
        *line_count = static_cast<int>(lines.total());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_detect_lines_fill(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    float* lines,
    int line_capacity,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_detect_lines_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, line_count, "line_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_count_fill_args(api_name, lines, line_capacity, "lines", "line_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *line_count = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Mat output;
        detector->value->detectLines(output);
        const int actual_count = static_cast<int>(output.total());
        if (line_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const cv::Vec4f value = output.at<cv::Vec4f>(i);
            const int offset = i * 4;
            lines[offset] = value[0];
            lines[offset + 1] = value[1];
            lines[offset + 2] = value[2];
            lines[offset + 3] = value[3];
        }

        *line_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector; (void)lines;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_count(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* index_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, index_count, "index_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *index_count = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<int> indices = detector->value->getSegmentIndicesOfLines();
        *index_count = static_cast<int>(indices.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_fill(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* indices,
    int index_capacity,
    int* index_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_get_segment_indices_of_lines_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, index_count, "index_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_count_fill_args(api_name, indices, index_capacity, "indices", "index_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *index_count = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<int> output = detector->value->getSegmentIndicesOfLines();
        const int actual_count = static_cast<int>(output.size());
        if (index_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "index_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            indices[i] = output[static_cast<size_t>(i)];
        }

        *index_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector; (void)indices;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_detect_ellipses(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    jyppx_ocv_mat* ellipses)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_detect_ellipses";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, ellipses, "ellipses");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->detectEllipses(opencv_csharp_native::mat_value(ellipses));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_detect_ellipses_count(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    int* ellipse_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_detect_ellipses_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, ellipse_count, "ellipse_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *ellipse_count = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Mat output;
        detector->value->detectEllipses(output);
        *ellipse_count = static_cast<int>(output.total());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_drawing_detect_ellipses_fill(
    jyppx_ocv_ximgproc_edge_drawing* detector,
    double* ellipses,
    int ellipse_capacity,
    int* ellipse_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_drawing_detect_ellipses_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, ellipse_count, "ellipse_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_count_fill_args(api_name, ellipses, ellipse_capacity, "ellipses", "ellipse_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *ellipse_count = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, detector, "detector");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Mat output;
        detector->value->detectEllipses(output);
        const int actual_count = static_cast<int>(output.total());
        if (ellipse_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "ellipse_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const cv::Vec<double, 6> value = output.at<cv::Vec<double, 6> >(i);
            const int offset = i * 6;
            ellipses[offset] = value[0];
            ellipses[offset + 1] = value[1];
            ellipses[offset + 2] = value[2];
            ellipses[offset + 3] = value[3];
            ellipses[offset + 4] = value[4];
            ellipses[offset + 5] = value[5];
        }

        *ellipse_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector; (void)ellipses;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_boxes_create(
    float alpha,
    float beta,
    float eta,
    float min_score,
    int max_boxes,
    float edge_min_mag,
    float edge_merge_thr,
    float cluster_min_mag,
    float max_aspect_ratio,
    float min_box_area,
    float gamma,
    float kappa,
    jyppx_ocv_ximgproc_edge_boxes** edge_boxes)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_boxes_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::createEdgeBoxes(
                alpha,
                beta,
                eta,
                min_score,
                max_boxes,
                edge_min_mag,
                edge_merge_thr,
                cluster_min_mag,
                max_aspect_ratio,
                min_box_area,
                gamma,
                kappa),
            edge_boxes,
            "edge_boxes");
#else
        (void)alpha; (void)beta; (void)eta; (void)min_score; (void)max_boxes; (void)edge_min_mag; (void)edge_merge_thr; (void)cluster_min_mag; (void)max_aspect_ratio; (void)min_box_area; (void)gamma; (void)kappa;
        if (edge_boxes != nullptr) { *edge_boxes = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_edge_boxes_release_handle(jyppx_ocv_ximgproc_edge_boxes* edge_boxes)
{
    delete edge_boxes;
}

int jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_count(
    jyppx_ocv_ximgproc_edge_boxes* edge_boxes,
    const jyppx_ocv_mat* edge_map,
    const jyppx_ocv_mat* orientation_map,
    int* box_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, box_count, "box_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *box_count = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        std::vector<cv::Rect> boxes;
        cv::Mat scores;
        status = edge_boxes_core(api_name, edge_boxes, edge_map, orientation_map, boxes, scores);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *box_count = static_cast<int>(boxes.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)edge_boxes; (void)edge_map; (void)orientation_map;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_fill(
    jyppx_ocv_ximgproc_edge_boxes* edge_boxes,
    const jyppx_ocv_mat* edge_map,
    const jyppx_ocv_mat* orientation_map,
    jyppx_ocv_ximgproc_edge_box* boxes,
    int box_capacity,
    int* box_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_edge_boxes_get_bounding_boxes_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, box_count, "box_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_count_fill_args(api_name, boxes, box_capacity, "boxes", "box_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *box_count = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        std::vector<cv::Rect> native_boxes;
        cv::Mat scores;
        status = edge_boxes_core(api_name, edge_boxes, edge_map, orientation_map, native_boxes, scores);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        const int actual_count = static_cast<int>(native_boxes.size());
        if (box_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "box_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const cv::Rect& rect = native_boxes[static_cast<size_t>(i)];
            boxes[i].x = rect.x;
            boxes[i].y = rect.y;
            boxes[i].width = rect.width;
            boxes[i].height = rect.height;
            boxes[i].score = scores.empty() ? 0.0F : scores.at<float>(i);
        }

        *box_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)edge_boxes; (void)edge_map; (void)orientation_map; (void)boxes;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_alpha, jyppx_ocv_ximgproc_edge_boxes, getAlpha)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_alpha, jyppx_ocv_ximgproc_edge_boxes, setAlpha)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_beta, jyppx_ocv_ximgproc_edge_boxes, getBeta)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_beta, jyppx_ocv_ximgproc_edge_boxes, setBeta)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_eta, jyppx_ocv_ximgproc_edge_boxes, getEta)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_eta, jyppx_ocv_ximgproc_edge_boxes, setEta)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_min_score, jyppx_ocv_ximgproc_edge_boxes, getMinScore)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_min_score, jyppx_ocv_ximgproc_edge_boxes, setMinScore)
OPENCV_CSHARP_XIMGPROC_GET_INT(jyppx_ocv_ximgproc_edge_boxes_get_max_boxes, jyppx_ocv_ximgproc_edge_boxes, getMaxBoxes)
OPENCV_CSHARP_XIMGPROC_SET_INT(jyppx_ocv_ximgproc_edge_boxes_set_max_boxes, jyppx_ocv_ximgproc_edge_boxes, setMaxBoxes)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_edge_min_mag, jyppx_ocv_ximgproc_edge_boxes, getEdgeMinMag)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_edge_min_mag, jyppx_ocv_ximgproc_edge_boxes, setEdgeMinMag)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_edge_merge_thr, jyppx_ocv_ximgproc_edge_boxes, getEdgeMergeThr)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_edge_merge_thr, jyppx_ocv_ximgproc_edge_boxes, setEdgeMergeThr)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_cluster_min_mag, jyppx_ocv_ximgproc_edge_boxes, getClusterMinMag)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_cluster_min_mag, jyppx_ocv_ximgproc_edge_boxes, setClusterMinMag)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_max_aspect_ratio, jyppx_ocv_ximgproc_edge_boxes, getMaxAspectRatio)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_max_aspect_ratio, jyppx_ocv_ximgproc_edge_boxes, setMaxAspectRatio)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_min_box_area, jyppx_ocv_ximgproc_edge_boxes, getMinBoxArea)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_min_box_area, jyppx_ocv_ximgproc_edge_boxes, setMinBoxArea)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_gamma, jyppx_ocv_ximgproc_edge_boxes, getGamma)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_gamma, jyppx_ocv_ximgproc_edge_boxes, setGamma)
OPENCV_CSHARP_XIMGPROC_GET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_get_kappa, jyppx_ocv_ximgproc_edge_boxes, getKappa)
OPENCV_CSHARP_XIMGPROC_SET_FLOAT(jyppx_ocv_ximgproc_edge_boxes_set_kappa, jyppx_ocv_ximgproc_edge_boxes, setKappa)

#undef OPENCV_CSHARP_XIMGPROC_GET_INT
#undef OPENCV_CSHARP_XIMGPROC_SET_INT
#undef OPENCV_CSHARP_XIMGPROC_GET_FLOAT
#undef OPENCV_CSHARP_XIMGPROC_SET_FLOAT
#undef OPENCV_CSHARP_XIMGPROC_GET_BOOL
#undef OPENCV_CSHARP_XIMGPROC_SET_BOOL

int jyppx_ocv_ximgproc_gradient_deriche_x(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, double alpha, double omega)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_gradient_deriche_x";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::GradientDericheX(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), alpha, omega);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)alpha; (void)omega;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_gradient_deriche_y(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, double alpha, double omega)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_gradient_deriche_y";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::GradientDericheY(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), alpha, omega);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)alpha; (void)omega;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_gradient_paillou_x(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, double alpha, double omega)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_gradient_paillou_x";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::GradientPaillouX(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), alpha, omega);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)alpha; (void)omega;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_gradient_paillou_y(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, double alpha, double omega)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_gradient_paillou_y";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::GradientPaillouY(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), alpha, omega);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)alpha; (void)omega;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_ridge_detection_filter_create(
    int ddepth,
    int dx,
    int dy,
    int ksize,
    int out_dtype,
    double scale,
    double delta,
    int border_type,
    jyppx_ocv_ximgproc_ridge_detection_filter** filter)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_ridge_detection_filter_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::RidgeDetectionFilter::create(ddepth, dx, dy, ksize, out_dtype, scale, delta, border_type),
            filter,
            "filter");
#else
        (void)ddepth; (void)dx; (void)dy; (void)ksize; (void)out_dtype; (void)scale; (void)delta; (void)border_type;
        if (filter != nullptr) { *filter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_ridge_detection_filter_release_handle(jyppx_ocv_ximgproc_ridge_detection_filter* filter)
{
    delete filter;
}

int jyppx_ocv_ximgproc_ridge_detection_filter_get_image(
    jyppx_ocv_ximgproc_ridge_detection_filter* filter,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_ridge_detection_filter_get_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, filter, "filter");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        filter->value->getRidgeFilteredImage(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)filter;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_fourier_descriptor(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int nb_elt, int nb_fd)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_fourier_descriptor";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::fourierDescriptor(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), nb_elt, nb_fd);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)nb_elt; (void)nb_fd;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_transform_fd(const jyppx_ocv_mat* src, const jyppx_ocv_mat* transform, jyppx_ocv_mat* dst, int fd_contour)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_transform_fd";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, transform, "transform");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::transformFD(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(transform),
            opencv_csharp_native::mat_value(dst),
            fd_contour != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)fd_contour;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_contour_sampling(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int nb_elt)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_contour_sampling";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::contourSampling(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), nb_elt);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)nb_elt;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_contour_fitting_create(int ctr, int fd, jyppx_ocv_ximgproc_contour_fitting** fitting)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_contour_fitting_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::createContourFitting(ctr, fd), fitting, "fitting");
#else
        (void)ctr; (void)fd;
        if (fitting != nullptr) { *fitting = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_contour_fitting_release_handle(jyppx_ocv_ximgproc_contour_fitting* fitting)
{
    delete fitting;
}

int jyppx_ocv_ximgproc_contour_fitting_estimate_transformation(
    jyppx_ocv_ximgproc_contour_fitting* fitting,
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* dst,
    jyppx_ocv_mat* alpha_phi_st,
    double* distance,
    int fd_contour)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_contour_fitting_estimate_transformation";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, alpha_phi_st, "alpha_phi_st");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, distance, "distance");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *distance = 0.0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, fitting, "fitting");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        fitting->value->estimateTransformation(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(alpha_phi_st),
            *distance,
            fd_contour != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)fitting; (void)fd_contour;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
#define OPENCV_CSHARP_XIMGPROC_CF_GET_INT(function_name, getter_name) \
int function_name(jyppx_ocv_ximgproc_contour_fitting* fitting, int* value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_output_int(api_name, value, "value"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = 0; \
        status = validate_handle(api_name, fitting, "fitting"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = fitting->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}
#define OPENCV_CSHARP_XIMGPROC_CF_SET_INT(function_name, setter_name) \
int function_name(jyppx_ocv_ximgproc_contour_fitting* fitting, int value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_handle(api_name, fitting, "fitting"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        fitting->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}
#else
#define OPENCV_CSHARP_XIMGPROC_CF_GET_INT(function_name, getter_name) \
int function_name(jyppx_ocv_ximgproc_contour_fitting* fitting, int* value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    int status = validate_output_int(api_name, value, "value"); \
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
    (void)fitting; *value = 0; \
    return opencv_csharp_native::set_not_linked(api_name); \
}
#define OPENCV_CSHARP_XIMGPROC_CF_SET_INT(function_name, setter_name) \
int function_name(jyppx_ocv_ximgproc_contour_fitting* fitting, int value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    (void)fitting; (void)value; \
    return opencv_csharp_native::set_not_linked(api_name); \
}
#endif

OPENCV_CSHARP_XIMGPROC_CF_GET_INT(jyppx_ocv_ximgproc_contour_fitting_get_ctr_size, getCtrSize)
OPENCV_CSHARP_XIMGPROC_CF_SET_INT(jyppx_ocv_ximgproc_contour_fitting_set_ctr_size, setCtrSize)
OPENCV_CSHARP_XIMGPROC_CF_GET_INT(jyppx_ocv_ximgproc_contour_fitting_get_fd_size, getFDSize)
OPENCV_CSHARP_XIMGPROC_CF_SET_INT(jyppx_ocv_ximgproc_contour_fitting_set_fd_size, setFDSize)

#undef OPENCV_CSHARP_XIMGPROC_CF_GET_INT
#undef OPENCV_CSHARP_XIMGPROC_CF_SET_INT

int jyppx_ocv_ximgproc_rl_threshold(const jyppx_ocv_mat* src, jyppx_ocv_mat* rl_dst, double thresh, int type)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_rl_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rl_dst, "rl_dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::rl::threshold(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(rl_dst), thresh, type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)thresh; (void)type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_rl_dilate(
    const jyppx_ocv_mat* rl_src,
    jyppx_ocv_mat* rl_dst,
    const jyppx_ocv_mat* rl_kernel,
    int anchor_x,
    int anchor_y)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_rl_dilate";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, rl_src, "rl_src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rl_dst, "rl_dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rl_kernel, "rl_kernel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::rl::dilate(
            opencv_csharp_native::mat_value(rl_src),
            opencv_csharp_native::mat_value(rl_dst),
            opencv_csharp_native::mat_value(rl_kernel),
            cv::Point(anchor_x, anchor_y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)anchor_x; (void)anchor_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_rl_erode(
    const jyppx_ocv_mat* rl_src,
    jyppx_ocv_mat* rl_dst,
    const jyppx_ocv_mat* rl_kernel,
    int boundary_on,
    int anchor_x,
    int anchor_y)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_rl_erode";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, rl_src, "rl_src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rl_dst, "rl_dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rl_kernel, "rl_kernel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::rl::erode(
            opencv_csharp_native::mat_value(rl_src),
            opencv_csharp_native::mat_value(rl_dst),
            opencv_csharp_native::mat_value(rl_kernel),
            boundary_on != 0,
            cv::Point(anchor_x, anchor_y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)boundary_on; (void)anchor_x; (void)anchor_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_rl_get_structuring_element(int shape, int width, int height, jyppx_ocv_mat* rl_kernel)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_rl_get_structuring_element";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, rl_kernel, "rl_kernel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        opencv_csharp_native::mat_value(rl_kernel) = cv::ximgproc::rl::getStructuringElement(shape, cv::Size(width, height));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)shape; (void)width; (void)height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_rl_paint(
    jyppx_ocv_mat* image,
    const jyppx_ocv_mat* rl_src,
    double value_v0,
    double value_v1,
    double value_v2,
    double value_v3)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_rl_paint";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rl_src, "rl_src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::rl::paint(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(rl_src),
            cv::Scalar(value_v0, value_v1, value_v2, value_v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)value_v0; (void)value_v1; (void)value_v2; (void)value_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_rl_is_morphology_possible(const jyppx_ocv_mat* rl_structuring_element, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_rl_is_morphology_possible";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = 0;
        status = validate_mat(api_name, rl_structuring_element, "rl_structuring_element");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        *value = cv::ximgproc::rl::isRLMorphologyPossible(opencv_csharp_native::mat_value(rl_structuring_element)) ? 1 : 0;
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

int jyppx_ocv_ximgproc_rl_create_rle_image(
    const jyppx_ocv_ximgproc_point3i* runs,
    int run_count,
    int width,
    int height,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_rl_create_rle_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (run_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "run_count");
        }
        if (runs == nullptr && run_count != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "runs");
        }
        int status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        std::vector<cv::Point3i> native_runs;
        native_runs.reserve(static_cast<size_t>(run_count));
        for (int i = 0; i < run_count; ++i)
        {
            native_runs.emplace_back(runs[i].x, runs[i].y, runs[i].z);
        }
        cv::ximgproc::rl::createRLEImage(native_runs, opencv_csharp_native::mat_value(dst), cv::Size(width, height));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)runs; (void)width; (void)height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_rl_morphology_ex(
    const jyppx_ocv_mat* rl_src,
    jyppx_ocv_mat* rl_dst,
    int op,
    const jyppx_ocv_mat* rl_kernel,
    int boundary_on_for_erosion,
    int anchor_x,
    int anchor_y)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_rl_morphology_ex";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, rl_src, "rl_src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rl_dst, "rl_dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rl_kernel, "rl_kernel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::rl::morphologyEx(
            opencv_csharp_native::mat_value(rl_src),
            opencv_csharp_native::mat_value(rl_dst),
            op,
            opencv_csharp_native::mat_value(rl_kernel),
            boundary_on_for_erosion != 0,
            cv::Point(anchor_x, anchor_y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)op; (void)boundary_on_for_erosion; (void)anchor_x; (void)anchor_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_scan_segment_create(
    int image_width,
    int image_height,
    int num_superpixels,
    int slices,
    int merge_small,
    jyppx_ocv_ximgproc_scan_segment** segment)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_scan_segment_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(
            api_name,
            cv::ximgproc::createScanSegment(image_width, image_height, num_superpixels, slices, merge_small != 0),
            segment,
            "segment");
#else
        (void)image_width; (void)image_height; (void)num_superpixels; (void)slices; (void)merge_small;
        if (segment != nullptr) { *segment = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_scan_segment_release_handle(jyppx_ocv_ximgproc_scan_segment* segment)
{
    delete segment;
}

int jyppx_ocv_ximgproc_scan_segment_get_number(jyppx_ocv_ximgproc_scan_segment* segment, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_scan_segment_get_number";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, segment, "segment");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = segment->value->getNumberOfSuperpixels();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segment;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_scan_segment_iterate(jyppx_ocv_ximgproc_scan_segment* segment, const jyppx_ocv_mat* image)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_scan_segment_iterate";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, segment, "segment");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segment->value->iterate(opencv_csharp_native::mat_value(image));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segment;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_scan_segment_get_labels(jyppx_ocv_ximgproc_scan_segment* segment, jyppx_ocv_mat* labels)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_scan_segment_get_labels";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, labels, "labels");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, segment, "segment");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segment->value->getLabels(opencv_csharp_native::mat_value(labels));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segment;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_scan_segment_get_label_contour_mask(
    jyppx_ocv_ximgproc_scan_segment* segment,
    jyppx_ocv_mat* image,
    int thick_line)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_scan_segment_get_label_contour_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, segment, "segment");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segment->value->getLabelContourMask(opencv_csharp_native::mat_value(image), thick_line != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segment; (void)thick_line;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_graph_segmentation_create(
    double sigma,
    float k,
    int min_size,
    jyppx_ocv_ximgproc_graph_segmentation** segmentation)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_graph_segmentation_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::segmentation::createGraphSegmentation(sigma, k, min_size), segmentation, "segmentation");
#else
        (void)sigma; (void)k; (void)min_size;
        if (segmentation != nullptr) { *segmentation = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_graph_segmentation_release_handle(jyppx_ocv_ximgproc_graph_segmentation* segmentation)
{
    delete segmentation;
}

int jyppx_ocv_ximgproc_graph_segmentation_process_image(
    jyppx_ocv_ximgproc_graph_segmentation* segmentation,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_graph_segmentation_process_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->processImage(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
#define OPENCV_CSHARP_XIMGPROC_GRAPH_GET_DOUBLE(function_name, getter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, double* value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_output_double(api_name, value, "value"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = 0.0; \
        status = validate_handle(api_name, segmentation, "segmentation"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = segmentation->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}
#define OPENCV_CSHARP_XIMGPROC_GRAPH_SET_DOUBLE(function_name, setter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, double value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_handle(api_name, segmentation, "segmentation"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        segmentation->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}
#define OPENCV_CSHARP_XIMGPROC_GRAPH_GET_FLOAT(function_name, getter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, float* value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_output_float(api_name, value, "value"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = 0.0F; \
        status = validate_handle(api_name, segmentation, "segmentation"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = segmentation->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}
#define OPENCV_CSHARP_XIMGPROC_GRAPH_SET_FLOAT(function_name, setter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, float value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_handle(api_name, segmentation, "segmentation"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        segmentation->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}
#define OPENCV_CSHARP_XIMGPROC_GRAPH_GET_INT(function_name, getter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, int* value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_output_int(api_name, value, "value"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = 0; \
        status = validate_handle(api_name, segmentation, "segmentation"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        *value = segmentation->value->getter_name(); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}
#define OPENCV_CSHARP_XIMGPROC_GRAPH_SET_INT(function_name, setter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, int value) \
{ \
    constexpr const char* api_name = #function_name; \
    try \
    { \
        opencv_csharp_native::clear_last_error(); \
        int status = validate_handle(api_name, segmentation, "segmentation"); \
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
        segmentation->value->setter_name(value); \
        return OPENCV_CSHARP_STATUS_OK; \
    } \
    catch (...) \
    { \
        return opencv_csharp_native::translate_current_exception(api_name); \
    } \
}
#else
#define OPENCV_CSHARP_XIMGPROC_GRAPH_GET_DOUBLE(function_name, getter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, double* value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    int status = validate_output_double(api_name, value, "value"); \
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
    (void)segmentation; *value = 0.0; \
    return opencv_csharp_native::set_not_linked(api_name); \
}
#define OPENCV_CSHARP_XIMGPROC_GRAPH_SET_DOUBLE(function_name, setter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, double value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    (void)segmentation; (void)value; \
    return opencv_csharp_native::set_not_linked(api_name); \
}
#define OPENCV_CSHARP_XIMGPROC_GRAPH_GET_FLOAT(function_name, getter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, float* value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    int status = validate_output_float(api_name, value, "value"); \
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
    (void)segmentation; *value = 0.0F; \
    return opencv_csharp_native::set_not_linked(api_name); \
}
#define OPENCV_CSHARP_XIMGPROC_GRAPH_SET_FLOAT(function_name, setter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, float value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    (void)segmentation; (void)value; \
    return opencv_csharp_native::set_not_linked(api_name); \
}
#define OPENCV_CSHARP_XIMGPROC_GRAPH_GET_INT(function_name, getter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, int* value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    int status = validate_output_int(api_name, value, "value"); \
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
    (void)segmentation; *value = 0; \
    return opencv_csharp_native::set_not_linked(api_name); \
}
#define OPENCV_CSHARP_XIMGPROC_GRAPH_SET_INT(function_name, setter_name) \
int function_name(jyppx_ocv_ximgproc_graph_segmentation* segmentation, int value) \
{ \
    constexpr const char* api_name = #function_name; \
    opencv_csharp_native::clear_last_error(); \
    (void)segmentation; (void)value; \
    return opencv_csharp_native::set_not_linked(api_name); \
}
#endif

OPENCV_CSHARP_XIMGPROC_GRAPH_GET_DOUBLE(jyppx_ocv_ximgproc_graph_segmentation_get_sigma, getSigma)
OPENCV_CSHARP_XIMGPROC_GRAPH_SET_DOUBLE(jyppx_ocv_ximgproc_graph_segmentation_set_sigma, setSigma)
OPENCV_CSHARP_XIMGPROC_GRAPH_GET_FLOAT(jyppx_ocv_ximgproc_graph_segmentation_get_k, getK)
OPENCV_CSHARP_XIMGPROC_GRAPH_SET_FLOAT(jyppx_ocv_ximgproc_graph_segmentation_set_k, setK)
OPENCV_CSHARP_XIMGPROC_GRAPH_GET_INT(jyppx_ocv_ximgproc_graph_segmentation_get_min_size, getMinSize)
OPENCV_CSHARP_XIMGPROC_GRAPH_SET_INT(jyppx_ocv_ximgproc_graph_segmentation_set_min_size, setMinSize)

#undef OPENCV_CSHARP_XIMGPROC_GRAPH_GET_DOUBLE
#undef OPENCV_CSHARP_XIMGPROC_GRAPH_SET_DOUBLE
#undef OPENCV_CSHARP_XIMGPROC_GRAPH_GET_FLOAT
#undef OPENCV_CSHARP_XIMGPROC_GRAPH_SET_FLOAT
#undef OPENCV_CSHARP_XIMGPROC_GRAPH_GET_INT
#undef OPENCV_CSHARP_XIMGPROC_GRAPH_SET_INT

int jyppx_ocv_ximgproc_selective_search_strategy_create_color(jyppx_ocv_ximgproc_selective_search_strategy** strategy)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_strategy_create_color";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::segmentation::createSelectiveSearchSegmentationStrategyColor(), strategy, "strategy");
#else
        if (strategy != nullptr) { *strategy = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_strategy_create_size(jyppx_ocv_ximgproc_selective_search_strategy** strategy)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_strategy_create_size";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::segmentation::createSelectiveSearchSegmentationStrategySize(), strategy, "strategy");
#else
        if (strategy != nullptr) { *strategy = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_strategy_create_texture(jyppx_ocv_ximgproc_selective_search_strategy** strategy)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_strategy_create_texture";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::segmentation::createSelectiveSearchSegmentationStrategyTexture(), strategy, "strategy");
#else
        if (strategy != nullptr) { *strategy = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_strategy_create_fill(jyppx_ocv_ximgproc_selective_search_strategy** strategy)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_strategy_create_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::segmentation::createSelectiveSearchSegmentationStrategyFill(), strategy, "strategy");
#else
        if (strategy != nullptr) { *strategy = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_strategy_create_multiple(jyppx_ocv_ximgproc_selective_search_strategy** strategy)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_strategy_create_multiple";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::segmentation::createSelectiveSearchSegmentationStrategyMultiple(), strategy, "strategy");
#else
        if (strategy != nullptr) { *strategy = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_selective_search_strategy_release_handle(jyppx_ocv_ximgproc_selective_search_strategy* strategy)
{
    delete strategy;
}

int jyppx_ocv_ximgproc_selective_search_strategy_set_image(
    jyppx_ocv_ximgproc_selective_search_strategy* strategy,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* regions,
    const jyppx_ocv_mat* sizes,
    int image_id)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_strategy_set_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, regions, "regions");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, sizes, "sizes");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, strategy, "strategy");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        strategy->value->setImage(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(regions),
            opencv_csharp_native::mat_value(sizes),
            image_id);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)strategy; (void)image_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_strategy_get(
    jyppx_ocv_ximgproc_selective_search_strategy* strategy,
    int r1,
    int r2,
    float* value)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_strategy_get";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = 0.0F;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, strategy, "strategy");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *value = strategy->value->get(r1, r2);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)strategy; (void)r1; (void)r2;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_strategy_merge(
    jyppx_ocv_ximgproc_selective_search_strategy* strategy,
    int r1,
    int r2)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_strategy_merge";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, strategy, "strategy");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        strategy->value->merge(r1, r2);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)strategy; (void)r1; (void)r2;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_strategy_multiple_add(
    jyppx_ocv_ximgproc_selective_search_strategy* multiple,
    jyppx_ocv_ximgproc_selective_search_strategy* strategy,
    float weight)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_strategy_multiple_add";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, multiple, "multiple");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_handle(api_name, strategy, "strategy");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Ptr<cv::ximgproc::segmentation::SelectiveSearchSegmentationStrategyMultiple> native_multiple =
            multiple->value.dynamicCast<cv::ximgproc::segmentation::SelectiveSearchSegmentationStrategyMultiple>();
        if (native_multiple.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "multiple");
        }
        native_multiple->addStrategy(strategy->value, weight);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)multiple; (void)strategy; (void)weight;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_strategy_multiple_clear(jyppx_ocv_ximgproc_selective_search_strategy* multiple)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_strategy_multiple_clear";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, multiple, "multiple");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Ptr<cv::ximgproc::segmentation::SelectiveSearchSegmentationStrategyMultiple> native_multiple =
            multiple->value.dynamicCast<cv::ximgproc::segmentation::SelectiveSearchSegmentationStrategyMultiple>();
        if (native_multiple.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "multiple");
        }
        native_multiple->clearStrategies();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)multiple;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_create(jyppx_ocv_ximgproc_selective_search_segmentation** segmentation)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        return create_handle(api_name, cv::ximgproc::segmentation::createSelectiveSearchSegmentation(), segmentation, "segmentation");
#else
        if (segmentation != nullptr) { *segmentation = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_ximgproc_selective_search_segmentation_release_handle(jyppx_ocv_ximgproc_selective_search_segmentation* segmentation)
{
    delete segmentation;
}

int jyppx_ocv_ximgproc_selective_search_segmentation_set_base_image(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    const jyppx_ocv_mat* image)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_set_base_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->setBaseImage(opencv_csharp_native::mat_value(image));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_single_strategy(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    int k,
    float sigma)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_single_strategy";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->switchToSingleStrategy(k, sigma);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation; (void)k; (void)sigma;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_fast(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    int base_k,
    int inc_k,
    float sigma)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_fast";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->switchToSelectiveSearchFast(base_k, inc_k, sigma);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation; (void)base_k; (void)inc_k; (void)sigma;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_quality(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    int base_k,
    int inc_k,
    float sigma)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_switch_to_quality";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->switchToSelectiveSearchQuality(base_k, inc_k, sigma);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation; (void)base_k; (void)inc_k; (void)sigma;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_add_image(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    const jyppx_ocv_mat* image)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_add_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->addImage(opencv_csharp_native::mat_value(image));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_clear_images(jyppx_ocv_ximgproc_selective_search_segmentation* segmentation)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_clear_images";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->clearImages();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_add_graph_segmentation(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    jyppx_ocv_ximgproc_graph_segmentation* graph_segmentation)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_add_graph_segmentation";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_handle(api_name, graph_segmentation, "graph_segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->addGraphSegmentation(graph_segmentation->value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation; (void)graph_segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_clear_graph_segmentations(jyppx_ocv_ximgproc_selective_search_segmentation* segmentation)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_clear_graph_segmentations";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->clearGraphSegmentations();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_add_strategy(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    jyppx_ocv_ximgproc_selective_search_strategy* strategy)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_add_strategy";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_handle(api_name, strategy, "strategy");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->addStrategy(strategy->value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation; (void)strategy;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_clear_strategies(jyppx_ocv_ximgproc_selective_search_segmentation* segmentation)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_clear_strategies";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        int status = validate_handle(api_name, segmentation, "segmentation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        segmentation->value->clearStrategies();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
static int selective_search_process_core(
    const char* api_name,
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    std::vector<cv::Rect>& rects)
{
    int status = validate_handle(api_name, segmentation, "segmentation");
    if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
    segmentation->value->process(rects);
    return OPENCV_CSHARP_STATUS_OK;
}
#endif

int jyppx_ocv_ximgproc_selective_search_segmentation_process_count(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    int* rect_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_process_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, rect_count, "rect_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *rect_count = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        std::vector<cv::Rect> rects;
        status = selective_search_process_core(api_name, segmentation, rects);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *rect_count = static_cast<int>(rects.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_selective_search_segmentation_process_fill(
    jyppx_ocv_ximgproc_selective_search_segmentation* segmentation,
    jyppx_ocv_ximgproc_rect* rects,
    int rect_capacity,
    int* rect_count)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_selective_search_segmentation_process_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, rect_count, "rect_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_count_fill_args(api_name, rects, rect_capacity, "rects", "rect_capacity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *rect_count = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        std::vector<cv::Rect> native_rects;
        status = selective_search_process_core(api_name, segmentation, native_rects);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        const int actual_count = static_cast<int>(native_rects.size());
        if (rect_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rect_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            from_rect(native_rects[static_cast<size_t>(i)], &rects[i]);
        }

        *rect_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)segmentation; (void)rects;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ximgproc_covariance_estimation(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int window_rows,
    int window_cols)
{
    constexpr const char* api_name = "jyppx_ocv_ximgproc_covariance_estimation";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_XIMGPROC)
        cv::ximgproc::covarianceEstimation(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            window_rows,
            window_cols);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)window_rows; (void)window_cols;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

