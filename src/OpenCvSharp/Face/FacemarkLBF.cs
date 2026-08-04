using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Face
{
    /// <summary>
    /// OpenCV contrib LBF facemark model.
    /// OpenCV contrib LBF 人脸关键点模型。
    /// </summary>
    public sealed class FacemarkLBF : FacemarkTrain
    {
        private readonly FacemarkLBFParams parameters;

        private FacemarkLBF(IntPtr nativeHandle, FacemarkLBFParams parameters)
            : base(nativeHandle)
        {
            this.parameters = parameters.Clone();
        }

        /// <summary>Gets a copy of the creation parameters. 获取创建参数副本。</summary>
        public FacemarkLBFParams Parameters
        {
            get { return parameters.Clone(); }
        }

        /// <summary>Gets the configured landmark count. 获取配置的关键点数量。</summary>
        public int NLandmarks
        {
            get { return parameters.NLandmarks; }
        }

        /// <summary>Gets the configured initial shape count. 获取配置的初始形状数量。</summary>
        public int InitialShapeCount
        {
            get { return parameters.InitialShapeCount; }
        }

        /// <summary>Gets the configured stage count. 获取配置的阶段数量。</summary>
        public int StageCount
        {
            get { return parameters.StageCount; }
        }

        /// <summary>Gets the configured tree count. 获取配置的树数量。</summary>
        public int TreeCount
        {
            get { return parameters.TreeCount; }
        }

        /// <summary>Gets the configured tree depth. 获取配置的树深度。</summary>
        public int TreeDepth
        {
            get { return parameters.TreeDepth; }
        }

        /// <summary>Gets the configured bagging overlap. 获取配置的 bagging 重叠比例。</summary>
        public double BaggingOverlap
        {
            get { return parameters.BaggingOverlap; }
        }

        /// <summary>Creates an LBF facemark model with OpenCV defaults. 使用 OpenCV 默认值创建 LBF facemark 模型。</summary>
        public static FacemarkLBF Create()
        {
            return Create(new FacemarkLBFParams());
        }

        /// <summary>Creates an LBF facemark model with explicit common parameters. 使用常用显式参数创建 LBF facemark 模型。</summary>
        public static FacemarkLBF Create(
            int nLandmarks,
            int initialShapeCount,
            int stageCount,
            int treeCount,
            int treeDepth,
            double shapeOffset = 0.0,
            double baggingOverlap = 0.4,
            bool verbose = false)
        {
            var parameters = new FacemarkLBFParams
            {
                NLandmarks = nLandmarks,
                InitialShapeCount = initialShapeCount,
                StageCount = stageCount,
                TreeCount = treeCount,
                TreeDepth = treeDepth,
                ShapeOffset = shapeOffset,
                BaggingOverlap = baggingOverlap,
                Verbose = verbose,
                SaveModel = false
            };
            return Create(parameters);
        }

        /// <summary>Creates an LBF facemark model with full parameter control. 使用完整参数控制创建 LBF facemark 模型。</summary>
        public static FacemarkLBF Create(FacemarkLBFParams parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            FacemarkLBFParams copy = parameters.Clone();
            copy.Validate();

            int[] featureCounts = copy.FeatureCounts;
            double[] radiusValues = copy.RadiusValues;
            int[] leftPupil = copy.LeftPupilIndices;
            int[] rightPupil = copy.RightPupilIndices;
            byte[] cascadeFace = FaceStringConvert.ToNullTerminatedUtf8(copy.CascadeFace, nameof(copy.CascadeFace));
            byte[] modelFilename = FaceStringConvert.ToNullTerminatedUtf8(copy.ModelFilename, nameof(copy.ModelFilename));

            NativeException.ThrowIfError(NativeMethods.FaceFacemarkLBFCreateEx(
                copy.NLandmarks,
                copy.InitialShapeCount,
                copy.StageCount,
                copy.TreeCount,
                copy.TreeDepth,
                copy.ShapeOffset,
                copy.BaggingOverlap,
                copy.Verbose ? 1 : 0,
                copy.SaveModel ? 1 : 0,
                copy.Seed,
                cascadeFace,
                modelFilename,
                featureCounts,
                featureCounts.Length,
                radiusValues,
                radiusValues.Length,
                leftPupil,
                leftPupil.Length,
                rightPupil,
                rightPupil.Length,
                copy.DetectRegion.X,
                copy.DetectRegion.Y,
                copy.DetectRegion.Width,
                copy.DetectRegion.Height,
                out IntPtr nativeHandle));

            return new FacemarkLBF(nativeHandle, copy);
        }
    }
}
