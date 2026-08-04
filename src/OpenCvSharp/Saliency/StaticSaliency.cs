using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Saliency
{
    /// <summary>
    /// Base class for static saliency algorithms.
    /// 静态显著性算法基类。
    /// </summary>
    public class StaticSaliency : Saliency
    {
        internal StaticSaliency(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>
        /// Computes a binary map from a saliency map.
        /// 从显著性图计算二值图。
        /// </summary>
        public bool ComputeBinaryMap(Mat saliencyMap, Mat binaryMap)
        {
            ThrowIfDisposed();
            ValidateNotNull(saliencyMap, nameof(saliencyMap));
            ValidateNotNull(binaryMap, nameof(binaryMap));
            ValidateSaliencyMapType(saliencyMap, nameof(saliencyMap));
            NativeException.ThrowIfError(NativeMethods.SaliencyStaticComputeBinaryMap(NativeHandle, saliencyMap.NativeHandle, binaryMap.NativeHandle, out int result));
            return result != 0;
        }

        /// <summary>
        /// Computes and returns a binary map from a saliency map.
        /// 从显著性图计算并返回二值图。
        /// </summary>
        public Mat ComputeBinaryMap(Mat saliencyMap)
        {
            var binaryMap = new Mat();
            try
            {
                ComputeBinaryMap(saliencyMap, binaryMap);
                return binaryMap;
            }
            catch
            {
                binaryMap.Dispose();
                throw;
            }
        }

        private static void ValidateSaliencyMapType(Mat saliencyMap, string parameterName)
        {
            if (saliencyMap.Type != MatType.CV_32FC1)
            {
                throw new ArgumentException("Static saliency binary map requires a CV_32FC1 saliency map.", parameterName);
            }
        }
    }
}
