# Face Guide / Face 指南

`JYPPX.OpenCvSharp.Face` wraps traditional face-recognition, descriptor, facemark, and MACE objects from the local OpenCV 5.0.0 contrib `face` tree. The linked runtime module is the factual OpenCV 5.0.0 runtime artifact `opencv_face500.dll`.

`JYPPX.OpenCvSharp.Face` 封装来自本地 OpenCV 5.0.0 contrib `face` 树的传统人脸识别、描述子、facemark 和 MACE 对象。linked runtime 模块是事实性 OpenCV 5.0.0 runtime 产物 `opencv_face500.dll`。

## Scope / 范围

- Base recognizer: `FaceRecognizer` with train, update, predict, read/write, label info, labels-by-string, and threshold.
- Basic recognizers: `BasicFaceRecognizer`, `EigenFaceRecognizer`, and `FisherFaceRecognizer`.
- LBPH recognizer: `LBPHFaceRecognizer` with radius, neighbors, grid size, labels, and histograms.
- Prediction collection: `StandardCollector` with min label, min distance, and sorted/unsorted result arrays.
- Descriptor helper: `BIF` with band/rotation settings and feature computation.
- Face alignment: `Facemark`, `FacemarkTrain`, `FacemarkLBF`, `FacemarkLBFParams`, and `FacemarkFitResult`.
- Template/filter helper: `MACE` with create/load/salt/train/same/save.

- 基类识别器：`FaceRecognizer`，包含 train、update、predict、read/write、label info、labels-by-string 和 threshold。
- Basic recognizer：`BasicFaceRecognizer`、`EigenFaceRecognizer` 和 `FisherFaceRecognizer`。
- LBPH 识别器：`LBPHFaceRecognizer`，包含 radius、neighbors、grid size、labels 和 histograms。
- 预测收集：`StandardCollector`，包含 min label、min distance 和排序/未排序结果数组。
- 描述子辅助：`BIF`，包含 band/rotation 设置和特征计算。
- 人脸关键点：`Facemark`、`FacemarkTrain`、`FacemarkLBF`、`FacemarkLBFParams` 和 `FacemarkFitResult`。
- 模板/滤波器辅助：`MACE`，包含 create/load/salt/train/same/save。

## Runtime / 运行时

`face` is an optional OpenCV contrib module. Runtime staging should include the factual OpenCV 5.0.0 runtime artifact `opencv_face500.dll` when OpenCV was built with contrib. The module depends on factual OpenCV 5.0.0 runtime artifacts such as `opencv_core500.dll` and `opencv_imgproc500.dll`. Traditional recognizers and BIF use model-free smoke paths, while `FacemarkLBF` real fitting needs caller-supplied landmark model/training data plus a cascade detector path for training samples, and `MACE` real quality depends on caller-supplied same-size examples.

`face` 是可选 OpenCV contrib 模块。OpenCV 使用 contrib 构建时，runtime staging 应包含事实性 OpenCV 5.0.0 runtime 产物 `opencv_face500.dll`。该模块依赖 `opencv_core500.dll`、`opencv_imgproc500.dll` 等事实性 OpenCV 5.0.0 runtime 产物。传统识别器和 BIF 使用不依赖模型的 smoke 路径；`FacemarkLBF` 真实拟合需要调用方提供关键点模型/训练数据，并在训练样本路径中提供 cascade detector 路径；`MACE` 真实质量取决于调用方提供的同尺寸样本。

If the factual OpenCV 5.0.0 runtime artifact `opencv_face500.dll` is not linked, the exported native ABI remains present and managed calls report `NOT_LINKED`.

如果未链接事实性 OpenCV 5.0.0 runtime 产物 `opencv_face500.dll`，导出的 native ABI 仍存在，managed 调用会报告 `NOT_LINKED`。

## Recognizer Notes / 识别器说明

`Train` and `Update` accept `Mat[]` images plus `int[]` labels. The C ABI receives only flat handle and label buffers, then converts to OpenCV vectors inside native code; no `std::vector<cv::Mat>`, `cv::Ptr`, `InputArray`, or `OutputArray` crosses the exported boundary.

`Train` 和 `Update` 接收 `Mat[]` 图像和 `int[]` 标签。C ABI 只接收平铺 handle 与标签缓冲，并在 native 内部转换为 OpenCV vector；`std::vector<cv::Mat>`、`cv::Ptr`、`InputArray` 和 `OutputArray` 都不会穿过导出边界。

Eigen, Fisher, and LBPH are traditional recognizers. They normally expect grayscale training images with matching sizes. Tiny generated images in tests and samples only prove wrapper call paths and result shapes; they do not represent real face-recognition quality.

Eigen、Fisher 和 LBPH 都是传统识别器。它们通常要求灰度且尺寸一致的训练图像。测试和示例中的 tiny 合成图只证明 wrapper 调用路径和结果形状，不代表真实人脸识别质量。

`StandardCollector.GetResults` uses stable count/fill marshalling for `FacePredictionResult[]`; native `std::vector<std::pair<int,double>>` stays inside the module. In the local OpenCV 5.0.0 implementation, `BIF.Compute` expects a `CV_32F` image. `BIF` is a descriptor and feature helper, not a modern DNN face-recognition embedding model.

`StandardCollector.GetResults` 使用稳定的 count/fill 封送返回 `FacePredictionResult[]`；native `std::vector<std::pair<int,double>>` 留在模块内部。在本地 OpenCV 5.0.0 实现中，`BIF.Compute` 需要 `CV_32F` 图像。`BIF` 是描述子和特征辅助对象，不等价于现代 DNN 人脸识别 embedding 模型。

For face alignment details, see [Face Alignment Guide](face-alignment-guide.md). For the MACE workflow, see [Face MACE Guide](face-mace-guide.md).

Face alignment 细节见 [Face Alignment Guide](face-alignment-guide.md)。MACE 工作流见 [Face MACE Guide](face-mace-guide.md)。

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Face;

using Mat first = new Mat(32, 32, MatType.CV_8UC1, new Scalar(30));
using Mat second = new Mat(32, 32, MatType.CV_8UC1, new Scalar(180));
using Mat query = new Mat(32, 32, MatType.CV_8UC1, new Scalar(32));
using Mat bifInput = query.ConvertTo(MatType.CV_32FC1, 1.0 / 255.0);
using LBPHFaceRecognizer recognizer = LBPHFaceRecognizer.Create(radius: 1, neighbors: 8, gridX: 4, gridY: 4);
using StandardCollector collector = StandardCollector.Create();
using BIF bif = BIF.Create(numBands: 2, numRotations: 3);

recognizer.Train(new[] { first, second }, new[] { 10, 20 });
FacePrediction prediction = recognizer.PredictWithConfidence(query);
recognizer.Predict(query, collector);
FacePredictionResult[] results = collector.GetResults(sorted: true);
using Mat features = bif.Compute(bifInput);
```
