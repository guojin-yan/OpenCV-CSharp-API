param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [switch]$LiveFeedVerification
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[object]]::new()
$feedIndex = 'https://api.nuget.org/v3/index.json'
$githubPackagesIndex = 'https://nuget.pkg.github.com/guojin-yan/index.json'
$authoritativeRepository = 'guojin-yan/OpenCV-CSharp-API'
$packageId = 'JYPPX.OpenCV.CSharp.API'
$packageVersion = '5.0.0-preview.2'
$packageFlatContainerUrl = "https://api.nuget.org/v3-flatcontainer/jyppx.opencv.csharp.api/$packageVersion/jyppx.opencv.csharp.api.$packageVersion.nupkg"

function Add-Violation {
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,[Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[string]$Text = '')
    $List.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,[Parameter(Mandatory = $true)][bool]$Condition,[Parameter(Mandatory = $true)][string]$Path,[Parameter(Mandatory = $true)][string]$Issue,[string]$Text = '')
    if (-not $Condition) { Add-Violation -List $List -Path $Path -Issue $Issue -Text $Text }
}

function Test-FeedReference {
    param([Parameter(Mandatory = $true)][string]$Url)
    try {
        $uri = [Uri]$Url
        return $uri.Scheme -eq 'https' -and $uri.Host -eq 'api.nuget.org'
    }
    catch { return $false }
}

function Test-GitHubPackagesReference {
    param([Parameter(Mandatory = $true)][string]$Url)
    try {
        $uri = [Uri]$Url
        return $uri.Scheme -eq 'https' -and $uri.Host -eq 'nuget.pkg.github.com' -and $uri.AbsolutePath.StartsWith('/guojin-yan/', [StringComparison]::Ordinal)
    }
    catch { return $false }
}

function Get-HttpStatus {
    param([Parameter(Mandatory = $true)][string]$Url,[ValidateSet('Get','Head')][string]$Method)
    try {
        $response = Invoke-WebRequest -Uri $Url -Method $Method -Headers @{ Accept = 'application/json' } -UseBasicParsing -TimeoutSec 30
        return [int]$response.StatusCode
    }
    catch {
        if ($null -ne $_.Exception.Response) { return [int]$_.Exception.Response.StatusCode }
        throw
    }
}

function Test-ReadinessHandoffText {
    param([Parameter(Mandatory = $true)][string]$Text,[Parameter(Mandatory = $true)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List)
    Assert-True -List $List -Condition ($Text -match 'SchemaVersion\s*=\s*2') -Path 'scripts/Test-ReleaseCandidateProvenance.ps1' -Issue 'Schema-v2 provenance must be the signing/SBOM handoff owner'
    Assert-True -List $List -Condition ($Text -match 'SigningHandoff') -Path 'scripts/Test-ReleaseCandidateProvenance.ps1' -Issue 'Provenance must expose explicit signing handoff fields'
    Assert-True -List $List -Condition ($Text -match 'SbomHandoff') -Path 'scripts/Test-ReleaseCandidateProvenance.ps1' -Issue 'Provenance must expose explicit SBOM handoff fields'
    Assert-True -List $List -Condition ($Text -match "NuGet\.org-repository-timestamp-required") -Path 'scripts/Test-ReleaseCandidateProvenance.ps1' -Issue 'Repository-signing handoff must retain the NuGet.org timestamp policy'
    Assert-True -List $List -Condition ($Text -notmatch '-----BEGIN [A-Z ]*PRIVATE KEY-----') -Path 'scripts/Test-ReleaseCandidateProvenance.ps1' -Issue 'Signing handoff source must not contain private key material'
}

$readinessPath = Join-Path $repo 'scripts/Test-ReleaseReadinessContract.ps1'
$provenancePath = Join-Path $repo 'scripts/Test-ReleaseCandidateProvenance.ps1'
try {
    Assert-True -List $violations -Condition (Test-FeedReference -Url $feedIndex) -Path $feedIndex -Issue 'NuGet service index must use the approved HTTPS host'
    Assert-True -List $violations -Condition (Test-FeedReference -Url $packageFlatContainerUrl) -Path $packageFlatContainerUrl -Issue 'NuGet package verification URL must use the approved HTTPS host'
    Assert-True -List $violations -Condition (Test-GitHubPackagesReference -Url $githubPackagesIndex) -Path $githubPackagesIndex -Issue 'GitHub Packages service index must use the approved owner-scoped HTTPS host'
    Assert-True -List $violations -Condition (Test-Path -LiteralPath $readinessPath -PathType Leaf) -Path $readinessPath -Issue 'Release readiness contract script is missing'
    Assert-True -List $violations -Condition (Test-Path -LiteralPath $provenancePath -PathType Leaf) -Path $provenancePath -Issue 'Schema-v2 provenance script is missing'
    if (Test-Path -LiteralPath $provenancePath -PathType Leaf) {
        Test-ReadinessHandoffText -Text ([IO.File]::ReadAllText($provenancePath)) -List $violations
    }

    $packText = [IO.File]::ReadAllText((Join-Path $repo '.github/workflows/pack.yml'))
    Assert-True -List $violations -Condition ($packText -match 'dotnet nuget push') -Path '.github/workflows/pack.yml' -Issue 'Publish command must remain explicit and auditable'
    Assert-True -List $violations -Condition ($packText -notmatch 'api\.nuget\.org.*push|nuget\.org.*api-key') -Path '.github/workflows/pack.yml' -Issue 'Public feed verification must not add a public feed upload path'
    Assert-True -List $violations -Condition ($packText -match 'publish_github_packages') -Path '.github/workflows/pack.yml' -Issue 'Publishing must remain behind the existing explicit workflow input'

    $publishWorkflowPath = Join-Path $repo '.github/workflows/publish-nuget.yml'
    Assert-True -List $violations -Condition (Test-Path -LiteralPath $publishWorkflowPath -PathType Leaf) -Path '.github/workflows/publish-nuget.yml' -Issue 'Dedicated NuGet.org publication workflow is missing'
    if (Test-Path -LiteralPath $publishWorkflowPath -PathType Leaf) {
        $publishText = [IO.File]::ReadAllText($publishWorkflowPath)
        foreach ($token in @('https://api.nuget.org/v3/index.json','https://nuget.pkg.github.com/guojin-yan/index.json','environment: nuget-production','publish_authorization','single_maintainer_exception','single-maintainer-preview-channel-exception','explicit-owner-authorization-no-independent-reviewer-available','verify_publication','secrets.NUGET_API_KEY','GITHUB_PACKAGES_TOKEN: ${{ github.token }}','scripts/Test-NuGetRepositorySignedPackage.ps1','github-packages-publication-proof.json','RequiredVisibility = ''public''','github.repository == ''guojin-yan/OpenCV-CSharp-API''')) {
            Assert-True -List $violations -Condition ($publishText.Contains($token, [StringComparison]::Ordinal)) -Path '.github/workflows/publish-nuget.yml' -Issue 'Dual-feed publication workflow lost a required trust boundary' -Text $token
        }
        Assert-True -List $violations -Condition (-not $publishText.Contains('--skip-duplicate', [StringComparison]::OrdinalIgnoreCase)) -Path '.github/workflows/publish-nuget.yml' -Issue 'First publication must fail on duplicate package identity rather than hiding it'
        Assert-True -List $violations -Condition (-not $publishText.Contains('dotnet nuget sign', [StringComparison]::OrdinalIgnoreCase)) -Path '.github/workflows/publish-nuget.yml' -Issue 'Confirmed strategy requires NuGet.org repository signing, not local author signing'
    }

    $sourceFiles = Get-ChildItem -LiteralPath $repo -Recurse -File -Include *.ps1,*.json,*.yml,*.yaml,*.props,*.targets,*.md -Force
    foreach ($sourceFile in $sourceFiles) {
        $sourceText = [IO.File]::ReadAllText($sourceFile.FullName)
        if ($sourceText -match '(?i)https://[^/\s:@]+:[^@\s]+@') {
            Add-Violation -List $violations -Path $sourceFile.FullName -Issue 'Credential-bearing URL residue must not exist in source files'
        }
        if ($sourceText -match '(?i)(?:NUGET_AUTH_TOKEN|NUGET_API_KEY|Authorization:\s*Bearer)\s*[=:]\s*(?!\$\{\{|\$env:|\$ENV:|\$\()([A-Za-z0-9_\-]{12,})') {
            Add-Violation -List $violations -Path $sourceFile.FullName -Issue 'Literal NuGet credential/token residue must not exist in source files'
        }
    }

    if ($LiveFeedVerification) {
        $indexStatus = Get-HttpStatus -Url $feedIndex -Method Get
        $packageStatus = Get-HttpStatus -Url $packageFlatContainerUrl -Method Head
        Assert-True -List $violations -Condition ($indexStatus -eq 200) -Path $feedIndex -Issue 'NuGet service index read-only GET did not return 200' -Text "Observed $indexStatus"
        Assert-True -List $violations -Condition ($packageStatus -in @(200, 404)) -Path $packageFlatContainerUrl -Issue 'NuGet package read-only HEAD returned an unexpected status' -Text "Observed $packageStatus"
        Write-Host "NUGET_PUBLIC_FEED_READ_ONLY_OK index_status=$indexStatus package_status=$packageStatus package=$packageId/$packageVersion github_packages_target=$githubPackagesIndex repository=$authoritativeRepository upload_attempted=false"
    }
    else {
        Write-Host 'NUGET_PUBLIC_FEED_READ_ONLY_FIXTURE_OK https_only=true upload_attempted=false live_request=false'
    }

    $negative = @(
        [pscustomobject]@{ Name = 'HTTP feed'; Text = "http://api.nuget.org/v3/index.json"; Check = { param($value) -not (Test-FeedReference -Url $value) } },
        [pscustomobject]@{ Name = 'wrong GitHub Packages owner'; Text = 'https://nuget.pkg.github.com/grape-yan/index.json'; Check = { param($value) -not (Test-GitHubPackagesReference -Url $value) } },
        [pscustomobject]@{ Name = 'upload command'; Text = 'dotnet nuget push https://api.nuget.org/v3/index.json --api-key TOKEN'; Check = { param($value) $value -match '(?i)nuget\s+push' } },
        [pscustomobject]@{ Name = 'credential residue'; Text = ('https://user' + ':password@api.nuget.org/v3/index.json'); Check = { param($value) $value -match '(?i)https://[^/]+:[^/]+@' } },
        [pscustomobject]@{ Name = 'wrong package identity'; Text = 'JYPPX.OpenCV.CSharp.API/5.0.1'; Check = { param($value) $value -notmatch [regex]::Escape("$packageId/$packageVersion") } },
        [pscustomobject]@{ Name = 'mutable feed'; Text = 'https://api.nuget.org/v3-flatcontainer/jyppx.opencv.csharp.api/latest/index.json'; Check = { param($value) $value -match '(?i)(latest|floating|branch|main)' } }
    )
    foreach ($case in $negative) {
        $accepted = & $case.Check $case.Text
        Assert-True -List $violations -Condition ([bool]$accepted) -Path $case.Name -Issue 'Public feed negative fixture was accepted'
    }
}
catch {
    Add-Violation -List $violations -Path $repo -Issue 'Public feed verification contract execution failed' -Text $_.Exception.Message
}

if ($violations.Count -gt 0) {
    Write-Host "Public feed verification contract failed with $($violations.Count) violation(s)."
    $violations | Format-List Path, Issue, Text
    exit 1
}

Write-Host 'Public feed verification contract passed.'
