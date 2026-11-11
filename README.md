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

The home page shows whether the API and its database are healthy (`/health/live` and `/health/ready`), and lists the lodges with a form to add one.

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

Migrations live in `api/src/Alpinarc.Api/Data/Migrations/` and are applied explicitly with `just migrate`, never on API startup.

The API tests start their own throwaway Postgres container (Testcontainers), so Docker must be running for `just check` and `just check-api`.

Building the API writes its OpenAPI document to `api/openapi/alpinarc-api.json`. Orval reads that file and generates the typed client into `web/lib/api/generated/`.
