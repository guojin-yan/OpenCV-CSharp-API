function ConvertTo-OpenCvCSharpPackageVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Version
    )

    $match = [regex]::Match(
        $Version,
        '^(?<major>0|[1-9][0-9]*)\.(?<minor>0|[1-9][0-9]*)\.(?<patch>0|[1-9][0-9]*)\.(?<revision>0|[1-9][0-9]*)(?<suffix>-(?<prerelease>[0-9a-z-]+(?:\.[0-9a-z-]+)*))?$')
    if (-not $match.Success) {
        throw "PackageVersion must use four numeric parts with an optional lowercase SemVer prerelease suffix, for example 5.0.0.0 or 5.0.0.0-preview.1. Actual: $Version"
    }

    $numericParts = [System.Collections.Generic.List[int]]::new()
    foreach ($name in @('major', 'minor', 'patch', 'revision')) {
        $value = 0
        if (-not [int]::TryParse(
                $match.Groups[$name].Value,
                [System.Globalization.NumberStyles]::None,
                [System.Globalization.CultureInfo]::InvariantCulture,
                [ref]$value)) {
            throw "PackageVersion numeric part '$name' is outside the supported Int32 range. Actual: $Version"
        }

        $numericParts.Add($value)
    }

    $prerelease = $match.Groups['prerelease'].Value
    if (-not [string]::IsNullOrEmpty($prerelease)) {
        foreach ($identifier in $prerelease.Split('.')) {
            if ($identifier -match '^[0-9]+$' -and $identifier.Length -gt 1 -and $identifier[0] -eq '0') {
                throw "PackageVersion numeric prerelease identifiers must not contain leading zeroes. Actual: $Version"
            }
        }
    }

    $openCvVersion = '{0}.{1}.{2}' -f $numericParts[0], $numericParts[1], $numericParts[2]
    $nuGetVersion = if ($numericParts[3] -eq 0) {
        $openCvVersion
    }
    else {
        '{0}.{1}' -f $openCvVersion, $numericParts[3]
    }

    if (-not [string]::IsNullOrEmpty($prerelease)) {
        $nuGetVersion = "$nuGetVersion-$prerelease"
    }

    return [pscustomobject]@{
        InputVersion = $Version
        OpenCvVersion = $openCvVersion
        PackageRevision = $numericParts[3]
        Prerelease = $prerelease
        IsPrerelease = -not [string]::IsNullOrEmpty($prerelease)
        NuGetVersion = $nuGetVersion
    }
}

function Assert-OpenCvCSharpPackageVersion {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Version,
        [Parameter(Mandatory = $true)]
        [string]$OpenCvVersion,
        [Parameter(Mandatory = $true)]
        [int]$PackageRevision
    )

    $record = ConvertTo-OpenCvCSharpPackageVersion -Version $Version
    if ($record.OpenCvVersion -ne $OpenCvVersion) {
        throw "PackageVersion must target OpenCV version '$OpenCvVersion'. Actual: $Version"
    }

    if ($record.PackageRevision -ne $PackageRevision) {
        throw "PackageVersion must use package revision '$PackageRevision'. Actual: $Version"
    }

    return $record
}
