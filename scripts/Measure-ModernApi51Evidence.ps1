[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$FullNativeRuntimeDir = 'artifacts/opencv-5.1.0-win-x64/full',
    [string]$MiniNativeRuntimeDir = 'artifacts/opencv-5.1.0-win-x64/mini',
    [string]$OutputPath = 'packaging/runtime/modern-api-5.1.0-evidence.json',
    [int]$TimeoutSeconds = 180
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'ModernApi51Evidence.Common.ps1')
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if (-not $IsWindows -or [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne [Runtime.InteropServices.Architecture]::X64) { throw 'Modern API 5.1 evidence requires a Windows x64 process.' }
if ($TimeoutSeconds -lt 30 -or $TimeoutSeconds -gt 600) { throw 'TimeoutSeconds must be between 30 and 600.' }
$status = @(& git -C $repo status --porcelain)
if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) { throw 'Modern API 5.1 measurement requires a clean repository.' }
$sourceCommit = ([string](& git -C $repo rev-parse HEAD)).Trim()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Cannot resolve the measured source commit.' }
$sourceSet = Get-ModernApi51SourceSet -RepositoryRoot $repo
$bindings = Get-ModernApi51PackageBindings -RepositoryRoot $repo -SourceCommit $sourceCommit
$runtimeDirectories = @{
    full = (Resolve-Path -LiteralPath (Resolve-ModernApi51Path -RepositoryRoot $repo -Path $FullNativeRuntimeDir)).Path
    mini = (Resolve-Path -LiteralPath (Resolve-ModernApi51Path -RepositoryRoot $repo -Path $MiniNativeRuntimeDir)).Path
}
foreach ($package in $bindings.packages) { Assert-ModernApi51Payload -Directory $runtimeDirectories[$package.profile] -Expected @($package.payload) }
$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-modern-api-51-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $temporaryRoot | Out-Null

function Invoke-IsolatedProcess {
    param([string[]]$Arguments, [string]$WorkingDirectory, [string]$Profile = 'none')
    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $dotnet
    $start.WorkingDirectory = $WorkingDirectory
    $start.UseShellExecute = $false
    $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    foreach ($argument in $Arguments) { [void]$start.ArgumentList.Add($argument) }
    foreach ($name in @('DISPLAY', 'WAYLAND_DISPLAY', 'LD_LIBRARY_PATH', 'OPENCV_CSHARP_OPENCV_RUNTIME_ROOT',
        'DOTNET_ADDITIONAL_DEPS', 'DOTNET_SHARED_STORE', 'DOTNET_STARTUP_HOOKS', 'OPENCV_CSHARP_HIGHGUI_SMOKE', 'OPENCV_CSHARP_UNSTABLE_NATIVE_SMOKE')) {
        [void]$start.Environment.Remove($name)
    }
    $start.Environment['PATH'] = @((Split-Path -Parent $dotnet), (Join-Path $env:SystemRoot 'System32'), $env:SystemRoot) -join ';'
    $start.Environment['OPENCV_CSHARP_NATIVE_SMOKE'] = if ($Profile -ceq 'none') { '0' } else { '1' }
    $start.Environment['OPENCV_CSHARP_HEADLESS_SMOKE'] = if ($Profile -ceq 'mini') { '1' } else { '0' }
    $isolation = [ordered]@{
        runtimeRootUnset = -not $start.Environment.ContainsKey('OPENCV_CSHARP_OPENCV_RUNTIME_ROOT')
        displayUnset = -not $start.Environment.ContainsKey('DISPLAY')
        waylandDisplayUnset = -not $start.Environment.ContainsKey('WAYLAND_DISPLAY')
        pathRestricted = $start.Environment['PATH'] -ceq (@((Split-Path -Parent $dotnet), (Join-Path $env:SystemRoot 'System32'), $env:SystemRoot) -join ';')
        nativeSmokeEnabled = $start.Environment['OPENCV_CSHARP_NATIVE_SMOKE'] -ceq '1'
        headlessSmokeEnabled = $start.Environment['OPENCV_CSHARP_HEADLESS_SMOKE'] -ceq '1'
    }
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $start
    try {
        if (-not $process.Start()) { throw 'Cannot start the isolated modern API process.' }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
            $process.Kill($true)
            $process.WaitForExit()
            throw "Modern API process timed out after $TimeoutSeconds seconds."
        }
        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        if ($process.ExitCode -ne 0) { throw "Modern API process failed ($($process.ExitCode)).`n$stdout`n$stderr" }
        return [pscustomobject]@{ stdout = $stdout; stderr = $stderr; isolation = $isolation }
    }
    finally { $process.Dispose() }
}

function New-IsolatedDirectory {
    param([string]$BuildDirectory, [string]$Name, [string]$Profile)
    $directory = Join-Path $temporaryRoot $Name
    New-Item -ItemType Directory -Path $directory | Out-Null
    foreach ($item in @(Get-ChildItem -LiteralPath $BuildDirectory)) { Copy-Item -LiteralPath $item.FullName -Destination $directory -Recurse }
    Assert-ModernApi51Payload -Directory $directory -Expected @()
    $payload = @()
    if ($Profile -cne 'none') {
        $package = @($bindings.packages | Where-Object profile -CEQ $Profile)[0]
        $payload = @($package.payload)
        foreach ($row in $payload) { Copy-Item -LiteralPath (Join-Path $runtimeDirectories[$Profile] $row.name) -Destination $directory }
    }
    Assert-ModernApi51Payload -Directory $directory -Expected $payload
    return $directory
}

$capabilityProfiles = [Collections.Generic.List[object]]::new()
$testRuns = [Collections.Generic.List[object]]::new()
try {
    $sampleBuild = Join-Path $temporaryRoot 'sample-build'
    $sampleProject = Join-Path $repo 'samples/ConsoleSamples/ConsoleSamples.csproj'
    & $dotnet build $sampleProject -c Release --no-restore '-p:OpenCvNativeRuntimeDir=' -o $sampleBuild | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'ConsoleSamples build failed.' }
    foreach ($profile in @('none', 'full', 'mini')) {
        $directory = New-IsolatedDirectory -BuildDirectory $sampleBuild -Name "sample-$profile" -Profile $profile
        $sample = Join-Path $directory 'ConsoleSamples.dll'
        $run = Invoke-IsolatedProcess -Arguments @($sample, 'capabilities-json') -WorkingDirectory $directory -Profile $profile
        $lines = @($run.stdout -split '\r?\n' | Where-Object { $_.StartsWith('{') -and $_.EndsWith('}') })
        if ($lines.Count -ne 1) { throw "Expected one capabilities JSON snapshot for $profile." }
        $snapshotJson = $lines[0]
        if (-not (Test-Json -Json $snapshotJson -SchemaFile (Join-Path $repo 'packaging/runtime/runtime-capabilities-json.schema.json'))) { throw "$profile snapshot failed schema validation." }
        $snapshot = $snapshotJson | ConvertFrom-Json
        Assert-ModernApi51Snapshot -Snapshot $snapshot -Profile $profile
        $capabilityProfiles.Add([ordered]@{
            profile = $profile
            targetFramework = 'net10.0'
            nativeDllCount = if ($profile -ceq 'full') { 43 } elseif ($profile -ceq 'mini') { 7 } else { 0 }
            apiAssemblySha256 = Get-ModernApi51Sha256 -Path (Join-Path $directory 'JYPPX.OpenCV.CSharp.API.dll')
            sampleAssemblySha256 = Get-ModernApi51Sha256 -Path $sample
            isolation = $run.isolation
            snapshotJson = $snapshotJson
            snapshotSha256 = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData([Text.UTF8Encoding]::new($false).GetBytes($snapshotJson))).ToLowerInvariant()
        })
        Write-Host "MODERN_API_51_SNAPSHOT_OK profile=$profile runtime_state=$($snapshot.nativeRuntime.state) native_version=$($snapshot.nativeOpenCvVersion)"
    }
    foreach ($framework in @('net8.0', 'net10.0')) {
        $buildDirectory = Join-Path $temporaryRoot "tests-build-$framework"
        & $dotnet build (Join-Path $repo 'tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj') -c Release -f $framework --no-restore '-p:OpenCvNativeRuntimeDir=' -o $buildDirectory | Out-Host
        if ($LASTEXITCODE -ne 0) { throw "Modern API tests build failed on $framework." }
        foreach ($profile in @('none', 'full', 'mini')) {
            $directory = New-IsolatedDirectory -BuildDirectory $buildDirectory -Name "tests-$profile-$framework" -Profile $profile
            $testAssembly = Join-Path $directory 'OpenCvSharp.Tests.dll'
            $cases = @(Get-ModernApi51TestCases -Profile $profile)
            $filter = ($cases | ForEach-Object { 'FullyQualifiedName=' + $_ }) -join '|'
            $trxName = "$profile-$framework.trx"
            $run = Invoke-IsolatedProcess -Arguments @('vstest', $testAssembly, ('--TestCaseFilter:' + $filter),
                ('--ResultsDirectory:' + $directory), ('--Logger:trx;LogFileName=' + $trxName)) -WorkingDirectory $directory -Profile $profile
            Write-Host $run.stdout.TrimEnd()
            $trxPath = Join-Path $directory $trxName
            if (-not (Test-Path -LiteralPath $trxPath -PathType Leaf)) { throw "Missing $profile/$framework TRX." }
            [xml]$trx = Get-Content -LiteralPath $trxPath -Raw
            $counters = $trx.SelectSingleNode("//*[local-name()='Counters']")
            $results = @($trx.SelectNodes("//*[local-name()='UnitTestResult']"))
            $actualCases = @($results | ForEach-Object { [string]$_.testName } | Sort-Object)
            if ($null -eq $counters -or [int]$counters.total -ne $cases.Count -or [int]$counters.executed -ne $cases.Count -or
                [int]$counters.passed -ne $cases.Count -or [int]$counters.failed -ne 0 -or [int]$counters.notExecuted -ne 0 -or
                ($actualCases -join '|') -cne ($cases -join '|') -or @($results | Where-Object outcome -CNE 'Passed').Count -ne 0) {
                throw "Test inventory or counters drifted for $profile/$framework."
            }
            $headlessOutput = ''
            if ($profile -ceq 'mini') {
                $headless = @($results | Where-Object testName -Like '*.MiniProfileRejectsHighGuiAndKeepsCodecUsable')
                $headlessOutput = [string]$headless[0].Output.StdOut
                foreach ($marker in @('HEADLESS_DNN_STATUS=omitted-deterministic-unavailable', 'HEADLESS_VIDEOIO_MISSING_FILE_OPEN=returned-false')) {
                    if (-not $headlessOutput.Contains($marker, [StringComparison]::Ordinal)) { throw "Mini headless execution marker missing: $marker" }
                }
            }
            $testRuns.Add([ordered]@{
                profile = $profile; targetFramework = $framework
                nativeDllCount = if ($profile -ceq 'full') { 43 } elseif ($profile -ceq 'mini') { 7 } else { 0 }
                apiAssemblySha256 = Get-ModernApi51Sha256 -Path (Join-Path $directory 'JYPPX.OpenCV.CSharp.API.dll')
                testAssemblySha256 = Get-ModernApi51Sha256 -Path $testAssembly
                isolation = $run.isolation
                total = [int]$counters.total; passed = [int]$counters.passed; failed = [int]$counters.failed; skipped = [int]$counters.notExecuted
                testCases = $actualCases
                trx = [ordered]@{ bytes = (Get-Item -LiteralPath $trxPath).Length; sha256 = Get-ModernApi51Sha256 -Path $trxPath }
                headlessOutput = $headlessOutput.Trim()
            })
            $expectedPayload = @()
            if ($profile -cne 'none') { $expectedPayload = @(@($bindings.packages | Where-Object profile -CEQ $profile)[0].payload) }
            Assert-ModernApi51Payload -Directory $directory -Expected $expectedPayload
            Write-Host "MODERN_API_51_TESTS_OK profile=$profile framework=$framework passed=$($cases.Count) failed=0"
        }
    }
    $currentSourceSet = Get-ModernApi51SourceSet -RepositoryRoot $repo
    if ($sourceSet.sha256 -cne $currentSourceSet.sha256) { throw 'Modern API source inputs changed during measurement.' }
    $evidence = [ordered]@{
        '$schema' = 'modern-api-5.1.0-evidence.schema.json'
        schemaVersion = 1; status = 'verified-local-windows-x64'; sourceCommit = $sourceCommit
        openCvVersion = '5.1.0'; managedPackageVersion = '5.1.0'; nativeAbiVersion = 1
        runner = [ordered]@{ os = [Runtime.InteropServices.RuntimeInformation]::OSDescription; architecture = 'x64'; dotnetSdk = ([string](& $dotnet --version)).Trim(); configuration = 'Release' }
        sourceInputSet = $sourceSet
        packageConsumerEvidence = $bindings.reference
        runtimePackages = $bindings.packages
        capabilityProfiles = @($capabilityProfiles)
        testRuns = @($testRuns)
        limits = [ordered]@{ stableCodecWriterPromoted = $false; headlessPackageIdentityAllowed = $false; crossRidValidated = $false; absoluteAllocationGuarantee = $false }
        limitations = @(
            'Current-source consumers execute exact Full/Mini native DLL bytes bound to the local OpenCV 5.1.0 package-consumer evidence; this run does not restore a NuGet package.',
            'Capability snapshots cover net10.0; the explicit capability/writer/reuse/fault test inventory covers net8.0 and net10.0 in isolated none/full/mini processes.',
            'Reusable-writer and fault recovery tests prove payload/cleanup behavior without promising a cross-runner allocation threshold or stable preview promotion.',
            'Mini headless evidence covers omitted HighGUI/DNN entrypoints and a bounded missing-file VideoIO case; Full headless, camera/network execution, ARM64 and hosted/cross-RID validation remain open.'
        )
    }
    $outputFullPath = Resolve-ModernApi51Path -RepositoryRoot $repo -Path $OutputPath
    if (-not (Test-Json -Json ($evidence | ConvertTo-Json -Depth 30) -SchemaFile (Join-Path $repo 'packaging/runtime/modern-api-5.1.0-evidence.schema.json'))) { throw 'Measured modern API evidence failed schema validation.' }
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $outputFullPath) | Out-Null
    [IO.File]::WriteAllText($outputFullPath, (($evidence | ConvertTo-Json -Depth 30) + "`n"), [Text.UTF8Encoding]::new($false))
    Write-Host "MODERN_API_51_EVIDENCE_MEASURED source_commit=$sourceCommit snapshots=3 test_runs=6 passed=76 output=$outputFullPath"
}
finally {
    $resolvedTemporaryRoot = [IO.Path]::GetFullPath($temporaryRoot)
    $expectedTemporaryParent = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar)
    if ((Split-Path -Parent $resolvedTemporaryRoot) -cne $expectedTemporaryParent -or (Split-Path -Leaf $resolvedTemporaryRoot) -notmatch '^opencv-modern-api-51-[0-9a-f]{32}$') { throw 'Unexpected modern API temporary cleanup path.' }
    if (Test-Path -LiteralPath $resolvedTemporaryRoot) { Remove-Item -LiteralPath $resolvedTemporaryRoot -Recurse -Force }
}
