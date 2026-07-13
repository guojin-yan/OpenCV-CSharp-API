#include "open_cv_sharp/calib3d/calib3d.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "stereo_handles.h"

#include <new>

namespace
{
    int validate_stereo_matcher(const char* api_name, const jyppx_ocv_stereo_matcher* stereo_matcher)
    {
        return stereo_matcher == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "stereo_matcher")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_stereo_bm(const char* api_name, const jyppx_ocv_stereo_bm* stereo_bm)
    {
        return stereo_bm == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "stereo_bm")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_stereo_sgbm(const char* api_name, const jyppx_ocv_stereo_sgbm* stereo_sgbm)
    {
        return stereo_sgbm == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "stereo_sgbm")
            : OPENCV_CSHARP_STATUS_OK;
    }

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

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    int assign_rect(const char* api_name, const cv::Rect& rect, int* x, int* y, int* width, int* height)
    {
        if (x == nullptr || y == nullptr || width == nullptr || height == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rect");
        }

        write_rect_values(rect.x, rect.y, rect.width, rect.height, x, y, width, height);
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

void jyppx_ocv_stereo_matcher_release(jyppx_ocv_stereo_matcher* stereo_matcher)
{
    delete stereo_matcher;
}

int jyppx_ocv_stereo_matcher_compute(
    jyppx_ocv_stereo_matcher* stereo_matcher,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right,
    jyppx_ocv_mat* disparity)
{
    constexpr const char* api_name = "jyppx_ocv_stereo_matcher_compute";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_stereo_matcher(api_name, stereo_matcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, left, "left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, right, "right");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, disparity, "disparity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        stereo_matcher->value->compute(
            opencv_csharp_native::mat_value(left),
            opencv_csharp_native::mat_value(right),
            opencv_csharp_native::mat_value(disparity));
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

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#define OCV_CSHARP_STEREO_MATCHER_GET_INT(native_name, method_name) \
    int native_name(const jyppx_ocv_stereo_matcher* stereo_matcher, int* value) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_stereo_matcher(api_name, stereo_matcher); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
            *value = stereo_matcher->value->method_name(); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }

#define OCV_CSHARP_STEREO_MATCHER_SET_INT(native_name, method_name) \
    int native_name(jyppx_ocv_stereo_matcher* stereo_matcher, int value) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_stereo_matcher(api_name, stereo_matcher); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            stereo_matcher->value->method_name(value); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }
#else
#define OCV_CSHARP_STEREO_MATCHER_GET_INT(native_name, method_name) \
    int native_name(const jyppx_ocv_stereo_matcher*, int* value) \
    { \
        if (value != nullptr) { *value = 0; } \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }

#define OCV_CSHARP_STEREO_MATCHER_SET_INT(native_name, method_name) \
    int native_name(jyppx_ocv_stereo_matcher*, int) \
    { \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }
#endif

OCV_CSHARP_STEREO_MATCHER_GET_INT(jyppx_ocv_stereo_matcher_get_min_disparity, getMinDisparity)
OCV_CSHARP_STEREO_MATCHER_SET_INT(jyppx_ocv_stereo_matcher_set_min_disparity, setMinDisparity)
OCV_CSHARP_STEREO_MATCHER_GET_INT(jyppx_ocv_stereo_matcher_get_num_disparities, getNumDisparities)
OCV_CSHARP_STEREO_MATCHER_SET_INT(jyppx_ocv_stereo_matcher_set_num_disparities, setNumDisparities)
OCV_CSHARP_STEREO_MATCHER_GET_INT(jyppx_ocv_stereo_matcher_get_block_size, getBlockSize)
OCV_CSHARP_STEREO_MATCHER_SET_INT(jyppx_ocv_stereo_matcher_set_block_size, setBlockSize)
OCV_CSHARP_STEREO_MATCHER_GET_INT(jyppx_ocv_stereo_matcher_get_speckle_window_size, getSpeckleWindowSize)
OCV_CSHARP_STEREO_MATCHER_SET_INT(jyppx_ocv_stereo_matcher_set_speckle_window_size, setSpeckleWindowSize)
OCV_CSHARP_STEREO_MATCHER_GET_INT(jyppx_ocv_stereo_matcher_get_speckle_range, getSpeckleRange)
OCV_CSHARP_STEREO_MATCHER_SET_INT(jyppx_ocv_stereo_matcher_set_speckle_range, setSpeckleRange)
OCV_CSHARP_STEREO_MATCHER_GET_INT(jyppx_ocv_stereo_matcher_get_disp12_max_diff, getDisp12MaxDiff)
OCV_CSHARP_STEREO_MATCHER_SET_INT(jyppx_ocv_stereo_matcher_set_disp12_max_diff, setDisp12MaxDiff)

#undef OCV_CSHARP_STEREO_MATCHER_GET_INT
#undef OCV_CSHARP_STEREO_MATCHER_SET_INT

int jyppx_ocv_stereo_bm_create(int num_disparities, int block_size, jyppx_ocv_stereo_bm** stereo_bm)
{
    constexpr const char* api_name = "jyppx_ocv_stereo_bm_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (stereo_bm == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "stereo_bm");
        }

        *stereo_bm = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_stereo_bm* created = new (std::nothrow) jyppx_ocv_stereo_bm();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::StereoBM::create(num_disparities, block_size);
        *stereo_bm = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)num_disparities;
        (void)block_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_stereo_bm_release(jyppx_ocv_stereo_bm* stereo_bm)
{
    delete stereo_bm;
}

int jyppx_ocv_stereo_bm_compute(
    jyppx_ocv_stereo_bm* stereo_bm,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right,
    jyppx_ocv_mat* disparity)
{
    constexpr const char* api_name = "jyppx_ocv_stereo_bm_compute";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_stereo_bm(api_name, stereo_bm);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, left, "left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, right, "right");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, disparity, "disparity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        stereo_bm->value->compute(
            opencv_csharp_native::mat_value(left),
            opencv_csharp_native::mat_value(right),
            opencv_csharp_native::mat_value(disparity));
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

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#define OCV_CSHARP_STEREO_BM_GET_INT(native_name, method_name) \
    int native_name(const jyppx_ocv_stereo_bm* stereo_bm, int* value) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_stereo_bm(api_name, stereo_bm); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
            *value = stereo_bm->value->method_name(); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }

#define OCV_CSHARP_STEREO_BM_SET_INT(native_name, method_name) \
    int native_name(jyppx_ocv_stereo_bm* stereo_bm, int value) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_stereo_bm(api_name, stereo_bm); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            stereo_bm->value->method_name(value); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }
#else
#define OCV_CSHARP_STEREO_BM_GET_INT(native_name, method_name) \
    int native_name(const jyppx_ocv_stereo_bm*, int* value) \
    { \
        if (value != nullptr) { *value = 0; } \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }

#define OCV_CSHARP_STEREO_BM_SET_INT(native_name, method_name) \
    int native_name(jyppx_ocv_stereo_bm*, int) \
    { \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }
#endif

OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_min_disparity, getMinDisparity)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_min_disparity, setMinDisparity)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_num_disparities, getNumDisparities)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_num_disparities, setNumDisparities)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_block_size, getBlockSize)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_block_size, setBlockSize)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_speckle_window_size, getSpeckleWindowSize)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_speckle_window_size, setSpeckleWindowSize)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_speckle_range, getSpeckleRange)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_speckle_range, setSpeckleRange)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_disp12_max_diff, getDisp12MaxDiff)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_disp12_max_diff, setDisp12MaxDiff)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_pre_filter_type, getPreFilterType)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_pre_filter_type, setPreFilterType)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_pre_filter_size, getPreFilterSize)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_pre_filter_size, setPreFilterSize)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_pre_filter_cap, getPreFilterCap)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_pre_filter_cap, setPreFilterCap)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_texture_threshold, getTextureThreshold)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_texture_threshold, setTextureThreshold)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_uniqueness_ratio, getUniquenessRatio)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_uniqueness_ratio, setUniquenessRatio)
OCV_CSHARP_STEREO_BM_GET_INT(jyppx_ocv_stereo_bm_get_smaller_block_size, getSmallerBlockSize)
OCV_CSHARP_STEREO_BM_SET_INT(jyppx_ocv_stereo_bm_set_smaller_block_size, setSmallerBlockSize)

#undef OCV_CSHARP_STEREO_BM_GET_INT
#undef OCV_CSHARP_STEREO_BM_SET_INT

int jyppx_ocv_stereo_bm_get_roi1(const jyppx_ocv_stereo_bm* stereo_bm, int* x, int* y, int* width, int* height)
{
    constexpr const char* api_name = "jyppx_ocv_stereo_bm_get_roi1";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_stereo_bm(api_name, stereo_bm);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return assign_rect(api_name, stereo_bm->value->getROI1(), x, y, width, height);
#else
        write_rect_values(0, 0, 0, 0, x, y, width, height);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stereo_bm_set_roi1(jyppx_ocv_stereo_bm* stereo_bm, int x, int y, int width, int height)
{
    constexpr const char* api_name = "jyppx_ocv_stereo_bm_set_roi1";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_stereo_bm(api_name, stereo_bm);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        stereo_bm->value->setROI1(cv::Rect(x, y, width, height));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x;
        (void)y;
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

int jyppx_ocv_stereo_bm_get_roi2(const jyppx_ocv_stereo_bm* stereo_bm, int* x, int* y, int* width, int* height)
{
    constexpr const char* api_name = "jyppx_ocv_stereo_bm_get_roi2";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_stereo_bm(api_name, stereo_bm);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return assign_rect(api_name, stereo_bm->value->getROI2(), x, y, width, height);
#else
        write_rect_values(0, 0, 0, 0, x, y, width, height);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stereo_bm_set_roi2(jyppx_ocv_stereo_bm* stereo_bm, int x, int y, int width, int height)
{
    constexpr const char* api_name = "jyppx_ocv_stereo_bm_set_roi2";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_stereo_bm(api_name, stereo_bm);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        stereo_bm->value->setROI2(cv::Rect(x, y, width, height));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x;
        (void)y;
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

int jyppx_ocv_stereo_sgbm_create(
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
    jyppx_ocv_stereo_sgbm** stereo_sgbm)
{
    constexpr const char* api_name = "jyppx_ocv_stereo_sgbm_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (stereo_sgbm == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "stereo_sgbm");
        }

        *stereo_sgbm = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_stereo_sgbm* created = new (std::nothrow) jyppx_ocv_stereo_sgbm();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::StereoSGBM::create(
            min_disparity,
            num_disparities,
            block_size,
            p1,
            p2,
            disp12_max_diff,
            pre_filter_cap,
            uniqueness_ratio,
            speckle_window_size,
            speckle_range,
            mode);
        *stereo_sgbm = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)min_disparity;
        (void)num_disparities;
        (void)block_size;
        (void)p1;
        (void)p2;
        (void)disp12_max_diff;
        (void)pre_filter_cap;
        (void)uniqueness_ratio;
        (void)speckle_window_size;
        (void)speckle_range;
        (void)mode;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_stereo_sgbm_release(jyppx_ocv_stereo_sgbm* stereo_sgbm)
{
    delete stereo_sgbm;
}

int jyppx_ocv_stereo_sgbm_compute(
    jyppx_ocv_stereo_sgbm* stereo_sgbm,
    const jyppx_ocv_mat* left,
    const jyppx_ocv_mat* right,
    jyppx_ocv_mat* disparity)
{
    constexpr const char* api_name = "jyppx_ocv_stereo_sgbm_compute";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_stereo_sgbm(api_name, stereo_sgbm);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, left, "left");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_input_mat(api_name, right, "right");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_mat(api_name, disparity, "disparity");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        stereo_sgbm->value->compute(
            opencv_csharp_native::mat_value(left),
            opencv_csharp_native::mat_value(right),
            opencv_csharp_native::mat_value(disparity));
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

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#define OCV_CSHARP_STEREO_SGBM_GET_INT(native_name, method_name) \
    int native_name(const jyppx_ocv_stereo_sgbm* stereo_sgbm, int* value) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_stereo_sgbm(api_name, stereo_sgbm); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            if (value == nullptr) { return opencv_csharp_native::set_invalid_argument(api_name, "value"); } \
            *value = stereo_sgbm->value->method_name(); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }

#define OCV_CSHARP_STEREO_SGBM_SET_INT(native_name, method_name) \
    int native_name(jyppx_ocv_stereo_sgbm* stereo_sgbm, int value) \
    { \
        constexpr const char* api_name = #native_name; \
        try \
        { \
            opencv_csharp_native::clear_last_error(); \
            int status = validate_stereo_sgbm(api_name, stereo_sgbm); \
            if (status != OPENCV_CSHARP_STATUS_OK) { return status; } \
            stereo_sgbm->value->method_name(value); \
            return OPENCV_CSHARP_STATUS_OK; \
        } \
        catch (...) \
        { \
            return opencv_csharp_native::translate_current_exception(api_name); \
        } \
    }
#else
#define OCV_CSHARP_STEREO_SGBM_GET_INT(native_name, method_name) \
    int native_name(const jyppx_ocv_stereo_sgbm*, int* value) \
    { \
        if (value != nullptr) { *value = 0; } \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }

#define OCV_CSHARP_STEREO_SGBM_SET_INT(native_name, method_name) \
    int native_name(jyppx_ocv_stereo_sgbm*, int) \
    { \
        return opencv_csharp_native::set_not_linked(#native_name); \
    }
#endif

OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_min_disparity, getMinDisparity)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_min_disparity, setMinDisparity)
OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_num_disparities, getNumDisparities)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_num_disparities, setNumDisparities)
OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_block_size, getBlockSize)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_block_size, setBlockSize)
OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_speckle_window_size, getSpeckleWindowSize)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_speckle_window_size, setSpeckleWindowSize)
OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_speckle_range, getSpeckleRange)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_speckle_range, setSpeckleRange)
OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_disp12_max_diff, getDisp12MaxDiff)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_disp12_max_diff, setDisp12MaxDiff)
OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_pre_filter_cap, getPreFilterCap)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_pre_filter_cap, setPreFilterCap)
OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_uniqueness_ratio, getUniquenessRatio)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_uniqueness_ratio, setUniquenessRatio)
OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_p1, getP1)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_p1, setP1)
OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_p2, getP2)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_p2, setP2)
OCV_CSHARP_STEREO_SGBM_GET_INT(jyppx_ocv_stereo_sgbm_get_mode, getMode)
OCV_CSHARP_STEREO_SGBM_SET_INT(jyppx_ocv_stereo_sgbm_set_mode, setMode)

#undef OCV_CSHARP_STEREO_SGBM_GET_INT
#undef OCV_CSHARP_STEREO_SGBM_SET_INT
