import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "alpinarc",
  description: "alpinarc",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en">
      <body>
        <header>
          <Link href="/">alpinarc</Link>
          <nav>
            <Link href="/lodges">Lodges</Link>
          </nav>
        </header>
        <main>{children}</main>
      </body>
    </html>
  );
}
