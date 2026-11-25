import Link from "next/link";
import styles from "./header.module.css";

export function Header() {
  return (
    <header className={styles.header}>
      <Link href="/">alpinarc</Link>
      <nav>
        <Link href="/lodges">Lodges</Link>
      </nav>
    </header>
  );
}
