#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_stereo_bm jyppx_ocv_stereo_bm;
typedef struct jyppx_ocv_stereo_sgbm jyppx_ocv_stereo_sgbm;
typedef struct jyppx_ocv_stereo_matcher jyppx_ocv_stereo_matcher;
typedef struct jyppx_ocv_calib3d_subdiv2d jyppx_ocv_calib3d_subdiv2d;

typedef struct jyppx_ocv_calib3d_point2f
{
    float x;
    float y;
} jyppx_ocv_calib3d_point2f;

typedef struct jyppx_ocv_calib3d_point3f
{
    float x;
    float y;
    float z;
} jyppx_ocv_calib3d_point3f;

typedef struct jyppx_ocv_calib3d_vec4f
{
    float v0;
    float v1;
    float v2;
    float v3;
} jyppx_ocv_calib3d_vec4f;

typedef struct jyppx_ocv_calib3d_vec6f
{
    float v0;
    float v1;
    float v2;
    float v3;
    float v4;
    float v5;
} jyppx_ocv_calib3d_vec6f;

typedef struct jyppx_ocv_calib3d_usac_params
{
    double confidence;
    int is_parallel;
    int lo_iterations;
    int lo_method;
    int lo_sample_size;
    int max_iterations;
    int neighbors_search;
    int random_generator_state;
    int sampler;
    int score;
    double threshold;
    int final_polisher;
    int final_polisher_iterations;
} jyppx_ocv_calib3d_usac_params;

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_create(
    jyppx_ocv_calib3d_subdiv2d** subdiv);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_create_rect(
    int x, int y, int width, int height, jyppx_ocv_calib3d_subdiv2d** subdiv);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_create_rect2f(
    float x, float y, float width, float height, jyppx_ocv_calib3d_subdiv2d** subdiv);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_calib3d_subdiv2d_release(
    jyppx_ocv_calib3d_subdiv2d* subdiv);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_init_delaunay(
    jyppx_ocv_calib3d_subdiv2d* subdiv, int x, int y, int width, int height);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_init_delaunay_rect2f(
    jyppx_ocv_calib3d_subdiv2d* subdiv, float x, float y, float width, float height);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_insert(
    jyppx_ocv_calib3d_subdiv2d* subdiv, float x, float y, int* vertex);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_insert_points(
    jyppx_ocv_calib3d_subdiv2d* subdiv, const jyppx_ocv_calib3d_point2f* points, int point_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_locate(
    jyppx_ocv_calib3d_subdiv2d* subdiv, float x, float y, int* location, int* edge, int* vertex);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_find_nearest(
    jyppx_ocv_calib3d_subdiv2d* subdiv, float x, float y, int* vertex, float* nearest_x, float* nearest_y);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_get_edge_list_count(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int* count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_get_edge_list_fill(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, jyppx_ocv_calib3d_vec4f* values, int capacity);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_get_leading_edge_list_count(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int* count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_get_leading_edge_list_fill(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int* values, int capacity);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_get_triangle_list_count(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int* count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_get_triangle_list_fill(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, jyppx_ocv_calib3d_vec6f* values, int capacity);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_get_voronoi_facet_list_count(
    jyppx_ocv_calib3d_subdiv2d* subdiv, const int* indices, int index_count, int* facet_count, int* point_count);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_get_voronoi_facet_list_fill(
    jyppx_ocv_calib3d_subdiv2d* subdiv,
    const int* indices,
    int index_count,
    int* facet_offsets,
    int facet_offset_capacity,
    jyppx_ocv_calib3d_point2f* points,
    int point_capacity,
    jyppx_ocv_calib3d_point2f* centers,
    int center_capacity);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_get_vertex(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int vertex, float* x, float* y, int* first_edge);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_get_edge(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int edge, int next_edge_type, int* related_edge);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_next_edge(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int edge, int* next_edge);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_rotate_edge(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int edge, int rotate, int* rotated_edge);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_sym_edge(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int edge, int* symmetric_edge);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_edge_org(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int edge, int* vertex, float* x, float* y);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_subdiv2d_edge_dst(
    const jyppx_ocv_calib3d_subdiv2d* subdiv, int edge, int* vertex, float* x, float* y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_usac_params_get_default(
    jyppx_ocv_calib3d_usac_params* params);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_homography_usac(
    const jyppx_ocv_mat* src_points,
    const jyppx_ocv_mat* dst_points,
    jyppx_ocv_mat* mask,
    const jyppx_ocv_calib3d_usac_params* params,
    jyppx_ocv_mat** homography);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_solve_pnp_ransac_usac(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    jyppx_ocv_mat* inliers,
    const jyppx_ocv_calib3d_usac_params* params,
    int* solved);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_fundamental_mat_usac(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    jyppx_ocv_mat* mask,
    const jyppx_ocv_calib3d_usac_params* params,
    jyppx_ocv_mat** fundamental);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_essential_mat_usac(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* camera_matrix1,
    const jyppx_ocv_mat* camera_matrix2,
    const jyppx_ocv_mat* dist_coeffs1,
    const jyppx_ocv_mat* dist_coeffs2,
    jyppx_ocv_mat* mask,
    const jyppx_ocv_calib3d_usac_params* params,
    jyppx_ocv_mat** essential);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_estimate_affine_2d_usac(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* transform,
    jyppx_ocv_mat* inliers,
    const jyppx_ocv_calib3d_usac_params* params);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_stereo_rectify(
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
    int new_image_width,
    int new_image_height,
    double balance,
    double fov_scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stereo_matcher_release(
    jyppx_ocv_stereo_matcher* stereo_matcher);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_compute(
    jyppx_ocv_stereo_matcher* stereo_matcher,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right,
    jyppx_ocv_mat* disparity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_get_min_disparity(const jyppx_ocv_stereo_matcher* stereo_matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_set_min_disparity(jyppx_ocv_stereo_matcher* stereo_matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_get_num_disparities(const jyppx_ocv_stereo_matcher* stereo_matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_set_num_disparities(jyppx_ocv_stereo_matcher* stereo_matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_get_block_size(const jyppx_ocv_stereo_matcher* stereo_matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_set_block_size(jyppx_ocv_stereo_matcher* stereo_matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_get_speckle_window_size(const jyppx_ocv_stereo_matcher* stereo_matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_set_speckle_window_size(jyppx_ocv_stereo_matcher* stereo_matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_get_speckle_range(const jyppx_ocv_stereo_matcher* stereo_matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_set_speckle_range(jyppx_ocv_stereo_matcher* stereo_matcher, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_get_disp12_max_diff(const jyppx_ocv_stereo_matcher* stereo_matcher, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_matcher_set_disp12_max_diff(jyppx_ocv_stereo_matcher* stereo_matcher, int value);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_rodrigues(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    jyppx_ocv_mat* jacobian);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_rq_decomp3x3(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* mtx_r,
    jyppx_ocv_mat* mtx_q,
    jyppx_ocv_mat* qx,
    jyppx_ocv_mat* qy,
    jyppx_ocv_mat* qz,
    double* euler_x,
    double* euler_y,
    double* euler_z);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_decompose_projection_matrix(
    const jyppx_ocv_mat* proj_matrix,
    jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* rot_matrix,
    jyppx_ocv_mat* trans_vect,
    jyppx_ocv_mat* rot_matrix_x,
    jyppx_ocv_mat* rot_matrix_y,
    jyppx_ocv_mat* rot_matrix_z,
    jyppx_ocv_mat* euler_angles);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_compose_rt(
    const jyppx_ocv_mat* rvec1,
    const jyppx_ocv_mat* tvec1,
    const jyppx_ocv_mat* rvec2,
    const jyppx_ocv_mat* tvec2,
    jyppx_ocv_mat* rvec3,
    jyppx_ocv_mat* tvec3);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_compose_rt_extended(
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
    jyppx_ocv_mat* dt3dt2);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_project_points(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* rvec,
    const jyppx_ocv_mat* tvec,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* image_points,
    jyppx_ocv_mat* jacobian,
    double aspect_ratio);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_project_points_separated_jacobians(
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
    double aspect_ratio);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_solve_pnp(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int use_extrinsic_guess,
    int flags,
    int* solved);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_solve_pnp_ransac(
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
    int* solved);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_project_points(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* rvec,
    const jyppx_ocv_mat* tvec,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* image_points,
    double alpha,
    jyppx_ocv_mat* jacobian);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_distort_points(
    const jyppx_ocv_mat* undistorted,
    jyppx_ocv_mat* distorted,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    double alpha);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_distort_points_with_camera_matrix(
    const jyppx_ocv_mat* undistorted,
    jyppx_ocv_mat* distorted,
    const jyppx_ocv_mat* undistorted_camera_matrix,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    double alpha);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_undistort_points(
    const jyppx_ocv_mat* distorted,
    jyppx_ocv_mat* undistorted,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_mat* r,
    const jyppx_ocv_mat* p,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_estimate_new_camera_matrix(
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    int image_width,
    int image_height,
    const jyppx_ocv_mat* r,
    jyppx_ocv_mat* new_camera_matrix,
    double balance,
    int new_image_width,
    int new_image_height,
    double fov_scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_solve_pnp(
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
    int* solved);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_solve_pnp_ransac(
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
    int* solved);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_homography(
    const jyppx_ocv_mat* src_points,
    const jyppx_ocv_mat* dst_points,
    int method,
    double ransac_reproj_threshold,
    jyppx_ocv_mat* mask,
    int max_iters,
    double confidence,
    jyppx_ocv_mat** homography);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_fundamental_mat(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    int method,
    double ransac_reproj_threshold,
    double confidence,
    int max_iters,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat** fundamental);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_essential_mat(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* camera_matrix,
    int method,
    double prob,
    double threshold,
    int max_iters,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat** essential);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_essential_mat_focal(
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
    jyppx_ocv_mat** essential);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_essential_mat_two_cameras(
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
    jyppx_ocv_mat** essential);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_decompose_essential_mat(
    const jyppx_ocv_mat* essential,
    jyppx_ocv_mat* r1,
    jyppx_ocv_mat* r2,
    jyppx_ocv_mat* t);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_recover_pose(
    const jyppx_ocv_mat* essential,
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    jyppx_ocv_mat* mask,
    int* inlier_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_recover_pose_focal(
    const jyppx_ocv_mat* essential,
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    double focal,
    double pp_x,
    double pp_y,
    jyppx_ocv_mat* mask,
    int* inlier_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_recover_pose_with_distance(
    const jyppx_ocv_mat* essential,
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* r,
    jyppx_ocv_mat* t,
    double distance_thresh,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat* triangulated_points,
    int* inlier_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_recover_pose_two_cameras(
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
    int* inlier_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_compute_correspond_epilines(
    const jyppx_ocv_mat* points,
    int which_image,
    const jyppx_ocv_mat* fundamental,
    jyppx_ocv_mat* lines);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_estimate_translation_3d(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* translation,
    jyppx_ocv_mat* inliers,
    double ransac_threshold,
    double confidence,
    int* found);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_estimate_translation_2d(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* inliers,
    int method,
    double ransac_reproj_threshold,
    int max_iters,
    double confidence,
    int refine_iters,
    double* translation_x,
    double* translation_y);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_estimate_affine_3d_ransac(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* transform,
    jyppx_ocv_mat* inliers,
    double ransac_threshold,
    double confidence,
    int* found);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_estimate_affine_3d_umeyama(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* transform,
    int force_rotation,
    double* scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_estimate_affine_2d(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* transform,
    jyppx_ocv_mat* inliers,
    int method,
    double ransac_reproj_threshold,
    int max_iters,
    double confidence,
    int refine_iters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_estimate_affine_partial_2d(
    const jyppx_ocv_mat* source,
    const jyppx_ocv_mat* destination,
    jyppx_ocv_mat* transform,
    jyppx_ocv_mat* inliers,
    int method,
    double ransac_reproj_threshold,
    int max_iters,
    double confidence,
    int refine_iters);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_decompose_homography_mat(
    const jyppx_ocv_mat* homography,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* const* rotations,
    jyppx_ocv_mat* const* translations,
    jyppx_ocv_mat* const* normals,
    int output_capacity,
    int* solution_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_filter_homography_decomp_by_visible_refpoints(
    const jyppx_ocv_mat* const* rotations,
    int rotation_count,
    const jyppx_ocv_mat* const* normals,
    int normal_count,
    const jyppx_ocv_mat* before_points,
    const jyppx_ocv_mat* after_points,
    jyppx_ocv_mat* possible_solutions,
    const jyppx_ocv_mat* points_mask);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_convert_points_to_homogeneous(
    const jyppx_ocv_mat* source,
    jyppx_ocv_mat* destination,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_convert_points_from_homogeneous(
    const jyppx_ocv_mat* source,
    jyppx_ocv_mat* destination,
    int dtype);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_correct_matches(
    const jyppx_ocv_mat* fundamental,
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    jyppx_ocv_mat* corrected_points1,
    jyppx_ocv_mat* corrected_points2);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_sampson_distance(
    const jyppx_ocv_mat* point1,
    const jyppx_ocv_mat* point2,
    const jyppx_ocv_mat* fundamental,
    double* distance);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_triangulate_points(
    const jyppx_ocv_mat* proj_matr1,
    const jyppx_ocv_mat* proj_matr2,
    const jyppx_ocv_mat* proj_points1,
    const jyppx_ocv_mat* proj_points2,
    jyppx_ocv_mat* points4d);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_undistort_points(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_mat* r,
    const jyppx_ocv_mat* p,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_undistort_image_points(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_filter_speckles(
    jyppx_ocv_mat* image,
    double new_value,
    int max_speckle_size,
    double max_difference,
    jyppx_ocv_mat* buffer);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_get_valid_disparity_roi(
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
    int* result_height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_validate_disparity(
    jyppx_ocv_mat* disparity,
    const jyppx_ocv_mat* cost,
    int min_disparity,
    int number_of_disparities,
    int disp12_max_difference);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_reproject_image_to_3d(
    const jyppx_ocv_mat* disparity,
    jyppx_ocv_mat* image3d,
    const jyppx_ocv_mat* q,
    int handle_missing_values,
    int ddepth);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_init_undistort_rectify_map(
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    const jyppx_ocv_mat* r,
    const jyppx_ocv_mat* new_camera_matrix,
    int size_width,
    int size_height,
    int m1type,
    jyppx_ocv_mat* map1,
    jyppx_ocv_mat* map2);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_stereo_rectify(
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
    int* roi2_height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_solve_pnp_generic(
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
    int* solution_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_solve_p3p(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvecs,
    jyppx_ocv_mat* tvecs,
    int flags,
    int* solution_count);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_mat_mul_deriv(
    const jyppx_ocv_mat* a,
    const jyppx_ocv_mat* b,
    jyppx_ocv_mat* d_ab_d_a,
    jyppx_ocv_mat* d_ab_d_b);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_solve_pnp_refine_lm(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_solve_pnp_refine_vvs(
    const jyppx_ocv_mat* object_points,
    const jyppx_ocv_mat* image_points,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* dist_coeffs,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    double vvs_lambda);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_chessboard_corners(
    const jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    jyppx_ocv_mat* corners,
    int flags,
    int* found);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_chessboard_corners_sb(
    const jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    jyppx_ocv_mat* corners,
    int flags,
    int* found);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_chessboard_corners_sb_with_meta(
    const jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    jyppx_ocv_mat* corners,
    int flags,
    jyppx_ocv_mat* meta,
    int* found);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_estimate_chessboard_sharpness(
    const jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    const jyppx_ocv_mat* corners,
    float rise_distance,
    int vertical,
    jyppx_ocv_mat* sharpness,
    double* value0,
    double* value1,
    double* value2,
    double* value3);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_4_quad_corner_subpix(
    const jyppx_ocv_mat* image,
    jyppx_ocv_mat* corners,
    int region_width,
    int region_height,
    int* found);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_check_chessboard(
    const jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    int* found);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_find_circles_grid(
    const jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    jyppx_ocv_mat* centers,
    int flags,
    int* found);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_draw_chessboard_corners(
    jyppx_ocv_mat* image,
    int pattern_width,
    int pattern_height,
    const jyppx_ocv_mat* corners,
    int pattern_was_found);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_get_default_new_camera_matrix(
    const jyppx_ocv_mat* camera_matrix,
    int image_width,
    int image_height,
    int center_principal_point,
    jyppx_ocv_mat* new_camera_matrix);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_get_undistort_rectangles(
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
    double* outer_height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_get_optimal_new_camera_matrix(
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
    jyppx_ocv_mat** new_camera_matrix);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_calibration_matrix_values(
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
    double* aspect_ratio);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_stereo_rectify_uncalibrated(
    const jyppx_ocv_mat* points1,
    const jyppx_ocv_mat* points2,
    const jyppx_ocv_mat* fundamental,
    int image_width,
    int image_height,
    jyppx_ocv_mat* h1,
    jyppx_ocv_mat* h2,
    double threshold,
    int* success);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_calibrate_hand_eye(
    const jyppx_ocv_mat* const* r_gripper2base,
    const jyppx_ocv_mat* const* t_gripper2base,
    const jyppx_ocv_mat* const* r_target2cam,
    const jyppx_ocv_mat* const* t_target2cam,
    int pose_count,
    jyppx_ocv_mat* r_cam2gripper,
    jyppx_ocv_mat* t_cam2gripper,
    int method);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_calibrate_robot_world_hand_eye(
    const jyppx_ocv_mat* const* r_world2cam,
    const jyppx_ocv_mat* const* t_world2cam,
    const jyppx_ocv_mat* const* r_base2gripper,
    const jyppx_ocv_mat* const* t_base2gripper,
    int pose_count,
    jyppx_ocv_mat* r_base2world,
    jyppx_ocv_mat* t_base2world,
    jyppx_ocv_mat* r_gripper2cam,
    jyppx_ocv_mat* t_gripper2cam,
    int method);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_init_camera_matrix_2d(
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
    jyppx_ocv_mat* camera_matrix);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_calibrate_camera(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_calibrate_camera_extended(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_calibrate_camera_ro(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_calibrate_camera_ro_extended(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_stereo_calibrate(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_stereo_calibrate_extended(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_calibrate(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_stereo_calibrate(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_fisheye_stereo_calibrate_extended(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_register_cameras(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_register_cameras_extended(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_calibrate_multiview(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_calibrate_multiview_extended(
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
    double* reprojection_error);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_calib3d_rectify3_collinear(
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
    float* scale);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_create(
    int num_disparities,
    int block_size,
    jyppx_ocv_stereo_bm** stereo_bm);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stereo_bm_release(
    jyppx_ocv_stereo_bm* stereo_bm);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_compute(
    jyppx_ocv_stereo_bm* stereo_bm,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right,
    jyppx_ocv_mat* disparity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_min_disparity(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_min_disparity(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_num_disparities(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_num_disparities(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_block_size(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_block_size(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_speckle_window_size(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_speckle_window_size(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_speckle_range(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_speckle_range(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_disp12_max_diff(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_disp12_max_diff(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_pre_filter_type(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_pre_filter_type(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_pre_filter_size(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_pre_filter_size(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_pre_filter_cap(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_pre_filter_cap(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_texture_threshold(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_texture_threshold(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_uniqueness_ratio(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_uniqueness_ratio(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_smaller_block_size(const jyppx_ocv_stereo_bm* stereo_bm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_smaller_block_size(jyppx_ocv_stereo_bm* stereo_bm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_roi1(const jyppx_ocv_stereo_bm* stereo_bm, int* x, int* y, int* width, int* height);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_roi1(jyppx_ocv_stereo_bm* stereo_bm, int x, int y, int width, int height);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_get_roi2(const jyppx_ocv_stereo_bm* stereo_bm, int* x, int* y, int* width, int* height);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_bm_set_roi2(jyppx_ocv_stereo_bm* stereo_bm, int x, int y, int width, int height);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_create(
    int min_disparity,
    int num_disparities,
    int block_size,
    int p1,
    int p2,
    int disp12_max_diff,
    int pre_filter_cap,
    int uniqueness_ratio,
    int speckle_window_size,
    int speckle_range,
    int mode,
    jyppx_ocv_stereo_sgbm** stereo_sgbm);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_stereo_sgbm_release(
    jyppx_ocv_stereo_sgbm* stereo_sgbm);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_compute(
    jyppx_ocv_stereo_sgbm* stereo_sgbm,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right,
    jyppx_ocv_mat* disparity);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_min_disparity(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_min_disparity(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_num_disparities(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_num_disparities(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_block_size(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_block_size(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_speckle_window_size(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_speckle_window_size(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_speckle_range(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_speckle_range(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_disp12_max_diff(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_disp12_max_diff(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_pre_filter_cap(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_pre_filter_cap(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_uniqueness_ratio(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_uniqueness_ratio(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_p1(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_p1(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_p2(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_p2(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_get_mode(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_stereo_sgbm_set_mode(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value);
