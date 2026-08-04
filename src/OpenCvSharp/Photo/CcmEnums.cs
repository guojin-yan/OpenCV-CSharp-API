namespace JYPPX.OpenCvSharp.Photo
{
    /// <summary>Shape of the color correction matrix.</summary>
    public enum CcmType
    {
        /// <summary>A 3 by 3 linear color correction matrix.</summary>
        Linear = 0,
        /// <summary>A 4 by 3 affine coefficient matrix.</summary>
        Affine = 1,
    }

    /// <summary>Initial color correction matrix estimation method.</summary>
    public enum InitialMethodType
    {
        /// <summary>White-balance initialization.</summary>
        WhiteBalance = 0,
        /// <summary>Least-squares initialization.</summary>
        LeastSquare = 1,
    }

    /// <summary>Built-in reference color checker.</summary>
    public enum ColorCheckerType
    {
        /// <summary>The 24-patch Macbeth ColorChecker.</summary>
        Macbeth = 0,
        /// <summary>The 24-patch DKK Vinyl ColorChecker.</summary>
        Vinyl = 1,
        /// <summary>The 140-patch DigitalSG ColorChecker.</summary>
        DigitalSg = 2,
    }

    /// <summary>Reference or working color space used by the color correction model.</summary>
    public enum ColorSpace
    {
        /// <summary>Nonlinear sRGB.</summary>
        Srgb = 0,
        /// <summary>Linear sRGB.</summary>
        SrgbLinear = 1,
        /// <summary>Nonlinear Adobe RGB.</summary>
        AdobeRgb = 2,
        /// <summary>Linear Adobe RGB.</summary>
        AdobeRgbLinear = 3,
        /// <summary>Nonlinear Wide Gamut RGB.</summary>
        WideGamutRgb = 4,
        /// <summary>Linear Wide Gamut RGB.</summary>
        WideGamutRgbLinear = 5,
        /// <summary>Nonlinear ProPhoto RGB.</summary>
        ProPhotoRgb = 6,
        /// <summary>Linear ProPhoto RGB.</summary>
        ProPhotoRgbLinear = 7,
        /// <summary>Nonlinear DCI-P3 RGB.</summary>
        DciP3Rgb = 8,
        /// <summary>Linear DCI-P3 RGB.</summary>
        DciP3RgbLinear = 9,
        /// <summary>Nonlinear Apple RGB.</summary>
        AppleRgb = 10,
        /// <summary>Linear Apple RGB.</summary>
        AppleRgbLinear = 11,
        /// <summary>Nonlinear Rec. 709 RGB.</summary>
        Rec709Rgb = 12,
        /// <summary>Linear Rec. 709 RGB.</summary>
        Rec709RgbLinear = 13,
        /// <summary>Nonlinear Rec. 2020 RGB.</summary>
        Rec2020Rgb = 14,
        /// <summary>Linear Rec. 2020 RGB.</summary>
        Rec2020RgbLinear = 15,
        /// <summary>CIE XYZ with D65 illuminant and 2-degree observer.</summary>
        XyzD65TwoDegree = 16,
        /// <summary>CIE XYZ with D50 illuminant and 2-degree observer.</summary>
        XyzD50TwoDegree = 17,
        /// <summary>CIE XYZ with D65 illuminant and 10-degree observer.</summary>
        XyzD65TenDegree = 18,
        /// <summary>CIE XYZ with D50 illuminant and 10-degree observer.</summary>
        XyzD50TenDegree = 19,
        /// <summary>CIE XYZ with A illuminant and 2-degree observer.</summary>
        XyzATwoDegree = 20,
        /// <summary>CIE XYZ with A illuminant and 10-degree observer.</summary>
        XyzATenDegree = 21,
        /// <summary>CIE XYZ with D55 illuminant and 2-degree observer.</summary>
        XyzD55TwoDegree = 22,
        /// <summary>CIE XYZ with D55 illuminant and 10-degree observer.</summary>
        XyzD55TenDegree = 23,
        /// <summary>CIE XYZ with D75 illuminant and 2-degree observer.</summary>
        XyzD75TwoDegree = 24,
        /// <summary>CIE XYZ with D75 illuminant and 10-degree observer.</summary>
        XyzD75TenDegree = 25,
        /// <summary>CIE XYZ with E illuminant and 2-degree observer.</summary>
        XyzETwoDegree = 26,
        /// <summary>CIE XYZ with E illuminant and 10-degree observer.</summary>
        XyzETenDegree = 27,
        /// <summary>CIELAB with D65 illuminant and 2-degree observer.</summary>
        LabD65TwoDegree = 28,
        /// <summary>CIELAB with D50 illuminant and 2-degree observer.</summary>
        LabD50TwoDegree = 29,
        /// <summary>CIELAB with D65 illuminant and 10-degree observer.</summary>
        LabD65TenDegree = 30,
        /// <summary>CIELAB with D50 illuminant and 10-degree observer.</summary>
        LabD50TenDegree = 31,
        /// <summary>CIELAB with A illuminant and 2-degree observer.</summary>
        LabATwoDegree = 32,
        /// <summary>CIELAB with A illuminant and 10-degree observer.</summary>
        LabATenDegree = 33,
        /// <summary>CIELAB with D55 illuminant and 2-degree observer.</summary>
        LabD55TwoDegree = 34,
        /// <summary>CIELAB with D55 illuminant and 10-degree observer.</summary>
        LabD55TenDegree = 35,
        /// <summary>CIELAB with D75 illuminant and 2-degree observer.</summary>
        LabD75TwoDegree = 36,
        /// <summary>CIELAB with D75 illuminant and 10-degree observer.</summary>
        LabD75TenDegree = 37,
        /// <summary>CIELAB with E illuminant and 2-degree observer.</summary>
        LabETwoDegree = 38,
        /// <summary>CIELAB with E illuminant and 10-degree observer.</summary>
        LabETenDegree = 39,
    }

    /// <summary>Transformation used to linearize measured RGB values.</summary>
    public enum LinearizationType
    {
        /// <summary>No linearization transform.</summary>
        Identity = 0,
        /// <summary>Gamma linearization.</summary>
        Gamma = 1,
        /// <summary>Per-channel polynomial fitting.</summary>
        ColorPolynomialFit = 2,
        /// <summary>Per-channel logarithmic polynomial fitting.</summary>
        ColorLogPolynomialFit = 3,
        /// <summary>Gray-patch polynomial fitting.</summary>
        GrayPolynomialFit = 4,
        /// <summary>Gray-patch logarithmic polynomial fitting.</summary>
        GrayLogPolynomialFit = 5,
    }

    /// <summary>Color distance used by model fitting.</summary>
    public enum DistanceType
    {
        /// <summary>CIE 1976 color difference.</summary>
        Cie76 = 0,
        /// <summary>CIE94 graphic-arts color difference.</summary>
        Cie94GraphicArts = 1,
        /// <summary>CIE94 textiles color difference.</summary>
        Cie94Textiles = 2,
        /// <summary>CIEDE2000 color difference.</summary>
        Cie2000 = 3,
        /// <summary>CMC l:c 1:1 color difference.</summary>
        CmcOneToOne = 4,
        /// <summary>CMC l:c 2:1 color difference.</summary>
        CmcTwoToOne = 5,
        /// <summary>Euclidean nonlinear RGB distance.</summary>
        Rgb = 6,
        /// <summary>Euclidean linear RGB distance.</summary>
        RgbLinear = 7,
    }
}
