import type { ReactNode } from "react";
import styles from "./list.module.css";

type ChildrenProps = { children: ReactNode };

/** A list without bullets, with a line between `ListItem`s. */
export function List({ children }: ChildrenProps) {
  return <ul className={styles.list}>{children}</ul>;
}

/**
 * A row of a `List`. Put a `ListItemContent` first and, optionally, a
 * `ListItemMeta` second: the content starts the row and the meta sits at the
 * end of it.
 *
 * @example
 * <ListItem>
 *   <ListItemContent>{lodge.name}</ListItemContent>
 *   <ListItemMeta>{createdAt}</ListItemMeta>
 * </ListItem>
 */
export function ListItem({ children }: ChildrenProps) {
  return <li className={styles.item}>{children}</li>;
}

/** The main content of a `ListItem`. */
export function ListItemContent({ children }: ChildrenProps) {
  return <div className={styles.content}>{children}</div>;
}

/** Secondary information at the end of a `ListItem`, in muted text. */
export function ListItemMeta({ children }: ChildrenProps) {
  return <div className={styles.meta}>{children}</div>;
}
