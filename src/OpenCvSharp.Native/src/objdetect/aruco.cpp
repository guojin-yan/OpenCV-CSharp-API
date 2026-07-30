#include "open_cv_sharp/objdetect/aruco.h"

#include "../core/mat_handle.h"
#include "../dnn/dnn_handles.h"
#include "../error_state.h"
#include "aruco_handles.h"

#include <limits>
#include <new>
#include <vector>

namespace
{
    int validate_dictionary(const char* api_name, const jyppx_ocv_aruco_dictionary* dictionary)
    {
        return dictionary == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "dictionary")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_detector(const char* api_name, const jyppx_ocv_aruco_detector* detector)
    {
        return detector == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "detector")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_grid_board(const char* api_name, const jyppx_ocv_aruco_grid_board* board)
    {
        return board == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "board")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_board(const char* api_name, const jyppx_ocv_aruco_board* board)
    {
        return board == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "board")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_charuco_board(const char* api_name, const jyppx_ocv_aruco_charuco_board* board)
    {
        return board == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "board")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_charuco_detector(const char* api_name, const jyppx_ocv_aruco_charuco_detector* detector)
    {
        return detector == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "detector")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mcc_checker(const char* api_name, const jyppx_ocv_mcc_checker* checker)
    {
        return checker == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "checker")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mcc_checker_detector(const char* api_name, const jyppx_ocv_mcc_checker_detector* detector)
    {
        return detector == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "detector")
            : OPENCV_CSHARP_STATUS_OK;
    }

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

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_rects(const char* api_name, const int* rois, int roi_count)
    {
        if (roi_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "roi_count");
        }

        if (roi_count > (std::numeric_limits<int>::max() / 4))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "roi_count");
        }

        if (roi_count > 0 && rois == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rois");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_color_chart_type(const char* api_name, int chart_type)
    {
        return chart_type < 0 || chart_type > 2
            ? opencv_csharp_native::set_invalid_argument(api_name, "chart_type")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_ids(const char* api_name, const int* ids, int id_count)
    {
        if (id_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "id_count");
        }

        if (id_count > 0 && ids == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "ids");
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

    void set_default_detector_params(jyppx_ocv_aruco_detector_params* params)
    {
        if (params == nullptr)
        {
            return;
        }

        params->adaptive_thresh_win_size_min = 3;
        params->adaptive_thresh_win_size_max = 23;
        params->adaptive_thresh_win_size_step = 10;
        params->adaptive_thresh_constant = 7.0;
        params->min_marker_perimeter_rate = 0.03;
        params->max_marker_perimeter_rate = 4.0;
        params->polygonal_approx_accuracy_rate = 0.03;
        params->min_corner_distance_rate = 0.05;
        params->min_distance_to_border = 3;
        params->min_marker_distance_rate = 0.125;
        params->min_group_distance = 0.21F;
        params->corner_refinement_method = 0;
        params->corner_refinement_win_size = 5;
        params->relative_corner_refinement_win_size = 0.3F;
        params->corner_refinement_max_iterations = 30;
        params->corner_refinement_min_accuracy = 0.1;
        params->marker_border_bits = 1;
        params->perspective_remove_pixel_per_cell = 4;
        params->perspective_remove_ignored_margin_per_cell = 0.13;
        params->max_erroneous_bits_in_border_rate = 0.35;
        params->min_otsu_std_dev = 5.0;
        params->error_correction_rate = 0.6;
        params->april_tag_quad_decimate = 0.0F;
        params->april_tag_quad_sigma = 0.0F;
        params->april_tag_min_cluster_pixels = 5;
        params->april_tag_max_nmaxima = 10;
        params->april_tag_critical_rad = 0.174532925F;
        params->april_tag_max_line_fit_mse = 10.0F;
        params->april_tag_min_white_black_diff = 5;
        params->april_tag_deglitch = 0;
        params->detect_inverted_marker = 0;
        params->use_aruco3_detection = 0;
        params->min_side_length_canonical_img = 32;
        params->min_marker_length_ratio_original_img = 0.0F;
        params->valid_bit_id_threshold = 0.49F;
    }

    void set_default_refine_params(jyppx_ocv_aruco_refine_params* params)
    {
        if (params == nullptr)
        {
            return;
        }

        params->min_rep_distance = 10.0F;
        params->error_correction_rate = 3.0F;
        params->check_all_orders = 1;
    }

    void set_default_charuco_params(jyppx_ocv_aruco_charuco_params* params)
    {
        if (params == nullptr)
        {
            return;
        }

        params->min_markers = 2;
        params->try_refine_markers = 0;
        params->check_markers = 1;
    }

    void set_default_mcc_params(jyppx_ocv_mcc_detector_params* params)
    {
        if (params == nullptr)
        {
            return;
        }

        params->adaptive_thresh_win_size_min = 23;
        params->adaptive_thresh_win_size_max = 153;
        params->adaptive_thresh_win_size_step = 16;
        params->adaptive_thresh_constant = 7.0;
        params->min_contours_area_rate = 0.003;
        params->min_contours_area = 100.0;
        params->confidence_threshold = 0.5;
        params->min_contour_solidity = 0.9;
        params->find_candidates_approx_poly_dp_eps_multiplier = 0.05;
        params->border_width = 0;
        params->b0_factor = 1.25F;
        params->max_error = 0.1F;
        params->min_contour_points_allowed = 4;
        params->min_contour_length_allowed = 100;
        params->min_inter_contour_distance = 100;
        params->min_inter_checker_distance = 10000;
        params->min_image_size = 1000;
        params->min_group_size = 4;
    }

    int validate_point2f_groups(
        const char* api_name,
        const int* offsets,
        int group_count,
        const jyppx_ocv_point2f* points,
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
            if (point_count != 0)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, points_name);
            }

            return OPENCV_CSHARP_STATUS_OK;
        }

        if (offsets == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, offsets_name);
        }

        if (point_count > 0 && points == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, points_name);
        }

        if (offsets[0] != 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, offsets_name);
        }

        for (int i = 0; i < group_count; ++i)
        {
            if (offsets[i] < 0 || offsets[i + 1] < offsets[i] || offsets[i + 1] > point_count)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, offsets_name);
            }
        }

        if (offsets[group_count] != point_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, points_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_point3f_groups(
        const char* api_name,
        const int* offsets,
        int group_count,
        const jyppx_ocv_point3f* points,
        int point_count,
        const char* offsets_name,
        const char* points_name)
    {
        if (group_count < 0 || point_count < 0)
            return opencv_csharp_native::set_invalid_argument(api_name, group_count < 0 ? offsets_name : points_name);
        if (group_count == 0)
            return point_count == 0 ? OPENCV_CSHARP_STATUS_OK : opencv_csharp_native::set_invalid_argument(api_name, points_name);
        if (offsets == nullptr || (point_count > 0 && points == nullptr) || offsets[0] != 0)
            return opencv_csharp_native::set_invalid_argument(api_name, offsets == nullptr || offsets[0] != 0 ? offsets_name : points_name);
        for (int i = 0; i < group_count; ++i)
            if (offsets[i] < 0 || offsets[i + 1] < offsets[i] || offsets[i + 1] > point_count)
                return opencv_csharp_native::set_invalid_argument(api_name, offsets_name);
        return offsets[group_count] == point_count
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_invalid_argument(api_name, points_name);
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::aruco::DetectorParameters to_detector_params(const jyppx_ocv_aruco_detector_params& source)
    {
        cv::aruco::DetectorParameters params;
        params.adaptiveThreshWinSizeMin = source.adaptive_thresh_win_size_min;
        params.adaptiveThreshWinSizeMax = source.adaptive_thresh_win_size_max;
        params.adaptiveThreshWinSizeStep = source.adaptive_thresh_win_size_step;
        params.adaptiveThreshConstant = source.adaptive_thresh_constant;
        params.minMarkerPerimeterRate = source.min_marker_perimeter_rate;
        params.maxMarkerPerimeterRate = source.max_marker_perimeter_rate;
        params.polygonalApproxAccuracyRate = source.polygonal_approx_accuracy_rate;
        params.minCornerDistanceRate = source.min_corner_distance_rate;
        params.minDistanceToBorder = source.min_distance_to_border;
        params.minMarkerDistanceRate = source.min_marker_distance_rate;
        params.minGroupDistance = source.min_group_distance;
        params.cornerRefinementMethod = source.corner_refinement_method;
        params.cornerRefinementWinSize = source.corner_refinement_win_size;
        params.relativeCornerRefinmentWinSize = source.relative_corner_refinement_win_size;
        params.cornerRefinementMaxIterations = source.corner_refinement_max_iterations;
        params.cornerRefinementMinAccuracy = source.corner_refinement_min_accuracy;
        params.markerBorderBits = source.marker_border_bits;
        params.perspectiveRemovePixelPerCell = source.perspective_remove_pixel_per_cell;
        params.perspectiveRemoveIgnoredMarginPerCell = source.perspective_remove_ignored_margin_per_cell;
        params.maxErroneousBitsInBorderRate = source.max_erroneous_bits_in_border_rate;
        params.minOtsuStdDev = source.min_otsu_std_dev;
        params.errorCorrectionRate = source.error_correction_rate;
        params.aprilTagQuadDecimate = source.april_tag_quad_decimate;
        params.aprilTagQuadSigma = source.april_tag_quad_sigma;
        params.aprilTagMinClusterPixels = source.april_tag_min_cluster_pixels;
        params.aprilTagMaxNmaxima = source.april_tag_max_nmaxima;
        params.aprilTagCriticalRad = source.april_tag_critical_rad;
        params.aprilTagMaxLineFitMse = source.april_tag_max_line_fit_mse;
        params.aprilTagMinWhiteBlackDiff = source.april_tag_min_white_black_diff;
        params.aprilTagDeglitch = source.april_tag_deglitch;
        params.detectInvertedMarker = source.detect_inverted_marker != 0;
        params.useAruco3Detection = source.use_aruco3_detection != 0;
        params.minSideLengthCanonicalImg = source.min_side_length_canonical_img;
        params.minMarkerLengthRatioOriginalImg = source.min_marker_length_ratio_original_img;
        params.validBitIdThreshold = source.valid_bit_id_threshold;
        return params;
    }

    jyppx_ocv_aruco_detector_params from_detector_params(const cv::aruco::DetectorParameters& source)
    {
        jyppx_ocv_aruco_detector_params params{};
        params.adaptive_thresh_win_size_min = source.adaptiveThreshWinSizeMin;
        params.adaptive_thresh_win_size_max = source.adaptiveThreshWinSizeMax;
        params.adaptive_thresh_win_size_step = source.adaptiveThreshWinSizeStep;
        params.adaptive_thresh_constant = source.adaptiveThreshConstant;
        params.min_marker_perimeter_rate = source.minMarkerPerimeterRate;
        params.max_marker_perimeter_rate = source.maxMarkerPerimeterRate;
        params.polygonal_approx_accuracy_rate = source.polygonalApproxAccuracyRate;
        params.min_corner_distance_rate = source.minCornerDistanceRate;
        params.min_distance_to_border = source.minDistanceToBorder;
        params.min_marker_distance_rate = source.minMarkerDistanceRate;
        params.min_group_distance = source.minGroupDistance;
        params.corner_refinement_method = source.cornerRefinementMethod;
        params.corner_refinement_win_size = source.cornerRefinementWinSize;
        params.relative_corner_refinement_win_size = source.relativeCornerRefinmentWinSize;
        params.corner_refinement_max_iterations = source.cornerRefinementMaxIterations;
        params.corner_refinement_min_accuracy = source.cornerRefinementMinAccuracy;
        params.marker_border_bits = source.markerBorderBits;
        params.perspective_remove_pixel_per_cell = source.perspectiveRemovePixelPerCell;
        params.perspective_remove_ignored_margin_per_cell = source.perspectiveRemoveIgnoredMarginPerCell;
        params.max_erroneous_bits_in_border_rate = source.maxErroneousBitsInBorderRate;
        params.min_otsu_std_dev = source.minOtsuStdDev;
        params.error_correction_rate = source.errorCorrectionRate;
        params.april_tag_quad_decimate = source.aprilTagQuadDecimate;
        params.april_tag_quad_sigma = source.aprilTagQuadSigma;
        params.april_tag_min_cluster_pixels = source.aprilTagMinClusterPixels;
        params.april_tag_max_nmaxima = source.aprilTagMaxNmaxima;
        params.april_tag_critical_rad = source.aprilTagCriticalRad;
        params.april_tag_max_line_fit_mse = source.aprilTagMaxLineFitMse;
        params.april_tag_min_white_black_diff = source.aprilTagMinWhiteBlackDiff;
        params.april_tag_deglitch = source.aprilTagDeglitch;
        params.detect_inverted_marker = source.detectInvertedMarker ? 1 : 0;
        params.use_aruco3_detection = source.useAruco3Detection ? 1 : 0;
        params.min_side_length_canonical_img = source.minSideLengthCanonicalImg;
        params.min_marker_length_ratio_original_img = source.minMarkerLengthRatioOriginalImg;
        params.valid_bit_id_threshold = source.validBitIdThreshold;
        return params;
    }

    cv::aruco::RefineParameters to_refine_params(const jyppx_ocv_aruco_refine_params& source)
    {
        return cv::aruco::RefineParameters(
            source.min_rep_distance,
            source.error_correction_rate,
            source.check_all_orders != 0);
    }

    jyppx_ocv_aruco_refine_params from_refine_params(const cv::aruco::RefineParameters& source)
    {
        jyppx_ocv_aruco_refine_params params{};
        params.min_rep_distance = source.minRepDistance;
        params.error_correction_rate = source.errorCorrectionRate;
        params.check_all_orders = source.checkAllOrders ? 1 : 0;
        return params;
    }

    cv::aruco::CharucoParameters to_charuco_params(
        const jyppx_ocv_aruco_charuco_params& source,
        const jyppx_ocv_mat* camera_matrix,
        const jyppx_ocv_mat* dist_coeffs)
    {
        cv::aruco::CharucoParameters params;
        params.minMarkers = source.min_markers;
        params.tryRefineMarkers = source.try_refine_markers != 0;
        params.checkMarkers = source.check_markers != 0;
        if (camera_matrix != nullptr)
        {
            params.cameraMatrix = opencv_csharp_native::mat_value(camera_matrix).clone();
        }

        if (dist_coeffs != nullptr)
        {
            params.distCoeffs = opencv_csharp_native::mat_value(dist_coeffs).clone();
        }

        return params;
    }

    jyppx_ocv_aruco_charuco_params from_charuco_params(const cv::aruco::CharucoParameters& source)
    {
        jyppx_ocv_aruco_charuco_params params{};
        params.min_markers = source.minMarkers;
        params.try_refine_markers = source.tryRefineMarkers ? 1 : 0;
        params.check_markers = source.checkMarkers ? 1 : 0;
        return params;
    }

    jyppx_ocv_mcc_detector_params from_mcc_params(const cv::mcc::DetectorParametersMCC& source)
    {
        jyppx_ocv_mcc_detector_params params{};
        params.adaptive_thresh_win_size_min = source.adaptiveThreshWinSizeMin;
        params.adaptive_thresh_win_size_max = source.adaptiveThreshWinSizeMax;
        params.adaptive_thresh_win_size_step = source.adaptiveThreshWinSizeStep;
        params.adaptive_thresh_constant = source.adaptiveThreshConstant;
        params.min_contours_area_rate = source.minContoursAreaRate;
        params.min_contours_area = source.minContoursArea;
        params.confidence_threshold = source.confidenceThreshold;
        params.min_contour_solidity = source.minContourSolidity;
        params.find_candidates_approx_poly_dp_eps_multiplier = source.findCandidatesApproxPolyDPEpsMultiplier;
        params.border_width = source.borderWidth;
        params.b0_factor = source.B0factor;
        params.max_error = source.maxError;
        params.min_contour_points_allowed = source.minContourPointsAllowed;
        params.min_contour_length_allowed = source.minContourLengthAllowed;
        params.min_inter_contour_distance = source.minInterContourDistance;
        params.min_inter_checker_distance = source.minInterCheckerDistance;
        params.min_image_size = source.minImageSize;
        params.min_group_size = source.minGroupSize;
        return params;
    }

    cv::mcc::DetectorParametersMCC to_mcc_params(const jyppx_ocv_mcc_detector_params& source)
    {
        cv::mcc::DetectorParametersMCC params;
        params.adaptiveThreshWinSizeMin = source.adaptive_thresh_win_size_min;
        params.adaptiveThreshWinSizeMax = source.adaptive_thresh_win_size_max;
        params.adaptiveThreshWinSizeStep = source.adaptive_thresh_win_size_step;
        params.adaptiveThreshConstant = source.adaptive_thresh_constant;
        params.minContoursAreaRate = source.min_contours_area_rate;
        params.minContoursArea = source.min_contours_area;
        params.confidenceThreshold = source.confidence_threshold;
        params.minContourSolidity = source.min_contour_solidity;
        params.findCandidatesApproxPolyDPEpsMultiplier = source.find_candidates_approx_poly_dp_eps_multiplier;
        params.borderWidth = source.border_width;
        params.B0factor = source.b0_factor;
        params.maxError = source.max_error;
        params.minContourPointsAllowed = source.min_contour_points_allowed;
        params.minContourLengthAllowed = source.min_contour_length_allowed;
        params.minInterContourDistance = source.min_inter_contour_distance;
        params.minInterCheckerDistance = source.min_inter_checker_distance;
        params.minImageSize = source.min_image_size;
        params.minGroupSize = source.min_group_size;
        return params;
    }

    int set_count_from_size(const char* api_name, size_t size, int* count, const char* argument_name)
    {
        if (count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (size > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        *count = static_cast<int>(size);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int point_count(const std::vector<std::vector<cv::Point2f>>& groups)
    {
        size_t total = 0;
        for (const std::vector<cv::Point2f>& group : groups)
        {
            total += group.size();
            if (total > static_cast<size_t>(std::numeric_limits<int>::max()))
            {
                return -1;
            }
        }

        return static_cast<int>(total);
    }

    int point3_count(const std::vector<std::vector<cv::Point3f>>& groups)
    {
        size_t total = 0;
        for (const std::vector<cv::Point3f>& group : groups)
        {
            total += group.size();
            if (total > static_cast<size_t>(std::numeric_limits<int>::max())) return -1;
        }
        return static_cast<int>(total);
    }

    int copy_point3_groups(
        const char* api_name,
        const std::vector<std::vector<cv::Point3f>>& groups,
        int* offsets,
        int offset_capacity,
        jyppx_ocv_point3f* points,
        int point_capacity,
        int* group_count,
        int* total_point_count)
    {
        int status = set_count_from_size(api_name, groups.size(), group_count, "marker_count");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (total_point_count == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "object_point_count");
        *total_point_count = point3_count(groups);
        if (*total_point_count < 0) return opencv_csharp_native::set_invalid_argument(api_name, "object_point_count");
        if (offsets == nullptr || offset_capacity < *group_count + 1) return opencv_csharp_native::set_invalid_argument(api_name, "offsets");
        if (*total_point_count > 0 && (points == nullptr || point_capacity < *total_point_count)) return opencv_csharp_native::set_invalid_argument(api_name, "points");
        int offset = 0;
        offsets[0] = 0;
        for (int i = 0; i < *group_count; ++i)
        {
            for (const cv::Point3f& point : groups[static_cast<size_t>(i)])
            {
                points[offset++] = { point.x, point.y, point.z };
            }
            offsets[i + 1] = offset;
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int set_point_group_counts(
        const char* api_name,
        const std::vector<std::vector<cv::Point2f>>& groups,
        int* group_count,
        int* total_point_count)
    {
        int status = set_count_from_size(api_name, groups.size(), group_count, "group_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        const int total = point_count(groups);
        if (total < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        if (total_point_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "point_count");
        }

        *total_point_count = total;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_point_groups(
        const char* api_name,
        const std::vector<std::vector<cv::Point2f>>& groups,
        int* offsets,
        int offset_capacity,
        jyppx_ocv_point2f* points,
        int point_capacity,
        int* group_count,
        int* total_point_count,
        const char* offsets_name,
        const char* points_name)
    {
        int status = set_point_group_counts(api_name, groups, group_count, total_point_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (offsets == nullptr || offset_capacity < (*group_count + 1))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, offsets_name);
        }

        if (*total_point_count > 0 && (points == nullptr || point_capacity < *total_point_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, points_name);
        }

        int offset = 0;
        offsets[0] = 0;
        for (int i = 0; i < *group_count; ++i)
        {
            const std::vector<cv::Point2f>& group = groups[static_cast<size_t>(i)];
            for (const cv::Point2f& point : group)
            {
                points[offset].x = point.x;
                points[offset].y = point.y;
                ++offset;
            }

            offsets[i + 1] = offset;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_ints(const char* api_name, const std::vector<int>& values, int* buffer, int buffer_capacity, const char* argument_name)
    {
        if (values.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        const int count = static_cast<int>(values.size());
        if (count > 0 && (buffer == nullptr || buffer_capacity < count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < count; ++i)
        {
            buffer[i] = values[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_floats(const char* api_name, const std::vector<float>& values, float* buffer, int buffer_capacity, const char* argument_name)
    {
        if (values.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        const int count = static_cast<int>(values.size());
        if (count > 0 && (buffer == nullptr || buffer_capacity < count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < count; ++i)
        {
            buffer[i] = values[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_point2f_vector(
        const char* api_name,
        const std::vector<cv::Point2f>& values,
        jyppx_ocv_point2f* buffer,
        int buffer_capacity,
        int* count,
        const char* argument_name)
    {
        int status = set_count_from_size(api_name, values.size(), count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (*count > 0 && (buffer == nullptr || buffer_capacity < *count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < *count; ++i)
        {
            const cv::Point2f& point = values[static_cast<size_t>(i)];
            buffer[i].x = point.x;
            buffer[i].y = point.y;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_point3f_vector(
        const char* api_name,
        const std::vector<cv::Point3f>& values,
        jyppx_ocv_point3f* buffer,
        int buffer_capacity,
        int* count,
        const char* argument_name)
    {
        int status = set_count_from_size(api_name, values.size(), count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (*count > 0 && (buffer == nullptr || buffer_capacity < *count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < *count; ++i)
        {
            const cv::Point3f& point = values[static_cast<size_t>(i)];
            buffer[i].x = point.x;
            buffer[i].y = point.y;
            buffer[i].z = point.z;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    std::vector<int> to_int_vector(const int* ids, int id_count)
    {
        return id_count <= 0 ? std::vector<int>() : std::vector<int>(ids, ids + id_count);
    }

    std::vector<cv::Rect> to_rect_vector(const int* values, int rect_count)
    {
        std::vector<cv::Rect> result;
        if (rect_count <= 0)
        {
            return result;
        }

        result.reserve(static_cast<size_t>(rect_count));
        for (int i = 0; i < rect_count; ++i)
        {
            const int offset = i * 4;
            result.emplace_back(values[offset], values[offset + 1], values[offset + 2], values[offset + 3]);
        }

        return result;
    }

    std::vector<cv::Point2f> to_point2f_vector(const jyppx_ocv_point2f* points, int point_count)
    {
        std::vector<cv::Point2f> result;
        if (point_count <= 0)
        {
            return result;
        }

        result.reserve(static_cast<size_t>(point_count));
        for (int i = 0; i < point_count; ++i)
        {
            result.emplace_back(points[i].x, points[i].y);
        }

        return result;
    }

    std::vector<std::vector<cv::Point2f>> to_point2f_groups(
        const int* offsets,
        int group_count,
        const jyppx_ocv_point2f* points,
        int point_count)
    {
        std::vector<std::vector<cv::Point2f>> result;
        if (group_count <= 0)
        {
            return result;
        }

        result.reserve(static_cast<size_t>(group_count));
        for (int i = 0; i < group_count; ++i)
        {
            const int start = offsets[i];
            const int end = offsets[i + 1];
            std::vector<cv::Point2f> group;
            group.reserve(static_cast<size_t>(end - start));
            for (int j = start; j < end; ++j)
            {
                group.emplace_back(points[j].x, points[j].y);
            }

            result.push_back(std::move(group));
        }

        (void)point_count;
        return result;
    }

    std::vector<std::vector<cv::Point3f>> to_point3f_groups(
        const int* offsets,
        int group_count,
        const jyppx_ocv_point3f* points)
    {
        std::vector<std::vector<cv::Point3f>> result;
        result.reserve(static_cast<size_t>(group_count));
        for (int i = 0; i < group_count; ++i)
        {
            std::vector<cv::Point3f> group;
            group.reserve(static_cast<size_t>(offsets[i + 1] - offsets[i]));
            for (int j = offsets[i]; j < offsets[i + 1]; ++j)
                group.emplace_back(points[j].x, points[j].y, points[j].z);
            result.push_back(std::move(group));
        }
        return result;
    }

    bool checker_is_valid(const jyppx_ocv_mcc_checker* checker)
    {
        return checker != nullptr && !checker->value.empty();
    }

    int validate_mcc_checker_value(const char* api_name, const jyppx_ocv_mcc_checker* checker)
    {
        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return checker->value.empty()
            ? opencv_csharp_native::set_invalid_argument(api_name, "checker")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mcc_checker_detector_value(const char* api_name, const jyppx_ocv_mcc_checker_detector* detector)
    {
        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return detector->value.empty()
            ? opencv_csharp_native::set_invalid_argument(api_name, "detector")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int create_mat_from_clone(const char* api_name, const cv::Mat& source, jyppx_ocv_mat** destination, const char* argument_name)
    {
        if (destination == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        *destination = new (std::nothrow) jyppx_ocv_mat{ source.clone() };
        return *destination == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
    }

    void refine_detected_markers(
        const jyppx_ocv_aruco_detector* detector,
        const jyppx_ocv_mat* image,
        const jyppx_ocv_aruco_grid_board* board,
        std::vector<std::vector<cv::Point2f>>& corners,
        std::vector<int>& ids,
        std::vector<std::vector<cv::Point2f>>& rejected,
        const jyppx_ocv_mat* camera_matrix,
        const jyppx_ocv_mat* dist_coeffs,
        std::vector<int>& recovered_indices)
    {
        if (camera_matrix != nullptr && dist_coeffs != nullptr)
        {
            detector->value.refineDetectedMarkers(
                opencv_csharp_native::mat_value(image),
                board->value,
                corners,
                ids,
                rejected,
                opencv_csharp_native::mat_value(camera_matrix),
                opencv_csharp_native::mat_value(dist_coeffs),
                recovered_indices);
            return;
        }

        if (camera_matrix != nullptr)
        {
            detector->value.refineDetectedMarkers(
                opencv_csharp_native::mat_value(image),
                board->value,
                corners,
                ids,
                rejected,
                opencv_csharp_native::mat_value(camera_matrix),
                cv::noArray(),
                recovered_indices);
            return;
        }

        if (dist_coeffs != nullptr)
        {
            detector->value.refineDetectedMarkers(
                opencv_csharp_native::mat_value(image),
                board->value,
                corners,
                ids,
                rejected,
                cv::noArray(),
                opencv_csharp_native::mat_value(dist_coeffs),
                recovered_indices);
            return;
        }

        detector->value.refineDetectedMarkers(
            opencv_csharp_native::mat_value(image),
            board->value,
            corners,
            ids,
            rejected,
            cv::noArray(),
            cv::noArray(),
            recovered_indices);
    }

#endif
}

int jyppx_ocv_aruco_dictionary_create_default(jyppx_ocv_aruco_dictionary** dictionary)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_create_default";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (dictionary == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dictionary");
        }

        *dictionary = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_aruco_dictionary* created = new (std::nothrow) jyppx_ocv_aruco_dictionary();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *dictionary = created;
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

int jyppx_ocv_aruco_dictionary_create_predefined(int dictionary_id, jyppx_ocv_aruco_dictionary** dictionary)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_create_predefined";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (dictionary == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dictionary");
        }

        *dictionary = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_aruco_dictionary* created = new (std::nothrow) jyppx_ocv_aruco_dictionary{ cv::aruco::getPredefinedDictionary(dictionary_id) };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *dictionary = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dictionary_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_create_from_bytes_list(
    const jyppx_ocv_mat* bytes_list,
    int marker_size,
    int max_correction_bits,
    jyppx_ocv_aruco_dictionary** dictionary)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_create_from_bytes_list";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, bytes_list, "bytes_list");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (dictionary == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dictionary");
        }

        *dictionary = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_aruco_dictionary* created = new (std::nothrow) jyppx_ocv_aruco_dictionary{ cv::aruco::Dictionary(opencv_csharp_native::mat_value(bytes_list), marker_size, max_correction_bits) };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *dictionary = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)marker_size;
        (void)max_correction_bits;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_extend(
    int marker_count,
    int marker_size,
    const jyppx_ocv_aruco_dictionary* base_dictionary,
    int random_seed,
    jyppx_ocv_aruco_dictionary** dictionary)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_extend";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (marker_count < 0 || marker_size <= 0) return opencv_csharp_native::set_invalid_argument(api_name, marker_count < 0 ? "marker_count" : "marker_size");
        if (dictionary == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "dictionary");
        *dictionary = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::aruco::Dictionary base = base_dictionary == nullptr ? cv::aruco::Dictionary() : base_dictionary->value;
        auto* created = new (std::nothrow) jyppx_ocv_aruco_dictionary{ cv::aruco::extendDictionary(marker_count, marker_size, base, random_seed) };
        if (created == nullptr) return opencv_csharp_native::set_out_of_memory(api_name);
        *dictionary = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)base_dictionary; (void)random_seed;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_aruco_dictionary_release_handle(jyppx_ocv_aruco_dictionary* dictionary)
{
    delete dictionary;
}

int jyppx_ocv_aruco_dictionary_get_bytes_list(const jyppx_ocv_aruco_dictionary* dictionary, jyppx_ocv_mat** bytes_list)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_get_bytes_list";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (bytes_list == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bytes_list");
        }

        *bytes_list = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *bytes_list = new (std::nothrow) jyppx_ocv_mat{ dictionary->value.bytesList.clone() };
        return *bytes_list == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_set_bytes_list(jyppx_ocv_aruco_dictionary* dictionary, const jyppx_ocv_mat* bytes_list)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_set_bytes_list";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, bytes_list, "bytes_list");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        dictionary->value.bytesList = opencv_csharp_native::mat_value(bytes_list).clone();
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

int jyppx_ocv_aruco_dictionary_get_marker_size(const jyppx_ocv_aruco_dictionary* dictionary, int* marker_size)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_get_marker_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, marker_size, "marker_size");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *marker_size = dictionary->value.markerSize;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *marker_size = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_set_marker_size(jyppx_ocv_aruco_dictionary* dictionary, int marker_size)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_set_marker_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        dictionary->value.markerSize = marker_size;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)marker_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_get_max_correction_bits(const jyppx_ocv_aruco_dictionary* dictionary, int* max_correction_bits)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_get_max_correction_bits";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, max_correction_bits, "max_correction_bits");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *max_correction_bits = dictionary->value.maxCorrectionBits;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *max_correction_bits = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_set_max_correction_bits(jyppx_ocv_aruco_dictionary* dictionary, int max_correction_bits)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_set_max_correction_bits";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        dictionary->value.maxCorrectionBits = max_correction_bits;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)max_correction_bits;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_identify(
    const jyppx_ocv_aruco_dictionary* dictionary,
    const jyppx_ocv_mat* bits,
    double max_correction_rate,
    int* identified,
    int* index,
    int* rotation)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_identify";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, bits, "bits");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, identified, "identified");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, index, "index");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, rotation, "rotation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(identified, dictionary->value.identify(opencv_csharp_native::mat_value(bits), *index, *rotation, max_correction_rate));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)max_correction_rate;
        *identified = 0;
        *index = -1;
        *rotation = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_identify_with_threshold(
    const jyppx_ocv_aruco_dictionary* dictionary,
    const jyppx_ocv_mat* cell_pixel_ratio,
    double max_correction_rate,
    float valid_bit_id_threshold,
    int* identified,
    int* index,
    int* rotation)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_identify_with_threshold";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, cell_pixel_ratio, "cell_pixel_ratio");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, identified, "identified");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, index, "index");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, rotation, "rotation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        assign_bool(identified, dictionary->value.identify(opencv_csharp_native::mat_value(cell_pixel_ratio), *index, *rotation, max_correction_rate, valid_bit_id_threshold));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)max_correction_rate;
        (void)valid_bit_id_threshold;
        *identified = 0;
        *index = -1;
        *rotation = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_get_distance_to_id(
    const jyppx_ocv_aruco_dictionary* dictionary,
    const jyppx_ocv_mat* bits,
    int id,
    int all_rotations,
    int* distance)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_get_distance_to_id";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, bits, "bits");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, distance, "distance");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *distance = dictionary->value.getDistanceToId(opencv_csharp_native::mat_value(bits), id, all_rotations != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)id;
        (void)all_rotations;
        *distance = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_generate_image_marker(
    const jyppx_ocv_aruco_dictionary* dictionary,
    int id,
    int side_pixels,
    jyppx_ocv_mat* image,
    int border_bits)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_generate_image_marker";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        dictionary->value.generateImageMarker(id, side_pixels, opencv_csharp_native::mat_value(image), border_bits);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)id;
        (void)side_pixels;
        (void)border_bits;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_get_marker_bits(
    const jyppx_ocv_aruco_dictionary* dictionary,
    int marker_id,
    int rotation_id,
    jyppx_ocv_mat** bits)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_get_marker_bits";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (bits == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bits");
        }

        *bits = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *bits = new (std::nothrow) jyppx_ocv_mat{ dictionary->value.getMarkerBits(marker_id, rotation_id) };
        return *bits == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        (void)marker_id;
        (void)rotation_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_get_byte_list_from_bits(const jyppx_ocv_mat* bits, jyppx_ocv_mat** byte_list)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_get_byte_list_from_bits";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, bits, "bits");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (byte_list == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "byte_list");
        }

        *byte_list = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *byte_list = new (std::nothrow) jyppx_ocv_mat{ cv::aruco::Dictionary::getByteListFromBits(opencv_csharp_native::mat_value(bits)) };
        return *byte_list == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_dictionary_get_bits_from_byte_list(
    const jyppx_ocv_mat* byte_list,
    int marker_size,
    int rotation_id,
    jyppx_ocv_mat** bits)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_dictionary_get_bits_from_byte_list";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mat(api_name, byte_list, "byte_list");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (bits == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bits");
        }

        *bits = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *bits = new (std::nothrow) jyppx_ocv_mat{ cv::aruco::Dictionary::getBitsFromByteList(opencv_csharp_native::mat_value(byte_list), marker_size, rotation_id) };
        return *bits == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        (void)marker_size;
        (void)rotation_id;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_default_params(jyppx_ocv_aruco_detector_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_default_params";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *params = from_detector_params(cv::aruco::DetectorParameters());
#else
        set_default_detector_params(params);
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_refine_default_params(jyppx_ocv_aruco_refine_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_refine_default_params";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *params = from_refine_params(cv::aruco::RefineParameters());
#else
        set_default_refine_params(params);
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_detector_default_params(jyppx_ocv_mcc_detector_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_detector_default_params";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *params = from_mcc_params(cv::mcc::DetectorParametersMCC());
#else
        set_default_mcc_params(params);
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_default_params(jyppx_ocv_aruco_charuco_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_default_params";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *params = from_charuco_params(cv::aruco::CharucoParameters());
#else
        set_default_charuco_params(params);
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_create(
    const jyppx_ocv_aruco_dictionary* dictionary,
    const jyppx_ocv_aruco_detector_params* detector_params,
    const jyppx_ocv_aruco_refine_params* refine_params,
    jyppx_ocv_aruco_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::aruco::DetectorParameters native_detector_params = detector_params == nullptr
            ? cv::aruco::DetectorParameters()
            : to_detector_params(*detector_params);
        cv::aruco::RefineParameters native_refine_params = refine_params == nullptr
            ? cv::aruco::RefineParameters()
            : to_refine_params(*refine_params);
        jyppx_ocv_aruco_detector* created = new (std::nothrow) jyppx_ocv_aruco_detector{ cv::aruco::ArucoDetector(dictionary->value, native_detector_params, native_refine_params) };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *detector = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector_params;
        (void)refine_params;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_create_multi_dictionary(
    const jyppx_ocv_aruco_dictionary* const* dictionaries,
    int dictionary_count,
    const jyppx_ocv_aruco_detector_params* detector_params,
    const jyppx_ocv_aruco_refine_params* refine_params,
    jyppx_ocv_aruco_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_create_multi_dictionary";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (dictionary_count <= 0 || dictionaries == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "dictionaries");
        if (detector == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        *detector = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::aruco::Dictionary> native_dictionaries;
        native_dictionaries.reserve(static_cast<size_t>(dictionary_count));
        for (int i = 0; i < dictionary_count; ++i)
        {
            if (dictionaries[i] == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "dictionaries");
            native_dictionaries.push_back(dictionaries[i]->value);
        }
        const auto native_detector_params = detector_params == nullptr ? cv::aruco::DetectorParameters() : to_detector_params(*detector_params);
        const auto native_refine_params = refine_params == nullptr ? cv::aruco::RefineParameters() : to_refine_params(*refine_params);
        auto* created = new (std::nothrow) jyppx_ocv_aruco_detector{ cv::aruco::ArucoDetector(native_dictionaries, native_detector_params, native_refine_params) };
        if (created == nullptr) return opencv_csharp_native::set_out_of_memory(api_name);
        *detector = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)detector_params; (void)refine_params;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

void jyppx_ocv_aruco_detector_release_handle(jyppx_ocv_aruco_detector* detector)
{
    delete detector;
}

int jyppx_ocv_aruco_detector_get_dictionary(const jyppx_ocv_aruco_detector* detector, jyppx_ocv_aruco_dictionary** dictionary)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_get_dictionary";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (dictionary == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dictionary");
        }

        *dictionary = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *dictionary = new (std::nothrow) jyppx_ocv_aruco_dictionary{ detector->value.getDictionary() };
        return *dictionary == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_set_dictionary(jyppx_ocv_aruco_detector* detector, const jyppx_ocv_aruco_dictionary* dictionary)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_set_dictionary";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setDictionary(dictionary->value);
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

int jyppx_ocv_aruco_detector_get_dictionaries_count(const jyppx_ocv_aruco_detector* detector, int* dictionary_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_get_dictionaries_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_count_from_size(api_name, detector->value.getDictionaries().size(), dictionary_count, "dictionary_count");
#else
        if (dictionary_count != nullptr) *dictionary_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_detector_get_dictionary_at(
    const jyppx_ocv_aruco_detector* detector,
    int dictionary_index,
    jyppx_ocv_aruco_dictionary** dictionary)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_get_dictionary_at";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (dictionary == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "dictionary");
        *dictionary = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const auto values = detector->value.getDictionaries();
        if (dictionary_index < 0 || static_cast<size_t>(dictionary_index) >= values.size()) return opencv_csharp_native::set_invalid_argument(api_name, "dictionary_index");
        auto* created = new (std::nothrow) jyppx_ocv_aruco_dictionary{ values[static_cast<size_t>(dictionary_index)] };
        if (created == nullptr) return opencv_csharp_native::set_out_of_memory(api_name);
        *dictionary = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dictionary_index;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_detector_set_dictionaries(
    jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_aruco_dictionary* const* dictionaries,
    int dictionary_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_set_dictionaries";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (dictionary_count <= 0 || dictionaries == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "dictionaries");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::aruco::Dictionary> values;
        values.reserve(static_cast<size_t>(dictionary_count));
        for (int i = 0; i < dictionary_count; ++i)
        {
            if (dictionaries[i] == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "dictionaries");
            values.push_back(dictionaries[i]->value);
        }
        detector->value.setDictionaries(values);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_detector_get_detector_parameters(const jyppx_ocv_aruco_detector* detector, jyppx_ocv_aruco_detector_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_get_detector_parameters";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *params = from_detector_params(detector->value.getDetectorParameters());
        return OPENCV_CSHARP_STATUS_OK;
#else
        set_default_detector_params(params);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_set_detector_parameters(jyppx_ocv_aruco_detector* detector, const jyppx_ocv_aruco_detector_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_set_detector_parameters";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setDetectorParameters(to_detector_params(*params));
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

int jyppx_ocv_aruco_detector_get_refine_parameters(const jyppx_ocv_aruco_detector* detector, jyppx_ocv_aruco_refine_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_get_refine_parameters";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *params = from_refine_params(detector->value.getRefineParameters());
        return OPENCV_CSHARP_STATUS_OK;
#else
        set_default_refine_params(params);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_set_refine_parameters(jyppx_ocv_aruco_detector* detector, const jyppx_ocv_aruco_refine_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_set_refine_parameters";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setRefineParameters(to_refine_params(*params));
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

int jyppx_ocv_aruco_detector_detect_markers_count(
    const jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_mat* image,
    int* marker_count,
    int* corner_point_count,
    int* rejected_count,
    int* rejected_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_detect_markers_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point2f>> corners;
        std::vector<int> ids;
        std::vector<std::vector<cv::Point2f>> rejected;
        detector->value.detectMarkers(opencv_csharp_native::mat_value(image), corners, ids, rejected);
        status = set_point_group_counts(api_name, corners, marker_count, corner_point_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return set_point_group_counts(api_name, rejected, rejected_count, rejected_point_count);
#else
        if (marker_count != nullptr) { *marker_count = 0; }
        if (corner_point_count != nullptr) { *corner_point_count = 0; }
        if (rejected_count != nullptr) { *rejected_count = 0; }
        if (rejected_point_count != nullptr) { *rejected_point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_detect_markers_fill(
    const jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_mat* image,
    int* corner_offsets,
    int corner_offset_capacity,
    jyppx_ocv_point2f* corners,
    int corner_capacity,
    int* ids,
    int id_capacity,
    int* rejected_offsets,
    int rejected_offset_capacity,
    jyppx_ocv_point2f* rejected_points,
    int rejected_point_capacity,
    int* marker_count,
    int* corner_point_count,
    int* rejected_count,
    int* rejected_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_detect_markers_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point2f>> native_corners;
        std::vector<int> native_ids;
        std::vector<std::vector<cv::Point2f>> native_rejected;
        detector->value.detectMarkers(opencv_csharp_native::mat_value(image), native_corners, native_ids, native_rejected);
        status = copy_point_groups(api_name, native_corners, corner_offsets, corner_offset_capacity, corners, corner_capacity, marker_count, corner_point_count, "corner_offsets", "corners");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = copy_ints(api_name, native_ids, ids, id_capacity, "ids");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_point_groups(api_name, native_rejected, rejected_offsets, rejected_offset_capacity, rejected_points, rejected_point_capacity, rejected_count, rejected_point_count, "rejected_offsets", "rejected_points");
#else
        (void)corner_offsets;
        (void)corner_offset_capacity;
        (void)corners;
        (void)corner_capacity;
        (void)ids;
        (void)id_capacity;
        (void)rejected_offsets;
        (void)rejected_offset_capacity;
        (void)rejected_points;
        (void)rejected_point_capacity;
        if (marker_count != nullptr) { *marker_count = 0; }
        if (corner_point_count != nullptr) { *corner_point_count = 0; }
        if (rejected_count != nullptr) { *rejected_count = 0; }
        if (rejected_point_count != nullptr) { *rejected_point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_detect_markers_multi_dictionary_count(
    const jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_mat* image,
    int* marker_count,
    int* corner_point_count,
    int* rejected_count,
    int* rejected_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_detect_markers_multi_dictionary_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point2f>> corners, rejected;
        std::vector<int> ids, dictionary_indices;
        detector->value.detectMarkersMultiDict(opencv_csharp_native::mat_value(image), corners, ids, rejected, dictionary_indices);
        status = set_point_group_counts(api_name, corners, marker_count, corner_point_count);
        return status == OPENCV_CSHARP_STATUS_OK
            ? set_point_group_counts(api_name, rejected, rejected_count, rejected_point_count)
            : status;
#else
        if (marker_count != nullptr) *marker_count = 0;
        if (corner_point_count != nullptr) *corner_point_count = 0;
        if (rejected_count != nullptr) *rejected_count = 0;
        if (rejected_point_count != nullptr) *rejected_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_detector_detect_markers_multi_dictionary_fill(
    const jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_mat* image,
    int* corner_offsets,
    int corner_offset_capacity,
    jyppx_ocv_point2f* corners,
    int corner_capacity,
    int* ids,
    int id_capacity,
    int* dictionary_indices,
    int dictionary_index_capacity,
    int* rejected_offsets,
    int rejected_offset_capacity,
    jyppx_ocv_point2f* rejected_points,
    int rejected_point_capacity,
    int* marker_count,
    int* corner_point_count,
    int* rejected_count,
    int* rejected_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_detect_markers_multi_dictionary_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point2f>> native_corners, native_rejected;
        std::vector<int> native_ids, native_dictionary_indices;
        detector->value.detectMarkersMultiDict(opencv_csharp_native::mat_value(image), native_corners, native_ids, native_rejected, native_dictionary_indices);
        status = copy_point_groups(api_name, native_corners, corner_offsets, corner_offset_capacity, corners, corner_capacity, marker_count, corner_point_count, "corner_offsets", "corners");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = copy_ints(api_name, native_ids, ids, id_capacity, "ids");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = copy_ints(api_name, native_dictionary_indices, dictionary_indices, dictionary_index_capacity, "dictionary_indices");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return copy_point_groups(api_name, native_rejected, rejected_offsets, rejected_offset_capacity, rejected_points, rejected_point_capacity, rejected_count, rejected_point_count, "rejected_offsets", "rejected_points");
#else
        (void)corner_offsets; (void)corner_offset_capacity; (void)corners; (void)corner_capacity;
        (void)ids; (void)id_capacity; (void)dictionary_indices; (void)dictionary_index_capacity;
        (void)rejected_offsets; (void)rejected_offset_capacity; (void)rejected_points; (void)rejected_point_capacity;
        if (marker_count != nullptr) *marker_count = 0;
        if (corner_point_count != nullptr) *corner_point_count = 0;
        if (rejected_count != nullptr) *rejected_count = 0;
        if (rejected_point_count != nullptr) *rejected_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_detector_detect_markers_with_confidence_count(
    const jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_mat* image,
    int* marker_count,
    int* corner_point_count,
    int* rejected_count,
    int* rejected_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_detect_markers_with_confidence_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point2f>> corners;
        std::vector<int> ids;
        std::vector<float> confidence;
        std::vector<std::vector<cv::Point2f>> rejected;
        detector->value.detectMarkersWithConfidence(opencv_csharp_native::mat_value(image), corners, ids, confidence, rejected);
        status = set_point_group_counts(api_name, corners, marker_count, corner_point_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return set_point_group_counts(api_name, rejected, rejected_count, rejected_point_count);
#else
        if (marker_count != nullptr) { *marker_count = 0; }
        if (corner_point_count != nullptr) { *corner_point_count = 0; }
        if (rejected_count != nullptr) { *rejected_count = 0; }
        if (rejected_point_count != nullptr) { *rejected_point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_detect_markers_with_confidence_fill(
    const jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_mat* image,
    int* corner_offsets,
    int corner_offset_capacity,
    jyppx_ocv_point2f* corners,
    int corner_capacity,
    int* ids,
    int id_capacity,
    float* confidence,
    int confidence_capacity,
    int* rejected_offsets,
    int rejected_offset_capacity,
    jyppx_ocv_point2f* rejected_points,
    int rejected_point_capacity,
    int* marker_count,
    int* corner_point_count,
    int* rejected_count,
    int* rejected_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_detect_markers_with_confidence_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point2f>> native_corners;
        std::vector<int> native_ids;
        std::vector<float> native_confidence;
        std::vector<std::vector<cv::Point2f>> native_rejected;
        detector->value.detectMarkersWithConfidence(opencv_csharp_native::mat_value(image), native_corners, native_ids, native_confidence, native_rejected);
        status = copy_point_groups(api_name, native_corners, corner_offsets, corner_offset_capacity, corners, corner_capacity, marker_count, corner_point_count, "corner_offsets", "corners");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = copy_ints(api_name, native_ids, ids, id_capacity, "ids");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = copy_floats(api_name, native_confidence, confidence, confidence_capacity, "confidence");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_point_groups(api_name, native_rejected, rejected_offsets, rejected_offset_capacity, rejected_points, rejected_point_capacity, rejected_count, rejected_point_count, "rejected_offsets", "rejected_points");
#else
        (void)corner_offsets;
        (void)corner_offset_capacity;
        (void)corners;
        (void)corner_capacity;
        (void)ids;
        (void)id_capacity;
        (void)confidence;
        (void)confidence_capacity;
        (void)rejected_offsets;
        (void)rejected_offset_capacity;
        (void)rejected_points;
        (void)rejected_point_capacity;
        if (marker_count != nullptr) { *marker_count = 0; }
        if (corner_point_count != nullptr) { *corner_point_count = 0; }
        if (rejected_count != nullptr) { *rejected_count = 0; }
        if (rejected_point_count != nullptr) { *rejected_point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_refine_detected_markers_count(
    const jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_aruco_grid_board* board,
    const int* detected_offsets,
    int detected_group_count,
    const jyppx_ocv_point2f* detected_points,
    int detected_point_count,
    const int* detected_ids,
    int detected_id_count,
    const int* rejected_offsets,
    int rejected_group_count,
    const jyppx_ocv_point2f* rejected_points,
    int rejected_point_count,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    int* refined_marker_count,
    int* refined_corner_point_count,
    int* refined_rejected_count,
    int* refined_rejected_point_count,
    int* recovered_index_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_refine_detected_markers_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_grid_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(api_name, detected_offsets, detected_group_count, detected_points, detected_point_count, "detected_offsets", "detected_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(api_name, rejected_offsets, rejected_group_count, rejected_points, rejected_point_count, "rejected_offsets", "rejected_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (detected_id_count != detected_group_count || (detected_id_count > 0 && detected_ids == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detected_ids");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point2f>> refined_corners = to_point2f_groups(detected_offsets, detected_group_count, detected_points, detected_point_count);
        std::vector<int> refined_ids = to_int_vector(detected_ids, detected_id_count);
        std::vector<std::vector<cv::Point2f>> refined_rejected = to_point2f_groups(rejected_offsets, rejected_group_count, rejected_points, rejected_point_count);
        std::vector<int> recovered_indices;
        refine_detected_markers(detector, image, board, refined_corners, refined_ids, refined_rejected, camera_matrix, dist_coeffs, recovered_indices);

        status = set_point_group_counts(api_name, refined_corners, refined_marker_count, refined_corner_point_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = set_point_group_counts(api_name, refined_rejected, refined_rejected_count, refined_rejected_point_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return set_count_from_size(api_name, recovered_indices.size(), recovered_index_count, "recovered_index_count");
#else
        (void)camera_matrix;
        (void)dist_coeffs;
        if (refined_marker_count != nullptr) { *refined_marker_count = 0; }
        if (refined_corner_point_count != nullptr) { *refined_corner_point_count = 0; }
        if (refined_rejected_count != nullptr) { *refined_rejected_count = 0; }
        if (refined_rejected_point_count != nullptr) { *refined_rejected_point_count = 0; }
        if (recovered_index_count != nullptr) { *recovered_index_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_detector_refine_detected_markers_fill(
    const jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_aruco_grid_board* board,
    const int* detected_offsets,
    int detected_group_count,
    const jyppx_ocv_point2f* detected_points,
    int detected_point_count,
    const int* detected_ids,
    int detected_id_count,
    const int* rejected_offsets,
    int rejected_group_count,
    const jyppx_ocv_point2f* rejected_points,
    int rejected_point_count,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    int* refined_offsets,
    int refined_offset_capacity,
    jyppx_ocv_point2f* refined_points,
    int refined_point_capacity,
    int* refined_ids,
    int refined_id_capacity,
    int* refined_rejected_offsets,
    int refined_rejected_offset_capacity,
    jyppx_ocv_point2f* refined_rejected_points,
    int refined_rejected_point_capacity,
    int* recovered_indices,
    int recovered_index_capacity,
    int* refined_marker_count,
    int* refined_corner_point_count,
    int* refined_rejected_count,
    int* refined_rejected_point_count,
    int* recovered_index_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_detector_refine_detected_markers_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_grid_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(api_name, detected_offsets, detected_group_count, detected_points, detected_point_count, "detected_offsets", "detected_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(api_name, rejected_offsets, rejected_group_count, rejected_points, rejected_point_count, "rejected_offsets", "rejected_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (detected_id_count != detected_group_count || (detected_id_count > 0 && detected_ids == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detected_ids");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point2f>> native_refined_corners = to_point2f_groups(detected_offsets, detected_group_count, detected_points, detected_point_count);
        std::vector<int> native_refined_ids = to_int_vector(detected_ids, detected_id_count);
        std::vector<std::vector<cv::Point2f>> native_refined_rejected = to_point2f_groups(rejected_offsets, rejected_group_count, rejected_points, rejected_point_count);
        std::vector<int> native_recovered_indices;
        refine_detected_markers(detector, image, board, native_refined_corners, native_refined_ids, native_refined_rejected, camera_matrix, dist_coeffs, native_recovered_indices);

        status = copy_point_groups(
            api_name,
            native_refined_corners,
            refined_offsets,
            refined_offset_capacity,
            refined_points,
            refined_point_capacity,
            refined_marker_count,
            refined_corner_point_count,
            "refined_offsets",
            "refined_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        status = copy_ints(api_name, native_refined_ids, refined_ids, refined_id_capacity, "refined_ids");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        status = copy_point_groups(
            api_name,
            native_refined_rejected,
            refined_rejected_offsets,
            refined_rejected_offset_capacity,
            refined_rejected_points,
            refined_rejected_point_capacity,
            refined_rejected_count,
            refined_rejected_point_count,
            "refined_rejected_offsets",
            "refined_rejected_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        status = copy_ints(api_name, native_recovered_indices, recovered_indices, recovered_index_capacity, "recovered_indices");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return set_count_from_size(api_name, native_recovered_indices.size(), recovered_index_count, "recovered_index_count");
#else
        (void)camera_matrix;
        (void)dist_coeffs;
        (void)refined_offsets;
        (void)refined_offset_capacity;
        (void)refined_points;
        (void)refined_point_capacity;
        (void)refined_ids;
        (void)refined_id_capacity;
        (void)refined_rejected_offsets;
        (void)refined_rejected_offset_capacity;
        (void)refined_rejected_points;
        (void)refined_rejected_point_capacity;
        (void)recovered_indices;
        (void)recovered_index_capacity;
        if (refined_marker_count != nullptr) { *refined_marker_count = 0; }
        if (refined_corner_point_count != nullptr) { *refined_corner_point_count = 0; }
        if (refined_rejected_count != nullptr) { *refined_rejected_count = 0; }
        if (refined_rejected_point_count != nullptr) { *refined_rejected_point_count = 0; }
        if (recovered_index_count != nullptr) { *recovered_index_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_draw_detected_markers(
    jyppx_ocv_mat* image,
    const int* corner_offsets,
    int marker_count,
    const jyppx_ocv_point2f* corners,
    int corner_point_count,
    const int* ids,
    int id_count,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_draw_detected_markers";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_point2f_groups(api_name, corner_offsets, marker_count, corners, corner_point_count, "corner_offsets", "corners");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_ids(api_name, ids, id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (id_count != 0 && id_count != marker_count) return opencv_csharp_native::set_invalid_argument(api_name, "id_count");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::aruco::drawDetectedMarkers(
            opencv_csharp_native::mat_value(image),
            to_point2f_groups(corner_offsets, marker_count, corners, corner_point_count),
            to_int_vector(ids, id_count),
            cv::Scalar(color_v0, color_v1, color_v2, color_v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color_v0; (void)color_v1; (void)color_v2; (void)color_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_board_create(
    const int* object_point_offsets,
    int marker_count,
    const jyppx_ocv_point3f* object_points,
    int object_point_count,
    const jyppx_ocv_aruco_dictionary* dictionary,
    const int* ids,
    int id_count,
    jyppx_ocv_aruco_board** board)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_board_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_point3f_groups(api_name, object_point_offsets, marker_count, object_points, object_point_count, "object_point_offsets", "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_ids(api_name, ids, id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (id_count != marker_count) return opencv_csharp_native::set_invalid_argument(api_name, "id_count");
        if (board == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "board");
        *board = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto native_points = to_point3f_groups(object_point_offsets, marker_count, object_points);
        auto* created = new (std::nothrow) jyppx_ocv_aruco_board{ cv::aruco::Board(native_points, dictionary->value, to_int_vector(ids, id_count)) };
        if (created == nullptr) return opencv_csharp_native::set_out_of_memory(api_name);
        *board = created;
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

void jyppx_ocv_aruco_board_release_handle(jyppx_ocv_aruco_board* board)
{
    delete board;
}

int jyppx_ocv_aruco_board_get_dictionary(
    const jyppx_ocv_aruco_board* board,
    jyppx_ocv_aruco_dictionary** dictionary)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_board_get_dictionary";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (dictionary == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "dictionary");
        *dictionary = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto* created = new (std::nothrow) jyppx_ocv_aruco_dictionary{ board->value.getDictionary() };
        if (created == nullptr) return opencv_csharp_native::set_out_of_memory(api_name);
        *dictionary = created;
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

int jyppx_ocv_aruco_board_get_object_points_count(
    const jyppx_ocv_aruco_board* board,
    int* marker_count,
    int* object_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_board_get_object_points_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = set_count_from_size(api_name, board->value.getObjPoints().size(), marker_count, "marker_count");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (object_point_count == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "object_point_count");
        *object_point_count = point3_count(board->value.getObjPoints());
        return *object_point_count < 0 ? opencv_csharp_native::set_invalid_argument(api_name, "object_point_count") : OPENCV_CSHARP_STATUS_OK;
#else
        if (marker_count != nullptr) *marker_count = 0;
        if (object_point_count != nullptr) *object_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_board_get_object_points_fill(
    const jyppx_ocv_aruco_board* board,
    int* offsets,
    int offset_capacity,
    jyppx_ocv_point3f* points,
    int point_capacity,
    int* marker_count,
    int* object_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_board_get_object_points_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return copy_point3_groups(api_name, board->value.getObjPoints(), offsets, offset_capacity, points, point_capacity, marker_count, object_point_count);
#else
        (void)offsets; (void)offset_capacity; (void)points; (void)point_capacity;
        if (marker_count != nullptr) *marker_count = 0;
        if (object_point_count != nullptr) *object_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_board_get_ids_count(const jyppx_ocv_aruco_board* board, int* id_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_board_get_ids_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_count_from_size(api_name, board->value.getIds().size(), id_count, "id_count");
#else
        if (id_count != nullptr) *id_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_board_get_ids_fill(
    const jyppx_ocv_aruco_board* board,
    int* ids,
    int id_capacity,
    int* id_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_board_get_ids_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = set_count_from_size(api_name, board->value.getIds().size(), id_count, "id_count");
        return status == OPENCV_CSHARP_STATUS_OK ? copy_ints(api_name, board->value.getIds(), ids, id_capacity, "ids") : status;
#else
        (void)ids; (void)id_capacity;
        if (id_count != nullptr) *id_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_board_get_right_bottom_corner(
    const jyppx_ocv_aruco_board* board,
    jyppx_ocv_point3f* corner)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_board_get_right_bottom_corner";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (corner == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "corner");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Point3f& value = board->value.getRightBottomCorner();
        *corner = { value.x, value.y, value.z };
        return OPENCV_CSHARP_STATUS_OK;
#else
        *corner = {};
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_board_match_image_points(
    const jyppx_ocv_aruco_board* board,
    const int* detected_offsets,
    int detected_group_count,
    const jyppx_ocv_point2f* detected_points,
    int detected_point_count,
    const int* detected_ids,
    int detected_id_count,
    jyppx_ocv_mat* object_points,
    jyppx_ocv_mat* image_points)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_board_match_image_points";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_point2f_groups(api_name, detected_offsets, detected_group_count, detected_points, detected_point_count, "detected_offsets", "detected_points");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_ids(api_name, detected_ids, detected_id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (detected_id_count != detected_group_count) return opencv_csharp_native::set_invalid_argument(api_name, "detected_id_count");
        status = validate_mat(api_name, object_points, "object_points");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, image_points, "image_points");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        board->value.matchImagePoints(
            to_point2f_groups(detected_offsets, detected_group_count, detected_points, detected_point_count),
            to_int_vector(detected_ids, detected_id_count),
            opencv_csharp_native::mat_value(object_points),
            opencv_csharp_native::mat_value(image_points));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_board_generate_image(
    const jyppx_ocv_aruco_board* board,
    int width,
    int height,
    jyppx_ocv_mat* image,
    int margin_size,
    int border_bits)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_board_generate_image";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        board->value.generateImage(cv::Size(width, height), opencv_csharp_native::mat_value(image), margin_size, border_bits);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width; (void)height; (void)margin_size; (void)border_bits;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_grid_board_create(
    int markers_x,
    int markers_y,
    float marker_length,
    float marker_separation,
    const jyppx_ocv_aruco_dictionary* dictionary,
    const int* ids,
    int id_count,
    jyppx_ocv_aruco_grid_board** board)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_grid_board_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_ids(api_name, ids, id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (board == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "board");
        }

        *board = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<int> native_ids = to_int_vector(ids, id_count);
        jyppx_ocv_aruco_grid_board* created = new (std::nothrow) jyppx_ocv_aruco_grid_board
        {
            native_ids.empty()
                ? cv::aruco::GridBoard(cv::Size(markers_x, markers_y), marker_length, marker_separation, dictionary->value)
                : cv::aruco::GridBoard(cv::Size(markers_x, markers_y), marker_length, marker_separation, dictionary->value, native_ids)
        };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *board = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)markers_x;
        (void)markers_y;
        (void)marker_length;
        (void)marker_separation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_aruco_grid_board_release_handle(jyppx_ocv_aruco_grid_board* board)
{
    delete board;
}

int jyppx_ocv_aruco_grid_board_get_grid_size(const jyppx_ocv_aruco_grid_board* board, int* markers_x, int* markers_y)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_grid_board_get_grid_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_grid_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, markers_x, "markers_x");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, markers_y, "markers_y");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Size size = board->value.getGridSize();
        *markers_x = size.width;
        *markers_y = size.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *markers_x = 0;
        *markers_y = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_grid_board_get_marker_length(const jyppx_ocv_aruco_grid_board* board, float* marker_length)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_grid_board_get_marker_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_grid_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, marker_length, "marker_length");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *marker_length = board->value.getMarkerLength();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *marker_length = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_grid_board_get_marker_separation(const jyppx_ocv_aruco_grid_board* board, float* marker_separation)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_grid_board_get_marker_separation";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_grid_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, marker_separation, "marker_separation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *marker_separation = board->value.getMarkerSeparation();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *marker_separation = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_grid_board_generate_image(
    const jyppx_ocv_aruco_grid_board* board,
    int width,
    int height,
    jyppx_ocv_mat* image,
    int margin_size,
    int border_bits)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_grid_board_generate_image";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_grid_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        board->value.generateImage(cv::Size(width, height), opencv_csharp_native::mat_value(image), margin_size, border_bits);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width;
        (void)height;
        (void)margin_size;
        (void)border_bits;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_board_create(
    int squares_x,
    int squares_y,
    float square_length,
    float marker_length,
    const jyppx_ocv_aruco_dictionary* dictionary,
    const int* ids,
    int id_count,
    jyppx_ocv_aruco_charuco_board** board)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_board_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_dictionary(api_name, dictionary);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_ids(api_name, ids, id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (board == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "board");
        }

        *board = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<int> native_ids = to_int_vector(ids, id_count);
        jyppx_ocv_aruco_charuco_board* created = new (std::nothrow) jyppx_ocv_aruco_charuco_board
        {
            native_ids.empty()
                ? cv::aruco::CharucoBoard(cv::Size(squares_x, squares_y), square_length, marker_length, dictionary->value)
                : cv::aruco::CharucoBoard(cv::Size(squares_x, squares_y), square_length, marker_length, dictionary->value, native_ids)
        };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *board = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)squares_x;
        (void)squares_y;
        (void)square_length;
        (void)marker_length;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_aruco_charuco_board_release_handle(jyppx_ocv_aruco_charuco_board* board)
{
    delete board;
}

int jyppx_ocv_aruco_charuco_board_get_chessboard_size(const jyppx_ocv_aruco_charuco_board* board, int* squares_x, int* squares_y)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_board_get_chessboard_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, squares_x, "squares_x");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, squares_y, "squares_y");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::Size size = board->value.getChessboardSize();
        *squares_x = size.width;
        *squares_y = size.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *squares_x = 0;
        *squares_y = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_board_get_square_length(const jyppx_ocv_aruco_charuco_board* board, float* square_length)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_board_get_square_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, square_length, "square_length");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *square_length = board->value.getSquareLength();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *square_length = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_board_get_marker_length(const jyppx_ocv_aruco_charuco_board* board, float* marker_length)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_board_get_marker_length";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, marker_length, "marker_length");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *marker_length = board->value.getMarkerLength();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *marker_length = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_board_get_legacy_pattern(const jyppx_ocv_aruco_charuco_board* board, int* legacy_pattern)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_board_get_legacy_pattern";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, legacy_pattern, "legacy_pattern");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *legacy_pattern = board->value.getLegacyPattern() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *legacy_pattern = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_board_set_legacy_pattern(jyppx_ocv_aruco_charuco_board* board, int legacy_pattern)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_board_set_legacy_pattern";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        board->value.setLegacyPattern(legacy_pattern != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)legacy_pattern;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_board_get_chessboard_corners_count(const jyppx_ocv_aruco_charuco_board* board, int* corner_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_board_get_chessboard_corners_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return set_count_from_size(api_name, board->value.getChessboardCorners().size(), corner_count, "corner_count");
#else
        if (corner_count != nullptr) { *corner_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_board_get_chessboard_corners_fill(
    const jyppx_ocv_aruco_charuco_board* board,
    jyppx_ocv_point3f* corners,
    int corner_capacity,
    int* corner_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_board_get_chessboard_corners_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return copy_point3f_vector(api_name, board->value.getChessboardCorners(), corners, corner_capacity, corner_count, "corners");
#else
        (void)corners;
        (void)corner_capacity;
        if (corner_count != nullptr) { *corner_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_board_check_corners_collinear(
    const jyppx_ocv_aruco_charuco_board* board,
    const int* charuco_ids,
    int charuco_id_count,
    int* collinear)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_board_check_corners_collinear";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_ids(api_name, charuco_ids, charuco_id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, collinear, "collinear");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::vector<int> native_ids = to_int_vector(charuco_ids, charuco_id_count);
        *collinear = board->value.checkCharucoCornersCollinear(native_ids) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *collinear = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_board_generate_image(
    const jyppx_ocv_aruco_charuco_board* board,
    int width,
    int height,
    jyppx_ocv_mat* image,
    int margin_size,
    int border_bits)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_board_generate_image";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        board->value.generateImage(cv::Size(width, height), opencv_csharp_native::mat_value(image), margin_size, border_bits);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width;
        (void)height;
        (void)margin_size;
        (void)border_bits;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_detector_create(
    const jyppx_ocv_aruco_charuco_board* board,
    const jyppx_ocv_aruco_charuco_params* charuco_params,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_aruco_detector_params* detector_params,
    const jyppx_ocv_aruco_refine_params* refine_params,
    jyppx_ocv_aruco_charuco_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_aruco_charuco_params default_charuco_params{};
        set_default_charuco_params(&default_charuco_params);
        const jyppx_ocv_aruco_charuco_params& selected_charuco_params = charuco_params == nullptr
            ? default_charuco_params
            : *charuco_params;
        cv::aruco::DetectorParameters native_detector_params = detector_params == nullptr
            ? cv::aruco::DetectorParameters()
            : to_detector_params(*detector_params);
        cv::aruco::RefineParameters native_refine_params = refine_params == nullptr
            ? cv::aruco::RefineParameters()
            : to_refine_params(*refine_params);
        cv::aruco::CharucoParameters native_charuco_params = to_charuco_params(selected_charuco_params, camera_matrix, dist_coeffs);

        jyppx_ocv_aruco_charuco_detector* created = new (std::nothrow) jyppx_ocv_aruco_charuco_detector
        {
            cv::aruco::CharucoDetector(board->value, native_charuco_params, native_detector_params, native_refine_params)
        };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *detector = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)charuco_params;
        (void)camera_matrix;
        (void)dist_coeffs;
        (void)detector_params;
        (void)refine_params;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_aruco_charuco_detector_release_handle(jyppx_ocv_aruco_charuco_detector* detector)
{
    delete detector;
}

int jyppx_ocv_aruco_charuco_detector_get_board(
    const jyppx_ocv_aruco_charuco_detector* detector,
    jyppx_ocv_aruco_charuco_board** board)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_get_board";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (board == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "board");
        }

        *board = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *board = new (std::nothrow) jyppx_ocv_aruco_charuco_board{ detector->value.getBoard() };
        return *board == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_detector_set_board(
    jyppx_ocv_aruco_charuco_detector* detector,
    const jyppx_ocv_aruco_charuco_board* board)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_set_board";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_charuco_board(api_name, board);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setBoard(board->value);
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

int jyppx_ocv_aruco_charuco_detector_get_charuco_parameters(
    const jyppx_ocv_aruco_charuco_detector* detector,
    jyppx_ocv_aruco_charuco_params* params,
    jyppx_ocv_mat** camera_matrix,
    jyppx_ocv_mat** dist_coeffs)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_get_charuco_parameters";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }
        if (camera_matrix != nullptr) { *camera_matrix = nullptr; }
        if (dist_coeffs != nullptr) { *dist_coeffs = nullptr; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const cv::aruco::CharucoParameters native_params = detector->value.getCharucoParameters();
        *params = from_charuco_params(native_params);
        if (camera_matrix != nullptr && !native_params.cameraMatrix.empty())
        {
            *camera_matrix = new (std::nothrow) jyppx_ocv_mat{ native_params.cameraMatrix.clone() };
            if (*camera_matrix == nullptr)
            {
                return opencv_csharp_native::set_out_of_memory(api_name);
            }
        }

        if (dist_coeffs != nullptr && !native_params.distCoeffs.empty())
        {
            *dist_coeffs = new (std::nothrow) jyppx_ocv_mat{ native_params.distCoeffs.clone() };
            if (*dist_coeffs == nullptr)
            {
                if (camera_matrix != nullptr)
                {
                    delete *camera_matrix;
                    *camera_matrix = nullptr;
                }

                return opencv_csharp_native::set_out_of_memory(api_name);
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        set_default_charuco_params(params);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_detector_set_charuco_parameters(
    jyppx_ocv_aruco_charuco_detector* detector,
    const jyppx_ocv_aruco_charuco_params* params,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_set_charuco_parameters";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::aruco::CharucoParameters native_params = to_charuco_params(*params, camera_matrix, dist_coeffs);
        detector->value.setCharucoParameters(native_params);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)camera_matrix;
        (void)dist_coeffs;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_detector_get_detector_parameters(
    const jyppx_ocv_aruco_charuco_detector* detector,
    jyppx_ocv_aruco_detector_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_get_detector_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (params == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "params");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *params = from_detector_params(detector->value.getDetectorParameters());
        return OPENCV_CSHARP_STATUS_OK;
#else
        set_default_detector_params(params);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_charuco_detector_set_detector_parameters(
    jyppx_ocv_aruco_charuco_detector* detector,
    const jyppx_ocv_aruco_detector_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_set_detector_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (params == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "params");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setDetectorParameters(to_detector_params(*params));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_charuco_detector_get_refine_parameters(
    const jyppx_ocv_aruco_charuco_detector* detector,
    jyppx_ocv_aruco_refine_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_get_refine_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (params == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "params");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *params = from_refine_params(detector->value.getRefineParameters());
        return OPENCV_CSHARP_STATUS_OK;
#else
        set_default_refine_params(params);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_charuco_detector_set_refine_parameters(
    jyppx_ocv_aruco_charuco_detector* detector,
    const jyppx_ocv_aruco_refine_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_set_refine_parameters";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (params == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "params");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        detector->value.setRefineParameters(to_refine_params(*params));
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_charuco_detector_detect_board_count(
    const jyppx_ocv_aruco_charuco_detector* detector,
    const jyppx_ocv_mat* image,
    const int* input_marker_offsets,
    int input_marker_group_count,
    const jyppx_ocv_point2f* input_marker_points,
    int input_marker_point_count,
    const int* input_marker_ids,
    int input_marker_id_count,
    int* charuco_count,
    int* marker_count,
    int* marker_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_detect_board_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(api_name, input_marker_offsets, input_marker_group_count, input_marker_points, input_marker_point_count, "input_marker_offsets", "input_marker_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_ids(api_name, input_marker_ids, input_marker_id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (input_marker_group_count != input_marker_id_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "input_marker_ids");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Point2f> charuco_corners;
        std::vector<int> charuco_ids;
        std::vector<std::vector<cv::Point2f>> marker_corners = to_point2f_groups(input_marker_offsets, input_marker_group_count, input_marker_points, input_marker_point_count);
        std::vector<int> marker_ids = to_int_vector(input_marker_ids, input_marker_id_count);

        detector->value.detectBoard(opencv_csharp_native::mat_value(image), charuco_corners, charuco_ids, marker_corners, marker_ids);
        status = set_count_from_size(api_name, charuco_corners.size(), charuco_count, "charuco_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return set_point_group_counts(api_name, marker_corners, marker_count, marker_point_count);
#else
        if (charuco_count != nullptr) { *charuco_count = 0; }
        if (marker_count != nullptr) { *marker_count = 0; }
        if (marker_point_count != nullptr) { *marker_point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_detector_detect_board_fill(
    const jyppx_ocv_aruco_charuco_detector* detector,
    const jyppx_ocv_mat* image,
    const int* input_marker_offsets,
    int input_marker_group_count,
    const jyppx_ocv_point2f* input_marker_points,
    int input_marker_point_count,
    const int* input_marker_ids,
    int input_marker_id_count,
    jyppx_ocv_point2f* charuco_corners,
    int charuco_corner_capacity,
    int* charuco_ids,
    int charuco_id_capacity,
    int* marker_offsets,
    int marker_offset_capacity,
    jyppx_ocv_point2f* marker_corners,
    int marker_corner_capacity,
    int* marker_ids,
    int marker_id_capacity,
    int* charuco_count,
    int* marker_count,
    int* marker_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_detect_board_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_point2f_groups(api_name, input_marker_offsets, input_marker_group_count, input_marker_points, input_marker_point_count, "input_marker_offsets", "input_marker_points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_ids(api_name, input_marker_ids, input_marker_id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (input_marker_group_count != input_marker_id_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "input_marker_ids");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Point2f> native_charuco_corners;
        std::vector<int> native_charuco_ids;
        std::vector<std::vector<cv::Point2f>> native_marker_corners = to_point2f_groups(input_marker_offsets, input_marker_group_count, input_marker_points, input_marker_point_count);
        std::vector<int> native_marker_ids = to_int_vector(input_marker_ids, input_marker_id_count);

        detector->value.detectBoard(opencv_csharp_native::mat_value(image), native_charuco_corners, native_charuco_ids, native_marker_corners, native_marker_ids);
        status = copy_point2f_vector(api_name, native_charuco_corners, charuco_corners, charuco_corner_capacity, charuco_count, "charuco_corners");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = copy_ints(api_name, native_charuco_ids, charuco_ids, charuco_id_capacity, "charuco_ids");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = copy_point_groups(api_name, native_marker_corners, marker_offsets, marker_offset_capacity, marker_corners, marker_corner_capacity, marker_count, marker_point_count, "marker_offsets", "marker_corners");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_ints(api_name, native_marker_ids, marker_ids, marker_id_capacity, "marker_ids");
#else
        (void)charuco_corners;
        (void)charuco_corner_capacity;
        (void)charuco_ids;
        (void)charuco_id_capacity;
        (void)marker_offsets;
        (void)marker_offset_capacity;
        (void)marker_corners;
        (void)marker_corner_capacity;
        (void)marker_ids;
        (void)marker_id_capacity;
        if (charuco_count != nullptr) { *charuco_count = 0; }
        if (marker_count != nullptr) { *marker_count = 0; }
        if (marker_point_count != nullptr) { *marker_point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_aruco_charuco_detector_detect_diamonds_count(
    const jyppx_ocv_aruco_charuco_detector* detector,
    const jyppx_ocv_mat* image,
    const int* input_marker_offsets,
    int input_marker_count,
    const jyppx_ocv_point2f* input_marker_points,
    int input_marker_point_count,
    const int* input_marker_ids,
    int input_marker_id_count,
    int* diamond_count,
    int* diamond_point_count,
    int* marker_count,
    int* marker_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_detect_diamonds_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_point2f_groups(api_name, input_marker_offsets, input_marker_count, input_marker_points, input_marker_point_count, "input_marker_offsets", "input_marker_points");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_ids(api_name, input_marker_ids, input_marker_id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (input_marker_id_count != input_marker_count) return opencv_csharp_native::set_invalid_argument(api_name, "input_marker_id_count");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point2f>> diamonds;
        std::vector<cv::Vec4i> diamond_ids;
        auto markers = to_point2f_groups(input_marker_offsets, input_marker_count, input_marker_points, input_marker_point_count);
        auto marker_ids = to_int_vector(input_marker_ids, input_marker_id_count);
        detector->value.detectDiamonds(opencv_csharp_native::mat_value(image), diamonds, diamond_ids, markers, marker_ids);
        status = set_point_group_counts(api_name, diamonds, diamond_count, diamond_point_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return set_point_group_counts(api_name, markers, marker_count, marker_point_count);
#else
        if (diamond_count != nullptr) *diamond_count = 0;
        if (diamond_point_count != nullptr) *diamond_point_count = 0;
        if (marker_count != nullptr) *marker_count = 0;
        if (marker_point_count != nullptr) *marker_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_charuco_detector_detect_diamonds_fill(
    const jyppx_ocv_aruco_charuco_detector* detector,
    const jyppx_ocv_mat* image,
    const int* input_marker_offsets,
    int input_marker_count,
    const jyppx_ocv_point2f* input_marker_points,
    int input_marker_point_count,
    const int* input_marker_ids,
    int input_marker_id_count,
    int* diamond_offsets,
    int diamond_offset_capacity,
    jyppx_ocv_point2f* diamond_points,
    int diamond_point_capacity,
    int* diamond_ids,
    int diamond_id_capacity,
    int* marker_offsets,
    int marker_offset_capacity,
    jyppx_ocv_point2f* marker_points,
    int marker_point_capacity,
    int* marker_ids,
    int marker_id_capacity,
    int* diamond_count,
    int* diamond_point_count,
    int* marker_count,
    int* marker_point_count)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_charuco_detector_detect_diamonds_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_charuco_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_point2f_groups(api_name, input_marker_offsets, input_marker_count, input_marker_points, input_marker_point_count, "input_marker_offsets", "input_marker_points");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_ids(api_name, input_marker_ids, input_marker_id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (input_marker_id_count != input_marker_count) return opencv_csharp_native::set_invalid_argument(api_name, "input_marker_id_count");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<std::vector<cv::Point2f>> diamonds;
        std::vector<cv::Vec4i> native_diamond_ids;
        auto markers = to_point2f_groups(input_marker_offsets, input_marker_count, input_marker_points, input_marker_point_count);
        auto native_marker_ids = to_int_vector(input_marker_ids, input_marker_id_count);
        detector->value.detectDiamonds(opencv_csharp_native::mat_value(image), diamonds, native_diamond_ids, markers, native_marker_ids);
        status = copy_point_groups(api_name, diamonds, diamond_offsets, diamond_offset_capacity, diamond_points, diamond_point_capacity, diamond_count, diamond_point_count, "diamond_offsets", "diamond_points");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (native_diamond_ids.size() != diamonds.size() || native_diamond_ids.size() > static_cast<size_t>(std::numeric_limits<int>::max() / 4))
            return opencv_csharp_native::set_invalid_argument(api_name, "diamond_ids");
        const int required_diamond_ids = static_cast<int>(native_diamond_ids.size()) * 4;
        if (required_diamond_ids > 0 && (diamond_ids == nullptr || diamond_id_capacity < required_diamond_ids))
            return opencv_csharp_native::set_invalid_argument(api_name, "diamond_ids");
        for (int i = 0; i < *diamond_count; ++i)
            for (int j = 0; j < 4; ++j)
                diamond_ids[i * 4 + j] = native_diamond_ids[static_cast<size_t>(i)][j];
        status = copy_point_groups(api_name, markers, marker_offsets, marker_offset_capacity, marker_points, marker_point_capacity, marker_count, marker_point_count, "marker_offsets", "marker_points");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        return copy_ints(api_name, native_marker_ids, marker_ids, marker_id_capacity, "marker_ids");
#else
        (void)diamond_offsets; (void)diamond_offset_capacity; (void)diamond_points; (void)diamond_point_capacity;
        (void)diamond_ids; (void)diamond_id_capacity; (void)marker_offsets; (void)marker_offset_capacity;
        (void)marker_points; (void)marker_point_capacity; (void)marker_ids; (void)marker_id_capacity;
        if (diamond_count != nullptr) *diamond_count = 0;
        if (diamond_point_count != nullptr) *diamond_point_count = 0;
        if (marker_count != nullptr) *marker_count = 0;
        if (marker_point_count != nullptr) *marker_point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_draw_detected_corners_charuco(
    jyppx_ocv_mat* image,
    const jyppx_ocv_point2f* corners,
    int corner_count,
    const int* ids,
    int id_count,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_draw_detected_corners_charuco";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (corner_count < 0 || (corner_count > 0 && corners == nullptr)) return opencv_csharp_native::set_invalid_argument(api_name, "corners");
        status = validate_ids(api_name, ids, id_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (id_count != 0 && id_count != corner_count) return opencv_csharp_native::set_invalid_argument(api_name, "id_count");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::aruco::drawDetectedCornersCharuco(
            opencv_csharp_native::mat_value(image),
            to_point2f_vector(corners, corner_count),
            to_int_vector(ids, id_count),
            cv::Scalar(color_v0, color_v1, color_v2, color_v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color_v0; (void)color_v1; (void)color_v2; (void)color_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_aruco_draw_detected_diamonds(
    jyppx_ocv_mat* image,
    const int* diamond_offsets,
    int diamond_count,
    const jyppx_ocv_point2f* diamond_points,
    int diamond_point_count,
    const int* diamond_ids,
    int diamond_id_count,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3)
{
    constexpr const char* api_name = "jyppx_ocv_aruco_draw_detected_diamonds";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_point2f_groups(api_name, diamond_offsets, diamond_count, diamond_points, diamond_point_count, "diamond_offsets", "diamond_points");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (diamond_count < 0 || diamond_count > (std::numeric_limits<int>::max() / 4) || diamond_id_count < 0 ||
            (diamond_id_count > 0 && diamond_ids == nullptr) || (diamond_id_count != 0 && diamond_id_count != diamond_count * 4))
            return opencv_csharp_native::set_invalid_argument(api_name, "diamond_ids");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Vec4i> native_ids;
        native_ids.reserve(static_cast<size_t>(diamond_count));
        for (int i = 0; i < diamond_id_count; i += 4)
            native_ids.emplace_back(diamond_ids[i], diamond_ids[i + 1], diamond_ids[i + 2], diamond_ids[i + 3]);
        cv::aruco::drawDetectedDiamonds(
            opencv_csharp_native::mat_value(image),
            to_point2f_groups(diamond_offsets, diamond_count, diamond_points, diamond_point_count),
            native_ids,
            cv::Scalar(color_v0, color_v1, color_v2, color_v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color_v0; (void)color_v1; (void)color_v2; (void)color_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...) { return opencv_csharp_native::translate_current_exception(api_name); }
}

int jyppx_ocv_mcc_checker_create(jyppx_ocv_mcc_checker** checker)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (checker == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "checker");
        }

        *checker = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_mcc_checker* created = new (std::nothrow) jyppx_ocv_mcc_checker{ cv::mcc::CChecker::create() };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        if (created->value.empty())
        {
            delete created;
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "OpenCV returned an empty CChecker.");
        }

        *checker = created;
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

void jyppx_ocv_mcc_checker_release_handle(jyppx_ocv_mcc_checker* checker)
{
    delete checker;
}

int jyppx_ocv_mcc_checker_get_target(const jyppx_ocv_mcc_checker* checker, int* target)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_get_target";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, target, "target");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *target = static_cast<int>(checker->value->getTarget());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *target = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_set_target(jyppx_ocv_mcc_checker* checker, int target)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_set_target";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_color_chart_type(api_name, target);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        checker->value->setTarget(static_cast<cv::mcc::ColorChart>(target));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)target;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_get_box_count(const jyppx_ocv_mcc_checker* checker, int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_get_box_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return set_count_from_size(api_name, checker->value->getBox().size(), point_count, "point_count");
#else
        if (point_count != nullptr) { *point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_get_box_fill(
    const jyppx_ocv_mcc_checker* checker,
    jyppx_ocv_point2f* points,
    int point_capacity,
    int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_get_box_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_point2f_vector(api_name, checker->value->getBox(), points, point_capacity, point_count, "points");
#else
        (void)points;
        (void)point_capacity;
        if (point_count != nullptr) { *point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_set_box(
    jyppx_ocv_mcc_checker* checker,
    const jyppx_ocv_point2f* points,
    int point_count)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_set_box";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (point_count < 0 || (point_count > 0 && points == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "points");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        checker->value->setBox(to_point2f_vector(points, point_count));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)points;
        (void)point_count;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_get_color_charts_count(const jyppx_ocv_mcc_checker* checker, int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_get_color_charts_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return set_count_from_size(api_name, checker->value->getColorCharts().size(), point_count, "point_count");
#else
        if (point_count != nullptr) { *point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_get_color_charts_fill(
    const jyppx_ocv_mcc_checker* checker,
    jyppx_ocv_point2f* points,
    int point_capacity,
    int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_get_color_charts_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_point2f_vector(api_name, checker->value->getColorCharts(), points, point_capacity, point_count, "points");
#else
        (void)points;
        (void)point_capacity;
        if (point_count != nullptr) { *point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_get_charts_rgb(
    const jyppx_ocv_mcc_checker* checker,
    int get_stats,
    jyppx_ocv_mat** charts_rgb)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_get_charts_rgb";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (charts_rgb == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "charts_rgb");
        }
        *charts_rgb = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_mat_from_clone(api_name, checker->value->getChartsRGB(get_stats != 0), charts_rgb, "charts_rgb");
#else
        (void)get_stats;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_set_charts_rgb(
    jyppx_ocv_mcc_checker* checker,
    const jyppx_ocv_mat* charts_rgb)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_set_charts_rgb";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, charts_rgb, "charts_rgb");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        checker->value->setChartsRGB(opencv_csharp_native::mat_value(charts_rgb).clone());
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

int jyppx_ocv_mcc_checker_get_charts_ycbcr(
    const jyppx_ocv_mcc_checker* checker,
    jyppx_ocv_mat** charts_ycbcr)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_get_charts_ycbcr";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (charts_ycbcr == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "charts_ycbcr");
        }
        *charts_ycbcr = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_mat_from_clone(api_name, checker->value->getChartsYCbCr(), charts_ycbcr, "charts_ycbcr");
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_set_charts_ycbcr(
    jyppx_ocv_mcc_checker* checker,
    const jyppx_ocv_mat* charts_ycbcr)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_set_charts_ycbcr";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, charts_ycbcr, "charts_ycbcr");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        checker->value->setChartsYCbCr(opencv_csharp_native::mat_value(charts_ycbcr).clone());
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

int jyppx_ocv_mcc_checker_get_cost(const jyppx_ocv_mcc_checker* checker, float* cost)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_get_cost";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, cost, "cost");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *cost = checker->value->getCost();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *cost = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_set_cost(jyppx_ocv_mcc_checker* checker, float cost)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_set_cost";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        checker->value->setCost(cost);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)cost;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_get_center(
    const jyppx_ocv_mcc_checker* checker,
    jyppx_ocv_point2f* center)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_get_center";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (center == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "center");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        const cv::Point2f point = checker->value->getCenter();
        center->x = point.x;
        center->y = point.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        center->x = 0.0F;
        center->y = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_set_center(
    jyppx_ocv_mcc_checker* checker,
    jyppx_ocv_point2f center)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_set_center";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_value(api_name, checker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        checker->value->setCenter(cv::Point2f(center.x, center.y));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)center;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_create(jyppx_ocv_mcc_checker_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_mcc_checker_detector* created = new (std::nothrow) jyppx_ocv_mcc_checker_detector{ cv::mcc::CCheckerDetector::create() };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        if (created->value.empty())
        {
            delete created;
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "OpenCV returned an empty CCheckerDetector.");
        }

        *detector = created;
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

int jyppx_ocv_mcc_checker_detector_create_from_net(
    const jyppx_ocv_dnn_net* net,
    jyppx_ocv_mcc_checker_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_create_from_net";

    try
    {
        opencv_csharp_native::clear_last_error();
        if (net == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "net");
        }
        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }
        *detector = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_mcc_checker_detector* created = new (std::nothrow) jyppx_ocv_mcc_checker_detector{ cv::mcc::CCheckerDetector::create(net->value) };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }
        if (created->value.empty())
        {
            delete created;
            return opencv_csharp_native::set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, "OpenCV returned an empty CCheckerDetector.");
        }
        *detector = created;
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

void jyppx_ocv_mcc_checker_detector_release_handle(jyppx_ocv_mcc_checker_detector* detector)
{
    delete detector;
}

int jyppx_ocv_mcc_checker_detector_get_use_dnn_model(
    const jyppx_ocv_mcc_checker_detector* detector,
    int* use_dnn_model)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_get_use_dnn_model";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, use_dnn_model, "use_dnn_model");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *use_dnn_model = detector->value->getUseDnnModel() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *use_dnn_model = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_set_use_dnn_model(
    jyppx_ocv_mcc_checker_detector* detector,
    int use_dnn_model)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_set_use_dnn_model";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->setUseDnnModel(use_dnn_model != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)use_dnn_model;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_process(
    jyppx_ocv_mcc_checker_detector* detector,
    const jyppx_ocv_mat* image,
    int nc,
    int* detected)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_process";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, detected, "detected");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *detected = detector->value->process(opencv_csharp_native::mat_value(image), nc) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)nc;
        *detected = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_process_with_roi(
    jyppx_ocv_mcc_checker_detector* detector,
    const jyppx_ocv_mat* image,
    const int* rois,
    int roi_count,
    int nc,
    int* detected)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_process_with_roi";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_rects(api_name, rois, roi_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, detected, "detected");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::Rect> native_rois = to_rect_vector(rois, roi_count);
        *detected = detector->value->process(opencv_csharp_native::mat_value(image), native_rois, nc) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rois;
        (void)roi_count;
        (void)nc;
        *detected = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_get_best_color_checker(
    const jyppx_ocv_mcc_checker_detector* detector,
    jyppx_ocv_mcc_checker** checker,
    int* has_checker)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_get_best_color_checker";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (checker == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "checker");
        }
        status = validate_output_int(api_name, has_checker, "has_checker");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *checker = nullptr;
        *has_checker = 0;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        cv::Ptr<cv::mcc::CChecker> native_checker = detector->value->getBestColorChecker();
        if (native_checker.empty())
        {
            return OPENCV_CSHARP_STATUS_OK;
        }

        jyppx_ocv_mcc_checker* created = new (std::nothrow) jyppx_ocv_mcc_checker{ native_checker };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *checker = created;
        *has_checker = 1;
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

int jyppx_ocv_mcc_checker_detector_get_list_color_checker_count(
    const jyppx_ocv_mcc_checker_detector* detector,
    int* checker_count)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_get_list_color_checker_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return set_count_from_size(api_name, detector->value->getListColorChecker().size(), checker_count, "checker_count");
#else
        if (checker_count != nullptr) { *checker_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_get_list_color_checker_fill(
    const jyppx_ocv_mcc_checker_detector* detector,
    jyppx_ocv_mcc_checker** checkers,
    int checker_capacity,
    int* checker_count)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_get_list_color_checker_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::Ptr<cv::mcc::CChecker>> native_checkers = detector->value->getListColorChecker();
        status = set_count_from_size(api_name, native_checkers.size(), checker_count, "checker_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (*checker_count > 0 && (checkers == nullptr || checker_capacity < *checker_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "checkers");
        }

        int created_count = 0;
        for (int i = 0; i < *checker_count; ++i)
        {
            checkers[i] = new (std::nothrow) jyppx_ocv_mcc_checker{ native_checkers[static_cast<size_t>(i)] };
            if (checkers[i] == nullptr)
            {
                for (int j = 0; j < created_count; ++j)
                {
                    delete checkers[j];
                    checkers[j] = nullptr;
                }

                return opencv_csharp_native::set_out_of_memory(api_name);
            }

            ++created_count;
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)checkers;
        (void)checker_capacity;
        if (checker_count != nullptr) { *checker_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_draw(
    jyppx_ocv_mcc_checker_detector* detector,
    const jyppx_ocv_mcc_checker* const* checkers,
    int checker_count,
    jyppx_ocv_mat* image,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_draw";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (checker_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "checker_count");
        }
        if (checker_count > 0 && checkers == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "checkers");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        std::vector<cv::Ptr<cv::mcc::CChecker>> native_checkers;
        native_checkers.reserve(static_cast<size_t>(checker_count));
        for (int i = 0; i < checker_count; ++i)
        {
            if (!checker_is_valid(checkers[i]))
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "checkers");
            }

            native_checkers.push_back(checkers[i]->value);
        }

        detector->value->draw(native_checkers, opencv_csharp_native::mat_value(image), cv::Scalar(color_v0, color_v1, color_v2, color_v3), thickness);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)checkers;
        (void)checker_count;
        (void)color_v0;
        (void)color_v1;
        (void)color_v2;
        (void)color_v3;
        (void)thickness;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_get_ref_colors(
    const jyppx_ocv_mcc_checker_detector* detector,
    jyppx_ocv_mat** ref_colors)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_get_ref_colors";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (ref_colors == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "ref_colors");
        }
        *ref_colors = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return create_mat_from_clone(api_name, detector->value->getRefColors(), ref_colors, "ref_colors");
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_get_detection_params(
    const jyppx_ocv_mcc_checker_detector* detector,
    jyppx_ocv_mcc_detector_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_get_detection_params";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *params = from_mcc_params(detector->value->getDetectionParams());
        return OPENCV_CSHARP_STATUS_OK;
#else
        set_default_mcc_params(params);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_set_detection_params(
    jyppx_ocv_mcc_checker_detector* detector,
    const jyppx_ocv_mcc_detector_params* params)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_set_detection_params";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (params == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "params");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->setDetectionParams(to_mcc_params(*params));
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

int jyppx_ocv_mcc_checker_detector_get_color_chart_type(
    const jyppx_ocv_mcc_checker_detector* detector,
    int* chart_type)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_get_color_chart_type";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, chart_type, "chart_type");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        *chart_type = static_cast<int>(detector->value->getColorChartType());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *chart_type = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mcc_checker_detector_set_color_chart_type(
    jyppx_ocv_mcc_checker_detector* detector,
    int chart_type)
{
    constexpr const char* api_name = "jyppx_ocv_mcc_checker_detector_set_color_chart_type";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_mcc_checker_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_color_chart_type(api_name, chart_type);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        status = validate_mcc_checker_detector_value(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        detector->value->setColorChartType(static_cast<cv::mcc::ColorChart>(chart_type));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)chart_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

