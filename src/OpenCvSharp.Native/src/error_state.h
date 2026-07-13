#pragma once

#include <string>

namespace opencv_csharp_native
{
    void clear_last_error() noexcept;
    const char* get_last_error() noexcept;
    int set_last_error(int status, const char* message) noexcept;
    int set_last_error(int status, const std::string& message) noexcept;
    int set_out_of_memory(const char* api_name) noexcept;
    int set_not_linked(const char* api_name) noexcept;
    int set_invalid_argument(const char* api_name, const char* argument_name) noexcept;
    int translate_current_exception(const char* api_name) noexcept;
}
