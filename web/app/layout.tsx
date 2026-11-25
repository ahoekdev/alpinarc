import type { Metadata } from "next";
import { Header } from "@/components/header";
import "./globals.css";
import styles from "./layout.module.css";

export const metadata: Metadata = {
  title: "alpinarc",
  description: "alpinarc",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en">
      <body>
        <div id="root">
          <Header />
          <main className={styles.main}>{children}</main>
        </div>
      </body>
    </html>
  );
}
