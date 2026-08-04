cmake_minimum_required(VERSION 3.20)

project(OpenCvCSharpNative VERSION 5.0.0 LANGUAGES CXX)

set(OPENCV_CSHARP_OPENCV_DIR_DESCRIPTION "Directory containing OpenCVConfig.cmake.")
set(OPENCV_CSHARP_OPENCV_BUILD_LIST_DEFAULT "core,imgproc,imgcodecs,videoio,flann,geometry,calib,stereo,dnn,objdetect,photo,features,video,highgui,stitching,ptcloud")
set(OPENCV_CSHARP_OPENCV_BUILD_LIST_DESCRIPTION "OpenCV main-module build list used by packaging automation. Contrib modules such as ml, img_hash, xfeatures2d, xobjdetect, quality, xphoto, optflow, bgsegm, tracking, plot, shape, line_descriptor, phase_unwrapping, structured_light, intensity_transform, fuzzy, hfs, reg, surface_matching, rapid, alphamat, bioinspired, and xstereo are added by scripts/Build-OpenCV.ps1 when -WithContrib is set.")
set(OPENCV_CSHARP_MINI_OPENCV_BUILD_LIST "core,imgproc,imgcodecs,videoio,geometry,flann")
set(OPENCV_CSHARP_RUNTIME_PROFILE "full" CACHE STRING "Native runtime profile: full or mini.")
set_property(CACHE OPENCV_CSHARP_RUNTIME_PROFILE PROPERTY STRINGS full mini)
string(TOLOWER "${OPENCV_CSHARP_RUNTIME_PROFILE}" OPENCV_CSHARP_RUNTIME_PROFILE)
if(NOT OPENCV_CSHARP_RUNTIME_PROFILE STREQUAL "full" AND NOT OPENCV_CSHARP_RUNTIME_PROFILE STREQUAL "mini")
    message(FATAL_ERROR "OPENCV_CSHARP_RUNTIME_PROFILE must be full or mini, got '${OPENCV_CSHARP_RUNTIME_PROFILE}'.")
endif()

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

if(OPENCV_CSHARP_RUNTIME_PROFILE STREQUAL "mini")
    if(NOT OPENCV_CSHARP_OPENCV_BUILD_LIST_WAS_DEFINED)
        set(OPENCV_CSHARP_OPENCV_BUILD_LIST "${OPENCV_CSHARP_MINI_OPENCV_BUILD_LIST}" CACHE STRING "${OPENCV_CSHARP_OPENCV_BUILD_LIST_DESCRIPTION}" FORCE)
    endif()

    if(NOT OPENCV_CSHARP_OPENCV_BUILD_LIST STREQUAL OPENCV_CSHARP_MINI_OPENCV_BUILD_LIST)
        message(FATAL_ERROR "The mini native profile requires OPENCV_CSHARP_OPENCV_BUILD_LIST=${OPENCV_CSHARP_MINI_OPENCV_BUILD_LIST}.")
    endif()
endif()

if(OPENCV_CSHARP_OPENCV_DIR)
    set(OpenCV_DIR "${OPENCV_CSHARP_OPENCV_DIR}" CACHE PATH "${OPENCV_CSHARP_OPENCV_DIR_DESCRIPTION}" FORCE)
    set(OPENCV_CSHARP_BUILD_WITH_OPENCV ON CACHE BOOL "Link against an installed OpenCV package." FORCE)
endif()

set(OPENCV_CSHARP_NATIVE_TARGET "JYPPX.OpenCV.Native")
set(OPENCV_CSHARP_NATIVE_TARGET_DISPLAY_NAME "${OPENCV_CSHARP_NATIVE_TARGET}")
set(OPENCV_CSHARP_NATIVE_SMOKE_TARGET "${OPENCV_CSHARP_NATIVE_TARGET}Smoke")
# JYPPX.OpenCV.Native is the version-neutral primary native target/output.
set(OPENCV_CSHARP_MINI_NATIVE_SOURCES
    src/error_state.cpp
    src/version.cpp
    src/core/mat.cpp
    src/core/decomp.cpp
    src/core/operations.cpp
    src/core/persistence.cpp
    src/videoio/videoio.cpp
    src/imgcodecs.cpp
    src/imgproc.cpp
)

set(OPENCV_CSHARP_FULL_ONLY_NATIVE_SOURCES
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
    src/photo/photo.cpp
    src/photo/hdr.cpp
    src/photo/ccm.cpp
    src/photo/intelligent_scissors.cpp
    src/features2d/features2d.cpp
    src/features2d/features2d_extra.cpp
)

if(OPENCV_CSHARP_RUNTIME_PROFILE STREQUAL "mini")
    set(OPENCV_CSHARP_NATIVE_ABI_MANIFEST generated/native_abi_mini_manifest.txt)
    set(OPENCV_CSHARP_NATIVE_SOURCES ${OPENCV_CSHARP_MINI_NATIVE_SOURCES})
else()
    set(OPENCV_CSHARP_NATIVE_ABI_MANIFEST generated/native_abi_manifest.txt)
    set(OPENCV_CSHARP_NATIVE_SOURCES
        ${OPENCV_CSHARP_MINI_NATIVE_SOURCES}
        ${OPENCV_CSHARP_FULL_ONLY_NATIVE_SOURCES}
    )
endif()

add_library(${OPENCV_CSHARP_NATIVE_TARGET} SHARED
    ${OPENCV_CSHARP_NATIVE_SOURCES}
)

target_compile_features(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE cxx_std_17)

target_include_directories(${OPENCV_CSHARP_NATIVE_TARGET}
    PUBLIC
        ${CMAKE_CURRENT_SOURCE_DIR}/include
)

target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET}
    PRIVATE
        OPENCV_CSHARP_NATIVE_EXPORTS
)

if(OPENCV_CSHARP_RUNTIME_PROFILE STREQUAL "mini")
    target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_RUNTIME_PROFILE_MINI=1)
else()
    target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_RUNTIME_PROFILE_FULL=1)
endif()

if(OPENCV_CSHARP_BUILD_WITH_OPENCV)
    if(OPENCV_CSHARP_RUNTIME_PROFILE STREQUAL "mini")
        find_package(OpenCV REQUIRED COMPONENTS core imgproc imgcodecs videoio geometry flann)
    else()
        find_package(OpenCV REQUIRED COMPONENTS core calib dnn geometry highgui imgcodecs imgproc objdetect photo stereo video videoio OPTIONAL_COMPONENTS features flann stitching ptcloud ml xfeatures2d xobjdetect quality xphoto img_hash optflow bgsegm ximgproc tracking face saliency plot shape line_descriptor phase_unwrapping structured_light intensity_transform fuzzy hfs reg surface_matching rapid alphamat bioinspired xstereo)
    endif()
    target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_GEOMETRY=1)
    target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE ${OpenCV_LIBS})
    target_include_directories(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE ${OpenCV_INCLUDE_DIRS})
    target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE
        OPENCV_CSHARP_HAS_OPENCV=1
        
    )

    if(OPENCV_CSHARP_RUNTIME_PROFILE STREQUAL "full")
        # OpenCV 5.0.0 renamed the old features2d module surface to the opencv_features target.
    if(TARGET opencv_features)
        target_link_libraries(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE opencv_features)
        target_compile_definitions(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE OPENCV_CSHARP_HAS_OPENCV_FEATURES=1 OPENCV_CSHARP_HAS_OPENCV_FEATURES2D=1 )
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

if(UNIX AND NOT APPLE AND OPENCV_CSHARP_BUILD_WITH_OPENCV)
    # Runtime NuGet packages place OpenCV dependencies beside the loader.
    set_target_properties(${OPENCV_CSHARP_NATIVE_TARGET} PROPERTIES
        BUILD_WITH_INSTALL_RPATH TRUE
        INSTALL_RPATH "\$ORIGIN"
    )

    # Keep the declared profile closure in DT_NEEDED so $ORIGIN resolves each
    # packaged module directly instead of relying on non-transitive RUNPATH.
    target_link_options(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE "LINKER:--no-as-needed")
endif()

if(ANDROID)
    target_link_options(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE
        "LINKER:-z,max-page-size=16384"
        "LINKER:-z,common-page-size=16384"
    )
endif()

enable_testing()

add_executable(${OPENCV_CSHARP_NATIVE_SMOKE_TARGET}
    tests/native_smoke.cpp
)

target_link_libraries(${OPENCV_CSHARP_NATIVE_SMOKE_TARGET} PRIVATE ${OPENCV_CSHARP_NATIVE_TARGET})
target_include_directories(${OPENCV_CSHARP_NATIVE_SMOKE_TARGET} PRIVATE ${CMAKE_CURRENT_SOURCE_DIR}/include)
if(OPENCV_CSHARP_RUNTIME_PROFILE STREQUAL "mini")
    target_compile_definitions(${OPENCV_CSHARP_NATIVE_SMOKE_TARGET} PRIVATE OPENCV_CSHARP_RUNTIME_PROFILE_MINI=1)
endif()

add_test(NAME ${OPENCV_CSHARP_NATIVE_SMOKE_TARGET} COMMAND ${OPENCV_CSHARP_NATIVE_SMOKE_TARGET})

set(
    OPENCV_CSHARP_NATIVE_RUNTIME_TESTS
    ${OPENCV_CSHARP_NATIVE_SMOKE_TARGET}
)

find_program(OPENCV_CSHARP_PWSH_EXECUTABLE NAMES pwsh)
if(OPENCV_CSHARP_PWSH_EXECUTABLE)
    add_test(
        NAME ${OPENCV_CSHARP_NATIVE_TARGET}AbiGeneratedCheck
        COMMAND
            ${OPENCV_CSHARP_PWSH_EXECUTABLE}
            -NoProfile
            -File
            ${CMAKE_CURRENT_SOURCE_DIR}/../../scripts/Generate-NativeAbiManifest.ps1
            -RepositoryRoot
            ${CMAKE_CURRENT_SOURCE_DIR}/../..
            -Check
    )

    set(OPENCV_CSHARP_NATIVE_ABI_EXPORT_TEST "${OPENCV_CSHARP_NATIVE_TARGET}AbiExportAudit")
    set(OPENCV_CSHARP_NATIVE_ABI_EXPORT_TOOL_ARGUMENTS)
    if(WIN32 AND MSVC AND CMAKE_LINKER)
        get_filename_component(OPENCV_CSHARP_MSVC_TOOL_DIRECTORY "${CMAKE_LINKER}" DIRECTORY)
        set(OPENCV_CSHARP_DUMPBIN_EXECUTABLE "${OPENCV_CSHARP_MSVC_TOOL_DIRECTORY}/dumpbin.exe")
        if(EXISTS "${OPENCV_CSHARP_DUMPBIN_EXECUTABLE}")
            list(APPEND
                OPENCV_CSHARP_NATIVE_ABI_EXPORT_TOOL_ARGUMENTS
                -ExportToolPath
                "${OPENCV_CSHARP_DUMPBIN_EXECUTABLE}")
        endif()
    endif()
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
            ${CMAKE_CURRENT_SOURCE_DIR}/${OPENCV_CSHARP_NATIVE_ABI_MANIFEST}
            ${OPENCV_CSHARP_NATIVE_ABI_EXPORT_TOOL_ARGUMENTS}
    )
    list(APPEND
        OPENCV_CSHARP_NATIVE_RUNTIME_TESTS
        ${OPENCV_CSHARP_NATIVE_ABI_EXPORT_TEST}
    )
else()
    message(WARNING "pwsh was not found. Native ABI generation and export checks are disabled.")
endif()

if(UNIX AND NOT APPLE AND OPENCV_CSHARP_BUILD_WITH_OPENCV AND TARGET opencv_core)
    get_target_property(OPENCV_CSHARP_OPENCV_CORE_RUNTIME opencv_core IMPORTED_LOCATION_RELEASE)
    if(NOT OPENCV_CSHARP_OPENCV_CORE_RUNTIME)
        get_target_property(OPENCV_CSHARP_OPENCV_CORE_RUNTIME opencv_core IMPORTED_LOCATION)
    endif()

    if(OPENCV_CSHARP_OPENCV_CORE_RUNTIME)
        get_filename_component(
            OPENCV_CSHARP_OPENCV_RUNTIME_DIRECTORY
            "${OPENCV_CSHARP_OPENCV_CORE_RUNTIME}"
            DIRECTORY
        )
        set_tests_properties(${OPENCV_CSHARP_NATIVE_RUNTIME_TESTS} PROPERTIES
            ENVIRONMENT "LD_LIBRARY_PATH=${OPENCV_CSHARP_OPENCV_RUNTIME_DIRECTORY}:$ENV{LD_LIBRARY_PATH}"
        )
    endif()
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
