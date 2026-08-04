using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.ObjDetect
{
    /// <summary>
    /// Represents one face detection row produced by OpenCV <c>cv::FaceDetectorYN</c>.
    /// 表示 OpenCV <c>cv::FaceDetectorYN</c> 输出的一行人脸检测结果。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FaceDetection : IEquatable<FaceDetection>
    {
        /// <summary>
        /// Initializes a face detection result.
        /// 初始化人脸检测结果。
        /// </summary>
        public FaceDetection(Rect bounds, Point2f rightEye, Point2f leftEye, Point2f noseTip, Point2f rightMouthCorner, Point2f leftMouthCorner, float score)
        {
            Bounds = bounds;
            RightEye = rightEye;
            LeftEye = leftEye;
            NoseTip = noseTip;
            RightMouthCorner = rightMouthCorner;
            LeftMouthCorner = leftMouthCorner;
            Score = score;
        }

        /// <summary>
        /// Gets the face bounding rectangle.
        /// 获取人脸边界矩形。
        /// </summary>
        public Rect Bounds { get; }

        /// <summary>
        /// Gets the right-eye landmark.
        /// 获取右眼关键点。
        /// </summary>
        public Point2f RightEye { get; }

        /// <summary>
        /// Gets the left-eye landmark.
        /// 获取左眼关键点。
        /// </summary>
        public Point2f LeftEye { get; }

        /// <summary>
        /// Gets the nose-tip landmark.
        /// 获取鼻尖关键点。
        /// </summary>
        public Point2f NoseTip { get; }

        /// <summary>
        /// Gets the right mouth-corner landmark.
        /// 获取右嘴角关键点。
        /// </summary>
        public Point2f RightMouthCorner { get; }

        /// <summary>
        /// Gets the left mouth-corner landmark.
        /// 获取左嘴角关键点。
        /// </summary>
        public Point2f LeftMouthCorner { get; }

        /// <summary>
        /// Gets the confidence score.
        /// 获取置信分数。
        /// </summary>
        public float Score { get; }

        /// <summary>Determines whether two detections are equal. 判断两个人脸检测结果是否相等。</summary>
        public static bool operator ==(FaceDetection left, FaceDetection right)
        {
            return left.Equals(right);
        }

        /// <summary>Determines whether two detections are different. 判断两个人脸检测结果是否不同。</summary>
        public static bool operator !=(FaceDetection left, FaceDetection right)
        {
            return !left.Equals(right);
        }

        /// <summary>Indicates whether this detection equals another detection. 指示此检测结果是否与另一个检测结果相等。</summary>
        public bool Equals(FaceDetection other)
        {
            return Bounds.Equals(other.Bounds) &&
                RightEye.Equals(other.RightEye) &&
                LeftEye.Equals(other.LeftEye) &&
                NoseTip.Equals(other.NoseTip) &&
                RightMouthCorner.Equals(other.RightMouthCorner) &&
                LeftMouthCorner.Equals(other.LeftMouthCorner) &&
                Score.Equals(other.Score);
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is FaceDetection other && Equals(other);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Bounds.GetHashCode();
                hash = (hash * 397) ^ RightEye.GetHashCode();
                hash = (hash * 397) ^ LeftEye.GetHashCode();
                hash = (hash * 397) ^ NoseTip.GetHashCode();
                hash = (hash * 397) ^ RightMouthCorner.GetHashCode();
                hash = (hash * 397) ^ LeftMouthCorner.GetHashCode();
                hash = (hash * 397) ^ Score.GetHashCode();
                return hash;
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "{{Bounds={0},RightEye={{X={1},Y={2}}},LeftEye={{X={3},Y={4}}},NoseTip={{X={5},Y={6}}},RightMouthCorner={{X={7},Y={8}}},LeftMouthCorner={{X={9},Y={10}}},Score={11}}}",
                Bounds,
                RightEye.X,
                RightEye.Y,
                LeftEye.X,
                LeftEye.Y,
                NoseTip.X,
                NoseTip.Y,
                RightMouthCorner.X,
                RightMouthCorner.Y,
                LeftMouthCorner.X,
                LeftMouthCorner.Y,
                Score);
        }
    }
}
