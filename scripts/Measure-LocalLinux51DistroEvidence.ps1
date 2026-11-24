[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$DebianOutput,
    [Parameter(Mandatory)][string]$FedoraOutput,
    [Parameter(Mandatory)][string]$DebianContainer,
    [Parameter(Mandatory)][string]$FedoraContainer,
    [Parameter(Mandatory)][string]$KitRoot,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'packaging/runtime/opencv-5.1.0-local-linux-distro-consumer-evidence.json'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalLinux51Distro.Common.ps1')
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$kit = (Resolve-Path -LiteralPath $KitRoot).Path
$baselinePath = 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json'
$baselineFile = Join-Path $repo $baselinePath
$baseline = Get-Content $baselineFile -Raw | ConvertFrom-Json
$matrixPath = 'packaging/runtime/runtime-generic-linux-5.1-local-consumer-matrix.json'
$graphPath = 'packaging/runtime/runtime-distro-rid-graph.json'
$graphText = [IO.File]::ReadAllText((Join-Path $repo $graphPath)).Replace("`r`n", "`n").Replace("`r", "`n").TrimEnd() + "`n"
$dockerHost = (& docker info --format '{"architecture":{{json .Architecture}},"platform":{{json .OSType}},"serverVersion":{{json .ServerVersion}}}') | ConvertFrom-Json
if ($LASTEXITCODE -ne 0) { throw 'Cannot inspect native Docker host.' }
$record = [ordered]@{
    schemaVersion = 1; evidenceKind = 'local-linux-5.1-three-distro-preview-consumers'
    status = 'verified-local-three-distro-x64-preview'; openCvVersion = '5.1.0'; packageVersion = '5.1.0'
    sourceBaseCommit = (& git -C $repo rev-parse HEAD); worktreeChangesIncluded = $true
    execution = 'local-docker'; hosted = $false; publicationAllowed = $false; genericStablePromoted = $false
    dockerHost = $dockerHost
    baselineEvidence = [ordered]@{ path = $baselinePath; sha256 = (Get-FileHash -LiteralPath $baselineFile -Algorithm SHA256).Hash.ToLowerInvariant() }
    matrix = [ordered]@{ path = $matrixPath; sha256 = (Get-FileHash -LiteralPath (Join-Path $repo $matrixPath) -Algorithm SHA256).Hash.ToLowerInvariant() }
    baseGraphJson = $graphText; baseGraphSha256 = Get-LocalLinux51TextHash $graphText
    sourceInputs = @(Get-LocalLinux51DistroSourceInputs $repo)
    kitManifest = @(Get-LocalLinux51DistroKitManifest $kit)
    toolchain = [ordered]@{
        powerShellArchiveSha256 = (Get-FileHash -LiteralPath (Join-Path $kit 'toolchain/powershell.tar.gz') -Algorithm SHA256).Hash.ToLowerInvariant()
        dotNetArchiveSha512 = (Get-FileHash -LiteralPath (Join-Path $kit 'toolchain/dotnet.tar.gz') -Algorithm SHA512).Hash.ToLowerInvariant()
        bootstrapCaSha256 = (Get-FileHash -LiteralPath (Join-Path $kit 'toolchain/ubuntu-ca-certificates.crt') -Algorithm SHA256).Hash.ToLowerInvariant()
    }
    consumers = @(
        (Get-LocalLinux51DistroConsumerRecord $repo (Resolve-Path $DebianOutput).Path $DebianContainer 'debian.12-x64'),
        (Get-LocalLinux51DistroConsumerRecord $repo (Resolve-Path $FedoraOutput).Path $FedoraContainer 'fedora.44-x64')
    )
    coverage = [ordered]@{
        consumerRids = @('ubuntu.22.04-x64', 'debian.12-x64', 'fedora.44-x64')
        profiles = @('full', 'mini'); totalProfilePairs = 6; baselineProfilePairs = 2; newProfilePairs = 4
        localThreeDistroConsumerGateComplete = $true
        baselineProducerSourceCommit = $baseline.sourceBaseCommit
        baselineUbuntuEvidenceReused = $true; newProducerBuildExecuted = $false
        arm64Validated = $false; hostedValidated = $false; dnnModelInferenceValidated = $false
        videoFileOrDeviceValidated = $false; finalReleaseApproved = $false
    }
}
$output = if ([IO.Path]::IsPathRooted($OutputPath)) { $OutputPath } else { Join-Path $repo $OutputPath }
[IO.File]::WriteAllText($output, (($record | ConvertTo-Json -Depth 22) -replace "`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
& (Join-Path $PSScriptRoot 'Test-LocalLinux51DistroEvidence.ps1') -RepositoryRoot $repo -EvidencePath $output -RequirePackageArtifacts
Write-Host "LOCAL_LINUX_51_DISTRO_EVIDENCE_MEASURED output=$output"
