#include "open_cv_sharp/plot/plot.h"

#include "../core/mat_handle.h"
#include "../error_state.h"
#include "plot_handles.h"

#include <new>

namespace
{
    int validate_mat(const char* api_name, const jyppx_ocv_mat* mat, const char* argument_name)
    {
        return mat == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_plot(const char* api_name, const jyppx_ocv_plot_2d* plot)
    {
        return plot == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, "plot")
            : OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
    int create_plot_handle(const char* api_name, const cv::Ptr<cv::plot::Plot2d>& native, jyppx_ocv_plot_2d** plot)
    {
        if (plot == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "plot");
        }

        *plot = nullptr;
        jyppx_ocv_plot_2d* created = new (std::nothrow) jyppx_ocv_plot_2d();
        if (created == nullptr)
        {
            return opencv_csharp_native::set_out_of_memory(api_name);
        }

        created->value = native;
        *plot = created;
        return OPENCV_CSHARP_STATUS_OK;
    }

    int set_scalar_property(
        const char* api_name,
        jyppx_ocv_plot_2d* plot,
        void (cv::plot::Plot2d::*setter)(cv::Scalar),
        double v0,
        double v1,
        double v2,
        double v3)
    {
        int status = validate_plot(api_name, plot);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        (plot->value.get()->*setter)(cv::Scalar(v0, v1, v2, v3));
        return OPENCV_CSHARP_STATUS_OK;
    }

    int set_double_property(
        const char* api_name,
        jyppx_ocv_plot_2d* plot,
        void (cv::plot::Plot2d::*setter)(double),
        double value)
    {
        int status = validate_plot(api_name, plot);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        (plot->value.get()->*setter)(value);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int set_int_property(
        const char* api_name,
        jyppx_ocv_plot_2d* plot,
        void (cv::plot::Plot2d::*setter)(int),
        int value)
    {
        int status = validate_plot(api_name, plot);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        (plot->value.get()->*setter)(value);
        return OPENCV_CSHARP_STATUS_OK;
    }

    int set_bool_property(
        const char* api_name,
        jyppx_ocv_plot_2d* plot,
        void (cv::plot::Plot2d::*setter)(bool),
        int value)
    {
        int status = validate_plot(api_name, plot);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

        (plot->value.get()->*setter)(value != 0);
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_plot_2d_create(const jyppx_ocv_mat* data, jyppx_ocv_plot_2d** plot)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_create";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, data, "data");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return create_plot_handle(api_name, cv::plot::Plot2d::create(opencv_csharp_native::mat_value(data)), plot);
#else
        if (plot != nullptr) { *plot = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_create_xy(const jyppx_ocv_mat* data_x, const jyppx_ocv_mat* data_y, jyppx_ocv_plot_2d** plot)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_create_xy";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_mat(api_name, data_x, "data_x");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, data_y, "data_y");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return create_plot_handle(
            api_name,
            cv::plot::Plot2d::create(opencv_csharp_native::mat_value(data_x), opencv_csharp_native::mat_value(data_y)),
            plot);
#else
        if (plot != nullptr) { *plot = nullptr; }
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_plot_2d_release_handle(jyppx_ocv_plot_2d* plot)
{
    delete plot;
}

int jyppx_ocv_plot_2d_set_min_x(jyppx_ocv_plot_2d* plot, double value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_min_x";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_double_property(api_name, plot, &cv::plot::Plot2d::setMinX, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_min_y(jyppx_ocv_plot_2d* plot, double value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_min_y";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_double_property(api_name, plot, &cv::plot::Plot2d::setMinY, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_max_x(jyppx_ocv_plot_2d* plot, double value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_max_x";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_double_property(api_name, plot, &cv::plot::Plot2d::setMaxX, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_max_y(jyppx_ocv_plot_2d* plot, double value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_max_y";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_double_property(api_name, plot, &cv::plot::Plot2d::setMaxY, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_plot_line_width(jyppx_ocv_plot_2d* plot, int value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_plot_line_width";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_int_property(api_name, plot, &cv::plot::Plot2d::setPlotLineWidth, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_need_plot_line(jyppx_ocv_plot_2d* plot, int value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_need_plot_line";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_bool_property(api_name, plot, &cv::plot::Plot2d::setNeedPlotLine, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_plot_line_color(jyppx_ocv_plot_2d* plot, double v0, double v1, double v2, double v3)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_plot_line_color";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_scalar_property(api_name, plot, &cv::plot::Plot2d::setPlotLineColor, v0, v1, v2, v3);
#else
        (void)v0; (void)v1; (void)v2; (void)v3;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_plot_background_color(jyppx_ocv_plot_2d* plot, double v0, double v1, double v2, double v3)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_plot_background_color";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_scalar_property(api_name, plot, &cv::plot::Plot2d::setPlotBackgroundColor, v0, v1, v2, v3);
#else
        (void)v0; (void)v1; (void)v2; (void)v3;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_plot_axis_color(jyppx_ocv_plot_2d* plot, double v0, double v1, double v2, double v3)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_plot_axis_color";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_scalar_property(api_name, plot, &cv::plot::Plot2d::setPlotAxisColor, v0, v1, v2, v3);
#else
        (void)v0; (void)v1; (void)v2; (void)v3;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_plot_grid_color(jyppx_ocv_plot_2d* plot, double v0, double v1, double v2, double v3)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_plot_grid_color";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_scalar_property(api_name, plot, &cv::plot::Plot2d::setPlotGridColor, v0, v1, v2, v3);
#else
        (void)v0; (void)v1; (void)v2; (void)v3;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_plot_text_color(jyppx_ocv_plot_2d* plot, double v0, double v1, double v2, double v3)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_plot_text_color";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_scalar_property(api_name, plot, &cv::plot::Plot2d::setPlotTextColor, v0, v1, v2, v3);
#else
        (void)v0; (void)v1; (void)v2; (void)v3;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_plot_size(jyppx_ocv_plot_2d* plot, int width, int height)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_plot_size";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_plot(api_name, plot);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        plot->value->setPlotSize(width, height);
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

int jyppx_ocv_plot_2d_set_show_grid(jyppx_ocv_plot_2d* plot, int value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_show_grid";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_bool_property(api_name, plot, &cv::plot::Plot2d::setShowGrid, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_show_text(jyppx_ocv_plot_2d* plot, int value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_show_text";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_bool_property(api_name, plot, &cv::plot::Plot2d::setShowText, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_grid_lines_number(jyppx_ocv_plot_2d* plot, int value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_grid_lines_number";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_int_property(api_name, plot, &cv::plot::Plot2d::setGridLinesNumber, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_invert_orientation(jyppx_ocv_plot_2d* plot, int value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_invert_orientation";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_bool_property(api_name, plot, &cv::plot::Plot2d::setInvertOrientation, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_set_point_idx_to_print(jyppx_ocv_plot_2d* plot, int value)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_set_point_idx_to_print";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        return set_int_property(api_name, plot, &cv::plot::Plot2d::setPointIdxToPrint, value);
#else
        (void)value;
        int status = validate_plot(api_name, plot);
        return status != OPENCV_CSHARP_STATUS_OK ? status : opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_plot_2d_render(jyppx_ocv_plot_2d* plot, jyppx_ocv_mat* result)
{
    constexpr const char* api_name = "jyppx_ocv_plot_2d_render";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_plot(api_name, plot);
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_PLOT)
        plot->value->render(opencv_csharp_native::mat_value(result));
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


