using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Reg
{
    /// <summary>
    /// Registration map that models a 2D projective transform.
    /// 表示二维投影变换的 registration map。
    /// </summary>
    public sealed class MapProjec : RegMap
    {
        internal MapProjec(NativeRegMapHandle handle)
            : base(handle)
        {
        }

        /// <summary>Creates an identity projective map. 创建单位投影 map。</summary>
        public MapProjec()
            : this(ProjectiveTransform2D.Identity)
        {
        }

        /// <summary>Creates a projective map from transform values. 使用变换值创建投影 map。</summary>
        public MapProjec(ProjectiveTransform2D transform)
            : base(CreateHandle(transform))
        {
        }

        /// <summary>Gets the projective transform. 获取投影变换。</summary>
        public ProjectiveTransform2D Transform
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.RegMapProjecGet(
                    NativeHandle,
                    out double m00,
                    out double m01,
                    out double m02,
                    out double m10,
                    out double m11,
                    out double m12,
                    out double m20,
                    out double m21,
                    out double m22));
                return new ProjectiveTransform2D(m00, m01, m02, m10, m11, m12, m20, m21, m22);
            }
        }

        /// <summary>Normalizes the homography scale. 归一化单应矩阵尺度。</summary>
        public void Normalize()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.RegMapProjecNormalize(NativeHandle));
        }

        private static NativeRegMapHandle CreateHandle(ProjectiveTransform2D transform)
        {
            double[] values = transform.ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                ValidateFinite(values[i], nameof(transform));
            }

            NativeException.ThrowIfError(NativeMethods.RegMapProjecCreate(
                transform.M00,
                transform.M01,
                transform.M02,
                transform.M10,
                transform.M11,
                transform.M12,
                transform.M20,
                transform.M21,
                transform.M22,
                out IntPtr nativeHandle));
            return NativeRegMapHandle.FromNativePointer(nativeHandle);
        }
    }
}
