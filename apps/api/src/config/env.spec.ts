import { validateDatabaseEnv, validateEnv } from './env.js';

describe('validateEnv', () => {
  const DATABASE_URL = 'postgresql://user:password@localhost:5432/db';

  it('should return the validated config with defaults applied', () => {
    expect(validateEnv({ DATABASE_URL })).toEqual({ DATABASE_URL, PORT: 3000 });
  });

  it('should coerce PORT to a number', () => {
    expect(validateEnv({ DATABASE_URL, PORT: '4000' }).PORT).toBe(4000);
  });

  it('should throw when DATABASE_URL is missing', () => {
    expect(() => validateEnv({})).toThrow(/DATABASE_URL/);
  });

  it('should throw when DATABASE_URL is not a postgres URL', () => {
    expect(() => validateEnv({ DATABASE_URL: 'mysql://localhost/db' })).toThrow(
      /DATABASE_URL/,
    );
  });
});

describe('validateDatabaseEnv', () => {
  it('should only require and return DATABASE_URL', () => {
    const DATABASE_URL = 'postgresql://user:password@localhost:5432/db';

    expect(validateDatabaseEnv({ DATABASE_URL, PORT: 'not-a-port' })).toEqual({
      DATABASE_URL,
    });
  });
});
