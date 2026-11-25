import type { Metadata } from "next";
import { Page } from "@/components/page";
import { LodgeForm } from "./components/lodge-form";
import { LodgeList } from "./components/lodge-list";

export const metadata: Metadata = {
  title: "Lodges | alpinarc",
};

export default function LodgesPage() {
  return (
    <Page title="Lodges">
      <LodgeForm />
      <LodgeList />
    </Page>
  );
}
