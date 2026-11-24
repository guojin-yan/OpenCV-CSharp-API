[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArtifactDirectory,
    [Parameter(Mandatory)][string]$ProducerKit,
    [Parameter(Mandatory)][string]$RuntimeKit,
    [Parameter(Mandatory)][string]$ProducerContainer,
    [Parameter(Mandatory)][string[]]$ConsumerContainers,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'packaging/runtime/opencv-5.1.0-local-linux-aot-evidence.json'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'LocalLinux51Aot.Common.ps1')
$repo = (Resolve-Path $RepositoryRoot).Path; $artifacts = (Resolve-Path $ArtifactDirectory).Path
$specs = @(Get-LocalLinux51AotSpecs)
if ($ConsumerContainers.Count -ne 3) { throw 'Expected Ubuntu, Debian, Fedora AOT consumers in that order.' }
$producerOutput = Join-Path $artifacts 'producer'
foreach ($profile in @('full', 'mini')) {
    # NuGet's signature-normalized content hash differs from the signed archive hash.
    $capture = @'
$cache = '/producer/build-__PROFILE__/nuget'
$items = @(Get-ChildItem $cache -Recurse -File -Filter '*.nupkg' | Sort-Object FullName -CaseSensitive | ForEach-Object {
    [ordered]@{
        path = [IO.Path]::GetRelativePath($cache, $_.FullName)
        metadata = Get-Content (Join-Path $_.DirectoryName '.nupkg.metadata') -Raw | ConvertFrom-Json
        archiveSha512Base64 = (Get-Content ($_.FullName + '.sha512') -Raw).Trim()
    }
})
ConvertTo-Json -InputObject $items -Depth 6
'@
    $metadata = & docker exec $ProducerContainer pwsh -NoProfile -Command $capture.Replace('__PROFILE__', $profile)
    if ($LASTEXITCODE -ne 0) { throw 'Cannot capture original AOT restore metadata.' }
    [IO.File]::WriteAllText((Join-Path $producerOutput "$profile/nuget-metadata.json"), ($metadata -join "`n") + "`n", [Text.UTF8Encoding]::new($false))
}
$references = foreach ($path in @('packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json',
    'packaging/runtime/opencv-5.1.0-local-linux-module-consumer-evidence.json',
    'packaging/performance/native-aot-smoke-evidence.json',
    'packaging/release/opencv-5.1.0-local-sbom/JYPPX.OpenCV.CSharp.API.5.1.0.spdx.json')) {
    [ordered]@{ path = $path; sha256 = (Get-FileHash (Join-Path $repo $path) -Algorithm SHA256).Hash.ToLowerInvariant() }
}
$record = [ordered]@{
    schemaVersion = 1; evidenceKind = 'local-linux-5.1-nativeaot-package-consumers'; status = 'verified-local-three-distro-x64-preview'
    openCvVersion = '5.1.0'; packageVersion = '5.1.0'; sourceBaseCommit = (& git -C $repo rev-parse HEAD); worktreeChangesIncluded = $true
    execution = 'local-docker'; hosted = $false; publicationAllowed = $false; genericStablePromoted = $false
    references = @($references); sourceInputs = @(Get-LocalLinux51AotSourceInputs $repo)
    producerKitManifest = @(Get-LocalLinux51AotManifest (Resolve-Path $ProducerKit).Path)
    runtimeKitManifest = @(Get-LocalLinux51AotManifest (Resolve-Path $RuntimeKit).Path)
    producer = [ordered]@{
        identity = Get-LocalLinux51AotIdentity $ProducerContainer
        outputDirectory = [IO.Path]::GetRelativePath($repo, $producerOutput).Replace('\', '/')
        blobs = @(foreach ($name in @('bootstrap.log', 'driver.log', 'producer.json')) { Get-LocalLinux51AotBlob $producerOutput $name })
        profiles = @(foreach ($profile in @('full', 'mini')) {
            [ordered]@{ name = $profile; blobs = @(foreach ($name in @('Consumer.csproj', 'NuGet.Config', 'project.assets.json',
                'restore.stdout.log', 'restore.stderr.log', 'publish.stdout.log', 'publish.stderr.log', 'nuget-metadata.json',
                'elf-header.stdout.log', 'elf-header.stderr.log', 'elf-dynamic.stdout.log', 'elf-dynamic.stderr.log',
                'elf-versions.stdout.log', 'elf-versions.stderr.log', 'ldd.stdout.log', 'ldd.stderr.log')) {
                    Get-LocalLinux51AotBlob (Join-Path $producerOutput $profile) $name
                }) }
        })
    }
    consumers = @(for ($index = 0; $index -lt 3; $index++) {
        $directory = Join-Path $artifacts $specs[$index].rid
        [ordered]@{
            rid = $specs[$index].rid; identity = Get-LocalLinux51AotIdentity $ConsumerContainers[$index]
            outputDirectory = [IO.Path]::GetRelativePath($repo, $directory).Replace('\', '/')
            blobs = @(foreach ($name in @('bootstrap.log', 'driver.log', 'environment.log', 'scripts.sha256')) { Get-LocalLinux51AotBlob $directory $name })
            runs = @(foreach ($profile in @('full', 'mini')) {
                $avi = Join-Path $directory "$profile/three-frames.avi"
                [ordered]@{
                    profile = $profile
                    blobs = @(foreach ($name in @('execute.stdout.log', 'execute.stderr.log', 'exit-code.log', 'publish.sha256', 'video.sha256')) {
                        Get-LocalLinux51AotBlob (Join-Path $directory $profile) $name
                    })
                    video = [ordered]@{ bytes = (Get-Item $avi).Length; sha256 = (Get-FileHash $avi -Algorithm SHA256).Hash.ToLowerInvariant() }
                }
            })
        }
    })
    coverage = [ordered]@{
        consumerRids = @($specs.rid); targetFramework = 'net10.0'; runtimeIdentifier = 'linux-x64'; runtimeFrameworkVersion = '10.0.15'
        profiles = @('full', 'mini'); nativeAotPublishes = 2; nativeAotExecutions = 6; fullOnnxInferenceExecutions = 6
        miniImporterOmissionExecutions = 3; mjpgAviWriteReadExecutions = 6; externalDotNetRequired = $false
        arm64Validated = $false; hostedValidated = $false; packageWideAotCompatibilityClaimed = $false
        cameraOrNetworkValidated = $false; arbitraryModelsValidated = $false; finalReleaseApproved = $false
    }
}
$output = if ([IO.Path]::IsPathRooted($OutputPath)) { $OutputPath } else { Join-Path $repo $OutputPath }
[IO.File]::WriteAllText($output, (($record | ConvertTo-Json -Depth 26) -replace "`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
& (Join-Path $PSScriptRoot 'Test-LocalLinux51AotEvidence.ps1') -RepositoryRoot $repo -EvidencePath $output -RequireArtifacts
Write-Host "LOCAL_LINUX_51_AOT_EVIDENCE_MEASURED output=$output"
