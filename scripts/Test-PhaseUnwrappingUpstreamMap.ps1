param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-PhaseUnwrappingUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/phase_unwrapping-upstream-summary.json') -Raw|ConvertFrom-Json
$raw=Get-Content (Join-Path $repo 'compatibility/phase_unwrapping-upstream-raw.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/phase_unwrapping-upstream-classifications.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
$managedBaseline=@(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt'))
$expectedHeaders=@('opencv-source/opencv_contrib-5.0.0/modules/phase_unwrapping/include/opencv2/phase_unwrapping/histogramphaseunwrapping.hpp','opencv-source/opencv_contrib-5.0.0/modules/phase_unwrapping/include/opencv2/phase_unwrapping/phase_unwrapping.hpp')
if([int]$summary.declarationCount -ne 7 -or [int]$summary.callableCount -ne 4 -or [int]$summary.classificationCounts.implemented -ne 3 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 1 -or [int]$summary.classificationCounts.missing -ne 0){throw 'PhaseUnwrapping upstream map partition drifted.'}
if(@($raw.sourceHeaders).Count -ne $expectedHeaders.Count){throw 'PhaseUnwrapping parser source-header closure count drifted.'}
foreach($header in $expectedHeaders){if(@($raw.sourceHeaders|Where-Object {$_.path -ceq $header}).Count -ne 1){throw "PhaseUnwrapping map source-header closure drifted: $header"}}
$omitted=@($map.declarations|Where-Object {$_.identity -like 'cv.phase_unwrapping.HistogramPhaseUnwrapping.Params.Params(*'})
if($omitted.Count -ne 1 -or $omitted[0].classification -cne 'intentionally-omitted' -or @($omitted[0].nativeEntrypoints).Count -ne 0 -or @($omitted[0].managedMembers).Count -ne 0 -or $omitted[0].reason -notmatch 'HistogramPhaseUnwrappingParams\.Default'){throw 'PhaseUnwrapping Params value-initialization boundary drifted.'}
$expected=@{
    'cv.phase_unwrapping.HistogramPhaseUnwrapping.create('=@('jyppx_ocv_phase_unwrapping_histogram_create','JYPPX\.OpenCvSharp\.PhaseUnwrapping\.HistogramPhaseUnwrapping\|method\|[^|]*\|.*\bCreate\(JYPPX\.OpenCvSharp\.PhaseUnwrapping\.HistogramPhaseUnwrappingParams parameters\)')
    'cv.phase_unwrapping.HistogramPhaseUnwrapping.getInverseReliabilityMap('=@('jyppx_ocv_phase_unwrapping_histogram_get_inverse_reliability_map','JYPPX\.OpenCvSharp\.PhaseUnwrapping\.HistogramPhaseUnwrapping\|method\|[^|]*\|.*\bGetInverseReliabilityMap\(JYPPX\.OpenCvSharp\.Core\.Mat reliabilityMap\)')
    'cv.phase_unwrapping.PhaseUnwrapping.unwrapPhaseMap('=@('jyppx_ocv_phase_unwrapping_unwrap_phase_map','JYPPX\.OpenCvSharp\.PhaseUnwrapping\.PhaseUnwrappingObject\|method\|[^|]*\|.*\bUnwrapPhaseMap\(JYPPX\.OpenCvSharp\.Core\.Mat wrappedPhaseMap,JYPPX\.OpenCvSharp\.Core\.Mat unwrappedPhaseMap,')
}
$callables=@($map.declarations|Where-Object {$_.classification -eq 'implemented'})
if($callables.Count -ne 3){throw 'PhaseUnwrapping implemented callable count drifted.'}
foreach($row in $callables){
    $identity=[string]$row.identity
    $key=@($expected.Keys|Where-Object {$identity.StartsWith($_,[StringComparison]::Ordinal)})
    if($key.Count -ne 1){throw "Unexpected PhaseUnwrapping callable identity: $identity"}
    $spec=$expected[$key[0]];$symbol=[string]$spec[0];$managedPattern=[string]$spec[1]
    if(@($row.nativeEntrypoints).Count -ne 1 -or $row.nativeEntrypoints[0] -cne $symbol -or $manifest -notcontains $symbol){throw "PhaseUnwrapping declaration-to-native ABI mapping drifted: $identity"}
    if(@($row.managedMembers).Count -ne 1 -or $row.managedMembers[0] -notmatch $managedPattern -or $managedBaseline -notcontains [string]$row.managedMembers[0]){throw "PhaseUnwrapping declaration-to-managed member mapping drifted: $identity"}
}
$release='jyppx_ocv_phase_unwrapping_release';$moduleSymbols=@($manifest|Where-Object {$_ -match '^jyppx_ocv_phase_unwrapping_'})
if($moduleSymbols.Count -ne 4 -or $manifest -notcontains $release -or @($callables|ForEach-Object {@($_.nativeEntrypoints)}|Where-Object {$_ -eq $release}).Count -ne 0){throw 'PhaseUnwrapping release ABI must remain outside the parser callable scope.'}
$conveniences=@($managedBaseline|Where-Object {$_ -match 'JYPPX\.OpenCvSharp\.PhaseUnwrapping\.PhaseUnwrappingCv2\|method\|.*\bCreateHistogramPhaseUnwrapping\('})
if($conveniences.Count -ne 2 -or [int]$summary.nativeEvidenceCount -ne 3 -or [int]$summary.managedEvidenceCount -ne 3){throw 'PhaseUnwrapping convenience-factory or evidence boundary drifted.'}
Write-Host "PHASE_UNWRAPPING_UPSTREAM_MAP_CONTRACT_OK declarations=7 callables=4 implemented=3 omitted=1 missing=0 native=3 managed=3 lifetime_symbol=outside_parser_scope sha256=$($summary.mappingSha256)"
