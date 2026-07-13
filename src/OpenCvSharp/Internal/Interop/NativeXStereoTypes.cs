using System.Runtime.InteropServices;

namespace OpenCvSharp.Internal.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeXStereoMatchQuasiDense
    {
        internal int P0X;
        internal int P0Y;
        internal int P1X;
        internal int P1Y;
        internal float Corr;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeXStereoPropagationParameters
    {
        internal int CorrWinSizeX;
        internal int CorrWinSizeY;
        internal int BorderX;
        internal int BorderY;
        internal float CorrelationThreshold;
        internal float TextureThreshold;
        internal int NeighborhoodSize;
        internal int DisparityGradient;
        internal int LkTemplateSize;
        internal int LkPyrLvl;
        internal int LkTermParam1;
        internal float LkTermParam2;
        internal float GftQualityThres;
        internal int GftMinSeperationDist;
        internal int GftMaxNumFeatures;
    }
}
