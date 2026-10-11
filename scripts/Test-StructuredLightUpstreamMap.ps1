param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-StructuredLightUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary = Get-Content (Join-Path $repo 'compatibility/structured_light-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/structured_light-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/structured_light-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managedBaseline = @(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt'))
$expected = [ordered]@{
    'cv.structured_light.GrayCodePattern.create(' = @('jyppx_ocv_structured_light_gray_code_pattern_create','GrayCodePattern','Create\(System.Int32 width,System.Int32 height\)')
    'cv.structured_light.GrayCodePattern.getNumberOfPatternImages(' = @('jyppx_ocv_structured_light_gray_code_pattern_get_number_of_pattern_images','GrayCodePattern','NumberOfPatternImages$')
    'cv.structured_light.GrayCodePattern.setWhiteThreshold(' = @('jyppx_ocv_structured_light_gray_code_pattern_set_white_threshold','GrayCodePattern','SetWhiteThreshold\(')
    'cv.structured_light.GrayCodePattern.setBlackThreshold(' = @('jyppx_ocv_structured_light_gray_code_pattern_set_black_threshold','GrayCodePattern','SetBlackThreshold\(')
    'cv.structured_light.GrayCodePattern.getImagesForShadowMasks(' = @('jyppx_ocv_structured_light_gray_code_pattern_get_images_for_shadow_masks','GrayCodePattern','GetImagesForShadowMasks\(JYPPX\.OpenCvSharp\.Core\.Mat blackImage,JYPPX\.OpenCvSharp\.Core\.Mat whiteImage\)')
    'cv.structured_light.GrayCodePattern.getProjPixel(' = @('jyppx_ocv_structured_light_gray_code_pattern_get_proj_pixel','GrayCodePattern','GetProjPixel\(')
    'cv.structured_light.SinusoidalPattern.create(' = @('jyppx_ocv_structured_light_sinusoidal_pattern_create','SinusoidalPattern','Create\(')
    'cv.structured_light.SinusoidalPattern.computePhaseMap(' = @('jyppx_ocv_structured_light_sinusoidal_pattern_compute_phase_map','SinusoidalPattern','ComputePhaseMap\(')
    'cv.structured_light.SinusoidalPattern.unwrapPhaseMap(' = @('jyppx_ocv_structured_light_sinusoidal_pattern_unwrap_phase_map','SinusoidalPattern','UnwrapPhaseMap\(')
    'cv.structured_light.SinusoidalPattern.computeDataModulationTerm(' = @('jyppx_ocv_structured_light_sinusoidal_pattern_compute_data_modulation_term','SinusoidalPattern','ComputeDataModulationTerm\(')
    'cv.structured_light.StructuredLightPattern.generate(' = @('jyppx_ocv_structured_light_pattern_generate_count','StructuredLightPattern','Generate\(')
}
$callables = @($map.declarations | Where-Object { $_.classification -eq 'implemented' })
if ([string]$raw.headerPath -cne 'opencv-source/opencv_contrib-5.0.0/modules/structured_light/include/opencv2/structured_light.hpp' -or @($raw.sourceHeaders).Count -ne 3 -or (@($raw.sourceHeaders | ForEach-Object { [int]$_.declarationCount }) -join '|') -cne '7|9|4') { throw 'StructuredLight parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 20 -or [int]$summary.callableCount -ne 14 -or $callables.Count -ne 11 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 3 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 6 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'StructuredLight declaration partition drifted.' }
if ($expected.Count -ne $callables.Count) { throw 'StructuredLight implemented-callable expectation set drifted.' }
foreach ($row in $callables) {
    $identity = [string]$row.identity
    $keys = @($expected.Keys | Where-Object { $identity.StartsWith($_, [StringComparison]::Ordinal) })
    if ($keys.Count -ne 1) { throw "Unexpected StructuredLight callable identity: $identity" }
    $spec = $expected[$keys[0]]
    $expectedNative = if ($identity.StartsWith('cv.structured_light.StructuredLightPattern.generate(', [StringComparison]::Ordinal)) { @('jyppx_ocv_structured_light_pattern_generate_count','jyppx_ocv_structured_light_pattern_generate_fill') } else { @([string]$spec[0]) }
    if (@($row.nativeEntrypoints).Count -ne @($expectedNative).Count -or (@($row.nativeEntrypoints | Sort-Object) -join '|') -cne (@($expectedNative | Sort-Object) -join '|') -or @($row.nativeEntrypoints | Where-Object { $manifest -notcontains [string]$_ }).Count -ne 0) { throw "StructuredLight native mapping drifted: $identity" }
    if (@($row.managedMembers).Count -ne 1 -or @($row.managedMembers | Where-Object { $managedBaseline -notcontains [string]$_ }).Count -ne 0) { throw "StructuredLight managed mapping drifted: $identity" }
    if (@($row.managedMembers | Where-Object { [string]$_ -match ('StructuredLight\.' + [regex]::Escape([string]$spec[1]) + '\|(?:method|property)\|.*' + [string]$spec[2]) }).Count -ne 1) { throw "StructuredLight member owner/name drifted: $identity" }
}
$omitted = @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' })
if (@($omitted | Where-Object { @($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 }).Count -ne 0) { throw 'StructuredLight intentional omissions must not retain partial evidence.' }
if (@($omitted | Where-Object { [string]$_.identity -notmatch '^cv\.structured_light\.(?:SinusoidalPattern\.Params\.Params|SinusoidalPattern\.findProCamMatches|StructuredLightPattern\.decode)\(' }).Count -ne 0 -or @($omitted | Where-Object { [string]$_.identity -match 'SinusoidalPattern\.Params\.Params' }).Count -ne 1 -or @($omitted | Where-Object { [string]$_.identity -match 'SinusoidalPattern\.findProCamMatches' }).Count -ne 1 -or @($omitted | Where-Object { [string]$_.identity -match 'StructuredLightPattern\.decode' }).Count -ne 1) { throw 'StructuredLight omission boundary drifted.' }
if (@($map.declarations | Where-Object { $_.classification -eq 'non-callable-metadata' -and [string]$_.identity -match '^enum cv\.structured_light\.<unnamed>' }).Count -ne 2) { throw 'StructuredLight enum metadata closure drifted.' }
foreach ($enumValue in @('Ftp=0','Psp=1','Faps=2')) { if (@($managedBaseline | Where-Object { $_ -match ('SinusoidalPatternMethod ' + [regex]::Escape($enumValue) + '$') }).Count -ne 1) { throw "StructuredLight managed enum value is missing or duplicated: $enumValue" } }
if (@($manifest | Where-Object { $_ -match '^jyppx_ocv_structured_light_' }).Count -ne 13 -or [int]$summary.nativeEvidenceCount -ne 12 -or [int]$summary.managedEvidenceCount -ne 11) { throw 'StructuredLight ABI evidence counts drifted.' }
Write-Host "STRUCTURED_LIGHT_UPSTREAM_MAP_CONTRACT_OK declarations=20 callables=14 implemented=11 omitted=3 missing=0 metadata=6 native=12 managed=11 sha256=$($summary.mappingSha256)"
