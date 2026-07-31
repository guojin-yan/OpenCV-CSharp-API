using System;
using OpenCvSharp.Core;
using OpenCvSharp.Features2D;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Stitching
{
    /// <summary>Owns the keypoints, descriptors, index, and image size used by OpenCV's stitching pipeline.</summary>
    public sealed unsafe class ImageFeatures : IDisposable
    {
        private NativeImageFeaturesHandle handle;
        private bool disposed;

        /// <summary>Creates an independently owned stitching feature record.</summary>
        public ImageFeatures(int imageIndex, Size imageSize, KeyPoint[] keypoints, Mat descriptors)
        {
            if (keypoints == null) throw new ArgumentNullException(nameof(keypoints));
            ValidateSize(imageSize, nameof(imageSize));
            ValidateDescriptors(descriptors, keypoints.Length, nameof(descriptors));

            NativeKeyPoint[] nativeKeypoints = KeyPointMarshaller.ToNative(keypoints);
            fixed (NativeKeyPoint* keypointPointer = nativeKeypoints)
            {
                NativeException.ThrowIfError(NativeMethods.StitchingImageFeaturesCreate(
                    imageIndex,
                    imageSize.Width,
                    imageSize.Height,
                    keypointPointer,
                    nativeKeypoints.Length,
                    descriptors.NativeHandle,
                    out IntPtr nativeHandle));
                handle = NativeImageFeaturesHandle.FromNativePointer(nativeHandle);
            }
            GC.KeepAlive(descriptors);
        }

        private ImageFeatures(IntPtr nativeHandle)
        {
            handle = NativeImageFeaturesHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this record has been disposed.</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets or sets the source image index.</summary>
        public int ImageIndex
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StitchingImageFeaturesGetImageIndex(NativeHandle, out int value));
                return value;
            }
            set
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StitchingImageFeaturesSetImageIndex(NativeHandle, value));
            }
        }

        /// <summary>Gets or sets the source image size.</summary>
        public Size ImageSize
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StitchingImageFeaturesGetImageSize(NativeHandle, out int width, out int height));
                return new Size(width, height);
            }
            set
            {
                ThrowIfDisposed();
                ValidateSize(value, nameof(value));
                NativeException.ThrowIfError(NativeMethods.StitchingImageFeaturesSetImageSize(NativeHandle, value.Width, value.Height));
            }
        }

        /// <summary>Gets a copy of the stored keypoints.</summary>
        public KeyPoint[] Keypoints
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StitchingImageFeaturesGetKeypointsCount(NativeHandle, out int count));
                if (count <= 0) return Array.Empty<KeyPoint>();
                var native = new NativeKeyPoint[count];
                fixed (NativeKeyPoint* pointer = native)
                {
                    NativeException.ThrowIfError(NativeMethods.StitchingImageFeaturesGetKeypointsFill(
                        NativeHandle, pointer, native.Length, out int written));
                    return KeyPointMarshaller.FromNative(native, CheckedWrittenCount(written, native.Length, "keypoints"));
                }
            }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Computes one independently owned stitching feature record.</summary>
        public static ImageFeatures Compute(Feature2D finder, Mat image, Mat? mask = null)
        {
            ValidateFinder(finder);
            ValidateImage(image, nameof(image));
            ValidateMask(mask, image, nameof(mask));
            Feature2DFinderBridge.Get(finder, out int finderKind, out IntPtr finderHandle);

            ImageFeatures result = CreateEmpty();
            try
            {
                NativeException.ThrowIfError(NativeMethods.StitchingComputeImageFeatures(
                    finderKind,
                    finderHandle,
                    image.NativeHandle,
                    mask == null ? IntPtr.Zero : mask.NativeHandle,
                    result.NativeHandle));
                result.ImageIndex = -1;
                GC.KeepAlive(finder);
                GC.KeepAlive(image);
                GC.KeepAlive(mask);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>Computes one independently owned record for each image.</summary>
        public static ImageFeatures[] Compute(Feature2D finder, Mat[] images, Mat[]? masks = null)
        {
            ValidateFinder(finder);
            IntPtr[] imageHandles = GetImageHandles(images);
            IntPtr[] maskHandles = GetMaskHandles(masks, images);
            Feature2DFinderBridge.Get(finder, out int finderKind, out IntPtr finderHandle);

            var result = new ImageFeatures[images.Length];
            int created = 0;
            try
            {
                for (; created < result.Length; ++created) result[created] = CreateEmpty();
                IntPtr[] featureHandles = GetFeatureHandles(result);
                NativeException.ThrowIfError(NativeMethods.StitchingComputeImageFeaturesBatch(
                    finderKind,
                    finderHandle,
                    imageHandles,
                    imageHandles.Length,
                    maskHandles,
                    maskHandles.Length,
                    featureHandles,
                    featureHandles.Length));
                for (int i = 0; i < result.Length; ++i) result[i].ImageIndex = i;
                GC.KeepAlive(finder);
                GC.KeepAlive(images);
                GC.KeepAlive(masks);
                return result;
            }
            catch
            {
                for (int i = 0; i < created; ++i) result[i]?.Dispose();
                throw;
            }
        }

        /// <summary>Copies the descriptors into a new independently owned Mat.</summary>
        public Mat GetDescriptors()
        {
            var result = new Mat();
            try
            {
                CopyDescriptorsTo(result);
                return result;
            }
            catch
            {
                result.Dispose();
                throw;
            }
        }

        /// <summary>Copies the descriptors into caller-owned Mat storage.</summary>
        public void CopyDescriptorsTo(Mat descriptors)
        {
            ThrowIfDisposed();
            if (descriptors == null) throw new ArgumentNullException(nameof(descriptors));
            NativeException.ThrowIfError(NativeMethods.StitchingImageFeaturesCopyDescriptors(NativeHandle, descriptors.NativeHandle));
            GC.KeepAlive(descriptors);
        }

        /// <summary>Releases the owned native feature record.</summary>
        public void Dispose()
        {
            if (disposed) return;
            handle.Dispose();
            disposed = true;
            GC.SuppressFinalize(this);
        }

        private static ImageFeatures CreateEmpty()
        {
            using (var descriptors = new Mat())
            {
                return new ImageFeatures(-1, new Size(0, 0), Array.Empty<KeyPoint>(), descriptors);
            }
        }

        private static IntPtr[] GetImageHandles(Mat[] images)
        {
            if (images == null) throw new ArgumentNullException(nameof(images));
            if (images.Length == 0) throw new ArgumentException("At least one image is required.", nameof(images));
            var handles = new IntPtr[images.Length];
            for (int i = 0; i < images.Length; ++i)
            {
                ValidateImage(images[i], nameof(images));
                handles[i] = images[i].NativeHandle;
            }
            return handles;
        }

        private static IntPtr[] GetMaskHandles(Mat[]? masks, Mat[] images)
        {
            if (masks == null) return Array.Empty<IntPtr>();
            if (masks.Length != images.Length)
            {
                throw new ArgumentException("Mask count must match image count.", nameof(masks));
            }
            var handles = new IntPtr[masks.Length];
            for (int i = 0; i < masks.Length; ++i)
            {
                ValidateMask(masks[i], images[i], nameof(masks));
                handles[i] = masks[i].NativeHandle;
            }
            return handles;
        }

        private static IntPtr[] GetFeatureHandles(ImageFeatures[] features)
        {
            var handles = new IntPtr[features.Length];
            for (int i = 0; i < features.Length; ++i) handles[i] = features[i].NativeHandle;
            return handles;
        }

        private static void ValidateFinder(Feature2D finder)
        {
            if (finder == null) throw new ArgumentNullException(nameof(finder));
            if (finder.IsDisposed) throw new ObjectDisposedException(finder.GetType().FullName);
        }

        private static void ValidateImage(Mat image, string parameterName)
        {
            if (image == null) throw new ArgumentNullException(parameterName);
            if (image.Empty) throw new ArgumentException("The image must not be empty.", parameterName);
        }

        private static void ValidateMask(Mat? mask, Mat image, string parameterName)
        {
            if (mask == null) return;
            if (!mask.Empty && (mask.Type != MatType.CV_8UC1 || mask.Rows != image.Rows || mask.Cols != image.Cols))
            {
                throw new ArgumentException("A non-empty mask must be same-sized CV_8UC1.", parameterName);
            }
        }

        private static void ValidateSize(Size value, string parameterName)
        {
            if (value.Width < 0 || value.Height < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Image dimensions must be non-negative.");
            }
        }

        private static void ValidateDescriptors(Mat descriptors, int keypointCount, string parameterName)
        {
            if (descriptors == null) throw new ArgumentNullException(parameterName);
            if (descriptors.Empty)
            {
                return;
            }
            if (descriptors.Dims != 2 || descriptors.Channels != 1 ||
                (descriptors.Depth != MatType.CV_8U && descriptors.Depth != MatType.CV_32F) ||
                descriptors.Rows != keypointCount)
            {
                throw new ArgumentException("Descriptors must be single-channel CV_8U or CV_32F with one row per keypoint.", parameterName);
            }
        }

        private static int CheckedWrittenCount(int written, int capacity, string collectionName)
        {
            if (written < 0 || written > capacity)
            {
                throw new OpenCvException("Native " + collectionName + " count exceeded the allocated capacity.");
            }
            return written;
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }

    internal static class Feature2DFinderBridge
    {
        internal static void Get(Feature2D finder, out int kind, out IntPtr handle)
        {
            if (finder is ORB orb) { kind = 0; handle = orb.NativeHandle; return; }
            if (finder is SIFT sift) { kind = 1; handle = sift.NativeHandle; return; }
            if (finder is FastFeatureDetector fast) { kind = 2; handle = fast.NativeHandle; return; }
            if (finder is GFTTDetector gftt) { kind = 3; handle = gftt.NativeHandle; return; }
            if (finder is MSER mser) { kind = 4; handle = mser.NativeHandle; return; }
            if (finder is SimpleBlobDetector simpleBlob) { kind = 5; handle = simpleBlob.NativeHandle; return; }
            if (finder is BRISK brisk) { kind = 6; handle = brisk.NativeHandle; return; }
            if (finder is KAZE kaze) { kind = 7; handle = kaze.NativeHandle; return; }
            if (finder is AKAZE akaze) { kind = 8; handle = akaze.NativeHandle; return; }
            if (finder is AffineFeature affine) { kind = 9; handle = affine.NativeHandle; return; }
            throw new NotSupportedException("Stitching feature computation supports ORB, SIFT, FastFeatureDetector, GFTTDetector, MSER, SimpleBlobDetector, BRISK, KAZE, AKAZE, and AffineFeature.");
        }
    }
}
