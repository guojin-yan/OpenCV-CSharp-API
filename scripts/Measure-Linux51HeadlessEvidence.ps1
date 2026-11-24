[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ArtifactDirectory,
    [Parameter(Mandatory)][string]$ConsumerKit,
    [Parameter(Mandatory)][string]$ProducerContainer,
    [Parameter(Mandatory)][string[]]$ConsumerContainers,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$OutputPath = 'packaging/runtime/opencv-5.1.0-local-linux-headless-evidence.json'
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
. (Join-Path $PSScriptRoot 'Linux51Headless.Common.ps1')
$repo = (Resolve-Path $RepositoryRoot).Path
$artifacts = (Resolve-Path $ArtifactDirectory).Path
$kit = (Resolve-Path $ConsumerKit).Path
$specs = @(Get-Headless51Specs)
if ($ConsumerContainers.Count -ne 3) { throw 'Expected Ubuntu 22.04, Ubuntu 24.04, Debian 12 consumers in that order.' }
function Write-HeadlessJson([string]$Path, [object]$Value) {
    [IO.File]::WriteAllText($Path, (($Value | ConvertTo-Json -Depth 36) -replace "`r`n", "`n") + "`n", [Text.UTF8Encoding]::new($false))
}
function Get-PackageEntries([string]$Path) {
    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        foreach ($entry in $archive.Entries | Where-Object { -not $_.FullName.EndsWith('/') } | Sort-Object FullName -CaseSensitive) {
            $stream = $entry.Open(); $memory = [IO.MemoryStream]::new()
            try { $stream.CopyTo($memory); $bytes = $memory.ToArray() } finally { $stream.Dispose(); $memory.Dispose() }
            [ordered]@{ path = $entry.FullName; bytes = $bytes.Length; sha256 = Get-LocalLinux51Hash $bytes
                sha1 = [Convert]::ToHexString([Security.Cryptography.SHA1]::HashData($bytes)).ToLowerInvariant() }
        }
    } finally { $archive.Dispose() }
}
$producerRoot = Join-Path $artifacts 'producer'
$build = Get-Content (Join-Path $producerRoot 'producer.json') -Raw | ConvertFrom-Json
$packages = foreach ($profile in $build.profiles) {
    $package = Join-Path $producerRoot $profile.package.path
    $entries = @(Get-PackageEntries $package)
    $relativeSbom = "packaging/release/opencv-5.1.0-local-headless-sbom/$($profile.package.id).$($profile.package.version).spdx.json"
    $sbomPath = Join-Path $repo $relativeSbom
    [IO.Directory]::CreateDirectory((Split-Path $sbomPath)) | Out-Null
    $verificationText = (($entries.sha1 | Sort-Object -CaseSensitive) -join '')
    $verificationCode = [Convert]::ToHexString([Security.Cryptography.SHA1]::HashData([Text.Encoding]::UTF8.GetBytes($verificationText))).ToLowerInvariant()
    $files = @(for ($i = 0; $i -lt $entries.Count; $i++) {
        [ordered]@{ SPDXID = "SPDXRef-File-$i"; fileName = './' + $entries[$i].path
            checksums = @([ordered]@{ algorithm = 'SHA1'; checksumValue = $entries[$i].sha1 }, [ordered]@{ algorithm = 'SHA256'; checksumValue = $entries[$i].sha256 })
            licenseConcluded = 'NOASSERTION'; licenseInfoInFiles = @('NOASSERTION'); copyrightText = 'NOASSERTION' }
    })
    $sbom = [ordered]@{
        spdxVersion = 'SPDX-2.3'; dataLicense = 'CC0-1.0'; SPDXID = 'SPDXRef-DOCUMENT'
        name = "$($profile.package.id) $($profile.package.version) local candidate inventory"
        documentNamespace = "https://github.com/guojin-yan/OpenCV-CSharp-API/spdx/headless/$($profile.package.sha256)"
        creationInfo = [ordered]@{ created = '2026-11-24T00:00:00Z'; creators = @('Tool: Measure-Linux51HeadlessEvidence.ps1') }
        comment = 'Candidate-only local package inventory. Reuses the measured upstream build; transitive static third-party inventory and final release approval remain open.'
        packages = @([ordered]@{ SPDXID = 'SPDXRef-Package'; name = $profile.package.id; versionInfo = $profile.package.version
            downloadLocation = 'NOASSERTION'; filesAnalyzed = $true; packageFileName = [IO.Path]::GetFileName($package)
            checksums = @([ordered]@{ algorithm = 'SHA256'; checksumValue = $profile.package.sha256 })
            packageVerificationCode = [ordered]@{ packageVerificationCodeValue = $verificationCode }
            licenseConcluded = 'NOASSERTION'; licenseDeclared = 'Apache-2.0'; licenseInfoFromFiles = @('NOASSERTION'); copyrightText = 'NOASSERTION' })
        files = $files
        relationships = @([ordered]@{ spdxElementId = 'SPDXRef-DOCUMENT'; relationshipType = 'DESCRIBES'; relatedSpdxElement = 'SPDXRef-Package' }) +
            @($files | ForEach-Object { [ordered]@{ spdxElementId = 'SPDXRef-Package'; relationshipType = 'CONTAINS'; relatedSpdxElement = $_.SPDXID } })
    }
    Write-HeadlessJson $sbomPath $sbom
    [ordered]@{ profile = $profile.profile; id = $profile.package.id; version = $profile.package.version
        artifactPath = [IO.Path]::GetRelativePath($repo, $package).Replace('\', '/'); bytes = (Get-Item $package).Length
        sha256 = (Get-FileHash $package).Hash.ToLowerInvariant(); entries = $entries
        sbom = [ordered]@{ path = $relativeSbom; sha256 = (Get-FileHash $sbomPath).Hash.ToLowerInvariant() } }
}
$managedPath = Join-Path $kit 'packages/JYPPX.OpenCV.CSharp.API.5.1.0.nupkg'
$references = @(foreach ($relative in @('packaging/runtime/runtime-headless-profile-contract.json',
    'packaging/runtime/runtime-headless-candidate-evidence.json', 'packaging/runtime/opencv-5.1.0-local-linux-preview-evidence.json')) {
    [ordered]@{ path = $relative; sha256 = (Get-FileHash (Join-Path $repo $relative)).Hash.ToLowerInvariant() }
})
$record = [ordered]@{
    schemaVersion = 1; evidenceKind = 'local-linux-5.1-headless-candidate'; status = 'verified-local-three-distro-x64-candidate'
    openCvVersion = '5.1.0'; packageVersion = $build.profiles[0].package.version
    sourceBaseCommit = $build.sourceBaseCommit; worktreeChangesIncluded = $true
    execution = 'local-docker'; hosted = $false; candidateOnly = $true; publicationAllowed = $false; activePackageIdentityAdded = $false
    references = $references; sourceInputs = @(Get-Headless51Inputs $repo); kitManifest = @(Get-Headless51Manifest $kit)
    producer = [ordered]@{
        identity = Get-Headless51Identity $ProducerContainer
        outputDirectory = [IO.Path]::GetRelativePath($repo, $producerRoot).Replace('\', '/')
        blobs = @(foreach ($name in @('producer.json', 'candidate-modules.json', 'abi-generate.log',
            'generated-abi/native_abi_manifest.txt', 'generated-abi/native_abi_mini_manifest.txt',
            'overlay/CMakeLists.txt', 'overlay/src/highgui/highgui.cpp', 'overlay/tests/native_smoke.cpp',
            'full-configure.log', 'full-build.log', 'full-ctest.log', 'full-elf.json', 'full-pack.log', 'full-compile-commands.json', 'full-CMakeCache.txt',
            'mini-configure.log', 'mini-build.log', 'mini-ctest.log', 'mini-elf.json', 'mini-pack.log', 'mini-compile-commands.json', 'mini-CMakeCache.txt')) {
            Get-Headless51Blob $producerRoot $name
        })
    }
    managedPackage = [ordered]@{ id = 'JYPPX.OpenCV.CSharp.API'; version = '5.1.0'; bytes = (Get-Item $managedPath).Length
        sha256 = (Get-FileHash $managedPath).Hash.ToLowerInvariant(); assemblies = @((Get-PackageEntries $managedPath) | Where-Object path -Match '^lib/net(8|10)\.0/JYPPX.OpenCV.CSharp.API.dll$') }
    packages = @($packages)
    consumers = @(for ($index = 0; $index -lt 3; $index++) {
        $directory = Join-Path $artifacts $specs[$index].rid
        [ordered]@{
            rid = $specs[$index].rid; identity = Get-Headless51Identity $ConsumerContainers[$index]
            outputDirectory = [IO.Path]::GetRelativePath($repo, $directory).Replace('\', '/')
            blobs = @(foreach ($name in @('bootstrap.log', 'driver.log', 'environment.json', 'runs.json')) { Get-Headless51Blob $directory $name })
            runs = @(foreach ($tfm in @('net8.0', 'net10.0')) { foreach ($profile in @('full', 'mini')) {
                $runDir = Join-Path $directory "$tfm-$profile"
                [ordered]@{ targetFramework = $tfm; profile = $profile
                    blobs = @(foreach ($name in @('result.json', 'execute.stdout.log', 'execute.stderr.log', 'build.stdout.log', 'build.stderr.log',
                        'restore.stdout.log', 'restore.stderr.log', 'project.assets.json', 'Consumer.csproj', 'NuGet.Config', 'cache-packages.json')) { Get-Headless51Blob $runDir $name })
                    video = [ordered]@{ bytes = (Get-Item (Join-Path $runDir 'three-frames.avi')).Length; sha256 = (Get-FileHash (Join-Path $runDir 'three-frames.avi')).Hash.ToLowerInvariant() }
                }
            } })
        }
    })
    coverage = [ordered]@{ targetRids = @($specs.rid); profiles = @('full', 'mini'); targetFrameworks = @('net8.0', 'net10.0')
        nativeCtestPassed = 6; managedExecutions = 12; guiNegativeCalls = 72; fullOnnxInferences = 12; mjpgWriteReadExecutions = 12
        upstreamBuildReused = $true; freshMinimalUpstreamBuild = $false; hostedValidated = $false; arm64Validated = $false
        cameraOrNetworkValidated = $false; transitiveStaticSbomComplete = $false; finalReleaseApproved = $false }
}
Write-HeadlessJson (Join-Path $repo $OutputPath) $record
& (Join-Path $PSScriptRoot 'Test-Linux51HeadlessEvidence.ps1') -RepositoryRoot $repo -EvidencePath $OutputPath -RequireArtifacts
Write-Host "HEADLESS_51_EVIDENCE_MEASURED output=$OutputPath"
