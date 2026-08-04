using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.XImgProc
{
    /// <summary>
    /// Weighted least-squares disparity filter wrapper.
    /// 加权最小二乘 disparity 滤波器包装。
    /// </summary>
    public sealed class DisparityWLSFilter : DisparityFilter
    {
        private NativeXImgProcDisparityWLSFilterHandle handle;
        private readonly bool useConfidence;

        internal DisparityWLSFilter(IntPtr nativeHandle, bool useConfidence)
        {
            handle = NativeXImgProcDisparityWLSFilterHandle.FromNativePointer(nativeHandle);
            this.useConfidence = useConfidence;
        }

        internal override IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Gets or sets WLS regularization strength. 获取或设置 WLS 正则化强度。</summary>
        public double Lambda
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterGetLambda(NativeHandle, out double value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterSetLambda(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets source-edge color sensitivity. 获取或设置源图边缘颜色敏感度。</summary>
        public double SigmaColor
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterGetSigmaColor(NativeHandle, out double value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterSetSigmaColor(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the left-right consistency threshold. 获取或设置左右一致性阈值。</summary>
        public int LrcThreshold
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterGetLrcThresh(NativeHandle, out int value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterSetLrcThresh(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets confidence radius near depth discontinuities. 获取或设置深度不连续附近的置信半径。</summary>
        public int DepthDiscontinuityRadius
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterGetDepthDiscontinuityRadius(NativeHandle, out int value));
                return value;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterSetDepthDiscontinuityRadius(NativeHandle, value));
            }
        }

        /// <summary>Gets the ROI used by the last filter call. 获取最近一次滤波使用的 ROI。</summary>
        public Rect ROI
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterGetRoi(NativeHandle, out NativeMethods.XImgProcRectNative roi));
                return NativeXImgProcConvert.ToRect(roi);
            }
        }

        /// <summary>Creates a generic WLS disparity filter. 创建 generic WLS disparity 滤波器。</summary>
        public static DisparityWLSFilter CreateGeneric(bool useConfidence)
        {
            NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterCreateGeneric(useConfidence ? 1 : 0, out IntPtr nativeHandle));
            return new DisparityWLSFilter(nativeHandle, useConfidence);
        }

        /// <inheritdoc/>
        public override void Filter(Mat disparityMapLeft, Mat leftView, Mat filteredDisparityMap, Mat? disparityMapRight = null, Rect? roi = null, Mat? rightView = null)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(disparityMapLeft, nameof(disparityMapLeft));
            XImgProcCv2.ValidateNotNull(leftView, nameof(leftView));
            XImgProcCv2.ValidateNotNull(filteredDisparityMap, nameof(filteredDisparityMap));
            XImgProcCv2.ValidateDisparityWLSFilterArguments(disparityMapLeft, leftView, disparityMapRight, useConfidence);
            NativeMethods.XImgProcRectNative nativeRoi = NativeXImgProcConvert.ToNative(roi ?? default(Rect));
            NativeException.ThrowIfError(NativeMethods.XImgProcDisparityFilterFilter(
                NativeHandle,
                disparityMapLeft.NativeHandle,
                leftView.NativeHandle,
                filteredDisparityMap.NativeHandle,
                XImgProcCv2.OptionalMatHandle(disparityMapRight),
                ref nativeRoi,
                XImgProcCv2.OptionalMatHandle(rightView)));
        }

        /// <summary>Copies the latest confidence map into <paramref name="confidenceMap"/>. 将最近置信图复制到 <paramref name="confidenceMap"/>。</summary>
        public void GetConfidenceMap(Mat confidenceMap)
        {
            ThrowIfDisposed();
            XImgProcCv2.ValidateNotNull(confidenceMap, nameof(confidenceMap));
            NativeException.ThrowIfError(NativeMethods.XImgProcDisparityWLSFilterGetConfidenceMap(NativeHandle, confidenceMap.NativeHandle));
        }

        /// <summary>Gets the latest confidence map as a new matrix. 以新矩阵返回最近置信图。</summary>
        public Mat GetConfidenceMap()
        {
            var result = new Mat();
            try
            {
                GetConfidenceMap(result);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing && handle != null)
            {
                handle.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
