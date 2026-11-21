param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [int]$TimeoutSeconds = 120
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($TimeoutSeconds -lt 10 -or $TimeoutSeconds -gt 600) { throw 'TimeoutSeconds must be between 10 and 600.' }

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$dotnetCommand = Get-Command dotnet -ErrorAction Stop
$project = Join-Path $repo 'tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) { throw "Test project was not found: $project" }
$manifestPath = Join-Path $repo 'packaging/codec/image-preflight-mutation-corpus.json'
$schemaPath = Join-Path $repo 'packaging/codec/image-preflight-mutation-corpus.schema.json'
if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf) -or -not (Test-Path -LiteralPath $schemaPath -PathType Leaf)) { throw 'Image preflight mutation corpus manifest/schema is missing.' }
if (-not (Test-Json -LiteralPath $manifestPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Image preflight mutation corpus manifest failed JSON Schema validation.' }
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$testText = Get-Content -LiteralPath (Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/ImagePreflightTests.cs') -Raw
foreach ($fixture in @($manifest.fixtures)) {
    if ($testText.IndexOf('Name = "' + [string]$fixture + '"', [StringComparison]::Ordinal) -lt 0) { throw "Mutation fixture is missing from the focused test: $fixture" }
}
if ($testText.IndexOf('IdentifyDeterministicMutationCorpusPreservesSeekAndConsumesNonSeekStreams', [StringComparison]::Ordinal) -lt 0) { throw 'Stream mutation test is missing from the focused test.' }
$gifTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/GifMetadataPreflightTests.cs'
if (-not (Test-Path -LiteralPath $gifTestPath -PathType Leaf)) { throw 'GIF metadata admission tests are missing.' }
$avifTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/AvifPrimaryItemPreflightTests.cs'
if (-not (Test-Path -LiteralPath $avifTestPath -PathType Leaf)) { throw 'AVIF primary item admission tests are missing.' }
$jpegTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/JpegMarkerPreflightTests.cs'
if (-not (Test-Path -LiteralPath $jpegTestPath -PathType Leaf)) { throw 'JPEG marker admission tests are missing.' }
$apngTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/ApngFramePreflightTests.cs'
if (-not (Test-Path -LiteralPath $apngTestPath -PathType Leaf)) { throw 'APNG frame admission tests are missing.' }
$webpTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/WebpContainerPreflightTests.cs'
if (-not (Test-Path -LiteralPath $webpTestPath -PathType Leaf)) { throw 'WebP container admission tests are missing.' }
$tiffTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/TiffDirectoryPreflightTests.cs'
if (-not (Test-Path -LiteralPath $tiffTestPath -PathType Leaf)) { throw 'TIFF directory admission tests are missing.' }
$jpeg2000TestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/Jpeg2000HeaderPreflightTests.cs'
if (-not (Test-Path -LiteralPath $jpeg2000TestPath -PathType Leaf)) { throw 'JPEG 2000 header admission tests are missing.' }
$openExrTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/OpenExrHeaderPreflightTests.cs'
if (-not (Test-Path -LiteralPath $openExrTestPath -PathType Leaf)) { throw 'OpenEXR first-header admission tests are missing.' }
$bmpTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/BmpHeaderPreflightTests.cs'
if (-not (Test-Path -LiteralPath $bmpTestPath -PathType Leaf)) { throw 'BMP DIB/palette admission tests are missing.' }
$pngTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/PngHeaderPreflightTests.cs'
if (-not (Test-Path -LiteralPath $pngTestPath -PathType Leaf)) { throw 'PNG IHDR/chunk admission tests are missing.' }
$pamTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/PamHeaderPreflightTests.cs'
if (-not (Test-Path -LiteralPath $pamTestPath -PathType Leaf)) { throw 'PAM header admission tests are missing.' }
$pnmTestPath = Join-Path $repo 'tests/OpenCvSharp.Tests/ImgCodecs/PnmHeaderPreflightTests.cs'
if (-not (Test-Path -LiteralPath $pnmTestPath -PathType Leaf)) { throw 'PNM text/raster admission tests are missing.' }
if (@($manifest.frameworks).Count -ne 2 -or @($manifest.replacementBytes).Count -ne 5 -or @($manifest.fixtures).Count -lt 13) { throw 'Mutation corpus manifest dimensions drifted.' }

$frameworks = @('net8.0', 'net10.0')
foreach ($framework in $frameworks) {
    $resultsDirectory = Join-Path ([IO.Path]::GetTempPath()) ('opencv-preflight-tests-' + [Guid]::NewGuid().ToString('N'))
    $trxPath = Join-Path $resultsDirectory 'preflight.trx'
    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $dotnetCommand.Source
    $startInfo.WorkingDirectory = $repo
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in @(
            'test', $project, '-c', 'Release', '-f', $framework, '--no-restore',
            '--filter', 'FullyQualifiedName~IdentifyDeterministicMutationCorpus|FullyQualifiedName~GifMetadataPreflightTests|FullyQualifiedName~AvifPrimaryItemPreflightTests|FullyQualifiedName~JpegMarkerPreflightTests|FullyQualifiedName~ApngFramePreflightTests|FullyQualifiedName~WebpContainerPreflightTests|FullyQualifiedName~TiffDirectoryPreflightTests|FullyQualifiedName~Jpeg2000HeaderPreflightTests|FullyQualifiedName~OpenExrHeaderPreflightTests|FullyQualifiedName~BmpHeaderPreflightTests|FullyQualifiedName~PngHeaderPreflightTests|FullyQualifiedName~PamHeaderPreflightTests|FullyQualifiedName~PnmHeaderPreflightTests',
            '--results-directory', $resultsDirectory, '--logger', 'trx;LogFileName=preflight.trx',
            '--logger', 'console;verbosity=minimal')) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    if (-not $process.Start()) { throw "Could not start the image preflight mutation corpus on $framework." }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        try { $process.Kill($true) } catch { }
        throw "Image preflight mutation corpus timed out on $framework after $TimeoutSeconds seconds."
    }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    if (-not [string]::IsNullOrWhiteSpace($stdout)) { Write-Host $stdout.TrimEnd() }
    if (-not [string]::IsNullOrWhiteSpace($stderr)) { Write-Host $stderr.TrimEnd() }
    if ($process.ExitCode -ne 0) { throw "Image preflight mutation corpus failed on $framework with exit code $($process.ExitCode)." }
    if (-not (Test-Path -LiteralPath $trxPath -PathType Leaf)) { throw "Image preflight tests produced no TRX execution evidence on $framework; an empty dotnet test invocation is not a pass." }
    $trx = [xml]([IO.File]::ReadAllText($trxPath))
    $counters = $trx.SelectSingleNode("//*[local-name()='Counters']")
    $results = @($trx.SelectNodes("//*[local-name()='UnitTestResult']"))
    if ($null -eq $counters -or [int]$counters.executed -lt 3 -or [int]$counters.executed -ne [int]$counters.passed -or [int]$counters.total -ne [int]$counters.executed) { throw "Image preflight tests were missing, skipped, or unsuccessful on $framework." }
    foreach ($requiredTest in @(
            'IdentifyDeterministicMutationCorpusNeverEscapesParserOrPublishesInvalidFacts',
            'IdentifyDeterministicMutationCorpusPreservesSeekAndConsumesNonSeekStreams',
            'GifMetadataBudgetsRejectBeforeNativeDecodeAndAcceptExactStoredLengths',
            'GifStrictMetadataPolicyRejectsUnknownLengthsWhilePermissiveIdentifyPreservesThem',
            'IdentifyDoesNotUseUnassociatedAvifProperties',
            'IdentifyUsesOnlyPropertiesOfThePrimaryAvifItem',
            'AvifPrimaryItemLimitsRejectBeforeNativeDecodeAndPreserveStreamPosition',
            'IdentifyDoesNotReadPastTruncatedJpegComponentTable',
            'IdentifyDoesNotTreatJpegMetadataPayloadAsEndOfImage',
            'IdentifyAcceptsOutOfOrderJpegIccFragments',
            'JpegMetadataBudgetsRejectBeforeNativeDecodeAndAcceptExactStoredLengths',
            'JpegStrictMetadataPolicyRejectsUnknownLengthsAndRestoresSeekPosition',
            'IdentifyDeterministicMutationCorpusJpegMarkersNeverEscapeOrPublishUnboundedFacts',
            'IdentifyDoesNotCountApngFramesOutsideTheCanvas',
            'IdentifyRequiresApngDefaultImageDataBeforeFrameData',
            'IdentifyNeverMixesApngIdatAndFdatInOneFrame',
            'IdentifyRequiresDefaultApngFrameToMatchTheWholeCanvas',
            'IdentifyRequiresConsecutiveApngDefaultImageChunks',
            'IdentifyDeterministicMutationCorpusApngFramesNeverEscapeOrPublishInvalidCanvasBudgets',
            'IdentifyReadsTheActualSimpleLossyWebpDimensionOffsets',
            'IdentifyReadsSimpleLosslessWebpDimensions',
            'IdentifyNeverCountsHeaderOnlyWebpAnimationFrames',
            'IdentifyNeverCountsWebpFramesOutsideTheCanvas',
            'IdentifyRequiresWebpFrameAndBitstreamDimensionsToAgree',
            'IdentifyNeverMixesStaticAndAnimatedWebpImageData',
            'WebpCanvasBudgetsRejectBeforeNativeDecodeAndPreserveStreams',
            'IdentifyDeterministicMutationCorpusWebpContainersNeverEscapeOrPublishUnboundedCanvasFacts',
            'IdentifyRejectsDuplicateTiffAdmissionTagsInBothDirectoryFormats',
            'TiffStrictPixelPolicyRejectsAmbiguousSamplesAndDuplicates',
            'IdentifyDeterministicMutationCorpusTiffDirectoriesNeverEscapeOrPublishInconsistentBudgets',
            'IdentifyRequiresJp2ImageHeaderComponentsToAgree',
            'IdentifyRequiresJp2PerComponentPrecisionToAgree',
            'IdentifyRejectsDuplicateJp2CodestreamBoxes',
            'IdentifyNeverIgnoresMalformedJp2BoxesAfterCodestream',
            'Jpeg2000HeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams',
            'Jpeg2000StrictPolicyRejectsConflictingHeadersAndMixedPrecision',
            'IdentifyDeterministicMutationCorpusJpeg2000NeverEscapesOrPublishesConflictingHeaderFacts',
            'OpenExrHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams',
            'OpenExrStrictPolicyRejectsConflictingOrUnknownPixelFacts',
            'IdentifyRequiresCompleteFirstOpenExrHeaderBeforePublishingFacts',
            'IdentifyDoesNotApplyLaterOpenExrDeepPartsToTheFirstScanlinePart',
            'IdentifyDeterministicMutationCorpusOpenExrNeverEscapesOrPublishesConflictingHeaderFacts',
            'IdentifyDoesNotInterpretUnsignedOs2BmpHeightAsTopDown',
            'BmpHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams',
            'BmpStrictPolicyRejectsUnknownLayoutsAndConflictingStorageFacts',
            'IdentifyRequiresCompleteBmpDibAndExactPayloadBeforePublishingFrameFacts',
            'IdentifyDeterministicMutationCorpusBmpNeverEscapesOrPublishesInconsistentStorageFacts',
            'IdentifyRequiresInitialPngIhdrBeforeMetadataCompletion',
            'PngHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams',
            'PngStrictPolicyRejectsUnknownHeaderOrMetadataFacts',
            'IdentifyCountsPngStoredMetadataWithoutInflatingProfilesOrText',
            'IdentifyDeterministicMutationCorpusPngNeverEscapesOrPublishesInconsistentHeaderFacts',
            'PamHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams',
            'PamStrictPolicyRejectsAmbiguousHeadersBeforeNativeDecode',
            'IdentifyAcceptsUnorderedPamFieldsCommentsBlankLinesAndRepeatedCustomTupleTypes',
            'IdentifyRequiresExactPamPayloadButPreservesTheCompleteHeader',
            'IdentifyDeterministicMutationCorpusPamNeverEscapesOrPublishesAmbiguousHeaderFacts',
            'PnmHeaderBudgetsRejectBeforeNativeDecodeAndPreserveStreams',
            'PnmStrictPolicyRejectsUnknownHeaderOrMetadataBeforeNativeDecode',
            'IdentifyPreservesEveryRawPnmFirstByteAfterEachSingleWhitespaceDelimiter',
            'IdentifyRequiresExactStandalonePnmRasterAndPreservesProvenHeaders',
            'IdentifyDeterministicMutationCorpusPnmNeverEscapesOrPublishesInconsistentRasterFacts')) {
        if (@($results | Where-Object { ([string]$_.testName).Contains($requiredTest) -and [string]$_.outcome -ceq 'Passed' }).Count -ne 1) { throw "Required image preflight test did not pass on ${framework}: $requiredTest" }
    }
    foreach ($requiredRegression in @(
            'IdentifyNeverLetsDuplicateTiffDimensionsOverrideTheAdmissionBudget',
            'IdentifyNeverLetsDuplicateTiffChannelsOverrideTheAdmissionBudget',
            'IdentifyRequiresTiffPerSampleBitCountsToMatchChannels')) {
        if (@($results | Where-Object { ([string]$_.testName).Contains($requiredRegression) -and [string]$_.outcome -ceq 'Passed' }).Count -ne 4) { throw "All four TIFF directory/byte-order regression cases must pass on ${framework}: $requiredRegression" }
    }
    foreach ($requiredRegression in @(
            'IdentifyRequiresJp2ImageHeaderAndCodestreamGeometryToAgree',
            'IdentifyRequiresJp2ConstantPrecisionAndSignToAgree',
            'IdentifyRejectsJpeg2000PrecisionOutsidePartOneRange',
            'IdentifyRejectsJpeg2000TileGridsAbove65535')) {
        if (@($results | Where-Object { ([string]$_.testName).Contains($requiredRegression) -and [string]$_.outcome -ceq 'Passed' }).Count -ne 2) { throw "Both JPEG 2000 regression cases must pass on ${framework}: $requiredRegression" }
    }
    if (@($results | Where-Object { ([string]$_.testName).Contains('IdentifyReadsRealLosslessJpeg2000Fixtures') -and [string]$_.outcome -ceq 'Passed' }).Count -ne 5) { throw "All five real JPEG 2000 fixtures must pass on $framework." }
    foreach ($regression in @(
            @{ Name = 'IdentifyRejectsDuplicateOpenExrChannelNames'; Count = 3 },
            @{ Name = 'IdentifyNeverIgnoresWrongTypedOpenExrChannels'; Count = 2 },
            @{ Name = 'IdentifyRejectsUnsupportedOpenExrVersionFlags'; Count = 3 },
            @{ Name = 'IdentifyRejectsOpenExrSamplingThatConflictsWithDataWindow'; Count = 4 },
            @{ Name = 'IdentifyReadsRealOpenExrFirstHeaderFixtures'; Count = 7 })) {
        if (@($results | Where-Object { ([string]$_.testName).Contains($regression.Name) -and [string]$_.outcome -ceq 'Passed' }).Count -ne $regression.Count) { throw "All $($regression.Count) OpenEXR cases must pass on ${framework}: $($regression.Name)" }
    }
    foreach ($regression in @(
            @{ Name = 'IdentifyReadsPlainPbmPixelsAsIndividualBits'; Count = 6 },
            @{ Name = 'IdentifyNeverCollapsesMultiplePlainPbmBitsIntoOneSample'; Count = 4 },
            @{ Name = 'IdentifyStopsPnmHeaderCommentsAtCarriageReturn'; Count = 6 },
            @{ Name = 'IdentifyPreservesLineFeedRasterByteAfterRawPnmCarriageReturnDelimiter'; Count = 3 },
            @{ Name = 'IdentifyNeverSkipsExtraRawPnmLineFeedAsHeaderWhitespace'; Count = 3 },
            @{ Name = 'IdentifyTreatsRawPnmCommentCarriageReturnAsItsOnlyRasterDelimiter'; Count = 3 },
            @{ Name = 'IdentifyDoesNotHidePlainPnmTrailingSamplesInsideCarriageReturnComments'; Count = 3 },
            @{ Name = 'IdentifyReadsRealPnmPlainAndRawFixtures'; Count = 16 })) {
        if (@($results | Where-Object { ([string]$_.testName).Contains($regression.Name) -and [string]$_.outcome -ceq 'Passed' }).Count -ne $regression.Count) { throw "All $($regression.Count) PNM cases must pass on ${framework}: $($regression.Name)" }
    }
    foreach ($regression in @(
            @{ Name = 'IdentifyRejectsDuplicatePamAdmissionFieldsEvenWhenValuesMatch'; Count = 12 },
            @{ Name = 'IdentifyNeverLetsLaterPamFieldsLowerAnAdmissionBudget'; Count = 4 },
            @{ Name = 'IdentifyAcceptsPamHeaderLineTrailingWhitespace'; Count = 5 },
            @{ Name = 'IdentifyReadsPamArbitraryPositiveDepthWithoutGuessingNativeSupport'; Count = 4 },
            @{ Name = 'IdentifyRequiresPamMagicOnItsOwnLine'; Count = 4 },
            @{ Name = 'IdentifyRejectsUnknownPamKeywordsAndEmptyTupleType'; Count = 7 },
            @{ Name = 'IdentifyReadsRealPamTupleAndArbitraryDepthFixtures'; Count = 13 })) {
        if (@($results | Where-Object { ([string]$_.testName).Contains($regression.Name) -and [string]$_.outcome -ceq 'Passed' }).Count -ne $regression.Count) { throw "All $($regression.Count) PAM cases must pass on ${framework}: $($regression.Name)" }
    }
    foreach ($regression in @(
            @{ Name = 'IdentifyRejectsIncompletePngIhdrBeforePublishingHeaderFacts'; Count = 4 },
            @{ Name = 'IdentifyRejectsUnsupportedPngIhdrFieldsAsOneHeader'; Count = 12 },
            @{ Name = 'IdentifyRejectsIndexedPngCompletionWithoutRequiredPalette'; Count = 4 },
            @{ Name = 'IdentifyRejectsConflictingPngPaletteDeclarations'; Count = 8 },
            @{ Name = 'IdentifyRejectsInvalidPngIccStorageEnvelopes'; Count = 14 },
            @{ Name = 'IdentifyAcceptsAllStandardPngColorDepthPairs'; Count = 15 },
            @{ Name = 'IdentifyReadsRealPngColorDepthPaletteAndAdam7Fixtures'; Count = 15 })) {
        if (@($results | Where-Object { ([string]$_.testName).Contains($regression.Name) -and [string]$_.outcome -ceq 'Passed' }).Count -ne $regression.Count) { throw "All $($regression.Count) PNG cases must pass on ${framework}: $($regression.Name)" }
    }
    foreach ($regression in @(
            @{ Name = 'IdentifyReadsBmpCoreDimensionsFromUnsigned16BitFields'; Count = 5 },
            @{ Name = 'IdentifyNeverPublishesIndexedBmpFactsWithoutCompletePalette'; Count = 3 },
            @{ Name = 'IdentifyRejectsConflictingBmpImageByteLengths'; Count = 2 },
            @{ Name = 'IdentifyRejectsUnrecognizedBmpDibLayouts'; Count = 2 },
            @{ Name = 'IdentifyRequiresOneBmpColorPlane'; Count = 2 },
            @{ Name = 'IdentifyDoesNotClaimFrameForInvalidBmpBitCounts'; Count = 2 },
            @{ Name = 'IdentifyNeverLetsBmpV5ProfilesOverlapHeaders'; Count = 2 },
            @{ Name = 'IdentifyReadsRealBmpDibAndPaletteFixtures'; Count = 10 })) {
        if (@($results | Where-Object { ([string]$_.testName).Contains($regression.Name) -and [string]$_.outcome -ceq 'Passed' }).Count -ne $regression.Count) { throw "All $($regression.Count) BMP cases must pass on ${framework}: $($regression.Name)" }
    }
    Write-Host "IMAGE_PREFLIGHT_MUTATION_FRAMEWORK_OK framework=$framework tests_passed=$($counters.passed) timeout_seconds=$TimeoutSeconds trx=$trxPath"
    $process.Dispose()
}

Write-Host "IMAGE_PREFLIGHT_MUTATION_CORPUS_OK fixtures=$(@($manifest.fixtures).Count) replacements=$(@($manifest.replacementBytes).Count) frameworks=$(@($manifest.frameworks).Count) minimum_mutations=$($manifest.expectedMinimumMutations) minimum_stream_cases=$($manifest.expectedMinimumStreamCases) native_runtime_required=false"
