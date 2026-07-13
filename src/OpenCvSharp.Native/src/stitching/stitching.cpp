#include "open_cv_sharp/stitching/stitching.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "stitching_handles.h"

#include <new>
#include <vector>

namespace
{
    constexpr int DOUBLE_PROPERTY_REGISTRATION_RESOL = 0;
    constexpr int DOUBLE_PROPERTY_SEAM_ESTIMATION_RESOL = 1;
    constexpr int DOUBLE_PROPERTY_COMPOSITING_RESOL = 2;
    constexpr int DOUBLE_PROPERTY_PANO_CONFIDENCE_THRESH = 3;
    constexpr int DOUBLE_PROPERTY_WORK_SCALE = 4;

    constexpr int INT_PROPERTY_WAVE_CORRECTION = 0;
    constexpr int INT_PROPERTY_INTERPOLATION_FLAGS = 1;
    constexpr int INT_PROPERTY_WAVE_CORRECT_KIND = 2;

    int validate_stitcher(const char* api_name, const jyppx_ocv_stitcher* stitcher)
    {
        return stitcher == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "stitcher")
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

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mat_array(const char* api_name, const jyppx_ocv_mat* const* mats, int mat_count, const char* argument_name)
    {
        if (mat_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (mat_count > 0 && mats == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < mat_count; ++i)
        {
            if (mats[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_optional_masks(
        const char* api_name,
        const jyppx_ocv_mat* const* masks,
        int mask_count,
        int image_count)
    {
        if (mask_count == 0)
        {
            return OPENCV_CSHARP_STATUS_OK;
        }

        if (mask_count != image_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "masks");
        }

        return validate_mat_array(api_name, masks, mask_count, "masks");
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
    std::vector<cv::Mat> to_mat_vector(const jyppx_ocv_mat* const* mats, int mat_count)
    {
        std::vector<cv::Mat> result;
        result.reserve(static_cast<size_t>(mat_count));
        for (int i = 0; i < mat_count; ++i)
        {
            result.push_back(opencv_csharp_native::mat_value(mats[i]));
        }

        return result;
    }

    int create_mat_handle(const char* api_name, const cv::Mat& value, jyppx_ocv_mat** mat)
    {
        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        *mat = nullptr;
        jyppx_ocv_mat* created = new (std::nothrow) jyppx_ocv_mat();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = value;
        *mat = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_camera_params(
        const char* api_name,
        const cv::detail::CameraParams& source,
        jyppx_ocv_stitching_camera_params* destination)
    {
        destination->focal = source.focal;
        destination->aspect = source.aspect;
        destination->ppx = source.ppx;
        destination->ppy = source.ppy;
        destination->r = nullptr;
        destination->t = nullptr;

        int status = create_mat_handle(api_name, source.R, &destination->r);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = create_mat_handle(api_name, source.t, &destination->t);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            delete destination->r;
            destination->r = nullptr;
            return status;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_stitcher_create(int mode, jyppx_ocv_stitcher** stitcher)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (stitcher == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "stitcher");
        }

        *stitcher = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        jyppx_ocv_stitcher* created = new (std::nothrow) jyppx_ocv_stitcher();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::Stitcher::create(static_cast<cv::Stitcher::Mode>(mode));
        *stitcher = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mode;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_stitcher_release_handle(jyppx_ocv_stitcher* stitcher)
{
    delete stitcher;
}

int jyppx_ocv_stitcher_get_double_property(const jyppx_ocv_stitcher* stitcher, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_double_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        switch (property_id)
        {
        case DOUBLE_PROPERTY_REGISTRATION_RESOL: *value = stitcher->value->registrationResol(); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_SEAM_ESTIMATION_RESOL: *value = stitcher->value->seamEstimationResol(); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_COMPOSITING_RESOL: *value = stitcher->value->compositingResol(); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_PANO_CONFIDENCE_THRESH: *value = stitcher->value->panoConfidenceThresh(); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_WORK_SCALE: *value = stitcher->value->workScale(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_set_double_property(jyppx_ocv_stitcher* stitcher, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_set_double_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        switch (property_id)
        {
        case DOUBLE_PROPERTY_REGISTRATION_RESOL: stitcher->value->setRegistrationResol(value); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_SEAM_ESTIMATION_RESOL: stitcher->value->setSeamEstimationResol(value); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_COMPOSITING_RESOL: stitcher->value->setCompositingResol(value); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_PANO_CONFIDENCE_THRESH: stitcher->value->setPanoConfidenceThresh(value); return OPENCV_CSHARP_STATUS_OK;
        case DOUBLE_PROPERTY_WORK_SCALE: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_int_property(const jyppx_ocv_stitcher* stitcher, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        switch (property_id)
        {
        case INT_PROPERTY_WAVE_CORRECTION: *value = stitcher->value->waveCorrection() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case INT_PROPERTY_INTERPOLATION_FLAGS: *value = static_cast<int>(stitcher->value->interpolationFlags()); return OPENCV_CSHARP_STATUS_OK;
        case INT_PROPERTY_WAVE_CORRECT_KIND: *value = static_cast<int>(stitcher->value->waveCorrectKind()); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_stitcher_set_int_property(jyppx_ocv_stitcher* stitcher, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_set_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        switch (property_id)
        {
        case INT_PROPERTY_WAVE_CORRECTION: stitcher->value->setWaveCorrection(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case INT_PROPERTY_INTERPOLATION_FLAGS: stitcher->value->setInterpolationFlags(static_cast<cv::InterpolationFlags>(value)); return OPENCV_CSHARP_STATUS_OK;
        case INT_PROPERTY_WAVE_CORRECT_KIND: stitcher->value->setWaveCorrectKind(static_cast<cv::detail::WaveCorrectKind>(value)); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_estimate_transform(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    int* status_code)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_estimate_transform";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_optional_masks(api_name, masks, mask_count, image_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, status_code, "status_code");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Mat> native_images = to_mat_vector(images, image_count);
        if (mask_count == 0)
        {
            *status_code = static_cast<int>(stitcher->value->estimateTransform(native_images));
        }
        else
        {
            std::vector<cv::Mat> native_masks = to_mat_vector(masks, mask_count);
            *status_code = static_cast<int>(stitcher->value->estimateTransform(native_images, native_masks));
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)images; (void)image_count; (void)masks; (void)mask_count;
        *status_code = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_compose_panorama(jyppx_ocv_stitcher* stitcher, jyppx_ocv_mat* pano, int* status_code)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_compose_panorama";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, pano, "pano");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, status_code, "status_code");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *status_code = static_cast<int>(stitcher->value->composePanorama(opencv_csharp_native::mat_value(pano)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *status_code = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_compose_panorama_images(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    jyppx_ocv_mat* pano,
    int* status_code)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_compose_panorama_images";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, pano, "pano");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, status_code, "status_code");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Mat> native_images = to_mat_vector(images, image_count);
        *status_code = static_cast<int>(stitcher->value->composePanorama(native_images, opencv_csharp_native::mat_value(pano)));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)images; (void)image_count;
        *status_code = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_stitch(
    jyppx_ocv_stitcher* stitcher,
    const jyppx_ocv_mat* const* images,
    int image_count,
    const jyppx_ocv_mat* const* masks,
    int mask_count,
    jyppx_ocv_mat* pano,
    int* status_code)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_stitch";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_optional_masks(api_name, masks, mask_count, image_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, pano, "pano");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, status_code, "status_code");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::Mat> native_images = to_mat_vector(images, image_count);
        if (mask_count == 0)
        {
            *status_code = static_cast<int>(stitcher->value->stitch(native_images, opencv_csharp_native::mat_value(pano)));
        }
        else
        {
            std::vector<cv::Mat> native_masks = to_mat_vector(masks, mask_count);
            *status_code = static_cast<int>(stitcher->value->stitch(native_images, native_masks, opencv_csharp_native::mat_value(pano)));
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)images; (void)image_count; (void)masks; (void)mask_count;
        *status_code = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_component_count(const jyppx_ocv_stitcher* stitcher, int* component_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_component_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, component_count, "component_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *component_count = static_cast<int>(stitcher->value->component().size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *component_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_component_fill(const jyppx_ocv_stitcher* stitcher, int* components, int component_capacity, int* component_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_component_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (component_capacity < 0 || (component_capacity > 0 && components == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "components");
        }
        status = validate_output_int(api_name, component_count, "component_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<int> values = stitcher->value->component();
        *component_count = static_cast<int>(values.size());
        const int writable = component_capacity < *component_count ? component_capacity : *component_count;
        for (int i = 0; i < writable; ++i)
        {
            components[i] = values[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        *component_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_cameras_count(const jyppx_ocv_stitcher* stitcher, int* camera_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_cameras_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, camera_count, "camera_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        *camera_count = static_cast<int>(stitcher->value->cameras().size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *camera_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_cameras_fill(
    const jyppx_ocv_stitcher* stitcher,
    jyppx_ocv_stitching_camera_params* cameras,
    int camera_capacity,
    int* camera_count)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_cameras_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (camera_capacity < 0 || (camera_capacity > 0 && cameras == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "cameras");
        }
        status = validate_output_int(api_name, camera_count, "camera_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        std::vector<cv::detail::CameraParams> values = stitcher->value->cameras();
        *camera_count = static_cast<int>(values.size());
        const int writable = camera_capacity < *camera_count ? camera_capacity : *camera_count;
        for (int i = 0; i < writable; ++i)
        {
            status = copy_camera_params(api_name, values[static_cast<size_t>(i)], &cameras[i]);
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                for (int cleanup = 0; cleanup < i; ++cleanup)
                {
                    delete cameras[cleanup].r;
                    delete cameras[cleanup].t;
                    cameras[cleanup].r = nullptr;
                    cameras[cleanup].t = nullptr;
                }

                return status;
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        *camera_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_stitcher_get_result_mask(const jyppx_ocv_stitcher* stitcher, jyppx_ocv_mat* result_mask)
{
    constexpr const char* api_name = "jyppx_ocv_stitcher_get_result_mask";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_stitcher(api_name, stitcher);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, result_mask, "result_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_STITCHING)
        stitcher->value->resultMask().copyTo(opencv_csharp_native::mat_value(result_mask));
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

