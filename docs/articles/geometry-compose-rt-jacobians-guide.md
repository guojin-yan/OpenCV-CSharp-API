# Geometry ComposeRT Jacobians Guide

Round 974 adds all eight transform-composition Jacobians to `JYPPX.OpenCvSharp.Calib3D.Cv2`.

Round 974 在 `JYPPX.OpenCvSharp.Calib3D.Cv2` 中增加变换组合的全部八个 Jacobian。

## Composition / 组合

For two rotation-vector and translation-vector transforms, `ComposeRT` computes:

对于两组旋转向量和平移向量变换，`ComposeRT` 计算：

```text
R1 = Rodrigues(rvec1)
R2 = Rodrigues(rvec2)
R3 = R2 * R1
rvec3 = RodriguesInverse(R3)
tvec3 = R2 * tvec1 + tvec2
```

All four inputs must be non-empty `1 x 3` row vectors or `3 x 1` column vectors. They must
have exactly the same orientation and type. Only `CV_32FC1` and `CV_64FC1` are accepted.
The composed vectors preserve the input orientation and depth.

四个输入都必须是非空的 `1 x 3` 行向量或 `3 x 1` 列向量，方向和类型必须完全相同。
仅接受 `CV_32FC1` 和 `CV_64FC1`。组合后的向量保留输入的方向和深度。

## Jacobians / Jacobian 矩阵

The extended overload returns these `3 x 3` matrices with the same type as the inputs:

扩展重载返回以下 `3 x 3` 矩阵，其类型与输入相同：

```text
dr3dr1 = d(rvec3) / d(rvec1)
dr3dt1 = d(rvec3) / d(tvec1)
dr3dr2 = d(rvec3) / d(rvec2)
dr3dt2 = d(rvec3) / d(tvec2)
dt3dr1 = d(tvec3) / d(rvec1)
dt3dt1 = d(tvec3) / d(tvec1)
dt3dr2 = d(tvec3) / d(rvec2)
dt3dt2 = d(tvec3) / d(tvec2)
```

Each Jacobian row corresponds to one output-vector coordinate, and each column corresponds to
one input-vector coordinate. The matrices have the following exact structural identities:

每个 Jacobian 的行对应一个输出向量坐标，列对应一个输入向量坐标。以下结构恒等式精确成立：

```text
dr3dt1 = 0
dr3dt2 = 0
dt3dr1 = 0
dt3dt1 = R2
dt3dt2 = I
```

`dr3dr1`, `dr3dr2`, and `dt3dr2` are the remaining nontrivial Jacobians. They are suitable for
analytic optimization chains and can be verified with central finite differences away from
Rodrigues singularities.

`dr3dr1`、`dr3dr2` 和 `dt3dr2` 是其余非平凡 Jacobian。它们可用于解析优化链，并可在
避开 Rodrigues 奇异点时通过中心有限差分验证。

## Caller-Owned Outputs / 调用方持有输出

Use the fourteen-parameter overload when output matrices belong to an existing processing
pipeline:

当输出矩阵属于现有处理流程时，使用十四参数重载：

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

using var rvec1 = new Mat(3, 1, MatType.CV_64FC1);
using var tvec1 = new Mat(3, 1, MatType.CV_64FC1);
using var rvec2 = new Mat(3, 1, MatType.CV_64FC1);
using var tvec2 = new Mat(3, 1, MatType.CV_64FC1);
using var rvec3 = new Mat();
using var tvec3 = new Mat();
using var dr3dr1 = new Mat();
using var dr3dt1 = new Mat();
using var dr3dr2 = new Mat();
using var dr3dt2 = new Mat();
using var dt3dr1 = new Mat();
using var dt3dt1 = new Mat();
using var dt3dr2 = new Mat();
using var dt3dt2 = new Mat();

rvec1.CopyFrom<double>(new double[] { 0.10, -0.20, 0.05 });
tvec1.CopyFrom<double>(new double[] { 1.00, 2.00, -0.50 });
rvec2.CopyFrom<double>(new double[] { -0.08, 0.04, 0.12 });
tvec2.CopyFrom<double>(new double[] { 0.25, -0.75, 1.50 });

Calib3DCv2.ComposeRT(
    rvec1, tvec1, rvec2, tvec2,
    rvec3, tvec3,
    dr3dr1, dr3dt1, dr3dr2, dr3dt2,
    dt3dr1, dt3dt1, dt3dr2, dt3dt2);
```

No output may alias an input or another output. Native execution may resize and replace existing
output storage.

任何输出都不得与输入或其他输出别名。原生执行可能调整并替换现有输出存储。

## Owned Result / 拥有所有权的结果

The four-parameter overload returns a `ComposeRTDerivativesResult` containing the two composed
vectors and all eight Jacobians:

四参数重载返回 `ComposeRTDerivativesResult`，其中包含两个组合向量和全部八个 Jacobian：

```csharp
ComposeRTDerivativesResult result =
    Calib3DCv2.ComposeRT(rvec1, tvec1, rvec2, tvec2);

using Mat rvec3 = result.Rvec3;
using Mat tvec3 = result.Tvec3;
using Mat dr3dr1 = result.Dr3Dr1;
using Mat dr3dt1 = result.Dr3Dt1;
using Mat dr3dr2 = result.Dr3Dr2;
using Mat dr3dt2 = result.Dr3Dt2;
using Mat dt3dr1 = result.Dt3Dr1;
using Mat dt3dt1 = result.Dt3Dt1;
using Mat dt3dr2 = result.Dt3Dr2;
using Mat dt3dt2 = result.Dt3Dt2;
```

The result is a lightweight value that groups owned matrices; it is not itself disposable.
The caller must dispose all ten `Mat` instances. If validation or native execution fails before
the result is returned, the overload disposes every temporary output automatically.

结果是对拥有所有权矩阵进行分组的轻量值，本身不可释放。调用方必须释放全部十个 `Mat`
实例。如果校验或原生执行在返回结果前失败，该重载会自动释放所有临时输出。

## Finite-Difference Verification / 有限差分验证

For each scalar input component `x`, compare the matching analytic Jacobian column with:

对于每个输入标量分量 `x`，将对应解析 Jacobian 列与以下中心差分比较：

```text
(output(x + epsilon) - output(x - epsilon)) / (2 * epsilon)
```

Use `CV_64FC1`, small nonzero rotations away from Rodrigues singularities, and restore each
perturbed component after evaluation. Rotation outputs should be compared through the same local
rotation-vector branch.

建议使用 `CV_64FC1`、避开 Rodrigues 奇异点的小非零旋转，并在每次计算后恢复被扰动分量。
旋转输出应在相同的局部旋转向量分支内比较。
