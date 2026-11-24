type ErrorStateProps = {
  /** Plural, lowercase name of what failed to load, e.g. `"lodges"`. */
  entityName: string;
};

/**
 * Shown when data could not be loaded. Uses `role="alert"`, so screen readers
 * announce it right away.
 *
 * @example
 * <ErrorState entityName="lodges" /> // Could not load lodges.
 */
export function ErrorState({ entityName }: ErrorStateProps) {
  return <p role="alert">Could not load {entityName}.</p>;
}
