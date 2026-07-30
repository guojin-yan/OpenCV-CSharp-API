using System;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Configures OpenCV's universal sample-consensus estimators.
    /// 配置 OpenCV 通用样本一致性估计器。
    /// </summary>
    public sealed class UsacParams
    {
        /// <summary>Initializes the OpenCV 5 default USAC configuration. 初始化 OpenCV 5 默认 USAC 配置。</summary>
        public UsacParams()
        {
            NativeException.ThrowIfError(NativeMethods.Calib3DUsacParamsGetDefault(out NativeMethods.Calib3DUsacParamsNative value));
            Confidence = value.Confidence;
            IsParallel = value.IsParallel != 0;
            LocalOptimizationIterations = value.LoIterations;
            LocalOptimizationMethod = (UsacLocalOptimizationMethod)value.LoMethod;
            LocalOptimizationSampleSize = value.LoSampleSize;
            MaxIterations = value.MaxIterations;
            NeighborSearchMethod = (UsacNeighborSearchMethod)value.NeighborsSearch;
            RandomGeneratorState = value.RandomGeneratorState;
            SamplingMethod = (UsacSamplingMethod)value.Sampler;
            ScoreMethod = (UsacScoreMethod)value.Score;
            Threshold = value.Threshold;
            FinalPolishingMethod = (UsacPolishingMethod)value.FinalPolisher;
            FinalPolishingIterations = value.FinalPolisherIterations;
        }

        /// <summary>Gets or sets the required confidence in (0, 1]. 获取或设置 (0, 1] 范围内的置信度。</summary>
        public double Confidence { get; set; }

        /// <summary>Gets or sets whether estimation may run in parallel. 获取或设置估计是否可并行运行。</summary>
        public bool IsParallel { get; set; }

        /// <summary>Gets or sets the local-optimization iteration count. 获取或设置局部优化迭代次数。</summary>
        public int LocalOptimizationIterations { get; set; }

        /// <summary>Gets or sets the local-optimization strategy. 获取或设置局部优化策略。</summary>
        public UsacLocalOptimizationMethod LocalOptimizationMethod { get; set; }

        /// <summary>Gets or sets the local-optimization sample size. 获取或设置局部优化样本数。</summary>
        public int LocalOptimizationSampleSize { get; set; }

        /// <summary>Gets or sets the maximum estimator iteration count. 获取或设置估计器最大迭代次数。</summary>
        public int MaxIterations { get; set; }

        /// <summary>Gets or sets the neighborhood search strategy. 获取或设置邻域搜索策略。</summary>
        public UsacNeighborSearchMethod NeighborSearchMethod { get; set; }

        /// <summary>Gets or sets the deterministic random-generator state. 获取或设置确定性的随机数生成器状态。</summary>
        public int RandomGeneratorState { get; set; }

        /// <summary>Gets or sets the minimal-sample strategy. 获取或设置最小样本策略。</summary>
        public UsacSamplingMethod SamplingMethod { get; set; }

        /// <summary>Gets or sets the model score. 获取或设置模型评分方法。</summary>
        public UsacScoreMethod ScoreMethod { get; set; }

        /// <summary>Gets or sets the inlier threshold. 获取或设置内点阈值。</summary>
        public double Threshold { get; set; }

        /// <summary>Gets or sets the final polishing strategy. 获取或设置最终精修策略。</summary>
        public UsacPolishingMethod FinalPolishingMethod { get; set; }

        /// <summary>Gets or sets the final polishing iteration count. 获取或设置最终精修迭代次数。</summary>
        public int FinalPolishingIterations { get; set; }

        internal NativeMethods.Calib3DUsacParamsNative ToNative()
        {
            if (double.IsNaN(Confidence) || double.IsInfinity(Confidence) || Confidence <= 0.0 || Confidence > 1.0)
                throw new ArgumentOutOfRangeException(nameof(Confidence), "Confidence must be finite and in (0, 1].");
            if (LocalOptimizationIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(LocalOptimizationIterations));
            if (!Enum.IsDefined(typeof(UsacLocalOptimizationMethod), LocalOptimizationMethod))
                throw new ArgumentOutOfRangeException(nameof(LocalOptimizationMethod));
            if (LocalOptimizationSampleSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(LocalOptimizationSampleSize));
            if (MaxIterations <= 0)
                throw new ArgumentOutOfRangeException(nameof(MaxIterations));
            if (!Enum.IsDefined(typeof(UsacNeighborSearchMethod), NeighborSearchMethod))
                throw new ArgumentOutOfRangeException(nameof(NeighborSearchMethod));
            if (!Enum.IsDefined(typeof(UsacSamplingMethod), SamplingMethod))
                throw new ArgumentOutOfRangeException(nameof(SamplingMethod));
            if (!Enum.IsDefined(typeof(UsacScoreMethod), ScoreMethod))
                throw new ArgumentOutOfRangeException(nameof(ScoreMethod));
            if (double.IsNaN(Threshold) || double.IsInfinity(Threshold) || Threshold <= 0.0)
                throw new ArgumentOutOfRangeException(nameof(Threshold), "Threshold must be finite and positive.");
            if (!Enum.IsDefined(typeof(UsacPolishingMethod), FinalPolishingMethod))
                throw new ArgumentOutOfRangeException(nameof(FinalPolishingMethod));
            if (FinalPolishingIterations < 0)
                throw new ArgumentOutOfRangeException(nameof(FinalPolishingIterations));

            return new NativeMethods.Calib3DUsacParamsNative
            {
                Confidence = Confidence,
                IsParallel = IsParallel ? 1 : 0,
                LoIterations = LocalOptimizationIterations,
                LoMethod = (int)LocalOptimizationMethod,
                LoSampleSize = LocalOptimizationSampleSize,
                MaxIterations = MaxIterations,
                NeighborsSearch = (int)NeighborSearchMethod,
                RandomGeneratorState = RandomGeneratorState,
                Sampler = (int)SamplingMethod,
                Score = (int)ScoreMethod,
                Threshold = Threshold,
                FinalPolisher = (int)FinalPolishingMethod,
                FinalPolisherIterations = FinalPolishingIterations
            };
        }
    }
}
