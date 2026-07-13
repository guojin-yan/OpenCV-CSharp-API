namespace OpenCvSharp.Reg
{
    /// <summary>
    /// Factory helpers for the OpenCV reg module.
    /// OpenCV reg 模块的工厂辅助方法。
    /// </summary>
    public static class RegCv2
    {
        /// <summary>Creates a translation map. 创建平移 map。</summary>
        public static MapShift CreateMapShift(double shiftX = 0.0, double shiftY = 0.0)
        {
            return new MapShift(shiftX, shiftY);
        }

        /// <summary>Creates an affine map. 创建仿射 map。</summary>
        public static MapAffine CreateMapAffine(AffineTransform2D transform)
        {
            return new MapAffine(transform);
        }

        /// <summary>Creates a projective map. 创建投影 map。</summary>
        public static MapProjec CreateMapProjec(ProjectiveTransform2D transform)
        {
            return new MapProjec(transform);
        }

        /// <summary>Creates a gradient shift mapper. 创建梯度平移 mapper。</summary>
        public static MapperGradShift CreateMapperGradShift()
        {
            return new MapperGradShift();
        }

        /// <summary>Creates a gradient Euclidean mapper. 创建梯度欧氏 mapper。</summary>
        public static MapperGradEuclid CreateMapperGradEuclid()
        {
            return new MapperGradEuclid();
        }

        /// <summary>Creates a gradient similarity mapper. 创建梯度相似 mapper。</summary>
        public static MapperGradSimilar CreateMapperGradSimilar()
        {
            return new MapperGradSimilar();
        }

        /// <summary>Creates a gradient affine mapper. 创建梯度仿射 mapper。</summary>
        public static MapperGradAffine CreateMapperGradAffine()
        {
            return new MapperGradAffine();
        }

        /// <summary>Creates a gradient projective mapper. 创建梯度投影 mapper。</summary>
        public static MapperGradProj CreateMapperGradProj()
        {
            return new MapperGradProj();
        }

        /// <summary>Creates a pyramid mapper. 创建金字塔 mapper。</summary>
        public static MapperPyramid CreateMapperPyramid(RegMapper baseMapper)
        {
            return new MapperPyramid(baseMapper);
        }
    }
}
