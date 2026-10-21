import { drizzle } from 'drizzle-orm/node-postgres';
import { Pool } from 'pg';
import { lodges } from './schema.js';

export const DATABASE = 'DATABASE';

export const databaseProvider = {
  provide: DATABASE,
  inject: [],
  useFactory: () => {
    const pool = new Pool({
      connectionString: process.env.DATABASE_URL,
    });

    return drizzle(pool, { schema: { lodges } });
  },
};

export type Database = ReturnType<typeof databaseProvider.useFactory>;
