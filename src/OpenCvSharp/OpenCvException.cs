using System;

namespace JYPPX.OpenCvSharp
{
    /// <summary>
    /// Represents an exception thrown by the OpenCV CSharp API managed layer.
    /// 表示 OpenCV CSharp API managed 层抛出的异常。
    /// </summary>
    public class OpenCvException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenCvException"/> class.
        /// 初始化 <see cref="OpenCvException"/> 类的新实例。
        /// </summary>
        public OpenCvException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenCvException"/> class with a message.
        /// 使用错误消息初始化 <see cref="OpenCvException"/> 类的新实例。
        /// </summary>
        /// <param name="message">The exception message. 异常消息。</param>
        public OpenCvException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenCvException"/> class with a message and inner exception.
        /// 使用错误消息和内部异常初始化 <see cref="OpenCvException"/> 类的新实例。
        /// </summary>
        /// <param name="message">The exception message. 异常消息。</param>
        /// <param name="innerException">The inner exception. 内部异常。</param>
        public OpenCvException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

