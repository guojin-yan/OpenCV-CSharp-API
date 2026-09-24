param(
    [string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$schemaRelativePath = 'packaging/runtime/runtime-capabilities-json.schema.json'
$schemaPath = Join-Path $repo ($schemaRelativePath -replace '/', [IO.Path]::DirectorySeparatorChar)
if (-not (Test-Path -LiteralPath $schemaPath -PathType Leaf)) { throw "Capabilities JSON schema was not found: $schemaPath" }

$dotnet = Get-Command dotnet -ErrorAction Stop
$project = Join-Path $repo 'samples/ConsoleSamples/ConsoleSamples.csproj'
$output = @(& $dotnet.Source run --project $project -c Release --no-restore -- capabilities-json 2>&1)
if ($LASTEXITCODE -ne 0) { throw "capabilities-json sample failed with exit code $LASTEXITCODE." }
$jsonLines = @($output | ForEach-Object { [string]$_ } | Where-Object { $_.TrimStart().StartsWith('{') -and $_.TrimEnd().EndsWith('}') })
if ($jsonLines.Count -ne 1) { throw "capabilities-json sample must emit exactly one JSON object line; found $($jsonLines.Count)." }
$json = $jsonLines[0].Trim()
if (-not (Test-Json -Json $json -SchemaFile $schemaPath -ErrorAction Stop)) { throw 'capabilities-json output failed JSON Schema validation.' }
$snapshot = $json | ConvertFrom-Json
if ((@($snapshot.modules).Count -lt 4) -or ((@($snapshot.codecs | ForEach-Object { $_.extension }) -join '|') -ne '.png|.jpg|.webp|.tiff|.bmp|.gif|.exr|.jp2')) {
    throw 'capabilities-json required module or codec ordering drifted.'
}

$invalid = $snapshot | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$invalid | Add-Member -NotePropertyName unexpected -NotePropertyValue $true
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Capabilities schema accepted an unknown root property fixture.' }
$invalid = $snapshot | ConvertTo-Json -Depth 20 | ConvertFrom-Json
$invalid.nativeRuntime.state = 'Supported'
if (Test-Json -Json ($invalid | ConvertTo-Json -Depth 20) -SchemaFile $schemaPath -ErrorAction SilentlyContinue) { throw 'Capabilities schema accepted an invalid state fixture.' }

Write-Host "CAPABILITIES_JSON_CONTRACT_OK schema=$schemaRelativePath modules=$(@($snapshot.modules).Count) codecs=$(@($snapshot.codecs).Count) invalid_fixtures=2"
