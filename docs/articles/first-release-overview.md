# First Release Overview / 首版概览

OpenCV CSharp API brings OpenCV 5.0.0 to .NET through a version-neutral `JYPPX.OpenCvSharp.*` object model and a stable `jyppx_ocv_*` native C ABI. The first release is intentionally organized around useful application workflows. It does not require every upstream OpenCV declaration to be wrapped before users can build real image, video, calibration, inference, and classical machine-learning applications.

OpenCV CSharp API 通过版本中立的 `JYPPX.OpenCvSharp.*` 对象模型和稳定的 `jyppx_ocv_*` native C ABI，把 OpenCV 5.0.0 带到 .NET。首版有意围绕实用工作流组织，不要求先封装完 OpenCV 的每一条上游声明，用户即可构建真实的图像、视频、标定、推理和传统机器学习应用。

## What You Can Build / 可以构建什么

| Scenario / 场景 | Main namespaces / 主要命名空间 | Typical work / 典型任务 |
| --- | --- | --- |
| Images / 图像 | `JYPPX.OpenCvSharp.Core`, `ImgCodecs`, `ImgProc`, `Photo` | Decode, encode, resize, filter, segment, contour, morphology, denoise, HDR |
| Features / 特征 | `Features2D`, `ImgHash`, `LineDescriptor` | ORB/SIFT/AKAZE, descriptors, matching, image similarity |
| Cameras and video / 相机与视频 | `VideoIO`, `Video`, `OptFlow`, `BgSegm` | Capture, write, motion, optical flow, background subtraction |
| Geometry / 几何 | `Geometry`, `Calib3D`, `ArUco` | Pose, calibration, stereo, homography, markers |
| AI and ML / AI 与机器学习 | `Dnn`, `ML`, `ObjDetect`, `Face` | ONNX inference, SVM/KNN/tree models, QR/barcode/HOG/face workflows |
| Composition / 合成 | `Stitching`, `AlphaMat`, `XImgProc`, `XPhoto` | Panorama, blending, matting, edge-aware filters |

The managed surface currently has 611 public types and 6,561 public/protected members across 41 namespaces rooted at `JYPPX.OpenCvSharp`. The full native profile exposes 2,657 stable C ABI functions, and every current native function is bound by managed interop. Those numbers describe the current release baseline, not a claim that every OpenCV module is complete.

当前托管层包含 611 个公共类型、6,561 个 public/protected 成员和 `JYPPX.OpenCvSharp` 根下的 41 个命名空间。full native profile 暴露 2,657 个稳定 C ABI 函数，当前每个 native 函数均已有 managed interop 绑定。这些数字描述当前发布基线，不代表所有 OpenCV 模块都已完整封装。

## Compatibility Promise / 兼容性承诺

- Public managed API additions and removals are checked against `compatibility/managed-public-api-baseline.txt`.
- Native ABI order, names, and signatures are checked against the full and mini ABI manifests.
- The primary identities stay version-neutral: `JYPPX.OpenCvSharp.*`, `JYPPX.OpenCV.CSharp.API`, `JYPPX.OpenCV.Native`, and `jyppx_ocv_*`.
- Existing fixed-major names are compatibility aliases, not the recommended identity for new applications.
- A later release may add APIs and modules, but compatibility-breaking changes require an explicit version and migration decision.

- managed 公共 API 的增删由 `compatibility/managed-public-api-baseline.txt` 进行差异检查。
- native ABI 的顺序、名称和签名由 full/mini ABI manifest 约束。
- 主身份保持版本中立：`JYPPX.OpenCvSharp.*`、`JYPPX.OpenCV.CSharp.API`、`JYPPX.OpenCV.Native` 和 `jyppx_ocv_*`。
- 既有固定大版本名称仅作为兼容别名，不是新应用推荐使用的身份。
- 后续版本可以新增 API 与模块，但破坏兼容性的变更必须经过明确的版本和迁移决策。

See [API And ABI Compatibility Policy](api-abi-compatibility-policy.md) for the detailed rules.

详细规则见 [API And ABI Compatibility Policy](api-abi-compatibility-policy.md)。

## Install Shape / 安装方式

Applications reference the managed API package and one runtime package with the same normalized NuGet version. Choose the exact RID and either the full or mini profile. The first public preview was `5.0.0-preview.1`; the corrected ML runtime and expanded sample/tutorial set are carried by the next immutable preview candidate.

应用需要引用相同 NuGet 规范版本的 managed API 包与一个 runtime 包，并选择精确 RID 以及 full 或 mini profile。首个公开预览版是 `5.0.0-preview.1`；修正后的 ML runtime 与扩展案例/教程集合由下一个不可覆盖的 preview candidate 承载。

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API --prerelease
dotnet add package JYPPX.OpenCV.runtime.win-x64 --prerelease
```

The mini profile targets common `core,imgproc,imgcodecs,videoio` workflows and their OpenCV 5 geometry/flann runtime dependencies. DNN, calibration, features, photo, HighGui, and other extended modules require full. Runtime availability and support claims are governed by [Support And Lifecycle Policy](support-lifecycle-policy.md), not merely by the existence of a package ID.

mini profile 面向常用 `core,imgproc,imgcodecs,videoio` 工作流，以及 OpenCV 5 所需的 geometry/flann runtime 依赖。DNN、标定、特征、photo、HighGui 和其他扩展模块需要 full。runtime 可用性和支持声明以 [Support And Lifecycle Policy](support-lifecycle-policy.md) 为准，不能仅根据 package ID 是否存在来判断。

## Start With Evidence / 从可执行案例开始

Run the original six-part [Tutorial Series](tutorial-series.md) or choose a capability from the expanded [Example Catalog](example-catalog.md). The grouped cases generate deterministic PNG files from redistributable inputs plus an application-selected CJK font where needed. They include direct Chinese rendering through OpenCV 5 `putText`. Then select a product route from [Scenario Recipes](scenario-recipes.md). The default `ConsoleSamples` path remains the broad smoke program; `tutorial` is the presentation-ready entry point and `showcase` remains its compatibility alias.

可以先运行包含 6 个案例的[系列教程](tutorial-series.md)，也可以从扩展后的[案例目录](example-catalog.md)按能力选择案例。分组案例使用可再分发的确定性输入，并在需要时使用应用选择的中文字体生成 PNG，其中包含通过 OpenCV 5 `putText` 直接绘制中文。再从 [Scenario Recipes](scenario-recipes.md) 选择产品路线。默认 `ConsoleSamples` 仍是广覆盖 smoke 程序；`tutorial` 是适合展示的入口，`showcase` 继续作为兼容别名。

## Release Boundaries / 首版边界

Some algorithms require external models, training data, codecs, GUI support, or optional OpenCV build features. The wrapper does not silently download those inputs. Android x64/x86 require fresh NDK, package, APK, and native-loading evidence for the single-loader payload before promotion; retired dual-loader runs do not establish support for the new source. Android ARM/ARM64 and macOS are not promoted by x64/x86 evidence. A package matrix entry alone is not a support guarantee.

部分算法依赖外部模型、训练数据、编解码器、GUI 支持或 OpenCV 可选构建能力，wrapper 不会静默下载这些输入。Android x64/x86 必须为单加载器产物重新取得独立的 NDK、package、APK 与原生加载证据，已淘汰的双加载器运行不能证明新源码受支持；x64/x86 证据也不会自动提升 Android ARM/ARM64 或 macOS。package matrix 中存在某一行不等于支持保证。
