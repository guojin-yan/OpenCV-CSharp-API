using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Reg
{
    /// <summary>
    /// Registration map that models a 2D translation.
    /// 表示二维平移的 registration map。
    /// </summary>
    public sealed class MapShift : RegMap
    {
        internal MapShift(NativeRegMapHandle handle)
            : base(handle)
        {
        }

        /// <summary>Creates a translation map. 创建平移 map。</summary>
        public MapShift(double shiftX = 0.0, double shiftY = 0.0)
            : base(CreateHandle(shiftX, shiftY))
        {
        }

        /// <summary>Gets the x translation. 获取 X 平移量。</summary>
        public double ShiftX
        {
            get
            {
                GetShift(out double shiftX, out _);
                return shiftX;
            }
        }

        /// <summary>Gets the y translation. 获取 Y 平移量。</summary>
        public double ShiftY
        {
            get
            {
                GetShift(out _, out double shiftY);
                return shiftY;
            }
        }

        /// <summary>Gets the shift values. 获取平移量。</summary>
        public void GetShift(out double shiftX, out double shiftY)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.RegMapShiftGet(NativeHandle, out shiftX, out shiftY));
        }

        private static NativeRegMapHandle CreateHandle(double shiftX, double shiftY)
        {
            ValidateFinite(shiftX, nameof(shiftX));
            ValidateFinite(shiftY, nameof(shiftY));
            NativeException.ThrowIfError(NativeMethods.RegMapShiftCreate(shiftX, shiftY, out IntPtr nativeHandle));
            return NativeRegMapHandle.FromNativePointer(nativeHandle);
        }
    }
}
