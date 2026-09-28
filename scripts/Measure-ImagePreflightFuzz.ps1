param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'packaging/codec/image-preflight-fuzz-evidence.json',
    [string]$SourceCommit = '',
    [int]$TimeoutSeconds = 120,
    [long]$MaxWorkingSetGrowthBytes = 268435456
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($TimeoutSeconds -lt 10 -or $TimeoutSeconds -gt 600) { throw 'TimeoutSeconds must be between 10 and 600.' }
if ($MaxWorkingSetGrowthBytes -lt 1) { throw 'MaxWorkingSetGrowthBytes must be positive.' }

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$dotnet = Get-Command dotnet -ErrorAction Stop
$project = Join-Path $repo 'tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj'
$manifestPath = Join-Path $repo 'packaging/codec/image-preflight-mutation-corpus.json'
$schemaPath = Join-Path $repo 'packaging/codec/image-preflight-mutation-corpus.schema.json'
foreach ($path in @($project, $manifestPath, $schemaPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Required fuzz input is missing: $path" }
}
if (-not (Test-Json -LiteralPath $manifestPath -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'Image preflight mutation corpus manifest failed JSON Schema validation.' }
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$sourceCommitValue = $SourceCommit
if ([string]::IsNullOrWhiteSpace($sourceCommitValue)) {
    $sourceCommitValue = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim()
}
if ($sourceCommitValue -notmatch '^[0-9a-f]{40}$') { throw "SourceCommit must be a lowercase commit SHA: $sourceCommitValue" }

$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) {
    [IO.Path]::GetFullPath($OutputPath)
} else {
    [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar)))
}
$outputDirectory = Split-Path -Parent $outputFullPath
New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null

function Get-ShortSummary {
    param([AllowEmptyString()][string]$Text)
    $normalized = (($Text -replace "`r", ' ') -replace "`n", ' ').Trim()
    if ($normalized.Length -gt 512) { return $normalized.Substring(0, 512) }
    return $normalized
}

function Get-ArchitectureName {
    $architecture = [string]$env:PROCESSOR_ARCHITECTURE
    if ($architecture -match '^(AMD64|x86_64)$') { return 'x64' }
    if ($architecture -match '^(ARM64|AARCH64)$') { return 'arm64' }
    if ($architecture -match '^(x86|X86)$') { return 'x86' }
    return $architecture.ToLowerInvariant()
}

function Invoke-FuzzFramework {
    param([Parameter(Mandatory)][string]$Framework)

    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $dotnet.Source
    $startInfo.WorkingDirectory = $repo
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in @(
            'test', $project, '-c', 'Release', '-f', $Framework, '--no-restore',
            '--filter', 'FullyQualifiedName~IdentifyDeterministicMutationCorpus',
            '--logger', 'console;verbosity=minimal')) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    if (-not $process.Start()) { throw "Could not start image preflight corpus on $Framework." }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $stopwatch = [Diagnostics.Stopwatch]::StartNew()
    $baselineWorkingSet = 0L
    $peakWorkingSet = 0L
    $timedOut = $false
    while ($true) {
        if ($process.HasExited) { break }
        try {
            $process.Refresh()
            $workingSet = [int64]$process.WorkingSet64
            if ($baselineWorkingSet -eq 0) { $baselineWorkingSet = $workingSet }
            if ($workingSet -gt $peakWorkingSet) { $peakWorkingSet = $workingSet }
        } catch { }
        if ($stopwatch.Elapsed.TotalSeconds -ge $TimeoutSeconds) {
            $timedOut = $true
            try { $process.Kill($true) } catch { }
            break
        }
        Start-Sleep -Milliseconds 100
    }
    try { $process.WaitForExit() } catch { }
    $stopwatch.Stop()
    try {
        $process.Refresh()
        $workingSetAtExit = [int64]$process.WorkingSet64
        if ($workingSetAtExit -gt $peakWorkingSet) { $peakWorkingSet = $workingSetAtExit }
    } catch { $workingSetAtExit = 0L }
    if ($baselineWorkingSet -eq 0) { $baselineWorkingSet = $workingSetAtExit }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    $exitCode = if ($timedOut) { -1 } else { [int]$process.ExitCode }
    $combined = $stdout + "`n" + $stderr
    $failureClass = if ($timedOut) {
        'hang'
    } elseif ($combined -match '(?i)OutOfMemoryException|InsufficientMemoryException|out of memory') {
        'oom'
    } elseif ($exitCode -eq 0) {
        'clean'
    } elseif ($combined -match '(?i)Test Run Failed|Failed!|Xunit\.Sdk|Assert\.') {
        'managed-test-failure'
    } elseif ($combined -match '(?i)Unhandled exception|fatal|unhandled') {
        'managed-unhandled-exception'
    } else {
        'crash'
    }
    $growth = [Math]::Max(0L, $peakWorkingSet - $baselineWorkingSet)
    $resourceLeakObserved = $growth -gt $MaxWorkingSetGrowthBytes
    if ($resourceLeakObserved -and $failureClass -eq 'clean') { $failureClass = 'resource-leak' }
    $process.Dispose()

    [ordered]@{
        targetFramework = $Framework
        status = if ($failureClass -eq 'clean') { 'clean' } else { 'failed' }
        failureClass = $failureClass
        exitCode = $exitCode
        durationMilliseconds = [int64]$stopwatch.ElapsedMilliseconds
        fixtureCount = [int]@($manifest.fixtures).Count
        expectedMinimumMutations = [int]$manifest.expectedMinimumMutations
        baselineWorkingSetBytes = $baselineWorkingSet
        peakWorkingSetBytes = $peakWorkingSet
        workingSetGrowthBytes = $growth
        resourceLeakObserved = $resourceLeakObserved
        managedUnhandledExceptionCount = if ($failureClass -eq 'managed-unhandled-exception') { 1 } else { 0 }
        managedTestFailureCount = if ($failureClass -eq 'managed-test-failure') { 1 } else { 0 }
        oomCount = if ($failureClass -eq 'oom') { 1 } else { 0 }
        crashCount = if ($failureClass -eq 'crash') { 1 } else { 0 }
        hangCount = if ($failureClass -eq 'hang') { 1 } else { 0 }
        failureSummary = if ($failureClass -eq 'clean') { '' } else { Get-ShortSummary -Text $combined }
    }
}

$frameworkResults = @()
foreach ($framework in @($manifest.frameworks)) {
    $frameworkResults += Invoke-FuzzFramework -Framework ([string]$framework)
}
$classifications = [ordered]@{
    clean = @($frameworkResults | Where-Object failureClass -eq 'clean').Count
    managedUnhandledException = @($frameworkResults | Where-Object failureClass -eq 'managed-unhandled-exception').Count
    managedTestFailure = @($frameworkResults | Where-Object failureClass -eq 'managed-test-failure').Count
    oom = @($frameworkResults | Where-Object failureClass -eq 'oom').Count
    crash = @($frameworkResults | Where-Object failureClass -eq 'crash').Count
    hang = @($frameworkResults | Where-Object failureClass -eq 'hang').Count
    resourceLeak = @($frameworkResults | Where-Object failureClass -eq 'resource-leak').Count
}
$hasFailure = @($frameworkResults | Where-Object { [string]$_.failureClass -ne 'clean' }).Count -gt 0
$status = if (-not $hasFailure -and [int]$classifications.clean -eq @($manifest.frameworks).Count) { 'verified' } else { 'failed' }
$evidence = [ordered]@{
    '$schema' = 'image-preflight-fuzz-evidence.schema.json'
    schemaVersion = 1
    status = $status
    sourceCommit = $sourceCommitValue
    corpusManifestSha256 = (Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant()
    testFilter = 'FullyQualifiedName~IdentifyDeterministicMutationCorpus'
    timeoutSeconds = $TimeoutSeconds
    maxWorkingSetGrowthBytes = $MaxWorkingSetGrowthBytes
    nativeRuntimeRequired = $false
    runner = [ordered]@{
        os = [Environment]::OSVersion.VersionString
        architecture = Get-ArchitectureName
        dotnet = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
        configuration = 'Release'
    }
    frameworks = @($frameworkResults)
    classifications = $classifications
    limitations = @(
        'The corpus is deterministic managed preflight coverage; it is not coverage-guided native fuzzing.',
        'The isolated dotnet test process distinguishes clean, managed failure, OOM, crash, and timeout outcomes.',
        'WorkingSet64 is sampled process RSS evidence for this runner; process teardown cannot prove absence of a long-lived native leak.',
        'No native runtime is required because the tested Identify paths stop before native decoding.'
    )
}
$utf8NoBom = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 8) + [Environment]::NewLine), $utf8NoBom)
Write-Host "IMAGE_PREFLIGHT_FUZZ_MEASURED status=$status source_commit=$sourceCommitValue frameworks=$(@($frameworkResults).Count) clean=$($classifications.clean) oom=$($classifications.oom) crash=$($classifications.crash) hang=$($classifications.hang) resource_leak=$($classifications.resourceLeak) output=$outputFullPath"
if ($status -ne 'verified') { throw 'Image preflight fuzz classification did not verify cleanly.' }
