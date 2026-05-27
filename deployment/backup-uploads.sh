#!/usr/bin/env sh
set -eu

UPLOADS_VOLUME="${UPLOADS_VOLUME:-regional-league-app-uploads}"
BACKUP_DIR="${BACKUP_DIR:-./backups/uploads}"
TIMESTAMP="$(date +%Y%m%d_%H%M%S)"
OUTPUT_FILE="regional_league_uploads_${TIMESTAMP}.tar.gz"

mkdir -p "$BACKUP_DIR"

docker run --rm \
  -v "${UPLOADS_VOLUME}:/data:ro" \
  -v "$(cd "$BACKUP_DIR" && pwd):/backup" \
  alpine:3.20 \
  tar -czf "/backup/${OUTPUT_FILE}" -C /data .

echo "Uploads backup created: ${BACKUP_DIR}/${OUTPUT_FILE}"
