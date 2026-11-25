import type { ReactNode } from "react";
import styles from "./list.module.css";

/** A list without bullets of `ListItem`s, separated by lines. */
export function List({ children }: { children: ReactNode }) {
  return <ul className={styles.list}>{children}</ul>;
}

type ListItemProps = {
  children: ReactNode;
  /** Secondary information, shown muted at the end of the row. */
  meta?: ReactNode;
};

export function ListItem({ children, meta }: ListItemProps) {
  return (
    <li className={styles.item}>
      {children}
      {meta && <span className={styles.meta}>{meta}</span>}
    </li>
  );
}
