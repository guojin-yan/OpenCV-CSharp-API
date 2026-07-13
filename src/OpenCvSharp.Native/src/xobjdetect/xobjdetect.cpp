#include "open_cv_sharp/xobjdetect/xobjdetect.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "xobjdetect_handles.h"

#include <limits>
#include <new>
#include <vector>

namespace
{
    int validate_cascade(const char* api_name, const jyppx_ocv_cascade_classifier* classifier)
    {
        return classifier == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "classifier")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_hog(const char* api_name, const jyppx_ocv_hog_descriptor* descriptor)
    {
        return descriptor == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "descriptor")
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

    int validate_output_size_t(const char* api_name, const size_t* value, const char* argument_name)
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

    int validate_filename(const char* api_name, const char* filename)
    {
        if (filename == nullptr || filename[0] == '\0')
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_float_array(const char* api_name, const float* values, int value_count)
    {
        if (value_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "values");
        }

        if (value_count > 0 && values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "values");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    void assign_bool(int* destination, bool value)
    {
        if (destination != nullptr)
        {
            *destination = value ? 1 : 0;
        }
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
    cv::Size to_size(int width, int height)
    {
        return cv::Size(width, height);
    }

    int set_count_from_size(const char* api_name, size_t size, int* count)
    {
        if (count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "count");
        }

        if (size > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "count");
        }

        *count = static_cast<int>(size);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_rectangles(const char* api_name, const std::vector<cv::Rect>& rectangles, int* buffer, int buffer_capacity, int* count)
    {
        int status = set_count_from_size(api_name, rectangles.size(), count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        const int required = (*count) * 4;
        if (buffer == nullptr || buffer_capacity < required)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rectangles");
        }

        for (int i = 0; i < *count; ++i)
        {
            const cv::Rect& rect = rectangles[static_cast<size_t>(i)];
            const int offset = i * 4;
            buffer[offset] = rect.x;
            buffer[offset + 1] = rect.y;
            buffer[offset + 2] = rect.width;
            buffer[offset + 3] = rect.height;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_points(const char* api_name, const std::vector<cv::Point>& points, int* buffer, int buffer_capacity, int* count)
    {
        int status = set_count_from_size(api_name, points.size(), count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        const int required = (*count) * 2;
        if (buffer == nullptr || buffer_capacity < required)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "locations_xy");
        }

        for (int i = 0; i < *count; ++i)
        {
            const cv::Point& point = points[static_cast<size_t>(i)];
            const int offset = i * 2;
            buffer[offset] = point.x;
            buffer[offset + 1] = point.y;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_ints(const char* api_name, const std::vector<int>& values, int* buffer, int buffer_capacity, const char* argument_name)
    {
        if (values.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        const int count = static_cast<int>(values.size());
        if (buffer == nullptr || buffer_capacity < count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < count; ++i)
        {
            buffer[i] = values[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_doubles(const char* api_name, const std::vector<double>& values, double* buffer, int buffer_capacity, const char* argument_name)
    {
        if (values.size() > static_cast<size_t>(std::numeric_limits<int>::max()))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        const int count = static_cast<int>(values.size());
        if (buffer == nullptr || buffer_capacity < count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < count; ++i)
        {
            buffer[i] = values[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_floats(const char* api_name, const std::vector<float>& values, float* buffer, int buffer_capacity, int* count)
    {
        int status = set_count_from_size(api_name, values.size(), count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        if (buffer == nullptr || buffer_capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "values");
        }

        for (int i = 0; i < *count; ++i)
        {
            buffer[i] = values[static_cast<size_t>(i)];
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    std::vector<float> to_float_vector(const float* values, int value_count)
    {
        return value_count <= 0 ? std::vector<float>() : std::vector<float>(values, values + value_count);
    }
#endif
}

int jyppx_ocv_cascade_classifier_create(jyppx_ocv_cascade_classifier** classifier)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (classifier == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "classifier");
        }

        *classifier = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        jyppx_ocv_cascade_classifier* created = new (std::nothrow) jyppx_ocv_cascade_classifier();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *classifier = created;
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

int jyppx_ocv_cascade_classifier_create_from_file(const char* filename, jyppx_ocv_cascade_classifier** classifier)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_create_from_file";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_filename(api_name, filename);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (classifier == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "classifier");
        }

        *classifier = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        jyppx_ocv_cascade_classifier* created = new (std::nothrow) jyppx_ocv_cascade_classifier{ cv::CascadeClassifier(filename) };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *classifier = created;
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

void jyppx_ocv_cascade_classifier_release_handle(jyppx_ocv_cascade_classifier* classifier)
{
    delete classifier;
}

int jyppx_ocv_cascade_classifier_load(jyppx_ocv_cascade_classifier* classifier, const char* filename, int* loaded)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_load";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_cascade(api_name, classifier);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_filename(api_name, filename);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, loaded, "loaded");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        assign_bool(loaded, classifier->value.load(filename));
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(loaded, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_cascade_classifier_empty(const jyppx_ocv_cascade_classifier* classifier, int* empty)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_empty";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_cascade(api_name, classifier);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, empty, "empty");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        assign_bool(empty, classifier->value.empty());
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(empty, true);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_cascade_classifier_get_original_window_size(const jyppx_ocv_cascade_classifier* classifier, int* width, int* height)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_get_original_window_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_cascade(api_name, classifier);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, width, "width");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, height, "height");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        cv::Size size = classifier->value.getOriginalWindowSize();
        *width = size.width;
        *height = size.height;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *width = 0;
        *height = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_cascade_classifier_is_old_format_cascade(const jyppx_ocv_cascade_classifier* classifier, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_is_old_format_cascade";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_cascade(api_name, classifier);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        assign_bool(result, classifier->value.isOldFormatCascade());
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(result, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_cascade_classifier_get_feature_type(const jyppx_ocv_cascade_classifier* classifier, int* feature_type)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_get_feature_type";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_cascade(api_name, classifier);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, feature_type, "feature_type");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        *feature_type = classifier->value.getFeatureType();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *feature_type = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_cascade_classifier_detect_multi_scale_count(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_detect_multi_scale_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_cascade(api_name, classifier);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        std::vector<cv::Rect> rectangles;
        classifier->value.detectMultiScale(opencv_csharp_native::mat_value(image), rectangles, scale_factor, min_neighbors, flags, to_size(min_width, min_height), to_size(max_width, max_height));
        return set_count_from_size(api_name, rectangles.size(), count);
#else
        (void)scale_factor;
        (void)min_neighbors;
        (void)flags;
        (void)min_width;
        (void)min_height;
        (void)max_width;
        (void)max_height;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_cascade_classifier_detect_multi_scale_fill(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int* rectangles,
    int rectangle_capacity,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_detect_multi_scale_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_cascade(api_name, classifier);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        std::vector<cv::Rect> values;
        classifier->value.detectMultiScale(opencv_csharp_native::mat_value(image), values, scale_factor, min_neighbors, flags, to_size(min_width, min_height), to_size(max_width, max_height));
        return copy_rectangles(api_name, values, rectangles, rectangle_capacity, count);
#else
        (void)scale_factor;
        (void)min_neighbors;
        (void)flags;
        (void)min_width;
        (void)min_height;
        (void)max_width;
        (void)max_height;
        (void)rectangles;
        (void)rectangle_capacity;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_cascade_classifier_detect_multi_scale2_count(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int* count)
{
    return jyppx_ocv_cascade_classifier_detect_multi_scale_count(
        classifier,
        image,
        scale_factor,
        min_neighbors,
        flags,
        min_width,
        min_height,
        max_width,
        max_height,
        count);
}

int jyppx_ocv_cascade_classifier_detect_multi_scale2_fill(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int* rectangles,
    int rectangle_capacity,
    int* num_detections,
    int num_detection_capacity,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_detect_multi_scale2_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_cascade(api_name, classifier);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        std::vector<cv::Rect> values;
        std::vector<int> detections;
        classifier->value.detectMultiScale(opencv_csharp_native::mat_value(image), values, detections, scale_factor, min_neighbors, flags, to_size(min_width, min_height), to_size(max_width, max_height));
        status = copy_rectangles(api_name, values, rectangles, rectangle_capacity, count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_ints(api_name, detections, num_detections, num_detection_capacity, "num_detections");
#else
        (void)scale_factor;
        (void)min_neighbors;
        (void)flags;
        (void)min_width;
        (void)min_height;
        (void)max_width;
        (void)max_height;
        (void)rectangles;
        (void)rectangle_capacity;
        (void)num_detections;
        (void)num_detection_capacity;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_cascade_classifier_detect_multi_scale3_count(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int output_reject_levels,
    int* count)
{
    (void)output_reject_levels;
    return jyppx_ocv_cascade_classifier_detect_multi_scale_count(
        classifier,
        image,
        scale_factor,
        min_neighbors,
        flags,
        min_width,
        min_height,
        max_width,
        max_height,
        count);
}

int jyppx_ocv_cascade_classifier_detect_multi_scale3_fill(
    jyppx_ocv_cascade_classifier* classifier,
    const jyppx_ocv_mat* image,
    double scale_factor,
    int min_neighbors,
    int flags,
    int min_width,
    int min_height,
    int max_width,
    int max_height,
    int output_reject_levels,
    int* rectangles,
    int rectangle_capacity,
    int* reject_levels,
    int reject_level_capacity,
    double* level_weights,
    int level_weight_capacity,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_cascade_classifier_detect_multi_scale3_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_cascade(api_name, classifier);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        std::vector<cv::Rect> values;
        std::vector<int> levels;
        std::vector<double> weights;
        classifier->value.detectMultiScale(opencv_csharp_native::mat_value(image), values, levels, weights, scale_factor, min_neighbors, flags, to_size(min_width, min_height), to_size(max_width, max_height), output_reject_levels != 0);
        status = copy_rectangles(api_name, values, rectangles, rectangle_capacity, count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = copy_ints(api_name, levels, reject_levels, reject_level_capacity, "reject_levels");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_doubles(api_name, weights, level_weights, level_weight_capacity, "level_weights");
#else
        (void)scale_factor;
        (void)min_neighbors;
        (void)flags;
        (void)min_width;
        (void)min_height;
        (void)max_width;
        (void)max_height;
        (void)output_reject_levels;
        (void)rectangles;
        (void)rectangle_capacity;
        (void)reject_levels;
        (void)reject_level_capacity;
        (void)level_weights;
        (void)level_weight_capacity;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_create(jyppx_ocv_hog_descriptor** descriptor)
{
    return jyppx_ocv_hog_descriptor_create_with_params(64, 128, 16, 16, 8, 8, 8, 8, 9, 1, -1.0, 0, 0.2, 1, 64, 0, descriptor);
}

int jyppx_ocv_hog_descriptor_create_with_params(
    int win_width,
    int win_height,
    int block_width,
    int block_height,
    int block_stride_width,
    int block_stride_height,
    int cell_width,
    int cell_height,
    int nbins,
    int deriv_aperture,
    double win_sigma,
    int histogram_norm_type,
    double l2_hys_threshold,
    int gamma_correction,
    int nlevels,
    int signed_gradient,
    jyppx_ocv_hog_descriptor** descriptor)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_create_with_params";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (descriptor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "descriptor");
        }

        *descriptor = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        jyppx_ocv_hog_descriptor* created = new (std::nothrow) jyppx_ocv_hog_descriptor{
            cv::HOGDescriptor(
                to_size(win_width, win_height),
                to_size(block_width, block_height),
                to_size(block_stride_width, block_stride_height),
                to_size(cell_width, cell_height),
                nbins,
                deriv_aperture,
                win_sigma,
                static_cast<cv::HOGDescriptor::HistogramNormType>(histogram_norm_type),
                l2_hys_threshold,
                gamma_correction != 0,
                nlevels,
                signed_gradient != 0)
        };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *descriptor = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)win_width;
        (void)win_height;
        (void)block_width;
        (void)block_height;
        (void)block_stride_width;
        (void)block_stride_height;
        (void)cell_width;
        (void)cell_height;
        (void)nbins;
        (void)deriv_aperture;
        (void)win_sigma;
        (void)histogram_norm_type;
        (void)l2_hys_threshold;
        (void)gamma_correction;
        (void)nlevels;
        (void)signed_gradient;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_create_from_file(const char* filename, jyppx_ocv_hog_descriptor** descriptor)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_create_from_file";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_filename(api_name, filename);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (descriptor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "descriptor");
        }

        *descriptor = nullptr;

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        jyppx_ocv_hog_descriptor* created = new (std::nothrow) jyppx_ocv_hog_descriptor{ cv::HOGDescriptor(filename) };
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *descriptor = created;
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

void jyppx_ocv_hog_descriptor_release_handle(jyppx_ocv_hog_descriptor* descriptor)
{
    delete descriptor;
}

int jyppx_ocv_hog_descriptor_get_default_people_detector_count(int* count)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_get_default_people_detector_count";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        return set_count_from_size(api_name, cv::HOGDescriptor::getDefaultPeopleDetector().size(), count);
#else
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_get_default_people_detector_fill(float* values, int value_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_get_default_people_detector_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        return copy_floats(api_name, cv::HOGDescriptor::getDefaultPeopleDetector(), values, value_capacity, count);
#else
        (void)values;
        (void)value_capacity;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_get_daimler_people_detector_count(int* count)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_get_daimler_people_detector_count";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        return set_count_from_size(api_name, cv::HOGDescriptor::getDaimlerPeopleDetector().size(), count);
#else
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_get_daimler_people_detector_fill(float* values, int value_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_get_daimler_people_detector_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        return copy_floats(api_name, cv::HOGDescriptor::getDaimlerPeopleDetector(), values, value_capacity, count);
#else
        (void)values;
        (void)value_capacity;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_set_svm_detector(jyppx_ocv_hog_descriptor* descriptor, const float* values, int value_count)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_set_svm_detector";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_hog(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_float_array(api_name, values, value_count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        descriptor->value.setSVMDetector(to_float_vector(values, value_count));
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

int jyppx_ocv_hog_descriptor_check_detector_size(const jyppx_ocv_hog_descriptor* descriptor, int* result)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_check_detector_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_hog(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        assign_bool(result, descriptor->value.checkDetectorSize());
        return OPENCV_CSHARP_STATUS_OK;
#else
        assign_bool(result, false);
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_get_descriptor_size(const jyppx_ocv_hog_descriptor* descriptor, size_t* descriptor_size)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_get_descriptor_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_hog(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_size_t(api_name, descriptor_size, "descriptor_size");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        *descriptor_size = descriptor->value.getDescriptorSize();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *descriptor_size = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_get_win_sigma(const jyppx_ocv_hog_descriptor* descriptor, double* win_sigma)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_get_win_sigma";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_hog(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, win_sigma, "win_sigma");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        *win_sigma = descriptor->value.getWinSigma();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *win_sigma = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_get_property(const jyppx_ocv_hog_descriptor* descriptor, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_get_property";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_hog(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        switch (property_id)
        {
        case 0: *value = descriptor->value.winSize.width; break;
        case 1: *value = descriptor->value.winSize.height; break;
        case 2: *value = descriptor->value.blockSize.width; break;
        case 3: *value = descriptor->value.blockSize.height; break;
        case 4: *value = descriptor->value.blockStride.width; break;
        case 5: *value = descriptor->value.blockStride.height; break;
        case 6: *value = descriptor->value.cellSize.width; break;
        case 7: *value = descriptor->value.cellSize.height; break;
        case 8: *value = descriptor->value.nbins; break;
        case 9: *value = descriptor->value.derivAperture; break;
        case 10: *value = descriptor->value.winSigma; break;
        case 11: *value = static_cast<int>(descriptor->value.histogramNormType); break;
        case 12: *value = descriptor->value.L2HysThreshold; break;
        case 13: *value = descriptor->value.gammaCorrection ? 1.0 : 0.0; break;
        case 14: *value = descriptor->value.nlevels; break;
        case 15: *value = descriptor->value.signedGradient ? 1.0 : 0.0; break;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }

        return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_hog_descriptor_set_property(jyppx_ocv_hog_descriptor* descriptor, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_set_property";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_hog(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        switch (property_id)
        {
        case 0: descriptor->value.winSize.width = static_cast<int>(value); break;
        case 1: descriptor->value.winSize.height = static_cast<int>(value); break;
        case 2: descriptor->value.blockSize.width = static_cast<int>(value); break;
        case 3: descriptor->value.blockSize.height = static_cast<int>(value); break;
        case 4: descriptor->value.blockStride.width = static_cast<int>(value); break;
        case 5: descriptor->value.blockStride.height = static_cast<int>(value); break;
        case 6: descriptor->value.cellSize.width = static_cast<int>(value); break;
        case 7: descriptor->value.cellSize.height = static_cast<int>(value); break;
        case 8: descriptor->value.nbins = static_cast<int>(value); break;
        case 9: descriptor->value.derivAperture = static_cast<int>(value); break;
        case 10: descriptor->value.winSigma = value; break;
        case 11: descriptor->value.histogramNormType = static_cast<cv::HOGDescriptor::HistogramNormType>(static_cast<int>(value)); break;
        case 12: descriptor->value.L2HysThreshold = value; break;
        case 13: descriptor->value.gammaCorrection = value != 0.0; break;
        case 14: descriptor->value.nlevels = static_cast<int>(value); break;
        case 15: descriptor->value.signedGradient = value != 0.0; break;
        default: return opencv_csharp_native::set_invalid_argument(api_name, "property_id");
        }

        return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_hog_descriptor_detect_count(
    const jyppx_ocv_hog_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    double hit_threshold,
    int win_stride_width,
    int win_stride_height,
    int padding_width,
    int padding_height,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_detect_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_hog(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        std::vector<cv::Point> locations;
        std::vector<double> weights;
        descriptor->value.detect(opencv_csharp_native::mat_value(image), locations, weights, hit_threshold, to_size(win_stride_width, win_stride_height), to_size(padding_width, padding_height));
        return set_count_from_size(api_name, locations.size(), count);
#else
        (void)hit_threshold;
        (void)win_stride_width;
        (void)win_stride_height;
        (void)padding_width;
        (void)padding_height;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_detect_fill(
    const jyppx_ocv_hog_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    double hit_threshold,
    int win_stride_width,
    int win_stride_height,
    int padding_width,
    int padding_height,
    int* locations_xy,
    int location_capacity,
    double* weights,
    int weight_capacity,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_detect_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_hog(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        std::vector<cv::Point> locations;
        std::vector<double> values;
        descriptor->value.detect(opencv_csharp_native::mat_value(image), locations, values, hit_threshold, to_size(win_stride_width, win_stride_height), to_size(padding_width, padding_height));
        status = copy_points(api_name, locations, locations_xy, location_capacity, count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_doubles(api_name, values, weights, weight_capacity, "weights");
#else
        (void)hit_threshold;
        (void)win_stride_width;
        (void)win_stride_height;
        (void)padding_width;
        (void)padding_height;
        (void)locations_xy;
        (void)location_capacity;
        (void)weights;
        (void)weight_capacity;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_detect_multi_scale_count(
    const jyppx_ocv_hog_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    double hit_threshold,
    int win_stride_width,
    int win_stride_height,
    int padding_width,
    int padding_height,
    double scale,
    double group_threshold,
    int use_meanshift_grouping,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_detect_multi_scale_count";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_hog(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        std::vector<cv::Rect> locations;
        std::vector<double> weights;
        descriptor->value.detectMultiScale(opencv_csharp_native::mat_value(image), locations, weights, hit_threshold, to_size(win_stride_width, win_stride_height), to_size(padding_width, padding_height), scale, group_threshold, use_meanshift_grouping != 0);
        return set_count_from_size(api_name, locations.size(), count);
#else
        (void)hit_threshold;
        (void)win_stride_width;
        (void)win_stride_height;
        (void)padding_width;
        (void)padding_height;
        (void)scale;
        (void)group_threshold;
        (void)use_meanshift_grouping;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_hog_descriptor_detect_multi_scale_fill(
    const jyppx_ocv_hog_descriptor* descriptor,
    const jyppx_ocv_mat* image,
    double hit_threshold,
    int win_stride_width,
    int win_stride_height,
    int padding_width,
    int padding_height,
    double scale,
    double group_threshold,
    int use_meanshift_grouping,
    int* rectangles,
    int rectangle_capacity,
    double* weights,
    int weight_capacity,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_hog_descriptor_detect_multi_scale_fill";

    try
    {
        opencv_csharp_native::clear_last_error();

        int status = validate_hog(api_name, descriptor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT)
        std::vector<cv::Rect> locations;
        std::vector<double> values;
        descriptor->value.detectMultiScale(opencv_csharp_native::mat_value(image), locations, values, hit_threshold, to_size(win_stride_width, win_stride_height), to_size(padding_width, padding_height), scale, group_threshold, use_meanshift_grouping != 0);
        status = copy_rectangles(api_name, locations, rectangles, rectangle_capacity, count);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return copy_doubles(api_name, values, weights, weight_capacity, "weights");
#else
        (void)hit_threshold;
        (void)win_stride_width;
        (void)win_stride_height;
        (void)padding_width;
        (void)padding_height;
        (void)scale;
        (void)group_threshold;
        (void)use_meanshift_grouping;
        (void)rectangles;
        (void)rectangle_capacity;
        (void)weights;
        (void)weight_capacity;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

