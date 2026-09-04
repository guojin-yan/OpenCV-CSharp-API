using System;
using JYPPX.OpenCvSharp.Core;

namespace JYPPX.OpenCvSharp.Tests.Core
{
    public class PixelTypeDescriptorTests
    {
        [Fact]
        public void RegistryMapsScalarsAndVectorsToExactMatTypes()
        {
            PixelTypeDescriptor gray = PixelTypeDescriptor.Get<byte>();
            Assert.Equal(MatType.CV_8UC1, gray.MatType);
            Assert.Equal(1, gray.Channels);
            Assert.Equal(1, gray.ElementSizeBytes);
            Assert.Equal(PixelChannelOrder.Unknown, gray.ChannelOrder);
            Assert.True(gray.MatchesMatType(MatType.CV_8UC1));

            PixelTypeDescriptor color = PixelTypeDescriptor.Get<Vec3b>();
            Assert.Equal(MatType.CV_8UC3, color.MatType);
            Assert.Equal(3, color.Channels);
            Assert.Equal(3, color.ElementSizeBytes);
            Assert.Equal(PixelChannelOrder.Unknown, color.ChannelOrder);
            Assert.Equal(PixelAlphaMode.Unknown, color.AlphaMode);
            Assert.True(color.MatchesMatType(MatType.CV_8UC3));
            Assert.False(color.MatchesMatType(MatType.CV_8UC4));
        }

        [Fact]
        public void RegistryCoversAllPublishedPixelVectorFamilies()
        {
            Assert.True(PixelTypeDescriptor.IsRegistered<Vec2b>());
            Assert.True(PixelTypeDescriptor.IsRegistered<Vec4b>());
            Assert.True(PixelTypeDescriptor.IsRegistered<Vec3s>());
            Assert.True(PixelTypeDescriptor.IsRegistered<Vec4w>());
            Assert.True(PixelTypeDescriptor.IsRegistered<Vec2i>());
            Assert.True(PixelTypeDescriptor.IsRegistered<Vec4i>());
            Assert.True(PixelTypeDescriptor.IsRegistered<Vec3f>());
            Assert.True(PixelTypeDescriptor.IsRegistered<Vec4d>());
            Assert.True(PixelTypeTraits.RegisteredTypeCount >= 28);
        }

        [Fact]
        public void UnknownTypesFailExplicitly()
        {
            PixelTypeDescriptor descriptor;
            Assert.False(PixelTypeTraits.TryGet(typeof(DateTime), out descriptor));
            Assert.Throws<NotSupportedException>(() => PixelTypeDescriptor.Get<DateTime>());
            Assert.Throws<ArgumentNullException>(() => PixelTypeTraits.TryGet((Type)null!, out descriptor));
        }

        [Fact]
        public void DescriptorEqualityIncludesSemanticFields()
        {
            PixelTypeDescriptor left = PixelTypeDescriptor.Get<Vec3b>();
            PixelTypeDescriptor right = PixelTypeTraits.Get<Vec3b>();
            Assert.Equal(left, right);
            Assert.Equal(left.GetHashCode(), right.GetHashCode());
            Assert.NotEqual(PixelTypeDescriptor.Get<Vec3b>(), PixelTypeDescriptor.Get<Vec4b>());
        }
    }
}
