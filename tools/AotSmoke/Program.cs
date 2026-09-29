using System;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;
using DnnCv2 = JYPPX.OpenCvSharp.Dnn.Cv2;

internal static class Program
{
    private static int Main()
    {
        Console.WriteLine("AOT_SMOKE_START");
        Console.WriteLine("managed_package=" + OpenCvSharpBuildInfo.NuGetPackageVersion);
        Console.WriteLine("opencv_version=" + OpenCvSharpBuildInfo.OpenCvVersion);
        Console.WriteLine("native_abi=" + OpenCvSharpBuildInfo.NativeAbiVersion);

        try
        {
            OpenCvSharpBuildInfo.VerifyNativeRuntimeCompatibility();
            using (Mat source = new Mat(2, 2, MatType.CV_8UC1))
            using (Mat blurred = new Mat())
            {
                source.SetTo(new Scalar(7));
                ImgProcCv2.Blur(source, blurred, new Size(1, 1));
                byte[] encoded = ImgCodecsCv2.ImEncode(".png", blurred);
                bool dnnCpuAvailable = false;
                int dnnTargetCount = 0;
                try
                {
                    DnnTarget[] dnnTargets = DnnCv2.GetAvailableTargets(DnnBackend.OpenCV);
                    dnnTargetCount = dnnTargets.Length;
                    dnnCpuAvailable = Array.IndexOf(dnnTargets, DnnTarget.Cpu) >= 0;
                }
                catch (OpenCvException)
                {
                    // A Mini runtime may omit DNN; the full-runtime evidence guard requires the CPU target.
                }
                Console.WriteLine("native_smoke=verified");
                Console.WriteLine("encoded_bytes=" + encoded.Length);
                Console.WriteLine("dnn_cpu_available=" + dnnCpuAvailable.ToString().ToLowerInvariant());
                Console.WriteLine("dnn_target_count=" + dnnTargetCount);
            }
        }
        catch (DllNotFoundException)
        {
            Console.WriteLine("native_smoke=skipped-native-runtime-missing");
        }
        catch (EntryPointNotFoundException)
        {
            Console.WriteLine("native_smoke=skipped-native-entrypoint-missing");
        }
        catch (BadImageFormatException)
        {
            Console.WriteLine("native_smoke=skipped-native-architecture-mismatch");
        }

        Console.WriteLine("AOT_SMOKE_OK");
        return 0;
    }
}
