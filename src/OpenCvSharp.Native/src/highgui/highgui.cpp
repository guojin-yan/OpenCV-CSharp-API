#include "open_cv_sharp/highgui/highgui.h"

#include "../core/mat_handle.h"
#include "../error_state.h"

#include <new>

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/highgui.hpp>
#endif

struct jyppx_ocv_highgui_trackbar
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    int value;
    jyppx_ocv_highgui_trackbar_callback callback;
    void* userdata;
#else
    int placeholder;
#endif
};

namespace
{
    int validate_string(const char* api_name, const char* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
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

    int validate_output_double(const char* api_name, const double* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_rect(const char* api_name, const jyppx_ocv_highgui_rect* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    jyppx_ocv_highgui_rect from_cv_rect(cv::Rect rect)
    {
        return jyppx_ocv_highgui_rect{ rect.x, rect.y, rect.width, rect.height };
    }

    void trackbar_callback_trampoline(int pos, void* userdata)
    {
        auto trackbar = reinterpret_cast<jyppx_ocv_highgui_trackbar*>(userdata);
        if (trackbar != nullptr)
        {
            trackbar->value = pos;
            if (trackbar->callback != nullptr)
            {
                trackbar->callback(pos, trackbar->userdata);
            }
        }
    }
#endif
}

int jyppx_ocv_highgui_named_window(const char* winname, int flags)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_named_window";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::namedWindow(winname, flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_destroy_window(const char* winname)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_destroy_window";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::destroyWindow(winname);
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

int jyppx_ocv_highgui_destroy_all_windows(void)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_destroy_all_windows";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::destroyAllWindows();
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

int jyppx_ocv_highgui_imshow(const char* winname, const jyppx_ocv_mat* mat)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_imshow";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mat, "mat");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::imshow(winname, opencv_csharp_native::mat_value(mat));
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

int jyppx_ocv_highgui_wait_key(int delay, int* key)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_wait_key";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, key, "key");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *key = cv::waitKey(delay);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)delay;
        *key = -1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_poll_key(int* key)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_poll_key";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, key, "key");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *key = cv::pollKey();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *key = -1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_move_window(const char* winname, int x, int y)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_move_window";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::moveWindow(winname, x, y);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x;
        (void)y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_resize_window(const char* winname, int width, int height)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_resize_window";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::resizeWindow(winname, width, height);
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

int jyppx_ocv_highgui_set_window_property(const char* winname, int prop_id, double prop_value)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_window_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setWindowProperty(winname, prop_id, prop_value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)prop_id;
        (void)prop_value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_get_window_property(const char* winname, int prop_id, double* prop_value)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_get_window_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, prop_value, "prop_value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *prop_value = cv::getWindowProperty(winname, prop_id);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)prop_id;
        *prop_value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_window_title(const char* winname, const char* title)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_window_title";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, title, "title");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setWindowTitle(winname, title);
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

int jyppx_ocv_highgui_get_window_image_rect(const char* winname, jyppx_ocv_highgui_rect* rect)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_get_window_image_rect";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_rect(api_name, rect, "rect");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *rect = from_cv_rect(cv::getWindowImageRect(winname));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *rect = jyppx_ocv_highgui_rect{ 0, 0, 0, 0 };
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_create_trackbar(
    const char* trackbarname,
    const char* winname,
    int initial_value,
    int count,
    jyppx_ocv_highgui_trackbar_callback callback,
    void* userdata,
    jyppx_ocv_highgui_trackbar** trackbar)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_create_trackbar";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, trackbarname, "trackbarname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (trackbar == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "trackbar");
        }

        *trackbar = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        jyppx_ocv_highgui_trackbar* created = new (std::nothrow) jyppx_ocv_highgui_trackbar();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = initial_value;
        created->callback = callback;
        created->userdata = userdata;
        cv::createTrackbar(trackbarname, winname, &created->value, count, callback == nullptr ? nullptr : trackbar_callback_trampoline, created);
        *trackbar = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)initial_value;
        (void)count;
        (void)callback;
        (void)userdata;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_highgui_trackbar_release_handle(jyppx_ocv_highgui_trackbar* trackbar)
{
    if (trackbar != nullptr)
    {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        trackbar->callback = nullptr;
        trackbar->userdata = nullptr;
#else
        (void)trackbar;
#endif
    }
}

int jyppx_ocv_highgui_get_trackbar_pos(const char* trackbarname, const char* winname, int* pos)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_get_trackbar_pos";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, trackbarname, "trackbarname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, pos, "pos");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *pos = cv::getTrackbarPos(trackbarname, winname);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *pos = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_trackbar_pos(const char* trackbarname, const char* winname, int pos)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_trackbar_pos";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, trackbarname, "trackbarname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setTrackbarPos(trackbarname, winname, pos);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pos;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_trackbar_min(const char* trackbarname, const char* winname, int minval)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_trackbar_min";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, trackbarname, "trackbarname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setTrackbarMin(trackbarname, winname, minval);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)minval;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_trackbar_max(const char* trackbarname, const char* winname, int maxval)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_trackbar_max";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, trackbarname, "trackbarname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setTrackbarMax(trackbarname, winname, maxval);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)maxval;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_mouse_callback(
    const char* winname,
    jyppx_ocv_highgui_mouse_callback callback,
    void* userdata)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_mouse_callback";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setMouseCallback(winname, callback, userdata);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)callback;
        (void)userdata;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_create_button(
    const char* button_name,
    jyppx_ocv_highgui_button_callback callback,
    void* userdata,
    int type,
    int initial_button_state)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_create_button";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, button_name, "button_name");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int result = cv::createButton(
            button_name,
            callback,
            userdata,
            type,
            initial_button_state != 0);
        return result >= 0 ? OPENCV_CSHARP_STATUS_OK : opencv_csharp_native::set_invalid_argument(api_name, "button");
#else
        (void)callback;
        (void)userdata;
        (void)type;
        (void)initial_button_state;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}


