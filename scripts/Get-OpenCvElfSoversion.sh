#!/bin/sh
set -eu

version="${1:-}"
case "$version" in
  ""|*[!0-9.]*)
    echo "Expected an OpenCV major.minor.patch version: $version" >&2
    exit 1
    ;;
esac
if ! printf '%s\n' "$version" | LC_ALL=C grep -Eq '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$'; then
  echo "Expected an OpenCV major.minor.patch version: $version" >&2
  exit 1
fi

# Match cmake/OpenCVVersion.cmake; the patch number is not part of the SONAME.
major="${version%%.*}"
remainder="${version#*.}"
minor="${remainder%%.*}"
minor_tail="$(printf '%s\n' "00$minor" | sed 's/.*\(..\)$/\1/')"
printf '%s%s\n' "$major" "$minor_tail"
