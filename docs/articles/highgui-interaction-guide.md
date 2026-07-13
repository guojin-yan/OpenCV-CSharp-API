# HighGui Interaction Guide

`OpenCvSharp.HighGui` now includes the second HighGUI batch: window properties, window title and image rectangle helpers, trackbars, mouse callbacks, and Qt button callbacks.

`OpenCvSharp.HighGui` 现在包含 HighGUI 第二批能力：窗口属性、窗口标题和图像区域 helper、trackbar、鼠标回调以及 Qt button 回调。

## Covered APIs / 已覆盖接口

- `Cv2.SetWindowProperty`
- `Cv2.GetWindowProperty`
- `Cv2.SetWindowTitle`
- `Cv2.GetWindowImageRect`
- `Cv2.CreateTrackbar`
- `Cv2.GetTrackbarPos`
- `Cv2.SetTrackbarPos`
- `Cv2.SetTrackbarMin`
- `Cv2.SetTrackbarMax`
- `Cv2.SetMouseCallback`
- `Cv2.CreateButton`
- `HighGuiTrackbar`
- `WindowPropertyFlags`, `MouseEventTypes`, `MouseEventFlags`, `QtButtonTypes`

- `Cv2.SetWindowProperty`
- `Cv2.GetWindowProperty`
- `Cv2.SetWindowTitle`
- `Cv2.GetWindowImageRect`
- `Cv2.CreateTrackbar`
- `Cv2.GetTrackbarPos`
- `Cv2.SetTrackbarPos`
- `Cv2.SetTrackbarMin`
- `Cv2.SetTrackbarMax`
- `Cv2.SetMouseCallback`
- `Cv2.CreateButton`
- `HighGuiTrackbar`
- `WindowPropertyFlags`、`MouseEventTypes`、`MouseEventFlags`、`QtButtonTypes`

## Guarded Interaction Smoke / 受控交互 Smoke

```csharp
using OpenCvSharp.Core;
using HighGuiCv2 = OpenCvSharp.HighGui.Cv2;

namespace HighGuiInteractionSample
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

            const string name = "OpenCvSharp.HighGui.Interaction";
            using (Mat image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(0, 128, 255)))
            {
                HighGuiCv2.NamedWindow(name);
                HighGuiCv2.SetWindowTitle(name, "Interaction smoke");
                HighGuiCv2.ImShow(name, image);
                using (OpenCvSharp.HighGui.HighGuiTrackbar trackbar = HighGuiCv2.CreateTrackbar("value", name, 0, 10, _ => { }))
                {
                    HighGuiCv2.SetTrackbarPos("value", name, 3);
                    HighGuiCv2.SetMouseCallback(name, null);
                    System.Console.WriteLine("trackbar=" + HighGuiCv2.GetTrackbarPos("value", name));
                }

                HighGuiCv2.DestroyWindow(name);
            }
        }
    }
}
```

## Callback Lifetime / 回调生命周期

`HighGuiTrackbar` keeps the managed delegate alive while the object is alive. OpenCV does not expose a remove-trackbar API, so releasing the managed object clears native callback state but intentionally does not delete the small native registration state that OpenCV may still reference.

`HighGuiTrackbar` 在对象存活期间保持 managed delegate 存活。OpenCV 不暴露移除 trackbar 的 API，因此释放 managed 对象时会清空 native callback 状态，但不会删除 OpenCV 仍可能引用的少量 native 注册状态。

Mouse and button callbacks are stored as process-level managed callback slots in this initial wrapper. Registering a new callback replaces the previous managed delegate. Applications that need multiple active callback registrations should keep this limit in mind.

鼠标和按钮回调在当前初始封装中以进程级 managed callback 槽位保存。注册新的回调会替换前一个 managed delegate。需要多个活跃回调注册的应用应注意这一限制。

## Runtime Notes / 运行时说明

Default tests keep window creation behind `OPENCV_CSHARP_HIGHGUI_SMOKE=1`. The older `OPENCV5SHARP_HIGHGUI_SMOKE=1` name remains accepted only as an existing-smoke-workflow compatibility alias. Headless CI should leave both variables unset. Stub builds still export the ABI and return `NOT_LINKED`.

默认测试把窗口创建放在 `OPENCV_CSHARP_HIGHGUI_SMOKE=1` 之后。旧的 `OPENCV5SHARP_HIGHGUI_SMOKE=1` 名称仍仅作为既有 smoke workflow 的兼容别名使用。无头 CI 应保持两个变量都未设置。stub build 仍导出 ABI 并返回 `NOT_LINKED`。
