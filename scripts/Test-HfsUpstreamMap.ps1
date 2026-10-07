param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-HfsUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/hfs-upstream-summary.json') -Raw|ConvertFrom-Json
$raw=Get-Content (Join-Path $repo 'compatibility/hfs-upstream-raw.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/hfs-upstream-classifications.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
$managedBaseline=@(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt'))
$header='opencv-source/opencv_contrib-5.0.0/modules/hfs/include/opencv2/hfs.hpp'
if([int]$summary.declarationCount -ne 18 -or [int]$summary.callableCount -ne 17 -or [int]$summary.classificationCounts.implemented -ne 17 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 0 -or [int]$summary.classificationCounts.missing -ne 0){throw 'HFS upstream map partition drifted.'}
if(@($raw.sourceHeaders|Where-Object {$_.path -ceq $header}).Count -ne 1){throw 'HFS map must cover the pinned public umbrella header.'}
$properties=@{
    SegEgbThresholdI=@('float','System.Single'); SegEgbThresholdII=@('float','System.Single'); SpatialWeight=@('float','System.Single')
    MinRegionSizeI=@('int','System.Int32'); MinRegionSizeII=@('int','System.Int32'); SlicSpixelSize=@('int','System.Int32'); NumSlicIter=@('int','System.Int32')
}
$expectedCallables=@{}
foreach($property in $properties.Keys){
    $kind=[string]$properties[$property][0];$managedType=[string]$properties[$property][1]
    $expectedCallables[("cv.hfs.HfsSegment.get${property}(")]=@("jyppx_ocv_hfs_segment_get_${kind}_property",("JYPPX\.OpenCvSharp\.Hfs\.HfsSegment\|property\|[^|]*\|$managedType $property$"))
    $expectedCallables[("cv.hfs.HfsSegment.set${property}(")]=@("jyppx_ocv_hfs_segment_set_${kind}_property",("JYPPX\.OpenCvSharp\.Hfs\.HfsSegment\|property\|[^|]*\|$managedType $property$"))
}
$expectedCallables['cv.hfs.HfsSegment.performSegmentCpu(']=@('jyppx_ocv_hfs_segment_perform_segment_cpu','JYPPX\.OpenCvSharp\.Hfs\.HfsSegment\|method\|[^|]*\|JYPPX\.OpenCvSharp\.Core\.Mat PerformSegmentCpu\(JYPPX\.OpenCvSharp\.Core\.Mat src,System\.Boolean draw=true\)')
$expectedCallables['cv.hfs.HfsSegment.performSegmentGpu(']=@('jyppx_ocv_hfs_segment_perform_segment_gpu','JYPPX\.OpenCvSharp\.Hfs\.HfsSegment\|method\|[^|]*\|JYPPX\.OpenCvSharp\.Core\.Mat PerformSegmentGpu\(JYPPX\.OpenCvSharp\.Core\.Mat src,System\.Boolean draw=true\)')
$expectedCallables['cv.hfs.HfsSegment.create(']=@('jyppx_ocv_hfs_segment_create','JYPPX\.OpenCvSharp\.Hfs\.HfsSegment\|method\|public;static\|JYPPX\.OpenCvSharp\.Hfs\.HfsSegment Create\(JYPPX\.OpenCvSharp\.Hfs\.HfsSegmentParams parameters\)')
$callables=@($map.declarations|Where-Object {$_.classification -eq 'implemented'})
if($callables.Count -ne 17 -or $expectedCallables.Count -ne 17){throw 'HFS semantic callable coverage drifted.'}
foreach($row in $callables){
    $identity=[string]$row.identity;$key=@($expectedCallables.Keys|Where-Object {$identity.StartsWith($_,[StringComparison]::Ordinal)})
    if($key.Count -ne 1){throw "Unexpected HFS callable identity: $identity"}
    $spec=$expectedCallables[$key[0]];$symbol=[string]$spec[0];$managedPattern=[string]$spec[1]
    if(@($row.nativeEntrypoints).Count -ne 1 -or $row.nativeEntrypoints[0] -cne $symbol -or $manifest -notcontains $symbol){throw "HFS declaration-to-native ABI mapping drifted: $identity"}
    if(@($row.managedMembers).Count -ne 1 -or $row.managedMembers[0] -notmatch $managedPattern -or $managedBaseline -notcontains [string]$row.managedMembers[0]){throw "HFS declaration-to-managed member mapping drifted: $identity"}
}
$release='jyppx_ocv_hfs_segment_release';$moduleSymbols=@($manifest|Where-Object {$_ -match '^jyppx_ocv_hfs_'})
if($moduleSymbols.Count -ne 8 -or $manifest -notcontains $release -or @($callables|ForEach-Object {@($_.nativeEntrypoints)}|Where-Object {$_ -eq $release}).Count -ne 0){throw 'HFS release ABI must remain outside the parser callable scope.'}
if([int]$summary.nativeEvidenceCount -ne 7 -or [int]$summary.managedEvidenceCount -ne 10){throw 'HFS distinct evidence counts drifted.'}
Write-Host "HFS_UPSTREAM_MAP_CONTRACT_OK declarations=18 callables=17 implemented=17 omitted=0 missing=0 native=7 managed=10 lifetime_symbol=outside_parser_scope sha256=$($summary.mappingSha256)"
