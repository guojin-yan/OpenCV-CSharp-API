using System;
using System.Linq;
using System.Reflection;
using VideoCv2 = JYPPX.OpenCvSharp.Video.Cv2;

namespace JYPPX.OpenCvSharp.Tests.Video
{
    public sealed class MotionTemplateTests
    {
        [Fact]
        public void MotionTemplateApiIsNotExposedWhenLocalOpenCvPublicHeadersDoNotDeclareIt()
        {
            string[] motionTemplateNames =
            {
                "UpdateMotionHistory",
                "CalcMotionGradient",
                "CalcGlobalOrientation",
                "SegmentMotion"
            };

            string[] publicMethodNames = typeof(VideoCv2)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Select(method => method.Name)
                .ToArray();

            for (int i = 0; i < motionTemplateNames.Length; i++)
            {
                Assert.DoesNotContain(motionTemplateNames[i], publicMethodNames);
            }
        }
    }
}
