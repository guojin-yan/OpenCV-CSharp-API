#include "open_cv_sharp/ptcloud/ptcloud.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "ptcloud_handles.h"

#include <new>

namespace
{
    constexpr int RGBD_NORMALS_PROPERTY_ROWS = 0;
    constexpr int RGBD_NORMALS_PROPERTY_COLS = 1;
    constexpr int RGBD_NORMALS_PROPERTY_WINDOW_SIZE = 2;
    constexpr int RGBD_NORMALS_PROPERTY_DEPTH = 3;
    constexpr int RGBD_NORMALS_PROPERTY_METHOD = 4;

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

    int validate_normals(const char* api_name, const jyppx_ocv_rgbd_normals* normals)
    {
        return normals == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "normals")
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
    cv::InputArray optional_input_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::OutputArray optional_output_array(jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::OutputArray(opencv_csharp_native::mat_value(mat));
    }
#endif
}

int jyppx_ocv_ptcloud_register_depth(
    const jyppx_ocv_mat* unregistered_camera_matrix,
    const jyppx_ocv_mat* registered_camera_matrix,
    const jyppx_ocv_mat* registered_dist_coeffs,
    const jyppx_ocv_mat* rt,
    const jyppx_ocv_mat* unregistered_depth,
    int output_width,
    int output_height,
    jyppx_ocv_mat* registered_depth,
    int depth_dilation)
{
    constexpr const char* api_name = "jyppx_ocv_ptcloud_register_depth";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, unregistered_camera_matrix, "unregistered_camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, registered_camera_matrix, "registered_camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, registered_dist_coeffs, "registered_dist_coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rt, "rt");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, unregistered_depth, "unregistered_depth");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, registered_depth, "registered_depth");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        cv::registerDepth(
            opencv_csharp_native::mat_value(unregistered_camera_matrix),
            opencv_csharp_native::mat_value(registered_camera_matrix),
            opencv_csharp_native::mat_value(registered_dist_coeffs),
            opencv_csharp_native::mat_value(rt),
            opencv_csharp_native::mat_value(unregistered_depth),
            cv::Size(output_width, output_height),
            opencv_csharp_native::mat_value(registered_depth),
            depth_dilation != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)output_width;
        (void)output_height;
        (void)depth_dilation;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ptcloud_depth_to_3d(
    const jyppx_ocv_mat* depth,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* points3d,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_ptcloud_depth_to_3d";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, depth, "depth");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points3d, "points3d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        cv::depthTo3d(
            opencv_csharp_native::mat_value(depth),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(points3d),
            optional_input_array(mask));
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

int jyppx_ocv_ptcloud_depth_to_3d_sparse(
    const jyppx_ocv_mat* depth,
    const jyppx_ocv_mat* camera_matrix,
    const jyppx_ocv_mat* points,
    jyppx_ocv_mat* points3d)
{
    constexpr const char* api_name = "jyppx_ocv_ptcloud_depth_to_3d_sparse";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, depth, "depth");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points3d, "points3d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        cv::depthTo3dSparse(
            opencv_csharp_native::mat_value(depth),
            opencv_csharp_native::mat_value(camera_matrix),
            opencv_csharp_native::mat_value(points),
            opencv_csharp_native::mat_value(points3d));
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

int jyppx_ocv_ptcloud_rescale_depth(const jyppx_ocv_mat* src, int type, jyppx_ocv_mat* dst, double depth_factor)
{
    constexpr const char* api_name = "jyppx_ocv_ptcloud_rescale_depth";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        cv::rescaleDepth(opencv_csharp_native::mat_value(src), type, opencv_csharp_native::mat_value(dst), depth_factor);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)type;
        (void)depth_factor;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ptcloud_warp_frame(
    const jyppx_ocv_mat* depth,
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    const jyppx_ocv_mat* rt,
    const jyppx_ocv_mat* camera_matrix,
    jyppx_ocv_mat* warped_depth,
    jyppx_ocv_mat* warped_image,
    jyppx_ocv_mat* warped_mask)
{
    constexpr const char* api_name = "jyppx_ocv_ptcloud_warp_frame";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, depth, "depth");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, rt, "rt");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        cv::warpFrame(
            opencv_csharp_native::mat_value(depth),
            optional_input_array(image),
            optional_input_array(mask),
            opencv_csharp_native::mat_value(rt),
            opencv_csharp_native::mat_value(camera_matrix),
            optional_output_array(warped_depth),
            optional_output_array(warped_image),
            optional_output_array(warped_mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)image;
        (void)mask;
        (void)warped_depth;
        (void)warped_image;
        (void)warped_mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_ptcloud_find_planes(
    const jyppx_ocv_mat* points3d,
    const jyppx_ocv_mat* normals,
    jyppx_ocv_mat* mask,
    jyppx_ocv_mat* plane_coefficients,
    int block_size,
    int min_size,
    double threshold,
    double sensor_error_a,
    double sensor_error_b,
    double sensor_error_c,
    int method)
{
    constexpr const char* api_name = "jyppx_ocv_ptcloud_find_planes";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, points3d, "points3d");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, plane_coefficients, "plane_coefficients");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        cv::findPlanes(
            opencv_csharp_native::mat_value(points3d),
            optional_input_array(normals),
            opencv_csharp_native::mat_value(mask),
            opencv_csharp_native::mat_value(plane_coefficients),
            block_size,
            min_size,
            threshold,
            sensor_error_a,
            sensor_error_b,
            sensor_error_c,
            static_cast<cv::RgbdPlaneMethod>(method));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)normals;
        (void)block_size;
        (void)min_size;
        (void)threshold;
        (void)sensor_error_a;
        (void)sensor_error_b;
        (void)sensor_error_c;
        (void)method;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rgbd_normals_create(
    int rows,
    int cols,
    int depth,
    const jyppx_ocv_mat* camera_matrix,
    int window_size,
    float diff_threshold,
    int method,
    jyppx_ocv_rgbd_normals** normals)
{
    constexpr const char* api_name = "jyppx_ocv_rgbd_normals_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (normals == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "normals");
        }

        *normals = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        jyppx_ocv_rgbd_normals* created = new (std::nothrow) jyppx_ocv_rgbd_normals();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::RgbdNormals::create(
            rows,
            cols,
            depth,
            optional_input_array(camera_matrix),
            window_size,
            diff_threshold,
            static_cast<cv::RgbdNormals::RgbdNormalsMethod>(method));
        *normals = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rows;
        (void)cols;
        (void)depth;
        (void)camera_matrix;
        (void)window_size;
        (void)diff_threshold;
        (void)method;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_rgbd_normals_release_handle(jyppx_ocv_rgbd_normals* normals)
{
    delete normals;
}

int jyppx_ocv_rgbd_normals_apply(const jyppx_ocv_rgbd_normals* normals, const jyppx_ocv_mat* points, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_rgbd_normals_apply";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_normals(api_name, normals);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, points, "points");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        normals->value->apply(opencv_csharp_native::mat_value(points), opencv_csharp_native::mat_value(dst));
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

int jyppx_ocv_rgbd_normals_cache(const jyppx_ocv_rgbd_normals* normals)
{
    constexpr const char* api_name = "jyppx_ocv_rgbd_normals_cache";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_normals(api_name, normals);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        normals->value->cache();
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

int jyppx_ocv_rgbd_normals_get_int_property(const jyppx_ocv_rgbd_normals* normals, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_rgbd_normals_get_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_normals(api_name, normals);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        switch (property_id)
        {
        case RGBD_NORMALS_PROPERTY_ROWS: *value = normals->value->getRows(); return OPENCV_CSHARP_STATUS_OK;
        case RGBD_NORMALS_PROPERTY_COLS: *value = normals->value->getCols(); return OPENCV_CSHARP_STATUS_OK;
        case RGBD_NORMALS_PROPERTY_WINDOW_SIZE: *value = normals->value->getWindowSize(); return OPENCV_CSHARP_STATUS_OK;
        case RGBD_NORMALS_PROPERTY_DEPTH: *value = normals->value->getDepth(); return OPENCV_CSHARP_STATUS_OK;
        case RGBD_NORMALS_PROPERTY_METHOD: *value = static_cast<int>(normals->value->getMethod()); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_rgbd_normals_set_int_property(jyppx_ocv_rgbd_normals* normals, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_rgbd_normals_set_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_normals(api_name, normals);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        switch (property_id)
        {
        case RGBD_NORMALS_PROPERTY_ROWS: normals->value->setRows(value); return OPENCV_CSHARP_STATUS_OK;
        case RGBD_NORMALS_PROPERTY_COLS: normals->value->setCols(value); return OPENCV_CSHARP_STATUS_OK;
        case RGBD_NORMALS_PROPERTY_WINDOW_SIZE: normals->value->setWindowSize(value); return OPENCV_CSHARP_STATUS_OK;
        case RGBD_NORMALS_PROPERTY_METHOD: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        case RGBD_NORMALS_PROPERTY_DEPTH: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_rgbd_normals_get_k(const jyppx_ocv_rgbd_normals* normals, jyppx_ocv_mat* camera_matrix)
{
    constexpr const char* api_name = "jyppx_ocv_rgbd_normals_get_k";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_normals(api_name, normals);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        normals->value->getK(opencv_csharp_native::mat_value(camera_matrix));
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

int jyppx_ocv_rgbd_normals_set_k(jyppx_ocv_rgbd_normals* normals, const jyppx_ocv_mat* camera_matrix)
{
    constexpr const char* api_name = "jyppx_ocv_rgbd_normals_set_k";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_normals(api_name, normals);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, camera_matrix, "camera_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PTCLOUD)
        normals->value->setK(opencv_csharp_native::mat_value(camera_matrix));
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


