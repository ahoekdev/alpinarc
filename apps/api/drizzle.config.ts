import dotenv from 'dotenv';
import { defineConfig } from 'drizzle-kit';

dotenv.config({ path: './.env.development.local' });

const { DATABASE_URL } = process.env;

// TODO Remove this check once we have env validation in place
if (!DATABASE_URL) {
  throw new Error('No DATABASE_URL found in .env file');
}

export default defineConfig({
  dialect: 'postgresql',
  out: './drizzle',
  schema: ['./src/database/schema.ts'],
  dbCredentials: {
    url: DATABASE_URL,
  },
});
