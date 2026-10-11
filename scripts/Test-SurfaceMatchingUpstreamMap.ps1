param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-SurfaceMatchingUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary = Get-Content (Join-Path $repo 'compatibility/surface_matching-upstream-summary.json') -Raw | ConvertFrom-Json
$raw = Get-Content (Join-Path $repo 'compatibility/surface_matching-upstream-raw.json') -Raw | ConvertFrom-Json
$map = Get-Content (Join-Path $repo 'compatibility/surface_matching-upstream-classifications.json') -Raw | ConvertFrom-Json
$manifest = @(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt') | ForEach-Object { ($_ -split '\|')[0] })
$managedBaseline = @(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt'))
$sourceCounts = @($raw.sourceHeaders | ForEach-Object { [int]$_.declarationCount }) -join '|'
if ([string]$raw.headerPath -cne 'opencv-source/opencv_contrib-5.0.0/modules/surface_matching/include/opencv2/surface_matching.hpp' -or @($raw.sourceHeaders).Count -ne 5 -or $sourceCounts -cne '5|9|8|5|0') { throw 'SurfaceMatching parser source closure drifted.' }
if ([int]$raw.declarationCount -ne 27 -or [int]$summary.callableCount -ne 23 -or [int]$summary.classCount -ne 4 -or [int]$summary.classificationCounts.implemented -ne 7 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 16 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 4 -or [int]$summary.classificationCounts.missing -ne 0) { throw 'SurfaceMatching declaration partition drifted.' }
$expectedImplemented = [ordered]@{
    'cv.ppf_match_3d.ICP.ICP(' = @('jyppx_ocv_surface_matching_icp_create','Icp','Create\(')
    'cv.ppf_match_3d.ICP.registerModelToScene(' = @('jyppx_ocv_surface_matching_icp_register_model_to_scene','Icp','RegisterModelToScene\(')
    'cv.ppf_match_3d.PPF3DDetector.PPF3DDetector(' = @('jyppx_ocv_surface_matching_ppf_3d_detector_create','Ppf3DDetector','Create\(')
    'cv.ppf_match_3d.PPF3DDetector.trainModel(' = @('jyppx_ocv_surface_matching_ppf_3d_detector_train_model','Ppf3DDetector','TrainModel\(')
    'cv.ppf_match_3d.PPF3DDetector.match(' = @('jyppx_ocv_surface_matching_ppf_3d_detector_match_count|jyppx_ocv_surface_matching_ppf_3d_detector_match_fill','Ppf3DDetector','Match\(')
}
$callables = @($map.declarations | Where-Object { $_.classification -eq 'implemented' })
if ($callables.Count -ne 7) { throw 'SurfaceMatching implemented callable count drifted.' }
foreach ($row in $callables) {
    $identity = [string]$row.identity
    $keys = @($expectedImplemented.Keys | Where-Object { $identity.StartsWith($_, [StringComparison]::Ordinal) })
    if ($keys.Count -ne 1) { throw "Unexpected SurfaceMatching implemented identity: $identity" }
    $spec = $expectedImplemented[$keys[0]]
    $expectedNative = @([string]$spec[0] -split '\|')
    if (@($row.nativeEntrypoints).Count -ne $expectedNative.Count -or (@($row.nativeEntrypoints | Sort-Object) -join '|') -cne (@($expectedNative | Sort-Object) -join '|') -or @($row.nativeEntrypoints | Where-Object { $manifest -notcontains [string]$_ }).Count -ne 0) { throw "SurfaceMatching native mapping drifted: $identity" }
    if (@($row.managedMembers).Count -ne 1 -or @($row.managedMembers | Where-Object { $managedBaseline -notcontains [string]$_ }).Count -ne 0 -or @($row.managedMembers | Where-Object { [string]$_ -match ('SurfaceMatching\.' + [regex]::Escape([string]$spec[1]) + '\|(?:method|property)\|.*' + [string]$spec[2]) }).Count -ne 1) { throw "SurfaceMatching managed mapping drifted: $identity" }
}
$omitted = @($map.declarations | Where-Object { $_.classification -eq 'intentionally-omitted' })
if (@($omitted | Where-Object { @($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0 }).Count -ne 0) { throw 'SurfaceMatching intentional omissions must not retain partial evidence.' }
if (@($omitted | Where-Object { [string]$_.identity -notmatch '^cv\.ppf_match_3d\.(?:ICP\.registerModelToScene|Pose3D\.|loadPLYSimple|writePLY|writePLYVisibleNormals|samplePCByQuantization|transformPCPose|getRandomPose|addNoisePC|computeNormalsPC3d)' }).Count -ne 0) { throw 'SurfaceMatching omission boundary drifted.' }
if (@($manifest | Where-Object { $_ -match '^jyppx_ocv_surface_matching_' }).Count -ne 9 -or [int]$summary.nativeEvidenceCount -ne 6 -or [int]$summary.managedEvidenceCount -ne 5) { throw 'SurfaceMatching ABI evidence counts drifted.' }
Write-Host "SURFACE_MATCHING_UPSTREAM_MAP_CONTRACT_OK declarations=27 callables=23 implemented=7 omitted=16 missing=0 metadata=4 native=6 managed=5 sha256=$($summary.mappingSha256)"
