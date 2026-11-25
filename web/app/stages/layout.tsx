import { Providers } from "@/app/providers";

export default function StagesLayout({ children }: LayoutProps<"/stages">) {
  return <Providers>{children}</Providers>;
}
