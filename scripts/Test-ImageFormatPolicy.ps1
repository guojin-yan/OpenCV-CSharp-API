param([string]$RepositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$repo = (Resolve-Path -LiteralPath $RepositoryRoot).Path
$policyRel = 'packaging/codec/image-format-policy.json'
$schemaRel = 'packaging/codec/image-format-policy.schema.json'
$policy = Join-Path $repo ($policyRel -replace '/', [IO.Path]::DirectorySeparatorChar)
$schema = Join-Path $repo ($schemaRel -replace '/', [IO.Path]::DirectorySeparatorChar)
foreach ($path in @($policy,$schema)) { if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Image format policy missing: $path" } }
if (-not (Test-Json -LiteralPath $policy -SchemaFile $schema -ErrorAction Stop)) { throw 'Image format policy failed JSON Schema validation.' }
$value = Get-Content -LiteralPath $policy -Raw | ConvertFrom-Json
$preflight = [IO.File]::ReadAllText((Join-Path $repo 'src/OpenCvSharp/ImgCodecs/Cv2.Preflight.cs'))
foreach ($format in @($value.formats)) {
    if ($preflight.IndexOf([string]$format.name, [StringComparison]::OrdinalIgnoreCase) -lt 0 -and [string]$format.name -notin @('j2k','jp2','pfm','radiance-hdr','openexr','bigtiff','sunraster')) { throw "Policy format is not represented by preflight sources: $($format.name)" }
    if ([string]$format.finalMatPrecision -cne 'unknown-until-decode') { throw "Policy format overclaims final precision: $($format.name)" }
}
if (@($value.formats).Count -lt 15 -or @($value.limits).Count -lt 10 -or @($value.forbiddenInferences).Count -lt 5) { throw 'Image format policy coverage is incomplete.' }
Write-Host "IMAGE_FORMAT_POLICY_OK formats=$(@($value.formats).Count) limits=$(@($value.limits).Count) final_precision_claim=unknown-until-decode"
