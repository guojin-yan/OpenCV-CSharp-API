param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$violations = [System.Collections.Generic.List[object]]::new()

function Read-RequiredText {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required targeted pack verification file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Assert-Contains {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Needle,
        [Parameter(Mandatory = $true)][string]$Issue
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        $violations.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Needle })
    }
}

function Assert-NotContains {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Needle,
        [Parameter(Mandatory = $true)][string]$Issue
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
        $violations.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Needle })
    }
}

function Assert-Matches {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Pattern,
        [Parameter(Mandatory = $true)][string]$Issue
    )

    if (-not [System.Text.RegularExpressions.Regex]::IsMatch(
            $Text,
            $Pattern,
            [System.Text.RegularExpressions.RegexOptions]::Singleline)) {
        $violations.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Pattern })
    }
}

$workflowPath = ".github/workflows/pack.yml"
$artifactGuardPath = "scripts/Test-GitHubPackArtifactMatrixSurface.ps1"
$consumerGuardPath = "scripts/Test-GitHubPackConsumerRestoreSurface.ps1"
$cmakePath = "src/OpenCvSharp.Native/CMakeLists.txt"
$readmePath = "README.md"
$guidePath = "docs/articles/linked-runtime-build-guide.md"

$workflowText = Read-RequiredText $workflowPath
$artifactGuardText = Read-RequiredText $artifactGuardPath
$consumerGuardText = Read-RequiredText $consumerGuardPath
$cmakeText = Read-RequiredText $cmakePath
$readmeText = Read-RequiredText $readmePath
$guideText = Read-RequiredText $guidePath

foreach ($expectation in @(
        @($workflowPath, $workflowText, "verify-targeted-real-mini:", "Pack workflow must keep the targeted real mini verification job"),
        @($workflowPath, $workflowText, "inputs.rid == 'ubuntu.24.04-x64' && inputs.runtime_profile == 'mini' && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Targeted verification must be limited to the proven non-synthetic non-publishing target"),
        @($workflowPath, $workflowText, "name: nupkg-managed", "Targeted verification must download the same-run managed artifact explicitly"),
        @($workflowPath, $workflowText, "name: nupkg-ubuntu.24.04-x64-mini", "Targeted verification must download the selected runtime artifact explicitly"),
        @($workflowPath, $workflowText, "path: artifacts/pack-targeted/nupkg-managed", "Targeted managed artifact must use an isolated exact path"),
        @($workflowPath, $workflowText, "path: artifacts/pack-targeted/nupkg-ubuntu.24.04-x64-mini", "Targeted runtime artifact must use an isolated exact path"),
        @($workflowPath, $workflowText, "-ExpectedSyntheticRuntimeInputs false", "Targeted artifact and consumer checks must require real provenance"),
        @($workflowPath, $workflowText, "-SelectedRid ubuntu.24.04-x64", "Targeted checks must select the proven distro RID"),
        @($workflowPath, $workflowText, "-SelectedRuntimeProfile mini", "Targeted checks must select mini explicitly"),
        @($workflowPath, $workflowText, "-RunNativeSmoke", "Targeted consumer verification must execute native calls"),
        @($workflowPath, $workflowText, "inputs.rid == 'all' && inputs.runtime_profile == 'all'", "Full-matrix artifact and restore verification condition must remain"),
        @($artifactGuardPath, $artifactGuardText, '[string]$SelectedRid = ""', "Artifact guard must support an explicit selected RID"),
        @($artifactGuardPath, $artifactGuardText, '[string]$SelectedRuntimeProfile = ""', "Artifact guard must support an explicit selected profile"),
        @($artifactGuardPath, $artifactGuardText, "Targeted runtime package native payload must exactly match", "Artifact guard must require an exact selected payload"),
        @($artifactGuardPath, $artifactGuardText, "Targeted runtime provenance files must exactly match", "Artifact guard must match provenance files to package files"),
        @($artifactGuardPath, $artifactGuardText, '"libopencv_$module.so"', "Real Linux artifact verification must require the unversioned loader name"),
        @($artifactGuardPath, $artifactGuardText, '"libopencv_$module.so.$openCvBinarySuffix"', "Real Linux artifact verification must require the ABI SONAME"),
        @($artifactGuardPath, $artifactGuardText, '"libopencv_$module.so.$expectedOpenCvVersion"', "Real Linux artifact verification must require the full-version SONAME companion"),
        @($consumerGuardPath, $consumerGuardText, '[switch]$RunNativeSmoke', "Consumer guard must expose native execution only as an explicit mode"),
        @($consumerGuardPath, $consumerGuardText, "CompileNativeSmoke and RunNativeSmoke require one selected non-synthetic RID/profile package pair", "Consumer guard must reject broad or synthetic native smoke compilation/execution"),
        @($consumerGuardPath, $consumerGuardText, "<clear />", "Consumer restore must clear external NuGet sources"),
        @($consumerGuardPath, $consumerGuardText, '$env:NUGET_PACKAGES = $nugetPackagesDir', "Consumer restore must isolate the global package cache"),
        @($consumerGuardPath, $consumerGuardText, "TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio", "Consumer must execute every supported wrapper module"),
        @($consumerGuardPath, $consumerGuardText, "NATIVE_LOADER_OR_SONAME_MISSING", "Consumer diagnostics must distinguish loader or SONAME failure"),
        @($consumerGuardPath, $consumerGuardText, "SUPPORTED_MINI_ENTRYPOINT_MISSING", "Consumer diagnostics must distinguish a missing supported entrypoint"),
        @($consumerGuardPath, $consumerGuardText, "-EchoOutputOnSuccess", "Successful targeted native smoke output must remain visible in GitHub logs"),
        @($consumerGuardPath, $consumerGuardText, '"run",', "Consumer guard must execute the restored package application"),
        @($cmakePath, $cmakeText, "BUILD_WITH_INSTALL_RPATH TRUE", "Linux loader must use package RPATH in producer output"),
        @($cmakePath, $cmakeText, 'INSTALL_RPATH "\$ORIGIN"', "Linux loader must resolve adjacent packaged dependencies"),
        @($cmakePath, $cmakeText, 'target_link_options(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE "LINKER:--no-as-needed")', "Linux mini loader must retain the complete six-module closure as direct dependencies"),
        @($readmePath, $readmeText, "exact 20-file six-module payload", "README must document the targeted real payload verification"),
        @($guidePath, $guideText, "exact 20-file six-module SONAME payload", "Linked runtime guide must document the targeted real payload verification"))) {
    Assert-Contains -Path $expectation[0] -Text $expectation[1] -Needle $expectation[2] -Issue $expectation[3]
}

Assert-NotContains `
    -Path $consumerGuardPath `
    -Text $consumerGuardText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "Packaged native consumer must not mask loader RUNPATH defects with an environment override"

Assert-Matches `
    -Path $cmakePath `
    -Text $cmakeText `
    -Pattern 'list\s*\(\s*APPEND\s+OPENCV_CSHARP_NATIVE_RUNTIME_TESTS\s+\$\{OPENCV_CSHARP_NATIVE_ABI_EXPORT_TEST\}\s*\).*LD_LIBRARY_PATH=\$\{OPENCV_CSHARP_OPENCV_RUNTIME_DIRECTORY\}' `
    -Issue "Producer Linux runtime environment must be applied after the ABI export audit joins the runtime test list"

if ($violations.Count -gt 0) {
    Write-Host "Targeted real pack consumer verification surface guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Issue | Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Targeted real pack consumer verification surface guard passed."
Write-Host "Target: ubuntu.24.04-x64 / mini / non-synthetic / non-publishing."
Write-Host "Packaged native smoke modules: core,imgproc,imgcodecs,videoio."
