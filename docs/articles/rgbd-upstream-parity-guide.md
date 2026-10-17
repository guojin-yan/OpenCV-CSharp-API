# RGBD Upstream Map / RGBD 上游映射

This guide records the scoped OpenCV 5.0.0 contrib `rgbd` parser/header boundary. It is evidence for the six-header public closure, not a claim of repository-wide contrib parity.

本指南记录固定 OpenCV 5.0.0 contrib `rgbd` 的六个头文件 parser/header 边界证据，不宣称整个 contrib 仓库已经 parity。

## Scope / 范围

The map covers `opencv2/rgbd.hpp` and its five included headers: `colored_kinfu.hpp`, `dynafu.hpp`, `kinfu.hpp`, `large_kinfu.hpp`, and `linemod.hpp`. The parser emits 105 declarations: 17 class metadata rows and 88 callables. The current package has no native RGBD wrapper or managed RGBD owner for these declarations, so all callables are explicitly classified as intentionally omitted; no ABI/API parity is inferred.

映射覆盖 `opencv2/rgbd.hpp` 及其包含的五个头文件：`colored_kinfu.hpp`、`dynafu.hpp`、`kinfu.hpp`、`large_kinfu.hpp` 和 `linemod.hpp`。parser 发出 105 个声明：17 个 class metadata 和 88 个 callable。当前包没有这些声明对应的 RGBD native wrapper 或 managed owner，因此所有 callable 都明确标为 intentional omission，不推断 ABI/API parity。

The module consumes caller-supplied depth/RGB sensor data and may use OpenCL according to the upstream runtime policy. Upstream also documents KinectFusion patent and `OPENCV_ENABLE_NONFREE` restrictions; those legal/build decisions remain outside this package.

该模块消费调用方提供的深度/RGB 传感器数据，并可按 upstream runtime policy 使用 OpenCL。upstream 还记录了 KinectFusion 的专利和 `OPENCV_ENABLE_NONFREE` 限制；这些法律/构建决策不属于本包范围。

## Verification / 验证

```powershell
pwsh -NoProfile -File .\scripts\Test-RgbdUpstreamMap.ps1
pwsh -NoProfile -File .\scripts\Test-UpstreamMapCatalog.ps1
```

The focused guard verifies the six-header closure, the 105-row declaration partition, 88 intentional omissions with zero native/managed RGBD evidence, both external dependency notes, and the prohibition on repository-wide parity claims.

专项 guard 会检查六个头文件 closure、105 行声明分区、88 个 intentional omission、RGBD native/managed 证据为零、两个外部依赖说明，以及禁止 repository-wide parity 声明。
