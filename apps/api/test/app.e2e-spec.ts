import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication } from '@nestjs/common';
import { inArray } from 'drizzle-orm';
import request from 'supertest';
import { App } from 'supertest/types.js';
import { AppModule } from './../src/app.module.js';
import {
  type Database,
  DATABASE,
} from './../src/database/database.provider.js';
import { lodges } from './../src/database/schema.js';
import { setupApp } from './../src/setup-app.js';

// Requires a migrated database at DATABASE_URL (see docker-compose.yml)
describe('LodgesController (e2e)', () => {
  let app: INestApplication<App>;
  const createdIds: string[] = [];

  beforeEach(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    }).compile();

    app = moduleFixture.createNestApplication();
    setupApp(app);
    await app.init();
  });

  async function createLodge(name: string) {
    const response = await request(app.getHttpServer())
      .post('/api/lodges')
      .send({ name })
      .expect(201);

    createdIds.push(response.body.id);
    return response.body;
  }

  it('/api/lodges (GET)', async () => {
    const response = await request(app.getHttpServer())
      .get('/api/lodges')
      .expect(200);

    expect(Array.isArray(response.body)).toBe(true);
  });

  it('/api/lodges (POST) creates a lodge', async () => {
    const lodge = await createLodge('  E2E Lodge  ');

    expect(lodge).toEqual({ id: expect.any(String), name: 'E2E Lodge' });
  });

  it('/api/lodges (POST) rejects an invalid body', async () => {
    const response = await request(app.getHttpServer())
      .post('/api/lodges')
      .send({ name: '' })
      .expect(400);

    expect(response.body).toMatchObject({
      message: 'Validation failed',
      errors: [{ path: 'name' }],
    });
  });

  it('/api/lodges (POST) rejects a name longer than 255 characters', async () => {
    const response = await request(app.getHttpServer())
      .post('/api/lodges')
      .send({ name: 'a'.repeat(256) })
      .expect(400);

    expect(response.body).toMatchObject({ errors: [{ path: 'name' }] });
  });

  it('/api/lodges/:id (GET) returns the lodge', async () => {
    const lodge = await createLodge('E2E Lodge');

    await request(app.getHttpServer())
      .get(`/api/lodges/${lodge.id}`)
      .expect(200, lodge);
  });

  it('/api/lodges/:id (GET) returns 404 for an unknown id', async () => {
    await request(app.getHttpServer())
      .get('/api/lodges/00000000-0000-4000-8000-000000000000')
      .expect(404);
  });

  it('/api/lodges/:id (GET) returns 400 for a malformed id', async () => {
    await request(app.getHttpServer())
      .get('/api/lodges/not-a-uuid')
      .expect(400);
  });

  afterEach(async () => {
    // Remove test data until the e2e tests have their own database
    if (createdIds.length > 0) {
      await app
        .get<Database>(DATABASE)
        .delete(lodges)
        .where(inArray(lodges.id, createdIds.splice(0)));
    }

    await app.close();
  });
});
