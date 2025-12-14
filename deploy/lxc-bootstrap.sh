#!/usr/bin/env bash
set -euo pipefail

REPO="${1:-${INDKOB_REPO:-}}"
if [[ -z "${REPO}" ]]; then
  echo "Usage: $0 <owner/repo>"
  echo "Or set INDKOB_REPO=owner/repo"
  exit 1
fi

if [[ "$(id -u)" -ne 0 ]]; then
  echo "Run as root (sudo)."
  exit 1
fi

DEBIAN_FRONTEND=noninteractive apt-get update
DEBIAN_FRONTEND=noninteractive apt-get install -y nginx ca-certificates curl tar zlib1g

install_first_available() {
  for pkg in "$@"; do
    if DEBIAN_FRONTEND=noninteractive apt-get install -y "$pkg" >/dev/null 2>&1; then
      echo "Installed: $pkg"
      return 0
    fi
  done
  return 1
}

# .NET self-contained apps still rely on a few system libraries (ICU/SSL).
install_first_available libicu76 libicu75 libicu74 libicu73 libicu72 libicu71 libicu70 libicu67 libicu-dev || true
install_first_available libssl3 libssl1.1 libssl-dev || true

useradd -r -s /usr/sbin/nologin indkob 2>/dev/null || true
install -d -o indkob -g indkob /opt/indkob /opt/indkob/releases /var/lib/indkob
install -d -o indkob -g indkob /var/lib/indkob/bundle-extract
install -d -o www-data -g www-data /var/www

install -m 0755 -d /usr/local/bin
install -m 0755 "$(dirname "$0")/lxc-update.sh" /usr/local/bin/indkob-update

install -m 0644 "$(dirname "$0")/indkob-api.service" /etc/systemd/system/indkob-api.service

install -d /etc/nginx/sites-available /etc/nginx/sites-enabled
install -m 0644 "$(dirname "$0")/nginx-indkob.conf" /etc/nginx/sites-available/indkob
ln -sf /etc/nginx/sites-available/indkob /etc/nginx/sites-enabled/indkob
rm -f /etc/nginx/sites-enabled/default || true
nginx -t
systemctl enable --now nginx

systemctl daemon-reload
systemctl enable indkob-api.service

/usr/local/bin/indkob-update "${REPO}"

echo "Done. Open: http://<container-ip>/"
