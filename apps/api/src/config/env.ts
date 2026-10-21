import { z } from 'zod';
import {
  type DatabaseEnv,
  databaseEnvSchema,
  type Env,
  envSchema,
} from './env.schema.js';

function parseEnv<T extends z.ZodType>(
  schema: T,
  config: Record<string, unknown>,
): z.infer<T> {
  const result = schema.safeParse(config);

  if (!result.success) {
    throw new Error(
      `Invalid environment variables:\n${z.prettifyError(result.error)}`,
    );
  }

  return result.data;
}

export function validateEnv(config: Record<string, unknown>): Env {
  return parseEnv(envSchema, config);
}

export function validateDatabaseEnv(
  config: Record<string, unknown>,
): DatabaseEnv {
  return parseEnv(databaseEnvSchema, config);
}
