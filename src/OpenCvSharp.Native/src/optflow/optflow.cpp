#include "open_cv_sharp/optflow/optflow.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "optflow_handles.h"

#include <new>
#include <vector>

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_dense(const char* api_name, const jyppx_ocv_optflow_dense* flow)
    {
        return flow == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "flow")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_sparse(const char* api_name, const jyppx_ocv_optflow_sparse* flow)
    {
        return flow == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "flow")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_dual_tvl1(const char* api_name, const jyppx_ocv_optflow_dual_tvl1* flow)
    {
        return flow == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "flow")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_parameter(const char* api_name, const jyppx_ocv_optflow_rlof_parameter* parameter)
    {
        return parameter == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "parameter")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_dense_rlof(const char* api_name, const jyppx_ocv_optflow_dense_rlof* flow)
    {
        return flow == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "flow")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_sparse_rlof(const char* api_name, const jyppx_ocv_optflow_sparse_rlof* flow)
    {
        return flow == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "flow")
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_int(const char* api_name, const int* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_float(const char* api_name, const float* value, const char* argument_name)
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

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
    cv::Ptr<cv::optflow::RLOFOpticalFlowParameter> optional_parameter(const jyppx_ocv_optflow_rlof_parameter* parameter)
    {
        return parameter == nullptr ? cv::Ptr<cv::optflow::RLOFOpticalFlowParameter>() : parameter->value;
    }

    int create_dense_handle(const char* api_name, const cv::Ptr<cv::DenseOpticalFlow>& native, jyppx_ocv_optflow_dense** flow)
    {
        if (flow == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "flow");
        }

        *flow = nullptr;
        jyppx_ocv_optflow_dense* created = new (std::nothrow) jyppx_ocv_optflow_dense();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *flow = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int create_parameter_handle(const char* api_name, const cv::Ptr<cv::optflow::RLOFOpticalFlowParameter>& native, jyppx_ocv_optflow_rlof_parameter** parameter)
    {
        if (parameter == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "parameter");
        }

        *parameter = nullptr;
        jyppx_ocv_optflow_rlof_parameter* created = new (std::nothrow) jyppx_ocv_optflow_rlof_parameter();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *parameter = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    jyppx_ocv_optflow_rect from_cv_rect(const cv::Rect& rect)
    {
        return jyppx_ocv_optflow_rect{ rect.x, rect.y, rect.width, rect.height };
    }
#endif
}

void jyppx_ocv_optflow_dense_release_handle(jyppx_ocv_optflow_dense* flow)
{
    delete flow;
}

void jyppx_ocv_optflow_sparse_release_handle(jyppx_ocv_optflow_sparse* flow)
{
    delete flow;
}

void jyppx_ocv_optflow_rlof_parameter_release_handle(jyppx_ocv_optflow_rlof_parameter* parameter)
{
    delete parameter;
}

int jyppx_ocv_optflow_dense_calc(jyppx_ocv_optflow_dense* flow, const jyppx_ocv_mat* i0, const jyppx_ocv_mat* i1, jyppx_ocv_mat* output_flow)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_calc";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dense(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, i0, "i0");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, i1, "i1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output_flow, "output_flow");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        flow->value->calc(opencv_csharp_native::mat_value(i0), opencv_csharp_native::mat_value(i1), opencv_csharp_native::mat_value(output_flow));
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

int jyppx_ocv_optflow_dense_collect_garbage(jyppx_ocv_optflow_dense* flow)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_collect_garbage";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dense(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        flow->value->collectGarbage();
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

int jyppx_ocv_optflow_sparse_calc(jyppx_ocv_optflow_sparse* flow, const jyppx_ocv_mat* prev_img, const jyppx_ocv_mat* next_img, const jyppx_ocv_mat* prev_pts, jyppx_ocv_mat* next_pts, jyppx_ocv_mat* status_mat, jyppx_ocv_mat* err)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_sparse_calc";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_sparse(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, prev_img, "prev_img");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, next_img, "next_img");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, prev_pts, "prev_pts");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, next_pts, "next_pts");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, status_mat, "status");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, err, "err");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        flow->value->calc(
            opencv_csharp_native::mat_value(prev_img),
            opencv_csharp_native::mat_value(next_img),
            opencv_csharp_native::mat_value(prev_pts),
            opencv_csharp_native::mat_value(next_pts),
            opencv_csharp_native::mat_value(status_mat),
            opencv_csharp_native::mat_value(err));
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

int jyppx_ocv_optflow_dual_tvl1_create(double tau, double lambda_value, double theta, int nscales, int warps, double epsilon, int inner_iterations, int outer_iterations, double scale_step, double gamma, int median_filtering, int use_initial_flow, jyppx_ocv_optflow_dual_tvl1** flow)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dual_tvl1_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        if (flow == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "flow");
        }

        *flow = nullptr;
        jyppx_ocv_optflow_dual_tvl1* created = new (std::nothrow) jyppx_ocv_optflow_dual_tvl1();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::optflow::DualTVL1OpticalFlow::create(tau, lambda_value, theta, nscales, warps, epsilon, inner_iterations, outer_iterations, scale_step, gamma, median_filtering, use_initial_flow != 0);
        created->value = created->concrete;
        *flow = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)tau; (void)lambda_value; (void)theta; (void)nscales; (void)warps; (void)epsilon; (void)inner_iterations;
        (void)outer_iterations; (void)scale_step; (void)gamma; (void)median_filtering; (void)use_initial_flow;
        if (flow != nullptr) { *flow = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_dual_tvl1_get_int(const jyppx_ocv_optflow_dual_tvl1* flow, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dual_tvl1_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dual_tvl1(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: *value = flow->concrete->getScalesNumber(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = flow->concrete->getWarpingsNumber(); return OPENCV_CSHARP_STATUS_OK;
        case 2: *value = flow->concrete->getInnerIterations(); return OPENCV_CSHARP_STATUS_OK;
        case 3: *value = flow->concrete->getOuterIterations(); return OPENCV_CSHARP_STATUS_OK;
        case 4: *value = flow->concrete->getUseInitialFlow() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case 5: *value = flow->concrete->getMedianFiltering(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_dual_tvl1_set_int(jyppx_ocv_optflow_dual_tvl1* flow, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dual_tvl1_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dual_tvl1(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: flow->concrete->setScalesNumber(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: flow->concrete->setWarpingsNumber(value); return OPENCV_CSHARP_STATUS_OK;
        case 2: flow->concrete->setInnerIterations(value); return OPENCV_CSHARP_STATUS_OK;
        case 3: flow->concrete->setOuterIterations(value); return OPENCV_CSHARP_STATUS_OK;
        case 4: flow->concrete->setUseInitialFlow(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case 5: flow->concrete->setMedianFiltering(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_dual_tvl1_get_double(const jyppx_ocv_optflow_dual_tvl1* flow, int property_id, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dual_tvl1_get_double";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dual_tvl1(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: *value = flow->concrete->getTau(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = flow->concrete->getLambda(); return OPENCV_CSHARP_STATUS_OK;
        case 2: *value = flow->concrete->getTheta(); return OPENCV_CSHARP_STATUS_OK;
        case 3: *value = flow->concrete->getGamma(); return OPENCV_CSHARP_STATUS_OK;
        case 4: *value = flow->concrete->getEpsilon(); return OPENCV_CSHARP_STATUS_OK;
        case 5: *value = flow->concrete->getScaleStep(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_dual_tvl1_set_double(jyppx_ocv_optflow_dual_tvl1* flow, int property_id, double value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dual_tvl1_set_double";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dual_tvl1(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: flow->concrete->setTau(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: flow->concrete->setLambda(value); return OPENCV_CSHARP_STATUS_OK;
        case 2: flow->concrete->setTheta(value); return OPENCV_CSHARP_STATUS_OK;
        case 3: flow->concrete->setGamma(value); return OPENCV_CSHARP_STATUS_OK;
        case 4: flow->concrete->setEpsilon(value); return OPENCV_CSHARP_STATUS_OK;
        case 5: flow->concrete->setScaleStep(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_rlof_parameter_create(jyppx_ocv_optflow_rlof_parameter** parameter)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_rlof_parameter_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        return create_parameter_handle(api_name, cv::optflow::RLOFOpticalFlowParameter::create(), parameter);
#else
        if (parameter != nullptr) { *parameter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_rlof_parameter_get_int(const jyppx_ocv_optflow_rlof_parameter* parameter, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_rlof_parameter_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_parameter(api_name, parameter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: *value = static_cast<int>(parameter->value->getSolverType()); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = static_cast<int>(parameter->value->getSupportRegionType()); return OPENCV_CSHARP_STATUS_OK;
        case 2: *value = parameter->value->getSmallWinSize(); return OPENCV_CSHARP_STATUS_OK;
        case 3: *value = parameter->value->getLargeWinSize(); return OPENCV_CSHARP_STATUS_OK;
        case 4: *value = parameter->value->getCrossSegmentationThreshold(); return OPENCV_CSHARP_STATUS_OK;
        case 5: *value = parameter->value->getMaxLevel(); return OPENCV_CSHARP_STATUS_OK;
        case 6: *value = parameter->value->getUseInitialFlow() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case 7: *value = parameter->value->getUseIlluminationModel() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case 8: *value = parameter->value->getUseGlobalMotionPrior() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case 9: *value = parameter->value->getMaxIteration(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_rlof_parameter_set_int(jyppx_ocv_optflow_rlof_parameter* parameter, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_rlof_parameter_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_parameter(api_name, parameter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: parameter->value->setSolverType(static_cast<cv::optflow::SolverType>(value)); return OPENCV_CSHARP_STATUS_OK;
        case 1: parameter->value->setSupportRegionType(static_cast<cv::optflow::SupportRegionType>(value)); return OPENCV_CSHARP_STATUS_OK;
        case 2: parameter->value->setSmallWinSize(value); return OPENCV_CSHARP_STATUS_OK;
        case 3: parameter->value->setLargeWinSize(value); return OPENCV_CSHARP_STATUS_OK;
        case 4: parameter->value->setCrossSegmentationThreshold(value); return OPENCV_CSHARP_STATUS_OK;
        case 5: parameter->value->setMaxLevel(value); return OPENCV_CSHARP_STATUS_OK;
        case 6: parameter->value->setUseInitialFlow(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case 7: parameter->value->setUseIlluminationModel(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case 8: parameter->value->setUseGlobalMotionPrior(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case 9: parameter->value->setMaxIteration(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_rlof_parameter_get_float(const jyppx_ocv_optflow_rlof_parameter* parameter, int property_id, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_rlof_parameter_get_float";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_parameter(api_name, parameter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: *value = parameter->value->getNormSigma0(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = parameter->value->getNormSigma1(); return OPENCV_CSHARP_STATUS_OK;
        case 2: *value = parameter->value->getMinEigenValue(); return OPENCV_CSHARP_STATUS_OK;
        case 3: *value = parameter->value->getGlobalMotionRansacThreshold(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_rlof_parameter_set_float(jyppx_ocv_optflow_rlof_parameter* parameter, int property_id, float value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_rlof_parameter_set_float";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_parameter(api_name, parameter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: parameter->value->setNormSigma0(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: parameter->value->setNormSigma1(value); return OPENCV_CSHARP_STATUS_OK;
        case 2: parameter->value->setMinEigenValue(value); return OPENCV_CSHARP_STATUS_OK;
        case 3: parameter->value->setGlobalMotionRansacThreshold(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_rlof_parameter_set_use_m_estimator(jyppx_ocv_optflow_rlof_parameter* parameter, int value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_rlof_parameter_set_use_m_estimator";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_parameter(api_name, parameter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        parameter->value->setUseMEstimator(value != 0);
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

int jyppx_ocv_optflow_dense_rlof_create(const jyppx_ocv_optflow_rlof_parameter* parameter, float forward_backward_threshold, int grid_width, int grid_height, int interpolation_type, int epic_k, float epic_sigma, float epic_lambda, int ric_sp_size, int ric_slic_type, int use_post_proc, float fgs_lambda, float fgs_sigma, int use_variational_refinement, jyppx_ocv_optflow_dense_rlof** flow)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_rlof_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        if (flow == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "flow");
        }

        *flow = nullptr;
        jyppx_ocv_optflow_dense_rlof* created = new (std::nothrow) jyppx_ocv_optflow_dense_rlof();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::optflow::DenseRLOFOpticalFlow::create(
            optional_parameter(parameter),
            forward_backward_threshold,
            cv::Size(grid_width, grid_height),
            static_cast<cv::optflow::InterpolationType>(interpolation_type),
            epic_k,
            epic_sigma,
            epic_lambda,
            ric_sp_size,
            ric_slic_type,
            use_post_proc != 0,
            fgs_lambda,
            fgs_sigma,
            use_variational_refinement != 0);
        created->value = created->concrete;
        *flow = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)parameter; (void)forward_backward_threshold; (void)grid_width; (void)grid_height; (void)interpolation_type;
        (void)epic_k; (void)epic_sigma; (void)epic_lambda; (void)ric_sp_size; (void)ric_slic_type; (void)use_post_proc;
        (void)fgs_lambda; (void)fgs_sigma; (void)use_variational_refinement;
        if (flow != nullptr) { *flow = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_dense_rlof_get_parameter(const jyppx_ocv_optflow_dense_rlof* flow, jyppx_ocv_optflow_rlof_parameter** parameter)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_rlof_get_parameter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dense_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        return create_parameter_handle(api_name, flow->concrete->getRLOFOpticalFlowParameter(), parameter);
#else
        if (parameter != nullptr) { *parameter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_dense_rlof_set_parameter(jyppx_ocv_optflow_dense_rlof* flow, const jyppx_ocv_optflow_rlof_parameter* parameter)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_rlof_set_parameter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dense_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_parameter(api_name, parameter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        flow->concrete->setRLOFOpticalFlowParameter(parameter->value);
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

int jyppx_ocv_optflow_dense_rlof_get_int(const jyppx_ocv_optflow_dense_rlof* flow, int property_id, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_rlof_get_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dense_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: *value = static_cast<int>(flow->concrete->getInterpolation()); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = flow->concrete->getEPICK(); return OPENCV_CSHARP_STATUS_OK;
        case 2: *value = flow->concrete->getUsePostProc() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case 3: *value = flow->concrete->getUseVariationalRefinement() ? 1 : 0; return OPENCV_CSHARP_STATUS_OK;
        case 4: *value = flow->concrete->getRICSPSize(); return OPENCV_CSHARP_STATUS_OK;
        case 5: *value = flow->concrete->getRICSLICType(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_dense_rlof_set_int(jyppx_ocv_optflow_dense_rlof* flow, int property_id, int value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_rlof_set_int";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dense_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: flow->concrete->setInterpolation(static_cast<cv::optflow::InterpolationType>(value)); return OPENCV_CSHARP_STATUS_OK;
        case 1: flow->concrete->setEPICK(value); return OPENCV_CSHARP_STATUS_OK;
        case 2: flow->concrete->setUsePostProc(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case 3: flow->concrete->setUseVariationalRefinement(value != 0); return OPENCV_CSHARP_STATUS_OK;
        case 4: flow->concrete->setRICSPSize(value); return OPENCV_CSHARP_STATUS_OK;
        case 5: flow->concrete->setRICSLICType(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_dense_rlof_get_float(const jyppx_ocv_optflow_dense_rlof* flow, int property_id, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_rlof_get_float";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dense_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: *value = flow->concrete->getForwardBackward(); return OPENCV_CSHARP_STATUS_OK;
        case 1: *value = flow->concrete->getEPICSigma(); return OPENCV_CSHARP_STATUS_OK;
        case 2: *value = flow->concrete->getEPICLambda(); return OPENCV_CSHARP_STATUS_OK;
        case 3: *value = flow->concrete->getFgsLambda(); return OPENCV_CSHARP_STATUS_OK;
        case 4: *value = flow->concrete->getFgsSigma(); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_dense_rlof_set_float(jyppx_ocv_optflow_dense_rlof* flow, int property_id, float value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_rlof_set_float";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dense_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        switch (property_id)
        {
        case 0: flow->concrete->setForwardBackward(value); return OPENCV_CSHARP_STATUS_OK;
        case 1: flow->concrete->setEPICSigma(value); return OPENCV_CSHARP_STATUS_OK;
        case 2: flow->concrete->setEPICLambda(value); return OPENCV_CSHARP_STATUS_OK;
        case 3: flow->concrete->setFgsLambda(value); return OPENCV_CSHARP_STATUS_OK;
        case 4: flow->concrete->setFgsSigma(value); return OPENCV_CSHARP_STATUS_OK;
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

int jyppx_ocv_optflow_dense_rlof_get_grid_step(const jyppx_ocv_optflow_dense_rlof* flow, int* width, int* height)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_rlof_get_grid_step";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dense_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, width, "width");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, height, "height");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        cv::Size size = flow->concrete->getGridStep();
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

int jyppx_ocv_optflow_dense_rlof_set_grid_step(jyppx_ocv_optflow_dense_rlof* flow, int width, int height)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_dense_rlof_set_grid_step";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_dense_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        flow->concrete->setGridStep(cv::Size(width, height));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width; (void)height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_sparse_rlof_create(const jyppx_ocv_optflow_rlof_parameter* parameter, float forward_backward_threshold, jyppx_ocv_optflow_sparse_rlof** flow)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_sparse_rlof_create";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        if (flow == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "flow");
        }

        *flow = nullptr;
        jyppx_ocv_optflow_sparse_rlof* created = new (std::nothrow) jyppx_ocv_optflow_sparse_rlof();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->concrete = cv::optflow::SparseRLOFOpticalFlow::create(optional_parameter(parameter), forward_backward_threshold);
        created->value = created->concrete;
        *flow = created;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)parameter; (void)forward_backward_threshold;
        if (flow != nullptr) { *flow = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_sparse_rlof_get_parameter(const jyppx_ocv_optflow_sparse_rlof* flow, jyppx_ocv_optflow_rlof_parameter** parameter)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_sparse_rlof_get_parameter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_sparse_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        return create_parameter_handle(api_name, flow->concrete->getRLOFOpticalFlowParameter(), parameter);
#else
        if (parameter != nullptr) { *parameter = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_sparse_rlof_set_parameter(jyppx_ocv_optflow_sparse_rlof* flow, const jyppx_ocv_optflow_rlof_parameter* parameter)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_sparse_rlof_set_parameter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_sparse_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_parameter(api_name, parameter);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        flow->concrete->setRLOFOpticalFlowParameter(parameter->value);
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

int jyppx_ocv_optflow_sparse_rlof_get_forward_backward(const jyppx_ocv_optflow_sparse_rlof* flow, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_sparse_rlof_get_forward_backward";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_sparse_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_float(api_name, value, "value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        *value = flow->concrete->getForwardBackward();
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

int jyppx_ocv_optflow_sparse_rlof_set_forward_backward(jyppx_ocv_optflow_sparse_rlof* flow, float value)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_sparse_rlof_set_forward_backward";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_sparse_rlof(api_name, flow);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        flow->concrete->setForwardBackward(value);
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

int jyppx_ocv_optflow_create_deep_flow(jyppx_ocv_optflow_dense** flow)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_create_deep_flow";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        return create_dense_handle(api_name, cv::optflow::createOptFlow_DeepFlow(), flow);
#else
        if (flow != nullptr) { *flow = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_create_simple_flow(jyppx_ocv_optflow_dense** flow)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_create_simple_flow";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        return create_dense_handle(api_name, cv::optflow::createOptFlow_SimpleFlow(), flow);
#else
        if (flow != nullptr) { *flow = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_create_farneback(jyppx_ocv_optflow_dense** flow)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_create_farneback";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        return create_dense_handle(api_name, cv::optflow::createOptFlow_Farneback(), flow);
#else
        if (flow != nullptr) { *flow = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_create_sparse_to_dense(jyppx_ocv_optflow_dense** flow)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_create_sparse_to_dense";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        return create_dense_handle(api_name, cv::optflow::createOptFlow_SparseToDense(), flow);
#else
        if (flow != nullptr) { *flow = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_calc_optical_flow_sf_simple(const jyppx_ocv_mat* from, const jyppx_ocv_mat* to, jyppx_ocv_mat* flow, int layers, int averaging_block_size, int max_flow)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_calc_optical_flow_sf_simple";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, from, "from");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, to, "to");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, flow, "flow");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        cv::optflow::calcOpticalFlowSF(opencv_csharp_native::mat_value(from), opencv_csharp_native::mat_value(to), opencv_csharp_native::mat_value(flow), layers, averaging_block_size, max_flow);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)layers; (void)averaging_block_size; (void)max_flow;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_calc_optical_flow_sf(const jyppx_ocv_mat* from, const jyppx_ocv_mat* to, jyppx_ocv_mat* flow, int layers, int averaging_block_size, int max_flow, double sigma_dist, double sigma_color, int postprocess_window, double sigma_dist_fix, double sigma_color_fix, double occ_thr, int upscale_averaging_radius, double upscale_sigma_dist, double upscale_sigma_color, double speed_up_thr)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_calc_optical_flow_sf";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, from, "from");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, to, "to");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, flow, "flow");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        cv::optflow::calcOpticalFlowSF(
            opencv_csharp_native::mat_value(from),
            opencv_csharp_native::mat_value(to),
            opencv_csharp_native::mat_value(flow),
            layers,
            averaging_block_size,
            max_flow,
            sigma_dist,
            sigma_color,
            postprocess_window,
            sigma_dist_fix,
            sigma_color_fix,
            occ_thr,
            upscale_averaging_radius,
            upscale_sigma_dist,
            upscale_sigma_color,
            speed_up_thr);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)layers; (void)averaging_block_size; (void)max_flow; (void)sigma_dist; (void)sigma_color; (void)postprocess_window;
        (void)sigma_dist_fix; (void)sigma_color_fix; (void)occ_thr; (void)upscale_averaging_radius; (void)upscale_sigma_dist;
        (void)upscale_sigma_color; (void)speed_up_thr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_calc_optical_flow_sparse_to_dense(const jyppx_ocv_mat* from, const jyppx_ocv_mat* to, jyppx_ocv_mat* flow, int grid_step, int k, float sigma, int use_post_proc, float fgs_lambda, float fgs_sigma)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_calc_optical_flow_sparse_to_dense";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, from, "from");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, to, "to");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, flow, "flow");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        cv::optflow::calcOpticalFlowSparseToDense(opencv_csharp_native::mat_value(from), opencv_csharp_native::mat_value(to), opencv_csharp_native::mat_value(flow), grid_step, k, sigma, use_post_proc != 0, fgs_lambda, fgs_sigma);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)grid_step; (void)k; (void)sigma; (void)use_post_proc; (void)fgs_lambda; (void)fgs_sigma;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_calc_optical_flow_dense_rlof(const jyppx_ocv_mat* i0, const jyppx_ocv_mat* i1, jyppx_ocv_mat* flow, const jyppx_ocv_optflow_rlof_parameter* parameter, float forward_backward_threshold, int grid_width, int grid_height, int interpolation_type, int epic_k, float epic_sigma, float epic_lambda, int ric_sp_size, int ric_slic_type, int use_post_proc, float fgs_lambda, float fgs_sigma, int use_variational_refinement)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_calc_optical_flow_dense_rlof";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, i0, "i0");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, i1, "i1");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, flow, "flow");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        cv::optflow::calcOpticalFlowDenseRLOF(
            opencv_csharp_native::mat_value(i0),
            opencv_csharp_native::mat_value(i1),
            opencv_csharp_native::mat_value(flow),
            optional_parameter(parameter),
            forward_backward_threshold,
            cv::Size(grid_width, grid_height),
            static_cast<cv::optflow::InterpolationType>(interpolation_type),
            epic_k,
            epic_sigma,
            epic_lambda,
            ric_sp_size,
            ric_slic_type,
            use_post_proc != 0,
            fgs_lambda,
            fgs_sigma,
            use_variational_refinement != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)parameter; (void)forward_backward_threshold; (void)grid_width; (void)grid_height; (void)interpolation_type; (void)epic_k;
        (void)epic_sigma; (void)epic_lambda; (void)ric_sp_size; (void)ric_slic_type; (void)use_post_proc; (void)fgs_lambda;
        (void)fgs_sigma; (void)use_variational_refinement;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_optflow_calc_optical_flow_sparse_rlof(const jyppx_ocv_mat* prev_img, const jyppx_ocv_mat* next_img, const jyppx_ocv_mat* prev_pts, jyppx_ocv_mat* next_pts, jyppx_ocv_mat* status_mat, jyppx_ocv_mat* err, const jyppx_ocv_optflow_rlof_parameter* parameter, float forward_backward_threshold)
{
    constexpr const char* api_name = "jyppx_ocv_optflow_calc_optical_flow_sparse_rlof";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, prev_img, "prev_img");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, next_img, "next_img");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, prev_pts, "prev_pts");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, next_pts, "next_pts");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, status_mat, "status");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, err, "err");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        cv::optflow::calcOpticalFlowSparseRLOF(
            opencv_csharp_native::mat_value(prev_img),
            opencv_csharp_native::mat_value(next_img),
            opencv_csharp_native::mat_value(prev_pts),
            opencv_csharp_native::mat_value(next_pts),
            opencv_csharp_native::mat_value(status_mat),
            opencv_csharp_native::mat_value(err),
            optional_parameter(parameter),
            forward_backward_threshold);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)parameter; (void)forward_backward_threshold;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_motempl_update_motion_history(const jyppx_ocv_mat* silhouette, jyppx_ocv_mat* mhi, double timestamp, double duration)
{
    constexpr const char* api_name = "jyppx_ocv_motempl_update_motion_history";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, silhouette, "silhouette");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mhi, "mhi");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        cv::motempl::updateMotionHistory(opencv_csharp_native::mat_value(silhouette), opencv_csharp_native::mat_value(mhi), timestamp, duration);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)timestamp; (void)duration;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_motempl_calc_motion_gradient(const jyppx_ocv_mat* mhi, jyppx_ocv_mat* mask, jyppx_ocv_mat* orientation, double delta1, double delta2, int aperture_size)
{
    constexpr const char* api_name = "jyppx_ocv_motempl_calc_motion_gradient";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, mhi, "mhi");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, orientation, "orientation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        cv::motempl::calcMotionGradient(opencv_csharp_native::mat_value(mhi), opencv_csharp_native::mat_value(mask), opencv_csharp_native::mat_value(orientation), delta1, delta2, aperture_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)delta1; (void)delta2; (void)aperture_size;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_motempl_calc_global_orientation(const jyppx_ocv_mat* orientation, const jyppx_ocv_mat* mask, const jyppx_ocv_mat* mhi, double timestamp, double duration, double* angle)
{
    constexpr const char* api_name = "jyppx_ocv_motempl_calc_global_orientation";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, orientation, "orientation");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mhi, "mhi");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, angle, "angle");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        *angle = cv::motempl::calcGlobalOrientation(opencv_csharp_native::mat_value(orientation), opencv_csharp_native::mat_value(mask), opencv_csharp_native::mat_value(mhi), timestamp, duration);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)timestamp; (void)duration;
        *angle = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_motempl_segment_motion_count(const jyppx_ocv_mat* mhi, jyppx_ocv_mat* segmask, double timestamp, double seg_thresh, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_motempl_segment_motion_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, mhi, "mhi");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, segmask, "segmask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        std::vector<cv::Rect> rects;
        cv::motempl::segmentMotion(opencv_csharp_native::mat_value(mhi), opencv_csharp_native::mat_value(segmask), rects, timestamp, seg_thresh);
        *count = static_cast<int>(rects.size());
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)timestamp; (void)seg_thresh;
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_motempl_segment_motion_fill(const jyppx_ocv_mat* mhi, jyppx_ocv_mat* segmask, double timestamp, double seg_thresh, jyppx_ocv_optflow_rect* rects, int rect_capacity, int* count)
{
    constexpr const char* api_name = "jyppx_ocv_motempl_segment_motion_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, mhi, "mhi");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, segmask, "segmask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, count, "count");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (rect_capacity < 0 || (rect_capacity > 0 && rects == nullptr))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rects");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
        std::vector<cv::Rect> native_rects;
        cv::motempl::segmentMotion(opencv_csharp_native::mat_value(mhi), opencv_csharp_native::mat_value(segmask), native_rects, timestamp, seg_thresh);
        *count = static_cast<int>(native_rects.size());
        if (rect_capacity < *count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rect_capacity");
        }

        for (int i = 0; i < *count; ++i)
        {
            rects[i] = from_cv_rect(native_rects[static_cast<size_t>(i)]);
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)timestamp; (void)seg_thresh;
        *count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

