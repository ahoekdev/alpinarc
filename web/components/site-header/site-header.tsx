import Link from "next/link";
import { Container } from "@/components/ui/container/container";
import styles from "./site-header.module.css";

export function SiteHeader() {
  return (
    <header className={styles.header}>
      <Container>
        <div className={styles.bar}>
          <Link href="/">alpinarc</Link>
          <nav>
            <Link href="/lodges">Lodges</Link>
          </nav>
        </div>
      </Container>
    </header>
  );
}
