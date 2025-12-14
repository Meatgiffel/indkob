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
DEBIAN_FRONTEND=noninteractive apt-get install -y nginx ca-certificates curl tar

useradd -r -s /usr/sbin/nologin indkob 2>/dev/null || true
install -d -o indkob -g indkob /opt/indkob /opt/indkob/releases /var/lib/indkob
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
