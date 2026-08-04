# HighGui Interaction Guide

`JYPPX.OpenCvSharp.HighGui` covers the OpenCV 5.0.0 main HighGUI window, event-loop, property, trackbar, mouse, and Qt-button surface that can be represented safely by the full runtime. The deterministic compatibility map classifies all 26 parser-emitted callables: 20 implemented, 3 intentionally omitted interactive ROI operations, and 3 Qt-only conditional operations.

`JYPPX.OpenCvSharp.HighGui` 覆盖 OpenCV 5.0.0 main HighGUI 中可由 full runtime 安全表达的窗口、事件循环、属性、trackbar、鼠标和 Qt button surface。确定性兼容性 map 对 parser 输出的 26 个 callable 完成分类：20 个 implemented、3 个 intentionally omitted 的交互式 ROI 操作，以及 3 个 Qt-only conditional 操作。

## Covered APIs / 已覆盖接口

- `Cv2.SetWindowProperty`
- `Cv2.GetCurrentUIFramework`
- `Cv2.StartWindowThread`
- `Cv2.WaitKeyEx`
- `Cv2.GetWindowProperty`
- `Cv2.SetWindowTitle`
- `Cv2.GetWindowImageRect`
- `Cv2.CreateTrackbar`
- `Cv2.GetTrackbarPos`
- `Cv2.SetTrackbarPos`
- `Cv2.SetTrackbarMin`
- `Cv2.SetTrackbarMax`
- `Cv2.SetMouseCallback`
- `Cv2.GetMouseWheelDelta`
- `Cv2.CreateButton`
- `Cv2.ThrowPendingCallbackException`
- `HighGuiTrackbar`
- `WindowPropertyFlags`, `MouseEventTypes`, `MouseEventFlags`, `QtButtonTypes`

- `Cv2.SetWindowProperty`
- `Cv2.GetCurrentUIFramework`
- `Cv2.StartWindowThread`
- `Cv2.WaitKeyEx`
- `Cv2.GetWindowProperty`
- `Cv2.SetWindowTitle`
- `Cv2.GetWindowImageRect`
- `Cv2.CreateTrackbar`
- `Cv2.GetTrackbarPos`
- `Cv2.SetTrackbarPos`
- `Cv2.SetTrackbarMin`
- `Cv2.SetTrackbarMax`
- `Cv2.SetMouseCallback`
- `Cv2.GetMouseWheelDelta`
- `Cv2.CreateButton`
- `Cv2.ThrowPendingCallbackException`
- `HighGuiTrackbar`
- `WindowPropertyFlags`、`MouseEventTypes`、`MouseEventFlags`、`QtButtonTypes`

## Guarded Interaction Smoke / 受控交互 Smoke

```csharp
using JYPPX.OpenCvSharp.Core;
using HighGuiCv2 = JYPPX.OpenCvSharp.HighGui.Cv2;

namespace HighGuiInteractionSample
{
    internal static class Program
    {
        private const string HighGuiSmokeVariable = "OPENCV_CSHARP_HIGHGUI_SMOKE";

        private static void Main()
        {
            string? smoke = System.Environment.GetEnvironmentVariable(HighGuiSmokeVariable)
                ?? System.Environment.GetEnvironmentVariable(CompatibilityHighGuiSmokeAliasVariable);
            if (smoke != "1")
            {
                return;
            }

            const string name = "JYPPX.OpenCvSharp.HighGui.Interaction";
            using (Mat image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(0, 128, 255)))
            {
                HighGuiCv2.NamedWindow(name);
                HighGuiCv2.SetWindowTitle(name, "Interaction smoke");
                HighGuiCv2.ImShow(name, image);
                using (JYPPX.OpenCvSharp.HighGui.HighGuiTrackbar trackbar = HighGuiCv2.CreateTrackbar("value", name, 0, 10, _ => { }))
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

`HighGuiTrackbar` owns the managed side of a reference-counted native registration. Disposing it immediately clears the callback and managed delegate root. Because OpenCV has no remove-trackbar API, the native side retains a callback-disabled registration until its window is destroyed. `DestroyWindow` releases native trackbar and mouse registrations for that window; `DestroyAllWindows` also releases Qt button registrations. Either destruction order is supported, and repeated managed disposal is harmless.

`HighGuiTrackbar` 拥有引用计数 native 注册的 managed 一侧。释放对象会立即清空 callback 并解除 managed delegate root。由于 OpenCV 没有 remove-trackbar API，native 一侧会保留一个已禁用 callback 的注册，直到窗口销毁。`DestroyWindow` 释放该窗口的 native trackbar 和 mouse 注册；`DestroyAllWindows` 还会释放 Qt button 注册。两种销毁顺序都受支持，重复 managed disposal 无害。

Mouse registrations are isolated by ordinal window name. Replacing or clearing one window's callback does not disturb another window. Qt does not expose button removal, so button delegates remain rooted until `DestroyAllWindows`. Managed exceptions never cross the native callback boundary: they are captured in FIFO order and must be observed explicitly by calling `ThrowPendingCallbackException` from application-controlled code. Callbacks run on the thread selected by the active backend, may be reentrant, and must not assume the UI thread is the caller thread.

Mouse 注册按 ordinal 窗口名称隔离；替换或清除一个窗口的 callback 不会影响其他窗口。Qt 不提供 button removal，因此 button delegate 会保持 rooted，直到 `DestroyAllWindows`。Managed 异常不会穿越 native callback 边界：它们按 FIFO 顺序捕获，并由应用控制代码显式调用 `ThrowPendingCallbackException` 观察。Callback 运行在 active backend 选择的线程上，可能重入，也不能假定 UI thread 就是调用线程。

## Backend And Event Loop / Backend 与事件循环

`GetCurrentUIFramework` is the noninteractive backend probe and returns an empty string when no UI backend is available. `StartWindowThread` is backend-specific and commonly returns zero; it does not replace `WaitKey`, `WaitKeyEx`, or `PollKey` as the event pump. `WaitKeyEx` preserves backend-specific full key codes, while `WaitKey` follows OpenCV's portable key-code behavior. A zero delay waits indefinitely, so automated code must use `PollKey` or a positive delay.

`GetCurrentUIFramework` 是无交互 backend 探针；没有 UI backend 时返回空字符串。`StartWindowThread` 依赖 backend，通常返回零，不能替代 `WaitKey`、`WaitKeyEx` 或 `PollKey` 的事件泵作用。`WaitKeyEx` 保留 backend-specific 完整键码，`WaitKey` 遵循 OpenCV 的可移植键码行为。零 delay 会无限等待，因此自动化代码必须使用 `PollKey` 或正数 delay。

All managed HighGUI strings reject `null`, embedded null characters, and invalid UTF-16 before entering native code. Explicit-length callback entrypoints independently reject invalid UTF-8. Image arguments remain caller-owned and are borrowed only for the native call. Interactive `selectROI` overloads are intentionally omitted because they block automation and have no cancellable managed contract. Qt overlay/status/font operations remain conditional and are not advertised by the verified WIN32 runtime.

所有 managed HighGUI 字符串在进入 native code 前拒绝 `null`、embedded null 和 invalid UTF-16。显式长度 callback entrypoint 还会独立拒绝 invalid UTF-8。图像参数仍由 caller 持有，只在 native call 期间 borrowed。交互式 `selectROI` overload 因会阻塞自动化且没有可取消 managed contract 而有意省略。Qt overlay/status/font 操作保持 conditional，不由已验证的 WIN32 runtime 宣称支持。

## Runtime Notes / 运行时说明
