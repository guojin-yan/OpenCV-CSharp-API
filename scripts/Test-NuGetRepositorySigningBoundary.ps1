param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [switch]$LiveReferenceVerification
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
Add-Type -AssemblyName System.IO.Compression.FileSystem

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [Collections.Generic.List[object]]::new()
$dotnet = (Get-Command dotnet -ErrorAction Stop).Source

function Add-Violation {
    param([string]$Path,[string]$Issue,[string]$Text = "")
    $violations.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param([bool]$Condition,[string]$Path,[string]$Issue,[string]$Text = "")
    if (-not $Condition) { Add-Violation -Path $Path -Issue $Issue -Text $Text }
}

function Invoke-ExpectedFailure {
    param([string]$Name,[string[]]$Arguments,[string]$ExpectedText)
    $output = @(& $dotnet @Arguments 2>&1)
    $exitCode = $LASTEXITCODE
    Assert-True -Condition ($exitCode -ne 0) -Path $Name -Issue "Repository-signing negative fixture was accepted"
    Assert-True -Condition (($output -join "`n").Contains($ExpectedText, [StringComparison]::OrdinalIgnoreCase)) -Path $Name -Issue "Repository-signing negative fixture failed for the wrong reason" -Text ($output -join "`n")
}

function Invoke-PowerShellExpectedFailure {
    param([string]$Name,[string]$Script,[string[]]$Arguments,[string]$ExpectedText)
    $output = @(& pwsh -NoProfile -File $Script @Arguments 2>&1)
    $exitCode = $LASTEXITCODE
    Assert-True -Condition ($exitCode -ne 0) -Path $Name -Issue "Publication-manifest negative fixture was accepted"
    Assert-True -Condition (($output -join "`n").Contains($ExpectedText, [StringComparison]::OrdinalIgnoreCase)) -Path $Name -Issue "Publication-manifest negative fixture failed for the wrong reason" -Text ($output -join "`n")
}

function Add-FakeSignature {
    param([string]$Path)
    $archive = [IO.Compression.ZipFile]::Open($Path, [IO.Compression.ZipArchiveMode]::Update)
    try {
        $entry = $archive.CreateEntry(".signature.p7s")
        $stream = $entry.Open()
        try {
            $bytes = [byte[]](1, 2, 3, 4, 5, 6)
            $stream.Write($bytes, 0, $bytes.Length)
        }
        finally { $stream.Dispose() }
    }
    finally { $archive.Dispose() }
}

function Remove-RepositorySignature {
    param([string]$SignedPath,[string]$UnsignedPath)
    $source = [IO.Compression.ZipFile]::OpenRead($SignedPath)
    $destination = [IO.Compression.ZipFile]::Open($UnsignedPath, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($entry in $source.Entries | Where-Object { $_.FullName -ne ".signature.p7s" }) {
            $copy = $destination.CreateEntry($entry.FullName, [IO.Compression.CompressionLevel]::Optimal)
            $copy.LastWriteTime = $entry.LastWriteTime
            if ($entry.FullName.EndsWith("/")) { continue }
            $input = $entry.Open()
            $output = $copy.Open()
            try { $input.CopyTo($output) }
            finally { $output.Dispose(); $input.Dispose() }
        }
    }
    finally { $destination.Dispose(); $source.Dispose() }
}

$toolProject = Join-Path $repo "tools/NuGetRepositorySignatureVerifier/NuGetRepositorySignatureVerifier.csproj"
$toolSource = Join-Path $repo "tools/NuGetRepositorySignatureVerifier/Program.cs"
$wrapper = Join-Path $repo "scripts/Test-NuGetRepositorySignedPackage.ps1"
$publicationBundle = Join-Path $repo "scripts/New-NuGetPublicationBundle.ps1"
$publicationManifest = Join-Path $repo "scripts/Test-NuGetPublicationManifest.ps1"
$workflow = Join-Path $repo ".github/workflows/publish-nuget.yml"
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ("opencv-nuget-repository-boundary-" + [guid]::NewGuid().ToString("N"))

try {
    foreach ($path in @($toolProject, $toolSource, $wrapper, $publicationBundle, $publicationManifest)) {
        Assert-True -Condition (Test-Path -LiteralPath $path -PathType Leaf) -Path $path -Issue "NuGet repository-signing boundary file is missing"
    }
    if ($violations.Count -gt 0) { throw "Required repository-signing files are missing." }

    $projectText = [IO.File]::ReadAllText($toolProject)
    $toolText = [IO.File]::ReadAllText($toolSource)
    $wrapperText = [IO.File]::ReadAllText($wrapper)
    $publicationBundleText = [IO.File]::ReadAllText($publicationBundle)
    $publicationManifestText = [IO.File]::ReadAllText($publicationManifest)
    $tokens = $null
    $parseErrors = $null
    [Management.Automation.Language.Parser]::ParseFile($publicationBundle, [ref]$tokens, [ref]$parseErrors) | Out-Null
    Assert-True -Condition ($parseErrors.Count -eq 0) -Path $publicationBundle -Issue "NuGet publication bundle script must parse without errors" -Text (($parseErrors | ForEach-Object Message) -join "`n")
    $tokens = $null
    $parseErrors = $null
    [Management.Automation.Language.Parser]::ParseFile($publicationManifest, [ref]$tokens, [ref]$parseErrors) | Out-Null
    Assert-True -Condition ($parseErrors.Count -eq 0) -Path $publicationManifest -Issue "NuGet publication manifest script must parse without errors" -Text (($parseErrors | ForEach-Object Message) -join "`n")
    Assert-True -Condition ($projectText.Contains('<PackageReference Include="NuGet.Packaging" Version="6.14.0" />', [StringComparison]::Ordinal)) -Path $toolProject -Issue "NuGet.Packaging dependency must remain exactly pinned"
    foreach ($token in @(
            "RepositoryPrimarySignature",
            "SignatureType.Repository",
            "ValidateIntegrityAsync",
            "V3ServiceIndexUrl",
            "PackageOwners",
            "CN=NuGet.org Repository by Microsoft",
            "TimestampCount",
            ".signature.p7s",
            "AssertPayloadEqual",
            "SensitiveMaterialPresent: false")) {
        Assert-True -Condition ($toolText.Contains($token, [StringComparison]::Ordinal)) -Path $toolSource -Issue "Structured repository-signature verifier lost a required contract" -Text $token
    }
    foreach ($token in @(
            "nuget verify --all --verbosity detailed",
            'DOTNET_CLI_UI_LANGUAGE = "en-US"',
            "Signature type:\s*Repository",
            "CN=NuGet\.org Repository by Microsoft",
            "NuGetRepositorySignatureVerifier.csproj")) {
        Assert-True -Condition ($wrapperText.Contains($token, [StringComparison]::Ordinal)) -Path $wrapper -Issue "Cryptographic repository-signature wrapper lost a required contract" -Text $token
    }
    foreach ($token in @(
            '$_.LastWriteTime.DateTime',
            '$nuspec.Load($nuspecStream)',
            'TrimEnd() + "`n"',
            'AuthorizationToken = "publish-nuget:sha256:$candidateHash"',
            'PublicationManifestPath',
            'PackRunId = $_.RunId',
            'PrivateKeyRequired = $false',
            'PublicationTargets = @(',
            "Channel = 'github-packages'",
            'https://nuget.pkg.github.com/guojin-yan/index.json',
            "RequiredVisibility = 'public'")) {
        Assert-True -Condition ($publicationBundleText.Contains($token, [StringComparison]::Ordinal)) -Path $publicationBundle -Issue "NuGet publication bundle lost a deterministic repository-signing contract" -Text $token
    }
    foreach ($token in @(
            'runtime-support-contract.json',
            '$realTargets.Count -ne 28',
            'Publication manifest must contain exactly',
            'JYPPX.OpenCV.CSharp.API',
            'nupkg-$rid-$profile',
            'NUGET_PUBLICATION_MANIFEST_OK')) {
        Assert-True -Condition ($publicationManifestText.Contains($token, [StringComparison]::Ordinal)) -Path $publicationManifest -Issue "NuGet publication manifest lost its all-real-supported package contract" -Text $token
    }
    foreach ($bypass in @("SkipCryptographic", "AllowUnsigned", "AllowAuthorSignature", "IgnorePayloadMismatch", "FixtureMode")) {
        Assert-True -Condition (-not $wrapperText.Contains($bypass, [StringComparison]::OrdinalIgnoreCase) -and -not $toolText.Contains($bypass, [StringComparison]::OrdinalIgnoreCase)) -Path $wrapper -Issue "Repository-signature verification must not expose a bypass" -Text $bypass
    }

    New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
    $support = Get-Content -LiteralPath (Join-Path $repo 'packaging/runtime/runtime-support-contract.json') -Raw | ConvertFrom-Json
    $manifestPackages = [Collections.Generic.List[object]]::new()
    $manifestPackages.Add([ordered]@{ Kind = 'managed'; Rid = ''; RuntimeProfile = ''; PackageId = 'JYPPX.OpenCV.CSharp.API'; ArtifactName = 'nupkg-managed'; RunId = '123'; Sha256 = ('0' * 64) })
    foreach ($target in @($support.realSupport | Sort-Object)) {
        $rid, $profile = $target -split '/'
        $suffix = if ($profile -eq 'mini') { '.mini' } else { '' }
        $manifestPackages.Add([ordered]@{ Kind = 'runtime'; Rid = $rid; RuntimeProfile = $profile; PackageId = "JYPPX.OpenCV.runtime.$rid$suffix"; ArtifactName = "nupkg-$rid-$profile"; RunId = '123'; Sha256 = ('1' * 64) })
    }
    $manifestRecord = [ordered]@{ SchemaVersion = 1; SourceRevision = ('a' * 40); PackageVersion = '5.0.0-preview.1'; Packages = @($manifestPackages) }
    $manifestFixture = Join-Path $temporaryRoot 'publication-manifest.input.json'
    $normalizedManifest = Join-Path $temporaryRoot 'publication-manifest.json'
    [IO.File]::WriteAllText($manifestFixture, (($manifestRecord | ConvertTo-Json -Depth 8) + "`n"), [Text.UTF8Encoding]::new($false))
    $manifestArguments = @('-ManifestPath', $manifestFixture, '-SourceCommit', ('a' * 40), '-PackageVersion', '5.0.0-preview.1', '-OutputPath', $normalizedManifest)
    & pwsh -NoProfile -File $publicationManifest @manifestArguments
    Assert-True -Condition ($LASTEXITCODE -eq 0) -Path $publicationManifest -Issue 'Valid all-real-supported publication manifest was rejected'
    & pwsh -NoProfile -File $publicationManifest -ManifestPath $normalizedManifest -SourceCommit ('a' * 40) -PackageVersion '5.0.0-preview.1' -OutputPath $normalizedManifest -Check
    Assert-True -Condition ($LASTEXITCODE -eq 0) -Path $publicationManifest -Issue 'Normalized publication manifest check failed'

    $badArtifact = ($manifestRecord | ConvertTo-Json -Depth 8 | ConvertFrom-Json)
    $badArtifact.Packages[1].ArtifactName = 'nupkg-win-x86-full'
    $badArtifactPath = Join-Path $temporaryRoot 'bad-artifact.json'
    [IO.File]::WriteAllText($badArtifactPath, (($badArtifact | ConvertTo-Json -Depth 8) + "`n"), [Text.UTF8Encoding]::new($false))
    Invoke-PowerShellExpectedFailure -Name $badArtifactPath -Script $publicationManifest -Arguments @('-ManifestPath', $badArtifactPath, '-SourceCommit', ('a' * 40), '-PackageVersion', '5.0.0-preview.1') -ExpectedText 'metadata mismatch'

    $pendingTarget = ($manifestRecord | ConvertTo-Json -Depth 8 | ConvertFrom-Json)
    $pendingTarget.Packages[1].Rid = 'win-x86'
    $pendingTarget.Packages[1].RuntimeProfile = 'full'
    $pendingTarget.Packages[1].PackageId = 'JYPPX.OpenCV.runtime.win-x86'
    $pendingTarget.Packages[1].ArtifactName = 'nupkg-win-x86-full'
    $pendingTargetPath = Join-Path $temporaryRoot 'pending-target.json'
    [IO.File]::WriteAllText($pendingTargetPath, (($pendingTarget | ConvertTo-Json -Depth 8) + "`n"), [Text.UTF8Encoding]::new($false))
    Invoke-PowerShellExpectedFailure -Name $pendingTargetPath -Script $publicationManifest -Arguments @('-ManifestPath', $pendingTargetPath, '-SourceCommit', ('a' * 40), '-PackageVersion', '5.0.0-preview.1') -ExpectedText 'missing exact package'

    $excludedTarget = ($manifestRecord | ConvertTo-Json -Depth 8 | ConvertFrom-Json)
    $excludedTarget.Packages[1].Rid = 'android-arm64'
    $excludedTarget.Packages[1].RuntimeProfile = 'full'
    $excludedTarget.Packages[1].PackageId = 'JYPPX.OpenCV.runtime.android-arm64'
    $excludedTarget.Packages[1].ArtifactName = 'nupkg-android-arm64-full'
    $excludedTargetPath = Join-Path $temporaryRoot 'excluded-target.json'
    [IO.File]::WriteAllText($excludedTargetPath, (($excludedTarget | ConvertTo-Json -Depth 8) + "`n"), [Text.UTF8Encoding]::new($false))
    Invoke-PowerShellExpectedFailure -Name $excludedTargetPath -Script $publicationManifest -Arguments @('-ManifestPath', $excludedTargetPath, '-SourceCommit', ('a' * 40), '-PackageVersion', '5.0.0-preview.1') -ExpectedText 'missing exact package'

    $uppercaseHash = ($manifestRecord | ConvertTo-Json -Depth 8 | ConvertFrom-Json)
    $uppercaseHash.Packages[1].Sha256 = ('A' * 64)
    $uppercaseHashPath = Join-Path $temporaryRoot 'uppercase-hash.json'
    [IO.File]::WriteAllText($uppercaseHashPath, (($uppercaseHash | ConvertTo-Json -Depth 8) + "`n"), [Text.UTF8Encoding]::new($false))
    Invoke-PowerShellExpectedFailure -Name $uppercaseHashPath -Script $publicationManifest -Arguments @('-ManifestPath', $uppercaseHashPath, '-SourceCommit', ('a' * 40), '-PackageVersion', '5.0.0-preview.1') -ExpectedText 'lowercase package SHA256'

    $missingPackage = ($manifestRecord | ConvertTo-Json -Depth 8 | ConvertFrom-Json)
    $missingPackage.Packages = @($missingPackage.Packages | Select-Object -Skip 1)
    $missingPackagePath = Join-Path $temporaryRoot 'missing-package.json'
    [IO.File]::WriteAllText($missingPackagePath, (($missingPackage | ConvertTo-Json -Depth 8) + "`n"), [Text.UTF8Encoding]::new($false))
    Invoke-PowerShellExpectedFailure -Name $missingPackagePath -Script $publicationManifest -Arguments @('-ManifestPath', $missingPackagePath, '-SourceCommit', ('a' * 40), '-PackageVersion', '5.0.0-preview.1') -ExpectedText 'exactly 29 packages'

    $fixtureProject = Join-Path $temporaryRoot "fixture/Fixture.csproj"
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $fixtureProject) | Out-Null
    [IO.File]::WriteAllText($fixtureProject, @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <PackageId>JYPPX.OpenCV.RepositorySigning.Fixture</PackageId>
    <Version>1.0.0</Version>
    <Authors>fixture</Authors>
    <Description>Repository-signing negative fixture.</Description>
  </PropertyGroup>
</Project>
"@, [Text.UTF8Encoding]::new($false))
    [IO.File]::WriteAllText((Join-Path (Split-Path -Parent $fixtureProject) "Class1.cs"), "public sealed class Class1 { }`n", [Text.UTF8Encoding]::new($false))
    & $dotnet pack $fixtureProject -c Release -o (Join-Path $temporaryRoot "packages") --nologo | Out-Host
    Assert-True -Condition ($LASTEXITCODE -eq 0) -Path $fixtureProject -Issue "Failed to build unsigned repository-signing fixture"
    $unsigned = Join-Path $temporaryRoot "packages/JYPPX.OpenCV.RepositorySigning.Fixture.1.0.0.nupkg"

    $common = @(
        "run", "--project", $toolProject, "--configuration", "Release", "--framework", "net8.0", "--",
        "--unsigned", $unsigned,
        "--package-id", "JYPPX.OpenCV.RepositorySigning.Fixture",
        "--package-version", "1.0.0",
        "--expected-owner", "GuojinYan",
        "--service-index", "https://api.nuget.org/v3/index.json",
        "--verified-at", "2026-08-02T10:30:00Z")
    Invoke-ExpectedFailure -Name "unsigned-as-signed" -Arguments ($common + @("--signed", $unsigned)) -ExpectedText "does not contain a non-empty .signature.p7s"

    $fakeSigned = Join-Path $temporaryRoot "fake-signed.nupkg"
    [IO.File]::Copy($unsigned, $fakeSigned, $true)
    Add-FakeSignature -Path $fakeSigned
    Invoke-ExpectedFailure -Name "fake-signature" -Arguments ($common + @("--signed", $fakeSigned)) -ExpectedText "signature file entry is invalid"

    $drifted = Join-Path $temporaryRoot "drifted.nupkg"
    [IO.File]::Copy($fakeSigned, $drifted, $true)
    $archive = [IO.Compression.ZipFile]::Open($drifted, [IO.Compression.ZipArchiveMode]::Update)
    try {
        $entry = $archive.CreateEntry("unexpected.txt")
        $stream = $entry.Open()
        try { $stream.WriteByte(1) }
        finally { $stream.Dispose() }
    }
    finally { $archive.Dispose() }
    Invoke-ExpectedFailure -Name "payload-drift" -Arguments ($common + @("--signed", $drifted)) -ExpectedText "payload closure drifted"

    if ($LiveReferenceVerification) {
        $referenceUrl = "https://api.nuget.org/v3-flatcontainer/jyppx.tensorrt.csharp.api/4.0.6170/jyppx.tensorrt.csharp.api.4.0.6170.nupkg"
        $referenceSigned = Join-Path $temporaryRoot "JYPPX.TensorRT.CSharp.API.4.0.6170.repository-signed.nupkg"
        Invoke-WebRequest -Uri $referenceUrl -OutFile $referenceSigned -UseBasicParsing -TimeoutSec 120
        $referenceHash = (Get-FileHash -LiteralPath $referenceSigned -Algorithm SHA256).Hash.ToLowerInvariant()
        Assert-True -Condition ($referenceHash -eq "0944937ed677191f079da81eeb2a9d988cfcfc093992dac1444a6c645f2db091") -Path $referenceUrl -Issue "Immutable TensorRtSharp repository-signed reference hash drifted" -Text $referenceHash
        $referenceUnsigned = Join-Path $temporaryRoot "JYPPX.TensorRT.CSharp.API.4.0.6170.unsigned-payload.nupkg"
        Remove-RepositorySignature -SignedPath $referenceSigned -UnsignedPath $referenceUnsigned
        & pwsh -NoProfile -File $wrapper `
            -UnsignedPackagePath $referenceUnsigned `
            -RepositorySignedPackagePath $referenceSigned `
            -PackageId "JYPPX.TensorRT.CSharp.API" `
            -PackageVersion "4.0.6170" `
            -ExpectedOwner "GuojinYan" `
            -VerifiedAt "2026-08-02T10:30:00Z" `
            -OutputPath (Join-Path $temporaryRoot "reference-report.json") `
            -DotNetPath $dotnet
        Assert-True -Condition ($LASTEXITCODE -eq 0) -Path $referenceUrl -Issue "Live immutable NuGet.org repository-signing reference failed verification"
    }

    if (Test-Path -LiteralPath $workflow -PathType Leaf) {
        $workflowText = [IO.File]::ReadAllText($workflow)
        foreach ($token in @(
                "if: github.repository == 'guojin-yan/OpenCV-CSharp-API'",
                "environment: nuget-production",
                "secrets.NUGET_API_KEY",
                "https://api.nuget.org/v3/index.json",
                "https://nuget.pkg.github.com/guojin-yan/index.json",
                "scripts/Test-NuGetRepositorySignedPackage.ps1",
                "scripts/Test-NuGetPublicationManifest.ps1",
                "publish_authorization",
                "verify_publication",
                "publish-github-packages:",
                "verify-github-packages:",
                "github-packages-publication-proof.json",
                "visibility -ne 'public'",
                "repository.full_name -cne 'guojin-yan/OpenCV-CSharp-API'",
                "publication_manifest_json",
                "gh run download",
                "actions/download-artifact@")) {
            Assert-True -Condition ($workflowText.Contains($token, [StringComparison]::Ordinal)) -Path $workflow -Issue "NuGet publication workflow lost a required repository-signing boundary" -Text $token
        }
        Assert-True -Condition (-not $workflowText.Contains("dotnet nuget sign", [StringComparison]::OrdinalIgnoreCase)) -Path $workflow -Issue "NuGet.org repository-signing strategy must not perform author signing"
    }
}
catch {
    Add-Violation -Path $temporaryRoot -Issue "NuGet repository-signing boundary execution failed" -Text $_.Exception.Message
}
finally {
    if (Test-Path -LiteralPath $temporaryRoot -PathType Container) {
        [IO.Directory]::Delete((Resolve-Path -LiteralPath $temporaryRoot).Path, $true)
    }
}

if ($violations.Count -gt 0) {
    Write-Host "NuGet repository-signing boundary failed with $($violations.Count) violation(s)."
    $violations | Format-List Path, Issue, Text
    exit 1
}

Write-Host "NUGET_REPOSITORY_SIGNING_BOUNDARY_OK strategy=nuget.org-repository-signing owner=GuojinYan public_key_required=false private_key_present=false live_reference=$($LiveReferenceVerification.IsPresent.ToString().ToLowerInvariant())"
Write-Host "Negative fixtures rejected: unsigned package, fake signature, payload drift, verifier bypass surface, pending/excluded runtime targets, artifact drift, hash casing, package-count drift."
