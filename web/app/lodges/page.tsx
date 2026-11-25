import type { Metadata } from "next";
import { LodgeForm } from "./components/lodge-form";
import { LodgeList } from "./components/lodge-list";
import styles from "./lodges.module.css";

export const metadata: Metadata = {
  title: "Lodges | alpinarc",
};

export default function LodgesPage() {
  return (
    <div className={styles.page}>
      <h1>Lodges</h1>
      <LodgeForm />
      <LodgeList />
    </div>
  );
}
