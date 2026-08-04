# Video Kalman Guide

`JYPPX.OpenCvSharp.Video.KalmanFilter` wraps OpenCV `cv::KalmanFilter` as an opaque native object. It covers creation, reinitialization, prediction, correction, and all primary Kalman matrices through copy-in/copy-out `Mat` accessors.

`JYPPX.OpenCvSharp.Video.KalmanFilter` 以 opaque native 对象封装 OpenCV `cv::KalmanFilter`。它覆盖创建、重新初始化、预测、校正，以及通过 `Mat` 拷入/拷出的主要 Kalman 矩阵访问。

## Covered APIs / 已覆盖接口

- `KalmanFilter` constructor and `Init`
- `Predict` and `Correct`
- `GetMatrix` and `SetMatrix`
- `StatePre`, `StatePost`, `TransitionMatrix`, `ControlMatrix`
- `MeasurementMatrix`, `ProcessNoiseCov`, `MeasurementNoiseCov`
- `ErrorCovPre`, `Gain`, `ErrorCovPost`
- `KalmanFilterMatrix`

- `KalmanFilter` 构造函数和 `Init`
- `Predict` 与 `Correct`
- `GetMatrix` 与 `SetMatrix`
- `StatePre`、`StatePost`、`TransitionMatrix`、`ControlMatrix`
- `MeasurementMatrix`、`ProcessNoiseCov`、`MeasurementNoiseCov`
- `ErrorCovPre`、`Gain`、`ErrorCovPost`
- `KalmanFilterMatrix`

## Basic Use / 基础用法

```csharp
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Video;

namespace KalmanSample
{
    internal static class Program
    {
        private static void Main()
        {
            using (KalmanFilter filter = new KalmanFilter(2, 1))
            using (Mat measurement = new Mat(1, 1, MatType.CV_32FC1, new Scalar(1.0)))
            using (Mat prediction = filter.Predict())
            using (Mat corrected = filter.Correct(measurement))
            {
                System.Console.WriteLine("Prediction=" + prediction.Size + ", corrected=" + corrected.Size);
            }
        }
    }
}
```

The matrix properties return owned `Mat` copies. Assigning a property copies data back into the native filter matrix. This keeps `cv::Mat` layout and `cv::KalmanFilter` object layout behind the C ABI.

矩阵属性返回 managed 层拥有的 `Mat` 副本。给属性赋值会把数据复制回 native filter 矩阵。这样 `cv::Mat` 布局和 `cv::KalmanFilter` 对象布局都留在 C ABI 边界内部。

## Runtime Notes / 运行时说明

`KalmanFilter` belongs to the OpenCV `video` module and requires the factual OpenCV 5.0.0 runtime artifact `opencv_video500.dll` in linked builds. In no-OpenCV builds, the exported ABI remains present and managed calls throw an `OpenCvException` with a `NOT_LINKED` message.

`KalmanFilter` 属于 OpenCV `video` 模块，linked build 需要事实性 OpenCV 5.0.0 runtime 产物 `opencv_video500.dll`。在 no-OpenCV build 中，导出的 ABI 仍然存在，managed 调用会抛出带有 `NOT_LINKED` 信息的 `OpenCvException`。
