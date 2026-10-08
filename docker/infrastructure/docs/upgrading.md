# Upgrading MongoDB

The MongoDB service in `docker-compose.yml` uses the exact image tag `mongo:9.0.2` (the latest 9.0 patch as of 2026-10-08). Avoid floating tags so an image pull cannot silently change the server version.

The service stores its data in the named `mongo-data` volume mounted at `/data/db`. Recreating the container keeps that volume, so the server binary changes while the existing database files remain in place.

## Upgrade from 8.3

This procedure assumes the running server is MongoDB 8.3 and is a standalone instance, as configured here. MongoDB requires FCV 8.3 before upgrading to 9.0. If the server is older than 8.3, or its FCV is older than 8.2, stop and follow MongoDB's version-specific upgrade procedure first.

Run these commands from `docker/infrastructure` in PowerShell.

1. Check the running server and feature compatibility version:

   ```powershell
   docker compose exec mongodb mongosh --quiet --eval "db.version()"
   docker compose exec mongodb mongosh --quiet --eval "db.adminCommand({ getParameter: 1, featureCompatibilityVersion: 1 })"
   ```

   Confirm the server reports `8.3` and the FCV result contains `version: '8.3'`. If the FCV is `8.2`, do not start MongoDB 9.0 yet; use step 4 to advance FCV with an 8.3 binary. If the server is older than 8.3 or FCV is older than 8.2, follow the [MongoDB version-specific upgrade instructions](https://www.mongodb.com/docs/manual/release-notes/) first.

2. Check application and driver compatibility with MongoDB 9.0 and test the upgrade in a non-production environment first.

3. Stop MongoDB and make an offline copy of its data volume. This example writes the archive to the current user's temporary directory; verify it exists before continuing:

   ```powershell
   docker compose stop mongodb
   docker run --rm --volumes-from mongodb -v "$($env:TEMP):/backup" alpine sh -c "tar -czf /backup/mongodb-data-before-9.0.tgz -C /data/db ."
   Test-Path "$env:TEMP\mongodb-data-before-9.0.tgz"
   ```

   Keep this backup until the upgrade has been verified. Do not remove the `mongo-data` volume.

4. If FCV is `8.2`, start a temporary MongoDB 8.3 container using the existing compose container's data volume, advance FCV, verify it, and remove the temporary container:

   ```powershell
   docker run -d --name mongodb-fcv-upgrade --volumes-from mongodb mongo:8.3.11
   docker exec mongodb-fcv-upgrade mongosh --quiet --eval "db.adminCommand({ setFeatureCompatibilityVersion: '8.3', confirm: true })"
   docker exec mongodb-fcv-upgrade mongosh --quiet --eval "db.adminCommand({ getParameter: 1, featureCompatibilityVersion: 1 })"
   docker rm -f mongodb-fcv-upgrade
   ```

   Confirm the FCV result contains `version: '8.3'` before continuing. Skip this step if FCV was already `8.3`.

5. Pull and start the new image. Compose will recreate the container and reuse `mongo-data`:

   ```powershell
   docker compose pull mongodb
   docker compose up -d mongodb
   docker compose logs -f mongodb
   ```

   Wait for MongoDB to finish starting, then verify the server version and that FCV is still `8.3`:

   ```powershell
   docker compose exec mongodb mongosh --quiet --eval "db.version()"
   docker compose exec mongodb mongosh --quiet --eval "db.adminCommand({ getParameter: 1, featureCompatibilityVersion: 1 })"
   ```

6. After the application has passed its burn-in period on MongoDB 9.0, enable 9.0 features by advancing FCV:

   ```powershell
   docker compose exec mongodb mongosh --quiet --eval "db.adminCommand({ setFeatureCompatibilityVersion: '9.0', confirm: true })"
   ```

   Confirm the command succeeds and FCV reports `9.0`.

## Recovery and downgrade

Keep the data-volume archive and the previous image tag until verification is complete. Do not assume it is safe to switch back to `mongo:8.3` after upgrading: MongoDB supports downgrades only to the immediately previous release, and the required steps depend on FCV and whether 9.0-only features have persisted data. Follow the [MongoDB 9.0 downgrade procedure](https://www.mongodb.com/docs/manual/release-notes/9.0-downgrade/) or restore the pre-upgrade backup into a compatible MongoDB 8.3 instance. Never start an older MongoDB binary against a volume that may contain incompatible newer-version data.

## References

- [MongoDB 9.0 release notes](https://www.mongodb.com/docs/manual/release-notes/9.0/)
- [Upgrade a standalone from 8.3 to 9.0](https://www.mongodb.com/docs/manual/release-notes/9.0-upgrade-standalone/)
- [MongoDB feature compatibility version](https://www.mongodb.com/docs/manual/reference/command/setFeatureCompatibilityVersion/)
- [Official MongoDB Docker image tags](https://hub.docker.com/_/mongo/tags)