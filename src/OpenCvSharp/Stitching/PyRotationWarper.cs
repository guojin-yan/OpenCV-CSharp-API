using System;
using System.Text;
using JYPPX.OpenCvSharp.Core;
using JYPPX.OpenCvSharp.ImgProc;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Stitching
{
    /// <summary>Owns OpenCV's public string-selected rotation warper adapter.</summary>
    public sealed class PyRotationWarper : IDisposable
    {
        private static readonly Encoding StrictUtf8 = new UTF8Encoding(false, true);
        private readonly bool configured;
        private NativePyRotationWarperHandle handle;
        private bool disposed;

        /// <summary>
        /// Creates the upstream default state. Point, map, ROI, and image operations are unavailable
        /// because OpenCV does not attach a projector in this constructor.
        /// </summary>
        public PyRotationWarper()
        {
            NativeException.ThrowIfError(NativeMethods.StitchingPyRotationWarperCreateDefault(out IntPtr nativeHandle));
            handle = NativePyRotationWarperHandle.FromNativePointer(nativeHandle);
            configured = false;
        }

        /// <summary>Creates a configured warper for an exact OpenCV projector name and positive scale.</summary>
        public PyRotationWarper(string type, float scale)
        {
            byte[] nativeType = EncodeType(type);
            ValidateScale(scale);
            NativeException.ThrowIfError(NativeMethods.StitchingPyRotationWarperCreate(nativeType, nativeType.Length, scale, out IntPtr nativeHandle));
            handle = NativePyRotationWarperHandle.FromNativePointer(nativeHandle);
            configured = true;
        }

        /// <summary>Gets whether this warper has been disposed.</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>
        /// Gets or sets the public adapter scale callable. OpenCV 5.0.0 always reports 1 and its setter
        /// is a no-op; the construction scale still controls the underlying projector.
        /// </summary>
        public float Scale
        {
            get
            {
                ThrowIfDisposed();
                NativeException.ThrowIfError(NativeMethods.StitchingPyRotationWarperGetScale(NativeHandle, out float value));
                return value;
            }
            set
            {
                ThrowIfDisposed();
                ValidateScale(value);
                NativeException.ThrowIfError(NativeMethods.StitchingPyRotationWarperSetScale(NativeHandle, value));
            }
        }

        /// <summary>Projects one source-image point.</summary>
        public Point2f WarpPoint(Point2f point, Mat cameraMatrix, Mat rotationMatrix)
        {
            ValidateOperation(cameraMatrix, rotationMatrix);
            NativeException.ThrowIfError(NativeMethods.StitchingPyRotationWarperWarpPoint(
                NativeHandle, point.X, point.Y, cameraMatrix.NativeHandle, rotationMatrix.NativeHandle, out NativeMethods.StitchingPoint2fNative value));
            GC.KeepAlive(cameraMatrix);
            GC.KeepAlive(rotationMatrix);
            return new Point2f(value.X, value.Y);
        }

        /// <summary>Projects one destination point back into source-image coordinates.</summary>
        public Point2f WarpPointBackward(Point2f point, Mat cameraMatrix, Mat rotationMatrix)
        {
            ValidateOperation(cameraMatrix, rotationMatrix);
            NativeException.ThrowIfError(NativeMethods.StitchingPyRotationWarperWarpPointBackward(
                NativeHandle, point.X, point.Y, cameraMatrix.NativeHandle, rotationMatrix.NativeHandle, out NativeMethods.StitchingPoint2fNative value));
            GC.KeepAlive(cameraMatrix);
            GC.KeepAlive(rotationMatrix);
            return new Point2f(value.X, value.Y);
        }

        /// <summary>Builds caller-owned single-channel CV_32F coordinate maps and returns their upstream ROI.</summary>
        public Rect BuildMaps(Size sourceSize, Mat cameraMatrix, Mat rotationMatrix, Mat xMap, Mat yMap)
        {
            ValidateSize(sourceSize, nameof(sourceSize));
            ValidateOperation(cameraMatrix, rotationMatrix);
            ValidateOutput(xMap, nameof(xMap));
            ValidateOutput(yMap, nameof(yMap));
            if (ReferenceEquals(xMap, yMap) || xMap.NativeHandle == yMap.NativeHandle)
            {
                throw new ArgumentException("The x and y maps must use distinct Mat handles.", nameof(yMap));
            }
            NativeException.ThrowIfError(NativeMethods.StitchingPyRotationWarperBuildMaps(
                NativeHandle, sourceSize.Width, sourceSize.Height, cameraMatrix.NativeHandle, rotationMatrix.NativeHandle,
                xMap.NativeHandle, yMap.NativeHandle, out NativeMethods.StitchingRectNative value));
            GC.KeepAlive(cameraMatrix); GC.KeepAlive(rotationMatrix); GC.KeepAlive(xMap); GC.KeepAlive(yMap);
            return new Rect(value.X, value.Y, value.Width, value.Height);
        }

        /// <summary>Warps an image into caller-owned destination storage and returns its top-left coordinate.</summary>
        public Point Warp(
            Mat source,
            Mat cameraMatrix,
            Mat rotationMatrix,
            InterpolationFlags interpolationMode,
            BorderTypes borderMode,
            Mat destination)
        {
            ValidateSource(source, nameof(source));
            ValidateOperation(cameraMatrix, rotationMatrix);
            ValidateOutput(destination, nameof(destination));
            if (ReferenceEquals(source, destination) || source.NativeHandle == destination.NativeHandle)
            {
                throw new ArgumentException("PyRotationWarper does not support in-place image warping.", nameof(destination));
            }
            NativeException.ThrowIfError(NativeMethods.StitchingPyRotationWarperWarp(
                NativeHandle, source.NativeHandle, cameraMatrix.NativeHandle, rotationMatrix.NativeHandle,
                (int)interpolationMode, (int)borderMode, destination.NativeHandle, out NativeMethods.StitchingPointNative value));
            GC.KeepAlive(source); GC.KeepAlive(cameraMatrix); GC.KeepAlive(rotationMatrix); GC.KeepAlive(destination);
            return new Point(value.X, value.Y);
        }

        /// <summary>Backward-warps a projected image into the requested caller-owned destination size.</summary>
        public void WarpBackward(
            Mat source,
            Mat cameraMatrix,
            Mat rotationMatrix,
            InterpolationFlags interpolationMode,
            BorderTypes borderMode,
            Size destinationSize,
            Mat destination)
        {
            ValidateSource(source, nameof(source));
            ValidateOperation(cameraMatrix, rotationMatrix);
            ValidateSize(destinationSize, nameof(destinationSize));
            ValidateOutput(destination, nameof(destination));
            if (ReferenceEquals(source, destination) || source.NativeHandle == destination.NativeHandle)
            {
                throw new ArgumentException("PyRotationWarper does not support in-place backward warping.", nameof(destination));
            }
            NativeException.ThrowIfError(NativeMethods.StitchingPyRotationWarperWarpBackward(
                NativeHandle, source.NativeHandle, cameraMatrix.NativeHandle, rotationMatrix.NativeHandle,
                (int)interpolationMode, (int)borderMode, destinationSize.Width, destinationSize.Height, destination.NativeHandle));
            GC.KeepAlive(source); GC.KeepAlive(cameraMatrix); GC.KeepAlive(rotationMatrix); GC.KeepAlive(destination);
        }

        /// <summary>Returns the inclusive projected image bounds represented as an OpenCV rectangle.</summary>
        public Rect WarpRoi(Size sourceSize, Mat cameraMatrix, Mat rotationMatrix)
        {
            ValidateSize(sourceSize, nameof(sourceSize));
            ValidateOperation(cameraMatrix, rotationMatrix);
            NativeException.ThrowIfError(NativeMethods.StitchingPyRotationWarperWarpRoi(
                NativeHandle, sourceSize.Width, sourceSize.Height, cameraMatrix.NativeHandle, rotationMatrix.NativeHandle,
                out NativeMethods.StitchingRectNative value));
            GC.KeepAlive(cameraMatrix); GC.KeepAlive(rotationMatrix);
            return new Rect(value.X, value.Y, value.Width, value.Height);
        }

        /// <summary>Releases the owned native adapter.</summary>
        public void Dispose()
        {
            if (disposed) return;
            handle.Dispose();
            disposed = true;
        }

        private IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        private void ValidateOperation(Mat cameraMatrix, Mat rotationMatrix)
        {
            ThrowIfDisposed();
            if (!configured)
            {
                throw new InvalidOperationException("The default PyRotationWarper has no configured projector.");
            }
            ValidateTransform(cameraMatrix, nameof(cameraMatrix));
            ValidateTransform(rotationMatrix, nameof(rotationMatrix));
        }

        private static void ValidateTransform(Mat matrix, string parameterName)
        {
            if (matrix == null) throw new ArgumentNullException(parameterName);
            if (matrix.Empty) throw new ArgumentException("The matrix must not be empty.", parameterName);
            if (matrix.Rows != 3 || matrix.Cols != 3 || matrix.Type != MatType.CV_32FC1)
            {
                throw new ArgumentException("The matrix must be 3x3 CV_32FC1.", parameterName);
            }
        }

        private static void ValidateSource(Mat source, string parameterName)
        {
            if (source == null) throw new ArgumentNullException(parameterName);
            if (source.Empty) throw new ArgumentException("The source image must not be empty.", parameterName);
        }

        private static void ValidateOutput(Mat output, string parameterName)
        {
            if (output == null) throw new ArgumentNullException(parameterName);
            _ = output.NativeHandle;
        }

        private static void ValidateSize(Size size, string parameterName)
        {
            if (size.Width <= 0 || size.Height <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName, "Width and height must be positive.");
            }
        }

        private static void ValidateScale(float scale)
        {
            if (float.IsNaN(scale) || float.IsInfinity(scale) || scale <= 0.0f)
            {
                throw new ArgumentOutOfRangeException(nameof(scale), "Scale must be finite and positive.");
            }
        }

        private static byte[] EncodeType(string type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (type.Length == 0) throw new ArgumentException("Warper type must not be empty.", nameof(type));
            if (type.IndexOf('\0') >= 0) throw new ArgumentException("Warper type must not contain an embedded null.", nameof(type));
            try { return StrictUtf8.GetBytes(type); }
            catch (EncoderFallbackException exception) { throw new ArgumentException("Warper type is not valid UTF-8 text.", nameof(type), exception); }
        }

        private void ThrowIfDisposed()
        {
            if (disposed) throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
