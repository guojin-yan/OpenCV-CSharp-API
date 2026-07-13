# MatType Constants / MatType 常量

`MatType` constants are generated from the OpenCV 5.0.0 header:

`MatType` 常量从 OpenCV 5.0.0 头文件生成：

```text
modules/core/include/opencv2/core/hal/interface.h
```

Generation command:

生成命令：

```powershell
pwsh -NoProfile -File .\scripts\Generate-MatTypeConstants.ps1
```

Check command:

校验命令：

```powershell
pwsh -NoProfile -File .\scripts\Generate-MatTypeConstants.ps1 -Check
```

## OpenCV 5 Encoding / OpenCV 5 编码

OpenCV 5.0.0 uses `CV_CN_SHIFT = 5`.

OpenCV 5.0.0 使用 `CV_CN_SHIFT = 5`。

This means:

这意味着：

```text
CV_8UC1 = 0
CV_8UC2 = 32
CV_8UC3 = 64
CV_8UC4 = 96
```

Do not copy OpenCV 4-era `CV_8UC3 = 16` assumptions into this project.

不要把 OpenCV 4 时代常见的 `CV_8UC3 = 16` 假设复制到本项目。

## Rules / 规则

- Edit `MatType.cs` for helper methods only.
- Do not manually edit `MatType.Generated.cs`.
- Run the generator after changing OpenCV source version.
- Use `-Check` in local validation when OpenCV source exists.

- 只在 `MatType.cs` 中编辑辅助方法。
- 不要手动编辑 `MatType.Generated.cs`。
- 更换 OpenCV 源码版本后运行生成脚本。
- 当本地存在 OpenCV 源码时，使用 `-Check` 进行校验。

