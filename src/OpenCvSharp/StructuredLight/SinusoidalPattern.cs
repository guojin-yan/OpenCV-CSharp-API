using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.StructuredLight
{
    /// <summary>
    /// Sinusoidal structured-light pattern generator.
    /// 正弦结构光图案生成器。
    /// </summary>
    public sealed class SinusoidalPattern : StructuredLightPattern
    {
        private SinusoidalPattern(NativeStructuredLightPatternHandle handle)
            : base(handle)
        {
        }

        /// <summary>
        /// Creates a sinusoidal pattern with OpenCV default parameters.
        /// 使用 OpenCV 默认参数创建正弦图案。
        /// </summary>
        public static SinusoidalPattern Create()
        {
            return Create(SinusoidalPatternParams.Default());
        }

        /// <summary>
        /// Creates a sinusoidal pattern.
        /// 创建正弦图案。
        /// </summary>
        public static unsafe SinusoidalPattern Create(SinusoidalPatternParams parameters)
        {
            ValidateNotNull(parameters, nameof(parameters));
            parameters.Validate();

            NativeStructuredLightPoint2f[] markers = ToNativeMarkers(parameters.GetMarkersLocationSnapshot());
            fixed (NativeStructuredLightPoint2f* markersPtr = markers)
            {
                NativeException.ThrowIfError(NativeMethods.StructuredLightSinusoidalPatternCreate(
                    parameters.Width,
                    parameters.Height,
                    parameters.NbrOfPeriods,
                    parameters.ShiftValue,
                    (int)parameters.Method,
                    parameters.NbrOfPixelsBetweenMarkers,
                    parameters.Horizontal ? 1 : 0,
                    parameters.SetMarkers ? 1 : 0,
                    markersPtr,
                    markers.Length,
                    out IntPtr nativeHandle));
                return new SinusoidalPattern(NativeStructuredLightPatternHandle.FromNativePointer(nativeHandle));
            }
        }

        /// <summary>
        /// Creates a sinusoidal pattern.
        /// 创建正弦图案。
        /// </summary>
        public static SinusoidalPattern Create(int width, int height, int nbrOfPeriods = 20, SinusoidalPatternMethod method = SinusoidalPatternMethod.Ftp)
        {
            return Create(new SinusoidalPatternParams
            {
                Width = width,
                Height = height,
                NbrOfPeriods = nbrOfPeriods,
                Method = method
            });
        }

        /// <summary>
        /// Computes a wrapped phase map from captured sinusoidal pattern images.
        /// 从采集到的正弦图案图像计算包裹相位图。
        /// </summary>
        public unsafe void ComputePhaseMap(Mat[] patternImages, Mat wrappedPhaseMap, Mat shadowMask, Mat? fundamental = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(wrappedPhaseMap, nameof(wrappedPhaseMap));
            ValidateNotNull(shadowMask, nameof(shadowMask));
            IntPtr[] handles = ToNativeHandles(patternImages, nameof(patternImages));
            fixed (IntPtr* handlesPtr = handles)
            {
                NativeException.ThrowIfError(NativeMethods.StructuredLightSinusoidalPatternComputePhaseMap(
                    NativeHandle,
                    handlesPtr,
                    handles.Length,
                    wrappedPhaseMap.NativeHandle,
                    shadowMask.NativeHandle,
                    OptionalHandle(fundamental)));
            }
        }

        /// <summary>
        /// Computes a wrapped phase map from captured sinusoidal pattern images.
        /// 从采集到的正弦图案图像计算包裹相位图。
        /// </summary>
        public Mat ComputePhaseMap(Mat[] patternImages, Mat shadowMask, Mat? fundamental = null)
        {
            var result = new Mat();
            try
            {
                ComputePhaseMap(patternImages, result, shadowMask, fundamental);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Unwraps a wrapped sinusoidal phase map.
        /// 展开包裹的正弦相位图。
        /// </summary>
        public void UnwrapPhaseMap(Mat wrappedPhaseMap, Mat unwrappedPhaseMap, Size cameraSize, Mat? shadowMask = null)
        {
            ThrowIfDisposed();
            ValidateNotNull(wrappedPhaseMap, nameof(wrappedPhaseMap));
            ValidateNotNull(unwrappedPhaseMap, nameof(unwrappedPhaseMap));
            NativeException.ThrowIfError(NativeMethods.StructuredLightSinusoidalPatternUnwrapPhaseMap(
                NativeHandle,
                wrappedPhaseMap.NativeHandle,
                unwrappedPhaseMap.NativeHandle,
                cameraSize.Width,
                cameraSize.Height,
                OptionalHandle(shadowMask)));
        }

        /// <summary>
        /// Unwraps a wrapped sinusoidal phase map and returns the result matrix.
        /// 展开包裹的正弦相位图并返回结果矩阵。
        /// </summary>
        public Mat UnwrapPhaseMap(Mat wrappedPhaseMap, Size cameraSize, Mat? shadowMask = null)
        {
            var result = new Mat();
            try
            {
                UnwrapPhaseMap(wrappedPhaseMap, result, cameraSize, shadowMask);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Computes the data modulation term from captured sinusoidal pattern images.
        /// 从采集到的正弦图案图像计算数据调制项。
        /// </summary>
        public unsafe void ComputeDataModulationTerm(Mat[] patternImages, Mat dataModulationTerm, Mat shadowMask)
        {
            ThrowIfDisposed();
            ValidateNotNull(dataModulationTerm, nameof(dataModulationTerm));
            ValidateNotNull(shadowMask, nameof(shadowMask));
            IntPtr[] handles = ToNativeHandles(patternImages, nameof(patternImages));
            fixed (IntPtr* handlesPtr = handles)
            {
                NativeException.ThrowIfError(NativeMethods.StructuredLightSinusoidalPatternComputeDataModulationTerm(
                    NativeHandle,
                    handlesPtr,
                    handles.Length,
                    dataModulationTerm.NativeHandle,
                    shadowMask.NativeHandle));
            }
        }

        /// <summary>
        /// Computes the data modulation term from captured sinusoidal pattern images.
        /// 从采集到的正弦图案图像计算数据调制项。
        /// </summary>
        public Mat ComputeDataModulationTerm(Mat[] patternImages, Mat shadowMask)
        {
            var result = new Mat();
            try
            {
                ComputeDataModulationTerm(patternImages, result, shadowMask);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        private static NativeStructuredLightPoint2f[] ToNativeMarkers(Point2f[] markers)
        {
            var result = new NativeStructuredLightPoint2f[markers.Length];
            for (int i = 0; i < markers.Length; i++)
            {
                result[i] = new NativeStructuredLightPoint2f { X = markers[i].X, Y = markers[i].Y };
            }

            return result;
        }
    }
}
