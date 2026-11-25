# alpinarc

A web application with a Next.js frontend and a .NET backend.

## Structure

| Folder | Contents |
|--------|----------|
| `web/` | Next.js app (TypeScript, App Router, CSS Modules) |
| `api/` | ASP.NET Core Web API (.NET 10, controllers) |

Local services such as Postgres run through Docker Compose.

## Prerequisites

- .NET SDK 10 (pinned in `api/global.json`)
- Node.js 26 and npm
- Docker
- [just](https://github.com/casey/just) (command runner)

## Getting started

Run these from the repo root:

```bash
just setup     # create .env files, install npm, NuGet packages and .NET tools
just db        # start Postgres
just migrate   # apply database migrations
```

Then run each app in its own terminal:

```bash
just api   # http://localhost:5033
just web   # http://localhost:3000
```

The lodges page (http://localhost:3000/lodges) lists the lodges and has a form to add one. The stages page (http://localhost:3000/stages) does the same for stages, the hikes between two lodges. The tours page (http://localhost:3000/tours) lists the tours, which are ordered lists of stages, and links to each tour's stages and totals. The web app doesn't show health; check it on the API at `/health/live` and `/health/ready`.

## Common tasks

Run `just` to list every recipe.

| Task | Command |
|------|---------|
| Run all CI checks | `just check` (or `just check-api`, `just check-web`) |
| Format API and web code | `just format` |
| Regenerate API client | `just generate-api` |
| Stop Postgres | `just db-down` |
| Apply database migrations | `just migrate` |
| Create a database migration | `just migration NAME` |

Migrations live in `api/src/Alpinarc.Api/Data/Migrations/` and are applied explicitly with `just migrate`, never on API startup. In production they run as Railway's pre-deploy command (see [Migrations](#migrations) under Deployment).

The API tests start their own throwaway Postgres container (Testcontainers), so Docker must be running for `just check` and `just check-api`.

Building the API writes its OpenAPI document to `api/openapi/alpinarc-api.json`. Orval reads that file and generates the typed client into `web/lib/api/__generated__/`.

## Deployment

alpinarc runs on [Railway](https://railway.com): one project, `AlpinArc`, with a single `production` environment in EU West (Amsterdam). Every push to `main` deploys automatically once CI has passed.

The repo has no Railway config file, so the settings below are the record of how the project is set up. Change them in Railway and update this section to match.

### Services

| Service | Source | Root directory | Dockerfile path | Watch paths | Pre-deploy command | Healthcheck | Restart |
|---------|--------|----------------|-----------------|-------------|--------------------|-------------|---------|
| `Postgres` | Railway's `postgres` template, image `ghcr.io/railwayapp-templates/postgres-ssl:18` | n/a | n/a | n/a | n/a | n/a | n/a |
| `api` | `ahoekdev/alpinarc`, branch `main` | `/api` | `Dockerfile` | `/api/**` | `/app/efbundle`, timeout 300 s | `/health/ready` | on failure |
| `web` | `ahoekdev/alpinarc`, branch `main` | `/` | `web/Dockerfile` | `/web/**`, `/api/openapi/**` | none | `/` | on failure |

- Postgres must stay on version 18, because the tables use `DEFAULT uuidv7()`. Don't switch the image to `:latest`.
- `Postgres` has no public domain or TCP proxy and is only reachable over Railway's private network. `api` (target port 8080) and `web` (target port 3000) have the default `*.up.railway.app` domains.
- All three services run in EU West (`europe-west4`).
- Railway only calls the healthcheck while a deploy rolls out, to decide whether to switch traffic to the new version. With `/health/ready`, an API version that can't reach the database never goes live.
- **Wait for CI** is on for `api` and `web` (in the service settings in the Railway dashboard).

### Variables

| Service | Variable | Value |
|---------|----------|-------|
| `api` | `ConnectionStrings__Alpinarc` | `Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}};Gss Encryption Mode=Disable` |
| `api` | `Cors__AllowedOrigins__0` | `https://${{web.RAILWAY_PUBLIC_DOMAIN}}` |
| `api` | `ASPNETCORE_ENVIRONMENT` | `Production` |
| `api` | `PORT` | `8080` |
| `web` | `NEXT_PUBLIC_API_URL` | `https://${{api.RAILWAY_PUBLIC_DOMAIN}}` |
| `web` | `PORT` | `3000` |

- The values are Railway reference variables, so the services stay linked without copying values by hand. The database password only exists in Railway.
- `ConnectionStrings__Alpinarc` uses Npgsql's key-value format, because Npgsql doesn't accept the `postgresql://` URL in `DATABASE_URL`. `Gss Encryption Mode=Disable` stops Npgsql from trying Kerberos encryption, which the API image has no library for (it logs a harmless `libgssapi_krb5.so.2` error otherwise).
- `NEXT_PUBLIC_API_URL` is fixed into the client bundle at build time. After changing it, `web` needs a new build (redeploy), not a restart.
- Both images listen on `PORT`. It's set explicitly to match each domain's target port, rather than relying on Railway's port detection.
- The domains have to exist before `web` first builds: otherwise `${{api.RAILWAY_PUBLIC_DOMAIN}}` is still empty and the build fixes `https://` into the app.

### Migrations

- The API image contains an EF Core migration bundle, `/app/efbundle`. It runs as the `api` service's pre-deploy command, before the new version starts, with the service's variables and on the private network. Migrations never run on API startup.
- If the bundle fails, or is still running after the 300 s timeout (for example while it waits for the migration lock), the deploy stops and the previous version keeps running.
- During a deploy the previous version is still serving traffic against the migrated database. Every migration therefore has to work with both versions: expand first (add a column or table), and contract (drop the old one) in a later deploy.

### Deploy flow

- Railway deploys a push to `main` only after the CI workflow has succeeded. If CI fails, or hasn't finished within two hours, the deploy is skipped.
- Watch paths decide which services redeploy. A web-only change redeploys only `web`. A change under `api/` redeploys `api`, and also `web` when it touches `api/openapi/`, because the generated client may have changed.
- An empty commit doesn't trigger a deploy. To redeploy without a change, use **Deploy Latest Commit** on the service in Railway.

### Rolling back

Redeploy an earlier deployment from the service's Deployments list in Railway. That doesn't undo migrations: to revert one, add a new migration that reverses it.

### Building the images locally

```bash
docker build -t alpinarc-api api
docker build -f web/Dockerfile --build-arg NEXT_PUBLIC_API_URL=http://localhost:8080 -t alpinarc-web .
```

To run the API image, pass `ConnectionStrings__Alpinarc` and `Cors__AllowedOrigins__0` with `-e`. The API listens on port 8080 and the web app on port 3000. To apply the migrations from the image, run `/app/efbundle` as the container's command.
