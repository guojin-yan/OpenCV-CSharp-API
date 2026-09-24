param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$programPath = Join-Path $repo 'samples/ConsoleSamples/Program.cs'
$guidePath = Join-Path $repo 'docs/articles/v501-modern-api-preview-guide.md'
$projectPath = Join-Path $repo 'samples/ConsoleSamples/ConsoleSamples.csproj'
foreach ($path in @($programPath,$guidePath,$projectPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Modern preview sample file missing: $path" } }
$program = [IO.File]::ReadAllText($programPath)
$guide = [IO.File]::ReadAllText($guidePath)
foreach ($token in @('typed-mat','buffered-codec','platform-probe','RunTypedMatPreview','RunBufferedCodecPreview','RunPlatformProbe','native-runtime-required','single-getspan-single-advance')) {
    if ($program.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "ConsoleSamples is missing preview sample token: $token" }
}
foreach ($token in @('typed-mat','buffered-codec','skip','ROI','IBufferWriter')) {
    if ($guide.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "Preview guide is missing sample token: $token" }
}
$dotnet = Get-Command dotnet -ErrorAction Stop
& $dotnet.Source build $projectPath -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "ConsoleSamples build failed with exit code $LASTEXITCODE." }
Write-Host 'MODERN_API_PREVIEW_SAMPLES_OK commands=typed-mat,buffered-codec native_runtime_optional=true'
