#pragma once

#if defined(_WIN32)
#if defined(OPENCV_CSHARP_NATIVE_EXPORTS) || defined(OPENCV5SHARP_NATIVE_EXPORTS)
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

// OPENCV5SHARP_* export macros remain only as source-compatibility aliases for existing native includes.
#ifndef OPENCV5SHARP_API
#define OPENCV5SHARP_API OPENCV_CSHARP_API
#endif

#ifndef OPENCV5SHARP_EXTERN_C
#define OPENCV5SHARP_EXTERN_C OPENCV_CSHARP_EXTERN_C
#endif
