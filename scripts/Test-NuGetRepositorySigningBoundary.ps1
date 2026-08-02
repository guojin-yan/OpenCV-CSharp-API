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
$workflow = Join-Path $repo ".github/workflows/publish-nuget.yml"
$temporaryRoot = Join-Path ([IO.Path]::GetTempPath()) ("opencv-nuget-repository-boundary-" + [guid]::NewGuid().ToString("N"))

try {
    foreach ($path in @($toolProject, $toolSource, $wrapper, $publicationBundle)) {
        Assert-True -Condition (Test-Path -LiteralPath $path -PathType Leaf) -Path $path -Issue "NuGet repository-signing boundary file is missing"
    }
    if ($violations.Count -gt 0) { throw "Required repository-signing files are missing." }

    $projectText = [IO.File]::ReadAllText($toolProject)
    $toolText = [IO.File]::ReadAllText($toolSource)
    $wrapperText = [IO.File]::ReadAllText($wrapper)
    $publicationBundleText = [IO.File]::ReadAllText($publicationBundle)
    $tokens = $null
    $parseErrors = $null
    [Management.Automation.Language.Parser]::ParseFile($publicationBundle, [ref]$tokens, [ref]$parseErrors) | Out-Null
    Assert-True -Condition ($parseErrors.Count -eq 0) -Path $publicationBundle -Issue "NuGet publication bundle script must parse without errors" -Text (($parseErrors | ForEach-Object Message) -join "`n")
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
            'PrivateKeyRequired = $false')) {
        Assert-True -Condition ($publicationBundleText.Contains($token, [StringComparison]::Ordinal)) -Path $publicationBundle -Issue "NuGet publication bundle lost a deterministic repository-signing contract" -Text $token
    }
    foreach ($bypass in @("SkipCryptographic", "AllowUnsigned", "AllowAuthorSignature", "IgnorePayloadMismatch", "FixtureMode")) {
        Assert-True -Condition (-not $wrapperText.Contains($bypass, [StringComparison]::OrdinalIgnoreCase) -and -not $toolText.Contains($bypass, [StringComparison]::OrdinalIgnoreCase)) -Path $wrapper -Issue "Repository-signature verification must not expose a bypass" -Text $bypass
    }

    New-Item -ItemType Directory -Force -Path $temporaryRoot | Out-Null
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
                "scripts/Test-NuGetRepositorySignedPackage.ps1",
                "publish_authorization",
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
Write-Host "Negative fixtures rejected: unsigned package, fake signature, payload drift, verifier bypass surface."
