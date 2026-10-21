import { ZodValidationPipe } from '../common/zod-validation.pipe.js';
import { createLodgeSchema } from './lodges.schema.js';

export const createLodgeValidationPipe = new ZodValidationPipe(
  createLodgeSchema,
);
