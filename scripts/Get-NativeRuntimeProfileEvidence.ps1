param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("full", "mini")]
    [string]$RuntimeProfile,
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$cmakePath = Join-Path $repo "src/OpenCvSharp.Native/CMakeLists.txt"
if (-not (Test-Path -LiteralPath $cmakePath -PathType Leaf)) {
    throw "Native CMakeLists.txt was not found: $cmakePath"
}

$cmakeText = [IO.File]::ReadAllText($cmakePath)
function Get-SourceList {
    param([Parameter(Mandatory = $true)][string]$VariableName)

    $match = [regex]::Match($cmakeText, "(?s)set\($VariableName(?<body>.*?)\)")
    if (-not $match.Success) {
        throw "Native CMake source list was not found: $VariableName"
    }

    return @(
        $match.Groups['body'].Value -split "`r?`n" |
            ForEach-Object { $_.Trim() } |
            Where-Object { $_ -match '^src/.+\.cpp$' }
    )
}

$miniSources = @(Get-SourceList -VariableName 'OPENCV_CSHARP_MINI_NATIVE_SOURCES')
$fullOnlySources = @(Get-SourceList -VariableName 'OPENCV_CSHARP_FULL_ONLY_NATIVE_SOURCES')
$sources = if ($RuntimeProfile -ceq 'mini') { $miniSources } else { @($miniSources + $fullOnlySources) }
$expectedSourceCount = if ($RuntimeProfile -ceq 'mini') { 9 } else { 49 }
$expectedAbiFunctionCount = if ($RuntimeProfile -ceq 'mini') { 527 } else { 2663 }
$abiManifestRelativePath = if ($RuntimeProfile -ceq 'mini') {
    'src/OpenCvSharp.Native/generated/native_abi_mini_manifest.txt'
}
else {
    'src/OpenCvSharp.Native/generated/native_abi_manifest.txt'
}
$abiManifestPath = Join-Path $repo $abiManifestRelativePath

if ($sources.Count -ne $expectedSourceCount -or @($sources | Sort-Object -Unique).Count -ne $expectedSourceCount) {
    throw "Native $RuntimeProfile profile source count drifted: actual=$($sources.Count) expected=$expectedSourceCount"
}
foreach ($source in $sources) {
    if (-not (Test-Path -LiteralPath (Join-Path (Split-Path -Parent $cmakePath) $source) -PathType Leaf)) {
        throw "Native $RuntimeProfile profile source was not found: $source"
    }
}
if (-not (Test-Path -LiteralPath $abiManifestPath -PathType Leaf)) {
    throw "Native ABI manifest was not found: $abiManifestRelativePath"
}

$abiFunctionLines = @(Get-Content -LiteralPath $abiManifestPath | Where-Object { $_ -match '^function-count=(\d+)$' })
if ($abiFunctionLines.Count -ne 1) {
    throw "Native ABI manifest must contain exactly one function-count entry: $abiManifestRelativePath"
}
$abiFunctionCount = [int]([regex]::Match($abiFunctionLines[0], '^function-count=(\d+)$').Groups[1].Value)
if ($abiFunctionCount -ne $expectedAbiFunctionCount) {
    throw "Native $RuntimeProfile ABI function count drifted: actual=$abiFunctionCount expected=$expectedAbiFunctionCount"
}

[ordered]@{
    RuntimeProfile = $RuntimeProfile
    Sources = @($sources)
    SourceCount = $sources.Count
    AbiManifest = $abiManifestRelativePath
    AbiFunctionCount = $abiFunctionCount
} | ConvertTo-Json -Depth 4
