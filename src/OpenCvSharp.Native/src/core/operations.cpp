#include "open_cv_sharp/core/operations.h"

#include "mat_handle.h"
#include "../error_state.h"

#include <new>
#include <vector>

namespace
{
    bool validate_scalar_buffer(const char* api_name, double* values, int length, const char* parameter_name)
    {
        if (values == nullptr)
        {
            opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
            return false;
        }

        if (length < 4)
        {
            opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
            return false;
        }

        return true;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::InputArray input_or_no_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::Scalar scalar_from_values(double v0, double v1, double v2, double v3)
    {
        return cv::Scalar(v0, v1, v2, v3);
    }

    bool validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* parameter_name)
    {
        if (mat == nullptr)
        {
            opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
            return false;
        }

        return true;
    }

    bool validate_output_mat(const char* api_name, jyppx_ocv_mat* mat, const char* parameter_name)
    {
        if (mat == nullptr)
        {
            opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
            return false;
        }

        return true;
    }

    void write_scalar(const cv::Scalar& value, double* values)
    {
        values[0] = value[0];
        values[1] = value[1];
        values[2] = value[2];
        values[3] = value[3];
    }

    typedef void (*binary_array_op)(
        cv::InputArray src1,
        cv::InputArray src2,
        cv::OutputArray dst,
        cv::InputArray mask,
        int dtype);

    int run_binary_array_op(
        const char* api_name,
        binary_array_op operation,
        const jyppx_ocv_mat* src1,
        const jyppx_ocv_mat* src2,
        jyppx_ocv_mat* dst,
        const jyppx_ocv_mat* mask,
        int dtype)
    {
        if (!validate_mat(api_name, src1, "src1") ||
            !validate_mat(api_name, src2, "src2") ||
            !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        operation(
            opencv_csharp_native::mat_value(src1),
            opencv_csharp_native::mat_value(src2),
            opencv_csharp_native::mat_value(dst),
            input_or_no_array(mask),
            dtype);
        return OPENCV_CSHARP_STATUS_OK;
    }

    typedef void (*binary_scalar_array_op)(
        cv::InputArray src1,
        cv::InputArray src2,
        cv::OutputArray dst,
        cv::InputArray mask,
        int dtype);

    int run_binary_scalar_array_op(
        const char* api_name,
        binary_scalar_array_op operation,
        const jyppx_ocv_mat* src,
        double v0,
        double v1,
        double v2,
        double v3,
        jyppx_ocv_mat* dst,
        const jyppx_ocv_mat* mask,
        int dtype)
    {
        if (!validate_mat(api_name, src, "src") ||
            !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        operation(
            opencv_csharp_native::mat_value(src),
            scalar_from_values(v0, v1, v2, v3),
            opencv_csharp_native::mat_value(dst),
            input_or_no_array(mask),
            dtype);
        return OPENCV_CSHARP_STATUS_OK;
    }

    typedef void (*binary_no_mask_op)(
        cv::InputArray src1,
        cv::InputArray src2,
        cv::OutputArray dst);

    int run_binary_no_mask_op(
        const char* api_name,
        binary_no_mask_op operation,
        const jyppx_ocv_mat* src1,
        const jyppx_ocv_mat* src2,
        jyppx_ocv_mat* dst)
    {
        if (!validate_mat(api_name, src1, "src1") ||
            !validate_mat(api_name, src2, "src2") ||
            !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        operation(
            opencv_csharp_native::mat_value(src1),
            opencv_csharp_native::mat_value(src2),
            opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
    }

    typedef void (*bitwise_op)(
        cv::InputArray src1,
        cv::InputArray src2,
        cv::OutputArray dst,
        cv::InputArray mask);

    int run_bitwise_op(
        const char* api_name,
        bitwise_op operation,
        const jyppx_ocv_mat* src1,
        const jyppx_ocv_mat* src2,
        jyppx_ocv_mat* dst,
        const jyppx_ocv_mat* mask)
    {
        if (!validate_mat(api_name, src1, "src1") ||
            !validate_mat(api_name, src2, "src2") ||
            !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        operation(
            opencv_csharp_native::mat_value(src1),
            opencv_csharp_native::mat_value(src2),
            opencv_csharp_native::mat_value(dst),
            input_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_core_add(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst, const jyppx_ocv_mat* mask, int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_core_add";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_binary_array_op(api_name, cv::add, src1, src2, dst, mask, dtype);
#else
        (void)src1; (void)src2; (void)dst; (void)mask; (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_add_scalar(const jyppx_ocv_mat* src, double v0, double v1, double v2, double v3, jyppx_ocv_mat* dst, const jyppx_ocv_mat* mask, int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_core_add_scalar";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_binary_scalar_array_op(api_name, cv::add, src, v0, v1, v2, v3, dst, mask, dtype);
#else
        (void)src; (void)v0; (void)v1; (void)v2; (void)v3; (void)dst; (void)mask; (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_subtract(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst, const jyppx_ocv_mat* mask, int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_core_subtract";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_binary_array_op(api_name, cv::subtract, src1, src2, dst, mask, dtype);
#else
        (void)src1; (void)src2; (void)dst; (void)mask; (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_subtract_scalar(const jyppx_ocv_mat* src, double v0, double v1, double v2, double v3, jyppx_ocv_mat* dst, const jyppx_ocv_mat* mask, int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_core_subtract_scalar";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_binary_scalar_array_op(api_name, cv::subtract, src, v0, v1, v2, v3, dst, mask, dtype);
#else
        (void)src; (void)v0; (void)v1; (void)v2; (void)v3; (void)dst; (void)mask; (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_multiply(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst, double scale, int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_core_multiply";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src1, "src1") || !validate_mat(api_name, src2, "src2") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::multiply(opencv_csharp_native::mat_value(src1), opencv_csharp_native::mat_value(src2), opencv_csharp_native::mat_value(dst), scale, dtype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src1; (void)src2; (void)dst; (void)scale; (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_divide(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst, double scale, int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_core_divide";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src1, "src1") || !validate_mat(api_name, src2, "src2") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::divide(opencv_csharp_native::mat_value(src1), opencv_csharp_native::mat_value(src2), opencv_csharp_native::mat_value(dst), scale, dtype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src1; (void)src2; (void)dst; (void)scale; (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_scale_add(const jyppx_ocv_mat* src1, double alpha, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_scale_add";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src1, "src1") || !validate_mat(api_name, src2, "src2") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::scaleAdd(opencv_csharp_native::mat_value(src1), alpha, opencv_csharp_native::mat_value(src2), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src1; (void)alpha; (void)src2; (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_add_weighted(const jyppx_ocv_mat* src1, double alpha, const jyppx_ocv_mat* src2, double beta, double gamma, jyppx_ocv_mat* dst, int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_core_add_weighted";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src1, "src1") || !validate_mat(api_name, src2, "src2") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::addWeighted(opencv_csharp_native::mat_value(src1), alpha, opencv_csharp_native::mat_value(src2), beta, gamma, opencv_csharp_native::mat_value(dst), dtype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src1; (void)alpha; (void)src2; (void)beta; (void)gamma; (void)dst; (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_absdiff(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_absdiff";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_binary_no_mask_op(api_name, cv::absdiff, src1, src2, dst);
#else
        (void)src1; (void)src2; (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_absdiff_scalar(const jyppx_ocv_mat* src, double v0, double v1, double v2, double v3, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_absdiff_scalar";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::absdiff(opencv_csharp_native::mat_value(src), scalar_from_values(v0, v1, v2, v3), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)v0; (void)v1; (void)v2; (void)v3; (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_bitwise_and(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst, const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_core_bitwise_and";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_bitwise_op(api_name, cv::bitwise_and, src1, src2, dst, mask);
#else
        (void)src1; (void)src2; (void)dst; (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_bitwise_or(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst, const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_core_bitwise_or";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_bitwise_op(api_name, cv::bitwise_or, src1, src2, dst, mask);
#else
        (void)src1; (void)src2; (void)dst; (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_bitwise_xor(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst, const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_core_bitwise_xor";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_bitwise_op(api_name, cv::bitwise_xor, src1, src2, dst, mask);
#else
        (void)src1; (void)src2; (void)dst; (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_bitwise_not(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_core_bitwise_not";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::bitwise_not(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), input_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)dst; (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_compare(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst, int cmpop)
{
    constexpr const char* api_name = "jyppx_ocv_core_compare";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src1, "src1") || !validate_mat(api_name, src2, "src2") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::compare(opencv_csharp_native::mat_value(src1), opencv_csharp_native::mat_value(src2), opencv_csharp_native::mat_value(dst), cmpop);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src1; (void)src2; (void)dst; (void)cmpop;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_min(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_min";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_binary_no_mask_op(api_name, cv::min, src1, src2, dst);
#else
        (void)src1; (void)src2; (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_max(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_max";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_binary_no_mask_op(api_name, cv::max, src1, src2, dst);
#else
        (void)src1; (void)src2; (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_in_range(const jyppx_ocv_mat* src, double lower_v0, double lower_v1, double lower_v2, double lower_v3, double upper_v0, double upper_v1, double upper_v2, double upper_v3, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_in_range";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::inRange(
            opencv_csharp_native::mat_value(src),
            scalar_from_values(lower_v0, lower_v1, lower_v2, lower_v3),
            scalar_from_values(upper_v0, upper_v1, upper_v2, upper_v3),
            opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)lower_v0; (void)lower_v1; (void)lower_v2; (void)lower_v3; (void)upper_v0; (void)upper_v1; (void)upper_v2; (void)upper_v3; (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_patch_nans(jyppx_ocv_mat* src, double value)
{
    constexpr const char* api_name = "jyppx_ocv_core_patch_nans";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_output_mat(api_name, src, "src"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::patchNaNs(opencv_csharp_native::mat_value(src), value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_count_non_zero(const jyppx_ocv_mat* src, int* out_count)
{
    constexpr const char* api_name = "jyppx_ocv_core_count_non_zero";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        *out_count = cv::countNonZero(opencv_csharp_native::mat_value(src));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        *out_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_mean(const jyppx_ocv_mat* src, const jyppx_ocv_mat* mask, double* out_values, int out_values_length)
{
    constexpr const char* api_name = "jyppx_ocv_core_mean";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_values == nullptr || out_values_length < 4)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_values");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        write_scalar(cv::mean(opencv_csharp_native::mat_value(src), input_or_no_array(mask)), out_values);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_mean_std_dev(const jyppx_ocv_mat* src, const jyppx_ocv_mat* mask, double* out_mean, int out_mean_length, double* out_stddev, int out_stddev_length)
{
    constexpr const char* api_name = "jyppx_ocv_core_mean_std_dev";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (!validate_scalar_buffer(api_name, out_mean, out_mean_length, "out_mean") ||
            !validate_scalar_buffer(api_name, out_stddev, out_stddev_length, "out_stddev"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::Scalar mean;
        cv::Scalar stddev;
        cv::meanStdDev(opencv_csharp_native::mat_value(src), mean, stddev, input_or_no_array(mask));
        write_scalar(mean, out_mean);
        write_scalar(stddev, out_stddev);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_min_max_loc(const jyppx_ocv_mat* src, const jyppx_ocv_mat* mask, double* out_min_val, double* out_max_val, int* out_min_x, int* out_min_y, int* out_max_x, int* out_max_y)
{
    constexpr const char* api_name = "jyppx_ocv_core_min_max_loc";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_min_val == nullptr || out_max_val == nullptr || out_min_x == nullptr || out_min_y == nullptr || out_max_x == nullptr || out_max_y == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::Point min_loc;
        cv::Point max_loc;
        cv::minMaxLoc(opencv_csharp_native::mat_value(src), out_min_val, out_max_val, &min_loc, &max_loc, input_or_no_array(mask));
        *out_min_x = min_loc.x;
        *out_min_y = min_loc.y;
        *out_max_x = max_loc.x;
        *out_max_y = max_loc.y;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_norm(const jyppx_ocv_mat* src1, int norm_type, const jyppx_ocv_mat* mask, double* out_value)
{
    constexpr const char* api_name = "jyppx_ocv_core_norm";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src1, "src1"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        *out_value = cv::norm(opencv_csharp_native::mat_value(src1), norm_type, input_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src1; (void)norm_type; (void)mask;
        *out_value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_norm_diff(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, int norm_type, const jyppx_ocv_mat* mask, double* out_value)
{
    constexpr const char* api_name = "jyppx_ocv_core_norm_diff";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src1, "src1") || !validate_mat(api_name, src2, "src2"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        *out_value = cv::norm(opencv_csharp_native::mat_value(src1), opencv_csharp_native::mat_value(src2), norm_type, input_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src1; (void)src2; (void)norm_type; (void)mask;
        *out_value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_normalize(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, double alpha, double beta, int norm_type, int dtype, const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_core_normalize";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::normalize(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), alpha, beta, norm_type, dtype, input_or_no_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)dst; (void)alpha; (void)beta; (void)norm_type; (void)dtype; (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_reduce(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int dim, int rtype, int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_core_reduce";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::reduce(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), dim, rtype, dtype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)dst; (void)dim; (void)rtype; (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_sum(const jyppx_ocv_mat* src, double* out_values, int out_values_length)
{
    constexpr const char* api_name = "jyppx_ocv_core_sum";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (!validate_scalar_buffer(api_name, out_values, out_values_length, "out_values"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        write_scalar(cv::sum(opencv_csharp_native::mat_value(src)), out_values);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_trace(const jyppx_ocv_mat* src, double* out_values, int out_values_length)
{
    constexpr const char* api_name = "jyppx_ocv_core_trace";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (!validate_scalar_buffer(api_name, out_values, out_values_length, "out_values"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        write_scalar(cv::trace(opencv_csharp_native::mat_value(src)), out_values);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_determinant(const jyppx_ocv_mat* src, double* out_value)
{
    constexpr const char* api_name = "jyppx_ocv_core_determinant";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        *out_value = cv::determinant(opencv_csharp_native::mat_value(src));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        *out_value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_invert(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int flags, double* out_value)
{
    constexpr const char* api_name = "jyppx_ocv_core_invert";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        *out_value = cv::invert(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)dst; (void)flags;
        *out_value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_solve(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, jyppx_ocv_mat* dst, int flags, int* out_success)
{
    constexpr const char* api_name = "jyppx_ocv_core_solve";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_success == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_success");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src1, "src1") || !validate_mat(api_name, src2, "src2") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        *out_success = cv::solve(opencv_csharp_native::mat_value(src1), opencv_csharp_native::mat_value(src2), opencv_csharp_native::mat_value(dst), flags) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src1; (void)src2; (void)dst; (void)flags;
        *out_success = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_mahalanobis(const jyppx_ocv_mat* v1, const jyppx_ocv_mat* v2, const jyppx_ocv_mat* icovar, double* out_value)
{
    constexpr const char* api_name = "jyppx_ocv_core_mahalanobis";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, v1, "v1") || !validate_mat(api_name, v2, "v2") || !validate_mat(api_name, icovar, "icovar"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        *out_value = cv::Mahalanobis(opencv_csharp_native::mat_value(v1), opencv_csharp_native::mat_value(v2), opencv_csharp_native::mat_value(icovar));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)v1; (void)v2; (void)icovar;
        *out_value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_split_count(const jyppx_ocv_mat* src, int* out_count)
{
    constexpr const char* api_name = "jyppx_ocv_core_split_count";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        *out_count = opencv_csharp_native::mat_value(src).channels();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        *out_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_split_fill(const jyppx_ocv_mat* src, jyppx_ocv_mat** dst, int dst_capacity, int* out_count)
{
    constexpr const char* api_name = "jyppx_ocv_core_split_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (dst == nullptr || out_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        if (dst_capacity < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst_capacity");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        std::vector<cv::Mat> channels;
        cv::split(opencv_csharp_native::mat_value(src), channels);
        *out_count = static_cast<int>(channels.size());
        if (dst_capacity < *out_count)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst_capacity");
        }

        for (int i = 0; i < *out_count; i++)
        {
            dst[i] = new (std::nothrow) jyppx_ocv_mat{ channels[static_cast<size_t>(i)] };
            if (dst[i] == nullptr)
            {
                for (int j = 0; j < i; j++)
                {
                    delete dst[j];
                    dst[j] = nullptr;
                }

                return opencv_csharp_native::set_out_of_memory(api_name);
            }
        }

        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        *out_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_merge(const jyppx_ocv_mat* const* src, int src_count, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_merge";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || src_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        std::vector<cv::Mat> mats;
        mats.reserve(static_cast<size_t>(src_count));
        for (int i = 0; i < src_count; i++)
        {
            if (src[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "src");
            }

            mats.push_back(opencv_csharp_native::mat_value(src[i]));
        }

        cv::merge(mats, opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_vconcat(const jyppx_ocv_mat* const* src, int src_count, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_vconcat";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || src_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        std::vector<cv::Mat> mats;
        mats.reserve(static_cast<size_t>(src_count));
        for (int i = 0; i < src_count; i++)
        {
            if (src[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "src");
            }

            mats.push_back(opencv_csharp_native::mat_value(src[i]));
        }

        cv::vconcat(mats, opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_hconcat(const jyppx_ocv_mat* const* src, int src_count, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_hconcat";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || src_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        std::vector<cv::Mat> mats;
        mats.reserve(static_cast<size_t>(src_count));
        for (int i = 0; i < src_count; i++)
        {
            if (src[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "src");
            }

            mats.push_back(opencv_csharp_native::mat_value(src[i]));
        }

        cv::hconcat(mats, opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_extract_channel(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int coi)
{
    constexpr const char* api_name = "jyppx_ocv_core_extract_channel";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::extractChannel(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), coi);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)dst; (void)coi;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_kmeans(
    const jyppx_ocv_mat* data,
    int k,
    jyppx_ocv_mat* best_labels,
    int criteria_type,
    int criteria_max_count,
    double criteria_epsilon,
    int attempts,
    int flags,
    jyppx_ocv_mat* centers,
    double* compactness)
{
    constexpr const char* api_name = "jyppx_ocv_core_kmeans";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (compactness == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "compactness");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, data, "data") ||
            !validate_output_mat(api_name, best_labels, "best_labels") ||
            !validate_output_mat(api_name, centers, "centers"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        if (k <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "k");
        }

        if (attempts <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "attempts");
        }

        cv::TermCriteria criteria(criteria_type, criteria_max_count, criteria_epsilon);
        *compactness = cv::kmeans(
            opencv_csharp_native::mat_value(data),
            k,
            opencv_csharp_native::mat_value(best_labels),
            criteria,
            attempts,
            flags,
            opencv_csharp_native::mat_value(centers));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)data; (void)k; (void)best_labels; (void)criteria_type; (void)criteria_max_count; (void)criteria_epsilon; (void)attempts; (void)flags; (void)centers;
        *compactness = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_insert_channel(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int coi)
{
    constexpr const char* api_name = "jyppx_ocv_core_insert_channel";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::insertChannel(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), coi);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)dst; (void)coi;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_mix_channels(const jyppx_ocv_mat* const* src, int src_count, jyppx_ocv_mat** dst, int dst_count, const int* from_to, int pair_count)
{
    constexpr const char* api_name = "jyppx_ocv_core_mix_channels";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (src == nullptr || src_count <= 0 || dst == nullptr || dst_count <= 0 || from_to == nullptr || pair_count <= 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "buffers");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::vector<cv::Mat> src_mats;
        std::vector<cv::Mat> dst_mats;
        src_mats.reserve(static_cast<size_t>(src_count));
        dst_mats.reserve(static_cast<size_t>(dst_count));
        for (int i = 0; i < src_count; i++)
        {
            if (src[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "src");
            }

            src_mats.push_back(opencv_csharp_native::mat_value(src[i]));
        }

        for (int i = 0; i < dst_count; i++)
        {
            if (dst[i] == nullptr)
            {
                return opencv_csharp_native::set_invalid_argument(api_name, "dst");
            }

            dst_mats.push_back(opencv_csharp_native::mat_value(dst[i]));
        }

        cv::mixChannels(src_mats, dst_mats, from_to, pair_count);
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

int jyppx_ocv_core_repeat(const jyppx_ocv_mat* src, int ny, int nx, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_repeat";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::repeat(opencv_csharp_native::mat_value(src), ny, nx, opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)ny; (void)nx; (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_flip(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int flip_code)
{
    constexpr const char* api_name = "jyppx_ocv_core_flip";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::flip(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), flip_code);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)dst; (void)flip_code;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rotate(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int rotate_code)
{
    constexpr const char* api_name = "jyppx_ocv_core_rotate";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::rotate(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), rotate_code);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)dst; (void)rotate_code;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_transpose(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_transpose";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::transpose(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_lut(const jyppx_ocv_mat* src, const jyppx_ocv_mat* lut, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_lut";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_mat(api_name, lut, "lut") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::LUT(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(lut), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)lut; (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_convert_scale_abs(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, double alpha, double beta)
{
    constexpr const char* api_name = "jyppx_ocv_core_convert_scale_abs";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_mat(api_name, src, "src") || !validate_output_mat(api_name, dst, "dst"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::convertScaleAbs(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), alpha, beta);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src; (void)dst; (void)alpha; (void)beta;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_complete_symm(jyppx_ocv_mat* mat, int lower_to_upper)
{
    constexpr const char* api_name = "jyppx_ocv_core_complete_symm";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_output_mat(api_name, mat, "mat"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::completeSymm(opencv_csharp_native::mat_value(mat), lower_to_upper != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mat; (void)lower_to_upper;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_set_identity(jyppx_ocv_mat* mat, double v0, double v1, double v2, double v3)
{
    constexpr const char* api_name = "jyppx_ocv_core_set_identity";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (!validate_output_mat(api_name, mat, "mat"))
        {
            return OPENCV_CSHARP_STATUS_INVALID_ARGUMENT;
        }

        cv::setIdentity(opencv_csharp_native::mat_value(mat), scalar_from_values(v0, v1, v2, v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mat; (void)v0; (void)v1; (void)v2; (void)v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

