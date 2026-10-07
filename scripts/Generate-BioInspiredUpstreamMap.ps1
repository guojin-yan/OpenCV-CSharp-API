param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')),[switch]$Check)
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
$repo=(Resolve-Path -LiteralPath $RepositoryRoot).Path
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module bioinspired -DisplayName BioInspired -RepositoryRoot $repo -Check:$Check
