#include "open_cv_sharp/highgui/highgui.h"

#include "../core/mat_handle.h"
#include "../error_state.h"

#include <algorithm>
#include <atomic>
#include <climits>
#include <cstring>
#include <memory>
#include <mutex>
#include <new>
#include <string>
#include <unordered_map>
#include <utility>
#include <vector>

#if defined(OPENCV_CSHARP_HAS_OPENCV)
#include <opencv2/highgui.hpp>
#endif

struct jyppx_ocv_highgui_trackbar
{
    std::atomic<int> references{ 1 };
    std::atomic<bool> native_attached{ false };
    std::atomic<jyppx_ocv_highgui_trackbar_callback> callback{ nullptr };
    std::atomic<void*> userdata{ nullptr };
    std::string window_name;
    int value{};

    void add_reference() noexcept { references.fetch_add(1, std::memory_order_relaxed); }
    void release_reference() noexcept
    {
        if (references.fetch_sub(1, std::memory_order_acq_rel) == 1)
        {
            delete this;
        }
    }
};

enum class highgui_callback_kind
{
    mouse,
    button
};

struct jyppx_ocv_highgui_callback_registration
{
    std::atomic<int> references{ 1 };
    std::atomic<bool> native_attached{ false };
    highgui_callback_kind kind{ highgui_callback_kind::mouse };
    std::atomic<jyppx_ocv_highgui_mouse_callback> mouse_callback{ nullptr };
    std::atomic<jyppx_ocv_highgui_button_callback> button_callback{ nullptr };
    std::atomic<void*> userdata{ nullptr };
    std::string window_name;

    void add_reference() noexcept { references.fetch_add(1, std::memory_order_relaxed); }
    void release_reference() noexcept
    {
        if (references.fetch_sub(1, std::memory_order_acq_rel) == 1)
        {
            delete this;
        }
    }
};

namespace
{
    int validate_string(const char* api_name, const char* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

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

    int validate_output_double(const char* api_name, const double* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    int validate_output_rect(const char* api_name, const jyppx_ocv_highgui_rect* value, const char* argument_name)
    {
        return value == nullptr
            ? opencv_csharp_native::set_invalid_argument(api_name, argument_name)
            : OPENCV_CSHARP_STATUS_OK;
    }

    bool is_valid_utf8(const unsigned char* value, int length)
    {
        int index = 0;
        while (index < length)
        {
            const unsigned char first = value[index++];
            if (first <= 0x7f)
            {
                if (first == 0) { return false; }
                continue;
            }

            int continuation_count = 0;
            unsigned char second_min = 0x80;
            unsigned char second_max = 0xbf;
            if (first >= 0xc2 && first <= 0xdf) continuation_count = 1;
            else if (first >= 0xe0 && first <= 0xef)
            {
                continuation_count = 2;
                if (first == 0xe0) second_min = 0xa0;
                if (first == 0xed) second_max = 0x9f;
            }
            else if (first >= 0xf0 && first <= 0xf4)
            {
                continuation_count = 3;
                if (first == 0xf0) second_min = 0x90;
                if (first == 0xf4) second_max = 0x8f;
            }
            else return false;

            if (index + continuation_count > length) return false;
            const unsigned char second = value[index++];
            if (second < second_min || second > second_max) return false;
            for (int i = 1; i < continuation_count; ++i)
            {
                const unsigned char next = value[index++];
                if (next < 0x80 || next > 0xbf) return false;
            }
        }
        return true;
    }

    int decode_utf8(
        const char* api_name,
        const unsigned char* value,
        int length,
        const char* argument_name,
        std::string& decoded)
    {
        if (length < 0 || (length > 0 && value == nullptr) || (length > 0 && !is_valid_utf8(value, length)))
        {
            return opencv_csharp_native::set_invalid_argument(api_name, argument_name);
        }
        if (length == 0) decoded.clear();
        else decoded.assign(reinterpret_cast<const char*>(value), static_cast<size_t>(length));
        return OPENCV_CSHARP_STATUS_OK;
    }

#if defined(OPENCV_CSHARP_HAS_OPENCV)
    std::mutex registry_mutex;
    std::unordered_multimap<std::string, jyppx_ocv_highgui_trackbar*> trackbar_registry;
    std::unordered_map<std::string, jyppx_ocv_highgui_callback_registration*> mouse_registry;
    std::vector<jyppx_ocv_highgui_callback_registration*> button_registry;

    jyppx_ocv_highgui_rect from_cv_rect(cv::Rect rect)
    {
        return jyppx_ocv_highgui_rect{ rect.x, rect.y, rect.width, rect.height };
    }

    void trackbar_callback_trampoline(int pos, void* userdata)
    {
        auto trackbar = reinterpret_cast<jyppx_ocv_highgui_trackbar*>(userdata);
        if (trackbar != nullptr)
        {
            trackbar->value = pos;
            jyppx_ocv_highgui_trackbar_callback callback = trackbar->callback.load(std::memory_order_acquire);
            void* callback_userdata = trackbar->userdata.load(std::memory_order_acquire);
            if (callback != nullptr)
            {
                callback(pos, callback_userdata);
            }
        }
    }

    void mouse_callback_trampoline(int event, int x, int y, int flags, void* userdata)
    {
        auto* registration = static_cast<jyppx_ocv_highgui_callback_registration*>(userdata);
        if (registration == nullptr) return;
        jyppx_ocv_highgui_mouse_callback callback = registration->mouse_callback.load(std::memory_order_acquire);
        void* callback_userdata = registration->userdata.load(std::memory_order_acquire);
        if (callback != nullptr) callback(event, x, y, flags, callback_userdata);
    }

    void button_callback_trampoline(int state, void* userdata)
    {
        auto* registration = static_cast<jyppx_ocv_highgui_callback_registration*>(userdata);
        if (registration == nullptr) return;
        jyppx_ocv_highgui_button_callback callback = registration->button_callback.load(std::memory_order_acquire);
        void* callback_userdata = registration->userdata.load(std::memory_order_acquire);
        if (callback != nullptr) callback(state, callback_userdata);
    }

    void detach_trackbar_native(jyppx_ocv_highgui_trackbar* trackbar) noexcept
    {
        trackbar->callback.store(nullptr, std::memory_order_release);
        trackbar->userdata.store(nullptr, std::memory_order_release);
        if (trackbar->native_attached.exchange(false, std::memory_order_acq_rel))
        {
            trackbar->release_reference();
        }
    }

    void detach_callback_native(jyppx_ocv_highgui_callback_registration* registration) noexcept
    {
        registration->mouse_callback.store(nullptr, std::memory_order_release);
        registration->button_callback.store(nullptr, std::memory_order_release);
        registration->userdata.store(nullptr, std::memory_order_release);
        if (registration->native_attached.exchange(false, std::memory_order_acq_rel))
        {
            registration->release_reference();
        }
    }

    void detach_window_registrations(const std::string& window_name)
    {
        std::vector<jyppx_ocv_highgui_trackbar*> trackbars;
        jyppx_ocv_highgui_callback_registration* mouse = nullptr;
        {
            std::lock_guard<std::mutex> lock(registry_mutex);
            auto range = trackbar_registry.equal_range(window_name);
            for (auto current = range.first; current != range.second; ++current) trackbars.push_back(current->second);
            trackbar_registry.erase(range.first, range.second);
            auto mouse_iterator = mouse_registry.find(window_name);
            if (mouse_iterator != mouse_registry.end())
            {
                mouse = mouse_iterator->second;
                mouse_registry.erase(mouse_iterator);
            }
        }
        for (auto* trackbar : trackbars) detach_trackbar_native(trackbar);
        if (mouse != nullptr) detach_callback_native(mouse);
    }

    void detach_all_registrations()
    {
        std::vector<jyppx_ocv_highgui_trackbar*> trackbars;
        std::vector<jyppx_ocv_highgui_callback_registration*> callbacks;
        {
            std::lock_guard<std::mutex> lock(registry_mutex);
            for (const auto& item : trackbar_registry) trackbars.push_back(item.second);
            for (const auto& item : mouse_registry) callbacks.push_back(item.second);
            callbacks.insert(callbacks.end(), button_registry.begin(), button_registry.end());
            trackbar_registry.clear();
            mouse_registry.clear();
            button_registry.clear();
        }
        for (auto* trackbar : trackbars) detach_trackbar_native(trackbar);
        for (auto* callback : callbacks) detach_callback_native(callback);
    }

    int create_trackbar_impl(
        const char* api_name,
        const std::string& trackbar_name,
        const std::string& window_name,
        int initial_value,
        int count,
        jyppx_ocv_highgui_trackbar_callback callback,
        void* userdata,
        jyppx_ocv_highgui_trackbar** trackbar)
    {
        if (count < 0 || initial_value < 0 || initial_value > count || trackbar == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "arguments");

        std::unique_ptr<jyppx_ocv_highgui_trackbar> created(new (std::nothrow) jyppx_ocv_highgui_trackbar());
        if (!created) return opencv_csharp_native::set_out_of_memory(api_name);
        created->window_name = window_name;
        created->value = initial_value;
        created->callback.store(callback, std::memory_order_release);
        created->userdata.store(userdata, std::memory_order_release);
        cv::createTrackbar(
            trackbar_name,
            window_name,
            &created->value,
            count,
            callback == nullptr ? nullptr : trackbar_callback_trampoline,
            created.get());
        created->native_attached.store(true, std::memory_order_release);
        created->add_reference();
        {
            std::lock_guard<std::mutex> lock(registry_mutex);
            trackbar_registry.emplace(window_name, created.get());
        }
        *trackbar = created.release();
        return OPENCV_CSHARP_STATUS_OK;
    }
#endif
}

int jyppx_ocv_highgui_named_window(const char* winname, int flags)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_named_window";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::namedWindow(winname, flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_destroy_window(const char* winname)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_destroy_window";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::destroyWindow(winname);
        detach_window_registrations(winname);
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

int jyppx_ocv_highgui_destroy_all_windows(void)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_destroy_all_windows";
    try
    {
        opencv_csharp_native::clear_last_error();
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::destroyAllWindows();
        detach_all_registrations();
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

int jyppx_ocv_highgui_current_ui_framework_length(int* byte_length)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_current_ui_framework_length";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, byte_length, "byte_length");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::string value = cv::currentUIFramework();
        if (value.size() > static_cast<size_t>(INT_MAX))
            return opencv_csharp_native::set_invalid_argument(api_name, "byte_length");
        *byte_length = static_cast<int>(value.size());
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

int jyppx_ocv_highgui_current_ui_framework_fill(unsigned char* buffer, int buffer_capacity, int* written)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_current_ui_framework_fill";
    try
    {
        opencv_csharp_native::clear_last_error();
        if (buffer_capacity < 0 || (buffer_capacity > 0 && buffer == nullptr) || written == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        const std::string value = cv::currentUIFramework();
        if (value.size() > static_cast<size_t>(INT_MAX) || buffer_capacity < static_cast<int>(value.size()))
            return opencv_csharp_native::set_invalid_argument(api_name, "buffer_capacity");
        if (!value.empty()) std::memcpy(buffer, value.data(), value.size());
        *written = static_cast<int>(value.size());
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

int jyppx_ocv_highgui_start_window_thread(int* result)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_start_window_thread";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, result, "result");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *result = cv::startWindowThread();
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

int jyppx_ocv_highgui_imshow(const char* winname, const jyppx_ocv_mat* mat)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_imshow";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_mat(api_name, mat, "mat");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::imshow(winname, opencv_csharp_native::mat_value(mat));
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

int jyppx_ocv_highgui_wait_key(int delay, int* key)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_wait_key";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, key, "key");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *key = cv::waitKey(delay);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)delay;
        *key = -1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_wait_key_ex(int delay, int* key)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_wait_key_ex";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, key, "key");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *key = cv::waitKeyEx(delay);
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

int jyppx_ocv_highgui_poll_key(int* key)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_poll_key";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, key, "key");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *key = cv::pollKey();
        return OPENCV_CSHARP_STATUS_OK;
#else
        *key = -1;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_move_window(const char* winname, int x, int y)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_move_window";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::moveWindow(winname, x, y);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)x;
        (void)y;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_resize_window(const char* winname, int width, int height)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_resize_window";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::resizeWindow(winname, width, height);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)width;
        (void)height;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_window_property(const char* winname, int prop_id, double prop_value)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_window_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setWindowProperty(winname, prop_id, prop_value);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)prop_id;
        (void)prop_value;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_get_window_property(const char* winname, int prop_id, double* prop_value)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_get_window_property";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_double(api_name, prop_value, "prop_value");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *prop_value = cv::getWindowProperty(winname, prop_id);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)prop_id;
        *prop_value = 0.0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_window_title(const char* winname, const char* title)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_window_title";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, title, "title");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setWindowTitle(winname, title);
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

int jyppx_ocv_highgui_get_window_image_rect(const char* winname, jyppx_ocv_highgui_rect* rect)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_get_window_image_rect";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_rect(api_name, rect, "rect");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *rect = from_cv_rect(cv::getWindowImageRect(winname));
        return OPENCV_CSHARP_STATUS_OK;
#else
        *rect = jyppx_ocv_highgui_rect{ 0, 0, 0, 0 };
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_create_trackbar(
    const char* trackbarname,
    const char* winname,
    int initial_value,
    int count,
    jyppx_ocv_highgui_trackbar_callback callback,
    void* userdata,
    jyppx_ocv_highgui_trackbar** trackbar)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_create_trackbar";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, trackbarname, "trackbarname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        if (trackbar == nullptr)
        {
            return opencv_csharp_native::set_invalid_argument(api_name, "trackbar");
        }

        *trackbar = nullptr;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_trackbar_impl(api_name, trackbarname, winname, initial_value, count, callback, userdata, trackbar);
#else
        (void)initial_value;
        (void)count;
        (void)callback;
        (void)userdata;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_create_trackbar_utf8(
    const unsigned char* trackbar_name,
    int trackbar_name_length,
    const unsigned char* window_name,
    int window_name_length,
    int initial_value,
    int count,
    jyppx_ocv_highgui_trackbar_callback callback,
    void* userdata,
    jyppx_ocv_highgui_trackbar** trackbar)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_create_trackbar_utf8";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::string decoded_trackbar_name;
        std::string decoded_window_name;
        int status = decode_utf8(api_name, trackbar_name, trackbar_name_length, "trackbar_name", decoded_trackbar_name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        status = decode_utf8(api_name, window_name, window_name_length, "window_name", decoded_window_name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (trackbar == nullptr) return opencv_csharp_native::set_invalid_argument(api_name, "trackbar");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        return create_trackbar_impl(
            api_name,
            decoded_trackbar_name,
            decoded_window_name,
            initial_value,
            count,
            callback,
            userdata,
            trackbar);
#else
        (void)initial_value; (void)count; (void)callback; (void)userdata;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_highgui_trackbar_release_handle(jyppx_ocv_highgui_trackbar* trackbar)
{
    if (trackbar != nullptr)
    {
        trackbar->callback.store(nullptr, std::memory_order_release);
        trackbar->userdata.store(nullptr, std::memory_order_release);
        trackbar->release_reference();
    }
}

int jyppx_ocv_highgui_get_trackbar_pos(const char* trackbarname, const char* winname, int* pos)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_get_trackbar_pos";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, trackbarname, "trackbarname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_output_int(api_name, pos, "pos");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *pos = cv::getTrackbarPos(trackbarname, winname);
        return OPENCV_CSHARP_STATUS_OK;
#else
        *pos = 0;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_trackbar_pos(const char* trackbarname, const char* winname, int pos)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_trackbar_pos";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, trackbarname, "trackbarname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setTrackbarPos(trackbarname, winname, pos);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)pos;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_trackbar_min(const char* trackbarname, const char* winname, int minval)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_trackbar_min";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, trackbarname, "trackbarname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setTrackbarMin(trackbarname, winname, minval);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)minval;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_trackbar_max(const char* trackbarname, const char* winname, int maxval)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_trackbar_max";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, trackbarname, "trackbarname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
        status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setTrackbarMax(trackbarname, winname, maxval);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)maxval;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_set_mouse_callback(
    const char* winname,
    jyppx_ocv_highgui_mouse_callback callback,
    void* userdata)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_set_mouse_callback";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, winname, "winname");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setMouseCallback(winname, callback, userdata);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)callback;
        (void)userdata;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_create_button(
    const char* button_name,
    jyppx_ocv_highgui_button_callback callback,
    void* userdata,
    int type,
    int initial_button_state)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_create_button";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_string(api_name, button_name, "button_name");
        if (status != OPENCV_CSHARP_STATUS_OK) { return status; }
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        int result = cv::createButton(
            button_name,
            callback,
            userdata,
            type,
            initial_button_state != 0);
        return result >= 0 ? OPENCV_CSHARP_STATUS_OK : opencv_csharp_native::set_invalid_argument(api_name, "button");
#else
        (void)callback;
        (void)userdata;
        (void)type;
        (void)initial_button_state;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_mouse_callback_create_utf8(
    const unsigned char* window_name,
    int window_name_length,
    jyppx_ocv_highgui_mouse_callback callback,
    void* userdata,
    jyppx_ocv_highgui_callback_registration** registration)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_mouse_callback_create_utf8";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::string decoded_window_name;
        int status = decode_utf8(api_name, window_name, window_name_length, "window_name", decoded_window_name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (callback == nullptr || registration == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "callback");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::unique_ptr<jyppx_ocv_highgui_callback_registration> created(
            new (std::nothrow) jyppx_ocv_highgui_callback_registration());
        if (!created) return opencv_csharp_native::set_out_of_memory(api_name);
        created->kind = highgui_callback_kind::mouse;
        created->window_name = decoded_window_name;
        created->mouse_callback.store(callback, std::memory_order_release);
        created->userdata.store(userdata, std::memory_order_release);
        cv::setMouseCallback(decoded_window_name, mouse_callback_trampoline, created.get());
        created->native_attached.store(true, std::memory_order_release);
        created->add_reference();

        jyppx_ocv_highgui_callback_registration* previous = nullptr;
        {
            std::lock_guard<std::mutex> lock(registry_mutex);
            auto iterator = mouse_registry.find(decoded_window_name);
            if (iterator != mouse_registry.end()) previous = iterator->second;
            mouse_registry[decoded_window_name] = created.get();
        }
        if (previous != nullptr) detach_callback_native(previous);
        *registration = created.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)userdata;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

int jyppx_ocv_highgui_mouse_callback_clear_utf8(const unsigned char* window_name, int window_name_length)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_mouse_callback_clear_utf8";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::string decoded_window_name;
        int status = decode_utf8(api_name, window_name, window_name_length, "window_name", decoded_window_name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        cv::setMouseCallback(decoded_window_name, nullptr, nullptr);
        jyppx_ocv_highgui_callback_registration* previous = nullptr;
        {
            std::lock_guard<std::mutex> lock(registry_mutex);
            auto iterator = mouse_registry.find(decoded_window_name);
            if (iterator != mouse_registry.end())
            {
                previous = iterator->second;
                mouse_registry.erase(iterator);
            }
        }
        if (previous != nullptr) detach_callback_native(previous);
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

int jyppx_ocv_highgui_button_callback_create_utf8(
    const unsigned char* button_name,
    int button_name_length,
    jyppx_ocv_highgui_button_callback callback,
    void* userdata,
    int type,
    int initial_button_state,
    jyppx_ocv_highgui_callback_registration** registration)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_button_callback_create_utf8";
    try
    {
        opencv_csharp_native::clear_last_error();
        std::string decoded_button_name;
        int status = decode_utf8(api_name, button_name, button_name_length, "button_name", decoded_button_name);
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
        if (callback == nullptr || registration == nullptr)
            return opencv_csharp_native::set_invalid_argument(api_name, "callback");
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        std::unique_ptr<jyppx_ocv_highgui_callback_registration> created(
            new (std::nothrow) jyppx_ocv_highgui_callback_registration());
        if (!created) return opencv_csharp_native::set_out_of_memory(api_name);
        created->kind = highgui_callback_kind::button;
        created->button_callback.store(callback, std::memory_order_release);
        created->userdata.store(userdata, std::memory_order_release);
        int result = cv::createButton(
            decoded_button_name,
            button_callback_trampoline,
            created.get(),
            type,
            initial_button_state != 0);
        if (result < 0) return opencv_csharp_native::set_invalid_argument(api_name, "button");
        created->native_attached.store(true, std::memory_order_release);
        created->add_reference();
        {
            std::lock_guard<std::mutex> lock(registry_mutex);
            button_registry.push_back(created.get());
        }
        *registration = created.release();
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)userdata; (void)type; (void)initial_button_state;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}

void jyppx_ocv_highgui_callback_registration_release_handle(
    jyppx_ocv_highgui_callback_registration* registration)
{
    if (registration != nullptr)
    {
        registration->mouse_callback.store(nullptr, std::memory_order_release);
        registration->button_callback.store(nullptr, std::memory_order_release);
        registration->userdata.store(nullptr, std::memory_order_release);
        registration->release_reference();
    }
}

int jyppx_ocv_highgui_get_mouse_wheel_delta(int flags, int* delta)
{
    constexpr const char* api_name = "jyppx_ocv_highgui_get_mouse_wheel_delta";
    try
    {
        opencv_csharp_native::clear_last_error();
        int status = validate_output_int(api_name, delta, "delta");
        if (status != OPENCV_CSHARP_STATUS_OK) return status;
#if defined(OPENCV_CSHARP_HAS_OPENCV)
        *delta = cv::getMouseWheelDelta(flags);
        return OPENCV_CSHARP_STATUS_OK;
#else
        (void)flags;
        return opencv_csharp_native::set_not_linked(api_name);
#endif
    }
    catch (...)
    {
        return opencv_csharp_native::translate_current_exception(api_name);
    }
}


