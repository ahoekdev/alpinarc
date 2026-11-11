import styles from "./page.module.css";
import { ApiHealth } from "./api-health";

export default function Home() {
  return (
    <main className={styles.main}>
      <h1>alpinarc</h1>
      <ApiHealth />
    </main>
  );
}
