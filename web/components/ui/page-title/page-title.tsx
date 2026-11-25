import type { ReactNode } from "react";

/** The page's main heading. Use it once per page. */
export function PageTitle({ children }: { children: ReactNode }) {
  return <h1>{children}</h1>;
}
