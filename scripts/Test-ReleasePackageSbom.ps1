param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

Add-Type -AssemblyName System.IO.Compression.FileSystem

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$generatorPath = Join-Path $repo "scripts/New-ReleasePackageSbom.ps1"
$pwsh = Get-Command pwsh -ErrorAction SilentlyContinue
if ($null -eq $pwsh) {
    throw "pwsh was not found. Release package SBOM tests require PowerShell 7+."
}
if (-not (Test-Path -LiteralPath $generatorPath -PathType Leaf)) {
    throw "Release package SBOM generator was not found: $generatorPath"
}

$sourceCommit = '1111111111111111111111111111111111111111'
$created = '2026-08-02T04:01:42Z'
$packageId = 'JYPPX.OpenCV.runtime.win-x64'
$packageVersion = '5.0.0-preview.1'
$sourcePackageVersion = '5.0.0.0-preview.1'
$violations = [Collections.Generic.List[object]]::new()
$negativeFixtureCount = 0

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[object]]$List,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $List.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [Collections.Generic.List[object]]$List,
        [Parameter(Mandatory = $true)]
        [bool]$Condition,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    if (-not $Condition) {
        Add-Violation -List $List -Path $Path -Issue $Issue -Text $Text
    }
}

function Get-TextBytes {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Text
    )

    return [Text.UTF8Encoding]::new($false).GetBytes($Text)
}

function New-FixturePackage {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [string]$NuspecVersion = $packageVersion,
        [string]$ProvenancePackageVersion = $sourcePackageVersion,
        [string]$ProvenanceOpenCvVersion = '5.0.0',
        [string]$RepositoryCommit = $sourceCommit,
        [string]$ReadmeText = '# deterministic SBOM fixture',
        [DateTimeOffset]$Timestamp = ([DateTimeOffset]::new(2000, 1, 1, 0, 0, 0, [TimeSpan]::Zero)),
        [switch]$Synthetic,
        [switch]$OmitNuspec,
        [switch]$OmitLicenseExpression,
        [switch]$IncludeSignature,
        [switch]$IncludeUnsafePath,
        [switch]$IncludeCaseCollision,
        [switch]$IncludeDtd
    )

    $directory = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
    if (Test-Path -LiteralPath $Path -PathType Leaf) {
        [IO.File]::Delete($Path)
    }

    $license = if ($OmitLicenseExpression) { '' } else { '<license type="expression">MIT AND Apache-2.0</license>' }
    $doctype = if ($IncludeDtd) { '<!DOCTYPE package [<!ENTITY author SYSTEM "file:///release-sbom-xxe">]>' } else { '' }
    $author = if ($IncludeDtd) { '&author;' } else { 'guojin-yan' }
    $nuspec = @"
<?xml version="1.0" encoding="utf-8"?>
$doctype<package><metadata><id>$packageId</id><version>$NuspecVersion</version><authors>$author</authors>$license<copyright>Copyright (c) 2026 guojin-yan</copyright><repository type="git" url="https://github.com/guojin-yan/OpenCV-CSharp-API" commit="$RepositoryCommit" /></metadata></package>
"@.Trim()

    $provenance = [ordered]@{
        SchemaVersion = 1
        PackageId = $packageId
        PackageVersion = $ProvenancePackageVersion
        OpenCvVersion = $ProvenanceOpenCvVersion
        Rid = 'win-x64'
        RuntimeProfile = 'full'
        SyntheticRuntimeInputs = [bool]$Synthetic
        RequiredModules = @('core', 'imgproc')
        OptionalModulesStaged = @('dnn')
    } | ConvertTo-Json -Depth 5

    $items = [Collections.Generic.List[object]]::new()
    if (-not $OmitNuspec) {
        $items.Add([pscustomobject]@{ Name = "$packageId.nuspec"; Bytes = Get-TextBytes -Text $nuspec })
    }
    $items.Add([pscustomobject]@{ Name = 'README.md'; Bytes = Get-TextBytes -Text $ReadmeText })
    $items.Add([pscustomobject]@{ Name = 'build/JYPPX.OpenCV.runtime.provenance.json'; Bytes = Get-TextBytes -Text $provenance })
    $items.Add([pscustomobject]@{ Name = 'licenses/LICENSE'; Bytes = Get-TextBytes -Text 'MIT and Apache-2.0 fixture evidence' })
    $items.Add([pscustomobject]@{ Name = 'runtimes/win-x64/native/JYPPX.OpenCV.Native.dll'; Bytes = [byte[]](1, 2, 3, 4) })
    $items.Add([pscustomobject]@{ Name = 'runtimes/win-x64/native/opencv_core500.dll'; Bytes = [byte[]](5, 6, 7, 8) })
    if ($IncludeSignature) {
        $items.Add([pscustomobject]@{ Name = '.signature.p7s'; Bytes = [byte[]](9, 10) })
    }
    if ($IncludeUnsafePath) {
        $items.Add([pscustomobject]@{ Name = '../escape.txt'; Bytes = Get-TextBytes -Text 'unsafe' })
    }
    if ($IncludeCaseCollision) {
        $items.Add([pscustomobject]@{ Name = 'readme.md'; Bytes = Get-TextBytes -Text 'collision' })
    }

    $archive = [IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($item in $items) {
            $entry = $archive.CreateEntry($item.Name, [IO.Compression.CompressionLevel]::Optimal)
            $entry.LastWriteTime = $Timestamp
            $stream = $entry.Open()
            try {
                $stream.Write($item.Bytes, 0, $item.Bytes.Length)
            }
            finally {
                $stream.Dispose()
            }
        }
    }
    finally {
        $archive.Dispose()
    }
}

function Invoke-SbomGenerator {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Package,
        [Parameter(Mandatory = $true)]
        [string]$Output,
        [string]$Commit = $sourceCommit,
        [string]$CreatedValue = $created,
        [string[]]$AdditionalArguments = @(),
        [switch]$Check
    )

    $arguments = @(
        '-NoProfile',
        '-File', $generatorPath,
        '-PackagePath', $Package,
        '-SourceCommit', $Commit,
        '-Created', $CreatedValue,
        '-OutputPath', $Output
    )
    $arguments += $AdditionalArguments
    if ($Check) {
        $arguments += '-Check'
    }

    $outputLines = @(& $pwsh.Source @arguments 2>&1)
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Output = ($outputLines | ForEach-Object { [string]$_ }) -join "`n"
    }
}

function Assert-Rejected {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name,
        [Parameter(Mandatory = $true)]
        [scriptblock]$Action,
        [Parameter(Mandatory = $true)]
        [string]$ExpectedText
    )

    $script:negativeFixtureCount++
    try {
        $result = & $Action
        Assert-True -List $violations -Condition ($result.ExitCode -ne 0) -Path $Name -Issue 'Negative SBOM fixture was accepted' -Text $result.Output
        Assert-True -List $violations -Condition ($result.Output.IndexOf($ExpectedText, [StringComparison]::OrdinalIgnoreCase) -ge 0) -Path $Name -Issue 'Negative SBOM fixture failed for the wrong reason' -Text $result.Output
    }
    catch {
        Add-Violation -List $violations -Path $Name -Issue 'Negative SBOM fixture harness failed' -Text $_.Exception.Message
    }
}

$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ("opencv-csharp-release-sbom-" + [guid]::NewGuid().ToString('N'))
try {
    $packageRoot = Join-Path $temporaryRoot 'packages'
    $validPackage = Join-Path $packageRoot "$packageId.$packageVersion.nupkg"
    $sbomOne = Join-Path $temporaryRoot 'sbom/one.spdx.json'
    $sbomTwo = Join-Path $temporaryRoot 'sbom/two.spdx.json'
    New-FixturePackage -Path $validPackage

    $first = Invoke-SbomGenerator -Package $validPackage -Output $sbomOne
    $second = Invoke-SbomGenerator -Package $validPackage -Output $sbomTwo
    Assert-True -List $violations -Condition ($first.ExitCode -eq 0) -Path $generatorPath -Issue 'Positive SBOM generation failed' -Text $first.Output
    Assert-True -List $violations -Condition ($second.ExitCode -eq 0) -Path $generatorPath -Issue 'Repeated positive SBOM generation failed' -Text $second.Output

    if ((Test-Path -LiteralPath $sbomOne -PathType Leaf) -and (Test-Path -LiteralPath $sbomTwo -PathType Leaf)) {
        $firstBytes = [IO.File]::ReadAllBytes($sbomOne)
        $secondBytes = [IO.File]::ReadAllBytes($sbomTwo)
        Assert-True -List $violations -Condition ([Linq.Enumerable]::SequenceEqual($firstBytes, $secondBytes)) -Path $generatorPath -Issue 'Repeated SBOM generation is not byte-for-byte deterministic'
        Assert-True -List $violations -Condition (-not ($firstBytes.Length -ge 3 -and $firstBytes[0] -eq 0xEF -and $firstBytes[1] -eq 0xBB -and $firstBytes[2] -eq 0xBF)) -Path $generatorPath -Issue 'SBOM output must use UTF-8 without BOM'

        $jsonText = [Text.UTF8Encoding]::new($false, $true).GetString($firstBytes)
        Assert-True -List $violations -Condition ($jsonText.EndsWith("`n", [StringComparison]::Ordinal) -and -not $jsonText.Contains("`r")) -Path $generatorPath -Issue 'SBOM output must use one final LF and no CR characters'
        Assert-True -List $violations -Condition ($jsonText.IndexOf($temporaryRoot, [StringComparison]::OrdinalIgnoreCase) -lt 0) -Path $generatorPath -Issue 'SBOM output must not retain temporary absolute paths'

        $document = $jsonText | ConvertFrom-Json
        $packageHash = (Get-FileHash -LiteralPath $validPackage -Algorithm SHA256).Hash.ToLowerInvariant()
        Assert-True -List $violations -Condition ($document.spdxVersion -eq 'SPDX-2.3' -and $document.dataLicense -eq 'CC0-1.0' -and $document.SPDXID -eq 'SPDXRef-DOCUMENT') -Path $generatorPath -Issue 'SBOM document header is not SPDX-2.3'
        $actualCreated = if ($document.creationInfo.created -is [DateTime]) {
            $document.creationInfo.created.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'")
        }
        else {
            [string]$document.creationInfo.created
        }
        Assert-True -List $violations -Condition ($actualCreated -eq $created -and @($document.creationInfo.creators).Count -eq 1) -Path $generatorPath -Issue 'SBOM creation provenance is incomplete or nondeterministic'
        Assert-True -List $violations -Condition ($document.documentNamespace.EndsWith("/$packageHash", [StringComparison]::Ordinal)) -Path $generatorPath -Issue 'SBOM namespace does not bind the package SHA256'
        Assert-True -List $violations -Condition (@($document.packages).Count -eq 2 -and @($document.files).Count -eq 6 -and @($document.relationships).Count -eq 8) -Path $generatorPath -Issue 'SBOM package/file/relationship closure is incomplete'

        $mainPackage = @($document.packages | Where-Object { $_.name -eq $packageId })
        $openCvPackage = @($document.packages | Where-Object { $_.name -eq 'OpenCV' })
        Assert-True -List $violations -Condition ($mainPackage.Count -eq 1 -and $mainPackage[0].versionInfo -eq $packageVersion -and [bool]$mainPackage[0].filesAnalyzed) -Path $generatorPath -Issue 'SBOM main package identity is incomplete'
        Assert-True -List $violations -Condition ($mainPackage.Count -eq 1 -and $mainPackage[0].checksums[0].checksumValue -eq $packageHash -and $mainPackage[0].packageVerificationCode.packageVerificationCodeValue -match '^[0-9a-f]{40}$') -Path $generatorPath -Issue 'SBOM main package checksums are incomplete'
        Assert-True -List $violations -Condition ($mainPackage.Count -eq 1 -and $mainPackage[0].externalRefs[0].referenceLocator -eq "pkg:nuget/$packageId@$packageVersion") -Path $generatorPath -Issue 'SBOM NuGet PURL is missing or incorrect'
        Assert-True -List $violations -Condition ($mainPackage.Count -eq 1 -and $mainPackage[0].comment -match 'core,dnn,imgproc' -and $mainPackage[0].comment -match [regex]::Escape($sourcePackageVersion)) -Path $generatorPath -Issue 'SBOM runtime module/source-version provenance is incomplete'
        Assert-True -List $violations -Condition ($openCvPackage.Count -eq 1 -and $openCvPackage[0].versionInfo -eq '5.0.0' -and -not [bool]$openCvPackage[0].filesAnalyzed -and $openCvPackage[0].licenseDeclared -eq 'Apache-2.0') -Path $generatorPath -Issue 'SBOM OpenCV dependency identity is incomplete'

        [string[]]$fileNames = @($document.files | ForEach-Object { [string]$_.fileName })
        [string[]]$sortedFileNames = @($fileNames)
        [Array]::Sort($sortedFileNames, [StringComparer]::Ordinal)
        Assert-True -List $violations -Condition (($fileNames -join "`n") -ceq ($sortedFileNames -join "`n")) -Path $generatorPath -Issue 'SBOM files are not ordered deterministically'
        foreach ($file in @($document.files)) {
            Assert-True -List $violations -Condition ($file.SPDXID -match '^SPDXRef-File-[0-9]{4}-[0-9a-f]{12}$' -and @($file.checksums).Count -eq 2 -and $file.checksums[0].checksumValue -match '^[0-9a-f]{40}$' -and $file.checksums[1].checksumValue -match '^[0-9a-f]{64}$') -Path $generatorPath -Issue 'SBOM file checksum/SPDX identity is malformed' -Text $file.fileName
        }
    }

    $check = Invoke-SbomGenerator -Package $validPackage -Output $sbomOne -Check
    Assert-True -List $violations -Condition ($check.ExitCode -eq 0 -and $check.Output -match 'mode=check') -Path $generatorPath -Issue 'SBOM check mode rejected a fresh document' -Text $check.Output

    Assert-Rejected -Name 'source commit drift' -ExpectedText 'repository provenance' -Action {
        Invoke-SbomGenerator -Package $validPackage -Output (Join-Path $temporaryRoot 'negative/source.spdx.json') -Commit ('2' * 40)
    }

    $timestampPackage = Join-Path $packageRoot 'timestamp/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $timestampPackage -Timestamp ([DateTimeOffset]::new(2001, 1, 1, 0, 0, 0, [TimeSpan]::Zero))
    Assert-Rejected -Name 'unnormalized timestamp' -ExpectedText 'timestamp' -Action {
        Invoke-SbomGenerator -Package $timestampPackage -Output (Join-Path $temporaryRoot 'negative/timestamp.spdx.json')
    }

    $signedPackage = Join-Path $packageRoot 'signed/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $signedPackage -IncludeSignature
    Assert-Rejected -Name 'signed package' -ExpectedText 'unsigned package' -Action {
        Invoke-SbomGenerator -Package $signedPackage -Output (Join-Path $temporaryRoot 'negative/signed.spdx.json')
    }

    $unsafePackage = Join-Path $packageRoot 'unsafe/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $unsafePackage -IncludeUnsafePath
    Assert-Rejected -Name 'unsafe entry path' -ExpectedText 'unsafe entry' -Action {
        Invoke-SbomGenerator -Package $unsafePackage -Output (Join-Path $temporaryRoot 'negative/unsafe.spdx.json')
    }

    $collisionPackage = Join-Path $packageRoot 'collision/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $collisionPackage -IncludeCaseCollision
    Assert-Rejected -Name 'case-colliding entry path' -ExpectedText 'case-colliding' -Action {
        Invoke-SbomGenerator -Package $collisionPackage -Output (Join-Path $temporaryRoot 'negative/collision.spdx.json')
    }

    $missingNuspecPackage = Join-Path $packageRoot 'missing-nuspec/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $missingNuspecPackage -OmitNuspec
    Assert-Rejected -Name 'missing nuspec' -ExpectedText 'exactly one nuspec' -Action {
        Invoke-SbomGenerator -Package $missingNuspecPackage -Output (Join-Path $temporaryRoot 'negative/missing-nuspec.spdx.json')
    }

    $wrongNamePackage = Join-Path $packageRoot 'wrong-name/wrong.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $wrongNamePackage
    Assert-Rejected -Name 'filename identity drift' -ExpectedText 'filename' -Action {
        Invoke-SbomGenerator -Package $wrongNamePackage -Output (Join-Path $temporaryRoot 'negative/wrong-name.spdx.json')
    }

    $syntheticPackage = Join-Path $packageRoot 'synthetic/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $syntheticPackage -Synthetic
    Assert-Rejected -Name 'synthetic runtime provenance' -ExpectedText 'release eligible' -Action {
        Invoke-SbomGenerator -Package $syntheticPackage -Output (Join-Path $temporaryRoot 'negative/synthetic.spdx.json')
    }

    $sourceVersionPackage = Join-Path $packageRoot 'source-version/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $sourceVersionPackage -ProvenancePackageVersion '5.0.0.1-preview.1'
    Assert-Rejected -Name 'source package version drift' -ExpectedText 'does not normalize' -Action {
        Invoke-SbomGenerator -Package $sourceVersionPackage -Output (Join-Path $temporaryRoot 'negative/source-version.spdx.json')
    }

    $openCvVersionPackage = Join-Path $packageRoot 'opencv-version/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $openCvVersionPackage -ProvenanceOpenCvVersion '5.0.1'
    Assert-Rejected -Name 'OpenCV version drift' -ExpectedText 'release eligible' -Action {
        Invoke-SbomGenerator -Package $openCvVersionPackage -Output (Join-Path $temporaryRoot 'negative/opencv-version.spdx.json')
    }

    $licensePackage = Join-Path $packageRoot 'license/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $licensePackage -OmitLicenseExpression
    Assert-Rejected -Name 'missing license expression' -ExpectedText 'license expression' -Action {
        Invoke-SbomGenerator -Package $licensePackage -Output (Join-Path $temporaryRoot 'negative/license.spdx.json')
    }

    $versionPackage = Join-Path $packageRoot 'version/JYPPX.OpenCV.runtime.win-x64.5.0.0-Preview.1.nupkg'
    New-FixturePackage -Path $versionPackage -NuspecVersion '5.0.0-Preview.1'
    Assert-Rejected -Name 'noncanonical NuGet version' -ExpectedText 'canonical NuGet' -Action {
        Invoke-SbomGenerator -Package $versionPackage -Output (Join-Path $temporaryRoot 'negative/version.spdx.json')
    }

    $dtdPackage = Join-Path $packageRoot 'dtd/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    New-FixturePackage -Path $dtdPackage -IncludeDtd
    Assert-Rejected -Name 'nuspec DTD or XXE' -ExpectedText 'DTD' -Action {
        Invoke-SbomGenerator -Package $dtdPackage -Output (Join-Path $temporaryRoot 'negative/dtd.spdx.json')
    }

    $stalePackage = Join-Path $packageRoot 'stale/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    $staleOutput = Join-Path $temporaryRoot 'negative/stale.spdx.json'
    New-FixturePackage -Path $stalePackage -ReadmeText 'before package drift'
    $staleInitial = Invoke-SbomGenerator -Package $stalePackage -Output $staleOutput
    Assert-True -List $violations -Condition ($staleInitial.ExitCode -eq 0) -Path 'stale package fixture' -Issue 'Could not create initial SBOM for stale-package fixture' -Text $staleInitial.Output
    New-FixturePackage -Path $stalePackage -ReadmeText 'after package drift'
    Assert-Rejected -Name 'package byte drift' -ExpectedText 'stale' -Action {
        Invoke-SbomGenerator -Package $stalePackage -Output $staleOutput -Check
    }

    $tamperedPackage = Join-Path $packageRoot 'tampered/JYPPX.OpenCV.runtime.win-x64.5.0.0-preview.1.nupkg'
    $tamperedOutput = Join-Path $temporaryRoot 'negative/tampered.spdx.json'
    New-FixturePackage -Path $tamperedPackage
    $tamperedInitial = Invoke-SbomGenerator -Package $tamperedPackage -Output $tamperedOutput
    Assert-True -List $violations -Condition ($tamperedInitial.ExitCode -eq 0) -Path 'tampered SBOM fixture' -Issue 'Could not create initial SBOM for document-tamper fixture' -Text $tamperedInitial.Output
    [IO.File]::AppendAllText($tamperedOutput, " ", [Text.UTF8Encoding]::new($false))
    Assert-Rejected -Name 'SBOM document drift' -ExpectedText 'stale' -Action {
        Invoke-SbomGenerator -Package $tamperedPackage -Output $tamperedOutput -Check
    }

    Assert-Rejected -Name 'invalid creation timestamp' -ExpectedText 'Cannot validate argument' -Action {
        Invoke-SbomGenerator -Package $validPackage -Output (Join-Path $temporaryRoot 'negative/created.spdx.json') -CreatedValue '2026-08-02T04:01:42+00:00'
    }

    Assert-Rejected -Name 'repository URL drift' -ExpectedText 'authoritative release repository' -Action {
        Invoke-SbomGenerator -Package $validPackage -Output (Join-Path $temporaryRoot 'negative/repository.spdx.json') -AdditionalArguments @('-RepositoryUrl', 'https://example.invalid/repository')
    }
}
catch {
    Add-Violation -List $violations -Path $temporaryRoot -Issue 'Release package SBOM guard execution failed' -Text $_.Exception.Message
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
        [IO.Directory]::Delete((Resolve-Path -LiteralPath $temporaryRoot).Path, $true)
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Release package SBOM guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Issue | Format-List Path, Issue, Text
    exit 1
}

Write-Host "RELEASE_PACKAGE_SBOM_OK format=SPDX-2.3 deterministic=true negative_fixtures=$negativeFixtureCount private_keys=false remote_mutation=false"
Write-Host 'Release package SBOM guard passed.'
Write-Host 'Validated exact normalized package bytes, source/runtime provenance, file closure, SPDX relationships, deterministic serialization, and fail-closed package/document drift.'
