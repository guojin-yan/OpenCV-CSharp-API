using System.Runtime.InteropServices;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeBioInspiredRetinaParvoParameters
    {
        internal int ColorMode;
        internal int NormaliseOutput;
        internal float PhotoreceptorsLocalAdaptationSensitivity;
        internal float PhotoreceptorsTemporalConstant;
        internal float PhotoreceptorsSpatialConstant;
        internal float HorizontalCellsGain;
        internal float HcellsTemporalConstant;
        internal float HcellsSpatialConstant;
        internal float GanglionCellsSensitivity;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeBioInspiredRetinaMagnoParameters
    {
        internal int NormaliseOutput;
        internal float ParasolCellsBeta;
        internal float ParasolCellsTau;
        internal float ParasolCellsK;
        internal float AmacrinCellsTemporalCutFrequency;
        internal float V0CompressionParameter;
        internal float LocalAdaptIntegrationTau;
        internal float LocalAdaptIntegrationK;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeBioInspiredSegmentationParameters
    {
        internal float ThresholdOn;
        internal float ThresholdOff;
        internal float LocalEnergyTemporalConstant;
        internal float LocalEnergySpatialConstant;
        internal float NeighborhoodEnergyTemporalConstant;
        internal float NeighborhoodEnergySpatialConstant;
        internal float ContextEnergyTemporalConstant;
        internal float ContextEnergySpatialConstant;
    }
}
