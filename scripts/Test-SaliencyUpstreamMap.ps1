param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path

& (Join-Path $PSScriptRoot 'Generate-SaliencyUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary = Get-Content (Join-Path $repo 'compatibility/saliency-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/saliency-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/saliency-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managedBaseline = @(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt'))

if ([int]$raw.declarationCount -ne 39 -or [int]$summary.callableCount -ne 31 -or [int]$summary.classCount -ne 8 -or
    [int]$summary.classificationCounts.implemented -ne 29 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 2 -or
    [int]$summary.classificationCounts.missing -ne 0 -or [int]$summary.nativeEvidenceCount -ne 27 -or [int]$summary.managedEvidenceCount -ne 21) {
    throw 'Saliency upstream declaration partition drifted.'
}
if ([string]$raw.headerPath -cne 'opencv-source/opencv_contrib-5.0.0/modules/saliency/include/opencv2/saliency.hpp' -or
    @($raw.sourceHeaders).Count -ne 2 -or @($raw.sourceHeaders | Where-Object { [string]$_.path -notmatch '^opencv-source/opencv_contrib-5\.0\.0/modules/saliency/include/opencv2/saliency/' }).Count -ne 0) {
    throw 'Saliency map must cover only the pinned umbrella and parser-emitted saliency headers.'
}
if ([int]$summary.negativeFixtureCount -lt 12 -or [int]$summary.externalDataDependencyCount -ne 1) { throw 'Saliency evidence contract drifted.' }

$expected = [ordered]@{
    'cv.saliency.Saliency.computeSaliency(' = @('jyppx_ocv_saliency_compute_saliency')
    'cv.saliency.StaticSaliency.computeBinaryMap(' = @('jyppx_ocv_saliency_static_compute_binary_map')
    'cv.saliency.StaticSaliencySpectralResidual.create(' = @('jyppx_ocv_saliency_spectral_residual_create')
    'cv.saliency.StaticSaliencySpectralResidual.computeSaliency(' = @('jyppx_ocv_saliency_compute_saliency')
    'cv.saliency.StaticSaliencySpectralResidual.getImageWidth(' = @('jyppx_ocv_saliency_spectral_residual_get_image_width')
    'cv.saliency.StaticSaliencySpectralResidual.setImageWidth(' = @('jyppx_ocv_saliency_spectral_residual_set_image_width')
    'cv.saliency.StaticSaliencySpectralResidual.getImageHeight(' = @('jyppx_ocv_saliency_spectral_residual_get_image_height')
    'cv.saliency.StaticSaliencySpectralResidual.setImageHeight(' = @('jyppx_ocv_saliency_spectral_residual_set_image_height')
    'cv.saliency.StaticSaliencyFineGrained.create(' = @('jyppx_ocv_saliency_fine_grained_create')
    'cv.saliency.StaticSaliencyFineGrained.computeSaliency(' = @('jyppx_ocv_saliency_compute_saliency')
    'cv.saliency.MotionSaliencyBinWangApr2014.create(' = @('jyppx_ocv_saliency_motion_bin_wang_create')
    'cv.saliency.MotionSaliencyBinWangApr2014.computeSaliency(' = @('jyppx_ocv_saliency_compute_saliency')
    'cv.saliency.MotionSaliencyBinWangApr2014.setImagesize(' = @('jyppx_ocv_saliency_motion_bin_wang_set_image_size')
    'cv.saliency.MotionSaliencyBinWangApr2014.init(' = @('jyppx_ocv_saliency_motion_bin_wang_init')
    'cv.saliency.MotionSaliencyBinWangApr2014.getImageWidth(' = @('jyppx_ocv_saliency_motion_bin_wang_get_image_width')
    'cv.saliency.MotionSaliencyBinWangApr2014.setImageWidth(' = @('jyppx_ocv_saliency_motion_bin_wang_set_image_width')
    'cv.saliency.MotionSaliencyBinWangApr2014.getImageHeight(' = @('jyppx_ocv_saliency_motion_bin_wang_get_image_height')
    'cv.saliency.MotionSaliencyBinWangApr2014.setImageHeight(' = @('jyppx_ocv_saliency_motion_bin_wang_set_image_height')
    'cv.saliency.ObjectnessBING.create(' = @('jyppx_ocv_saliency_objectness_bing_create')
    'cv.saliency.ObjectnessBING.computeSaliency(' = @('jyppx_ocv_saliency_objectness_bing_compute')
    'cv.saliency.ObjectnessBING.getobjectnessValues(' = @('jyppx_ocv_saliency_objectness_bing_get_objectness_values_count','jyppx_ocv_saliency_objectness_bing_get_objectness_values_fill')
    'cv.saliency.ObjectnessBING.setTrainingPath(' = @('jyppx_ocv_saliency_objectness_bing_set_training_path')
    'cv.saliency.ObjectnessBING.setBBResDir(' = @('jyppx_ocv_saliency_objectness_bing_set_bb_res_dir')
    'cv.saliency.ObjectnessBING.getBase(' = @('jyppx_ocv_saliency_objectness_bing_get_base')
    'cv.saliency.ObjectnessBING.setBase(' = @('jyppx_ocv_saliency_objectness_bing_set_base')
    'cv.saliency.ObjectnessBING.getNSS(' = @('jyppx_ocv_saliency_objectness_bing_get_nss')
    'cv.saliency.ObjectnessBING.setNSS(' = @('jyppx_ocv_saliency_objectness_bing_set_nss')
    'cv.saliency.ObjectnessBING.getW(' = @('jyppx_ocv_saliency_objectness_bing_get_w')
    'cv.saliency.ObjectnessBING.setW(' = @('jyppx_ocv_saliency_objectness_bing_set_w')
}

$implemented = @($map.declarations | Where-Object { $_.classification -eq 'implemented' })
if ($implemented.Count -ne $expected.Count) { throw 'Saliency implemented callable coverage drifted.' }
foreach ($row in $implemented) {
    $identity = [string]$row.identity
    $key = @($expected.Keys | Where-Object { $identity.StartsWith($_, [StringComparison]::Ordinal) })
    if ($key.Count -ne 1) { throw "Unexpected implemented Saliency callable identity: $identity" }
    $symbols = @($expected[$key[0]])
    if ((@($row.nativeEntrypoints) -join '|') -cne ($symbols -join '|')) { throw "Saliency native mapping drifted: $identity" }
    foreach ($symbol in $symbols) { if ($manifest -notcontains $symbol) { throw "Saliency native symbol is absent from ABI manifest: $symbol" } }
    if (@($row.managedMembers).Count -eq 0 -or @($row.managedMembers | Where-Object { $managedBaseline -notcontains [string]$_ }).Count -ne 0) { throw "Saliency managed evidence drifted: $identity" }
}

$omitted = @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' })
if (@($omitted | Where-Object { @($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 }).Count -ne 0) { throw 'Saliency omissions must not retain partial bindings.' }
if (@($omitted | Where-Object { [string]$_.identity -notmatch 'StaticSaliencySpectralResidual\.(?:read|write)\(' }).Count -ne 0) { throw 'Saliency omission boundary drifted.' }
$metadata = @($map.declarations | Where-Object { $_.classification -eq 'non-callable-metadata' })
if ($metadata.Count -ne 8) { throw 'Saliency class metadata closure drifted.' }
$moduleSymbols = @($manifest | Where-Object { $_ -match '^jyppx_ocv_saliency_' })
$parserSymbols = @($implemented | ForEach-Object { @($_.nativeEntrypoints) } | Sort-Object -Unique)
if ($moduleSymbols.Count -ne 30 -or $parserSymbols.Count -ne 27 -or $moduleSymbols -notcontains 'jyppx_ocv_saliency_saliency_release_handle' -or $parserSymbols -contains 'jyppx_ocv_saliency_saliency_release_handle' -or $parserSymbols -contains 'jyppx_ocv_saliency_objectness_bing_get_boxes_count' -or $parserSymbols -contains 'jyppx_ocv_saliency_objectness_bing_get_boxes_fill') { throw 'Saliency ABI lifetime/helper boundary drifted.' }
Write-Host "SALIENCY_UPSTREAM_MAP_CONTRACT_OK declarations=39 callables=31 implemented=29 omitted=2 missing=0 metadata=8 native=27 managed=21 sha256=$($summary.mappingSha256)"
