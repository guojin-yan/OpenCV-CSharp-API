#!/bin/sh
set -eu
. /etc/os-release
test "$(uname -m)" = x86_64
if [ ! -s /etc/ssl/certs/ca-certificates.crt ]; then
  mkdir -p /etc/ssl/certs
  cp /kit/toolchain/ubuntu-ca-certificates.crt /etc/ssl/certs/ca-certificates.crt
fi
case "$ID:$VERSION_ID" in
  ubuntu:22.04)
    sed -i 's|http://archive.ubuntu.com|https://archive.ubuntu.com|g;s|http://security.ubuntu.com|https://security.ubuntu.com|g' /etc/apt/sources.list
    export DEBIAN_FRONTEND=noninteractive
    apt-get -o Acquire::Retries=3 update
    apt-get -o Acquire::Retries=3 install -y --no-install-recommends ca-certificates libicu70 libssl3 libunwind8 libgssapi-krb5-2 zlib1g libstdc++6 tar gzip
    ;;
  debian:12)
    sed -i 's|http://deb.debian.org|https://deb.debian.org|g' /etc/apt/sources.list.d/debian.sources
    export DEBIAN_FRONTEND=noninteractive
    apt-get -o Acquire::Retries=3 update
    apt-get -o Acquire::Retries=3 install -y --no-install-recommends ca-certificates libicu72 libssl3 libunwind8 libgssapi-krb5-2 zlib1g libstdc++6 tar gzip
    ;;
  ubuntu:24.04)
    sed -i 's|http://archive.ubuntu.com|https://archive.ubuntu.com|g;s|http://security.ubuntu.com|https://security.ubuntu.com|g' /etc/apt/sources.list.d/ubuntu.sources
    export DEBIAN_FRONTEND=noninteractive
    apt-get -o Acquire::Retries=3 update
    apt-get -o Acquire::Retries=3 install -y --no-install-recommends ca-certificates libicu74 libssl3t64 libunwind8 libgssapi-krb5-2 zlib1g libstdc++6 tar gzip
    ;;
  fedora:44)
    dnf -y --setopt=install_weak_deps=False install ca-certificates libicu openssl-libs libunwind krb5-libs zlib-ng-compat libstdc++ tar gzip findutils
    ;;
  *) echo 'Unexpected module-consumer distro' >&2; exit 1 ;;
esac
printf '%s  %s\n' dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d /kit/toolchain/powershell.tar.gz | sha256sum -c -
printf '%s  %s\n' 33212bb99a2bf003973d41106c7e8fe94208a12958ff601d4a476be379403c85a7dc9183a6ec64dfb44ded7102965be09f35add1758e68e9251bae7891d7b7bf /kit/toolchain/dotnet8.tar.gz | sha512sum -c -
printf '%s  %s\n' 51c8b999af9e8dd9998c9edc5944e19a90788862068acd38694e098889054ce8c23d4f0c5cccfa16bf187d044562359e5ee69a9f8ad0bbe913ba90311fbce25b /kit/toolchain/dotnet10.tar.gz | sha512sum -c -
mkdir -p /opt/dotnet8 /opt/dotnet10 /opt/powershell /consumer/artifacts
cp -a /kit/source /consumer/source
cp -a /kit/scripts /consumer/scripts
cp -a /kit/packages /consumer/feed
cp /kit/packages.json /consumer/packages.json
tar -xzf /kit/toolchain/dotnet8.tar.gz -C /opt/dotnet8
tar -xzf /kit/toolchain/dotnet10.tar.gz -C /opt/dotnet10
tar -xzf /kit/toolchain/powershell.tar.gz -C /opt/powershell
chmod +x /opt/powershell/pwsh
ln -s /opt/powershell/pwsh /usr/local/bin/pwsh
for forbidden in gcc g++ clang clang++ cc c++ cmake ninja; do
  if command -v "$forbidden"; then exit 9; fi
done
test ! -e /workspace
test ! -e /repo-input
test ! -e /upstream-input
test ! -e /tmp/.X11-unix
if dpkg-query -W -f='${binary:Package}\n' | awk '/^(libgtk|libgdk|libqt|libx11|libxext|libwayland)/ {found=1} END {exit !found}'; then
  echo 'GUI runtime installed in candidate consumer' >&2
  exit 9
fi
getconf GNU_LIBC_VERSION
pwsh -NoProfile -Command '$PSVersionTable.PSVersion.ToString()'
/opt/dotnet8/dotnet --version
/opt/dotnet10/dotnet --version
echo "HEADLESS_51_CONSUMER_READY distro=$ID version=$VERSION_ID"
