param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$BashPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$workflowRoot = Join-Path $repo ".github/workflows"
if (-not (Test-Path -LiteralPath $workflowRoot -PathType Container)) {
    throw "GitHub workflow directory was not found: $workflowRoot"
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
        [Parameter(Mandatory)][string]$Step,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    $Violations.Add([pscustomobject]@{
        Path = $Path
        Line = $Line
        Step = $Step
        Issue = $Issue
        Text = $Text.Trim()
    })
}

function Convert-GitHubExpressionsForParsing {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Script)

    return [regex]::Replace(
        $Script,
        '\$\{\{(?s:.*?)\}\}',
        'GHA_EXPRESSION')
}

function Resolve-BashExecutable {
    param([string]$RequestedPath)

    $candidates = [System.Collections.Generic.List[string]]::new()
    if (-not [string]::IsNullOrWhiteSpace($RequestedPath)) {
        [void]$candidates.Add($RequestedPath)
    }

    $command = Get-Command bash -ErrorAction SilentlyContinue
    if ($null -ne $command) {
        [void]$candidates.Add($command.Source)
    }

    foreach ($programFiles in @($env:ProgramFiles, ${env:ProgramFiles(x86)})) {
        if (-not [string]::IsNullOrWhiteSpace($programFiles)) {
            [void]$candidates.Add((Join-Path $programFiles "Git/bin/bash.exe"))
            [void]$candidates.Add((Join-Path $programFiles "Git/usr/bin/bash.exe"))
        }
    }

    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($candidate in $candidates) {
        if ([string]::IsNullOrWhiteSpace($candidate) -or -not $seen.Add($candidate)) {
            continue
        }

        $resolved = $candidate
        $candidateCommand = Get-Command $candidate -ErrorAction SilentlyContinue
        if ($null -ne $candidateCommand) {
            $resolved = $candidateCommand.Source
        }
        if (-not (Test-Path -LiteralPath $resolved -PathType Leaf)) {
            continue
        }

        try {
            & $resolved -n -c "true" 2>$null | Out-Null
            if ($LASTEXITCODE -eq 0) {
                return $resolved
            }
        }
        catch {
            continue
        }
    }

    throw "A usable bash executable was not found. Install Git for Windows Bash or a native Bash runtime."
}

function Invoke-BashSyntaxCheck {
    param(
        [Parameter(Mandatory)][string]$Executable,
        [Parameter(Mandatory)][AllowEmptyString()][string]$Script
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $Executable
    [void]$startInfo.ArgumentList.Add("-n")
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardInput = $true
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    try {
        [void]$process.Start()
        $process.StandardInput.Write($Script)
        $process.StandardInput.Close()
        $standardOutput = $process.StandardOutput.ReadToEnd()
        $standardError = $process.StandardError.ReadToEnd()
        $process.WaitForExit()
        return [pscustomobject]@{
            ExitCode = $process.ExitCode
            Output = @($standardOutput, $standardError) -join " "
        }
    }
    finally {
        $process.Dispose()
    }
}

$bashExecutable = Resolve-BashExecutable -RequestedPath $BashPath
$violations = [System.Collections.Generic.List[object]]::new()
$workflowFiles = @(
    Get-ChildItem -LiteralPath $workflowRoot -File |
        Where-Object { $_.Extension -in @(".yml", ".yaml") } |
        Sort-Object FullName
)
if ($workflowFiles.Count -eq 0) {
    throw "No GitHub workflow YAML files were found under $workflowRoot"
}

$shellDeclarationCount = 0
$parsedScriptCount = 0
foreach ($workflowFile in $workflowFiles) {
    $relativePath = Get-RelativePath -Path $workflowFile.FullName
    $lines = [System.IO.File]::ReadAllLines($workflowFile.FullName)

    for ($shellIndex = 0; $shellIndex -lt $lines.Count; $shellIndex++) {
        $shellMatch = [regex]::Match($lines[$shellIndex], '^(?<indent> *)shell:\s*bash\s*$')
        if (-not $shellMatch.Success) {
            continue
        }

        $shellDeclarationCount++
        $propertyIndent = $shellMatch.Groups["indent"].Value.Length
        $stepIndent = $propertyIndent - 2
        $fallbackStepName = "bash step at line $($shellIndex + 1)"
        if ($stepIndent -lt 0) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line ($shellIndex + 1) `
                -Step $fallbackStepName `
                -Issue "Explicit bash shell declaration has no parent step indentation"
            continue
        }

        $stepStart = -1
        for ($index = $shellIndex; $index -ge 0; $index--) {
            if ($lines[$index] -match "^ {$stepIndent}-\s+") {
                $stepStart = $index
                break
            }
        }
        if ($stepStart -lt 0) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line ($shellIndex + 1) `
                -Step $fallbackStepName `
                -Issue "Explicit bash shell declaration could not be associated with a workflow step"
            continue
        }

        $stepEnd = $lines.Count
        for ($index = $stepStart + 1; $index -lt $lines.Count; $index++) {
            if ([string]::IsNullOrWhiteSpace($lines[$index])) {
                continue
            }

            $indent = Get-IndentLength -Line $lines[$index]
            if ($indent -lt 0) {
                Add-Violation `
                    -Violations $violations `
                    -Path $relativePath `
                    -Line ($index + 1) `
                    -Step $fallbackStepName `
                    -Issue "Workflow YAML indentation must not contain tabs"
                $stepEnd = $index
                break
            }
            if ($indent -le $stepIndent) {
                $stepEnd = $index
                break
            }
        }

        $stepName = $fallbackStepName
        $inlineNameMatch = [regex]::Match($lines[$stepStart], "^ {$stepIndent}-\s+name:\s*(?<name>.+?)\s*$")
        if ($inlineNameMatch.Success) {
            $stepName = $inlineNameMatch.Groups["name"].Value.Trim('"', "'")
        }
        $runIndexes = [System.Collections.Generic.List[int]]::new()
        for ($index = $stepStart; $index -lt $stepEnd; $index++) {
            $nameMatch = [regex]::Match($lines[$index], "^ {$propertyIndent}name:\s*(?<name>.+?)\s*$")
            if ($nameMatch.Success) {
                $stepName = $nameMatch.Groups["name"].Value.Trim('"', "'")
            }

            if ($lines[$index] -match "^ {$propertyIndent}run:\s*") {
                $runIndexes.Add($index)
            }
        }

        if ($runIndexes.Count -ne 1) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line ($shellIndex + 1) `
                -Step $stepName `
                -Issue "Explicit bash workflow step must contain exactly one run property" `
                -Text "run properties: $($runIndexes.Count)"
            continue
        }

        $runIndex = $runIndexes[0]
        $runMatch = [regex]::Match($lines[$runIndex], "^ {$propertyIndent}run:\s*(?<value>.*)$")
        $runValue = $runMatch.Groups["value"].Value.Trim()
        $script = ""
        $scriptWorkflowStartLine = $runIndex + 1

        if ($runValue -match '^(?<style>[|>])(?<chomp>[+-]?)$') {
            $contentStart = $runIndex + 1
            $contentEnd = $stepEnd
            for ($index = $contentStart; $index -lt $stepEnd; $index++) {
                if ([string]::IsNullOrWhiteSpace($lines[$index])) {
                    continue
                }

                $indent = Get-IndentLength -Line $lines[$index]
                if ($indent -le $propertyIndent) {
                    $contentEnd = $index
                    break
                }
            }

            $nonBlankContentIndents = @(
                for ($index = $contentStart; $index -lt $contentEnd; $index++) {
                    if (-not [string]::IsNullOrWhiteSpace($lines[$index])) {
                        Get-IndentLength -Line $lines[$index]
                    }
                }
            )
            if ($nonBlankContentIndents.Count -eq 0) {
                Add-Violation `
                    -Violations $violations `
                    -Path $relativePath `
                    -Line ($runIndex + 1) `
                    -Step $stepName `
                    -Issue "Explicit bash block scalar must contain a non-empty script"
                continue
            }

            $contentIndent = ($nonBlankContentIndents | Measure-Object -Minimum).Minimum
            if ($contentIndent -le $propertyIndent) {
                Add-Violation `
                    -Violations $violations `
                    -Path $relativePath `
                    -Line ($runIndex + 1) `
                    -Step $stepName `
                    -Issue "Explicit bash block scalar content must be indented below run"
                continue
            }

            $scriptLines = [System.Collections.Generic.List[string]]::new()
            for ($index = $contentStart; $index -lt $contentEnd; $index++) {
                if ([string]::IsNullOrWhiteSpace($lines[$index])) {
                    $scriptLines.Add("")
                }
                elseif ($lines[$index].Length -ge $contentIndent) {
                    $scriptLines.Add($lines[$index].Substring($contentIndent))
                }
                else {
                    Add-Violation `
                        -Violations $violations `
                        -Path $relativePath `
                        -Line ($index + 1) `
                        -Step $stepName `
                        -Issue "Explicit bash block scalar contains inconsistent indentation"
                }
            }
            $script = $scriptLines -join [System.Environment]::NewLine
            $scriptWorkflowStartLine = $contentStart + 1
        }
        elseif ($runValue.StartsWith("|") -or $runValue.StartsWith(">")) {
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line ($runIndex + 1) `
                -Step $stepName `
                -Issue "Explicit bash run uses an unsupported YAML block scalar header" `
                -Text $runValue
            continue
        }
        else {
            if ([string]::IsNullOrWhiteSpace($runValue)) {
                Add-Violation `
                    -Violations $violations `
                    -Path $relativePath `
                    -Line ($runIndex + 1) `
                    -Step $stepName `
                    -Issue "Explicit bash inline run must not be empty"
                continue
            }
            if (($runValue.StartsWith("'") -and $runValue.EndsWith("'")) -or
                ($runValue.StartsWith('"') -and $runValue.EndsWith('"'))) {
                Add-Violation `
                    -Violations $violations `
                    -Path $relativePath `
                    -Line ($runIndex + 1) `
                    -Step $stepName `
                    -Issue "Quoted YAML inline bash run is not inspected; use a block scalar or plain command"
                continue
            }
            $script = $runValue
        }

        $sanitizedScript = Convert-GitHubExpressionsForParsing -Script $script
        $bashResult = Invoke-BashSyntaxCheck -Executable $bashExecutable -Script $sanitizedScript
        $bashOutput = $bashResult.Output
        $bashExitCode = $bashResult.ExitCode
        if ($bashExitCode -ne 0) {
            $diagnostic = $bashOutput.Trim()
            if ([string]::IsNullOrWhiteSpace($diagnostic)) {
                $diagnostic = "bash -n exited with code $bashExitCode"
            }
            Add-Violation `
                -Violations $violations `
                -Path $relativePath `
                -Line $scriptWorkflowStartLine `
                -Step $stepName `
                -Issue "Embedded bash script failed bash -n syntax parsing" `
                -Text $diagnostic
        }
        $parsedScriptCount++
    }
}

if ($shellDeclarationCount -ne $parsedScriptCount -or $violations.Count -gt 0) {
    if ($shellDeclarationCount -ne $parsedScriptCount) {
        Add-Violation `
            -Violations $violations `
            -Path ".github/workflows" `
            -Step "aggregate" `
            -Issue "Every explicit bash shell declaration must be parsed exactly once" `
            -Text "declarations=$shellDeclarationCount parsed=$parsedScriptCount"
    }

    Write-Host "Workflow Bash syntax guard failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Line, Step, Issue | Format-Table Path, Line, Step, Issue, Text -AutoSize
    exit 1
}

Write-Host "Workflow Bash syntax guard passed."
Write-Host "Bash executable: $bashExecutable"
Write-Host "Workflow files checked: $($workflowFiles.Count); explicit bash scripts parsed: $parsedScriptCount."
