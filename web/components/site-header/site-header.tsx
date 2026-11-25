import Link from "next/link";
import { Container } from "@/components/ui/container/container";
import styles from "./site-header.module.css";

export function SiteHeader() {
  return (
    <header className={styles.header}>
      <Container>
        <div className={styles.bar}>
          <Link href="/">alpinarc</Link>
          <nav className={styles.nav}>
            <Link href="/lodges">Lodges</Link>
            <Link href="/stages">Stages</Link>
            <Link href="/tours">Tours</Link>
          </nav>
        </div>
      </Container>
    </header>
  );
}
