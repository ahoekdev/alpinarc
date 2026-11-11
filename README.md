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

## Getting started

1. Start Postgres:

   ```bash
   cp .env.example .env
   docker compose up -d
   ```

2. Run the API (http://localhost:5033):

   ```bash
   cd api
   dotnet run --project src/Alpinarc.Api --launch-profile http
   ```

3. Run the web app (http://localhost:3000):

   ```bash
   cd web
   cp .env.example .env.local
   npm install
   npm run dev
   ```

The home page shows whether the web app can reach the API's `/health` endpoint.

## Common tasks

| Task | Command |
|------|---------|
| API tests | `cd api && dotnet test` |
| API formatting | `cd api && dotnet format` |
| Web lint / typecheck / format | `cd web && npm run lint`, `npm run typecheck`, `npm run format` |
| Regenerate API client | `cd api && dotnet build`, then `cd web && npm run generate:api` |

Building the API writes its OpenAPI document to `api/openapi/alpinarc-api.json`. Orval reads that file and generates the typed client into `web/lib/api/generated/`.

## Planning

See [issue #1](https://github.com/ahoekdev/alpinarc/issues/1) for the initialization plan.
