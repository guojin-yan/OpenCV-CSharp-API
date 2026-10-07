param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-RapidUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/rapid-upstream-summary.json') -Raw|ConvertFrom-Json
$raw=Get-Content (Join-Path $repo 'compatibility/rapid-upstream-raw.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/rapid-upstream-classifications.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
$managedBaseline=@(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt'))
$header='opencv-source/opencv_contrib-5.0.0/modules/rapid/include/opencv2/rapid.hpp'
if([string]$raw.headerPath -cne $header -or @($raw.sourceHeaders).Count -ne 1 -or [string]$raw.sourceHeaders[0].path -cne $header){throw 'Rapid map must cover only the pinned public umbrella header.'}
if([int]$summary.declarationCount -ne 17 -or [int]$summary.callableCount -ne 13 -or [int]$summary.classCount -ne 4 -or [int]$summary.classificationCounts.implemented -ne 12 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 1 -or [int]$summary.classificationCounts.missing -ne 0 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 4){throw 'Rapid upstream map partition drifted.'}

$expectedCallables=[ordered]@{
    'cv.rapid.drawCorrespondencies('=@('jyppx_ocv_rapid_draw_correspondencies','RapidCv2','DrawCorrespondencies')
    'cv.rapid.drawSearchLines('=@('jyppx_ocv_rapid_draw_search_lines','RapidCv2','DrawSearchLines')
    'cv.rapid.drawWireframe('=@('jyppx_ocv_rapid_draw_wireframe','RapidCv2','DrawWireframe')
    'cv.rapid.extractControlPoints('=@('jyppx_ocv_rapid_extract_control_points','RapidCv2','ExtractControlPoints')
    'cv.rapid.extractLineBundle('=@('jyppx_ocv_rapid_extract_line_bundle','RapidCv2','ExtractLineBundle')
    'cv.rapid.findCorrespondencies('=@('jyppx_ocv_rapid_find_correspondencies','RapidCv2','FindCorrespondencies')
    'cv.rapid.convertCorrespondencies('=@('jyppx_ocv_rapid_convert_correspondencies','RapidCv2','ConvertCorrespondencies')
    'cv.rapid.rapid('=@('jyppx_ocv_rapid_run','RapidCv2','Run')
    'cv.rapid.Tracker.compute('=@('jyppx_ocv_rapid_tracker_compute','RapidTracker','Compute')
    'cv.rapid.Tracker.clearState('=@('jyppx_ocv_rapid_tracker_clear_state','RapidTracker','ClearState')
    'cv.rapid.Rapid.create('=@('jyppx_ocv_rapid_tracker_create','RapidSilhouetteTracker','Create')
    'cv.rapid.OLSTracker.create('=@('jyppx_ocv_rapid_ols_tracker_create','OlsTracker','Create')
}
$callables=@($map.declarations|Where-Object {$_.classification -eq 'implemented'})
if($callables.Count -ne 12 -or $expectedCallables.Count -ne 12){throw 'Rapid semantic callable coverage drifted.'}
foreach($row in $callables){
    $identity=[string]$row.identity
    $key=@($expectedCallables.Keys|Where-Object {$identity.StartsWith($_,[StringComparison]::Ordinal)})
    if($key.Count -ne 1){throw "Unexpected implemented Rapid callable identity: $identity"}
    $spec=$expectedCallables[$key[0]];$symbol=[string]$spec[0];$type=[string]$spec[1];$method=[string]$spec[2]
    if(@($row.nativeEntrypoints).Count -ne 1 -or $row.nativeEntrypoints[0] -cne $symbol -or $manifest -notcontains $symbol){throw "Rapid declaration-to-native ABI mapping drifted: $identity"}
    if(@($row.managedMembers).Count -ne 1){throw "Rapid declaration must bind exactly one managed member: $identity"}
    $member=[string]$row.managedMembers[0]
    if($managedBaseline -notcontains $member -or $member -notmatch ('^MEMBER\|JYPPX\.OpenCvSharp\.Rapid\.'+[regex]::Escape($type)+'\|method\|[^|]*\|[^|]*\b'+[regex]::Escape($method)+'\(')){throw "Rapid declaration-to-managed member mapping drifted: $identity"}
}
$omitted=@($map.declarations|Where-Object {$_.classification -eq 'intentionally-omitted'})
if($omitted.Count -ne 1 -or [string]$omitted[0].identity -notmatch '^cv\.rapid\.GOSTracker\.create\(' -or @($omitted[0].nativeEntrypoints).Count -ne 0 -or @($omitted[0].managedMembers).Count -ne 0 -or [string]$omitted[0].reason -notmatch 'GOSTracker.*deliberately omitted'){throw 'Rapid GOSTracker create must remain an explicit, evidence-free intentional omission.'}
$metadata=@($map.declarations|Where-Object {$_.classification -eq 'non-callable-metadata'})
if($metadata.Count -ne 4 -or (@($metadata|ForEach-Object {[string]$_.identity}) -join '|') -cne 'class cv.rapid.Tracker|class cv.rapid.Rapid|class cv.rapid.OLSTracker|class cv.rapid.GOSTracker'){throw 'Rapid class metadata closure drifted.'}
$moduleSymbols=@($manifest|Where-Object {$_ -match '^jyppx_ocv_rapid_'})
$release='jyppx_ocv_rapid_tracker_release'
if($moduleSymbols.Count -ne 13 -or $manifest -notcontains $release -or @($callables|ForEach-Object {@($_.nativeEntrypoints)}|Where-Object {$_ -eq $release}).Count -ne 0){throw 'Rapid tracker lifetime ABI must remain outside the parser callable scope.'}
if([int]$summary.nativeEvidenceCount -ne 12 -or [int]$summary.managedEvidenceCount -ne 12){throw 'Rapid distinct evidence counts drifted.'}
Write-Host "RAPID_UPSTREAM_MAP_CONTRACT_OK declarations=17 callables=13 implemented=12 omitted=1 missing=0 metadata_classes=4 native=12 managed=12 lifetime_symbol=outside_parser_scope sha256=$($summary.mappingSha256)"
