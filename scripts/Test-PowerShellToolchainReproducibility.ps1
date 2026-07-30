param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workflowRoot = Join-Path $repo ".github/workflows"
$aggregatePath = Join-Path $repo "scripts/Test-ProjectInvariants.ps1"

$expectedDebianPackageVersion = "7.6.4-1.deb"
$expectedRpmPackageNevra = "powershell-7.6.4-1.rh.x86_64"
$expectedRpmPackageIdentity = "powershell|7.6.4-1.rh|x86_64|(none)"
$expectedPackageSemanticVersion = "7.6.4"
$expectedAlpinePackageVersion = "7.4.6-r1"
$expectedAlpineSemanticVersion = "7.4.6"
$expectedArm64ArchiveVersion = "7.4.17"
$expectedArm64ArchiveSha256 = "68f3874cdb6cd564acf404103dfc410ee85435b02f0ad648e73a958853175d6c"

function Add-Violation {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][string]$Path,
        [int]$Line = 0,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Get-RelativePath {
    param([Parameter(Mandatory)][string]$Path)

    return ([System.IO.Path]::GetRelativePath($repo, $Path)) -replace "\\", "/"
}

function Get-LineNumber {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][int]$Index
    )

    if ($Index -lt 0) {
        return 0
    }

    return [regex]::Matches($Text.Substring(0, $Index), "\n").Count + 1
}

function Get-TextCount {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Needle
    )

    return [regex]::Matches($Text, [regex]::Escape($Needle)).Count
}

function Get-DelimitedContext {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$StartNeedle,
        [Parameter(Mandatory)][string]$EndNeedle,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Name,
        [int]$SearchStart = 0
    )

    $start = $Text.IndexOf($StartNeedle, $SearchStart, [System.StringComparison]::Ordinal)
    if ($start -lt 0) {
        return $null
    }

    $end = $Text.IndexOf($EndNeedle, $start + $StartNeedle.Length, [System.StringComparison]::Ordinal)
    if ($end -lt 0) {
        return $null
    }

    $end += $EndNeedle.Length
    return [pscustomobject]@{
        Name = $Name
        Path = $Path
        Line = Get-LineNumber -Text $Text -Index $start
        Text = $Text.Substring($start, $end - $start)
    }
}

function Get-JobContext {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][string]$JobName,
        [Parameter(Mandatory)][string]$Path
    )

    $start = -1
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -ceq ("  {0}:" -f $JobName)) {
            $start = $index
            break
        }
    }
    if ($start -lt 0) {
        return $null
    }

    $end = $Lines.Count
    for ($index = $start + 1; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -match '^  [A-Za-z0-9_-]+:\s*$') {
            $end = $index
            break
        }
    }

    return [pscustomobject]@{
        Name = $JobName
        Path = $Path
        Line = $start + 1
        Text = ($Lines[$start..($end - 1)] -join [Environment]::NewLine)
    }
}

function Add-Context {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Contexts,
        [AllowNull()][object]$Context,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Issue
    )

    if ($null -eq $Context) {
        Add-Violation -Violations $script:violations -Path $Path -Issue $Issue
        return
    }

    $Contexts.Add($Context)
}

function Assert-Contains {
    param(
        [Parameter(Mandatory)][object]$Context,
        [Parameter(Mandatory)][string]$Needle,
        [Parameter(Mandatory)][string]$Issue
    )

    if ($Context.Text.IndexOf($Needle, [System.StringComparison]::Ordinal) -lt 0) {
        Add-Violation -Violations $script:violations -Path $Context.Path -Line $Context.Line -Issue $Issue -Text "$($Context.Name): missing '$Needle'"
    }
}

function Assert-Matches {
    param(
        [Parameter(Mandatory)][object]$Context,
        [Parameter(Mandatory)][string]$Pattern,
        [Parameter(Mandatory)][string]$Issue
    )

    if (-not [regex]::IsMatch($Context.Text, $Pattern)) {
        Add-Violation -Violations $script:violations -Path $Context.Path -Line $Context.Line -Issue $Issue -Text "$($Context.Name): missing pattern '$Pattern'"
    }
}

function Assert-NotMatches {
    param(
        [Parameter(Mandatory)][object]$Context,
        [Parameter(Mandatory)][string]$Pattern,
        [Parameter(Mandatory)][string]$Issue
    )

    foreach ($match in [regex]::Matches($Context.Text, $Pattern)) {
        Add-Violation -Violations $script:violations -Path $Context.Path -Line ($Context.Line + (Get-LineNumber -Text $Context.Text -Index $match.Index) - 1) -Issue $Issue -Text "$($Context.Name): $($match.Value)"
    }
}

function Assert-OrderedTokens {
    param(
        [Parameter(Mandatory)][object]$Context,
        [Parameter(Mandatory)][string[]]$Tokens,
        [Parameter(Mandatory)][string]$Issue
    )

    $previous = -1
    foreach ($token in $Tokens) {
        $index = $Context.Text.IndexOf($token, $previous + 1, [System.StringComparison]::Ordinal)
        if ($index -lt 0 -or $index -le $previous) {
            Add-Violation -Violations $script:violations -Path $Context.Path -Line $Context.Line -Issue $Issue -Text "$($Context.Name): missing/out-of-order '$token'"
            return
        }
        $previous = $index
    }
}

function Assert-GlobalCount {
    param(
        [Parameter(Mandatory)][string]$Needle,
        [Parameter(Mandatory)][int]$ExpectedCount,
        [Parameter(Mandatory)][string]$Issue
    )

    $count = Get-TextCount -Text $script:allWorkflowText -Needle $Needle
    if ($count -ne $ExpectedCount) {
        Add-Violation -Violations $script:violations -Path ".github/workflows" -Issue $Issue -Text "'$Needle' actual=$count expected=$ExpectedCount"
    }
}

$script:violations = [System.Collections.Generic.List[object]]::new()
$expectedWorkflowNames = @(
    "build-managed.yml",
    "build-native.yml",
    "docs.yml",
    "pack.yml",
    "runtime-input.yml"
)

$workflowFiles = @(Get-ChildItem -LiteralPath $workflowRoot -File | Where-Object { $_.Extension -in @(".yml", ".yaml") } | Sort-Object Name)
$workflowDifference = @(Compare-Object -ReferenceObject $expectedWorkflowNames -DifferenceObject @($workflowFiles.Name))
if ($workflowDifference.Count -gt 0) {
    Add-Violation -Violations $script:violations -Path ".github/workflows" -Issue "PowerShell toolchain guard requires the exact five-workflow set" -Text (($workflowDifference | ForEach-Object { "$($_.SideIndicator)$($_.InputObject)" }) -join ", ")
}

$workflowRecords = @(
    foreach ($workflowFile in $workflowFiles) {
        [pscustomobject]@{
            Path = Get-RelativePath -Path $workflowFile.FullName
            Text = [System.IO.File]::ReadAllText($workflowFile.FullName)
        }
    }
)
$script:allWorkflowText = (@($workflowRecords.Text) -join [Environment]::NewLine)

$runtimePath = Join-Path $workflowRoot "runtime-input.yml"
$packPath = Join-Path $workflowRoot "pack.yml"
$runtimeRelativePath = Get-RelativePath -Path $runtimePath
$packRelativePath = Get-RelativePath -Path $packPath
$runtimeText = [System.IO.File]::ReadAllText($runtimePath)
$packText = [System.IO.File]::ReadAllText($packPath)
$packLines = [System.IO.File]::ReadAllLines($packPath)
$script:controlledText = $runtimeText + [Environment]::NewLine + $packText

$runtimeApt = Get-DelimitedContext -Text $runtimeText -StartNeedle '                    test "$package_architecture" = "amd64"' -EndNeedle '                    echo "POWERSHELL_APT_TOOLCHAIN_EVIDENCE' -Path $runtimeRelativePath -Name "runtime-input/debian.12-x64"
$runtimeApk = Get-DelimitedContext -Text $runtimeText -StartNeedle '                alpine)' -EndNeedle '                  echo "POWERSHELL_APK_TOOLCHAIN_EVIDENCE' -Path $runtimeRelativePath -Name "runtime-input/alpine.3.20-x64"
$runtimeRpm = Get-DelimitedContext -Text $runtimeText -StartNeedle '                fedora|rhel|rocky)' -EndNeedle '                  echo "POWERSHELL_RPM_TOOLCHAIN_EVIDENCE' -Path $runtimeRelativePath -Name "runtime-input/fedora-rocky-rhel"
$runtimeArm64Archive = Get-DelimitedContext -Text $runtimeText -StartNeedle '                  if [ "$package_architecture" = "arm64" ]; then' -EndNeedle '                      echo "DEBIAN_12_ARM64_POWERSHELL_EVIDENCE' -Path $runtimeRelativePath -Name "runtime-input/ubuntu-debian-arm64-archive"

$aptContexts = [System.Collections.Generic.List[object]]::new()
Add-Context -Contexts $aptContexts -Context $runtimeApt -Path $runtimeRelativePath -Issue "Runtime Debian APT PowerShell toolchain context was not found"
Add-Context -Contexts $aptContexts -Context (Get-JobContext -Lines $packLines -JobName "verify-targeted-real-debian" -Path $packRelativePath) -Path $packRelativePath -Issue "Pack Debian APT PowerShell toolchain context was not found"

$rpmContexts = [System.Collections.Generic.List[object]]::new()
Add-Context -Contexts $rpmContexts -Context $runtimeRpm -Path $runtimeRelativePath -Issue "Runtime RPM PowerShell toolchain context was not found"
foreach ($jobName in @("verify-targeted-real-fedora", "verify-targeted-real-rocky", "verify-targeted-real-rhel")) {
    Add-Context -Contexts $rpmContexts -Context (Get-JobContext -Lines $packLines -JobName $jobName -Path $packRelativePath) -Path $packRelativePath -Issue "Pack RPM PowerShell toolchain context was not found"
}

$apkContexts = [System.Collections.Generic.List[object]]::new()
Add-Context -Contexts $apkContexts -Context $runtimeApk -Path $runtimeRelativePath -Issue "Runtime Alpine APK PowerShell toolchain context was not found"
Add-Context -Contexts $apkContexts -Context (Get-JobContext -Lines $packLines -JobName "verify-targeted-real-alpine" -Path $packRelativePath) -Path $packRelativePath -Issue "Pack Alpine APK PowerShell toolchain context was not found"

$arm64ArchiveContexts = [System.Collections.Generic.List[object]]::new()
Add-Context -Contexts $arm64ArchiveContexts -Context $runtimeArm64Archive -Path $runtimeRelativePath -Issue "Runtime ARM64 archive PowerShell context was not found"
Add-Context -Contexts $arm64ArchiveContexts -Context (Get-JobContext -Lines $packLines -JobName "verify-targeted-real-ubuntu2204-arm64" -Path $packRelativePath) -Path $packRelativePath -Issue "Pack Ubuntu 22.04 ARM64 archive PowerShell context was not found"
Add-Context -Contexts $arm64ArchiveContexts -Context (Get-JobContext -Lines $packLines -JobName "verify-targeted-real-debian-arm64" -Path $packRelativePath) -Path $packRelativePath -Issue "Pack Debian 12 ARM64 archive PowerShell context was not found"

if ($aptContexts.Count -ne 2) {
    Add-Violation -Violations $script:violations -Path ".github/workflows" -Issue "PowerShell APT package-manager contexts must be classified exactly" -Text "actual=$($aptContexts.Count) expected=2"
}
if ($rpmContexts.Count -ne 4) {
    Add-Violation -Violations $script:violations -Path ".github/workflows" -Issue "PowerShell RPM package-manager contexts must be classified exactly" -Text "actual=$($rpmContexts.Count) expected=4"
}
if ($apkContexts.Count -ne 2) {
    Add-Violation -Violations $script:violations -Path ".github/workflows" -Issue "PowerShell APK package-manager contexts must be classified exactly" -Text "actual=$($apkContexts.Count) expected=2"
}
if ($arm64ArchiveContexts.Count -ne 3) {
    Add-Violation -Violations $script:violations -Path ".github/workflows" -Issue "PowerShell ARM64 archive contexts must be classified exactly" -Text "actual=$($arm64ArchiveContexts.Count) expected=3"
}

foreach ($context in $aptContexts) {
    Assert-Contains -Context $context -Needle 'apt-get install -y --no-install-recommends "powershell=$POWERSHELL_DEBIAN_PACKAGE_VERSION"' -Issue "APT contexts must install the exact audited PowerShell Debian package"
    Assert-Contains -Context $context -Needle 'dpkg-query -W -f=' -Issue "APT contexts must query the installed PowerShell package identity"
    Assert-Contains -Context $context -Needle 'Version' -Issue "APT contexts must query the installed PowerShell package version"
    Assert-Contains -Context $context -Needle 'Architecture' -Issue "APT contexts must query the installed PowerShell package architecture"
    Assert-Contains -Context $context -Needle 'test "$installed_powershell_package_version" = "$POWERSHELL_DEBIAN_PACKAGE_VERSION"' -Issue "APT contexts must compare the installed package version to the audited pin"
    Assert-Contains -Context $context -Needle 'test "$installed_powershell_package_architecture" = "amd64"' -Issue "APT contexts must require the installed amd64 PowerShell package"
    Assert-Contains -Context $context -Needle 'PSVersionTable.PSVersion.ToString()' -Issue "APT contexts must read the actual pwsh semantic version"
    Assert-Contains -Context $context -Needle 'test "$installed_powershell_version" = "$POWERSHELL_PACKAGE_SEMANTIC_VERSION"' -Issue "APT contexts must compare actual pwsh version to the audited semantic pin"
    Assert-Matches -Context $context -Pattern 'test "\$(powershell_)?process_architecture" = "X64"' -Issue "APT contexts must require an x64 PowerShell process"
    Assert-Contains -Context $context -Needle 'POWERSHELL_APT_TOOLCHAIN_EVIDENCE' -Issue "APT contexts must emit exact PowerShell toolchain evidence"
    Assert-NotMatches -Context $context -Pattern '(?m)apt-get install[^\r\n]*--no-install-recommends[^\r\n]*\bpowershell\b(?![=\$])' -Issue "Unversioned APT PowerShell install is forbidden"
    Assert-OrderedTokens -Context $context -Tokens @('MICROSOFT_APT_TRUST_EVIDENCE', 'apt-get install -y --no-install-recommends "powershell=$POWERSHELL_DEBIAN_PACKAGE_VERSION"', 'test "$installed_powershell_package_version" = "$POWERSHELL_DEBIAN_PACKAGE_VERSION"', 'PSVersionTable.PSVersion.ToString()', 'POWERSHELL_APT_TOOLCHAIN_EVIDENCE') -Issue "APT trust, exact install, installed package check, pwsh check, and evidence must remain ordered"
}

foreach ($context in $rpmContexts) {
    Assert-Contains -Context $context -Needle 'dnf-plugins-core' -Issue "RPM contexts must install dnf-plugins-core before repoquery"
    Assert-Contains -Context $context -Needle 'dnf repoquery --disablerepo="*" --enablerepo="packages-microsoft-com-prod" --qf "%{name}-%{evr}.%{arch}|%{repoid}" "$POWERSHELL_RPM_PACKAGE_NEVRA"' -Issue "RPM contexts must resolve the exact audited PowerShell RPM NEVRA from the hardened repo"
    Assert-Contains -Context $context -Needle 'grep -Fqx "$POWERSHELL_RPM_PACKAGE_NEVRA|packages-microsoft-com-prod"' -Issue "RPM contexts must reject a repoquery result from any other repository"
    Assert-Contains -Context $context -Needle 'dnf install -y "$POWERSHELL_RPM_PACKAGE_NEVRA"' -Issue "RPM contexts must install the exact audited PowerShell RPM"
    Assert-Contains -Context $context -Needle 'rpm -q --qf' -Issue "RPM contexts must query installed package identity"
    Assert-Contains -Context $context -Needle '%{VENDOR}' -Issue "RPM contexts must include vendor in installed package identity"
    Assert-Contains -Context $context -Needle ('test "$installed_powershell_package_identity" = "{0}"' -f $expectedRpmPackageIdentity) -Issue "RPM contexts must compare installed package identity to the audited pin"
    Assert-Contains -Context $context -Needle 'PSVersionTable.PSVersion.ToString()' -Issue "RPM contexts must read the actual pwsh semantic version"
    Assert-Contains -Context $context -Needle 'test "$installed_powershell_version" = "$POWERSHELL_PACKAGE_SEMANTIC_VERSION"' -Issue "RPM contexts must compare actual pwsh version to the audited semantic pin"
    Assert-Matches -Context $context -Pattern 'test "\$(powershell_)?process_architecture" = "X64"' -Issue "RPM contexts must require an x64 PowerShell process"
    Assert-Contains -Context $context -Needle 'POWERSHELL_RPM_TOOLCHAIN_EVIDENCE' -Issue "RPM contexts must emit exact PowerShell toolchain evidence"
    Assert-NotMatches -Context $context -Pattern '(?m)dnf install -y powershell($|\s)' -Issue "Unversioned RPM PowerShell install is forbidden"
    Assert-OrderedTokens -Context $context -Tokens @('MICROSOFT_RPM_TRUST_EVIDENCE', 'dnf repoquery --disablerepo="*" --enablerepo="packages-microsoft-com-prod"', 'dnf install -y "$POWERSHELL_RPM_PACKAGE_NEVRA"', 'test "$installed_powershell_package_identity" = "powershell|7.6.4-1.rh|x86_64|(none)"', 'PSVersionTable.PSVersion.ToString()', 'POWERSHELL_RPM_TOOLCHAIN_EVIDENCE') -Issue "RPM trust, exact repoquery, exact install, package identity, pwsh check, and evidence must remain ordered"
}

foreach ($context in $apkContexts) {
    Assert-Contains -Context $context -Needle 'powershell=$POWERSHELL_ALPINE_PACKAGE_VERSION' -Issue "APK contexts must install the exact audited Alpine PowerShell package"
    Assert-Contains -Context $context -Needle 'apk info -e -vv powershell' -Issue "APK contexts must query the installed PowerShell package identity"
    Assert-Contains -Context $context -Needle 'test "$installed_powershell_package" = "powershell-$POWERSHELL_ALPINE_PACKAGE_VERSION - A cross-platform automation and configuration tool/framework"' -Issue "APK contexts must compare the installed package identity to the audited pin"
    Assert-Contains -Context $context -Needle 'PSVersionTable.PSVersion.ToString()' -Issue "APK contexts must read the actual pwsh semantic version"
    Assert-Contains -Context $context -Needle 'test "$installed_powershell_version" = "$POWERSHELL_ALPINE_SEMANTIC_VERSION"' -Issue "APK contexts must compare actual pwsh version to the audited semantic pin"
    Assert-Contains -Context $context -Needle 'test "$powershell_process_architecture" = "X64"' -Issue "APK contexts must require an x64 PowerShell process"
    Assert-Contains -Context $context -Needle 'POWERSHELL_APK_TOOLCHAIN_EVIDENCE' -Issue "APK contexts must emit exact PowerShell toolchain evidence"
    Assert-NotMatches -Context $context -Pattern '(?m)^\s*powershell\s*\\?\s*$' -Issue "Unversioned APK PowerShell install is forbidden"
    Assert-OrderedTokens -Context $context -Tokens @('ALPINE_3_20_REPOSITORY_EVIDENCE', 'powershell=$POWERSHELL_ALPINE_PACKAGE_VERSION', 'apk info -e -vv powershell', 'PSVersionTable.PSVersion.ToString()', 'POWERSHELL_APK_TOOLCHAIN_EVIDENCE') -Issue "APK repository evidence, exact install, installed package check, pwsh check, and evidence must remain ordered"
}

foreach ($context in $arm64ArchiveContexts) {
    Assert-Contains -Context $context -Needle 'powershell_archive="/tmp/powershell-$POWERSHELL_VERSION-linux-arm64.tar.gz"' -Issue "ARM64 contexts must use only the version-pinned PowerShell archive filename"
    Assert-Contains -Context $context -Needle 'PowerShell/PowerShell/releases/download/v$POWERSHELL_VERSION/powershell-$POWERSHELL_VERSION-linux-arm64.tar.gz' -Issue "ARM64 contexts must download only the exact version-pinned PowerShell archive"
    Assert-Contains -Context $context -Needle 'echo "$POWERSHELL_SHA256  $powershell_archive" | sha256sum -c -' -Issue "ARM64 contexts must verify the exact audited PowerShell archive SHA256 before extraction"
    Assert-Contains -Context $context -Needle 'tar -xzf "$powershell_archive"' -Issue "ARM64 contexts must extract only the verified PowerShell archive"
    Assert-Matches -Context $context -Pattern '(POWERSHELL_EVIDENCE version=\$POWERSHELL_VERSION archive_sha256=\$POWERSHELL_SHA256 architecture=arm64|POWERSHELL_VERSION=\$POWERSHELL_VERSION POWERSHELL_SHA256=\$POWERSHELL_SHA256)' -Issue "ARM64 contexts must emit archive version and SHA evidence"
}

$expectedVersionAssignments = [ordered]@{
    "POWERSHELL_VERSION" = $expectedArm64ArchiveVersion
    "POWERSHELL_SHA256" = $expectedArm64ArchiveSha256
    "POWERSHELL_DEBIAN_PACKAGE_VERSION" = $expectedDebianPackageVersion
    "POWERSHELL_RPM_PACKAGE_NEVRA" = $expectedRpmPackageNevra
    "POWERSHELL_PACKAGE_SEMANTIC_VERSION" = $expectedPackageSemanticVersion
    "POWERSHELL_ALPINE_PACKAGE_VERSION" = $expectedAlpinePackageVersion
    "POWERSHELL_ALPINE_SEMANTIC_VERSION" = $expectedAlpineSemanticVersion
}
foreach ($workflowRecord in $workflowRecords) {
    foreach ($match in [regex]::Matches($workflowRecord.Text, '(?m)^\s+(POWERSHELL_[A-Z0-9_]+):\s+([^\s]+)\s*$')) {
        $name = $match.Groups[1].Value
        $value = $match.Groups[2].Value
        if ($expectedVersionAssignments.Contains($name) -and $value -cne $expectedVersionAssignments[$name]) {
            Add-Violation -Violations $script:violations -Path $workflowRecord.Path -Line (Get-LineNumber -Text $workflowRecord.Text -Index $match.Index) -Issue "PowerShell toolchain version assignment drifted from the audited pin" -Text $match.Value
        }
    }
}

Assert-GlobalCount -Needle "POWERSHELL_DEBIAN_PACKAGE_VERSION: $expectedDebianPackageVersion" -ExpectedCount 2 -Issue "Debian package-manager pin must appear once in the producer and once in the verifier"
Assert-GlobalCount -Needle "POWERSHELL_RPM_PACKAGE_NEVRA: $expectedRpmPackageNevra" -ExpectedCount 4 -Issue "RPM package-manager pin must appear once in the producer and once per RPM verifier"
Assert-GlobalCount -Needle "POWERSHELL_PACKAGE_SEMANTIC_VERSION: $expectedPackageSemanticVersion" -ExpectedCount 5 -Issue "Debian/RPM semantic pwsh pin must appear once in the producer and once per verifier"
Assert-GlobalCount -Needle "POWERSHELL_ALPINE_PACKAGE_VERSION: $expectedAlpinePackageVersion" -ExpectedCount 2 -Issue "Alpine package-manager pin must appear once in the producer and once in the verifier"
Assert-GlobalCount -Needle "POWERSHELL_ALPINE_SEMANTIC_VERSION: $expectedAlpineSemanticVersion" -ExpectedCount 2 -Issue "Alpine semantic pwsh pin must appear once in the producer and once in the verifier"
Assert-GlobalCount -Needle "POWERSHELL_VERSION: $expectedArm64ArchiveVersion" -ExpectedCount 3 -Issue "ARM64 archive version pin must remain exact in the producer and two ARM64 verifiers"
Assert-GlobalCount -Needle "POWERSHELL_SHA256: $expectedArm64ArchiveSha256" -ExpectedCount 3 -Issue "ARM64 archive SHA256 pin must remain exact in the producer and two ARM64 verifiers"
Assert-GlobalCount -Needle 'apt-get install -y --no-install-recommends "powershell=$POWERSHELL_DEBIAN_PACKAGE_VERSION"' -ExpectedCount 2 -Issue "Only the two classified APT contexts may install PowerShell"
Assert-GlobalCount -Needle 'dnf install -y "$POWERSHELL_RPM_PACKAGE_NEVRA"' -ExpectedCount 4 -Issue "Only the four classified RPM contexts may install PowerShell"
Assert-GlobalCount -Needle 'powershell=$POWERSHELL_ALPINE_PACKAGE_VERSION' -ExpectedCount 2 -Issue "Only the two classified APK contexts may install PowerShell"
Assert-GlobalCount -Needle 'PowerShell/PowerShell/releases/download/v$POWERSHELL_VERSION/powershell-$POWERSHELL_VERSION-linux-arm64.tar.gz' -ExpectedCount 3 -Issue "Only the three classified ARM64 archive contexts may download PowerShell"
Assert-GlobalCount -Needle 'echo "$POWERSHELL_SHA256  $powershell_archive" | sha256sum -c -' -ExpectedCount 3 -Issue "Every ARM64 archive download must keep SHA256 verification"
Assert-GlobalCount -Needle 'POWERSHELL_APT_TOOLCHAIN_EVIDENCE' -ExpectedCount 2 -Issue "APT toolchain evidence count must match the classified surface"
Assert-GlobalCount -Needle 'POWERSHELL_RPM_TOOLCHAIN_EVIDENCE' -ExpectedCount 4 -Issue "RPM toolchain evidence count must match the classified surface"
Assert-GlobalCount -Needle 'POWERSHELL_APK_TOOLCHAIN_EVIDENCE' -ExpectedCount 2 -Issue "APK toolchain evidence count must match the classified surface"

$forbiddenLinePatterns = [ordered]@{
    '(?im)apt-get install[^\r\n]*\bpowershell\b(?![=\$])' = "Unversioned or selector-free APT PowerShell install is forbidden"
    '(?im)dnf install -y powershell($|\s)' = "Unversioned RPM PowerShell install is forbidden"
    '(?im)^\s*powershell\s*\\?\s*$' = "Unversioned APK PowerShell install is forbidden"
    '(?im)powershell\s*=\s*(latest|preview|stable|lts|channel|edge|main|community|\*)' = "Mutable PowerShell package selector is forbidden"
    '(?im)POWERSHELL_[A-Z0-9_]+:\s*(latest|preview|stable|lts|channel|edge|main|community|.*[*~^<>])' = "Mutable PowerShell version assignment is forbidden"
    '(?im)PowerShell/PowerShell/releases/download/(latest|preview|stable|lts|channel)' = "Mutable PowerShell archive download selector is forbidden"
}
foreach ($pattern in $forbiddenLinePatterns.Keys) {
    foreach ($workflowRecord in $workflowRecords) {
        foreach ($match in [regex]::Matches($workflowRecord.Text, $pattern)) {
            Add-Violation -Violations $script:violations -Path $workflowRecord.Path -Line (Get-LineNumber -Text $workflowRecord.Text -Index $match.Index) -Issue $forbiddenLinePatterns[$pattern] -Text $match.Value
        }
    }
}

$aggregateText = [System.IO.File]::ReadAllText($aggregatePath)
if ((Get-TextCount -Text $aggregateText -Needle "Test-PowerShellToolchainReproducibility.ps1") -ne 1) {
    Add-Violation -Violations $script:violations -Path (Get-RelativePath -Path $aggregatePath) -Issue "PowerShell toolchain reproducibility guard must be registered exactly once in the aggregate suite"
}

if ($script:violations.Count -gt 0) {
    Write-Host "PowerShell toolchain reproducibility guard failed with $($script:violations.Count) violation(s)."
    $script:violations | Sort-Object Path, Line, Issue | Format-Table Path, Line, Issue, Text -AutoSize -Wrap
    exit 1
}

Write-Host "PowerShell toolchain reproducibility guard passed."
Write-Host "Classified contexts: APT=$($aptContexts.Count), RPM=$($rpmContexts.Count), APK=$($apkContexts.Count), ARM64 archives=$($arm64ArchiveContexts.Count)."
Write-Host "Package-manager pins: apt=$expectedDebianPackageVersion, rpm=$expectedRpmPackageNevra, apk=$expectedAlpinePackageVersion; ARM64 archive=$expectedArm64ArchiveVersion $($expectedArm64ArchiveSha256.ToUpperInvariant())."
