using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenCvSharp.Core;
using OpenCvSharp.Saliency;
using ImgProcCv2 = OpenCvSharp.ImgProc.Cv2;

namespace OpenCvSharp.Tests.Saliency
{
    public sealed class SaliencyTests
    {
        [Fact]
        public void ObjectnessResultObjectsExposeValues()
        {
            var box = new ObjectnessBINGBox(1, 2, 8, 10);
            var result = new ObjectnessBINGResult(true, new[] { box }, new[] { 0.75F });

            Assert.True(result.Success);
            Assert.Equal(1, result.Count);
            Assert.True(result.HasObjectnessValues);
            Assert.Equal(1, result.ObjectnessValueCount);
            Assert.Equal(7, box.Width);
            Assert.Equal(8, box.Height);
            Assert.Equal(new Rect(1, 2, 7, 8).ToString(), box.ToRect().ToString());
            Assert.Equal(new ObjectnessBINGBox(1, 2, 8, 10), box);
            Assert.True(box == new ObjectnessBINGBox(1, 2, 8, 10));
            Assert.True(box != new ObjectnessBINGBox(1, 2, 8, 11));
            Assert.False(box.Equals("not a box"));
            Assert.Equal(new ObjectnessBINGBox(1, 2, 8, 10).GetHashCode(), box.GetHashCode());
            Assert.Equal("{MinX=1,MinY=2,MaxX=8,MaxY=10}", box.ToString());
            Assert.Equal(0.75F, result.ObjectnessValues[0], 3);
            Assert.Equal("ObjectnessBINGResult(Success=True, Count=1, ObjectnessValues=1)", result.ToString());
        }

        [Fact]
        public void ObjectnessBINGResultClonesArraysAndGuardsNullInputs()
        {
            var boxes = new[] { new ObjectnessBINGBox(1, 2, 8, 10), new ObjectnessBINGBox(3, 4, 9, 12) };
            var values = new[] { 0.75F, 0.25F };
            var result = new ObjectnessBINGResult(false, boxes, values);

            boxes[0] = new ObjectnessBINGBox(9, 9, 10, 10);
            values[0] = 1.0F;

            Assert.False(result.Success);
            Assert.Equal(2, result.Count);
            Assert.True(result.HasObjectnessValues);
            Assert.Equal(2, result.ObjectnessValueCount);
            Assert.Equal(new ObjectnessBINGBox(1, 2, 8, 10), result.Boxes[0]);
            Assert.Equal(0.75F, result.ObjectnessValues[0], 3);

            ObjectnessBINGBox[] returnedBoxes = result.Boxes;
            float[] returnedValues = result.ObjectnessValues;
            returnedBoxes[0] = new ObjectnessBINGBox(20, 21, 22, 23);
            returnedValues[0] = 2.0F;

            Assert.Equal(new ObjectnessBINGBox(1, 2, 8, 10), result.Boxes[0]);
            Assert.Equal(0.75F, result.ObjectnessValues[0], 3);
            Assert.Equal("ObjectnessBINGResult(Success=False, Count=2, ObjectnessValues=2)", result.ToString());

            var empty = new ObjectnessBINGResult(true, new ObjectnessBINGBox[0], new float[0]);
            Assert.True(empty.Success);
            Assert.Equal(0, empty.Count);
            Assert.False(empty.HasObjectnessValues);
            Assert.Equal(0, empty.ObjectnessValueCount);
            Assert.Equal("ObjectnessBINGResult(Success=True, Count=0, ObjectnessValues=0)", empty.ToString());

            Assert.Throws<ArgumentNullException>(() => new ObjectnessBINGResult(true, null!, values));
            Assert.Throws<ArgumentNullException>(() => new ObjectnessBINGResult(true, boxes, null!));
            Assert.Throws<ArgumentException>(() => new ObjectnessBINGResult(true, boxes, new[] { 0.1F }));
        }

        [Fact]
        public void ObjectnessBINGBoxHasSequentialOpenCvCompatibleLayout()
        {
            Assert.Equal(16, Marshal.SizeOf<ObjectnessBINGBox>());

            Assert.Equal(0, FieldOffset<ObjectnessBINGBox>("<MinX>k__BackingField"));
            Assert.Equal(4, FieldOffset<ObjectnessBINGBox>("<MinY>k__BackingField"));
            Assert.Equal(8, FieldOffset<ObjectnessBINGBox>("<MaxX>k__BackingField"));
            Assert.Equal(12, FieldOffset<ObjectnessBINGBox>("<MaxY>k__BackingField"));
        }

        [Fact]
        public void ObjectnessBINGPathValidationRejectsInvalidStringsBeforeNativeHandleAccess()
        {
            var objectness = (ObjectnessBING)RuntimeHelpers.GetUninitializedObject(typeof(ObjectnessBING));

            Assert.Throws<ArgumentNullException>(() => objectness.SetTrainingPath(null!));
            Assert.Throws<ArgumentNullException>(() => objectness.SetBBResDir(null!));
            Assert.Throws<ArgumentException>(() => objectness.SetTrainingPath("training\0path"));
            Assert.Throws<ArgumentException>(() => objectness.SetBBResDir("results\0dir"));
        }

        [Fact]
        public void FactoryReturnsObjectOrExplicitNativeBoundary()
        {
            if (TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            try
            {
                using (StaticSaliencySpectralResidual.Create())
                {
                }

                using (ObjectnessBING.Create())
                {
                }
            }
            catch (OpenCvException ex) when (IsSaliencyModuleMissing(ex))
            {
                Assert.True(IsSaliencyModuleMissing(ex), ex.Message);
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }
        }

        [Fact]
        public void ManagedArgumentValidationRunsBeforeNativeCall()
        {
            StaticSaliencySpectralResidual? saliency = TryCreateSpectralResidual();
            if (saliency == null)
            {
                return;
            }

            using (saliency)
            using (Mat image = CreateImage())
            using (Mat output = new Mat())
            {
                Assert.Throws<ArgumentNullException>(() => saliency.ComputeSaliency(null!, output));
                Assert.Throws<ArgumentNullException>(() => saliency.ComputeSaliency(image, null!));
                Assert.Throws<ArgumentNullException>(() => saliency.ComputeSaliency(null!));
                Assert.Throws<ArgumentNullException>(() => saliency.ComputeBinaryMap(null!, output));
                Assert.Throws<ArgumentNullException>(() => saliency.ComputeBinaryMap(output, null!));
                Assert.Throws<ArgumentNullException>(() => saliency.ComputeBinaryMap(null!));
                Assert.Throws<ArgumentException>(() => saliency.ComputeBinaryMap(image, output));
                Assert.Throws<ArgumentException>(() => saliency.ComputeBinaryMap(image));
            }

            ObjectnessBING? objectness = TryCreateObjectnessBing();
            if (objectness == null)
            {
                return;
            }

            using (objectness)
            using (Mat image = CreateImage())
            {
                Assert.Throws<ArgumentNullException>(() => objectness.SetTrainingPath(null!));
                Assert.Throws<ArgumentNullException>(() => objectness.SetBBResDir(null!));
                Assert.Throws<ArgumentException>(() => objectness.SetTrainingPath("training\0path"));
                Assert.Throws<ArgumentException>(() => objectness.SetBBResDir("results\0dir"));
                Assert.Throws<ArgumentNullException>(() => objectness.ComputeObjectness(null!));
                Assert.Empty(objectness.GetBoxes());
                Assert.Empty(objectness.GetObjectnessValues());
            }
        }

        [Fact]
        public void DisposedStateRejectsCalls()
        {
            StaticSaliencySpectralResidual? saliency = TryCreateSpectralResidual();
            if (saliency == null)
            {
                return;
            }

            saliency.Dispose();
            Assert.True(saliency.IsDisposed);
            using (Mat image = CreateImage())
            using (Mat output = new Mat())
            {
                Assert.Throws<ObjectDisposedException>(() => saliency.ComputeSaliency(image, output));
                Assert.Throws<ObjectDisposedException>(() => saliency.ComputeSaliency(image));
                Assert.Throws<ObjectDisposedException>(() => saliency.ComputeBinaryMap(image, output));
                Assert.Throws<ObjectDisposedException>(() => saliency.ComputeBinaryMap(image));
                Assert.Throws<ObjectDisposedException>(() => saliency.ImageWidth);
                Assert.Throws<ObjectDisposedException>(() => saliency.ImageWidth = image.Cols);
                Assert.Throws<ObjectDisposedException>(() => saliency.ImageHeight);
                Assert.Throws<ObjectDisposedException>(() => saliency.ImageHeight = image.Rows);
            }

            using (ObjectnessBING? objectness = TryCreateObjectnessBing())
            using (Mat image = CreateImage())
            {
                if (objectness != null)
                {
                    objectness.Dispose();
                    Assert.True(objectness.IsDisposed);
                    Assert.Throws<ObjectDisposedException>(() => objectness.ComputeObjectness(image));
                    Assert.Throws<ObjectDisposedException>(() => objectness.SetTrainingPath(Path.GetTempPath()));
                    Assert.Throws<ObjectDisposedException>(() => objectness.SetBBResDir(Path.GetTempPath()));
                    Assert.Throws<ObjectDisposedException>(() => objectness.GetBoxes());
                    Assert.Throws<ObjectDisposedException>(() => objectness.GetObjectnessValues());
                    Assert.Throws<ObjectDisposedException>(() => objectness.Base);
                    Assert.Throws<ObjectDisposedException>(() => objectness.Base = 2.0);
                    Assert.Throws<ObjectDisposedException>(() => objectness.NSS);
                    Assert.Throws<ObjectDisposedException>(() => objectness.NSS = 3);
                    Assert.Throws<ObjectDisposedException>(() => objectness.W);
                    Assert.Throws<ObjectDisposedException>(() => objectness.W = 8);
                }
            }
        }

        [Fact]
        public void StaticSaliencySmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat image = CreateImage())
            using (StaticSaliencySpectralResidual spectral = StaticSaliencySpectralResidual.Create())
            using (StaticSaliencyFineGrained fine = StaticSaliencyFineGrained.Create())
            using (Mat spectralMap = new Mat())
            using (Mat binaryMap = new Mat())
            using (Mat fineMap = new Mat())
            {
                spectral.ImageWidth = image.Cols;
                spectral.ImageHeight = image.Rows;
                Assert.Equal(image.Cols, spectral.ImageWidth);
                Assert.Equal(image.Rows, spectral.ImageHeight);

                Assert.True(spectral.ComputeSaliency(image, spectralMap));
                Assert.True(spectral.ComputeBinaryMap(spectralMap, binaryMap));
                Assert.True(fine.ComputeSaliency(image, fineMap));

                Assert.Equal(image.Rows, spectralMap.Rows);
                Assert.Equal(image.Cols, spectralMap.Cols);
                Assert.False(binaryMap.Empty);
                Assert.False(fineMap.Empty);
            }
        }

        [Fact]
        public void MotionSaliencySmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (Mat frame = CreateMotionImage())
            using (Mat colorFrame = CreateImage())
            using (MotionSaliencyBinWangApr2014 motion = MotionSaliencyBinWangApr2014.Create())
            using (Mat map = new Mat())
            {
                motion.SetImageSize(frame.Cols, frame.Rows);
                Assert.True(motion.Init());
                Assert.Equal(frame.Cols, motion.ImageWidth);
                Assert.Equal(frame.Rows, motion.ImageHeight);
                Assert.True(motion.ComputeSaliency(frame, map));
                Assert.False(map.Empty);
                Assert.Throws<ArgumentException>(() => motion.ComputeSaliency(colorFrame, map));
                Assert.Throws<ArgumentException>(() => motion.ComputeSaliency(colorFrame));
            }
        }

        [Fact]
        public void ObjectnessBingSmokeRunsWhenNativeSmokeIsEnabled()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            using (ObjectnessBING objectness = ObjectnessBING.Create())
            {
                objectness.SetTrainingPath(Path.GetTempPath());
                objectness.SetBBResDir(Path.GetTempPath());
                objectness.Base = 2.0;
                objectness.NSS = 3;
                objectness.W = 8;

                Assert.Equal(2.0, objectness.Base, 3);
                Assert.Equal(3, objectness.NSS);
                Assert.Equal(8, objectness.W);
                Assert.Empty(objectness.GetBoxes());
                Assert.Empty(objectness.GetObjectnessValues());
            }
        }

        private static StaticSaliencySpectralResidual? TryCreateSpectralResidual()
        {
            try
            {
                return StaticSaliencySpectralResidual.Create();
            }
            catch (OpenCvException ex) when (IsSaliencyModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static ObjectnessBING? TryCreateObjectnessBing()
        {
            try
            {
                return ObjectnessBING.Create();
            }
            catch (OpenCvException ex) when (IsSaliencyModuleMissing(ex))
            {
                return null;
            }
            catch (DllNotFoundException)
            {
                return null;
            }
            catch (EntryPointNotFoundException)
            {
                return null;
            }
        }

        private static Mat CreateImage()
        {
            var image = new Mat(32, 32, MatType.CV_8UC3, new Scalar(20, 30, 40));
            ImgProcCv2.Rectangle(image, new Rect(6, 6, 10, 12), new Scalar(230, 30, 80), -1);
            ImgProcCv2.Circle(image, new Point(23, 22), 5, new Scalar(40, 220, 120), -1);
            return image;
        }

        private static Mat CreateMotionImage()
        {
            var image = new Mat(32, 32, MatType.CV_8UC1, new Scalar(40));
            ImgProcCv2.Rectangle(image, new Rect(6, 6, 10, 12), new Scalar(220), -1);
            ImgProcCv2.Circle(image, new Point(23, 22), 5, new Scalar(120), -1);
            return image;
        }

        private static bool IsSaliencyModuleMissing(OpenCvException exception)
        {
            return exception.Message.IndexOf("saliency", StringComparison.OrdinalIgnoreCase) >= 0
                && (exception.Message.IndexOf("OpenCV", StringComparison.OrdinalIgnoreCase) >= 0
                    || exception.Message.IndexOf("NOT_LINKED", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static int FieldOffset<T>(string fieldName)
        {
            return Marshal.OffsetOf<T>(fieldName).ToInt32();
        }

    }
}
