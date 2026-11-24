type LoadingStateProps = {
  /** Plural, lowercase name of what is loading, e.g. `"lodges"`. */
  entityName: string;
};

/**
 * Shown while data loads. Uses `role="status"`, so screen readers announce it.
 *
 * @example
 * <LoadingState entityName="lodges" /> // Loading lodges…
 */
export function LoadingState({ entityName }: LoadingStateProps) {
  return <p role="status">Loading {entityName}…</p>;
}
