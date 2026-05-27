# Deployment and Backups

This guide documents a simple backup strategy for a Docker/ZimaBoard deployment of Regional League App.

## Storage Recommendation for ZimaBoard

- SSD: active Docker data, PostgreSQL volume and app uploads.
- USB HDD: backup destination.

Example backup directory:

```bash
/mnt/usb-backups/regional-league-app
```

Keep this directory outside the Docker volumes that are being backed up.

## Environment Variables

The scripts do not include real credentials. Export the same values used by `.env` before running backups:

```bash
export POSTGRES_DB=regional_league
export POSTGRES_USER=regional_user
export POSTGRES_PASSWORD='replace_with_real_password'
export BACKUP_DIR=/mnt/usb-backups/regional-league-app/postgres
```

Optional overrides:

```bash
export POSTGRES_CONTAINER=regional-league-app-postgres
export UPLOADS_VOLUME=regional-league-app-uploads
```

## Manual PostgreSQL Backup

From the repository folder on the ZimaBoard:

```bash
chmod +x deployment/backup-postgres.sh
POSTGRES_DB=regional_league \
POSTGRES_USER=regional_user \
POSTGRES_PASSWORD='replace_with_real_password' \
BACKUP_DIR=/mnt/usb-backups/regional-league-app/postgres \
deployment/backup-postgres.sh
```

Equivalent raw command:

```bash
docker exec -e PGPASSWORD="$POSTGRES_PASSWORD" regional-league-app-postgres \
  pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -Fc \
  > /mnt/usb-backups/regional-league-app/postgres/regional_league_postgres_$(date +%Y%m%d_%H%M%S).dump
```

The backup uses PostgreSQL custom format (`-Fc`), which is suitable for `pg_restore`.

## PostgreSQL Restore

Warning: restore can replace existing database objects. Test restore on a disposable database/container before relying on backups.

```bash
chmod +x deployment/restore-postgres.sh
POSTGRES_DB=regional_league \
POSTGRES_USER=regional_user \
POSTGRES_PASSWORD='replace_with_real_password' \
deployment/restore-postgres.sh /mnt/usb-backups/regional-league-app/postgres/regional_league_postgres_YYYYMMDD_HHMMSS.dump
```

Equivalent raw command:

```bash
cat /mnt/usb-backups/regional-league-app/postgres/regional_league_postgres_YYYYMMDD_HHMMSS.dump | \
  docker exec -i -e PGPASSWORD="$POSTGRES_PASSWORD" regional-league-app-postgres \
  pg_restore --clean --if-exists --no-owner -U "$POSTGRES_USER" -d "$POSTGRES_DB"
```

## Uploads Backup

Uploads are stored in the Docker volume:

```text
regional-league-app-uploads
```

Run:

```bash
chmod +x deployment/backup-uploads.sh
BACKUP_DIR=/mnt/usb-backups/regional-league-app/uploads \
deployment/backup-uploads.sh
```

Equivalent raw command:

```bash
docker run --rm \
  -v regional-league-app-uploads:/data:ro \
  -v /mnt/usb-backups/regional-league-app/uploads:/backup \
  alpine:3.20 \
  tar -czf /backup/regional_league_uploads_$(date +%Y%m%d_%H%M%S).tar.gz -C /data .
```

## Uploads Restore Example

For a test restore into the existing uploads volume:

```bash
docker run --rm \
  -v regional-league-app-uploads:/data \
  -v /mnt/usb-backups/regional-league-app/uploads:/backup \
  alpine:3.20 \
  sh -c 'tar -xzf /backup/regional_league_uploads_YYYYMMDD_HHMMSS.tar.gz -C /data'
```

For production, stop the web container before restoring uploads to avoid writes during extraction:

```bash
docker compose stop web
# restore uploads
docker compose start web
```

## Suggested Manual Backup Routine

```bash
POSTGRES_DB=regional_league \
POSTGRES_USER=regional_user \
POSTGRES_PASSWORD='replace_with_real_password' \
BACKUP_DIR=/mnt/usb-backups/regional-league-app/postgres \
deployment/backup-postgres.sh

BACKUP_DIR=/mnt/usb-backups/regional-league-app/uploads \
deployment/backup-uploads.sh
```

## Important Restore Warning

A backup is only useful if restore works. Before depending on this strategy:

1. Create a backup.
2. Restore it into a disposable Docker/PostgreSQL instance.
3. Start the app against the restored database.
4. Confirm clubs, matches, events, standings, users and uploads are present.
