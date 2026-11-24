[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ProducerOutput,
    [Parameter(Mandatory)][string]$ConsumerOutput,
    [Parameter(Mandatory)][string]$ProducerContainer,
    [Parameter(Mandatory)][string]$ConsumerContainer,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalLinux51.Common.ps1')
$repo = (Resolve-Path $RepositoryRoot).Path
$producerRoot = (Resolve-Path $ProducerOutput).Path
$consumerRoot = (Resolve-Path $ConsumerOutput).Path
$producerJson = [IO.File]::ReadAllText((Join-Path $producerRoot 'producer.json'))
$producer = $producerJson | ConvertFrom-Json
$consumerDriverOutput = [IO.File]::ReadAllText((Join-Path $consumerRoot 'consumer-driver.log'))
$profiles = foreach ($name in @('full', 'mini')) {
    $packageId = if ($name -ceq 'full') { 'JYPPX.OpenCV.runtime.linux-x64.preview' } else { 'JYPPX.OpenCV.runtime.linux-x64.preview.mini' }
    $package = Join-Path $producerRoot "nupkg-linux-x64-preview-$name/$packageId.5.1.0.nupkg"
    $consumerPackage = Join-Path $consumerRoot "nupkg-linux-x64-preview-$name/$packageId.5.1.0.nupkg"
    $hash = (Get-FileHash $package -Algorithm SHA256).Hash.ToLowerInvariant()
    if ((Get-FileHash $consumerPackage -Algorithm SHA256).Hash.ToLowerInvariant() -cne $hash) { throw 'Consumer package differs from producer package.' }
    $consumerJson = [IO.File]::ReadAllText((Join-Path $consumerRoot "generic-linux-preview-consumer-evidence-ubuntu.22.04-x64-$name.json"))
    $consumerLog = [IO.File]::ReadAllText((Join-Path $consumerRoot "consumer-ubuntu.22.04-x64-$name.log"))
    $provenanceJson = [IO.File]::ReadAllText((Join-Path $producerRoot "runtime-inputs/ubuntu.22.04-x64-$name/runtime-input.provenance.json"))
    $packageJson = [IO.File]::ReadAllText((Join-Path $producerRoot "nupkg-linux-x64-preview-$name/generic-linux-preview-package-evidence-$name.json"))
    [ordered]@{
        name = $name; packageId = $packageId; packageVersion = '5.1.0'; packageBytes = (Get-Item $package).Length; packageSha256 = $hash
        artifactPath = [IO.Path]::GetRelativePath($repo, $package).Replace('\', '/')
        entries = @(Get-LocalLinux51PackageManifest $package)
        producerProvenanceJson = $provenanceJson; producerProvenanceSha256 = Get-LocalLinux51TextHash $provenanceJson
        packageEvidenceJson = $packageJson; packageEvidenceSha256 = Get-LocalLinux51TextHash $packageJson
        consumerEvidenceJson = $consumerJson; consumerEvidenceSha256 = Get-LocalLinux51TextHash $consumerJson
        consumerOutput = $consumerLog; consumerOutputSha256 = Get-LocalLinux51TextHash $consumerLog
    }
}
$record = [ordered]@{
    schemaVersion = 1; evidenceKind = 'local-linux-5.1-preview-package-consumer'; status = 'verified-local-ubuntu22-x64-preview'
    openCvVersion = '5.1.0'; packageVersion = '5.1.0'; sourceBaseCommit = $producer.sourceCommit; worktreeChangesIncluded = $true
    sourceInputSet = Get-LocalLinux51SourceSet $repo
    execution = 'local-docker'; hosted = $false; publicationAllowed = $false; genericStablePromoted = $false
    producerIdentity = Get-LocalLinux51DockerIdentity $ProducerContainer
    consumerIdentity = Get-LocalLinux51DockerIdentity $ConsumerContainer
    producerJson = $producerJson; producerSha256 = Get-LocalLinux51TextHash $producerJson
    consumerDriverOutput = $consumerDriverOutput; consumerDriverOutputSha256 = Get-LocalLinux51TextHash $consumerDriverOutput
    managedPackage = [ordered]@{
        artifactPath = 'artifacts/opencv-5.1.0-local-candidate/39c73be01bb1a568d9db4051f75ca1516bdfae51/nupkg-managed/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg'
        sha256 = (Get-FileHash (Join-Path $consumerRoot 'nupkg-managed/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg') -Algorithm SHA256).Hash.ToLowerInvariant()
        bytes = (Get-Item (Join-Path $consumerRoot 'nupkg-managed/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg')).Length
    }
    profiles = @($profiles)
    limits = [ordered]@{ consumerDistros = @('ubuntu.22.04-x64'); threeDistroGateComplete = $false; arm64Validated = $false; hostedValidated = $false; finalReleaseApproved = $false }
}
$output = if ([IO.Path]::IsPathRooted($OutputPath)) { $OutputPath } else { Join-Path $repo $OutputPath }
[IO.File]::WriteAllText($output, (($record | ConvertTo-Json -Depth 22) -replace "`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
& (Join-Path $PSScriptRoot 'Test-LocalLinux51Evidence.ps1') -RepositoryRoot $repo -EvidencePath $output -RequirePackageArtifacts
Write-Host "LOCAL_LINUX_51_EVIDENCE_MEASURED output=$output"
