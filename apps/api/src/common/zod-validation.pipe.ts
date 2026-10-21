import { BadRequestException, type PipeTransform } from '@nestjs/common';
import type { z } from 'zod';

// Schema-based validation as described in the NestJS docs (Pipes chapter):
// bind per parameter with `@Body(new ZodValidationPipe(schema))`
export class ZodValidationPipe implements PipeTransform {
  constructor(private readonly schema: z.ZodType) {}

  transform(value: unknown) {
    const result = this.schema.safeParse(value);

    if (!result.success) {
      throw new BadRequestException({
        statusCode: 400,
        error: 'Bad Request',
        message: 'Validation failed',
        errors: result.error.issues.map((issue) => ({
          path: issue.path.join('.'),
          message: issue.message,
        })),
      });
    }

    return result.data;
  }
}
