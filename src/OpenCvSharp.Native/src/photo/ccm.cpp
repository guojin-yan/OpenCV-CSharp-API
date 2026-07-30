#include "open_cv_sharp/photo/photo.h"

#include "../core/mat_handle.h"
#include "../core/persistence_handle_access.h"
#include "../error_state.h"
#include "photo_handles.h"

#include <cmath>
#include <memory>
#include <vector>

namespace
{
    template <typename TAction>
    int guarded(const char* api_name, TAction action) noexcept
    {
        try
        {
            opencv_csharp_native::clear_last_error();
            return action();
        }
        catch (...)
        {
            return opencv_csharp_native::translate_current_exception(api_name);
        }
    }

    template <typename T>
    int validate_pointer(const char* api_name, const T* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_model(const char* api_name, const jyppx_ocv_color_correction_model* model)
    {
        if (model == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!model->value)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }
#endif
        return OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    int validate_color_samples(
        const char* api_name,
        const cv::Mat& value,
        const char* argument_name,
        int expected_count = -1)
    {
        if (value.empty() || value.dims > 2 || value.type() != CV_64FC3 || value.cols != 1 ||
            (expected_count >= 0 && value.rows != expected_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_color_mask(
        const char* api_name,
        const cv::Mat& value,
        int expected_count)
    {
        if (value.empty() || value.dims > 2 || value.type() != CV_8UC1 ||
            value.cols != 1 || value.rows != expected_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "colored_patches_mask");
        }
        return OPENCV_CSHARP_STATUS_OK;
    }

    int color_checker_count(int color_checker)
    {
        switch (color_checker)
        {
        case cv::ccm::COLORCHECKER_MACBETH:
        case cv::ccm::COLORCHECKER_VINYL:
            return 24;
        case cv::ccm::COLORCHECKER_DIGITAL_SG:
            return 140;
        default:
            return -1;
        }
    }

    bool valid_reference_color_space(int value)
    {
        return value >= cv::ccm::COLOR_SPACE_SRGB && value <= cv::ccm::COLOR_SPACE_LAB_E_10;
    }

    bool valid_model_color_space(int value)
    {
        return value >= cv::ccm::COLOR_SPACE_SRGB &&
            value <= cv::ccm::COLOR_SPACE_REC_2020_RGB && (value % 2) == 0;
    }

    int validate_ready(const char* api_name, const jyppx_ocv_color_correction_model* model)
    {
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }
        return model->ready
            ? OPENCV_CSHARP_STATUS_OK
            : opencv_csharp_native::set_invalid_argument(api_name, "model_state");
    }

    bool numeric_node(const cv::FileNode& node)
    {
        return node.isInt() || node.isReal();
    }

    int validate_persisted_model_node(const char* api_name, const cv::FileNode& node)
    {
        if (!node.isMap())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "node");
        }

        const cv::FileNode ccm_node = node["ccm"];
        const cv::FileNode loss_node = node["loss"];
        const cv::FileNode color_space_node = node["csEnum"];
        const cv::FileNode ccm_type_node = node["ccm_type"];
        const cv::FileNode shape_node = node["shape"];
        const cv::FileNode linear_node = node["linear"];
        const cv::FileNode distance_node = node["distance"];
        const cv::FileNode linear_type_node = node["linear_type"];
        const cv::FileNode gamma_node = node["gamma"];
        const cv::FileNode degree_node = node["deg"];
        const cv::FileNode saturated_node = node["saturated_threshold"];

        if (ccm_node.empty() || !numeric_node(loss_node) || !numeric_node(color_space_node) ||
            !numeric_node(ccm_type_node) || !numeric_node(shape_node) || !linear_node.isMap() ||
            !numeric_node(distance_node) || !numeric_node(linear_type_node) ||
            !numeric_node(gamma_node) || !numeric_node(degree_node) || !saturated_node.isSeq())
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "node");
        }

        cv::Mat ccm;
        ccm_node >> ccm;
        const int color_space = static_cast<int>(color_space_node);
        const int ccm_type = static_cast<int>(ccm_type_node);
        const int shape = static_cast<int>(shape_node);
        const int distance = static_cast<int>(distance_node);
        const int linear_type = static_cast<int>(linear_type_node);
        const double loss = static_cast<double>(loss_node);
        const double gamma = static_cast<double>(gamma_node);
        const int degree = static_cast<int>(degree_node);
        std::vector<double> saturated_threshold;
        saturated_node >> saturated_threshold;

        const int expected_rows = ccm_type == cv::ccm::CCM_LINEAR ? 3 : 4;
        const int expected_shape = ccm_type == cv::ccm::CCM_LINEAR ? 9 : 12;
        if (ccm_type < cv::ccm::CCM_LINEAR || ccm_type > cv::ccm::CCM_AFFINE ||
            ccm.empty() || ccm.dims > 2 || ccm.type() != CV_64FC1 ||
            ccm.rows != expected_rows || ccm.cols != 3 || shape != expected_shape ||
            !valid_model_color_space(color_space) ||
            distance < cv::ccm::DISTANCE_CIE76 || distance > cv::ccm::DISTANCE_RGBL ||
            linear_type < cv::ccm::LINEARIZATION_IDENTITY ||
            linear_type > cv::ccm::LINEARIZATION_GRAYLOGPOLYFIT ||
            !std::isfinite(loss) || !std::isfinite(gamma) || gamma <= 0.0 || degree <= 0 ||
            saturated_threshold.size() != 2 || !std::isfinite(saturated_threshold[0]) ||
            !std::isfinite(saturated_threshold[1]) || saturated_threshold[0] < 0.0 ||
            saturated_threshold[0] >= saturated_threshold[1] || saturated_threshold[1] > 1.0 ||
            !cv::checkRange(ccm))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "node");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }
#endif

    template <typename TGetter>
    int copy_model_mat(
        const char* api_name,
        const jyppx_ocv_color_correction_model* model,
        jyppx_ocv_mat* destination,
        TGetter getter)
    {
        return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
            int status = validate_ready(api_name, model);
            if (status != OPENCV_CSHARP_STATUS_OK) return status;
            status = validate_pointer(api_name, destination, "destination");
            if (status != OPENCV_CSHARP_STATUS_OK) return status;
            getter(*model->value).copyTo(opencv_csharp_native::mat_value(destination));
            return OPENCV_CSHARP_STATUS_OK;
#else
            (void)model;
            (void)destination;
            (void)getter;
            return opencv_csharp_native::set_not_linked(api_name);
#endif
        });
    }
}

int jyppx_ocv_photo_ccm_gamma_correction(
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    double gamma)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_gamma_correction";
    return guarded(api_name, [&]() {
        int status = validate_pointer(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_pointer(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!std::isfinite(gamma) || gamma <= 0.0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "gamma");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::ccm::gammaCorrection(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            gamma);
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_create(jyppx_ocv_color_correction_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_create";
    return guarded(api_name, [&]() {
        if (model == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }
        *model = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        auto result = std::make_unique<jyppx_ocv_color_correction_model>();
        result->value = std::make_unique<cv::ccm::ColorCorrectionModel>();
        *model = result.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_create_color_checker(
    const jyppx_ocv_mat* src,
    int color_checker,
    jyppx_ocv_color_correction_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_create_color_checker";
    return guarded(api_name, [&]() {
        if (model == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }
        *model = nullptr;
        int status = validate_pointer(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const int sample_count = color_checker_count(color_checker);
        if (sample_count < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "color_checker");
        }
        const cv::Mat source = opencv_csharp_native::mat_value(src).clone();
        status = validate_color_samples(api_name, source, "src", sample_count);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        auto result = std::make_unique<jyppx_ocv_color_correction_model>();
        result->value = std::make_unique<cv::ccm::ColorCorrectionModel>(source, color_checker);
        result->sample_count = sample_count;
        *model = result.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)color_checker;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_create_reference_colors(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* colors,
    int reference_color_space,
    jyppx_ocv_color_correction_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_create_reference_colors";
    return guarded(api_name, [&]() {
        if (model == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }
        *model = nullptr;
        int status = validate_pointer(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_pointer(api_name, colors, "colors");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!valid_reference_color_space(reference_color_space))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reference_color_space");
        }
        const cv::Mat source = opencv_csharp_native::mat_value(src).clone();
        const cv::Mat reference = opencv_csharp_native::mat_value(colors).clone();
        status = validate_color_samples(api_name, source, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_color_samples(api_name, reference, "colors", source.rows);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        auto result = std::make_unique<jyppx_ocv_color_correction_model>();
        result->value = std::make_unique<cv::ccm::ColorCorrectionModel>(
            source,
            reference,
            static_cast<cv::ccm::ColorSpace>(reference_color_space));
        result->sample_count = source.rows;
        *model = result.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)reference_color_space;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_create_reference_colors_masked(
    const jyppx_ocv_mat* src,
    const jyppx_ocv_mat* colors,
    int reference_color_space,
    const jyppx_ocv_mat* colored_patches_mask,
    jyppx_ocv_color_correction_model** model)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_create_reference_colors_masked";
    return guarded(api_name, [&]() {
        if (model == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "model");
        }
        *model = nullptr;
        int status = validate_pointer(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_pointer(api_name, colors, "colors");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_pointer(api_name, colored_patches_mask, "colored_patches_mask");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!valid_reference_color_space(reference_color_space))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "reference_color_space");
        }
        const cv::Mat source = opencv_csharp_native::mat_value(src).clone();
        const cv::Mat reference = opencv_csharp_native::mat_value(colors).clone();
        const cv::Mat mask = opencv_csharp_native::mat_value(colored_patches_mask).clone();
        status = validate_color_samples(api_name, source, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_color_samples(api_name, reference, "colors", source.rows);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_color_mask(api_name, mask, source.rows);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        auto result = std::make_unique<jyppx_ocv_color_correction_model>();
        result->value = std::make_unique<cv::ccm::ColorCorrectionModel>(
            source,
            reference,
            static_cast<cv::ccm::ColorSpace>(reference_color_space),
            mask);
        result->sample_count = source.rows;
        *model = result.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)reference_color_space;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

void jyppx_ocv_photo_ccm_release_handle(jyppx_ocv_color_correction_model* model)
{
    delete model;
}

int jyppx_ocv_photo_ccm_set_color_space(jyppx_ocv_color_correction_model* model, int color_space)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_color_space";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!valid_model_color_space(color_space))
            return opencv_csharp_native::set_invalid_argument(api_name, "color_space");
        model->value->setColorSpace(static_cast<cv::ccm::ColorSpace>(color_space));
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)color_space;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_ccm_type(jyppx_ocv_color_correction_model* model, int ccm_type)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_ccm_type";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (ccm_type < cv::ccm::CCM_LINEAR || ccm_type > cv::ccm::CCM_AFFINE)
            return opencv_csharp_native::set_invalid_argument(api_name, "ccm_type");
        model->value->setCcmType(static_cast<cv::ccm::CcmType>(ccm_type));
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)ccm_type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_distance(jyppx_ocv_color_correction_model* model, int distance)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_distance";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (distance < cv::ccm::DISTANCE_CIE76 || distance > cv::ccm::DISTANCE_RGBL)
            return opencv_csharp_native::set_invalid_argument(api_name, "distance");
        model->value->setDistance(static_cast<cv::ccm::DistanceType>(distance));
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)distance;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_linearization(jyppx_ocv_color_correction_model* model, int linearization)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_linearization";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (linearization < cv::ccm::LINEARIZATION_IDENTITY || linearization > cv::ccm::LINEARIZATION_GRAYLOGPOLYFIT)
            return opencv_csharp_native::set_invalid_argument(api_name, "linearization");
        model->value->setLinearization(static_cast<cv::ccm::LinearizationType>(linearization));
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)linearization;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_linearization_gamma(jyppx_ocv_color_correction_model* model, double gamma)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_linearization_gamma";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!std::isfinite(gamma) || gamma <= 0.0)
            return opencv_csharp_native::set_invalid_argument(api_name, "gamma");
        model->value->setLinearizationGamma(gamma);
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)gamma;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_linearization_degree(jyppx_ocv_color_correction_model* model, int degree)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_linearization_degree";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (degree <= 0)
            return opencv_csharp_native::set_invalid_argument(api_name, "degree");
        model->value->setLinearizationDegree(degree);
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)degree;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_saturated_threshold(
    jyppx_ocv_color_correction_model* model,
    double lower,
    double upper)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_saturated_threshold";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!std::isfinite(lower) || !std::isfinite(upper) || lower < 0.0 || lower >= upper || upper > 1.0)
            return opencv_csharp_native::set_invalid_argument(api_name, "saturated_threshold");
        model->value->setSaturatedThreshold(lower, upper);
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)lower;
        (void)upper;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_weights_list(
    jyppx_ocv_color_correction_model* model,
    const jyppx_ocv_mat* weights_list)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_weights_list";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_pointer(api_name, weights_list, "weights_list");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::Mat weights = opencv_csharp_native::mat_value(weights_list).clone();
        if (!weights.empty() && (weights.dims > 2 || weights.type() != CV_64FC1 ||
            weights.cols != 1 || weights.rows != model->sample_count))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "weights_list");
        }
        model->value->setWeightsList(weights);
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)weights_list;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_weight_coeff(jyppx_ocv_color_correction_model* model, double weight_coeff)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_weight_coeff";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!std::isfinite(weight_coeff))
            return opencv_csharp_native::set_invalid_argument(api_name, "weight_coeff");
        model->value->setWeightCoeff(weight_coeff);
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)weight_coeff;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_initial_method(jyppx_ocv_color_correction_model* model, int initial_method)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_initial_method";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (initial_method < cv::ccm::INITIAL_METHOD_WHITE_BALANCE ||
            initial_method > cv::ccm::INITIAL_METHOD_LEAST_SQUARE)
            return opencv_csharp_native::set_invalid_argument(api_name, "initial_method");
        model->value->setInitialMethod(static_cast<cv::ccm::InitialMethodType>(initial_method));
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)initial_method;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_max_count(jyppx_ocv_color_correction_model* model, int max_count)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_max_count";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (max_count <= 0)
            return opencv_csharp_native::set_invalid_argument(api_name, "max_count");
        model->value->setMaxCount(max_count);
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)max_count;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_epsilon(jyppx_ocv_color_correction_model* model, double epsilon)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_epsilon";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (!std::isfinite(epsilon) || epsilon <= 0.0)
            return opencv_csharp_native::set_invalid_argument(api_name, "epsilon");
        model->value->setEpsilon(epsilon);
        model->ready = false;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)epsilon;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_set_rgb(jyppx_ocv_color_correction_model* model, int rgb)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_set_rgb";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (rgb != 0 && rgb != 1)
            return opencv_csharp_native::set_invalid_argument(api_name, "rgb");
        model->value->setRGB(rgb != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)rgb;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_compute(
    jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* color_correction_matrix)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_compute";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_pointer(api_name, color_correction_matrix, "color_correction_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (model->sample_count <= 0)
            return opencv_csharp_native::set_invalid_argument(api_name, "model_state");
        model->ready = false;
        model->value->compute().copyTo(opencv_csharp_native::mat_value(color_correction_matrix));
        model->ready = true;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)color_correction_matrix;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_get_color_correction_matrix(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* color_correction_matrix)
{
    return copy_model_mat(
        "jyppx_ocv_photo_ccm_get_color_correction_matrix",
        model,
        color_correction_matrix,
        [](const auto& value) { return value.getColorCorrectionMatrix(); });
}

int jyppx_ocv_photo_ccm_get_loss(
    const jyppx_ocv_color_correction_model* model,
    double* loss)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_get_loss";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_ready(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_pointer(api_name, loss, "loss");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        *loss = model->value->getLoss();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)loss;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_get_src_linear_rgb(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* src_linear_rgb)
{
    return copy_model_mat(
        "jyppx_ocv_photo_ccm_get_src_linear_rgb",
        model,
        src_linear_rgb,
        [](const auto& value) { return value.getSrcLinearRGB(); });
}

int jyppx_ocv_photo_ccm_get_ref_linear_rgb(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* ref_linear_rgb)
{
    return copy_model_mat(
        "jyppx_ocv_photo_ccm_get_ref_linear_rgb",
        model,
        ref_linear_rgb,
        [](const auto& value) { return value.getRefLinearRGB(); });
}

int jyppx_ocv_photo_ccm_get_mask(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* mask)
{
    return copy_model_mat(
        "jyppx_ocv_photo_ccm_get_mask",
        model,
        mask,
        [](const auto& value) { return value.getMask(); });
}

int jyppx_ocv_photo_ccm_get_weights(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_mat* weights)
{
    return copy_model_mat(
        "jyppx_ocv_photo_ccm_get_weights",
        model,
        weights,
        [](const auto& value) { return value.getWeights(); });
}

int jyppx_ocv_photo_ccm_correct_image(
    const jyppx_ocv_color_correction_model* model,
    const jyppx_ocv_mat* src,
    jyppx_ocv_mat* dst,
    int is_linear)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_correct_image";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_ready(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_pointer(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_pointer(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (is_linear != 0 && is_linear != 1)
            return opencv_csharp_native::set_invalid_argument(api_name, "is_linear");
        const cv::Mat& source = opencv_csharp_native::mat_value(src);
        if (source.empty() || source.dims > 2 ||
            (source.type() != CV_8UC3 && source.type() != CV_16UC3 && source.type() != CV_32FC3))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }
        model->value->correctImage(source, opencv_csharp_native::mat_value(dst), is_linear != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)src;
        (void)dst;
        (void)is_linear;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_write(
    const jyppx_ocv_color_correction_model* model,
    jyppx_ocv_core_file_storage* storage)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_write";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_ready(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::FileStorage* value = nullptr;
        status = opencv_csharp_native::access_core_file_storage(api_name, storage, &value);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        model->value->write(*value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)storage;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}

int jyppx_ocv_photo_ccm_read(
    jyppx_ocv_color_correction_model* model,
    const jyppx_ocv_core_file_node* node)
{
    constexpr const char* api_name = "jyppx_ocv_photo_ccm_read";
    return guarded(api_name, [&]() {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_model(api_name, model);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        const cv::FileNode* value = nullptr;
        status = opencv_csharp_native::access_core_file_node(api_name, node, &value);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_persisted_model_node(api_name, *value);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        model->ready = false;
        model->value->read(*value);
        if (model->value->getColorCorrectionMatrix().empty())
            return opencv_csharp_native::set_invalid_argument(api_name, "node");
        model->sample_count = 0;
        model->ready = true;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)model;
        (void)node;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    });
}
