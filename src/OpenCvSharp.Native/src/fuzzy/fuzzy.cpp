#include "open_cv_sharp/fuzzy/fuzzy.h"

#include "../core/mat_handle.h"
#include "../error_state.h"

namespace
{
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

    int validate_matrix_kernel_output(const char* api_name, const jyppx_ocv_mat* matrix, const jyppx_ocv_mat* kernel, jyppx_ocv_mat* output)
    {
        int status = validate_mat(api_name, matrix, "matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, kernel, "kernel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        return validate_mat(api_name, output, "output");
    }
}

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
#include <opencv2/fuzzy.hpp>

namespace
{
    cv::InputArray optional_input_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::OutputArray optional_output_array(jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::OutputArray(opencv_csharp_native::mat_value(mat));
    }
}
#endif

int jyppx_ocv_fuzzy_create_kernel_from_functions(
    const jyppx_ocv_mat* function_x,
    const jyppx_ocv_mat* function_y,
    jyppx_ocv_mat* kernel,
    int channels)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_create_kernel_from_functions";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, function_x, "function_x");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, function_y, "function_y");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, kernel, "kernel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::createKernel(opencv_csharp_native::mat_value(function_x), opencv_csharp_native::mat_value(function_y), opencv_csharp_native::mat_value(kernel), channels);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)channels;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_create_kernel(int function_type, int radius, jyppx_ocv_mat* kernel, int channels)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_create_kernel";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, kernel, "kernel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::createKernel(function_type, radius, opencv_csharp_native::mat_value(kernel), channels);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)function_type; (void)radius; (void)channels;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_inpaint(
    const jyppx_ocv_mat* image,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_mat* output,
    int radius,
    int function_type,
    int algorithm)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_inpaint";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, image, "image");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::inpaint(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(mask), opencv_csharp_native::mat_value(output), radius, function_type, algorithm);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)radius; (void)function_type; (void)algorithm;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_filter(const jyppx_ocv_mat* image, const jyppx_ocv_mat* kernel, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_filter";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_matrix_kernel_output(api_name, image, kernel, output);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::filter(opencv_csharp_native::mat_value(image), opencv_csharp_native::mat_value(kernel), opencv_csharp_native::mat_value(output));
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

int jyppx_ocv_fuzzy_ft02d_components(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* components,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft02d_components";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_matrix_kernel_output(api_name, matrix, kernel, components);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT02D_components(opencv_csharp_native::mat_value(matrix), opencv_csharp_native::mat_value(kernel), opencv_csharp_native::mat_value(components), optional_input_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_ft02d_inverse_ft(
    const jyppx_ocv_mat* components,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output,
    int width,
    int height)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft02d_inverse_ft";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_matrix_kernel_output(api_name, components, kernel, output);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT02D_inverseFT(opencv_csharp_native::mat_value(components), opencv_csharp_native::mat_value(kernel), opencv_csharp_native::mat_value(output), width, height);
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

int jyppx_ocv_fuzzy_ft02d_process(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft02d_process";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_matrix_kernel_output(api_name, matrix, kernel, output);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT02D_process(opencv_csharp_native::mat_value(matrix), opencv_csharp_native::mat_value(kernel), opencv_csharp_native::mat_value(output), optional_input_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_ft02d_iteration(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output,
    const jyppx_ocv_mat* mask,
    jyppx_ocv_mat* mask_output,
    int first_stop,
    int* state)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft02d_iteration";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_matrix_kernel_output(api_name, matrix, kernel, output);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mask, "mask");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, state, "state");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        *state = cv::ft::FT02D_iteration(
            opencv_csharp_native::mat_value(matrix),
            opencv_csharp_native::mat_value(kernel),
            opencv_csharp_native::mat_value(output),
            opencv_csharp_native::mat_value(mask),
            optional_output_array(mask_output),
            first_stop != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask_output; (void)first_stop;
        *state = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_ft02d_fl_process(const jyppx_ocv_mat* matrix, int radius, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft02d_fl_process";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, matrix, "matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT02D_FL_process(opencv_csharp_native::mat_value(matrix), radius, opencv_csharp_native::mat_value(output));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)radius;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_ft02d_fl_process_float(const jyppx_ocv_mat* matrix, int radius, jyppx_ocv_mat* output)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft02d_fl_process_float";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, matrix, "matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, output, "output");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT02D_FL_process_float(opencv_csharp_native::mat_value(matrix), radius, opencv_csharp_native::mat_value(output));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)radius;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_ft12d_components(const jyppx_ocv_mat* matrix, const jyppx_ocv_mat* kernel, jyppx_ocv_mat* components)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft12d_components";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_matrix_kernel_output(api_name, matrix, kernel, components);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT12D_components(opencv_csharp_native::mat_value(matrix), opencv_csharp_native::mat_value(kernel), opencv_csharp_native::mat_value(components));
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

int jyppx_ocv_fuzzy_ft12d_polynomial(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* c00,
    jyppx_ocv_mat* c10,
    jyppx_ocv_mat* c01,
    jyppx_ocv_mat* components,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft12d_polynomial";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, matrix, "matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, kernel, "kernel");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, c00, "c00");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, c10, "c10");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, c01, "c01");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, components, "components");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT12D_polynomial(
            opencv_csharp_native::mat_value(matrix),
            opencv_csharp_native::mat_value(kernel),
            opencv_csharp_native::mat_value(c00),
            opencv_csharp_native::mat_value(c10),
            opencv_csharp_native::mat_value(c01),
            opencv_csharp_native::mat_value(components),
            optional_input_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_vertical(int radius, jyppx_ocv_mat* matrix, int channels)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_vertical";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, matrix, "matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT12D_createPolynomMatrixVertical(radius, opencv_csharp_native::mat_value(matrix), channels);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)radius; (void)channels;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_horizontal(int radius, jyppx_ocv_mat* matrix, int channels)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft12d_create_polynom_matrix_horizontal";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, matrix, "matrix");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT12D_createPolynomMatrixHorizontal(radius, opencv_csharp_native::mat_value(matrix), channels);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)radius; (void)channels;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_fuzzy_ft12d_inverse_ft(
    const jyppx_ocv_mat* components,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output,
    int width,
    int height)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft12d_inverse_ft";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_matrix_kernel_output(api_name, components, kernel, output);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT12D_inverseFT(opencv_csharp_native::mat_value(components), opencv_csharp_native::mat_value(kernel), opencv_csharp_native::mat_value(output), width, height);
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

int jyppx_ocv_fuzzy_ft12d_process(
    const jyppx_ocv_mat* matrix,
    const jyppx_ocv_mat* kernel,
    jyppx_ocv_mat* output,
    const jyppx_ocv_mat* mask)
{
    constexpr const char* api_name = "jyppx_ocv_fuzzy_ft12d_process";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_matrix_kernel_output(api_name, matrix, kernel, output);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_FUZZY)
        cv::ft::FT12D_process(opencv_csharp_native::mat_value(matrix), opencv_csharp_native::mat_value(kernel), opencv_csharp_native::mat_value(output), optional_input_array(mask));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)mask;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}


