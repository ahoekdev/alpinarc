import { Test, TestingModule } from '@nestjs/testing';
import { INestApplication } from '@nestjs/common';
import request from 'supertest';
import { App } from 'supertest/types.js';
import { AppModule } from './../src/app.module.js';

// Requires a migrated database at DATABASE_URL (see docker-compose.yml)
describe('LodgesController (e2e)', () => {
  let app: INestApplication<App>;

  beforeEach(async () => {
    const moduleFixture: TestingModule = await Test.createTestingModule({
      imports: [AppModule],
    }).compile();

    app = moduleFixture.createNestApplication();
    await app.init();
  });

  it('/lodges (GET)', async () => {
    const response = await request(app.getHttpServer())
      .get('/lodges')
      .expect(200);

    expect(Array.isArray(response.body)).toBe(true);
  });

  afterEach(async () => {
    await app.close();
  });
});
