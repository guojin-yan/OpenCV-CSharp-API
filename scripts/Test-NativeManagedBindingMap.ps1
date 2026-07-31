param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path,
    [string]$DotNetPath = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$mappingPath = Join-Path $repo "compatibility/native-managed-binding-map.txt"
$summaryPath = Join-Path $repo "compatibility/native-managed-binding-summary.json"
$manifestPath = Join-Path $repo "src/OpenCvSharp.Native/generated/legacy_abi_manifest.txt"
$violations = [System.Collections.Generic.List[object]]::new()

function Add-Violation {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    $List.Add([pscustomobject]@{ Path = $Path; Issue = $Issue; Text = $Text.Trim() })
}

function Assert-True {
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][bool]$Condition,
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][string]$Issue,
        [string]$Text = ""
    )

    if (-not $Condition) {
        Add-Violation -List $List -Path $Path -Issue $Issue -Text $Text
    }
}

function Normalize-Text {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Text)

    return (($Text -replace "`r`n", "`n") -replace "`r", "`n").TrimEnd() + "`n"
}

function Get-TextSha256 {
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Text)

    $bytes = [Text.UTF8Encoding]::new($false).GetBytes((Normalize-Text $Text))
    return [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
}

function Get-OrdinalSorted {
    param([Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Values)

    $copy = [string[]]$Values.Clone()
    [Array]::Sort($copy, [StringComparer]::Ordinal)
    return $copy
}

function Test-BindingMapDocument {
    param(
        [Parameter(Mandatory)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory)][object]$Summary,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]]$ManifestEntrypoints,
        [Parameter(Mandatory)][AllowEmptyCollection()][System.Collections.Generic.List[object]]$List,
        [Parameter(Mandatory)][string]$Path
    )

    $normalized = Normalize-Text $Text
    $lines = @($normalized.TrimEnd("`n").Split("`n"))
    $bindingsIndex = [Array]::IndexOf($lines, "[bindings]")
    $managedOnlyIndex = [Array]::IndexOf($lines, "[managed-only]")
    Assert-True -List $List -Condition ($lines.Count -gt 6 -and $lines[0] -eq "# Native-to-managed binding map") -Path $Path -Issue "Binding-map generator header is missing"
    Assert-True -List $List -Condition ($bindingsIndex -eq 4 -and $managedOnlyIndex -gt $bindingsIndex) -Path $Path -Issue "Binding-map sections are missing or reordered"
    if ($bindingsIndex -lt 0 -or $managedOnlyIndex -le $bindingsIndex) { return }

    Assert-True -List $List -Condition ($lines[1] -eq "schema-version=1" -and $lines[2] -eq "primary-prefix=jyppx_ocv_") -Path $Path -Issue "Binding-map identity metadata drifted"
    Assert-True -List $List -Condition ($lines[3] -eq "classification-order=managed-bound,native-infrastructure,compatibility-only,unbound") -Path $Path -Issue "Binding-map classification order drifted"

    $bindingLines = @($lines[($bindingsIndex + 1)..($managedOnlyIndex - 1)] | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    [string[]]$managedOnlyLines = @()
    if ($managedOnlyIndex -lt $lines.Count - 1) {
        $managedOnlyLines = @($lines[($managedOnlyIndex + 1)..($lines.Count - 1)] | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    }

    $entrypoints = [System.Collections.Generic.List[string]]::new()
    $classifications = [System.Collections.Generic.List[string]]::new()
    $methodCount = 0
    $sourceCount = 0
    foreach ($line in $bindingLines) {
        $parts = $line.Split([char]'|')
        if ($parts.Count -ne 4) {
            Add-Violation -List $List -Path $Path -Issue "Binding-map entry is malformed" -Text $line
            continue
        }

        $entrypoint = $parts[0]
        $classification = $parts[1]
        $entrypoints.Add($entrypoint)
        $classifications.Add($classification)
        Assert-True -List $List -Condition ($entrypoint.StartsWith("jyppx_ocv_", [StringComparison]::Ordinal) -and $entrypoint -notmatch '^jyppx_ocv[0-9]+_') -Path $Path -Issue "Binding-map contains a fixed-major or non-neutral entrypoint" -Text $entrypoint
        Assert-True -List $List -Condition ($classification -eq "managed-bound") -Path $Path -Issue "Binding-map contains an undocumented classification" -Text "$entrypoint|$classification"
        Assert-True -List $List -Condition ($parts[2] -ne "-" -and $parts[3] -ne "-") -Path $Path -Issue "Managed-bound entry is missing method or source evidence" -Text $entrypoint

        if ($parts[2] -ne "-") {
            $methods = @($parts[2].Split([char]';'))
            $sortedMethods = @(Get-OrdinalSorted -Values $methods)
            $uniqueMethods = @($methods | Sort-Object -Unique)
            Assert-True -List $List -Condition (($methods -join "`n") -ceq ($sortedMethods -join "`n") -and $methods.Count -eq $uniqueMethods.Count) -Path $Path -Issue "Binding-map method evidence is duplicated or not deterministically ordered" -Text $entrypoint
            $methodCount += $methods.Count
        }

        if ($parts[3] -ne "-") {
            $sources = @($parts[3].Split([char]';'))
            $sortedSources = @(Get-OrdinalSorted -Values $sources)
            $uniqueSources = @($sources | Sort-Object -Unique)
            Assert-True -List $List -Condition (($sources -join "`n") -ceq ($sortedSources -join "`n") -and $sources.Count -eq $uniqueSources.Count) -Path $Path -Issue "Binding-map source evidence is duplicated or not deterministically ordered" -Text $entrypoint
            foreach ($source in $sources) {
                Assert-True -List $List -Condition ($source -match '^src/OpenCvSharp/Internal/Interop/.+\.cs:[1-9][0-9]*$') -Path $Path -Issue "Binding-map source reference is malformed" -Text $source
            }
            $sourceCount += $sources.Count
        }
    }

    $actualEntrypoints = @($entrypoints)
    $sortedEntrypoints = @(Get-OrdinalSorted -Values $actualEntrypoints)
    Assert-True -List $List -Condition ($bindingLines.Count -eq 2603 -and $actualEntrypoints.Count -eq 2603) -Path $Path -Issue "Binding-map native entry count drifted" -Text "actual=$($actualEntrypoints.Count) expected=2603"
    Assert-True -List $List -Condition (($actualEntrypoints -join "`n") -ceq ($sortedEntrypoints -join "`n")) -Path $Path -Issue "Binding-map entries must use ordinal entrypoint ordering"
    $uniqueEntrypoints = @($actualEntrypoints | Sort-Object -Unique)
    Assert-True -List $List -Condition ($actualEntrypoints.Count -eq $uniqueEntrypoints.Count) -Path $Path -Issue "Binding-map contains duplicate entrypoints"
    Assert-True -List $List -Condition (($actualEntrypoints -join "`n") -ceq ($ManifestEntrypoints -join "`n")) -Path $Path -Issue "Binding-map entrypoints drifted from the native ABI manifest"
    Assert-True -List $List -Condition ($managedOnlyLines.Count -eq 0) -Path $Path -Issue "Binding-map contains managed-only entrypoints" -Text ($managedOnlyLines -join ";")

    $classificationCounts = @{}
    foreach ($classification in $classifications) {
        $classificationCounts[$classification] = 1 + [int]($classificationCounts[$classification])
    }
    $mappingHash = Get-TextSha256 $normalized
    Assert-True -List $List -Condition ($Summary.schemaVersion -eq 1 -and $Summary.generator -eq "tools/NativeManagedBindingMap" -and $Summary.assemblyName -eq "JYPPX.OpenCV.CSharp.API" -and $Summary.targetFramework -eq ".NETCoreApp,Version=v10.0") -Path $Path -Issue "Binding-map summary identity drifted"
    Assert-True -List $List -Condition ($Summary.nativeManifestPath -eq "src/OpenCvSharp.Native/generated/legacy_abi_manifest.txt" -and $Summary.managedSourceRoot -eq "src/OpenCvSharp/Internal/Interop" -and $Summary.mappingPath -eq "compatibility/native-managed-binding-map.txt") -Path $Path -Issue "Binding-map summary paths drifted"
    Assert-True -List $List -Condition ($Summary.mappingSha256 -eq $mappingHash) -Path $Path -Issue "Binding-map SHA256 does not match its summary" -Text "actual=$mappingHash summary=$($Summary.mappingSha256)"
    Assert-True -List $List -Condition ([int]$Summary.nativeFunctionCount -eq 2603 -and [int]$Summary.managedEntryPointCount -eq 2603 -and [int]$Summary.managedBoundCount -eq 2603) -Path $Path -Issue "Binding-map summary parity counts drifted"
    Assert-True -List $List -Condition ([int]$Summary.managedImportMethodCount -eq $methodCount -and [int]$Summary.managedSourceDeclarationCount -eq $sourceCount) -Path $Path -Issue "Binding-map summary method/source counts drifted" -Text "methods=$methodCount sources=$sourceCount"
    Assert-True -List $List -Condition ([int]$Summary.nativeInfrastructureCount -eq 0 -and [int]$Summary.compatibilityOnlyCount -eq 0 -and [int]$Summary.unboundCount -eq 0 -and [int]$Summary.managedOnlyCount -eq 0) -Path $Path -Issue "Binding-map summary must retain zero unbound, managed-only, or classified exceptions"
}

function Assert-FixtureRejected {
    param(
        [Parameter(Mandatory)][string]$Name,
        [Parameter(Mandatory)][scriptblock]$Action,
        [Parameter(Mandatory)][string]$ExpectedIssue
    )

    $fixtureViolations = [System.Collections.Generic.List[object]]::new()
    & $Action $fixtureViolations
    if ($fixtureViolations.Count -eq 0) {
        throw "Negative native/managed binding-map fixture was accepted: $Name"
    }
    if (-not @($fixtureViolations | Where-Object { $_.Issue -like "*$ExpectedIssue*" })) {
        throw "Negative binding-map fixture '$Name' failed for the wrong reason: $($fixtureViolations.Issue -join '; ')"
    }
}

foreach ($path in @($mappingPath, $summaryPath, $manifestPath)) {
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Required native/managed binding-map file was not found: $path"
    }
}

$generatorArguments = @{
    RepositoryRoot = $repo
    Check = $true
}
if (-not [string]::IsNullOrWhiteSpace($DotNetPath)) {
    $generatorArguments.DotNetPath = $DotNetPath
}
& (Join-Path $repo "scripts/Generate-NativeManagedBindingMap.ps1") @generatorArguments
if (-not $?) { throw "Native/managed binding-map generated-file freshness check failed." }

$mappingText = [IO.File]::ReadAllText($mappingPath)
$summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
$manifestEntrypoints = @(Get-Content -LiteralPath $manifestPath | Where-Object { $_ -match '^jyppx_ocv_.*\|' } | ForEach-Object { $_.Split([char]'|')[0] })
Test-BindingMapDocument -Text $mappingText -Summary $summary -ManifestEntrypoints $manifestEntrypoints -List $violations -Path "compatibility/native-managed-binding-map.txt"

$mapLines = @((Normalize-Text $mappingText).TrimEnd("`n").Split("`n"))
$bindingsIndex = [Array]::IndexOf($mapLines, "[bindings]")
$managedOnlyIndex = [Array]::IndexOf($mapLines, "[managed-only]")
$multiSourceIndex = -1
for ($index = $bindingsIndex + 1; $index -lt $managedOnlyIndex; $index++) {
    if ($mapLines[$index].Split([char]'|')[3].Contains(';')) {
        $multiSourceIndex = $index
        break
    }
}
if ($multiSourceIndex -lt 0) { throw "Binding-map negative fixtures require a row with multiple source references." }

Assert-FixtureRejected -Name "missing binding" -ExpectedIssue "native entry count" -Action {
    param($list)
    $fixture = [Collections.Generic.List[string]]::new()
    $fixture.AddRange([string[]]$mapLines)
    $fixture.RemoveAt($bindingsIndex + 1)
    Test-BindingMapDocument -Text (($fixture -join "`n") + "`n") -Summary $summary -ManifestEntrypoints $manifestEntrypoints -List $list -Path "fixture/missing-binding.txt"
}
Assert-FixtureRejected -Name "duplicate binding" -ExpectedIssue "duplicate entrypoints" -Action {
    param($list)
    $fixture = [Collections.Generic.List[string]]::new()
    $fixture.AddRange([string[]]$mapLines)
    $fixture.Insert($bindingsIndex + 2, $fixture[$bindingsIndex + 1])
    Test-BindingMapDocument -Text (($fixture -join "`n") + "`n") -Summary $summary -ManifestEntrypoints $manifestEntrypoints -List $list -Path "fixture/duplicate-binding.txt"
}
Assert-FixtureRejected -Name "reordered bindings" -ExpectedIssue "ordinal entrypoint ordering" -Action {
    param($list)
    $fixture = [string[]]$mapLines.Clone()
    $temporary = $fixture[$bindingsIndex + 1]
    $fixture[$bindingsIndex + 1] = $fixture[$bindingsIndex + 2]
    $fixture[$bindingsIndex + 2] = $temporary
    Test-BindingMapDocument -Text (($fixture -join "`n") + "`n") -Summary $summary -ManifestEntrypoints $manifestEntrypoints -List $list -Path "fixture/reordered-bindings.txt"
}
Assert-FixtureRejected -Name "entrypoint drift" -ExpectedIssue "drifted from the native ABI manifest" -Action {
    param($list)
    $fixture = [string[]]$mapLines.Clone()
    $fixture[$bindingsIndex + 1] = $fixture[$bindingsIndex + 1] -replace '^jyppx_ocv_[^|]+', 'jyppx_ocv_aaa_drift_fixture'
    Test-BindingMapDocument -Text (($fixture -join "`n") + "`n") -Summary $summary -ManifestEntrypoints $manifestEntrypoints -List $list -Path "fixture/entrypoint-drift.txt"
}
Assert-FixtureRejected -Name "fixed-major entrypoint" -ExpectedIssue "fixed-major or non-neutral" -Action {
    param($list)
    $fixture = [string[]]$mapLines.Clone()
    $fixture[$bindingsIndex + 1] = $fixture[$bindingsIndex + 1] -replace '^jyppx_ocv_', 'jyppx_ocv6_'
    Test-BindingMapDocument -Text (($fixture -join "`n") + "`n") -Summary $summary -ManifestEntrypoints $manifestEntrypoints -List $list -Path "fixture/fixed-major-entrypoint.txt"
}
Assert-FixtureRejected -Name "stale hash" -ExpectedIssue "SHA256" -Action {
    param($list)
    $summaryFixture = $summary | ConvertTo-Json -Depth 10 | ConvertFrom-Json
    $summaryFixture.mappingSha256 = "0" * 64
    Test-BindingMapDocument -Text $mappingText -Summary $summaryFixture -ManifestEntrypoints $manifestEntrypoints -List $list -Path "fixture/stale-hash.txt"
}
Assert-FixtureRejected -Name "undocumented classification" -ExpectedIssue "undocumented classification" -Action {
    param($list)
    $fixture = [string[]]$mapLines.Clone()
    $fixture[$bindingsIndex + 1] = $fixture[$bindingsIndex + 1] -replace '\|managed-bound\|', '|native-infrastructure|'
    Test-BindingMapDocument -Text (($fixture -join "`n") + "`n") -Summary $summary -ManifestEntrypoints $manifestEntrypoints -List $list -Path "fixture/undocumented-classification.txt"
}
Assert-FixtureRejected -Name "nondeterministic source ordering" -ExpectedIssue "source evidence" -Action {
    param($list)
    $fixture = [string[]]$mapLines.Clone()
    $parts = $fixture[$multiSourceIndex].Split([char]'|')
    $sources = $parts[3].Split([char]';')
    $temporary = $sources[0]
    $sources[0] = $sources[1]
    $sources[1] = $temporary
    $parts[3] = $sources -join ';'
    $fixture[$multiSourceIndex] = $parts -join '|'
    Test-BindingMapDocument -Text (($fixture -join "`n") + "`n") -Summary $summary -ManifestEntrypoints $manifestEntrypoints -List $list -Path "fixture/nondeterministic-source-ordering.txt"
}

if ($violations.Count -gt 0) {
    Write-Host "Native/managed binding-map contract failed with $($violations.Count) violation(s)."
    $violations | Sort-Object Path, Issue, Text | Format-Table Path, Issue, Text -AutoSize
    exit 1
}

Write-Host "NATIVE_MANAGED_BINDING_MAP_CONTRACT_OK native=2603 bound=2603 unbound=0 managed_only=0 imports=$($summary.managedImportMethodCount) sha256=$($summary.mappingSha256)"
Write-Host "Negative fixtures rejected: missing, duplicate, reorder, entrypoint drift, fixed-major identity, stale hash, undocumented classification, nondeterministic source ordering."
