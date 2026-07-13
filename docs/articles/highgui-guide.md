# HighGui Guide

`OpenCvSharp.HighGui` provides a small, explicit wrapper for OpenCV HighGUI window, key, property, and interaction helpers. These APIs are useful for local inspection, but they are GUI-thread and platform dependent, so default tests do not create windows.

`OpenCvSharp.HighGui` 为 OpenCV HighGUI 窗口、按键、属性和交互辅助函数提供小而明确的封装。这些 API 适合本地检查图像，但依赖 GUI 线程和平台环境，因此默认测试不会创建窗口。

## Covered APIs / 已覆盖接口

- `Cv2.NamedWindow`
- `Cv2.DestroyWindow`
- `Cv2.DestroyAllWindows`
- `Cv2.ImShow`
- `Cv2.WaitKey`
- `Cv2.PollKey`
- `Cv2.MoveWindow`
- `Cv2.ResizeWindow`
- `Cv2.SetWindowProperty`
- `Cv2.GetWindowProperty`
- `Cv2.SetWindowTitle`
- `Cv2.GetWindowImageRect`
- `Cv2.CreateTrackbar`
- `Cv2.SetMouseCallback`
- `Cv2.CreateButton`
- `WindowFlags`

- `Cv2.NamedWindow`
- `Cv2.DestroyWindow`
- `Cv2.DestroyAllWindows`
- `Cv2.ImShow`
- `Cv2.WaitKey`
- `Cv2.PollKey`
- `Cv2.MoveWindow`
- `Cv2.ResizeWindow`
- `Cv2.SetWindowProperty`
- `Cv2.GetWindowProperty`
- `Cv2.SetWindowTitle`
- `Cv2.GetWindowImageRect`
- `Cv2.CreateTrackbar`
- `Cv2.SetMouseCallback`
- `Cv2.CreateButton`
- `WindowFlags`

## Guarded Smoke / 受控 Smoke

```csharp
using OpenCvSharp.Core;
using HighGuiCv2 = OpenCvSharp.HighGui.Cv2;

namespace HighGuiSmokeSample
{
    internal static class Program
    {
        private const string HighGuiSmokeVariable = "OPENCV_CSHARP_HIGHGUI_SMOKE";
        private const string CompatibilityHighGuiSmokeAliasVariable = "OPENCV5SHARP_HIGHGUI_SMOKE";

        private static void Main()
        {
            string? smoke = System.Environment.GetEnvironmentVariable(HighGuiSmokeVariable)
                ?? System.Environment.GetEnvironmentVariable(CompatibilityHighGuiSmokeAliasVariable);
            if (smoke != "1")
            {
                return;
            }

            const string name = "OpenCvSharp.HighGui.Smoke";
            using (Mat image = new Mat(64, 64, MatType.CV_8UC3, new Scalar(0, 128, 255)))
            {
                HighGuiCv2.NamedWindow(name);
                HighGuiCv2.ImShow(name, image);
                HighGuiCv2.WaitKey(1);
                HighGuiCv2.DestroyWindow(name);
            }
        }
    }
}
```

## Runtime Notes / 运行时说明

HighGUI requires the factual OpenCV 5.0.0 runtime artifact `opencv_highgui500.dll` and may require additional platform GUI dependencies. Headless runners should keep `OPENCV_CSHARP_HIGHGUI_SMOKE` unset. The older `OPENCV5SHARP_HIGHGUI_SMOKE` name remains accepted only as an existing-smoke-workflow compatibility alias. Stub builds still export the ABI and return `NOT_LINKED` for native calls.

HighGUI 需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_highgui500.dll`，并可能需要额外平台 GUI 依赖。无头 runner 应保持 `OPENCV_CSHARP_HIGHGUI_SMOKE` 未设置。旧的 `OPENCV5SHARP_HIGHGUI_SMOKE` 名称仍仅作为既有 smoke workflow 的兼容别名使用。stub build 仍会导出 ABI，并让 native 调用返回 `NOT_LINKED`。

Trackbar, mouse callback, button callback, and callback lifetime notes are covered in [HighGui Interaction Guide](highgui-interaction-guide.md).

Trackbar、鼠标回调、按钮回调和回调生命周期说明见 [HighGui Interaction Guide](highgui-interaction-guide.md)。
