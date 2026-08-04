param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$producerWorkflowPath = ".github/workflows/runtime-input.yml"
$runtimeInputScriptPath = "scripts/New-RuntimeInputArtifact.ps1"
$windowsPeAuditPath = "scripts/Test-WindowsRuntimePeClosure.ps1"
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

function Get-ContainerNativeScript {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Text
    )

    $normalizedText = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
    $startMarker = '"$container_shell" -lc '''
    $start = $normalizedText.IndexOf($startMarker, [System.StringComparison]::Ordinal)
    if ($start -lt 0) {
        return ""
    }

    $start += $startMarker.Length
    $end = $normalizedText.IndexOf("`n            '`n", $start, [System.StringComparison]::Ordinal)
    if ($end -lt 0) {
        return ""
    }

    return $normalizedText.Substring($start, $end - $start)
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
        [pscustomobject]@{ Rid = "win-x64"; Profile = "full"; Runner = "windows-latest"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "win-x64"; Profile = "mini"; Runner = "windows-latest"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "win-x86"; Profile = "full"; Runner = "windows-latest"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "win-arm64"; Profile = "full"; Runner = "windows-11-vs2026-arm"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "win-arm64"; Profile = "mini"; Runner = "windows-11-vs2026-arm"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "ubuntu.24.04-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "ubuntu.24.04-x64"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "ubuntu.24.04-arm64"; Profile = "full"; Runner = "ubuntu-24.04-arm"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "ubuntu.24.04-arm64"; Profile = "mini"; Runner = "ubuntu-24.04-arm"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "ubuntu.22.04-x64"; Profile = "full"; Runner = "ubuntu-22.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "ubuntu.22.04-x64"; Profile = "mini"; Runner = "ubuntu-22.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "ubuntu.22.04-arm64"; Profile = "full"; Runner = "ubuntu-24.04-arm"; ContainerImage = "ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "ubuntu.22.04-arm64"; Profile = "mini"; Runner = "ubuntu-24.04-arm"; ContainerImage = "ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "debian.12-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = "debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "debian.12-x64"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = "debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "debian.12-arm64"; Profile = "full"; Runner = "ubuntu-24.04-arm"; ContainerImage = "debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "debian.12-arm64"; Profile = "mini"; Runner = "ubuntu-24.04-arm"; ContainerImage = "debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "fedora.40-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = "fedora:40@sha256:3c86d25fef9d2001712bc3d9b091fc40cf04be4767e48f1aa3b785bf58d300ed"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "fedora.40-x64"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = "fedora:40@sha256:3c86d25fef9d2001712bc3d9b091fc40cf04be4767e48f1aa3b785bf58d300ed"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "rhel.9-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = "registry.access.redhat.com/ubi9/ubi:9.8@sha256:50701171b9917ed51048b614924598d45b00bce9a64b73860c057922fc13bec2"; OpenCvExtraCMakeArgs = "-DCMAKE_CXX_FLAGS=-DCV_AVXVNNI_AVAILABLE=0" },
        [pscustomobject]@{ Rid = "rhel.9-x64"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = "registry.access.redhat.com/ubi9/ubi:9.8@sha256:50701171b9917ed51048b614924598d45b00bce9a64b73860c057922fc13bec2"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "rocky.9-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = "rockylinux:9@sha256:d7be1c094cc5845ee815d4632fe377514ee6ebcf8efaed6892889657e5ddaaa6"; OpenCvExtraCMakeArgs = "-DCMAKE_CXX_FLAGS=-DCV_AVXVNNI_AVAILABLE=0" },
        [pscustomobject]@{ Rid = "rocky.9-x64"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = "rockylinux:9@sha256:d7be1c094cc5845ee815d4632fe377514ee6ebcf8efaed6892889657e5ddaaa6"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "alpine.3.20-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = "alpine:3.20@sha256:d9e853e87e55526f6b2917df91a2115c36dd7c696a35be12163d44e6e2a4b6bc"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "alpine.3.20-x64"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = "alpine:3.20@sha256:d9e853e87e55526f6b2917df91a2115c36dd7c696a35be12163d44e6e2a4b6bc"; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "android-arm64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "android-arm64"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "android-arm"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "android-arm"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "android-x64"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "android-x64"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "android-x86"; Profile = "full"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" },
        [pscustomobject]@{ Rid = "android-x86"; Profile = "mini"; Runner = "ubuntu-24.04"; ContainerImage = ""; OpenCvExtraCMakeArgs = "" }
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

        $platformFamily = [string]$ridSpec[0].platformFamily
        if ($target.Rid -in @("win-x64", "win-x86", "win-arm64")) {
            if (-not $platformFamily.Equals("windows", [System.StringComparison]::OrdinalIgnoreCase) -or
                ($target.Rid -eq "win-x64" -and $target.Profile -notin @("full", "mini")) -or
                ($target.Rid -eq "win-x86" -and $target.Profile -ne "full") -or
                ($target.Rid -eq "win-arm64" -and $target.Profile -notin @("full", "mini")) -or
                -not [string]::IsNullOrWhiteSpace([string]$target.ContainerImage)) {
                Add-Violation -Violations $Violations -Path $RuntimeMatrixPath -Issue "Approved Windows producer must remain exact hosted win-x64 full/mini, win-x86 full, or win-arm64 full/mini without a container" -Text "$($target.Rid)/$($target.Profile) platform=$platformFamily"
            }
        }
        elseif ($platformFamily.Equals("android", [System.StringComparison]::OrdinalIgnoreCase)) {
            if (-not $target.Rid.StartsWith("android-", [System.StringComparison]::Ordinal) -or
                $target.Profile -notin @("full", "mini") -or
                -not [string]::IsNullOrWhiteSpace([string]$target.ContainerImage)) {
                Add-Violation -Violations $Violations -Path $RuntimeMatrixPath -Issue "Approved Android producer must remain an NDK-hosted full/mini target without a container" -Text "$($target.Rid)/$($target.Profile) platform=$platformFamily"
            }
        }
        else {
            if (-not $platformFamily.Equals("linux", [System.StringComparison]::OrdinalIgnoreCase)) {
                Add-Violation -Violations $Violations -Path $RuntimeMatrixPath -Issue "Approved distro real producer target must remain a Linux runtime package RID" -Text $target.Rid
            }

            $distroProperty = $ridSpec[0].PSObject.Properties["distro"]
            $distro = if ($null -eq $distroProperty) { "" } else { [string]$distroProperty.Value }
            if ([string]::IsNullOrWhiteSpace($distro)) {
                Add-Violation -Violations $Violations -Path $RuntimeMatrixPath -Issue "Approved distro real producer target must record a distro in the runtime matrix" -Text $target.Rid
            }

            if ($distro.Equals("ubuntu", [System.StringComparison]::OrdinalIgnoreCase)) {
                if ($target.Rid -eq "ubuntu.22.04-arm64") {
                    if ([string]::IsNullOrWhiteSpace([string]$target.ContainerImage)) {
                        Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Ubuntu 22.04 ARM64 must declare its audited container-native userspace" -Text $target.Rid
                    }
                }
                elseif (-not [string]::IsNullOrWhiteSpace([string]$target.ContainerImage)) {
                    Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Ubuntu real producer targets should remain hosted-runner native until deliberately converted" -Text "$($target.Rid): $($target.ContainerImage)"
                }
            }
            else {
                if ([string]::IsNullOrWhiteSpace([string]$target.ContainerImage)) {
                    Add-Violation -Violations $Violations -Path $ProducerWorkflowPath -Issue "Non-Ubuntu real producer targets must declare a distro-native container image" -Text $target.Rid
                }

                # Exact approved image matching above handles registries whose repository name differs from /etc/os-release ID, such as rockylinux:9 with ID=rocky.
            }
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
$windowsPeAuditText = Read-RequiredText -RelativePath $windowsPeAuditPath
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
        [pscustomobject]@{ Needle = "produce-android:"; Issue = "Android production must remain in a separate NDK-hosted job" },
        [pscustomobject]@{ Needle = "Build-AndroidRuntimeInput.ps1"; Issue = "Android producer must use the audited NDK build and ELF evidence script" },
        [pscustomobject]@{ Needle = "28.2.13676358"; Issue = "Android producer must pin the audited NDK revision" },
        [pscustomobject]@{ Needle = "ANDROID_PACKAGE_CONSUMER_OK"; Issue = "Android producer must build an APK from the same-run managed and runtime packages" },
        [pscustomobject]@{ Needle = "system-images;android-35;default;x86_64"; Issue = "Android x64 emulator smoke must use the pinned x86_64 system image" },
        [pscustomobject]@{ Needle = "system-images;android-29;default;x86"; Issue = "Android x86 emulator smoke must use the last available default x86 system image" },
        [pscustomobject]@{ Needle = "matrix.rid == 'android-x64' || matrix.rid == 'android-x86'"; Issue = "Android emulator smoke must cover both x64 and x86 package targets" },
        [pscustomobject]@{ Needle = "timeout 20m bash -c"; Issue = "Android emulator system-image installation must have a bounded timeout" },
        [pscustomobject]@{ Needle = 'export ANDROID_AVD_HOME="$RUNNER_TEMP/android-avd"'; Issue = "Android AVD creation and emulator lookup must share an explicit runner-local directory" },
        [pscustomobject]@{ Needle = 'test -f "$ANDROID_AVD_HOME/opencv-csharp-smoke.ini"'; Issue = "Android emulator smoke must verify the named AVD index before startup" },
        [pscustomobject]@{ Needle = 'timeout 3m "$adb" wait-for-device'; Issue = "Android emulator adb connection wait must have a bounded timeout" },
        [pscustomobject]@{ Needle = 'cat "$emulator_log"'; Issue = "Android emulator failures must expose the emulator log" },
        [pscustomobject]@{ Needle = "ANDROID_EMULATOR_LOADING_OK"; Issue = "Android x64 and x86 producers must execute a native loading smoke in an emulator" },
        [pscustomobject]@{ Needle = "produce-windows:"; Issue = "Native Windows production must remain in a separate hosted Windows job" },
        [pscustomobject]@{ Needle = "evidence_prefix: WINDOWS_X64"; Issue = "Windows producer matrix must retain the x64 evidence branch" },
        [pscustomobject]@{ Needle = "evidence_prefix: WINDOWS_ARM64"; Issue = "Windows producer matrix must retain the ARM64 evidence branch" },
        [pscustomobject]@{ Needle = "`${{ matrix.evidence_prefix }}_PRODUCER_HOST_EVIDENCE"; Issue = "Windows producer must record actual host, process, CPU, memory, and disk evidence" },
        [pscustomobject]@{ Needle = "`${{ matrix.evidence_prefix }}_PRODUCER_TOOLCHAIN_EVIDENCE"; Issue = "Windows producer must record the architecture-specific VS/MSVC/CMake toolchain" },
        [pscustomobject]@{ Needle = "`${{ matrix.evidence_prefix }}_OPENCV_PATH_SANITIZED"; Issue = "Windows producer must record its build-scoped foreign tool PATH exclusions" },
        [pscustomobject]@{ Needle = "`$plan = & ./scripts/Build-OpenCV.ps1"; Issue = "Windows profile plan must bind leading-hyphen CMake arguments directly in the current PowerShell process" },
        [pscustomobject]@{ Needle = "& ./scripts/Build-OpenCV.ps1"; Issue = "Windows OpenCV build must bind profile-specific CMake arguments directly in the current PowerShell process" },
        [pscustomobject]@{ Needle = "& ./scripts/New-RuntimeInputArtifact.ps1"; Issue = "Windows runtime artifact creation must bind leading-hyphen CMake evidence directly in the current PowerShell process" },
        [pscustomobject]@{ Needle = "`$foreignToolPattern = '(?i)(?:^|[\\/])(?:mingw(?:32|64)?|msys(?:2|64)?|cygwin(?:64)?)(?:[\\/]|`$)'"; Issue = "Windows producer must exclude named MinGW, MSYS, and Cygwin tool directories from the OpenCV build PATH" },
        [pscustomobject]@{ Needle = "Get-Command -Name `$tool -CommandType Application -All"; Issue = "Windows producer must remove every PATH directory that exposes a CMake generic compiler candidate, including bundled Strawberry GCC" },
        [pscustomobject]@{ Needle = "`$genericCompilerDirectorySet.Contains"; Issue = "Windows producer PATH sanitization must use the factual generic compiler directory set" },
        [pscustomobject]@{ Needle = "Remove-Item Env:ASM"; Issue = "Windows producer must clear an inherited generic ASM compiler override before OpenCV configuration" },
        [pscustomobject]@{ Needle = "`${{ matrix.evidence_prefix }}_OPENCV_ASM_CACHE_EVIDENCE"; Issue = "Windows producer must emit the architecture-specific generic-ASM and MLAS cache values" },
        [pscustomobject]@{ Needle = "Groups[1].Value -ne 'NOTFOUND'"; Issue = "Windows producer must require the CMake CheckLanguage literal generic-ASM fallback value" },
        [pscustomobject]@{ Needle = "OPENCV_DNN_MLAS_ENABLED:INTERNAL"; Issue = "Windows producer must verify that unsupported GNU-assembly MLAS is not retained in the MSVC build" },
        [pscustomobject]@{ Needle = "'-lpthread', '.dll.a', 'mingw', 'msys', 'cygwin'"; Issue = "Windows producer must reject foreign linker and import-library tokens from generated MSVC projects" },
        [pscustomobject]@{ Needle = "`${{ matrix.evidence_prefix }}_OPENCV_BUILD_EVIDENCE"; Issue = "Windows producer must record architecture-specific generator, SDK, build list, and CPU configuration" },
        [pscustomobject]@{ Needle = "`${{ matrix.evidence_prefix }}_NATIVE_PROFILE_EVIDENCE"; Issue = "Windows producer must record profile-specific wrapper source and ABI counts" },
        [pscustomobject]@{ Needle = "expectedSourceCount = if (`$profileName -eq 'mini') { 9 } else { 49 }"; Issue = "Windows producer must lock exact mini/full wrapper source counts" },
        [pscustomobject]@{ Needle = "expectedAbiFunctionCount = if (`$profileName -eq 'mini') { 526 } else { 2656 }"; Issue = "Windows producer must lock exact mini/full ABI counts" },
        [pscustomobject]@{ Needle = "Windows mini OpenCV build unexpectedly configured full-only DNN MLAS"; Issue = "Windows mini producer must reject full-only DNN MLAS configuration" },
        [pscustomobject]@{ Needle = "Windows mini OpenCV build unexpectedly generated the full-only DNN project"; Issue = "Windows mini producer must reject a generated DNN project" },
        [pscustomobject]@{ Needle = "100% tests passed(?:, 0 tests failed)? out of 3"; Issue = "Windows producer must accept the audited three-test success summary" },
        [pscustomobject]@{ Needle = "`${{ matrix.evidence_prefix }}_LINKED_CTEST_EVIDENCE passed=3 total=3"; Issue = "Windows producer must require all three linked CTests on the selected native architecture" },
        [pscustomobject]@{ Needle = "Windows ARM64 full OpenCV build did not retain its factual pure-MSVC MLAS fallback boundary"; Issue = "Windows ARM64 producer must verify its own ASM/MLAS boundary independently from x64" },
        [pscustomobject]@{ Needle = "Generated Windows ARM64 OpenCV project retained x64 compiler or machine evidence"; Issue = "Windows ARM64 producer must reject generated x64 compiler and machine evidence" },
        [pscustomobject]@{ Needle = "Apply audited Windows ARM64 OpenCV MLAS processor patch"; Issue = "Windows ARM64 producer must apply the audited OpenCV processor-case fix before building DNN" },
        [pscustomobject]@{ Needle = "git -C `$sourceDir apply --check `$patchPath"; Issue = "Windows ARM64 producer must prove the exact OpenCV patch applies cleanly" },
        [pscustomobject]@{ Needle = "WINDOWS_ARM64_OPENCV_SOURCE_PATCH_OK"; Issue = "Windows ARM64 producer must emit hashed source-patch evidence" },
        [pscustomobject]@{ Needle = "packaging/runtime/patches/windows-arm64-mlas-processor-case.patch"; Issue = "Windows ARM64 producer must use the repository-owned audited OpenCV patch" },
        [pscustomobject]@{ Needle = "Test-WindowsRuntimePeClosure.ps1"; Issue = "Windows producer must run the reusable PE closure guard before upload" },
        [pscustomobject]@{ Needle = "-HostedProcessArchitecture"; Issue = "Windows producer provenance must record actual process architecture" },
        [pscustomobject]@{ Needle = "-WindowsSdkVersion"; Issue = "Windows producer provenance must record the selected Windows SDK" },
        [pscustomobject]@{ Needle = "-PeAuditEvidence"; Issue = "Windows producer provenance must retain the PE closure marker" },
        [pscustomobject]@{ Needle = "-ExcludedForeignToolDirectories"; Issue = "Windows producer provenance must retain the build-scoped PATH exclusions" },
        [pscustomobject]@{ Needle = "-OpenCvAsmConfiguration"; Issue = "Windows producer provenance must retain the generic-ASM and MLAS fallback configuration" },
        [pscustomobject]@{ Needle = "-OpenCvSourcePatchEvidence"; Issue = "Windows ARM64 producer provenance must retain the audited OpenCV source patch" },
        [pscustomobject]@{ Needle = "-NativeWrapperSources"; Issue = "Windows producer provenance must retain the exact wrapper source list" },
        [pscustomobject]@{ Needle = "-NativeWrapperSourceCount"; Issue = "Windows producer provenance must retain the wrapper source count" },
        [pscustomobject]@{ Needle = "-NativeAbiFunctionCount"; Issue = "Windows producer provenance must retain the ABI function count" },
        [pscustomobject]@{ Needle = "os: ubuntu-24.04-arm"; Issue = "Ubuntu 24.04 ARM64 producer must use the native GitHub-hosted ARM64 runner" },
        [pscustomobject]@{ Needle = "container_image: ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982"; Issue = "Ubuntu 22.04 ARM64 producer must pin the audited official multi-architecture image digest" },
        [pscustomobject]@{ Needle = "UBUNTU_22_04_ARM64_PRODUCER_HOST_EVIDENCE"; Issue = "Ubuntu 22.04 ARM64 producer must record the native AArch64 Docker host" },
        [pscustomobject]@{ Needle = "UBUNTU_22_04_ARM64_PRODUCER_IMAGE_EVIDENCE"; Issue = "Ubuntu 22.04 ARM64 producer must record the official image identity and digest" },
        [pscustomobject]@{ Needle = "UBUNTU_22_04_ARM64_PRODUCER_CONTAINER_EVIDENCE"; Issue = "Ubuntu 22.04 ARM64 producer must record factual target userspace identity" },
        [pscustomobject]@{ Needle = "UBUNTU_22_04_ARM64_POWERSHELL_EVIDENCE version=`$POWERSHELL_VERSION archive_sha256=`$POWERSHELL_SHA256 architecture=arm64"; Issue = "Ubuntu 22.04 ARM64 producer must verify its native PowerShell archive" },
        [pscustomobject]@{ Needle = "UBUNTU_22_04_ARM64_PRODUCER_NEON_EVIDENCE profile=`$RUNTIME_PROFILE machine=AArch64 neon_compile=success"; Issue = "Ubuntu 22.04 ARM64 producer must compile and audit a profile-specific native NEON object" },
        [pscustomobject]@{ Needle = "UBUNTU_22_04_ARM64_TOOLCHAIN_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Ubuntu 22.04 ARM64 producer must retain target compiler, assembler, CMake, Ninja, PowerShell, and host .NET evidence" },
        [pscustomobject]@{ Needle = "UBUNTU_22_04_ARM64_OPENCV_CPU_EVIDENCE"; Issue = "Ubuntu 22.04 ARM64 producer must retain factual OpenCV CPU configuration" },
        [pscustomobject]@{ Needle = "UBUNTU_22_04_ARM64_LINKED_CTEST_EVIDENCE profile=`$RUNTIME_PROFILE passed=3 total=3"; Issue = "Ubuntu 22.04 ARM64 producer must require profile-specific linked CTest 3/3" },
        [pscustomobject]@{ Needle = "UBUNTU_22_04_ARM64_PRODUCER_ELF_EVIDENCE profile=`$RUNTIME_PROFILE files=`$expected_canonical_count runtime_files=`$expected_runtime_file_count machine=AArch64 origin=`$expected_canonical_count producer_paths=0 direct_opencv=`$expected_direct_opencv missing_dependencies=0"; Issue = "Ubuntu 22.04 ARM64 producer must audit its exact profile-derived AArch64 ELF closure" },
         [pscustomobject]@{ Needle = "68f3874cdb6cd564acf404103dfc410ee85435b02f0ad648e73a958853175d6c"; Issue = "Ubuntu 22.04 ARM64 producer must pin the audited PowerShell 7.4.17 ARM64 archive hash" },
         [pscustomobject]@{ Needle = "2fa9c05d591a1582a9aba276272478c262e95ad00acf60eaee1644d93941e3c6"; Issue = "Container producer must bind the audited Microsoft signing-key payload hash" },
         [pscustomobject]@{ Needle = "BC528686B50D79E339D3721CEB3E94ADBE1229CF"; Issue = "Container producer must bind the audited full Microsoft signing-key fingerprint" },
         [pscustomobject]@{ Needle = "MICROSOFT_APT_TRUST_EVIDENCE rid=`$PRODUCER_RID"; Issue = "Debian x64 producer must emit verified scoped APT trust evidence" },
         [pscustomobject]@{ Needle = "MICROSOFT_RPM_TRUST_EVIDENCE rid=`$PRODUCER_RID"; Issue = "Fedora, Rocky, and RHEL producers must emit verified local RPM trust evidence" },
         [pscustomobject]@{ Needle = "POWERSHELL_DEBIAN_PACKAGE_VERSION: 7.6.4-1.deb"; Issue = "Debian package-manager producer must bind the audited exact PowerShell Debian package version" },
         [pscustomobject]@{ Needle = "POWERSHELL_RPM_PACKAGE_NEVRA: powershell-7.6.4-1.rh.x86_64"; Issue = "RPM package-manager producers must bind the audited exact PowerShell RPM NEVRA" },
         [pscustomobject]@{ Needle = "POWERSHELL_PACKAGE_SEMANTIC_VERSION: 7.6.4"; Issue = "Debian and RPM package-manager producers must bind the audited actual pwsh semantic version" },
         [pscustomobject]@{ Needle = "POWERSHELL_ALPINE_PACKAGE_VERSION: 7.4.6-r1"; Issue = "Alpine package-manager producer must bind the audited exact PowerShell APK version" },
         [pscustomobject]@{ Needle = "POWERSHELL_ALPINE_SEMANTIC_VERSION: 7.4.6"; Issue = "Alpine package-manager producer must bind the audited actual pwsh semantic version" },
         [pscustomobject]@{ Needle = "POWERSHELL_APT_TOOLCHAIN_EVIDENCE rid=`$PRODUCER_RID"; Issue = "Debian x64 producer must emit exact package-installed PowerShell evidence" },
         [pscustomobject]@{ Needle = "POWERSHELL_RPM_TOOLCHAIN_EVIDENCE rid=`$PRODUCER_RID"; Issue = "Fedora, Rocky, and RHEL producers must emit exact package-installed PowerShell evidence" },
         [pscustomobject]@{ Needle = "POWERSHELL_APK_TOOLCHAIN_EVIDENCE rid=`$PRODUCER_RID"; Issue = "Alpine producer must emit exact package-installed PowerShell evidence" },
         [pscustomobject]@{ Needle = "gpgkey=file:///etc/pki/rpm-gpg/RPM-GPG-KEY-Microsoft"; Issue = "RPM producers must use only the verified local Microsoft signing key" },
         [pscustomobject]@{ Needle = "container_image: debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc"; Issue = "Debian 12 ARM64 producer must pin the audited official multi-architecture image digest" },
         [pscustomobject]@{ Needle = "DEBIAN_12_ARM64_PRODUCER_HOST_EVIDENCE"; Issue = "Debian 12 ARM64 producer must record the native AArch64 Docker host" },
         [pscustomobject]@{ Needle = "DEBIAN_12_ARM64_PRODUCER_IMAGE_EVIDENCE"; Issue = "Debian 12 ARM64 producer must record the official image identity and digest" },
         [pscustomobject]@{ Needle = "DEBIAN_12_ARM64_PRODUCER_CONTAINER_EVIDENCE"; Issue = "Debian 12 ARM64 producer must record factual target userspace identity" },
         [pscustomobject]@{ Needle = "DEBIAN_12_ARM64_POWERSHELL_EVIDENCE version=`$POWERSHELL_VERSION archive_sha256=`$POWERSHELL_SHA256 architecture=arm64"; Issue = "Debian 12 ARM64 producer must verify its native PowerShell archive" },
          [pscustomobject]@{ Needle = "DEBIAN_12_ARM64_TOOLCHAIN_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Debian 12 ARM64 producer must retain target compiler, assembler, CMake, Ninja, PowerShell, and host .NET evidence" },
          [pscustomobject]@{ Needle = "DEBIAN_12_ARM64_PRODUCER_NEON_EVIDENCE profile=`$RUNTIME_PROFILE machine=AArch64 neon_compile=success"; Issue = "Debian 12 ARM64 producer must compile and audit a profile-specific native NEON object" },
          [pscustomobject]@{ Needle = "DEBIAN_12_ARM64_OPENCV_CPU_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Debian 12 ARM64 producer must retain factual profile-specific OpenCV CPU configuration" },
          [pscustomobject]@{ Needle = "DEBIAN_12_ARM64_LINKED_CTEST_EVIDENCE profile=`$RUNTIME_PROFILE passed=3 total=3"; Issue = "Debian 12 ARM64 producer must require profile-specific linked CTest 3/3" },
          [pscustomobject]@{ Needle = "DEBIAN_12_ARM64_PRODUCER_ELF_EVIDENCE profile=`$RUNTIME_PROFILE files=`$expected_canonical_count runtime_files=`$expected_runtime_file_count machine=AArch64 origin=`$expected_canonical_count producer_paths=0 direct_opencv=`$expected_direct_opencv missing_dependencies=0"; Issue = "Debian 12 ARM64 producer must audit its exact profile-derived AArch64 ELF closure" },
          [pscustomobject]@{ Needle = "DEBIAN_12_X64_PRODUCER_HOST_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Debian 12 x64 producer must record its native x64 Docker host" },
          [pscustomobject]@{ Needle = "DEBIAN_12_X64_PRODUCER_IMAGE_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Debian 12 x64 producer must record the resolved official image identity and digest" },
          [pscustomobject]@{ Needle = "DEBIAN_12_X64_PRODUCER_CONTAINER_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Debian 12 x64 producer must record factual Debian userspace and PowerShell evidence" },
          [pscustomobject]@{ Needle = "DEBIAN_12_X64_TOOLCHAIN_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Debian 12 x64 producer must retain compiler, assembler, CMake, Ninja, PowerShell, and host .NET evidence" },
          [pscustomobject]@{ Needle = "DEBIAN_12_X64_OPENCV_CPU_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Debian 12 x64 producer must retain factual SSE or AVX OpenCV CPU evidence" },
          [pscustomobject]@{ Needle = "DEBIAN_12_X64_LINKED_CTEST_EVIDENCE profile=`$RUNTIME_PROFILE passed=3 total=3"; Issue = "Debian 12 x64 producer must require profile-specific linked CTest 3/3" },
          [pscustomobject]@{ Needle = "DEBIAN_12_X64_PRODUCER_ELF_EVIDENCE profile=`$RUNTIME_PROFILE files=`$expected_canonical_count runtime_files=`$expected_runtime_file_count machine=X86-64 origin=`$expected_canonical_count producer_paths=0 direct_opencv=`$expected_direct_opencv missing_dependencies=0"; Issue = "Debian 12 x64 producer must audit its exact profile-derived x86-64 ELF closure" },
        [pscustomobject]@{ Needle = "UBUNTU_24_04_ARM64_RUNNER_EVIDENCE"; Issue = "Ubuntu ARM64 producer must emit actual runner image, distro, architecture, libc, CPU, and disk evidence" },
        [pscustomobject]@{ Needle = "UBUNTU_24_04_ARM64_TOOLCHAIN_EVIDENCE"; Issue = "Ubuntu ARM64 producer must emit native compiler, assembler, CMake, Ninja, PowerShell, and .NET evidence" },
        [pscustomobject]@{ Needle = 'PRODUCER_ASSEMBLER_VERSION: ${{ steps.ubuntu2204_x64_runner.outputs.assembler_version || steps.arm64_runner.outputs.assembler_version }}'; Issue = "Hosted Ubuntu producers must transport factual assembler evidence without interpolating tool output into PowerShell source" },
        [pscustomobject]@{ Needle = '-AssemblerVersion $env:PRODUCER_ASSEMBLER_VERSION'; Issue = "Ubuntu ARM64 artifact creation must bind assembler evidence through the step environment" },
        [pscustomobject]@{ Needle = 'test "$(uname -m)" = "aarch64"'; Issue = "Ubuntu ARM64 producer must reject non-AArch64 execution" },
        [pscustomobject]@{ Needle = 'test "$(dpkg --print-architecture)" = "arm64"'; Issue = "Ubuntu ARM64 producer must require the native Debian arm64 architecture" },
        [pscustomobject]@{ Needle = "UBUNTU_24_04_ARM64_NEON_EVIDENCE machine=AArch64 neon_compile=success"; Issue = "Ubuntu ARM64 producer must compile and audit an actual NEON AArch64 object" },
        [pscustomobject]@{ Needle = "UBUNTU_24_04_ARM64_OPENCV_CPU_EVIDENCE"; Issue = "Ubuntu ARM64 producer must report factual OpenCV CPU configuration" },
        [pscustomobject]@{ Needle = 'UBUNTU_24_04_ARM64_LINKED_CTEST_EVIDENCE profile=${{ matrix.profile }} passed=3 total=3'; Issue = "Ubuntu ARM64 producer must require all three linked CTests for the exact selected profile" },
        [pscustomobject]@{ Needle = 'expected_canonical_count=7'; Issue = "Ubuntu ARM64 mini producer must require exactly seven canonical AArch64 ELFs" },
        [pscustomobject]@{ Needle = 'expected_runtime_file_count=19'; Issue = "Ubuntu ARM64 mini producer must require the exact 19-file Linux payload" },
        [pscustomobject]@{ Needle = 'expected_direct_opencv=6'; Issue = "Ubuntu ARM64 mini producer must require exactly six direct OpenCV dependencies" },
        [pscustomobject]@{ Needle = 'UBUNTU_24_04_ARM64_PRODUCER_ELF_EVIDENCE profile=$profile files=$expected_canonical_count runtime_files=$expected_runtime_file_count machine=AArch64 origin=$expected_canonical_count producer_paths=0 direct_opencv=$expected_direct_opencv missing_dependencies=0'; Issue = "Ubuntu ARM64 producer must emit profile-derived complete canonical AArch64 ELF closure evidence" },
        [pscustomobject]@{ Needle = 'UBUNTU_22_04_X64_RUNNER_EVIDENCE profile=mini'; Issue = "Ubuntu 22.04 x64 mini producer must record the matching hosted runner" },
        [pscustomobject]@{ Needle = 'UBUNTU_22_04_X64_TOOLCHAIN_EVIDENCE profile=mini'; Issue = "Ubuntu 22.04 x64 mini producer must record its native toolchain" },
        [pscustomobject]@{ Needle = 'UBUNTU_22_04_X64_COMPILER_EVIDENCE profile=mini machine=X86-64 compile=success'; Issue = "Ubuntu 22.04 x64 mini producer must compile a native x86-64 object" },
        [pscustomobject]@{ Needle = 'UBUNTU_22_04_X64_OPENCV_CPU_EVIDENCE profile=mini'; Issue = "Ubuntu 22.04 x64 mini producer must record factual SSE or AVX OpenCV CPU evidence" },
        [pscustomobject]@{ Needle = 'UBUNTU_22_04_X64_LINKED_CTEST_EVIDENCE profile=mini passed=3 total=3'; Issue = "Ubuntu 22.04 x64 mini producer must require linked CTest 3/3" },
        [pscustomobject]@{ Needle = 'UBUNTU_22_04_X64_PRODUCER_ELF_EVIDENCE profile=mini files=7 runtime_files=19 machine=X86-64 origin=7 producer_paths=0 direct_opencv=6 missing_dependencies=0'; Issue = "Ubuntu 22.04 x64 mini producer must emit its exact ELF closure evidence" },
        [pscustomobject]@{ Needle = 'LINUX_NATIVE_PROFILE_EVIDENCE rid=${{ matrix.rid }} profile=$profileName sources=$($nativeSources.Count) abi_functions=$abiFunctionCount'; Issue = "Linux producer must record exact wrapper source and ABI evidence" },
        [pscustomobject]@{ Needle = 'readelf -h "$elf" | grep -q ''Machine:.*AArch64'''; Issue = "Ubuntu ARM64 producer must inspect every canonical ELF machine type" },
        [pscustomobject]@{ Needle = 'missing="$(ldd "$elf" | grep ''not found'' || true)"'; Issue = "Ubuntu ARM64 producer must reject unresolved native dependencies without an environment override" },
        [pscustomobject]@{ Needle = "container_image: debian:12"; Issue = "Producer workflow must declare the Debian 12 container-native boundary" },
        [pscustomobject]@{ Needle = "container_image: fedora:40"; Issue = "Producer workflow must declare the Fedora 40 container-native boundary" },
        [pscustomobject]@{ Needle = "FEDORA_40_X64_PRODUCER_HOST_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Fedora producer must record its Ubuntu 24.04 x64 Docker host" },
        [pscustomobject]@{ Needle = "FEDORA_40_X64_PRODUCER_IMAGE_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Fedora producer must record the resolved Fedora image identity and digest" },
        [pscustomobject]@{ Needle = 'test "${SUPPORT_END:-}" = "2025-05-13"'; Issue = "Fedora producer must lock the ended-support compatibility boundary" },
        [pscustomobject]@{ Needle = '(archive\\.fedoraproject\\.org|fedora-archive|/archive).*/fedora/linux/releases/40/Everything/x86_64/os/'; Issue = "Fedora producer must require an archive mirror serving the exact Fedora 40 release repository path" },
        [pscustomobject]@{ Needle = '(archive\\.fedoraproject\\.org|fedora-archive|/archive).*/fedora/linux/updates/40/Everything/x86_64/'; Issue = "Fedora producer must require an archive mirror serving the exact Fedora 40 updates repository path" },
        [pscustomobject]@{ Needle = "FEDORA_40_REPOSITORY_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Fedora producer must emit exact lifecycle and archive repository evidence" },
        [pscustomobject]@{ Needle = "diffutils"; Issue = "Fedora producer must install diff for exact runtime payload comparison" },
        [pscustomobject]@{ Needle = 'test "$(rpm --eval "%{_arch}")" = "x86_64"'; Issue = "Fedora producer must require native RPM x86_64 package architecture" },
        [pscustomobject]@{ Needle = "FEDORA_40_X64_PRODUCER_CONTAINER_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Fedora producer must record exact target-container architecture, libc, and PowerShell evidence" },
        [pscustomobject]@{ Needle = "FEDORA_40_X64_TOOLCHAIN_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Fedora producer must record its native x86-64 toolchain" },
        [pscustomobject]@{ Needle = "FEDORA_40_X64_OPENCV_CPU_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Fedora producer must record factual SSE or AVX OpenCV CPU evidence" },
        [pscustomobject]@{ Needle = "FEDORA_40_X64_LINKED_CTEST_EVIDENCE profile=`$RUNTIME_PROFILE passed=3 total=3"; Issue = "Fedora producer must require profile-specific linked CTest 3/3" },
        [pscustomobject]@{ Needle = "FEDORA_40_X64_PRODUCER_ELF_EVIDENCE profile=`$RUNTIME_PROFILE files=`$expected_canonical_count runtime_files=`$expected_runtime_file_count machine=X86-64 origin=`$expected_canonical_count producer_paths=0 direct_opencv=`$expected_direct_opencv missing_dependencies=0"; Issue = "Fedora producer must audit the exact profile-derived x86-64 ELF closure" },
        [pscustomobject]@{ Needle = "ROCKY_9_X64_PRODUCER_HOST_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Rocky producer must record its Ubuntu 24.04 x64 Docker host" },
        [pscustomobject]@{ Needle = "ROCKY_9_X64_PRODUCER_IMAGE_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Rocky producer must record the resolved Rocky image identity and digest" },
        [pscustomobject]@{ Needle = "ROCKY_9_X64_PRODUCER_CONTAINER_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Rocky producer must record exact target-container architecture, libc, and PowerShell evidence" },
        [pscustomobject]@{ Needle = "ROCKY_9_X64_TOOLCHAIN_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Rocky producer must record its native x86-64 toolchain" },
        [pscustomobject]@{ Needle = "ROCKY_9_X64_OPENCV_CPU_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Rocky producer must record factual SSE or AVX OpenCV CPU evidence" },
        [pscustomobject]@{ Needle = "ROCKY_9_X64_LINKED_CTEST_EVIDENCE profile=`$RUNTIME_PROFILE passed=3 total=3"; Issue = "Rocky producer must require profile-specific linked CTest 3/3" },
        [pscustomobject]@{ Needle = "ROCKY_9_X64_PRODUCER_ELF_EVIDENCE profile=`$RUNTIME_PROFILE files=`$expected_canonical_count runtime_files=`$expected_runtime_file_count machine=X86-64 origin=`$expected_canonical_count producer_paths=0 direct_opencv=`$expected_direct_opencv missing_dependencies=0"; Issue = "Rocky producer must audit the exact profile-derived x86-64 ELF closure" },
        [pscustomobject]@{ Needle = "RHEL_9_X64_PRODUCER_HOST_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "RHEL producer must record its Ubuntu 24.04 x64 Docker host" },
        [pscustomobject]@{ Needle = "RHEL_9_X64_PRODUCER_IMAGE_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "RHEL producer must record the resolved official UBI image identity and digest" },
        [pscustomobject]@{ Needle = "RHEL_9_X64_PRODUCER_CONTAINER_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "RHEL producer must record exact target-container platform, architecture, libc, and PowerShell evidence" },
        [pscustomobject]@{ Needle = "RHEL_9_X64_TOOLCHAIN_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "RHEL producer must record its native x86-64 toolchain" },
        [pscustomobject]@{ Needle = "RHEL_9_X64_OPENCV_CPU_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "RHEL producer must record factual SSE or AVX OpenCV CPU evidence" },
        [pscustomobject]@{ Needle = "RHEL_9_X64_LINKED_CTEST_EVIDENCE profile=`$RUNTIME_PROFILE passed=3 total=3"; Issue = "RHEL producer must require profile-specific linked CTest 3/3" },
        [pscustomobject]@{ Needle = "RHEL_9_X64_PRODUCER_ELF_EVIDENCE profile=`$RUNTIME_PROFILE files=`$expected_canonical_count runtime_files=`$expected_runtime_file_count machine=X86-64 origin=`$expected_canonical_count producer_paths=0 direct_opencv=`$expected_direct_opencv missing_dependencies=0"; Issue = "RHEL producer must audit the exact profile-derived x86-64 ELF closure" },
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
        [pscustomobject]@{ Needle = 'test "$(docker info --format ''{{.Architecture}}'')" = "x86_64"'; Issue = "Alpine producer must require the factual x86_64 Docker host architecture" },
        [pscustomobject]@{ Needle = "linux-headers"; Issue = "Alpine producer must install Linux headers required by OpenCV core" },
        [pscustomobject]@{ Needle = "samurai"; Issue = "Alpine producer must install the audited Ninja-compatible build tool" },
        [pscustomobject]@{ Needle = "util-linux-misc"; Issue = "Alpine producer must install the exact package that provides factual lscpu CPU evidence" },
          [pscustomobject]@{ Needle = "ALPINE_3_20_PRODUCER_ELF_EVIDENCE profile=`$RUNTIME_PROFILE files=`$expected_canonical_count runtime_files=`$expected_runtime_file_count machine=X86-64 origin=`$expected_canonical_count producer_paths=0 direct_opencv=`$expected_direct_opencv missing_dependencies=0"; Issue = "Alpine producer must audit the exact profile-derived canonical ELF closure before upload" },
          [pscustomobject]@{ Needle = "ALPINE_3_20_LINKED_CTEST_EVIDENCE profile=`$RUNTIME_PROFILE passed=3 total=3"; Issue = "Alpine producer must require profile-specific linked CTest 3/3" },
          [pscustomobject]@{ Needle = "ALPINE_3_20_PRODUCER_CONTAINER_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Alpine producer must record target container and musl evidence" },
          [pscustomobject]@{ Needle = "ALPINE_3_20_TOOLCHAIN_EVIDENCE profile=`$RUNTIME_PROFILE"; Issue = "Alpine producer must record profile-specific toolchain evidence" },
          [pscustomobject]@{ Needle = "expected_runtime_file_count=19"; Issue = "Alpine mini producer must require the exact 19-file Linux payload" },
          [pscustomobject]@{ Needle = 'opencv_elf_count="$(find "$audit_dir" -maxdepth 1 -type f -name "libopencv_*.so.$OPENCV_VERSION" | wc -l | tr -d " ")"'; Issue = "Shared container producer ELF audit must remain POSIX-compatible and outer-shell quote-safe for Alpine sh" },
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
        [pscustomobject]@{ Needle = 'LINUX_CONTAINER_NATIVE_PROFILE_EVIDENCE rid=${{ matrix.rid }} profile=$profileName sources=$($nativeSources.Count) abi_functions=$abiFunctionCount build_list=$($profile[0].buildList)'; Issue = "Container producer must record the exact selected native source, ABI, and OpenCV build-list profile" },
        [pscustomobject]@{ Needle = "Container distro mismatch for `$PRODUCER_RID"; Issue = "Producer workflow must reject container images whose actual distro does not match the runtime RID matrix" },
        [pscustomobject]@{ Needle = "Container distro version mismatch for `$PRODUCER_RID"; Issue = "Producer workflow must reject container images whose actual distro version does not match the runtime RID matrix" },
        [pscustomobject]@{ Needle = "getconf GNU_LIBC_VERSION"; Issue = "Producer workflow must record libc evidence for container-native Linux outputs" },
        [pscustomobject]@{ Needle = 'OpenCV source reset target escaped its dedicated root'; Issue = "Producer workflow must fail closed before clearing partial upstream-map source evidence" },
        [pscustomobject]@{ Needle = '[IO.Directory]::Delete($sourceDir, $true)'; Issue = "Hosted Linux, Android, and Windows producers must clear partial upstream-map source evidence before cloning" },
        [pscustomobject]@{ Needle = 'if [ "$source_dir" != "$source_root/opencv-$OPENCV_VERSION" ]'; Issue = "Container producers must prove the exact OpenCV source reset target" },
        [pscustomobject]@{ Needle = 'rm -rf -- "$source_dir"'; Issue = "Container producers must clear partial upstream-map source evidence before cloning" },
        [pscustomobject]@{ Needle = 'OPENCV_SOURCE_RESET_EVIDENCE path=$sourceDir reason=upstream-map-partial-source'; Issue = "Hosted Linux and Windows producers must record their partial-source reset" },
        [pscustomobject]@{ Needle = 'ANDROID_OPENCV_SOURCE_RESET_EVIDENCE path=$sourceDir reason=partial-source'; Issue = "Android producer must record its guarded partial-source reset" },
        [pscustomobject]@{ Needle = 'OPENCV_SOURCE_RESET_EVIDENCE path=$source_dir reason=upstream-map-partial-source'; Issue = "Container producers must record their partial-source reset" },
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

if ([regex]::Matches($producerWorkflowText, [regex]::Escape('[IO.Directory]::Delete($sourceDir, $true)')).Count -ne 3) {
    Add-Violation -Violations $violations -Path $producerWorkflowPath -Issue "Producer workflow must keep exactly three guarded PowerShell source resets" -Text '[IO.Directory]::Delete($sourceDir, $true)'
}
if ([regex]::Matches($producerWorkflowText, 'OPENCV_SOURCE_RESET_EVIDENCE').Count -ne 4) {
    Add-Violation -Violations $violations -Path $producerWorkflowPath -Issue "Producer workflow must record exactly four hosted/container/Android source-reset boundaries" -Text 'OPENCV_SOURCE_RESET_EVIDENCE'
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
Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "https://packages.microsoft.com/config/rhel/9/prod.repo" -Issue "Producer workflow must not download mutable Microsoft repository configuration"
Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "rpm --import https://" -Issue "Producer workflow must not import package signing keys directly from a network URL"
Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "'ubuntu.24.04-arm64/mini'" -Issue "Ubuntu 24.04 ARM64 mini must be present in the exact real producer allowlist"
Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "'ubuntu.22.04-x64/mini'" -Issue "Ubuntu 22.04 x64 mini must be present in the exact real producer allowlist"
Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "'ubuntu.22.04-arm64/mini'" -Issue "Ubuntu 22.04 ARM64 mini must be present in the exact real producer allowlist"
Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "'fedora.40-x64/mini'" -Issue "Fedora 40 x64 mini must be present in the exact real producer allowlist"
Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "'rocky.9-x64/mini'" -Issue "Rocky 9 x64 mini must be present in the exact real producer allowlist"
Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "'rhel.9-x64/mini'" -Issue "RHEL 9 x64 mini must be present in the exact real producer allowlist"
Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "'win-x86/full'" -Issue "Windows x86 full must be present in the exact real producer allowlist"
Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "'win-x86/mini'" -Issue "Windows x86 mini must remain outside the real producer allowlist until independently proven"
foreach ($windowsX86Needle in @(
        "platform: Win32",
        "tool_host: Hostx64",
        "tool_target: x86",
        "package_architecture: x86",
        "evidence_prefix: WINDOWS_X86",
        "-T host=x64",
        "CMAKE_C_COMPILER_ARCHITECTURE_ID `"X86`"",
        "0x014c",
        "WINDOWS_X86_WOW64_PROBE_OK pointer_bits=32 process_machine=0x014C native_machine=0x8664",
        "_LINKED_CTEST_EVIDENCE passed=3 total=3",
        "-LinkerPath '`${{ steps.windows.outputs.linker_path }}'",
        "-LibrarianPath '`${{ steps.windows.outputs.librarian_path }}'",
        "-DumpbinPath '`${{ steps.windows.outputs.dumpbin_path }}'",
        "-WindowsTargetExecutionEvidence '`${{ steps.windows.outputs.target_execution_evidence }}'")) {
    Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle $windowsX86Needle -Issue "Windows x86 producer must retain factual Win32/Hostx64-x86/I386/WoW64 evidence: $windowsX86Needle"
}
Assert-Contains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "'win-arm64/mini'" -Issue "Windows ARM64 mini must be present in the exact real producer allowlist"
Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "LD_LIBRARY_PATH" -Issue "Ubuntu ARM64 producer closure audit must not use an environment override"
Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $producerWorkflowText -Needle "modules=(" -Issue "Shared container producer script must not retain Bash-only module arrays that Alpine sh cannot parse"

$containerNativeScript = Get-ContainerNativeScript -Text $producerWorkflowText
if ([string]::IsNullOrWhiteSpace($containerNativeScript)) {
    Add-Violation -Violations $violations -Path $producerWorkflowPath -Issue "Container-native producer must retain one extractable fixed shell script argument"
}
else {
    Assert-NotContains -Violations $violations -Path $producerWorkflowPath -Text $containerNativeScript -Needle "'" -Issue "Container-native producer script must not contain a single quote that splits the outer shell argument"
}

$crlfProducerWorkflowText = $producerWorkflowText.Replace("`r`n", "`n").Replace("`r", "`n").Replace("`n", "`r`n")
if ([string]::IsNullOrWhiteSpace((Get-ContainerNativeScript -Text $crlfProducerWorkflowText))) {
    Add-Violation -Violations $violations -Path $producerWorkflowPath -Issue "Container-native producer script extraction must be stable after a Windows CRLF checkout"
}

foreach ($required in @(
        [pscustomobject]@{ Needle = '$architectureSpec = switch ($Rid)'; Issue = "Windows PE audit must derive architecture expectations from exact RID" },
        [pscustomobject]@{ Needle = '"win-x64"'; Issue = "Windows PE audit must retain the exact win-x64 branch" },
        [pscustomobject]@{ Needle = '"win-x86"'; Issue = "Windows PE audit must retain the exact win-x86 branch" },
        [pscustomobject]@{ Needle = '"win-arm64"'; Issue = "Windows PE audit must retain the exact win-arm64 branch" },
        [pscustomobject]@{ Needle = 'HostRuntimeArchitecture = "X64"'; Issue = "Windows PE audit must require an actual x64 inspection process for AMD64-hosted targets" },
        [pscustomobject]@{ Needle = 'HostRuntimeArchitecture = "Arm64"'; Issue = "Windows PE audit must require an actual ARM64 process for win-arm64" },
        [pscustomobject]@{ Needle = 'return $reader.ReadUInt16()'; Issue = "Windows PE audit must read the structured COFF machine field" },
        [pscustomobject]@{ Needle = 'Machine = 0x8664'; Issue = "Windows PE audit must map win-x64 to AMD64 machine 0x8664" },
        [pscustomobject]@{ Needle = 'Machine = 0x014c'; Issue = "Windows PE audit must map win-x86 to I386 machine 0x014c" },
        [pscustomobject]@{ Needle = 'Machine = 0xAA64'; Issue = "Windows PE audit must map win-arm64 to ARM64 machine 0xAA64" },
        [pscustomobject]@{ Needle = 'Host = "Hostx64"'; Issue = "Windows x86 PE audit must use a factual x64-hosted MSVC inspection tool" },
        [pscustomobject]@{ Needle = 'Target = "x86"'; Issue = "Windows x86 PE audit must resolve the x86 target dumpbin tool" },
        [pscustomobject]@{ Needle = 'if ($machine -ne $architectureSpec.Machine)'; Issue = "Windows PE audit must reject mixed or wrong machine values for every DLL" },
        [pscustomobject]@{ Needle = '& $Dumpbin /dependents'; Issue = "Windows PE audit must inspect dependency tables with dumpbin" },
        [pscustomobject]@{ Needle = 'Where-Object { $_.Source -match $dumpbinPattern }'; Issue = "Windows PE audit must reject PATH dumpbin candidates for the wrong host or target architecture" },
        [pscustomobject]@{ Needle = 'Explicit dumpbin.exe path did not match the native'; Issue = "Windows PE audit must reject an explicit dumpbin from the wrong host or target architecture" },
        [pscustomobject]@{ Needle = 'Resolve-Dumpbin -ExplicitPath $DumpbinPath -TargetRid $Rid'; Issue = "Windows PE audit must discover architecture-specific dumpbin without a producer path" },
        [pscustomobject]@{ Needle = '$expectedFileNames = @($primaryLoaderName) + $moduleFileNames'; Issue = "Windows PE audit must require exactly one neutral loader plus the matrix modules" },
        [pscustomobject]@{ Needle = 'Packaged OpenCV dependency closure is incomplete'; Issue = "Windows PE audit must reject missing package-owned imports" },
        [pscustomobject]@{ Needle = 'Matrix-required OpenCV DLLs must all be reachable from the primary loader import graph'; Issue = "Windows PE audit must require the complete 16-module graph closure" },
        [pscustomobject]@{ Needle = 'WINDOWS_PE_AUDIT_OK'; Issue = "Windows PE audit must emit a deterministic success marker" })) {
    Assert-Contains -Violations $violations -Path $windowsPeAuditPath -Text $windowsPeAuditText -Needle $required.Needle -Issue $required.Issue
}

foreach ($forbidden in @("AddDllDirectory", "LoadLibrary", "OpenCvNativeRuntimeDir", "OPENCV_CSHARP_OPENCV_RUNTIME_ROOT")) {
    Assert-NotContains -Violations $violations -Path $windowsPeAuditPath -Text $windowsPeAuditText -Needle $forbidden -Issue "Windows PE audit must not alter package DLL search behavior: $forbidden"
}

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
        [pscustomobject]@{ Needle = "RunnerImage = `$RunnerImage"; Issue = "Runtime input artifact provenance must record the actual hosted runner image" },
        [pscustomobject]@{ Needle = "RunnerImageVersion = `$RunnerImageVersion"; Issue = "Runtime input artifact provenance must record the actual hosted runner image version" },
        [pscustomobject]@{ Needle = "HostedDistro = `$HostedDistro"; Issue = "Runtime input artifact provenance must record the actual hosted distro" },
        [pscustomobject]@{ Needle = "HostedDistroVersion = `$HostedDistroVersion"; Issue = "Runtime input artifact provenance must record the actual hosted distro version" },
        [pscustomobject]@{ Needle = "HostedArchitecture = `$HostedArchitecture"; Issue = "Runtime input artifact provenance must record the actual hosted machine architecture" },
        [pscustomobject]@{ Needle = "HostedPackageArchitecture = `$HostedPackageArchitecture"; Issue = "Runtime input artifact provenance must record the actual hosted package architecture" },
        [pscustomobject]@{ Needle = "HostedLibc = `$HostedLibc"; Issue = "Runtime input artifact provenance must record the actual hosted libc" },
        [pscustomobject]@{ Needle = "HostedCpuModel = `$HostedCpuModel"; Issue = "Runtime input artifact provenance must record the actual hosted CPU model" },
        [pscustomobject]@{ Needle = "HostedMemoryBytes = `$HostedMemoryBytes"; Issue = "Runtime input artifact provenance must record factual hosted memory evidence" },
        [pscustomobject]@{ Needle = "HostedDiskAvailableBytes = `$HostedDiskAvailableBytes"; Issue = "Runtime input artifact provenance must record factual available disk evidence" },
        [pscustomobject]@{ Needle = "HostedOsCaption = `$HostedOsCaption"; Issue = "Runtime input artifact provenance must record the hosted Windows edition" },
        [pscustomobject]@{ Needle = "HostedOsVersion = `$HostedOsVersion"; Issue = "Runtime input artifact provenance must record the hosted OS version" },
        [pscustomobject]@{ Needle = "HostedOsBuildNumber = `$HostedOsBuildNumber"; Issue = "Runtime input artifact provenance must record the hosted OS build number" },
        [pscustomobject]@{ Needle = "HostedProcessArchitecture = `$HostedProcessArchitecture"; Issue = "Runtime input artifact provenance must record the producer process architecture" },
        [pscustomobject]@{ Needle = "VisualStudioVersion = `$VisualStudioVersion"; Issue = "Runtime input artifact provenance must record Visual Studio" },
        [pscustomobject]@{ Needle = "MsvcVersion = `$MsvcVersion"; Issue = "Runtime input artifact provenance must record MSVC" },
        [pscustomobject]@{ Needle = "WindowsSdkVersion = `$WindowsSdkVersion"; Issue = "Runtime input artifact provenance must record the selected Windows SDK" },
        [pscustomobject]@{ Needle = "CMakeVersion = `$CMakeVersion"; Issue = "Runtime input artifact provenance must record CMake" },
        [pscustomobject]@{ Needle = "CMakeGenerator = `$CMakeGenerator"; Issue = "Runtime input artifact provenance must record the CMake generator" },
        [pscustomobject]@{ Needle = "CMakePlatform = `$CMakePlatform"; Issue = "Runtime input artifact provenance must record the CMake platform" },
        [pscustomobject]@{ Needle = "BuildConfiguration = `$BuildConfiguration"; Issue = "Runtime input artifact provenance must record the build configuration" },
        [pscustomobject]@{ Needle = "CompilerPath = `$CompilerPath"; Issue = "Runtime input artifact provenance must record the selected compiler" },
        [pscustomobject]@{ Needle = "LinkerPath = `$LinkerPath"; Issue = "Runtime input artifact provenance must record the selected linker" },
        [pscustomobject]@{ Needle = "LibrarianPath = `$LibrarianPath"; Issue = "Runtime input artifact provenance must record the selected librarian" },
        [pscustomobject]@{ Needle = "DumpbinPath = `$DumpbinPath"; Issue = "Runtime input artifact provenance must record the selected dumpbin" },
        [pscustomobject]@{ Needle = "CompilerVersion = `$CompilerVersion"; Issue = "Runtime input artifact provenance must record the selected compiler version" },
        [pscustomobject]@{ Needle = "AssemblerVersion = `$AssemblerVersion"; Issue = "Runtime input artifact provenance must record the assembler version" },
        [pscustomobject]@{ Needle = "NinjaVersion = `$NinjaVersion"; Issue = "Runtime input artifact provenance must record the Ninja version" },
        [pscustomobject]@{ Needle = "DotNetVersion = `$DotNetVersion"; Issue = "Runtime input artifact provenance must record the native .NET version" },
        [pscustomobject]@{ Needle = "OpenCvCMakeArguments = `$OpenCvCMakeArguments"; Issue = "Runtime input artifact provenance must record the factual OpenCV CMake arguments" },
        [pscustomobject]@{ Needle = "PeAuditEvidence = `$PeAuditEvidence"; Issue = "Runtime input artifact provenance must record the Windows PE audit marker" },
        [pscustomobject]@{ Needle = "WindowsTargetExecutionEvidence = `$WindowsTargetExecutionEvidence"; Issue = "Runtime input artifact provenance must record factual Windows target execution evidence" },
        [pscustomobject]@{ Needle = "ElfAuditEvidence = `$ElfAuditEvidence"; Issue = "Runtime input artifact provenance must record the Linux ELF audit marker" },
        [pscustomobject]@{ Needle = "OpenCvCpuConfiguration = `$OpenCvCpuConfiguration"; Issue = "Runtime input artifact provenance must record factual OpenCV CPU configuration" },
        [pscustomobject]@{ Needle = "ExcludedForeignToolDirectories = `$ExcludedForeignToolDirectories"; Issue = "Runtime input artifact provenance must record Windows build-scoped PATH exclusions" },
        [pscustomobject]@{ Needle = "OpenCvAsmConfiguration = `$OpenCvAsmConfiguration"; Issue = "Runtime input artifact provenance must record the Windows generic-ASM fallback" },
        [pscustomobject]@{ Needle = "NativeWrapperSources = `$NativeWrapperSources"; Issue = "Runtime input artifact provenance must record the wrapper source list" },
        [pscustomobject]@{ Needle = "NativeWrapperSourceCount = `$NativeWrapperSourceCount"; Issue = "Runtime input artifact provenance must record the wrapper source count" },
        [pscustomobject]@{ Needle = "NativeAbiFunctionCount = `$NativeAbiFunctionCount"; Issue = "Runtime input artifact provenance must record the ABI function count" },
        [pscustomobject]@{ Needle = "ContainerImage = `$ContainerImage"; Issue = "Runtime input artifact provenance must record the container image for container-native producers" },
        [pscustomobject]@{ Needle = "ContainerImageId = `$ContainerImageId"; Issue = "Runtime input artifact provenance must record the resolved container image ID" },
        [pscustomobject]@{ Needle = "ContainerImageDigest = `$ContainerImageDigest"; Issue = "Runtime input artifact provenance must record the resolved container image digest" },
        [pscustomobject]@{ Needle = "ContainerDistro = `$ContainerDistro"; Issue = "Runtime input artifact provenance must record actual container distro evidence" },
        [pscustomobject]@{ Needle = "ContainerDistroVersion = `$ContainerDistroVersion"; Issue = "Runtime input artifact provenance must record actual container distro version evidence" },
        [pscustomobject]@{ Needle = "ContainerArchitecture = `$ContainerArchitecture"; Issue = "Runtime input artifact provenance must record actual container machine architecture" },
        [pscustomobject]@{ Needle = "ContainerPackageArchitecture = `$ContainerPackageArchitecture"; Issue = "Runtime input artifact provenance must record actual container package architecture" },
        [pscustomobject]@{ Needle = "ContainerLibc = `$ContainerLibc"; Issue = "Runtime input artifact provenance must record libc evidence for Linux container builds" },
        [pscustomobject]@{ Needle = "PowerShellVersion = `$PowerShellVersion"; Issue = "Runtime input artifact provenance must record an explicitly bootstrapped PowerShell version" },
        [pscustomobject]@{ Needle = "PowerShellArchiveSha256 = `$PowerShellArchiveSha256"; Issue = "Runtime input artifact provenance must record an explicitly verified PowerShell archive hash" },
        [pscustomobject]@{ Needle = "OpenCvExtraCMakeArgs = `$OpenCvExtraCMakeArgs"; Issue = "Runtime input artifact provenance must record distro-specific OpenCV CMake arguments" },
        [pscustomobject]@{ Needle = "OpenCvSourcePatchEvidence = `$OpenCvSourcePatchEvidence"; Issue = "Runtime input artifact provenance must record audited OpenCV source patch evidence" },
        [pscustomobject]@{ Needle = "BuildList = Get-OptionalStringProperty"; Issue = "Runtime input artifact provenance must record the profile build list from the runtime matrix" },
        [pscustomobject]@{ Needle = "JYPPX.OpenCV.Native"; Issue = "Runtime input artifact script must require the neutral native loader" },
        [pscustomobject]@{ Needle = 'return @("JYPPX.OpenCV.Native.dll")'; Issue = "Runtime input artifact script must expose only the neutral Windows loader" },
        [pscustomobject]@{ Needle = "OpenCV source LICENSE was not found"; Issue = "Runtime input artifact script must require OpenCV source license evidence" },
        [pscustomobject]@{ Needle = 'Join-Path $openCvInstallPath "sdk/etc/licenses"'; Issue = "Runtime input artifact script must preserve Android OpenCV install-license evidence" },
        [pscustomobject]@{ Needle = "Assert-NoAbsoluteElfRuntimePaths"; Issue = "Runtime input artifact script must audit real Linux ELF dynamic paths" },
        [pscustomobject]@{ Needle = 'ELF runtime contains an absolute RPATH/RUNPATH entry'; Issue = "Runtime input artifact script must reject producer absolute dynamic paths" },
        [pscustomobject]@{ Needle = "Runtime input artifact name: runtime-input-`$Rid-`$RuntimeProfile"; Issue = "Runtime input artifact script must print the neutral artifact name" })) {
    Assert-Contains -Violations $violations -Path $runtimeInputScriptPath -Text $runtimeInputScriptText -Needle $required.Needle -Issue $required.Issue
}

Assert-NotContains -Violations $violations -Path $runtimeInputScriptPath -Text $runtimeInputScriptText -Needle "New-SyntheticRuntimeInputs" -Issue "Runtime input artifact script must not call synthetic input generation"

Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "real_runtime_artifact_run_id" -Issue "Pack workflow must keep consuming producer run ids"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle 'runtime-input-${{ matrix.rid }}-${{ matrix.profile }}' -Issue "Pack workflow must consume the same neutral producer artifact names"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "provenance.ExcludedForeignToolDirectories | ConvertFrom-Json" -Issue "Windows pack validation must parse and constrain producer PATH exclusion evidence"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "`$expectedAsmPattern = if (`$profileName -eq 'mini')" -Issue "Windows pack validation must derive profile-specific generic-ASM and MLAS evidence"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "NativeWrapperSourceCount -ne 9" -Issue "Windows pack validation must require the exact mini wrapper source count"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "'src/core/persistence.cpp'," -Issue "Windows pack validation must retain persistence.cpp in the exact nine-source mini wrapper set"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "NativeAbiFunctionCount -ne 526" -Issue "Windows pack validation must require the exact mini ABI count"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle 'UBUNTU_24_04_ARM64_RUNTIME_INPUT_PROVENANCE_OK profile=$profileName files=$expectedPayloadFileCount modules=$expectedModuleCount sources=$expectedSourceCount abi_functions=$expectedAbiFunctionCount synthetic=false' -Issue "Pack runtime job must validate profile-derived real Ubuntu ARM64 provenance before packaging"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle "provenance.ElfAuditEvidence -notmatch `$expectedElfPattern" -Issue "Pack runtime job must require exact Ubuntu ARM64 producer ELF evidence"
Assert-Contains -Violations $violations -Path $packWorkflowPath -Text $packWorkflowText -Needle 'UBUNTU_22_04_ARM64_RUNTIME_INPUT_PROVENANCE_OK profile=$profileName files=$expectedPayloadFileCount modules=$expectedModuleCount sources=$expectedSourceCount abi_functions=$expectedAbiFunctionCount synthetic=false' -Issue "Pack runtime job must validate profile-derived Ubuntu 22.04 ARM64 host/container provenance before packaging"

foreach ($doc in @(
        [pscustomobject]@{ Path = $readmePath; Text = $readmeText },
        [pscustomobject]@{ Path = $linkedRuntimeBuildGuidePath; Text = $linkedRuntimeBuildGuideText })) {
    foreach ($needle in @(
            '`runtime-input.yml`',
            '`runtime-input-win-x64-full`',
            '`runtime-input-ubuntu.24.04-x64-full`',
            '`runtime-input-ubuntu.24.04-x64-mini`',
            '`runtime-input-ubuntu.24.04-arm64-full`',
            '`runtime-input-ubuntu.22.04-x64-full`',
            '`runtime-input-ubuntu.22.04-x64-mini`',
            '`runtime-input-ubuntu.22.04-arm64-full`',
            '`runtime-input-debian.12-x64-full`',
            '`runtime-input-debian.12-arm64-full`',
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

    foreach ($needle in @(
            '`CMAKE_ASM_COMPILER=NOTFOUND`',
            '`OPENCV_DNN_MLAS_ENABLED=0`',
            '`opencv_<module>500.dll`',
            '17 AMD64 DLL',
            'Linux SONAME')) {
        Assert-Contains -Violations $violations -Path $doc.Path -Text $doc.Text -Needle $needle -Issue "$($doc.Path) must keep factual Windows runtime evidence distinct from Linux payload evidence"
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
        Write-FixtureFile -Path (Join-Path $fixtureNativeDir "JYPPX.OpenCV.Native.dll")
        Write-FixtureFile -Path (Join-Path $fixtureSourceDir "LICENSE") -Text "OpenCV license fixture"
        Write-FixtureFile -Path (Join-Path (Join-Path (Join-Path $fixtureSourceDir "3rdparty") "ippicv") "readme.htm") -Text "ippicv fixture"
        Write-FixtureFile -Path (Join-Path (Join-Path $fixtureInstallDir "etc/licenses") "opencv-license.txt") -Text "install license fixture"

        $matrix = Get-Content -LiteralPath (Join-Path $repo "packaging/runtime/runtime-package-matrix.json") -Raw | ConvertFrom-Json
        foreach ($producerTarget in @(Get-RuntimeInputProducerTargets -Text $producerWorkflowText)) {
            $ridSpec = @($matrix.rids | Where-Object { $_.rid -eq $producerTarget.Rid } | Select-Object -First 1)
            if ($ridSpec.Count -eq 0) {
                throw "Fixture producer target RID was not found in runtime matrix: $($producerTarget.Rid)"
            }

            $expectedPlatformFamily = [string]$ridSpec[0].platformFamily
            $distroProperty = $ridSpec[0].PSObject.Properties["distro"]
            $distroVersionProperty = $ridSpec[0].PSObject.Properties["distroVersion"]
            $expectedDistro = if ($null -eq $distroProperty) { "" } else { [string]$distroProperty.Value }
            $expectedDistroVersion = if ($null -eq $distroVersionProperty) { "" } else { [string]$distroVersionProperty.Value }
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
            $isDirectUbuntuArm64 = $producerTarget.Rid -eq "ubuntu.24.04-arm64" -and $producerTarget.Profile -in @("full", "mini")
            $isUbuntu2204Arm64 = $producerTarget.Rid -eq "ubuntu.22.04-arm64" -and $producerTarget.Profile -in @("full", "mini")
            $isDebian1204Arm64 = $producerTarget.Rid -eq "debian.12-arm64" -and $producerTarget.Profile -in @("full", "mini")
            $isDebian1204X64 = $producerTarget.Rid -eq "debian.12-x64" -and $producerTarget.Profile -in @("full", "mini")
            $isFedora40X64 = $producerTarget.Rid -eq "fedora.40-x64" -and $producerTarget.Profile -in @("full", "mini")
            $isRhel9X64 = $producerTarget.Rid -eq "rhel.9-x64" -and $producerTarget.Profile -in @("full", "mini")
            $isArm64Container = $isUbuntu2204Arm64 -or $isDebian1204Arm64
            $isArm64Hosted = $isDirectUbuntuArm64 -or $isArm64Container
            $hasDetailedLinuxEvidence = $isArm64Hosted -or $isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64
            $isWindowsX64 = $producerTarget.Rid -eq "win-x64" -and $producerTarget.Profile -in @("full", "mini")
            $isWindowsX86 = $producerTarget.Rid -eq "win-x86" -and $producerTarget.Profile -eq "full"
            $isWindowsArm64 = $producerTarget.Rid -eq "win-arm64" -and $producerTarget.Profile -in @("full", "mini")
            $isWindowsTarget = $isWindowsX64 -or $isWindowsX86 -or $isWindowsArm64
            $isWindowsMini = $isWindowsTarget -and $producerTarget.Profile -eq "mini"
            $isAndroidTarget = $expectedPlatformFamily.Equals("android", [System.StringComparison]::OrdinalIgnoreCase)
            $runnerImage = if ($isWindowsArm64) { "win11-vs2026-arm64" } elseif ($isWindowsX64 -or $isWindowsX86) { "win25-vs2026" } elseif ($isArm64Hosted) { "ubuntu24-arm64" } elseif ($isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "ubuntu24" } else { "" }
            $runnerImageVersion = if ($isWindowsTarget -or $hasDetailedLinuxEvidence) { "fixture" } else { "" }
            $hostedDistro = if ($isWindowsTarget) { "windows" } elseif ($hasDetailedLinuxEvidence) { "ubuntu" } else { "" }
            $hostedDistroVersion = if ($isWindowsArm64) { "10.0.26200" } elseif ($isWindowsX64 -or $isWindowsX86) { "10.0.26100" } elseif ($hasDetailedLinuxEvidence) { "24.04" } else { "" }
            $hostedArchitecture = if ($isWindowsArm64) { "Arm64" } elseif ($isWindowsX64 -or $isWindowsX86) { "X64" } elseif ($isArm64Hosted) { "aarch64" } elseif ($isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "x86_64" } else { "" }
            $hostedPackageArchitecture = if ($isWindowsArm64) { "ARM64" } elseif ($isWindowsX86) { "x86" } elseif ($isWindowsX64) { "AMD64" } elseif ($isArm64Hosted) { "arm64" } elseif ($isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "amd64" } else { "" }
            $hostedLibc = if ($hasDetailedLinuxEvidence) { "glibc fixture" } else { "" }
            $hostedCpuModel = if ($isWindowsArm64) { "Cobalt 100 fixture" } elseif ($isWindowsX64 -or $isWindowsX86) { "AMD64 fixture" } elseif ($isArm64Hosted) { "Neoverse fixture" } elseif ($isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "x86-64 fixture" } else { "" }
            $hostedMemoryBytes = if ($isWindowsTarget -or $hasDetailedLinuxEvidence) { "17169428480" } else { "" }
            $hostedDiskAvailableBytes = if ($isWindowsTarget -or $hasDetailedLinuxEvidence) { "1" } else { "" }
            $hostedOsCaption = if ($isWindowsArm64) { "Microsoft Windows 11 Enterprise fixture" } elseif ($isWindowsX64 -or $isWindowsX86) { "Microsoft Windows Server fixture" } else { "" }
            $hostedOsVersion = if ($isWindowsArm64) { "10.0.26200" } elseif ($isWindowsX64 -or $isWindowsX86) { "10.0.26100" } else { "" }
            $hostedOsBuildNumber = if ($isWindowsArm64) { "26200" } elseif ($isWindowsX64 -or $isWindowsX86) { "26100" } else { "" }
            $hostedProcessArchitecture = if ($isWindowsArm64 -or $isArm64Hosted) { "Arm64" } elseif ($isWindowsX64 -or $isWindowsX86 -or $isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "X64" } else { "" }
            $visualStudioVersion = if ($isWindowsTarget) { "18.7.fixture" } else { "" }
            $msvcVersion = if ($isWindowsTarget) { "14.51.fixture (compiler 19.51.fixture)" } else { "" }
            $windowsSdkVersion = if ($isWindowsTarget) { "10.0.26100.0" } else { "" }
            $cmakeVersion = if ($isWindowsTarget -or $hasDetailedLinuxEvidence) { "cmake version fixture" } else { "" }
            $cmakeGenerator = if ($isWindowsTarget) { "Visual Studio 18 2026" } elseif ($hasDetailedLinuxEvidence) { "Ninja" } else { "" }
            $cmakePlatform = if ($isWindowsArm64) { "ARM64" } elseif ($isWindowsX86) { "Win32" } elseif ($isWindowsX64) { "x64" } else { "" }
            $buildConfiguration = if ($isWindowsTarget -or $hasDetailedLinuxEvidence) { "Release" } else { "" }
            $compilerPath = if ($isWindowsArm64) { "C:\fixture\Hostarm64\arm64\cl.exe" } elseif ($isWindowsX86) { "C:\fixture\Hostx64\x86\cl.exe" } elseif ($isWindowsX64) { "C:\fixture\Hostx64\x64\cl.exe" } elseif ($hasDetailedLinuxEvidence) { "/usr/bin/g++" } else { "" }
            $linkerPath = if ($isWindowsTarget) { $compilerPath -replace 'cl\.exe$', 'link.exe' } else { "" }
            $librarianPath = if ($isWindowsTarget) { $compilerPath -replace 'cl\.exe$', 'lib.exe' } else { "" }
            $dumpbinPath = if ($isWindowsTarget) { $compilerPath -replace 'cl\.exe$', 'dumpbin.exe' } else { "" }
            $compilerVersion = if ($isArm64Hosted) { "g++ fixture aarch64" } elseif ($isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "g++ fixture x86_64" } else { "" }
            $assemblerVersion = if ($isArm64Hosted) { "GNU assembler fixture aarch64" } elseif ($isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "GNU assembler fixture x86_64" } else { "" }
            $ninjaVersion = if ($hasDetailedLinuxEvidence) { "1.13.2" } else { "" }
            $dotNetVersion = if ($hasDetailedLinuxEvidence) { "8.0.fixture" } else { "" }
            $profileSpec = @($matrix.profiles | Where-Object { $_.name -eq $producerTarget.Profile } | Select-Object -First 1)
            if ($profileSpec.Count -eq 0) {
                throw "Fixture producer profile was not found in runtime matrix: $($producerTarget.Profile)"
            }
            $openCvExtraCMakeArgs = if ($isWindowsMini) { "-DCMAKE_ASM_COMPILER:FILEPATH=NOTFOUND" } elseif ($isAndroidTarget) { "-DANDROID_SUPPORT_FLEXIBLE_PAGE_SIZES=ON" } else { [string]$producerTarget.OpenCvExtraCMakeArgs }
            $openCvSourcePatchEvidence = if ($isWindowsArm64) {
                [ordered]@{
                    Path = "packaging/runtime/patches/windows-arm64-mlas-processor-case.patch"
                    Sha256 = (Get-FileHash -LiteralPath (Join-Path $repo "packaging/runtime/patches/windows-arm64-mlas-processor-case.patch") -Algorithm SHA256).Hash
                    Target = "3rdparty/mlas/CMakeLists.txt"
                    Reason = "accept-uppercase-cmake-arm64-for-mlas-detection"
                } | ConvertTo-Json -Compress
            }
            else {
                ""
            }
            $openCvCMakeArguments = if ($isWindowsArm64 -and $isWindowsMini) { '["-G","Visual Studio 18 2026","-A","ARM64","-DCMAKE_ASM_COMPILER:FILEPATH=NOTFOUND"]' } elseif ($isWindowsMini) { '["-G","Visual Studio 18 2026","-A","x64","-DCMAKE_ASM_COMPILER:FILEPATH=NOTFOUND"]' } elseif ($isWindowsX86) { '["-G","Visual Studio 18 2026","-A","Win32","-T","host=x64"]' } elseif ($isWindowsArm64) { '["-G","Visual Studio 18 2026","-A","ARM64"]' } elseif ($isWindowsX64) { '["-G","Visual Studio 18 2026","-A","x64"]' } elseif ($isAndroidTarget -or $hasDetailedLinuxEvidence) { "[`"-G`",`"Ninja`",`"-DBUILD_LIST=$([string]$profileSpec[0].buildList)`"]" } else { "" }
            $expectedModuleCount = @($profileSpec[0].modules).Count
            $peAuditEvidence = if ($isWindowsArm64) { "WINDOWS_PE_AUDIT_OK rid=win-arm64 profile=$($producerTarget.Profile) files=$($expectedModuleCount + 1) machine=ARM64 packaged_modules=$expectedModuleCount reachable_modules=$expectedModuleCount loader_opencv_imports=5 opencv_import_edges=12 missing_opencv_imports=0" } elseif ($isWindowsX86) { "WINDOWS_PE_AUDIT_OK rid=win-x86 profile=full files=$($expectedModuleCount + 1) machine=I386 packaged_modules=$expectedModuleCount reachable_modules=$expectedModuleCount loader_opencv_imports=5 opencv_import_edges=12 missing_opencv_imports=0" } elseif ($isWindowsX64) { "WINDOWS_PE_AUDIT_OK rid=win-x64 profile=$($producerTarget.Profile) files=$($expectedModuleCount + 1) machine=AMD64 packaged_modules=$expectedModuleCount reachable_modules=$expectedModuleCount loader_opencv_imports=5 opencv_import_edges=12 missing_opencv_imports=0" } else { "" }
            $expectedCanonicalCount = $expectedModuleCount + 1
            $expectedLinuxPayloadCount = ($expectedModuleCount * 3) + 1
            $elfAuditEvidence = if ($isAndroidTarget) { "ANDROID_ELF_EVIDENCE rid=$($producerTarget.Rid) profile=$($producerTarget.Profile) files=$expectedCanonicalCount required_modules=$expectedModuleCount min_page_alignment=16384 versioned_so=0 libcxx_shared=0" } elseif ($isDirectUbuntuArm64) { "UBUNTU_24_04_ARM64_PRODUCER_ELF_EVIDENCE profile=$($producerTarget.Profile) files=$expectedCanonicalCount runtime_files=$expectedLinuxPayloadCount machine=AArch64 origin=$expectedCanonicalCount producer_paths=0 direct_opencv=$expectedModuleCount missing_dependencies=0" } elseif ($isUbuntu2204Arm64) { "UBUNTU_22_04_ARM64_PRODUCER_ELF_EVIDENCE profile=$($producerTarget.Profile) files=$expectedCanonicalCount runtime_files=$expectedLinuxPayloadCount machine=AArch64 origin=$expectedCanonicalCount producer_paths=0 direct_opencv=$expectedModuleCount missing_dependencies=0" } elseif ($isDebian1204Arm64) { "DEBIAN_12_ARM64_PRODUCER_ELF_EVIDENCE profile=$($producerTarget.Profile) files=$expectedCanonicalCount runtime_files=$expectedLinuxPayloadCount machine=AArch64 origin=$expectedCanonicalCount producer_paths=0 direct_opencv=$expectedModuleCount missing_dependencies=0" } elseif ($isDebian1204X64) { "DEBIAN_12_X64_PRODUCER_ELF_EVIDENCE profile=$($producerTarget.Profile) files=$expectedCanonicalCount runtime_files=$expectedLinuxPayloadCount machine=X86-64 origin=$expectedCanonicalCount producer_paths=0 direct_opencv=$expectedModuleCount missing_dependencies=0" } elseif ($isFedora40X64) { "FEDORA_40_X64_PRODUCER_ELF_EVIDENCE profile=$($producerTarget.Profile) files=$expectedCanonicalCount runtime_files=$expectedLinuxPayloadCount machine=X86-64 origin=$expectedCanonicalCount producer_paths=0 direct_opencv=$expectedModuleCount missing_dependencies=0" } elseif ($isRhel9X64) { "RHEL_9_X64_PRODUCER_ELF_EVIDENCE profile=$($producerTarget.Profile) files=$expectedCanonicalCount runtime_files=$expectedLinuxPayloadCount machine=X86-64 origin=$expectedCanonicalCount producer_paths=0 direct_opencv=$expectedModuleCount missing_dependencies=0" } else { "" }
            $openCvCpuConfiguration = if ($isWindowsArm64) { "CPU_BASELINE:NEON;CPU_DISPATCH:" } elseif ($isWindowsX86) { "CPU_BASELINE:SSE2;CPU_DISPATCH:SSE4_1" } elseif ($isWindowsX64) { "CPU_BASELINE:SSE3;CPU_DISPATCH:SSE4_1" } elseif ($isArm64Hosted) { "CPU_BASELINE=NEON" } elseif ($isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "CPU_BASELINE=SSE3;CPU_DISPATCH=AVX2" } else { "" }
            $excludedForeignToolDirectories = if ($isWindowsTarget) { '["C:\\mingw64\\bin"]' } else { "" }
            $openCvAsmConfiguration = if ($isWindowsMini) { "CMAKE_ASM_COMPILER=NOTFOUND;OPENCV_DNN_MLAS_ENABLED=NOT_BUILT;OPENCV_DNN_MLAS_SKIP_REASON=dnn excluded by mini profile" } elseif ($isWindowsArm64) { "CMAKE_ASM_COMPILER=NOTFOUND;OPENCV_DNN_MLAS_ENABLED=0;OPENCV_DNN_MLAS_SKIP_REASON=no ASM compiler available for ARM64" } elseif ($isWindowsX86 -or $isWindowsX64) { "CMAKE_ASM_COMPILER=NOTFOUND;OPENCV_DNN_MLAS_ENABLED=0;OPENCV_DNN_MLAS_SKIP_REASON=no ASM compiler available for AMD64" } else { "" }
            $windowsTargetExecutionEvidence = if ($isWindowsX86) { "WINDOWS_X86_WOW64_PROBE_OK pointer_bits=32 process_machine=0x014C native_machine=0x8664" } else { "" }
            $hasMiniProfileEvidence = $isWindowsMini -or (($isAndroidTarget -or $isDirectUbuntuArm64 -or $isUbuntu2204Arm64 -or $isDebian1204Arm64 -or $isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) -and $producerTarget.Profile -eq "mini")
            $hasFullProfileEvidence = ($isWindowsTarget -and -not $isWindowsMini) -or (($isAndroidTarget -or $isDirectUbuntuArm64 -or $isUbuntu2204Arm64 -or $isDebian1204Arm64 -or $isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) -and $producerTarget.Profile -eq "full")
            $nativeWrapperSources = if ($hasMiniProfileEvidence) { '["src/error_state.cpp","src/version.cpp","src/core/mat.cpp","src/core/decomp.cpp","src/core/operations.cpp","src/core/persistence.cpp","src/videoio/videoio.cpp","src/imgcodecs.cpp","src/imgproc.cpp"]' } elseif ($hasFullProfileEvidence) { '["full-source-fixture"]' } else { "" }
            $nativeWrapperSourceCount = if ($hasMiniProfileEvidence) { "9" } elseif ($hasFullProfileEvidence) { "49" } else { "" }
            $nativeAbiFunctionCount = if ($hasMiniProfileEvidence) { "526" } elseif ($hasFullProfileEvidence) { "2656" } else { "" }
            $containerImageId = if ($isArm64Container -or $isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "sha256:fixture" } else { "" }
            $containerImageDigest = if ($isUbuntu2204Arm64) { "ubuntu@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982" } elseif ($isDebian1204Arm64) { "debian@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc" } elseif ($isDebian1204X64) { "debian@sha256:fixture" } elseif ($isFedora40X64) { "fedora@sha256:fixture" } elseif ($isRhel9X64) { "registry.access.redhat.com/ubi9/ubi@sha256:fixture" } else { "" }
            $containerArchitecture = if ($isArm64Container) { "aarch64" } elseif ($isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "x86_64" } else { "" }
            $containerPackageArchitecture = if ($isArm64Container) { "arm64" } elseif ($isDebian1204X64) { "amd64" } elseif ($isFedora40X64 -or $isRhel9X64) { "x86_64" } else { "" }
            $powerShellVersion = if ($isArm64Container) { "7.4.17" } elseif ($isDirectUbuntuArm64 -or $isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) { "7.6.fixture" } else { "" }
            $powerShellArchiveSha256 = if ($isArm64Container) { "68f3874cdb6cd564acf404103dfc410ee85435b02f0ad648e73a958853175d6c" } else { "" }
            $fixtureRuntimeDir = Join-Path $fixtureRoot "opencv-runtime-$($producerTarget.Rid)-$($producerTarget.Profile)"
            foreach ($module in @($profileSpec[0].modules)) {
                $runtimeFileName = if ($isWindowsTarget) { "opencv_$($module)500.dll" } elseif ($isAndroidTarget) { "libopencv_$module.so" } else { "libopencv_$module.so.5.0.0" }
                Write-FixtureFile -Path (Join-Path $fixtureRuntimeDir $runtimeFileName)
            }

            $standardInstallLicenseDir = Join-Path $fixtureInstallDir "etc/licenses"
            $androidInstallLicenseDir = Join-Path $fixtureInstallDir "sdk/etc/licenses"
            if ($isAndroidTarget) {
                if (Test-Path -LiteralPath $standardInstallLicenseDir -PathType Container) {
                    [IO.Directory]::Delete($standardInstallLicenseDir, $true)
                }
                Write-FixtureFile -Path (Join-Path $androidInstallLicenseDir "opencv-license.txt") -Text "Android install license fixture"
            }
            else {
                if (Test-Path -LiteralPath $androidInstallLicenseDir -PathType Container) {
                    [IO.Directory]::Delete($androidInstallLicenseDir, $true)
                }
                Write-FixtureFile -Path (Join-Path $standardInstallLicenseDir "opencv-license.txt") -Text "install license fixture"
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
                -RunnerImage $runnerImage `
                -RunnerImageVersion $runnerImageVersion `
                -HostedDistro $hostedDistro `
                -HostedDistroVersion $hostedDistroVersion `
                -HostedArchitecture $hostedArchitecture `
                -HostedPackageArchitecture $hostedPackageArchitecture `
                -HostedLibc $hostedLibc `
                -HostedCpuModel $hostedCpuModel `
                -HostedMemoryBytes $hostedMemoryBytes `
                -HostedDiskAvailableBytes $hostedDiskAvailableBytes `
                -HostedOsCaption $hostedOsCaption `
                -HostedOsVersion $hostedOsVersion `
                -HostedOsBuildNumber $hostedOsBuildNumber `
                -HostedProcessArchitecture $hostedProcessArchitecture `
                -VisualStudioVersion $visualStudioVersion `
                -MsvcVersion $msvcVersion `
                -WindowsSdkVersion $windowsSdkVersion `
                -CMakeVersion $cmakeVersion `
                -CMakeGenerator $cmakeGenerator `
                -CMakePlatform $cmakePlatform `
                -BuildConfiguration $buildConfiguration `
                -CompilerPath $compilerPath `
                -LinkerPath $linkerPath `
                -LibrarianPath $librarianPath `
                -DumpbinPath $dumpbinPath `
                -CompilerVersion $compilerVersion `
                -AssemblerVersion $assemblerVersion `
                -NinjaVersion $ninjaVersion `
                -DotNetVersion $dotNetVersion `
                -OpenCvCMakeArguments $openCvCMakeArguments `
                -PeAuditEvidence $peAuditEvidence `
                -WindowsTargetExecutionEvidence $windowsTargetExecutionEvidence `
                -ElfAuditEvidence $elfAuditEvidence `
                -OpenCvCpuConfiguration $openCvCpuConfiguration `
                -ExcludedForeignToolDirectories $excludedForeignToolDirectories `
                -OpenCvAsmConfiguration $openCvAsmConfiguration `
                -NativeWrapperSources $nativeWrapperSources `
                -NativeWrapperSourceCount $nativeWrapperSourceCount `
                -NativeAbiFunctionCount $nativeAbiFunctionCount `
                -ContainerImage ([string]$producerTarget.ContainerImage) `
                -ContainerImageId $containerImageId `
                -ContainerImageDigest $containerImageDigest `
                -ContainerDistro $containerDistro `
                -ContainerDistroVersion $containerDistroVersion `
                -ContainerArchitecture $containerArchitecture `
                -ContainerPackageArchitecture $containerPackageArchitecture `
                -ContainerLibc $containerLibc `
                -PowerShellVersion $powerShellVersion `
                -PowerShellArchiveSha256 $powerShellArchiveSha256 `
                -OpenCvExtraCMakeArgs $openCvExtraCMakeArgs `
                -OpenCvSourcePatchEvidence $openCvSourcePatchEvidence `
                -OutputRoot $fixtureOutputRoot

            $manifestPath = Join-Path (Join-Path $fixtureOutputRoot "$($producerTarget.Rid)-$($producerTarget.Profile)") "runtime-input.provenance.json"
            if (-not (Test-Path -LiteralPath $manifestPath -PathType Leaf)) {
                throw "Fixture runtime input provenance was not written: $manifestPath"
            }

            $installLicenseArtifact = Join-Path (Split-Path $manifestPath -Parent) "opencv-install/etc/licenses/opencv-license.txt"
            if (-not (Test-Path -LiteralPath $installLicenseArtifact -PathType Leaf)) {
                throw "Fixture runtime input did not normalize install-license layout for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
            if ($manifest.SyntheticRuntimeInputs -ne $false) {
                throw "Fixture provenance did not mark SyntheticRuntimeInputs=false for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if (-not ([string]$manifest.PlatformFamily).Equals($expectedPlatformFamily, [System.StringComparison]::OrdinalIgnoreCase)) {
                throw "Fixture provenance did not record the matrix PlatformFamily for $($producerTarget.Rid)/$($producerTarget.Profile)."
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

            if ($hasDetailedLinuxEvidence) {
                if (-not ([string]$manifest.RunnerImage).Equals($runnerImage, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.RunnerImageVersion).Equals($runnerImageVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedDistro).Equals($hostedDistro, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedDistroVersion).Equals($hostedDistroVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedArchitecture).Equals($hostedArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedPackageArchitecture).Equals($hostedPackageArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedLibc).Equals($hostedLibc, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedCpuModel).Equals($hostedCpuModel, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedDiskAvailableBytes).Equals($hostedDiskAvailableBytes, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.OpenCvCpuConfiguration).Equals($openCvCpuConfiguration, [System.StringComparison]::Ordinal)) {
                throw "Fixture provenance did not retain the complete detailed Linux hosted evidence."
                }
            }

            if ($isDirectUbuntuArm64) {
                if (-not ([string]$manifest.HostedMemoryBytes).Equals($hostedMemoryBytes, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedProcessArchitecture).Equals($hostedProcessArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CMakeVersion).Equals($cmakeVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CMakeGenerator).Equals($cmakeGenerator, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.BuildConfiguration).Equals($buildConfiguration, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CompilerPath).Equals($compilerPath, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CompilerVersion).Equals($compilerVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.AssemblerVersion).Equals($assemblerVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NinjaVersion).Equals($ninjaVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.PowerShellVersion).Equals($powerShellVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.DotNetVersion).Equals($dotNetVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.OpenCvCMakeArguments).Equals($openCvCMakeArguments, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.ElfAuditEvidence).Equals($elfAuditEvidence, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NativeWrapperSources).Equals($nativeWrapperSources, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NativeWrapperSourceCount).Equals($nativeWrapperSourceCount, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NativeAbiFunctionCount).Equals($nativeAbiFunctionCount, [System.StringComparison]::Ordinal)) {
                    throw "Fixture provenance did not retain the complete direct Ubuntu ARM64 toolchain, profile, and ELF evidence."
                }
            }

            if ($isDebian1204X64 -or $isFedora40X64 -or $isRhel9X64) {
                if (-not ([string]$manifest.HostedMemoryBytes).Equals($hostedMemoryBytes, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedProcessArchitecture).Equals($hostedProcessArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CMakeVersion).Equals($cmakeVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CMakeGenerator).Equals($cmakeGenerator, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.BuildConfiguration).Equals($buildConfiguration, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CompilerPath).Equals($compilerPath, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CompilerVersion).Equals($compilerVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.AssemblerVersion).Equals($assemblerVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NinjaVersion).Equals($ninjaVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.PowerShellVersion).Equals($powerShellVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.DotNetVersion).Equals($dotNetVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.OpenCvCMakeArguments).Equals($openCvCMakeArguments, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.ElfAuditEvidence).Equals($elfAuditEvidence, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NativeWrapperSources).Equals($nativeWrapperSources, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NativeWrapperSourceCount).Equals($nativeWrapperSourceCount, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NativeAbiFunctionCount).Equals($nativeAbiFunctionCount, [System.StringComparison]::Ordinal)) {
                    throw "Fixture provenance did not retain the complete x64 glibc container toolchain, profile, and ELF evidence."
                }
            }

            if ($isWindowsTarget) {
                if (-not ([string]$manifest.RunnerImage).Equals($runnerImage, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.RunnerImageVersion).Equals($runnerImageVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedArchitecture).Equals($hostedArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedPackageArchitecture).Equals($hostedPackageArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedMemoryBytes).Equals($hostedMemoryBytes, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedProcessArchitecture).Equals($hostedProcessArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedOsCaption).Equals($hostedOsCaption, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedOsVersion).Equals($hostedOsVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.HostedOsBuildNumber).Equals($hostedOsBuildNumber, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.VisualStudioVersion).Equals($visualStudioVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.MsvcVersion).Equals($msvcVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.WindowsSdkVersion).Equals($windowsSdkVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CMakeGenerator).Equals($cmakeGenerator, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CMakePlatform).Equals($cmakePlatform, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.BuildConfiguration).Equals($buildConfiguration, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.CompilerPath).Equals($compilerPath, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.LinkerPath).Equals($linkerPath, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.LibrarianPath).Equals($librarianPath, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.DumpbinPath).Equals($dumpbinPath, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.ExcludedForeignToolDirectories).Equals($excludedForeignToolDirectories, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.OpenCvAsmConfiguration).Equals($openCvAsmConfiguration, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.PeAuditEvidence).Equals($peAuditEvidence, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.WindowsTargetExecutionEvidence).Equals($windowsTargetExecutionEvidence, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NativeWrapperSources).Equals($nativeWrapperSources, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NativeWrapperSourceCount).Equals($nativeWrapperSourceCount, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.NativeAbiFunctionCount).Equals($nativeAbiFunctionCount, [System.StringComparison]::Ordinal)) {
                    throw "Fixture provenance did not retain the complete Windows hosted, toolchain, build, and PE evidence."
                }
            }

            if (-not ([string]$manifest.ContainerImage).Equals([string]$producerTarget.ContainerImage, [System.StringComparison]::Ordinal)) {
                throw "Fixture provenance ContainerImage did not match producer target container image for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if ($isArm64Container) {
                if (-not ([string]$manifest.ContainerImageId).Equals($containerImageId, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.ContainerImageDigest).Equals($containerImageDigest, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.ContainerArchitecture).Equals($containerArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.ContainerPackageArchitecture).Equals($containerPackageArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.PowerShellVersion).Equals($powerShellVersion, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.PowerShellArchiveSha256).Equals($powerShellArchiveSha256, [System.StringComparison]::Ordinal)) {
                    throw "Fixture provenance did not retain the complete ARM64 container and PowerShell evidence."
                }
            }

            if ($isDebian1204X64 -or $isFedora40X64) {
                if (-not ([string]$manifest.ContainerImageId).Equals($containerImageId, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.ContainerImageDigest).Equals($containerImageDigest, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.ContainerArchitecture).Equals($containerArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.ContainerPackageArchitecture).Equals($containerPackageArchitecture, [System.StringComparison]::Ordinal) -or
                    -not ([string]$manifest.PowerShellVersion).Equals($powerShellVersion, [System.StringComparison]::Ordinal) -or
                    -not [string]::IsNullOrWhiteSpace([string]$manifest.PowerShellArchiveSha256)) {
                    throw "Fixture provenance did not retain the complete x64 glibc container and package-installed PowerShell evidence."
                }
            }

            if (-not ([string]$manifest.OpenCvExtraCMakeArgs).Equals($openCvExtraCMakeArgs, [System.StringComparison]::Ordinal)) {
                throw "Fixture provenance OpenCvExtraCMakeArgs did not match producer target build arguments for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }
            if (-not ([string]$manifest.OpenCvSourcePatchEvidence).Equals($openCvSourcePatchEvidence, [System.StringComparison]::Ordinal)) {
                throw "Fixture provenance OpenCvSourcePatchEvidence did not match producer source patch evidence for $($producerTarget.Rid)/$($producerTarget.Profile)."
            }

            if ([string]::IsNullOrWhiteSpace([string]$producerTarget.ContainerImage)) {
                if (-not [string]::IsNullOrWhiteSpace([string]$manifest.ContainerDistro) -or
                    -not [string]::IsNullOrWhiteSpace([string]$manifest.ContainerDistroVersion) -or
                    -not [string]::IsNullOrWhiteSpace([string]$manifest.ContainerImageId) -or
                    -not [string]::IsNullOrWhiteSpace([string]$manifest.ContainerImageDigest) -or
                    -not [string]::IsNullOrWhiteSpace([string]$manifest.ContainerArchitecture) -or
                    -not [string]::IsNullOrWhiteSpace([string]$manifest.ContainerPackageArchitecture) -or
                    (-not $isDirectUbuntuArm64 -and -not [string]::IsNullOrWhiteSpace([string]$manifest.PowerShellVersion)) -or
                    -not [string]::IsNullOrWhiteSpace([string]$manifest.PowerShellArchiveSha256) -or
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

            $expectedLoaderFile = if ($isWindowsTarget) { "JYPPX.OpenCV.Native.dll" } else { "libJYPPX.OpenCV.Native.so" }
            if (@($manifest.NativeLoaderFiles).Count -ne 1 -or
                -not ([string]$manifest.NativeLoaderFiles[0].FileName).Equals($expectedLoaderFile, [System.StringComparison]::Ordinal)) {
                throw "Fixture provenance did not include exactly the neutral native loader for $($producerTarget.Rid)/$($producerTarget.Profile)."
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
Write-Host "Producer artifacts: runtime-input-win-x64-full, runtime-input-win-x64-mini, runtime-input-win-x86-full, runtime-input-win-arm64-full, runtime-input-win-arm64-mini, runtime-input-ubuntu.24.04-x64-full, runtime-input-ubuntu.24.04-x64-mini, runtime-input-ubuntu.24.04-arm64-full, runtime-input-ubuntu.24.04-arm64-mini, runtime-input-ubuntu.22.04-x64-full, runtime-input-ubuntu.22.04-x64-mini, runtime-input-ubuntu.22.04-arm64-full, runtime-input-ubuntu.22.04-arm64-mini, runtime-input-debian.12-x64-full, runtime-input-debian.12-x64-mini, runtime-input-debian.12-arm64-full, runtime-input-debian.12-arm64-mini, runtime-input-fedora.40-x64-full, runtime-input-fedora.40-x64-mini, runtime-input-rhel.9-x64-full, runtime-input-rhel.9-x64-mini, runtime-input-rocky.9-x64-full, runtime-input-rocky.9-x64-mini, runtime-input-alpine.3.20-x64-full, runtime-input-alpine.3.20-x64-mini, runtime-input-android-arm64-full, runtime-input-android-arm64-mini, runtime-input-android-arm-full, runtime-input-android-arm-mini, runtime-input-android-x64-full, runtime-input-android-x64-mini, runtime-input-android-x86-full, runtime-input-android-x86-mini."
Write-Host "Producer handoff layout: native-wrapper, opencv-runtime, opencv-source, optional opencv-install."
