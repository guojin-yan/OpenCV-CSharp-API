[CmdletBinding()]
param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$OpenCvSourceDir,
    [Parameter(Mandatory)][string]$OpenCvInstallDir,
    [string]$FullNativeRuntimeDir = 'artifacts/opencv-5.1.0-win-x64/full',
    [string]$MiniNativeRuntimeDir = 'artifacts/opencv-5.1.0-win-x64/mini',
    [int]$TimeoutSeconds = 600
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalPackageSbom51.Common.ps1')
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if (-not $IsWindows -or [Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture -ne [Runtime.InteropServices.Architecture]::X64) { throw 'Local package SBOM measurement requires Windows x64.' }
if ($TimeoutSeconds -lt 60 -or $TimeoutSeconds -gt 1800) { throw 'TimeoutSeconds must be between 60 and 1800.' }
$status = @(& git -C $repo status --porcelain)
if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) { throw 'Local package SBOM measurement requires a clean repository.' }
$sourceCommit = ([string](& git -C $repo rev-parse HEAD)).Trim()
if ($LASTEXITCODE -ne 0 -or $sourceCommit -notmatch '^[0-9a-f]{40}$') { throw 'Cannot resolve source commit.' }
$sourceSet = Get-LocalPackageSbom51SourceSet -RepositoryRoot $repo
$bindings = Get-ModernApi51PackageBindings -RepositoryRoot $repo -SourceCommit $sourceCommit
$runtimeDirs = @{
    full = (Resolve-Path -LiteralPath (Resolve-ModernApi51Path -RepositoryRoot $repo -Path $FullNativeRuntimeDir)).Path
    mini = (Resolve-Path -LiteralPath (Resolve-ModernApi51Path -RepositoryRoot $repo -Path $MiniNativeRuntimeDir)).Path
}
foreach ($profile in @('full', 'mini')) { Assert-ModernApi51Payload -Directory $runtimeDirs[$profile] -Expected @(@($bindings.packages | Where-Object profile -CEQ $profile)[0].payload) }
$sourceDir = (Resolve-Path -LiteralPath $OpenCvSourceDir).Path
$installDir = (Resolve-Path -LiteralPath $OpenCvInstallDir).Path
if (-not (Test-Path -LiteralPath (Join-Path $sourceDir 'LICENSE') -PathType Leaf) -or
    @(Get-ChildItem -LiteralPath (Join-Path $installDir 'etc/licenses') -File).Count -lt 1) { throw 'Real OpenCV source/install license inputs are required.' }
$artifactRelative = "artifacts/opencv-5.1.0-local-candidate/$sourceCommit"
$artifactRoot = Join-Path $repo $artifactRelative
if (Test-Path -LiteralPath $artifactRoot) { throw 'Candidate artifact directory already exists; retain it and measure from a new clean commit.' }
$sbomRelative = 'packaging/release/opencv-5.1.0-local-sbom'
$sbomDir = Join-Path $repo $sbomRelative
$pwsh = (Get-Command pwsh -ErrorAction Stop).Source
$logDir = Join-Path $artifactRoot 'logs'
New-Item -ItemType Directory -Path $logDir -Force | Out-Null
$created = [DateTime]::UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")

function Invoke-LocalSbomStep {
    param([string]$Name, [string]$Script, [string[]]$Arguments)
    $start = [Diagnostics.ProcessStartInfo]::new()
    $start.FileName = $pwsh; $start.WorkingDirectory = $repo; $start.UseShellExecute = $false; $start.CreateNoWindow = $true
    $start.RedirectStandardOutput = $true; $start.RedirectStandardError = $true
    foreach ($arg in @('-NoProfile', '-File', (Join-Path $repo $Script)) + $Arguments) { [void]$start.ArgumentList.Add($arg) }
    foreach ($envName in @('OPENCV_CSHARP_OPENCV_RUNTIME_ROOT', 'LD_LIBRARY_PATH', 'DOTNET_ADDITIONAL_DEPS', 'DOTNET_SHARED_STORE', 'DOTNET_STARTUP_HOOKS')) { [void]$start.Environment.Remove($envName) }
    $process = [Diagnostics.Process]::new(); $process.StartInfo = $start
    try {
        if (-not $process.Start()) { throw "Cannot start $Name." }
        $stdoutTask = $process.StandardOutput.ReadToEndAsync(); $stderrTask = $process.StandardError.ReadToEndAsync()
        if (-not $process.WaitForExit($TimeoutSeconds * 1000)) { $process.Kill($true); $process.WaitForExit(); throw "$Name timed out." }
        $output = $stdoutTask.GetAwaiter().GetResult() + $stderrTask.GetAwaiter().GetResult()
        [IO.File]::WriteAllText((Join-Path $logDir "$Name.log"), $output, [Text.UTF8Encoding]::new($false))
        if ($process.ExitCode -ne 0) { throw "$Name failed ($($process.ExitCode)).`n$output" }
        Write-Host "LOCAL_SBOM_51_STEP_OK step=$Name"
        return $output
    }
    finally { $process.Dispose() }
}

$managedOutput = Join-Path $artifactRoot 'nupkg-managed'
[void](Invoke-LocalSbomStep -Name 'pack-managed' -Script 'scripts/Pack-Managed.ps1' -Arguments @('-OutputDir', $managedOutput, '-NoRestore'))
foreach ($profile in @('full', 'mini')) {
    $outputDir = Join-Path $artifactRoot "nupkg-win-x64-$profile"
    [void](Invoke-LocalSbomStep -Name "pack-$profile" -Script 'scripts/Pack-Runtime.ps1' -Arguments @('-Rid', 'win-x64', '-RuntimeProfile', $profile,
        '-StageRuntime', '-RequireReleasePreflight', '-OpenCvNativeRuntimeDir', $runtimeDirs[$profile], '-OpenCvRuntimeDir', $runtimeDirs[$profile],
        '-OpenCvSourceDir', $sourceDir, '-OpenCvInstallDir', $installDir, '-StageOutputRoot', (Join-Path $artifactRoot "stage-$profile"), '-OutputDir', $outputDir))
}

$packages = [Collections.Generic.List[object]]::new()
foreach ($role in @('managed', 'full', 'mini')) {
    $id = if ($role -ceq 'managed') { 'JYPPX.OpenCV.CSharp.API' } elseif ($role -ceq 'full') { 'JYPPX.OpenCV.runtime.win-x64' } else { 'JYPPX.OpenCV.runtime.win-x64.mini' }
    $folder = if ($role -ceq 'managed') { 'nupkg-managed' } else { "nupkg-win-x64-$role" }
    $fileName = "$id.5.1.0.nupkg"
    $packagePath = Join-Path (Join-Path $artifactRoot $folder) $fileName
    $row = [ordered]@{
        role = $role; packageId = $id; fileName = $fileName; artifactPath = "$artifactRelative/$folder/$fileName"
        bytes = (Get-Item -LiteralPath $packagePath).Length; sha256 = Get-ModernApi51Sha256 -Path $packagePath
        entryCount = 0; entries = @(Get-LocalPackageSbom51Manifest -PackagePath $packagePath)
        sbom = $null
    }
    $row.entryCount = $row.entries.Count
    Assert-LocalPackageSbom51Payload -Package $row -Bindings $bindings
    $sbomPath = Join-Path $sbomDir "$id.5.1.0.spdx.json"
    $argsForSbom = @('-PackagePath', $packagePath, '-SourceCommit', $sourceCommit, '-Created', $created, '-OpenCvVersion', '5.1.0', '-OutputPath', $sbomPath)
    [void](Invoke-LocalSbomStep -Name "sbom-$role" -Script 'scripts/New-ReleasePackageSbom.ps1' -Arguments $argsForSbom)
    $firstHash = Get-ModernApi51Sha256 -Path $sbomPath
    [void](Invoke-LocalSbomStep -Name "sbom-$role-repeat" -Script 'scripts/New-ReleasePackageSbom.ps1' -Arguments $argsForSbom)
    if ($firstHash -cne (Get-ModernApi51Sha256 -Path $sbomPath)) { throw 'Repeated SPDX generation changed bytes.' }
    [void](Invoke-LocalSbomStep -Name "sbom-$role-check" -Script 'scripts/New-ReleasePackageSbom.ps1' -Arguments ($argsForSbom + '-Check'))
    Assert-LocalPackageSbom51Document -Document (ConvertFrom-LocalPackageSbom51Json -Json (Get-Content -LiteralPath $sbomPath -Raw)) -Package $row -SourceCommit $sourceCommit -Created $created
    $row.sbom = [ordered]@{ path = "$sbomRelative/$id.5.1.0.spdx.json"; bytes = (Get-Item -LiteralPath $sbomPath).Length; sha256 = $firstHash; regenerationIdentical = $true; checkPassed = $true }
    if ($role -cne 'managed') {
        $audit = Invoke-LocalSbomStep -Name "pe-$role" -Script 'scripts/Test-PackagedWindowsRuntimePeClosure.ps1' -Arguments @('-PackagePath', $packagePath, '-RuntimeProfile', $role)
        if ($audit -notmatch 'PACKAGED_WINDOWS_PE_AUDIT_OK' -or $audit -notmatch [regex]::Escape("package_sha256=$($row.sha256)")) { throw 'Package PE closure did not bind the current archive.' }
    }
    $packages.Add([pscustomobject]$row)
}

$consumers = [Collections.Generic.List[object]]::new()
foreach ($framework in @('net8.0', 'net10.0')) {
    foreach ($profile in @('full', 'mini')) {
        $output = Invoke-LocalSbomStep -Name "consumer-$profile-$framework" -Script 'scripts/Test-GitHubPackConsumerRestoreSurface.ps1' -Arguments @(
            '-ArtifactRoot', $artifactRoot, '-RepositoryRoot', $repo, '-ExpectedPackageVersion', '5.1.0.0', '-ExpectedSyntheticRuntimeInputs', 'false',
            '-SelectedRid', 'win-x64', '-SelectedRuntimeProfile', $profile, '-OpenCvVersion', '5.1.0', '-CompileNativeSmoke', '-RunNativeSmoke', '-ConsumerTargetFramework', $framework)
        $marker = if ($profile -ceq 'full') { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,dnn,ml_knn profile=full' } else { 'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,not_linked profile=mini' }
        if (-not $output.Contains($marker, [StringComparison]::Ordinal) -or -not $output.Contains('Consumer package pairs checked: 1.', [StringComparison]::Ordinal)) { throw 'Package consumer did not execute exactly one real native smoke.' }
        $consumers.Add([ordered]@{ profile = $profile; targetFramework = $framework; restore = 'passed'; build = 'passed'; nativeSmoke = 'passed'; marker = $marker
            logSha256 = Get-ModernApi51Sha256 -Path (Join-Path $logDir "consumer-$profile-$framework.log") })
    }
}

if ($sourceSet.sha256 -cne (Get-LocalPackageSbom51SourceSet -RepositoryRoot $repo).sha256) { throw 'Source inputs changed during local package/SBOM measurement.' }
foreach ($package in $packages) { if ($package.sha256 -cne (Get-ModernApi51Sha256 -Path (Join-Path $repo $package.artifactPath))) { throw 'Candidate package changed during measurement.' } }
$record = [ordered]@{
    '$schema' = 'opencv-5.1.0-local-sbom-evidence.schema.json'; schemaVersion = 1; status = 'verified-local-windows-x64'; openCvVersion = '5.1.0'; packageVersion = '5.1.0'
    sourceCommit = $sourceCommit; created = $created; dotnetSdk = ([string](& dotnet --version)).Trim(); sourceInputSet = $sourceSet
    nativePackageConsumerEvidence = $bindings.reference; artifactRoot = $artifactRelative; packages = @($packages); consumers = @($consumers)
    limits = [ordered]@{ localPackageSubset = $true; published = $false; signed = $false; approved = $false; finalReleaseSetComplete = $false; crossRidValidated = $false; transitiveComponentInventoryComplete = $false }
    limitations = @('This local subset contains one 15-TFM managed package and Windows x64 Full/Mini packages; it is not the complete release support set.',
        'SPDX documents bind archive entries, packaged licenses, OpenCV version and source commit. Third-party transitive component/license attribution and vulnerability review remain separate.',
        'Native DLL bytes match the historical verified 5.1.0 package-consumer payload; these newly normalized archives have independent hashes.',
        'Final release packages, hosted/cross-RID consumer evidence, SBOM approval, signatures and publication still require final candidate review.')
}
$recordPath = Join-Path $repo 'packaging/release/opencv-5.1.0-local-sbom-evidence.json'
$json = (($record | ConvertTo-Json -Depth 25) -replace "`r`n", "`n").TrimEnd() + "`n"
if (-not (Test-Json -Json $json -SchemaFile (Join-Path $repo 'packaging/release/opencv-5.1.0-local-sbom-evidence.schema.json'))) { throw 'Local SBOM evidence schema validation failed.' }
[IO.File]::WriteAllText($recordPath, $json, [Text.UTF8Encoding]::new($false))
Write-Host "LOCAL_SBOM_51_EVIDENCE_MEASURED source_commit=$sourceCommit packages=3 managed_tfms=15 native_dlls=50 consumers=4 documents=3"
