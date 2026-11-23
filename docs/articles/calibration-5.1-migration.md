# Calibration flags in OpenCV 5.1

`CalibrationFlags.DisableSchurComplement` now has value `1 << 27` (134217728), matching OpenCV 5.1. Its former value, `1 << 18` (262144), also denotes `TiltedModel`. Passing the old constant to a 5.1 runtime selected tilted-sensor distortion instead of disabling the Schur solver. A synthetic calibration reproduces this as 14 distortion coefficients when the standard model should return five.

这是公开枚举常量的行为修正。请重新编译使用过 `DisableSchurComplement` 的应用；.NET 会把枚举常量嵌入调用方程序集。若配置或文件将它作为数字保存，请根据原始意图显式迁移为 134217728。数字 262144 继续表示 `TiltedModel`，由于旧值重复，运行时无法自动判断其原始意图。

`DisableSchurComplement | TiltedModel` now combines two independent options. Managed validation accepts bit 27, and the C ABI forwards the mask to OpenCV without translating bit 18. API signatures, C ABI exports and ABI version 1 are unchanged. This correction targets the pinned OpenCV 5.1 runtime; it does not establish compatibility with older native binaries.

Regression tests calibrate the same synthetic board with the default solver/model, Schur disabled, tilted distortion, and both options. They verify the distortion-vector shape, reprojection error and recovered focal lengths through the real managed/C ABI path.
