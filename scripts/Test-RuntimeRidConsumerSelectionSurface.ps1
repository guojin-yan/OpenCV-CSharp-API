param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$managedPackageId = "JYPPX.OpenCV.CSharp.API"
$runtimePackagePrefix = "JYPPX.OpenCV.runtime"
$runtimePackageShape = "$runtimePackagePrefix.<rid>"
$runtimeMiniPackageShape = "$runtimePackagePrefix.<rid>.mini"
$currentWindowsRuntimePackage = "$runtimePackagePrefix.win-x64"
$examplePackageVersion = "5.0.0"
$preferredRuntimeProperty = "OpenCvNativeRuntimeDir"

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
        throw "Required runtime RID consumer selection file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Test-ContainsText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Needle
    )

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
        [string]$Issue
    )

    if (-not (Test-ContainsText -Text $Text -Needle $Needle)) {
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

$readmePath = "README.md"
$contributingPath = "CONTRIBUTING.md"
$quickStartPath = "docs/articles/quick-start.md"
$linkedRuntimeBuildGuidePath = "docs/articles/linked-runtime-build-guide.md"
$linkedRuntimeSmokeGuidePath = "docs/articles/linked-runtime-smoke-guide.md"
$smokeProfilesGuidePath = "docs/articles/smoke-profiles-guide.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"
$bugTemplatePath = ".github/ISSUE_TEMPLATE/bug_report.yml"
$runtimeReadmePath = "packaging/runtime/JYPPX.OpenCV.runtime/README.md"
$sampleProjectPath = "samples/ConsoleSamples/ConsoleSamples.csproj"
$testProjectPath = "tests/OpenCvSharp.Tests/OpenCvSharp.Tests.csproj"

$readmeText = Read-RequiredText -RelativePath $readmePath
$contributingText = Read-RequiredText -RelativePath $contributingPath
$quickStartText = Read-RequiredText -RelativePath $quickStartPath
$linkedRuntimeBuildGuideText = Read-RequiredText -RelativePath $linkedRuntimeBuildGuidePath
$linkedRuntimeSmokeGuideText = Read-RequiredText -RelativePath $linkedRuntimeSmokeGuidePath
$smokeProfilesGuideText = Read-RequiredText -RelativePath $smokeProfilesGuidePath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath
$bugTemplateText = Read-RequiredText -RelativePath $bugTemplatePath
$runtimeReadmeText = Read-RequiredText -RelativePath $runtimeReadmePath
$sampleProjectText = Read-RequiredText -RelativePath $sampleProjectPath
$testProjectText = Read-RequiredText -RelativePath $testProjectPath

Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle "dotnet add package $managedPackageId" -Issue "Quick Start must install the neutral stable managed package without a hardcoded version"
Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle "dotnet add package $currentWindowsRuntimePackage" -Issue "Quick Start may keep win-x64 only as the current stable Windows x64 runtime package example without a hardcoded version"
Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle $runtimePackageShape -Issue "Quick Start must describe generic runtime package selection as JYPPX.OpenCV.runtime.<rid>"
Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle $runtimeMiniPackageShape -Issue "Quick Start must describe mini runtime package selection as JYPPX.OpenCV.runtime.<rid>.mini"
Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle "target RID" -Issue "Quick Start must tell consumers to choose the runtime package for their target RID"
Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle "no matching" -Issue "Quick Start must keep no-matching-runtime-package fallback guidance visible"
Assert-Matches -Violations $violations -Path $quickStartPath -Text $quickStartText -Pattern "win-x64.*example|example.*win-x64" -Issue "Quick Start must label win-x64 as an example RID"
Assert-Contains -Violations $violations -Path $quickStartPath -Text $quickStartText -Needle "same resolved NuGet version" -Issue "Quick Start must explain managed/runtime package version alignment"

$installRegex = [System.Text.RegularExpressions.Regex]::new(
    "^\s*dotnet\s+add\s+package\s+(?<PackageId>\S+)(?:\s+#.*)?\s*$",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase -bor [System.Text.RegularExpressions.RegexOptions]::Multiline)
$installMatches = @($installRegex.Matches($quickStartText))
if ($installMatches.Count -ne 2) {
    Add-Violation -Violations $violations -Path $quickStartPath -Issue "Quick Start must contain exactly two version-free prerelease install commands: managed and current Windows x64 runtime example"
}
else {
    $packageIds = @($installMatches | ForEach-Object { $_.Groups["PackageId"].Value })
    if ($packageIds -notcontains $managedPackageId -or $packageIds -notcontains $currentWindowsRuntimePackage) {
        Add-Violation -Violations $violations -Path $quickStartPath -Issue "Quick Start install commands must use neutral managed package and current Windows x64 runtime package example"
    }
}
if ($quickStartText -match "(?im)^\s*dotnet\s+add\s+package\s+\S+.*\s--version(?:\s|=)") {
    Add-Violation -Violations $violations -Path $quickStartPath -Issue "Quick Start install commands must not hardcode package versions"
}

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $contributingPath; Text = $contributingText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText },
        [pscustomobject]@{ Path = $linkedRuntimeSmokeGuidePath; Text = $linkedRuntimeSmokeGuideText },
        [pscustomobject]@{ Path = $smokeProfilesGuidePath; Text = $smokeProfilesGuideText },
        [pscustomobject]@{ Path = $versionNeutralGuidePath; Text = $versionNeutralGuideText },
        [pscustomobject]@{ Path = $bugTemplatePath; Text = $bugTemplateText },
        [pscustomobject]@{ Path = $runtimeReadmePath; Text = $runtimeReadmeText })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $runtimePackageShape -Issue "$($doc.Path) must keep generic runtime package shape visible to consumers"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $runtimeMiniPackageShape -Issue "$($doc.Path) must keep mini runtime package shape visible to consumers"
}

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $contributingPath; Text = $contributingText },
        [pscustomobject]@{ Path = $linkedRuntimeSmokeGuidePath; Text = $linkedRuntimeSmokeGuideText },
        [pscustomobject]@{ Path = $smokeProfilesGuidePath; Text = $smokeProfilesGuideText },
        [pscustomobject]@{ Path = $bugTemplatePath; Text = $bugTemplateText })) {
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "target RID" -Issue "$($doc.Path) must tell consumers to choose the runtime package for their target RID"
    Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle "profile" -Issue "$($doc.Path) must keep runtime profile selection visible"
}

Assert-Contains -Violations $violations -Path $bugTemplatePath -Text $bugTemplateText -Needle "current Windows x64 example" -Issue "Bug template must label the runtime package placeholder as the current Windows x64 example"
Assert-Contains -Violations $violations -Path $bugTemplatePath -Text $bugTemplateText -Needle "placeholder: $currentWindowsRuntimePackage $examplePackageVersion" -Issue "Bug template may keep win-x64 only as the current Windows x64 placeholder"

Assert-Contains -Violations $violations -Path $contributingPath -Text $contributingText -Needle "current Windows x64 example" -Issue "CONTRIBUTING must require current-example context for win-x64 install snippets"
Assert-Contains -Violations $violations -Path $contributingPath -Text $contributingText -Needle "not the only supported runtime package" -Issue "CONTRIBUTING must reject win-x64-only consumer install guidance"

foreach ($project in @(
        [pscustomobject]@{ Path = $sampleProjectPath; Text = $sampleProjectText; Kind = "sample" },
        [pscustomobject]@{ Path = $testProjectPath; Text = $testProjectText; Kind = "test" })) {
    Assert-Contains -Violations $violations -Path $project.Path -Text $project.Text -Needle $preferredRuntimeProperty -Issue "$($project.Kind) project must prefer the neutral runtime copy property"
}

$consumerRidFiles = @(
    $readmePath,
    $contributingPath,
    $quickStartPath,
    $linkedRuntimeBuildGuidePath,
    $linkedRuntimeSmokeGuidePath,
    $smokeProfilesGuidePath,
    $bugTemplatePath,
    $runtimeReadmePath
)
$winPackageRegex = [System.Text.RegularExpressions.Regex]::new(
    "JYPPX\.OpenCV\.runtime\.win-x64",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
$currentExampleContextRegex = [System.Text.RegularExpressions.Regex]::new(
    "current|example|default|placeholder|Windows x64|concrete|当前|示例|默认|具体|占位",
    [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)

foreach ($relativePath in $consumerRidFiles) {
    $path = Join-Path $repo $relativePath
    $text = [System.IO.File]::ReadAllText($path)
    if ($winPackageRegex.IsMatch($text) -and -not $currentExampleContextRegex.IsMatch($text)) {
        Add-Violation -Violations $violations -Path $relativePath -Issue "win-x64 runtime package mentions must be labelled as current Windows x64 examples/defaults/placeholders"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Runtime RID consumer selection surface guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Line, Issue |
        Format-Table Path, Line, Issue, Text -AutoSize
    exit 1
}

Write-Host "Runtime RID consumer selection surface guard passed."
Write-Host "Consumer RID files checked: $($consumerRidFiles.Count)."
Write-Host "Runtime package shape: $runtimePackageShape."
Write-Host "Mini runtime package shape: $runtimeMiniPackageShape."
