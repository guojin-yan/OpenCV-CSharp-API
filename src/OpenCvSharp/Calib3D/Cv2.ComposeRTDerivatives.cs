using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Calib3D
{
    public static partial class Cv2
    {
        /// <summary>
        /// Composes two rotation and translation transforms and computes all eight Jacobians.
        /// 组合两个旋转和平移变换并计算全部八个 Jacobian。
        /// </summary>
        public static void ComposeRT(
            Mat rvec1,
            Mat tvec1,
            Mat rvec2,
            Mat tvec2,
            Mat rvec3,
            Mat tvec3,
            Mat dr3dr1,
            Mat dr3dt1,
            Mat dr3dr2,
            Mat dr3dt2,
            Mat dt3dr1,
            Mat dt3dt1,
            Mat dt3dr2,
            Mat dt3dt2)
        {
            ThrowIfNull(rvec1, nameof(rvec1));
            ThrowIfNull(tvec1, nameof(tvec1));
            ThrowIfNull(rvec2, nameof(rvec2));
            ThrowIfNull(tvec2, nameof(tvec2));
            ThrowIfNull(rvec3, nameof(rvec3));
            ThrowIfNull(tvec3, nameof(tvec3));
            ThrowIfNull(dr3dr1, nameof(dr3dr1));
            ThrowIfNull(dr3dt1, nameof(dr3dt1));
            ThrowIfNull(dr3dr2, nameof(dr3dr2));
            ThrowIfNull(dr3dt2, nameof(dr3dt2));
            ThrowIfNull(dt3dr1, nameof(dt3dr1));
            ThrowIfNull(dt3dt1, nameof(dt3dt1));
            ThrowIfNull(dt3dr2, nameof(dt3dr2));
            ThrowIfNull(dt3dt2, nameof(dt3dt2));

            ValidateComposeRTDerivativeInputs(rvec1, tvec1, rvec2, tvec2);
            ValidateComposeRTDerivativeOutputs(
                new[] { rvec1, tvec1, rvec2, tvec2 },
                new[]
                {
                    rvec3,
                    tvec3,
                    dr3dr1,
                    dr3dt1,
                    dr3dr2,
                    dr3dt2,
                    dt3dr1,
                    dt3dt1,
                    dt3dr2,
                    dt3dt2
                },
                new[]
                {
                    nameof(rvec3),
                    nameof(tvec3),
                    nameof(dr3dr1),
                    nameof(dr3dt1),
                    nameof(dr3dr2),
                    nameof(dr3dt2),
                    nameof(dt3dr1),
                    nameof(dt3dt1),
                    nameof(dt3dr2),
                    nameof(dt3dt2)
                });

            NativeException.ThrowIfError(NativeMethods.Calib3DComposeRTExtended(
                rvec1.NativeHandle,
                tvec1.NativeHandle,
                rvec2.NativeHandle,
                tvec2.NativeHandle,
                rvec3.NativeHandle,
                tvec3.NativeHandle,
                dr3dr1.NativeHandle,
                dr3dt1.NativeHandle,
                dr3dr2.NativeHandle,
                dr3dt2.NativeHandle,
                dt3dr1.NativeHandle,
                dt3dt1.NativeHandle,
                dt3dr2.NativeHandle,
                dt3dt2.NativeHandle));
        }

        /// <summary>
        /// Composes two transforms and returns owned vectors and Jacobian matrices.
        /// 组合两个变换并返回拥有所有权的向量和 Jacobian 矩阵。
        /// </summary>
        public static ComposeRTDerivativesResult ComposeRT(
            Mat rvec1,
            Mat tvec1,
            Mat rvec2,
            Mat tvec2)
        {
            var rvec3 = new Mat();
            var tvec3 = new Mat();
            var dr3dr1 = new Mat();
            var dr3dt1 = new Mat();
            var dr3dr2 = new Mat();
            var dr3dt2 = new Mat();
            var dt3dr1 = new Mat();
            var dt3dt1 = new Mat();
            var dt3dr2 = new Mat();
            var dt3dt2 = new Mat();

            try
            {
                ComposeRT(
                    rvec1,
                    tvec1,
                    rvec2,
                    tvec2,
                    rvec3,
                    tvec3,
                    dr3dr1,
                    dr3dt1,
                    dr3dr2,
                    dr3dt2,
                    dt3dr1,
                    dt3dt1,
                    dt3dr2,
                    dt3dt2);
                return new ComposeRTDerivativesResult(
                    rvec3,
                    tvec3,
                    dr3dr1,
                    dr3dt1,
                    dr3dr2,
                    dr3dt2,
                    dt3dr1,
                    dt3dt1,
                    dt3dr2,
                    dt3dt2);
            }
            catch
            {
                DisposeComposeRTDerivativeOutputs(
                    rvec3,
                    tvec3,
                    dr3dr1,
                    dr3dt1,
                    dr3dr2,
                    dr3dt2,
                    dt3dr1,
                    dt3dt1,
                    dt3dr2,
                    dt3dt2);
                throw;
            }
        }

        private static void ValidateComposeRTDerivativeInputs(
            Mat rvec1,
            Mat tvec1,
            Mat rvec2,
            Mat tvec2)
        {
            ValidateComposeRTDerivativeVector(rvec1, nameof(rvec1));
            ValidateComposeRTDerivativeVector(tvec1, nameof(tvec1));
            ValidateComposeRTDerivativeVector(rvec2, nameof(rvec2));
            ValidateComposeRTDerivativeVector(tvec2, nameof(tvec2));

            Mat[] inputs = { rvec1, tvec1, rvec2, tvec2 };
            string[] names = { nameof(rvec1), nameof(tvec1), nameof(rvec2), nameof(tvec2) };
            for (int index = 1; index < inputs.Length; index++)
            {
                if (inputs[index].Rows != rvec1.Rows ||
                    inputs[index].Cols != rvec1.Cols)
                {
                    throw new ArgumentException(
                        "All ComposeRT inputs must have the same vector orientation.",
                        names[index]);
                }
                if (inputs[index].Type != rvec1.Type)
                {
                    throw new ArgumentException(
                        "All ComposeRT inputs must have exactly the same type.",
                        names[index]);
                }
            }
        }

        private static void ValidateComposeRTDerivativeVector(
            Mat vector,
            string parameterName)
        {
            if (vector.Empty)
            {
                throw new ArgumentException(
                    "ComposeRT input vectors cannot be empty.",
                    parameterName);
            }
            if (!((vector.Rows == 1 && vector.Cols == 3) ||
                (vector.Rows == 3 && vector.Cols == 1)))
            {
                throw new ArgumentException(
                    "ComposeRT inputs must be 1 x 3 or 3 x 1 vectors.",
                    parameterName);
            }
            if (vector.Type != MatType.CV_32FC1 &&
                vector.Type != MatType.CV_64FC1)
            {
                throw new ArgumentException(
                    "ComposeRT inputs must be CV_32FC1 or CV_64FC1.",
                    parameterName);
            }
        }

        private static void ValidateComposeRTDerivativeOutputs(
            Mat[] inputs,
            Mat[] outputs,
            string[] outputNames)
        {
            var inputHandles = new IntPtr[inputs.Length];
            for (int index = 0; index < inputs.Length; index++)
            {
                inputHandles[index] = inputs[index].NativeHandle;
            }

            var outputHandles = new IntPtr[outputs.Length];
            for (int index = 0; index < outputs.Length; index++)
            {
                outputHandles[index] = outputs[index].NativeHandle;
                for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
                {
                    if (ComposeRTDerivativeMatsAlias(
                        outputs[index],
                        outputHandles[index],
                        inputs[inputIndex],
                        inputHandles[inputIndex]))
                    {
                        throw new ArgumentException(
                            "ComposeRT outputs must not alias any input matrix.",
                            outputNames[index]);
                    }
                }

                for (int previous = 0; previous < index; previous++)
                {
                    if (ComposeRTDerivativeMatsAlias(
                        outputs[index],
                        outputHandles[index],
                        outputs[previous],
                        outputHandles[previous]))
                    {
                        throw new ArgumentException(
                            "ComposeRT outputs must not alias each other.",
                            outputNames[index]);
                    }
                }
            }
        }

        private static bool ComposeRTDerivativeMatsAlias(
            Mat first,
            IntPtr firstHandle,
            Mat second,
            IntPtr secondHandle)
        {
            return ReferenceEquals(first, second) || firstHandle == secondHandle;
        }

        private static void DisposeComposeRTDerivativeOutputs(params Mat[] outputs)
        {
            for (int index = 0; index < outputs.Length; index++)
            {
                outputs[index].Dispose();
            }
        }
    }
}
