#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

REPO="${1:-}"
SUDO="sudo"

detect_repo_from_git_remote() {
  if ! command -v git >/dev/null 2>&1; then
    return 1
  fi

  local remote
  remote="$(git -C "${SCRIPT_DIR}" config --get remote.origin.url 2>/dev/null || true)"
  if [[ -z "${remote}" ]]; then
    return 1
  fi

  # Examples:
  # - git@github.com:Meatgiffel/indkob.git
  # - https://github.com/Meatgiffel/indkob.git
  # - ssh://git@github.com/Meatgiffel/indkob.git
  if [[ "${remote}" =~ ^git@github\.com:([^/]+/[^/]+)(\.git)?$ ]]; then
    echo "${BASH_REMATCH[1]}"
    return 0
  fi
  if [[ "${remote}" =~ ^https?://github\.com/([^/]+/[^/]+)(\.git)?/?$ ]]; then
    echo "${BASH_REMATCH[1]}"
    return 0
  fi
  if [[ "${remote}" =~ ^ssh://git@github\.com/([^/]+/[^/]+)(\.git)?$ ]]; then
    echo "${BASH_REMATCH[1]}"
    return 0
  fi

  return 1
}

if [[ -z "${REPO}" ]]; then
  REPO="$(detect_repo_from_git_remote || true)"
fi

REPO="${REPO%.git}"

if [[ -z "${REPO}" ]]; then
  echo "Usage: $0 <owner/repo>"
  echo "Tip: run this inside a git checkout with origin set to GitHub."
  exit 1
fi

if [[ "$(id -u)" -ne 0 ]]; then
  if ! command -v sudo >/dev/null 2>&1; then
    echo "This command needs root privileges (sudo not found)."
    exit 1
  fi
else
  SUDO=""
fi

if [[ -f "${SCRIPT_DIR}/deploy/lxc-update.sh" ]]; then
  ${SUDO} install -m 0755 "${SCRIPT_DIR}/deploy/lxc-update.sh" /usr/local/bin/indkob-update
fi

if [[ ! -x "/usr/local/bin/indkob-update" ]]; then
  echo "Missing: /usr/local/bin/indkob-update"
  echo "Run first-time install: ${SCRIPT_DIR}/bootstrap.sh ${REPO}"
  exit 1
fi

${SUDO} /usr/local/bin/indkob-update "${REPO}"
