using System;

namespace OpenCvSharp.Core
{
    /// <summary>Describes the scalar or collection shape of a FileNode.</summary>
    [Flags]
    public enum FileNodeTypes
    {
        /// <summary>Indicates an absent node.</summary>
        None = 0,
        /// <summary>Indicates an integer scalar.</summary>
        Integer = 1,
        /// <summary>Indicates a floating-point scalar.</summary>
        Real = 2,
        /// <summary>Aliases <see cref="Real"/>.</summary>
        Float = Real,
        /// <summary>Indicates a string scalar.</summary>
        String = 3,
        /// <summary>Indicates an ordered sequence.</summary>
        Sequence = 4,
        /// <summary>Indicates a named-value map.</summary>
        Map = 5,
        /// <summary>Masks the node type bits.</summary>
        TypeMask = 7,
        /// <summary>Requests flow-style formatting for a collection.</summary>
        Flow = 8,
        /// <summary>Aliases <see cref="Flow"/>.</summary>
        Uniform = Flow,
        /// <summary>Indicates an empty collection.</summary>
        Empty = 16,
        /// <summary>Indicates a named node.</summary>
        Named = 32
    }
}
