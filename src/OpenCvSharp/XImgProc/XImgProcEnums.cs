using System;

namespace OpenCvSharp.XImgProc
{
    /// <summary>Thinning algorithms. 细化算法。</summary>
    public enum ThinningTypes
    {
        /// <summary>Zhang-Suen thinning. Zhang-Suen 细化。</summary>
        ZhangSuen = 0,

        /// <summary>Guo-Hall thinning. Guo-Hall 细化。</summary>
        GuoHall = 1
    }

    /// <summary>Local binarization methods for NiBlack thresholding. NiBlack 阈值的局部二值化方法。</summary>
    public enum LocalBinarizationMethods
    {
        /// <summary>Classic Niblack binarization. 经典 Niblack 二值化。</summary>
        NiBlack = 0,

        /// <summary>Sauvola binarization. Sauvola 二值化。</summary>
        Sauvola = 1,

        /// <summary>Wolf binarization. Wolf 二值化。</summary>
        Wolf = 2,

        /// <summary>NICK binarization. NICK 二值化。</summary>
        Nick = 3
    }

    /// <summary>Weighted median filter weight modes. 加权中值滤波权重模式。</summary>
    [Flags]
    public enum WeightedMedianFilterWeightType
    {
        /// <summary>Exponential range weight. 指数范围权重。</summary>
        Exp = 1,

        /// <summary>Inverse absolute-difference weight. 绝对差倒数权重。</summary>
        Iv1 = 1 << 1,

        /// <summary>Inverse squared-difference weight. 平方差倒数权重。</summary>
        Iv2 = 1 << 2,

        /// <summary>Cosine similarity weight. 余弦相似权重。</summary>
        Cos = 1 << 3,

        /// <summary>Jaccard-like color weight. 类 Jaccard 颜色权重。</summary>
        Jac = 1 << 4,

        /// <summary>Unweighted median filter. 非加权中值滤波。</summary>
        Off = 1 << 5
    }

    /// <summary>Superpixel SLIC algorithm variants. SLIC 超像素算法变体。</summary>
    public enum SLICType
    {
        /// <summary>Baseline SLIC. 基础 SLIC。</summary>
        SLIC = 100,

        /// <summary>Zero-parameter SLIC. 零参数 SLIC。</summary>
        SLICO = 101,

        /// <summary>Manifold SLIC. Manifold SLIC。</summary>
        MSLIC = 102
    }

    /// <summary>Domain-transform filter modes. Domain Transform 滤波模式。</summary>
    public enum DomainTransformFilterMode
    {
        /// <summary>Normalized convolution mode. 归一化卷积模式。</summary>
        NormalizedConvolution = 0,

        /// <summary>Interpolated convolution mode. 插值卷积模式。</summary>
        InterpolatedConvolution = 1,

        /// <summary>Recursive filtering mode. 递归滤波模式。</summary>
        RecursiveFiltering = 2
    }

    /// <summary>Fast Hough transform angle ranges. 快速 Hough 变换角度范围。</summary>
    public enum AngleRangeOption
    {
        /// <summary>0 to 45 degrees. 0 到 45 度。</summary>
        Aro0To45 = 0,

        /// <summary>45 to 90 degrees. 45 到 90 度。</summary>
        Aro45To90 = 1,

        /// <summary>90 to 135 degrees. 90 到 135 度。</summary>
        Aro90To135 = 2,

        /// <summary>315 to 0 degrees. 315 到 0 度。</summary>
        Aro315To0 = 3,

        /// <summary>315 to 45 degrees. 315 到 45 度。</summary>
        Aro315To45 = 4,

        /// <summary>45 to 135 degrees. 45 到 135 度。</summary>
        Aro45To135 = 5,

        /// <summary>Full 315 to 135 range. 完整 315 到 135 范围。</summary>
        Aro315To135 = 6,

        /// <summary>Centered horizontal range. 居中水平范围。</summary>
        AroCenteredHorizontal = 7,

        /// <summary>Centered vertical range. 居中垂直范围。</summary>
        AroCenteredVertical = 8
    }

    /// <summary>Fast Hough transform reduction operation. 快速 Hough 变换归约操作。</summary>
    public enum HoughOp
    {
        /// <summary>Minimum operation. 最小值操作。</summary>
        Min = 0,

        /// <summary>Maximum operation. 最大值操作。</summary>
        Max = 1,

        /// <summary>Add operation. 加法操作。</summary>
        Add = 2,

        /// <summary>Average operation. 平均值操作。</summary>
        Average = 3
    }

    /// <summary>Fast Hough deskew mode. 快速 Hough 去斜模式。</summary>
    public enum HoughDeskewOption
    {
        /// <summary>Use raw cyclic Hough image. 使用原始循环 Hough 图像。</summary>
        Raw = 0,

        /// <summary>Prepare a deskewed Hough image. 生成去斜 Hough 图像。</summary>
        Deskew = 1
    }

    /// <summary>Hough point-to-line validation rules. Hough 点转线段校验规则。</summary>
    [Flags]
    public enum RulesOption
    {
        /// <summary>Strict validation. 严格校验。</summary>
        Strict = 0,

        /// <summary>Ignore border validation. 忽略边界校验。</summary>
        IgnoreBorders = 1
    }

    /// <summary>EdgeDrawing gradient operators. EdgeDrawing 梯度算子。</summary>
    public enum EdgeDrawingGradientOperator
    {
        /// <summary>Prewitt operator. Prewitt 算子。</summary>
        Prewitt = 0,

        /// <summary>Sobel operator. Sobel 算子。</summary>
        Sobel = 1,

        /// <summary>Scharr operator. Scharr 算子。</summary>
        Scharr = 2,

        /// <summary>LSD-style operator. LSD 风格算子。</summary>
        Lsd = 3
    }
}
