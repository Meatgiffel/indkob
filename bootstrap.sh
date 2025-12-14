#!/usr/bin/env bash
set -euo pipefail

REPO="${1:-}"

if [[ -z "${REPO}" ]]; then
  if command -v git >/dev/null 2>&1; then
    REPO="$(git config --get remote.origin.url 2>/dev/null || true)"
  fi

  if [[ "${REPO}" =~ ^git@github\.com:([^/]+/[^/]+)(\.git)?$ ]]; then
    REPO="${BASH_REMATCH[1]}"
  elif [[ "${REPO}" =~ ^https?://github\.com/([^/]+/[^/]+)(\.git)?/?$ ]]; then
    REPO="${BASH_REMATCH[1]}"
  elif [[ "${REPO}" =~ ^ssh://git@github\.com/([^/]+/[^/]+)(\.git)?$ ]]; then
    REPO="${BASH_REMATCH[1]}"
  else
    REPO=""
  fi
fi

if [[ -z "${REPO}" ]]; then
  echo "Usage: $0 <owner/repo>"
  echo "Example: $0 Meatgiffel/indkob"
  exit 1
fi

if [[ ! -f "deploy/lxc-bootstrap.sh" ]]; then
  echo "Missing: deploy/lxc-bootstrap.sh"
  echo "Run this from the repo root."
  exit 1
fi

if [[ "$(id -u)" -ne 0 ]]; then
  if ! command -v sudo >/dev/null 2>&1; then
    echo "This command needs root privileges (sudo not found)."
    exit 1
  fi
fi

sudo bash deploy/lxc-bootstrap.sh "${REPO}"

