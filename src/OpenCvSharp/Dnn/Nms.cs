using System;
using System.Collections.Generic;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Dnn
{
    /// <summary>Selects the score-decay function used by <see cref="Cv2.SoftNMSBoxes"/>.</summary>
    public enum SoftNMSMethod
    {
        /// <summary>Linearly decays scores according to overlap.</summary>
        Linear = 1,
        /// <summary>Applies Gaussian score decay.</summary>
        Gaussian = 2
    }

    /// <summary>Results returned by <see cref="Cv2.SoftNMSBoxes"/>.</summary>
    public readonly struct SoftNmsResult
    {
        /// <summary>Creates a Soft-NMS result.</summary>
        public SoftNmsResult(int[] indices, float[] updatedScores)
        {
            Indices = indices ?? throw new ArgumentNullException(nameof(indices));
            UpdatedScores = updatedScores ?? throw new ArgumentNullException(nameof(updatedScores));
        }

        /// <summary>Gets the source box indices retained by Soft-NMS.</summary>
        public int[] Indices { get; }

        /// <summary>Gets the updated scores returned by OpenCV, in the same working order as the native result.</summary>
        public float[] UpdatedScores { get; }
    }

    public static partial class Cv2
    {
        /// <summary>Applies non-maximum suppression to axis-aligned integer rectangles.</summary>
        public static int[] NMSBoxes(IReadOnlyList<Rect> boxes, IReadOnlyList<float> scores, float scoreThreshold, float nmsThreshold, float eta = 1.0F, int topK = 0)
        {
            ValidateNmsInputs(boxes, scores, nameof(boxes), nameof(scores), scoreThreshold, nmsThreshold, eta, topK);
            if (boxes.Count == 0) return Array.Empty<int>();
            var nativeBoxes = ToNative(boxes);
            var nativeScores = ToArray(scores);
            var indices = new int[boxes.Count];
            NativeException.ThrowIfError(NativeMethods.DnnNmsBoxesRect(nativeBoxes, nativeBoxes.Length, nativeScores, nativeScores.Length, scoreThreshold, nmsThreshold, eta, topK, indices, indices.Length, out int count));
            return Trim(indices, count);
        }

        /// <summary>Applies non-maximum suppression to double-precision rectangles.</summary>
        public static int[] NMSBoxes(IReadOnlyList<Rect2d> boxes, IReadOnlyList<float> scores, float scoreThreshold, float nmsThreshold, float eta = 1.0F, int topK = 0)
        {
            ValidateNmsInputs(boxes, scores, nameof(boxes), nameof(scores), scoreThreshold, nmsThreshold, eta, topK);
            if (boxes.Count == 0) return Array.Empty<int>();
            var nativeBoxes = ToNative(boxes);
            var nativeScores = ToArray(scores);
            var indices = new int[boxes.Count];
            NativeException.ThrowIfError(NativeMethods.DnnNmsBoxesRect2d(nativeBoxes, nativeBoxes.Length, nativeScores, nativeScores.Length, scoreThreshold, nmsThreshold, eta, topK, indices, indices.Length, out int count));
            return Trim(indices, count);
        }

        /// <summary>Applies non-maximum suppression to rotated rectangles.</summary>
        public static int[] NMSBoxes(IReadOnlyList<RotatedRect> boxes, IReadOnlyList<float> scores, float scoreThreshold, float nmsThreshold, float eta = 1.0F, int topK = 0)
        {
            ValidateNmsInputs(boxes, scores, nameof(boxes), nameof(scores), scoreThreshold, nmsThreshold, eta, topK);
            if (boxes.Count == 0) return Array.Empty<int>();
            var nativeBoxes = ToNative(boxes);
            var nativeScores = ToArray(scores);
            var indices = new int[boxes.Count];
            NativeException.ThrowIfError(NativeMethods.DnnNmsBoxesRotatedRect(nativeBoxes, nativeBoxes.Length, nativeScores, nativeScores.Length, scoreThreshold, nmsThreshold, eta, topK, indices, indices.Length, out int count));
            return Trim(indices, count);
        }

        /// <summary>Applies class-aware non-maximum suppression to integer rectangles.</summary>
        public static int[] NMSBoxesBatched(IReadOnlyList<Rect> boxes, IReadOnlyList<float> scores, IReadOnlyList<int> classIds, float scoreThreshold, float nmsThreshold, float eta = 1.0F, int topK = 0)
        {
            ValidateNmsInputs(boxes, scores, nameof(boxes), nameof(scores), scoreThreshold, nmsThreshold, eta, topK);
            ValidateClassIds(boxes, classIds);
            if (boxes.Count == 0) return Array.Empty<int>();
            var nativeBoxes = ToNative(boxes);
            var nativeScores = ToArray(scores);
            var nativeClassIds = ToArray(classIds);
            var indices = new int[boxes.Count];
            NativeException.ThrowIfError(NativeMethods.DnnNmsBoxesBatchedRect(nativeBoxes, nativeBoxes.Length, nativeScores, nativeScores.Length, nativeClassIds, nativeClassIds.Length, scoreThreshold, nmsThreshold, eta, topK, indices, indices.Length, out int count));
            return Trim(indices, count);
        }

        /// <summary>Applies class-aware non-maximum suppression to double-precision rectangles.</summary>
        public static int[] NMSBoxesBatched(IReadOnlyList<Rect2d> boxes, IReadOnlyList<float> scores, IReadOnlyList<int> classIds, float scoreThreshold, float nmsThreshold, float eta = 1.0F, int topK = 0)
        {
            ValidateNmsInputs(boxes, scores, nameof(boxes), nameof(scores), scoreThreshold, nmsThreshold, eta, topK);
            ValidateClassIds(boxes, classIds);
            if (boxes.Count == 0) return Array.Empty<int>();
            var nativeBoxes = ToNative(boxes);
            var nativeScores = ToArray(scores);
            var nativeClassIds = ToArray(classIds);
            var indices = new int[boxes.Count];
            NativeException.ThrowIfError(NativeMethods.DnnNmsBoxesBatchedRect2d(nativeBoxes, nativeBoxes.Length, nativeScores, nativeScores.Length, nativeClassIds, nativeClassIds.Length, scoreThreshold, nmsThreshold, eta, topK, indices, indices.Length, out int count));
            return Trim(indices, count);
        }

        /// <summary>Applies Soft-NMS to axis-aligned integer rectangles.</summary>
        public static SoftNmsResult SoftNMSBoxes(IReadOnlyList<Rect> boxes, IReadOnlyList<float> scores, float scoreThreshold, float nmsThreshold, int topK = 0, float sigma = 0.5F, SoftNMSMethod method = SoftNMSMethod.Gaussian)
        {
            ValidateNmsInputs(boxes, scores, nameof(boxes), nameof(scores), scoreThreshold, nmsThreshold, 1.0F, topK);
            if (!IsFinite(sigma) || sigma < 0.0F) throw new ArgumentOutOfRangeException(nameof(sigma));
            if (method != SoftNMSMethod.Linear && method != SoftNMSMethod.Gaussian) throw new ArgumentOutOfRangeException(nameof(method));
            if (boxes.Count == 0) return new SoftNmsResult(Array.Empty<int>(), Array.Empty<float>());
            var nativeBoxes = ToNative(boxes);
            var nativeScores = ToArray(scores);
            var updatedScores = new float[boxes.Count];
            var indices = new int[boxes.Count];
            NativeException.ThrowIfError(NativeMethods.DnnSoftNmsBoxesRect(nativeBoxes, nativeBoxes.Length, nativeScores, nativeScores.Length, scoreThreshold, nmsThreshold, updatedScores, updatedScores.Length, out int scoreCount, indices, indices.Length, out int indexCount, topK, sigma, (int)method));
            return new SoftNmsResult(Trim(indices, indexCount), Trim(updatedScores, scoreCount));
        }

        private static void ValidateNmsInputs<TBox>(IReadOnlyList<TBox> boxes, IReadOnlyList<float> scores, string boxesName, string scoresName, float scoreThreshold, float nmsThreshold, float eta, int topK)
        {
            if (boxes == null) throw new ArgumentNullException(boxesName);
            if (scores == null) throw new ArgumentNullException(scoresName);
            if (boxes.Count != scores.Count) throw new ArgumentException("Boxes and scores must have the same length.", scoresName);
            if (!IsFinite(scoreThreshold)) throw new ArgumentOutOfRangeException(nameof(scoreThreshold));
            if (!IsFinite(nmsThreshold) || nmsThreshold < 0.0F) throw new ArgumentOutOfRangeException(nameof(nmsThreshold));
            if (!IsFinite(eta) || eta <= 0.0F) throw new ArgumentOutOfRangeException(nameof(eta));
            if (topK < 0) throw new ArgumentOutOfRangeException(nameof(topK));
        }

        private static void ValidateClassIds<TBox>(IReadOnlyList<TBox> boxes, IReadOnlyList<int> classIds)
        {
            if (classIds == null) throw new ArgumentNullException(nameof(classIds));
            if (boxes.Count != classIds.Count) throw new ArgumentException("Boxes and class IDs must have the same length.", nameof(classIds));
        }

        private static NativeDnnRect[] ToNative(IReadOnlyList<Rect> values)
        {
            var result = new NativeDnnRect[values.Count];
            for (int i = 0; i < result.Length; i++) result[i] = new NativeDnnRect { X = values[i].X, Y = values[i].Y, Width = values[i].Width, Height = values[i].Height };
            return result;
        }

        private static NativeDnnRect2d[] ToNative(IReadOnlyList<Rect2d> values)
        {
            var result = new NativeDnnRect2d[values.Count];
            for (int i = 0; i < result.Length; i++) result[i] = new NativeDnnRect2d { X = values[i].X, Y = values[i].Y, Width = values[i].Width, Height = values[i].Height };
            return result;
        }

        private static NativeDnnRotatedRect[] ToNative(IReadOnlyList<RotatedRect> values)
        {
            var result = new NativeDnnRotatedRect[values.Count];
            for (int i = 0; i < result.Length; i++) result[i] = new NativeDnnRotatedRect { CenterX = values[i].Center.X, CenterY = values[i].Center.Y, Width = values[i].Size.Width, Height = values[i].Size.Height, Angle = values[i].Angle };
            return result;
        }

        private static float[] ToArray(IReadOnlyList<float> values)
        {
            var result = new float[values.Count];
            for (int i = 0; i < result.Length; i++) result[i] = values[i];
            return result;
        }

        private static int[] ToArray(IReadOnlyList<int> values)
        {
            var result = new int[values.Count];
            for (int i = 0; i < result.Length; i++) result[i] = values[i];
            return result;
        }

        private static int[] Trim(int[] values, int count)
        {
            if (count < 0 || count > values.Length) throw new OpenCvException("Native DNN NMS index count is invalid.");
            if (count == values.Length) return values;
            var result = new int[count];
            Array.Copy(values, result, count);
            return result;
        }

        private static float[] Trim(float[] values, int count)
        {
            if (count < 0 || count > values.Length) throw new OpenCvException("Native DNN Soft-NMS score count is invalid.");
            if (count == values.Length) return values;
            var result = new float[count];
            Array.Copy(values, result, count);
            return result;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
