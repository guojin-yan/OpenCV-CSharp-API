using System;
using OpenCvSharp.Core;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.Stitching
{
    /// <summary>Distance-weighted stitching blender.</summary>
    public sealed class FeatherBlender : Blender
    {
        /// <summary>Creates a feather blender. OpenCV accepts finite positive, zero, and negative sharpness values.</summary>
        public FeatherBlender(float sharpness = 0.02f)
            : base(CreateNative(sharpness), BlenderType.Feather)
        {
        }

        internal FeatherBlender(IntPtr nativeHandle)
            : base(nativeHandle, BlenderType.Feather)
        {
        }

        /// <summary>Gets or sets the finite distance-weight scale used for subsequent feeds.</summary>
        public float Sharpness
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.StitchingBlenderGetSharpness(NativeHandle, out float value));
                return value;
            }
            set
            {
                ValidateSharpness(value);
                NativeException.ThrowIfError(NativeMethods.StitchingBlenderSetSharpness(NativeHandle, value));
            }
        }

        /// <summary>Creates independently owned normalized CV_32FC1 weight maps and returns their union ROI.</summary>
        public Mat[] CreateWeightMaps(Mat[] masks, Point[] corners, out Rect destinationRoi)
        {
            if (masks == null) throw new ArgumentNullException(nameof(masks));
            if (corners == null) throw new ArgumentNullException(nameof(corners));
            if (masks.Length == 0) throw new ArgumentException("At least one mask is required.", nameof(masks));
            if (masks.Length != corners.Length) throw new ArgumentException("Mask and corner counts must match.", nameof(corners));

            var maskHandles = new IntPtr[masks.Length];
            var cornerX = new int[corners.Length];
            var cornerY = new int[corners.Length];
            for (int i = 0; i < masks.Length; ++i)
            {
                Mat mask = masks[i] ?? throw new ArgumentNullException(nameof(masks), "The mask collection contains null.");
                ValidateMask(mask, nameof(masks));
                long right = (long)corners[i].X + mask.Cols;
                long bottom = (long)corners[i].Y + mask.Rows;
                if (right > int.MaxValue || right < int.MinValue || bottom > int.MaxValue || bottom < int.MinValue)
                {
                    throw new ArgumentOutOfRangeException(nameof(corners), "A mask placement exceeds the Int32 coordinate range.");
                }
                maskHandles[i] = mask.NativeHandle;
                cornerX[i] = corners[i].X;
                cornerY[i] = corners[i].Y;
            }

            Mat[] weightMaps = CreateEmptyMats(masks.Length);
            destinationRoi = default(Rect);
            try
            {
                IntPtr[] outputHandles = GetHandles(weightMaps);
                NativeException.ThrowIfError(NativeMethods.StitchingBlenderCreateWeightMaps(
                    NativeHandle,
                    maskHandles,
                    maskHandles.Length,
                    cornerX,
                    cornerY,
                    corners.Length,
                    outputHandles,
                    outputHandles.Length,
                    out NativeMethods.StitchingRectNative result));
                destinationRoi = new Rect(result.X, result.Y, result.Width, result.Height);
                GC.KeepAlive(masks);
                GC.KeepAlive(weightMaps);
                return weightMaps;
            }
            catch
            {
                DisposeMats(weightMaps);
                throw;
            }
        }

        private static IntPtr CreateNative(float sharpness)
        {
            ValidateSharpness(sharpness);
            NativeException.ThrowIfError(NativeMethods.StitchingBlenderCreateFeather(sharpness, out IntPtr nativeHandle));
            return nativeHandle;
        }
    }

    /// <summary>Laplacian-pyramid stitching blender with CPU fallback when GPU execution is unavailable.</summary>
    public sealed class MultiBandBlender : Blender
    {
        /// <summary>Creates a multi-band blender with CV_32FC1 weights by default.</summary>
        public MultiBandBlender(bool tryGpu = false, int numberOfBands = 5, int weightType = MatType.CV_32FC1)
            : base(CreateNative(tryGpu, numberOfBands, weightType), BlenderType.MultiBand)
        {
        }

        internal MultiBandBlender(IntPtr nativeHandle)
            : base(nativeHandle, BlenderType.MultiBand)
        {
        }

        /// <summary>Gets or sets the requested number of pyramid bands for the next preparation.</summary>
        public int NumberOfBands
        {
            get
            {
                NativeException.ThrowIfError(NativeMethods.StitchingBlenderGetNumberOfBands(NativeHandle, out int value));
                return value;
            }
            set
            {
                ValidateNumberOfBands(value);
                NativeException.ThrowIfError(NativeMethods.StitchingBlenderSetNumberOfBands(NativeHandle, value));
            }
        }

        private static IntPtr CreateNative(bool tryGpu, int numberOfBands, int weightType)
        {
            ValidateNumberOfBands(numberOfBands);
            ValidateWeightType(weightType);
            NativeException.ThrowIfError(NativeMethods.StitchingBlenderCreateMultiBand(
                tryGpu ? 1 : 0, numberOfBands, weightType, out IntPtr nativeHandle));
            return nativeHandle;
        }
    }
}
