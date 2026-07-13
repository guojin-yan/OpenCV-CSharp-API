using OpenCvSharp.Core;

namespace OpenCvSharp.Plot
{
    /// <summary>
    /// Provides convenience factory methods for the Plot module.
    /// 提供 Plot 模块的便捷工厂方法。
    /// </summary>
    public static class PlotCv2
    {
        /// <summary>
        /// Creates a plot from a vector of Y values.
        /// 从 Y 值向量创建曲线图。
        /// </summary>
        public static Plot2d CreatePlot2d(Mat data)
        {
            return Plot2d.Create(data);
        }

        /// <summary>
        /// Creates a plot from vectors of X and Y values.
        /// 从 X 与 Y 值向量创建曲线图。
        /// </summary>
        public static Plot2d CreatePlot2d(Mat dataX, Mat dataY)
        {
            return Plot2d.Create(dataX, dataY);
        }
    }
}
