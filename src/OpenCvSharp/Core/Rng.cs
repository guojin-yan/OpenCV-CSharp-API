using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Core
{
    /// <summary>
    /// Represents an OpenCV random number generator compatible with <c>cv::RNG</c>.
    /// 表示与 OpenCV <c>cv::RNG</c> 兼容的随机数生成器。
    /// </summary>
    public sealed class Rng : IDisposable
    {
        private NativeRngHandle handle;
        private bool disposed;

        /// <summary>
        /// Initializes a random number generator with OpenCV default state.
        /// 使用 OpenCV 默认状态初始化随机数生成器。
        /// </summary>
        public Rng()
        {
            NativeException.ThrowIfError(NativeMethods.CoreRngCreateDefault(out IntPtr nativeHandle));
            handle = NativeRngHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Initializes a random number generator with a specified state.
        /// 使用指定状态初始化随机数生成器。
        /// </summary>
        /// <param name="state">The 64-bit RNG state. 64 位 RNG 状态。</param>
        public Rng(ulong state)
        {
            NativeException.ThrowIfError(NativeMethods.CoreRngCreate(state, out IntPtr nativeHandle));
            handle = NativeRngHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// 获取此对象是否已经释放。
        /// </summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the 64-bit RNG state.
        /// 获取或设置 64 位 RNG 状态。
        /// </summary>
        public ulong State
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.CoreRngGetState(NativeHandle, out ulong state));
                return state;
            }

            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.CoreRngSetState(NativeHandle, value));
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>
        /// Returns the next 32-bit random number.
        /// 返回下一个 32 位随机数。
        /// </summary>
        public uint Next()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.CoreRngNext(NativeHandle, out uint value));
            return value;
        }

        /// <summary>
        /// Returns a uniformly distributed integer from <paramref name="a"/> inclusive to <paramref name="b"/> exclusive.
        /// 返回范围为包含 <paramref name="a"/>、不包含 <paramref name="b"/> 的均匀分布整数。
        /// </summary>
        public int Uniform(int a, int b)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.CoreRngUniformInt(NativeHandle, a, b, out int value));
            return value;
        }

        /// <summary>
        /// Returns a uniformly distributed single-precision value.
        /// 返回均匀分布的单精度浮点值。
        /// </summary>
        public float Uniform(float a, float b)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.CoreRngUniformFloat(NativeHandle, a, b, out float value));
            return value;
        }

        /// <summary>
        /// Returns a uniformly distributed double-precision value.
        /// 返回均匀分布的双精度浮点值。
        /// </summary>
        public double Uniform(double a, double b)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.CoreRngUniformDouble(NativeHandle, a, b, out double value));
            return value;
        }

        /// <summary>
        /// Returns a Gaussian random value with zero mean and the specified sigma.
        /// 返回均值为 0、标准差为指定 sigma 的高斯随机值。
        /// </summary>
        /// <param name="sigma">The standard deviation. 标准差。</param>
        public double Gaussian(double sigma)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.CoreRngGaussian(NativeHandle, sigma, out double value));
            return value;
        }

        /// <summary>
        /// Fills a matrix with random values from the specified distribution.
        /// 使用指定分布的随机值填充矩阵。
        /// </summary>
        /// <param name="mat">The destination matrix. 目标矩阵。</param>
        /// <param name="distType">The distribution type. 分布类型。</param>
        /// <param name="a">The first distribution parameter. 第一个分布参数。</param>
        /// <param name="b">The second distribution parameter. 第二个分布参数。</param>
        /// <param name="saturateRange">Whether to pre-saturate range for uniform distribution. 是否对均匀分布范围预先饱和。</param>
        public void Fill(Mat mat, RngDistributionTypes distType, Scalar a, Scalar b, bool saturateRange = false)
        {
            ThrowIfDisposed();
            ValidateNotNull(mat, nameof(mat));
            NativeException.ThrowIfError(NativeMethods.CoreRngFill(
                NativeHandle,
                mat.NativeHandle,
                (int)distType,
                a.V0,
                a.V1,
                a.V2,
                a.V3,
                b.V0,
                b.V1,
                b.V2,
                b.V3,
                saturateRange ? 1 : 0));
        }

        /// <summary>
        /// Fills a matrix with uniformly distributed random values.
        /// 使用均匀分布随机值填充矩阵。
        /// </summary>
        public void FillUniform(Mat mat, Scalar lowerInclusive, Scalar upperExclusive, bool saturateRange = false)
        {
            Fill(mat, RngDistributionTypes.Uniform, lowerInclusive, upperExclusive, saturateRange);
        }

        /// <summary>
        /// Fills a matrix with normally distributed random values.
        /// 使用正态分布随机值填充矩阵。
        /// </summary>
        public void FillNormal(Mat mat, Scalar mean, Scalar stdDev)
        {
            Fill(mat, RngDistributionTypes.Normal, mean, stdDev);
        }

        /// <summary>
        /// Releases the native RNG object.
        /// 释放 native RNG 对象。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return disposed ? "{Disposed=True}" : "{State=" + State + "}";
        }

        private void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing && handle != null)
                {
                    handle.Dispose();
                }

                disposed = true;
            }
        }

        private static void ValidateNotNull<T>(T value, string parameterName)
            where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
