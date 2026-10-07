param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-OptFlowUpstreamMap.ps1') -RepositoryRoot $repo -Check
if(-not $?){throw 'OptFlow upstream map freshness validation failed.'}
$s=Get-Content (Join-Path $repo 'compatibility/optflow-upstream-summary.json') -Raw|ConvertFrom-Json
$classes=Get-Content (Join-Path $repo 'compatibility/optflow-upstream-classifications.json') -Raw|ConvertFrom-Json
$native=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|Where-Object {$_ -match '^jyppx_ocv_(optflow|motempl)_'}|ForEach-Object {($_ -split '\|')[0]})
if([int]$s.classificationCounts.missing -ne 0 -or [int]$s.declarationCount -ne 85 -or [int]$s.classificationCounts.implemented -ne 70 -or [int]$s.classificationCounts.'intentionally-omitted' -ne 1){throw 'OptFlow upstream map partition drifted.'}
foreach($row in @($classes.declarations|Where-Object classification -eq 'implemented')){foreach($entry in @($row.nativeEntrypoints)){if($native -notcontains [string]$entry){throw "OptFlow implemented row references an unknown native entrypoint: $entry"}}}
$aliases=@($classes.declarations|Where-Object {$_.classification -eq 'implemented' -and $_.identity -notmatch '^cv\.motempl\.' -and @($_.nativeEntrypoints|Where-Object {$_ -match '^jyppx_ocv_motempl_' }).Count -gt 0})
if($aliases.Count -gt 0){throw 'OptFlow map reused a motion-template entrypoint for a non-motempl declaration.'}
foreach($expected in @(@{identity='cv.optflow.RLOFOpticalFlowParameter.create(';native='jyppx_ocv_optflow_rlof_parameter_create'},@{identity='cv.optflow.DenseRLOFOpticalFlow.create(';native='jyppx_ocv_optflow_dense_rlof_create'},@{identity='cv.optflow.calcOpticalFlowDenseRLOF(';native='jyppx_ocv_optflow_calc_optical_flow_dense_rlof'},@{identity='cv.optflow.createOptFlow_DenseRLOF(';native='jyppx_ocv_optflow_dense_rlof_create'})){ $row=@($classes.declarations|Where-Object {$_.identity.StartsWith($expected.identity)}); if($row.Count -ne 1 -or @($row[0].nativeEntrypoints) -notcontains $expected.native -or @($row[0].managedMembers).Count -eq 0){throw "OptFlow declaration-to-symbol evidence drifted: $($expected.identity)"} }
if(@($classes.declarations|Where-Object {$_.identity.StartsWith('cv.optflow.createOptFlow_PCAFlow(') -and $_.classification -eq 'implemented'}).Count -ne 0){throw 'OptFlow PCAFlow must remain omitted without a wrapper entrypoint.'}
Write-Host "OPTFLOW_UPSTREAM_MAP_CONTRACT_OK declarations=$($s.declarationCount) callables=$($s.callableCount) implemented=$($s.classificationCounts.implemented) omitted=$($s.classificationCounts.'intentionally-omitted') missing=0 fixtures=$($s.negativeFixtureCount) nativeEvidence=$($s.nativeEvidenceCount) sha256=$($s.mappingSha256)"
