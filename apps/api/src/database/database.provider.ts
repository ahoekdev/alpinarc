import { ConfigService } from '@nestjs/config';
import { drizzle } from 'drizzle-orm/node-postgres';
import { Pool } from 'pg';
import type { Env } from '../config/env.js';
import * as schema from './schema.js';

export const DATABASE = 'DATABASE';

export const databaseProvider = {
  provide: DATABASE,
  inject: [ConfigService],
  useFactory: (config: ConfigService<Env, true>) => {
    const pool = new Pool({
      connectionString: config.get('DATABASE_URL', { infer: true }),
    });

    return drizzle(pool, { schema, casing: 'snake_case' });
  },
};

export type Database = ReturnType<typeof databaseProvider.useFactory>;
