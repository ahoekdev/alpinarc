import styles from "./page.module.css";
import { ApiHealth } from "./api-health";
import { Lodges } from "./lodges";

export default function Home() {
  return (
    <main className={styles.main}>
      <h1>alpinarc</h1>
      <ApiHealth />
      <Lodges />
    </main>
  );
}
