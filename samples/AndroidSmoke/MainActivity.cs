using Android.App;
using Android.OS;
using Android.Util;
using Android.Widget;
using OpenCvSharp;

namespace OpenCvSharp.AndroidSmoke;

[Activity(
    Name = "io.github.guojinyan.opencvcsharp.smoke.MainActivity",
    Label = "OpenCV CSharp Smoke",
    MainLauncher = true,
    Exported = true)]
public sealed class MainActivity : Activity
{
    private const string LogTag = "OpenCvSharpSmoke";

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        string result;
        try
        {
            using var image = new Mat(8, 8, MatType.CV_8UC1, new Scalar(7));
            var sum = Cv2.Sum(image);
            result = Math.Abs(sum.V0 - 448.0) < 0.001
                ? $"PASS version={OpenCvSharpBuildInfo.GetNativeOpenCvVersion()} sum={sum.V0:0}"
                : $"FAIL unexpected-sum={sum.V0}";
        }
        catch (Exception exception)
        {
            result = $"FAIL {exception.GetType().Name}: {exception.Message}";
        }

        Log.Info(LogTag, result);
        SetContentView(new TextView(this)
        {
            Text = result,
            TextSize = 18
        });
    }
}
