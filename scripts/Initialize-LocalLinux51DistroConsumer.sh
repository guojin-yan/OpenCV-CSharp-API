#!/bin/sh
set -eu

consumer_rid="${1:-}"
. /etc/os-release
test "$(uname -m)" = x86_64
case "$consumer_rid:$ID:$VERSION_ID" in
  debian.12-x64:debian:12)
    # The minimal official image has archive signing keys but may lack a TLS bundle.
    if [ ! -s /etc/ssl/certs/ca-certificates.crt ]; then
      mkdir -p /etc/ssl/certs
      cp /kit/toolchain/ubuntu-ca-certificates.crt /etc/ssl/certs/ca-certificates.crt
    fi
    sed -i 's|http://deb.debian.org|https://deb.debian.org|g' /etc/apt/sources.list.d/debian.sources
    export DEBIAN_FRONTEND=noninteractive
    apt-get -o Acquire::Retries=3 update
    apt-get -o Acquire::Retries=3 install -y --no-install-recommends \
      ca-certificates libicu72 libssl3 libunwind8 libgssapi-krb5-2 zlib1g libstdc++6 tar gzip
    ;;
  fedora.44-x64:fedora:44)
    dnf -y --setopt=install_weak_deps=False install \
      ca-certificates libicu openssl-libs libunwind krb5-libs zlib-ng-compat libstdc++ tar gzip findutils
    ;;
  *)
    echo "Unexpected independent consumer: $consumer_rid/$ID/$VERSION_ID" >&2
    exit 1
    ;;
esac

printf '%s  %s\n' dcfe6060fc86abcb859ce1f8f80843ce50bab0585396de56380ed9f25176ac6d /kit/toolchain/powershell.tar.gz | sha256sum -c -
printf '%s  %s\n' 51c8b999af9e8dd9998c9edc5944e19a90788862068acd38694e098889054ce8c23d4f0c5cccfa16bf187d044562359e5ee69a9f8ad0bbe913ba90311fbce25b /kit/toolchain/dotnet.tar.gz | sha512sum -c -
mkdir -p /opt/dotnet /opt/powershell /consumer/artifacts
cp -a /kit/repository /consumer/repository
cp -a /kit/packages/. /consumer/artifacts/
tar -xzf /kit/toolchain/dotnet.tar.gz -C /opt/dotnet
tar -xzf /kit/toolchain/powershell.tar.gz -C /opt/powershell
chmod +x /opt/powershell/pwsh
ln -s /opt/dotnet/dotnet /usr/local/bin/dotnet
ln -s /opt/powershell/pwsh /usr/local/bin/pwsh
for forbidden in gcc g++ clang clang++ cc c++ cmake ninja; do
  if command -v "$forbidden"; then exit 9; fi
done
test ! -e /workspace
test ! -e /repo-input
test ! -e /upstream-input
getconf GNU_LIBC_VERSION
pwsh -NoProfile -Command '$PSVersionTable.PSVersion.ToString()'
DOTNET_ROOT=/opt/dotnet dotnet --version
echo "LOCAL_LINUX_51_DISTRO_CONSUMER_READY rid=$consumer_rid"
