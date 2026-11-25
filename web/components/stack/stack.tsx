import type { ReactNode } from "react";
import styles from "./stack.module.css";

/** Stacks its children vertically, with space between them. */
export function Stack({ children }: { children: ReactNode }) {
  return <div className={styles.stack}>{children}</div>;
}
