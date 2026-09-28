param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$evidenceRel = 'packaging/codec/image-preflight-fuzz-evidence.json'
$schemaRel = 'packaging/codec/image-preflight-fuzz-evidence.schema.json'
$manifestRel = 'packaging/codec/image-preflight-mutation-corpus.json'
$evidencePath = Join-Path $repo ($evidenceRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schemaPath = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$manifestPath = Join-Path $repo ($manifestRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($evidencePath, $schemaPath, $manifestPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Image preflight fuzz evidence input is missing: $path" }
}
if (-not (Test-Json -LiteralPath $evidencePath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Image preflight fuzz evidence failed JSON Schema validation.' }
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
if ([string]$evidence.status -cne 'verified') { throw "Image preflight fuzz evidence is not verified: $($evidence.status)" }
if ([string]$evidence.sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Image preflight fuzz source commit is invalid.' }
$commitType = ((& git -C $repo cat-file -t ([string]$evidence.sourceCommit) 2>$null) | Select-Object -First 1).Trim()
if ($LASTEXITCODE -ne 0 -or $commitType -cne 'commit') { throw 'Image preflight fuzz source commit is not present in the repository.' }
& git -C $repo merge-base --is-ancestor ([string]$evidence.sourceCommit) HEAD 2>$null
if ($LASTEXITCODE -ne 0) { throw 'Image preflight fuzz source commit is not an ancestor of HEAD.' }
$manifestHash = (Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant()
if ([string]$evidence.corpusManifestSha256 -cne $manifestHash) { throw 'Image preflight fuzz corpus manifest hash drifted.' }
if ([string]$evidence.testFilter -cne 'FullyQualifiedName~IdentifyDeterministicMutationCorpus') { throw 'Image preflight fuzz test filter drifted.' }
if ([bool]$evidence.nativeRuntimeRequired) { throw 'Image preflight fuzz evidence must remain managed-runtime independent.' }
if ([string]$evidence.runner.configuration -cne 'Release' -or [string]$evidence.runner.architecture -cne 'x64') { throw 'Image preflight fuzz runner identity drifted.' }
if (@($evidence.frameworks).Count -ne @($manifest.frameworks).Count) { throw 'Image preflight fuzz framework count drifted.' }
$expectedFrameworks = @($manifest.frameworks | ForEach-Object { [string]$_ })
foreach ($framework in $evidence.frameworks) {
    if ($expectedFrameworks -notcontains [string]$framework.targetFramework) { throw "Unexpected fuzz framework: $($framework.targetFramework)" }
    if ([string]$framework.failureClass -cne 'clean' -or [string]$framework.status -cne 'clean' -or [int]$framework.exitCode -ne 0) { throw "Fuzz framework did not finish cleanly: $($framework.targetFramework)" }
    if ([bool]$framework.resourceLeakObserved) { throw "Fuzz framework exceeded the resource budget: $($framework.targetFramework)" }
    foreach ($field in @('durationMilliseconds','fixtureCount','expectedMinimumMutations','baselineWorkingSetBytes','peakWorkingSetBytes','workingSetGrowthBytes')) {
        if ([int64]$framework.$field -lt 0) { throw "Fuzz framework metric is negative: $($framework.targetFramework)/$field" }
    }
    if ([int64]$framework.workingSetGrowthBytes -gt [int64]$evidence.maxWorkingSetGrowthBytes) { throw "Fuzz framework working-set growth exceeded the bound: $($framework.targetFramework)" }
    if ([int]$framework.fixtureCount -ne @($manifest.fixtures).Count -or [int]$framework.expectedMinimumMutations -ne [int]$manifest.expectedMinimumMutations) { throw "Fuzz corpus dimensions drifted: $($framework.targetFramework)" }
    foreach ($field in @('managedUnhandledExceptionCount','managedTestFailureCount','oomCount','crashCount','hangCount')) {
        if ([int]$framework.$field -ne 0) { throw "Fuzz framework recorded a failure classification: $($framework.targetFramework)/$field" }
    }
}
if ([int]$evidence.classifications.clean -ne @($manifest.frameworks).Count) { throw 'Image preflight fuzz clean classification count drifted.' }
foreach ($field in @('managedUnhandledException','managedTestFailure','oom','crash','hang','resourceLeak')) {
    if ([int]$evidence.classifications.$field -ne 0) { throw "Image preflight fuzz classification is non-zero: $field" }
}
Write-Host "IMAGE_PREFLIGHT_FUZZ_EVIDENCE_OK source_commit=$($evidence.sourceCommit) frameworks=$(@($evidence.frameworks).Count) clean=$($evidence.classifications.clean) oom=$($evidence.classifications.oom) crash=$($evidence.classifications.crash) hang=$($evidence.classifications.hang) resource_leak=$($evidence.classifications.resourceLeak)"
