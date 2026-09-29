param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$FullNativeRuntimeDir,
    [Parameter(Mandatory)][string]$MiniNativeRuntimeDir,
    [Parameter(Mandatory)][string]$FullRuntimePackagePath,
    [Parameter(Mandatory)][string]$MiniRuntimePackagePath,
    [string]$OutputPath = 'packaging/codec/codec-native-mutation-evidence.json',
    [int]$TimeoutSeconds = 120,
    [long]$MaxWorkingSetGrowthBytes = 268435456
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($TimeoutSeconds -lt 10 -or $TimeoutSeconds -gt 600) { throw 'TimeoutSeconds must be between 10 and 600.' }
if ($MaxWorkingSetGrowthBytes -lt 1) { throw 'MaxWorkingSetGrowthBytes must be positive.' }
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows) -or
    [Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [Runtime.InteropServices.Architecture]::X64) {
    throw 'Native codec mutation evidence requires a Windows x64 runner.'
}
$status = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) { throw 'Native codec mutation measurement requires a clean repository.' }
$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim().ToLowerInvariant()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Unable to resolve the codec mutation source commit.' }
$dotnet = Get-Command dotnet -ErrorAction Stop
$project = Join-Path $repo 'tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj'
$manifestPath = Join-Path $repo 'packaging/codec/image-preflight-mutation-corpus.json'
$manifestSchemaPath = Join-Path $repo 'packaging/codec/image-preflight-mutation-corpus.schema.json'
foreach ($path in @($project,$manifestPath,$manifestSchemaPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Native codec mutation input is missing: $path" } }
if (-not (Test-Json -LiteralPath $manifestPath -SchemaFile $manifestSchemaPath -ErrorAction Stop)) { throw 'Image preflight mutation corpus failed schema validation.' }
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$manifestHash = (Get-FileHash -LiteralPath $manifestPath -Algorithm SHA256).Hash.ToLowerInvariant()

function Get-FileSha256 {
    param([Parameter(Mandatory)][string]$Path)
    (Get-FileHash -LiteralPath $Path -Algorithm SHA256).Hash.ToLowerInvariant()
}

function Get-RuntimePackageEvidence {
    param(
        [Parameter(Mandatory)][string]$Profile,
        [Parameter(Mandatory)][string]$RuntimeDirectory,
        [Parameter(Mandatory)][string]$PackagePath
    )
    $runtimePath = (Resolve-Path -LiteralPath $RuntimeDirectory).Path
    $packageFullPath = (Resolve-Path -LiteralPath $PackagePath).Path
    $expectedPackageName = if ($Profile -ceq 'full') { 'jyppx.opencv.runtime.win-x64.5.0.0.nupkg' } else { 'jyppx.opencv.runtime.win-x64.mini.5.0.0.nupkg' }
    if ([IO.Path]::GetFileName($packageFullPath).ToLowerInvariant() -cne $expectedPackageName) { throw "Unexpected $Profile runtime package name." }
    $runtimeFiles = @(Get-ChildItem -LiteralPath $runtimePath -File -Filter '*.dll' | Sort-Object Name)
    $expectedCount = if ($Profile -ceq 'full') { 18 } else { 7 }
    if ($runtimeFiles.Count -ne $expectedCount) { throw "Expected $expectedCount $Profile runtime DLLs; found $($runtimeFiles.Count)." }
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [IO.Compression.ZipFile]::OpenRead($packageFullPath)
    try {
        $entries = @($archive.Entries | Where-Object { $_.FullName -match '^runtimes/win-x64/native/[^/]+\.dll$' } | Sort-Object FullName)
        if ($entries.Count -ne $expectedCount) { throw "Runtime package has an unexpected $Profile native payload count." }
        $payload = [System.Collections.Generic.List[object]]::new()
        for ($index = 0; $index -lt $entries.Count; $index++) {
            $entry = $entries[$index]
            $file = $runtimeFiles[$index]
            if ([IO.Path]::GetFileName($entry.FullName) -cne $file.Name) { throw "$Profile package/runtime payload order or name differs at index $index." }
            $stream = $entry.Open()
            try { $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($stream)).ToLowerInvariant() }
            finally { $stream.Dispose() }
            $fileHash = Get-FileSha256 -Path $file.FullName
            if ([int64]$entry.Length -ne [int64]$file.Length -or $hash -cne $fileHash) { throw "$Profile package/runtime payload mismatch: $($file.Name)." }
            $payload.Add([ordered]@{ name = $file.Name; bytes = [int64]$file.Length; sha256 = $fileHash })
        }
    }
    finally { $archive.Dispose() }
    [ordered]@{
        profile = $Profile
        packageFileName = [IO.Path]::GetFileName($packageFullPath)
        packageSha256 = Get-FileSha256 -Path $packageFullPath
        nativeDllCount = $expectedCount
        payload = @($payload)
    }
}

function Get-ProcessTreeWorkingSet {
    param([Parameter(Mandatory)][int]$RootProcessId)
    $processes = @(Get-CimInstance -ClassName Win32_Process -ErrorAction Stop)
    $ids = [System.Collections.Generic.HashSet[int]]::new()
    [void]$ids.Add($RootProcessId)
    $changed = $true
    while ($changed) {
        $changed = $false
        foreach ($item in $processes) {
            $pidValue = [int]$item.ProcessId
            if (-not $ids.Contains($pidValue) -and $ids.Contains([int]$item.ParentProcessId)) {
                [void]$ids.Add($pidValue)
                $changed = $true
            }
        }
    }
    [long]$sum = 0
    foreach ($item in $processes) {
        if ($ids.Contains([int]$item.ProcessId)) { $sum += [long]$item.WorkingSetSize }
    }
    return $sum
}

function Invoke-NativeMutationFramework {
    param(
        [Parameter(Mandatory)][string]$Profile,
        [Parameter(Mandatory)][string]$Framework,
        [Parameter(Mandatory)][string]$RuntimePath,
        [Parameter(Mandatory)][string]$ResultsDirectory
    )
    $buildArguments = @('build', $project, '-c', 'Release', '-f', $Framework, '--no-restore', ('-p:OpenCvNativeRuntimeDir=' + $RuntimePath))
    & $dotnet.Source @buildArguments | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Native codec mutation build failed for $Profile/$Framework with exit code $LASTEXITCODE." }
    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $dotnet.Source
    $startInfo.WorkingDirectory = $repo
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in @(
            'test', $project, '-c', 'Release', '-f', $Framework, '--no-restore', '--no-build',
            '--filter', 'FullyQualifiedName~ImDecodeDeterministicMutationCorpusStaysWithinNativeBoundary',
            ('-p:OpenCvNativeRuntimeDir=' + $RuntimePath), '--logger', ('trx;LogFileName=NativeMutation-' + $Profile + '-' + $Framework + '.trx'),
            '--results-directory', $ResultsDirectory, '--verbosity', 'minimal')) {
        [void]$startInfo.ArgumentList.Add($argument)
    }
    $startInfo.Environment['OPENCV_CSHARP_NATIVE_SMOKE'] = '1'
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    if (-not $process.Start()) { throw "Could not start native codec mutation process for $Profile/$Framework." }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $stopwatch = [Diagnostics.Stopwatch]::StartNew()
    [long]$baselineWorkingSet = 0
    [long]$peakWorkingSet = 0
    $timedOut = $false
    while (-not $process.HasExited) {
        try {
            $treeWorkingSet = Get-ProcessTreeWorkingSet -RootProcessId $process.Id
            if ($baselineWorkingSet -eq 0) { $baselineWorkingSet = $treeWorkingSet }
            if ($treeWorkingSet -gt $peakWorkingSet) { $peakWorkingSet = $treeWorkingSet }
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
        $treeWorkingSet = Get-ProcessTreeWorkingSet -RootProcessId $process.Id
        if ($treeWorkingSet -gt $peakWorkingSet) { $peakWorkingSet = $treeWorkingSet }
        if ($baselineWorkingSet -eq 0) { $baselineWorkingSet = $treeWorkingSet }
    } catch { }
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    $exitCode = if ($timedOut) { -1 } else { [int]$process.ExitCode }
    $combined = $stdout + "`n" + $stderr
    $failureClass = if ($timedOut) {
        'hang'
    } elseif ($combined -match '(?i)OutOfMemoryException|InsufficientMemoryException|out of memory|E_OUTOFMEMORY') {
        'oom'
    } elseif ($exitCode -eq 0) {
        'clean'
    } elseif ($combined -match '(?i)Test Run Failed|Failed!|Xunit\.Sdk|Assert\.') {
        'managed-test-failure'
    } elseif ($combined -match '(?i)Unhandled exception|fatal|unhandled|AccessViolationException') {
        'managed-unhandled-exception'
    } else {
        'crash'
    }
    $growth = [Math]::Max(0L, $peakWorkingSet - $baselineWorkingSet)
    $resourceLeakObserved = $growth -gt $MaxWorkingSetGrowthBytes
    if ($resourceLeakObserved -and $failureClass -eq 'clean') { $failureClass = 'resource-leak' }
    $process.Dispose()
    $trxPath = Join-Path $ResultsDirectory ("NativeMutation-$Profile-$Framework.trx")
    $testPassed = $false
    if (Test-Path -LiteralPath $trxPath -PathType Leaf) {
        [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
        $counters = $trx.TestRun.ResultSummary.Counters
        $testPassed = $null -ne $counters -and [int]$counters.total -eq 1 -and [int]$counters.executed -eq 1 -and [int]$counters.passed -eq 1 -and [int]$counters.failed -eq 0 -and [int]$counters.notExecuted -eq 0
    }
    if ($failureClass -eq 'clean' -and -not $testPassed) { $failureClass = 'test-result-missing-or-incomplete' }
    [ordered]@{
        profile = $Profile
        targetFramework = $Framework
        status = if ($failureClass -eq 'clean') { 'clean' } else { 'failed' }
        failureClass = $failureClass
        exitCode = $exitCode
        passed = $testPassed
        durationMilliseconds = [int64]$stopwatch.ElapsedMilliseconds
        fixtureCount = [int]@($manifest.fixtures).Count
        baselineProcessTreeWorkingSetBytes = $baselineWorkingSet
        peakProcessTreeWorkingSetBytes = $peakWorkingSet
        processTreeWorkingSetGrowthBytes = $growth
        maxProcessTreeWorkingSetGrowthBytes = $MaxWorkingSetGrowthBytes
        resourceLeakObserved = $resourceLeakObserved
        oomCount = if ($failureClass -eq 'oom') { 1 } else { 0 }
        crashCount = if ($failureClass -eq 'crash') { 1 } else { 0 }
        hangCount = if ($failureClass -eq 'hang') { 1 } else { 0 }
        failureSummary = if ($failureClass -eq 'clean') { '' } else { (($combined -replace "`r", ' ') -replace "`n", ' ').Trim().Substring(0, [Math]::Min(512, (($combined -replace "`r", ' ') -replace "`n", ' ').Trim().Length)) }
    }
}

$runtimeRows = @(
    Get-RuntimePackageEvidence -Profile 'full' -RuntimeDirectory $FullNativeRuntimeDir -PackagePath $FullRuntimePackagePath
    Get-RuntimePackageEvidence -Profile 'mini' -RuntimeDirectory $MiniNativeRuntimeDir -PackagePath $MiniRuntimePackagePath
)
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-codec-native-mutation-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
$frameworkRows = [System.Collections.Generic.List[object]]::new()
try {
    foreach ($runtime in $runtimeRows) {
        $runtimePath = if ([string]$runtime.profile -ceq 'full') { (Resolve-Path -LiteralPath $FullNativeRuntimeDir).Path } else { (Resolve-Path -LiteralPath $MiniNativeRuntimeDir).Path }
        foreach ($framework in @('net8.0','net10.0')) {
            $frameworkRows.Add((Invoke-NativeMutationFramework -Profile ([string]$runtime.profile) -Framework $framework -RuntimePath $runtimePath -ResultsDirectory $temporaryRoot))
        }
    }
}
finally { if (Test-Path -LiteralPath $temporaryRoot -PathType Container) { Remove-Item -LiteralPath $temporaryRoot -Recurse -Force } }
$classificationNames = @('clean','oom','crash','hang','managedTestFailure','resourceLeak')
$classifications = [ordered]@{
    clean = @($frameworkRows | Where-Object failureClass -eq 'clean').Count
    oom = @($frameworkRows | Where-Object failureClass -eq 'oom').Count
    crash = @($frameworkRows | Where-Object failureClass -eq 'crash').Count
    hang = @($frameworkRows | Where-Object failureClass -eq 'hang').Count
    managedTestFailure = @($frameworkRows | Where-Object { [string]$_.failureClass -match '^managed-' -or [string]$_.failureClass -eq 'test-result-missing-or-incomplete' }).Count
    resourceLeak = @($frameworkRows | Where-Object failureClass -eq 'resource-leak').Count
}
$statusValue = if ($frameworkRows.Count -eq 4 -and $classifications.clean -eq 4) { 'verified' } else { 'failed' }
$evidence = [ordered]@{
    '$schema' = 'codec-native-mutation-evidence.schema.json'
    schemaVersion = 1
    status = $statusValue
    sourceCommit = $sourceCommit
    corpusManifestSha256 = $manifestHash
    testFilter = 'FullyQualifiedName~ImDecodeDeterministicMutationCorpusStaysWithinNativeBoundary'
    timeoutSeconds = $TimeoutSeconds
    maxWorkingSetGrowthBytes = $MaxWorkingSetGrowthBytes
    nativeRuntimeRequired = $true
    runner = [ordered]@{
        os = [Runtime.InteropServices.RuntimeInformation]::OSDescription
        architecture = 'x64'
        dotnet = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim()
        configuration = 'Release'
    }
    runtimePackages = @($runtimeRows)
    frameworks = @($frameworkRows)
    classifications = $classifications
    limitations = @(
        'The deterministic input corpus applies bounded byte replacements to complete format fixtures; it is not coverage-guided fuzzing.',
        'Each test runs in an isolated dotnet test process with native runtime DLLs copied from the exact full or mini NuGet package.',
        'WorkingSet64 is sampled across the dotnet test process tree on this Windows x64 runner; teardown cannot prove absence of a long-lived process leak.',
        'Malformed codec rejection is permitted as InvalidDataException or OpenCvException; successful decodes must remain within the configured 64-by-64 bound.'
    )
}
$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) { [IO.Path]::GetFullPath($OutputPath) } else { [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar))) }
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 16) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "CODEC_NATIVE_MUTATION_MEASURED status=$statusValue source_commit=$sourceCommit profiles=full,mini frameworks=net8.0,net10.0 clean=$($classifications.clean) oom=$($classifications.oom) crash=$($classifications.crash) hang=$($classifications.hang) rss_limit=$MaxWorkingSetGrowthBytes output=$outputFullPath"
if ($statusValue -ne 'verified') { throw 'Native codec mutation classification did not verify cleanly.' }
