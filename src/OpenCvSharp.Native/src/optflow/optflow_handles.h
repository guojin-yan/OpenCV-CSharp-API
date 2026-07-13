#pragma once

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
#include <opencv2/optflow.hpp>
#endif

struct jyppx_ocv_optflow_dense
{
    virtual ~jyppx_ocv_optflow_dense() = default;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
    cv::Ptr<cv::DenseOpticalFlow> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_optflow_sparse
{
    virtual ~jyppx_ocv_optflow_sparse() = default;

#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
    cv::Ptr<cv::SparseOpticalFlow> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_optflow_dual_tvl1 : jyppx_ocv_optflow_dense
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
    cv::Ptr<cv::optflow::DualTVL1OpticalFlow> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_optflow_rlof_parameter
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
    cv::Ptr<cv::optflow::RLOFOpticalFlowParameter> value;
#else
    int placeholder;
#endif
};

struct jyppx_ocv_optflow_dense_rlof : jyppx_ocv_optflow_dense
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
    cv::Ptr<cv::optflow::DenseRLOFOpticalFlow> concrete;
#else
    int placeholder2;
#endif
};

struct jyppx_ocv_optflow_sparse_rlof : jyppx_ocv_optflow_sparse
{
#if defined(OPENCV_CSHARP_HAS_OPENCV) && defined(OPENCV_CSHARP_HAS_OPENCV_OPTFLOW)
    cv::Ptr<cv::optflow::SparseRLOFOpticalFlow> concrete;
#else
    int placeholder2;
#endif
};
