#include "open_cv_sharp/core/mat.h"

#include "mat_handle.h"
#include "../error_state.h"

#include <new>

#if defined(OPENCV_CSHARP_HAS_OPENCV)
namespace opencv_csharp_native
{
    cv::Mat& mat_value(jyppx_ocv_mat* mat) noexcept
    {
        return mat->value;
    }

    const cv::Mat& mat_value(const jyppx_ocv_mat* mat) noexcept
    {
        return mat->value;
    }
}
#endif

int jyppx_ocv_mat_create_empty(jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_create_empty";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_mat = new (std::nothrow) jyppx_ocv_mat{};
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_create(int rows, int cols, int type, jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_create";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        if (rows < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "rows");
        }

        if (cols < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "cols");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_mat = new (std::nothrow) jyppx_ocv_mat{ cv::Mat(rows, cols, type) };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        (void)type;
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_create_with_scalar(int rows, int cols, int type, double v0, double v1, double v2, double v3, jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_create_with_scalar";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        if (rows < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "rows");
        }

        if (cols < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "cols");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_mat = new (std::nothrow) jyppx_ocv_mat{ cv::Mat(rows, cols, type, cv::Scalar(v0, v1, v2, v3)) };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        (void)v0;
        (void)v1;
        (void)v2;
        (void)v3;
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_create_in_place(jyppx_ocv_mat* mat, int rows, int cols, int type)
{
    constexpr const char* api_name = "jyppx_ocv_mat_create_in_place";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (rows < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "rows");
        }

        if (cols < 0)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "cols");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        mat->value = cv::Mat(rows, cols, type);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)type;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_zeros(int rows, int cols, int type, jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_zeros";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        if (rows < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "rows");
        }

        if (cols < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "cols");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_mat = new (std::nothrow) jyppx_ocv_mat{ cv::Mat::zeros(rows, cols, type) };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        (void)type;
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_ones(int rows, int cols, int type, jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_ones";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        if (rows < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "rows");
        }

        if (cols < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "cols");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_mat = new (std::nothrow) jyppx_ocv_mat{ cv::Mat::ones(rows, cols, type) };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        (void)type;
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_eye(int rows, int cols, int type, jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_eye";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        if (rows < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "rows");
        }

        if (cols < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "cols");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_mat = new (std::nothrow) jyppx_ocv_mat{ cv::Mat::eye(rows, cols, type) };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        (void)type;
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_mat_release(jyppx_ocv_mat* mat)
{
    try
    {
        delete mat;
    }
    catch (...)
    {
    }
}

int jyppx_ocv_mat_clone(const jyppx_ocv_mat* mat, jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_clone";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_mat = new (std::nothrow) jyppx_ocv_mat{ mat->value.clone() };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_copy_to(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst)
{
    constexpr const char* api_name = "jyppx_ocv_mat_copy_to";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        src->value.copyTo(dst->value);
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

int jyppx_ocv_mat_convert_to(const jyppx_ocv_mat* src, jyppx_ocv_mat* dst, int rtype, double alpha, double beta)
{
    constexpr const char* api_name = "jyppx_ocv_mat_convert_to";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (src == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "src");
        }

        if (dst == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "dst");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        src->value.convertTo(dst->value, rtype, alpha, beta);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)rtype;
        (void)alpha;
        (void)beta;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_set_to(jyppx_ocv_mat* mat, double v0, double v1, double v2, double v3)
{
    constexpr const char* api_name = "jyppx_ocv_mat_set_to";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        mat->value.setTo(cv::Scalar(v0, v1, v2, v3));
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)v0;
        (void)v1;
        (void)v2;
        (void)v3;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_submat(const jyppx_ocv_mat* mat, int x, int y, int width, int height, jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_submat";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        if (x < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "x");
        }

        if (y < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "y");
        }

        if (width < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (height < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (x > mat->value.cols || width > mat->value.cols - x)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "width");
        }

        if (y > mat->value.rows || height > mat->value.rows - y)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "height");
        }

        *out_mat = new (std::nothrow) jyppx_ocv_mat{ mat->value(cv::Rect(x, y, width, height)) };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_row_range(const jyppx_ocv_mat* mat, int start_row, int end_row, jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_row_range";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        if (start_row < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "start_row");
        }

        if (end_row < start_row)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "end_row");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (end_row > mat->value.rows)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "end_row");
        }

        *out_mat = new (std::nothrow) jyppx_ocv_mat{ mat->value.rowRange(start_row, end_row) };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_col_range(const jyppx_ocv_mat* mat, int start_col, int end_col, jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_col_range";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        if (start_col < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "start_col");
        }

        if (end_col < start_col)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "end_col");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        if (end_col > mat->value.cols)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "end_col");
        }

        *out_mat = new (std::nothrow) jyppx_ocv_mat{ mat->value.colRange(start_col, end_col) };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_reshape(const jyppx_ocv_mat* mat, int channels, int rows, jyppx_ocv_mat** out_mat)
{
    constexpr const char* api_name = "jyppx_ocv_mat_reshape";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_mat");
        }

        if (channels < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "channels");
        }

        if (rows < 0)
        {
            *out_mat = nullptr;
            return opencv_csharp_native::set_invalid_argument(api_name, "rows");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_mat = new (std::nothrow) jyppx_ocv_mat{ mat->value.reshape(channels, rows) };
        return *out_mat == nullptr ? opencv_csharp_native::set_out_of_memory(api_name) : OPENCV_CSHARP_STATUS_OK;
#else
        *out_mat = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_empty(const jyppx_ocv_mat* mat, int* out_empty)
{
    constexpr const char* api_name = "jyppx_ocv_mat_empty";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_empty == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_empty");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_empty = mat->value.empty() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_empty = 1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_dims(const jyppx_ocv_mat* mat, int* out_dims)
{
    constexpr const char* api_name = "jyppx_ocv_mat_dims";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_dims == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_dims");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_dims = mat->value.dims;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_dims = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_rows(const jyppx_ocv_mat* mat, int* out_rows)
{
    constexpr const char* api_name = "jyppx_ocv_mat_rows";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_rows == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_rows");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_rows = mat->value.rows;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_rows = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_cols(const jyppx_ocv_mat* mat, int* out_cols)
{
    constexpr const char* api_name = "jyppx_ocv_mat_cols";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_cols == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_cols");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_cols = mat->value.cols;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_cols = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_channels(const jyppx_ocv_mat* mat, int* out_channels)
{
    constexpr const char* api_name = "jyppx_ocv_mat_channels";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_channels == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_channels");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_channels = mat->value.channels();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_channels = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_depth(const jyppx_ocv_mat* mat, int* out_depth)
{
    constexpr const char* api_name = "jyppx_ocv_mat_depth";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_depth == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_depth");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_depth = mat->value.depth();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_depth = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_type(const jyppx_ocv_mat* mat, int* out_type)
{
    constexpr const char* api_name = "jyppx_ocv_mat_type";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_type == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_type");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_type = mat->value.type();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_type = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_total(const jyppx_ocv_mat* mat, size_t* out_total)
{
    constexpr const char* api_name = "jyppx_ocv_mat_total";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_total == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_total");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_total = mat->value.total();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_total = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_elem_size(const jyppx_ocv_mat* mat, size_t* out_elem_size)
{
    constexpr const char* api_name = "jyppx_ocv_mat_elem_size";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_elem_size == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_elem_size");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_elem_size = mat->value.elemSize();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_elem_size = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_elem_size1(const jyppx_ocv_mat* mat, size_t* out_elem_size1)
{
    constexpr const char* api_name = "jyppx_ocv_mat_elem_size1";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_elem_size1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_elem_size1");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_elem_size1 = mat->value.elemSize1();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_elem_size1 = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_step(const jyppx_ocv_mat* mat, size_t* out_step)
{
    constexpr const char* api_name = "jyppx_ocv_mat_step";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_step == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_step");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_step = mat->value.step;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_step = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_step1(const jyppx_ocv_mat* mat, size_t* out_step1)
{
    constexpr const char* api_name = "jyppx_ocv_mat_step1";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_step1 == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_step1");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_step1 = mat->value.step1();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_step1 = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_data(const jyppx_ocv_mat* mat, unsigned char** out_data)
{
    constexpr const char* api_name = "jyppx_ocv_mat_data";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_data == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_data");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_data = mat->value.data;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_data = nullptr;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_is_continuous(const jyppx_ocv_mat* mat, int* out_is_continuous)
{
    constexpr const char* api_name = "jyppx_ocv_mat_is_continuous";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_is_continuous == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_is_continuous");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_is_continuous = mat->value.isContinuous() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_is_continuous = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_mat_is_submatrix(const jyppx_ocv_mat* mat, int* out_is_submatrix)
{
    constexpr const char* api_name = "jyppx_ocv_mat_is_submatrix";

    try
    {
        opencv_csharp_native::clear_last_error();

        if (mat == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "mat");
        }

        if (out_is_submatrix == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "out_is_submatrix");
        }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *out_is_submatrix = mat->value.isSubmatrix() ? 1 : 0;
        return OPENCV_CSHARP_STATUS_OK;
#else
        *out_is_submatrix = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}


