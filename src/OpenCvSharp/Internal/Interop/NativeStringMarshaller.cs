using System;
using System.Text;

namespace JYPPX.OpenCvSharp.Internal.Interop
{
    internal static unsafe class NativeStringMarshaller
    {
        internal delegate int StringLengthGetter(IntPtr handle, out int length);

        internal delegate int StringFillMethod(IntPtr handle, byte* buffer, int bufferCapacity, out int written);

        internal delegate int StringLengthGetterByInt(int value, out int length);

        internal delegate int StringFillMethodByInt(int value, byte* buffer, int bufferCapacity, out int written);

        internal static string GetString(IntPtr handle, StringLengthGetter getLength, StringFillMethod fill)
        {
            NativeException.ThrowIfError(getLength(handle, out int length));
            return GetString(length, delegate (byte* buffer, int bufferCapacity, out int written)
            {
                return fill(handle, buffer, bufferCapacity, out written);
            });
        }

        internal static string GetString(int value, StringLengthGetterByInt getLength, StringFillMethodByInt fill)
        {
            NativeException.ThrowIfError(getLength(value, out int length));
            return GetString(length, delegate (byte* buffer, int bufferCapacity, out int written)
            {
                return fill(value, buffer, bufferCapacity, out written);
            });
        }

        private delegate int FillBufferMethod(byte* buffer, int bufferCapacity, out int written);

        private static string GetString(int length, FillBufferMethod fill)
        {
            if (length <= 0)
            {
                return string.Empty;
            }

            var buffer = new byte[length];
            int written;
            fixed (byte* bufferPtr = buffer)
            {
                NativeException.ThrowIfError(fill(bufferPtr, buffer.Length, out written));
            }
            if (written <= 0)
            {
                return string.Empty;
            }

            if (written > buffer.Length)
            {
                written = buffer.Length;
            }

            if (written != buffer.Length)
            {
                Array.Resize(ref buffer, written);
            }

            return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
        }
    }
}
