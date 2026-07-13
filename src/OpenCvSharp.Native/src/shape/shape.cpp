#include "open_cv_sharp/shape/shape.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "shape_handles.h"

#include <new>

namespace
{
    constexpr int HISTOGRAM_COST_KIND_NORM = 1;
    constexpr int HISTOGRAM_COST_KIND_EMD = 2;
    constexpr int HISTOGRAM_COST_KIND_CHI = 3;
    constexpr int HISTOGRAM_COST_KIND_EMD_L1 = 4;
    constexpr int DISTANCE_KIND_SHAPE_CONTEXT = 1;
    constexpr int DISTANCE_KIND_HAUSDORFF = 2;

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_extractor(const char* api_name, const jyppx_ocv_shape_histogram_cost_extractor* extractor)
    {
        return extractor == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "extractor")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_distance_extractor(const char* api_name, const jyppx_ocv_shape_distance_extractor* extractor)
    {
        return extractor == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "extractor")
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

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
    int create_histogram_cost_handle(
        const char* api_name,
        const cv::Ptr<cv::HistogramCostExtractor>& native,
        int kind,
        jyppx_ocv_shape_histogram_cost_extractor** extractor)
    {
        if (extractor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "extractor");
        }

        *extractor = nullptr;
        jyppx_ocv_shape_histogram_cost_extractor* created = new (std::nothrow) jyppx_ocv_shape_histogram_cost_extractor();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        created->kind = kind;
        *extractor = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::NormHistogramCostExtractor* as_norm(jyppx_ocv_shape_histogram_cost_extractor* extractor)
    {
        return extractor->kind == HISTOGRAM_COST_KIND_NORM
            ? dynamic_cast<cv::NormHistogramCostExtractor*>(extractor->value.get())
            : nullptr;
    }

    const cv::NormHistogramCostExtractor* as_norm(const jyppx_ocv_shape_histogram_cost_extractor* extractor)
    {
        return extractor->kind == HISTOGRAM_COST_KIND_NORM
            ? dynamic_cast<const cv::NormHistogramCostExtractor*>(extractor->value.get())
            : nullptr;
    }

    cv::EMDHistogramCostExtractor* as_emd(jyppx_ocv_shape_histogram_cost_extractor* extractor)
    {
        return extractor->kind == HISTOGRAM_COST_KIND_EMD
            ? dynamic_cast<cv::EMDHistogramCostExtractor*>(extractor->value.get())
            : nullptr;
    }

    const cv::EMDHistogramCostExtractor* as_emd(const jyppx_ocv_shape_histogram_cost_extractor* extractor)
    {
        return extractor->kind == HISTOGRAM_COST_KIND_EMD
            ? dynamic_cast<const cv::EMDHistogramCostExtractor*>(extractor->value.get())
            : nullptr;
    }

    int create_distance_handle(
        const char* api_name,
        const cv::Ptr<cv::ShapeDistanceExtractor>& native,
        int kind,
        jyppx_ocv_shape_distance_extractor** extractor)
    {
        if (extractor == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "extractor");
        }

        *extractor = nullptr;
        jyppx_ocv_shape_distance_extractor* created = new (std::nothrow) jyppx_ocv_shape_distance_extractor();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        created->kind = kind;
        *extractor = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    cv::ShapeContextDistanceExtractor* as_shape_context(jyppx_ocv_shape_distance_extractor* extractor)
    {
        return extractor->kind == DISTANCE_KIND_SHAPE_CONTEXT
            ? dynamic_cast<cv::ShapeContextDistanceExtractor*>(extractor->value.get())
            : nullptr;
    }

    const cv::ShapeContextDistanceExtractor* as_shape_context(const jyppx_ocv_shape_distance_extractor* extractor)
    {
        return extractor->kind == DISTANCE_KIND_SHAPE_CONTEXT
            ? dynamic_cast<const cv::ShapeContextDistanceExtractor*>(extractor->value.get())
            : nullptr;
    }

    cv::HausdorffDistanceExtractor* as_hausdorff(jyppx_ocv_shape_distance_extractor* extractor)
    {
        return extractor->kind == DISTANCE_KIND_HAUSDORFF
            ? dynamic_cast<cv::HausdorffDistanceExtractor*>(extractor->value.get())
            : nullptr;
    }

    const cv::HausdorffDistanceExtractor* as_hausdorff(const jyppx_ocv_shape_distance_extractor* extractor)
    {
        return extractor->kind == DISTANCE_KIND_HAUSDORFF
            ? dynamic_cast<const cv::HausdorffDistanceExtractor*>(extractor->value.get())
            : nullptr;
    }

#endif
}

int jyppx_ocv_shape_emd_l1(const jyppx_ocv_mat* signature1, const jyppx_ocv_mat* signature2, float* distance)
{
    constexpr const char* api_name = "jyppx_ocv_shape_emd_l1";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, signature1, "signature1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, signature2, "signature2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, distance, "distance");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        *distance = cv::EMDL1(opencv_csharp_native::mat_value(signature1), opencv_csharp_native::mat_value(signature2));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *distance = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_norm_histogram_cost_extractor_create(
    int flag,
    int n_dummies,
    float default_cost,
    jyppx_ocv_shape_histogram_cost_extractor** extractor)
{
    constexpr const char* api_name = "jyppx_ocv_shape_norm_histogram_cost_extractor_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        return create_histogram_cost_handle(
            api_name,
            cv::createNormHistogramCostExtractor(flag, n_dummies, default_cost),
            HISTOGRAM_COST_KIND_NORM,
            extractor);
#else
        (void)flag; (void)n_dummies; (void)default_cost;
        if (extractor != nullptr) { *extractor = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_emd_histogram_cost_extractor_create(
    int flag,
    int n_dummies,
    float default_cost,
    jyppx_ocv_shape_histogram_cost_extractor** extractor)
{
    constexpr const char* api_name = "jyppx_ocv_shape_emd_histogram_cost_extractor_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        return create_histogram_cost_handle(
            api_name,
            cv::createEMDHistogramCostExtractor(flag, n_dummies, default_cost),
            HISTOGRAM_COST_KIND_EMD,
            extractor);
#else
        (void)flag; (void)n_dummies; (void)default_cost;
        if (extractor != nullptr) { *extractor = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_chi_histogram_cost_extractor_create(
    int n_dummies,
    float default_cost,
    jyppx_ocv_shape_histogram_cost_extractor** extractor)
{
    constexpr const char* api_name = "jyppx_ocv_shape_chi_histogram_cost_extractor_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        return create_histogram_cost_handle(
            api_name,
            cv::createChiHistogramCostExtractor(n_dummies, default_cost),
            HISTOGRAM_COST_KIND_CHI,
            extractor);
#else
        (void)n_dummies; (void)default_cost;
        if (extractor != nullptr) { *extractor = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_emd_l1_histogram_cost_extractor_create(
    int n_dummies,
    float default_cost,
    jyppx_ocv_shape_histogram_cost_extractor** extractor)
{
    constexpr const char* api_name = "jyppx_ocv_shape_emd_l1_histogram_cost_extractor_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        return create_histogram_cost_handle(
            api_name,
            cv::createEMDL1HistogramCostExtractor(n_dummies, default_cost),
            HISTOGRAM_COST_KIND_EMD_L1,
            extractor);
#else
        (void)n_dummies; (void)default_cost;
        if (extractor != nullptr) { *extractor = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_shape_histogram_cost_extractor_release_handle(jyppx_ocv_shape_histogram_cost_extractor* extractor)
{
    delete extractor;
}

int jyppx_ocv_shape_histogram_cost_extractor_build_cost_matrix(
    jyppx_ocv_shape_histogram_cost_extractor* extractor,
    const jyppx_ocv_mat* descriptors1,
    const jyppx_ocv_mat* descriptors2,
    jyppx_ocv_mat* cost_matrix)
{
    constexpr const char* api_name = "jyppx_ocv_shape_histogram_cost_extractor_build_cost_matrix";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, descriptors1, "descriptors1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, descriptors2, "descriptors2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, cost_matrix, "cost_matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        extractor->value->buildCostMatrix(
            opencv_csharp_native::mat_value(descriptors1),
            opencv_csharp_native::mat_value(descriptors2),
            opencv_csharp_native::mat_value(cost_matrix));
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

int jyppx_ocv_shape_histogram_cost_extractor_set_n_dummies(
    jyppx_ocv_shape_histogram_cost_extractor* extractor,
    int value)
{
    constexpr const char* api_name = "jyppx_ocv_shape_histogram_cost_extractor_set_n_dummies";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        extractor->value->setNDummies(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_histogram_cost_extractor_get_n_dummies(
    const jyppx_ocv_shape_histogram_cost_extractor* extractor,
    int* value)
{
    constexpr const char* api_name = "jyppx_ocv_shape_histogram_cost_extractor_get_n_dummies";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        *value = extractor->value->getNDummies();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_histogram_cost_extractor_set_default_cost(
    jyppx_ocv_shape_histogram_cost_extractor* extractor,
    float value)
{
    constexpr const char* api_name = "jyppx_ocv_shape_histogram_cost_extractor_set_default_cost";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        extractor->value->setDefaultCost(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_histogram_cost_extractor_get_default_cost(
    const jyppx_ocv_shape_histogram_cost_extractor* extractor,
    float* value)
{
    constexpr const char* api_name = "jyppx_ocv_shape_histogram_cost_extractor_get_default_cost";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        *value = extractor->value->getDefaultCost();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *value = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_histogram_cost_extractor_set_norm_flag(
    jyppx_ocv_shape_histogram_cost_extractor* extractor,
    int value)
{
    constexpr const char* api_name = "jyppx_ocv_shape_histogram_cost_extractor_set_norm_flag";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        cv::NormHistogramCostExtractor* norm = as_norm(extractor);
        if (norm != nullptr)
        {
            norm->setNormFlag(value);
            return OPENCV_CSHARP_STATUS_OK;
        }

        cv::EMDHistogramCostExtractor* emd = as_emd(extractor);
        if (emd != nullptr)
        {
            emd->setNormFlag(value);
            return OPENCV_CSHARP_STATUS_OK;
        }

        return opencv_csharp_native::set_invalid_argument(api_name, "extractor");
#else
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_histogram_cost_extractor_get_norm_flag(
    const jyppx_ocv_shape_histogram_cost_extractor* extractor,
    int* value)
{
    constexpr const char* api_name = "jyppx_ocv_shape_histogram_cost_extractor_get_norm_flag";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        const cv::NormHistogramCostExtractor* norm = as_norm(extractor);
        if (norm != nullptr)
        {
            *value = norm->getNormFlag();
            return OPENCV_CSHARP_STATUS_OK;
        }

        const cv::EMDHistogramCostExtractor* emd = as_emd(extractor);
        if (emd != nullptr)
        {
            *value = emd->getNormFlag();
            return OPENCV_CSHARP_STATUS_OK;
        }

        *value = 0;
        return opencv_csharp_native::set_invalid_argument(api_name, "extractor");
#else
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_context_distance_extractor_create(
    int n_angular_bins,
    int n_radial_bins,
    float inner_radius,
    float outer_radius,
    int iterations,
    jyppx_ocv_shape_distance_extractor** extractor)
{
    constexpr const char* api_name = "jyppx_ocv_shape_context_distance_extractor_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        return create_distance_handle(
            api_name,
            cv::createShapeContextDistanceExtractor(n_angular_bins, n_radial_bins, inner_radius, outer_radius, iterations),
            DISTANCE_KIND_SHAPE_CONTEXT,
            extractor);
#else
        (void)n_angular_bins; (void)n_radial_bins; (void)inner_radius; (void)outer_radius; (void)iterations;
        if (extractor != nullptr) { *extractor = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_hausdorff_distance_extractor_create(
    int distance_flag,
    float rank_proportion,
    jyppx_ocv_shape_distance_extractor** extractor)
{
    constexpr const char* api_name = "jyppx_ocv_shape_hausdorff_distance_extractor_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        return create_distance_handle(
            api_name,
            cv::createHausdorffDistanceExtractor(distance_flag, rank_proportion),
            DISTANCE_KIND_HAUSDORFF,
            extractor);
#else
        (void)distance_flag; (void)rank_proportion;
        if (extractor != nullptr) { *extractor = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_shape_distance_extractor_release_handle(jyppx_ocv_shape_distance_extractor* extractor)
{
    delete extractor;
}

int jyppx_ocv_shape_distance_extractor_compute_distance(
    jyppx_ocv_shape_distance_extractor* extractor,
    const jyppx_ocv_mat* contour1,
    const jyppx_ocv_mat* contour2,
    float* distance)
{
    constexpr const char* api_name = "jyppx_ocv_shape_distance_extractor_compute_distance";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_distance_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, contour1, "contour1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, contour2, "contour2");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, distance, "distance");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        *distance = extractor->value->computeDistance(
            opencv_csharp_native::mat_value(contour1),
            opencv_csharp_native::mat_value(contour2));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *distance = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_hausdorff_distance_extractor_set_distance_flag(
    jyppx_ocv_shape_distance_extractor* extractor,
    int value)
{
    constexpr const char* api_name = "jyppx_ocv_shape_hausdorff_distance_extractor_set_distance_flag";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_distance_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        cv::HausdorffDistanceExtractor* typed = as_hausdorff(extractor);
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "extractor");
        }

        typed->setDistanceFlag(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_hausdorff_distance_extractor_get_distance_flag(
    const jyppx_ocv_shape_distance_extractor* extractor,
    int* value)
{
    constexpr const char* api_name = "jyppx_ocv_shape_hausdorff_distance_extractor_get_distance_flag";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_distance_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        const cv::HausdorffDistanceExtractor* typed = as_hausdorff(extractor);
        if (typed == nullptr)
        {
            *value = 0;
            return opencv_csharp_native::set_invalid_argument(api_name, "extractor");
        }

        *value = typed->getDistanceFlag();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_hausdorff_distance_extractor_set_rank_proportion(
    jyppx_ocv_shape_distance_extractor* extractor,
    float value)
{
    constexpr const char* api_name = "jyppx_ocv_shape_hausdorff_distance_extractor_set_rank_proportion";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_distance_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        cv::HausdorffDistanceExtractor* typed = as_hausdorff(extractor);
        if (typed == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "extractor");
        }

        typed->setRankProportion(value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_shape_hausdorff_distance_extractor_get_rank_proportion(
    const jyppx_ocv_shape_distance_extractor* extractor,
    float* value)
{
    constexpr const char* api_name = "jyppx_ocv_shape_hausdorff_distance_extractor_get_rank_proportion";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_distance_extractor(api_name, extractor);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_SHAPE)
        const cv::HausdorffDistanceExtractor* typed = as_hausdorff(extractor);
        if (typed == nullptr)
        {
            *value = 0.0F;
            return opencv_csharp_native::set_invalid_argument(api_name, "extractor");
        }

        *value = typed->getRankProportion();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *value = 0.0F;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}


