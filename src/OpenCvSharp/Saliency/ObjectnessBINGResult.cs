using System;

namespace OpenCvSharp.Saliency
{
    /// <summary>
    /// ObjectnessBING candidate boxes and their optional scores.
    /// ObjectnessBING 候选框及其可选分数。
    /// </summary>
    public sealed class ObjectnessBINGResult
    {
        /// <summary>Initializes a result object. 初始化结果对象。</summary>
        public ObjectnessBINGResult(bool success, ObjectnessBINGBox[] boxes, float[] objectnessValues)
        {
            if (boxes == null)
            {
                throw new ArgumentNullException(nameof(boxes));
            }

            if (objectnessValues == null)
            {
                throw new ArgumentNullException(nameof(objectnessValues));
            }

            if (objectnessValues.Length != 0 && objectnessValues.Length != boxes.Length)
            {
                throw new ArgumentException("Objectness value count must be zero or match the box count.", nameof(objectnessValues));
            }

            Success = success;
            this.boxes = Clone(boxes);
            this.objectnessValues = Clone(objectnessValues);
        }

        private readonly ObjectnessBINGBox[] boxes;
        private readonly float[] objectnessValues;

        /// <summary>Gets whether OpenCV reported a successful computation. 获取 OpenCV 是否报告计算成功。</summary>
        public bool Success { get; }

        /// <summary>Gets candidate boxes. 获取候选框。</summary>
        public ObjectnessBINGBox[] Boxes
        {
            get { return Clone(boxes); }
        }

        /// <summary>Gets objectness values in the same order as boxes when OpenCV provides them. 获取与候选框同序的 objectness 值。</summary>
        public float[] ObjectnessValues
        {
            get { return Clone(objectnessValues); }
        }

        /// <summary>Gets the number of boxes. 获取候选框数量。</summary>
        public int Count
        {
            get { return boxes.Length; }
        }

        /// <summary>Gets whether score values were returned. 获取是否返回了分数值。</summary>
        public bool HasObjectnessValues
        {
            get { return objectnessValues.Length > 0; }
        }

        /// <summary>Gets the number of objectness values. 获取 objectness 值数量。</summary>
        public int ObjectnessValueCount
        {
            get { return objectnessValues.Length; }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{nameof(ObjectnessBINGResult)}(" +
                $"{nameof(Success)}={Success}, " +
                $"{nameof(Count)}={Count}, " +
                $"{nameof(ObjectnessValues)}={ObjectnessValueCount})";
        }

        private static ObjectnessBINGBox[] Clone(ObjectnessBINGBox[] values)
        {
            var clone = new ObjectnessBINGBox[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }

        private static float[] Clone(float[] values)
        {
            var clone = new float[values.Length];
            Array.Copy(values, clone, clone.Length);
            return clone;
        }
    }
}
