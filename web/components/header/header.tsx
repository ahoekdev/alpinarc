import Link from "next/link";
import { Container } from "@/components/container/container";
import styles from "./header.module.css";

export function Header() {
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
