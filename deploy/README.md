# Production deployment

This folder contains the nginx templates and a non-secret environment template for a single-server deployment.

## Before the first deployment

1. Point `app`, `api`, `auth`, and `files` DNS records to the server IP.
2. Copy `deploy/.env.production.example` to `.env` beside `docker-compose.production.yml` on the server, then replace every placeholder with a unique secret and the actual image tags.
3. Replace every `example.com` value in the nginx templates with the selected domain.
4. Install the bootstrap nginx configuration, obtain certificates with Certbot, then replace it with the HTTPS configuration and reload nginx.

## Deploy or update

From the repository root on the server:

```bash
docker compose --env-file .env -f docker-compose.production.yml pull
docker compose --env-file .env -f docker-compose.production.yml up -d
docker compose --env-file .env -f docker-compose.production.yml ps
```

The `migrations` container runs database migrations before the API starts. Keep Docker volumes between releases: they store PostgreSQL, Keycloak, MinIO, and Seq data.

## Verification

```bash
curl --fail https://api.example.com/health/live
curl --fail https://api.example.com/health/ready
```

Open `https://app.example.com`, sign in through `https://auth.example.com`, upload an image, and confirm that its URL uses `https://files.example.com`.

## Important notes

- Nginx is the TLS endpoint. Containers are bound only to `127.0.0.1`; do not expose their ports publicly.
- Keycloak imports the realm only when its database is empty. Update an existing realm in the Keycloak admin console or through the Admin API; changing the JSON file alone does not update it.
- Back up Docker volumes before upgrades that modify PostgreSQL or Keycloak versions.
