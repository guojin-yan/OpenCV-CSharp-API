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
$windowsPeAuditPath = "scripts/Test-WindowsRuntimePeClosure.ps1"
$cmakePath = "src/OpenCvSharp.Native/CMakeLists.txt"
$readmePath = "README.md"
$guidePath = "docs/articles/linked-runtime-build-guide.md"
$runtimeMatrixPath = "packaging/runtime/runtime-package-matrix.json"

$workflowText = Read-RequiredText $workflowPath
$artifactGuardText = Read-RequiredText $artifactGuardPath
$consumerGuardText = Read-RequiredText $consumerGuardPath
$windowsPeAuditText = Read-RequiredText $windowsPeAuditPath
$cmakeText = Read-RequiredText $cmakePath
$readmeText = Read-RequiredText $readmePath
$guideText = Read-RequiredText $guidePath
$runtimeMatrixText = Read-RequiredText $runtimeMatrixPath
$runtimeMatrix = $runtimeMatrixText | ConvertFrom-Json
$windowsJobText = Get-WorkflowJobText -JobName "verify-targeted-real-windows-x64"
$windowsArm64JobText = Get-WorkflowJobText -JobName "verify-targeted-real-windows-arm64"
$ubuntuJobText = Get-WorkflowJobText -JobName "verify-targeted-real"
$ubuntuArm64JobText = Get-WorkflowJobText -JobName "verify-targeted-real-ubuntu-arm64"
$ubuntu2204Arm64JobText = Get-WorkflowJobText -JobName "verify-targeted-real-ubuntu2204-arm64"
$debianJobText = Get-WorkflowJobText -JobName "verify-targeted-real-debian"
$debianArm64JobText = Get-WorkflowJobText -JobName "verify-targeted-real-debian-arm64"
$fedoraJobText = Get-WorkflowJobText -JobName "verify-targeted-real-fedora"
$rockyJobText = Get-WorkflowJobText -JobName "verify-targeted-real-rocky"
$rhelJobText = Get-WorkflowJobText -JobName "verify-targeted-real-rhel"
$alpineJobText = Get-WorkflowJobText -JobName "verify-targeted-real-alpine"

foreach ($expectedTarget in @(
        [pscustomobject]@{ Rid = "win-x64"; Runner = "windows-latest" },
        [pscustomobject]@{ Rid = "win-arm64"; Runner = "windows-11-vs2026-arm" },
        [pscustomobject]@{ Rid = "ubuntu.22.04-x64"; Runner = "ubuntu-22.04" },
        [pscustomobject]@{ Rid = "ubuntu.22.04-arm64"; Runner = "ubuntu-24.04-arm" },
        [pscustomobject]@{ Rid = "debian.12-arm64"; Runner = "ubuntu-24.04-arm" },
        [pscustomobject]@{ Rid = "ubuntu.24.04-x64"; Runner = "ubuntu-24.04" },
        [pscustomobject]@{ Rid = "ubuntu.24.04-arm64"; Runner = "ubuntu-24.04-arm" })) {
    $ridSpecs = @($runtimeMatrix.rids | Where-Object { $_.rid -eq $expectedTarget.Rid })
    if ($ridSpecs.Count -ne 1 -or [string]$ridSpecs[0].runner -ne $expectedTarget.Runner) {
        $violations.Add([pscustomobject]@{
            Path = $runtimeMatrixPath
            Issue = "Targeted native execution RID must map to its approved native runner"
            Text = "$($expectedTarget.Rid) -> $($expectedTarget.Runner)"
        })
    }
}

foreach ($expectation in @(
        @($workflowPath, $windowsJobText, "verify-targeted-real-windows-x64:", "Pack workflow must keep a separate Windows x64 verification job"),
        @($workflowPath, $windowsJobText, "inputs.rid == 'win-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Windows verification must use the exact full/mini non-synthetic non-publishing gate"),
        @($workflowPath, $windowsJobText, "runs-on: windows-latest", "Windows verification must run on the audited hosted Windows image"),
        @($workflowPath, $windowsJobText, "PROCESSOR_ARCHITECTURE -ne 'AMD64'", "Windows verification must require an actual AMD64 host"),
        @($workflowPath, $windowsJobText, "ProcessArchitecture.ToString()", "Windows verification must require an actual x64 process"),
        @($workflowPath, $windowsJobText, "WINDOWS_X64_PACKAGE_CONSUMER_HOST_OK", "Windows verification must emit factual host/process evidence"),
        @($workflowPath, $windowsJobText, "name: nupkg-managed", "Windows verification must download the same-run managed package"),
        @($workflowPath, $windowsJobText, 'name: nupkg-${{ inputs.rid }}-${{ inputs.runtime_profile }}', "Windows verification must derive the exact runtime artifact from its locked RID/profile gate"),
        @($workflowPath, $windowsJobText, "path: artifacts/pack-targeted-windows/nupkg-managed", "Windows managed artifact must use an isolated path"),
        @($workflowPath, $windowsJobText, 'path: artifacts/pack-targeted-windows/nupkg-${{ inputs.rid }}-${{ inputs.runtime_profile }}', "Windows runtime artifact must use an isolated selected path"),
        @($workflowPath, $windowsJobText, "-ExpectedSyntheticRuntimeInputs false", "Windows artifact and consumer guards must require real provenance"),
        @($workflowPath, $windowsJobText, "-SelectedRid win-x64", "Windows guards must select exact win-x64"),
        @($workflowPath, $windowsJobText, "-SelectedRuntimeProfile '`${{ inputs.runtime_profile }}'", "Windows guards must select the exact selected full/mini profile"),
        @($workflowPath, $windowsJobText, "Test-WindowsRuntimePeClosure.ps1", "Windows package must pass the reusable PE closure audit"),
        @($workflowPath, $windowsJobText, "-CompileNativeSmoke", "Windows verifier must compile the package consumer"),
        @($workflowPath, $windowsJobText, "-RunNativeSmoke", "Windows verifier must run package-output profile-specific native calls"),
        @($workflowPath, $windowsJobText, "artifacts\real-runtime-inputs", "Windows verifier must explicitly reject producer artifact directories in PATH"),
        @($workflowPath, $windowsArm64JobText, "verify-targeted-real-windows-arm64:", "Pack workflow must keep a separate Windows ARM64 verification job"),
        @($workflowPath, $windowsArm64JobText, "inputs.rid == 'win-arm64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Windows ARM64 verification must use the exact full/mini non-synthetic non-publishing gate"),
        @($workflowPath, $windowsArm64JobText, "runs-on: windows-11-vs2026-arm", "Windows ARM64 verification must run on the audited native hosted ARM64 image"),
        @($workflowPath, $windowsArm64JobText, "RUNNER_ARCH -ne 'ARM64'", "Windows ARM64 verification must require the GitHub ARM64 runner architecture"),
        @($workflowPath, $windowsArm64JobText, "PROCESSOR_ARCHITECTURE -ne 'ARM64'", "Windows ARM64 verification must require an actual ARM64 host"),
        @($workflowPath, $windowsArm64JobText, "OSArchitecture.ToString()", "Windows ARM64 verification must inspect actual OS architecture"),
        @($workflowPath, $windowsArm64JobText, "ProcessArchitecture.ToString()", "Windows ARM64 verification must inspect actual process architecture"),
        @($workflowPath, $windowsArm64JobText, "PROCESSOR_ARCHITEW6432", "Windows ARM64 verification must reject compatibility translation"),
        @($workflowPath, $windowsArm64JobText, "WINDOWS_ARM64_PACKAGE_CONSUMER_HOST_OK", "Windows ARM64 verification must emit factual native host/process evidence"),
        @($workflowPath, $windowsArm64JobText, "name: nupkg-managed", "Windows ARM64 verification must download the same-run managed package"),
        @($workflowPath, $windowsArm64JobText, 'name: nupkg-win-arm64-${{ inputs.runtime_profile }}', "Windows ARM64 verification must download only the exact selected full/mini runtime artifact"),
        @($workflowPath, $windowsArm64JobText, "path: artifacts/pack-targeted-windows-arm64/nupkg-managed", "Windows ARM64 managed artifact must use an isolated path"),
        @($workflowPath, $windowsArm64JobText, 'path: artifacts/pack-targeted-windows-arm64/nupkg-win-arm64-${{ inputs.runtime_profile }}', "Windows ARM64 runtime artifact must use an isolated selected path"),
        @($workflowPath, $windowsArm64JobText, "-ExpectedSyntheticRuntimeInputs false", "Windows ARM64 artifact and consumer guards must require real provenance"),
        @($workflowPath, $windowsArm64JobText, "-SelectedRid win-arm64", "Windows ARM64 guards must select exact win-arm64"),
        @($workflowPath, $windowsArm64JobText, "-SelectedRuntimeProfile '`${{ inputs.runtime_profile }}'", "Windows ARM64 guards must select the exact selected full/mini profile"),
        @($workflowPath, $windowsArm64JobText, "Test-WindowsRuntimePeClosure.ps1", "Windows ARM64 package must pass the reusable architecture-aware PE closure audit"),
        @($workflowPath, $windowsArm64JobText, "-CompileNativeSmoke", "Windows ARM64 verifier must compile the package consumer natively"),
        @($workflowPath, $windowsArm64JobText, "-RunNativeSmoke", "Windows ARM64 verifier must execute package-output profile-specific native calls"),
        @($workflowPath, $windowsArm64JobText, "artifacts\real-runtime-inputs", "Windows ARM64 verifier must explicitly reject producer artifact directories in PATH"),
        @($workflowPath, $workflowText, 'WINDOWS_ARM64_RUNTIME_INPUT_PROVENANCE_OK profile=$profileName files=$expectedPayloadFileCount modules=$expectedRuntimeFileCount sources=$expectedSourceCount abi_functions=$expectedAbiFunctionCount synthetic=false', "Pack runtime job must validate profile-derived real Windows ARM64 provenance before packaging"),
        @($workflowPath, $workflowText, "OpenCvSourcePatchEvidence", "Pack runtime job must require Windows ARM64 OpenCV source-patch provenance"),
        @($workflowPath, $workflowText, "windows-arm64-mlas-processor-case.patch", "Pack runtime job must verify the repository-owned Windows ARM64 OpenCV patch"),
        @($workflowPath, $workflowText, "Windows ARM64 provenance does not retain the audited OpenCV MLAS processor-case patch", "Pack runtime job must reject absent or changed Windows ARM64 OpenCV patch evidence"),
        @($workflowPath, $ubuntuJobText, "verify-targeted-real:", "Pack workflow must keep the targeted real Ubuntu verification job"),
        @($workflowPath, $ubuntuJobText, "((inputs.rid == 'ubuntu.24.04-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini')) || (inputs.rid == 'ubuntu.22.04-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini'))) && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Hosted targeted verification must keep the exact four-target non-synthetic non-publishing allowlist"),
        @($workflowPath, $ubuntuJobText, "runs-on: `${{ inputs.rid == 'ubuntu.22.04-x64' && 'ubuntu-22.04' || 'ubuntu-24.04' }}", "Hosted targeted verification must execute each proven RID on its matching Ubuntu runner"),
        @($workflowPath, $ubuntuJobText, "UBUNTU_22_04_X64_CONSUMER_EVIDENCE", "Ubuntu 22.04 x64 consumer must record matching hosted runner evidence before package execution"),
        @($workflowPath, $workflowText, 'UBUNTU_22_04_X64_RUNTIME_INPUT_PROVENANCE_OK profile=mini files=$expectedPayloadFileCount modules=$expectedModuleCount sources=8 abi_functions=304 synthetic=false', "Pack runtime job must validate exact Ubuntu 22.04 x64 mini producer provenance"),
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
        @($workflowPath, $ubuntuArm64JobText, "verify-targeted-real-ubuntu-arm64:", "Pack workflow must keep a separate native Ubuntu ARM64 verification job"),
        @($workflowPath, $ubuntuArm64JobText, "inputs.rid == 'ubuntu.24.04-arm64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Ubuntu ARM64 verification must use the exact full/mini non-synthetic non-publishing gate"),
        @($workflowPath, $ubuntuArm64JobText, "runs-on: ubuntu-24.04-arm", "Ubuntu ARM64 verification must execute on the native GitHub-hosted ARM64 runner"),
        @($workflowPath, $ubuntuArm64JobText, 'test "$(uname -m)" = "aarch64"', "Ubuntu ARM64 verification must reject non-AArch64 execution"),
        @($workflowPath, $ubuntuArm64JobText, 'test "$(dpkg --print-architecture)" = "arm64"', "Ubuntu ARM64 verification must require the native Debian arm64 architecture"),
        @($workflowPath, $ubuntuArm64JobText, 'test "${ID:-}" = "ubuntu"', "Ubuntu ARM64 verification must require the Ubuntu distro identity"),
        @($workflowPath, $ubuntuArm64JobText, 'test "${VERSION_ID:-}" = "24.04"', "Ubuntu ARM64 verification must require Ubuntu 24.04"),
        @($workflowPath, $ubuntuArm64JobText, "getconf GNU_LIBC_VERSION", "Ubuntu ARM64 verification must report actual glibc evidence"),
        @($workflowPath, $ubuntuArm64JobText, "UBUNTU_24_04_ARM64_CONSUMER_EVIDENCE", "Ubuntu ARM64 verification must emit explicit native consumer evidence"),
        @($workflowPath, $ubuntuArm64JobText, "name: nupkg-managed", "Ubuntu ARM64 verification must download the same-run managed artifact explicitly"),
        @($workflowPath, $ubuntuArm64JobText, 'name: nupkg-ubuntu.24.04-arm64-${{ inputs.runtime_profile }}', "Ubuntu ARM64 verification must download only the exact selected full/mini runtime artifact"),
        @($workflowPath, $ubuntuArm64JobText, "path: artifacts/pack-targeted-ubuntu-arm64/nupkg-managed", "Ubuntu ARM64 managed artifact must use its isolated exact path"),
        @($workflowPath, $ubuntuArm64JobText, 'path: artifacts/pack-targeted-ubuntu-arm64/nupkg-ubuntu.24.04-arm64-${{ inputs.runtime_profile }}', "Ubuntu ARM64 runtime artifact must use its isolated exact selected path"),
        @($workflowPath, $ubuntuArm64JobText, "-ExpectedSyntheticRuntimeInputs false", "Ubuntu ARM64 artifact and consumer checks must require real provenance"),
        @($workflowPath, $ubuntuArm64JobText, "-SelectedRid ubuntu.24.04-arm64", "Ubuntu ARM64 checks must select only the proven distro-specific RID"),
        @($workflowPath, $ubuntuArm64JobText, "-SelectedRuntimeProfile '`${{ inputs.runtime_profile }}'", "Ubuntu ARM64 checks must select the exact full/mini profile"),
        @($workflowPath, $ubuntuArm64JobText, "UBUNTU_24_04_ARM64_PACKAGE_ELF_EVIDENCE", "Ubuntu ARM64 package must pass the exact profile-derived AArch64 ELF closure audit"),
        @($workflowPath, $ubuntuArm64JobText, "expected_runtime_file_count=20", "Ubuntu ARM64 mini package audit must require exactly 20 runtime files"),
        @($workflowPath, $ubuntuArm64JobText, "expected_canonical_count=8", "Ubuntu ARM64 mini package audit must require exactly eight canonical ELFs"),
        @($workflowPath, $ubuntuArm64JobText, "expected_direct_opencv=6", "Ubuntu ARM64 mini package audit must require exactly six direct OpenCV dependencies"),
        @($workflowPath, $ubuntuArm64JobText, 'readelf -h "$elf"', "Ubuntu ARM64 package audit must inspect every canonical ELF machine type"),
        @($workflowPath, $ubuntuArm64JobText, "-RunNativeSmoke", "Ubuntu ARM64 consumer verification must execute full DNN or mini NOT_LINKED native calls"),
        @($workflowPath, $workflowText, 'UBUNTU_24_04_ARM64_RUNTIME_INPUT_PROVENANCE_OK profile=$profileName files=$expectedPayloadFileCount modules=$expectedModuleCount sources=$expectedSourceCount abi_functions=$expectedAbiFunctionCount synthetic=false', "Pack runtime job must validate profile-derived real Ubuntu ARM64 provenance"),
        @($workflowPath, $ubuntu2204Arm64JobText, "verify-targeted-real-ubuntu2204-arm64:", "Pack workflow must keep a separate Ubuntu 22.04 ARM64 verification job"),
        @($workflowPath, $ubuntu2204Arm64JobText, "inputs.rid == 'ubuntu.22.04-arm64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Ubuntu 22.04 ARM64 verification must use the exact full/mini non-synthetic non-publishing gate"),
        @($workflowPath, $ubuntu2204Arm64JobText, "runs-on: ubuntu-24.04-arm", "Ubuntu 22.04 ARM64 verification must use the native AArch64 Docker host"),
        @($workflowPath, $ubuntu2204Arm64JobText, "ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982", "Ubuntu 22.04 ARM64 verification must pin the audited official image digest"),
        @($workflowPath, $ubuntu2204Arm64JobText, 'test "$(uname -m)" = "aarch64"', "Ubuntu 22.04 ARM64 verification must reject non-AArch64 host and container execution"),
        @($workflowPath, $ubuntu2204Arm64JobText, 'test "$(dpkg --print-architecture)" = "arm64"', "Ubuntu 22.04 ARM64 verification must require native Debian arm64 package architecture"),
        @($workflowPath, $ubuntu2204Arm64JobText, 'test "$(docker info --format ''{{.Architecture}}'')" = "aarch64"', "Ubuntu 22.04 ARM64 verification must require a native AArch64 Docker server"),
        @($workflowPath, $ubuntu2204Arm64JobText, 'test "${ID:-}" = "ubuntu"', "Ubuntu 22.04 ARM64 verification must require Ubuntu userspace"),
        @($workflowPath, $ubuntu2204Arm64JobText, 'test "${VERSION_ID:-}" = "22.04"', "Ubuntu 22.04 ARM64 verification must require exact Ubuntu 22.04 userspace"),
        @($workflowPath, $ubuntu2204Arm64JobText, "UBUNTU_22_04_ARM64_CONSUMER_HOST_EVIDENCE", "Ubuntu 22.04 ARM64 verification must distinguish its host boundary"),
        @($workflowPath, $ubuntu2204Arm64JobText, "UBUNTU_22_04_ARM64_CONSUMER_IMAGE_EVIDENCE", "Ubuntu 22.04 ARM64 verification must retain official image evidence"),
        @($workflowPath, $ubuntu2204Arm64JobText, "UBUNTU_22_04_ARM64_CONSUMER_EVIDENCE", "Ubuntu 22.04 ARM64 verification must emit factual container evidence"),
        @($workflowPath, $ubuntu2204Arm64JobText, "68f3874cdb6cd564acf404103dfc410ee85435b02f0ad648e73a958853175d6c", "Ubuntu 22.04 ARM64 verification must pin the audited PowerShell ARM64 archive hash"),
        @($workflowPath, $ubuntu2204Arm64JobText, "--channel 8.0 --architecture arm64", "Ubuntu 22.04 ARM64 verification must install a native ARM64 .NET 8 SDK"),
        @($workflowPath, $ubuntu2204Arm64JobText, "name: nupkg-managed", "Ubuntu 22.04 ARM64 verification must download the same-run managed artifact"),
        @($workflowPath, $ubuntu2204Arm64JobText, 'name: nupkg-ubuntu.22.04-arm64-${{ inputs.runtime_profile }}', "Ubuntu 22.04 ARM64 verification must download only its exact selected full/mini runtime artifact"),
        @($workflowPath, $ubuntu2204Arm64JobText, "path: artifacts/pack-targeted-ubuntu2204-arm64/nupkg-managed", "Ubuntu 22.04 ARM64 managed artifact must use an isolated path"),
        @($workflowPath, $ubuntu2204Arm64JobText, 'path: artifacts/pack-targeted-ubuntu2204-arm64/nupkg-ubuntu.22.04-arm64-${{ inputs.runtime_profile }}', "Ubuntu 22.04 ARM64 runtime artifact must use an isolated selected path"),
        @($workflowPath, $ubuntu2204Arm64JobText, "-ExpectedSyntheticRuntimeInputs false", "Ubuntu 22.04 ARM64 guards must require real provenance"),
        @($workflowPath, $ubuntu2204Arm64JobText, "-SelectedRid ubuntu.22.04-arm64", "Ubuntu 22.04 ARM64 guards must select the exact distro RID"),
        @($workflowPath, $ubuntu2204Arm64JobText, '-SelectedRuntimeProfile "$RUNTIME_PROFILE"', "Ubuntu 22.04 ARM64 guards must select the exact full/mini profile"),
        @($workflowPath, $ubuntu2204Arm64JobText, "UBUNTU_22_04_ARM64_PACKAGE_ELF_EVIDENCE", "Ubuntu 22.04 ARM64 package must pass the exact profile-derived target-container ELF closure audit"),
        @($workflowPath, $ubuntu2204Arm64JobText, "expected_runtime_file_count=20", "Ubuntu 22.04 ARM64 mini package audit must require exactly 20 runtime files"),
        @($workflowPath, $ubuntu2204Arm64JobText, "expected_canonical_count=8", "Ubuntu 22.04 ARM64 mini package audit must require exactly eight canonical ELFs"),
        @($workflowPath, $ubuntu2204Arm64JobText, "expected_direct_opencv=6", "Ubuntu 22.04 ARM64 mini package audit must require exactly six direct OpenCV dependencies"),
        @($workflowPath, $ubuntu2204Arm64JobText, 'readelf -h "$elf"', "Ubuntu 22.04 ARM64 package audit must inspect every canonical ELF machine type"),
        @($workflowPath, $workflowText, 'UBUNTU_22_04_ARM64_RUNTIME_INPUT_PROVENANCE_OK profile=$profileName files=$expectedPayloadFileCount modules=$expectedModuleCount sources=$expectedSourceCount abi_functions=$expectedAbiFunctionCount synthetic=false', "Pack runtime job must validate exact Ubuntu 22.04 ARM64 host/container provenance"),
        @($workflowPath, $ubuntu2204Arm64JobText, "-RunNativeSmoke", "Ubuntu 22.04 ARM64 consumer must execute full DNN or mini NOT_LINKED native calls"),
        @($workflowPath, $debianJobText, "verify-targeted-real-debian:", "Pack workflow must keep a separate Debian container verification job"),
        @($workflowPath, $debianJobText, "inputs.rid == 'debian.12-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Debian verification must use the exact full/mini non-synthetic non-publishing gate"),
        @($workflowPath, $debianJobText, "runs-on: ubuntu-24.04", "Debian container verification must use the supported hosted runner"),
        @($workflowPath, $debianJobText, "container: debian:12", "Debian verification must execute in the Debian 12 job container"),
        @($workflowPath, $debianJobText, "cat /etc/os-release", "Debian verification must expose container distro evidence"),
        @($workflowPath, $debianJobText, 'if [ "${ID:-}" != "debian" ]', "Debian verification must require the Debian distro identity"),
        @($workflowPath, $debianJobText, '12|12.*) ;;', "Debian verification must require Debian version 12 or 12.x"),
        @($workflowPath, $debianJobText, "getconf GNU_LIBC_VERSION", "Debian verification must record the container libc identity"),
        @($workflowPath, $debianJobText, 'test "$(uname -m)" = "x86_64"', "Debian verification must require native x86-64 execution"),
        @($workflowPath, $debianJobText, 'test "$(dpkg --print-architecture)" = "amd64"', "Debian verification must require native amd64 package architecture"),
        @($workflowPath, $debianJobText, 'test "$process_architecture" = "X64"', "Debian verification must require a native x64 PowerShell process"),
        @($workflowPath, $debianJobText, "DEBIAN_12_X64_CONTAINER_EVIDENCE", "Debian verification must emit an explicit profile-aware container evidence marker"),
        @($workflowPath, $debianJobText, "apt-get install -y --no-install-recommends powershell", "Debian verification must install PowerShell before invoking repository guards"),
        @($workflowPath, $debianJobText, "10.0.x", "Debian verification must install .NET 10"),
        @($workflowPath, $debianJobText, "9.0.x", "Debian verification must install .NET 9"),
        @($workflowPath, $debianJobText, "8.0.x", "Debian verification must install .NET 8"),
        @($workflowPath, $debianJobText, "name: nupkg-managed", "Debian verification must download the same-run managed artifact explicitly"),
        @($workflowPath, $debianJobText, 'name: nupkg-debian.12-x64-${{ inputs.runtime_profile }}', "Debian verification must download only the exact selected Debian full/mini runtime artifact"),
        @($workflowPath, $debianJobText, "path: artifacts/pack-targeted-debian/nupkg-managed", "Debian managed artifact must use its isolated exact path"),
        @($workflowPath, $debianJobText, 'path: artifacts/pack-targeted-debian/nupkg-debian.12-x64-${{ inputs.runtime_profile }}', "Debian runtime artifact must use its isolated selected path"),
        @($workflowPath, $debianJobText, "-ExpectedSyntheticRuntimeInputs false", "Debian artifact and consumer checks must require real provenance"),
        @($workflowPath, $debianJobText, "-SelectedRid debian.12-x64", "Debian checks must select only the proven Debian RID"),
        @($workflowPath, $debianJobText, '-SelectedRuntimeProfile ''${{ inputs.runtime_profile }}''', "Debian checks must select the exact full/mini profile"),
        @($workflowPath, $debianJobText, "DEBIAN_12_X64_PACKAGE_ELF_EVIDENCE", "Debian package must pass a profile-derived x86-64 ELF closure audit"),
        @($workflowPath, $debianJobText, "expected_runtime_file_count=20", "Debian mini package audit must require exactly 20 runtime files"),
        @($workflowPath, $debianJobText, "expected_canonical_count=8", "Debian mini package audit must require exactly eight canonical ELFs"),
        @($workflowPath, $debianJobText, "expected_direct_opencv=6", "Debian mini package audit must require exactly six direct OpenCV dependencies"),
        @($workflowPath, $workflowText, 'DEBIAN_12_X64_RUNTIME_INPUT_PROVENANCE_OK profile=mini files=$expectedPayloadFileCount modules=$expectedModuleCount sources=8 abi_functions=304 synthetic=false', "Pack runtime job must validate the exact Debian 12 x64 mini producer provenance"),
        @($workflowPath, $debianJobText, "-RunNativeSmoke", "Debian consumer verification must execute full DNN or mini NOT_LINKED native calls inside the container"),
        @($workflowPath, $debianArm64JobText, "verify-targeted-real-debian-arm64:", "Pack workflow must keep a separate native Debian 12 ARM64 verification job"),
        @($workflowPath, $debianArm64JobText, "inputs.rid == 'debian.12-arm64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Debian ARM64 verification must use the exact full/mini non-synthetic non-publishing gate"),
        @($workflowPath, $debianArm64JobText, "runs-on: ubuntu-24.04-arm", "Debian ARM64 verification must use the native AArch64 Docker host"),
        @($workflowPath, $debianArm64JobText, "debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc", "Debian ARM64 verification must pin the audited official image digest"),
        @($workflowPath, $debianArm64JobText, 'test "$(uname -m)" = "aarch64"', "Debian ARM64 verification must reject non-AArch64 host and container execution"),
        @($workflowPath, $debianArm64JobText, 'test "$(dpkg --print-architecture)" = "arm64"', "Debian ARM64 verification must require native arm64 package architecture"),
        @($workflowPath, $debianArm64JobText, 'test "$(docker info --format ''{{.Architecture}}'')" = "aarch64"', "Debian ARM64 verification must require a native AArch64 Docker server"),
        @($workflowPath, $debianArm64JobText, 'test "${ID:-}" = "debian"', "Debian ARM64 verification must require Debian userspace"),
        @($workflowPath, $debianArm64JobText, '12|12.*) ;;', "Debian ARM64 verification must require exact Debian 12 userspace"),
        @($workflowPath, $debianArm64JobText, "DEBIAN_12_ARM64_CONSUMER_HOST_EVIDENCE", "Debian ARM64 verification must distinguish its host boundary"),
        @($workflowPath, $debianArm64JobText, "DEBIAN_12_ARM64_CONSUMER_IMAGE_EVIDENCE", "Debian ARM64 verification must retain official image evidence"),
        @($workflowPath, $debianArm64JobText, "DEBIAN_12_ARM64_CONSUMER_EVIDENCE", "Debian ARM64 verification must emit factual container evidence"),
        @($workflowPath, $debianArm64JobText, "DEBIAN_12_ARM64_POWERSHELL_EVIDENCE", "Debian ARM64 verification must verify native PowerShell"),
        @($workflowPath, $debianArm64JobText, "--channel 8.0 --architecture arm64", "Debian ARM64 verification must install a native ARM64 .NET 8 SDK"),
        @($workflowPath, $debianArm64JobText, "name: nupkg-managed", "Debian ARM64 verification must download the same-run managed artifact"),
        @($workflowPath, $debianArm64JobText, 'name: nupkg-debian.12-arm64-${{ inputs.runtime_profile }}', "Debian ARM64 verification must download only its exact selected full/mini runtime artifact"),
        @($workflowPath, $debianArm64JobText, "path: artifacts/pack-targeted-debian-arm64/nupkg-managed", "Debian ARM64 managed artifact must use an isolated path"),
        @($workflowPath, $debianArm64JobText, 'path: artifacts/pack-targeted-debian-arm64/nupkg-debian.12-arm64-${{ inputs.runtime_profile }}', "Debian ARM64 runtime artifact must use an isolated selected path"),
        @($workflowPath, $debianArm64JobText, "-ExpectedSyntheticRuntimeInputs false", "Debian ARM64 guards must require real provenance"),
        @($workflowPath, $debianArm64JobText, "-SelectedRid debian.12-arm64", "Debian ARM64 guards must select the exact distro RID"),
        @($workflowPath, $debianArm64JobText, '-SelectedRuntimeProfile "$RUNTIME_PROFILE"', "Debian ARM64 guards must select the exact full/mini profile"),
        @($workflowPath, $debianArm64JobText, "DEBIAN_12_ARM64_PACKAGE_ELF_EVIDENCE", "Debian ARM64 package must pass the exact profile-derived target-container ELF closure audit"),
        @($workflowPath, $debianArm64JobText, "expected_runtime_file_count=20", "Debian ARM64 mini package audit must require exactly 20 runtime files"),
        @($workflowPath, $debianArm64JobText, "expected_canonical_count=8", "Debian ARM64 mini package audit must require exactly eight canonical ELFs"),
        @($workflowPath, $debianArm64JobText, "expected_direct_opencv=6", "Debian ARM64 mini package audit must require exactly six direct OpenCV dependencies"),
        @($workflowPath, $debianArm64JobText, 'readelf -h "$elf"', "Debian ARM64 package audit must inspect every canonical ELF machine type"),
        @($workflowPath, $workflowText, 'DEBIAN_12_ARM64_RUNTIME_INPUT_PROVENANCE_OK profile=$profileName files=$expectedPayloadFileCount modules=$expectedModuleCount sources=$expectedSourceCount abi_functions=$expectedAbiFunctionCount synthetic=false', "Pack runtime job must validate exact Debian 12 ARM64 host/container provenance"),
        @($workflowPath, $debianArm64JobText, "-RunNativeSmoke", "Debian ARM64 consumer must execute native and deterministic DNN or mini NOT_LINKED calls"),
        @($workflowPath, $fedoraJobText, "verify-targeted-real-fedora:", "Pack workflow must keep a separate Fedora container verification job"),
        @($workflowPath, $fedoraJobText, "inputs.rid == 'fedora.40-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Fedora verification must use the exact full/mini non-synthetic non-publishing gate"),
        @($workflowPath, $fedoraJobText, "runs-on: ubuntu-24.04", "Fedora container verification must use the supported hosted runner"),
        @($workflowPath, $fedoraJobText, "container: fedora:40", "Fedora verification must execute in the Fedora 40 job container"),
        @($workflowPath, $fedoraJobText, "cat /etc/os-release", "Fedora verification must expose container distro evidence"),
        @($workflowPath, $fedoraJobText, 'if [ "${ID:-}" != "fedora" ]', "Fedora verification must require the Fedora distro identity"),
        @($workflowPath, $fedoraJobText, '40|40.*) ;;', "Fedora verification must require Fedora version 40 or 40.x"),
        @($workflowPath, $fedoraJobText, 'test "${SUPPORT_END:-}" = "2025-05-13"', "Fedora verification must lock the exact ended-support boundary"),
        @($workflowPath, $fedoraJobText, 'test "$(uname -m)" = "x86_64"', "Fedora verification must require native x86-64 execution"),
        @($workflowPath, $fedoraJobText, 'test "$(rpm --eval ''%{_arch}'')" = "x86_64"', "Fedora verification must require native RPM x86_64 package architecture"),
        @($workflowPath, $fedoraJobText, 'test "$process_architecture" = "X64"', "Fedora verification must require a native x64 PowerShell process"),
        @($workflowPath, $fedoraJobText, '^glibc 2\.39($|\.)', "Fedora verification must require the Fedora 40 glibc 2.39 boundary"),
        @($workflowPath, $fedoraJobText, '(archive\.fedoraproject\.org|fedora-archive).*/fedora/linux/releases/40/Everything/x86_64/os/', "Fedora verification must require an archive mirror serving the exact Fedora 40 release repository path"),
        @($workflowPath, $fedoraJobText, '(archive\.fedoraproject\.org|fedora-archive).*/fedora/linux/updates/40/Everything/x86_64/', "Fedora verification must require an archive mirror serving the exact Fedora 40 updates repository path"),
        @($workflowPath, $fedoraJobText, "FEDORA_40_REPOSITORY_EVIDENCE", "Fedora verification must emit exact lifecycle and archive repository evidence"),
        @($workflowPath, $fedoraJobText, "FEDORA_40_CONTAINER_EVIDENCE profile=", "Fedora verification must preserve its profile-aware container evidence marker"),
        @($workflowPath, $fedoraJobText, "binutils", "Fedora package verification must install readelf for ELF closure auditing"),
        @($workflowPath, $fedoraJobText, "diffutils", "Fedora package verification must install diff for exact runtime payload comparison"),
        @($workflowPath, $fedoraJobText, "unzip", "Fedora package verification must install unzip for exact package inspection"),
        @($workflowPath, $fedoraJobText, "dnf install -y powershell", "Fedora verification must install PowerShell before invoking repository guards"),
        @($workflowPath, $fedoraJobText, "Microsoft publishes the compatible PowerShell RPM feed under its RHEL 9 path", "Fedora verifier must explain the factual PowerShell feed path without changing distro identity"),
        @($workflowPath, $fedoraJobText, "10.0.x", "Fedora verification must install .NET 10"),
        @($workflowPath, $fedoraJobText, "9.0.x", "Fedora verification must install .NET 9"),
        @($workflowPath, $fedoraJobText, "8.0.x", "Fedora verification must install .NET 8"),
        @($workflowPath, $fedoraJobText, "name: nupkg-managed", "Fedora verification must download the same-run managed artifact explicitly"),
        @($workflowPath, $fedoraJobText, 'name: nupkg-fedora.40-x64-${{ inputs.runtime_profile }}', "Fedora verification must download only the exact selected Fedora full/mini runtime artifact"),
        @($workflowPath, $fedoraJobText, "path: artifacts/pack-targeted-fedora/nupkg-managed", "Fedora managed artifact must use its isolated exact path"),
        @($workflowPath, $fedoraJobText, 'path: artifacts/pack-targeted-fedora/nupkg-fedora.40-x64-${{ inputs.runtime_profile }}', "Fedora runtime artifact must use its isolated selected path"),
        @($workflowPath, $fedoraJobText, "-ExpectedSyntheticRuntimeInputs false", "Fedora artifact and consumer checks must require real provenance"),
        @($workflowPath, $fedoraJobText, "-SelectedRid fedora.40-x64", "Fedora checks must select only the proven Fedora RID"),
        @($workflowPath, $fedoraJobText, '-SelectedRuntimeProfile ''${{ inputs.runtime_profile }}''', "Fedora checks must select the exact full/mini profile"),
        @($workflowPath, $fedoraJobText, "FEDORA_40_X64_PACKAGE_ELF_EVIDENCE", "Fedora package must pass a profile-derived x86-64 ELF closure audit"),
        @($workflowPath, $fedoraJobText, "expected_runtime_file_count=20", "Fedora mini package audit must require exactly 20 runtime files"),
        @($workflowPath, $fedoraJobText, "expected_canonical_count=8", "Fedora mini package audit must require exactly eight canonical ELFs"),
        @($workflowPath, $fedoraJobText, "expected_direct_opencv=6", "Fedora mini package audit must require exactly six direct OpenCV dependencies"),
        @($workflowPath, $fedoraJobText, 'readelf -h "$elf"', "Fedora package audit must inspect every canonical ELF machine type"),
        @($workflowPath, $fedoraJobText, 'grep -Fq "\$ORIGIN"', "Fedora package audit must require adjacent-library RUNPATH on every canonical ELF"),
        @($workflowPath, $workflowText, 'FEDORA_40_X64_RUNTIME_INPUT_PROVENANCE_OK profile=mini files=$expectedPayloadFileCount modules=$expectedModuleCount sources=8 abi_functions=304 synthetic=false', "Pack runtime job must validate the exact Fedora 40 x64 mini producer provenance"),
        @($workflowPath, $fedoraJobText, "-RunNativeSmoke", "Fedora consumer verification must execute full DNN or mini NOT_LINKED native calls inside the container"),
        @($workflowPath, $rockyJobText, "verify-targeted-real-rocky:", "Pack workflow must keep a separate Rocky container verification job"),
        @($workflowPath, $rockyJobText, "inputs.rid == 'rocky.9-x64' && inputs.runtime_profile == 'full' && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Rocky verification must use the exact full-only non-synthetic non-publishing gate"),
        @($workflowPath, $rockyJobText, "runs-on: ubuntu-24.04", "Rocky container verification must use the supported hosted runner"),
        @($workflowPath, $rockyJobText, "container: rockylinux:9", "Rocky verification must execute in the Rocky Linux 9 job container"),
        @($workflowPath, $rockyJobText, "cat /etc/os-release", "Rocky verification must expose container distro evidence"),
        @($workflowPath, $rockyJobText, 'if [ "${ID:-}" != "rocky" ]', "Rocky verification must require the Rocky distro identity"),
        @($workflowPath, $rockyJobText, '9|9.*) ;;', "Rocky verification must require Rocky Linux version 9 or 9.x"),
        @($workflowPath, $rockyJobText, "getconf GNU_LIBC_VERSION", "Rocky verification must record the container libc identity"),
        @($workflowPath, $rockyJobText, "ROCKY_9_CONTAINER_EVIDENCE", "Rocky verification must emit an explicit container evidence marker"),
        @($workflowPath, $rockyJobText, "ROCKY_9_ASSEMBLER_EVIDENCE", "Rocky verification must retain assembler evidence for the producer workaround context"),
        @($workflowPath, $rockyJobText, "dnf config-manager --set-enabled crb", "Rocky verification must enable CRB for its native tooling boundary"),
        @($workflowPath, $rockyJobText, "curl-minimal", "Rocky verification must preserve the distro's non-conflicting curl package"),
        @($workflowPath, $rockyJobText, "dnf install -y powershell", "Rocky verification must install PowerShell before invoking repository guards"),
        @($workflowPath, $rockyJobText, "Microsoft publishes the compatible PowerShell RPM feed under its RHEL 9 path", "Rocky verifier must explain the factual PowerShell feed path without changing distro identity"),
        @($workflowPath, $rockyJobText, "10.0.x", "Rocky verification must install .NET 10"),
        @($workflowPath, $rockyJobText, "9.0.x", "Rocky verification must install .NET 9"),
        @($workflowPath, $rockyJobText, "8.0.x", "Rocky verification must install .NET 8"),
        @($workflowPath, $rockyJobText, "name: nupkg-managed", "Rocky verification must download the same-run managed artifact explicitly"),
        @($workflowPath, $rockyJobText, "name: nupkg-rocky.9-x64-full", "Rocky verification must download only the exact Rocky full runtime artifact"),
        @($workflowPath, $rockyJobText, "path: artifacts/pack-targeted-rocky/nupkg-managed", "Rocky managed artifact must use its isolated exact path"),
        @($workflowPath, $rockyJobText, "path: artifacts/pack-targeted-rocky/nupkg-rocky.9-x64-full", "Rocky runtime artifact must use its isolated exact path"),
        @($workflowPath, $rockyJobText, "-ExpectedSyntheticRuntimeInputs false", "Rocky artifact and consumer checks must require real provenance"),
        @($workflowPath, $rockyJobText, "-SelectedRid rocky.9-x64", "Rocky checks must select only the proven Rocky RID"),
        @($workflowPath, $rockyJobText, "-SelectedRuntimeProfile full", "Rocky checks must select only the full profile"),
        @($workflowPath, $rockyJobText, "-RunNativeSmoke", "Rocky consumer verification must execute native calls inside the container"),
        @($workflowPath, $rhelJobText, "verify-targeted-real-rhel:", "Pack workflow must keep a separate RHEL UBI container verification job"),
        @($workflowPath, $rhelJobText, "inputs.rid == 'rhel.9-x64' && inputs.runtime_profile == 'full' && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "RHEL verification must use the exact full-only non-synthetic non-publishing gate"),
        @($workflowPath, $rhelJobText, "runs-on: ubuntu-24.04", "RHEL UBI verification must use the supported hosted runner as Docker host"),
        @($workflowPath, $rhelJobText, "container: registry.access.redhat.com/ubi9/ubi:9.8", "RHEL verification must execute in the audited official UBI 9.8 job container"),
        @($workflowPath, $rhelJobText, "cat /etc/os-release", "RHEL verification must expose container distro evidence"),
        @($workflowPath, $rhelJobText, 'if [ "${ID:-}" != "rhel" ]', "RHEL verification must require the factual RHEL distro identity"),
        @($workflowPath, $rhelJobText, '9|9.*) ;;', "RHEL verification must require RHEL version 9 or 9.x"),
        @($workflowPath, $rhelJobText, '"${PLATFORM_ID:-}" != "platform:el9"', "RHEL verification must require the factual Enterprise Linux 9 platform identity"),
        @($workflowPath, $rhelJobText, "getconf GNU_LIBC_VERSION", "RHEL verification must record the container libc identity"),
        @($workflowPath, $rhelJobText, "RHEL_9_UBI_CONTAINER_EVIDENCE", "RHEL verification must emit explicit distro/version/platform/libc evidence"),
        @($workflowPath, $rhelJobText, "RHEL_9_UBI_REPOSITORY_EVIDENCE", "RHEL verification must emit its available UBI repository boundary"),
        @($workflowPath, $rhelJobText, "ubi-9-baseos-rpms", "RHEL verifier must require UBI BaseOS"),
        @($workflowPath, $rhelJobText, "ubi-9-appstream-rpms", "RHEL verifier must require UBI AppStream"),
        @($workflowPath, $rhelJobText, "ubi-9-codeready-builder-rpms", "RHEL verifier must require UBI CodeReady Builder"),
        @($workflowPath, $rhelJobText, "curl-minimal", "RHEL verifier must preserve the UBI non-conflicting curl package"),
        @($workflowPath, $rhelJobText, "RHEL_9_UBI_ASSEMBLER_EVIDENCE", "RHEL verification must retain assembler evidence for its producer workaround"),
        @($workflowPath, $rhelJobText, "Microsoft's RHEL 9 feed matches this RHEL UBI container and supplies tooling only", "RHEL verifier must describe the Microsoft feed as tooling rather than runtime evidence"),
        @($workflowPath, $rhelJobText, "dnf install -y powershell", "RHEL verification must install PowerShell before invoking repository guards"),
        @($workflowPath, $rhelJobText, "10.0.x", "RHEL verification must install .NET 10"),
        @($workflowPath, $rhelJobText, "9.0.x", "RHEL verification must install .NET 9"),
        @($workflowPath, $rhelJobText, "8.0.x", "RHEL verification must install .NET 8"),
        @($workflowPath, $rhelJobText, "name: nupkg-managed", "RHEL verification must download the same-run managed artifact explicitly"),
        @($workflowPath, $rhelJobText, "name: nupkg-rhel.9-x64-full", "RHEL verification must download only the exact RHEL full runtime artifact"),
        @($workflowPath, $rhelJobText, "path: artifacts/pack-targeted-rhel/nupkg-managed", "RHEL managed artifact must use its isolated exact path"),
        @($workflowPath, $rhelJobText, "path: artifacts/pack-targeted-rhel/nupkg-rhel.9-x64-full", "RHEL runtime artifact must use its isolated exact path"),
        @($workflowPath, $rhelJobText, "-ExpectedSyntheticRuntimeInputs false", "RHEL artifact and consumer checks must require real provenance"),
        @($workflowPath, $rhelJobText, "-SelectedRid rhel.9-x64", "RHEL checks must select only the proven RHEL RID"),
        @($workflowPath, $rhelJobText, "-SelectedRuntimeProfile full", "RHEL checks must select only the full profile"),
        @($workflowPath, $rhelJobText, "-RunNativeSmoke", "RHEL consumer verification must execute native calls inside the UBI container"),
        @($workflowPath, $alpineJobText, "verify-targeted-real-alpine:", "Pack workflow must keep a separate Alpine musl verification job"),
        @($workflowPath, $alpineJobText, "inputs.rid == 'alpine.3.20-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true'", "Alpine verification must use the exact full/mini non-synthetic non-publishing gate"),
        @($workflowPath, $alpineJobText, "runs-on: ubuntu-24.04", "Alpine verification must use a supported Docker host"),
        @($workflowPath, $alpineJobText, "docker run --rm", "Alpine native execution must occur inside an explicit Alpine container while actions stay on the host"),
        @($workflowPath, $alpineJobText, "alpine:3.20", "Alpine verification must execute against the exact Alpine 3.20 image"),
        @($workflowPath, $alpineJobText, "-w /tmp", "Alpine net8 consumer must run outside the repository working directory so the repository .NET 10 global.json does not override the audited Alpine SDK"),
        @($workflowPath, $alpineJobText, "/workspace/scripts/Test-GitHubPackConsumerRestoreSurface.ps1", "Alpine verification must invoke the repository guard explicitly from its isolated working directory"),
        @($workflowPath, $alpineJobText, "cat /etc/os-release", "Alpine verification must expose actual distro evidence"),
        @($workflowPath, $alpineJobText, 'if [ "${ID:-}" != "alpine" ]', "Alpine verification must require the Alpine distro identity"),
        @($workflowPath, $alpineJobText, '3.20|3.20.*) ;;', "Alpine verification must require Alpine version 3.20 or 3.20.x"),
        @($workflowPath, $alpineJobText, '"$(uname -m)" != "x86_64"', "Alpine verification must require the x86_64 architecture"),
        @($workflowPath, $alpineJobText, "/lib/ld-musl-x86_64.so.1", "Alpine verification must derive libc evidence from the musl loader"),
        @($workflowPath, $alpineJobText, "ALPINE_3_20_CONTAINER_EVIDENCE", "Alpine verification must emit explicit distro/version/architecture/musl evidence"),
        @($workflowPath, $alpineJobText, "ALPINE_3_20_PACKAGE_ELF_EVIDENCE", "Alpine verification must audit the exact selected package ELF closure"),
        @($workflowPath, $alpineJobText, "expected_opencv_count=16", "Alpine full package audit must require 16 canonical OpenCV ELFs"),
        @($workflowPath, $alpineJobText, "expected_opencv_count=6", "Alpine mini package audit must require six canonical OpenCV ELFs"),
        @($workflowPath, $alpineJobText, "expected_runtime_file_count=50", "Alpine full package audit must require the exact 50-file payload"),
        @($workflowPath, $alpineJobText, "expected_runtime_file_count=20", "Alpine mini package audit must require the exact 20-file payload"),
        @($workflowPath, $alpineJobText, 'test "$alpine_opencv_count" -eq "$expected_opencv_count"', "Alpine package audit must compare canonical OpenCV ELFs against the selected module count"),
        @($workflowPath, $alpineJobText, "ALPINE_3_20_REPOSITORY_EVIDENCE", "Alpine verification must require the exact v3.20 repositories"),
        @($workflowPath, $alpineJobText, "dotnet8-sdk", "Alpine verification must use the official Alpine .NET 8 SDK package for its net8 consumer"),
        @($workflowPath, $alpineJobText, "powershell", "Alpine verification must use the official Alpine PowerShell package"),
        @($workflowPath, $alpineJobText, "name: nupkg-managed", "Alpine verification must download the same-run managed artifact explicitly"),
        @($workflowPath, $alpineJobText, 'name: nupkg-alpine.3.20-x64-${{ inputs.runtime_profile }}', "Alpine verification must download only the exact selected Alpine runtime artifact"),
        @($workflowPath, $alpineJobText, "path: artifacts/pack-targeted-alpine/nupkg-managed", "Alpine managed artifact must use its isolated exact path"),
        @($workflowPath, $alpineJobText, 'path: artifacts/pack-targeted-alpine/nupkg-alpine.3.20-x64-${{ inputs.runtime_profile }}', "Alpine runtime artifact must use its isolated selected path"),
        @($workflowPath, $alpineJobText, "-ExpectedSyntheticRuntimeInputs false", "Alpine artifact and consumer checks must require real provenance"),
        @($workflowPath, $alpineJobText, "-SelectedRid alpine.3.20-x64", "Alpine checks must select only the proven musl RID"),
        @($workflowPath, $alpineJobText, '-SelectedRuntimeProfile "$RUNTIME_PROFILE"', "Alpine checks must select the exact selected full/mini profile"),
        @($workflowPath, $alpineJobText, "-RunNativeSmoke", "Alpine consumer verification must execute native and deterministic DNN calls inside Alpine"),
        @($workflowPath, $workflowText, "inputs.rid == 'all' && inputs.runtime_profile == 'all'", "Full-matrix artifact and restore verification condition must remain"),
        @($artifactGuardPath, $artifactGuardText, '[string]$SelectedRid = ""', "Artifact guard must support an explicit selected RID"),
        @($artifactGuardPath, $artifactGuardText, '[string]$SelectedRuntimeProfile = ""', "Artifact guard must support an explicit selected profile"),
        @($artifactGuardPath, $artifactGuardText, "Targeted runtime package native payload must exactly match", "Artifact guard must require an exact selected payload"),
        @($artifactGuardPath, $artifactGuardText, "Targeted runtime provenance files must exactly match", "Artifact guard must match provenance files to package files"),
        @($artifactGuardPath, $artifactGuardText, '"libopencv_$module.so"', "Real Linux artifact verification must require the unversioned loader name"),
        @($artifactGuardPath, $artifactGuardText, '"libopencv_$module.so.$openCvBinarySuffix"', "Real Linux artifact verification must require the ABI SONAME"),
        @($artifactGuardPath, $artifactGuardText, '"libopencv_$module.so.$expectedOpenCvVersion"', "Real Linux artifact verification must require the full-version SONAME companion"),
        @($artifactGuardPath, $artifactGuardText, '"opencv_$module$openCvBinarySuffix.dll"', "Windows artifact verification must derive exact upstream module DLL names"),
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
        @($readmePath, $readmeText, "Ubuntu 24.04 x64 full/mini and Ubuntu 22.04 x64 full/mini", "README must document the exact hosted targeted native-execution allowlist"),
        @($readmePath, $readmeText, 'Ubuntu 24.04 ARM64 full runs natively on `ubuntu-24.04-arm`', "README must document the exact native Ubuntu ARM64 consumer boundary"),
        @($readmePath, $readmeText, 'Ubuntu 22.04 ARM64 full runs through a separate host-orchestrated `docker run` verifier', "README must document the exact Ubuntu 22.04 ARM64 container consumer boundary"),
        @($readmePath, $readmeText, 'Debian 12 ARM64 full uses its own host-orchestrated `docker run` verifier on native `ubuntu-24.04-arm`', "README must document the exact Debian ARM64 container consumer boundary"),
        @($readmePath, $readmeText, 'Debian 12 ARM64 full 使用由原生 `ubuntu-24.04-arm` 宿主机编排的独立 `docker run` verifier', "README Chinese text must document the exact Debian ARM64 container consumer boundary"),
        @($readmePath, $readmeText, 'Debian 12 x64 full runs in a separate `debian:12` job container', "README must document the exact Debian x64 container-native consumer execution"),
        @($readmePath, $readmeText, 'Fedora 40 full runs in its own separate `fedora:40` job container', "README must document Fedora container-native consumer execution"),
        @($readmePath, $readmeText, 'Rocky Linux 9 full runs in a fourth separate `rockylinux:9` job container', "README must document Rocky container-native consumer execution"),
        @($readmePath, $readmeText, 'RHEL 9 full runs in a fifth separate official Red Hat UBI 9 job container', "README must document factual RHEL UBI container-native consumer execution"),
        @($readmePath, $readmeText, 'Alpine 3.20 full runs through a separate host-orchestrated `docker run alpine:3.20` verifier', "README must document host-orchestrated Alpine musl consumer execution"),
        @($guidePath, $guideText, "matrix-required modules plus the ordered staged-optional subset recorded in provenance", "Linked runtime guide must document provenance-derived full payload verification"),
        @($guidePath, $guideText, "Ubuntu 24.04 x64 full/mini and Ubuntu 22.04 x64 full/mini", "Linked runtime guide must document the exact hosted targeted native-execution allowlist"),
        @($guidePath, $guideText, 'Ubuntu 24.04 ARM64 full runs natively on `ubuntu-24.04-arm`', "Linked runtime guide must document the exact native Ubuntu ARM64 consumer boundary"),
        @($guidePath, $guideText, 'Ubuntu 22.04 ARM64 full runs through a separate host-orchestrated `docker run` verifier', "Linked runtime guide must document the exact Ubuntu 22.04 ARM64 container consumer boundary"),
        @($guidePath, $guideText, 'Debian 12 ARM64 full runs through its own host-orchestrated `docker run` verifier on native `ubuntu-24.04-arm`', "Linked runtime guide must document the exact Debian ARM64 container consumer boundary"),
        @($guidePath, $guideText, 'Debian 12 ARM64 full 使用由原生 `ubuntu-24.04-arm` 宿主机编排的独立 `docker run` verifier', "Linked runtime guide Chinese text must document the exact Debian ARM64 container consumer boundary"),
        @($guidePath, $guideText, 'Debian 12 x64 full runs in a separate `debian:12` job container', "Linked runtime guide must document the exact Debian x64 container-native consumer execution"),
        @($guidePath, $guideText, 'Fedora 40 full runs in its own separate `fedora:40` job container', "Linked runtime guide must document Fedora container-native consumer execution"),
        @($guidePath, $guideText, 'Rocky Linux 9 full runs in a fourth separate `rockylinux:9` job container', "Linked runtime guide must document Rocky container-native consumer execution"),
        @($guidePath, $guideText, 'RHEL 9 full runs in a fifth separate official Red Hat UBI 9 job container', "Linked runtime guide must document factual RHEL UBI container-native consumer execution"),
        @($guidePath, $guideText, 'Alpine 3.20 full runs through a separate host-orchestrated `docker run alpine:3.20` verifier', "Linked runtime guide must document host-orchestrated Alpine musl consumer execution"))) {
    Assert-Contains -Path $expectation[0] -Text $expectation[1] -Needle $expectation[2] -Issue $expectation[3]
}

Assert-ExactLine `
    -Path $workflowPath `
    -Text $windowsJobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'win-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Windows x64 targeted verification condition must remain exact, full/mini, non-synthetic, and non-publishing"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $windowsJobText `
    -Needle "inputs.rid ==" `
    -ExpectedCount 1 `
    -Issue "Windows x64 verifier must gate on exactly one RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $windowsJobText `
    -Needle "inputs.runtime_profile ==" `
    -ExpectedCount 2 `
    -Issue "Windows x64 verifier must gate on exactly the full and mini runtime profiles"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $windowsJobText `
    -Needle "-SelectedRid win-x64" `
    -ExpectedCount 2 `
    -Issue "Both Windows package guards must select exact win-x64"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $windowsJobText `
    -Needle "-SelectedRuntimeProfile '`${{ inputs.runtime_profile }}'" `
    -ExpectedCount 2 `
    -Issue "Both Windows package guards must select the exact chosen full/mini profile"

foreach ($forbiddenWindowsText in @(
        "win-x86",
        "win-arm64",
        "run-id:",
        "repository:",
        "docker run",
        "qemu",
        "wine",
        "AddDllDirectory",
        "SetDllDirectory",
        "PATH=")) {
    Assert-NotContains `
        -Path $workflowPath `
        -Text $windowsJobText `
        -Needle $forbiddenWindowsText `
        -Issue "Windows package verification must remain native, same-run, full/mini-only, and free of DLL search overrides: $forbiddenWindowsText"
}

Assert-ExactLine `
    -Path $workflowPath `
    -Text $windowsArm64JobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'win-arm64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Windows ARM64 targeted verification condition must remain exact, full/mini, non-synthetic, and non-publishing"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $windowsArm64JobText `
    -Needle "inputs.rid ==" `
    -ExpectedCount 1 `
    -Issue "Windows ARM64 verifier must gate on exactly one RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $windowsArm64JobText `
    -Needle "inputs.runtime_profile ==" `
    -ExpectedCount 2 `
    -Issue "Windows ARM64 verifier must gate on exactly the full and mini runtime profiles"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $windowsArm64JobText `
    -Needle "-SelectedRid win-arm64" `
    -ExpectedCount 2 `
    -Issue "Both Windows ARM64 package guards must select exact win-arm64"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $windowsArm64JobText `
    -Needle "-SelectedRuntimeProfile '`${{ inputs.runtime_profile }}'" `
    -ExpectedCount 2 `
    -Issue "Both Windows ARM64 package guards must select the exact chosen full/mini profile"

foreach ($forbiddenWindowsArm64Text in @(
        "win-x64",
        "win-x86",
        "run-id:",
        "repository:",
        "docker run",
        "qemu",
        "wine",
        "AddDllDirectory",
        "SetDllDirectory",
        "PATH=")) {
    Assert-NotContains `
        -Path $workflowPath `
        -Text $windowsArm64JobText `
        -Needle $forbiddenWindowsArm64Text `
        -Issue "Windows ARM64 package verification must remain native, same-run, full/mini-only, and free of DLL search overrides: $forbiddenWindowsArm64Text"
}

foreach ($forbiddenConsumerText in @("AddDllDirectory", "SetDllDirectory", "OpenCvNativeRuntimeDir", "OPENCV_CSHARP_OPENCV_RUNTIME_ROOT")) {
    Assert-NotContains `
        -Path $consumerGuardPath `
        -Text $consumerGuardText `
        -Needle $forbiddenConsumerText `
        -Issue "Package consumer guard must not introduce a native DLL search override: $forbiddenConsumerText"
}

foreach ($miniNotLinkedNeedle in @(
        'EntryPoint = "jyppx_ocv_imgproc_good_features_to_track_count"',
        'status != -100 || cornerCount != 0',
        'TARGETED_NATIVE_SMOKE_OK core,imgproc,imgcodecs,videoio,not_linked profile=mini')) {
    Assert-Contains `
        -Path $consumerGuardPath `
        -Text $consumerGuardText `
        -Needle $miniNotLinkedNeedle `
        -Issue "Package consumer guard must prove the mini NOT_LINKED compatibility boundary: $miniNotLinkedNeedle"
}

Assert-ExactLine `
    -Path $workflowPath `
    -Text $ubuntuJobText `
    -ExpectedLine "    if: `${{ ((inputs.rid == 'ubuntu.24.04-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini')) || (inputs.rid == 'ubuntu.22.04-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini'))) && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Hosted targeted verification condition must remain exactly the four proven targets"

Assert-ExactLine `
    -Path $workflowPath `
    -Text $ubuntuArm64JobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'ubuntu.24.04-arm64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Ubuntu ARM64 targeted verification condition must remain exactly native Ubuntu 24.04 ARM64 full/mini"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $ubuntuArm64JobText `
    -Needle "inputs.rid ==" `
    -ExpectedCount 1 `
    -Issue "Ubuntu ARM64 verification job must gate on exactly one RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $ubuntuArm64JobText `
    -Needle "inputs.runtime_profile ==" `
    -ExpectedCount 2 `
    -Issue "Ubuntu ARM64 verification job must gate on exactly the full and mini runtime profiles"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $ubuntuArm64JobText `
    -Needle "-SelectedRid ubuntu.24.04-arm64" `
    -ExpectedCount 2 `
    -Issue "Both Ubuntu ARM64 guards must select the exact distro-specific RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $ubuntuArm64JobText `
    -Needle "-SelectedRuntimeProfile '`${{ inputs.runtime_profile }}'" `
    -ExpectedCount 2 `
    -Issue "Both Ubuntu ARM64 guards must select the exact full/mini profile"

Assert-ExactLine `
    -Path $workflowPath `
    -Text $ubuntu2204Arm64JobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'ubuntu.22.04-arm64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Ubuntu 22.04 ARM64 targeted verification condition must remain exact, full/mini, non-synthetic, and non-publishing"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $ubuntu2204Arm64JobText `
    -Needle "inputs.rid ==" `
    -ExpectedCount 1 `
    -Issue "Ubuntu 22.04 ARM64 verifier must gate on exactly one RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $ubuntu2204Arm64JobText `
    -Needle "inputs.runtime_profile ==" `
    -ExpectedCount 2 `
    -Issue "Ubuntu 22.04 ARM64 verifier must gate on exactly full and mini"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $ubuntu2204Arm64JobText `
    -Needle "-SelectedRid ubuntu.22.04-arm64" `
    -ExpectedCount 2 `
    -Issue "Both Ubuntu 22.04 ARM64 guards must select the exact distro RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $ubuntu2204Arm64JobText `
    -Needle '-SelectedRuntimeProfile "$RUNTIME_PROFILE"' `
    -ExpectedCount 2 `
    -Issue "Both Ubuntu 22.04 ARM64 guards must select the exact full/mini profile"

Assert-ExactLine `
    -Path $workflowPath `
    -Text $debianJobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'debian.12-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Debian targeted verification condition must remain exactly Debian 12 x64 full/mini"

Assert-ExactLine `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'fedora.40-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Fedora targeted verification condition must remain exactly Fedora 40 x64 full/mini"

Assert-ExactLine `
    -Path $workflowPath `
    -Text $rockyJobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'rocky.9-x64' && inputs.runtime_profile == 'full' && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Rocky targeted verification condition must remain exactly Rocky 9 x64 full"

Assert-ExactLine `
    -Path $workflowPath `
    -Text $rhelJobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'rhel.9-x64' && inputs.runtime_profile == 'full' && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "RHEL targeted verification condition must remain exactly RHEL 9 x64 full"

Assert-ExactLine `
    -Path $workflowPath `
    -Text $alpineJobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'alpine.3.20-x64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Alpine targeted verification condition must remain exactly Alpine 3.20 x64 full/mini"

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
    -ExpectedCount 2 `
    -Issue "Debian container job must gate on exactly full and mini"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "-SelectedRid debian.12-x64" `
    -ExpectedCount 2 `
    -Issue "Both Debian guards must select the exact Debian RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "-SelectedRuntimeProfile '`${{ inputs.runtime_profile }}'" `
    -ExpectedCount 2 `
    -Issue "Both Debian guards must select the exact full/mini profile"

Assert-ExactLine `
    -Path $workflowPath `
    -Text $debianArm64JobText `
    -ExpectedLine "    if: `${{ inputs.rid == 'debian.12-arm64' && (inputs.runtime_profile == 'full' || inputs.runtime_profile == 'mini') && inputs.validate_synthetic_runtime != 'true' && inputs.publish_github_packages != 'true' }}" `
    -Issue "Debian ARM64 targeted verification condition must remain exact full/mini Debian 12 ARM64"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $debianArm64JobText `
    -Needle "inputs.rid ==" `
    -ExpectedCount 1 `
    -Issue "Debian ARM64 verifier must gate on exactly one RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $debianArm64JobText `
    -Needle "inputs.runtime_profile ==" `
    -ExpectedCount 2 `
    -Issue "Debian ARM64 verifier must gate on exactly full and mini"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $debianArm64JobText `
    -Needle "-SelectedRid debian.12-arm64" `
    -ExpectedCount 2 `
    -Issue "Both Debian ARM64 guards must select the exact Debian ARM64 RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $debianArm64JobText `
    -Needle '-SelectedRuntimeProfile "$RUNTIME_PROFILE"' `
    -ExpectedCount 2 `
    -Issue "Both Debian ARM64 guards must select the exact full/mini profile"

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
    -ExpectedCount 2 `
    -Issue "Fedora container job must gate on exactly full and mini"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "-SelectedRid fedora.40-x64" `
    -ExpectedCount 2 `
    -Issue "Both Fedora guards must select the exact Fedora RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "-SelectedRuntimeProfile '`${{ inputs.runtime_profile }}'" `
    -ExpectedCount 2 `
    -Issue "Both Fedora guards must select the exact full/mini profile"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $rockyJobText `
    -Needle "inputs.rid ==" `
    -ExpectedCount 1 `
    -Issue "Rocky container job must gate on exactly one RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $rockyJobText `
    -Needle "inputs.runtime_profile ==" `
    -ExpectedCount 1 `
    -Issue "Rocky container job must gate on exactly one runtime profile"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $rockyJobText `
    -Needle "-SelectedRid rocky.9-x64" `
    -ExpectedCount 2 `
    -Issue "Both Rocky guards must select the exact Rocky RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $rockyJobText `
    -Needle "-SelectedRuntimeProfile full" `
    -ExpectedCount 2 `
    -Issue "Both Rocky guards must select only the full profile"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $rhelJobText `
    -Needle "inputs.rid ==" `
    -ExpectedCount 1 `
    -Issue "RHEL UBI container job must gate on exactly one RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $rhelJobText `
    -Needle "inputs.runtime_profile ==" `
    -ExpectedCount 1 `
    -Issue "RHEL UBI container job must gate on exactly one runtime profile"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $rhelJobText `
    -Needle "-SelectedRid rhel.9-x64" `
    -ExpectedCount 2 `
    -Issue "Both RHEL guards must select the exact RHEL RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $rhelJobText `
    -Needle "-SelectedRuntimeProfile full" `
    -ExpectedCount 2 `
    -Issue "Both RHEL guards must select only the full profile"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $alpineJobText `
    -Needle "inputs.rid ==" `
    -ExpectedCount 1 `
    -Issue "Alpine verifier must gate on exactly one RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $alpineJobText `
    -Needle "inputs.runtime_profile ==" `
    -ExpectedCount 2 `
    -Issue "Alpine verifier must gate on exactly the full and mini runtime profiles"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $alpineJobText `
    -Needle "-SelectedRid alpine.3.20-x64" `
    -ExpectedCount 2 `
    -Issue "Both Alpine guards must select the exact Alpine RID"

Assert-OccurrenceCount `
    -Path $workflowPath `
    -Text $alpineJobText `
    -Needle '-SelectedRuntimeProfile "$RUNTIME_PROFILE"' `
    -ExpectedCount 2 `
    -Issue "Alpine checks must select the exact selected full/mini profile"

Assert-NotContains `
    -Path $consumerGuardPath `
    -Text $consumerGuardText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "Packaged native consumer must not mask loader RUNPATH defects with an environment override"

foreach ($forbiddenArm64Text in @(
        "LD_LIBRARY_PATH",
        "x86_64",
        "qemu",
        "docker run",
        "container:",
        "run-id:",
        "repository:")) {
    Assert-NotContains `
        -Path $workflowPath `
        -Text $ubuntuArm64JobText `
        -Needle $forbiddenArm64Text `
        -Issue "Ubuntu ARM64 verification must remain native, full/mini-only, same-run, and free of loader overrides: $forbiddenArm64Text"
}

foreach ($forbiddenUbuntu2204Arm64Text in @(
        "LD_LIBRARY_PATH",
        "x86_64",
        "qemu",
        "--platform",
        "container:",
        "run-id:",
        "repository:")) {
    Assert-NotContains `
        -Path $workflowPath `
        -Text $ubuntu2204Arm64JobText `
        -Needle $forbiddenUbuntu2204Arm64Text `
        -Issue "Ubuntu 22.04 ARM64 verification must remain native, full/mini-only, same-run, and free of loader overrides or emulation: $forbiddenUbuntu2204Arm64Text"
}

foreach ($forbiddenDebianArm64Text in @(
        "LD_LIBRARY_PATH",
        "x86_64",
        "qemu",
        "--platform",
        "container:",
        "run-id:",
        "repository:")) {
    Assert-NotContains `
        -Path $workflowPath `
        -Text $debianArm64JobText `
        -Needle $forbiddenDebianArm64Text `
        -Issue "Debian 12 ARM64 verification must remain native, full/mini-only, same-run, and free of loader overrides or emulation: $forbiddenDebianArm64Text"
}

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
    -Text $ubuntuJobText `
    -Needle "rocky.9-x64" `
    -Issue "Rocky must not be folded into the hosted Ubuntu verification allowlist"

Assert-NotContains `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "rocky.9-x64" `
    -Issue "Rocky must not be folded into the Debian container verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "rocky.9-x64" `
    -Issue "Rocky must not be folded into the Fedora container verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $ubuntuJobText `
    -Needle "rhel.9-x64" `
    -Issue "RHEL must not be folded into the hosted Ubuntu verification allowlist"

Assert-NotContains `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "rhel.9-x64" `
    -Issue "RHEL must not be folded into the Debian container verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "rhel.9-x64" `
    -Issue "RHEL must not be folded into the Fedora container verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $debianArm64JobText `
    -Needle "debian.12-x64" `
    -Issue "Debian ARM64 verification must not consume the Debian x64 package"

Assert-NotContains `
    -Path $workflowPath `
    -Text $debianArm64JobText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "Debian ARM64 verification must not mask loader RUNPATH defects with an environment override"

Assert-NotContains `
    -Path $workflowPath `
    -Text $debianJobText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "Debian container verification must not mask loader RUNPATH defects with an environment override"

Assert-Contains `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "runtime_profile == 'mini'" `
    -Issue "Fedora mini must enter the exact container-native verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $fedoraJobText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "Fedora container verification must not mask loader RUNPATH defects with an environment override"

Assert-NotContains `
    -Path $workflowPath `
    -Text $rockyJobText `
    -Needle "runtime_profile == 'mini'" `
    -Issue "Rocky mini must not enter the container-native verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $rockyJobText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "Rocky container verification must not mask loader RUNPATH defects with an environment override"

Assert-NotContains `
    -Path $workflowPath `
    -Text $rockyJobText `
    -Needle "inputs.rid == 'rhel.9-x64'" `
    -Issue "Rocky container verification must not be relabelled as a RHEL RID"

Assert-NotContains `
    -Path $workflowPath `
    -Text $rockyJobText `
    -Needle "-SelectedRid rhel.9-x64" `
    -Issue "Rocky selected package guards must not consume the separate RHEL package identity"

Assert-NotContains `
    -Path $workflowPath `
    -Text $rhelJobText `
    -Needle "runtime_profile == 'mini'" `
    -Issue "RHEL mini must not enter the UBI container-native verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $rhelJobText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "RHEL UBI verification must not mask loader dynamic-path defects with an environment override"

Assert-NotContains `
    -Path $workflowPath `
    -Text $rhelJobText `
    -Needle "rocky.9-x64" `
    -Issue "RHEL verification must not consume or relabel the Rocky package identity"

Assert-NotContains `
    -Path $workflowPath `
    -Text $rhelJobText `
    -Needle "container: rockylinux:9" `
    -Issue "RHEL verification must never use Rocky Linux as RHEL evidence"

Assert-Contains `
    -Path $workflowPath `
    -Text $alpineJobText `
    -Needle "runtime_profile == 'mini'" `
    -Issue "Alpine mini must enter the exact musl verification job"

Assert-NotContains `
    -Path $workflowPath `
    -Text $alpineJobText `
    -Needle "LD_LIBRARY_PATH" `
    -Issue "Alpine verification must not mask package dynamic-path defects with an environment override"

Assert-NotContains `
    -Path $workflowPath `
    -Text $alpineJobText `
    -Needle "container: alpine:3.20" `
    -Issue "Node-based GitHub actions must stay on the glibc host rather than run directly in the musl job container"

Assert-NotContains `
    -Path $workflowPath `
    -Text $alpineJobText `
    -Needle "glibc" `
    -Issue "Alpine verification must not relabel glibc evidence as musl"

Assert-NotContains `
    -Path $workflowPath `
    -Text $alpineJobText `
    -Needle "CV_AVXVNNI_AVAILABLE=0" `
    -Issue "Alpine verification must not inherit the independently scoped Rocky/RHEL AVX-VNNI workaround"

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
Write-Host "Hosted targets: win-x64/full and win-x64/mini on actual Windows x64; win-arm64/full and win-arm64/mini in their separate native Windows ARM64 verifier; ubuntu.24.04-x64/full, ubuntu.24.04-x64/mini, ubuntu.22.04-x64/full, ubuntu.22.04-x64/mini; ubuntu.24.04-arm64/full and ubuntu.24.04-arm64/mini run in their separate native ARM64 verifier."
Write-Host "Container targets: ubuntu.22.04-arm64/full and ubuntu.22.04-arm64/mini through host-orchestrated official Ubuntu 22.04 on native AArch64; debian.12-arm64/full and debian.12-arm64/mini through host-orchestrated official Debian 12 on native AArch64; debian.12-x64/full and debian.12-x64/mini in debian:12; fedora.40-x64/full and fedora.40-x64/mini in fedora:40; rocky.9-x64/full in rockylinux:9; rhel.9-x64/full in official Red Hat UBI 9.8; alpine.3.20-x64/full and alpine.3.20-x64/mini through host-orchestrated alpine:3.20."
Write-Host "All targeted execution is non-synthetic and non-publishing."
Write-Host "Packaged native smoke modules: mini core,imgproc,imgcodecs,videoio plus NOT_LINKED compatibility evidence; full adds dnn."
