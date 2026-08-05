# 13 Feature Homography / 特征单应性

The case combines ORB, cross-checked matches, `FindHomography` with RANSAC, and `PerspectiveTransform` to recover the location of a planar source in a warped scene.

本案例组合 ORB、交叉检查匹配、带 RANSAC 的 `FindHomography` 和 `PerspectiveTransform`，从透视场景中恢复平面源图位置。

```powershell
dotnet run --project .\samples\Geometry\02.FeatureHomography\FeatureHomography.csproj -c Release -- .\artifacts\tutorial-13
```

The report prints match and inlier counts. In a real system reject results with too few matches or a low inlier ratio before accepting the projected quadrilateral.

## Pipeline / 流程

The source is warped once to create a second view. The best Hamming matches become paired `Point2f` arrays; `FindHomography(..., RANSAC, ...)` returns the robust projective model and an inlier mask. The mask count is shown in the output metric, so a caller can gate downstream actions on it.

程序先透视变换生成第二视图，再把最佳 Hamming 匹配转换为成对的 `Point2f` 数组；`FindHomography(..., RANSAC, ...)` 返回稳健的投影模型和内点掩码。输出指标展示内点数量，调用方可以据此决定是否继续后续动作。
