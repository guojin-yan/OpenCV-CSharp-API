#include "open_cv_sharp/alphamat/alphamat.h"

#include "../core/mat_handle.h"
#include "../error_state.h"

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ALPHAMAT)
#include <opencv2/alphamat.hpp>
#endif

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }
}

int jyppx_ocv_alphamat_info_flow(const jyppx_ocv_mat* image, const jyppx_ocv_mat* trimap, jyppx_ocv_mat* result)
{
    constexpr const char* api_name = "jyppx_ocv_alphamat_info_flow";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, trimap, "trimap");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_ALPHAMAT)
        cv::alphamat::infoFlow(
            opencv_csharp_native::mat_value(image),
            opencv_csharp_native::mat_value(trimap),
            opencv_csharp_native::mat_value(result));
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

