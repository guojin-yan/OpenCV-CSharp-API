# 12 Perspective Transform / 透视变换

A four-corner correspondence is enough to rectify a planar target. The sample calls `GetPerspectiveTransform`, warps the source with `WarpPerspective`, and draws the destination quadrilateral.

四个角点对应关系即可校正平面目标。本案例调用 `GetPerspectiveTransform`，再用 `WarpPerspective` 变换输入并绘制目标四边形。

```powershell
dotnet run --project .\samples\Geometry\01.PerspectiveTransform\PerspectiveTransform.csproj -c Release -- .\artifacts\tutorial-12
```

Use this pattern for document scans, screen rectification, and planar measurement. The four points must follow the same winding order in source and destination.

## Pipeline / 流程

The source quadrilateral is the four image corners. The destination points deliberately use a mild skew, making the effect visible while keeping every destination point inside the output panel. The 3x3 matrix is owned by the `Mat` wrapper and disposed with the workflow.

源四边形取图像四角；目标点使用轻微倾斜，使效果清晰且仍落在输出面板内。3x3 矩阵由 `Mat` 包装对象持有，并随流程确定性释放。
