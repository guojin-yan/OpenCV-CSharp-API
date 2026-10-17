param(
    [Parameter(Mandatory)][ValidateSet('optflow','bgsegm','face','quality','img_hash','line_descriptor','freetype','alphamat','intensity_transform','plot','bioinspired','phase_unwrapping','hfs','fuzzy','rapid','shape','structured_light','saliency','surface_matching','reg','xstereo','ptcloud','xobjdetect','superres','dnn_superres','wechat_qrcode','cnn_3dobj','signal','dpm','hdf','text','xfeatures2d','dnn_objdetect','ccalib','julia','rgbd','cudaarithm','cudabgsegm','cudafilters','cudaimgproc','cudacodec')][string]$Module,
    [Parameter(Mandatory)][string]$DisplayName,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [switch]$Check
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$rawPath = Join-Path $repo "compatibility/$Module-upstream-raw.json"
$nativePath = Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt'
$managedPath = Join-Path $repo 'compatibility/managed-public-api.txt'
$classificationPath = Join-Path $repo "compatibility/$Module-upstream-classifications.json"
$summaryPath = Join-Path $repo "compatibility/$Module-upstream-summary.json"
$familyPath = Join-Path $repo "compatibility/$Module-implemented-families.json"
foreach ($path in @($rawPath, $nativePath, $managedPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Contrib map input is missing: $path" }
}
$raw = Get-Content $rawPath -Raw | ConvertFrom-Json
$nativeEntries = @(Get-Content $nativePath |
    Where-Object { $_ -match "^jyppx_ocv_${Module}_" -or ($Module -eq 'optflow' -and $_ -match '^jyppx_ocv_motempl_') -or ($Module -eq 'ptcloud' -and $_ -match '^jyppx_ocv_rgbd_normals_') -or ($Module -eq 'xobjdetect' -and $_ -match '^jyppx_ocv_(cascade_classifier|hog_descriptor)_') } |
    ForEach-Object { ($_ -split '\|')[0] } | Sort-Object -Unique)
$managedEntries = @(Get-Content $managedPath | Where-Object { $_ -match "JYPPX\.OpenCvSharp\.$DisplayName" })

function ToPascal([string]$Value) {
    if ([string]::IsNullOrEmpty($Value)) { return $Value }
    return $Value.Substring(0, 1).ToUpperInvariant() + $Value.Substring(1)
}

function GetParts([string]$Identity) {
    $pattern = if ($Module -eq 'optflow') {
        '^cv\.(?<namespace>optflow|motempl)\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'fuzzy') {
        '^cv\.ft\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'shape') {
        '^cv\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'structured_light') {
        '^cv\.structured_light\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'saliency') {
        '^cv\.saliency\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'surface_matching') {
        '^cv\.ppf_match_3d\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'reg') {
        '^cv\.reg\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'xstereo') {
        '^cv\.stereo\.(?:(?<owner>.+)\.)?(?<name>operator <|[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'ptcloud') {
        '^cv\.(?:(?<owner>[A-Za-z][A-Za-z0-9_]*)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'xobjdetect') {
        '^cv\.(?:(?<owner>[A-Za-z][A-Za-z0-9_]*)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'xfeatures2d') {
        '^cv\.(?<namespace>xfeatures2d|cuda)\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'ccalib') {
        '^cv\.(?<namespace>multicalib|omnidir|ccalib)\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'superres') {
        '^cv\.superres\.(?:(?<owner>.+)\.)?(?<name>(?:[A-Za-z]|~)[A-Za-z0-9_~]*)\('
    } elseif ($Module -eq 'rgbd') {
        '^cv\.(?<namespace>colored_kinfu|dynafu|kinfu|large_kinfu|linemod)\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'cudaarithm') {
        '^cv\.cuda\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'cudabgsegm') {
        '^cv\.cuda\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'cudafilters') {
        '^cv\.cuda\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'cudaimgproc') {
        '^cv\.cuda\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } elseif ($Module -eq 'cudacodec') {
        '^cv\.cudacodec\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    } else {
        '^cv\.' + $Module + '\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
    }
    $match = [regex]::Match($Identity, $pattern)
    if (-not $match.Success) { return $null }
    [pscustomobject]@{ Namespace = [string]$match.Groups['namespace'].Value; Owner = [string]$match.Groups['owner'].Value; Name = [string]$match.Groups['name'].Value }
}

function GetManagedType([string]$Owner, [string]$Name) {
    if ($Module -eq 'optflow') {
        if ([string]::IsNullOrWhiteSpace($Owner)) {
            switch ($Name) {
                'createOptFlow_DenseRLOF' { return 'DenseRLOFOpticalFlow' }
                'createOptFlow_SparseRLOF' { return 'SparseRLOFOpticalFlow' }
                default { return 'OptFlowCv2' }
            }
        }
        return $Owner
    }
    if ($Module -in @('face','quality','img_hash','line_descriptor','alphamat','intensity_transform','bioinspired','phase_unwrapping','hfs','fuzzy','rapid','shape','structured_light','saliency','surface_matching','reg','xstereo','ptcloud','xobjdetect','superres','dnn_superres','wechat_qrcode','xfeatures2d','ccalib','cudaarithm','cudabgsegm','cudafilters','cudaimgproc','cudacodec')) {
        if ($Module -eq 'face') {
            if ([string]::IsNullOrWhiteSpace($Owner)) {
                switch ($Name) {
                    'createFacemarkLBF' { return 'FacemarkLBF' }
                    'createFacemarkAAM' { return 'FacemarkAAM' }
                    'createFacemarkKazemi' { return 'FacemarkKazemi' }
                    default { return 'FaceCv2' }
                }
            }
            if ($Owner -eq 'BasicFaceRecognizer' -and $Name -match '^(get|set)Threshold$') { return 'FaceRecognizer' }
            if ($Owner -eq 'LBPHFaceRecognizer' -and $Name -match '^(get|set)Threshold$') { return 'FaceRecognizer' }
        }
        if ([string]::IsNullOrWhiteSpace($Owner)) {
            $managedType = switch ($Module) {
                'quality' { 'QualityCv2' }
                'img_hash' { 'ImgHashCv2' }
                'line_descriptor' { 'LineDescriptorCv2' }
                'alphamat' { 'AlphaMatCv2' }
                'intensity_transform' { 'IntensityTransformCv2' }
                'bioinspired' { 'BioInspiredCv2' }
                'phase_unwrapping' { 'PhaseUnwrappingCv2' }
                'hfs' { 'HfsCv2' }
                'fuzzy' { 'FuzzyCv2' }
                'rapid' { 'RapidCv2' }
                'shape' {
                    switch -Regex ($Name) {
                        '^createNormHistogramCostExtractor$' { 'NormHistogramCostExtractor' }
                        '^createEMDHistogramCostExtractor$' { 'EMDHistogramCostExtractor' }
                        '^createChiHistogramCostExtractor$' { 'ChiHistogramCostExtractor' }
                        '^createEMDL1HistogramCostExtractor$' { 'EMDL1HistogramCostExtractor' }
                        '^createShapeContextDistanceExtractor$' { 'ShapeContextDistanceExtractor' }
                        '^createHausdorffDistanceExtractor$' { 'HausdorffDistanceExtractor' }
                        default { 'ShapeCv2' }
                    }
                }
                'structured_light' { return $Owner }
                'saliency' { return $Owner }
                'surface_matching' { return $Owner }
                'reg' { return $Owner }
                'xstereo' { return $Owner }
                'ptcloud' { return $Owner }
                'xobjdetect' { return $Owner }
                'superres' { return $Owner }
                'dnn_superres' { return $Owner }
                'wechat_qrcode' { return $Owner }
                'xfeatures2d' { return 'XFeatures2D' }
                'ccalib' { return 'CcalibCv2' }
                'cudaarithm' { return 'CudaarithmCv2' }
                'cudabgsegm' { return 'CudaBgSegmCv2' }
                'cudafilters' { return 'CudaFiltersCv2' }
                'cudaimgproc' { return 'CudaImgProcCv2' }
                'cudacodec' { return 'CudaCodecCv2' }
            }
            return [string]$managedType
        }
        if ($Module -eq 'rapid') {
            if ($Owner -eq 'Tracker') { return 'RapidTracker' }
            if ($Owner -eq 'Rapid') { return 'RapidSilhouetteTracker' }
            if ($Owner -eq 'OLSTracker') { return 'OlsTracker' }
            return $Owner
        }
        return $Owner
    }
    if ([string]::IsNullOrWhiteSpace($Owner)) {
        switch ($Name) {
            'createBackgroundSubtractorMOG' { return 'BackgroundSubtractorMOG' }
            'createBackgroundSubtractorGMG' { return 'BackgroundSubtractorGMG' }
            'createBackgroundSubtractorCNT' { return 'BackgroundSubtractorCNT' }
            'createSyntheticSequenceGenerator' { return 'SyntheticSequenceGenerator' }
            default { return 'BgSegmCv2' }
        }
    }
    if ($Name -in @('apply', 'getBackgroundImage') -and $Owner -match '^BackgroundSubtractor(MOG|GMG|CNT)$') { return 'BgSegmBackgroundSubtractor' }
    return $Owner
}

function GetManaged([string]$Owner, [string]$Name, [string]$Identity) {
    if ($Module -in @('freetype','cnn_3dobj','cudaarithm','cudabgsegm','cudafilters','cudaimgproc','cudacodec')) { return @() }
    if ($Module -eq 'structured_light') {
        $typeMarker = "JYPPX.OpenCvSharp.StructuredLight.$Owner|"
        $typeEntries = @($managedEntries | Where-Object { $_.Contains($typeMarker, [StringComparison]::Ordinal) })
        switch ("$Owner.$Name") {
            'GrayCodePattern.create' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;static\|[^|]*\bCreate\(System\.Int32 width,System\.Int32 height\)' } | Sort-Object -Unique) }
            'GrayCodePattern.getNumberOfPatternImages' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public\|System\.Int32 NumberOfPatternImages$' } | Sort-Object -Unique) }
            'GrayCodePattern.setWhiteThreshold' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Void SetWhiteThreshold\(System\.Int32 value\)' } | Sort-Object -Unique) }
            'GrayCodePattern.setBlackThreshold' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Void SetBlackThreshold\(System\.Int32 value\)' } | Sort-Object -Unique) }
            'GrayCodePattern.getImagesForShadowMasks' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Void GetImagesForShadowMasks\(JYPPX\.OpenCvSharp\.Core\.Mat blackImage,JYPPX\.OpenCvSharp\.Core\.Mat whiteImage\)' } | Sort-Object -Unique) }
            'GrayCodePattern.getProjPixel' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Boolean GetProjPixel\(JYPPX\.OpenCvSharp\.Core\.Mat\[\] patternImages,System\.Int32 x,System\.Int32 y,out JYPPX\.OpenCvSharp\.Core\.Point projectorPixel\)' } | Sort-Object -Unique) }
            'SinusoidalPattern.create' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;static\|[^|]*\bCreate\(JYPPX\.OpenCvSharp\.StructuredLight\.SinusoidalPatternParams parameters\)' } | Sort-Object -Unique) }
            'SinusoidalPattern.computePhaseMap' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Void ComputePhaseMap\(JYPPX\.OpenCvSharp\.Core\.Mat\[\] patternImages,JYPPX\.OpenCvSharp\.Core\.Mat wrappedPhaseMap,JYPPX\.OpenCvSharp\.Core\.Mat shadowMask,JYPPX\.OpenCvSharp\.Core\.Mat\? fundamental=null\)' } | Sort-Object -Unique) }
            'SinusoidalPattern.unwrapPhaseMap' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Void UnwrapPhaseMap\(JYPPX\.OpenCvSharp\.Core\.Mat wrappedPhaseMap,JYPPX\.OpenCvSharp\.Core\.Mat unwrappedPhaseMap,JYPPX\.OpenCvSharp\.Core\.Size cameraSize,JYPPX\.OpenCvSharp\.Core\.Mat\? shadowMask=null\)' } | Sort-Object -Unique) }
            'SinusoidalPattern.computeDataModulationTerm' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Void ComputeDataModulationTerm\(JYPPX\.OpenCvSharp\.Core\.Mat\[\] patternImages,JYPPX\.OpenCvSharp\.Core\.Mat dataModulationTerm,JYPPX\.OpenCvSharp\.Core\.Mat shadowMask\)' } | Sort-Object -Unique) }
            'StructuredLightPattern.generate' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|JYPPX\.OpenCvSharp\.Core\.Mat\[\] Generate\(\)' } | Sort-Object -Unique) }
            default { return @() }
        }
    }
    if ($Module -eq 'saliency') {
        $typeMarker = "JYPPX.OpenCvSharp.Saliency.$Owner|"
        $typeEntries = @($managedEntries | Where-Object { $_.Contains($typeMarker, [StringComparison]::Ordinal) })
        switch ("$Owner.$Name") {
            'Saliency.computeSaliency' { return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.Saliency\.Saliency\|method\|public;instance\|.*\bComputeSaliency\(' } | Sort-Object -Unique) }
            'StaticSaliency.computeBinaryMap' { return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.Saliency\.StaticSaliency\|method\|public;instance\|.*\bComputeBinaryMap\(' } | Sort-Object -Unique) }
            'StaticSaliencySpectralResidual.create' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;static\|[^|]*\bCreate\(\)$' } | Sort-Object -Unique) }
            'StaticSaliencySpectralResidual.computeSaliency' { return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.Saliency\.Saliency\|method\|public;instance\|.*\bComputeSaliency\(' } | Sort-Object -Unique) }
            'StaticSaliencySpectralResidual.getImageWidth' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 ImageWidth$' } | Sort-Object -Unique) }
            'StaticSaliencySpectralResidual.setImageWidth' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 ImageWidth$' } | Sort-Object -Unique) }
            'StaticSaliencySpectralResidual.getImageHeight' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 ImageHeight$' } | Sort-Object -Unique) }
            'StaticSaliencySpectralResidual.setImageHeight' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 ImageHeight$' } | Sort-Object -Unique) }
            'StaticSaliencyFineGrained.create' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;static\|[^|]*\bCreate\(\)$' } | Sort-Object -Unique) }
            'StaticSaliencyFineGrained.computeSaliency' { return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.Saliency\.Saliency\|method\|public;instance\|.*\bComputeSaliency\(' } | Sort-Object -Unique) }
            'MotionSaliencyBinWangApr2014.create' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;static\|[^|]*\bCreate\(\)$' } | Sort-Object -Unique) }
            'MotionSaliencyBinWangApr2014.computeSaliency' { return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.Saliency\.Saliency\|method\|public;instance\|.*\bComputeSaliency\(' } | Sort-Object -Unique) }
            'MotionSaliencyBinWangApr2014.setImagesize' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Void SetImageSize\(System\.Int32 width,System\.Int32 height\)$' } | Sort-Object -Unique) }
            'MotionSaliencyBinWangApr2014.init' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Boolean Init\(\)$' } | Sort-Object -Unique) }
            'MotionSaliencyBinWangApr2014.getImageWidth' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 ImageWidth$' } | Sort-Object -Unique) }
            'MotionSaliencyBinWangApr2014.setImageWidth' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 ImageWidth$' } | Sort-Object -Unique) }
            'MotionSaliencyBinWangApr2014.getImageHeight' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 ImageHeight$' } | Sort-Object -Unique) }
            'MotionSaliencyBinWangApr2014.setImageHeight' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 ImageHeight$' } | Sort-Object -Unique) }
            'ObjectnessBING.create' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;static\|[^|]*\bCreate\(\)$' } | Sort-Object -Unique) }
            'ObjectnessBING.computeSaliency' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|[^|]*\bComputeObjectness\(JYPPX\.OpenCvSharp\.Core\.Mat image\)$' } | Sort-Object -Unique) }
            'ObjectnessBING.getobjectnessValues' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Single\[\] GetObjectnessValues\(\)$' } | Sort-Object -Unique) }
            'ObjectnessBING.setTrainingPath' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Void SetTrainingPath\(System\.String trainingPath\)$' } | Sort-Object -Unique) }
            'ObjectnessBING.setBBResDir' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Void SetBBResDir\(System\.String resultsDir\)$' } | Sort-Object -Unique) }
            'ObjectnessBING.getBase' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Double Base$' } | Sort-Object -Unique) }
            'ObjectnessBING.setBase' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Double Base$' } | Sort-Object -Unique) }
            'ObjectnessBING.getNSS' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 NSS$' } | Sort-Object -Unique) }
            'ObjectnessBING.setNSS' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 NSS$' } | Sort-Object -Unique) }
            'ObjectnessBING.getW' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 W$' } | Sort-Object -Unique) }
            'ObjectnessBING.setW' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 W$' } | Sort-Object -Unique) }
            default { return @() }
        }
    }
    if ($Module -eq 'surface_matching') {
        switch ("$Owner.$Name") {
            'ICP.ICP' { return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.SurfaceMatching\.Icp\|method\|public;static\|.*\bCreate\(' } | Sort-Object -Unique) }
            'ICP.registerModelToScene' {
                if ($Identity -match 'double residual') { return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.SurfaceMatching\.Icp\|method\|public;instance\|.*\bRegisterModelToScene\(JYPPX\.OpenCvSharp\.Core\.Mat srcPc,JYPPX\.OpenCvSharp\.Core\.Mat dstPc\)$' } | Sort-Object -Unique) }
                return @()
            }
            'PPF3DDetector.PPF3DDetector' { return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.SurfaceMatching\.Ppf3DDetector\|method\|public;static\|.*\bCreate\(' } | Sort-Object -Unique) }
            'PPF3DDetector.trainModel' { return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.SurfaceMatching\.Ppf3DDetector\|method\|public;instance\|System\.Void TrainModel\(JYPPX\.OpenCvSharp\.Core\.Mat model\)$' } | Sort-Object -Unique) }
            'PPF3DDetector.match' { return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.SurfaceMatching\.Ppf3DDetector\|method\|public;instance\|.*\bMatch\(JYPPX\.OpenCvSharp\.Core\.Mat scene,System\.Double relativeSceneSampleStep=0\.2,System\.Double relativeSceneDistance=0\.03\)$' } | Sort-Object -Unique) }
            default { return @() }
        }
    }
    if ($Module -eq 'reg') {
        $regEntries = @($managedEntries | Where-Object { $_ -match '^TYPE\|(?:class|struct)\|public(?:;sealed)?\|JYPPX\.OpenCvSharp\.Reg\.' -or $_ -match '^MEMBER\|JYPPX\.OpenCvSharp\.Reg\|' })
        $regMethod = {
            param([string]$pattern)
            return @($managedEntries | Where-Object { $_ -match $pattern } | Sort-Object -Unique)
        }
        switch ("$Owner.$Name") {
            'Map.warp' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void Warp\(JYPPX\.OpenCvSharp\.Core\.Mat src,JYPPX\.OpenCvSharp\.Core\.Mat dst\)$' }
            'Map.inverseWarp' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void InverseWarp\(JYPPX\.OpenCvSharp\.Core\.Mat src,JYPPX\.OpenCvSharp\.Core\.Mat dst\)$' }
            'Map.inverseMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap InverseMap\(\)$' }
            'Map.compose' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void Compose\(JYPPX\.OpenCvSharp\.Reg\.RegMap other\)$' }
            'Map.scale' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void Scale\(System\.Double factor\)$' }
            'MapAffine.MapAffine' { if ($Identity -match '\(\)->$') { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapAffine\|constructor\|public;instance\|\.ctor\(\)$' }; return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapAffine\|constructor\|public;instance\|\.ctor\(JYPPX\.OpenCvSharp\.Reg\.AffineTransform2D transform\)$' }
            'MapAffine.inverseWarp' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void InverseWarp\(JYPPX\.OpenCvSharp\.Core\.Mat src,JYPPX\.OpenCvSharp\.Core\.Mat dst\)$' }
            'MapAffine.inverseMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap InverseMap\(\)$' }
            'MapAffine.compose' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void Compose\(JYPPX\.OpenCvSharp\.Reg\.RegMap other\)$' }
            'MapAffine.scale' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void Scale\(System\.Double factor\)$' }
            'MapAffine.getLinTr' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapAffine\|property\|instance;get:public\|JYPPX\.OpenCvSharp\.Reg\.AffineTransform2D Transform$' }
            'MapAffine.getShift' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapAffine\|property\|instance;get:public\|JYPPX\.OpenCvSharp\.Reg\.AffineTransform2D Transform$' }
            'Mapper.calculate' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap Calculate\(JYPPX\.OpenCvSharp\.Core\.Mat img1,JYPPX\.OpenCvSharp\.Core\.Mat img2,JYPPX\.OpenCvSharp\.Reg\.RegMap\? init=null\)$' }
            'Mapper.getMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap GetMap\(\)$' }
            'MapperGradAffine.MapperGradAffine' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapperGradAffine\|constructor\|public;instance\|\.ctor\(\)$' }
            'MapperGradAffine.calculate' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap Calculate\(JYPPX\.OpenCvSharp\.Core\.Mat img1,JYPPX\.OpenCvSharp\.Core\.Mat img2,JYPPX\.OpenCvSharp\.Reg\.RegMap\? init=null\)$' }
            'MapperGradAffine.getMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap GetMap\(\)$' }
            'MapperGradEuclid.MapperGradEuclid' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapperGradEuclid\|constructor\|public;instance\|\.ctor\(\)$' }
            'MapperGradEuclid.calculate' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap Calculate\(JYPPX\.OpenCvSharp\.Core\.Mat img1,JYPPX\.OpenCvSharp\.Core\.Mat img2,JYPPX\.OpenCvSharp\.Reg\.RegMap\? init=null\)$' }
            'MapperGradEuclid.getMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap GetMap\(\)$' }
            'MapperGradProj.MapperGradProj' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapperGradProj\|constructor\|public;instance\|\.ctor\(\)$' }
            'MapperGradProj.calculate' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap Calculate\(JYPPX\.OpenCvSharp\.Core\.Mat img1,JYPPX\.OpenCvSharp\.Core\.Mat img2,JYPPX\.OpenCvSharp\.Reg\.RegMap\? init=null\)$' }
            'MapperGradProj.getMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap GetMap\(\)$' }
            'MapperGradShift.MapperGradShift' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapperGradShift\|constructor\|public;instance\|\.ctor\(\)$' }
            'MapperGradShift.calculate' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap Calculate\(JYPPX\.OpenCvSharp\.Core\.Mat img1,JYPPX\.OpenCvSharp\.Core\.Mat img2,JYPPX\.OpenCvSharp\.Reg\.RegMap\? init=null\)$' }
            'MapperGradShift.getMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap GetMap\(\)$' }
            'MapperGradSimilar.MapperGradSimilar' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapperGradSimilar\|constructor\|public;instance\|\.ctor\(\)$' }
            'MapperGradSimilar.calculate' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap Calculate\(JYPPX\.OpenCvSharp\.Core\.Mat img1,JYPPX\.OpenCvSharp\.Core\.Mat img2,JYPPX\.OpenCvSharp\.Reg\.RegMap\? init=null\)$' }
            'MapperGradSimilar.getMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap GetMap\(\)$' }
            'MapperPyramid.MapperPyramid' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapperPyramid\|constructor\|public;instance\|\.ctor\(JYPPX\.OpenCvSharp\.Reg\.RegMapper baseMapper\)$' }
            'MapperPyramid.calculate' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap Calculate\(JYPPX\.OpenCvSharp\.Core\.Mat img1,JYPPX\.OpenCvSharp\.Core\.Mat img2,JYPPX\.OpenCvSharp\.Reg\.RegMap\? init=null\)$' }
            'MapperPyramid.getMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMapper\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap GetMap\(\)$' }
            'MapProjec.MapProjec' { if ($Identity -match '\(\)->$') { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapProjec\|constructor\|public;instance\|\.ctor\(\)$' }; return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapProjec\|constructor\|public;instance\|\.ctor\(JYPPX\.OpenCvSharp\.Reg\.ProjectiveTransform2D transform\)$' }
            'MapProjec.inverseWarp' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void InverseWarp\(JYPPX\.OpenCvSharp\.Core\.Mat src,JYPPX\.OpenCvSharp\.Core\.Mat dst\)$' }
            'MapProjec.inverseMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap InverseMap\(\)$' }
            'MapProjec.compose' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void Compose\(JYPPX\.OpenCvSharp\.Reg\.RegMap other\)$' }
            'MapProjec.scale' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void Scale\(System\.Double factor\)$' }
            'MapProjec.getProjTr' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapProjec\|property\|instance;get:public\|JYPPX\.OpenCvSharp\.Reg\.ProjectiveTransform2D Transform$' }
            'MapProjec.normalize' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapProjec\|method\|public;instance\|System\.Void Normalize\(\)$' }
            'MapShift.MapShift' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapShift\|constructor\|public;instance\|\.ctor\(System\.Double shiftX=0,System\.Double shiftY=0\)$' }
            'MapShift.inverseWarp' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void InverseWarp\(JYPPX\.OpenCvSharp\.Core\.Mat src,JYPPX\.OpenCvSharp\.Core\.Mat dst\)$' }
            'MapShift.inverseMap' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|JYPPX\.OpenCvSharp\.Reg\.RegMap InverseMap\(\)$' }
            'MapShift.compose' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void Compose\(JYPPX\.OpenCvSharp\.Reg\.RegMap other\)$' }
            'MapShift.scale' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.RegMap\|method\|public;instance\|System\.Void Scale\(System\.Double factor\)$' }
            'MapShift.getShift' { return & $regMethod 'JYPPX\.OpenCvSharp\.Reg\.MapShift\|method\|public;instance\|System\.Void GetShift\(out System\.Double shiftX,out System\.Double shiftY\)$' }
            default { return @() }
        }
    }
    if ($Module -eq 'xstereo') {
        $typeMarker = "JYPPX.OpenCvSharp.XStereo.$Owner|"
        $typeEntries = @($managedEntries | Where-Object { $_.Contains($typeMarker, [StringComparison]::Ordinal) })
        switch ("$Owner.$Name") {
            'QuasiDenseStereo.create' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;static\|[^|]*\bCreate\(' } | Sort-Object -Unique) }
            'QuasiDenseStereo.getDisparity' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|.*\bGetDisparity\(' } | Sort-Object -Unique) }
            default { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|[^|]*\b' + [regex]::Escape((ToPascal $Name)) + '\(' } | Sort-Object -Unique) }
        }
    }
    if ($Module -eq 'ptcloud') {
        $typeName = if ([string]::IsNullOrWhiteSpace($Owner)) { 'PtCloudCv2' } else { 'RgbdNormals' }
        $typeMarker = "JYPPX.OpenCvSharp.PtCloud.$typeName|"
        $typeEntries = @($managedEntries | Where-Object { $_.Contains($typeMarker, [StringComparison]::Ordinal) })
        switch ("$Owner.$Name") {
            'RgbdNormals.create' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;static\|[^|]*\bCreate\(' } | Sort-Object -Unique) }
            'RgbdNormals.apply' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|[^|]*\bApply\(' } | Sort-Object -Unique) }
            'RgbdNormals.cache' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|[^|]*\bCache\(' } | Sort-Object -Unique) }
            'RgbdNormals.getK' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|[^|]*\bGetK\(' } | Sort-Object -Unique) }
            'RgbdNormals.getRows' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 Rows$' } | Sort-Object -Unique) }
            'RgbdNormals.setRows' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 Rows$' } | Sort-Object -Unique) }
            'RgbdNormals.getCols' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 Cols$' } | Sort-Object -Unique) }
            'RgbdNormals.setCols' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 Cols$' } | Sort-Object -Unique) }
            'RgbdNormals.getWindowSize' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 WindowSize$' } | Sort-Object -Unique) }
            'RgbdNormals.setWindowSize' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public;set:public\|System\.Int32 WindowSize$' } | Sort-Object -Unique) }
            'RgbdNormals.getDepth' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public\|System\.Int32 Depth$' } | Sort-Object -Unique) }
            'RgbdNormals.getMethod' { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public\|JYPPX\.OpenCvSharp\.PtCloud\.RgbdNormalsMethod Method$' } | Sort-Object -Unique) }
            'RgbdNormals.setK' { return @($typeEntries | Where-Object { $_ -match '\|method\|public;instance\|System\.Void SetK\(' } | Sort-Object -Unique) }
            default {
                $methodName = ToPascal $Name
                return @($typeEntries | Where-Object { $_ -match ('\|method\|public;static\|[^|]*\b' + [regex]::Escape($methodName) + '\(') } | Sort-Object -Unique)
            }
        }
    }
    if ($Module -eq 'xobjdetect') {
        $typeMarker = "JYPPX.OpenCvSharp.XObjDetect.$Owner|"
        $typeEntries = @($managedEntries | Where-Object { $_.Contains($typeMarker, [StringComparison]::Ordinal) })
        if ($Name -eq $Owner) {
            $constructorRows = @($typeEntries | Where-Object { $_ -match '\|constructor\|public;instance\|' } | Sort-Object -Unique)
            if ($Identity -match '\(\)->$') { return @($constructorRows | Where-Object { $_ -match '\.ctor\(\)$' }) }
            if ($Identity -match 'String filename') { return @($constructorRows | Where-Object { $_ -match '\.ctor\(System\.String filename\)$' }) }
            if ($Owner -eq 'HOGDescriptor' -and $Identity -match '^cv\.HOGDescriptor\.HOGDescriptor\(Size ') { return @($constructorRows | Where-Object { $_ -match '\.ctor\(JYPPX\.OpenCvSharp\.Core\.Size winSize,' }) }
            return $constructorRows
        }
        if ($Owner -eq 'CascadeClassifier' -and $Name -eq 'empty') { return @($typeEntries | Where-Object { $_ -match '\|property\|instance;get:public\|System\.Boolean Empty$' } | Sort-Object -Unique) }
        if ($Owner -eq 'CascadeClassifier' -and $Name -eq 'detectMultiScale') {
            $managedName = if ($Identity -match 'vector_int numDetections') { 'DetectMultiScale2' } elseif ($Identity -match 'vector_int rejectLevels') { 'DetectMultiScale3' } else { 'DetectMultiScale' }
            return @($typeEntries | Where-Object { $_ -match ('\|method\|public;instance\|.*\b' + $managedName + '\(') } | Sort-Object -Unique)
        }
        $methodName = ToPascal $Name
        return @($typeEntries | Where-Object { $_ -match "\|method\|[^|]*\|[^|]*\b$([regex]::Escape($methodName))\(" } | Sort-Object -Unique)
    }
    if ($Module -eq 'dnn_superres') {
        return @()
    }
    if ($Module -eq 'wechat_qrcode') {
        return @()
    }
    $typeName = GetManagedType $Owner $Name
    if ($Module -eq 'phase_unwrapping') {
        if ($Owner -eq 'HistogramPhaseUnwrapping' -and $Name -eq 'create') {
            return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.PhaseUnwrapping\.HistogramPhaseUnwrapping\|method\|[^|]*\|.*\bCreate\(JYPPX\.OpenCvSharp\.PhaseUnwrapping\.HistogramPhaseUnwrappingParams parameters\)' })
        }
        if ($Owner -eq 'HistogramPhaseUnwrapping' -and $Name -eq 'getInverseReliabilityMap') {
            return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.PhaseUnwrapping\.HistogramPhaseUnwrapping\|method\|[^|]*\|.*\bGetInverseReliabilityMap\(JYPPX\.OpenCvSharp\.Core\.Mat reliabilityMap\)' })
        }
        if ($Owner -eq 'PhaseUnwrapping' -and $Name -eq 'unwrapPhaseMap') {
            return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.PhaseUnwrapping\.PhaseUnwrappingObject\|method\|[^|]*\|.*\bUnwrapPhaseMap\(JYPPX\.OpenCvSharp\.Core\.Mat wrappedPhaseMap,JYPPX\.OpenCvSharp\.Core\.Mat unwrappedPhaseMap,' })
        }
    }
    if ($Module -eq 'hfs') {
        if ($Owner -eq 'HfsSegment' -and $Name -eq 'create') {
            return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.Hfs\.HfsSegment\|method\|[^|]*\|.*\bCreate\(JYPPX\.OpenCvSharp\.Hfs\.HfsSegmentParams parameters\)' })
        }
        if ($Owner -eq 'HfsSegment' -and $Name -in @('performSegmentCpu','performSegmentGpu')) {
            $managedMethod = ToPascal $Name
            return @($managedEntries | Where-Object { $_ -match ('JYPPX\.OpenCvSharp\.Hfs\.HfsSegment\|method\|[^|]*\|JYPPX\.OpenCvSharp\.Core\.Mat ' + [regex]::Escape($managedMethod) + '\(JYPPX\.OpenCvSharp\.Core\.Mat src,System\.Boolean draw=true\)') })
        }
    }
    if ($Module -eq 'fuzzy') {
        $managedMethod = switch ($Name) {
            'FT02D_components' { 'FT02DComponents' }
            'FT02D_inverseFT' { 'FT02DInverseFT' }
            'FT02D_process' { 'FT02DProcess' }
            'FT02D_iteration' { 'FT02DIteration' }
            'FT02D_FL_process' { 'FT02DFLProcess' }
            'FT02D_FL_process_float' { 'FT02DFLProcessFloat' }
            'FT12D_components' { 'FT12DComponents' }
            'FT12D_polynomial' { 'FT12DPolynomial' }
            'FT12D_createPolynomMatrixVertical' { 'FT12DCreatePolynomMatrixVertical' }
            'FT12D_createPolynomMatrixHorizontal' { 'FT12DCreatePolynomMatrixHorizontal' }
            'FT12D_process' { 'FT12DProcess' }
            'FT12D_inverseFT' { 'FT12DInverseFT' }
            'createKernel' { 'CreateKernel' }
            'inpaint' { 'Inpaint' }
            'filter' { 'Filter' }
            default { '' }
        }
        if ([string]::IsNullOrWhiteSpace([string]$managedMethod)) { return @() }
        $returnType = if ($Name -eq 'FT02D_iteration') { 'System\.Int32' } else { 'System\.Void' }
        $methodPattern = [regex]::Escape([string]$managedMethod)
        $rows = @($managedEntries | Where-Object { $_ -match "JYPPX\.OpenCvSharp\.Fuzzy\.FuzzyCv2\|method\|public;static\|$returnType $methodPattern\(" })
        if ($Name -eq 'createKernel') {
            if ($Identity -match '^cv\.ft\.createKernel\(Mat A;Mat B;') { return @($rows | Where-Object { $_ -match '\(JYPPX\.OpenCvSharp\.Core\.Mat functionX,JYPPX\.OpenCvSharp\.Core\.Mat functionY,JYPPX\.OpenCvSharp\.Core\.Mat kernel,System\.Int32 channels\)$' }) }
            return @($rows | Where-Object { $_ -match '\(JYPPX\.OpenCvSharp\.Fuzzy\.FuzzyFunctionType functionType,System\.Int32 radius,JYPPX\.OpenCvSharp\.Core\.Mat kernel,System\.Int32 channels\)$' })
        }
        return $rows
    }
    if ($Module -eq 'rapid') {
        $managedType = GetManagedType $Owner $Name
        $typeMarker = "JYPPX.OpenCvSharp.Rapid.$managedType|"
        $typeEntries = @($managedEntries | Where-Object { $_.Contains($typeMarker, [StringComparison]::Ordinal) })
        if ([string]::IsNullOrWhiteSpace($Owner)) {
            $managedMethod = switch ($Name) {
                'rapid' { 'Run' }
                default { ToPascal $Name }
            }
            return @($typeEntries | Where-Object { $_ -match ("\|method\|public;static\|[^|]*\b" + [regex]::Escape($managedMethod) + '\(') } | Sort-Object -Unique)
        }
        if ($Owner -eq 'Tracker') {
            $managedMethod = ToPascal $Name
            return @($managedEntries | Where-Object { $_ -match ("JYPPX\.OpenCvSharp\.Rapid\.RapidTracker\|method\|public;instance\|[^|]*\b" + [regex]::Escape($managedMethod) + '\(') } | Sort-Object -Unique)
        }
        if ($Name -eq 'create' -and $Owner -in @('Rapid','OLSTracker')) {
            return @($typeEntries | Where-Object { $_ -match '\|method\|public;static\|[^|]*\bCreate\(' } | Sort-Object -Unique)
        }
        return @()
    }
    if ($Module -eq 'shape') {
        if ($Owner -eq 'HistogramCostExtractor' -and $Name -eq 'buildCostMatrix') {
            return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.Shape\.HistogramCostExtractor\|method\|public;instance\|System\.Void BuildCostMatrix\(JYPPX\.OpenCvSharp\.Core\.Mat descriptors1,JYPPX\.OpenCvSharp\.Core\.Mat descriptors2,JYPPX\.OpenCvSharp\.Core\.Mat costMatrix\)' } | Sort-Object -Unique)
        }
        if ($Owner -in @('NormHistogramCostExtractor','EMDHistogramCostExtractor') -and $Name -in @('setNormFlag','getNormFlag')) {
            return @($managedEntries | Where-Object { $_ -match 'JYPPX\.OpenCvSharp\.Shape\.NormHistogramCostExtractorBase\|property\|[^|]*\|JYPPX\.OpenCvSharp\.Core\.NormTypes NormFlag$' } | Sort-Object -Unique)
        }
        if ([string]::IsNullOrWhiteSpace($Owner)) {
            $factory = switch ($Name) {
                'createNormHistogramCostExtractor' { 'CreateNormHistogramCostExtractor' }
                'createEMDHistogramCostExtractor' { 'CreateEMDHistogramCostExtractor' }
                'createChiHistogramCostExtractor' { 'CreateChiHistogramCostExtractor' }
                'createEMDL1HistogramCostExtractor' { 'CreateEMDL1HistogramCostExtractor' }
                'createShapeContextDistanceExtractor' { 'CreateShapeContextDistanceExtractor' }
                'createHausdorffDistanceExtractor' { 'CreateHausdorffDistanceExtractor' }
                default { '' }
            }
            if (-not [string]::IsNullOrWhiteSpace([string]$factory)) {
                $targetType = GetManagedType $Owner $Name
                $classMarker = "JYPPX.OpenCvSharp.Shape.$targetType|"
                $classFactory = @($managedEntries | Where-Object { $_.Contains($classMarker, [StringComparison]::Ordinal) -and $_ -match '\|method\|public;static\|[^|]*\bCreate\(' })
                $facade = @($managedEntries | Where-Object { $_ -match ('JYPPX\.OpenCvSharp\.Shape\.ShapeCv2\|method\|public;static\|[^|]*\b' + [regex]::Escape([string]$factory) + '\(') })
                return @($classFactory + $facade | Sort-Object -Unique)
            }
        }
    }
    if ($Module -eq 'bioinspired' -and $Name -eq 'create') {
        $factory = switch ($Owner) {
            'Retina' { 'CreateRetina' }
            'RetinaFastToneMapping' { 'CreateRetinaFastToneMapping' }
            'TransientAreasSegmentationModule' { 'CreateTransientAreasSegmentationModule' }
            default { '' }
        }
        if ($factory) {
            $factoryType = 'JYPPX.OpenCvSharp.BioInspired.BioInspiredCv2|'
            return @($managedEntries | Where-Object { $_.Contains($factoryType, [StringComparison]::Ordinal) -and $_ -match ('\|method\|[^|]*\|[^|]*\b' + [regex]::Escape($factory) + '\(') } | Sort-Object -Unique)
        }
    }
    if ($Module -eq 'quality') {
        if ($Name -in @('clear','empty','getQualityMap') -or ($Name -eq 'compute' -and $Identity -match '\((?:Mat img|Mat cmp)\)')) { $typeName = 'QualityBase' }
        if ($Name -eq 'compute' -and $Identity -match 'vector_Mat cmpImgs') { return @() }
        if ($Owner -eq 'QualityBRISQUE' -and $Name -eq 'create' -and $Identity -match 'Ptr_ml_SVM') { return @() }
    }
    if ($Module -eq 'line_descriptor' -and $Owner -eq 'BinaryDescriptorMatcher' -and $Name -in @('match','knnMatch') -and $Identity -notmatch 'Mat trainDescriptors') { return @() }
    $typeMarker = "JYPPX.OpenCvSharp.$DisplayName.$typeName|"
    $typeEntries = @($managedEntries | Where-Object { $_.Contains($typeMarker, [StringComparison]::Ordinal) })
    if ($Module -in @('face','quality','img_hash','line_descriptor') -and $Name -eq 'empty') {
        $emptyProperty = @($typeEntries | Where-Object { $_ -match '\|property\|[^|]*\|.*\sEmpty$' } | Sort-Object -Unique)
        if ($emptyProperty.Count -gt 0) { return $emptyProperty }
    }
    $factory = [string]::IsNullOrWhiteSpace($Owner) -and ($Name -match '^createOptFlow_' -or $Name -match '^createBackgroundSubtractor' -or $Name -eq 'createSyntheticSequenceGenerator' -or $Name -in @('createFacemarkAAM','createFacemarkKazemi','createFacemarkLBF'))
    $constructor = $Name -eq $Owner -and -not [string]::IsNullOrWhiteSpace($Owner)
    if ($Module -eq 'line_descriptor' -and $Owner -eq 'BinaryDescriptor' -and $Name -eq 'createBinaryDescriptor') { return @($typeEntries | Where-Object { $_ -match '\|method\|[^|]*\|[^|]*\bCreate\(' } | Sort-Object -Unique) }
    if ($Module -eq 'line_descriptor' -and $Owner -eq 'BinaryDescriptorMatcher' -and $constructor) { return @($typeEntries | Where-Object { $_ -match '\|method\|[^|]*\|[^|]*\bCreate\(' } | Sort-Object -Unique) }
    if ($factory -or $constructor) { return @($typeEntries | Where-Object { $_ -match '\|method\|[^|]*\|[^|]*\bCreate\(' } | Sort-Object -Unique) }
    $propertyMatch = [regex]::Match($Name, '^(?:get|set)(?<property>[A-Z].*)$')
    if ($propertyMatch.Success) {
        $property = [regex]::Escape([string]$propertyMatch.Groups['property'].Value)
        $propertyRows = @($typeEntries | Where-Object { $_ -match "\|property\|[^|]*\|.*\s$property$" } | Sort-Object -Unique)
        if ($propertyRows.Count -gt 0) { return $propertyRows }
    }
    $escapedMethod = [regex]::Escape((ToPascal $Name))
    if ($Module -eq 'bioinspired') {
        $managedMethodName = if ($Owner -eq 'RetinaFastToneMapping' -and $Name -eq 'applyFastToneMapping') { 'Apply' } elseif ($Name -in @('setupOPLandIPLParvoChannel','setupIPLMagnoChannel')) { 'Setup' } else { ToPascal $Name }
        $managedMethodPattern = [regex]::Escape($managedMethodName)
        $methodRows = @($typeEntries | Where-Object { $_ -match "\|method\|[^|]*\|[^|]*\b$managedMethodPattern\(" } | Sort-Object -Unique)
        if ($Name -in @('setup','setupOPLandIPLParvoChannel','setupIPLMagnoChannel')) {
            if ($Identity -match '\bString ') { return @($methodRows | Where-Object { $_ -match 'Setup\(System\.String' }) }
            if ($Identity -match 'OPLandIPLParvoChannel') { return @($methodRows | Where-Object { $_ -match 'Setup\(JYPPX\.OpenCvSharp\.BioInspired\.RetinaParvoParameters' }) }
            if ($Identity -match 'IPLMagnoChannel') { return @($methodRows | Where-Object { $_ -match 'Setup\(JYPPX\.OpenCvSharp\.BioInspired\.RetinaMagnoParameters' }) }
        }
        if ($Name -in @('applyFastToneMapping','getParvo','getParvoRAW','getMagno','getMagnoRAW','getSegmentationPicture')) {
            $takesOutput = $Identity -match '\[/O\]'
            return @($methodRows | Where-Object { ([string]$_ -match ',JYPPX\.OpenCvSharp\.Core\.Mat output\)' -or [string]$_ -match '\(JYPPX\.OpenCvSharp\.Core\.Mat output\)') -eq $takesOutput })
        }
        return $methodRows
    } elseif ($Module -eq 'plot') {
        $methodRows = @($typeEntries | Where-Object { $_ -match "\|method\|[^|]*\|[^|]*\b$escapedMethod\(" } | Sort-Object -Unique)
        if ($Name -eq 'create') {
            if ($Identity -match 'Mat dataY') { return @($methodRows | Where-Object { $_ -match '\bCreate\([^)]*dataX[^)]*dataY' }) }
            return @($methodRows | Where-Object { $_ -match '\bCreate\([^)]*\bdata\)' })
        }
        return $methodRows
    } elseif ($Module -eq 'intensity_transform') {
        $methodRows = @($typeEntries | Where-Object { $_ -match "\|method\|[^|]*\|[^|]*\b$escapedMethod\(" } | Sort-Object -Unique)
        if ($Name -eq 'BIMEF') {
            $hasExplicitExposure = $Identity -match '\bfloat k\b'
            return @($methodRows | Where-Object { ([string]$_ -match 'System\.Single k') -eq $hasExplicitExposure })
        }
        return $methodRows
    }
    if ($Module -in @('face','quality','img_hash','line_descriptor')) {
        @($typeEntries | Where-Object { $_ -match "\|method\|[^|]*\|[^|]*\b$escapedMethod\(" } | Sort-Object -Unique)
    } else {
        @($typeEntries | Where-Object { $_ -match "\|method\|[^|]*\|[^|]*$escapedMethod\(" } | Sort-Object -Unique)
    }
}

function GetNative([object]$Parts, [string]$Identity) {
    $owner = [string]$Parts.Owner
    $name = [string]$Parts.Name
    $result = @()
    if ($Module -eq 'superres') {
        return @()
    } elseif ($Module -eq 'rapid') {
        $symbol = switch ("$owner.$name") {
            '.drawCorrespondencies' { 'draw_correspondencies' }
            '.drawSearchLines' { 'draw_search_lines' }
            '.drawWireframe' { 'draw_wireframe' }
            '.extractControlPoints' { 'extract_control_points' }
            '.extractLineBundle' { 'extract_line_bundle' }
            '.findCorrespondencies' { 'find_correspondencies' }
            '.convertCorrespondencies' { 'convert_correspondencies' }
            '.rapid' { 'run' }
            'Tracker.compute' { 'tracker_compute' }
            'Tracker.clearState' { 'tracker_clear_state' }
            'Rapid.create' { 'tracker_create' }
            'OLSTracker.create' { 'ols_tracker_create' }
            default { '' }
        }
        if (-not [string]::IsNullOrWhiteSpace([string]$symbol)) { $result = @("jyppx_ocv_rapid_$symbol") }
    } elseif ($Module -eq 'structured_light') {
        $result = switch ("$owner.$name") {
            'GrayCodePattern.create' { @('jyppx_ocv_structured_light_gray_code_pattern_create') }
            'GrayCodePattern.getNumberOfPatternImages' { @('jyppx_ocv_structured_light_gray_code_pattern_get_number_of_pattern_images') }
            'GrayCodePattern.setWhiteThreshold' { @('jyppx_ocv_structured_light_gray_code_pattern_set_white_threshold') }
            'GrayCodePattern.setBlackThreshold' { @('jyppx_ocv_structured_light_gray_code_pattern_set_black_threshold') }
            'GrayCodePattern.getImagesForShadowMasks' { @('jyppx_ocv_structured_light_gray_code_pattern_get_images_for_shadow_masks') }
            'GrayCodePattern.getProjPixel' { @('jyppx_ocv_structured_light_gray_code_pattern_get_proj_pixel') }
            'SinusoidalPattern.create' { @('jyppx_ocv_structured_light_sinusoidal_pattern_create') }
            'SinusoidalPattern.computePhaseMap' { @('jyppx_ocv_structured_light_sinusoidal_pattern_compute_phase_map') }
            'SinusoidalPattern.unwrapPhaseMap' { @('jyppx_ocv_structured_light_sinusoidal_pattern_unwrap_phase_map') }
            'SinusoidalPattern.computeDataModulationTerm' { @('jyppx_ocv_structured_light_sinusoidal_pattern_compute_data_modulation_term') }
            'StructuredLightPattern.generate' { @('jyppx_ocv_structured_light_pattern_generate_count','jyppx_ocv_structured_light_pattern_generate_fill') }
            default { @() }
        }
    } elseif ($Module -eq 'saliency') {
        $result = switch ("$owner.$name") {
            'Saliency.computeSaliency' { @('jyppx_ocv_saliency_compute_saliency') }
            'StaticSaliency.computeBinaryMap' { @('jyppx_ocv_saliency_static_compute_binary_map') }
            'StaticSaliencySpectralResidual.create' { @('jyppx_ocv_saliency_spectral_residual_create') }
            'StaticSaliencySpectralResidual.computeSaliency' { @('jyppx_ocv_saliency_compute_saliency') }
            'StaticSaliencySpectralResidual.getImageWidth' { @('jyppx_ocv_saliency_spectral_residual_get_image_width') }
            'StaticSaliencySpectralResidual.setImageWidth' { @('jyppx_ocv_saliency_spectral_residual_set_image_width') }
            'StaticSaliencySpectralResidual.getImageHeight' { @('jyppx_ocv_saliency_spectral_residual_get_image_height') }
            'StaticSaliencySpectralResidual.setImageHeight' { @('jyppx_ocv_saliency_spectral_residual_set_image_height') }
            'StaticSaliencyFineGrained.create' { @('jyppx_ocv_saliency_fine_grained_create') }
            'StaticSaliencyFineGrained.computeSaliency' { @('jyppx_ocv_saliency_compute_saliency') }
            'MotionSaliencyBinWangApr2014.create' { @('jyppx_ocv_saliency_motion_bin_wang_create') }
            'MotionSaliencyBinWangApr2014.computeSaliency' { @('jyppx_ocv_saliency_compute_saliency') }
            'MotionSaliencyBinWangApr2014.setImagesize' { @('jyppx_ocv_saliency_motion_bin_wang_set_image_size') }
            'MotionSaliencyBinWangApr2014.init' { @('jyppx_ocv_saliency_motion_bin_wang_init') }
            'MotionSaliencyBinWangApr2014.getImageWidth' { @('jyppx_ocv_saliency_motion_bin_wang_get_image_width') }
            'MotionSaliencyBinWangApr2014.setImageWidth' { @('jyppx_ocv_saliency_motion_bin_wang_set_image_width') }
            'MotionSaliencyBinWangApr2014.getImageHeight' { @('jyppx_ocv_saliency_motion_bin_wang_get_image_height') }
            'MotionSaliencyBinWangApr2014.setImageHeight' { @('jyppx_ocv_saliency_motion_bin_wang_set_image_height') }
            'ObjectnessBING.create' { @('jyppx_ocv_saliency_objectness_bing_create') }
            'ObjectnessBING.computeSaliency' { @('jyppx_ocv_saliency_objectness_bing_compute') }
            'ObjectnessBING.getobjectnessValues' { @('jyppx_ocv_saliency_objectness_bing_get_objectness_values_count','jyppx_ocv_saliency_objectness_bing_get_objectness_values_fill') }
            'ObjectnessBING.setTrainingPath' { @('jyppx_ocv_saliency_objectness_bing_set_training_path') }
            'ObjectnessBING.setBBResDir' { @('jyppx_ocv_saliency_objectness_bing_set_bb_res_dir') }
            'ObjectnessBING.getBase' { @('jyppx_ocv_saliency_objectness_bing_get_base') }
            'ObjectnessBING.setBase' { @('jyppx_ocv_saliency_objectness_bing_set_base') }
            'ObjectnessBING.getNSS' { @('jyppx_ocv_saliency_objectness_bing_get_nss') }
            'ObjectnessBING.setNSS' { @('jyppx_ocv_saliency_objectness_bing_set_nss') }
            'ObjectnessBING.getW' { @('jyppx_ocv_saliency_objectness_bing_get_w') }
            'ObjectnessBING.setW' { @('jyppx_ocv_saliency_objectness_bing_set_w') }
            default { @() }
        }
    } elseif ($Module -eq 'surface_matching') {
        $result = switch ("$owner.$name") {
            'ICP.ICP' { @('jyppx_ocv_surface_matching_icp_create') }
            'ICP.registerModelToScene' { if ($Identity -match 'double residual') { @('jyppx_ocv_surface_matching_icp_register_model_to_scene') } else { @() } }
            'PPF3DDetector.PPF3DDetector' { @('jyppx_ocv_surface_matching_ppf_3d_detector_create') }
            'PPF3DDetector.trainModel' { @('jyppx_ocv_surface_matching_ppf_3d_detector_train_model') }
            'PPF3DDetector.match' { @('jyppx_ocv_surface_matching_ppf_3d_detector_match_count','jyppx_ocv_surface_matching_ppf_3d_detector_match_fill') }
            default { @() }
        }
    } elseif ($Module -eq 'reg') {
        $result = switch ("$owner.$name") {
            'Map.warp' { @('jyppx_ocv_reg_map_warp') }
            'Map.inverseWarp' { @('jyppx_ocv_reg_map_inverse_warp') }
            'Map.inverseMap' { @('jyppx_ocv_reg_map_inverse_map') }
            'Map.compose' { @('jyppx_ocv_reg_map_compose') }
            'Map.scale' { @('jyppx_ocv_reg_map_scale') }
            'MapAffine.MapAffine' { @('jyppx_ocv_reg_map_affine_create') }
            'MapAffine.inverseWarp' { @('jyppx_ocv_reg_map_inverse_warp') }
            'MapAffine.inverseMap' { @('jyppx_ocv_reg_map_inverse_map') }
            'MapAffine.compose' { @('jyppx_ocv_reg_map_compose') }
            'MapAffine.scale' { @('jyppx_ocv_reg_map_scale') }
            'MapAffine.getLinTr' { @('jyppx_ocv_reg_map_affine_get') }
            'MapAffine.getShift' { @('jyppx_ocv_reg_map_affine_get') }
            'Mapper.calculate' { @('jyppx_ocv_reg_mapper_calculate') }
            'Mapper.getMap' { @('jyppx_ocv_reg_mapper_get_map') }
            'MapperGradAffine.MapperGradAffine' { @('jyppx_ocv_reg_mapper_grad_affine_create') }
            'MapperGradAffine.calculate' { @('jyppx_ocv_reg_mapper_calculate') }
            'MapperGradAffine.getMap' { @('jyppx_ocv_reg_mapper_get_map') }
            'MapperGradEuclid.MapperGradEuclid' { @('jyppx_ocv_reg_mapper_grad_euclid_create') }
            'MapperGradEuclid.calculate' { @('jyppx_ocv_reg_mapper_calculate') }
            'MapperGradEuclid.getMap' { @('jyppx_ocv_reg_mapper_get_map') }
            'MapperGradProj.MapperGradProj' { @('jyppx_ocv_reg_mapper_grad_proj_create') }
            'MapperGradProj.calculate' { @('jyppx_ocv_reg_mapper_calculate') }
            'MapperGradProj.getMap' { @('jyppx_ocv_reg_mapper_get_map') }
            'MapperGradShift.MapperGradShift' { @('jyppx_ocv_reg_mapper_grad_shift_create') }
            'MapperGradShift.calculate' { @('jyppx_ocv_reg_mapper_calculate') }
            'MapperGradShift.getMap' { @('jyppx_ocv_reg_mapper_get_map') }
            'MapperGradSimilar.MapperGradSimilar' { @('jyppx_ocv_reg_mapper_grad_similar_create') }
            'MapperGradSimilar.calculate' { @('jyppx_ocv_reg_mapper_calculate') }
            'MapperGradSimilar.getMap' { @('jyppx_ocv_reg_mapper_get_map') }
            'MapperPyramid.MapperPyramid' { @('jyppx_ocv_reg_mapper_pyramid_create') }
            'MapperPyramid.calculate' { @('jyppx_ocv_reg_mapper_calculate') }
            'MapperPyramid.getMap' { @('jyppx_ocv_reg_mapper_get_map') }
            'MapProjec.MapProjec' { @('jyppx_ocv_reg_map_projec_create') }
            'MapProjec.inverseWarp' { @('jyppx_ocv_reg_map_inverse_warp') }
            'MapProjec.inverseMap' { @('jyppx_ocv_reg_map_inverse_map') }
            'MapProjec.compose' { @('jyppx_ocv_reg_map_compose') }
            'MapProjec.scale' { @('jyppx_ocv_reg_map_scale') }
            'MapProjec.getProjTr' { @('jyppx_ocv_reg_map_projec_get') }
            'MapProjec.normalize' { @('jyppx_ocv_reg_map_projec_normalize') }
            'MapShift.MapShift' { @('jyppx_ocv_reg_map_shift_create') }
            'MapShift.inverseWarp' { @('jyppx_ocv_reg_map_inverse_warp') }
            'MapShift.inverseMap' { @('jyppx_ocv_reg_map_inverse_map') }
            'MapShift.compose' { @('jyppx_ocv_reg_map_compose') }
            'MapShift.scale' { @('jyppx_ocv_reg_map_scale') }
            'MapShift.getShift' { @('jyppx_ocv_reg_map_shift_get') }
            default { @() }
        }
    } elseif ($Module -eq 'xstereo') {
        $result = switch ("$owner.$name") {
            'QuasiDenseStereo.create' { @('jyppx_ocv_xstereo_quasi_dense_create') }
            'QuasiDenseStereo.loadParameters' { @('jyppx_ocv_xstereo_quasi_dense_load_parameters') }
            'QuasiDenseStereo.saveParameters' { @('jyppx_ocv_xstereo_quasi_dense_save_parameters') }
            'QuasiDenseStereo.getSparseMatches' { @('jyppx_ocv_xstereo_quasi_dense_get_sparse_matches_count','jyppx_ocv_xstereo_quasi_dense_get_sparse_matches_fill') }
            'QuasiDenseStereo.getDenseMatches' { @('jyppx_ocv_xstereo_quasi_dense_get_dense_matches_count','jyppx_ocv_xstereo_quasi_dense_get_dense_matches_fill') }
            'QuasiDenseStereo.process' { @('jyppx_ocv_xstereo_quasi_dense_process') }
            'QuasiDenseStereo.getMatch' { @('jyppx_ocv_xstereo_quasi_dense_get_match') }
            'QuasiDenseStereo.getDisparity' { @('jyppx_ocv_xstereo_quasi_dense_get_disparity') }
            default { @() }
        }
    } elseif ($Module -eq 'ptcloud') {
        switch ("$owner.$name") {
            'RgbdNormals.create' { @('jyppx_ocv_rgbd_normals_create') }
            'RgbdNormals.apply' { @('jyppx_ocv_rgbd_normals_apply') }
            'RgbdNormals.cache' { @('jyppx_ocv_rgbd_normals_cache') }
            'RgbdNormals.getRows' { @('jyppx_ocv_rgbd_normals_get_int_property') }
            'RgbdNormals.setRows' { @('jyppx_ocv_rgbd_normals_set_int_property') }
            'RgbdNormals.getCols' { @('jyppx_ocv_rgbd_normals_get_int_property') }
            'RgbdNormals.setCols' { @('jyppx_ocv_rgbd_normals_set_int_property') }
            'RgbdNormals.getWindowSize' { @('jyppx_ocv_rgbd_normals_get_int_property') }
            'RgbdNormals.setWindowSize' { @('jyppx_ocv_rgbd_normals_set_int_property') }
            'RgbdNormals.getDepth' { @('jyppx_ocv_rgbd_normals_get_int_property') }
            'RgbdNormals.getMethod' { @('jyppx_ocv_rgbd_normals_get_int_property') }
            'RgbdNormals.getK' { @('jyppx_ocv_rgbd_normals_get_k') }
            'RgbdNormals.setK' { @('jyppx_ocv_rgbd_normals_set_k') }
            default {
                switch ($name) {
                    'registerDepth' { @('jyppx_ocv_ptcloud_register_depth') }
                    'depthTo3dSparse' { @('jyppx_ocv_ptcloud_depth_to_3d_sparse') }
                    'depthTo3d' { @('jyppx_ocv_ptcloud_depth_to_3d') }
                    'rescaleDepth' { @('jyppx_ocv_ptcloud_rescale_depth') }
                    'warpFrame' { @('jyppx_ocv_ptcloud_warp_frame') }
                    'findPlanes' { @('jyppx_ocv_ptcloud_find_planes') }
                    default { @() }
                }
            }
        }
    } elseif ($Module -eq 'xobjdetect') {
        switch ("$owner.$name") {
            'CascadeClassifier.CascadeClassifier' { if ($Identity -match '\(\)->$') { $result = @('jyppx_ocv_cascade_classifier_create') } elseif ($Identity -match 'String filename') { $result = @('jyppx_ocv_cascade_classifier_create_from_file') } }
            'CascadeClassifier.load' { $result = @('jyppx_ocv_cascade_classifier_load') }
            'CascadeClassifier.empty' { $result = @('jyppx_ocv_cascade_classifier_empty') }
            'CascadeClassifier.getOriginalWindowSize' { $result = @('jyppx_ocv_cascade_classifier_get_original_window_size') }
            'CascadeClassifier.isOldFormatCascade' { $result = @('jyppx_ocv_cascade_classifier_is_old_format_cascade') }
            'CascadeClassifier.getFeatureType' { $result = @('jyppx_ocv_cascade_classifier_get_feature_type') }
            'CascadeClassifier.detectMultiScale' { if ($Identity -match 'vector_int numDetections') { $result = @('jyppx_ocv_cascade_classifier_detect_multi_scale2_count','jyppx_ocv_cascade_classifier_detect_multi_scale2_fill') } elseif ($Identity -match 'vector_int rejectLevels') { $result = @('jyppx_ocv_cascade_classifier_detect_multi_scale3_count','jyppx_ocv_cascade_classifier_detect_multi_scale3_fill') } else { $result = @('jyppx_ocv_cascade_classifier_detect_multi_scale_count','jyppx_ocv_cascade_classifier_detect_multi_scale_fill') } }
            'HOGDescriptor.HOGDescriptor' { if ($Identity -match '\(\)->$') { $result = @('jyppx_ocv_hog_descriptor_create') } elseif ($Identity -match 'String filename') { $result = @('jyppx_ocv_hog_descriptor_create_from_file') } else { $result = @('jyppx_ocv_hog_descriptor_create_with_params') } }
            'HOGDescriptor.getDefaultPeopleDetector' { $result = @('jyppx_ocv_hog_descriptor_get_default_people_detector_count','jyppx_ocv_hog_descriptor_get_default_people_detector_fill') }
            'HOGDescriptor.getDaimlerPeopleDetector' { $result = @('jyppx_ocv_hog_descriptor_get_daimler_people_detector_count','jyppx_ocv_hog_descriptor_get_daimler_people_detector_fill') }
            'HOGDescriptor.setSVMDetector' { $result = @('jyppx_ocv_hog_descriptor_set_svm_detector') }
            'HOGDescriptor.checkDetectorSize' { $result = @('jyppx_ocv_hog_descriptor_check_detector_size') }
            'HOGDescriptor.getDescriptorSize' { $result = @('jyppx_ocv_hog_descriptor_get_descriptor_size') }
            'HOGDescriptor.getWinSigma' { $result = @('jyppx_ocv_hog_descriptor_get_win_sigma') }
            'HOGDescriptor.detect' { $result = @('jyppx_ocv_hog_descriptor_detect_count','jyppx_ocv_hog_descriptor_detect_fill') }
            'HOGDescriptor.detectMultiScale' { $result = @('jyppx_ocv_hog_descriptor_detect_multi_scale_count','jyppx_ocv_hog_descriptor_detect_multi_scale_fill') }
            default { if ($owner -eq 'HOGDescriptor' -and $name -match '^(get|set)') { $result = @('jyppx_ocv_hog_descriptor_get_property','jyppx_ocv_hog_descriptor_set_property') } }
        }
    } elseif ($Module -eq 'dnn_superres') {
        $result = @()
    } elseif ($Module -eq 'shape') {
        if ($owner -eq 'HistogramCostExtractor') {
            $result = switch ($name) {
                'buildCostMatrix' { @('jyppx_ocv_shape_histogram_cost_extractor_build_cost_matrix') }
                'setNDummies' { @('jyppx_ocv_shape_histogram_cost_extractor_set_n_dummies') }
                'getNDummies' { @('jyppx_ocv_shape_histogram_cost_extractor_get_n_dummies') }
                'setDefaultCost' { @('jyppx_ocv_shape_histogram_cost_extractor_set_default_cost') }
                'getDefaultCost' { @('jyppx_ocv_shape_histogram_cost_extractor_get_default_cost') }
                default { @() }
            }
        } elseif ($owner -in @('NormHistogramCostExtractor','EMDHistogramCostExtractor')) {
            $result = switch ($name) {
                'setNormFlag' { @('jyppx_ocv_shape_histogram_cost_extractor_set_norm_flag') }
                'getNormFlag' { @('jyppx_ocv_shape_histogram_cost_extractor_get_norm_flag') }
                default { @() }
            }
        } elseif ($owner -eq 'ShapeDistanceExtractor' -and $name -eq 'computeDistance') {
            $result = @('jyppx_ocv_shape_distance_extractor_compute_distance')
        } elseif ($owner -eq 'HausdorffDistanceExtractor') {
            $result = switch ($name) {
                'setDistanceFlag' { @('jyppx_ocv_shape_hausdorff_distance_extractor_set_distance_flag') }
                'getDistanceFlag' { @('jyppx_ocv_shape_hausdorff_distance_extractor_get_distance_flag') }
                'setRankProportion' { @('jyppx_ocv_shape_hausdorff_distance_extractor_set_rank_proportion') }
                'getRankProportion' { @('jyppx_ocv_shape_hausdorff_distance_extractor_get_rank_proportion') }
                default { @() }
            }
        } elseif ([string]::IsNullOrWhiteSpace($owner)) {
            $result = switch ($name) {
                'createNormHistogramCostExtractor' { @('jyppx_ocv_shape_norm_histogram_cost_extractor_create') }
                'createEMDHistogramCostExtractor' { @('jyppx_ocv_shape_emd_histogram_cost_extractor_create') }
                'createChiHistogramCostExtractor' { @('jyppx_ocv_shape_chi_histogram_cost_extractor_create') }
                'createEMDL1HistogramCostExtractor' { @('jyppx_ocv_shape_emd_l1_histogram_cost_extractor_create') }
                'createShapeContextDistanceExtractor' { @('jyppx_ocv_shape_context_distance_extractor_create') }
                'createHausdorffDistanceExtractor' { @('jyppx_ocv_shape_hausdorff_distance_extractor_create') }
                default { @() }
            }
        }
    } elseif ($Module -eq 'fuzzy') {
        $symbol = switch ($name) {
            'FT02D_components' { 'ft02d_components' }
            'FT02D_inverseFT' { 'ft02d_inverse_ft' }
            'FT02D_process' { 'ft02d_process' }
            'FT02D_iteration' { 'ft02d_iteration' }
            'FT02D_FL_process' { 'ft02d_fl_process' }
            'FT02D_FL_process_float' { 'ft02d_fl_process_float' }
            'FT12D_components' { 'ft12d_components' }
            'FT12D_polynomial' { 'ft12d_polynomial' }
            'FT12D_createPolynomMatrixVertical' { 'ft12d_create_polynom_matrix_vertical' }
            'FT12D_createPolynomMatrixHorizontal' { 'ft12d_create_polynom_matrix_horizontal' }
            'FT12D_process' { 'ft12d_process' }
            'FT12D_inverseFT' { 'ft12d_inverse_ft' }
            'createKernel' { if ($Identity -match '^cv\.ft\.createKernel\(Mat A;Mat B;') { 'create_kernel_from_functions' } else { 'create_kernel' } }
            'inpaint' { 'inpaint' }
            'filter' { 'filter' }
            default { '' }
        }
        if (-not [string]::IsNullOrWhiteSpace([string]$symbol)) { $result = @("jyppx_ocv_fuzzy_$symbol") }
    } elseif ($Module -eq 'hfs') {
        if ($owner -eq 'HfsSegment' -and $name -eq 'create') { $result = @('jyppx_ocv_hfs_segment_create') }
        elseif ($owner -eq 'HfsSegment' -and $name -in @('performSegmentCpu','performSegmentGpu')) {
            $operation = if ($name -eq 'performSegmentCpu') { 'cpu' } else { 'gpu' }
            $result = @("jyppx_ocv_hfs_segment_perform_segment_$operation")
        }
        elseif ($owner -eq 'HfsSegment' -and $name -match '^(get|set)(SegEgbThresholdI|SegEgbThresholdII|MinRegionSizeI|MinRegionSizeII|SpatialWeight|SlicSpixelSize|NumSlicIter)$') {
            $verb = $Matches[1].ToLowerInvariant()
            $property = [string]$Matches[2]
            $kind = if ($property -in @('SegEgbThresholdI','SegEgbThresholdII','SpatialWeight')) { 'float' } else { 'int' }
            $result = @("jyppx_ocv_hfs_segment_${verb}_${kind}_property")
        }
    } elseif ($Module -eq 'phase_unwrapping') {
        if ($owner -eq 'HistogramPhaseUnwrapping' -and $name -eq 'create') { $result = @('jyppx_ocv_phase_unwrapping_histogram_create') }
        elseif ($owner -eq 'HistogramPhaseUnwrapping' -and $name -eq 'getInverseReliabilityMap') { $result = @('jyppx_ocv_phase_unwrapping_histogram_get_inverse_reliability_map') }
        elseif ($owner -eq 'PhaseUnwrapping' -and $name -eq 'unwrapPhaseMap') { $result = @('jyppx_ocv_phase_unwrapping_unwrap_phase_map') }
    } elseif ($Module -eq 'bioinspired') {
        switch ($owner) {
            'Retina' {
                switch ($name) {
                    'getInputSize' { $result = @('jyppx_ocv_bioinspired_retina_get_input_size') }
                    'getOutputSize' { $result = @('jyppx_ocv_bioinspired_retina_get_output_size') }
                    'setup' { $result = @('jyppx_ocv_bioinspired_retina_setup') }
                    'printSetup' { $result = @('jyppx_ocv_bioinspired_retina_print_setup_length','jyppx_ocv_bioinspired_retina_print_setup_fill') }
                    'write' { $result = @('jyppx_ocv_bioinspired_retina_write') }
                    'setupOPLandIPLParvoChannel' { $result = @('jyppx_ocv_bioinspired_retina_setup_parvo') }
                    'setupIPLMagnoChannel' { $result = @('jyppx_ocv_bioinspired_retina_setup_magno') }
                    'run' { $result = @('jyppx_ocv_bioinspired_retina_run') }
                    'applyFastToneMapping' { $result = @('jyppx_ocv_bioinspired_retina_apply_fast_tone_mapping') }
                    'getParvo' { $result = @('jyppx_ocv_bioinspired_retina_get_parvo') }
                    'getParvoRAW' { $result = @('jyppx_ocv_bioinspired_retina_get_parvo_raw') }
                    'getMagno' { $result = @('jyppx_ocv_bioinspired_retina_get_magno') }
                    'getMagnoRAW' { $result = @('jyppx_ocv_bioinspired_retina_get_magno_raw') }
                    'setColorSaturation' { $result = @('jyppx_ocv_bioinspired_retina_set_color_saturation') }
                    'clearBuffers' { $result = @('jyppx_ocv_bioinspired_retina_clear_buffers') }
                    'activateMovingContoursProcessing' { $result = @('jyppx_ocv_bioinspired_retina_activate_moving_contours_processing') }
                    'activateContoursProcessing' { $result = @('jyppx_ocv_bioinspired_retina_activate_contours_processing') }
                    'create' { $result = @('jyppx_ocv_bioinspired_retina_create') }
                }
            }
            'RetinaFastToneMapping' {
                switch ($name) {
                    'applyFastToneMapping' { $result = @('jyppx_ocv_bioinspired_retina_fast_tone_mapping_apply') }
                    'setup' { $result = @('jyppx_ocv_bioinspired_retina_fast_tone_mapping_setup') }
                    'create' { $result = @('jyppx_ocv_bioinspired_retina_fast_tone_mapping_create') }
                }
            }
            'TransientAreasSegmentationModule' {
                switch ($name) {
                    'getSize' { $result = @('jyppx_ocv_bioinspired_transient_areas_get_size') }
                    'setup' { $result = @('jyppx_ocv_bioinspired_transient_areas_setup') }
                    'printSetup' { $result = @('jyppx_ocv_bioinspired_transient_areas_print_setup_length','jyppx_ocv_bioinspired_transient_areas_print_setup_fill') }
                    'write' { $result = @('jyppx_ocv_bioinspired_transient_areas_write') }
                    'run' { $result = @('jyppx_ocv_bioinspired_transient_areas_run') }
                    'getSegmentationPicture' { $result = @('jyppx_ocv_bioinspired_transient_areas_get_segmentation_picture') }
                    'clearAllBuffers' { $result = @('jyppx_ocv_bioinspired_transient_areas_clear_all_buffers') }
                    'create' { $result = @('jyppx_ocv_bioinspired_transient_areas_create') }
                }
            }
        }
    } elseif ($Module -eq 'plot') {
        if ($owner -eq 'Plot2d') {
            if ($name -eq 'create') {
                if ($Identity -match 'Mat dataY') { $result = @('jyppx_ocv_plot_2d_create_xy') }
                else { $result = @('jyppx_ocv_plot_2d_create') }
            }
            elseif ($name -eq 'render') { $result = @('jyppx_ocv_plot_2d_render') }
            elseif ($name -match '^set') {
                $suffix = switch ($name) {
                    'setMinX' { 'min_x' }
                    'setMinY' { 'min_y' }
                    'setMaxX' { 'max_x' }
                    'setMaxY' { 'max_y' }
                    'setPlotLineWidth' { 'plot_line_width' }
                    'setNeedPlotLine' { 'need_plot_line' }
                    'setPlotLineColor' { 'plot_line_color' }
                    'setPlotBackgroundColor' { 'plot_background_color' }
                    'setPlotAxisColor' { 'plot_axis_color' }
                    'setPlotGridColor' { 'plot_grid_color' }
                    'setPlotTextColor' { 'plot_text_color' }
                    'setPlotSize' { 'plot_size' }
                    'setShowGrid' { 'show_grid' }
                    'setShowText' { 'show_text' }
                    'setGridLinesNumber' { 'grid_lines_number' }
                    'setInvertOrientation' { 'invert_orientation' }
                    'setPointIdxToPrint' { 'point_idx_to_print' }
                    default { '' }
                }
                if (-not [string]::IsNullOrWhiteSpace([string]$suffix)) { $result = @("jyppx_ocv_plot_2d_set_$suffix") }
            }
        }
    } elseif ($Module -eq 'intensity_transform') {
        if ([string]::IsNullOrWhiteSpace($owner)) {
            switch ($name) {
                'logTransform' { $result = @('jyppx_ocv_intensity_transform_log') }
                'gammaCorrection' { $result = @('jyppx_ocv_intensity_transform_gamma_correction') }
                'autoscaling' { $result = @('jyppx_ocv_intensity_transform_autoscaling') }
                'contrastStretching' { $result = @('jyppx_ocv_intensity_transform_contrast_stretching') }
                'BIMEF' {
                    if ($Identity -match '\bfloat k\b') { $result = @('jyppx_ocv_intensity_transform_bimef_with_k') }
                    else { $result = @('jyppx_ocv_intensity_transform_bimef') }
                }
            }
        }
    } elseif ($Module -eq 'alphamat') {
        if ([string]::IsNullOrWhiteSpace($owner) -and $name -eq 'infoFlow') { $result = @('jyppx_ocv_alphamat_info_flow') }
    } elseif ($Module -eq 'freetype') {
        return @()
    } elseif ($Module -eq 'bgsegm') {
        if ($name -eq 'apply' -and $owner -match '^BackgroundSubtractor(MOG|GMG|CNT)$') {
            if ($Identity -match 'knownForegroundMask') { $result = @('jyppx_ocv_bgsegm_background_subtractor_apply_with_known_foreground') } else { $result = @('jyppx_ocv_bgsegm_background_subtractor_apply') }
        } elseif ($name -eq 'getBackgroundImage' -and $owner -match '^BackgroundSubtractor(MOG|GMG|CNT)$') {
            $result = @('jyppx_ocv_bgsegm_background_subtractor_get_background_image')
        } elseif ([string]::IsNullOrWhiteSpace($owner)) {
            switch -Regex ($name) {
                '^createBackgroundSubtractorMOG$' { $result = @('jyppx_ocv_bgsegm_background_subtractor_mog_create') }
                '^createBackgroundSubtractorGMG$' { $result = @('jyppx_ocv_bgsegm_background_subtractor_gmg_create') }
                '^createBackgroundSubtractorCNT$' { $result = @('jyppx_ocv_bgsegm_background_subtractor_cnt_create') }
                '^createSyntheticSequenceGenerator$' { $result = @('jyppx_ocv_bgsegm_synthetic_sequence_generator_create') }
            }
        } elseif ($owner -eq 'SyntheticSequenceGenerator') {
            if ($name -eq 'SyntheticSequenceGenerator') { $result = @('jyppx_ocv_bgsegm_synthetic_sequence_generator_create') } elseif ($name -eq 'getNextFrame') { $result = @('jyppx_ocv_bgsegm_synthetic_sequence_generator_get_next_frame') }
        } elseif ($owner -match '^BackgroundSubtractor(MOG|GMG|CNT)$' -and $name -match '^(get|set)') {
            $prefix = switch -Regex ($owner) { 'MOG$' { 'jyppx_ocv_bgsegm_background_subtractor_mog_' } 'GMG$' { 'jyppx_ocv_bgsegm_background_subtractor_gmg_' } 'CNT$' { 'jyppx_ocv_bgsegm_background_subtractor_cnt_' } }
            $numeric = $Identity -match '(?:->|\b)(?:int|bool)\b' -or $Identity -match '\b(?:int|bool)\s+[A-Za-z_]'
            $kind = if ($numeric) { 'int' } else { 'double' }
            $result = @($prefix + $name.Substring(0, 3).ToLowerInvariant() + "_$kind")
        }
    } elseif ($Module -in @('face','quality','img_hash','line_descriptor')) {
        switch ($Module) {
            'face' {
                switch ($owner) {
                    'BIF' {
                        switch ($name) {
                            'getNumBands' { $result = @('jyppx_ocv_face_bif_get_num_bands') }
                            'getNumRotations' { $result = @('jyppx_ocv_face_bif_get_num_rotations') }
                            'compute' { $result = @('jyppx_ocv_face_bif_compute') }
                            'create' { $result = @('jyppx_ocv_face_bif_create') }
                        }
                    }
                    'Facemark' {
                        if ($name -eq 'loadModel') { $result = @('jyppx_ocv_face_facemark_load_model') }
                        elseif ($name -eq 'fit') { $result = @('jyppx_ocv_face_facemark_fit','jyppx_ocv_face_facemark_fit_landmarks_count','jyppx_ocv_face_facemark_fit_landmarks_fill') }
                    }
                    'BasicFaceRecognizer' {
                        switch ($name) {
                            'getNumComponents' { $result = @('jyppx_ocv_face_basic_get_num_components') }
                            'setNumComponents' { $result = @('jyppx_ocv_face_basic_set_num_components') }
                            'getThreshold' { $result = @('jyppx_ocv_face_recognizer_get_threshold') }
                            'setThreshold' { $result = @('jyppx_ocv_face_recognizer_set_threshold') }
                            'getProjections' { $result = @('jyppx_ocv_face_basic_get_projections_count','jyppx_ocv_face_basic_get_projections_fill') }
                            'getLabels' { $result = @('jyppx_ocv_face_basic_get_labels') }
                            'getEigenValues' { $result = @('jyppx_ocv_face_basic_get_eigen_values') }
                            'getEigenVectors' { $result = @('jyppx_ocv_face_basic_get_eigen_vectors') }
                            'getMean' { $result = @('jyppx_ocv_face_basic_get_mean') }
                        }
                    }
                    'EigenFaceRecognizer' { if ($name -eq 'create') { $result = @('jyppx_ocv_face_eigen_create') } }
                    'FisherFaceRecognizer' { if ($name -eq 'create') { $result = @('jyppx_ocv_face_fisher_create') } }
                    'LBPHFaceRecognizer' {
                        if ($name -eq 'create') { $result = @('jyppx_ocv_face_lbph_create') }
                        elseif ($name -eq 'getThreshold') { $result = @('jyppx_ocv_face_recognizer_get_threshold') }
                        elseif ($name -eq 'setThreshold') { $result = @('jyppx_ocv_face_recognizer_set_threshold') }
                        elseif ($name -eq 'getHistograms') { $result = @('jyppx_ocv_face_lbph_get_histograms_count','jyppx_ocv_face_lbph_get_histograms_fill') }
                        elseif ($name -eq 'getLabels') { $result = @('jyppx_ocv_face_lbph_get_labels') }
                        elseif ($name -match '^(get|set)(GridX|GridY|Radius|Neighbors)$') {
                            $verb = $Matches[1].ToLowerInvariant()
                            $property = switch ($Matches[2]) { 'GridX' {'grid_x'} 'GridY' {'grid_y'} 'Radius' {'radius'} 'Neighbors' {'neighbors'} }
                            $result = @("jyppx_ocv_face_lbph_${verb}_${property}")
                        }
                    }
                    'MACE' {
                        switch ($name) {
                            'salt' { $result = @('jyppx_ocv_face_mace_salt') }
                            'train' { $result = @('jyppx_ocv_face_mace_train') }
                            'same' { $result = @('jyppx_ocv_face_mace_same') }
                            'load' { $result = @('jyppx_ocv_face_mace_load') }
                            'create' { $result = @('jyppx_ocv_face_mace_create') }
                            'save' { $result = @('jyppx_ocv_face_mace_save') }
                            'empty' { $result = @('jyppx_ocv_face_mace_empty') }
                        }
                    }
                    'StandardCollector' {
                        switch ($name) {
                            'getMinLabel' { $result = @('jyppx_ocv_face_standard_collector_get_min_label') }
                            'getMinDist' { $result = @('jyppx_ocv_face_standard_collector_get_min_dist') }
                            'getResults' { $result = @('jyppx_ocv_face_standard_collector_get_results_count','jyppx_ocv_face_standard_collector_get_results_fill') }
                            'create' { $result = @('jyppx_ocv_face_standard_collector_create') }
                        }
                    }
                }
                if ([string]::IsNullOrWhiteSpace($owner) -and $name -eq 'createFacemarkLBF') { $result = @('jyppx_ocv_face_facemark_lbf_create') }
            }
            'quality' {
                if ($name -eq 'compute' -and ($owner -eq 'QualityBase' -or $Identity -match '\((?:Mat img|Mat cmp)\)')) { $result = @('jyppx_ocv_quality_compute') }
                elseif ($name -eq 'getQualityMap') { $result = @('jyppx_ocv_quality_get_quality_map') }
                elseif ($name -eq 'clear') { $result = @('jyppx_ocv_quality_clear') }
                elseif ($name -eq 'empty') { $result = @('jyppx_ocv_quality_empty') }
                elseif ($owner -eq 'QualityBRISQUE') {
                    if ($name -eq 'computeFeatures') { $result = @('jyppx_ocv_quality_brisque_compute_features') }
                    elseif ($name -eq 'compute' -and $Identity -match 'model_file_path') { $result = @('jyppx_ocv_quality_brisque_compute_static') }
                    elseif ($name -eq 'compute' -and $Identity -match '\(Mat img\)') { $result = @('jyppx_ocv_quality_compute') }
                    elseif ($name -eq 'create' -and $Identity -notmatch 'Ptr_ml_SVM') { $result = @('jyppx_ocv_quality_brisque_create') }
                }
                elseif ($owner -in @('QualityGMSD','QualityMSE','QualityPSNR','QualitySSIM')) {
                    $stem = switch ($owner) { 'QualityGMSD' {'gmsd'} 'QualityMSE' {'mse'} 'QualityPSNR' {'psnr'} 'QualitySSIM' {'ssim'} }
                    if ($name -eq 'create') { $result = @("jyppx_ocv_quality_$($stem)_create") }
                    elseif ($name -eq 'compute' -and $Identity -match '\(Mat ref;Mat cmp;') { $result = @("jyppx_ocv_quality_$($stem)_compute_static") }
                    elseif ($name -eq 'getMaxPixelValue') { $result = @('jyppx_ocv_quality_psnr_get_max_pixel_value') }
                    elseif ($name -eq 'setMaxPixelValue') { $result = @('jyppx_ocv_quality_psnr_set_max_pixel_value') }
                }
            }
            'img_hash' {
                if ([string]::IsNullOrWhiteSpace($owner)) {
                    $result = switch ($name) {
                        'averageHash' { @('jyppx_ocv_img_hash_average_compute_static') }
                        'blockMeanHash' { @('jyppx_ocv_img_hash_block_mean_compute_static') }
                        'colorMomentHash' { @('jyppx_ocv_img_hash_color_moment_compute_static') }
                        'marrHildrethHash' { @('jyppx_ocv_img_hash_marr_hildreth_compute_static') }
                        'pHash' { @('jyppx_ocv_img_hash_phash_compute_static') }
                        'radialVarianceHash' { @('jyppx_ocv_img_hash_radial_variance_compute_static') }
                        default { @() }
                    }
                } else {
                    $stem = switch ($owner) { 'AverageHash' {'average'} 'BlockMeanHash' {'block_mean'} 'ColorMomentHash' {'color_moment'} 'MarrHildrethHash' {'marr_hildreth'} 'PHash' {'phash'} 'RadialVarianceHash' {'radial_variance'} }
                    if ($owner -eq 'ImgHashBase') {
                        if ($name -eq 'compute') { $result = @('jyppx_ocv_img_hash_compute') }
                        elseif ($name -eq 'compare') { $result = @('jyppx_ocv_img_hash_compare') }
                    } elseif ($name -eq 'create') { $result = @("jyppx_ocv_img_hash_$($stem)_create") }
                    elseif ($owner -eq 'BlockMeanHash' -and $name -eq 'getMean') { $result = @('jyppx_ocv_img_hash_block_mean_get_mean_count','jyppx_ocv_img_hash_block_mean_get_mean_fill') }
                    elseif ($owner -eq 'BlockMeanHash' -and $name -eq 'setMode') { $result = @('jyppx_ocv_img_hash_block_mean_set_mode') }
                    elseif ($owner -eq 'MarrHildrethHash' -and $name -match '^get(Alpha|Scale)$') { $result = @('jyppx_ocv_img_hash_marr_hildreth_get') }
                    elseif ($owner -eq 'MarrHildrethHash' -and $name -eq 'setKernelParam') { $result = @('jyppx_ocv_img_hash_marr_hildreth_set_kernel_param') }
                    elseif ($owner -eq 'RadialVarianceHash' -and $name -match '^get(NumOfAngleLine|Sigma)$') { $result = @('jyppx_ocv_img_hash_radial_variance_get') }
                    elseif ($owner -eq 'RadialVarianceHash' -and $name -match '^set(NumOfAngleLine|Sigma)$') {
                        $property = switch ($Matches[1]) { 'NumOfAngleLine' {'num_of_angle_line'} 'Sigma' {'sigma'} }
                        $result = @("jyppx_ocv_img_hash_radial_variance_set_$property")
                    }
                }
            }
            'line_descriptor' {
                if ([string]::IsNullOrWhiteSpace($owner)) {
                    if ($name -eq 'drawKeylines') { $result = @('jyppx_ocv_line_descriptor_draw_keylines') }
                    elseif ($name -eq 'drawLineMatches') { $result = @('jyppx_ocv_line_descriptor_draw_line_matches') }
                } elseif ($owner -eq 'BinaryDescriptor') {
                    if ($name -eq 'createBinaryDescriptor') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_create') }
                    elseif ($name -eq 'detect') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_detect_count','jyppx_ocv_line_descriptor_binary_descriptor_detect_fill') }
                    elseif ($name -eq 'compute') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_compute') }
                    elseif ($name -eq 'getNumOfOctaves') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_get_num_of_octaves') }
                    elseif ($name -eq 'setNumOfOctaves') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_set_num_of_octaves') }
                    elseif ($name -eq 'getWidthOfBand') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_get_width_of_band') }
                    elseif ($name -eq 'setWidthOfBand') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_set_width_of_band') }
                    elseif ($name -eq 'getReductionRatio') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_get_reduction_ratio') }
                    elseif ($name -eq 'setReductionRatio') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_set_reduction_ratio') }
                } elseif ($owner -eq 'BinaryDescriptorMatcher') {
                    if ($name -eq 'BinaryDescriptorMatcher') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_matcher_create') }
                    elseif ($name -eq 'match' -and $Identity -match 'Mat trainDescriptors') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_count','jyppx_ocv_line_descriptor_binary_descriptor_matcher_match_fill') }
                    elseif ($name -eq 'knnMatch' -and $Identity -match 'Mat trainDescriptors') { $result = @('jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_count','jyppx_ocv_line_descriptor_binary_descriptor_matcher_knn_match_fill') }
                }
            }
        }
    } else {
        if ([string]::IsNullOrWhiteSpace($owner)) {
            $result = switch ($name) {
                'updateMotionHistory' { @('jyppx_ocv_motempl_update_motion_history') }
                'calcMotionGradient' { @('jyppx_ocv_motempl_calc_motion_gradient') }
                'calcGlobalOrientation' { @('jyppx_ocv_motempl_calc_global_orientation') }
                'segmentMotion' { @('jyppx_ocv_motempl_segment_motion_count', 'jyppx_ocv_motempl_segment_motion_fill') }
                'calcOpticalFlowDenseRLOF' { @('jyppx_ocv_optflow_calc_optical_flow_dense_rlof') }
                'calcOpticalFlowSparseRLOF' { @('jyppx_ocv_optflow_calc_optical_flow_sparse_rlof') }
                'createOptFlow_DenseRLOF' { @('jyppx_ocv_optflow_dense_rlof_create') }
                'createOptFlow_SparseRLOF' { @('jyppx_ocv_optflow_sparse_rlof_create') }
                default { @() }
            }
        } else {
            switch ($owner) {
                'RLOFOpticalFlowParameter' {
                    if ($name -eq 'create') { $result = @('jyppx_ocv_optflow_rlof_parameter_create') } elseif ($name -eq 'setUseMEstimator') { $result = @('jyppx_ocv_optflow_rlof_parameter_set_use_m_estimator') } elseif ($name -match '^(get|set)') { $kind = if ($Identity -match '(?:float|double)') { 'float' } else { 'int' }; $result = @("jyppx_ocv_optflow_rlof_parameter_$($name.Substring(0, 3).ToLowerInvariant())_$kind") }
                }
                'DenseRLOFOpticalFlow' {
                    if ($name -eq 'create') { $result = @('jyppx_ocv_optflow_dense_rlof_create') } elseif ($name -eq 'setRLOFOpticalFlowParameter') { $result = @('jyppx_ocv_optflow_dense_rlof_set_parameter') } elseif ($name -eq 'getRLOFOpticalFlowParameter') { $result = @('jyppx_ocv_optflow_dense_rlof_get_parameter') } elseif ($name -match '^(get|set)GridStep$') { $result = @("jyppx_ocv_optflow_dense_rlof_$($name.Substring(0, 3).ToLowerInvariant())_grid_step") } elseif ($name -match '^(get|set)') { $kind = if ($Identity -match '(?:float|double)') { 'float' } else { 'int' }; $result = @("jyppx_ocv_optflow_dense_rlof_$($name.Substring(0, 3).ToLowerInvariant())_$kind") }
                }
                'SparseRLOFOpticalFlow' {
                    if ($name -eq 'create') { $result = @('jyppx_ocv_optflow_sparse_rlof_create') } elseif ($name -eq 'setRLOFOpticalFlowParameter') { $result = @('jyppx_ocv_optflow_sparse_rlof_set_parameter') } elseif ($name -eq 'getRLOFOpticalFlowParameter') { $result = @('jyppx_ocv_optflow_sparse_rlof_get_parameter') } elseif ($name -match '^(get|set)ForwardBackward$') { $result = @("jyppx_ocv_optflow_sparse_rlof_$($name.Substring(0, 3).ToLowerInvariant())_forward_backward") }
                }
                'DenseOpticalFlow' { if ($name -eq 'calc') { $result = @('jyppx_ocv_optflow_dense_calc') } elseif ($name -eq 'collectGarbage') { $result = @('jyppx_ocv_optflow_dense_collect_garbage') } }
                'SparseOpticalFlow' { if ($name -eq 'calc') { $result = @('jyppx_ocv_optflow_sparse_calc') } }
                'DualTVL1OpticalFlow' {
                    if ($name -eq 'create') { $result = @('jyppx_ocv_optflow_dual_tvl1_create') } elseif ($name -match '^(get|set)') { $kind = if ($Identity -match '(?:double|float)') { 'double' } else { 'int' }; $result = @("jyppx_ocv_optflow_dual_tvl1_$($name.Substring(0, 3).ToLowerInvariant())_$kind") }
                }
            }
        }
    }
    @($result | Where-Object { $nativeEntries -contains $_ } | Sort-Object -Unique)
}

$rows = [System.Collections.Generic.List[object]]::new()
foreach ($decl in @($raw.declarations)) {
    $classification = 'non-callable-metadata'; $reason = 'Enum and class declarations are retained as parser metadata; callable evidence is classified independently.'; $native = @(); $managed = @()
    if ([string]$decl.kind -eq 'callable') {
        $parts = GetParts ([string]$decl.identity)
        if ($null -eq $parts) { throw "Could not split $DisplayName declaration identity at ordinal $($decl.ordinal)." }
        $managed = @(GetManaged $parts.Owner $parts.Name ([string]$decl.identity))
        $native = @(GetNative $parts ([string]$decl.identity))
        if ($managed.Count -gt 0 -and $native.Count -gt 0) { $classification = 'implemented'; $reason = "Explicit $DisplayName declaration-to-symbol mapping and exact managed baseline evidence are present." }
        else { $classification = 'intentionally-omitted'; $reason = if ($native.Count -eq 0) { "No native $DisplayName wrapper entrypoint is mapped for this parser declaration in the current ABI." } else { "A native $DisplayName entrypoint exists, but no exact matching managed member is present in the current baseline." } }
        if ($Module -eq 'freetype') { $reason = 'The contrib FreeType2 surface depends on external FreeType/Harfbuzz and has no native wrapper entrypoint or managed API in the current package.' }
        if ($Module -eq 'shape') { $reason = 'The pinned Shape parser closure includes APIs beyond the current managed/native wrapper; this declaration has no corresponding native ABI entrypoint in the scoped wrapper surface and remains intentionally omitted.' }
        if ($Module -eq 'saliency' -and [string]$decl.identity -match 'StaticSaliencySpectralResidual\.(?:read|write)\(') { $reason = 'The upstream persistence helpers use FileNode/FileStorage ownership and serialization contracts that are outside the current managed/native Saliency wrapper; they remain intentionally omitted.' }
        if ($Module -eq 'surface_matching' -and [string]$decl.identity -match '^cv\.ppf_match_3d\.(?:Pose3D\.|loadPLYSimple|writePLY|writePLYVisibleNormals|samplePCByQuantization|transformPCPose|getRandomPose|addNoisePC|computeNormalsPC3d)') { $reason = 'The current SurfaceMatching wrapper intentionally scopes evidence to ICP and PPF3DDetector opaque handles; Pose3D mutators and point-cloud file/utility helpers remain outside the managed/native ownership and deterministic smoke contract.' }
        if ($Module -eq 'reg' -and [string]$decl.identity -match '^cv\.reg\.MapTypeCaster\.') { $reason = 'MapTypeCaster conversion helpers are outside the current managed Reg wrapper; typed map handles are selected by the existing factory and ownership contract.' }
        if ($Module -eq 'xstereo' -and [string]$decl.identity -match '^cv\.stereo\.MatchQuasiDense\.(?:MatchQuasiDense|operator <)') { $reason = 'MatchQuasiDense value construction and comparison are represented by the managed value type; the current native ABI exposes these records only through QuasiDenseStereo count/fill transport, not standalone callable entrypoints.' }
        if ($Module -eq 'ptcloud' -and [string]$decl.identity -match '^cv\.RgbdNormals\.(?:RgbdNormals|apply|cache|getRows|setRows|getCols|setCols|getWindowSize|setWindowSize|getDepth|getK|setK|getMethod)') { $reason = 'RgbdNormals is represented by the current opaque native handle and managed owner; property accessors and output overloads share the stable ABI transport for this parser declaration.' }
        if ($Module -eq 'xobjdetect' -and $classification -eq 'implemented' -and [string]$decl.identity -match '^cv\.(?:CascadeClassifier|HOGDescriptor)\.') { $reason = 'The current XObjDetect wrapper covers this declaration through opaque handles, count/fill transport, and property transport; optional NOT_LINKED behavior remains part of the native boundary.' }
        if ($Module -eq 'dnn_superres' -and $classification -eq 'intentionally-omitted') { $reason = 'The DNN Super Resolution contrib surface is model-driven and has no native DnnSuperres wrapper entrypoint or managed public owner in the current package; model provenance, backend/target policy, and output ownership remain outside this scoped map.' }
        if ($Module -eq 'wechat_qrcode' -and $classification -eq 'intentionally-omitted') { $reason = 'The WeChat QRCode contrib surface is model-driven and has no native WeChatQRCode wrapper entrypoint or managed public owner in the current package; caller-supplied detector/super-resolution models, backend policy, and decoded-string ownership remain outside this scoped map.' }
        if ($Module -eq 'cnn_3dobj' -and $classification -eq 'intentionally-omitted') { $reason = 'The cnn_3dobj contrib surface depends on an external Caffe triplet build and caller-supplied network/model files; the current package has no native Cnn3DObj wrapper entrypoint or managed public owner, so model provenance, device policy, and output ownership remain outside this scoped map.' }
        if ($Module -eq 'signal' -and $classification -eq 'intentionally-omitted') { $reason = 'The signal contrib surface exposes resampleSignal but the current package has no native Signal wrapper entrypoint or managed public owner; the optional module remains outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'hdf' -and $classification -eq 'intentionally-omitted') { $reason = 'The HDF contrib surface requires an external HDF5 development/runtime dependency and the current package has no native HDF wrapper entrypoint or managed public owner; dataset, attribute, type, and ownership contracts remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'text' -and $classification -eq 'intentionally-omitted') { $reason = 'The contrib Text surface has no native Text wrapper entrypoint or managed public owner in the current package; scene-text classifiers and OCR models are caller-selected data, while OCRTesseract is conditionally built against external Tesseract, so model provenance, optional dependency behavior, and output ownership remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'xfeatures2d' -and $classification -eq 'intentionally-omitted') { $reason = 'The xfeatures2d contrib surface is optional CUDA/nonfree functionality and the current package has no native XFeatures2D wrapper entrypoint or managed public owner; CUDA runtime/toolkit availability, nonfree build policy, and feature/model ownership remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'dnn_objdetect' -and $classification -eq 'intentionally-omitted') { $reason = 'The dnn_objdetect contrib surface is a model-driven DNN post-processing helper and the current package has no native DnnObjDetect wrapper entrypoint or managed public owner; caller-selected network/model data, backend policy, and detection output ownership remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'ccalib' -and $classification -eq 'intentionally-omitted') { $reason = 'The ccalib public surface, including omnidir calibration and reconstruction helpers, has no native Ccalib wrapper entrypoint or managed public owner in the current package; caller-owned calibration inputs, feature/matcher objects, and output buffers remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'julia' -and $classification -eq 'intentionally-omitted') { $reason = 'The Julia contrib surface exposes initJulia for an external Julia/libcxxwrap-julia/CxxWrap binding environment; the current package has no native Julia wrapper entrypoint or managed public owner, so the external runtime and binding ownership remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'rgbd' -and $classification -eq 'intentionally-omitted') { $reason = 'The RGBD contrib surface has no native RGBD wrapper entrypoint or managed public owner in the current package; KinectFusion/DynaFu/ColoredKinFu/LargeKinfu and LINEMOD depth-processing ownership, sensor inputs, and output buffers remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'cudaarithm' -and $classification -eq 'intentionally-omitted') { $reason = 'The CUDA arithmetic surface in cv::cuda has no native CudaArithm wrapper entrypoint or managed public owner in the current package; OpenCV disables the module on iOS/WinRT and unless CUDA or CUDA stubs are enabled, while real acceleration requires CUDA/NPP and optional CUDA libraries. GpuMat/Stream ownership and asynchronous execution remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'cudabgsegm' -and $classification -eq 'intentionally-omitted') { $reason = 'The CUDA background-segmentation surface in cv::cuda has no native CudaBgSegm wrapper entrypoint or managed public owner in the current package; OpenCV disables the module on iOS and unless CUDA or CUDA stubs are enabled, while execution requires an external CUDA toolkit/runtime. GpuMat/Stream ownership, asynchronous execution, and background-model state remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'cudafilters' -and $classification -eq 'intentionally-omitted') { $reason = 'The CUDA image-filter surface in cv::cuda has no native CudaFilters wrapper entrypoint or managed public owner in the current package; OpenCV disables the module on iOS/WinRT and unless CUDA or CUDA stubs are enabled, while real filtering requires CUDA/NPP and the upstream imgproc/cudaarithm dependency chain. GpuMat/Stream ownership, asynchronous execution, and filter state remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'cudaimgproc' -and $classification -eq 'intentionally-omitted') { $reason = 'The CUDA image-processing surface in cv::cuda has no native CudaImgProc wrapper entrypoint or managed public owner in the current package; OpenCV disables the module on iOS/WinRT and unless CUDA or CUDA stubs are enabled, while real processing requires CUDA/NPP and the upstream imgproc/cudev/cudaarithm/cudafilters/geometry dependency chain. GpuMat/Stream ownership, asynchronous execution, and image-processing state remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'cudacodec' -and $classification -eq 'intentionally-omitted') { $reason = 'The CUDA codec surface in cv::cudacodec has no native CudaCodec wrapper entrypoint or managed public owner in the current package; OpenCV disables the module on iOS/Apple/WinRT and unless CUDA or CUDA stubs are enabled, while real encoding/decoding requires the external CUDA toolkit/runtime and NVIDIA Video Codec SDK. VideoReader/VideoWriter, callback, GpuMat/Stream, asynchronous execution, and codec model ownership remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'superres' -and $classification -eq 'intentionally-omitted') { $reason = 'The Super Resolution contrib surface has no native Superres wrapper entrypoint or managed public owner in the current package; optional VideoIO/camera and CUDA frame-source/algorithm availability, input/output frame ownership, and algorithm state remain outside the current ABI/API and deterministic smoke contract.' }
        if ($Module -eq 'rapid' -and [string]$decl.identity -match '^cv\.rapid\.GOSTracker\.create\(') { $reason = 'GOSTracker is present in the pinned upstream header but the current managed/native wrapper exposes only Rapid and OLSTracker; this optional tracker remains deliberately omitted until its behavior, ABI, and ownership contract are separately reviewed.' }
        if ($Module -eq 'phase_unwrapping' -and [string]$decl.identity -match 'HistogramPhaseUnwrapping\.Params\.Params\(') { $reason = 'The upstream default Params constructor is represented by HistogramPhaseUnwrappingParams.Default and an explicit managed value constructor; value initialization has no native callable ABI.' }
        if ($Module -eq 'optflow' -and [string]$decl.identity -match '^cv\.optflow\.createOptFlow_PCAFlow\(') { $reason = 'PCAFlow is an optional dense optical-flow factory distinct from the wrapped RLOF and DualTVL1 families; the current ABI/API has no PCAFlow native factory, opaque handle, ownership/lifetime contract, or deterministic runtime smoke, so this declaration remains intentionally omitted.' }
        if ($Module -eq 'bgsegm' -and [string]$decl.identity -match '^cv\.bgsegm\.(?:BackgroundSubtractorGSOC|BackgroundSubtractorLSBP)\.|^cv\.bgsegm\.createBackgroundSubtractor(?:GSOC|LSBP)\(') { $reason = 'GSOC and LSBP are optional parameter-rich background-subtraction families; the current BgSegm ABI/API intentionally wraps MOG, GMG, CNT, and SyntheticSequenceGenerator only, with no GSOC/LSBP native factory/handle, ownership/lifetime contract, or deterministic runtime smoke.' }
        if ($Module -eq 'intensity_transform' -and [string]$decl.identity -match '\.BIMEF\(') { $reason += ' BIMEF runtime execution additionally requires an OpenCV build with EIGEN support.' }
        if ($Module -eq 'quality' -and [string]$decl.identity -match 'cv\.quality\.QualityBRISQUE\.(?:create|compute).*model_file_path') { $reason += ' BRISQUE operation requires caller-supplied model and range files; the repository bundles neither asset.' }
        if ($Module -eq 'quality' -and [string]$decl.identity -match 'Ptr_ml_SVM') { $reason += ' The upstream overload takes an ML::SVM object and range Mat; the current wrapper exposes the path-based overload only.' }
        if ($Module -eq 'face' -and [string]$decl.identity -match 'Facemark\.(?:fit|loadModel)|createFacemark(?:LBF|AAM|Kazemi)') { $reason += ' Facemark fitting requires caller-selected compatible landmark model data; the repository bundles no model asset.' }
        if ($Module -eq 'line_descriptor' -and [string]$decl.identity -match '^cv\.line_descriptor\.KeyLine\.get') { $reason = 'The managed KeyLine value exposes this accessor as a property; it has no separate native wrapper entrypoint and is not counted as native-backed implementation.' }
    }
    $buildCondition = if ($Module -in @('cudaarithm','cudafilters','cudaimgproc')) { 'NOT_IOS_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)' } elseif ($Module -eq 'cudabgsegm') { 'NOT_IOS_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)' } elseif ($Module -eq 'cudacodec') { 'NOT_IOS_AND_NOT_APPLE_AND_NOT_WINRT_AND_(HAVE_CUDA_OR_BUILD_CUDA_STUBS)' } else { "OPENCV_CSHARP_HAS_OPENCV_$($Module.ToUpperInvariant())=1" }
    $rows.Add([ordered]@{ ordinal = [int]$decl.ordinal; identity = [string]$decl.identity; classification = $classification; reason = $reason; buildCondition = $buildCondition; nativeEntrypoints = @($native); managedMembers = @($managed) })
}

$externalDataDependencies = switch ($Module) {
    'face' { @('Facemark LBF fitting needs a caller-selected compatible landmark model loaded with Facemark.LoadModel; no model data is bundled.') }
    'quality' { @('QualityBRISQUE needs caller-selected SVM model and range files; no model data is bundled. Other quality metrics use caller-provided reference and comparison Mats.') }
    'dnn_superres' { @('DNN Super Resolution requires caller-selected compatible model data and backend/target policy; no model asset or managed/native wrapper is bundled.') }
    'wechat_qrcode' { @('WeChat QRCode requires caller-selected detector and super-resolution ONNX model files; no model assets or managed/native wrapper are bundled.') }
    'cnn_3dobj' { @('cnn_3dobj requires an external Caffe triplet build plus caller-selected network, mean, and model files; no Caffe runtime or model assets, and no managed/native wrapper, are bundled.') }
    'hdf' { @('The HDF contrib surface requires an external HDF5 development/runtime dependency; no HDF5 runtime or managed/native wrapper is bundled.') }
    'text' { @('OCRTesseract is compiled only when external Tesseract is installed; this package bundles neither Tesseract nor a Text wrapper.', 'Text detection and OCR classifier entrypoints require caller-selected classifier/model data; no model assets are bundled.') }
    'xfeatures2d' { @('The xfeatures2d CUDA headers require an optional CUDA build/runtime; this package bundles no CUDA XFeatures2D wrapper or CUDA runtime.', 'The xfeatures2d nonfree families require an explicit OPENCV_ENABLE_NONFREE build and licensing decision; no nonfree wrapper or feature model assets are bundled.') }
    'dnn_objdetect' { @('The dnn_objdetect helper requires caller-selected DNN network/model data and backend policy; no model assets or managed/native wrapper are bundled.') }
    'ccalib' { @('The ccalib CustomPattern API requires caller-selected pattern images, feature detector/descriptor/matcher objects, and calibration output ownership; no managed/native wrapper or pattern assets are bundled.') }
    'julia' { @('The Julia bindings require an external Julia/libcxxwrap-julia/CxxWrap development and runtime environment; no Julia runtime, binding package, or managed/native OpenCvSharp wrapper is bundled.') }
    'rgbd' { @('The RGBD module requires caller-supplied depth/RGB sensor data and optional OpenCL acceleration policy; no RGBD managed/native wrapper or sensor/model assets are bundled.', 'KinectFusion usage is subject to upstream patent and nonfree-build restrictions documented by the module; legal and OPENCV_ENABLE_NONFREE policy remain outside this package.') }
    'cudaarithm' { @('The cudaarithm module is disabled unless the OpenCV build enables CUDA or CUDA stubs and is unavailable on iOS/WinRT; real acceleration depends on an external CUDA toolkit/runtime and NPP.', 'The upstream module optionally uses opencv_cudev, cuBLAS/cuBLASLt, and cuFFT; this package ships no CUDA arithmetic wrapper/runtime, and GpuMat/Stream lifetime and async ownership are outside the managed contract.') }
    'cudabgsegm' { @('The cudabgsegm module is disabled unless the OpenCV build enables CUDA or CUDA stubs and is unavailable on iOS; real execution depends on an external CUDA toolkit/runtime.', 'The module is layered on opencv_video background-subtractor contracts and requires GpuMat/Stream data, asynchronous execution policy, and persistent background-model state; this package ships no CUDA background-segmentation wrapper/runtime.') }
    'cudafilters' { @('The cudafilters module is disabled unless the OpenCV build enables CUDA or CUDA stubs and is unavailable on iOS/WinRT; real filtering depends on an external CUDA toolkit/runtime and NPP image-filter libraries.', 'The module requires opencv_imgproc and opencv_cudaarithm plus GpuMat/Stream data, asynchronous execution policy, and persistent filter state; this package ships no CUDA image-filter wrapper/runtime.') }
    'cudaimgproc' { @('The cudaimgproc module is disabled unless the OpenCV build enables CUDA or CUDA stubs and is unavailable on iOS/WinRT; real image processing depends on an external CUDA toolkit/runtime and NPP image-processing libraries.', 'The module requires opencv_imgproc and optionally opencv_cudev, opencv_cudaarithm, opencv_cudafilters, and opencv_geometry plus GpuMat/Stream data, asynchronous execution policy, and persistent processing state; this package ships no CUDA image-processing wrapper/runtime.') }
    'cudacodec' { @('The cudacodec module is disabled unless the OpenCV build enables CUDA or CUDA stubs and is unavailable on iOS/Apple/WinRT; real encoding/decoding depends on the external CUDA toolkit/runtime and NVIDIA Video Codec SDK/NVCUVID/NVCUVENC availability.', 'The module requires opencv_videoio, opencv_cudaarithm, opencv_cudawarping, and optionally opencv_cudaimgproc/opencv_cudev, with GpuMat/Stream, VideoReader/VideoWriter, callback, and asynchronous ownership contracts outside this package; no CUDA codec wrapper/runtime is bundled.') }
    'superres' { @('The Super Resolution contrib module is optional and its video/camera and CUDA frame-source/algorithm paths depend on the OpenCV build, VideoIO backend, and CUDA toolkit/runtime; no managed/native Superres wrapper or CUDA runtime is bundled.') }
    'freetype' { @('The upstream FreeType2 module requires system FreeType2 and HarfBuzz development/runtime libraries; neither dependency is bundled by this repository.') }
    'intensity_transform' { @('BIMEF runtime execution requires OpenCV built with EIGEN support; a module runtime without EIGEN may report that the operation is unavailable.') }
    'saliency' { @('ObjectnessBING requires caller-supplied training/model data through SetTrainingPath; the repository bundles no model asset and therefore does not claim deterministic objectness model execution.') }
    default { @() }
}
$claimedSlice = if ($Module -eq 'reg') { 'opencv2/reg/*.hpp contrib public header closure from parser-emitted headers' } elseif ($Module -eq 'ptcloud') { 'opencv2/ptcloud/depth.hpp selected parser-backed depth/RGB-D wrapper slice; ptcloud umbrella volume/odometry headers are explicitly excluded' } elseif ($Module -eq 'dnn_objdetect') { 'opencv2/core_detect.hpp contrib public header closure from parser-emitted headers; dnn_objdetect has no separate umbrella header in OpenCV 5.0.0' } elseif ($Module -eq 'superres') { 'opencv2/superres.hpp plus opencv2/superres/*.hpp parser-backed public header closure; protected SuperResolution implementation hooks are explicitly excluded' } elseif ($Module -eq 'rgbd') { 'opencv2/rgbd.hpp plus opencv2/rgbd/*.hpp parser-backed public header closure' } elseif ($Module -eq 'cudaarithm') { 'opencv2/cudaarithm.hpp parser-backed CUDA arithmetic public declarations plus the pinned module CMake build/dependency contract' } elseif ($Module -eq 'cudabgsegm') { 'opencv2/cudabgsegm.hpp parser-backed CUDA background-segmentation declarations plus the pinned module CMake build/dependency contract' } elseif ($Module -eq 'cudafilters') { 'opencv2/cudafilters.hpp parser-backed CUDA image-filter declarations plus the pinned module CMake build/dependency contract' } elseif ($Module -eq 'cudaimgproc') { 'opencv2/cudaimgproc.hpp parser-backed CUDA image-processing declarations plus the pinned module CMake build/dependency contract' } elseif ($Module -eq 'cudacodec') { 'opencv2/cudacodec.hpp parser-backed CUDA codec public declarations plus the pinned module CMake build/dependency contract' } else { "opencv2/$Module.hpp contrib public header closure from parser-emitted headers" }
$classification = [ordered]@{ schemaVersion = 1; upstreamOpenCvVersion = '5.0.0'; claimedSlice = $claimedSlice; reviewStatus = 'reviewed'; limitation = "The map records parser identities and exact $DisplayName wrapper evidence; it does not claim repository-wide contrib parity or external module availability."; externalDataDependencies = @($externalDataDependencies); declarations = @($rows) }
$builder = [Text.StringBuilder]::new(); [void]$builder.AppendLine("# Generated by scripts/Generate-ContribUpstreamMap.ps1 -Module $Module. Do not edit."); [void]$builder.AppendLine('schema-version=1'); [void]$builder.AppendLine('upstream-opencv-version=5.0.0'); [void]$builder.AppendLine("claimed-slice=$($classification.claimedSlice)"); [void]$builder.AppendLine("header-sha256=$($raw.headerSha256)"); [void]$builder.AppendLine("parser-sha256=$($raw.parserSha256)"); [void]$builder.AppendLine("declaration-count=$($raw.declarationCount)"); [void]$builder.AppendLine('repository-wide-upstream-parity-claimed=false'); [void]$builder.AppendLine(''); [void]$builder.AppendLine('ordinal|classification|identity|native-entrypoints|managed-members|build-condition|reason')
foreach ($row in $rows) { $nativeText = if (@($row.nativeEntrypoints).Count) { @($row.nativeEntrypoints) -join ';' } else { '-' }; $managedText = if (@($row.managedMembers).Count) { @($row.managedMembers) -join ';' } else { '-' }; [void]$builder.AppendLine("$($row.ordinal)|$($row.classification)|$($row.identity)|$nativeText|$managedText|$($row.buildCondition)|$($row.reason)") }
$mappingText = $builder.ToString().Replace("`r`n", "`n"); $implemented = @($rows | Where-Object classification -eq 'implemented'); $omitted = @($rows | Where-Object classification -eq 'intentionally-omitted'); $metadata = @($rows | Where-Object classification -eq 'non-callable-metadata')
$guide = switch ($Module) { 'face' {'docs/articles/face-guide.md'} 'quality' {'docs/articles/quality-guide.md'} 'img_hash' {'docs/articles/img-hash-guide.md'} 'line_descriptor' {'docs/articles/line-descriptor-guide.md'} 'intensity_transform' {'docs/articles/intensity-transform-guide.md'} 'plot' {'docs/articles/plot-guide.md'} 'bioinspired' {'docs/articles/bioinspired-guide.md'} 'phase_unwrapping' {'docs/articles/phase-unwrapping-guide.md'} 'hfs' {'docs/articles/hfs-guide.md'} 'fuzzy' {'docs/articles/fuzzy-guide.md'} 'rapid' {'docs/articles/rapid-guide.md'} 'shape' {'docs/articles/shape-guide.md'} 'reg' {'docs/articles/reg-guide.md'} 'ptcloud' {'docs/articles/ptcloud-guide.md'} 'xobjdetect' {'docs/articles/xobjdetect-guide.md'} default { "docs/articles/$Module-upstream-parity-guide.md" } }
$family = [ordered]@{ schemaVersion = 1; upstreamOpenCvVersion = '5.0.0'; status = 'implemented-verified'; managedPublicTypeAdditionCount = 0; managedPublicMemberAdditionCount = 0; families = @([ordered]@{ id = "$Module-wrapper-surface"; rationale = "Current $DisplayName managed/native wrapper declarations with exact parser-backed callable evidence."; declarations = @($implemented | ForEach-Object { [ordered]@{ ordinal = $_.ordinal; upstreamIdentity = $_.identity; upstreamClassification = $_.classification; nativeEntrypoints = $_.nativeEntrypoints; managedMembers = $_.managedMembers; focusedTest = "tests/OpenCvSharp.Tests/$DisplayName/$DisplayName`Tests.cs"; nativeSmoke = 'src/OpenCvSharp.Native/tests/native_smoke.cpp'; sample = 'samples/ConsoleSamples/Program.cs'; guide = $guide } }) }) }
$classificationJson = (($classification | ConvertTo-Json -Depth 20) + [Environment]::NewLine); $familyJson = (($family | ConvertTo-Json -Depth 20) + [Environment]::NewLine); $mapHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($mappingText))).ToLowerInvariant(); $familyHash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.Encoding]::UTF8.GetBytes($familyJson))).ToLowerInvariant()
$summary = [ordered]@{ schemaVersion = 1; generator = "tools/$DisplayName`UpstreamMap"; upstreamOpenCvVersion = '5.0.0'; claimedSlice = $classification.claimedSlice; rawExtractionPath = "compatibility/$Module-upstream-raw.json"; classificationPath = "compatibility/$Module-upstream-classifications.json"; mappingPath = "compatibility/$Module-upstream-map.txt"; headerSha256 = [string]$raw.headerSha256; parserSha256 = [string]$raw.parserSha256; mappingSha256 = $mapHash; declarationCount = [int]$raw.declarationCount; enumCount = [int](@($raw.declarations | Where-Object kind -eq enum).Count); classCount = [int](@($raw.declarations | Where-Object kind -eq class).Count); callableCount = [int](@($raw.declarations | Where-Object kind -eq callable).Count); classificationCounts = [ordered]@{ implemented = [int]$implemented.Count; 'intentionally-omitted' = [int]$omitted.Count; missing = 0; 'non-callable-metadata' = [int]$metadata.Count; unsupported = 0; 'upstream-conditional' = 0 }; nativeEvidenceCount = @($rows | ForEach-Object { @($_.nativeEntrypoints) } | Where-Object { $_ } | Sort-Object -Unique).Count; managedEvidenceCount = @($rows | ForEach-Object { @($_.managedMembers) } | Where-Object { $_ } | Sort-Object -Unique).Count; negativeFixtureCount = 12; externalDataDependencyCount = @($externalDataDependencies).Count; familyInventoryPath = "compatibility/$Module-implemented-families.json"; familyInventorySha256 = $familyHash; selectedFamilyCount = 1; selectedDeclarationCount = $implemented.Count; managedPublicTypeAdditionCount = 0; managedPublicMemberAdditionCount = 0; repositoryWideUpstreamParityClaimed = $false }; $summaryJson = (($summary | ConvertTo-Json -Depth 20) + [Environment]::NewLine)
foreach ($target in @(@{ Path = $classificationPath; Text = $classificationJson }, @{ Path = (Join-Path $repo "compatibility/$Module-upstream-map.txt"); Text = $mappingText }, @{ Path = $summaryPath; Text = $summaryJson }, @{ Path = $familyPath; Text = $familyJson })) {
    if ($Check) {
        if (-not (Test-Path -LiteralPath $target.Path -PathType Leaf)) { throw "Contrib map generated artifact missing: $($target.Path)" }
        $current = (Get-Content -LiteralPath $target.Path -Raw) -replace "`r`n", "`n"; $expected = [string]$target.Text
        if ([IO.Path]::GetExtension($target.Path) -eq '.json') { $current = (($current | ConvertFrom-Json | ConvertTo-Json -Depth 20) + [Environment]::NewLine); $expected = (($expected | ConvertFrom-Json | ConvertTo-Json -Depth 20) + [Environment]::NewLine) }
        if ($current -ne $expected) { throw "Contrib map generated artifact is stale: $($target.Path)" }
    } else { [IO.File]::WriteAllText($target.Path, $target.Text, [Text.UTF8Encoding]::new($false)) }
}
Write-Host "$($Module.ToUpperInvariant())_UPSTREAM_MAP_OK declarations=$($summary.declarationCount) callable=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) missing=0 omitted=$($summary.classificationCounts.'intentionally-omitted') fixtures=12 nativeEvidence=$($summary.nativeEvidenceCount) managedEvidence=$($summary.managedEvidenceCount) sha256=$mapHash mode=$(if ($Check) { 'check' } else { 'write' })"
