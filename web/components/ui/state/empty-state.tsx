import styles from "./state.module.css";

type EmptyStateProps = {
  /** Plural, lowercase name of what the list holds, e.g. `"lodges"`. */
  entityName: string;
};

/** Shown when a list has loaded but has no items. */
export function EmptyState({ entityName }: EmptyStateProps) {
  return <p className={styles.muted}>No {entityName} yet.</p>;
}
