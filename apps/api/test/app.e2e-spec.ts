import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication } from '@nestjs/common';
import request from 'supertest';
import { App } from 'supertest/types.js';
import { AppModule } from './../src/app.module.js';
import { setupApp } from './../src/setup-app.js';

// Requires a migrated database at DATABASE_URL (see docker-compose.yml)
describe('LodgesController (e2e)', () => {
  let app: INestApplication<App>;

  beforeEach(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    }).compile();

    app = moduleFixture.createNestApplication();
    setupApp(app);
    await app.init();
  });

  it('/api/lodges (GET)', async () => {
    const response = await request(app.getHttpServer())
      .get('/api/lodges')
      .expect(200);

    expect(Array.isArray(response.body)).toBe(true);
  });

  afterEach(async () => {
    await app.close();
  });
});
