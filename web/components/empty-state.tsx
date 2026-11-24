type EmptyStateProps = {
  /** Plural, lowercase name of what the list holds, e.g. `"lodges"`. */
  entityName: string;
};

/**
 * Shown when a list has loaded but has no items.
 *
 * @example
 * <EmptyState entityName="lodges" /> // No lodges yet.
 */
export function EmptyState({ entityName }: EmptyStateProps) {
  return <p>No {entityName} yet.</p>;
}
