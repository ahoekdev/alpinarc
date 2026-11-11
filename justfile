# Task runner for alpinarc. Run `just` to list the recipes.

# List available recipes
default:
    @just --list

# First-time setup: env files, npm packages, NuGet packages and .NET tools
setup:
    [ -f .env ] || cp .env.example .env
    [ -f web/.env.local ] || cp web/.env.example web/.env.local
    npm --prefix web install
    dotnet restore api/Alpinarc.slnx
    cd api && dotnet tool restore

# Start Postgres in the background
db:
    docker compose up -d

# Stop Postgres (data is kept in the named volume)
db-down:
    docker compose down

# Apply database migrations to the local database
[working-directory('api')]
migrate:
    dotnet ef database update --project src/Alpinarc.Api

# Create a new database migration, e.g. `just migration AddHuts`
[working-directory('api')]
migration NAME:
    dotnet ef migrations add {{NAME}} --project src/Alpinarc.Api --output-dir Data/Migrations

# Run the API on http://localhost:5033
api:
    dotnet run --project api/src/Alpinarc.Api

# Run the web app on http://localhost:3000
web:
    npm --prefix web run dev

# Regenerate the typed API client from the API's OpenAPI document
generate-api:
    dotnet build api/Alpinarc.slnx
    npm --prefix web run generate:api

# Format the API and web code
format:
    dotnet format api/Alpinarc.slnx
    npm --prefix web run format

# Run the same checks as CI
check: check-api check-web

# CI checks for the API: formatting, build and tests
[working-directory('api')]
check-api:
    dotnet format Alpinarc.slnx --verify-no-changes
    dotnet build Alpinarc.slnx
    dotnet test Alpinarc.slnx --no-build

# CI checks for the web app: lint, formatting, types and build
[working-directory('web')]
check-web:
    npm run generate:api
    npm run lint
    npm run format:check
    npm run typecheck
    npm run build
