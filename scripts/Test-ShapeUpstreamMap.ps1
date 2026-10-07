param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-ShapeUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/shape-upstream-summary.json') -Raw|ConvertFrom-Json
$raw=Get-Content (Join-Path $repo 'compatibility/shape-upstream-raw.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/shape-upstream-classifications.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
$managedBaseline=@(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt'))
$umbrella='opencv-source/opencv_contrib-5.0.0/modules/shape/include/opencv2/shape.hpp'
if([string]$raw.headerPath -cne $umbrella -or @($raw.sourceHeaders).Count -ne 5 -or @($raw.sourceHeaders|Where-Object {[string]$_.path -notmatch '^opencv-source/opencv_contrib-5\.0\.0/modules/shape/include/opencv2/shape/'}).Count -ne 0){throw 'Shape map must cover only the pinned umbrella and its parser-emitted module header closure.'}
if([int]$summary.declarationCount -ne 66 -or [int]$summary.callableCount -ne 55 -or [int]$summary.classCount -ne 11 -or [int]$summary.classificationCounts.implemented -ne 20 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 35 -or [int]$summary.classificationCounts.missing -ne 0 -or [int]$summary.classificationCounts.'non-callable-metadata' -ne 11){throw 'Shape upstream declaration partition drifted.'}

$expected=[ordered]@{
    'cv.HistogramCostExtractor.buildCostMatrix('=@('jyppx_ocv_shape_histogram_cost_extractor_build_cost_matrix','HistogramCostExtractor','BuildCostMatrix')
    'cv.HistogramCostExtractor.setNDummies('=@('jyppx_ocv_shape_histogram_cost_extractor_set_n_dummies','HistogramCostExtractor','NDummies')
    'cv.HistogramCostExtractor.getNDummies('=@('jyppx_ocv_shape_histogram_cost_extractor_get_n_dummies','HistogramCostExtractor','NDummies')
    'cv.HistogramCostExtractor.setDefaultCost('=@('jyppx_ocv_shape_histogram_cost_extractor_set_default_cost','HistogramCostExtractor','DefaultCost')
    'cv.HistogramCostExtractor.getDefaultCost('=@('jyppx_ocv_shape_histogram_cost_extractor_get_default_cost','HistogramCostExtractor','DefaultCost')
    'cv.NormHistogramCostExtractor.setNormFlag('=@('jyppx_ocv_shape_histogram_cost_extractor_set_norm_flag','NormHistogramCostExtractorBase','NormFlag')
    'cv.NormHistogramCostExtractor.getNormFlag('=@('jyppx_ocv_shape_histogram_cost_extractor_get_norm_flag','NormHistogramCostExtractorBase','NormFlag')
    'cv.createNormHistogramCostExtractor('=@('jyppx_ocv_shape_norm_histogram_cost_extractor_create','NormHistogramCostExtractor','CreateNormHistogramCostExtractor')
    'cv.EMDHistogramCostExtractor.setNormFlag('=@('jyppx_ocv_shape_histogram_cost_extractor_set_norm_flag','NormHistogramCostExtractorBase','NormFlag')
    'cv.EMDHistogramCostExtractor.getNormFlag('=@('jyppx_ocv_shape_histogram_cost_extractor_get_norm_flag','NormHistogramCostExtractorBase','NormFlag')
    'cv.createEMDHistogramCostExtractor('=@('jyppx_ocv_shape_emd_histogram_cost_extractor_create','EMDHistogramCostExtractor','CreateEMDHistogramCostExtractor')
    'cv.createChiHistogramCostExtractor('=@('jyppx_ocv_shape_chi_histogram_cost_extractor_create','ChiHistogramCostExtractor','CreateChiHistogramCostExtractor')
    'cv.createEMDL1HistogramCostExtractor('=@('jyppx_ocv_shape_emd_l1_histogram_cost_extractor_create','EMDL1HistogramCostExtractor','CreateEMDL1HistogramCostExtractor')
    'cv.ShapeDistanceExtractor.computeDistance('=@('jyppx_ocv_shape_distance_extractor_compute_distance','ShapeDistanceExtractor','ComputeDistance')
    'cv.createShapeContextDistanceExtractor('=@('jyppx_ocv_shape_context_distance_extractor_create','ShapeContextDistanceExtractor','CreateShapeContextDistanceExtractor')
    'cv.HausdorffDistanceExtractor.setDistanceFlag('=@('jyppx_ocv_shape_hausdorff_distance_extractor_set_distance_flag','HausdorffDistanceExtractor','DistanceFlag')
    'cv.HausdorffDistanceExtractor.getDistanceFlag('=@('jyppx_ocv_shape_hausdorff_distance_extractor_get_distance_flag','HausdorffDistanceExtractor','DistanceFlag')
    'cv.HausdorffDistanceExtractor.setRankProportion('=@('jyppx_ocv_shape_hausdorff_distance_extractor_set_rank_proportion','HausdorffDistanceExtractor','RankProportion')
    'cv.HausdorffDistanceExtractor.getRankProportion('=@('jyppx_ocv_shape_hausdorff_distance_extractor_get_rank_proportion','HausdorffDistanceExtractor','RankProportion')
    'cv.createHausdorffDistanceExtractor('=@('jyppx_ocv_shape_hausdorff_distance_extractor_create','HausdorffDistanceExtractor','CreateHausdorffDistanceExtractor')
}
$implemented=@($map.declarations|Where-Object {$_.classification -eq 'implemented'})
if($implemented.Count -ne 20 -or $expected.Count -ne 20){throw 'Shape semantic callable coverage drifted.'}
foreach($row in $implemented){
    $identity=[string]$row.identity
    $key=@($expected.Keys|Where-Object {$identity.StartsWith($_,[StringComparison]::Ordinal)})
    if($key.Count -ne 1){throw "Unexpected implemented Shape callable identity: $identity"}
    $spec=$expected[$key[0]];$symbol=[string]$spec[0];$owner=[string]$spec[1];$member=[string]$spec[2]
    if(@($row.nativeEntrypoints).Count -ne 1 -or [string]$row.nativeEntrypoints[0] -cne $symbol -or $manifest -notcontains $symbol){throw "Shape declaration-to-native ABI mapping drifted: $identity"}
    if(@($row.managedMembers).Count -eq 0 -or @($row.managedMembers|Where-Object {$managedBaseline -notcontains [string]$_}).Count -ne 0){throw "Shape declaration has missing managed baseline evidence: $identity"}
    if($member -in @('CreateNormHistogramCostExtractor','CreateEMDHistogramCostExtractor','CreateChiHistogramCostExtractor','CreateEMDL1HistogramCostExtractor','CreateShapeContextDistanceExtractor','CreateHausdorffDistanceExtractor')){
        if(@($row.managedMembers).Count -ne 2 -or @($row.managedMembers|Where-Object {$_ -match ('Shape\.'+[regex]::Escape($owner)+'\|method\|public;static\|.*\bCreate\(')}).Count -ne 1 -or @($row.managedMembers|Where-Object {$_ -match ('Shape\.ShapeCv2\|method\|public;static\|.*\b'+[regex]::Escape($member)+'\(')}).Count -ne 1){throw "Shape factory must bind both the owner Create API and ShapeCv2 facade: $identity"}
    } elseif(@($row.managedMembers|Where-Object {$_ -match ('Shape\.'+[regex]::Escape($owner)+'\|(?:method|property)\|[^|]*\|.*\b'+[regex]::Escape($member)+'(?:\(|$)')}).Count -ne 1){throw "Shape declaration-to-managed member mapping drifted: $identity"}
}
$omitted=@($map.declarations|Where-Object {$_.classification -eq 'intentionally-omitted'})
if(@($omitted|Where-Object {@($_.nativeEntrypoints).Count -ne 0 -or @($_.managedMembers).Count -ne 0}).Count -ne 0){throw 'Shape intentional omissions must not retain partial ABI or managed bindings.'}
if(@($omitted|Where-Object {[string]$_.identity -like 'cv.ShapeContextDistanceExtractor.setAngularBins(*'}).Count -ne 1 -or @($omitted|Where-Object {[string]$_.identity -like 'cv.ShapeTransformer.estimateTransformation(*'}).Count -ne 1 -or @($omitted|Where-Object {[string]$_.identity -like 'cv.createThinPlateSplineShapeTransformer(*'}).Count -ne 1){throw 'Shape wrapper omission boundary drifted.'}
$metadata=@($map.declarations|Where-Object {$_.classification -eq 'non-callable-metadata'})
if($metadata.Count -ne 11 -or @($metadata|Where-Object {[string]$_.identity -notmatch '^class cv\.(HistogramCostExtractor|NormHistogramCostExtractor|EMDHistogramCostExtractor|ChiHistogramCostExtractor|EMDL1HistogramCostExtractor|ShapeDistanceExtractor|ShapeContextDistanceExtractor|HausdorffDistanceExtractor|ShapeTransformer|ThinPlateSplineShapeTransformer|AffineTransformer)$'}).Count -ne 0){throw 'Shape class metadata closure drifted.'}
$moduleSymbols=@($manifest|Where-Object {$_ -match '^jyppx_ocv_shape_'})
$parserSymbols=@($implemented|ForEach-Object {@($_.nativeEntrypoints)}|Sort-Object -Unique)
$outside=@('jyppx_ocv_shape_emd_l1','jyppx_ocv_shape_histogram_cost_extractor_release_handle','jyppx_ocv_shape_distance_extractor_release_handle')
if($moduleSymbols.Count -ne 21 -or $parserSymbols.Count -ne 18 -or @($outside|Where-Object {$moduleSymbols -notcontains $_ -or $parserSymbols -contains $_}).Count -ne 0){throw 'Shape EMD-L1 and lifetime exports must remain outside parser callable scope.'}
if([int]$summary.nativeEvidenceCount -ne 18 -or [int]$summary.managedEvidenceCount -ne 19){throw 'Shape distinct evidence counts drifted.'}
Write-Host "SHAPE_UPSTREAM_MAP_CONTRACT_OK declarations=66 callables=55 implemented=20 omitted=35 missing=0 metadata_classes=11 native=18 managed=19 outside_scope=3 sha256=$($summary.mappingSha256)"
