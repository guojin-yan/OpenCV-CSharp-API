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

function Assert-ExactLine {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$ExpectedLine,
        [Parameter(Mandatory = $true)][string]$Issue
    )

    if (-not (($Text -split "\r?\n") -ccontains $ExpectedLine)) {
        $violations.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $ExpectedLine })
    }
}

function Assert-OccurrenceCount {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text,
        [Parameter(Mandatory = $true)][string]$Needle,
        [Parameter(Mandatory = $true)][int]$ExpectedCount,
        [Parameter(Mandatory = $true)][string]$Issue
    )

    $count = 0
    $offset = 0
    while (($offset = $Text.IndexOf($Needle, $offset, [System.StringComparison]::Ordinal)) -ge 0) {
        $count++
        $offset += $Needle.Length
    }

    if ($count -ne $ExpectedCount) {
        $violations.Add([pscustomobject]@{
            Path = $Path
            Issue = $Issue
            Text = "$Needle (expected $ExpectedCount, found $count)"
        })
    }
}

function Get-WorkflowJobText {
    param([Parameter(Mandatory = $true)][string]$JobName)

    $pattern = "(?ms)^  $([System.Text.RegularExpressions.Regex]::Escape($JobName)):\r?\n.*?(?=^  [A-Za-z0-9_-]+:\r?\n|\z)"
    $match = [System.Text.RegularExpressions.Regex]::Match($workflowText, $pattern)
    if (-not $match.Success) {
        $violations.Add([pscustomobject]@{
            Path = $workflowPath
            Issue = "Required targeted verification job was not found"
            Text = $JobName
        })
        return ""
    }

    return $match.Value
}

$workflowPath = ".github/workflows/pack.yml"
$artifactGuardPath = "scripts/Test-GitHubPackArtifactMatrixSurface.ps1"
$consumerGuardPath = "scripts/Test-GitHubPackConsumerRestoreSurface.ps1"
$cmakePath = "src/OpenCvSharp.Native/CMakeLists.txt"
$readmePath = "README.md"
$guidePath = "docs/articles/linked-runtime-build-guide.md"
$runtimeMatrixPath = "packaging/runtime/runtime-package-matrix.json"

$workflowText = Read-RequiredText $workflowPath
$artifactGuardText = Read-RequiredText $artifactGuardPath
$consumerGuardText = Read-RequiredText $consumerGuardPath
$cmakeText = Read-RequiredText $cmakePath
$readmeText = Read-RequiredText $readmePath
$guideText = Read-RequiredText $guidePath
$runtimeMatrixText = Read-RequiredText $runtimeMatrixPath
$runtimeMatrix = $runtimeMatrixText | ConvertFrom-Json
$ubuntuJobText = Get-WorkflowJobText -JobName "verify-targeted-real"
$debianJobText = Get-WorkflowJobText -JobName "verify-targeted-real-debian"
$fedoraJobText = Get-WorkflowJobText -JobName "verify-targeted-real-fedora"

foreach ($expectedTarget in @(
        [pscustomobject]@{ Rid = "ubuntu.22.04-x64"; Runner = "ubuntu-22.04" },
        [pscustomobject]@{ Rid = "ubuntu.24.04-x64"; Runner = "ubuntu-24.04" })) {
    $ridSpecs = @($runtimeMatrix.rids | Where-Object { $_.rid -eq $expectedTarget.Rid })
    if ($ridSpecs.Count -ne 1 -or [string]$ridSpecs[0].runner -ne $expectedTarget.Runner) {
        $violations.Add([pscustomobject]@{
            Path = $runtimeMatrixPath
            Issue = "Targeted native execution RID must map to its matching Ubuntu runner"
            Text = "$($expectedTarget.Rid) -> $($expectedTarget.Runner)"
        })
    }
}

foreach ($expectation in @(
        @($workflowPath, $ubuntuJobText, "verify-targeted-real:", "Pack workflow must keep the targeted real Ubuntu verification job"),
        @($workflowPath, $ubuntuJobText, "((inputs.rid == 'ubuntu.24.04-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini')) || (inputs.rid == 'ubuntu.22.04-x64' && inputs.runtime_profile == 'full')) && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Hosted targeted verification must keep the exact three-target non-synthetic non-publishing allowlist"),
        @($workflowPath, $ubuntuJobText, "runs-on: `${{ inputs.rid == 'ubuntu.22.04-x64' && 'ubuntu-22.04' || 'ubuntu-24.04' }}", "Hosted targeted verification must execute each proven RID on its matching Ubuntu runner"),
        @($workflowPath, $ubuntuJobText, "name: nupkg-managed", "Hosted targeted verification must download the same-run managed artifact explicitly"),
        @($workflowPath, $ubuntuJobText, 'name: nupkg-${{ inputs.rid }}-${{ inputs.runtime_profile }}', "Hosted targeted verification must download the exact selected RID/profile artifact"),
        @($workflowPath, $ubuntuJobText, "path: artifacts/pack-targeted/nupkg-managed", "Hosted targeted managed artifact must use an isolated exact path"),
        @($workflowPath, $ubuntuJobText, 'path: artifacts/pack-targeted/nupkg-${{ inputs.rid }}-${{ inputs.runtime_profile }}', "Hosted targeted runtime artifact must use an isolated selected RID/profile path"),
        @($workflowPath, $ubuntuJobText, '$runtimeRid = ''${{ inputs.rid }}''', "Hosted targeted guards must receive the selected RID instead of a hardcoded distro"),
        @($workflowPath, $ubuntuJobText, '$runtimeProfile = ''${{ inputs.runtime_profile }}''', "Hosted targeted guards must receive the selected profile instead of a hardcoded mini value"),
        @($workflowPath, $ubuntuJobText, "-ExpectedSyntheticRuntimeInputs false", "Hosted targeted artifact and consumer checks must require real provenance"),
        @($workflowPath, $ubuntuJobText, "-SelectedRid `$runtimeRid", "Hosted targeted checks must forward the selected proven distro RID"),
        @($workflowPath, $ubuntuJobText, "-SelectedRuntimeProfile `$runtimeProfile", "Hosted targeted checks must forward the selected proven profile"),
        @($workflowPath, $ubuntuJobText, "-RunNativeSmoke", "Hosted targeted consumer verification must execute native calls"),
        @($workflowPath, $debianJobText, "verify-targeted-real-debian:", "Pack workflow must keep a separate Debian container verification job"),
        @($workflowPath, $debianJobText, "inputs.rid == 'debian.12-x64' && inputs.runtime_profile == 'full' && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Debian verification must use the exact full-only non-synthetic non-publishing gate"),
        @($workflowPath, $debianJobText, "runs-on: ubuntu-24.04", "Debian container verification must use the supported hosted runner"),
        @($workflowPath, $debianJobText, "container: debian:12", "Debian verification must execute in the Debian 12 job container"),
        @($workflowPath, $debianJobText, "cat /etc/os-release", "Debian verification must expose container distro evidence"),
        @($workflowPath, $debianJobText, 'if [ "${ID:-}" != "debian" ]', "Debian verification must require the Debian distro identity"),
        @($workflowPath, $debianJobText, '12|12.*) ;;', "Debian verification must require Debian version 12 or 12.x"),
        @($workflowPath, $debianJobText, "getconf GNU_LIBC_VERSION", "Debian verification must record the container libc identity"),
        @($workflowPath, $debianJobText, "DEBIAN_12_CONTAINER_EVIDENCE", "Debian verification must emit an explicit container evidence marker"),
        @($workflowPath, $debianJobText, "apt-get install -y --no-install-recommends powershell", "Debian verification must install PowerShell before invoking repository guards"),
        @($workflowPath, $debianJobText, "10.0.x", "Debian verification must install .NET 10"),
        @($workflowPath, $debianJobText, "9.0.x", "Debian verification must install .NET 9"),
        @($workflowPath, $debianJobText, "8.0.x", "Debian verification must install .NET 8"),
        @($workflowPath, $debianJobText, "name: nupkg-managed", "Debian verification must download the same-run managed artifact explicitly"),
        @($workflowPath, $debianJobText, "name: nupkg-debian.12-x64-full", "Debian verification must download only the exact Debian full runtime artifact"),
        @($workflowPath, $debianJobText, "path: artifacts/pack-targeted-debian/nupkg-managed", "Debian managed artifact must use its isolated exact path"),
        @($workflowPath, $debianJobText, "path: artifacts/pack-targeted-debian/nupkg-debian.12-x64-full", "Debian runtime artifact must use its isolated exact path"),
        @($workflowPath, $debianJobText, "-ExpectedSyntheticRuntimeInputs false", "Debian artifact and consumer checks must require real provenance"),
        @($workflowPath, $debianJobText, "-SelectedRid debian.12-x64", "Debian checks must select only the proven Debian RID"),
        @($workflowPath, $debianJobText, "-SelectedRuntimeProfile full", "Debian checks must select only the full profile"),
        @($workflowPath, $debianJobText, "-RunNativeSmoke", "Debian consumer verification must execute native calls inside the container"),
        @($workflowPath, $fedoraJobText, "verify-targeted-real-fedora:", "Pack workflow must keep a separate Fedora container verification job"),
        @($workflowPath, $fedoraJobText, "inputs.rid == 'fedora.40-x64' && inputs.runtime_profile == 'full' && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Fedora verification must use the exact full-only non-synthetic non-publishing gate"),
        @($workflowPath, $fedoraJobText, "runs-on: ubuntu-24.04", "Fedora container verification must use the supported hosted runner"),
        @($workflowPath, $fedoraJobText, "container: fedora:40", "Fedora verification must execute in the Fedora 40 job container"),
        @($workflowPath, $fedoraJobText, "cat /etc/os-release", "Fedora verification must expose container distro evidence"),
        @($workflowPath, $fedoraJobText, 'if [ "${ID:-}" != "fedora" ]', "Fedora verification must require the Fedora distro identity"),
        @($workflowPath, $fedoraJobText, '40|40.*) ;;', "Fedora verification must require Fedora version 40 or 40.x"),
        @($workflowPath, $fedoraJobText, "getconf GNU_LIBC_VERSION", "Fedora verification must record the container libc identity"),
        @($workflowPath, $fedoraJobText, "FEDORA_40_CONTAINER_EVIDENCE", "Fedora verification must emit an explicit container evidence marker"),
        @($workflowPath, $fedoraJobText, "dnf install -y powershell", "Fedora verification must install PowerShell before invoking repository guards"),
        @($workflowPath, $fedoraJobText, "Microsoft publishes the compatible PowerShell RPM feed under its RHEL 9 path", "Fedora verifier must explain the factual PowerShell feed path without changing distro identity"),
        @($workflowPath, $fedoraJobText, "10.0.x", "Fedora verification must install .NET 10"),
        @($workflowPath, $fedoraJobText, "9.0.x", "Fedora verification must install .NET 9"),
        @($workflowPath, $fedoraJobText, "8.0.x", "Fedora verification must install .NET 8"),
        @($workflowPath, $fedoraJobText, "name: nupkg-managed", "Fedora verification must download the same-run managed artifact explicitly"),
        @($workflowPath, $fedoraJobText, "name: nupkg-fedora.40-x64-full", "Fedora verification must download only the exact Fedora full runtime artifact"),
        @($workflowPath, $fedoraJobText, "path: artifacts/pack-targeted-fedora/nupkg-managed", "Fedora managed artifact must use its isolated exact path"),
        @($workflowPath, $fedoraJobText, "path: artifacts/pack-targeted-fedora/nupkg-fedora.40-x64-full", "Fedora runtime artifact must use its isolated exact path"),
        @($workflowPath, $fedoraJobText, "-ExpectedSyntheticRuntimeInputs false", "Fedora artifact and consumer checks must require real provenance"),
        @($workflowPath, $fedoraJobText, "-SelectedRid fedora.40-x64", "Fedora checks must select only the proven Fedora RID"),
        @($workflowPath, $fedoraJobText, "-SelectedRuntimeProfile full", "Fedora checks must select only the full profile"),
        @($workflowPath, $fedoraJobText, "-RunNativeSmoke", "Fedora consumer verification must execute native calls inside the container"),
        @($workflowPath, $workflowText, "inputs.rid == 'all' && inputs.runtime_profile == 'all'", "Full-matrix artifact and restore verification condition must remain"),
        @($artifactGuardPath, $artifactGuardText, '[string]$SelectedRid = ""', "Artifact guard must support an explicit selected RID"),
        @($artifactGuardPath, $artifactGuardText, '[string]$SelectedRuntimeProfile = ""', "Artifact guard must support an explicit selected profile"),
        @($artifactGuardPath, $artifactGuardText, "Targeted runtime package native payload must exactly match", "Artifact guard must require an exact selected payload"),
        @($artifactGuardPath, $artifactGuardText, "Targeted runtime provenance files must exactly match", "Artifact guard must match provenance files to package files"),
        @($artifactGuardPath, $artifactGuardText, '"libopencv_$module.so"', "Real Linux artifact verification must require the unversioned loader name"),
        @($artifactGuardPath, $artifactGuardText, '"libopencv_$module.so.$openCvBinarySuffix"', "Real Linux artifact verification must require the ABI SONAME"),
        @($artifactGuardPath, $artifactGuardText, '"libopencv_$module.so.$expectedOpenCvVersion"', "Real Linux artifact verification must require the full-version SONAME companion"),
        @($artifactGuardPath, $artifactGuardText, "Runtime provenance manifest optional modules must match selected runtime profile", "Artifact guard must validate the profile's requested optional modules"),
        @($artifactGuardPath, $artifactGuardText, "Runtime provenance staged optional modules must be an ordered unique subset", "Artifact guard must constrain provenance-derived full optional modules"),
        @($artifactGuardPath, $artifactGuardText, '$expectedStagedModules += $manifestOptionalModulesStaged', "Artifact guard must combine required and actually staged optional modules for exact payload checks"),
        @($consumerGuardPath, $consumerGuardText, '[switch]$RunNativeSmoke', "Consumer guard must expose native execution only as an explicit mode"),
        @($consumerGuardPath, $consumerGuardText, "CompileNativeSmoke requires one selected RID/profile package pair", "Consumer guard must reject broad native smoke compilation"),
        @($consumerGuardPath, $consumerGuardText, "RunNativeSmoke requires one selected non-synthetic RID/profile package pair", "Consumer guard must reject broad or synthetic native execution"),
        @($consumerGuardPath, $consumerGuardText, "<clear />", "Consumer restore must clear external NuGet sources"),
        @($consumerGuardPath, $consumerGuardText, '$env:NUGET_PACKAGES = $nugetPackagesDir', "Consumer restore must isolate the global package cache"),
        @($consumerGuardPath, $consumerGuardText, "TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio", "Consumer must execute every mini wrapper module"),
        @($consumerGuardPath, $consumerGuardText, "TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,dnn profile=full", "Full consumer must execute a deterministic full-only DNN call"),
        @($consumerGuardPath, $consumerGuardText, "FULL_DNN_SMOKE_FAILED", "Full consumer must diagnose its full-only API execution separately"),
        @($consumerGuardPath, $consumerGuardText, "Consumer runtime provenance staged optional modules must be an ordered unique subset", "Consumer native asset expectations must include only allowed staged optional modules"),
        @($consumerGuardPath, $consumerGuardText, "NATIVE_LOADER_OR_SONAME_MISSING", "Consumer diagnostics must distinguish loader or SONAME failure"),
        @($consumerGuardPath, $consumerGuardText, "SUPPORTED_PROFILE_ENTRYPOINT_MISSING", "Consumer diagnostics must distinguish a missing supported profile entrypoint"),
        @($consumerGuardPath, $consumerGuardText, "-EchoOutputOnSuccess", "Successful targeted native smoke output must remain visible in GitHub logs"),
        @($consumerGuardPath, $consumerGuardText, '"run",', "Consumer guard must execute the restored package application"),
        @($cmakePath, $cmakeText, "BUILD_WITH_INSTALL_RPATH TRUE", "Linux loader must use package RPATH in producer output"),
        @($cmakePath, $cmakeText, 'INSTALL_RPATH "\$ORIGIN"', "Linux loader must resolve adjacent packaged dependencies"),
        @($cmakePath, $cmakeText, 'target_link_options(${OPENCV_CSHARP_NATIVE_TARGET} PRIVATE "LINKER:--no-as-needed")', "Linux full and mini loaders must retain their complete declared closures as direct dependencies"),
        @($readmePath, $readmeText, "matrix-required modules plus provenance-recorded staged optional modules", "README must document provenance-derived full payload verification"),
        @($readmePath, $readmeText, "Ubuntu 24.04 x64 full/mini and Ubuntu 22.04 x64 full", "README must document the exact hosted targeted native-execution allowlist"),
        @($readmePath, $readmeText, 'Debian 12 full runs in a separate `debian:12` job container', "README must document Debian container-native consumer execution"),
        @($readmePath, $readmeText, 'Fedora 40 full runs in its own separate `fedora:40` job container', "README must document Fedora container-native consumer execution"),
        @($guidePath, $guideText, "matrix-required modules plus the ordered staged-optional subset recorded in provenance", "Linked runtime guide must document provenance-derived full payload verification"),
        @($guidePath, $guideText, "Ubuntu 24.04 x64 full/mini and Ubuntu 22.04 x64 full", "Linked runtime guide must document the exact hosted targeted native-execution allowlist"),
        @($guidePath, $guideText, 'Debian 12 full runs in a separate `debian:12` job container', "Linked runtime guide must document Debian container-native consumer execution"),
        @($guidePath, $guideText, 'Fedora 40 full runs in its own separate `fedora:40` job container', "Linked runtime guide must document Fedora container-native consumer execution"))) {
    Assert-Contains -Path $expectation[0] -Text $expectation[1] -Needle $expectation[2] -Issue $expectation[3]
}

Assert-ExactLine `
    -Path $workflowPath `
    -Text $ubuntuJobText `
    -ExpectedLine "    if: `${{ ((inputs.rid == 'ubuntu.24.04-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini')) || (inputs.rid == 'ubuntu.22.04-x64' && inputs.runtime_profile == 'full')) && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Hosted targeted verification condition must remain exactly the three proven targets"

Assert-ExactLine `
    -Path $workflowPath `
    -Text $debianJobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'debian.12-x64' && inputs.runtime_profile == 'full' && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Debian targeted verification condition must remain exactly Debian 12 x64 full"

Assert-ExactLine `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'fedora.40-x64' && inputs.runtime_profile == 'full' && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Fedora targeted verification condition must remain exactly Fedora 40 x64 full"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "inputs.rid ==" `
    -ExpectedCount 1 `
    -Issue "Debian container job must gate on exactly one RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "inputs.runtime_profile ==" `
    -ExpectedCount 1 `
    -Issue "Debian container job must gate on exactly one runtime profile"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "-SelectedRid debian.12-x64" `
    -ExpectedCount 2 `
    -Issue "Both Debian guards must select the exact Debian RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "-SelectedRuntimeProfile full" `
    -ExpectedCount 2 `
    -Issue "Both Debian guards must select only the full profile"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "inputs.rid ==" `
    -ExpectedCount 1 `
    -Issue "Fedora container job must gate on exactly one RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "inputs.runtime_profile ==" `
    -ExpectedCount 1 `
    -Issue "Fedora container job must gate on exactly one runtime profile"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "-SelectedRid fedora.40-x64" `
    -ExpectedCount 2 `
    -Issue "Both Fedora guards must select the exact Fedora RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "-SelectedRuntimeProfile full" `
    -ExpectedCount 2 `
    -Issue "Both Fedora guards must select only the full profile"

Assert-NotContains `
    -Path $consumerGuardPath `
    -Text $consumerGuardText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "Packaged native consumer must not mask loader RUNPATH defects with an environment override"

Assert-NotContains `
    -Path $workflowPath `
    -Text $ubuntuJobText `
    -Needle "debian.12-x64" `
    -Issue "Debian must not be folded into the hosted Ubuntu verification allowlist"

Assert-NotContains `
    -Path $workflowPath `
    -Text $ubuntuJobText `
    -Needle "fedora.40-x64" `
    -Issue "Fedora must not be folded into the hosted Ubuntu verification allowlist"

Assert-NotContains `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "fedora.40-x64" `
    -Issue "Fedora must not be folded into the Debian container verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "runtime_profile == 'mini'" `
    -Issue "Debian mini must not enter the container-native verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "Debian container verification must not mask loader RUNPATH defects with an environment override"

Assert-NotContains `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "runtime_profile == 'mini'" `
    -Issue "Fedora mini must not enter the container-native verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "Fedora container verification must not mask loader RUNPATH defects with an environment override"

Assert-NotContains `
    -Path $workflowPath `
    -Text $ubuntuJobText `
    -Needle "verify-targeted-real-mini:" `
    -Issue "Targeted verification job name must not claim mini-only behavior after adding the proven full path"

Assert-NotContains `
    -Path $workflowPath `
    -Text $workflowText `
    -Needle "name: nupkg-ubuntu.24.04-x64-" `
    -Issue "Targeted runtime artifact download must follow the selected proven RID/profile"

Assert-NotContains `
    -Path $workflowPath `
    -Text $workflowText `
    -Needle "-SelectedRid ubuntu.24.04-x64" `
    -Issue "Targeted guards must not hardcode Ubuntu 24.04 after adding Ubuntu 22.04 full"

Assert-NotContains `
    -Path $workflowPath `
    -Text $workflowText `
    -Needle "inputs.rid == 'ubuntu.22.04-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini')" `
    -Issue "Ubuntu 22.04 mini must not enter the targeted native-execution allowlist"

Assert-Matches `
    -Path $cmakePath `
    -Text $cmakeText `
    -Pattern 'list\s*\(\s*APPEND\s+OPENCV_CSHARP_NATIVE_RUNTIME_TESTS\s+\$\{OPENCV_CSHARP_NATIVE_ABI_EXPORT_TEST\}\s*\).*LD_LIBRARY_PATH=\$\{OPENCV_CSHARP_OPENCV_RUNTIME_DIRECTORY\}' `
    -Issue "Producer Linux runtime environment must be applied after the ABI export audit joins the runtime test list"

Assert-Matches `
    -Path $cmakePath `
    -Text $cmakeText `
    -Pattern 'if\(UNIX AND NOT APPLE AND OPENCV_CSHARP_BUILD_WITH_OPENCV\).*INSTALL_RPATH "\\\$ORIGIN"\s*\).*target_link_options\(\$\{OPENCV_CSHARP_NATIVE_TARGET\} PRIVATE "LINKER:--no-as-needed"\)\s*endif\(\)' `
    -Issue "Linux no-as-needed closure must apply to both full and mini profiles inside the packaged RUNPATH block"

if ($violations.Count -gt 0) {
    Write-Host "Targeted real pack consumer verification surface guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Issue | Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Targeted real pack consumer verification surface guard passed."
Write-Host "Hosted targets: ubuntu.24.04-x64/full, ubuntu.24.04-x64/mini, ubuntu.22.04-x64/full."
Write-Host "Container targets: debian.12-x64/full in debian:12; fedora.40-x64/full in fedora:40."
Write-Host "All targeted execution is non-synthetic and non-publishing."
Write-Host "Packaged native smoke modules: mini core,imgproc,imgcodecs,videoio; full adds dnn."
