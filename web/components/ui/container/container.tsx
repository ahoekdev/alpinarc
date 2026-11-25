import type { ReactNode } from "react";
import styles from "./container.module.css";

/**
 * Limits its content to a readable width and centres it. Leave it out for
 * content that should span the whole page.
 */
export function Container({ children }: { children: ReactNode }) {
  return <div className={styles.container}>{children}</div>;
}
