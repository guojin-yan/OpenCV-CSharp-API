param(
    [Parameter(Mandatory = $true)]
    [string]$ArtifactRoot,
    [Parameter(Mandatory = $true)]
    [ValidateSet('fedora.44-x64', 'alpine.3.23-x64')]
    [string]$Rid,
    [Parameter(Mandatory = $true)]
    [ValidateSet('full', 'mini')]
    [string]$RuntimeProfile,
    [Parameter(Mandatory = $true)]
    [string]$OpenCvVersion,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [string]$CandidateMatrix = 'packaging/runtime/runtime-lifecycle-refresh-matrix.json'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$artifact = (Resolve-Path -LiteralPath $ArtifactRoot).Path
$matrixPath = if ([IO.Path]::IsPathRooted($CandidateMatrix)) { $CandidateMatrix } else { Join-Path $repo $CandidateMatrix }
$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
if ([string]$matrix.status -cne 'candidate-only' -or [bool]$matrix.publicationAllowed) {
    throw 'Lifecycle-refresh runtime input validation requires a non-publishable candidate matrix.'
}

$ridRows = @($matrix.rids | Where-Object { [string]$_.rid -ceq $Rid })
$profileRows = @($matrix.profiles | Where-Object { [string]$_.name -ceq $RuntimeProfile })
if ($ridRows.Count -ne 1 -or $profileRows.Count -ne 1) {
    throw "Candidate matrix does not contain exactly one target/profile: $Rid/$RuntimeProfile"
}
$ridRow = $ridRows[0]
$profileRow = $profileRows[0]
if (@($ridRow.producer.profiles) -notcontains $RuntimeProfile -or [string]$ridRow.producer.kind -cne 'container') {
    throw "Candidate producer does not approve the selected target/profile: $Rid/$RuntimeProfile"
}

$provenancePath = Join-Path $artifact 'runtime-input.provenance.json'
$evidencePath = Join-Path $artifact 'lifecycle-refresh-candidate-evidence.json'
foreach ($requiredFile in @($provenancePath, $evidencePath)) {
    if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
        throw "Lifecycle-refresh artifact is missing required evidence: $requiredFile"
    }
}
$provenance = Get-Content -LiteralPath $provenancePath -Raw | ConvertFrom-Json
$evidence = Get-Content -LiteralPath $evidencePath -Raw | ConvertFrom-Json

$expectedImage = [string]$ridRow.producer.containerImage
$expectedDigest = [string]$ridRow.producer.containerRepoDigest
$expectedSourceCount = if ($RuntimeProfile -ceq 'full') { 49 } else { 9 }
$expectedAbiCount = if ($RuntimeProfile -ceq 'full') { 2663 } else { 527 }
$expectedRuntimeFileCount = if ($RuntimeProfile -ceq 'full') { 51 } else { 18 }
$expectedModuleCount = @($profileRow.modules).Count

if ([string]$provenance.Rid -cne $Rid -or
    [string]$provenance.OpenCvRid -cne $Rid -or
    [string]$provenance.RuntimeProfile -cne $RuntimeProfile -or
    [string]$provenance.OpenCvVersion -cne $OpenCvVersion -or
    [bool]$provenance.SyntheticRuntimeInputs -or
    [string]$provenance.ContainerImage -cne $expectedImage -or
    [string]$provenance.ContainerImageDigest -cne $expectedDigest -or
    [string]$provenance.ContainerDistro -cne [string]$ridRow.distro -or
    [string]$provenance.ContainerDistroVersion -cne [string]$ridRow.distroVersion -or
    [int]$provenance.NativeWrapperSourceCount -ne $expectedSourceCount -or
    [int]$provenance.NativeAbiFunctionCount -ne $expectedAbiCount -or
    @($provenance.RequiredModules).Count -ne $expectedModuleCount -or
    @($provenance.NativeLoaderFiles).Count -ne 1 -or
    @($provenance.RuntimeFiles).Count -ne $expectedRuntimeFileCount) {
    throw "Lifecycle-refresh runtime-input provenance does not match the candidate contract: $Rid/$RuntimeProfile"
}

if ([string]$evidence.Rid -cne $Rid -or
    [string]$evidence.RuntimeProfile -cne $RuntimeProfile -or
    [string]$evidence.OpenCvVersion -cne $OpenCvVersion -or
    [bool]$evidence.PublicationAllowed -or
    [string]$evidence.ContainerImage -cne $expectedImage -or
    [string]$evidence.ContainerImageDigest -cne $expectedDigest -or
    [string]$evidence.RuntimeInputProvenanceSha256 -cne (Get-FileHash -LiteralPath $provenancePath -Algorithm SHA256).Hash.ToLowerInvariant() -or
    [string]$evidence.CandidateMatrixSha256 -cne (Get-FileHash -LiteralPath $matrixPath -Algorithm SHA256).Hash.ToLowerInvariant() -or
    [int]$evidence.HostDotNetSdkMajor -ne 10) {
    throw "Lifecycle-refresh candidate evidence is not bound to the selected immutable input: $Rid/$RuntimeProfile"
}

foreach ($directoryName in @('native-wrapper', 'opencv-runtime', 'opencv-source')) {
    $directory = Join-Path $artifact $directoryName
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        throw "Lifecycle-refresh runtime-input artifact is missing directory: $directoryName"
    }
}
$nativeLoader = @(Get-ChildItem -LiteralPath (Join-Path $artifact 'native-wrapper') -File | Where-Object { $_.Name -eq 'libJYPPX.OpenCV.Native.so' })
$opencvFiles = @(Get-ChildItem -LiteralPath (Join-Path $artifact 'opencv-runtime') -File | Where-Object { $_.Name -like 'libopencv_*.so*' })
if ($nativeLoader.Count -ne 1 -or $opencvFiles.Count -ne $expectedRuntimeFileCount) {
    throw "Lifecycle-refresh runtime-input native payload count is not profile-aligned: loader=$($nativeLoader.Count) opencv=$($opencvFiles.Count) expected=$expectedRuntimeFileCount"
}

Write-Host "LIFECYCLE_REFRESH_RUNTIME_INPUT_ARTIFACT_OK target=$Rid/$RuntimeProfile files=$($opencvFiles.Count + $nativeLoader.Count) sources=$expectedSourceCount abi_functions=$expectedAbiCount publication_allowed=false"
