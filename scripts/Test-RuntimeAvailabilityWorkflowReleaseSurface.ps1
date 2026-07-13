param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$runtimePackageShape = "$runtimePackagePrefix.<rid>"
$runtimeMiniPackageShape = "$runtimePackagePrefix.<rid>.mini"
$aggregateScript = "scripts/Test-ProjectInvariants.ps1"

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [int]$Line = 0,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Read-RequiredText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$RelativePath
    )

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required runtime workflow/release surface file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Assert-Contains {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [switch]$NormalizeSlashes
    )

    if ($NormalizeSlashes) {
        $Text = $Text.Replace("\", "/")
        $Needle = $Needle.Replace("\", "/")
    }

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue
    }
}

function Assert-NotContains {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue
    }
}

$violations = [System.Collections.Generic.List[object]]::new()

$packWorkflowPath = ".github/workflows/pack.yml"
$buildNativeWorkflowPath = ".github/workflows/build-native.yml"
$buildManagedWorkflowPath = ".github/workflows/build-managed.yml"
$docsWorkflowPath = ".github/workflows/docs.yml"
$prTemplatePath = ".github/pull_request_template.md"
$bugTemplatePath = ".github/ISSUE_TEMPLATE/bug_report.yml"
$featureTemplatePath = ".github/ISSUE_TEMPLATE/feature_request.yml"
$releaseYamlPath = ".github/release.yml"
$readmePath = "README.md"
$contributingPath = "CONTRIBUTING.md"
$linkedRuntimeBuildGuidePath = "docs/articles/linked-runtime-build-guide.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"
$releasePackageGuardPath = "scripts/Test-ReleasePackageArtifactSurface.ps1"
$availabilityGuardPath = "scripts/Test-RuntimePackageAvailabilityFallbackGuidance.ps1"

$packWorkflowText = Read-RequiredText -RelativePath $packWorkflowPath
$buildNativeWorkflowText = Read-RequiredText -RelativePath $buildNativeWorkflowPath
$buildManagedWorkflowText = Read-RequiredText -RelativePath $buildManagedWorkflowPath
$docsWorkflowText = Read-RequiredText -RelativePath $docsWorkflowPath
$prTemplateText = Read-RequiredText -RelativePath $prTemplatePath
$bugTemplateText = Read-RequiredText -RelativePath $bugTemplatePath
$featureTemplateText = Read-RequiredText -RelativePath $featureTemplatePath
$readmeText = Read-RequiredText -RelativePath $readmePath
$contributingText = Read-RequiredText -RelativePath $contributingPath
$linkedRuntimeBuildGuideText = Read-RequiredText -RelativePath $linkedRuntimeBuildGuidePath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath
$releasePackageGuardText = Read-RequiredText -RelativePath $releasePackageGuardPath
$availabilityGuardText = Read-RequiredText -RelativePath $availabilityGuardPath

Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "rid:" -Issue "Pack workflow must expose a runtime identifier input"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "runtime_profile:" -Issue "Pack workflow must expose runtime profile input"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "default: all" -Issue "Pack workflow must default to the full configured RID/profile matrix"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "'-Rid', '`${{ matrix.rid }}'" -Issue "Pack workflow must forward the matrix RID to Pack-Runtime"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "'-RuntimeProfile', '`${{ matrix.profile }}'" -Issue "Pack workflow must forward the matrix runtime profile to Pack-Runtime"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "validate_synthetic_runtime" -Issue "Pack workflow must expose synthetic runtime package-surface validation"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "Reject synthetic publish" -Issue "Pack workflow must prevent publishing synthetic runtime package validation artifacts"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "strategy:" -Issue "Pack workflow must run a runtime matrix"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "name: nupkg-" -Issue "Pack workflow artifact names must stay neutral and matrix-scoped"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "path: artifacts/packages/*.nupkg" -Issue "Pack workflow must upload neutral package output artifacts"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "dotnet nuget push ./artifacts/packages/*.nupkg" -Issue "Pack workflow publish step must push from neutral package output root"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle $aggregateScript -Issue "Pack workflow must run aggregate invariant checks before pack work" -NormalizeSlashes

foreach ($workflow in @(
        [pscustomobject]@{ Path = $buildNativeWorkflowPath; Text = $buildNativeWorkflowText },
        [pscustomobject]@{ Path = $buildManagedWorkflowPath; Text = $buildManagedWorkflowText },
        [pscustomobject]@{ Path = $docsWorkflowPath; Text = $docsWorkflowText })) {
    Assert-Contains -Violations $violations -Path $workflow.Path -Text $workflow.Text -Needle $aggregateScript -Issue "$($workflow.Path) must run aggregate invariant checks" -NormalizeSlashes
}

Assert-Contains -Violations $violations -Path $buildNativeWorkflowPath -Text $buildNativeWorkflowText -Needle "matrix:" -Issue "Build-native may use an OS matrix only for native ABI validation"
Assert-Contains -Violations $violations -Path $buildNativeWorkflowPath -Text $buildNativeWorkflowText -Needle "os:" -Issue "Build-native matrix must be OS-scoped, not runtime-package RID-scoped"
Assert-NotContains -Violations $violations -Path $buildNativeWorkflowPath -Text $buildNativeWorkflowText -Needle $runtimePackagePrefix -Issue "Build-native workflow must not advertise runtime package release IDs"

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $contributingPath; Text = $contributingText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "pack workflow" -Issue "$($doc.Path) must document pack workflow RID/release behavior"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "multi-RID" -Issue "$($doc.Path) must document the active multi-RID runtime matrix"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "full/mini" -Issue "$($doc.Path) must document full/mini runtime profiles"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "synthetic" -Issue "$($doc.Path) must document synthetic package-surface validation boundary"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "artifacts/packages" -Issue "$($doc.Path) must keep package output rooted under artifacts/packages"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "nupkg" -Issue "$($doc.Path) must keep workflow artifact labels neutral"
}

Assert-Contains -Violations $violations -Path $versionNeutralGuidePath -Text $versionNeutralGuideText -Needle "Test-RuntimeAvailabilityWorkflowReleaseSurface.ps1" -Issue "Version-neutral naming guide must list the workflow/release availability guard"
Assert-Contains -Violations $violations -Path $releasePackageGuardPath -Text $releasePackageGuardText -Needle "name: `$uploadArtifactName" -Issue "Release package artifact guard must keep upload artifact labels neutral"
Assert-Contains -Violations $violations -Path $availabilityGuardPath -Text $availabilityGuardText -Needle "runtime package matrix" -Issue "Availability/fallback guard must track the runtime package matrix"

if (Test-Path -LiteralPath (Join-Path $repo $releaseYamlPath) -PathType Leaf) {
    $releaseYamlText = Read-RequiredText -RelativePath $releaseYamlPath
    Assert-Contains -Violations $violations -Path $releaseYamlPath -Text $releaseYamlText -Needle "artifacts/packages" -Issue "Release configuration must use neutral package output root"
    Assert-Contains -Violations $violations -Path $releaseYamlPath -Text $releaseYamlText -Needle "nupkg" -Issue "Release configuration package labels must stay neutral"
}

$githubFiles = @(
    $packWorkflowPath,
    $buildNativeWorkflowPath,
    $buildManagedWorkflowPath,
    $docsWorkflowPath,
    $prTemplatePath,
    $bugTemplatePath,
    $featureTemplatePath
)
if (Test-Path -LiteralPath (Join-Path $repo $releaseYamlPath) -PathType Leaf) {
    $githubFiles += $releaseYamlPath
}

$fixedRidArtifactRegex = [System.Text.RegularExpressions.Regex]::new(
    "(?:artifact|name|path|upload|release)[^\r\n]*(?:runtime\.win-x64|win-x64-nupkg|nupkg-win-x64)",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

foreach ($relativePath in $githubFiles) {
    $path = Join-Path $repo $relativePath
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($path)) {
        $lineNumber++
        if ($fixedRidArtifactRegex.IsMatch($line)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "Workflow/release artifact labels must not encode fixed RID package identities" `
                -Text $line
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime availability workflow/release surface guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "Runtime availability workflow/release surface guard passed."
Write-Host "Runtime package shape: $runtimePackageShape."
Write-Host "Mini runtime package shape: $runtimeMiniPackageShape."
Write-Host "Workflow/release GitHub files checked: $($githubFiles.Count)."
