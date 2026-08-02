param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$managedPackageId = "JYPPX.OpenCV.CSharp.API"
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$exampleRuntimePackageId = "$runtimePackagePrefix.win-x64"
$examplePackageVersion = "5.0.0-preview.1"
$preferredRuntimeProperty = "OpenCvNativeRuntimeDir"
$compatibilityRuntimeProperty = "Open" + "Cv5SharpNativeRuntimeDir"
$fixedMajorManagedIdentity = "Open" + "Cv5Sharp"
$fixedMajorRuntimeIdentity = $fixedMajorManagedIdentity + "\.runtime"
$fixedMajorRuntimeIdentityLower = "opencv" + "5sharp\.runtime"

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
        throw "Required package install/consumer surface file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Test-ContainsText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle,
        [switch]$NormalizeSlashes
    )

    if ($NormalizeSlashes) {
        $Text = $Text.Replace("\", "/")
        $Needle = $Needle.Replace("\", "/")
    }

    return $Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
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

    if (-not (Test-ContainsText -Text $Text -Needle $Needle -NormalizeSlashes:$NormalizeSlashes)) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue
    }
}

function Assert-Matches {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Pattern,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    if ($Text -notmatch $Pattern) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue
    }
}

$violations = [System.Collections.Generic.List[object]]::new()

$quickStartPath = "docs/articles/quick-start.md"
$linkedRuntimeBuildGuidePath = "docs/articles/linked-runtime-build-guide.md"
$linkedRuntimeSmokeGuidePath = "docs/articles/linked-runtime-smoke-guide.md"
$smokeProfilesGuidePath = "docs/articles/smoke-profiles-guide.md"
$runtimeReadmePath = "packaging/runtime/JYPPX.OpenCV.runtime/README.md"
$readmePath = "README.md"
$contributingPath = "CONTRIBUTING.md"
$bugTemplatePath = ".github/ISSUE_TEMPLATE/bug_report.yml"
$featureTemplatePath = ".github/ISSUE_TEMPLATE/feature_request.yml"
$sampleProjectPath = "samples/ConsoleSamples/ConsoleSamples.csproj"
$testProjectPath = "tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj"

$quickStartText = Read-RequiredText -RelativePath $quickStartPath
$linkedRuntimeBuildGuideText = Read-RequiredText -RelativePath $linkedRuntimeBuildGuidePath
$linkedRuntimeSmokeGuideText = Read-RequiredText -RelativePath $linkedRuntimeSmokeGuidePath
$smokeProfilesGuideText = Read-RequiredText -RelativePath $smokeProfilesGuidePath
$runtimeReadmeText = Read-RequiredText -RelativePath $runtimeReadmePath
$readmeText = Read-RequiredText -RelativePath $readmePath
$contributingText = Read-RequiredText -RelativePath $contributingPath
$bugTemplateText = Read-RequiredText -RelativePath $bugTemplatePath
$featureTemplateText = Read-RequiredText -RelativePath $featureTemplatePath
$sampleProjectText = Read-RequiredText -RelativePath $sampleProjectPath
$testProjectText = Read-RequiredText -RelativePath $testProjectPath

Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle "dotnet add package $managedPackageId --version $examplePackageVersion" -Issue "Quick Start must install the neutral managed preview package"
Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle "dotnet add package $exampleRuntimePackageId --version $examplePackageVersion" -Issue "Quick Start must install the matching neutral runtime preview package"
Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle "same normalized NuGet package version" -Issue "Quick Start must explain managed/runtime package version alignment"
Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle "package IDs and public namespace stay version-neutral" -Issue "Quick Start must state package IDs and namespace stay version-neutral"

$installRegex = [System.Text.RegularExpressions.Regex]::new(
    "dotnet\s+add\s+package\s+(?<PackageId>\S+)\s+--version\s+(?<Version>\d+\.\d+\.\d+(?:\.\d+)?(?:-[0-9a-z-]+(?:\.[0-9a-z-]+)*)?)",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$installMatches = @($installRegex.Matches($quickStartText))
if ($installMatches.Count -ne 2) {
    Add-Violation -Violations $violations -Path $quickStartPath -Issue "Quick Start must contain exactly two dotnet add package commands: managed and runtime"
}
else {
    $packageIds = @($installMatches | ForEach-Object { $_.Groups["PackageId"].Value })
    $versions = @($installMatches | ForEach-Object { $_.Groups["Version"].Value })
    if ($packageIds -notcontains $managedPackageId -or $packageIds -notcontains $exampleRuntimePackageId) {
        Add-Violation -Violations $violations -Path $quickStartPath -Issue "Quick Start install commands must use neutral managed/runtime package IDs"
    }

    if (@($versions | Sort-Object -Unique).Count -ne 1) {
        Add-Violation -Violations $violations -Path $quickStartPath -Issue "Quick Start managed/runtime install commands must use the same package version metadata"
    }
}

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText },
        [pscustomobject]@{ Path = $linkedRuntimeSmokeGuidePath; Text = $linkedRuntimeSmokeGuideText },
        [pscustomobject]@{ Path = $smokeProfilesGuidePath; Text = $smokeProfilesGuideText })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $managedPackageId -Issue "$($doc.Path) must name the neutral managed package"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "$runtimePackagePrefix" -Issue "$($doc.Path) must name the neutral runtime package prefix"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "normalized NuGet package version" -Issue "$($doc.Path) must document matching consumer package versions"
}

Assert-Contains -Violations $violations -Path $linkedRuntimeSmokeGuidePath -Text $linkedRuntimeSmokeGuideText -Needle $preferredRuntimeProperty -Issue "Linked runtime smoke guide must prefer the neutral runtime copy property"
Assert-Contains -Violations $violations -Path $linkedRuntimeSmokeGuidePath -Text $linkedRuntimeSmokeGuideText -Needle $compatibilityRuntimeProperty -Issue "Linked runtime smoke guide must document the existing compatibility runtime copy property"
Assert-Contains -Violations $violations -Path $linkedRuntimeSmokeGuidePath -Text $linkedRuntimeSmokeGuideText -Needle "compatibility alias" -Issue "Linked runtime smoke guide must label the fixed-major runtime property as compatibility-only"

Assert-Contains -Violations $violations -Path $runtimeReadmePath -Text $runtimeReadmeText -Needle "The package ID is version-neutral" -Issue "Runtime package README must state package ID neutrality"
Assert-Contains -Violations $violations -Path $runtimeReadmePath -Text $runtimeReadmeText -Needle "package version metadata" -Issue "Runtime package README must describe runtime identity as version metadata"
Assert-Contains -Violations $violations -Path $runtimeReadmePath -Text $runtimeReadmeText -Needle $runtimePackagePrefix -Issue "Runtime package README must name the neutral runtime package prefix"

Assert-Contains -Violations $violations -Path $bugTemplatePath -Text $bugTemplateText -Needle "Version of $managedPackageId." -Issue "Bug template must ask for neutral managed package version"
Assert-Contains -Violations $violations -Path $bugTemplatePath -Text $bugTemplateText -Needle "placeholder: $managedPackageId $examplePackageVersion" -Issue "Bug template managed package placeholder must use neutral package ID"
Assert-Contains -Violations $violations -Path $bugTemplatePath -Text $bugTemplateText -Needle "placeholder: $exampleRuntimePackageId $examplePackageVersion" -Issue "Bug template runtime package placeholder must use neutral runtime package ID"

Assert-Contains -Violations $violations -Path $contributingPath -Text $contributingText -Needle 'runtime package IDs stay `JYPPX.OpenCV.runtime.<rid>`' -Issue "CONTRIBUTING must keep runtime install/package identity neutral"
Assert-Contains -Violations $violations -Path $contributingPath -Text $contributingText -Needle "fixed-major names in consumer-facing files must be explicitly labelled as compatibility or legacy aliases" -Issue "CONTRIBUTING must require compatibility context for fixed-major consumer names"

foreach ($project in @(
        [pscustomobject]@{ Path = $sampleProjectPath; Text = $sampleProjectText; Kind = "sample" },
        [pscustomobject]@{ Path = $testProjectPath; Text = $testProjectText; Kind = "test" })) {
    Assert-Contains -Violations $violations -Path $project.Path -Text $project.Text -Needle '..\..\src\OpenCvSharp\OpenCvSharp.csproj' -Issue "$($project.Kind) project must use the neutral local managed project reference"
    Assert-Contains -Violations $violations -Path $project.Path -Text $project.Text -Needle $preferredRuntimeProperty -Issue "$($project.Kind) project must prefer the neutral runtime copy property"
    Assert-Contains -Violations $violations -Path $project.Path -Text $project.Text -Needle $compatibilityRuntimeProperty -Issue "$($project.Kind) project must preserve the existing compatibility runtime copy alias"
    Assert-Contains -Violations $violations -Path $project.Path -Text $project.Text -Needle "compatibility alias" -Issue "$($project.Kind) project must label the fixed-major runtime copy property as compatibility-only"
}

$retiredNamespacePattern = '(?:^|\s)(?:using|namespace)\s+' +
    $fixedMajorManagedIdentity +
    '(?:[.;\s{])'
$fixedMajorInstallPattern = "dotnet\s+add\s+package\s+" +
    $fixedMajorManagedIdentity +
    "\b"
$fixedMajorPackageReferencePattern = "Package" +
    "Reference[^\r\n]*" +
    $fixedMajorManagedIdentity
$activeLeakPattern = [string]::Join('|', [string[]]@(
    $retiredNamespacePattern,
    $fixedMajorInstallPattern,
    $fixedMajorPackageReferencePattern,
    $fixedMajorRuntimeIdentity,
    $fixedMajorRuntimeIdentityLower
))
$activeLeakRegex = [System.Text.RegularExpressions.Regex]::new(
    $activeLeakPattern,
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$compatibilityNameRegex = [System.Text.RegularExpressions.Regex]::new(
    "$fixedMajorManagedIdentity|opencv" + "5sharp",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$compatibilityContextRegex = [System.Text.RegularExpressions.Regex]::new(
    "compatib|legacy|existing|already-compiled|kept stable|explicit|alias|facade|retired|previous|兼容|既有|已编译|保留|明确|别名|旧",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

$consumerSurfaceFiles = @(
    $quickStartPath,
    $linkedRuntimeBuildGuidePath,
    $linkedRuntimeSmokeGuidePath,
    $smokeProfilesGuidePath,
    $runtimeReadmePath,
    $readmePath,
    $contributingPath,
    $bugTemplatePath,
    $featureTemplatePath,
    $sampleProjectPath,
    $testProjectPath
)

foreach ($relativePath in $consumerSurfaceFiles) {
    $path = Join-Path $repo $relativePath
    $lineNumber = 0
    foreach ($line in [System.IO.File]::ReadLines($path)) {
        $lineNumber++
        if ($activeLeakRegex.IsMatch($line)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "Install/consumer surfaces must not use fixed-major packages or namespaces as current identities" `
                -Text $line
        }

        if ($compatibilityNameRegex.IsMatch($line) -and -not $compatibilityContextRegex.IsMatch($line)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $lineNumber `
                -Issue "Fixed-major consumer-surface mentions must be explicitly compatibility-scoped" `
                -Text $line
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Package install consumer surface guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "Package install consumer surface guard passed."
Write-Host "Consumer install files checked: $($consumerSurfaceFiles.Count)."
Write-Host "Managed package ID: $managedPackageId."
Write-Host "Runtime package example: $exampleRuntimePackageId."
