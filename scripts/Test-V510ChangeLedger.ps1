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
if ($build.limits.published -or $build.limits.multiRid51Validated -or -not $build.limits.packageConsumer51Validated -or -not $build.limits.packagePeClosureValidated -or -not $build.limits.localPeClosureValidated -or $build.limits.upstream51ParityClaimed -or $build.limits.upstreamMapVersion -cne '5.0.0') { throw '5.1 evidence must keep publication, multi-RID and parity boundaries while recording the verified local package consumer and package PE closure.' }
$packageConsumerEvidenceRel = 'packaging/runtime/opencv-5.1.0-package-consumer-evidence.json'
$packageConsumerEvidencePath = Join-Path $repo $packageConsumerEvidenceRel
if (-not (Test-Path -LiteralPath $packageConsumerEvidencePath -PathType Leaf)) { throw '5.1 package consumer evidence is missing.' }
$packageConsumer = Get-Content -LiteralPath $packageConsumerEvidencePath -Raw | ConvertFrom-Json
$packageConsumerHash = (Get-FileHash -LiteralPath $packageConsumerEvidencePath -Algorithm SHA256).Hash.ToLowerInvariant()
$recordedPackageConsumer = $build.packageConsumer51Evidence
if ($recordedPackageConsumer.path -cne $packageConsumerEvidenceRel -or $recordedPackageConsumer.sha256 -cne $packageConsumerHash -or
    $recordedPackageConsumer.status -cne 'verified-local-windows-x64' -or $recordedPackageConsumer.packageCount -ne 3 -or
    $recordedPackageConsumer.consumerCount -ne 2 -or (@($recordedPackageConsumer.profiles) -join '|') -cne 'full|mini' -or
    $recordedPackageConsumer.guard -cne 'scripts/Test-GitHubPackConsumerRestoreSurface.ps1') { throw '5.1 package consumer evidence binding drifted.' }
if ($packageConsumer.schemaVersion -ne 1 -or $packageConsumer.recordKind -cne 'opencv-5.1.0-package-consumer-evidence' -or
    $packageConsumer.status -cne 'verified-local-windows-x64' -or $packageConsumer.openCvVersion -cne '5.1.0' -or
    $packageConsumer.packageVersion -cne '5.1.0.0' -or $packageConsumer.rid -cne 'win-x64' -or
    $packageConsumer.configuration -cne 'Release' -or $packageConsumer.runner -cne 'local-windows-x64' -or
    $packageConsumer.sourceCommit -notmatch '^[0-9a-f]{40}$' -or $packageConsumer.nativeBuildSourceCommit -notmatch '^[0-9a-f]{40}$' -or
    $packageConsumer.dotnetSdk -notmatch '^10\.0\.\d+$' -or $packageConsumer.guard -cne 'scripts/Test-GitHubPackConsumerRestoreSurface.ps1') { throw '5.1 package consumer identity is incomplete.' }
foreach ($revision in @($packageConsumer.sourceCommit, $packageConsumer.nativeBuildSourceCommit)) {
    & git -C $repo merge-base --is-ancestor $revision HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw "5.1 package consumer source revision is not an ancestor of HEAD: $revision" }
}
$packageLabels = @($packageConsumer.packages | ForEach-Object { [string]$_.label })
if (($packageLabels -join '|') -cne 'managed|full|mini') { throw '5.1 package consumer package set drifted.' }
foreach ($package in @($packageConsumer.packages)) {
    if ($package.nugetVersion -cne '5.1.0' -or $package.packageVersion -cne '5.1.0.0' -or
        $package.sha256 -notmatch '^[0-9a-f]{64}$' -or [int]$package.entryCount -le 0 -or [int]$package.nativeFiles -lt 0) { throw "5.1 package consumer package record is incomplete: $($package.label)" }
}
if ([int]$packageConsumer.packages[0].entryCount -ne 8 -or [int]$packageConsumer.packages[1].entryCount -ne 53 -or
    [int]$packageConsumer.packages[1].nativeFiles -ne 43 -or [int]$packageConsumer.packages[2].entryCount -ne 17 -or
    [int]$packageConsumer.packages[2].nativeFiles -ne 7) { throw '5.1 package entry/native asset counts drifted.' }
if (@($packageConsumer.consumers).Count -ne 2 -or (@($packageConsumer.consumers.profile) -join '|') -cne 'full|mini' -or
    @($packageConsumer.consumers | Where-Object { $_.rid -ne 'win-x64' -or $_.targetFramework -ne 'net8.0' -or $_.restore -ne 'passed' -or $_.build -ne 'passed' -or $_.nativeSmoke -ne 'passed' }).Count -ne 0) { throw '5.1 package consumer restore/build/native smoke evidence is incomplete.' }
$packagePeClosureEvidenceRel = 'packaging/runtime/opencv-5.1.0-package-pe-closure-evidence.json'
$packagePeClosureEvidencePath = Join-Path $repo $packagePeClosureEvidenceRel
if (-not (Test-Path -LiteralPath $packagePeClosureEvidencePath -PathType Leaf)) { throw '5.1 package PE closure evidence is missing.' }
$packagePeClosure = Get-Content -LiteralPath $packagePeClosureEvidencePath -Raw | ConvertFrom-Json
$packagePeClosureHash = (Get-FileHash -LiteralPath $packagePeClosureEvidencePath -Algorithm SHA256).Hash.ToLowerInvariant()
$recordedPackagePeClosure = $build.packagePeClosure51Evidence
if ($recordedPackagePeClosure.path -cne $packagePeClosureEvidenceRel -or $recordedPackagePeClosure.sha256 -cne $packagePeClosureHash -or $recordedPackagePeClosure.status -cne 'verified-local-windows-x64' -or
    $recordedPackagePeClosure.sourceCommit -cne $packageConsumer.sourceCommit -or $recordedPackagePeClosure.nativeBuildSourceCommit -cne $packageConsumer.nativeBuildSourceCommit -or
    $recordedPackagePeClosure.packageCount -ne 2 -or (@($recordedPackagePeClosure.profiles) -join '|') -cne 'full|mini' -or
    $recordedPackagePeClosure.guard -cne 'scripts/Test-PackagedWindowsRuntimePeClosure.ps1') { throw '5.1 package PE closure evidence binding drifted.' }
if ($packagePeClosure.schemaVersion -ne 1 -or $packagePeClosure.recordKind -cne 'opencv-5.1.0-package-pe-closure-evidence' -or
    $packagePeClosure.status -cne 'verified-local-windows-x64' -or $packagePeClosure.openCvVersion -cne '5.1.0' -or
    $packagePeClosure.packageVersion -cne '5.1.0' -or $packagePeClosure.rid -ne 'win-x64' -or
    $packagePeClosure.guard -cne 'scripts/Test-PackagedWindowsRuntimePeClosure.ps1' -or
    $packagePeClosure.dependencyGuard -cne 'scripts/Test-WindowsRuntimePeClosure.ps1' -or
    $packagePeClosure.sourceCommit -cne $packageConsumer.sourceCommit -or $packagePeClosure.nativeBuildSourceCommit -cne $packageConsumer.nativeBuildSourceCommit) { throw '5.1 package PE closure identity is incomplete.' }
$packagePeRows = @($packagePeClosure.packages)
if ($packagePeRows.Count -ne 2 -or (@($packagePeRows.label) -join '|') -cne 'full|mini') { throw '5.1 package PE closure package set drifted.' }
foreach ($peRow in $packagePeRows) {
    $consumerPackage = @($packageConsumer.packages | Where-Object { $_.label -ceq $peRow.label })
    if ($consumerPackage.Count -ne 1 -or $peRow.packageId -cne $consumerPackage[0].packageId -or
        $peRow.packageVersion -cne $consumerPackage[0].packageVersion -or $peRow.nugetVersion -cne $consumerPackage[0].nugetVersion -or
        $peRow.nupkgSha256 -cne $consumerPackage[0].sha256 -or [int]$peRow.nativeFiles -ne [int]$consumerPackage[0].nativeFiles -or
        @($peRow.nativeFilesSha256).Count -ne [int]$consumerPackage[0].nativeFiles) { throw "5.1 package PE closure payload binding drifted: $($peRow.label)" }
    if ([string]$peRow.audit.machine -cne 'AMD64' -or [int]$peRow.audit.missingOpenCvImports -ne 0 -or
        [int]$peRow.audit.files -ne [int]$peRow.nativeFiles -or [int]$peRow.audit.packagedModules -ne ([int]$consumerPackage[0].nativeFiles - 1) -or
        [int]$peRow.audit.reachableModules -ne [int]$peRow.audit.packagedModules -or [int]$peRow.audit.loaderOpenCvImports -le 0 -or
        [int]$peRow.audit.opencvImportEdges -le 0) { throw "5.1 package PE closure audit result is incomplete: $($peRow.label)" }
}
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
if ($refresh.sourceBuildInputsChanged -or $refresh.buildSourceCommit -cne $build.sourceCommit -or
    $refresh.catalog.path -cne $catalogPath -or $refresh.catalog.sha256 -cne $catalogHash -or $refresh.inventory.path -cne $inventoryPath -or $refresh.inventory.sha256 -cne $inventoryHash -or
    $refresh.refreshedParserSliceCount -ne 73 -or $refresh.pendingCatalogMapCount -ne 0 -or $refresh.historicalCatalogVersion -cne '5.0.0' -or
    $refresh.newDeclarationCount -ne 1727 -or $refresh.totalReviewedDeclarationCount -ne 5778 -or $refresh.sharedGeneratorNegativeFixtureCount -ne 2420 -or
    $refresh.newManagedApiOrNativeAbiAdded -or $refresh.newRuntimeBehaviorCertified -or $refresh.repositoryWideUpstreamParityClaimed) { throw '5.1 shared parser refresh evidence drifted.' }
$contribReview = $build.contribModule51Review
if ($contribReview.status -cne 'verified-parser-review' -or
    (@($contribReview.modules) -join '|') -cne 'xphoto|ximgproc|optflow|bgsegm|face|quality|img_hash|line_descriptor' -or
    $contribReview.newDeclarations -ne 611 -or $contribReview.newCallables -ne 511 -or
    $contribReview.implemented -ne 445 -or $contribReview.omitted -ne 66 -or $contribReview.metadata -ne 100 -or
    $contribReview.currentParserScopes -ne 22 -or $contribReview.totalDeclarations -ne 2825 -or $contribReview.remainingHistoricalCatalogMaps -ne 51 -or
    $contribReview.newScopeNegativeFixtures -ne 257 -or $contribReview.sharedGeneratorNegativeFixtures -ne 675 -or
    $contribReview.regeneratedArtifacts -ne 127 -or $contribReview.unchangedPriorArtifacts -ne 84 -or $contribReview.officialSourceInputs -ne 131 -or
    $contribReview.actualCliNegativeFixtures -ne 28 -or -not $contribReview.inheritedBindingSetsPreserved -or
    -not $contribReview.managedBinariesReusedUnchanged -or -not $contribReview.nativeBinariesReusedUnchanged -or
    $contribReview.newPublicMemberOrNativeExport -or $contribReview.newRuntimeBehaviorCertified) { throw '5.1 contrib parser review evidence drifted.' }
$imageReview = $build.imageGeometry51Review
if ($imageReview.status -cne 'verified-parser-review' -or
    (@($imageReview.modules) -join '|') -cne 'freetype|alphamat|intensity_transform|plot|bioinspired|phase_unwrapping|hfs|fuzzy|rapid|shape|structured_light|saliency|surface_matching|reg' -or
    $imageReview.newDeclarations -ne 346 -or $imageReview.newCallables -ne 289 -or
    $imageReview.implemented -ne 222 -or $imageReview.omitted -ne 67 -or $imageReview.metadata -ne 57 -or
    $imageReview.currentParserScopes -ne 36 -or $imageReview.totalDeclarations -ne 3171 -or $imageReview.remainingHistoricalCatalogMaps -ne 37 -or
    $imageReview.newScopeNegativeFixtures -ne 448 -or $imageReview.sharedGeneratorNegativeFixtures -ne 1123 -or
    $imageReview.regeneratedArtifacts -ne 211 -or $imageReview.unchangedPriorArtifacts -ne 132 -or $imageReview.officialSourceInputs -ne 180 -or
    $imageReview.actualCliNegativeFixtures -ne 42 -or -not $imageReview.inheritedBindingSetsPreserved -or
    -not $imageReview.managedBinariesReusedUnchanged -or -not $imageReview.nativeBinariesReusedUnchanged -or
    $imageReview.newPublicMemberOrNativeExport -or $imageReview.newRuntimeBehaviorCertified) { throw '5.1 image/geometry parser review evidence drifted.' }
$publicReview = $build.publicModules51Review
if ($publicReview.status -cne 'verified-parser-review' -or
    (@($publicReview.modules) -join '|') -cne 'xstereo|ptcloud|xobjdetect|wechat_qrcode|dnn_superres|cnn_3dobj|signal|dpm|hdf|text|xfeatures2d|dnn_objdetect|ccalib|superres|julia|rgbd|videostab|cvv|datasets' -or
    $publicReview.newDeclarations -ne 880 -or $publicReview.newCallables -ne 681 -or
    $publicReview.implemented -ne 47 -or $publicReview.omitted -ne 634 -or $publicReview.metadata -ne 199 -or
    $publicReview.currentParserScopes -ne 55 -or $publicReview.totalDeclarations -ne 4051 -or $publicReview.remainingHistoricalCatalogMaps -ne 18 -or
    $publicReview.newScopeNegativeFixtures -ne 630 -or $publicReview.sharedGeneratorNegativeFixtures -ne 1753 -or
    $publicReview.regeneratedArtifacts -ne 325 -or $publicReview.unchangedPriorArtifacts -ne 216 -or $publicReview.officialSourceInputs -ne 274 -or
    $publicReview.actualCliNegativeFixtures -ne 61 -or -not $publicReview.inheritedBindingSetsPreserved -or
    -not $publicReview.cvvBuildConditionReviewed -or -not $publicReview.rgbdSignatureReplacementReviewed -or
    -not $publicReview.managedBinariesReusedUnchanged -or -not $publicReview.nativeBinariesReusedUnchanged -or
    $publicReview.newPublicMemberOrNativeExport -or $publicReview.newRuntimeBehaviorCertified) { throw '5.1 public module parser review evidence drifted.' }
$backendReview = $build.backendModules51Review
if ($backendReview.status -cne 'verified-parser-review' -or
    (@($backendReview.modules) -join '|') -cne 'cannops|cudaarithm|cudabgsegm|cudacodec|cudafeatures2d|cudafilters|cudaimgproc|cudalegacy|cudaobjdetect|cudaoptflow|cudastereo|cudawarping|cudev|fastcv|matlab|ovis|sfm|viz' -or
    $backendReview.newDeclarations -ne 1727 -or $backendReview.newCallables -ne 1520 -or $backendReview.implemented -ne 0 -or $backendReview.omitted -ne 1520 -or $backendReview.metadata -ne 207 -or
    $backendReview.currentParserScopes -ne 73 -or $backendReview.totalDeclarations -ne 5778 -or $backendReview.remainingHistoricalCatalogMaps -ne 0 -or
    $backendReview.newScopeNegativeFixtures -ne 667 -or $backendReview.sharedGeneratorNegativeFixtures -ne 2420 -or $backendReview.sourceSemanticNegativeFixtures -ne 64 -or
    $backendReview.regeneratedArtifacts -ne 437 -or $backendReview.unchangedPriorArtifacts -ne 330 -or $backendReview.officialSourceInputs -ne 467 -or
    $backendReview.cudevSourceCallables -ne 5463 -or $backendReview.matlabSourceCallables -ne 242 -or
    -not $backendReview.inheritedBindingSetsPreserved -or -not $backendReview.specializedStrategiesPreserved -or
    -not $backendReview.managedBinariesReusedUnchanged -or -not $backendReview.nativeBinariesReusedUnchanged -or
    $backendReview.newPublicMemberOrNativeExport -or $backendReview.newRuntimeBehaviorCertified) { throw '5.1 backend/dependency parser review evidence drifted.' }
$calibration = $build.calibration51Correction
$calibSummary = Get-Content -LiteralPath (Join-Path $repo 'compatibility/opencv-5.1.0/calib3d-upstream-summary.json') -Raw | ConvertFrom-Json
if ($calibration.sourceCommit -cne $build.sourceCommit -or $calibration.disableSchurComplement -ne 134217728 -or $calibration.tiltedModel -ne 262144 -or
    -not $calibration.callerRecompileRequired -or -not $calibration.nativeBinariesReusedUnchanged -or $calibration.publicSignatureOrExportAdded -or
    $calibration.beforeRegression.total -ne 4 -or $calibration.beforeRegression.failed -ne 1 -or $calibration.beforeRegression.passed -ne 3 -or
    $calibration.bfMatcherMasksWithCrossCheck -or -not $calibration.bfMatcherMasksWithoutCrossCheck -or
    $calibSummary.declarationCount -ne 208 -or $calibSummary.classificationCounts.implemented -ne 169 -or $calibSummary.classificationCounts.missing -ne 0 -or
    $calibSummary.reviewedExistingBindingCount -ne 3 -or $calibSummary.negativeFixtureCount -ne 37) { throw '5.1 calibration/matcher correction or reviewed binding evidence drifted.' }
$headless = Get-Content (Join-Path $repo 'packaging/runtime/opencv-5.1.0-local-linux-headless-evidence.json') -Raw | ConvertFrom-Json
$headlessEntry = @($ledger.entries | Where-Object id -CEQ 'V510-LOCAL-LINUX-HEADLESS')
if ($headlessEntry.Count -ne 1 -or $headlessEntry[0].evidence -cnotcontains 'scripts/Test-Linux51HeadlessEvidence.ps1' -or
    $headless.status -cne 'verified-local-three-distro-x64-candidate' -or -not $headless.candidateOnly -or $headless.publicationAllowed -or $headless.activePackageIdentityAdded -or
    $headless.coverage.managedExecutions -ne 12 -or $headless.coverage.nativeCtestPassed -ne 6 -or $headless.coverage.freshMinimalUpstreamBuild -or
    $headless.coverage.cameraOrNetworkValidated -or $headless.coverage.hostedValidated) { throw '5.1 local headless candidate evidence/ledger scope drifted.' }
$thirdParty = Get-Content (Join-Path $repo 'packaging/release/opencv-5.1.0-linux-thirdparty-evidence.json') -Raw | ConvertFrom-Json
$thirdPartyEntry = @($ledger.entries | Where-Object id -CEQ 'V510-LOCAL-LINUX-THIRD-PARTY')
if ($thirdPartyEntry.Count -ne 1 -or $thirdPartyEntry[0].evidence -cnotcontains 'scripts/Test-Linux51ThirdPartyEvidence.ps1' -or
    $thirdParty.coverage.namedComponents -ne 15 -or $thirdParty.coverage.candidatePackages -ne 4 -or $thirdParty.coverage.consumerExecutions -ne 24 -or
    -not $thirdParty.candidateOnly -or $thirdParty.publicationAllowed -or $thirdParty.activeIdentityAdded -or $thirdParty.coverage.nativePayloadChanged -or
    $thirdParty.coverage.completeSourceLicenseReview -or $thirdParty.coverage.vulnerabilityReviewComplete -or $thirdParty.coverage.releaseApproved) { throw '5.1 third-party candidate supplement evidence/ledger scope drifted.' }
Write-Host "V510_CHANGE_LEDGER_OK entries=$(@($ledger.entries).Count) from=$($ledger.fromVersion) to=$($ledger.toVersion) native_abi=$($ledger.nativeAbiVersion)"
Write-Host "OPENCV_510_SOURCE_BUILD_EVIDENCE_OK input_files=$($inputPaths.Count) full_exports=2669 mini_exports=527 native_ctest=6 managed=4180 historical_catalog=5.0.0 parser_slices=73 calibration_schur_bit=27"
