param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$MiniNativeRuntimeDir,
    [Parameter(Mandatory)][string]$MiniRuntimePackagePath,
    [string]$OutputPath = 'packaging/runtime/runtime-headless-candidate-evidence.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
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
$environmentNames = @('OPENCV_CSHARP_NATIVE_SMOKE','OPENCV_CSHARP_HEADLESS_SMOKE','DISPLAY','WAYLAND_DISPLAY','LD_LIBRARY_PATH','OPENCV_CSHARP_OPENCV_RUNTIME_ROOT')
$savedEnvironment = @{}
foreach ($name in $environmentNames) { $savedEnvironment[$name] = [Environment]::GetEnvironmentVariable($name, 'Process') }
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-headless-candidate-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
try {
    foreach ($framework in @('net8.0','net10.0')) {
        $trxPath = Join-Path $temporaryRoot ("Headless-$framework.trx")
        $env:OPENCV_CSHARP_NATIVE_SMOKE = '1'
        $env:OPENCV_CSHARP_HEADLESS_SMOKE = '1'
        $env:DISPLAY = $null
        $env:WAYLAND_DISPLAY = $null
        $env:LD_LIBRARY_PATH = $null
        $env:OPENCV_CSHARP_OPENCV_RUNTIME_ROOT = $null
        $consoleLines = @(& $dotnet.Source test $project -c Release -f $framework --no-restore --filter 'FullyQualifiedName~HeadlessRuntimeCandidateTests' ("-p:OpenCvNativeRuntimeDir=" + $miniRuntime) --logger ("trx;LogFileName=" + $trxPath) 2>&1)
        $consoleLines | ForEach-Object { Write-Host ([string]$_) }
        if ($LASTEXITCODE -ne 0) { throw "Headless mini candidate tests failed for $framework with exit code $LASTEXITCODE." }
        foreach ($line in $consoleLines) {
            $text = [string]$line
            if ($text -match '^HEADLESS_VIDEOIO_BACKENDS=(.*)$') { $videoioBackends.Add($Matches[1]) }
            if ($text -match '^HEADLESS_VIDEOIO_CAMERA_BACKENDS=(.*)$') { $videoioCameraBackends.Add($Matches[1]) }
        }
        [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
        $counters = $trx.TestRun.ResultSummary.Counters
        if ($null -eq $counters -or [int]$counters.total -ne 1 -or [int]$counters.executed -ne 1 -or [int]$counters.passed -ne 1 -or [int]$counters.failed -ne 0 -or [int]$counters.notExecuted -ne 0) { throw "Headless mini candidate counters drifted for $framework." }
        $results.Add([ordered]@{ targetFramework = $framework; total = [int]$counters.total; executed = [int]$counters.executed; passed = [int]$counters.passed; failed = [int]$counters.failed; skipped = [int]$counters.notExecuted })
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
    runtimePackage = [ordered]@{ packageFileName = [IO.Path]::GetFileName($miniPackage); packageSha256 = $packageHash; nativeDllCount = 7; payload = $payload }
    environment = [ordered]@{ displayUnset = $true; waylandDisplayUnset = $true; ldLibraryPathUnset = $true; runtimeRootUnset = $true }
    frameworks = @($results)
    videoio = [ordered]@{ backends = [string]($videoioBackends -join '||'); cameraBackends = [string]($videoioCameraBackends -join '||'); missingFileOpen = 'returned-false' }
    verifiedCases = @('HighGui NamedWindow deterministic missing-entrypoint or NOT_LINKED path','HighGui DestroyWindow deterministic missing-entrypoint or NOT_LINKED path','HighGui ImShow deterministic missing-entrypoint or NOT_LINKED path','HighGui CreateTrackbar deterministic missing-entrypoint or NOT_LINKED path','HighGui current UI framework deterministic missing-entrypoint or NOT_LINKED path','codec PNG encode remains usable after failed HighGui calls','VideoIO missing-file open returns explicit false and disposes cleanly')
    limitations = @('This candidate evidence covers the existing Mini runtime payload, whose native wrapper omits HighGui entrypoints; it does not create a new headless package identity.', 'Full headless requires a separately rebuilt wrapper/profile without HighGui and is intentionally pending.', 'The VideoIO result is a bounded missing-file negative case; camera/network backend availability and multi-distro consumer gates remain pending.')
}
$outputFullPath = if ([IO.Path]::IsPathRooted($OutputPath)) { [IO.Path]::GetFullPath($OutputPath) } else { [IO.Path]::GetFullPath((Join-Path $repo ($OutputPath -replace '/', [IO.Path]::DirectorySeparatorChar))) }
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
[IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 12) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))
Write-Host "HEADLESS_CANDIDATE_EVIDENCE_MEASURED status=candidate-partial source_commit=$sourceCommit profile=mini-headless-candidate frameworks=net8.0,net10.0 output=$outputFullPath"
