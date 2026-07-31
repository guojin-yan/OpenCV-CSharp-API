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
if ($summary.surfaceCounts.'detail-matchers'.declarations -ne 23 -or $summary.surfaceCounts.'detail-matchers'.callables -ne 16 -or $summary.surfaceCounts.'detail-matchers'.implemented -ne 14) { throw "Stitching matcher partition drifted." }
if ($summary.surfaceCounts.'detail-autocalib'.declarations -ne 2 -or $summary.surfaceCounts.'detail-autocalib'.callables -ne 2 -or $summary.surfaceCounts.'detail-autocalib'.implemented -ne 2) { throw "Stitching autocalibration partition drifted." }
if ($summary.surfaceCounts.'detail-camera'.declarations -ne 2 -or $summary.surfaceCounts.'detail-camera'.callables -ne 1 -or $summary.surfaceCounts.'detail-camera'.implemented -ne 1) { throw "Stitching camera partition drifted." }
if ($summary.surfaceCounts.'detail-motion-estimators'.declarations -ne 27 -or $summary.surfaceCounts.'detail-motion-estimators'.callables -ne 17 -or $summary.surfaceCounts.'detail-motion-estimators'.implemented -ne 17) { throw "Stitching motion-estimator partition drifted." }
if ($summary.classificationCounts.implemented -ne 134 -or $summary.classificationCounts.missing -ne 22 -or
    $summary.classificationCounts.'non-callable-metadata' -ne 49 -or $summary.classificationCounts.'intentionally-omitted' -ne 0 -or
    $summary.classificationCounts.unsupported -ne 2 -or $summary.classificationCounts.'upstream-conditional' -ne 0) { throw "Stitching classification contract drifted." }
if ($summary.negativeFixtureCount -ne 28 -or $summary.selectedFamilyCount -ne 5 -or $summary.selectedDeclarationCount -ne 124 -or $summary.highLevelImplementedCallableCount -ne 21) { throw "Stitching fixture/family contract drifted." }
if ($summary.managedPublicTypeAdditionCount -ne 10 -or $summary.managedPublicMemberAdditionCount -ne 22 -or $summary.nativeEntrypointAdditionCount -ne 21) { throw "Stitching addition counts drifted." }
if ($summary.umatExecutionClaimed -or $summary.detailRowsMixedIntoHighLevel -or $summary.repositoryWideUpstreamParityClaimed) { throw "Stitching ownership or parity boundary was weakened." }
if ($summary.mappingSha256 -ne "d083e4238c9349064e23255cafdc2894fcd30583067d00b4176314475d46c43d" -or
    $summary.familyInventorySha256 -ne "54392669767d751dce3dbd00147ae31f34733d5070b3fa2d60f6cc9fbf27f71e") { throw "Stitching generated map hashes drifted." }

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

$matcherOrdinals = @(122, 123, 124, 126, 127, 129, 130, 131, 132, 134, 135, 136, 138, 140)
foreach ($ordinal in $matcherOrdinals) {
    $row = $classifications.declarations[$ordinal]
    if ($row.ordinal -ne $ordinal -or $row.surface -ne "detail-matchers" -or $row.classification -ne "implemented" -or
        $row.nativeEntrypoints.Count -eq 0 -or $row.managedMembers.Count -eq 0) { throw "Stitching matcher evidence drifted at ordinal $ordinal." }
}
foreach ($ordinal in @(121, 125, 128, 133, 137, 139, 141)) {
    if ($classifications.declarations[$ordinal].classification -ne "non-callable-metadata") { throw "Stitching matcher metadata drifted at ordinal $ordinal." }
}
foreach ($ordinal in @(142, 143)) {
    if ($classifications.declarations[$ordinal].classification -ne "unsupported" -or
        $classifications.declarations[$ordinal].nativeEntrypoints.Count -ne 0 -or
        $classifications.declarations[$ordinal].managedMembers.Count -ne 0) { throw "Stitching LightGlue boundary drifted at ordinal $ordinal." }
}
$matcherFamily = @($families.families | Where-Object id -eq "stitching-detail-feature-matchers-completion")
if ($matcherFamily.Count -ne 1 -or $matcherFamily[0].declarations.Count -ne 14 -or
    (@($matcherFamily[0].declarations.ordinal) -join ',') -ne ($matcherOrdinals -join ',')) { throw "Stitching matcher family inventory drifted." }

$cameraMotionOrdinals = @(36, 37, 67, 145, 147, 149, 151, 152, 153, 154, 155, 156, 158, 160, 162, 164, 166, 168, 169, 170)
foreach ($ordinal in $cameraMotionOrdinals) {
    $row = $classifications.declarations[$ordinal]
    if ($row.ordinal -ne $ordinal -or $row.classification -ne "implemented" -or
        $row.nativeEntrypoints.Count -eq 0 -or $row.managedMembers.Count -eq 0) { throw "Stitching camera/motion evidence drifted at ordinal $ordinal." }
}
foreach ($ordinal in @(146, 148, 150, 157, 159, 161, 163, 165, 167)) {
    if ($classifications.declarations[$ordinal].classification -ne "non-callable-metadata") { throw "Stitching motion metadata drifted at ordinal $ordinal." }
}
$cameraMotionFamily = @($families.families | Where-Object id -eq "stitching-camera-motion-estimator-completion")
if ($cameraMotionFamily.Count -ne 1 -or $cameraMotionFamily[0].declarations.Count -ne 20 -or
    (@($cameraMotionFamily[0].declarations.ordinal) -join ',') -ne ($cameraMotionOrdinals -join ',')) { throw "Stitching camera/motion family inventory drifted." }

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
    "jyppx_ocv_stitching_restore_image_from_laplace_pyramid_gpu",
    "jyppx_ocv_stitching_image_features_create", "jyppx_ocv_stitching_image_features_release_handle",
    "jyppx_ocv_stitching_image_features_get_image_index", "jyppx_ocv_stitching_image_features_set_image_index",
    "jyppx_ocv_stitching_image_features_get_image_size", "jyppx_ocv_stitching_image_features_set_image_size",
    "jyppx_ocv_stitching_image_features_get_keypoints_count", "jyppx_ocv_stitching_image_features_get_keypoints_fill",
    "jyppx_ocv_stitching_image_features_copy_descriptors", "jyppx_ocv_stitching_compute_image_features",
    "jyppx_ocv_stitching_compute_image_features_batch", "jyppx_ocv_stitching_matches_info_create",
    "jyppx_ocv_stitching_matches_info_release_handle", "jyppx_ocv_stitching_matches_info_get_metadata",
    "jyppx_ocv_stitching_matches_info_copy_homography", "jyppx_ocv_stitching_matches_info_get_matches_count",
    "jyppx_ocv_stitching_matches_info_get_matches_fill", "jyppx_ocv_stitching_matches_info_get_inliers_count",
    "jyppx_ocv_stitching_matches_info_get_inliers_fill", "jyppx_ocv_stitching_features_matcher_create_best_of_two_nearest",
    "jyppx_ocv_stitching_features_matcher_factory_best_of_two_nearest", "jyppx_ocv_stitching_features_matcher_create_range",
    "jyppx_ocv_stitching_features_matcher_create_affine", "jyppx_ocv_stitching_features_matcher_release_handle",
    "jyppx_ocv_stitching_features_matcher_match_pair", "jyppx_ocv_stitching_features_matcher_match_batch",
    "jyppx_ocv_stitching_features_matcher_is_thread_safe", "jyppx_ocv_stitching_features_matcher_collect_garbage",
    "jyppx_ocv_stitching_camera_params_get_k", "jyppx_ocv_stitching_focals_from_homography",
    "jyppx_ocv_stitching_calibrate_rotating_camera", "jyppx_ocv_stitching_estimator_create_homography",
    "jyppx_ocv_stitching_estimator_create_affine", "jyppx_ocv_stitching_estimator_create_no_bundle_adjuster",
    "jyppx_ocv_stitching_estimator_create_bundle_adjuster_reproj", "jyppx_ocv_stitching_estimator_create_bundle_adjuster_ray",
    "jyppx_ocv_stitching_estimator_create_bundle_adjuster_affine", "jyppx_ocv_stitching_estimator_create_bundle_adjuster_affine_partial",
    "jyppx_ocv_stitching_estimator_release_handle", "jyppx_ocv_stitching_estimator_apply",
    "jyppx_ocv_stitching_bundle_adjuster_copy_refinement_mask", "jyppx_ocv_stitching_bundle_adjuster_set_refinement_mask",
    "jyppx_ocv_stitching_bundle_adjuster_get_confidence_threshold", "jyppx_ocv_stitching_bundle_adjuster_set_confidence_threshold",
    "jyppx_ocv_stitching_bundle_adjuster_get_term_criteria", "jyppx_ocv_stitching_bundle_adjuster_set_term_criteria",
    "jyppx_ocv_stitching_wave_correct", "jyppx_ocv_stitching_matches_graph_as_string",
    "jyppx_ocv_stitching_leave_biggest_component"
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
$matcherTests = Get-Content -LiteralPath (Join-Path $repo "tests/OpenCvSharp.Tests/Stitching/FeaturesMatcherTests.cs") -Raw
$motionTests = Get-Content -LiteralPath (Join-Path $repo "tests/OpenCvSharp.Tests/Stitching/MotionEstimatorTests.cs") -Raw
$sample = Get-Content -LiteralPath (Join-Path $repo "samples/ConsoleSamples/Program.cs") -Raw
$guide = Get-Content -LiteralPath (Join-Path $repo "docs/articles/stitching-structured-parity-guide.md") -Raw
$consumer = Get-Content -LiteralPath (Join-Path $repo "scripts/Test-ManagedPackageStandaloneLocalConsumerCompile.ps1") -Raw
if (-not $smoke.Contains("run_stitching_blender_smoke") -or -not $smoke.Contains("OPENCV_CSHARP_STATUS_NATIVE_EXCEPTION") -or
    -not $smoke.Contains("jyppx_ocv_stitching_blender_create_default(0, 0, &blender) != OPENCV_CSHARP_STATUS_NOT_LINKED")) { throw "Stitching Blender native smoke or profile-boundary evidence is incomplete." }
if (-not $tests.Contains("GpuNamedPyramidHelpersPreserveUpstreamUnavailableError") -or -not $tests.Contains("CpuLaplacePyramidRoundTripsRoiInput") -or
    -not $sample.Contains("RunBlenderSummary") -or -not $guide.Contains("CUDA optimization is unavailable") -or
    -not $consumer.Contains("typeof(OpenCvSharp.Stitching.MultiBandBlender)")) { throw "Stitching Blender managed evidence is incomplete." }
if (-not $smoke.Contains("run_stitching_features_matcher_smoke") -or -not $smoke.Contains("features_matcher != reinterpret_cast") -or
    -not $matcherTests.Contains("BatchMaskAndRangeMatcherProduceExactRowMajorResults") -or -not $matcherTests.Contains("OrbComputesSingleBatchAndNonContinuousRoi") -or
    -not $sample.Contains("RunFeaturesMatcherSummary") -or -not $guide.Contains("LightGlue matching remains explicitly unsupported") -or
    -not $consumer.Contains("typeof(OpenCvSharp.Stitching.BestOf2NearestMatcher)")) { throw "Stitching matcher evidence is incomplete." }
if (-not $smoke.Contains("run_stitching_motion_estimator_smoke") -or -not $smoke.Contains("unchanged_estimator != reinterpret_cast") -or
    -not $motionTests.Contains("HomographyAndAffineEstimatorsReturnIndependentCameras") -or -not $motionTests.Contains("WaveGraphAndLargestComponentPreserveOwnership") -or
    -not $sample.Contains("RunMotionEstimatorSummary") -or -not $guide.Contains("The measured module result is 134 implemented, 22 missing") -or
    -not $consumer.Contains("typeof(OpenCvSharp.Stitching.BundleAdjusterAffinePartial)")) { throw "Stitching camera/motion evidence is incomplete." }

Write-Output "STITCHING_UPSTREAM_MAP_GUARD_OK declarations=207 high-level=24/21/21 public-warper=12/10/10 blender=28/24/24 exposure=53/45/45 matcher=23/16/14 camera-motion=31/20/20 implemented=134 missing=22 unsupported=2 fixtures=28"
