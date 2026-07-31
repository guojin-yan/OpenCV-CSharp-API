[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if ([string]::IsNullOrWhiteSpace($DotNetPath)) { $DotNetPath = (Get-Command dotnet -ErrorAction Stop).Source }
$dotnet = (Resolve-Path -LiteralPath $DotNetPath).Path

& (Join-Path $repo "scripts/Generate-StitchingUpstreamMap.ps1") -RepositoryRoot $repo -DotNetPath $dotnet -Check
if ($LASTEXITCODE -ne 0) { throw "Stitching map check failed with exit code $LASTEXITCODE." }

$summary = Get-Content -LiteralPath (Join-Path $repo "compatibility/stitching-upstream-summary.json") -Raw | ConvertFrom-Json
if ($summary.declarationCount -ne 207 -or $summary.callableCount -ne 158 -or $summary.enumCount -ne 9 -or $summary.classCount -ne 40) { throw "Stitching parser count contract drifted." }
if ($summary.compatibilityHeaderCount -ne 14 -or $summary.sourceHeaderCount -ne 14) { throw "Stitching public header closure drifted." }
if ($summary.surfaceCounts.primary.declarations -ne 24 -or $summary.surfaceCounts.primary.callables -ne 21 -or $summary.surfaceCounts.primary.implemented -ne 21) { throw "Stitching high-level partition drifted." }
if ($summary.surfaceCounts.'public-warpers'.declarations -ne 12 -or $summary.surfaceCounts.'public-warpers'.callables -ne 10 -or $summary.surfaceCounts.'public-warpers'.implemented -ne 10) { throw "Stitching public-warper partition drifted." }
if ($summary.surfaceCounts.'detail-blenders'.declarations -ne 28 -or $summary.surfaceCounts.'detail-blenders'.callables -ne 24 -or $summary.surfaceCounts.'detail-blenders'.implemented -ne 24) { throw "Stitching Blender partition drifted." }
if ($summary.surfaceCounts.'detail-exposure'.declarations -ne 53 -or $summary.surfaceCounts.'detail-exposure'.callables -ne 45 -or $summary.surfaceCounts.'detail-exposure'.implemented -ne 45) { throw "Stitching Exposure partition drifted." }
if ($summary.classificationCounts.implemented -ne 100 -or $summary.classificationCounts.missing -ne 58 -or
    $summary.classificationCounts.'non-callable-metadata' -ne 49 -or $summary.classificationCounts.'intentionally-omitted' -ne 0 -or
    $summary.classificationCounts.unsupported -ne 0 -or $summary.classificationCounts.'upstream-conditional' -ne 0) { throw "Stitching classification contract drifted." }
if ($summary.negativeFixtureCount -ne 24 -or $summary.selectedFamilyCount -ne 3 -or $summary.selectedDeclarationCount -ne 79 -or $summary.highLevelImplementedCallableCount -ne 21) { throw "Stitching fixture/family contract drifted." }
if ($summary.managedPublicTypeAdditionCount -ne 13 -or $summary.managedPublicMemberAdditionCount -ne 60 -or $summary.nativeEntrypointAdditionCount -ne 52) { throw "Stitching addition counts drifted." }
if ($summary.umatExecutionClaimed -or $summary.detailRowsMixedIntoHighLevel -or $summary.repositoryWideUpstreamParityClaimed) { throw "Stitching ownership or parity boundary was weakened." }
if ($summary.mappingSha256 -ne "b531f712752503b41dc8dd5138d1fa6cf154529bb7f67b20c9cafb6ef0fa17f0" -or
    $summary.familyInventorySha256 -ne "4bf62428de8e76a4485882fb659ee933e44ef8e291c2dc09f8f62d54a9c6341f") { throw "Stitching generated map hashes drifted." }

$rawHash = (Get-FileHash -LiteralPath (Join-Path $repo "compatibility/stitching-upstream-raw.json") -Algorithm SHA256).Hash.ToLowerInvariant()
if ($rawHash -ne "e712d4faed827e0c3e35ed584cde22fe11c60a9ca7589c3d87f6b840321399ad") { throw "Stitching raw extraction hash drifted." }

$classifications = Get-Content -LiteralPath (Join-Path $repo "compatibility/stitching-upstream-classifications.json") -Raw | ConvertFrom-Json
$blenderOrdinals = @(40, 41, 42, 43, 44, 46, 47, 48, 49, 50, 51, 52, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65)
foreach ($ordinal in $blenderOrdinals) {
    $row = $classifications.declarations[$ordinal]
    if ($row.ordinal -ne $ordinal -or $row.surface -ne "detail-blenders" -or $row.classification -ne "implemented" -or
        $row.nativeEntrypoints.Count -eq 0 -or $row.managedMembers.Count -eq 0) { throw "Stitching Blender evidence drifted at ordinal $ordinal." }
}
foreach ($ordinal in @(38, 39, 45, 53)) {
    if ($classifications.declarations[$ordinal].classification -ne "non-callable-metadata") { throw "Stitching Blender metadata drifted at ordinal $ordinal." }
}

$families = Get-Content -LiteralPath (Join-Path $repo "compatibility/stitching-implemented-families.json") -Raw | ConvertFrom-Json
$blenderFamily = @($families.families | Where-Object id -eq "stitching-detail-blender-completion")
if ($blenderFamily.Count -ne 1 -or $blenderFamily[0].declarations.Count -ne 24 -or
    (@($blenderFamily[0].declarations.ordinal) -join ',') -ne ($blenderOrdinals -join ',')) { throw "Stitching Blender family inventory drifted." }

$entrypoints = @(
    "jyppx_ocv_stitching_blender_create_default", "jyppx_ocv_stitching_blender_create_feather",
    "jyppx_ocv_stitching_blender_create_multi_band", "jyppx_ocv_stitching_blender_release_handle",
    "jyppx_ocv_stitching_blender_prepare", "jyppx_ocv_stitching_blender_prepare_roi",
    "jyppx_ocv_stitching_blender_feed", "jyppx_ocv_stitching_blender_blend",
    "jyppx_ocv_stitching_blender_get_sharpness", "jyppx_ocv_stitching_blender_set_sharpness",
    "jyppx_ocv_stitching_blender_get_number_of_bands", "jyppx_ocv_stitching_blender_set_number_of_bands",
    "jyppx_ocv_stitching_blender_create_weight_maps", "jyppx_ocv_stitching_normalize_using_weight_map",
    "jyppx_ocv_stitching_create_weight_map", "jyppx_ocv_stitching_create_laplace_pyramid",
    "jyppx_ocv_stitching_create_laplace_pyramid_gpu", "jyppx_ocv_stitching_restore_image_from_laplace_pyramid",
    "jyppx_ocv_stitching_restore_image_from_laplace_pyramid_gpu"
)
$dllImport = Get-Content -LiteralPath (Join-Path $repo "src/OpenCvSharp/Internal/Interop/NativeMethods.Stitching.DllImport.cs") -Raw
$libraryImport = Get-Content -LiteralPath (Join-Path $repo "src/OpenCvSharp/Internal/Interop/NativeMethods.Stitching.LibraryImport.cs") -Raw
$manifest = Get-Content -LiteralPath (Join-Path $repo "src/OpenCvSharp.Native/generated/legacy_abi_manifest.txt") -Raw
foreach ($entrypoint in $entrypoints) {
    if (-not $dllImport.Contains($entrypoint) -or -not $libraryImport.Contains($entrypoint) -or -not $manifest.Contains("$entrypoint|")) {
        throw "Stitching Blender ABI/interop evidence is incomplete: $entrypoint"
    }
}

$smoke = Get-Content -LiteralPath (Join-Path $repo "src/OpenCvSharp.Native/tests/native_smoke.cpp") -Raw
$tests = Get-Content -LiteralPath (Join-Path $repo "tests/OpenCvSharp.Tests/Stitching/BlenderTests.cs") -Raw
$sample = Get-Content -LiteralPath (Join-Path $repo "samples/ConsoleSamples/Program.cs") -Raw
$guide = Get-Content -LiteralPath (Join-Path $repo "docs/articles/stitching-structured-parity-guide.md") -Raw
$consumer = Get-Content -LiteralPath (Join-Path $repo "scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1") -Raw
if (-not $smoke.Contains("run_stitching_blender_smoke") -or -not $smoke.Contains("OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION") -or
    -not $smoke.Contains("jyppx_ocv_stitching_blender_create_default(0, 0, &blender) != OPENCV_CSHARP_STATUS_NOT_LINKED")) { throw "Stitching Blender native smoke or profile-boundary evidence is incomplete." }
if (-not $tests.Contains("GpuNamedPyramidHelpersPreserveUpstreamUnavailableError") -or -not $tests.Contains("CpuLaplacePyramidRoundTripsRoiInput") -or
    -not $sample.Contains("RunBlenderSummary") -or -not $guide.Contains("CUDA optimization is unavailable") -or
    -not $consumer.Contains("typeof(OpenCvSharp.Stitching.MultiBandBlender)")) { throw "Stitching Blender managed evidence is incomplete." }

Write-Output "STITCHING_UPSTREAM_MAP_GUARD_OK declarations=207 high-level=24/21/21 public-warper=12/10/10 blender=28/24/24 exposure=53/45/45 implemented=100 missing=58 fixtures=24"
