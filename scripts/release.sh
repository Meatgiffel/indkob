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

echo "Publishing API (self-contained linux-x64)…"
pushd "${ROOT_DIR}/Api" >/dev/null
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false -o "${OUT_DIR}/api"
popd >/dev/null

echo "Done: ${OUT_DIR}"
