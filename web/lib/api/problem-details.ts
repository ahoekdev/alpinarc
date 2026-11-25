import type { ValidationProblemDetails } from "@/lib/api/__generated__/alpinarcApiV1.schemas";

/**
 * The API's message for one field, from an error the generated client threw.
 *
 * Validation errors (400) and conflicts such as a duplicate name (409) come back
 * as ValidationProblemDetails, with messages keyed by JSON property name. The
 * client puts the parsed body on `info`.
 */
export function getFieldError(
  error: unknown,
  field: string,
): string | undefined {
  if (!(error instanceof Error) || !("info" in error)) {
    return undefined;
  }

  const problem = error.info as ValidationProblemDetails | undefined;
  return problem?.errors?.[field]?.[0];
}
