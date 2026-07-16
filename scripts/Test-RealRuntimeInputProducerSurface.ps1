param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$producerWorkflowPath = ".github/workflows/runtime-input.yml"
$runtimeInputScriptPath = "scripts/New-RuntimeInputArtifact.ps1"
$packWorkflowPath = ".github/workflows/pack.yml"
$runtimeMatrixPath = "packaging/runtime/runtime-package-matrix.json"
$readmePath = "README.md"
$linkedRuntimeBuildGuidePath = "docs/articles/linked-runtime-build-guide.md"
$versionNeutralGuidePath = "docs/articles/version-neutral-naming-guide.md"

function Add-Violation {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Read-RequiredText {
    param([Parameter(Mandatory = $true)][string]$RelativePath)

    $path = Join-Path $repo $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required real runtime input producer file was not found: $RelativePath"
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
        [string]$Issue
    )

    if ($Text.IndexOf($Needle, [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
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
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text $Needle
    }
}

function Assert-TextOrder {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [Parameter(Mandatory = $true)]
        [string]$Text,
        [Parameter(Mandatory = $true)]
        [string]$Earlier,
        [Parameter(Mandatory = $true)]
        [string]$Later,
        [Parameter(Mandatory = $true)]
        [string]$Issue
    )

    $earlierIndex = $Text.IndexOf($Earlier, [System.StringComparison]::OrdinalIgnoreCase)
    $laterIndex = $Text.IndexOf($Later, [System.StringComparison]::OrdinalIgnoreCase)
    if ($earlierIndex -lt 0 -or $laterIndex -lt 0 -or $earlierIndex -ge $laterIndex) {
        Add-Violation -Violations $Violations -Path $Path -Issue $Issue -Text "$Earlier before $Later"
    }
}

function Write-FixtureFile {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,
        [string]$Text = "fixture"
    )

    $parent = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $parent | Out-Null
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [System.IO.File]::WriteAllText($Path, $Text, $utf8NoBom)
}

function Convert-YamlScalar {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Value
    )

    $trimmed = ($Value -replace "\s+#.*$", "").Trim()
    if ($trimmed.Length -ge 2) {
        $first = $trimmed[0]
        $last = $trimmed[$trimmed.Length - 1]
        if (($first -eq '"' -and $last -eq '"') -or ($first -eq "'" -and $last -eq "'")) {
            return $trimmed.Substring(1, $trimmed.Length - 2)
        }
    }

    return $trimmed
}

function Get-RuntimeInputProducerTargets {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    $targets = [System.Collections.Generic.List[object]]::new()
    $lines = [System.Text.RegularExpressions.Regex]::Split($Text, "\r?\n")
    $inProduceJob = $false
    $current = $null

    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = $lines[$index]

        if ($line -match "^\s{2}([A-Za-z0-9_-]+):\s*$") {
            if ($null -ne $current) {
                $targets.Add($current)
                $current = $null
            }

            $jobName = [string]$Matches[1]
            $inProduceJob = $jobName.StartsWith("produce", [System.StringComparison]::Ordinal)
            continue
        }

        if (-not $inProduceJob) {
            continue
        }

        if ($line -match "^\s{10}-\s+rid:\s*(.+?)\s*$") {
            if ($null -ne $current) {
                $targets.Add($current)
            }

            $current = [pscustomobject]@{
                Rid = Convert-YamlScalar -Value $Matches[1]
                Profile = ""
                Runner = ""
                ContainerImage = ""
                OpenCvExtraCMakeArgs = ""
            }
            continue
        }

        if ($null -ne $current) {
            if ($line -match "^\s{12}profile:\s*(.+?)\s*$") {
                $current.Profile = Convert-YamlScalar -Value $Matches[1]
                continue
            }

            if ($line -match "^\s{12}os:\s*(.+?)\s*$") {
                $current.Runner = Convert-YamlScalar -Value $Matches[1]
                continue
            }

            if ($line -match "^\s{12}container_image:\s*(.+?)\s*$") {
                $current.ContainerImage = Convert-YamlScalar -Value $Matches[1]
                continue
            }

            if ($line -match "^\s{12}opencv_extra_cmake_args:\s*(.*?)\s*$") {
                $current.OpenCvExtraCMakeArgs = Convert-YamlScalar -Value $Matches[1]
                continue
            }
        }
    }

    if ($null -ne $current) {
        $targets.Add($current)
    }

    return @($targets)
}

function Assert-RealProducerTargets {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory = $true)]
        [string]$ProducerWorkflowText,
        [Parameter(Mandatory = $true)]
        [string]$ProducerWorkflowPath,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeMatrixText,
        [Parameter(Mandatory = $true)]
        [string]$RuntimeMatrixPath
    )

    $expectedTargets = @(
        [pscustomobject]@{ Rid = "ubuntu.24.04-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "ubuntu.24.04-x64"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "ubuntu.22.04-x64"; Profile = "full"; Runner = "ubuntu-22.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "debian.12-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = "debian:12"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "fedora.40-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = "fedora:40"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "rhel.9-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = "registry.access.redhat.com/ubi9/ubi:9.8"; OpenCvExtraCMakeArgs = "-DCMAKE_CXX_FLAGS=-DCV_AVXVNNI_AVAILABLE=0" },
        [pscustomobject]@{ Rid = "rocky.9-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = "rockylinux:9"; OpenCvExtraCMakeArgs = "-DCMAKE_CXX_FLAGS=-DCV_AVXVNNI_AVAILABLE=0" },
        [pscustomobject]@{ Rid = "alpine.3.20-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = "alpine:3.20"; OpenCvExtraCMakeArgs = "" }
    )
    $expectedByKey = @{}
    foreach ($target in $expectedTargets) {
        $expectedByKey["$($target.Rid)|$($target.Profile)"] = $target
    }

    $workflowTargets = @(Get-RuntimeInputProducerTargets -Text $ProducerWorkflowText)
    if ($workflowTargets.Count -eq 0) {
        Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Runtime input workflow must declare explicit real producer target matrix entries"
        return
    }

    $seenByKey = @{}
    foreach ($target in $workflowTargets) {
        $key = "$($target.Rid)|$($target.Profile)"
        if ([string]::IsNullOrWhiteSpace([string]$target.Rid) -or
            [string]::IsNullOrWhiteSpace([string]$target.Profile) -or
            [string]::IsNullOrWhiteSpace([string]$target.Runner)) {
            Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Every real producer target must declare rid, profile, and os" -Text "$key|$($target.Runner)"
            continue
        }

        if ($seenByKey.ContainsKey($key)) {
            Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Runtime input workflow contains duplicate real producer target $key" -Text $target.Runner
            continue
        }

        $seenByKey[$key] = $target
        if (-not $expectedByKey.ContainsKey($key)) {
            Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Runtime input workflow advertises unsupported real producer target" -Text "$key on $($target.Runner)"
            continue
        }

        $expected = $expectedByKey[$key]
        if (-not ([string]$target.Runner).Equals([string]$expected.Runner, [System.StringComparison]::Ordinal)) {
            Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Runtime input workflow target runner must match the approved real producer host" -Text "$key workflow=$($target.Runner); expected=$($expected.Runner)"
        }

        if (-not ([string]$target.ContainerImage).Equals([string]$expected.ContainerImage, [System.StringComparison]::Ordinal)) {
            Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Runtime input workflow target container image must match the approved real producer boundary" -Text "$key workflow=$($target.ContainerImage); expected=$($expected.ContainerImage)"
        }

        if (-not ([string]$target.OpenCvExtraCMakeArgs).Equals([string]$expected.OpenCvExtraCMakeArgs, [System.StringComparison]::Ordinal)) {
            Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Runtime input workflow target OpenCV CMake arguments must match the approved distro toolchain boundary" -Text "$key workflow=$($target.OpenCvExtraCMakeArgs); expected=$($expected.OpenCvExtraCMakeArgs)"
        }
    }

    foreach ($expected in $expectedTargets) {
        $key = "$($expected.Rid)|$($expected.Profile)"
        if (-not $seenByKey.ContainsKey($key)) {
            Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Runtime input workflow is missing approved real producer target $key" -Text $expected.Runner
        }
    }

    $matrix = $RuntimeMatrixText | ConvertFrom-Json
    foreach ($target in $expectedTargets) {
        $ridSpec = @($matrix.rids | Where-Object { $_.rid -eq $target.Rid } | Select-Object -First 1)
        if ($ridSpec.Count -eq 0) {
            Add-Violation -Violations $Violations -Path $RuntimeMatrixPath -Issue "Approved real producer target RID is missing from runtime package matrix" -Text $target.Rid
            continue
        }

        if (-not ([string]$ridSpec[0].runner).Equals($target.Runner, [System.StringComparison]::Ordinal)) {
            Add-Violation -Violations $Violations -Path $RuntimeMatrixPath -Issue "Approved real producer runner must match runtime package matrix runner" -Text "$($target.Rid): producer=$($target.Runner); matrix=$($ridSpec[0].runner)"
        }

        if (-not ([string]$ridSpec[0].platformFamily).Equals("linux", [System.StringComparison]::OrdinalIgnoreCase)) {
            Add-Violation -Violations $Violations -Path $RuntimeMatrixPath -Issue "Approved distro real producer target must remain a Linux runtime package RID" -Text $target.Rid
        }

        $distro = [string]$ridSpec[0].distro
        if ([string]::IsNullOrWhiteSpace($distro)) {
            Add-Violation -Violations $Violations -Path $RuntimeMatrixPath -Issue "Approved distro real producer target must record a distro in the runtime matrix" -Text $target.Rid
        }

        if ($distro.Equals("ubuntu", [System.StringComparison]::OrdinalIgnoreCase)) {
            if (-not [string]::IsNullOrWhiteSpace([string]$target.ContainerImage)) {
                Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Ubuntu real producer targets should remain hosted-runner native until deliberately converted" -Text "$($target.Rid): $($target.ContainerImage)"
            }
        }
        else {
            if ([string]::IsNullOrWhiteSpace([string]$target.ContainerImage)) {
                Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Non-Ubuntu real producer targets must declare a distro-native container image" -Text $target.Rid
            }

            # Exact approved image matching above handles registries whose repository name differs from /etc/os-release ID, such as rockylinux:9 with ID=rocky.
        }

        $profileSpec = @($matrix.profiles | Where-Object { $_.name -eq $target.Profile } | Select-Object -First 1)
        if ($profileSpec.Count -eq 0) {
            Add-Violation -Violations $Violations -Path $RuntimeMatrixPath -Issue "Approved real producer target profile is missing from runtime package matrix" -Text $target.Profile
        }
    }
}

$violations = [System.Collections.Generic.List[object]]::new()

$producerWorkflowText = Read-RequiredText -RelativePath $producerWorkflowPath
$runtimeInputScriptText = Read-RequiredText -RelativePath $runtimeInputScriptPath
$packWorkflowText = Read-RequiredText -RelativePath $packWorkflowPath
$runtimeMatrixText = Read-RequiredText -RelativePath $runtimeMatrixPath
$readmeText = Read-RequiredText -RelativePath $readmePath
$linkedRuntimeBuildGuideText = Read-RequiredText -RelativePath $linkedRuntimeBuildGuidePath
$versionNeutralGuideText = Read-RequiredText -RelativePath $versionNeutralGuidePath

foreach ($required in @(
        [pscustomobject]@{ Needle = "name: runtime-input"; Issue = "Producer workflow must have a neutral runtime-input name" },
        [pscustomobject]@{ Needle = "workflow_dispatch:"; Issue = "Producer workflow must be manually dispatched until real build cost is proven" },
        [pscustomobject]@{ Needle = "default: ubuntu.24.04-x64"; Issue = "Producer workflow must start with the first real Ubuntu 24.04 x64 target" },
        [pscustomobject]@{ Needle = "default: full"; Issue = "Producer workflow must keep full as the default while mini remains an explicit target" },
        [pscustomobject]@{ Needle = "validate-target:"; Issue = "Producer workflow must reject unsupported real producer targets before matrix work starts" },
        [pscustomobject]@{ Needle = "runs-on: `${{ matrix.os }}"; Issue = "Producer workflow must run real target builds on the target matrix runner" },
        [pscustomobject]@{ Needle = "Skip unmatched producer target"; Issue = "Producer workflow matrix must skip unmatched target rows explicitly" },
        [pscustomobject]@{ Needle = "Skip unmatched container producer target"; Issue = "Producer workflow must skip unmatched container target rows explicitly" },
        [pscustomobject]@{ Needle = "Check project invariants"; Issue = "Producer workflow must run project invariants before building runtime inputs" },
        [pscustomobject]@{ Needle = "runtime-input-ubuntu.24.04-x64-mini"; Issue = "Producer workflow must explicitly advertise the first real mini producer target" },
        [pscustomobject]@{ Needle = "container_image: debian:12"; Issue = "Producer workflow must declare the Debian 12 container-native boundary" },
        [pscustomobject]@{ Needle = "container_image: fedora:40"; Issue = "Producer workflow must declare the Fedora 40 container-native boundary" },
        [pscustomobject]@{ Needle = "container_image: registry.access.redhat.com/ubi9/ubi:9.8"; Issue = "Producer workflow must declare the official RHEL UBI 9.8 container-native boundary" },
        [pscustomobject]@{ Needle = "container_image: rockylinux:9"; Issue = "Producer workflow must declare the Rocky Linux 9 container-native boundary" },
        [pscustomobject]@{ Needle = "container_image: alpine:3.20"; Issue = "Producer workflow must declare the exact Alpine 3.20 musl boundary" },
        [pscustomobject]@{ Needle = 'container_shell="sh"'; Issue = "Alpine producer must bootstrap with the base image's available POSIX shell" },
        [pscustomobject]@{ Needle = "ALPINE_3_20_REPOSITORY_EVIDENCE"; Issue = "Alpine producer must require factual v3.20 main/community repository evidence" },
        [pscustomobject]@{ Needle = 'musl_banner="$("$musl_loader" 2>&1)" || musl_status=$?'; Issue = "Alpine producer must tolerate only the musl loader's expected version-banner exit path" },
        [pscustomobject]@{ Needle = 'test "$musl_status" -eq 1'; Issue = "Alpine producer must assert the musl loader version-banner exit code before parsing evidence" },
        [pscustomobject]@{ Needle = "ALPINE_3_20_MUSL_EVIDENCE"; Issue = "Alpine producer must emit actual distro/version/architecture/musl evidence" },
        [pscustomobject]@{ Needle = "ALPINE_3_20_ASSEMBLER_EVIDENCE"; Issue = "Alpine producer must report its independently audited assembler" },
        [pscustomobject]@{ Needle = "ALPINE_3_20_AVXVNNI_EVIDENCE supported with no OpenCV CMake workaround"; Issue = "Alpine producer must prove its own AVX-VNNI path without copying the RPM-family workaround" },
        [pscustomobject]@{ Needle = "linux-headers"; Issue = "Alpine producer must install Linux headers required by OpenCV core" },
        [pscustomobject]@{ Needle = "samurai"; Issue = "Alpine producer must install the audited Ninja-compatible build tool" },
        [pscustomobject]@{ Needle = "ALPINE_3_20_PRODUCER_ELF_EVIDENCE files=18 origin=18 direct_opencv=16"; Issue = "Alpine producer must audit both loaders and the full canonical ELF closure before upload" },
        [pscustomobject]@{ Needle = "fedora|rhel|rocky)"; Issue = "Producer workflow must install the audited RPM-family build dependencies" },
        [pscustomobject]@{ Needle = "dnf config-manager --set-enabled crb"; Issue = "Producer workflow must enable Rocky Linux CRB before installing ninja-build" },
        [pscustomobject]@{ Needle = "RHEL_9_UBI_REPOSITORY_EVIDENCE"; Issue = "Producer workflow must require the audited UBI BaseOS, AppStream, and CodeReady Builder repositories" },
        [pscustomobject]@{ Needle = "ubi-9-codeready-builder-rpms"; Issue = "RHEL producer must retain the factual UBI CodeReady Builder repository boundary for ninja-build" },
        [pscustomobject]@{ Needle = 'curl_package="curl-minimal"'; Issue = "Producer workflow must preserve the audited Rocky and RHEL non-conflicting curl package" },
        [pscustomobject]@{ Needle = "as --version"; Issue = "Producer workflow must install and report the distro assembler used for OpenCV CPU-dispatch code" },
        [pscustomobject]@{ Needle = 'opencv_extra_cmake_args: "-DCMAKE_CXX_FLAGS=-DCV_AVXVNNI_AVAILABLE=0"'; Issue = "Producer workflow must disable only independently audited unsupported GCC 11 AVX-VNNI DNN paths" },
        [pscustomobject]@{ Needle = '-ExtraCMakeArgs "$OPENCV_EXTRA_CMAKE_ARGS"'; Issue = "Producer workflow must pass distro-specific OpenCV CMake arguments into the real build" },
        [pscustomobject]@{ Needle = "docker run --rm"; Issue = "Producer workflow must execute non-Ubuntu producer work inside the distro container" },
        [pscustomobject]@{ Needle = "EXPECTED_DISTRO_VERSION"; Issue = "Producer workflow must carry runtime matrix distro version into the container boundary" },
        [pscustomobject]@{ Needle = "Container distro mismatch for `$PRODUCER_RID"; Issue = "Producer workflow must reject container images whose actual distro does not match the runtime RID matrix" },
        [pscustomobject]@{ Needle = "Container distro version mismatch for `$PRODUCER_RID"; Issue = "Producer workflow must reject container images whose actual distro version does not match the runtime RID matrix" },
        [pscustomobject]@{ Needle = "getconf GNU_LIBC_VERSION"; Issue = "Producer workflow must record libc evidence for container-native Linux outputs" },
        [pscustomobject]@{ Needle = "git -c advice.detachedHead=false clone --depth 1 --branch"; Issue = "Producer workflow must fetch factual OpenCV source for real runtime inputs" },
        [pscustomobject]@{ Needle = "https://github.com/opencv/opencv.git"; Issue = "Producer workflow must fetch OpenCV from the upstream source repository" },
        [pscustomobject]@{ Needle = "./scripts/Build-OpenCV.ps1"; Issue = "Producer workflow must build OpenCV runtime inputs" },
        [pscustomobject]@{ Needle = "-Build"; Issue = "Producer workflow must run the OpenCV build/install target, not only describe it" },
        [pscustomobject]@{ Needle = "OPENCV_CSHARP_OPENCV_DIR"; Issue = "Producer workflow must link native wrapper against produced OpenCV config" },
        [pscustomobject]@{ Needle = "OPENCV_CSHARP_RUNTIME_PROFILE"; Issue = "Producer workflow must pass the selected runtime profile to native CMake" },
        [pscustomobject]@{ Needle = "lib64/cmake/opencv5"; Issue = "Producer workflow must probe lib64 OpenCVConfig.cmake for Fedora-style Linux installs" },
        [pscustomobject]@{ Needle = "open_cv_install_dir/lib64"; Issue = "Producer workflow must probe lib64 runtime directories for Fedora-style Linux installs" },
        [pscustomobject]@{ Needle = "cmake --build build/native-linked"; Issue = "Producer workflow must build the linked native wrapper" },
        [pscustomobject]@{ Needle = "ctest --test-dir build/native-linked"; Issue = "Producer workflow must test the linked native wrapper" },
        [pscustomobject]@{ Needle = "sudo apt-get install -y binutils ninja-build"; Issue = "Hosted Linux producers must install readelf for runtime dynamic-path auditing" },
        [pscustomobject]@{ Needle = "./scripts/New-RuntimeInputArtifact.ps1"; Issue = "Producer workflow must assemble the agreed handoff layout" },
        [pscustomobject]@{ Needle = 'runtime-input-${{ matrix.rid }}-${{ matrix.profile }}'; Issue = "Producer workflow must upload neutral runtime-input artifact names" },
        [pscustomobject]@{ Needle = 'artifacts/runtime-inputs/${{ matrix.rid }}-${{ matrix.profile }}'; Issue = "Producer workflow must upload the agreed runtime-input layout root" })) {
    Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle $required.Needle -Issue $required.Issue
}

Assert-RealProducerTargets `
    -Violations $violations `
    -ProducerWorkflowText $producerWorkflowText `
    -ProducerWorkflowPath $producerWorkflowPath `
    -RuntimeMatrixText $runtimeMatrixText `
    -RuntimeMatrixPath $runtimeMatrixPath

Assert-Contains -Violations $violations -Path $runtimeMatrixPath -Text $runtimeMatrixText -Needle "Alpine 3.20 standard support ended on 2026-04-01 and fixes are now on request" -Issue "Alpine runtime matrix guidance must preserve the factual upstream lifecycle boundary"

Assert-TextOrder -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Earlier "Build OpenCV runtime" -Later "Configure linked native wrapper" -Issue "Producer workflow must build OpenCV before configuring the linked native wrapper"
Assert-TextOrder -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Earlier "Build linked native wrapper" -Later "Create runtime input artifact layout" -Issue "Producer workflow must build native wrapper before assembling the artifact"
Assert-TextOrder -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Earlier "Create runtime input artifact layout" -Later "Upload runtime input artifact" -Issue "Producer workflow must assemble the artifact before upload"

Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "New-SyntheticRuntimeInputs.ps1" -Issue "Producer workflow must not use synthetic runtime input generation"
Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "publish_github_packages" -Issue "Producer workflow must not publish packages"
Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "dotnet nuget push" -Issue "Producer workflow must not push packages"

foreach ($required in @(
        [pscustomobject]@{ Needle = '[string]$OutputRoot = "artifacts/runtime-inputs"'; Issue = "Runtime input artifact script must use a neutral generated output root" },
        [pscustomobject]@{ Needle = "native-wrapper"; Issue = "Runtime input artifact script must create native-wrapper layout" },
        [pscustomobject]@{ Needle = "opencv-runtime"; Issue = "Runtime input artifact script must create opencv-runtime layout" },
        [pscustomobject]@{ Needle = "opencv-source"; Issue = "Runtime input artifact script must create opencv-source layout" },
        [pscustomobject]@{ Needle = "opencv-install"; Issue = "Runtime input artifact script must create optional opencv-install layout" },
        [pscustomobject]@{ Needle = "SyntheticRuntimeInputs = `$false"; Issue = "Runtime input artifact provenance must mark produced handoff as non-synthetic" },
        [pscustomobject]@{ Needle = "runtime-input.provenance.json"; Issue = "Runtime input artifact script must write handoff provenance" },
        [pscustomobject]@{ Needle = "PlatformFamily = Get-OptionalStringProperty"; Issue = "Runtime input artifact provenance must record platform family from the runtime matrix" },
        [pscustomobject]@{ Needle = "Distro = Get-OptionalStringProperty"; Issue = "Runtime input artifact provenance must record distro from the runtime matrix" },
        [pscustomobject]@{ Needle = "DistroVersion = Get-OptionalStringProperty"; Issue = "Runtime input artifact provenance must record distro version from the runtime matrix" },
        [pscustomobject]@{ Needle = "MatrixRunner = Get-OptionalStringProperty"; Issue = "Runtime input artifact provenance must record the matrix runner from the runtime matrix" },
        [pscustomobject]@{ Needle = "HostedRunner = `$HostedRunner"; Issue = "Runtime input artifact provenance must record the hosted runner used by the producer workflow" },
        [pscustomobject]@{ Needle = "ContainerImage = `$ContainerImage"; Issue = "Runtime input artifact provenance must record the container image for container-native producers" },
        [pscustomobject]@{ Needle = "ContainerDistro = `$ContainerDistro"; Issue = "Runtime input artifact provenance must record actual container distro evidence" },
        [pscustomobject]@{ Needle = "ContainerDistroVersion = `$ContainerDistroVersion"; Issue = "Runtime input artifact provenance must record actual container distro version evidence" },
        [pscustomobject]@{ Needle = "ContainerLibc = `$ContainerLibc"; Issue = "Runtime input artifact provenance must record libc evidence for Linux container builds" },
        [pscustomobject]@{ Needle = "OpenCvExtraCMakeArgs = `$OpenCvExtraCMakeArgs"; Issue = "Runtime input artifact provenance must record distro-specific OpenCV CMake arguments" },
        [pscustomobject]@{ Needle = "BuildList = Get-OptionalStringProperty"; Issue = "Runtime input artifact provenance must record the profile build list from the runtime matrix" },
        [pscustomobject]@{ Needle = "JYPPX.OpenCV.Native"; Issue = "Runtime input artifact script must require the neutral native loader" },
        [pscustomobject]@{ Needle = '"Open" + "Cv5Sharp.Native" # compatibility loader for already-compiled consumers'; Issue = "Runtime input artifact script must keep compatibility loader explicitly scoped" },
        [pscustomobject]@{ Needle = "OpenCV source LICENSE was not found"; Issue = "Runtime input artifact script must require OpenCV source license evidence" },
        [pscustomobject]@{ Needle = "Assert-NoAbsoluteElfRuntimePaths"; Issue = "Runtime input artifact script must audit real Linux ELF dynamic paths" },
        [pscustomobject]@{ Needle = 'ELF runtime contains an absolute RPATH/RUNPATH entry'; Issue = "Runtime input artifact script must reject producer absolute dynamic paths" },
        [pscustomobject]@{ Needle = "Runtime input artifact name: runtime-input-`$Rid-`$RuntimeProfile"; Issue = "Runtime input artifact script must print the neutral artifact name" })) {
    Assert-Contains -Violations $violations -Path $runtimeInputScriptPath -Text $runtimeInputScriptText -Needle $required.Needle -Issue $required.Issue
}

Assert-NotContains -Violations $violations -Path $runtimeInputScriptPath -Text $runtimeInputScriptText -Needle "New-SyntheticRuntimeInputs" -Issue "Runtime input artifact script must not call synthetic input generation"

Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "real_runtime_artifact_run_id" -Issue "Pack workflow must keep consuming producer run ids"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle 'runtime-input-${{ matrix.rid }}-${{ matrix.profile }}' -Issue "Pack workflow must consume the same neutral producer artifact names"

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText })) {
    foreach ($needle in @(
            '`runtime-input.yml`',
            '`runtime-input-ubuntu.24.04-x64-full`',
            '`runtime-input-ubuntu.24.04-x64-mini`',
            '`runtime-input-ubuntu.22.04-x64-full`',
            '`runtime-input-debian.12-x64-full`',
            '`runtime-input-fedora.40-x64-full`',
            '`runtime-input-rhel.9-x64-full`',
            '`runtime-input-rocky.9-x64-full`',
            '`runtime-input-alpine.3.20-x64-full`',
            '`runtime-input-<rid>-<profile>`',
            '`native-wrapper/`',
            '`opencv-runtime/`',
            '`opencv-source/`')) {
        Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $needle -Issue "$($doc.Path) must document real runtime input producer text '$needle'"
    }
}

Assert-Contains -Violations $violations -Path $versionNeutralGuidePath -Text $versionNeutralGuideText -Needle "Test-RealRuntimeInputProducerSurface.ps1" -Issue "Version-neutral guide must list the real runtime input producer guard"

if ($violations.Count -eq 0) {
    $fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("opencv-csharp-runtime-input-fixture-" + [System.Guid]::NewGuid().ToString("N"))
    try {
        $fixtureNativeDir = Join-Path $fixtureRoot "native"
        $fixtureSourceDir = Join-Path $fixtureRoot "opencv-source"
        $fixtureInstallDir = Join-Path $fixtureRoot "opencv-install"
        $fixtureOutputRoot = Join-Path $fixtureRoot "out"

        Write-FixtureFile -Path (Join-Path $fixtureNativeDir "libJYPPX.OpenCV.Native.so")
        Write-FixtureFile -Path (Join-Path $fixtureNativeDir "libOpenCv5Sharp.Native.so")
        Write-FixtureFile -Path (Join-Path $fixtureSourceDir "LICENSE") -Text "OpenCV license fixture"
        Write-FixtureFile -Path (Join-Path (Join-Path (Join-Path $fixtureSourceDir "3rdparty") "ippicv") "readme.htm") -Text "ippicv fixture"
        Write-FixtureFile -Path (Join-Path (Join-Path $fixtureInstallDir "etc/licenses") "opencv-license.txt") -Text "install license fixture"

        $matrix = Get-Content -LiteralPath (Join-Path $repo "packaging/runtime/runtime-package-matrix.json") -Raw | ConvertFrom-Json
        foreach ($producerTarget in @(Get-RuntimeInputProducerTargets -Text $producerWorkflowText)) {
            $ridSpec = @($matrix.rids | Where-Object { $_.rid -eq $producerTarget.Rid } | Select-Object -First 1)
            if ($ridSpec.Count -eq 0) {
                throw "Fixture producer target RID was not found in runtime matrix: $($producerTarget.Rid)"
            }

            $expectedDistro = [string]$ridSpec[0].distro
            $expectedDistroVersion = [string]$ridSpec[0].distroVersion
            $containerDistro = if ([string]::IsNullOrWhiteSpace([string]$producerTarget.ContainerImage)) { "" } else { $expectedDistro }
            $containerDistroVersion = if ([string]::IsNullOrWhiteSpace([string]$producerTarget.ContainerImage)) { "" } else { $expectedDistroVersion }
            $containerLibc = if ([string]::IsNullOrWhiteSpace([string]$producerTarget.ContainerImage)) {
                ""
            }
            elseif ($expectedDistro.Equals("alpine", [System.StringComparison]::OrdinalIgnoreCase)) {
                "musl fixture"
            }
            else {
                "glibc fixture"
            }
            $profileSpec = @($matrix.profiles | Where-Object { $_.name -eq $producerTarget.Profile } | Select-Object -First 1)
            if ($profileSpec.Count -eq 0) {
                throw "Fixture producer profile was not found in runtime matrix: $($producerTarget.Profile)"
            }

            $fixtureRuntimeDir = Join-Path $fixtureRoot "opencv-runtime-$($producerTarget.Rid)-$($producerTarget.Profile)"
            foreach ($module in @($profileSpec[0].modules)) {
                Write-FixtureFile -Path (Join-Path $fixtureRuntimeDir "libopencv_$module.so.5.0.0")
            }

            & (Join-Path $repo $runtimeInputScriptPath) `
                -Rid ([string]$producerTarget.Rid) `
                -RuntimeProfile ([string]$producerTarget.Profile) `
                -OpenCvVersion "5.0.0" `
                -NativeRuntimeDir $fixtureNativeDir `
                -OpenCvRuntimeDir $fixtureRuntimeDir `
                -OpenCvSourceDir $fixtureSourceDir `
                -OpenCvInstallDir $fixtureInstallDir `
                -HostedRunner ([string]$producerTarget.Runner) `
                -ContainerImage ([string]$producerTarget.ContainerImage) `
                -ContainerDistro $containerDistro `
                -ContainerDistroVersion $containerDistroVersion `
                -ContainerLibc $containerLibc `
                -OpenCvExtraCMakeArgs ([string]$producerTarget.OpenCvExtraCMakeArgs) `
                -OutputRoot $fixtureOutputRoot

            $manifestPath = Join-Path (Join-Path $fixtureOutputRoot "$($producerTarget.Rid)-$($producerTarget.Profile)") "runtime-input.provenance.json"
            if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
                throw "Fixture runtime input provenance was not written: $manifestPath"
            }

            $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
            if ($manifest.SyntheticRuntimeInputs -ne $false) {
                throw "Fixture provenance did not mark SyntheticRuntimeInputs=false for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if (-not ([string]$manifest.PlatformFamily).Equals("linux", [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "Fixture provenance did not record linux PlatformFamily for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if (-not ([string]$manifest.Distro).Equals($expectedDistro, [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "Fixture provenance Distro did not match runtime matrix for $($producerTarget.Rid)/$($producerTarget.Profile). Expected $expectedDistro, got $($manifest.Distro)."
            }

            if (-not ([string]$manifest.DistroVersion).Equals($expectedDistroVersion, [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "Fixture provenance DistroVersion did not match runtime matrix for $($producerTarget.Rid)/$($producerTarget.Profile). Expected $expectedDistroVersion, got $($manifest.DistroVersion)."
            }

            if (-not ([string]$manifest.MatrixRunner).Equals([string]$producerTarget.Runner, [System.StringComparison]::Ordinal)) {
                throw "Fixture provenance MatrixRunner did not match producer target runner for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if (-not ([string]$manifest.HostedRunner).Equals([string]$producerTarget.Runner, [System.StringComparison]::Ordinal)) {
                throw "Fixture provenance HostedRunner did not match producer target runner for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if (-not ([string]$manifest.ContainerImage).Equals([string]$producerTarget.ContainerImage, [System.StringComparison]::Ordinal)) {
                throw "Fixture provenance ContainerImage did not match producer target container image for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if (-not ([string]$manifest.OpenCvExtraCMakeArgs).Equals([string]$producerTarget.OpenCvExtraCMakeArgs, [System.StringComparison]::Ordinal)) {
                throw "Fixture provenance OpenCvExtraCMakeArgs did not match producer target build arguments for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if ([string]::IsNullOrWhiteSpace([string]$producerTarget.ContainerImage)) {
                if (-not [string]::IsNullOrWhiteSpace([string]$manifest.ContainerDistro) -or
                    -not [string]::IsNullOrWhiteSpace([string]$manifest.ContainerDistroVersion) -or
                    -not [string]::IsNullOrWhiteSpace([string]$manifest.ContainerLibc)) {
                    throw "Fixture provenance should not record container-only fields for hosted producer $($producerTarget.Rid)/$($producerTarget.Profile)."
                }
            }
            else {
                if (-not ([string]$manifest.ContainerDistro).Equals($expectedDistro, [System.StringComparison]::OrdinalIgnoreCase)) {
                    throw "Fixture provenance ContainerDistro did not match runtime matrix for $($producerTarget.Rid)/$($producerTarget.Profile)."
                }

                if (-not ([string]$manifest.ContainerDistroVersion).Equals($expectedDistroVersion, [System.StringComparison]::OrdinalIgnoreCase)) {
                    throw "Fixture provenance ContainerDistroVersion did not match runtime matrix for $($producerTarget.Rid)/$($producerTarget.Profile)."
                }

                if ([string]::IsNullOrWhiteSpace([string]$manifest.ContainerLibc)) {
                    throw "Fixture provenance did not record ContainerLibc for $($producerTarget.Rid)/$($producerTarget.Profile)."
                }
            }

            if ([string]::IsNullOrWhiteSpace([string]$manifest.BuildList)) {
                throw "Fixture provenance did not record BuildList for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if (-not ([string]$manifest.BuildList).Equals([string]$profileSpec[0].buildList, [System.StringComparison]::Ordinal)) {
                throw "Fixture provenance BuildList did not match runtime matrix for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if (@($manifest.NativeLoaderFiles).Count -lt 2) {
                throw "Fixture provenance did not include both native loader files for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if (@($manifest.RuntimeFiles).Count -ne @($profileSpec[0].modules).Count) {
                throw "Fixture provenance runtime file count did not match the exact profile module set for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }
        }
    }
    catch {
        Add-Violation -Violations $violations -Path $runtimeInputScriptPath -Issue "Runtime input artifact script must run against a minimal real-layout fixture" -Text $_.Exception.Message
    }
    finally {
        if (Test-Path -LiteralPath $fixtureRoot) {
            Remove-Item -LiteralPath $fixtureRoot -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Real runtime input producer surface guard failed with $($violations.Count) violation(s)."
    $violations |
        Sort-Object Path, Issue |
        Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "Real runtime input producer surface guard passed."
Write-Host "Producer artifacts: runtime-input-ubuntu.24.04-x64-full, runtime-input-ubuntu.24.04-x64-mini, runtime-input-ubuntu.22.04-x64-full, runtime-input-debian.12-x64-full, runtime-input-fedora.40-x64-full, runtime-input-rhel.9-x64-full, runtime-input-rocky.9-x64-full, runtime-input-alpine.3.20-x64-full."
Write-Host "Producer handoff layout: native-wrapper, opencv-runtime, opencv-source, optional opencv-install."
