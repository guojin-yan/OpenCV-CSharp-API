using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Saliency
{
    /// <summary>
    /// OpenCV contrib ObjectnessBING objectness proposal algorithm.
    /// OpenCV contrib ObjectnessBING 目标候选框算法。
    /// </summary>
    public sealed class ObjectnessBING : Saliency
    {
        private ObjectnessBING(IntPtr nativeHandle)
            : base(nativeHandle)
        {
        }

        /// <summary>Gets or sets the window-size quantization base. 获取或设置窗口尺寸量化 base。</summary>
        public double Base
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGGetBase(NativeHandle, out double value));
                return value;
            }
            set
            {
                NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGSetBase(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets non-maximal suppression size. 获取或设置非极大值抑制尺寸。</summary>
        public int NSS
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGGetNSS(NativeHandle, out int value));
                return value;
            }
            set
            {
                NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGSetNSS(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the BING feature window size. 获取或设置 BING 特征窗口尺寸。</summary>
        public int W
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGGetW(NativeHandle, out int value));
                return value;
            }
            set
            {
                NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGSetW(NativeHandle, value));
            }
        }

        /// <summary>Creates an ObjectnessBING instance. 创建 ObjectnessBING 实例。</summary>
        public static ObjectnessBING Create()
        {
            NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGCreate(out IntPtr nativeHandle));
            return new ObjectnessBING(nativeHandle);
        }

        /// <summary>Sets the trained model directory path. 设置训练模型目录路径。</summary>
        public void SetTrainingPath(string trainingPath)
        {
            byte[] nativePath = SaliencyStringConvert.ToNullTerminatedUtf8(trainingPath, nameof(trainingPath));
            NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGSetTrainingPath(NativeHandle, nativePath));
        }

        /// <summary>Sets the optional bounding-box result directory. 设置可选的候选框结果目录。</summary>
        public void SetBBResDir(string resultsDir)
        {
            byte[] nativePath = SaliencyStringConvert.ToNullTerminatedUtf8(resultsDir, nameof(resultsDir));
            NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGSetBBResDir(NativeHandle, nativePath));
        }

        /// <summary>Computes objectness boxes and values for an image. 计算图像的目标候选框和值。</summary>
        public ObjectnessBINGResult ComputeObjectness(Mat image)
        {
            ValidateNotNull(image, nameof(image));
            NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGCompute(NativeHandle, image.NativeHandle, out int success));
            return new ObjectnessBINGResult(success != 0, GetBoxes(), GetObjectnessValues());
        }

        /// <summary>Gets cached boxes from the last objectness computation. 获取上次 objectness 计算缓存的候选框。</summary>
        public ObjectnessBINGBox[] GetBoxes()
        {
            NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGGetBoxesCount(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<ObjectnessBINGBox>();
            }

            var nativeBoxes = new int[count * 4];
            NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGGetBoxesFill(NativeHandle, nativeBoxes, count, out int written));
            return ToBoxes(nativeBoxes, Math.Max(0, Math.Min(written, count)));
        }

        /// <summary>Gets cached objectness values from the last computation. 获取上次计算缓存的 objectness 值。</summary>
        public float[] GetObjectnessValues()
        {
            NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGGetObjectnessValuesCount(NativeHandle, out int count));
            if (count <= 0)
            {
                return Array.Empty<float>();
            }

            var values = new float[count];
            NativeException.ThrowIfError(NativeMethods.SaliencyObjectnessBINGGetObjectnessValuesFill(NativeHandle, values, values.Length, out int written));
            return Trim(values, written);
        }

        private static ObjectnessBINGBox[] ToBoxes(int[] nativeBoxes, int count)
        {
            var result = new ObjectnessBINGBox[count];
            for (int i = 0; i < result.Length; i++)
            {
                int offset = i * 4;
                result[i] = new ObjectnessBINGBox(nativeBoxes[offset], nativeBoxes[offset + 1], nativeBoxes[offset + 2], nativeBoxes[offset + 3]);
            }

            return result;
        }

        private static float[] Trim(float[] values, int count)
        {
            int resultCount = Math.Max(0, Math.Min(count, values.Length));
            if (resultCount == values.Length)
            {
                return values;
            }

            var result = new float[resultCount];
            Array.Copy(values, result, result.Length);
            return result;
        }
    }
}
