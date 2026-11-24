[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'packaging/performance/perf-001-5.1.0-baseline.json',
    [int]$TimeoutSeconds = 180
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Performance51.Common.ps1')
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if (-not $IsWindows -or [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne [Runtime.InteropServices.Architecture]::X64) { throw 'Performance 5.1 measurement requires Windows x64.' }
if ($TimeoutSeconds -lt 30 -or $TimeoutSeconds -gt 600) { throw 'TimeoutSeconds must be between 30 and 600.' }
$output = Resolve-ModernApi51Path -RepositoryRoot $repo -Path $OutputPath
if (Test-Path -LiteralPath $output) { throw 'Performance evidence is immutable; select a new output path.' }
$status = @(& git -C $repo status --porcelain)
if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) { throw 'Performance measurement requires a clean repository.' }
$sourceCommit = ([string](& git -C $repo rev-parse HEAD)).Trim()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Cannot resolve the measured source.' }
$sourceSet = Get-Performance51SourceSet -RepositoryRoot $repo
$package = Get-Performance51Package -RepositoryRoot $repo -SourceCommit $sourceCommit
$packagePath = Join-Path $repo $package.artifactPath
if ((Get-ModernApi51Sha256 -Path $packagePath) -cne $package.sha256 -or (Get-Item -LiteralPath $packagePath).Length -ne $package.bytes) { throw 'Performance package bytes drifted.' }
$sbom = Get-Content -LiteralPath (Join-Path $repo $package.evidence.path) -Raw | ConvertFrom-Json
$full = @($sbom.packages | Where-Object role -CEQ 'full')[0]
if ((Get-LocalPackageSbom51Manifest -PackagePath $packagePath | ConvertTo-Json -Depth 5 -Compress) -cne ($full.entries | ConvertTo-Json -Depth 5 -Compress)) { throw 'Performance package entries differ from their measured manifest.' }
$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
$processor = @(Get-CimInstance Win32_Processor | Sort-Object DeviceID | ForEach-Object { ([string]$_.Name).Trim() }) -join '; '
$runner = [ordered]@{
    os = [Runtime.InteropServices.RuntimeInformation]::OSDescription; architecture = 'x64'; configuration = 'Release'
    dotnetSdk = ([string](& $dotnet --version)).Trim(); cpu = $processor
    logicalProcessors = [Environment]::ProcessorCount; stopwatchFrequency = [Diagnostics.Stopwatch]::Frequency
    dotnetRuntimes = @(& $dotnet --list-runtimes | ForEach-Object { ([string]$_ -replace ' \[.*\]$', '').Trim() })
}
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-perf-51-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null
$artifactRoot = Join-Path $repo "artifacts/performance-5.1.0/$sourceCommit"
New-Item -ItemType Directory -Path $artifactRoot -Force | Out-Null
$jobs = @(
    [pscustomobject]@{ scenario = 'codec'; project = 'CodecBenchmark'; tfm = 'net8.0' },
    [pscustomobject]@{ scenario = 'typedMat'; project = 'TypedMatBenchmark'; tfm = 'net8.0' },
    [pscustomobject]@{ scenario = 'typedMat'; project = 'TypedMatBenchmark'; tfm = 'net10.0' },
    [pscustomobject]@{ scenario = 'scenario'; project = 'ScenarioBenchmark'; tfm = 'net10.0' }
)
foreach ($job in $jobs) {
    $directory = Join-Path $temporaryRoot "$($job.project)-$($job.tfm)"
    & $dotnet build (Join-Path $repo "tools/$($job.project)/$($job.project).csproj") -c Release -f $job.tfm '-p:OpenCvNativeRuntimeDir=' -o $directory | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Performance harness build failed: $($job.project)/$($job.tfm)." }
    Assert-ModernApi51Payload -Directory $directory -Expected @()
    $archive = [IO.Compression.ZipFile]::OpenRead($packagePath)
    try {
        foreach ($row in $package.payload) {
            $entry = $archive.GetEntry("runtimes/win-x64/native/$($row.name)")
            [IO.Compression.ZipFileExtensions]::ExtractToFile($entry, (Join-Path $directory $row.name), $false)
        }
    }
    finally { $archive.Dispose() }
    Assert-ModernApi51Payload -Directory $directory -Expected $package.payload
    $job | Add-Member -NotePropertyName directory -NotePropertyValue $directory
}
$runs = [Collections.Generic.List[object]]::new()
for ($round = 1; $round -le 3; $round++) {
    foreach ($job in $jobs) {
        $start = [Diagnostics.ProcessStartInfo]::new()
        $start.FileName = $dotnet; $start.WorkingDirectory = $job.directory; $start.UseShellExecute = $false; $start.CreateNoWindow = $true
        $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
        foreach ($key in @($start.Environment.Keys)) {
            if ($key -match '^(DOTNET_|COMPlus_|CORECLR_|COR_|OPENCV_)' -or $key -in @('LD_LIBRARY_PATH', 'DISPLAY', 'WAYLAND_DISPLAY')) { [void]$start.Environment.Remove($key) }
        }
        $start.Environment['PATH'] = @((Split-Path -Parent $dotnet), (Join-Path $env:SystemRoot 'System32'), $env:SystemRoot) -join ';'
        [void]$start.ArgumentList.Add((Join-Path $job.directory "$($job.project).dll"))
        Assert-ModernApi51Payload -Directory $job.directory -Expected $package.payload
        $process = [Diagnostics.Process]::new(); $process.StartInfo = $start
        try {
            if (-not $process.Start()) { throw 'Cannot start isolated performance harness.' }
            $stdoutTask = $process.StandardOutput.ReadToEndAsync(); $stderrTask = $process.StandardError.ReadToEndAsync()
            if (-not $process.WaitForExit($TimeoutSeconds * 1000)) { $process.Kill($true); $process.WaitForExit(); throw 'Performance harness timed out.' }
            $stdout = $stdoutTask.GetAwaiter().GetResult(); $stderr = $stderrTask.GetAwaiter().GetResult()
            if ($process.ExitCode -ne 0) { throw "Performance harness failed: $stdout`n$stderr" }
        }
        finally { $process.Dispose() }
        $lines = @($stdout -split '\r?\n' | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        if ($lines.Count -ne 1) { throw 'Performance harness must emit exactly one JSON result.' }
        $json = [string]$lines[0]
        Assert-Performance51Output -Scenario $job.scenario -TargetFramework $job.tfm -Output ($json | ConvertFrom-Json)
        $runs.Add([ordered]@{
            round = $round; scenario = $job.scenario; targetFramework = $job.tfm; nativeDllCount = $package.payload.Count
            apiAssemblySha256 = Get-ModernApi51Sha256 -Path (Join-Path $job.directory 'JYPPX.OpenCV.CSharp.API.dll')
            harnessSha256 = Get-ModernApi51Sha256 -Path (Join-Path $job.directory "$($job.project).dll")
            stdoutJson = $json; stdoutSha256 = Get-Performance51TextHash -Text $json
            stderr = $stderr; stderrSha256 = Get-Performance51TextHash -Text $stderr
            isolation = [ordered]@{
                pathRestricted = $start.Environment['PATH'] -ceq (@((Split-Path -Parent $dotnet), (Join-Path $env:SystemRoot 'System32'), $env:SystemRoot) -join ';')
                runtimeOverridesRemoved = @($start.Environment.Keys | Where-Object { $_ -match '^OPENCV_' -or $_ -in @('LD_LIBRARY_PATH', 'DISPLAY', 'WAYLAND_DISPLAY') }).Count -eq 0
                dotnetOverridesRemoved = @($start.Environment.Keys | Where-Object { $_ -match '^(DOTNET_|COMPlus_|CORECLR_|COR_)' }).Count -eq 0
                payloadVerified = $true
            }
        })
        [IO.File]::WriteAllText((Join-Path $artifactRoot "$round-$($job.scenario)-$($job.tfm).json"), $json + "`n", [Text.UTF8Encoding]::new($false))
        Write-Host "PERFORMANCE_51_RUN_OK round=$round scenario=$($job.scenario) tfm=$($job.tfm) dlls=43"
    }
}
if (($sourceSet | ConvertTo-Json -Depth 5 -Compress) -cne (Get-Performance51SourceSet -RepositoryRoot $repo | ConvertTo-Json -Depth 5 -Compress) -or
    $sourceCommit -cne ([string](& git -C $repo rev-parse HEAD)).Trim()) { throw 'Performance source changed during measurement.' }
$record = [ordered]@{
    '$schema' = 'perf-001-5.1.0-baseline.schema.json'; schemaVersion = 1; status = 'measured-baseline'; openCvVersion = '5.1.0'
    sourceCommit = $sourceCommit; sourceInputSet = $sourceSet; measuredAtUtc = [DateTime]::UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
    runner = $runner; runtimePackage = $package; runs = @($runs.ToArray()); metrics = Get-Performance51Metrics -Runs $runs.ToArray()
    thresholds = [ordered]@{ elapsedTicksPercent = 20; managedAllocatedBytesPercent = 10; peakWorkingSetBytesPercent = 10 }
    limits = [ordered]@{ historical50Comparison = $false; crossRidValidated = $false; stablePreviewApisPromoted = $false; finalReleaseApproved = $false }
    limitations = @('Three independent processes per harness, each with 100 warmed iterations; summaries are per-metric medians.',
        'This initializes the OpenCV 5.1 Windows x64 Full baseline. Historical 5.0 measurements use different native bytes and are not a regression comparator.',
        'Comparisons require identical runner, SDK/runtimes, workloads and exact runtime package/DLL hashes; thresholds are elapsed +20%, allocation +10%, sampled peak working set +10%.',
        'Video uses a three-frame MJPG AVI, and DNN uses the fixed 147-byte identity ONNX model on CPU. Camera, network, GPU, large models and other RIDs remain unmeasured.',
        'WorkingSet64 is sampled between operations, not a native allocator high-water mark; microbenchmark timings are runner-specific and do not establish stable API promotion.')
}
New-Item -ItemType Directory -Path (Split-Path -Parent $output) -Force | Out-Null
[IO.File]::WriteAllText($output, ($record | ConvertTo-Json -Depth 16) + "`n", [Text.UTF8Encoding]::new($false))
Write-Host "PERFORMANCE_51_MEASURED source=$sourceCommit runs=12 metrics=14 dlls=43 output=$output artifacts=$artifactRoot temporary=$temporaryRoot"
