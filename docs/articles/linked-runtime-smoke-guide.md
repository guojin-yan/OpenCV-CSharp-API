# Linked Runtime Smoke Guide

This smoke path checks that the linked runtime for the current packaged OpenCV runtime identity, OpenCV 5.0.0, can execute representative module calls without requiring cameras, windows, or downloaded models by default.

此 smoke 流程用于检查当前打包 OpenCV runtime 身份 OpenCV 5.0.0 对应的 linked runtime 能执行代表性模块调用，同时默认不要求摄像头、窗口或下载模型。

Run linked smoke with the managed package `JYPPX.OpenCV.CSharp.API` and the matching full `JYPPX.OpenCV.runtime.<rid>` or mini `JYPPX.OpenCV.runtime.<rid>.mini` package on the same normalized NuGet package version. Choose the runtime package for the target RID/profile under test.

运行 linked smoke 时，managed 主包 `JYPPX.OpenCV.CSharp.API` 与匹配的 full `JYPPX.OpenCV.runtime.<rid>` 或 mini `JYPPX.OpenCV.runtime.<rid>.mini` 包应使用相同的四段 package version 元数据。请选择被测 target RID/profile 对应的 runtime 包。

Linux package smoke should use distro-specific Linux RID package IDs such as `JYPPX.OpenCV.runtime.ubuntu.22.04-x64`, `JYPPX.OpenCV.runtime.ubuntu.24.04-x64`, `JYPPX.OpenCV.runtime.debian.12-x64`, or `JYPPX.OpenCV.runtime.alpine.3.20-x64`; do not substitute generic `linux-x64` package IDs for distro-built runtime outputs. For external consumer projects, configure `RuntimeIdentifierGraphPath` to `packaging/runtime/runtime-distro-rid-graph.json` or an equivalent copied graph before restore when the selected distro RID is project-defined.

Linux package smoke 应使用 distro-specific Linux RID package ID，例如 `JYPPX.OpenCV.runtime.ubuntu.22.04-x64`、`JYPPX.OpenCV.runtime.ubuntu.24.04-x64`、`JYPPX.OpenCV.runtime.debian.12-x64` 或 `JYPPX.OpenCV.runtime.alpine.3.20-x64`；不要用通用 `linux-x64` package ID 替代按发行版构建的 runtime 输出。外部 consumer project 使用项目自定义 distro RID 时，如果 SDK 默认不认识所选 RID，请在 restore 前把 `RuntimeIdentifierGraphPath` 指向 `packaging/runtime/runtime-distro-rid-graph.json` 或复制后的等效 graph。

If no matching runtime package is available yet, build and stage a local native runtime with `Build-OpenCV.ps1` and `Stage-Runtime.ps1`, then run linked smoke with `OpenCvNativeRuntimeDir` pointing at that local native output.

如果 no matching runtime package is available yet，请使用 `Build-OpenCV.ps1` 和 `Stage-Runtime.ps1` 构建并暂存 local native runtime，然后用 `OpenCvNativeRuntimeDir` 指向该本地 native 输出运行 linked smoke。

Build and fallback setup details live in the [Linked Runtime Build Guide](linked-runtime-build-guide.md). Runtime package license layout is covered by [Runtime Licenses](runtime-licenses.md).

构建和 fallback 设置细节见 [Linked Runtime Build Guide](linked-runtime-build-guide.md)。runtime package license 布局见 [Runtime Licenses](runtime-licenses.md)。

For new build invocations, prefer the version-neutral runtime path/build property `OpenCvNativeRuntimeDir`. The older `OpenCv5SharpNativeRuntimeDir` property remains supported only as an existing-build-script compatibility alias.

新增构建调用优先使用版本中立 runtime path/build 属性 `OpenCvNativeRuntimeDir`。旧属性 `OpenCv5SharpNativeRuntimeDir` 仅作为既有 build script 的兼容别名保留。

For new opt-in smoke settings, prefer `OPENCV_CSHARP_*` environment variables. The older `OPENCV5SHARP_*` names remain supported only as existing-smoke-workflow compatibility aliases.

新增可选 smoke 设置优先使用 `OPENCV_CSHARP_*` 环境变量。旧的 `OPENCV5SHARP_*` 名称仅作为既有 smoke workflow 的兼容别名支持。

For a profile-level overview of default tests, ordinary native smoke, ConsoleSamples, HighGUI, model-backed paths, and unstable diagnostics, see [Smoke Profiles Guide](smoke-profiles-guide.md).

关于默认测试、普通 native smoke、ConsoleSamples、HighGUI、模型驱动路径和 unstable 诊断的 profile 级总览，见 [Smoke Profiles Guide](smoke-profiles-guide.md)。

## Default Smoke / 默认 Smoke

```powershell
dotnet build .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release
dotnet run --project .\samples\ConsoleSamples\ConsoleSamples.csproj -c Release
```

The default console sample exercises stable synthetic-frame paths for Core, ImgProc, and the current XStereo summary; AlphaMat and BioInspired are intentionally skipped in the default console path because their optional contrib algorithms can expose local OpenCV runtime-specific numeric or tiny-data boundaries. Longer historical sample coverage is available by setting `OPENCV_CSHARP_CONSOLE_EXTENDED=1`; that extended path touches additional optional contrib algorithms. BioInspired algorithm execution in the console sample additionally requires `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1`. The older `OPENCV5SHARP_CONSOLE_EXTENDED=1` and `OPENCV5SHARP_UNSTABLE_NATIVE_SMOKE=1` names remain accepted only as existing-console-smoke-workflow compatibility aliases.

默认 Console sample 会运行稳定的合成数据路径，覆盖 Core、ImgProc 以及当前 XStereo summary；AlphaMat 和 BioInspired 在默认路径中有意跳过，因为这些 optional contrib 算法可能暴露本地 OpenCV runtime 特有的数值或 tiny-data 边界。设置 `OPENCV_CSHARP_CONSOLE_EXTENDED=1` 可以启用更长的历史样例覆盖；该扩展路径会触及更多 optional contrib 算法。Console sample 中的 BioInspired 算法执行还需要额外设置 `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1`。旧的 `OPENCV5SHARP_CONSOLE_EXTENDED=1` 和 `OPENCV5SHARP_UNSTABLE_NATIVE_SMOKE=1` 名称仍仅作为既有 Console smoke workflow 的兼容别名使用。

## Smoke Tiers / Smoke 分层

- Default tests and samples cover managed shape, argument validation, disposed-state checks, `NOT_LINKED` boundaries, and stable synthetic calls only.
- `OPENCV_CSHARP_NATIVE_SMOKE=1` enables ordinary linked native smoke that should not use downloads, cameras, windows, or known process-crashing tiny-data paths.
- `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` enables experimental linked native paths that may expose local runtime crashes or fragile optional contrib behavior. Use it only when actively diagnosing those modules. `OPENCV5SHARP_UNSTABLE_NATIVE_SMOKE=1` remains accepted only as an existing-smoke-workflow compatibility alias.

- 默认测试和样例只覆盖 managed 形状、参数校验、disposed-state、`NOT_LINKED` 边界，以及已验证稳定的合成调用。
- `OPENCV_CSHARP_NATIVE_SMOKE=1` 启用普通 linked native smoke；该层不应依赖下载、摄像头、窗口，也不应触发已知可能导致进程级崩溃的 tiny-data 路径。
- `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` 启用实验性 linked native 路径，这些路径可能暴露本地 runtime crash 或可选 contrib 的脆弱行为。只有在主动诊断这些模块时才应启用。`OPENCV5SHARP_UNSTABLE_NATIVE_SMOKE=1` 仍仅作为既有 smoke workflow 的兼容别名接受。

## Optional Real Paths / 可选真实路径

- Set `OPENCV_CSHARP_NATIVE_SMOKE=1` for tests that call linked native algorithms. `OPENCV5SHARP_NATIVE_SMOKE=1` remains accepted only as an existing-smoke-workflow compatibility alias.
- Set `OPENCV_CSHARP_FACE_CASCADE` to a local `haarcascade_frontalface_default.xml` path when you want FacemarkLBF add-sample smoke to use a specific cascade; otherwise the tests try the local OpenCV install path and skip add-sample when no cascade is found. `OPENCV5SHARP_FACE_CASCADE` remains accepted only as an existing-smoke-workflow compatibility alias.
- XImgProc smoke paths use tiny grayscale, binary, color, disparity, sparse-match, edge, orientation, contour, RLE, segmentation, and complex matrices. They verify NiBlack/thinning helpers, edge-aware filters, superpixel label/mask shape, FastLineDetector, WLS disparity helpers, sparse interpolators, EdgeDrawing, EdgeBoxes, ridge/gradient utilities, Fourier descriptors, run-length morphology, ScanSegment, GraphSegmentation, Selective Search, and covariance-estimation call paths; they do not measure segmentation, stereo, interpolation, line-detection, descriptor, morphology, or proposal quality. `FastBilateralSolverFilter` is treated as unavailable when the linked OpenCV runtime reports that EIGEN support is required.
- OptFlow and BgSegm smoke paths use tiny generated frames. They verify linked calls, output shape, and background-subtractor object behavior; they do not measure optical-flow accuracy or background-model quality.
- Tracking smoke paths use tiny generated frames. They verify linked calls and output shape; they do not measure tracker quality.
- Face smoke paths use tiny generated grayscale images and landmarks. They verify LBPH/Eigen/Fisher/collector/BIF, FacemarkLBF parameter/add-sample shape, and MACE train/save/load/same call paths; they do not measure real face-recognition, alignment, or template quality.
- Saliency smoke paths use tiny generated images. They verify static and motion saliency output shape, binary-map calls, and ObjectnessBING parameter/cached-output shape; they do not measure saliency or objectness quality.
- Plot smoke paths use tiny generated vectors. They verify render output shape and styling call paths; they do not measure plotting quality or axis labeling fidelity.
- Shape smoke paths use tiny generated signatures, descriptors, and contours. They verify `EMDL1`, histogram cost-matrix, Shape Context, and Hausdorff call paths; they do not measure real matching quality.
- LineDescriptor smoke paths use tiny generated line images. They verify `BinaryDescriptor`, `BinaryDescriptorMatcher`, `DrawKeylines`, and `DrawLineMatches` call paths; default tests keep linked calls behind `OPENCV_CSHARP_NATIVE_SMOKE=1`. The older `OPENCV5SHARP_NATIVE_SMOKE=1` name remains accepted only as an existing-smoke-workflow compatibility alias.
- PhaseUnwrapping smoke paths use tiny `CV_32FC1` wrapped phase maps. They verify histogram unwrap and inverse reliability-map output shape; they do not measure real phase quality.
- StructuredLight smoke paths use generated Gray-code and sinusoidal patterns. They verify pattern image ownership, count, size, shadow-mask image generation, and basic projector-pixel call paths; real projector-camera decode requires caller-captured images.
- IntensityTransform smoke paths use tiny grayscale and BGR matrices. They verify log/gamma/autoscale/contrast output shape; BIMEF is treated as unavailable when the linked OpenCV runtime reports that EIGEN support is required.
- Fuzzy smoke paths use tiny `CV_32FC1`, `CV_8UC3`, and mask matrices. They verify kernel creation, filter, inpaint, F0/F1 process, inverse transform, polynomial matrix, and iteration call paths; they do not measure restoration quality.
- HFS smoke paths use tiny synthetic BGR images and CPU segmentation only. They verify output shape and property wiring; they do not measure segmentation quality or GPU availability.
- Reg smoke paths use tiny grayscale images and generated shifts. They verify map creation, warp/inverse-warp, mapper calculation, inverse-map output shape, and pyramid property wiring; they do not measure registration quality.
- SurfaceMatching smoke paths use tiny generated `Nx6 CV_32FC1` point clouds with normals. They verify ICP pose output and PPF train/match call paths; tiny clouds can hit OpenCV numeric or assertion boundaries.
- Rapid smoke paths use tiny generated square meshes, camera matrices, pose vectors, and edge images. They verify draw/run/tracker call paths; tiny meshes or sparse edges can hit OpenCV contour/assertion boundaries.
- AlphaMat smoke paths use tiny BGR images and trimaps. They verify `InfoFlow` output shape, not alpha quality.
- BioInspired linked object and algorithm paths are behind `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1`. Ordinary native smoke skips linked BioInspired calls because the linked runtime for the current packaged runtime identity, OpenCV 5.0.0, can hit process-level crashes even around tiny object setup/teardown and generated inputs.
- XStereo smoke paths use tiny shifted grayscale stereo pairs. They verify census, binary matcher, and quasi-dense call paths, not disparity quality.
- Empty DNN `Net` metadata and freshly created MCC `CChecker` defaults can be runtime-specific. Smoke tests verify stable calls and explicit set/get wiring, not exact default metadata or uninitialized checker target values.
- Build tests with the preferred `/p:OpenCvNativeRuntimeDir=<runtime-native-dir>` runtime path/build property before `--no-build` smoke runs so the test output contains the primary `JYPPX.OpenCV.Native.dll`, the explicit compatibility loader copy `OpenCv5Sharp.Native.dll` kept stable for already-compiled consumers, and the factual OpenCV DLLs. `/p:OpenCv5SharpNativeRuntimeDir=<runtime-native-dir>` is still accepted only as an existing-test-build-script compatibility alias.
- Set `OPENCV_CSHARP_DNN_MODEL`, `OPENCV_CSHARP_DNN_CONFIG`, and optionally `OPENCV_CSHARP_DNN_FRAMEWORK` for real DNN forward/profile smoke. The older `OPENCV5SHARP_DNN_*` names remain accepted only as existing-smoke-workflow compatibility aliases.
- Set `OPENCV_CSHARP_BRISQUE_MODEL` and `OPENCV_CSHARP_BRISQUE_RANGE` for real BRISQUE quality smoke. The older `OPENCV5SHARP_BRISQUE_*` names remain accepted only as existing-smoke-workflow compatibility aliases.
- Set `OPENCV_CSHARP_ML_MODEL_DIR` to choose where ML save/load smoke writes temporary model files; otherwise the system temp directory is used. `OPENCV5SHARP_ML_MODEL_DIR` remains accepted only as an existing-smoke-workflow compatibility alias.
- Set `OPENCV_CSHARP_STITCHING_IMAGES` to semicolon-separated image paths for real stitching sample input. `OPENCV5SHARP_STITCHING_IMAGES` remains accepted only as an existing-smoke-workflow compatibility alias.
- Set `OPENCV_CSHARP_HIGHGUI_SMOKE=1` only on a machine with an interactive desktop. `OPENCV5SHARP_HIGHGUI_SMOKE=1` remains accepted only as an existing-smoke-workflow compatibility alias.

- 设置 `OPENCV_CSHARP_NATIVE_SMOKE=1` 以运行调用 linked native 算法的测试。`OPENCV5SHARP_NATIVE_SMOKE=1` 仍仅作为既有 smoke workflow 的兼容别名使用。
- 设置 `OPENCV_CSHARP_FACE_CASCADE` 为本地 `haarcascade_frontalface_default.xml` 路径，可让 FacemarkLBF add-sample smoke 使用指定 cascade；未设置时测试会尝试本地 OpenCV install 路径，找不到 cascade 时跳过 add-sample。`OPENCV5SHARP_FACE_CASCADE` 仍仅作为既有 smoke workflow 的兼容别名使用。
- XImgProc smoke 路径使用 tiny 灰度、二值、彩色、disparity、稀疏匹配、edge、orientation、轮廓、RLE、分割和复数矩阵。它们验证 NiBlack/thinning helper、edge-aware filter、超像素 label/mask 形状、FastLineDetector、WLS disparity helper、稀疏插值器、EdgeDrawing、EdgeBoxes、ridge/gradient 工具、Fourier descriptor、run-length morphology、ScanSegment、GraphSegmentation、Selective Search 和 covariance estimation 调用路径，不衡量分割、stereo、插值、线段检测、descriptor、形态学或 proposal 质量。当 linked OpenCV runtime 报告需要 EIGEN 支持时，`FastBilateralSolverFilter` 会被视为不可用能力。
- OptFlow 和 BgSegm smoke 路径使用 tiny 合成帧。它们验证 linked 调用、输出形状和背景减除对象行为，不衡量光流精度或背景模型质量。
- Tracking smoke 路径使用 tiny 合成帧。它们验证 linked 调用和输出形状，不衡量 tracker 质量。
- Face smoke 路径使用 tiny 合成灰度图和关键点。它们验证 LBPH/Eigen/Fisher/collector/BIF、FacemarkLBF 参数/add-sample 形状，以及 MACE train/save/load/same 调用路径，不衡量真实人脸识别、关键点拟合或模板质量。
- Saliency smoke 路径使用 tiny 合成图。它们验证静态与运动显著性输出形状、binary-map 调用，以及 ObjectnessBING 参数/缓存输出形状，不衡量显著性或 objectness 质量。
- Plot smoke 路径使用 tiny 合成向量。它们验证 render 输出形状和样式设置调用路径，不衡量绘图质量或坐标轴标注精度。
- Shape smoke 路径使用 tiny 合成 signature、descriptor 和 contour。它们验证 `EMDL1`、histogram cost-matrix、Shape Context 和 Hausdorff 调用路径，不衡量真实匹配质量。
- LineDescriptor smoke 路径使用 tiny 合成线段图像。它们验证 `BinaryDescriptor`、`BinaryDescriptorMatcher`、`DrawKeylines` 和 `DrawLineMatches` 调用路径；默认测试仍通过 `OPENCV_CSHARP_NATIVE_SMOKE=1` 才运行 linked 调用。旧的 `OPENCV5SHARP_NATIVE_SMOKE=1` 名称仍仅作为既有 smoke workflow 的兼容别名使用。
- PhaseUnwrapping smoke 路径使用 tiny `CV_32FC1` wrapped phase map。它们验证 histogram unwrap 和 inverse reliability-map 输出形状，不衡量真实相位质量。
- StructuredLight smoke 路径使用生成的 Gray-code 和正弦图案。它们验证 pattern 图像所有权、数量、尺寸、shadow-mask 图像生成和基础 projector-pixel 调用路径；真实 projector-camera decode 需要调用方采集的图像。
- IntensityTransform smoke 路径使用 tiny 灰度和 BGR 矩阵。它们验证 log/gamma/autoscale/contrast 输出形状；当 linked OpenCV runtime 报告需要 EIGEN 支持时，BIMEF 会被视为不可用能力。
- Fuzzy smoke 路径使用 tiny `CV_32FC1`、`CV_8UC3` 和 mask 矩阵。它们验证 kernel 创建、filter、inpaint、F0/F1 process、inverse transform、polynomial matrix 和 iteration 调用路径，不衡量修复质量。
- HFS smoke 路径使用 tiny 合成 BGR 图像，并且只使用 CPU 分割。它们验证输出形状和属性接线，不衡量分割质量或 GPU 可用性。
- Reg smoke 路径使用 tiny 灰度图和合成平移。它们验证 map 创建、warp/inverse-warp、mapper calculate、inverse-map 输出形状和 pyramid 属性接线，不衡量配准质量。
- SurfaceMatching smoke 路径使用 tiny 生成的 `Nx6 CV_32FC1` 带法线点云。它们验证 ICP pose 输出和 PPF train/match 调用路径；tiny 点云可能触发 OpenCV 数值或断言边界。
- Rapid smoke 路径使用 tiny 生成的方形网格、相机矩阵、位姿向量和边缘图。它们验证 draw/run/tracker 调用路径；tiny 网格或稀疏边缘可能触发 OpenCV contour/assertion 边界。
- AlphaMat smoke 路径使用 tiny BGR 图像和 trimap。它们验证 `InfoFlow` 输出形状，不衡量 alpha 质量。
- BioInspired linked object 和算法路径都放在 `OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE=1` 后面。普通 native smoke 会跳过 linked BioInspired 调用，因为当前打包 runtime 身份 OpenCV 5.0.0 对应的 linked runtime 即使在 tiny object setup/teardown 和合成输入附近也可能触发进程级崩溃。
- XStereo smoke 路径使用 tiny 位移灰度 stereo pair。它们验证 census、binary matcher 和 quasi-dense 调用路径，不衡量视差质量。
- 空 DNN `Net` metadata 和刚创建的 MCC `CChecker` 默认值可能随 runtime 而不同。Smoke 测试验证调用稳定性和显式 set/get 接线，而不要求精确默认 metadata 或未初始化 checker target 值。
- 在 `--no-build` smoke 运行前使用首选 runtime path/build 属性 `/p:OpenCvNativeRuntimeDir=<runtime-native-dir>` 构建测试项目，确保测试输出目录包含主 loader `JYPPX.OpenCV.Native.dll`、为已编译消费者保持稳定的明确兼容 loader 副本 `OpenCv5Sharp.Native.dll` 和事实性 OpenCV DLL。`/p:OpenCv5SharpNativeRuntimeDir=<runtime-native-dir>` 仍仅作为既有 test build script 的兼容别名接受。
- 设置 `OPENCV_CSHARP_DNN_MODEL`、`OPENCV_CSHARP_DNN_CONFIG`，并按需设置 `OPENCV_CSHARP_DNN_FRAMEWORK` 以运行真实 DNN forward/profile smoke。旧的 `OPENCV5SHARP_DNN_*` 名称仍仅作为既有 smoke workflow 的兼容别名使用。
- 设置 `OPENCV_CSHARP_BRISQUE_MODEL` 和 `OPENCV_CSHARP_BRISQUE_RANGE` 以运行真实 BRISQUE quality smoke。旧的 `OPENCV5SHARP_BRISQUE_*` 名称仍仅作为既有 smoke workflow 的兼容别名使用。
- 设置 `OPENCV_CSHARP_ML_MODEL_DIR` 可指定 ML save/load smoke 写入临时模型文件的位置；未设置时使用系统临时目录。`OPENCV5SHARP_ML_MODEL_DIR` 仍仅作为既有 smoke workflow 的兼容别名使用。
- 设置 `OPENCV_CSHARP_STITCHING_IMAGES` 为分号分隔图像路径，以运行真实 stitching 示例输入。`OPENCV5SHARP_STITCHING_IMAGES` 仍仅作为既有 smoke workflow 的兼容别名使用。
- 只有在具备交互式桌面的机器上才设置 `OPENCV_CSHARP_HIGHGUI_SMOKE=1`。`OPENCV5SHARP_HIGHGUI_SMOKE=1` 仍仅作为既有 smoke workflow 的兼容别名使用。

## Expected DLLs / 预期 DLL

These entries combine the version-neutral primary loader, the explicit compatibility loader copy kept stable for already-compiled consumers, and factual OpenCV runtime artifacts for the current packaged runtime identity, OpenCV 5.0.0.

这些条目包含版本中立的主 loader、为已编译消费者保持稳定的明确兼容 loader 副本，以及当前打包 runtime 身份 OpenCV 5.0.0 的事实性 OpenCV runtime 产物。

- `JYPPX.OpenCV.Native.dll` (primary loader)
- `OpenCv5Sharp.Native.dll` (explicit compatibility loader copy kept stable for already-compiled consumers)
- factual OpenCV 5.0.0 runtime artifact `opencv_core500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_imgproc500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_imgcodecs500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_video500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_dnn500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_highgui500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_stitching500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_ptcloud500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_ml500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_img_hash500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_optflow500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_ximgproc500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_bgsegm500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_tracking500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_face500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_saliency500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_quality500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_xphoto500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_plot500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_shape500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_line_descriptor500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_phase_unwrapping500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_structured_light500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_intensity_transform500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_fuzzy500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_hfs500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_reg500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_surface_matching500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_rapid500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_alphamat500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_bioinspired500.dll`
- factual OpenCV 5.0.0 runtime artifact `opencv_xstereo500.dll`
- Other factual OpenCV 5.0.0 runtime artifact DLLs used by the wider package, such as `opencv_videoio500.dll`, `opencv_objdetect500.dll`, `opencv_photo500.dll`, `opencv_calib500.dll`, and optional contrib DLLs.

- `JYPPX.OpenCV.Native.dll`（主 loader）
- `OpenCv5Sharp.Native.dll`（为已编译消费者保持稳定的明确兼容 loader 副本）
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_core500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_imgproc500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_imgcodecs500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_video500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_dnn500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_highgui500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_stitching500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_ptcloud500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_ml500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_img_hash500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_optflow500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_ximgproc500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_bgsegm500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_tracking500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_face500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_saliency500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_quality500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_xphoto500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_plot500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_shape500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_line_descriptor500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_phase_unwrapping500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_structured_light500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_intensity_transform500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_fuzzy500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_hfs500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_reg500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_surface_matching500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_rapid500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_alphamat500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_bioinspired500.dll`
- 事实性 OpenCV 5.0.0 runtime 产物 `opencv_xstereo500.dll`
- 更大能力包使用的其他事实性 OpenCV 5.0.0 runtime 产物 DLL，例如 `opencv_videoio500.dll`、`opencv_objdetect500.dll`、`opencv_photo500.dll`、`opencv_calib500.dll` 和可选 contrib DLL。

If a module DLL is missing, the managed API shape remains stable but calls return a clear `OpenCvException` with `NOT_LINKED`.

如果缺少某个模块 DLL，managed API 形状仍保持稳定，但调用会返回带有明确 `NOT_LINKED` 信息的 `OpenCvException`。
