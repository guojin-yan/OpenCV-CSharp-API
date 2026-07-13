#pragma once

#include "open_cv_sharp/core/mat.h"
#include "open_cv_sharp/export.h"
#include "open_cv_sharp/status.h"

typedef struct jyppx_ocv_highgui_trackbar jyppx_ocv_highgui_trackbar;

typedef struct jyppx_ocv_highgui_rect
{
    int x;
    int y;
    int width;
    int height;
} jyppx_ocv_highgui_rect;

typedef void (*jyppx_ocv_highgui_mouse_callback)(int event, int x, int y, int flags, void* userdata);
typedef void (*jyppx_ocv_highgui_trackbar_callback)(int pos, void* userdata);
typedef void (*jyppx_ocv_highgui_button_callback)(int state, void* userdata);

OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_named_window(const char* winname, int flags);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_destroy_window(const char* winname);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_destroy_all_windows(void);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_imshow(const char* winname, const jyppx_ocv_mat* mat);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_wait_key(int delay, int* key);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_poll_key(int* key);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_move_window(const char* winname, int x, int y);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_resize_window(const char* winname, int width, int height);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_set_window_property(const char* winname, int prop_id, double prop_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_get_window_property(const char* winname, int prop_id, double* prop_value);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_set_window_title(const char* winname, const char* title);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_get_window_image_rect(const char* winname, jyppx_ocv_highgui_rect* rect);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_create_trackbar(
    const char* trackbarname,
    const char* winname,
    int initial_value,
    int count,
    jyppx_ocv_highgui_trackbar_callback callback,
    void* userdata,
    jyppx_ocv_highgui_trackbar** trackbar);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API void jyppx_ocv_highgui_trackbar_release_handle(jyppx_ocv_highgui_trackbar* trackbar);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_get_trackbar_pos(const char* trackbarname, const char* winname, int* pos);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_set_trackbar_pos(const char* trackbarname, const char* winname, int pos);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_set_trackbar_min(const char* trackbarname, const char* winname, int minval);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_set_trackbar_max(const char* trackbarname, const char* winname, int maxval);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_set_mouse_callback(
    const char* winname,
    jyppx_ocv_highgui_mouse_callback callback,
    void* userdata);
OPENCV_CSHARP_EXTERN_C OPENCV_CSHARP_API int jyppx_ocv_highgui_create_button(
    const char* button_name,
    jyppx_ocv_highgui_button_callback callback,
    void* userdata,
    int type,
    int initial_button_state);
