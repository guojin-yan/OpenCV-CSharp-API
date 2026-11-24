[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArtifactDirectory,
    [Parameter(Mandatory)][string]$KitRoot,
    [string]$ProducerContainer = 'opencv51-fedora44-producer-b7f8180',
    [string]$ConsumerContainer = 'opencv51-fedora44-candidate-consumer-b7f8180',
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'packaging/runtime/opencv-5.1.0-local-fedora44-evidence.json'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalFedora51.Common.ps1')
$repo = (Resolve-Path $RepositoryRoot).Path; $artifact = (Resolve-Path $ArtifactDirectory).Path
$producerDirectory = Join-Path $artifact 'producer'; $consumerDirectory = Join-Path $artifact 'consumer'
$producer = Get-Content (Join-Path $producerDirectory 'producer.json') -Raw | ConvertFrom-Json
$consumer = Get-LocalLinux51ModuleConsumerRecord $repo $consumerDirectory $ConsumerContainer 'fedora.44-x64'
$consumer.identity = Get-LocalFedora51Identity $ConsumerContainer
foreach ($run in $consumer.runs) {
    $run.blobs += Get-LocalLinux51ModuleBlob (Join-Path $consumerDirectory "$($run.targetFramework)-$($run.profile)") 'candidate-rid-graph.json'
}
$packages = @(foreach ($profile in @('full','mini')) {
    $suffix = if ($profile -ceq 'mini') { '.mini' } else { '' }
    $file = Join-Path $artifact "packages/JYPPX.OpenCV.runtime.fedora.44-x64$suffix.5.1.0-fedora44.b7f8180.nupkg"
    [ordered]@{ profile = $profile; packageId = "JYPPX.OpenCV.runtime.fedora.44-x64$suffix";
        version = '5.1.0-fedora44.b7f8180'; file = Get-LocalFedora51File $repo $file;
        entries = @(Get-LocalLinux51PackageManifest $file) }
})
$record = [ordered]@{
    schemaVersion = 1; evidenceKind = 'local-fedora44-5.1-source-package-consumer'; status = 'verified-local-fedora44-x64-candidate'
    openCvVersion = '5.1.0'; sourceBaseCommit = 'b7f8180c3949aa341bbdf2be97432fb08d1e85e2'
    execution = 'local-docker'; candidateOnly = $true; publicationAllowed = $false; hostedValidated = $false; activeIdentityPromoted = $false
    artifactDirectory = [IO.Path]::GetRelativePath($repo,$artifact).Replace('\','/')
    sourceInputs = @(Get-LocalFedora51SourceInputs $repo)
    kitManifest = @(Get-ChildItem $KitRoot -Recurse -File | Sort-Object FullName -CaseSensitive | ForEach-Object {
        [ordered]@{ path = [IO.Path]::GetRelativePath($KitRoot,$_.FullName).Replace('\','/'); bytes = $_.Length;
            sha256 = (Get-FileHash $_.FullName).Hash.ToLowerInvariant() } })
    managedSbom = Get-LocalFedora51File $repo (Join-Path $repo 'packaging/release/opencv-5.1.0-local-sbom/JYPPX.OpenCV.CSharp.API.5.1.0.spdx.json')
    producer = [ordered]@{
        identity = Get-LocalFedora51Identity $ProducerContainer; measurement = $producer
        blobs = @(foreach ($name in @('producer.json','installed-packages.txt','bootstrap.log','driver.log')) { Get-LocalLinux51ModuleBlob $producerDirectory $name })
        profiles = @(foreach ($profile in @('full','mini')) {
            $directory = Join-Path $producerDirectory $profile
            [ordered]@{ profile = $profile; blobs = @(foreach ($name in @('build-plan.json','opencv-CMakeCache.txt','wrapper-CMakeCache.txt','ctest.log','elf.json','compile_commands.json')) {
                Get-LocalLinux51ModuleBlob $directory $name }) }
        })
    }
    packages = $packages; consumer = $consumer
    artifactManifest = @(Get-ChildItem $artifact -Recurse -File | Where-Object { $_.FullName -notmatch '[\\/]package-project[\\/]' } |
        Sort-Object FullName -CaseSensitive | ForEach-Object { Get-LocalFedora51File $repo $_.FullName })
    coverage = [ordered]@{
        independentUpstreamBuilds = 2; nativeCtestPassed = 6; packageCount = 2; managedExecutions = 4
        fullOnnxRuns = 2; miniDnnOmissionRuns = 2; freshPackageCaches = 4
        genericLinuxBaselineClaimed = $false; completeThirdPartyLicenseReview = $false; vulnerabilityReviewComplete = $false
        cameraOrNetworkValidated = $false; arbitraryModelsValidated = $false; arm64Validated = $false; releaseApproved = $false
    }
}
$output = if ([IO.Path]::IsPathRooted($OutputPath)) { $OutputPath } else { Join-Path $repo $OutputPath }
[IO.File]::WriteAllText($output, ($record | ConvertTo-Json -Depth 40) + "`n", [Text.UTF8Encoding]::new($false))
& (Join-Path $PSScriptRoot 'Test-LocalFedora51Evidence.ps1') -RepositoryRoot $repo -EvidencePath $output -RequireArtifacts
Write-Host 'FEDORA_51_EVIDENCE_MEASURED profiles=2 native_ctest=6 consumers=4'
