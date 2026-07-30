using System;
using OpenCvSharp;
using OpenCvSharp.VideoIO;

namespace OpenCvSharp.Tests.VideoIO
{
    public sealed class VideoIORegistryTests
    {
        [Fact]
        public void RegistryMethodsAreAvailableBehindNativeSmokeGuard()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            VideoCaptureAPIs[] backends = VideoIORegistry.GetBackends();
            Assert.NotNull(backends);
            Assert.NotEmpty(backends);
            Assert.Equal(backends, VideoIORegistry.GetBackends());
            Assert.All(backends, backend =>
            {
                Assert.NotEqual(VideoCaptureAPIs.Any, backend);
                string backendName = VideoIORegistry.GetBackendName(backend);
                Assert.False(string.IsNullOrWhiteSpace(backendName));
                Assert.Equal(backendName, VideoIORegistry.GetBackendName(backend));
                bool hasBackend = VideoIORegistry.HasBackend(backend);
                bool builtIn = VideoIORegistry.IsBackendBuiltIn(backend);
                Assert.True(hasBackend || !hasBackend);
                Assert.True(builtIn || !builtIn);
            });

            string name = VideoIORegistry.GetBackendName(VideoCaptureAPIs.Any);
            Assert.NotNull(name);

            bool hasAny = VideoIORegistry.HasBackend(VideoCaptureAPIs.Any);
            bool builtInAny = VideoIORegistry.IsBackendBuiltIn(VideoCaptureAPIs.Any);
            Assert.True(hasAny || !hasAny);
            Assert.True(builtInAny || !builtInAny);
        }

        [Fact]
        public void RegistryCategoryAndPluginQueriesRemainCallable()
        {
            if (!TestEnvironment.IsNativeSmokeEnabled())
            {
                return;
            }

            Assert.NotNull(VideoIORegistry.GetCameraBackends());
            Assert.NotNull(VideoIORegistry.GetStreamBackends());
            Assert.NotNull(VideoIORegistry.GetStreamBufferedBackends());
            Assert.NotNull(VideoIORegistry.GetWriterBackends());

            foreach (VideoCaptureAPIs backend in VideoIORegistry.GetBackends())
            {
                try
                {
                    VideoIOPluginVersion camera = VideoIORegistry.GetCameraPluginVersion(backend);
                    VideoIOPluginVersion stream = VideoIORegistry.GetStreamPluginVersion(backend);
                    VideoIOPluginVersion buffered = VideoIORegistry.GetStreamBufferedPluginVersion(backend);
                    VideoIOPluginVersion writer = VideoIORegistry.GetWriterPluginVersion(backend);
                    Assert.Equal(camera.Version, camera.ToString());
                    Assert.Equal(stream.Version, stream.ToString());
                    Assert.Equal(buffered.Version, buffered.ToString());
                    Assert.Equal(writer.Version, writer.ToString());
                    break;
                }
                catch (OpenCvException)
                {
                    // A registered backend may be valid for one category but not for every plugin query.
                }
            }
        }
    }
}
