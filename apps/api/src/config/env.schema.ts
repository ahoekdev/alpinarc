import { z } from 'zod';

export const databaseEnvSchema = z.object({
  DATABASE_URL: z.url({ protocol: /^postgres(ql)?$/ }),
});

export const envSchema = databaseEnvSchema.extend({
  PORT: z.coerce.number().int().positive().default(3000),
});

export type DatabaseEnv = z.infer<typeof databaseEnvSchema>;
export type Env = z.infer<typeof envSchema>;
