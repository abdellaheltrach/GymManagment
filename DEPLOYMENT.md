# Deployment Guide

## Local Development

1. Setup environment variables:
   Copy `.env.example` to `.env` and fill in the missing passwords.

   ```bash
   cp .env.example .env
   ```

2. Generate a local developer overrides file:

   ```bash
   cp docker-compose.override.yml.example docker-compose.override.yml
   ```

3. Start the application:

   ```bash
   make up
   ```

4. Wait an additional 30 seconds for the database to complete initialization, then monitor logs:

   ```bash
   make logs-app
   ```

5. The application will automatically run migrations and seed the database on startup in the `Development` environment.
   Monitor the logs to ensure everything is ready:

   ```bash
   make logs-app
   ```

6. The API should now be running. You can verify the healthcheck:
   ```bash
   curl http://localhost:8080/health/ready
   ```

## Production Deployment via GitHub Actions

The `cd.yml` file is configured to push an image to the GitHub Container Registry (GHCR) and deploy remotely to your production environment over SSH whenever code merges into `main`.

Required GitHub Secrets (Under Repository Settings -> Secrets -> Actions):

- `PROD_CONNECTION_STRING`: Your production database SQL Server connection string
- `PROD_HOST`: Production server hostname or IP address
- `PROD_SSH_USER`: SSH username (e.g. `ubuntu`)
- `PROD_SSH_KEY`: Private SSH Key contents
