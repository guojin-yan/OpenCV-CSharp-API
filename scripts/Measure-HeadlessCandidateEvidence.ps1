param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$MiniNativeRuntimeDir,
    [Parameter(Mandatory)][string]$MiniRuntimePackagePath,
    [string]$OutputPath = 'packaging/runtime/runtime-headless-candidate-evidence.json',
    [int]$TimeoutSeconds = 60
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if ($TimeoutSeconds -lt 10 -or $TimeoutSeconds -gt 600) { throw 'TimeoutSeconds must be between 10 and 600.' }
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if (-not [Runtime.InteropServices.RuntimeInformation]::IsOSPlatform([Runtime.InteropServices.OSPlatform]::Windows) -or [Runtime.InteropServices.RuntimeInformation]::OSArchitecture -ne [Runtime.InteropServices.Architecture]::X64) { throw 'Headless candidate evidence requires Windows x64.' }
$status = @(& git -C $repo status --porcelain 2>$null)
if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) { throw 'Headless candidate evidence requires a clean repository.' }
$sourceCommit = ((& git -C $repo rev-parse HEAD 2>$null) | Select-Object -First 1).Trim().ToLowerInvariant()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Unable to resolve headless candidate source commit.' }
$miniRuntime = (Resolve-Path -LiteralPath $MiniNativeRuntimeDir).Path
$miniPackage = (Resolve-Path -LiteralPath $MiniRuntimePackagePath).Path
if ([IO.Path]::GetFileName($miniPackage).ToLowerInvariant() -cne 'jyppx.opencv.runtime.win-x64.mini.5.0.0.nupkg') { throw 'Unexpected mini runtime package name.' }
$packageHash = (Get-FileHash -LiteralPath $miniPackage -Algorithm SHA256).Hash.ToLowerInvariant()
if ($packageHash -cne 'c4712ed62f34f31eacaaa4c51f84ee6003bf5bab3cfcca850d39c3b812628918') { throw 'Mini runtime package hash drifted.' }
$runtimeFiles = @(Get-ChildItem -LiteralPath $miniRuntime -File -Filter '*.dll' | Sort-Object Name)
if ($runtimeFiles.Count -ne 7 -or @($runtimeFiles.Name) -contains 'opencv_highgui500.dll') { throw 'Mini headless candidate runtime payload is not HighGui-free.' }
$payload = @($runtimeFiles | ForEach-Object { [ordered]@{ name = $_.Name; bytes = [int64]$_.Length; sha256 = (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() } })
$dotnet = Get-Command dotnet -ErrorAction Stop
$project = Join-Path $repo 'tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj'
$results = [System.Collections.Generic.List[object]]::new()
$videoioBackends = [System.Collections.Generic.List[string]]::new()
$videoioCameraBackends = [System.Collections.Generic.List[string]]::new()
$dnnStatuses = [System.Collections.Generic.List[string]]::new()
$environmentNames = @('OPENCV_CSHARP_NATIVE_SMOKE','OPENCV_CSHARP_HEADLESS_SMOKE','DISPLAY','WAYLAND_DISPLAY','LD_LIBRARY_PATH','OPENCV_CSHARP_OPENCV_RUNTIME_ROOT')
$savedEnvironment = @{}
foreach ($name in $environmentNames) { $savedEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-headless-candidate-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null

function Invoke-HeadlessFramework {
    param([Parameter(Mandatory)][string]$Framework)
    $buildArguments = @('build', $project, '-c', 'Release', '-f', $Framework, '--no-restore', ('-p:OpenCvNativeRuntimeDir=' + $miniRuntime))
    & $dotnet.Source @buildArguments | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Headless candidate build failed for $Framework with exit code $LASTEXITCODE." }
    $trxPath = Join-Path $temporaryRoot ("Headless-$Framework.trx")
    $startInfo = [Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $dotnet.Source
    $startInfo.WorkingDirectory = $repo
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    foreach ($argument in @('test', $project, '-c', 'Release', '-f', $Framework, '--no-restore', '--no-build', '--filter', 'FullyQualifiedName~HeadlessRuntimeCandidateTests', ('-p:OpenCvNativeRuntimeDir=' + $miniRuntime), '--logger', ('trx;LogFileName=' + $trxPath))) { [void]$startInfo.ArgumentList.Add($argument) }
    foreach ($name in $environmentNames) { [void]$startInfo.Environment.Remove($name) }
    $startInfo.Environment['OPENCV_CSHARP_NATIVE_SMOKE'] = '1'
    $startInfo.Environment['OPENCV_CSHARP_HEADLESS_SMOKE'] = '1'
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    if (-not $process.Start()) { throw "Could not start headless candidate test process for $Framework." }
    $stdoutTask = $process.StandardOutput.ReadToEndAsync()
    $stderrTask = $process.StandardError.ReadToEndAsync()
    $stopwatch = [Diagnostics.Stopwatch]::StartNew()
    $timedOut = $false
    while (-not $process.HasExited) {
        if ($stopwatch.Elapsed.TotalSeconds -ge $TimeoutSeconds) {
            $timedOut = $true
            try { $process.Kill($true) } catch { }
            break
        }
        Start-Sleep -Milliseconds 100
    }
    try { $process.WaitForExit() } catch { }
    $stopwatch.Stop()
    $stdout = $stdoutTask.GetAwaiter().GetResult()
    $stderr = $stderrTask.GetAwaiter().GetResult()
    $exitCode = if ($timedOut) { -1 } else { [int]$process.ExitCode }
    $process.Dispose()
    $failureClass = if ($timedOut) { 'hang' } elseif ($exitCode -eq 0) { 'clean' } else { 'test-failure' }
    [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
    $counters = $trx.TestRun.ResultSummary.Counters
    if ($null -eq $counters -or [int]$counters.total -ne 1 -or [int]$counters.executed -ne 1 -or [int]$counters.passed -ne 1 -or [int]$counters.failed -ne 0 -or [int]$counters.notExecuted -ne 0) { throw "Headless mini candidate counters drifted for $Framework." }
    $testResult = @($trx.TestRun.Results.UnitTestResult | Select-Object -First 1)
    $testOutput = if ($testResult.Count -eq 1 -and $null -ne $testResult[0].Output) { [string]$testResult[0].Output.StdOut } else { '' }
    [ordered]@{
        row = [ordered]@{ targetFramework = $Framework; total = [int]$counters.total; executed = [int]$counters.executed; passed = [int]$counters.passed; failed = [int]$counters.failed; skipped = [int]$counters.notExecuted; timedOut = $timedOut; failureClass = $failureClass; durationMilliseconds = [int64]$stopwatch.ElapsedMilliseconds }
        output = $testOutput
    }
}

try {
    foreach ($framework in @('net8.0','net10.0')) {
        $run = Invoke-HeadlessFramework -Framework $framework
        $results.Add($run.row)
        if ($run.output -match '(?m)^HEADLESS_VIDEOIO_BACKENDS=(.*)$') { $videoioBackends.Add($Matches[1].Trim()) }
        if ($run.output -match '(?m)^HEADLESS_VIDEOIO_CAMERA_BACKENDS=(.*)$') { $videoioCameraBackends.Add($Matches[1].Trim()) }
        if ($run.output -match '(?m)^HEADLESS_DNN_STATUS=(.*)$') { $dnnStatuses.Add($Matches[1].Trim()) }
    }
}
finally {
    foreach ($name in $environmentNames) {
        [Environment]::SetEnvironmentVariable($name, [string]$savedEnvironment[$name], 'Process')
    }
    if (Test-Path -LiteralPath $temporaryRoot) { Remove-Item -LiteralPath $temporaryRoot -Recurse -Force }
}
$evidence = [ordered]@{
    '$schema' = 'runtime-headless-candidate-evidence.schema.json'
    schemaVersion = 1
    status = 'candidate-partial'
    sourceCommit = $sourceCommit
    candidateProfile = 'mini-headless-candidate'
    candidatePackageIdentityAllowed = $false
    runner = [ordered]@{ os = [Runtime.InteropServices.RuntimeInformation]::OSDescription; architecture = 'x64'; dotnet = ((& $dotnet.Source --version 2>$null) | Select-Object -First 1).Trim(); configuration = 'Release' }
    timeoutSeconds = $TimeoutSeconds
    runtimePackage = [ordered]@{ packageFileName = [IO.Path]::GetFileName($miniPackage); packageSha256 = $packageHash; nativeDllCount = 7; payload = $payload }
    environment = [ordered]@{ displayUnset = $true; waylandDisplayUnset = $true; ldLibraryPathUnset = $true; runtimeRootUnset = $true }
    frameworks = @($results)
    videoio = [ordered]@{ backends = [string]($videoioBackends -join '||'); cameraBackends = [string]($videoioCameraBackends -join '||'); missingFileOpen = 'returned-false' }
    dnn = [ordered]@{ status = if (@($dnnStatuses | Where-Object { $_ -ceq 'omitted-deterministic-unavailable' }).Count -eq 2) { 'omitted-deterministic-unavailable' } else { 'unexpected' } }
    verifiedCases = @('HighGui NamedWindow deterministic missing-entrypoint or NOT_LINKED path','HighGui DestroyWindow deterministic missing-entrypoint or NOT_LINKED path','HighGui ImShow deterministic missing-entrypoint or NOT_LINKED path','HighGui CreateTrackbar deterministic missing-entrypoint or NOT_LINKED path','HighGui current UI framework deterministic missing-entrypoint or NOT_LINKED path','codec PNG encode remains usable after failed HighGui calls','VideoIO missing-file open returns explicit false and disposes cleanly','DNN target query is deterministically unavailable because Mini omits dnn')
    limitations = @('This candidate evidence covers the existing Mini runtime payload, whose native wrapper omits HighGui entrypoints; it does not create a new headless package identity.', 'Full headless requires a separately rebuilt wrapper/profile without HighGui and is intentionally pending.', 'The VideoIO result is a bounded missing-file negative case; camera/network backend availability and multi-distro consumer gates remain pending.')
}
$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) { [IO.Path]::GetFullPath($OutputPath) } else { [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar))) }
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 12) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "HEADLESS_CANDIDATE_EVIDENCE_MEASURED status=candidate-partial source_commit=$sourceCommit profile=mini-headless-candidate frameworks=net8.0,net10.0 output=$outputFullPath"
