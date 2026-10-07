param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-FaceUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/face-upstream-summary.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/face-upstream-classifications.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
if([int]$summary.declarationCount -ne 62 -or [int]$summary.callableCount -ne 49 -or [int]$summary.classificationCounts.implemented -ne 40 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 9 -or [int]$summary.classificationCounts.missing -ne 0){throw 'Face upstream map partition drifted.'}
function Assert-FaceRow([string]$Prefix,[string[]]$Native){$rows=@($map.declarations|Where-Object {$_.identity.StartsWith($Prefix)});if($rows.Count -ne 1){throw "Expected one Face declaration: $Prefix"};foreach($entry in $Native){if(@($rows[0].nativeEntrypoints) -notcontains $entry -or $manifest -notcontains $entry){throw "Face declaration-to-symbol mapping drifted: $Prefix -> $entry"}};if(@($rows[0].managedMembers).Count -eq 0){throw "Face managed evidence is missing: $Prefix"}}
Assert-FaceRow 'cv.face.BIF.compute(' @('jyppx_ocv_face_bif_compute')
Assert-FaceRow 'cv.face.createFacemarkLBF(' @('jyppx_ocv_face_facemark_lbf_create')
Assert-FaceRow 'cv.face.BasicFaceRecognizer.getProjections(' @('jyppx_ocv_face_basic_get_projections_count','jyppx_ocv_face_basic_get_projections_fill')
Assert-FaceRow 'cv.face.LBPHFaceRecognizer.getGridX(' @('jyppx_ocv_face_lbph_get_grid_x')
Assert-FaceRow 'cv.face.MACE.train(' @('jyppx_ocv_face_mace_train')
foreach($factory in @('createFacemarkAAM','createFacemarkKazemi')){if(@($map.declarations|Where-Object {$_.identity.StartsWith("cv.face.$factory(") -and $_.classification -eq 'implemented'}).Count -ne 0){throw "Face $factory must remain omitted without a wrapper surface."}}
if(@($map.externalDataDependencies|Where-Object {$_ -match 'caller-selected compatible landmark model' -and $_ -match 'no model data is bundled'}).Count -ne 1){throw 'Face landmark-model dependency declaration drifted.'}
Write-Host "FACE_UPSTREAM_MAP_CONTRACT_OK declarations=$($summary.declarationCount) callables=$($summary.callableCount) implemented=$($summary.classificationCounts.implemented) omitted=$($summary.classificationCounts.'intentionally-omitted') missing=0 modelDependencies=$($summary.externalDataDependencyCount) sha256=$($summary.mappingSha256)"
