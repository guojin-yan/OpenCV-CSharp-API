#include "open_cv_sharp/calib3d/calib3d.h"

#include "core/mat_handle.h"
#include "error_state.h"

#include <cmath>
#include <limits>
#include <new>
#include <vector>

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/geometry/3d.hpp>
#include <opencv2/imgproc.hpp>
#include <opencv2/objdetect.hpp>
#include <opencv2/calib.hpp>
#include <opencv2/stereo.hpp>
#endif

namespace
{
    int validate_input_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_term_criteria(
        const char* api_name,
        int type,
        int max_count,
        double epsilon)
    {
        constexpr int count_type = 1;
        constexpr int epsilon_type = 2;
        constexpr int supported_types = count_type | epsilon_type;
        if (type == 0 || (type & ~supported_types) != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "criteria_type");
        }
        if ((type & count_type) != 0 && max_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "criteria_max_count");
        }
        if ((type & epsilon_type) != 0 && (!std::isfinite(epsilon) || epsilon <= 0.0))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "criteria_epsilon");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_input_mat_array(
        const char* api_name,
        const jyppx_ocv_mat* const* values,
        int value_count,
        int minimum_count,
        const char* argument_name)
    {
        if (value_count < minimum_count || values == nullptr)
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

    int validate_output_mat_array(
        const char* api_name,
        jyppx_ocv_mat* const* values,
        int value_count,
        int minimum_count,
        const char* argument_name)
    {
        if (value_count < minimum_count || values == nullptr)
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

    int validate_multiview_arguments(
        const char* api_name,
        int frame_count,
        int camera_count,
        int image_frame_count,
        const int* image_widths,
        const int* image_heights,
        const unsigned char* detection_mask,
        const int* camera_models,
        jyppx_ocv_mat* const* camera_matrices,
        jyppx_ocv_mat* const* dist_coeffs,
        jyppx_ocv_mat* const* rotation_vectors,
        jyppx_ocv_mat* const* translation_vectors,
        const int* flags_for_intrinsics,
        int flags,
        int criteria_type,
        int criteria_max_count,
        double criteria_epsilon,
        double* reprojection_error,
        bool extended,
        jyppx_ocv_mat* initialization_pairs,
        jyppx_ocv_mat* const* rvecs0,
        jyppx_ocv_mat* const* tvecs0,
        jyppx_ocv_mat* per_frame_errors)
    {
        if (frame_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "frame_count");
        }
        if (camera_count < 2)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_count");
        }
        if (image_frame_count != frame_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_frame_count");
        }
        if (camera_count > std::numeric_limits<int>::max() / frame_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_count");
        }
        if (image_widths == nullptr || image_heights == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(
                api_name,
                image_widths == nullptr ? "image_widths" : "image_heights");
        }
        if (detection_mask == nullptr || camera_models == nullptr || flags_for_intrinsics == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(
                api_name,
                detection_mask == nullptr
                    ? "detection_mask"
                    : (camera_models == nullptr ? "camera_models" : "flags_for_intrinsics"));
        }

        for (int camera = 0; camera < camera_count; ++camera)
        {
            if (image_widths[camera] <= 0 || image_heights[camera] <= 0)
            {
                return opencv_csharp_native::set_invalid_argument(
                    api_name,
                    image_widths[camera] <= 0 ? "image_widths" : "image_heights");
            }
            if (camera_models[camera] != 0 && camera_models[camera] != 1)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "camera_models");
            }

            int visible_frame_count = 0;
            for (int frame = 0; frame < frame_count; ++frame)
            {
                visible_frame_count += detection_mask[camera * frame_count + frame] == 0 ? 0 : 1;
            }
            if (visible_frame_count == 0)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "detection_mask");
            }
        }

        constexpr int stereo_registration_flag = 1 << 26;
        if ((flags & stereo_registration_flag) != 0)
        {
            for (int camera = 1; camera < camera_count; ++camera)
            {
                if (camera_models[camera] != camera_models[0])
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "camera_models");
                }
            }
        }

        int status = validate_output_mat_array(api_name, camera_matrices, camera_count, 2, "camera_matrices");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat_array(api_name, dist_coeffs, camera_count, 2, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat_array(api_name, rotation_vectors, camera_count, 2, "rotation_vectors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat_array(api_name, translation_vectors, camera_count, 2, "translation_vectors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (extended)
        {
            status = validate_output_mat(api_name, initialization_pairs, "initialization_pairs");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            status = validate_output_mat_array(api_name, rvecs0, frame_count, 1, "rvecs0");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            status = validate_output_mat_array(api_name, tvecs0, frame_count, 1, "tvecs0");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
            status = validate_output_mat(api_name, per_frame_errors, "per_frame_errors");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }

        status = validate_term_criteria(api_name, criteria_type, criteria_max_count, criteria_epsilon);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return reprojection_error == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error")
            : OPENCV_CSHARP_STATUS_OK;
    }

    void write_rect_values(int rect_x, int rect_y, int rect_width, int rect_height, int* x, int* y, int* width, int* height)
    {
        if (x != nullptr)
        {
            *x = rect_x;
        }

        if (y != nullptr)
        {
            *y = rect_y;
        }

        if (width != nullptr)
        {
            *width = rect_width;
        }

        if (height != nullptr)
        {
            *height = rect_height;
        }
    }

    void write_rect2d_values(
        double rect_x,
        double rect_y,
        double rect_width,
        double rect_height,
        double* x,
        double* y,
        double* width,
        double* height)
    {
        *x = rect_x;
        *y = rect_y;
        *width = rect_width;
        *height = rect_height;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::InputArray input_or_no_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::OutputArray output_or_no_array(jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::OutputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::InputOutputArray input_output_or_no_array(jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputOutputArray(opencv_csharp_native::mat_value(mat));
    }

    std::vector<cv::Mat> to_mat_vector(const jyppx_ocv_mat* const* values, int value_count)
    {
        std::vector<cv::Mat> result;
        result.reserve(static_cast<std::size_t>(value_count));
        for (int i = 0; i < value_count; ++i)
        {
            result.push_back(opencv_csharp_native::mat_value(values[i]));
        }

        return result;
    }

    int assign_new_mat(const char* api_name, const cv::Mat& value, jyppx_ocv_mat** out_mat)
    {
        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        *out_mat = new (std::nothrow) jyppx_ocv_mat{ value };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
    }

    cv::TermCriteria make_term_criteria(int type, int max_count, double epsilon)
    {
        return cv::TermCriteria(type, max_count, epsilon);
    }

    void write_rect(const cv::Rect& rect, int* x, int* y, int* width, int* height)
    {
        write_rect_values(rect.x, rect.y, rect.width, rect.height, x, y, width, height);
    }

    void pack_pose_vectors(const std::vector<cv::Mat>& values, cv::Mat& output)
    {
        output.create(static_cast<int>(values.size()), 3, CV_64FC1);

        for (std::size_t i = 0; i < values.size(); i++)
        {
            cv::Mat row = values[i].reshape(1, 1);
            cv::Mat converted;
            row.convertTo(converted, CV_64F);

            double* destination = output.ptr<double>(static_cast<int>(i));
            for (int column = 0; column < 3; column++)
            {
                destination[column] = converted.at<double>(0, column);
            }
        }
    }

    void pack_point3f_values(const std::vector<cv::Point3f>& values, cv::Mat& output)
    {
        if (values.empty())
        {
            output.release();
            return;
        }

        output.create(static_cast<int>(values.size()), 3, CV_64FC1);
        for (std::size_t i = 0; i < values.size(); ++i)
        {
            double* destination = output.ptr<double>(static_cast<int>(i));
            destination[0] = values[i].x;
            destination[1] = values[i].y;
            destination[2] = values[i].z;
        }
    }

    int validate_point2f_groups(
        const char* api_name,
        const int* offsets,
        int group_count,
        const jyppx_ocv_calib3d_point2f* points,
        int point_count,
        const char* offsets_name,
        const char* points_name)
    {
        if (group_count < 0 || point_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, group_count < 0 ? offsets_name : points_name);
        }

        if (group_count == 0)
        {
            return point_count == 0
                ? OPENCV_CSHARP_STATUS_OK
                : opencv_csharp_native::set_invalid_argument(api_name, points_name);
        }

        if (offsets == nullptr || offsets[0] != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, offsets_name);
        }

        if (point_count > 0 && points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, points_name);
        }

        for (int i = 0; i < group_count; ++i)
        {
            if (offsets[i] < 0 || offsets[i + 1] < offsets[i] || offsets[i + 1] > point_count)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, offsets_name);
            }
        }

        return offsets[group_count] == point_count
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_invalid_argument(api_name, points_name);
    }

    int validate_point3f_groups(
        const char* api_name,
        const int* offsets,
        int group_count,
        const jyppx_ocv_calib3d_point3f* points,
        int point_count,
        const char* offsets_name,
        const char* points_name)
    {
        if (group_count < 0 || point_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, group_count < 0 ? offsets_name : points_name);
        }

        if (group_count == 0)
        {
            return point_count == 0
                ? OPENCV_CSHARP_STATUS_OK
                : opencv_csharp_native::set_invalid_argument(api_name, points_name);
        }

        if (offsets == nullptr || offsets[0] != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, offsets_name);
        }

        if (point_count > 0 && points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, points_name);
        }

        for (int i = 0; i < group_count; ++i)
        {
            if (offsets[i] < 0 || offsets[i + 1] < offsets[i] || offsets[i + 1] > point_count)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, offsets_name);
            }
        }

        return offsets[group_count] == point_count
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_invalid_argument(api_name, points_name);
    }

    int validate_calibration_point_groups(
        const char* api_name,
        const int* object_offsets,
        int object_group_count,
        const jyppx_ocv_calib3d_point3f* object_points,
        int object_point_count,
        const int* image_offsets,
        int image_group_count,
        const jyppx_ocv_calib3d_point2f* image_points,
        int image_point_count,
        const char* image_offsets_name,
        const char* image_points_name)
    {
        int status = validate_point3f_groups(api_name, object_offsets, object_group_count, object_points, object_point_count, "object_point_offsets", "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        status = validate_point2f_groups(api_name, image_offsets, image_group_count, image_points, image_point_count, image_offsets_name, image_points_name);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (object_group_count <= 0 || object_group_count != image_group_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_group_count");
        }

        for (int i = 0; i < object_group_count; ++i)
        {
            if ((object_offsets[i + 1] - object_offsets[i]) != (image_offsets[i + 1] - image_offsets[i]))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "point_group_count");
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_camera_registration_point_groups(
        const char* api_name,
        const int* object_point1_offsets,
        int object_point1_group_count,
        const jyppx_ocv_calib3d_point3f* object_points1,
        int object_point1_count,
        const int* object_point2_offsets,
        int object_point2_group_count,
        const jyppx_ocv_calib3d_point3f* object_points2,
        int object_point2_count,
        const int* image_point1_offsets,
        int image_point1_group_count,
        const jyppx_ocv_calib3d_point2f* image_points1,
        int image_point1_count,
        const int* image_point2_offsets,
        int image_point2_group_count,
        const jyppx_ocv_calib3d_point2f* image_points2,
        int image_point2_count)
    {
        int status = validate_calibration_point_groups(
            api_name,
            object_point1_offsets,
            object_point1_group_count,
            object_points1,
            object_point1_count,
            image_point1_offsets,
            image_point1_group_count,
            image_points1,
            image_point1_count,
            "image_point1_offsets",
            "image_points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        status = validate_calibration_point_groups(
            api_name,
            object_point2_offsets,
            object_point2_group_count,
            object_points2,
            object_point2_count,
            image_point2_offsets,
            image_point2_group_count,
            image_points2,
            image_point2_count,
            "image_point2_offsets",
            "image_points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        return object_point1_group_count == object_point2_group_count
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_invalid_argument(api_name, "object_point2_group_count");
    }

    int validate_planar_object_points(
        const char* api_name,
        const jyppx_ocv_calib3d_point3f* object_points,
        int object_point_count)
    {
        constexpr float planar_tolerance = 1.0e-6F;
        for (int i = 0; i < object_point_count; ++i)
        {
            if (std::fabs(object_points[i].z) > planar_tolerance)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "object_points");
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    std::vector<std::vector<cv::Point2f>> to_point2f_groups(
        const int* offsets,
        int group_count,
        const jyppx_ocv_calib3d_point2f* points)
    {
        std::vector<std::vector<cv::Point2f>> result;
        result.reserve(static_cast<size_t>(group_count));
        for (int i = 0; i < group_count; ++i)
        {
            std::vector<cv::Point2f> group;
            group.reserve(static_cast<size_t>(offsets[i + 1] - offsets[i]));
            for (int j = offsets[i]; j < offsets[i + 1]; ++j)
            {
                group.emplace_back(points[j].x, points[j].y);
            }

            result.push_back(std::move(group));
        }

        return result;
    }

    std::vector<std::vector<cv::Point3f>> to_point3f_groups(
        const int* offsets,
        int group_count,
        const jyppx_ocv_calib3d_point3f* points)
    {
        std::vector<std::vector<cv::Point3f>> result;
        result.reserve(static_cast<size_t>(group_count));
        for (int i = 0; i < group_count; ++i)
        {
            std::vector<cv::Point3f> group;
            group.reserve(static_cast<size_t>(offsets[i + 1] - offsets[i]));
            for (int j = offsets[i]; j < offsets[i + 1]; ++j)
            {
                group.emplace_back(points[j].x, points[j].y, points[j].z);
            }

            result.push_back(std::move(group));
        }

        return result;
    }

    std::vector<std::vector<cv::Mat>> to_multiview_image_points(
        const int* offsets,
        int camera_count,
        int frame_count,
        const jyppx_ocv_calib3d_point2f* points)
    {
        std::vector<std::vector<cv::Mat>> result(static_cast<std::size_t>(camera_count));
        for (int camera = 0; camera < camera_count; ++camera)
        {
            std::vector<cv::Mat>& frames = result[static_cast<std::size_t>(camera)];
            frames.reserve(static_cast<std::size_t>(frame_count));
            for (int frame = 0; frame < frame_count; ++frame)
            {
                const int group_index = camera * frame_count + frame;
                const int point_count = offsets[group_index + 1] - offsets[group_index];
                if (point_count == 0)
                {
                    frames.emplace_back();
                    continue;
                }

                cv::Mat group(1, point_count, CV_32FC2);
                cv::Point2f* destination = group.ptr<cv::Point2f>();
                for (int point = 0; point < point_count; ++point)
                {
                    const jyppx_ocv_calib3d_point2f& source = points[offsets[group_index] + point];
                    destination[point] = cv::Point2f(source.x, source.y);
                }
                frames.push_back(std::move(group));
            }
        }

        return result;
    }

    std::vector<cv::Size> to_multiview_image_sizes(
        const int* image_widths,
        const int* image_heights,
        int camera_count)
    {
        std::vector<cv::Size> result;
        result.reserve(static_cast<std::size_t>(camera_count));
        for (int camera = 0; camera < camera_count; ++camera)
        {
            result.emplace_back(image_widths[camera], image_heights[camera]);
        }
        return result;
    }

    cv::Mat to_multiview_detection_mask(
        const unsigned char* detection_mask,
        int camera_count,
        int frame_count)
    {
        cv::Mat result(camera_count, frame_count, CV_8UC1);
        for (int camera = 0; camera < camera_count; ++camera)
        {
            unsigned char* destination = result.ptr<unsigned char>(camera);
            for (int frame = 0; frame < frame_count; ++frame)
            {
                destination[frame] = detection_mask[camera * frame_count + frame] == 0 ? 0 : 1;
            }
        }
        return result;
    }

    cv::Mat to_multiview_models(const int* camera_models, int camera_count)
    {
        cv::Mat result(camera_count, 1, CV_8UC1);
        for (int camera = 0; camera < camera_count; ++camera)
        {
            result.at<unsigned char>(camera, 0) = static_cast<unsigned char>(camera_models[camera]);
        }
        return result;
    }

    cv::Mat to_multiview_intrinsic_flags(const int* flags_for_intrinsics, int camera_count)
    {
        cv::Mat result(camera_count, 1, CV_32SC1);
        for (int camera = 0; camera < camera_count; ++camera)
        {
            result.at<int>(camera, 0) = flags_for_intrinsics[camera];
        }
        return result;
    }

    std::vector<cv::Mat> to_output_mat_vector(jyppx_ocv_mat* const* values, int value_count)
    {
        std::vector<cv::Mat> result;
        result.reserve(static_cast<std::size_t>(value_count));
        for (int i = 0; i < value_count; ++i)
        {
            result.push_back(opencv_csharp_native::mat_value(values[i]));
        }
        return result;
    }

    void assign_mat_vector(
        const std::vector<cv::Mat>& values,
        jyppx_ocv_mat* const* outputs,
        int output_count)
    {
        CV_Assert(values.size() == static_cast<std::size_t>(output_count));
        for (int i = 0; i < output_count; ++i)
        {
            opencv_csharp_native::mat_value(outputs[i]) = values[static_cast<std::size_t>(i)];
        }
    }

    void assign_multiview_frame_vectors(
        const std::vector<cv::Mat>& values,
        jyppx_ocv_mat* const* outputs,
        const unsigned char* detection_mask,
        int camera_count,
        int frame_count)
    {
        CV_Assert(values.size() == static_cast<std::size_t>(frame_count));
        for (int frame = 0; frame < frame_count; ++frame)
        {
            bool visible = false;
            for (int camera = 0; camera < camera_count; ++camera)
            {
                if (detection_mask[camera * frame_count + frame] != 0)
                {
                    visible = true;
                    break;
                }
            }

            cv::Mat& output = opencv_csharp_native::mat_value(outputs[frame]);
            if (visible)
            {
                output = values[static_cast<std::size_t>(frame)];
            }
            else
            {
                output.release();
            }
        }
    }

    int validate_multiview_point_groups(
        const char* api_name,
        const int* object_point_offsets,
        int frame_count,
        const jyppx_ocv_calib3d_point3f* object_points,
        int object_point_count,
        const int* image_point_offsets,
        int camera_count,
        const jyppx_ocv_calib3d_point2f* image_points,
        int image_point_count,
        const unsigned char* detection_mask)
    {
        int status = validate_point3f_groups(
            api_name,
            object_point_offsets,
            frame_count,
            object_points,
            object_point_count,
            "object_point_offsets",
            "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        const int image_group_count = camera_count * frame_count;
        status = validate_point2f_groups(
            api_name,
            image_point_offsets,
            image_group_count,
            image_points,
            image_point_count,
            "image_point_offsets",
            "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        for (int frame = 0; frame < frame_count; ++frame)
        {
            if (object_point_offsets[frame + 1] == object_point_offsets[frame])
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "object_points");
            }
        }

        for (int camera = 0; camera < camera_count; ++camera)
        {
            for (int frame = 0; frame < frame_count; ++frame)
            {
                if (detection_mask[camera * frame_count + frame] == 0)
                {
                    continue;
                }

                const int object_count =
                    object_point_offsets[frame + 1] - object_point_offsets[frame];
                const int image_group_index = camera * frame_count + frame;
                const int image_count =
                    image_point_offsets[image_group_index + 1] - image_point_offsets[image_group_index];
                if (image_count != object_count)
                {
                    return opencv_csharp_native::set_invalid_argument(api_name, "image_points");
                }
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    double run_multiview_calibration(
        const int* object_point_offsets,
        int frame_count,
        const jyppx_ocv_calib3d_point3f* object_points,
        const int* image_point_offsets,
        int camera_count,
        const jyppx_ocv_calib3d_point2f* image_points,
        const int* image_widths,
        const int* image_heights,
        const unsigned char* detection_mask,
        const int* camera_models,
        jyppx_ocv_mat* const* camera_matrices,
        jyppx_ocv_mat* const* dist_coeffs,
        jyppx_ocv_mat* const* rotation_vectors,
        jyppx_ocv_mat* const* translation_vectors,
        jyppx_ocv_mat* initialization_pairs,
        jyppx_ocv_mat* const* rvecs0,
        jyppx_ocv_mat* const* tvecs0,
        jyppx_ocv_mat* per_frame_errors,
        const int* flags_for_intrinsics,
        int flags,
        int criteria_type,
        int criteria_max_count,
        double criteria_epsilon)
    {
        std::vector<std::vector<cv::Point3f>> native_object_points =
            to_point3f_groups(object_point_offsets, frame_count, object_points);
        std::vector<std::vector<cv::Mat>> native_image_points =
            to_multiview_image_points(image_point_offsets, camera_count, frame_count, image_points);
        std::vector<cv::Size> native_image_sizes =
            to_multiview_image_sizes(image_widths, image_heights, camera_count);
        cv::Mat native_detection_mask =
            to_multiview_detection_mask(detection_mask, camera_count, frame_count);
        cv::Mat native_models = to_multiview_models(camera_models, camera_count);
        cv::Mat native_intrinsic_flags =
            to_multiview_intrinsic_flags(flags_for_intrinsics, camera_count);
        std::vector<cv::Mat> native_camera_matrices =
            to_output_mat_vector(camera_matrices, camera_count);
        std::vector<cv::Mat> native_dist_coeffs =
            to_output_mat_vector(dist_coeffs, camera_count);
        std::vector<cv::Mat> native_rotation_vectors =
            to_output_mat_vector(rotation_vectors, camera_count);
        std::vector<cv::Mat> native_translation_vectors =
            to_output_mat_vector(translation_vectors, camera_count);

        double reprojection_error;
        if (initialization_pairs != nullptr)
        {
            cv::Mat native_initialization_pairs;
            std::vector<cv::Mat> native_rvecs0;
            std::vector<cv::Mat> native_tvecs0;
            cv::Mat native_per_frame_errors;
            reprojection_error = cv::calibrateMultiview(
                native_object_points,
                native_image_points,
                native_image_sizes,
                native_detection_mask,
                native_models,
                native_camera_matrices,
                native_dist_coeffs,
                native_rotation_vectors,
                native_translation_vectors,
                native_initialization_pairs,
                native_rvecs0,
                native_tvecs0,
                native_per_frame_errors,
                native_intrinsic_flags,
                flags,
                make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));

            opencv_csharp_native::mat_value(initialization_pairs) = native_initialization_pairs;
            assign_multiview_frame_vectors(
                native_rvecs0,
                rvecs0,
                detection_mask,
                camera_count,
                frame_count);
            assign_multiview_frame_vectors(
                native_tvecs0,
                tvecs0,
                detection_mask,
                camera_count,
                frame_count);
            opencv_csharp_native::mat_value(per_frame_errors) = native_per_frame_errors;
        }
        else
        {
            reprojection_error = cv::calibrateMultiview(
                native_object_points,
                native_image_points,
                native_image_sizes,
                native_detection_mask,
                native_models,
                native_camera_matrices,
                native_dist_coeffs,
                native_rotation_vectors,
                native_translation_vectors,
                native_intrinsic_flags,
                flags,
                make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        }

        assign_mat_vector(native_camera_matrices, camera_matrices, camera_count);
        assign_mat_vector(native_dist_coeffs, dist_coeffs, camera_count);
        assign_mat_vector(native_rotation_vectors, rotation_vectors, camera_count);
        assign_mat_vector(native_translation_vectors, translation_vectors, camera_count);
        return reprojection_error;
    }
#endif
}

int jyppx_ocv_calib3d_rodrigues(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    jyppx_ocv_mat* jacobian)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_rodrigues";

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
        cv::Rodrigues(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            output_or_no_array(jacobian));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)jacobian;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_rq_decomp3x3(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* mtx_r,
    jyppx_ocv_mat* mtx_q,
    jyppx_ocv_mat* qx,
    jyppx_ocv_mat* qy,
    jyppx_ocv_mat* qz,
    double* euler_x,
    double* euler_y,
    double* euler_z)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_rq_decomp3x3";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (mtx_r == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mtx_r");
        }

        if (mtx_q == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mtx_q");
        }

        if (euler_x == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "euler_x");
        }

        if (euler_y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "euler_y");
        }

        if (euler_z == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "euler_z");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Vec3d euler = cv::RQDecomp3x3(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(mtx_r),
            opencv_csharp_native::mat_value(mtx_q),
            output_or_no_array(qx),
            output_or_no_array(qy),
            output_or_no_array(qz));
        *euler_x = euler[0];
        *euler_y = euler[1];
        *euler_z = euler[2];
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)qx;
        (void)qy;
        (void)qz;
        *euler_x = 0.0;
        *euler_y = 0.0;
        *euler_z = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_decompose_projection_matrix(
    const jyppx_ocv_mat* proj_matrix,
    jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* rot_matrix,
    jyppx_ocv_mat* trans_vect,
    jyppx_ocv_mat* rot_matrix_x,
    jyppx_ocv_mat* rot_matrix_y,
    jyppx_ocv_mat* rot_matrix_z,
    jyppx_ocv_mat* euler_angles)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_decompose_projection_matrix";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (proj_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "proj_matrix");
        }

        if (camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_matrix");
        }

        if (rot_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rot_matrix");
        }

        if (trans_vect == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "trans_vect");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::decomposeProjectionMatrix(
            opencv_csharp_native::mat_value(proj_matrix),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(rot_matrix),
            opencv_csharp_native::mat_value(trans_vect),
            output_or_no_array(rot_matrix_x),
            output_or_no_array(rot_matrix_y),
            output_or_no_array(rot_matrix_z),
            output_or_no_array(euler_angles));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rot_matrix_x;
        (void)rot_matrix_y;
        (void)rot_matrix_z;
        (void)euler_angles;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_compose_rt(
    const jyppx_ocv_mat* rvec1,
    const jyppx_ocv_mat* tvec1,
    const jyppx_ocv_mat* rvec2,
    const jyppx_ocv_mat* tvec2,
    jyppx_ocv_mat* rvec3,
    jyppx_ocv_mat* tvec3)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_compose_rt";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (rvec1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rvec1");
        }

        if (tvec1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tvec1");
        }

        if (rvec2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rvec2");
        }

        if (tvec2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tvec2");
        }

        if (rvec3 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rvec3");
        }

        if (tvec3 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tvec3");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::composeRT(
            opencv_csharp_native::mat_value(rvec1),
            opencv_csharp_native::mat_value(tvec1),
            opencv_csharp_native::mat_value(rvec2),
            opencv_csharp_native::mat_value(tvec2),
            opencv_csharp_native::mat_value(rvec3),
            opencv_csharp_native::mat_value(tvec3));
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

int jyppx_ocv_calib3d_compose_rt_extended(
    const jyppx_ocv_mat* rvec1,
    const jyppx_ocv_mat* tvec1,
    const jyppx_ocv_mat* rvec2,
    const jyppx_ocv_mat* tvec2,
    jyppx_ocv_mat* rvec3,
    jyppx_ocv_mat* tvec3,
    jyppx_ocv_mat* dr3dr1,
    jyppx_ocv_mat* dr3dt1,
    jyppx_ocv_mat* dr3dr2,
    jyppx_ocv_mat* dr3dt2,
    jyppx_ocv_mat* dt3dr1,
    jyppx_ocv_mat* dt3dt1,
    jyppx_ocv_mat* dt3dr2,
    jyppx_ocv_mat* dt3dt2)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_compose_rt_extended";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, rvec1, "rvec1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, tvec1, "tvec1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, rvec2, "rvec2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, tvec2, "tvec2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvec3, "rvec3");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvec3, "tvec3");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dr3dr1, "dr3dr1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dr3dt1, "dr3dt1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dr3dr2, "dr3dr2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dr3dt2, "dr3dt2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dt3dr1, "dt3dr1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dt3dt1, "dt3dt1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dt3dr2, "dt3dr2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dt3dt2, "dt3dt2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Mat& input_rvec1 = opencv_csharp_native::mat_value(rvec1);
        const bool row_vectors = input_rvec1.rows == 1;

        if (row_vectors)
        {
            cv::Mat rvec1_column = input_rvec1.t();
            cv::Mat tvec1_column = opencv_csharp_native::mat_value(tvec1).t();
            cv::Mat rvec2_column = opencv_csharp_native::mat_value(rvec2).t();
            cv::Mat tvec2_column = opencv_csharp_native::mat_value(tvec2).t();
            cv::Mat rvec3_column;
            cv::Mat tvec3_column;

            cv::composeRT(
                rvec1_column,
                tvec1_column,
                rvec2_column,
                tvec2_column,
                rvec3_column,
                tvec3_column,
                opencv_csharp_native::mat_value(dr3dr1),
                opencv_csharp_native::mat_value(dr3dt1),
                opencv_csharp_native::mat_value(dr3dr2),
                opencv_csharp_native::mat_value(dr3dt2),
                opencv_csharp_native::mat_value(dt3dr1),
                opencv_csharp_native::mat_value(dt3dt1),
                opencv_csharp_native::mat_value(dt3dr2),
                opencv_csharp_native::mat_value(dt3dt2));

            cv::transpose(rvec3_column, opencv_csharp_native::mat_value(rvec3));
            cv::transpose(tvec3_column, opencv_csharp_native::mat_value(tvec3));
        }
        else
        {
            cv::composeRT(
                input_rvec1,
                opencv_csharp_native::mat_value(tvec1),
                opencv_csharp_native::mat_value(rvec2),
                opencv_csharp_native::mat_value(tvec2),
                opencv_csharp_native::mat_value(rvec3),
                opencv_csharp_native::mat_value(tvec3),
                opencv_csharp_native::mat_value(dr3dr1),
                opencv_csharp_native::mat_value(dr3dt1),
                opencv_csharp_native::mat_value(dr3dr2),
                opencv_csharp_native::mat_value(dr3dt2),
                opencv_csharp_native::mat_value(dt3dr1),
                opencv_csharp_native::mat_value(dt3dt1),
                opencv_csharp_native::mat_value(dt3dr2),
                opencv_csharp_native::mat_value(dt3dt2));
        }
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

int jyppx_ocv_calib3d_project_points(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* rvec,
    const jyppx_ocv_mat* tvec,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* image_points,
    jyppx_ocv_mat* jacobian,
    double aspect_ratio)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_project_points";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (object_points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "object_points");
        }

        if (rvec == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rvec");
        }

        if (tvec == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tvec");
        }

        if (camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_matrix");
        }

        if (dist_coeffs == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dist_coeffs");
        }

        if (image_points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_points");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::projectPoints(
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            opencv_csharp_native::mat_value(image_points),
            output_or_no_array(jacobian),
            aspect_ratio);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)jacobian;
        (void)aspect_ratio;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_project_points_separated_jacobians(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* rvec,
    const jyppx_ocv_mat* tvec,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* image_points,
    jyppx_ocv_mat* dpdr,
    jyppx_ocv_mat* dpdt,
    jyppx_ocv_mat* dpdf,
    jyppx_ocv_mat* dpdc,
    jyppx_ocv_mat* dpdk,
    jyppx_ocv_mat* dpdo,
    double aspect_ratio)
{
    constexpr const char* api_name =
        "jyppx_ocv_calib3d_project_points_separated_jacobians";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, object_points, "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, rvec, "rvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, tvec, "tvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, image_points, "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dpdr, "dpdr");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dpdt, "dpdt");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dpdf, "dpdf");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dpdc, "dpdc");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dpdk, "dpdk");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dpdo, "dpdo");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat normalized_object_points =
            opencv_csharp_native::mat_value(object_points);
        int point_count = normalized_object_points.checkVector(3);
        if (point_count < 0)
        {
            normalized_object_points = normalized_object_points.t();
        }
        point_count = normalized_object_points.checkVector(3);
        CV_Assert(
            point_count >= 0 &&
            (normalized_object_points.depth() == CV_32F ||
             normalized_object_points.depth() == CV_64F));
        if (normalized_object_points.cols == 3)
        {
            normalized_object_points = normalized_object_points.reshape(3);
        }

        const cv::Mat& input_dist_coeffs =
            opencv_csharp_native::mat_value(dist_coeffs);
        cv::Mat zero_dist_coeffs;
        const cv::Mat* effective_dist_coeffs = &input_dist_coeffs;
        if (input_dist_coeffs.empty())
        {
            zero_dist_coeffs = cv::Mat::zeros(5, 1, CV_64FC1);
            effective_dist_coeffs = &zero_dist_coeffs;
        }

        cv::projectPoints(
            normalized_object_points,
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            opencv_csharp_native::mat_value(camera_matrix),
            *effective_dist_coeffs,
            opencv_csharp_native::mat_value(image_points),
            opencv_csharp_native::mat_value(dpdr),
            opencv_csharp_native::mat_value(dpdt),
            opencv_csharp_native::mat_value(dpdf),
            opencv_csharp_native::mat_value(dpdc),
            opencv_csharp_native::mat_value(dpdk),
            opencv_csharp_native::mat_value(dpdo),
            aspect_ratio);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)aspect_ratio;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_solve_pnp(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int use_extrinsic_guess,
    int flags,
    int* solved)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_solve_pnp";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (object_points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "object_points");
        }

        if (image_points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_points");
        }

        if (camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_matrix");
        }

        if (dist_coeffs == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dist_coeffs");
        }

        if (rvec == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rvec");
        }

        if (tvec == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tvec");
        }

        if (solved == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "solved");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        bool result = cv::solvePnP(
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(image_points),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            use_extrinsic_guess != 0,
            flags);
        *solved = result ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)use_extrinsic_guess;
        (void)flags;
        *solved = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_solve_pnp_ransac(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int use_extrinsic_guess,
    int iterations_count,
    float reprojection_error,
    double confidence,
    jyppx_ocv_mat* inliers,
    int flags,
    int* solved)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_solve_pnp_ransac";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (object_points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "object_points");
        }

        if (image_points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_points");
        }

        if (camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_matrix");
        }

        if (dist_coeffs == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dist_coeffs");
        }

        if (rvec == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rvec");
        }

        if (tvec == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tvec");
        }

        if (solved == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "solved");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        bool result = cv::solvePnPRansac(
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(image_points),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            use_extrinsic_guess != 0,
            iterations_count,
            reprojection_error,
            confidence,
            output_or_no_array(inliers),
            flags);
        *solved = result ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)use_extrinsic_guess;
        (void)iterations_count;
        (void)reprojection_error;
        (void)confidence;
        (void)inliers;
        (void)flags;
        *solved = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_fisheye_project_points(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* rvec,
    const jyppx_ocv_mat* tvec,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* image_points,
    double alpha,
    jyppx_ocv_mat* jacobian)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_fisheye_project_points";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, object_points, "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, rvec, "rvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, tvec, "tvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, image_points, "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!std::isfinite(alpha))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "alpha");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::fisheye::projectPoints(
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(image_points),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            alpha,
            output_or_no_array(jacobian));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)jacobian;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_fisheye_distort_points(
    const jyppx_ocv_mat* undistorted,
    jyppx_ocv_mat* distorted,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    double alpha)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_fisheye_distort_points";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, undistorted, "undistorted");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, distorted, "distorted");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!std::isfinite(alpha))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "alpha");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::fisheye::distortPoints(
            opencv_csharp_native::mat_value(undistorted),
            opencv_csharp_native::mat_value(distorted),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            alpha);
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

int jyppx_ocv_calib3d_fisheye_distort_points_with_camera_matrix(
    const jyppx_ocv_mat* undistorted,
    jyppx_ocv_mat* distorted,
    const jyppx_ocv_mat* undistorted_camera_matrix,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    double alpha)
{
    constexpr const char* api_name =
        "jyppx_ocv_calib3d_fisheye_distort_points_with_camera_matrix";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, undistorted, "undistorted");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, distorted, "distorted");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(
            api_name,
            undistorted_camera_matrix,
            "undistorted_camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (!std::isfinite(alpha))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "alpha");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::fisheye::distortPoints(
            opencv_csharp_native::mat_value(undistorted),
            opencv_csharp_native::mat_value(distorted),
            opencv_csharp_native::mat_value(undistorted_camera_matrix),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            alpha);
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

int jyppx_ocv_calib3d_fisheye_undistort_points(
    const jyppx_ocv_mat* distorted,
    jyppx_ocv_mat* undistorted,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_mat* r,
    const jyppx_ocv_mat* p,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_fisheye_undistort_points";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, distorted, "distorted");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, undistorted, "undistorted");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_term_criteria(
            api_name,
            criteria_type,
            criteria_max_count,
            criteria_epsilon);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::fisheye::undistortPoints(
            opencv_csharp_native::mat_value(distorted),
            opencv_csharp_native::mat_value(undistorted),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            input_or_no_array(r),
            input_or_no_array(p),
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)r;
        (void)p;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_fisheye_estimate_new_camera_matrix(
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    int image_width,
    int image_height,
    const jyppx_ocv_mat* r,
    jyppx_ocv_mat* new_camera_matrix,
    double balance,
    int new_image_width,
    int new_image_height,
    double fov_scale)
{
    constexpr const char* api_name =
        "jyppx_ocv_calib3d_fisheye_estimate_new_camera_matrix";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, new_camera_matrix, "new_camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_width <= 0 || image_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(
                api_name,
                image_width <= 0 ? "image_width" : "image_height");
        }
        if (!std::isfinite(balance) || balance < 0.0 || balance > 1.0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "balance");
        }
        if (!std::isfinite(fov_scale) || fov_scale <= 0.0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "fov_scale");
        }
        if (new_image_width < 0 ||
            new_image_height < 0 ||
            ((new_image_width == 0) != (new_image_height == 0)))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "new_image_size");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::fisheye::estimateNewCameraMatrixForUndistortRectify(
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            cv::Size(image_width, image_height),
            input_or_no_array(r),
            opencv_csharp_native::mat_value(new_camera_matrix),
            balance,
            cv::Size(new_image_width, new_image_height),
            fov_scale);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)r;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_fisheye_solve_pnp(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int use_extrinsic_guess,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int* solved)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_fisheye_solve_pnp";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, object_points, "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, image_points, "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvec, "rvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvec, "tvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_term_criteria(
            api_name,
            criteria_type,
            criteria_max_count,
            criteria_epsilon);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (solved == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "solved");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        bool result = cv::fisheye::solvePnP(
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(image_points),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            use_extrinsic_guess != 0,
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        *solved = result ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)use_extrinsic_guess;
        (void)flags;
        *solved = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_fisheye_solve_pnp_ransac(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int use_extrinsic_guess,
    int iterations_count,
    float reprojection_error,
    double confidence,
    jyppx_ocv_mat* inliers,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int* solved)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_fisheye_solve_pnp_ransac";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, object_points, "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, image_points, "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvec, "rvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvec, "tvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_term_criteria(
            api_name,
            criteria_type,
            criteria_max_count,
            criteria_epsilon);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (iterations_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "iterations_count");
        }
        if (!std::isfinite(reprojection_error) || reprojection_error <= 0.0F)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }
        if (!std::isfinite(confidence) || confidence <= 0.0 || confidence >= 1.0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "confidence");
        }
        if (solved == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "solved");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        bool result = cv::fisheye::solvePnPRansac(
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(image_points),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            use_extrinsic_guess != 0,
            iterations_count,
            reprojection_error,
            confidence,
            output_or_no_array(inliers),
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        *solved = result ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)use_extrinsic_guess;
        (void)inliers;
        (void)flags;
        *solved = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_find_homography(
    const jyppx_ocv_mat* src_points,
    const jyppx_ocv_mat* dst_points,
    int method,
    double ransac_reproj_threshold,
    jyppx_ocv_mat* mask,
    int max_iters,
    double confidence,
    jyppx_ocv_mat** homography)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_find_homography";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src_points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src_points");
        }

        if (dst_points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst_points");
        }

        if (homography == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "homography");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat value = cv::findHomography(
            opencv_csharp_native::mat_value(src_points),
            opencv_csharp_native::mat_value(dst_points),
            method,
            ransac_reproj_threshold,
            output_or_no_array(mask),
            max_iters,
            confidence);
        return assign_new_mat(api_name, value, homography);
#else
        (void)method;
        (void)ransac_reproj_threshold;
        (void)mask;
        (void)max_iters;
        (void)confidence;
        *homography = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_find_fundamental_mat(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    int method,
    double ransac_reproj_threshold,
    double confidence,
    int max_iters,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat** fundamental)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_find_fundamental_mat";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points1");
        }

        if (points2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points2");
        }

        if (fundamental == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "fundamental");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat value = cv::findFundamentalMat(
            opencv_csharp_native::mat_value(points1),
            opencv_csharp_native::mat_value(points2),
            method,
            ransac_reproj_threshold,
            confidence,
            max_iters,
            output_or_no_array(mask));
        return assign_new_mat(api_name, value, fundamental);
#else
        (void)method;
        (void)ransac_reproj_threshold;
        (void)confidence;
        (void)max_iters;
        (void)mask;
        *fundamental = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_find_essential_mat(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* camera_matrix,
    int method,
    double prob,
    double threshold,
    int max_iters,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat** essential)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_find_essential_mat";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points1");
        }

        if (points2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points2");
        }

        if (camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_matrix");
        }

        if (essential == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "essential");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat value = cv::findEssentialMat(
            opencv_csharp_native::mat_value(points1),
            opencv_csharp_native::mat_value(points2),
            opencv_csharp_native::mat_value(camera_matrix),
            method,
            prob,
            threshold,
            max_iters,
            output_or_no_array(mask));
        return assign_new_mat(api_name, value, essential);
#else
        (void)method;
        (void)prob;
        (void)threshold;
        (void)max_iters;
        (void)mask;
        *essential = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_find_essential_mat_focal(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    double focal,
    double pp_x,
    double pp_y,
    int method,
    double prob,
    double threshold,
    int max_iters,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat** essential)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_find_essential_mat_focal";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points1");
        }

        if (points2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points2");
        }

        if (essential == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "essential");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat value = cv::findEssentialMat(
            opencv_csharp_native::mat_value(points1),
            opencv_csharp_native::mat_value(points2),
            focal,
            cv::Point2d(pp_x, pp_y),
            method,
            prob,
            threshold,
            max_iters,
            output_or_no_array(mask));
        return assign_new_mat(api_name, value, essential);
#else
        (void)focal;
        (void)pp_x;
        (void)pp_y;
        (void)method;
        (void)prob;
        (void)threshold;
        (void)max_iters;
        (void)mask;
        *essential = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_find_essential_mat_two_cameras(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* camera_matrix1,
    const jyppx_ocv_mat* dist_coeffs1,
    const jyppx_ocv_mat* camera_matrix2,
    const jyppx_ocv_mat* dist_coeffs2,
    int method,
    double prob,
    double threshold,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat** essential)
{
    constexpr const char* api_name =
        "jyppx_ocv_calib3d_find_essential_mat_two_cameras";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, points1, "points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points2, "points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix1, "camera_matrix1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs1, "dist_coeffs1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix2, "camera_matrix2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs2, "dist_coeffs2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (essential == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "essential");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Mat value = cv::findEssentialMat(
            opencv_csharp_native::mat_value(points1),
            opencv_csharp_native::mat_value(points2),
            opencv_csharp_native::mat_value(camera_matrix1),
            opencv_csharp_native::mat_value(dist_coeffs1),
            opencv_csharp_native::mat_value(camera_matrix2),
            opencv_csharp_native::mat_value(dist_coeffs2),
            method,
            prob,
            threshold,
            output_or_no_array(mask));
        return assign_new_mat(api_name, value, essential);
#else
        (void)method;
        (void)prob;
        (void)threshold;
        (void)mask;
        *essential = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_decompose_essential_mat(
    const jyppx_ocv_mat* essential,
    jyppx_ocv_mat* r1,
    jyppx_ocv_mat* r2,
    jyppx_ocv_mat* t)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_decompose_essential_mat";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (essential == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "essential");
        }

        if (r1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "r1");
        }

        if (r2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "r2");
        }

        if (t == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "t");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::decomposeEssentialMat(
            opencv_csharp_native::mat_value(essential),
            opencv_csharp_native::mat_value(r1),
            opencv_csharp_native::mat_value(r2),
            opencv_csharp_native::mat_value(t));
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

int jyppx_ocv_calib3d_recover_pose(
    const jyppx_ocv_mat* essential,
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    jyppx_ocv_mat* mask,
    int* inlier_count)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_recover_pose";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, essential, "essential");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points1, "points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points2, "points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (inlier_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "inlier_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *inlier_count = cv::recoverPose(
            opencv_csharp_native::mat_value(essential),
            opencv_csharp_native::mat_value(points1),
            opencv_csharp_native::mat_value(points2),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            input_output_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask;
        *inlier_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_recover_pose_focal(
    const jyppx_ocv_mat* essential,
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    double focal,
    double pp_x,
    double pp_y,
    jyppx_ocv_mat* mask,
    int* inlier_count)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_recover_pose_focal";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, essential, "essential");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points1, "points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points2, "points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (inlier_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "inlier_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *inlier_count = cv::recoverPose(
            opencv_csharp_native::mat_value(essential),
            opencv_csharp_native::mat_value(points1),
            opencv_csharp_native::mat_value(points2),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            focal,
            cv::Point2d(pp_x, pp_y),
            input_output_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)focal;
        (void)pp_x;
        (void)pp_y;
        (void)mask;
        *inlier_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_recover_pose_with_distance(
    const jyppx_ocv_mat* essential,
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    double distance_thresh,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat* triangulated_points,
    int* inlier_count)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_recover_pose_with_distance";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, essential, "essential");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points1, "points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points2, "points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (inlier_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "inlier_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *inlier_count = cv::recoverPose(
            opencv_csharp_native::mat_value(essential),
            opencv_csharp_native::mat_value(points1),
            opencv_csharp_native::mat_value(points2),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            distance_thresh,
            input_output_or_no_array(mask),
            output_or_no_array(triangulated_points));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)distance_thresh;
        (void)mask;
        (void)triangulated_points;
        *inlier_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_recover_pose_two_cameras(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* camera_matrix1,
    const jyppx_ocv_mat* dist_coeffs1,
    const jyppx_ocv_mat* camera_matrix2,
    const jyppx_ocv_mat* dist_coeffs2,
    jyppx_ocv_mat* essential,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    int method,
    double prob,
    double threshold,
    jyppx_ocv_mat* mask,
    int* inlier_count)
{
    constexpr const char* api_name =
        "jyppx_ocv_calib3d_recover_pose_two_cameras";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, points1, "points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points2, "points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix1, "camera_matrix1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs1, "dist_coeffs1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix2, "camera_matrix2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs2, "dist_coeffs2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, essential, "essential");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (inlier_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "inlier_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *inlier_count = cv::recoverPose(
            opencv_csharp_native::mat_value(points1),
            opencv_csharp_native::mat_value(points2),
            opencv_csharp_native::mat_value(camera_matrix1),
            opencv_csharp_native::mat_value(dist_coeffs1),
            opencv_csharp_native::mat_value(camera_matrix2),
            opencv_csharp_native::mat_value(dist_coeffs2),
            opencv_csharp_native::mat_value(essential),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            method,
            prob,
            threshold,
            input_output_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)method;
        (void)prob;
        (void)threshold;
        (void)mask;
        *inlier_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_compute_correspond_epilines(
    const jyppx_ocv_mat* points,
    int which_image,
    const jyppx_ocv_mat* fundamental,
    jyppx_ocv_mat* lines)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_compute_correspond_epilines";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points");
        }

        if (fundamental == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "fundamental");
        }

        if (lines == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "lines");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::computeCorrespondEpilines(
            opencv_csharp_native::mat_value(points),
            which_image,
            opencv_csharp_native::mat_value(fundamental),
            opencv_csharp_native::mat_value(lines));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)which_image;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_estimate_translation_3d(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* translation,
    jyppx_ocv_mat* inliers,
    double ransac_threshold,
    double confidence,
    int* found)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_estimate_translation_3d";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, source, "source");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, destination, "destination");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, translation, "translation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (inliers != nullptr)
        {
            status = validate_output_mat(api_name, inliers, "inliers");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
        if (found == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "found");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *found = cv::estimateTranslation3D(
            opencv_csharp_native::mat_value(source),
            opencv_csharp_native::mat_value(destination),
            opencv_csharp_native::mat_value(translation),
            output_or_no_array(inliers),
            ransac_threshold,
            confidence)
            ? 1
            : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ransac_threshold;
        (void)confidence;
        *found = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_estimate_translation_2d(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* inliers,
    int method,
    double ransac_reproj_threshold,
    int max_iters,
    double confidence,
    int refine_iters,
    double* translation_x,
    double* translation_y)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_estimate_translation_2d";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, source, "source");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, destination, "destination");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (inliers != nullptr)
        {
            status = validate_output_mat(api_name, inliers, "inliers");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
        if (max_iters <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "max_iters");
        }
        if (refine_iters < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "refine_iters");
        }
        if (translation_x == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "translation_x");
        }
        if (translation_y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "translation_y");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Vec2d translation = cv::estimateTranslation2D(
            opencv_csharp_native::mat_value(source),
            opencv_csharp_native::mat_value(destination),
            output_or_no_array(inliers),
            method,
            ransac_reproj_threshold,
            static_cast<std::size_t>(max_iters),
            confidence,
            static_cast<std::size_t>(refine_iters));
        *translation_x = translation[0];
        *translation_y = translation[1];
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)method;
        (void)ransac_reproj_threshold;
        (void)confidence;
        *translation_x = 0.0;
        *translation_y = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_estimate_affine_3d_ransac(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* transform,
    jyppx_ocv_mat* inliers,
    double ransac_threshold,
    double confidence,
    int* found)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_estimate_affine_3d_ransac";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, source, "source");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, destination, "destination");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, transform, "transform");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (inliers != nullptr)
        {
            status = validate_output_mat(api_name, inliers, "inliers");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
        if (found == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "found");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *found = cv::estimateAffine3D(
            opencv_csharp_native::mat_value(source),
            opencv_csharp_native::mat_value(destination),
            opencv_csharp_native::mat_value(transform),
            output_or_no_array(inliers),
            ransac_threshold,
            confidence)
            ? 1
            : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)ransac_threshold;
        (void)confidence;
        *found = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_estimate_affine_3d_umeyama(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* transform,
    int force_rotation,
    double* scale)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_estimate_affine_3d_umeyama";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, source, "source");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, destination, "destination");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, transform, "transform");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (scale == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "scale");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        opencv_csharp_native::mat_value(transform) = cv::estimateAffine3D(
            opencv_csharp_native::mat_value(source),
            opencv_csharp_native::mat_value(destination),
            scale,
            force_rotation != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)force_rotation;
        *scale = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_estimate_affine_2d(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* transform,
    jyppx_ocv_mat* inliers,
    int method,
    double ransac_reproj_threshold,
    int max_iters,
    double confidence,
    int refine_iters)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_estimate_affine_2d";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, source, "source");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, destination, "destination");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, transform, "transform");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (inliers != nullptr)
        {
            status = validate_output_mat(api_name, inliers, "inliers");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
        if (max_iters <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "max_iters");
        }
        if (refine_iters < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "refine_iters");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        opencv_csharp_native::mat_value(transform) = cv::estimateAffine2D(
            opencv_csharp_native::mat_value(source),
            opencv_csharp_native::mat_value(destination),
            output_or_no_array(inliers),
            method,
            ransac_reproj_threshold,
            static_cast<std::size_t>(max_iters),
            confidence,
            static_cast<std::size_t>(refine_iters));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)method;
        (void)ransac_reproj_threshold;
        (void)confidence;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_estimate_affine_partial_2d(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* transform,
    jyppx_ocv_mat* inliers,
    int method,
    double ransac_reproj_threshold,
    int max_iters,
    double confidence,
    int refine_iters)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_estimate_affine_partial_2d";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, source, "source");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, destination, "destination");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, transform, "transform");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (inliers != nullptr)
        {
            status = validate_output_mat(api_name, inliers, "inliers");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }
        if (max_iters <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "max_iters");
        }
        if (refine_iters < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "refine_iters");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        opencv_csharp_native::mat_value(transform) = cv::estimateAffinePartial2D(
            opencv_csharp_native::mat_value(source),
            opencv_csharp_native::mat_value(destination),
            output_or_no_array(inliers),
            method,
            ransac_reproj_threshold,
            static_cast<std::size_t>(max_iters),
            confidence,
            static_cast<std::size_t>(refine_iters));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)method;
        (void)ransac_reproj_threshold;
        (void)confidence;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_decompose_homography_mat(
    const jyppx_ocv_mat* homography,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* const* rotations,
    jyppx_ocv_mat* const* translations,
    jyppx_ocv_mat* const* normals,
    int output_capacity,
    int* solution_count)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_decompose_homography_mat";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, homography, "homography");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat_array(api_name, rotations, output_capacity, 4, "rotations");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat_array(api_name, translations, output_capacity, 4, "translations");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat_array(api_name, normals, output_capacity, 4, "normals");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (solution_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "solution_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_rotations;
        std::vector<cv::Mat> native_translations;
        std::vector<cv::Mat> native_normals;
        const int count = cv::decomposeHomographyMat(
            opencv_csharp_native::mat_value(homography),
            opencv_csharp_native::mat_value(camera_matrix),
            native_rotations,
            native_translations,
            native_normals);
        if (count < 0 ||
            count > output_capacity ||
            native_rotations.size() != static_cast<std::size_t>(count) ||
            native_translations.size() != static_cast<std::size_t>(count) ||
            native_normals.size() != static_cast<std::size_t>(count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output_capacity");
        }

        for (int i = 0; i < count; ++i)
        {
            const std::size_t index = static_cast<std::size_t>(i);
            opencv_csharp_native::mat_value(rotations[i]) = native_rotations[index];
            opencv_csharp_native::mat_value(translations[i]) = native_translations[index];
            opencv_csharp_native::mat_value(normals[i]) = native_normals[index];
        }
        *solution_count = count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *solution_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_filter_homography_decomp_by_visible_refpoints(
    const jyppx_ocv_mat* const* rotations,
    int rotation_count,
    const jyppx_ocv_mat* const* normals,
    int normal_count,
    const jyppx_ocv_mat* before_points,
    const jyppx_ocv_mat* after_points,
    jyppx_ocv_mat* possible_solutions,
    const jyppx_ocv_mat* points_mask)
{
    constexpr const char* api_name =
        "jyppx_ocv_calib3d_filter_homography_decomp_by_visible_refpoints";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat_array(api_name, rotations, rotation_count, 1, "rotations");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat_array(api_name, normals, normal_count, 1, "normals");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (rotation_count != normal_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "normal_count");
        }
        status = validate_input_mat(api_name, before_points, "before_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, after_points, "after_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, possible_solutions, "possible_solutions");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<cv::Mat> native_rotations =
            to_mat_vector(rotations, rotation_count);
        const std::vector<cv::Mat> native_normals =
            to_mat_vector(normals, normal_count);
        cv::Mat native_possible_solutions;
        cv::filterHomographyDecompByVisibleRefpoints(
            native_rotations,
            native_normals,
            opencv_csharp_native::mat_value(before_points),
            opencv_csharp_native::mat_value(after_points),
            native_possible_solutions,
            input_or_no_array(points_mask));
        native_possible_solutions.copyTo(
            opencv_csharp_native::mat_value(possible_solutions));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)points_mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_convert_points_to_homogeneous(
    const jyppx_ocv_mat* source,
    jyppx_ocv_mat* destination,
    int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_convert_points_to_homogeneous";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, source, "source");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, destination, "destination");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::convertPointsToHomogeneous(
            opencv_csharp_native::mat_value(source),
            opencv_csharp_native::mat_value(destination),
            dtype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_convert_points_from_homogeneous(
    const jyppx_ocv_mat* source,
    jyppx_ocv_mat* destination,
    int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_convert_points_from_homogeneous";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, source, "source");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, destination, "destination");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::convertPointsFromHomogeneous(
            opencv_csharp_native::mat_value(source),
            opencv_csharp_native::mat_value(destination),
            dtype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_correct_matches(
    const jyppx_ocv_mat* fundamental,
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    jyppx_ocv_mat* corrected_points1,
    jyppx_ocv_mat* corrected_points2)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_correct_matches";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, fundamental, "fundamental");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points1, "points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points2, "points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, corrected_points1, "corrected_points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, corrected_points2, "corrected_points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::correctMatches(
            opencv_csharp_native::mat_value(fundamental),
            opencv_csharp_native::mat_value(points1),
            opencv_csharp_native::mat_value(points2),
            opencv_csharp_native::mat_value(corrected_points1),
            opencv_csharp_native::mat_value(corrected_points2));
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

int jyppx_ocv_calib3d_sampson_distance(
    const jyppx_ocv_mat* point1,
    const jyppx_ocv_mat* point2,
    const jyppx_ocv_mat* fundamental,
    double* distance)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_sampson_distance";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, point1, "point1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, point2, "point2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, fundamental, "fundamental");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (distance == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "distance");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *distance = cv::sampsonDistance(
            opencv_csharp_native::mat_value(point1),
            opencv_csharp_native::mat_value(point2),
            opencv_csharp_native::mat_value(fundamental));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *distance = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_triangulate_points(
    const jyppx_ocv_mat* proj_matr1,
    const jyppx_ocv_mat* proj_matr2,
    const jyppx_ocv_mat* proj_points1,
    const jyppx_ocv_mat* proj_points2,
    jyppx_ocv_mat* points4d)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_triangulate_points";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (proj_matr1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "proj_matr1");
        }

        if (proj_matr2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "proj_matr2");
        }

        if (proj_points1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "proj_points1");
        }

        if (proj_points2 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "proj_points2");
        }

        if (points4d == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points4d");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::triangulatePoints(
            opencv_csharp_native::mat_value(proj_matr1),
            opencv_csharp_native::mat_value(proj_matr2),
            opencv_csharp_native::mat_value(proj_points1),
            opencv_csharp_native::mat_value(proj_points2),
            opencv_csharp_native::mat_value(points4d));
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

int jyppx_ocv_calib3d_undistort_points(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_mat* r,
    const jyppx_ocv_mat* p,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_undistort_points";

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

        if (camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_matrix");
        }

        if (dist_coeffs == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dist_coeffs");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::undistortPoints(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            input_or_no_array(r),
            input_or_no_array(p),
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)r;
        (void)p;
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

int jyppx_ocv_calib3d_undistort_image_points(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_undistort_image_points";

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

        if (camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_matrix");
        }

        if (dist_coeffs == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dist_coeffs");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::undistortImagePoints(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
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

int jyppx_ocv_calib3d_filter_speckles(
    jyppx_ocv_mat* image,
    double new_value,
    int max_speckle_size,
    double max_difference,
    jyppx_ocv_mat* buffer)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_filter_speckles";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (image == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::filterSpeckles(
            opencv_csharp_native::mat_value(image),
            new_value,
            max_speckle_size,
            max_difference,
            input_output_or_no_array(buffer));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)new_value;
        (void)max_speckle_size;
        (void)max_difference;
        (void)buffer;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_get_valid_disparity_roi(
    int roi1_x,
    int roi1_y,
    int roi1_width,
    int roi1_height,
    int roi2_x,
    int roi2_y,
    int roi2_width,
    int roi2_height,
    int min_disparity,
    int number_of_disparities,
    int block_size,
    int* result_x,
    int* result_y,
    int* result_width,
    int* result_height)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_get_valid_disparity_roi";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (result_x == nullptr ||
            result_y == nullptr ||
            result_width == nullptr ||
            result_height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(
                api_name,
                result_x == nullptr
                    ? "result_x"
                    : (result_y == nullptr
                        ? "result_y"
                        : (result_width == nullptr ? "result_width" : "result_height")));
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Rect result = cv::getValidDisparityROI(
            cv::Rect(roi1_x, roi1_y, roi1_width, roi1_height),
            cv::Rect(roi2_x, roi2_y, roi2_width, roi2_height),
            min_disparity,
            number_of_disparities,
            block_size);
        write_rect(result, result_x, result_y, result_width, result_height);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)roi1_x;
        (void)roi1_y;
        (void)roi1_width;
        (void)roi1_height;
        (void)roi2_x;
        (void)roi2_y;
        (void)roi2_width;
        (void)roi2_height;
        (void)min_disparity;
        (void)number_of_disparities;
        (void)block_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_validate_disparity(
    jyppx_ocv_mat* disparity,
    const jyppx_ocv_mat* cost,
    int min_disparity,
    int number_of_disparities,
    int disp12_max_difference)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_validate_disparity";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (disparity == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "disparity");
        }

        if (cost == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "cost");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::validateDisparity(
            opencv_csharp_native::mat_value(disparity),
            opencv_csharp_native::mat_value(cost),
            min_disparity,
            number_of_disparities,
            disp12_max_difference);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)min_disparity;
        (void)number_of_disparities;
        (void)disp12_max_difference;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_reproject_image_to_3d(
    const jyppx_ocv_mat* disparity,
    jyppx_ocv_mat* image3d,
    const jyppx_ocv_mat* q,
    int handle_missing_values,
    int ddepth)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_reproject_image_to_3d";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (disparity == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "disparity");
        }

        if (image3d == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image3d");
        }

        if (q == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "q");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::reprojectImageTo3D(
            opencv_csharp_native::mat_value(disparity),
            opencv_csharp_native::mat_value(image3d),
            opencv_csharp_native::mat_value(q),
            handle_missing_values != 0,
            ddepth);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)handle_missing_values;
        (void)ddepth;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_init_undistort_rectify_map(
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
    constexpr const char* api_name = "jyppx_ocv_calib3d_init_undistort_rectify_map";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "camera_matrix");
        }

        if (dist_coeffs == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dist_coeffs");
        }

        if (r == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "r");
        }

        if (new_camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "new_camera_matrix");
        }

        if (size_width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "size_width");
        }

        if (size_height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "size_height");
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
        cv::initUndistortRectifyMap(
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

int jyppx_ocv_calib3d_stereo_rectify(
    const jyppx_ocv_mat* camera_matrix1,
    const jyppx_ocv_mat* dist_coeffs1,
    const jyppx_ocv_mat* camera_matrix2,
    const jyppx_ocv_mat* dist_coeffs2,
    int image_width,
    int image_height,
    const jyppx_ocv_mat* r,
    const jyppx_ocv_mat* t,
    jyppx_ocv_mat* r1,
    jyppx_ocv_mat* r2,
    jyppx_ocv_mat* p1,
    jyppx_ocv_mat* p2,
    jyppx_ocv_mat* q,
    int flags,
    double alpha,
    int new_image_width,
    int new_image_height,
    int* roi1_x,
    int* roi1_y,
    int* roi1_width,
    int* roi1_height,
    int* roi2_x,
    int* roi2_y,
    int* roi2_width,
    int* roi2_height)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_stereo_rectify";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, camera_matrix1, "camera_matrix1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs1, "dist_coeffs1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix2, "camera_matrix2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs2, "dist_coeffs2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r1, "r1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r2, "r2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, p1, "p1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, p2, "p2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, q, "q");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (image_width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_width");
        }

        if (image_height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_height");
        }

        if (new_image_width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "new_image_width");
        }

        if (new_image_height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "new_image_height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Rect roi1;
        cv::Rect roi2;
        cv::stereoRectify(
            opencv_csharp_native::mat_value(camera_matrix1),
            opencv_csharp_native::mat_value(dist_coeffs1),
            opencv_csharp_native::mat_value(camera_matrix2),
            opencv_csharp_native::mat_value(dist_coeffs2),
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            opencv_csharp_native::mat_value(r1),
            opencv_csharp_native::mat_value(r2),
            opencv_csharp_native::mat_value(p1),
            opencv_csharp_native::mat_value(p2),
            opencv_csharp_native::mat_value(q),
            flags,
            alpha,
            cv::Size(new_image_width, new_image_height),
            &roi1,
            &roi2);

        write_rect(roi1, roi1_x, roi1_y, roi1_width, roi1_height);
        write_rect(roi2, roi2_x, roi2_y, roi2_width, roi2_height);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        (void)alpha;
        write_rect_values(0, 0, 0, 0, roi1_x, roi1_y, roi1_width, roi1_height);
        write_rect_values(0, 0, 0, 0, roi2_x, roi2_y, roi2_width, roi2_height);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_solve_pnp_generic(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    int use_extrinsic_guess,
    int flags,
    const jyppx_ocv_mat* rvec,
    const jyppx_ocv_mat* tvec,
    jyppx_ocv_mat* reprojection_error,
    int* solution_count)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_solve_pnp_generic";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, object_points, "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, image_points, "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvecs, "rvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvecs, "tvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (solution_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "solution_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_rvecs;
        std::vector<cv::Mat> native_tvecs;
        int count = cv::solvePnPGeneric(
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(image_points),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            native_rvecs,
            native_tvecs,
            use_extrinsic_guess != 0,
            flags,
            input_or_no_array(rvec),
            input_or_no_array(tvec),
            output_or_no_array(reprojection_error));

        pack_pose_vectors(native_rvecs, opencv_csharp_native::mat_value(rvecs));
        pack_pose_vectors(native_tvecs, opencv_csharp_native::mat_value(tvecs));
        *solution_count = count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)use_extrinsic_guess;
        (void)flags;
        (void)rvec;
        (void)tvec;
        (void)reprojection_error;
        *solution_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_solve_p3p(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    int flags,
    int* solution_count)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_solve_p3p";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, object_points, "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, image_points, "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvecs, "rvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvecs, "tvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (solution_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "solution_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> native_rvecs;
        std::vector<cv::Mat> native_tvecs;
        int count = cv::solveP3P(
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(image_points),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            native_rvecs,
            native_tvecs,
            flags);

        pack_pose_vectors(native_rvecs, opencv_csharp_native::mat_value(rvecs));
        pack_pose_vectors(native_tvecs, opencv_csharp_native::mat_value(tvecs));
        *solution_count = count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        *solution_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_mat_mul_deriv(
    const jyppx_ocv_mat* a,
    const jyppx_ocv_mat* b,
    jyppx_ocv_mat* d_ab_d_a,
    jyppx_ocv_mat* d_ab_d_b)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_mat_mul_deriv";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, a, "a");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, b, "b");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, d_ab_d_a, "d_ab_d_a");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, d_ab_d_b, "d_ab_d_b");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::matMulDeriv(
            opencv_csharp_native::mat_value(a),
            opencv_csharp_native::mat_value(b),
            opencv_csharp_native::mat_value(d_ab_d_a),
            opencv_csharp_native::mat_value(d_ab_d_b));
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

int jyppx_ocv_calib3d_solve_pnp_refine_lm(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_solve_pnp_refine_lm";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, object_points, "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, image_points, "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvec, "rvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvec, "tvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::solvePnPRefineLM(
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(image_points),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
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

int jyppx_ocv_calib3d_solve_pnp_refine_vvs(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double vvs_lambda)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_solve_pnp_refine_vvs";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, object_points, "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, image_points, "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvec, "rvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvec, "tvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::solvePnPRefineVVS(
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(image_points),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon),
            vvs_lambda);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)criteria_type;
        (void)criteria_max_count;
        (void)criteria_epsilon;
        (void)vvs_lambda;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_find_chessboard_corners(
    const jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    jyppx_ocv_mat* corners,
    int flags,
    int* found)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_find_chessboard_corners";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, corners, "corners");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (pattern_width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern_width");
        }

        if (pattern_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern_height");
        }

        if (found == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "found");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        bool result = cv::findChessboardCorners(
            opencv_csharp_native::mat_value(image),
            cv::Size(pattern_width, pattern_height),
            opencv_csharp_native::mat_value(corners),
            flags);
        *found = result ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        *found = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_check_chessboard(
    const jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    int* found)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_check_chessboard";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (pattern_width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern_width");
        }

        if (pattern_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern_height");
        }

        if (found == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "found");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        bool result = cv::checkChessboard(
            opencv_csharp_native::mat_value(image),
            cv::Size(pattern_width, pattern_height));
        *found = result ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *found = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_find_circles_grid(
    const jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    jyppx_ocv_mat* centers,
    int flags,
    int* found)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_find_circles_grid";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, centers, "centers");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (pattern_width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern_width");
        }

        if (pattern_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern_height");
        }

        if (found == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "found");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        bool result = cv::findCirclesGrid(
            opencv_csharp_native::mat_value(image),
            cv::Size(pattern_width, pattern_height),
            opencv_csharp_native::mat_value(centers),
            flags);
        *found = result ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        *found = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_draw_chessboard_corners(
    jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    const jyppx_ocv_mat* corners,
    int pattern_was_found)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_draw_chessboard_corners";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, corners, "corners");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (pattern_width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern_width");
        }

        if (pattern_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pattern_height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::drawChessboardCorners(
            opencv_csharp_native::mat_value(image),
            cv::Size(pattern_width, pattern_height),
            opencv_csharp_native::mat_value(corners),
            pattern_was_found != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pattern_was_found;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_get_default_new_camera_matrix(
    const jyppx_ocv_mat* camera_matrix,
    int image_width,
    int image_height,
    int center_principal_point,
    jyppx_ocv_mat* new_camera_matrix)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_get_default_new_camera_matrix";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, new_camera_matrix, "new_camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (center_principal_point != 0 && image_width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_width");
        }
        if (center_principal_point != 0 && image_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Mat value = cv::getDefaultNewCameraMatrix(
            opencv_csharp_native::mat_value(camera_matrix),
            cv::Size(image_width, image_height),
            center_principal_point != 0);
        value.copyTo(opencv_csharp_native::mat_value(new_camera_matrix));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image_width;
        (void)image_height;
        (void)center_principal_point;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_get_undistort_rectangles(
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_mat* r,
    const jyppx_ocv_mat* new_camera_matrix,
    int image_width,
    int image_height,
    double* inner_x,
    double* inner_y,
    double* inner_width,
    double* inner_height,
    double* outer_x,
    double* outer_y,
    double* outer_width,
    double* outer_height)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_get_undistort_rectangles";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (image_width <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_width");
        }
        if (image_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_height");
        }
        if (inner_x == nullptr || inner_y == nullptr ||
            inner_width == nullptr || inner_height == nullptr ||
            outer_x == nullptr || outer_y == nullptr ||
            outer_width == nullptr || outer_height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Rect2d inner;
        cv::Rect2d outer;
        cv::getUndistortRectangles(
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            input_or_no_array(r),
            input_or_no_array(new_camera_matrix),
            cv::Size(image_width, image_height),
            inner,
            outer);
        write_rect2d_values(
            inner.x,
            inner.y,
            inner.width,
            inner.height,
            inner_x,
            inner_y,
            inner_width,
            inner_height);
        write_rect2d_values(
            outer.x,
            outer.y,
            outer.width,
            outer.height,
            outer_x,
            outer_y,
            outer_width,
            outer_height);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)r;
        (void)new_camera_matrix;
        write_rect2d_values(0.0, 0.0, 0.0, 0.0, inner_x, inner_y, inner_width, inner_height);
        write_rect2d_values(0.0, 0.0, 0.0, 0.0, outer_x, outer_y, outer_width, outer_height);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_get_optimal_new_camera_matrix(
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    int image_width,
    int image_height,
    double alpha,
    int new_image_width,
    int new_image_height,
    int center_principal_point,
    int* roi_x,
    int* roi_y,
    int* roi_width,
    int* roi_height,
    jyppx_ocv_mat** new_camera_matrix)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_get_optimal_new_camera_matrix";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (image_width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_width");
        }

        if (image_height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_height");
        }

        if (new_image_width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "new_image_width");
        }

        if (new_image_height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "new_image_height");
        }

        if (new_camera_matrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "new_camera_matrix");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Rect roi;
        cv::Mat value = cv::getOptimalNewCameraMatrix(
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            cv::Size(image_width, image_height),
            alpha,
            cv::Size(new_image_width, new_image_height),
            &roi,
            center_principal_point != 0);

        write_rect(roi, roi_x, roi_y, roi_width, roi_height);
        return assign_new_mat(api_name, value, new_camera_matrix);
#else
        (void)alpha;
        (void)center_principal_point;
        write_rect_values(0, 0, 0, 0, roi_x, roi_y, roi_width, roi_height);
        *new_camera_matrix = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_calibration_matrix_values(
    const jyppx_ocv_mat* camera_matrix,
    int image_width,
    int image_height,
    double aperture_width,
    double aperture_height,
    double* fov_x,
    double* fov_y,
    double* focal_length,
    double* principal_point_x,
    double* principal_point_y,
    double* aspect_ratio)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_calibration_matrix_values";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (image_width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_width");
        }

        if (image_height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_height");
        }

        if (fov_x == nullptr || fov_y == nullptr || focal_length == nullptr ||
            principal_point_x == nullptr || principal_point_y == nullptr || aspect_ratio == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::Point2d principal_point;
        cv::calibrationMatrixValues(
            opencv_csharp_native::mat_value(camera_matrix),
            cv::Size(image_width, image_height),
            aperture_width,
            aperture_height,
            *fov_x,
            *fov_y,
            *focal_length,
            principal_point,
            *aspect_ratio);

        *principal_point_x = principal_point.x;
        *principal_point_y = principal_point.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)aperture_width;
        (void)aperture_height;
        *fov_x = 0.0;
        *fov_y = 0.0;
        *focal_length = 0.0;
        *principal_point_x = 0.0;
        *principal_point_y = 0.0;
        *aspect_ratio = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_stereo_rectify_uncalibrated(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* fundamental,
    int image_width,
    int image_height,
    jyppx_ocv_mat* h1,
    jyppx_ocv_mat* h2,
    double threshold,
    int* success)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_stereo_rectify_uncalibrated";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, points1, "points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, points2, "points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, fundamental, "fundamental");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, h1, "h1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, h2, "h2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (image_width < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_width");
        }

        if (image_height < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_height");
        }

        if (success == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "success");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        bool result = cv::stereoRectifyUncalibrated(
            opencv_csharp_native::mat_value(points1),
            opencv_csharp_native::mat_value(points2),
            opencv_csharp_native::mat_value(fundamental),
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(h1),
            opencv_csharp_native::mat_value(h2),
            threshold);
        *success = result ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)threshold;
        *success = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_calibrate_hand_eye(
    const jyppx_ocv_mat* const* r_gripper2base,
    const jyppx_ocv_mat* const* t_gripper2base,
    const jyppx_ocv_mat* const* r_target2cam,
    const jyppx_ocv_mat* const* t_target2cam,
    int pose_count,
    jyppx_ocv_mat* r_cam2gripper,
    jyppx_ocv_mat* t_cam2gripper,
    int method)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_calibrate_hand_eye";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat_array(api_name, r_gripper2base, pose_count, 3, "r_gripper2base");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat_array(api_name, t_gripper2base, pose_count, 3, "t_gripper2base");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat_array(api_name, r_target2cam, pose_count, 3, "r_target2cam");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat_array(api_name, t_target2cam, pose_count, 3, "t_target2cam");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r_cam2gripper, "r_cam2gripper");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t_cam2gripper, "t_cam2gripper");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (method < 0 || method > 4)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "method");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::calibrateHandEye(
            to_mat_vector(r_gripper2base, pose_count),
            to_mat_vector(t_gripper2base, pose_count),
            to_mat_vector(r_target2cam, pose_count),
            to_mat_vector(t_target2cam, pose_count),
            opencv_csharp_native::mat_value(r_cam2gripper),
            opencv_csharp_native::mat_value(t_cam2gripper),
            static_cast<cv::HandEyeCalibrationMethod>(method));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)method;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_calibrate_robot_world_hand_eye(
    const jyppx_ocv_mat* const* r_world2cam,
    const jyppx_ocv_mat* const* t_world2cam,
    const jyppx_ocv_mat* const* r_base2gripper,
    const jyppx_ocv_mat* const* t_base2gripper,
    int pose_count,
    jyppx_ocv_mat* r_base2world,
    jyppx_ocv_mat* t_base2world,
    jyppx_ocv_mat* r_gripper2cam,
    jyppx_ocv_mat* t_gripper2cam,
    int method)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_calibrate_robot_world_hand_eye";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat_array(api_name, r_world2cam, pose_count, 3, "r_world2cam");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat_array(api_name, t_world2cam, pose_count, 3, "t_world2cam");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat_array(api_name, r_base2gripper, pose_count, 3, "r_base2gripper");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat_array(api_name, t_base2gripper, pose_count, 3, "t_base2gripper");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r_base2world, "r_base2world");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t_base2world, "t_base2world");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r_gripper2cam, "r_gripper2cam");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t_gripper2cam, "t_gripper2cam");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (method < 0 || method > 1)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "method");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::calibrateRobotWorldHandEye(
            to_mat_vector(r_world2cam, pose_count),
            to_mat_vector(t_world2cam, pose_count),
            to_mat_vector(r_base2gripper, pose_count),
            to_mat_vector(t_base2gripper, pose_count),
            opencv_csharp_native::mat_value(r_base2world),
            opencv_csharp_native::mat_value(t_base2world),
            opencv_csharp_native::mat_value(r_gripper2cam),
            opencv_csharp_native::mat_value(t_gripper2cam),
            static_cast<cv::RobotWorldHandEyeCalibrationMethod>(method));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)method;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_init_camera_matrix_2d(
    const int* object_point_offsets,
    int object_point_group_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point_offsets,
    int image_point_group_count,
    const jyppx_ocv_calib3d_point2f* image_points,
    int image_point_count,
    int image_width,
    int image_height,
    double aspect_ratio,
    jyppx_ocv_mat* camera_matrix)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_init_camera_matrix_2d";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_width <= 0 || image_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, image_width <= 0 ? "image_width" : "image_height");
        }
        if (!std::isfinite(aspect_ratio))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "aspect_ratio");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_calibration_point_groups(
            api_name,
            object_point_offsets,
            object_point_group_count,
            object_points,
            object_point_count,
            image_point_offsets,
            image_point_group_count,
            image_points,
            image_point_count,
            "image_point_offsets",
            "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_planar_object_points(api_name, object_points, object_point_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        // The public contract treats every non-positive value as unconstrained,
        // while some upstream implementations only special-case zero.
        const double effective_aspect_ratio = aspect_ratio > 0.0 ? aspect_ratio : 0.0;
        cv::Mat initialized = cv::initCameraMatrix2D(
            to_point3f_groups(object_point_offsets, object_point_group_count, object_points),
            to_point2f_groups(image_point_offsets, image_point_group_count, image_points),
            cv::Size(image_width, image_height),
            effective_aspect_ratio);
        initialized.copyTo(opencv_csharp_native::mat_value(camera_matrix));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_point_group_count;
        (void)object_points;
        (void)object_point_count;
        (void)image_point_offsets;
        (void)image_point_group_count;
        (void)image_points;
        (void)image_point_count;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_calibrate_camera(
    const int* object_point_offsets,
    int object_point_group_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point_offsets,
    int image_point_group_count,
    const jyppx_ocv_calib3d_point2f* image_points,
    int image_point_count,
    int image_width,
    int image_height,
    jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_calibrate_camera";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvecs, "rvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvecs, "tvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_calibration_point_groups(
            api_name,
            object_point_offsets,
            object_point_group_count,
            object_points,
            object_point_count,
            image_point_offsets,
            image_point_group_count,
            image_points,
            image_point_count,
            "image_point_offsets",
            "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        std::vector<std::vector<cv::Point3f>> native_object_points = to_point3f_groups(object_point_offsets, object_point_group_count, object_points);
        std::vector<std::vector<cv::Point2f>> native_image_points = to_point2f_groups(image_point_offsets, image_point_group_count, image_points);
        std::vector<cv::Mat> native_rvecs;
        std::vector<cv::Mat> native_tvecs;
        *reprojection_error = cv::calibrateCamera(
            native_object_points,
            native_image_points,
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            native_rvecs,
            native_tvecs,
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));

        pack_pose_vectors(native_rvecs, opencv_csharp_native::mat_value(rvecs));
        pack_pose_vectors(native_tvecs, opencv_csharp_native::mat_value(tvecs));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_point_group_count;
        (void)object_points;
        (void)object_point_count;
        (void)image_point_offsets;
        (void)image_point_group_count;
        (void)image_points;
        (void)image_point_count;
        (void)image_width;
        (void)image_height;
        (void)flags;
        (void)criteria_type;
        (void)criteria_max_count;
        (void)criteria_epsilon;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_calibrate_camera_extended(
    const int* object_point_offsets,
    int object_point_group_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point_offsets,
    int image_point_group_count,
    const jyppx_ocv_calib3d_point2f* image_points,
    int image_point_count,
    int image_width,
    int image_height,
    jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    jyppx_ocv_mat* std_deviations_intrinsics,
    jyppx_ocv_mat* std_deviations_extrinsics,
    jyppx_ocv_mat* per_view_errors,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_calibrate_camera_extended";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvecs, "rvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvecs, "tvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_calibration_point_groups(
            api_name,
            object_point_offsets,
            object_point_group_count,
            object_points,
            object_point_count,
            image_point_offsets,
            image_point_group_count,
            image_points,
            image_point_count,
            "image_point_offsets",
            "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        std::vector<std::vector<cv::Point3f>> native_object_points = to_point3f_groups(object_point_offsets, object_point_group_count, object_points);
        std::vector<std::vector<cv::Point2f>> native_image_points = to_point2f_groups(image_point_offsets, image_point_group_count, image_points);
        std::vector<cv::Mat> native_rvecs;
        std::vector<cv::Mat> native_tvecs;
        *reprojection_error = cv::calibrateCamera(
            native_object_points,
            native_image_points,
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            native_rvecs,
            native_tvecs,
            output_or_no_array(std_deviations_intrinsics),
            output_or_no_array(std_deviations_extrinsics),
            output_or_no_array(per_view_errors),
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));

        pack_pose_vectors(native_rvecs, opencv_csharp_native::mat_value(rvecs));
        pack_pose_vectors(native_tvecs, opencv_csharp_native::mat_value(tvecs));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_point_group_count;
        (void)object_points;
        (void)object_point_count;
        (void)image_point_offsets;
        (void)image_point_group_count;
        (void)image_points;
        (void)image_point_count;
        (void)image_width;
        (void)image_height;
        (void)std_deviations_intrinsics;
        (void)std_deviations_extrinsics;
        (void)per_view_errors;
        (void)flags;
        (void)criteria_type;
        (void)criteria_max_count;
        (void)criteria_epsilon;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_calibrate_camera_ro(
    const int* object_point_offsets,
    int object_point_group_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point_offsets,
    int image_point_group_count,
    const jyppx_ocv_calib3d_point2f* image_points,
    int image_point_count,
    int image_width,
    int image_height,
    int i_fixed_point,
    jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    jyppx_ocv_mat* new_object_points,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_calibrate_camera_ro";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvecs, "rvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvecs, "tvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, new_object_points, "new_object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_width <= 0 || image_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, image_width <= 0 ? "image_width" : "image_height");
        }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_calibration_point_groups(
            api_name,
            object_point_offsets,
            object_point_group_count,
            object_points,
            object_point_count,
            image_point_offsets,
            image_point_group_count,
            image_points,
            image_point_count,
            "image_point_offsets",
            "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        std::vector<std::vector<cv::Point3f>> native_object_points = to_point3f_groups(object_point_offsets, object_point_group_count, object_points);
        std::vector<std::vector<cv::Point2f>> native_image_points = to_point2f_groups(image_point_offsets, image_point_group_count, image_points);
        std::vector<cv::Mat> native_rvecs;
        std::vector<cv::Mat> native_tvecs;
        std::vector<cv::Point3f> native_new_object_points;
        *reprojection_error = cv::calibrateCameraRO(
            native_object_points,
            native_image_points,
            cv::Size(image_width, image_height),
            i_fixed_point,
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            native_rvecs,
            native_tvecs,
            native_new_object_points,
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));

        pack_pose_vectors(native_rvecs, opencv_csharp_native::mat_value(rvecs));
        pack_pose_vectors(native_tvecs, opencv_csharp_native::mat_value(tvecs));
        pack_point3f_values(native_new_object_points, opencv_csharp_native::mat_value(new_object_points));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_point_group_count;
        (void)object_points;
        (void)object_point_count;
        (void)image_point_offsets;
        (void)image_point_group_count;
        (void)image_points;
        (void)image_point_count;
        (void)i_fixed_point;
        (void)flags;
        (void)criteria_type;
        (void)criteria_max_count;
        (void)criteria_epsilon;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_calibrate_camera_ro_extended(
    const int* object_point_offsets,
    int object_point_group_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point_offsets,
    int image_point_group_count,
    const jyppx_ocv_calib3d_point2f* image_points,
    int image_point_count,
    int image_width,
    int image_height,
    int i_fixed_point,
    jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    jyppx_ocv_mat* new_object_points,
    jyppx_ocv_mat* std_deviations_intrinsics,
    jyppx_ocv_mat* std_deviations_extrinsics,
    jyppx_ocv_mat* std_deviations_object_points,
    jyppx_ocv_mat* per_view_errors,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_calibrate_camera_ro_extended";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvecs, "rvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvecs, "tvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, new_object_points, "new_object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, std_deviations_intrinsics, "std_deviations_intrinsics");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, std_deviations_extrinsics, "std_deviations_extrinsics");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, std_deviations_object_points, "std_deviations_object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, per_view_errors, "per_view_errors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_width <= 0 || image_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, image_width <= 0 ? "image_width" : "image_height");
        }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_calibration_point_groups(
            api_name,
            object_point_offsets,
            object_point_group_count,
            object_points,
            object_point_count,
            image_point_offsets,
            image_point_group_count,
            image_points,
            image_point_count,
            "image_point_offsets",
            "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        std::vector<std::vector<cv::Point3f>> native_object_points = to_point3f_groups(object_point_offsets, object_point_group_count, object_points);
        std::vector<std::vector<cv::Point2f>> native_image_points = to_point2f_groups(image_point_offsets, image_point_group_count, image_points);
        std::vector<cv::Mat> native_rvecs;
        std::vector<cv::Mat> native_tvecs;
        std::vector<cv::Point3f> native_new_object_points;
        cv::Mat native_std_deviations_object_points;
        *reprojection_error = cv::calibrateCameraRO(
            native_object_points,
            native_image_points,
            cv::Size(image_width, image_height),
            i_fixed_point,
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            native_rvecs,
            native_tvecs,
            native_new_object_points,
            opencv_csharp_native::mat_value(std_deviations_intrinsics),
            opencv_csharp_native::mat_value(std_deviations_extrinsics),
            native_std_deviations_object_points,
            opencv_csharp_native::mat_value(per_view_errors),
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));

        pack_pose_vectors(native_rvecs, opencv_csharp_native::mat_value(rvecs));
        pack_pose_vectors(native_tvecs, opencv_csharp_native::mat_value(tvecs));
        pack_point3f_values(native_new_object_points, opencv_csharp_native::mat_value(new_object_points));
        if (native_std_deviations_object_points.empty())
        {
            opencv_csharp_native::mat_value(std_deviations_object_points).release();
        }
        else
        {
            native_std_deviations_object_points.copyTo(opencv_csharp_native::mat_value(std_deviations_object_points));
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_point_group_count;
        (void)object_points;
        (void)object_point_count;
        (void)image_point_offsets;
        (void)image_point_group_count;
        (void)image_points;
        (void)image_point_count;
        (void)i_fixed_point;
        (void)std_deviations_intrinsics;
        (void)std_deviations_extrinsics;
        (void)std_deviations_object_points;
        (void)per_view_errors;
        (void)flags;
        (void)criteria_type;
        (void)criteria_max_count;
        (void)criteria_epsilon;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_stereo_calibrate(
    const int* object_point_offsets,
    int object_point_group_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point1_offsets,
    int image_point1_group_count,
    const jyppx_ocv_calib3d_point2f* image_points1,
    int image_point1_count,
    const int* image_point2_offsets,
    int image_point2_group_count,
    const jyppx_ocv_calib3d_point2f* image_points2,
    int image_point2_count,
    jyppx_ocv_mat* camera_matrix1,
    jyppx_ocv_mat* dist_coeffs1,
    jyppx_ocv_mat* camera_matrix2,
    jyppx_ocv_mat* dist_coeffs2,
    int image_width,
    int image_height,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    jyppx_ocv_mat* e,
    jyppx_ocv_mat* f,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_stereo_calibrate";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, camera_matrix1, "camera_matrix1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs1, "dist_coeffs1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, camera_matrix2, "camera_matrix2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs2, "dist_coeffs2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, e, "e");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, f, "f");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_calibration_point_groups(
            api_name,
            object_point_offsets,
            object_point_group_count,
            object_points,
            object_point_count,
            image_point1_offsets,
            image_point1_group_count,
            image_points1,
            image_point1_count,
            "image_point1_offsets",
            "image_points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(api_name, image_point2_offsets, image_point2_group_count, image_points2, image_point2_count, "image_point2_offsets", "image_points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_point2_group_count != object_point_group_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_point2_group_count");
        }

        for (int i = 0; i < object_point_group_count; ++i)
        {
            if ((object_point_offsets[i + 1] - object_point_offsets[i]) != (image_point2_offsets[i + 1] - image_point2_offsets[i]))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "image_point2_group_count");
            }
        }

        std::vector<std::vector<cv::Point3f>> native_object_points = to_point3f_groups(object_point_offsets, object_point_group_count, object_points);
        std::vector<std::vector<cv::Point2f>> native_image_points1 = to_point2f_groups(image_point1_offsets, image_point1_group_count, image_points1);
        std::vector<std::vector<cv::Point2f>> native_image_points2 = to_point2f_groups(image_point2_offsets, image_point2_group_count, image_points2);
        *reprojection_error = cv::stereoCalibrate(
            native_object_points,
            native_image_points1,
            native_image_points2,
            opencv_csharp_native::mat_value(camera_matrix1),
            opencv_csharp_native::mat_value(dist_coeffs1),
            opencv_csharp_native::mat_value(camera_matrix2),
            opencv_csharp_native::mat_value(dist_coeffs2),
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            opencv_csharp_native::mat_value(e),
            opencv_csharp_native::mat_value(f),
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_point_group_count;
        (void)object_points;
        (void)object_point_count;
        (void)image_point1_offsets;
        (void)image_point1_group_count;
        (void)image_points1;
        (void)image_point1_count;
        (void)image_point2_offsets;
        (void)image_point2_group_count;
        (void)image_points2;
        (void)image_point2_count;
        (void)image_width;
        (void)image_height;
        (void)flags;
        (void)criteria_type;
        (void)criteria_max_count;
        (void)criteria_epsilon;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_stereo_calibrate_extended(
    const int* object_point_offsets,
    int object_point_group_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point1_offsets,
    int image_point1_group_count,
    const jyppx_ocv_calib3d_point2f* image_points1,
    int image_point1_count,
    const int* image_point2_offsets,
    int image_point2_group_count,
    const jyppx_ocv_calib3d_point2f* image_points2,
    int image_point2_count,
    jyppx_ocv_mat* camera_matrix1,
    jyppx_ocv_mat* dist_coeffs1,
    jyppx_ocv_mat* camera_matrix2,
    jyppx_ocv_mat* dist_coeffs2,
    int image_width,
    int image_height,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    jyppx_ocv_mat* e,
    jyppx_ocv_mat* f,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    jyppx_ocv_mat* per_view_errors,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_stereo_calibrate_extended";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, camera_matrix1, "camera_matrix1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs1, "dist_coeffs1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, camera_matrix2, "camera_matrix2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs2, "dist_coeffs2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, e, "e");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, f, "f");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvecs, "rvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvecs, "tvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_calibration_point_groups(
            api_name,
            object_point_offsets,
            object_point_group_count,
            object_points,
            object_point_count,
            image_point1_offsets,
            image_point1_group_count,
            image_points1,
            image_point1_count,
            "image_point1_offsets",
            "image_points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(api_name, image_point2_offsets, image_point2_group_count, image_points2, image_point2_count, "image_point2_offsets", "image_points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_point2_group_count != object_point_group_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_point2_group_count");
        }

        for (int i = 0; i < object_point_group_count; ++i)
        {
            if ((object_point_offsets[i + 1] - object_point_offsets[i]) != (image_point2_offsets[i + 1] - image_point2_offsets[i]))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "image_point2_group_count");
            }
        }

        std::vector<std::vector<cv::Point3f>> native_object_points = to_point3f_groups(object_point_offsets, object_point_group_count, object_points);
        std::vector<std::vector<cv::Point2f>> native_image_points1 = to_point2f_groups(image_point1_offsets, image_point1_group_count, image_points1);
        std::vector<std::vector<cv::Point2f>> native_image_points2 = to_point2f_groups(image_point2_offsets, image_point2_group_count, image_points2);
        std::vector<cv::Mat> native_rvecs;
        std::vector<cv::Mat> native_tvecs;
        *reprojection_error = cv::stereoCalibrate(
            native_object_points,
            native_image_points1,
            native_image_points2,
            opencv_csharp_native::mat_value(camera_matrix1),
            opencv_csharp_native::mat_value(dist_coeffs1),
            opencv_csharp_native::mat_value(camera_matrix2),
            opencv_csharp_native::mat_value(dist_coeffs2),
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            opencv_csharp_native::mat_value(e),
            opencv_csharp_native::mat_value(f),
            native_rvecs,
            native_tvecs,
            output_or_no_array(per_view_errors),
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));

        pack_pose_vectors(native_rvecs, opencv_csharp_native::mat_value(rvecs));
        pack_pose_vectors(native_tvecs, opencv_csharp_native::mat_value(tvecs));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_point_group_count;
        (void)object_points;
        (void)object_point_count;
        (void)image_point1_offsets;
        (void)image_point1_group_count;
        (void)image_points1;
        (void)image_point1_count;
        (void)image_point2_offsets;
        (void)image_point2_group_count;
        (void)image_points2;
        (void)image_point2_count;
        (void)image_width;
        (void)image_height;
        (void)per_view_errors;
        (void)flags;
        (void)criteria_type;
        (void)criteria_max_count;
        (void)criteria_epsilon;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_fisheye_calibrate(
    const int* object_point_offsets,
    int object_point_group_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point_offsets,
    int image_point_group_count,
    const jyppx_ocv_calib3d_point2f* image_points,
    int image_point_count,
    int image_width,
    int image_height,
    jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_fisheye_calibrate";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs, "dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvecs, "rvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvecs, "tvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_width <= 0 || image_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(
                api_name,
                image_width <= 0 ? "image_width" : "image_height");
        }
        status = validate_term_criteria(api_name, criteria_type, criteria_max_count, criteria_epsilon);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_calibration_point_groups(
            api_name,
            object_point_offsets,
            object_point_group_count,
            object_points,
            object_point_count,
            image_point_offsets,
            image_point_group_count,
            image_points,
            image_point_count,
            "image_point_offsets",
            "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        std::vector<std::vector<cv::Point3f>> native_object_points =
            to_point3f_groups(object_point_offsets, object_point_group_count, object_points);
        std::vector<std::vector<cv::Point2f>> native_image_points =
            to_point2f_groups(image_point_offsets, image_point_group_count, image_points);
        std::vector<cv::Mat> native_rvecs;
        std::vector<cv::Mat> native_tvecs;
        *reprojection_error = cv::fisheye::calibrate(
            native_object_points,
            native_image_points,
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(dist_coeffs),
            native_rvecs,
            native_tvecs,
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        pack_pose_vectors(native_rvecs, opencv_csharp_native::mat_value(rvecs));
        pack_pose_vectors(native_tvecs, opencv_csharp_native::mat_value(tvecs));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_point_group_count;
        (void)object_points;
        (void)object_point_count;
        (void)image_point_offsets;
        (void)image_point_group_count;
        (void)image_points;
        (void)image_point_count;
        (void)flags;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_fisheye_stereo_calibrate(
    const int* object_point_offsets,
    int object_point_group_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point1_offsets,
    int image_point1_group_count,
    const jyppx_ocv_calib3d_point2f* image_points1,
    int image_point1_count,
    const int* image_point2_offsets,
    int image_point2_group_count,
    const jyppx_ocv_calib3d_point2f* image_points2,
    int image_point2_count,
    jyppx_ocv_mat* camera_matrix1,
    jyppx_ocv_mat* dist_coeffs1,
    jyppx_ocv_mat* camera_matrix2,
    jyppx_ocv_mat* dist_coeffs2,
    int image_width,
    int image_height,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_fisheye_stereo_calibrate";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, camera_matrix1, "camera_matrix1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs1, "dist_coeffs1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, camera_matrix2, "camera_matrix2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs2, "dist_coeffs2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_width <= 0 || image_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(
                api_name,
                image_width <= 0 ? "image_width" : "image_height");
        }
        status = validate_term_criteria(api_name, criteria_type, criteria_max_count, criteria_epsilon);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_calibration_point_groups(
            api_name,
            object_point_offsets,
            object_point_group_count,
            object_points,
            object_point_count,
            image_point1_offsets,
            image_point1_group_count,
            image_points1,
            image_point1_count,
            "image_point1_offsets",
            "image_points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(
            api_name,
            image_point2_offsets,
            image_point2_group_count,
            image_points2,
            image_point2_count,
            "image_point2_offsets",
            "image_points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_point2_group_count != object_point_group_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_point2_group_count");
        }
        for (int i = 0; i < object_point_group_count; ++i)
        {
            if ((object_point_offsets[i + 1] - object_point_offsets[i]) !=
                (image_point2_offsets[i + 1] - image_point2_offsets[i]))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "image_point2_group_count");
            }
        }

        std::vector<std::vector<cv::Point3f>> native_object_points =
            to_point3f_groups(object_point_offsets, object_point_group_count, object_points);
        std::vector<std::vector<cv::Point2f>> native_image_points1 =
            to_point2f_groups(image_point1_offsets, image_point1_group_count, image_points1);
        std::vector<std::vector<cv::Point2f>> native_image_points2 =
            to_point2f_groups(image_point2_offsets, image_point2_group_count, image_points2);
        *reprojection_error = cv::fisheye::stereoCalibrate(
            native_object_points,
            native_image_points1,
            native_image_points2,
            opencv_csharp_native::mat_value(camera_matrix1),
            opencv_csharp_native::mat_value(dist_coeffs1),
            opencv_csharp_native::mat_value(camera_matrix2),
            opencv_csharp_native::mat_value(dist_coeffs2),
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_point_group_count;
        (void)object_points;
        (void)object_point_count;
        (void)image_point1_offsets;
        (void)image_point1_group_count;
        (void)image_points1;
        (void)image_point1_count;
        (void)image_point2_offsets;
        (void)image_point2_group_count;
        (void)image_points2;
        (void)image_point2_count;
        (void)flags;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_fisheye_stereo_calibrate_extended(
    const int* object_point_offsets,
    int object_point_group_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point1_offsets,
    int image_point1_group_count,
    const jyppx_ocv_calib3d_point2f* image_points1,
    int image_point1_count,
    const int* image_point2_offsets,
    int image_point2_group_count,
    const jyppx_ocv_calib3d_point2f* image_points2,
    int image_point2_count,
    jyppx_ocv_mat* camera_matrix1,
    jyppx_ocv_mat* dist_coeffs1,
    jyppx_ocv_mat* camera_matrix2,
    jyppx_ocv_mat* dist_coeffs2,
    int image_width,
    int image_height,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_fisheye_stereo_calibrate_extended";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_output_mat(api_name, camera_matrix1, "camera_matrix1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs1, "dist_coeffs1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, camera_matrix2, "camera_matrix2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, dist_coeffs2, "dist_coeffs2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvecs, "rvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvecs, "tvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_width <= 0 || image_height <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(
                api_name,
                image_width <= 0 ? "image_width" : "image_height");
        }
        status = validate_term_criteria(api_name, criteria_type, criteria_max_count, criteria_epsilon);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_calibration_point_groups(
            api_name,
            object_point_offsets,
            object_point_group_count,
            object_points,
            object_point_count,
            image_point1_offsets,
            image_point1_group_count,
            image_points1,
            image_point1_count,
            "image_point1_offsets",
            "image_points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(
            api_name,
            image_point2_offsets,
            image_point2_group_count,
            image_points2,
            image_point2_count,
            "image_point2_offsets",
            "image_points2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_point2_group_count != object_point_group_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "image_point2_group_count");
        }
        for (int i = 0; i < object_point_group_count; ++i)
        {
            if ((object_point_offsets[i + 1] - object_point_offsets[i]) !=
                (image_point2_offsets[i + 1] - image_point2_offsets[i]))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "image_point2_group_count");
            }
        }

        std::vector<std::vector<cv::Point3f>> native_object_points =
            to_point3f_groups(object_point_offsets, object_point_group_count, object_points);
        std::vector<std::vector<cv::Point2f>> native_image_points1 =
            to_point2f_groups(image_point1_offsets, image_point1_group_count, image_points1);
        std::vector<std::vector<cv::Point2f>> native_image_points2 =
            to_point2f_groups(image_point2_offsets, image_point2_group_count, image_points2);
        std::vector<cv::Mat> native_rvecs;
        std::vector<cv::Mat> native_tvecs;
        *reprojection_error = cv::fisheye::stereoCalibrate(
            native_object_points,
            native_image_points1,
            native_image_points2,
            opencv_csharp_native::mat_value(camera_matrix1),
            opencv_csharp_native::mat_value(dist_coeffs1),
            opencv_csharp_native::mat_value(camera_matrix2),
            opencv_csharp_native::mat_value(dist_coeffs2),
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            native_rvecs,
            native_tvecs,
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        pack_pose_vectors(native_rvecs, opencv_csharp_native::mat_value(rvecs));
        pack_pose_vectors(native_tvecs, opencv_csharp_native::mat_value(tvecs));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_point_group_count;
        (void)object_points;
        (void)object_point_count;
        (void)image_point1_offsets;
        (void)image_point1_group_count;
        (void)image_points1;
        (void)image_point1_count;
        (void)image_point2_offsets;
        (void)image_point2_group_count;
        (void)image_points2;
        (void)image_point2_count;
        (void)flags;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_register_cameras(
    const int* object_point1_offsets,
    int object_point1_group_count,
    const jyppx_ocv_calib3d_point3f* object_points1,
    int object_point1_count,
    const int* object_point2_offsets,
    int object_point2_group_count,
    const jyppx_ocv_calib3d_point3f* object_points2,
    int object_point2_count,
    const int* image_point1_offsets,
    int image_point1_group_count,
    const jyppx_ocv_calib3d_point2f* image_points1,
    int image_point1_count,
    const int* image_point2_offsets,
    int image_point2_group_count,
    const jyppx_ocv_calib3d_point2f* image_points2,
    int image_point2_count,
    const jyppx_ocv_mat* camera_matrix1,
    const jyppx_ocv_mat* dist_coeffs1,
    int camera_model1,
    const jyppx_ocv_mat* camera_matrix2,
    const jyppx_ocv_mat* dist_coeffs2,
    int camera_model2,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    jyppx_ocv_mat* e,
    jyppx_ocv_mat* f,
    jyppx_ocv_mat* per_view_errors,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_register_cameras";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, camera_matrix1, "camera_matrix1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs1, "dist_coeffs1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix2, "camera_matrix2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs2, "dist_coeffs2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, e, "e");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, f, "f");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, per_view_errors, "per_view_errors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if ((camera_model1 != 0 && camera_model1 != 1) ||
            (camera_model2 != 0 && camera_model2 != 1))
        {
            return opencv_csharp_native::set_invalid_argument(
                api_name,
                camera_model1 != 0 && camera_model1 != 1 ? "camera_model1" : "camera_model2");
        }
        status = validate_term_criteria(api_name, criteria_type, criteria_max_count, criteria_epsilon);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_camera_registration_point_groups(
            api_name,
            object_point1_offsets,
            object_point1_group_count,
            object_points1,
            object_point1_count,
            object_point2_offsets,
            object_point2_group_count,
            object_points2,
            object_point2_count,
            image_point1_offsets,
            image_point1_group_count,
            image_points1,
            image_point1_count,
            image_point2_offsets,
            image_point2_group_count,
            image_points2,
            image_point2_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        *reprojection_error = cv::registerCameras(
            to_point3f_groups(object_point1_offsets, object_point1_group_count, object_points1),
            to_point3f_groups(object_point2_offsets, object_point2_group_count, object_points2),
            to_point2f_groups(image_point1_offsets, image_point1_group_count, image_points1),
            to_point2f_groups(image_point2_offsets, image_point2_group_count, image_points2),
            opencv_csharp_native::mat_value(camera_matrix1),
            opencv_csharp_native::mat_value(dist_coeffs1),
            static_cast<cv::CameraModel>(camera_model1),
            opencv_csharp_native::mat_value(camera_matrix2),
            opencv_csharp_native::mat_value(dist_coeffs2),
            static_cast<cv::CameraModel>(camera_model2),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            opencv_csharp_native::mat_value(e),
            opencv_csharp_native::mat_value(f),
            opencv_csharp_native::mat_value(per_view_errors),
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point1_offsets;
        (void)object_point1_group_count;
        (void)object_points1;
        (void)object_point1_count;
        (void)object_point2_offsets;
        (void)object_point2_group_count;
        (void)object_points2;
        (void)object_point2_count;
        (void)image_point1_offsets;
        (void)image_point1_group_count;
        (void)image_points1;
        (void)image_point1_count;
        (void)image_point2_offsets;
        (void)image_point2_group_count;
        (void)image_points2;
        (void)image_point2_count;
        (void)flags;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_register_cameras_extended(
    const int* object_point1_offsets,
    int object_point1_group_count,
    const jyppx_ocv_calib3d_point3f* object_points1,
    int object_point1_count,
    const int* object_point2_offsets,
    int object_point2_group_count,
    const jyppx_ocv_calib3d_point3f* object_points2,
    int object_point2_count,
    const int* image_point1_offsets,
    int image_point1_group_count,
    const jyppx_ocv_calib3d_point2f* image_points1,
    int image_point1_count,
    const int* image_point2_offsets,
    int image_point2_group_count,
    const jyppx_ocv_calib3d_point2f* image_points2,
    int image_point2_count,
    const jyppx_ocv_mat* camera_matrix1,
    const jyppx_ocv_mat* dist_coeffs1,
    int camera_model1,
    const jyppx_ocv_mat* camera_matrix2,
    const jyppx_ocv_mat* dist_coeffs2,
    int camera_model2,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    jyppx_ocv_mat* e,
    jyppx_ocv_mat* f,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    jyppx_ocv_mat* per_view_errors,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_register_cameras_extended";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, camera_matrix1, "camera_matrix1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs1, "dist_coeffs1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix2, "camera_matrix2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs2, "dist_coeffs2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r, "r");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, t, "t");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, e, "e");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, f, "f");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, rvecs, "rvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, tvecs, "tvecs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, per_view_errors, "per_view_errors");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if ((camera_model1 != 0 && camera_model1 != 1) ||
            (camera_model2 != 0 && camera_model2 != 1))
        {
            return opencv_csharp_native::set_invalid_argument(
                api_name,
                camera_model1 != 0 && camera_model1 != 1 ? "camera_model1" : "camera_model2");
        }
        status = validate_term_criteria(api_name, criteria_type, criteria_max_count, criteria_epsilon);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (reprojection_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reprojection_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_camera_registration_point_groups(
            api_name,
            object_point1_offsets,
            object_point1_group_count,
            object_points1,
            object_point1_count,
            object_point2_offsets,
            object_point2_group_count,
            object_points2,
            object_point2_count,
            image_point1_offsets,
            image_point1_group_count,
            image_points1,
            image_point1_count,
            image_point2_offsets,
            image_point2_group_count,
            image_points2,
            image_point2_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        std::vector<cv::Mat> native_rvecs;
        std::vector<cv::Mat> native_tvecs;
        *reprojection_error = cv::registerCameras(
            to_point3f_groups(object_point1_offsets, object_point1_group_count, object_points1),
            to_point3f_groups(object_point2_offsets, object_point2_group_count, object_points2),
            to_point2f_groups(image_point1_offsets, image_point1_group_count, image_points1),
            to_point2f_groups(image_point2_offsets, image_point2_group_count, image_points2),
            opencv_csharp_native::mat_value(camera_matrix1),
            opencv_csharp_native::mat_value(dist_coeffs1),
            static_cast<cv::CameraModel>(camera_model1),
            opencv_csharp_native::mat_value(camera_matrix2),
            opencv_csharp_native::mat_value(dist_coeffs2),
            static_cast<cv::CameraModel>(camera_model2),
            opencv_csharp_native::mat_value(r),
            opencv_csharp_native::mat_value(t),
            opencv_csharp_native::mat_value(e),
            opencv_csharp_native::mat_value(f),
            native_rvecs,
            native_tvecs,
            opencv_csharp_native::mat_value(per_view_errors),
            flags,
            make_term_criteria(criteria_type, criteria_max_count, criteria_epsilon));
        pack_pose_vectors(native_rvecs, opencv_csharp_native::mat_value(rvecs));
        pack_pose_vectors(native_tvecs, opencv_csharp_native::mat_value(tvecs));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point1_offsets;
        (void)object_point1_group_count;
        (void)object_points1;
        (void)object_point1_count;
        (void)object_point2_offsets;
        (void)object_point2_group_count;
        (void)object_points2;
        (void)object_point2_count;
        (void)image_point1_offsets;
        (void)image_point1_group_count;
        (void)image_points1;
        (void)image_point1_count;
        (void)image_point2_offsets;
        (void)image_point2_group_count;
        (void)image_points2;
        (void)image_point2_count;
        (void)flags;
        *reprojection_error = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_calibrate_multiview(
    const int* object_point_offsets,
    int frame_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point_offsets,
    int camera_count,
    int image_frame_count,
    const jyppx_ocv_calib3d_point2f* image_points,
    int image_point_count,
    const int* image_widths,
    const int* image_heights,
    const unsigned char* detection_mask,
    const int* camera_models,
    jyppx_ocv_mat* const* camera_matrices,
    jyppx_ocv_mat* const* dist_coeffs,
    jyppx_ocv_mat* const* rotation_vectors,
    jyppx_ocv_mat* const* translation_vectors,
    const int* flags_for_intrinsics,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_calibrate_multiview";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_multiview_arguments(
            api_name,
            frame_count,
            camera_count,
            image_frame_count,
            image_widths,
            image_heights,
            detection_mask,
            camera_models,
            camera_matrices,
            dist_coeffs,
            rotation_vectors,
            translation_vectors,
            flags_for_intrinsics,
            flags,
            criteria_type,
            criteria_max_count,
            criteria_epsilon,
            reprojection_error,
            false,
            nullptr,
            nullptr,
            nullptr,
            nullptr);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *reprojection_error = 0.0;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_multiview_point_groups(
            api_name,
            object_point_offsets,
            frame_count,
            object_points,
            object_point_count,
            image_point_offsets,
            camera_count,
            image_points,
            image_point_count,
            detection_mask);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        *reprojection_error = run_multiview_calibration(
            object_point_offsets,
            frame_count,
            object_points,
            image_point_offsets,
            camera_count,
            image_points,
            image_widths,
            image_heights,
            detection_mask,
            camera_models,
            camera_matrices,
            dist_coeffs,
            rotation_vectors,
            translation_vectors,
            nullptr,
            nullptr,
            nullptr,
            nullptr,
            flags_for_intrinsics,
            flags,
            criteria_type,
            criteria_max_count,
            criteria_epsilon);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_points;
        (void)object_point_count;
        (void)image_point_offsets;
        (void)image_points;
        (void)image_point_count;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_calibrate_multiview_extended(
    const int* object_point_offsets,
    int frame_count,
    const jyppx_ocv_calib3d_point3f* object_points,
    int object_point_count,
    const int* image_point_offsets,
    int camera_count,
    int image_frame_count,
    const jyppx_ocv_calib3d_point2f* image_points,
    int image_point_count,
    const int* image_widths,
    const int* image_heights,
    const unsigned char* detection_mask,
    const int* camera_models,
    jyppx_ocv_mat* const* camera_matrices,
    jyppx_ocv_mat* const* dist_coeffs,
    jyppx_ocv_mat* const* rotation_vectors,
    jyppx_ocv_mat* const* translation_vectors,
    jyppx_ocv_mat* initialization_pairs,
    jyppx_ocv_mat* const* rvecs0,
    jyppx_ocv_mat* const* tvecs0,
    jyppx_ocv_mat* per_frame_errors,
    const int* flags_for_intrinsics,
    int flags,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double* reprojection_error)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_calibrate_multiview_extended";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_multiview_arguments(
            api_name,
            frame_count,
            camera_count,
            image_frame_count,
            image_widths,
            image_heights,
            detection_mask,
            camera_models,
            camera_matrices,
            dist_coeffs,
            rotation_vectors,
            translation_vectors,
            flags_for_intrinsics,
            flags,
            criteria_type,
            criteria_max_count,
            criteria_epsilon,
            reprojection_error,
            true,
            initialization_pairs,
            rvecs0,
            tvecs0,
            per_frame_errors);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *reprojection_error = 0.0;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_multiview_point_groups(
            api_name,
            object_point_offsets,
            frame_count,
            object_points,
            object_point_count,
            image_point_offsets,
            camera_count,
            image_points,
            image_point_count,
            detection_mask);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        *reprojection_error = run_multiview_calibration(
            object_point_offsets,
            frame_count,
            object_points,
            image_point_offsets,
            camera_count,
            image_points,
            image_widths,
            image_heights,
            detection_mask,
            camera_models,
            camera_matrices,
            dist_coeffs,
            rotation_vectors,
            translation_vectors,
            initialization_pairs,
            rvecs0,
            tvecs0,
            per_frame_errors,
            flags_for_intrinsics,
            flags,
            criteria_type,
            criteria_max_count,
            criteria_epsilon);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)object_point_offsets;
        (void)object_points;
        (void)object_point_count;
        (void)image_point_offsets;
        (void)image_points;
        (void)image_point_count;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_calib3d_rectify3_collinear(
    const jyppx_ocv_mat* camera_matrix1,
    const jyppx_ocv_mat* dist_coeffs1,
    const jyppx_ocv_mat* camera_matrix2,
    const jyppx_ocv_mat* dist_coeffs2,
    const jyppx_ocv_mat* camera_matrix3,
    const jyppx_ocv_mat* dist_coeffs3,
    const int* image_point1_offsets,
    int image_point1_group_count,
    const jyppx_ocv_calib3d_point2f* image_points1,
    int image_point1_count,
    const int* image_point3_offsets,
    int image_point3_group_count,
    const jyppx_ocv_calib3d_point2f* image_points3,
    int image_point3_count,
    int image_width,
    int image_height,
    const jyppx_ocv_mat* r12,
    const jyppx_ocv_mat* t12,
    const jyppx_ocv_mat* r13,
    const jyppx_ocv_mat* t13,
    jyppx_ocv_mat* r1,
    jyppx_ocv_mat* r2,
    jyppx_ocv_mat* r3,
    jyppx_ocv_mat* p1,
    jyppx_ocv_mat* p2,
    jyppx_ocv_mat* p3,
    jyppx_ocv_mat* q,
    double alpha,
    int new_image_width,
    int new_image_height,
    int flags,
    int* roi1_x,
    int* roi1_y,
    int* roi1_width,
    int* roi1_height,
    int* roi2_x,
    int* roi2_y,
    int* roi2_width,
    int* roi2_height,
    float* scale)
{
    constexpr const char* api_name = "jyppx_ocv_calib3d_rectify3_collinear";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_input_mat(api_name, camera_matrix1, "camera_matrix1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs1, "dist_coeffs1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix2, "camera_matrix2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs2, "dist_coeffs2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, camera_matrix3, "camera_matrix3");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, dist_coeffs3, "dist_coeffs3");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, r12, "r12");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, t12, "t12");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, r13, "r13");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, t13, "t13");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r1, "r1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r2, "r2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, r3, "r3");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, p1, "p1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, p2, "p2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, p3, "p3");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, q, "q");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (scale == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "scale");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_point2f_groups(api_name, image_point1_offsets, image_point1_group_count, image_points1, image_point1_count, "image_point1_offsets", "image_points1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(api_name, image_point3_offsets, image_point3_group_count, image_points3, image_point3_count, "image_point3_offsets", "image_points3");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        std::vector<std::vector<cv::Point2f>> native_image_points1 = to_point2f_groups(image_point1_offsets, image_point1_group_count, image_points1);
        std::vector<std::vector<cv::Point2f>> native_image_points3 = to_point2f_groups(image_point3_offsets, image_point3_group_count, image_points3);
        cv::Rect roi1;
        cv::Rect roi2;
        *scale = cv::rectify3Collinear(
            opencv_csharp_native::mat_value(camera_matrix1),
            opencv_csharp_native::mat_value(dist_coeffs1),
            opencv_csharp_native::mat_value(camera_matrix2),
            opencv_csharp_native::mat_value(dist_coeffs2),
            opencv_csharp_native::mat_value(camera_matrix3),
            opencv_csharp_native::mat_value(dist_coeffs3),
            native_image_points1,
            native_image_points3,
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(r12),
            opencv_csharp_native::mat_value(t12),
            opencv_csharp_native::mat_value(r13),
            opencv_csharp_native::mat_value(t13),
            opencv_csharp_native::mat_value(r1),
            opencv_csharp_native::mat_value(r2),
            opencv_csharp_native::mat_value(r3),
            opencv_csharp_native::mat_value(p1),
            opencv_csharp_native::mat_value(p2),
            opencv_csharp_native::mat_value(p3),
            opencv_csharp_native::mat_value(q),
            alpha,
            cv::Size(new_image_width, new_image_height),
            &roi1,
            &roi2,
            flags);
        write_rect(roi1, roi1_x, roi1_y, roi1_width, roi1_height);
        write_rect(roi2, roi2_x, roi2_y, roi2_width, roi2_height);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image_point1_offsets;
        (void)image_point1_group_count;
        (void)image_points1;
        (void)image_point1_count;
        (void)image_point3_offsets;
        (void)image_point3_group_count;
        (void)image_points3;
        (void)image_point3_count;
        (void)image_width;
        (void)image_height;
        (void)alpha;
        (void)new_image_width;
        (void)new_image_height;
        (void)flags;
        write_rect_values(0, 0, 0, 0, roi1_x, roi1_y, roi1_width, roi1_height);
        write_rect_values(0, 0, 0, 0, roi2_x, roi2_y, roi2_width, roi2_height);
        *scale = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

