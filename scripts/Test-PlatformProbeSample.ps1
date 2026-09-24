param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$project = Join-Path $repo 'samples/ConsoleSamples/ConsoleSamples.csproj'
$program = Join-Path $repo 'samples/ConsoleSamples/Program.cs'
$guide = Join-Path $repo 'docs/articles/v501-modern-api-preview-guide.md'
foreach ($path in @($project,$program,$guide)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Platform probe sample file missing: $path" } }
$programText = [IO.File]::ReadAllText($program)
$guideText = [IO.File]::ReadAllText($guide)
foreach ($token in @('platform-probe','RunPlatformProbe','JsonSerializer','nativeRuntimeState','warnings')) { if ($programText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "Platform probe implementation token missing: $token" } }
foreach ($token in @('platform-probe','PlatformProbe','absolute','path')) { if ($guideText.IndexOf($token, [StringComparison]::OrdinalIgnoreCase) -lt 0) { throw "Platform probe documentation token missing: $token" } }
$dotnet = Get-Command dotnet -ErrorAction Stop
& $dotnet.Source build $project -c Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "ConsoleSamples build failed with exit code $LASTEXITCODE." }
$output = @(& $dotnet.Source run --project $project -c Release --no-build -- platform-probe 2>&1)
if ($LASTEXITCODE -ne 0) { throw "Platform probe sample failed with exit code $LASTEXITCODE." }
$jsonLines = @($output | ForEach-Object { [string]$_ } | Where-Object { $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}') })
if ($jsonLines.Count -ne 1) { throw "Platform probe must emit exactly one JSON object; found $($jsonLines.Count)." }
$probe = $jsonLines[0] | ConvertFrom-Json
foreach ($property in @('status','command','operatingSystem','operatingSystemDescription','processArchitecture','processBitness','runtimeIdentifier','runtimeFrameworkDescription','nativeRuntimeState','warnings')) { if ($null -eq $probe.PSObject.Properties[$property]) { throw "Platform probe is missing property: $property" } }
if ([string]$probe.status -cne 'measured' -or [string]$probe.command -cne 'platform-probe' -or [int]$probe.processBitness -notin @(32,64)) { throw 'Platform probe identity is invalid.' }
if ($jsonLines[0] -match '(?i)([A-Za-z]:\\|/home/|/Users/|LD_LIBRARY_PATH|OPENCV_CSHARP_OPENCV_RUNTIME_ROOT)') { throw 'Platform probe exposed a path or runtime environment variable.' }
Write-Host 'PLATFORM_PROBE_SAMPLE_OK command=platform-probe path_free=true native_runtime_optional=true'
