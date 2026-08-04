namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Predefined ArUco and AprilTag dictionaries from the current OpenCV runtime.
    /// 当前 OpenCV runtime 提供的预定义 ArUco 与 AprilTag 字典。
    /// </summary>
    public enum PredefinedDictionaryType
    {
        /// <summary>4x4 markers, 50 codes. 4x4 标记，50 个编码。</summary>
        Dict4X4_50 = 0,

        /// <summary>4x4 markers, 100 codes. 4x4 标记，100 个编码。</summary>
        Dict4X4_100 = 1,

        /// <summary>4x4 markers, 250 codes. 4x4 标记，250 个编码。</summary>
        Dict4X4_250 = 2,

        /// <summary>4x4 markers, 1000 codes. 4x4 标记，1000 个编码。</summary>
        Dict4X4_1000 = 3,

        /// <summary>5x5 markers, 50 codes. 5x5 标记，50 个编码。</summary>
        Dict5X5_50 = 4,

        /// <summary>5x5 markers, 100 codes. 5x5 标记，100 个编码。</summary>
        Dict5X5_100 = 5,

        /// <summary>5x5 markers, 250 codes. 5x5 标记，250 个编码。</summary>
        Dict5X5_250 = 6,

        /// <summary>5x5 markers, 1000 codes. 5x5 标记，1000 个编码。</summary>
        Dict5X5_1000 = 7,

        /// <summary>6x6 markers, 50 codes. 6x6 标记，50 个编码。</summary>
        Dict6X6_50 = 8,

        /// <summary>6x6 markers, 100 codes. 6x6 标记，100 个编码。</summary>
        Dict6X6_100 = 9,

        /// <summary>6x6 markers, 250 codes. 6x6 标记，250 个编码。</summary>
        Dict6X6_250 = 10,

        /// <summary>6x6 markers, 1000 codes. 6x6 标记，1000 个编码。</summary>
        Dict6X6_1000 = 11,

        /// <summary>7x7 markers, 50 codes. 7x7 标记，50 个编码。</summary>
        Dict7X7_50 = 12,

        /// <summary>7x7 markers, 100 codes. 7x7 标记，100 个编码。</summary>
        Dict7X7_100 = 13,

        /// <summary>7x7 markers, 250 codes. 7x7 标记，250 个编码。</summary>
        Dict7X7_250 = 14,

        /// <summary>7x7 markers, 1000 codes. 7x7 标记，1000 个编码。</summary>
        Dict7X7_1000 = 15,

        /// <summary>Original ArUco dictionary. 原始 ArUco 字典。</summary>
        DictArucoOriginal = 16,

        /// <summary>AprilTag 16h5 dictionary. AprilTag 16h5 字典。</summary>
        DictAprilTag16h5 = 17,

        /// <summary>AprilTag 25h9 dictionary. AprilTag 25h9 字典。</summary>
        DictAprilTag25h9 = 18,

        /// <summary>AprilTag 36h10 dictionary. AprilTag 36h10 字典。</summary>
        DictAprilTag36h10 = 19,

        /// <summary>AprilTag 36h11 dictionary. AprilTag 36h11 字典。</summary>
        DictAprilTag36h11 = 20,

        /// <summary>ArUco MIP 36h12 dictionary. ArUco MIP 36h12 字典。</summary>
        DictArucoMip36h12 = 21
    }
}
