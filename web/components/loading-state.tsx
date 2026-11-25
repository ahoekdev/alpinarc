import styles from "./state.module.css";

type LoadingStateProps = {
  /** Plural, lowercase name of what is loading, e.g. `"lodges"`. */
  entityName: string;
};

/** Shown while data loads. Uses `role="status"`, so screen readers announce it. */
export function LoadingState({ entityName }: LoadingStateProps) {
  return (
    <p role="status" className={styles.muted}>
      Loading {entityName}…
    </p>
  );
}
