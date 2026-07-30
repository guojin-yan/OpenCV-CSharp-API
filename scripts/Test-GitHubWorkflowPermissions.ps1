param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workflowRoot = Join-Path $repo ".github/workflows"
if (-not (Test-Path -LiteralPath $workflowRoot -PathType Container)) {
    throw "GitHub workflow directory was not found: $workflowRoot"
}

function New-PermissionMap {
    param([hashtable]$Values = @{})

    $map = [ordered]@{}
    foreach ($key in $Values.Keys | Sort-Object) {
        $map[$key] = $Values[$key]
    }
    return $map
}

$expectations = [ordered]@{
    ".github/workflows/build-managed.yml" = [pscustomobject]@{
        WorkflowPermissions = New-PermissionMap @{ contents = "read" }
        JobPermissions = [ordered]@{}
        RequiredJobMarkers = [ordered]@{}
    }
    ".github/workflows/build-native.yml" = [pscustomobject]@{
        WorkflowPermissions = New-PermissionMap @{ contents = "read" }
        JobPermissions = [ordered]@{}
        RequiredJobMarkers = [ordered]@{}
    }
    ".github/workflows/docs.yml" = [pscustomobject]@{
        WorkflowPermissions = New-PermissionMap @{ contents = "read" }
        JobPermissions = [ordered]@{
            "deploy-pages" = New-PermissionMap @{
                "id-token" = "write"
                pages = "write"
            }
        }
        RequiredJobMarkers = [ordered]@{
            "deploy-pages" = @(
                "actions/deploy-pages@",
                "environment:"
            )
        }
    }
    ".github/workflows/pack.yml" = [pscustomobject]@{
        WorkflowPermissions = New-PermissionMap @{ contents = "read" }
        JobPermissions = [ordered]@{
            "pack-managed" = New-PermissionMap @{
                contents = "read"
                packages = "write"
            }
            "pack-runtime" = New-PermissionMap @{
                actions = "read"
                contents = "read"
                packages = "write"
            }
        }
        RequiredJobMarkers = [ordered]@{
            "pack-managed" = @(
                "dotnet nuget push",
                'secrets.GITHUB_TOKEN'
            )
            "pack-runtime" = @(
                "dotnet nuget push",
                'secrets.GITHUB_TOKEN',
                "run-id:",
                "github-token:"
            )
        }
    }
    ".github/workflows/runtime-input.yml" = [pscustomobject]@{
        WorkflowPermissions = New-PermissionMap @{ contents = "read" }
        JobPermissions = [ordered]@{}
        RequiredJobMarkers = [ordered]@{}
    }
}

function Get-RelativePath {
    param([Parameter(Mandatory)][string]$Path)

    return ([System.IO.Path]::GetRelativePath($repo, $Path)) -replace "\\", "/"
}

function Get-IndentLength {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Line)

    if ($Line.Contains("`t")) {
        return -1
    }
    return $Line.Length - $Line.TrimStart(" ").Length
}

function Add-Violation {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations,
        [Parameter(Mandatory)][string]$Path,
        [int]$Line = 0,
        [Parameter(Mandatory)][string]$Context,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    [void]$Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
        Context = $Context
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Read-PermissionBlock {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string[]]$Lines,
        [Parameter(Mandatory)][int]$HeaderIndex,
        [Parameter(Mandatory)][int]$HeaderIndent,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Context,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations
    )

    $permissions = [ordered]@{}
    if ($Lines[$HeaderIndex] -ne ((" " * $HeaderIndent) + "permissions:")) {
        Add-Violation `
            -Violations $Violations `
            -Path $Path `
            -Line ($HeaderIndex + 1) `
            -Context $Context `
            -Issue "Permissions declaration must use an inspectable mapping block" `
            -Text $Lines[$HeaderIndex]
        return $permissions
    }

    $entryIndent = $HeaderIndent + 2
    for ($index = $HeaderIndex + 1; $index -lt $Lines.Count; $index++) {
        if ([string]::IsNullOrWhiteSpace($Lines[$index])) {
            continue
        }

        $indent = Get-IndentLength -Line $Lines[$index]
        if ($indent -lt 0) {
            Add-Violation `
                -Violations $Violations `
                -Path $Path `
                -Line ($index + 1) `
                -Context $Context `
                -Issue "Workflow YAML indentation must not contain tabs"
            break
        }
        if ($indent -le $HeaderIndent) {
            break
        }

        $entryMatch = [regex]::Match(
            $Lines[$index],
            "^ {$entryIndent}(?<scope>[a-z-]+):\s*(?<access>read|write|none)\s*$")
        if (-not $entryMatch.Success) {
            Add-Violation `
                -Violations $Violations `
                -Path $Path `
                -Line ($index + 1) `
                -Context $Context `
                -Issue "Permission entry must be a direct scope with read, write, or none access" `
                -Text $Lines[$index]
            continue
        }

        $scope = $entryMatch.Groups["scope"].Value
        if ($permissions.Contains($scope)) {
            Add-Violation `
                -Violations $Violations `
                -Path $Path `
                -Line ($index + 1) `
                -Context $Context `
                -Issue "Permission scope must not be declared more than once" `
                -Text $scope
            continue
        }
        $permissions[$scope] = $entryMatch.Groups["access"].Value
    }

    if ($permissions.Count -eq 0) {
        Add-Violation `
            -Violations $Violations `
            -Path $Path `
            -Line ($HeaderIndex + 1) `
            -Context $Context `
            -Issue "Permissions mapping must contain at least one explicit scope"
    }
    return $permissions
}

function Assert-PermissionMap {
    param(
        [Parameter(Mandatory)][System.Collections.IDictionary]$Expected,
        [Parameter(Mandatory)][System.Collections.IDictionary]$Actual,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Context,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$Violations
    )

    foreach ($scope in $Expected.Keys) {
        if (-not $Actual.Contains($scope)) {
            Add-Violation `
                -Violations $Violations `
                -Path $Path `
                -Context $Context `
                -Issue "Required permission scope is missing" `
                -Text "$scope=$($Expected[$scope])"
        }
        elseif ($Actual[$scope] -ne $Expected[$scope]) {
            Add-Violation `
                -Violations $Violations `
                -Path $Path `
                -Context $Context `
                -Issue "Permission access does not match the least-privilege contract" `
                -Text "$scope actual=$($Actual[$scope]) expected=$($Expected[$scope])"
        }
    }

    foreach ($scope in $Actual.Keys) {
        if (-not $Expected.Contains($scope)) {
            Add-Violation `
                -Violations $Violations `
                -Path $Path `
                -Context $Context `
                -Issue "Unexpected permission scope widens the least-privilege contract" `
                -Text "$scope=$($Actual[$scope])"
        }
    }
}

$violations = [System.Collections.Generic.List[object]]::new()
$workflowFiles = @(
    Get-ChildItem -LiteralPath $workflowRoot -File |
        Where-Object { $_.Extension -in @(".yml", ".yaml") } |
        Sort-Object FullName
)

$actualWorkflowPaths = @($workflowFiles | ForEach-Object { Get-RelativePath -Path $_.FullName })
foreach ($expectedPath in $expectations.Keys) {
    if ($expectedPath -notin $actualWorkflowPaths) {
        Add-Violation `
            -Violations $violations `
            -Path $expectedPath `
            -Context "workflow" `
            -Issue "Expected workflow permission boundary file is missing"
    }
}
foreach ($actualPath in $actualWorkflowPaths) {
    if (-not $expectations.Contains($actualPath)) {
        Add-Violation `
            -Violations $violations `
            -Path $actualPath `
            -Context "workflow" `
            -Issue "Workflow has no explicit permission boundary expectation"
    }
}

$jobCount = 0
$jobPermissionBlockCount = 0
foreach ($workflowFile in $workflowFiles) {
    $relativePath = Get-RelativePath -Path $workflowFile.FullName
    if (-not $expectations.Contains($relativePath)) {
        continue
    }

    $expected = $expectations[$relativePath]
    $lines = [System.IO.File]::ReadAllLines($workflowFile.FullName)
    $topPermissionIndexes = @(
        for ($index = 0; $index -lt $lines.Count; $index++) {
            if ($lines[$index] -match '^permissions\s*:') {
                $index
            }
        }
    )
    if ($topPermissionIndexes.Count -ne 1) {
        Add-Violation `
            -Violations $violations `
            -Path $relativePath `
            -Context "workflow" `
            -Issue "Workflow must contain exactly one explicit top-level permissions mapping" `
            -Text "declarations=$($topPermissionIndexes.Count)"
        $actualWorkflowPermissions = [ordered]@{}
    }
    else {
        $actualWorkflowPermissions = Read-PermissionBlock `
            -Lines $lines `
            -HeaderIndex $topPermissionIndexes[0] `
            -HeaderIndent 0 `
            -Path $relativePath `
            -Context "workflow" `
            -Violations $violations
    }
    Assert-PermissionMap `
        -Expected $expected.WorkflowPermissions `
        -Actual $actualWorkflowPermissions `
        -Path $relativePath `
        -Context "workflow" `
        -Violations $violations

    $jobsIndexes = @(
        for ($index = 0; $index -lt $lines.Count; $index++) {
            if ($lines[$index] -match '^jobs:\s*$') {
                $index
            }
        }
    )
    if ($jobsIndexes.Count -ne 1) {
        Add-Violation `
            -Violations $violations `
            -Path $relativePath `
            -Context "workflow" `
            -Issue "Workflow must contain exactly one jobs mapping" `
            -Text "declarations=$($jobsIndexes.Count)"
        continue
    }

    $jobStarts = [System.Collections.Generic.List[object]]::new()
    for ($index = $jobsIndexes[0] + 1; $index -lt $lines.Count; $index++) {
        $jobMatch = [regex]::Match($lines[$index], '^  (?<job>[A-Za-z0-9_-]+):\s*$')
        if ($jobMatch.Success) {
            [void]$jobStarts.Add([pscustomobject]@{
                Name = $jobMatch.Groups["job"].Value
                Index = $index
            })
        }
    }
    $jobCount += $jobStarts.Count

    $actualJobPermissions = [ordered]@{}
    $jobTexts = @{}
    for ($jobIndex = 0; $jobIndex -lt $jobStarts.Count; $jobIndex++) {
        $job = $jobStarts[$jobIndex]
        $jobEnd = if ($jobIndex + 1 -lt $jobStarts.Count) {
            $jobStarts[$jobIndex + 1].Index
        }
        else {
            $lines.Count
        }
        $jobTexts[$job.Name] = $lines[$job.Index..($jobEnd - 1)] -join [System.Environment]::NewLine

        $permissionIndexes = @(
            for ($index = $job.Index + 1; $index -lt $jobEnd; $index++) {
                if ($lines[$index] -match '^    permissions\s*:') {
                    $index
                }
            }
        )
        if ($permissionIndexes.Count -gt 1) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line ($permissionIndexes[1] + 1) `
                -Context "job:$($job.Name)" `
                -Issue "Job must not contain multiple permissions mappings" `
                -Text "declarations=$($permissionIndexes.Count)"
        }
        if ($permissionIndexes.Count -gt 0) {
            $jobPermissionBlockCount++
            $actualJobPermissions[$job.Name] = Read-PermissionBlock `
                -Lines $lines `
                -HeaderIndex $permissionIndexes[0] `
                -HeaderIndent 4 `
                -Path $relativePath `
                -Context "job:$($job.Name)" `
                -Violations $violations
        }
    }

    foreach ($jobName in $expected.JobPermissions.Keys) {
        if (-not $jobTexts.ContainsKey($jobName)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Context "job:$jobName" `
                -Issue "Expected privileged job is missing"
            continue
        }
        if (-not $actualJobPermissions.Contains($jobName)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Context "job:$jobName" `
                -Issue "Required job-level permissions mapping is missing"
            continue
        }
        Assert-PermissionMap `
            -Expected $expected.JobPermissions[$jobName] `
            -Actual $actualJobPermissions[$jobName] `
            -Path $relativePath `
            -Context "job:$jobName" `
            -Violations $violations
    }

    foreach ($jobName in $actualJobPermissions.Keys) {
        if (-not $expected.JobPermissions.Contains($jobName)) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Context "job:$jobName" `
                -Issue "Unexpected job-level permissions mapping widens or obscures the contract"
        }
    }

    foreach ($jobName in $expected.RequiredJobMarkers.Keys) {
        if (-not $jobTexts.ContainsKey($jobName)) {
            continue
        }
        foreach ($marker in $expected.RequiredJobMarkers[$jobName]) {
            if ($jobTexts[$jobName].IndexOf($marker, [System.StringComparison]::Ordinal) -lt 0) {
                Add-Violation `
                    -Violations $violations `
                    -Path $relativePath `
                    -Context "job:$jobName" `
                    -Issue "Privileged job no longer contains the operation that justifies its permission" `
                    -Text $marker
            }
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host "GitHub workflow permissions guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Line, Context, Issue | Format-Table Path, Line, Context, Issue, Text -AutoSize
    exit 1
}

Write-Host "GitHub workflow permissions guard passed."
Write-Host "Workflow files checked: $($workflowFiles.Count); jobs checked: $jobCount; job-level permission blocks: $jobPermissionBlockCount."
Write-Host "Workflow-level permissions are read-only; write scopes are limited to deploy-pages, pack-managed, and pack-runtime."
