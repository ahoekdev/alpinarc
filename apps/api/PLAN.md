# API improvement plan

Improvements for `apps/api`, grouped into phases. Each phase can land as its own PR.
Within a phase, the items are ordered so that earlier ones unblock later ones.

## Recommended order

Phases 1 and 2 are done, and Phase 3 is done except for CORS. Next:

1. **Commit the completed work** in a few focused commits: dependency cleanup, the `/api` prefix and `setupApp()`, and the lodge endpoints with validation (including the migration).
2. **Phase 5: separate test database.** This is the most urgent item, because the e2e tests now write to the dev database (see Phase 5).
3. **Phase 4: timestamps.** This is cheap while `lodges` has almost no data. Doing it after Phase 5 means the migration also gets tested on the test database.
4. **Phase 6: CI**, running lint, build, unit tests and e2e tests against a Postgres service container.
5. **CORS (Phase 3)** once the web app starts calling the API, so the configuration can be tested against a real client.

Update the README (Phase 7) along the way, as the setup steps change.

## Phase 1: Configuration and database foundation

These changes shape all code written afterwards, so they come first.

- [x] **Env validation**
  - Add a Zod schema (e.g. `src/config/env.ts`) and pass it to `ConfigModule.forRoot({ validate })`.
  - Validate `DATABASE_URL` (required URL) and `PORT` (optional number, default 3000).
  - Export a typed `Env` type for use with `ConfigService<Env, true>`.
- [x] **`.env.example`**
  - Commit a `.env.example` listing every variable with placeholder values.
  - Keep `.env.development.local` gitignored.
- [x] **Drizzle config env loading**
  - Replace `dotenv` in `drizzle.config.ts` with Node's built-in `process.loadEnvFile()`.
  - Reuse the env schema there and remove the TODO check.
- [x] **Database provider**
  - Inject `ConfigService` instead of reading `process.env` directly.
  - Change the `DATABASE` token from a string to a `Symbol`.
  - Close the `pg` pool on shutdown with an `onApplicationShutdown` hook in `DatabaseModule`.
  - Call `app.enableShutdownHooks()` in `main.ts`.

## Phase 2: Dependency cleanup

- [x] Remove unused devDependencies: `tsx` and `source-map-support`.
- [x] Remove `dotenv` once Phase 1 has replaced it.
- [x] Remove the `deploy` script and `@nestjs/mau`, unless deploying with Mau is planned.
- [x] Pin the Node version:
  - Add `"engines": { "node": ">=26" }` and an `.nvmrc`.
  - Align `@types/node` with that version.
- [x] Drop the `db:push` script and standardize on `db:generate` + `db:migrate`.

## Phase 3: HTTP bootstrap

Do this before adding any write endpoints.

- [x] Add global request validation with Zod.
  - `ZodValidationPipe` (`src/common/`) follows the NestJS docs' schema-based validation: bind it per parameter with `@Body(new ZodValidationPipe(schema))`. It returns 400 with per-field errors.
  - Request schemas are derived from Drizzle tables with `drizzle-zod`, and DTOs are `z.infer` types (e.g. `src/lodges/lodges.schema.ts`).
  - Each feature creates its pipe instances once in `<feature>.pipes.ts` (e.g. `createLodgeValidationPipe`), so controllers use `@Body(createLodgeValidationPipe)`.
  - Use `ParseUUIDPipe` for `:id` params.
- [ ] Enable CORS for the web app's origin, configured via env (e.g. `CORS_ORIGIN`).
  - Postponed until the web app calls the API. Add it in `setupApp()`.
- [x] Decide on a global prefix (`/api`), and update `requests/*.http` and the e2e tests to match.

## Phase 4: Schema conventions

Do this after Phase 5, so the migration also runs against the test database.

- [ ] Add `createdAt` and `updatedAt` to `lodges` using `timestamp({ withTimezone: true }).defaultNow().notNull()`.
  - `updatedAt` can use `.$onUpdate(() => new Date())`.
- [x] Give `name` an explicit type: `varchar({ length: 255 })` or `text()`.
- [ ] Generate the timestamps migration with `npm run db:generate -- --name <description>`.
- [ ] Write down the conventions (plural snake_case tables, timestamps on every table) in the README.

## Phase 5: Testing

Highest priority. The e2e tests now create lodges in the dev database and delete them again in `afterEach`. If a test fails before that cleanup runs, the test rows stay in the dev data.

- [ ] **Separate test database**
  - Add a `.env.test.local` and a second database in `docker-compose.yml` (or an init script that creates `alpinarc_test`).
  - Make `test:e2e` load the test env and run migrations before the suite.
  - Truncate tables between tests, and remove the `createdIds` cleanup from `test/app.e2e-spec.ts`.
- [ ] In `test/app.e2e-spec.ts`, switch `beforeEach`/`afterEach` to `beforeAll`/`afterAll`.
- [ ] Decide how to test services. Recommended: against the real test database, truncating tables between tests.
- [ ] Add a `LodgesService` test using that approach.

## Phase 6: Tooling and CI

- [ ] Change the `format` script to `prettier --write .`, add a `.prettierignore`, and add a `format:check` script.
- [ ] Re-enable `typescript/no-explicit-any` in `.oxlintrc.json`.
- [ ] Add a GitHub Actions workflow (`.github/workflows/api.yml`) that runs install, lint, `format:check`, build and unit tests on every PR.
  - Add a Postgres service container, run migrations, and run the e2e tests. This depends on the Phase 5 test database setup.

## Phase 7: Documentation

Update this incrementally alongside the other phases instead of saving it for the end.

- [ ] Replace the boilerplate `README.md` with:
  - prerequisites;
  - `docker compose up -d`;
  - env setup (copy `.env.example`);
  - migrations;
  - dev, test and lint commands;
  - project conventions.

## Later (when needed)

- [ ] Health endpoint with `@nestjs/terminus` (database ping) for Docker and deployment checks.
- [ ] `Dockerfile` for the API, added as a service in `docker-compose.yml`.
- [ ] Structured logging (e.g. `nestjs-pino`).
- [ ] Major upgrades, each as its own PR: Vitest 5, TypeScript 7.
- [ ] Rename the primary key index `lodge_pkey` to `lodges_pkey` (left over from the table rename) in a migration.
