import styles from "./state.module.css";

type ErrorStateProps = {
  /** Plural, lowercase name of what failed to load, e.g. `"lodges"`. */
  entityName: string;
};

/**
 * Shown when data could not be loaded. Uses `role="alert"`, so screen readers
 * announce it right away.
 */
export function ErrorState({ entityName }: ErrorStateProps) {
  return (
    <p role="alert" className={styles.error}>
      Could not load {entityName}.
    </p>
  );
}
