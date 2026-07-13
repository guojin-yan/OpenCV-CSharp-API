#include "open_cv_sharp/face/face.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "face_handles.h"

#include <cstring>
#include <new>
#include <string>
#include <vector>

namespace
{
    int validate_recognizer(const char* api_name, const jyppx_ocv_face_recognizer* recognizer)
    {
        return recognizer == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "recognizer")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_basic(const char* api_name, const jyppx_ocv_face_basic_recognizer* recognizer)
    {
        return recognizer == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "recognizer")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_lbph(const char* api_name, const jyppx_ocv_face_lbph_recognizer* recognizer)
    {
        return recognizer == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "recognizer")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_collector(const char* api_name, const jyppx_ocv_face_standard_collector* collector)
    {
        return collector == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "collector")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_bif(const char* api_name, const jyppx_ocv_face_bif* bif)
    {
        return bif == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "bif")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_facemark(const char* api_name, const jyppx_ocv_face_facemark* facemark)
    {
        return facemark == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "facemark")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_facemark_train(const char* api_name, const jyppx_ocv_face_facemark_train* facemark)
    {
        return facemark == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "facemark")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_lbf(const char* api_name, const jyppx_ocv_face_facemark_lbf* facemark)
    {
        return facemark == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "facemark")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_mace(const char* api_name, const jyppx_ocv_face_mace* mace)
    {
        return mace == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "mace")
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

    int validate_mat_array(const char* api_name, const jyppx_ocv_mat* const* values, int value_count, const char* argument_name)
    {
        if (value_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (value_count > 0 && values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        for (int i = 0; i < value_count; ++i)
        {
            if (values[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_int_array(const char* api_name, const int* values, int value_count, const char* argument_name)
    {
        if (value_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (value_count > 0 && values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_double_array(const char* api_name, const double* values, int value_count, const char* argument_name)
    {
        if (value_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (value_count > 0 && values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_float_array(const char* api_name, const float* values, int value_count, const char* argument_name)
    {
        if (value_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        if (value_count > 0 && values == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
    std::string safe_string(const char* value)
    {
        return value == nullptr ? std::string() : std::string(value);
    }

    std::vector<cv::Mat> to_mat_vector(const jyppx_ocv_mat* const* values, int value_count)
    {
        std::vector<cv::Mat> result;
        result.reserve(static_cast<size_t>(value_count));
        for (int i = 0; i < value_count; ++i)
        {
            result.push_back(opencv_csharp_native::mat_value(values[i]));
        }

        return result;
    }

    cv::Mat to_label_mat(const int* labels, int label_count)
    {
        return cv::Mat(label_count, 1, CV_32SC1, const_cast<int*>(labels)).clone();
    }

    std::vector<int> to_int_vector(const int* values, int value_count)
    {
        return value_count <= 0
            ? std::vector<int>()
            : std::vector<int>(values, values + value_count);
    }

    std::vector<double> to_double_vector(const double* values, int value_count)
    {
        return value_count <= 0
            ? std::vector<double>()
            : std::vector<double>(values, values + value_count);
    }

    std::vector<cv::Rect> to_rect_vector(const int* faces, int face_count)
    {
        std::vector<cv::Rect> result;
        result.reserve(static_cast<size_t>(face_count));
        for (int i = 0; i < face_count; ++i)
        {
            const int offset = i * 4;
            result.emplace_back(faces[offset], faces[offset + 1], faces[offset + 2], faces[offset + 3]);
        }

        return result;
    }

    std::vector<cv::Point2f> to_point2f_vector(const float* landmarks, int landmark_count)
    {
        std::vector<cv::Point2f> result;
        result.reserve(static_cast<size_t>(landmark_count));
        for (int i = 0; i < landmark_count; ++i)
        {
            const int offset = i * 2;
            result.emplace_back(landmarks[offset], landmarks[offset + 1]);
        }

        return result;
    }

    int create_mat_handle(const char* api_name, const cv::Mat& value, jyppx_ocv_mat** out_mat)
    {
        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        *out_mat = nullptr;
        jyppx_ocv_mat* created = new (std::nothrow) jyppx_ocv_mat();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = value.clone();
        *out_mat = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    template <typename THandle, typename TConcrete>
    int create_basic_handle(const char* api_name, const cv::Ptr<TConcrete>& native, THandle** recognizer)
    {
        if (recognizer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "recognizer");
        }

        *recognizer = nullptr;
        THandle* created = new (std::nothrow) THandle();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = native;
        created->basic = native;
        created->value = native;
        *recognizer = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int copy_mats(
        const char* api_name,
        const std::vector<cv::Mat>& mats,
        jyppx_ocv_mat** output,
        int output_capacity,
        int* count)
    {
        if (output_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output_capacity");
        }

        if (output_capacity > 0 && output == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "output");
        }

        int status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        const int actual_count = static_cast<int>(mats.size());
        const int copy_count = actual_count < output_capacity ? actual_count : output_capacity;
        for (int i = 0; i < copy_count; ++i)
        {
            status = create_mat_handle(api_name, mats[static_cast<size_t>(i)], &output[i]);
            if (status != OPENCV_CSHARP_STATUS_OK)
            {
                return status;
            }
        }

        *count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int count_landmarks(
        const std::vector<std::vector<cv::Point2f>>& landmarks,
        int* face_count,
        int* point_count)
    {
        int total_points = 0;
        for (size_t i = 0; i < landmarks.size(); ++i)
        {
            total_points += static_cast<int>(landmarks[i].size());
        }

        *face_count = static_cast<int>(landmarks.size());
        *point_count = total_points;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int fill_landmarks(
        const char* api_name,
        const std::vector<std::vector<cv::Point2f>>& landmarks,
        int* landmark_offsets,
        int landmark_offset_capacity,
        float* landmarks_buffer,
        int landmark_point_capacity,
        int* face_count,
        int* point_count)
    {
        if (landmark_offset_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "landmark_offset_capacity");
        }

        if (landmark_point_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "landmark_point_capacity");
        }

        int status = validate_output_int(api_name, face_count, "face_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, point_count, "point_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        int actual_faces = 0;
        int actual_points = 0;
        count_landmarks(landmarks, &actual_faces, &actual_points);
        *face_count = actual_faces;
        *point_count = actual_points;

        if (actual_faces > 0 && (landmark_offsets == nullptr || landmark_offset_capacity < actual_faces + 1))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "landmark_offsets");
        }

        if (actual_points > 0 && (landmarks_buffer == nullptr || landmark_point_capacity < actual_points))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "landmarks_buffer");
        }

        int point_offset = 0;
        if (actual_faces >= 0 && landmark_offsets != nullptr && landmark_offset_capacity > 0)
        {
            landmark_offsets[0] = 0;
        }

        for (int i = 0; i < actual_faces; ++i)
        {
            const std::vector<cv::Point2f>& group = landmarks[static_cast<size_t>(i)];
            for (size_t j = 0; j < group.size(); ++j)
            {
                const int buffer_offset = point_offset * 2;
                landmarks_buffer[buffer_offset] = group[j].x;
                landmarks_buffer[buffer_offset + 1] = group[j].y;
                ++point_offset;
            }

            landmark_offsets[i + 1] = point_offset;
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_lbf_handle(
        const char* api_name,
        const cv::face::FacemarkLBF::Params& parameters,
        jyppx_ocv_face_facemark_lbf** facemark)
    {
        if (facemark == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "facemark");
        }

        *facemark = nullptr;
        jyppx_ocv_face_facemark_lbf* created = new (std::nothrow) jyppx_ocv_face_facemark_lbf();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->params = parameters;
        created->concrete = cv::face::FacemarkLBF::create(parameters);
        created->train_value = created->concrete;
        created->value = created->concrete;
        *facemark = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int get_utf8_length(const cv::String& value, int* length)
    {
        *length = static_cast<int>(value.size());
        return OPENCV_CSHARP_STATUS_OK;
    }

    int fill_utf8_string(const char* api_name, const cv::String& value, char* buffer, int buffer_capacity, int* written)
    {
        if (buffer_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer_capacity");
        }

        if (buffer_capacity > 0 && buffer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
        }

        int status = validate_output_int(api_name, written, "written");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        int actual = static_cast<int>(value.size());
        int copy = actual < buffer_capacity ? actual : buffer_capacity;
        if (copy > 0)
        {
            std::memcpy(buffer, value.data(), static_cast<size_t>(copy));
        }

        *written = actual;
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif

    int get_basic_mat(
        const char* api_name,
        const jyppx_ocv_face_basic_recognizer* recognizer,
        jyppx_ocv_mat** out_mat,
        int mat_property)
    {
        int status = validate_basic(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        *out_mat = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        switch (mat_property)
        {
        case 0:
            return create_mat_handle(api_name, recognizer->basic->getLabels(), out_mat);
        case 1:
            return create_mat_handle(api_name, recognizer->basic->getEigenValues(), out_mat);
        case 2:
            return create_mat_handle(api_name, recognizer->basic->getEigenVectors(), out_mat);
        case 3:
            return create_mat_handle(api_name, recognizer->basic->getMean(), out_mat);
        default:
            return opencv_csharp_native::set_invalid_argument(api_name, "mat_property");
        }
#else
        (void)mat_property;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }

    int get_lbph_int(
        const char* api_name,
        const jyppx_ocv_face_lbph_recognizer* recognizer,
        int* value,
        int int_property)
    {
        int status = validate_lbph(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        switch (int_property)
        {
        case 0:
            *value = recognizer->concrete->getRadius();
            return OPENCV_CSHARP_STATUS_OK;
        case 1:
            *value = recognizer->concrete->getNeighbors();
            return OPENCV_CSHARP_STATUS_OK;
        case 2:
            *value = recognizer->concrete->getGridX();
            return OPENCV_CSHARP_STATUS_OK;
        case 3:
            *value = recognizer->concrete->getGridY();
            return OPENCV_CSHARP_STATUS_OK;
        default:
            return opencv_csharp_native::set_invalid_argument(api_name, "int_property");
        }
#else
        *value = 0;
        (void)int_property;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }

    int set_lbph_int(
        const char* api_name,
        jyppx_ocv_face_lbph_recognizer* recognizer,
        int value,
        int int_property)
    {
        int status = validate_lbph(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        switch (int_property)
        {
        case 0:
            recognizer->concrete->setRadius(value);
            return OPENCV_CSHARP_STATUS_OK;
        case 1:
            recognizer->concrete->setNeighbors(value);
            return OPENCV_CSHARP_STATUS_OK;
        case 2:
            recognizer->concrete->setGridX(value);
            return OPENCV_CSHARP_STATUS_OK;
        case 3:
            recognizer->concrete->setGridY(value);
            return OPENCV_CSHARP_STATUS_OK;
        default:
            return opencv_csharp_native::set_invalid_argument(api_name, "int_property");
        }
#else
        (void)value;
        (void)int_property;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
}

void jyppx_ocv_face_recognizer_release_handle(jyppx_ocv_face_recognizer* recognizer)
{
    delete recognizer;
}

void jyppx_ocv_face_standard_collector_release_handle(jyppx_ocv_face_standard_collector* collector)
{
    delete collector;
}

void jyppx_ocv_face_bif_release_handle(jyppx_ocv_face_bif* bif)
{
    delete bif;
}

int jyppx_ocv_face_eigen_create(int num_components, double threshold, jyppx_ocv_face_eigen_recognizer** recognizer)
{
    constexpr const char* api_name = "jyppx_ocv_face_eigen_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        return create_basic_handle(api_name, cv::face::EigenFaceRecognizer::create(num_components, threshold), recognizer);
#else
        (void)num_components; (void)threshold; (void)recognizer;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_fisher_create(int num_components, double threshold, jyppx_ocv_face_fisher_recognizer** recognizer)
{
    constexpr const char* api_name = "jyppx_ocv_face_fisher_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        return create_basic_handle(api_name, cv::face::FisherFaceRecognizer::create(num_components, threshold), recognizer);
#else
        (void)num_components; (void)threshold; (void)recognizer;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_create(int radius, int neighbors, int grid_x, int grid_y, double threshold, jyppx_ocv_face_lbph_recognizer** recognizer)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (recognizer == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "recognizer");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *recognizer = nullptr;
        jyppx_ocv_face_lbph_recognizer* created = new (std::nothrow) jyppx_ocv_face_lbph_recognizer();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::face::LBPHFaceRecognizer::create(radius, neighbors, grid_x, grid_y, threshold);
        created->value = created->concrete;
        *recognizer = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)radius; (void)neighbors; (void)grid_x; (void)grid_y; (void)threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_train(jyppx_ocv_face_recognizer* recognizer, const jyppx_ocv_mat* const* images, int image_count, const int* labels, int label_count)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_train";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_int_array(api_name, labels, label_count, "labels");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_count != label_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "labels");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        recognizer->value->train(to_mat_vector(images, image_count), to_label_mat(labels, label_count));
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

int jyppx_ocv_face_recognizer_update(jyppx_ocv_face_recognizer* recognizer, const jyppx_ocv_mat* const* images, int image_count, const int* labels, int label_count)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_update";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_int_array(api_name, labels, label_count, "labels");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (image_count != label_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "labels");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        recognizer->value->update(to_mat_vector(images, image_count), to_label_mat(labels, label_count));
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

int jyppx_ocv_face_recognizer_predict_label(const jyppx_ocv_face_recognizer* recognizer, const jyppx_ocv_mat* image, int* label)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_predict_label";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, label, "label");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *label = recognizer->value->predict(opencv_csharp_native::mat_value(image));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *label = -1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_predict(const jyppx_ocv_face_recognizer* recognizer, const jyppx_ocv_mat* image, int* label, double* confidence)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_predict";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, label, "label");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, confidence, "confidence");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        recognizer->value->predict(opencv_csharp_native::mat_value(image), *label, *confidence);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *label = -1;
        *confidence = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_predict_collect(const jyppx_ocv_face_recognizer* recognizer, const jyppx_ocv_mat* image, jyppx_ocv_face_standard_collector* collector)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_predict_collect";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_collector(api_name, collector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        recognizer->value->predict(opencv_csharp_native::mat_value(image), collector->value);
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

int jyppx_ocv_face_recognizer_read(jyppx_ocv_face_recognizer* recognizer, const char* path)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_read";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (path == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "path");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        recognizer->value->read(safe_string(path));
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

int jyppx_ocv_face_recognizer_write(const jyppx_ocv_face_recognizer* recognizer, const char* path)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_write";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (path == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "path");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        recognizer->value->write(safe_string(path));
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

int jyppx_ocv_face_recognizer_empty(const jyppx_ocv_face_recognizer* recognizer, int* empty)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, empty, "empty");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *empty = recognizer->value->empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *empty = 1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_set_label_info(jyppx_ocv_face_recognizer* recognizer, int label, const char* info)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_set_label_info";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (info == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "info");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        recognizer->value->setLabelInfo(label, safe_string(info));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)label;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_get_label_info_length(const jyppx_ocv_face_recognizer* recognizer, int label, int* length)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_get_label_info_length";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, length, "length");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        return get_utf8_length(recognizer->value->getLabelInfo(label), length);
#else
        *length = 0;
        (void)label;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_get_label_info_fill(const jyppx_ocv_face_recognizer* recognizer, int label, char* buffer, int buffer_capacity, int* written)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_get_label_info_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        return fill_utf8_string(api_name, recognizer->value->getLabelInfo(label), buffer, buffer_capacity, written);
#else
        (void)label; (void)buffer; (void)buffer_capacity;
        if (written != nullptr) { *written = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_get_labels_by_string_count(const jyppx_ocv_face_recognizer* recognizer, const char* substring, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_get_labels_by_string_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (substring == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "substring");
        }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *count = static_cast<int>(recognizer->value->getLabelsByString(safe_string(substring)).size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_get_labels_by_string_fill(const jyppx_ocv_face_recognizer* recognizer, const char* substring, int* labels, int label_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_get_labels_by_string_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (substring == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "substring");
        }
        if (label_capacity < 0 || (label_capacity > 0 && labels == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "labels");
        }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        std::vector<int> native_labels = recognizer->value->getLabelsByString(safe_string(substring));
        int actual_count = static_cast<int>(native_labels.size());
        int copy_count = actual_count < label_capacity ? actual_count : label_capacity;
        for (int i = 0; i < copy_count; ++i)
        {
            labels[i] = native_labels[static_cast<size_t>(i)];
        }
        *count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_get_threshold(const jyppx_ocv_face_recognizer* recognizer, double* threshold)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_get_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, threshold, "threshold");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *threshold = recognizer->value->getThreshold();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *threshold = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_recognizer_set_threshold(jyppx_ocv_face_recognizer* recognizer, double threshold)
{
    constexpr const char* api_name = "jyppx_ocv_face_recognizer_set_threshold";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_recognizer(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        recognizer->value->setThreshold(threshold);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_basic_get_num_components(const jyppx_ocv_face_basic_recognizer* recognizer, int* num_components)
{
    constexpr const char* api_name = "jyppx_ocv_face_basic_get_num_components";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_basic(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, num_components, "num_components");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *num_components = recognizer->basic->getNumComponents();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *num_components = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_basic_set_num_components(jyppx_ocv_face_basic_recognizer* recognizer, int num_components)
{
    constexpr const char* api_name = "jyppx_ocv_face_basic_set_num_components";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_basic(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        recognizer->basic->setNumComponents(num_components);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)num_components;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_basic_get_labels(const jyppx_ocv_face_basic_recognizer* recognizer, jyppx_ocv_mat** labels)
{
    constexpr const char* api_name = "jyppx_ocv_face_basic_get_labels";
    try
    {
        opencv_csharp_native::clear_last_error();
        return get_basic_mat(api_name, recognizer, labels, 0);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_basic_get_eigen_values(const jyppx_ocv_face_basic_recognizer* recognizer, jyppx_ocv_mat** eigen_values)
{
    constexpr const char* api_name = "jyppx_ocv_face_basic_get_eigen_values";
    try
    {
        opencv_csharp_native::clear_last_error();
        return get_basic_mat(api_name, recognizer, eigen_values, 1);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_basic_get_eigen_vectors(const jyppx_ocv_face_basic_recognizer* recognizer, jyppx_ocv_mat** eigen_vectors)
{
    constexpr const char* api_name = "jyppx_ocv_face_basic_get_eigen_vectors";
    try
    {
        opencv_csharp_native::clear_last_error();
        return get_basic_mat(api_name, recognizer, eigen_vectors, 2);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_basic_get_mean(const jyppx_ocv_face_basic_recognizer* recognizer, jyppx_ocv_mat** mean)
{
    constexpr const char* api_name = "jyppx_ocv_face_basic_get_mean";
    try
    {
        opencv_csharp_native::clear_last_error();
        return get_basic_mat(api_name, recognizer, mean, 3);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_basic_get_projections_count(const jyppx_ocv_face_basic_recognizer* recognizer, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_face_basic_get_projections_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_basic(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *count = static_cast<int>(recognizer->basic->getProjections().size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_basic_get_projections_fill(const jyppx_ocv_face_basic_recognizer* recognizer, jyppx_ocv_mat** projections, int projection_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_face_basic_get_projections_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_basic(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        return copy_mats(api_name, recognizer->basic->getProjections(), projections, projection_capacity, count);
#else
        (void)projections; (void)projection_capacity;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_get_radius(const jyppx_ocv_face_lbph_recognizer* recognizer, int* radius)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_get_radius";
    try
    {
        opencv_csharp_native::clear_last_error();
        return get_lbph_int(api_name, recognizer, radius, 0);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_set_radius(jyppx_ocv_face_lbph_recognizer* recognizer, int radius)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_set_radius";
    try
    {
        opencv_csharp_native::clear_last_error();
        return set_lbph_int(api_name, recognizer, radius, 0);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_get_neighbors(const jyppx_ocv_face_lbph_recognizer* recognizer, int* neighbors)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_get_neighbors";
    try
    {
        opencv_csharp_native::clear_last_error();
        return get_lbph_int(api_name, recognizer, neighbors, 1);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_set_neighbors(jyppx_ocv_face_lbph_recognizer* recognizer, int neighbors)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_set_neighbors";
    try
    {
        opencv_csharp_native::clear_last_error();
        return set_lbph_int(api_name, recognizer, neighbors, 1);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_get_grid_x(const jyppx_ocv_face_lbph_recognizer* recognizer, int* grid_x)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_get_grid_x";
    try
    {
        opencv_csharp_native::clear_last_error();
        return get_lbph_int(api_name, recognizer, grid_x, 2);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_set_grid_x(jyppx_ocv_face_lbph_recognizer* recognizer, int grid_x)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_set_grid_x";
    try
    {
        opencv_csharp_native::clear_last_error();
        return set_lbph_int(api_name, recognizer, grid_x, 2);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_get_grid_y(const jyppx_ocv_face_lbph_recognizer* recognizer, int* grid_y)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_get_grid_y";
    try
    {
        opencv_csharp_native::clear_last_error();
        return get_lbph_int(api_name, recognizer, grid_y, 3);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_set_grid_y(jyppx_ocv_face_lbph_recognizer* recognizer, int grid_y)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_set_grid_y";
    try
    {
        opencv_csharp_native::clear_last_error();
        return set_lbph_int(api_name, recognizer, grid_y, 3);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_get_labels(const jyppx_ocv_face_lbph_recognizer* recognizer, jyppx_ocv_mat** labels)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_get_labels";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_lbph(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (labels == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "labels");
        }
        *labels = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        return create_mat_handle(api_name, recognizer->concrete->getLabels(), labels);
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_get_histograms_count(const jyppx_ocv_face_lbph_recognizer* recognizer, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_get_histograms_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_lbph(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *count = static_cast<int>(recognizer->concrete->getHistograms().size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_lbph_get_histograms_fill(const jyppx_ocv_face_lbph_recognizer* recognizer, jyppx_ocv_mat** histograms, int histogram_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_face_lbph_get_histograms_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_lbph(api_name, recognizer);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        return copy_mats(api_name, recognizer->concrete->getHistograms(), histograms, histogram_capacity, count);
#else
        (void)histograms; (void)histogram_capacity;
        if (count != nullptr) { *count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_standard_collector_create(double threshold, jyppx_ocv_face_standard_collector** collector)
{
    constexpr const char* api_name = "jyppx_ocv_face_standard_collector_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (collector == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "collector");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *collector = nullptr;
        jyppx_ocv_face_standard_collector* created = new (std::nothrow) jyppx_ocv_face_standard_collector();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::face::StandardCollector::create(threshold);
        *collector = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_standard_collector_get_min_label(const jyppx_ocv_face_standard_collector* collector, int* label)
{
    constexpr const char* api_name = "jyppx_ocv_face_standard_collector_get_min_label";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_collector(api_name, collector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, label, "label");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *label = collector->value->getMinLabel();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *label = -1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_standard_collector_get_min_dist(const jyppx_ocv_face_standard_collector* collector, double* distance)
{
    constexpr const char* api_name = "jyppx_ocv_face_standard_collector_get_min_dist";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_collector(api_name, collector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, distance, "distance");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *distance = collector->value->getMinDist();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *distance = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_standard_collector_get_results_count(const jyppx_ocv_face_standard_collector* collector, int sorted, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_face_standard_collector_get_results_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_collector(api_name, collector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *count = static_cast<int>(collector->value->getResults(sorted != 0).size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_standard_collector_get_results_fill(const jyppx_ocv_face_standard_collector* collector, int sorted, jyppx_ocv_face_prediction_result* results, int result_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_face_standard_collector_get_results_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_collector(api_name, collector);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (result_capacity < 0 || (result_capacity > 0 && results == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "results");
        }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        std::vector<std::pair<int, double>> native_results = collector->value->getResults(sorted != 0);
        int actual_count = static_cast<int>(native_results.size());
        int copy_count = actual_count < result_capacity ? actual_count : result_capacity;
        for (int i = 0; i < copy_count; ++i)
        {
            results[i] = jyppx_ocv_face_prediction_result{
                native_results[static_cast<size_t>(i)].first,
                native_results[static_cast<size_t>(i)].second
            };
        }
        *count = actual_count;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_bif_create(int num_bands, int num_rotations, jyppx_ocv_face_bif** bif)
{
    constexpr const char* api_name = "jyppx_ocv_face_bif_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (bif == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "bif");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *bif = nullptr;
        jyppx_ocv_face_bif* created = new (std::nothrow) jyppx_ocv_face_bif();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::face::BIF::create(num_bands, num_rotations);
        *bif = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)num_bands; (void)num_rotations;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_bif_get_num_bands(const jyppx_ocv_face_bif* bif, int* num_bands)
{
    constexpr const char* api_name = "jyppx_ocv_face_bif_get_num_bands";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_bif(api_name, bif);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, num_bands, "num_bands");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *num_bands = bif->value->getNumBands();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *num_bands = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_bif_get_num_rotations(const jyppx_ocv_face_bif* bif, int* num_rotations)
{
    constexpr const char* api_name = "jyppx_ocv_face_bif_get_num_rotations";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_bif(api_name, bif);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, num_rotations, "num_rotations");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *num_rotations = bif->value->getNumRotations();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *num_rotations = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_bif_compute(const jyppx_ocv_face_bif* bif, const jyppx_ocv_mat* image, jyppx_ocv_mat* features)
{
    constexpr const char* api_name = "jyppx_ocv_face_bif_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_bif(api_name, bif);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, features, "features");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        bif->value->compute(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(features));
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

void jyppx_ocv_face_facemark_release_handle(jyppx_ocv_face_facemark* facemark)
{
    delete facemark;
}

void jyppx_ocv_face_mace_release_handle(jyppx_ocv_face_mace* mace)
{
    delete mace;
}

int jyppx_ocv_face_facemark_load_model(jyppx_ocv_face_facemark* facemark, const char* model_path)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_load_model";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_facemark(api_name, facemark);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (model_path == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model_path");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        facemark->value->loadModel(safe_string(model_path));
        facemark->last_landmarks.clear();
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

int jyppx_ocv_face_facemark_fit(
    jyppx_ocv_face_facemark* facemark,
    const jyppx_ocv_mat* image,
    const int* faces,
    int face_count,
    int* result)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_fit";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_facemark(api_name, facemark);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_int_array(api_name, faces, face_count * 4, "faces");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        std::vector<cv::Rect> native_faces = to_rect_vector(faces, face_count);
        facemark->last_landmarks.clear();
        *result = facemark->value->fit(
            opencv_csharp_native::mat_value(image),
            native_faces,
            facemark->last_landmarks) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_facemark_fit_landmarks_count(
    const jyppx_ocv_face_facemark* facemark,
    int* face_count,
    int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_fit_landmarks_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_facemark(api_name, facemark);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, face_count, "face_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, point_count, "point_count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        return count_landmarks(facemark->last_landmarks, face_count, point_count);
#else
        *face_count = 0;
        *point_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_facemark_fit_landmarks_fill(
    const jyppx_ocv_face_facemark* facemark,
    int* landmark_offsets,
    int landmark_offset_capacity,
    float* landmarks_buffer,
    int landmark_point_capacity,
    int* face_count,
    int* point_count)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_fit_landmarks_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_facemark(api_name, facemark);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        return fill_landmarks(
            api_name,
            facemark->last_landmarks,
            landmark_offsets,
            landmark_offset_capacity,
            landmarks_buffer,
            landmark_point_capacity,
            face_count,
            point_count);
#else
        (void)landmark_offsets; (void)landmark_offset_capacity; (void)landmarks_buffer; (void)landmark_point_capacity;
        if (face_count != nullptr) { *face_count = 0; }
        if (point_count != nullptr) { *point_count = 0; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_facemark_train_add_sample(
    jyppx_ocv_face_facemark_train* facemark,
    const jyppx_ocv_mat* image,
    const float* landmarks,
    int landmark_count)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_train_add_sample";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_facemark_train(api_name, facemark);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_float_array(api_name, landmarks, landmark_count * 2, "landmarks");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        std::vector<cv::Point2f> points = to_point2f_vector(landmarks, landmark_count);
        int added = facemark->train_value->addTrainingSample(opencv_csharp_native::mat_value(image), points) ? 1 : 0;
        return added != 0 ? OPENCV_CSHARP_STATUS_OK : opencv_csharp_native::set_invalid_argument(api_name, "landmarks");
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_facemark_train_training(jyppx_ocv_face_facemark_train* facemark)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_train_training";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_facemark_train(api_name, facemark);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        facemark->train_value->training();
        facemark->last_landmarks.clear();
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

int jyppx_ocv_face_facemark_train_get_faces_count(
    jyppx_ocv_face_facemark_train* facemark,
    const jyppx_ocv_mat* image,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_train_get_faces_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_facemark_train(api_name, facemark);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        facemark->last_faces.clear();
        int ok = facemark->train_value->getFaces(opencv_csharp_native::mat_value(image), facemark->last_faces) ? 1 : 0;
        *count = ok != 0 ? static_cast<int>(facemark->last_faces.size()) : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_facemark_train_get_faces_fill(
    jyppx_ocv_face_facemark_train* facemark,
    const jyppx_ocv_mat* image,
    int* faces_buffer,
    int face_capacity,
    int* count)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_train_get_faces_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_facemark_train(api_name, facemark);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (face_capacity < 0 || (face_capacity > 0 && faces_buffer == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "faces_buffer");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        if (facemark->last_faces.empty())
        {
            facemark->train_value->getFaces(opencv_csharp_native::mat_value(image), facemark->last_faces);
        }

        int actual = static_cast<int>(facemark->last_faces.size());
        int copy = actual < face_capacity ? actual : face_capacity;
        for (int i = 0; i < copy; ++i)
        {
            const cv::Rect& rect = facemark->last_faces[static_cast<size_t>(i)];
            const int offset = i * 4;
            faces_buffer[offset] = rect.x;
            faces_buffer[offset + 1] = rect.y;
            faces_buffer[offset + 2] = rect.width;
            faces_buffer[offset + 3] = rect.height;
        }

        *count = actual;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_facemark_save(const jyppx_ocv_face_facemark* facemark, const char* path)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_save";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_facemark(api_name, facemark);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (path == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "path");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        facemark->value->save(safe_string(path));
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

int jyppx_ocv_face_facemark_lbf_create(
    int n_landmarks,
    int init_shape_n,
    int stages_n,
    int tree_n,
    int tree_depth,
    double shape_offset,
    double bagging_overlap,
    int verbose,
    jyppx_ocv_face_facemark_lbf** facemark)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_lbf_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (facemark == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "facemark");
        }
        *facemark = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        cv::face::FacemarkLBF::Params parameters;
        parameters.n_landmarks = n_landmarks;
        parameters.initShape_n = init_shape_n;
        parameters.stages_n = stages_n;
        parameters.tree_n = tree_n;
        parameters.tree_depth = tree_depth;
        parameters.shape_offset = shape_offset;
        parameters.bagging_overlap = bagging_overlap;
        parameters.verbose = verbose != 0;
        parameters.save_model = false;
        return create_lbf_handle(api_name, parameters, facemark);
#else
        (void)n_landmarks; (void)init_shape_n; (void)stages_n; (void)tree_n; (void)tree_depth; (void)shape_offset; (void)bagging_overlap; (void)verbose;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_facemark_lbf_create_ex(
    int n_landmarks,
    int init_shape_n,
    int stages_n,
    int tree_n,
    int tree_depth,
    double shape_offset,
    double bagging_overlap,
    int verbose,
    int save_model,
    unsigned int seed,
    const char* cascade_face,
    const char* model_filename,
    const int* feats_m,
    int feats_count,
    const double* radius_m,
    int radius_count,
    const int* left_pupil,
    int left_pupil_count,
    const int* right_pupil,
    int right_pupil_count,
    int detect_roi_x,
    int detect_roi_y,
    int detect_roi_width,
    int detect_roi_height,
    jyppx_ocv_face_facemark_lbf** facemark)
{
    constexpr const char* api_name = "jyppx_ocv_face_facemark_lbf_create_ex";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (facemark == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "facemark");
        }
        *facemark = nullptr;
        int status = validate_int_array(api_name, feats_m, feats_count, "feats_m");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_double_array(api_name, radius_m, radius_count, "radius_m");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_int_array(api_name, left_pupil, left_pupil_count, "left_pupil");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_int_array(api_name, right_pupil, right_pupil_count, "right_pupil");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        cv::face::FacemarkLBF::Params parameters;
        parameters.n_landmarks = n_landmarks;
        parameters.initShape_n = init_shape_n;
        parameters.stages_n = stages_n;
        parameters.tree_n = tree_n;
        parameters.tree_depth = tree_depth;
        parameters.shape_offset = shape_offset;
        parameters.bagging_overlap = bagging_overlap;
        parameters.verbose = verbose != 0;
        parameters.save_model = save_model != 0;
        parameters.seed = seed;
        parameters.cascade_face = safe_string(cascade_face);
        parameters.model_filename = safe_string(model_filename);
        if (feats_count > 0) { parameters.feats_m = to_int_vector(feats_m, feats_count); }
        if (radius_count > 0) { parameters.radius_m = to_double_vector(radius_m, radius_count); }
        if (left_pupil_count > 0) { parameters.pupils[0] = to_int_vector(left_pupil, left_pupil_count); }
        if (right_pupil_count > 0) { parameters.pupils[1] = to_int_vector(right_pupil, right_pupil_count); }
        parameters.detectROI = cv::Rect(detect_roi_x, detect_roi_y, detect_roi_width, detect_roi_height);
        return create_lbf_handle(api_name, parameters, facemark);
#else
        (void)n_landmarks; (void)init_shape_n; (void)stages_n; (void)tree_n; (void)tree_depth;
        (void)shape_offset; (void)bagging_overlap; (void)verbose; (void)save_model; (void)seed;
        (void)cascade_face; (void)model_filename; (void)detect_roi_x; (void)detect_roi_y; (void)detect_roi_width; (void)detect_roi_height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_mace_create(int imgsize, jyppx_ocv_face_mace** mace)
{
    constexpr const char* api_name = "jyppx_ocv_face_mace_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (mace == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mace");
        }
        *mace = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        jyppx_ocv_face_mace* created = new (std::nothrow) jyppx_ocv_face_mace();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = cv::face::MACE::create(imgsize);
        *mace = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)imgsize;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_mace_load(const char* filename, const char* objname, jyppx_ocv_face_mace** mace)
{
    constexpr const char* api_name = "jyppx_ocv_face_mace_load";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (filename == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }
        if (mace == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mace");
        }
        *mace = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        cv::Ptr<cv::face::MACE> native = cv::face::MACE::load(safe_string(filename), safe_string(objname));
        if (native.empty())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "filename");
        }

        jyppx_ocv_face_mace* created = new (std::nothrow) jyppx_ocv_face_mace();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *mace = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)objname;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_mace_salt(jyppx_ocv_face_mace* mace, const char* passphrase)
{
    constexpr const char* api_name = "jyppx_ocv_face_mace_salt";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mace(api_name, mace);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (passphrase == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "passphrase");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        mace->value->salt(safe_string(passphrase));
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

int jyppx_ocv_face_mace_train(jyppx_ocv_face_mace* mace, const jyppx_ocv_mat* const* images, int image_count)
{
    constexpr const char* api_name = "jyppx_ocv_face_mace_train";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mace(api_name, mace);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat_array(api_name, images, image_count, "images");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        mace->value->train(to_mat_vector(images, image_count));
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

int jyppx_ocv_face_mace_same(const jyppx_ocv_face_mace* mace, const jyppx_ocv_mat* query, int* same)
{
    constexpr const char* api_name = "jyppx_ocv_face_mace_same";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mace(api_name, mace);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, query, "query");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, same, "same");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *same = mace->value->same(opencv_csharp_native::mat_value(query)) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *same = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_face_mace_save(const jyppx_ocv_face_mace* mace, const char* path)
{
    constexpr const char* api_name = "jyppx_ocv_face_mace_save";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mace(api_name, mace);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (path == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "path");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        mace->value->save(safe_string(path));
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

int jyppx_ocv_face_mace_empty(const jyppx_ocv_face_mace* mace, int* empty)
{
    constexpr const char* api_name = "jyppx_ocv_face_mace_empty";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mace(api_name, mace);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, empty, "empty");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FACE)
        *empty = mace->value->empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *empty = 1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

