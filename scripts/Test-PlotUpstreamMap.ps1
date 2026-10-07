param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')))
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-PlotUpstreamMap.ps1') -RepositoryRoot $repo -Check
$summary=Get-Content (Join-Path $repo 'compatibility/plot-upstream-summary.json') -Raw|ConvertFrom-Json
$map=Get-Content (Join-Path $repo 'compatibility/plot-upstream-classifications.json') -Raw|ConvertFrom-Json
$raw=Get-Content (Join-Path $repo 'compatibility/plot-upstream-raw.json') -Raw|ConvertFrom-Json
$manifest=@(Get-Content (Join-Path $repo 'src/OpenCvSharp.Native/generated/native_abi_manifest.txt')|ForEach-Object {($_ -split '\|')[0]})
$expected=@{
    setMinX='jyppx_ocv_plot_2d_set_min_x'; setMinY='jyppx_ocv_plot_2d_set_min_y'
    setMaxX='jyppx_ocv_plot_2d_set_max_x'; setMaxY='jyppx_ocv_plot_2d_set_max_y'
    setPlotLineWidth='jyppx_ocv_plot_2d_set_plot_line_width'; setNeedPlotLine='jyppx_ocv_plot_2d_set_need_plot_line'
    setPlotLineColor='jyppx_ocv_plot_2d_set_plot_line_color'; setPlotBackgroundColor='jyppx_ocv_plot_2d_set_plot_background_color'
    setPlotAxisColor='jyppx_ocv_plot_2d_set_plot_axis_color'; setPlotGridColor='jyppx_ocv_plot_2d_set_plot_grid_color'
    setPlotTextColor='jyppx_ocv_plot_2d_set_plot_text_color'; setPlotSize='jyppx_ocv_plot_2d_set_plot_size'
    setShowGrid='jyppx_ocv_plot_2d_set_show_grid'; setShowText='jyppx_ocv_plot_2d_set_show_text'
    setGridLinesNumber='jyppx_ocv_plot_2d_set_grid_lines_number'; setInvertOrientation='jyppx_ocv_plot_2d_set_invert_orientation'
    setPointIdxToPrint='jyppx_ocv_plot_2d_set_point_idx_to_print'; render='jyppx_ocv_plot_2d_render'
}
if([int]$summary.declarationCount -ne 21 -or [int]$summary.callableCount -ne 20 -or [int]$summary.classificationCounts.implemented -ne 20 -or [int]$summary.classificationCounts.'intentionally-omitted' -ne 0 -or [int]$summary.classificationCounts.missing -ne 0){throw 'Plot upstream map partition drifted.'}
if(@($raw.sourceHeaders|Where-Object {$_.path -eq 'opencv-source/opencv_contrib-5.0.0/modules/plot/include/opencv2/plot.hpp'}).Count -ne 1){throw 'Plot map must cover the pinned public umbrella header.'}
$callables=@($map.declarations|Where-Object {$_.classification -eq 'implemented'})
if($callables.Count -ne 20){throw 'Plot map implemented callable count drifted.'}
foreach($row in $callables){
    if($row.identity -notmatch '^cv\.plot\.Plot2d\.(?<name>[A-Za-z][A-Za-z0-9_]*)\('){throw "Unexpected Plot callable identity: $($row.identity)"}
    $name=[string]$Matches.name
    $nativeSymbol=[string]$expected[$name]
    if($name -eq 'create'){$nativeSymbol=if($row.identity -match 'Mat dataY'){'jyppx_ocv_plot_2d_create_xy'}else{'jyppx_ocv_plot_2d_create'}}
    if([string]::IsNullOrWhiteSpace($nativeSymbol) -or @($row.nativeEntrypoints).Count -ne 1 -or $row.nativeEntrypoints[0] -cne $nativeSymbol -or $manifest -notcontains $nativeSymbol){throw "Plot declaration-to-native ABI mapping drifted: $($row.identity)"}
    $managed=@($row.managedMembers|Where-Object {$_ -match '\.Plot\.Plot2d\|method\|'})
    $expectedManagedCount=if($name -eq 'render'){2}else{1}
    if($managed.Count -ne $expectedManagedCount){throw "Plot declaration-to-managed overload mapping drifted: $($row.identity)"}
    $managedName=if($name -eq 'create'){'Create'}elseif($name -eq 'render'){'Render'}else{$name.Substring(0,1).ToUpperInvariant()+$name.Substring(1)}
    if(@($managed|Where-Object {$_ -match ('\b'+[regex]::Escape($managedName)+'\(')}).Count -ne $expectedManagedCount){throw "Plot managed member name drifted: $($row.identity)"}
}
$createRows=@($callables|Where-Object {$_.identity -match '\.create\('})
if($createRows.Count -ne 2 -or @($createRows|Where-Object {$_.identity -match 'Mat dataY' -and $_.nativeEntrypoints -notcontains 'jyppx_ocv_plot_2d_create_xy'}).Count -ne 0){throw 'Plot factory overload/native binding drifted.'}
$release='jyppx_ocv_plot_2d_release_handle'
$moduleSymbols=@($manifest|Where-Object {$_ -match '^jyppx_ocv_plot_'})
if($moduleSymbols.Count -ne 21 -or $manifest -notcontains $release -or @($callables|ForEach-Object {@($_.nativeEntrypoints)}|Where-Object {$_ -eq $release}).Count -ne 0){throw 'Plot module ABI boundary drifted: release_handle is wrapper lifetime support, not a parser callable.'}
$plotCv2=@(Get-Content (Join-Path $repo 'compatibility/managed-public-api.txt')|Where-Object {$_ -match 'JYPPX\.OpenCvSharp\.Plot\.PlotCv2\|method\|.*\bCreatePlot2d\('})
if($plotCv2.Count -ne 2 -or [int]$summary.nativeEvidenceCount -ne 20 -or [int]$summary.managedEvidenceCount -ne 21){throw 'Plot evidence partition drifted; PlotCv2 conveniences and Plot2d lifetime are outside the parser callable count.'}
Write-Host "PLOT_UPSTREAM_MAP_CONTRACT_OK declarations=21 callables=20 implemented=20 omitted=0 missing=0 native=$($summary.nativeEvidenceCount) managed=$($summary.managedEvidenceCount) lifetime_symbol=outside_parser_scope sha256=$($summary.mappingSha256)"
