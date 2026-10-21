import { createInsertSchema, createSelectSchema } from 'drizzle-zod';
import type { z } from 'zod';
import { openApiRegistry } from '../common/openapi.js';
import { lodges } from '../database/schema.js';

export const lodgeSchema = createSelectSchema(lodges).register(
  openApiRegistry,
  { id: 'Lodge' },
);

export const createLodgeSchema = createInsertSchema(lodges, {
  name: (schema) => schema.trim().min(1),
})
  .omit({ id: true })
  .register(openApiRegistry, { id: 'CreateLodge' });

export type CreateLodgeDto = z.infer<typeof createLodgeSchema>;
