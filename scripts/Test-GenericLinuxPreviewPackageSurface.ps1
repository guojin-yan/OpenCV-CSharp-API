param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$matrixRelativePath = 'packaging/runtime/runtime-generic-linux-preview-matrix.json'
$schemaRelativePath = 'packaging/runtime/runtime-generic-linux-preview-matrix.schema.json'
$packageScriptRelativePath = 'scripts/New-GenericLinuxPreviewPackage.ps1'
$consumerScriptRelativePath = 'scripts/Test-GenericLinuxPreviewConsumer.ps1'
$managedConsumerScriptRelativePath = 'scripts/Test-GitHubPackConsumerRestoreSurface.ps1'
$packWorkflowRelativePath = '.github/workflows/pack.yml'
$producerWorkflowRelativePath = '.github/workflows/runtime-input.yml'
$buildScriptRelativePath = 'scripts/Build-OpenCV.ps1'

function Read-RequiredText {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $repo ($RelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Generic Linux preview package surface was not found: $RelativePath"
    }
    return [IO.File]::ReadAllText($path)
}

$matrixText = Read-RequiredText -RelativePath $matrixRelativePath
$schemaText = Read-RequiredText -RelativePath $schemaRelativePath
$packageText = Read-RequiredText -RelativePath $packageScriptRelativePath
$consumerText = Read-RequiredText -RelativePath $consumerScriptRelativePath
$managedConsumerText = Read-RequiredText -RelativePath $managedConsumerScriptRelativePath
$packWorkflowText = Read-RequiredText -RelativePath $packWorkflowRelativePath
$producerWorkflowText = Read-RequiredText -RelativePath $producerWorkflowRelativePath
$buildScriptText = Read-RequiredText -RelativePath $buildScriptRelativePath
$matrix = $matrixText | ConvertFrom-Json

if ($matrix.status -cne 'preview-only' -or [bool]$matrix.publicationAllowed -or [bool]$matrix.activePackageIdentityAllowed -or
    $matrix.targetRid -cne 'linux-x64' -or $matrix.producer.sourceRid -cne 'ubuntu.22.04-x64') {
    throw 'Generic Linux preview package surface must remain preview-only, non-publishable, and baseline-produced.'
}

foreach ($token in @(
        'preview-only',
        'activePackageIdentityAllowed',
        'previewArtifactIdentityAllowed',
        'publicationAllowed',
        'runtime-generic-linux-preview-matrix.json',
        'runtime-input.provenance.json',
        'Directory.Build.props',
        '[IO.Path]::IsPathRooted($OutputDir)',
        'SyntheticRuntimeInputs',
        'Test-RuntimeReleaseCandidatePreflight.ps1',
        'Normalize-NuGetPackageDeterminism.ps1',
        'GENERIC_LINUX_PREVIEW_PACKAGE_OK')) {
    if ($packageText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "$packageScriptRelativePath is missing required preview package boundary: $token"
    }
}

foreach ($token in @(
        'generic_linux_preview:',
        '$genericLinuxPreviewArguments = @()',
        "`$genericLinuxPreviewArguments += '-GenericLinuxPreview'",
        '$buildPlanParameters = @{',
        '& ./scripts/Build-OpenCV.ps1 @buildPlanParameters',
        '$buildParameters = @{',
        '& ./scripts/Build-OpenCV.ps1 @buildParameters',
        '$artifactParameters = @{',
        '& ./scripts/New-RuntimeInputArtifact.ps1 @artifactParameters',
        'generic-linux-preview-runtime-input-${{ matrix.rid }}-${{ matrix.profile }}',
        'GENERIC_LINUX_PREVIEW_ELF_EVIDENCE',
        "grep -Fq '`$ORIGIN'")) {
    if ($producerWorkflowText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "$producerWorkflowRelativePath is missing generic-compatible producer boundary: $token"
    }
}

foreach ($token in @(
        'bundled-codecs-bundled-protobuf-glibc-cxx-system-only-v2',
        'GenericLinuxDependencyPolicy',
        '-DBUILD_ZLIB=ON',
        '-DBUILD_JPEG=ON',
        '-DBUILD_PNG=ON',
        '-DBUILD_TIFF=ON',
        '-DBUILD_WEBP=ON',
        '-DWITH_OPENEXR=OFF',
        '-DWITH_GSTREAMER=OFF',
        '-DWITH_GTK=OFF',
        '-DWITH_V4L=OFF',
        '-DBUILD_PROTOBUF=ON')) {
    if ($buildScriptText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "$buildScriptRelativePath is missing generic dependency policy: $token"
    }
}

foreach ($forbidden in @('dotnet nuget push', 'gh release', 'publish_github_packages', 'nuget.org')) {
    if ($packageText.IndexOf($forbidden, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
        throw "$packageScriptRelativePath contains forbidden publication surface: $forbidden"
    }
}

if ([regex]::Matches($packageText, '\$LASTEXITCODE\s+-ne\s+0').Count -ne 2) {
    throw "$packageScriptRelativePath must inspect LASTEXITCODE only after its dotnet and child-pwsh process calls; in-process PowerShell staging propagates terminating errors directly."
}

foreach ($token in @(
        'Test-GitHubPackConsumerRestoreSurface.ps1',
        'RuntimePackageIdOverride',
        'RuntimeAssetRidOverride',
        'ConsumerRuntimeRidOverride',
        'runtime-generic-linux-preview-matrix.json',
        'runtime_asset_rid',
        'GENERIC_LINUX_PREVIEW_CONSUMER_OK')) {
    if ($consumerText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "$consumerScriptRelativePath is missing required independent consumer boundary: $token"
    }
}

if ($consumerText.IndexOf('RunNativeSmoke', [StringComparison]::OrdinalIgnoreCase) -lt 0 -or
    $consumerText.IndexOf('PublicationAllowed = $false', [StringComparison]::OrdinalIgnoreCase) -lt 0) {
    throw "$consumerScriptRelativePath must retain native smoke and non-publication evidence."
}

foreach ($token in @(
        'RuntimePackageIdOverride',
        'RuntimeAssetRidOverride',
        'ConsumerRuntimeRidOverride',
        'RuntimeIdentifierGraphPath',
        'runtimes/$runtimeAssetRid/native')) {
    if ($managedConsumerText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        throw "$managedConsumerScriptRelativePath is missing preview consumer override support: $token"
    }
}

if ($schemaText.IndexOf('JYPPX\\.OpenCV\\.runtime\\.linux-x64\\.preview', [StringComparison]::OrdinalIgnoreCase) -lt 0) {
    throw "$schemaRelativePath must constrain preview package identities to the .preview suffix."
}

foreach ($token in @(
        'generic_linux_preview:',
        'generic_linux_preview_full_artifact_run_id:',
        'generic_linux_preview_mini_artifact_run_id:',
        'generic_linux_preview_profile:',
        'validate-generic-linux-preview:',
        'pack-generic-linux-preview:',
        'verify-generic-linux-preview-ubuntu:',
        'verify-generic-linux-preview-debian:',
        'verify-generic-linux-preview-fedora:',
        'generic-linux-preview-runtime-input-ubuntu.22.04-x64-${{ matrix.profile }}',
        'run-id: ${{ matrix.run_id }}',
        'scripts/New-GenericLinuxPreviewPackage.ps1',
        'scripts/Test-GenericLinuxPreviewConsumer.ps1',
        'generic-linux-preview-consumer-ubuntu-${{ matrix.profile }}',
        'generic-linux-preview-consumer-debian-${{ matrix.profile }}',
        'generic-linux-preview-consumer-fedora-${{ matrix.profile }}',
        'generic-linux-preview-consumer-evidence-*.json')) {
    if ($packWorkflowText.IndexOf($token, [StringComparison]::Ordinal) -lt 0) {
        throw "$packWorkflowRelativePath is missing required generic Linux preview dispatch boundary: $token"
    }
}

foreach ($jobName in @(
        'validate-generic-linux-preview',
        'pack-generic-linux-preview',
        'verify-generic-linux-preview-ubuntu',
        'verify-generic-linux-preview-debian',
        'verify-generic-linux-preview-fedora')) {
    $jobMatch = [regex]::Match($packWorkflowText, "(?ms)^  $([regex]::Escape($jobName)):(?<body>.*?)(?=^  [A-Za-z0-9_-]+:|\z)")
    if (-not $jobMatch.Success) {
        throw "$packWorkflowRelativePath is missing generic Linux preview job: $jobName"
    }
    foreach ($forbidden in @('dotnet nuget push', 'gh release', 'publish_github_packages')) {
        if ($jobName -eq 'validate-generic-linux-preview' -and $forbidden -eq 'publish_github_packages') {
            continue
        }
        if ($jobMatch.Groups['body'].Value.IndexOf($forbidden, [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw "$packWorkflowRelativePath generic Linux preview job contains forbidden publication surface: $jobName/$forbidden"
        }
    }
}

Write-Host 'GENERIC_LINUX_PREVIEW_PACKAGE_SURFACE_OK producer=ubuntu.22.04-x64 consumers=3 profiles=2 publication_allowed=false'
Write-Host 'Preview package creation and consumer scripts are isolated from active publication and support matrices.'
