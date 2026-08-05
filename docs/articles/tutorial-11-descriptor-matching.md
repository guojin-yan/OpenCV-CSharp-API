# 11 Descriptor Matching / 描述子匹配

This workflow rotates the deterministic source, extracts ORB descriptors from both images, performs brute-force Hamming matching with cross-check, and renders the strongest correspondences.

本案例旋转确定性输入，在两张图上提取 ORB 描述子，使用带交叉检查的暴力 Hamming 匹配，并绘制最强的对应关系。

```powershell
dotnet run --project .\samples\Features\03.DescriptorMatching\DescriptorMatching.csproj -c Release -- .\artifacts\tutorial-11
```

Cross-check removes many one-way false matches. For production visual search, add a ratio test, geometric verification, and a domain-specific confidence threshold after this descriptor stage.

## Pipeline / 流程

`ORB.DetectAndCompute` produces keypoints and `CV_8U` binary descriptors. `BFMatcher` with `NormTypes.Hamming` compares the descriptors; sorting by `Distance` and keeping the best 40 makes the output stable. `Features2D.Cv2.DrawMatches` then creates a reviewable correspondence panel.

`ORB.DetectAndCompute` 生成关键点和 `CV_8U` 二进制描述子；`BFMatcher` 使用 `NormTypes.Hamming` 比较描述子，按 `Distance` 排序后保留前 40 个，保证输出稳定；最后用 `Features2D.Cv2.DrawMatches` 生成可检查的匹配图。
