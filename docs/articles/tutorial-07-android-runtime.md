# Android Runtime And Native Loading / Android Runtime 与原生加载

This tutorial creates a .NET for Android application, installs the managed API plus one Android runtime package, and proves that the APK executes a native OpenCV call. The checked-in reference implementation is `samples/AndroidSmoke`.

本教程创建一个 .NET for Android 应用，安装 managed API 和一个 Android runtime 包，并验证 APK 能够执行真实的 OpenCV native 调用。仓库中的参考实现位于 `samples/AndroidSmoke`。

## Supported Targets / 支持目标

| Emulator ABI | RID | Full package | Mini package | Verified system image |
|---|---|---|---|---|
| x86_64 | `android-x64` | `JYPPX.OpenCV.runtime.android-x64` | `JYPPX.OpenCV.runtime.android-x64.mini` | Android 35 `default;x86_64` |
| x86 | `android-x86` | `JYPPX.OpenCV.runtime.android-x86` | `JYPPX.OpenCV.runtime.android-x86.mini` | Android 29 `default;x86` |

Android x64/x86 Full and Mini are real-supported after the single-loader packages passed authoritative emulator-native loading with `Mat` and `Cv2.Sum`. Android ARM/ARM64 remain `android-evidence-pending` until ABI-matched physical-device loading passes, so do not use package-matrix presence as a support claim. Current and superseded CI records are stored in [`android-runtime-evidence.json`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/android-runtime-evidence.json).

Android x64/x86 的 Full 与 Mini 已通过单加载器包的正式模拟器原生加载，并实际执行 `Mat` 与 `Cv2.Sum`，现归类为 real support。Android ARM/ARM64 在 ABI 匹配的真机加载通过前仍为 `android-evidence-pending`，不能仅根据 package matrix 中存在包名就宣称已受支持。当前及已淘汰的 CI 记录见 [`android-runtime-evidence.json`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/packaging/runtime/android-runtime-evidence.json)。

## 1. Prepare The Android Toolchain / 准备 Android 工具链

Install the .NET Android workload and ensure `adb` can see the target emulator:

安装 .NET Android workload，并确认 `adb` 能识别目标模拟器：

```powershell
dotnet workload install android
adb devices
```

Use an x86_64 emulator for `android-x64` or an x86 emulator for `android-x86`. The application declares Android 7.0 / API 24 as its minimum OS version.

`android-x64` 必须配合 x86_64 模拟器，`android-x86` 必须配合 x86 模拟器。应用最低系统版本为 Android 7.0 / API 24。

## 2. Create The Project And Install Packages / 创建项目并安装包

The public installation commands intentionally do not pin a version. Live NuGet badges in the repository README show the current stable release, so this tutorial does not become stale after an update.

公开安装命令有意不固定版本。仓库 README 的 NuGet 实时徽章会显示当前稳定版，后续更新时无需改写教程。

```powershell
dotnet new android -n OpenCvAndroidDemo
Set-Location OpenCvAndroidDemo
dotnet add package JYPPX.OpenCV.CSharp.API
dotnet add package JYPPX.OpenCV.runtime.android-x64.mini
dotnet list package
```

Add these properties to the project's main `PropertyGroup`:

在项目主 `PropertyGroup` 中加入以下属性：

```xml
<SupportedOSPlatformVersion>24.0</SupportedOSPlatformVersion>
<ApplicationId>io.github.guojinyan.opencvcsharp.demo</ApplicationId>
<AndroidPackageFormat>apk</AndroidPackageFormat>
<RuntimeIdentifier>android-x64</RuntimeIdentifier>
```

Reference exactly one runtime profile. Mini is sufficient for Core, ImgProc, ImgCodecs, and VideoIO. Replace it with `JYPPX.OpenCV.runtime.android-x64` when the application needs DNN, calibration, features, Photo, ML, or other Full modules. Do not install Full and Mini together. Confirm with `dotnet list package` that the managed and runtime packages resolve to the same version; commit a NuGet lock file for reproducible applications.

只能引用一个 runtime profile。Core、ImgProc、ImgCodecs 和 VideoIO 使用 Mini 即可；需要 DNN、标定、Features、Photo、ML 或其他 Full 模块时，改用 `JYPPX.OpenCV.runtime.android-x64`。不要同时安装 Full 与 Mini。使用 `dotnet list package` 确认 managed 与 runtime 解析到相同版本；需要可重复构建的应用应提交 NuGet lock file。

## 3. Execute A Native OpenCV Call / 执行原生 OpenCV 调用

Replace `MainActivity.cs` with the following code. It creates an 8 x 8 native `Mat`, fills every element with `7`, calls native `Cv2.Sum`, and reports the native OpenCV version. The expected sum is `8 * 8 * 7 = 448`.

用以下代码替换 `MainActivity.cs`。它创建 8 x 8 native `Mat`，把每个元素设为 `7`，调用 native `Cv2.Sum`，并显示 native OpenCV 版本。预期总和为 `8 * 8 * 7 = 448`。

```csharp
using Android.App;
using Android.OS;
using Android.Util;
using Android.Widget;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;

namespace OpenCvAndroidDemo;

[Activity(
    Name = "io.github.guojinyan.opencvcsharp.demo.MainActivity",
    Label = "OpenCV CSharp Demo",
    MainLauncher = true,
    Exported = true)]
public sealed class MainActivity : Activity
{
    private const string LogTag = "OpenCvSharpDemo";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        string result;
        try
        {
            using var image = new Mat(8, 8, MatType.CV_8UC1, new Scalar(7));
            Scalar sum = Cv2.Sum(image);
            result = Math.Abs(sum.V0 - 448.0) < 0.001
                ? $"PASS version={OpenCvSharpBuildInfo.GetNativeOpenCvVersion()} sum={sum.V0:0}"
                : $"FAIL unexpected-sum={sum.V0}";
        }
        catch (Exception exception)
        {
            result = $"FAIL {exception.GetType().Name}: {exception.Message}";
        }

        Log.Info(LogTag, result);
        SetContentView(new TextView(this) { Text = result, TextSize = 18 });
    }
}
```

This is a loader test, not a managed-only calculation: constructing `Mat`, calling `Cv2.Sum`, and reading the native version all cross the native ABI.

这不是 managed-only 计算：创建 `Mat`、调用 `Cv2.Sum` 和读取 native 版本都会跨越 native ABI，因此能够验证 `.so` 已真正被 APK 加载。

## 4. Build, Install, And Verify / 构建、安装与验证

```powershell
dotnet build -c Release -f net10.0-android
$apk = Get-ChildItem .\bin\Release\net10.0-android\android-x64 -Recurse -Filter '*-Signed.apk' |
  Select-Object -First 1
adb install -r $apk.FullName
adb logcat -c
adb shell am start -n io.github.guojinyan.opencvcsharp.demo/io.github.guojinyan.opencvcsharp.demo.MainActivity
adb logcat -d -s OpenCvSharpDemo:I '*:S'
```

The screen and log must contain a marker equivalent to:

屏幕和日志必须出现等价结果：

```text
PASS version=5.0.0 sum=448
```

A restore/build-only result is insufficient. `DllNotFoundException`, a missing `libJYPPX.OpenCV.Native.so`, the wrong ABI directory, or a result without the native version is a failed runtime integration.

只完成 restore/build 不足以证明 runtime 可用。出现 `DllNotFoundException`、缺少 `libJYPPX.OpenCV.Native.so`、ABI 目录错误，或结果中没有 native version，都表示 runtime 集成失败。

## 5. Switch To The x86 Emulator / 切换到 x86 模拟器

Remove the x64 runtime, install the x86 package without pinning a version, and change `RuntimeIdentifier` to `android-x86`:

移除 x64 runtime，安装不固定版本的 x86 包，并把 `RuntimeIdentifier` 改为 `android-x86`：

```powershell
dotnet remove package JYPPX.OpenCV.runtime.android-x64.mini
dotnet add package JYPPX.OpenCV.runtime.android-x86.mini
dotnet list package
dotnet build -c Release -f net10.0-android -p:RuntimeIdentifier=android-x86
```

Install the APK on an x86 Android 29 emulator and repeat the same log check. Never combine an x86 APK/runtime with an x86_64 emulator assumption; package RID, APK ABI directory, and emulator ABI must agree.

把 APK 安装到 x86 Android 29 模拟器并重复日志检查。不能把 x86 APK/runtime 当作 x86_64 使用；package RID、APK ABI 目录与模拟器 ABI 必须一致。

## Reference Implementation / 参考实现

The repository's [`AndroidSmoke.csproj`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/AndroidSmoke/AndroidSmoke.csproj) and [`MainActivity.cs`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/AndroidSmoke/MainActivity.cs) are the release-evidence implementation. CI additionally audits every package `.so`, confirms the package `buildTransitive` target places those files in the correct APK `lib/<abi>/` directory, installs the APK, and waits for the same native marker.

仓库中的 [`AndroidSmoke.csproj`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/AndroidSmoke/AndroidSmoke.csproj) 与 [`MainActivity.cs`](https://github.com/guojin-yan/OpenCV-CSharp-API/blob/opencv5.x/samples/AndroidSmoke/MainActivity.cs) 是发布证据使用的实现。CI 还会审计包中的每个 `.so`，确认 package `buildTransitive` target 把它们写入正确的 APK `lib/<abi>/` 目录，安装 APK，并等待同一个 native marker。
