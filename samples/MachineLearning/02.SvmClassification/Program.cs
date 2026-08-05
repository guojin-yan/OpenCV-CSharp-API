using System;
using JYPPX.OpenCvSharp;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.ML;
using JYPPX.OpenCvSharp.Samples.Common;
using ImgProcCv2 = JYPPX.OpenCvSharp.ImgProc.Cv2;

namespace JYPPX.OpenCvSharp.Samples.MachineLearning.SvmClassification
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            string outputDirectory = SampleSupport.GetOutputDirectory(args, "svm-classification");
            const int gridWidth = 80;
            const int gridHeight = 45;
            float[] trainingValues =
            {
                0.12F, 0.18F, 0.20F, 0.34F, 0.34F, 0.13F, 0.42F, 0.39F,
                0.62F, 0.62F, 0.72F, 0.78F, 0.82F, 0.56F, 0.88F, 0.84F
            };
            int[] trainingLabels = { 0, 0, 0, 0, 1, 1, 1, 1 };
            int queryCount = gridWidth * gridHeight;
            bool mlLinked = true;
            Mat panel;

            try
            {
                using (var samples = new Mat(8, 2, MatType.CV_32FC1))
                using (var responses = new Mat(8, 1, MatType.CV_32SC1))
                using (var queries = new Mat(queryCount, 2, MatType.CV_32FC1))
                using (var results = new Mat())
                using (SVM svm = SVM.Create())
                {
                    samples.CopyFrom<float>(trainingValues);
                    responses.CopyFrom<int>(trainingLabels);
                    float[] queryValues = new float[queryCount * 2];
                    for (int y = 0; y < gridHeight; y++)
                    {
                        for (int x = 0; x < gridWidth; x++)
                        {
                            int offset = ((y * gridWidth) + x) * 2;
                            queryValues[offset] = x / (float)(gridWidth - 1);
                            queryValues[offset + 1] = y / (float)(gridHeight - 1);
                        }
                    }
                    queries.CopyFrom<float>(queryValues);
                    svm.Type = SVMTypes.CSvc;
                    svm.SetKernel(SVMKernelTypes.Rbf);
                    svm.C = 2.0;
                    svm.Gamma = 3.0;
                    svm.TermCriteria = TermCriteria.ByCountAndEpsilon(100, 1e-6);
                    if (!svm.Train(samples, SampleTypes.RowSample, responses))
                    {
                        throw new InvalidOperationException("SVM sample training failed.");
                    }
                    svm.Predict(queries, results);
                    float[] labels = results.ToArray<float>();
                    byte[] pixels = new byte[queryCount * 3];
                    for (int i = 0; i < labels.Length; i++)
                    {
                        bool secondClass = labels[i] >= 0.5F;
                        pixels[i * 3] = secondClass ? (byte)74 : (byte)198;
                        pixels[i * 3 + 1] = secondClass ? (byte)156 : (byte)91;
                        pixels[i * 3 + 2] = secondClass ? (byte)232 : (byte)62;
                    }

                    using (var grid = new Mat(gridHeight, gridWidth, MatType.CV_8UC3))
                    {
                        grid.CopyFrom<byte>(pixels);
                        panel = new Mat();
                        ImgProcCv2.Resize(grid, panel,
                            new Size(SampleSupport.PanelWidth, SampleSupport.PanelHeight),
                            interpolation: InterpolationFlags.Nearest);
                    }
                    for (int i = 0; i < trainingLabels.Length; i++)
                    {
                        int x = (int)Math.Round(trainingValues[i * 2] * (SampleSupport.PanelWidth - 1));
                        int y = (int)Math.Round(trainingValues[i * 2 + 1] * (SampleSupport.PanelHeight - 1));
                        Scalar color = trainingLabels[i] == 0
                            ? new Scalar(255, 225, 220)
                            : new Scalar(220, 250, 255);
                        ImgProcCv2.Circle(panel, new Point(x, y), 10, color, -1, LineTypes.AntiAlias);
                        ImgProcCv2.Circle(panel, new Point(x, y), 10,
                            new Scalar(32, 36, 42), 2, LineTypes.AntiAlias);
                    }
                    SampleSupport.AddPanelLabel(panel, "SVM CLASSIFICATION", new Scalar(242, 164, 52));
                    SampleSupport.AddMetric(panel, "RBF  3,600 queries");
                }
            }
            catch (OpenCvException exception) when (exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                mlLinked = false;
                queryCount = 0;
                panel = new Mat(SampleSupport.PanelHeight, SampleSupport.PanelWidth,
                    MatType.CV_8UC3, new Scalar(31, 35, 42));
                SampleSupport.AddPanelLabel(panel, "SVM CLASSIFICATION", new Scalar(242, 164, 52));
                ImgProcCv2.PutText(panel, "Native ML module is not linked in this runtime.",
                    new Point(32, 166), HersheyFonts.HersheySimplex, 0.8,
                    new Scalar(232, 240, 245), 2, LineTypes.AntiAlias);
                ImgProcCv2.PutText(panel, "Install a runtime profile with ML support to execute SVM.",
                    new Point(32, 204), HersheyFonts.HersheySimplex, 0.62,
                    new Scalar(190, 202, 214), 1, LineTypes.AntiAlias);
                SampleSupport.AddMetric(panel, "Native ML  NOT_LINKED");
            }

            using (panel)
            {
                SampleSupport.WritePng(outputDirectory, "svm-classification.png", panel);
            }
            SampleSupport.WriteSummary("SVM classification", outputDirectory, mlLinked
                ? "queries=" + queryCount + ", kernel=RBF"
                : "status=NOT_LINKED, queries=0");
        }
    }
}
