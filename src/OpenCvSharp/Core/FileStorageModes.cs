using System;

namespace OpenCvSharp.Core
{
    /// <summary>Specifies FileStorage operation, memory, format, and Base64 modes.</summary>
    [Flags]
    public enum FileStorageModes
    {
        /// <summary>Opens an existing storage for reading.</summary>
        Read = 0,
        /// <summary>Opens a storage for writing and replaces existing content.</summary>
        Write = 1,
        /// <summary>Opens a storage for appending to existing content.</summary>
        Append = 2,
        /// <summary>Uses an in-memory input document or output buffer.</summary>
        Memory = 4,
        /// <summary>Masks the format bits.</summary>
        FormatMask = 7 << 3,
        /// <summary>Detects the input format or selects it from the file extension.</summary>
        FormatAuto = 0,
        /// <summary>Uses XML persistence format.</summary>
        FormatXml = 1 << 3,
        /// <summary>Uses YAML persistence format.</summary>
        FormatYaml = 2 << 3,
        /// <summary>Uses JSON persistence format.</summary>
        FormatJson = 3 << 3,
        /// <summary>Uses YAML 1.0 persistence format.</summary>
        FormatYaml10 = 4 << 3,
        /// <summary>Writes raw data in Base64 form where supported.</summary>
        Base64 = 64,
        /// <summary>Opens a storage for writing with Base64 data encoding.</summary>
        WriteBase64 = Write | Base64
    }
}
