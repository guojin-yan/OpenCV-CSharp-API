using System;
using JYPPX.OpenCvSharp.Internal.Interop;

namespace JYPPX.OpenCvSharp.Dnn
{
    /// <summary>Owned ref-counted reference to an OpenCV DNN layer.</summary>
    /// <remarks>The native reference is independent of its originating <see cref="Net"/> and must be disposed.</remarks>
    public sealed class Layer : IDisposable
    {
        private readonly NativeDnnLayerHandle handle;

        internal Layer(IntPtr value)
        {
            handle = NativeDnnLayerHandle.FromNativePointer(value);
        }

        /// <summary>Gets whether this layer reference has been disposed.</summary>
        public bool IsDisposed { get { return handle.IsClosed; } }

        /// <summary>Returns the numeric index for a named layer output.</summary>
        /// <param name="outputName">Output name encoded as UTF-8 for OpenCV.</param>
        /// <returns>The zero-based output index reported by the layer.</returns>
        /// <exception cref="ObjectDisposedException">This layer reference has been disposed.</exception>
        public int OutputNameToIndex(string outputName)
        {
            byte[] nativeName = DnnStringConvert.ToNullTerminatedUtf8(outputName, nameof(outputName));
            int result = 0;
            WithNativeHandle(value => NativeException.ThrowIfError(NativeMethods.DnnLayerOutputNameToIndex(value, nativeName, out result)));
            return result;
        }

        /// <summary>Releases this independently ref-counted layer reference.</summary>
        /// <remarks>Repeated calls are safe.</remarks>
        public void Dispose()
        {
            handle.Dispose();
            GC.SuppressFinalize(this);
        }

        private void WithNativeHandle(Action<IntPtr> action)
        {
            bool addedReference = false;
            try
            {
                if (handle.IsClosed || handle.IsInvalid) throw new ObjectDisposedException(GetType().FullName);
                handle.DangerousAddRef(ref addedReference);
                action(handle.DangerousGetHandle());
            }
            finally
            {
                if (addedReference) handle.DangerousRelease();
            }
        }
    }
}
