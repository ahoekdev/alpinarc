import { existsSync } from 'node:fs';
import { defineConfig } from 'drizzle-kit';
import { validateDatabaseEnv } from './src/config/env.js';

const envFile = './.env.development.local';

// The file is optional so CI can provide the variables directly
if (existsSync(envFile)) {
  process.loadEnvFile(envFile);
}

const { DATABASE_URL } = validateDatabaseEnv(process.env);

export default defineConfig({
  dialect: 'postgresql',
  out: './drizzle',
  schema: ['./src/database/schema.ts'],
  casing: 'snake_case',
  dbCredentials: {
    url: DATABASE_URL,
  },
});
