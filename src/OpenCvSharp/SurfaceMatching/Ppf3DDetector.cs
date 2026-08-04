using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.SurfaceMatching
{
    /// <summary>
    /// PPF 3D detector from OpenCV surface matching.
    /// OpenCV surface matching 的 PPF 3D 检测器。
    /// </summary>
    public sealed class Ppf3DDetector : IDisposable
    {
        private NativeSurfaceMatchingPpf3DDetectorHandle handle;
        private bool disposed;

        private Ppf3DDetector(NativeSurfaceMatchingPpf3DDetectorHandle handle)
        {
            this.handle = handle;
        }

        /// <summary>Gets whether this detector has been disposed. 获取对象是否已经释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Creates a PPF 3D detector. 创建 PPF 3D 检测器。</summary>
        public static Ppf3DDetector Create(double relativeSamplingStep = 0.05, double relativeDistanceStep = 0.05, double numAngles = 30.0)
        {
            Icp.ValidatePositiveFinite(relativeSamplingStep, nameof(relativeSamplingStep));
            Icp.ValidatePositiveFinite(relativeDistanceStep, nameof(relativeDistanceStep));
            Icp.ValidatePositiveFinite(numAngles, nameof(numAngles));
            NativeException.ThrowIfError(NativeMethods.SurfaceMatchingPpf3DDetectorCreate(
                relativeSamplingStep,
                relativeDistanceStep,
                numAngles,
                out IntPtr nativeHandle));
            return new Ppf3DDetector(NativeSurfaceMatchingPpf3DDetectorHandle.FromNativePointer(nativeHandle));
        }

        /// <summary>Sets pose clustering search parameters. 设置 pose 聚类搜索参数。</summary>
        public void SetSearchParams(double positionThreshold = -1.0, double rotationThreshold = -1.0, bool useWeightedClustering = false)
        {
            ThrowIfDisposed();
            ValidateFiniteOrMinusOne(positionThreshold, nameof(positionThreshold));
            ValidateFiniteOrMinusOne(rotationThreshold, nameof(rotationThreshold));
            NativeException.ThrowIfError(NativeMethods.SurfaceMatchingPpf3DDetectorSetSearchParams(
                NativeHandle,
                positionThreshold,
                rotationThreshold,
                useWeightedClustering ? 1 : 0));
        }

        /// <summary>Trains the detector from an Nx6 point cloud with normals. 使用带法线的 Nx6 点云训练检测器。</summary>
        public void TrainModel(Mat model)
        {
            ThrowIfDisposed();
            Icp.ValidateNotNull(model, nameof(model));
            ValidatePointCloudType(model, nameof(model));
            NativeException.ThrowIfError(NativeMethods.SurfaceMatchingPpf3DDetectorTrainModel(NativeHandle, model.NativeHandle));
        }

        /// <summary>Matches a scene point cloud and returns pose summaries. 匹配场景点云并返回 pose 摘要。</summary>
        public Pose3DResult[] Match(Mat scene, double relativeSceneSampleStep = 1.0 / 5.0, double relativeSceneDistance = 0.03)
        {
            ThrowIfDisposed();
            Icp.ValidateNotNull(scene, nameof(scene));
            ValidatePointCloudType(scene, nameof(scene));
            ValidateSceneSampleStep(relativeSceneSampleStep, nameof(relativeSceneSampleStep));
            Icp.ValidatePositiveFinite(relativeSceneDistance, nameof(relativeSceneDistance));

            NativeException.ThrowIfError(NativeMethods.SurfaceMatchingPpf3DDetectorMatchCount(
                NativeHandle,
                scene.NativeHandle,
                relativeSceneSampleStep,
                relativeSceneDistance,
                out int count));

            if (count <= 0)
            {
                return Array.Empty<Pose3DResult>();
            }

            var nativeResults = new NativeSurfaceMatchingPose3DResult[count];
            NativeException.ThrowIfError(NativeMethods.SurfaceMatchingPpf3DDetectorMatchFill(
                NativeHandle,
                scene.NativeHandle,
                relativeSceneSampleStep,
                relativeSceneDistance,
                nativeResults,
                nativeResults.Length,
                out int writtenCount));

            var results = new Pose3DResult[writtenCount];
            for (int i = 0; i < writtenCount; i++)
            {
                results[i] = new Pose3DResult(nativeResults[i]);
            }

            return results;
        }

        /// <summary>Releases native resources. 释放 native 资源。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        private static void ValidateFiniteOrMinusOne(double value, string parameterName)
        {
            if ((value != -1.0 && value < 0.0) || double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be -1 or a finite non-negative value.");
            }
        }

        private static void ValidateSceneSampleStep(double value, string parameterName)
        {
            if (value <= 0.0 || value > 1.0 || double.IsNaN(value) || double.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be a finite value greater than 0 and less than or equal to 1.");
            }
        }

        private static void ValidatePointCloudType(Mat value, string parameterName)
        {
            if (value.Type != MatType.CV_32FC1)
            {
                throw new ArgumentException("Point cloud must be CV_32FC1.", parameterName);
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
