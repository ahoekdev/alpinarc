import { Providers } from "@/app/providers";

export default function LodgesLayout({ children }: LayoutProps<"/lodges">) {
  return <Providers>{children}</Providers>;
}
