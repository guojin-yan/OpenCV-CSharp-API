[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('\A(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\z')]
    [string]$OpenCvVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Match upstream cmake/OpenCVVersion.cmake: major + last two digits of 00<minor>.
# The patch number belongs to the full filename, not the ELF SONAME.
$parts = $OpenCvVersion.Split('.')
$paddedMinor = '00' + $parts[1]
$parts[0] + $paddedMinor.Substring($paddedMinor.Length - 2)
