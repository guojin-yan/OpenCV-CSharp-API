#include "open_cv_5_sharp/alphamat/alphamat.h"
#include "open_cv_5_sharp/bgsegm/bgsegm.h"
#include "open_cv_5_sharp/bioinspired/bioinspired.h"
#include "open_cv_5_sharp/calib3d/calib3d.h"
#include "open_cv_5_sharp/core/decomp.h"
#include "open_cv_5_sharp/core/mat.h"
#include "open_cv_5_sharp/core/operations.h"
#include "open_cv_5_sharp/core/persistence.h"
#include "open_cv_5_sharp/core/utility.h"
#include "open_cv_5_sharp/dnn/dnn.h"
#include "open_cv_5_sharp/error.h"
#include "open_cv_5_sharp/export.h"
#include "open_cv_5_sharp/face/face.h"
#include "open_cv_5_sharp/features2d/features2d.h"
#include "open_cv_5_sharp/features2d/types.h"
#include "open_cv_5_sharp/fuzzy/fuzzy.h"
#include "open_cv_5_sharp/hfs/hfs.h"
#include "open_cv_5_sharp/highgui/highgui.h"
#include "open_cv_5_sharp/img_hash/img_hash.h"
#include "open_cv_5_sharp/imgcodecs.h"
#include "open_cv_5_sharp/imgproc.h"
#include "open_cv_5_sharp/intensity_transform/intensity_transform.h"
#include "open_cv_5_sharp/line_descriptor/line_descriptor.h"
#include "open_cv_5_sharp/ml/ml.h"
#include "open_cv_5_sharp/objdetect/aruco.h"
#include "open_cv_5_sharp/objdetect/objdetect.h"
#include "open_cv_5_sharp/optflow/optflow.h"
#include "open_cv_5_sharp/phase_unwrapping/phase_unwrapping.h"
#include "open_cv_5_sharp/photo/photo.h"
#include "open_cv_5_sharp/plot/plot.h"
#include "open_cv_5_sharp/ptcloud/ptcloud.h"
#include "open_cv_5_sharp/quality/quality.h"
#include "open_cv_5_sharp/rapid/rapid.h"
#include "open_cv_5_sharp/reg/reg.h"
#include "open_cv_5_sharp/saliency/saliency.h"
#include "open_cv_5_sharp/shape/shape.h"
#include "open_cv_5_sharp/status.h"
#include "open_cv_5_sharp/stitching/stitching.h"
#include "open_cv_5_sharp/structured_light/structured_light.h"
#include "open_cv_5_sharp/surface_matching/surface_matching.h"
#include "open_cv_5_sharp/tracking/tracking.h"
#include "open_cv_5_sharp/version.h"
#include "open_cv_5_sharp/video/video.h"
#include "open_cv_5_sharp/videoio/videoio.h"
#include "open_cv_5_sharp/ximgproc/ximgproc.h"
#include "open_cv_5_sharp/xobjdetect/xobjdetect.h"
#include "open_cv_5_sharp/xphoto/xphoto.h"
#include "open_cv_5_sharp/xstereo/xstereo.h"

#include <cstring>

int main()
{
    static_assert(
        JYPPX_OCV5_REG_MAP_KIND_UNKNOWN == JYPPX_OCV_REG_MAP_KIND_UNKNOWN,
        "Compatibility Reg unknown kind alias must remain source-compatible.");
    static_assert(
        JYPPX_OCV5_REG_MAP_KIND_SHIFT == JYPPX_OCV_REG_MAP_KIND_SHIFT,
        "Compatibility Reg shift kind alias must remain source-compatible.");
    static_assert(
        JYPPX_OCV5_REG_MAP_KIND_AFFINE == JYPPX_OCV_REG_MAP_KIND_AFFINE,
        "Compatibility Reg affine kind alias must remain source-compatible.");
    static_assert(
        JYPPX_OCV5_REG_MAP_KIND_PROJEC == JYPPX_OCV_REG_MAP_KIND_PROJEC,
        "Compatibility Reg projective kind alias must remain source-compatible.");

    if (OPENCV5SHARP_STATUS_OK != OPENCV_CSHARP_STATUS_OK ||
        OPENCV5SHARP_STATUS_NOT_LINKED != OPENCV_CSHARP_STATUS_NOT_LINKED ||
        OPENCV5SHARP_STATUS_EXCEPTION != OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION)
    {
        return 1;
    }

    if (jyppx_ocv5_get_version_major() != jyppx_ocv_get_version_major() ||
        jyppx_ocv5_get_version_minor() != jyppx_ocv_get_version_minor() ||
        jyppx_ocv5_get_version_revision() != jyppx_ocv_get_version_revision())
    {
        return 2;
    }

    const char* legacyVersion = jyppx_ocv5_get_version_string();
    const char* neutralVersion = jyppx_ocv_get_version_string();
    return legacyVersion != nullptr &&
                   neutralVersion != nullptr &&
                   std::strcmp(legacyVersion, neutralVersion) == 0 &&
                   std::strlen(legacyVersion) > 0
               ? 0
               : 3;
}
