using System;
using OpenCvSharp.Core;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace OpenCvSharp.Tests.Features2D
{
    internal static class Feature2DTestData
    {
        internal static bool IsNativeSmokeEnabled()
        {
            return TestEnvironment.IsNativeSmokeEnabled();
        }

        internal static bool IsFeaturesModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("features2d", StringComparison.OrdinalIgnoreCase) >= 0
                && exception.Message.IndexOf("OpenCV", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        internal static Mat CreateFeatureImage()
        {
            Mat image = new Mat(96, 96, MatType.CV_8UC1);
            image.SetTo(new Scalar(0));
            ImgProcCv2.Rectangle(image, new Rect(8, 8, 28, 28), new Scalar(255), -1);
            ImgProcCv2.Circle(image, new Point(64, 64), 14, new Scalar(230), -1);
            ImgProcCv2.Line(image, new Point(8, 82), new Point(88, 10), new Scalar(180), 2);
            ImgProcCv2.Line(image, new Point(4, 46), new Point(92, 46), new Scalar(160), 1);
            return image;
        }

        internal static Mat CreateMserImage()
        {
            Mat image = new Mat(160, 160, MatType.CV_8UC1);
            image.SetTo(new Scalar(0));
            ImgProcCv2.Rectangle(image, new Rect(16, 16, 128, 128), new Scalar(48), -1);
            ImgProcCv2.Rectangle(image, new Rect(36, 36, 88, 88), new Scalar(144), -1);
            ImgProcCv2.Rectangle(image, new Rect(56, 56, 48, 48), new Scalar(224), -1);
            ImgProcCv2.Circle(image, new Point(112, 48), 18, new Scalar(192), -1);
            ImgProcCv2.Circle(image, new Point(48, 112), 16, new Scalar(96), -1);
            return image;
        }

        internal static Mat CreateBlobImage()
        {
            Mat image = new Mat(160, 160, MatType.CV_8UC1);
            image.SetTo(new Scalar(255));
            ImgProcCv2.Circle(image, new Point(40, 40), 12, new Scalar(0), -1);
            ImgProcCv2.Circle(image, new Point(112, 48), 16, new Scalar(0), -1);
            ImgProcCv2.Circle(image, new Point(80, 112), 20, new Scalar(0), -1);
            return image;
        }

        internal static Mat[] CreateBatchFeatureImages()
        {
            return new[]
            {
                CreateFeatureImage(),
                CreateFeatureImage()
            };
        }

        internal static Mat CreateFloatDescriptors(params float[] values)
        {
            Mat descriptors = new Mat(values.Length / 2, 2, MatType.CV_32FC1);
            descriptors.CopyFrom(values);
            return descriptors;
        }
    }
}
