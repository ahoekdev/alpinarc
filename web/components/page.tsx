import type { ReactNode } from "react";
import styles from "./page.module.css";

type PageProps = {
  title: ReactNode;
  children: ReactNode;
};

/** A page's heading and content, spaced apart. */
export function Page({ title, children }: PageProps) {
  return (
    <div className={styles.page}>
      <h1>{title}</h1>
      {children}
    </div>
  );
}
