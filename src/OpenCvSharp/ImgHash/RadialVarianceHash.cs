using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.ImgHash
{
    /// <summary>Radial variance image hash. Radial variance 图像哈希。</summary>
    public sealed class RadialVarianceHash : ImgHashBase
    {
        private RadialVarianceHash(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the Gaussian sigma. 获取或设置高斯 sigma。</summary>
        public double Sigma
        {
            get
            {
                GetParameters(out double sigma, out _);
                return sigma;
            }

            set
            {
                ThrowIfDisposed();
                if (value < 1.0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Sigma must be greater than or equal to 1.0.");
                }

                NativeException.ThrowIfError(NativeMethods.ImgHashRadialVarianceSetSigma(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the number of angle lines. 获取或设置角线数量。</summary>
        public int NumOfAngleLine
        {
            get
            {
                GetParameters(out _, out int value);
                return value;
            }

            set
            {
                ThrowIfDisposed();
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "The number of angle lines must be greater than 0.");
                }

                NativeException.ThrowIfError(NativeMethods.ImgHashRadialVarianceSetNumOfAngleLine(NativeHandle, value));
            }
        }

        /// <summary>Creates a RadialVarianceHash object. 创建 RadialVarianceHash 对象。</summary>
        public static RadialVarianceHash Create(double sigma = 1.0, int numOfAngleLine = 180)
        {
            NativeException.ThrowIfError(NativeMethods.ImgHashRadialVarianceCreate(sigma, numOfAngleLine, out IntPtr nativeHandle));
            return new RadialVarianceHash(nativeHandle);
        }

        /// <summary>Gets both parameters. 获取两个参数。</summary>
        public void GetParameters(out double sigma, out int numOfAngleLine)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgHashRadialVarianceGet(NativeHandle, out sigma, out numOfAngleLine));
        }
    }
}
