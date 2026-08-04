using System;
using System.Reflection;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.Features2D;

namespace JYPPX.OpenCvSharp.Tests.Features2D
{
    public class DescriptorMatcherTests
    {
        [Fact]
        public void CreateByNameRejectsNullBeforeNativeCall()
        {
            Assert.Throws<ArgumentNullException>(() => DescriptorMatcher.Create(null!));
        }

        [Fact]
        public void CreateByTypeRejectsInvalidTypeBeforeNativeCall()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => DescriptorMatcher.Create((DescriptorMatcherType)99));
        }

        [Fact]
        public void ScalarValidatorsRejectInvalidKAndMaxDistanceBeforeNativeCall()
        {
            InvokeDescriptorMatcherCoreValidator("ValidateK", 1, "k");
            InvokeDescriptorMatcherCoreValidator("ValidateMaxDistance", 0.0F, "maxDistance");
            InvokeDescriptorMatcherCoreValidator("ValidateMaxDistance", 1.0F, "maxDistance");

            AssertDescriptorMatcherCoreThrows<ArgumentOutOfRangeException>("ValidateK", 0, "k");
            AssertDescriptorMatcherCoreThrows<ArgumentOutOfRangeException>("ValidateK", -1, "k");
            AssertDescriptorMatcherCoreThrows<ArgumentOutOfRangeException>("ValidateMaxDistance", -0.1F, "maxDistance");
            AssertDescriptorMatcherCoreThrows<ArgumentOutOfRangeException>("ValidateMaxDistance", float.NaN, "maxDistance");
            AssertDescriptorMatcherCoreThrows<ArgumentOutOfRangeException>("ValidateMaxDistance", float.PositiveInfinity, "maxDistance");
        }

        [Fact]
        public void CollectionHelpersRejectNullInputsBeforeNativeCall()
        {
            InvokeDescriptorMatcherCoreArrayValidator(Array.Empty<Mat>(), "descriptors");
            IntPtr[] maskHandles = Assert.IsType<IntPtr[]>(InvokeDescriptorMatcherCoreExArrayMethod(
                "NormalizeMaskHandles",
                Array.Empty<Mat>()));
            Assert.Empty(maskHandles);

            AssertDescriptorMatcherCoreArrayThrows<ArgumentNullException>(null, "descriptors");
            AssertDescriptorMatcherCoreArrayThrows<ArgumentNullException>(new Mat[] { null! }, "descriptors");
            AssertDescriptorMatcherCoreExArrayThrows<ArgumentNullException>("NormalizeMaskHandles", null);
            AssertDescriptorMatcherCoreExArrayThrows<ArgumentNullException>("NormalizeMaskHandles", new Mat[] { null! });
        }

        [Fact]
        public void BFMatcherCanBeUsedThroughDescriptorMatcherBase()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            DescriptorMatcher? matcher = TryCreateBFMatcher();
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            using (Mat query = Feature2DTestData.CreateFloatDescriptors(0.0F, 0.0F, 10.0F, 10.0F))
            using (Mat train = Feature2DTestData.CreateFloatDescriptors(0.0F, 0.0F, 9.0F, 9.0F, 50.0F, 50.0F))
            {
                matcher.Add(new[] { train });
                matcher.Train();

                Assert.False(matcher.IsDisposed);
                Assert.True(matcher.IsMaskSupported);
                Assert.False(matcher.Empty);
                Assert.Equal(2, matcher.Match(query, train).Length);
                Assert.Equal(2, matcher.Match(query).Length);
                Assert.Equal(2, matcher.KnnMatch(query, train, 2).Length);
                Assert.Equal(2, matcher.KnnMatch(query, 1).Length);
                Assert.Equal(2, matcher.RadiusMatch(query, train, 3.0F).Length);
                Assert.Equal(2, matcher.RadiusMatch(query, 3.0F).Length);
            }
        }

        [Fact]
        public void FlannMatcherCanBeUsedThroughDescriptorMatcherBase()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            DescriptorMatcher? matcher = TryCreateFlannMatcher();
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            using (Mat query = Feature2DTestData.CreateFloatDescriptors(0.0F, 0.0F, 10.0F, 10.0F))
            using (Mat train = Feature2DTestData.CreateFloatDescriptors(0.0F, 0.0F, 9.0F, 9.0F, 50.0F, 50.0F))
            {
                matcher.Add(new[] { train });
                matcher.Train();

                Assert.False(matcher.IsDisposed);
                Assert.False(matcher.IsMaskSupported);
                Assert.False(matcher.Empty);
                Assert.Equal(2, matcher.Match(query, train).Length);
                Assert.Equal(2, matcher.Match(query).Length);
                Assert.Equal(2, matcher.KnnMatch(query, train, 2).Length);
                Assert.Equal(2, matcher.KnnMatch(query, 1).Length);
                Assert.Equal(2, matcher.RadiusMatch(query, train, 3.0F).Length);
                Assert.Equal(2, matcher.RadiusMatch(query, 3.0F).Length);
            }
        }

        [Fact]
        public void FlannMatcherRejectsMasksThroughBaseApi()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            DescriptorMatcher? matcher = TryCreateFlannMatcher();
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            using (Mat query = Feature2DTestData.CreateFloatDescriptors(0.0F, 0.0F))
            using (Mat train = Feature2DTestData.CreateFloatDescriptors(0.0F, 0.0F))
            using (Mat mask = new Mat(1, 1, MatType.CV_8UC1, new Scalar(255)))
            {
                Assert.Throws<NotSupportedException>(() => matcher.Match(query, train, mask));
                Assert.Throws<NotSupportedException>(() => matcher.KnnMatch(query, train, 1, mask));
                Assert.Throws<NotSupportedException>(() => matcher.RadiusMatch(query, train, 1.0F, mask));
            }
        }

        [Fact]
        public void AddValidatesDescriptorArraysThroughBaseApi()
        {
            if (!Feature2DTestData.IsNativeSmokeEnabled())
            {
                return;
            }

            DescriptorMatcher? matcher = TryCreateBFMatcher();
            if (matcher == null)
            {
                return;
            }

            using (matcher)
            {
                Assert.Throws<ArgumentNullException>(() => matcher.Add(null!));
                Assert.Throws<ArgumentNullException>(() => matcher.Add(new Mat[] { null! }));

#if NETCOREAPP3_1_OR_GREATER
                Mat[] descriptors = new Mat[] { null! };
                Assert.Throws<ArgumentNullException>(() => matcher.Add(descriptors.AsSpan()));
#endif
            }
        }

        private static DescriptorMatcher? TryCreateBFMatcher()
        {
            try
            {
                return BFMatcher.Create(NormTypes.L2, crossCheck: false);
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static DescriptorMatcher? TryCreateFlannMatcher()
        {
            try
            {
                return FlannBasedMatcher.Create();
            }
            catch (OpenCvException ex) when (Feature2DTestData.IsFeaturesModuleMissing(ex))
            {
                return null;
            }
        }

        private static void InvokeDescriptorMatcherCoreValidator(string methodName, params object?[] arguments)
        {
            GetDescriptorMatcherCoreMethod(methodName).Invoke(null, arguments);
        }

        private static TException AssertDescriptorMatcherCoreThrows<TException>(string methodName, params object?[] arguments)
            where TException : Exception
        {
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                () => InvokeDescriptorMatcherCoreValidator(methodName, arguments));
            return Assert.IsType<TException>(exception.InnerException);
        }

        private static void InvokeDescriptorMatcherCoreArrayValidator(Mat[]? descriptors, string parameterName)
        {
            MethodInfo method = GetDescriptorMatcherCoreMethod("ValidateNonNullArray").MakeGenericMethod(typeof(Mat));
            method.Invoke(null, new object?[] { descriptors, parameterName });
        }

        private static TException AssertDescriptorMatcherCoreArrayThrows<TException>(Mat[]? descriptors, string parameterName)
            where TException : Exception
        {
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                () => InvokeDescriptorMatcherCoreArrayValidator(descriptors, parameterName));
            return Assert.IsType<TException>(exception.InnerException);
        }

        private static object? InvokeDescriptorMatcherCoreExArrayMethod(string methodName, Mat[]? masks)
        {
            return GetDescriptorMatcherCoreExMethod(methodName, typeof(Mat[])).Invoke(null, new object?[] { masks });
        }

        private static TException AssertDescriptorMatcherCoreExArrayThrows<TException>(string methodName, Mat[]? masks)
            where TException : Exception
        {
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                () => InvokeDescriptorMatcherCoreExArrayMethod(methodName, masks));
            return Assert.IsType<TException>(exception.InnerException);
        }

        private static MethodInfo GetDescriptorMatcherCoreMethod(string methodName)
        {
            Type? coreType = typeof(DescriptorMatcher).Assembly.GetType("JYPPX.OpenCvSharp.Features2D.DescriptorMatcherCore");
            Assert.NotNull(coreType);
            MethodInfo? method = coreType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(method);
            return method;
        }

        private static MethodInfo GetDescriptorMatcherCoreExMethod(string methodName, params Type[] parameterTypes)
        {
            Type? coreType = typeof(DescriptorMatcher).Assembly.GetType("JYPPX.OpenCvSharp.Features2D.DescriptorMatcherCoreEx");
            Assert.NotNull(coreType);
            MethodInfo? method = coreType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic, null, parameterTypes, null);
            Assert.NotNull(method);
            return method;
        }
    }
}
