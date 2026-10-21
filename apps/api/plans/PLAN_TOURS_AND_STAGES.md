# Tours and stages plan

A tour is an ordered sequence of stages. A stage connects a start lodge to an end lodge and holds the distance and duration of that leg. Stages can be reused in more than one tour, so a join table links stages to tours and records each stage's position within the tour.

The work is split into phases. Each phase can land as its own PR.

## Decisions to make first

- [ ] **Timestamps.** `PLAN.md` (Phase 4) says every table gets `createdAt` and `updatedAt`. Add them to the new tables from the start, and preferably to `lodges` in the same migration.
- [ ] **Continuity.** Should consecutive stages have to connect (stage n's end lodge is stage n+1's start lodge)? Options: reject the request, return a warning, or allow it.
- [ ] **Stage direction.** This plan assumes A→B and B→A are different stages, because duration usually differs uphill and downhill. The alternative is bidirectional stages with a `reversed` flag on `tour_stages`.
- [ ] **Extra stage fields.** Elevation gain and loss, difficulty, or a name are cheaper to add now than to migrate later.

## Data model

```
lodges ◄── start_lodge_id ── stages ──► end_lodge_id ── lodges
                               ▲
                               │ stage_id
                          tour_stages (tour_id, position)
                               │ tour_id
                               ▼
                             tours
```

### `stages`

| column             | type                | notes                 |
| ------------------ | ------------------- | --------------------- |
| `id`               | uuid PK             | `defaultRandom()`     |
| `start_lodge_id`   | uuid FK → `lodges`  | `onDelete: 'restrict'` |
| `end_lodge_id`     | uuid FK → `lodges`  | `onDelete: 'restrict'` |
| `distance_meters`  | integer, not null   | check `> 0`           |
| `duration_minutes` | integer, not null   | check `> 0`           |

- Add a check constraint `start_lodge_id <> end_lodge_id`.
- Store integers in fixed units (meters, minutes). With `pg`, `numeric` comes back as a string and `interval` is awkward to serialize. The API or UI formats the values.
- Don't make `(start_lodge_id, end_lodge_id)` unique. Two lodges can be connected by more than one route (for example a glacier route and a valley route).
- Index both FK columns. Postgres doesn't index FKs automatically.

### `tours`

| column | type                    | notes             |
| ------ | ----------------------- | ----------------- |
| `id`   | uuid PK                 | `defaultRandom()` |
| `name` | varchar(255), not null  |                   |

### `tour_stages`

| column     | type               | notes                 |
| ---------- | ------------------ | --------------------- |
| `tour_id`  | uuid FK → `tours`  | `onDelete: 'cascade'` |
| `stage_id` | uuid FK → `stages` | `onDelete: 'restrict'` |
| `position` | integer, not null  | check `>= 0`          |

- The primary key is `(tour_id, position)`. It prevents duplicate positions within a tour and still allows the same stage to appear twice in one tour.
- The column is called `position` instead of `order`, because `order` is a reserved word in SQL. The API can still expose it as `order` if wanted.
- Index `stage_id`, for "which tours use this stage?" and to keep the `restrict` check fast.

### Delete behavior

- Deleting a tour removes its `tour_stages` rows (cascade).
- A stage that is used in a tour can't be deleted (restrict).
- A lodge that is used in a stage can't be deleted (restrict).

### Derived data

A tour's total distance, total duration, start lodge and end lodge are computed at query time, not stored.

## Ordering strategy

Reordering one row at a time would temporarily violate the `(tour_id, position)` primary key, and Drizzle can't declare `DEFERRABLE` constraints. Instead, the stage list is replaced as a whole:

- `PUT /tours/:id/stages` takes `{ stageIds: [...] }`.
- In one transaction, it deletes the tour's `tour_stages` rows and inserts the new list with `position = index`.
- Positions are always contiguous, and the client never handles position numbers.

Fine-grained endpoints (insert at a position, move, remove) are only worth adding if the UI really needs them.

## Validation

Done in the services, so clients get a clear 400 instead of a 500 from a raw FK violation (`23503`).

- **Stage:** a Zod refine for `startLodgeId !== endLodgeId`, and a check that both lodges exist.
- **Tour stages:** every `stageId` must exist.
- **Continuity:** depends on the decision above.

## API

### Stages

- `GET /stages`
- `GET /stages/:id`: includes the `startLodge` and `endLodge` objects (join `lodges` twice using Drizzle's `alias()`).
- `POST /stages`

### Tours

- `GET /tours`: a summary per tour (name, stage count, total distance and duration).
- `GET /tours/:id`: the tour with its ordered stages (each with its lodges) and the totals.
- `POST /tours`: `{ name, stageIds? }`
- `PUT /tours/:id/stages`: `{ stageIds: [...] }`

### Response schemas

The drizzle-zod select schemas cover the flat rows. The nested responses (`StageWithLodges`, `TourDetail`) need hand-composed Zod schemas registered in `openApiRegistry`, so orval generates named types for them.

## Phase 1: Schema and migration

- [ ] Add `stages`, `tours` and `tourStages` to `src/database/schema.ts`, with FKs, check constraints, indexes, and `$inferSelect` / `$inferInsert` types.
- [ ] Generate the migration with `npm run db:generate -- --name add_tours_and_stages`, and review the SQL.

## Phase 2: Stages module

- [ ] Create `src/stages/`, mirroring `src/lodges/`: `stages.schema.ts`, `stages.pipes.ts`, `stages.service.ts`, `stages.controller.ts`, `stages.controller.spec.ts` and `stages.module.ts`.
- [ ] Register `StagesModule` in `src/app.module.ts`.
- [ ] Add `requests/stages.http`.
- [ ] Add e2e tests.

## Phase 3: Tours module

- [ ] Create `src/tours/` with the same structure.
  - `replaceStages` uses `db.transaction()`.
  - `findOne` uses one query that joins `tour_stages` → `stages` → `lodges` (aliased twice), ordered by `position`, and groups the rows in TypeScript.
- [ ] Register `ToursModule` in `src/app.module.ts`.
- [ ] Add `requests/tours.http`.
- [ ] Add e2e tests.
- [ ] Regenerate the web client with `npm run api:generate` in `apps/web`.

## Later (when needed)

- [ ] `PATCH` and `DELETE` endpoints for stages and tours.
- [ ] `GET /lodges/:id/stages`.
- [ ] Fine-grained reorder endpoints, if the UI needs them.
