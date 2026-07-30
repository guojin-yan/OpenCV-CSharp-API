#include "open_cv_sharp/imgproc.h"

#include "core/mat_handle.h"
#include "error_state.h"

#include <new>
#include <cstring>
#include <functional>
#include <string>
#include <vector>

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/imgproc.hpp>
#if defined(OPENCV_CSHARP_HAS_OPENCV_GEOMETRY)
#include <opencv2/geometry/2d.hpp>
#endif
#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES)
#include <opencv2/features.hpp>
#endif
#endif

struct jyppx_ocv_clahe
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::CLAHE> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_line_segment_detector
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::LineSegmentDetector> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_generalized_hough
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::Ptr<cv::GeneralizedHough> value;
    cv::Ptr<cv::GeneralizedHoughBallard> ballard;
    cv::Ptr<cv::GeneralizedHoughGuil> guil;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_font_face
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::FontFace value;

    jyppx_ocv_font_face() = default;

    explicit jyppx_ocv_font_face(const char* font_path_or_name)
        : value(font_path_or_name)
    {
    }
#else
    int placeholder;
#endif
};

namespace
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::InputArray input_or_no_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::OutputArray output_or_no_array(jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::OutputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::Point2f get_point2f(float x, float y)
    {
        return cv::Point2f(x, y);
    }

    cv::Scalar get_morphology_border_value(
        int has_border_value,
        double border_value_v0,
        double border_value_v1,
        double border_value_v2,
        double border_value_v3)
    {
        return has_border_value != 0
            ? cv::Scalar(border_value_v0, border_value_v1, border_value_v2, border_value_v3)
            : cv::morphologyDefaultBorderValue();
    }

    typedef void (*morphology_function)(
        cv::InputArray src,
        cv::OutputArray dst,
        cv::InputArray kernel,
        cv::Point anchor,
        int iterations,
        int borderType,
        const cv::Scalar& borderValue);

    int run_morphology(
        const char* api_name,
        morphology_function operation,
        const jyppx_ocv_mat* src,
        jyppx_ocv_mat* dst,
        const jyppx_ocv_mat* kernel,
        int anchor_x,
        int anchor_y,
        int iterations,
        int border_type,
        int has_border_value,
        double border_value_v0,
        double border_value_v1,
        double border_value_v2,
        double border_value_v3)
    {
        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (kernel == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "kernel");
        }

        if (iterations < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "iterations");
        }

        operation(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(kernel),
            cv::Point(anchor_x, anchor_y),
            iterations,
            border_type,
            get_morphology_border_value(
                has_border_value,
                border_value_v0,
                border_value_v1,
                border_value_v2,
                border_value_v3));

        return OPENCV_CSHARP_STATUS_OK;
    }

    int run_morphology_ex(
        const char* api_name,
        const jyppx_ocv_mat* src,
        jyppx_ocv_mat* dst,
        int op,
        const jyppx_ocv_mat* kernel,
        int anchor_x,
        int anchor_y,
        int iterations,
        int border_type,
        int has_border_value,
        double border_value_v0,
        double border_value_v1,
        double border_value_v2,
        double border_value_v3)
    {
        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (kernel == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "kernel");
        }

        if (iterations < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "iterations");
        }

        cv::morphologyEx(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            op,
            opencv_csharp_native::mat_value(kernel),
            cv::Point(anchor_x, anchor_y),
            iterations,
            border_type,
            get_morphology_border_value(
                has_border_value,
                border_value_v0,
                border_value_v1,
                border_value_v2,
                border_value_v3));

        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::Scalar get_scalar(
        double value_v0,
        double value_v1,
        double value_v2,
        double value_v3)
    {
        return cv::Scalar(value_v0, value_v1, value_v2, value_v3);
    }

    std::vector<cv::Point2f> get_point2f_points_from_xy(const float* points_xy, int point_count)
    {
        std::vector<cv::Point2f> points;
        points.reserve(static_cast<size_t>(point_count));
        for (int i = 0; i < point_count; ++i)
        {
            const int offset = i * 2;
            points.push_back(cv::Point2f(points_xy[offset], points_xy[offset + 1]));
        }

        return points;
    }

    cv::Mat get_point2f_mat_from_xy(const float* points_xy, int point_count)
    {
        std::vector<cv::Point2f> points = get_point2f_points_from_xy(points_xy, point_count);
        cv::Mat mat(points, true);
        return mat.reshape(2, point_count);
    }

    std::vector<cv::Point> get_ellipse2_poly_points(
        int center_x,
        int center_y,
        int axes_width,
        int axes_height,
        int angle,
        int arc_start,
        int arc_end,
        int delta)
    {
        std::vector<cv::Point> points;
        cv::ellipse2Poly(
            cv::Point(center_x, center_y),
            cv::Size(axes_width, axes_height),
            angle,
            arc_start,
            arc_end,
            delta,
            points);
        return points;
    }

    std::vector<cv::Point> get_points_from_xy(const int* points_xy, int point_count)
    {
        std::vector<cv::Point> points;
        points.reserve(static_cast<size_t>(point_count));
        for (int i = 0; i < point_count; ++i)
        {
            const int offset = i * 2;
            points.push_back(cv::Point(points_xy[offset], points_xy[offset + 1]));
        }

        return points;
    }

    cv::Mat get_points_mat_from_xy(const int* points_xy, int point_count)
    {
        std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        cv::Mat mat(points, true);
        return mat.reshape(2, point_count);
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

    int validate_clahe(const char* api_name, const jyppx_ocv_clahe* clahe)
    {
        if (clahe == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "clahe");
        }

        if (clahe->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "clahe");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_line_segment_detector(const char* api_name, const jyppx_ocv_line_segment_detector* detector)
    {
        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        if (detector->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_generalized_hough(const char* api_name, const jyppx_ocv_generalized_hough* hough)
    {
        if (hough == nullptr || hough->value.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hough");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int hough_lines_core(
        const char* api_name,
        const jyppx_ocv_mat* image,
        double rho,
        double theta,
        int threshold,
        double srn,
        double stn,
        double min_theta,
        double max_theta,
        int use_edgeval,
        cv::Mat& lines)
    {
        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        cv::HoughLines(
            opencv_csharp_native::mat_value(image),
            lines,
            rho,
            theta,
            threshold,
            srn,
            stn,
            min_theta,
            max_theta,
            use_edgeval != 0);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int hough_lines_p_core(
        const char* api_name,
        const jyppx_ocv_mat* image,
        double rho,
        double theta,
        int threshold,
        double min_line_length,
        double max_line_gap,
        cv::Mat& lines)
    {
        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        cv::HoughLinesP(
            opencv_csharp_native::mat_value(image),
            lines,
            rho,
            theta,
            threshold,
            min_line_length,
            max_line_gap);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int hough_lines_point_set_core(
        const char* api_name,
        const int* points_xy,
        int point_count,
        int lines_max,
        int threshold,
        double min_rho,
        double max_rho,
        double rho_step,
        double min_theta,
        double max_theta,
        double theta_step,
        cv::Mat& lines)
    {
        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        const cv::Mat points = get_points_mat_from_xy(points_xy, point_count);
        cv::HoughLinesPointSet(
            points,
            lines,
            lines_max,
            threshold,
            min_rho,
            max_rho,
            rho_step,
            min_theta,
            max_theta,
            theta_step);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int hough_circles_core(
        const char* api_name,
        const jyppx_ocv_mat* image,
        int method,
        double dp,
        double min_dist,
        double param1,
        double param2,
        int min_radius,
        int max_radius,
        cv::Mat& circles)
    {
        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        cv::HoughCircles(
            opencv_csharp_native::mat_value(image),
            circles,
            method,
            dp,
            min_dist,
            param1,
            param2,
            min_radius,
            max_radius);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int line_segment_detect_core(
        const char* api_name,
        jyppx_ocv_line_segment_detector* detector,
        const jyppx_ocv_mat* image,
        cv::Mat& lines)
    {
        int status = validate_line_segment_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        detector->value->detect(opencv_csharp_native::mat_value(image), lines);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int calc_hist_uniform_core(
        const char* api_name,
        const jyppx_ocv_mat* image,
        const jyppx_ocv_mat* mask,
        const int* channels,
        int channel_count,
        jyppx_ocv_mat* hist,
        const int* hist_size,
        int hist_dims,
        const float* ranges,
        int range_count,
        int accumulate)
    {
        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (hist == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hist");
        }

        if (channels == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "channels");
        }

        if (hist_size == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hist_size");
        }

        if (ranges == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "ranges");
        }

        if (channel_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "channel_count");
        }

        if (hist_dims <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hist_dims");
        }

        if (range_count < hist_dims * 2)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "range_count");
        }

        std::vector<const float*> range_ptrs(static_cast<size_t>(hist_dims));
        for (int i = 0; i < hist_dims; ++i)
        {
            range_ptrs[static_cast<size_t>(i)] = ranges + (i * 2);
        }

        const cv::Mat* images = &opencv_csharp_native::mat_value(image);
        cv::calcHist(
            images,
            1,
            channels,
            mask == nullptr ? cv::Mat() : opencv_csharp_native::mat_value(mask),
            opencv_csharp_native::mat_value(hist),
            hist_dims,
            hist_size,
            range_ptrs.data(),
            true,
            accumulate != 0);
        return OPENCV_CSHARP_STATUS_OK;
    }

    std::vector<cv::Point> get_approx_poly_dp_points(
        const int* curve_xy,
        int point_count,
        double epsilon,
        int closed)
    {
        const std::vector<cv::Point> curve = get_points_from_xy(curve_xy, point_count);
        std::vector<cv::Point> approx_curve;
        cv::approxPolyDP(curve, approx_curve, epsilon, closed != 0);
        return approx_curve;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV_GEOMETRY)
    std::vector<cv::Point2f> get_approx_poly_n_points(
        const int* curve_xy,
        int point_count,
        int nsides,
        float epsilon_percentage,
        int ensure_convex)
    {
        const std::vector<cv::Point> curve = get_points_from_xy(curve_xy, point_count);
        std::vector<cv::Point2f> approx_curve;
        cv::approxPolyN(curve, approx_curve, nsides, epsilon_percentage, ensure_convex != 0);
        return approx_curve;
    }
#endif

    std::vector<cv::Point> get_convex_hull_points(
        const int* points_xy,
        int point_count,
        int clockwise)
    {
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        std::vector<cv::Point> hull;
        cv::convexHull(points, hull, clockwise != 0, true);
        return hull;
    }

    std::vector<int> get_convex_hull_indices(
        const int* points_xy,
        int point_count,
        int clockwise)
    {
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        std::vector<int> hull_indices;
        cv::convexHull(points, hull_indices, clockwise != 0, false);
        return hull_indices;
    }

    std::vector<cv::Vec4i> get_convexity_defects(
        const int* contour_xy,
        int contour_point_count,
        const int* hull_indices,
        int hull_index_count)
    {
        const std::vector<cv::Point> contour = get_points_from_xy(contour_xy, contour_point_count);
        std::vector<int> hull(
            hull_indices,
            hull_indices + hull_index_count);
        std::vector<cv::Vec4i> defects;
        cv::convexityDefects(contour, hull, defects);
        return defects;
    }

    void write_rotated_rect(
        const cv::RotatedRect& rect,
        float* center_x,
        float* center_y,
        float* width,
        float* height,
        float* angle)
    {
        *center_x = rect.center.x;
        *center_y = rect.center.y;
        *width = rect.size.width;
        *height = rect.size.height;
        *angle = rect.angle;
    }

    cv::RotatedRect get_rotated_rect(
        float center_x,
        float center_y,
        float width,
        float height,
        float angle)
    {
        return cv::RotatedRect(cv::Point2f(center_x, center_y), cv::Size2f(width, height), angle);
    }

    void write_point2f_points(
        const std::vector<cv::Point2f>& points,
        float* points_xy)
    {
        for (int i = 0; i < static_cast<int>(points.size()); ++i)
        {
            const int offset = i * 2;
            points_xy[offset] = points[static_cast<size_t>(i)].x;
            points_xy[offset + 1] = points[static_cast<size_t>(i)].y;
        }
    }

    void write_int_values(
        const std::vector<int>& values,
        int* output)
    {
        for (int i = 0; i < static_cast<int>(values.size()); ++i)
        {
            output[i] = values[static_cast<size_t>(i)];
        }
    }

    void write_vec4i_values(
        const std::vector<cv::Vec4i>& values,
        int* output)
    {
        for (int i = 0; i < static_cast<int>(values.size()); ++i)
        {
            const int offset = i * 4;
            const cv::Vec4i& value = values[static_cast<size_t>(i)];
            output[offset] = value[0];
            output[offset + 1] = value[1];
            output[offset + 2] = value[2];
            output[offset + 3] = value[3];
        }
    }

    std::vector<cv::Point2f> get_intersect_convex_convex_points(
        const int* polygon1_xy,
        int polygon1_point_count,
        const int* polygon2_xy,
        int polygon2_point_count,
        int handle_nested,
        float* area)
    {
        const std::vector<cv::Point> polygon1 = get_points_from_xy(polygon1_xy, polygon1_point_count);
        const std::vector<cv::Point> polygon2 = get_points_from_xy(polygon2_xy, polygon2_point_count);
        std::vector<cv::Point2f> intersecting_region;
        *area = cv::intersectConvexConvex(polygon1, polygon2, intersecting_region, handle_nested != 0);
        return intersecting_region;
    }

    std::vector<cv::Point2f> get_point2f_points(
        const std::vector<cv::Point>& points)
    {
        std::vector<cv::Point2f> result;
        result.reserve(points.size());
        for (size_t i = 0; i < points.size(); ++i)
        {
            result.push_back(cv::Point2f(
                static_cast<float>(points[i].x),
                static_cast<float>(points[i].y)));
        }

        return result;
    }

    int get_rotated_rectangle_intersection_points(
        float rect1_center_x,
        float rect1_center_y,
        float rect1_width,
        float rect1_height,
        float rect1_angle,
        float rect2_center_x,
        float rect2_center_y,
        float rect2_width,
        float rect2_height,
        float rect2_angle,
        std::vector<cv::Point2f>& points)
    {
        const cv::RotatedRect rect1 = get_rotated_rect(rect1_center_x, rect1_center_y, rect1_width, rect1_height, rect1_angle);
        const cv::RotatedRect rect2 = get_rotated_rect(rect2_center_x, rect2_center_y, rect2_width, rect2_height, rect2_angle);
        return cv::rotatedRectangleIntersection(rect1, rect2, points);
    }

    typedef cv::RotatedRect (*fit_ellipse_function)(cv::InputArray points);

    int run_fit_ellipse(
        const char* api_name,
        fit_ellipse_function operation,
        const int* points_xy,
        int point_count,
        float* center_x,
        float* center_y,
        float* width,
        float* height,
        float* angle)
    {
        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count < 5)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (center_x == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "center_x");
        }

        if (center_y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "center_y");
        }

        if (width == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

        if (angle == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "angle");
        }

        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        const cv::RotatedRect rect = operation(points);
        write_rotated_rect(rect, center_x, center_y, width, height, angle);
        return OPENCV_CSHARP_STATUS_OK;
    }

    void write_moments_values(const cv::Moments& moments, double* values)
    {
        values[0] = moments.m00;
        values[1] = moments.m10;
        values[2] = moments.m01;
        values[3] = moments.m20;
        values[4] = moments.m11;
        values[5] = moments.m02;
        values[6] = moments.m30;
        values[7] = moments.m21;
        values[8] = moments.m12;
        values[9] = moments.m03;
        values[10] = moments.mu20;
        values[11] = moments.mu11;
        values[12] = moments.mu02;
        values[13] = moments.mu30;
        values[14] = moments.mu21;
        values[15] = moments.mu12;
        values[16] = moments.mu03;
        values[17] = moments.nu20;
        values[18] = moments.nu11;
        values[19] = moments.nu02;
        values[20] = moments.nu30;
        values[21] = moments.nu21;
        values[22] = moments.nu12;
        values[23] = moments.nu03;
    }

    cv::Moments get_moments_from_values(const double* values)
    {
        cv::Moments moments;
        moments.m00 = values[0];
        moments.m10 = values[1];
        moments.m01 = values[2];
        moments.m20 = values[3];
        moments.m11 = values[4];
        moments.m02 = values[5];
        moments.m30 = values[6];
        moments.m21 = values[7];
        moments.m12 = values[8];
        moments.m03 = values[9];
        moments.mu20 = values[10];
        moments.mu11 = values[11];
        moments.mu02 = values[12];
        moments.mu30 = values[13];
        moments.mu21 = values[14];
        moments.mu12 = values[15];
        moments.mu03 = values[16];
        moments.nu20 = values[17];
        moments.nu11 = values[18];
        moments.nu02 = values[19];
        moments.nu30 = values[20];
        moments.nu21 = values[21];
        moments.nu12 = values[22];
        moments.nu03 = values[23];
        return moments;
    }

    std::vector<std::vector<cv::Point>> get_contours_from_flat(
        const int* contours_xy,
        const int* contour_lengths,
        int contour_count)
    {
        std::vector<std::vector<cv::Point>> contours;
        contours.reserve(static_cast<size_t>(contour_count));

        int point_offset = 0;
        for (int contour_index = 0; contour_index < contour_count; ++contour_index)
        {
            const int point_count = contour_lengths[contour_index];
            std::vector<cv::Point> contour;
            contour.reserve(static_cast<size_t>(point_count));

            for (int point_index = 0; point_index < point_count; ++point_index)
            {
                const int xy_offset = (point_offset + point_index) * 2;
                contour.push_back(cv::Point(contours_xy[xy_offset], contours_xy[xy_offset + 1]));
            }

            point_offset += point_count;
            contours.push_back(contour);
        }

        return contours;
    }

    std::vector<cv::Vec4i> get_hierarchy_from_flat(
        const int* hierarchy,
        int contour_count)
    {
        std::vector<cv::Vec4i> result;
        result.reserve(static_cast<size_t>(contour_count));

        for (int contour_index = 0; contour_index < contour_count; ++contour_index)
        {
            const int offset = contour_index * 4;
            result.push_back(cv::Vec4i(
                hierarchy[offset],
                hierarchy[offset + 1],
                hierarchy[offset + 2],
                hierarchy[offset + 3]));
        }

        return result;
    }

    int find_contours_core(
        const char* api_name,
        const jyppx_ocv_mat* image,
        int mode,
        int method,
        int offset_x,
        int offset_y,
        std::vector<std::vector<cv::Point>>& contours,
        std::vector<cv::Vec4i>& hierarchy)
    {
        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        cv::findContours(
            opencv_csharp_native::mat_value(image),
            contours,
            hierarchy,
            mode,
            method,
            cv::Point(offset_x, offset_y));
        return OPENCV_CSHARP_STATUS_OK;
    }

    int find_contours_link_runs_core(
        const char* api_name,
        const jyppx_ocv_mat* image,
        int include_hierarchy,
        std::vector<std::vector<cv::Point>>& contours,
        std::vector<cv::Vec4i>& hierarchy)
    {
        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (include_hierarchy != 0)
        {
            cv::findContoursLinkRuns(opencv_csharp_native::mat_value(image), contours, hierarchy);
        }
        else
        {
            cv::findContoursLinkRuns(opencv_csharp_native::mat_value(image), contours);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

#endif
}

int jyppx_ocv_imgproc_cvt_color(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int code, int dst_cn)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_cvt_color";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::cvtColor(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), code, dst_cn);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)code;
        (void)dst_cn;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_resize(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    double fx,
    double fy,
    int interpolation)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_resize";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::resize(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), cv::Size(width, height), fx, fy, interpolation);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)fx;
        (void)fy;
        (void)interpolation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_threshold(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double thresh,
    double maxval,
    int type,
    double* out_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (out_threshold == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_threshold");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_threshold = cv::threshold(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), thresh, maxval, type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)thresh;
        (void)maxval;
        (void)type;
        *out_threshold = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_adaptive_threshold(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double max_value,
    int adaptive_method,
    int threshold_type,
    int block_size,
    double c)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_adaptive_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (block_size <= 1 || (block_size % 2) == 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "block_size");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::adaptiveThreshold(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            max_value,
            adaptive_method,
            threshold_type,
            block_size,
            c);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)max_value;
        (void)adaptive_method;
        (void)threshold_type;
        (void)c;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_integral(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* sum,
    int sdepth)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_integral";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (sum == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "sum");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::integral(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(sum), sdepth);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sdepth;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_integral2(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* sum,
    jyppx_ocv_mat* sqsum,
    int sdepth,
    int sqdepth)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_integral2";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (sum == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "sum");
        }

        if (sqsum == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "sqsum");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::integral(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(sum),
            opencv_csharp_native::mat_value(sqsum),
            sdepth,
            sqdepth);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sdepth;
        (void)sqdepth;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_integral3(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* sum,
    jyppx_ocv_mat* sqsum,
    jyppx_ocv_mat* tilted,
    int sdepth,
    int sqdepth)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_integral3";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (sum == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "sum");
        }

        if (sqsum == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "sqsum");
        }

        if (tilted == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tilted");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::integral(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(sum),
            opencv_csharp_native::mat_value(sqsum),
            opencv_csharp_native::mat_value(tilted),
            sdepth,
            sqdepth);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sdepth;
        (void)sqdepth;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_distance_transform(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int distance_type,
    int mask_size,
    int dst_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_distance_transform";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::distanceTransform(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            distance_type,
            mask_size,
            dst_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)distance_type;
        (void)mask_size;
        (void)dst_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_distance_transform_with_labels(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    jyppx_ocv_mat* labels,
    int distance_type,
    int mask_size,
    int label_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_distance_transform_with_labels";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (labels == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "labels");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::distanceTransform(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(labels),
            distance_type,
            mask_size,
            label_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)distance_type;
        (void)mask_size;
        (void)label_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_flood_fill(
    jyppx_ocv_mat* image,
    int seed_x,
    int seed_y,
    double new_value_v0,
    double new_value_v1,
    double new_value_v2,
    double new_value_v3,
    int* rect_x,
    int* rect_y,
    int* rect_width,
    int* rect_height,
    double lo_diff_v0,
    double lo_diff_v1,
    double lo_diff_v2,
    double lo_diff_v3,
    double up_diff_v0,
    double up_diff_v1,
    double up_diff_v2,
    double up_diff_v3,
    int flags,
    int* filled_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_flood_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (rect_x == nullptr || rect_y == nullptr || rect_width == nullptr || rect_height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rect");
        }

        if (filled_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filled_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Rect rect;
        *filled_count = cv::floodFill(
            opencv_csharp_native::mat_value(image),
            cv::Point(seed_x, seed_y),
            get_scalar(new_value_v0, new_value_v1, new_value_v2, new_value_v3),
            &rect,
            get_scalar(lo_diff_v0, lo_diff_v1, lo_diff_v2, lo_diff_v3),
            get_scalar(up_diff_v0, up_diff_v1, up_diff_v2, up_diff_v3),
            flags);
        *rect_x = rect.x;
        *rect_y = rect.y;
        *rect_width = rect.width;
        *rect_height = rect.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)seed_x;
        (void)seed_y;
        (void)new_value_v0;
        (void)new_value_v1;
        (void)new_value_v2;
        (void)new_value_v3;
        (void)lo_diff_v0;
        (void)lo_diff_v1;
        (void)lo_diff_v2;
        (void)lo_diff_v3;
        (void)up_diff_v0;
        (void)up_diff_v1;
        (void)up_diff_v2;
        (void)up_diff_v3;
        (void)flags;
        *filled_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_flood_fill_mask(
    jyppx_ocv_mat* image,
    jyppx_ocv_mat* mask,
    int seed_x,
    int seed_y,
    double new_value_v0,
    double new_value_v1,
    double new_value_v2,
    double new_value_v3,
    int* rect_x,
    int* rect_y,
    int* rect_width,
    int* rect_height,
    double lo_diff_v0,
    double lo_diff_v1,
    double lo_diff_v2,
    double lo_diff_v3,
    double up_diff_v0,
    double up_diff_v1,
    double up_diff_v2,
    double up_diff_v3,
    int flags,
    int* filled_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_flood_fill_mask";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (mask == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mask");
        }

        if (rect_x == nullptr || rect_y == nullptr || rect_width == nullptr || rect_height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rect");
        }

        if (filled_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filled_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Rect rect;
        *filled_count = cv::floodFill(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(mask),
            cv::Point(seed_x, seed_y),
            get_scalar(new_value_v0, new_value_v1, new_value_v2, new_value_v3),
            &rect,
            get_scalar(lo_diff_v0, lo_diff_v1, lo_diff_v2, lo_diff_v3),
            get_scalar(up_diff_v0, up_diff_v1, up_diff_v2, up_diff_v3),
            flags);
        *rect_x = rect.x;
        *rect_y = rect.y;
        *rect_width = rect.width;
        *rect_height = rect.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)seed_x;
        (void)seed_y;
        (void)new_value_v0;
        (void)new_value_v1;
        (void)new_value_v2;
        (void)new_value_v3;
        (void)lo_diff_v0;
        (void)lo_diff_v1;
        (void)lo_diff_v2;
        (void)lo_diff_v3;
        (void)up_diff_v0;
        (void)up_diff_v1;
        (void)up_diff_v2;
        (void)up_diff_v3;
        (void)flags;
        *filled_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_connected_components(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* labels,
    int connectivity,
    int ltype,
    int* label_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_connected_components";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (labels == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "labels");
        }

        if (label_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "label_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *label_count = cv::connectedComponents(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(labels),
            connectivity,
            ltype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)connectivity;
        (void)ltype;
        *label_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_connected_components_with_algorithm(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* labels,
    int connectivity,
    int ltype,
    int ccltype,
    int* label_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_connected_components_with_algorithm";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (labels == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "labels");
        }

        if (label_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "label_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *label_count = cv::connectedComponents(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(labels),
            connectivity,
            ltype,
            ccltype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)connectivity;
        (void)ltype;
        (void)ccltype;
        *label_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_connected_components_with_stats(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* labels,
    jyppx_ocv_mat* stats,
    jyppx_ocv_mat* centroids,
    int connectivity,
    int ltype,
    int* label_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_connected_components_with_stats";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (labels == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "labels");
        }

        if (stats == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "stats");
        }

        if (centroids == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "centroids");
        }

        if (label_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "label_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *label_count = cv::connectedComponentsWithStats(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(labels),
            opencv_csharp_native::mat_value(stats),
            opencv_csharp_native::mat_value(centroids),
            connectivity,
            ltype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)connectivity;
        (void)ltype;
        *label_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_connected_components_with_stats_with_algorithm(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* labels,
    jyppx_ocv_mat* stats,
    jyppx_ocv_mat* centroids,
    int connectivity,
    int ltype,
    int ccltype,
    int* label_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_connected_components_with_stats_with_algorithm";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (labels == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "labels");
        }

        if (stats == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "stats");
        }

        if (centroids == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "centroids");
        }

        if (label_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "label_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *label_count = cv::connectedComponentsWithStats(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(labels),
            opencv_csharp_native::mat_value(stats),
            opencv_csharp_native::mat_value(centroids),
            connectivity,
            ltype,
            ccltype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)connectivity;
        (void)ltype;
        (void)ccltype;
        *label_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_equalize_hist(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_equalize_hist";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::equalizeHist(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_imgproc_clahe_create(
    double clip_limit,
    int tiles_grid_width,
    int tiles_grid_height,
    jyppx_ocv_clahe** clahe)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_clahe_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (clahe == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "clahe");
        }

        if (tiles_grid_width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tiles_grid_width");
        }

        if (tiles_grid_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tiles_grid_height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto result = new (std::nothrow) jyppx_ocv_clahe{ cv::createCLAHE(clip_limit, cv::Size(tiles_grid_width, tiles_grid_height)) };
        if (result == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *clahe = result;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clip_limit;
        *clahe = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_imgproc_clahe_release(jyppx_ocv_clahe* clahe)
{
    delete clahe;
}

int jyppx_ocv_imgproc_clahe_apply(
    jyppx_ocv_clahe* clahe,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_clahe_apply";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_clahe(api_name, clahe);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        clahe->value->apply(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clahe;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_clahe_get_clip_limit(
    const jyppx_ocv_clahe* clahe,
    double* clip_limit)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_clahe_get_clip_limit";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (clip_limit == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "clip_limit");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_clahe(api_name, clahe);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *clip_limit = clahe->value->getClipLimit();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clahe;
        *clip_limit = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_clahe_set_clip_limit(
    jyppx_ocv_clahe* clahe,
    double clip_limit)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_clahe_set_clip_limit";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_clahe(api_name, clahe);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        clahe->value->setClipLimit(clip_limit);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clahe;
        (void)clip_limit;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_clahe_get_tiles_grid_size(
    const jyppx_ocv_clahe* clahe,
    int* width,
    int* height)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_clahe_get_tiles_grid_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (width == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_clahe(api_name, clahe);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        const cv::Size size = clahe->value->getTilesGridSize();
        *width = size.width;
        *height = size.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clahe;
        *width = 0;
        *height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_clahe_set_tiles_grid_size(
    jyppx_ocv_clahe* clahe,
    int width,
    int height)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_clahe_set_tiles_grid_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_clahe(api_name, clahe);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        clahe->value->setTilesGridSize(cv::Size(width, height));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clahe;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_clahe_get_bit_shift(
    const jyppx_ocv_clahe* clahe,
    int* bit_shift)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_clahe_get_bit_shift";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (bit_shift == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bit_shift");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_clahe(api_name, clahe);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *bit_shift = clahe->value->getBitShift();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clahe;
        *bit_shift = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_clahe_set_bit_shift(
    jyppx_ocv_clahe* clahe,
    int bit_shift)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_clahe_set_bit_shift";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_clahe(api_name, clahe);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        clahe->value->setBitShift(bit_shift);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clahe;
        (void)bit_shift;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_clahe_collect_garbage(jyppx_ocv_clahe* clahe)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_clahe_collect_garbage";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_clahe(api_name, clahe);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        clahe->value->collectGarbage();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clahe;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_corner_harris(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int block_size,
    int ksize,
    double k,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_corner_harris";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::cornerHarris(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), block_size, ksize, k, border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)block_size;
        (void)ksize;
        (void)k;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_corner_min_eigen_val(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int block_size,
    int ksize,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_corner_min_eigen_val";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::cornerMinEigenVal(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), block_size, ksize, border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)block_size;
        (void)ksize;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_corner_eigen_vals_and_vecs(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int block_size,
    int ksize,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_corner_eigen_vals_and_vecs";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::cornerEigenValsAndVecs(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), block_size, ksize, border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)block_size;
        (void)ksize;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_pre_corner_detect(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ksize,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_pre_corner_detect";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::preCornerDetect(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), ksize, border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ksize;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_corner_sub_pix(
    const jyppx_ocv_mat* image,
    float* corners_xy,
    int corner_count,
    int win_width,
    int win_height,
    int zero_zone_width,
    int zero_zone_height,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_corner_sub_pix";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (corners_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "corners_xy");
        }

        if (corner_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "corner_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Point2f> corners = get_point2f_points_from_xy(corners_xy, corner_count);
        cv::cornerSubPix(
            opencv_csharp_native::mat_value(image),
            corners,
            cv::Size(win_width, win_height),
            cv::Size(zero_zone_width, zero_zone_height),
            cv::TermCriteria(criteria_type, criteria_max_count, criteria_epsilon));

        for (int i = 0; i < corner_count; ++i)
        {
            const int offset = i * 2;
            corners_xy[offset] = corners[static_cast<size_t>(i)].x;
            corners_xy[offset + 1] = corners[static_cast<size_t>(i)].y;
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)win_width;
        (void)win_height;
        (void)zero_zone_width;
        (void)zero_zone_height;
        (void)criteria_type;
        (void)criteria_max_count;
        (void)criteria_epsilon;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_good_features_to_track_count(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    int max_corners,
    double quality_level,
    double min_distance,
    int block_size,
    int gradient_size,
    int use_harris_detector,
    double k,
    int* corner_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_good_features_to_track_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (corner_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "corner_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES)
        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        std::vector<cv::Point2f> corners;
        cv::goodFeaturesToTrack(
            opencv_csharp_native::mat_value(image),
            corners,
            max_corners,
            quality_level,
            min_distance,
            mask == nullptr ? cv::Mat() : opencv_csharp_native::mat_value(mask),
            block_size,
            gradient_size,
            use_harris_detector != 0,
            k);
        *corner_count = static_cast<int>(corners.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)mask;
        (void)max_corners;
        (void)quality_level;
        (void)min_distance;
        (void)block_size;
        (void)gradient_size;
        (void)use_harris_detector;
        (void)k;
        *corner_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_good_features_to_track_fill(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    int max_corners,
    double quality_level,
    double min_distance,
    int block_size,
    int gradient_size,
    int use_harris_detector,
    double k,
    float* corners_xy,
    int corner_capacity,
    int* corner_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_good_features_to_track_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (corner_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "corner_count");
        }

        if (corners_xy == nullptr && corner_capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "corners_xy");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV_FEATURES)
        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        std::vector<cv::Point2f> corners;
        cv::goodFeaturesToTrack(
            opencv_csharp_native::mat_value(image),
            corners,
            max_corners,
            quality_level,
            min_distance,
            mask == nullptr ? cv::Mat() : opencv_csharp_native::mat_value(mask),
            block_size,
            gradient_size,
            use_harris_detector != 0,
            k);

        const int actual_count = static_cast<int>(corners.size());
        if (corner_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "corner_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const int offset = i * 2;
            corners_xy[offset] = corners[static_cast<size_t>(i)].x;
            corners_xy[offset + 1] = corners[static_cast<size_t>(i)].y;
        }

        *corner_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)mask;
        (void)max_corners;
        (void)quality_level;
        (void)min_distance;
        (void)block_size;
        (void)gradient_size;
        (void)use_harris_detector;
        (void)k;
        *corner_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_hough_lines_count(
    const jyppx_ocv_mat* image,
    double rho,
    double theta,
    int threshold,
    double srn,
    double stn,
    double min_theta,
    double max_theta,
    int use_edgeval,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_hough_lines_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (line_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat lines;
        int status = hough_lines_core(api_name, image, rho, theta, threshold, srn, stn, min_theta, max_theta, use_edgeval, lines);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *line_count = lines.rows * lines.cols;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)rho;
        (void)theta;
        (void)threshold;
        (void)srn;
        (void)stn;
        (void)min_theta;
        (void)max_theta;
        (void)use_edgeval;
        *line_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_hough_lines_fill(
    const jyppx_ocv_mat* image,
    double rho,
    double theta,
    int threshold,
    double srn,
    double stn,
    double min_theta,
    double max_theta,
    int use_edgeval,
    float* lines,
    int line_capacity,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_hough_lines_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (line_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_count");
        }

        if (lines == nullptr && line_capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat output;
        int status = hough_lines_core(api_name, image, rho, theta, threshold, srn, stn, min_theta, max_theta, use_edgeval, output);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        const int actual_count = output.rows * output.cols;
        if (line_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const int channel_count = output.channels();
            const float* value = output.ptr<float>() + (static_cast<size_t>(i) * static_cast<size_t>(channel_count));
            const int offset = i * 2;
            lines[offset] = value[0];
            lines[offset + 1] = channel_count > 1 ? value[1] : 0.0F;
        }

        *line_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)rho;
        (void)theta;
        (void)threshold;
        (void)srn;
        (void)stn;
        (void)min_theta;
        (void)max_theta;
        (void)use_edgeval;
        *line_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_hough_lines_p_count(
    const jyppx_ocv_mat* image,
    double rho,
    double theta,
    int threshold,
    double min_line_length,
    double max_line_gap,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_hough_lines_p_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (line_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat lines;
        int status = hough_lines_p_core(api_name, image, rho, theta, threshold, min_line_length, max_line_gap, lines);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *line_count = lines.rows * lines.cols;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)rho;
        (void)theta;
        (void)threshold;
        (void)min_line_length;
        (void)max_line_gap;
        *line_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_hough_lines_p_fill(
    const jyppx_ocv_mat* image,
    double rho,
    double theta,
    int threshold,
    double min_line_length,
    double max_line_gap,
    int* lines,
    int line_capacity,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_hough_lines_p_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (line_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_count");
        }

        if (lines == nullptr && line_capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat output;
        int status = hough_lines_p_core(api_name, image, rho, theta, threshold, min_line_length, max_line_gap, output);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        const int actual_count = output.rows * output.cols;
        if (line_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const cv::Vec4i value = output.at<cv::Vec4i>(i);
            const int offset = i * 4;
            lines[offset] = value[0];
            lines[offset + 1] = value[1];
            lines[offset + 2] = value[2];
            lines[offset + 3] = value[3];
        }

        *line_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)rho;
        (void)theta;
        (void)threshold;
        (void)min_line_length;
        (void)max_line_gap;
        *line_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_hough_lines_point_set_count(
    const int* points_xy,
    int point_count,
    int lines_max,
    int threshold,
    double min_rho,
    double max_rho,
    double rho_step,
    double min_theta,
    double max_theta,
    double theta_step,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_hough_lines_point_set_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (line_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat lines;
        int status = hough_lines_point_set_core(api_name, points_xy, point_count, lines_max, threshold, min_rho, max_rho, rho_step, min_theta, max_theta, theta_step, lines);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *line_count = lines.rows * lines.cols;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)points_xy;
        (void)point_count;
        (void)lines_max;
        (void)threshold;
        (void)min_rho;
        (void)max_rho;
        (void)rho_step;
        (void)min_theta;
        (void)max_theta;
        (void)theta_step;
        *line_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_hough_lines_point_set_fill(
    const int* points_xy,
    int point_count,
    int lines_max,
    int threshold,
    double min_rho,
    double max_rho,
    double rho_step,
    double min_theta,
    double max_theta,
    double theta_step,
    double* lines,
    int line_capacity,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_hough_lines_point_set_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (line_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_count");
        }

        if (lines == nullptr && line_capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat output;
        int status = hough_lines_point_set_core(api_name, points_xy, point_count, lines_max, threshold, min_rho, max_rho, rho_step, min_theta, max_theta, theta_step, output);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        const int actual_count = output.rows * output.cols;
        if (line_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const cv::Vec3d value = output.at<cv::Vec3d>(i);
            const int offset = i * 3;
            lines[offset] = value[0];
            lines[offset + 1] = value[1];
            lines[offset + 2] = value[2];
        }

        *line_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)points_xy;
        (void)point_count;
        (void)lines_max;
        (void)threshold;
        (void)min_rho;
        (void)max_rho;
        (void)rho_step;
        (void)min_theta;
        (void)max_theta;
        (void)theta_step;
        *line_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_hough_circles_count(
    const jyppx_ocv_mat* image,
    int method,
    double dp,
    double min_dist,
    double param1,
    double param2,
    int min_radius,
    int max_radius,
    int* circle_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_hough_circles_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (circle_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "circle_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat circles;
        int status = hough_circles_core(api_name, image, method, dp, min_dist, param1, param2, min_radius, max_radius, circles);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *circle_count = circles.rows * circles.cols;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)method;
        (void)dp;
        (void)min_dist;
        (void)param1;
        (void)param2;
        (void)min_radius;
        (void)max_radius;
        *circle_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_hough_circles_fill(
    const jyppx_ocv_mat* image,
    int method,
    double dp,
    double min_dist,
    double param1,
    double param2,
    int min_radius,
    int max_radius,
    float* circles,
    int circle_capacity,
    int* circle_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_hough_circles_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (circle_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "circle_count");
        }

        if (circles == nullptr && circle_capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "circles");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat output;
        int status = hough_circles_core(api_name, image, method, dp, min_dist, param1, param2, min_radius, max_radius, output);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        const int actual_count = output.rows * output.cols;
        if (circle_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "circle_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const int channel_count = output.channels();
            const float* value = output.ptr<float>() + (static_cast<size_t>(i) * static_cast<size_t>(channel_count));
            const int offset = i * 3;
            circles[offset] = value[0];
            circles[offset + 1] = channel_count > 1 ? value[1] : 0.0F;
            circles[offset + 2] = channel_count > 2 ? value[2] : 0.0F;
        }

        *circle_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)method;
        (void)dp;
        (void)min_dist;
        (void)param1;
        (void)param2;
        (void)min_radius;
        (void)max_radius;
        *circle_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_calc_hist_uniform(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const int* channels,
    int channel_count,
    jyppx_ocv_mat* hist,
    const int* hist_size,
    int hist_dims,
    const float* ranges,
    int range_count,
    int accumulate)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_calc_hist_uniform";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return calc_hist_uniform_core(
            api_name,
            image,
            mask,
            channels,
            channel_count,
            hist,
            hist_size,
            hist_dims,
            ranges,
            range_count,
            accumulate);
#else
        (void)image;
        (void)mask;
        (void)channels;
        (void)channel_count;
        (void)hist;
        (void)hist_size;
        (void)hist_dims;
        (void)ranges;
        (void)range_count;
        (void)accumulate;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_calc_back_project_uniform(
    const jyppx_ocv_mat* image,
    const int* channels,
    int channel_count,
    const jyppx_ocv_mat* hist,
    jyppx_ocv_mat* back_project,
    const float* ranges,
    int range_count,
    double scale)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_calc_back_project_uniform";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (channels == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "channels");
        }

        if (hist == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hist");
        }

        if (back_project == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "back_project");
        }

        if (ranges == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "ranges");
        }

        if (channel_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "channel_count");
        }

        if (range_count < channel_count * 2)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "range_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<const float*> range_ptrs(static_cast<size_t>(channel_count));
        for (int i = 0; i < channel_count; ++i)
        {
            range_ptrs[static_cast<size_t>(i)] = ranges + (i * 2);
        }

        const cv::Mat* images = &opencv_csharp_native::mat_value(image);
        cv::calcBackProject(
            images,
            1,
            channels,
            opencv_csharp_native::mat_value(hist),
            opencv_csharp_native::mat_value(back_project),
            range_ptrs.data(),
            scale,
            true);
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

int jyppx_ocv_imgproc_compare_hist(
    const jyppx_ocv_mat* h1,
    const jyppx_ocv_mat* h2,
    int method,
    double* result)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_compare_hist";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (h1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "h1");
        }

        if (h2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "h2");
        }

        if (result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "result");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *result = cv::compareHist(opencv_csharp_native::mat_value(h1), opencv_csharp_native::mat_value(h2), method);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)method;
        *result = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_line_segment_detector_create(
    int refine,
    double scale,
    double sigma_scale,
    double quant,
    double ang_th,
    double log_eps,
    double density_th,
    int n_bins,
    jyppx_ocv_line_segment_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_line_segment_detector_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto result = new (std::nothrow) jyppx_ocv_line_segment_detector{
            cv::createLineSegmentDetector(
                static_cast<cv::LineSegmentDetectorModes>(refine),
                scale,
                sigma_scale,
                quant,
                ang_th,
                log_eps,
                density_th,
                n_bins)
        };
        if (result == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *detector = result;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)refine;
        (void)scale;
        (void)sigma_scale;
        (void)quant;
        (void)ang_th;
        (void)log_eps;
        (void)density_th;
        (void)n_bins;
        *detector = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_imgproc_line_segment_detector_release(jyppx_ocv_line_segment_detector* detector)
{
    delete detector;
}

int jyppx_ocv_imgproc_line_segment_detector_detect(
    jyppx_ocv_line_segment_detector* detector,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* lines,
    jyppx_ocv_mat* width,
    jyppx_ocv_mat* prec,
    jyppx_ocv_mat* nfa)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_line_segment_detector_detect";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (lines == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_line_segment_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::OutputArray width_output = width == nullptr
            ? cv::noArray()
            : cv::OutputArray(opencv_csharp_native::mat_value(width));
        cv::OutputArray prec_output = prec == nullptr
            ? cv::noArray()
            : cv::OutputArray(opencv_csharp_native::mat_value(prec));
        cv::OutputArray nfa_output = nfa == nullptr
            ? cv::noArray()
            : cv::OutputArray(opencv_csharp_native::mat_value(nfa));

        detector->value->detect(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(lines),
            width_output,
            prec_output,
            nfa_output);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        (void)width;
        (void)prec;
        (void)nfa;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_line_segment_detector_detect_count(
    jyppx_ocv_line_segment_detector* detector,
    const jyppx_ocv_mat* image,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_line_segment_detector_detect_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (line_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat lines;
        int status = line_segment_detect_core(api_name, detector, image, lines);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *line_count = lines.rows * lines.cols;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        (void)image;
        *line_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_line_segment_detector_detect_fill(
    jyppx_ocv_line_segment_detector* detector,
    const jyppx_ocv_mat* image,
    float* lines,
    int line_capacity,
    int* line_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_line_segment_detector_detect_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (line_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_count");
        }

        if (lines == nullptr && line_capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat output;
        int status = line_segment_detect_core(api_name, detector, image, output);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        const int actual_count = output.rows * output.cols;
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
        (void)detector;
        (void)image;
        *line_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_line_segment_detector_draw_segments(
    jyppx_ocv_line_segment_detector* detector,
    jyppx_ocv_mat* image,
    const jyppx_ocv_mat* lines)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_line_segment_detector_draw_segments";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (lines == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_line_segment_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        detector->value->drawSegments(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(lines));
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

int jyppx_ocv_imgproc_line_segment_detector_draw_segments_array(
    jyppx_ocv_line_segment_detector* detector,
    jyppx_ocv_mat* image,
    const float* lines,
    int line_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_line_segment_detector_draw_segments_array";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (lines == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines");
        }

        if (line_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_line_segment_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::Mat lines_mat = get_vec4f_mat_from_flat(lines, line_count);
        detector->value->drawSegments(opencv_csharp_native::mat_value(image), lines_mat);
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

int jyppx_ocv_imgproc_line_segment_detector_compare_segments(
    jyppx_ocv_line_segment_detector* detector,
    int width,
    int height,
    const jyppx_ocv_mat* lines1,
    const jyppx_ocv_mat* lines2,
    jyppx_ocv_mat* image,
    int* mismatch_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_line_segment_detector_compare_segments";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (lines1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines1");
        }

        if (lines2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines2");
        }

        if (mismatch_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mismatch_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_line_segment_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::InputOutputArray image_output = image == nullptr
            ? cv::noArray()
            : cv::InputOutputArray(opencv_csharp_native::mat_value(image));

        *mismatch_count = detector->value->compareSegments(
            cv::Size(width, height),
            opencv_csharp_native::mat_value(lines1),
            opencv_csharp_native::mat_value(lines2),
            image_output);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        (void)width;
        (void)height;
        (void)image;
        *mismatch_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_line_segment_detector_compare_segments_array(
    jyppx_ocv_line_segment_detector* detector,
    int width,
    int height,
    const float* lines1,
    int line1_count,
    const float* lines2,
    int line2_count,
    jyppx_ocv_mat* image,
    int* mismatch_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_line_segment_detector_compare_segments_array";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (lines1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines1");
        }

        if (lines2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines2");
        }

        if (line1_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line1_count");
        }

        if (line2_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "line2_count");
        }

        if (mismatch_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mismatch_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_line_segment_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::Mat lines1_mat = get_vec4f_mat_from_flat(lines1, line1_count);
        cv::Mat lines2_mat = get_vec4f_mat_from_flat(lines2, line2_count);
        cv::InputOutputArray image_output = image == nullptr
            ? cv::noArray()
            : cv::InputOutputArray(opencv_csharp_native::mat_value(image));

        *mismatch_count = detector->value->compareSegments(
            cv::Size(width, height),
            lines1_mat,
            lines2_mat,
            image_output);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector;
        (void)width;
        (void)height;
        (void)image;
        *mismatch_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_gaussian_blur(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    double sigma_x,
    double sigma_y,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_gaussian_blur";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::GaussianBlur(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            cv::Size(width, height),
            sigma_x,
            sigma_y,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma_x;
        (void)sigma_y;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_blur(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    int anchor_x,
    int anchor_y,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_blur";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::blur(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            cv::Size(width, height),
            cv::Point(anchor_x, anchor_y),
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)anchor_x;
        (void)anchor_y;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_box_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    int width,
    int height,
    int anchor_x,
    int anchor_y,
    int normalize,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_box_filter";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::boxFilter(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            ddepth,
            cv::Size(width, height),
            cv::Point(anchor_x, anchor_y),
            normalize != 0,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ddepth;
        (void)anchor_x;
        (void)anchor_y;
        (void)normalize;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_sqr_box_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    int width,
    int height,
    int anchor_x,
    int anchor_y,
    int normalize,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_sqr_box_filter";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::sqrBoxFilter(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            ddepth,
            cv::Size(width, height),
            cv::Point(anchor_x, anchor_y),
            normalize != 0,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ddepth;
        (void)anchor_x;
        (void)anchor_y;
        (void)normalize;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_median_blur(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ksize)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_median_blur";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (ksize <= 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "ksize");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::medianBlur(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), ksize);
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

int jyppx_ocv_imgproc_bilateral_filter(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int d,
    double sigma_color,
    double sigma_space,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_bilateral_filter";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::bilateralFilter(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            d,
            sigma_color,
            sigma_space,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)d;
        (void)sigma_color;
        (void)sigma_space;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_filter2d(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    const jyppx_ocv_mat* kernel,
    int anchor_x,
    int anchor_y,
    double delta,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_filter2d";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (kernel == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "kernel");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::filter2D(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            ddepth,
            opencv_csharp_native::mat_value(kernel),
            cv::Point(anchor_x, anchor_y),
            delta,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ddepth;
        (void)anchor_x;
        (void)anchor_y;
        (void)delta;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_sep_filter2d(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    const jyppx_ocv_mat* kernel_x,
    const jyppx_ocv_mat* kernel_y,
    int anchor_x,
    int anchor_y,
    double delta,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_sep_filter2d";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (kernel_x == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "kernel_x");
        }

        if (kernel_y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "kernel_y");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::sepFilter2D(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            ddepth,
            opencv_csharp_native::mat_value(kernel_x),
            opencv_csharp_native::mat_value(kernel_y),
            cv::Point(anchor_x, anchor_y),
            delta,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ddepth;
        (void)anchor_x;
        (void)anchor_y;
        (void)delta;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_sobel(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    int dx,
    int dy,
    int ksize,
    double scale,
    double delta,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_sobel";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Sobel(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            ddepth,
            dx,
            dy,
            ksize,
            scale,
            delta,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ddepth;
        (void)dx;
        (void)dy;
        (void)ksize;
        (void)scale;
        (void)delta;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_scharr(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    int dx,
    int dy,
    double scale,
    double delta,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_scharr";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Scharr(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            ddepth,
            dx,
            dy,
            scale,
            delta,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ddepth;
        (void)dx;
        (void)dy;
        (void)scale;
        (void)delta;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_laplacian(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int ddepth,
    int ksize,
    double scale,
    double delta,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_laplacian";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Laplacian(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            ddepth,
            ksize,
            scale,
            delta,
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ddepth;
        (void)ksize;
        (void)scale;
        (void)delta;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_canny(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* edges,
    double threshold1,
    double threshold2,
    int aperture_size,
    int l2_gradient)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_canny";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (edges == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "edges");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Canny(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(edges),
            threshold1,
            threshold2,
            aperture_size,
            l2_gradient != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)threshold1;
        (void)threshold2;
        (void)aperture_size;
        (void)l2_gradient;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_canny_derivatives(
    const jyppx_ocv_mat* dx,
    const jyppx_ocv_mat* dy,
    jyppx_ocv_mat* edges,
    double threshold1,
    double threshold2,
    int l2_gradient)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_canny_derivatives";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (dx == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dx");
        }

        if (dy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dy");
        }

        if (edges == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "edges");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Canny(
            opencv_csharp_native::mat_value(dx),
            opencv_csharp_native::mat_value(dy),
            opencv_csharp_native::mat_value(edges),
            threshold1,
            threshold2,
            l2_gradient != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)threshold1;
        (void)threshold2;
        (void)l2_gradient;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_gaussian_kernel(
    int ksize,
    double sigma,
    int ktype,
    jyppx_ocv_mat** out_kernel)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_gaussian_kernel";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_kernel == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_kernel");
        }

        *out_kernel = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat kernel = cv::getGaussianKernel(ksize, sigma, ktype);
        auto handle = new (std::nothrow) jyppx_ocv_mat{ kernel };
        if (handle == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *out_kernel = handle;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ksize;
        (void)sigma;
        (void)ktype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_deriv_kernels(
    jyppx_ocv_mat* kx,
    jyppx_ocv_mat* ky,
    int dx,
    int dy,
    int ksize,
    int normalize,
    int ktype)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_deriv_kernels";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (kx == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "kx");
        }

        if (ky == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "ky");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::getDerivKernels(
            opencv_csharp_native::mat_value(kx),
            opencv_csharp_native::mat_value(ky),
            dx,
            dy,
            ksize,
            normalize != 0,
            ktype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dx;
        (void)dy;
        (void)ksize;
        (void)normalize;
        (void)ktype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_gabor_kernel(
    int width,
    int height,
    double sigma,
    double theta,
    double lambd,
    double gamma,
    double psi,
    int ktype,
    jyppx_ocv_mat** out_kernel)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_gabor_kernel";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

        if (out_kernel == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_kernel");
        }

        *out_kernel = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat kernel = cv::getGaborKernel(cv::Size(width, height), sigma, theta, lambd, gamma, psi, ktype);
        auto handle = new (std::nothrow) jyppx_ocv_mat{ kernel };
        if (handle == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *out_kernel = handle;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)sigma;
        (void)theta;
        (void)lambd;
        (void)gamma;
        (void)psi;
        (void)ktype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_pyr_down(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_pyr_down";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::pyrDown(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            cv::Size(width, height),
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_pyr_up(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_pyr_up";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::pyrUp(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            cv::Size(width, height),
            border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_warp_affine(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* transform,
    int width,
    int height,
    int flags,
    int border_mode,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_warp_affine";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (transform == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "transform");
        }

        if (width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::warpAffine(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(transform),
            cv::Size(width, height),
            flags,
            border_mode,
            get_scalar(border_value_v0, border_value_v1, border_value_v2, border_value_v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        (void)border_mode;
        (void)border_value_v0;
        (void)border_value_v1;
        (void)border_value_v2;
        (void)border_value_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_warp_perspective(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* transform,
    int width,
    int height,
    int flags,
    int border_mode,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_warp_perspective";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (transform == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "transform");
        }

        if (width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::warpPerspective(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(transform),
            cv::Size(width, height),
            flags,
            border_mode,
            get_scalar(border_value_v0, border_value_v1, border_value_v2, border_value_v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        (void)border_mode;
        (void)border_value_v0;
        (void)border_value_v1;
        (void)border_value_v2;
        (void)border_value_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_rotation_matrix2d(
    float center_x,
    float center_y,
    double angle,
    double scale,
    jyppx_ocv_mat** out_transform)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_rotation_matrix2d";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_transform == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_transform");
        }

        *out_transform = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat transform = cv::getRotationMatrix2D(get_point2f(center_x, center_y), angle, scale);
        auto handle = new (std::nothrow) jyppx_ocv_mat{ transform };
        if (handle == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *out_transform = handle;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)center_x;
        (void)center_y;
        (void)angle;
        (void)scale;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_affine_transform(
    const float* src_xy,
    const float* dst_xy,
    jyppx_ocv_mat** out_transform)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_affine_transform";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src_xy");
        }

        if (dst_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst_xy");
        }

        if (out_transform == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_transform");
        }

        *out_transform = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point2f> src_points = get_point2f_points_from_xy(src_xy, 3);
        const std::vector<cv::Point2f> dst_points = get_point2f_points_from_xy(dst_xy, 3);
        cv::Mat transform = cv::getAffineTransform(src_points.data(), dst_points.data());
        auto handle = new (std::nothrow) jyppx_ocv_mat{ transform };
        if (handle == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *out_transform = handle;
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

int jyppx_ocv_imgproc_get_perspective_transform(
    const float* src_xy,
    const float* dst_xy,
    int solve_method,
    jyppx_ocv_mat** out_transform)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_perspective_transform";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src_xy");
        }

        if (dst_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst_xy");
        }

        if (out_transform == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_transform");
        }

        *out_transform = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point2f> src_points = get_point2f_points_from_xy(src_xy, 4);
        const std::vector<cv::Point2f> dst_points = get_point2f_points_from_xy(dst_xy, 4);
        cv::Mat transform = cv::getPerspectiveTransform(src_points.data(), dst_points.data(), solve_method);
        auto handle = new (std::nothrow) jyppx_ocv_mat{ transform };
        if (handle == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *out_transform = handle;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)solve_method;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_invert_affine_transform(
    const jyppx_ocv_mat* transform,
    jyppx_ocv_mat* inverse_transform)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_invert_affine_transform";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (transform == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "transform");
        }

        if (inverse_transform == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "inverse_transform");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::invertAffineTransform(
            opencv_csharp_native::mat_value(transform),
            opencv_csharp_native::mat_value(inverse_transform));
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

int jyppx_ocv_imgproc_remap(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* map1,
    const jyppx_ocv_mat* map2,
    int interpolation,
    int border_mode,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_remap";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (map1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map1");
        }

        if (map2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map2");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::remap(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(map1),
            opencv_csharp_native::mat_value(map2),
            interpolation,
            border_mode,
            get_scalar(border_value_v0, border_value_v1, border_value_v2, border_value_v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)interpolation;
        (void)border_mode;
        (void)border_value_v0;
        (void)border_value_v1;
        (void)border_value_v2;
        (void)border_value_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_convert_maps(
    const jyppx_ocv_mat* map1,
    const jyppx_ocv_mat* map2,
    jyppx_ocv_mat* dstmap1,
    jyppx_ocv_mat* dstmap2,
    int dstmap1type,
    int nninterpolation)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_convert_maps";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (map1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map1");
        }

        if (map2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "map2");
        }

        if (dstmap1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dstmap1");
        }

        if (dstmap2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dstmap2");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::convertMaps(
            opencv_csharp_native::mat_value(map1),
            opencv_csharp_native::mat_value(map2),
            opencv_csharp_native::mat_value(dstmap1),
            opencv_csharp_native::mat_value(dstmap2),
            dstmap1type,
            nninterpolation != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dstmap1type;
        (void)nninterpolation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_structuring_element(
    int shape,
    int width,
    int height,
    int anchor_x,
    int anchor_y,
    jyppx_ocv_mat** out_element)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_structuring_element";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_element == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_element");
        }

        *out_element = nullptr;

        if (width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat native_element = cv::getStructuringElement(shape, cv::Size(width, height), cv::Point(anchor_x, anchor_y));
        auto element = new (std::nothrow) jyppx_ocv_mat{ native_element };
        if (element == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *out_element = element;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)shape;
        (void)anchor_x;
        (void)anchor_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_erode(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* kernel,
    int anchor_x,
    int anchor_y,
    int iterations,
    int border_type,
    int has_border_value,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_erode";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_morphology(
            api_name,
            cv::erode,
            src,
            dst,
            kernel,
            anchor_x,
            anchor_y,
            iterations,
            border_type,
            has_border_value,
            border_value_v0,
            border_value_v1,
            border_value_v2,
            border_value_v3);
#else
        (void)src;
        (void)dst;
        (void)kernel;
        (void)anchor_x;
        (void)anchor_y;
        (void)iterations;
        (void)border_type;
        (void)has_border_value;
        (void)border_value_v0;
        (void)border_value_v1;
        (void)border_value_v2;
        (void)border_value_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_dilate(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* kernel,
    int anchor_x,
    int anchor_y,
    int iterations,
    int border_type,
    int has_border_value,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_dilate";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_morphology(
            api_name,
            cv::dilate,
            src,
            dst,
            kernel,
            anchor_x,
            anchor_y,
            iterations,
            border_type,
            has_border_value,
            border_value_v0,
            border_value_v1,
            border_value_v2,
            border_value_v3);
#else
        (void)src;
        (void)dst;
        (void)kernel;
        (void)anchor_x;
        (void)anchor_y;
        (void)iterations;
        (void)border_type;
        (void)has_border_value;
        (void)border_value_v0;
        (void)border_value_v1;
        (void)border_value_v2;
        (void)border_value_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_morphology_ex(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int op,
    const jyppx_ocv_mat* kernel,
    int anchor_x,
    int anchor_y,
    int iterations,
    int border_type,
    int has_border_value,
    double border_value_v0,
    double border_value_v1,
    double border_value_v2,
    double border_value_v3)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_morphology_ex";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_morphology_ex(
            api_name,
            src,
            dst,
            op,
            kernel,
            anchor_x,
            anchor_y,
            iterations,
            border_type,
            has_border_value,
            border_value_v0,
            border_value_v1,
            border_value_v2,
            border_value_v3);
#else
        (void)src;
        (void)dst;
        (void)op;
        (void)kernel;
        (void)anchor_x;
        (void)anchor_y;
        (void)iterations;
        (void)border_type;
        (void)has_border_value;
        (void)border_value_v0;
        (void)border_value_v1;
        (void)border_value_v2;
        (void)border_value_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_line(
    jyppx_ocv_mat* img,
    int x1,
    int y1,
    int x2,
    int y2,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_line";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (img == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "img");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::line(
            opencv_csharp_native::mat_value(img),
            cv::Point(x1, y1),
            cv::Point(x2, y2),
            get_scalar(color_v0, color_v1, color_v2, color_v3),
            thickness,
            line_type,
            shift);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x1;
        (void)y1;
        (void)x2;
        (void)y2;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)thickness;
        (void)line_type;
        (void)shift;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_arrowed_line(
    jyppx_ocv_mat* img,
    int x1,
    int y1,
    int x2,
    int y2,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift,
    double tip_length)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_arrowed_line";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (img == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "img");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::arrowedLine(
            opencv_csharp_native::mat_value(img),
            cv::Point(x1, y1),
            cv::Point(x2, y2),
            get_scalar(color_v0, color_v1, color_v2, color_v3),
            thickness,
            line_type,
            shift,
            tip_length);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x1;
        (void)y1;
        (void)x2;
        (void)y2;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)thickness;
        (void)line_type;
        (void)shift;
        (void)tip_length;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_clip_line_rect(
    int rect_x,
    int rect_y,
    int rect_width,
    int rect_height,
    int* pt1_x,
    int* pt1_y,
    int* pt2_x,
    int* pt2_y,
    int* intersects)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_clip_line_rect";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (pt1_x == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pt1_x");
        }

        if (pt1_y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pt1_y");
        }

        if (pt2_x == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pt2_x");
        }

        if (pt2_y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pt2_y");
        }

        if (intersects == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "intersects");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Point pt1(*pt1_x, *pt1_y);
        cv::Point pt2(*pt2_x, *pt2_y);
        const bool result = cv::clipLine(cv::Rect(rect_x, rect_y, rect_width, rect_height), pt1, pt2);

        *pt1_x = pt1.x;
        *pt1_y = pt1.y;
        *pt2_x = pt2.x;
        *pt2_y = pt2.y;
        *intersects = result ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rect_x;
        (void)rect_y;
        (void)rect_width;
        (void)rect_height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_polylines(
    jyppx_ocv_mat* img,
    const int* points_xy,
    int point_count,
    int is_closed,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_polylines";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (img == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "img");
        }

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Point> points;
        points.reserve(static_cast<size_t>(point_count));
        for (int i = 0; i < point_count; ++i)
        {
            const int offset = i * 2;
            points.push_back(cv::Point(points_xy[offset], points_xy[offset + 1]));
        }

        const cv::Point* contour = points.data();
        const int contour_count = point_count;
        cv::polylines(
            opencv_csharp_native::mat_value(img),
            &contour,
            &contour_count,
            1,
            is_closed != 0,
            get_scalar(color_v0, color_v1, color_v2, color_v3),
            thickness,
            line_type,
            shift);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)is_closed;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)thickness;
        (void)line_type;
        (void)shift;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_fill_poly(
    jyppx_ocv_mat* img,
    const int* points_xy,
    int point_count,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int line_type,
    int shift,
    int offset_x,
    int offset_y)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_fill_poly";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (img == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "img");
        }

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Point> points;
        points.reserve(static_cast<size_t>(point_count));
        for (int i = 0; i < point_count; ++i)
        {
            const int offset = i * 2;
            points.push_back(cv::Point(points_xy[offset], points_xy[offset + 1]));
        }

        const cv::Point* contour = points.data();
        const int contour_count = point_count;
        cv::fillPoly(
            opencv_csharp_native::mat_value(img),
            &contour,
            &contour_count,
            1,
            get_scalar(color_v0, color_v1, color_v2, color_v3),
            line_type,
            shift,
            cv::Point(offset_x, offset_y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)line_type;
        (void)shift;
        (void)offset_x;
        (void)offset_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_ellipse2_poly_count(
    int center_x,
    int center_y,
    int axes_width,
    int axes_height,
    int angle,
    int arc_start,
    int arc_end,
    int delta,
    int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_ellipse2_poly_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_ellipse2_poly_points(
            center_x,
            center_y,
            axes_width,
            axes_height,
            angle,
            arc_start,
            arc_end,
            delta);
        *point_count = static_cast<int>(points.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)center_x;
        (void)center_y;
        (void)axes_width;
        (void)axes_height;
        (void)angle;
        (void)arc_start;
        (void)arc_end;
        (void)delta;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_ellipse2_poly_fill(
    int center_x,
    int center_y,
    int axes_width,
    int axes_height,
    int angle,
    int arc_start,
    int arc_end,
    int delta,
    int* points_xy,
    int point_capacity,
    int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_ellipse2_poly_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_capacity");
        }

        if (point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_ellipse2_poly_points(
            center_x,
            center_y,
            axes_width,
            axes_height,
            angle,
            arc_start,
            arc_end,
            delta);
        const int actual_count = static_cast<int>(points.size());
        if (point_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const int offset = i * 2;
            points_xy[offset] = points[static_cast<size_t>(i)].x;
            points_xy[offset + 1] = points[static_cast<size_t>(i)].y;
        }

        *point_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)center_x;
        (void)center_y;
        (void)axes_width;
        (void)axes_height;
        (void)angle;
        (void)arc_start;
        (void)arc_end;
        (void)delta;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_contour_area(
    const int* points_xy,
    int point_count,
    int oriented,
    double* area)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_contour_area";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (area == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "area");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        *area = cv::contourArea(points, oriented != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)oriented;
        *area = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_find_contours_count(
    const jyppx_ocv_mat* image,
    int mode,
    int method,
    int offset_x,
    int offset_y,
    int* contour_count,
    int* total_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_find_contours_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (contour_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_count");
        }

        if (total_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "total_point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point>> contours;
        std::vector<cv::Vec4i> hierarchy;
        int status = find_contours_core(api_name, image, mode, method, offset_x, offset_y, contours, hierarchy);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        int total_points = 0;
        for (size_t i = 0; i < contours.size(); ++i)
        {
            total_points += static_cast<int>(contours[i].size());
        }

        *contour_count = static_cast<int>(contours.size());
        *total_point_count = total_points;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)mode;
        (void)method;
        (void)offset_x;
        (void)offset_y;
        *contour_count = 0;
        *total_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_find_contours_fill(
    const jyppx_ocv_mat* image,
    int mode,
    int method,
    int offset_x,
    int offset_y,
    int* contours_xy,
    int point_capacity,
    int* contour_lengths,
    int contour_capacity,
    int* hierarchy,
    int hierarchy_capacity,
    int* contour_count,
    int* total_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_find_contours_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (contours_xy == nullptr && point_capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contours_xy");
        }

        if (contour_lengths == nullptr && contour_capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_lengths");
        }

        if (hierarchy == nullptr && hierarchy_capacity != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hierarchy");
        }

        if (contour_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_count");
        }

        if (total_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "total_point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point>> contours;
        std::vector<cv::Vec4i> hierarchy_values;
        int status = find_contours_core(api_name, image, mode, method, offset_x, offset_y, contours, hierarchy_values);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        int total_points = 0;
        for (size_t i = 0; i < contours.size(); ++i)
        {
            total_points += static_cast<int>(contours[i].size());
        }

        const int actual_contour_count = static_cast<int>(contours.size());
        if (contour_capacity < actual_contour_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_capacity");
        }

        if (point_capacity < total_points)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_capacity");
        }

        if (hierarchy_capacity < actual_contour_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hierarchy_capacity");
        }

        int point_offset = 0;
        for (int contour_index = 0; contour_index < actual_contour_count; ++contour_index)
        {
            const std::vector<cv::Point>& contour = contours[static_cast<size_t>(contour_index)];
            contour_lengths[contour_index] = static_cast<int>(contour.size());
            for (int point_index = 0; point_index < static_cast<int>(contour.size()); ++point_index)
            {
                const int xy_offset = (point_offset + point_index) * 2;
                const cv::Point& point = contour[static_cast<size_t>(point_index)];
                contours_xy[xy_offset] = point.x;
                contours_xy[xy_offset + 1] = point.y;
            }

            point_offset += static_cast<int>(contour.size());
        }

        write_vec4i_values(hierarchy_values, hierarchy);
        *contour_count = actual_contour_count;
        *total_point_count = total_points;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)mode;
        (void)method;
        (void)offset_x;
        (void)offset_y;
        *contour_count = 0;
        *total_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_draw_contours(
    jyppx_ocv_mat* image,
    const int* contours_xy,
    const int* contour_lengths,
    int contour_count,
    int contour_index,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    const int* hierarchy,
    int has_hierarchy,
    int max_level,
    int offset_x,
    int offset_y)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_draw_contours";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

        if (contours_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contours_xy");
        }

        if (contour_lengths == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_lengths");
        }

        if (contour_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<std::vector<cv::Point>> contours = get_contours_from_flat(contours_xy, contour_lengths, contour_count);
        const std::vector<cv::Vec4i> hierarchy_values = hierarchy == nullptr || has_hierarchy == 0
            ? std::vector<cv::Vec4i>()
            : get_hierarchy_from_flat(hierarchy, contour_count);
        cv::drawContours(
            opencv_csharp_native::mat_value(image),
            contours,
            contour_index,
            get_scalar(color_v0, color_v1, color_v2, color_v3),
            thickness,
            line_type,
            hierarchy_values,
            max_level,
            cv::Point(offset_x, offset_y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)contour_index;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)thickness;
        (void)line_type;
        (void)hierarchy;
        (void)has_hierarchy;
        (void)max_level;
        (void)offset_x;
        (void)offset_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_moments_points(
    const int* points_xy,
    int point_count,
    int binary_image,
    double* values,
    int value_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_moments_points";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "values");
        }

        if (value_capacity < 24)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value_capacity");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        const cv::Moments moments = cv::moments(points, binary_image != 0);
        write_moments_values(moments, values);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)binary_image;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_moments_mat(
    const jyppx_ocv_mat* array,
    int binary_image,
    double* values,
    int value_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_moments_mat";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (array == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "array");
        }

        if (values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "values");
        }

        if (value_capacity < 24)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value_capacity");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Moments moments = cv::moments(opencv_csharp_native::mat_value(array), binary_image != 0);
        write_moments_values(moments, values);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)binary_image;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_hu_moments(
    const double* moments_values,
    int value_count,
    double* hu_values,
    int hu_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_hu_moments";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (moments_values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "moments_values");
        }

        if (value_count < 24)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value_count");
        }

        if (hu_values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hu_values");
        }

        if (hu_capacity < 7)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hu_capacity");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Moments moments = get_moments_from_values(moments_values);
        cv::HuMoments(moments, hu_values);
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

int jyppx_ocv_imgproc_arc_length(
    const int* points_xy,
    int point_count,
    int closed,
    double* length)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_arc_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (length == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "length");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        *length = cv::arcLength(points, closed != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)closed;
        *length = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_approx_poly_dp_count(
    const int* curve_xy,
    int point_count,
    double epsilon,
    int closed,
    int* approx_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_approx_poly_dp_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (curve_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "curve_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (approx_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "approx_point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> approx_curve = get_approx_poly_dp_points(curve_xy, point_count, epsilon, closed);
        *approx_point_count = static_cast<int>(approx_curve.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)epsilon;
        (void)closed;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_approx_poly_dp_fill(
    const int* curve_xy,
    int point_count,
    double epsilon,
    int closed,
    int* approx_points_xy,
    int approx_point_capacity,
    int* approx_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_approx_poly_dp_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (curve_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "curve_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (approx_points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "approx_points_xy");
        }

        if (approx_point_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "approx_point_capacity");
        }

        if (approx_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "approx_point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> approx_curve = get_approx_poly_dp_points(curve_xy, point_count, epsilon, closed);
        const int actual_count = static_cast<int>(approx_curve.size());
        if (approx_point_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "approx_point_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const int offset = i * 2;
            approx_points_xy[offset] = approx_curve[static_cast<size_t>(i)].x;
            approx_points_xy[offset + 1] = approx_curve[static_cast<size_t>(i)].y;
        }

        *approx_point_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)epsilon;
        (void)closed;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_approx_poly_n_count(
    const int* curve_xy,
    int point_count,
    int nsides,
    float epsilon_percentage,
    int ensure_convex,
    int* approx_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_approx_poly_n_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (curve_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "curve_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (nsides <= 2)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "nsides");
        }

        if (approx_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "approx_point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV_GEOMETRY)
        const std::vector<cv::Point2f> approx_curve = get_approx_poly_n_points(curve_xy, point_count, nsides, epsilon_percentage, ensure_convex);
        *approx_point_count = static_cast<int>(approx_curve.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)epsilon_percentage;
        (void)ensure_convex;
        *approx_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_approx_poly_n_fill(
    const int* curve_xy,
    int point_count,
    int nsides,
    float epsilon_percentage,
    int ensure_convex,
    float* approx_points_xy,
    int approx_point_capacity,
    int* approx_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_approx_poly_n_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (curve_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "curve_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (nsides <= 2)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "nsides");
        }

        if (approx_points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "approx_points_xy");
        }

        if (approx_point_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "approx_point_capacity");
        }

        if (approx_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "approx_point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV_GEOMETRY)
        const std::vector<cv::Point2f> approx_curve = get_approx_poly_n_points(curve_xy, point_count, nsides, epsilon_percentage, ensure_convex);
        const int actual_count = static_cast<int>(approx_curve.size());
        if (approx_point_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "approx_point_capacity");
        }

        write_point2f_points(approx_curve, approx_points_xy);
        *approx_point_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)epsilon_percentage;
        (void)ensure_convex;
        *approx_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_bounding_rect(
    const int* points_xy,
    int point_count,
    int* x,
    int* y,
    int* width,
    int* height)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_bounding_rect";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (x == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "x");
        }

        if (y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "y");
        }

        if (width == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        const cv::Rect rect = cv::boundingRect(points);
        *x = rect.x;
        *y = rect.y;
        *width = rect.width;
        *height = rect.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *x = 0;
        *y = 0;
        *width = 0;
        *height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_is_contour_convex(
    const int* points_xy,
    int point_count,
    int* is_convex)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_is_contour_convex";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (is_convex == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "is_convex");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        *is_convex = cv::isContourConvex(points) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *is_convex = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_convex_hull_count(
    const int* points_xy,
    int point_count,
    int clockwise,
    int* hull_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_convex_hull_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (hull_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> hull = get_convex_hull_points(points_xy, point_count, clockwise);
        *hull_point_count = static_cast<int>(hull.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clockwise;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_convex_hull_fill(
    const int* points_xy,
    int point_count,
    int clockwise,
    int* hull_points_xy,
    int hull_point_capacity,
    int* hull_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_convex_hull_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (hull_points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_points_xy");
        }

        if (hull_point_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_point_capacity");
        }

        if (hull_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> hull = get_convex_hull_points(points_xy, point_count, clockwise);
        const int actual_count = static_cast<int>(hull.size());
        if (hull_point_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_point_capacity");
        }

        for (int i = 0; i < actual_count; ++i)
        {
            const int offset = i * 2;
            hull_points_xy[offset] = hull[static_cast<size_t>(i)].x;
            hull_points_xy[offset + 1] = hull[static_cast<size_t>(i)].y;
        }

        *hull_point_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clockwise;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_convex_hull_indices_count(
    const int* points_xy,
    int point_count,
    int clockwise,
    int* hull_index_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_convex_hull_indices_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (hull_index_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_index_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<int> hull = get_convex_hull_indices(points_xy, point_count, clockwise);
        *hull_index_count = static_cast<int>(hull.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clockwise;
        *hull_index_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_convex_hull_indices_fill(
    const int* points_xy,
    int point_count,
    int clockwise,
    int* hull_indices,
    int hull_index_capacity,
    int* hull_index_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_convex_hull_indices_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (hull_indices == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_indices");
        }

        if (hull_index_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_index_capacity");
        }

        if (hull_index_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_index_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<int> hull = get_convex_hull_indices(points_xy, point_count, clockwise);
        const int actual_count = static_cast<int>(hull.size());
        if (hull_index_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_index_capacity");
        }

        write_int_values(hull, hull_indices);
        *hull_index_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)clockwise;
        *hull_index_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_convexity_defects_count(
    const int* contour_xy,
    int contour_point_count,
    const int* hull_indices,
    int hull_index_count,
    int* defect_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_convexity_defects_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (contour_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_xy");
        }

        if (contour_point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_point_count");
        }

        if (hull_indices == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_indices");
        }

        if (hull_index_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_index_count");
        }

        if (defect_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "defect_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Vec4i> defects = get_convexity_defects(contour_xy, contour_point_count, hull_indices, hull_index_count);
        *defect_count = static_cast<int>(defects.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *defect_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_convexity_defects_fill(
    const int* contour_xy,
    int contour_point_count,
    const int* hull_indices,
    int hull_index_count,
    int* defects,
    int defect_capacity,
    int* defect_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_convexity_defects_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (contour_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_xy");
        }

        if (contour_point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_point_count");
        }

        if (hull_indices == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_indices");
        }

        if (hull_index_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hull_index_count");
        }

        if (defects == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "defects");
        }

        if (defect_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "defect_capacity");
        }

        if (defect_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "defect_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Vec4i> defects_values = get_convexity_defects(contour_xy, contour_point_count, hull_indices, hull_index_count);
        const int actual_count = static_cast<int>(defects_values.size());
        if (defect_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "defect_capacity");
        }

        write_vec4i_values(defects_values, defects);
        *defect_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *defect_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_min_enclosing_circle(
    const int* points_xy,
    int point_count,
    float* center_x,
    float* center_y,
    float* radius)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_min_enclosing_circle";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (center_x == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "center_x");
        }

        if (center_y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "center_y");
        }

        if (radius == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "radius");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        cv::Point2f center;
        float native_radius = 0.0F;
        cv::minEnclosingCircle(points, center, native_radius);
        *center_x = center.x;
        *center_y = center.y;
        *radius = native_radius;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *center_x = 0.0F;
        *center_y = 0.0F;
        *radius = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_point_polygon_test(
    const int* contour_xy,
    int point_count,
    float point_x,
    float point_y,
    int measure_dist,
    double* result)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_point_polygon_test";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (contour_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "result");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> contour = get_points_from_xy(contour_xy, point_count);
        *result = cv::pointPolygonTest(contour, cv::Point2f(point_x, point_y), measure_dist != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)point_x;
        (void)point_y;
        (void)measure_dist;
        *result = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_match_shapes(
    const int* contour1_xy,
    int contour1_point_count,
    const int* contour2_xy,
    int contour2_point_count,
    int method,
    double parameter,
    double* result)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_match_shapes";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (contour1_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour1_xy");
        }

        if (contour1_point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour1_point_count");
        }

        if (contour2_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour2_xy");
        }

        if (contour2_point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "contour2_point_count");
        }

        if (result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "result");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> contour1 = get_points_from_xy(contour1_xy, contour1_point_count);
        const std::vector<cv::Point> contour2 = get_points_from_xy(contour2_xy, contour2_point_count);
        *result = cv::matchShapes(contour1, contour2, method, parameter);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)method;
        (void)parameter;
        *result = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_min_area_rect(
    const int* points_xy,
    int point_count,
    float* center_x,
    float* center_y,
    float* width,
    float* height,
    float* angle)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_min_area_rect";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (center_x == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "center_x");
        }

        if (center_y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "center_y");
        }

        if (width == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

        if (angle == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "angle");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        const cv::RotatedRect rect = cv::minAreaRect(points);
        write_rotated_rect(rect, center_x, center_y, width, height, angle);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *center_x = 0.0F;
        *center_y = 0.0F;
        *width = 0.0F;
        *height = 0.0F;
        *angle = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_box_points(
    float center_x,
    float center_y,
    float width,
    float height,
    float angle,
    float* points_xy,
    int point_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_box_points";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_capacity < 4)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_capacity");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Point2f points[4];
        const cv::RotatedRect rect(cv::Point2f(center_x, center_y), cv::Size2f(width, height), angle);
        rect.points(points);

        for (int i = 0; i < 4; ++i)
        {
            const int offset = i * 2;
            points_xy[offset] = points[i].x;
            points_xy[offset + 1] = points[i].y;
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)center_x;
        (void)center_y;
        (void)width;
        (void)height;
        (void)angle;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_fit_ellipse(
    const int* points_xy,
    int point_count,
    float* center_x,
    float* center_y,
    float* width,
    float* height,
    float* angle)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_fit_ellipse";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_fit_ellipse(api_name, cv::fitEllipse, points_xy, point_count, center_x, center_y, width, height, angle);
#else
        (void)points_xy;
        (void)point_count;
        if (center_x != nullptr)
        {
            *center_x = 0.0F;
        }

        if (center_y != nullptr)
        {
            *center_y = 0.0F;
        }

        if (width != nullptr)
        {
            *width = 0.0F;
        }

        if (height != nullptr)
        {
            *height = 0.0F;
        }

        if (angle != nullptr)
        {
            *angle = 0.0F;
        }

        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_fit_ellipse_ams(
    const int* points_xy,
    int point_count,
    float* center_x,
    float* center_y,
    float* width,
    float* height,
    float* angle)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_fit_ellipse_ams";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_fit_ellipse(api_name, cv::fitEllipseAMS, points_xy, point_count, center_x, center_y, width, height, angle);
#else
        (void)points_xy;
        (void)point_count;
        if (center_x != nullptr)
        {
            *center_x = 0.0F;
        }

        if (center_y != nullptr)
        {
            *center_y = 0.0F;
        }

        if (width != nullptr)
        {
            *width = 0.0F;
        }

        if (height != nullptr)
        {
            *height = 0.0F;
        }

        if (angle != nullptr)
        {
            *angle = 0.0F;
        }

        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_fit_ellipse_direct(
    const int* points_xy,
    int point_count,
    float* center_x,
    float* center_y,
    float* width,
    float* height,
    float* angle)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_fit_ellipse_direct";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_fit_ellipse(api_name, cv::fitEllipseDirect, points_xy, point_count, center_x, center_y, width, height, angle);
#else
        (void)points_xy;
        (void)point_count;
        if (center_x != nullptr)
        {
            *center_x = 0.0F;
        }

        if (center_y != nullptr)
        {
            *center_y = 0.0F;
        }

        if (width != nullptr)
        {
            *width = 0.0F;
        }

        if (height != nullptr)
        {
            *height = 0.0F;
        }

        if (angle != nullptr)
        {
            *angle = 0.0F;
        }

        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_rotated_rectangle_intersection_count(
    float rect1_center_x,
    float rect1_center_y,
    float rect1_width,
    float rect1_height,
    float rect1_angle,
    float rect2_center_x,
    float rect2_center_y,
    float rect2_width,
    float rect2_height,
    float rect2_angle,
    int* intersection_type,
    int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_rotated_rectangle_intersection_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (intersection_type == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "intersection_type");
        }

        if (point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Point2f> points;
        *intersection_type = get_rotated_rectangle_intersection_points(
            rect1_center_x,
            rect1_center_y,
            rect1_width,
            rect1_height,
            rect1_angle,
            rect2_center_x,
            rect2_center_y,
            rect2_width,
            rect2_height,
            rect2_angle,
            points);
        *point_count = static_cast<int>(points.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rect1_center_x;
        (void)rect1_center_y;
        (void)rect1_width;
        (void)rect1_height;
        (void)rect1_angle;
        (void)rect2_center_x;
        (void)rect2_center_y;
        (void)rect2_width;
        (void)rect2_height;
        (void)rect2_angle;
        *intersection_type = 0;
        *point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_rotated_rectangle_intersection_fill(
    float rect1_center_x,
    float rect1_center_y,
    float rect1_width,
    float rect1_height,
    float rect1_angle,
    float rect2_center_x,
    float rect2_center_y,
    float rect2_width,
    float rect2_height,
    float rect2_angle,
    float* points_xy,
    int point_capacity,
    int* intersection_type,
    int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_rotated_rectangle_intersection_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_capacity");
        }

        if (intersection_type == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "intersection_type");
        }

        if (point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Point2f> points;
        *intersection_type = get_rotated_rectangle_intersection_points(
            rect1_center_x,
            rect1_center_y,
            rect1_width,
            rect1_height,
            rect1_angle,
            rect2_center_x,
            rect2_center_y,
            rect2_width,
            rect2_height,
            rect2_angle,
            points);

        const int actual_count = static_cast<int>(points.size());
        if (point_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_capacity");
        }

        write_point2f_points(points, points_xy);
        *point_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rect1_center_x;
        (void)rect1_center_y;
        (void)rect1_width;
        (void)rect1_height;
        (void)rect1_angle;
        (void)rect2_center_x;
        (void)rect2_center_y;
        (void)rect2_width;
        (void)rect2_height;
        (void)rect2_angle;
        *intersection_type = 0;
        *point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_closest_ellipse_points(
    float center_x,
    float center_y,
    float width,
    float height,
    float angle,
    const int* points_xy,
    int point_count,
    float* closest_points_xy,
    int closest_point_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_closest_ellipse_points";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (closest_points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "closest_points_xy");
        }

        if (closest_point_capacity < point_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "closest_point_capacity");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV_GEOMETRY)
        const cv::RotatedRect ellipse = get_rotated_rect(center_x, center_y, width, height, angle);
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        std::vector<cv::Point2f> closest_points;
        cv::getClosestEllipsePoints(ellipse, points, closest_points);
        write_point2f_points(closest_points, closest_points_xy);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)center_x;
        (void)center_y;
        (void)width;
        (void)height;
        (void)angle;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_min_enclosing_triangle(
    const int* points_xy,
    int point_count,
    float* triangle_points_xy,
    int triangle_point_capacity,
    double* area)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_min_enclosing_triangle";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (triangle_points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "triangle_points_xy");
        }

        if (triangle_point_capacity < 3)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "triangle_point_capacity");
        }

        if (area == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "area");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        std::vector<cv::Point2f> triangle;
        *area = cv::minEnclosingTriangle(points, triangle);
        write_point2f_points(triangle, triangle_points_xy);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *area = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_min_enclosing_convex_polygon(
    const int* points_xy,
    int point_count,
    int k,
    float* polygon_points_xy,
    int polygon_point_capacity,
    int* polygon_point_count,
    double* area)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_min_enclosing_convex_polygon";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (k <= 2)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "k");
        }

        if (polygon_points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon_points_xy");
        }

        if (polygon_point_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon_point_capacity");
        }

        if (polygon_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon_point_count");
        }

        if (area == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "area");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV_GEOMETRY)
        const std::vector<cv::Point> integer_points = get_points_from_xy(points_xy, point_count);
        const std::vector<cv::Point2f> points = get_point2f_points(integer_points);
        std::vector<cv::Point2f> polygon;

        if (point_count == k)
        {
            polygon = points;
            *area = static_cast<double>(cv::contourArea(points, false));
        }
        else
        {
            *area = cv::minEnclosingConvexPolygon(points, polygon, k);
        }

        const int actual_count = static_cast<int>(polygon.size());
        if (polygon_point_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon_point_capacity");
        }

        write_point2f_points(polygon, polygon_points_xy);
        *polygon_point_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *area = 0.0;
        *polygon_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_intersect_convex_convex_count(
    const int* polygon1_xy,
    int polygon1_point_count,
    const int* polygon2_xy,
    int polygon2_point_count,
    int handle_nested,
    float* area,
    int* intersecting_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_intersect_convex_convex_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (polygon1_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon1_xy");
        }

        if (polygon1_point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon1_point_count");
        }

        if (polygon2_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon2_xy");
        }

        if (polygon2_point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon2_point_count");
        }

        if (area == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "area");
        }

        if (intersecting_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "intersecting_point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point2f> intersecting_region = get_intersect_convex_convex_points(
            polygon1_xy,
            polygon1_point_count,
            polygon2_xy,
            polygon2_point_count,
            handle_nested,
            area);
        *intersecting_point_count = static_cast<int>(intersecting_region.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)handle_nested;
        *area = 0.0F;
        *intersecting_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_intersect_convex_convex_fill(
    const int* polygon1_xy,
    int polygon1_point_count,
    const int* polygon2_xy,
    int polygon2_point_count,
    int handle_nested,
    float* intersecting_points_xy,
    int intersecting_point_capacity,
    float* area,
    int* intersecting_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_intersect_convex_convex_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (polygon1_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon1_xy");
        }

        if (polygon1_point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon1_point_count");
        }

        if (polygon2_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon2_xy");
        }

        if (polygon2_point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "polygon2_point_count");
        }

        if (intersecting_points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "intersecting_points_xy");
        }

        if (intersecting_point_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "intersecting_point_capacity");
        }

        if (area == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "area");
        }

        if (intersecting_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "intersecting_point_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point2f> intersecting_region = get_intersect_convex_convex_points(
            polygon1_xy,
            polygon1_point_count,
            polygon2_xy,
            polygon2_point_count,
            handle_nested,
            area);
        const int actual_count = static_cast<int>(intersecting_region.size());
        if (intersecting_point_capacity < actual_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "intersecting_point_capacity");
        }

        write_point2f_points(intersecting_region, intersecting_points_xy);
        *intersecting_point_count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)handle_nested;
        *area = 0.0F;
        *intersecting_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_fit_line_2d(
    const int* points_xy,
    int point_count,
    int dist_type,
    double param,
    double reps,
    double aeps,
    float* vx,
    float* vy,
    float* x0,
    float* y0)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_fit_line_2d";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points_xy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points_xy");
        }

        if (point_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (vx == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "vx");
        }

        if (vy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "vy");
        }

        if (x0 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "x0");
        }

        if (y0 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "y0");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        cv::Vec4f line;
        cv::fitLine(points, line, dist_type, param, reps, aeps);
        *vx = line[0];
        *vy = line[1];
        *x0 = line[2];
        *y0 = line[3];
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dist_type;
        (void)param;
        (void)reps;
        (void)aeps;
        *vx = 0.0F;
        *vy = 0.0F;
        *x0 = 0.0F;
        *y0 = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_rectangle(
    jyppx_ocv_mat* img,
    int x1,
    int y1,
    int x2,
    int y2,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_rectangle";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (img == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "img");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::rectangle(
            opencv_csharp_native::mat_value(img),
            cv::Point(x1, y1),
            cv::Point(x2, y2),
            get_scalar(color_v0, color_v1, color_v2, color_v3),
            thickness,
            line_type,
            shift);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x1;
        (void)y1;
        (void)x2;
        (void)y2;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)thickness;
        (void)line_type;
        (void)shift;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_rectangle_by_rect(
    jyppx_ocv_mat* img,
    int x,
    int y,
    int width,
    int height,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_rectangle_by_rect";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (img == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "img");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::rectangle(
            opencv_csharp_native::mat_value(img),
            cv::Rect(x, y, width, height),
            get_scalar(color_v0, color_v1, color_v2, color_v3),
            thickness,
            line_type,
            shift);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x;
        (void)y;
        (void)width;
        (void)height;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)thickness;
        (void)line_type;
        (void)shift;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_circle(
    jyppx_ocv_mat* img,
    int center_x,
    int center_y,
    int radius,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_circle";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (img == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "img");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::circle(
            opencv_csharp_native::mat_value(img),
            cv::Point(center_x, center_y),
            radius,
            get_scalar(color_v0, color_v1, color_v2, color_v3),
            thickness,
            line_type,
            shift);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)center_x;
        (void)center_y;
        (void)radius;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)thickness;
        (void)line_type;
        (void)shift;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_ellipse(
    jyppx_ocv_mat* img,
    int center_x,
    int center_y,
    int axes_width,
    int axes_height,
    double angle,
    double start_angle,
    double end_angle,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int shift)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_ellipse";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (img == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "img");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::ellipse(
            opencv_csharp_native::mat_value(img),
            cv::Point(center_x, center_y),
            cv::Size(axes_width, axes_height),
            angle,
            start_angle,
            end_angle,
            get_scalar(color_v0, color_v1, color_v2, color_v3),
            thickness,
            line_type,
            shift);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)center_x;
        (void)center_y;
        (void)axes_width;
        (void)axes_height;
        (void)angle;
        (void)start_angle;
        (void)end_angle;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)thickness;
        (void)line_type;
        (void)shift;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_put_text(
    jyppx_ocv_mat* img,
    const char* text,
    int org_x,
    int org_y,
    int font_face,
    double font_scale,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness,
    int line_type,
    int bottom_left_origin)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_put_text";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (img == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "img");
        }

        if (text == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "text");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::putText(
            opencv_csharp_native::mat_value(img),
            std::string(text),
            cv::Point(org_x, org_y),
            font_face,
            font_scale,
            get_scalar(color_v0, color_v1, color_v2, color_v3),
            thickness,
            line_type,
            bottom_left_origin != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)org_x;
        (void)org_y;
        (void)font_face;
        (void)font_scale;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)thickness;
        (void)line_type;
        (void)bottom_left_origin;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_text_size(
    const char* text,
    int font_face,
    double font_scale,
    int thickness,
    int* width,
    int* height,
    int* base_line)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_text_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (text == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "text");
        }

        if (width == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

        if (base_line == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "base_line");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int actual_base_line = 0;
        const cv::Size text_size = cv::getTextSize(
            std::string(text),
            font_face,
            font_scale,
            thickness,
            &actual_base_line);

        *width = text_size.width;
        *height = text_size.height;
        *base_line = actual_base_line;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)font_face;
        (void)font_scale;
        (void)thickness;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_generalized_hough_ballard_create(jyppx_ocv_generalized_hough** hough)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_generalized_hough_ballard_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (hough == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hough");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Ptr<cv::GeneralizedHoughBallard> value = cv::createGeneralizedHoughBallard();
        auto result = new (std::nothrow) jyppx_ocv_generalized_hough{};
        if (result == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        result->value = value;
        result->ballard = value;
        *hough = result;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *hough = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_generalized_hough_guil_create(jyppx_ocv_generalized_hough** hough)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_generalized_hough_guil_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (hough == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "hough");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Ptr<cv::GeneralizedHoughGuil> value = cv::createGeneralizedHoughGuil();
        auto result = new (std::nothrow) jyppx_ocv_generalized_hough{};
        if (result == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        result->value = value;
        result->guil = value;
        *hough = result;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *hough = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_imgproc_generalized_hough_release(jyppx_ocv_generalized_hough* hough)
{
    delete hough;
}

int jyppx_ocv_imgproc_generalized_hough_set_template(
    jyppx_ocv_generalized_hough* hough,
    const jyppx_ocv_mat* templ,
    int center_x,
    int center_y)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_generalized_hough_set_template";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (templ == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "templ");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_generalized_hough(api_name, hough);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        hough->value->setTemplate(opencv_csharp_native::mat_value(templ), cv::Point(center_x, center_y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)hough;
        (void)center_x;
        (void)center_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_generalized_hough_set_template_edges(
    jyppx_ocv_generalized_hough* hough,
    const jyppx_ocv_mat* edges,
    const jyppx_ocv_mat* dx,
    const jyppx_ocv_mat* dy,
    int center_x,
    int center_y)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_generalized_hough_set_template_edges";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (edges == nullptr || dx == nullptr || dy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, edges == nullptr ? "edges" : dx == nullptr ? "dx" : "dy");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_generalized_hough(api_name, hough);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        hough->value->setTemplate(
            opencv_csharp_native::mat_value(edges),
            opencv_csharp_native::mat_value(dx),
            opencv_csharp_native::mat_value(dy),
            cv::Point(center_x, center_y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)hough;
        (void)center_x;
        (void)center_y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_generalized_hough_detect(
    jyppx_ocv_generalized_hough* hough,
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* positions,
    jyppx_ocv_mat* votes)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_generalized_hough_detect";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (image == nullptr || positions == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, image == nullptr ? "image" : "positions");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_generalized_hough(api_name, hough);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        cv::OutputArray votes_output = votes == nullptr ? cv::noArray() : cv::OutputArray(opencv_csharp_native::mat_value(votes));
        hough->value->detect(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(positions), votes_output);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)hough;
        (void)votes;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_generalized_hough_detect_edges(
    jyppx_ocv_generalized_hough* hough,
    const jyppx_ocv_mat* edges,
    const jyppx_ocv_mat* dx,
    const jyppx_ocv_mat* dy,
    jyppx_ocv_mat* positions,
    jyppx_ocv_mat* votes)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_generalized_hough_detect_edges";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (edges == nullptr || dx == nullptr || dy == nullptr || positions == nullptr)
        {
            const char* parameter = edges == nullptr ? "edges" : dx == nullptr ? "dx" : dy == nullptr ? "dy" : "positions";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_generalized_hough(api_name, hough);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        cv::OutputArray votes_output = votes == nullptr ? cv::noArray() : cv::OutputArray(opencv_csharp_native::mat_value(votes));
        hough->value->detect(
            opencv_csharp_native::mat_value(edges),
            opencv_csharp_native::mat_value(dx),
            opencv_csharp_native::mat_value(dy),
            opencv_csharp_native::mat_value(positions),
            votes_output);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)hough;
        (void)votes;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_generalized_hough_get_int_property(
    const jyppx_ocv_generalized_hough* hough,
    int property,
    int* value)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_generalized_hough_get_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_generalized_hough(api_name, hough);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        switch (property)
        {
        case 0: *value = hough->value->getCannyLowThresh(); break;
        case 1: *value = hough->value->getCannyHighThresh(); break;
        case 2: *value = hough->value->getMaxBufferSize(); break;
        case 3:
            if (!hough->ballard.empty()) *value = hough->ballard->getLevels();
            else if (!hough->guil.empty()) *value = hough->guil->getLevels();
            else return opencv_csharp_native::set_invalid_argument(api_name, "property");
            break;
        case 4:
            if (hough->ballard.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property");
            *value = hough->ballard->getVotesThreshold();
            break;
        case 5:
            if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property");
            *value = hough->guil->getAngleThresh();
            break;
        case 6:
            if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property");
            *value = hough->guil->getScaleThresh();
            break;
        case 7:
            if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property");
            *value = hough->guil->getPosThresh();
            break;
        default:
            return opencv_csharp_native::set_invalid_argument(api_name, "property");
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)hough;
        (void)property;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_generalized_hough_set_int_property(
    jyppx_ocv_generalized_hough* hough,
    int property,
    int value)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_generalized_hough_set_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_generalized_hough(api_name, hough);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        switch (property)
        {
        case 0: hough->value->setCannyLowThresh(value); break;
        case 1: hough->value->setCannyHighThresh(value); break;
        case 2: hough->value->setMaxBufferSize(value); break;
        case 3:
            if (!hough->ballard.empty()) hough->ballard->setLevels(value);
            else if (!hough->guil.empty()) hough->guil->setLevels(value);
            else return opencv_csharp_native::set_invalid_argument(api_name, "property");
            break;
        case 4:
            if (hough->ballard.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property");
            hough->ballard->setVotesThreshold(value);
            break;
        case 5:
            if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property");
            hough->guil->setAngleThresh(value);
            break;
        case 6:
            if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property");
            hough->guil->setScaleThresh(value);
            break;
        case 7:
            if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property");
            hough->guil->setPosThresh(value);
            break;
        default:
            return opencv_csharp_native::set_invalid_argument(api_name, "property");
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)hough;
        (void)property;
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_generalized_hough_get_double_property(
    const jyppx_ocv_generalized_hough* hough,
    int property,
    double* value)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_generalized_hough_get_double_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_generalized_hough(api_name, hough);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        switch (property)
        {
        case 0: *value = hough->value->getMinDist(); break;
        case 1: *value = hough->value->getDp(); break;
        case 2: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); *value = hough->guil->getXi(); break;
        case 3: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); *value = hough->guil->getAngleEpsilon(); break;
        case 4: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); *value = hough->guil->getMinAngle(); break;
        case 5: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); *value = hough->guil->getMaxAngle(); break;
        case 6: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); *value = hough->guil->getAngleStep(); break;
        case 7: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); *value = hough->guil->getMinScale(); break;
        case 8: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); *value = hough->guil->getMaxScale(); break;
        case 9: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); *value = hough->guil->getScaleStep(); break;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property");
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)hough;
        (void)property;
        *value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_generalized_hough_set_double_property(
    jyppx_ocv_generalized_hough* hough,
    int property,
    double value)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_generalized_hough_set_double_property";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_generalized_hough(api_name, hough);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        switch (property)
        {
        case 0: hough->value->setMinDist(value); break;
        case 1: hough->value->setDp(value); break;
        case 2: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); hough->guil->setXi(value); break;
        case 3: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); hough->guil->setAngleEpsilon(value); break;
        case 4: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); hough->guil->setMinAngle(value); break;
        case 5: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); hough->guil->setMaxAngle(value); break;
        case 6: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); hough->guil->setAngleStep(value); break;
        case 7: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); hough->guil->setMinScale(value); break;
        case 8: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); hough->guil->setMaxScale(value); break;
        case 9: if (hough->guil.empty()) return opencv_csharp_native::set_invalid_argument(api_name, "property"); hough->guil->setScaleStep(value); break;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property");
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)hough;
        (void)property;
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_cvt_color_two_plane(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst, int code)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_cvt_color_two_plane";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src1 == nullptr || src2 == nullptr || dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, src1 == nullptr ? "src1" : src2 == nullptr ? "src2" : "dst");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::cvtColorTwoPlane(opencv_csharp_native::mat_value(src1), opencv_csharp_native::mat_value(src2), opencv_csharp_native::mat_value(dst), code);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)code;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_demosaicing(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int code, int dst_cn)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_demosaicing";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, src == nullptr ? "src" : "dst");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::demosaicing(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), code, dst_cn);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)code;
        (void)dst_cn;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_apply_color_map(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int colormap)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_apply_color_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, src == nullptr ? "src" : "dst");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::applyColorMap(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), colormap);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)colormap;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_apply_color_map_user(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, const jyppx_ocv_mat* user_color)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_apply_color_map_user";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr || user_color == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, src == nullptr ? "src" : dst == nullptr ? "dst" : "user_color");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::applyColorMap(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), opencv_csharp_native::mat_value(user_color));
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

int jyppx_ocv_imgproc_blend_linear(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    const jyppx_ocv_mat* weights1,
    const jyppx_ocv_mat* weights2,
    jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_blend_linear";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src1 == nullptr || src2 == nullptr || weights1 == nullptr || weights2 == nullptr || dst == nullptr)
        {
            const char* parameter = src1 == nullptr ? "src1" : src2 == nullptr ? "src2" : weights1 == nullptr ? "weights1" : weights2 == nullptr ? "weights2" : "dst";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::blendLinear(
            opencv_csharp_native::mat_value(src1),
            opencv_csharp_native::mat_value(src2),
            opencv_csharp_native::mat_value(weights1),
            opencv_csharp_native::mat_value(weights2),
            opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_imgproc_stack_blur(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int width, int height)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_stack_blur";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, src == nullptr ? "src" : "dst");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::stackBlur(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), cv::Size(width, height));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width;
        (void)height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_spatial_gradient(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dx,
    jyppx_ocv_mat* dy,
    int ksize,
    int border_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_spatial_gradient";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dx == nullptr || dy == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, src == nullptr ? "src" : dx == nullptr ? "dx" : "dy");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::spatialGradient(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dx), opencv_csharp_native::mat_value(dy), ksize, border_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ksize;
        (void)border_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_threshold_with_mask(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask,
    double thresh,
    double maxval,
    int type,
    double* result)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_threshold_with_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr || mask == nullptr || result == nullptr)
        {
            const char* parameter = src == nullptr ? "src" : dst == nullptr ? "dst" : mask == nullptr ? "mask" : "result";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *result = cv::thresholdWithMask(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(mask),
            thresh,
            maxval,
            type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)thresh;
        (void)maxval;
        (void)type;
        *result = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_draw_marker(
    jyppx_ocv_mat* image,
    int position_x,
    int position_y,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int marker_type,
    int marker_size,
    int thickness,
    int line_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_draw_marker";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::drawMarker(
            opencv_csharp_native::mat_value(image),
            cv::Point(position_x, position_y),
            cv::Scalar(color_v0, color_v1, color_v2, color_v3),
            marker_type,
            marker_size,
            thickness,
            line_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)position_x;
        (void)position_y;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)marker_type;
        (void)marker_size;
        (void)thickness;
        (void)line_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_fill_convex_poly(
    jyppx_ocv_mat* image,
    const int* points_xy,
    int point_count,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int line_type,
    int shift)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_fill_convex_poly";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (image == nullptr || points_xy == nullptr || point_count < 3)
        {
            const char* parameter = image == nullptr ? "image" : points_xy == nullptr ? "points_xy" : "point_count";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Point> points = get_points_from_xy(points_xy, point_count);
        cv::fillConvexPoly(
            opencv_csharp_native::mat_value(image),
            points,
            cv::Scalar(color_v0, color_v1, color_v2, color_v3),
            line_type,
            shift);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)line_type;
        (void)shift;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_font_scale_from_height(int font_face, int pixel_height, int thickness, double* result)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_font_scale_from_height";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "result");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *result = cv::getFontScaleFromHeight(font_face, pixel_height, thickness);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)font_face;
        (void)pixel_height;
        (void)thickness;
        *result = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_undistort(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_mat* new_camera_matrix)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_undistort";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr || camera_matrix == nullptr || dist_coeffs == nullptr)
        {
            const char* parameter = src == nullptr ? "src" : dst == nullptr ? "dst" : camera_matrix == nullptr ? "camera_matrix" : "dist_coeffs";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::undistort(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            input_or_no_array(new_camera_matrix));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)new_camera_matrix;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_init_inverse_rectification_map(
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_mat* r,
    const jyppx_ocv_mat* new_camera_matrix,
    int size_width,
    int size_height,
    int m1type,
    jyppx_ocv_mat* map1,
    jyppx_ocv_mat* map2)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_init_inverse_rectification_map";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (camera_matrix == nullptr || dist_coeffs == nullptr || r == nullptr || new_camera_matrix == nullptr || map1 == nullptr || map2 == nullptr || size_width <= 0 || size_height <= 0)
        {
            const char* parameter = camera_matrix == nullptr ? "camera_matrix"
                : dist_coeffs == nullptr ? "dist_coeffs"
                : r == nullptr ? "r"
                : new_camera_matrix == nullptr ? "new_camera_matrix"
                : map1 == nullptr ? "map1"
                : map2 == nullptr ? "map2"
                : size_width <= 0 ? "size_width" : "size_height";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::initInverseRectificationMap(
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(new_camera_matrix),
            cv::Size(size_width, size_height),
            m1type,
            opencv_csharp_native::mat_value(map1),
            opencv_csharp_native::mat_value(map2));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)m1type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_fisheye_undistort_image(
    const jyppx_ocv_mat* distorted,
    jyppx_ocv_mat* undistorted,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_mat* new_camera_matrix,
    int new_size_width,
    int new_size_height)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_fisheye_undistort_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (distorted == nullptr || undistorted == nullptr || camera_matrix == nullptr || dist_coeffs == nullptr || new_size_width < 0 || new_size_height < 0)
        {
            const char* parameter = distorted == nullptr ? "distorted"
                : undistorted == nullptr ? "undistorted"
                : camera_matrix == nullptr ? "camera_matrix"
                : dist_coeffs == nullptr ? "dist_coeffs"
                : new_size_width < 0 ? "new_size_width" : "new_size_height";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::fisheye::undistortImage(
            opencv_csharp_native::mat_value(distorted),
            opencv_csharp_native::mat_value(undistorted),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            input_or_no_array(new_camera_matrix),
            cv::Size(new_size_width, new_size_height));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)new_camera_matrix;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_rect_sub_pix(
    const jyppx_ocv_mat* image,
    int patch_width,
    int patch_height,
    float center_x,
    float center_y,
    jyppx_ocv_mat* patch,
    int patch_type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_rect_sub_pix";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (image == nullptr || patch == nullptr || patch_width <= 0 || patch_height <= 0)
        {
            const char* parameter = image == nullptr ? "image" : patch == nullptr ? "patch" : patch_width <= 0 ? "patch_width" : "patch_height";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::getRectSubPix(
            opencv_csharp_native::mat_value(image),
            cv::Size(patch_width, patch_height),
            cv::Point2f(center_x, center_y),
            opencv_csharp_native::mat_value(patch),
            patch_type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)center_x;
        (void)center_y;
        (void)patch_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_warp_polar(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int width,
    int height,
    float center_x,
    float center_y,
    double max_radius,
    int flags)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_warp_polar";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr || width < 0 || height < 0 || !(max_radius > 0.0))
        {
            const char* parameter = src == nullptr ? "src" : dst == nullptr ? "dst" : width < 0 ? "width" : height < 0 ? "height" : "max_radius";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::warpPolar(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            cv::Size(width, height),
            cv::Point2f(center_x, center_y),
            max_radius,
            flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)center_x;
        (void)center_y;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_accumulate(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_accumulate";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, src == nullptr ? "src" : "dst");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::accumulate(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), input_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_accumulate_square(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_accumulate_square";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, src == nullptr ? "src" : "dst");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::accumulateSquare(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), input_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_accumulate_product(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_accumulate_product";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src1 == nullptr || src2 == nullptr || dst == nullptr)
        {
            const char* parameter = src1 == nullptr ? "src1" : src2 == nullptr ? "src2" : "dst";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::accumulateProduct(
            opencv_csharp_native::mat_value(src1),
            opencv_csharp_native::mat_value(src2),
            opencv_csharp_native::mat_value(dst),
            input_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_accumulate_weighted(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double alpha,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_accumulate_weighted";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr || alpha < 0.0 || alpha > 1.0)
        {
            const char* parameter = src == nullptr ? "src" : dst == nullptr ? "dst" : "alpha";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::accumulateWeighted(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            alpha,
            input_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_phase_correlate(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    const jyppx_ocv_mat* window,
    double* shift_x,
    double* shift_y,
    double* response)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_phase_correlate";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src1 == nullptr || src2 == nullptr || shift_x == nullptr || shift_y == nullptr || response == nullptr)
        {
            const char* parameter = src1 == nullptr ? "src1" : src2 == nullptr ? "src2" : shift_x == nullptr ? "shift_x" : shift_y == nullptr ? "shift_y" : "response";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Point2d shift = cv::phaseCorrelate(
            opencv_csharp_native::mat_value(src1),
            opencv_csharp_native::mat_value(src2),
            input_or_no_array(window),
            response);
        *shift_x = shift.x;
        *shift_y = shift.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)window;
        *shift_x = 0.0;
        *shift_y = 0.0;
        *response = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_phase_correlate_iterative(
    const jyppx_ocv_mat* src1,
    const jyppx_ocv_mat* src2,
    int l2_size,
    int max_iters,
    double* shift_x,
    double* shift_y)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_phase_correlate_iterative";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src1 == nullptr || src2 == nullptr || shift_x == nullptr || shift_y == nullptr || l2_size <= 0 || max_iters <= 0)
        {
            const char* parameter = src1 == nullptr ? "src1" : src2 == nullptr ? "src2" : shift_x == nullptr ? "shift_x" : shift_y == nullptr ? "shift_y" : l2_size <= 0 ? "l2_size" : "max_iters";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Point2d shift = cv::phaseCorrelateIterative(
            opencv_csharp_native::mat_value(src1),
            opencv_csharp_native::mat_value(src2),
            l2_size,
            max_iters);
        *shift_x = shift.x;
        *shift_y = shift.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *shift_x = 0.0;
        *shift_y = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_create_hanning_window(jyppx_ocv_mat* dst, int width, int height, int type)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_create_hanning_window";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (dst == nullptr || width <= 1 || height <= 1)
        {
            const char* parameter = dst == nullptr ? "dst" : width <= 1 ? "width" : "height";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::createHanningWindow(opencv_csharp_native::mat_value(dst), cv::Size(width, height), type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_emd(
    const jyppx_ocv_mat* signature1,
    const jyppx_ocv_mat* signature2,
    int distance_type,
    const jyppx_ocv_mat* cost,
    int has_lower_bound,
    float* lower_bound,
    jyppx_ocv_mat* flow,
    float* distance)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_emd";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (signature1 == nullptr || signature2 == nullptr || distance == nullptr || (has_lower_bound != 0 && lower_bound == nullptr))
        {
            const char* parameter = signature1 == nullptr ? "signature1" : signature2 == nullptr ? "signature2" : distance == nullptr ? "distance" : "lower_bound";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *distance = cv::EMD(
            opencv_csharp_native::mat_value(signature1),
            opencv_csharp_native::mat_value(signature2),
            distance_type,
            input_or_no_array(cost),
            has_lower_bound != 0 ? lower_bound : nullptr,
            output_or_no_array(flow));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)distance_type;
        (void)cost;
        (void)flow;
        *distance = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_watershed(const jyppx_ocv_mat* image, jyppx_ocv_mat* markers)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_watershed";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (image == nullptr || markers == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, image == nullptr ? "image" : "markers");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::watershed(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(markers));
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

int jyppx_ocv_imgproc_pyr_mean_shift_filtering(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double spatial_radius,
    double color_radius,
    int max_level,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_pyr_mean_shift_filtering";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || dst == nullptr || !(spatial_radius > 0.0) || !(color_radius > 0.0) || max_level < 0)
        {
            const char* parameter = src == nullptr ? "src" : dst == nullptr ? "dst" : !(spatial_radius > 0.0) ? "spatial_radius" : !(color_radius > 0.0) ? "color_radius" : "max_level";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::pyrMeanShiftFiltering(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            spatial_radius,
            color_radius,
            max_level,
            cv::TermCriteria(criteria_type, criteria_max_count, criteria_epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)criteria_type;
        (void)criteria_max_count;
        (void)criteria_epsilon;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_grab_cut(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* mask,
    int rect_x,
    int rect_y,
    int rect_width,
    int rect_height,
    jyppx_ocv_mat* background_model,
    jyppx_ocv_mat* foreground_model,
    int iteration_count,
    int mode)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_grab_cut";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (image == nullptr || mask == nullptr || background_model == nullptr || foreground_model == nullptr || iteration_count < 0 || rect_width < 0 || rect_height < 0)
        {
            const char* parameter = image == nullptr ? "image"
                : mask == nullptr ? "mask"
                : background_model == nullptr ? "background_model"
                : foreground_model == nullptr ? "foreground_model"
                : iteration_count < 0 ? "iteration_count"
                : rect_width < 0 ? "rect_width" : "rect_height";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::grabCut(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(mask),
            cv::Rect(rect_x, rect_y, rect_width, rect_height),
            opencv_csharp_native::mat_value(background_model),
            opencv_csharp_native::mat_value(foreground_model),
            iteration_count,
            mode);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rect_x;
        (void)rect_y;
        (void)mode;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_match_template(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* templ,
    jyppx_ocv_mat* result,
    int method,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_match_template";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (image == nullptr || templ == nullptr || result == nullptr)
        {
            const char* parameter = image == nullptr ? "image" : templ == nullptr ? "templ" : "result";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::matchTemplate(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(templ),
            opencv_csharp_native::mat_value(result),
            method,
            input_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)method;
        (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_find_contours_link_runs_count(
    const jyppx_ocv_mat* image,
    int include_hierarchy,
    int* contour_count,
    int* total_point_count,
    int* hierarchy_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_find_contours_link_runs_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (contour_count == nullptr || total_point_count == nullptr || hierarchy_count == nullptr || (include_hierarchy != 0 && include_hierarchy != 1))
        {
            const char* parameter = contour_count == nullptr ? "contour_count" : total_point_count == nullptr ? "total_point_count" : hierarchy_count == nullptr ? "hierarchy_count" : "include_hierarchy";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point>> contours;
        std::vector<cv::Vec4i> hierarchy;
        const int status = find_contours_link_runs_core(api_name, image, include_hierarchy, contours, hierarchy);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        int total_points = 0;
        for (const std::vector<cv::Point>& contour : contours)
        {
            total_points += static_cast<int>(contour.size());
        }
        *contour_count = static_cast<int>(contours.size());
        *total_point_count = total_points;
        *hierarchy_count = include_hierarchy != 0 ? static_cast<int>(hierarchy.size()) : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        *contour_count = 0;
        *total_point_count = 0;
        *hierarchy_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_find_contours_link_runs_fill(
    const jyppx_ocv_mat* image,
    int include_hierarchy,
    int* contours_xy,
    int point_capacity,
    int* contour_lengths,
    int contour_capacity,
    int* hierarchy_values,
    int hierarchy_capacity,
    int* written_contour_count,
    int* written_point_count,
    int* written_hierarchy_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_find_contours_link_runs_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        if ((point_capacity > 0 && contours_xy == nullptr)
            || (contour_capacity > 0 && contour_lengths == nullptr)
            || (hierarchy_capacity > 0 && hierarchy_values == nullptr)
            || point_capacity < 0
            || contour_capacity < 0
            || hierarchy_capacity < 0
            || written_contour_count == nullptr
            || written_point_count == nullptr
            || written_hierarchy_count == nullptr
            || (include_hierarchy != 0 && include_hierarchy != 1))
        {
            const char* parameter = point_capacity > 0 && contours_xy == nullptr ? "contours_xy"
                : contour_capacity > 0 && contour_lengths == nullptr ? "contour_lengths"
                : hierarchy_capacity > 0 && hierarchy_values == nullptr ? "hierarchy_values"
                : point_capacity < 0 ? "point_capacity"
                : contour_capacity < 0 ? "contour_capacity"
                : hierarchy_capacity < 0 ? "hierarchy_capacity"
                : written_contour_count == nullptr ? "written_contour_count"
                : written_point_count == nullptr ? "written_point_count"
                : written_hierarchy_count == nullptr ? "written_hierarchy_count" : "include_hierarchy";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point>> contours;
        std::vector<cv::Vec4i> hierarchy;
        const int status = find_contours_link_runs_core(api_name, image, include_hierarchy, contours, hierarchy);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        int total_points = 0;
        for (const std::vector<cv::Point>& contour : contours)
        {
            total_points += static_cast<int>(contour.size());
        }
        const int actual_contours = static_cast<int>(contours.size());
        const int actual_hierarchy = include_hierarchy != 0 ? static_cast<int>(hierarchy.size()) : 0;
        if (point_capacity < total_points || contour_capacity < actual_contours || hierarchy_capacity < actual_hierarchy)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "capacity");
        }
        int point_offset = 0;
        for (int contour_index = 0; contour_index < actual_contours; ++contour_index)
        {
            const std::vector<cv::Point>& contour = contours[static_cast<size_t>(contour_index)];
            contour_lengths[contour_index] = static_cast<int>(contour.size());
            for (const cv::Point& point : contour)
            {
                contours_xy[point_offset * 2] = point.x;
                contours_xy[point_offset * 2 + 1] = point.y;
                ++point_offset;
            }
        }
        for (int index = 0; index < actual_hierarchy; ++index)
        {
            const cv::Vec4i& value = hierarchy[static_cast<size_t>(index)];
            hierarchy_values[index * 4] = value[0];
            hierarchy_values[index * 4 + 1] = value[1];
            hierarchy_values[index * 4 + 2] = value[2];
            hierarchy_values[index * 4 + 3] = value[3];
        }
        *written_contour_count = actual_contours;
        *written_point_count = total_points;
        *written_hierarchy_count = actual_hierarchy;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)contours_xy;
        (void)contour_lengths;
        (void)hierarchy_values;
        *written_contour_count = 0;
        *written_point_count = 0;
        *written_hierarchy_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_draw_frame_axes(
    jyppx_ocv_mat* image,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_mat* rotation_vector,
    const jyppx_ocv_mat* translation_vector,
    float length,
    int thickness)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_draw_frame_axes";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (image == nullptr || camera_matrix == nullptr || dist_coeffs == nullptr || rotation_vector == nullptr || translation_vector == nullptr || !(length > 0.0F) || thickness <= 0)
        {
            const char* parameter = image == nullptr ? "image"
                : camera_matrix == nullptr ? "camera_matrix"
                : dist_coeffs == nullptr ? "dist_coeffs"
                : rotation_vector == nullptr ? "rotation_vector"
                : translation_vector == nullptr ? "translation_vector"
                : !(length > 0.0F) ? "length" : "thickness";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::drawFrameAxes(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            opencv_csharp_native::mat_value(rotation_vector),
            opencv_csharp_native::mat_value(translation_vector),
            length,
            thickness);
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

int jyppx_ocv_imgproc_font_face_create_default(jyppx_ocv_font_face** font_face)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_font_face_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (font_face == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "font_face");
        }
        *font_face = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_font_face* result = new (std::nothrow) jyppx_ocv_font_face();
        if (result == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        *font_face = result;
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

int jyppx_ocv_imgproc_font_face_create(
    const unsigned char* font_path_or_name_utf8,
    jyppx_ocv_font_face** font_face)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_font_face_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (font_path_or_name_utf8 == nullptr || font_face == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, font_path_or_name_utf8 == nullptr ? "font_path_or_name_utf8" : "font_face");
        }
        *font_face = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_font_face* result = new (std::nothrow) jyppx_ocv_font_face(reinterpret_cast<const char*>(font_path_or_name_utf8));
        if (result == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        *font_face = result;
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

void jyppx_ocv_imgproc_font_face_release(jyppx_ocv_font_face* font_face)
{
    delete font_face;
}

int jyppx_ocv_imgproc_font_face_set(
    jyppx_ocv_font_face* font_face,
    const unsigned char* font_path_or_name_utf8,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_font_face_set";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (font_face == nullptr || font_path_or_name_utf8 == nullptr || result == nullptr)
        {
            const char* parameter = font_face == nullptr ? "font_face" : font_path_or_name_utf8 == nullptr ? "font_path_or_name_utf8" : "result";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *result = font_face->value.set(reinterpret_cast<const char*>(font_path_or_name_utf8)) ? 1 : 0;
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

int jyppx_ocv_imgproc_font_face_get_name_size(const jyppx_ocv_font_face* font_face, int* byte_count)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_font_face_get_name_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (font_face == nullptr || byte_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, font_face == nullptr ? "font_face" : "byte_count");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *byte_count = static_cast<int>(font_face->value.getName().size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *byte_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_font_face_get_name_fill(
    const jyppx_ocv_font_face* font_face,
    unsigned char* buffer,
    int buffer_capacity,
    int* bytes_written)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_font_face_get_name_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (font_face == nullptr || buffer_capacity < 0 || (buffer_capacity > 0 && buffer == nullptr) || bytes_written == nullptr)
        {
            const char* parameter = font_face == nullptr ? "font_face" : buffer_capacity < 0 ? "buffer_capacity" : buffer_capacity > 0 && buffer == nullptr ? "buffer" : "bytes_written";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::String name = font_face->value.getName();
        const int required = static_cast<int>(name.size());
        if (buffer_capacity < required)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer_capacity");
        }
        if (required > 0)
        {
            std::memcpy(buffer, name.data(), static_cast<size_t>(required));
        }
        *bytes_written = required;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *bytes_written = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_font_face_set_instance(
    jyppx_ocv_font_face* font_face,
    const int* parameters,
    int parameter_count,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_font_face_set_instance";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (font_face == nullptr || parameter_count < 0 || (parameter_count > 0 && parameters == nullptr) || parameter_count % 2 != 0 || result == nullptr)
        {
            const char* parameter = font_face == nullptr ? "font_face"
                : parameter_count < 0 || parameter_count % 2 != 0 ? "parameter_count"
                : parameter_count > 0 && parameters == nullptr ? "parameters" : "result";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<int> values;
        if (parameter_count > 0)
        {
            values.assign(parameters, parameters + parameter_count);
        }
        *result = font_face->value.setInstance(values) ? 1 : 0;
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

int jyppx_ocv_imgproc_font_face_get_instance_count(
    const jyppx_ocv_font_face* font_face,
    int* parameter_count,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_font_face_get_instance_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (font_face == nullptr || parameter_count == nullptr || result == nullptr)
        {
            const char* parameter = font_face == nullptr ? "font_face" : parameter_count == nullptr ? "parameter_count" : "result";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<int> values;
        const bool success = font_face->value.getInstance(values);
        *parameter_count = static_cast<int>(values.size());
        *result = success ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *parameter_count = 0;
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_font_face_get_instance_fill(
    const jyppx_ocv_font_face* font_face,
    int* parameters,
    int parameter_capacity,
    int* parameters_written,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_font_face_get_instance_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (font_face == nullptr || parameter_capacity < 0 || (parameter_capacity > 0 && parameters == nullptr) || parameters_written == nullptr || result == nullptr)
        {
            const char* parameter = font_face == nullptr ? "font_face"
                : parameter_capacity < 0 ? "parameter_capacity"
                : parameter_capacity > 0 && parameters == nullptr ? "parameters"
                : parameters_written == nullptr ? "parameters_written" : "result";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<int> values;
        const bool success = font_face->value.getInstance(values);
        const int required = static_cast<int>(values.size());
        if (parameter_capacity < required)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameter_capacity");
        }
        if (required > 0)
        {
            std::memcpy(parameters, values.data(), static_cast<size_t>(required) * sizeof(int));
        }
        *parameters_written = required;
        *result = success ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *parameters_written = 0;
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_put_text_font_face(
    jyppx_ocv_mat* image,
    const unsigned char* text_utf8,
    int origin_x,
    int origin_y,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    jyppx_ocv_font_face* font_face,
    int size,
    int weight,
    int flags,
    int has_wrap,
    int wrap_start,
    int wrap_end,
    int* next_x,
    int* next_y)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_put_text_font_face";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (image == nullptr || text_utf8 == nullptr || font_face == nullptr || size <= 0 || weight < 0 || next_x == nullptr || next_y == nullptr || (has_wrap != 0 && wrap_end < wrap_start))
        {
            const char* parameter = image == nullptr ? "image"
                : text_utf8 == nullptr ? "text_utf8"
                : font_face == nullptr ? "font_face"
                : size <= 0 ? "size"
                : weight < 0 ? "weight"
                : next_x == nullptr ? "next_x"
                : next_y == nullptr ? "next_y" : "wrap";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Point next = cv::putText(
            opencv_csharp_native::mat_value(image),
            reinterpret_cast<const char*>(text_utf8),
            cv::Point(origin_x, origin_y),
            cv::Scalar(color_v0, color_v1, color_v2, color_v3),
            font_face->value,
            size,
            weight,
            static_cast<cv::PutTextFlags>(flags),
            has_wrap != 0 ? cv::Range(wrap_start, wrap_end) : cv::Range());
        *next_x = next.x;
        *next_y = next.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)origin_x;
        (void)origin_y;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)flags;
        *next_x = 0;
        *next_y = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_imgproc_get_text_size_font_face(
    int image_width,
    int image_height,
    const unsigned char* text_utf8,
    int origin_x,
    int origin_y,
    jyppx_ocv_font_face* font_face,
    int size,
    int weight,
    int flags,
    int has_wrap,
    int wrap_start,
    int wrap_end,
    int* result_x,
    int* result_y,
    int* result_width,
    int* result_height)
{
    constexpr const char* api_name = "jyppx_ocv_imgproc_get_text_size_font_face";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (image_width < 0 || image_height < 0 || text_utf8 == nullptr || font_face == nullptr || size <= 0 || weight < 0 || result_x == nullptr || result_y == nullptr || result_width == nullptr || result_height == nullptr || (has_wrap != 0 && wrap_end < wrap_start))
        {
            const char* parameter = image_width < 0 ? "image_width"
                : image_height < 0 ? "image_height"
                : text_utf8 == nullptr ? "text_utf8"
                : font_face == nullptr ? "font_face"
                : size <= 0 ? "size"
                : weight < 0 ? "weight"
                : result_x == nullptr ? "result_x"
                : result_y == nullptr ? "result_y"
                : result_width == nullptr ? "result_width"
                : result_height == nullptr ? "result_height" : "wrap";
            return opencv_csharp_native::set_invalid_argument(api_name, parameter);
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Rect result = cv::getTextSize(
            cv::Size(image_width, image_height),
            reinterpret_cast<const char*>(text_utf8),
            cv::Point(origin_x, origin_y),
            font_face->value,
            size,
            weight,
            static_cast<cv::PutTextFlags>(flags),
            has_wrap != 0 ? cv::Range(wrap_start, wrap_end) : cv::Range());
        *result_x = result.x;
        *result_y = result.y;
        *result_width = result.width;
        *result_height = result.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)origin_x;
        (void)origin_y;
        (void)flags;
        *result_x = 0;
        *result_y = 0;
        *result_width = 0;
        *result_height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

