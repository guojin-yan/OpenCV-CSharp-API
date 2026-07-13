using System;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using OpenCvSharp.ImgProc;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.XImgProc
{
    /// <summary>
    /// Static entry points for OpenCV contrib ximgproc.
    /// OpenCV contrib ximgproc 静态入口。
    /// </summary>
    public static class XImgProcCv2
    {
        /// <summary>Runs local NiBlack-family thresholding. 执行 NiBlack 系列局部阈值。</summary>
        public static void NiBlackThreshold(Mat src, Mat dst, double maxValue, ThresholdTypes type, int blockSize, double k, LocalBinarizationMethods binarizationMethod = LocalBinarizationMethods.NiBlack, double r = 128.0)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateLocalBinarizationMethod(binarizationMethod, nameof(binarizationMethod));
            ValidateNiBlackThresholdOutput(src, dst);
            ValidateNiBlackThresholdArguments(src, type, blockSize, binarizationMethod, r);
            NativeException.ThrowIfError(NativeMethods.XImgProcNiBlackThreshold(src.NativeHandle, dst.NativeHandle, maxValue, (int)type, blockSize, k, (int)binarizationMethod, r));
        }

        /// <summary>Runs local NiBlack-family thresholding and returns a new matrix. 执行 NiBlack 系列局部阈值并返回新矩阵。</summary>
        public static Mat NiBlackThreshold(Mat src, double maxValue, ThresholdTypes type, int blockSize, double k, LocalBinarizationMethods binarizationMethod = LocalBinarizationMethods.NiBlack, double r = 128.0)
        {
            return CreateOutput(delegate (Mat dst) { NiBlackThreshold(src, dst, maxValue, type, blockSize, k, binarizationMethod, r); });
        }

        /// <summary>Applies binary thinning. 执行二值图细化。</summary>
        public static void Thinning(Mat src, Mat dst, ThinningTypes thinningType = ThinningTypes.ZhangSuen)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateThinningType(thinningType, nameof(thinningType));
            ValidateThinningSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcThinning(src.NativeHandle, dst.NativeHandle, (int)thinningType));
        }

        /// <summary>Applies binary thinning and returns a new matrix. 执行二值图细化并返回新矩阵。</summary>
        public static Mat Thinning(Mat src, ThinningTypes thinningType = ThinningTypes.ZhangSuen)
        {
            return CreateOutput(delegate (Mat dst) { Thinning(src, dst, thinningType); });
        }

        /// <summary>Runs anisotropic diffusion. 执行各向异性扩散。</summary>
        public static void AnisotropicDiffusion(Mat src, Mat dst, float alpha, float k, int niters)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateAnisotropicDiffusionArguments(src, alpha, k, niters);
            NativeException.ThrowIfError(NativeMethods.XImgProcAnisotropicDiffusion(src.NativeHandle, dst.NativeHandle, alpha, k, niters));
        }

        /// <summary>Runs anisotropic diffusion and returns a new matrix. 执行各向异性扩散并返回新矩阵。</summary>
        public static Mat AnisotropicDiffusion(Mat src, float alpha, float k, int niters)
        {
            return CreateOutput(delegate (Mat dst) { AnisotropicDiffusion(src, dst, alpha, k, niters); });
        }

        /// <summary>Applies joint bilateral filtering. 应用联合双边滤波。</summary>
        public static void JointBilateralFilter(Mat joint, Mat src, Mat dst, int d, double sigmaColor, double sigmaSpace, BorderTypes borderType = BorderTypes.Default)
        {
            ValidateNotNull(joint, nameof(joint));
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateJointBilateralFilterArguments(joint, src);
            NativeException.ThrowIfError(NativeMethods.XImgProcJointBilateralFilter(joint.NativeHandle, src.NativeHandle, dst.NativeHandle, d, sigmaColor, sigmaSpace, (int)borderType));
        }

        /// <summary>Applies joint bilateral filtering and returns a new matrix. 应用联合双边滤波并返回新矩阵。</summary>
        public static Mat JointBilateralFilter(Mat joint, Mat src, int d, double sigmaColor, double sigmaSpace, BorderTypes borderType = BorderTypes.Default)
        {
            return CreateOutput(delegate (Mat dst) { JointBilateralFilter(joint, src, dst, d, sigmaColor, sigmaSpace, borderType); });
        }

        /// <summary>Applies one-shot guided filtering. 应用一次性 guided filter。</summary>
        public static void GuidedFilter(Mat guide, Mat src, Mat dst, int radius, double eps, int dDepth = -1, double scale = 1.0)
        {
            ValidateNotNull(guide, nameof(guide));
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateGuidedFilterCreateArguments(guide, radius, eps, scale);
            ValidateGuidedFilterSource(src, guide.Rows, guide.Cols);
            NativeException.ThrowIfError(NativeMethods.XImgProcGuidedFilter(guide.NativeHandle, src.NativeHandle, dst.NativeHandle, radius, eps, dDepth, scale));
        }

        /// <summary>Applies one-shot guided filtering and returns a new matrix. 应用一次性 guided filter 并返回新矩阵。</summary>
        public static Mat GuidedFilter(Mat guide, Mat src, int radius, double eps, int dDepth = -1, double scale = 1.0)
        {
            return CreateOutput(delegate (Mat dst) { GuidedFilter(guide, src, dst, radius, eps, dDepth, scale); });
        }

        /// <summary>Applies rolling guidance filtering. 应用 rolling guidance 滤波。</summary>
        public static void RollingGuidanceFilter(Mat src, Mat dst, int d = -1, double sigmaColor = 25.0, double sigmaSpace = 3.0, int numOfIter = 4, BorderTypes borderType = BorderTypes.Default)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateRollingGuidanceFilterSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcRollingGuidanceFilter(src.NativeHandle, dst.NativeHandle, d, sigmaColor, sigmaSpace, numOfIter, (int)borderType));
        }

        /// <summary>Applies rolling guidance filtering and returns a new matrix. 应用 rolling guidance 滤波并返回新矩阵。</summary>
        public static Mat RollingGuidanceFilter(Mat src, int d = -1, double sigmaColor = 25.0, double sigmaSpace = 3.0, int numOfIter = 4, BorderTypes borderType = BorderTypes.Default)
        {
            return CreateOutput(delegate (Mat dst) { RollingGuidanceFilter(src, dst, d, sigmaColor, sigmaSpace, numOfIter, borderType); });
        }

        /// <summary>Applies weighted median filtering. 应用加权中值滤波。</summary>
        public static void WeightedMedianFilter(Mat joint, Mat src, Mat dst, int r, double sigma = 25.5, WeightedMedianFilterWeightType weightType = WeightedMedianFilterWeightType.Exp, Mat? mask = null)
        {
            ValidateNotNull(joint, nameof(joint));
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateWeightedMedianFilterWeightType(weightType, nameof(weightType));
            ValidateWeightedMedianFilterArguments(joint, src, r, sigma);
            NativeException.ThrowIfError(NativeMethods.XImgProcWeightedMedianFilter(joint.NativeHandle, src.NativeHandle, dst.NativeHandle, r, sigma, (int)weightType, OptionalMatHandle(mask)));
        }

        /// <summary>Applies weighted median filtering and returns a new matrix. 应用加权中值滤波并返回新矩阵。</summary>
        public static Mat WeightedMedianFilter(Mat joint, Mat src, int r, double sigma = 25.5, WeightedMedianFilterWeightType weightType = WeightedMedianFilterWeightType.Exp, Mat? mask = null)
        {
            return CreateOutput(delegate (Mat dst) { WeightedMedianFilter(joint, src, dst, r, sigma, weightType, mask); });
        }

        /// <summary>Applies one-shot domain-transform filtering. 应用一次性 Domain Transform 滤波。</summary>
        public static void DtFilter(Mat guide, Mat src, Mat dst, double sigmaSpatial, double sigmaColor, DomainTransformFilterMode mode = DomainTransformFilterMode.NormalizedConvolution, int numIters = 3)
        {
            ValidateNotNull(guide, nameof(guide));
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateDomainTransformFilterMode(mode, nameof(mode));
            NativeException.ThrowIfError(NativeMethods.XImgProcDtFilter(guide.NativeHandle, src.NativeHandle, dst.NativeHandle, sigmaSpatial, sigmaColor, (int)mode, numIters));
        }

        /// <summary>Applies one-shot domain-transform filtering and returns a new matrix. 应用一次性 Domain Transform 滤波并返回新矩阵。</summary>
        public static Mat DtFilter(Mat guide, Mat src, double sigmaSpatial, double sigmaColor, DomainTransformFilterMode mode = DomainTransformFilterMode.NormalizedConvolution, int numIters = 3)
        {
            return CreateOutput(delegate (Mat dst) { DtFilter(guide, src, dst, sigmaSpatial, sigmaColor, mode, numIters); });
        }

        /// <summary>Applies adaptive-manifold filtering. 应用 adaptive manifold 滤波。</summary>
        public static void AmFilter(Mat joint, Mat src, Mat dst, double sigmaS, double sigmaR, bool adjustOutliers = false)
        {
            ValidateNotNull(joint, nameof(joint));
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateAdaptiveManifoldFilterArguments(sigmaS, sigmaR);
            ValidateAdaptiveManifoldFilterInput(joint, src);
            NativeException.ThrowIfError(NativeMethods.XImgProcAmFilter(joint.NativeHandle, src.NativeHandle, dst.NativeHandle, sigmaS, sigmaR, adjustOutliers ? 1 : 0));
        }

        /// <summary>Applies adaptive-manifold filtering and returns a new matrix. 应用 adaptive manifold 滤波并返回新矩阵。</summary>
        public static Mat AmFilter(Mat joint, Mat src, double sigmaS, double sigmaR, bool adjustOutliers = false)
        {
            return CreateOutput(delegate (Mat dst) { AmFilter(joint, src, dst, sigmaS, sigmaR, adjustOutliers); });
        }

        /// <summary>Applies bilateral texture filtering. 应用 bilateral texture 滤波。</summary>
        public static void BilateralTextureFilter(Mat src, Mat dst, int fr = 3, int numIter = 1, double sigmaAlpha = -1.0, double sigmaAvg = -1.0)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateBilateralTextureFilterArguments(src, fr, numIter);
            NativeException.ThrowIfError(NativeMethods.XImgProcBilateralTextureFilter(src.NativeHandle, dst.NativeHandle, fr, numIter, sigmaAlpha, sigmaAvg));
        }

        /// <summary>Applies bilateral texture filtering and returns a new matrix. 应用 bilateral texture 滤波并返回新矩阵。</summary>
        public static Mat BilateralTextureFilter(Mat src, int fr = 3, int numIter = 1, double sigmaAlpha = -1.0, double sigmaAvg = -1.0)
        {
            return CreateOutput(delegate (Mat dst) { BilateralTextureFilter(src, dst, fr, numIter, sigmaAlpha, sigmaAvg); });
        }

        /// <summary>Applies edge-preserving denoising. 应用边缘保持去噪。</summary>
        public static void EdgePreservingFilter(Mat src, Mat dst, int d, double threshold)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateEdgePreservingFilterSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcEdgePreservingFilter(src.NativeHandle, dst.NativeHandle, d, threshold));
        }

        /// <summary>Applies edge-preserving denoising and returns a new matrix. 应用边缘保持去噪并返回新矩阵。</summary>
        public static Mat EdgePreservingFilter(Mat src, int d, double threshold)
        {
            return CreateOutput(delegate (Mat dst) { EdgePreservingFilter(src, dst, d, threshold); });
        }

        /// <summary>Applies one-shot fast global smoother filtering. 应用一次性 fast global smoother 滤波。</summary>
        public static void FastGlobalSmootherFilter(Mat guide, Mat src, Mat dst, double lambda, double sigmaColor, double lambdaAttenuation = 0.25, int numIter = 3)
        {
            ValidateNotNull(guide, nameof(guide));
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateFastGlobalSmootherCreateArguments(guide, lambda, sigmaColor, numIter);
            ValidateFastGlobalSmootherFilterSource(src, guide.Rows, guide.Cols);
            NativeException.ThrowIfError(NativeMethods.XImgProcFastGlobalSmootherFilterRun(guide.NativeHandle, src.NativeHandle, dst.NativeHandle, lambda, sigmaColor, lambdaAttenuation, numIter));
        }

        /// <summary>Applies one-shot fast global smoother filtering and returns a new matrix. 应用一次性 fast global smoother 滤波并返回新矩阵。</summary>
        public static Mat FastGlobalSmootherFilter(Mat guide, Mat src, double lambda, double sigmaColor, double lambdaAttenuation = 0.25, int numIter = 3)
        {
            return CreateOutput(delegate (Mat dst) { FastGlobalSmootherFilter(guide, src, dst, lambda, sigmaColor, lambdaAttenuation, numIter); });
        }

        /// <summary>Applies L0 gradient smoothing. 应用 L0 梯度平滑。</summary>
        public static void L0Smooth(Mat src, Mat dst, double lambda = 0.02, double kappa = 2.0)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateL0SmoothArguments(src, lambda, kappa);
            NativeException.ThrowIfError(NativeMethods.XImgProcL0Smooth(src.NativeHandle, dst.NativeHandle, lambda, kappa));
        }

        /// <summary>Applies L0 gradient smoothing and returns a new matrix. 应用 L0 梯度平滑并返回新矩阵。</summary>
        public static Mat L0Smooth(Mat src, double lambda = 0.02, double kappa = 2.0)
        {
            return CreateOutput(delegate (Mat dst) { L0Smooth(src, dst, lambda, kappa); });
        }

        /// <summary>Computes a fast Hough transform. 计算快速 Hough 变换。</summary>
        public static void FastHoughTransform(Mat src, Mat dst, int dstMatDepth, AngleRangeOption angleRange = AngleRangeOption.Aro315To135, HoughOp op = HoughOp.Add, HoughDeskewOption makeSkew = HoughDeskewOption.Deskew)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateAngleRangeOption(angleRange, nameof(angleRange));
            ValidateHoughOp(op, nameof(op));
            ValidateHoughDeskewOption(makeSkew, nameof(makeSkew));
            ValidateFastHoughTransformSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcFastHoughTransform(src.NativeHandle, dst.NativeHandle, dstMatDepth, (int)angleRange, (int)op, (int)makeSkew));
        }

        /// <summary>Computes a fast Hough transform and returns a new matrix. 计算快速 Hough 变换并返回新矩阵。</summary>
        public static Mat FastHoughTransform(Mat src, int dstMatDepth, AngleRangeOption angleRange = AngleRangeOption.Aro315To135, HoughOp op = HoughOp.Add, HoughDeskewOption makeSkew = HoughDeskewOption.Deskew)
        {
            return CreateOutput(delegate (Mat dst) { FastHoughTransform(src, dst, dstMatDepth, angleRange, op, makeSkew); });
        }

        /// <summary>Converts a point in Hough space to a line segment. 将 Hough 空间点转换为线段。</summary>
        public static LineSegment HoughPointToLine(int houghX, int houghY, Mat srcImgInfo, AngleRangeOption angleRange = AngleRangeOption.Aro315To135, HoughDeskewOption makeSkew = HoughDeskewOption.Deskew, RulesOption rules = RulesOption.IgnoreBorders)
        {
            ValidateNotNull(srcImgInfo, nameof(srcImgInfo));
            ValidateAngleRangeOption(angleRange, nameof(angleRange));
            ValidateHoughDeskewOption(makeSkew, nameof(makeSkew));
            ValidateRulesOption(rules, nameof(rules));
            NativeException.ThrowIfError(NativeMethods.XImgProcHoughPointToLine(houghX, houghY, srcImgInfo.NativeHandle, (int)angleRange, (int)makeSkew, (int)rules, out int x1, out int y1, out int x2, out int y2));
            return new LineSegment(x1, y1, x2, y2);
        }

        /// <summary>Computes Pei-Lin normalization transform. 计算 Pei-Lin 归一化变换。</summary>
        public static void PeiLinNormalization(Mat src, Mat dst)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            NativeException.ThrowIfError(NativeMethods.XImgProcPeiLinNormalization(src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>Computes Pei-Lin normalization transform and returns a new matrix. 计算 Pei-Lin 归一化变换并返回新矩阵。</summary>
        public static Mat PeiLinNormalization(Mat src)
        {
            return CreateOutput(delegate (Mat dst) { PeiLinNormalization(src, dst); });
        }

        /// <summary>Computes the X gradient with Deriche filtering. 使用 Deriche 滤波计算 X 梯度。</summary>
        public static void GradientDericheX(Mat src, Mat dst, double alpha, double omega)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateDerichePaillouGradientSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcGradientDericheX(src.NativeHandle, dst.NativeHandle, alpha, omega));
        }

        /// <summary>Computes the X gradient with Deriche filtering and returns a new matrix. 使用 Deriche 滤波计算 X 梯度并返回新矩阵。</summary>
        public static Mat GradientDericheX(Mat src, double alpha, double omega)
        {
            return CreateOutput(delegate (Mat dst) { GradientDericheX(src, dst, alpha, omega); });
        }

        /// <summary>Computes the Y gradient with Deriche filtering. 使用 Deriche 滤波计算 Y 梯度。</summary>
        public static void GradientDericheY(Mat src, Mat dst, double alpha, double omega)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateDerichePaillouGradientSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcGradientDericheY(src.NativeHandle, dst.NativeHandle, alpha, omega));
        }

        /// <summary>Computes the Y gradient with Deriche filtering and returns a new matrix. 使用 Deriche 滤波计算 Y 梯度并返回新矩阵。</summary>
        public static Mat GradientDericheY(Mat src, double alpha, double omega)
        {
            return CreateOutput(delegate (Mat dst) { GradientDericheY(src, dst, alpha, omega); });
        }

        /// <summary>Computes the X gradient with Paillou filtering. 使用 Paillou 滤波计算 X 梯度。</summary>
        public static void GradientPaillouX(Mat src, Mat dst, double alpha, double omega)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateDerichePaillouGradientSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcGradientPaillouX(src.NativeHandle, dst.NativeHandle, alpha, omega));
        }

        /// <summary>Computes the X gradient with Paillou filtering and returns a new matrix. 使用 Paillou 滤波计算 X 梯度并返回新矩阵。</summary>
        public static Mat GradientPaillouX(Mat src, double alpha, double omega)
        {
            return CreateOutput(delegate (Mat dst) { GradientPaillouX(src, dst, alpha, omega); });
        }

        /// <summary>Computes the Y gradient with Paillou filtering. 使用 Paillou 滤波计算 Y 梯度。</summary>
        public static void GradientPaillouY(Mat src, Mat dst, double alpha, double omega)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateDerichePaillouGradientSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcGradientPaillouY(src.NativeHandle, dst.NativeHandle, alpha, omega));
        }

        /// <summary>Computes the Y gradient with Paillou filtering and returns a new matrix. 使用 Paillou 滤波计算 Y 梯度并返回新矩阵。</summary>
        public static Mat GradientPaillouY(Mat src, double alpha, double omega)
        {
            return CreateOutput(delegate (Mat dst) { GradientPaillouY(src, dst, alpha, omega); });
        }

        /// <summary>Computes Fourier descriptors for a closed contour. 计算闭合轮廓的 Fourier descriptors。</summary>
        public static void FourierDescriptor(Mat src, Mat dst, int nbElt = -1, int nbFD = -1)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateFourierDescriptorArguments(nbElt, nbFD);
            NativeException.ThrowIfError(NativeMethods.XImgProcFourierDescriptor(src.NativeHandle, dst.NativeHandle, nbElt, nbFD));
        }

        /// <summary>Computes Fourier descriptors and returns a new matrix. 计算 Fourier descriptors 并返回新矩阵。</summary>
        public static Mat FourierDescriptor(Mat src, int nbElt = -1, int nbFD = -1)
        {
            return CreateOutput(delegate (Mat dst) { FourierDescriptor(src, dst, nbElt, nbFD); });
        }

        /// <summary>Transforms a contour or Fourier descriptor matrix. 变换轮廓或 Fourier descriptor 矩阵。</summary>
        public static void TransformFD(Mat src, Mat transform, Mat dst, bool fdContour = true)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(transform, nameof(transform));
            ValidateNotNull(dst, nameof(dst));
            ValidateTransformFDTransform(transform);
            NativeException.ThrowIfError(NativeMethods.XImgProcTransformFD(src.NativeHandle, transform.NativeHandle, dst.NativeHandle, fdContour ? 1 : 0));
        }

        /// <summary>Transforms a contour or Fourier descriptor matrix and returns a new matrix. 变换轮廓或 Fourier descriptor 矩阵并返回新矩阵。</summary>
        public static Mat TransformFD(Mat src, Mat transform, bool fdContour = true)
        {
            return CreateOutput(delegate (Mat dst) { TransformFD(src, transform, dst, fdContour); });
        }

        /// <summary>Samples a contour to a fixed number of points. 将轮廓重采样为固定数量的点。</summary>
        public static void ContourSampling(Mat src, Mat dst, int nbElt)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateContourSamplingElementCount(nbElt);
            NativeException.ThrowIfError(NativeMethods.XImgProcContourSampling(src.NativeHandle, dst.NativeHandle, nbElt));
        }

        /// <summary>Samples a contour and returns a new matrix. 重采样轮廓并返回新矩阵。</summary>
        public static Mat ContourSampling(Mat src, int nbElt)
        {
            return CreateOutput(delegate (Mat dst) { ContourSampling(src, dst, nbElt); });
        }

        /// <summary>Computes covariance estimation for a complex image. 对复数图像计算 covariance estimation。</summary>
        public static void CovarianceEstimation(Mat src, Mat dst, int windowRows, int windowCols)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateCovarianceEstimationSource(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcCovarianceEstimation(src.NativeHandle, dst.NativeHandle, windowRows, windowCols));
        }

        /// <summary>Computes covariance estimation and returns a new matrix. 计算 covariance estimation 并返回新矩阵。</summary>
        public static Mat CovarianceEstimation(Mat src, int windowRows, int windowCols)
        {
            return CreateOutput(delegate (Mat dst) { CovarianceEstimation(src, dst, windowRows, windowCols); });
        }

        /// <summary>Creates a clamped disparity visualization. 创建裁剪后的 disparity 可视化图。</summary>
        public static void GetDisparityVis(Mat src, Mat dst, double scale = 1.0)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateDisparityMapForVisualization(src);
            NativeException.ThrowIfError(NativeMethods.XImgProcGetDisparityVis(src.NativeHandle, dst.NativeHandle, scale));
        }

        /// <summary>Creates a clamped disparity visualization and returns a new matrix. 创建 disparity 可视化图并返回新矩阵。</summary>
        public static Mat GetDisparityVis(Mat src, double scale = 1.0)
        {
            return CreateOutput(delegate (Mat dst) { GetDisparityVis(src, dst, scale); });
        }

        /// <summary>Computes disparity mean square error over an ROI. 计算 ROI 上的 disparity 均方误差。</summary>
        public static double ComputeMSE(Mat gt, Mat src, Rect roi)
        {
            ValidateNotNull(gt, nameof(gt));
            ValidateNotNull(src, nameof(src));
            ValidateDisparityEvaluationInputs(gt, src);
            NativeMethods.XImgProcRectNative nativeRoi = NativeXImgProcConvert.ToNative(roi);
            NativeException.ThrowIfError(NativeMethods.XImgProcComputeMSE(gt.NativeHandle, src.NativeHandle, ref nativeRoi, out double value));
            return value;
        }

        /// <summary>Computes bad-pixel percent over an ROI. 计算 ROI 上的坏点比例。</summary>
        public static double ComputeBadPixelPercent(Mat gt, Mat src, Rect roi, int thresh = 24)
        {
            ValidateNotNull(gt, nameof(gt));
            ValidateNotNull(src, nameof(src));
            ValidateDisparityEvaluationInputs(gt, src);
            NativeMethods.XImgProcRectNative nativeRoi = NativeXImgProcConvert.ToNative(roi);
            NativeException.ThrowIfError(NativeMethods.XImgProcComputeBadPixelPercent(gt.NativeHandle, src.NativeHandle, ref nativeRoi, thresh, out double value));
            return value;
        }

        /// <summary>Applies one-shot fast bilateral solver filtering. 应用一次性 fast bilateral solver 滤波。</summary>
        public static void FastBilateralSolverFilter(Mat guide, Mat src, Mat confidence, Mat dst, double sigmaSpatial = 8.0, double sigmaLuma = 8.0, double sigmaChroma = 8.0, double lambda = 128.0, int numIter = 25, double maxTol = 1e-5)
        {
            ValidateNotNull(guide, nameof(guide));
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(confidence, nameof(confidence));
            ValidateNotNull(dst, nameof(dst));
            ValidateFastBilateralSolverCreateArguments(guide);
            ValidateFastBilateralSolverFilterSource(src, guide.Rows, guide.Cols);
            ValidateFastBilateralSolverConfidence(confidence, guide.Rows, guide.Cols);
            NativeException.ThrowIfError(NativeMethods.XImgProcFastBilateralSolverFilterRun(guide.NativeHandle, src.NativeHandle, confidence.NativeHandle, dst.NativeHandle, sigmaSpatial, sigmaLuma, sigmaChroma, lambda, numIter, maxTol));
        }

        /// <summary>Applies one-shot fast bilateral solver filtering and returns a new matrix. 应用一次性 fast bilateral solver 滤波并返回新矩阵。</summary>
        public static Mat FastBilateralSolverFilter(Mat guide, Mat src, Mat confidence, double sigmaSpatial = 8.0, double sigmaLuma = 8.0, double sigmaChroma = 8.0, double lambda = 128.0, int numIter = 25, double maxTol = 1e-5)
        {
            return CreateOutput(delegate (Mat dst) { FastBilateralSolverFilter(guide, src, confidence, dst, sigmaSpatial, sigmaLuma, sigmaChroma, lambda, numIter, maxTol); });
        }

        /// <summary>Creates a reusable guided filter. 创建可复用 guided filter。</summary>
        public static OpenCvSharp.XImgProc.GuidedFilter CreateGuidedFilter(Mat guide, int radius, double eps, double scale = 1.0)
        {
            return OpenCvSharp.XImgProc.GuidedFilter.Create(guide, radius, eps, scale);
        }

        /// <summary>Creates a reusable fast global smoother filter. 创建可复用 fast global smoother filter。</summary>
        public static OpenCvSharp.XImgProc.FastGlobalSmootherFilter CreateFastGlobalSmootherFilter(Mat guide, double lambda, double sigmaColor, double lambdaAttenuation = 0.25, int numIter = 3)
        {
            return OpenCvSharp.XImgProc.FastGlobalSmootherFilter.Create(guide, lambda, sigmaColor, lambdaAttenuation, numIter);
        }

        /// <summary>Creates a SLIC superpixel segmenter. 创建 SLIC 超像素分割器。</summary>
        public static SuperpixelSLIC CreateSuperpixelSLIC(Mat image, SLICType algorithm = SLICType.SLICO, int regionSize = 10, float ruler = 10.0F)
        {
            return SuperpixelSLIC.Create(image, algorithm, regionSize, ruler);
        }

        /// <summary>Creates a SEEDS superpixel segmenter. 创建 SEEDS 超像素分割器。</summary>
        public static SuperpixelSEEDS CreateSuperpixelSEEDS(int imageWidth, int imageHeight, int imageChannels, int numSuperpixels, int numLevels, int prior = 2, int histogramBins = 5, bool doubleStep = false)
        {
            return SuperpixelSEEDS.Create(imageWidth, imageHeight, imageChannels, numSuperpixels, numLevels, prior, histogramBins, doubleStep);
        }

        /// <summary>Creates an LSC superpixel segmenter. 创建 LSC 超像素分割器。</summary>
        public static SuperpixelLSC CreateSuperpixelLSC(Mat image, int regionSize = 10, float ratio = 0.075F)
        {
            return SuperpixelLSC.Create(image, regionSize, ratio);
        }

        /// <summary>Creates a fast line detector. 创建快速线段检测器。</summary>
        public static FastLineDetector CreateFastLineDetector(int lengthThreshold = 10, float distanceThreshold = 1.414213562F, double cannyTh1 = 50.0, double cannyTh2 = 50.0, int cannyApertureSize = 3, bool doMerge = false)
        {
            return FastLineDetector.Create(lengthThreshold, distanceThreshold, cannyTh1, cannyTh2, cannyApertureSize, doMerge);
        }

        /// <summary>Creates a generic WLS disparity filter. 创建 generic WLS disparity 滤波器。</summary>
        public static DisparityWLSFilter CreateDisparityWLSFilterGeneric(bool useConfidence = false)
        {
            return DisparityWLSFilter.CreateGeneric(useConfidence);
        }

        /// <summary>Creates a confidence-enabled WLS filter from a StereoBM matcher. 从 StereoBM matcher 创建启用置信度的 WLS filter。</summary>
        public static DisparityWLSFilter CreateDisparityWLSFilter(StereoBM matcherLeft)
        {
            ValidateNotNull(matcherLeft, nameof(matcherLeft));
            NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterCreateFromStereoBM(
                matcherLeft.NativeHandle,
                out IntPtr nativeHandle));
            return new DisparityWLSFilter(nativeHandle, true);
        }

        /// <summary>Creates a confidence-enabled WLS filter from a StereoSGBM matcher. 从 StereoSGBM matcher 创建启用置信度的 WLS filter。</summary>
        public static DisparityWLSFilter CreateDisparityWLSFilter(StereoSGBM matcherLeft)
        {
            ValidateNotNull(matcherLeft, nameof(matcherLeft));
            NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterCreateFromStereoSGBM(
                matcherLeft.NativeHandle,
                out IntPtr nativeHandle));
            return new DisparityWLSFilter(nativeHandle, true);
        }

        /// <summary>Creates a confidence-enabled WLS filter from an owned generic matcher. 从 owned 通用 matcher 创建启用置信度的 WLS filter。</summary>
        public static DisparityWLSFilter CreateDisparityWLSFilter(StereoMatcher matcherLeft)
        {
            ValidateNotNull(matcherLeft, nameof(matcherLeft));
            NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterCreateFromStereoMatcher(
                matcherLeft.NativeHandle,
                out IntPtr nativeHandle));
            return new DisparityWLSFilter(nativeHandle, true);
        }

        /// <summary>Creates an owned right-view matcher from StereoBM. 从 StereoBM 创建 owned 右视图 matcher。</summary>
        public static StereoMatcher CreateRightMatcher(StereoBM matcherLeft)
        {
            ValidateNotNull(matcherLeft, nameof(matcherLeft));
            NativeException.ThrowIfError(NativeMethods.XImgProcCreateRightMatcherFromStereoBM(
                matcherLeft.NativeHandle,
                out IntPtr nativeHandle));
            return StereoMatcher.FromNativePointer(nativeHandle, false);
        }

        /// <summary>Creates an owned right-view matcher from StereoSGBM. 从 StereoSGBM 创建 owned 右视图 matcher。</summary>
        public static StereoMatcher CreateRightMatcher(StereoSGBM matcherLeft)
        {
            ValidateNotNull(matcherLeft, nameof(matcherLeft));
            NativeException.ThrowIfError(NativeMethods.XImgProcCreateRightMatcherFromStereoSGBM(
                matcherLeft.NativeHandle,
                out IntPtr nativeHandle));
            return StereoMatcher.FromNativePointer(nativeHandle, true);
        }

        /// <summary>Creates another owned right-view matcher from a generic matcher. 从通用 matcher 创建另一个 owned 右视图 matcher。</summary>
        public static StereoMatcher CreateRightMatcher(StereoMatcher matcherLeft)
        {
            ValidateNotNull(matcherLeft, nameof(matcherLeft));
            bool supportsColor = matcherLeft.SupportsColor;
            NativeException.ThrowIfError(NativeMethods.XImgProcCreateRightMatcherFromStereoMatcher(
                matcherLeft.NativeHandle,
                out IntPtr nativeHandle));
            return StereoMatcher.FromNativePointer(nativeHandle, supportsColor);
        }

        /// <summary>Creates a reusable fast bilateral solver filter. 创建可复用 fast bilateral solver filter。</summary>
        public static OpenCvSharp.XImgProc.FastBilateralSolverFilter CreateFastBilateralSolverFilter(Mat guide, double sigmaSpatial, double sigmaLuma, double sigmaChroma, double lambda = 128.0, int numIter = 25, double maxTol = 1e-5)
        {
            return OpenCvSharp.XImgProc.FastBilateralSolverFilter.Create(guide, sigmaSpatial, sigmaLuma, sigmaChroma, lambda, numIter, maxTol);
        }

        /// <summary>Creates an edge-aware sparse match interpolator. 创建边缘感知稀疏匹配插值器。</summary>
        public static EdgeAwareInterpolator CreateEdgeAwareInterpolator()
        {
            return EdgeAwareInterpolator.Create();
        }

        /// <summary>Creates a RIC sparse match interpolator. 创建 RIC 稀疏匹配插值器。</summary>
        public static RICInterpolator CreateRICInterpolator()
        {
            return RICInterpolator.Create();
        }

        /// <summary>Creates an EdgeDrawing detector. 创建 EdgeDrawing 检测器。</summary>
        public static EdgeDrawing CreateEdgeDrawing()
        {
            return EdgeDrawing.Create();
        }

        /// <summary>Creates an EdgeBoxes proposal generator. 创建 EdgeBoxes 候选框生成器。</summary>
        public static EdgeBoxes CreateEdgeBoxes(float alpha = 0.65F, float beta = 0.75F, float eta = 1.0F, float minScore = 0.01F, int maxBoxes = 10000, float edgeMinMag = 0.1F, float edgeMergeThr = 0.5F, float clusterMinMag = 0.5F, float maxAspectRatio = 3.0F, float minBoxArea = 1000.0F, float gamma = 2.0F, float kappa = 1.5F)
        {
            return EdgeBoxes.Create(alpha, beta, eta, minScore, maxBoxes, edgeMinMag, edgeMergeThr, clusterMinMag, maxAspectRatio, minBoxArea, gamma, kappa);
        }

        /// <summary>Creates a ridge detection filter. 创建脊线检测滤波器。</summary>
        public static RidgeDetectionFilter CreateRidgeDetectionFilter(int ddepth = MatType.CV_32FC1, int dx = 1, int dy = 1, int ksize = 3, int outDtype = MatType.CV_8UC1, double scale = 1.0, double delta = 0.0, BorderTypes borderType = BorderTypes.Default)
        {
            return RidgeDetectionFilter.Create(ddepth, dx, dy, ksize, outDtype, scale, delta, borderType);
        }

        /// <summary>Creates a Fourier-descriptor contour fitting object. 创建 Fourier descriptor 轮廓拟合对象。</summary>
        public static ContourFitting CreateContourFitting(int ctr = 1024, int fd = 16)
        {
            return ContourFitting.Create(ctr, fd);
        }

        /// <summary>Creates a ScanSegment superpixel segmenter. 创建 ScanSegment 超像素分割器。</summary>
        public static ScanSegment CreateScanSegment(int imageWidth, int imageHeight, int numSuperpixels, int slices = 8, bool mergeSmall = true)
        {
            return ScanSegment.Create(imageWidth, imageHeight, numSuperpixels, slices, mergeSmall);
        }

        /// <summary>Creates a graph segmentation object. 创建基于图的分割对象。</summary>
        public static GraphSegmentation CreateGraphSegmentation(double sigma = 0.5, float k = 300.0F, int minSize = 100)
        {
            return GraphSegmentation.Create(sigma, k, minSize);
        }

        /// <summary>Creates a Selective Search segmentation object. 创建 Selective Search 分割对象。</summary>
        public static SelectiveSearchSegmentation CreateSelectiveSearchSegmentation()
        {
            return SelectiveSearchSegmentation.Create();
        }

        /// <summary>Creates a color Selective Search strategy. 创建颜色 Selective Search 策略。</summary>
        public static SelectiveSearchSegmentationStrategy CreateSelectiveSearchSegmentationStrategyColor()
        {
            return SelectiveSearchSegmentationStrategy.CreateColor();
        }

        /// <summary>Creates a size Selective Search strategy. 创建大小 Selective Search 策略。</summary>
        public static SelectiveSearchSegmentationStrategy CreateSelectiveSearchSegmentationStrategySize()
        {
            return SelectiveSearchSegmentationStrategy.CreateSize();
        }

        /// <summary>Creates a texture Selective Search strategy. 创建纹理 Selective Search 策略。</summary>
        public static SelectiveSearchSegmentationStrategy CreateSelectiveSearchSegmentationStrategyTexture()
        {
            return SelectiveSearchSegmentationStrategy.CreateTexture();
        }

        /// <summary>Creates a fill Selective Search strategy. 创建填充度 Selective Search 策略。</summary>
        public static SelectiveSearchSegmentationStrategy CreateSelectiveSearchSegmentationStrategyFill()
        {
            return SelectiveSearchSegmentationStrategy.CreateFill();
        }

        /// <summary>Creates a combined Selective Search strategy. 创建组合 Selective Search 策略。</summary>
        public static SelectiveSearchSegmentationStrategyMultiple CreateSelectiveSearchSegmentationStrategyMultiple()
        {
            return SelectiveSearchSegmentationStrategy.CreateMultiple();
        }

        internal static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        internal static void ValidateSLICType(SLICType value, string parameterName)
        {
            if (value != SLICType.SLIC && value != SLICType.SLICO && value != SLICType.MSLIC)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported SLIC algorithm type.");
            }
        }

        private static void ValidateAnisotropicDiffusionArguments(Mat src, float alpha, float k, int niters)
        {
            if (niters < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(niters), "Number of iterations cannot be negative.");
            }

            if (niters == 0)
            {
                return;
            }

            if (src.Dims != 2 || src.Type != MatType.CV_8UC3)
            {
                throw new ArgumentException("Anisotropic diffusion source image must be a 2D CV_8UC3 Mat.", nameof(src));
            }

            if (!(alpha > 0.0F))
            {
                throw new ArgumentOutOfRangeException(nameof(alpha), "Alpha must be greater than zero.");
            }

            if (k == 0.0F)
            {
                throw new ArgumentOutOfRangeException(nameof(k), "K must not be zero.");
            }
        }

        private static void ValidateBilateralTextureFilterArguments(Mat src, int fr, int numIter)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Bilateral texture filter source image must not be empty.", nameof(src));
            }

            int depth = MatType.Depth(src.Type);
            if (depth != MatType.CV_8U && depth != MatType.CV_32F)
            {
                throw new ArgumentException("Bilateral texture filter source image depth must be CV_8U or CV_32F.", nameof(src));
            }

            if (fr <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fr), "Filter radius must be greater than zero.");
            }

            if (numIter <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(numIter), "Number of iterations must be greater than zero.");
            }
        }

        private static void ValidateAdaptiveManifoldFilterArguments(double sigmaS, double sigmaR)
        {
            if (sigmaS < 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigmaS), "SigmaS must be at least one.");
            }

            if (sigmaR <= 0.0 || sigmaR > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigmaR), "SigmaR must be greater than zero and no greater than one.");
            }
        }

        private static void ValidateAdaptiveManifoldFilterInput(Mat joint, Mat src)
        {
            if (joint.Empty)
            {
                throw new ArgumentException("Adaptive manifold filter joint image must not be empty.", nameof(joint));
            }

            if (src.Empty)
            {
                throw new ArgumentException("Adaptive manifold filter source image must not be empty.", nameof(src));
            }

            if (joint.Rows != src.Rows || joint.Cols != src.Cols)
            {
                throw new ArgumentException("Adaptive manifold filter joint and source images must have the same size.", nameof(joint));
            }

            int jointDepth = MatType.Depth(joint.Type);
            if (jointDepth != MatType.CV_8U && jointDepth != MatType.CV_16U && jointDepth != MatType.CV_32F)
            {
                throw new ArgumentException("Adaptive manifold filter joint image depth must be CV_8U, CV_16U, or CV_32F.", nameof(joint));
            }
        }

        private static void ValidateEdgePreservingFilterSource(Mat src)
        {
            if (src.Type != MatType.CV_8UC3)
            {
                throw new ArgumentException("Edge-preserving filter source image must be CV_8UC3.", nameof(src));
            }
        }

        private static void ValidateRollingGuidanceFilterSource(Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Rolling guidance filter source image must not be empty.", nameof(src));
            }

            int depth = MatType.Depth(src.Type);
            if (depth != MatType.CV_8U && depth != MatType.CV_32F)
            {
                throw new ArgumentException("Rolling guidance filter source image depth must be CV_8U or CV_32F.", nameof(src));
            }

            int channels = MatType.Channels(src.Type);
            if (channels != 1 && channels != 3)
            {
                throw new ArgumentException("Rolling guidance filter source image must have 1 or 3 channels.", nameof(src));
            }
        }

        private static void ValidateWeightedMedianFilterArguments(Mat joint, Mat src, int r, double sigma)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Weighted median filter source image must not be empty.", nameof(src));
            }

            if (r <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(r), "Radius must be greater than zero.");
            }

            if (sigma <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigma), "Sigma must be greater than zero.");
            }

            int sourceDepth = MatType.Depth(src.Type);
            if (sourceDepth != MatType.CV_8U && sourceDepth != MatType.CV_32F)
            {
                throw new ArgumentException("Weighted median filter source image depth must be CV_8U or CV_32F.", nameof(src));
            }

            if (!joint.Empty)
            {
                int jointDepth = MatType.Depth(joint.Type);
                if (jointDepth != MatType.CV_8U)
                {
                    throw new ArgumentException("Weighted median filter joint image depth must be CV_8U.", nameof(joint));
                }

                int jointChannels = MatType.Channels(joint.Type);
                if (jointChannels != 1 && jointChannels != 3)
                {
                    throw new ArgumentException("Weighted median filter joint image must have 1 or 3 channels.", nameof(joint));
                }
            }
        }

        private static void ValidateWeightedMedianFilterWeightType(WeightedMedianFilterWeightType value, string parameterName)
        {
            if (value != WeightedMedianFilterWeightType.Exp &&
                value != WeightedMedianFilterWeightType.Iv1 &&
                value != WeightedMedianFilterWeightType.Iv2 &&
                value != WeightedMedianFilterWeightType.Cos &&
                value != WeightedMedianFilterWeightType.Jac &&
                value != WeightedMedianFilterWeightType.Off)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported weighted median filter weight type.");
            }
        }

        private static void ValidateL0SmoothArguments(Mat src, double lambda, double kappa)
        {
            if (src.Empty)
            {
                throw new ArgumentException("L0 smooth source image must not be empty.", nameof(src));
            }

            int depth = MatType.Depth(src.Type);
            if (depth != MatType.CV_8U &&
                depth != MatType.CV_16U &&
                depth != MatType.CV_32F &&
                depth != MatType.CV_64F)
            {
                throw new ArgumentException("L0 smooth source image depth must be CV_8U, CV_16U, CV_32F, or CV_64F.", nameof(src));
            }

            if (lambda <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(lambda), "Lambda must be greater than zero.");
            }

            if (kappa <= 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(kappa), "Kappa must be greater than one.");
            }
        }

        private static void ValidateJointBilateralFilterArguments(Mat joint, Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Joint bilateral filter source image must not be empty.", nameof(src));
            }

            int sourceDepth = MatType.Depth(src.Type);
            if (sourceDepth != MatType.CV_8U && sourceDepth != MatType.CV_32F)
            {
                throw new ArgumentException("Joint bilateral filter source image depth must be CV_8U or CV_32F.", nameof(src));
            }

            int sourceChannels = MatType.Channels(src.Type);
            if (sourceChannels != 1 && sourceChannels != 3)
            {
                throw new ArgumentException("Joint bilateral filter source image must have 1 or 3 channels.", nameof(src));
            }

            if (!joint.Empty && !ReferenceEquals(joint, src))
            {
                if (joint.Rows != src.Rows || joint.Cols != src.Cols)
                {
                    throw new ArgumentException("Joint bilateral filter joint image size must match source image size.", nameof(joint));
                }

                int jointDepth = MatType.Depth(joint.Type);
                if (jointDepth != sourceDepth)
                {
                    throw new ArgumentException("Joint bilateral filter joint image depth must match source image depth.", nameof(joint));
                }

                int jointChannels = MatType.Channels(joint.Type);
                if (jointChannels != 1 && jointChannels != 3)
                {
                    throw new ArgumentException("Joint bilateral filter joint image must have 1 or 3 channels.", nameof(joint));
                }
            }
        }

        private static void ValidateDerichePaillouGradientSource(Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Deriche/Paillou gradient source image must not be empty.", nameof(src));
            }

            int depth = MatType.Depth(src.Type);
            if (depth != MatType.CV_8U &&
                depth != MatType.CV_8S &&
                depth != MatType.CV_16U &&
                depth != MatType.CV_16S &&
                depth != MatType.CV_32F)
            {
                throw new ArgumentException("Deriche/Paillou gradient source image depth must be CV_8U, CV_8S, CV_16U, CV_16S, or CV_32F.", nameof(src));
            }
        }

        private static void ValidateFastHoughTransformSource(Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Fast Hough transform source image must not be empty.", nameof(src));
            }
        }

        private static void ValidateContourSamplingElementCount(int nbElt)
        {
            if (nbElt <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nbElt), "Contour sampling element count must be greater than zero.");
            }
        }

        private static void ValidateFourierDescriptorArguments(int nbElt, int nbFD)
        {
            if (nbElt != -1 && nbElt <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(nbElt), "Fourier descriptor element count must be -1 or greater than zero.");
            }

            if (nbFD != -1 && nbFD < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(nbFD), "Fourier descriptor count must be -1 or at least one.");
            }

            if (nbElt > 0 && nbFD > nbElt / 2)
            {
                throw new ArgumentOutOfRangeException(nameof(nbFD), "Fourier descriptor count must be -1 or no greater than half of the element count.");
            }
        }

        private static void ValidateTransformFDTransform(Mat transform)
        {
            if (transform.Rows != 1 || transform.Cols != 5)
            {
                throw new ArgumentException("TransformFD transform matrix must be 1x5.", nameof(transform));
            }

            if (MatType.Depth(transform.Type) != MatType.CV_64F)
            {
                throw new ArgumentException("TransformFD transform matrix depth must be CV_64F.", nameof(transform));
            }
        }

        private static void ValidateCovarianceEstimationSource(Mat src)
        {
            if (MatType.Channels(src.Type) > 2)
            {
                throw new ArgumentException("Covariance estimation source image must have at most 2 channels.", nameof(src));
            }
        }

        internal static void ValidateFastGlobalSmootherCreateArguments(Mat guide, double lambda, double sigmaColor, int numIter)
        {
            if (guide.Empty)
            {
                throw new ArgumentException("Fast global smoother guide image must not be empty.", nameof(guide));
            }

            if (MatType.Depth(guide.Type) != MatType.CV_8U)
            {
                throw new ArgumentException("Fast global smoother guide image depth must be CV_8U.", nameof(guide));
            }

            int guideChannels = MatType.Channels(guide.Type);
            if (guideChannels != 1 && guideChannels != 3)
            {
                throw new ArgumentException("Fast global smoother guide image must have 1 or 3 channels.", nameof(guide));
            }

            if (lambda < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(lambda), "Lambda must be greater than or equal to zero.");
            }

            if (sigmaColor < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(sigmaColor), "Sigma color must be greater than or equal to zero.");
            }

            if (numIter < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(numIter), "Iteration count must be greater than or equal to one.");
            }
        }

        internal static void ValidateFastGlobalSmootherFilterSource(Mat src, int guideRows, int guideCols)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Fast global smoother source image must not be empty.", nameof(src));
            }

            int sourceDepth = MatType.Depth(src.Type);
            if (sourceDepth != MatType.CV_8U &&
                sourceDepth != MatType.CV_16S &&
                sourceDepth != MatType.CV_32F)
            {
                throw new ArgumentException("Fast global smoother source image depth must be CV_8U, CV_16S, or CV_32F.", nameof(src));
            }

            if (MatType.Channels(src.Type) > 4)
            {
                throw new ArgumentException("Fast global smoother source image must have at most 4 channels.", nameof(src));
            }

            if (src.Rows != guideRows || src.Cols != guideCols)
            {
                throw new ArgumentException("Fast global smoother source image size must match guide image size.", nameof(src));
            }
        }

        internal static void ValidateGuidedFilterCreateArguments(Mat guide, int radius, double eps, double scale)
        {
            if (guide.Empty)
            {
                throw new ArgumentException("Guided filter guide image must not be empty.", nameof(guide));
            }

            int guideDepth = MatType.Depth(guide.Type);
            if (guideDepth != MatType.CV_8U &&
                guideDepth != MatType.CV_16U &&
                guideDepth != MatType.CV_32F)
            {
                throw new ArgumentException("Guided filter guide image depth must be CV_8U, CV_16U, or CV_32F.", nameof(guide));
            }

            if (MatType.Channels(guide.Type) > 3)
            {
                throw new ArgumentException("Guided filter guide image must have at most 3 channels.", nameof(guide));
            }

            if (radius < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), "Radius must be greater than or equal to zero.");
            }

            if (eps < 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(eps), "Epsilon must be greater than or equal to zero.");
            }

            if (scale > 1.0)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be less than or equal to one.");
            }
        }

        internal static void ValidateGuidedFilterSource(Mat src, int guideRows, int guideCols)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Guided filter source image must not be empty.", nameof(src));
            }

            int sourceDepth = MatType.Depth(src.Type);
            if (sourceDepth != MatType.CV_8U && sourceDepth != MatType.CV_32F)
            {
                throw new ArgumentException("Guided filter source image depth must be CV_8U or CV_32F.", nameof(src));
            }

            if (src.Rows != guideRows || src.Cols != guideCols)
            {
                throw new ArgumentException("Guided filter source image size must match guide image size.", nameof(src));
            }
        }

        internal static void ValidateFastBilateralSolverCreateArguments(Mat guide)
        {
            if (guide.Empty)
            {
                throw new ArgumentException("Fast bilateral solver guide image must not be empty.", nameof(guide));
            }

            if (MatType.Depth(guide.Type) != MatType.CV_8U)
            {
                throw new ArgumentException("Fast bilateral solver guide image depth must be CV_8U.", nameof(guide));
            }

            int guideChannels = MatType.Channels(guide.Type);
            if (guideChannels != 1 && guideChannels != 3)
            {
                throw new ArgumentException("Fast bilateral solver guide image must have 1 or 3 channels.", nameof(guide));
            }
        }

        internal static void ValidateFastBilateralSolverFilterSource(Mat src, int guideRows, int guideCols)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Fast bilateral solver source image must not be empty.", nameof(src));
            }

            int sourceDepth = MatType.Depth(src.Type);
            if (sourceDepth != MatType.CV_8U &&
                sourceDepth != MatType.CV_16U &&
                sourceDepth != MatType.CV_16S &&
                sourceDepth != MatType.CV_32F)
            {
                throw new ArgumentException("Fast bilateral solver source image depth must be CV_8U, CV_16U, CV_16S, or CV_32F.", nameof(src));
            }

            if (MatType.Channels(src.Type) > 4)
            {
                throw new ArgumentException("Fast bilateral solver source image must have at most 4 channels.", nameof(src));
            }

            if (src.Rows != guideRows || src.Cols != guideCols)
            {
                throw new ArgumentException("Fast bilateral solver source image size must match guide image size.", nameof(src));
            }
        }

        internal static void ValidateFastBilateralSolverConfidence(Mat confidence, int guideRows, int guideCols)
        {
            if (confidence.Empty)
            {
                throw new ArgumentException("Fast bilateral solver confidence image must not be empty.", nameof(confidence));
            }

            int confidenceDepth = MatType.Depth(confidence.Type);
            if (confidenceDepth != MatType.CV_8U && confidenceDepth != MatType.CV_32F)
            {
                throw new ArgumentException("Fast bilateral solver confidence image depth must be CV_8U or CV_32F.", nameof(confidence));
            }

            if (MatType.Channels(confidence.Type) != 1)
            {
                throw new ArgumentException("Fast bilateral solver confidence image must have 1 channel.", nameof(confidence));
            }

            if (confidence.Rows != guideRows || confidence.Cols != guideCols)
            {
                throw new ArgumentException("Fast bilateral solver confidence image size must match guide image size.", nameof(confidence));
            }
        }

        internal static void ValidateDisparityWLSFilterArguments(Mat disparityMapLeft, Mat leftView, Mat? disparityMapRight, bool useConfidence)
        {
            if (disparityMapLeft.Empty)
            {
                throw new ArgumentException("Disparity WLS left disparity map must not be empty.", nameof(disparityMapLeft));
            }

            if (MatType.Channels(disparityMapLeft.Type) != 1)
            {
                throw new ArgumentException("Disparity WLS left disparity map must have 1 channel.", nameof(disparityMapLeft));
            }

            if (leftView.Empty)
            {
                throw new ArgumentException("Disparity WLS left view image must not be empty.", nameof(leftView));
            }

            if (MatType.Depth(leftView.Type) != MatType.CV_8U)
            {
                throw new ArgumentException("Disparity WLS left view image depth must be CV_8U.", nameof(leftView));
            }

            int leftViewChannels = MatType.Channels(leftView.Type);
            if (leftViewChannels != 1 && leftViewChannels != 3)
            {
                throw new ArgumentException("Disparity WLS left view image must have 1 or 3 channels.", nameof(leftView));
            }

            if (!useConfidence || disparityMapRight == null)
            {
                return;
            }

            if (disparityMapRight.Empty)
            {
                throw new ArgumentException("Disparity WLS right disparity map must not be empty when confidence filtering is enabled.", nameof(disparityMapRight));
            }

            if (MatType.Channels(disparityMapRight.Type) != 1)
            {
                throw new ArgumentException("Disparity WLS right disparity map must have 1 channel when confidence filtering is enabled.", nameof(disparityMapRight));
            }

            if (disparityMapRight.Rows != disparityMapLeft.Rows || disparityMapRight.Cols != disparityMapLeft.Cols)
            {
                throw new ArgumentException("Disparity WLS right disparity map size must match left disparity map size when confidence filtering is enabled.", nameof(disparityMapRight));
            }
        }

        internal static void ValidateDisparityMapForVisualization(Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Disparity visualization source map must not be empty.", nameof(src));
            }

            ValidateDisparityEvaluationMap(src, nameof(src), "Disparity visualization source map");
        }

        internal static void ValidateDisparityEvaluationInputs(Mat gt, Mat src)
        {
            if (gt.Empty)
            {
                throw new ArgumentException("Disparity ground-truth map must not be empty.", nameof(gt));
            }

            if (src.Empty)
            {
                throw new ArgumentException("Disparity source map must not be empty.", nameof(src));
            }

            ValidateDisparityEvaluationMap(gt, nameof(gt), "Disparity ground-truth map");
            ValidateDisparityEvaluationMap(src, nameof(src), "Disparity source map");

            if (src.Rows != gt.Rows || src.Cols != gt.Cols)
            {
                throw new ArgumentException("Disparity source map size must match ground-truth map size.", nameof(src));
            }
        }

        private static void ValidateDisparityEvaluationMap(Mat map, string parameterName, string displayName)
        {
            int depth = MatType.Depth(map.Type);
            if (depth != MatType.CV_16S && depth != MatType.CV_32F)
            {
                throw new ArgumentException(displayName + " depth must be CV_16S or CV_32F.", parameterName);
            }

            if (MatType.Channels(map.Type) != 1)
            {
                throw new ArgumentException(displayName + " must have 1 channel.", parameterName);
            }
        }

        private static void ValidateThinningType(ThinningTypes value, string parameterName)
        {
            if (value != ThinningTypes.ZhangSuen && value != ThinningTypes.GuoHall)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported thinning type.");
            }
        }

        private static void ValidateThinningSource(Mat src)
        {
            if (src.Empty)
            {
                throw new ArgumentException("Thinning source image must not be empty.", nameof(src));
            }

            if (src.Type != MatType.CV_8UC1)
            {
                throw new ArgumentException("Thinning source image must be CV_8UC1.", nameof(src));
            }
        }

        private static void ValidateNiBlackThresholdArguments(Mat src, ThresholdTypes type, int blockSize, LocalBinarizationMethods binarizationMethod, double r)
        {
            if (src.Empty)
            {
                throw new ArgumentException("NiBlack threshold source image must not be empty.", nameof(src));
            }

            if (src.Type != MatType.CV_8UC1)
            {
                throw new ArgumentException("NiBlack threshold source image must be CV_8UC1.", nameof(src));
            }

            if (blockSize <= 1 || blockSize % 2 == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(blockSize), "NiBlack threshold block size must be an odd value greater than 1.");
            }

            ThresholdTypes maskedType = type & ThresholdTypes.Mask;
            if (maskedType != ThresholdTypes.Binary &&
                maskedType != ThresholdTypes.BinaryInv &&
                maskedType != ThresholdTypes.Trunc &&
                maskedType != ThresholdTypes.ToZero &&
                maskedType != ThresholdTypes.ToZeroInv)
            {
                throw new ArgumentOutOfRangeException(nameof(type), "Unsupported NiBlack threshold type.");
            }

            if (binarizationMethod == LocalBinarizationMethods.Sauvola && r == 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(r), "Sauvola binarization r must be non-zero.");
            }
        }

        private static void ValidateNiBlackThresholdOutput(Mat src, Mat dst)
        {
            if (!src.Empty && !dst.Empty && src.Data == dst.Data)
            {
                throw new ArgumentException("NiBlack threshold cannot process source and destination in-place.", nameof(dst));
            }
        }

        private static void ValidateLocalBinarizationMethod(LocalBinarizationMethods value, string parameterName)
        {
            if (value != LocalBinarizationMethods.NiBlack &&
                value != LocalBinarizationMethods.Sauvola &&
                value != LocalBinarizationMethods.Wolf &&
                value != LocalBinarizationMethods.Nick)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported local binarization method.");
            }
        }

        private static void ValidateDomainTransformFilterMode(DomainTransformFilterMode value, string parameterName)
        {
            if (value != DomainTransformFilterMode.NormalizedConvolution &&
                value != DomainTransformFilterMode.InterpolatedConvolution &&
                value != DomainTransformFilterMode.RecursiveFiltering)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported domain transform filter mode.");
            }
        }

        private static void ValidateAngleRangeOption(AngleRangeOption value, string parameterName)
        {
            if (value < AngleRangeOption.Aro0To45 || value > AngleRangeOption.AroCenteredVertical)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported angle range option.");
            }
        }

        private static void ValidateHoughOp(HoughOp value, string parameterName)
        {
            if (value != HoughOp.Min && value != HoughOp.Max && value != HoughOp.Add && value != HoughOp.Average)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported Hough operation.");
            }
        }

        private static void ValidateHoughDeskewOption(HoughDeskewOption value, string parameterName)
        {
            if (value != HoughDeskewOption.Raw && value != HoughDeskewOption.Deskew)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported Hough deskew option.");
            }
        }

        private static void ValidateRulesOption(RulesOption value, string parameterName)
        {
            if (value != RulesOption.Strict && value != RulesOption.IgnoreBorders)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Unsupported Hough point-to-line rules option.");
            }
        }

        internal static IntPtr OptionalMatHandle(Mat? mat)
        {
            return mat == null ? IntPtr.Zero : mat.NativeHandle;
        }

        private static Mat CreateOutput(Action<Mat> action)
        {
            var dst = new Mat();
            try
            {
                action(dst);
                return dst;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }
    }
}
