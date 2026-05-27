#!/usr/bin/env sh
set -eu

CONTAINER_NAME="${POSTGRES_CONTAINER:-regional-league-app-postgres}"
BACKUP_DIR="${BACKUP_DIR:-./backups/postgres}"
POSTGRES_DB="${POSTGRES_DB:?Set POSTGRES_DB}"
POSTGRES_USER="${POSTGRES_USER:?Set POSTGRES_USER}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:?Set POSTGRES_PASSWORD}"
TIMESTAMP="$(date +%Y%m%d_%H%M%S)"
OUTPUT_FILE="${BACKUP_DIR}/regional_league_postgres_${TIMESTAMP}.dump"

mkdir -p "$BACKUP_DIR"

docker exec \
  -e PGPASSWORD="$POSTGRES_PASSWORD" \
  "$CONTAINER_NAME" \
  pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -Fc > "$OUTPUT_FILE"

echo "PostgreSQL backup created: $OUTPUT_FILE"
