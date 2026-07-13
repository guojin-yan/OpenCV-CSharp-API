using System;
using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    internal static partial class NativeMethods
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct TrackingRectNative
        {
            internal int X;
            internal int Y;
            internal int Width;
            internal int Height;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TrackingRect2dNative
        {
            internal double X;
            internal double Y;
            internal double Width;
            internal double Height;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TrackingKcfParamsNative
        {
            internal float DetectThresh;
            internal float Sigma;
            internal float LambdaValue;
            internal float InterpFactor;
            internal float OutputSigmaFactor;
            internal float PcaLearningRate;
            internal int Resize;
            internal int SplitCoeff;
            internal int WrapKernel;
            internal int CompressFeature;
            internal int MaxPatchSize;
            internal int CompressedSize;
            internal int DescPca;
            internal int DescNpca;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TrackingCsrtParamsNative
        {
            internal int UseHog;
            internal int UseColorNames;
            internal int UseGray;
            internal int UseRgb;
            internal int UseChannelWeights;
            internal int UseSegmentation;
            internal IntPtr WindowFunction;
            internal float KaiserAlpha;
            internal float ChebAttenuation;
            internal float TemplateSize;
            internal float GslSigma;
            internal float HogOrientations;
            internal float HogClip;
            internal float Padding;
            internal float FilterLr;
            internal float WeightsLr;
            internal int NumHogChannelsUsed;
            internal int AdmmIterations;
            internal int HistogramBins;
            internal float HistogramLr;
            internal int BackgroundRatio;
            internal int NumberOfScales;
            internal float ScaleSigmaFactor;
            internal float ScaleModelMaxArea;
            internal float ScaleLr;
            internal float ScaleStep;
            internal float PsrThreshold;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TrackingMilParamsNative
        {
            internal float SamplerInitInRadius;
            internal float SamplerSearchWinSize;
            internal int SamplerInitMaxNegNum;
            internal float SamplerTrackInRadius;
            internal int SamplerTrackMaxPosNum;
            internal int SamplerTrackMaxNegNum;
            internal int FeatureSetNumFeatures;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct TrackingMedianFlowParamsNative
        {
            internal int PointsInGrid;
            internal int WinWidth;
            internal int WinHeight;
            internal int MaxLevel;
            internal int CriteriaType;
            internal int CriteriaMaxCount;
            internal double CriteriaEpsilon;
            internal int WinWidthNcc;
            internal int WinHeightNcc;
            internal double MaxMedianLengthOfDisplacementDifference;
        }
    }
}
