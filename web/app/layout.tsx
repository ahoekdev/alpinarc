import type { Metadata } from "next";
import { SiteHeader } from "@/components/site-header/site-header";
import "./globals.css";

export const metadata: Metadata = {
  title: "alpinarc",
  description: "alpinarc",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html lang="en">
      <body>
        <div id="root">
          <SiteHeader />
          <main>{children}</main>
        </div>
      </body>
    </html>
  );
}
