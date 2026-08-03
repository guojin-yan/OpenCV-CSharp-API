using System;
using System.Text;
using OpenCvSharp.Internal.Interop;

namespace OpenCvSharp.ImgProc
{
    /// <summary>Owns an OpenCV TrueType/OpenType font face for Unicode text, including Chinese when the selected font contains the required glyphs. 拥有用于 Unicode 文本的 OpenCV TrueType/OpenType 字体对象；所选字体包含相应字形时可绘制中文。</summary>
    public sealed class FontFace : IDisposable
    {
        private NativeFontFaceHandle handle;
        private bool disposed;

        /// <summary>Creates a font face with OpenCV's default state. 创建使用 OpenCV 默认状态的字体对象。</summary>
        public FontFace()
        {
            NativeException.ThrowIfError(NativeMethods.ImgProcFontFaceCreateDefault(out IntPtr nativeHandle));
            handle = NativeFontFaceHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Loads an embedded font name or a TTF/TTC/OpenType font file path. Use a CJK font file to render Chinese with <c>Cv2.PutText</c>. 加载内嵌字体名称或 TTF/TTC/OpenType 字体文件路径；使用包含中文字形的字体文件即可通过 <c>Cv2.PutText</c> 绘制中文。</summary>
        public FontFace(string fontPathOrName)
        {
            byte[] nativeName = ToNullTerminatedUtf8(fontPathOrName, nameof(fontPathOrName));
            NativeException.ThrowIfError(NativeMethods.ImgProcFontFaceCreate(nativeName, out IntPtr nativeHandle));
            handle = NativeFontFaceHandle.FromNativePointer(nativeHandle);
        }

        /// <summary>Gets whether this object has been disposed. 获取此对象是否已释放。</summary>
        public bool IsDisposed
        {
            get { return disposed; }
        }

        /// <summary>Gets the active font name. 获取当前字体名称。</summary>
        public string Name
        {
            get { return GetName(); }
        }

        internal IntPtr NativeHandle
        {
            get
            {
                ThrowIfDisposed();
                return handle.DangerousGetHandle();
            }
        }

        /// <summary>Loads a new embedded font name or font file. 加载新的内嵌字体名称或字体文件。</summary>
        public bool Set(string fontPathOrName)
        {
            ThrowIfDisposed();
            byte[] nativeName = ToNullTerminatedUtf8(fontPathOrName, nameof(fontPathOrName));
            NativeException.ThrowIfError(NativeMethods.ImgProcFontFaceSet(NativeHandle, nativeName, out int result));
            return result != 0;
        }

        /// <summary>Gets the active font name. 获取当前字体名称。</summary>
        public string GetName()
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgProcFontFaceGetNameSize(NativeHandle, out int byteCount));
            if (byteCount < 0)
            {
                throw new OpenCvException("Native FontFace name length is invalid.");
            }

            if (byteCount == 0)
            {
                return string.Empty;
            }

            var buffer = new byte[byteCount];
            NativeException.ThrowIfError(NativeMethods.ImgProcFontFaceGetNameFill(NativeHandle, buffer, buffer.Length, out int written));
            if (written < 0 || written > buffer.Length)
            {
                throw new OpenCvException("Native FontFace name output is inconsistent.");
            }

            return Encoding.UTF8.GetString(buffer, 0, written);
        }

        /// <summary>Sets variable-font axis tag/value pairs. 设置可变字体轴的标签/数值对。</summary>
        public bool SetInstance(int[] parameters)
        {
            ThrowIfDisposed();
            ValidateInstanceParameters(parameters, nameof(parameters));
            NativeException.ThrowIfError(NativeMethods.ImgProcFontFaceSetInstance(
                NativeHandle,
                parameters,
                parameters.Length,
                out int result));
            return result != 0;
        }

#if NETCOREAPP3_1_OR_GREATER
        /// <summary>Sets variable-font axis tag/value pairs from a span. 从 Span 设置可变字体轴标签/数值对。</summary>
        public bool SetInstance(ReadOnlySpan<int> parameters)
        {
            if (parameters.Length % 2 != 0)
            {
                throw new ArgumentException("Font instance parameters must be tag/value pairs.", nameof(parameters));
            }

            return SetInstance(parameters.ToArray());
        }
#endif

        /// <summary>Gets variable-font axis tag/value pairs. 获取可变字体轴的标签/数值对。</summary>
        public bool GetInstance(out int[] parameters)
        {
            ThrowIfDisposed();
            NativeException.ThrowIfError(NativeMethods.ImgProcFontFaceGetInstanceCount(
                NativeHandle,
                out int parameterCount,
                out int countResult));
            if (parameterCount < 0 || parameterCount % 2 != 0)
            {
                throw new OpenCvException("Native FontFace instance count is invalid.");
            }

            if (parameterCount == 0)
            {
                parameters = Array.Empty<int>();
                return countResult != 0;
            }

            var values = new int[parameterCount];
            NativeException.ThrowIfError(NativeMethods.ImgProcFontFaceGetInstanceFill(
                NativeHandle,
                values,
                values.Length,
                out int written,
                out int fillResult));
            if (written < 0 || written > values.Length || written % 2 != 0)
            {
                throw new OpenCvException("Native FontFace instance output is inconsistent.");
            }

            if (written != values.Length)
            {
                Array.Resize(ref values, written);
            }

            parameters = values;
            return countResult != 0 && fillResult != 0;
        }

        /// <summary>Gets variable-font axis tag/value pairs or throws if the face has no active font. 获取轴参数；字体未激活时抛出异常。</summary>
        public int[] GetInstance()
        {
            if (!GetInstance(out int[] parameters))
            {
                throw new InvalidOperationException("The font face does not expose an active font instance.");
            }

            return parameters;
        }

        /// <summary>Releases the native font face. 释放 native 字体对象。</summary>
        public void Dispose()
        {
            if (!disposed)
            {
                handle.Dispose();
                disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        internal static byte[] ToNullTerminatedUtf8(string value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            int byteCount = Encoding.UTF8.GetByteCount(value);
            var buffer = new byte[byteCount + 1];
            Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            return buffer;
        }

        private static void ValidateInstanceParameters(int[] parameters, string parameterName)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            if (parameters.Length % 2 != 0)
            {
                throw new ArgumentException("Font instance parameters must be tag/value pairs.", parameterName);
            }
        }

        private void ThrowIfDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}
