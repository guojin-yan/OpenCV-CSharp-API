using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OpenCvSharp;
using OpenCvSharp.AlphaMat;
using OpenCvSharp.BgSegm;
using OpenCvSharp.BioInspired;
using OpenCvSharp.Calib3D;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;
using OpenCvSharp.Face;
using OpenCvSharp.Geometry;
using OpenCvSharp.Hfs;
using OpenCvSharp.ImgCodecs;
using OpenCvSharp.ImgHash;
using OpenCvSharp.ImgProc;
using OpenCvSharp.IntensityTransform;
using OpenCvSharp.LineDescriptor;
using OpenCvSharp.ML;
using OpenCvSharp.ObjDetect;
using OpenCvSharp.OptFlow;
using OpenCvSharp.Photo;
using OpenCvSharp.PhaseUnwrapping;
using OpenCvSharp.Plot;
using OpenCvSharp.PtCloud;
using OpenCvSharp.Quality;
using OpenCvSharp.Fuzzy;
using OpenCvSharp.Rapid;
using OpenCvSharp.Reg;
using OpenCvSharp.Saliency;
using OpenCvSharp.Shape;
using OpenCvSharp.StructuredLight;
using OpenCvSharp.SurfaceMatching;
using OpenCvSharp.VideoIO;
using OpenCvSharp.XImgProc;
using OpenCvSharp.XObjDetect;
using OpenCvSharp.XPhoto;
using OpenCvSharp.XStereo;
using Calib3DCv2 = OpenCvSharp.Calib3D.Cv2;
using CoreCv2 = OpenCvSharp.Core.Cv2;
using Features2DCv2 = OpenCvSharp.Features2D.Cv2;
using DnnCv2 = OpenCvSharp.Dnn.Cv2;
using HighGuiCv2 = OpenCvSharp.HighGui.Cv2;
using ImgCodecsCv2 = OpenCvSharp.ImgCodecs.Cv2;
using ImgHashCv2 = OpenCvSharp.ImgHash.ImgHashCv2;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;
using PhotoCv2 = OpenCvSharp.Photo.PhotoCv2;
using PlotCv2 = OpenCvSharp.Plot.PlotCv2;
using PtCloudCv2 = OpenCvSharp.PtCloud.PtCloudCv2;
using ShapeCv2 = OpenCvSharp.Shape.ShapeCv2;
using StitcherObject = OpenCvSharp.Stitching.Stitcher;
using VideoCv2 = OpenCvSharp.Video.Cv2;
using XImgProcCv2 = OpenCvSharp.XImgProc.XImgProcCv2;
using XPhotoCv2 = OpenCvSharp.XPhoto.XPhotoCv2;
using BgSegmBackgroundSubtractorCNTObject = OpenCvSharp.BgSegm.BackgroundSubtractorCNT;
using BgSegmBackgroundSubtractorGMGObject = OpenCvSharp.BgSegm.BackgroundSubtractorGMG;
using BgSegmBackgroundSubtractorMOGObject = OpenCvSharp.BgSegm.BackgroundSubtractorMOG;
using BgSegmSyntheticSequenceGeneratorObject = OpenCvSharp.BgSegm.SyntheticSequenceGenerator;
using BackgroundSubtractorKNNObject = OpenCvSharp.Video.BackgroundSubtractorKNN;
using BackgroundSubtractorMOG2Object = OpenCvSharp.Video.BackgroundSubtractorMOG2;
using BlockMeanHashObject = OpenCvSharp.ImgHash.BlockMeanHash;
using CamShiftResult = OpenCvSharp.Video.CamShiftResult;
using MeanShiftResult = OpenCvSharp.Video.MeanShiftResult;
using MLKNearestObject = OpenCvSharp.ML.KNearest;
using MLAnnMlpObject = OpenCvSharp.ML.ANN_MLP;
using MLBoostObject = OpenCvSharp.ML.Boost;
using MLDTreesObject = OpenCvSharp.ML.DTrees;
using MLEMObject = OpenCvSharp.ML.EM;
using MLLogisticRegressionObject = OpenCvSharp.ML.LogisticRegression;
using MLNormalBayesClassifierObject = OpenCvSharp.ML.NormalBayesClassifier;
using MLRTreesObject = OpenCvSharp.ML.RTrees;
using MLSvmObject = OpenCvSharp.ML.SVM;
using MLSVMSGDObject = OpenCvSharp.ML.SVMSGD;
using OptFlowCv2Object = OpenCvSharp.OptFlow.OptFlowCv2;
using OptFlowDualTVL1Object = OpenCvSharp.OptFlow.DualTVL1OpticalFlow;
using OptFlowRLOFParameterObject = OpenCvSharp.OptFlow.RLOFOpticalFlowParameter;
using OpticalFlowPyramidResult = OpenCvSharp.Video.OpticalFlowPyramidResult;
using TrackerCSRTObject = OpenCvSharp.Tracking.TrackerCSRT;
using TrackerKCFObject = OpenCvSharp.Tracking.TrackerKCF;
using TrackerKCFParamsObject = OpenCvSharp.Tracking.TrackerKCFParams;
using TrackerMILObject = OpenCvSharp.Tracking.Legacy.TrackerMIL;
using TrackerMedianFlowObject = OpenCvSharp.Tracking.Legacy.TrackerMedianFlow;
using TrackerMOSSEObject = OpenCvSharp.Tracking.Legacy.TrackerMOSSE;
using LegacyTrackerBoostingObject = OpenCvSharp.Tracking.Legacy.TrackerBoosting;
using LegacyTrackerCSRTObject = OpenCvSharp.Tracking.Legacy.TrackerCSRT;
using LegacyTrackerKCFObject = OpenCvSharp.Tracking.Legacy.TrackerKCF;
using LegacyTrackerTLDObject = OpenCvSharp.Tracking.Legacy.TrackerTLD;
using OpenCvLegacyMultiTrackerObject = OpenCvSharp.Tracking.Legacy.MultiTracker;
using QRCodeDetectorObject = OpenCvSharp.ObjDetect.QRCodeDetector;
using QRCodeDetectorArucoObject = OpenCvSharp.ObjDetect.QRCodeDetectorAruco;
using QRCodeEncoderObject = OpenCvSharp.ObjDetect.QRCodeEncoder;
using BarcodeDetectorObject = OpenCvSharp.ObjDetect.BarcodeDetector;
using CascadeClassifierObject = OpenCvSharp.XObjDetect.CascadeClassifier;
using DnnNetObject = OpenCvSharp.Dnn.Net;
using EigenFaceRecognizerObject = OpenCvSharp.Face.EigenFaceRecognizer;
using FisherFaceRecognizerObject = OpenCvSharp.Face.FisherFaceRecognizer;
using LBPHFaceRecognizerObject = OpenCvSharp.Face.LBPHFaceRecognizer;
using HOGDescriptorObject = OpenCvSharp.XObjDetect.HOGDescriptor;
using MotionSaliencyBinWangObject = OpenCvSharp.Saliency.MotionSaliencyBinWangApr2014;
using RgbdNormalsObject = OpenCvSharp.PtCloud.RgbdNormals;
using StaticSaliencyFineGrainedObject = OpenCvSharp.Saliency.StaticSaliencyFineGrained;
using StaticSaliencySpectralResidualObject = OpenCvSharp.Saliency.StaticSaliencySpectralResidual;
using VideoKalmanFilterObject = OpenCvSharp.Video.KalmanFilter;
using DisOpticalFlowObject = OpenCvSharp.Video.DisOpticalFlow;
using FarnebackOpticalFlowObject = OpenCvSharp.Video.FarnebackOpticalFlow;
using OpticalFlowFlags = OpenCvSharp.Video.OpticalFlowFlags;
using SparsePyrLkOpticalFlowObject = OpenCvSharp.Video.SparsePyrLkOpticalFlow;
using VideoTrackerMILObject = OpenCvSharp.Video.TrackerMIL;
using VariationalRefinementObject = OpenCvSharp.Video.VariationalRefinement;
using VideoCaptureObject = OpenCvSharp.VideoIO.VideoCapture;
using VideoWriterObject = OpenCvSharp.VideoIO.VideoWriter;
using BioInspiredCv2Object = OpenCvSharp.BioInspired.BioInspiredCv2;
using XStereoCv2Object = OpenCvSharp.XStereo.XStereoCv2;

namespace ConsoleSamples
{
    internal static class Program
    {
        private const string DnnModelVariable = "OPENCV_CSHARP_DNN_MODEL";
        private const string CompatibilityDnnModelAlias = "OPENCV5SHARP_DNN_MODEL";
        private const string DnnConfigVariable = "OPENCV_CSHARP_DNN_CONFIG";
        private const string CompatibilityDnnConfigAlias = "OPENCV5SHARP_DNN_CONFIG";
        private const string DnnFrameworkVariable = "OPENCV_CSHARP_DNN_FRAMEWORK";
        private const string CompatibilityDnnFrameworkAlias = "OPENCV5SHARP_DNN_FRAMEWORK";
        private const string HighGuiSmokeVariable = "OPENCV_CSHARP_HIGHGUI_SMOKE";
        private const string CompatibilityHighGuiSmokeAlias = "OPENCV5SHARP_HIGHGUI_SMOKE";
        private const string ConsoleExtendedVariable = "OPENCV_CSHARP_CONSOLE_EXTENDED";
        private const string CompatibilityConsoleExtendedAlias = "OPENCV5SHARP_CONSOLE_EXTENDED";
        private const string UnstableNativeSmokeVariable = "OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE";
        private const string CompatibilityUnstableNativeSmokeAlias = "OPENCV5SHARP_UNSTABLE_NATIVE_SMOKE";
        private const string BrisqueModelVariable = "OPENCV_CSHARP_BRISQUE_MODEL";
        private const string CompatibilityBrisqueModelAlias = "OPENCV5SHARP_BRISQUE_MODEL";
        private const string BrisqueRangeVariable = "OPENCV_CSHARP_BRISQUE_RANGE";
        private const string CompatibilityBrisqueRangeAlias = "OPENCV5SHARP_BRISQUE_RANGE";
        private const string FaceCascadeVariable = "OPENCV_CSHARP_FACE_CASCADE";
        private const string CompatibilityFaceCascadeAlias = "OPENCV5SHARP_FACE_CASCADE";
        private const string StitchingImagesVariable = "OPENCV_CSHARP_STITCHING_IMAGES";
        private const string CompatibilityStitchingImagesAlias = "OPENCV5SHARP_STITCHING_IMAGES";
        private static readonly string FactualOpenCvInstallCacheName = "opencv-" + OpenCvSharpBuildInfo.OpenCvVersion + "-windows-x64";

        private static void Main(string[] args)
        {
            if (args.Length > 0 && string.Equals(args[0], "showcase", StringComparison.OrdinalIgnoreCase))
            {
                ShowcaseRunner.Run(args.Skip(1).ToArray());
                return;
            }

            Console.WriteLine(OpenCvSharpBuildInfo.GetDisplayString());

            Point point = new Point(10, 20);
            Size size = new Size(640, 480);
            Rect rect = new Rect(point, new Size(320, 240));
            Scalar scalar = new Scalar(0, 128, 255);

            Console.WriteLine("Point: " + point);
            Console.WriteLine("Size: " + size);
            Console.WriteLine("Rect: " + rect);
            Console.WriteLine("Scalar: " + scalar);
            Console.WriteLine("CV_8UC3: " + MatType.CV_8UC3);

            try
            {
                using (Mat src = new Mat(2, 2, MatType.CV_8UC3))
                using (Mat matZeros = Mat.Zeros(new Size(2, 2), MatType.CV_8UC1))
                using (Mat matEye = Mat.Eye(3, 3, MatType.CV_8UC1))
                using (Mat gray = new Mat())
                using (Mat resized = new Mat())
                using (Mat thresholded = new Mat())
                using (Mat blurred = new Mat())
                using (Mat boxFiltered = new Mat())
                using (Mat edges = new Mat())
                using (Mat sobelX = new Mat())
                using (Mat gaussianKernel = ImgProcCv2.GetGaussianKernel(3, 0, MatType.CV_64F))
                using (Mat gaborKernel = ImgProcCv2.GetGaborKernel(new Size(3, 3), 1.0, 0.0, 2.0, 0.5))
                using (Mat rotation = ImgProcCv2.GetRotationMatrix2D(new Point2f(1.0F, 1.0F), 0.0, 1.0))
                using (Mat warped = new Mat())
                using (Mat mapX = new Mat(2, 2, MatType.CV_32FC1))
                using (Mat mapY = new Mat(2, 2, MatType.CV_32FC1))
                using (Mat remapped = new Mat())
                using (Mat kernel = ImgProcCv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
                using (Mat morphSrc = new Mat(5, 5, MatType.CV_8UC1))
                using (Mat eroded = new Mat())
                using (Mat dilated = new Mat())
                using (Mat opened = new Mat())
                using (Mat drawing = new Mat(40, 120, MatType.CV_8UC1))
                using (Mat analysis = new Mat(8, 8, MatType.CV_8UC1))
                using (Mat equalized = new Mat())
                using (Mat adaptive = new Mat())
                using (Mat integral = new Mat())
                using (Mat integralSq = new Mat())
                using (Mat integralTilted = new Mat())
                using (Mat distance = new Mat())
                using (Mat distanceLabels = new Mat())
                using (Mat connectedLabels = new Mat())
                using (Mat connectedStats = new Mat())
                using (Mat connectedCentroids = new Mat())
                using (Mat contourCanvas = new Mat(8, 8, MatType.CV_8UC1))
                using (Mat cornerResponse = new Mat())
                using (Mat claheResult = new Mat())
                using (Mat hist = new Mat())
                using (Mat claheHist = new Mat())
                using (Mat backProject = new Mat())
                using (Mat houghBinary = new Mat(64, 64, MatType.CV_8UC1))
                using (Mat houghCircleImage = new Mat(64, 64, MatType.CV_8UC1))
                using (Mat lsdDrawing = new Mat(64, 64, MatType.CV_8UC3))
                using (Mat lsdLines = new Mat())
                {
                    src.CopyFrom(new byte[]
                    {
                        0, 0, 0,
                        10, 20, 30,
                        50, 100, 150,
                        255, 255, 255
                    });

                    matZeros.SetTo(new Scalar(2));
                    using (Mat matClone = matZeros.Clone())
                    using (Mat matRoi = matClone.SubMat(new Rect(0, 0, 1, 1)))
                    using (Mat matRow = matClone.Row(0))
                    using (Mat matReshaped = matEye.Reshape(1, 1))
                    using (Mat matRoiClone = matRoi.Clone())
                    using (Mat matRowClone = matRow.Clone())
                    {
                        Console.WriteLine("Mat zeros: " + string.Join(",", matZeros.ToBytes()));
                        Console.WriteLine("Mat eye: " + string.Join(",", matEye.ToBytes()));
                        Console.WriteLine("Mat clone: " + string.Join(",", matClone.ToBytes()));
                        Console.WriteLine("Mat roi: " + string.Join(",", matRoiClone.ToBytes()));
                        Console.WriteLine("Mat row: " + string.Join(",", matRowClone.ToBytes()));
                        Console.WriteLine("Mat reshaped: " + matReshaped.Rows + "x" + matReshaped.Cols + ", channels=" + matReshaped.Channels);
#if NETCOREAPP3_1_OR_GREATER
                        if (matClone.TryGetByteSpan(out Span<byte> cloneSpan))
                        {
                            Console.WriteLine("Mat clone span length: " + cloneSpan.Length);
                        }
#endif
                    }

                    using (Mat coreA = new Mat(2, 3, MatType.CV_8UC1))
                    using (Mat coreB = new Mat(2, 3, MatType.CV_8UC1))
                    using (Mat coreAdded = new Mat())
                    using (Mat coreMask = new Mat())
                    using (Mat coreNormalized = new Mat())
                    using (Mat coreChannelImage = new Mat(2, 2, MatType.CV_8UC3))
                    using (Mat coreMerged = new Mat())
                    {
                        coreA.CopyFrom(new byte[] { 1, 2, 3, 4, 5, 6 });
                        coreB.CopyFrom(new byte[] { 6, 5, 4, 3, 2, 1 });
                        coreChannelImage.CopyFrom(new byte[]
                        {
                            1, 10, 100,
                            2, 20, 110,
                            3, 30, 120,
                            4, 40, 130
                        });

                        CoreCv2.Add(coreA, coreB, coreAdded);
                        CoreCv2.Compare(coreA, coreB, coreMask, CmpTypes.LT);
                        CoreCv2.Normalize(coreA, coreNormalized, 0.0, 255.0, NormTypes.MinMax);
                        Scalar coreMean = CoreCv2.Mean(coreA);
                        Scalar coreSum = CoreCv2.Sum(coreA);
                        MinMaxLocResult coreMinMax = CoreCv2.MinMaxLoc(coreA);
                        Mat[] coreChannels = CoreCv2.Split(coreChannelImage);
                        try
                        {
                            CoreCv2.Merge(coreChannels, coreMerged);

                            Console.WriteLine("Core add: " + string.Join(",", coreAdded.ToBytes()));
                            Console.WriteLine("Core compare: " + string.Join(",", coreMask.ToBytes()));
                            Console.WriteLine("Core normalize: " + string.Join(",", coreNormalized.ToBytes()));
                            Console.WriteLine("Core mean: " + coreMean);
                            Console.WriteLine("Core sum: " + coreSum);
                            Console.WriteLine("Core minmax: " + coreMinMax);
                            Console.WriteLine("Core split channels: " + coreChannels.Length + ", merged type=" + coreMerged.Type);
                        }
                        finally
                        {
                            for (int i = 0; i < coreChannels.Length; i++)
                            {
                                coreChannels[i].Dispose();
                            }
                        }
                    }

                    using (Mat coreParitySource = new Mat(2, 3, MatType.CV_32SC1))
                    using (Mat coreParityShape = new Mat(1, 2, MatType.CV_32SC1))
                    using (Mat coreParityFloat = new Mat(1, 4, MatType.CV_32FC1))
                    {
                        coreParitySource.CopyFrom(new int[] { 3, 1, 1, 2, 4, 0 });
                        coreParityShape.CopyFrom(new int[] { 2, 3 });
                        coreParityFloat.CopyFrom(new float[] { 1.0F, float.NaN, float.PositiveInfinity, -2.0F });

                        using (Mat coreParityMin = CoreCv2.ReduceArgMin(coreParitySource, 1))
                        using (Mat coreParitySorted = CoreCv2.Sort(coreParitySource, SortFlags.EveryRow | SortFlags.Descending))
                        using (Mat parityRow = coreParitySource.Row(0))
                        using (Mat coreParityBroadcast = CoreCv2.Broadcast(parityRow, coreParityShape))
                        using (Mat coreParityFinite = CoreCv2.FiniteMask(coreParityFloat))
                        using (Mat coreParityPoints = CoreCv2.FindNonZero(coreParitySource))
                        {
                        CheckRangeResult range = CoreCv2.CheckRange(coreParitySource, 0.0, 5.0);
                        Console.WriteLine("Core upstream: min=" + string.Join(",", coreParityMin.ToArray<int>())
                            + ", sorted=" + string.Join(",", coreParitySorted.ToArray<int>())
                            + ", broadcast=" + coreParityBroadcast.Rows + "x" + coreParityBroadcast.Cols
                            + ", finite=" + string.Join(",", coreParityFinite.ToArray<byte>())
                            + ", points=" + coreParityPoints.Total.ToUInt64()
                            + ", range=" + range.IsValid);
                        }
                    }

                    using (Mat persistenceMatrix = new Mat(2, 2, MatType.CV_32SC1))
                    {
                        persistenceMatrix.CopyFrom(new int[] { 2, 4, 6, 8 });
                        string persistenceDocument;
                        using (FileStorage writer = new FileStorage(
                            "memory.yml",
                            FileStorageModes.Write | FileStorageModes.Memory | FileStorageModes.FormatYaml))
                        {
                            writer.Write("title", "core-persistence");
                            writer.Write("labels", new string[] { "alpha", "", "gamma" });
                            writer.Write("matrix", persistenceMatrix);
                            writer.StartWriteStruct("values", FileNodeTypes.Sequence);
                            writer.Write(string.Empty, 11);
                            writer.Write(string.Empty, 13);
                            writer.EndWriteStruct();
                            persistenceDocument = writer.ReleaseAndGetString();
                        }

                        using (FileStorage reader = new FileStorage(
                            persistenceDocument,
                            FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml))
                        using (FileNode root = reader.Root())
                        using (FileNode title = reader["title"])
                        using (FileNode labels = reader["labels"])
                        using (FileNode matrixNode = reader["matrix"])
                        using (FileNode values = reader["values"])
                        using (FileNode lastValue = values[1])
                        using (Mat restoredMatrix = matrixNode.ToMat())
                        {
                            Console.WriteLine("Core persistence: title=" + title.String
                                + ", keys=" + root.Keys.Length
                                + ", labels=" + labels.Size
                                + ", matrix=" + restoredMatrix.Rows + "x" + restoredMatrix.Cols
                                + ", values=" + values.Size
                                + ", last=" + lastValue.Real);
                        }
                    }

                    using (Mat numericalData = new Mat(3, 2, MatType.CV_64FC1))
                    using (Mat numericalCovar = new Mat())
                    using (Mat numericalMean = new Mat())
                    using (Mat numericalVectors = new Mat())
                    using (Mat numericalValues = new Mat())
                    using (Mat numericalDescriptors = new Mat(3, 2, MatType.CV_32FC1))
                    using (Mat numericalDistances = new Mat())
                    using (Mat numericalIndices = new Mat())
                    using (Mat numericalRandom = new Mat(1, 4, MatType.CV_32FC1))
                    using (Mat lpObjective = new Mat(1, 2, MatType.CV_64FC1))
                    using (Mat lpConstraints = new Mat(3, 3, MatType.CV_64FC1))
                    using (Mat lpSolution = new Mat())
                    {
                        numericalData.CopyFrom(new double[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0 });
                        numericalDescriptors.CopyFrom(new float[] { 1.0F, 2.0F, 3.0F, 4.0F, 5.0F, 6.0F });
                        CoreCv2.CalcCovarMatrix(
                            numericalData,
                            numericalCovar,
                            numericalMean,
                            CovarFlags.Normal | CovarFlags.Rows | CovarFlags.Scale);
                        CoreCv2.PcaCompute(numericalData, numericalMean, numericalVectors, numericalValues, 1);
                        using (Mat numericalProjected = CoreCv2.PcaProject(numericalData, numericalMean, numericalVectors))
                        using (Mat numericalReconstructed = CoreCv2.PcaBackProject(numericalProjected, numericalMean, numericalVectors))
                        {
                            CoreCv2.BatchDistance(
                                numericalDescriptors,
                                numericalDescriptors,
                                numericalDistances,
                                MatType.CV_32F,
                                numericalIndices,
                                NormTypes.L2,
                                1);
                            CoreCv2.SetRngSeed(2026);
                            CoreCv2.Randu(numericalRandom, new Scalar(0.0), new Scalar(1.0));

                            lpObjective.CopyFrom(new double[] { 1.0, 1.0 });
                            lpConstraints.CopyFrom(new double[]
                            {
                                1.0, 0.0, 2.0,
                                0.0, 1.0, 3.0,
                                1.0, 1.0, 4.0
                            });
                            SolveLpResult lpResult = CoreCv2.SolveLp(lpObjective, lpConstraints, lpSolution);

                            Console.WriteLine("Core numerical: cube=" + CoreCv2.CubeRoot(27.0F)
                                + ", angle=" + CoreCv2.FastAtan2(1.0F, 0.0F)
                                + ", covar=" + numericalCovar.Rows + "x" + numericalCovar.Cols
                                + ", components=" + numericalValues.Total.ToUInt64()
                                + ", projected=" + numericalProjected.Rows + "x" + numericalProjected.Cols
                                + ", reconstructed=" + numericalReconstructed.Rows + "x" + numericalReconstructed.Cols
                                + ", nearest=" + string.Join(",", numericalIndices.ToArray<int>())
                                + ", random=" + string.Join(",", numericalRandom.ToArray<float>())
                                + ", lp=" + lpResult);
                        }
                    }

                    int originalThreadCount = CoreCv2.GetNumThreads();
                    bool originalUseOptimized = CoreCv2.UseOptimized();
                    try
                    {
                        CoreCv2.SetNumThreads(1);
                        CoreCv2.SetUseOptimized(!originalUseOptimized);
                        using (TickMeter runtimeMeter = new TickMeter())
                        {
                            runtimeMeter.Start();
                            long runtimeTick = CoreCv2.GetTickCount();
                            runtimeMeter.Stop();
                            Console.WriteLine("Core runtime: version=" + OpenCvSharpBuildInfo.GetNativeOpenCvVersion()
                                + ", cpus=" + CoreCv2.GetNumberOfCpus()
                                + ", threads=" + CoreCv2.GetNumThreads()
                                + ", tickFrequency=" + CoreCv2.GetTickFrequency()
                                + ", tick=" + runtimeTick
                                + ", optimized=" + CoreCv2.UseOptimized()
                                + ", timerCounter=" + runtimeMeter.Counter
                                + ", timerMilliseconds=" + runtimeMeter.TimeMilliseconds
                                + ", cpuFeatures=" + CoreCv2.GetCpuFeaturesLine());
                        }
                    }
                    finally
                    {
                        CoreCv2.SetUseOptimized(originalUseOptimized);
                        CoreCv2.SetNumThreads(originalThreadCount);
                    }

                    using (Mat laA = new Mat(2, 2, MatType.CV_64FC1))
                    using (Mat laB = new Mat(2, 2, MatType.CV_64FC1))
                    using (Mat laBias = new Mat(2, 2, MatType.CV_64FC1, new Scalar(1.0)))
                    using (Mat laGemm = new Mat())
                    using (Mat laRhs = new Mat(2, 1, MatType.CV_64FC1))
                    using (Mat laSolution = new Mat())
                    using (Mat rngValues = new Mat(2, 3, MatType.CV_32SC1))
                    using (Mat vectorX = new Mat(1, 2, MatType.CV_32FC1))
                    using (Mat vectorY = new Mat(1, 2, MatType.CV_32FC1))
                    using (Mat magnitude = new Mat())
                    using (Mat angle = new Mat())
                    using (Mat dftInput = new Mat(1, 4, MatType.CV_64FC1))
                    using (Mat spectrum = new Mat())
                    using (Mat recovered = new Mat())
                    {
                        laA.CopyFrom<double>(new double[] { 1.0, 0.0, 0.0, 2.0 });
                        laB.CopyFrom<double>(new double[] { 5.0, 6.0, 7.0, 8.0 });
                        laRhs.CopyFrom<double>(new double[] { 3.0, 8.0 });
                        vectorX.CopyFrom<float>(new float[] { 3.0F, 0.0F });
                        vectorY.CopyFrom<float>(new float[] { 4.0F, 1.0F });
                        dftInput.CopyFrom<double>(new double[] { 1.0, 2.0, 3.0, 4.0 });

                        CoreCv2.Gemm(laA, laB, 1.0, laBias, 1.0, laGemm);
                        CoreCv2.CartToPolar(vectorX, vectorY, magnitude, angle, angleInDegrees: true);
                        CoreCv2.Dft(dftInput, spectrum, DftFlags.ComplexOutput);
                        CoreCv2.Idft(spectrum, recovered, DftFlags.Scale | DftFlags.RealOutput);

                        using (Svd svd = new Svd(laA))
                        using (Mat singularValues = svd.W)
                        using (Rng rng = new Rng(42UL))
                        {
                            svd.BackSubst(laRhs, laSolution);
                            rng.FillUniform(rngValues, new Scalar(0), new Scalar(10));

                            Console.WriteLine("Core gemm: " + string.Join(",", laGemm.ToArray<double>()));
                            Console.WriteLine("SVD singular values: " + string.Join(",", singularValues.ToArray<double>()));
                            Console.WriteLine("SVD solution: " + string.Join(",", laSolution.ToArray<double>()));
                            Console.WriteLine("RNG values: " + string.Join(",", rngValues.ToArray<int>()));
                            Console.WriteLine("CartToPolar magnitude: " + string.Join(",", magnitude.ToArray<float>()));
                            Console.WriteLine("CartToPolar angle: " + string.Join(",", angle.ToArray<float>()));
                            Console.WriteLine("DFT recovered: " + string.Join(",", recovered.ToArray<double>()));
                        }
                    }

                    ImgProcCv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
                    ImgProcCv2.Resize(gray, resized, new Size(4, 4), interpolation: InterpolationFlags.Nearest);
                    ImgProcCv2.Threshold(gray, thresholded, 127, 255, ThresholdTypes.Binary);
                    ImgProcCv2.GaussianBlur(thresholded, blurred, new Size(3, 3), 0, 0);
                    ImgProcCv2.BoxFilter(gray, boxFiltered, -1, new Size(3, 3));
                    ImgProcCv2.Sobel(gray, sobelX, MatType.CV_16S, 1, 0);
                    ImgProcCv2.Canny(gray, edges, 32.0, 96.0);
                    ImgProcCv2.WarpAffine(gray, warped, rotation, new Size(2, 2), InterpolationFlags.Nearest);
                    mapX.CopyFrom<float>(new float[] { 0, 1, 0, 1 });
                    mapY.CopyFrom<float>(new float[] { 0, 0, 1, 1 });
                    ImgProcCv2.Remap(gray, remapped, mapX, mapY, InterpolationFlags.Nearest);

                    byte[] png = ImgCodecsCv2.ImEncode(".png", src, new int[]
                    {
                        (int)ImwriteFlags.PngCompression, 9
                    });
                    using (Mat decoded = ImgCodecsCv2.ImDecode(png, ImreadModes.Color))
                    {
                        Console.WriteLine("PNG bytes: " + png.Length);
                        Console.WriteLine("Decoded: " + decoded.Rows + "x" + decoded.Cols + ", channels=" + decoded.Channels);
                    }

                    string samplePath = Path.Combine(Path.GetTempPath(), "opencv-csharp-sample.png");
                    ImgCodecsCv2.ImWrite(samplePath, src);
                    using (Mat fileImage = ImgCodecsCv2.ImRead(samplePath, ImreadModes.Color))
                    {
                        Console.WriteLine("File image: " + fileImage.Rows + "x" + fileImage.Cols + ", channels=" + fileImage.Channels);
                    }

                    File.Delete(samplePath);

                    byte[] grayPixels = new byte[gray.ByteLength];
                    byte[] thresholdPixels = new byte[thresholded.ByteLength];
                    gray.CopyTo(grayPixels);
                    thresholded.CopyTo(thresholdPixels);

                    Console.WriteLine("Gray: " + gray.Rows + "x" + gray.Cols + ", channels=" + gray.Channels);
                    Console.WriteLine("Resized: " + resized.Rows + "x" + resized.Cols + ", channels=" + resized.Channels);
                    Console.WriteLine("Gray pixels: " + string.Join(",", grayPixels));
                    Console.WriteLine("Threshold pixels: " + string.Join(",", thresholdPixels));
                    Console.WriteLine("Blurred: " + blurred.Rows + "x" + blurred.Cols + ", channels=" + blurred.Channels);
                    Console.WriteLine("Box filter: " + boxFiltered.Rows + "x" + boxFiltered.Cols + ", type=" + boxFiltered.Type);
                    Console.WriteLine("Sobel X: " + sobelX.Rows + "x" + sobelX.Cols + ", type=" + sobelX.Type);
                    Console.WriteLine("Canny: " + string.Join(",", edges.ToBytes()));
                    Console.WriteLine("Gaussian kernel: " + string.Join(",", gaussianKernel.ToArray<double>()));
                    Console.WriteLine("Gabor kernel: " + gaborKernel.Rows + "x" + gaborKernel.Cols + ", type=" + gaborKernel.Type);
                    Console.WriteLine("Warp affine: " + string.Join(",", warped.ToBytes()));
                    Console.WriteLine("Remap: " + string.Join(",", remapped.ToBytes()));
                    Console.WriteLine("Morph kernel: " + kernel.Rows + "x" + kernel.Cols + ", type=" + kernel.Type);

                    morphSrc.CopyFrom(new byte[]
                    {
                        0, 0, 0, 0, 0,
                        0, 255, 255, 255, 0,
                        0, 255, 255, 255, 0,
                        0, 255, 255, 255, 0,
                        0, 0, 0, 0, 0
                    });

                    ImgProcCv2.Erode(morphSrc, eroded, kernel);
                    ImgProcCv2.Dilate(morphSrc, dilated, kernel);
                    ImgProcCv2.MorphologyEx(morphSrc, opened, MorphTypes.Open, kernel);

                    byte[] erodedPixels = new byte[eroded.ByteLength];
                    byte[] dilatedPixels = new byte[dilated.ByteLength];
                    byte[] openedPixels = new byte[opened.ByteLength];
                    eroded.CopyTo(erodedPixels);
                    dilated.CopyTo(dilatedPixels);
                    opened.CopyTo(openedPixels);

                    Console.WriteLine("Eroded pixels: " + string.Join(",", erodedPixels));
                    Console.WriteLine("Dilated pixels: " + string.Join(",", dilatedPixels));
                    Console.WriteLine("Opened pixels: " + string.Join(",", openedPixels));

                    byte[] drawingSource = new byte[drawing.ByteLength];
                    drawing.CopyFrom(drawingSource);

                    ImgProcCv2.Line(drawing, new Point(0, 0), new Point(4, 4), new Scalar(255));
                    ImgProcCv2.ArrowedLine(drawing, new Point(32, 8), new Point(112, 8), new Scalar(180), tipLength: 0.2);
                    Point clipPt1 = new Point(-5, 2);
                    Point clipPt2 = new Point(15, 2);
                    bool clipIntersects = ImgProcCv2.ClipLine(new Rect(0, 0, 10, 10), ref clipPt1, ref clipPt2);
                    ImgProcCv2.Rectangle(drawing, new Rect(1, 1, 3, 3), new Scalar(128), -1);
                    ImgProcCv2.Polylines(
                        drawing,
                        new Point[]
                        {
                            new Point(48, 18),
                            new Point(64, 30),
                            new Point(80, 18)
                        },
                        true,
                        new Scalar(210));
                    ImgProcCv2.FillPoly(
                        drawing,
                        new Point[]
                        {
                            new Point(88, 18),
                            new Point(104, 30),
                            new Point(116, 18)
                        },
                        new Scalar(90));
                    Point[] ellipsePoints = ImgProcCv2.Ellipse2Poly(new Point(96, 28), new Size(10, 6), 0, 0, 270, 30);
                    Point[] contour = new Point[]
                    {
                        new Point(0, 0),
                        new Point(4, 0),
                        new Point(4, 3),
                        new Point(0, 3)
                    };
                    double contourArea = ImgProcCv2.ContourArea(contour);
                    double contourAreaFromSpan = ImgProcCv2.ContourArea(contour.AsSpan());
                    double contourLength = ImgProcCv2.ArcLength(contour, true);
                    double contourLengthFromSpan = ImgProcCv2.ArcLength(contour.AsSpan(), true);
                    Point[] approxContour = ImgProcCv2.ApproxPolyDP(
                        new Point[]
                        {
                            new Point(0, 0),
                            new Point(2, 0),
                            new Point(4, 0),
                            new Point(4, 3),
                            new Point(0, 3)
                        },
                        0.5,
                        true);
                    Rect boundingRect = ImgProcCv2.BoundingRect(approxContour);
                    bool isConvex = ImgProcCv2.IsContourConvex(approxContour);
                    Point[] convexHull = ImgProcCv2.ConvexHull(approxContour);
                    Point[] convexHullFromSpan = ImgProcCv2.ConvexHull(approxContour.AsSpan());
                    int[] convexHullIndices = ImgProcCv2.ConvexHullIndices(approxContour);
                    Point2f[] approxConvexPolygon = ImgProcCv2.ApproxPolyN(approxContour, 4);
                    Point2f[] approxConvexPolygonFromSpan = ImgProcCv2.ApproxPolyN(approxContour.AsSpan(), 4);
                    Point[] concaveContour = new Point[]
                    {
                        new Point(0, 0),
                        new Point(4, 0),
                        new Point(4, 4),
                        new Point(2, 2),
                        new Point(0, 4)
                    };
                    int[] concaveHullIndices = ImgProcCv2.ConvexHullIndices(concaveContour);
                    Vec4i[] convexityDefects = ImgProcCv2.ConvexityDefects(concaveContour, concaveHullIndices);
                    ImgProcCv2.MinEnclosingCircle(approxContour, out Point2f enclosingCenter, out float enclosingRadius);
                    double polygonTest = ImgProcCv2.PointPolygonTest(approxContour, new Point2f(2.0F, 1.0F), false);
                    double shapeDistance = ImgProcCv2.MatchShapes(contour, approxContour, ShapeMatchModes.I1);
                    RotatedRect minAreaRect = ImgProcCv2.MinAreaRect(approxContour);
                    Point2f[] boxPoints = ImgProcCv2.BoxPoints(minAreaRect);
                    Point[] ellipseFitPoints = new Point[]
                    {
                        new Point(0, 2),
                        new Point(1, 0),
                        new Point(3, 0),
                        new Point(4, 2),
                        new Point(3, 4),
                        new Point(1, 4)
                    };
                    RotatedRect fitEllipse = ImgProcCv2.FitEllipse(ellipseFitPoints);
                    RotatedRect fitEllipseAms = ImgProcCv2.FitEllipseAMS(ellipseFitPoints);
                    RotatedRect fitEllipseDirect = ImgProcCv2.FitEllipseDirect(ellipseFitPoints);
                    RotatedRect fitEllipseFromSpan = ImgProcCv2.FitEllipse(ellipseFitPoints.AsSpan());
                    RotatedRect fitEllipseAmsFromSpan = ImgProcCv2.FitEllipseAMS(ellipseFitPoints.AsSpan());
                    RotatedRect fitEllipseDirectFromSpan = ImgProcCv2.FitEllipseDirect(ellipseFitPoints.AsSpan());
                    RectanglesIntersectTypes intersectionType = ImgProcCv2.RotatedRectangleIntersection(
                        minAreaRect,
                        new RotatedRect(new Point2f(2.5F, 1.5F), new Size2f(4.0F, 3.0F), 0.0F),
                        out Point2f[] intersectionRegion);
                    Point2f[] closestEllipsePoints = ImgProcCv2.GetClosestEllipsePoints(fitEllipse, ellipseFitPoints);
                    Point2f[] closestEllipsePointsFromSpan = ImgProcCv2.GetClosestEllipsePoints(fitEllipse, ellipseFitPoints.AsSpan());
                    double enclosingTriangleArea = ImgProcCv2.MinEnclosingTriangle(approxContour, out Point2f[] enclosingTriangle);
                    double enclosingTriangleAreaFromSpan = ImgProcCv2.MinEnclosingTriangle(approxContour.AsSpan(), out Point2f[] enclosingTriangleFromSpan);
                    double enclosingConvexPolygonArea = ImgProcCv2.MinEnclosingConvexPolygon(approxContour, 4, out Point2f[] enclosingConvexPolygon);
                    double enclosingConvexPolygonAreaFromSpan = ImgProcCv2.MinEnclosingConvexPolygon(approxContour.AsSpan(), 4, out Point2f[] enclosingConvexPolygonFromSpan);
                    Point[] intersectingContour = new Point[]
                    {
                        new Point(2, 0),
                        new Point(6, 0),
                        new Point(6, 3),
                        new Point(2, 3)
                    };
                    float intersectConvexArea = ImgProcCv2.IntersectConvexConvex(
                        contour,
                        intersectingContour,
                        out Point2f[] intersectConvexRegion);
                    float intersectConvexAreaFromSpan = ImgProcCv2.IntersectConvexConvex(
                        contour.AsSpan(),
                        intersectingContour.AsSpan(),
                        out Point2f[] intersectConvexRegionFromSpan);
                    Vec4f fitLine = ImgProcCv2.FitLine(
                        new Point[]
                        {
                            new Point(0, 1),
                            new Point(2, 5),
                            new Point(4, 9)
                        },
                        DistanceTypes.L2,
                        0.0,
                        0.01,
                        0.01);
                    ImgProcCv2.Polylines(drawing, ellipsePoints, false, new Scalar(150));
                    ImgProcCv2.Circle(drawing, new Point(12, 12), 6, new Scalar(200), 1);
                    ImgProcCv2.Ellipse(drawing, new Point(24, 12), new Size(8, 4), 0, 0, 360, new Scalar(64), 1);
                    int baseLine;
                    Size textSize = ImgProcCv2.GetTextSize("OpenCV", HersheyFonts.HersheySimplex, 0.45, 1, out baseLine);
                    ImgProcCv2.PutText(
                        drawing,
                        "OpenCV",
                        new Point(4, 32),
                        HersheyFonts.HersheySimplex,
                        0.45,
                        new Scalar(255));

                    byte[] drawingPixels = new byte[drawing.ByteLength];
                    drawing.CopyTo(drawingPixels);
                    Console.WriteLine("ClipLine: " + clipIntersects + ", pt1=" + clipPt1 + ", pt2=" + clipPt2);
                    Console.WriteLine("Ellipse2Poly points: " + ellipsePoints.Length);
                    Console.WriteLine("Contour area: " + contourArea);
                    Console.WriteLine("Contour area span: " + contourAreaFromSpan);
                    Console.WriteLine("Contour length: " + contourLength);
                    Console.WriteLine("Contour length span: " + contourLengthFromSpan);
                    Console.WriteLine("Approx contour points: " + approxContour.Length);
                    Console.WriteLine("Bounding rect: " + boundingRect);
                    Console.WriteLine("Is convex: " + isConvex);
                    Console.WriteLine("Convex hull points: " + convexHull.Length);
                    Console.WriteLine("Convex hull span points: " + convexHullFromSpan.Length);
                    Console.WriteLine("Convex hull indices: " + convexHullIndices.Length);
                    Console.WriteLine("Approx convex polygon points: " + approxConvexPolygon.Length);
                    Console.WriteLine("Approx convex polygon span points: " + approxConvexPolygonFromSpan.Length);
                    Console.WriteLine("Convexity defects: " + convexityDefects.Length);
                    Console.WriteLine("Min enclosing circle: center=" + enclosingCenter + ", radius=" + enclosingRadius);
                    Console.WriteLine("Point polygon test: " + polygonTest);
                    Console.WriteLine("Shape distance: " + shapeDistance);
                    Console.WriteLine("Min area rect: " + minAreaRect);
                    Console.WriteLine("Box points: " + boxPoints.Length);
                    Console.WriteLine("Fit ellipse: " + fitEllipse);
                    Console.WriteLine("Fit ellipse AMS: " + fitEllipseAms);
                    Console.WriteLine("Fit ellipse direct: " + fitEllipseDirect);
                    Console.WriteLine("Fit ellipse span: " + fitEllipseFromSpan);
                    Console.WriteLine("Fit ellipse AMS span: " + fitEllipseAmsFromSpan);
                    Console.WriteLine("Fit ellipse direct span: " + fitEllipseDirectFromSpan);
                    Console.WriteLine("Rotated rect intersection: " + intersectionType + ", points=" + intersectionRegion.Length);
                    Console.WriteLine("Closest ellipse points: " + closestEllipsePoints.Length);
                    Console.WriteLine("Closest ellipse span points: " + closestEllipsePointsFromSpan.Length);
                    Console.WriteLine("Min enclosing triangle: area=" + enclosingTriangleArea + ", points=" + enclosingTriangle.Length);
                    Console.WriteLine("Min enclosing triangle span: area=" + enclosingTriangleAreaFromSpan + ", points=" + enclosingTriangleFromSpan.Length);
                    Console.WriteLine("Min enclosing convex polygon: area=" + enclosingConvexPolygonArea + ", points=" + enclosingConvexPolygon.Length);
                    Console.WriteLine("Min enclosing convex polygon span: area=" + enclosingConvexPolygonAreaFromSpan + ", points=" + enclosingConvexPolygonFromSpan.Length);
                    Console.WriteLine("Intersect convex convex: area=" + intersectConvexArea + ", points=" + intersectConvexRegion.Length);
                    Console.WriteLine("Intersect convex convex span: area=" + intersectConvexAreaFromSpan + ", points=" + intersectConvexRegionFromSpan.Length);
                    Console.WriteLine("Fit line: " + fitLine);
                    Console.WriteLine("Text size: " + textSize + ", baseLine=" + baseLine);
                    Console.WriteLine("Drawing bytes: " + drawingPixels.Length);
                    Console.WriteLine("Drawing first pixels: " + string.Join(",", new byte[]
                    {
                        drawingPixels[0],
                        drawingPixels[1],
                        drawingPixels[2],
                        drawingPixels[3],
                        drawingPixels[4],
                        drawingPixels[5],
                        drawingPixels[6],
                        drawingPixels[7]
                    }));
                    Console.WriteLine(RunImgProcUpstreamParitySummary());
                    Console.WriteLine(RunImgProcRemainingParitySummary());
                    Console.WriteLine(RunImgCodecsUpstreamParitySummary());
                    Console.WriteLine(RunCalib3DUpstreamParitySummary());
                    Console.WriteLine(RunVideoIODefaultSummary());
                    Console.WriteLine(RunVideoOpticalFlowObjectSummary());
                    Console.WriteLine(RunVideoEccTrackerMilSummary());
                    Console.WriteLine(RunDnnStructuredDefaultSummary());
                    Console.WriteLine(RunFeaturesAnnIndexSummary());
                    Console.WriteLine(RunObjDetectStructuredSummary());
                    Console.WriteLine(RunPhotoHdrDefaultSummary());
                    Console.WriteLine(RunPhotoCcmDefaultSummary());
                    Console.WriteLine(RunPhotoIntelligentScissorsDefaultSummary());
                    Console.WriteLine(RunPhotoFinalCallablesDefaultSummary());
                    Console.WriteLine(RunMLTreeModelsDefaultSummary());
                    Console.WriteLine(RunMLEMDefaultSummary());
                    Console.WriteLine(RunMLRemainingCallablesDefaultSummary());
                    Console.WriteLine(RunTrackingDefaultSummary());
                    Console.WriteLine(RunExposureCompensationSummary());
                    Console.WriteLine(RunPyRotationWarperSummary());
                    Console.WriteLine(RunBlenderSummary());
                    Console.WriteLine(RunFeaturesMatcherSummary());
                    Console.WriteLine(RunStitchingDetailSummary());

                    if (!IsExtendedConsoleSamplesEnabled())
                    {
                        PrintStableRound73Summaries();
                        return;
                    }

                    analysis.CopyFrom(new byte[]
                    {
                        0, 0, 0, 0, 0, 0, 0, 0,
                        0, 40, 60, 60, 40, 0, 0, 0,
                        0, 60, 180, 180, 60, 0, 0, 0,
                        0, 60, 180, 180, 60, 0, 0, 0,
                        0, 40, 60, 60, 40, 0, 0, 0,
                        0, 0, 0, 0, 0, 0, 220, 0,
                        0, 0, 0, 0, 0, 0, 220, 0,
                        0, 0, 0, 0, 0, 0, 0, 0
                    });

                    ImgProcCv2.EqualizeHist(analysis, equalized);
                    ImgProcCv2.AdaptiveThreshold(equalized, adaptive, 255, AdaptiveThresholdTypes.MeanC, ThresholdTypes.Binary, 3, 2);
                    ImgProcCv2.Integral3(adaptive, integral, integralSq, integralTilted);
                    ImgProcCv2.DistanceTransform(adaptive, distance, distanceLabels, DistanceTypes.L2, DistanceTransformMasks.Mask3);
                    int labelCount = ImgProcCv2.ConnectedComponentsWithStats(adaptive, connectedLabels, connectedStats, connectedCentroids);
                    ImgProcCv2.FindContours(adaptive, out Point[][] foundContours, out Vec4i[] foundHierarchy, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
                    ImgProcCv2.DrawContours(contourCanvas, foundContours, -1, new Scalar(255), 1, hierarchy: foundHierarchy);
                    Moments imageMoments = ImgProcCv2.Moments(adaptive, true);
                    double[] huMoments = ImgProcCv2.HuMoments(imageMoments);
                    ImgProcCv2.CornerHarris(adaptive, cornerResponse, 3, 3, 0.04);

                    using (CLAHE clahe = ImgProcCv2.CreateCLAHE(2.0, new Size(4, 4)))
                    {
                        clahe.ClipLimit = 3.0;
                        clahe.TilesGridSize = new Size(2, 2);
                        clahe.BitShift = 0;
                        clahe.Apply(analysis, claheResult);
                        clahe.CollectGarbage();
                    }

                    ImgProcCv2.CalcHist(analysis, 0, null, hist, 8, 0, 256);
                    ImgProcCv2.CalcHist(claheResult, new[] { 0 }, null, claheHist, new[] { 8 }, new[] { 0F, 256F });
                    ImgProcCv2.CalcBackProject(analysis, 0, hist, backProject, 0, 256);
                    double histogramCorrelation = ImgProcCv2.CompareHist(hist, claheHist, HistogramComparisonTypes.Correl);

                    houghBinary.SetTo(new Scalar(0));
                    houghCircleImage.SetTo(new Scalar(0));
                    lsdDrawing.SetTo(new Scalar(0, 0, 0));
                    ImgProcCv2.Line(houghBinary, new Point(5, 8), new Point(58, 8), new Scalar(255), 1);
                    ImgProcCv2.Line(houghBinary, new Point(5, 20), new Point(58, 50), new Scalar(255), 1);
                    ImgProcCv2.Circle(houghCircleImage, new Point(32, 32), 12, new Scalar(255), 2);

                    HoughLine[] houghLines = ImgProcCv2.HoughLines(houghBinary, 1.0, Math.PI / 180.0, 20);
                    Vec4i[] houghSegments = ImgProcCv2.HoughLinesP(houghBinary, 1.0, Math.PI / 180.0, 10, 8.0, 2.0);
                    HoughCircle[] houghCircles = ImgProcCv2.HoughCircles(houghCircleImage, HoughModes.Gradient, 1.0, 16.0, 80.0, 8.0, 5, 20);
                    Point[] houghPointSet = new Point[]
                    {
                        new Point(0, 0),
                        new Point(5, 5),
                        new Point(10, 10),
                        new Point(15, 15),
                        new Point(20, 20)
                    };
                    HoughLinePointSet[] pointSetLines = ImgProcCv2.HoughLinesPointSet(
                        houghPointSet,
                        4,
                        2,
                        -50,
                        50,
                        1,
                        0,
                        Math.PI,
                        Math.PI / 180.0);

                    var refinedCorners = new[]
                    {
                        new Point2f(3.0F, 3.0F),
                        new Point2f(6.0F, 6.0F)
                    };
#if NETCOREAPP3_1_OR_GREATER
                    HoughLinePointSet[] pointSetLinesFromSpan = ImgProcCv2.HoughLinesPointSet(
                        houghPointSet.AsSpan(),
                        4,
                        2,
                        -50,
                        50,
                        1,
                        0,
                        Math.PI,
                        Math.PI / 180.0);
#endif

                    string goodFeaturesSummary;
                    try
                    {
                        Point2f[] goodCorners = ImgProcCv2.GoodFeaturesToTrack(adaptive, 8, 0.01, 2.0);
                        goodFeaturesSummary = "GoodFeaturesToTrack corners: " + goodCorners.Length;
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("good_features_to_track", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        goodFeaturesSummary = "GoodFeaturesToTrack boundary: " + ex.Message;
                    }

                    string features2dSummary;
                    try
                    {
                        using (ORB orb = ORB.Create(maxFeatures: 64, fastThreshold: 8))
                        using (SIFT sift = SIFT.Create(nFeatures: 64))
                        using (FastFeatureDetector fast = FastFeatureDetector.Create(threshold: 12))
                        using (GFTTDetector gftt = GFTTDetector.Create(maxCorners: 16, qualityLevel: 0.01, minDistance: 2.0))
                        using (AffineFeature affineOrb = AffineFeature.Create(orb, maxTilt: 1, minTilt: 0))
                        using (Mat orbDescriptors = new Mat())
                        using (Mat siftDescriptors = new Mat())
                        using (Mat featureDrawing = new Mat())
                        {
                            KeyPoint[] orbKeypoints = orb.Detect(houghBinary);
                            KeyPoint[] computedOrbKeypoints = orb.Compute(houghBinary, orbKeypoints, orbDescriptors);
                            KeyPoint[] siftKeypoints = sift.DetectAndCompute(houghBinary, null, Array.Empty<KeyPoint>(), siftDescriptors, useProvidedKeypoints: false);
                            KeyPoint[] fastKeypoints = fast.Detect(houghBinary);
                            KeyPoint[] gfttKeypoints = gftt.Detect(houghBinary);
                            Features2DCv2.DrawKeypoints(
                                houghBinary,
                                computedOrbKeypoints,
                                featureDrawing,
                                new Scalar(0, 255, 0),
                                DrawMatchesFlags.DrawRichKeypoints);

                            KeyPoint[][] orbBatch = orb.Detect(new[] { houghBinary, houghCircleImage });
                            string fastSpanBatchSummary = string.Empty;
                            string affineSpanSummary = string.Empty;
                            string briskSummary = string.Empty;
                            string kazeSummary = string.Empty;
                            string akazeSummary = string.Empty;
                            string bowSummary = string.Empty;

                            affineOrb.SetViewParams(new[] { 1.0F, 2.0F }, new[] { 0.0F, 45.0F });
                            affineOrb.GetViewParams(out float[] affineTilts, out float[] affineRolls);
                            KeyPoint[] affineKeypoints = affineOrb.Detect(houghBinary);

#if NETCOREAPP3_1_OR_GREATER
                            KeyPoint[][] fastBatch = fast.Detect(new[] { houghBinary, houghCircleImage }.AsSpan());
                            fastSpanBatchSummary = ", FAST span batch=" + fastBatch.Length;

                            ReadOnlySpan<float> affineSpanTilts = stackalloc float[] { 1.0F, 1.41421356F };
                            ReadOnlySpan<float> affineSpanRolls = stackalloc float[] { 0.0F, 30.0F };
                            Span<float> returnedAffineTilts = stackalloc float[2];
                            Span<float> returnedAffineRolls = stackalloc float[2];
                            affineOrb.SetViewParams(affineSpanTilts, affineSpanRolls);
                            int affineSpanViews = affineOrb.GetViewParams(returnedAffineTilts, returnedAffineRolls);
                            affineSpanSummary = ", Affine span views=" + affineSpanViews;
#endif

                            using (DescriptorMatcher bfMatcher = BFMatcher.Create(orb.DefaultNorm))
                            using (DescriptorMatcher bowMatcher = BFMatcher.Create(NormTypes.L2))
                            using (DescriptorMatcher flannMatcher = FlannBasedMatcher.Create())
                            using (MSER mser = MSER.Create(delta: 5, minArea: 20, maxArea: 22000))
                            using (Mat mserImage = new Mat(160, 160, MatType.CV_8UC1))
                            using (Mat blobImage = new Mat(160, 160, MatType.CV_8UC1))
                            {
                                mserImage.SetTo(new Scalar(0));
                                ImgProcCv2.Rectangle(mserImage, new Rect(16, 16, 128, 128), new Scalar(48), -1);
                                ImgProcCv2.Rectangle(mserImage, new Rect(36, 36, 88, 88), new Scalar(144), -1);
                                ImgProcCv2.Rectangle(mserImage, new Rect(56, 56, 48, 48), new Scalar(224), -1);
                                ImgProcCv2.Circle(mserImage, new Point(112, 48), 18, new Scalar(192), -1);
                                ImgProcCv2.Circle(mserImage, new Point(48, 112), 16, new Scalar(96), -1);
                                MserRegion[] mserRegions = mser.DetectRegions(mserImage);
                                KeyPoint[] mserKeypoints = mser.Detect(mserImage);

                                blobImage.SetTo(new Scalar(255));
                                ImgProcCv2.Circle(blobImage, new Point(40, 40), 12, new Scalar(0), -1);
                                ImgProcCv2.Circle(blobImage, new Point(112, 48), 16, new Scalar(0), -1);
                                ImgProcCv2.Circle(blobImage, new Point(80, 112), 20, new Scalar(0), -1);
                                SimpleBlobDetectorParams blobParams = new SimpleBlobDetectorParams
                                {
                                    ThresholdStep = 5.0F,
                                    MinThreshold = 0.0F,
                                    MaxThreshold = 255.0F,
                                    MinRepeatability = 1,
                                    MinDistBetweenBlobs = 5.0F,
                                    FilterByColor = true,
                                    BlobColor = 0,
                                    FilterByArea = true,
                                    MinArea = 20.0F,
                                    MaxArea = 2500.0F,
                                    FilterByCircularity = false,
                                    FilterByInertia = false,
                                    FilterByConvexity = false,
                                    CollectContours = true
                                };

                                using (Feature2D blobDetector = SimpleBlobDetector.Create(blobParams))
                                {
                                    KeyPoint[] blobKeypoints = blobDetector.Detect(blobImage);
                                    KeyPoint[][] blobBatch = blobDetector.Detect(new[] { blobImage, blobImage });
                                    Point[][] blobContours = ((SimpleBlobDetector)blobDetector).GetBlobContours();
                                    try
                                    {
                                        using (BRISK brisk = BRISK.Create(threshold: 24, octaves: 2))
                                        using (KAZE kaze = KAZE.Create(nOctaves: 3, nOctaveLayers: 3))
                                        using (AKAZE akaze = AKAZE.Create(nOctaves: 3, nOctaveLayers: 3, maxPoints: 128))
                                        using (Mat briskDescriptors = new Mat())
                                        using (Mat kazeDescriptors = new Mat())
                                        using (Mat akazeDescriptors = new Mat())
                                        {
                                            KeyPoint[] briskKeypoints = brisk.Detect(houghBinary);
                                            KeyPoint[] kazeKeypoints = kaze.Detect(houghBinary);
                                            KeyPoint[] akazeKeypoints = akaze.Detect(houghBinary);
                                            brisk.Compute(houghBinary, briskKeypoints, briskDescriptors);
                                            kaze.DetectAndCompute(houghBinary, null, out KeyPoint[] kazeDetected, kazeDescriptors);
                                            akaze.DetectAndCompute(houghBinary, null, out KeyPoint[] akazeDetected, akazeDescriptors);
                                            briskSummary = ", BRISK keypoints=" + briskKeypoints.Length + ", BRISK name=" + brisk.DefaultName + ", BRISK descriptor rows=" + briskDescriptors.Rows;
                                            kazeSummary = ", KAZE keypoints=" + kazeKeypoints.Length + ", KAZE detected=" + kazeDetected.Length + ", KAZE name=" + kaze.DefaultName + ", KAZE descriptor rows=" + kazeDescriptors.Rows;
                                            akazeSummary = ", AKAZE keypoints=" + akazeKeypoints.Length + ", AKAZE detected=" + akazeDetected.Length + ", AKAZE name=" + akaze.DefaultName + ", AKAZE descriptor rows=" + akazeDescriptors.Rows;
                                        }
                                    }
                                    catch (OpenCvException ex) when (ex.Message.IndexOf("xfeatures2d", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        briskSummary = ", BRISK boundary=" + ex.Message;
                                        kazeSummary = string.Empty;
                                        akazeSummary = string.Empty;
                                    }

                                    DMatch[] orbSelfMatches = orbDescriptors.Empty
                                        ? Array.Empty<DMatch>()
                                        : bfMatcher.Match(orbDescriptors, orbDescriptors);
                                    DMatch[] siftSelfMatches = siftDescriptors.Empty
                                        ? Array.Empty<DMatch>()
                                        : flannMatcher.Match(siftDescriptors, siftDescriptors);
                                    if (!orbDescriptors.Empty)
                                    {
                                        bfMatcher.Add(new[] { orbDescriptors });
                                        bfMatcher.Train();
                                    }

                                    if (!siftDescriptors.Empty)
                                    {
                                        flannMatcher.Add(new[] { siftDescriptors });
                                        flannMatcher.Train();
                                    }

                                    DMatch[] trainedOrbMatches = orbDescriptors.Empty ? Array.Empty<DMatch>() : bfMatcher.Match(orbDescriptors);
                                    DMatch[] trainedSiftMatches = siftDescriptors.Empty ? Array.Empty<DMatch>() : flannMatcher.Match(siftDescriptors);
                                    if (!siftDescriptors.Empty && siftDescriptors.Rows >= 2)
                                    {
                                        using (BOWKMeansTrainer bowTrainer = new BOWKMeansTrainer(2, TermCriteria.ByCountAndEpsilon(20, 0.001), attempts: 1))
                                        using (Mat bowVocabulary = bowTrainer.Cluster(siftDescriptors))
                                        using (BOWImgDescriptorExtractor bowExtractor = new BOWImgDescriptorExtractor(sift, bowMatcher))
                                        using (Mat bowImageDescriptor = new Mat())
                                        {
                                            bowExtractor.SetVocabulary(bowVocabulary);
                                            bowExtractor.Compute(houghBinary, siftKeypoints, bowImageDescriptor);
                                            bowSummary = ", BOW vocab=" + bowVocabulary.Rows + "x" + bowVocabulary.Cols
                                                + ", BOW descriptor=" + bowImageDescriptor.Rows + "x" + bowImageDescriptor.Cols;
                                        }
                                    }

                                    features2dSummary = "Features2D ORB keypoints: " + computedOrbKeypoints.Length
                                        + ", ORB name=" + orb.DefaultName
                                        + ", ORB descriptor rows=" + orbDescriptors.Rows
                                        + ", ORB self matches=" + orbSelfMatches.Length
                                        + ", ORB trained matches=" + trainedOrbMatches.Length
                                        + ", ORB batch=" + orbBatch.Length
                                        + ", Affine name=" + affineOrb.DefaultName
                                        + ", Affine keypoints=" + affineKeypoints.Length
                                        + ", Affine views=" + affineTilts.Length + "/" + affineRolls.Length
                                        + affineSpanSummary
                                        + ", SIFT keypoints=" + siftKeypoints.Length
                                        + ", SIFT descriptor rows=" + siftDescriptors.Rows
                                        + ", SIFT FLANN matches=" + siftSelfMatches.Length
                                        + ", SIFT trained matches=" + trainedSiftMatches.Length
                                        + ", FAST keypoints=" + fastKeypoints.Length
                                        + fastSpanBatchSummary
                                        + ", GFTT keypoints=" + gfttKeypoints.Length
                                        + ", MSER keypoints=" + mserKeypoints.Length
                                        + ", MSER regions=" + mserRegions.Length
                                        + ", SimpleBlob keypoints=" + blobKeypoints.Length
                                        + ", SimpleBlob contours=" + blobContours.Length
                                        + ", SimpleBlob batch=" + blobBatch.Length
                                        + bowSummary
                                        + briskSummary
                                        + kazeSummary
                                        + akazeSummary
                                        + ", drawing=" + featureDrawing.Rows + "x" + featureDrawing.Cols;
                                }
                            }
                        }
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("features2d", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        features2dSummary = "Features2D boundary: " + ex.Message;
                    }

                    string calib3dSummary;
                    try
                    {
                        Point3f[] objectPoints = new[]
                        {
                            new Point3f(-1.0F, -1.0F, 0.0F),
                            new Point3f(1.0F, -1.0F, 0.0F),
                            new Point3f(1.0F, 1.0F, 0.0F),
                            new Point3f(-1.0F, 1.0F, 0.0F)
                        };
                        Point2f[] imagePoints = new[]
                        {
                            new Point2f(100.0F, 100.0F),
                            new Point2f(200.0F, 100.0F),
                            new Point2f(200.0F, 200.0F),
                            new Point2f(100.0F, 200.0F)
                        };

                        using (Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_64FC1))
                        using (Mat distCoeffs = new Mat(1, 5, MatType.CV_64FC1))
                        using (Mat rvec = new Mat())
                        using (Mat tvec = new Mat())
                        using (Mat chessboardImage = new Mat(80, 80, MatType.CV_8UC1))
                        using (Mat chessboardCorners = new Mat())
                        using (Mat circlesCenters = new Mat())
                        using (Mat stereoLeft = new Mat(64, 64, MatType.CV_8UC1))
                        using (Mat stereoRight = new Mat(64, 64, MatType.CV_8UC1))
                        using (Mat disparity = new Mat())
                        using (Mat srcQuad = Calib3DCv2.ToPointMat(new[]
                        {
                            new Point2f(0.0F, 0.0F),
                            new Point2f(1.0F, 0.0F),
                            new Point2f(1.0F, 1.0F),
                            new Point2f(0.0F, 1.0F)
                        }))
                        using (Mat dstQuad = Calib3DCv2.ToPointMat(imagePoints))
                        using (Mat homography = Calib3DCv2.FindHomography(srcQuad, dstQuad))
                        {
                            cameraMatrix.SetValue(0, 100.0);
                            cameraMatrix.SetValue(2, 150.0);
                            cameraMatrix.SetValue(4, 100.0);
                            cameraMatrix.SetValue(5, 150.0);
                            distCoeffs.SetTo(new Scalar(0.0));

                            bool solved = Calib3DCv2.SolvePnP(
                                objectPoints,
                                imagePoints,
                                cameraMatrix,
                                distCoeffs,
                                rvec,
                                tvec,
                                flags: SolvePnPFlags.IPPE);
                            SolvePnPGenericResult generic = Calib3DCv2.SolvePnPGeneric(
                                objectPoints,
                                imagePoints,
                                cameraMatrix,
                                distCoeffs,
                                flags: SolvePnPFlags.IPPE,
                                returnReprojectionError: true);
                            using (Mat calibRotation = Calib3DCv2.Rodrigues(rvec))
                            using (Mat projected = Calib3DCv2.ProjectPoints(objectPoints, rvec, tvec, cameraMatrix, distCoeffs))
                            using (StereoBM stereo = StereoBM.Create(16, 9))
                            {
                                try
                                {
                                    Calib3DCv2.SolvePnPRefineLM(objectPoints, imagePoints, cameraMatrix, distCoeffs, rvec, tvec);
                                    Calib3DCv2.SolvePnPRefineVVS(objectPoints, imagePoints, cameraMatrix, distCoeffs, rvec, tvec);
                                    bool hasChessboard = Calib3DCv2.CheckChessboard(chessboardImage, new Size(3, 3));
                                    bool foundChessboard = Calib3DCv2.FindChessboardCorners(chessboardImage, new Size(3, 3), chessboardCorners);
                                    bool foundCircles = Calib3DCv2.FindCirclesGrid(chessboardImage, new Size(3, 3), circlesCenters);
                                    OptimalNewCameraMatrixResult optimal = Calib3DCv2.GetOptimalNewCameraMatrix(cameraMatrix, distCoeffs, new Size(300, 300), 0.0);
                                    CalibrationMatrixValuesResult matrixValues = Calib3DCv2.CalibrationMatrixValues(cameraMatrix, new Size(300, 300), 36.0, 24.0);
                                    Point3f[][] calibrationObjectPoints = new[]
                                    {
                                        objectPoints,
                                        objectPoints
                                    };
                                    Point2f[][] calibrationImagePoints = new[]
                                    {
                                        imagePoints,
                                        new[]
                                        {
                                            new Point2f(102.0F, 98.0F),
                                            new Point2f(202.0F, 99.0F),
                                            new Point2f(201.0F, 201.0F),
                                            new Point2f(101.0F, 202.0F)
                                        }
                                    };
                                    Point2f[][] stereoImagePoints2 = new[]
                                    {
                                        new[]
                                        {
                                            new Point2f(96.0F, 100.0F),
                                            new Point2f(196.0F, 100.0F),
                                            new Point2f(196.0F, 200.0F),
                                            new Point2f(96.0F, 200.0F)
                                        },
                                        new[]
                                        {
                                            new Point2f(98.0F, 98.0F),
                                            new Point2f(198.0F, 99.0F),
                                            new Point2f(197.0F, 201.0F),
                                            new Point2f(97.0F, 202.0F)
                                        }
                                    };

                                    try
                                    {
                                        stereoLeft.SetTo(new Scalar(0));
                                        stereoRight.SetTo(new Scalar(0));
                                        ImgProcCv2.Rectangle(stereoLeft, new Rect(20, 20, 20, 20), new Scalar(255), -1);
                                        ImgProcCv2.Rectangle(stereoRight, new Rect(16, 20, 20, 20), new Scalar(255), -1);
                                        stereo.NumDisparities = 16;
                                        stereo.BlockSize = 9;
                                        stereo.Compute(stereoLeft, stereoRight, disparity);
                                        CalibrationResult fullCalibration = Calib3DCv2.CalibrateCamera(calibrationObjectPoints, calibrationImagePoints, new Size(300, 300));
                                        StereoCalibrationResult stereoCalibration = Calib3DCv2.StereoCalibrate(calibrationObjectPoints, calibrationImagePoints, stereoImagePoints2, new Size(300, 300));

                                        try
                                        {
                                            calib3dSummary = "Calib3D SolvePnP=" + solved
                                                + ", generic=" + generic.SolutionCount
                                                + ", rvec=" + rvec.Rows + "x" + rvec.Cols
                                                + ", rotation=" + calibRotation.Rows + "x" + calibRotation.Cols
                                                + ", projected=" + projected.Rows + "x" + projected.Cols
                                                + ", homography=" + homography.Rows + "x" + homography.Cols
                                                + ", chessboard=" + hasChessboard + "/" + foundChessboard
                                                + ", circles=" + foundCircles
                                                + ", optimal=" + optimal.CameraMatrix.Rows + "x" + optimal.CameraMatrix.Cols
                                                + ", fovX=" + matrixValues.FovX
                                                + ", calibrationRvecs=" + fullCalibration.Rvecs.Rows + "x" + fullCalibration.Rvecs.Cols
                                                + ", stereoR=" + stereoCalibration.R.Rows + "x" + stereoCalibration.R.Cols
                                                + ", disparity=" + disparity.Rows + "x" + disparity.Cols;
                                        }
                                        finally
                                        {
                                            fullCalibration.CameraMatrix.Dispose();
                                            fullCalibration.DistCoeffs.Dispose();
                                            fullCalibration.Rvecs.Dispose();
                                            fullCalibration.Tvecs.Dispose();
                                            stereoCalibration.CameraMatrix1.Dispose();
                                            stereoCalibration.DistCoeffs1.Dispose();
                                            stereoCalibration.CameraMatrix2.Dispose();
                                            stereoCalibration.DistCoeffs2.Dispose();
                                            stereoCalibration.R.Dispose();
                                            stereoCalibration.T.Dispose();
                                            stereoCalibration.E.Dispose();
                                            stereoCalibration.F.Dispose();
                                        }
                                    }
                                    finally
                                    {
                                        optimal.CameraMatrix.Dispose();
                                    }
                                }
                                finally
                                {
                                    generic.Rvecs.Dispose();
                                    generic.Tvecs.Dispose();
                                    generic.ReprojectionError?.Dispose();
                                }
                            }
                        }
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("calib3d", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        calib3dSummary = "Calib3D boundary: " + ex.Message;
                    }

                    string videoIoSummary;
                    try
                    {
                        int mjpg = VideoWriterObject.FourCC("MJPG");
                        VideoCaptureAPIs[] backends = VideoIORegistry.GetBackends();
                        VideoCaptureAPIs[] cameraBackends = VideoIORegistry.GetCameraBackends();
                        VideoCaptureAPIs[] streamBackends = VideoIORegistry.GetStreamBackends();
                        VideoCaptureAPIs[] writerBackends = VideoIORegistry.GetWriterBackends();
                        string firstBackendName = backends.Length == 0 ? string.Empty : VideoIORegistry.GetBackendName(backends[0]);
                        bool firstBackendBuiltIn = backends.Length > 0 && VideoIORegistry.IsBackendBuiltIn(backends[0]);
                        using (VideoCaptureObject capture = VideoCaptureObject.Create())
                        using (VideoWriterObject writer = VideoWriterObject.Create())
                        {
                            capture.ExceptionMode = false;
                            videoIoSummary = "VideoIO FourCC=" + mjpg
                                + ", captureOpened=" + capture.IsOpened
                                + ", exceptionMode=" + capture.ExceptionMode
                                + ", cameraOpen=skipped"
                                + ", writerOpened=" + writer.IsOpened
                                + ", registryBackends=" + backends.Length
                                + ", cameraBackends=" + cameraBackends.Length
                                + ", streamBackends=" + streamBackends.Length
                                + ", writerBackends=" + writerBackends.Length
                                + ", firstBackend=" + firstBackendName
                                + ", firstBuiltIn=" + firstBackendBuiltIn;
                        }
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("video", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        videoIoSummary = "VideoIO boundary: " + ex.Message;
                    }

                    string objDetectSummary;
                    try
                    {
                        using (QRCodeDetectorObject qrcode = QRCodeDetectorObject.Create())
                        using (BarcodeDetectorObject barcode = BarcodeDetectorObject.Create())
                        using (QRCodeDetectorArucoObject arucoQr = QRCodeDetectorArucoObject.Create())
                        using (QRCodeEncoderObject qrEncoder = QRCodeEncoderObject.Create(new QRCodeEncoderParams(0, QRCodeEncoderCorrectionLevel.L, QRCodeEncoderEncodeMode.Auto, 1)))
                        using (Mat qrPoints = new Mat())
                        using (Mat barcodePoints = new Mat())
                        using (Mat arucoPoints = new Mat())
                        using (Mat encodedQr = qrEncoder.Encode("OpenCvSharp"))
                        using (Mat faceInput = new Mat(32, 32, MatType.CV_8UC3))
                        using (Mat faceRows = new Mat())
                        {
                            qrcode.SetEpsX(0.2).SetEpsY(0.2).SetUseAlignmentMarkers(true);
                            barcode.DownsamplingThreshold = 512;
                            barcode.GradientThreshold = 64;
                            barcode.SetDetectorScales(new[] { 0.01F, 0.03F, 0.06F, 0.08F });
                            QRCodeDetectorArucoParams arucoParams = arucoQr.GetDetectorParameters();
                            arucoQr.SetDetectorParameters(arucoParams);

                            bool qrDetected = qrcode.Detect(faceInput, qrPoints);
                            bool barcodeDetected = barcode.Detect(faceInput, barcodePoints);
                            bool arucoDetected = arucoQr.Detect(faceInput, arucoPoints);
                            QRCodeEncoderECIEncodings encoding = QRCodeEncoderECIEncodings.Utf8;
                            try
                            {
                                encoding = qrcode.GetEncoding();
                            }
                            catch (OpenCvException)
                            {
                            }

                            FaceDetection[] detections = FaceDetectorYN.ToFaceDetections(faceRows);
                            objDetectSummary = "ObjDetect QR detected=" + qrDetected
                                + ", points=" + qrPoints.Rows + "x" + qrPoints.Cols
                                + ", encoding=" + encoding
                                + ", barcodeDetected=" + barcodeDetected
                                + ", barcodePoints=" + barcodePoints.Rows + "x" + barcodePoints.Cols
                                + ", arucoQrDetected=" + arucoDetected
                                + ", arucoParamsMinModule=" + arucoParams.MinModuleSizeInPyramid
                                + ", encodedQr=" + encodedQr.Rows + "x" + encodedQr.Cols
                                + ", DNN backend=" + DnnBackend.OpenCV
                                + ", faceRows=" + detections.Length;
                        }
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("objdetect", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        objDetectSummary = "ObjDetect boundary: " + ex.Message;
                    }

                    string arucoSummary;
                    try
                    {
                        using (ArucoDictionary dictionary = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
                        using (ArucoDetector detector = new ArucoDetector(dictionary))
                        using (Mat marker = dictionary.GenerateImageMarker(0, 48))
                        using (Mat boardImage = new Mat())
                        using (Mat charucoImage = new Mat())
                        using (ArucoGridBoard board = new ArucoGridBoard(new Size(2, 2), 0.04F, 0.01F, dictionary))
                        using (CharucoBoard charucoBoard = new CharucoBoard(new Size(4, 5), 0.04F, 0.02F, dictionary))
                        using (CharucoDetector charucoDetector = new CharucoDetector(charucoBoard))
                        using (CCheckerDetector checkerDetector = new CCheckerDetector())
                        using (CChecker checker = new CChecker())
                        {
                            ArucoDetectorParameters detectorParameters = detector.GetDetectorParameters();
                            detector.SetDetectorParameters(detectorParameters);
                            ArucoDetectionResult detection = detector.DetectMarkers(marker);
                            ArucoRefineResult refined = detector.RefineDetectedMarkers(
                                marker,
                                board,
                                detection.Corners,
                                detection.Ids,
                                detection.RejectedCandidates);
                            board.GenerateImage(new Size(96, 96), boardImage, 4, 1);
                            charucoBoard.GenerateImage(new Size(160, 120), charucoImage, 8, 1);
                            CharucoDetectionResult charucoDetection = charucoDetector.DetectBoard(marker);
                            DetectorParametersMCC mccParameters = new DetectorParametersMCC();
                            checkerDetector.SetDetectionParams(mccParameters);
                            checkerDetector.ColorChartType = ColorChart.Mcc24;
                            checker.Target = ColorChart.Mcc24;
                            checker.SetBox(new[]
                            {
                                new Point2f(0.0F, 0.0F),
                                new Point2f(1.0F, 0.0F),
                                new Point2f(1.0F, 1.0F),
                                new Point2f(0.0F, 1.0F)
                            });

                            arucoSummary = "Aruco marker=" + marker.Rows + "x" + marker.Cols
                                + ", board=" + boardImage.Rows + "x" + boardImage.Cols
                                + ", charucoBoard=" + charucoImage.Rows + "x" + charucoImage.Cols
                                + ", detected=" + detection.Count
                                + ", rejected=" + detection.RejectedCandidates.Length
                                + ", refined=" + refined.Count
                                + ", recovered=" + refined.RecoveredIndices.Length
                                + ", charucoCorners=" + charucoDetection.Count
                                + ", cornerMethod=" + detectorParameters.CornerRefinementMethod
                                + ", MCC minGroup=" + mccParameters.MinGroupSize
                                + ", MCC chart=" + checkerDetector.ColorChartType
                                + ", checkerBox=" + checker.GetBox().Length;
                        }
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("objdetect", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        arucoSummary = "Aruco boundary: " + ex.Message;
                    }

                    string photoSummary;
                    try
                    {
                        using (Mat photoSrc = new Mat(8, 8, MatType.CV_8UC1, new Scalar(32)))
                        using (Mat photoMask = new Mat(8, 8, MatType.CV_8UC1, new Scalar(0)))
                        using (Mat colorPhoto = new Mat(8, 8, MatType.CV_8UC3, new Scalar(32, 64, 96)))
                        using (Mat colorMask = new Mat(8, 8, MatType.CV_8UC1, new Scalar(255)))
                        using (Mat grayscale = new Mat())
                        using (Mat colorBoost = new Mat())
                        using (Mat cloned = new Mat())
                        using (Mat changed = new Mat())
                        using (Mat flattened = new Mat())
                        using (Mat smoothed = new Mat())
                        using (Mat sketchGray = new Mat())
                        using (Mat sketchColor = new Mat())
                        using (Mat photoDst = new Mat())
                        using (Mat denoised = new Mat())
                        using (Mat multiFrame1 = new Mat(8, 8, MatType.CV_8UC1, new Scalar(30)))
                        using (Mat multiFrame2 = new Mat(8, 8, MatType.CV_8UC1, new Scalar(32)))
                        using (Mat multiFrame3 = new Mat(8, 8, MatType.CV_8UC1, new Scalar(31)))
                        using (Mat multiDenoised = new Mat())
                        using (Mat exposure0 = new Mat(16, 16, MatType.CV_8UC3, new Scalar(32, 40, 48)))
                        using (Mat exposure1 = new Mat(16, 16, MatType.CV_8UC3, new Scalar(96, 104, 112)))
                        using (Mat exposure2 = new Mat(16, 16, MatType.CV_8UC3, new Scalar(192, 200, 208)))
                        using (Mat exposureTimes = new Mat(3, 1, MatType.CV_32FC1, new Scalar(0.5)))
                        using (Mat aligned0 = new Mat())
                        using (Mat aligned1 = new Mat())
                        using (Mat aligned2 = new Mat())
                        using (Mat cameraResponse = new Mat())
                        using (Mat fusedExposure = new Mat())
                        using (TonemapDrago tonemap = PhotoCv2.CreateTonemapDrago())
                        using (AlignMTB alignMtb = PhotoCv2.CreateAlignMTB(cut: false))
                        using (CalibrateDebevec calibrateDebevec = PhotoCv2.CreateCalibrateDebevec(samples: 16))
                        using (MergeMertens mergeMertens = PhotoCv2.CreateMergeMertens())
                        {
                            PhotoCv2.Inpaint(photoSrc, photoMask, photoDst, 3.0, InpaintMethod.Telea);
                            PhotoCv2.FastNlMeansDenoising(photoSrc, denoised);
                            PhotoCv2.FastNlMeansDenoisingMulti(new[] { multiFrame1, multiFrame2, multiFrame3 }, multiDenoised, 1, 3);
                            PhotoCv2.Decolor(colorPhoto, grayscale, colorBoost);
                            PhotoCv2.SeamlessClone(colorPhoto, colorPhoto, colorMask, new Point(4, 4), cloned, SeamlessCloneFlags.NormalClone);
                            PhotoCv2.ColorChange(colorPhoto, colorMask, changed, 1.0F, 1.0F, 1.0F);
                            PhotoCv2.TextureFlattening(colorPhoto, colorMask, flattened);
                            PhotoCv2.EdgePreservingFilter(colorPhoto, smoothed, EdgePreservingFilterFlags.RecursiveFilter);
                            PhotoCv2.PencilSketch(colorPhoto, sketchGray, sketchColor);
                            tonemap.Gamma = 1.0F;
                            Mat[] exposures = { exposure0, exposure1, exposure2 };
                            alignMtb.Process(exposures, new[] { aligned0, aligned1, aligned2 });
                            calibrateDebevec.Process(exposures, cameraResponse, exposureTimes);
                            mergeMertens.Process(exposures, fusedExposure);

                            photoSummary = "Photo inpaint=" + photoDst.Rows + "x" + photoDst.Cols
                                + ", denoise=" + denoised.Rows + "x" + denoised.Cols
                                + ", multiDenoise=" + multiDenoised.Rows + "x" + multiDenoised.Cols
                                + ", decolor=" + grayscale.Rows + "x" + grayscale.Cols
                                + ", seamless=" + cloned.Rows + "x" + cloned.Cols
                                + ", edge=" + smoothed.Rows + "x" + smoothed.Cols
                                + ", sketch=" + sketchGray.Rows + "x" + sketchGray.Cols
                                + ", tonemapGamma=" + tonemap.Gamma
                                + ", aligned=" + 3 + "x" + aligned0.Rows + "x" + aligned0.Cols
                                + ", response=" + cameraResponse.Rows + "x" + cameraResponse.Cols
                                + ", fused=" + fusedExposure.Rows + "x" + fusedExposure.Cols;
                        }
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("photo", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        photoSummary = "Photo boundary: " + ex.Message;
                    }

                    string videoSummary;
                    try
                    {
                        using (Mat prevFrame = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0)))
                        using (Mat nextFrame = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0)))
                        using (Mat farnebackFlow = new Mat())
                        using (Mat tinyFlow = new Mat(2, 2, MatType.CV_32FC2, new Scalar(1.0, 0.5, 0.0, 0.0)))
                        using (Mat probability = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0)))
                        using (Mat knownForeground = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0)))
                        using (Mat mog2Mask = new Mat())
                        using (Mat mog2KnownMask = new Mat())
                        using (Mat knnMask = new Mat())
                        using (BackgroundSubtractorMOG2Object mog2 = BackgroundSubtractorMOG2Object.Create(history: 16, varThreshold: 12.0, detectShadows: true))
                        using (BackgroundSubtractorKNNObject knn = BackgroundSubtractorKNNObject.Create(history: 16, dist2Threshold: 300.0, detectShadows: true))
                        using (VideoKalmanFilterObject kalman = new VideoKalmanFilterObject(2, 1))
                        using (Mat measurement = new Mat(1, 1, MatType.CV_32FC1, new Scalar(1.0)))
                        {
                            ImgProcCv2.Rectangle(prevFrame, new Rect(8, 8, 8, 8), new Scalar(255), -1);
                            ImgProcCv2.Rectangle(nextFrame, new Rect(10, 8, 8, 8), new Scalar(255), -1);
                            ImgProcCv2.Rectangle(probability, new Rect(8, 8, 10, 10), new Scalar(255), -1);
                            ImgProcCv2.Rectangle(knownForeground, new Rect(10, 8, 8, 8), new Scalar(255), -1);

                            Point2f[] tracked = VideoCv2.CalcOpticalFlowPyrLK(
                                prevFrame,
                                nextFrame,
                                new[] { new Point2f(12.0F, 12.0F) },
                                out byte[] lkStatus,
                                out float[] lkError);
                            VideoCv2.CalcOpticalFlowFarneback(prevFrame, nextFrame, farnebackFlow, 0.5, 1, 5, 1, 5, 1.1);
                            string flowIoSummary = WriteReadFlowSummary(tinyFlow);
                            mog2.Apply(prevFrame, mog2Mask);
                            mog2.Apply(nextFrame, knownForeground, mog2KnownMask);
                            knn.Apply(nextFrame, knnMask);
                            OpticalFlowPyramidResult pyramid = VideoCv2.BuildOpticalFlowPyramid(prevFrame, new Size(5, 5), 1);
                            try
                            {
                                MeanShiftResult meanShift = VideoCv2.MeanShift(probability, new Rect(6, 6, 12, 12), TermCriteria.ByCountAndEpsilon(5, 1.0));
                                CamShiftResult camShift = VideoCv2.CamShift(probability, new Rect(6, 6, 12, 12), TermCriteria.ByCountAndEpsilon(5, 1.0));
                                using (Mat prediction = kalman.Predict())
                                using (Mat corrected = kalman.Correct(measurement))
                                {
                                    videoSummary = "Video LK status=" + string.Join(",", lkStatus)
                                        + ", tracked=" + string.Join(";", tracked)
                                        + ", err=" + string.Join(",", lkError)
                                        + ", Farneback=" + farnebackFlow.Rows + "x" + farnebackFlow.Cols + " type=" + farnebackFlow.Type
                                        + ", " + flowIoSummary
                                        + ", MOG2=" + mog2Mask.Rows + "x" + mog2Mask.Cols + "/history=" + mog2.History + "/shadow=" + mog2.DetectShadows
                                        + ", MOG2Known=" + mog2KnownMask.Rows + "x" + mog2KnownMask.Cols
                                        + ", KNN=" + knnMask.Rows + "x" + knnMask.Cols + "/samples=" + knn.NSamples
                                        + ", pyramidLevels=" + pyramid.LevelCount
                                        + ", pyramidMats=" + pyramid.Pyramid.Length
                                        + ", meanShiftIterations=" + meanShift.Iterations
                                        + ", camShiftAngle=" + camShift.Box.Angle
                                        + ", predict=" + prediction.Rows + "x" + prediction.Cols
                                        + ", correct=" + corrected.Rows + "x" + corrected.Cols;
                                }
                            }
                            finally
                            {
                                for (int i = 0; i < pyramid.Pyramid.Length; i++)
                                {
                                    pyramid.Pyramid[i].Dispose();
                                }
                            }
                        }
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("video", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        videoSummary = "Video boundary: " + ex.Message;
                    }

                    string dnnSummary;
                    try
                    {
                        using (Mat dnnInput = new Mat(16, 16, MatType.CV_8UC3, new Scalar(32, 64, 96)))
                        using (Mat blob = DnnCv2.BlobFromImage(dnnInput, size: new Size(16, 16), mean: new Scalar(1.0, 2.0, 3.0), swapRB: true))
                        using (DnnNetObject emptyNet = DnnNetObject.CreateEmpty())
                        {
                            Mat[] images = DnnCv2.ImagesFromBlob(blob);
                            try
                            {
                                string modelPath = GetEnvironmentVariable(DnnModelVariable, CompatibilityDnnModelAlias) ?? string.Empty;
                                string configPath = GetEnvironmentVariable(DnnConfigVariable, CompatibilityDnnConfigAlias) ?? string.Empty;
                                string framework = GetEnvironmentVariable(DnnFrameworkVariable, CompatibilityDnnFrameworkAlias) ?? string.Empty;
                                string modelSummary = string.IsNullOrWhiteSpace(modelPath)
                                    ? "modelForward=skipped"
                                    : RunDnnForwardSummary(modelPath, configPath, framework, blob);
                                string emptyMetadata = ReadEmptyDnnMetadataSummary(emptyNet);
                                dnnSummary = "DNN blob=" + blob.Rows + "x" + blob.Cols
                                    + ", imagesFromBlob=" + images.Length
                                    + ", emptyNet=" + emptyNet.Empty
                                    + ", " + emptyMetadata
                                    + ", backend=" + OpenCvSharp.Dnn.DnnBackend.OpenCV
                                    + ", target=" + OpenCvSharp.Dnn.DnnTarget.Cpu
                                    + ", " + modelSummary;
                            }
                            finally
                            {
                                for (int i = 0; i < images.Length; i++)
                                {
                                    images[i].Dispose();
                                }
                            }
                        }
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("dnn", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        dnnSummary = "DNN boundary: " + ex.Message;
                    }

                    string highGuiSummary;
                    try
                    {
                        string highGuiBackend = HighGuiCv2.GetCurrentUIFramework();
                        int highGuiWheelDelta = HighGuiCv2.GetMouseWheelDelta((OpenCvSharp.HighGui.MouseEventFlags)unchecked((int)0x00780000));
                        bool smokeEnabled = IsEnvironmentFlagEnabled(HighGuiSmokeVariable, CompatibilityHighGuiSmokeAlias);
                        if (smokeEnabled)
                        {
                            const string windowName = "OpenCvSharp.HighGui.Smoke";
                            using (Mat preview = new Mat(24, 24, MatType.CV_8UC3, new Scalar(0, 128, 255)))
                            {
                                HighGuiCv2.NamedWindow(windowName, OpenCvSharp.HighGui.WindowFlags.AutoSize);
                                HighGuiCv2.SetWindowTitle(windowName, "OpenCvSharp HighGui Smoke");
                                HighGuiCv2.ImShow(windowName, preview);
                                using (OpenCvSharp.HighGui.HighGuiTrackbar trackbar = HighGuiCv2.CreateTrackbar("value", windowName, 0, 10, _ => { }))
                                {
                                    HighGuiCv2.SetTrackbarMin("value", windowName, 0);
                                    HighGuiCv2.SetTrackbarMax("value", windowName, 10);
                                    HighGuiCv2.SetTrackbarPos("value", windowName, 3);
                                    int position = HighGuiCv2.GetTrackbarPos("value", windowName);
                                    HighGuiCv2.SetWindowProperty(windowName, OpenCvSharp.HighGui.WindowPropertyFlags.Topmost, 0.0);
                                    Rect imageRect = HighGuiCv2.GetWindowImageRect(windowName);
                                    HighGuiCv2.SetMouseCallback(windowName, null);
                                    int key = HighGuiCv2.WaitKey(1);
                                    highGuiSummary = "HighGui smoke key=" + key
                                        + ", backend=" + (highGuiBackend.Length == 0 ? "none" : highGuiBackend)
                                        + ", wheelDelta=" + highGuiWheelDelta
                                        + ", trackbar=" + position
                                        + ", imageRect=" + imageRect.Width + "x" + imageRect.Height
                                        + ", trackbarDisposed=" + trackbar.IsDisposed;
                                }
                                HighGuiCv2.DestroyWindow(windowName);
                            }
                        }
                        else
                        {
                            highGuiSummary = "HighGui backend=" + (highGuiBackend.Length == 0 ? "none" : highGuiBackend)
                                + ", wheelDelta=" + highGuiWheelDelta
                                + ", smoke=skipped, enum=" + OpenCvSharp.HighGui.WindowFlags.AutoSize;
                        }
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("highgui", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        highGuiSummary = "HighGui boundary: " + ex.Message;
                    }

                    string xobjDetectSummary;
                    try
                    {
                        using (CascadeClassifierObject cascade = new CascadeClassifierObject())
                        using (HOGDescriptorObject hog = new HOGDescriptorObject())
                        using (Mat hogInput = new Mat(128, 64, MatType.CV_8UC1))
                        {
                            float[] peopleDetector = HOGDescriptorObject.GetDefaultPeopleDetector();
                            if (peopleDetector.Length > 0)
                            {
                                hog.SetSVMDetector(peopleDetector);
                            }

                            bool cascadeEmpty = cascade.Empty;
                            bool hogDetectorSize = hog.CheckDetectorSize();
                            HOGDetectionResult hogResult = hog.DetectMultiScale(hogInput);
                            xobjDetectSummary = "XObjDetect cascadeEmpty=" + cascadeEmpty
                                + ", hogDescriptorSize=" + hog.GetDescriptorSize()
                                + ", hogDetectorSize=" + hogDetectorSize
                                + ", hogDetections=" + hogResult.Rectangles.Length;
                        }
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("xobjdetect", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        xobjDetectSummary = "XObjDetect boundary: " + ex.Message;
                    }

                    string ptCloudSummary;
                    try
                    {
                        ptCloudSummary = RunPtCloudSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("ptcloud", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ptCloudSummary = "PtCloud boundary: " + ex.Message;
                    }

                    string qualitySummary;
                    try
                    {
                        qualitySummary = RunQualitySummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("quality", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        qualitySummary = "Quality boundary: " + ex.Message;
                    }

                    string xphotoSummary;
                    try
                    {
                        xphotoSummary = RunXPhotoSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("xphoto", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        xphotoSummary = "XPhoto boundary: " + ex.Message;
                    }

                    string ximgprocSummary;
                    string ximgprocSecondBatchSummary;
                    string ximgprocRemainingSummary;
                    try
                    {
                        ximgprocSummary = RunXImgProcSummary();
                        ximgprocSecondBatchSummary = RunXImgProcSecondBatchSummary();
                        ximgprocRemainingSummary = RunXImgProcRemainingUtilitiesSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("ximgproc", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ximgprocSummary = "XImgProc boundary: " + ex.Message;
                        ximgprocSecondBatchSummary = "XImgProc second batch boundary: " + ex.Message;
                        ximgprocRemainingSummary = "XImgProc remaining utilities boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        ximgprocSummary = "XImgProc boundary: stale native runtime: " + ex.Message;
                        ximgprocSecondBatchSummary = "XImgProc second batch boundary: stale native runtime: " + ex.Message;
                        ximgprocRemainingSummary = "XImgProc remaining utilities boundary: stale native runtime: " + ex.Message;
                    }

                    string mlSummary;
                    try
                    {
                        mlSummary = RunMLSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("ml", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        mlSummary = "ML boundary: " + ex.Message;
                    }

                    string imgHashSummary;
                    try
                    {
                        imgHashSummary = RunImgHashSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("img_hash", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        imgHashSummary = "ImgHash boundary: " + ex.Message;
                    }

                    string plotSummary;
                    try
                    {
                        plotSummary = RunPlotSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("plot", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        plotSummary = "Plot boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        plotSummary = "Plot boundary: stale native runtime: " + ex.Message;
                    }

                    string shapeSummary;
                    try
                    {
                        shapeSummary = RunShapeSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("shape", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        shapeSummary = "Shape boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        shapeSummary = "Shape boundary: stale native runtime: " + ex.Message;
                    }

                    string lineDescriptorSummary;
                    try
                    {
                        lineDescriptorSummary = RunLineDescriptorSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("line_descriptor", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        lineDescriptorSummary = "LineDescriptor boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        lineDescriptorSummary = "LineDescriptor boundary: stale native runtime: " + ex.Message;
                    }

                    string phaseUnwrappingSummary;
                    try
                    {
                        phaseUnwrappingSummary = RunPhaseUnwrappingSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("phase_unwrapping", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        phaseUnwrappingSummary = "PhaseUnwrapping boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        phaseUnwrappingSummary = "PhaseUnwrapping boundary: stale native runtime: " + ex.Message;
                    }

                    string structuredLightSummary;
                    try
                    {
                        structuredLightSummary = RunStructuredLightSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("structured_light", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        structuredLightSummary = "StructuredLight boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        structuredLightSummary = "StructuredLight boundary: stale native runtime: " + ex.Message;
                    }

                    string intensityTransformSummary;
                    try
                    {
                        intensityTransformSummary = RunIntensityTransformSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("intensity_transform", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        intensityTransformSummary = "IntensityTransform boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        intensityTransformSummary = "IntensityTransform boundary: stale native runtime: " + ex.Message;
                    }

                    string fuzzySummary;
                    try
                    {
                        fuzzySummary = RunFuzzySummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("fuzzy", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        fuzzySummary = "Fuzzy boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        fuzzySummary = "Fuzzy boundary: stale native runtime: " + ex.Message;
                    }

                    string hfsSummary;
                    try
                    {
                        hfsSummary = RunHfsSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("hfs", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        hfsSummary = "HFS boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        hfsSummary = "HFS boundary: stale native runtime: " + ex.Message;
                    }

                    string regSummary;
                    try
                    {
                        regSummary = RunRegSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("reg", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        regSummary = "Reg boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        regSummary = "Reg boundary: stale native runtime: " + ex.Message;
                    }

                    string surfaceMatchingSummary;
                    try
                    {
                        surfaceMatchingSummary = RunSurfaceMatchingSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("surface", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("ppf", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        surfaceMatchingSummary = "SurfaceMatching boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        surfaceMatchingSummary = "SurfaceMatching boundary: stale native runtime: " + ex.Message;
                    }

                    string rapidSummary;
                    try
                    {
                        rapidSummary = RunRapidSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("rapid", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        rapidSummary = "Rapid boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        rapidSummary = "Rapid boundary: stale native runtime: " + ex.Message;
                    }

                    string bioInspiredSummary;
                    if (IsUnstableNativeSmokeEnabled())
                    {
                        try
                        {
                            bioInspiredSummary = RunBioInspiredSummary();
                        }
                        catch (OpenCvException ex) when (ex.Message.IndexOf("bioinspired", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            bioInspiredSummary = "BioInspired boundary: " + ex.Message;
                        }
                        catch (EntryPointNotFoundException ex)
                        {
                            bioInspiredSummary = "BioInspired boundary: stale native runtime: " + ex.Message;
                        }
                    }
                    else
                    {
                        bioInspiredSummary = GetBioInspiredUnstableSkipSummary();
                    }

                    string xstereoSummary;
                    try
                    {
                        xstereoSummary = RunXStereoSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("xstereo", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        xstereoSummary = "XStereo boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        xstereoSummary = "XStereo boundary: stale native runtime: " + ex.Message;
                    }

                    string optFlowSummary;
                    try
                    {
                        optFlowSummary = RunOptFlowSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("optflow", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("ximgproc", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        optFlowSummary = "OptFlow boundary: " + ex.Message;
                    }

                    string bgSegmSummary;
                    try
                    {
                        bgSegmSummary = RunBgSegmSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("bgsegm", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        bgSegmSummary = "BgSegm boundary: " + ex.Message;
                    }

                    string faceSummary;
                    try
                    {
                        faceSummary = RunFaceSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("face", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        faceSummary = "Face boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        faceSummary = "Face boundary: stale native runtime: " + ex.Message;
                    }

                    string saliencySummary;
                    try
                    {
                        saliencySummary = RunSaliencySummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("saliency", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        saliencySummary = "Saliency boundary: " + ex.Message;
                    }
                    catch (EntryPointNotFoundException ex)
                    {
                        saliencySummary = "Saliency boundary: stale native runtime: " + ex.Message;
                    }

                    string stitchingSummary;
                    try
                    {
                        stitchingSummary = RunStitchingSummary();
                    }
                    catch (OpenCvException ex) when (ex.Message.IndexOf("stitch", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        stitchingSummary = "Stitching boundary: " + ex.Message;
                    }

                    using (LineSegmentDetector detector = ImgProcCv2.CreateLineSegmentDetector())
                    {
                        detector.Detect(houghBinary, lsdLines);
                        LineSegment[] lsdSegments = detector.Detect(houghBinary);
                        detector.DrawSegments(lsdDrawing, lsdLines);
                        if (lsdSegments.Length > 0)
                        {
                            detector.DrawSegments(lsdDrawing, lsdSegments);
                        }

                        int lsdMismatch = detector.CompareSegments(houghBinary.Size, lsdSegments, lsdSegments);
                        Console.WriteLine("LSD disposed: " + detector.IsDisposed + ", mat rows=" + lsdLines.Rows + ", segments=" + lsdSegments.Length + ", mismatch=" + lsdMismatch);
                    }

                    Console.WriteLine("Analysis equalized type: " + equalized.Type);
                    Console.WriteLine("Analysis adaptive pixels: " + string.Join(",", adaptive.ToBytes()));
                    Console.WriteLine("Integral shape: " + integral.Rows + "x" + integral.Cols);
                    Console.WriteLine("Distance transform type: " + distance.Type + ", labels type=" + distanceLabels.Type);
                    Console.WriteLine("Connected components: " + labelCount + ", stats=" + connectedStats.Rows + "x" + connectedStats.Cols);
                    Console.WriteLine("Contours: " + foundContours.Length + ", hierarchy=" + foundHierarchy.Length);
                    Console.WriteLine("Moment M00: " + imageMoments.M00 + ", Hu0=" + huMoments[0]);
                    Console.WriteLine("Corner response: " + cornerResponse.Rows + "x" + cornerResponse.Cols + ", type=" + cornerResponse.Type);
                    Console.WriteLine("CLAHE result: " + claheResult.Rows + "x" + claheResult.Cols + ", type=" + claheResult.Type);
                    Console.WriteLine("Hist bins: " + hist.ValueCount + ", CLAHE hist bins=" + claheHist.ValueCount + ", correlation=" + histogramCorrelation);
                    Console.WriteLine("BackProject: " + backProject.Rows + "x" + backProject.Cols + ", type=" + backProject.Type);
                    Console.WriteLine("Hough lines: " + houghLines.Length + ", segments=" + houghSegments.Length + ", circles=" + houghCircles.Length + ", point-set lines=" + pointSetLines.Length);
#if NETCOREAPP3_1_OR_GREATER
                    Console.WriteLine("Hough point-set lines from span: " + pointSetLinesFromSpan.Length);
#endif
                    Console.WriteLine("Refined corners: " + string.Join(";", refinedCorners));
                    Console.WriteLine(goodFeaturesSummary);
                    Console.WriteLine(features2dSummary);
                    Console.WriteLine(calib3dSummary);
                    Console.WriteLine(videoIoSummary);
                    Console.WriteLine(objDetectSummary);
                    Console.WriteLine(arucoSummary);
                    Console.WriteLine(photoSummary);
                    Console.WriteLine(videoSummary);
                    Console.WriteLine(dnnSummary);
                    Console.WriteLine(highGuiSummary);
                    Console.WriteLine(xobjDetectSummary);
                    Console.WriteLine(ptCloudSummary);
                    Console.WriteLine(qualitySummary);
                    Console.WriteLine(xphotoSummary);
                    Console.WriteLine(ximgprocSummary);
                    Console.WriteLine(ximgprocSecondBatchSummary);
                    Console.WriteLine(ximgprocRemainingSummary);
                    Console.WriteLine(mlSummary);
                    Console.WriteLine(imgHashSummary);
                    Console.WriteLine(plotSummary);
                    Console.WriteLine(shapeSummary);
                    Console.WriteLine(lineDescriptorSummary);
                    Console.WriteLine(phaseUnwrappingSummary);
                    Console.WriteLine(structuredLightSummary);
                    Console.WriteLine(intensityTransformSummary);
                    Console.WriteLine(fuzzySummary);
                    Console.WriteLine(hfsSummary);
                    Console.WriteLine(regSummary);
                    Console.WriteLine(surfaceMatchingSummary);
                    Console.WriteLine(rapidSummary);
                    Console.WriteLine(bioInspiredSummary);
                    Console.WriteLine(xstereoSummary);
                    Console.WriteLine(optFlowSummary);
                    Console.WriteLine(bgSegmSummary);
                    Console.WriteLine(faceSummary);
                    Console.WriteLine(saliencySummary);
                    Console.WriteLine(stitchingSummary);
                    Console.WriteLine(GetAlphaMatSummaryLast());
                }
            }
            catch (OpenCvException ex)
            {
                Console.WriteLine("Native runtime is not available or OpenCV returned an error: " + ex.Message);
            }
        }

        private static string RunImgProcUpstreamParitySummary()
        {
            using (Mat gray = new Mat(16, 16, MatType.CV_8UC1))
            using (Mat second = new Mat(16, 16, MatType.CV_8UC1))
            using (Mat weights1 = new Mat(16, 16, MatType.CV_32FC1))
            using (Mat weights2 = new Mat(16, 16, MatType.CV_32FC1))
            using (Mat mask = new Mat(16, 16, MatType.CV_8UC1))
            using (Mat thresholded = new Mat(16, 16, MatType.CV_8UC1))
            using (Mat colored = new Mat())
            using (Mat blended = new Mat())
            using (Mat blurred = new Mat())
            using (Mat dx = new Mat())
            using (Mat dy = new Mat())
            using (Mat template = new Mat(12, 12, MatType.CV_8UC1))
            using (Mat image = new Mat(32, 32, MatType.CV_8UC1))
            using (Mat positions = new Mat())
            using (GeneralizedHoughBallard detector = ImgProcCv2.CreateGeneralizedHoughBallard())
            {
                gray.SetTo(new Scalar(32));
                second.SetTo(new Scalar(192));
                weights1.SetTo(new Scalar(0.25));
                weights2.SetTo(new Scalar(0.75));
                mask.SetTo(new Scalar(255));
                thresholded.SetTo(new Scalar(0));
                ImgProcCv2.ApplyColorMap(gray, colored, ColormapTypes.Turbo);
                ImgProcCv2.BlendLinear(gray, second, weights1, weights2, blended);
                ImgProcCv2.StackBlur(gray, blurred, new Size(3, 3));
                ImgProcCv2.SpatialGradient(gray, dx, dy);
                double threshold = ImgProcCv2.ThresholdWithMask(gray, thresholded, mask, 64, 255, ThresholdTypes.Binary);
                ImgProcCv2.DrawMarker(colored, new Point(8, 8), new Scalar(0, 255, 0), MarkerTypes.Star, 7);
                ImgProcCv2.FillConvexPoly(
                    colored,
                    new[] { new Point(2, 12), new Point(8, 2), new Point(14, 12) },
                    new Scalar(255, 0, 0));
                double fontScale = ImgProcCv2.GetFontScaleFromHeight(HersheyFonts.HersheySimplex, 18);

                template.SetTo(new Scalar(0));
                image.SetTo(new Scalar(0));
                ImgProcCv2.Rectangle(template, new Rect(2, 2, 8, 8), new Scalar(255), 1);
                ImgProcCv2.Rectangle(image, new Rect(10, 10, 8, 8), new Scalar(255), 1);
                detector.CannyLowThreshold = 25;
                detector.CannyHighThreshold = 75;
                detector.Levels = 90;
                detector.VotesThreshold = 1;
                detector.SetTemplate(template);
                detector.Detect(image, positions);

                return "ImgProc upstream families: colormap=" + colored.Type
                    + ", blend=" + blended.Type
                    + ", gradient=" + dx.Type + "/" + dy.Type
                    + ", threshold=" + threshold
                    + ", fontScale=" + fontScale
                    + ", generalizedHoughRows=" + positions.Rows;
            }
        }

        private static string RunFeaturesAnnIndexSummary()
        {
            try
            {
                using (ANNIndex index = ANNIndex.Create(2))
                using (Mat features = new Mat(4, 2, MatType.CV_32FC1))
                using (Mat query = new Mat(2, 2, MatType.CV_32FC1))
                using (Mat indices = new Mat())
                using (Mat distances = new Mat())
                {
                    features.CopyFrom(new float[]
                    {
                        0.0F, 0.0F,
                        10.0F, 10.0F,
                        2.0F, 2.0F,
                        -2.0F, -2.0F
                    });
                    query.CopyFrom(new float[] { 0.1F, 0.1F, 9.5F, 10.5F });
                    index.SetSeed(1234);
                    index.AddItems(features);
                    index.Build(2);
                    index.KnnSearch(query, indices, distances, 1);
                    return "Features ANNIndex: items=" + index.ItemNumber
                        + ", trees=" + index.TreeNumber
                        + ", indices=" + indices.Rows + "x" + indices.Cols
                        + ", distancesType=" + distances.Type
                        + ", nearest=" + string.Join(",", indices.ToArray<int>());
                }
            }
            catch (OpenCvException ex) when (ex.Message.IndexOf("features2d", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Features ANNIndex boundary: " + ex.Message;
            }
        }

        private static string RunObjDetectStructuredSummary()
        {
            try
            {
                using (ArucoDictionary first = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict4X4_50))
                using (ArucoDictionary second = ArucoDictionary.GetPredefinedDictionary(PredefinedDictionaryType.Dict5X5_50))
                using (ArucoDictionary extended = ArucoDictionary.Extend(55, 4, first, 12345))
                using (Mat extendedBytes = extended.BytesList)
                using (ArucoBoard board = new ArucoBoard(
                    new[]
                    {
                        new[]
                        {
                            new Point3f(0, 0, 0),
                            new Point3f(1, 0, 0),
                            new Point3f(1, 1, 0),
                            new Point3f(0, 1, 0)
                        }
                    },
                    first,
                    new[] { 7 }))
                using (ArucoDetector multiDetector = ArucoDetector.Create(new[] { first, second }))
                using (Mat marker = first.GenerateImageMarker(3, 128))
                using (Mat borderedMarker = CoreCv2.CopyMakeBorder(marker, 32, 32, 32, 32, BorderTypes.Constant, new Scalar(255)))
                using (Mat boardImage = board.GenerateImage(new Size(96, 96), 4))
                using (QRCodeEncoderObject encoder = QRCodeEncoderObject.Create())
                using (QRCodeDetectorObject qrDetector = QRCodeDetectorObject.Create())
                using (Mat qrCode = encoder.Encode("objdetect-structured-sample"))
                using (Mat scaledQrCode = new Mat())
                using (CharucoBoard charucoBoard = new CharucoBoard(new Size(5, 7), 80, 40, first))
                using (Mat chessboard = charucoBoard.GenerateImage(new Size(600, 840), 40))
                using (Mat chessboardCorners = new Mat())
                using (Mat chessboardMeta = new Mat())
                using (Mat chessboardSharpness = new Mat())
                {
                    ArucoMultiDictionaryDetectionResult detection = multiDetector.DetectMarkersMultiDictionary(borderedMarker);
                    ImgProcCv2.Resize(qrCode, scaledQrCode, new Size(qrCode.Cols * 8, qrCode.Rows * 8), interpolation: InterpolationFlags.Nearest);
                    using (Mat borderedQrCode = CoreCv2.CopyMakeBorder(scaledQrCode, 32, 32, 32, 32, BorderTypes.Constant, new Scalar(255)))
                    {
                        byte[] decoded = qrDetector.DetectAndDecodeBytes(borderedQrCode);
                        bool found = Calib3DCv2.FindChessboardCornersSB(chessboard, new Size(4, 6), chessboardCorners, chessboardMeta);
                        Scalar sharpness = Calib3DCv2.EstimateChessboardSharpness(
                            chessboard,
                            new Size(4, 6),
                            chessboardCorners,
                            sharpness: chessboardSharpness);
                        bool refined = Calib3DCv2.Find4QuadCornerSubpix(chessboard, chessboardCorners, new Size(5, 5));

                        return "ObjDetect structured: extended=" + extendedBytes.Rows
                            + ", board=" + board.ObjectPoints.Length + "/" + boardImage.Rows + "x" + boardImage.Cols
                            + ", multi=" + detection.Count + "/" + string.Join(",", detection.DictionaryIndices)
                            + ", qrBytes=" + decoded.Length
                            + ", chessboard=" + found + "/" + refined
                            + ", meta=" + chessboardMeta.Rows + "x" + chessboardMeta.Cols
                            + ", sharpness=" + sharpness.V0 + "/" + chessboardSharpness.Rows;
                    }
                }
            }
            catch (OpenCvException ex) when (
                ex.Message.IndexOf("objdetect", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ex.Message.IndexOf("calib3d", StringComparison.OrdinalIgnoreCase) >= 0 ||
                ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "ObjDetect structured boundary: " + ex.Message;
            }
        }

        private static string RunImgCodecsUpstreamParitySummary()
        {
            string root = Path.Combine(Path.GetTempPath(), "opencv-csharp-imgcodecs-" + Guid.NewGuid().ToString("N"));
            string tiffPath = Path.Combine(root, "多页.tiff");
            string jpegPath = Path.Combine(root, "元数据.jpeg");
            string gifPath = Path.Combine(root, "动画.gif");
            Directory.CreateDirectory(root);

            using (Mat first = new Mat(8, 8, MatType.CV_8UC3, new Scalar(20, 30, 40)))
            using (Mat second = new Mat(8, 8, MatType.CV_8UC3, new Scalar(120, 130, 140)))
            using (Mat exif = new Mat(1, CreateSampleExifMetadata().Length, MatType.CV_8UC1))
            {
                Mat[] filePages = Array.Empty<Mat>();
                Mat[] memoryPages = Array.Empty<Mat>();
                try
                {
                    byte[] exifBytes = CreateSampleExifMetadata();
                    exif.CopyFrom(exifBytes);
                    var pages = new[] { first, second };
                    bool multiWritten = ImgCodecsCv2.ImWriteMulti(tiffPath, pages);
                    bool multiRead = ImgCodecsCv2.ImReadMulti(tiffPath, out filePages);
                    byte[] multiBuffer = ImgCodecsCv2.ImEncodeMulti(".tiff", pages);
                    bool rangeDecoded = ImgCodecsCv2.ImDecodeMulti(multiBuffer, 1, 2, out memoryPages);

                    var metadata = new[] { new ImageMetadataChunk(ImageMetadataType.Exif, exif) };
                    bool metadataWritten = ImgCodecsCv2.ImWriteWithMetadata(jpegPath, first, metadata);
                    byte[] metadataBuffer = ImgCodecsCv2.ImEncodeWithMetadata(".jpeg", first, metadata);
                    using (ImageMetadataResult metadataResult = ImgCodecsCv2.ImDecodeWithMetadata(metadataBuffer))
                    using (var animation = new Animation(2, new Scalar(1, 2, 3, 4)))
                    {
                        animation.SetFrames(pages, new[] { 40, 80 });
                        byte[] animationBuffer = ImgCodecsCv2.ImEncodeAnimation(".gif", animation);
                        bool animationWritten = ImgCodecsCv2.ImWriteAnimation(gifPath, animation);
                        using (var decodedAnimation = new Animation())
                        using (var collection = new ImageCollection(tiffPath, ImreadModes.Unchanged))
                        using (Mat lazyPage = collection[1])
                        {
                            bool animationDecoded = ImgCodecsCv2.ImDecodeAnimation(animationBuffer, decodedAnimation);
                            return "ImgCodecs upstream families: multi=" + multiWritten + "/" + multiRead
                                + "/pages=" + filePages.Length + "/range=" + rangeDecoded + ":" + memoryPages.Length
                                + ", count=" + ImgCodecsCv2.ImCount(tiffPath)
                                + ", metadata=" + metadataWritten + "/chunks=" + metadataResult.Metadata.Count
                                + ", animation=" + animationWritten + "/" + animationDecoded + "/frames=" + decodedAnimation.FrameCount
                                + ", collection=" + collection.Count + "/lazyRows=" + lazyPage.Rows
                                + ", probes=" + ImgCodecsCv2.HaveImageReader(tiffPath) + "/" + ImgCodecsCv2.HaveImageWriter(".gif");
                        }
                    }
                }
                finally
                {
                    DisposeAll(filePages);
                    DisposeAll(memoryPages);
                    if (File.Exists(tiffPath)) File.Delete(tiffPath);
                    if (File.Exists(jpegPath)) File.Delete(jpegPath);
                    if (File.Exists(gifPath)) File.Delete(gifPath);
                    if (Directory.Exists(root)) Directory.Delete(root);
                }
            }
        }

        private static byte[] CreateSampleExifMetadata()
        {
            return new byte[]
            {
                (byte)'M', (byte)'M', 0, 42, 0, 0, 0, 8, 0, 10, 1, 0, 0, 4, 0, 0, 0, 1, 0, 0, 5,
                0, 1, 1, 0, 4, 0, 0, 0, 1, 0, 0, 2, 208, 1, 2, 0, 3, 0, 0, 0, 1,
                0, 10, 0, 0, 1, 18, 0, 3, 0, 0, 0, 1, 0, 1, 0, 0, 1, 14, 0, 2, 0, 0,
                0, 34, 0, 0, 0, 176, 1, 49, 0, 2, 0, 0, 0, 7, 0, 0, 0, 210, 1, 26,
                0, 5, 0, 0, 0, 1, 0, 0, 0, 218, 1, 27, 0, 5, 0, 0, 0, 1, 0, 0, 0,
                226, 1, 40, 0, 3, 0, 0, 0, 1, 0, 2, 0, 0, 135, 105, 0, 4, 0, 0, 0,
                1, 0, 0, 0, 134, 0, 0, 0, 0, 0, 3, 144, 0, 0, 7, 0, 0, 0, 4, 48, 50,
                50, 49, 160, 2, 0, 4, 0, 0, 0, 1, 0, 0, 5, 0, 160, 3, 0, 4, 0, 0,
                0, 1, 0, 0, 2, 208, 0, 0, 0, 0, 83, 97, 109, 112, 108, 101, 32, 49, 48,
                45, 98, 105, 116, 32, 105, 109, 97, 103, 101, 32, 119, 105, 116, 104, 32,
                109, 101, 116, 97, 100, 97, 116, 97, 0, 79, 112, 101, 110, 67, 86, 0, 0,
                0, 0, 0, 72, 0, 0, 0, 1, 0, 0, 0, 72, 0, 0, 0, 1
            };
        }

        private static string RunImgProcRemainingParitySummary()
        {
            using (Mat color = new Mat(16, 16, MatType.CV_8UC3))
            using (Mat camera = new Mat(3, 3, MatType.CV_64FC1))
            using (Mat distortion = Mat.Zeros(new Size(5, 1), MatType.CV_64FC1))
            using (Mat fisheyeDistortion = Mat.Zeros(new Size(4, 1), MatType.CV_64FC1))
            using (Mat rectification = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (Mat rvec = Mat.Zeros(new Size(1, 3), MatType.CV_64FC1))
            using (Mat tvec = Mat.Zeros(new Size(1, 3), MatType.CV_64FC1))
            using (Mat phase1 = Mat.Zeros(new Size(8, 8), MatType.CV_32FC1))
            using (Mat phase2 = Mat.Zeros(new Size(8, 8), MatType.CV_32FC1))
            using (Mat accumulator = Mat.Zeros(new Size(8, 8), MatType.CV_32FC1))
            using (Mat signature1 = new Mat(2, 2, MatType.CV_32FC1))
            using (Mat signature2 = new Mat(2, 2, MatType.CV_32FC1))
            using (Mat flow = new Mat())
            using (Mat markers = Mat.Zeros(new Size(16, 16), MatType.CV_32SC1))
            using (Mat grabMask = Mat.Zeros(new Size(16, 16), MatType.CV_8UC1))
            using (Mat backgroundModel = new Mat())
            using (Mat foregroundModel = new Mat())
            using (Mat gray = Mat.Zeros(new Size(16, 16), MatType.CV_8UC1))
            using (Mat template = Mat.Zeros(new Size(3, 3), MatType.CV_8UC1))
            using (FontFace font = new FontFace("sans"))
            {
                var pixels = new byte[16 * 16 * 3];
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        bool inside = x >= 3 && x < 13 && y >= 3 && y < 13;
                        int offset = (y * 16 + x) * 3;
                        pixels[offset] = (byte)(inside ? 180 + x % 5 : 10 + x);
                        pixels[offset + 1] = (byte)(inside ? 40 + y % 7 : 15 + y);
                        pixels[offset + 2] = (byte)(inside ? 60 + (x + y) % 9 : 20 + (x + y) % 5);
                    }
                }
                color.CopyFrom(pixels);
                camera.CopyFrom<double>(new[]
                {
                    12.0, 0.0, 8.0,
                    0.0, 12.0, 8.0,
                    0.0, 0.0, 1.0
                });
                tvec.CopyFrom<double>(new[] { 0.0, 0.0, 2.0 });

                using (Mat undistorted = Calib3DCv2.Undistort(color, camera, distortion, camera))
                using (Mat fisheye = Calib3DCv2.FisheyeUndistortImage(color, camera, fisheyeDistortion, camera, new Size(16, 16)))
                using (Mat patch = ImgProcCv2.GetRectSubPix(undistorted, new Size(5, 5), new Point2f(8, 8)))
                using (Mat polar = ImgProcCv2.WarpPolar(fisheye, new Size(16, 16), new Point2f(8, 8), 7, InterpolationFlags.Linear))
                {
                    UndistortRectifyMapResult maps = Calib3DCv2.InitInverseRectificationMap(
                        camera,
                        distortion,
                        rectification,
                        camera,
                        new Size(16, 16),
                        MatType.CV_32FC1);
                    using (maps.Map1)
                    using (maps.Map2)
                    {
                        Calib3DCv2.DrawFrameAxes(undistorted, camera, distortion, rvec, tvec, 0.5F, 1);

                        float[] values1 = new float[64];
                        float[] values2 = new float[64];
                        values1[2 * 8 + 2] = 1;
                        values2[3 * 8 + 4] = 1;
                        phase1.CopyFrom(values1);
                        phase2.CopyFrom(values2);
                        using (Mat window = ImgProcCv2.CreateHanningWindow(new Size(8, 8), MatType.CV_32F))
                        {
                            ImgProcCv2.Accumulate(phase1, accumulator);
                            ImgProcCv2.AccumulateSquare(phase1, accumulator);
                            ImgProcCv2.AccumulateProduct(phase1, phase2, accumulator);
                            ImgProcCv2.AccumulateWeighted(phase2, accumulator, 0.25);
                            Point2d shift = ImgProcCv2.PhaseCorrelate(phase1, phase2, window, out double response);
                            Point2d iterativeShift = ImgProcCv2.PhaseCorrelateIterative(phase1, phase2);

                            signature1.CopyFrom<float>(new[] { 1.0F, 0.0F, 1.0F, 1.0F });
                            signature2.CopyFrom<float>(new[] { 1.0F, 0.25F, 1.0F, 1.25F });
                            float lowerBound = 0;
                            float emd = ImgProcCv2.EMD(signature1, signature2, DistanceTypes.L2, ref lowerBound, flow: flow);

                            using (Mat filtered = ImgProcCv2.PyrMeanShiftFiltering(color, 2, 8, 0))
                            {
                                int[] markerValues = new int[256];
                                markerValues[2 * 16 + 2] = 1;
                                markerValues[13 * 16 + 13] = 2;
                                markers.CopyFrom(markerValues);
                                ImgProcCv2.Watershed(color, markers);
                                ImgProcCv2.GrabCut(
                                    color,
                                    grabMask,
                                    new Rect(2, 2, 12, 12),
                                    backgroundModel,
                                    foregroundModel,
                                    1,
                                    GrabCutModes.InitWithRect);

                                ImgProcCv2.Rectangle(gray, new Rect(3, 3, 8, 8), new Scalar(255), -1);
                                using (Mat match = ImgProcCv2.MatchTemplate(gray, template, TemplateMatchModes.CCoeffNormed))
                                {
                                    ImgProcCv2.FindContoursLinkRuns(gray, out Point[][] contours, out Vec4i[] hierarchy);
                                    font.SetInstance(Array.Empty<int>());
                                    Point textNext = ImgProcCv2.PutText(
                                        filtered,
                                        "OpenCV",
                                        new Point(1, 14),
                                        new Scalar(255, 255, 255),
                                        font,
                                        8);
                                    Rect textBounds = ImgProcCv2.GetTextSize(
                                        new Size(16, 16),
                                        "OpenCV",
                                        new Point(1, 14),
                                        font,
                                        8);

                                    return "ImgProc remaining families: calibration=" + maps.Rows + "x" + maps.Cols
                                        + ", patch=" + patch.Rows + "x" + patch.Cols
                                        + ", polar=" + polar.Rows + "x" + polar.Cols
                                        + ", phase=" + shift + "/response=" + response
                                        + ", iterative=" + iterativeShift
                                        + ", emd=" + emd + "/lowerBound=" + lowerBound + "/flowRows=" + flow.Rows
                                        + ", filtered=" + filtered.Type
                                        + ", match=" + match.Rows + "x" + match.Cols
                                        + ", linkRuns=" + contours.Length + "/hierarchy=" + hierarchy.Length
                                        + ", grabModels=" + backgroundModel.Rows + "/" + foregroundModel.Rows
                                        + ", font=" + font.Name
                                        + ", text=" + textBounds.Width + "x" + textBounds.Height + "/next=" + textNext;
                                }
                            }
                        }
                    }
                }
            }
        }

        private static string RunDnnForwardSummary(string modelPath, string configPath, string framework, Mat blob)
        {
            using (DnnNetObject net = DnnNetObject.ReadNet(modelPath, configPath, framework))
            {
                net.SetPreferableBackend(OpenCvSharp.Dnn.DnnBackend.OpenCV);
                net.SetPreferableTarget(OpenCvSharp.Dnn.DnnTarget.Cpu);
                net.SetInput(blob);
                using (Mat output = net.Forward())
                {
                    string[] layerNames = net.GetLayerNames();
                    string[] outNames = net.GetUnconnectedOutLayersNames();
                    int[] outLayerIds = net.GetUnconnectedOutLayers();
                    string[] layerTypes = net.GetLayerTypes();
                    Mat[] multiOutputs = outNames.Length == 0 ? Array.Empty<Mat>() : net.Forward(outNames);
                    return "modelForward=" + output.Rows + "x" + output.Cols
                        + ", layers=" + layerNames.Length
                        + ", outLayers=" + outNames.Length
                        + ", outLayerIds=" + outLayerIds.Length
                        + ", layerTypes=" + layerTypes.Length
                        + ", multiOutputs=" + DisposeAndCount(multiOutputs)
                        + ", perfLayers=" + net.GetPerfProfile().LayerTimings.Length;
                }
            }
        }

        private static string RunDnnStructuredDefaultSummary()
        {
            string fixturePath = Path.Combine(AppContext.BaseDirectory, "Dnn", "Fixtures", "identity-opset13.onnx.base64");
            byte[] model = Convert.FromBase64String(File.ReadAllText(fixturePath).Trim());
            using (DnnNetObject net = DnnNetObject.ReadNetFromOnnx(model, OpenCvSharp.Dnn.DnnEngine.Classic))
            using (Mat image = new Mat(2, 2, MatType.CV_32FC1))
            {
                image.CopyFrom(new[] { 1.0F, 2.0F, 3.0F, 4.0F });
                using (Mat blob = DnnCv2.BlobFromImage(image, new OpenCvSharp.Dnn.Image2BlobParams()))
                {
                    net.SetPreferableBackend(OpenCvSharp.Dnn.DnnBackend.OpenCV)
                        .SetPreferableTarget(OpenCvSharp.Dnn.DnnTarget.Cpu)
                        .SetProfilingMode(OpenCvSharp.Dnn.DnnProfilingMode.Detailed)
                        .SetInput(blob, "input");
                    string[] layerNames = net.GetLayerNames();
                    string[] outputNames = net.GetUnconnectedOutLayersNames();
                    int layerId = net.GetLayerId(layerNames[0]);
                    int[][] inputShapes = { new[] { 1, 1, 2, 2 } };
                    int[] inputTypes = { MatType.CV_32F };
                    OpenCvSharp.Dnn.DnnLayerShapes shapes = net.GetLayerShapes(inputShapes, inputTypes, layerId);
                    long flops = net.GetFLOPS(inputShapes, inputTypes);
                    OpenCvSharp.Dnn.DnnMemoryConsumption memory = net.GetMemoryConsumption(inputShapes, inputTypes);
                    net.FinalizeNetwork();
                    using (Mat output = net.Forward(outputNames[0]))
                    {
                        Mat[][] nested = net.ForwardAndRetrieve(outputNames);
                        try
                        {
                            int nestedCount = 0;
                            for (int i = 0; i < nested.Length; i++) nestedCount += nested[i].Length;
                            OpenCvSharp.Dnn.DnnDetailedPerfProfile detailed = net.GetDetailedPerfProfile();
                            return "DNN structured: modelBytes=" + model.Length
                                + ", format=" + net.ModelFormat
                                + ", layers=" + layerNames.Length
                                + ", inferredShapes=" + shapes.InputShapes.Length + "/" + shapes.OutputShapes.Length
                                + ", outputDims=" + output.Dims
                                + ", outputValues=" + string.Join(",", output.ToArray<float>())
                                + ", nestedMats=" + nestedCount
                                + ", flops=" + flops
                                + ", memory=" + memory.WeightsBytes + "/" + memory.BlobBytes
                                + ", profileRows=" + detailed.Count;
                        }
                        finally
                        {
                            for (int i = 0; i < nested.Length; i++)
                                for (int j = 0; j < nested[i].Length; j++)
                                    nested[i][j].Dispose();
                        }
                    }
                }
            }
        }

        private static bool IsExtendedConsoleSamplesEnabled()
        {
            return IsEnvironmentFlagEnabled(ConsoleExtendedVariable, CompatibilityConsoleExtendedAlias);
        }

        private static bool IsUnstableNativeSmokeEnabled()
        {
            return IsEnvironmentFlagEnabled(UnstableNativeSmokeVariable, CompatibilityUnstableNativeSmokeAlias);
        }

        private static void PrintStableRound73Summaries()
        {
            Console.WriteLine(GetBioInspiredUnstableSkipSummary());
            Console.WriteLine(GetXStereoSummary());
            Console.WriteLine("AlphaMat infoFlow=skipped in default console sample");
        }

        private static string RunVideoIODefaultSummary()
        {
            string path = Path.Combine(Path.GetTempPath(), "opencv-csharp-api-videoio-default.avi");
            int fourcc = VideoWriterObject.FourCC("MJPG");
            int framesWritten = 0;
            int decodedFrames = 0;
            int streamDecodedFrames = 0;
            bool writerOpened = false;
            bool captureOpened = false;
            bool streamOpened = false;
            double frameCount = -1;
            string writerBackend = string.Empty;
            string captureBackend = string.Empty;
            string streamStatus = "not-opened";
            VideoCaptureAPIs[] backends = Array.Empty<VideoCaptureAPIs>();
            VideoCaptureAPIs[] streamBackends = Array.Empty<VideoCaptureAPIs>();

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                using (Mat frame = new Mat(32, 32, MatType.CV_8UC3))
                using (VideoWriterObject writer = new VideoWriterObject())
                {
                    writerOpened = writer.Open(path, fourcc, 10.0, frame.Size, Array.Empty<int>());
                    if (writerOpened)
                    {
                        writerBackend = writer.GetBackendName();
                        for (int i = 0; i < 3; i++)
                        {
                            frame.SetTo(new Scalar(24 + i * 40, 64 + i * 32, 128 + i * 24));
                            if (writer.Write(frame))
                            {
                                framesWritten++;
                            }
                        }
                    }
                }

                using (VideoCaptureObject capture = new VideoCaptureObject())
                using (Mat decoded = new Mat())
                {
                    capture.ExceptionMode = false;
                    captureOpened = capture.Open(path, VideoCaptureAPIs.Any, Array.Empty<int>());
                    if (captureOpened)
                    {
                        captureBackend = capture.GetBackendName();
                        frameCount = capture.FrameCount;
                        while (capture.Read(decoded))
                        {
                            decodedFrames++;
                        }
                    }
                }

                backends = VideoIORegistry.GetBackends();
                streamBackends = VideoIORegistry.GetStreamBackends();
                VideoCaptureAPIs streamApi = streamBackends.Length == 0 ? VideoCaptureAPIs.Any : streamBackends[0];
                try
                {
                    using (FileStream stream = File.OpenRead(path))
                    using (VideoCaptureObject streamCapture = new VideoCaptureObject())
                    using (Mat streamed = new Mat())
                    {
                        streamCapture.ExceptionMode = false;
                        streamOpened = streamCapture.Open(stream, streamApi, true, Array.Empty<int>());
                        if (streamOpened)
                        {
                            while (streamCapture.Read(streamed))
                            {
                                streamDecodedFrames++;
                            }
                            streamStatus = "decoded";
                        }
                        else
                        {
                            streamStatus = "backend-rejected";
                        }
                    }
                }
                catch (OpenCvException ex)
                {
                    streamStatus = "boundary:" + ex.Message.Replace("\r", " ").Replace("\n", " ");
                }

                string firstBackend = backends.Length == 0 ? string.Empty : VideoIORegistry.GetBackendName(backends[0]);
                bool firstBuiltIn = backends.Length > 0 && VideoIORegistry.IsBackendBuiltIn(backends[0]);
                return "VideoIO default: writer=" + writerOpened + "/" + framesWritten
                    + ", capture=" + captureOpened + "/" + decodedFrames
                    + ", frameCount=" + frameCount
                    + ", stream=" + streamOpened + "/" + streamDecodedFrames + "/" + streamStatus
                    + ", writerBackend=" + writerBackend
                    + ", captureBackend=" + captureBackend
                    + ", registry=" + backends.Length + "/" + firstBackend + "/builtIn=" + firstBuiltIn
                    + ", streamBackends=" + streamBackends.Length + "/api=" + streamApi;
            }
            catch (OpenCvException ex)
            {
                return "VideoIO default boundary: " + ex.Message.Replace("\r", " ").Replace("\n", " ");
            }
            finally
            {
                try
                {
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
                catch
                {
                    // The sample remains useful even when a backend delays file release.
                }
            }
        }

        private static string RunPhotoHdrDefaultSummary()
        {
            using (Mat exposure0 = new Mat(16, 16, MatType.CV_8UC3, new Scalar(32, 40, 48)))
            using (Mat exposure1 = new Mat(16, 16, MatType.CV_8UC3, new Scalar(96, 104, 112)))
            using (Mat exposure2 = new Mat(16, 16, MatType.CV_8UC3, new Scalar(192, 200, 208)))
            using (Mat exposureTimes = new Mat(3, 1, MatType.CV_32FC1, new Scalar(0.5)))
            using (Mat aligned0 = new Mat())
            using (Mat aligned1 = new Mat())
            using (Mat aligned2 = new Mat())
            using (Mat cameraResponse = new Mat())
            using (Mat fusedExposure = new Mat())
            using (AlignMTB align = PhotoCv2.CreateAlignMTB(cut: false))
            using (CalibrateDebevec calibrate = PhotoCv2.CreateCalibrateDebevec(samples: 16))
            using (MergeMertens merge = PhotoCv2.CreateMergeMertens())
            {
                Mat[] exposures = { exposure0, exposure1, exposure2 };
                align.Process(exposures, new[] { aligned0, aligned1, aligned2 });
                calibrate.Process(exposures, cameraResponse, exposureTimes);
                merge.Process(exposures, fusedExposure);

                return "Photo HDR default: aligned=3/" + aligned0.Rows + "x" + aligned0.Cols
                    + ", response=" + cameraResponse.Rows + "x" + cameraResponse.Cols
                    + ", fused=" + fusedExposure.Rows + "x" + fusedExposure.Cols;
            }
        }

        private static string RunVideoOpticalFlowObjectSummary()
        {
            using (Mat first = CreateVideoOpticalFlowFrame(0))
            using (Mat second = CreateVideoOpticalFlowFrame(1))
            using (FarnebackOpticalFlowObject farneback = FarnebackOpticalFlowObject.Create(numLevels: 3, winSize: 9, numIterations: 3))
            using (VariationalRefinementObject variational = VariationalRefinementObject.Create())
            using (DisOpticalFlowObject dis = DisOpticalFlowObject.Create())
            using (SparsePyrLkOpticalFlowObject sparse = SparsePyrLkOpticalFlowObject.Create(winSize: new Size(11, 11), maxLevel: 1))
            using (Mat farnebackFlow = farneback.Calc(first, second))
            using (Mat refinedFlow = variational.Calc(first, second))
            using (Mat disFlow = dis.Calc(first, second))
            {
                Point2f[] points = { new Point2f(10, 10), new Point2f(15, 15), new Point2f(20, 20) };
                sparse.Flags = OpticalFlowFlags.UseInitialFlow;
                Point2f[] nextPoints = sparse.Calc(first, second, points, points, out byte[] status, out float[] error);
                farneback.CollectGarbage();
                variational.CollectGarbage();
                dis.CollectGarbage();

                return "Video optical-flow objects: farneback=" + farnebackFlow.Rows + "x" + farnebackFlow.Cols + "/type=" + farnebackFlow.Type
                    + ", variational=" + refinedFlow.Rows + "x" + refinedFlow.Cols + "/type=" + refinedFlow.Type
                    + ", dis=" + disFlow.Rows + "x" + disFlow.Cols + "/type=" + disFlow.Type
                    + ", sparse=" + nextPoints.Length + "/status=" + status.Length + "/error=" + error.Length;
            }
        }

        private static Mat CreateVideoOpticalFlowFrame(int offset)
        {
            var frame = new Mat(32, 32, MatType.CV_8UC1);
            var pixels = new byte[32 * 32];
            for (int y = 7; y < 25; y++)
            {
                for (int x = 7 + offset; x < 23 + offset; x++)
                {
                    pixels[(y * 32) + x] = (byte)(((x + y) & 1) == 0 ? 255 : 80);
                }
            }
            frame.CopyFrom(pixels);
            return frame;
        }

        private static string RunVideoEccTrackerMilSummary()
        {
            using (Mat reference = CreateVideoEccFrame())
            using (OpenCvSharp.Video.ECCRegistrationResult registration = VideoCv2.FindTransformECC(
                reference,
                reference,
                OpenCvSharp.Video.MotionType.Translation,
                TermCriteria.ByCountAndEpsilon(20, 1e-6)))
            using (Mat first = CreateVideoTrackerMilFrame(0))
            using (Mat second = CreateVideoTrackerMilFrame(2))
            using (VideoTrackerMILObject tracker = VideoTrackerMILObject.Create())
            {
                var box = new Rect(20, 22, 20, 20);
                tracker.Init(first, box);
                bool found = tracker.Update(second, ref box);
                double correlation = VideoCv2.ComputeECC(reference, reference);

                return "Video ECC/TrackerMIL: correlation=" + correlation.ToString("F6", CultureInfo.InvariantCulture)
                    + ", score=" + registration.Score.ToString("F6", CultureInfo.InvariantCulture)
                    + ", warp=" + registration.WarpMatrix.Rows + "x" + registration.WarpMatrix.Cols
                    + "/type=" + registration.WarpMatrix.Type
                    + ", initialized=" + tracker.IsInitialized
                    + ", found=" + found
                    + ", box=" + box;
            }
        }

        private static Mat CreateVideoEccFrame()
        {
            var frame = new Mat(64, 64, MatType.CV_8UC1);
            var pixels = new byte[64 * 64];
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    pixels[(y * 64) + x] = (byte)((x * 7 + y * 11 + ((x / 5 + y / 7) & 1) * 80) & 255);
                }
            }
            frame.CopyFrom(pixels);
            return frame;
        }

        private static Mat CreateVideoTrackerMilFrame(int offset)
        {
            var frame = new Mat(80, 80, MatType.CV_8UC1);
            var pixels = new byte[80 * 80];
            for (int y = 0; y < 80; y++)
            {
                for (int x = 0; x < 80; x++)
                {
                    pixels[(y * 80) + x] = (byte)((x * 3 + y * 5) & 31);
                }
            }
            for (int y = 22; y < 42; y++)
            {
                for (int x = 20 + offset; x < 40 + offset; x++)
                {
                    pixels[(y * 80) + x] = (byte)(((x + y) & 1) == 0 ? 240 : 96);
                }
            }
            frame.CopyFrom(pixels);
            return frame;
        }

        private static string RunPhotoCcmDefaultSummary()
        {
            using (Mat measured = CreatePhotoCcmSamples())
            using (Mat reference = measured.Clone())
            using (ColorCorrectionModel model = PhotoCv2.CreateColorCorrectionModel(
                measured, reference, ColorSpace.Srgb))
            using (Mat image = new Mat(2, 3, MatType.CV_8UC3, new Scalar(48, 96, 160)))
            {
                model.SetDistance(DistanceType.RgbLinear);
                model.SetMaxCount(20);
                model.SetEpsilon(0.01);
                using Mat ccm = model.Compute();
                using Mat corrected = model.CorrectImage(image);
                double loss = model.GetLoss();

                string document;
                using (var writer = new FileStorage(
                    "memory.yml",
                    FileStorageModes.Write | FileStorageModes.Memory | FileStorageModes.FormatYaml))
                {
                    model.Write(writer);
                    document = writer.ReleaseAndGetString();
                }

                bool persisted;
                using (var reader = new FileStorage(
                    document,
                    FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml))
                using (FileNode node = reader["ColorCorrectionModel"])
                using (ColorCorrectionModel loaded = PhotoCv2.CreateColorCorrectionModel())
                {
                    loaded.Read(node);
                    using Mat loadedCcm = loaded.GetColorCorrectionMatrix();
                    persisted = loadedCcm.Size.Equals(ccm.Size) && loadedCcm.Type == ccm.Type;
                }

                return "Photo CCM default: ccm=" + ccm.Rows + "x" + ccm.Cols
                    + ", loss=" + loss.ToString("G6", CultureInfo.InvariantCulture)
                    + ", corrected=" + corrected.Rows + "x" + corrected.Cols + "/type=" + corrected.Type
                    + ", persisted=" + persisted;
            }
        }

        private static Mat CreatePhotoCcmSamples()
        {
            var samples = new Mat(24, 1, MatType.CV_64FC3);
            var values = new double[24 * 3];
            for (int i = 0; i < 24; i++)
            {
                values[(i * 3)] = 0.08 + (((i * 7) % 19) + 1) / 25.0;
                values[(i * 3) + 1] = 0.07 + (((i * 11) % 17) + 1) / 24.0;
                values[(i * 3) + 2] = 0.06 + (((i * 13) % 16) + 1) / 23.0;
            }

            samples.CopyFrom(values);
            return samples;
        }

        private static string RunPhotoIntelligentScissorsDefaultSummary()
        {
            using (Mat image = CreatePhotoIntelligentScissorsImage())
            using (var scissors = new IntelligentScissorsMB())
            {
                scissors.SetEdgeFeatureCannyParameters(20.0, 60.0);
                scissors.ApplyImage(image);
                scissors.BuildMap(new Point(2, 2));
                using Mat contour = scissors.GetContour(new Point(13, 2));
                int[] points = contour.ToArray<int>();
                return "Photo Intelligent Scissors default: image=" + image.Rows + "x" + image.Cols
                    + ", contour=" + contour.Rows + "x" + contour.Cols + "/type=" + contour.Type
                    + ", endpoints=" + points[0] + "," + points[1]
                    + "->" + points[points.Length - 2] + "," + points[points.Length - 1];
            }
        }

        private static string RunPhotoFinalCallablesDefaultSummary()
        {
            using var observation0 = new Mat(4, 4, MatType.CV_8UC1, new Scalar(80));
            using var observation1 = new Mat(4, 4, MatType.CV_8UC1, new Scalar(96));
            using Mat denoised = PhotoCv2.DenoiseTvl1(
                new[] { observation0, observation1 },
                lambda: 1.0,
                niters: 2);

            const string calibration = "%YAML:1.0\n" +
                "image_width: 4\n" +
                "image_height: 4\n" +
                "red_channel:\n" +
                "  coeffs_x: [0.]\n" +
                "  coeffs_y: [0.]\n" +
                "blue_channel:\n" +
                "  coeffs_x: [0.]\n" +
                "  coeffs_y: [0.]\n";
            using var storage = new FileStorage(
                calibration,
                FileStorageModes.Read | FileStorageModes.Memory | FileStorageModes.FormatYaml);
            using FileNode root = storage.Root();
            using ChromaticAberrationParameters parameters =
                PhotoCv2.LoadChromaticAberrationParams(root);
            using var image = new Mat(4, 4, MatType.CV_8UC3, new Scalar(20, 40, 80));
            using Mat corrected = PhotoCv2.CorrectChromaticAberration(
                image,
                parameters.Coefficients,
                parameters.CalibrationSize,
                parameters.Degree);

            return "Photo final default: tvl1=" + denoised.Rows + "x" + denoised.Cols + "/type=" + denoised.Type
                + ", coefficients=" + parameters.Coefficients.Rows + "x" + parameters.Coefficients.Cols
                + "/degree=" + parameters.Degree + "/size=" + parameters.CalibrationSize
                + ", corrected=" + corrected.Rows + "x" + corrected.Cols + "/type=" + corrected.Type;
        }

        private static Mat CreatePhotoIntelligentScissorsImage()
        {
            const int size = 16;
            var image = new Mat(size, size, MatType.CV_8UC1);
            var pixels = new byte[size * size];
            for (int coordinate = 2; coordinate <= 13; coordinate++)
            {
                pixels[(2 * size) + coordinate] = 255;
                pixels[(13 * size) + coordinate] = 255;
                pixels[(coordinate * size) + 2] = 255;
                pixels[(coordinate * size) + 13] = 255;
            }
            image.CopyFrom(pixels);
            return image;
        }

        private static string RunCalib3DUpstreamParitySummary()
        {
            using (var subdiv = new Subdiv2D(new Rect2f(0, 0, 100, 100)))
            using (var source = new Mat(8, 2, MatType.CV_32FC1))
            using (var destination = new Mat(8, 2, MatType.CV_32FC1))
            using (var mask = new Mat())
            using (Mat cameraMatrix1 = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (Mat cameraMatrix2 = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (Mat distCoeffs1 = Mat.Zeros(4, 1, MatType.CV_64FC1))
            using (Mat distCoeffs2 = Mat.Zeros(4, 1, MatType.CV_64FC1))
            using (Mat r = Mat.Eye(3, 3, MatType.CV_64FC1))
            using (var t = new Mat(3, 1, MatType.CV_64FC1))
            {
                Point2f[] points =
                {
                    new Point2f(10, 10), new Point2f(85, 10), new Point2f(85, 85),
                    new Point2f(10, 85), new Point2f(48, 48)
                };
                subdiv.Insert(points);
                int centerVertex = subdiv.FindNearest(new Point2f(47, 47), out Point2f nearest);
                subdiv.GetVoronoiFacetList(new[] { centerVertex }, out Point2f[][] facets, out Point2f[] centers);

                float[] sourceValues = { 0, 0, 40, 0, 40, 30, 0, 30, 8, 7, 31, 6, 34, 24, 7, 22 };
                float[] destinationValues = new float[sourceValues.Length];
                for (int index = 0; index < sourceValues.Length; index += 2)
                {
                    destinationValues[index] = sourceValues[index] + 3;
                    destinationValues[index + 1] = sourceValues[index + 1] + 4;
                }
                source.CopyFrom(sourceValues);
                destination.CopyFrom(destinationValues);
                using (Mat homography = Calib3DCv2.FindHomography(source, destination, mask, new UsacParams()))
                {
                    cameraMatrix1.SetValue(0, 500.0);
                    cameraMatrix1.SetValue(2, 320.0);
                    cameraMatrix1.SetValue(4, 500.0);
                    cameraMatrix1.SetValue(5, 240.0);
                    cameraMatrix2.CopyFrom(cameraMatrix1.ToArray<double>());
                    t.SetValue(0, 0.2);
                    FisheyeStereoRectifyResult rectified = Calib3DCv2.FisheyeStereoRectify(
                        cameraMatrix1,
                        distCoeffs1,
                        cameraMatrix2,
                        distCoeffs2,
                        new Size(640, 480),
                        r,
                        t);
                    using (rectified.R1)
                    using (rectified.R2)
                    using (rectified.P1)
                    using (rectified.P2)
                    using (rectified.Q)
                    {
                        return "Calib3D upstream: edges=" + subdiv.GetEdgeList().Length
                            + ", triangles=" + subdiv.GetTriangleList().Length
                            + ", voronoi=" + facets.Length + "/" + centers.Length
                            + ", nearest=" + nearest
                            + ", usac=" + homography.Rows + "x" + homography.Cols
                            + ", fisheyeQ=" + rectified.Q.Rows + "x" + rectified.Q.Cols;
                    }
                }
            }
        }

        private static string GetBioInspiredUnstableSkipSummary()
        {
            return "BioInspired retina/tone/transient=skipped; set OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1 for experimental linked smoke";
        }

        private static string GetXStereoSummary()
        {
            try
            {
                return RunXStereoSummary();
            }
            catch (OpenCvException ex) when (ex.Message.IndexOf("xstereo", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "XStereo boundary: " + ex.Message;
            }
            catch (EntryPointNotFoundException ex)
            {
                return "XStereo boundary: stale native runtime: " + ex.Message;
            }
        }

        private static string GetAlphaMatSummaryLast()
        {
            try
            {
                return RunAlphaMatSummary();
            }
            catch (OpenCvException ex) when (ex.Message.IndexOf("alphamat", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("linked with OpenCV", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "AlphaMat boundary: " + ex.Message;
            }
            catch (EntryPointNotFoundException ex)
            {
                return "AlphaMat boundary: stale native runtime: " + ex.Message;
            }
        }

        private static string ReadEmptyDnnMetadataSummary(DnnNetObject net)
        {
            try
            {
                return "emptyOutLayerIds=" + net.GetUnconnectedOutLayers().Length
                    + ", emptyLayerTypes=" + net.GetLayerTypes().Length;
            }
            catch (OpenCvException ex)
            {
                return "emptyMetadata=unavailable:" + ex.Message;
            }
        }

        private static int DisposeAndCount(Mat[] mats)
        {
            try
            {
                return mats.Length;
            }
            finally
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i].Dispose();
                }
            }
        }

        private static string RunPtCloudSummary()
        {
            using (Mat depth = new Mat(2, 2, MatType.CV_16UC1, new Scalar(1000)))
            using (Mat cameraMatrix = Mat.Eye(3, 3, MatType.CV_32F))
            using (Mat rescaled = new Mat())
            using (Mat points3d = new Mat())
            using (RgbdNormalsObject normals = RgbdNormalsObject.Create(2, 2, MatType.CV_32F, cameraMatrix, 3))
            {
                PtCloudCv2.RescaleDepth(depth, MatType.CV_32F, rescaled);
                PtCloudCv2.DepthTo3d(rescaled, cameraMatrix, points3d);
                normals.Cache();
                return "PtCloud depth=" + depth.Rows + "x" + depth.Cols
                    + ", rescaledType=" + rescaled.Type
                    + ", points3d=" + points3d.Rows + "x" + points3d.Cols + "/type=" + points3d.Type
                    + ", normalsMethod=" + normals.Method;
            }
        }

        private static string RunQualitySummary()
        {
            using (Mat reference = new Mat(4, 4, MatType.CV_8UC1, new Scalar(10)))
            using (Mat comparison = new Mat(4, 4, MatType.CV_8UC1, new Scalar(12)))
            using (Mat qualityMap = new Mat())
            {
                Scalar mse = QualityMSE.Compute(reference, comparison, qualityMap);
                Scalar psnr = QualityPSNR.Compute(reference, comparison);
                Scalar ssim = QualitySSIM.Compute(reference, comparison);
                Scalar gmsd = QualityGMSD.Compute(reference, comparison);
                string brisqueSummary = "BRISQUE=skipped";
                string model = GetEnvironmentVariable(BrisqueModelVariable, CompatibilityBrisqueModelAlias) ?? string.Empty;
                string range = GetEnvironmentVariable(BrisqueRangeVariable, CompatibilityBrisqueRangeAlias) ?? string.Empty;
                if (!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(range))
                {
                    brisqueSummary = "BRISQUE=" + QualityBRISQUE.Compute(reference, model, range).V0;
                }

                return "Quality MSE=" + mse.V0
                    + ", PSNR=" + psnr.V0
                    + ", SSIM=" + ssim.V0
                    + ", GMSD=" + gmsd.V0
                    + ", map=" + qualityMap.Rows + "x" + qualityMap.Cols
                    + ", " + brisqueSummary;
            }
        }

        private static string RunXPhotoSummary()
        {
            using (Mat color = new Mat(8, 8, MatType.CV_8UC3, new Scalar(10, 20, 30)))
            using (Mat gray = new Mat(8, 8, MatType.CV_8UC1, new Scalar(127)))
            using (Mat gains = new Mat())
            using (Mat denoised = new Mat())
            using (SimpleWB simple = XPhotoCv2.CreateSimpleWB())
            using (GrayworldWB grayworld = XPhotoCv2.CreateGrayworldWB())
            {
                simple.P = 1.0F;
                grayworld.SaturationThreshold = 0.95F;
                XPhotoCv2.ApplyChannelGains(color, gains, 1.0F, 1.1F, 0.9F);
                XPhotoCv2.DctDenoising(gray, denoised, 1.0, 4);
                return "XPhoto gains=" + gains.Rows + "x" + gains.Cols
                    + ", dct=" + denoised.Rows + "x" + denoised.Cols
                    + ", SimpleWB.P=" + simple.P
                    + ", Grayworld=" + grayworld.SaturationThreshold
                    + ", BM3DStep=" + Bm3dSteps.StepAll;
            }
        }

        private static string RunXImgProcSummary()
        {
            using (Mat gray = new Mat(16, 16, MatType.CV_8UC1, new Scalar(96)))
            using (Mat binary = new Mat())
            using (Mat skeleton = new Mat())
            using (Mat guided = new Mat())
            using (Mat weighted = new Mat())
            using (Mat fgs = new Mat())
            using (Mat color = CreateXImgProcColorImage())
            using (Mat l0 = new Mat())
            using (Mat slicLabels = new Mat())
            using (Mat slicMask = new Mat())
            using (Mat seedsLabels = new Mat())
            using (Mat lscLabels = new Mat())
            using (Mat lineImage = new Mat(32, 32, MatType.CV_8UC1, new Scalar(0)))
            using (Mat lineDrawing = new Mat(32, 32, MatType.CV_8UC3, new Scalar(0, 0, 0)))
            using (GuidedFilter guidedFilter = XImgProcCv2.CreateGuidedFilter(gray, 2, 1.0))
            using (FastGlobalSmootherFilter smoother = XImgProcCv2.CreateFastGlobalSmootherFilter(gray, 8.0, 12.0))
            using (SuperpixelSLIC slic = XImgProcCv2.CreateSuperpixelSLIC(color, SLICType.SLICO, 4, 10.0F))
            using (SuperpixelSEEDS seeds = XImgProcCv2.CreateSuperpixelSEEDS(color.Cols, color.Rows, color.Channels, 4, 2))
            using (SuperpixelLSC lsc = XImgProcCv2.CreateSuperpixelLSC(color, 4, 0.075F))
            using (FastLineDetector fld = XImgProcCv2.CreateFastLineDetector(lengthThreshold: 6, cannyApertureSize: 3))
            {
                ImgProcCv2.Rectangle(gray, new Rect(4, 4, 8, 8), new Scalar(180), -1);
                ImgProcCv2.Line(lineImage, new Point(3, 4), new Point(28, 24), new Scalar(255), 1);

                XImgProcCv2.NiBlackThreshold(gray, binary, 255.0, ThresholdTypes.Binary, 3, -0.2);
                XImgProcCv2.Thinning(binary, skeleton);
                guidedFilter.Filter(gray, guided);
                XImgProcCv2.WeightedMedianFilter(gray, gray, weighted, 1, 12.0, WeightedMedianFilterWeightType.Off);
                smoother.Filter(gray, fgs);
                XImgProcCv2.L0Smooth(color, l0);

                slic.Iterate(1);
                slic.GetLabels(slicLabels);
                slic.GetLabelContourMask(slicMask);
                seeds.Iterate(color, 1);
                seeds.GetLabels(seedsLabels);
                lsc.Iterate(1);
                lsc.GetLabels(lscLabels);

                LineSegment[] lines = fld.Detect(lineImage);
                if (lines.Length > 0)
                {
                    fld.DrawSegments(lineDrawing, lines);
                }

                return "XImgProc binary=" + binary.Rows + "x" + binary.Cols
                    + ", skeleton=" + skeleton.Rows + "x" + skeleton.Cols
                    + ", guided=" + guided.Rows + "x" + guided.Cols
                    + ", weighted=" + weighted.Rows + "x" + weighted.Cols
                    + ", fgs=" + fgs.Rows + "x" + fgs.Cols
                    + ", l0=" + l0.Rows + "x" + l0.Cols
                    + ", slic=" + slic.NumberOfSuperpixels + "/" + slicLabels.Rows + "x" + slicLabels.Cols + "/" + slicMask.Type
                    + ", seeds=" + seeds.NumberOfSuperpixels + "/" + seedsLabels.Rows + "x" + seedsLabels.Cols
                    + ", lsc=" + lsc.NumberOfSuperpixels + "/" + lscLabels.Rows + "x" + lscLabels.Cols
                    + ", fld=" + lines.Length;
            }
        }

        private static string RunXImgProcSecondBatchSummary()
        {
            using (Mat gray = new Mat(16, 16, MatType.CV_8UC1, new Scalar(96)))
            using (Mat color = CreateXImgProcColorImage())
            using (Mat disparity = CreateXImgProcDisparityMap())
            using (Mat disparityRight = CreateXImgProcDisparityMap())
            using (Mat filtered = new Mat())
            using (Mat disparityVis = new Mat())
            using (DisparityWLSFilter wls = XImgProcCv2.CreateDisparityWLSFilterGeneric(false))
            using (Mat fromPoints = Calib3DCv2.ToPointMat(CreateXImgProcFromPoints()))
            using (Mat toPoints = Calib3DCv2.ToPointMat(CreateXImgProcToPoints()))
            using (Mat edgeAwareFlow = new Mat())
            using (Mat ricFlow = new Mat())
            using (EdgeAwareInterpolator edgeAware = XImgProcCv2.CreateEdgeAwareInterpolator())
            using (RICInterpolator ric = XImgProcCv2.CreateRICInterpolator())
            using (Mat edgeImage = new Mat(48, 48, MatType.CV_8UC1, new Scalar(0)))
            using (Mat edgeMap = new Mat(48, 48, MatType.CV_32FC1, new Scalar(0.1)))
            using (Mat orientationMap = new Mat(48, 48, MatType.CV_32FC1, new Scalar(0.0)))
            using (Mat edEdges = new Mat())
            using (EdgeDrawing edgeDrawing = XImgProcCv2.CreateEdgeDrawing())
            using (EdgeBoxes edgeBoxes = XImgProcCv2.CreateEdgeBoxes(maxBoxes: 5, minScore: 0.0F, minBoxArea: 4.0F))
            {
                ImgProcCv2.Rectangle(gray, new Rect(4, 4, 8, 8), new Scalar(180), -1);
                wls.Lambda = 8000.0;
                wls.SigmaColor = 1.5;
                wls.Filter(disparity, gray, filtered, disparityRight, new Rect(0, 0, gray.Cols, gray.Rows), gray);
                XImgProcCv2.GetDisparityVis(filtered, disparityVis);
                double mse = XImgProcCv2.ComputeMSE(disparity, filtered, new Rect(0, 0, gray.Cols, gray.Rows));

                edgeAware.K = 4;
                edgeAware.UsePostProcessing = false;
                edgeAware.Interpolate(color, fromPoints, color, toPoints, edgeAwareFlow);
                ric.K = 4;
                ric.SuperpixelSize = 8;
                ric.SuperpixelNNCount = 8;
                ric.UseGlobalSmootherFilter = false;
                ric.UseVariationalRefinement = false;
                ric.Interpolate(color, fromPoints, color, toPoints, ricFlow);

                ImgProcCv2.Line(edgeImage, new Point(4, 4), new Point(42, 34), new Scalar(255), 1);
                ImgProcCv2.Circle(edgeImage, new Point(24, 24), 8, new Scalar(255), 1);
                EdgeDrawingParams parameters = edgeDrawing.Params;
                parameters.MinLineLength = 4;
                parameters.MinPathLength = 4;
                edgeDrawing.Params = parameters;
                edgeDrawing.DetectEdges(edgeImage);
                edgeDrawing.GetEdgeImage(edEdges);
                LineSegment[] lines = edgeDrawing.DetectLines();
                EdgeBox[] boxes = edgeBoxes.GetBoundingBoxes(edgeMap, orientationMap);

                return "XImgProc second disparity=" + filtered.Rows + "x" + filtered.Cols + "/" + disparityVis.Type + "/mse=" + mse
                    + ", sparse=" + edgeAwareFlow.Rows + "x" + edgeAwareFlow.Cols + "/" + ricFlow.Rows + "x" + ricFlow.Cols
                    + ", edge=" + edEdges.Rows + "x" + edEdges.Cols + "/lines=" + lines.Length
                    + ", boxes=" + boxes.Length
                    + ", fbs=" + RunFastBilateralSolverStatus(gray);
            }
        }

        private static string RunXImgProcRemainingUtilitiesSummary()
        {
            using (Mat gray = new Mat(16, 16, MatType.CV_8UC1, new Scalar(96)))
            using (Mat color = CreateXImgProcColorImage())
            using (Mat dericheX = new Mat())
            using (Mat dericheY = new Mat())
            using (Mat paillouX = new Mat())
            using (Mat paillouY = new Mat())
            using (RidgeDetectionFilter ridge = XImgProcCv2.CreateRidgeDetectionFilter())
            using (Mat ridgeOutput = new Mat())
            using (Mat contour = CreateXImgProcContourPointMat())
            using (Mat sampled = XImgProcCv2.ContourSampling(contour, 8))
            using (Mat descriptor = XImgProcCv2.FourierDescriptor(contour, nbElt: 8, nbFD: 4))
            using (ContourFitting fitting = XImgProcCv2.CreateContourFitting(8, 3))
            using (Mat transform = fitting.EstimateTransformation(sampled, sampled, out double distance))
            using (Mat transformed = XImgProcCv2.TransformFD(sampled, transform, fdContour: false))
            using (Mat rl = XImgProcRlCv2.Threshold(gray, 100.0, ThresholdTypes.Binary))
            using (Mat rlKernel = XImgProcRlCv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)))
            using (Mat rlOpened = XImgProcRlCv2.MorphologyEx(rl, MorphTypes.Open, rlKernel))
            using (Mat painted = new Mat(gray.Rows, gray.Cols, MatType.CV_8UC1, new Scalar(0)))
            using (Mat rleRuns = XImgProcRlCv2.CreateRLEImage(new[] { new Point3i(1, 4, 1), new Point3i(1, 4, 2) }, gray.Size))
            using (ScanSegment scan = XImgProcCv2.CreateScanSegment(color.Cols, color.Rows, 8, slices: 1, mergeSmall: true))
            using (Mat scanLabels = new Mat())
            using (GraphSegmentation graph = XImgProcCv2.CreateGraphSegmentation(sigma: 0.5, k: 50.0F, minSize: 2))
            using (Mat graphLabels = new Mat())
            using (SelectiveSearchSegmentation selectiveSearch = XImgProcCv2.CreateSelectiveSearchSegmentation())
            using (SelectiveSearchSegmentationStrategy colorStrategy = XImgProcCv2.CreateSelectiveSearchSegmentationStrategyColor())
            using (SelectiveSearchSegmentationStrategyMultiple multipleStrategy = XImgProcCv2.CreateSelectiveSearchSegmentationStrategyMultiple())
            using (Mat complex = CreateXImgProcComplexImage())
            using (Mat covariance = XImgProcCv2.CovarianceEstimation(complex, 2, 2))
            {
                ImgProcCv2.Rectangle(gray, new Rect(4, 4, 8, 8), new Scalar(180), -1);

                XImgProcCv2.GradientDericheX(color, dericheX, 0.5, 0.0005);
                XImgProcCv2.GradientDericheY(color, dericheY, 0.5, 0.0005);
                XImgProcCv2.GradientPaillouX(color, paillouX, 1.0, 1.0);
                XImgProcCv2.GradientPaillouY(color, paillouY, 1.0, 1.0);
                ridge.GetRidgeFilteredImage(gray, ridgeOutput);

                XImgProcRlCv2.Paint(painted, rlOpened, new Scalar(255));
                scan.Iterate(color);
                scan.GetLabels(scanLabels);
                graph.ProcessImage(color, graphLabels);

                selectiveSearch.SetBaseImage(color);
                selectiveSearch.SwitchToSingleStrategy(k: 20, sigma: 0.8F);
                multipleStrategy.AddStrategy(colorStrategy, 1.0F);
                multipleStrategy.ClearStrategies();
                Rect[] proposals = selectiveSearch.Process();

                return "XImgProc remaining gradients=" + dericheX.Rows + "x" + dericheX.Cols + "/" + paillouX.Rows + "x" + paillouX.Cols
                    + ", ridge=" + ridgeOutput.Rows + "x" + ridgeOutput.Cols
                    + ", fourier=" + sampled.Rows + "x" + sampled.Cols + "/" + descriptor.Rows + "x" + descriptor.Cols + "/dist=" + distance
                    + ", transform=" + transformed.Rows + "x" + transformed.Cols
                    + ", rle=" + rl.Rows + "x" + rl.Cols + "/kernel=" + XImgProcRlCv2.IsRLMorphologyPossible(rlKernel) + "/paint=" + painted.Rows + "x" + painted.Cols + "/runs=" + rleRuns.Rows
                    + ", scan=" + scan.NumberOfSuperpixels + "/" + scanLabels.Rows + "x" + scanLabels.Cols
                    + ", graph=" + graphLabels.Rows + "x" + graphLabels.Cols
                    + ", selective=" + proposals.Length
                    + ", covariance=" + covariance.Rows + "x" + covariance.Cols;
            }
        }

        private static string RunMLSummary()
        {
            using (Mat samples = CreateMLSamples())
            using (Mat responses = CreateMLResponses())
            using (Mat query = new Mat(1, 2, MatType.CV_32FC1))
            using (Mat knnResults = new Mat())
            using (Mat neighborResponses = new Mat())
            using (Mat dists = new Mat())
            using (Mat bayesResults = new Mat())
            using (Mat bayesProbabilities = new Mat())
            using (Mat svmPrediction = new Mat())
            using (Mat annResponses = CreateMLAnnResponses())
            using (Mat annLayers = new Mat(1, 3, MatType.CV_32SC1))
            using (Mat annPrediction = new Mat())
            using (TrainData data = TrainData.Create(samples, SampleTypes.RowSample, responses))
            using (MLKNearestObject knn = MLKNearestObject.Create())
            using (MLSvmObject svm = MLSvmObject.Create())
            using (MLNormalBayesClassifierObject bayes = MLNormalBayesClassifierObject.Create())
            using (MLAnnMlpObject ann = MLAnnMlpObject.Create())
            {
                query.CopyFrom<float>(new float[] { 0.1F, 0.2F });
                annLayers.CopyFrom<int>(new int[] { 2, 4, 1 });
                data.SetTrainTestSplitRatio(0.75, shuffle: false);

                knn.DefaultK = 1;
                knn.IsClassifierModel = true;
                knn.AlgorithmType = KNearestTypes.BruteForce;
                knn.Train(data);
                float knnValue = knn.FindNearest(query, 1, knnResults, neighborResponses, dists);

                svm.Type = SVMTypes.CSvc;
                svm.SetKernel(SVMKernelTypes.Linear);
                svm.C = 1.0;
                svm.TermCriteria = TermCriteria.ByCountAndEpsilon(100, 1e-6);
                svm.Train(samples, SampleTypes.RowSample, responses);
                float svmValue = svm.Predict(query, svmPrediction);
                using (Mat supportVectors = svm.GetSupportVectors())
                {
                    bayes.Train(samples, SampleTypes.RowSample, responses);
                    float bayesValue = bayes.PredictProb(query, bayesResults, bayesProbabilities);

                    ann.SetLayerSizes(annLayers);
                    ann.SetActivationFunction(ANN_MLPActivationFunctions.Identity);
                    ann.SetTrainMethod(ANN_MLPTrainingMethods.Rprop, 0.1, 1e-6);
                    ann.TermCriteria = TermCriteria.ByCountAndEpsilon(300, 1e-6);
                    ann.Train(samples, SampleTypes.RowSample, annResponses);
                    float annValue = ann.Predict(query, annPrediction);
                    using (Mat annWeights = ann.GetWeights(1))
                    {
                        return "ML train=" + data.NTrainSamples + "/" + data.NSamples
                            + ", KNN=" + knnValue
                            + ", SVM=" + svmValue
                            + ", support=" + supportVectors.Rows + "x" + supportVectors.Cols
                            + ", Bayes=" + bayesValue
                            + ", probs=" + bayesProbabilities.Rows + "x" + bayesProbabilities.Cols
                            + ", ANN=" + annValue
                            + ", layers=2x4x1"
                            + ", weights=" + annWeights.Rows + "x" + annWeights.Cols;
                    }
                }
            }
        }

        private static string RunMLTreeModelsDefaultSummary()
        {
            using (Mat samples = CreateMLSamples())
            using (Mat responses = CreateMLResponses())
            using (Mat query = new Mat(1, 2, MatType.CV_32FC1))
            using (Mat dtreesResults = new Mat())
            using (Mat rtreesResults = new Mat())
            using (Mat boostResults = new Mat())
            using (MLDTreesObject dtrees = MLDTreesObject.Create())
            using (MLRTreesObject rtrees = MLRTreesObject.Create())
            using (MLBoostObject boost = MLBoostObject.Create())
            {
                query.CopyFrom<float>(new float[] { 0.1F, 0.2F });

                dtrees.MaxDepth = 4;
                dtrees.MinSampleCount = 1;
                dtrees.CVFolds = 0;
                bool dtreesTrained = dtrees.Train(samples, SampleTypes.RowSample, responses);
                float dtreesPrediction = dtrees.Predict(query, dtreesResults);

                CoreCv2.SetRngSeed(24680);
                rtrees.MaxDepth = 4;
                rtrees.MinSampleCount = 1;
                rtrees.CalculateVarImportance = true;
                rtrees.ActiveVarCount = 1;
                rtrees.TermCriteria = TermCriteria.ByCount(8);
                bool rtreesTrained = rtrees.Train(samples, SampleTypes.RowSample, responses);
                float rtreesPrediction = rtrees.Predict(query, rtreesResults);

                boost.BoostType = BoostTypes.Discrete;
                boost.WeakCount = 8;
                boost.WeightTrimRate = 0.9;
                boost.MinSampleCount = 1;
                bool boostTrained = boost.Train(samples, SampleTypes.RowSample, responses);
                float boostPrediction = boost.Predict(query, boostResults);

                using (Mat votes = rtrees.GetVotes(query, DTreesPredictionFlags.MaxVote))
                using (Mat importance = rtrees.GetVarImportance())
                {
                    return "ML trees: models=DTrees/RTrees/Boost"
                        + ", trained=" + dtreesTrained + "/" + rtreesTrained + "/" + boostTrained
                        + ", predictions=" + dtreesPrediction + "/" + rtreesPrediction + "/" + boostPrediction
                        + ", votes=" + votes.Rows + "x" + votes.Cols
                        + ", importance=" + importance.Total
                        + ", oobFinite=" + double.IsFinite(rtrees.OobError);
                }
            }
        }

        private static string RunMLEMDefaultSummary()
        {
            using (Mat samples = CreateMLSamples())
            using (Mat query = new Mat(1, 2, MatType.CV_32FC1))
            using (Mat logLikelihoods = new Mat())
            using (Mat labels = new Mat())
            using (Mat probabilities = new Mat())
            using (Mat predictionProbabilities = new Mat())
            using (MLEMObject model = MLEMObject.Create())
            {
                query.CopyFrom<float>(new float[] { 0.1F, 0.2F });
                CoreCv2.SetRngSeed(13579);
                model.ClustersNumber = 2;
                model.CovarianceMatrixType = EMCovarianceMatrixTypes.Generic;
                bool trained = model.TrainEM(samples, logLikelihoods, labels, probabilities);
                EMPredictionResult prediction = model.Predict2(query, predictionProbabilities);
                using (Mat weights = model.GetWeights())
                using (Mat means = model.GetMeans())
                {
                    Mat[] covariances = model.GetCovariances();
                    try
                    {
                        return "ML EM: clusters=" + model.ClustersNumber
                            + ", trained=" + trained
                            + ", label=" + prediction.Label
                            + ", finiteLikelihood=" + double.IsFinite(prediction.LogLikelihood)
                            + ", probs=" + predictionProbabilities.Rows + "x" + predictionProbabilities.Cols
                            + ", weights=" + weights.Total
                            + ", means=" + means.Rows + "x" + means.Cols
                            + ", covariances=" + covariances.Length;
                    }
                    finally
                    {
                        DisposeAll(covariances);
                    }
                }
            }
        }

        private static string RunMLRemainingCallablesDefaultSummary()
        {
            using (Mat samples = CreateMLSamples())
            using (Mat logisticResponses = new Mat(6, 1, MatType.CV_32FC1))
            using (Mat svmsgdResponses = new Mat(6, 1, MatType.CV_32FC1))
            using (Mat negativeQuery = new Mat(1, 2, MatType.CV_32FC1))
            using (Mat positiveQuery = new Mat(1, 2, MatType.CV_32FC1))
            {
                logisticResponses.CopyFrom<float>(new[] { 0.0F, 0.0F, 0.0F, 1.0F, 1.0F, 1.0F });
                svmsgdResponses.CopyFrom<float>(new[] { -1.0F, -1.0F, -1.0F, 1.0F, 1.0F, 1.0F });
                negativeQuery.CopyFrom<float>(new[] { 0.1F, 0.2F });
                positiveQuery.CopyFrom<float>(new[] { 5.2F, 5.1F });

                using (TrainData data = TrainData.Create(samples, SampleTypes.RowSample, logisticResponses))
                using (MLLogisticRegressionObject logistic = MLLogisticRegressionObject.Create())
                using (MLSVMSGDObject svmsgd = MLSVMSGDObject.Create())
                {
                    float[] sample = data.GetSample(4);
                    float[] firstVariable = data.GetValues(0);

                    logistic.LearningRate = 0.05;
                    logistic.Iterations = 1000;
                    logistic.TrainingMethod = LogisticRegressionTrainingMethods.MiniBatch;
                    logistic.MiniBatchSize = 2;
                    logistic.TermCriteria = TermCriteria.ByCountAndEpsilon(1000, 1e-6);
                    bool logisticTrained = logistic.Train(samples, SampleTypes.RowSample, logisticResponses);

                    svmsgd.SetOptimalParameters();
                    svmsgd.TermCriteria = TermCriteria.ByCountAndEpsilon(10000, 1e-6);
                    bool svmsgdTrained = svmsgd.Train(samples, SampleTypes.RowSample, svmsgdResponses);

                    using (Mat thetas = logistic.GetLearntThetas())
                    using (Mat weights = svmsgd.GetWeights())
                    {
                        return "ML remaining: sample=" + string.Join(",", sample)
                            + ", values=" + firstVariable.Length
                            + ", logistic=" + logisticTrained + "/" + logistic.Predict(negativeQuery) + "/" + logistic.Predict(positiveQuery)
                            + ", thetas=" + thetas.Rows + "x" + thetas.Cols
                            + ", svmsgd=" + svmsgdTrained + "/" + svmsgd.Predict(negativeQuery) + "/" + svmsgd.Predict(positiveQuery)
                            + ", weights=" + weights.Rows + "x" + weights.Cols
                            + ", shiftFinite=" + float.IsFinite(svmsgd.Shift);
                    }
                }
            }
        }

        private static string RunImgHashSummary()
        {
            using (Mat image = CreateImgHashImage())
            using (Mat average = ImgHashCv2.AverageHash(image))
            using (Mat phash = ImgHashCv2.PHash(image))
            using (Mat blockMean = ImgHashCv2.BlockMeanHash(image, BlockMeanHashMode.Mode0))
            using (Mat colorMoment = ImgHashCv2.ColorMomentHash(image))
            using (Mat marrHildreth = ImgHashCv2.MarrHildrethHash(image))
            using (Mat radialVariance = ImgHashCv2.RadialVarianceHash(image))
            using (BlockMeanHashObject comparer = BlockMeanHashObject.Create(BlockMeanHashMode.Mode0))
            {
                double sameDistance = comparer.Compare(blockMean, blockMean);
                double[] mean = comparer.GetMean();
                return "ImgHash avg=" + average.Rows + "x" + average.Cols + "/" + average.Type
                    + ", pHash=" + phash.Rows + "x" + phash.Cols + "/" + phash.Type
                    + ", block=" + blockMean.Rows + "x" + blockMean.Cols + "/" + blockMean.Type
                    + ", colorMoment=" + colorMoment.Rows + "x" + colorMoment.Cols + "/" + colorMoment.Type
                    + ", marr=" + marrHildreth.Rows + "x" + marrHildreth.Cols + "/" + marrHildreth.Type
                    + ", radial=" + radialVariance.Rows + "x" + radialVariance.Cols + "/" + radialVariance.Type
                    + ", same=" + sameDistance
                    + ", meanCount=" + mean.Length;
            }
        }

        private static string RunPlotSummary()
        {
            using (Mat y = CreateDoubleColumn(0.0, 1.0, 0.0, 2.0, 1.5))
            using (Plot2d plot = PlotCv2.CreatePlot2d(y))
            using (Mat rendered = new Mat())
            {
                plot.SetPlotSize(480, 320)
                    .SetMinX(0.0)
                    .SetMaxX(4.0)
                    .SetMinY(-0.5)
                    .SetMaxY(2.5)
                    .SetNeedPlotLine(true)
                    .SetPlotLineWidth(2)
                    .SetShowGrid(true)
                    .SetShowText(false)
                    .SetGridLinesNumber(4)
                    .SetPlotLineColor(new Scalar(40, 120, 220))
                    .SetPlotBackgroundColor(new Scalar(255, 255, 255));

                plot.Render(rendered);

                return "Plot render=" + rendered.Rows + "x" + rendered.Cols + "/" + rendered.Type
                    + ", channels=" + rendered.Channels
                    + ", disposed=" + plot.IsDisposed;
            }
        }

        private static string RunShapeSummary()
        {
            using (Mat firstSignature = CreateFloatColumn(0.2F, 0.3F, 0.5F))
            using (Mat secondSignature = CreateFloatColumn(0.1F, 0.4F, 0.5F))
            using (Mat descriptors1 = CreateShapeDescriptors())
            using (Mat descriptors2 = CreateShapeDescriptors())
            using (Mat contour1 = CreateShapeContour(0.0F))
            using (Mat contour2 = CreateShapeContour(0.2F))
            using (NormHistogramCostExtractor extractor = ShapeCv2.CreateNormHistogramCostExtractor(NormTypes.L2, 2, 0.25F))
            using (HausdorffDistanceExtractor hausdorff = ShapeCv2.CreateHausdorffDistanceExtractor(NormTypes.L2, 0.6F))
            using (Mat costMatrix = extractor.BuildCostMatrix(descriptors1, descriptors2))
            {
                float distance = ShapeCv2.EMDL1(firstSignature, secondSignature);
                float contourDistance = hausdorff.ComputeDistance(contour1, contour2);
                extractor.NDummies = 2;
                extractor.DefaultCost = 0.25F;
                hausdorff.RankProportion = 0.6F;

                return "Shape EMDL1=" + distance
                    + ", hausdorff=" + contourDistance
                    + ", cost=" + costMatrix.Rows + "x" + costMatrix.Cols + "/" + costMatrix.Type
                    + ", dummies=" + extractor.NDummies
                    + ", defaultCost=" + extractor.DefaultCost
                    + ", rank=" + hausdorff.RankProportion;
            }
        }

        private static string RunLineDescriptorSummary()
        {
            using (Mat image = CreateLineDescriptorImage())
            using (BinaryDescriptor descriptor = BinaryDescriptor.Create())
            using (BinaryDescriptorMatcher matcher = BinaryDescriptorMatcher.Create())
            using (Mat descriptors = new Mat())
            {
                KeyLine[] detected = descriptor.Detect(image);
                using (Mat drawn = LineDescriptorCv2.DrawKeylines(image, detected, new Scalar(0, 255, 0)))
                {
                    KeyLine[] computed = descriptor.DetectAndCompute(image, null, detected, descriptors, useProvidedKeylines: detected.Length > 0);
                    int matchCount = 0;
                    int groupedCount = 0;
                    if (!descriptors.Empty && descriptors.Rows > 0)
                    {
                        matchCount = matcher.Match(descriptors, descriptors).Length;
                        groupedCount = matcher.KnnMatch(descriptors, descriptors, 1).Length;
                    }

                    return "LineDescriptor lines=" + detected.Length
                        + ", computed=" + computed.Length
                        + ", descriptors=" + descriptors.Rows + "x" + descriptors.Cols + "/" + descriptors.Type
                        + ", matches=" + matchCount
                        + ", knnGroups=" + groupedCount
                        + ", drawn=" + drawn.Rows + "x" + drawn.Cols;
                }
            }
        }

        private static string RunPhaseUnwrappingSummary()
        {
            using (HistogramPhaseUnwrapping unwrapper = HistogramPhaseUnwrapping.Create(8, 8, (float)(3.0 * Math.PI * Math.PI)))
            using (Mat wrapped = CreatePhaseUnwrappingMap())
            using (Mat unwrapped = unwrapper.UnwrapPhaseMap(wrapped))
            using (Mat reliability = unwrapper.GetInverseReliabilityMap())
            {
                return "PhaseUnwrapping unwrapped=" + unwrapped.Rows + "x" + unwrapped.Cols + "/" + unwrapped.Type
                    + ", reliability=" + reliability.Rows + "x" + reliability.Cols + "/" + reliability.Type
                    + ", disposed=" + unwrapper.IsDisposed;
            }
        }

        private static string RunStructuredLightSummary()
        {
            using (GrayCodePattern gray = GrayCodePattern.Create(16, 8))
            using (SinusoidalPattern sinusoidal = SinusoidalPattern.Create(new SinusoidalPatternParams
            {
                Width = 24,
                Height = 16,
                NbrOfPeriods = 4,
                Method = SinusoidalPatternMethod.Psp
            }))
            {
                Mat[] grayImages = gray.Generate();
                Mat[] sinusoidalImages = sinusoidal.Generate();
                try
                {
                    gray.GetImagesForShadowMasks(out Mat black, out Mat white);
                    using (black)
                    using (white)
                    {
                        Point projectorPixel = new Point(0, 0);
                        bool found = grayImages.Length > 0 && gray.GetProjPixel(grayImages, 0, 0, out projectorPixel);
                        string grayShape = grayImages.Length == 0 ? "empty" : grayImages[0].Rows + "x" + grayImages[0].Cols;
                        string sinusoidalShape = sinusoidalImages.Length == 0 ? "empty" : sinusoidalImages[0].Rows + "x" + sinusoidalImages[0].Cols;
                        return "StructuredLight gray=" + grayImages.Length + "/" + gray.NumberOfPatternImages
                            + "/" + grayShape
                            + ", sinusoidal=" + sinusoidalImages.Length + "/" + sinusoidalShape
                            + ", shadows=" + black.Rows + "x" + black.Cols + "/" + white.Rows + "x" + white.Cols
                            + ", pixel=" + found + "/" + projectorPixel;
                    }
                }
                finally
                {
                    DisposeAll(grayImages);
                    DisposeAll(sinusoidalImages);
                }
            }
        }

        private static string RunIntensityTransformSummary()
        {
            using (Mat gray = CreateIntensityGrayImage())
            using (Mat bgr = CreateSmallBgrImage(8, 8))
            using (Mat log = IntensityTransformCv2.LogTransform(gray))
            using (Mat gamma = IntensityTransformCv2.GammaCorrection(gray, 1.2F))
            using (Mat autoscaled = IntensityTransformCv2.Autoscaling(gray))
            using (Mat stretched = IntensityTransformCv2.ContrastStretching(gray, 16, 0, 192, 255))
            {
                string bimefSummary = RunBimefSummary(bgr);
                return "IntensityTransform log=" + log.Rows + "x" + log.Cols + "/" + log.Type
                    + ", gamma=" + gamma.Rows + "x" + gamma.Cols + "/" + gamma.Type
                    + ", autoscale=" + autoscaled.Rows + "x" + autoscaled.Cols + "/" + autoscaled.Type
                    + ", contrast=" + stretched.Rows + "x" + stretched.Cols + "/" + stretched.Type
                    + ", bimef=" + bimefSummary;
            }
        }

        private static string RunBimefSummary(Mat source)
        {
            try
            {
                using (Mat bimef = IntensityTransformCv2.Bimef(source))
                {
                    return bimef.Rows + "x" + bimef.Cols + "/" + bimef.Type;
                }
            }
            catch (OpenCvException ex) when (ex.Message.IndexOf("BIMEF", StringComparison.OrdinalIgnoreCase) >= 0 && ex.Message.IndexOf("Eigen", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "requires-eigen";
            }
        }

        private static string RunFuzzySummary()
        {
            using (Mat gray = CreateFuzzyFloatImage())
            using (Mat bgr = CreateSmallBgrImage(8, 8))
            using (Mat mask = CreateFuzzyMask())
            using (Mat grayKernel = FuzzyCv2.CreateKernel(FuzzyFunctionType.Linear, 2, 1))
            using (Mat bgrKernel = FuzzyCv2.CreateKernel(FuzzyFunctionType.Linear, 2, bgr.Channels))
            using (Mat filtered = FuzzyCv2.Filter(bgr, bgrKernel))
            using (Mat inpainted = FuzzyCv2.Inpaint(bgr, mask, 2, FuzzyFunctionType.Linear, FuzzyInpaintAlgorithm.OneStep))
            using (Mat f0 = FuzzyCv2.FT02DProcess(gray, grayKernel))
            using (Mat f1 = FuzzyCv2.FT12DProcess(gray, grayKernel))
            {
                return "Fuzzy kernel=" + grayKernel.Rows + "x" + grayKernel.Cols + "/" + grayKernel.Type
                    + ", filter=" + filtered.Rows + "x" + filtered.Cols + "/" + filtered.Type
                    + ", inpaint=" + inpainted.Rows + "x" + inpainted.Cols + "/" + inpainted.Type
                    + ", f0=" + f0.Rows + "x" + f0.Cols + "/" + f0.Type
                    + ", f1=" + f1.Rows + "x" + f1.Cols + "/" + f1.Type;
            }
        }

        private static string RunHfsSummary()
        {
            using (Mat image = CreateSmallBgrImage(32, 32))
            using (HfsSegment segment = HfsCv2.CreateHfsSegment(32, 32))
            using (Mat drawn = segment.PerformSegmentCpu(image))
            using (Mat labels = segment.PerformSegmentCpu(image, draw: false))
            {
                return "HFS cpu=" + drawn.Rows + "x" + drawn.Cols + "/" + drawn.Type
                    + ", labels=" + labels.Rows + "x" + labels.Cols + "/" + labels.Type
                    + ", params=" + segment.SegEgbThresholdI + "/" + segment.MinRegionSizeI
                    + "/" + segment.SegEgbThresholdII + "/" + segment.MinRegionSizeII
                    + "/" + segment.SpatialWeight + "/" + segment.SlicSpixelSize + "/" + segment.NumSlicIter;
            }
        }

        private static string RunRegSummary()
        {
            using (Mat image = CreateRegGrayImage())
            using (MapShift shiftMap = RegCv2.CreateMapShift(1.0, 0.0))
            using (Mat shifted = shiftMap.InverseWarp(image))
            using (MapperGradShift mapper = RegCv2.CreateMapperGradShift())
            using (RegMap result = mapper.Calculate(image, shifted))
            using (Mat warped = result.Warp(shifted))
            using (RegMap inverse = result.InverseMap())
            using (Mat restored = inverse.Warp(image))
            using (MapAffine affine = RegCv2.CreateMapAffine(new AffineTransform2D(1.0, 0.0, 0.0, 1.0, 1.0, 0.0)))
            using (MapProjec projective = RegCv2.CreateMapProjec(ProjectiveTransform2D.Identity))
            using (MapperPyramid pyramid = RegCv2.CreateMapperPyramid(mapper))
            {
                shiftMap.GetShift(out double shiftX, out double shiftY);
                return "Reg shift=" + shiftX + "/" + shiftY
                    + ", calc=" + result.Kind + "/" + warped.Rows + "x" + warped.Cols + "/" + warped.Type
                    + ", inverse=" + restored.Rows + "x" + restored.Cols + "/" + restored.Type
                    + ", affine=" + affine.Transform.ShiftX + "/" + affine.Transform.ShiftY
                    + ", proj=" + projective.Kind
                    + ", pyramid=" + pyramid.NumLevels + "/" + pyramid.NumIterationsPerScale;
            }
        }

        private static string RunSurfaceMatchingSummary()
        {
            string ppfSummary;
            using (Mat cloud = CreateSurfaceMatchingPointCloud())
            using (Icp icp = SurfaceMatchingCv2.CreateIcp(iterations: 1, tolerance: 0.05F, numLevels: 1))
            {
                IcpRegistrationResult result = icp.RegisterModelToScene(cloud, cloud);
                ppfSummary = RunPpfSummary(cloud);
                return "SurfaceMatching ICP=" + result.ResultCode + "/" + result.Residual + "/" + result.Pose.Length
                    + ", PPF=" + ppfSummary;
            }
        }

        private static string RunPpfSummary(Mat cloud)
        {
            try
            {
                using (Ppf3DDetector detector = SurfaceMatchingCv2.CreatePpf3DDetector(0.2, 0.2, 20.0))
                {
                    detector.SetSearchParams();
                    detector.TrainModel(cloud);
                    Pose3DResult[] poses = detector.Match(cloud, 1.0, 0.2);
                    return "poses=" + poses.Length;
                }
            }
            catch (OpenCvException ex) when (IsTinyGeometryBoundary(ex))
            {
                return "tiny-boundary=" + ex.Message;
            }
        }

        private static string RunRapidSummary()
        {
            using (Mat image = CreateRapidEdgeImage())
            using (Mat mesh = CreateRapidMeshPoints())
            using (Mat tris = CreateRapidMeshTriangles())
            using (Mat camera = CreateRapidCameraMatrix())
            using (Mat rvec = CreateRapidPoseVector(0.0, 0.0, 0.0))
            using (Mat tvec = CreateRapidPoseVector(0.0, 0.0, 6.0))
            using (Mat pts2d = CreateRapidProjectedPoints())
            using (Mat wire = image.Clone())
            using (RapidSilhouetteTracker tracker = RapidSilhouetteTracker.Create(mesh, tris))
            {
                RapidCv2.DrawWireframe(wire, pts2d, tris, new Scalar(255), LineTypes.Line8);
                string runSummary = RunRapidIterationSummary(image, mesh, tris, camera, rvec, tvec);
                tracker.ClearState();
                return "Rapid wire=" + wire.Rows + "x" + wire.Cols + "/" + wire.Type
                    + ", run=" + runSummary
                    + ", trackerDisposed=" + tracker.IsDisposed;
            }
        }

        private static string RunRapidIterationSummary(Mat image, Mat mesh, Mat tris, Mat camera, Mat rvec, Mat tvec)
        {
            try
            {
                RapidResult result = RapidCv2.Run(image, 8, 3, mesh, tris, camera, rvec, tvec, computeRmsd: true);
                return result.Ratio + "/" + (result.Rmsd.HasValue ? result.Rmsd.Value.ToString() : "null");
            }
            catch (OpenCvException ex) when (IsTinyGeometryBoundary(ex))
            {
                return "tiny-boundary=" + ex.Message;
            }
        }

        private static bool IsTinyGeometryBoundary(OpenCvException exception)
        {
            return exception.Message.IndexOf("assert", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("failed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("nan", StringComparison.OrdinalIgnoreCase) >= 0 ||
                exception.Message.IndexOf("contours", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string RunAlphaMatSummary()
        {
            using (Mat image = CreateAlphaMatColorImage())
            using (Mat trimap = CreateAlphaMatTrimap())
            using (Mat matte = AlphaMatCv2.InfoFlow(image, trimap))
            {
                return "AlphaMat infoFlow=" + matte.Rows + "x" + matte.Cols + "/" + matte.Type;
            }
        }

        private static string RunBioInspiredSummary()
        {
            using (Mat image = CreateSmallBgrImage(32, 32))
            using (Retina retina = BioInspiredCv2Object.CreateRetina(image.Size))
            using (RetinaFastToneMapping toneMapping = BioInspiredCv2Object.CreateRetinaFastToneMapping(image.Size))
            using (TransientAreasSegmentationModule segmentation = BioInspiredCv2Object.CreateTransientAreasSegmentationModule(image.Size))
            {
                retina.Run(image);
                using (Mat parvo = retina.GetParvo())
                using (Mat magno = retina.GetMagno())
                using (Mat tone = toneMapping.Apply(image))
                {
                    RetinaParameters parameters = retina.Parameters;
                    return "BioInspired retina=" + parvo.Rows + "x" + parvo.Cols + "/" + parvo.Type
                        + ", magno=" + magno.Rows + "x" + magno.Cols + "/" + magno.Type
                        + ", tone=" + tone.Rows + "x" + tone.Cols + "/" + tone.Type
                        + ", transientSize=" + segmentation.Size
                        + ", params=" + parameters.Parvo.ColorMode + "/" + parameters.Magno.NormaliseOutput;
                }
            }
        }

        private static string RunXStereoSummary()
        {
            using (Mat left = CreateXStereoLeftImage())
            using (Mat right = CreateXStereoRightImage())
            using (Mat census = XStereoCv2Object.CensusTransform(left, 5))
            using (StereoBinaryBM bm = StereoBinaryBM.Create(16, 9))
            using (StereoBinarySGBM sgbm = StereoBinarySGBM.Create(0, 16, 3))
            using (QuasiDenseStereo quasiDense = QuasiDenseStereo.Create(left.Size))
            using (Mat bmDisparity = bm.Compute(left, right))
            using (Mat sgbmDisparity = sgbm.Compute(left, right))
            {
                string quasiDenseSummary = RunQuasiDenseSummary(quasiDense, left, right);
                return "XStereo census=" + census.Rows + "x" + census.Cols + "/" + census.Type
                    + ", bm=" + bmDisparity.Rows + "x" + bmDisparity.Cols + "/" + bmDisparity.Type
                    + ", sgbm=" + sgbmDisparity.Rows + "x" + sgbmDisparity.Cols + "/" + sgbmDisparity.Type
                    + ", quasiDense=" + quasiDenseSummary;
            }
        }

        private static string RunQuasiDenseSummary(QuasiDenseStereo quasiDense, Mat left, Mat right)
        {
            try
            {
                quasiDense.Process(left, right);
                MatchQuasiDense[] sparse = quasiDense.GetSparseMatches();
                MatchQuasiDense[] dense = quasiDense.GetDenseMatches();
                using (Mat disparity = quasiDense.GetDisparity())
                {
                    return sparse.Length + "/" + dense.Length + "/" + disparity.Rows + "x" + disparity.Cols + "/" + disparity.Type;
                }
            }
            catch (OpenCvException ex) when (IsTinyGeometryBoundary(ex))
            {
                return "tiny-boundary=" + ex.Message;
            }
        }

        private static string RunOptFlowSummary()
        {
            using (Mat first = CreateOptFlowFrame(2))
            using (Mat second = CreateOptFlowFrame(5))
            using (Mat sparseToDenseFlow = new Mat())
            using (Mat tvl1First = new Mat())
            using (Mat tvl1Second = new Mat())
            using (Mat tvl1Flow = new Mat())
            using (Mat silhouette = new Mat(24, 24, MatType.CV_8UC1, new Scalar(0)))
            using (Mat mhi = new Mat(24, 24, MatType.CV_32FC1, new Scalar(0)))
            using (Mat mask = new Mat())
            using (Mat orientation = new Mat())
            using (Mat segmask = new Mat())
            using (OptFlowRLOFParameterObject parameter = OptFlowRLOFParameterObject.Create())
            using (OptFlowDualTVL1Object tvl1 = OptFlowDualTVL1Object.Create(nscales: 2, warps: 1, innerIterations: 2, outerIterations: 1))
            {
                parameter.SupportRegionType = OptFlowSupportRegionType.Fixed;
                parameter.SolverType = OptFlowSolverType.Bilinear;
                parameter.MaxLevel = 1;
                parameter.SetUseMEstimator(false);

                OptFlowCv2Object.CalcOpticalFlowSparseToDense(first, second, sparseToDenseFlow, gridStep: 4, k: 8, sigma: 0.05F, usePostProc: false);

                ImgProcCv2.CvtColor(first, tvl1First, ColorConversionCodes.BGR2GRAY);
                ImgProcCv2.CvtColor(second, tvl1Second, ColorConversionCodes.BGR2GRAY);
                tvl1.Calc(tvl1First, tvl1Second, tvl1Flow);

                ImgProcCv2.Rectangle(silhouette, new Rect(4, 4, 8, 8), new Scalar(255), -1);
                OptFlowCv2Object.UpdateMotionHistory(silhouette, mhi, 1.0, 10.0);
                OptFlowCv2Object.CalcMotionGradient(mhi, mask, orientation, 0.25, 1.0);
                double angle = OptFlowCv2Object.CalcGlobalOrientation(orientation, mask, mhi, 1.0, 10.0);
                Rect[] motions = OptFlowCv2Object.SegmentMotion(mhi, segmask, 1.0, 0.5);

                return "OptFlow sparseToDense=" + sparseToDenseFlow.Rows + "x" + sparseToDenseFlow.Cols + "/" + sparseToDenseFlow.Type
                    + ", TVL1=" + tvl1Flow.Rows + "x" + tvl1Flow.Cols + "/" + tvl1Flow.Type
                    + ", RLOF=" + parameter.SupportRegionType + "/" + parameter.SolverType
                    + ", motionAngle=" + angle
                    + ", segments=" + motions.Length;
            }
        }

        private static string RunBgSegmSummary()
        {
            using (Mat first = CreateOptFlowFrame(1))
            using (Mat second = CreateOptFlowFrame(4))
            using (Mat mask = new Mat())
            using (Mat background = new Mat())
            using (Mat syntheticBackground = new Mat(32, 32, MatType.CV_8UC3, new Scalar(16, 24, 32)))
            using (Mat syntheticObject = new Mat(8, 8, MatType.CV_8UC3, new Scalar(220, 40, 30)))
            using (Mat generatedFrame = new Mat())
            using (Mat generatedMask = new Mat())
            using (BgSegmBackgroundSubtractorMOGObject mog = BgSegmBackgroundSubtractorMOGObject.Create(history: 10, nmixtures: 3))
            using (BgSegmBackgroundSubtractorGMGObject gmg = BgSegmBackgroundSubtractorGMGObject.Create(initializationFrames: 3, decisionThreshold: 0.7))
            using (BgSegmBackgroundSubtractorCNTObject cnt = BgSegmBackgroundSubtractorCNTObject.Create(minPixelStability: 2, useHistory: true, maxPixelStability: 8, isParallel: false))
            using (BgSegmSyntheticSequenceGeneratorObject generator = BgSegmSyntheticSequenceGeneratorObject.Create(syntheticBackground, syntheticObject))
            {
                mog.BackgroundRatio = 0.6;
                gmg.NumFrames = 3;
                cnt.UseHistory = true;

                mog.Apply(first, mask, 1.0);
                mog.Apply(second, mask, 0.5);

                gmg.Apply(first, mask, 1.0);
                gmg.Apply(second, mask, 0.5);
                cnt.Apply(first, mask, 1.0);
                cnt.Apply(second, mask, 0.5);
                cnt.GetBackgroundImage(background);
                generator.GetNextFrame(generatedFrame, generatedMask);

                return "BgSegm MOG mask=" + mask.Rows + "x" + mask.Cols + "/" + mask.Type
                    + ", background=" + background.Rows + "x" + background.Cols
                    + ", GMG frames=" + gmg.NumFrames
                    + ", CNT history=" + cnt.UseHistory
                    + ", synthetic=" + generatedFrame.Rows + "x" + generatedFrame.Cols + "/" + generatedMask.Type;
            }
        }

        private static string RunTrackingDefaultSummary()
        {
            try
            {
                return RunTrackingSummary();
            }
            catch (OpenCvException ex) when (ex.Message.IndexOf("tracking", StringComparison.OrdinalIgnoreCase) >= 0 || ex.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return "Tracking boundary: " + ex.Message;
            }
        }

        private static string RunTrackingSummary()
        {
            using (Mat first = CreateOptFlowFrame(2))
            using (Mat second = CreateOptFlowFrame(4))
            using (TrackerKCFObject kcf = TrackerKCFObject.Create(TrackerKCFParamsObject.Default))
            using (TrackerCSRTObject csrt = TrackerCSRTObject.Create())
            using (TrackerMOSSEObject mosse = TrackerMOSSEObject.Create())
            using (TrackerMILObject mil = TrackerMILObject.Create())
            using (TrackerMedianFlowObject medianFlow = TrackerMedianFlowObject.Create())
            using (LegacyTrackerBoostingObject boosting = LegacyTrackerBoostingObject.Create())
            using (LegacyTrackerTLDObject tld = LegacyTrackerTLDObject.Create())
            using (LegacyTrackerKCFObject legacyKcf = LegacyTrackerKCFObject.Create(TrackerKCFParamsObject.Default))
            using (LegacyTrackerCSRTObject legacyCsrt = LegacyTrackerCSRTObject.Create())
            using (OpenCvSharp.Tracking.Tracker upgraded = legacyKcf.Upgrade())
            using (OpenCvLegacyMultiTrackerObject multiTracker = OpenCvLegacyMultiTrackerObject.Create())
            {
                Rect modernBox = new Rect(6, 7, 8, 8);
                kcf.Init(first, modernBox);
                OpenCvSharp.Tracking.TrackerUpdateResult kcfUpdate = kcf.Update(second, modernBox);

                Rect csrtBox = new Rect(6, 7, 8, 8);
                csrt.Init(first, csrtBox);
                OpenCvSharp.Tracking.TrackerUpdateResult csrtUpdate = csrt.Update(second, csrtBox);

                Rect2d openCvLegacyBox = new Rect2d(6.0, 7.0, 8.0, 8.0);
                mil.Init(first, openCvLegacyBox);
                OpenCvSharp.Tracking.Legacy.LegacyTrackerUpdateResult milUpdate = mil.Update(second, openCvLegacyBox);
                bool added = multiTracker.Add(medianFlow, first, openCvLegacyBox);
                OpenCvSharp.Tracking.Legacy.LegacyMultiTrackerUpdateResult multiUpdate = multiTracker.Update(second);

                upgraded.Init(first, modernBox);
                OpenCvSharp.Tracking.TrackerUpdateResult upgradedUpdate = upgraded.Update(second, modernBox);

                return "Tracking KCF=" + kcfUpdate.Success + "/" + kcfUpdate.BoundingBox
                    + ", CSRT=" + csrtUpdate.Success + "/" + csrtUpdate.BoundingBox
                    + ", MOSSE disposed=" + mosse.IsDisposed
                    + ", MIL=" + milUpdate.Success + "/" + milUpdate.BoundingBox
                    + ", legacy=Boosting/TLD/KCF/CSRT"
                    + ", upgraded=" + upgradedUpdate.Success + "/" + upgradedUpdate.BoundingBox
                    + ", ready=" + (!boosting.IsDisposed && !tld.IsDisposed && !legacyCsrt.IsDisposed)
                    + ", MultiTracker added=" + added
                    + ", boxes=" + multiUpdate.BoundingBoxes.Length;
            }
        }

        private static string RunFaceSummary()
        {
            using (Mat first = CreateFaceImage(30))
            using (Mat second = CreateFaceImage(180))
            using (Mat query = CreateFaceImage(32))
            using (Mat bifInput = query.ConvertTo(MatType.CV_32FC1, 1.0 / 255.0))
            using (LBPHFaceRecognizerObject lbph = LBPHFaceRecognizerObject.Create(radius: 1, neighbors: 8, gridX: 4, gridY: 4))
            using (EigenFaceRecognizerObject eigen = EigenFaceRecognizerObject.Create(numComponents: 2))
            using (FisherFaceRecognizerObject fisher = FisherFaceRecognizerObject.Create(numComponents: 1))
            using (StandardCollector collector = StandardCollector.Create())
            using (BIF bif = BIF.Create(numBands: 2, numRotations: 3))
            using (FacemarkLBF facemark = FacemarkLBF.Create(CreateTinyLbfParams()))
            using (MACE mace = MACE.Create(imageSize: 32))
            {
                Mat[] training = new[] { first, second };
                int[] labels = new[] { 10, 20 };
                lbph.Train(training, labels);
                eigen.Train(training, labels);
                fisher.Train(training, labels);
                string facemarkSample = TryAddFacemarkTrainingSample(facemark, query);
                mace.Salt("console-sample");
                mace.Train(first, second);
                bool maceSame = mace.Same(query);

                lbph.SetLabelInfo(10, "first");
                FacePrediction prediction = lbph.PredictWithConfidence(query);
                lbph.Predict(query, collector);
                FacePredictionResult[] results = collector.GetResults(sorted: true);

                using (Mat bifFeatures = bif.Compute(bifInput))
                using (Mat eigenValues = eigen.GetEigenValues())
                using (Mat fisherLabels = fisher.GetLabels())
                {
                    return "Face lbph=" + prediction.Label + "/" + prediction.Confidence
                        + ", collector=" + collector.MinLabel + "/" + collector.MinDist + "/" + results.Length
                        + ", eigen=" + eigen.NumComponents + "/" + eigenValues.Rows + "x" + eigenValues.Cols
                        + ", fisher=" + fisher.NumComponents + "/" + fisherLabels.Rows + "x" + fisherLabels.Cols
                        + ", bif=" + bif.NumBands + "/" + bif.NumRotations + "/" + bifFeatures.Rows + "x" + bifFeatures.Cols
                        + ", facemarkLBF=" + facemark.NLandmarks + "/" + facemark.StageCount + "/" + facemarkSample
                        + ", mace=" + mace.Empty + "/" + maceSame + "/" + SaveAndLoadMace(mace);
                }
            }
        }

        private static string RunSaliencySummary()
        {
            using (Mat image = CreateSaliencyImage())
            using (Mat motionImage = CreateMotionSaliencyImage())
            using (StaticSaliencySpectralResidualObject spectral = StaticSaliencySpectralResidualObject.Create())
            using (StaticSaliencyFineGrainedObject fine = StaticSaliencyFineGrainedObject.Create())
            using (MotionSaliencyBinWangObject motion = MotionSaliencyBinWangObject.Create())
            using (ObjectnessBING objectness = ObjectnessBING.Create())
            using (Mat spectralMap = new Mat())
            using (Mat binaryMap = new Mat())
            using (Mat fineMap = new Mat())
            using (Mat motionMap = new Mat())
            {
                spectral.ImageWidth = image.Cols;
                spectral.ImageHeight = image.Rows;
                motion.SetImageSize(motionImage.Cols, motionImage.Rows);
                motion.Init();
                objectness.SetTrainingPath(Path.GetTempPath());
                objectness.SetBBResDir(Path.GetTempPath());
                objectness.Base = 2.0;
                objectness.NSS = 3;
                objectness.W = 8;

                bool spectralOk = spectral.ComputeSaliency(image, spectralMap);
                bool binaryOk = spectral.ComputeBinaryMap(spectralMap, binaryMap);
                bool fineOk = fine.ComputeSaliency(image, fineMap);
                bool motionOk = motion.ComputeSaliency(motionImage, motionMap);

                return "Saliency spectral=" + spectralOk + "/" + spectralMap.Rows + "x" + spectralMap.Cols
                    + ", binary=" + binaryOk + "/" + binaryMap.Rows + "x" + binaryMap.Cols
                    + ", fine=" + fineOk + "/" + fineMap.Rows + "x" + fineMap.Cols
                    + ", motion=" + motionOk + "/" + motionMap.Rows + "x" + motionMap.Cols
                    + ", image=" + spectral.ImageWidth + "x" + spectral.ImageHeight
                    + ", objectness=" + objectness.Base + "/" + objectness.NSS + "/" + objectness.W
                    + ", cached=" + objectness.GetBoxes().Length + "/" + objectness.GetObjectnessValues().Length;
            }
        }

        private static string SaveAndLoadMace(MACE mace)
        {
            string path = Path.Combine(Path.GetTempPath(), "opencv-csharp-sample-mace-" + Guid.NewGuid().ToString("N") + ".xml");
            try
            {
                mace.Save(path);
                using (MACE loaded = MACE.Load(path))
                {
                    return loaded.Empty ? "empty" : "loaded";
                }
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private static string TryAddFacemarkTrainingSample(FacemarkLBF facemark, Mat image)
        {
            if (string.IsNullOrEmpty(facemark.Parameters.CascadeFace))
            {
                return "noCascade";
            }

            facemark.AddTrainingSample(image, CreateFaceLandmarks());
            return "sampled";
        }

        private static string WriteReadFlowSummary(Mat flow)
        {
            string path = Path.Combine(Path.GetTempPath(), "opencv-csharp-sample-flow-" + Guid.NewGuid().ToString("N") + ".flo");
            try
            {
                bool written = VideoCv2.WriteOpticalFlow(path, flow);
                using (Mat read = VideoCv2.ReadOpticalFlow(path))
                {
                    return "flowIo=" + written + "/" + read.Rows + "x" + read.Cols + "/" + read.Type;
                }
            }
            finally
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        private static Mat CreateFloatColumn(params float[] values)
        {
            var mat = new Mat(values.Length, 1, MatType.CV_32FC1);
            mat.CopyFrom<float>(values);
            return mat;
        }

        private static Mat CreateDoubleColumn(params double[] values)
        {
            var mat = new Mat(values.Length, 1, MatType.CV_64FC1);
            mat.CopyFrom<double>(values);
            return mat;
        }

        private static Mat CreateShapeDescriptors()
        {
            var descriptors = new Mat(3, 2, MatType.CV_32FC1);
            descriptors.CopyFrom<float>(new float[]
            {
                0.0F, 1.0F,
                1.0F, 0.0F,
                0.5F, 0.5F
            });
            return descriptors;
        }

        private static Mat CreateShapeContour(float offset)
        {
            var mat = new Mat(4, 1, MatType.CV_32FC2);
            mat.CopyFrom<float>(new float[]
            {
                0.0F + offset, 0.0F,
                1.0F + offset, 0.0F,
                1.0F + offset, 1.0F,
                0.0F + offset, 1.0F
            });
            return mat;
        }

        private static Mat CreateLineDescriptorImage()
        {
            var image = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0));
            ImgProcCv2.Line(image, new Point(8, 12), new Point(56, 12), new Scalar(255), 2);
            ImgProcCv2.Line(image, new Point(10, 50), new Point(54, 18), new Scalar(255), 2);
            return image;
        }

        private static Mat CreatePhaseUnwrappingMap()
        {
            var mat = new Mat(8, 8, MatType.CV_32FC1);
            var values = new float[8 * 8];
            for (int i = 0; i < values.Length; i++)
            {
                int x = i % 8;
                int y = i / 8;
                values[i] = (float)(Math.Sin(x * 0.35) + Math.Cos(y * 0.25));
            }

            mat.CopyFrom<float>(values);
            return mat;
        }

        private static Mat CreateIntensityGrayImage()
        {
            var mat = new Mat(8, 8, MatType.CV_8UC1);
            var values = new byte[8 * 8];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (byte)(16 + i * 3);
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static Mat CreateFuzzyFloatImage()
        {
            var mat = new Mat(8, 8, MatType.CV_32FC1);
            var values = new float[8 * 8];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = (float)(0.1 + (i % 11) * 0.05);
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static Mat CreateFuzzyMask()
        {
            var mat = new Mat(8, 8, MatType.CV_8UC1);
            var values = new byte[8 * 8];
            for (int y = 3; y < 5; y++)
            {
                for (int x = 3; x < 5; x++)
                {
                    values[(y * 8) + x] = 255;
                }
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static Mat CreateRegGrayImage()
        {
            var mat = new Mat(32, 32, MatType.CV_8UC1);
            var values = new byte[32 * 32];
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    values[(y * 32) + x] = (byte)(x >= 8 && x < 24 && y >= 8 && y < 24 ? 220 : 30);
                }
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static Mat CreateSurfaceMatchingPointCloud()
        {
            var cloud = new Mat(8, 6, MatType.CV_32FC1);
            cloud.CopyFrom<float>(new float[]
            {
                0.0F, 0.0F, 0.0F, 0.0F, 0.0F, 1.0F,
                1.0F, 0.0F, 0.0F, 0.0F, 0.0F, 1.0F,
                0.0F, 1.0F, 0.0F, 0.0F, 0.0F, 1.0F,
                1.0F, 1.0F, 0.0F, 0.0F, 0.0F, 1.0F,
                0.0F, 0.0F, 1.0F, 0.0F, 0.0F, 1.0F,
                1.0F, 0.0F, 1.0F, 0.0F, 0.0F, 1.0F,
                0.0F, 1.0F, 1.0F, 0.0F, 0.0F, 1.0F,
                1.0F, 1.0F, 1.0F, 0.0F, 0.0F, 1.0F
            });
            return cloud;
        }

        private static Mat CreateRapidEdgeImage()
        {
            var image = new Mat(64, 64, MatType.CV_8UC1, new Scalar(0));
            for (int y = 16; y < 48; y++)
            {
                image.SetValue((y * 64) + 16, (byte)255);
                image.SetValue((y * 64) + 47, (byte)255);
            }

            for (int x = 16; x < 48; x++)
            {
                image.SetValue((16 * 64) + x, (byte)255);
                image.SetValue((47 * 64) + x, (byte)255);
            }

            return image;
        }

        private static Mat CreateRapidMeshPoints()
        {
            var mat = new Mat(4, 1, MatType.CV_32FC3);
            mat.CopyFrom<float>(new float[]
            {
                -1.0F, -1.0F, 0.0F,
                1.0F, -1.0F, 0.0F,
                1.0F, 1.0F, 0.0F,
                -1.0F, 1.0F, 0.0F
            });
            return mat;
        }

        private static Mat CreateRapidMeshTriangles()
        {
            var mat = new Mat(2, 1, MatType.CV_32SC3);
            mat.CopyFrom<int>(new[] { 0, 1, 2, 0, 2, 3 });
            return mat;
        }

        private static Mat CreateRapidCameraMatrix()
        {
            var mat = new Mat(3, 3, MatType.CV_64FC1);
            mat.CopyFrom<double>(new double[] { 60.0, 0.0, 32.0, 0.0, 60.0, 32.0, 0.0, 0.0, 1.0 });
            return mat;
        }

        private static Mat CreateRapidPoseVector(double x, double y, double z)
        {
            var mat = new Mat(3, 1, MatType.CV_64FC1);
            mat.CopyFrom<double>(new[] { x, y, z });
            return mat;
        }

        private static Mat CreateRapidProjectedPoints()
        {
            var mat = new Mat(4, 1, MatType.CV_32FC2);
            mat.CopyFrom<float>(new float[] { 12.0F, 12.0F, 52.0F, 12.0F, 52.0F, 52.0F, 12.0F, 52.0F });
            return mat;
        }

        private static Mat CreateSmallBgrImage(int width, int height)
        {
            var mat = new Mat(height, width, MatType.CV_8UC3);
            var values = new byte[width * height * 3];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = ((y * width) + x) * 3;
                    bool topLeft = x < width / 2 && y < height / 2;
                    bool topRight = x >= width / 2 && y < height / 2;
                    bool bottomLeft = x < width / 2 && y >= height / 2;
                    values[offset] = (byte)(topLeft ? 220 : bottomLeft ? 50 : 90);
                    values[offset + 1] = (byte)(topRight ? 220 : bottomLeft ? 180 : 70);
                    values[offset + 2] = (byte)(bottomLeft ? 220 : topRight ? 50 : 80);
                }
            }

            mat.CopyFrom(values);
            return mat;
        }

        private static Mat CreateAlphaMatColorImage()
        {
            var image = new Mat(24, 24, MatType.CV_8UC3, new Scalar(10, 20, 30));
            ImgProcCv2.Rectangle(image, new Rect(6, 6, 12, 12), new Scalar(210, 190, 120), -1);
            return image;
        }

        private static Mat CreateAlphaMatTrimap()
        {
            var trimap = new Mat(24, 24, MatType.CV_8UC1, new Scalar(0));
            ImgProcCv2.Rectangle(trimap, new Rect(4, 4, 16, 16), new Scalar(128), -1);
            ImgProcCv2.Rectangle(trimap, new Rect(8, 8, 8, 8), new Scalar(255), -1);
            return trimap;
        }

        private static Mat CreateBioInspiredShiftedImage()
        {
            var image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(25, 35, 45));
            ImgProcCv2.Rectangle(image, new Rect(10, 8, 12, 12), new Scalar(210, 80, 120), -1);
            ImgProcCv2.Circle(image, new Point(24, 22), 5, new Scalar(40, 220, 120), -1);
            return image;
        }

        private static Mat CreateXStereoLeftImage()
        {
            var image = new Mat(32, 48, MatType.CV_8UC1, new Scalar(30));
            ImgProcCv2.Rectangle(image, new Rect(16, 8, 16, 14), new Scalar(220), -1);
            ImgProcCv2.Circle(image, new Point(32, 24), 5, new Scalar(130), -1);
            return image;
        }

        private static Mat CreateXStereoRightImage()
        {
            var image = new Mat(32, 48, MatType.CV_8UC1, new Scalar(30));
            ImgProcCv2.Rectangle(image, new Rect(13, 8, 16, 14), new Scalar(220), -1);
            ImgProcCv2.Circle(image, new Point(29, 24), 5, new Scalar(130), -1);
            return image;
        }

        private static Mat CreateMLSamples()
        {
            var samples = new Mat(6, 2, MatType.CV_32FC1);
            samples.CopyFrom<float>(new float[]
            {
                0.0F, 0.0F,
                0.0F, 1.0F,
                1.0F, 0.0F,
                5.0F, 5.0F,
                5.0F, 6.0F,
                6.0F, 5.0F
            });
            return samples;
        }

        private static Mat CreateMLResponses()
        {
            var responses = new Mat(6, 1, MatType.CV_32SC1);
            responses.CopyFrom<int>(new int[] { 0, 0, 0, 1, 1, 1 });
            return responses;
        }

        private static Mat CreateMLAnnResponses()
        {
            var responses = new Mat(6, 1, MatType.CV_32FC1);
            responses.CopyFrom<float>(new float[] { 0.0F, 0.1F, 0.1F, 1.0F, 1.1F, 1.1F });
            return responses;
        }

        private static Mat CreateImgHashImage()
        {
            var image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(16, 32, 48));
            ImgProcCv2.Rectangle(image, new Rect(4, 4, 10, 10), new Scalar(220, 40, 30), -1);
            ImgProcCv2.Circle(image, new Point(23, 22), 6, new Scalar(30, 200, 120), -1);
            return image;
        }

        private static Mat CreateXImgProcColorImage()
        {
            var image = new Mat(16, 16, MatType.CV_8UC3, new Scalar(24, 48, 72));
            ImgProcCv2.Rectangle(image, new Rect(2, 2, 6, 6), new Scalar(220, 40, 30), -1);
            ImgProcCv2.Rectangle(image, new Rect(8, 2, 6, 6), new Scalar(30, 200, 80), -1);
            ImgProcCv2.Circle(image, new Point(8, 12), 3, new Scalar(40, 80, 220), -1);
            return image;
        }

        private static Mat CreateXImgProcDisparityMap()
        {
            var disparity = new Mat(16, 16, MatType.CV_16SC1, new Scalar(0));
            short[] values = new short[16 * 16];
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    values[(y * 16) + x] = (short)((x + 1) * 16);
                }
            }

            disparity.CopyFrom(values);
            return disparity;
        }

        private static Point2f[] CreateXImgProcFromPoints()
        {
            return new[]
            {
                new Point2f(2.0F, 2.0F),
                new Point2f(13.0F, 2.0F),
                new Point2f(2.0F, 13.0F),
                new Point2f(13.0F, 13.0F)
            };
        }

        private static Point2f[] CreateXImgProcToPoints()
        {
            return new[]
            {
                new Point2f(3.0F, 2.0F),
                new Point2f(14.0F, 2.0F),
                new Point2f(3.0F, 13.0F),
                new Point2f(14.0F, 13.0F)
            };
        }

        private static Mat CreateXImgProcContourPointMat()
        {
            return Calib3DCv2.ToPointMat(new[]
            {
                new Point2f(0.0F, 0.0F),
                new Point2f(16.0F, 0.0F),
                new Point2f(16.0F, 16.0F),
                new Point2f(0.0F, 16.0F)
            });
        }

        private static Mat CreateXImgProcComplexImage()
        {
            var complex = new Mat(4, 4, MatType.CV_32FC2, new Scalar(0));
            float[] values = new float[4 * 4 * 2];
            for (int i = 0; i < values.Length; i += 2)
            {
                values[i] = (i / 2) + 1.0F;
                values[i + 1] = 0.5F;
            }

            complex.CopyFrom(values);
            return complex;
        }

        private static string RunFastBilateralSolverStatus(Mat gray)
        {
            return gray.Rows + "x" + gray.Cols + " skipped";
        }

        private static Mat CreateOptFlowFrame(int offset)
        {
            var frame = new Mat(24, 24, MatType.CV_8UC3, new Scalar(20, 40, 60));
            ImgProcCv2.Rectangle(frame, new Rect(4 + offset, 5, 8, 9), new Scalar(200, 30, 80), -1);
            ImgProcCv2.Circle(frame, new Point(16 + offset / 2, 16), 3, new Scalar(40, 190, 120), -1);
            return frame;
        }

        private static Mat CreateFaceImage(byte value)
        {
            var image = new Mat(32, 32, MatType.CV_8UC1, new Scalar(value));
            ImgProcCv2.Circle(image, new Point(12, 12), 4, new Scalar(220 - value / 2), -1);
            ImgProcCv2.Circle(image, new Point(22, 12), 4, new Scalar(220 - value / 2), -1);
            ImgProcCv2.Rectangle(image, new Rect(11, 22, 12, 3), new Scalar(20 + value / 2), -1);
            return image;
        }

        private static FacemarkLBFParams CreateTinyLbfParams()
        {
            return new FacemarkLBFParams
            {
                NLandmarks = 68,
                InitialShapeCount = 1,
                StageCount = 1,
                TreeCount = 1,
                TreeDepth = 2,
                FeatureCounts = new[] { 8 },
                RadiusValues = new[] { 0.2 },
                CascadeFace = FindFaceCascadePath() ?? string.Empty,
                SaveModel = false,
                Verbose = false,
                Seed = 123
            };
        }

        private static string? FindFaceCascadePath()
        {
            string? configured = GetEnvironmentVariable(FaceCascadeVariable, CompatibilityFaceCascadeAlias);
            if (!string.IsNullOrWhiteSpace(configured) && File.Exists(configured))
            {
                return configured;
            }

            string? fromCurrent = FindFaceCascadePathFromRoot(Directory.GetCurrentDirectory());
            if (fromCurrent != null)
            {
                return fromCurrent;
            }

            return FindFaceCascadePathFromRoot(AppContext.BaseDirectory);
        }

        private static string? FindFaceCascadePathFromRoot(string root)
        {
            var directory = new DirectoryInfo(root);
            for (int i = 0; i < 8 && directory != null; i++)
            {
                string candidate = Path.Combine(
                    directory.FullName,
                    "artifacts",
                    "opencv-install",
                    FactualOpenCvInstallCacheName,
                    "etc",
                    "haarcascades",
                    "haarcascade_frontalface_default.xml");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private static Point2f[] CreateFaceLandmarks()
        {
            var points = new Point2f[68];
            for (int i = 0; i < points.Length; i++)
            {
                float angle = (float)(i * Math.PI * 2.0 / points.Length);
                points[i] = new Point2f(16.0F + 8.0F * (float)Math.Cos(angle), 16.0F + 9.0F * (float)Math.Sin(angle));
            }

            return points;
        }

        private static Mat CreateSaliencyImage()
        {
            var image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(20, 30, 40));
            ImgProcCv2.Rectangle(image, new Rect(6, 6, 10, 12), new Scalar(230, 30, 80), -1);
            ImgProcCv2.Circle(image, new Point(23, 22), 5, new Scalar(40, 220, 120), -1);
            return image;
        }

        private static Mat CreateMotionSaliencyImage()
        {
            var image = new Mat(32, 32, MatType.CV_8UC1, new Scalar(40));
            ImgProcCv2.Rectangle(image, new Rect(6, 6, 10, 12), new Scalar(220), -1);
            ImgProcCv2.Circle(image, new Point(23, 22), 5, new Scalar(120), -1);
            return image;
        }

        private static string RunStitchingSummary()
        {
            using (StitcherObject stitcher = StitcherObject.Create())
            using (Mat pano = new Mat())
            {
                stitcher.RegistrationResol = 0.2;
                stitcher.SeamEstimationResol = 0.1;
                stitcher.CompositingResol = -1.0;
                stitcher.PanoConfidenceThresh = 0.5;
                stitcher.WaveCorrection = true;
                stitcher.WaveCorrectKind = OpenCvSharp.Stitching.WaveCorrectKind.Auto;

                Mat[] images = LoadStitchingImagesFromEnvironment();
                bool ownsImages = true;
                if (images.Length == 0)
                {
                    images = CreateSyntheticStitchingImages();
                }

                try
                {
                    OpenCvSharp.Stitching.StitcherStatus status = stitcher.Stitch(images, pano);
                    return "Stitching status=" + status
                        + ", images=" + images.Length
                        + ", pano=" + pano.Rows + "x" + pano.Cols
                        + ", component=" + stitcher.GetComponent().Length
                        + ", cameras=" + DisposeAndCount(stitcher.GetCameras())
                        + ", workScale=" + stitcher.WorkScale
                        + ", " + RunExposureCompensationSummary()
                        + ", " + RunPyRotationWarperSummary()
                        + ", " + RunBlenderSummary()
                        + ", " + RunFeaturesMatcherSummary();
                }
                finally
                {
                    if (ownsImages)
                    {
                        DisposeAll(images);
                    }
                }
            }
        }

        private static string RunExposureCompensationSummary()
        {
            using (var first = new Mat(24, 24, MatType.CV_8UC3, new Scalar(40, 40, 40)))
            using (var second = new Mat(24, 24, MatType.CV_8UC3, new Scalar(80, 80, 80)))
            using (var firstMask = new Mat(24, 24, MatType.CV_8UC1, new Scalar(255)))
            using (var secondMask = new Mat(24, 24, MatType.CV_8UC1, new Scalar(255)))
            using (var compensator = new OpenCvSharp.Stitching.GainCompensator())
            {
                var corners = new[] { new Point(0, 0), new Point(0, 0) };
                compensator.Feed(corners, new[] { first, second }, new[] { firstMask, secondMask });
                compensator.Apply(0, corners[0], first, firstMask);
                Mat[] gains = compensator.GetMatGains();
                try
                {
                    return "exposureGains=" + gains.Length
                        + ", output=" + first.Rows + "x" + first.Cols
                        + ", type=" + first.Type;
                }
                finally
                {
                    DisposeAll(gains);
                }
            }
        }

        private static string RunPyRotationWarperSummary()
        {
            using (var camera = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var rotation = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var source = new Mat(4, 5, MatType.CV_8UC1, new Scalar(37)))
            using (var destination = new Mat())
            using (var warper = new OpenCvSharp.Stitching.PyRotationWarper("plane", 1.0f))
            {
                Point2f projected = warper.WarpPoint(new Point2f(2.0f, 3.0f), camera, rotation);
                Rect roi = warper.WarpRoi(new Size(source.Cols, source.Rows), camera, rotation);
                Point topLeft = warper.Warp(source, camera, rotation, InterpolationFlags.Nearest, BorderTypes.Replicate, destination);
                return "warperPoint=" + projected.X.ToString("0.0", CultureInfo.InvariantCulture)
                    + "," + projected.Y.ToString("0.0", CultureInfo.InvariantCulture)
                    + ", warperRoi=" + roi.Width + "x" + roi.Height
                    + ", topLeft=" + topLeft.X + "," + topLeft.Y
                    + ", warped=" + destination.Cols + "x" + destination.Rows;
            }
        }

        private static string RunBlenderSummary()
        {
            using (var image = new Mat(8, 8, MatType.CV_8UC3, new Scalar(32, 48, 64)))
            using (var mask = new Mat(8, 8, MatType.CV_8UC1, new Scalar(255)))
            using (var destination = new Mat())
            using (var destinationMask = new Mat())
            using (var blender = new OpenCvSharp.Stitching.MultiBandBlender(tryGpu: true, numberOfBands: 2))
            {
                blender.Prepare(new Rect(0, 0, image.Cols, image.Rows));
                blender.Feed(image, mask, new Point(0, 0));
                blender.Blend(destination, destinationMask);
                Mat[] pyramid = OpenCvSharp.Stitching.Blender.CreateLaplacePyramid(image, 1);
                try
                {
                    OpenCvSharp.Stitching.Blender.RestoreImageFromLaplacePyramid(pyramid);
                    return "blender=" + destination.Cols + "x" + destination.Rows
                        + ", blenderType=" + destination.Type
                        + ", blenderMask=" + destinationMask.Type
                        + ", pyramidLevels=" + pyramid.Length;
                }
                finally
                {
                    DisposeAll(pyramid);
                }
            }
        }

        private static string RunStitchingDetailSummary()
        {
            var corners = new[] { new Point(-2, 3), new Point(2, 1) };
            var sizes = new[] { new Size(6, 4), new Size(5, 7) };
            Rect union = OpenCvSharp.Stitching.StitchingUtilities.ResultRoi(corners, sizes);
            Rect intersection = OpenCvSharp.Stitching.StitchingUtilities.ResultRoiIntersection(corners, sizes);
            using (var seamImage = new Mat(4, 6, MatType.CV_32FC3, new Scalar(10, 20, 30)))
            using (var seamMask = new Mat(4, 6, MatType.CV_8UC1, new Scalar(255)))
            using (OpenCvSharp.Stitching.SeamFinder seamFinder =
                OpenCvSharp.Stitching.SeamFinder.CreateDefault(OpenCvSharp.Stitching.SeamFinderType.None))
            using (var timelapseImage = new Mat(2, 3, MatType.CV_16SC3, new Scalar(7, 11, 13)))
            using (var timelapseMask = new Mat(2, 3, MatType.CV_8UC1, new Scalar(255)))
            using (OpenCvSharp.Stitching.Timelapser timelapser =
                OpenCvSharp.Stitching.Timelapser.CreateDefault(OpenCvSharp.Stitching.TimelapserType.AsIs))
            using (var camera = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var rotation = Mat.Eye(3, 3, MatType.CV_32FC1))
            using (var projector = new OpenCvSharp.Stitching.SphericalProjector(2F, camera, rotation))
            {
                seamFinder.Find(new[] { seamImage }, new[] { corners[0] }, new[] { seamMask });
                timelapser.Initialize(new[] { new Point(-1, 2) }, new[] { new Size(3, 2) });
                timelapser.Process(timelapseImage, timelapseMask, new Point(-1, 2));
                using (Mat destination = timelapser.GetDestination())
                {
                    Point2f spherical = projector.MapForward(new Point2f(0, 0));
                    return "detailUnion=" + union.Width + "x" + union.Height
                        + ", detailIntersection=" + intersection.Width + "x" + intersection.Height
                        + ", seamMaskMean=" + CoreCv2.Mean(seamMask).V0.ToString("0", CultureInfo.InvariantCulture)
                        + ", timelapse=" + destination.Cols + "x" + destination.Rows
                        + ", spherical=" + spherical.X.ToString("0.0", CultureInfo.InvariantCulture)
                        + "," + spherical.Y.ToString("0.0", CultureInfo.InvariantCulture);
                }
            }
        }

        private static string RunFeaturesMatcherSummary()
        {
            using (var first = new Mat(96, 96, MatType.CV_8UC1, new Scalar(0)))
            using (var second = new Mat(96, 96, MatType.CV_8UC1, new Scalar(0)))
            using (ORB orb = ORB.Create(maxFeatures: 120))
            using (var matcher = new OpenCvSharp.Stitching.BestOf2NearestMatcher(matchConfidence: 0.8f))
            {
                ImgProcCv2.Rectangle(first, new Rect(12, 12, 60, 60), new Scalar(255), 3);
                ImgProcCv2.Line(first, new Point(8, 84), new Point(84, 8), new Scalar(200), 2);
                ImgProcCv2.Circle(first, new Point(68, 68), 12, new Scalar(180), 2);
                ImgProcCv2.Rectangle(second, new Rect(16, 15, 60, 60), new Scalar(255), 3);
                ImgProcCv2.Line(second, new Point(12, 87), new Point(88, 11), new Scalar(200), 2);
                ImgProcCv2.Circle(second, new Point(72, 71), 12, new Scalar(180), 2);

                OpenCvSharp.Stitching.ImageFeatures[] features = OpenCvSharp.Stitching.ImageFeatures.Compute(
                    orb, new[] { first, second });
                OpenCvSharp.Stitching.MatchesInfo[] matches = Array.Empty<OpenCvSharp.Stitching.MatchesInfo>();
                try
                {
                    matches = matcher.Match(features);
                    return "featureKeypoints=" + features[0].Keypoints.Length + "," + features[1].Keypoints.Length
                        + ", pairwise=" + matches.Length
                        + ", forwardMatches=" + matches[1].Matches.Length
                        + ", inliers=" + matches[1].NumberOfInliers
                        + ", " + RunMotionEstimatorSummary(features, matches);
                }
                finally
                {
                    foreach (OpenCvSharp.Stitching.MatchesInfo match in matches) match.Dispose();
                    foreach (OpenCvSharp.Stitching.ImageFeatures feature in features) feature.Dispose();
                }
            }
        }

        private static string RunMotionEstimatorSummary(
            OpenCvSharp.Stitching.ImageFeatures[] features,
            OpenCvSharp.Stitching.MatchesInfo[] matches)
        {
            var initial = new OpenCvSharp.Stitching.StitcherCameraParams[features.Length];
            var rotations = new Mat[features.Length];
            var translations = new Mat[features.Length];
            OpenCvSharp.Stitching.StitcherCameraParams[] adjusted = Array.Empty<OpenCvSharp.Stitching.StitcherCameraParams>();
            OpenCvSharp.Stitching.ImageFeatures[] componentFeatures = Array.Empty<OpenCvSharp.Stitching.ImageFeatures>();
            OpenCvSharp.Stitching.MatchesInfo[] componentMatches = Array.Empty<OpenCvSharp.Stitching.MatchesInfo>();
            try
            {
                for (int i = 0; i < features.Length; ++i)
                {
                    rotations[i] = Mat.Eye(3, 3, MatType.CV_32FC1);
                    translations[i] = new Mat(3, 1, MatType.CV_32FC1, new Scalar(0));
                    initial[i] = new OpenCvSharp.Stitching.StitcherCameraParams(
                        500, 1, 48, 48, rotations[i], translations[i]);
                }

                using (var adjuster = new OpenCvSharp.Stitching.NoBundleAdjuster())
                {
                    adjuster.ConfidenceThreshold = 0.0;
                    bool succeeded = adjuster.Apply(features, matches, initial, out adjusted);
                    Mat[] correctedRotations = adjusted.Select(camera => camera.Rotation).ToArray();
                    OpenCvSharp.Stitching.StitchingMotion.WaveCorrect(
                        correctedRotations, OpenCvSharp.Stitching.WaveCorrectKind.Horizontal);
                    using (Mat intrinsic = adjusted[0].GetCameraMatrix())
                    {
                        string graph = OpenCvSharp.Stitching.StitchingMotion.MatchesGraphAsString(
                            new[] { "left.png", "right.png" }, matches, 0.0f);
                        int[] component = OpenCvSharp.Stitching.StitchingMotion.LeaveBiggestComponent(
                            features, matches, 0.0f, out componentFeatures, out componentMatches);
                        return "motionAdjusted=" + succeeded
                            + ", K=" + intrinsic.Rows + "x" + intrinsic.Cols
                            + ", graphBytes=" + Encoding.UTF8.GetByteCount(graph)
                            + ", largestComponent=" + component.Length;
                    }
                }
            }
            finally
            {
                foreach (OpenCvSharp.Stitching.MatchesInfo match in componentMatches) match.Dispose();
                foreach (OpenCvSharp.Stitching.ImageFeatures feature in componentFeatures) feature.Dispose();
                foreach (OpenCvSharp.Stitching.StitcherCameraParams camera in adjusted)
                {
                    camera.Rotation.Dispose();
                    camera.Translation.Dispose();
                }
                DisposeAll(translations);
                DisposeAll(rotations);
            }
        }

        private static Mat[] LoadStitchingImagesFromEnvironment()
        {
            string imageList = GetEnvironmentVariable(StitchingImagesVariable, CompatibilityStitchingImagesAlias) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(imageList))
            {
                return Array.Empty<Mat>();
            }

            string[] paths = imageList.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            var images = new Mat[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                string path = paths[i].Trim();
                images[i] = ImgCodecsCv2.ImRead(path, ImreadModes.Color);
            }

            return images;
        }

        private static Mat[] CreateSyntheticStitchingImages()
        {
            var first = new Mat(32, 48, MatType.CV_8UC3, new Scalar(0, 0, 0));
            var second = new Mat(32, 48, MatType.CV_8UC3, new Scalar(0, 0, 0));
            ImgProcCv2.Rectangle(first, new Rect(8, 8, 16, 16), new Scalar(255, 255, 255), -1);
            ImgProcCv2.Rectangle(second, new Rect(12, 8, 16, 16), new Scalar(255, 255, 255), -1);
            return new[] { first, second };
        }

        private static string? GetEnvironmentVariable(string neutralName, string compatibilityAliasName)
        {
            string? neutralValue = Environment.GetEnvironmentVariable(neutralName);
            return string.IsNullOrEmpty(neutralValue) ? Environment.GetEnvironmentVariable(compatibilityAliasName) : neutralValue;
        }

        private static bool IsEnvironmentFlagEnabled(string neutralName, string compatibilityAliasName)
        {
            string? value = GetEnvironmentVariable(neutralName, compatibilityAliasName);
            return string.Equals(value, "1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
        }

        private static int DisposeAndCount(OpenCvSharp.Stitching.StitcherCameraParams[] cameras)
        {
            try
            {
                return cameras.Length;
            }
            finally
            {
                for (int i = 0; i < cameras.Length; i++)
                {
                    cameras[i].Rotation.Dispose();
                    cameras[i].Translation.Dispose();
                }
            }
        }

        private static void DisposeAll(Mat[] mats)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i].Dispose();
            }
        }
    }
}
