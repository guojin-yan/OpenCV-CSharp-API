param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-IntensityTransformUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/intensity_transform-upstream-summary.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/intensity_transform-upstream-classifications.json') -Raw|ConvertFrom-Json
$raw=Get-Content (Join-Path $repo 'compatibility/intensity_transform-upstream-raw.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
$expected=@{
    logTransform='jyppx_ocv_intensity_transform_log'
    gammaCorrection='jyppx_ocv_intensity_transform_gamma_correction'
    autoscaling='jyppx_ocv_intensity_transform_autoscaling'
    contrastStretching='jyppx_ocv_intensity_transform_contrast_stretching'
}
if([int]$summary.declarationCount -ne 6 -or [int]$summary.callableCount -ne 6 -or [int]$summary.classificationCounts.implemented -ne 6 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 0 -or [int]$summary.classificationCounts.missing -ne 0){throw 'IntensityTransform upstream map partition drifted.'}
if(@($raw.sourceHeaders|Where-Object {$_.path -eq 'opencv-source/opencv_contrib-5.0.0/modules/intensity_transform/include/opencv2/intensity_transform.hpp'}).Count -ne 1){throw 'IntensityTransform map must cover the pinned public umbrella header.'}
foreach($name in $expected.Keys){
    $rows=@($map.declarations|Where-Object {$_.identity -match "\.${name}\("})
    if($rows.Count -ne 1 -or @($rows[0].nativeEntrypoints|Where-Object {$_ -eq $expected[$name]}).Count -ne 1 -or $manifest -notcontains $expected[$name]){throw "IntensityTransform declaration-to-native ABI mapping drifted: $name"}
    if(@($rows[0].managedMembers|Where-Object {$_ -match '\.IntensityTransformCv2\|method\|.*\b' + [regex]::Escape($name) + '\('}).Count -ne 2){throw "IntensityTransform managed output/return overload evidence drifted: $name"}
}
$bimef=@($map.declarations|Where-Object {$_.identity -match '\.BIMEF\('})
if($bimef.Count -ne 2){throw 'IntensityTransform BIMEF overload count drifted.'}
$automatic=@($bimef|Where-Object {$_.identity -notmatch '\bfloat k\b'})
$explicit=@($bimef|Where-Object {$_.identity -match '\bfloat k\b'})
if($automatic.Count -ne 1 -or $explicit.Count -ne 1 -or $automatic[0].nativeEntrypoints -notcontains 'jyppx_ocv_intensity_transform_bimef' -or $explicit[0].nativeEntrypoints -notcontains 'jyppx_ocv_intensity_transform_bimef_with_k' -or $automatic[0].managedMembers.Count -ne 2 -or $explicit[0].managedMembers.Count -ne 2){throw 'IntensityTransform BIMEF overload/native/managed mapping drifted.'}
if(@($summary.nativeEvidenceCount) -ne 6 -or @($summary.managedEvidenceCount) -ne 12){throw 'IntensityTransform evidence counts drifted.'}
if(@($map.externalDataDependencies|Where-Object {$_ -match 'BIMEF' -and $_ -match 'EIGEN'}).Count -ne 1){throw 'IntensityTransform EIGEN runtime boundary is not documented.'}
Write-Host "INTENSITY_TRANSFORM_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) omitted=0 missing=0 native=$($summary.nativeEvidenceCount) managed=$($summary.managedEvidenceCount) sha256=$($summary.mappingSha256)"
