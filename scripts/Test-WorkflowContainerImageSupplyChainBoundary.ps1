param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workflowRoot = Join-Path $repo ".github/workflows"
$aggregatePath = Join-Path $repo "scripts/Test-ProjectInvariants.ps1"

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

function Convert-YamlScalar {
    param([Parameter(Mandatory)][string]$Value)

    $result = $Value.Trim()
    if ($result.Length -ge 2 -and
        (($result.StartsWith('"') -and $result.EndsWith('"')) -or
         ($result.StartsWith("'") -and $result.EndsWith("'")))) {
        return $result.Substring(1, $result.Length - 2)
    }

    return $result
}

function Get-TextCount {
    param(
        [Parameter(Mandatory)][string]$Text,
        [Parameter(Mandatory)][string]$Needle
    )

    return [regex]::Matches($Text, [regex]::Escape($Needle)).Count
}

function Test-ImmutableImageReference {
    param([Parameter(Mandatory)][string]$Reference)

    return $Reference -cmatch '@sha256:[0-9a-f]{64}$' -and
        $Reference -cnotmatch '(?:^|[/:])latest(?:@|$)' -and
        $Reference -notmatch '\$|\{\{'
}

function Get-JobBounds {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][string]$JobName
    )

    $start = -1
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        if ($Lines[$index] -ceq "  $JobName`:") {
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

    return [pscustomobject]@{ Start = $start; End = $end }
}

$violations = [System.Collections.Generic.List[object]]::new()
$expectedWorkflowNames = @(
    "build-managed.yml",
    "build-native.yml",
    "docs.yml",
    "pack.yml",
    "publish-nuget.yml",
    "runtime-input.yml"
)
$workflowFiles = @(
    Get-ChildItem -LiteralPath $workflowRoot -File |
        Where-Object { $_.Extension -in @(".yml", ".yaml") } |
        Sort-Object Name
)
$actualWorkflowNames = @($workflowFiles.Name)
$workflowDifference = @(
    Compare-Object -ReferenceObject $expectedWorkflowNames -DifferenceObject $actualWorkflowNames
)
if ($workflowDifference.Count -gt 0) {
    Add-Violation `
        -Violations $violations `
        -Path ".github/workflows" `
        -Issue "Workflow container image guard requires the exact six-workflow set" `
        -Text (($workflowDifference | ForEach-Object { "$($_.SideIndicator)$($_.InputObject)" }) -join ", ")
}

$runtimePath = Join-Path $workflowRoot "runtime-input.yml"
$packPath = Join-Path $workflowRoot "pack.yml"
if (-not (Test-Path -LiteralPath $runtimePath -PathType Leaf) -or
    -not (Test-Path -LiteralPath $packPath -PathType Leaf)) {
    throw "Required runtime-input.yml or pack.yml workflow was not found."
}

$runtimeRelativePath = Get-RelativePath -Path $runtimePath
$packRelativePath = Get-RelativePath -Path $packPath
$runtimeLines = [System.IO.File]::ReadAllLines($runtimePath)
$packLines = [System.IO.File]::ReadAllLines($packPath)
$runtimeText = [System.IO.File]::ReadAllText($runtimePath)
$packText = [System.IO.File]::ReadAllText($packPath)
$runtimeMatrixPath = Join-Path $repo "packaging/runtime/runtime-package-matrix.json"
$runtimeMatrix = Get-Content -LiteralPath $runtimeMatrixPath -Raw | ConvertFrom-Json
$allWorkflowText = ($workflowFiles | ForEach-Object { [System.IO.File]::ReadAllText($_.FullName) }) -join "`n"

$approvedByRid = [ordered]@{
    "ubuntu.22.04-arm64" = "ubuntu:22.04@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982"
    "debian.12-arm64" = "debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc"
    "debian.12-x64" = "debian:12@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc"
    "fedora.40-x64" = "fedora:40@sha256:3c86d25fef9d2001712bc3d9b091fc40cf04be4767e48f1aa3b785bf58d300ed"
    "rhel.9-x64" = "registry.access.redhat.com/ubi9/ubi:9.8@sha256:50701171b9917ed51048b614924598d45b00bce9a64b73860c057922fc13bec2"
    "rocky.9-x64" = "rockylinux:9@sha256:d7be1c094cc5845ee815d4632fe377514ee6ebcf8efaed6892889657e5ddaaa6"
    "alpine.3.20-x64" = "alpine:3.20@sha256:d9e853e87e55526f6b2917df91a2115c36dd7c696a35be12163d44e6e2a4b6bc"
}

$expectedRows = [ordered]@{}
foreach ($rid in $approvedByRid.Keys) {
    foreach ($profile in @("full", "mini")) {
        $expectedRows["$rid|$profile"] = $approvedByRid[$rid]
    }
}

$producerBounds = Get-JobBounds -Lines $runtimeLines -JobName "produce-container"
if ($null -eq $producerBounds) {
    Add-Violation -Violations $violations -Path $runtimeRelativePath -Issue "Runtime workflow must contain the produce-container job"
}
else {
    $producerRows = [System.Collections.Generic.List[object]]::new()
    foreach ($ridSpec in @($runtimeMatrix.rids | Where-Object { $_.producer.kind -eq 'container' })) {
        foreach ($profile in @($ridSpec.producer.profiles)) {
            $producerRows.Add([pscustomobject]@{
                Rid = [string]$ridSpec.rid
                Profile = [string]$profile
                Image = [string]$ridSpec.producer.containerImage
                RidLine = 0
                ImageLine = 0
            })
        }
    }

    if ($producerRows.Count -ne 14) {
        Add-Violation -Violations $violations -Path $runtimeRelativePath -Line ($producerBounds.Start + 1) -Issue "produce-container must declare exactly 14 executable image rows" -Text "actual=$($producerRows.Count)"
    }

    $seenRows = @{}
    foreach ($row in $producerRows) {
        $key = "$($row.Rid)|$($row.Profile)"
        if ($seenRows.ContainsKey($key)) {
            Add-Violation -Violations $violations -Path $runtimeRelativePath -Line $row.RidLine -Issue "Producer image matrix contains a duplicate RID/profile row" -Text $key
            continue
        }
        $seenRows[$key] = $row

        if ([string]::IsNullOrWhiteSpace($row.Image) -or -not (Test-ImmutableImageReference -Reference $row.Image)) {
            Add-Violation -Violations $violations -Path $runtimeRelativePath -Line $row.ImageLine -Issue "Executable producer container image must use an immutable lowercase SHA256 digest" -Text "$key image=$($row.Image)"
        }
        if (-not $expectedRows.Contains($key)) {
            Add-Violation -Violations $violations -Path $runtimeRelativePath -Line $row.RidLine -Issue "Producer image matrix contains an unclassified RID/profile" -Text $key
            continue
        }
        if (-not $row.Image.Equals($expectedRows[$key], [System.StringComparison]::Ordinal)) {
            Add-Violation -Violations $violations -Path $runtimeRelativePath -Line $row.ImageLine -Issue "Producer container image does not match the audited target digest" -Text "$key actual=$($row.Image) expected=$($expectedRows[$key])"
        }
    }

    foreach ($key in $expectedRows.Keys) {
        if (-not $seenRows.ContainsKey($key)) {
            Add-Violation -Violations $violations -Path $runtimeRelativePath -Line ($producerBounds.Start + 1) -Issue "Producer image matrix is missing an approved RID/profile row" -Text $key
        }
    }
}

$expectedJobImages = [ordered]@{
    "verify-targeted-real-debian" = $approvedByRid["debian.12-x64"]
    "verify-targeted-real-fedora" = $approvedByRid["fedora.40-x64"]
    "verify-targeted-real-rocky" = $approvedByRid["rocky.9-x64"]
    "verify-targeted-real-rhel" = $approvedByRid["rhel.9-x64"]
}
$jobContainers = [System.Collections.Generic.List[object]]::new()
foreach ($workflowFile in $workflowFiles) {
    $relativePath = Get-RelativePath -Path $workflowFile.FullName
    $lines = [System.IO.File]::ReadAllLines($workflowFile.FullName)
    $currentJob = ""
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -match '^  ([A-Za-z0-9_-]+):\s*$') {
            $currentJob = $Matches[1]
        }
        if ($lines[$index] -match '^\s{4}container:\s*(\S+)\s*$') {
            $jobContainers.Add([pscustomobject]@{
                Path = $relativePath
                Line = $index + 1
                Job = $currentJob
                Image = Convert-YamlScalar -Value $Matches[1]
            })
        }
    }
}

if ($jobContainers.Count -ne 4) {
    Add-Violation -Violations $violations -Path ".github/workflows" -Issue "Workflows must contain exactly four job-level container selectors" -Text "actual=$($jobContainers.Count)"
}
$seenJobs = @{}
foreach ($container in $jobContainers) {
    $seenJobs[$container.Job] = $container
    if (-not (Test-ImmutableImageReference -Reference $container.Image)) {
        Add-Violation -Violations $violations -Path $container.Path -Line $container.Line -Issue "Job container image must use an immutable lowercase SHA256 digest" -Text "$($container.Job) image=$($container.Image)"
    }
    if (-not $expectedJobImages.Contains($container.Job)) {
        Add-Violation -Violations $violations -Path $container.Path -Line $container.Line -Issue "Workflow contains an unclassified job-level container selector" -Text "$($container.Job)=$($container.Image)"
        continue
    }
    if (-not $container.Image.Equals($expectedJobImages[$container.Job], [System.StringComparison]::Ordinal)) {
        Add-Violation -Violations $violations -Path $container.Path -Line $container.Line -Issue "Package verifier image must match its producer image and audited target digest" -Text "$($container.Job) actual=$($container.Image) expected=$($expectedJobImages[$container.Job])"
    }
}
foreach ($jobName in $expectedJobImages.Keys) {
    if (-not $seenJobs.ContainsKey($jobName)) {
        Add-Violation -Violations $violations -Path $packRelativePath -Issue "Pack workflow is missing an approved digest-pinned verifier job container" -Text $jobName
    }
}

$expectedHostImages = [ordered]@{
    "UBUNTU_2204_ARM64_IMAGE" = $approvedByRid["ubuntu.22.04-arm64"]
    "DEBIAN_12_ARM64_IMAGE" = $approvedByRid["debian.12-arm64"]
    "ALPINE_3_20_IMAGE" = $approvedByRid["alpine.3.20-x64"]
}
$hostImages = [System.Collections.Generic.List[object]]::new()
for ($index = 0; $index -lt $packLines.Count; $index++) {
    if ($packLines[$index] -match '^\s+(?<name>[A-Z][A-Z0-9_]*_IMAGE):\s*(?<image>\S+)\s*$') {
        $name = $Matches["name"]
        $image = Convert-YamlScalar -Value $Matches["image"]
        if ($expectedHostImages.Contains($name) -or $image -match '@sha256:') {
            $hostImages.Add([pscustomobject]@{ Name = $name; Image = $image; Line = $index + 1 })
        }
    }
}
if ($hostImages.Count -ne 3) {
    Add-Violation -Violations $violations -Path $packRelativePath -Issue "Pack workflow must contain exactly three classified host-orchestrated image literals" -Text "actual=$($hostImages.Count)"
}
$seenHostImages = @{}
foreach ($hostImage in $hostImages) {
    $seenHostImages[$hostImage.Name] = $hostImage
    if (-not $expectedHostImages.Contains($hostImage.Name)) {
        Add-Violation -Violations $violations -Path $packRelativePath -Line $hostImage.Line -Issue "Pack workflow contains an unclassified literal image environment value" -Text "$($hostImage.Name)=$($hostImage.Image)"
        continue
    }
    if (-not $hostImage.Image.Equals($expectedHostImages[$hostImage.Name], [System.StringComparison]::Ordinal) -or
        -not (Test-ImmutableImageReference -Reference $hostImage.Image)) {
        Add-Violation -Violations $violations -Path $packRelativePath -Line $hostImage.Line -Issue "Host-orchestrated verifier image must match its audited immutable digest" -Text "$($hostImage.Name) actual=$($hostImage.Image) expected=$($expectedHostImages[$hostImage.Name])"
    }
}
foreach ($name in $expectedHostImages.Keys) {
    if (-not $seenHostImages.ContainsKey($name)) {
        Add-Violation -Violations $violations -Path $packRelativePath -Issue "Pack workflow is missing an approved host-orchestrated image literal" -Text $name
    }
}

$dynamicSourceNeedle = 'CONTAINER_IMAGE: ${{ matrix.container_image }}'
if ((Get-TextCount -Text $runtimeText -Needle $dynamicSourceNeedle) -ne 1) {
    Add-Violation -Violations $violations -Path $runtimeRelativePath -Issue "Container producer must bind CONTAINER_IMAGE exactly once to its audited matrix row" -Text $dynamicSourceNeedle
}

$pullLines = [System.Collections.Generic.List[object]]::new()
$runLines = [System.Collections.Generic.List[object]]::new()
foreach ($workflowFile in $workflowFiles) {
    $relativePath = Get-RelativePath -Path $workflowFile.FullName
    $lines = [System.IO.File]::ReadAllLines($workflowFile.FullName)
    for ($index = 0; $index -lt $lines.Count; $index++) {
        if ($lines[$index] -match '^\s*docker pull\b') {
            $pullLines.Add([pscustomobject]@{ Path = $relativePath; Line = $index + 1; Text = $lines[$index].Trim() })
        }
        if ($lines[$index] -match '^\s*docker run\b') {
            $runLines.Add([pscustomobject]@{ Path = $relativePath; Line = $index + 1; Text = $lines[$index].Trim() })
        }
        if ($lines[$index] -match '^\s*(?:docker|podman) (?:create|build|buildx)\b.*(?:^|\s)[a-z0-9./_-]+:[A-Za-z0-9._-]+(?:\s|$)') {
            Add-Violation -Violations $violations -Path $relativePath -Line ($index + 1) -Issue "Unclassified executable container image command is not allowed" -Text $lines[$index]
        }
    }
}

if ($pullLines.Count -ne 9) {
    Add-Violation -Violations $violations -Path ".github/workflows" -Issue "Workflow image boundary requires exactly nine classified docker pull commands" -Text "actual=$($pullLines.Count)"
}
$allowedPullLines = @(
    'docker pull "$CONTAINER_IMAGE"',
    'docker pull "$UBUNTU_2204_ARM64_IMAGE"',
    'docker pull "$DEBIAN_12_ARM64_IMAGE"',
    'docker pull "$ALPINE_3_20_IMAGE"'
)
foreach ($pull in $pullLines) {
    if ($pull.Text -cnotin $allowedPullLines) {
        Add-Violation -Violations $violations -Path $pull.Path -Line $pull.Line -Issue "Docker pull must consume a classified digest-pinned image variable" -Text $pull.Text
    }
}
if (@($pullLines | Where-Object { $_.Text -ceq 'docker pull "$CONTAINER_IMAGE"' }).Count -ne 6) {
    Add-Violation -Violations $violations -Path $runtimeRelativePath -Issue "Container producer must pull its matrix-bound image in exactly six target branches"
}
foreach ($name in $expectedHostImages.Keys) {
    if (@($pullLines | Where-Object { $_.Text -ceq "docker pull `"`$$name`"" }).Count -ne 1) {
        Add-Violation -Violations $violations -Path $packRelativePath -Issue "Host-orchestrated verifier must pull its classified image exactly once" -Text $name
    }
}

if ($runLines.Count -ne 4 -or @($runLines | Where-Object { $_.Text -ceq 'docker run --rm \' }).Count -ne 4) {
    Add-Violation -Violations $violations -Path ".github/workflows" -Issue "Workflow image boundary requires exactly four disposable classified docker run commands" -Text "actual=$($runLines.Count)"
}
$runImageArguments = @(
    '"$CONTAINER_IMAGE" \',
    '"$UBUNTU_2204_ARM64_IMAGE" \',
    '"$DEBIAN_12_ARM64_IMAGE" \',
    '"$ALPINE_3_20_IMAGE" \'
)
foreach ($argument in $runImageArguments) {
    $count = @($runtimeLines + $packLines | Where-Object { $_.Trim() -ceq $argument }).Count
    if ($count -ne 1) {
        Add-Violation -Violations $violations -Path ".github/workflows" -Issue "Each docker run must consume its classified digest-pinned image variable exactly once" -Text "$argument count=$count"
    }
}

$x64Provenance = [ordered]@{
    "debian.12-x64" = [pscustomobject]@{ Image = $approvedByRid["debian.12-x64"]; RepoDigest = "debian@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc" }
    "fedora.40-x64" = [pscustomobject]@{ Image = $approvedByRid["fedora.40-x64"]; RepoDigest = "fedora@sha256:3c86d25fef9d2001712bc3d9b091fc40cf04be4767e48f1aa3b785bf58d300ed" }
    "rocky.9-x64" = [pscustomobject]@{ Image = $approvedByRid["rocky.9-x64"]; RepoDigest = "rockylinux@sha256:d7be1c094cc5845ee815d4632fe377514ee6ebcf8efaed6892889657e5ddaaa6" }
    "rhel.9-x64" = [pscustomobject]@{ Image = $approvedByRid["rhel.9-x64"]; RepoDigest = "registry.access.redhat.com/ubi9/ubi@sha256:50701171b9917ed51048b614924598d45b00bce9a64b73860c057922fc13bec2" }
}
foreach ($rid in $x64Provenance.Keys) {
    $expected = $x64Provenance[$rid]
    $condition = "if [ `"`$PRODUCER_RID`" = `"$rid`" ]; then"
    $start = $runtimeText.IndexOf($condition, [System.StringComparison]::Ordinal)
    if ($start -lt 0) {
        Add-Violation -Violations $violations -Path $runtimeRelativePath -Issue "Producer workflow is missing the target-specific image audit branch" -Text $rid
    }
    else {
        $next = $runtimeText.IndexOf('if [ "$PRODUCER_RID" = "', $start + $condition.Length, [System.StringComparison]::Ordinal)
        if ($next -lt 0) {
            $next = $runtimeText.IndexOf("docker run --rm", $start, [System.StringComparison]::Ordinal)
        }
        $branch = $runtimeText.Substring($start, $next - $start)
        $digestNeedle = "grep -Fq '$($expected.RepoDigest)'"
        if ((Get-TextCount -Text $branch -Needle $digestNeedle) -ne 1) {
            Add-Violation -Violations $violations -Path $runtimeRelativePath -Issue "Producer target branch must verify its exact canonical RepoDigest" -Text "$rid expected=$($expected.RepoDigest)"
        }
        if ((Get-TextCount -Text $branch -Needle 'docker pull "$CONTAINER_IMAGE"') -ne 1) {
            Add-Violation -Violations $violations -Path $runtimeRelativePath -Issue "Producer target branch must pull exactly its matrix-bound image" -Text $rid
        }
    }

    $imageNeedle = "`$provenance.ContainerImage -ne '$($expected.Image)'"
    $repoNeedle = "`$provenance.ContainerImageDigest -ne '$($expected.RepoDigest)'"
    if ((Get-TextCount -Text $packText -Needle $imageNeedle) -ne 1 -or
        (Get-TextCount -Text $packText -Needle $repoNeedle) -ne 1) {
        Add-Violation -Violations $violations -Path $packRelativePath -Issue "Pack provenance must require the exact pinned image and canonical RepoDigest" -Text $rid
    }
    else {
        $imageIndex = $packText.IndexOf($imageNeedle, [System.StringComparison]::Ordinal)
        $repoIndex = $packText.IndexOf($repoNeedle, [System.StringComparison]::Ordinal)
        if ($repoIndex -lt $imageIndex -or ($repoIndex - $imageIndex) -gt 500) {
            Add-Violation -Violations $violations -Path $packRelativePath -Issue "Pack image and RepoDigest assertions must remain adjacent in the owning provenance check" -Text $rid
        }
    }
}

for ($index = 0; $index -lt $packLines.Count; $index++) {
    if ($packLines[$index] -match 'ContainerImageDigest\s+-notmatch') {
        Add-Violation -Violations $violations -Path $packRelativePath -Line ($index + 1) -Issue "Container image provenance must not accept a repository-prefix-only digest regex" -Text $packLines[$index]
    }
}
for ($index = 0; $index -lt $runtimeLines.Count; $index++) {
    if ($runtimeLines[$index] -match "grep\s+-Fq\s+'[^']+@sha256:'") {
        Add-Violation -Violations $violations -Path $runtimeRelativePath -Line ($index + 1) -Issue "Producer image audit must not accept a repository-prefix-only RepoDigest" -Text $runtimeLines[$index]
    }
}

$expectedRuntimeDigestChecks = [ordered]@{
    "ubuntu@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982" = 1
    "debian@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc" = 2
    "fedora@sha256:3c86d25fef9d2001712bc3d9b091fc40cf04be4767e48f1aa3b785bf58d300ed" = 1
    "rockylinux@sha256:d7be1c094cc5845ee815d4632fe377514ee6ebcf8efaed6892889657e5ddaaa6" = 1
    "registry.access.redhat.com/ubi9/ubi@sha256:50701171b9917ed51048b614924598d45b00bce9a64b73860c057922fc13bec2" = 1
    "alpine@sha256:d9e853e87e55526f6b2917df91a2115c36dd7c696a35be12163d44e6e2a4b6bc" = 1
}
foreach ($repoDigest in $expectedRuntimeDigestChecks.Keys) {
    $needle = "grep -Fq '$repoDigest'"
    $count = Get-TextCount -Text $runtimeText -Needle $needle
    if ($count -ne $expectedRuntimeDigestChecks[$repoDigest]) {
        Add-Violation -Violations $violations -Path $runtimeRelativePath -Issue "Producer workflow must retain the exact classified RepoDigest check count" -Text "$repoDigest actual=$count expected=$($expectedRuntimeDigestChecks[$repoDigest])"
    }
}
foreach ($repoDigest in @(
        "ubuntu@sha256:0e0a0fc6d18feda9db1590da249ac93e8d5abfea8f4c3c0c849ce512b5ef8982",
        "debian@sha256:9344f8b8992482f80cba753f323adeaf17690076c095ccff6cc9536be98185dc",
        "alpine@sha256:d9e853e87e55526f6b2917df91a2115c36dd7c696a35be12163d44e6e2a4b6bc")) {
    if ((Get-TextCount -Text $packText -Needle "grep -Fq '$repoDigest'") -ne 1) {
        Add-Violation -Violations $violations -Path $packRelativePath -Issue "Host-orchestrated verifier must retain its exact RepoDigest check" -Text $repoDigest
    }
}

$aggregateText = [System.IO.File]::ReadAllText($aggregatePath)
foreach ($scriptName in @(
        "Test-WorkflowContainerImageSupplyChainBoundary.ps1",
        "Test-GitHubActionSupplyChainBoundary.ps1",
        "Test-DotNetInstallerSupplyChainBoundary.ps1")) {
    $count = Get-TextCount -Text $aggregateText -Needle $scriptName
    if ($count -ne 1) {
        Add-Violation -Violations $violations -Path (Get-RelativePath -Path $aggregatePath) -Issue "Supply-chain guard must be registered exactly once in the aggregate suite" -Text "$scriptName count=$count"
    }
}

if ($violations.Count -gt 0) {
    Write-Host "Workflow container image supply-chain boundary failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Line, Issue | Format-Table Path, Line, Issue, Text -AutoSize -Wrap
    exit 1
}

Write-Host "Workflow container image supply-chain boundary passed."
Write-Host "Workflow files: $($workflowFiles.Count); producer image rows: 14; job containers: $($jobContainers.Count); host-orchestrated images: $($hostImages.Count); docker pulls: $($pullLines.Count); docker runs: $($runLines.Count)."
Write-Host "All executable workflow container selections are bound to audited immutable SHA256 digests and exact target provenance."
