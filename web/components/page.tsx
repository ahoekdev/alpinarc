import type { ReactNode } from "react";
import styles from "./page.module.css";

/**
 * The outer frame of a page: spacing above and below its content. Put a
 * `Container` inside for content of a readable width, or leave it out for
 * content that spans the whole page.
 */
export function Page({ children }: { children: ReactNode }) {
  return <div className={styles.page}>{children}</div>;
}
