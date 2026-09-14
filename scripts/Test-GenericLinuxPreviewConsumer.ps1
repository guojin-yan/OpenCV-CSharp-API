param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactRoot,
    [Parameter(Mandatory = $true)]
    [string]$ConsumerRid,
    [Parameter(Mandatory = $true)]
    [ValidateSet('full', 'mini')]
    [string]$RuntimeProfile,
    [Parameter(Mandatory = $true)]
    [string]$ExpectedPackageVersion,
    [Parameter(Mandatory = $true)]
    [string]$OpenCvVersion,
    [string]$PreviewMatrixPath = 'packaging/runtime/runtime-generic-linux-preview-matrix.json',
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [switch]$RunNativeSmoke
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$artifact = (Resolve-Path -LiteralPath $ArtifactRoot).Path
$matrixPath = if ([IO.Path]::IsPathRooted($PreviewMatrixPath)) { $PreviewMatrixPath } else { Join-Path $repo ($PreviewMatrixPath.Replace('\\', '/') -replace '/', [IO.Path]::DirectorySeparatorChar) }
$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
$targetRid = [string]$matrix.targetRid
$profile = @($matrix.profiles | Where-Object { [string]$_.name -ceq $RuntimeProfile })
$consumer = @($matrix.consumers | Where-Object { [string]$_.rid -ceq $ConsumerRid })
if ($profile.Count -ne 1 -or $consumer.Count -ne 1) {
    throw "Generic Linux preview matrix does not contain exactly one selected profile/consumer: $ConsumerRid/$RuntimeProfile"
}
if (@($consumer[0].profiles) -notcontains $RuntimeProfile) {
    throw "Generic Linux preview consumer does not cover selected profile: $ConsumerRid/$RuntimeProfile"
}

$managedDir = Join-Path $artifact 'nupkg-managed'
$previewDir = Join-Path $artifact ([string]$profile[0].artifactName)
$previewPackage = @(Get-ChildItem -LiteralPath $previewDir -Filter '*.nupkg' -File)
$managedPackage = @(Get-ChildItem -LiteralPath $managedDir -Filter '*.nupkg' -File)
if ($previewPackage.Count -ne 1 -or $managedPackage.Count -ne 1) {
    throw "Generic Linux preview consumer requires exactly one managed and one preview runtime package: $ConsumerRid/$RuntimeProfile"
}
if ($previewPackage[0].BaseName -notlike "$([string]$profile[0].packageId).*" -or
    $managedPackage[0].BaseName -notlike 'JYPPX.OpenCV.CSharp.API.*') {
    throw 'Generic Linux preview consumer package identities drifted from the preview matrix.'
}

$consumerArgs = @(
    '-NoProfile',
    '-File', (Join-Path $repo 'scripts/Test-GitHubPackConsumerRestoreSurface.ps1'),
    '-ArtifactRoot', $artifact,
    '-ExpectedPackageVersion', $ExpectedPackageVersion,
    '-ExpectedSyntheticRuntimeInputs', 'false',
    '-SelectedRid', $targetRid,
    '-SelectedRuntimeProfile', $RuntimeProfile,
    '-ConsumerRuntimeRidOverride', $ConsumerRid,
    '-RuntimePackageIdOverride', [string]$profile[0].packageId,
    '-RuntimeAssetRidOverride', $targetRid,
    '-RuntimePackageMatrix', $PreviewMatrixPath,
    '-OpenCvVersion', $OpenCvVersion,
    '-ConsumerTargetFramework', 'net10.0'
)
if ($RunNativeSmoke) {
    $consumerArgs += '-RunNativeSmoke'
}
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw 'pwsh is required for generic Linux preview consumer validation.'
}
& $pwsh.Source @consumerArgs
if ($LASTEXITCODE -ne 0) {
    throw "Generic Linux preview consumer restore/build/native smoke failed: $ConsumerRid/$RuntimeProfile"
}

$evidence = [ordered]@{
    SchemaVersion = 1
    EvidenceKind = 'generic-linux-preview-package-consumer-native-smoke'
    Status = 'preview-only'
    PublicationAllowed = $false
    ConsumerRid = $ConsumerRid
    ConsumerDistro = [string]$consumer[0].distro
    ConsumerDistroVersion = [string]$consumer[0].distroVersion
    RuntimeProfile = $RuntimeProfile
    PackageId = [string]$profile[0].packageId
    PackageVersion = $ExpectedPackageVersion
    PreviewPackageSha256 = (Get-FileHash -LiteralPath $previewPackage[0].FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    ManagedPackageSha256 = (Get-FileHash -LiteralPath $managedPackage[0].FullName -Algorithm SHA256).Hash.ToLowerInvariant()
    NativeSmokeExecuted = [bool]$RunNativeSmoke
    RuntimeAssetRid = $targetRid
    RecordedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
}
$evidencePath = Join-Path $artifact "generic-linux-preview-consumer-evidence-$ConsumerRid-$RuntimeProfile.json"
[IO.File]::WriteAllText($evidencePath, (($evidence | ConvertTo-Json -Depth 8) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))

Write-Host "GENERIC_LINUX_PREVIEW_CONSUMER_OK consumer=$ConsumerRid profile=$RuntimeProfile package=$($profile[0].packageId) runtime_asset_rid=$targetRid native_smoke_executed=$([bool]$RunNativeSmoke) publication_allowed=false"
Write-Host "Generic Linux preview consumer evidence: $evidencePath"
