[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArtifactDirectory,
    [Parameter(Mandatory)][string]$KitRoot,
    [Parameter(Mandatory)][string[]]$Containers,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'packaging/runtime/opencv-5.1.0-local-linux-module-consumer-evidence.json'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalLinux51Module.Common.ps1')
$repo = (Resolve-Path $RepositoryRoot).Path; $kit = (Resolve-Path $KitRoot).Path
$specs = @(Get-LocalLinux51ModuleSpecs)
if ($Containers.Count -ne 3) { throw 'Expected Ubuntu, Debian and Fedora container names in that order.' }
$baselinePath = 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json'
$distroPath = 'packaging/runtime/opencv-5.1.0-local-linux-distro-consumer-evidence.json'
$managedSbomPath = 'packaging/release/opencv-5.1.0-local-sbom/JYPPX.OpenCV.CSharp.API.5.1.0.spdx.json'
$dockerHost = (& docker info --format '{"architecture":{{json .Architecture}},"platform":{{json .OSType}},"serverVersion":{{json .ServerVersion}}}') | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect Docker host.' }
$kitManifest = foreach ($file in Get-ChildItem $kit -Recurse -File | Sort-Object FullName -CaseSensitive) {
    [ordered]@{ path = [IO.Path]::GetRelativePath($kit, $file.FullName).Replace('\', '/'); bytes = $file.Length; sha256 = (Get-FileHash $file.FullName -Algorithm SHA256).Hash.ToLowerInvariant() }
}
$record = [ordered]@{
    schemaVersion = 1; evidenceKind = 'local-linux-5.1-module-package-consumers'; status = 'verified-local-three-distro-x64-preview'
    openCvVersion = '5.1.0'; packageVersion = '5.1.0'; sourceBaseCommit = (& git -C $repo rev-parse HEAD); worktreeChangesIncluded = $true
    execution = 'local-docker'; hosted = $false; publicationAllowed = $false; genericStablePromoted = $false
    baselineEvidence = [ordered]@{ path = $baselinePath; sha256 = (Get-FileHash (Join-Path $repo $baselinePath) -Algorithm SHA256).Hash.ToLowerInvariant() }
    originalDistroEvidence = [ordered]@{ path = $distroPath; sha256 = (Get-FileHash (Join-Path $repo $distroPath) -Algorithm SHA256).Hash.ToLowerInvariant() }
    managedSbomEvidence = [ordered]@{ path = $managedSbomPath; sha256 = (Get-FileHash (Join-Path $repo $managedSbomPath) -Algorithm SHA256).Hash.ToLowerInvariant() }
    dockerHost = $dockerHost; sourceInputs = @(Get-LocalLinux51ModuleSourceInputs $repo); kitManifest = @($kitManifest)
    toolchains = @(
        [ordered]@{ targetFramework = 'net8.0'; sdkVersion = '8.0.428'; runtimeVersion = '8.0.34'; archive = 'toolchain/dotnet8.tar.gz'; archiveSha512 = (Get-FileHash (Join-Path $kit 'toolchain/dotnet8.tar.gz') -Algorithm SHA512).Hash.ToLowerInvariant(); officialUrl = 'https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.428/dotnet-sdk-8.0.428-linux-x64.tar.gz' },
        [ordered]@{ targetFramework = 'net10.0'; sdkVersion = '10.0.401'; runtimeVersion = '10.0.12'; archive = 'toolchain/dotnet10.tar.gz'; archiveSha512 = (Get-FileHash (Join-Path $kit 'toolchain/dotnet10.tar.gz') -Algorithm SHA512).Hash.ToLowerInvariant(); officialUrl = 'https://builds.dotnet.microsoft.com/dotnet/Sdk/10.0.401/dotnet-sdk-10.0.401-linux-x64.tar.gz' }
    )
    consumers = @(for ($index = 0; $index -lt 3; $index++) {
        Get-LocalLinux51ModuleConsumerRecord $repo (Resolve-Path (Join-Path $ArtifactDirectory $specs[$index].rid)).Path $Containers[$index] $specs[$index].rid
    })
    coverage = [ordered]@{
        consumerRids = @($specs.rid); targetFrameworks = @('net8.0', 'net10.0'); profiles = @('full', 'mini'); nativeRuns = 12
        fullOnnxInferenceRuns = 6; miniDnnOmissionRuns = 6; mjpgAviWriteReadRuns = 12; freshPackageCaches = 12
        newProducerBuildExecuted = $false; arm64Validated = $false; hostedValidated = $false
        cameraOrNetworkValidated = $false; arbitraryDnnModelsValidated = $false; finalReleaseApproved = $false
    }
}
$output = if ([IO.Path]::IsPathRooted($OutputPath)) { $OutputPath } else { Join-Path $repo $OutputPath }
[IO.File]::WriteAllText($output, (($record | ConvertTo-Json -Depth 24) -replace "`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
& (Join-Path $PSScriptRoot 'Test-LocalLinux51ModuleEvidence.ps1') -RepositoryRoot $repo -EvidencePath $output -RequireArtifacts
Write-Host "LOCAL_LINUX_51_MODULE_EVIDENCE_MEASURED output=$output"
