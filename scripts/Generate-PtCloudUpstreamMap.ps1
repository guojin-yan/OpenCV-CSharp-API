param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')),[switch]$Check)
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace=(Resolve-Path -LiteralPath (Join-Path $repo '..')).Path
$python='C:\Users\guoji\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe'
if (-not (Test-Path -LiteralPath $python -PathType Leaf)) { throw "Python runtime is missing: $python" }
$extractor=Join-Path $repo 'tools/PtCloudUpstreamMap/extract_ptcloud.py'
$raw=Join-Path $repo 'compatibility/ptcloud-upstream-raw.json'
if ($Check) {
    $probe = [IO.Path]::GetTempFileName()
    try {
        & $python $extractor --workspace $workspace --opencv-root (Join-Path $workspace 'opencv-source/opencv-5.0.0') --output $probe
        if ($LASTEXITCODE -ne 0) { throw 'PtCloud upstream extraction failed.' }
        $currentRaw = (Get-Content -LiteralPath $raw -Raw | ConvertFrom-Json | ConvertTo-Json -Depth 30)
        $expectedRaw = (Get-Content -LiteralPath $probe -Raw | ConvertFrom-Json | ConvertTo-Json -Depth 30)
        if ($currentRaw -ne $expectedRaw) { throw "PtCloud upstream raw evidence is stale: $raw" }
    } finally {
        if (Test-Path -LiteralPath $probe) { Remove-Item -LiteralPath $probe -Force }
    }
} else {
    & $python $extractor --workspace $workspace --opencv-root (Join-Path $workspace 'opencv-source/opencv-5.0.0') --output $raw
    if ($LASTEXITCODE -ne 0) { throw 'PtCloud upstream extraction failed.' }
}
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module ptcloud -DisplayName PtCloud -RepositoryRoot $repo -Check:$Check
