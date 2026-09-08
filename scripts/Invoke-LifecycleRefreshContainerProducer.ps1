param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('fedora.44-x64', 'alpine.3.23-x64')]
    [string]$Rid,
    [Parameter(Mandatory = $true)]
    [ValidateSet('full', 'mini')]
    [string]$RuntimeProfile,
    [Parameter(Mandatory = $true)]
    [string]$OpenCvVersion,
    [Parameter(Mandatory = $true)]
    [string]$ContainerImage,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$CandidateMatrix = 'packaging/runtime/runtime-lifecycle-refresh-matrix.json',
    [string]$OutputRoot = 'artifacts/lifecycle-refresh-runtime-inputs'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
if ($OpenCvVersion -notmatch '^\d+\.\d+\.\d+(?:[-+][0-9A-Za-z.-]+)?$') {
    throw "OpenCvVersion is not safe for lifecycle-refresh production: $OpenCvVersion"
}
if ($CandidateMatrix -cne 'packaging/runtime/runtime-lifecycle-refresh-matrix.json') {
    throw "Lifecycle-refresh production must use the repository-owned candidate matrix."
}
if ($OutputRoot -cne 'artifacts/lifecycle-refresh-runtime-inputs') {
    throw "Lifecycle-refresh output must remain in the isolated candidate artifact root."
}

function Read-RepositoryJson {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $repo ($RelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required lifecycle-refresh input was not found: $RelativePath"
    }
    return [IO.File]::ReadAllText($path) | ConvertFrom-Json
}

function Invoke-CheckedCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(ValueFromRemainingArguments = $true)][string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath failed with exit code $LASTEXITCODE."
    }
}

$candidate = Read-RepositoryJson -RelativePath $CandidateMatrix
$activeMatrix = Read-RepositoryJson -RelativePath 'packaging/runtime/runtime-package-matrix.json'
$supportContract = Read-RepositoryJson -RelativePath 'packaging/runtime/runtime-support-contract.json'
if ([string]$candidate.status -cne 'candidate-only' -or [bool]$candidate.publicationAllowed) {
    throw 'Lifecycle-refresh matrix is not an isolated non-publishable candidate overlay.'
}
if (@($activeMatrix.rids | Where-Object { [string]$_.rid -ceq $Rid }).Count -ne 0) {
    throw "Lifecycle-refresh producer refuses an RID that is already in the active package matrix: $Rid"
}

$ridRows = @($candidate.rids | Where-Object { [string]$_.rid -ceq $Rid })
if ($ridRows.Count -ne 1) {
    throw "Lifecycle-refresh RID is missing or ambiguous: $Rid"
}
$ridRow = $ridRows[0]
$profileRows = @($candidate.profiles | Where-Object { [string]$_.name -ceq $RuntimeProfile })
if ($profileRows.Count -ne 1 -or @($ridRow.producer.profiles) -notcontains $RuntimeProfile) {
    throw "Lifecycle-refresh profile is not approved for ${Rid}: $RuntimeProfile"
}
if ([string]$ridRow.producer.kind -cne 'container') {
    throw "Lifecycle-refresh RID is not classified as a container producer: $Rid"
}
$expectedImage = [string]$ridRow.producer.containerImage
if ($ContainerImage -cne $expectedImage -or $expectedImage -notmatch '@sha256:[0-9a-f]{64}$') {
    throw "Lifecycle-refresh image does not match the immutable candidate matrix value for $Rid."
}
$supportRows = @($supportContract.lifecycleRefreshCandidates | Where-Object { [string]$_.rid -ceq $Rid })
if ($supportRows.Count -ne 1 -or
    [string]$supportRows[0].status -cne 'lifecycle-refresh-pending' -or
    [string]$supportRows[0].containerImage -cne $expectedImage) {
    throw "Lifecycle-refresh support-contract candidate does not match the selected matrix row: $Rid"
}

$dotnetCommand = Get-Command dotnet -CommandType Application -ErrorAction Stop
$dotnetVersion = (& $dotnetCommand.Source --version | Select-Object -Last 1).Trim()
$parsedDotNetVersion = [version]$null
if ($LASTEXITCODE -ne 0 -or -not [version]::TryParse($dotnetVersion, [ref]$parsedDotNetVersion)) {
    throw 'Unable to read the host .NET SDK version.'
}
if ($parsedDotNetVersion.Major -ne 10) {
    throw "Lifecycle-refresh orchestration requires any .NET 10 SDK, not a fixed feature band: $dotnetVersion"
}

$dockerCommand = Get-Command docker -CommandType Application -ErrorAction Stop
$dockerArchitecture = (& $dockerCommand.Source info --format '{{.Architecture}}' | Select-Object -Last 1).Trim()
if ($LASTEXITCODE -ne 0 -or $dockerArchitecture -notin @('x86_64', 'amd64')) {
    throw "Lifecycle-refresh containers require an x64 Docker host: $dockerArchitecture"
}

Invoke-CheckedCommand $dockerCommand.Source pull $expectedImage
$imageJson = (& $dockerCommand.Source image inspect --format '{{json .}}' $expectedImage | Select-Object -Last 1)
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($imageJson)) {
    throw "Unable to inspect lifecycle-refresh image: $expectedImage"
}
$imageInspect = $imageJson | ConvertFrom-Json
$imageId = [string]$imageInspect.Id
$imageDigest = ($expectedImage -split '@', 2)[1]
$repository = (($expectedImage -split '@', 2)[0] -replace ':[^/:]+$', '')
$expectedRepoDigest = "$repository@$imageDigest"
if ($imageId -notmatch '^sha256:[0-9a-f]{64}$' -or @($imageInspect.RepoDigests) -cnotcontains $expectedRepoDigest) {
    throw "Pulled image provenance does not contain the exact candidate RepoDigest: $expectedRepoDigest"
}

$workspaceRoot = (Resolve-Path -LiteralPath (Split-Path -Parent $repo)).Path
$repositoryDirectoryName = Split-Path -Leaf $repo
$extraCMakeArgs = [string]$ridRow.producer.openCvExtraCMakeArgs.$RuntimeProfile
$dockerArguments = @(
    'run', '--rm',
    '-e', "LIFECYCLE_REFRESH_RID=$Rid",
    '-e', "LIFECYCLE_REFRESH_PROFILE=$RuntimeProfile",
    '-e', "LIFECYCLE_REFRESH_OPENCV_VERSION=$OpenCvVersion",
    '-e', "LIFECYCLE_REFRESH_CONTAINER_IMAGE=$expectedImage",
    '-e', "LIFECYCLE_REFRESH_CONTAINER_IMAGE_ID=$imageId",
    '-e', "LIFECYCLE_REFRESH_CONTAINER_IMAGE_DIGEST=$expectedRepoDigest",
    '-e', "LIFECYCLE_REFRESH_BUILD_LIST=$([string]$profileRows[0].buildList)",
    '-e', "LIFECYCLE_REFRESH_EXTRA_CMAKE_ARGS=$extraCMakeArgs",
    '-e', "LIFECYCLE_REFRESH_HOST_DOTNET_VERSION=$dotnetVersion",
    '-e', "LIFECYCLE_REFRESH_HOSTED_RUNNER=$($env:RUNNER_NAME)",
    '-e', "LIFECYCLE_REFRESH_RUNNER_IMAGE=$($env:ImageOS)",
    '-e', "LIFECYCLE_REFRESH_RUNNER_IMAGE_VERSION=$($env:ImageVersion)",
    '-v', "${workspaceRoot}:/workspace",
    '-w', "/workspace/$repositoryDirectoryName",
    $expectedImage,
    '/bin/sh', './scripts/Invoke-LifecycleRefreshContainerProducer.sh'
)
Invoke-CheckedCommand $dockerCommand.Source @dockerArguments

$artifactRoot = Join-Path $repo "$OutputRoot/$Rid-$RuntimeProfile"
$provenancePath = Join-Path $artifactRoot 'runtime-input.provenance.json'
if (-not (Test-Path -LiteralPath $provenancePath -PathType Leaf)) {
    throw "Lifecycle-refresh producer did not create runtime-input provenance: $provenancePath"
}
$provenance = [IO.File]::ReadAllText($provenancePath) | ConvertFrom-Json
if ([string]$provenance.Rid -cne $Rid -or
    [string]$provenance.RuntimeProfile -cne $RuntimeProfile -or
    [string]$provenance.OpenCvVersion -cne $OpenCvVersion -or
    [bool]$provenance.SyntheticRuntimeInputs -or
    [string]$provenance.ContainerImage -cne $expectedImage -or
    [string]$provenance.ContainerImageId -cne $imageId -or
    [string]$provenance.ContainerImageDigest -cne $expectedRepoDigest -or
    @($provenance.NativeLoaderFiles).Count -ne 1 -or
    @($provenance.RuntimeFiles).Count -eq 0) {
    throw "Lifecycle-refresh runtime-input provenance failed post-container validation: $Rid/$RuntimeProfile"
}

$matrixPath = Join-Path $repo ($CandidateMatrix -replace '/', [IO.Path]::DirectorySeparatorChar)
$evidence = [ordered]@{
    SchemaVersion = 1
    Status = 'candidate-only'
    PublicationAllowed = $false
    Target = "$Rid/$RuntimeProfile"
    CandidateMatrix = $CandidateMatrix
    CandidateMatrixSha256 = (Get-FileHash -LiteralPath $matrixPath -Algorithm SHA256).Hash.ToLowerInvariant()
    RuntimeInputProvenance = 'runtime-input.provenance.json'
    RuntimeInputProvenanceSha256 = (Get-FileHash -LiteralPath $provenancePath -Algorithm SHA256).Hash.ToLowerInvariant()
    ContainerImage = $expectedImage
    ContainerImageId = $imageId
    ContainerImageDigest = $expectedRepoDigest
    OpenCvVersion = $OpenCvVersion
    HostDotNetSdkMajor = $parsedDotNetVersion.Major
    HostDotNetSdkVersion = $dotnetVersion
    ProducerCompletedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
}
$evidencePath = Join-Path $artifactRoot 'lifecycle-refresh-candidate-evidence.json'
$utf8NoBom = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText($evidencePath, (($evidence | ConvertTo-Json -Depth 5) + [Environment]::NewLine), $utf8NoBom)

Write-Host "LIFECYCLE_REFRESH_CANDIDATE_PRODUCER_OK target=$Rid/$RuntimeProfile publication_allowed=false image=$expectedRepoDigest dotnet_major=$($parsedDotNetVersion.Major)"
Write-Host "Lifecycle-refresh candidate evidence: $evidencePath"
