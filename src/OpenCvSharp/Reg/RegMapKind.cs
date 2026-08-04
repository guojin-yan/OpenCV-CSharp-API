namespace JYPPX.OpenCvSharp.Reg
{
    /// <summary>
    /// Identifies the concrete OpenCV registration map type.
    /// 标识具体的 OpenCV registration map 类型。
    /// </summary>
    public enum RegMapKind
    {
        /// <summary>Unknown or unsupported map type. 未知或暂不支持的 map 类型。</summary>
        Unknown = 0,

        /// <summary>Translation map. 平移 map。</summary>
        Shift = 1,

        /// <summary>Affine map. 仿射 map。</summary>
        Affine = 2,

        /// <summary>Projective map. 投影 map。</summary>
        Projec = 3
    }
}
