#include "error_state.h"

#include "open_cv_sharp/error.h"
#include "open_cv_sharp/status.h"

#include <exception>
#include <string>

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/core.hpp>
#endif

namespace
{
    thread_local std::string last_error;
}

namespace opencv_csharp_native
{
    void clear_last_error() noexcept
    {
        last_error.clear();
    }

    const char* get_last_error() noexcept
    {
        return last_error.c_str();
    }

    int set_last_error(int status, const char* message) noexcept
    {
        last_error = message == nullptr ? "" : message;
        return status;
    }

    int set_last_error(int status, const std::string& message) noexcept
    {
        last_error = message;
        return status;
    }

    int set_out_of_memory(const char* api_name) noexcept
    {
        std::string message = api_name == nullptr ? "Native allocation failed." : std::string(api_name) + " failed: native allocation returned null.";
        return set_last_error(OPENCV_CSHARP_STATUS_OUT_OF_MEMORY, message);
    }

    int set_not_linked(const char* api_name) noexcept
    {
        std::string message = api_name == nullptr ? "OpenCV backend is not linked." : std::string(api_name) + " requires a native build linked with OpenCV.";
        return set_last_error(OPENCV_CSHARP_STATUS_NOT_LINKED, message);
    }

    int set_invalid_argument(const char* api_name, const char* argument_name) noexcept
    {
        std::string message;
        if (api_name != nullptr)
        {
            message += api_name;
            message += " failed: ";
        }

        message += "invalid argument";
        if (argument_name != nullptr)
        {
            message += " '";
            message += argument_name;
            message += "'";
        }

        message += ".";
        return set_last_error(OPENCV_CSHARP_STATUS_INVALID_ARGUMENT, message);
    }

    int translate_current_exception(const char* api_name) noexcept
    {
        try
        {
            throw;
        }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        catch (const cv::Exception& exception)
        {
            std::string message = api_name == nullptr ? exception.what() : std::string(api_name) + " failed: " + exception.what();
            return set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, message);
        }
#endif
        catch (const std::bad_alloc&)
        {
            return set_out_of_memory(api_name);
        }
        catch (const std::exception& exception)
        {
            std::string message = api_name == nullptr ? exception.what() : std::string(api_name) + " failed: " + exception.what();
            return set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, message);
        }
        catch (...)
        {
            std::string message = api_name == nullptr ? "Unknown native exception." : std::string(api_name) + " failed with an unknown native exception.";
            return set_last_error(OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION, message);
        }
    }
}

const char* jyppx_ocv_get_last_error(void)
{
    return opencv_csharp_native::get_last_error();
}

void jyppx_ocv_clear_last_error(void)
{
    opencv_csharp_native::clear_last_error();
}
