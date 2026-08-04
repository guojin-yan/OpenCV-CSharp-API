# Geometry Matrix Product Derivatives Guide

Round 973 adds direct matrix-product Jacobians to `JYPPX.OpenCvSharp.Calib3D.Cv2`.

Round 973 在 `JYPPX.OpenCvSharp.Calib3D.Cv2` 中增加直接矩阵乘积 Jacobian 计算。

## Operation / 运算

For matrices:

对于矩阵：

```text
A: M x L
B: L x N
C = A * B: M x N
```

`MatMulDeriv` computes both derivatives:

`MatMulDeriv` 同时计算两个导数：

```text
dABdA = d(A*B)/dA: (M*N) x (M*L)
dABdB = d(A*B)/dB: (M*N) x (L*N)
```

The input matrices must be non-empty, single-channel, and exactly the same type. Only
`CV_32FC1` and `CV_64FC1` are accepted, and `A.Cols` must equal `B.Rows`. The output depth is
the same as the input depth.

输入矩阵必须非空、单通道且类型完全相同。仅接受 `CV_32FC1` 和 `CV_64FC1`，并且
`A.Cols` 必须等于 `B.Rows`。输出深度与输入深度相同。

## Flattening Order / 展开顺序

Rows of each derivative correspond to elements of `C` in row-major order:

每个导数矩阵的行按照 `C` 的行主序元素排列：

```text
C(0,0), C(0,1), ..., C(1,0), ...
```

Columns of `dABdA` correspond to elements of `A` in row-major order. Columns of `dABdB`
correspond to elements of `B` in row-major order.

`dABdA` 的列按照 `A` 的行主序元素排列，`dABdB` 的列按照 `B` 的行主序元素排列。

For:

对于：

```text
C(i,j) = sum(k, A(i,k) * B(k,j))
```

the nonzero entries are:

非零项为：

```text
d C(i,j) / d A(i,k) = B(k,j)
d C(i,j) / d B(k,j) = A(i,k)
```

All other entries in the corresponding Jacobian row are zero.

对应 Jacobian 行中的其他元素均为零。

## Caller-Owned Outputs / 调用方持有输出

Use caller-owned matrices when outputs belong to a longer processing pipeline:

当输出属于更长的处理流程时，使用调用方持有的矩阵：

```csharp
using JYPPX.OpenCvSharp.Calib3D;
using JYPPX.OpenCvSharp.Core;
using Calib3DCv2 = JYPPX.OpenCvSharp.Calib3D.Cv2;

using var a = new Mat(2, 3, MatType.CV_64FC1);
using var b = new Mat(3, 2, MatType.CV_64FC1);
using var dABdA = new Mat();
using var dABdB = new Mat();

a.CopyFrom<double>(new double[] { 1, 2, 3, 4, 5, 6 });
b.CopyFrom<double>(new double[] { 7, 8, 9, 10, 11, 12 });

Calib3DCv2.MatMulDeriv(a, b, dABdA, dABdB);
```

The two outputs must not alias each other, and neither output may alias `A` or `B`. Existing
output storage may be resized and replaced by the native operation.

两个输出不得互相别名，任何输出也不得与 `A` 或 `B` 别名。原生运算可能调整并替换现有
输出存储。

## Owned Outputs / 拥有所有权的输出

The owned overload allocates both matrices:

拥有所有权的重载会分配两个矩阵：

```csharp
Calib3DCv2.MatMulDeriv(a, b, out Mat dABdA, out Mat dABdB);
using (dABdA)
using (dABdB)
{
    // Consume the Jacobians here.
}
```

If validation or native execution fails, both temporary output matrices are disposed before the
exception is rethrown.

如果校验或原生执行失败，两个临时输出矩阵会在异常重新抛出前释放。

## Finite-Difference Verification / 有限差分验证

For a selected element `x` in `A` or `B`, perturb it by a small `epsilon` and compare the
analytic Jacobian column with:

对于 `A` 或 `B` 中选定的元素 `x`，用较小的 `epsilon` 扰动，并将解析 Jacobian 列与
以下中心差分比较：

```text
(C(x + epsilon) - C(x - epsilon)) / (2 * epsilon)
```

Use `CV_64FC1` for strict verification, restore every perturbed input element after each check,
and compare the flattened product in the same row-major order described above.

严格验证时建议使用 `CV_64FC1`，每次检查后恢复被扰动的输入元素，并按照上述相同的
行主序比较展开后的乘积。
