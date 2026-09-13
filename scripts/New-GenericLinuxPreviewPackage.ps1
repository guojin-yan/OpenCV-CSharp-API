param(
    [Parameter(Mandatory = $true)]
    [string]$RuntimeInputRoot,
    [Parameter(Mandatory = $true)]
    [ValidateSet('full', 'mini')]
    [string]$RuntimeProfile,
    [Parameter(Mandatory = $true)]
    [string]$OpenCvVersion,
    [Parameter(Mandatory = $true)]
    [int]$PackageRevision,
    [Parameter(Mandatory = $true)]
    [string]$PackageVersion,
    [string]$OutputDir = 'artifacts/generic-linux-preview',
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$packageVersionScript = Join-Path $repo 'scripts/PackageVersion.ps1'
. $packageVersionScript
$matrixRelativePath = 'packaging/runtime/runtime-generic-linux-preview-matrix.json'
$matrixPath = Join-Path $repo ($matrixRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
$inputRoot = (Resolve-Path -LiteralPath $RuntimeInputRoot).Path
$outputRootCandidate = if ([IO.Path]::IsPathRooted($OutputDir)) { $OutputDir } else { Join-Path $repo $OutputDir }
$outputRoot = [IO.Path]::GetFullPath($outputRootCandidate)
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if ($null -eq $dotnet) {
    throw 'dotnet is required to create the generic Linux preview package.'
}

if (-not (Test-Path -LiteralPath $matrixPath -PathType Leaf)) {
    throw "Generic Linux preview matrix was not found: $matrixPath"
}
$matrix = Get-Content -LiteralPath $matrixPath -Raw | ConvertFrom-Json
if ([string]$matrix.status -cne 'preview-only' -or
    [bool]$matrix.activePackageIdentityAllowed -or
    -not [bool]$matrix.previewArtifactIdentityAllowed -or
    [bool]$matrix.publicationAllowed -or
    [string]$matrix.targetRid -cne 'linux-x64') {
    throw 'Generic Linux preview package creation requires an isolated, non-publishable preview matrix.'
}

$profile = @($matrix.profiles | Where-Object { [string]$_.name -ceq $RuntimeProfile })
if ($profile.Count -ne 1) {
    throw "Generic Linux preview profile is missing or ambiguous: $RuntimeProfile"
}
$previewPackageId = [string]$profile[0].packageId
$artifactName = [string]$profile[0].artifactName
$expectedSourceRid = [string]$matrix.producer.sourceRid

$provenancePath = Join-Path $inputRoot 'runtime-input.provenance.json'
if (-not (Test-Path -LiteralPath $provenancePath -PathType Leaf)) {
    throw "Baseline runtime-input provenance was not found: $provenancePath"
}
$provenance = Get-Content -LiteralPath $provenancePath -Raw | ConvertFrom-Json
if ([string]$provenance.Rid -cne $expectedSourceRid -or
    [string]$provenance.OpenCvRid -cne $expectedSourceRid -or
    [string]$provenance.RuntimeProfile -cne $RuntimeProfile -or
    [string]$provenance.OpenCvVersion -cne $OpenCvVersion -or
    -not [bool]$provenance.GenericLinuxPreview -or
    [string]$provenance.GenericLinuxDependencyPolicy -cne 'bundled-codecs-bundled-protobuf-glibc-cxx-system-only-v2' -or
    [bool]$provenance.SyntheticRuntimeInputs) {
    throw 'Generic Linux preview packaging requires a generic-compatible Ubuntu 22.04 x64 runtime-input with matching profile/version and dependency policy.'
}

foreach ($directoryName in @('native-wrapper', 'opencv-runtime', 'opencv-source')) {
    $directory = Join-Path $inputRoot $directoryName
    if (-not (Test-Path -LiteralPath $directory -PathType Container)) {
        throw "Baseline runtime-input is missing required directory: $directoryName"
    }
}

function Remove-TemporaryRoot {
    param([Parameter(Mandatory = $true)][string]$Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Container)) {
        return
    }
    $tempRoot = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd([IO.Path]::DirectorySeparatorChar, [IO.Path]::AltDirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    $fullPath = [IO.Path]::GetFullPath($Path)
    if (-not $fullPath.StartsWith($tempRoot, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to remove preview temporary root outside the system temp directory: $Path"
    }
    Remove-Item -LiteralPath $fullPath -Recurse -Force
}

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-generic-linux-preview-' + [Guid]::NewGuid().ToString('N'))
$temporaryWorkspace = Join-Path $temporaryRoot 'workspace'
$runtimeProject = Join-Path $temporaryWorkspace 'packaging/runtime/JYPPX.OpenCV.runtime'
$stageRoot = Join-Path $temporaryRoot 'stage'
$packageOutput = Join-Path $temporaryRoot 'packages'
$runtimeProjectTemplate = Join-Path $repo 'packaging/runtime/JYPPX.OpenCV.runtime'
$packagePath = Join-Path $outputRoot "$previewPackageId.$((ConvertTo-OpenCvCSharpPackageVersion -Version $PackageVersion).NuGetVersion).nupkg"

try {
    $packageVersionRecord = Assert-OpenCvCSharpPackageVersion -Version $PackageVersion -OpenCvVersion $OpenCvVersion -PackageRevision $PackageRevision
    if (-not (Test-Path -LiteralPath $runtimeProjectTemplate -PathType Container)) {
        throw "Runtime package template was not found: $runtimeProjectTemplate"
    }
    New-Item -ItemType Directory -Force -Path $temporaryWorkspace | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $temporaryWorkspace 'packaging/runtime') | Out-Null
    Copy-Item -LiteralPath $runtimeProjectTemplate -Destination $runtimeProject -Recurse -Force
    Copy-Item -LiteralPath (Join-Path $repo 'Directory.Build.props') -Destination (Join-Path $temporaryWorkspace 'Directory.Build.props') -Force
    Copy-Item -LiteralPath (Join-Path $repo 'README.md') -Destination (Join-Path $temporaryWorkspace 'README.md') -Force
    New-Item -ItemType Directory -Force -Path (Join-Path $temporaryWorkspace 'nuget'), (Join-Path $temporaryWorkspace 'packaging/runtime') | Out-Null
    Copy-Item -LiteralPath (Join-Path $repo 'nuget/logo.jpg') -Destination (Join-Path $temporaryWorkspace 'nuget/logo.jpg') -Force
    Copy-Item -LiteralPath (Join-Path $repo 'packaging/runtime/runtime-distro-rid-graph.json') -Destination (Join-Path $temporaryWorkspace 'packaging/runtime/runtime-distro-rid-graph.json') -Force

    & (Join-Path $repo 'scripts/Stage-Runtime.ps1') `
        -Rid 'linux-x64' `
        -Configuration 'Release' `
        -OpenCvNativeRuntimeDir (Join-Path $inputRoot 'native-wrapper') `
        -OpenCvVersion $OpenCvVersion `
        -OpenCvRid $expectedSourceRid `
        -OpenCvRuntimeVersionSuffix "$OpenCvVersion-$expectedSourceRid" `
        -OpenCvRuntimeDir (Join-Path $inputRoot 'opencv-runtime') `
        -OpenCvSourceDir (Join-Path $inputRoot 'opencv-source') `
        -RuntimeProject $runtimeProject `
        -RuntimePackageMatrix $matrixPath `
        -RuntimeProfile $RuntimeProfile `
        -RuntimePackageId $previewPackageId `
        -PackageVersion $PackageVersion `
        -OutputRoot $stageRoot

    New-Item -ItemType Directory -Force -Path $packageOutput, $outputRoot | Out-Null
    & $dotnet.Source pack (Join-Path $runtimeProject 'JYPPX.OpenCV.runtime.csproj') `
        -c Release -o $packageOutput `
        -p:RuntimePackageRid=linux-x64 `
        -p:RuntimePackageProfile=$RuntimeProfile `
        -p:Version=$PackageVersion `
        -p:PackageVersion=$PackageVersion `
        -p:PackageId=$previewPackageId
    if ($LASTEXITCODE -ne 0) {
        throw 'dotnet pack failed while creating the generic Linux preview package.'
    }

    $produced = @(Get-ChildItem -LiteralPath $packageOutput -Filter "$previewPackageId.*.nupkg" -File)
    if ($produced.Count -ne 1) {
        throw "Generic Linux preview package output was not unique: $($produced.Count)"
    }
    & (Join-Path $repo 'scripts/Normalize-NuGetPackageDeterminism.ps1') -PackagePath $produced[0].FullName
    Copy-Item -LiteralPath $produced[0].FullName -Destination $packagePath -Force

    if ($null -eq $pwsh) {
        throw 'pwsh is required to validate the generic Linux preview package provenance.'
    }
    & $pwsh.Source -NoProfile -File (Join-Path $repo 'scripts/Test-RuntimeReleaseCandidatePreflight.ps1') `
        -RepositoryRoot $repo `
        -RuntimeProject $runtimeProject `
        -RuntimePackageMatrix $matrixPath `
        -Rid 'linux-x64' `
        -RuntimeProfile $RuntimeProfile `
        -RuntimePackageId $previewPackageId `
        -PackageVersion $PackageVersion `
        -OpenCvVersion $OpenCvVersion `
        -OpenCvRid $expectedSourceRid
    if ($LASTEXITCODE -ne 0) {
        throw 'Generic Linux preview runtime preflight failed.'
    }

    $evidence = [ordered]@{
        SchemaVersion = 1
        Status = 'preview-only'
        PublicationAllowed = $false
        TargetRid = 'linux-x64'
        RuntimeProfile = $RuntimeProfile
        PackageId = $previewPackageId
        PackageVersion = $PackageVersion
        ArtifactName = $artifactName
        ProducerSourceRid = $expectedSourceRid
        ProducerInputProvenanceSha256 = (Get-FileHash -LiteralPath $provenancePath -Algorithm SHA256).Hash.ToLowerInvariant()
        PreviewMatrixSha256 = (Get-FileHash -LiteralPath $matrixPath -Algorithm SHA256).Hash.ToLowerInvariant()
        PackageSha256 = (Get-FileHash -LiteralPath $packagePath -Algorithm SHA256).Hash.ToLowerInvariant()
        PackageBytes = (Get-Item -LiteralPath $packagePath).Length
        NativeSmokeExecuted = $false
        ConsumerEvidencePending = $true
    }
    $evidencePath = Join-Path $outputRoot "generic-linux-preview-package-evidence-$RuntimeProfile.json"
    [IO.File]::WriteAllText($evidencePath, (($evidence | ConvertTo-Json -Depth 8) + [Environment]::NewLine), [Text.UTF8Encoding]::new($false))

    Write-Host "GENERIC_LINUX_PREVIEW_PACKAGE_OK target=linux-x64 profile=$RuntimeProfile package=$previewPackageId version=$PackageVersion bytes=$($evidence.PackageBytes) publication_allowed=false native_smoke_executed=false"
    Write-Host "Generic Linux preview package: $packagePath"
    Write-Host "Generic Linux preview evidence: $evidencePath"
}
finally {
    Remove-TemporaryRoot -Path $temporaryRoot
}
