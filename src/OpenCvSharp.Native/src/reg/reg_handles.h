#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
#include <opencv2/reg/map.hpp>
#include <opencv2/reg/mapaffine.hpp>
#include <opencv2/reg/mapprojec.hpp>
#include <opencv2/reg/mapshift.hpp>
#include <opencv2/reg/mapper.hpp>
#include <opencv2/reg/mappergradaffine.hpp>
#include <opencv2/reg/mappergradeuclid.hpp>
#include <opencv2/reg/mappergradproj.hpp>
#include <opencv2/reg/mappergradsimilar.hpp>
#include <opencv2/reg/mappergradshift.hpp>
#include <opencv2/reg/mapperpyramid.hpp>
#endif

struct jyppx_ocv_reg_map
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
    cv::Ptr<cv::reg::Map> value;
    int kind;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_reg_mapper
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_REG)
    cv::Ptr<cv::reg::Mapper> value;
    cv::Ptr<cv::reg::Mapper> base_mapper;
    int kind;
#else
    int placeholder;
#endif
};
