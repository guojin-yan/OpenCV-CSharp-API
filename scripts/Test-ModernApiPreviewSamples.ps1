param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$programPath = Join-Path $repo 'samples/ConsoleSamples/Program.cs'
$guidePath = Join-Path $repo 'docs/articles/v501-modern-api-preview-guide.md'
$projectPath = Join-Path $repo 'samples/ConsoleSamples/ConsoleSamples.csproj'
$readmePath = Join-Path $repo 'README.md'
$readmeCnPath = Join-Path $repo 'README_cn.md'
foreach ($path in @($programPath,$guidePath,$projectPath,$readmePath,$readmeCnPath)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Modern preview sample file missing: $path" } }
$program = [IO.File]::ReadAllText($programPath)
$guide = [IO.File]::ReadAllText($guidePath)
$readme = [IO.File]::ReadAllText($readmePath)
$readmeCn = [IO.File]::ReadAllText($readmeCnPath)
foreach ($token in @('typed-mat','buffered-codec','platform-probe','headless-smoke','HeadlessServer','RunTypedMatPreview','RunBufferedCodecPreview','RunPlatformProbe','RunHeadlessSmoke','native-runtime-required','single-getspan-single-advance')) {
    if ($program.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "ConsoleSamples is missing preview sample token: $token" }
}
foreach ($token in @('typed-mat','buffered-codec','headless-smoke','HeadlessServer','skip','ROI','IBufferWriter','DISPLAY','WAYLAND_DISPLAY')) {
    if ($guide.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "Preview guide is missing sample token: $token" }
}
foreach ($readmeName in @('README.md','README_cn.md')) {
    $readmeText = if ($readmeName -ceq 'README.md') { $readme } else { $readmeCn }
    foreach ($token in @('docs/articles/v501-modern-api-preview-guide.md','capabilities-json','typed-mat','buffered-codec','headless-smoke','platform-probe','candidate-only')) {
        if ($readmeText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "$readmeName is missing 5.0.1 preview token: $token" }
    }
}
$dotnet = Get-Command dotnet -ErrorAction Stop
& $dotnet.Source build $projectPath -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "ConsoleSamples build failed with exit code $LASTEXITCODE." }
$displayValue = $env:DISPLAY
$waylandValue = $env:WAYLAND_DISPLAY
try {
    Remove-Item Env:DISPLAY -ErrorAction SilentlyContinue
    Remove-Item Env:WAYLAND_DISPLAY -ErrorAction SilentlyContinue
    $headlessOutput = @(& $dotnet.Source run --project $projectPath -c Release --no-build -- headless-smoke 2>&1)
    $headlessExitCode = $LASTEXITCODE
}
finally {
    if ($null -eq $displayValue) { Remove-Item Env:DISPLAY -ErrorAction SilentlyContinue } else { $env:DISPLAY = $displayValue }
    if ($null -eq $waylandValue) { Remove-Item Env:WAYLAND_DISPLAY -ErrorAction SilentlyContinue } else { $env:WAYLAND_DISPLAY = $waylandValue }
}
if ($headlessExitCode -ne 0) { throw "Headless smoke sample failed with exit code $headlessExitCode." }
$headlessJsonLines = @($headlessOutput | ForEach-Object { [string]$_ } | Where-Object { $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}') })
if ($headlessJsonLines.Count -ne 1) { throw "Headless smoke must emit exactly one JSON object; found $($headlessJsonLines.Count)." }
$headless = $headlessJsonLines[0] | ConvertFrom-Json
foreach ($property in @('status','command','sample')) { if ($null -eq $headless.PSObject.Properties[$property]) { throw "Headless smoke is missing property: $property" } }
if ([string]$headless.command -cne 'headless-smoke' -or [string]$headless.sample -cne 'HeadlessServer' -or [string]$headless.status -notin @('measured','skipped')) { throw 'Headless smoke identity or status is invalid.' }
if ([string]$headless.status -ceq 'measured') {
    foreach ($property in @('displayUnset','waylandUnset','guiBackendState','encodedBytes','decodedRows','decodedColumns','highGuiCalls')) {
        if ($null -eq $headless.PSObject.Properties[$property]) { throw "Measured headless smoke is missing property: $property" }
    }
    if (-not [bool]$headless.displayUnset -or -not [bool]$headless.waylandUnset -or [bool]$headless.highGuiCalls -or [int]$headless.encodedBytes -le 0 -or [int]$headless.decodedRows -ne 2 -or [int]$headless.decodedColumns -ne 2) {
        throw 'Measured headless smoke did not prove the no-display codec boundary.'
    }
}
Write-Host "MODERN_API_PREVIEW_SAMPLES_OK commands=typed-mat,buffered-codec,platform-probe,headless-smoke headless_status=$($headless.status) native_runtime_optional=true"
