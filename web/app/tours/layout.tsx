import { Providers } from "@/app/providers";

export default function ToursLayout({ children }: LayoutProps<"/tours">) {
  return <Providers>{children}</Providers>;
}
