param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$ledgerRel = 'compatibility/v5.1.0-change-ledger.json'
$schemaRel = 'compatibility/v5.1.0-change-ledger.schema.json'
$ledgerPath = Join-Path $repo ($ledgerRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($ledgerPath,$schemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Change ledger file missing: $path" } }
if (-not (Test-Json -LiteralPath $ledgerPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'v5.1.0 change ledger failed JSON Schema validation.' }
$ledger = Get-Content -LiteralPath $ledgerPath -Raw | ConvertFrom-Json
foreach ($entry in @($ledger.entries)) {
    foreach ($evidence in @($entry.evidence)) {
        $path = Join-Path $repo ($evidence -replace '/', [IO.Path]::DirectorySeparatorChar)
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Change ledger evidence is missing for $($entry.id): $evidence" }
    }
    if ([string]$entry.surface -eq 'managed-api' -and [string]$entry.category -notin @('additive-safe','tfm-conditional-preview','behavior-fix')) { throw "Managed API ledger entry has invalid category: $($entry.id)" }
}
if ([int]$ledger.nativeAbiVersion -ne 1 -or [string]$ledger.openCvVersion -cne '5.1.0') { throw 'Change ledger OpenCV/ABI identity drifted.' }
$buildEvidencePath = Join-Path $repo 'packaging/runtime/opencv-5.1.0-source-build-evidence.json'
$build = Get-Content -LiteralPath $buildEvidencePath -Raw | ConvertFrom-Json
if ($build.status -cne 'verified-local-source-build' -or $build.openCvVersion -cne '5.1.0' -or $build.packageVersion -cne '5.1.0' -or $build.rid -cne 'win-x64') { throw '5.1 source build evidence identity drifted.' }
if ($build.upstream.openCv.commit -cne 'e4f78fbcf1a773fbed69e14500e8b734963eb585' -or $build.upstream.contrib.commit -cne 'b67a855188a3d683813731010f6d39ac614733a4') { throw '5.1 source evidence must retain both official tag commits.' }
if ($build.limits.published -or $build.limits.multiRid51Validated -or $build.limits.packageConsumer51Validated -or $build.limits.upstream51ParityClaimed -or $build.limits.upstreamMapVersion -cne '5.0.0') { throw 'Historical 5.0 evidence cannot establish 5.1 publication, platform or parity claims.' }
$expectedScopes = @('Directory.Build.props','src/OpenCvSharp.Native','src/OpenCvSharp','tests/OpenCvSharp.Tests','scripts/Build-OpenCV.ps1','scripts/Build-Native.ps1','scripts/Apply-OpenCvSourcePatches.ps1','scripts/New-RuntimeInputArtifact.ps1','scripts/Test-WindowsRuntimePeClosure.ps1')
if (($build.sourceInputSet.paths -join '|') -cne ($expectedScopes -join '|')) { throw '5.1 source input scope drifted.' }
$inputPaths = [string[]]@(& git -C $repo ls-files --cached --others --exclude-standard -- @expectedScopes | Sort-Object -Unique -CaseSensitive)
if ($LASTEXITCODE -ne 0) { throw 'Cannot enumerate 5.1 source input files.' }
[Array]::Sort($inputPaths, [StringComparer]::Ordinal)
$rows = foreach ($inputPath in $inputPaths) {
    $text = [IO.File]::ReadAllText((Join-Path $repo $inputPath))
    $normalized = (($text -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n"
    $bytes = [Text.UTF8Encoding]::new($false).GetBytes($normalized)
    $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
    "$inputPath|$($bytes.Length)|$hash"
}
$digest = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.UTF8Encoding]::new($false).GetBytes(($rows -join "`n") + "`n"))).ToLowerInvariant()
if ($inputPaths.Count -ne $build.sourceInputSet.fileCount -or $digest -cne $build.sourceInputSet.sha256) { throw '5.1 build evidence is stale for the current source input set.' }
if ($build.sourceCommit -cnotmatch '^[0-9a-f]{40}$') { throw '5.1 build source commit is missing or invalid.' }
& git -C $repo merge-base --is-ancestor $build.sourceCommit HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw '5.1 build source commit is not an ancestor of HEAD.' }
$sourceDrift = @(& git -C $repo diff --name-only $build.sourceCommit -- @expectedScopes)
if ($LASTEXITCODE -ne 0 -or $sourceDrift.Count -ne 0) { throw '5.1 source input scope differs from the committed build source.' }
if (@($build.wrapperProfiles).Count -ne 2) { throw '5.1 source build evidence requires Full and Mini wrappers.' }
foreach ($profile in $build.wrapperProfiles) {
    $exports = if ($profile.profile -ceq 'full') { 2669 } elseif ($profile.profile -ceq 'mini') { 527 } else { throw 'Unknown build profile.' }
    if ($profile.abiVersion -ne 1 -or $profile.exportCount -ne $exports -or $profile.ctest.passed -ne 3 -or $profile.ctest.failed -ne 0 -or $profile.wrapper.sha256 -cnotmatch '^[0-9a-f]{64}$') { throw '5.1 wrapper build/CTest/ABI evidence is incomplete.' }
    if ($profile.openCvInstallProfile -cne 'full' -or $profile.separateMinimalUpstreamBuild) { throw 'Mini evidence must identify its shared Full upstream installation.' }
}
if ((@($build.managedRegression.targetFramework) -join ',') -cne 'net8.0,net10.0') { throw '5.1 native-enabled managed evidence requires both frameworks.' }
foreach ($run in $build.managedRegression) {
    if (-not $run.nativeSmokeEnabled -or $run.nativeOpenCvVersion -cne '5.1.0' -or $run.passed -ne 2090 -or $run.failed -ne 0 -or $run.skipped -ne 0 -or @($run.requiredPassedTests).Count -ne 17) { throw '5.1 native-enabled regression evidence is incomplete.' }
    foreach ($name in @('OnnxImportPathsKeepAbiEngineValues','TFLiteImportReportsStableFormatAndRunsInference')) {
        if (@($run.requiredPassedTests | Where-Object { $_ -like "*.$name(*)" }).Count -ne 4) { throw '5.1 enum projection requires all four engine choices for ONNX and TFLite.' }
    }
    if (@($run.requiredPassedTests | Where-Object { $_ -like '*.SchurSolverSelectionPreservesDistortionModel(*)' }).Count -ne 4 -or
        @($run.requiredPassedTests | Where-Object { $_ -like '*.PropertiesExposeConstructorSettingsWhenFeaturesModuleIsLinked(*)' }).Count -ne 2) { throw '5.1 solver/model and BFMatcher property combinations are incomplete.' }
}
$patchHash = (Get-FileHash -LiteralPath (Join-Path $repo $build.patch.path) -Algorithm SHA256).Hash.ToLowerInvariant()
$dnn = $build.dnn51Correction
$dnnSummaryHash = (Get-FileHash -LiteralPath (Join-Path $repo 'compatibility/opencv-5.1.0/dnn-upstream-summary.json') -Algorithm SHA256).Hash.ToLowerInvariant()
& git -C $repo merge-base --is-ancestor $dnn.sourceCommit $build.sourceCommit 2>$null
if ($LASTEXITCODE -ne 0) { throw 'Historical DNN correction must be included in the current build source.' }
if (-not $dnn.abiValuesUnchanged -or $dnn.ortBackendRuntimeValidated -or
    $dnn.parserSlice.sha256 -cne $dnnSummaryHash -or $dnn.parserSlice.declarations -ne 185 -or $dnn.parserSlice.negativeFixtures -ne 22 -or
    $dnn.parserSlice.remainingHistoricalCatalogMaps -ne 72 -or $dnn.parserSlice.completeDnnCppParityClaimed -or
    $dnn.beforeRegression.total -ne 4 -or $dnn.beforeRegression.failed -ne 4 -or $dnn.beforeRegression.passed -ne 0 -or $dnn.managedBuild.targetFrameworkCount -ne 15) {
    throw '5.1 DNN correction or parser-slice evidence drifted.'
}
if ($patchHash -cne $build.patch.sha256 -or -not $build.patch.actualSourceIdempotencePassed -or $build.patch.negativeFixtureCount -ne 2 -or $build.runtimeInput.openCvVersion -cne '5.1.0' -or -not $build.runtimeInput.versionedPatchRecorded) { throw '5.1 CCM patch or runtime input provenance evidence drifted.' }
$refresh = $build.upstream51ParserRefresh
$inventoryPath = 'compatibility/opencv-5.1.0/upstream-map-refresh-summary.json'
$catalogPath = 'compatibility/opencv-5.1.0/upstream-map-refresh-catalog.json'
$inventoryHash = (Get-FileHash -LiteralPath (Join-Path $repo $inventoryPath) -Algorithm SHA256).Hash.ToLowerInvariant()
$catalogHash = (Get-FileHash -LiteralPath (Join-Path $repo $catalogPath) -Algorithm SHA256).Hash.ToLowerInvariant()
if (-not $refresh.sourceBuildInputsChanged -or $refresh.buildSourceCommit -cne $build.sourceCommit -or
    $refresh.catalog.path -cne $catalogPath -or $refresh.catalog.sha256 -cne $catalogHash -or $refresh.inventory.path -cne $inventoryPath -or $refresh.inventory.sha256 -cne $inventoryHash -or
    $refresh.refreshedParserSliceCount -ne 5 -or $refresh.pendingCatalogMapCount -ne 68 -or $refresh.historicalCatalogVersion -cne '5.0.0' -or
    $refresh.newDeclarationCount -ne 208 -or $refresh.totalReviewedDeclarationCount -ne 914 -or $refresh.sharedGeneratorNegativeFixtureCount -ne 127 -or
    $refresh.newManagedApiOrNativeAbiAdded -or -not $refresh.newRuntimeBehaviorCertified -or $refresh.repositoryWideUpstreamParityClaimed) { throw '5.1 shared parser refresh evidence drifted.' }
$calibration = $build.calibration51Correction
$calibSummary = Get-Content -LiteralPath (Join-Path $repo 'compatibility/opencv-5.1.0/calib3d-upstream-summary.json') -Raw | ConvertFrom-Json
if ($calibration.sourceCommit -cne $build.sourceCommit -or $calibration.disableSchurComplement -ne 134217728 -or $calibration.tiltedModel -ne 262144 -or
    -not $calibration.callerRecompileRequired -or -not $calibration.nativeBinariesReusedUnchanged -or $calibration.publicSignatureOrExportAdded -or
    $calibration.beforeRegression.total -ne 4 -or $calibration.beforeRegression.failed -ne 1 -or $calibration.beforeRegression.passed -ne 3 -or
    $calibration.bfMatcherMasksWithCrossCheck -or -not $calibration.bfMatcherMasksWithoutCrossCheck -or
    $calibSummary.declarationCount -ne 208 -or $calibSummary.classificationCounts.implemented -ne 169 -or $calibSummary.classificationCounts.missing -ne 0 -or
    $calibSummary.reviewedExistingBindingCount -ne 3 -or $calibSummary.negativeFixtureCount -ne 37) { throw '5.1 calibration/matcher correction or reviewed binding evidence drifted.' }
Write-Host "V510_CHANGE_LEDGER_OK entries=$(@($ledger.entries).Count) from=$($ledger.fromVersion) to=$($ledger.toVersion) native_abi=$($ledger.nativeAbiVersion)"
Write-Host "OPENCV_510_SOURCE_BUILD_EVIDENCE_OK input_files=$($inputPaths.Count) full_exports=2669 mini_exports=527 native_ctest=6 managed=4180 historical_catalog=5.0.0 parser_slices=5 calibration_schur_bit=27"
