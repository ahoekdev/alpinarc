import { createInsertSchema } from 'drizzle-zod';
import type { z } from 'zod';
import { lodges } from '../database/schema.js';

export const createLodgeSchema = createInsertSchema(lodges, {
  name: (schema) => schema.trim().min(1),
}).omit({ id: true });

export type CreateLodgeDto = z.infer<typeof createLodgeSchema>;
