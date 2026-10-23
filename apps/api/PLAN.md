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
   - Phase 8 proxies the web app's browser requests through Next.js, so the API may not need CORS at all.
6. **Phase 8: admin authentication**, after Phase 5, because it adds tables and tests that should not run against the dev database.

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
- [x] OpenAPI spec for the web app's generated client.
  - `@nestjs/swagger` builds the document. Zod schemas registered in `openApiRegistry` (`src/common/openapi.ts`) become named components via `z.toJSONSchema`, and controllers reference them with `schemaRef()`.
  - Swagger UI is served at `/api/docs` and the raw spec at `/api/docs-json`. With the API running, the web app's `npm run api:generate` turns the spec into a typed client with orval.

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

## Phase 8: Admin authentication

Goal: anonymous users can read lodges, and only admins can create or update them.
Admins sign in with an email and password and get a session cookie. No JWTs, no public sign-up.

Built on `@nestjs/authentication` and `@nestjs/authorization` ([docs](https://docs.nestjs.com/security/authentication)).
Both are 0.1.x and only weeks old, so pin exact versions and keep everything auth-related in `src/auth/`, `src/users/` and the policies.
Better Auth is the fallback if they prove unstable.

How the pieces fit:

- `AuthenticationModule` registers a global guard: every route requires a signed-in user unless it is `@Public()`.
- `AuthorizationModule` denies any handler that has no `@Can()`, `@Can.Anyone()` or `@Public()`, so a new endpoint is closed until it states who may call it.
- A session is a random 256-bit token in an `HttpOnly`, `Secure`, `SameSite=Lax` cookie (`__Host-sid`). Only its SHA-256 hash is stored, in a `sessions` table. Signing out deletes the row.
- Cookie-authenticated `POST`/`PATCH`/`DELETE` requests from another origin are ignored, and cross-origin sign-ins get a 403 (CSRF protection). Requests without `Origin` and `Sec-Fetch-Site` headers (curl, server-to-server) are not affected.
- Sessions need no secret in env. With `NODE_ENV=production` the module refuses to start until a database session store is registered.

### Step 1: Users and sessions

- [ ] Install `@nestjs/authentication` and `@nestjs/authorization` with exact versions. `@nestjs/websockets` and `@nestjs/microservices` are optional peers and not needed.
- [ ] Add a `users` table:
  - `id` (uuid);
  - `email` (`varchar(255)`, unique, stored lowercased);
  - `password_hash` (`text`);
  - `role` (`'admin'`, as a `pgEnum` so more roles can be added later);
  - `created_at` and `updated_at`.
- [ ] Add the `sessions` table from the docs' Drizzle schema (`id`, `user_id`, `created_at`, `expires_at`, `last_active_at`, `mfa`, `metadata`, plus the `user_id` and `expires_at` indexes).
  - Keep `user_id` as `text` without a foreign key: the store contract tests create sessions for random user ids. Delete a user's sessions in code when deleting the user.
- [ ] Generate the migration with `npm run db:generate -- --name add_users_and_sessions`.
- [ ] `UsersRepository` (`src/users/`) with `findById`, `findCredentials(email)` and `create`.
  - Only a `User` without the password hash leaves the repository, so a user can be returned from a route or logged safely.
  - Declare the `User` type once for the package with `declare module '@nestjs/authentication' { interface AuthenticationTypes { user: User } }`.

### Step 2: Sign-in and sign-out

- [ ] `DrizzleSessionStore` (`src/auth/`) implementing only `SessionStore`, injecting our existing `DATABASE` provider. Register it with `AuthenticationStorage.registerSource({ sessions: this })`.
  - Copy the session methods from the docs' `DrizzleAuthenticationStore`: `touchSession` and `deleteSession` must each be a single conditional statement.
  - Test it with `authenticationStoreContract(create, { contracts: ['sessions'], concurrent: true })` from `@nestjs/authentication/testing`.
- [ ] `SessionAuth extends SessionCookieProvider<User>`, which loads the user in `validate()`.
- [ ] `AuthenticationModule.forRoot({ session: { absoluteTtl, idleTtl } })` in `AppModule`, e.g. 7 days absolute and 1 day idle (the defaults).
- [ ] `AuthController`, validated with our `ZodValidationPipe` and documented in Swagger:
  - `POST /api/auth/sign-in` (`@Public()`): `CredentialsService.verify()` checks the password with `PasswordHasher` (scrypt, constant time even for unknown emails), then `SignInService.signIn(user.id, { method: 'password' })` sets the cookie. Wrong email or password: 401.
  - `POST /api/auth/sign-out`: `SignInService.signOut()`.
  - `GET /api/me`: `@CurrentUser()`, so the web app can tell whether an admin is signed in.
- [ ] Rate-limit sign-in with `@nestjs/throttler` (the docs' "Rate-limit sign-in" section).
- [ ] `npm run admin:create` script that creates an admin from an email and a password prompt, using `PasswordHasher` and `UsersRepository` in a Nest standalone application context (`NestFactory.createApplicationContext`), run from the build.

### Step 3: Lock down the lodge endpoints

- [ ] `AuthorizationModule` in `AppModule`, and a `LodgePolicy` (`src/lodges/lodge.policy.ts`) whose `create` and `update` return `user?.role === 'admin'`.
- [ ] `GET /api/lodges` and `GET /api/lodges/:id`: `@Public()`.
- [ ] `POST /api/lodges`: `@Can(LodgePolicy, 'create')`. Anonymous: 401. Signed in but not an admin: 403.
- [ ] Add `PATCH /api/lodges/:id` with `@Can(LodgePolicy, 'update')`, an `updateLodgeSchema` (`createLodgeSchema.partial()`, registered as `UpdateLodge`) and `LodgesService.update()` returning 404 for unknown ids.
- [ ] OpenAPI: add a cookie security scheme (`DocumentBuilder.addCookieAuth('__Host-sid')`), mark the write endpoints with it, and document their 401 and 403 responses.
- [ ] Add sign-in and lodge write requests to `requests/*.http`.

### Step 4: Tests

- [ ] e2e helper that creates an admin directly through `UsersRepository` and signs in through `POST /api/auth/sign-in`, returning the cookie. Override `PasswordHasher` with `new PasswordHasher({ logN: 10 })` so tests stay fast.
- [ ] Lodge e2e tests: anonymous `GET` is 200, anonymous `POST`/`PATCH` is 401, admin `POST`/`PATCH` succeeds.
- [ ] Auth e2e tests: wrong password is 401, sign-out ends the session, `GET /api/me` without a cookie is 401.
- [ ] Unit test `LodgePolicy`.

### Step 5: Web app

- [ ] Proxy `/api/:path*` to the API with `rewrites()` in `next.config.ts`, so the browser only talks to the web app's origin. The session cookie then belongs to that origin, and the API's cross-origin check passes without CORS or `session.trustedOrigins`.
- [ ] `/login` page that posts to `/api/auth/sign-in` through the proxy.
- [ ] Admin pages (create and edit lodge): server components and server actions forward the incoming `cookie` header to the API. Redirect to `/login` when `GET /api/me` returns 401.
- [ ] Public pages such as `/lodges` stay anonymous and unchanged.
- [ ] Regenerate the orval client.
- [ ] Read the Next.js 16 guides in `node_modules/next/dist/docs/` for rewrites, server actions and cookies before writing this (see `apps/web/AGENTS.md`).

### Later

- Password reset by email (`@nestjs/mail`, `email_tokens` table), two-factor authentication for admins, or more roles. Each is a module option plus one store interface.

## Later (when needed)

- [ ] Health endpoint with `@nestjs/terminus` (database ping) for Docker and deployment checks.
- [ ] `Dockerfile` for the API, added as a service in `docker-compose.yml`.
- [ ] Structured logging (e.g. `nestjs-pino`).
- [ ] Major upgrades, each as its own PR: Vitest 5, TypeScript 7.
- [ ] Rename the primary key index `lodge_pkey` to `lodges_pkey` (left over from the table rename) in a migration.
