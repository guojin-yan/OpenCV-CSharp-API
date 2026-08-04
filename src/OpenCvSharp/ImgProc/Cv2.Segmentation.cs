using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Geometry;
using JYPPX.OpenCvSharp.Internal.Interop;

#if NETCOREAPP3_1_OR_GREATER
using System.Buffers;
#endif

namespace JYPPX.OpenCvSharp.ImgProc
{
    public static partial class Cv2
    {
        private const int MomentsValueCount = 24;
        private const int HuMomentsValueCount = 7;

        /// <summary>
        /// Applies adaptive thresholding to a grayscale image.
        /// 对灰度图像执行自适应阈值处理。
        /// </summary>
        /// <param name="src">The source 8-bit single-channel image. 源 8 位单通道图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <param name="maxValue">The value assigned to pixels satisfying the condition. 满足条件时赋给像素的值。</param>
        /// <param name="adaptiveMethod">The adaptive thresholding algorithm. 自适应阈值算法。</param>
        /// <param name="thresholdType">The thresholding type, usually binary or inverse binary. 阈值类型，通常为二值或反二值。</param>
        /// <param name="blockSize">The odd neighborhood size greater than one. 大于一的奇数邻域尺寸。</param>
        /// <param name="c">The constant subtracted from the weighted mean. 从加权均值中减去的常量。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="blockSize"/> is not an odd value greater than one. 当 <paramref name="blockSize"/> 不是大于一的奇数时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void AdaptiveThreshold(
            Mat src,
            Mat dst,
            double maxValue,
            AdaptiveThresholdTypes adaptiveMethod,
            ThresholdTypes thresholdType,
            int blockSize,
            double c)
        {
            ValidateMatPair(src, dst);
            ValidateOddGreaterThanOne(blockSize, nameof(blockSize));

            NativeException.ThrowIfError(NativeMethods.ImgProcAdaptiveThreshold(
                src.NativeHandle,
                dst.NativeHandle,
                maxValue,
                (int)adaptiveMethod,
                (int)thresholdType,
                blockSize,
                c));
        }

        /// <summary>
        /// Calculates the integral image.
        /// 计算积分图。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="sum">The output integral image. 输出积分图。</param>
        /// <param name="sdepth">The depth of the integral image, or -1 for OpenCV default. 积分图深度，-1 表示使用 OpenCV 默认值。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="sum"/> is null. 当 <paramref name="src"/> 或 <paramref name="sum"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Integral(Mat src, Mat sum, int sdepth = -1)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(sum, nameof(sum));

            NativeException.ThrowIfError(NativeMethods.ImgProcIntegral(src.NativeHandle, sum.NativeHandle, sdepth));
        }

        /// <summary>
        /// Calculates the integral image and squared integral image.
        /// 计算积分图和平方积分图。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="sum">The output integral image. 输出积分图。</param>
        /// <param name="sqsum">The output squared integral image. 输出平方积分图。</param>
        /// <param name="sdepth">The depth of the integral image, or -1 for OpenCV default. 积分图深度，-1 表示使用 OpenCV 默认值。</param>
        /// <param name="sqdepth">The depth of the squared integral image, or -1 for OpenCV default. 平方积分图深度，-1 表示使用 OpenCV 默认值。</param>
        /// <exception cref="ArgumentNullException">Thrown when any matrix argument is null. 当任意矩阵参数为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Integral(Mat src, Mat sum, Mat sqsum, int sdepth = -1, int sqdepth = -1)
        {
            Integral2(src, sum, sqsum, sdepth, sqdepth);
        }

        /// <summary>
        /// Calculates the integral image and squared integral image.
        /// 计算积分图和平方积分图。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="sum">The output integral image. 输出积分图。</param>
        /// <param name="sqsum">The output squared integral image. 输出平方积分图。</param>
        /// <param name="sdepth">The depth of the integral image, or -1 for OpenCV default. 积分图深度，-1 表示使用 OpenCV 默认值。</param>
        /// <param name="sqdepth">The depth of the squared integral image, or -1 for OpenCV default. 平方积分图深度，-1 表示使用 OpenCV 默认值。</param>
        /// <exception cref="ArgumentNullException">Thrown when any matrix argument is null. 当任意矩阵参数为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Integral2(Mat src, Mat sum, Mat sqsum, int sdepth = -1, int sqdepth = -1)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(sum, nameof(sum));
            ValidateNotNull(sqsum, nameof(sqsum));

            NativeException.ThrowIfError(NativeMethods.ImgProcIntegral2(
                src.NativeHandle,
                sum.NativeHandle,
                sqsum.NativeHandle,
                sdepth,
                sqdepth));
        }

        /// <summary>
        /// Calculates the integral image, squared integral image, and tilted integral image.
        /// 计算积分图、平方积分图和倾斜积分图。
        /// </summary>
        /// <param name="src">The source image. 源图像。</param>
        /// <param name="sum">The output integral image. 输出积分图。</param>
        /// <param name="sqsum">The output squared integral image. 输出平方积分图。</param>
        /// <param name="tilted">The output tilted integral image. 输出倾斜积分图。</param>
        /// <param name="sdepth">The depth of the integral image, or -1 for OpenCV default. 积分图深度，-1 表示使用 OpenCV 默认值。</param>
        /// <param name="sqdepth">The depth of the squared integral image, or -1 for OpenCV default. 平方积分图深度，-1 表示使用 OpenCV 默认值。</param>
        /// <exception cref="ArgumentNullException">Thrown when any matrix argument is null. 当任意矩阵参数为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void Integral3(Mat src, Mat sum, Mat sqsum, Mat tilted, int sdepth = -1, int sqdepth = -1)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(sum, nameof(sum));
            ValidateNotNull(sqsum, nameof(sqsum));
            ValidateNotNull(tilted, nameof(tilted));

            NativeException.ThrowIfError(NativeMethods.ImgProcIntegral3(
                src.NativeHandle,
                sum.NativeHandle,
                sqsum.NativeHandle,
                tilted.NativeHandle,
                sdepth,
                sqdepth));
        }

        /// <summary>
        /// Calculates a distance transform for a binary image.
        /// 计算二值图像的距离变换。
        /// </summary>
        /// <param name="src">The source 8-bit single-channel binary image. 源 8 位单通道二值图像。</param>
        /// <param name="dst">The output distance image. 输出距离图像。</param>
        /// <param name="distanceType">The distance metric. 距离度量。</param>
        /// <param name="maskSize">The distance transform mask size. 距离变换掩码尺寸。</param>
        /// <param name="dstType">The destination matrix type, usually <c>CV_32F</c>. 目标矩阵类型，通常为 <c>CV_32F</c>。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maskSize"/> is unsupported. 当 <paramref name="maskSize"/> 不受支持时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void DistanceTransform(
            Mat src,
            Mat dst,
            DistanceTypes distanceType,
            DistanceTransformMasks maskSize,
            int dstType = MatType.CV_32F)
        {
            ValidateMatPair(src, dst);
            ValidateDistanceTransformMask(maskSize, nameof(maskSize));

            NativeException.ThrowIfError(NativeMethods.ImgProcDistanceTransform(
                src.NativeHandle,
                dst.NativeHandle,
                (int)distanceType,
                (int)maskSize,
                dstType));
        }

        /// <summary>
        /// Calculates a distance transform and labels for a binary image.
        /// 计算二值图像的距离变换和标签图。
        /// </summary>
        /// <param name="src">The source 8-bit single-channel binary image. 源 8 位单通道二值图像。</param>
        /// <param name="dst">The output distance image. 输出距离图像。</param>
        /// <param name="labels">The output label image. 输出标签图。</param>
        /// <param name="distanceType">The distance metric. 距离度量。</param>
        /// <param name="maskSize">The distance transform mask size. 距离变换掩码尺寸。</param>
        /// <param name="labelType">The label output type. 标签输出类型。</param>
        /// <exception cref="ArgumentNullException">Thrown when any matrix argument is null. 当任意矩阵参数为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maskSize"/> or <paramref name="labelType"/> is unsupported. 当 <paramref name="maskSize"/> 或 <paramref name="labelType"/> 不受支持时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void DistanceTransform(
            Mat src,
            Mat dst,
            Mat labels,
            DistanceTypes distanceType,
            DistanceTransformMasks maskSize,
            DistanceTransformLabelTypes labelType = DistanceTransformLabelTypes.CComp)
        {
            ValidateNotNull(src, nameof(src));
            ValidateNotNull(dst, nameof(dst));
            ValidateNotNull(labels, nameof(labels));
            ValidateDistanceTransformMask(maskSize, nameof(maskSize));
            ValidateDistanceTransformLabelType(labelType, nameof(labelType));

            NativeException.ThrowIfError(NativeMethods.ImgProcDistanceTransformWithLabels(
                src.NativeHandle,
                dst.NativeHandle,
                labels.NativeHandle,
                (int)distanceType,
                (int)maskSize,
                (int)labelType));
        }

        /// <summary>
        /// Flood-fills a connected component in an image.
        /// 在图像中泛洪填充一个连通区域。
        /// </summary>
        /// <param name="image">The image to modify. 要修改的图像。</param>
        /// <param name="seedPoint">The seed point. 种子点。</param>
        /// <param name="newVal">The fill color. 填充值。</param>
        /// <param name="rect">The bounding rectangle of the repainted area. 重新绘制区域的外接矩形。</param>
        /// <param name="loDiff">The lower brightness/color difference. 下界亮度或颜色差。</param>
        /// <param name="upDiff">The upper brightness/color difference. 上界亮度或颜色差。</param>
        /// <param name="flags">The flood-fill flags. 泛洪填充标志。</param>
        /// <returns>The number of repainted pixels. 被重新绘制的像素数量。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null. 当 <paramref name="image"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="flags"/> contains an unsupported connectivity or unknown high bits. 当 <paramref name="flags"/> 包含不支持的连通性或未知高位标志时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static int FloodFill(
            Mat image,
            Point seedPoint,
            Scalar newVal,
            out Rect rect,
            Scalar? loDiff = null,
            Scalar? upDiff = null,
            FloodFillFlags flags = FloodFillFlags.Connectivity4)
        {
            ValidateNotNull(image, nameof(image));
            ValidateFloodFillFlags(flags, nameof(flags));

            Scalar actualLoDiff = loDiff ?? new Scalar();
            Scalar actualUpDiff = upDiff ?? new Scalar();

            NativeException.ThrowIfError(NativeMethods.ImgProcFloodFill(
                image.NativeHandle,
                seedPoint.X,
                seedPoint.Y,
                newVal.V0,
                newVal.V1,
                newVal.V2,
                newVal.V3,
                out int rectX,
                out int rectY,
                out int rectWidth,
                out int rectHeight,
                actualLoDiff.V0,
                actualLoDiff.V1,
                actualLoDiff.V2,
                actualLoDiff.V3,
                actualUpDiff.V0,
                actualUpDiff.V1,
                actualUpDiff.V2,
                actualUpDiff.V3,
                (int)flags,
                out int filledCount));

            rect = new Rect(rectX, rectY, rectWidth, rectHeight);
            return filledCount;
        }

        /// <summary>
        /// Flood-fills a connected component using a mask.
        /// 使用掩码泛洪填充一个连通区域。
        /// </summary>
        /// <param name="image">The image to modify. 要修改的图像。</param>
        /// <param name="mask">The operation mask. 操作掩码。</param>
        /// <param name="seedPoint">The seed point. 种子点。</param>
        /// <param name="newVal">The fill color. 填充值。</param>
        /// <param name="rect">The bounding rectangle of the repainted area. 重新绘制区域的外接矩形。</param>
        /// <param name="loDiff">The lower brightness/color difference. 下界亮度或颜色差。</param>
        /// <param name="upDiff">The upper brightness/color difference. 上界亮度或颜色差。</param>
        /// <param name="flags">The flood-fill flags. 泛洪填充标志。</param>
        /// <returns>The number of repainted pixels. 被重新绘制的像素数量。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> or <paramref name="mask"/> is null. 当 <paramref name="image"/> 或 <paramref name="mask"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="flags"/> contains an unsupported connectivity or unknown high bits. 当 <paramref name="flags"/> 包含不支持的连通性或未知高位标志时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static int FloodFill(
            Mat image,
            Mat mask,
            Point seedPoint,
            Scalar newVal,
            out Rect rect,
            Scalar? loDiff = null,
            Scalar? upDiff = null,
            FloodFillFlags flags = FloodFillFlags.Connectivity4)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(mask, nameof(mask));
            ValidateFloodFillFlags(flags, nameof(flags));

            Scalar actualLoDiff = loDiff ?? new Scalar();
            Scalar actualUpDiff = upDiff ?? new Scalar();

            NativeException.ThrowIfError(NativeMethods.ImgProcFloodFillMask(
                image.NativeHandle,
                mask.NativeHandle,
                seedPoint.X,
                seedPoint.Y,
                newVal.V0,
                newVal.V1,
                newVal.V2,
                newVal.V3,
                out int rectX,
                out int rectY,
                out int rectWidth,
                out int rectHeight,
                actualLoDiff.V0,
                actualLoDiff.V1,
                actualLoDiff.V2,
                actualLoDiff.V3,
                actualUpDiff.V0,
                actualUpDiff.V1,
                actualUpDiff.V2,
                actualUpDiff.V3,
                (int)flags,
                out int filledCount));

            rect = new Rect(rectX, rectY, rectWidth, rectHeight);
            return filledCount;
        }

        /// <summary>
        /// Labels connected components in a binary image.
        /// 标记二值图像中的连通区域。
        /// </summary>
        /// <param name="image">The source binary image. 源二值图像。</param>
        /// <param name="labels">The output label image. 输出标签图。</param>
        /// <param name="connectivity">The pixel connectivity, usually 4 or 8. 像素连通性，通常为 4 或 8。</param>
        /// <param name="ltype">The label image type, usually <c>CV_32S</c> or <c>CV_16U</c>. 标签图类型，通常为 <c>CV_32S</c> 或 <c>CV_16U</c>。</param>
        /// <returns>The number of labels, including background. 标签数量，包含背景。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> or <paramref name="labels"/> is null. 当 <paramref name="image"/> 或 <paramref name="labels"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="connectivity"/> is not 4 or 8. 当 <paramref name="connectivity"/> 不是 4 或 8 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static int ConnectedComponents(Mat image, Mat labels, int connectivity = 8, int ltype = MatType.CV_32S)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(labels, nameof(labels));
            ValidateConnectivity(connectivity);

            NativeException.ThrowIfError(NativeMethods.ImgProcConnectedComponents(
                image.NativeHandle,
                labels.NativeHandle,
                connectivity,
                ltype,
                out int labelCount));

            return labelCount;
        }

        /// <summary>
        /// Labels connected components in a binary image with a selected algorithm.
        /// 使用指定算法标记二值图像中的连通区域。
        /// </summary>
        /// <param name="image">The source binary image. 源二值图像。</param>
        /// <param name="labels">The output label image. 输出标签图。</param>
        /// <param name="connectivity">The pixel connectivity, usually 4 or 8. 像素连通性，通常为 4 或 8。</param>
        /// <param name="ltype">The label image type, usually <c>CV_32S</c> or <c>CV_16U</c>. 标签图类型，通常为 <c>CV_32S</c> 或 <c>CV_16U</c>。</param>
        /// <param name="ccltype">The connected-components algorithm. 连通域标记算法。</param>
        /// <returns>The number of labels, including background. 标签数量，包含背景。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> or <paramref name="labels"/> is null. 当 <paramref name="image"/> 或 <paramref name="labels"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="connectivity"/> is not 4 or 8. 当 <paramref name="connectivity"/> 不是 4 或 8 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static int ConnectedComponentsWithAlgorithm(
            Mat image,
            Mat labels,
            int connectivity,
            int ltype,
            ConnectedComponentsAlgorithmsTypes ccltype)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(labels, nameof(labels));
            ValidateConnectivity(connectivity);
            ValidateConnectedComponentsAlgorithm(ccltype, nameof(ccltype));

            NativeException.ThrowIfError(NativeMethods.ImgProcConnectedComponentsWithAlgorithm(
                image.NativeHandle,
                labels.NativeHandle,
                connectivity,
                ltype,
                (int)ccltype,
                out int labelCount));

            return labelCount;
        }

        /// <summary>
        /// Labels connected components and calculates statistics and centroids.
        /// 标记连通区域并计算统计量和质心。
        /// </summary>
        /// <param name="image">The source binary image. 源二值图像。</param>
        /// <param name="labels">The output label image. 输出标签图。</param>
        /// <param name="stats">The output statistics matrix with columns from <see cref="ConnectedComponentsTypes"/>. 输出统计矩阵，列由 <see cref="ConnectedComponentsTypes"/> 指定。</param>
        /// <param name="centroids">The output centroid matrix. 输出质心矩阵。</param>
        /// <param name="connectivity">The pixel connectivity, usually 4 or 8. 像素连通性，通常为 4 或 8。</param>
        /// <param name="ltype">The label image type, usually <c>CV_32S</c> or <c>CV_16U</c>. 标签图类型，通常为 <c>CV_32S</c> 或 <c>CV_16U</c>。</param>
        /// <returns>The number of labels, including background. 标签数量，包含背景。</returns>
        /// <exception cref="ArgumentNullException">Thrown when any matrix argument is null. 当任意矩阵参数为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="connectivity"/> is not 4 or 8. 当 <paramref name="connectivity"/> 不是 4 或 8 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static int ConnectedComponentsWithStats(
            Mat image,
            Mat labels,
            Mat stats,
            Mat centroids,
            int connectivity = 8,
            int ltype = MatType.CV_32S)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(labels, nameof(labels));
            ValidateNotNull(stats, nameof(stats));
            ValidateNotNull(centroids, nameof(centroids));
            ValidateConnectivity(connectivity);

            NativeException.ThrowIfError(NativeMethods.ImgProcConnectedComponentsWithStats(
                image.NativeHandle,
                labels.NativeHandle,
                stats.NativeHandle,
                centroids.NativeHandle,
                connectivity,
                ltype,
                out int labelCount));

            return labelCount;
        }

        /// <summary>
        /// Labels connected components and calculates statistics and centroids with a selected algorithm.
        /// 使用指定算法标记连通区域并计算统计量和质心。
        /// </summary>
        /// <param name="image">The source binary image. 源二值图像。</param>
        /// <param name="labels">The output label image. 输出标签图。</param>
        /// <param name="stats">The output statistics matrix with columns from <see cref="ConnectedComponentsTypes"/>. 输出统计矩阵，列由 <see cref="ConnectedComponentsTypes"/> 指定。</param>
        /// <param name="centroids">The output centroid matrix. 输出质心矩阵。</param>
        /// <param name="connectivity">The pixel connectivity, usually 4 or 8. 像素连通性，通常为 4 或 8。</param>
        /// <param name="ltype">The label image type, usually <c>CV_32S</c> or <c>CV_16U</c>. 标签图类型，通常为 <c>CV_32S</c> 或 <c>CV_16U</c>。</param>
        /// <param name="ccltype">The connected-components algorithm. 连通域标记算法。</param>
        /// <returns>The number of labels, including background. 标签数量，包含背景。</returns>
        /// <exception cref="ArgumentNullException">Thrown when any matrix argument is null. 当任意矩阵参数为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="connectivity"/> is not 4 or 8. 当 <paramref name="connectivity"/> 不是 4 或 8 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static int ConnectedComponentsWithStatsWithAlgorithm(
            Mat image,
            Mat labels,
            Mat stats,
            Mat centroids,
            int connectivity,
            int ltype,
            ConnectedComponentsAlgorithmsTypes ccltype)
        {
            ValidateNotNull(image, nameof(image));
            ValidateNotNull(labels, nameof(labels));
            ValidateNotNull(stats, nameof(stats));
            ValidateNotNull(centroids, nameof(centroids));
            ValidateConnectivity(connectivity);
            ValidateConnectedComponentsAlgorithm(ccltype, nameof(ccltype));

            NativeException.ThrowIfError(NativeMethods.ImgProcConnectedComponentsWithStatsWithAlgorithm(
                image.NativeHandle,
                labels.NativeHandle,
                stats.NativeHandle,
                centroids.NativeHandle,
                connectivity,
                ltype,
                (int)ccltype,
                out int labelCount));

            return labelCount;
        }

        /// <summary>
        /// Equalizes the histogram of a grayscale image.
        /// 均衡化灰度图像的直方图。
        /// </summary>
        /// <param name="src">The source 8-bit single-channel image. 源 8 位单通道图像。</param>
        /// <param name="dst">The destination image. 目标图像。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void EqualizeHist(Mat src, Mat dst)
        {
            ValidateMatPair(src, dst);
            NativeException.ThrowIfError(NativeMethods.ImgProcEqualizeHist(src.NativeHandle, dst.NativeHandle));
        }

        /// <summary>
        /// Calculates the Harris corner response map.
        /// 计算 Harris 角点响应图。
        /// </summary>
        /// <param name="src">The source single-channel image. 源单通道图像。</param>
        /// <param name="dst">The destination response image. 目标响应图。</param>
        /// <param name="blockSize">The neighborhood size. 邻域尺寸。</param>
        /// <param name="ksize">The aperture parameter for Sobel derivatives. Sobel 导数孔径参数。</param>
        /// <param name="k">The Harris detector free parameter. Harris 检测器自由参数。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="blockSize"/> or <paramref name="ksize"/> is non-positive. 当 <paramref name="blockSize"/> 或 <paramref name="ksize"/> 非正时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void CornerHarris(
            Mat src,
            Mat dst,
            int blockSize,
            int ksize,
            double k,
            BorderTypes borderType = BorderTypes.Default)
        {
            ValidateMatPair(src, dst);
            ValidatePositive(blockSize, nameof(blockSize));
            ValidatePositive(ksize, nameof(ksize));

            NativeException.ThrowIfError(NativeMethods.ImgProcCornerHarris(
                src.NativeHandle,
                dst.NativeHandle,
                blockSize,
                ksize,
                k,
                (int)borderType));
        }

        /// <summary>
        /// Calculates the minimum eigenvalue of gradient covariance matrices for corner detection.
        /// 计算角点检测中梯度协方差矩阵的最小特征值。
        /// </summary>
        /// <param name="src">The source single-channel image. 源单通道图像。</param>
        /// <param name="dst">The destination response image. 目标响应图。</param>
        /// <param name="blockSize">The neighborhood size. 邻域尺寸。</param>
        /// <param name="ksize">The aperture parameter for Sobel derivatives. Sobel 导数孔径参数。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="blockSize"/> or <paramref name="ksize"/> is non-positive. 当 <paramref name="blockSize"/> 或 <paramref name="ksize"/> 非正时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void CornerMinEigenVal(
            Mat src,
            Mat dst,
            int blockSize,
            int ksize = 3,
            BorderTypes borderType = BorderTypes.Default)
        {
            ValidateMatPair(src, dst);
            ValidatePositive(blockSize, nameof(blockSize));
            ValidatePositive(ksize, nameof(ksize));

            NativeException.ThrowIfError(NativeMethods.ImgProcCornerMinEigenVal(
                src.NativeHandle,
                dst.NativeHandle,
                blockSize,
                ksize,
                (int)borderType));
        }

        /// <summary>
        /// Calculates eigenvalues and eigenvectors of gradient covariance matrices for corner detection.
        /// 计算角点检测中梯度协方差矩阵的特征值和特征向量。
        /// </summary>
        /// <param name="src">The source single-channel image. 源单通道图像。</param>
        /// <param name="dst">The destination image with six channels per pixel. 每个像素六通道的目标图像。</param>
        /// <param name="blockSize">The neighborhood size. 邻域尺寸。</param>
        /// <param name="ksize">The aperture parameter for Sobel derivatives. Sobel 导数孔径参数。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="blockSize"/> or <paramref name="ksize"/> is non-positive. 当 <paramref name="blockSize"/> 或 <paramref name="ksize"/> 非正时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void CornerEigenValsAndVecs(
            Mat src,
            Mat dst,
            int blockSize,
            int ksize,
            BorderTypes borderType = BorderTypes.Default)
        {
            ValidateMatPair(src, dst);
            ValidatePositive(blockSize, nameof(blockSize));
            ValidatePositive(ksize, nameof(ksize));

            NativeException.ThrowIfError(NativeMethods.ImgProcCornerEigenValsAndVecs(
                src.NativeHandle,
                dst.NativeHandle,
                blockSize,
                ksize,
                (int)borderType));
        }

        /// <summary>
        /// Calculates a feature map for corner detection.
        /// 计算用于角点检测的特征图。
        /// </summary>
        /// <param name="src">The source single-channel image. 源单通道图像。</param>
        /// <param name="dst">The destination response image. 目标响应图。</param>
        /// <param name="ksize">The aperture parameter for Sobel derivatives. Sobel 导数孔径参数。</param>
        /// <param name="borderType">The pixel extrapolation method. 像素边界外推方式。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="src"/> or <paramref name="dst"/> is null. 当 <paramref name="src"/> 或 <paramref name="dst"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="ksize"/> is non-positive. 当 <paramref name="ksize"/> 非正时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void PreCornerDetect(
            Mat src,
            Mat dst,
            int ksize,
            BorderTypes borderType = BorderTypes.Default)
        {
            ValidateMatPair(src, dst);
            ValidatePositive(ksize, nameof(ksize));

            NativeException.ThrowIfError(NativeMethods.ImgProcPreCornerDetect(
                src.NativeHandle,
                dst.NativeHandle,
                ksize,
                (int)borderType));
        }

        /// <summary>
        /// Finds contours in a binary image.
        /// 在二值图像中查找轮廓。
        /// </summary>
        /// <param name="image">The source image; OpenCV may modify it internally. 源图像；OpenCV 可能在内部修改它。</param>
        /// <param name="contours">The output contours. 输出轮廓。</param>
        /// <param name="hierarchy">The output contour hierarchy. 输出轮廓层级。</param>
        /// <param name="mode">The contour retrieval mode. 轮廓检索模式。</param>
        /// <param name="method">The contour approximation method. 轮廓近似方法。</param>
        /// <param name="offset">The optional offset added to all contour points. 添加到所有轮廓点的可选偏移。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> is null. 当 <paramref name="image"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void FindContours(
            Mat image,
            out Point[][] contours,
            out Vec4i[] hierarchy,
            RetrievalModes mode,
            ContourApproximationModes method,
            Point? offset = null)
        {
            ValidateNotNull(image, nameof(image));
            Point actualOffset = offset ?? new Point();

            NativeException.ThrowIfError(NativeMethods.ImgProcFindContoursCount(
                image.NativeHandle,
                (int)mode,
                (int)method,
                actualOffset.X,
                actualOffset.Y,
                out int contourCount,
                out int totalPointCount));

            if (contourCount <= 0 || totalPointCount <= 0)
            {
                contours = Array.Empty<Point[]>();
                hierarchy = Array.Empty<Vec4i>();
                return;
            }

            var contoursXy = new int[totalPointCount * 2];
            var contourLengths = new int[contourCount];
            var hierarchyValues = new int[contourCount * 4];

            NativeException.ThrowIfError(NativeMethods.ImgProcFindContoursFill(
                image.NativeHandle,
                (int)mode,
                (int)method,
                actualOffset.X,
                actualOffset.Y,
                contoursXy,
                totalPointCount,
                contourLengths,
                contourCount,
                hierarchyValues,
                contourCount,
                out int writtenContourCount,
                out int writtenPointCount));

            contours = FromFlatContours(contoursXy, contourLengths, writtenContourCount, writtenPointCount);
            hierarchy = FromInterleavedVec4iLocal(hierarchyValues, writtenContourCount);
        }

        /// <summary>
        /// Draws contour outlines or filled contours.
        /// 绘制轮廓边线或填充轮廓。
        /// </summary>
        /// <param name="image">The image to draw on. 要绘制的图像。</param>
        /// <param name="contours">The contours to draw. 要绘制的轮廓。</param>
        /// <param name="contourIdx">The contour index to draw, or -1 for all contours. 要绘制的轮廓索引，-1 表示全部轮廓。</param>
        /// <param name="color">The drawing color. 绘制颜色。</param>
        /// <param name="thickness">The line thickness, or a negative value to fill contours. 线条粗细；负数表示填充轮廓。</param>
        /// <param name="lineType">The line drawing algorithm. 线条绘制算法。</param>
        /// <param name="hierarchy">The optional contour hierarchy. 可选轮廓层级。</param>
        /// <param name="maxLevel">The maximum hierarchy level to draw. 要绘制的最大层级。</param>
        /// <param name="offset">The optional contour offset. 可选轮廓偏移。</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="image"/> or <paramref name="contours"/> is null. 当 <paramref name="image"/> 或 <paramref name="contours"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when contours contain null or empty entries. 当轮廓包含空引用或空轮廓时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static void DrawContours(
            Mat image,
            Point[][] contours,
            int contourIdx,
            Scalar color,
            int thickness = 1,
            LineTypes lineType = LineTypes.Line8,
            Vec4i[]? hierarchy = null,
            int maxLevel = int.MaxValue,
            Point? offset = null)
        {
            ValidateNotNull(image, nameof(image));
            FlattenContours(contours, nameof(contours), out int[] contoursXy, out int[] contourLengths, out int totalPointCount);

            if (contourLengths.Length == 0)
            {
                return;
            }

            int[] hierarchyValues = hierarchy == null ? Array.Empty<int>() : ToInterleavedVec4i(hierarchy, nameof(hierarchy));
            Point actualOffset = offset ?? new Point();

            NativeException.ThrowIfError(NativeMethods.ImgProcDrawContours(
                image.NativeHandle,
                contoursXy,
                contourLengths,
                contourLengths.Length,
                contourIdx,
                color.V0,
                color.V1,
                color.V2,
                color.V3,
                thickness,
                (int)lineType,
                hierarchyValues,
                hierarchy == null ? 0 : 1,
                maxLevel,
                actualOffset.X,
                actualOffset.Y));
        }

        /// <summary>
        /// Calculates moments of a raster image.
        /// 计算栅格图像的矩。
        /// </summary>
        /// <param name="array">The source raster image. 源栅格图像。</param>
        /// <param name="binaryImage">Whether all non-zero pixels are treated as one. 是否将所有非零像素视为一。</param>
        /// <returns>The calculated moments. 计算得到的矩。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="array"/> is null. 当 <paramref name="array"/> 为空时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Moments Moments(Mat array, bool binaryImage = false)
        {
            ValidateNotNull(array, nameof(array));
            var values = new double[MomentsValueCount];

            NativeException.ThrowIfError(NativeMethods.ImgProcMomentsMat(
                array.NativeHandle,
                binaryImage ? 1 : 0,
                values,
                values.Length));

            return CreateMoments(values);
        }

        /// <summary>
        /// Calculates moments of a contour represented by integer points.
        /// 计算由整数点表示的轮廓矩。
        /// </summary>
        /// <param name="points">The contour points. 轮廓点。</param>
        /// <param name="binaryImage">Whether all non-zero pixels are treated as one. 是否将所有非零像素视为一。</param>
        /// <returns>The calculated moments. 计算得到的矩。</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="points"/> is null. 当 <paramref name="points"/> 为空时抛出。</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空数组时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static Moments Moments(Point[] points, bool binaryImage = false)
        {
#if NETCOREAPP3_1_OR_GREATER
            if (points == null)
            {
                throw new ArgumentNullException(nameof(points));
            }

            return Moments(points.AsSpan(), binaryImage);
#else
            int[] pointsXy = ToInterleavedPointsLocal(points, nameof(points));
            var values = new double[MomentsValueCount];
            NativeException.ThrowIfError(NativeMethods.ImgProcMomentsPoints(pointsXy, points.Length, binaryImage ? 1 : 0, values, values.Length));
            return CreateMoments(values);
#endif
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>
        /// Calculates moments of a contour represented by a point span.
        /// 计算由点 Span 表示的轮廓矩。
        /// </summary>
        /// <param name="points">The contour points. 轮廓点。</param>
        /// <param name="binaryImage">Whether all non-zero pixels are treated as one. 是否将所有非零像素视为一。</param>
        /// <returns>The calculated moments. 计算得到的矩。</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="points"/> is empty. 当 <paramref name="points"/> 为空 Span 时抛出。</exception>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static unsafe Moments Moments(ReadOnlySpan<Point> points, bool binaryImage = false)
        {
            PointSetMarshaller.ValidateNotEmpty(points, nameof(points));
            ReadOnlySpan<int> pointsXy = PointSetMarshaller.AsInterleaved(points);
            Span<double> values = stackalloc double[MomentsValueCount];

            fixed (int* pointsPtr = pointsXy)
            fixed (double* valuesPtr = values)
            {
                NativeException.ThrowIfError(NativeMethods.ImgProcMomentsPointsPtr(
                    pointsPtr,
                    points.Length,
                    binaryImage ? 1 : 0,
                    valuesPtr,
                    MomentsValueCount));
            }

            return CreateMoments(values);
        }
#endif

        /// <summary>
        /// Calculates the seven Hu invariant moments.
        /// 计算七个 Hu 不变矩。
        /// </summary>
        /// <param name="moments">The source moments. 源矩。</param>
        /// <returns>The seven Hu invariant moments. 七个 Hu 不变矩。</returns>
        /// <exception cref="OpenCvException">Thrown when the native OpenCV operation fails. 当 native OpenCV 操作失败时抛出。</exception>
        public static double[] HuMoments(Moments moments)
        {
            double[] values = moments.ToArray();
            var hu = new double[HuMomentsValueCount];
            NativeException.ThrowIfError(NativeMethods.ImgProcHuMoments(values, values.Length, hu, hu.Length));
            return hu;
        }

        private static void ValidatePositive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Value must be positive.");
            }
        }

        private static void ValidateConnectivity(int connectivity)
        {
            if (connectivity != 4 && connectivity != 8)
            {
                throw new ArgumentOutOfRangeException(nameof(connectivity), "Connectivity must be 4 or 8.");
            }
        }

        private static void ValidateConnectedComponentsAlgorithm(ConnectedComponentsAlgorithmsTypes value, string parameterName)
        {
            if (value < ConnectedComponentsAlgorithmsTypes.Default || value > ConnectedComponentsAlgorithmsTypes.Spaghetti)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported connected-components algorithm.");
            }
        }

        private static void ValidateDistanceTransformMask(DistanceTransformMasks value, string parameterName)
        {
            if (value != DistanceTransformMasks.Precise
                && value != DistanceTransformMasks.Mask3
                && value != DistanceTransformMasks.Mask5)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported distance-transform mask size.");
            }
        }

        private static void ValidateDistanceTransformLabelType(DistanceTransformLabelTypes value, string parameterName)
        {
            if (value != DistanceTransformLabelTypes.CComp
                && value != DistanceTransformLabelTypes.Pixel)
            {
                throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported distance-transform label type.");
            }
        }

        private static void ValidateFloodFillFlags(FloodFillFlags flags, string parameterName)
        {
            const int connectivityMask = 0xff;
            const int maskFillValueMask = 0xff00;
            const int knownSpecialFlags = (int)(FloodFillFlags.FixedRange | FloodFillFlags.MaskOnly);
            const int knownFlagsMask = connectivityMask | maskFillValueMask | knownSpecialFlags;

            int value = (int)flags;
            int connectivity = value & connectivityMask;
            if (connectivity != 0 && connectivity != (int)FloodFillFlags.Connectivity4 && connectivity != (int)FloodFillFlags.Connectivity8)
            {
                throw new ArgumentOutOfRangeException(parameterName, flags, "Flood-fill connectivity must be 0, 4, or 8.");
            }

            if ((value & ~knownFlagsMask) != 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, flags, "Unknown flood-fill flag bits are not supported.");
            }
        }

        private static Moments CreateMoments(double[] values)
        {
            if (values.Length < MomentsValueCount)
            {
                throw new ArgumentException("Moments value buffer is too small.", nameof(values));
            }

            return new Moments(
                values[0],
                values[1],
                values[2],
                values[3],
                values[4],
                values[5],
                values[6],
                values[7],
                values[8],
                values[9],
                values[10],
                values[11],
                values[12],
                values[13],
                values[14],
                values[15],
                values[16],
                values[17],
                values[18],
                values[19],
                values[20],
                values[21],
                values[22],
                values[23]);
        }

#if NETCOREAPP3_1_OR_GREATER
        private static Moments CreateMoments(ReadOnlySpan<double> values)
        {
            if (values.Length < MomentsValueCount)
            {
                throw new ArgumentException("Moments value buffer is too small.", nameof(values));
            }

            return new Moments(
                values[0],
                values[1],
                values[2],
                values[3],
                values[4],
                values[5],
                values[6],
                values[7],
                values[8],
                values[9],
                values[10],
                values[11],
                values[12],
                values[13],
                values[14],
                values[15],
                values[16],
                values[17],
                values[18],
                values[19],
                values[20],
                values[21],
                values[22],
                values[23]);
        }
#endif

        private static Point[][] FromFlatContours(int[] contoursXy, int[] contourLengths, int contourCount, int totalPointCount)
        {
            var contours = new Point[contourCount][];
            int pointOffset = 0;

            for (int contourIndex = 0; contourIndex < contourCount; contourIndex++)
            {
                int pointCount = contourLengths[contourIndex];
                if (pointCount < 0 || pointOffset + pointCount > totalPointCount)
                {
                    throw new OpenCvException("Native contour output is inconsistent.");
                }

                var contour = new Point[pointCount];
                for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
                {
                    int xyOffset = (pointOffset + pointIndex) * 2;
                    contour[pointIndex] = new Point(contoursXy[xyOffset], contoursXy[xyOffset + 1]);
                }

                contours[contourIndex] = contour;
                pointOffset += pointCount;
            }

            return contours;
        }

        private static Vec4i[] FromInterleavedVec4iLocal(int[] values, int valueCount)
        {
            var result = new Vec4i[valueCount];
            for (int i = 0; i < valueCount; i++)
            {
                int offset = i * 4;
                result[i] = new Vec4i(values[offset], values[offset + 1], values[offset + 2], values[offset + 3]);
            }

            return result;
        }

        private static int[] ToInterleavedVec4i(Vec4i[] values, string parameterName)
        {
            if (values == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            var result = new int[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
            {
                int offset = i * 4;
                result[offset] = values[i].V0;
                result[offset + 1] = values[i].V1;
                result[offset + 2] = values[i].V2;
                result[offset + 3] = values[i].V3;
            }

            return result;
        }

        private static void FlattenContours(Point[][] contours, string parameterName, out int[] contoursXy, out int[] contourLengths, out int totalPointCount)
        {
            if (contours == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            totalPointCount = 0;
            contourLengths = new int[contours.Length];

            for (int contourIndex = 0; contourIndex < contours.Length; contourIndex++)
            {
                Point[] contour = contours[contourIndex];
                if (contour == null)
                {
                    throw new ArgumentException("Contour entry cannot be null.", parameterName);
                }

                if (contour.Length == 0)
                {
                    throw new ArgumentException("Contour entry cannot be empty.", parameterName);
                }

                contourLengths[contourIndex] = contour.Length;
                checked
                {
                    totalPointCount += contour.Length;
                }
            }

            contoursXy = new int[totalPointCount * 2];
            int pointOffset = 0;
            for (int contourIndex = 0; contourIndex < contours.Length; contourIndex++)
            {
                Point[] contour = contours[contourIndex];
                for (int pointIndex = 0; pointIndex < contour.Length; pointIndex++)
                {
                    int xyOffset = (pointOffset + pointIndex) * 2;
                    contoursXy[xyOffset] = contour[pointIndex].X;
                    contoursXy[xyOffset + 1] = contour[pointIndex].Y;
                }

                pointOffset += contour.Length;
            }
        }

        private static int[] ToInterleavedPointsLocal(Point[] points, string parameterName)
        {
            PointSetMarshaller.ValidateNotEmpty(points, parameterName);
            var values = new int[points.Length * 2];

            for (int i = 0; i < points.Length; i++)
            {
                int offset = i * 2;
                values[offset] = points[i].X;
                values[offset + 1] = points[i].Y;
            }

            return values;
        }
    }
}
