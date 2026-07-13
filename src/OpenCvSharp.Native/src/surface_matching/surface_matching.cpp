#include "open_cv_sharp/surface_matching/surface_matching.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "surface_matching_handles.h"

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

    int validate_icp(const char* api_name, const jyppx_ocv_surface_matching_icp* icp)
    {
        return icp == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "icp")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_detector(const char* api_name, const jyppx_ocv_surface_matching_ppf_3d_detector* detector)
    {
        return detector == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "detector")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_pointer(const char* api_name, const void* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
    void fill_pose_matrix(const cv::Matx44d& pose, double* destination)
    {
        for (int row = 0; row < 4; ++row)
        {
            for (int col = 0; col < 4; ++col)
            {
                destination[row * 4 + col] = pose(row, col);
            }
        }
    }

    void fill_pose_result(const cv::ppf_match_3d::Pose3DPtr& pose, jyppx_ocv_surface_matching_pose_3d_result* destination)
    {
        destination->alpha = pose->alpha;
        destination->residual = pose->residual;
        destination->model_index = static_cast<uint64_t>(pose->modelIndex);
        destination->num_votes = static_cast<uint64_t>(pose->numVotes);
        destination->angle = pose->angle;
        destination->t0 = pose->t[0];
        destination->t1 = pose->t[1];
        destination->t2 = pose->t[2];
        destination->q0 = pose->q[0];
        destination->q1 = pose->q[1];
        destination->q2 = pose->q[2];
        destination->q3 = pose->q[3];
        destination->pose00 = pose->pose(0, 0);
        destination->pose01 = pose->pose(0, 1);
        destination->pose02 = pose->pose(0, 2);
        destination->pose03 = pose->pose(0, 3);
        destination->pose10 = pose->pose(1, 0);
        destination->pose11 = pose->pose(1, 1);
        destination->pose12 = pose->pose(1, 2);
        destination->pose13 = pose->pose(1, 3);
        destination->pose20 = pose->pose(2, 0);
        destination->pose21 = pose->pose(2, 1);
        destination->pose22 = pose->pose(2, 2);
        destination->pose23 = pose->pose(2, 3);
        destination->pose30 = pose->pose(3, 0);
        destination->pose31 = pose->pose(3, 1);
        destination->pose32 = pose->pose(3, 2);
        destination->pose33 = pose->pose(3, 3);
    }

    void match_detector(
        jyppx_ocv_surface_matching_ppf_3d_detector* detector,
        const jyppx_ocv_mat* scene,
        double relative_scene_sample_step,
        double relative_scene_distance,
        std::vector<cv::ppf_match_3d::Pose3DPtr>& results)
    {
        detector->value->match(
            opencv_csharp_native::mat_value(scene),
            results,
            relative_scene_sample_step,
            relative_scene_distance);
    }
#endif
}

int jyppx_ocv_surface_matching_icp_create(
    int iterations,
    float tolerance,
    float rejection_scale,
    int num_levels,
    int sample_type,
    int num_max_corr,
    jyppx_ocv_surface_matching_icp** icp)
{
    constexpr const char* api_name = "jyppx_ocv_surface_matching_icp_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (icp == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "icp");
        }

        *icp = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
        jyppx_ocv_surface_matching_icp* created = new (std::nothrow) jyppx_ocv_surface_matching_icp{
            cv::ppf_match_3d::ICP(iterations, tolerance, rejection_scale, num_levels, sample_type, num_max_corr)
        };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *icp = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)iterations; (void)tolerance; (void)rejection_scale; (void)num_levels; (void)sample_type; (void)num_max_corr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_surface_matching_icp_release(jyppx_ocv_surface_matching_icp* icp)
{
    delete icp;
}

int jyppx_ocv_surface_matching_icp_register_model_to_scene(
    jyppx_ocv_surface_matching_icp* icp,
    const jyppx_ocv_mat* src_pc,
    const jyppx_ocv_mat* dst_pc,
    int* result_code,
    double* residual,
    double* pose16,
    int pose16_capacity)
{
    constexpr const char* api_name = "jyppx_ocv_surface_matching_icp_register_model_to_scene";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_icp(api_name, icp);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src_pc, "src_pc");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst_pc, "dst_pc");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, result_code, "result_code");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, residual, "residual");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, pose16, "pose16");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (pose16_capacity < 16)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "pose16_capacity");
        }

        *result_code = 0;
        *residual = 0.0;
        for (int i = 0; i < 16; ++i)
        {
            pose16[i] = 0.0;
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
        cv::Matx44d pose = cv::Matx44d::eye();
        *result_code = icp->value.registerModelToScene(
            opencv_csharp_native::mat_value(src_pc),
            opencv_csharp_native::mat_value(dst_pc),
            *residual,
            pose);
        fill_pose_matrix(pose, pose16);
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

int jyppx_ocv_surface_matching_ppf_3d_detector_create(
    double relative_sampling_step,
    double relative_distance_step,
    double num_angles,
    jyppx_ocv_surface_matching_ppf_3d_detector** detector)
{
    constexpr const char* api_name = "jyppx_ocv_surface_matching_ppf_3d_detector_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (detector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "detector");
        }

        *detector = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
        jyppx_ocv_surface_matching_ppf_3d_detector* created = new (std::nothrow) jyppx_ocv_surface_matching_ppf_3d_detector();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::makePtr<cv::ppf_match_3d::PPF3DDetector>(
            relative_sampling_step,
            relative_distance_step,
            num_angles);
        *detector = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)relative_sampling_step; (void)relative_distance_step; (void)num_angles;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_surface_matching_ppf_3d_detector_release(jyppx_ocv_surface_matching_ppf_3d_detector* detector)
{
    delete detector;
}

int jyppx_ocv_surface_matching_ppf_3d_detector_set_search_params(
    jyppx_ocv_surface_matching_ppf_3d_detector* detector,
    double position_threshold,
    double rotation_threshold,
    int use_weighted_clustering)
{
    constexpr const char* api_name = "jyppx_ocv_surface_matching_ppf_3d_detector_set_search_params";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
        detector->value->setSearchParams(position_threshold, rotation_threshold, use_weighted_clustering != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)position_threshold; (void)rotation_threshold; (void)use_weighted_clustering;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_surface_matching_ppf_3d_detector_train_model(
    jyppx_ocv_surface_matching_ppf_3d_detector* detector,
    const jyppx_ocv_mat* model)
{
    constexpr const char* api_name = "jyppx_ocv_surface_matching_ppf_3d_detector_train_model";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, model, "model");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
        detector->value->trainModel(opencv_csharp_native::mat_value(model));
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

int jyppx_ocv_surface_matching_ppf_3d_detector_match_count(
    jyppx_ocv_surface_matching_ppf_3d_detector* detector,
    const jyppx_ocv_mat* scene,
    double relative_scene_sample_step,
    double relative_scene_distance,
    int* result_count)
{
    constexpr const char* api_name = "jyppx_ocv_surface_matching_ppf_3d_detector_match_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, scene, "scene");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, result_count, "result_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        *result_count = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
        std::vector<cv::ppf_match_3d::Pose3DPtr> results;
        match_detector(detector, scene, relative_scene_sample_step, relative_scene_distance, results);
        *result_count = static_cast<int>(results.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)relative_scene_sample_step; (void)relative_scene_distance;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_surface_matching_ppf_3d_detector_match_fill(
    jyppx_ocv_surface_matching_ppf_3d_detector* detector,
    const jyppx_ocv_mat* scene,
    double relative_scene_sample_step,
    double relative_scene_distance,
    jyppx_ocv_surface_matching_pose_3d_result* results,
    int result_capacity,
    int* result_count)
{
    constexpr const char* api_name = "jyppx_ocv_surface_matching_ppf_3d_detector_match_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_detector(api_name, detector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, scene, "scene");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, result_count, "result_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (result_capacity < 0 || (result_capacity > 0 && results == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "results");
        }

        *result_count = 0;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING)
        std::vector<cv::ppf_match_3d::Pose3DPtr> native_results;
        match_detector(detector, scene, relative_scene_sample_step, relative_scene_distance, native_results);
        *result_count = static_cast<int>(native_results.size());
        if (result_capacity < *result_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "result_capacity");
        }

        for (int i = 0; i < *result_count; ++i)
        {
            fill_pose_result(native_results[static_cast<size_t>(i)], &results[i]);
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)relative_scene_sample_step; (void)relative_scene_distance;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}
