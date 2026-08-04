using System;
using System.Globalization;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// Creation parameters for OpenCV contrib <c>FacemarkLBF</c>.
    /// OpenCV contrib <c>FacemarkLBF</c> 创建参数。
    /// </summary>
    public sealed class FacemarkLBFParams
    {
        private int[] featureCounts;
        private double[] radiusValues;
        private int[] leftPupilIndices;
        private int[] rightPupilIndices;

        /// <summary>Initializes parameters with OpenCV 5.0.0 LBF defaults. 使用 OpenCV 5.0.0 LBF 默认值初始化参数。</summary>
        public FacemarkLBFParams()
        {
            ShapeOffset = 0.0;
            CascadeFace = string.Empty;
            Verbose = false;
            NLandmarks = 68;
            InitialShapeCount = 10;
            StageCount = 5;
            TreeCount = 6;
            TreeDepth = 5;
            BaggingOverlap = 0.4;
            ModelFilename = string.Empty;
            SaveModel = true;
            Seed = 0;
            featureCounts = new[] { 500, 500, 500, 300, 300, 300, 200, 200, 200, 100 };
            radiusValues = new[] { 0.3, 0.2, 0.15, 0.12, 0.10, 0.10, 0.08, 0.06, 0.06, 0.05 };
            leftPupilIndices = new[] { 36, 37, 38, 39, 40, 41 };
            rightPupilIndices = new[] { 42, 43, 44, 45, 46, 47 };
            DetectRegion = new Rect(-1, -1, -1, -1);
        }

        /// <summary>Initializes parameters by copying another instance. 通过复制另一个实例初始化参数。</summary>
        /// <param name="other">The parameters to copy. 要复制的参数。</param>
        public FacemarkLBFParams(FacemarkLBFParams other)
        {
            if (other == null) { throw new ArgumentNullException(nameof(other)); }

            ShapeOffset = other.ShapeOffset;
            CascadeFace = other.CascadeFace;
            Verbose = other.Verbose;
            NLandmarks = other.NLandmarks;
            InitialShapeCount = other.InitialShapeCount;
            StageCount = other.StageCount;
            TreeCount = other.TreeCount;
            TreeDepth = other.TreeDepth;
            BaggingOverlap = other.BaggingOverlap;
            ModelFilename = other.ModelFilename;
            SaveModel = other.SaveModel;
            Seed = other.Seed;
            featureCounts = Clone(other.featureCounts);
            radiusValues = Clone(other.radiusValues);
            leftPupilIndices = Clone(other.leftPupilIndices);
            rightPupilIndices = Clone(other.rightPupilIndices);
            DetectRegion = other.DetectRegion;
        }

        /// <summary>Gets or sets landmark coordinate offset used while loading training points. 获取或设置加载训练点时使用的坐标偏移。</summary>
        public double ShapeOffset { get; set; }

        /// <summary>Gets or sets the cascade face detector file path. 获取或设置 cascade 人脸检测器文件路径。</summary>
        public string CascadeFace { get; set; }

        /// <summary>Gets or sets whether OpenCV should print training progress. 获取或设置 OpenCV 是否输出训练进度。</summary>
        public bool Verbose { get; set; }

        /// <summary>Gets or sets the number of landmarks, normally 29 or 68. 获取或设置关键点数量，通常为 29 或 68。</summary>
        public int NLandmarks { get; set; }

        /// <summary>Gets or sets the initial shape augmentation count. 获取或设置初始形状增强数量。</summary>
        public int InitialShapeCount { get; set; }

        /// <summary>Gets or sets the number of refinement stages. 获取或设置细化阶段数量。</summary>
        public int StageCount { get; set; }

        /// <summary>Gets or sets the tree count per landmark refinement. 获取或设置每个关键点细化使用的树数量。</summary>
        public int TreeCount { get; set; }

        /// <summary>Gets or sets the tree depth. 获取或设置树深度。</summary>
        public int TreeDepth { get; set; }

        /// <summary>Gets or sets the bagging overlap ratio. 获取或设置 bagging 重叠比例。</summary>
        public double BaggingOverlap { get; set; }

        /// <summary>Gets or sets the model filename used by OpenCV training save. 获取或设置 OpenCV 训练保存使用的模型文件名。</summary>
        public string ModelFilename { get; set; }

        /// <summary>Gets or sets whether training should save the model. 获取或设置训练是否保存模型。</summary>
        public bool SaveModel { get; set; }

        /// <summary>Gets or sets the OpenCV random seed used by training. 获取或设置训练使用的 OpenCV 随机种子。</summary>
        public uint Seed { get; set; }

        /// <summary>Gets or sets the per-stage feature counts. 获取或设置每阶段特征数量。</summary>
        public int[] FeatureCounts
        {
            get { return Clone(featureCounts); }
            set { featureCounts = CloneRequired(value, nameof(value)); }
        }

        /// <summary>Gets or sets the per-stage feature radii. 获取或设置每阶段特征半径。</summary>
        public double[] RadiusValues
        {
            get { return Clone(radiusValues); }
            set { radiusValues = CloneRequired(value, nameof(value)); }
        }

        /// <summary>Gets or sets the left-eye pupil landmark indices. 获取或设置左眼 pupil 关键点索引。</summary>
        public int[] LeftPupilIndices
        {
            get { return Clone(leftPupilIndices); }
            set { leftPupilIndices = CloneRequired(value, nameof(value)); }
        }

        /// <summary>Gets or sets the right-eye pupil landmark indices. 获取或设置右眼 pupil 关键点索引。</summary>
        public int[] RightPupilIndices
        {
            get { return Clone(rightPupilIndices); }
            set { rightPupilIndices = CloneRequired(value, nameof(value)); }
        }

        /// <summary>Gets or sets the optional detection ROI. 获取或设置可选检测 ROI。</summary>
        public Rect DetectRegion { get; set; }

        /// <summary>Creates a copy of this parameter object. 创建此参数对象的副本。</summary>
        public FacemarkLBFParams Clone()
        {
            return new FacemarkLBFParams(this);
        }

        internal void Validate()
        {
            if (CascadeFace == null) { throw new ArgumentNullException(nameof(CascadeFace)); }
            if (ModelFilename == null) { throw new ArgumentNullException(nameof(ModelFilename)); }
            if (NLandmarks <= 0) { throw new ArgumentOutOfRangeException(nameof(NLandmarks)); }
            if (InitialShapeCount <= 0) { throw new ArgumentOutOfRangeException(nameof(InitialShapeCount)); }
            if (StageCount <= 0) { throw new ArgumentOutOfRangeException(nameof(StageCount)); }
            if (TreeCount <= 0) { throw new ArgumentOutOfRangeException(nameof(TreeCount)); }
            if (TreeDepth <= 0) { throw new ArgumentOutOfRangeException(nameof(TreeDepth)); }
            if (featureCounts.Length != radiusValues.Length)
            {
                throw new ArgumentException("FeatureCounts and RadiusValues must have the same length.", nameof(RadiusValues));
            }
        }

        private static int[] Clone(int[] values)
        {
            var clone = new int[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }

        private static double[] Clone(double[] values)
        {
            var clone = new double[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }

        private static int[] CloneRequired(int[] values, string parameterName)
        {
            if (values == null) { throw new ArgumentNullException(parameterName); }
            return Clone(values);
        }

        private static double[] CloneRequired(double[] values, string parameterName)
        {
            if (values == null) { throw new ArgumentNullException(parameterName); }
            return Clone(values);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{ShapeOffset=" + ShapeOffset.ToString(CultureInfo.InvariantCulture)
                + ",CascadeFace=" + CascadeFace
                + ",Verbose=" + Verbose
                + ",NLandmarks=" + NLandmarks
                + ",InitialShapeCount=" + InitialShapeCount
                + ",StageCount=" + StageCount
                + ",TreeCount=" + TreeCount
                + ",TreeDepth=" + TreeDepth
                + ",BaggingOverlap=" + BaggingOverlap.ToString(CultureInfo.InvariantCulture)
                + ",ModelFilename=" + ModelFilename
                + ",SaveModel=" + SaveModel
                + ",Seed=" + Seed.ToString(CultureInfo.InvariantCulture)
                + ",FeatureCounts=" + (featureCounts == null ? "<null>" : featureCounts.Length.ToString(CultureInfo.InvariantCulture))
                + ",RadiusValues=" + (radiusValues == null ? "<null>" : radiusValues.Length.ToString(CultureInfo.InvariantCulture))
                + ",LeftPupilIndices=" + (leftPupilIndices == null ? "<null>" : leftPupilIndices.Length.ToString(CultureInfo.InvariantCulture))
                + ",RightPupilIndices=" + (rightPupilIndices == null ? "<null>" : rightPupilIndices.Length.ToString(CultureInfo.InvariantCulture))
                + ",DetectRegion=" + DetectRegion
                + "}";
        }
    }
}
