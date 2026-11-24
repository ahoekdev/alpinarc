import { Providers } from "@/components/providers";

export default function LodgesLayout({ children }: LayoutProps<"/lodges">) {
  return <Providers>{children}</Providers>;
}
