# Scenario Recipes / 场景路线

This page maps common product requirements to the shortest useful documentation path. Start with one workflow, keep the input and ownership rules explicit, and add broader modules only when the application needs them.

本页把常见产品需求映射到最短可用的文档路线。建议先完成一个端到端工作流，明确输入与所有权规则，只在应用确有需要时再引入更广泛的模块。

## Image Services / 图像服务

Use this route for upload processing, thumbnails, OCR preprocessing, document cleanup, quality inspection, and media conversion:

适用于上传处理、缩略图、OCR 预处理、文档清理、质量检测和媒体转换：

1. [Quick Start](quick-start.md) for package and runtime selection.
2. [Mat Object Model](mat-object-model.md) and [Mat Data Access](mat-data-access.md) for ownership and managed memory access.
3. [ImgCodecs Boundary](imgcodecs-boundary.md) for file and memory encoding.
4. [ImgProc Filter Transform Guide](imgproc-filter-transform-guide.md) and [ImgProc Segmentation Contours Features Guide](imgproc-segmentation-contours-features-guide.md).
5. [Photo Guide](photo-guide.md) for denoising, inpainting, HDR, and color operations.

The executable starting point is `tutorial image` in the [Tutorial Series](tutorial-series.md).

可执行起点是[系列教程](tutorial-series.md)中的 `tutorial image`。

## Visual Search And Registration / 视觉搜索与配准

Use ORB, SIFT, BRISK, KAZE, or AKAZE for keypoints and descriptors, then choose a matcher appropriate for binary or floating-point descriptors. Add geometry only after filtering matches.

使用 ORB、SIFT、BRISK、KAZE 或 AKAZE 生成 keypoint 与 descriptor，再根据二进制或浮点 descriptor 选择 matcher，并在筛选 match 后进入几何估计。

- [Features2D ORB Guide](features2d-orb-guide.md)
- [Features2D BRISK KAZE AKAZE Guide](features2d-brisk-kaze-akaze-guide.md)
- [Features2D Matcher Guide](features2d-matcher-guide.md)
- [Geometry Homogeneous And Epipolar Utilities Guide](geometry-homogeneous-epipolar-utilities-guide.md)
- [Calib3D Geometry Guide](calib3d-geometry-guide.md)

Run `tutorial features` for a model-free visual proof and `tutorial template` for the simpler response-map alternative.

运行 `tutorial features` 可得到不依赖模型的视觉证明；更简单的 response-map 路线可运行 `tutorial template`。

## Camera And Video Analytics / 相机与视频分析

Use [VideoIO Guide](videoio-guide.md) for capture/writer lifetime and backend properties. Build motion pipelines with [Video Motion Guide](video-motion-guide.md), [Video Background Subtractor Guide](video-background-subtractor-guide.md), and [OptFlow Guide](optflow-guide.md). Keep codec and camera availability as runtime capabilities rather than assumptions.

使用 [VideoIO Guide](videoio-guide.md) 处理 capture/writer 生命周期与 backend property；使用 [Video Motion Guide](video-motion-guide.md)、[Video Background Subtractor Guide](video-background-subtractor-guide.md) 和 [OptFlow Guide](optflow-guide.md) 构建运动分析。编解码器和相机可用性应作为 runtime capability 检测，而不能直接假设存在。

## Calibration, Pose, And Markers / 标定、位姿与标记

Start with the calibration pattern guide, choose the ordinary or fisheye model, and keep point/matrix shape validation at the application boundary. ArUco and ChArUco add marker-based detection and calibration workflows.

先从 calibration pattern 开始，根据镜头选择普通或 fisheye 模型，并在应用边界保留 point/matrix shape 校验；ArUco 和 ChArUco 可进一步提供基于 marker 的检测与标定工作流。

- [Calib3D Calibration Pattern Guide](calib3d-calibration-guide.md)
- [Calib3D Full Calibration Guide](calib3d-full-calibration-guide.md)
- [Calib3D Fisheye Calibration Guide](calib3d-fisheye-calibration-guide.md)
- [ArUco Guide](aruco-guide.md)
- [ChArUco Guide](charuco-guide.md)

## DNN Inference / DNN 推理

Use [DNN Net Guide](dnn-net-guide.md) for model loading, input blobs, forward output, backend/target selection, and lifetime. Use [DNN Net Advanced Guide](dnn-net-advanced-guide.md) for multiple inputs/outputs and dynamic shapes. The library does not bundle application models; pin model bytes and preprocessing metadata in the consuming application.

使用 [DNN Net Guide](dnn-net-guide.md) 处理模型加载、input blob、forward output、backend/target 选择和生命周期；多输入输出及动态 shape 见 [DNN Net Advanced Guide](dnn-net-advanced-guide.md)。库本身不内置应用模型，消费应用应固定模型字节及预处理元数据。

## Classical Machine Learning / 传统机器学习

[ML Guide](ml-guide.md) covers `TrainData`, KNN, SVM, Bayes, trees, boosting, EM, logistic regression, and neural-network models. `tutorial ml` demonstrates batch prediction and visualization with KNN. Keep feature scaling, label type, train/test separation, and model persistence under application control.

[ML Guide](ml-guide.md) 覆盖 `TrainData`、KNN、SVM、Bayes、trees、boosting、EM、logistic regression 和神经网络模型；`tutorial ml` 演示 KNN 批量预测与可视化。feature scaling、label type、train/test 划分和模型持久化应由应用控制。

## Detection, Tracking, And Faces / 检测、跟踪与人脸

Choose an input source first, then a detector, then a tracker. QR/barcode and classical HOG/cascade paths can run without DNN model infrastructure, while face recognition and saliency have their own training/model requirements.

先确定输入源，再选择 detector 和 tracker。QR/barcode 与传统 HOG/cascade 路线可以不依赖 DNN 模型基础设施；人脸识别和 saliency 则有各自的训练/模型要求。

- [ObjDetect Guide](objdetect-guide.md)
- [Tracking Guide](tracking-guide.md)
- [Face Guide](face-guide.md)
- [Saliency Guide](saliency-guide.md)

## Panorama And Composition / 全景与合成

Use [Stitching Stitcher Guide](stitching-stitcher-guide.md) for the high-level pipeline and [Stitching Runtime Guide](stitching-runtime-guide.md) for real image inputs. Advanced users can move to the structured detail APIs for matching, camera motion, exposure, seams, blending, and timelapse composition.

高层 pipeline 见 [Stitching Stitcher Guide](stitching-stitcher-guide.md)，真实图像输入见 [Stitching Runtime Guide](stitching-runtime-guide.md)。高级用户可继续使用 structured detail API 组合 matching、camera motion、exposure、seam、blending 和 timelapse。

## Desktop Preview Or Headless Output / 桌面预览或无头输出

Use [HighGui Guide](highgui-guide.md) only when the process has a compatible desktop UI backend and thread model. Servers, containers, tests, and promotional asset generation should prefer `ImWrite` or `ImEncode`, as the showcase does.

仅当进程具有兼容的桌面 UI backend 与线程模型时使用 [HighGui Guide](highgui-guide.md)。服务器、容器、测试和宣传素材生成应像 showcase 一样优先使用 `ImWrite` 或 `ImEncode`。

## Shipping Checklist / 交付清单

1. Pin the managed and runtime packages to the same four-part version.
2. Select the exact target RID and full/mini profile.
3. Run one representative workflow against package-owned native assets.
4. Record model, codec, GUI, camera, and optional-module requirements.
5. Dispose native-backed objects deterministically.
6. Review API/ABI changes before upgrading.

1. 将 managed 与 runtime 包固定为相同四段版本。
2. 选择精确 target RID 和 full/mini profile。
3. 使用 package-owned native assets 运行一个代表性工作流。
4. 记录模型、codec、GUI、相机和 optional module 要求。
5. 确定性释放 native-backed 对象。
6. 升级前审阅 API/ABI 差异。
