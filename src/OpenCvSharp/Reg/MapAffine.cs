using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Reg
{
    /// <summary>
    /// Registration map that models a 2D affine transform.
    /// 表示二维仿射变换的 registration map。
    /// </summary>
    public sealed class MapAffine : RegMap
    {
        internal MapAffine(NativeRegMapHandle handle)
            : base(handle)
        {
        }

        /// <summary>Creates an identity affine map. 创建单位仿射 map。</summary>
        public MapAffine()
            : this(AffineTransform2D.Identity)
        {
        }

        /// <summary>Creates an affine map from transform values. 使用变换值创建仿射 map。</summary>
        public MapAffine(AffineTransform2D transform)
            : base(CreateHandle(transform))
        {
        }

        /// <summary>Gets the affine transform. 获取仿射变换。</summary>
        public AffineTransform2D Transform
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.RegMapAffineGet(
                    NativeHandle,
                    out double m00,
                    out double m01,
                    out double m10,
                    out double m11,
                    out double shiftX,
                    out double shiftY));
                return new AffineTransform2D(m00, m01, m10, m11, shiftX, shiftY);
            }
        }

        private static NativeRegMapHandle CreateHandle(AffineTransform2D transform)
        {
            ValidateFinite(transform.M00, nameof(transform));
            ValidateFinite(transform.M01, nameof(transform));
            ValidateFinite(transform.M10, nameof(transform));
            ValidateFinite(transform.M11, nameof(transform));
            ValidateFinite(transform.ShiftX, nameof(transform));
            ValidateFinite(transform.ShiftY, nameof(transform));
            NativeException.ThrowIfError(NativeMethods.RegMapAffineCreate(
                transform.M00,
                transform.M01,
                transform.M10,
                transform.M11,
                transform.ShiftX,
                transform.ShiftY,
                out IntPtr nativeHandle));
            return NativeRegMapHandle.FromNativePointer(nativeHandle);
        }
    }
}
