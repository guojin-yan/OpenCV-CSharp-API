param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path,
    [Parameter(Mandatory)][string]$PythonPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$requirements = Join-Path $repo 'tools/CudevUpstreamMap/syntax-requirements.txt'
$requirementsHash = (Get-FileHash -LiteralPath $requirements -Algorithm SHA256).Hash.ToLowerInvariant()
$pythonTag = & $PythonPath -c 'import sys, sysconfig; print(sys.implementation.cache_tag + "-" + sysconfig.get_platform())'
if ($LASTEXITCODE -ne 0 -or $pythonTag -notmatch '^[A-Za-z0-9_.-]+$') { throw 'Cannot identify the Cudev generation Python runtime.' }
$target = Join-Path $repo "artifacts/tools/cudev-syntax/$pythonTag-$($requirementsHash.Substring(0,16))"
$marker = Join-Path $target 'requirements.sha256'
if (-not (Test-Path -LiteralPath $marker -PathType Leaf) -or [IO.File]::ReadAllText($marker).Trim() -cne $requirementsHash) {
    # A dedicated ignored target avoids changing the user's Python environment.
    & $PythonPath -m pip --isolated install --disable-pip-version-check --index-url https://pypi.org/simple --require-hashes --only-binary=:all: --no-deps --upgrade --target $target --requirement $requirements | Out-Host
    if ($LASTEXITCODE -ne 0) { throw 'Pinned Cudev syntax parser wheel installation failed.' }
    [IO.File]::WriteAllText($marker, "$requirementsHash`n", [Text.UTF8Encoding]::new($false))
}
return $target
