#!/usr/bin/env bash
set -euo pipefail

REPO="${1:-${INDKOB_REPO:-}}"
if [[ -z "${REPO}" ]]; then
  echo "Usage: $0 <owner/repo>"
  echo "Or set INDKOB_REPO=owner/repo"
  exit 1
fi

ARCH="$(uname -m)"
if [[ "${ARCH}" != "x86_64" ]]; then
  echo "This deploy script is configured for x86_64 only (got: ${ARCH})."
  exit 1
fi

ASSET_NAME="indkob-release-linux-x64.tar.gz"
DOWNLOAD_URL="https://github.com/${REPO}/releases/latest/download/${ASSET_NAME}"

RELEASES_DIR="/opt/indkob/releases"
CURRENT_DIR="/opt/indkob/current"
API_LINK="/opt/indkob/api"
WWW_LINK="/var/www/indkob"
DATA_DIR="/var/lib/indkob"
BUNDLE_DIR="/var/lib/indkob/bundle-extract"

if [[ "$(id -u)" -ne 0 ]]; then
  echo "Run as root (sudo)."
  exit 1
fi

install_first_available() {
  for pkg in "$@"; do
    if DEBIAN_FRONTEND=noninteractive apt-get install -y "$pkg" >/dev/null 2>&1; then
      echo "Installed: $pkg"
      return 0
    fi
  done
  return 1
}

maybe_install_dotnet_deps() {
  if ! command -v apt-get >/dev/null 2>&1; then
    return 0
  fi

  # Only run apt-get update if something looks missing.
  local need_update="false"
  dpkg -s zlib1g >/dev/null 2>&1 || need_update="true"
  (ldconfig -p 2>/dev/null | grep -q 'libicu') || need_update="true"
  (ldconfig -p 2>/dev/null | grep -q 'libssl') || need_update="true"

  if [[ "${need_update}" == "true" ]]; then
    DEBIAN_FRONTEND=noninteractive apt-get update >/dev/null 2>&1 || true
    DEBIAN_FRONTEND=noninteractive apt-get install -y zlib1g >/dev/null 2>&1 || true
    install_first_available libicu76 libicu75 libicu74 libicu73 libicu72 libicu71 libicu70 libicu67 libicu-dev || true
    install_first_available libssl3 libssl1.1 libssl-dev || true
  fi
}

maybe_install_dotnet_deps

install -d -m 0755 "${RELEASES_DIR}" "${DATA_DIR}" "${BUNDLE_DIR}"
install -d -m 0755 /var/www

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "${TMP_DIR}"' EXIT

echo "Downloading ${DOWNLOAD_URL}"
curl -fL "${DOWNLOAD_URL}" -o "${TMP_DIR}/${ASSET_NAME}"

STAMP="$(date +%Y%m%d%H%M%S)"
DEST="${RELEASES_DIR}/${STAMP}"
mkdir -p "${DEST}"

tar -xzf "${TMP_DIR}/${ASSET_NAME}" -C "${DEST}"

if [[ ! -x "${DEST}/api/Api" ]]; then
  chmod +x "${DEST}/api/Api" || true
fi

ln -sfn "${DEST}" "${CURRENT_DIR}"
ln -sfn "${CURRENT_DIR}/api" "${API_LINK}"
ln -sfn "${CURRENT_DIR}/www" "${WWW_LINK}"

if id indkob >/dev/null 2>&1; then
  chown -R indkob:indkob "${CURRENT_DIR}/api" || true
  chown -R www-data:www-data "${CURRENT_DIR}/www" || true
  chown -R indkob:indkob "${DATA_DIR}" || true
  chown -R indkob:indkob "${BUNDLE_DIR}" || true
fi

# Ensure the single-file bundle can extract to a writable path (works even if the unit file is old).
install -d -m 0755 /etc/systemd/system/indkob-api.service.d
cat >/etc/systemd/system/indkob-api.service.d/override.conf <<'EOF'
[Service]
Environment=DOTNET_BUNDLE_EXTRACT_BASE_DIR=/var/lib/indkob/bundle-extract
EOF

systemctl daemon-reload || true
systemctl restart indkob-api || true
systemctl reload nginx || true

echo "Updated to ${STAMP} (${ASSET_NAME})"

# keep last 5 releases
ls -1dt "${RELEASES_DIR}"/* 2>/dev/null | tail -n +6 | xargs -r rm -rf
