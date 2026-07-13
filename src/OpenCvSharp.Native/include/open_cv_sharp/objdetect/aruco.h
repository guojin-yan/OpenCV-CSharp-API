#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_aruco_dictionary jyppx_ocv_aruco_dictionary;
typedef struct jyppx_ocv_aruco_detector jyppx_ocv_aruco_detector;
typedef struct jyppx_ocv_aruco_grid_board jyppx_ocv_aruco_grid_board;
typedef struct jyppx_ocv_aruco_charuco_board jyppx_ocv_aruco_charuco_board;
typedef struct jyppx_ocv_aruco_charuco_detector jyppx_ocv_aruco_charuco_detector;
typedef struct jyppx_ocv_mcc_checker jyppx_ocv_mcc_checker;
typedef struct jyppx_ocv_mcc_checker_detector jyppx_ocv_mcc_checker_detector;

typedef struct jyppx_ocv_point2f
{
    float x;
    float y;
} jyppx_ocv_point2f;

typedef struct jyppx_ocv_point3f
{
    float x;
    float y;
    float z;
} jyppx_ocv_point3f;

typedef struct jyppx_ocv_aruco_detector_params
{
    int adaptive_thresh_win_size_min;
    int adaptive_thresh_win_size_max;
    int adaptive_thresh_win_size_step;
    double adaptive_thresh_constant;
    double min_marker_perimeter_rate;
    double max_marker_perimeter_rate;
    double polygonal_approx_accuracy_rate;
    double min_corner_distance_rate;
    int min_distance_to_border;
    double min_marker_distance_rate;
    float min_group_distance;
    int corner_refinement_method;
    int corner_refinement_win_size;
    float relative_corner_refinement_win_size;
    int corner_refinement_max_iterations;
    double corner_refinement_min_accuracy;
    int marker_border_bits;
    int perspective_remove_pixel_per_cell;
    double perspective_remove_ignored_margin_per_cell;
    double max_erroneous_bits_in_border_rate;
    double min_otsu_std_dev;
    double error_correction_rate;
    float april_tag_quad_decimate;
    float april_tag_quad_sigma;
    int april_tag_min_cluster_pixels;
    int april_tag_max_nmaxima;
    float april_tag_critical_rad;
    float april_tag_max_line_fit_mse;
    int april_tag_min_white_black_diff;
    int april_tag_deglitch;
    int detect_inverted_marker;
    int use_aruco3_detection;
    int min_side_length_canonical_img;
    float min_marker_length_ratio_original_img;
    float valid_bit_id_threshold;
} jyppx_ocv_aruco_detector_params;

typedef struct jyppx_ocv_aruco_refine_params
{
    float min_rep_distance;
    float error_correction_rate;
    int check_all_orders;
} jyppx_ocv_aruco_refine_params;

typedef struct jyppx_ocv_aruco_charuco_params
{
    int min_markers;
    int try_refine_markers;
    int check_markers;
} jyppx_ocv_aruco_charuco_params;

typedef struct jyppx_ocv_mcc_detector_params
{
    int adaptive_thresh_win_size_min;
    int adaptive_thresh_win_size_max;
    int adaptive_thresh_win_size_step;
    double adaptive_thresh_constant;
    double min_contours_area_rate;
    double min_contours_area;
    double confidence_threshold;
    double min_contour_solidity;
    double find_candidates_approx_poly_dp_eps_multiplier;
    int border_width;
    float b0_factor;
    float max_error;
    int min_contour_points_allowed;
    int min_contour_length_allowed;
    int min_inter_contour_distance;
    int min_inter_checker_distance;
    int min_image_size;
    int min_group_size;
} jyppx_ocv_mcc_detector_params;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_create_default(
    jyppx_ocv_aruco_dictionary** dictionary);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_create_predefined(
    int dictionary_id,
    jyppx_ocv_aruco_dictionary** dictionary);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_create_from_bytes_list(
    const jyppx_ocv_mat* bytes_list,
    int marker_size,
    int max_correction_bits,
    jyppx_ocv_aruco_dictionary** dictionary);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_aruco_dictionary_release_handle(
    jyppx_ocv_aruco_dictionary* dictionary);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_get_bytes_list(
    const jyppx_ocv_aruco_dictionary* dictionary,
    jyppx_ocv_mat** bytes_list);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_set_bytes_list(
    jyppx_ocv_aruco_dictionary* dictionary,
    const jyppx_ocv_mat* bytes_list);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_get_marker_size(
    const jyppx_ocv_aruco_dictionary* dictionary,
    int* marker_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_set_marker_size(
    jyppx_ocv_aruco_dictionary* dictionary,
    int marker_size);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_get_max_correction_bits(
    const jyppx_ocv_aruco_dictionary* dictionary,
    int* max_correction_bits);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_set_max_correction_bits(
    jyppx_ocv_aruco_dictionary* dictionary,
    int max_correction_bits);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_identify(
    const jyppx_ocv_aruco_dictionary* dictionary,
    const jyppx_ocv_mat* bits,
    double max_correction_rate,
    int* identified,
    int* index,
    int* rotation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_identify_with_threshold(
    const jyppx_ocv_aruco_dictionary* dictionary,
    const jyppx_ocv_mat* cell_pixel_ratio,
    double max_correction_rate,
    float valid_bit_id_threshold,
    int* identified,
    int* index,
    int* rotation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_get_distance_to_id(
    const jyppx_ocv_aruco_dictionary* dictionary,
    const jyppx_ocv_mat* bits,
    int id,
    int all_rotations,
    int* distance);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_generate_image_marker(
    const jyppx_ocv_aruco_dictionary* dictionary,
    int id,
    int side_pixels,
    jyppx_ocv_mat* image,
    int border_bits);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_get_marker_bits(
    const jyppx_ocv_aruco_dictionary* dictionary,
    int marker_id,
    int rotation_id,
    jyppx_ocv_mat** bits);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_get_byte_list_from_bits(
    const jyppx_ocv_mat* bits,
    jyppx_ocv_mat** byte_list);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_dictionary_get_bits_from_byte_list(
    const jyppx_ocv_mat* byte_list,
    int marker_size,
    int rotation_id,
    jyppx_ocv_mat** bits);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_default_params(
    jyppx_ocv_aruco_detector_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_refine_default_params(
    jyppx_ocv_aruco_refine_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_detector_default_params(
    jyppx_ocv_mcc_detector_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_default_params(
    jyppx_ocv_aruco_charuco_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_create(
    const jyppx_ocv_aruco_dictionary* dictionary,
    const jyppx_ocv_aruco_detector_params* detector_params,
    const jyppx_ocv_aruco_refine_params* refine_params,
    jyppx_ocv_aruco_detector** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_aruco_detector_release_handle(
    jyppx_ocv_aruco_detector* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_get_dictionary(
    const jyppx_ocv_aruco_detector* detector,
    jyppx_ocv_aruco_dictionary** dictionary);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_set_dictionary(
    jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_aruco_dictionary* dictionary);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_get_detector_parameters(
    const jyppx_ocv_aruco_detector* detector,
    jyppx_ocv_aruco_detector_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_set_detector_parameters(
    jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_aruco_detector_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_get_refine_parameters(
    const jyppx_ocv_aruco_detector* detector,
    jyppx_ocv_aruco_refine_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_set_refine_parameters(
    jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_aruco_refine_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_detect_markers_count(
    const jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_mat* image,
    int* marker_count,
    int* corner_point_count,
    int* rejected_count,
    int* rejected_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_detect_markers_fill(
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
    int* rejected_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_detect_markers_with_confidence_count(
    const jyppx_ocv_aruco_detector* detector,
    const jyppx_ocv_mat* image,
    int* marker_count,
    int* corner_point_count,
    int* rejected_count,
    int* rejected_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_detect_markers_with_confidence_fill(
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
    int* rejected_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_refine_detected_markers_count(
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
    int* recovered_index_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_detector_refine_detected_markers_fill(
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
    int* recovered_index_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_grid_board_create(
    int markers_x,
    int markers_y,
    float marker_length,
    float marker_separation,
    const jyppx_ocv_aruco_dictionary* dictionary,
    const int* ids,
    int id_count,
    jyppx_ocv_aruco_grid_board** board);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_aruco_grid_board_release_handle(
    jyppx_ocv_aruco_grid_board* board);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_grid_board_get_grid_size(
    const jyppx_ocv_aruco_grid_board* board,
    int* markers_x,
    int* markers_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_grid_board_get_marker_length(
    const jyppx_ocv_aruco_grid_board* board,
    float* marker_length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_grid_board_get_marker_separation(
    const jyppx_ocv_aruco_grid_board* board,
    float* marker_separation);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_grid_board_generate_image(
    const jyppx_ocv_aruco_grid_board* board,
    int width,
    int height,
    jyppx_ocv_mat* image,
    int margin_size,
    int border_bits);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_board_create(
    int squares_x,
    int squares_y,
    float square_length,
    float marker_length,
    const jyppx_ocv_aruco_dictionary* dictionary,
    const int* ids,
    int id_count,
    jyppx_ocv_aruco_charuco_board** board);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_aruco_charuco_board_release_handle(
    jyppx_ocv_aruco_charuco_board* board);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_board_get_chessboard_size(
    const jyppx_ocv_aruco_charuco_board* board,
    int* squares_x,
    int* squares_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_board_get_square_length(
    const jyppx_ocv_aruco_charuco_board* board,
    float* square_length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_board_get_marker_length(
    const jyppx_ocv_aruco_charuco_board* board,
    float* marker_length);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_board_get_legacy_pattern(
    const jyppx_ocv_aruco_charuco_board* board,
    int* legacy_pattern);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_board_set_legacy_pattern(
    jyppx_ocv_aruco_charuco_board* board,
    int legacy_pattern);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_board_get_chessboard_corners_count(
    const jyppx_ocv_aruco_charuco_board* board,
    int* corner_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_board_get_chessboard_corners_fill(
    const jyppx_ocv_aruco_charuco_board* board,
    jyppx_ocv_point3f* corners,
    int corner_capacity,
    int* corner_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_board_check_corners_collinear(
    const jyppx_ocv_aruco_charuco_board* board,
    const int* charuco_ids,
    int charuco_id_count,
    int* collinear);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_board_generate_image(
    const jyppx_ocv_aruco_charuco_board* board,
    int width,
    int height,
    jyppx_ocv_mat* image,
    int margin_size,
    int border_bits);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_detector_create(
    const jyppx_ocv_aruco_charuco_board* board,
    const jyppx_ocv_aruco_charuco_params* charuco_params,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_aruco_detector_params* detector_params,
    const jyppx_ocv_aruco_refine_params* refine_params,
    jyppx_ocv_aruco_charuco_detector** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_aruco_charuco_detector_release_handle(
    jyppx_ocv_aruco_charuco_detector* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_detector_get_board(
    const jyppx_ocv_aruco_charuco_detector* detector,
    jyppx_ocv_aruco_charuco_board** board);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_detector_set_board(
    jyppx_ocv_aruco_charuco_detector* detector,
    const jyppx_ocv_aruco_charuco_board* board);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_detector_get_charuco_parameters(
    const jyppx_ocv_aruco_charuco_detector* detector,
    jyppx_ocv_aruco_charuco_params* params,
    jyppx_ocv_mat** camera_matrix,
    jyppx_ocv_mat** dist_coeffs);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_detector_set_charuco_parameters(
    jyppx_ocv_aruco_charuco_detector* detector,
    const jyppx_ocv_aruco_charuco_params* params,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_detector_detect_board_count(
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
    int* marker_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_aruco_charuco_detector_detect_board_fill(
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
    int* marker_point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_create(
    jyppx_ocv_mcc_checker** checker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_mcc_checker_release_handle(
    jyppx_ocv_mcc_checker* checker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_get_target(
    const jyppx_ocv_mcc_checker* checker,
    int* target);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_set_target(
    jyppx_ocv_mcc_checker* checker,
    int target);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_get_box_count(
    const jyppx_ocv_mcc_checker* checker,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_get_box_fill(
    const jyppx_ocv_mcc_checker* checker,
    jyppx_ocv_point2f* points,
    int point_capacity,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_set_box(
    jyppx_ocv_mcc_checker* checker,
    const jyppx_ocv_point2f* points,
    int point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_get_color_charts_count(
    const jyppx_ocv_mcc_checker* checker,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_get_color_charts_fill(
    const jyppx_ocv_mcc_checker* checker,
    jyppx_ocv_point2f* points,
    int point_capacity,
    int* point_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_get_charts_rgb(
    const jyppx_ocv_mcc_checker* checker,
    int get_stats,
    jyppx_ocv_mat** charts_rgb);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_set_charts_rgb(
    jyppx_ocv_mcc_checker* checker,
    const jyppx_ocv_mat* charts_rgb);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_get_charts_ycbcr(
    const jyppx_ocv_mcc_checker* checker,
    jyppx_ocv_mat** charts_ycbcr);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_set_charts_ycbcr(
    jyppx_ocv_mcc_checker* checker,
    const jyppx_ocv_mat* charts_ycbcr);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_get_cost(
    const jyppx_ocv_mcc_checker* checker,
    float* cost);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_set_cost(
    jyppx_ocv_mcc_checker* checker,
    float cost);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_get_center(
    const jyppx_ocv_mcc_checker* checker,
    jyppx_ocv_point2f* center);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_set_center(
    jyppx_ocv_mcc_checker* checker,
    jyppx_ocv_point2f center);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_create(
    jyppx_ocv_mcc_checker_detector** detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_mcc_checker_detector_release_handle(
    jyppx_ocv_mcc_checker_detector* detector);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_process(
    jyppx_ocv_mcc_checker_detector* detector,
    const jyppx_ocv_mat* image,
    int nc,
    int* detected);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_process_with_roi(
    jyppx_ocv_mcc_checker_detector* detector,
    const jyppx_ocv_mat* image,
    const int* rois,
    int roi_count,
    int nc,
    int* detected);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_get_best_color_checker(
    const jyppx_ocv_mcc_checker_detector* detector,
    jyppx_ocv_mcc_checker** checker,
    int* has_checker);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_get_list_color_checker_count(
    const jyppx_ocv_mcc_checker_detector* detector,
    int* checker_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_get_list_color_checker_fill(
    const jyppx_ocv_mcc_checker_detector* detector,
    jyppx_ocv_mcc_checker** checkers,
    int checker_capacity,
    int* checker_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_draw(
    jyppx_ocv_mcc_checker_detector* detector,
    const jyppx_ocv_mcc_checker* const* checkers,
    int checker_count,
    jyppx_ocv_mat* image,
    double color_v0,
    double color_v1,
    double color_v2,
    double color_v3,
    int thickness);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_get_ref_colors(
    const jyppx_ocv_mcc_checker_detector* detector,
    jyppx_ocv_mat** ref_colors);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_get_detection_params(
    const jyppx_ocv_mcc_checker_detector* detector,
    jyppx_ocv_mcc_detector_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_set_detection_params(
    jyppx_ocv_mcc_checker_detector* detector,
    const jyppx_ocv_mcc_detector_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_get_color_chart_type(
    const jyppx_ocv_mcc_checker_detector* detector,
    int* chart_type);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_mcc_checker_detector_set_color_chart_type(
    jyppx_ocv_mcc_checker_detector* detector,
    int chart_type);
