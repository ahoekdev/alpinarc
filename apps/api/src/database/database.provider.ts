import { Logger } from '@nestjs/common';
import { ConfigService } from '@nestjs/config';
import { drizzle } from 'drizzle-orm/node-postgres';
import { Pool } from 'pg';
import type { Env } from '../config/env.schema.js';
import * as schema from './schema.js';

export const DATABASE = Symbol('DATABASE');

const logger = new Logger('Database');

export const databaseProvider = {
  provide: DATABASE,
  inject: [ConfigService],
  useFactory: (config: ConfigService<Env, true>) => {
    const pool = new Pool({
      connectionString: config.get('DATABASE_URL', { infer: true }),
    });

    pool.on('error', (err) => {
      logger.error(`Idle pool client error: ${err.message}`, err.stack);
    });

    return drizzle(pool, { schema, casing: 'snake_case' });
  },
};

export type Database = ReturnType<typeof databaseProvider.useFactory>;
