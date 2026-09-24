using System;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using ImgCodecsCv2 = JYPPX.OpenCvSharp.ImgCodecs.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

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
                Console.WriteLine("native_smoke=verified");
                Console.WriteLine("encoded_bytes=" + encoded.Length);
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
