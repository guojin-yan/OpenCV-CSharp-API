#pragma once

#if defined(_WIN32)
#if defined(OPENCV_CSHARP_NATIVE_EXPORTS)
#define OPENCV_CSHARP_API __declspec(dllexport)
#else
#define OPENCV_CSHARP_API __declspec(dllimport)
#endif
#else
#define OPENCV_CSHARP_API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
#define OPENCV_CSHARP_EXTERN_C extern "C"
#else
#define OPENCV_CSHARP_EXTERN_C
#endif
