namespace OpenCvSharp.Stitching
{
    /// <summary>Built-in OpenCV stitching blender strategies.</summary>
    public enum BlenderType
    {
        /// <summary>Copies masked pixels without seam weighting.</summary>
        None = 0,

        /// <summary>Blends overlap using distance-based feather weights.</summary>
        Feather = 1,

        /// <summary>Blends overlap using a multi-band Laplacian pyramid.</summary>
        MultiBand = 2
    }
}
