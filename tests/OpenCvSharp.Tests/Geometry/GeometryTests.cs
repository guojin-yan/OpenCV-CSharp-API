using OpenCvSharp.Geometry;

namespace OpenCvSharp.Tests.Geometry
{
    public sealed class GeometryTests
    {
        [Fact]
        public void DistanceTypesMatchOpenCvConstants()
        {
            Assert.Equal(-1, (int)DistanceTypes.User);
            Assert.Equal(1, (int)DistanceTypes.L1);
            Assert.Equal(2, (int)DistanceTypes.L2);
            Assert.Equal(3, (int)DistanceTypes.C);
            Assert.Equal(4, (int)DistanceTypes.L12);
            Assert.Equal(5, (int)DistanceTypes.Fair);
            Assert.Equal(6, (int)DistanceTypes.Welsch);
            Assert.Equal(7, (int)DistanceTypes.Huber);
        }

        [Fact]
        public void RectanglesIntersectTypesMatchOpenCvConstants()
        {
            Assert.Equal(0, (int)RectanglesIntersectTypes.IntersectNone);
            Assert.Equal(1, (int)RectanglesIntersectTypes.IntersectPartial);
            Assert.Equal(2, (int)RectanglesIntersectTypes.IntersectFull);
        }
    }
}
