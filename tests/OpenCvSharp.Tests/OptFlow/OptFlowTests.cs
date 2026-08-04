using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.OptFlow;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Tests.OptFlow
{
    public sealed class OptFlowTests
    {
        [Fact]
        public void EnumValuesMatchOpenCvOptFlowConstants()
        {
            Assert.Equal(0, (int)OptFlowSupportRegionType.Fixed);
            Assert.Equal(1, (int)OptFlowSupportRegionType.Cross);
            Assert.Equal(0, (int)OptFlowSolverType.Standart);
            Assert.Equal(1, (int)OptFlowSolverType.Bilinear);
            Assert.Equal(0, (int)OptFlowInterpolationType.Geo);
            Assert.Equal(1, (int)OptFlowInterpolationType.Epic);
            Assert.Equal(2, (int)OptFlowInterpolationType.Ric);
        }

        [Fact]
        public void StaticFunctionsValidateManagedArguments()
        {
            using (var mat = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSF(null!, mat, mat, 1, 2, 3));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSF(mat, null!, mat, 1, 2, 3));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSF(mat, mat, null!, 1, 2, 3));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSparseToDense(null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSparseToDense(mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSparseToDense(mat, mat, null!));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowDenseRLOF(null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowDenseRLOF(mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowDenseRLOF(mat, mat, null!));
                Assert.Throws<ArgumentOutOfRangeException>(() => OptFlowCv2.CalcOpticalFlowDenseRLOF(mat, mat, mat, interpolation: (OptFlowInterpolationType)99));
                Assert.Throws<ArgumentOutOfRangeException>(() => DenseRLOFOpticalFlow.Create(interpolation: (OptFlowInterpolationType)99));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSparseRLOF(null!, mat, mat, mat, mat, mat));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSparseRLOF(mat, null!, mat, mat, mat, mat));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSparseRLOF(mat, mat, null!, mat, mat, mat));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSparseRLOF(mat, mat, mat, null!, mat, mat));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSparseRLOF(mat, mat, mat, mat, null!, mat));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcOpticalFlowSparseRLOF(mat, mat, mat, mat, mat, null!));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.UpdateMotionHistory(null!, mat, 1.0, 1.0));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.UpdateMotionHistory(mat, null!, 1.0, 1.0));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcMotionGradient(null!, mat, mat, 1.0, 2.0));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcMotionGradient(mat, null!, mat, 1.0, 2.0));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcMotionGradient(mat, mat, null!, 1.0, 2.0));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcGlobalOrientation(null!, mat, mat, 1.0, 1.0));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcGlobalOrientation(mat, null!, mat, 1.0, 1.0));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.CalcGlobalOrientation(mat, mat, null!, 1.0, 1.0));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.SegmentMotion(null!, mat, 1.0, 1.0));
                Assert.Throws<ArgumentNullException>(() => OptFlowCv2.SegmentMotion(mat, null!, 1.0, 1.0));
            }
        }

        [Fact]
        public void ObjectSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var parameter = RLOFOpticalFlowParameter.Create())
            using (var tvl1 = DualTVL1OpticalFlow.Create(nscales: 2, warps: 1, innerIterations: 2, outerIterations: 1))
            using (var dense = DenseRLOFOpticalFlow.Create(parameter, forwardBackwardThreshold: 0.0F, gridStep: new Size(4, 4), usePostProc: false))
            using (var sparse = SparseRLOFOpticalFlow.Create(parameter, forwardBackwardThreshold: 0.0F))
            {
                parameter.SolverType = OptFlowSolverType.Bilinear;
                parameter.SupportRegionType = OptFlowSupportRegionType.Fixed;
                Assert.Throws<ArgumentOutOfRangeException>(() => parameter.SolverType = (OptFlowSolverType)99);
                Assert.Throws<ArgumentOutOfRangeException>(() => parameter.SupportRegionType = (OptFlowSupportRegionType)99);
                parameter.SetUseMEstimator(false);
                parameter.NormSigma0 = 1.25F;
                parameter.NormSigma1 = 1.75F;
                parameter.SmallWinSize = 7;
                parameter.LargeWinSize = 15;
                parameter.CrossSegmentationThreshold = 21;
                parameter.MaxLevel = 1;
                parameter.UseInitialFlow = false;
                parameter.UseIlluminationModel = false;
                parameter.UseGlobalMotionPrior = false;
                parameter.MaxIteration = 9;
                parameter.MinEigenValue = 0.0002F;
                parameter.GlobalMotionRansacThreshold = 8.5F;

                tvl1.Tau = 0.24;
                tvl1.Lambda = 0.14;
                tvl1.Theta = 0.32;
                tvl1.Gamma = 0.01;
                tvl1.Epsilon = 0.005;
                tvl1.ScaleStep = 0.75;
                tvl1.ScalesNumber = 2;
                tvl1.WarpingsNumber = 1;
                tvl1.InnerIterations = 3;
                tvl1.OuterIterations = 2;
                tvl1.UseInitialFlow = true;
                tvl1.MedianFiltering = 3;

                dense.ForwardBackward = 0.0F;
                dense.GridStep = new Size(4, 4);
                dense.Interpolation = OptFlowInterpolationType.Epic;
                Assert.Throws<ArgumentOutOfRangeException>(() => dense.Interpolation = (OptFlowInterpolationType)99);
                dense.EPICK = 32;
                dense.EPICSigma = 0.07F;
                dense.EPICLambda = 600.0F;
                dense.FgsLambda = 400.0F;
                dense.FgsSigma = 1.2F;
                dense.UsePostProc = false;
                dense.UseVariationalRefinement = false;
                dense.RICSPSize = 12;
                dense.RICSLICType = 100;

                using (var sparseParameter = RLOFOpticalFlowParameter.Create())
                {
                    sparseParameter.MaxLevel = 2;
                    sparseParameter.MaxIteration = 6;
                    sparse.SetRLOFOpticalFlowParameter(sparseParameter);
                }

                sparse.ForwardBackward = 0.25F;

                Assert.Equal(OptFlowSolverType.Bilinear, parameter.SolverType);
                Assert.Equal(OptFlowSupportRegionType.Fixed, parameter.SupportRegionType);
                Assert.Equal(1.25F, parameter.NormSigma0, 4);
                Assert.Equal(1.75F, parameter.NormSigma1, 4);
                Assert.Equal(7, parameter.SmallWinSize);
                Assert.Equal(15, parameter.LargeWinSize);
                Assert.Equal(21, parameter.CrossSegmentationThreshold);
                Assert.Equal(1, parameter.MaxLevel);
                Assert.False(parameter.UseInitialFlow);
                Assert.False(parameter.UseIlluminationModel);
                Assert.False(parameter.UseGlobalMotionPrior);
                Assert.Equal(9, parameter.MaxIteration);
                Assert.Equal(0.0002F, parameter.MinEigenValue, 7);
                Assert.Equal(8.5F, parameter.GlobalMotionRansacThreshold, 4);
                Assert.Equal(0.24, tvl1.Tau, 4);
                Assert.Equal(0.14, tvl1.Lambda, 4);
                Assert.Equal(0.32, tvl1.Theta, 4);
                Assert.Equal(0.01, tvl1.Gamma, 4);
                Assert.Equal(0.005, tvl1.Epsilon, 5);
                Assert.Equal(0.75, tvl1.ScaleStep, 4);
                Assert.Equal(2, tvl1.ScalesNumber);
                Assert.Equal(1, tvl1.WarpingsNumber);
                Assert.Equal(3, tvl1.InnerIterations);
                Assert.Equal(2, tvl1.OuterIterations);
                Assert.True(tvl1.UseInitialFlow);
                Assert.Equal(3, tvl1.MedianFiltering);
                Assert.Equal(new Size(4, 4).ToString(), dense.GridStep.ToString());
                Assert.Equal(0.0F, dense.ForwardBackward, 3);
                Assert.Equal(OptFlowInterpolationType.Epic, dense.Interpolation);
                Assert.Equal(32, dense.EPICK);
                Assert.Equal(0.07F, dense.EPICSigma, 4);
                Assert.Equal(600.0F, dense.EPICLambda, 4);
                Assert.Equal(400.0F, dense.FgsLambda, 4);
                Assert.Equal(1.2F, dense.FgsSigma, 4);
                Assert.False(dense.UsePostProc);
                Assert.False(dense.UseVariationalRefinement);
                Assert.Equal(12, dense.RICSPSize);
                Assert.Equal(100, dense.RICSLICType);
                Assert.Equal(0.25F, sparse.ForwardBackward, 4);
                using (var roundTripSparseParameter = sparse.GetRLOFOpticalFlowParameter())
                {
                    Assert.Equal(2, roundTripSparseParameter.MaxLevel);
                    Assert.Equal(6, roundTripSparseParameter.MaxIteration);
                }
            }
        }

        [Fact]
        public void DenseOpticalFlowValidatesManagedArguments()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var flow = DualTVL1OpticalFlow.Create(nscales: 2, warps: 1, innerIterations: 2, outerIterations: 1))
            using (var first = CreateFrame(2))
            using (var second = CreateFrame(5))
            using (var output = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => flow.Calc(null!, second, output));
                Assert.Throws<ArgumentNullException>(() => flow.Calc(first, null!, output));
                Assert.Throws<ArgumentNullException>(() => flow.Calc(first, second, null!));
                Assert.Throws<ArgumentNullException>(() => flow.Calc(null!, second));
                Assert.Throws<ArgumentNullException>(() => flow.Calc(first, null!));
            }
        }

        [Fact]
        public void DenseOpticalFlowThrowsAfterDispose()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var first = CreateFrame(2))
            using (var second = CreateFrame(5))
            using (var output = new Mat())
            {
                var flow = DualTVL1OpticalFlow.Create(nscales: 2, warps: 1, innerIterations: 2, outerIterations: 1);
                flow.Dispose();

                Assert.True(flow.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => flow.Calc(first, second, output));
                Assert.Throws<ObjectDisposedException>(() => flow.Calc(first, second));
                Assert.Throws<ObjectDisposedException>(() => flow.CollectGarbage());
            }
        }

        [Fact]
        public void RLOFParameterThrowsAfterDispose()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var parameter = RLOFOpticalFlowParameter.Create();
            parameter.Dispose();

            Assert.True(parameter.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => parameter.SolverType);
            Assert.Throws<ObjectDisposedException>(() => parameter.SolverType = OptFlowSolverType.Bilinear);
            Assert.Throws<ObjectDisposedException>(() => parameter.SupportRegionType);
            Assert.Throws<ObjectDisposedException>(() => parameter.SupportRegionType = OptFlowSupportRegionType.Fixed);
            Assert.Throws<ObjectDisposedException>(() => parameter.NormSigma0);
            Assert.Throws<ObjectDisposedException>(() => parameter.NormSigma0 = 1.0F);
            Assert.Throws<ObjectDisposedException>(() => parameter.NormSigma1);
            Assert.Throws<ObjectDisposedException>(() => parameter.NormSigma1 = 1.0F);
            Assert.Throws<ObjectDisposedException>(() => parameter.SmallWinSize);
            Assert.Throws<ObjectDisposedException>(() => parameter.SmallWinSize = 7);
            Assert.Throws<ObjectDisposedException>(() => parameter.LargeWinSize);
            Assert.Throws<ObjectDisposedException>(() => parameter.LargeWinSize = 15);
            Assert.Throws<ObjectDisposedException>(() => parameter.CrossSegmentationThreshold);
            Assert.Throws<ObjectDisposedException>(() => parameter.CrossSegmentationThreshold = 20);
            Assert.Throws<ObjectDisposedException>(() => parameter.MaxLevel);
            Assert.Throws<ObjectDisposedException>(() => parameter.MaxLevel = 1);
            Assert.Throws<ObjectDisposedException>(() => parameter.UseInitialFlow);
            Assert.Throws<ObjectDisposedException>(() => parameter.UseInitialFlow = false);
            Assert.Throws<ObjectDisposedException>(() => parameter.UseIlluminationModel);
            Assert.Throws<ObjectDisposedException>(() => parameter.UseIlluminationModel = false);
            Assert.Throws<ObjectDisposedException>(() => parameter.UseGlobalMotionPrior);
            Assert.Throws<ObjectDisposedException>(() => parameter.UseGlobalMotionPrior = false);
            Assert.Throws<ObjectDisposedException>(() => parameter.MaxIteration);
            Assert.Throws<ObjectDisposedException>(() => parameter.MaxIteration = 10);
            Assert.Throws<ObjectDisposedException>(() => parameter.MinEigenValue);
            Assert.Throws<ObjectDisposedException>(() => parameter.MinEigenValue = 0.0001F);
            Assert.Throws<ObjectDisposedException>(() => parameter.GlobalMotionRansacThreshold);
            Assert.Throws<ObjectDisposedException>(() => parameter.GlobalMotionRansacThreshold = 10.0F);
            Assert.Throws<ObjectDisposedException>(() => parameter.SetUseMEstimator(false));
        }

        [Fact]
        public void SparseRLOFOpticalFlowThrowsAfterDispose()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var first = CreateFrame(2))
            using (var second = CreateFrame(5))
            using (var previousPoints = CreateSparsePoints())
            using (var nextPoints = new Mat())
            using (var status = new Mat())
            using (var err = new Mat())
            using (var parameter = RLOFOpticalFlowParameter.Create())
            {
                var flow = SparseRLOFOpticalFlow.Create(parameter, forwardBackwardThreshold: 0.0F);
                flow.Dispose();

                Assert.True(flow.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => flow.ForwardBackward);
                Assert.Throws<ObjectDisposedException>(() => flow.ForwardBackward = 0.0F);
                Assert.Throws<ObjectDisposedException>(() => flow.GetRLOFOpticalFlowParameter());
                Assert.Throws<ObjectDisposedException>(() => flow.SetRLOFOpticalFlowParameter(parameter));
                Assert.Throws<ObjectDisposedException>(() => flow.Calc(first, second, previousPoints, nextPoints, status, err));
            }
        }

        [Fact]
        public void SparseOpticalFlowValidatesManagedArguments()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var flow = SparseRLOFOpticalFlow.Create(forwardBackwardThreshold: 0.0F))
            using (var first = CreateFrame(2))
            using (var second = CreateFrame(5))
            using (var previousPoints = CreateSparsePoints())
            using (var nextPoints = new Mat())
            using (var status = new Mat())
            using (var err = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => flow.Calc(null!, second, previousPoints, nextPoints, status, err));
                Assert.Throws<ArgumentNullException>(() => flow.Calc(first, null!, previousPoints, nextPoints, status, err));
                Assert.Throws<ArgumentNullException>(() => flow.Calc(first, second, null!, nextPoints, status, err));
                Assert.Throws<ArgumentNullException>(() => flow.Calc(first, second, previousPoints, null!, status, err));
                Assert.Throws<ArgumentNullException>(() => flow.Calc(first, second, previousPoints, nextPoints, null!, err));
                Assert.Throws<ArgumentNullException>(() => flow.Calc(first, second, previousPoints, nextPoints, status, null!));
            }
        }

        [Fact]
        public void DenseRLOFOpticalFlowThrowsAfterDispose()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var first = CreateFrame(2))
            using (var second = CreateFrame(5))
            using (var output = new Mat())
            using (var parameter = RLOFOpticalFlowParameter.Create())
            {
                var flow = DenseRLOFOpticalFlow.Create(parameter, forwardBackwardThreshold: 0.0F, gridStep: new Size(4, 4), usePostProc: false);
                flow.Dispose();

                Assert.True(flow.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => flow.ForwardBackward);
                Assert.Throws<ObjectDisposedException>(() => flow.ForwardBackward = 0.0F);
                Assert.Throws<ObjectDisposedException>(() => flow.GridStep);
                Assert.Throws<ObjectDisposedException>(() => flow.GridStep = new Size(4, 4));
                Assert.Throws<ObjectDisposedException>(() => flow.Interpolation);
                Assert.Throws<ObjectDisposedException>(() => flow.Interpolation = OptFlowInterpolationType.Epic);
                Assert.Throws<ObjectDisposedException>(() => flow.EPICK);
                Assert.Throws<ObjectDisposedException>(() => flow.EPICK = 64);
                Assert.Throws<ObjectDisposedException>(() => flow.EPICSigma);
                Assert.Throws<ObjectDisposedException>(() => flow.EPICSigma = 0.05F);
                Assert.Throws<ObjectDisposedException>(() => flow.EPICLambda);
                Assert.Throws<ObjectDisposedException>(() => flow.EPICLambda = 999.0F);
                Assert.Throws<ObjectDisposedException>(() => flow.FgsLambda);
                Assert.Throws<ObjectDisposedException>(() => flow.FgsLambda = 500.0F);
                Assert.Throws<ObjectDisposedException>(() => flow.FgsSigma);
                Assert.Throws<ObjectDisposedException>(() => flow.FgsSigma = 1.5F);
                Assert.Throws<ObjectDisposedException>(() => flow.UsePostProc);
                Assert.Throws<ObjectDisposedException>(() => flow.UsePostProc = false);
                Assert.Throws<ObjectDisposedException>(() => flow.UseVariationalRefinement);
                Assert.Throws<ObjectDisposedException>(() => flow.UseVariationalRefinement = false);
                Assert.Throws<ObjectDisposedException>(() => flow.RICSPSize);
                Assert.Throws<ObjectDisposedException>(() => flow.RICSPSize = 15);
                Assert.Throws<ObjectDisposedException>(() => flow.RICSLICType);
                Assert.Throws<ObjectDisposedException>(() => flow.RICSLICType = 100);
                Assert.Throws<ObjectDisposedException>(() => flow.GetRLOFOpticalFlowParameter());
                Assert.Throws<ObjectDisposedException>(() => flow.SetRLOFOpticalFlowParameter(parameter));
                Assert.Throws<ObjectDisposedException>(() => flow.Calc(first, second, output));
                Assert.Throws<ObjectDisposedException>(() => flow.CollectGarbage());
            }
        }

        [Fact]
        public void DualTVL1OpticalFlowThrowsPropertiesAfterDispose()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            var flow = DualTVL1OpticalFlow.Create(nscales: 2, warps: 1, innerIterations: 2, outerIterations: 1);
            flow.Dispose();

            Assert.True(flow.IsDisposed);
            Assert.Throws<ObjectDisposedException>(() => flow.Tau);
            Assert.Throws<ObjectDisposedException>(() => flow.Tau = 0.25);
            Assert.Throws<ObjectDisposedException>(() => flow.Lambda);
            Assert.Throws<ObjectDisposedException>(() => flow.Lambda = 0.15);
            Assert.Throws<ObjectDisposedException>(() => flow.Theta);
            Assert.Throws<ObjectDisposedException>(() => flow.Theta = 0.3);
            Assert.Throws<ObjectDisposedException>(() => flow.Gamma);
            Assert.Throws<ObjectDisposedException>(() => flow.Gamma = 0.0);
            Assert.Throws<ObjectDisposedException>(() => flow.Epsilon);
            Assert.Throws<ObjectDisposedException>(() => flow.Epsilon = 0.01);
            Assert.Throws<ObjectDisposedException>(() => flow.ScaleStep);
            Assert.Throws<ObjectDisposedException>(() => flow.ScaleStep = 0.8);
            Assert.Throws<ObjectDisposedException>(() => flow.ScalesNumber);
            Assert.Throws<ObjectDisposedException>(() => flow.ScalesNumber = 2);
            Assert.Throws<ObjectDisposedException>(() => flow.WarpingsNumber);
            Assert.Throws<ObjectDisposedException>(() => flow.WarpingsNumber = 1);
            Assert.Throws<ObjectDisposedException>(() => flow.InnerIterations);
            Assert.Throws<ObjectDisposedException>(() => flow.InnerIterations = 2);
            Assert.Throws<ObjectDisposedException>(() => flow.OuterIterations);
            Assert.Throws<ObjectDisposedException>(() => flow.OuterIterations = 1);
            Assert.Throws<ObjectDisposedException>(() => flow.UseInitialFlow);
            Assert.Throws<ObjectDisposedException>(() => flow.UseInitialFlow = false);
            Assert.Throws<ObjectDisposedException>(() => flow.MedianFiltering);
            Assert.Throws<ObjectDisposedException>(() => flow.MedianFiltering = 5);
        }

        [Fact]
        public void DenseOpticalFlowReturnHelperRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var flow = DualTVL1OpticalFlow.Create(nscales: 2, warps: 1, innerIterations: 2, outerIterations: 1))
            using (var first = CreateGrayFrame(2))
            using (var second = CreateGrayFrame(5))
            using (var denseFlow = flow.Calc(first, second))
            {
                Assert.False(denseFlow.Empty);
                Assert.Equal(first.Rows, denseFlow.Rows);
                Assert.Equal(first.Cols, denseFlow.Cols);
                Assert.Equal(2, denseFlow.Channels);
            }
        }

        [Fact]
        public void MotionTemplateSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var silhouette = new Mat(24, 24, MatType.CV_8UC1, new Scalar(0)))
            using (var mhi = new Mat(24, 24, MatType.CV_32FC1, new Scalar(0)))
            using (var mask = new Mat())
            using (var orientation = new Mat())
            using (var segmask = new Mat())
            {
                ImgProcCv2.Rectangle(silhouette, new Rect(4, 4, 8, 8), new Scalar(255), -1);
                OptFlowCv2.UpdateMotionHistory(silhouette, mhi, 1.0, 10.0);
                OptFlowCv2.CalcMotionGradient(mhi, mask, orientation, 0.25, 1.0);
                double angle = OptFlowCv2.CalcGlobalOrientation(orientation, mask, mhi, 1.0, 10.0);
                Rect[] rects = OptFlowCv2.SegmentMotion(mhi, segmask, 1.0, 0.5);

                Assert.False(mask.Empty);
                Assert.False(orientation.Empty);
                Assert.False(segmask.Empty);
                Assert.True(angle >= 0.0 && angle <= 360.0);
                Assert.NotNull(rects);
            }
        }

        [Fact]
        public void DenseFlowStaticSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (var first = CreateFrame(2))
            using (var second = CreateFrame(5))
            using (var flow = new Mat())
            {
                OptFlowCv2.CalcOpticalFlowSparseToDense(first, second, flow, gridStep: 4, k: 8, sigma: 0.05F, usePostProc: false);

                Assert.False(flow.Empty);
                Assert.Equal(first.Rows, flow.Rows);
                Assert.Equal(first.Cols, flow.Cols);
                Assert.Equal(2, flow.Channels);
            }
        }

        private static Mat CreateFrame(int offset)
        {
            var frame = new Mat(24, 24, MatType.CV_8UC3, new Scalar(20, 40, 60));
            ImgProcCv2.Rectangle(frame, new Rect(4 + offset, 6, 8, 8), new Scalar(200, 30, 80), -1);
            return frame;
        }

        private static Mat CreateGrayFrame(int offset)
        {
            var frame = new Mat(24, 24, MatType.CV_8UC1, new Scalar(20));
            ImgProcCv2.Rectangle(frame, new Rect(4 + offset, 6, 8, 8), new Scalar(200), -1);
            return frame;
        }

        private static Mat CreateSparsePoints()
        {
            return new Mat();
        }

    }
}
