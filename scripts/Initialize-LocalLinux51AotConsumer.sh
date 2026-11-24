#!/bin/sh
set -eu
. /etc/os-release
test "$(uname -m)" = x86_64
if [ ! -s /etc/ssl/certs/ca-certificates.crt ]; then
  mkdir -p /etc/ssl/certs
  cp /kit/ubuntu-ca-certificates.crt /etc/ssl/certs/ca-certificates.crt
fi
case "$ID:$VERSION_ID" in
  ubuntu:22.04)
    sed -i 's|http://archive.ubuntu.com|https://archive.ubuntu.com|g;s|http://security.ubuntu.com|https://security.ubuntu.com|g' /etc/apt/sources.list
    export DEBIAN_FRONTEND=noninteractive
    apt-get -o Acquire::Retries=3 update
    apt-get -o Acquire::Retries=3 install -y --no-install-recommends ca-certificates libstdc++6 libssl3 zlib1g
    ;;
  debian:12)
    sed -i 's|http://deb.debian.org|https://deb.debian.org|g' /etc/apt/sources.list.d/debian.sources
    export DEBIAN_FRONTEND=noninteractive
    apt-get -o Acquire::Retries=3 update
    apt-get -o Acquire::Retries=3 install -y --no-install-recommends ca-certificates libstdc++6 libssl3 zlib1g
    ;;
  fedora:44)
    dnf -y --setopt=install_weak_deps=False install ca-certificates libstdc++ openssl-libs zlib-ng-compat
    ;;
  *) echo 'Unexpected AOT consumer distribution' >&2; exit 1 ;;
esac
test ! -e /consumer/publish
mkdir -p /consumer/artifacts
cp -a /kit/publish /consumer/publish
chmod +x /consumer/publish/full/LinuxAotConsumer /consumer/publish/mini/LinuxAotConsumer
echo "LOCAL_LINUX_51_AOT_CONSUMER_READY distro=$ID version=$VERSION_ID"
