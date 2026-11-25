import type { Metadata } from "next";
import { Container } from "@/components/container";
import { Page } from "@/components/page";
import { LodgeForm } from "./components/lodge-form";
import { LodgeList } from "./components/lodge-list";

export const metadata: Metadata = {
  title: "Lodges | alpinarc",
};

export default function LodgesPage() {
  return (
    <Container>
      <Page title="Lodges">
        <LodgeForm />
        <LodgeList />
      </Page>
    </Container>
  );
}
