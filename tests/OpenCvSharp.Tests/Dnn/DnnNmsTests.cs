using System;
using JYPPX.OpenCvSharp.Core;
using DnnCv2 = JYPPX.OpenCvSharp.Dnn.Cv2;
using JYPPX.OpenCvSharp.Dnn;

namespace JYPPX.OpenCvSharp.Tests.Dnn
{
    public sealed class DnnNmsTests
    {
        [Fact]
        public void ManagedValidationRejectsInconsistentNmsInputs()
        {
            Assert.Throws<ArgumentNullException>(() => DnnCv2.NMSBoxes((Rect[])null!, Array.Empty<float>(), 0.1F, 0.5F));
            Assert.Throws<ArgumentNullException>(() => DnnCv2.NMSBoxes(Array.Empty<Rect>(), null!, 0.1F, 0.5F));
            Assert.Throws<ArgumentException>(() => DnnCv2.NMSBoxes(new[] { new Rect(0, 0, 1, 1) }, Array.Empty<float>(), 0.1F, 0.5F));
            Assert.Throws<ArgumentOutOfRangeException>(() => DnnCv2.NMSBoxes(Array.Empty<Rect>(), Array.Empty<float>(), float.NaN, 0.5F));
            Assert.Throws<ArgumentOutOfRangeException>(() => DnnCv2.NMSBoxes(Array.Empty<Rect>(), Array.Empty<float>(), 0.1F, -0.1F));
            Assert.Throws<ArgumentOutOfRangeException>(() => DnnCv2.NMSBoxes(Array.Empty<Rect>(), Array.Empty<float>(), 0.1F, 0.5F, 0.0F));
            Assert.Throws<ArgumentException>(() => DnnCv2.NMSBoxesBatched(new[] { new Rect(0, 0, 1, 1) }, new[] { 0.5F }, Array.Empty<int>(), 0.1F, 0.5F));
            Assert.Throws<ArgumentOutOfRangeException>(() => DnnCv2.SoftNMSBoxes(Array.Empty<Rect>(), Array.Empty<float>(), 0.1F, 0.5F, sigma: -0.1F));
            Assert.Throws<ArgumentOutOfRangeException>(() => DnnCv2.SoftNMSBoxes(Array.Empty<Rect>(), Array.Empty<float>(), 0.1F, 0.5F, method: (SoftNMSMethod)99));
        }

        [Fact]
        public void NmsFamiliesReturnOpenCvSelectionsWhenNativeRuntimeIsAvailable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled()) return;

            var boxes = new[]
            {
                new Rect(0, 0, 10, 10),
                new Rect(1, 1, 10, 10),
                new Rect(30, 30, 5, 5)
            };
            float[] scores = { 0.9F, 0.8F, 0.7F };

            Assert.Equal(new[] { 0, 2 }, DnnCv2.NMSBoxes(boxes, scores, 0.1F, 0.5F));
            Assert.Equal(new[] { 0, 1, 2 }, DnnCv2.NMSBoxesBatched(boxes, scores, new[] { 0, 1, 0 }, 0.1F, 0.5F));

            Rect2d[] boxes2d = Array.ConvertAll(boxes, box => new Rect2d(box.X, box.Y, box.Width, box.Height));
            Assert.Equal(new[] { 0, 2 }, DnnCv2.NMSBoxes(boxes2d, scores, 0.1F, 0.5F));
            Assert.Equal(new[] { 0, 1, 2 }, DnnCv2.NMSBoxesBatched(boxes2d, scores, new[] { 0, 1, 0 }, 0.1F, 0.5F));

            var rotated = new[]
            {
                new RotatedRect(new Point2f(5, 5), new Size2f(10, 10), 0),
                new RotatedRect(new Point2f(6, 6), new Size2f(10, 10), 0),
                new RotatedRect(new Point2f(32.5F, 32.5F), new Size2f(5, 5), 0)
            };
            Assert.Equal(new[] { 0, 2 }, DnnCv2.NMSBoxes(rotated, scores, 0.1F, 0.5F));

            SoftNmsResult soft = DnnCv2.SoftNMSBoxes(boxes, scores, 0.1F, 0.5F);
            Assert.NotEmpty(soft.Indices);
            Assert.Equal(soft.Indices.Length, soft.UpdatedScores.Length);
            Assert.Equal(0, soft.Indices[0]);
            Assert.Equal(scores[0], soft.UpdatedScores[0]);
        }
    }
}
