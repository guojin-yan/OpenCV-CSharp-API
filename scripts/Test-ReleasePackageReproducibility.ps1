param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[object]]::new()
$fixedTimestamp = [DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)

function Add-Violation {
    param([Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[string]$Text = '')
    $violations.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param([Parameter(Mandatory = $true)][bool]$Condition,[Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[string]$Text = '')
    if (-not $Condition) { Add-Violation -Path $Path -Issue $Issue -Text $Text }
}

function Write-ZipEntry {
    param([Parameter(Mandatory = $true)][IO.Compression.ZipArchive]$Archive,[Parameter(Mandatory = $true)][string]$Name,[Parameter(Mandatory = $true)][byte[]]$Bytes)
    $entry = $Archive.CreateEntry($Name)
    $stream = $entry.Open()
    try { $stream.Write($Bytes, 0, $Bytes.Length) }
    finally { $stream.Dispose() }
}

function New-NondeterministicFixture {
    param([Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][bool]$ReverseOrder)

    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $Path) | Out-Null
    $coreName = "package/services/metadata/core-properties/$([guid]::NewGuid().ToString('N')).psmdcp"
    $coreText = @"
<?xml version="1.0" encoding="utf-8"?>
<coreProperties xmlns="http://schemas.openxmlformats.org/package/2006/metadata/core-properties">
  <dc:creator xmlns:dc="http://purl.org/dc/elements/1.1/">fixture</dc:creator>
  <dc:identifier xmlns:dc="http://purl.org/dc/elements/1.1/">JYPPX.OpenCV.CSharp.API</dc:identifier>
</coreProperties>
"@.TrimStart()
    $relsText = @"
<?xml version="1.0" encoding="utf-8"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Type="http://schemas.microsoft.com/packaging/2010/07/manifest" Target="/JYPPX.OpenCV.CSharp.API.nuspec" Id="R$([guid]::NewGuid().ToString('N'))" />
  <Relationship Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="/$coreName" Id="R$([guid]::NewGuid().ToString('N'))" />
</Relationships>
"@.TrimStart()
    $entries = @(
        [pscustomobject]@{ Name = 'JYPPX.OpenCV.CSharp.API.nuspec'; Bytes = [Text.UTF8Encoding]::new($false).GetBytes('<package><metadata><id>JYPPX.OpenCV.CSharp.API</id><version>5.0.0.0</version></metadata></package>') },
        [pscustomobject]@{ Name = '_rels/.rels'; Bytes = [Text.UTF8Encoding]::new($false).GetBytes($relsText) },
        [pscustomobject]@{ Name = $coreName; Bytes = [Text.UTF8Encoding]::new($false).GetBytes($coreText) },
        [pscustomobject]@{ Name = 'readme.txt'; Bytes = [Text.UTF8Encoding]::new($false).GetBytes('reproducibility fixture') }
    )
    if ($ReverseOrder) { [array]::Reverse($entries) }
    $archive = [IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Create)
    try { foreach ($entry in $entries) { Write-ZipEntry -Archive $archive -Name $entry.Name -Bytes $entry.Bytes } }
    finally { $archive.Dispose() }
}

function Get-ZipEntryFacts {
    param([Parameter(Mandatory = $true)][string]$Path)

    $archive = [IO.Compression.ZipFile]::OpenRead($Path)
    try {
        return @($archive.Entries | Sort-Object FullName | ForEach-Object {
            [pscustomobject]@{
                Name = $_.FullName
                Length = $_.Length
                LastWriteTime = $_.LastWriteTime
            }
        })
    }
    finally { $archive.Dispose() }
}

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ('opencv-csharp-package-repro-' + [guid]::NewGuid().ToString('N'))
$normalizer = Join-Path $repo 'scripts/Normalize-NuGetPackageDeterminism.ps1'
$managedPack = Join-Path $repo 'scripts/Pack-Managed.ps1'
$runtimePack = Join-Path $repo 'scripts/Pack-Runtime.ps1'
try {
    New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
    $fixture1 = Join-Path $temporaryRoot 'fixture1.nupkg'
    $fixture2 = Join-Path $temporaryRoot 'fixture2.nupkg'
    New-NondeterministicFixture -Path $fixture1 -ReverseOrder $false
    New-NondeterministicFixture -Path $fixture2 -ReverseOrder $true

    foreach ($fixture in @($fixture1, $fixture2)) {
        & pwsh -NoProfile -File $normalizer -PackagePath $fixture | Out-Host
        Assert-True -Condition ($LASTEXITCODE -eq 0) -Path $fixture -Issue 'NuGet package normalizer failed for a valid unsigned package'
    }

    $hash1 = (Get-FileHash -LiteralPath $fixture1 -Algorithm SHA256).Hash.ToLowerInvariant()
    $hash2 = (Get-FileHash -LiteralPath $fixture2 -Algorithm SHA256).Hash.ToLowerInvariant()
    Assert-True -Condition ($hash1 -eq $hash2) -Path $temporaryRoot -Issue 'Normalized packages from equivalent inputs are not byte-identical' -Text "$hash1 / $hash2"
    $beforeIdempotence = $hash1
    & pwsh -NoProfile -File $normalizer -PackagePath $fixture1 | Out-Host
    Assert-True -Condition ($LASTEXITCODE -eq 0) -Path $fixture1 -Issue 'NuGet package normalizer is not idempotent'
    $afterIdempotence = (Get-FileHash -LiteralPath $fixture1 -Algorithm SHA256).Hash.ToLowerInvariant()
    Assert-True -Condition ($beforeIdempotence -eq $afterIdempotence) -Path $fixture1 -Issue 'Second normalization changed package bytes'

    $facts = @(Get-ZipEntryFacts -Path $fixture1)
    Assert-True -Condition (@($facts | Where-Object { $_.LastWriteTime.DateTime -ne $fixedTimestamp.DateTime }).Count -eq 0) -Path $fixture1 -Issue 'Normalized package contains a non-fixed ZIP entry timestamp'
    Assert-True -Condition (@($facts | Where-Object { $_.Name -eq 'package/services/metadata/core-properties/core-properties.psmdcp' }).Count -eq 1) -Path $fixture1 -Issue 'Normalized package core-properties path is not canonical'
    Assert-True -Condition (@($facts | Where-Object { $_.Name -match '^package/services/metadata/core-properties/[^/]+\.psmdcp$' -and $_.Name -ne 'package/services/metadata/core-properties/core-properties.psmdcp' }).Count -eq 0) -Path $fixture1 -Issue 'Normalized package retained a random core-properties path'

    $signedFixture = Join-Path $temporaryRoot 'signed.nupkg'
    [IO.File]::Copy($fixture1, $signedFixture, $true)
    $signedArchive = [IO.Compression.ZipFile]::Open($signedFixture, [IO.Compression.ZipArchiveMode]::Update)
    try { Write-ZipEntry -Archive $signedArchive -Name 'package/services/metadata/signatures/.signature.p7s' -Bytes ([byte[]](1, 2, 3)) }
    finally { $signedArchive.Dispose() }
    $null = & pwsh -NoProfile -File $normalizer -PackagePath $signedFixture 2>&1
    Assert-True -Condition ($LASTEXITCODE -ne 0) -Path $signedFixture -Issue 'NuGet package normalizer accepted a signed package that must be normalized before signing'

    foreach ($packScript in @($managedPack, $runtimePack)) {
        $packText = [IO.File]::ReadAllText($packScript)
        Assert-True -Condition ($packText.Contains('Normalize-NuGetPackageDeterminism.ps1')) -Path $packScript -Issue 'Pack script does not invoke deterministic NuGet normalization'
        Assert-True -Condition (([regex]::Matches($packText, 'Normalize-NuGetPackageDeterminism\.ps1')).Count -eq 1) -Path $packScript -Issue 'Pack script must invoke deterministic NuGet normalization exactly once'
    }

    Write-Host "RELEASE_PACKAGE_REPRODUCIBILITY_OK entries=$($facts.Count) fixed_timestamp=2000-01-01T00:00:00Z hash=$hash1"
}
catch {
    Add-Violation -Path $temporaryRoot -Issue 'Release package reproducibility guard execution failed' -Text $_.Exception.Message
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) { [IO.Directory]::Delete((Resolve-Path -LiteralPath $temporaryRoot).Path, $true) }
}

if ($violations.Count -gt 0) {
    Write-Host "Release package reproducibility guard failed with $($violations.Count) violation(s)."
    $violations | Format-List Path, Issue, Text
    exit 1
}

Write-Host 'Release package reproducibility guard passed.'
Write-Host 'Validated deterministic ZIP ordering/timestamps, canonical core-properties relationships, idempotence, signed-package refusal, and managed/runtime pack integration.'
