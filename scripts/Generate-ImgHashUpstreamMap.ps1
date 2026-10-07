param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')),[switch]$Check)
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module img_hash -DisplayName ImgHash -RepositoryRoot $RepositoryRoot -Check:$Check
