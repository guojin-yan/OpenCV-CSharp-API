# DNN Super Resolution Upstream Map / DNN Super Resolution 上游映射

This document defines the measured scope for the OpenCV 5.0.0 contrib DNN Super Resolution map. It is a source/header evidence record, not a claim that the package implements every OpenCV or contrib API.

本文定义 OpenCV 5.0.0 contrib DNN Super Resolution 映射的测量范围。它是源码/头文件证据记录，不代表本包实现了 OpenCV 或 contrib 的全部 API。

## Scope / 范围

The map is generated from the pinned parser closure of `opencv-source/opencv_contrib-5.0.0/modules/dnn_superres/include/opencv2/dnn_superres.hpp`. It contains 10 parser declarations: one `DnnSuperResImpl` class metadata row and nine callable rows (`create`, model loading/configuration, backend/target selection, upsampling, and scale/algorithm queries).

映射来自固定版本 parser 对 `opencv-source/opencv_contrib-5.0.0/modules/dnn_superres/include/opencv2/dnn_superres.hpp` 的闭包解析，共 10 个声明：一个 `DnnSuperResImpl` 类元数据行，以及九个 callable 行（创建、模型加载/配置、backend/target 选择、上采样和 scale/algorithm 查询）。

The checked artifacts are:

```text
compatibility/dnn_superres-upstream-raw.json
compatibility/dnn_superres-upstream-classifications.json
compatibility/dnn_superres-upstream-map.txt
compatibility/dnn_superres-upstream-summary.json
compatibility/dnn_superres-implemented-families.json
```

已审计的产物为上述五个文件。

## Classification boundary / 分类边界

All nine callable rows are `intentionally-omitted`, with zero native entrypoints, zero managed members, and zero unexplained missing rows. The omission is deliberate: the current package has no native DnnSuperres wrapper entrypoint or managed public owner, and a safe future wrapper would need an explicit contract for caller-supplied model files, backend/target policy, and output ownership.

九个 callable 行全部标记为 `intentionally-omitted`，native entrypoint 和 managed member 都为零，也没有未解释的 missing 行。该 omission 是有意的：当前包没有 DnnSuperres native wrapper entrypoint 或 managed public owner；未来若增加 wrapper，必须先明确调用方提供的模型文件、backend/target 策略和输出所有权契约。

The map therefore does not infer repository-wide contrib parity, does not bundle model assets, and does not promise deterministic super-resolution output. The optional DNN Super Resolution module may also be unavailable in a particular OpenCV build.

因此，该映射不推断整个 contrib 仓库的 parity，不打包模型资产，也不承诺确定性的超分辨率输出；具体 OpenCV 构建也可能不包含可选 DNN Super Resolution 模块。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-DnnSuperresUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The semantic guard checks the pinned header closure, the 10/9/1 declaration partition, the nine reviewed omission reasons, the absence of native/managed DnnSuperres evidence, and the external model/dependency boundary.

语义守卫会检查固定头文件闭包、10/9/1 声明分区、九个已审阅 omission 原因、native/managed DnnSuperres 证据为空，以及外部模型/依赖边界。
