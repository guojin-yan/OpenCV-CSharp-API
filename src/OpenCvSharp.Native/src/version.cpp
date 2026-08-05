#include "open_cv_sharp/version.h"

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/core/version.hpp>
#endif

int jyppx_ocv_get_native_abi_version(void)
{
    return OPENCV_CSHARP_NATIVE_ABI_VERSION;
}

int jyppx_ocv_get_version_major(void)
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    return CV_VERSION_MAJOR;
#else
    return 5;
#endif
}

int jyppx_ocv_get_version_minor(void)
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    return CV_VERSION_MINOR;
#else
    return 0;
#endif
}

int jyppx_ocv_get_version_revision(void)
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    return CV_VERSION_REVISION;
#else
    return 0;
#endif
}

const char* jyppx_ocv_get_version_string(void)
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    return CV_VERSION;
#else
    return "5.0.0";
#endif
}
