using System;
using OpenCvSharp.Core;

namespace OpenCvSharp.Calib3D
{
    /// <summary>
    /// Result returned by the owned <see cref="Cv2.InitUndistortRectifyMap(Mat, Mat, Mat, Mat, Size, int)"/> overload.
    /// owned <see cref="Cv2.InitUndistortRectifyMap(Mat, Mat, Mat, Mat, Size, int)"/> 重载返回的结果。
    /// </summary>
    public readonly struct UndistortRectifyMapResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UndistortRectifyMapResult"/> struct.
        /// 初始化 <see cref="UndistortRectifyMapResult"/> 结构的新实例。
        /// </summary>
        /// <param name="map1">The first owned map. 第一个 owned 映射矩阵。</param>
        /// <param name="map2">The second owned map. 第二个 owned 映射矩阵。</param>
        public UndistortRectifyMapResult(Mat map1, Mat map2)
        {
            Map1 = map1 ?? throw new ArgumentNullException(nameof(map1));
            Map2 = map2 ?? throw new ArgumentNullException(nameof(map2));
        }

        /// <summary>
        /// Gets the first owned map.
        /// 获取第一个 owned 映射矩阵。
        /// </summary>
        public Mat Map1 { get; }

        /// <summary>
        /// Gets the second owned map.
        /// 获取第二个 owned 映射矩阵。
        /// </summary>
        public Mat Map2 { get; }

        /// <summary>
        /// Gets the number of map rows.
        /// 获取映射矩阵行数。
        /// </summary>
        public int Rows
        {
            get { return Map1.Rows; }
        }

        /// <summary>
        /// Gets the number of map columns.
        /// 获取映射矩阵列数。
        /// </summary>
        public int Cols
        {
            get { return Map1.Cols; }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "{Map1=" + Map1.Rows + "x" + Map1.Cols + ",Map2=" + Map2.Rows + "x" + Map2.Cols + "}";
        }
    }
}
