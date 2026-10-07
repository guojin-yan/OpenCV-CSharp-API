param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-QualityUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/quality-upstream-summary.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/quality-upstream-classifications.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
if([int]$summary.declarationCount -ne 37 -or [int]$summary.callableCount -ne 31 -or [int]$summary.classificationCounts.implemented -ne 29 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 2 -or [int]$summary.classificationCounts.missing -ne 0){throw 'Quality upstream map partition drifted.'}
function Assert-QualityRow([string]$Prefix,[string[]]$Native,[string]$ManagedText){$rows=@($map.declarations|Where-Object {$_.identity.StartsWith($Prefix)});if($rows.Count -ne 1){throw "Expected one Quality declaration: $Prefix"};foreach($entry in $Native){if(@($rows[0].nativeEntrypoints) -notcontains $entry -or $manifest -notcontains $entry){throw "Quality declaration-to-symbol mapping drifted: $Prefix -> $entry"}};if(@($rows[0].managedMembers|Where-Object {$_ -match $ManagedText}).Count -eq 0){throw "Quality managed evidence is missing: $Prefix"}}
Assert-QualityRow 'cv.quality.QualityGMSD.compute(Mat cmp)' @('jyppx_ocv_quality_compute') '\.QualityBase\|method\|.* Compute\('
Assert-QualityRow 'cv.quality.QualityGMSD.compute(Mat ref;' @('jyppx_ocv_quality_gmsd_compute_static') '\.QualityGMSD\|method\|.*static.* Compute\('
Assert-QualityRow 'cv.quality.QualityBase.empty(' @('jyppx_ocv_quality_empty') '\.QualityBase\|property\|.* Empty$'
$svm=@($map.declarations|Where-Object {$_.identity -match 'QualityBRISQUE\.create\(Ptr_ml_SVM' -and $_.classification -eq 'intentionally-omitted'});if($svm.Count -ne 1 -or @($svm[0].managedMembers).Count -ne 0){throw 'Quality BRISQUE SVM-object overload must remain explicitly omitted.'}
$mse=@($map.declarations|Where-Object {$_.identity -match 'QualityMSE\.compute\(vector_Mat cmpImgs' -and $_.classification -eq 'intentionally-omitted'});if($mse.Count -ne 1){throw 'Quality MSE batch overload classification drifted.'}
if(@($map.externalDataDependencies|Where-Object {$_ -match 'QualityBRISQUE' -and $_ -match 'SVM model and range files' -and $_ -match 'no model data is bundled'}).Count -ne 1){throw 'Quality external-model dependency declaration drifted.'}
Write-Host "QUALITY_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 modelDependencies=$($summary.externalDataDependencyCount) sha256=$($summary.mappingSha256)"
