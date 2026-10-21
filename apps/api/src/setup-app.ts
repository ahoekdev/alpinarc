import type { INestApplication } from '@nestjs/common';

// Shared by main.ts and the e2e tests so both run the same app configuration
export function setupApp(app: INestApplication) {
  app.setGlobalPrefix('api');
}
