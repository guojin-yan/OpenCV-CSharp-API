using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.OptFlow
{
    /// <summary>
    /// Static helpers from OpenCV contrib optflow and motion template APIs.
    /// OpenCV contrib optflow 与 motion template 静态辅助函数。
    /// </summary>
    public static class OptFlowCv2
    {
        /// <summary>Creates a DeepFlow dense optical flow wrapper. 创建 DeepFlow 密集光流包装。</summary>
        public static DenseOpticalFlow CreateOptFlowDeepFlow()
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowCreateDeepFlow(out IntPtr nativeHandle));
            return new DenseOpticalFlow(nativeHandle);
        }

        /// <summary>Creates a SimpleFlow dense optical flow wrapper. 创建 SimpleFlow 密集光流包装。</summary>
        public static DenseOpticalFlow CreateOptFlowSimpleFlow()
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowCreateSimpleFlow(out IntPtr nativeHandle));
            return new DenseOpticalFlow(nativeHandle);
        }

        /// <summary>Creates a Farneback dense optical flow wrapper. 创建 Farneback 密集光流包装。</summary>
        public static DenseOpticalFlow CreateOptFlowFarneback()
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowCreateFarneback(out IntPtr nativeHandle));
            return new DenseOpticalFlow(nativeHandle);
        }

        /// <summary>Creates a SparseToDense dense optical flow wrapper. 创建 SparseToDense 密集光流包装。</summary>
        public static DenseOpticalFlow CreateOptFlowSparseToDense()
        {
            NativeException.ThrowIfError(NativeMethods.OptFlowCreateSparseToDense(out IntPtr nativeHandle));
            return new DenseOpticalFlow(nativeHandle);
        }

        /// <summary>Calculates SimpleFlow optical flow with compact parameters. 使用简短参数计算 SimpleFlow 光流。</summary>
        public static void CalcOpticalFlowSF(Mat from, Mat to, Mat flow, int layers, int averagingBlockSize, int maxFlow)
        {
            ValidateMats(from, to, flow);
            NativeException.ThrowIfError(NativeMethods.OptFlowCalcOpticalFlowSFSimple(from.NativeHandle, to.NativeHandle, flow.NativeHandle, layers, averagingBlockSize, maxFlow));
        }

        /// <summary>Calculates SimpleFlow optical flow with full parameters. 使用完整参数计算 SimpleFlow 光流。</summary>
        public static void CalcOpticalFlowSF(
            Mat from,
            Mat to,
            Mat flow,
            int layers,
            int averagingBlockSize,
            int maxFlow,
            double sigmaDist,
            double sigmaColor,
            int postprocessWindow,
            double sigmaDistFix,
            double sigmaColorFix,
            double occThr,
            int upscaleAveragingRadius,
            double upscaleSigmaDist,
            double upscaleSigmaColor,
            double speedUpThr)
        {
            ValidateMats(from, to, flow);
            NativeException.ThrowIfError(NativeMethods.OptFlowCalcOpticalFlowSF(
                from.NativeHandle,
                to.NativeHandle,
                flow.NativeHandle,
                layers,
                averagingBlockSize,
                maxFlow,
                sigmaDist,
                sigmaColor,
                postprocessWindow,
                sigmaDistFix,
                sigmaColorFix,
                occThr,
                upscaleAveragingRadius,
                upscaleSigmaDist,
                upscaleSigmaColor,
                speedUpThr));
        }

        /// <summary>Calculates fast sparse-to-dense optical flow. 计算快速 sparse-to-dense 光流。</summary>
        public static void CalcOpticalFlowSparseToDense(
            Mat from,
            Mat to,
            Mat flow,
            int gridStep = 8,
            int k = 128,
            float sigma = 0.05F,
            bool usePostProc = true,
            float fgsLambda = 500.0F,
            float fgsSigma = 1.5F)
        {
            ValidateMats(from, to, flow);
            NativeException.ThrowIfError(NativeMethods.OptFlowCalcOpticalFlowSparseToDense(
                from.NativeHandle,
                to.NativeHandle,
                flow.NativeHandle,
                gridStep,
                k,
                sigma,
                usePostProc ? 1 : 0,
                fgsLambda,
                fgsSigma));
        }

        /// <summary>Calculates dense RLOF optical flow. 计算密集 RLOF 光流。</summary>
        public static void CalcOpticalFlowDenseRLOF(
            Mat i0,
            Mat i1,
            Mat flow,
            RLOFOpticalFlowParameter? parameter = null,
            float forwardBackwardThreshold = 0.0F,
            Size? gridStep = null,
            OptFlowInterpolationType interpolation = OptFlowInterpolationType.Epic,
            int epicK = 128,
            float epicSigma = 0.05F,
            float epicLambda = 100.0F,
            int ricSpSize = 15,
            int ricSlicType = 100,
            bool usePostProc = true,
            float fgsLambda = 500.0F,
            float fgsSigma = 1.5F,
            bool useVariationalRefinement = false)
        {
            ValidateMats(i0, i1, flow);
            ValidateInterpolationType(interpolation, nameof(interpolation));
            Size step = gridStep ?? new Size(6, 6);
            NativeException.ThrowIfError(NativeMethods.OptFlowCalcOpticalFlowDenseRlof(
                i0.NativeHandle,
                i1.NativeHandle,
                flow.NativeHandle,
                RLOFOpticalFlowParameter.OptionalHandle(parameter),
                forwardBackwardThreshold,
                step.Width,
                step.Height,
                (int)interpolation,
                epicK,
                epicSigma,
                epicLambda,
                ricSpSize,
                ricSlicType,
                usePostProc ? 1 : 0,
                fgsLambda,
                fgsSigma,
                useVariationalRefinement ? 1 : 0));
        }

        /// <summary>Calculates sparse RLOF optical flow. 计算稀疏 RLOF 光流。</summary>
        public static void CalcOpticalFlowSparseRLOF(
            Mat prevImg,
            Mat nextImg,
            Mat prevPts,
            Mat nextPts,
            Mat status,
            Mat err,
            RLOFOpticalFlowParameter? parameter = null,
            float forwardBackwardThreshold = 0.0F)
        {
            DenseOpticalFlow.ValidateNotNull(prevImg, nameof(prevImg));
            DenseOpticalFlow.ValidateNotNull(nextImg, nameof(nextImg));
            DenseOpticalFlow.ValidateNotNull(prevPts, nameof(prevPts));
            DenseOpticalFlow.ValidateNotNull(nextPts, nameof(nextPts));
            DenseOpticalFlow.ValidateNotNull(status, nameof(status));
            DenseOpticalFlow.ValidateNotNull(err, nameof(err));
            NativeException.ThrowIfError(NativeMethods.OptFlowCalcOpticalFlowSparseRlof(
                prevImg.NativeHandle,
                nextImg.NativeHandle,
                prevPts.NativeHandle,
                nextPts.NativeHandle,
                status.NativeHandle,
                err.NativeHandle,
                RLOFOpticalFlowParameter.OptionalHandle(parameter),
                forwardBackwardThreshold));
        }

        /// <summary>Updates a motion history image. 更新运动历史图像。</summary>
        public static void UpdateMotionHistory(Mat silhouette, Mat mhi, double timestamp, double duration)
        {
            DenseOpticalFlow.ValidateNotNull(silhouette, nameof(silhouette));
            DenseOpticalFlow.ValidateNotNull(mhi, nameof(mhi));
            NativeException.ThrowIfError(NativeMethods.MotionTemplateUpdateMotionHistory(silhouette.NativeHandle, mhi.NativeHandle, timestamp, duration));
        }

        /// <summary>Calculates motion gradient mask and orientation. 计算运动梯度掩码和方向。</summary>
        public static void CalcMotionGradient(Mat mhi, Mat mask, Mat orientation, double delta1, double delta2, int apertureSize = 3)
        {
            DenseOpticalFlow.ValidateNotNull(mhi, nameof(mhi));
            DenseOpticalFlow.ValidateNotNull(mask, nameof(mask));
            DenseOpticalFlow.ValidateNotNull(orientation, nameof(orientation));
            NativeException.ThrowIfError(NativeMethods.MotionTemplateCalcMotionGradient(mhi.NativeHandle, mask.NativeHandle, orientation.NativeHandle, delta1, delta2, apertureSize));
        }

        /// <summary>Calculates global motion orientation. 计算全局运动方向。</summary>
        public static double CalcGlobalOrientation(Mat orientation, Mat mask, Mat mhi, double timestamp, double duration)
        {
            DenseOpticalFlow.ValidateNotNull(orientation, nameof(orientation));
            DenseOpticalFlow.ValidateNotNull(mask, nameof(mask));
            DenseOpticalFlow.ValidateNotNull(mhi, nameof(mhi));
            NativeException.ThrowIfError(NativeMethods.MotionTemplateCalcGlobalOrientation(orientation.NativeHandle, mask.NativeHandle, mhi.NativeHandle, timestamp, duration, out double angle));
            return angle;
        }

        /// <summary>
        /// Segments a motion history image and returns bounding rectangles.
        /// 分割运动历史图像并返回边界矩形。
        /// </summary>
        public static Rect[] SegmentMotion(Mat mhi, Mat segmask, double timestamp, double segThresh)
        {
            DenseOpticalFlow.ValidateNotNull(mhi, nameof(mhi));
            DenseOpticalFlow.ValidateNotNull(segmask, nameof(segmask));
            NativeException.ThrowIfError(NativeMethods.MotionTemplateSegmentMotionCount(mhi.NativeHandle, segmask.NativeHandle, timestamp, segThresh, out int count));
            if (count == 0)
            {
                return Array.Empty<Rect>();
            }

            var nativeRects = new OptFlowRectNative[count];
            NativeException.ThrowIfError(NativeMethods.MotionTemplateSegmentMotionFill(mhi.NativeHandle, segmask.NativeHandle, timestamp, segThresh, nativeRects, nativeRects.Length, out count));
            var rects = new Rect[count];
            for (int i = 0; i < count; i++)
            {
                rects[i] = new Rect(nativeRects[i].X, nativeRects[i].Y, nativeRects[i].Width, nativeRects[i].Height);
            }

            return rects;
        }

        private static void ValidateMats(Mat first, Mat second, Mat third)
        {
            DenseOpticalFlow.ValidateNotNull(first, nameof(first));
            DenseOpticalFlow.ValidateNotNull(second, nameof(second));
            DenseOpticalFlow.ValidateNotNull(third, nameof(third));
        }

        internal static void ValidateSolverType(OptFlowSolverType value, string parameterName)
        {
            if (value != OptFlowSolverType.Standart
                && value != OptFlowSolverType.Bilinear)
            {
                throw new ArgumentOutOfRangeException(parameterName, "OptFlow solver type must be a defined value.");
            }
        }

        internal static void ValidateSupportRegionType(OptFlowSupportRegionType value, string parameterName)
        {
            if (value != OptFlowSupportRegionType.Fixed
                && value != OptFlowSupportRegionType.Cross)
            {
                throw new ArgumentOutOfRangeException(parameterName, "OptFlow support region type must be a defined value.");
            }
        }

        internal static void ValidateInterpolationType(OptFlowInterpolationType value, string parameterName)
        {
            if (value != OptFlowInterpolationType.Geo
                && value != OptFlowInterpolationType.Epic
                && value != OptFlowInterpolationType.Ric)
            {
                throw new ArgumentOutOfRangeException(parameterName, "OptFlow interpolation type must be a defined value.");
            }
        }
    }
}
