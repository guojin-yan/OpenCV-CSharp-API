#include "open_cv_sharp/core/decomp.h"

#include "mat_handle.h"
#include "../error_state.h"

#include <new>

struct jyppx_ocv_svd
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::SVD value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_rng
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::RNG value;
#else
    int placeholder;
#endif
};

namespace
{
#if defined(OPENCV_CSHARP_HAS_OPENCV)
    cv::InputArray input_or_no_array(const jyppx_ocv_mat* mat)
    {
        return mat == nullptr ? cv::noArray() : cv::InputArray(opencv_csharp_native::mat_value(mat));
    }

    cv::Scalar scalar_from_values(double v0, double v1, double v2, double v3)
    {
        return cv::Scalar(v0, v1, v2, v3);
    }

    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* parameter_name)
    {
        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_mat(const char* api_name, jyppx_ocv_mat* mat, const char* parameter_name)
    {
        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, parameter_name);
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_svd(const char* api_name, const jyppx_ocv_svd* svd)
    {
        if (svd == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "svd");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int validate_rng(const char* api_name, const jyppx_ocv_rng* rng)
    {
        if (rng == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rng");
        }

        return OPENCV_CSHARP_STATUS_OK;
    }

    int clone_mat_to_handle(const char* api_name, const cv::Mat& value, jyppx_ocv_mat** dst)
    {
        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

        auto handle = new (std::nothrow) jyppx_ocv_mat{ value.clone() };
        if (handle == nullptr)
        {
            *dst = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *dst = handle;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int run_unary_array_op(
        const char* api_name,
        void (*operation)(cv::InputArray, cv::OutputArray),
        const jyppx_ocv_mat* src,
        jyppx_ocv_mat* dst)
    {
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        operation(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_core_svd_create_empty(jyppx_ocv_svd** svd)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_create_empty";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (svd == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "svd");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *svd = new (std::nothrow) jyppx_ocv_svd{};
        return *svd == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        *svd = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_svd_create(const jyppx_ocv_mat* src, int flags, jyppx_ocv_svd** svd)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_create";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (svd == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "svd");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            *svd = nullptr;
            return status;
        }

        auto result = new (std::nothrow) jyppx_ocv_svd{ cv::SVD(opencv_csharp_native::mat_value(src), flags) };
        if (result == nullptr)
        {
            *svd = nullptr;
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        *svd = result;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)flags;
        *svd = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        if (svd != nullptr)
        {
            *svd = nullptr;
        }

        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_core_svd_release(jyppx_ocv_svd* svd)
{
    try
    {
        delete svd;
    }
    catch (...)
    {
    }
}

int jyppx_ocv_core_svd_compute(jyppx_ocv_svd* svd, const jyppx_ocv_mat* src, int flags)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_svd(api_name, svd);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        svd->value(opencv_csharp_native::mat_value(src), flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)svd;
        (void)src;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_svd_get_w(const jyppx_ocv_svd* svd, jyppx_ocv_mat** dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_get_w";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_svd(api_name, svd);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return clone_mat_to_handle(api_name, svd->value.w, dst);
#else
        (void)svd;
        if (dst != nullptr)
        {
            *dst = nullptr;
        }

        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_svd_get_u(const jyppx_ocv_svd* svd, jyppx_ocv_mat** dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_get_u";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_svd(api_name, svd);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return clone_mat_to_handle(api_name, svd->value.u, dst);
#else
        (void)svd;
        if (dst != nullptr)
        {
            *dst = nullptr;
        }

        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_svd_get_vt(const jyppx_ocv_svd* svd, jyppx_ocv_mat** dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_get_vt";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_svd(api_name, svd);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        return clone_mat_to_handle(api_name, svd->value.vt, dst);
#else
        (void)svd;
        if (dst != nullptr)
        {
            *dst = nullptr;
        }

        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_svd_back_subst(const jyppx_ocv_svd* svd, const jyppx_ocv_mat* rhs, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_back_subst";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_svd(api_name, svd);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, rhs, "rhs");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        svd->value.backSubst(opencv_csharp_native::mat_value(rhs), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)svd;
        (void)rhs;
        (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_svd_static_compute(const jyppx_ocv_mat* src, jyppx_ocv_mat* w, jyppx_ocv_mat* u, jyppx_ocv_mat* vt, int flags)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_static_compute";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, w, "w");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, u, "u");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, vt, "vt");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::SVD::compute(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(w), opencv_csharp_native::mat_value(u), opencv_csharp_native::mat_value(vt), flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)w;
        (void)u;
        (void)vt;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_svd_static_compute_values(const jyppx_ocv_mat* src, jyppx_ocv_mat* w, int flags)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_static_compute_values";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, w, "w");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::SVD::compute(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(w), flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)w;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_svd_static_back_subst(const jyppx_ocv_mat* w, const jyppx_ocv_mat* u, const jyppx_ocv_mat* vt, const jyppx_ocv_mat* rhs, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_static_back_subst";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, w, "w");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, u, "u");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, vt, "vt");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, rhs, "rhs");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::SVD::backSubst(
            opencv_csharp_native::mat_value(w),
            opencv_csharp_native::mat_value(u),
            opencv_csharp_native::mat_value(vt),
            opencv_csharp_native::mat_value(rhs),
            opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)w;
        (void)u;
        (void)vt;
        (void)rhs;
        (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_svd_solve_z(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_svd_solve_z";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::SVD::solveZ(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rng_create_default(jyppx_ocv_rng** rng)
{
    constexpr const char* api_name = "jyppx_ocv_core_rng_create_default";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (rng == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rng");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *rng = new (std::nothrow) jyppx_ocv_rng{};
        return *rng == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        *rng = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rng_create(uint64_t state, jyppx_ocv_rng** rng)
{
    constexpr const char* api_name = "jyppx_ocv_core_rng_create";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (rng == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rng");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *rng = new (std::nothrow) jyppx_ocv_rng{ cv::RNG(state) };
        return *rng == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        (void)state;
        *rng = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_core_rng_release(jyppx_ocv_rng* rng)
{
    try
    {
        delete rng;
    }
    catch (...)
    {
    }
}

int jyppx_ocv_core_rng_get_state(const jyppx_ocv_rng* rng, uint64_t* state)
{
    constexpr const char* api_name = "jyppx_ocv_core_rng_get_state";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (state == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "state");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_rng(api_name, rng);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *state = rng->value.state;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rng;
        *state = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rng_set_state(jyppx_ocv_rng* rng, uint64_t state)
{
    constexpr const char* api_name = "jyppx_ocv_core_rng_set_state";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_rng(api_name, rng);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        rng->value.state = state;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rng;
        (void)state;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rng_next(jyppx_ocv_rng* rng, uint32_t* value)
{
    constexpr const char* api_name = "jyppx_ocv_core_rng_next";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_rng(api_name, rng);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = rng->value.next();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rng;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rng_uniform_int(jyppx_ocv_rng* rng, int a, int b, int* value)
{
    constexpr const char* api_name = "jyppx_ocv_core_rng_uniform_int";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_rng(api_name, rng);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = rng->value.uniform(a, b);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rng;
        (void)a;
        (void)b;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rng_uniform_float(jyppx_ocv_rng* rng, float a, float b, float* value)
{
    constexpr const char* api_name = "jyppx_ocv_core_rng_uniform_float";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_rng(api_name, rng);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = rng->value.uniform(a, b);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rng;
        (void)a;
        (void)b;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rng_uniform_double(jyppx_ocv_rng* rng, double a, double b, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_core_rng_uniform_double";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_rng(api_name, rng);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = rng->value.uniform(a, b);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rng;
        (void)a;
        (void)b;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rng_gaussian(jyppx_ocv_rng* rng, double sigma, double* value)
{
    constexpr const char* api_name = "jyppx_ocv_core_rng_gaussian";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (value == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "value");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_rng(api_name, rng);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *value = rng->value.gaussian(sigma);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rng;
        (void)sigma;
        *value = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rng_fill(
    jyppx_ocv_rng* rng,
    jyppx_ocv_mat* mat,
    int dist_type,
    double a_v0,
    double a_v1,
    double a_v2,
    double a_v3,
    double b_v0,
    double b_v1,
    double b_v2,
    double b_v3,
    int saturate_range)
{
    constexpr const char* api_name = "jyppx_ocv_core_rng_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_rng(api_name, rng);
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, mat, "mat");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        rng->value.fill(
            opencv_csharp_native::mat_value(mat),
            dist_type,
            scalar_from_values(a_v0, a_v1, a_v2, a_v3),
            scalar_from_values(b_v0, b_v1, b_v2, b_v3),
            saturate_range != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rng;
        (void)mat;
        (void)dist_type;
        (void)a_v0;
        (void)a_v1;
        (void)a_v2;
        (void)a_v3;
        (void)b_v0;
        (void)b_v1;
        (void)b_v2;
        (void)b_v3;
        (void)saturate_range;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_gemm(const jyppx_ocv_mat* src1, const jyppx_ocv_mat* src2, double alpha, const jyppx_ocv_mat* src3, double beta, jyppx_ocv_mat* dst, int flags)
{
    constexpr const char* api_name = "jyppx_ocv_core_gemm";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src1, "src1");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, src2, "src2");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::gemm(
            opencv_csharp_native::mat_value(src1),
            opencv_csharp_native::mat_value(src2),
            alpha,
            input_or_no_array(src3),
            beta,
            opencv_csharp_native::mat_value(dst),
            flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src1;
        (void)src2;
        (void)alpha;
        (void)src3;
        (void)beta;
        (void)dst;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_mul_transposed(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int a_ta, const jyppx_ocv_mat* delta, double scale, int dtype)
{
    constexpr const char* api_name = "jyppx_ocv_core_mul_transposed";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::mulTransposed(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(dst),
            a_ta != 0,
            input_or_no_array(delta),
            scale,
            dtype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)dst;
        (void)a_ta;
        (void)delta;
        (void)scale;
        (void)dtype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_transform(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, const jyppx_ocv_mat* m)
{
    constexpr const char* api_name = "jyppx_ocv_core_transform";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, m, "m");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::transform(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), opencv_csharp_native::mat_value(m));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)dst;
        (void)m;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_perspective_transform(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, const jyppx_ocv_mat* m)
{
    constexpr const char* api_name = "jyppx_ocv_core_perspective_transform";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, m, "m");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::perspectiveTransform(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), opencv_csharp_native::mat_value(m));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)dst;
        (void)m;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_magnitude(const jyppx_ocv_mat* x, const jyppx_ocv_mat* y, jyppx_ocv_mat* magnitude)
{
    constexpr const char* api_name = "jyppx_ocv_core_magnitude";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, x, "x");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, y, "y");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, magnitude, "magnitude");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::magnitude(opencv_csharp_native::mat_value(x), opencv_csharp_native::mat_value(y), opencv_csharp_native::mat_value(magnitude));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x;
        (void)y;
        (void)magnitude;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_phase(const jyppx_ocv_mat* x, const jyppx_ocv_mat* y, jyppx_ocv_mat* angle, int angle_in_degrees)
{
    constexpr const char* api_name = "jyppx_ocv_core_phase";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, x, "x");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, y, "y");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, angle, "angle");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::phase(opencv_csharp_native::mat_value(x), opencv_csharp_native::mat_value(y), opencv_csharp_native::mat_value(angle), angle_in_degrees != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x;
        (void)y;
        (void)angle;
        (void)angle_in_degrees;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_cart_to_polar(const jyppx_ocv_mat* x, const jyppx_ocv_mat* y, jyppx_ocv_mat* magnitude, jyppx_ocv_mat* angle, int angle_in_degrees)
{
    constexpr const char* api_name = "jyppx_ocv_core_cart_to_polar";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, x, "x");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, y, "y");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, magnitude, "magnitude");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, angle, "angle");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::cartToPolar(
            opencv_csharp_native::mat_value(x),
            opencv_csharp_native::mat_value(y),
            opencv_csharp_native::mat_value(magnitude),
            opencv_csharp_native::mat_value(angle),
            angle_in_degrees != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x;
        (void)y;
        (void)magnitude;
        (void)angle;
        (void)angle_in_degrees;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_polar_to_cart(const jyppx_ocv_mat* magnitude, const jyppx_ocv_mat* angle, jyppx_ocv_mat* x, jyppx_ocv_mat* y, int angle_in_degrees)
{
    constexpr const char* api_name = "jyppx_ocv_core_polar_to_cart";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, magnitude, "magnitude");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, angle, "angle");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, x, "x");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, y, "y");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::polarToCart(
            opencv_csharp_native::mat_value(magnitude),
            opencv_csharp_native::mat_value(angle),
            opencv_csharp_native::mat_value(x),
            opencv_csharp_native::mat_value(y),
            angle_in_degrees != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)magnitude;
        (void)angle;
        (void)x;
        (void)y;
        (void)angle_in_degrees;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_dft(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int flags, int nonzero_rows)
{
    constexpr const char* api_name = "jyppx_ocv_core_dft";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::dft(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), flags, nonzero_rows);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)dst;
        (void)flags;
        (void)nonzero_rows;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_idft(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int flags, int nonzero_rows)
{
    constexpr const char* api_name = "jyppx_ocv_core_idft";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::idft(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), flags, nonzero_rows);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)dst;
        (void)flags;
        (void)nonzero_rows;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_dct(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int flags)
{
    constexpr const char* api_name = "jyppx_ocv_core_dct";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::dct(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)dst;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_idct(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int flags)
{
    constexpr const char* api_name = "jyppx_ocv_core_idct";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::idct(opencv_csharp_native::mat_value(src), opencv_csharp_native::mat_value(dst), flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)dst;
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_mul_spectrums(const jyppx_ocv_mat* a, const jyppx_ocv_mat* b, jyppx_ocv_mat* c, int flags, int conj_b)
{
    constexpr const char* api_name = "jyppx_ocv_core_mul_spectrums";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, a, "a");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, b, "b");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, c, "c");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::mulSpectrums(opencv_csharp_native::mat_value(a), opencv_csharp_native::mat_value(b), opencv_csharp_native::mat_value(c), flags, conj_b != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)a;
        (void)b;
        (void)c;
        (void)flags;
        (void)conj_b;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_div_spectrums(const jyppx_ocv_mat* a, const jyppx_ocv_mat* b, jyppx_ocv_mat* c, int flags, int conj_b)
{
    constexpr const char* api_name = "jyppx_ocv_core_div_spectrums";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, a, "a");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_mat(api_name, b, "b");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, c, "c");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::divSpectrums(opencv_csharp_native::mat_value(a), opencv_csharp_native::mat_value(b), opencv_csharp_native::mat_value(c), flags, conj_b != 0);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)a;
        (void)b;
        (void)c;
        (void)flags;
        (void)conj_b;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_get_optimal_dft_size(int vec_size, int* out_size)
{
    constexpr const char* api_name = "jyppx_ocv_core_get_optimal_dft_size";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_size == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_size");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_size = cv::getOptimalDFTSize(vec_size);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)vec_size;
        *out_size = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_eigen(const jyppx_ocv_mat* src, jyppx_ocv_mat* eigenvalues, jyppx_ocv_mat* eigenvectors, int* out_success)
{
    constexpr const char* api_name = "jyppx_ocv_core_eigen";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_success == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_success");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, eigenvalues, "eigenvalues");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, eigenvectors, "eigenvectors");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *out_success = cv::eigen(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(eigenvalues),
            opencv_csharp_native::mat_value(eigenvectors)) ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)eigenvalues;
        (void)eigenvectors;
        *out_success = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_eigen_non_symmetric(const jyppx_ocv_mat* src, jyppx_ocv_mat* eigenvalues, jyppx_ocv_mat* eigenvectors)
{
    constexpr const char* api_name = "jyppx_ocv_core_eigen_non_symmetric";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, eigenvalues, "eigenvalues");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, eigenvectors, "eigenvectors");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::eigenNonSymmetric(
            opencv_csharp_native::mat_value(src),
            opencv_csharp_native::mat_value(eigenvalues),
            opencv_csharp_native::mat_value(eigenvectors));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)eigenvalues;
        (void)eigenvectors;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_solve_cubic(const jyppx_ocv_mat* coeffs, jyppx_ocv_mat* roots, int* out_root_count)
{
    constexpr const char* api_name = "jyppx_ocv_core_solve_cubic";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_root_count == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_root_count");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, coeffs, "coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, roots, "roots");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *out_root_count = cv::solveCubic(opencv_csharp_native::mat_value(coeffs), opencv_csharp_native::mat_value(roots));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)coeffs;
        (void)roots;
        *out_root_count = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_solve_poly(const jyppx_ocv_mat* coeffs, jyppx_ocv_mat* roots, int max_iters, double* out_error)
{
    constexpr const char* api_name = "jyppx_ocv_core_solve_poly";
    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_error == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_error");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, coeffs, "coeffs");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, roots, "roots");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        *out_error = cv::solvePoly(opencv_csharp_native::mat_value(coeffs), opencv_csharp_native::mat_value(roots), max_iters);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)coeffs;
        (void)roots;
        (void)max_iters;
        *out_error = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_exp(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_exp";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_unary_array_op(api_name, cv::exp, src, dst);
#else
        (void)src;
        (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_log(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_log";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_unary_array_op(api_name, cv::log, src, dst);
#else
        (void)src;
        (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_sqrt(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_sqrt";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return run_unary_array_op(api_name, cv::sqrt, src, dst);
#else
        (void)src;
        (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_pow(const jyppx_ocv_mat* src, double power, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_core_pow";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, src, "src");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK)
        {
            return status;
        }

        cv::pow(opencv_csharp_native::mat_value(src), power, opencv_csharp_native::mat_value(dst));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)src;
        (void)power;
        (void)dst;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_calc_covar_matrix(
    const jyppx_ocv_mat* samples,
    jyppx_ocv_mat* covar,
    jyppx_ocv_mat* mean,
    int flags,
    int ctype)
{
    constexpr const char* api_name = "jyppx_ocv_core_calc_covar_matrix";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, samples, "samples");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_mat(api_name, covar, "covar");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_mat(api_name, mean, "mean");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::calcCovarMatrix(
            opencv_csharp_native::mat_value(samples),
            opencv_csharp_native::mat_value(covar),
            opencv_csharp_native::mat_value(mean),
            flags,
            ctype);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)samples; (void)covar; (void)mean; (void)flags; (void)ctype;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_pca_compute_max_components(
    const jyppx_ocv_mat* data,
    jyppx_ocv_mat* mean,
    jyppx_ocv_mat* eigenvectors,
    jyppx_ocv_mat* eigenvalues,
    int max_components)
{
    constexpr const char* api_name = "jyppx_ocv_core_pca_compute_max_components";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, data, "data");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_mat(api_name, mean, "mean");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_mat(api_name, eigenvectors, "eigenvectors");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (eigenvalues == nullptr)
        {
            cv::PCACompute(
                opencv_csharp_native::mat_value(data),
                opencv_csharp_native::mat_value(mean),
                opencv_csharp_native::mat_value(eigenvectors),
                max_components);
        }
        else
        {
            cv::PCACompute(
                opencv_csharp_native::mat_value(data),
                opencv_csharp_native::mat_value(mean),
                opencv_csharp_native::mat_value(eigenvectors),
                opencv_csharp_native::mat_value(eigenvalues),
                max_components);
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)data; (void)mean; (void)eigenvectors; (void)eigenvalues; (void)max_components;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_pca_compute_retained_variance(
    const jyppx_ocv_mat* data,
    jyppx_ocv_mat* mean,
    jyppx_ocv_mat* eigenvectors,
    jyppx_ocv_mat* eigenvalues,
    double retained_variance)
{
    constexpr const char* api_name = "jyppx_ocv_core_pca_compute_retained_variance";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, data, "data");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_mat(api_name, mean, "mean");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_mat(api_name, eigenvectors, "eigenvectors");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (eigenvalues == nullptr)
        {
            cv::PCACompute(
                opencv_csharp_native::mat_value(data),
                opencv_csharp_native::mat_value(mean),
                opencv_csharp_native::mat_value(eigenvectors),
                retained_variance);
        }
        else
        {
            cv::PCACompute(
                opencv_csharp_native::mat_value(data),
                opencv_csharp_native::mat_value(mean),
                opencv_csharp_native::mat_value(eigenvectors),
                opencv_csharp_native::mat_value(eigenvalues),
                retained_variance);
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)data; (void)mean; (void)eigenvectors; (void)eigenvalues; (void)retained_variance;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

namespace
{
    int run_pca_projection(
        const char* api_name,
        bool back_project,
        const jyppx_ocv_mat* data,
        const jyppx_ocv_mat* mean,
        const jyppx_ocv_mat* eigenvectors,
        jyppx_ocv_mat* result)
    {
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, data, "data");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, mean, "mean");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, eigenvectors, "eigenvectors");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_mat(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (back_project)
        {
            cv::PCABackProject(
                opencv_csharp_native::mat_value(data),
                opencv_csharp_native::mat_value(mean),
                opencv_csharp_native::mat_value(eigenvectors),
                opencv_csharp_native::mat_value(result));
        }
        else
        {
            cv::PCAProject(
                opencv_csharp_native::mat_value(data),
                opencv_csharp_native::mat_value(mean),
                opencv_csharp_native::mat_value(eigenvectors),
                opencv_csharp_native::mat_value(result));
        }
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)back_project; (void)data; (void)mean; (void)eigenvectors; (void)result;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
}

int jyppx_ocv_core_pca_project(
    const jyppx_ocv_mat* data,
    const jyppx_ocv_mat* mean,
    const jyppx_ocv_mat* eigenvectors,
    jyppx_ocv_mat* result)
{
    constexpr const char* api_name = "jyppx_ocv_core_pca_project";
    try
    {
        opencv_csharp_native::clear_last_error();
        return run_pca_projection(api_name, false, data, mean, eigenvectors, result);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_pca_back_project(
    const jyppx_ocv_mat* data,
    const jyppx_ocv_mat* mean,
    const jyppx_ocv_mat* eigenvectors,
    jyppx_ocv_mat* result)
{
    constexpr const char* api_name = "jyppx_ocv_core_pca_back_project";
    try
    {
        opencv_csharp_native::clear_last_error();
        return run_pca_projection(api_name, true, data, mean, eigenvectors, result);
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_set_rng_seed(int seed)
{
    constexpr const char* api_name = "jyppx_ocv_core_set_rng_seed";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setRNGSeed(seed);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)seed;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_randu_mat(
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* low,
    const jyppx_ocv_mat* high)
{
    constexpr const char* api_name = "jyppx_ocv_core_randu_mat";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, low, "low");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, high, "high");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::randu(opencv_csharp_native::mat_value(dst), opencv_csharp_native::mat_value(low), opencv_csharp_native::mat_value(high));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dst; (void)low; (void)high;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_randu_scalar(
    jyppx_ocv_mat* dst,
    double low_v0,
    double low_v1,
    double low_v2,
    double low_v3,
    double high_v0,
    double high_v1,
    double high_v2,
    double high_v3)
{
    constexpr const char* api_name = "jyppx_ocv_core_randu_scalar";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::randu(
            opencv_csharp_native::mat_value(dst),
            scalar_from_values(low_v0, low_v1, low_v2, low_v3),
            scalar_from_values(high_v0, high_v1, high_v2, high_v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dst; (void)low_v0; (void)low_v1; (void)low_v2; (void)low_v3;
        (void)high_v0; (void)high_v1; (void)high_v2; (void)high_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_randn_mat(
    jyppx_ocv_mat* dst,
    const jyppx_ocv_mat* mean,
    const jyppx_ocv_mat* stddev)
{
    constexpr const char* api_name = "jyppx_ocv_core_randn_mat";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, mean, "mean");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, stddev, "stddev");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::randn(opencv_csharp_native::mat_value(dst), opencv_csharp_native::mat_value(mean), opencv_csharp_native::mat_value(stddev));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dst; (void)mean; (void)stddev;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_randn_scalar(
    jyppx_ocv_mat* dst,
    double mean_v0,
    double mean_v1,
    double mean_v2,
    double mean_v3,
    double stddev_v0,
    double stddev_v1,
    double stddev_v2,
    double stddev_v3)
{
    constexpr const char* api_name = "jyppx_ocv_core_randn_scalar";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::randn(
            opencv_csharp_native::mat_value(dst),
            scalar_from_values(mean_v0, mean_v1, mean_v2, mean_v3),
            scalar_from_values(stddev_v0, stddev_v1, stddev_v2, stddev_v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dst; (void)mean_v0; (void)mean_v1; (void)mean_v2; (void)mean_v3;
        (void)stddev_v0; (void)stddev_v1; (void)stddev_v2; (void)stddev_v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_rand_shuffle(jyppx_ocv_mat* dst, double iter_factor, jyppx_ocv_rng* rng)
{
    constexpr const char* api_name = "jyppx_ocv_core_rand_shuffle";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_output_mat(api_name, dst, "dst");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        cv::randShuffle(opencv_csharp_native::mat_value(dst), iter_factor, rng == nullptr ? nullptr : &rng->value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)dst; (void)iter_factor; (void)rng;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_core_solve_lp(
    const jyppx_ocv_mat* objective,
    const jyppx_ocv_mat* constraints,
    jyppx_ocv_mat* solution,
    double constraint_epsilon,
    int* out_result)
{
    constexpr const char* api_name = "jyppx_ocv_core_solve_lp";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (out_result == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_result");
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int status = validate_mat(api_name, objective, "objective");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_mat(api_name, constraints, "constraints");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = validate_output_mat(api_name, solution, "solution");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        *out_result = cv::solveLP(
            opencv_csharp_native::mat_value(objective),
            opencv_csharp_native::mat_value(constraints),
            opencv_csharp_native::mat_value(solution),
            constraint_epsilon);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)objective; (void)constraints; (void)solution; (void)constraint_epsilon;
        *out_result = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

