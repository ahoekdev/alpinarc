import { Providers } from "@/components/providers/providers";

export default function LodgesLayout({ children }: LayoutProps<"/lodges">) {
  return <Providers>{children}</Providers>;
}
