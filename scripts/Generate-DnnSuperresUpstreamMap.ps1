param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')),[switch]$Check)
Set-StrictMode -Version Latest;$ErrorActionPreference='Stop'
& (Join-Path $PSScriptRoot 'Generate-ContribUpstreamMap.ps1') -Module dnn_superres -DisplayName DnnSuperres -RepositoryRoot $RepositoryRoot -Check:$Check
