cmake_minimum_required(VERSION 3.20)

project(OpenCvCSharpNative VERSION 5.0.0 LANGUAGES CXX)

set(OPENCV_CSHARP_OPENCV_DIR_DESCRIPTION "Directory containing OpenCVConfig.cmake.")
set(OPENCV_CSHARP_OPENCV_BUILD_LIST_DEFAULT "core,imgproc,imgcodecs,videoio,flann,geometry,calib,stereo,dnn,objdetect,photo,features,video,highgui,stitching,ptcloud")
set(OPENCV_CSHARP_OPENCV_BUILD_LIST_DESCRIPTION "OpenCV main-module build list used by packaging automation. Contrib modules such as ml, img_hash, xfeatures2d, xobjdetect, quality, xphoto, optflow, bgsegm, tracking, plot, shape, line_descriptor, phase_unwrapping, structured_light, intensity_transform, fuzzy, hfs, reg, surface_matching, rapid, alphamat, bioinspired, and xstereo are added by scripts/Build-OpenCV.ps1 when -WithContrib is set.")

set(OPENCV_CSHARP_BUILD_WITH_OPENCV_WAS_DEFINED FALSE)
if(DEFINED OPENCV_CSHARP_BUILD_WITH_OPENCV)
    set(OPENCV_CSHARP_BUILD_WITH_OPENCV_WAS_DEFINED TRUE)
endif()

set(OPENCV_CSHARP_OPENCV_DIR_WAS_DEFINED FALSE)
if(DEFINED OPENCV_CSHARP_OPENCV_DIR)
    set(OPENCV_CSHARP_OPENCV_DIR_WAS_DEFINED TRUE)
endif()

set(OPENCV_CSHARP_OPENCV_BUILD_LIST_WAS_DEFINED FALSE)
if(DEFINED OPENCV_CSHARP_OPENCV_BUILD_LIST)
    set(OPENCV_CSHARP_OPENCV_BUILD_LIST_WAS_DEFINED TRUE)
endif()

option(OPENCV_CSHARP_BUILD_WITH_OPENCV "Link against an installed OpenCV package." OFF)
set(OPENCV_CSHARP_OPENCV_DIR "" CACHE PATH "${OPENCV_CSHARP_OPENCV_DIR_DESCRIPTION}")
set(OPENCV_CSHARP_OPENCV_BUILD_LIST "${OPENCV_CSHARP_OPENCV_BUILD_LIST_DEFAULT}" CACHE STRING "${OPENCV_CSHARP_OPENCV_BUILD_LIST_DESCRIPTION}")

if(NOT OPENCV_CSHARP_BUILD_WITH_OPENCV_WAS_DEFINED AND DEFINED OPENCV5SHARP_BUILD_WITH_OPENCV)
    set(OPENCV_CSHARP_BUILD_WITH_OPENCV "${OPENCV5SHARP_BUILD_WITH_OPENCV}" CACHE BOOL "Link against an installed OpenCV package." FORCE)
endif()

if(NOT OPENCV_CSHARP_OPENCV_DIR_WAS_DEFINED AND DEFINED OPENCV5SHARP_OPENCV_DIR AND NOT "${OPENCV5SHARP_OPENCV_DIR}" STREQUAL "")
    set(OPENCV_CSHARP_OPENCV_DIR "${OPENCV5SHARP_OPENCV_DIR}" CACHE PATH "${OPENCV_CSHARP_OPENCV_DIR_DESCRIPTION}" FORCE)
endif()

if(NOT OPENCV_CSHARP_OPENCV_BUILD_LIST_WAS_DEFINED AND DEFINED OPENCV5SHARP_OPENCV_BUILD_LIST AND NOT "${OPENCV5SHARP_OPENCV_BUILD_LIST}" STREQUAL "")
    set(OPENCV_CSHARP_OPENCV_BUILD_LIST "${OPENCV5SHARP_OPENCV_BUILD_LIST}" CACHE STRING "${OPENCV_CSHARP_OPENCV_BUILD_LIST_DESCRIPTION}" FORCE)
endif()

if(OPENCV_CSHARP_OPENCV_DIR)
    set(OpenCV_DIR "${OPENCV_CSHARP_OPENCV_DIR}" CACHE PATH "${OPENCV_CSHARP_OPENCV_DIR_DESCRIPTION}" FORCE)
    set(OPENCV_CSHARP_BUILD_WITH_OPENCV ON CACHE BOOL "Link against an installed OpenCV package." FORCE)
endif()

set(OPENCV5SHARP_BUILD_WITH_OPENCV "${OPENCV_CSHARP_BUILD_WITH_OPENCV}" CACHE BOOL "Existing-build-script compatibility alias for OPENCV_CSHARP_BUILD_WITH_OPENCV." FORCE)
set(OPENCV5SHARP_OPENCV_DIR "${OPENCV_CSHARP_OPENCV_DIR}" CACHE PATH "Existing-build-script compatibility alias for OPENCV_CSHARP_OPENCV_DIR." FORCE)
set(OPENCV5SHARP_OPENCV_BUILD_LIST "${OPENCV_CSHARP_OPENCV_BUILD_LIST}" CACHE STRING "Existing-build-script compatibility alias for OPENCV_CSHARP_OPENCV_BUILD_LIST." FORCE)

set(OPENCV_CSHARP_NATIVE_TARGET "JYPPX.OpenCV.Native")
set(OPENCV_CSHARP_COMPATIBILITY_NATIVE_TARGET "OpenCv5Sharp.Native")
set(OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME "${OPENCV_CSHARP_NATIVE_TARGET}")
set(OPENCV_CSHARP_NATIVE_SMOKE_TARGET "${OPENCV_CSHARP_NATIVE_TARGET}Smoke")
set(OPENCV_CSHARP_COMPATIBILITY_SOURCE_SMOKE_TARGET "${OPENCV_CSHARP_NATIVE_TARGET}CompatibilitySourceSmoke")
# JYPPX.OpenCV.Native is the version-neutral primary native target/output.
# OpenCv5Sharp.Native remains only as a compatibility target and copied loader filename.
# Keep OPENCV_CSHARP_* variables primary and OPENCV5SHARP_* variables only as existing-build-script compatibility aliases.
add_library(${OPENCV_CSHARP_NATIVE_TARGET} SHARED
    generated/legacy_abi.cpp
    src/error_state.cpp
    src/version.cpp
    src/core/mat.cpp
    src/core/decomp.cpp
    src/core/operations.cpp
    src/calib3d.cpp
    src/stereo/stereo.cpp
    src/objdetect/objdetect.cpp
    src/objdetect/aruco.cpp
    src/xobjdetect/xobjdetect.cpp
    src/video/video.cpp
    src/tracking/tracking.cpp
    src/face/face.cpp
    src/saliency/saliency.cpp
    src/plot/plot.cpp
    src/shape/shape.cpp
    src/line_descriptor/line_descriptor.cpp
    src/phase_unwrapping/phase_unwrapping.cpp
    src/structured_light/structured_light.cpp
    src/intensity_transform/intensity_transform.cpp
    src/fuzzy/fuzzy.cpp
    src/hfs/hfs.cpp
    src/reg/reg.cpp
    src/surface_matching/surface_matching.cpp
    src/rapid/rapid.cpp
    src/alphamat/alphamat.cpp
    src/bioinspired/bioinspired.cpp
    src/xstereo/xstereo.cpp
    src/ximgproc/ximgproc.cpp
    src/optflow/optflow.cpp
    src/dnn/dnn.cpp
    src/highgui/highgui.cpp
    src/stitching/stitching.cpp
    src/ptcloud/ptcloud.cpp
    src/ml/ml.cpp
    src/bgsegm/bgsegm.cpp
    src/quality/quality.cpp
    src/xphoto/xphoto.cpp
    src/img_hash/img_hash.cpp
    src/videoio/videoio.cpp
    src/photo/photo.cpp
    src/features2d/features2d.cpp
    src/features2d/features2d_extra.cpp
    src/imgcodecs.cpp
    src/imgproc.cpp
)

add_library(${OPENCV_CSHARP_COMPATIBILITY_NATIVE_TARGET} ALIAS ${OPENCV_CSHARP_NATIVE_TARGET})

target_compile_features(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE cxx_std_17)

target_include_directories(${OPENCV_CSHARP_NATIVE_TARGET}
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET}
    PRIVATE
        OPENCV_CSHARP_NATIVE_EXPORTS
)

if(OPENCV_CSHARP_BUILD_WITH_OPENCV)
    find_package(OpenCV REQUIRED COMPONENTS core calib dnn geometry highgui imgcodecs imgproc objdetect photo stereo video videoio OPTIONAL_COMPONENTS features flann stitching ptcloud ml xfeatures2d xobjdetect quality xphoto img_hash optflow bgsegm ximgproc tracking face saliency plot shape line_descriptor phase_unwrapping structured_light intensity_transform fuzzy hfs reg surface_matching rapid alphamat bioinspired xstereo)
    target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE ${OpenCV_LIBS})
    target_include_directories(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE ${OpenCV_INCLUDE_DIRS})
    target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE
        # Keep OPENCV5SHARP_* feature macros only as existing-include-guard compatibility definitions.
        OPENCV_CSHARP_HAS_OPENCV=1
        
    )

    # OpenCV 5.0.0 renamed the old features2d module surface to the opencv_features target.
    if(TARGET opencv_features)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_features)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_FEATURES2D=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} features2d support is enabled through opencv_features")
    else()
        message(WARNING "opencv_features target was not found. features2d APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_xfeatures2d)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_xfeatures2d)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_XFEATURES2D=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} xfeatures2d support is enabled through opencv_xfeatures2d")
    else()
        message(STATUS "opencv_xfeatures2d target was not found. BRISK/KAZE/AKAZE APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_stitching)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_stitching)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_STITCHING=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} stitching support is enabled through opencv_stitching")
    else()
        message(STATUS "opencv_stitching target was not found. stitching APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_xobjdetect)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_xobjdetect)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_XOBJDETECT=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} xobjdetect support is enabled through opencv_xobjdetect")
    else()
        message(STATUS "opencv_xobjdetect target was not found. xobjdetect APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_ptcloud)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_ptcloud)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_PTCLOUD=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} ptcloud support is enabled through opencv_ptcloud")
    else()
        message(STATUS "opencv_ptcloud target was not found. ptcloud APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_quality)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_quality)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_QUALITY=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} quality support is enabled through opencv_quality")
    else()
        message(STATUS "opencv_quality target was not found. quality APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_ml)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_ml)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_ML=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} ml support is enabled through opencv_ml")
    else()
        message(STATUS "opencv_ml target was not found. ml APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_xphoto)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_xphoto)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_XPHOTO=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} xphoto support is enabled through opencv_xphoto")
    else()
        message(STATUS "opencv_xphoto target was not found. xphoto APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_img_hash)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_img_hash)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_IMG_HASH=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} img_hash support is enabled through opencv_img_hash")
    else()
        message(STATUS "opencv_img_hash target was not found. img_hash APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_ximgproc)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_ximgproc)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_XIMGPROC=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} ximgproc support is enabled through opencv_ximgproc")
    else()
        message(STATUS "opencv_ximgproc target was not found. ximgproc APIs and optflow contrib algorithms that depend on it may report NOT_LINKED.")
    endif()

    if(TARGET opencv_optflow)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_optflow)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_OPTFLOW=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} optflow support is enabled through opencv_optflow")
    else()
        message(STATUS "opencv_optflow target was not found. optflow APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_bgsegm)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_bgsegm)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_BGSEGM=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} bgsegm support is enabled through opencv_bgsegm")
    else()
        message(STATUS "opencv_bgsegm target was not found. bgsegm APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_tracking)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_tracking)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_TRACKING=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} tracking support is enabled through opencv_tracking")
    else()
        message(STATUS "opencv_tracking target was not found. tracking APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_face)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_face)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_FACE=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} face support is enabled through opencv_face")
    else()
        message(STATUS "opencv_face target was not found. face APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_saliency)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_saliency)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_SALIENCY=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} saliency support is enabled through opencv_saliency")
    else()
        message(STATUS "opencv_saliency target was not found. saliency APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_plot)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_plot)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_PLOT=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} plot support is enabled through opencv_plot")
    else()
        message(STATUS "opencv_plot target was not found. plot APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_shape)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_shape)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_SHAPE=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} shape support is enabled through opencv_shape")
    else()
        message(STATUS "opencv_shape target was not found. shape APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_line_descriptor)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_line_descriptor)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_LINE_DESCRIPTOR=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} line_descriptor support is enabled through opencv_line_descriptor")
    else()
        message(STATUS "opencv_line_descriptor target was not found. line descriptor APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_phase_unwrapping)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_phase_unwrapping)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_PHASE_UNWRAPPING=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} phase_unwrapping support is enabled through opencv_phase_unwrapping")
    else()
        message(STATUS "opencv_phase_unwrapping target was not found. phase unwrapping APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_structured_light)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_structured_light)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_STRUCTURED_LIGHT=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} structured_light support is enabled through opencv_structured_light")
    else()
        message(STATUS "opencv_structured_light target was not found. structured light APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_intensity_transform)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_intensity_transform)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_INTENSITY_TRANSFORM=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} intensity_transform support is enabled through opencv_intensity_transform")
    else()
        message(STATUS "opencv_intensity_transform target was not found. intensity transform APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_fuzzy)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_fuzzy)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_FUZZY=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} fuzzy support is enabled through opencv_fuzzy")
    else()
        message(STATUS "opencv_fuzzy target was not found. fuzzy APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_hfs)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_hfs)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_HFS=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} hfs support is enabled through opencv_hfs")
    else()
        message(STATUS "opencv_hfs target was not found. hfs APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_reg)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_reg)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_REG=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} reg support is enabled through opencv_reg")
    else()
        message(STATUS "opencv_reg target was not found. reg APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_surface_matching)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_surface_matching)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_SURFACE_MATCHING=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} surface_matching support is enabled through opencv_surface_matching")
    else()
        message(STATUS "opencv_surface_matching target was not found. surface matching APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_rapid)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_rapid)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_RAPID=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} rapid support is enabled through opencv_rapid")
    else()
        message(STATUS "opencv_rapid target was not found. rapid APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_alphamat)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_alphamat)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_ALPHAMAT=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} alphamat support is enabled through opencv_alphamat")
    else()
        message(STATUS "opencv_alphamat target was not found. alphamat APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_bioinspired)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_bioinspired)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_BIOINSPIRED=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} bioinspired support is enabled through opencv_bioinspired")
    else()
        message(STATUS "opencv_bioinspired target was not found. bioinspired APIs will report NOT_LINKED.")
    endif()

    if(TARGET opencv_xstereo)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_xstereo)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_XSTEREO=1 )
        message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} xstereo support is enabled through opencv_xstereo")
    else()
        message(STATUS "opencv_xstereo target was not found. xstereo APIs will report NOT_LINKED.")
    endif()

    message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} is linked with OpenCV ${OpenCV_VERSION}")

    if(WIN32)
        set(OPENCV_CSHARP_OPENCV_RUNTIME_ROOT "")
        # Discover runtime DLLs from OpenCV_DIR using version-neutral project variables.
        # The copied opencv*.dll names remain factual upstream artifacts for the linked OpenCV build.
        set(OPENCV_CSHARP_OPENCV_RUNTIME_ROOT_CANDIDATES
            "${OpenCV_DIR}/bin"
            "${OpenCV_DIR}/x64/vc18/bin"
            "${OpenCV_DIR}/../bin"
            "${OpenCV_DIR}/../../bin"
        )

        foreach(OPENCV_CSHARP_CANDIDATE IN LISTS OPENCV_CSHARP_OPENCV_RUNTIME_ROOT_CANDIDATES)
            if(EXISTS "${OPENCV_CSHARP_CANDIDATE}")
                file(GLOB OPENCV_CSHARP_CANDIDATE_FLAT_DLLS
                    "${OPENCV_CSHARP_CANDIDATE}/opencv*.dll")
                file(GLOB OPENCV_CSHARP_CANDIDATE_RELEASE_DLLS
                    "${OPENCV_CSHARP_CANDIDATE}/Release/opencv*.dll")

                if(OPENCV_CSHARP_CANDIDATE_RELEASE_DLLS)
                    set(OPENCV_CSHARP_OPENCV_RUNTIME_ROOT "${OPENCV_CSHARP_CANDIDATE}/Release")
                    break()
                endif()

                if(OPENCV_CSHARP_CANDIDATE_FLAT_DLLS)
                    set(OPENCV_CSHARP_OPENCV_RUNTIME_ROOT "${OPENCV_CSHARP_CANDIDATE}")
                    break()
                endif()
            endif()
        endforeach()

        if(OPENCV_CSHARP_OPENCV_RUNTIME_ROOT)
            message(STATUS "OpenCV runtime root: ${OPENCV_CSHARP_OPENCV_RUNTIME_ROOT}")
            add_custom_command(TARGET ${OPENCV_CSHARP_NATIVE_TARGET} POST_BUILD
                COMMAND ${CMAKE_COMMAND} -E copy_directory
                    "${OPENCV_CSHARP_OPENCV_RUNTIME_ROOT}"
                    "$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>"
            )
        else()
            message(WARNING "OpenCV runtime directory was not found. Native smoke tests may need PATH to include OpenCV binaries.")
        endif()
    endif()
else()
    message(STATUS "${OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME} is building without OpenCV. Runtime Mat APIs will report NOT_LINKED.")
endif()

set_target_properties(${OPENCV_CSHARP_NATIVE_TARGET} PROPERTIES
    OUTPUT_NAME "${OPENCV_CSHARP_NATIVE_TARGET}"
)

set(
    OPENCV_CSHARP_COMPATIBILITY_NATIVE_FILE_NAME
    "${CMAKE_SHARED_LIBRARY_PREFIX}${OPENCV_CSHARP_COMPATIBILITY_NATIVE_TARGET}${CMAKE_SHARED_LIBRARY_SUFFIX}"
)
add_custom_command(TARGET ${OPENCV_CSHARP_NATIVE_TARGET} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        "$<TARGET_FILE:${OPENCV_CSHARP_NATIVE_TARGET}>"
        "$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>/${OPENCV_CSHARP_COMPATIBILITY_NATIVE_FILE_NAME}"
    COMMENT "Creating native loader compatibility copy ${OPENCV_CSHARP_COMPATIBILITY_NATIVE_FILE_NAME}"
    VERBATIM
)

enable_testing()

add_executable(${OPENCV_CSHARP_NATIVE_SMOKE_TARGET}
    tests/native_smoke.cpp
)

target_link_libraries(${OPENCV_CSHARP_NATIVE_SMOKE_TARGET} PRIVATE ${OPENCV_CSHARP_NATIVE_TARGET})
target_include_directories(${OPENCV_CSHARP_NATIVE_SMOKE_TARGET} PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/include)

add_test(NAME ${OPENCV_CSHARP_NATIVE_SMOKE_TARGET} COMMAND ${OPENCV_CSHARP_NATIVE_SMOKE_TARGET})

add_executable(${OPENCV_CSHARP_COMPATIBILITY_SOURCE_SMOKE_TARGET}
    tests/legacy_source_compat_smoke.cpp
)
target_link_libraries(${OPENCV_CSHARP_COMPATIBILITY_SOURCE_SMOKE_TARGET} PRIVATE ${OPENCV_CSHARP_NATIVE_TARGET})
target_include_directories(${OPENCV_CSHARP_COMPATIBILITY_SOURCE_SMOKE_TARGET} PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/include)
add_test(
    NAME ${OPENCV_CSHARP_COMPATIBILITY_SOURCE_SMOKE_TARGET}
    COMMAND ${OPENCV_CSHARP_COMPATIBILITY_SOURCE_SMOKE_TARGET}
)

set(
    OPENCV_CSHARP_NATIVE_RUNTIME_TESTS
    ${OPENCV_CSHARP_NATIVE_SMOKE_TARGET}
    ${OPENCV_CSHARP_COMPATIBILITY_SOURCE_SMOKE_TARGET}
)

find_program(OPENCV_CSHARP_PWSH_EXECUTABLE NAMES pwsh)
if(OPENCV_CSHARP_PWSH_EXECUTABLE)
    add_test(
        NAME ${OPENCV_CSHARP_NATIVE_TARGET}AbiGeneratedCheck
        COMMAND
            ${OPENCV_CSHARP_PWSH_EXECUTABLE}
            -NoProfile
            -File
            ${CMAKE_CURRENT_SOURCE_DIR}/../../scripts/Generate-NativeAbiCompatibility.ps1
            -RepositoryRoot
            ${CMAKE_CURRENT_SOURCE_DIR}/../..
            -Check
    )

    add_test(
        NAME ${OPENCV_CSHARP_NATIVE_TARGET}LegacyIncludeParity
        COMMAND
            ${OPENCV_CSHARP_PWSH_EXECUTABLE}
            -NoProfile
            -File
            ${CMAKE_CURRENT_SOURCE_DIR}/../../scripts/Test-NativeLegacyIncludeParity.ps1
            -RepositoryRoot
            ${CMAKE_CURRENT_SOURCE_DIR}/../..
    )

    set(OPENCV_CSHARP_NATIVE_ABI_EXPORT_TEST "${OPENCV_CSHARP_NATIVE_TARGET}AbiExportAudit")
    add_test(
        NAME ${OPENCV_CSHARP_NATIVE_ABI_EXPORT_TEST}
        COMMAND
            ${OPENCV_CSHARP_PWSH_EXECUTABLE}
            -NoProfile
            -File
            ${CMAKE_CURRENT_SOURCE_DIR}/../../scripts/Test-NativeAbiExports.ps1
            -LibraryPath
            $<TARGET_FILE:${OPENCV_CSHARP_NATIVE_TARGET}>
            -ManifestPath
            ${CMAKE_CURRENT_SOURCE_DIR}/generated/legacy_abi_manifest.txt
    )
    list(APPEND
        OPENCV_CSHARP_NATIVE_RUNTIME_TESTS
        ${OPENCV_CSHARP_NATIVE_ABI_EXPORT_TEST}
    )
else()
    message(WARNING "pwsh was not found. Native ABI generation, legacy include parity, and export checks are disabled.")
endif()

if(WIN32)
    if(OPENCV_CSHARP_OPENCV_RUNTIME_ROOT)
        set_tests_properties(${OPENCV_CSHARP_NATIVE_RUNTIME_TESTS} PROPERTIES
            ENVIRONMENT "PATH=$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>;${OPENCV_CSHARP_OPENCV_RUNTIME_ROOT};$ENV{PATH}"
        )
    else()
        set_tests_properties(${OPENCV_CSHARP_NATIVE_RUNTIME_TESTS} PROPERTIES
            ENVIRONMENT "PATH=$<TARGET_FILE_DIR:${OPENCV_CSHARP_NATIVE_TARGET}>;$ENV{PATH}"
        )
    endif()
endif()
