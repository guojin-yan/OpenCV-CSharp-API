#include "open_cv_sharp/rapid/rapid.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "rapid_handles.h"

#include <new>

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_tracker(const char* api_name, const jyppx_ocv_rapid_tracker* tracker)
    {
        return tracker == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "tracker")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_pointer(const char* api_name, const void* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
    cv::InputArray optional_input_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::InputOutputArray optional_input_output_array(jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputOutputArray(opencv_csharp_native::mat_value(mat));
    }

    int create_tracker_handle(const char* api_name, const cv::Ptr<cv::rapid::Tracker>& native, jyppx_ocv_rapid_tracker** tracker)
    {
        if (tracker == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tracker");
        }

        *tracker = nullptr;
        if (native.empty())
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        jyppx_ocv_rapid_tracker* created = new (std::nothrow) jyppx_ocv_rapid_tracker();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *tracker = created;
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_rapid_draw_correspondencies(jyppx_ocv_mat* bundle, const jyppx_ocv_mat* cols, const jyppx_ocv_mat* colors)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_draw_correspondencies";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, bundle, "bundle");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, cols, "cols");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        cv::rapid::drawCorrespondencies(
            opencv_csharp_native::mat_value(bundle),
            opencv_csharp_native::mat_value(cols),
            optional_input_array(colors));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)colors;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rapid_draw_search_lines(
    jyppx_ocv_mat* img,
    const jyppx_ocv_mat* locations,
    double color0,
    double color1,
    double color2,
    double color3)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_draw_search_lines";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, img, "img");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, locations, "locations");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        cv::rapid::drawSearchLines(
            opencv_csharp_native::mat_value(img),
            opencv_csharp_native::mat_value(locations),
            cv::Scalar(color0, color1, color2, color3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color0; (void)color1; (void)color2; (void)color3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rapid_draw_wireframe(
    jyppx_ocv_mat* img,
    const jyppx_ocv_mat* pts2d,
    const jyppx_ocv_mat* tris,
    double color0,
    double color1,
    double color2,
    double color3,
    int line_type,
    int cull_backface)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_draw_wireframe";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, img, "img");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, pts2d, "pts2d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, tris, "tris");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        cv::rapid::drawWireframe(
            opencv_csharp_native::mat_value(img),
            opencv_csharp_native::mat_value(pts2d),
            opencv_csharp_native::mat_value(tris),
            cv::Scalar(color0, color1, color2, color3),
            line_type,
            cull_backface != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color0; (void)color1; (void)color2; (void)color3; (void)line_type; (void)cull_backface;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rapid_extract_control_points(
    int num,
    int len,
    const jyppx_ocv_mat* pts3d,
    const jyppx_ocv_mat* rvec,
    const jyppx_ocv_mat* tvec,
    const jyppx_ocv_mat* camera_matrix,
    int image_width,
    int image_height,
    const jyppx_ocv_mat* tris,
    jyppx_ocv_mat* ctl2d,
    jyppx_ocv_mat* ctl3d)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_extract_control_points";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, pts3d, "pts3d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rvec, "rvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, tvec, "tvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, tris, "tris");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, ctl2d, "ctl2d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, ctl3d, "ctl3d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        cv::rapid::extractControlPoints(
            num,
            len,
            opencv_csharp_native::mat_value(pts3d),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            opencv_csharp_native::mat_value(camera_matrix),
            cv::Size(image_width, image_height),
            opencv_csharp_native::mat_value(tris),
            opencv_csharp_native::mat_value(ctl2d),
            opencv_csharp_native::mat_value(ctl3d));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)num; (void)len; (void)image_width; (void)image_height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rapid_extract_line_bundle(
    int len,
    const jyppx_ocv_mat* ctl2d,
    const jyppx_ocv_mat* img,
    jyppx_ocv_mat* bundle,
    jyppx_ocv_mat* src_locations)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_extract_line_bundle";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, ctl2d, "ctl2d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, img, "img");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, bundle, "bundle");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src_locations, "src_locations");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        cv::rapid::extractLineBundle(
            len,
            opencv_csharp_native::mat_value(ctl2d),
            opencv_csharp_native::mat_value(img),
            opencv_csharp_native::mat_value(bundle),
            opencv_csharp_native::mat_value(src_locations));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)len;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rapid_find_correspondencies(const jyppx_ocv_mat* bundle, jyppx_ocv_mat* cols, jyppx_ocv_mat* response)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_find_correspondencies";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, bundle, "bundle");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, cols, "cols");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        cv::rapid::findCorrespondencies(
            opencv_csharp_native::mat_value(bundle),
            opencv_csharp_native::mat_value(cols),
            optional_input_output_array(response));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)response;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rapid_convert_correspondencies(
    const jyppx_ocv_mat* cols,
    const jyppx_ocv_mat* src_locations,
    jyppx_ocv_mat* pts2d,
    jyppx_ocv_mat* pts3d,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_convert_correspondencies";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, cols, "cols");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src_locations, "src_locations");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, pts2d, "pts2d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        cv::rapid::convertCorrespondencies(
            opencv_csharp_native::mat_value(cols),
            opencv_csharp_native::mat_value(src_locations),
            opencv_csharp_native::mat_value(pts2d),
            optional_input_output_array(pts3d),
            optional_input_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pts3d; (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rapid_run(
    const jyppx_ocv_mat* img,
    int num,
    int len,
    const jyppx_ocv_mat* pts3d,
    const jyppx_ocv_mat* tris,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int compute_rmsd,
    float* ratio,
    double* rmsd)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_run";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, img, "img");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, pts3d, "pts3d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, tris, "tris");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rvec, "rvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, tvec, "tvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, ratio, "ratio");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (compute_rmsd != 0)
        {
            status = validate_output_pointer(api_name, rmsd, "rmsd");
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        }

        *ratio = 0.0F;
        if (rmsd != nullptr)
        {
            *rmsd = 0.0;
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        double native_rmsd = 0.0;
        *ratio = cv::rapid::rapid(
            opencv_csharp_native::mat_value(img),
            num,
            len,
            opencv_csharp_native::mat_value(pts3d),
            opencv_csharp_native::mat_value(tris),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            compute_rmsd != 0 ? &native_rmsd : nullptr);
        if (compute_rmsd != 0 && rmsd != nullptr)
        {
            *rmsd = native_rmsd;
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)num; (void)len; (void)compute_rmsd;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rapid_tracker_create(const jyppx_ocv_mat* pts3d, const jyppx_ocv_mat* tris, jyppx_ocv_rapid_tracker** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_tracker_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, pts3d, "pts3d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, tris, "tris");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (tracker == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tracker");
        }

        *tracker = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        return create_tracker_handle(
            api_name,
            cv::rapid::Rapid::create(opencv_csharp_native::mat_value(pts3d), opencv_csharp_native::mat_value(tris)),
            tracker);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rapid_ols_tracker_create(
    const jyppx_ocv_mat* pts3d,
    const jyppx_ocv_mat* tris,
    int hist_bins,
    int sobel_thresh,
    jyppx_ocv_rapid_tracker** tracker)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_ols_tracker_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, pts3d, "pts3d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, tris, "tris");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (tracker == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "tracker");
        }

        *tracker = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        return create_tracker_handle(
            api_name,
            cv::rapid::OLSTracker::create(
                opencv_csharp_native::mat_value(pts3d),
                opencv_csharp_native::mat_value(tris),
                hist_bins,
                static_cast<unsigned char>(sobel_thresh)),
            tracker);
#else
        (void)hist_bins; (void)sobel_thresh;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_rapid_tracker_release(jyppx_ocv_rapid_tracker* tracker)
{
    delete tracker;
}

int jyppx_ocv_rapid_tracker_compute(
    jyppx_ocv_rapid_tracker* tracker,
    const jyppx_ocv_mat* img,
    int num,
    int len,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* rvec,
    jyppx_ocv_mat* tvec,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    float* ratio)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_tracker_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_tracker(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, img, "img");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rvec, "rvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, tvec, "tvec");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_pointer(api_name, ratio, "ratio");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        *ratio = 0.0F;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        *ratio = tracker->value->compute(
            opencv_csharp_native::mat_value(img),
            num,
            len,
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(rvec),
            opencv_csharp_native::mat_value(tvec),
            cv::TermCriteria(criteria_type, criteria_max_count, criteria_epsilon));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)num; (void)len; (void)criteria_type; (void)criteria_max_count; (void)criteria_epsilon;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rapid_tracker_clear_state(jyppx_ocv_rapid_tracker* tracker)
{
    constexpr const char* api_name = "jyppx_ocv_rapid_tracker_clear_state";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_tracker(api_name, tracker);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_RAPID)
        tracker->value->clearState();
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


