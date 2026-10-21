import { BadRequestException } from '@nestjs/common';
import { z } from 'zod';
import { ZodValidationPipe } from './zod-validation.pipe.js';

describe('ZodValidationPipe', () => {
  const pipe = new ZodValidationPipe(z.object({ name: z.string().trim() }));

  it('should return the parsed value for valid input', () => {
    expect(pipe.transform({ name: '  Lodge  ' })).toEqual({ name: 'Lodge' });
  });

  it('should throw a BadRequestException listing the invalid fields', () => {
    try {
      pipe.transform({ name: 42 });
      expect.unreachable();
    } catch (error) {
      expect(error).toBeInstanceOf(BadRequestException);
      expect((error as BadRequestException).getResponse()).toMatchObject({
        message: 'Validation failed',
        errors: [{ path: 'name' }],
      });
    }
  });
});
