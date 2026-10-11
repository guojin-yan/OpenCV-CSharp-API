param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,[switch]$RegenerateRaw,[string]$PythonPath="",[switch]$Check)
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
$workspace=(Resolve-Path (Join-Path $repo '..')).Path
$raw=Join-Path $repo 'compatibility/xobjdetect-upstream-raw.json'
if($RegenerateRaw){if([string]::IsNullOrWhiteSpace($PythonPath)){throw '-RegenerateRaw requires -PythonPath'};& $PythonPath (Join-Path $repo 'tools/XObjDetectUpstreamMap/extract_xobjdetect.py') --workspace $workspace --opencv-root (Join-Path $workspace 'opencv-source/opencv-5.0.0') --output $raw;if($LASTEXITCODE -ne 0){throw 'XObjDetect raw extraction failed.'}}
& (Join-Path $repo 'scripts/Generate-ContribUpstreamMap.ps1') -Module xobjdetect -DisplayName XObjDetect -RepositoryRoot $repo -Check:$Check
