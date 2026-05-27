#!/usr/bin/env sh
set -eu

if [ "$#" -ne 1 ]; then
  echo "Usage: POSTGRES_DB=... POSTGRES_USER=... POSTGRES_PASSWORD=... $0 /path/to/backup.dump"
  exit 1
fi

CONTAINER_NAME="${POSTGRES_CONTAINER:-regional-league-app-postgres}"
BACKUP_FILE="$1"
POSTGRES_DB="${POSTGRES_DB:?Set POSTGRES_DB}"
POSTGRES_USER="${POSTGRES_USER:?Set POSTGRES_USER}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:?Set POSTGRES_PASSWORD}"

if [ ! -f "$BACKUP_FILE" ]; then
  echo "Backup file not found: $BACKUP_FILE"
  exit 1
fi

echo "Restoring '$BACKUP_FILE' into database '$POSTGRES_DB' on container '$CONTAINER_NAME'."
echo "This may replace existing database objects. Make sure you tested this process before relying on it."

cat "$BACKUP_FILE" | docker exec -i \
  -e PGPASSWORD="$POSTGRES_PASSWORD" \
  "$CONTAINER_NAME" \
  pg_restore --clean --if-exists --no-owner -U "$POSTGRES_USER" -d "$POSTGRES_DB"

echo "PostgreSQL restore completed."
