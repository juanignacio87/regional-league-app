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

## Automatic Updates from GHCR with Watchtower

The production `docker-compose.yml` uses the image published by GitHub Actions:

```text
ghcr.io/juanignacio87/regional-league-app-web:latest
```

The `web` service is labeled for Watchtower updates, while `postgres` is explicitly excluded. This keeps PostgreSQL data persistent in the Docker volume and avoids replacing the database container during app image updates.

The compose file remains compatible with Portainer: deploy the stack from `docker-compose.yml`, configure the same `.env` values, and Portainer will pull the GHCR image instead of building it locally.

### Install Watchtower Manually

Do not install Watchtower from this repository automatically. On the ZimaBoard, install it only when you are ready to enable automatic updates:

```bash
docker run -d \
  --name watchtower \
  --restart unless-stopped \
  -v /var/run/docker.sock:/var/run/docker.sock \
  containrrr/watchtower \
  --label-enable \
  --cleanup \
  --interval 300
```

With `--label-enable`, Watchtower only updates containers that have:

```yaml
com.centurylinklabs.watchtower.enable: "true"
```

In this stack, that means only `regional-league-app-web`.

If the GHCR package is private, log in on the ZimaBoard before starting or restarting the stack:

```bash
echo "$GHCR_TOKEN" | docker login ghcr.io -u juanignacio87 --password-stdin
```

Use a GitHub token with package read permissions.

### How Auto-Update Works

1. GitHub Actions builds and publishes `ghcr.io/juanignacio87/regional-league-app-web:latest`.
2. Watchtower checks Docker Hub/GHCR on the configured interval.
3. When `latest` points to a newer image, Watchtower pulls it.
4. Watchtower recreates only the labeled `web` container.
5. The existing PostgreSQL and uploads volumes remain mounted:

```text
regional-league-app-postgres -> /var/lib/postgresql/data
regional-league-app-uploads  -> /app/wwwroot/uploads
```

### Auto-Update Risks

- A bad image pushed to `latest` can be deployed automatically.
- Database migrations or incompatible schema changes can break startup if they are not planned carefully.
- The app container restarts during the update, causing a short interruption.
- If GHCR authentication expires or the package is private and Docker is not logged in, updates will fail.

Before relying on auto-update, keep tested PostgreSQL and uploads backups available.

### Rollback

If the latest image fails, rollback to a known-good image tag. Prefer immutable version tags when the workflow publishes them. If only `latest` exists, use the previous image ID from Docker if it is still present locally.

Show recent images:

```bash
docker images ghcr.io/juanignacio87/regional-league-app-web
```

Temporarily pin the `web` service in `docker-compose.yml` to a known-good tag or digest:

```yaml
image: ghcr.io/juanignacio87/regional-league-app-web:<known-good-tag>
```

Then redeploy:

```bash
docker compose pull web
docker compose up -d web
```

If Watchtower is running, either keep the rollback tag pinned until a fixed release is available or pause Watchtower:

```bash
docker stop watchtower
```

After publishing a fixed image, restore the `latest` image reference and redeploy:

```bash
docker compose pull web
docker compose up -d web
docker start watchtower
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
