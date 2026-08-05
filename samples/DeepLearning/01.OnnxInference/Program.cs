using System;
using System.IO;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Dnn;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Samples.Common;
using DnnCv2 = JYPPX.OpenCvSharp.Dnn.Cv2;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.DeepLearning.OnnxInference
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "onnx-inference");
            string fixturePath = Path.Combine(AppContext.BaseDirectory, "Model", "identity-opset13.onnx.base64");
            bool dnnLinked = true;
            Mat panel;
            string details;

            try
            {
                byte[] model = Convert.FromBase64String(File.ReadAllText(fixturePath).Trim());
                using (Net net = Net.ReadNetFromOnnx(model, DnnEngine.Classic))
                using (var image = new Mat(2, 2, MatType.CV_32FC1))
                {
                    image.CopyFrom<float>(new[] { 1F, 2F, 3F, 4F });
                    using (Mat blob = DnnCv2.BlobFromImage(image, new Image2BlobParams()))
                    {
                        net.SetPreferableBackend(DnnBackend.OpenCV)
                            .SetPreferableTarget(DnnTarget.Cpu)
                            .SetInput(blob, "input");
                        string[] outputNames = net.GetUnconnectedOutLayersNames();
                        using (Mat output = net.Forward(outputNames[0]))
                        {
                            float[] values = output.ToArray<float>();
                            panel = new Mat(SampleSupport.PanelHeight, SampleSupport.PanelWidth,
                                MatType.CV_8UC3, new Scalar(28, 34, 40));
                            SampleSupport.AddPanelLabel(panel, "ONNX IDENTITY INFERENCE", new Scalar(88, 196, 255));
                            ImgProcCv2.PutText(panel, "Input:  [1, 2, 3, 4]", new Point(42, 138),
                                HersheyFonts.HersheyDuplex, 0.9, new Scalar(240, 245, 250), 2, LineTypes.AntiAlias);
                            ImgProcCv2.PutText(panel, "Output: [" + string.Join(", ", values) + "]", new Point(42, 190),
                                HersheyFonts.HersheyDuplex, 0.9, new Scalar(88, 220, 168), 2, LineTypes.AntiAlias);
                            ImgProcCv2.PutText(panel, "OpenCV DNN  |  ONNX opset 13  |  CPU", new Point(42, 246),
                                HersheyFonts.HersheySimplex, 0.72, new Scalar(190, 202, 214), 1, LineTypes.AntiAlias);
                            SampleSupport.AddMetric(panel, "Model  " + model.Length + " bytes");
                            SampleSupport.WritePng(outputDirectory, "onnx-inference.png", panel);
                            details = "modelBytes=" + model.Length + ", output=" + string.Join(",", values);
                        }
                    }
                }
            }
            catch (OpenCvException exception) when (exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                dnnLinked = false;
                panel = new Mat(SampleSupport.PanelHeight, SampleSupport.PanelWidth,
                    MatType.CV_8UC3, new Scalar(31, 35, 42));
                SampleSupport.AddPanelLabel(panel, "ONNX IDENTITY INFERENCE", new Scalar(88, 196, 255));
                ImgProcCv2.PutText(panel, "Native DNN module is not linked in this runtime.",
                    new Point(32, 166), HersheyFonts.HersheySimplex, 0.8,
                    new Scalar(232, 240, 245), 2, LineTypes.AntiAlias);
                ImgProcCv2.PutText(panel, "Install a runtime profile with DNN support to run ONNX.",
                    new Point(32, 204), HersheyFonts.HersheySimplex, 0.62,
                    new Scalar(190, 202, 214), 1, LineTypes.AntiAlias);
                SampleSupport.AddMetric(panel, "Native DNN  NOT_LINKED");
                details = "status=NOT_LINKED";
            }

            using (panel)
            {
                if (!dnnLinked)
                {
                    SampleSupport.WritePng(outputDirectory, "onnx-inference.png", panel);
                }
            }
            SampleSupport.WriteSummary("ONNX identity inference", outputDirectory, details);
        }
    }
}
