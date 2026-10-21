import { ConfigService } from '@nestjs/config';
import { NestFactory } from '@nestjs/core';
import { SwaggerModule } from '@nestjs/swagger';
import { AppModule } from './app.module.js';
import { createOpenApiDocument } from './common/openapi.js';
import type { Env } from './config/env.schema.js';
import { setupApp } from './setup-app.js';

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  setupApp(app);

  SwaggerModule.setup('api/docs', app, createOpenApiDocument(app));

  app.enableShutdownHooks();

  const config = app.get<ConfigService<Env, true>>(ConfigService);

  try {
    await app.listen(config.get('PORT', { infer: true }));
  } catch (error) {
    await app.close();
    throw error;
  }
}

await bootstrap();
