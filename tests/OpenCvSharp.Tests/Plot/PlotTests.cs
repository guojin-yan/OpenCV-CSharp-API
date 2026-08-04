using System;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Plot;

namespace JYPPX.OpenCvSharp.Tests.Plot
{
    public sealed class PlotTests
    {
        [Fact]
        public void FactoriesValidateManagedArguments()
        {
            using (var data = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => Plot2d.Create(null!));
                Assert.Throws<ArgumentNullException>(() => Plot2d.Create(null!, data));
                Assert.Throws<ArgumentNullException>(() => Plot2d.Create(data, null!));
                Assert.Throws<ArgumentNullException>(() => PlotCv2.CreatePlot2d(null!));
                Assert.Throws<ArgumentNullException>(() => PlotCv2.CreatePlot2d(null!, data));
                Assert.Throws<ArgumentNullException>(() => PlotCv2.CreatePlot2d(data, null!));
            }
        }

        [Fact]
        public void RenderSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat y = CreateSeries(0.0F, 1.0F, 0.0F, 2.0F))
            using (Plot2d plot = Plot2d.Create(y))
            using (Mat rendered = new Mat())
            {
                plot.SetPlotSize(480, 320)
                    .SetMinX(0.0)
                    .SetMaxX(3.0)
                    .SetMinY(-1.0)
                    .SetMaxY(3.0)
                    .SetPlotLineWidth(2)
                    .SetNeedPlotLine(true)
                    .SetPlotLineColor(new Scalar(0, 0, 255))
                    .SetPlotBackgroundColor(new Scalar(255, 255, 255))
                    .SetPlotAxisColor(new Scalar(0, 0, 0))
                    .SetPlotGridColor(new Scalar(200, 200, 200))
                    .SetPlotTextColor(new Scalar(30, 30, 30))
                    .SetShowGrid(true)
                    .SetShowText(false)
                    .SetGridLinesNumber(4)
                    .SetInvertOrientation(false)
                    .SetPointIdxToPrint(1);

                plot.Render(rendered);

                Assert.False(plot.IsDisposed);
                Assert.False(rendered.Empty);
                Assert.Equal(320, rendered.Rows);
                Assert.Equal(480, rendered.Cols);
                Assert.Equal(3, rendered.Channels);
            }
        }

        [Fact]
        public void XYRenderAndDisposedStateRunWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat x = CreateSeries(0.0F, 1.0F, 2.0F, 3.0F))
            using (Mat y = CreateSeries(1.0F, 2.0F, 1.0F, 3.0F))
            {
                Plot2d plot = PlotCv2.CreatePlot2d(x, y);
                using (plot)
                using (Mat rendered = plot.SetPlotSize(420, 320).SetNeedPlotLine(false).Render())
                {
                    Assert.False(rendered.Empty);
                    Assert.Equal(320, rendered.Rows);
                    Assert.Equal(420, rendered.Cols);
                }

                Assert.True(plot.IsDisposed);
                Assert.Throws<ObjectDisposedException>(() => plot.SetMinX(0.0));
                Assert.Throws<ObjectDisposedException>(() => plot.SetMinY(0.0));
                Assert.Throws<ObjectDisposedException>(() => plot.SetMaxX(1.0));
                Assert.Throws<ObjectDisposedException>(() => plot.SetMaxY(1.0));
                Assert.Throws<ObjectDisposedException>(() => plot.SetPlotLineWidth(1));
                Assert.Throws<ObjectDisposedException>(() => plot.SetNeedPlotLine(true));
                Assert.Throws<ObjectDisposedException>(() => plot.SetPlotLineColor(new Scalar(255, 0, 0)));
                Assert.Throws<ObjectDisposedException>(() => plot.SetPlotBackgroundColor(new Scalar(255, 255, 255)));
                Assert.Throws<ObjectDisposedException>(() => plot.SetPlotAxisColor(new Scalar(0, 0, 0)));
                Assert.Throws<ObjectDisposedException>(() => plot.SetPlotGridColor(new Scalar(200, 200, 200)));
                Assert.Throws<ObjectDisposedException>(() => plot.SetPlotTextColor(new Scalar(30, 30, 30)));
                Assert.Throws<ObjectDisposedException>(() => plot.SetPlotSize(320, 240));
                Assert.Throws<ObjectDisposedException>(() => plot.SetShowGrid(true));
                Assert.Throws<ObjectDisposedException>(() => plot.SetShowText(true));
                Assert.Throws<ObjectDisposedException>(() => plot.SetGridLinesNumber(4));
                Assert.Throws<ObjectDisposedException>(() => plot.SetInvertOrientation(false));
                Assert.Throws<ObjectDisposedException>(() => plot.SetPointIdxToPrint(0));
                Assert.Throws<ObjectDisposedException>(() => plot.Render(new Mat()));
                Assert.Throws<ObjectDisposedException>(() => plot.Render());
            }
        }

        [Fact]
        public void RenderValidatesManagedArgumentsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat y = CreateSeries(0.0F, 1.0F))
            using (Plot2d plot = Plot2d.Create(y))
            {
                Assert.Throws<ArgumentNullException>(() => plot.Render(null!));
            }
        }

        private static Mat CreateSeries(params double[] values)
        {
            var mat = new Mat(values.Length, 1, MatType.CV_64FC1);
            mat.CopyFrom<double>(values);
            return mat;
        }

    }
}
