# First Release Overview / 首版概览

OpenCV CSharp API brings OpenCV 5.0.0 to .NET through a version-neutral `OpenCvSharp.*` object model and a stable `jyppx_ocv_*` native C ABI. The first release is intentionally organized around useful application workflows. It does not require every upstream OpenCV declaration to be wrapped before users can build real image, video, calibration, inference, and classical machine-learning applications.

OpenCV CSharp API 通过版本中立的 `OpenCvSharp.*` 对象模型和稳定的 `jyppx_ocv_*` native C ABI，把 OpenCV 5.0.0 带到 .NET。首版有意围绕实用工作流组织，不要求先封装完 OpenCV 的每一条上游声明，用户即可构建真实的图像、视频、标定、推理和传统机器学习应用。

## What You Can Build / 可以构建什么

| Scenario / 场景 | Main namespaces / 主要命名空间 | Typical work / 典型任务 |
| --- | --- | --- |
| Images / 图像 | `OpenCvSharp.Core`, `ImgCodecs`, `ImgProc`, `Photo` | Decode, encode, resize, filter, segment, contour, morphology, denoise, HDR |
| Features / 特征 | `Features2D`, `ImgHash`, `LineDescriptor` | ORB/SIFT/AKAZE, descriptors, matching, image similarity |
| Cameras and video / 相机与视频 | `VideoIO`, `Video`, `OptFlow`, `BgSegm` | Capture, write, motion, optical flow, background subtraction |
| Geometry / 几何 | `Geometry`, `Calib3D`, `ArUco` | Pose, calibration, stereo, homography, markers |
| AI and ML / AI 与机器学习 | `Dnn`, `ML`, `ObjDetect`, `Face` | ONNX inference, SVM/KNN/tree models, QR/barcode/HOG/face workflows |
| Composition / 合成 | `Stitching`, `AlphaMat`, `XImgProc`, `XPhoto` | Panorama, blending, matting, edge-aware filters |

The managed surface currently has 612 public types and 6,314 public/protected members across 41 namespaces. The full native profile exposes 2,656 stable C ABI functions, and every current native function is bound by managed interop. Those numbers describe the current release baseline, not a claim that every OpenCV module is complete.

当前托管层包含 612 个公共类型、6,314 个 public/protected 成员和 41 个命名空间。full native profile 暴露 2,656 个稳定 C ABI 函数，当前每个 native 函数均已有 managed interop 绑定。这些数字描述首版基线，不代表所有 OpenCV 模块都已完整封装。

## Compatibility Promise / 兼容性承诺

- Public managed API additions and removals are checked against `compatibility/managed-public-api-baseline.txt`.
- Native ABI order, names, and signatures are checked against the full and mini ABI manifests.
- The primary identities stay version-neutral: `OpenCvSharp.*`, `JYPPX.OpenCV.CSharp.API`, `JYPPX.OpenCV.Native`, and `jyppx_ocv_*`.
- Existing fixed-major names are compatibility aliases, not the recommended identity for new applications.
- A later release may add APIs and modules, but compatibility-breaking changes require an explicit version and migration decision.

- managed 公共 API 的增删由 `compatibility/managed-public-api-baseline.txt` 进行差异检查。
- native ABI 的顺序、名称和签名由 full/mini ABI manifest 约束。
- 主身份保持版本中立：`OpenCvSharp.*`、`JYPPX.OpenCV.CSharp.API`、`JYPPX.OpenCV.Native` 和 `jyppx_ocv_*`。
- 既有固定大版本名称仅作为兼容别名，不是新应用推荐使用的身份。
- 后续版本可以新增 API 与模块，但破坏兼容性的变更必须经过明确的版本和迁移决策。

See [API And ABI Compatibility Policy](api-abi-compatibility-policy.md) for the detailed rules.

详细规则见 [API And ABI Compatibility Policy](api-abi-compatibility-policy.md)。

## Install Shape / 安装方式

Applications reference the managed API package and one runtime package with the same normalized NuGet version. Choose the exact RID and either the full or mini profile. The first public candidate is `5.0.0-preview.1`; it remains a preview until publication authorization and public-feed verification are complete.

应用需要引用相同 NuGet 规范版本的 managed API 包与一个 runtime 包，并选择精确 RID 以及 full 或 mini profile。首个公开候选版本为 `5.0.0-preview.1`；在发布授权和公共源验证完成前，其状态仍为 preview。

```powershell
dotnet add package JYPPX.OpenCV.CSharp.API --version 5.0.0-preview.1
dotnet add package JYPPX.OpenCV.runtime.win-x64 --version 5.0.0-preview.1
```

The mini profile targets common `core,imgproc,imgcodecs,videoio` workflows and their OpenCV 5 geometry/flann runtime dependencies. DNN, calibration, features, photo, HighGui, and other extended modules require full. Runtime availability and support claims are governed by [Support And Lifecycle Policy](support-lifecycle-policy.md), not merely by the existence of a package ID.

mini profile 面向常用 `core,imgproc,imgcodecs,videoio` 工作流，以及 OpenCV 5 所需的 geometry/flann runtime 依赖。DNN、标定、特征、photo、HighGui 和其他扩展模块需要 full。runtime 可用性和支持声明以 [Support And Lifecycle Policy](support-lifecycle-policy.md) 为准，不能仅根据 package ID 是否存在来判断。

## Start With Evidence / 从可执行案例开始

Run the [Visual Showcase](visual-showcase.md) to generate six PNG files from synthetic, redistributable inputs. Then select a focused guide from [Scenario Recipes](scenario-recipes.md). The default `ConsoleSamples` path remains the broad smoke program; the `showcase` path is the short, presentation-ready entry point.

先运行 [Visual Showcase](visual-showcase.md)，使用可再分发的合成输入生成 6 个 PNG 文件，再从 [Scenario Recipes](scenario-recipes.md) 选择对应的专题指南。默认 `ConsoleSamples` 仍是广覆盖 smoke 程序，`showcase` 则是简短、适合展示的入口。

## Release Boundaries / 首版边界

Some algorithms require external models, training data, codecs, GUI support, or optional OpenCV build features. The wrapper does not silently download those inputs. Android and macOS are not promoted by the Windows/Linux evidence, and a package matrix entry alone is not a support guarantee. These boundaries are documented per guide so applications can detect and handle runtime capabilities explicitly.

部分算法依赖外部模型、训练数据、编解码器、GUI 支持或 OpenCV 可选构建能力，wrapper 不会静默下载这些输入。Windows/Linux 证据不能用于提升 Android 或 macOS 支持声明，package matrix 中存在某一行也不等于支持保证。各专题指南会明确说明这些边界，便于应用显式检测和处理 runtime capability。
