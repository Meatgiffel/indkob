#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
OUT_DIR="${ROOT_DIR}/release"

rm -rf "${OUT_DIR}"
mkdir -p "${OUT_DIR}/api" "${OUT_DIR}/www"

echo "Building client…"
pushd "${ROOT_DIR}/client" >/dev/null
npm ci
npm run build
popd >/dev/null

cp -R "${ROOT_DIR}/client/dist/client/browser/." "${OUT_DIR}/www/"

echo "Writing build info…"
VERSION="${GITHUB_REF_NAME:-}"
if [[ -z "${VERSION}" ]]; then
  VERSION="$(git -C "${ROOT_DIR}" describe --tags --always --dirty 2>/dev/null || echo "dev")"
fi
COMMIT_SHA="$(git -C "${ROOT_DIR}" rev-parse --short HEAD 2>/dev/null || echo "unknown")"
BUILD_TIME_UTC="$(date -u +%Y-%m-%dT%H:%M:%SZ)"

cat >"${OUT_DIR}/www/version.json" <<EOF
{
  "version": "${VERSION}",
  "commit": "${COMMIT_SHA}",
  "builtAtUtc": "${BUILD_TIME_UTC}"
}
EOF

echo "Publishing API (self-contained linux-x64)…"
pushd "${ROOT_DIR}/Api" >/dev/null
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o "${OUT_DIR}/api"
popd >/dev/null

echo "Done: ${OUT_DIR}"
