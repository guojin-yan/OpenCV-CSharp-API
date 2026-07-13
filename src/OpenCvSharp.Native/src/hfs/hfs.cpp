#include "open_cv_sharp/hfs/hfs.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "hfs_handles.h"

#include <new>

namespace
{
    constexpr int HFS_FLOAT_PROPERTY_SEG_EGB_THRESHOLD_I = 0;
    constexpr int HFS_FLOAT_PROPERTY_SEG_EGB_THRESHOLD_II = 1;
    constexpr int HFS_FLOAT_PROPERTY_SPATIAL_WEIGHT = 2;

    constexpr int HFS_INT_PROPERTY_MIN_REGION_SIZE_I = 0;
    constexpr int HFS_INT_PROPERTY_MIN_REGION_SIZE_II = 1;
    constexpr int HFS_INT_PROPERTY_SLIC_SPIXEL_SIZE = 2;
    constexpr int HFS_INT_PROPERTY_NUM_SLIC_ITER = 3;

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_segment(const char* api_name, const jyppx_ocv_hfs_segment* segment)
    {
        return segment == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "segment")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int(const char* api_name, const int* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }
}

int jyppx_ocv_hfs_segment_create(
    int height,
    int width,
    float seg_egb_threshold_i,
    int min_region_size_i,
    float seg_egb_threshold_ii,
    int min_region_size_ii,
    float spatial_weight,
    int slic_spixel_size,
    int num_slic_iter,
    jyppx_ocv_hfs_segment** segment)
{
    constexpr const char* api_name = "jyppx_ocv_hfs_segment_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (segment == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "segment");
        }

        *segment = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_HFS)
        jyppx_ocv_hfs_segment* created = new (std::nothrow) jyppx_ocv_hfs_segment();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::hfs::HfsSegment::create(
            height,
            width,
            seg_egb_threshold_i,
            min_region_size_i,
            seg_egb_threshold_ii,
            min_region_size_ii,
            spatial_weight,
            slic_spixel_size,
            num_slic_iter);
        *segment = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)height; (void)width; (void)seg_egb_threshold_i; (void)min_region_size_i;
        (void)seg_egb_threshold_ii; (void)min_region_size_ii; (void)spatial_weight;
        (void)slic_spixel_size; (void)num_slic_iter;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_hfs_segment_release(jyppx_ocv_hfs_segment* segment)
{
    delete segment;
}

int jyppx_ocv_hfs_segment_get_float_property(const jyppx_ocv_hfs_segment* segment, int property_id, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_hfs_segment_get_float_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_segment(api_name, segment);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_HFS)
        switch (property_id)
        {
        case HFS_FLOAT_PROPERTY_SEG_EGB_THRESHOLD_I: *value = segment->value->getSegEgbThresholdI(); return OPENCV_CSHARP_STATUS_OK;
        case HFS_FLOAT_PROPERTY_SEG_EGB_THRESHOLD_II: *value = segment->value->getSegEgbThresholdII(); return OPENCV_CSHARP_STATUS_OK;
        case HFS_FLOAT_PROPERTY_SPATIAL_WEIGHT: *value = segment->value->getSpatialWeight(); return OPENCV_CSHARP_STATUS_OK;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }
#else
        (void)property_id;
        *value = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hfs_segment_set_float_property(jyppx_ocv_hfs_segment* segment, int property_id, float value)
{
    constexpr const char* api_name = "jyppx_ocv_hfs_segment_set_float_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_segment(api_name, segment);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_HFS)
        switch (property_id)
        {
        case HFS_FLOAT_PROPERTY_SEG_EGB_THRESHOLD_I: segment->value->setSegEgbThresholdI(value); return OPENCV_CSHARP_STATUS_OK;
        case HFS_FLOAT_PROPERTY_SEG_EGB_THRESHOLD_II: segment->value->setSegEgbThresholdII(value); return OPENCV_CSHARP_STATUS_OK;
        case HFS_FLOAT_PROPERTY_SPATIAL_WEIGHT: segment->value->setSpatialWeight(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_hfs_segment_get_int_property(const jyppx_ocv_hfs_segment* segment, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_hfs_segment_get_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_segment(api_name, segment);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_HFS)
        switch (property_id)
        {
        case HFS_INT_PROPERTY_MIN_REGION_SIZE_I: *value = segment->value->getMinRegionSizeI(); return OPENCV_CSHARP_STATUS_OK;
        case HFS_INT_PROPERTY_MIN_REGION_SIZE_II: *value = segment->value->getMinRegionSizeII(); return OPENCV_CSHARP_STATUS_OK;
        case HFS_INT_PROPERTY_SLIC_SPIXEL_SIZE: *value = segment->value->getSlicSpixelSize(); return OPENCV_CSHARP_STATUS_OK;
        case HFS_INT_PROPERTY_NUM_SLIC_ITER: *value = segment->value->getNumSlicIter(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_hfs_segment_set_int_property(jyppx_ocv_hfs_segment* segment, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_hfs_segment_set_int_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_segment(api_name, segment);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_HFS)
        switch (property_id)
        {
        case HFS_INT_PROPERTY_MIN_REGION_SIZE_I: segment->value->setMinRegionSizeI(value); return OPENCV_CSHARP_STATUS_OK;
        case HFS_INT_PROPERTY_MIN_REGION_SIZE_II: segment->value->setMinRegionSizeII(value); return OPENCV_CSHARP_STATUS_OK;
        case HFS_INT_PROPERTY_SLIC_SPIXEL_SIZE: segment->value->setSlicSpixelSize(value); return OPENCV_CSHARP_STATUS_OK;
        case HFS_INT_PROPERTY_NUM_SLIC_ITER: segment->value->setNumSlicIter(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_hfs_segment_perform_segment_cpu(
    jyppx_ocv_hfs_segment* segment,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int draw)
{
    constexpr const char* api_name = "jyppx_ocv_hfs_segment_perform_segment_cpu";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_segment(api_name, segment);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_HFS)
        opencv_csharp_native::mat_value(dst) = segment->value->performSegmentCpu(opencv_csharp_native::mat_value(src), draw != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)draw;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hfs_segment_perform_segment_gpu(
    jyppx_ocv_hfs_segment* segment,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int draw)
{
    constexpr const char* api_name = "jyppx_ocv_hfs_segment_perform_segment_gpu";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_segment(api_name, segment);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_HFS)
        opencv_csharp_native::mat_value(dst) = segment->value->performSegmentGpu(opencv_csharp_native::mat_value(src), draw != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)draw;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

