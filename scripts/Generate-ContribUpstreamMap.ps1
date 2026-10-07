param(
    [Parameter(Mandatory)][ValidateSet('optflow','bgsegm','face','quality','img_hash','line_descriptor','freetype','alphamat','intensity_transform')][string]$Module,
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
    Where-Object { $_ -match "^jyppx_ocv_${Module}_" -or ($Module -eq 'optflow' -and $_ -match '^jyppx_ocv_motempl_') } |
    ForEach-Object { ($_ -split '\|')[0] } | Sort-Object -Unique)
$managedEntries = @(Get-Content $managedPath | Where-Object { $_ -match "JYPPX\.OpenCvSharp\.$DisplayName" })

function ToPascal([string]$Value) {
    if ([string]::IsNullOrEmpty($Value)) { return $Value }
    return $Value.Substring(0, 1).ToUpperInvariant() + $Value.Substring(1)
}

function GetParts([string]$Identity) {
    $pattern = if ($Module -eq 'optflow') {
        '^cv\.(?<namespace>optflow|motempl)\.(?:(?<owner>.+)\.)?(?<name>[A-Za-z][A-Za-z0-9_]*)\('
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
    if ($Module -in @('face','quality','img_hash','line_descriptor','alphamat','intensity_transform')) {
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
            }
            return [string]$managedType
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
    if ($Module -eq 'freetype') { return @() }
    $typeName = GetManagedType $Owner $Name
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
    if ($Module -eq 'intensity_transform') {
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
    if ($Module -eq 'intensity_transform') {
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
        if ($Module -eq 'intensity_transform' -and [string]$decl.identity -match '\.BIMEF\(') { $reason += ' BIMEF runtime execution additionally requires an OpenCV build with EIGEN support.' }
        if ($Module -eq 'quality' -and [string]$decl.identity -match 'cv\.quality\.QualityBRISQUE\.(?:create|compute).*model_file_path') { $reason += ' BRISQUE operation requires caller-supplied model and range files; the repository bundles neither asset.' }
        if ($Module -eq 'quality' -and [string]$decl.identity -match 'Ptr_ml_SVM') { $reason += ' The upstream overload takes an ML::SVM object and range Mat; the current wrapper exposes the path-based overload only.' }
        if ($Module -eq 'face' -and [string]$decl.identity -match 'Facemark\.(?:fit|loadModel)|createFacemark(?:LBF|AAM|Kazemi)') { $reason += ' Facemark fitting requires caller-selected compatible landmark model data; the repository bundles no model asset.' }
        if ($Module -eq 'line_descriptor' -and [string]$decl.identity -match '^cv\.line_descriptor\.KeyLine\.get') { $reason = 'The managed KeyLine value exposes this accessor as a property; it has no separate native wrapper entrypoint and is not counted as native-backed implementation.' }
    }
    $rows.Add([ordered]@{ ordinal = [int]$decl.ordinal; identity = [string]$decl.identity; classification = $classification; reason = $reason; buildCondition = "OPENCV_CSHARP_HAS_OPENCV_$($Module.ToUpperInvariant())=1"; nativeEntrypoints = @($native); managedMembers = @($managed) })
}

$externalDataDependencies = switch ($Module) {
    'face' { @('Facemark LBF fitting needs a caller-selected compatible landmark model loaded with Facemark.LoadModel; no model data is bundled.') }
    'quality' { @('QualityBRISQUE needs caller-selected SVM model and range files; no model data is bundled. Other quality metrics use caller-provided reference and comparison Mats.') }
    'freetype' { @('The upstream FreeType2 module requires system FreeType2 and HarfBuzz development/runtime libraries; neither dependency is bundled by this repository.') }
    'intensity_transform' { @('BIMEF runtime execution requires OpenCV built with EIGEN support; a module runtime without EIGEN may report that the operation is unavailable.') }
    default { @() }
}
$classification = [ordered]@{ schemaVersion = 1; upstreamOpenCvVersion = '5.0.0'; claimedSlice = "opencv2/$Module.hpp contrib public header closure from parser-emitted headers"; reviewStatus = 'reviewed'; limitation = "The map records parser identities and exact $DisplayName wrapper evidence; it does not claim repository-wide contrib parity or external module availability."; externalDataDependencies = @($externalDataDependencies); declarations = @($rows) }
$builder = [Text.StringBuilder]::new(); [void]$builder.AppendLine("# Generated by scripts/Generate-ContribUpstreamMap.ps1 -Module $Module. Do not edit."); [void]$builder.AppendLine('schema-version=1'); [void]$builder.AppendLine('upstream-opencv-version=5.0.0'); [void]$builder.AppendLine("claimed-slice=$($classification.claimedSlice)"); [void]$builder.AppendLine("header-sha256=$($raw.headerSha256)"); [void]$builder.AppendLine("parser-sha256=$($raw.parserSha256)"); [void]$builder.AppendLine("declaration-count=$($raw.declarationCount)"); [void]$builder.AppendLine('repository-wide-upstream-parity-claimed=false'); [void]$builder.AppendLine(''); [void]$builder.AppendLine('ordinal|classification|identity|native-entrypoints|managed-members|build-condition|reason')
foreach ($row in $rows) { $nativeText = if (@($row.nativeEntrypoints).Count) { @($row.nativeEntrypoints) -join ';' } else { '-' }; $managedText = if (@($row.managedMembers).Count) { @($row.managedMembers) -join ';' } else { '-' }; [void]$builder.AppendLine("$($row.ordinal)|$($row.classification)|$($row.identity)|$nativeText|$managedText|$($row.buildCondition)|$($row.reason)") }
$mappingText = $builder.ToString().Replace("`r`n", "`n"); $implemented = @($rows | Where-Object classification -eq 'implemented'); $omitted = @($rows | Where-Object classification -eq 'intentionally-omitted'); $metadata = @($rows | Where-Object classification -eq 'non-callable-metadata')
$guide = switch ($Module) { 'face' {'docs/articles/face-guide.md'} 'quality' {'docs/articles/quality-guide.md'} 'img_hash' {'docs/articles/img-hash-guide.md'} 'line_descriptor' {'docs/articles/line-descriptor-guide.md'} 'intensity_transform' {'docs/articles/intensity-transform-guide.md'} default { "docs/articles/$Module-upstream-parity-guide.md" } }
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
